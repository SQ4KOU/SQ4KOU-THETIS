namespace RawInput_dll;

public struct DeviceInfoKeyboard
{
	public uint Type;

	public uint SubType;

	public uint KeyboardMode;

	public uint NumberOfFunctionKeys;

	public uint NumberOfIndicators;

	public uint NumberOfKeysTotal;

	public override string ToString()
	{
		return $"DeviceInfoKeyboard\n Type: {Type}\n SubType: {SubType}\n KeyboardMode: {KeyboardMode}\n NumberOfFunctionKeys: {NumberOfFunctionKeys}\n NumberOfIndicators {NumberOfIndicators}\n NumberOfKeysTotal: {NumberOfKeysTotal}\n";
	}
}
