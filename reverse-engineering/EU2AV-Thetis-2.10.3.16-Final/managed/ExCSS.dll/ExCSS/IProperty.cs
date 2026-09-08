namespace ExCSS;

public interface IProperty : IStylesheetNode, IStyleFormattable
{
	string Name { get; }

	string Value { get; }

	string Original { get; }

	bool IsImportant { get; }
}
