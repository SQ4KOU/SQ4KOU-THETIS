using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class CollectionInitializerAddMethodBinder : Binder
{
	public SyntaxNode Syntax { get; }

	public TypeSymbol CollectionType { get; }

	internal CollectionInitializerAddMethodBinder(SyntaxNode syntax, TypeSymbol collectionType, Binder next)
		: base(next, next.Flags | BinderFlags.CollectionInitializerAddMethod)
	{
		Syntax = syntax;
		CollectionType = collectionType;
	}
}
