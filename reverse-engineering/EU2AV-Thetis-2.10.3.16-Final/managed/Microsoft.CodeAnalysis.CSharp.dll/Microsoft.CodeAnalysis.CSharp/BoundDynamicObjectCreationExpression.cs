using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDynamicObjectCreationExpression : BoundObjectCreationExpressionBase
{
	public override MethodSymbol? Constructor => null;

	public override bool Expanded => false;

	public override ImmutableArray<int> ArgsToParamsOpt => default(ImmutableArray<int>);

	public override BitVector DefaultArguments => default(BitVector);

	protected override ImmutableArray<BoundNode?> Children => StaticCast<BoundNode>.From(Arguments.AddRange(BoundObjectCreationExpression.GetChildInitializers(InitializerExpressionOpt)));

	public string Name { get; }

	public override ImmutableArray<BoundExpression> Arguments { get; }

	public override ImmutableArray<string?> ArgumentNamesOpt { get; }

	public override ImmutableArray<RefKind> ArgumentRefKindsOpt { get; }

	public override BoundObjectInitializerExpressionBase? InitializerExpressionOpt { get; }

	public ImmutableArray<MethodSymbol> ApplicableMethods { get; }

	public override bool WasTargetTyped { get; }

	public BoundDynamicObjectCreationExpression(SyntaxNode syntax, string name, ImmutableArray<BoundExpression> arguments, ImmutableArray<string?> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, BoundObjectInitializerExpressionBase? initializerExpressionOpt, ImmutableArray<MethodSymbol> applicableMethods, bool wasTargetTyped, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.DynamicObjectCreationExpression, syntax, type, hasErrors || arguments.HasErrors() || initializerExpressionOpt.HasErrors())
	{
		Name = name;
		Arguments = arguments;
		ArgumentNamesOpt = argumentNamesOpt;
		ArgumentRefKindsOpt = argumentRefKindsOpt;
		InitializerExpressionOpt = initializerExpressionOpt;
		ApplicableMethods = applicableMethods;
		WasTargetTyped = wasTargetTyped;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDynamicObjectCreationExpression(this);
	}

	public BoundDynamicObjectCreationExpression Update(string name, ImmutableArray<BoundExpression> arguments, ImmutableArray<string?> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, BoundObjectInitializerExpressionBase? initializerExpressionOpt, ImmutableArray<MethodSymbol> applicableMethods, bool wasTargetTyped, TypeSymbol type)
	{
		if (name != Name || arguments != Arguments || argumentNamesOpt != ArgumentNamesOpt || argumentRefKindsOpt != ArgumentRefKindsOpt || initializerExpressionOpt != InitializerExpressionOpt || applicableMethods != ApplicableMethods || wasTargetTyped != WasTargetTyped || !TypeSymbol.Equals(type, base.Type, TypeCompareKind.ConsiderEverything))
		{
			BoundDynamicObjectCreationExpression boundDynamicObjectCreationExpression = new BoundDynamicObjectCreationExpression(Syntax, name, arguments, argumentNamesOpt, argumentRefKindsOpt, initializerExpressionOpt, applicableMethods, wasTargetTyped, type, base.HasErrors);
			boundDynamicObjectCreationExpression.CopyAttributes(this);
			return boundDynamicObjectCreationExpression;
		}
		return this;
	}
}
