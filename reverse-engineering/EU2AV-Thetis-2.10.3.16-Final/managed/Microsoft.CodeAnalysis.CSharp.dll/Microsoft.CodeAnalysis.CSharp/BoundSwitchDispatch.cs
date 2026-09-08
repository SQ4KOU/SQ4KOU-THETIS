using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundSwitchDispatch : BoundStatement
{
	public BoundExpression Expression { get; }

	public ImmutableArray<(ConstantValue value, LabelSymbol label)> Cases { get; }

	public LabelSymbol DefaultLabel { get; }

	public LengthBasedStringSwitchData? LengthBasedStringSwitchDataOpt { get; }

	public BoundSwitchDispatch(SyntaxNode syntax, BoundExpression expression, ImmutableArray<(ConstantValue value, LabelSymbol label)> cases, LabelSymbol defaultLabel, LengthBasedStringSwitchData? lengthBasedStringSwitchDataOpt, bool hasErrors = false)
		: base(BoundKind.SwitchDispatch, syntax, hasErrors || expression.HasErrors())
	{
		Expression = expression;
		Cases = cases;
		DefaultLabel = defaultLabel;
		LengthBasedStringSwitchDataOpt = lengthBasedStringSwitchDataOpt;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitSwitchDispatch(this);
	}

	public BoundSwitchDispatch Update(BoundExpression expression, ImmutableArray<(ConstantValue value, LabelSymbol label)> cases, LabelSymbol defaultLabel, LengthBasedStringSwitchData? lengthBasedStringSwitchDataOpt)
	{
		if (expression != Expression || cases != Cases || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(defaultLabel, DefaultLabel) || lengthBasedStringSwitchDataOpt != LengthBasedStringSwitchDataOpt)
		{
			BoundSwitchDispatch boundSwitchDispatch = new BoundSwitchDispatch(Syntax, expression, cases, defaultLabel, lengthBasedStringSwitchDataOpt, base.HasErrors);
			boundSwitchDispatch.CopyAttributes(this);
			return boundSwitchDispatch;
		}
		return this;
	}
}
