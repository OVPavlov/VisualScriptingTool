using System;
using UnityEngine;

namespace NodeEditor
{

    [Serializable]
    public class InputNode: Node
    {
        public Component Component;
        public string Address;
        ReflectedValue _reflectedValue = new ReflectedValue();


        public override string GetPath()
        {
            return "Other/Input";
        }
        public InputNode()
        {
            Name = "Input";
            InitLinks(0);
            OutputType = ValueType.Float;
            DrawProperties = new[] {"Component", "Address"};
            NodeWidth = 9;
            CalcNodeHeight();
        }

        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            return GetMember();
        }

        ValueType GetMember()
        {
            bool found = _reflectedValue.FindMember(Component ? Component : null, Address);
            ValueType valueType;
            if (found && _reflectedValue.ValidateGet())
                valueType = NodeCollector.GetValueType(_reflectedValue.Type);
            else
                valueType = ValueType.Error;
            return valueType;
        }

        protected override void OnValidate()
        {
            CashedOutputType = OutputType = GetMember();
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            if (CashedOutputType != ValueType.Error)
            {
                if (CashedOutputType == ValueType.Float)
                    processor.FloatOut = _reflectedValue.CompileGetMethod<float>();
                if (CashedOutputType == ValueType.Vector2)
                    processor.Vector2Out = _reflectedValue.CompileGetMethod<Vector2>();
                if (CashedOutputType == ValueType.Vector3)
                    processor.Vector3Out = _reflectedValue.CompileGetMethod<Vector3>();
                if (CashedOutputType == ValueType.Vector4)
                    processor.Vector4Out = _reflectedValue.CompileGetMethod<Vector4>();
                if (CashedOutputType == ValueType.Color)
                    processor.ColorOut = _reflectedValue.CompileGetMethod<Color>();
                if (CashedOutputType == ValueType.Int)
                    processor.IntOut = _reflectedValue.CompileGetMethod<int>();
                if (CashedOutputType == ValueType.Bool)
                    processor.BoolOut = _reflectedValue.CompileGetMethod<bool>();
                if (CashedOutputType == ValueType.Texture2D)
                    processor.Texture2DOut = _reflectedValue.CompileGetMethod<Texture2D>();
                if (CashedOutputType == ValueType.RenderTexture)
                    processor.RenderTextureOut = _reflectedValue.CompileGetMethod<RenderTexture>();
            }
        }
    }

    [Serializable]
    public class OutputNode: Node
    {
        public Component Component;
        public string Address;
        ReflectedValue _reflectedValue = new ReflectedValue();

        public override string GetPath()
        {
            return "Other/Output";
        }
        public OutputNode()
        {
            Name = "Output";
            InitLinks(1);
            Inputs[0].Initialize(ValueType.None, "In", Link.Settings.None);
            DrawProperties = new[] {"Component", "Address"};
            OutputType = ValueType.None;
            NodeWidth = 9;
            CalcNodeHeight();
        }

        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            Inputs[0].Type = GetMember();
            return ValueType.None;
        }
        ValueType GetMember()
        {
            bool found = _reflectedValue.FindMember(Component ? Component : null, Address);
            ValueType valueType;
            if (found && _reflectedValue.ValidateSet())
                valueType = NodeCollector.GetValueType(_reflectedValue.Type);
            else
                valueType = ValueType.Error;
            return valueType;
        }

        protected override void OnValidate()
        {
            ValueType valueType = GetMember();
            Inputs[0].Type = valueType;
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            //ValueType valueType = GetMember();
            //Inputs[0].Type = valueType;
            ValueType valueType = Inputs[0].Type;
            if (valueType != ValueType.Error)
            {
                if (valueType == ValueType.Float)
                    processor.VoidOut = () => _reflectedValue.CompileSetMethod<float>()(processor.Inputs[0].FloatOut());
                if (valueType == ValueType.Vector2)
                    processor.VoidOut = () => _reflectedValue.CompileSetMethod<Vector2>()(processor.Inputs[0].Vector2Out());
                if (valueType == ValueType.Vector3)
                    processor.VoidOut = () => _reflectedValue.CompileSetMethod<Vector3>()(processor.Inputs[0].Vector3Out());
                if (valueType == ValueType.Vector4)
                    processor.VoidOut = () => _reflectedValue.CompileSetMethod<Vector4>()(processor.Inputs[0].Vector4Out());
                if (valueType == ValueType.Color)
                    processor.VoidOut = () => _reflectedValue.CompileSetMethod<Color>()(processor.Inputs[0].ColorOut());
                if (valueType == ValueType.Int)
                    processor.VoidOut = () => _reflectedValue.CompileSetMethod<int>()(processor.Inputs[0].IntOut());
                if (valueType == ValueType.Bool)
                    processor.VoidOut = () => _reflectedValue.CompileSetMethod<bool>()(processor.Inputs[0].BoolOut());
                if (valueType == ValueType.Texture2D)
                    processor.VoidOut = () => _reflectedValue.CompileSetMethod<Texture2D>()(processor.Inputs[0].Texture2DOut());
                if (valueType == ValueType.RenderTexture)
                    processor.VoidOut = () => _reflectedValue.CompileSetMethod<RenderTexture>()(processor.Inputs[0].RenderTextureOut());
            }
        }

    }

    [Serializable]
    public class SpringNode: Node
    {
        public float Hardness = 0.1f;
        public float Damping = 0.5f;

        float _damping;

        float _velocity;
        Vector2 _velocity2;
        Vector3 _velocity3;
        Vector4 _velocity4;

        float _current;
        Vector2 _current2;
        Vector3 _current3;
        Vector4 _current4;

        public override string GetPath()
        {
            return "Other/Spring";
        }
        public SpringNode()
        {
            Name = "Spring";
            InitLinks(1);
            Inputs[0].Initialize(ValueType.AutoValue, "Acceleration");
            OutputType = ValueType.AutoValue;
            DrawProperties = new[] {"Hardness", "Damping"};
            NodeWidth = 7;
            CalcNodeHeight();
        }

        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            switch (inTypes[0])
            {
                case ValueType.Float:
                case ValueType.Vector2:
                case ValueType.Vector3:
                case ValueType.Vector4:
                    return inTypes[0];
                default:
                    return ValueType.Error;
            }
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            Damping = Math.Max(Damping, float.Epsilon);
            _damping = 1f / (1f + Mathf.Pow(Damping, 5));
            processor.FloatOut = () =>
            {
                _velocity += processor.Inputs[0].FloatOut();
                _velocity -= _current * Hardness;
                _velocity *= Mathf.Pow(_damping, Parent.Dt);
                _current += _velocity;
                return _current;
            };
            processor.Vector2Out = () =>
            {
                _velocity2 += processor.Inputs[0].Vector2Out();
                _velocity2 -= _current2 * Hardness;
                _velocity2 *= Mathf.Pow(_damping, Parent.Dt);
                _current2 += _velocity2;
                return _current2;
            };
            processor.Vector3Out = () =>
            {
                _velocity3 += processor.Inputs[0].Vector3Out();
                _velocity3 -= _current3 * Hardness;
                _velocity3 *= Mathf.Pow(_damping, Parent.Dt);
                _current3 += _velocity3;
                return _current3;
            };
            processor.Vector4Out = () =>
            {
                _velocity4 += processor.Inputs[0].Vector4Out();
                _velocity4 -= _current4 * Hardness;
                _velocity4 *= Mathf.Pow(_damping, Parent.Dt);
                _current4 += _velocity4;
                return _current4;
            };
        }
    }

    [Serializable]
    public class DeltaNode: Node
    {
        float _lastVal;
        Vector2 _lastVal2;
        Vector3 _lastVal3;
        Vector4 _lastVal4;

        public override string GetPath()
        {
            return "Other/Delta";
        }
        public DeltaNode()
        {
            Name = "Delta";
            InitLinks(1);
            Inputs[0].Initialize(ValueType.AutoValue, "Val");
            OutputType = ValueType.AutoValue;
            CalcNodeHeight();
        }

        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            switch (inTypes[0])
            {
                case ValueType.Float:
                case ValueType.Vector2:
                case ValueType.Vector3:
                case ValueType.Vector4:
                    return inTypes[0];
                default:
                    return ValueType.Error;
            }
        }


        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            processor.FloatOut = () =>
            {
                float val = processor.Inputs[0].FloatOut();
                float delta = val - _lastVal;
                _lastVal = val;
                return delta;
            };
            processor.Vector2Out = () =>
            {
                Vector2 val2 = processor.Inputs[0].Vector2Out();
                Vector2 delta2 = val2 - _lastVal2;
                _lastVal2 = val2;
                return delta2;
            };
            processor.Vector3Out = () =>
            {
                Vector3 val3 = processor.Inputs[0].Vector3Out();
                Vector3 delta3 = val3 - _lastVal3;
                _lastVal3 = val3;
                return delta3;
            };
            processor.Vector4Out = () =>
            {
                Vector4 val4 = processor.Inputs[0].Vector4Out();
                Vector4 delta4 = val4 - _lastVal4;
                _lastVal4 = val4;
                return delta4;
            };
        }
    }
    
    [Serializable]
    public class DozerNode: Node
    {
        public bool Freeze;
        float _lastVal;
        Vector2 _lastVal2;
        Vector3 _lastVal3;
        Vector4 _lastVal4;
        Color _lastValC;

        public override string GetPath()
        {
            return "Other/Dozer";
        }
        public DozerNode()
        {
            Name = "Dozer";
            InitLinks(2);
            Inputs[0].Initialize(ValueType.AutoValue, "Val");
            Inputs[1].Initialize(ValueType.Bool, "Update");
            DrawProperties = new[] {"Freeze"};
            OutputType = ValueType.AutoValue;
            CalcNodeHeight();
        }

        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            if (inTypes[1] != ValueType.Bool) return ValueType.Error;
            switch (inTypes[0])
            {
                case ValueType.Float:
                case ValueType.Vector2:
                case ValueType.Vector3:
                case ValueType.Vector4:
                case ValueType.Color:
                    return inTypes[0];
                default:
                    return ValueType.Error;
            }
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            processor.FloatOut = () =>
            {
                if (processor.Inputs[1].BoolOut())
                    _lastVal = processor.Inputs[0].FloatOut();
                else if (!Freeze)
                    _lastVal = 0;
                return _lastVal;

            };
            processor.Vector2Out = () =>
            {
                if (processor.Inputs[1].BoolOut())
                    _lastVal2 = processor.Inputs[0].Vector2Out();
                else if (!Freeze)
                    _lastVal2 = new Vector2();
                return _lastVal2;
            };
            processor.Vector3Out = () =>
            {
                if (processor.Inputs[1].BoolOut())
                    _lastVal3 = processor.Inputs[0].Vector3Out();
                else if (!Freeze)
                    _lastVal3 = new Vector3();
                return _lastVal3;
            };
            processor.Vector4Out = () =>
            {
                if (processor.Inputs[1].BoolOut())
                    _lastVal4 = processor.Inputs[0].Vector4Out();
                else if (!Freeze)
                    _lastVal4 = new Vector4();
                return _lastVal4;
            }; processor.ColorOut = () =>
            {
                if (processor.Inputs[1].BoolOut())
                    _lastValC = processor.Inputs[0].ColorOut();
                else if (!Freeze)
                    _lastValC = new Color();
                return _lastValC;
            };
        }
    }
    
    [Serializable]
    public class TimerNode: Node
    {
        public LimitType LType;
        public float TimeSpeed = 1f;
        public float Limit = 1f;
        public bool Inverse;
        public bool To01;

        float _time;

        public override string GetPath()
        {
            return "Other/Timer";
        }
        public TimerNode()
        {
            Name = "Timer";
            InitLinks(1);
            Inputs[0].Initialize(ValueType.Bool, "Reset");
            DrawProperties = new[] {"LType", "TimeSpeed", "Limit", "Inverse", "To01"};
            OutputType = ValueType.Float;
            CalcNodeHeight();
        }

        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            if (inTypes[0] != ValueType.Bool) return ValueType.Error;
            return ValueType.Float;
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            processor.FloatOut = () =>
            {
                _time += Parent.Dt * TimeSpeed;
                if (processor.Inputs[0].BoolOut())
                    _time = 0f;

                float val = _time;
                if (_time > Limit)
                {
                    switch (LType)
                    {
                        case LimitType.Repeat:
                            val = Mathf.Repeat(_time, Limit);
                            break;
                        case LimitType.StayToStart:
                            val = 0f;
                            break;
                        case LimitType.StayToEnd:
                            val = Limit;
                            break;
                        case LimitType.PingPong:
                            val = Mathf.PingPong(_time, Limit);
                            break;
                        case LimitType.PingPongOnce:
                            val = Mathf.Max(0, Limit - Mathf.Abs(_time - Limit));
                            break;
                        default:
                            val = _time;
                            break;
                    }
                }

                if (Inverse) val = Limit - val;
                if (To01) val /= Limit;
                return val;
            };
        }


        public enum LimitType
        {
            None,
            Repeat,
            StayToStart,
            StayToEnd,
            PingPong,
            PingPongOnce,
        }

    }
}