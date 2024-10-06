using System;
using System.IO;
using OpenTK.Mathematics;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Mathematics;
using VeroEngine.Core.Rendering;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace VeroEngine.Core.NodeTree.Nodes;

public class CameraNode : Node
{
    private Camera _renderCamera;
    private RenderMesh _cameraMesh;
    
    public float FOV { get; set; } = 90;

    public override void Create()
    {
        _renderCamera = Collections.RootTree.SceneCamera;
        _cameraMesh = RenderMesh.FromModelFile(Path.Combine("Editor", "camera.obj"));
    }

    public override void Draw()
    {
        if (Collections.InEditorHint || !Visible)
        {
            var proj = Collections.RootTree.SceneCamera.GetProjectionMatrix();
            var view = Collections.RootTree.SceneCamera.GetViewMatrix();
            var translation = GlobalPosition.GetTranslationMatrix();
            var rotation = GlobalRotation.GetRotationMatrix();
            
            float scaleFactorX = 1.0f / MathF.Tan((float)Util.Deg2Rad(FOV) / 2);
            float scaleFactorY = FOV/90;
            float scaleFactorZ = FOV/90;
            var scale = Matrix4.CreateScale(new Vector3(scaleFactorX, scaleFactorY, scaleFactorZ));

            
            var model = scale * rotation * translation;
            _cameraMesh.Render(model, view, proj, new Mathematics.Vector3(255, 0, 0), true);
        }

        Collections.IsCameraStolen = Visible;
        if (!Visible) return;
        if (Collections.InEditorHint) return;
        _renderCamera.UpdatePosition(GlobalPosition);
        _renderCamera.SetRotation(GlobalRotation);
        _renderCamera.SetFieldOfView((float)Util.Deg2Rad(FOV));
        
        base.Draw();
    }
    
}