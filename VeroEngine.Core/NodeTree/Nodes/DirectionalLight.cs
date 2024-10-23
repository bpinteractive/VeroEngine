using System.IO;
using OpenTK.Mathematics;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Rendering;

namespace VeroEngine.Core.NodeTree.Nodes;

public class DirectionalLight : Node
{
	public float Intensity { get; set; }
        
        private RenderMesh _renderMesh;
    
        public override void Create()
        {
            base.Create();
            _renderMesh = RenderMesh.FromModelFile(Path.Combine("Models", "cube.obj"));
            _renderMesh.Material = RenderMaterial.Load(Path.Combine("Editor", "light"));
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
            
                _renderMesh?.Render(scale * translation, view, proj, Color * Intensity);
            }
            Collections.Registry.Set("dirlight_enabled", Visible);
            Collections.Registry.Set("dirlight_rot", GlobalRotation);
            Collections.Registry.Set("dirlight_col", Color * Intensity);
            base.Draw();
        }
}