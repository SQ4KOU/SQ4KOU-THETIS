namespace Microsoft.CodeAnalysis;

internal readonly record struct NodeStateEntry<T>(T Item, EntryState State, int OutputIndex, IncrementalGeneratorRunStep? Step);
