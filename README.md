# VisualScriptingTool
Simplifies some procedural stuff, useful for Technical artists.
* procedural animation
* procedural generation
* partial customization of your pipelines


### Easy to add properties in the inspector of the graph

![inspector](https://media.giphy.com/media/1yTeIyYoQbX1vIvOxr/giphy.gif)


### Nodes adapt to the type of your input

![Types](https://media.giphy.com/media/1AeePGaIz4uFbfZHL5/giphy.gif)


### Easy to add graph in your code in order to give designers more control over some stages of execution

```csharp
public NodeDataInput InspectorValues; // if you want user to add properties in the inspector (optional)
public NodeData Data;
Node _inputNode;
Node _outputNode;

void Initialize()
{
    if (Data == null) return;

    // specify Nodes that must be input and output of your graph (optional)
    Data.BeginInputInit();
    _inputNode = Data.CteateInput(ValueType.Vector2, "Position");
    _outputNode = Data.CteateOutput(ValueType.Color, "Color");
    Data.EndInputInit();

    // if you want user to add properties in the inspector (optional)
    if (InspectorValues == null) InspectorValues = new NodeDataInput();
    InspectorValues.InitializeFrom(Data);
}

void Update(Vector2 position, out Color color)
{
    InspectorValues.SetTo(Data); // reload user created properties from the inspector (optional)
    Data.SetVector2(_inputNode, position);
    Data.Process();
    color = Data.GetColor(_outputNode);
}
```



### Examples
[procedural mesh.mp4](https://video.twimg.com/ext_tw_video/949941009131147264/pu/vid/490x360/JCRjIK995U2KD4ct.mp4)<br>
[tiled fire texture.mp4](https://video.twimg.com/ext_tw_video/950790167761219584/pu/vid/1128x720/jokOlldZewNjATE3.mp4)<br>
[stardome.mp4](https://video.twimg.com/ext_tw_video/951759340247150592/pu/vid/490x360/AuvcEoNDe_SUroJL.mp4)
