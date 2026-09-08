using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class VariableDeclarationOperation : Operation, IVariableDeclarationOperation, IOperation
{
	public ImmutableArray<IVariableDeclaratorOperation> Declarators { get; }

	public IVariableInitializerOperation? Initializer { get; }

	public ImmutableArray<IOperation> IgnoredDimensions { get; }

	internal override int ChildOperationsCount => Declarators.Length + ((Initializer != null) ? 1 : 0) + IgnoredDimensions.Length;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.VariableDeclaration;

	internal VariableDeclarationOperation(ImmutableArray<IVariableDeclaratorOperation> declarators, IVariableInitializerOperation? initializer, ImmutableArray<IOperation> ignoredDimensions, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Declarators = Operation.SetParentOperation(declarators, this);
		Initializer = Operation.SetParentOperation(initializer, this);
		IgnoredDimensions = Operation.SetParentOperation(ignoredDimensions, this);
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (index < IgnoredDimensions.Length)
			{
				return IgnoredDimensions[index];
			}
			break;
		case 1:
			if (index < Declarators.Length)
			{
				return Declarators[index];
			}
			break;
		case 2:
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
			if (!IgnoredDimensions.IsEmpty)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto IL_005a;
		case 0:
			if (previousIndex + 1 < IgnoredDimensions.Length)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: previousIndex + 1);
			}
			goto IL_005a;
		case 1:
			if (previousIndex + 1 < Declarators.Length)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: previousIndex + 1);
			}
			goto IL_0091;
		case 2:
		case 3:
			return (hasNext: false, nextSlot: 3, nextIndex: 0);
		default:
			{
				throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
			}
			IL_005a:
			if (!Declarators.IsEmpty)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto IL_0091;
			IL_0091:
			if (Initializer != null)
			{
				return (hasNext: true, nextSlot: 2, nextIndex: 0);
			}
			goto case 2;
		}
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNextReversed(int previousSlot, int previousIndex)
	{
		switch (previousSlot)
		{
		case int.MaxValue:
			if (Initializer != null)
			{
				return (hasNext: true, nextSlot: 2, nextIndex: 0);
			}
			goto case 2;
		case 2:
			if (!Declarators.IsEmpty)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: Declarators.Length - 1);
			}
			goto IL_006d;
		case 1:
			if (previousIndex > 0)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: previousIndex - 1);
			}
			goto IL_006d;
		case 0:
			if (previousIndex > 0)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: previousIndex - 1);
			}
			goto case -1;
		case -1:
			return (hasNext: false, nextSlot: -1, nextIndex: 0);
		default:
			{
				throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
			}
			IL_006d:
			if (!IgnoredDimensions.IsEmpty)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: IgnoredDimensions.Length - 1);
			}
			goto case -1;
		}
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitVariableDeclaration(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitVariableDeclaration(this, argument);
	}
}
