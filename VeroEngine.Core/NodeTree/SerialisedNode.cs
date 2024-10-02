using System.Collections.Generic;

namespace VeroEngine.Core.NodeTree;

public class SerialisedNode
{
    public List<KeyValuePair<string, object>> Variables;
    public string ClassName { get; set; } = "Node";
    public List<SerialisedNode> Children { get; set; }
}