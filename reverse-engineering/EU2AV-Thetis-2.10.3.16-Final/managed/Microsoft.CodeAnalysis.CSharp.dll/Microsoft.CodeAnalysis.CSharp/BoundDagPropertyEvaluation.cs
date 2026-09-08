using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDagPropertyEvaluation : BoundDagEvaluation
{
	public PropertySymbol Property { get; }

	public bool IsLengthOrCount { get; }

	public BoundDagPropertyEvaluation(SyntaxNode syntax, PropertySymbol property, bool isLengthOrCount, BoundDagTemp input, bool hasErrors = false)
		: base(BoundKind.DagPropertyEvaluation, syntax, input, hasErrors || input.HasErrors())
	{
		Property = property;
		IsLengthOrCount = isLengthOrCount;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDagPropertyEvaluation(this);
	}

	public BoundDagPropertyEvaluation Update(PropertySymbol property, bool isLengthOrCount, BoundDagTemp input)
	{
		if (!Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(property, Property) || isLengthOrCount != IsLengthOrCount || input != base.Input)
		{
			BoundDagPropertyEvaluation boundDagPropertyEvaluation = new BoundDagPropertyEvaluation(Syntax, property, isLengthOrCount, input, base.HasErrors);
			boundDagPropertyEvaluation.CopyAttributes(this);
			return boundDagPropertyEvaluation;
		}
		return this;
	}
}
