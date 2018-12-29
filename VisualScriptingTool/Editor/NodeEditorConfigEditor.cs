using System;
using UnityEditor;
using UnityEngine;

namespace NodeEditor
{
    [CustomEditor(typeof (NodeEditorConfig))]
    public class NodeEditorConfigEditor: Editor
    {
        NodeEditorConfig _config;
        static bool _colorsFoldout = true;

        void OnEnable()
        {
            _config = (NodeEditorConfig)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IsDefaut"));
            GUILayout.Space(16);
            _colorsFoldout = EditorGUILayout.Foldout(_colorsFoldout, "Colors");
            if (_colorsFoldout)
            {
                EditorGUI.indentLevel++;
                DrawColors(ref _config.Colors, typeof (ValueType));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("BackgroundColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("NodeColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("InputsOutputs"), true);


            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                Styles.SetDirty();
            }
        }

        static void DrawColors(ref Color[] colors, Type enumType)
        {
            string[] colorNames = Enum.GetNames(enumType);

            if (colors == null) colors = new Color[colorNames.Length];
            if (colors.Length < colorNames.Length)
            {
                Color[] newColors = new Color[colorNames.Length];
                colors.CopyTo(newColors, 0);
                colors = newColors;
            }

            for (int i = 0; i < colorNames.Length; i++)
            {
                colors[i] = EditorGUILayout.ColorField(colorNames[i], colors[i]);
            }
        }

    }
}