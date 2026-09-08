namespace RawInput_dll;

public enum HidUsage : ushort
{
	Undefined = 0,
	Pointer = 1,
	Mouse = 2,
	Joystick = 4,
	Gamepad = 5,
	Keyboard = 6,
	Keypad = 7,
	SystemControl = 128,
	Tablet = SystemControl,
	Consumer = 12
}
