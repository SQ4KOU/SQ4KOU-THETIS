using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ObjectCreationOperation : Operation, IObjectCreationOperation, IOperation
{
	public IMethodSymbol? Constructor { get; }

	public IObjectOrCollectionInitializerOperation? Initializer { get; }

	public ImmutableArray<IArgumentOperation> Arguments { get; }

	internal override int ChildOperationsCount => ((Initializer != null) ? 1 : 0) + Arguments.Length;

	public override ITypeSymbol? Type { get; }

	internal override ConstantValue? OperationConstantValue { get; }

	public override OperationKind Kind => OperationKind.ObjectCreation;

	internal ObjectCreationOperation(IMethodSymbol? constructor, IObjectOrCollectionInitializerOperation? initializer, ImmutableArray<IArgumentOperation> arguments, SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, ConstantValue? constantValue, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Constructor = constructor;
		Initializer = Operation.SetParentOperation(initializer, this);
		Arguments = Operation.SetParentOperation(arguments, this);
		OperationConstantValue = constantValue;
		Type = type;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (index < Arguments.Length)
			{
				return Arguments[index];
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
			if (!Arguments.IsEmpty)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto IL_0053;
		case 0:
			if (previousIndex + 1 < Arguments.Length)
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
			if (!Arguments.IsEmpty)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: Arguments.Length - 1);
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
		visitor.VisitObjectCreation(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitObjectCreation(this, argument);
	}
}
