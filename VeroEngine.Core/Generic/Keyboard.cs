using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace VeroEngine.Core.Generic;

public static class Keyboard
{
	private static readonly Dictionary<Keys, bool> KeyStates = new();
	private static readonly Dictionary<Keys, bool> KeyJustPressedStates = new();

	public static bool KeyPress(Keys key)
	{
		return KeyStates.ContainsKey(key) && KeyStates[key];
	}

	public static bool KeyJustPressed(Keys key)
	{
		if (KeyJustPressedStates.ContainsKey(key) && KeyJustPressedStates[key])
		{
			KeyJustPressedStates[key] = false;
			return true;
		}

		return false;
	}

	public static void SetKeyDown(Keys key)
	{
		if (!KeyStates.ContainsKey(key) || !KeyStates[key]) KeyJustPressedStates[key] = true;
		KeyStates[key] = true;
	}

	public static void SetKeyUp(Keys key)
	{
		KeyStates[key] = false;
		KeyJustPressedStates[key] = false;
	}
}