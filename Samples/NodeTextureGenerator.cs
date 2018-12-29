using NodeEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/NodeBased/TextureGenerator")]
public class NodeTextureGenerator : BaseTextureGenerator
{
    public int Width = 64;
    public int Height = 64;
    public TextureFormat Format = TextureFormat.ARGB32;
    public bool UseMipMap = false;

    public NodeDataInput Input;
    public NodeData Data;
    Node _color;
    Node _position;
    Node _rToBorder;
    Node _rToCorner;
    Node _angle;

    //[PreviewTexture(HideProperty = true)]
    public Texture Texture;


    protected override void GetParameters(out int width, out int height, out TextureFormat format, out bool useMipMap)
    {
        width = Width;
        height = Height;
        format = Format;
        useMipMap = UseMipMap;
    }
    protected override void FillTexture(Texture2D texture)
    {
        if (Data == null) return;
        Data.BeginInputInit();
        _color = Data.CteateOutput(ValueType.Color, "Color");
        _position = Data.CteateInput(ValueType.Vector2, "Position");
        _rToBorder = Data.CteateInput(ValueType.Float, "Radius To Border");
        _rToCorner = Data.CteateInput(ValueType.Float, "Radius To Corner");
        _angle = Data.CteateInput(ValueType.Float, "Angle");
        Data.EndInputInit();

        if (Input == null) Input = new NodeDataInput();
        Input.InitializeFrom(Data);
        Input.SetTo(Data);

        int width = texture.width;
        int height = texture.height;

        Color[] colors = new Color[width * height];
        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;
        float cornerlength = Mathf.Sqrt(2);

        for (int i = 0; i < colors.Length; i++)
        {
            float y = (int)(i / width);
            float x = i - y * width;
            y -= halfHeight - 0.5f;
            x -= halfWidth - 0.5f;
            Vector2 position = new Vector2(x / halfWidth, y / halfHeight);

            float r = position.magnitude;
            float rToBorder = r;
            float rToCorner = r / cornerlength;
            float angle = 0.5f + Mathf.Atan2(-x, -y) / (2 * Mathf.PI);

            Data.SetVector2(_position, new Vector2(position.x * 0.5f + 0.5f, position.y * 0.5f + 0.5f));
            Data.SetFloat(_rToBorder, rToBorder);
            Data.SetFloat(_rToCorner, rToCorner);
            Data.SetFloat(_angle, angle);
            Data.Process();
            colors[i] = Data.GetColor(_color);
        }

        texture.SetPixels(colors);
        texture.Apply();
        Texture = texture;
    }


    public enum Type
    {
        Gradient,
        Curve,
    }

    public enum LengthType
    {
        ToBorder,
        ToCorner
    }
}