namespace Thetis;

public class SkinFileDownload
{
	public string Path { get; set; }

	public long BytesDownloaded { get; set; }

	public long TotalBytes { get; set; }

	public string Url { get; set; }

	public string FinalUri { get; set; }

	public bool Complete { get; set; }

	public bool Cancelled { get; set; }

	public int PercentageDownloaded { get; set; }

	public bool BypassRootFolderCheck { get; set; }

	public bool IsMeterSkin { get; set; }
}
