using System;
using UnityEngine;

namespace NodeEditor
{
    public class ValueTypeConfig
    {
        public static readonly ValueTypeConfig[] Types =
        {/*                                                                                        isVector isValue isValueExceptInt*/
            new ValueTypeConfig(ValueType.None, null, "",/*                                        */ false, false, false),
            new ValueTypeConfig(ValueType.Error, null, "",/*                                       */ false, false, false),
            new ValueTypeConfig(ValueType.AutoValue, null, "",/*                                   */ false, true, true),
            new ValueTypeConfig(ValueType.AutoVector, null, "",/*                                  */ true, true, true),
            new ValueTypeConfig(ValueType.Float, typeof (float), "float",/*                        */ false, true, true),
            new ValueTypeConfig(ValueType.Vector2, typeof (Vector2), "Vector2",/*                  */ true, true, true),
            new ValueTypeConfig(ValueType.Vector3, typeof (Vector3), "Vector3",/*                  */ true, true, true),
            new ValueTypeConfig(ValueType.Vector4, typeof (Vector4), "Vector4",/*                  */ true, true, true),
            new ValueTypeConfig(ValueType.Color, typeof (Color), "Color",/*                        */ true, true, true),
            new ValueTypeConfig(ValueType.Int, typeof (int), "int",/*                              */ false, true, false),
            new ValueTypeConfig(ValueType.Bool, typeof (bool), "bool",/*                           */ false, false, false),
            new ValueTypeConfig(ValueType.FloatMap2D, null, "unimplemented",/*                     */ false, false, false),
            new ValueTypeConfig(ValueType.Texture2D, typeof (Texture2D), "Texture2D",/*            */ false, false, false),
            new ValueTypeConfig(ValueType.RenderTexture, typeof (RenderTexture), "RenderTexture",/**/ false, false, false),
            new ValueTypeConfig(ValueType.Polygon, null, "unimplemented",/*                        */ false, false, false),
            new ValueTypeConfig(ValueType.Mesh, typeof (Mesh), "Mesh",/*                           */ false, false, false),
        };

        public readonly ValueType ValueType;
        public readonly Type Type;
        public readonly string RealName;
        public readonly bool IsVector;
        public readonly bool IsValue;
        public readonly bool IsValueExceptInt;

        public ValueTypeConfig(ValueType valueType, Type type, string realName, bool isVector, bool isValue, bool isValueExceptInt)
        {
            ValueType = valueType;
            Type = type;
            RealName = realName;
            IsVector = isVector;
            IsValue = isValue;
            IsValueExceptInt = isValueExceptInt;
        }
    }
}