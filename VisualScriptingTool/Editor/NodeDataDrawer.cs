using UnityEditor;
using UnityEngine;

namespace NodeEditor
{
    [CustomPropertyDrawer(typeof (NodeData))]
    public class NodeDataDrawer: PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);
            if (GUI.Button(position, "Open Node Editor"))
            {
                NodeEditorWindow mw = EditorWindow.GetWindow<NodeEditorWindow>("Node Editor");
                mw.Initialize((NodeData)GetObject(property), property.serializedObject.targetObject);
            }
        }
        static object GetObject(SerializedProperty property)
        {
            object obj = property.serializedObject.targetObject;
            return obj.GetType().GetField(property.name).GetValue(obj);
        }

    }
}