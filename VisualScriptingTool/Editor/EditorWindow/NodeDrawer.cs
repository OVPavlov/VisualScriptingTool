using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NodeEditor
{
    class NodeDrawer
    {
        public event Action OnRebuildGraph;
        NodeEditorWindow _nodeEditorWindow;
        NodeData _nodeData;
        UnityEngine.Object _nodeDataContainer;
        readonly NodeEditorWindow.LinkDraging _linkDraging;
        readonly Draging _draging;

        public NodeDrawer(NodeEditorWindow nodeEditorWindow, NodeData nodeData, UnityEngine.Object nodeDataContainer, NodeEditorWindow.LinkDraging linkDraging, Draging draging)
        {
            _nodeEditorWindow = nodeEditorWindow;
            _nodeData = nodeData;
            _nodeDataContainer = nodeDataContainer;
            _linkDraging = linkDraging;
            _draging = draging;
        }

        public void SetNodeData(NodeData nodeData, UnityEngine.Object nodeDataContainer)
        {
            _nodeData = nodeData;
            _nodeDataContainer = nodeDataContainer;
        }

        void RebuildGraph()
        {
            if (OnRebuildGraph != null)
                OnRebuildGraph();
        }



        public void DrawNode(Node node, HashSet<Node> selection, float scale)
        {
            if (node is DefaultNode) return;
            float CellSize = Styles.CellSize;
            bool selected = selection.Contains(node);

            EditorGUIUtility.labelWidth = 50;
            float nodeWidth = CellSize * node.NodeWidth;
            float nodeHeight = CellSize * node.NodeHeight;
            const float widthPadding = 2;


            Rect rect = new Rect(node.Position, new Vector2(nodeWidth, nodeHeight));
            rect.yMin -= CellSize;

            DrawBodyAndLable(node, rect, selected);

            float x = node.Position.x + widthPadding;
            float y = node.Position.y + CellSize / 2;


            DrawPropertyes(node, widthPadding, nodeWidth, x, ref y);

            DrawInputs(node, x, y, nodeWidth, nodeHeight);

            DrawOutput(node, nodeWidth, nodeHeight);

            ProcessEvents(node, rect, selected, selection, scale);
        }

        void ProcessEvents(Node node, Rect rect, bool selected, HashSet<Node> selection, float scale)
        {
            Event currentEvent = Event.current;
            EventType eventType = currentEvent.type;

            if ((eventType == EventType.MouseUp && currentEvent.button == 0 || eventType == EventType.Used) && rect.Contains(currentEvent.mousePosition))
            {
                if (!selected)
                    EditorGUI.FocusTextInControl("");
                if (currentEvent.control)
                {
                    if (selected)
                        selection.Remove(node);
                    else
                        selection.Add(node);
                }
                else
                {
                    selection.Clear();
                    selection.Add(node);
                }
            }

            if (Event.current.type == EventType.ContextClick && rect.Contains(currentEvent.mousePosition))
            {
                GenericMenu nodeMenu = new GenericMenu();
                string s = node.GetType().Name + "s[" + node.NodeId + "]";
                nodeMenu.AddItem(new GUIContent("To Clipboard: " + s), false, (st) => { EditorGUIUtility.systemCopyBuffer = (string)st; }, s);
                if (node.Controllable)
                {
                    nodeMenu.AddItem(new GUIContent("Remove"), false, (n) =>
                    {
                        _nodeEditorWindow.RemoveSelected();
                    }, node);
                    nodeMenu.AddItem(new GUIContent("Duplicate"), false, (sel) =>
                    {
                        _nodeEditorWindow.DuplicateSelected();
                    }, selection);
                    ExternalValueNode external = node as ExternalValueNode;
                    if (external != null)
                    {
                        nodeMenu.AddItem(new GUIContent(external.UseAsExternal ? "Close" : "Open"), false, n =>
                        {
                            Undo.RecordObject(_nodeDataContainer, "Change External Settings");
                            ExternalValueNode ext = (ExternalValueNode)n;
                            ext.UseAsExternal = !ext.UseAsExternal;
                            _nodeData.Externals.InitializeFrom(_nodeData);
                        }, external);
                    }
                }
                Matrix4x4 m = GUI.matrix;
                GUI.matrix *= Matrix4x4.Scale(new Vector3(1f / scale, 1f / scale, 1));
                nodeMenu.ShowAsContext();
                GUI.matrix = m;
                Event.current.Use();
            }
            bool startDraging;
            Vector2 newPos = _draging.Drag(rect, node.Position, node, out startDraging, scale);
            Styles.SnapPosition(ref newPos);
            if (startDraging)
            {
                Undo.RecordObject(_nodeDataContainer, "Draging");
                if (!selection.Contains(node))
                {
                    selection.Clear();
                    selection.Add(node);
                }
            }
            if (node.Position != newPos)
            {
                Vector2 delta = newPos - node.Position;
                foreach (Node n in selection)
                    n.Position += delta;
                node.Position = newPos;
            }
        }

        void DrawBodyAndLable(Node node, Rect rect, bool selected)
        {
            Color typeColor = Styles.GetColor(node.OutputType);
            Color nodeColor = Styles.NodeColor;
            nodeColor = Color.Lerp(typeColor, nodeColor, nodeColor.a / typeColor.a);
            nodeColor.a = 1;
            GUI.color = nodeColor;
            GUI.Box(rect, "", selected ? Styles.NodeActive : Styles.Node);
            GUI.color = Color.white;

            Rect labelRect = rect;
            labelRect.height = Styles.CellSize;
            GUI.color = Styles.Gray(1, 0.35f);

            ExternalValueNode external = node as ExternalValueNode;
            if (external != null && external.UseAsExternal)
            {
                GUI.Label(new Rect(labelRect.x + 1, labelRect.y + 1, labelRect.width, labelRect.height), external.ValueName, Styles.Title);
                GUI.color = Styles.Gray(0, 0.8f);
                EditorGUI.BeginChangeCheck();
                string newName = EditorGUI.TextField(labelRect, external.ValueName, Styles.Title);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_nodeDataContainer, "Change Node Name");
                    external.ValueName = newName;
                    RebuildGraph();
                }
            }
            else
            {
                GUI.Label(new Rect(labelRect.x + 1, labelRect.y + 1, labelRect.width, labelRect.height), node.Name, Styles.Title);
                GUI.color = Styles.Gray(0, 0.8f);
                GUI.Label(labelRect, node.Name, Styles.Title);
            }
            if (external != null && node.Controllable)
            {
                GUI.color = new Color(0.0f, 0.0f, 0.0f, 0.4f);
                GUI.Label(labelRect, new GUIContent(Styles.ExternalIndicatorImageShadow), Styles.ExternalIndicator);
                GUI.color = external.UseAsExternal ? new Color(0.5f, 1f, 0.5f, 1f) : new Color(1f, 1f, 1f, 0.3f);
                GUI.Label(labelRect, new GUIContent(Styles.ExternalIndicatorImage), Styles.ExternalIndicator);
            }

            GUI.color = Color.white;
        }

        void DrawPropertyes(Node node, float widthPadding, float nodeWidth, float x, ref float y)
        {
            if (node.DrawProperties != null && node.DrawProperties.Length > 0)
            {
                ExternalValueNode external = node as ExternalValueNode;
                bool turnOff = !node.Controllable || (external != null && external.UseAsExternal);
                if (turnOff) GUI.enabled = false;
                Rect propertyRect = new Rect(x, y, nodeWidth - widthPadding * 3, Styles.CellSize);
                for (int i = 0; i < node.DrawProperties.Length; i++)
                {
                    DrawProperty(propertyRect, node, node.DrawProperties[i], node.DrawProperties[i]);
                    propertyRect.y = y += propertyRect.height;
                }
                if (turnOff) GUI.enabled = true;
            }
        }

        void DrawProperty(Rect rect, Node node, string property, string name)
        {
            FieldInfo field = node.GetType().GetField(property);
            if (field == null) return;
            object value = field.GetValue(node);

            EditorGUI.BeginChangeCheck();
            value = DrawProperty(rect, field.FieldType, name, value);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_nodeDataContainer, "Change Property");
                field.SetValue(node, value);

                node.UpdateTypes(_nodeData, new HashSet<Node>());
                DefaultNode.InitDefaults(node, _nodeData);

                RebuildGraph();
            }
        }

        public static object DrawProperty(Rect rect, Type fieldType, string name, object value)
        {
            if (fieldType == typeof (Transform))
                return EditorGUI.ObjectField(rect, name, (Transform)value, typeof (Transform), true);
            if (fieldType == typeof (float))
                return EditorGUI.FloatField(rect, name, (float)value);
            if (fieldType == typeof (Vector2))
                return EditorGUI.Vector2Field(rect, "", (Vector2)value);
            if (fieldType == typeof (Vector3))
                return EditorGUI.Vector3Field(rect, "", (Vector3)value);
            if (fieldType == typeof (Vector4))
                return EditorGUI.Vector4Field(rect, "", (Vector4)value);
            if (fieldType == typeof (Color))
            {
                Color val = EditorGUI.ColorField(rect, GUIContent.none, (Color)value, false, true, true, new ColorPickerHDRConfig(-64, 64, 0, 1));
                return val;
            }
            if (fieldType == typeof (int))
                return EditorGUI.IntField(rect, name, (int)value);
            if (fieldType == typeof (bool))
                return EditorGUI.Toggle(rect, name, (bool)value);
            if (fieldType == typeof (AnimationCurve))
                return EditorGUI.CurveField(rect, "", (AnimationCurve)value);
            if (fieldType == typeof (string))
                return EditorGUI.TextField(rect, "", (string)value);
            if (fieldType == typeof (Component))
                return EditorGUI.ObjectField(rect, "", (Component)value, typeof (Component), true);
            if (fieldType.BaseType == typeof (Enum))
                return EditorGUI.EnumPopup(rect, "", (Enum)value);
            if (fieldType == typeof (Gradient))
                return EditorHelper.GradientField(rect, (Gradient)value);

            GUI.Label(rect, name + "   " + fieldType);
            return value;
        }

        void DrawInputs(Node node, float x, float y, float nodeWidth, float nodeHeight)
        {
            float CellSize = Styles.CellSize;

            Rect inputRect = new Rect(x, y, nodeWidth, CellSize);
            Rect graphicRect = new Rect(node.Position.x + Styles.InputShift.x, y + Styles.InputShift.y, Styles.IOSize, Styles.IOSize);
            float interactionOffset = Mathf.Max(graphicRect.width, CellSize * 2);
            Rect interactionRect = new Rect(node.Position.x - interactionOffset, y, inputRect.width + interactionOffset, CellSize);
            Rect smallInputRect = new Rect(interactionRect.x, interactionRect.y, interactionOffset, CellSize);
            for (int i = 0; i < node.Inputs.Length; i++)
            {
                Link link = node.Inputs[i];
                Node inNode = _nodeData.GetNode(link);

                GUI.Label(inputRect, link.Name);

                if (inNode is DefaultNode)
                {
                    Rect defaultRect = inputRect;
                    defaultRect.width = inNode.NodeWidth * CellSize;
                    defaultRect.x -= defaultRect.width;
                    GUI.color = Styles.GetColor(inNode.OutputType);
                    Rect defaultBackRect = defaultRect;
                    defaultBackRect.height += 1;
                    GUI.Box(defaultBackRect, "", Styles.DefaultValue);
                    GUI.color = Color.white;
                    float savedLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 8;
                    string name = (inNode.OutputType == ValueType.Float || inNode.OutputType == ValueType.Int) ? " " : "";
                    DrawProperty(defaultRect, inNode, "Value", name);
                    EditorGUIUtility.labelWidth = savedLabelWidth;
                    defaultRect.xMax += inputRect.width;
                    _linkDraging.ProcessNodeInput(node, i, defaultRect, smallInputRect);
                }
                else
                {
                    GUIStyle gs = inNode != null ? Styles.GraphicPointActive : Styles.GraphicPoint;

                    GUI.color = inNode != null ? NodeEditorWindow.GetLinkColor(inNode, node) : Styles.GetColor(link.Type);
                    GUI.Box(graphicRect, "", gs);
                    GUI.color = Color.white;
                    _linkDraging.ProcessNodeInput(node, i, interactionRect, smallInputRect);
                }
                //_linkDraging.ProcessNodeInput(node, i, interactionRect, smallInputRect);
                inputRect.y = y += inputRect.height;
                graphicRect.y = inputRect.y + Styles.InputShift.y;
                interactionRect.y = inputRect.y;
                smallInputRect.y = inputRect.y;
            }
        }


        void DrawOutput(Node node, float nodeWidth, float nodeHeight)
        {
            if (node.OutputType != ValueType.None)
            {
                Vector2 position = node.Position + new Vector2(nodeWidth, nodeHeight * 0.5f);
                Rect outputRect = new Rect(position + Styles.OutputShift, new Vector2(Styles.IOSize, Styles.IOSize));
                Color color = Styles.GetColor(node.CashedOutputType);
                color.a = 1;
                GUI.color = color;
                GUI.Box(outputRect, "", Styles.GraphicPointActive);
                GUI.color = Color.white;
                _linkDraging.ProcessNodeOutput(node, new Rect(position.x, position.y - 16, 32, 32));
            }
        }


    }
}