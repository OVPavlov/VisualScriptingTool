using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor
{
    class NodeSelection
    {
        Vector2 _startPos;
        bool _draging;
        bool _phase2;
        public readonly HashSet<Node> Selection = new HashSet<Node>();

        public void Process(List<Node> nodeList, float scale)
        {
            Event currentEvent = Event.current;
            EventType eventType = currentEvent.type;
            Vector2 mousePosition = currentEvent.mousePosition;
            Vector2 mouseScreenPosition = GUIUtility.GUIToScreenPoint(mousePosition) * scale;
            if (!_draging)
            {
                if (eventType == EventType.MouseDown && currentEvent.button == 0)
                {
                    _startPos = mouseScreenPosition;
                    _draging = true;
                    _phase2 = false;
                    //currentEvent.Use();
                }
            }
            else
            {
                if (!_phase2)
                {
                    if (eventType == EventType.MouseUp && currentEvent.button == 0 || eventType == EventType.MouseLeaveWindow)
                    {
                        _draging = false;
                        _phase2 = false;
                        Selection.Clear();
                        currentEvent.Use();
                    }
                    if ((_startPos - mouseScreenPosition).magnitude > 5)
                        _phase2 = true;
                }
                else
                {
                    Rect rect = new Rect();
                    Vector2 start = GUIUtility.ScreenToGUIPoint(_startPos) / scale;
                    Vector2 end = GUIUtility.ScreenToGUIPoint(mouseScreenPosition) / scale;
                    rect.min = Vector2.Min(start, end);
                    rect.max = Vector2.Max(start, end);

                    GUI.Box(rect, "", new GUIStyle("SelectionRect"));


                    if (eventType == EventType.MouseUp && currentEvent.button == 0 || eventType == EventType.MouseLeaveWindow)
                    {
                        _draging = false;
                        _phase2 = false;
                        if (!currentEvent.control)
                            Selection.Clear();
                        foreach (Node node in nodeList)
                        {
                            Rect nRect = new Rect(node.Position, new Vector2(Styles.CellSize * node.NodeWidth, Styles.CellSize * node.NodeHeight));
                            if (rect.Overlaps(nRect))
                                Selection.Add(node);
                        }

                        currentEvent.Use();
                    }
                }
            }
        }
    }
}