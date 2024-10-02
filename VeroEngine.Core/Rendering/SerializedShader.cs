using System;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace VeroEngine.Core.Rendering;

public class SerializedShader
{
    public string FriendlyName { get; set; }
    public string VertexShader { get; set; }
    public string FragmentShader { get; set; }
    
    public static SerializedShader Deserialize(string jsonString)
    {
        var jString = PreprocessJson(jsonString);
        return JsonSerializer.Deserialize<SerializedShader>(jString);
    }

    private static string PreprocessJson(string jsonString)
    {
        string res = "";
        foreach (var fakeline in jsonString.Split("\n"))
        {
            res += fakeline.Split("//")[0] + "\n";
        }

        return res;
    }
}