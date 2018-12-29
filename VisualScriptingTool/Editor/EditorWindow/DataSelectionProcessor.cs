using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NodeEditor
{
    class DataSelectionProcessor
    {
        public Action<NodeData, Object> OnNewDataSelected;
        GenericMenu _dataMenu;
        Object _lastObj;
        Item _firstElement = null;
        public Object Host;


        public DataSelectionProcessor(Action<NodeData, Object> onNewDataSelected)
        {
            OnNewDataSelected = onNewDataSelected;
            _dataMenu = new GenericMenu();
        }

        public void Select(Object obj)
        {
            _lastObj = obj;
            _dataMenu = new GenericMenu();
            _firstElement = null;

            List<Item> list = GetDataFromObject(obj);
            if (list != null && list.Count > 0)
            {
                _dataMenu = CreateMenu(list);
                _firstElement = list[0];
            }

            if (_firstElement != null)
                SelectItem(_firstElement);
        }

        public void Process()
        {
            Object obj = Selection.activeObject;

            if (_lastObj != obj)
            {
                Select(obj);
            }
        }


        GenericMenu CreateMenu(List<Item> list)
        {
            GenericMenu menu = new GenericMenu();
            foreach (Item item in list)
                menu.AddItem(new GUIContent(item.Name), false, SelectItem, item);
            return menu;
        }

        static List<Item> GetDataFromObject(Object obj)
        {
            if (obj is GameObject)
                return GetDataFromGameObject((GameObject)obj);
            else
                return GetDataFromScript(obj);
        }

        static List<Item> GetDataFromGameObject(GameObject go)
        {
            MonoBehaviour[] monoBehaviours = go.GetComponents<MonoBehaviour>();
            List<List<Item>> listOfLists = new List<List<Item>>();
            List<string> names = new List<string>();
            foreach (MonoBehaviour monoBehaviour in monoBehaviours)
            {
                List<Item> list = GetDataFromScript(monoBehaviour);
                if (list == null || list.Count == 0) continue;
                listOfLists.Add(list);
                names.Add(monoBehaviour.GetType().Name);
            }
            if (names.Count == 0) return null;
            List<Item> result = new List<Item>();
            for (int i = 0; i < listOfLists.Count; i++)
            {
                string name = names[i];
                List<Item> list = listOfLists[i];
                if (listOfLists.Count > 1) name += "[" + i + "]";
                if (list.Count > 1)
                {
                    foreach (Item item in list)
                        item.Name = name + " " + item.Name;
                }
                else
                    list[0].Name = name;
            }
            foreach (List<Item> list in listOfLists)
                result.AddRange(list);
            return result;
        }

        static List<Item> GetDataFromScript(Object obj)
        {
            if (obj == null) return null;
            List<Item> list = null;
            FieldInfo[] fields = obj.GetType().GetFields();
            foreach (FieldInfo fieldInfo in fields)
                if (fieldInfo.FieldType == typeof (NodeData))
                {
                    NodeData nodeData = (NodeData)fieldInfo.GetValue(obj);
                    if (list == null) list = new List<Item>();
                    list.Add(new Item(nodeData, obj, fieldInfo.Name));
                }
            return list;
        }

        void SelectItem(object obj)
        {
            Item item = obj as Item;
            if (item == null) return;
            if (OnNewDataSelected != null)
                OnNewDataSelected(item.Data, item.Host);
            Host = item.Host;
        }

        public void DropDown()
        {
            if (_dataMenu.GetItemCount() < 2) return;
            if (GUILayout.Button("Select", EditorStyles.toolbarDropDown))
                _dataMenu.DropDown(new Rect(1, EditorStyles.toolbar.fixedHeight - 2, 1, 1));
        }

        public void Reset()
        {
            _lastObj = null;
            Process();
        }

        class Item
        {
            public string Name;
            public NodeData Data;
            public Object Host;
            public Item(NodeData data, Object host, string name)
            {
                Name = name;
                Data = data;
                Host = host;
            }
        }

    }
}