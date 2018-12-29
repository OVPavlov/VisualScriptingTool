using System;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor
{
    public abstract class Node
    {
        public Link[] Inputs;
        [NonSerialized]
        public ValueType OutputType;
        [NonSerialized]
        public ValueType CashedOutputType;
        [NonSerialized]
        public NodeProcessor Processor;
        [NonSerialized]
        public NodeData Parent;

        [NonSerialized]
        public string Name;
        public Vector2 Position;
        public Vector2 SizeInCells{get {return new Vector2(NodeWidth, NodeHeight);}}
        [NonSerialized]
        public string[] DrawProperties;
        [NonSerialized]
        public int NodeWidth = 7;
        [NonSerialized]
        public int NodeHeight = DefaultHeight;
        public const int DefaultHeight = 1;

        public const int NoNodeID = -1;
        public int NodeId = NoNodeID;
        [NonSerialized]
        public bool Controllable = true;




        protected void InitLinks(int count)
        {
            if (Inputs != null && Inputs.Length == count) return;
            Link[] newInputs = new Link[count];
            if (Inputs != null)
            {
                int length = Math.Min(newInputs.Length, Inputs.Length);
                for (int i = 0; i < length; i++)
                    newInputs[i] = Inputs[i];
                for (int i = Inputs.Length; i < newInputs.Length; i++)
                    newInputs[i] = new Link();
            }
            else
            {
                for (int i = 0; i < newInputs.Length; i++)
                    newInputs[i] = new Link();
            }
            Inputs = newInputs;
        }

        protected void CalcNodeHeight()
        {
            NodeHeight = DefaultHeight;
            if (DrawProperties != null)
                NodeHeight += DrawProperties.Length;
            NodeHeight += Inputs.Length;
        }

        public ValueType UpdateTypes(NodeData nodeData, HashSet<Node> loopProtection)
        {
            OnValidate();
            if (loopProtection.Contains(this)) return CashedOutputType = ValueType.Error;
            loopProtection.Add(this);

            ValueType[] inTypes = new ValueType[Inputs.Length];
            for (int i = 0; i < Inputs.Length; i++)
            {
                Link input = Inputs[i];
                Node inNode = nodeData.GetNode(input);
                if (inNode == null)
                    inTypes[i] = ValueType.None;
                else
                    inTypes[i] = inNode.UpdateTypes(nodeData, new HashSet<Node>(loopProtection));
            }
            return CashedOutputType = CheckInputsAndGetType(inTypes);
        }
        public ValueType UpdateTypesLight(NodeData nodeData)
        {
            OnValidate();
            ValueType[] inTypes = new ValueType[Inputs.Length];
            for (int i = 0; i < Inputs.Length; i++)
            {
                Link input = Inputs[i];
                Node inNode = nodeData.GetNode(input);
                if (inNode == null)
                    inTypes[i] = ValueType.None;
                else
                    inTypes[i] = inNode.UpdateTypesLight(nodeData);
            }
            return CashedOutputType = CheckInputsAndGetType(inTypes);
        }

        protected abstract ValueType CheckInputsAndGetType(ValueType[] inTypes);
        protected virtual void OnValidate() {}

        protected ValueType InputSame(params ValueType[] types)
        {
            ValueType type = types[0];
            for (int i = 1; i < types.Length; i++)
                if (type != types[i]) return ValueType.Error;
            return type;
        }
        protected ValueType InputExhact(ValueType input, ValueType type)
        {
            if (input != type) return ValueType.Error;
            return type;
        }
        protected bool InputIsExhactOrNone(ValueType input, ValueType type)
        {
            return input == type || input == ValueType.None;
        }
        protected ValueType InputVector(ValueType type)
        {
            return ValueTypeConfig.Types[(int)type].IsVector ? type : ValueType.Error;
        }
        protected ValueType InputValue(ValueType type)
        {
            return ValueTypeConfig.Types[(int)type].IsValue ? type : ValueType.Error;
        }
        protected ValueType InputVectorFloat(ValueType typeA, ValueType typeB)
        {
            bool isTypeAVector = ValueTypeConfig.Types[(int)typeA].IsVector;
            bool isTypeBVector = ValueTypeConfig.Types[(int)typeB].IsVector;

            if (isTypeAVector && isTypeBVector && typeA == typeB) return typeB;
            if (isTypeAVector && typeB == ValueType.Float) return typeA;
            if (isTypeBVector && typeA == ValueType.Float) return typeB;
            if (typeA == ValueType.Float && typeB == ValueType.Float) return ValueType.Float;
            return ValueType.Error;
        }
        protected ValueType InputVectorFloatInt(ValueType typeA, ValueType typeB)
        {
            bool isTypeAVector = ValueTypeConfig.Types[(int)typeA].IsVector;
            bool isTypeBVector = ValueTypeConfig.Types[(int)typeB].IsVector;
            bool isTypeAfoi = typeA == ValueType.Float || typeA == ValueType.Int;
            bool isTypeBfoi = typeB == ValueType.Float || typeB == ValueType.Int;

            if (isTypeAVector && isTypeBVector && typeA == typeB) return typeB;
            if (isTypeAVector && isTypeBfoi) return typeA;
            if (isTypeBVector && isTypeAfoi) return typeB;
            return ValueType.Error;
        }
        protected ValueType InputFloatInt(ValueType typeA, ValueType typeB)
        {
            bool isTypeAfoi = typeA == ValueType.Float || typeA == ValueType.Int;
            bool isTypeBfoi = typeB == ValueType.Float || typeB == ValueType.Int;

            if (!isTypeAfoi || !isTypeBfoi) return ValueType.Error;
            if (typeA == typeB) return typeA;
            return ValueType.Float;
        }


        public virtual void InitializeNodeProcessor(NodeProcessor processor) {}

        public abstract string GetPath();
    }
}