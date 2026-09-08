namespace Microsoft.CodeAnalysis.Operations;

internal sealed class UsingDeclarationOperation : Operation, IUsingDeclarationOperation, IOperation
{
	public IVariableDeclarationGroupOperation DeclarationGroup { get; }

	public bool IsAsynchronous { get; }

	public DisposeOperationInfo DisposeInfo { get; }

	internal override int ChildOperationsCount => (DeclarationGroup != null) ? 1 : 0;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.UsingDeclaration;

	internal UsingDeclarationOperation(IVariableDeclarationGroupOperation declarationGroup, bool isAsynchronous, DisposeOperationInfo disposeInfo, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		DeclarationGroup = Operation.SetParentOperation(declarationGroup, this);
		IsAsynchronous = isAsynchronous;
		DisposeInfo = disposeInfo;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		if (slot == 0 && DeclarationGroup != null)
		{
			return DeclarationGroup;
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
		else if (DeclarationGroup != null)
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
			if (DeclarationGroup != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitUsingDeclaration(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitUsingDeclaration(this, argument);
	}
}
