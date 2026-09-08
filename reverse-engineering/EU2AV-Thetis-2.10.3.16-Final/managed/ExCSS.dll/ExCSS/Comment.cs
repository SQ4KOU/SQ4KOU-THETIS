using System.IO;

namespace ExCSS;

internal sealed class Comment : StylesheetNode
{
	public string Data { get; }

	public Comment(string data)
	{
		Data = data;
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		writer.Write(formatter.Comment(Data));
	}
}
