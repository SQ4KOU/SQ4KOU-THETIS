using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundArrayCreation : BoundExpression
{
	public new bool IsParamsArrayOrCollection
	{
		get
		{
			return base.IsParamsArrayOrCollection;
		}
		init
		{
			base.IsParamsArrayOrCollection = value;
		}
	}

	public new TypeSymbol Type => base.Type;

	public ImmutableArray<BoundExpression> Bounds { get; }

	public BoundArrayInitialization? InitializerOpt { get; }

	public BoundArrayCreation(SyntaxNode syntax, ImmutableArray<BoundExpression> bounds, BoundArrayInitialization? initializerOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ArrayCreation, syntax, type, hasErrors || bounds.HasErrors() || initializerOpt.HasErrors())
	{
		Bounds = bounds;
		InitializerOpt = initializerOpt;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitArrayCreation(this);
	}

	public BoundArrayCreation Update(ImmutableArray<BoundExpression> bounds, BoundArrayInitialization? initializerOpt, TypeSymbol type)
	{
		if (bounds != Bounds || initializerOpt != InitializerOpt || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundArrayCreation boundArrayCreation = new BoundArrayCreation(Syntax, bounds, initializerOpt, type, base.HasErrors);
			boundArrayCreation.CopyAttributes(this);
			return boundArrayCreation;
		}
		return this;
	}
}
