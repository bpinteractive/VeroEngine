using System.Collections.Generic;
using System.Text.Json;

namespace VeroEngine.Core.Generic;

public class SerialisedMaterial
{
    public string Shader { get; set; }
    public Dictionary<string, SerialisedUniform> Uniforms { get; set; }

    public static SerialisedMaterial Deserialize(string json)
    {
        return JsonSerializer.Deserialize<SerialisedMaterial>(json);
    }
}

public class SerialisedUniform
{
    public string Type { get; set; }
    public JsonElement Value { get; set; }
}