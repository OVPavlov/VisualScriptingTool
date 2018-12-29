using NodeEditor;
using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObjects/NodeBased/ParticleMeshGenerator")]
public class NodeParticleMeshGenerator : BaseAssetGenerator<Mesh>
{
    public int Seed;
    public SpriteItem[] Items;
    public NodeData Data;

    Node _particleUV;
    Node _particlePosition;

    Node _updateParticle;
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
        GenerateMesh(asset);
    }

    void GenerateMesh(Mesh mesh)
    {
        if (Data == null) return;
        Data.BeginInputInit();

        _particleUV = Data.CteateInput(ValueType.Vector2, "Particle UV");
        _particlePosition = Data.CteateInput(ValueType.Vector2, "Particle Position");
        _updateParticle = Data.CteateInput(ValueType.Bool, "Update Particle");

        _position = Data.CteateOutput(ValueType.Vector3, "Position");
        _normal = Data.CteateOutput(ValueType.Vector3, "Normal");
        _tangent = Data.CteateOutput(ValueType.Vector4, "Tangent");
        _color = Data.CteateOutput(ValueType.Color, "Color");
        _uv0 = Data.CteateOutput(ValueType.Vector2, "UV0");
        _uv1 = Data.CteateOutput(ValueType.Vector2, "UV1");
        _uv2 = Data.CteateOutput(ValueType.Vector2, "UV2");
        _uv3 = Data.CteateOutput(ValueType.Vector2, "UV3");

        Data.EndInputInit();

        if (!IsConnected(_position))
        {
            mesh.Clear();
            return;
        }

        Random.InitState(Seed);

        int vertexCount = 0;
        int triangleCount = 0;
        foreach (SpriteItem item in Items)
        {
            if (item.Input == null) item.Input = new NodeDataInput();
            item.Input.InitializeFrom(Data);
            //item.Input.SetTo(Data);
            int verts = item.Sprite != null ? item.Sprite.vertices.Length : 4;
            int tris = item.Sprite != null ? item.Sprite.triangles.Length : 6;
            vertexCount += verts * item.Count;
            triangleCount += tris * item.Count;
        }


        Vector3[] positions = IsConnected(_position) ? new Vector3[vertexCount] : null;
        Vector3[] normals = IsConnected(_normal) ? new Vector3[vertexCount] : null;
        Vector4[] tangents = IsConnected(_tangent) ? new Vector4[vertexCount] : null;
        Color[] colors = IsConnected(_color) ? new Color[vertexCount] : null;
        Vector2[] uv0 = IsConnected(_uv0) ? new Vector2[vertexCount] : null;
        Vector2[] uv1 = IsConnected(_uv1) ? new Vector2[vertexCount] : null;
        Vector2[] uv2 = IsConnected(_uv2) ? new Vector2[vertexCount] : null;
        Vector2[] uv3 = IsConnected(_uv3) ? new Vector2[vertexCount] : null;

        int[] triangles = new int[triangleCount];


        int vi = 0;
        int ti = 0;
        foreach (SpriteItem item in Items)
        {
            item.Input.SetTo(Data);
            bool spriteExist = item.Sprite != null;
            Vector2[] vertices = spriteExist ? item.Sprite.vertices : QuadVertices;
            Vector2[] uvs = spriteExist ? item.Sprite.uv : QuadUV;
            ushort[] trianglesP = spriteExist ? item.Sprite.triangles : QuadTriangles;

            for (int i = 0; i < item.Count; i++)
            {
                for (int t = 0; t < trianglesP.Length; t++,ti++)
                    triangles[ti] = trianglesP[t] + vi;


                for (int v = 0; v < vertices.Length; v++, vi++)
                {
                    Data.SetVector2(_particleUV, uvs[v]);
                    Data.SetVector2(_particlePosition, vertices[v]);
                    Data.SetBool(_updateParticle, v == 0);
                    Data.Process();
                    positions[vi] = Data.GetVector3(_position);
                    if (normals != null) normals[vi] = Data.GetVector3(_normal);
                    if (tangents != null) tangents[vi] = Data.GetVector4(_tangent);
                    if (colors != null) colors[vi] = Data.GetColor(_color);
                    if (uv0 != null) uv0[vi] = Data.GetVector2(_uv0);
                    if (uv1 != null) uv1[vi] = Data.GetVector2(_uv1);
                    if (uv2 != null) uv2[vi] = Data.GetVector2(_uv2);
                    if (uv3 != null) uv3[vi] = Data.GetVector2(_uv3);
                }
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
    }

    static bool IsConnected(Node node)
    {
        if (node == null || node.Inputs == null || node.Inputs.Length == 0 || node.Inputs[0] == null) return false;
        return node.Inputs[0].IsConnected;
    }

    //0 - 1
    //| \ |
    //2 - 3
    static Vector2[] QuadVertices =
    {
        new Vector2(-1, -1),
        new Vector2(1, -1),
        new Vector2(-1, 1),
        new Vector2(1, 1),
    };
    static Vector2[] QuadUV =
    {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(0, 1),
        new Vector2(1, 1),
    };
    static ushort[] QuadTriangles =
    {
        0, 2, 3,
        3, 1, 0
    };

    [System.Serializable]
    public class SpriteItem
    {
        public Sprite Sprite;
        public int Count = 10;
        public NodeDataInput Input;
    }
}
