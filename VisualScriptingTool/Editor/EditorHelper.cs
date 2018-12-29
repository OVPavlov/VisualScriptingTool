using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NodeEditor
{
    public static class EditorHelper
    {
        public static void RepaintInspectors()
        {
            Type inspectorType = Assembly.GetAssembly(typeof (Editor)).GetType("UnityEditor.InspectorWindow");
            EditorWindow[] windows = (EditorWindow[])Resources.FindObjectsOfTypeAll(inspectorType);
            foreach (EditorWindow window in windows)
                window.Repaint();
        }

        public static void CallOnValidate(UnityEngine.Object obj)
        {
            for (Type t = obj.GetType(); t != typeof (object); t = t.BaseType)
            {
                var onValidate = t.GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
                if (onValidate == null) continue;
                onValidate.Invoke(obj, new object[0]);
            }
        }


        static readonly MethodInfo GradientMethodInfo2 = typeof (EditorGUI).GetMethod("GradientField", BindingFlags.NonPublic | BindingFlags.Static, null, new[] {typeof (string), typeof (Rect), typeof (Gradient)}, null);
        public static Gradient GradientField(Rect rect, string name, Gradient gradient)
        {
            if (gradient == null) gradient = new Gradient();
            return (Gradient)GradientMethodInfo2.Invoke(null, new object[] {name, rect, gradient});
        }

        static readonly MethodInfo GradientMethodInfo = typeof (EditorGUI).GetMethod("GradientField", BindingFlags.NonPublic | BindingFlags.Static, null, new[] {typeof (Rect), typeof (Gradient)}, null);
        public static object GradientField(Rect rect, Gradient gradient)
        {
            return GradientMethodInfo.Invoke(null, new object[] {rect, gradient});
        }




        public static object GetPropertyObject(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }


    }
}