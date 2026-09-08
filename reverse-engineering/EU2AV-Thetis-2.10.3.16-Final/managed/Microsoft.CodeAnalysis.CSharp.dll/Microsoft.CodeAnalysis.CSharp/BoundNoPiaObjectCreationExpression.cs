using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundNoPiaObjectCreationExpression : BoundObjectCreationExpressionBase
{
	public override MethodSymbol? Constructor => null;

	public override ImmutableArray<BoundExpression> Arguments => ImmutableArray<BoundExpression>.Empty;

	public override ImmutableArray<string?> ArgumentNamesOpt => default(ImmutableArray<string>);

	public override ImmutableArray<RefKind> ArgumentRefKindsOpt => default(ImmutableArray<RefKind>);

	public override bool Expanded => false;

	public override ImmutableArray<int> ArgsToParamsOpt => default(ImmutableArray<int>);

	public override BitVector DefaultArguments => default(BitVector);

	public string? GuidString { get; }

	public override BoundObjectInitializerExpressionBase? InitializerExpressionOpt { get; }

	public override bool WasTargetTyped { get; }

	public BoundNoPiaObjectCreationExpression(SyntaxNode syntax, string? guidString, BoundObjectInitializerExpressionBase? initializerExpressionOpt, bool wasTargetTyped, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.NoPiaObjectCreationExpression, syntax, type, hasErrors || initializerExpressionOpt.HasErrors())
	{
		GuidString = guidString;
		InitializerExpressionOpt = initializerExpressionOpt;
		WasTargetTyped = wasTargetTyped;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitNoPiaObjectCreationExpression(this);
	}

	public BoundNoPiaObjectCreationExpression Update(string? guidString, BoundObjectInitializerExpressionBase? initializerExpressionOpt, bool wasTargetTyped, TypeSymbol type)
	{
		if (guidString != GuidString || initializerExpressionOpt != InitializerExpressionOpt || wasTargetTyped != WasTargetTyped || !TypeSymbol.Equals(type, base.Type, TypeCompareKind.ConsiderEverything))
		{
			BoundNoPiaObjectCreationExpression boundNoPiaObjectCreationExpression = new BoundNoPiaObjectCreationExpression(Syntax, guidString, initializerExpressionOpt, wasTargetTyped, type, base.HasErrors);
			boundNoPiaObjectCreationExpression.CopyAttributes(this);
			return boundNoPiaObjectCreationExpression;
		}
		return this;
	}
}
