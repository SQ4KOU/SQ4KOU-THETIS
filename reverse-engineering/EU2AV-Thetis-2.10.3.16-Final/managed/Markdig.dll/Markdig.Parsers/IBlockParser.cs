using Markdig.Syntax;

namespace Markdig.Parsers;

public interface IBlockParser<in TProcessor> : IMarkdownParser<TProcessor>
{
	bool CanInterrupt(TProcessor processor, Block block);

	BlockState TryOpen(TProcessor processor);

	BlockState TryContinue(TProcessor processor, Block block);

	bool Close(TProcessor processor, Block block);
}
