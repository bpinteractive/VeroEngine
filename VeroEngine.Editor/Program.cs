using System;
using System.Linq;
using System.Reflection;
using ImGuiNET;
using ScriptingAssembly;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Mathematics;
using VeroEngine.Core.NodeTree;
using VeroEngine.Core.NodeTree.Nodes;
using VeroEngine.Core.Rendering;

namespace VeroEngine.Editor;

internal class Program
{
    private static bool _newNodeOpenMenu;
    private static string _newNodeName = "Node";
    private static string _newNodeClass = "Node";
    private static Node _newNodeParent;

    private static void PopupNewNodeMenu()
    {
        _newNodeOpenMenu = true;
        _newNodeName = "New";
        _newNodeClass = "Node";
    }

    public static void Main()
    {
        // run hangs the current thread so you need to subscribe to the Ready event
        try
        {
            var wnd = new VeroWindow();
            MeshNode node;
            wnd.OnReady += () =>
            {
                // Shader s = Shader.Load("VeroEngine.Basic");
                wnd.SetTitle("Vero Editor: " + Collections.AppConfig.Title);
            };
            wnd.OnDrawGui += delta =>
            {
                ImGui.Begin("Node Tree");


                IterateNode(wnd.SceneTree.GetRoot());

                ImGui.End();

                ImGui.Begin("Camera");
                var campos = wnd.SceneTree.SceneCamera.GetPosition().ToSystem();
                var camrot = Util.Rad2Deg(wnd.SceneTree.SceneCamera.GetRotation()).ToSystem();
                var fov = (float)Util.Rad2Deg(wnd.SceneTree.SceneCamera.GetFieldOfView());
                ImGui.DragFloat3("Position", ref campos, 0.01f);
                ImGui.DragFloat3("Rotation (degrees)", ref camrot, 0.1f);
                ImGui.DragFloat("FOV (degrees)", ref fov, 0.1f);
                ImGui.End();

                if (_newNodeOpenMenu)
                {
                    ImGui.Begin("New Node");

                    ImGui.Text("New Node");

                    ImGui.InputText("Name", ref _newNodeName, 250);
                    ImGui.InputText("Class", ref _newNodeClass, 250);
                    if (ImGui.Button("Create"))
                    {
                        var inst = SceneTree.CreateNode(_newNodeClass, ScriptingInterface.GetAssembly());
                        inst.Name = _newNodeName;
                        _newNodeParent.AddChild(inst);
                        _newNodeOpenMenu = false;
                    }

                    if (ImGui.Button("Cancel")) _newNodeOpenMenu = false;

                    ImGui.End();
                }

                wnd.SceneTree.SceneCamera.SetFieldOfView((float)Util.Deg2Rad(fov));
                wnd.SceneTree.SceneCamera.UpdatePosition(Vector3.FromSystem(campos));
                wnd.SceneTree.SceneCamera.SetRotation((float)Util.Deg2Rad(camrot.X),
                    (float)Util.Deg2Rad(camrot.Y),
                    (float)Util.Deg2Rad(camrot.Z));
            };

            wnd.Run();
        }
        catch // let the internal exception handler work
        {
        }
    }

    public static void RunProperty(PropertyInfo prop, Node n)
    {
        var val = prop.GetValue(n);
        switch (prop.PropertyType.Name.ToLower())
        {
            case "vector3":
                var sysvec = ((Vector3)val).ToSystem();
                ImGui.DragFloat3(prop.Name, ref sysvec, 0.01f);
                prop.SetValue(n, Vector3.FromSystem(sysvec));
                // n.Position = Vector3.FromSystem(sysvec);
                break;
            case "float":
                var floate = (float)val;
                ImGui.DragFloat(prop.Name, ref floate, 0.01f);
                prop.SetValue(n, floate);
                // n.Position = Vector3.FromSystem(sysvec);
                break;
            case "string":
                var str = (string)val;
                if (ImGui.InputText(prop.Name, ref str, 255)) prop.SetValue(n, str);

                // n.Position = Vector3.FromSystem(sysvec);
                break;
            case "boolean":
                var bl = (bool)val;
                ImGui.Checkbox(prop.Name, ref bl);
                prop.SetValue(n, bl);
                // n.Position = Vector3.FromSystem(sysvec);
                break;
            default:
                val = prop.GetValue(n).ToString();
                ImGui.Text(prop.Name + " = " + val);
                break;
        }
    }

    public static void IterateNode(Node n, bool runnode = true)
    {
        if (ImGui.TreeNode(n.Name))
        {
            ImGui.Text("Class = " + n.GetType().Name);
            foreach (var prop in n.GetType().GetProperties())
                try
                {
                    if (prop.GetValue(n) != null)
                        RunProperty(prop, n);
                    else
                        ImGui.Text(prop.Name + " = null");
                }
                catch (Exception e)
                {
                }

            if (n.Parent != null)
            {
                if (ImGui.Button("Duplicate"))
                {
                    n.Duplicate();
                }
                
                if (ImGui.Button("Destroy"))
                {
                    n.Destroy();
                }
            }
            
            if (ImGui.Button("New Node"))
            {
                _newNodeParent = n;
                PopupNewNodeMenu();
            }

            foreach (var child in n.Children.ToList()) IterateNode(child);

            ImGui.TreePop();
        }
    }
}