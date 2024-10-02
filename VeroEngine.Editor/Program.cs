using System;
using ImGuiNET;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Mathematics;
using VeroEngine.Core.NodeTree;
using VeroEngine.Core.NodeTree.Nodes;
using VeroEngine.Core.Rendering;

namespace VeroEngine.Editor
{
    internal class Program
    {
        public static void Main()
        {
            var wnd = new VeroWindow("Vero Editor");
            wnd.OnReady += () =>
            {
                // Override some app settings
                Collections.AppConfig.Display.EnableUiDock = true;
                Collections.AppConfig.Display.VSync = true;
                Collections.AppConfig.Display.FullScreen = false;
                var gizmo = new GizmoNode();
                gizmo.Name = "gizmo";
                Collections.RootTree.AddChild(gizmo);
            };
            wnd.OnDrawGui += (delta) =>
            {
                    ImGui.Begin("Node Tree");
                    if (ImGui.TreeNode("SceneTree"))
                    {
                        IterateNode(wnd.SceneTree.GetRoot());
                        ImGui.TreePop();
                    }

                    ImGui.End();

                    ImGui.Begin("Camera");
                    System.Numerics.Vector3 campos = wnd.SceneTree.SceneCamera.GetPosition().ToSystem();
                    System.Numerics.Vector3 camrot = Util.Rad2Deg(wnd.SceneTree.SceneCamera.GetRotation()).ToSystem();
                    float fov = (float)Util.Rad2Deg(wnd.SceneTree.SceneCamera.GetFieldOfView());
                    ImGui.DragFloat3("Position", ref campos, 0.01f);
                    ImGui.DragFloat3("Rotation (degrees)", ref camrot, 0.1f);
                    ImGui.DragFloat("FOV (degrees)", ref fov, 0.1f);
                    ImGui.End();
                    wnd.SceneTree.SceneCamera.SetFieldOfView((float)Util.Deg2Rad(fov));
                    wnd.SceneTree.SceneCamera.UpdatePosition(Vector3.FromSystem(campos));
                    wnd.SceneTree.SceneCamera.SetRotation((float)Util.Deg2Rad(camrot.X),
                        (float)Util.Deg2Rad(camrot.Y),
                        (float)Util.Deg2Rad(camrot.Z));
            };

            wnd.Run();
        }
        
        public static void IterateNode(Node n, bool runnode = true)
        {
            if(runnode)
            {
                if (ImGui.TreeNode(n.Name))
                {
                    ImGui.Text("Class = " + n.GetType().Name);
                    foreach (var prop in n.GetType().GetProperties())
                    {
                        if(prop.GetValue(n) != null)
                        {
                            var val = prop.GetValue(n).ToString();
                            ImGui.Text(prop.Name + " = " + val);
                        }
                        else
                        {
                            ImGui.Text(prop.Name + " = null");
                        }
                    }
                    foreach (var child in n.Children)
                    {
                        if (ImGui.TreeNode(child.Name))
                        {
                            IterateNode(child, false);
                            ImGui.TreePop();
                        }
                    }

                    ImGui.TreePop();
                }
            }
            else
            {
                ImGui.Text("Class = " + n.GetType().Name);
                foreach (var prop in n.GetType().GetProperties())
                {
                    if(prop.GetValue(n) != null)
                    {
                        var val = prop.GetValue(n).ToString();
                        ImGui.Text(prop.Name + " = " + val);
                    }
                    else
                    {
                        ImGui.Text(prop.Name + " = null");
                    }
                }
                foreach (var child in n.Children)
                {
                    if (ImGui.TreeNode(child.Name))
                    {
                        IterateNode(child, false);
                        ImGui.TreePop();
                    }
                }
            }
        }
    }
}
