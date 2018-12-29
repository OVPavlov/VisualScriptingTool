using System;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor
{
    public partial class NodeData : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public Dictionary<int, Node> Nodes;
        [SerializeField]
        int _lastId = 1;
        [SerializeField]
        int _lastDefaultNodeId = -2;
        [SerializeField]
        string _data;

        #region IO
        public bool GetBool(Node node)
        {
            BoolNode n = node as BoolNode;
            if (n == null) throw new System.Exception("Wrong type");
            return n.Value;
        }
        public void SetBool(Node node, bool value)
        {
            BoolNode n = node as BoolNode;
            if (n == null) throw new System.Exception("Wrong type");
            n.Value = value;
        }
        public int GetInt(Node node)
        {
            IntNode n = node as IntNode;
            if (n == null) throw new System.Exception("Wrong type");
            return n.Value;
        }
        public void SetInt(Node node, int value)
        {
            IntNode n = node as IntNode;
            if (n == null) throw new System.Exception("Wrong type");
            n.Value = value;
        }
        public Color GetColor(Node node)
        {
            ColorNode n = node as ColorNode;
            if (n == null) throw new System.Exception("Wrong type");
            return n.Value;
        }
        public void SetColor(Node node, Color value)
        {
            ColorNode n = node as ColorNode;
            if (n == null) throw new System.Exception("Wrong type");
            n.Value = value;
        }
        public float GetFloat(Node node)
        {
            FloatNode n = node as FloatNode;
            if (n == null) throw new System.Exception("Wrong type");
            return n.Value;
        }
        public void SetFloat(Node node, float value)
        {
            FloatNode n = node as FloatNode;
            if (n == null) throw new System.Exception("Wrong type");
            n.Value = value;
        }
        public Vector2 GetVector2(Node node)
        {
            Vector2Node n = node as Vector2Node;
            if (n == null) throw new System.Exception("Wrong type");
            return n.Value;
        }
        public void SetVector2(Node node, Vector2 value)
        {
            Vector2Node n = node as Vector2Node;
            if (n == null) throw new System.Exception("Wrong type");
            n.Value = value;
        }
        public Vector3 GetVector3(Node node)
        {
            Vector3Node n = node as Vector3Node;
            if (n == null) throw new System.Exception("Wrong type");
            return n.Value;
        }
        public void SetVector3(Node node, Vector3 value)
        {
            Vector3Node n = node as Vector3Node;
            if (n == null) throw new System.Exception("Wrong type");
            n.Value = value;
        }
        public Vector4 GetVector4(Node node)
        {
            Vector4Node n = node as Vector4Node;
            if (n == null) throw new System.Exception("Wrong type");
            return n.Value;
        }
        public void SetVector4(Node node, Vector4 value)
        {
            Vector4Node n = node as Vector4Node;
            if (n == null) throw new System.Exception("Wrong type");
            n.Value = value;
        }




        static readonly Dictionary<string, ValueNode> TempIONodes = new Dictionary<string, ValueNode>();
        static readonly int[] TempIOPositions = new int[2];
        public void BeginInputInit()
        {
            TempIONodes.Clear();
            foreach (KeyValuePair<int, Node> pair in Nodes)
            {
                ValueNode vn = pair.Value as ValueNode;
                if (vn == null || vn.IOMode == IOMode.None) continue;
                TempIONodes.Add(vn.ValueName, vn);
            }

            for (int i = 0; i < 2; i++)
                TempIOPositions[i] = 0;
        }
        public void EndInputInit()
        {
            foreach (KeyValuePair<string, ValueNode> pair in TempIONodes)
            {
                if (pair.Value.UseAsExternal) continue;
                RemoveNode(pair.Value);
            }
            TempIONodes.Clear();
            Prepare();
        }
        public Node CteateInput(ValueType valueType, string name)
        {
            bool createdNewOne;
            Node node = CteateIO(valueType, name, IOMode.Input, out createdNewOne);
            if (createdNewOne)
                node.Position = new Vector2(-10 - node.NodeWidth, TempIOPositions[0]) * 16;
            TempIOPositions[0] += node.NodeHeight + 2;
            return node;
        }
        public Node CteateOutput(ValueType valueType, string name)
        {
            bool createdNewOne;
            Node node = CteateIO(valueType, name, IOMode.Output, out createdNewOne);
            if (createdNewOne)
                node.Position = new Vector2(10, TempIOPositions[1]) * 16;
            TempIOPositions[1] += node.NodeHeight + 2;
            return node;
        }
        Node CteateIO(ValueType valueType, string name, IOMode mode, out bool createdNewOne)
        {
            createdNewOne = false;
            NodeCollector.NodeInfo info = NodeCollector.IOTypes[(int)valueType];
            if (info == null) throw new Exception("Wrong ValueType");
            ValueNode vn;
            if (!TempIONodes.TryGetValue(name, out vn))
            {
                vn = (ValueNode)CreateNode(info.TypeID);
                vn.ValueName = name;
                createdNewOne = true;
            }
            else TempIONodes.Remove(name);

            vn.IOMode = mode;
            return vn;
        }

        #endregion


        public Node GetNode(Link link)
        {
            return GetNode(link.NodeId);
        }
        public Node GetNode(int id)
        {
            Node node;
            if (Nodes.TryGetValue(id, out node)) return node;
            return null;
        }
        
        public Node CreateNode(string typeID)
        {
            return CreateNode(typeID, ++_lastId);
        }
        public Node CreateNode(string typeID, int nodeID)
        {
            NodeCollector.NodeInfo info;
            if (!NodeCollector.Nodes.TryGetValue(typeID, out info)) return null;
            Node node = (Node)info.Type.GetConstructor(new Type[0]).Invoke(new object[0]);
            node.NodeId = nodeID;
            Nodes.Add(nodeID, node);
            return node;
        }
        public Node CreateNode(Node node)
        {
            string typeID = NodeCollector.ToID(node.GetType());
            return CreateNode(typeID);
        }

        public Node CreateDefaultNode(ValueType valueType)
        {
            NodeCollector.NodeInfo info = NodeCollector.DefaultNodes[(int)valueType];
            if (info == null) return null;
            Node node = (Node)info.Type.GetConstructor(new Type[0]).Invoke(new object[0]);
            node.NodeId = --_lastDefaultNodeId;
            Nodes.Add(node.NodeId, node);
            return node;
        }

        public void RemoveNode(Node node)
        {
            foreach (KeyValuePair<int, Node> pair in Nodes)
            {
                foreach (Link link in pair.Value.Inputs)
                {
                    if (link.NodeId == node.NodeId)
                        link.NodeId =  Node.NoNodeID;
                }
            }
            Nodes.Remove(node.NodeId);
        }

        public List<Node> GetAllNodes()
        {
            if (Nodes == null) return new List<Node>();
            List<Node> list = new List<Node>();

            foreach (KeyValuePair<int, Node> pair in Nodes)
                list.Add(pair.Value);
            return list;
        }

        public void OnBeforeSerialize()
        {
            if (Nodes == null) Nodes = new Dictionary<int, Node>();
            _data = Serializer.Serialize(Nodes);
        }
        public void OnAfterDeserialize()
        {
            if (Nodes == null) Nodes = new Dictionary<int, Node>();
            Serializer.Deserialize(_data, this, Nodes);
        }
    }
}