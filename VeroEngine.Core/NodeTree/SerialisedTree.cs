using System.Text.Json;

namespace VeroEngine.Core.NodeTree;

public class SerialisedTree
{
    public SerialisedNode RootNode { get; set; }

    public string Serialise()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions(){});
    }
    
    public static SerialisedTree Deserialise(string json)
    {
        return JsonSerializer.Deserialize<SerialisedTree>(json);
    }
}