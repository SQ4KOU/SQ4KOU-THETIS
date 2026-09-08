namespace Markdig.Extensions.Tables;

public class PipeTableOptions
{
	public bool RequireHeaderSeparator { get; set; }

	public bool UseHeaderForColumnCount { get; set; }

	public bool InferColumnWidthsFromSeparator { get; set; }

	public PipeTableOptions()
	{
		RequireHeaderSeparator = true;
		UseHeaderForColumnCount = false;
	}
}
