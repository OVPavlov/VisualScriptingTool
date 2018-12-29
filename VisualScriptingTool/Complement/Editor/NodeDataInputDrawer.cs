using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NodeEditor
{
    [CustomPropertyDrawer(typeof(NodeDataInput))]
    public class NodeDataInputDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            NodeDataInput input = (NodeDataInput)EditorHelper.GetPropertyObject(property);
            if (input == null || input.Externals == null || input.Externals.BoolNodes == null)
            {
                GUI.Label(position, label.text + " isn't initialized");
                return;
            }
            NodeDataExternals externals = input.Externals;
            input.Initialize();

            position.height = 16f;

            for (int i = 0; i < externals.BoolNodes.Count; i++)
            {
                input.Bools[i] = EditorGUI.Toggle(position, externals.BoolNodes[i].Name, input.Bools[i]);
                position.y += position.height;
            }
            for (int i = 0; i < externals.ColorNodes.Count; i++)
            {
                input.Colors[i] = EditorGUI.ColorField(position, new GUIContent(externals.ColorNodes[i].Name), input.Colors[i], true, true, true, new ColorPickerHDRConfig(-64, 64, -64, 64));
                position.y += position.height;
            }
            for (int i = 0; i < externals.FloatNodes.Count; i++)
            {
                input.Floats[i] = EditorGUI.FloatField(position, externals.FloatNodes[i].Name, input.Floats[i]);
                position.y += position.height;
            }
            for (int i = 0; i < externals.IntNodes.Count; i++)
            {
                input.Ints[i] = EditorGUI.IntField(position, externals.IntNodes[i].Name, input.Ints[i]);
                position.y += position.height;
            }
            for (int i = 0; i < externals.Vector2Nodes.Count; i++)
            {
                input.Vector2s[i] = EditorGUI.Vector2Field(position, externals.Vector2Nodes[i].Name, input.Vector2s[i]);
                position.y += position.height;
            }
            for (int i = 0; i < externals.Vector3Nodes.Count; i++)
            {
                input.Vector3s[i] = EditorGUI.Vector3Field(position, externals.Vector3Nodes[i].Name, input.Vector3s[i]);
                position.y += position.height;
            }
            for (int i = 0; i < externals.Vector4Nodes.Count; i++)
            {
                input.Vector4s[i] = EditorGUI.Vector4Field(position, externals.Vector4Nodes[i].Name, input.Vector4s[i]);
                position.y += position.height;
            }
            for (int i = 0; i < externals.AnimationCurveNodes.Count; i++)
            {
                input.AnimationCurves[i] = EditorGUI.CurveField(position, externals.AnimationCurveNodes[i].Name, input.AnimationCurves[i]);
                position.y += position.height;
            }
            for (int i = 0; i < externals.GradientNodes.Count; i++)
            {
                input.Gradients[i] = EditorHelper.GradientField(position, externals.GradientNodes[i].Name, input.Gradients[i]);
                position.y += position.height;
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(property.serializedObject.targetObject);
                EditorHelper.CallOnValidate(property.serializedObject.targetObject);
                RepaintNodeEditorWindows();
            }
            
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            NodeDataInput input = (NodeDataInput)EditorHelper.GetPropertyObject(property);
            if (input == null) return 0;
            NodeDataExternals externals = input.Externals;
            //Debug.Log("GetPropertyHeight " + input + "  " + input.Externals);
            int count = GetFieldsCount(externals);
            return count * 16;
        }

        public static void RepaintNodeEditorWindows()
        {
            EditorWindow[] windows = (EditorWindow[])Resources.FindObjectsOfTypeAll(typeof(NodeEditorWindow));
            foreach (EditorWindow window in windows)
                window.Repaint();
        }


        static int GetFieldsCount(NodeDataExternals externals)
        {
            //Debug.Log("GetFieldsCount:"+externals);
            if (externals == null || externals.FloatNodes == null) return 1;
            int count = 0;
            count += externals.BoolNodes.Count;
            count += externals.ColorNodes.Count;
            count += externals.FloatNodes.Count;
            count += externals.IntNodes.Count;
            count += externals.Vector2Nodes.Count;
            count += externals.Vector3Nodes.Count;
            count += externals.Vector4Nodes.Count;
            count += externals.AnimationCurveNodes.Count;
            count += externals.GradientNodes.Count;
            return count;
        }
 }
}