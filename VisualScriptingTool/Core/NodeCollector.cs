using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NodeEditor
{
    public static class NodeCollector
    {
        public static Dictionary<string, NodeInfo> Nodes = GetNodes();
        public static NodeInfo[] IOTypes = GetIOTypes(Nodes);
        public static NodeInfo[] DefaultNodes = GetDefaultNodes(Nodes);
        static Type[] GetTypes()
        {
            Type baseType = typeof (Node);
            Type[] allTypes = baseType.Assembly.GetTypes();
            return allTypes.Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract).ToArray();
        }

        static Dictionary<string, NodeInfo> GetNodes()
        {
            Type[] allTypes = GetTypes();
            Dictionary<string, NodeInfo> typesDict = new Dictionary<string, NodeInfo>();

            foreach (Type type in allTypes)
            {
                Node node = (Node)type.GetConstructor(new Type[0]).Invoke(new object[0]);
                var ioMode = node.GetType().GetField("IOMode");
                typesDict.Add(ToID(type), new NodeInfo
                {
                    Type = type,
                    TypeID = ToID(type),
                    Path = node.GetPath(),
                    IsIO = ioMode != null,
                    ValueType = node.OutputType
                });
            }

            return typesDict;
        }

        static NodeInfo[] GetIOTypes(Dictionary<string, NodeInfo> nodes)
        {
            NodeInfo[] types = new NodeInfo[Enum.GetValues(typeof (ValueType)).Length];
            foreach (var pair in nodes)
                if (pair.Value.IsIO)
                {
                    Node node = (Node)pair.Value.Type.GetConstructor(new Type[0]).Invoke(new object[0]);
                    ValueNode vn = node as ValueNode;
                    if (vn != null)
                        types[(int)vn.ValueNodeType] = pair.Value;
                }
            return types;
        }

        static NodeInfo[] GetDefaultNodes(Dictionary<string, NodeInfo> nodes)
        {
            NodeInfo[] types = new NodeInfo[Enum.GetValues(typeof (ValueType)).Length];
            foreach (var pair in nodes)
            {
                Node node = (Node)pair.Value.Type.GetConstructor(new Type[0]).Invoke(new object[0]);
                if (node is DefaultNode)
                    types[(int)node.OutputType] = pair.Value;
            }
            types[(int)ValueType.AutoValue] = types[(int)ValueType.Float];
            types[(int)ValueType.AutoVector] = types[(int)ValueType.Vector3];
            return types;
        }

        public static string ToID(Type type)
        {
            return type.Name; 
        }


        public static bool ValidateTypes(ValueType ourType, ValueType proposedType)
        {
            if (ourType == ValueType.AutoValue)
                return ValueTypeConfig.Types[(int)proposedType].IsValue;
            if (ourType == ValueType.AutoVector)
                return ValueTypeConfig.Types[(int)proposedType].IsVector;
            return ourType == proposedType;
        }

        public static ValueType GetValueType(Type type)
        {
            if (type == typeof(float))
                return ValueType.Float;
            if (type == typeof(Vector2))
                return ValueType.Vector2;
            if (type == typeof(Vector3))
                return ValueType.Vector3;
            if (type == typeof(Vector4))
                return ValueType.Vector4;
            if (type == typeof(Color))
                return ValueType.Color;
            if (type == typeof(int))
                return ValueType.Int;
            if (type == typeof(bool))
                return ValueType.Bool;
            if (type == typeof(Texture2D))
                return ValueType.Texture2D;
            if (type == typeof(RenderTexture))
                return ValueType.RenderTexture;
            return ValueType.Error;
        }
        
        public class NodeInfo
        {
            public string TypeID;
            public Type Type;
            public string Path;
            public ValueType ValueType;
            public bool IsIO;
        }
    }


    public enum ValueType
    {
        None,
        Error,
        AutoValue,
        AutoVector,
        Float,
        Vector2,
        Vector3,
        Vector4,
        Color,
        Int,
        Bool,
        FloatMap2D,
        Texture2D,
        RenderTexture,
        Polygon,
        Mesh
    }
}