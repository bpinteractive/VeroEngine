using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using VeroEngine.Core.NodeTree;

// Just telling the compiler to automatically fill in the build number 1.0.X
[assembly: AssemblyVersion("1.0.*")]

namespace VeroEngine.Core.Generic;

public struct Collections
{
    public static Version EngineVersion;
    public static SerialisedApp AppConfig;
    public static SceneManager SceneManager;

    public static bool InEditorHint = false;

    public static SceneTree RootTree;

    public static void LoadAppConfig()
    {
        Log.Info("Loading app config...");

        var filePath = Path.Combine("Game", "App.json");

        // Check if the config file exists
        if (!File.Exists(filePath))
        {
            Log.Error($"App config file not found: {filePath}");
            return; // Early exit if the file doesn't exist
        }

        try
        {
            var jsonContent = File.ReadAllText(filePath);
            AppConfig = SerialisedApp.Deserialize(jsonContent);

            Log.Info("App config loaded successfully.");
        }
        catch (JsonException jsonEx)
        {
            Log.Error($"Error deserializing app config: {jsonEx.Message}");
        }
        catch (IOException ioEx)
        {
            Log.Error($"I/O error while loading app config: {ioEx.Message}");
        }
        catch (Exception ex)
        {
            Log.Error($"Unexpected error occurred while loading app config: {ex.Message}");
        }
    }

    public static void GetVersionsFromEngine()
    {
        var assembly = typeof(Collections).Assembly;
        EngineVersion = assembly.GetName().Version;
    }

    public static string GetUserDirectory() // C:/Users/{USER}/AppData/Roaming/{DATADIR}/
    {
        var userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var fullPath = Path.Combine(userProfilePath, "AppData", "Roaming", AppConfig.UserData);
        if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);

        return fullPath + Path.DirectorySeparatorChar; // Add trailing slash
    }
}