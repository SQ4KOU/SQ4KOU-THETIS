namespace Midi2Cat.Data;

public class FixUp
{
	public static ControlType FixControlType(int controlType)
	{
		int result = controlType;
		switch (controlType)
		{
		case 2:
			controlType = 1;
			return (ControlType)result;
		case 3:
			controlType = 4;
			break;
		}
		return (ControlType)result;
	}
}
