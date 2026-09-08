using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class DynamicMemberReferenceOperation : Operation, IDynamicMemberReferenceOperation, IOperation
{
	public IOperation? Instance { get; }

	public string MemberName { get; }

	public ImmutableArray<ITypeSymbol> TypeArguments { get; }

	public ITypeSymbol? ContainingType { get; }

	internal override int ChildOperationsCount => (Instance != null) ? 1 : 0;

	public override ITypeSymbol? Type { get; }

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.DynamicMemberReference;

	internal DynamicMemberReferenceOperation(IOperation? instance, string memberName, ImmutableArray<ITypeSymbol> typeArguments, ITypeSymbol? containingType, SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Instance = Operation.SetParentOperation(instance, this);
		MemberName = memberName;
		TypeArguments = typeArguments;
		ContainingType = containingType;
		Type = type;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		if (slot == 0 && Instance != null)
		{
			return Instance;
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
		else if (Instance != null)
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
			if (Instance != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitDynamicMemberReference(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitDynamicMemberReference(this, argument);
	}
}
