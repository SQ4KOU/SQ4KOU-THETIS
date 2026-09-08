namespace ExCSS;

public interface IMediaFeature : IStylesheetNode, IStyleFormattable
{
	string Name { get; }

	string Value { get; }

	bool HasValue { get; }
}
