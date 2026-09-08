using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class DynamicObjectCreationOperation : HasDynamicArgumentsExpression, IDynamicObjectCreationOperation, IOperation
{
	public IObjectOrCollectionInitializerOperation? Initializer { get; }

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.DynamicObjectCreation;

	internal override int ChildOperationsCount => ((Initializer != null) ? 1 : 0) + base.Arguments.Length;

	public DynamicObjectCreationOperation(IObjectOrCollectionInitializerOperation? initializer, ImmutableArray<IOperation> arguments, ImmutableArray<string?> argumentNames, ImmutableArray<RefKind> argumentRefKinds, SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, bool isImplicit)
		: base(arguments, argumentNames, argumentRefKinds, semanticModel, syntax, type, isImplicit)
	{
		Initializer = Operation.SetParentOperation(initializer, this);
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (index < base.Arguments.Length)
			{
				return base.Arguments[index];
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
			if (!base.Arguments.IsEmpty)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto IL_0053;
		case 0:
			if (previousIndex + 1 < base.Arguments.Length)
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
			if (!base.Arguments.IsEmpty)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: base.Arguments.Length - 1);
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
		visitor.VisitDynamicObjectCreation(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitDynamicObjectCreation(this, argument);
	}
}
