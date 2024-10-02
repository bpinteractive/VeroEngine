using System.Text.Json;

namespace VeroEngine.Core.Generic;

public class SerialisedApp
{
    public string Title { get; set; }
    public string Version { get; set; }
    public string UserData { get; set; }
    public Resolution Resolution { get; set; }
    public DisplaySettings Display { get; set; }
    
    public static SerialisedApp Deserialize(string jsonString)
    {
        return JsonSerializer.Deserialize<SerialisedApp>(jsonString);
    }
    
    public string Serialize()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}

public class Resolution
{
    public int Width { get; set; }
    public int Height { get; set; }
}

public class DisplaySettings
{
    public bool EnableUiDock { get; set; }
    public bool FullScreen { get; set; }
    public bool VSync { get; set; }
    public int FpsLimit { get; set; }
    public float Brightness { get; set; }
}