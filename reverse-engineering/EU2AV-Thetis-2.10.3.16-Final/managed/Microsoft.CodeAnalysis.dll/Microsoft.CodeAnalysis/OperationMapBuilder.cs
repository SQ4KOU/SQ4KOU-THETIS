using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeAnalysis;

internal static class OperationMapBuilder
{
	private sealed class Walker : OperationWalker<Dictionary<SyntaxNode, IOperation>>
	{
		internal static readonly Walker Instance = new Walker();

		public override object? DefaultVisit(IOperation operation, Dictionary<SyntaxNode, IOperation> argument)
		{
			RecordOperation(operation, argument);
			return base.DefaultVisit(operation, argument);
		}

		public override object? VisitBinaryOperator([DisallowNull] IBinaryOperation? operation, Dictionary<SyntaxNode, IOperation> argument)
		{
			while (true)
			{
				RecordOperation(operation, argument);
				Visit(operation.RightOperand, argument);
				if (!(operation.LeftOperand is IBinaryOperation binaryOperation))
				{
					break;
				}
				operation = binaryOperation;
			}
			Visit(operation.LeftOperand, argument);
			return null;
		}

		public override object? VisitConditional(IConditionalOperation operation, Dictionary<SyntaxNode, IOperation> argument)
		{
			while (true)
			{
				RecordOperation(operation, argument);
				Visit(operation.Condition, argument);
				Visit(operation.WhenTrue, argument);
				if (!(operation.WhenFalse is IConditionalOperation conditionalOperation))
				{
					break;
				}
				operation = conditionalOperation;
			}
			Visit(operation.WhenFalse, argument);
			return null;
		}

		public override object? VisitBinaryPattern(IBinaryPatternOperation operation, Dictionary<SyntaxNode, IOperation> argument)
		{
			while (true)
			{
				RecordOperation(operation, argument);
				Visit(operation.RightPattern, argument);
				if (!(operation.LeftPattern is IBinaryPatternOperation binaryPatternOperation))
				{
					break;
				}
				operation = binaryPatternOperation;
			}
			Visit(operation.LeftPattern, argument);
			return null;
		}

		internal override object? VisitNoneOperation(IOperation operation, Dictionary<SyntaxNode, IOperation> argument)
		{
			return DefaultVisit(operation, argument);
		}

		private static void RecordOperation(IOperation operation, Dictionary<SyntaxNode, IOperation> argument)
		{
			if (!operation.IsImplicit)
			{
				argument.Add(operation.Syntax, operation);
			}
		}
	}

	internal static void AddToMap(IOperation root, Dictionary<SyntaxNode, IOperation> dictionary)
	{
		Walker.Instance.Visit(root, dictionary);
	}
}
