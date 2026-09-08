namespace Microsoft.CodeAnalysis.Operations;

internal sealed class InterpolationOperation : BaseInterpolatedStringContentOperation, IInterpolationOperation, IInterpolatedStringContentOperation, IOperation
{
	public IOperation Expression { get; }

	public IOperation? Alignment { get; }

	public IOperation? FormatString { get; }

	internal override int ChildOperationsCount => ((Expression != null) ? 1 : 0) + ((Alignment != null) ? 1 : 0) + ((FormatString != null) ? 1 : 0);

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.Interpolation;

	internal InterpolationOperation(IOperation expression, IOperation? alignment, IOperation? formatString, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Expression = Operation.SetParentOperation(expression, this);
		Alignment = Operation.SetParentOperation(alignment, this);
		FormatString = Operation.SetParentOperation(formatString, this);
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (Expression != null)
			{
				return Expression;
			}
			break;
		case 1:
			if (Alignment != null)
			{
				return Alignment;
			}
			break;
		case 2:
			if (FormatString != null)
			{
				return FormatString;
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
			if (Expression != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (Alignment != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		case 1:
			if (FormatString != null)
			{
				return (hasNext: true, nextSlot: 2, nextIndex: 0);
			}
			goto case 2;
		case 2:
		case 3:
			return (hasNext: false, nextSlot: 3, nextIndex: 0);
		default:
			throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
		}
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNextReversed(int previousSlot, int previousIndex)
	{
		switch (previousSlot)
		{
		case int.MaxValue:
			if (FormatString != null)
			{
				return (hasNext: true, nextSlot: 2, nextIndex: 0);
			}
			goto case 2;
		case 2:
			if (Alignment != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		case 1:
			if (Expression != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case -1;
		case -1:
		case 0:
			return (hasNext: false, nextSlot: -1, nextIndex: 0);
		default:
			throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
		}
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitInterpolation(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitInterpolation(this, argument);
	}
}
