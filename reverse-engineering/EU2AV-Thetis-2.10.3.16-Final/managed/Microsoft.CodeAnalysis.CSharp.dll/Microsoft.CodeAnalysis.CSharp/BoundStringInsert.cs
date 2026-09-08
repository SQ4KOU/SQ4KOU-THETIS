using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundStringInsert : BoundExpression
{
	public new TypeSymbol? Type => base.Type;

	public BoundExpression Value { get; }

	public BoundExpression? Alignment { get; }

	public BoundLiteral? Format { get; }

	public bool IsInterpolatedStringHandlerAppendCall { get; }

	public BoundStringInsert(SyntaxNode syntax, BoundExpression value, BoundExpression? alignment, BoundLiteral? format, bool isInterpolatedStringHandlerAppendCall, bool hasErrors = false)
		: base(BoundKind.StringInsert, syntax, null, hasErrors || value.HasErrors() || alignment.HasErrors() || format.HasErrors())
	{
		Value = value;
		Alignment = alignment;
		Format = format;
		IsInterpolatedStringHandlerAppendCall = isInterpolatedStringHandlerAppendCall;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitStringInsert(this);
	}

	public BoundStringInsert Update(BoundExpression value, BoundExpression? alignment, BoundLiteral? format, bool isInterpolatedStringHandlerAppendCall)
	{
		if (value != Value || alignment != Alignment || format != Format || isInterpolatedStringHandlerAppendCall != IsInterpolatedStringHandlerAppendCall)
		{
			BoundStringInsert boundStringInsert = new BoundStringInsert(Syntax, value, alignment, format, isInterpolatedStringHandlerAppendCall, base.HasErrors);
			boundStringInsert.CopyAttributes(this);
			return boundStringInsert;
		}
		return this;
	}
}
