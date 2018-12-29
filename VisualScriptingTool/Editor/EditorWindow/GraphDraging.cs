using UnityEngine;

namespace NodeEditor
{
    class GraphDraging
    {
        Vector2 _delta;
        bool _draging;

        public Vector2 Drag(Vector2 position, float scale)
        {
            Event currentEvent = Event.current;
            EventType eventType = currentEvent.type;
            Vector2 mousePosition = currentEvent.mousePosition;
            Vector2 mouseScreenPosition = GUIUtility.GUIToScreenPoint(mousePosition / scale);

            if (eventType == EventType.MouseDown && currentEvent.button == 2)
            {
                _delta = position - mouseScreenPosition;
                _draging = true;
                currentEvent.Use();
                return mouseScreenPosition + _delta;
            }
            if (_draging)
            {
                if (eventType == EventType.MouseUp && currentEvent.button == 2 || eventType == EventType.MouseLeaveWindow)
                {
                    _draging = false;
                    currentEvent.Use();
                }
                return mouseScreenPosition + _delta;
            }
            return position;
        }
    }
}