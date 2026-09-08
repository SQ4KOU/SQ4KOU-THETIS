namespace Thetis;

public class ThetisSkin
{
	private string _skinName;

	public string SkinName
	{
		get
		{
			return _skinName;
		}
		set
		{
			_skinName = value.Left(45);
		}
	}

	public string SkinUrl { get; set; }

	public string SkinVersion { get; set; }

	public string FromThetisVersion { get; set; }

	public string ThumbnailUrl { get; set; }

	public string SkinHomepageUrl { get; set; }

	public string DateReleased { get; set; }

	public string Overview { get; set; }

	public bool IsMeterSkin { get; set; }
}
