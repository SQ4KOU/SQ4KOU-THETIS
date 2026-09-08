using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundExpression : BoundNode
{
	public SimpleNameSyntax? InterceptableNameSyntax
	{
		get
		{
			if (base.WasCompilerGenerated || !(Syntax is InvocationExpressionSyntax invocation))
			{
				return null;
			}
			return invocation.GetInterceptableNameSyntax();
		}
	}

	public virtual ConstantValue? ConstantValueOpt => null;

	public virtual Symbol? ExpressionSymbol => null;

	public virtual LookupResultKind ResultKind => LookupResultKind.Viable;

	public virtual bool SuppressVirtualCalls => false;

	public new NullabilityInfo TopLevelNullability
	{
		get
		{
			return base.TopLevelNullability;
		}
		set
		{
			base.TopLevelNullability = value;
		}
	}

	public virtual bool IsEquivalentToThisReference => false;

	public virtual object Display => Type;

	public TypeSymbol? Type { get; }

	internal BoundExpression WithSuppression(bool suppress = true)
	{
		if (base.IsSuppressed == suppress)
		{
			return this;
		}
		BoundExpression obj = (BoundExpression)MemberwiseClone();
		obj.IsSuppressed = suppress;
		return obj;
	}

	internal BoundExpression WithWasConverted()
	{
		return this;
	}

	internal new BoundExpression WithHasErrors()
	{
		return (BoundExpression)base.WithHasErrors();
	}

	internal bool NeedsToBeConverted()
	{
		switch (base.Kind)
		{
		case BoundKind.UnconvertedConditionalOperator:
		case BoundKind.DefaultLiteral:
		case BoundKind.UnconvertedSwitchExpression:
		case BoundKind.UnconvertedObjectCreationExpression:
		case BoundKind.UnconvertedCollectionExpression:
		case BoundKind.TupleLiteral:
		case BoundKind.UnconvertedInterpolatedString:
			return true;
		case BoundKind.StackAllocArrayCreation:
			return (object)Type == null;
		case BoundKind.BinaryOperator:
			return ((BoundBinaryOperator)this).IsUnconvertedInterpolatedStringAddition;
		default:
			return false;
		}
	}

	public ITypeSymbol? GetPublicTypeSymbol()
	{
		return Type?.GetITypeSymbol(TopLevelNullability.FlowState.ToAnnotation());
	}

	protected BoundExpression(BoundKind kind, SyntaxNode syntax, TypeSymbol? type, bool hasErrors)
		: base(kind, syntax, hasErrors)
	{
		Type = type;
	}

	protected BoundExpression(BoundKind kind, SyntaxNode syntax, TypeSymbol? type)
		: base(kind, syntax)
	{
		Type = type;
	}
}
