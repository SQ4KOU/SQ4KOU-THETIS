namespace Microsoft.CodeAnalysis.Operations;

internal sealed class MethodReferenceOperation : BaseMemberReferenceOperation, IMethodReferenceOperation, IMemberReferenceOperation, IOperation
{
	public IMethodSymbol Method { get; }

	public override ITypeSymbol? ConstrainedToType { get; }

	public bool IsVirtual { get; }

	internal override int ChildOperationsCount => (base.Instance != null) ? 1 : 0;

	public override ITypeSymbol? Type { get; }

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.MethodReference;

	public override ISymbol Member => Method;

	internal MethodReferenceOperation(IMethodSymbol method, ITypeSymbol? constrainedToType, bool isVirtual, IOperation? instance, SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, bool isImplicit)
		: base(instance, semanticModel, syntax, isImplicit)
	{
		Method = method;
		ConstrainedToType = constrainedToType;
		IsVirtual = isVirtual;
		Type = type;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		if (slot == 0 && base.Instance != null)
		{
			return base.Instance;
		}
		throw ExceptionUtilities.UnexpectedValue((slot, index));
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNext(int previousSlot, int previousIndex)
	{
		if (previousSlot != -1)
		{
			if ((uint)previousSlot > 1u)
			{
				throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
			}
		}
		else if (base.Instance != null)
		{
			return (hasNext: true, nextSlot: 0, nextIndex: 0);
		}
		return (hasNext: false, nextSlot: 1, nextIndex: 0);
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNextReversed(int previousSlot, int previousIndex)
	{
		if ((uint)(previousSlot - -1) > 1u)
		{
			if (previousSlot != int.MaxValue)
			{
				throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
			}
			if (base.Instance != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitMethodReference(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitMethodReference(this, argument);
	}
}
