using System.Text;

namespace Thetis;

public class PADeviceInfo
{
	private string _Name;

	private int _Index;

	public string Name => _Name;

	public int Index => _Index;

	public PADeviceInfo(string argName, int argIndex)
	{
		_Name = argName;
		_Index = argIndex;
	}

	public override string ToString()
	{
		byte[] bytes = Encoding.Default.GetBytes(_Name);
		return Encoding.UTF8.GetString(bytes);
	}
}
