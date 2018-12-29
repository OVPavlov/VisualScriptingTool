using UnityEditor;
using UnityEngine;

namespace NodeEditor
{
    partial class NodeEditorWindow
    {
        public class LinkDraging
        {
            public NodeEditorWindow Window;
            Node _node;
            Vector2 _inputPosition;
            bool _hoverInput;

            public LinkDraging(NodeEditorWindow window)
            {
                Window = window;
            }

            public void DrawLink()
            {
                if (_node == null) return;
                if (Event.current.type != EventType.Repaint) return;
                Vector2 linkPosition = _hoverInput ? _inputPosition : Event.current.mousePosition;
                _hoverInput = false;
                DrawCurve(GetNodeOutputPosition(_node), linkPosition, new Color(1, 1, 1, 0.5f));
                Window.Repaint();
            }

            public void AfterDrawNodes()
            {
                if (_node == null) return;
                if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseLeaveWindow)
                {
                    _node = null;
                    Window.RebuildGraph();
                    Event.current.Use();
                }
            }

            public void ProcessNodeInput(Node node, int inputId, Rect bigRect, Rect smallRect)
            {
                Event currentEvent = Event.current;
                EventType eventType = currentEvent.type;
                if (_node != null)//draging
                {
                    if (!bigRect.Contains(currentEvent.mousePosition)) return;
                    Link link = node.Inputs[inputId];


                    bool validate = NodeCollector.ValidateTypes(link.Type, _node.CashedOutputType) || NodeCollector.ValidateTypes(_node.CashedOutputType, link.Type);
                    if (eventType == EventType.Repaint && validate)
                    {
                        _hoverInput = true;
                        _inputPosition = GetNodeInputPosition(node, inputId);
                    }

                    if (eventType == EventType.MouseUp && currentEvent.button == 0)
                    {
                        Undo.RecordObject(Window._nodeDataContainer, "Change Link");
                        if (validate)
                        {
                            link.NodeId = _node.NodeId;
                        }
                    }
                }
                else//
                {
                    if (eventType == EventType.MouseDown && currentEvent.button == 0 && smallRect.Contains(currentEvent.mousePosition))
                    {
                        Link link = node.Inputs[inputId];
                        Node inNode = Window._nodeData.GetNode(link);
                        if (inNode != null)
                        {
                            Undo.RecordObject(Window._nodeDataContainer, "Change Link");
                            _node = inNode;
                            link.NodeId = Node.NoNodeID;

                            DefaultNode.ToDefault(link, Window._nodeData);

                            _hoverInput = false;
                            Window.RebuildGraph();
                            currentEvent.Use();
                        }
                    }
                }
            }

            public void ProcessNodeOutput(Node node, Rect outputRect)
            {
                if (_node == null && Event.current.type == EventType.MouseDown)//draging
                {
                    if (outputRect.Contains(Event.current.mousePosition))
                    {
                        _node = node;
                        Event.current.Use();
                    }
                }
            }

        }
    }
}