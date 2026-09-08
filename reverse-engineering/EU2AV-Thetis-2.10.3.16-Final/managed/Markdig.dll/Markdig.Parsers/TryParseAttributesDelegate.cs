using Markdig.Helpers;
using Markdig.Syntax;

namespace Markdig.Parsers;

public delegate bool TryParseAttributesDelegate(BlockProcessor processor, ref StringSlice slice, IBlock block);
