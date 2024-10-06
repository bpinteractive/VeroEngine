using BepuPhysics;
using BepuPhysics.Collidables;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Mathematics;

namespace VeroEngine.Core.NodeTree.Nodes;

public class RigidBody : Node
{
	private BodyDescription _thisDescription;
	public BodyHandle Handle;
	public string InheritedMeshPath;
	public CollisionShape InheritedShape;
	public Vector3 ShapeRotation;
	public Vector3 ShapeSize;
	public bool Valid;
	public float Mass { get; set; } = 1f;

	public override void ChildAdded(Node n)
	{
		if (Valid) return;
		if (!Collections.InEditorHint)
		{
			Log.Info(HasChild(typeof(Collider)).ToString());
			if (HasChild(typeof(Collider)))
			{
				InheritedShape = ((Collider)GetChild(typeof(Collider))).Shape;
				ShapeRotation = ((Collider)GetChild(typeof(Collider))).GlobalRotation;
				ShapeSize = ((Collider)GetChild(typeof(Collider))).GlobalScale * 10;
				InheritedMeshPath = ((Collider)GetChild(typeof(Collider))).MeshPath;
				Log.Info(InheritedShape.ToString());
				Log.Info(ShapeRotation.ToString());
				Log.Info(ShapeSize.ToString());


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
						var inee = new Box(ShapeSize.Z, ShapeSize.Y, ShapeSize.X);
						var interiaa = inee.ComputeInertia(Mass);
						_thisDescription = BodyDescription.CreateDynamic(new RigidPose(GlobalPosition.ToSystem(), GlobalRotation.ToSysQuaternion()), interiaa,
							Collections.RootTree.Physics.Simulation.Shapes.Add(inee), 0.001f);
						break;
					case CollisionShape.COLLISION_SHAPE_SPHERE:
						var ine = new Sphere(ShapeSize.X);
						var interia = ine.ComputeInertia(Mass);
						_thisDescription = BodyDescription.CreateDynamic(new RigidPose(GlobalPosition.ToSystem(), GlobalRotation.ToSysQuaternion()), interia,
							Collections.RootTree.Physics.Simulation.Shapes.Add(ine), 0.001f);
						break;
				}
				
				Handle = Collections.RootTree.Physics.Simulation.Bodies.Add(_thisDescription);
			}
		}
	}

	public override void Destroy()
	{
		// Deregister static body
		if (Valid && !Collections.InEditorHint) Collections.RootTree.Physics.Simulation.Bodies.Remove(Handle);
		base.Destroy();
	}

	public override void Update(double delta, bool editorHint)
	{
		if (Valid && !Collections.InEditorHint)
		{
			var bodyReference = Collections.RootTree.Physics.Simulation.Bodies.GetBodyReference(Handle);
			if (bodyReference.Exists)
			{
				Position = Vector3.FromSystem(bodyReference.Pose.Position);
				Rotation = Vector3.FromQuaternion(bodyReference.Pose.Orientation);
			}
		}

		base.Update(delta, editorHint);
	}
}