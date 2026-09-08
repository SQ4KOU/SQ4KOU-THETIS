namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LocalFunctionOperation : Operation, ILocalFunctionOperation, IOperation
{
	public IMethodSymbol Symbol { get; }

	public IBlockOperation? Body { get; }

	public IBlockOperation? IgnoredBody { get; }

	internal override int ChildOperationsCount => ((Body != null) ? 1 : 0) + ((IgnoredBody != null) ? 1 : 0);

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.LocalFunction;

	internal LocalFunctionOperation(IMethodSymbol symbol, IBlockOperation? body, IBlockOperation? ignoredBody, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Symbol = symbol;
		Body = Operation.SetParentOperation(body, this);
		IgnoredBody = Operation.SetParentOperation(ignoredBody, this);
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (Body != null)
			{
				return Body;
			}
			break;
		case 1:
			if (IgnoredBody != null)
			{
				return IgnoredBody;
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
			if (Body != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (IgnoredBody != null)
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
				if (IgnoredBody != null)
				{
					return (hasNext: true, nextSlot: 1, nextIndex: 0);
				}
			}
			if (Body != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitLocalFunction(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitLocalFunction(this, argument);
	}
}
