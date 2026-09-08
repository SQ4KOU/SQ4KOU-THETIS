using System.Diagnostics.CodeAnalysis;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundDagEvaluation : BoundDagTest
{
	private Symbol? Symbol
	{
		get
		{
			if (!(this is BoundDagFieldEvaluation boundDagFieldEvaluation))
			{
				if (!(this is BoundDagPropertyEvaluation boundDagPropertyEvaluation))
				{
					if (!(this is BoundDagTypeEvaluation boundDagTypeEvaluation))
					{
						if (!(this is BoundDagDeconstructEvaluation boundDagDeconstructEvaluation))
						{
							if (!(this is BoundDagIndexEvaluation boundDagIndexEvaluation))
							{
								if (!(this is BoundDagSliceEvaluation boundDagSliceEvaluation))
								{
									if (!(this is BoundDagIndexerEvaluation boundDagIndexerEvaluation))
									{
										if (this is BoundDagAssignmentEvaluation)
										{
											return null;
										}
										throw ExceptionUtilities.UnexpectedValue(base.Kind);
									}
									return getSymbolFromIndexerAccess(boundDagIndexerEvaluation.IndexerAccess);
								}
								return getSymbolFromIndexerAccess(boundDagSliceEvaluation.IndexerAccess);
							}
							return boundDagIndexEvaluation.Property;
						}
						return boundDagDeconstructEvaluation.DeconstructMethod;
					}
					return boundDagTypeEvaluation.Type;
				}
				return boundDagPropertyEvaluation.Property;
			}
			return boundDagFieldEvaluation.Field.CorrespondingTupleField ?? boundDagFieldEvaluation.Field;
			static Symbol? getSymbolFromIndexerAccess(BoundExpression indexerAccess)
			{
				if (indexerAccess is BoundArrayAccess boundArrayAccess)
				{
					return boundArrayAccess.Expression.Type;
				}
				if (indexerAccess is BoundImplicitIndexerAccess { IndexerOrSliceAccess: BoundArrayAccess indexerOrSliceAccess })
				{
					return indexerOrSliceAccess.Expression.Type;
				}
				return Binder.GetIndexerOrImplicitIndexerSymbol(indexerAccess);
			}
		}
	}

	public sealed override bool Equals([NotNullWhen(true)] object? obj)
	{
		if (obj is BoundDagEvaluation other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(BoundDagEvaluation other)
	{
		if (this != other)
		{
			if (IsEquivalentTo(other))
			{
				return base.Input.Equals(other.Input);
			}
			return false;
		}
		return true;
	}

	public virtual bool IsEquivalentTo(BoundDagEvaluation other)
	{
		if (this != other)
		{
			if (base.Kind == other.Kind)
			{
				return Microsoft.CodeAnalysis.CSharp.Symbol.Equals(Symbol, other.Symbol, TypeCompareKind.AllIgnoreOptions);
			}
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(base.Input.GetHashCode(), Symbol?.GetHashCode() ?? 0);
	}

	protected BoundDagEvaluation(BoundKind kind, SyntaxNode syntax, BoundDagTemp input, bool hasErrors = false)
		: base(kind, syntax, input, hasErrors)
	{
	}
}
