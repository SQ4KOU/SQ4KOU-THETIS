namespace Markdig.Syntax;

public interface IMarkdownObject
{
	void SetData(object key, object value);

	bool ContainsData(object key);

	object? GetData(object key);

	bool RemoveData(object key);
}
