using System;
using UnityEngine;

namespace NodeEditor
{
    [Serializable]
    public class PerlinNode : Node
    {//int octaves, int lacunarity, float gain, int repeat, int seed

        public override string GetPath()
        {
            return "Operations/Perlin";
        }
        public PerlinNode()
        {
            Name = "Perlin";
            InitLinks(5);
            Inputs[0].Initialize(ValueType.Int, "Repeat Size");
            Inputs[1].Initialize(ValueType.Int, "Octaves");
            Inputs[2].Initialize(ValueType.Int, "Seed");
            Inputs[3].Initialize(ValueType.Float, "Roughness");
            Inputs[4].Initialize(ValueType.Vector2, "In", Link.Settings.ValueRequired | Link.Settings.NoDefaults);
            OutputType = ValueType.Float;
            NodeWidth = 9;
            CalcNodeHeight();
        }
        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            if (inTypes[0] == ValueType.Int &&
                inTypes[1] == ValueType.Int &&
                inTypes[2] == ValueType.Int &&
                inTypes[3] == ValueType.Float &&
                inTypes[4] == ValueType.Vector2) return ValueType.Float;
            return ValueType.Error;
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            processor.FloatOut = () =>
            {
                int repeatSize = processor.Inputs[0].IntOut();
                int octaves = processor.Inputs[1].IntOut();
                int seed = processor.Inputs[2].IntOut();
                float roughness = processor.Inputs[3].FloatOut();
                Vector2 in0 = processor.Inputs[4].Vector2Out();
                return Perlin.FBM(in0.x, in0.y, octaves, 2, roughness, repeatSize, seed);
            };
        }
    }
}