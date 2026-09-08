using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundNewT : BoundObjectCreationExpressionBase
{
	public override MethodSymbol? Constructor => null;

	public override ImmutableArray<BoundExpression> Arguments => ImmutableArray<BoundExpression>.Empty;

	public override ImmutableArray<string?> ArgumentNamesOpt => default(ImmutableArray<string>);

	public override ImmutableArray<RefKind> ArgumentRefKindsOpt => default(ImmutableArray<RefKind>);

	public override bool Expanded => false;

	public override ImmutableArray<int> ArgsToParamsOpt => default(ImmutableArray<int>);

	public override BitVector DefaultArguments => default(BitVector);

	public override BoundObjectInitializerExpressionBase? InitializerExpressionOpt { get; }

	public override bool WasTargetTyped { get; }

	public BoundNewT(SyntaxNode syntax, BoundObjectInitializerExpressionBase? initializerExpressionOpt, bool wasTargetTyped, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.NewT, syntax, type, hasErrors || initializerExpressionOpt.HasErrors())
	{
		InitializerExpressionOpt = initializerExpressionOpt;
		WasTargetTyped = wasTargetTyped;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitNewT(this);
	}

	public BoundNewT Update(BoundObjectInitializerExpressionBase? initializerExpressionOpt, bool wasTargetTyped, TypeSymbol type)
	{
		if (initializerExpressionOpt != InitializerExpressionOpt || wasTargetTyped != WasTargetTyped || !TypeSymbol.Equals(type, base.Type, TypeCompareKind.ConsiderEverything))
		{
			BoundNewT boundNewT = new BoundNewT(Syntax, initializerExpressionOpt, wasTargetTyped, type, base.HasErrors);
			boundNewT.CopyAttributes(this);
			return boundNewT;
		}
		return this;
	}
}
