using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDagIndexEvaluation : BoundDagEvaluation
{
	public PropertySymbol Property { get; }

	public int Index { get; }

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ Index;
	}

	public override bool IsEquivalentTo(BoundDagEvaluation obj)
	{
		if (base.IsEquivalentTo(obj))
		{
			return Index == ((BoundDagIndexEvaluation)obj).Index;
		}
		return false;
	}

	public BoundDagIndexEvaluation(SyntaxNode syntax, PropertySymbol property, int index, BoundDagTemp input, bool hasErrors = false)
		: base(BoundKind.DagIndexEvaluation, syntax, input, hasErrors || input.HasErrors())
	{
		Property = property;
		Index = index;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDagIndexEvaluation(this);
	}

	public BoundDagIndexEvaluation Update(PropertySymbol property, int index, BoundDagTemp input)
	{
		if (!Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(property, Property) || index != Index || input != base.Input)
		{
			BoundDagIndexEvaluation boundDagIndexEvaluation = new BoundDagIndexEvaluation(Syntax, property, index, input, base.HasErrors);
			boundDagIndexEvaluation.CopyAttributes(this);
			return boundDagIndexEvaluation;
		}
		return this;
	}
}
