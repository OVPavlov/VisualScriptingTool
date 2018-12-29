using System;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor
{
    [SerializeField]
    public class NodeDataExternals
    {
        public List<Link> BoolNodes;
        public List<Link> ColorNodes;
        public List<Link> FloatNodes;
        public List<Link> IntNodes;
        public List<Link> Vector2Nodes;
        public List<Link> Vector3Nodes;
        public List<Link> Vector4Nodes;
        public List<Link> AnimationCurveNodes;
        public List<Link> GradientNodes;

        public void InitializeFrom(NodeData data)
        {
            if (data.Nodes == null) return;

            if (BoolNodes == null) BoolNodes = new List<Link>();
            if (ColorNodes == null) ColorNodes = new List<Link>();
            if (FloatNodes == null) FloatNodes = new List<Link>();
            if (IntNodes == null) IntNodes = new List<Link>();
            if (Vector2Nodes == null) Vector2Nodes = new List<Link>();
            if (Vector3Nodes == null) Vector3Nodes = new List<Link>();
            if (Vector4Nodes == null) Vector4Nodes = new List<Link>();
            if (AnimationCurveNodes == null) AnimationCurveNodes = new List<Link>();
            if (GradientNodes == null) GradientNodes = new List<Link>();

            BoolNodes.Clear();
            ColorNodes.Clear();
            FloatNodes.Clear();
            IntNodes.Clear();
            Vector2Nodes.Clear();
            Vector3Nodes.Clear();
            Vector4Nodes.Clear();
            AnimationCurveNodes.Clear();
            GradientNodes.Clear();

            foreach (var pair in data.Nodes)
            {
                ExternalValueNode vn = pair.Value as ExternalValueNode;
                if (vn == null || !vn.UseAsExternal) continue;
                GetArray(vn).Add(new Link {NodeId = vn.NodeId, Name = vn.ValueName});
            }
        }

        List<Link> GetArray(ExternalValueNode vn)
        {
            if (vn is FloatNode)
                return FloatNodes;
            if (vn is IntNode)
                return IntNodes;
            if (vn is Vector2Node)
                return Vector2Nodes;
            if (vn is Vector3Node)
                return Vector3Nodes;
            if (vn is Vector4Node)
                return Vector4Nodes;
            if (vn is ColorNode)
                return ColorNodes;
            if (vn is BoolNode)
                return BoolNodes;
            if (vn is AnimationCurveNode)
                return AnimationCurveNodes;
            if (vn is GradientNode)
                return GradientNodes;
            throw new ArgumentOutOfRangeException("vn", vn, null);
        }
    }
}