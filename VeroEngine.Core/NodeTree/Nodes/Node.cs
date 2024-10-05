using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using Vector3 = VeroEngine.Core.Mathematics.Vector3;

namespace VeroEngine.Core.NodeTree.Nodes;

// Base class of everything
public class Node : IDisposable
{
    private readonly List<Node> _children;
    private bool _disposed; // Tracks whether the node has been disposed.

    public Vector3 GlobalPosition { get; set; } = new(0, 0, 0);
    public Vector3 GlobalRotation { get; set; } = new(0, 0, 0);

    public Vector3 GlobalScale { get; set; } = new(1, 1, 1);
    public Node Parent;

    public Node()
    {
        _children = new List<Node>();
        Create();
    }

    public bool Visible { get; set; } = true;
    public string Name { get; set; } = "UnknownNode";
    public Vector3 Position { get; set; } = new(0, 0, 0);
    public Vector3 Rotation { get; set; } = new(0, 0, 0);
    public Vector3 Scale { get; set; } = new(1, 1, 1);
    public Vector3 Color { get; set; } = new(1, 1, 1);

    public IReadOnlyList<Node> Children => _children;

    // IDisposable Implementation
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public Node GetChild(string name)
    {
        return _children.FirstOrDefault(node => node.Name == name);
    }

    public override string ToString()
    {
        return Name;
    }

    public void AddChild(Node child)
    {
        var baseName = child.Name;
        var count = 1;

        while (_children.Any(node => node.Name == child.Name)) child.Name = $"{baseName}_{count++}";
        
        child.Parent?.RemoveChild(child);
        child.Parent = this;
        _children.Add(child);
    }

    public void RemoveChild(Node child)
    {
        _children.Remove(child);
        child.Parent = null;
    }

    public virtual void Create()
    {
    }

    public virtual void Destroy()
    {
        // Call Dispose when Destroy is invoked
        Dispose();
    }

    public virtual void Draw()
    {
        if (!Visible) return;
        foreach (var child in _children) child.Draw();
    }

    public virtual void Update(double delta, bool editorHint)
    {
        foreach (var child in _children) child.Update(delta, editorHint);

        if (Parent != null)
        {
            Matrix4 parentRotationMatrix = Parent.GlobalRotation.GetRotationMatrix();
            
            Vector3 rotatedPosition = (Position * Parent.GlobalScale).Transform(parentRotationMatrix);
            GlobalScale = Parent.GlobalScale * Scale;
            GlobalPosition = Parent.GlobalPosition + rotatedPosition;
            GlobalRotation = Parent.GlobalRotation + Rotation;
        }
        else
        {
            GlobalPosition = Position;
            GlobalRotation = Rotation;
            GlobalScale = Scale;
        }
    }


    public virtual Node Duplicate()
    {
        var copy = (Node)Activator.CreateInstance(GetType());

        copy.Name = Name;
        copy.Position = Position;
        copy.Rotation = Rotation;
        copy.Color = Color;
        copy.Visible = Visible;

        foreach (var child in _children.ToList())
        {
            var childCopy = child.Duplicate();
            Parent.RemoveChild(childCopy);
            copy.AddChild(childCopy);
        }

        Parent?.AddChild(copy);
        return copy;
    }

    // Protected Dispose method to handle resource cleanup
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (Parent != null) Parent.RemoveChild(this);

                foreach (var child in new List<Node>(_children)) child.Dispose();

                _children.Clear();
            }
            

            _disposed = true;
        }
    }

    // Destructor (Finalizer) to catch non-disposed objects
    ~Node()
    {
        Dispose(false);
    }
}