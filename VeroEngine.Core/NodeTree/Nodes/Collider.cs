using System.IO;
using OpenTK.Mathematics;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Rendering;
using Vector3 = VeroEngine.Core.Mathematics.Vector3;

namespace VeroEngine.Core.NodeTree.Nodes;

public enum CollisionShape
{
	COLLISION_SHAPE_MESH,
	COLLISION_SHAPE_SPHERE,
	COLLISION_SHAPE_BOX
}

public class Collider : Node
{
	private RenderMesh _mesh;

	private string _meshPath;
	private CollisionShape _shape;

	public CollisionShape Shape
	{
		get => _shape;
		set
		{
			_mesh?.Dispose();
			_mesh = null;
			_shape = value;
			switch (_shape)
			{
				case CollisionShape.COLLISION_SHAPE_MESH:
					_mesh = RenderMesh.FromModelFile(MeshPath);
					break;
				case CollisionShape.COLLISION_SHAPE_SPHERE:
					_mesh = RenderMesh.FromModelFile(Path.Combine("Editor", "debug_sphere.obj"));
					break;
				case CollisionShape.COLLISION_SHAPE_BOX:
					_mesh = RenderMesh.FromModelFile(Path.Combine("Editor", "debug_cube.obj"));
					break;
			}
		}
	}

	public string MeshPath
	{
		get => _meshPath;
		set
		{
			_meshPath = value;
			if (Shape == CollisionShape.COLLISION_SHAPE_MESH)
			{
				_mesh?.Dispose();
				_mesh = null;
				_mesh = RenderMesh.FromModelFile(MeshPath);
			}
		}
	}

	public override void Create()
	{
		Shape = CollisionShape.COLLISION_SHAPE_BOX;
		base.Create();
	}

	public override void Update(double delta, bool editorHint)
	{
		if (Shape == CollisionShape.COLLISION_SHAPE_SPHERE)
		{
			var a = (Scale.X + Scale.Y + Scale.Z) / 3;
			Scale = new Vector3(a);
		}

		base.Update(delta, editorHint);
	}

	public override void Draw()
	{
		// draw collider
		if (_shape == null)
		{
			base.Draw();
			return;
		}

		if (!Visible) return;
		var proj = Collections.RootTree.SceneCamera.GetProjectionMatrix();
		var view = Collections.RootTree.SceneCamera.GetViewMatrix();
		var translation = GlobalPosition.GetTranslationMatrix();
		var rotation = GlobalRotation.GetRotationMatrix();
		var scale = Matrix4.CreateScale(GlobalScale.ToOpenTK());

		var model = scale * rotation * translation;
		_mesh?.Render(model, view, proj, new Vector3(0, 0, 255), true, 0);
		base.Draw();
	}
}