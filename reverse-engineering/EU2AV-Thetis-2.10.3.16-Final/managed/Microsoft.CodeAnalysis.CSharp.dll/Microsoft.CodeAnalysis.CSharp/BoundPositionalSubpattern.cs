using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundPositionalSubpattern : BoundSubpattern
{
	public Symbol? Symbol { get; }

	internal BoundPositionalSubpattern WithPattern(BoundPattern pattern)
	{
		return Update(Symbol, pattern);
	}

	public BoundPositionalSubpattern(SyntaxNode syntax, Symbol? symbol, BoundPattern pattern, bool hasErrors = false)
		: base(BoundKind.PositionalSubpattern, syntax, pattern, hasErrors || pattern.HasErrors())
	{
		Symbol = symbol;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitPositionalSubpattern(this);
	}

	public BoundPositionalSubpattern Update(Symbol? symbol, BoundPattern pattern)
	{
		if (!Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(symbol, Symbol) || pattern != base.Pattern)
		{
			BoundPositionalSubpattern boundPositionalSubpattern = new BoundPositionalSubpattern(Syntax, symbol, pattern, base.HasErrors);
			boundPositionalSubpattern.CopyAttributes(this);
			return boundPositionalSubpattern;
		}
		return this;
	}
}
