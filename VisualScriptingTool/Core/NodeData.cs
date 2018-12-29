using System;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor
{
    [Serializable]
    public partial class NodeData
    {
        public NodeDataExternals Externals;
        public int[] Outputs;
        NodeProcessor[] _processors;
        [NonSerialized]
        public float Dt;
        float _lastTime;

        #region Initialize

        public NodeData()
        {
            Outputs = new int[0];
            Externals = new NodeDataExternals();
        }

        public void Initialize()
        {
            Externals.InitializeFrom(this);
        }


        #endregion


        #region Public
        public void Prepare()
        {
            _processors = new NodeProcessor[Outputs.Length];
            for (int i = 0; i < Outputs.Length; i++)
            {
                Node node = GetNode(Outputs[i]);
                node.UpdateTypesLight(this);
                _processors[i] = GetNodeProcessor(node);
            }
        }

        public void Process()
        {
            float time = Time.time;
            Dt = time - _lastTime;
            _lastTime = time;
            NodeProcessor.CurrentFrame++;

            if (_processors == null) return;
            for (int i = 0; i < _processors.Length; i++)
                _processors[i].VoidOut();
        }

        #endregion




        public HashSet<Node> GetDeadEnds(List<Node> nodes)
        {
            HashSet<Node> deadEnds = new HashSet<Node>(nodes);
            foreach (Node node in nodes)
                foreach (Link link in node.Inputs)
                {
                    Node inNode = GetNode(link);
                    if (inNode == null) continue;
                    deadEnds.Remove(inNode);
                }
            return deadEnds;
        }

        public NodeProcessor GetNodeProcessor(Node node)
        {
            if (node == null) return null;
            if (node.CashedOutputType == ValueType.Error) return null;

            if (node.Processor == null)
            {
                node.Parent = this;
                node.Processor = new NodeProcessor();
                node.Processor.Inputs = new NodeProcessor[node.Inputs.Length];
                node.InitializeNodeProcessor(node.Processor);
            }

            node.Processor.OutputCount++;

            for (int i = 0; i < node.Inputs.Length; i++)
            {
                node.Processor.Inputs[i] = GetNodeProcessor(GetNode(node.Inputs[i]));
                if (node.Inputs[i].ValueRequired && node.Processor.Inputs[i] == null) return null;
            }


            if (node.Processor.OutputCount == 2) node.Processor.SeveralOutputsOptimisation();
            return node.Processor;

        }

        public bool ValidateNodes(Node node)
        {
            if (node == null) return false;
            if (node.CashedOutputType == ValueType.Error)
                return false;

            for (int i = 0; i < node.Inputs.Length; i++)
            {
                if (node.Inputs[i].Type == ValueType.Error)
                    return false;
                Node inNode = GetNode(node.Inputs[i]);
                if (node.Inputs[i].ValueRequired &&
                    (inNode == null || inNode.CashedOutputType == ValueType.Error)) return false;
            }

            return true;
        }
    }
}