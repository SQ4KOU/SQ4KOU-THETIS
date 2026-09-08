using System.Collections.Generic;

namespace Thetis;

public class SkinsData
{
	public string AuthorName { get; set; }

	public string AuthorCallsign { get; set; }

	public string AuthorNickname { get; set; }

	public string SkinsHomepageUrl { get; set; }

	public string DonateUrl { get; set; }

	public List<ThetisSkin> ThetisSkins { get; set; }
}
