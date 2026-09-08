using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class SimpleLocalScopeBinder : LocalScopeBinder
{
	private readonly ImmutableArray<LocalSymbol> _locals;

	public SimpleLocalScopeBinder(ImmutableArray<LocalSymbol> locals, Binder next)
		: base(next)
	{
		_locals = locals;
	}

	protected override ImmutableArray<LocalSymbol> BuildLocals()
	{
		return _locals;
	}

	internal override ImmutableArray<LocalSymbol> GetDeclaredLocalsForScope(SyntaxNode scopeDesignator)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/SimpleLocalScopeBinder.cs", 32);
	}

	internal override ImmutableArray<LocalFunctionSymbol> GetDeclaredLocalFunctionsForScope(CSharpSyntaxNode scopeDesignator)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/SimpleLocalScopeBinder.cs", 37);
	}
}
