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
	private RenderMesh _cameraMesh;
	private Camera _renderCamera;

	public float FOV { get; set; } = 90;
	public bool Orthographic { get; set; } = false;

	public override void Create()
	{
		_renderCamera = Collections.RootTree.SceneCamera;
		_cameraMesh = RenderMesh.FromModelFile(Path.Combine("Editor", "camera.obj"));
	}

	public override void Update(double delta, bool editorHint)
	{
		FOV = Math.Clamp(FOV, 10.0f, 179.9f);
		base.Update(delta, editorHint);
	}

	public override void Draw()
	{
		base.Draw();
		if (Collections.IsShadowPass) return;
		if (Collections.InEditorHint || !Visible)
		{
			var proj = Collections.RootTree.SceneCamera.GetProjectionMatrix();
			var view = Collections.RootTree.SceneCamera.GetViewMatrix();
			var translation = GlobalPosition.GetTranslationMatrix();
			var rotation = GlobalRotation.GetRotationMatrix();

			var scaleFactorX = 1.0f / MathF.Tan((float)Util.Deg2Rad(FOV) / 2);
			var scaleFactorY = FOV / 90;
			var scaleFactorZ = FOV / 90;
			var scale = Matrix4.CreateScale(new Vector3(scaleFactorX, scaleFactorY, scaleFactorZ));


			var model = scale * rotation * translation;
			_cameraMesh.Render(model, view, proj, new Mathematics.Vector3(255, 0, 0), true);
		}

		Collections.IsCameraStolen = Visible;
		_renderCamera.Orthographic = Orthographic && Visible && !Collections.InEditorHint;
		if (!Visible) return;
		if (Collections.InEditorHint) return;
		_renderCamera.SetPosition(GlobalPosition);
		_renderCamera.SetRotation(new(-GlobalRotation.Y, GlobalRotation.Z, GlobalRotation.X));
		_renderCamera.SetFieldOfView((float)Util.Deg2Rad(FOV));
		
	}
}