using System;
using System.Linq;
using UnityEngine;

namespace NodeEditor
{
    public abstract class DefaultNode : Node, ICustomSerialiser
    {
        public override string GetPath()
        {
            return null;
        }

        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            return OutputType;
        }

        public abstract object GetValue();


        public abstract string Serialize();
        public abstract void Deserialize(string[] lines);

        public static void ToDefault(Link link, NodeData nodeData)
        {
            if (link.NoDefaults) return;
            if (link.NodeId > NoNodeID) return;

            int nodeID = link.NodeId < NoNodeID ? link.NodeId : link.LastDefaultNode;

            Node defaultNode = nodeID < NoNodeID ? nodeData.GetNode(link.NodeId) : null;

            if (defaultNode == null || !NodeCollector.ValidateTypes(link.Type, defaultNode.OutputType))
                defaultNode = nodeData.CreateDefaultNode(link.Type);

            link.LastDefaultNode = link.NodeId = defaultNode != null ? defaultNode.NodeId : NoNodeID;
        }



        public static void InitDefaults(Node node, NodeData nodeData)
        {
            foreach (Link link in node.Inputs)
                ToDefault(link, nodeData);
        }

        public static void InitDefaults(NodeData nodeData)
        {
            Node[] nodes = nodeData.Nodes.Values.ToArray();
            for (int i = 0; i < nodes.Length; i++)
                InitDefaults(nodes[i], nodeData);
        }
    }

    public abstract class DefaultNode<T> : DefaultNode
    {
        public T Value;

        public override object GetValue()
        {
            return Value;
        }
    }

    public class Fl: DefaultNode<float>
    {
        public Fl()
        {
            OutputType = ValueType.Float;
            NodeWidth = 3;
            InitLinks(0);
        }
        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            processor.FloatOut = () => Value;
        }
        public override string Serialize()
        {
            return Value.ToString();
        }
        public override void Deserialize(string[] lines)
        {
            Value = float.Parse(lines[2]);
        }
    }

    public class Bo : DefaultNode<bool>
    {
        public Bo()
        {
            OutputType = ValueType.Bool;
            NodeWidth = 1;
            InitLinks(0);
        }
        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            processor.BoolOut = () => Value;
        }
        public override string Serialize()
        {
            return Value ? "t" : "f";
        }
        public override void Deserialize(string[] lines)
        {
            Value = lines[2] == "t";
        }
    }

    public class In : DefaultNode<int>
    {
        public In()
        {
            OutputType = ValueType.Int;
            NodeWidth = 3;
            InitLinks(0);
        }
        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            processor.IntOut = () => Value;
        }
        public override string Serialize()
        {
            return Value.ToString();
        }
        public override void Deserialize(string[] lines)
        {
            Value = int.Parse(lines[2]);
        }
    }

    public class V2 : DefaultNode<Vector2>
    {
        public V2()
        {
            OutputType = ValueType.Vector2;
            NodeWidth = 5;
            InitLinks(0);
        }
        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            processor.Vector2Out = () => Value;
        }
        public override string Serialize()
        {
            return Value.x.ToString() + '|' + Value.y.ToString();
        }
        public override void Deserialize(string[] lines)
        {
            Value = new Vector2(float.Parse(lines[2]), float.Parse(lines[3]));
        }
    }

    public class V3 : DefaultNode<Vector3>
    {
        public V3()
        {
            OutputType = ValueType.Vector3;
            NodeWidth = 7;
            InitLinks(0);
        }
        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            processor.Vector3Out = () => Value;
        }
        public override string Serialize()
        {
            return Value.x.ToString() + '|' + Value.y.ToString() + '|' + Value.z.ToString();
        }
        public override void Deserialize(string[] lines)
        {
            Value = new Vector3(float.Parse(lines[2]), float.Parse(lines[3]), float.Parse(lines[4]));
        }
    }
    
    public class V4 : DefaultNode<Vector4>
    {
        public V4()
        {
            OutputType = ValueType.Vector4;
            NodeWidth = 9;
            InitLinks(0);
        }
        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            processor.Vector4Out = () => Value;
        }
        public override string Serialize()
        {
            return Value.x.ToString() + '|' + Value.y.ToString() + '|' + Value.z.ToString() + '|' + Value.w.ToString();
        }
        public override void Deserialize(string[] lines)
        {
            Value = new Vector4(float.Parse(lines[2]), float.Parse(lines[3]), float.Parse(lines[4]), float.Parse(lines[5]));
        }
    }
    
    public class Co : DefaultNode<Color>
    {
        public Co()
        {
            OutputType = ValueType.Color;
            NodeWidth = 1;
            InitLinks(0);
        }
        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            processor.ColorOut = () => Value;
        }
        public override string Serialize()
        {
            return Value.r.ToString() + '|' + Value.g.ToString() + '|' + Value.b.ToString() + '|' + Value.a.ToString();
        }
        public override void Deserialize(string[] lines)
        {
            Value = new Color(float.Parse(lines[2]), float.Parse(lines[3]), float.Parse(lines[4]), float.Parse(lines[5]));
        }
    }
    
}