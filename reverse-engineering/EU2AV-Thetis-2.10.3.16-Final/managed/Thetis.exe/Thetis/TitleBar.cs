namespace Thetis;

internal class TitleBar
{
	public const string BUILD_NAME = "EU2AV";

	public const string EXTENDED_NAME = "extended version-eu2av";

	public static string GetString(bool bWithFirmware = true)
	{
		string text = "." + Common.GetRevision();
		if (text == ".0")
		{
			text = "";
		}
		string text2 = Common.GetVerNum() + text;
		string text3 = "Thetis-extended";
		string text4 = (Common.Is64Bit ? " x64" : " x86");
		text3 = text3 + " v" + text2 + text4;
		text3 = text3 + " (" + VersionInfo.BuildDate + ")<FW>";
		if (!bWithFirmware)
		{
			text3 = text3.Replace("<FW>", "");
		}
		return text3;
	}
}
