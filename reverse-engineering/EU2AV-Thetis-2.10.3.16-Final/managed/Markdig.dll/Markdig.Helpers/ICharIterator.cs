namespace Markdig.Helpers;

public interface ICharIterator
{
	int Start { get; }

	char CurrentChar { get; }

	int End { get; }

	bool IsEmpty { get; }

	char NextChar();

	void SkipChar();

	char PeekChar();

	char PeekChar(int offset);

	bool TrimStart();
}
