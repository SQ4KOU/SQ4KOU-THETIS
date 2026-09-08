using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis.CSharp;

internal class Instrumenter
{
	public static readonly Instrumenter NoOp = new Instrumenter();

	private static BoundStatement InstrumentStatement(BoundStatement original, BoundStatement rewritten)
	{
		return rewritten;
	}

	public virtual BoundStatement InstrumentNoOpStatement(BoundNoOpStatement original, BoundStatement rewritten)
	{
		return InstrumentStatement(original, rewritten);
	}

	public virtual BoundStatement InstrumentYieldBreakStatement(BoundYieldBreakStatement original, BoundStatement rewritten)
	{
		return rewritten;
	}

	public virtual BoundStatement InstrumentYieldReturnStatement(BoundYieldReturnStatement original, BoundStatement rewritten)
	{
		return InstrumentStatement(original, rewritten);
	}

	public virtual void PreInstrumentBlock(BoundBlock original, LocalRewriter rewriter)
	{
	}

	public virtual void InstrumentBlock(BoundBlock original, LocalRewriter rewriter, ref TemporaryArray<LocalSymbol> additionalLocals, out BoundStatement? prologue, out BoundStatement? epilogue, out BoundBlockInstrumentation? instrumentation)
	{
		prologue = null;
		epilogue = null;
		instrumentation = null;
	}

	public virtual BoundStatement InstrumentThrowStatement(BoundThrowStatement original, BoundStatement rewritten)
	{
		return InstrumentStatement(original, rewritten);
	}

	public virtual BoundStatement InstrumentContinueStatement(BoundContinueStatement original, BoundStatement rewritten)
	{
		return InstrumentStatement(original, rewritten);
	}

	public virtual BoundStatement InstrumentGotoStatement(BoundGotoStatement original, BoundStatement rewritten)
	{
		return InstrumentStatement(original, rewritten);
	}

	public virtual BoundStatement InstrumentExpressionStatement(BoundExpressionStatement original, BoundStatement rewritten)
	{
		return InstrumentStatement(original, rewritten);
	}

	public virtual BoundStatement InstrumentFieldOrPropertyInitializer(BoundStatement original, BoundStatement rewritten)
	{
		return InstrumentStatement(original, rewritten);
	}

	public virtual BoundStatement InstrumentBreakStatement(BoundBreakStatement original, BoundStatement rewritten)
	{
		return InstrumentStatement(original, rewritten);
	}

	public virtual BoundExpression InstrumentDoStatementCondition(BoundDoStatement original, BoundExpression rewrittenCondition, SyntheticBoundNodeFactory factory)
	{
		return rewrittenCondition;
	}

	public virtual BoundExpression InstrumentWhileStatementCondition(BoundWhileStatement original, BoundExpression rewrittenCondition, SyntheticBoundNodeFactory factory)
	{
		return rewrittenCondition;
	}

	public virtual BoundStatement InstrumentDoStatementConditionalGotoStart(BoundDoStatement original, BoundStatement ifConditionGotoStart)
	{
		return ifConditionGotoStart;
	}

	public virtual BoundStatement InstrumentWhileStatementConditionalGotoStartOrBreak(BoundWhileStatement original, BoundStatement ifConditionGotoStart)
	{
		return ifConditionGotoStart;
	}

	[return: NotNullIfNotNull("collectionVarDecl")]
	public virtual BoundStatement? InstrumentForEachStatementCollectionVarDeclaration(BoundForEachStatement original, BoundStatement? collectionVarDecl)
	{
		return collectionVarDecl;
	}

	public virtual BoundStatement InstrumentForEachStatement(BoundForEachStatement original, BoundStatement rewritten)
	{
		return InstrumentStatement(original, rewritten);
	}

	public virtual BoundStatement InstrumentForEachStatementIterationVarDeclaration(BoundForEachStatement original, BoundStatement iterationVarDecl)
	{
		return iterationVarDecl;
	}

	public virtual BoundStatement InstrumentForEachStatementDeconstructionVariablesDeclaration(BoundForEachStatement original, BoundStatement iterationVarDecl)
	{
		return iterationVarDecl;
	}

	public virtual BoundStatement InstrumentForEachStatementConditionalGotoStart(BoundForEachStatement original, BoundStatement branchBack)
	{
		return branchBack;
	}

	public virtual BoundStatement InstrumentForStatementConditionalGotoStartOrBreak(BoundForStatement original, BoundStatement branchBack)
	{
		return branchBack;
	}

	public virtual BoundExpression InstrumentForStatementCondition(BoundForStatement original, BoundExpression rewrittenCondition, SyntheticBoundNodeFactory factory)
	{
		return rewrittenCondition;
	}

	public virtual BoundStatement InstrumentIfStatementConditionalGoto(BoundIfStatement original, BoundStatement rewritten)
	{
		return InstrumentStatement(original, rewritten);
	}

	public virtual BoundExpression InstrumentIfStatementCondition(BoundIfStatement original, BoundExpression rewrittenCondition, SyntheticBoundNodeFactory factory)
	{
		return rewrittenCondition;
	}

	public virtual BoundStatement InstrumentLabelStatement(BoundLabeledStatement original, BoundStatement rewritten)
	{
		return InstrumentStatement(original, rewritten);
	}

	public virtual BoundStatement InstrumentUserDefinedLocalInitialization(BoundLocalDeclaration original, BoundStatement rewritten)
	{
		return InstrumentStatement(original, rewritten);
	}

	public virtual BoundExpression InstrumentUserDefinedLocalAssignment(BoundAssignmentOperator original)
	{
		return original;
	}

	public virtual BoundExpression InstrumentCall(BoundCall original, BoundExpression rewritten)
	{
		return rewritten;
	}

	public virtual void InterceptCallAndAdjustArguments(ref MethodSymbol method, ref BoundExpression? receiver, ref ImmutableArray<BoundExpression> arguments, ref ImmutableArray<RefKind> argumentRefKindsOpt)
	{
	}

	public virtual BoundExpression InstrumentObjectCreationExpression(BoundObjectCreationExpression original, BoundExpression rewritten)
	{
		return rewritten;
	}

	public virtual BoundExpression InstrumentFunctionPointerInvocation(BoundFunctionPointerInvocation original, BoundExpression rewritten)
	{
		return rewritten;
	}

	public virtual BoundStatement InstrumentLockTargetCapture(BoundLockStatement original, BoundStatement lockTargetCapture)
	{
		return lockTargetCapture;
	}

	public virtual BoundStatement InstrumentReturnStatement(BoundReturnStatement original, BoundStatement rewritten)
	{
		return rewritten;
	}

	public virtual BoundStatement InstrumentSwitchStatement(BoundSwitchStatement original, BoundStatement rewritten)
	{
		return InstrumentStatement(original, rewritten);
	}

	public virtual BoundStatement InstrumentSwitchWhenClauseConditionalGotoBody(BoundExpression original, BoundStatement ifConditionGotoBody)
	{
		return ifConditionGotoBody;
	}

	public virtual BoundStatement InstrumentUsingTargetCapture(BoundUsingStatement original, BoundStatement usingTargetCapture)
	{
		return usingTargetCapture;
	}

	public virtual void InstrumentCatchBlock(BoundCatchBlock original, ref BoundExpression? rewrittenSource, ref BoundStatementList? rewrittenFilterPrologue, ref BoundExpression? rewrittenFilter, ref BoundBlock rewrittenBody, ref TypeSymbol? rewrittenType, SyntheticBoundNodeFactory factory)
	{
	}

	public virtual BoundExpression InstrumentSwitchStatementExpression(BoundStatement original, BoundExpression rewrittenExpression, SyntheticBoundNodeFactory factory)
	{
		return rewrittenExpression;
	}

	public virtual BoundExpression InstrumentSwitchExpressionArmExpression(BoundExpression original, BoundExpression rewrittenExpression, SyntheticBoundNodeFactory factory)
	{
		return rewrittenExpression;
	}

	public virtual BoundStatement InstrumentSwitchBindCasePatternVariables(BoundStatement bindings)
	{
		return bindings;
	}
}
