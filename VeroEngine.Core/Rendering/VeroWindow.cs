using System;
using System.Threading;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VeroEngine.Core.Generic;
using VeroEngine.Core.NodeTree;
using static VeroEngine.Core.Generic.NativeMethods;
using Vector2 = System.Numerics.Vector2;

namespace VeroEngine.Core.Rendering;

public delegate void ReadyDelgate();

public delegate void DrawDelgate(double deltaTime);

public class VeroWindow : GameWindow
{
	private ImGuiController _controller;
	public SceneTree SceneTree;

	public VeroWindow(string windowTitle = "Game") : base(GameWindowSettings.Default,
		new NativeWindowSettings
		{
			Size = new Vector2i(1600, 900), APIVersion = new Version(3, 3),
			Title = windowTitle
		})
	{
		// Before we load we must allocate a console and register the global exception handler
		AppDomain.CurrentDomain.UnhandledException += CurrentDomainException;

		FreeConsole(); // if there is already a console window we don't need it

		// Allocate console window
		AllocConsole();
		Console.Title = "Vero Engine";

		Log.Info("Pre-initialization complete");
		Log.Info("Test Info");
		Log.Warn("Test Warning");
		Log.Error("Test Error");
	}

	public event ReadyDelgate OnReady;
	public event DrawDelgate OnDraw;
	public event DrawDelgate OnDrawGui;

	public void SetTitle(string title)
	{
		Title = title;
		Title += " | OpenGL Version: " + GL.GetString(StringName.Version) + " | Vero Engine " +
		         Collections.EngineVersion.Build;
		// throw new Exception();
	}

	public void GrabMouse()
	{
		CursorState = CursorState.Grabbed;
	}

	public void ReleaseMouse()
	{
		CursorState = CursorState.Normal;
	}

	private void CurrentDomainException(object sender, UnhandledExceptionEventArgs e)
	{
		var ee = (Exception)e.ExceptionObject;
		Log.Error(ee.ToString());

		if (IsFatalException(ee))
		{
			MessageBox((IntPtr)0,
				"Vero Engine has encountered a fatal exception.\n\n" +
				"Please check the system's event log for more details.\n\n" +
				"You can also report this issue to the App developers\n\n" +
				ee + "\n" +
				ee.StackTrace +
				"\n\nThe application will now close.",
				"Vero Engine",
				0);

			Environment.FailFast("Fatal exception: " + ee.Message, ee); // Immediately crashes the application
		}
		else
		{
			MessageBox((IntPtr)0, "An unhandled exception occured while running the application.\n\n" + ee,
				"Vero Engine", 0);
		}
	}

	private static bool IsFatalException(Exception ex)
	{
		return ex is OutOfMemoryException ||
		       ex is StackOverflowException ||
		       ex is ThreadAbortException;
	}

	protected override void OnLoad()
	{
		base.OnLoad();

		Collections.GetVersionsFromEngine();
		Collections.LoadAppConfig();
		if (Collections.AppConfig.Display.VSync) VSync = VSyncMode.On;
		Title = Collections.AppConfig.Title;
		Title += " | OpenGL Version: " + GL.GetString(StringName.Version) + " | Vero Engine " +
		         Collections.EngineVersion.Build;
		Console.Title = "Vero Engine " + Collections.EngineVersion.Build;
		Log.Info("Vero Engine " + Collections.EngineVersion.Build);
		_controller = new ImGuiController(ClientSize.X, ClientSize.Y);

		SceneTree = new SceneTree((float)ClientSize.Y / ClientSize.X);

		Collections.RootTree = SceneTree;
		Collections.SceneManager = new SceneManager();

		OnReady?.Invoke();
		if (Collections.AppConfig.Display.FullScreen) WindowState = WindowState.Maximized;
		Focus();
		SetupImGuiStyle();
	}
	
	public void SetupImGuiStyle()
	{
		// Rounded Visual Studio styleRedNicStone from ImThemes
		var style = ImGuiNET.ImGui.GetStyle();
		
		style.Alpha = 1.0f;
		style.DisabledAlpha = 0.6000000238418579f;
		style.WindowPadding = new Vector2(8.0f, 8.0f);
		style.WindowRounding = 4.0f;
		style.WindowBorderSize = 0.0f;
		style.WindowMinSize = new Vector2(32.0f, 32.0f);
		style.WindowTitleAlign = new Vector2(0.0f, 0.5f);
		style.WindowMenuButtonPosition = ImGuiDir.Left;
		style.ChildRounding = 0.0f;
		style.ChildBorderSize = 1.0f;
		style.PopupRounding = 4.0f;
		style.PopupBorderSize = 1.0f;
		style.FramePadding = new Vector2(4.0f, 3.0f);
		style.FrameRounding = 2.5f;
		style.FrameBorderSize = 0.0f;
		style.ItemSpacing = new Vector2(8.0f, 4.0f);
		style.ItemInnerSpacing = new Vector2(4.0f, 4.0f);
		style.CellPadding = new Vector2(4.0f, 2.0f);
		style.IndentSpacing = 21.0f;
		style.ColumnsMinSpacing = 6.0f;
		style.ScrollbarSize = 11.0f;
		style.ScrollbarRounding = 2.5f;
		style.GrabMinSize = 10.0f;
		style.GrabRounding = 2.0f;
		style.TabRounding = 3.5f;
		style.TabBorderSize = 0.0f;
		style.TabMinWidthForCloseButton = 0.0f;
		style.ColorButtonPosition = ImGuiDir.Right;
		style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
		style.SelectableTextAlign = new Vector2(0.0f, 0.0f);
		
		style.Colors[(int)ImGuiCol.Text] = new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 1.0f);
		style.Colors[(int)ImGuiCol.TextDisabled] = new System.Numerics.Vector4(0.5921568870544434f, 0.5921568870544434f, 0.5921568870544434f, 1.0f);
		style.Colors[(int)ImGuiCol.WindowBg] = new System.Numerics.Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
		style.Colors[(int)ImGuiCol.ChildBg] = new System.Numerics.Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
		style.Colors[(int)ImGuiCol.PopupBg] = new System.Numerics.Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
		style.Colors[(int)ImGuiCol.Border] = new System.Numerics.Vector4(0.3058823645114899f, 0.3058823645114899f, 0.3058823645114899f, 1.0f);
		style.Colors[(int)ImGuiCol.BorderShadow] = new System.Numerics.Vector4(0.3058823645114899f, 0.3058823645114899f, 0.3058823645114899f, 1.0f);
		style.Colors[(int)ImGuiCol.FrameBg] = new System.Numerics.Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2156862765550613f, 1.0f);
		style.Colors[(int)ImGuiCol.FrameBgHovered] = new System.Numerics.Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
		style.Colors[(int)ImGuiCol.FrameBgActive] = new System.Numerics.Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
		style.Colors[(int)ImGuiCol.TitleBg] = new System.Numerics.Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
		style.Colors[(int)ImGuiCol.TitleBgActive] = new System.Numerics.Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
		style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new System.Numerics.Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
		style.Colors[(int)ImGuiCol.MenuBarBg] = new System.Numerics.Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2156862765550613f, 1.0f);
		style.Colors[(int)ImGuiCol.ScrollbarBg] = new System.Numerics.Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2156862765550613f, 1.0f);
		style.Colors[(int)ImGuiCol.ScrollbarGrab] = new System.Numerics.Vector4(0.321568638086319f, 0.321568638086319f, 0.3333333432674408f, 1.0f);
		style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new System.Numerics.Vector4(0.3529411852359772f, 0.3529411852359772f, 0.3725490272045135f, 1.0f);
		style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new System.Numerics.Vector4(0.3529411852359772f, 0.3529411852359772f, 0.3725490272045135f, 1.0f);
		style.Colors[(int)ImGuiCol.CheckMark] = new System.Numerics.Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
		style.Colors[(int)ImGuiCol.SliderGrab] = new System.Numerics.Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
		style.Colors[(int)ImGuiCol.SliderGrabActive] = new System.Numerics.Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
		style.Colors[(int)ImGuiCol.Button] = new System.Numerics.Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2156862765550613f, 1.0f);
		style.Colors[(int)ImGuiCol.ButtonHovered] = new System.Numerics.Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
		style.Colors[(int)ImGuiCol.ButtonActive] = new System.Numerics.Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
		style.Colors[(int)ImGuiCol.Header] = new System.Numerics.Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2156862765550613f, 1.0f);
		style.Colors[(int)ImGuiCol.HeaderHovered] = new System.Numerics.Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
		style.Colors[(int)ImGuiCol.HeaderActive] = new System.Numerics.Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
		style.Colors[(int)ImGuiCol.Separator] = new System.Numerics.Vector4(0.3058823645114899f, 0.3058823645114899f, 0.3058823645114899f, 1.0f);
		style.Colors[(int)ImGuiCol.SeparatorHovered] = new System.Numerics.Vector4(0.3058823645114899f, 0.3058823645114899f, 0.3058823645114899f, 1.0f);
		style.Colors[(int)ImGuiCol.SeparatorActive] = new System.Numerics.Vector4(0.3058823645114899f, 0.3058823645114899f, 0.3058823645114899f, 1.0f);
		style.Colors[(int)ImGuiCol.ResizeGrip] = new System.Numerics.Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
		style.Colors[(int)ImGuiCol.ResizeGripHovered] = new System.Numerics.Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2156862765550613f, 1.0f);
		style.Colors[(int)ImGuiCol.ResizeGripActive] = new System.Numerics.Vector4(0.321568638086319f, 0.321568638086319f, 0.3333333432674408f, 1.0f);
		style.Colors[(int)ImGuiCol.Tab] = new System.Numerics.Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
		style.Colors[(int)ImGuiCol.TabHovered] = new System.Numerics.Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
		style.Colors[(int)ImGuiCol.TabActive] = new System.Numerics.Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
		style.Colors[(int)ImGuiCol.TabUnfocused] = new System.Numerics.Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
		style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new System.Numerics.Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
		style.Colors[(int)ImGuiCol.PlotLines] = new System.Numerics.Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
		style.Colors[(int)ImGuiCol.PlotLinesHovered] = new System.Numerics.Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
		style.Colors[(int)ImGuiCol.PlotHistogram] = new System.Numerics.Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
		style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new System.Numerics.Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
		style.Colors[(int)ImGuiCol.TableHeaderBg] = new System.Numerics.Vector4(0.1882352977991104f, 0.1882352977991104f, 0.2000000029802322f, 1.0f);
		style.Colors[(int)ImGuiCol.TableBorderStrong] = new System.Numerics.Vector4(0.3098039329051971f, 0.3098039329051971f, 0.3490196168422699f, 1.0f);
		style.Colors[(int)ImGuiCol.TableBorderLight] = new System.Numerics.Vector4(0.2274509817361832f, 0.2274509817361832f, 0.2470588237047195f, 1.0f);
		style.Colors[(int)ImGuiCol.TableRowBg] = new System.Numerics.Vector4(0.0f, 0.0f, 0.0f, 0.0f);
		style.Colors[(int)ImGuiCol.TableRowBgAlt] = new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 0.05999999865889549f);
		style.Colors[(int)ImGuiCol.TextSelectedBg] = new System.Numerics.Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
		style.Colors[(int)ImGuiCol.DragDropTarget] = new System.Numerics.Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
		style.Colors[(int)ImGuiCol.NavHighlight] = new System.Numerics.Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
		style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 0.699999988079071f);
		style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new System.Numerics.Vector4(0.800000011920929f, 0.800000011920929f, 0.800000011920929f, 0.2000000029802322f);
		style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new System.Numerics.Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
	}

	protected override void OnResize(ResizeEventArgs e)
	{
		base.OnResize(e);

		GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
		// Log.Info(((float)ClientSize.Y / ClientSize.X).ToString());
		SceneTree.SceneCamera.SetAspectRatio((float)ClientSize.X / ClientSize.Y);

		_controller.WindowResized(ClientSize.X, ClientSize.Y);
	}

	protected override void OnRenderFrame(FrameEventArgs e)
	{
		base.OnRenderFrame(e);

		_controller.Update(this, (float)e.Time);
		Collections.ViewportSize = new Mathematics.Vector2(ClientSize.X, ClientSize.Y);
		GL.ClearColor(new Color4(0, 0, 0, 255));
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
		GL.Enable(EnableCap.DepthTest);
		GL.Enable(EnableCap.CullFace);
		if (Collections.AppConfig.Display.EnableUiDock)
			ImGui.DockSpaceOverViewport(null, ImGuiDockNodeFlags.PassthruCentralNode); // buggy

		SceneTree.DrawChildren(e.Time);
		OnDraw?.Invoke(e.Time);
		GL.Disable(EnableCap.CullFace);
		GL.Disable(EnableCap.DepthTest);
		GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
		OnDrawGui?.Invoke(e.Time);
		// ImGui.ShowStyleEditor();
		ImGui.Begin("Debug",
			ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoDocking |
			ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoMouseInputs);
		ImGui.SetWindowPos(new Vector2(0, 2));
		ImGui.Text($"fps {(int)(1 / e.Time)}");
		ImGui.Text($"delta {e.Time}");
		ImGui.End();


		_controller.Render();

		ImGuiController.CheckGLError("End of frame");

		SwapBuffers();
	}


	protected override void OnTextInput(TextInputEventArgs e)
	{
		base.OnTextInput(e);


		_controller.PressChar((char)e.Unicode);
	}

	protected override void OnKeyDown(KeyboardKeyEventArgs e)
	{
		base.OnKeyDown(e);

		Keyboard.SetKeyDown(e.Key);
	}

	protected override void OnKeyUp(KeyboardKeyEventArgs e)
	{
		base.OnKeyUp(e);

		Keyboard.SetKeyUp(e.Key);
	}


	protected override void OnMouseWheel(MouseWheelEventArgs e)
	{
		base.OnMouseWheel(e);

		_controller.MouseScroll(e.Offset);
	}

	protected override void OnMouseMove(MouseMoveEventArgs e)
	{
		base.OnMouseMove(e);
		Mouse.Position = new Mathematics.Vector2(e.Position.X, e.Position.Y);
	}

	protected override void OnMouseDown(MouseButtonEventArgs e)
	{
		base.OnMouseDown(e);

		if (e.Button == MouseButton.Left)
		{
			Mouse.LeftButtonDown = true;
			Mouse.LeftJustButtonDown = true;
		}
		else if (e.Button == MouseButton.Right)
		{
			Mouse.RightButtonDown = true;
			Mouse.RightJustButtonDown = true;
		}
	}

	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.OnMouseUp(e);

		if (e.Button == MouseButton.Left)
		{
			Mouse.LeftButtonDown = false;
			Mouse.LeftJustButtonDown = false;
		}
		else if (e.Button == MouseButton.Right)
		{
			Mouse.RightButtonDown = false;
			Mouse.RightJustButtonDown = false;
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		SceneTree.Destroy();
		FreeConsole();
	}
}