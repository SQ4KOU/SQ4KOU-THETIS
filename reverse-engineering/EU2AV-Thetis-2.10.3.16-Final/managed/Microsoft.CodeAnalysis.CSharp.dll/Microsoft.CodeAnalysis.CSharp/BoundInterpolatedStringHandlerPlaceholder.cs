using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundInterpolatedStringHandlerPlaceholder : BoundValuePlaceholderBase
{
	public sealed override bool IsEquivalentToThisReference => false;

	public BoundInterpolatedStringHandlerPlaceholder(SyntaxNode syntax, TypeSymbol? type, bool hasErrors)
		: base(BoundKind.InterpolatedStringHandlerPlaceholder, syntax, type, hasErrors)
	{
	}

	public BoundInterpolatedStringHandlerPlaceholder(SyntaxNode syntax, TypeSymbol? type)
		: base(BoundKind.InterpolatedStringHandlerPlaceholder, syntax, type)
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitInterpolatedStringHandlerPlaceholder(this);
	}

	public BoundInterpolatedStringHandlerPlaceholder Update(TypeSymbol? type)
	{
		if (!TypeSymbol.Equals(type, base.Type, TypeCompareKind.ConsiderEverything))
		{
			BoundInterpolatedStringHandlerPlaceholder boundInterpolatedStringHandlerPlaceholder = new BoundInterpolatedStringHandlerPlaceholder(Syntax, type, base.HasErrors);
			boundInterpolatedStringHandlerPlaceholder.CopyAttributes(this);
			return boundInterpolatedStringHandlerPlaceholder;
		}
		return this;
	}
}
