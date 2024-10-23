using System;
using System.Linq;
using System.Reflection;
using OpenTK.Graphics.OpenGL4;
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
		_root.Name = "Workspace";
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
		foreach (var s in Shader.InternalCache.ToList())
		{
			s.Value.Dispose();
		}
		Shader.InternalCache.Clear();
		
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

	    // // DRAW SHADOW PASS FIRST
	    // foreach (var light in Collections.SceneLights)
	    // {
	    //     if (light is { } pointLight)
	    //     {
	    //         if (pointLight.DepthFbo == 0)
	    //         {
	    //             pointLight.SetupShadowMapping();
	    //         }
	    //         
	    //         GL.BindFramebuffer(FramebufferTarget.Framebuffer, pointLight.DepthFbo);
	    //         GL.Viewport(0, 0, 1024, 1024);
	    //         
	    //         GL.Clear(ClearBufferMask.DepthBufferBit);
	    //         
	    //         SceneCamera.SetPosition(pointLight.Position);
	    //
	    //         // Set rotation angles for each face of the cubemap
	    //         var faceRotations = new Vector3[]
	    //         {
	    //             new Vector3(0.0f, 0.0f, 0.0f),        // Positive X
	    //             new Vector3(0.0f, 180.0f * (float)Math.PI / 180.0f, 0.0f), // Negative X
	    //             new Vector3(90.0f * (float)Math.PI / 180.0f, 0.0f, 0.0f),  // Positive Y
	    //             new Vector3(-90.0f * (float)Math.PI / 180.0f, 0.0f, 0.0f), // Negative Y
	    //             new Vector3(0.0f, 0.0f, 90.0f * (float)Math.PI / 180.0f),  // Positive Z
	    //             new Vector3(0.0f, 0.0f, -90.0f * (float)Math.PI / 180.0f)  // Negative Z
	    //         };
	    //         
	    //         for (int i = 0; i < 6; i++)
	    //         {
	    //             SceneCamera.SetRotation(faceRotations[i]);
	    //             
	    //             Collections.IsShadowPass = true;
	    //             _root.Draw();
	    //         }
	    //         
	    //     }
	    // }
	    // Collections.IsShadowPass = false;
	    // GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
	    // no...
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