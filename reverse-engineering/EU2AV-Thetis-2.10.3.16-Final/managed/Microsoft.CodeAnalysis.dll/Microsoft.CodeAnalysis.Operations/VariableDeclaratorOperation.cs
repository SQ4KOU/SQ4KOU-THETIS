using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class VariableDeclaratorOperation : Operation, IVariableDeclaratorOperation, IOperation
{
	public ILocalSymbol Symbol { get; }

	public IVariableInitializerOperation? Initializer { get; }

	public ImmutableArray<IOperation> IgnoredArguments { get; }

	internal override int ChildOperationsCount => ((Initializer != null) ? 1 : 0) + IgnoredArguments.Length;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.VariableDeclarator;

	internal VariableDeclaratorOperation(ILocalSymbol symbol, IVariableInitializerOperation? initializer, ImmutableArray<IOperation> ignoredArguments, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Symbol = symbol;
		Initializer = Operation.SetParentOperation(initializer, this);
		IgnoredArguments = Operation.SetParentOperation(ignoredArguments, this);
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (index < IgnoredArguments.Length)
			{
				return IgnoredArguments[index];
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
			if (!IgnoredArguments.IsEmpty)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto IL_0053;
		case 0:
			if (previousIndex + 1 < IgnoredArguments.Length)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: previousIndex + 1);
			}
			goto IL_0053;
		case 1:
		case 2:
			return (hasNext: false, nextSlot: 2, nextIndex: 0);
		default:
			{
				throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
			}
			IL_0053:
			if (Initializer != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		}
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNextReversed(int previousSlot, int previousIndex)
	{
		switch (previousSlot)
		{
		case int.MaxValue:
			if (Initializer != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		case 1:
			if (!IgnoredArguments.IsEmpty)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: IgnoredArguments.Length - 1);
			}
			goto case -1;
		case 0:
			if (previousIndex > 0)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: previousIndex - 1);
			}
			goto case -1;
		case -1:
			return (hasNext: false, nextSlot: -1, nextIndex: 0);
		default:
			throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
		}
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitVariableDeclarator(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitVariableDeclarator(this, argument);
	}
}
