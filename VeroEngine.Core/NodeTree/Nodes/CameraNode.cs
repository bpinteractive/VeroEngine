using System;
using System.IO;
using OpenTK.Mathematics;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Mathematics;
using VeroEngine.Core.Rendering;

namespace VeroEngine.Core.NodeTree.Nodes;

public class CameraNode : Node
{
    private Camera _renderCamera;
    private RenderMesh _cameraMesh;

    public override void Create()
    {
        _renderCamera = Collections.RootTree.SceneCamera;
        _cameraMesh = RenderMesh.FromModelFile(Path.Combine("Editor", "camera.obj"));
    }

    public override void Draw()
    {
        if (!Visible)
        {
            var proj = Collections.RootTree.SceneCamera.GetProjectionMatrix();
            var view = Collections.RootTree.SceneCamera.GetViewMatrix();
            var translation = GlobalPosition.GetTranslationMatrix();
            var rotation = GlobalRotation.GetRotationMatrix();

            var model = rotation * translation;
            _cameraMesh.Render(model, view, proj, Color, true);
        }
        if (Collections.InEditorHint)
        {
            var proj = Collections.RootTree.SceneCamera.GetProjectionMatrix();
            var view = Collections.RootTree.SceneCamera.GetViewMatrix();
            var translation = GlobalPosition.GetTranslationMatrix();
            var rotation = GlobalRotation.GetRotationMatrix();

            var model = rotation * translation;
            _cameraMesh.Render(model, view, proj, Color, true);
        }

        if (!Visible) return;
        if (Collections.InEditorHint) return;
        _renderCamera.UpdatePosition(GlobalPosition);
        _renderCamera.SetRotation(GlobalRotation);
        base.Draw();
    }
    
}