using NodeEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/NodeBased/MipMapProcessor")]
public class MipMapProcessor : BaseTextureGenerator
{
    public Texture2D Source;
    public NodeDataInput Input;
    public NodeData Data;

    Node _in0;
    Node _in1;
    Node _in2;
    Node _in3;
    Node _original;
    Node _out;
    Node _pos;
    Node _mipPos;
    Node _mipCount;

    protected override void GetParameters(out int width, out int height, out TextureFormat format, out bool useMipMap)
    {
        if (Source == null)
        {
            width = 1;
            height = 1;
        }
        else
        {
            width = Source.width;
            height = Source.height;
            //format = Source.format;
        }
        format = TextureFormat.ARGB32;
        useMipMap = true;
    }
    protected override void FillTexture(Texture2D texture)
    {
        if (Data == null || Source == null) return;
        bool isReadable = false;
#if UNITY_EDITOR
        string path = UnityEditor.AssetDatabase.GetAssetPath(Source);
        UnityEditor.TextureImporter ti = (UnityEditor.TextureImporter)UnityEditor.TextureImporter.GetAtPath(path);
        if (ti != null)
            isReadable = ti.isReadable;
#endif
        if (!isReadable) return;

        Data.BeginInputInit();
        _in0 = Data.CteateInput(ValueType.Color, "In 0");
        _in1 = Data.CteateInput(ValueType.Color, "In 1");
        _in2 = Data.CteateInput(ValueType.Color, "In 2");
        _in3 = Data.CteateInput(ValueType.Color, "In 3");
        _original = Data.CteateInput(ValueType.Color, "Original");

        _pos = Data.CteateInput(ValueType.Vector2, "Position");
        _mipPos = Data.CteateInput(ValueType.Float, "Mip Position");
        _mipCount = Data.CteateInput(ValueType.Float, "Mip Count");

        _out = Data.CteateOutput(ValueType.Color, "Color");
        Data.EndInputInit();

        if (Input == null) Input = new NodeDataInput();
        Input.InitializeFrom(Data);
        Input.SetTo(Data);


        int mipmapCount = 1 + (int)Mathf.Log(Mathf.Max(Source.width, Source.height), 2);
        Color[] lastColors = null;

        Data.SetFloat(_mipCount, mipmapCount);
        for (int mI = 0; mI < mipmapCount; mI++)
        {
            int mipWidth = Mathf.Max(texture.width >> mI, 1);
            int mipHeight = Mathf.Max(texture.height >> mI, 1);
            int lastMipWidth = Mathf.Max(texture.width >> (mI - 1), 1);
            int lastMipHeight = Mathf.Max(texture.height >> (mI - 1), 1);
            int mipDelta = lastMipWidth > mipWidth ? 2 : 1;
            int mipDeltaY = mipDelta * (lastMipHeight > mipHeight ? 2 : 1);

            Data.SetFloat(_mipPos, mI / (float)mipmapCount);

            Color[] colors = lastColors == null || Source.mipmapCount == mipmapCount ? Source.GetPixels(mI) : new Color[mipWidth * mipHeight];

            if (lastColors != null)
            {
                for (int i = 0; i < colors.Length; i++)
                {
                    int y = i / mipWidth;
                    int x = i - y * mipWidth;
                    int i2 = x * mipDelta + y * mipWidth * mipDeltaY;
                    if (lastColors.Length < 4) lastMipWidth = 0;


                    Data.SetVector2(_pos, new Vector2(x / (float)mipWidth, y / (float)mipHeight));
                    Data.SetColor(_in0, lastColors[i2]);
                    Data.SetColor(_in1, lastColors[i2 + 1]);
                    Data.SetColor(_in2, lastColors[i2 + lastMipWidth]);
                    Data.SetColor(_in3, lastColors[i2 + lastMipWidth + 1]);
                    Data.SetColor(_original, colors[i]);

                    Data.Process();
                    colors[i] = Data.GetColor(_out);
                }


            }

            texture.SetPixels(colors, mI);
            lastColors = colors;
        }
        texture.Apply(false);
    }
}