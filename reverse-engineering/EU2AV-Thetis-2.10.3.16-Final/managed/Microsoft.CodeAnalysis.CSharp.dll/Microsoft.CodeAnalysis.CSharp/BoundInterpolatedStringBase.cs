using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundInterpolatedStringBase : BoundExpression
{
	public ImmutableArray<BoundExpression> Parts { get; }

	public override ConstantValue? ConstantValueOpt { get; }

	protected BoundInterpolatedStringBase(BoundKind kind, SyntaxNode syntax, ImmutableArray<BoundExpression> parts, ConstantValue? constantValueOpt, TypeSymbol? type, bool hasErrors = false)
		: base(kind, syntax, type, hasErrors)
	{
		Parts = parts;
		ConstantValueOpt = constantValueOpt;
	}
}
