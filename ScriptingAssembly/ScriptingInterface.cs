using System.Reflection;

namespace ScriptingAssembly;

public class ScriptingInterface // sole purpose is to allow us to get the assembly
{
	public static Assembly GetAssembly()
	{
		return typeof(ScriptingInterface).Assembly;
	}
}