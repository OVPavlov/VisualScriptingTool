using System;
using UnityEngine;

namespace NodeEditor
{
    [Serializable]
    public class QuaternionMathNode: Node
    {
        public OperationType _Type;
        public override string GetPath()
        {
            return "Operations/Quaternion Math";
        }
        public QuaternionMathNode()
        {
            Name = "Quaternion Math";
            InitLinks(2);
            Inputs[0].Initialize(ValueType.AutoValue, "A");
            Inputs[1].Initialize(ValueType.AutoValue, "B");
            DrawProperties = new[] {"_Type"};
            OutputType = ValueType.AutoValue;
            NodeWidth = 6;
            CalcNodeHeight();
        }
        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            switch (_Type)
            {
                case OperationType.Multyply:
                {
                    ValueType t0 = inTypes[0];
                    ValueType t1 = inTypes[1];
                    if (t0 == ValueType.Vector4 &&
                        (t1 == ValueType.Vector2 || t1 == ValueType.Vector3 || t1 == ValueType.Vector4))
                        return t1;
                    if (t1 == ValueType.Vector4 &&
                        (t0 == ValueType.Vector2 || t0 == ValueType.Vector3 || t0 == ValueType.Vector4))
                        return t0;
                }
                    break;
                case OperationType.AxisAngle:
                {
                    ValueType t0 = inTypes[0];
                    ValueType t1 = inTypes[1];
                    if (t0 == ValueType.Float && t1 == ValueType.Vector3) return ValueType.Vector4;
                }
                    break;
                case OperationType.FormEuler:
                {
                    ValueType t0 = inTypes[0];
                    if (t0 == ValueType.Vector3) return ValueType.Vector4;
                }
                    break;
                case OperationType.ToEuler:
                {
                    ValueType t0 = inTypes[0];
                    if (t0 == ValueType.Vector4) return ValueType.Vector3;
                }
                    break;
                case OperationType.FromToRotation:
                {
                    ValueType t0 = inTypes[0];
                    ValueType t1 = inTypes[1];
                    if (t0 == ValueType.Vector3 && t1 == ValueType.Vector3) return ValueType.Vector4;
                }
                    break;
                case OperationType.Inverse:
                {
                    ValueType t0 = inTypes[0];
                    if (t0 == ValueType.Vector4) return ValueType.Vector4;
                }
                    break;
                case OperationType.LerpUnclamped:
                case OperationType.SlerpUnclamped:
                {
                    ValueType t0 = inTypes[0];
                    ValueType t1 = inTypes[1];
                    ValueType t2 = inTypes[2];
                    if (t0 == ValueType.Vector4 && t1 == ValueType.Vector4 && t2 == ValueType.Float) return ValueType.Vector4;
                }
                    break;
                case OperationType.LookRotation:
                {
                    ValueType t0 = inTypes[0];
                    ValueType t1 = inTypes[1];
                    if (t0 == ValueType.Vector3 && (t1 == ValueType.Vector3 || t1 == ValueType.None)) return ValueType.Vector4;
                }
                    break;
            }
            return ValueType.Error;
        }

        protected override void OnValidate()
        {
            switch (_Type)
            {
                case OperationType.Multyply:
                {
                    InitLinks(2);
                    Inputs[0].Initialize(ValueType.AutoVector, "A");
                    Inputs[1].Initialize(ValueType.AutoVector, "B");
                }
                    break;
                case OperationType.AxisAngle:
                {
                    InitLinks(2);
                    Inputs[0].Initialize(ValueType.Float, "Angle");
                    Inputs[1].Initialize(ValueType.Vector3, "Axis");
                }
                    break;
                case OperationType.FormEuler:
                {
                    InitLinks(1);
                    Inputs[0].Initialize(ValueType.Vector3, "Euler");
                }
                    break;
                case OperationType.ToEuler:
                {
                    InitLinks(1);
                    Inputs[0].Initialize(ValueType.Vector4, "Quaternion");
                }
                    break;
                case OperationType.FromToRotation:
                {
                    InitLinks(2);
                    Inputs[0].Initialize(ValueType.Vector3, "From");
                    Inputs[1].Initialize(ValueType.Vector3, "To");
                }
                    break;
                case OperationType.Inverse:
                {
                    InitLinks(1);
                    Inputs[0].Initialize(ValueType.Vector4, "In");
                }
                    break;
                case OperationType.LerpUnclamped:
                case OperationType.SlerpUnclamped:
                {
                    InitLinks(3);
                    Inputs[0].Initialize(ValueType.Vector4, "a");
                    Inputs[1].Initialize(ValueType.Vector4, "b");
                    Inputs[2].Initialize(ValueType.Float, "t");
                }
                    break;
                case OperationType.LookRotation:
                {
                    InitLinks(2);
                    Inputs[0].Initialize(ValueType.Vector3, "Forward");
                    Inputs[1].Initialize(ValueType.Vector3, "Upward");
                }
                    break;
            }

            CalcNodeHeight();
        }

        static Vector4 ToV(Quaternion q)
        {
            return new Vector4(q.x, q.y, q.z, q.w);
        }
        static Quaternion ToQ(Vector4 v)
        {
            return new Quaternion(v.x, v.y, v.z, v.w);
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            switch (_Type)
            {
                case OperationType.Multyply:
                {
                    ValueType t0 = Parent.GetNode(Inputs[0]).CashedOutputType;
                    ValueType t1 = Parent.GetNode(Inputs[1]).CashedOutputType;
                    if (t0 == ValueType.Vector3 && t1 == ValueType.Vector4)
                    {
                        processor.Vector3Out = () =>
                        {
                            Vector4 q = processor.Inputs[1].Vector4Out();
                            Vector3 v = processor.Inputs[0].Vector3Out();
                            return ToQ(q) * v;
                        };
                    }
                    if (t0 == ValueType.Vector2 && t1 == ValueType.Vector4)
                    {
                        processor.Vector2Out = () =>
                        {
                            Vector4 q = processor.Inputs[1].Vector4Out();
                            Vector2 v = processor.Inputs[0].Vector2Out();
                            return ToQ(q) * v;
                        };
                    }
                    if (t0 == ValueType.Vector4 && t1 == ValueType.Vector3)
                    {
                        processor.Vector3Out = () =>
                        {
                            Vector4 q = processor.Inputs[0].Vector4Out();
                            Vector3 v = processor.Inputs[1].Vector3Out();
                            return ToQ(q) * v;
                        };
                    }
                    if (t0 == ValueType.Vector4 && t1 == ValueType.Vector2)
                    {
                        processor.Vector2Out = () =>
                        {
                            Vector4 q = processor.Inputs[0].Vector4Out();
                            Vector2 v = processor.Inputs[1].Vector2Out();
                            return ToQ(q) * v;
                        };
                    }
                    if (t0 == ValueType.Vector4 && t1 == ValueType.Vector4)
                    {
                        processor.Vector4Out = () =>
                        {
                            Vector4 q0 = processor.Inputs[0].Vector4Out();
                            Vector4 q1 = processor.Inputs[1].Vector4Out();
                            return ToV(ToQ(q0) * ToQ(q1));
                        };
                    }
                }
                    break;
                case OperationType.AxisAngle:
                {
                    processor.Vector4Out = () =>
                    {
                        float a = processor.Inputs[0].FloatOut();
                        Vector3 b = processor.Inputs[1].Vector3Out();
                        return ToV(Quaternion.AngleAxis(a, b));
                    };
                }
                    break;

                case OperationType.FormEuler:
                {
                    processor.Vector4Out = () =>
                    {
                        Vector3 a = processor.Inputs[0].Vector3Out();
                        return ToV(Quaternion.Euler(a));
                    };
                }
                    break;
                case OperationType.ToEuler:
                {
                    processor.Vector3Out = () =>
                    {
                        Vector4 a = processor.Inputs[0].Vector4Out();
                        return ToQ(a).eulerAngles;
                    };
                }
                    break;
                case OperationType.FromToRotation:
                {
                    processor.Vector4Out = () =>
                    {
                        Vector3 a = processor.Inputs[0].Vector3Out();
                        Vector3 b = processor.Inputs[1].Vector3Out();
                        return ToV(Quaternion.FromToRotation(a, b));
                    };
                }
                    break;
                case OperationType.Inverse:
                {
                    processor.Vector4Out = () =>
                    {
                        Vector4 a = processor.Inputs[0].Vector4Out();
                        return ToV(Quaternion.Inverse(ToQ(a)));
                    };
                }
                    break;
                case OperationType.LerpUnclamped:
                {
                    processor.Vector4Out = () =>
                    {
                        Vector4 a = processor.Inputs[0].Vector4Out();
                        Vector4 b = processor.Inputs[1].Vector4Out();
                        float t = processor.Inputs[2].FloatOut();
                        return ToV(Quaternion.LerpUnclamped(ToQ(a), ToQ(b), t));
                    };
                    break;
                }
                case OperationType.SlerpUnclamped:
                {
                    processor.Vector4Out = () =>
                    {
                        Vector4 a = processor.Inputs[0].Vector4Out();
                        Vector4 b = processor.Inputs[1].Vector4Out();
                        float t = processor.Inputs[2].FloatOut();
                        return ToV(Quaternion.SlerpUnclamped(ToQ(a), ToQ(b), t));
                    };
                }
                    break;
                case OperationType.LookRotation:
                {
                    if (Inputs[1].Type == ValueType.None)
                        processor.Vector4Out = () =>
                        {
                            Vector4 a = processor.Inputs[0].Vector4Out();
                            return ToV(Quaternion.LookRotation(a));
                        };
                    else
                        processor.Vector4Out = () =>
                        {
                            Vector4 a = processor.Inputs[0].Vector4Out();
                            Vector4 b = processor.Inputs[1].Vector4Out();
                            return ToV(Quaternion.LookRotation(a, b));
                        };
                }
                    break;
            }
        }

        public enum OperationType
        {
            Multyply,
            FormEuler,
            ToEuler,
            FromToRotation,
            AxisAngle,
            Inverse,
            LerpUnclamped,
            SlerpUnclamped,
            LookRotation,
        }
    }

}