using System;
using UnityEngine;

namespace NodeEditor
{
    public class NodeProcessor
    {
        public NodeProcessor[] Inputs;
        public int OutputCount;

        public Func<float> FloatOut;
        Func<float> _rawFloatOut;
        float _cachedFloat;

        public Func<Vector2> Vector2Out;
        Func<Vector2> _rawVector2Out;
        Vector2 _cachedVector2;

        public Func<Vector3> Vector3Out;
        Func<Vector3> _rawVector3Out;
        Vector3 _cachedVector3;

        public Func<Vector4> Vector4Out;
        Func<Vector4> _rawVector4Out;
        Vector4 _cachedVector4;

        public Func<Color> ColorOut;
        Func<Color> _rawColorOut;
        Color _cachedColor;

        public Func<int> IntOut;
        Func<int> _rawIntOut;
        int _cachedInt;

        public Func<bool> BoolOut;
        Func<bool> _rawBoolOut;
        bool _cachedBool;

        public Func<Texture2D> Texture2DOut;
        Func<Texture2D> _rawTexture2DOut;
        Texture2D _cachedTexture2D;

        public Func<RenderTexture> RenderTextureOut;
        Func<RenderTexture> _rawRenderTextureOut;
        RenderTexture _cachedRenderTexture;

        public Action VoidOut;

        public static long CurrentFrame;
        long _lastNodeFrame = -1;

        public void SeveralOutputsOptimisation()
        {
            //Float
            _rawFloatOut = FloatOut;
            FloatOut = delegate
            {
                if (CurrentFrame == _lastNodeFrame) return _cachedFloat;
                _lastNodeFrame = CurrentFrame;
                return _cachedFloat = _rawFloatOut();
            };

            //Vector2
            _rawVector2Out = Vector2Out;
            Vector2Out = delegate
            {
                if (CurrentFrame == _lastNodeFrame) return _cachedVector2;
                _lastNodeFrame = CurrentFrame;
                return _cachedVector2 = _rawVector2Out();
            };

            //Vector3
            _rawVector3Out = Vector3Out;
            Vector3Out = delegate
            {
                if (CurrentFrame == _lastNodeFrame) return _cachedVector3;
                _lastNodeFrame = CurrentFrame;
                return _cachedVector3 = _rawVector3Out();
            };

            //Vector4
            _rawVector4Out = Vector4Out;
            Vector4Out = delegate
            {
                if (CurrentFrame == _lastNodeFrame) return _cachedVector4;
                _lastNodeFrame = CurrentFrame;
                return _cachedVector4 = _rawVector4Out();
            };

            //Color
            _rawColorOut = ColorOut;
            ColorOut = delegate
            {
                if (CurrentFrame == _lastNodeFrame) return _cachedColor;
                _lastNodeFrame = CurrentFrame;
                return _cachedColor = _rawColorOut();
            };

            //Int
            _rawIntOut = IntOut;
            IntOut = delegate
            {
                if (CurrentFrame == _lastNodeFrame) return _cachedInt;
                _lastNodeFrame = CurrentFrame;
                return _cachedInt = _rawIntOut();
            };

            //Bool
            _rawBoolOut = BoolOut;
            BoolOut = delegate
            {
                if (CurrentFrame == _lastNodeFrame) return _cachedBool;
                _lastNodeFrame = CurrentFrame;
                return _cachedBool = _rawBoolOut();
            };

            //Texture2D
            _rawTexture2DOut = Texture2DOut;
            Texture2DOut = delegate
            {
                if (CurrentFrame == _lastNodeFrame) return _cachedTexture2D;
                _lastNodeFrame = CurrentFrame;
                return _cachedTexture2D = _rawTexture2DOut();
            };

            //RenderTexture
            _rawRenderTextureOut = RenderTextureOut;
            RenderTextureOut = delegate
            {
                if (CurrentFrame == _lastNodeFrame) return _cachedRenderTexture;
                _lastNodeFrame = CurrentFrame;
                return _cachedRenderTexture = _rawRenderTextureOut();
            };
        }
    }
}