using System;
using UnityEngine;
using System.Collections.Generic;
using NodeEditor;
using UnityEditor;
using Object = UnityEngine.Object;

namespace NodeEditor
{
    public partial class NodeEditorWindow: EditorWindow
    {
        GenericMenu _newNodeMenu;
        GenericMenu _rightClickMenu;
        Vector2 _scrollSide;
        Vector2 _scrollMain;
        Vector2 _graphViewSize;
        Vector2 _rightClickMenuPosition;
        public NodeData _nodeData;
        public Object _nodeDataContainer;
        Draging _draging = new Draging();
        LinkDraging _linkDraging;
        GraphDraging _graphDraging = new GraphDraging();
        DataSelectionProcessor _selectionProcessor;
        NodeSelection _nodeSelection = new NodeSelection();
        NodeDrawer _nodeDrawer;
        Duplicator _duplicator = new Duplicator();
        List<Node> _removeNodes = new List<Node>();
        List<Node> _nodeList;
        HashSet<Node> _deadEnds;
        float _scale = 1f;
        float _lastScale = 1f;


        [MenuItem("Window/Node Editor")]
        static void Init()
        {
            GetWindow<NodeEditorWindow>("Node Editor").Show();
        }

        public void Initialize(NodeData nodeData, Object nodeDataContainer)
        {
            OnNewDataSelected(nodeData, nodeDataContainer);

            _newNodeMenu = new GenericMenu();
            _rightClickMenu = new GenericMenu();

            foreach (var pair in NodeCollector.Nodes)
            {
                var info = pair.Value;
                if (string.IsNullOrEmpty(info.Path)) continue;
                _newNodeMenu.AddItem(new GUIContent(info.Path), false, data =>
                {
                    CreateNode((string)data, _graphViewSize * 0.5f - _scrollMain);
                }, info.TypeID);

                _rightClickMenu.AddItem(new GUIContent("Create/" + info.Path), false, data =>
                {
                    CreateNode((string)data, _rightClickMenuPosition - _scrollMain);
                }, info.TypeID);
            }
            Show();
            DefaultNode.InitDefaults(nodeData);
        }

        public void OnNewDataSelected(NodeData nodeData, Object nodeDataContainer)
        {
            _nodeData = nodeData;
            _nodeDataContainer = nodeDataContainer;
            if (_nodeData == null) return;
            _nodeData.Initialize();
            RebuildGraph();
            Repaint();
            _nodeDrawer.SetNodeData(nodeData, nodeDataContainer);
            _duplicator.Initialize(nodeData);
        }

        void OnEnable()
        {
            EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;
            Selection.selectionChanged += OnHierarchyWindowChanged;

            _linkDraging = new LinkDraging(this);
            _selectionProcessor = new DataSelectionProcessor(OnNewDataSelected);

            _nodeDrawer = new NodeDrawer(this, _nodeData, _nodeDataContainer, _linkDraging, _draging);
            _nodeDrawer.OnRebuildGraph += RebuildGraph;

            _duplicator.Initialize(_nodeData);

            _selectionProcessor.Reset();
            if (_nodeDataContainer != null)
                _selectionProcessor.Select(_nodeDataContainer);

            Undo.undoRedoPerformed += OnUndo;
        }

        void OnDisable()
        {
            EditorApplication.hierarchyWindowChanged = null;

            _selectionProcessor.Reset();
            Undo.undoRedoPerformed = null;
        }
        void OnHierarchyWindowChanged()
        {
            _selectionProcessor.Reset();
            Repaint();
        }

        void OnUndo()
        {
            Initialize(_nodeData, _nodeDataContainer);
            RebuildGraph();
            Repaint();
        }



        void OnGUI()
        {
            _selectionProcessor.Process();
            if (!_selectionProcessor.Host) _nodeData = null;

            Styles.Initialize();
            if (_nodeData == null)
            {
                DrawBigMessage("No Selected Data");
                return;
            }
            if (_newNodeMenu == null) Initialize(_nodeData, _nodeDataContainer);

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Create Node", EditorStyles.toolbarDropDown))
                _newNodeMenu.DropDown(new Rect(1, EditorStyles.toolbar.fixedHeight - 2, 1, 1));

            _selectionProcessor.DropDown();

            if (Event.current.type == EventType.ScrollWheel)
                _scale -= Event.current.delta.y * 0.03f;
            _scale = EditorGUILayout.Slider(new GUIContent("Scale"), _scale, 0.1f, 2f, GUILayout.ExpandWidth(false));
            if (Mathf.Abs(_scale - 1f) < 0.05f) _scale = 1f;

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            if (Event.current.type == EventType.Repaint)
            {
                _graphViewSize = position.size;
                _graphViewSize.y -= GUILayoutUtility.GetLastRect().height;
            }


            EditorGUILayout.BeginHorizontal();
            DrawMainViewGUI(_scale);
            EditorGUILayout.EndHorizontal();



            if (_removeNodes.Count > 0)
            {
                foreach (Node node in _removeNodes)
                    _nodeData.RemoveNode(node);
                _removeNodes.Clear();
                Repaint();
            }


        }


        void DrawMainViewGUI(float scale)
        {
            Rect rect = GUILayoutUtility.GetRect(10, 8192, 10, 8192, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            rect.y += 20;
            rect.y /= scale;


            Matrix4x4 mm = GUI.matrix;
            {  
                GUI.EndGroup();
                GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), Vector2.zero);
                
                if (Math.Abs(_lastScale - scale) > 0.001f)
                {
                    _scrollMain -= (rect.size * 0.5f / _lastScale) * (1f - _lastScale);
                    _scrollMain += (rect.size * 0.5f / scale) * (1f - scale);
                    _lastScale = scale;
                }
                rect.size /= scale;
                GUI.BeginClip(rect, _scrollMain, Vector2.zero, false);
            }
            {//BackGround
                float cellSize = Styles.CellSize;
                if (scale < 0.75) cellSize *= 2;
                if (scale < 0.375) cellSize *= 2;
                if (scale < 0.187) cellSize *= 2;
                Rect bgRect = new Rect(0, 0, 128000, 128000);
                Rect bgCoords = bgRect;
                Vector2 shift = (Vector2)Vector2Int.FloorToInt(_scrollMain / cellSize) * cellSize;
                bgRect.position = -bgRect.size * 0.5f - shift;
                bgCoords.size /= cellSize;
                GUI.color = Styles.BackgroundColor;
                GUI.DrawTextureWithTexCoords(bgRect, Styles.BackgroundGrid, bgCoords);
                GUI.color = Color.white;
            }

            DrawLinks();

            DrawNodes(scale);

            _nodeSelection.Process(_nodeList, scale);

            if (Event.current.type == EventType.MouseDown)
            {
                EditorGUI.FocusTextInControl("");
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
            {
                Vector2 pos = new Vector2();
                foreach (Node node in _nodeList)
                    pos += node.Position + node.SizeInCells * Styles.CellSize * 0.5f;
                pos /= _nodeList.Count;
                _scrollMain = _graphViewSize * 0.5f - pos;
            }


            _scrollMain = _graphDraging.Drag(_scrollMain, scale);
            _scrollMain = new Vector2((int)_scrollMain.x, (int)_scrollMain.y);


            {
                GUI.matrix = mm;
                GUI.EndClip();
                GUI.BeginGroup(rect);
            }
            if (Event.current.type == EventType.ContextClick)
            {
                _rightClickMenuPosition = Event.current.mousePosition / scale; 
                _rightClickMenu.ShowAsContext();
                Event.current.Use();
            }

        }

        static Vector2 GetNodeInputPosition(Node node, int input)
        {
            int properties = 0;
            if (node.DrawProperties != null) properties = node.DrawProperties.Length;
            return node.Position + new Vector2(Styles.InputLinkShift, (properties + input + 1) * Styles.CellSize);
        }

        static Vector2 GetNodeOutputPosition(Node node)
        {
            float nodeWidth = Styles.CellSize * node.NodeWidth;
            float nodeHeight = Styles.CellSize * node.NodeHeight;

            return node.Position + new Vector2(nodeWidth + Styles.OutputLinkShift, nodeHeight * 0.5f);
        }

        public static Color GetLinkColor(Node input, Node output)
        {
            ValueType type = input.CashedOutputType;
            if (output.CashedOutputType == ValueType.Error) type = ValueType.Error;
            Color color = Styles.GetColor(type);
            color.a = 1;
            return color;
        }

        void DrawLinks()
        {
            if (Event.current.type == EventType.Repaint)
                foreach (Node node in _nodeList)
                {
                    if (node == null) continue;
                    for (int l = 0; l < node.Inputs.Length; l++)
                    {
                        Link link = node.Inputs[l];
                        Node inNode = _nodeData.GetNode(link);

                        if (inNode != null && !(inNode is DefaultNode))
                        {
                            Vector2 p1 = GetNodeInputPosition(node, l);
                            Vector2 p0 = GetNodeOutputPosition(inNode);

                            DrawCurve(p0, p1, GetLinkColor(inNode, node));
                        }
                    }
                }
            _linkDraging.DrawLink();
        }

        void DrawNodes(float scale)
        {
            //if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp) Repaint();
            if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout) Repaint();

            foreach (Node node in _nodeList)
            {
                if (node == null) continue;

                _nodeDrawer.DrawNode(node, _nodeSelection.Selection, scale);
            }

            if (Event.current.type == EventType.ValidateCommand && Event.current.commandName == "Duplicate")
            {
                DuplicateSelected();
            }
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete && GUIUtility.keyboardControl == 0)
            {
                RemoveSelected();
            }
            _linkDraging.AfterDrawNodes();
        }

        static void DrawCurve(Vector2 p0, Vector2 p1, Color color)
        {
            Vector2 dir = p1 - p0;
            Vector2 normal = new Vector2(30 + dir.magnitude * 0.1f, 0);
            /*
        Vector2 normal = new Vector2(Mathf.Sqrt(6f + Mathf.Max(-dir.x, 16f)) * 8f, 0f);
        if (dir.x < 0)
        {
            float dy = Mathf.Max(Mathf.Abs(dir.y), 64f) * Mathf.Sign(dir.y);
            normal.y += Mathf.Clamp((-dir.x / dy) * 10f, -80f, 80f);
            //normal.y += Mathf.Clamp(dir.y * 0.3f * Mathf.Clamp01(-dir.x * 0.02f), -80f, 80f);
        }
        //*/


            Vector2 n0 = p0 + normal;
            Vector2 n1 = p1 - normal;
            //float dist = HandleUtility.DistancePointBezier(Event.current.mousePosition, p0, p1, n0, n1);
            Handles.DrawBezier(p0, p1, n0, n1, color, null, 3);
        }

        void DrawBigMessage(string message)
        {
            GUILayout.Label(message, Styles.BigMessageLabel, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        }

        void RebuildGraph()
        {
            _nodeList = _nodeData.GetAllNodes();
            _deadEnds = _nodeData.GetDeadEnds(_nodeList);

            foreach (Node node in _nodeList)
                node.Processor = null;

            List<int> outputs = new List<int>();
            foreach (Node node in _deadEnds)
            {
                node.UpdateTypes(_nodeData, new HashSet<Node>());

                NodeProcessor pr = _nodeData.GetNodeProcessor(node);

                if (!_nodeData.ValidateNodes(node)) continue;
                if (pr != null && pr.VoidOut != null && node.OutputType != ValueType.Error)
                    outputs.Add(node.NodeId);
            }
            _nodeData.Outputs = outputs.ToArray();
            _nodeData.Prepare();

            if (_nodeData.Externals == null) _nodeData.Externals = new NodeDataExternals();
            _nodeData.Externals.InitializeFrom(_nodeData);

            if (_selectionProcessor.Host != null)
            {
                EditorUtility.SetDirty(_selectionProcessor.Host);
                EditorHelper.CallOnValidate(_selectionProcessor.Host);
                EditorHelper.RepaintInspectors();
            }
        }


        public Node CreateNode(string typeID, Vector2 pos)
        {
            Undo.RecordObject(_nodeDataContainer, "Create Node");
            Node node = _nodeData.CreateNode(typeID);
            node.Position = pos;
            DefaultNode.InitDefaults(node, _nodeData);
            RebuildGraph();
            return node; 
        }

        public void RemoveNode(Node node)
        {
            if (!node.Controllable) return;
            Undo.RecordObject(_nodeDataContainer, "Remove Node");
            _nodeData.RemoveNode(node);
            RebuildGraph();
        }

        public void DuplicateSelected()
        {
            Undo.RecordObject(_nodeDataContainer, "Duplicate");
            _duplicator.Duplicate(_nodeSelection.Selection);
            RebuildGraph();
        }

        public void RemoveSelected()
        {
            Undo.RecordObject(_nodeDataContainer, "Remove Nodes");
            foreach (Node node in _nodeSelection.Selection)
                if (node.Controllable)
                    _nodeData.RemoveNode(node);
            RebuildGraph();
        }

    }
}