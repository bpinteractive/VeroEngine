using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ImGuiNET;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ScriptingAssembly;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Mathematics;
using VeroEngine.Core.NodeTree;
using VeroEngine.Core.NodeTree.Nodes;
using VeroEngine.Core.Rendering;
using Vector3 = VeroEngine.Core.Mathematics.Vector3;

namespace VeroEngine.Editor;

internal class Program
{
    private static bool _newNodeOpenMenu;
    private static string _newNodeName = "Node";
    private static string _newNodeClass = "Node";
    private static Node _newNodeParent;

    private static bool _gameSettingsMenuOpen = false;
    private static bool _settingsMenuOpen = false;

    // Camera movement variables
    private static readonly float _cameraSpeed = 5.0f; // Speed of the camera movement
    private static bool _isRightMousePressed;
    private static Vector2 _lastMousePosition;

    private static void PopupNewNodeMenu()
    {
        _newNodeOpenMenu = true;
        _newNodeName = "New";
        _newNodeClass = "Node";
    }

    public static void Main()
    {
        var wnd = new VeroWindow();
        MeshNode node;
        wnd.OnReady += () =>
        {
            Collections.AppConfig.Display.EnableUiDock = true;
            Collections.AppConfig.Display.FullScreen = false;
            
            // Setup imgui styling here
            ImGui.GetStyle().WindowRounding = 4;
            ImGui.GetStyle().ChildRounding = 4;
            ImGui.GetStyle().FrameRounding = 3;
            ImGui.GetStyle().PopupRounding = 6;
            ImGui.GetStyle().ScrollbarRounding = 12;
            ImGui.GetStyle().GrabRounding = 6;
            ImGui.GetStyle().TabRounding = 6;

            ImGui.GetStyle().WindowTitleAlign.X = 0;
            ImGui.GetStyle().WindowMenuButtonPosition = ImGuiDir.None;
            ImGui.GetStyle().TabBarBorderSize = 2;
            
        };

        wnd.OnDraw += delta =>
        {
            wnd.SetTitle("Vero Editor: " + Collections.AppConfig.Title);
            _isRightMousePressed = false;
            // wnd.ReleaseMouse();
            if (Keyboard.KeyPress(Keys.LeftAlt))
            {
                if (Mouse.RightButtonDown)
                {
                    // wnd.GrabMouse();
                    _isRightMousePressed = true;

                    var mousePosition = Mouse.Position;
                    if (_lastMousePosition == default) _lastMousePosition = mousePosition;

                    var mouseDelta = mousePosition - _lastMousePosition;
                    Log.Info(mouseDelta.ToString());

                    var camrot = wnd.SceneTree.SceneCamera.GetRotation();
                    camrot.X += (float)Util.Deg2Rad(mouseDelta.X * 0.1f);
                    camrot.Y -= (float)Util.Deg2Rad(mouseDelta.Y * 0.1f);
                    wnd.SceneTree.SceneCamera.SetRotation(camrot);

                    _lastMousePosition = mousePosition;
                }
                else
                {
                    _isRightMousePressed = false;
                    _lastMousePosition = default;
                }
            }

            if (_isRightMousePressed)
            {
                var camPos = wnd.SceneTree.SceneCamera.GetPosition().ToSystem();

                if (Keyboard.KeyPress(Keys.W))
                    camPos += wnd.SceneTree.SceneCamera.GetFront().ToSystem() * _cameraSpeed * (float)delta;

                if (Keyboard.KeyPress(Keys.S))
                    camPos -= wnd.SceneTree.SceneCamera.GetFront().ToSystem() * _cameraSpeed * (float)delta;

                if (Keyboard.KeyPress(Keys.A))
                    camPos -= wnd.SceneTree.SceneCamera.GetRight().ToSystem() * _cameraSpeed * (float)delta;

                if (Keyboard.KeyPress(Keys.D))
                    camPos += wnd.SceneTree.SceneCamera.GetRight().ToSystem() * _cameraSpeed * (float)delta;

                wnd.SceneTree.SceneCamera.UpdatePosition(Vector3.FromSystem(camPos));
            }
        };

        wnd.OnDrawGui += delta =>
        {
            if (_gameSettingsMenuOpen)
            {
                ImGui.Begin("App Settings", ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize);
                var title = Collections.AppConfig.Title;
                var user = Collections.AppConfig.UserData;
                Vector2 res = new(Collections.AppConfig.Resolution.Width,
                    Collections.AppConfig.Resolution.Height);
                var siz = res.ToSystem();
                var vsync = Collections.AppConfig.Display.VSync;
                var fullscreen = Collections.AppConfig.Display.FullScreen;

                if (ImGui.InputText("Title", ref title, 256)) Collections.AppConfig.Title = title;

                if (ImGui.InputText("User Folder", ref user, 256)) Collections.AppConfig.UserData = user;
                
                
                if (ImGui.DragFloat2("Resolution", ref siz))
                {
                    Collections.AppConfig.Resolution.Width = (int)siz.X;
                    Collections.AppConfig.Resolution.Height = (int)siz.Y;
                }
                if (ImGui.Checkbox("Vsync", ref vsync)) Collections.AppConfig.Display.VSync = vsync;
                if (ImGui.Checkbox("Fullscreen", ref fullscreen)) Collections.AppConfig.Display.FullScreen = fullscreen;


                if (ImGui.Button("Save"))
                {
                    string serial = Collections.AppConfig.Serialize();
                    File.WriteAllText(Path.Combine("Game", "App.json"), serial);
                    Log.Info("Wrote AppConfig");
                    _gameSettingsMenuOpen = false;
                }
                ImGui.SameLine();
                if (ImGui.Button("Cancel"))
                {
                    _gameSettingsMenuOpen = false;
                }
                ImGui.End();
            }

            if (_settingsMenuOpen)
            {
                ImGui.Begin("Editor Settings");
                if (ImGui.Button("Close"))
                {
                    _settingsMenuOpen = false;
                }
                ImGui.Text("If you dont know what your doing this is useless!!!");
                ImGui.ShowStyleEditor();
                ImGui.End();
            }

            ImGui.Begin("Content");
            ImGui.End();
            ImGui.Begin("Console");
            ImGui.TextColored(new(255, 0, 0, 255), "hi");
            ImGui.End();
            ImGui.Begin("Actions");
            ImGui.Button("Play");
            ImGui.SameLine();
            if (ImGui.Button("Settings")) _gameSettingsMenuOpen = true;
            ImGui.SameLine();
            if (ImGui.Button("Editor Settings")) _settingsMenuOpen = true;
            ImGui.SameLine();
            if (ImGui.Button("Clear Cache"))
            {
                // Clear pipeline cache for shaders so we can recompile them easily
                Log.Info("Clearing Pipeline Cache");
                Directory.Delete(Shader.GetCachePath(), true);
            }
            ImGui.End();
            ImGui.Begin("Node Tree");
            IterateNode(wnd.SceneTree.GetRoot());
            ImGui.End();

            if (_newNodeOpenMenu)
            {
                ImGui.Begin("New Node", ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize);

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

                ImGui.SameLine();
                if (ImGui.Button("Cancel")) _newNodeOpenMenu = false;

                ImGui.End();
            }
        };

        wnd.Run();
    }

    public static void RunProperty(PropertyInfo prop, Node n)
    {
        var val = prop.GetValue(n);
        switch (prop.PropertyType.Name.ToLower())
        {
            case "vector3":
                if (prop.Name.ToLower() == "color")
                {
                    var sysvec2 = ((Vector3)val).ToSystem();
                    ImGui.ColorEdit3(prop.Name, ref sysvec2);
                    prop.SetValue(n, Vector3.FromSystem(sysvec2));
                    break;
                }

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
                if (ImGui.Button("Duplicate")) n.Duplicate();

                if (ImGui.Button("Destroy")) n.Destroy();
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