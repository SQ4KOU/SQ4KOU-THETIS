using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class LocalInProgressBinder : Binder
{
	public readonly EqualsValueClauseSyntax InitializerSyntax;

	private LocalSymbol? _localSymbol;

	internal override LocalSymbol LocalInProgress => _localSymbol;

	internal LocalInProgressBinder(EqualsValueClauseSyntax initializerSyntax, Binder next)
		: base(next)
	{
		InitializerSyntax = initializerSyntax;
	}

	internal void SetLocalSymbol(LocalSymbol local)
	{
		Interlocked.CompareExchange(ref _localSymbol, local, null);
	}
}
