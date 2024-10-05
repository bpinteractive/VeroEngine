using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using ImGuiNET;
using OpenTK.Platform.Windows;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ScriptingAssembly;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Mathematics;
using VeroEngine.Core.NodeTree;
using VeroEngine.Core.NodeTree.Nodes;
using VeroEngine.Core.Rendering;

using NativeFileDialogExtendedSharp;

using Vector3 = VeroEngine.Core.Mathematics.Vector3;

namespace VeroEngine.Editor;

internal class Program
{
    private static VeroWindow _wnd;
    
    private static Node _currentNode;
    
    private static string _currentScene = "";
    
    private static bool _newNodeOpenMenu;
    private static string _newNodeName = "Node";
    private static string _newNodeClass = "Node";
    private static Node _newNodeParent;

    private static bool _gameSettingsMenuOpen = false;
    private static bool _settingsMenuOpen = false;

    private static bool _setParentOpen = false;
    private static int _newParentIndex;

    private static bool _renameNodeOpen = false;
    private static string _renameNodeName = "Node";

    // Camera movement variables
    private static readonly float _cameraSpeed = 5.0f; // Speed of the camera movement
    private static bool _isRightMousePressed;
    private static Vector2 _lastMousePosition;

    private static string _currentFilesystemDirectory = Path.GetFullPath(Path.Combine("Game", "Content"));
    private static List<SerialisedFsObject> _serialisedObjects = new();

    private static void PopupNewNodeMenu()
    {
        _newNodeOpenMenu = true;
        _newNodeName = "New";
        _newNodeClass = "Node";
    }
    
    private static void PopupReparent(Node n)
    {
        _setParentOpen = true;
        _currentNode = n;
        _renameNodeOpen = false;
    }
    
    private static void PopupRename(Node n)
    {
        _renameNodeOpen = true;
        _currentNode = n;
        _renameNodeName = n.Name;
    }

    public static void Main()
    {
        _wnd = new VeroWindow();
        MeshNode node;
        _wnd.OnReady += () =>
        {
            Collections.AppConfig.Display.EnableUiDock = true;
            Collections.AppConfig.Display.FullScreen = false;
            
            Collections.InEditorHint = true;
            
            _wnd.SetTitle("Vero Editor: " + Collections.AppConfig.Title);
            
            Collections.ScriptingAssembly = ScriptingInterface.GetAssembly();
            
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
            
            RefreshFilesystem();
        };

        _wnd.OnDraw += delta =>
        {
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
                    // Log.Info(mouseDelta.ToString());

                    var camrot = _wnd.SceneTree.SceneCamera.GetRotation();
                    camrot.Y -= (float)Util.Deg2Rad(mouseDelta.X * 0.1f);
                    camrot.Z += (float)Util.Deg2Rad(mouseDelta.Y * 0.1f);
                    _wnd.SceneTree.SceneCamera.SetRotation(camrot);

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
                var camPos = _wnd.SceneTree.SceneCamera.GetPosition().ToSystem();

                if (Keyboard.KeyPress(Keys.W))
                    camPos += _wnd.SceneTree.SceneCamera.GetFront().ToSystem() * _cameraSpeed * (float)delta;

                if (Keyboard.KeyPress(Keys.S))
                    camPos -= _wnd.SceneTree.SceneCamera.GetFront().ToSystem() * _cameraSpeed * (float)delta;

                if (Keyboard.KeyPress(Keys.A))
                    camPos -= _wnd.SceneTree.SceneCamera.GetRight().ToSystem() * _cameraSpeed * (float)delta;

                if (Keyboard.KeyPress(Keys.D))
                    camPos += _wnd.SceneTree.SceneCamera.GetRight().ToSystem() * _cameraSpeed * (float)delta;

                _wnd.SceneTree.SceneCamera.UpdatePosition(Vector3.FromSystem(camPos));
            }
        };
        _wnd.FileDrop += args =>
        {
            // load the file as a scene
            LoadScene(args.FileNames[0]);
        };
        _wnd.OnDrawGui += delta =>
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("Scene"))
                {
                    if (ImGui.MenuItem("Save", "Ctrl+S"))
                    {
                        // save
                        if (_currentScene != "")
                        {
                            SaveScene(_currentScene);
                        }
                        else
                        {
                            SaveAsSceneMenu();
                        }
                    }
                    if (ImGui.MenuItem("Save As", "Ctrl+Shift+S"))
                    {
                        SaveAsSceneMenu();
                    }
                    if (ImGui.MenuItem("Open", "Ctrl+O"))
                    {
                        LoadSceneMenu();
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar();
            }

            if (_renameNodeOpen)
            {
                ImGui.Begin("Rename Node", ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.InputText("Name", ref _newNodeName, 128);
                if (ImGui.Button("Rename"))
                {
                    _currentNode.Name = _newNodeName;
                    _renameNodeOpen = false;
                }
                ImGui.SameLine();
                if (ImGui.Button("Cancel"))
                { 
                    _renameNodeOpen = false;
                }
                ImGui.End();
            }
            if (_setParentOpen)
            {
                ImGui.Begin("Set Parent", ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.Text(_currentNode.Name + "->" + _currentNode.Parent.Name);
                List<string> items = _currentNode.Parent.Children.Select(c => c.Name).ToList();
                items.Insert(0, _currentNode.Parent.Name);
                ImGui.Combo("New Parent", ref _newParentIndex, items.ToArray(), items.Count);
                if (ImGui.Button("Reparent"))
                {
                    if (_newParentIndex == items.IndexOf(_currentNode.Parent.Name))
                    {
                        _setParentOpen = false;
                    }
                    else
                    {
                        var p = _currentNode.Parent.GetChild(items[_newParentIndex]);
                        p.AddChild(_currentNode);
                        _setParentOpen = false;
                    }
                    
                }
                ImGui.SameLine();
                if (ImGui.Button("Cancel"))
                { 
                    _setParentOpen = false;
                }
                ImGui.End();
            }
            if (_gameSettingsMenuOpen)
            {
                ImGui.Begin("App Settings", ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize);
                var title = Collections.AppConfig.Title;
                var user = Collections.AppConfig.UserData;
                var initscene = Collections.AppConfig.StartScene;
                Vector2 res = new(Collections.AppConfig.Resolution.Width,
                    Collections.AppConfig.Resolution.Height);
                var siz = res.ToSystem();
                var vsync = Collections.AppConfig.Display.VSync;
                var fullscreen = Collections.AppConfig.Display.FullScreen;

                if (ImGui.InputText("Title", ref title, 256)) Collections.AppConfig.Title = title;

                if (ImGui.InputText("User Folder", ref user, 256)) Collections.AppConfig.UserData = user;
                if (ImGui.InputText("Initial Scene", ref initscene, 256)) Collections.AppConfig.StartScene = initscene;
                
                
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
                    // Time to restart
                    if (Environment.ProcessPath != null)
                    {
                        var info = new System.Diagnostics.ProcessStartInfo(Environment.ProcessPath);
                        System.Diagnostics.Process.Start(info );
                        Environment.Exit(0);
                    }
                    else
                    {
                        Log.Error("If you can see this then pray to god");
                    }
                    
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

            ImGui.Begin("Camera Info", ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize);
            ImGui.Text("Position = " + Collections.RootTree.SceneCamera.GetPosition());
            ImGui.Text("Rotation = " + Collections.RootTree.SceneCamera.GetRotation());
            ImGui.End();
            ImGui.Begin("Content Properties");
            ImGui.End();

            if(ImGui.Begin("Content"))
            {
                if (_currentFilesystemDirectory != Path.GetFullPath(Path.Combine("Game", "Content")))
                {
                    if (ImGui.Button(".."))
                    {
                        _currentFilesystemDirectory = Path.GetFullPath(Path.Combine(_currentFilesystemDirectory, @".."));
                        Log.Info(_currentFilesystemDirectory);
                        RefreshFilesystem();
                    }
                }

                foreach (var fsObject in _serialisedObjects)
                {
                    if (ImGui.Button(fsObject.FileName))
                    {
                        if (fsObject.IsFile)
                        {
                            switch (fsObject.FileName.Split(".")[1])
                            {
                                case "scn":
                                    LoadScene(fsObject.Path);
                                    break;
                                case "obj":
                                    break;
                                default:
                                    OpenWithDefaultProgram(fsObject.Path);
                                    break;
                            }
                        }
                        else
                        {
                            Log.Info(_currentFilesystemDirectory);
                            _currentFilesystemDirectory = fsObject.Path;
                            RefreshFilesystem();
                        }
                    }
                }
            }
            ImGui.End();
            ImGui.Begin("Console");
            ImGui.TextColored(new(255, 0, 0, 255), "hi");
            ImGui.End();
            ImGui.Begin("Actions");
            if (Collections.InEditorHint)
            {
                if (ImGui.Button("Play"))
                {
                    // WE NEED TO SAVE AND RELOAD THE SCENE :thumbsup:
                    if (_currentScene == "")
                    {
                        NativeMethods.MessageBox((IntPtr)0, "You have to save your scene to play!", "Vero Editor", 0);
                    }
                    else
                    {
                        SaveScene(_currentScene);
                        Collections.InEditorHint = false;
                        LoadScene(_currentScene);
                    }
                    
                }
            }
            else
            {
                if (ImGui.Button("Stop"))
                {
                    Collections.InEditorHint = true;
                    LoadScene(_currentScene);
                }
                
            }
            ImGui.SameLine();
            if (ImGui.Button("Settings")) _gameSettingsMenuOpen = true;
            ImGui.SameLine();
            if (ImGui.Button("Style Editor")) _settingsMenuOpen = true;
            ImGui.SameLine();
            if (ImGui.Button("Clear Cache"))
            {
                // Clear pipeline cache for shaders so we can recompile them easily
                Log.Info("Clearing Pipeline Cache");
                try
                {
                    Directory.Delete(Shader.GetCachePath(), true);
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Refresh"))
            {
                RefreshFilesystem();
            }
            ImGui.End();
            ImGui.Begin("Node Tree");
            IterateNode(_wnd.SceneTree.GetRoot());
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

        _wnd.Run();
    }

    private static void SaveAsSceneMenu()
    {
        var f = new List<NfdFilter>();
        f.Add(new NfdFilter() { Description = "Scene File", Specification = "scn" });
        var res = Nfd.FileSave(f);
        // time to save :sob:
        SaveScene(res.Path);
    }
    
    private static void LoadSceneMenu()
    {
        var f = new List<NfdFilter>();
        f.Add(new NfdFilter() { Description = "Scene File", Specification = "scn" });
        var res = Nfd.FileOpen(f);
        // time to save :sob:
        LoadScene(res.Path);
    }
    private static void LoadScene(string fullpath)
    {
        try
        {
            SceneManager.ChangeScene(fullpath, false);
            _currentScene = fullpath;
        }
        catch (Exception e)
        {
            NativeMethods.MessageBox((IntPtr)0, "Failed to open scene " + fullpath + "\n" + e.ToString(), "Vero Editor", 0);
        }

        // PLEASE WORK
    }
    
    private static void SaveScene(string fullpath)
    {
        try
        {
            Log.Info(fullpath);
            var root = Collections.RootTree.GetRoot();
            var serialisedRoot = SceneManager.NodeAsTree(root);
            File.WriteAllText(fullpath, serialisedRoot.Serialise());
            _currentScene = fullpath;
        }
        catch (Exception e)
        {
            NativeMethods.MessageBox((IntPtr)0, "Failed to save scene " + fullpath + "\n" + e.ToString(), "Vero Editor", 0);
        }
    }
    
    public static void OpenWithDefaultProgram(string path)
    {
        using Process fileopener = new Process();

        fileopener.StartInfo.FileName = "explorer";
        fileopener.StartInfo.Arguments = "\"" + path + "\"";
        fileopener.Start();
    }

    private static void RefreshFilesystem()
    {
        _serialisedObjects = GetFilesystemObjectsAt(_currentFilesystemDirectory); // :sob:
    }
    
    private static List<SerialisedFsObject> GetFilesystemObjectsAt(string path)
    {
        List<SerialisedFsObject> objects = new List<SerialisedFsObject>();
        foreach (var file in Directory.GetFiles(path))
        {
            objects.Add(new SerialisedFsObject
            {
                Path = file,
                FileName = Path.GetFileName(file),
                PreviewPath = Path.Combine(Path.Combine(Collections.GetUserDirectory(), "IconCache"), Path.GetFileName(file), ".cached"),
                IsFile = true
            });
        }
        foreach (var file in Directory.GetDirectories(path))
        {
            objects.Add(new SerialisedFsObject
            {
                Path = file,
                FileName = Path.GetFileName(file),
                PreviewPath = null,
                IsFile = false
            });
        }
        return objects;
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
                    if (prop.Name is "GlobalPosition" or "GlobalRotation" or "GlobalScale" or "Name")
                    {
                        ImGui.Text(prop.Name + " = " + prop.GetValue(n));
                        continue;
                    }
                    
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
                if (ImGui.Button("Reparent")) PopupReparent(n);
                if (ImGui.Button("Rename")) PopupRename(n);
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