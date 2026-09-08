using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class FunctionPointerInvocationOperation : Operation, IFunctionPointerInvocationOperation, IOperation
{
	public IOperation Target { get; }

	public ImmutableArray<IArgumentOperation> Arguments { get; }

	internal override int ChildOperationsCount => ((Target != null) ? 1 : 0) + Arguments.Length;

	public override ITypeSymbol? Type { get; }

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.FunctionPointerInvocation;

	internal FunctionPointerInvocationOperation(IOperation target, ImmutableArray<IArgumentOperation> arguments, SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Target = Operation.SetParentOperation(target, this);
		Arguments = Operation.SetParentOperation(arguments, this);
		Type = type;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (Target != null)
			{
				return Target;
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
			if (Target != null)
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
			if (Target != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitFunctionPointerInvocation(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitFunctionPointerInvocation(this, argument);
	}
}
