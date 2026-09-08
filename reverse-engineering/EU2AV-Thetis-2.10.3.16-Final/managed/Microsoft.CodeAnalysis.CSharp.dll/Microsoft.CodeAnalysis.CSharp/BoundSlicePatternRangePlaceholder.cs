using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundSlicePatternRangePlaceholder : BoundEarlyValuePlaceholderBase
{
	public sealed override bool IsEquivalentToThisReference
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/BoundTree/BoundExpression.cs", 217);
		}
	}

	public new TypeSymbol Type => base.Type;

	public BoundSlicePatternRangePlaceholder(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.SlicePatternRangePlaceholder, syntax, type, hasErrors)
	{
	}

	public BoundSlicePatternRangePlaceholder(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.SlicePatternRangePlaceholder, syntax, type)
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitSlicePatternRangePlaceholder(this);
	}

	public BoundSlicePatternRangePlaceholder Update(TypeSymbol type)
	{
		if (!TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundSlicePatternRangePlaceholder boundSlicePatternRangePlaceholder = new BoundSlicePatternRangePlaceholder(Syntax, type, base.HasErrors);
			boundSlicePatternRangePlaceholder.CopyAttributes(this);
			return boundSlicePatternRangePlaceholder;
		}
		return this;
	}
}
