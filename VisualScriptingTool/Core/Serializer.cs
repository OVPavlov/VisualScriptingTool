using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace NodeEditor
{
    public static class Serializer
    {
        const string VectorPercision = "";
        const string ColorPercision = "0.###";
        const string CurvePercision = "0.##";
        const string KeyTimePercision = "0.###";
        static readonly HashSet<string> Exeptions = new HashSet<string> {"Inputs", "Position", "NodeId"};

        public static string Serialize(Dictionary<int, Node> nodes)
        {
            HashSet<int> usedDefaults = GetUsedDefaults(nodes);
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (var pair in nodes)
            {
                Node node = pair.Value;
                if (node is DefaultNode && !usedDefaults.Contains(node.NodeId)) continue;
                if (!first)
                    sb.Append('#');
                first = false;
                sb.Append(NodeCollector.ToID(node.GetType()));
                sb.Append('|');
                sb.Append(node.NodeId);
                sb.Append('|');
                ICustomSerialiser custom = node as ICustomSerialiser;
                if (custom != null)
                {
                    sb.Append(custom.Serialize());
                    continue;
                }
                sb.Append(PositionToString(node.Position));
                sb.Append('|');
                for (int i = 0; i < node.Inputs.Length; i++)
                {
                    if (i != 0) sb.Append(' ');
                    sb.Append(node.Inputs[i].NodeId);
                }
                sb.Append('|');

                FieldInfo[] fields = node.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                for (int i = 0; i < fields.Length; i++)
                {
                    if ((fields[i].Attributes & FieldAttributes.NotSerialized) != 0) continue;
                    if (Exeptions.Contains(fields[i].Name)) continue;
                    if (i != 0) sb.Append('|');
                    sb.Append(SerializeField(fields[i], node));

                }
            }
            return sb.ToString();
        }
        public static void Deserialize(string data, NodeData nodeData, Dictionary<int, Node> nodes)
        {
            nodes.Clear();
            string[] nodeStrings = data.Split('#');
            if (nodeStrings.Length < 2) return;
            for (int n = 0; n < nodeStrings.Length; n++)
            {
                string[] lines = nodeStrings[n].Split('|');

                Node node = nodeData.CreateNode(lines[0], int.Parse(lines[1]));
                if (node == null) continue;
                ICustomSerialiser custom = node as ICustomSerialiser;
                if (custom != null)
                {
                    custom.Deserialize(lines);
                    continue;
                }

                node.Position = StringToPosition(lines[2]);

                FillInputs(ref node.Inputs, lines[3]);

                FieldInfo[] fields = node.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                for (int i = 0, l = 4; i < fields.Length; i++)
                {
                    if ((fields[i].Attributes & FieldAttributes.NotSerialized) != 0) continue;
                    if (Exeptions.Contains(fields[i].Name)) continue;

                    DeserializeField(fields[i], node, lines[l++]);
                }
            }
        }

        static HashSet<int> GetUsedDefaults(Dictionary<int, Node> nodes)
        {
            HashSet<int> defaults = new HashSet<int>();
            foreach (KeyValuePair<int, Node> pair in nodes)
            {
                var inputs = pair.Value.Inputs;
                for (int i = 0; i < inputs.Length; i++)
                {
                    Link link = inputs[i];
                    if (link.NodeId < Node.NoNodeID)
                        defaults.Add(link.NodeId);
                }
            }
            return defaults;
        }

        static string SerializeField(FieldInfo field, object obj)
        {
            obj = field.GetValue(obj);
            Type fieldType = field.FieldType;
            if (fieldType == typeof (int) || fieldType == typeof (float))
                return obj.ToString();
            if (fieldType.IsEnum)
                return ((int)obj).ToString();
            if (fieldType == typeof (bool))
                return (bool)obj ? "t" : "f";
            if (fieldType == typeof (string))
                return (string)obj;
            if (fieldType == typeof (AnimationCurve))
                return CurveToString((AnimationCurve)obj);
            if (fieldType == typeof (Gradient))
            {
                return GradientToString((Gradient)obj);
            }
            if (fieldType == typeof (Vector2))
            {
                Vector2 v = (Vector2)obj;
                return VecToString(VectorPercision, v.x, v.y);
            }
            if (fieldType == typeof (Vector3))
            {
                Vector3 v = (Vector3)obj;
                return VecToString(VectorPercision, v.x, v.y, v.z);
            }
            if (fieldType == typeof (Vector4))
            {
                Vector4 v = (Vector4)obj;
                return VecToString(VectorPercision, v.x, v.y, v.z, v.w);
            }
            if (fieldType == typeof (Color))
            {
                Color c = (Color)obj;
                return VecToString(ColorPercision, c.r, c.g, c.b, c.a);
            }

            return JsonUtility.ToJson(obj);

        }
        static void DeserializeField(FieldInfo field, object obj, string str)
        {
            Type fieldType = field.FieldType;
            if (fieldType == typeof (int))
                field.SetValue(obj, int.Parse(str));
            else if (fieldType == typeof (float))
                field.SetValue(obj, float.Parse(str));
            else if (fieldType.IsEnum)
                field.SetValue(obj, int.Parse(str));
            else if (fieldType == typeof (bool))
                field.SetValue(obj, str == "t");
            else if (fieldType == typeof (string))
                field.SetValue(obj, str);
            else if (fieldType == typeof (AnimationCurve))
                field.SetValue(obj, StringToCurve(str));
            else if (fieldType == typeof(Gradient))
                field.SetValue(obj, StringToGradient(str)); 
            else if (fieldType == typeof (Vector2))
                field.SetValue(obj, StringToVec2(str));
            else if (fieldType == typeof (Vector3))
                field.SetValue(obj, StringToVec3(str));
            else if (fieldType == typeof (Vector4))
                field.SetValue(obj, StringToVec4(str));
            else if (fieldType == typeof (Color))
                field.SetValue(obj, StringToColor(str));
            else
                field.SetValue(obj, JsonUtility.FromJson(str, fieldType));
        }

        static string CurveToString(AnimationCurve curve)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append((int)curve.preWrapMode);
            sb.Append(';');
            sb.Append((int)curve.postWrapMode);
            for (int k = 0; k < curve.keys.Length; k++)
            {
                Keyframe key = curve.keys[k];
                sb.Append(';');
                sb.Append(key.time.ToString(KeyTimePercision));
                sb.Append(',');
                sb.Append(key.value.ToString(CurvePercision));
                sb.Append(',');
                sb.Append(key.inTangent.ToString(CurvePercision));
                sb.Append(',');
                sb.Append(key.outTangent.ToString(CurvePercision));
                sb.Append(',');
                sb.Append(key.tangentMode);
            }
            return sb.ToString();
        }
        static AnimationCurve StringToCurve(string str)
        {
            string[] lines = str.Split(';');
            AnimationCurve curve = new AnimationCurve();
            if (lines.Length < 2) return curve;

            curve.preWrapMode = (WrapMode)int.Parse(lines[0]);
            curve.postWrapMode = (WrapMode)int.Parse(lines[1]);
            Keyframe[] keys = new Keyframe[lines.Length - 2];
            for (int l = 2; l < lines.Length; l++)
            {
                string[] vals = lines[l].Split(',');
                keys[l - 2] = new Keyframe
                {
                    time = float.Parse(vals[0]),
                    value = float.Parse(vals[1]),
                    inTangent = float.Parse(vals[2]),
                    outTangent = float.Parse(vals[3]),
                    tangentMode = int.Parse(vals[4])
                };
            }
            curve.keys = keys;
            return curve;
        }

        static string GradientToString(Gradient gradient)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append((int)gradient.mode);
            sb.Append(';');
            GradientColorKey[] colorKeys = gradient.colorKeys;
            for (int k = 0; k < colorKeys.Length; k++)
            {
                GradientColorKey key = colorKeys[k];
                if (k != 0) sb.Append(':');
                sb.Append(key.time.ToString(KeyTimePercision));
                sb.Append(',');
                sb.Append(key.color.r.ToString(ColorPercision));
                sb.Append(',');
                sb.Append(key.color.g.ToString(ColorPercision));
                sb.Append(',');
                sb.Append(key.color.b.ToString(ColorPercision));
                sb.Append(',');
                sb.Append(key.color.a.ToString(ColorPercision));
            }
            sb.Append(';');
            GradientAlphaKey[] alphaKeys = gradient.alphaKeys;
            for (int k = 0; k < alphaKeys.Length; k++)
            {
                GradientAlphaKey key = alphaKeys[k];
                if (k != 0) sb.Append(':');
                sb.Append(key.time.ToString(KeyTimePercision));
                sb.Append(',');
                sb.Append(key.alpha);
            }
            return sb.ToString();
        }
        static Gradient StringToGradient(string str)
        {
            string[] lines = str.Split(';');
            Gradient gradient = new Gradient();
            if (lines.Length < 2) return gradient;
            gradient.mode = (GradientMode)int.Parse(lines[0]);

            string[] sKeys = lines[1].Split(':');
            GradientColorKey[] colorKeys = new GradientColorKey[sKeys.Length];
            for (int k = 0; k < sKeys.Length; k++)
            {
                string[] vals = sKeys[k].Split(',');
                colorKeys[k] = new GradientColorKey
                {
                    time = float.Parse(vals[0]),
                    color = new Color(float.Parse(vals[1]), float.Parse(vals[2]), float.Parse(vals[3]), float.Parse(vals[4]))
                };
            }
            gradient.colorKeys = colorKeys;

            sKeys = lines[2].Split(':');
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[sKeys.Length];
            for (int k = 0; k < sKeys.Length; k++)
            {
                string[] vals = sKeys[k].Split(',');
                alphaKeys[k] = new GradientAlphaKey
                {
                    time = float.Parse(vals[0]),
                    alpha = float.Parse(vals[1])
                };
            }
            gradient.alphaKeys = alphaKeys;
            return gradient;
        }
        
        static string VecToString(string percision, params float[] vec)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < vec.Length; i++)
            {
                if (i != 0) sb.Append(',');
                sb.Append(vec[i].ToString(percision));
            }
            return sb.ToString();
        }
        static Vector2 StringToVec2(string str)
        {
            string[] v = str.Split(',');
            return new Vector2(float.Parse(v[0]), float.Parse(v[1]));
        }
        static Vector3 StringToVec3(string str)
        {
            string[] v = str.Split(',');
            return new Vector3(float.Parse(v[0]), float.Parse(v[1]), float.Parse(v[2]));
        }
        static Vector4 StringToVec4(string str)
        {
            string[] v = str.Split(',');
            return new Vector4(float.Parse(v[0]), float.Parse(v[1]), float.Parse(v[2]), float.Parse(v[3]));
        }
        static Color StringToColor(string str)
        {
            string[] v = str.Split(',');
            return new Color(float.Parse(v[0]), float.Parse(v[1]), float.Parse(v[2]), float.Parse(v[3]));
        }


        static void FillInputs(ref Link[] links, string str)
        {
            string[] ids = str.Split(' ');
            if (ids[0] == "") return;
            if (links.Length < ids.Length)
            {
                Link[] newLinks = new Link[ids.Length];
                for (int i = 0; i < links.Length; i++)
                    newLinks[i] = links[i];
                for (int i = links.Length; i < newLinks.Length; i++)
                    newLinks[i] = new Link();
                links = newLinks;
            }

            for (int i = 0; i < ids.Length; i++)
                links[i].NodeId = int.Parse(ids[i]);
        }

        static string PositionToString(Vector2 pos)
        {
            return Mathf.RoundToInt(pos.x / 16) + " " + Mathf.RoundToInt(pos.y / 16);
        }
        static Vector2 StringToPosition(string str)
        {
            string[] strings = str.Split(' ');
            return new Vector2(int.Parse(strings[0]) * 16f, int.Parse(strings[1]) * 16f);
        }
    }

    interface ICustomSerialiser
    {
        string Serialize();
        void Deserialize(string[] lines);
    }
}