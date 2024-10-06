using BepuPhysics;
using BepuPhysics.Collidables;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Mathematics;

namespace VeroEngine.Core.NodeTree.Nodes;

public class StaticBody : Node
{
	private StaticDescription _thisDescription;
	public StaticHandle Handle;
	public string InheritedMeshPath;
	public CollisionShape InheritedShape;
	public Vector3 ShapeRotation;
	public Vector3 ShapeSize;
	public bool Valid;

	public override void ChildAdded(Node node)
	{
		if (Valid)
			return;
		if (!Collections.InEditorHint)
		{
			Log.Info(HasChild(typeof(Collider)).ToString());
			if (HasChild(typeof(Collider)))
			{
				InheritedShape = ((Collider)GetChild(typeof(Collider))).Shape;
				ShapeRotation = ((Collider)GetChild(typeof(Collider))).GlobalRotation;
				ShapeSize = ((Collider)GetChild(typeof(Collider))).GlobalScale;
				InheritedMeshPath = ((Collider)GetChild(typeof(Collider))).MeshPath;


				Valid = true;
			}
			else
			{
				Valid = false;
			}

			if (Valid)
			{
				switch (InheritedShape)
				{
					case CollisionShape.COLLISION_SHAPE_BOX:
						_thisDescription = new StaticDescription(new RigidPose(GlobalPosition.ToSystem(), GlobalRotation.ToSysQuaternion()),
							Collections.RootTree.Physics.Simulation.Shapes.Add(new Box(ShapeSize.Z, ShapeSize.Y,
								ShapeSize.X)));
						break;
					case CollisionShape.COLLISION_SHAPE_SPHERE:
						_thisDescription = new StaticDescription(new RigidPose(GlobalPosition.ToSystem(), GlobalRotation.ToSysQuaternion()),
							Collections.RootTree.Physics.Simulation.Shapes.Add(new Sphere(ShapeSize.X)));
						break;
				}

				Handle = Collections.RootTree.Physics.Simulation.Statics.Add(_thisDescription);
			}
		}
	}

	public override void Destroy()
	{
		// Deregister static body
		if (Valid && !Collections.InEditorHint) Collections.RootTree.Physics.Simulation.Statics.Remove(Handle);
		base.Destroy();
	}

	public override void Update(double delta, bool editorHint)
	{
		if (Valid && !Collections.InEditorHint)
		{
			var b = Collections.RootTree.Physics.Simulation.Statics[Handle];
			if (b.Exists)
			{
				Position = Vector3.FromSystem(b.Pose.Position);
				Rotation = Vector3.FromQuaternion(b.Pose.Orientation);
			}
		}
		base.Update(delta, editorHint);
	}
}