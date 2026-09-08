using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundInterpolatedString : BoundInterpolatedStringBase
{
	public const string AppendFormattedMethod = "AppendFormatted";

	public const string AppendLiteralMethod = "AppendLiteral";

	public InterpolatedStringHandlerData? InterpolationData { get; }

	public BoundInterpolatedString(SyntaxNode syntax, InterpolatedStringHandlerData? interpolationData, ImmutableArray<BoundExpression> parts, ConstantValue? constantValueOpt, TypeSymbol? type, bool hasErrors = false)
		: base(BoundKind.InterpolatedString, syntax, parts, constantValueOpt, type, hasErrors || parts.HasErrors())
	{
		InterpolationData = interpolationData;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitInterpolatedString(this);
	}

	public BoundInterpolatedString Update(InterpolatedStringHandlerData? interpolationData, ImmutableArray<BoundExpression> parts, ConstantValue? constantValueOpt, TypeSymbol? type)
	{
		if (!interpolationData.Equals(InterpolationData) || parts != base.Parts || constantValueOpt != ConstantValueOpt || !TypeSymbol.Equals(type, base.Type, TypeCompareKind.ConsiderEverything))
		{
			BoundInterpolatedString boundInterpolatedString = new BoundInterpolatedString(Syntax, interpolationData, parts, constantValueOpt, type, base.HasErrors);
			boundInterpolatedString.CopyAttributes(this);
			return boundInterpolatedString;
		}
		return this;
	}
}
