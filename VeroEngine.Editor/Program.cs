using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ImGuiNET;
using NativeFileDialogExtendedSharp;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ScriptingAssembly;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Mathematics;
using VeroEngine.Core.NodeTree;
using VeroEngine.Core.NodeTree.Nodes;
using VeroEngine.Core.Rendering;
using Vector2 = VeroEngine.Core.Mathematics.Vector2;
using Vector3 = VeroEngine.Core.Mathematics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace VeroEngine.Editor;

internal class Program
{
	private static VeroWindow _wnd;

	private static Node _currentNode;

	private static string _currentScene = "";

	private static bool _newNodeOpenMenu;
	private static string _newNodeName = "Node";
	private static int _newNodeClass;
	private static Node _newNodeParent;

	private static bool _gameSettingsMenuOpen;
	private static bool _settingsMenuOpen;

	private static bool _setParentOpen;
	private static int _newParentIndex;

	private static bool _renameNodeOpen;
	private static string _renameNodeName = "Node";

	private static readonly List<string> _nodeClasses = new();

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
		_newNodeClass = 0;
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

	public static void RefreshNodes()
	{
		_nodeClasses.Clear();
		// ADD BUILTIN NODES HERE PLEASE
		_nodeClasses.Add("Node");
		_nodeClasses.Add("MeshNode");
		_nodeClasses.Add("PointLight");
		_nodeClasses.Add("DirectionalLight");
		_nodeClasses.Add("RigidBody");
		_nodeClasses.Add("Collider");
		_nodeClasses.Add("StaticBody");
		_nodeClasses.Add("CameraNode");
		_nodeClasses.Add("RotateNode");

		var tee = GetTypesInNamespace(ScriptingInterface.GetAssembly(), "ScriptingAssembly.Nodes").ToList();
		foreach (var te in tee) _nodeClasses.Add(te.Name);
	}

	public static void newFile()
	{
		if (_currentScene != "") SaveScene(_currentScene);
		else
		{
			var res = NativeMethods.MessageBox(IntPtr.Zero, "Do you want to create a new file\nwithout saving?", "Vero Engine", 1); // ok==1 cancel==2
			if (res == 2)
			{
				return;
			}
		}
		_currentScene = "";
		Collections.RootTree.GetRoot().Destroy();
		Collections.RootTree.SetRoot(new Node { Name = "Workspace" });
	}
	
	public static void openFile()
	{
		if (_currentScene != "") SaveScene(_currentScene);
		else
		{
			var res = NativeMethods.MessageBox(IntPtr.Zero, "Do you want to load a new file\nwithout saving?", "Vero Engine", 1); // ok==1 cancel==2
			if (res == 2)
			{
				return;
			}
		}
		LoadSceneMenu();
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
			Collections.ActuallyInEditor = true;

			RefreshFilesystem();
			RefreshNodes();
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
					_wnd.SceneTree.SceneCamera.Rotate((float)Util.Deg2Rad(mouseDelta.X * 0.1f), -(float)Util.Deg2Rad(mouseDelta.Y * 0.1f), 0);

					_lastMousePosition = mousePosition;
				}
				else
				{
					_isRightMousePressed = false;
					_lastMousePosition = default;
				}
			}

			/*if (!Collections.IsCameraStolen)
			{
				_wnd.SceneTree.SceneCamera.SetFieldOfView((float)Util.Deg2Rad(90));
				var r = _wnd.SceneTree.SceneCamera.GetRotation();
				r.Z = 0.0f;
				_wnd.SceneTree.SceneCamera.SetRotation(r);
			}*/

			if (Keyboard.KeyJustPressed(Keys.S) && Keyboard.KeyPress(Keys.LeftControl))
			{
				if (_currentScene != "")
				{
					SaveScene(_currentScene);
				}
				else
				{
					SaveAsSceneMenu();
				}
			}
			else if (Keyboard.KeyJustPressed(Keys.S) && Keyboard.KeyPress(Keys.LeftControl) && !Keyboard.KeyPress(Keys.LeftShift))
			{
				SaveAsSceneMenu();
			}
			else if (Keyboard.KeyJustPressed(Keys.N) && Keyboard.KeyPress(Keys.LeftControl))
			{
				newFile();
			}
			else if (Keyboard.KeyJustPressed(Keys.O) && Keyboard.KeyPress(Keys.LeftControl))
			{
				openFile();
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

				_wnd.SceneTree.SceneCamera.SetPosition(Vector3.FromSystem(camPos));
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
					if (ImGui.MenuItem("New", "Ctrl+N"))
					{
						newFile();
					}

					if (ImGui.MenuItem("Save", "Ctrl+S"))
					{
						// save
						if (_currentScene != "")
							SaveScene(_currentScene);
						else
							SaveAsSceneMenu();
					}

					if (ImGui.MenuItem("Save As", "Ctrl+Shift+S")) SaveAsSceneMenu();

					if (ImGui.MenuItem("Open", "Ctrl+O"))
					{
						openFile();
					}

					ImGui.EndMenu();
				}

				ImGui.EndMainMenuBar();
			}

			if (_renameNodeOpen)
			{
				ImGui.Begin("Rename Node",
					ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize);
				ImGui.InputText("Name", ref _newNodeName, 128);
				if (ImGui.Button("Rename"))
				{
					_currentNode.Name = _newNodeName;
					_renameNodeOpen = false;
				}

				ImGui.SameLine();
				if (ImGui.Button("Cancel")) _renameNodeOpen = false;

				ImGui.End();
			}

			if (_setParentOpen)
			{
				ImGui.Begin("Set Parent",
					ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize);
				ImGui.Text(_currentNode.Name + "->" + _currentNode.Parent.Name);
				var items = _currentNode.Parent.Children.Select(c => c.Name).ToList();
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
				if (ImGui.Button("Cancel")) _setParentOpen = false;

				ImGui.End();
			}

			if (_gameSettingsMenuOpen)
			{
				ImGui.Begin("App Settings",
					ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize);
				var title = Collections.AppConfig.Title;
				var user = Collections.AppConfig.UserData;
				var initscene = Collections.AppConfig.StartScene;
				var fnt = Collections.AppConfig.Display.Font;
				var fntS = Collections.AppConfig.Display.FontScale;
				
				Vector2 res = new(Collections.AppConfig.Resolution.Width,
					Collections.AppConfig.Resolution.Height);
				var siz = res.ToSystem();
				var vsync = Collections.AppConfig.Display.VSync;
				var fullscreen = Collections.AppConfig.Display.FullScreen;

				if (ImGui.InputText("Title", ref title, 256)) Collections.AppConfig.Title = title;

				if (ImGui.InputText("User Folder", ref user, 256)) Collections.AppConfig.UserData = user;
				if (ImGui.InputText("Initial Scene", ref initscene, 256)) Collections.AppConfig.StartScene = initscene;
				
				if (ImGui.InputText("Font", ref fnt, 256)) Collections.AppConfig.Display.Font = fnt;
				if(ImGui.DragInt("Font Size", ref fntS, 1, 10, 32)) Collections.AppConfig.Display.FontScale = fntS;


				if (ImGui.DragFloat2("Resolution", ref siz))
				{
					Collections.AppConfig.Resolution.Width = (int)siz.X;
					Collections.AppConfig.Resolution.Height = (int)siz.Y;
				}

				if (ImGui.Checkbox("Vsync", ref vsync)) Collections.AppConfig.Display.VSync = vsync;
				if (ImGui.Checkbox("Fullscreen", ref fullscreen)) Collections.AppConfig.Display.FullScreen = fullscreen;


				if (ImGui.Button("Save"))
				{
					var serial = Collections.AppConfig.Serialize();
					File.WriteAllText(Path.Combine("Game", "App.json"), serial);
					Log.Info("Wrote AppConfig");
					// Time to restart
					if (Environment.ProcessPath != null)
					{
						var info = new ProcessStartInfo(Environment.ProcessPath);
						Process.Start(info);
						Environment.Exit(0);
					}
					else
					{
						Log.Error("If you can see this then pray to god");
					}

					_gameSettingsMenuOpen = false;
				}

				ImGui.SameLine();
				if (ImGui.Button("Cancel")) _gameSettingsMenuOpen = false;

				ImGui.End();
			}

			if (_settingsMenuOpen)
			{
				ImGui.Begin("Editor Settings");
				if (ImGui.Button("Close")) _settingsMenuOpen = false;

				ImGui.Text("If you dont know what your doing this is useless!!!");
				ImGui.ShowStyleEditor();
				ImGui.End();
			}
			
			ImGui.Begin("Content Properties");
			ImGui.End();

			if (ImGui.Begin("Content"))
			{
				if (_currentFilesystemDirectory != Path.GetFullPath(Path.Combine("Game", "Content")))
					if (ImGui.Button(".."))
					{
						_currentFilesystemDirectory =
							Path.GetFullPath(Path.Combine(_currentFilesystemDirectory, @".."));
						Log.Info(_currentFilesystemDirectory);
						RefreshFilesystem();
					}

				foreach (var fsObject in _serialisedObjects)
				{
					if (ImGui.IsMouseReleased(ImGuiMouseButton.Right) && ImGui.IsItemHovered())
					{
						// todo: copy path to clipboard
					}
					if (!ImGui.Button(fsObject.FileName)) continue;
					if (fsObject.IsFile)
					{
						switch (fsObject.FileName.Split(".")[1])
						{
							case "scn":
								LoadScene(fsObject.Path);
								break;
							case "obj":
								LoadScene(fsObject.Path); // as a preview
								break;
							case "mat":
								// open a preview window?
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

			ImGui.End();
			ImGui.Begin("Console");
			foreach (var entry in Log.LogsSent.ToList())
				switch (entry.Level.ToLower())
				{
					case "warn":
						ImGui.TextColored(new Vector4(255, 255, 0, 255), entry.Message);
						break;
					case "error":
						ImGui.TextColored(new Vector4(255, 0, 0, 255), entry.Message);
						break;
					case "info":
						ImGui.TextColored(new Vector4(0, 255, 255, 255), entry.Message);
						break;
				}

			if (ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
				ImGui.SetScrollHereY(1.0f);
			ImGui.End();
			ImGui.Begin("Actions", ImGuiWindowFlags.AlwaysAutoResize);
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
				Shader.InternalCache.Clear();
			}

			ImGui.SameLine();
			if (ImGui.Button("Refresh")) RefreshFilesystem();

			ImGui.End();
			ImGui.Begin("Node Tree");
			IterateNode(_wnd.SceneTree.GetRoot());
			ImGui.End();

			#region New Node

			if (_newNodeOpenMenu)
			{
				ImGui.Begin("New Node",
					ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize);

				ImGui.InputText("Name", ref _newNodeName, 250);

				ImGui.Combo("Class", ref _newNodeClass, _nodeClasses.ToArray(), _nodeClasses.Count);
				if (ImGui.Button("Create"))
				{
					var inst = SceneTree.CreateNode(_nodeClasses[_newNodeClass], ScriptingInterface.GetAssembly());
					inst.Name = _newNodeName;
					_newNodeParent.AddChild(inst);
					inst.Create();
					_newNodeOpenMenu = false;
				}

				ImGui.SameLine();
				if (ImGui.Button("Cancel")) _newNodeOpenMenu = false;

				ImGui.End();
			}

			#endregion
		};

		_wnd.Run();
	}

	private static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
	{
		return
			assembly.GetTypes()
				.Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
				.ToArray();
	}

	private static void SaveAsSceneMenu()
	{
		var f = new List<NfdFilter>();
		f.Add(new NfdFilter { Description = "Scene File", Specification = "scn" });
		var res = Nfd.FileSave(f);
		if (res.Status == NfdStatus.Cancel)
		{
			return;
		}
		SaveScene(res.Path);
	}

	private static void LoadSceneMenu()
	{
		var f = new List<NfdFilter>();
		f.Add(new NfdFilter { Description = "Scene File", Specification = "scn" });
		var res = Nfd.FileOpen(f);
		if (res.Status == NfdStatus.Cancel)
		{
			return;
		}
		LoadScene(res.Path);
	}

	private static void LoadScene(string fullpath)
	{
		if (fullpath.EndsWith(".obj"))
		{
			if(_currentScene!="")
				SaveScene(_currentScene);
			
			Collections.RootTree.SetRoot(new() {Name = "Workspace"});
			var mdl = new MeshNode();
			mdl.Model = fullpath;
			Collections.RootTree.AddChild(mdl);
			return;
		}
		try
		{
			SceneManager.ChangeScene(fullpath, false);
			_currentScene = fullpath;
		}
		catch (Exception e)
		{
			NativeMethods.MessageBox((IntPtr)0, "Failed to open scene " + fullpath + "\n" + e, "Vero Editor",
				0);
		}
		
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
			NativeMethods.MessageBox((IntPtr)0, "Failed to save scene " + fullpath + "\n" + e, "Vero Editor",
				0);
		}
	}

	public static void OpenWithDefaultProgram(string path)
	{
		using var fileopener = new Process();

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
		var objects = new List<SerialisedFsObject>();
		foreach (var file in Directory.GetFiles(path))
			objects.Add(new SerialisedFsObject
			{
				Path = file,
				FileName = Path.GetFileName(file),
				PreviewPath = Path.Combine(Path.Combine(Collections.GetUserDirectory(), "IconCache"),
					Path.GetFileName(file), ".cached"),
				IsFile = true
			});

		foreach (var file in Directory.GetDirectories(path))
			objects.Add(new SerialisedFsObject
			{
				Path = file,
				FileName = Path.GetFileName(file),
				PreviewPath = null,
				IsFile = false
			});

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
				ImGui.DragFloat3(prop.Name, ref sysvec, 0.1f);
				prop.SetValue(n, Vector3.FromSystem(sysvec));
				break;

			case "single": // float
				var floate = (float)val;
				ImGui.DragFloat(prop.Name, ref floate, 0.1f);
				prop.SetValue(n, floate);
				break;

			case "string":
				var str = (string)val;
				if (ImGui.InputText(prop.Name, ref str, 255)) prop.SetValue(n, str);
				break;

			case "boolean":
				var bl = (bool)val;
				ImGui.Checkbox(prop.Name, ref bl);
				prop.SetValue(n, bl);
				break;

			case "collisionshape":
				var myStruct = (CollisionShape)val;
				string[] structOptions = { "COLLISION_SHAPE_MESH", "COLLISION_SHAPE_SPHERE", "COLLISION_SHAPE_BOX" };
				var selectedIndex = Array.IndexOf(structOptions, myStruct.ToString());

				if (ImGui.Combo(prop.Name, ref selectedIndex, structOptions, structOptions.Length))
				{
					myStruct = (CollisionShape)Enum.Parse(prop.PropertyType, structOptions[selectedIndex]);
					prop.SetValue(n, myStruct);
				}

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