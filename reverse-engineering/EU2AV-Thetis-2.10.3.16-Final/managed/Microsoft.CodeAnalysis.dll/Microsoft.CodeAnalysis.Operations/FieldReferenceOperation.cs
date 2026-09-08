namespace Microsoft.CodeAnalysis.Operations;

internal sealed class FieldReferenceOperation : BaseMemberReferenceOperation, IFieldReferenceOperation, IMemberReferenceOperation, IOperation
{
	public IFieldSymbol Field { get; }

	public bool IsDeclaration { get; }

	internal override int ChildOperationsCount => (base.Instance != null) ? 1 : 0;

	public override ITypeSymbol? Type { get; }

	internal override ConstantValue? OperationConstantValue { get; }

	public override OperationKind Kind => OperationKind.FieldReference;

	public override ISymbol Member => Field;

	public override ITypeSymbol? ConstrainedToType => null;

	internal FieldReferenceOperation(IFieldSymbol field, bool isDeclaration, IOperation? instance, SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, ConstantValue? constantValue, bool isImplicit)
		: base(instance, semanticModel, syntax, isImplicit)
	{
		Field = field;
		IsDeclaration = isDeclaration;
		OperationConstantValue = constantValue;
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
		visitor.VisitFieldReference(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitFieldReference(this, argument);
	}
}
