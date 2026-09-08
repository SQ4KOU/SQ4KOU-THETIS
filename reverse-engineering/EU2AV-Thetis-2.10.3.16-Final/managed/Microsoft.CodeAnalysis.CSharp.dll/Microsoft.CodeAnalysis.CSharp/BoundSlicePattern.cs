using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundSlicePattern : BoundPattern
{
	public BoundPattern? Pattern { get; }

	public BoundExpression? IndexerAccess { get; }

	public BoundSlicePatternReceiverPlaceholder? ReceiverPlaceholder { get; }

	public BoundSlicePatternRangePlaceholder? ArgumentPlaceholder { get; }

	internal BoundSlicePattern WithPattern(BoundPattern? pattern)
	{
		return Update(pattern, IndexerAccess, ReceiverPlaceholder, ArgumentPlaceholder, base.InputType, base.NarrowedType);
	}

	public BoundSlicePattern(SyntaxNode syntax, BoundPattern? pattern, BoundExpression? indexerAccess, BoundSlicePatternReceiverPlaceholder? receiverPlaceholder, BoundSlicePatternRangePlaceholder? argumentPlaceholder, TypeSymbol inputType, TypeSymbol narrowedType, bool hasErrors = false)
		: base(BoundKind.SlicePattern, syntax, inputType, narrowedType, hasErrors || pattern.HasErrors() || indexerAccess.HasErrors() || receiverPlaceholder.HasErrors() || argumentPlaceholder.HasErrors())
	{
		Pattern = pattern;
		IndexerAccess = indexerAccess;
		ReceiverPlaceholder = receiverPlaceholder;
		ArgumentPlaceholder = argumentPlaceholder;
	}

	[Conditional("DEBUG")]
	private void Validate()
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitSlicePattern(this);
	}

	public BoundSlicePattern Update(BoundPattern? pattern, BoundExpression? indexerAccess, BoundSlicePatternReceiverPlaceholder? receiverPlaceholder, BoundSlicePatternRangePlaceholder? argumentPlaceholder, TypeSymbol inputType, TypeSymbol narrowedType)
	{
		if (pattern != Pattern || indexerAccess != IndexerAccess || receiverPlaceholder != ReceiverPlaceholder || argumentPlaceholder != ArgumentPlaceholder || !TypeSymbol.Equals(inputType, base.InputType, TypeCompareKind.ConsiderEverything) || !TypeSymbol.Equals(narrowedType, base.NarrowedType, TypeCompareKind.ConsiderEverything))
		{
			BoundSlicePattern boundSlicePattern = new BoundSlicePattern(Syntax, pattern, indexerAccess, receiverPlaceholder, argumentPlaceholder, inputType, narrowedType, base.HasErrors);
			boundSlicePattern.CopyAttributes(this);
			return boundSlicePattern;
		}
		return this;
	}
}
