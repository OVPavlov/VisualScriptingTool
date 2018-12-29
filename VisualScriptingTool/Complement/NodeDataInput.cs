using System.Collections.Generic;
using UnityEngine;

namespace NodeEditor
{
    [System.Serializable]
    public class NodeDataInput
    {
        public bool[] Bools;
        public Color[] Colors;
        public float[] Floats;
        public int[] Ints;
        public Vector2[] Vector2s;
        public Vector3[] Vector3s;
        public Vector4[] Vector4s;
        public AnimationCurve[] AnimationCurves;
        public Gradient[] Gradients;


        public NodeDataExternals Externals{get;private set;}
        public NodeData NodeData{get;private set;}


        public void InitializeFrom(NodeData nodeData)
        {
            NodeData = nodeData;
            Externals = nodeData.Externals;
            if (Externals == null || Externals.FloatNodes == null) NodeData.Initialize();
            Initialize();
        }

        public void Initialize()
        {
            if (Externals == null || Externals.FloatNodes == null) return;

            FixArray(ref Floats, Externals.FloatNodes, NodeData);
            FixArray(ref Ints, Externals.IntNodes, NodeData);
            FixArray(ref Bools, Externals.BoolNodes, NodeData);
            FixArray(ref Colors, Externals.ColorNodes, NodeData);
            FixArray(ref Vector2s, Externals.Vector2Nodes, NodeData);
            FixArray(ref Vector3s, Externals.Vector3Nodes, NodeData);
            FixArray(ref Vector4s, Externals.Vector4Nodes, NodeData);
            FixArray(ref AnimationCurves, Externals.AnimationCurveNodes, NodeData);
            FixArray(ref Gradients, Externals.GradientNodes, NodeData);
        }

        static void FixArray<T>(ref T[] array, List<Link> links, NodeData nodeData)
        {
            if (array == null) array = new T[0];
            if (array.Length != links.Count)
            {
                T[] newArray = new T[links.Count];
                int min = Mathf.Min(links.Count, array.Length);
                for (int i = 0; i < min; i++)
                    newArray[i] = array[i];
                for (int i = min; i < links.Count; i++)
                    newArray[i] = ((IGetSet<T>)nodeData.GetNode(links[i])).GetValue();
                array = newArray;
            }
        }

        public void SetTo(NodeData data)
        {
            if (Externals.FloatNodes == null) return;

            Set(Bools, Externals.BoolNodes, data);
            Set(Colors, Externals.ColorNodes, data);
            Set(Floats, Externals.FloatNodes, data);
            Set(Ints, Externals.IntNodes, data);
            Set(Vector2s, Externals.Vector2Nodes, data);
            Set(Vector3s, Externals.Vector3Nodes, data);
            Set(Vector4s, Externals.Vector4Nodes, data);
            Set(AnimationCurves, Externals.AnimationCurveNodes, data);
            Set(Gradients, Externals.GradientNodes, data);
        }

        static void Set<T>(T[] array, List<Link> links, NodeData data)
        {
            int length = Mathf.Min(array.Length, links.Count);
            for (int i = 0; i < length; i++)
                ((IGetSet<T>)data.GetNode(links[i])).SetValue(array[i]);
        }
    }
}