using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using VeroEngine.Core.NodeTree;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Mathematics;
using static VeroEngine.Core.Generic.NativeMethods;
using Vector3 = VeroEngine.Core.Mathematics.Vector3;

namespace VeroEngine.Core.Rendering
{
    public delegate void ReadyDelgate(); 
    public delegate void DrawDelgate(double deltaTime); 
    public class VeroWindow : GameWindow
    {
        private ImGuiController _controller;
        public SceneTree SceneTree;
        public event ReadyDelgate OnReady; 
        public event DrawDelgate OnDraw; 
        public event DrawDelgate OnDrawGui; 
        public VeroWindow(string windowTitle = "Game") : base(GameWindowSettings.Default,
            new NativeWindowSettings()
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

        public void SetTitle(string title)
        {
            Title = title;
            Title += " | OpenGL Version: " + GL.GetString(StringName.Version) + " | Vero Engine " + Collections.EngineVersion.Build;
            // throw new Exception();
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

                Environment.FailFast("Fatal exception: " + ee.Message, ee);  // Immediately crashes the application
            }
            else
            {
                MessageBox((IntPtr)0, "An unhandled exception occured while running the application.\n\n" + ee.ToString(), "Vero Engine", 0);
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
            if (Collections.AppConfig.Display.VSync)
            {
                VSync = VSyncMode.On;
            }
            Title += " | OpenGL Version: " + GL.GetString(StringName.Version) + " | Vero Engine " + Collections.EngineVersion.Build;
            Console.Title = "Vero Engine " + Collections.EngineVersion.Build;
            Log.Info("Vero Engine " + Collections.EngineVersion.Build);
            _controller = new ImGuiController(ClientSize.X, ClientSize.Y);
            
            SceneTree = new SceneTree((float)ClientSize.Y/ ClientSize.X);
            
            Collections.RootTree = SceneTree;
            Collections.SceneManager = new SceneManager();
            
            OnReady?.Invoke();
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

            GL.ClearColor(new Color4(0, 0, 0, 255));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            // if (Collections.AppConfig.Display.EnableUiDock) ImGui.DockSpaceOverViewport(); // buggy
            
            SceneTree.DrawChildren(e.Time);
            OnDraw?.Invoke(e.Time);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            OnDrawGui?.Invoke(e.Time);
            ImGui.Begin("Debug", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBackground);
            ImGui.SetWindowPos(new System.Numerics.Vector2(0, 0));
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

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            
            _controller.MouseScroll(e.Offset);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            SceneTree.Destroy();
            FreeConsole();
        }
        
    }
}
