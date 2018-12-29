using UnityEngine;

namespace NodeEditor
{
    class Draging
    {
        public Node Node;
        public Vector2 Delta;

        Node _goingToDragNode;
        Vector2 _startPosition;
        float _threshold = 8f;


        public Vector2 Drag(Rect rect, Vector2 position, Node node, out bool startDraging, float scale)
        {
            Event currentEvent = Event.current;
            EventType eventType = currentEvent.type;
            startDraging = false;
            Vector2 mousePosition = currentEvent.mousePosition;
            Vector2 mouseScreenPosition = GUIUtility.GUIToScreenPoint(mousePosition / scale);
            if (_goingToDragNode == null && Node == null && eventType == EventType.MouseDown && currentEvent.button == 0 && rect.Contains(mousePosition))
            {
                Delta = position - mouseScreenPosition;
                _startPosition = mouseScreenPosition;
                _goingToDragNode = node;
                currentEvent.Use();
            }
            if (_goingToDragNode == node)
            {
                if ((mouseScreenPosition - _startPosition).magnitude > _threshold)
                {
                    Node = node;
                    _goingToDragNode = null;
                    startDraging = true;
                }
                if (eventType == EventType.MouseUp && currentEvent.button == 0 || eventType == EventType.MouseLeaveWindow)
                {
                    _goingToDragNode = null;
                }
            }
            if (Node == node)
            {
                if (eventType == EventType.MouseUp && currentEvent.button == 0 || eventType == EventType.MouseLeaveWindow)
                {
                    Node = null;
                    currentEvent.Use();
                }
                return mouseScreenPosition + Delta;
            }
            return position;
        }
    }
}