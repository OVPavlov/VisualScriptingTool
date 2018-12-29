using NodeEditor;
using UnityEngine;

[ExecuteInEditMode]
public class NodeGraph: MonoBehaviour
{
    public NodeDataInput Input;

    public NodeData Data = new NodeData();

    void OnEnable()
    {
        if (Data == null) return;
        Data.Prepare();
        if (Input == null) Input = new NodeDataInput();
        Input.InitializeFrom(Data);
    }

    void Update()
    {
        if (Data == null) return;
        Data.Process();
    }
}
