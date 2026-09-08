using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundArrayInitialization : BoundExpression
{
	public new TypeSymbol? Type => base.Type;

	public bool IsInferred { get; }

	public ImmutableArray<BoundExpression> Initializers { get; }

	public BoundArrayInitialization Update(ImmutableArray<BoundExpression> initializers)
	{
		return Update(IsInferred, initializers);
	}

	public BoundArrayInitialization(SyntaxNode syntax, bool isInferred, ImmutableArray<BoundExpression> initializers, bool hasErrors = false)
		: base(BoundKind.ArrayInitialization, syntax, null, hasErrors || initializers.HasErrors())
	{
		IsInferred = isInferred;
		Initializers = initializers;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitArrayInitialization(this);
	}

	public BoundArrayInitialization Update(bool isInferred, ImmutableArray<BoundExpression> initializers)
	{
		if (isInferred != IsInferred || initializers != Initializers)
		{
			BoundArrayInitialization boundArrayInitialization = new BoundArrayInitialization(Syntax, isInferred, initializers, base.HasErrors);
			boundArrayInitialization.CopyAttributes(this);
			return boundArrayInitialization;
		}
		return this;
	}
}
