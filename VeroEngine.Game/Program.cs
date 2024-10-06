using ScriptingAssembly;
using VeroEngine.Core.Generic;
using VeroEngine.Core.NodeTree;
using VeroEngine.Core.Rendering;

namespace VeroEngine.Game;

internal abstract class Program
{
	public static void Main()
	{
		var wnd = new VeroWindow();
		wnd.OnReady += () =>
		{
			wnd.SetTitle(Collections.AppConfig.Title);
			Collections.ScriptingAssembly = ScriptingInterface.GetAssembly();
			SceneManager.ChangeScene(Collections.AppConfig.StartScene);
			Collections.InEditorHint = false;
		};
		wnd.Run();
	}
}