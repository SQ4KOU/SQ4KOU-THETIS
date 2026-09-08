using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundTypeOrValueExpression : BoundExpression
{
	public new TypeSymbol Type => base.Type;

	public Binder Binder { get; }

	public Symbol ValueSymbol { get; }

	public BoundTypeOrValueExpression(SyntaxNode syntax, Binder binder, Symbol valueSymbol, TypeSymbol type, bool hasErrors)
		: base(BoundKind.TypeOrValueExpression, syntax, type, hasErrors)
	{
		Binder = binder;
		ValueSymbol = valueSymbol;
	}

	[Conditional("DEBUG")]
	private void Validate()
	{
	}

	public BoundTypeOrValueExpression(SyntaxNode syntax, Binder binder, Symbol valueSymbol, TypeSymbol type)
		: base(BoundKind.TypeOrValueExpression, syntax, type)
	{
		Binder = binder;
		ValueSymbol = valueSymbol;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTypeOrValueExpression(this);
	}

	public BoundTypeOrValueExpression Update(Binder binder, Symbol valueSymbol, TypeSymbol type)
	{
		if (binder != Binder || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(valueSymbol, ValueSymbol) || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundTypeOrValueExpression boundTypeOrValueExpression = new BoundTypeOrValueExpression(Syntax, binder, valueSymbol, type, base.HasErrors);
			boundTypeOrValueExpression.CopyAttributes(this);
			return boundTypeOrValueExpression;
		}
		return this;
	}
}
