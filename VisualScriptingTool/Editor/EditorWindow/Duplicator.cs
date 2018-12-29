using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NodeEditor
{
    public class Duplicator
    {
        NodeData _nodeData;

        public void Initialize(NodeData nodeData)
        {
            _nodeData = nodeData;
        }

        public void Duplicate(HashSet<Node> selection)
        {
            if (selection.Count == 0) return;

            {
                List<Node> nodesToremove = new List<Node>();
                foreach (Node node in selection)
                    if (!node.Controllable) nodesToremove.Add(node);
                foreach (Node node in nodesToremove)
                    selection.Remove(node);
            }

            List<LinkItem> savedLinks = new List<LinkItem>();
            Dictionary<Node, Node> nodes = new Dictionary<Node, Node>();

            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (Node node in selection)
            {
                minY = Math.Min(minY, node.Position.y);
                maxY = Math.Max(maxY, node.Position.y);
            }
            float shift = (maxY - minY) + 64;



            foreach (Node node in selection)
            {
                for (int i = 0; i < node.Inputs.Length; i++)
                {
                    Node inputNode = _nodeData.GetNode(node.Inputs[i]);
                    if (selection.Contains(inputNode))
                        savedLinks.Add(new LinkItem(node, i));
                }

                Node newNode = _nodeData.CreateNode(node);
                nodes.Add(node, newNode);
                newNode.Position = node.Position + new Vector2(1, shift);
                if (node.DrawProperties != null)
                    foreach (string property in node.DrawProperties)
                    {
                        CopyProperty(node, newNode, property);
                    }
            }


            foreach (LinkItem linkItem in savedLinks)
            {
                Node oldOut = linkItem.Node;
                Link link = oldOut.Inputs[linkItem.Input];
                Node oldIn = _nodeData.GetNode(link);

                Node newOut = nodes[oldOut];
                Node newIn = nodes[oldIn];
                newOut.Inputs[linkItem.Input].NodeId = newIn.NodeId;
            }


            selection.Clear();
            foreach (var pair in nodes)
                selection.Add(pair.Value);
        }

        static void CopyProperty(Node from, Node to, string property)
        {
            FieldInfo field = from.GetType().GetField(property);
            if (field == null) return;
            if (field.FieldType == typeof (AnimationCurve))
                field.SetValue(to, AnimationCurveNode.Copy((AnimationCurve)field.GetValue(from))); 
            else if (field.FieldType == typeof (Gradient))
                field.SetValue(to, GradientNode.Copy((Gradient)field.GetValue(from)));
            else
                field.SetValue(to, field.GetValue(from));
        }

        class LinkItem
        {
            public Node Node;
            public int Input;
            public LinkItem(Node node, int input)
            {
                Node = node;
                Input = input;
            }
        }
    }
}