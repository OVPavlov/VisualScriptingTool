using System;
using UnityEngine;

namespace NodeEditor
{
    [Serializable]
    public abstract class ExternalValueNode : Node
    {
        public string ValueName;
        public bool UseAsExternal;
    }

    [Serializable]
    public abstract class ValueNode : ExternalValueNode
    {
        [NonSerialized]
        public ValueType ValueNodeType;
        public IOMode IOMode;
        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            if (IOMode == IOMode.Output && !InputIsExhactOrNone(inTypes[0], ValueNodeType)) return ValueType.Error;
            IOUpdate();
            return (IOMode == IOMode.None || IOMode == IOMode.Input) ? ValueNodeType : ValueType.None;
        }

        protected override void OnValidate()
        {
            if (IOMode != IOMode.None) Name = ValueName;
            IOUpdate();
            Controllable = IOMode == IOMode.None;
            NodeHeight = DefaultHeight;
            CalcNodeHeight();
        }

        protected void IOUpdate()
        {
            if (IOMode == IOMode.Output)
            {
                InitLinks(1);
                Inputs[0].Initialize(ValueNodeType, "In", Link.Settings.ValueRequired | Link.Settings.NoDefaults);
                OutputType = ValueType.None;
            }
            else
            {
                InitLinks(0);
                OutputType = ValueNodeType;
            }
        }
    }

    public interface IGetSet<T>
    {
        T GetValue();
        void SetValue(T value);
    }

    [Serializable]
    public class FloatNode: ValueNode, IGetSet<float>
    {
        public float Value;
        public override string GetPath()
        {
            return "Values/Float";
        }
        public FloatNode()
        {
            Name = "Float";
            InitLinks(0);
            OutputType = ValueNodeType = ValueType.Float;
            DrawProperties = new[] {"Value"};
            NodeWidth = 6;
            CalcNodeHeight();
        }


        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            IOUpdate();
            if (IOMode == IOMode.Output)
                processor.VoidOut = () => Value = processor.Inputs[0].FloatOut();
            else
                processor.FloatOut = () => Value;

        }
        public float GetValue()
        {
            return Value;
        }
        public void SetValue(float value)
        {
            Value = value;
        }
    }

    [Serializable]
    public class Vector2Node : ValueNode, IGetSet<Vector2>
    {
        public Vector2 Value;

        public override string GetPath()
        {
            return "Values/Vector2";
        }
        public Vector2Node()
        {
            Name = "Vector2";
            InitLinks(0);
            OutputType = ValueNodeType = ValueType.Vector2;
            DrawProperties = new[] {"Value"};
            NodeWidth = 7;
            CalcNodeHeight();
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            IOUpdate();
            if (IOMode == IOMode.Output)
                processor.VoidOut = () => Value = processor.Inputs[0].Vector2Out();
            else
                processor.Vector2Out = () => Value;
        }
        public Vector2 GetValue()
        {
            return Value;
        }
        public void SetValue(Vector2 value)
        {
            Value = value;
        }
    }

    [Serializable]
    public class Vector3Node : ValueNode, IGetSet<Vector3>
    {
        public Vector3 Value;
        public override string GetPath()
        {
            return "Values/Vector3";
        }
        public Vector3Node()
        {
            Name = "Vector3";
            InitLinks(0);
            OutputType = ValueNodeType = ValueType.Vector3;
            DrawProperties = new[] {"Value"};
            NodeWidth = 11;
            CalcNodeHeight();
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            IOUpdate();
            if (IOMode == IOMode.Output)
                processor.VoidOut = () => Value = processor.Inputs[0].Vector3Out();
            else
                processor.Vector3Out = () => Value;
        }
        public Vector3 GetValue()
        {
            return Value;
        }
        public void SetValue(Vector3 value)
        {
            Value = value;
        }
    }
    
    [Serializable]
    public class Vector4Node : ValueNode, IGetSet<Vector4>
    {
        public Vector4 Value;
        public override string GetPath()
        {
            return "Values/Vector4";
        }
        public Vector4Node()
        {
            Name = "Vector4";
            InitLinks(0);
            OutputType = ValueNodeType = ValueType.Vector4;
            DrawProperties = new[] {"Value"};
            NodeWidth = 14;
            CalcNodeHeight();
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            IOUpdate();
            if (IOMode == IOMode.Output)
                processor.VoidOut = () => Value = processor.Inputs[0].Vector4Out();
            else
                processor.Vector4Out = () => Value;
        }
        public Vector4 GetValue()
        {
            return Value;
        }
        public void SetValue(Vector4 value)
        {
            Value = value;
        }
    }

    [Serializable]
    public class ColorNode : ValueNode, IGetSet<Color>
    {
        public Color Value;
        public override string GetPath()
        {
            return "Values/Color";
        }
        public ColorNode()
        {
            Name = "Color";
            InitLinks(0);
            OutputType = ValueNodeType = ValueType.Color;

            DrawProperties = new[] {"Value"};
            CalcNodeHeight();
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            IOUpdate();
            if (IOMode == IOMode.Output)
                processor.VoidOut = () => Value = processor.Inputs[0].ColorOut();
            else
                processor.ColorOut = () => Value;
        }
        public Color GetValue()
        {
            return Value;
        }
        public void SetValue(Color value)
        {
            Value = value;
        }
    }

    [Serializable]
    public class BoolNode : ValueNode, IGetSet<bool>
    {
        public bool Value;
        public override string GetPath()
        {
            return "Values/Bool";
        }
        public BoolNode()
        {
            Name = "Bool";
            InitLinks(0);
            OutputType = ValueNodeType = ValueType.Bool;
            DrawProperties = new[] {"Value"};
            NodeWidth = 6;
            CalcNodeHeight();
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            IOUpdate();
            if (IOMode == IOMode.Output)
                processor.VoidOut = () => Value = processor.Inputs[0].BoolOut();
            else
                processor.BoolOut = () => Value;
        }
        public bool GetValue()
        {
            return Value;
        }
        public void SetValue(bool value)
        {
            Value = value;
        }
    }

    [Serializable]
    public class IntNode : ValueNode, IGetSet<int>
    {
        public int Value;
        public override string GetPath()
        {
            return "Values/Int";
        }
        public IntNode()
        {
            Name = "Int";
            InitLinks(0);
            OutputType = ValueNodeType = ValueType.Int;
            DrawProperties = new[] { "Value" };
            NodeWidth = 6;
            CalcNodeHeight();
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            IOUpdate();
            if (IOMode == IOMode.Output)
                processor.VoidOut = () => Value = processor.Inputs[0].IntOut();
            else
                processor.IntOut = () => Value;

        }
        public int GetValue()
        {
            return Value;
        }
        public void SetValue(int value)
        {
            Value = value;
        }
    }


    [Serializable]
    public class AnimationCurveNode : ExternalValueNode, IGetSet<AnimationCurve>
    {
        public AnimationCurve Curve = AnimationCurve.Linear(0, 0, 1, 1);
        public override string GetPath()
        {
            return "Values/AnimationCurve";
        }
        public AnimationCurveNode()
        {
            Name = "AnimationCurve";
            InitLinks(1);
            Inputs[0].Initialize(ValueType.Float, "In", Link.Settings.ValueRequired | Link.Settings.NoDefaults);
            OutputType = ValueType.Float;
            DrawProperties = new[] { "Curve" };

            CalcNodeHeight();
        }
        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            if (inTypes[0] == ValueType.Float) return ValueType.Float;
            return ValueType.Error;
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            processor.FloatOut = () => Curve.Evaluate(processor.Inputs[0].FloatOut());
        }

        public static AnimationCurve Copy(AnimationCurve curve)
        {
            AnimationCurve newCurve = new AnimationCurve();
            newCurve.postWrapMode = curve.postWrapMode;
            newCurve.preWrapMode = curve.preWrapMode;
            Keyframe[] keys = new Keyframe[curve.keys.Length];
            curve.keys.CopyTo(keys, 0);
            newCurve.keys = keys;
            return newCurve;
        }
        public AnimationCurve GetValue()
        {
            return Copy(Curve);
        }
        public void SetValue(AnimationCurve value)
        {
            Curve = value;
        }
    }

    [Serializable]
    public class GradientNode : ExternalValueNode, IGetSet<Gradient>
    {
        public Gradient Gradient = new Gradient();
        public override string GetPath()
        {
            return "Values/Gradient";
        }
        public GradientNode()
        {
            Name = "Gradient";
            InitLinks(1);
            Inputs[0].Initialize(ValueType.Float, "In", Link.Settings.ValueRequired | Link.Settings.NoDefaults);
            OutputType = ValueType.Color;
            DrawProperties = new[] { "Gradient" };

            CalcNodeHeight();
        }
        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            if (inTypes[0] == ValueType.Float) return ValueType.Color;
            return ValueType.Error;
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            processor.ColorOut = () => Gradient.Evaluate(processor.Inputs[0].FloatOut());
        }
        public static Gradient Copy(Gradient gradient)
        {
            Gradient newGradient = new Gradient();
            newGradient.mode = gradient.mode;

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[gradient.alphaKeys.Length];
            gradient.alphaKeys.CopyTo(alphaKeys, 0);
            newGradient.alphaKeys = alphaKeys;

            GradientColorKey[] colorKeys = new GradientColorKey[gradient.colorKeys.Length];
            gradient.colorKeys.CopyTo(colorKeys, 0);
            newGradient.colorKeys = colorKeys;
            return newGradient;
        }
        public Gradient GetValue()
        {
            return Copy(Gradient);
        }
        public void SetValue(Gradient value)
        {
            Gradient = value;
        }
    }
    

    [Serializable]
    public class RandomNode: Node
    {
        public RandomType RType;

        public override string GetPath()
        {
            return "Values/Random";
        }
        public RandomNode()
        {
            Name = "Random";
            InitLinks(0);
            DrawProperties = new[] {"RType",};

            OutputType = ValueType.AutoVector;
            NodeWidth = 5;
            CalcNodeHeight();
        }

        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            return GetType(RType);
        }

        static ValueType GetType(RandomType type)
        {
            switch (type)
            {
                case RandomType.InsideUnitCircle:
                case RandomType.OnUnitCircle:
                    return ValueType.Vector2;
                case RandomType.InsideUnitSphere:
                case RandomType.OnUnitSphere:
                    return ValueType.Vector3;
                case RandomType.Value01:
                case RandomType.Valuem11:
                    return ValueType.Float;
                case RandomType.Rotation:
                case RandomType.RotationUniform:
                    return ValueType.Vector4;
            }
            return ValueType.Error;
        }

        protected override void OnValidate()
        {
            OutputType = GetType(RType);
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            if (RType == RandomType.InsideUnitCircle)
                processor.Vector2Out = () => UnityEngine.Random.insideUnitCircle;
            else if (RType == RandomType.OnUnitCircle)
                processor.Vector2Out = () => UnityEngine.Random.insideUnitCircle.normalized;
            if (RType == RandomType.InsideUnitSphere)
                processor.Vector3Out = () => UnityEngine.Random.insideUnitSphere;
            if (RType == RandomType.OnUnitSphere)
                processor.Vector3Out = () => UnityEngine.Random.onUnitSphere;
            if (RType == RandomType.Value01)
                processor.FloatOut = () => UnityEngine.Random.value;
            if (RType == RandomType.Valuem11)
                processor.FloatOut = () => UnityEngine.Random.Range(-1f, 1f);
            if (RType == RandomType.Rotation)
                processor.Vector4Out = () => ToV(UnityEngine.Random.rotation);
            if (RType == RandomType.RotationUniform)
                processor.Vector4Out = () => ToV(UnityEngine.Random.rotationUniform);

        }

        static Vector4 ToV(Quaternion q)
        {
            return new Vector4(q.x, q.y, q.z, q.w);
        }

        public enum RandomType
        {
            InsideUnitCircle,
            OnUnitCircle,
            InsideUnitSphere,
            OnUnitSphere,
            Value01,
            Valuem11,
            Rotation,
            RotationUniform
        }
    }

    public enum IOMode
    {
        None = 0,
        Input,
        Output
    }
}
