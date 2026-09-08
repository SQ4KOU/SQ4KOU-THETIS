using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundNameOfOperator : BoundExpression
{
	protected override ImmutableArray<BoundNode?> Children => ImmutableArray.Create((BoundNode)Argument);

	public new TypeSymbol Type => base.Type;

	public BoundExpression Argument { get; }

	public override ConstantValue ConstantValueOpt { get; }

	public BoundNameOfOperator(SyntaxNode syntax, BoundExpression argument, ConstantValue constantValueOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.NameOfOperator, syntax, type, hasErrors || argument.HasErrors())
	{
		Argument = argument;
		ConstantValueOpt = constantValueOpt;
	}

	[Conditional("DEBUG")]
	private void Validate()
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitNameOfOperator(this);
	}

	public BoundNameOfOperator Update(BoundExpression argument, ConstantValue constantValueOpt, TypeSymbol type)
	{
		if (argument != Argument || constantValueOpt != ConstantValueOpt || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundNameOfOperator boundNameOfOperator = new BoundNameOfOperator(Syntax, argument, constantValueOpt, type, base.HasErrors);
			boundNameOfOperator.CopyAttributes(this);
			return boundNameOfOperator;
		}
		return this;
	}
}
