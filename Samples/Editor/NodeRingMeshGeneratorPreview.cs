using System.Reflection;
using UnityEditor;
using UnityEngine;



public static class PreviewUtils
{
    static readonly Material DefaultMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
    static readonly Material WireMaterial = CreateWireframeMaterial();
    static Vector2 _previewDir = new Vector2(120f, -20f);

    public static void DrawMeshPreview(PreviewRenderUtility previewUtility, Rect previewRect, Rect meshInfoRect, Mesh mesh, int meshSubset = 0)
    {
        _previewDir = Drag2D(_previewDir, previewRect);
        if (Event.current.type == EventType.Repaint)
        {
            previewUtility.BeginPreview(previewRect, "preBackground");
            previewUtility.cameraFieldOfView = 30;
            RenderMeshPreview(mesh, previewUtility, DefaultMaterial, WireMaterial, _previewDir, meshSubset);
            previewUtility.EndAndDrawPreview(previewRect);
            EditorGUI.DropShadowLabel(meshInfoRect, mesh.name);
        }
    }

    static readonly MethodInfo Drag2DMethodInfo = typeof(EditorGUI).Assembly.GetType("PreviewGUI").GetMethod("Drag2D", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Vector2), typeof(Rect) }, null);
    static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
    {
        return (Vector2)Drag2DMethodInfo.Invoke(null, new object[] { scrollPosition, position });
    }

    static readonly MethodInfo RenderMeshPreviewInfo = typeof(EditorGUI).Assembly.GetType("UnityEditor.ModelInspector").GetMethod("RenderMeshPreview", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null,
        new[] { typeof(Mesh), typeof(PreviewRenderUtility), typeof(Material), typeof(Material), typeof(Vector2), typeof(int) }, null);
    static void RenderMeshPreview(Mesh mesh, PreviewRenderUtility previewUtility, Material litMaterial, Material wireMaterial, Vector2 direction, int meshSubset)
    {
        RenderMeshPreviewInfo.Invoke(null, new object[] { mesh, previewUtility, litMaterial, wireMaterial, direction, meshSubset });
    }

    static Material CreateWireframeMaterial()
    {
        Material material;
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        if (shader)
        {
            Material material1 = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            material1.SetColor("_Color", new Color(1f, 1f, 1f, 0.3f));
            material1.SetInt("_ZWrite", 0);
            material1.SetFloat("_ZBias", -1f);
            material = material1;
        }
        else
        {
            Debug.LogWarning("Could not find Colored builtin shader");
            material = null;
        }
        return material;
    }
}



[CustomPreview(typeof(NodeMeshGenerator))]
public class NodeRingMeshGeneratorPreview : ObjectPreview
{
    PreviewRenderUtility _previewRenderUtility = new PreviewRenderUtility();


    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        GUI.Label(r, target.name + " is being previewed");
        NodeMeshGenerator generator = target as NodeMeshGenerator;


        if (generator.Asset == null) return;
        PreviewUtils.DrawMeshPreview(_previewRenderUtility, r, r, generator.Asset);
    }
}



[CustomPreview(typeof(NodeParticleMeshGenerator))]
public class NodeParticleMeshGeneratorPreview : ObjectPreview
{
    PreviewRenderUtility _previewRenderUtility = new PreviewRenderUtility();


    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        GUI.Label(r, target.name + " is being previewed");
        NodeParticleMeshGenerator generator = target as NodeParticleMeshGenerator;


        if (generator.Asset == null) return;
        PreviewUtils.DrawMeshPreview(_previewRenderUtility, r, r, generator.Asset);
    }
}