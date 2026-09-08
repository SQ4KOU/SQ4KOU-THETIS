using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Thetis;

public abstract class Keyboard
{
	[Flags]
	private enum KeyStates
	{
		None = 0,
		Down = 1,
		Toggled = 2
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	private static extern short GetKeyState(int keyCode);

	private static KeyStates GetKeyState(Keys key)
	{
		KeyStates keyStates = KeyStates.None;
		short keyState = GetKeyState((int)key);
		if ((keyState & 0x8000) == 32768)
		{
			keyStates |= KeyStates.Down;
		}
		if ((keyState & 1) == 1)
		{
			keyStates |= KeyStates.Toggled;
		}
		return keyStates;
	}

	public static bool IsKeyDown(Keys key)
	{
		return KeyStates.Down == (GetKeyState(key) & KeyStates.Down);
	}

	public static bool IsKeyToggled(Keys key)
	{
		return KeyStates.Toggled == (GetKeyState(key) & KeyStates.Toggled);
	}
}
