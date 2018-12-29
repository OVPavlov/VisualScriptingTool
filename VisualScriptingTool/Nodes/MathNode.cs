using System;
using UnityEngine;

namespace NodeEditor
{
    [Serializable]
    public class MathNode: Node
    {
        public OperationType _Type;
        Func<float, float> OperationOneInput;
        Func<float, float, float> OperationTwoInput;
        Func<int, int> OperationOneInputInt;
        Func<int, int, int> OperationTwoInputInt;
        public override string GetPath()
        {
            return "Operations/Math";
        }
        public MathNode()
        {
            Name = "Math";
            InitLinks(2);
            Inputs[0].Initialize(ValueType.AutoValue, "A");
            Inputs[1].Initialize(ValueType.AutoValue, "B");
            DrawProperties = new[] {"_Type"};
            OutputType = ValueType.AutoValue;
            NodeWidth = 5;
            CalcNodeHeight();
        }
        protected override ValueType CheckInputsAndGetType(ValueType[] inTypes)
        {
            int inputsCount = OperationConfigs[(int)_Type].InputCount;
            if (inputsCount == 2)
            {
                ValueType fi = InputFloatInt(inTypes[0], inTypes[1]);
                if (fi != ValueType.Error) return fi;
                return InputVectorFloatInt(inTypes[0], inTypes[1]);
            }
            else
            {
                return InputValue(inTypes[0]);
            }
        }

        protected override void OnValidate()
        {
            int inputsCount = OperationConfigs[(int)_Type].InputCount;
            if (Inputs.Length != inputsCount)
            {
                InitLinks(inputsCount);
                Inputs[0].Initialize(ValueType.AutoValue, "A");
                if (inputsCount == 2)
                    Inputs[1].Initialize(ValueType.AutoValue, "B");
            }
            CalcNodeHeight();
        }

        static string GenerateInitializerTwoInput()
        {
            string funcStruct = @"
    void GeneratedProcessorInitializerTwoInput(NodeProcessor processor, ValueTypeConfig aConfig, ValueTypeConfig bConfig)
    {
        if (aConfig.IsVector && bConfig.IsVector)
        {";
            foreach (ValueType vector in Vectors)
            {
                funcStruct += FunctionGenerator.GenerateAnonymousFuncForNodeProcessor(new[] {vector, vector}, vector, "OperationTwoInput({0}, {1})", 3);
            }

            funcStruct += @"
        }
        else if (aConfig.IsVector && !bConfig.IsVector)
        {
            if (bConfig.ValueType == ValueType.Float)
            {";
            foreach (ValueType vector in Vectors)
            {
                funcStruct += FunctionGenerator.GenerateAnonymousFuncForNodeProcessor(new[] {vector, ValueType.Float}, vector, "OperationTwoInput({0}, {1})", 3);
            }
            funcStruct += @"
            }
            else
            {";
            foreach (ValueType vector in Vectors)
            {
                funcStruct += FunctionGenerator.GenerateAnonymousFuncForNodeProcessor(new[] { vector, ValueType.Int }, vector, "OperationTwoInput({0}, {1})", 3);
            }
            funcStruct += @"
            }
        }
        else if (!aConfig.IsVector && bConfig.IsVector)
        {
            if (aConfig.ValueType == ValueType.Float)
            {";
            foreach (ValueType vector in Vectors)
            {
                funcStruct += FunctionGenerator.GenerateAnonymousFuncForNodeProcessor(new[] {ValueType.Float, vector}, vector, "OperationTwoInput({0}, {1})", 3);
            }
            funcStruct += @"
            }
            else
            {";
            foreach (ValueType vector in Vectors)
            {
                funcStruct += FunctionGenerator.GenerateAnonymousFuncForNodeProcessor(new[] { ValueType.Int, vector }, vector, "OperationTwoInput({0}, {1})", 3);
            }
            funcStruct += @"
            }
        }
        else if (!aConfig.IsVector && !bConfig.IsVector)
        {
            if (aConfig.ValueType == ValueType.Float)
            {
                if (bConfig.ValueType == ValueType.Float)
                {";
            funcStruct += FunctionGenerator.GenerateAnonymousFuncForNodeProcessor(new[] {ValueType.Float, ValueType.Float}, ValueType.Float, "OperationTwoInput({0}, {1})", 3);
            funcStruct += @"
                }
                else
                {";
            funcStruct += FunctionGenerator.GenerateAnonymousFuncForNodeProcessor(new[] { ValueType.Float, ValueType.Int }, ValueType.Float, "OperationTwoInput({0}, {1})", 3);
            funcStruct += @"
                }
            }
            else
            {
                if (bConfig.ValueType == ValueType.Float)
                {";
            funcStruct += FunctionGenerator.GenerateAnonymousFuncForNodeProcessor(new[] { ValueType.Int, ValueType.Float }, ValueType.Float, "OperationTwoInput({0}, {1})", 3);
            funcStruct += @"
                }
                else
                {";
            funcStruct += FunctionGenerator.GenerateAnonymousFuncForNodeProcessor(new[] { ValueType.Int, ValueType.Int }, ValueType.Int, "OperationTwoInputInt({0}, {1})", 3);
            funcStruct += @"
                }
            }
        }
    }
";
            return funcStruct;
        }

        static string GenerateInitializerOneInput()
        {
            string funcStruct = @"
    void GeneratedProcessorInitializerOneInput(NodeProcessor processor)
    {";
            foreach (ValueType vector in Vectors)
            {
                funcStruct += FunctionGenerator.GenerateAnonymousFuncForNodeProcessor(new[] {vector}, vector, "OperationOneInput({0})", 3);
            }
            funcStruct += FunctionGenerator.GenerateAnonymousFuncForNodeProcessor(new[] {ValueType.Float}, ValueType.Float, "OperationOneInput({0})", 3);

            funcStruct += FunctionGenerator.GenerateAnonymousFuncForNodeProcessor(new[] {ValueType.Int}, ValueType.Int, "OperationOneInputInt({0})", 3);
            funcStruct += @"
    }
";
            return funcStruct;
        }

        static string GenerateOperations()
        {
            string funcStruct = @"
    void GeneratedOperationInitializer(OperationType type)
    {
        switch (type)
        {";
            foreach (OperationConfig operation in OperationConfigs)
            {
                string operationFunc = string.Format(operation.Format, "a", "b");

                string simplifiedAccessorTwo = "(a, b) =>" + operationFunc;
                string simplifiedAccessorOne = "a =>" + operationFunc;

                int bracets = operationFunc.Split('(').Length - 1;
                if (bracets == 1)
                {
                    string cuttedOperationFunc = operationFunc.Remove(operationFunc.IndexOf("("));
                    simplifiedAccessorTwo = cuttedOperationFunc;
                    simplifiedAccessorOne = cuttedOperationFunc;
                }


                if (operation.InputCount == 2)
                {

                    funcStruct += string.Format(@"
            case OperationType.{0}:
                OperationTwoInput = {2};
                OperationTwoInputInt = (a, b) =>(int){1};
                break;",
                        operation.OperationType, operationFunc, simplifiedAccessorTwo);
                }
                else
                {
                    funcStruct += string.Format(@"
            case OperationType.{0}:
                OperationOneInput = {2};
                OperationOneInputInt = a =>(int){1};
                break;
", operation.OperationType, operationFunc, simplifiedAccessorOne);
                }

            }
            funcStruct += @"
        } 
    }";

            return funcStruct;
        }




        void GeneratedOperationInitializer(OperationType type)
        {
            switch (type)
            {
                case OperationType.Multyply:
                    OperationTwoInput = (a, b) => a * b;
                    OperationTwoInputInt = (a, b) => (int)a * b;
                    break;

                case OperationType.Divide:
                    OperationTwoInput = (a, b) => a / b;
                    OperationTwoInputInt = (a, b) => (int)a / b;
                    break;

                case OperationType.Add:
                    OperationTwoInput = (a, b) => a + b;
                    OperationTwoInputInt = (a, b) => (int)a + b;
                    break;

                case OperationType.Subtract:
                    OperationTwoInput = (a, b) => a - b;
                    OperationTwoInputInt = (a, b) => (int)a - b;
                    break;

                case OperationType.Negative:
                    OperationOneInput = a => -a;
                    OperationOneInputInt = a => (int)-a;
                    break;

                case OperationType.Log:
                    OperationTwoInput = Mathf.Log;
                    OperationTwoInputInt = (a, b) => (int)Mathf.Log(a, b);
                    break;

                case OperationType.Pow:
                    OperationTwoInput = Mathf.Pow;
                    OperationTwoInputInt = (a, b) => (int)Mathf.Pow(a, b);
                    break;

                case OperationType.Sqrt:
                    OperationOneInput = Mathf.Sqrt;
                    OperationOneInputInt = a => (int)Mathf.Sqrt(a);
                    break;

                case OperationType.Exp:
                    OperationOneInput = Mathf.Exp;
                    OperationOneInputInt = a => (int)Mathf.Exp(a);
                    break;

                case OperationType.ClosestPowerOfTwo:
                    OperationOneInput = a => Mathf.ClosestPowerOfTwo((int)a);
                    OperationOneInputInt = a => (int)Mathf.ClosestPowerOfTwo((int)a);
                    break;

                case OperationType.PerlinNoise:
                    OperationTwoInput = Mathf.PerlinNoise;
                    OperationTwoInputInt = (a, b) => (int)Mathf.PerlinNoise(a, b);
                    break;

                case OperationType.Repeat:
                    OperationTwoInput = Mathf.Repeat;
                    OperationTwoInputInt = (a, b) => (int)Mathf.Repeat(a, b);
                    break;

                case OperationType.PingPong:
                    OperationTwoInput = Mathf.PingPong;
                    OperationTwoInputInt = (a, b) => (int)Mathf.PingPong(a, b);
                    break;

                case OperationType.Abs:
                    OperationOneInput = Mathf.Abs;
                    OperationOneInputInt = a => (int)Mathf.Abs(a);
                    break;

                case OperationType.Sign:
                    OperationOneInput = Mathf.Sign;
                    OperationOneInputInt = a => (int)Mathf.Sign(a);
                    break;

                case OperationType.Round:
                    OperationOneInput = Mathf.Round;
                    OperationOneInputInt = a => (int)Mathf.Round(a);
                    break;

                case OperationType.Floor:
                    OperationOneInput = Mathf.Floor;
                    OperationOneInputInt = a => (int)Mathf.Floor(a);
                    break;

                case OperationType.Ceil:
                    OperationOneInput = Mathf.Ceil;
                    OperationOneInputInt = a => (int)Mathf.Ceil(a);
                    break;

                case OperationType.Max:
                    OperationTwoInput = Mathf.Max;
                    OperationTwoInputInt = (a, b) => (int)Mathf.Max(a, b);
                    break;

                case OperationType.Min:
                    OperationTwoInput = Mathf.Min;
                    OperationTwoInputInt = (a, b) => (int)Mathf.Min(a, b);
                    break;

                case OperationType.Clamp01:
                    OperationOneInput = Mathf.Clamp01;
                    OperationOneInputInt = a => (int)Mathf.Clamp01(a);
                    break;

                case OperationType.Sin:
                    OperationOneInput = Mathf.Sin;
                    OperationOneInputInt = a => (int)Mathf.Sin(a);
                    break;

                case OperationType.Cos:
                    OperationOneInput = Mathf.Cos;
                    OperationOneInputInt = a => (int)Mathf.Cos(a);
                    break;

                case OperationType.Tan:
                    OperationOneInput = Mathf.Tan;
                    OperationOneInputInt = a => (int)Mathf.Tan(a);
                    break;

                case OperationType.Asin:
                    OperationOneInput = Mathf.Asin;
                    OperationOneInputInt = a => (int)Mathf.Asin(a);
                    break;

                case OperationType.Acos:
                    OperationOneInput = Mathf.Acos;
                    OperationOneInputInt = a => (int)Mathf.Acos(a);
                    break;

                case OperationType.Atan:
                    OperationOneInput = Mathf.Atan;
                    OperationOneInputInt = a => (int)Mathf.Atan(a);
                    break;

                case OperationType.Atan2:
                    OperationTwoInput = Mathf.Atan2;
                    OperationTwoInputInt = (a, b) => (int)Mathf.Atan2(a, b);
                    break;

            }
        }

        void GeneratedProcessorInitializerOneInput(NodeProcessor processor)
        {
            processor.Vector2Out = () =>
            {
                Vector2 in0 = processor.Inputs[0].Vector2Out();
                return new Vector2(OperationOneInput(in0.x), OperationOneInput(in0.y));
            };
            processor.Vector3Out = () =>
            {
                Vector3 in0 = processor.Inputs[0].Vector3Out();
                return new Vector3(OperationOneInput(in0.x), OperationOneInput(in0.y), OperationOneInput(in0.z));
            };
            processor.Vector4Out = () =>
            {
                Vector4 in0 = processor.Inputs[0].Vector4Out();
                return new Vector4(OperationOneInput(in0.x), OperationOneInput(in0.y), OperationOneInput(in0.z), OperationOneInput(in0.w));
            };
            processor.ColorOut = () =>
            {
                Color in0 = processor.Inputs[0].ColorOut();
                return new Color(OperationOneInput(in0.r), OperationOneInput(in0.g), OperationOneInput(in0.b), OperationOneInput(in0.a));
            };
            processor.FloatOut = () =>
            {
                float in0 = processor.Inputs[0].FloatOut();
                return OperationOneInput(in0);
            };
            processor.IntOut = () =>
            {
                int in0 = processor.Inputs[0].IntOut();
                return OperationOneInputInt(in0);
            };
        }

        void GeneratedProcessorInitializerTwoInput(NodeProcessor processor, ValueTypeConfig aConfig, ValueTypeConfig bConfig)
        {
            if (aConfig.IsVector && bConfig.IsVector)
            {
                processor.Vector2Out = () =>
                {
                    Vector2 in0 = processor.Inputs[0].Vector2Out();
                    Vector2 in1 = processor.Inputs[1].Vector2Out();
                    return new Vector2(OperationTwoInput(in0.x, in1.x), OperationTwoInput(in0.y, in1.y));
                };
                processor.Vector3Out = () =>
                {
                    Vector3 in0 = processor.Inputs[0].Vector3Out();
                    Vector3 in1 = processor.Inputs[1].Vector3Out();
                    return new Vector3(OperationTwoInput(in0.x, in1.x), OperationTwoInput(in0.y, in1.y), OperationTwoInput(in0.z, in1.z));
                };
                processor.Vector4Out = () =>
                {
                    Vector4 in0 = processor.Inputs[0].Vector4Out();
                    Vector4 in1 = processor.Inputs[1].Vector4Out();
                    return new Vector4(OperationTwoInput(in0.x, in1.x), OperationTwoInput(in0.y, in1.y), OperationTwoInput(in0.z, in1.z), OperationTwoInput(in0.w, in1.w));
                };
                processor.ColorOut = () =>
                {
                    Color in0 = processor.Inputs[0].ColorOut();
                    Color in1 = processor.Inputs[1].ColorOut();
                    return new Color(OperationTwoInput(in0.r, in1.r), OperationTwoInput(in0.g, in1.g), OperationTwoInput(in0.b, in1.b), OperationTwoInput(in0.a, in1.a));
                };
            }
            else if (aConfig.IsVector && !bConfig.IsVector)
            {
                if (bConfig.ValueType == ValueType.Float)
                {
                    processor.Vector2Out = () =>
                    {
                        Vector2 in0 = processor.Inputs[0].Vector2Out();
                        float in1 = processor.Inputs[1].FloatOut();
                        return new Vector2(OperationTwoInput(in0.x, in1), OperationTwoInput(in0.y, in1));
                    };
                    processor.Vector3Out = () =>
                    {
                        Vector3 in0 = processor.Inputs[0].Vector3Out();
                        float in1 = processor.Inputs[1].FloatOut();
                        return new Vector3(OperationTwoInput(in0.x, in1), OperationTwoInput(in0.y, in1), OperationTwoInput(in0.z, in1));
                    };
                    processor.Vector4Out = () =>
                    {
                        Vector4 in0 = processor.Inputs[0].Vector4Out();
                        float in1 = processor.Inputs[1].FloatOut();
                        return new Vector4(OperationTwoInput(in0.x, in1), OperationTwoInput(in0.y, in1), OperationTwoInput(in0.z, in1), OperationTwoInput(in0.w, in1));
                    };
                    processor.ColorOut = () =>
                    {
                        Color in0 = processor.Inputs[0].ColorOut();
                        float in1 = processor.Inputs[1].FloatOut();
                        return new Color(OperationTwoInput(in0.r, in1), OperationTwoInput(in0.g, in1), OperationTwoInput(in0.b, in1), OperationTwoInput(in0.a, in1));
                    };
                }
                else
                {
                    processor.Vector2Out = () =>
                    {
                        Vector2 in0 = processor.Inputs[0].Vector2Out();
                        int in1 = processor.Inputs[1].IntOut();
                        return new Vector2(OperationTwoInput(in0.x, in1), OperationTwoInput(in0.y, in1));
                    };
                    processor.Vector3Out = () =>
                    {
                        Vector3 in0 = processor.Inputs[0].Vector3Out();
                        int in1 = processor.Inputs[1].IntOut();
                        return new Vector3(OperationTwoInput(in0.x, in1), OperationTwoInput(in0.y, in1), OperationTwoInput(in0.z, in1));
                    };
                    processor.Vector4Out = () =>
                    {
                        Vector4 in0 = processor.Inputs[0].Vector4Out();
                        int in1 = processor.Inputs[1].IntOut();
                        return new Vector4(OperationTwoInput(in0.x, in1), OperationTwoInput(in0.y, in1), OperationTwoInput(in0.z, in1), OperationTwoInput(in0.w, in1));
                    };
                    processor.ColorOut = () =>
                    {
                        Color in0 = processor.Inputs[0].ColorOut();
                        int in1 = processor.Inputs[1].IntOut();
                        return new Color(OperationTwoInput(in0.r, in1), OperationTwoInput(in0.g, in1), OperationTwoInput(in0.b, in1), OperationTwoInput(in0.a, in1));
                    };
                }
            }
            else if (!aConfig.IsVector && bConfig.IsVector)
            {
                if (aConfig.ValueType == ValueType.Float)
                {
                    processor.Vector2Out = () =>
                    {
                        float in0 = processor.Inputs[0].FloatOut();
                        Vector2 in1 = processor.Inputs[1].Vector2Out();
                        return new Vector2(OperationTwoInput(in0, in1.x), OperationTwoInput(in0, in1.y));
                    };
                    processor.Vector3Out = () =>
                    {
                        float in0 = processor.Inputs[0].FloatOut();
                        Vector3 in1 = processor.Inputs[1].Vector3Out();
                        return new Vector3(OperationTwoInput(in0, in1.x), OperationTwoInput(in0, in1.y), OperationTwoInput(in0, in1.z));
                    };
                    processor.Vector4Out = () =>
                    {
                        float in0 = processor.Inputs[0].FloatOut();
                        Vector4 in1 = processor.Inputs[1].Vector4Out();
                        return new Vector4(OperationTwoInput(in0, in1.x), OperationTwoInput(in0, in1.y), OperationTwoInput(in0, in1.z), OperationTwoInput(in0, in1.w));
                    };
                    processor.ColorOut = () =>
                    {
                        float in0 = processor.Inputs[0].FloatOut();
                        Color in1 = processor.Inputs[1].ColorOut();
                        return new Color(OperationTwoInput(in0, in1.r), OperationTwoInput(in0, in1.g), OperationTwoInput(in0, in1.b), OperationTwoInput(in0, in1.a));
                    };
                }
                else
                {
                    processor.Vector2Out = () =>
                    {
                        int in0 = processor.Inputs[0].IntOut();
                        Vector2 in1 = processor.Inputs[1].Vector2Out();
                        return new Vector2(OperationTwoInput(in0, in1.x), OperationTwoInput(in0, in1.y));
                    };
                    processor.Vector3Out = () =>
                    {
                        int in0 = processor.Inputs[0].IntOut();
                        Vector3 in1 = processor.Inputs[1].Vector3Out();
                        return new Vector3(OperationTwoInput(in0, in1.x), OperationTwoInput(in0, in1.y), OperationTwoInput(in0, in1.z));
                    };
                    processor.Vector4Out = () =>
                    {
                        int in0 = processor.Inputs[0].IntOut();
                        Vector4 in1 = processor.Inputs[1].Vector4Out();
                        return new Vector4(OperationTwoInput(in0, in1.x), OperationTwoInput(in0, in1.y), OperationTwoInput(in0, in1.z), OperationTwoInput(in0, in1.w));
                    };
                    processor.ColorOut = () =>
                    {
                        int in0 = processor.Inputs[0].IntOut();
                        Color in1 = processor.Inputs[1].ColorOut();
                        return new Color(OperationTwoInput(in0, in1.r), OperationTwoInput(in0, in1.g), OperationTwoInput(in0, in1.b), OperationTwoInput(in0, in1.a));
                    };
                }
            }
            else if (!aConfig.IsVector && !bConfig.IsVector)
            {
                if (aConfig.ValueType == ValueType.Float)
                {
                    if (bConfig.ValueType == ValueType.Float)
                    {
                        processor.FloatOut = () =>
                        {
                            float in0 = processor.Inputs[0].FloatOut();
                            float in1 = processor.Inputs[1].FloatOut();
                            return OperationTwoInput(in0, in1);
                        };
                    }
                    else
                    {
                        processor.FloatOut = () =>
                        {
                            float in0 = processor.Inputs[0].FloatOut();
                            int in1 = processor.Inputs[1].IntOut();
                            return OperationTwoInput(in0, in1);
                        };
                    }
                }
                else
                {
                    if (bConfig.ValueType == ValueType.Float)
                    {
                        processor.FloatOut = () =>
                        {
                            int in0 = processor.Inputs[0].IntOut();
                            float in1 = processor.Inputs[1].FloatOut();
                            return OperationTwoInput(in0, in1);
                        };
                    }
                    else
                    {
                        processor.IntOut = () =>
                        {
                            int in0 = processor.Inputs[0].IntOut();
                            int in1 = processor.Inputs[1].IntOut();
                            return OperationTwoInputInt(in0, in1);
                        };
                    }
                }
            }
        }

        public override void InitializeNodeProcessor(NodeProcessor processor)
        {
            GeneratedOperationInitializer(_Type);
            int inputsCount = OperationConfigs[(int)_Type].InputCount;
            if (inputsCount == 2)
            {
                ValueTypeConfig a = ValueTypeConfig.Types[(int)Parent.GetNode(Inputs[0]).CashedOutputType];
                ValueTypeConfig b = ValueTypeConfig.Types[(int)Parent.GetNode(Inputs[1]).CashedOutputType];
                GeneratedProcessorInitializerTwoInput(processor, a, b);
            }
            else
            {
                GeneratedProcessorInitializerOneInput(processor);
            }
        }

        public enum OperationType
        {
            Multyply,
            Divide,
            Add,
            Subtract,
            Negative,

            Log,
            Pow,
            Sqrt,
            Exp,
            ClosestPowerOfTwo,

            PerlinNoise,

            Repeat,
            PingPong,
            Abs,
            Sign,

            Round,
            Floor,
            Ceil,

            Max,
            Min,
            Clamp01,

            Sin,
            Cos,
            Tan,
            Asin,
            Acos,
            Atan,
            Atan2,
        }

        class OperationConfig
        {
            public readonly OperationType OperationType;
            public readonly string Format;
            public readonly int InputCount;

            public OperationConfig(OperationType operationType, string format, int inputCount)
            {
                OperationType = operationType;
                Format = format;
                InputCount = inputCount;
            }
        }

        static readonly OperationConfig[] OperationConfigs =
        {
            new OperationConfig(OperationType.Multyply, "{0} * {1}", 2),
            new OperationConfig(OperationType.Divide, "{0} / {1}", 2),
            new OperationConfig(OperationType.Add, "{0} + {1}", 2),
            new OperationConfig(OperationType.Subtract, "{0} - {1}", 2),
            new OperationConfig(OperationType.Negative, "-{0}", 1),

            new OperationConfig(OperationType.Log, "Mathf.Log({0}, {1})", 2),
            new OperationConfig(OperationType.Pow, "Mathf.Pow({0}, {1})", 2),
            new OperationConfig(OperationType.Sqrt, "Mathf.Sqrt({0})", 1),
            new OperationConfig(OperationType.Exp, "Mathf.Exp({0})", 1),
            new OperationConfig(OperationType.ClosestPowerOfTwo, "Mathf.ClosestPowerOfTwo((int)a)", 1),

            new OperationConfig(OperationType.PerlinNoise, "Mathf.PerlinNoise({0}, {1})", 2),

            new OperationConfig(OperationType.Repeat, "Mathf.Repeat({0}, {1})", 2),
            new OperationConfig(OperationType.PingPong, "Mathf.PingPong({0}, {1})", 2),
            new OperationConfig(OperationType.Abs, "Mathf.Abs({0})", 1),
            new OperationConfig(OperationType.Sign, "Mathf.Sign({0})", 1),

            new OperationConfig(OperationType.Round, "Mathf.Round({0})", 1),
            new OperationConfig(OperationType.Floor, "Mathf.Floor({0})", 1),
            new OperationConfig(OperationType.Ceil, "Mathf.Ceil({0})", 1),

            new OperationConfig(OperationType.Max, "Mathf.Max({0}, {1})", 2),
            new OperationConfig(OperationType.Min, "Mathf.Min({0}, {1})", 2),
            new OperationConfig(OperationType.Clamp01, "Mathf.Clamp01({0})", 1),

            new OperationConfig(OperationType.Sin, "Mathf.Sin({0})", 1),
            new OperationConfig(OperationType.Cos, "Mathf.Cos({0})", 1),
            new OperationConfig(OperationType.Tan, "Mathf.Tan({0})", 1),
            new OperationConfig(OperationType.Asin, "Mathf.Asin({0})", 1),
            new OperationConfig(OperationType.Acos, "Mathf.Acos({0})", 1),
            new OperationConfig(OperationType.Atan, "Mathf.Atan({0})", 1),
            new OperationConfig(OperationType.Atan2, "Mathf.Atan2({0}, {1})", 2),
        };

        static readonly ValueType[] Vectors =
        {
            ValueType.Vector2,
            ValueType.Vector3,
            ValueType.Vector4,
            ValueType.Color
        };
    }
}