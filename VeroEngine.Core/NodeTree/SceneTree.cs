using System;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Mathematics;
using VeroEngine.Core.NodeTree.Nodes;
using VeroEngine.Core.Rendering;

namespace VeroEngine.Core.NodeTree;

// Everything
public class SceneTree
{
    private Node _root;

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
            new Vector3(-2f, 0, -1),
            0,
            0,
            0,
            new Vector3(0, 1, 0),
            (float)Util.Deg2Rad(90.0),
            aspect,
            0.1f,
            1000f
        );
    }

    public Node GetRoot()
    {
        return _root;
    }

    public void DrawChildren(double deltaTime)
    {
        _root.Update(deltaTime, Collections.InEditorHint);
        _root.Draw();
    }

    public void AddChild(Node child)
    {
        _root.AddChild(child);
    }

    public void Destroy()
    {
        _root.Destroy();
        GC.SuppressFinalize(this);
    }
}