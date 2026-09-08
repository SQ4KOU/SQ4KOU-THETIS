using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundForEachStatement : BoundLoopStatement
{
	public ForEachEnumeratorInfo? EnumeratorInfoOpt { get; }

	public BoundValuePlaceholder? ElementPlaceholder { get; }

	public BoundExpression? ElementConversion { get; }

	public BoundTypeExpression IterationVariableType { get; }

	public ImmutableArray<LocalSymbol> IterationVariables { get; }

	public BoundExpression? IterationErrorExpressionOpt { get; }

	public BoundExpression Expression { get; }

	public BoundForEachDeconstructStep? DeconstructionOpt { get; }

	public BoundStatement Body { get; }

	public BoundForEachStatement(SyntaxNode syntax, ForEachEnumeratorInfo? enumeratorInfoOpt, BoundValuePlaceholder? elementPlaceholder, BoundExpression? elementConversion, BoundTypeExpression iterationVariableType, ImmutableArray<LocalSymbol> iterationVariables, BoundExpression? iterationErrorExpressionOpt, BoundExpression expression, BoundForEachDeconstructStep? deconstructionOpt, BoundStatement body, LabelSymbol breakLabel, LabelSymbol continueLabel, bool hasErrors = false)
		: base(BoundKind.ForEachStatement, syntax, breakLabel, continueLabel, hasErrors || elementPlaceholder.HasErrors() || elementConversion.HasErrors() || iterationVariableType.HasErrors() || iterationErrorExpressionOpt.HasErrors() || expression.HasErrors() || deconstructionOpt.HasErrors() || body.HasErrors())
	{
		EnumeratorInfoOpt = enumeratorInfoOpt;
		ElementPlaceholder = elementPlaceholder;
		ElementConversion = elementConversion;
		IterationVariableType = iterationVariableType;
		IterationVariables = iterationVariables;
		IterationErrorExpressionOpt = iterationErrorExpressionOpt;
		Expression = expression;
		DeconstructionOpt = deconstructionOpt;
		Body = body;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitForEachStatement(this);
	}

	public BoundForEachStatement Update(ForEachEnumeratorInfo? enumeratorInfoOpt, BoundValuePlaceholder? elementPlaceholder, BoundExpression? elementConversion, BoundTypeExpression iterationVariableType, ImmutableArray<LocalSymbol> iterationVariables, BoundExpression? iterationErrorExpressionOpt, BoundExpression expression, BoundForEachDeconstructStep? deconstructionOpt, BoundStatement body, LabelSymbol breakLabel, LabelSymbol continueLabel)
	{
		if (enumeratorInfoOpt != EnumeratorInfoOpt || elementPlaceholder != ElementPlaceholder || elementConversion != ElementConversion || iterationVariableType != IterationVariableType || iterationVariables != IterationVariables || iterationErrorExpressionOpt != IterationErrorExpressionOpt || expression != Expression || deconstructionOpt != DeconstructionOpt || body != Body || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(breakLabel, base.BreakLabel) || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(continueLabel, base.ContinueLabel))
		{
			BoundForEachStatement boundForEachStatement = new BoundForEachStatement(Syntax, enumeratorInfoOpt, elementPlaceholder, elementConversion, iterationVariableType, iterationVariables, iterationErrorExpressionOpt, expression, deconstructionOpt, body, breakLabel, continueLabel, base.HasErrors);
			boundForEachStatement.CopyAttributes(this);
			return boundForEachStatement;
		}
		return this;
	}
}
