using System.Collections.Generic;

namespace VeroEngine.Core.NodeTree;

public class SerialisedNode
{
    public string ClassName { get; set; } = "Node";
    public List<KeyValuePair<string, object>> Variables { get; set; }
    public List<SerialisedNode> Children { get; set; }
}