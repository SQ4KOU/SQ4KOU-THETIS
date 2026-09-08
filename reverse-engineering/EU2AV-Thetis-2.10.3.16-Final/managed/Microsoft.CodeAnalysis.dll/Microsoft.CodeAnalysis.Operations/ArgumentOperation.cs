namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ArgumentOperation : Operation, IArgumentOperation, IOperation
{
	public ArgumentKind ArgumentKind { get; }

	public IParameterSymbol? Parameter { get; }

	public IOperation Value { get; }

	internal IConvertibleConversion InConversionConvertible { get; }

	public CommonConversion InConversion => InConversionConvertible.ToCommonConversion();

	internal IConvertibleConversion OutConversionConvertible { get; }

	public CommonConversion OutConversion => OutConversionConvertible.ToCommonConversion();

	internal override int ChildOperationsCount => (Value != null) ? 1 : 0;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.Argument;

	internal ArgumentOperation(ArgumentKind argumentKind, IParameterSymbol? parameter, IOperation value, IConvertibleConversion inConversion, IConvertibleConversion outConversion, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		ArgumentKind = argumentKind;
		Parameter = parameter;
		Value = Operation.SetParentOperation(value, this);
		InConversionConvertible = inConversion;
		OutConversionConvertible = outConversion;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		if (slot == 0 && Value != null)
		{
			return Value;
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
		else if (Value != null)
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
			if (Value != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitArgument(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitArgument(this, argument);
	}
}
