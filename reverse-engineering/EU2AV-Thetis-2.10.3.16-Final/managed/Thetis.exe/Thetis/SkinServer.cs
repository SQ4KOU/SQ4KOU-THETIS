namespace Thetis;

public class SkinServer
{
	private string _desc;

	public string SkinServerUrl { get; set; }

	public string Description
	{
		get
		{
			return _desc;
		}
		set
		{
			_desc = value.Left(50);
		}
	}

	public bool BypassRootFolderCheck { get; set; }
}
