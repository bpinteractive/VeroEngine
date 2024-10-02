using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ImGuiNET;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Mathematics;
using VeroEngine.Core.NodeTree;
using VeroEngine.Core.NodeTree.Nodes;
using VeroEngine.Core.Rendering;

namespace VeroEngine.Game
{
    internal class Program
    {
        private static bool _newNodeOpenMenu = false;
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
                    wnd.SetTitle(Collections.AppConfig.Title);
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

                    if (_newNodeOpenMenu)
                    {
                        ImGui.Begin("New Node");
                        
                        ImGui.Text("New Node");
                        
                        ImGui.InputText("Name", ref _newNodeName, 250);
                        ImGui.InputText("Class", ref _newNodeClass, 250);
                        if (ImGui.Button("Create"))
                        {
                            var type = typeof(VeroWindow).Assembly.GetType("VeroEngine.Core.NodeTree.Nodes." + _newNodeClass);
                            if (type != null)
                            {
                                Node instance = Activator.CreateInstance(type) as Node;
                                if (instance != null)
                                {
                                    instance.Name = _newNodeName;
                                    _newNodeParent.AddChild(instance);
                                }
                            }
                            _newNodeOpenMenu = false;
                        }
                        if (ImGui.Button("Cancel"))
                        {
                            _newNodeOpenMenu = false;
                        }
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
            object val = prop.GetValue(n);
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
                    if (ImGui.InputText(prop.Name, ref str, 255))
                    {
                        prop.SetValue(n, str);
                    }
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
            if(runnode)
            {
                if (ImGui.TreeNode(n.Name))
                {
                    ImGui.Text("Class = " + n.GetType().Name);
                    foreach (var prop in n.GetType().GetProperties())
                    {
                        try
                        {
                            if(prop.GetValue(n) != null)
                            {
                                RunProperty(prop, n);
                            }
                            else
                            {
                                ImGui.Text(prop.Name + " = null");
                            }
                        }
                        catch (Exception e)
                        {
                        }
                        
                    }
                    if (ImGui.Button("New Node"))
                    {
                        _newNodeParent = n;
                        PopupNewNodeMenu();
                    }
                    foreach (var child in n.Children.ToList())
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
                    try
                    {
                        if(prop.GetValue(n) != null)
                        {
                            RunProperty(prop, n);
                        }
                        else
                        {
                            ImGui.Text(prop.Name + " = null");
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }
                
                if (ImGui.Button("New Node"))
                {
                    _newNodeParent = n;
                    PopupNewNodeMenu();
                }
                
                if (ImGui.Button("Duplicate"))
                {
                    n.Duplicate();
                }
                
                if (ImGui.Button("Destroy"))
                {
                    n.Destroy();
                }
                
                foreach (var child in n.Children.ToList())
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
