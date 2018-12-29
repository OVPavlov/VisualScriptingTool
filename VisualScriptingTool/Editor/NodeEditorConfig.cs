using UnityEngine;


namespace NodeEditor
{

[CreateAssetMenu(menuName = "NodeEditorConfig")]
    public class NodeEditorConfig: ScriptableObjectSingleton<NodeEditorConfig>
    {
        public InputsOutputsData InputsOutputs;
        public Color BackgroundColor = Color.white;
        public Color NodeColor = new Color(1, 1, 1, 0);
        public Color[] Colors;

        [System.Serializable]
        public class InputsOutputsData
        {
            public Styles.ShapeType Shape;
            public int Size;
            public float Edge;
            public int Padding;
        }
    }
}