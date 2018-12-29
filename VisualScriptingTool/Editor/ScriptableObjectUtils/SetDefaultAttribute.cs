using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Must be above bool field</summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public class SetDefaultAttribute : PropertyAttribute
{

}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SetDefaultAttribute))]
public class SetDefaultDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position.height *= 0.5f;
        Color savedColor = GUI.color;
        GUI.color = new Color(1, 1, 1, 0.4f);
        GUI.Box(position, GUIContent.none);
        GUI.color = Color.white;
        
        Object obj = property.serializedObject.targetObject;
        Type type = obj.GetType();
        Object[] objects = Resources.LoadAll("", type);

        bool inResources = false;
        foreach (Object o in objects)
            if (o == obj)
            {
                inResources = true;
                break;
            }

        int defaultCount = 0;
        foreach (Object o in objects)
            if ((bool)fieldInfo.GetValue(o)) defaultCount++;
        if (defaultCount != 1)
        {
            foreach (Object o in objects)
                fieldInfo.SetValue(o, false);
            property.boolValue = true;
        }

        Object defaultObj = null;
        foreach (Object o in objects)
        {
            if (!(bool)fieldInfo.GetValue(o)) continue;
            defaultObj = o;
            break;
        }


        if (!inResources)
        {
            if (defaultObj == null)
            {
                GUI.Label(position, obj.name + " is not in Resources folder");
            }
            else
            {
                GUI.Label(position, obj.name + " is not in Resources folder. \'" + defaultObj.name + "\' is default");
                SelectObject(position, defaultObj);
            }
        }
        else if (property.boolValue)
        {
            GUI.Label(position, "This is default " + type.Name);
        }
        else
        {

            position.width *= 0.5f;
            GUI.Label(position, "\'" + defaultObj.name + "\' is default");
            SelectObject(position, defaultObj);


            position.x += position.width;
            if (GUI.Button(position, "Set as default"))
            {
                foreach (Object o in objects)
                    fieldInfo.SetValue(o, o == obj);
            }
        }
        GUI.color = savedColor;
    }

    void SelectObject(Rect position, Object obj)
    {
        EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
        if (Event.current.type == EventType.MouseDown && position.Contains(Event.current.mousePosition))
            //EditorGUIUtility.PingObject(obj);
            Selection.activeObject = obj;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) * 2f;
    }

}
#endif