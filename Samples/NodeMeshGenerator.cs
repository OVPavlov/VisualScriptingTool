using NodeEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/NodeBased/PlaneMeshGenerator")]
public class NodeMeshGenerator : BaseAssetGenerator<Mesh>
{
    public int Width;
    public int Height;
    public bool RecalculateNormals;
    public bool RecalculateTangents;

    [Header("Custom Inputs")]
    public NodeDataInput Input;
    public NodeData Data;

    Node _x;
    Node _y;
    Node _width;
    Node _height;
    Node _start;

    Node _position;
    Node _normal;
    Node _tangent;
    Node _color;
    Node _uv0;
    Node _uv1;
    Node _uv2;
    Node _uv3;

    protected override Mesh CreateAsset()
    {
        return new Mesh();
    }
    protected override void ProcessAsset(Mesh asset)
    {
        Width = Mathf.Max(Width, 1);
        Height = Mathf.Max(Height, 1);
        GenerateMesh(asset, Width, Height);
    }

    void GenerateMesh(Mesh mesh, int width, int height)
    {
        if (Data == null) return;
        Data.BeginInputInit();

        _x = Data.CteateInput(ValueType.Float, "X");
        _y = Data.CteateInput(ValueType.Float, "Y");
        _width = Data.CteateInput(ValueType.Float, "Width");
        _height = Data.CteateInput(ValueType.Float, "Height");
        _start = Data.CteateInput(ValueType.Bool, "Start");

        _position = Data.CteateOutput(ValueType.Vector3, "Position");
        _normal = Data.CteateOutput(ValueType.Vector3, "Normal");
        _tangent = Data.CteateOutput(ValueType.Vector4, "Tangent");
        _color = Data.CteateOutput(ValueType.Color, "Color");
        _uv0 = Data.CteateOutput(ValueType.Vector2, "UV0");
        _uv1 = Data.CteateOutput(ValueType.Vector2, "UV1");
        _uv2 = Data.CteateOutput(ValueType.Vector2, "UV2");
        _uv3 = Data.CteateOutput(ValueType.Vector2, "UV3");

        Data.EndInputInit();

        if (Input == null) Input = new NodeDataInput();
        Input.InitializeFrom(Data);
        Input.SetTo(Data);


        int vertWidth = width + 1;
        int vertHeight = height + 1;


        int verticesCount = vertWidth * vertHeight;
        Vector3[] positions = IsConnected(_position) ? new Vector3[verticesCount] : null;
        Vector3[] normals = IsConnected(_normal) ? new Vector3[verticesCount] : null;
        Vector4[] tangents = IsConnected(_tangent) ? new Vector4[verticesCount] : null;
        Color[] colors = IsConnected(_color) ? new Color[verticesCount] : null;
        Vector2[] uv0 = IsConnected(_uv0) ? new Vector2[verticesCount] : null;
        Vector2[] uv1 = IsConnected(_uv1) ? new Vector2[verticesCount] : null;
        Vector2[] uv2 = IsConnected(_uv2) ? new Vector2[verticesCount] : null;
        Vector2[] uv3 = IsConnected(_uv3) ? new Vector2[verticesCount] : null;

        if (positions == null)
        {
            mesh.Clear();
            return;
        }

        Data.SetFloat(_width, width);
        Data.SetFloat(_height, height);

        for (int y = 0, i = 0; y < vertHeight; y++)
        {
            for (int x = 0; x < vertWidth; x++, i++)
            {
                float fx = x / (float)width;
                float fy = y / (float)height;

                Data.SetBool(_start, i == 0);
                Data.SetFloat(_x, fx);
                Data.SetFloat(_y, fy);
                Data.Process();
                positions[i] = Data.GetVector3(_position);
                if (normals != null) normals[i] = Data.GetVector3(_normal);
                if (tangents != null) tangents[i] = Data.GetVector4(_tangent);
                if (colors != null) colors[i] = Data.GetColor(_color);
                if (uv0 != null) uv0[i] = Data.GetVector2(_uv0);
                if (uv1 != null) uv1[i] = Data.GetVector2(_uv1);
                if (uv2 != null) uv2[i] = Data.GetVector2(_uv2);
                if (uv3 != null) uv3[i] = Data.GetVector2(_uv3);
            }
        }

        // \ | \ |  
        // - 0 - 1 -
        // \ | \ | \
        // - 2 - 3 -
        //   | \ | \
        int[] triangles = new int[width * height * 6];
        for (int y = 0, t = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int v0 = y * vertWidth + x;
                int v1 = v0 + 1;
                int v2 = v0 + vertWidth;
                int v3 = v2 + 1;

                triangles[t++] = v0;
                triangles[t++] = v2;
                triangles[t++] = v3;
                triangles[t++] = v3;
                triangles[t++] = v1;
                triangles[t++] = v0;
            }
        }

        mesh.Clear();
        mesh.vertices = positions;
        mesh.triangles = triangles;
        if (normals != null) mesh.normals = normals;
        if (tangents != null) mesh.tangents = tangents;
        if (colors != null) mesh.colors = colors;
        if (uv0 != null) mesh.uv = uv0;
        if (uv1 != null) mesh.uv2 = uv1;
        if (uv2 != null) mesh.uv3 = uv2;
        if (uv3 != null) mesh.uv4 = uv3;
        mesh.RecalculateBounds();
        if (RecalculateNormals) mesh.RecalculateNormals();
        if (RecalculateTangents) mesh.RecalculateTangents();
    }

    static bool IsConnected(Node node)
    {
        if (node == null || node.Inputs == null || node.Inputs.Length == 0 || node.Inputs[0] == null) return false;
        return node.Inputs[0].IsConnected;
    }
}