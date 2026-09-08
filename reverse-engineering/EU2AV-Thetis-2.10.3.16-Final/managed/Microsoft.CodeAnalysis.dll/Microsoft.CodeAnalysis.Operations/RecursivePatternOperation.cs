using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class RecursivePatternOperation : BasePatternOperation, IRecursivePatternOperation, IPatternOperation, IOperation
{
	public ITypeSymbol MatchedType { get; }

	public ISymbol? DeconstructSymbol { get; }

	public ImmutableArray<IPatternOperation> DeconstructionSubpatterns { get; }

	public ImmutableArray<IPropertySubpatternOperation> PropertySubpatterns { get; }

	public ISymbol? DeclaredSymbol { get; }

	internal override int ChildOperationsCount => DeconstructionSubpatterns.Length + PropertySubpatterns.Length;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.RecursivePattern;

	internal RecursivePatternOperation(ITypeSymbol matchedType, ISymbol? deconstructSymbol, ImmutableArray<IPatternOperation> deconstructionSubpatterns, ImmutableArray<IPropertySubpatternOperation> propertySubpatterns, ISymbol? declaredSymbol, ITypeSymbol inputType, ITypeSymbol narrowedType, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(inputType, narrowedType, semanticModel, syntax, isImplicit)
	{
		MatchedType = matchedType;
		DeconstructSymbol = deconstructSymbol;
		DeconstructionSubpatterns = Operation.SetParentOperation(deconstructionSubpatterns, this);
		PropertySubpatterns = Operation.SetParentOperation(propertySubpatterns, this);
		DeclaredSymbol = declaredSymbol;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (index < DeconstructionSubpatterns.Length)
			{
				return DeconstructionSubpatterns[index];
			}
			break;
		case 1:
			if (index < PropertySubpatterns.Length)
			{
				return PropertySubpatterns[index];
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
			if (!DeconstructionSubpatterns.IsEmpty)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto IL_0053;
		case 0:
			if (previousIndex + 1 < DeconstructionSubpatterns.Length)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: previousIndex + 1);
			}
			goto IL_0053;
		case 1:
			if (previousIndex + 1 < PropertySubpatterns.Length)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: previousIndex + 1);
			}
			goto case 2;
		case 2:
			return (hasNext: false, nextSlot: 2, nextIndex: 0);
		default:
			{
				throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
			}
			IL_0053:
			if (!PropertySubpatterns.IsEmpty)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 2;
		}
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNextReversed(int previousSlot, int previousIndex)
	{
		switch (previousSlot)
		{
		case int.MaxValue:
			if (!PropertySubpatterns.IsEmpty)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: PropertySubpatterns.Length - 1);
			}
			goto IL_0055;
		case 1:
			if (previousIndex > 0)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: previousIndex - 1);
			}
			goto IL_0055;
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
			IL_0055:
			if (!DeconstructionSubpatterns.IsEmpty)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: DeconstructionSubpatterns.Length - 1);
			}
			goto case -1;
		}
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitRecursivePattern(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitRecursivePattern(this, argument);
	}
}
