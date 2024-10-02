using VeroEngine.Core.Generic;
using VeroEngine.Core.Rendering;

namespace VeroEngine.Game;

internal abstract class Program
{
    public static void Main()
    {
        var wnd = new VeroWindow();
        wnd.OnReady += () => { wnd.SetTitle(Collections.AppConfig.Title); };
        wnd.Run();
    }
}