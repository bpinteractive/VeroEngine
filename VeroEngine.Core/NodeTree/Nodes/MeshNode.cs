using OpenTK.Graphics.OpenGL4;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Rendering;
using Matrix4 = OpenTK.Mathematics.Matrix4;

namespace VeroEngine.Core.NodeTree.Nodes;

public class MeshNode : Node
{
    private RenderMesh _renderMesh;
    public bool DepthTest { get; set; } = true;
    private string _model;
    public string Model
    {
        set
        {
            _model = value;
            SetMesh(RenderMesh.FromModelFile(value)); // FIRE
        }
        get => _model;
    }

    public void SetMesh(RenderMesh mesh)
    {
        _renderMesh?.Dispose();
        _renderMesh = mesh;
    }

    public override Node Duplicate()
    {
        MeshNode meshNode = base.Duplicate() as MeshNode;
        meshNode.Model = Model;
        return meshNode;
    }

    public override void Create()
    {
        Model = "Models/cube.obj";
    }

    public override void Update(double delta, bool editorHint)
    {
        base.Update(delta, editorHint);
    }

    public override void Draw()
    {
        if (!Visible) return;
        if (!DepthTest)
        {
            GL.Disable(EnableCap.DepthTest);
        }
        var proj = Collections.RootTree.SceneCamera.GetProjectionMatrix();
        var view = Collections.RootTree.SceneCamera.GetViewMatrix();
        var translation = GlobalPosition.GetTranslationMatrix();
        var rotation = GlobalRotation.GetRotationMatrix();
        var scale = Matrix4.CreateScale(GlobalScale.ToOpenTK());
        
        var model = scale * rotation * translation;
        _renderMesh?.Render(model, view, proj, Color);
        GL.Enable(EnableCap.DepthTest);
        base.Draw();
    }

    public override void Destroy()
    {
        _renderMesh?.Dispose();
        base.Destroy();
    }
}