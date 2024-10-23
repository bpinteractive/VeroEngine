using System.Collections.Generic;

namespace VeroEngine.Core.Generic;

public class Registry
{
	private Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

	public T Get<T>(string key)
	{
		if (!Data.TryGetValue(key, out var value))
		{
			return default;
		}
		return (T)value;
	}

	public void Set(string key, object value)
	{
		Data[key] = value;
	}
}