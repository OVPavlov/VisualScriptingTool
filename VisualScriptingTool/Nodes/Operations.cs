using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NodeEditor
{
    [Serializable]
    public class LerpNode: Node
    {
        public override string GetPath()
        {
            return "Operations/Lerp";
        }
        public LerpNode()
        {
            Name = "Lerp";
            InitLinks(3);
            Inputs[0].Initialize(ValueType.AutoValue, "A");
            Inputs[1].Initialize(ValueType.AutoValue, "B");
            Inputs[2].Initialize(ValueType.Float, "t");
            OutputType = ValueType.AutoValue;
            NodeWidth = 3;
            CalcNodeHeight();
        }

        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            ValueType type = InputSame(inTypes[0], inTypes[1]);
            type = InputValue(type);
            return inTypes[2] == ValueType.Float ? type : ValueType.Error;
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            processor.FloatOut = () => Mathf.LerpUnclamped(processor.Inputs[0].FloatOut(), processor.Inputs[1].FloatOut(), processor.Inputs[2].FloatOut());
            processor.Vector2Out = () => Vector2.LerpUnclamped(processor.Inputs[0].Vector2Out(), processor.Inputs[1].Vector2Out(), processor.Inputs[2].FloatOut());
            processor.Vector3Out = () => Vector3.LerpUnclamped(processor.Inputs[0].Vector3Out(), processor.Inputs[1].Vector3Out(), processor.Inputs[2].FloatOut());
            processor.Vector4Out = () => Vector4.LerpUnclamped(processor.Inputs[0].Vector4Out(), processor.Inputs[1].Vector4Out(), processor.Inputs[2].FloatOut());
            processor.ColorOut = () => Color.LerpUnclamped(processor.Inputs[0].ColorOut(), processor.Inputs[1].ColorOut(), processor.Inputs[2].FloatOut());
            processor.IntOut = () => (int)Mathf.LerpUnclamped(processor.Inputs[0].IntOut(), processor.Inputs[1].IntOut(), processor.Inputs[2].FloatOut());
        }
    }

    [Serializable]
    public class ConditionNode: Node
    {
        public OperationType _Type;
        public bool Invert;
        public override string GetPath()
        {
            return "Operations/Condition";
        }
        public ConditionNode()
        {
            Name = "Condition";
            InitLinks(2);
            Inputs[0].Initialize(ValueType.Float, "A");
            Inputs[1].Initialize(ValueType.Float, "B");
            DrawProperties = new[] {"_Type", "Invert"};
            OutputType = ValueType.Bool;
            NodeWidth = 6;
            CalcNodeHeight();
        }

        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            if (inTypes[0] != ValueType.Float) return ValueType.Error;
            if (inTypes[1] != ValueType.Float) return ValueType.Error;
            return ValueType.Bool;
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            switch (_Type)
            {
                case OperationType.AMoreThanB:
                    processor.BoolOut = () => (processor.Inputs[0].FloatOut() > processor.Inputs[1].FloatOut()) ^ Invert;
                    break;
                case OperationType.AMoreOrEqualB:
                    processor.BoolOut = () => (processor.Inputs[0].FloatOut() >= processor.Inputs[1].FloatOut()) ^ Invert;
                    break;
                case OperationType.AEqualB:
                    processor.BoolOut = () => (processor.Inputs[0].FloatOut() == processor.Inputs[1].FloatOut()) ^ Invert;
                    break;
            }
        }

        public enum OperationType
        {
            AMoreThanB,
            AMoreOrEqualB,
            AEqualB,
        }
    }
    
    [Serializable]
    public class IfNode : Node
    {
        public override string GetPath()
        {
            return "Operations/If";
        }
        public IfNode()
        {
            Name = "If";
            InitLinks(3);
            Inputs[0].Initialize(ValueType.Bool, "If");
            Inputs[1].Initialize(ValueType.AutoValue, "Than");
            Inputs[2].Initialize(ValueType.AutoValue, "Else");
            OutputType = ValueType.AutoValue;
            NodeWidth = 3;
            CalcNodeHeight();
        }

        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            if (inTypes[0] != ValueType.Bool) return ValueType.Error;
            return InputValue(InputSame(inTypes[1], inTypes[2]));
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            processor.FloatOut = () => processor.Inputs[0].BoolOut() ? processor.Inputs[1].FloatOut() : processor.Inputs[2].FloatOut();
            processor.Vector2Out = () => processor.Inputs[0].BoolOut() ? processor.Inputs[1].Vector2Out() : processor.Inputs[2].Vector2Out();
            processor.Vector3Out = () => processor.Inputs[0].BoolOut() ? processor.Inputs[1].Vector3Out() : processor.Inputs[2].Vector3Out();
            processor.Vector4Out = () => processor.Inputs[0].BoolOut() ? processor.Inputs[1].Vector4Out() : processor.Inputs[2].Vector4Out();
            processor.ColorOut = () => processor.Inputs[0].BoolOut() ? processor.Inputs[1].ColorOut() : processor.Inputs[2].ColorOut();
        }
    }
    
    [Serializable]
    public class BoolOperatorNode: Node
    {
        public OperationType _Type;
        bool _lastVal;
        public override string GetPath()
        {
            return "Operations/BoolOperator";
        }
        public BoolOperatorNode()
        {
            Name = "BoolOperator";
            InitLinks(2);
            Inputs[0].Initialize(ValueType.Bool, "A");
            Inputs[1].Initialize(ValueType.Bool, "B");
            DrawProperties = new[] {"_Type"};
            OutputType = ValueType.Bool;
            NodeWidth = 5;
            CalcNodeHeight();
        }

        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            if (inTypes[0] != ValueType.Bool) return ValueType.Error;
            if (inTypes[1] != ValueType.Bool && inTypes[1] != ValueType.None) return ValueType.Error;
            return ValueType.Bool;
        }


        protected override void OnValidate()
        {
            switch (_Type)
            {
                case OperationType.InvertA:
                case OperationType.TriggerA:
                    Inputs[1].ValueRequired = false;
                    break;
                default:
                    Inputs[1].ValueRequired = true;
                    break;
            }
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            switch (_Type)
            {
                case OperationType.InvertA:
                    processor.BoolOut = () => !processor.Inputs[0].BoolOut();
                    break;
                case OperationType.TriggerA:
                    processor.BoolOut = () =>
                    {
                        bool val = processor.Inputs[0].BoolOut();
                        if (_lastVal == val) return false;
                        _lastVal = val;
                        return true;
                    };
                    break;
                case OperationType.Equal:
                    processor.BoolOut = () => processor.Inputs[0].BoolOut() == processor.Inputs[1].BoolOut();
                    break;
                case OperationType.Or:
                    processor.BoolOut = () => processor.Inputs[0].BoolOut() || processor.Inputs[1].BoolOut();
                    break;
                case OperationType.And:
                    processor.BoolOut = () => processor.Inputs[0].BoolOut() && processor.Inputs[1].BoolOut();
                    break;
                case OperationType.Xor:
                    processor.BoolOut = () => processor.Inputs[0].BoolOut() ^ processor.Inputs[1].BoolOut();
                    break;
            }
        }

        public enum OperationType
        {
            InvertA,
            TriggerA,
            Equal,
            Or,
            And,
            Xor
        }
    }
    
    [Serializable]
    public class VecModifyNode : Node
    {
        static readonly Vector2 _zero2 = new Vector2();
        static Vector3 _zero3 = new Vector3();
        static Vector4 _zero4 = new Vector4();
        static Color _zeroC = new Color();

        public override string GetPath()
        {
            return "Operations/VecModify";
        }
        public VecModifyNode()
        {
            Name = "VecModify";
            InitLinks(1);
            Inputs[0].Initialize(ValueType.AutoVector, "Vec", Link.Settings.NoDefaults);
            OutputType = ValueType.AutoVector;
            NodeWidth = 4;
            CalcNodeHeight();
        }


        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            const Link.Settings settings = Link.Settings.NoDefaults;
            switch (inTypes[0])
            {
                case ValueType.Vector2:
                    InitLinks(3);
                    Inputs[1].Initialize(ValueType.Float, "x", settings);
                    Inputs[2].Initialize(ValueType.Float, "y", settings);
                    break;
                case ValueType.Vector3:
                    InitLinks(4);
                    Inputs[1].Initialize(ValueType.Float, "x", settings);
                    Inputs[2].Initialize(ValueType.Float, "y", settings);
                    Inputs[3].Initialize(ValueType.Float, "z", settings);
                    break;
                case ValueType.Vector4:
                    InitLinks(5);
                    Inputs[1].Initialize(ValueType.Float, "x", settings);
                    Inputs[2].Initialize(ValueType.Float, "y", settings);
                    Inputs[3].Initialize(ValueType.Float, "z", settings);
                    Inputs[4].Initialize(ValueType.Float, "w", settings);
                    break;
                case ValueType.Color:
                    InitLinks(5);
                    Inputs[1].Initialize(ValueType.Float, "r", settings);
                    Inputs[2].Initialize(ValueType.Float, "g", settings);
                    Inputs[3].Initialize(ValueType.Float, "b", settings);
                    Inputs[4].Initialize(ValueType.Float, "a", settings);
                    break;
                default:
                    InitLinks(1);
                    break;
            }
            CalcNodeHeight();

            for (int i = 1; i < inTypes.Length; i++)
                if (!InputIsExhactOrNone(inTypes[i], ValueType.Float)) return ValueType.Error;

            return InputVector(inTypes[0]);
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            processor.Vector2Out = () =>
            {
                NodeProcessor inv = processor.Inputs[0];
                Vector2 v = inv != null ? inv.Vector2Out() : _zero2;
                NodeProcessor inx = processor.Inputs[1];
                NodeProcessor iny = processor.Inputs[2];
                if (inx != null) v.x = inx.FloatOut();
                if (iny != null) v.y = iny.FloatOut();
                return v;
            };
            processor.Vector3Out = () =>
            {
                NodeProcessor inv = processor.Inputs[0];
                Vector3 v = inv != null ? inv.Vector3Out() : _zero3;
                NodeProcessor inx = processor.Inputs[1];
                NodeProcessor iny = processor.Inputs[2];
                NodeProcessor inz = processor.Inputs[3];
                if (inx != null) v.x = inx.FloatOut();
                if (iny != null) v.y = iny.FloatOut();
                if (inz != null) v.z = inz.FloatOut();
                return v;
            };
            processor.Vector4Out = () =>
            {
                NodeProcessor inv = processor.Inputs[0];
                Vector4 v = inv != null ? inv.Vector4Out() : _zero4;
                NodeProcessor inx = processor.Inputs[1];
                NodeProcessor iny = processor.Inputs[2];
                NodeProcessor inz = processor.Inputs[3];
                NodeProcessor inw = processor.Inputs[4];
                if (inx != null) v.x = inx.FloatOut();
                if (iny != null) v.y = iny.FloatOut();
                if (inz != null) v.z = inz.FloatOut();
                if (inw != null) v.w = inw.FloatOut();
                return v;
            };
            processor.ColorOut = () =>
            {
                NodeProcessor inv = processor.Inputs[0];
                Color v = inv != null ? inv.ColorOut() : _zeroC;
                NodeProcessor inx = processor.Inputs[1];
                NodeProcessor iny = processor.Inputs[2];
                NodeProcessor inz = processor.Inputs[3];
                NodeProcessor inw = processor.Inputs[4];
                if (inx != null) v.r = inx.FloatOut();
                if (iny != null) v.g = iny.FloatOut();
                if (inz != null) v.b = inz.FloatOut();
                if (inw != null) v.a = inw.FloatOut();
                return v;
            };
        }
    }

    [Serializable]
    public class VecConstrNode : Node
    {
        static readonly Vector2 _zero2 = new Vector2();
        static Vector3 _zero3 = new Vector3();
        static Vector4 _zero4 = new Vector4();
        static Color _zeroC = new Color();
        public VectorType VType;

        public override string GetPath()
        {
            return "Operations/VecConstructor";
        }
        public VecConstrNode()
        {
            Name = "Constructor";
            InitLinks(0);
            DrawProperties = new[] { "VType" };

            OutputType = ValueType.AutoVector;
            NodeWidth = 4;
            CalcNodeHeight();
        }

        protected override void OnValidate()
        {
            const Link.Settings settings = Link.Settings.None;
            if (VType == VectorType.Vec2)
            {
                InitLinks(2);
                Inputs[0].Initialize(ValueType.Float, "x", settings);
                Inputs[1].Initialize(ValueType.Float, "y", settings);

            }
            else if (VType == VectorType.Vec3)
            {
                InitLinks(3);
                Inputs[0].Initialize(ValueType.Float, "x", settings);
                Inputs[1].Initialize(ValueType.Float, "y", settings);
                Inputs[2].Initialize(ValueType.Float, "z", settings);
            }
            else if (VType == VectorType.Vec4)
            {
                InitLinks(4);
                Inputs[0].Initialize(ValueType.Float, "x", settings);
                Inputs[1].Initialize(ValueType.Float, "y", settings);
                Inputs[2].Initialize(ValueType.Float, "z", settings);
                Inputs[3].Initialize(ValueType.Float, "w", settings);
            }
            else if (VType == VectorType.Color)
            {
                InitLinks(4);
                Inputs[0].Initialize(ValueType.Float, "r", settings);
                Inputs[1].Initialize(ValueType.Float, "g", settings);
                Inputs[2].Initialize(ValueType.Float, "b", settings);
                Inputs[3].Initialize(ValueType.Float, "a", settings);
            }
            CalcNodeHeight();
        }

        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            for (int i = 0; i < inTypes.Length; i++)
                if (!InputIsExhactOrNone(inTypes[i], ValueType.Float)) return ValueType.Error;
            switch (VType)
            {
                case VectorType.Vec2:
                    return ValueType.Vector2;
                case VectorType.Vec3:
                    return ValueType.Vector3;
                case VectorType.Vec4:
                    return ValueType.Vector4;
                case VectorType.Color:
                    return ValueType.Color;
            }
            return ValueType.Error;
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            processor.Vector2Out = () =>
            {
                Vector2 v = _zero2;
                NodeProcessor inx = processor.Inputs[0];
                NodeProcessor iny = processor.Inputs[1];
                if (inx != null) v.x = inx.FloatOut();
                if (iny != null) v.y = iny.FloatOut();
                return v;
            };
            processor.Vector3Out = () =>
            {
                Vector3 v = _zero3;
                NodeProcessor inx = processor.Inputs[0];
                NodeProcessor iny = processor.Inputs[1];
                NodeProcessor inz = processor.Inputs[2];
                if (inx != null) v.x = inx.FloatOut();
                if (iny != null) v.y = iny.FloatOut();
                if (inz != null) v.z = inz.FloatOut();
                return v;
            };
            processor.Vector4Out = () =>
            {
                Vector4 v = _zero4;
                NodeProcessor inx = processor.Inputs[0];
                NodeProcessor iny = processor.Inputs[1];
                NodeProcessor inz = processor.Inputs[2];
                NodeProcessor inw = processor.Inputs[3];
                if (inx != null) v.x = inx.FloatOut();
                if (iny != null) v.y = iny.FloatOut();
                if (inz != null) v.z = inz.FloatOut();
                if (inw != null) v.w = inw.FloatOut();
                return v;
            };
            processor.ColorOut = () =>
            {
                Color v = _zeroC;
                NodeProcessor inx = processor.Inputs[0];
                NodeProcessor iny = processor.Inputs[1];
                NodeProcessor inz = processor.Inputs[2];
                NodeProcessor inw = processor.Inputs[3];
                if (inx != null) v.r = inx.FloatOut();
                if (iny != null) v.g = iny.FloatOut();
                if (inz != null) v.b = inz.FloatOut();
                if (inw != null) v.a = inw.FloatOut();
                return v;
            };
        }

        public enum VectorType
        {
            Vec2,
            Vec3,
            Vec4,
            Color,
        }
    }

    [Serializable]
    public class ColorHSVNode: Node
    {
        public OperationType _Type;
        public override string GetPath()
        {
            return "Operations/ColorHSV";
        }
        public ColorHSVNode()
        {
            Name = "RGB/HSV";
            InitLinks(1);
            Inputs[0].Initialize(ValueType.Color, "RGB");
            OutputType = ValueType.Color;
            DrawProperties = new[] {"_Type"};
            NodeWidth = 5;
            CalcNodeHeight();
        }

        protected override void OnValidate()
        {
            InitLinks(_Type == OperationType.HSVv_RGB ? 3 : 1);
            if (_Type == OperationType.HSV_RGB)
                Inputs[0].Initialize(ValueType.Vector3, "HSV");
            else if (_Type == OperationType.RGB_HSV)
                Inputs[0].Initialize(ValueType.Color, "RGB");
            else
            {
                Inputs[0].Initialize(ValueType.Float, "Hue");
                Inputs[1].Initialize(ValueType.Float, "Saturation");
                Inputs[2].Initialize(ValueType.Float, "Value");
            }
            CalcNodeHeight();
        }

        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            if (_Type == OperationType.HSV_RGB)
                return inTypes[0] == ValueType.Vector3 ? ValueType.Color : ValueType.Error;
            else if (_Type == OperationType.RGB_HSV)
                return inTypes[0] == ValueType.Color ? ValueType.Vector3 : ValueType.Error;
            else
                return InputSame(inTypes[0], inTypes[1], inTypes[2]) == ValueType.Float ? ValueType.Color : ValueType.Error;
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            if (_Type == OperationType.HSV_RGB)
            {
                processor.ColorOut = () =>
                {
                    Vector3 v = processor.Inputs[0].Vector3Out();
                    return Color.HSVToRGB(v.x, v.y, v.z, true);
                };
            }
            else if (_Type == OperationType.RGB_HSV)
            {
                processor.Vector3Out = () =>
                {
                    Vector3 v = new Vector3();
                    Color.RGBToHSV(processor.Inputs[0].ColorOut(), out v.x, out v.y, out v.z);
                    return v;
                };
            }
            else if (_Type == OperationType.HSVv_RGB)
            {
                processor.ColorOut = () => { return Color.HSVToRGB(processor.Inputs[0].FloatOut(), processor.Inputs[1].FloatOut(), processor.Inputs[2].FloatOut(), true); };
            }
        }

        public enum OperationType
        {
            RGB_HSV,
            HSV_RGB,
            HSVv_RGB,
        }
    }

    [Serializable]
    public class ConvertNode: Node
    {
        const float One2Deg = 360f;
        const float Deg2One = 1f / 360f;
        const float One2Rad = One2Deg * Mathf.Deg2Rad;
        const float Rad2One = 1f / One2Rad;

        public OperationType _Type;
        Func<float, float> _operation;
        public override string GetPath()
        {
            return "Operations/Convert";
        }
        public ConvertNode()
        {
            Name = "Convert";
            InitLinks(1);
            Inputs[0].Initialize(ValueType.AutoValue, "In");
            OutputType = ValueType.AutoValue;
            DrawProperties = new[] {"_Type"};
            NodeWidth = 5;
            CalcNodeHeight();
        }

        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            return ValueTypeConfig.Types[(int)inTypes[0]].IsValueExceptInt ? inTypes[0] : ValueType.Error;
        }

        void OperationInitializer(OperationType type)
        {
            switch (type)
            {
                case OperationType.Deg2Rad:
                    _operation = (a) => a * Mathf.Deg2Rad;
                    break;
                case OperationType.Rad2Deg:
                    _operation = (a) => a * Mathf.Rad2Deg;
                    break;
                case OperationType.One2Rad:
                    _operation = (a) => a * One2Rad;
                    break;
                case OperationType.Rad2One:
                    _operation = (a) => a * Rad2One;
                    break;
                case OperationType.One2Deg:
                    _operation = (a) => a * One2Deg;
                    break;
                case OperationType.Deg2One:
                    _operation = (a) => a * Deg2One;
                    break;
            }
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            OperationInitializer(_Type);
            processor.FloatOut = () => _operation(processor.Inputs[0].FloatOut());
            processor.Vector2Out = () =>
            {
                Vector2 a = processor.Inputs[0].Vector2Out();
                return new Vector2(_operation(a.x), _operation(a.y));
            };
            processor.Vector3Out = () =>
            {
                Vector3 a = processor.Inputs[0].Vector3Out();
                return new Vector3(_operation(a.x), _operation(a.y), _operation(a.z));
            };
            processor.Vector4Out = () =>
            {
                Vector4 a = processor.Inputs[0].Vector4Out();
                return new Vector4(_operation(a.x), _operation(a.y), _operation(a.z), _operation(a.w));
            };
            processor.ColorOut = () =>
            {
                Color a = processor.Inputs[0].ColorOut();
                return new Color(_operation(a.r), _operation(a.g), _operation(a.b), _operation(a.a));
            };
        }

        public enum OperationType
        {
            Deg2Rad,
            Rad2Deg,
            One2Rad,
            Rad2One,
            One2Deg,
            Deg2One,
        }
    }
    
    [Serializable]
    public class VecSwizzleNode: Node
    {
        public string Components;
        int _comp0;
        int _comp1;
        int _comp2;
        int _comp3;

        public override string GetPath()
        {
            return "Operations/Swizzle";
        }
        public VecSwizzleNode()
        {
            Name = "Swizzle";
            InitLinks(1);
            Inputs[0].Initialize(ValueType.AutoValue, "Vec", Link.Settings.NoDefaults);
            DrawProperties = new[] {"Components"};

            OutputType = ValueType.Float;
            NodeWidth = 5;
            CalcNodeHeight();
        }

        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            ValueType input = inTypes[0];
            ValueTypeConfig config = ValueTypeConfig.Types[(int)input];
            if (!config.IsValue) return ValueType.Error;


            int[] comps = ValidateSwizzle(ref Components, GetComponentsCount(input) - 1);

            if (comps.Length > 0) _comp0 = comps[0];
            if (comps.Length > 1) _comp1 = comps[1];
            if (comps.Length > 2) _comp2 = comps[2];
            if (comps.Length > 3) _comp3 = comps[3];

            bool siColor = IsColor(Components);
            return GetOutType(comps.Length, siColor);
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            GeneratedSwizzles(processor, Parent.GetNode(Inputs[0]).CashedOutputType);
        }

        static void GenerateSwizzles()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            ValueType[] types = {ValueType.Float, ValueType.Vector2, ValueType.Vector3, ValueType.Vector4, ValueType.Color, ValueType.Int};
            string[] typeNames = {"float", "Vector2", "Vector3", "Vector4", "Color", "int"};
            string[] constructors = {"{0}", "new Vector2({0})", "new Vector3({0})", "new Vector4({0})", "new Color({0})", "(int){0}"};


            for (int i = 0; i < types.Length; i++)
            {
                ValueType input = types[i];
                string inputName = typeNames[i];
                bool scalar = input == ValueType.Float || input == ValueType.Int;

                sb.AppendFormat(@"            {1}if (input == ValueType.{0})
                {{", input, i == 0 ? "" : "else ");

                for (int o = 0; o < types.Length; o++)
                {
                    ValueType output = types[o];
                    string outputName = typeNames[o];
                    int compCount = GetComponentsCount(output);



                    string inputs = "";
                    for (int j = 0; j < compCount; j++)
                        inputs += string.Format((j == 0 ? "" : ", ") + (scalar ? "v" : "v[_comp{0}]"), j);

                    inputs = string.Format(constructors[o], inputs);

                    sb.AppendFormat(@"   processor.{0}Out = () =>
            {{
                {3} v = processor.Inputs[0].{2}Out();
                return {4};
            }};", output, outputName, input, inputName, inputs);

                }
                sb.Append(@"                }");
            }

            Debug.Log(sb.ToString());



        }

        void GeneratedSwizzles(NodeProcessor processor, ValueType input)
        {
            if (input == ValueType.Float)
            {
                processor.FloatOut = () =>
                {
                    float v = processor.Inputs[0].FloatOut();
                    return v;
                };
                processor.Vector2Out = () =>
                {
                    float v = processor.Inputs[0].FloatOut();
                    return new Vector2(v, v);
                };
                processor.Vector3Out = () =>
                {
                    float v = processor.Inputs[0].FloatOut();
                    return new Vector3(v, v, v);
                };
                processor.Vector4Out = () =>
                {
                    float v = processor.Inputs[0].FloatOut();
                    return new Vector4(v, v, v, v);
                };
                processor.ColorOut = () =>
                {
                    float v = processor.Inputs[0].FloatOut();
                    return new Color(v, v, v, v);
                };
                processor.IntOut = () =>
                {
                    float v = processor.Inputs[0].FloatOut();
                    return (int)v;
                };
            }
            else if (input == ValueType.Vector2)
            {
                processor.FloatOut = () =>
                {
                    Vector2 v = processor.Inputs[0].Vector2Out();
                    return v[_comp0];
                };
                processor.Vector2Out = () =>
                {
                    Vector2 v = processor.Inputs[0].Vector2Out();
                    return new Vector2(v[_comp0], v[_comp1]);
                };
                processor.Vector3Out = () =>
                {
                    Vector2 v = processor.Inputs[0].Vector2Out();
                    return new Vector3(v[_comp0], v[_comp1], v[_comp2]);
                };
                processor.Vector4Out = () =>
                {
                    Vector2 v = processor.Inputs[0].Vector2Out();
                    return new Vector4(v[_comp0], v[_comp1], v[_comp2], v[_comp3]);
                };
                processor.ColorOut = () =>
                {
                    Vector2 v = processor.Inputs[0].Vector2Out();
                    return new Color(v[_comp0], v[_comp1], v[_comp2], v[_comp3]);
                };
                processor.IntOut = () =>
                {
                    Vector2 v = processor.Inputs[0].Vector2Out();
                    return (int)v[_comp0];
                };
            }
            else if (input == ValueType.Vector3)
            {
                processor.FloatOut = () =>
                {
                    Vector3 v = processor.Inputs[0].Vector3Out();
                    return v[_comp0];
                };
                processor.Vector2Out = () =>
                {
                    Vector3 v = processor.Inputs[0].Vector3Out();
                    return new Vector2(v[_comp0], v[_comp1]);
                };
                processor.Vector3Out = () =>
                {
                    Vector3 v = processor.Inputs[0].Vector3Out();
                    return new Vector3(v[_comp0], v[_comp1], v[_comp2]);
                };
                processor.Vector4Out = () =>
                {
                    Vector3 v = processor.Inputs[0].Vector3Out();
                    return new Vector4(v[_comp0], v[_comp1], v[_comp2], v[_comp3]);
                };
                processor.ColorOut = () =>
                {
                    Vector3 v = processor.Inputs[0].Vector3Out();
                    return new Color(v[_comp0], v[_comp1], v[_comp2], v[_comp3]);
                };
                processor.IntOut = () =>
                {
                    Vector3 v = processor.Inputs[0].Vector3Out();
                    return (int)v[_comp0];
                };
            }
            else if (input == ValueType.Vector4)
            {
                processor.FloatOut = () =>
                {
                    Vector4 v = processor.Inputs[0].Vector4Out();
                    return v[_comp0];
                };
                processor.Vector2Out = () =>
                {
                    Vector4 v = processor.Inputs[0].Vector4Out();
                    return new Vector2(v[_comp0], v[_comp1]);
                };
                processor.Vector3Out = () =>
                {
                    Vector4 v = processor.Inputs[0].Vector4Out();
                    return new Vector3(v[_comp0], v[_comp1], v[_comp2]);
                };
                processor.Vector4Out = () =>
                {
                    Vector4 v = processor.Inputs[0].Vector4Out();
                    return new Vector4(v[_comp0], v[_comp1], v[_comp2], v[_comp3]);
                };
                processor.ColorOut = () =>
                {
                    Vector4 v = processor.Inputs[0].Vector4Out();
                    return new Color(v[_comp0], v[_comp1], v[_comp2], v[_comp3]);
                };
                processor.IntOut = () =>
                {
                    Vector4 v = processor.Inputs[0].Vector4Out();
                    return (int)v[_comp0];
                };
            }
            else if (input == ValueType.Color)
            {
                processor.FloatOut = () =>
                {
                    Color v = processor.Inputs[0].ColorOut();
                    return v[_comp0];
                };
                processor.Vector2Out = () =>
                {
                    Color v = processor.Inputs[0].ColorOut();
                    return new Vector2(v[_comp0], v[_comp1]);
                };
                processor.Vector3Out = () =>
                {
                    Color v = processor.Inputs[0].ColorOut();
                    return new Vector3(v[_comp0], v[_comp1], v[_comp2]);
                };
                processor.Vector4Out = () =>
                {
                    Color v = processor.Inputs[0].ColorOut();
                    return new Vector4(v[_comp0], v[_comp1], v[_comp2], v[_comp3]);
                };
                processor.ColorOut = () =>
                {
                    Color v = processor.Inputs[0].ColorOut();
                    return new Color(v[_comp0], v[_comp1], v[_comp2], v[_comp3]);
                };
                processor.IntOut = () =>
                {
                    Color v = processor.Inputs[0].ColorOut();
                    return (int)v[_comp0];
                };
            }
            else if (input == ValueType.Int)
            {
                processor.FloatOut = () =>
                {
                    int v = processor.Inputs[0].IntOut();
                    return v;
                };
                processor.Vector2Out = () =>
                {
                    int v = processor.Inputs[0].IntOut();
                    return new Vector2(v, v);
                };
                processor.Vector3Out = () =>
                {
                    int v = processor.Inputs[0].IntOut();
                    return new Vector3(v, v, v);
                };
                processor.Vector4Out = () =>
                {
                    int v = processor.Inputs[0].IntOut();
                    return new Vector4(v, v, v, v);
                };
                processor.ColorOut = () =>
                {
                    int v = processor.Inputs[0].IntOut();
                    return new Color(v, v, v, v);
                };
                processor.IntOut = () =>
                {
                    int v = processor.Inputs[0].IntOut();
                    return (int)v;
                };
            }
        }

        static ValueType GetOutType(int comps, bool isColor)
        {
            switch (comps)
            {
                case 1:
                    return ValueType.Float;
                case 2:
                    return ValueType.Vector2;
                case 3:
                    return ValueType.Vector3;
                case 4:
                    return isColor ? ValueType.Color : ValueType.Vector4;
            }
            return ValueType.Error;
        }

        static int[] ValidateSwizzle(ref string swizzle, int max)
        {
            swizzle = swizzle.ToLower();
            List<char> chars = new List<char>(4);
            List<int> comps = new List<int>(4);
            for (int i = 0; i < swizzle.Length; i++)
            {
                char c = char.ToLower(swizzle[i]);
                int comp = ToComponent(c);
                comp = comp > max ? max : comp;
                if (comp == -1) continue;
                chars.Add(IsColor(c) ? ColorComponents[comp] : VectorComponents[comp]);
                comps.Add(comp);
                if (chars.Count == 4) break;
            }
            if (chars.Count == 0)
            {
                swizzle = "x";
                return new[] {0};
            }
            swizzle = new string(chars.ToArray());
            return comps.ToArray();
        }

        static readonly char[] VectorComponents = {'x', 'y', 'z', 'w'};
        static readonly char[] ColorComponents = {'r', 'g', 'b', 'a'};

        static bool IsColor(string swizzle)
        {
            int isColor = 0, isVector = 0;
            for (int i = 0; i < swizzle.Length; i++)
            {
                char c = char.ToLower(swizzle[i]);
                if (IsColor(c)) isColor++;
                else isVector++;
            }
            return isColor > isVector;
        }

        static bool IsColor(char letter)
        {
            switch (letter)
            {
                case 'x':
                case 'y':
                case 'z':
                case 'w':
                    return false;
                case 'r':
                case 'g':
                case 'b':
                case 'a':
                    return true;
            }
            return false;
        }

        static int GetComponentsCount(ValueType valueType)
        {
            switch (valueType)
            {
                case ValueType.Float:
                    return 1;
                case ValueType.Vector2:
                    return 2;
                case ValueType.Vector3:
                    return 3;
                case ValueType.Vector4:
                    return 4;
                case ValueType.Color:
                    return 4;
                case ValueType.Int:
                    return 1;
            }
            return -1;
        }

        static int ToComponent(char letter)
        {
            switch (letter)
            {
                case 'x':
                case 'r':
                    return 0;
                case 'y':
                case 'g':
                    return 1;
                case 'z':
                case 'b':
                    return 2;
                case 'w':
                case 'a':
                    return 3;
            }
            return -1;
        }
    }

}