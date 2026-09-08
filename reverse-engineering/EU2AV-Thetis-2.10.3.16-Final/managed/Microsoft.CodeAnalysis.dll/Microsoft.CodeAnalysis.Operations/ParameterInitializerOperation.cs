using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ParameterInitializerOperation : BaseSymbolInitializerOperation, IParameterInitializerOperation, ISymbolInitializerOperation, IOperation
{
	public IParameterSymbol Parameter { get; }

	internal override int ChildOperationsCount => (base.Value != null) ? 1 : 0;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.ParameterInitializer;

	internal ParameterInitializerOperation(IParameterSymbol parameter, ImmutableArray<ILocalSymbol> locals, IOperation value, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(locals, value, semanticModel, syntax, isImplicit)
	{
		Parameter = parameter;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		if (slot == 0 && base.Value != null)
		{
			return base.Value;
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
		else if (base.Value != null)
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
			if (base.Value != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitParameterInitializer(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitParameterInitializer(this, argument);
	}
}
