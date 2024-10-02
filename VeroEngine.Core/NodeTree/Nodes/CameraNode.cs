using VeroEngine.Core.Generic;
using VeroEngine.Core.Rendering;

namespace VeroEngine.Core.NodeTree.Nodes;

public class CameraNode : Node
{
    private Camera _renderCamera;


    public override void Create()
    {
        _renderCamera = Collections.RootTree.SceneCamera;
    }

    public override void Draw()
    {
        if(!Visible) return;
        _renderCamera.UpdatePosition(GlobalPosition);
        _renderCamera.SetRotation(GlobalRotation);
        base.Draw();
        
    }
}