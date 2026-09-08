using System.IO;

namespace ExCSS;

internal abstract class DocumentFunction : StylesheetNode, IDocumentFunction, IStylesheetNode, IStyleFormattable
{
	public string Name { get; }

	public string Data { get; }

	internal DocumentFunction(string name, string data)
	{
		Name = name;
		Data = data;
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		writer.Write(Name.StylesheetFunction(Data.StylesheetString()));
	}

	public abstract bool Matches(Url url);
}
