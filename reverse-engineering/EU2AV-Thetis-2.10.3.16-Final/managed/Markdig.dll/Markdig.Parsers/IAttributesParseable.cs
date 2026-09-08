namespace Markdig.Parsers;

public interface IAttributesParseable
{
	TryParseAttributesDelegate? TryParseAttributes { get; set; }
}
