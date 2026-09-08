using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundListPatternIndexPlaceholder : BoundEarlyValuePlaceholderBase
{
	public sealed override bool IsEquivalentToThisReference
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/BoundTree/BoundExpression.cs", 207);
		}
	}

	public new TypeSymbol Type => base.Type;

	public BoundListPatternIndexPlaceholder(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.ListPatternIndexPlaceholder, syntax, type, hasErrors)
	{
	}

	public BoundListPatternIndexPlaceholder(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.ListPatternIndexPlaceholder, syntax, type)
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitListPatternIndexPlaceholder(this);
	}

	public BoundListPatternIndexPlaceholder Update(TypeSymbol type)
	{
		if (!TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundListPatternIndexPlaceholder boundListPatternIndexPlaceholder = new BoundListPatternIndexPlaceholder(Syntax, type, base.HasErrors);
			boundListPatternIndexPlaceholder.CopyAttributes(this);
			return boundListPatternIndexPlaceholder;
		}
		return this;
	}
}
