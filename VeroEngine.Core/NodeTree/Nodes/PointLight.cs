using System;
using System.IO;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Rendering;
using Vector3 = VeroEngine.Core.Mathematics.Vector3;

namespace VeroEngine.Core.NodeTree.Nodes;

public class PointLight : Node
{
    public float Intensity { get; set; }
    public int DepthCubemap;
    public int DepthFbo;
    
    private RenderMesh _renderMesh;

    public override void Create()
    {
        base.Create();
        Collections.SceneLights.Add(this);
        _renderMesh = RenderMesh.FromModelFile(Path.Combine("Models", "cube.obj"));
        _renderMesh.Material = RenderMaterial.Load(Path.Combine("Editor", "light"));
    }

    public override void Destroy()
    {
        base.Destroy();
        Collections.SceneLights.Remove(this);
        
        // Clean up the framebuffer and cubemap
        if (DepthFbo != 0)
        {
            GL.DeleteFramebuffer(DepthFbo);
            DepthFbo = 0;
        }

        if (DepthCubemap != 0)
        {
            GL.DeleteTexture(DepthCubemap);
            DepthCubemap = 0;
        }
    }

    public override Node Duplicate()
    {
        PointLight a = (PointLight)base.Duplicate();
        a.Intensity = Intensity;
        return a;
    }
    

    public override void Draw()
    {
        if (Collections.ActuallyInEditor)
        {
            var proj = Collections.RootTree.SceneCamera.GetProjectionMatrix();
            var view = Collections.RootTree.SceneCamera.GetViewMatrix();
            var translation = GlobalPosition.GetTranslationMatrix();
            var scale = Matrix4.CreateScale(.5f, .5f, .5f);
            var rot = Matrix4.Identity;
        
            _renderMesh?.Render(scale * rot * translation, view, proj, Color * Intensity);
        }
        base.Draw();
    }

}
