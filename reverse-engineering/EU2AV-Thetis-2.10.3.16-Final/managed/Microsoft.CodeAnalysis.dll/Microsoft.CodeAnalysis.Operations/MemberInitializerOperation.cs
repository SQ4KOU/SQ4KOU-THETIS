namespace Microsoft.CodeAnalysis.Operations;

internal sealed class MemberInitializerOperation : Operation, IMemberInitializerOperation, IOperation
{
	public IOperation InitializedMember { get; }

	public IObjectOrCollectionInitializerOperation Initializer { get; }

	internal override int ChildOperationsCount => ((InitializedMember != null) ? 1 : 0) + ((Initializer != null) ? 1 : 0);

	public override ITypeSymbol? Type { get; }

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.MemberInitializer;

	internal MemberInitializerOperation(IOperation initializedMember, IObjectOrCollectionInitializerOperation initializer, SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		InitializedMember = Operation.SetParentOperation(initializedMember, this);
		Initializer = Operation.SetParentOperation(initializer, this);
		Type = type;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (InitializedMember != null)
			{
				return InitializedMember;
			}
			break;
		case 1:
			if (Initializer != null)
			{
				return Initializer;
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
			if (InitializedMember != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (Initializer != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		case 1:
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
				if (Initializer != null)
				{
					return (hasNext: true, nextSlot: 1, nextIndex: 0);
				}
			}
			if (InitializedMember != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitMemberInitializer(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitMemberInitializer(this, argument);
	}
}
