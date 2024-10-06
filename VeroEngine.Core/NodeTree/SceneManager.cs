using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Mathematics;
using VeroEngine.Core.NodeTree.Nodes;

namespace VeroEngine.Core.NodeTree;

public class SceneManager
{
	public static SerialisedTree NodeAsTree(Node node)
	{
		var tree = new SerialisedTree
		{
			RootNode = SerialiseNode(node)
		};
		return tree;
	}

	public static void ChangeScene(string scene, bool relative = true)
	{
		Collections.RootTree.Physics?.Cleanup();
		Collections.RootTree.Physics = new ScenePhysics();
		Collections.RootTree.Physics.Init();
		if (relative)
		{
			scene = Path.Combine(Path.Combine("Game", "Content"), scene);
			var deserialised = TreeAsNode(SerialisedTree.Deserialise(File.ReadAllText(scene)),
				Collections.ScriptingAssembly);
			Collections.RootTree.SetRoot(deserialised);
		}
		else
		{
			var deserialised = TreeAsNode(SerialisedTree.Deserialise(File.ReadAllText(scene)),
				Collections.ScriptingAssembly);
			Collections.RootTree.SetRoot(deserialised);
		}
	}

	public static SerialisedTree LoadScene(string path)
	{
		var realpath = Path.Combine("Game", "Content", path);
		if (!File.Exists(realpath)) return default;
		var content = File.ReadAllText(realpath);
		var serial = JsonSerializer.Deserialize<SerialisedTree>(content);
		return serial;
	}

	public static void SaveScene(string path, SerialisedTree tree)
	{
		var realpath = Path.Combine("Game", "Content", path);
		var content = JsonSerializer.Serialize(tree);
		File.WriteAllText(realpath, content);
	}

	public static SerialisedNode SerialiseNode(Node node)
	{
		var snode = new SerialisedNode
		{
			ClassName = node.GetType().Name,
			Children = new List<SerialisedNode>(),
			Variables = new List<KeyValuePair<string, object>>()
		};

		var properties = node.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
			.Where(p => p.Name != "Children" && p.Name != "Parent");

		foreach (var property in properties)
			try
			{
				var value = property.GetValue(node);
				snode.Variables.Add(new KeyValuePair<string, object>(property.Name, value));
			}
			catch (Exception ex)
			{
				Log.Error($"Error retrieving property {property.Name}: {ex.Message}");
			}

		foreach (var cnode in node.Children.ToList()) snode.Children.Add(SerialiseNode(cnode));

		return snode;
	}

	public static Node TreeAsNode(SerialisedTree tree, Assembly assembly)
	{
		if (tree.RootNode == null) return null;
		return DeserialiseNode(tree.RootNode, assembly);
	}

	public static Node DeserialiseNode(SerialisedNode snode, Assembly assembly)
	{
		var node = SceneTree.CreateNode(snode.ClassName, assembly);

		var properties = node.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
			.Where(p => p.Name != "Children" && p.Name != "Parent");

		foreach (var childSnode in snode.Children)
		{
			var childNode = DeserialiseNode(childSnode, assembly);
			if (childNode != null) node.AddChild(childNode);
		}


		foreach (var property in properties)
		{
			var matchingVariable = snode.Variables.FirstOrDefault(kv => kv.Key == property.Name);
			if (!matchingVariable.Equals(default(KeyValuePair<string, object>)))
				try
				{
					var value = matchingVariable.Value;

					// Handle JsonElement types manually
					if (value is JsonElement element)
					{
						object convertedValue = null;

						if (property.PropertyType == typeof(Vector3))
							convertedValue = DeserializeVector3(element);
						else if (property.PropertyType == typeof(bool))
							convertedValue = element.GetBoolean();
						else if (property.PropertyType == typeof(string))
							convertedValue = element.GetString();
						else if (property.PropertyType == typeof(float))
							convertedValue = element.GetSingle();
						else if (property.PropertyType.IsEnum)
							convertedValue = element.GetInt32();
						else
							convertedValue = element;

						if (convertedValue != null) property.SetValue(node, convertedValue);
					}
					else
					{
						property.SetValue(node, value);
					}
				}
				catch (Exception ex)
				{
					Log.Error($"Error setting property {property.Name}: {ex.Message}");
				}
		}

		return node;
	}

	private static Vector3 DeserializeVector3(JsonElement element)
	{
		var x = element.GetProperty("X").GetSingle();
		var y = element.GetProperty("Y").GetSingle();
		var z = element.GetProperty("Z").GetSingle();

		return new Vector3(x, y, z);
	}
}