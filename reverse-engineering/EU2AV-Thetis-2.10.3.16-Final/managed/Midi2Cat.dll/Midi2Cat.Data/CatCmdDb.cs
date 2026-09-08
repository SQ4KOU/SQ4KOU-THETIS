namespace Midi2Cat.Data;

public static class CatCmdDb
{
	public static CatCommandAttribute Get(CatCmd Id)
	{
		CatCommandAttribute obj = (CatCommandAttribute)typeof(CatCmd).GetMember(Id.ToString())[0].GetCustomAttributes(typeof(CatCommandAttribute), inherit: false)[0];
		obj.CatCommandId = Id;
		return obj;
	}
}
