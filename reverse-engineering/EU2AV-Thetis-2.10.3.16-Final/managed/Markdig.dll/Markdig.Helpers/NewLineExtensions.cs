namespace Markdig.Helpers;

public static class NewLineExtensions
{
	public static string AsString(this NewLine newLine)
	{
		return newLine switch
		{
			NewLine.CarriageReturnLineFeed => "\r\n", 
			NewLine.LineFeed => "\n", 
			NewLine.CarriageReturn => "\r", 
			_ => string.Empty, 
		};
	}

	public static int Length(this NewLine newLine)
	{
		return (int)(newLine & (NewLine)3);
	}
}
