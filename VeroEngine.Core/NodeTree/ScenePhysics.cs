using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;

namespace VeroEngine.Core.NodeTree;

public class ScenePhysics
{
	private BufferPool _bufferPool;
	private ThreadDispatcher _threadDispatcher;

	public Simulation Simulation;

	public void Init()
	{
		_bufferPool = new BufferPool();
		Simulation = Simulation.Create(_bufferPool, new NarrowPhaseCallbacks(),
			new PoseIntegratorCallbacks(new Vector3(0, -10, 0)), new SolveDescription(8, 1));
		_threadDispatcher = new ThreadDispatcher(Environment.ProcessorCount);
	}

	public void Tick(double delta)
	{
		// Simulation.Timestep((float)delta, _threadDispatcher);
		// Simulation.Timestep((float)delta);
		Simulation?.Timestep(1 / 60f, _threadDispatcher);
		// Simulation?.Timestep(1 / 60f);
	}

	public void Cleanup()
	{
		// Remove used stuff
		Simulation.Dispose();
		_threadDispatcher.Dispose();
		_bufferPool.Clear();
	}

	private struct NarrowPhaseCallbacks : INarrowPhaseCallbacks
	{
		public void Initialize(Simulation simulation)
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b,
			ref float speculativeMargin)
		{
			return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
		{
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold,
			out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
		{
			pairMaterial.FrictionCoefficient = 1f;
			pairMaterial.MaximumRecoveryVelocity = 2f;
			pairMaterial.SpringSettings = new SpringSettings(30, 1);
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB,
			ref ConvexContactManifold manifold)
		{
			return true;
		}

		public void Dispose()
		{
		}
	}

	public struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks
	{
		public void Initialize(Simulation simulation)
		{
		}

		public readonly AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;

		public readonly bool AllowSubstepsForUnconstrainedBodies => false;
		public readonly bool IntegrateVelocityForKinematics => false;

		public Vector3 Gravity;

		public PoseIntegratorCallbacks(Vector3 gravity) : this()
		{
			Gravity = gravity;
		}


		private Vector3Wide gravityWideDt;

		public void PrepareForIntegration(float dt)
		{
			gravityWideDt = Vector3Wide.Broadcast(Gravity * dt);
		}

		public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation,
			BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt,
			ref BodyVelocityWide velocity)
		{
			velocity.Linear += gravityWideDt;
		}
	}
}