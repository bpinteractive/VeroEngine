using System;
using System.Reflection;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Mathematics;
using VeroEngine.Core.NodeTree.Nodes;
using VeroEngine.Core.Rendering;

namespace VeroEngine.Core.NodeTree;

// Everything
public class SceneTree
{
	private Node _root;
	public ScenePhysics Physics;

	public Camera SceneCamera;

	public SceneTree(float aspect)
	{
		RemoveAll(aspect);
	}

	public void RemoveAll(float aspect)
	{
		_root?.Destroy();
		SceneCamera = null;
		_root = new Node();
		_root.Name = "root";
		SceneCamera = new Camera(
			new Vector3(0, 0, 0),
			0,
			0,
			0,
			(float)Util.Deg2Rad(90.0),
			aspect,
			0.1f,
			1000f
		);
		Physics?.Cleanup();
		Physics = new ScenePhysics();
		Physics.Init();
	}

	public static Node CreateNode(string nodeType, Assembly customAssembly)
	{
		var type = GetTypeFromAssembly(typeof(VeroWindow).Assembly, "VeroEngine.Core.NodeTree.Nodes." + nodeType)
		           ?? GetTypeFromAssembly(customAssembly, "ScriptingAssembly.Nodes." + nodeType);

		if (type != null && Activator.CreateInstance(type) is Node instance)
		{
			instance.Name = "InstancedNode";
			return instance;
		}

		return null;
	}

	private static Type GetTypeFromAssembly(Assembly assembly, string typeName)
	{
		return assembly?.GetType(typeName);
	}


	public Node GetRoot()
	{
		return _root;
	}

	public void SetRoot(Node root)
	{
		_root.Destroy();
		_root = root;
	}

	public void DrawChildren(double deltaTime)
	{
		Physics?.Tick(deltaTime);
		_root.Update(deltaTime, Collections.InEditorHint);
		_root.Draw();
	}

	public void AddChild(Node child)
	{
		_root.AddChild(child);
	}

	public void Destroy()
	{
		Physics?.Cleanup();
		_root.Destroy();
		GC.SuppressFinalize(this);
	}
}