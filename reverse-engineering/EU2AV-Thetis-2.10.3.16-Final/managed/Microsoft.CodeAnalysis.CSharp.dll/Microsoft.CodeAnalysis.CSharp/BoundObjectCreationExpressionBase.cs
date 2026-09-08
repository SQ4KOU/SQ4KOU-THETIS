using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundObjectCreationExpressionBase : BoundExpression
{
	public abstract MethodSymbol? Constructor { get; }

	public abstract ImmutableArray<BoundExpression> Arguments { get; }

	public abstract ImmutableArray<string?> ArgumentNamesOpt { get; }

	public abstract ImmutableArray<RefKind> ArgumentRefKindsOpt { get; }

	public abstract bool Expanded { get; }

	public abstract ImmutableArray<int> ArgsToParamsOpt { get; }

	public abstract BitVector DefaultArguments { get; }

	public abstract BoundObjectInitializerExpressionBase? InitializerExpressionOpt { get; }

	public abstract bool WasTargetTyped { get; }

	public new TypeSymbol Type => base.Type;

	protected BoundObjectCreationExpressionBase(BoundKind kind, SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(kind, syntax, type, hasErrors)
	{
	}

	protected BoundObjectCreationExpressionBase(BoundKind kind, SyntaxNode syntax, TypeSymbol type)
		: base(kind, syntax, type)
	{
	}
}
