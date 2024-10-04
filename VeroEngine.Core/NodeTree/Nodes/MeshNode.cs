using OpenTK.Graphics.OpenGL4;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Rendering;
using Matrix4 = OpenTK.Mathematics.Matrix4;

namespace VeroEngine.Core.NodeTree.Nodes;

public class MeshNode : Node
{
    private string _material = "empty";
    private string _model;
    private RenderMesh _renderMesh;
    public bool DepthTest { get; set; } = true;
    public bool Wireframe { get; set; } = false;

    public string Model
    {
        set
        {
            _model = value;
            SetMesh(RenderMesh.FromModelFile(value)); // FIRE
        }
        get => _model;
    }

    public string Material
    {
        set
        {
            _material = value;
            _renderMesh?.Material?.Dispose();
            if (_renderMesh != null) _renderMesh.Material = RenderMaterial.Load(value);
        }
        get => _material;
    }

    public void SetMesh(RenderMesh mesh)
    {
        _renderMesh?.Dispose();
        _renderMesh = mesh;
    }

    public override Node Duplicate()
    {
        var meshNode = base.Duplicate() as MeshNode;
        meshNode.Model = Model;
        return meshNode;
    }

    public override void Create()
    {
        Model = "Models/cube.obj";
    }
    

    public override void Draw()
    {
        if (!Visible) return;
        if (!DepthTest) GL.Disable(EnableCap.DepthTest);
        var proj = Collections.RootTree.SceneCamera.GetProjectionMatrix();
        var view = Collections.RootTree.SceneCamera.GetViewMatrix();
        var translation = GlobalPosition.GetTranslationMatrix();
        var rotation = GlobalRotation.GetRotationMatrix();
        var scale = Matrix4.CreateScale(GlobalScale.ToOpenTK());

        var model = scale * rotation * translation;
        _renderMesh?.Render(model, view, proj, Color, Wireframe);
        GL.Enable(EnableCap.DepthTest);
        base.Draw();
    }

    public override void Destroy()
    {
        _renderMesh?.Dispose();
        base.Destroy();
    }
}