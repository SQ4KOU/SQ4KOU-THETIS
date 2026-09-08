using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ParamsCollectionTypeInProgressBinder : Binder
{
	private readonly NamedTypeSymbol _inProgress;

	private readonly MethodSymbol? _constructorInProgress;

	internal override NamedTypeSymbol ParamsCollectionTypeInProgress => _inProgress;

	internal override MethodSymbol? ParamsCollectionConstructorInProgress => _constructorInProgress;

	internal ParamsCollectionTypeInProgressBinder(NamedTypeSymbol inProgress, Binder next, MethodSymbol? constructorInProgress = null)
		: base(next, (BinderFlags)((uint)next.Flags | 0x80000000u))
	{
		_inProgress = inProgress;
		_constructorInProgress = constructorInProgress;
	}
}
