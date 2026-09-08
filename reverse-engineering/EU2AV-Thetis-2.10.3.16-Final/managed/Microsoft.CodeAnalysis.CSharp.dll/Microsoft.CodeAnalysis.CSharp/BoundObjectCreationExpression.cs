using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundObjectCreationExpression : BoundObjectCreationExpressionBase, IBoundInvalidNode
{
	public override Symbol ExpressionSymbol => Constructor;

	ImmutableArray<BoundNode> IBoundInvalidNode.InvalidNodeChildren => CSharpOperationFactory.CreateInvalidChildrenFromArgumentsExpression(null, Arguments, InitializerExpressionOpt);

	public override MethodSymbol Constructor { get; }

	public ImmutableArray<MethodSymbol> ConstructorsGroup { get; }

	public override ImmutableArray<BoundExpression> Arguments { get; }

	public override ImmutableArray<string?> ArgumentNamesOpt { get; }

	public override ImmutableArray<RefKind> ArgumentRefKindsOpt { get; }

	public override bool Expanded { get; }

	public override ImmutableArray<int> ArgsToParamsOpt { get; }

	public override BitVector DefaultArguments { get; }

	public override ConstantValue? ConstantValueOpt { get; }

	public override BoundObjectInitializerExpressionBase? InitializerExpressionOpt { get; }

	public override bool WasTargetTyped { get; }

	internal BoundObjectCreationExpression Update(MethodSymbol constructor, ImmutableArray<BoundExpression> newArguments, ImmutableArray<RefKind> newRefKinds, BoundObjectInitializerExpressionBase? newInitializerExpression, TypeSymbol? changeTypeOpt = null)
	{
		return Update(constructor, newArguments, default(ImmutableArray<string>), newRefKinds, expanded: false, default(ImmutableArray<int>), default(BitVector), ConstantValueOpt, newInitializerExpression, changeTypeOpt ?? base.Type);
	}

	public BoundObjectCreationExpression(SyntaxNode syntax, MethodSymbol constructor, ImmutableArray<BoundExpression> arguments, ImmutableArray<string?> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, bool expanded, ImmutableArray<int> argsToParamsOpt, BitVector defaultArguments, ConstantValue? constantValueOpt, BoundObjectInitializerExpressionBase? initializerExpressionOpt, TypeSymbol type, bool hasErrors = false)
		: this(syntax, constructor, ImmutableArray<MethodSymbol>.Empty, arguments, argumentNamesOpt, argumentRefKindsOpt, expanded, argsToParamsOpt, defaultArguments, constantValueOpt, initializerExpressionOpt, wasTargetTyped: false, type, hasErrors)
	{
	}

	public BoundObjectCreationExpression Update(MethodSymbol constructor, ImmutableArray<BoundExpression> arguments, ImmutableArray<string?> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, bool expanded, ImmutableArray<int> argsToParamsOpt, BitVector defaultArguments, ConstantValue? constantValueOpt, BoundObjectInitializerExpressionBase? initializerExpressionOpt, TypeSymbol type)
	{
		return Update(constructor, ImmutableArray<MethodSymbol>.Empty, arguments, argumentNamesOpt, argumentRefKindsOpt, expanded, argsToParamsOpt, defaultArguments, constantValueOpt, initializerExpressionOpt, WasTargetTyped, type);
	}

	public BoundObjectCreationExpression Update(MethodSymbol constructor, ImmutableArray<MethodSymbol> constructorsGroup, ImmutableArray<BoundExpression> arguments, ImmutableArray<string?> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, bool expanded, ImmutableArray<int> argsToParamsOpt, BitVector defaultArguments, ConstantValue? constantValueOpt, BoundObjectInitializerExpressionBase? initializerExpressionOpt, TypeSymbol type)
	{
		return Update(constructor, constructorsGroup, arguments, argumentNamesOpt, argumentRefKindsOpt, expanded, argsToParamsOpt, defaultArguments, constantValueOpt, initializerExpressionOpt, WasTargetTyped, type);
	}

	public BoundObjectCreationExpression(SyntaxNode syntax, MethodSymbol constructor, params BoundExpression[] arguments)
		: this(syntax, constructor, ImmutableArray.Create(arguments), default(ImmutableArray<string>), default(ImmutableArray<RefKind>), expanded: false, default(ImmutableArray<int>), default(BitVector), null, null, constructor.ContainingType)
	{
	}

	public BoundObjectCreationExpression(SyntaxNode syntax, MethodSymbol constructor, ImmutableArray<BoundExpression> arguments)
		: this(syntax, constructor, arguments, default(ImmutableArray<string>), default(ImmutableArray<RefKind>), expanded: false, default(ImmutableArray<int>), default(BitVector), null, null, constructor.ContainingType)
	{
	}

	internal static ImmutableArray<BoundExpression> GetChildInitializers(BoundExpression? objectOrCollectionInitializer)
	{
		if (objectOrCollectionInitializer is BoundObjectInitializerExpression boundObjectInitializerExpression)
		{
			return boundObjectInitializerExpression.Initializers;
		}
		if (objectOrCollectionInitializer is BoundCollectionInitializerExpression boundCollectionInitializerExpression)
		{
			return boundCollectionInitializerExpression.Initializers;
		}
		return ImmutableArray<BoundExpression>.Empty;
	}

	public BoundObjectCreationExpression(SyntaxNode syntax, MethodSymbol constructor, ImmutableArray<MethodSymbol> constructorsGroup, ImmutableArray<BoundExpression> arguments, ImmutableArray<string?> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, bool expanded, ImmutableArray<int> argsToParamsOpt, BitVector defaultArguments, ConstantValue? constantValueOpt, BoundObjectInitializerExpressionBase? initializerExpressionOpt, bool wasTargetTyped, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ObjectCreationExpression, syntax, type, hasErrors || arguments.HasErrors() || initializerExpressionOpt.HasErrors())
	{
		Constructor = constructor;
		ConstructorsGroup = constructorsGroup;
		Arguments = arguments;
		ArgumentNamesOpt = argumentNamesOpt;
		ArgumentRefKindsOpt = argumentRefKindsOpt;
		Expanded = expanded;
		ArgsToParamsOpt = argsToParamsOpt;
		DefaultArguments = defaultArguments;
		ConstantValueOpt = constantValueOpt;
		InitializerExpressionOpt = initializerExpressionOpt;
		WasTargetTyped = wasTargetTyped;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitObjectCreationExpression(this);
	}

	public BoundObjectCreationExpression Update(MethodSymbol constructor, ImmutableArray<MethodSymbol> constructorsGroup, ImmutableArray<BoundExpression> arguments, ImmutableArray<string?> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, bool expanded, ImmutableArray<int> argsToParamsOpt, BitVector defaultArguments, ConstantValue? constantValueOpt, BoundObjectInitializerExpressionBase? initializerExpressionOpt, bool wasTargetTyped, TypeSymbol type)
	{
		if (!Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(constructor, Constructor) || constructorsGroup != ConstructorsGroup || arguments != Arguments || argumentNamesOpt != ArgumentNamesOpt || argumentRefKindsOpt != ArgumentRefKindsOpt || expanded != Expanded || argsToParamsOpt != ArgsToParamsOpt || defaultArguments != DefaultArguments || constantValueOpt != ConstantValueOpt || initializerExpressionOpt != InitializerExpressionOpt || wasTargetTyped != WasTargetTyped || !TypeSymbol.Equals(type, base.Type, TypeCompareKind.ConsiderEverything))
		{
			BoundObjectCreationExpression boundObjectCreationExpression = new BoundObjectCreationExpression(Syntax, constructor, constructorsGroup, arguments, argumentNamesOpt, argumentRefKindsOpt, expanded, argsToParamsOpt, defaultArguments, constantValueOpt, initializerExpressionOpt, wasTargetTyped, type, base.HasErrors);
			boundObjectCreationExpression.CopyAttributes(this);
			return boundObjectCreationExpression;
		}
		return this;
	}
}
