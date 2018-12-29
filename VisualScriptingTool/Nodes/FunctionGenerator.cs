using NodeEditor;
using UnityEngine;

namespace NodeEditor
{
    public static class FunctionGenerator
    {
        public static string TestFunc()
        {
            string functionFormat = "{0} * {1}";
            Debug.Log(GenerateAnonymousFuncForNodeProcessor(new[] {ValueType.Float, ValueType.Vector3}, ValueType.Vector3, functionFormat, 3));
            return null;
        }

        public static string GenerateFuncBody(ValueType[] inputs, ValueType output, string functionFormat)
        {
            TypeConfig outputConfig = TypeConfigs[(int)output];
            TypeConfig[] inConfigs = new TypeConfig[inputs.Length];
            for (int i = 0; i < inputs.Length; i++)
                inConfigs[i] = TypeConfigs[(int)inputs[i]];
            int maxComponents = outputConfig.VectorComponents.Length;


            {// {0} * {1}  ->  in0{0} * in1{1}
                object[] inValNames = new object[inputs.Length];
                for (int i = 0; i < inputs.Length; i++)
                    inValNames[i] = string.Format("in{0}{{{0}}}", i);
                functionFormat = string.Format(functionFormat, inValNames);
            }

            //  in0.r / in1.r, in0.g / in1.g, in0.b / in1.b, in0.a / in1.a
            string returnString = outputConfig.Constructor;
            {
                object[] returnValues = new object[maxComponents];
                for (int i = 0; i < maxComponents; i++)
                {
                    object[] inValNames = new object[inputs.Length];
                    for (int j = 0; j < inputs.Length; j++)
                        inValNames[j] = inConfigs[j].GetComponent(i);

                    returnValues[i] = string.Format(functionFormat, inValNames);
                }
                returnString = string.Format(returnString, returnValues);
            }

            string inputVals = "";
            for (int i = 0; i < inputs.Length; i++)
                inputVals += string.Format("\n\t{2} in{0} = processor.Inputs[{0}].{1}Out();", i, inputs[i], inConfigs[i].RealName);

            return string.Format("{0}\n\treturn {1};", inputVals, returnString);
        }


        public static string GenerateAnonymousFuncForNodeProcessor(ValueType[] inputs, ValueType output, string functionFormat, int indent)
        {
            string funcString = string.Format(@" 
processor.{0}Out = () =>
{{{1}
}};", output, GenerateFuncBody(inputs, output, functionFormat));
            funcString = funcString.Replace("\n", "\n" + new string('\t', indent));
            return funcString;
        }



        static TypeConfig[] InitTypeConfigs()
        {
            TypeConfig[] types = new TypeConfig[System.Enum.GetValues(typeof (ValueType)).Length];
            types[(int)ValueType.Float] = new TypeConfig("float");
            types[(int)ValueType.Bool] = new TypeConfig("bool");
            types[(int)ValueType.Int] = new TypeConfig("int");
            types[(int)ValueType.Vector2] = new TypeConfig("Vector2", "new Vector2({0}, {1})", new[] {".x", ".y"});
            types[(int)ValueType.Vector3] = new TypeConfig("Vector3", "new Vector3({0}, {1}, {2})", new[] {".x", ".y", ".z"});
            types[(int)ValueType.Vector4] = new TypeConfig("Vector4", "new Vector4({0}, {1}, {2}, {3})", new[] {".x", ".y", ".z", ".w"});
            types[(int)ValueType.Color] = new TypeConfig("Color", "new Color({0}, {1}, {2}, {3})", new[] {".r", ".g", ".b", ".a"});
            return types;
        }
        public static readonly TypeConfig[] TypeConfigs = InitTypeConfigs();

        public class TypeConfig
        {
            public string RealName;
            public string Constructor = "{0}";
            public string[] VectorComponents = {""};

            public TypeConfig(string realName, string constructor, string[] vectorComponents)
            {
                RealName = realName;
                Constructor = constructor;
                VectorComponents = vectorComponents;
            }
            public TypeConfig(string realName)
            {
                RealName = realName;
            }

            public string GetComponent(int i)
            {
                i = i % VectorComponents.Length;
                return VectorComponents[i];
            }
        }
    }
}