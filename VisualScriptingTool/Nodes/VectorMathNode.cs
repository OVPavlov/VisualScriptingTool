using System;
using UnityEngine;

namespace NodeEditor
{
    [Serializable]
    public class VectorMathNode : Node
    {
        public OperationType _Type;
        public override string GetPath()
        {
            return "Operations/Vector Math";
        }
        public VectorMathNode()
        {
            Name = "Vector Math";
            InitLinks(2);
            Inputs[0].Initialize(ValueType.AutoValue, "A");
            Inputs[1].Initialize(ValueType.AutoValue, "B");
            DrawProperties = new[] { "_Type" };
            OutputType = ValueType.AutoValue;
            NodeWidth = 6;
            CalcNodeHeight();
        }
        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            switch (_Type)
            {
                case OperationType.Dot:
                {
                    ValueType t0 = inTypes[0];
                    ValueType t1 = inTypes[1];
                    return InputVector(InputSame(t0, t1)) != ValueType.Error ? ValueType.Float : ValueType.Error;
                }
                case OperationType.Normalize:
                {
                    return InputVector(inTypes[0]);
                }
                case OperationType.Length:
                {
                    return InputVector(inTypes[0]) != ValueType.Error ? ValueType.Float : ValueType.Error;
                }
                case OperationType.Distance:
                {
                    ValueType t0 = inTypes[0];
                    ValueType t1 = inTypes[1];
                    return InputVector(InputSame(t0, t1)) != ValueType.Error ? ValueType.Float : ValueType.Error;
                }
                case OperationType.Cross:
                {
                    ValueType t0 = inTypes[0];
                    ValueType t1 = inTypes[1];
                    return InputSame(t0, t1) == ValueType.Vector3 ? ValueType.Vector3 : ValueType.Error;
                }
                case OperationType.Project:
                {
                    ValueType t0 = inTypes[0];
                    ValueType t1 = inTypes[1];
                    ValueType t = InputSame(t0, t1);
                    return (t == ValueType.Vector3 || t == ValueType.Vector4) ? t : ValueType.Error;
                }
                case OperationType.ProjectOnPlane:
                {
                    ValueType t0 = inTypes[0];
                    ValueType t1 = inTypes[1];
                    return InputSame(t0, t1) == ValueType.Vector3 ? ValueType.Vector3 : ValueType.Error;
                }
                case OperationType.Reflect:
                {
                    ValueType t0 = inTypes[0];
                    ValueType t1 = inTypes[1];
                    ValueType t = InputSame(t0, t1);
                    return (t == ValueType.Vector3 || t == ValueType.Vector2) ? t : ValueType.Error;
                }
            }
            return ValueType.Error;
        }

        protected override void OnValidate()
        {
            switch (_Type)
            {
                case OperationType.Dot:
                case OperationType.Distance:
                case OperationType.Cross:
                {
                    InitLinks(2);
                    Inputs[0].Initialize(ValueType.AutoVector, "A");
                    Inputs[1].Initialize(ValueType.AutoVector, "B");
                }
                    break;
                case OperationType.Normalize:
                case OperationType.Length:
                {
                    InitLinks(1);
                    Inputs[0].Initialize(ValueType.AutoVector, "In");
                }
                    break;
                case OperationType.Project:
                case OperationType.ProjectOnPlane:
                case OperationType.Reflect:
                {

                    InitLinks(2);
                    Inputs[0].Initialize(ValueType.AutoVector, "Vector");
                    Inputs[1].Initialize(ValueType.AutoVector, "Normal");
                }
                    break;
            }
            CalcNodeHeight();
        }

        static float ColorDot(Color a, Color b)
        {
            return a.r * b.r + a.g * b.g + a.b * b.b + a.a * b.a;
        }
        static float ColorLength(Color a)
        {
            return Mathf.Sqrt(ColorDot(a, a));
        }
        static Color ColorNormalized(Color a)
        {
            float length = ColorLength(a);
            return a / length;
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            switch (_Type)
            {
                case OperationType.Dot:
                {
                    ValueType t0 = Parent.GetNode(Inputs[0]).CashedOutputType;
                    if (t0 == ValueType.Vector2)
                    {
                        processor.FloatOut = () =>
                        {
                            Vector2 a = processor.Inputs[0].Vector2Out();
                            Vector2 b = processor.Inputs[1].Vector2Out();
                            return Vector2.Dot(a, b);
                        };
                    }
                    if (t0 == ValueType.Vector3)
                    {
                        processor.FloatOut = () =>
                        {
                            Vector3 a = processor.Inputs[0].Vector3Out();
                            Vector3 b = processor.Inputs[1].Vector3Out();
                            return Vector3.Dot(a, b);
                        };
                    }
                    if (t0 == ValueType.Vector4)
                    {
                        processor.FloatOut = () =>
                        {
                            Vector4 a = processor.Inputs[0].Vector4Out();
                            Vector4 b = processor.Inputs[1].Vector4Out();
                            return Vector4.Dot(a, b);
                        };
                    }
                    if (t0 == ValueType.Color)
                    {
                        processor.FloatOut = () =>
                        {
                            Color a = processor.Inputs[0].ColorOut();
                            Color b = processor.Inputs[1].ColorOut();
                            return ColorDot(a, b);
                        };
                    }
                }
                    break;
                case OperationType.Normalize:
                {
                    processor.Vector2Out = () =>
                    {
                        Vector2 a = processor.Inputs[0].Vector2Out();
                        return a.normalized;
                    };
                    processor.Vector3Out = () =>
                    {
                        Vector3 a = processor.Inputs[0].Vector3Out();
                        return a.normalized;
                    };
                    processor.Vector4Out = () =>
                    {
                        Vector4 a = processor.Inputs[0].Vector4Out();
                        return a.normalized;
                    };
                    processor.ColorOut = () =>
                    {
                        Color a = processor.Inputs[0].ColorOut();
                        return ColorNormalized(a);
                    };
                }
                    break;
                case OperationType.Length:
                {
                    ValueType t0 = Parent.GetNode(Inputs[0]).CashedOutputType;
                    if (t0 == ValueType.Vector2)
                    {
                        processor.FloatOut = () =>
                        {
                            Vector2 a = processor.Inputs[0].Vector2Out();
                            return a.magnitude;
                        };
                    }
                    if (t0 == ValueType.Vector3)
                    {
                        processor.FloatOut = () =>
                        {
                            Vector3 a = processor.Inputs[0].Vector3Out();
                            return a.magnitude;
                        };
                    }
                    if (t0 == ValueType.Vector4)
                    {
                        processor.FloatOut = () =>
                        {
                            Vector4 a = processor.Inputs[0].Vector4Out();
                            return a.magnitude;
                        };
                    }
                    if (t0 == ValueType.Color)
                    {
                        processor.FloatOut = () =>
                        {
                            Color a = processor.Inputs[0].ColorOut();
                            return ColorLength(a);
                        };
                    }
                }
                    break;
                case OperationType.Distance:
                {
                    ValueType t0 = Parent.GetNode(Inputs[0]).CashedOutputType;
                    if (t0 == ValueType.Vector2)
                    {
                        processor.FloatOut = () =>
                        {
                            Vector2 a = processor.Inputs[0].Vector2Out();
                            Vector2 b = processor.Inputs[1].Vector2Out();
                            return Vector2.Distance(a, b);
                        };
                    }
                    if (t0 == ValueType.Vector3)
                    {
                        processor.FloatOut = () =>
                        {
                            Vector3 a = processor.Inputs[0].Vector3Out();
                            Vector3 b = processor.Inputs[1].Vector3Out();
                            return Vector3.Distance(a, b);
                        };
                    }
                    if (t0 == ValueType.Vector4)
                    {
                        processor.FloatOut = () =>
                        {
                            Vector4 a = processor.Inputs[0].Vector4Out();
                            Vector4 b = processor.Inputs[1].Vector4Out();
                            return Vector4.Distance(a, b);
                        };
                    }
                    if (t0 == ValueType.Color)
                    {
                        processor.FloatOut = () =>
                        {
                            Color a = processor.Inputs[0].ColorOut();
                            Color b = processor.Inputs[1].ColorOut();
                            return ColorLength(a - b);
                        };
                    }
                }
                    break;
                case OperationType.Cross:
                {

                    processor.Vector3Out = () =>
                    {
                        Vector3 a = processor.Inputs[0].Vector3Out();
                        Vector3 b = processor.Inputs[1].Vector3Out();
                        return Vector3.Cross(a, b);
                    };

                }
                    break;
                case OperationType.Project:
                {
                    processor.Vector3Out = () =>
                    {
                        Vector3 a = processor.Inputs[0].Vector3Out();
                        Vector3 b = processor.Inputs[1].Vector3Out();
                        return Vector3.Project(a, b);
                    };

                    processor.Vector4Out = () =>
                    {
                        Vector4 a = processor.Inputs[0].Vector4Out();
                        Vector4 b = processor.Inputs[1].Vector4Out();
                        return Vector4.Project(a, b);
                    };
                }
                    break;
                case OperationType.ProjectOnPlane:
                {
                    processor.Vector3Out = () =>
                    {
                        Vector3 a = processor.Inputs[0].Vector3Out();
                        Vector3 b = processor.Inputs[1].Vector3Out();
                        return Vector3.ProjectOnPlane(a, b);
                    };
                }
                    break;
                case OperationType.Reflect:
                {
                    processor.Vector2Out = () =>
                    {
                        Vector2 a = processor.Inputs[0].Vector2Out();
                        Vector2 b = processor.Inputs[1].Vector2Out();
                        return Vector2.Reflect(a, b);
                    };

                    processor.Vector3Out = () =>
                    {
                        Vector3 a = processor.Inputs[0].Vector3Out();
                        Vector3 b = processor.Inputs[1].Vector3Out();
                        return Vector3.Reflect(a, b);
                    };
                }
                    break;
            }
        }

        public enum OperationType
        {
            Dot,
            Normalize,
            Length,
            Distance,
            Cross,
            Project,
            ProjectOnPlane,
            Reflect
        }
    }
}