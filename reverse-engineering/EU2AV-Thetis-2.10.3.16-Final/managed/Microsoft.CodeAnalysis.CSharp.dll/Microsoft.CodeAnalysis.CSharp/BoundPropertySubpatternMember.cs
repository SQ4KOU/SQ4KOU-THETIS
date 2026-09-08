using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundPropertySubpatternMember : BoundNode
{
	public BoundPropertySubpatternMember? Receiver { get; }

	public Symbol? Symbol { get; }

	public TypeSymbol Type { get; }

	public BoundPropertySubpatternMember(SyntaxNode syntax, BoundPropertySubpatternMember? receiver, Symbol? symbol, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.PropertySubpatternMember, syntax, hasErrors || receiver.HasErrors())
	{
		Receiver = receiver;
		Symbol = symbol;
		Type = type;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitPropertySubpatternMember(this);
	}

	public BoundPropertySubpatternMember Update(BoundPropertySubpatternMember? receiver, Symbol? symbol, TypeSymbol type)
	{
		if (receiver != Receiver || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(symbol, Symbol) || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundPropertySubpatternMember boundPropertySubpatternMember = new BoundPropertySubpatternMember(Syntax, receiver, symbol, type, base.HasErrors);
			boundPropertySubpatternMember.CopyAttributes(this);
			return boundPropertySubpatternMember;
		}
		return this;
	}
}
