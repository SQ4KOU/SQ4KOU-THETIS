using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundThrowIfModuleCancellationRequested : BoundExpression
{
	public new TypeSymbol Type => base.Type;

	public BoundThrowIfModuleCancellationRequested(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.ThrowIfModuleCancellationRequested, syntax, type, hasErrors)
	{
	}

	public BoundThrowIfModuleCancellationRequested(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.ThrowIfModuleCancellationRequested, syntax, type)
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitThrowIfModuleCancellationRequested(this);
	}

	public BoundThrowIfModuleCancellationRequested Update(TypeSymbol type)
	{
		if (!TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundThrowIfModuleCancellationRequested boundThrowIfModuleCancellationRequested = new BoundThrowIfModuleCancellationRequested(Syntax, type, base.HasErrors);
			boundThrowIfModuleCancellationRequested.CopyAttributes(this);
			return boundThrowIfModuleCancellationRequested;
		}
		return this;
	}
}
