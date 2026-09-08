using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundInlineArrayAccess : BoundExpression
{
	public new TypeSymbol Type => base.Type;

	public BoundExpression Expression { get; }

	public BoundExpression Argument { get; }

	public bool IsValue { get; }

	public WellKnownMember GetItemOrSliceHelper { get; }

	public BoundInlineArrayAccess(SyntaxNode syntax, BoundExpression expression, BoundExpression argument, bool isValue, WellKnownMember getItemOrSliceHelper, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.InlineArrayAccess, syntax, type, hasErrors || expression.HasErrors() || argument.HasErrors())
	{
		Expression = expression;
		Argument = argument;
		IsValue = isValue;
		GetItemOrSliceHelper = getItemOrSliceHelper;
	}

	[Conditional("DEBUG")]
	private void Validate()
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitInlineArrayAccess(this);
	}

	public BoundInlineArrayAccess Update(BoundExpression expression, BoundExpression argument, bool isValue, WellKnownMember getItemOrSliceHelper, TypeSymbol type)
	{
		if (expression != Expression || argument != Argument || isValue != IsValue || getItemOrSliceHelper != GetItemOrSliceHelper || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundInlineArrayAccess boundInlineArrayAccess = new BoundInlineArrayAccess(Syntax, expression, argument, isValue, getItemOrSliceHelper, type, base.HasErrors);
			boundInlineArrayAccess.CopyAttributes(this);
			return boundInlineArrayAccess;
		}
		return this;
	}
}
