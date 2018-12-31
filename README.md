# VisualScriptingTool
Simplifies some procedural stuff, useful for Technical artists.


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
    
