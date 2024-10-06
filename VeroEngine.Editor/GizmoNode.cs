using VeroEngine.Core.NodeTree.Nodes;
using VeroEngine.Core.Rendering;

namespace VeroEngine.Editor;

public class GizmoNode : Node
{
	private MeshNode _moveNode;
	private MeshNode _rotateNode;

	public override void Create()
	{
		_moveNode = new MeshNode();
		_moveNode.SetMesh(RenderMesh.FromModelFile("Editor/GizmoMove.obj"));
		_moveNode.Name = "move";
		_moveNode.DepthTest = false;
		AddChild(_moveNode);

		_rotateNode = new MeshNode();
		_rotateNode.Name = "rotate";
		_rotateNode.AddChild(_moveNode);
		_rotateNode.Visible = false;
	}
}