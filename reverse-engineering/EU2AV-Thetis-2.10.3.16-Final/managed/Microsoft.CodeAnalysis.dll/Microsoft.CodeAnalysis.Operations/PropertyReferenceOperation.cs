using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class PropertyReferenceOperation : BaseMemberReferenceOperation, IPropertyReferenceOperation, IMemberReferenceOperation, IOperation
{
	public IPropertySymbol Property { get; }

	public override ITypeSymbol? ConstrainedToType { get; }

	public ImmutableArray<IArgumentOperation> Arguments { get; }

	internal override int ChildOperationsCount => Arguments.Length + ((base.Instance != null) ? 1 : 0);

	public override ITypeSymbol? Type { get; }

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.PropertyReference;

	public override ISymbol Member => Property;

	internal PropertyReferenceOperation(IPropertySymbol property, ITypeSymbol? constrainedToType, ImmutableArray<IArgumentOperation> arguments, IOperation? instance, SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, bool isImplicit)
		: base(instance, semanticModel, syntax, isImplicit)
	{
		Property = property;
		ConstrainedToType = constrainedToType;
		Arguments = Operation.SetParentOperation(arguments, this);
		Type = type;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (base.Instance != null)
			{
				return base.Instance;
			}
			break;
		case 1:
			if (index < Arguments.Length)
			{
				return Arguments[index];
			}
			break;
		}
		throw ExceptionUtilities.UnexpectedValue((slot, index));
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNext(int previousSlot, int previousIndex)
	{
		switch (previousSlot)
		{
		case -1:
			if (base.Instance != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (!Arguments.IsEmpty)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 2;
		case 1:
			if (previousIndex + 1 < Arguments.Length)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: previousIndex + 1);
			}
			goto case 2;
		case 2:
			return (hasNext: false, nextSlot: 2, nextIndex: 0);
		default:
			throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
		}
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNextReversed(int previousSlot, int previousIndex)
	{
		if ((uint)(previousSlot - -1) > 1u)
		{
			if (previousSlot != 1)
			{
				if (previousSlot != int.MaxValue)
				{
					throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
				}
				if (!Arguments.IsEmpty)
				{
					return (hasNext: true, nextSlot: 1, nextIndex: Arguments.Length - 1);
				}
			}
			else if (previousIndex > 0)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: previousIndex - 1);
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
		visitor.VisitPropertyReference(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitPropertyReference(this, argument);
	}
}
