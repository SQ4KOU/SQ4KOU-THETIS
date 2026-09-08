using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundNegatedPattern : BoundPattern
{
	public BoundPattern Negated { get; }

	public BoundNegatedPattern(SyntaxNode syntax, BoundPattern negated, TypeSymbol inputType, TypeSymbol narrowedType, bool hasErrors = false)
		: base(BoundKind.NegatedPattern, syntax, inputType, narrowedType, hasErrors || negated.HasErrors())
	{
		Negated = negated;
	}

	[Conditional("DEBUG")]
	private void Validate()
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitNegatedPattern(this);
	}

	public BoundNegatedPattern Update(BoundPattern negated, TypeSymbol inputType, TypeSymbol narrowedType)
	{
		if (negated != Negated || !TypeSymbol.Equals(inputType, base.InputType, TypeCompareKind.ConsiderEverything) || !TypeSymbol.Equals(narrowedType, base.NarrowedType, TypeCompareKind.ConsiderEverything))
		{
			BoundNegatedPattern boundNegatedPattern = new BoundNegatedPattern(Syntax, negated, inputType, narrowedType, base.HasErrors);
			boundNegatedPattern.CopyAttributes(this);
			return boundNegatedPattern;
		}
		return this;
	}
}
