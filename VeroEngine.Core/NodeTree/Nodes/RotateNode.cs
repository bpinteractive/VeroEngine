using VeroEngine.Core.Mathematics;

namespace VeroEngine.Core.NodeTree.Nodes;

public class RotateNode : Node
{
    public Vector3 SpinVector { get; set; } = new();

    public override void Update(double delta, bool editorHint)
    {
        if (!editorHint)
        {
            Rotation += SpinVector * (float)delta;
        }
        base.Update(delta, editorHint);
    }
}