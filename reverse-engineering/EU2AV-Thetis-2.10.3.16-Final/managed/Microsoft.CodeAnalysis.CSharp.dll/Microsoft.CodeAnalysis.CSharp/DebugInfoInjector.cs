using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp;

internal class DebugInfoInjector : CompoundInstrumenter
{
	private static readonly DebugInfoInjector s_singleton = new DebugInfoInjector(Instrumenter.NoOp);

	private DebugInfoInjector(Instrumenter previous)
		: base(previous)
	{
	}

	public static DebugInfoInjector Create(Instrumenter previous)
	{
		if (previous != Instrumenter.NoOp)
		{
			return new DebugInfoInjector(previous);
		}
		return s_singleton;
	}

	protected override CompoundInstrumenter WithPreviousImpl(Instrumenter previous)
	{
		return Create(previous);
	}

	public override BoundStatement InstrumentNoOpStatement(BoundNoOpStatement original, BoundStatement rewritten)
	{
		return AddSequencePoint(base.InstrumentNoOpStatement(original, rewritten));
	}

	public override BoundStatement InstrumentBreakStatement(BoundBreakStatement original, BoundStatement rewritten)
	{
		return AddSequencePoint(base.InstrumentBreakStatement(original, rewritten));
	}

	public override BoundStatement InstrumentContinueStatement(BoundContinueStatement original, BoundStatement rewritten)
	{
		return AddSequencePoint(base.InstrumentContinueStatement(original, rewritten));
	}

	public override BoundStatement InstrumentExpressionStatement(BoundExpressionStatement original, BoundStatement rewritten)
	{
		rewritten = base.InstrumentExpressionStatement(original, rewritten);
		if (original.IsConstructorInitializer())
		{
			SyntaxNode syntax = original.Syntax;
			if (!(syntax is ConstructorDeclarationSyntax constructorDeclarationSyntax))
			{
				if (!(syntax is ConstructorInitializerSyntax constructorInitializerSyntax))
				{
					if (!(syntax is TypeDeclarationSyntax typeDeclarationSyntax))
					{
						if (syntax is PrimaryConstructorBaseTypeSyntax primaryConstructorBaseTypeSyntax)
						{
							return new BoundSequencePointWithSpan(primaryConstructorBaseTypeSyntax, rewritten, primaryConstructorBaseTypeSyntax.Span);
						}
						throw ExceptionUtilities.UnexpectedValue(original.Syntax.Kind());
					}
					return new BoundSequencePointWithSpan(typeDeclarationSyntax, rewritten, TextSpan.FromBounds(typeDeclarationSyntax.Identifier.SpanStart, typeDeclarationSyntax.ParameterList.Span.End));
				}
				return new BoundSequencePointWithSpan(constructorInitializerSyntax, rewritten, TextSpan.FromBounds(constructorInitializerSyntax.ThisOrBaseKeyword.SpanStart, constructorInitializerSyntax.ArgumentList.CloseParenToken.Span.End));
			}
			TextSpan span;
			if (constructorDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword))
			{
				int spanStart = constructorDeclarationSyntax.Body.OpenBraceToken.SpanStart;
				int end = constructorDeclarationSyntax.Body.OpenBraceToken.Span.End;
				span = TextSpan.FromBounds(spanStart, end);
			}
			else
			{
				span = CreateSpan(constructorDeclarationSyntax.Modifiers, constructorDeclarationSyntax.Identifier, constructorDeclarationSyntax.ParameterList.CloseParenToken);
			}
			return new BoundSequencePointWithSpan(constructorDeclarationSyntax, rewritten, span);
		}
		if (original.Syntax is ParameterSyntax parameterSyntax)
		{
			return new BoundSequencePointWithSpan(parameterSyntax, rewritten, CreateSpan(parameterSyntax));
		}
		return AddSequencePoint(rewritten);
	}

	public override BoundStatement InstrumentFieldOrPropertyInitializer(BoundStatement original, BoundStatement rewritten)
	{
		rewritten = base.InstrumentFieldOrPropertyInitializer(original, rewritten);
		SyntaxNode syntax = original.Syntax;
		if (rewritten.Kind == BoundKind.Block)
		{
			BoundBlock boundBlock = (BoundBlock)rewritten;
			return boundBlock.Update(boundBlock.Locals, boundBlock.LocalFunctions, boundBlock.HasUnsafeModifier, boundBlock.Instrumentation, ImmutableArray.Create(InstrumentFieldOrPropertyInitializer(boundBlock.Statements.Single(), syntax)));
		}
		return InstrumentFieldOrPropertyInitializer(rewritten, syntax);
	}

	private static BoundStatement InstrumentFieldOrPropertyInitializer(BoundStatement rewritten, SyntaxNode syntax)
	{
		if (syntax.IsKind(SyntaxKind.Parameter))
		{
			return rewritten;
		}
		SyntaxNode parent = syntax.Parent.Parent;
		return parent.Kind() switch
		{
			SyntaxKind.VariableDeclarator => AddSequencePoint((VariableDeclaratorSyntax)parent, rewritten), 
			SyntaxKind.PropertyDeclaration => AddSequencePoint((PropertyDeclarationSyntax)parent, rewritten), 
			_ => throw ExceptionUtilities.UnexpectedValue(parent.Kind()), 
		};
	}

	public override BoundStatement InstrumentGotoStatement(BoundGotoStatement original, BoundStatement rewritten)
	{
		return AddSequencePoint(base.InstrumentGotoStatement(original, rewritten));
	}

	public override BoundStatement InstrumentThrowStatement(BoundThrowStatement original, BoundStatement rewritten)
	{
		return AddSequencePoint(base.InstrumentThrowStatement(original, rewritten));
	}

	public override BoundStatement InstrumentYieldBreakStatement(BoundYieldBreakStatement original, BoundStatement rewritten)
	{
		rewritten = base.InstrumentYieldBreakStatement(original, rewritten);
		if (original.WasCompilerGenerated && original.Syntax.Kind() == SyntaxKind.Block)
		{
			return new BoundSequencePointWithSpan(original.Syntax, rewritten, ((BlockSyntax)original.Syntax).CloseBraceToken.Span);
		}
		return AddSequencePoint(rewritten);
	}

	public override BoundStatement InstrumentYieldReturnStatement(BoundYieldReturnStatement original, BoundStatement rewritten)
	{
		return AddSequencePoint(base.InstrumentYieldReturnStatement(original, rewritten));
	}

	public override void InstrumentBlock(BoundBlock original, LocalRewriter rewriter, ref TemporaryArray<LocalSymbol> additionalLocals, out BoundStatement? prologue, out BoundStatement? epilogue, out BoundBlockInstrumentation? instrumentation)
	{
		base.InstrumentBlock(original, rewriter, ref additionalLocals, out BoundStatement prologue2, out BoundStatement epilogue2, out instrumentation);
		prologue = prologue2;
		epilogue = epilogue2;
		if (original.Syntax is BlockSyntax blockSyntax && !original.WasCompilerGenerated)
		{
			prologue = new BoundSequencePointWithSpan(original.Syntax, prologue2, blockSyntax.OpenBraceToken.Span);
			SyntaxNode parent = original.Syntax.Parent;
			if (parent == null || (!parent.IsAnonymousFunction() && !(parent is BaseMethodDeclarationSyntax)))
			{
				epilogue = new BoundSequencePointWithSpan(original.Syntax, epilogue2, blockSyntax.CloseBraceToken.Span);
			}
			return;
		}
		if (original != rewriter.CurrentMethodBody)
		{
			return;
		}
		if (prologue2 != null || rewriter.Factory.TopLevelMethod is SynthesizedSimpleProgramEntryPointSymbol)
		{
			goto IL_00e1;
		}
		if (original.Syntax is RecordDeclarationSyntax recordDeclarationSyntax)
		{
			ParameterListSyntax parameterList = recordDeclarationSyntax.ParameterList;
			if (parameterList != null && parameterList.Parameters.Count > 0)
			{
				goto IL_00e1;
			}
		}
		goto IL_00eb;
		IL_00e1:
		prologue = BoundSequencePoint.CreateHidden(prologue2);
		goto IL_00eb;
		IL_00eb:
		if (epilogue2 != null)
		{
			epilogue = BoundSequencePoint.CreateHidden(epilogue2);
		}
	}

	public override BoundExpression InstrumentDoStatementCondition(BoundDoStatement original, BoundExpression rewrittenCondition, SyntheticBoundNodeFactory factory)
	{
		return AddConditionSequencePoint(base.InstrumentDoStatementCondition(original, rewrittenCondition, factory), original.Syntax, factory);
	}

	public override BoundExpression InstrumentWhileStatementCondition(BoundWhileStatement original, BoundExpression rewrittenCondition, SyntheticBoundNodeFactory factory)
	{
		return AddConditionSequencePoint(base.InstrumentWhileStatementCondition(original, rewrittenCondition, factory), original.Syntax, factory);
	}

	public override BoundStatement InstrumentDoStatementConditionalGotoStart(BoundDoStatement original, BoundStatement ifConditionGotoStart)
	{
		DoStatementSyntax doStatementSyntax = (DoStatementSyntax)original.Syntax;
		TextSpan span = TextSpan.FromBounds(doStatementSyntax.WhileKeyword.SpanStart, doStatementSyntax.SemicolonToken.Span.End);
		return new BoundSequencePointWithSpan(doStatementSyntax, base.InstrumentDoStatementConditionalGotoStart(original, ifConditionGotoStart), span);
	}

	public override BoundStatement InstrumentWhileStatementConditionalGotoStartOrBreak(BoundWhileStatement original, BoundStatement ifConditionGotoStart)
	{
		WhileStatementSyntax whileStatementSyntax = (WhileStatementSyntax)original.Syntax;
		TextSpan span = TextSpan.FromBounds(whileStatementSyntax.WhileKeyword.SpanStart, whileStatementSyntax.CloseParenToken.Span.End);
		return new BoundSequencePointWithSpan(whileStatementSyntax, base.InstrumentWhileStatementConditionalGotoStartOrBreak(original, ifConditionGotoStart), span);
	}

	public override BoundStatement InstrumentForEachStatementCollectionVarDeclaration(BoundForEachStatement original, BoundStatement? collectionVarDecl)
	{
		return new BoundSequencePoint(((CommonForEachStatementSyntax)original.Syntax).Expression, base.InstrumentForEachStatementCollectionVarDeclaration(original, collectionVarDecl));
	}

	public override BoundStatement InstrumentForEachStatementDeconstructionVariablesDeclaration(BoundForEachStatement original, BoundStatement iterationVarDecl)
	{
		ForEachVariableStatementSyntax forEachVariableStatementSyntax = (ForEachVariableStatementSyntax)original.Syntax;
		return new BoundSequencePointWithSpan(forEachVariableStatementSyntax, base.InstrumentForEachStatementDeconstructionVariablesDeclaration(original, iterationVarDecl), forEachVariableStatementSyntax.Variable.Span);
	}

	public override BoundStatement InstrumentForEachStatement(BoundForEachStatement original, BoundStatement rewritten)
	{
		CommonForEachStatementSyntax commonForEachStatementSyntax = (CommonForEachStatementSyntax)original.Syntax;
		TextSpan span = ((commonForEachStatementSyntax.AwaitKeyword != default(SyntaxToken)) ? TextSpan.FromBounds(commonForEachStatementSyntax.AwaitKeyword.Span.Start, commonForEachStatementSyntax.ForEachKeyword.Span.End) : commonForEachStatementSyntax.ForEachKeyword.Span);
		BoundSequencePointWithSpan item = new BoundSequencePointWithSpan(commonForEachStatementSyntax, null, span);
		return new BoundStatementList(commonForEachStatementSyntax, ImmutableArray.Create(item, base.InstrumentForEachStatement(original, rewritten)));
	}

	public override BoundStatement InstrumentForEachStatementIterationVarDeclaration(BoundForEachStatement original, BoundStatement iterationVarDecl)
	{
		TextSpan span;
		switch (original.Syntax.Kind())
		{
		case SyntaxKind.ForEachStatement:
		{
			ForEachStatementSyntax forEachStatementSyntax = (ForEachStatementSyntax)original.Syntax;
			span = TextSpan.FromBounds(forEachStatementSyntax.Type.SpanStart, forEachStatementSyntax.Identifier.Span.End);
			break;
		}
		case SyntaxKind.ForEachVariableStatement:
			span = ((ForEachVariableStatementSyntax)original.Syntax).Variable.Span;
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(original.Syntax.Kind());
		}
		return new BoundSequencePointWithSpan(original.Syntax, base.InstrumentForEachStatementIterationVarDeclaration(original, iterationVarDecl), span);
	}

	public override BoundStatement InstrumentForStatementConditionalGotoStartOrBreak(BoundForStatement original, BoundStatement branchBack)
	{
		return BoundSequencePoint.Create(original.Condition?.Syntax, base.InstrumentForStatementConditionalGotoStartOrBreak(original, branchBack));
	}

	public override BoundStatement InstrumentForEachStatementConditionalGotoStart(BoundForEachStatement original, BoundStatement branchBack)
	{
		CommonForEachStatementSyntax commonForEachStatementSyntax = (CommonForEachStatementSyntax)original.Syntax;
		return new BoundSequencePointWithSpan(commonForEachStatementSyntax, base.InstrumentForEachStatementConditionalGotoStart(original, branchBack), commonForEachStatementSyntax.InKeyword.Span);
	}

	public override BoundExpression InstrumentForStatementCondition(BoundForStatement original, BoundExpression rewrittenCondition, SyntheticBoundNodeFactory factory)
	{
		return AddConditionSequencePoint(base.InstrumentForStatementCondition(original, rewrittenCondition, factory), original.Syntax, factory);
	}

	public override BoundStatement InstrumentIfStatementConditionalGoto(BoundIfStatement original, BoundStatement rewritten)
	{
		IfStatementSyntax ifStatementSyntax = (IfStatementSyntax)original.Syntax;
		return new BoundSequencePointWithSpan(ifStatementSyntax, base.InstrumentIfStatementConditionalGoto(original, rewritten), TextSpan.FromBounds(ifStatementSyntax.IfKeyword.SpanStart, ifStatementSyntax.CloseParenToken.Span.End), original.HasErrors);
	}

	public override BoundExpression InstrumentIfStatementCondition(BoundIfStatement original, BoundExpression rewrittenCondition, SyntheticBoundNodeFactory factory)
	{
		return AddConditionSequencePoint(base.InstrumentIfStatementCondition(original, rewrittenCondition, factory), original.Syntax, factory);
	}

	public override BoundStatement InstrumentLabelStatement(BoundLabeledStatement original, BoundStatement rewritten)
	{
		LabeledStatementSyntax labeledStatementSyntax = (LabeledStatementSyntax)original.Syntax;
		TextSpan span = TextSpan.FromBounds(labeledStatementSyntax.Identifier.SpanStart, labeledStatementSyntax.ColonToken.Span.End);
		return new BoundSequencePointWithSpan(labeledStatementSyntax, base.InstrumentLabelStatement(original, rewritten), span);
	}

	public override BoundStatement InstrumentUserDefinedLocalInitialization(BoundLocalDeclaration original, BoundStatement rewritten)
	{
		return AddSequencePoint((original.Syntax.Kind() == SyntaxKind.VariableDeclarator) ? ((VariableDeclaratorSyntax)original.Syntax) : ((LocalDeclarationStatementSyntax)original.Syntax).Declaration.Variables.First(), base.InstrumentUserDefinedLocalInitialization(original, rewritten));
	}

	public override BoundStatement InstrumentLockTargetCapture(BoundLockStatement original, BoundStatement lockTargetCapture)
	{
		LockStatementSyntax lockStatementSyntax = (LockStatementSyntax)original.Syntax;
		return new BoundSequencePointWithSpan(lockStatementSyntax, base.InstrumentLockTargetCapture(original, lockTargetCapture), TextSpan.FromBounds(lockStatementSyntax.LockKeyword.SpanStart, lockStatementSyntax.CloseParenToken.Span.End));
	}

	public override BoundStatement InstrumentReturnStatement(BoundReturnStatement original, BoundStatement rewritten)
	{
		rewritten = base.InstrumentReturnStatement(original, rewritten);
		if (original.WasCompilerGenerated && original.ExpressionOpt == null && original.Syntax.Kind() == SyntaxKind.Block)
		{
			return new BoundSequencePointWithSpan(original.Syntax, rewritten, ((BlockSyntax)original.Syntax).CloseBraceToken.Span);
		}
		if (original.Syntax is ParameterSyntax parameterSyntax)
		{
			return new BoundSequencePointWithSpan(parameterSyntax, rewritten, CreateSpan(parameterSyntax));
		}
		return new BoundSequencePoint(original.Syntax, rewritten);
	}

	public override BoundStatement InstrumentSwitchStatement(BoundSwitchStatement original, BoundStatement rewritten)
	{
		SwitchStatementSyntax switchStatementSyntax = (SwitchStatementSyntax)original.Syntax;
		TextSpan span = TextSpan.FromBounds(switchStatementSyntax.SwitchKeyword.SpanStart, (switchStatementSyntax.CloseParenToken != default(SyntaxToken)) ? switchStatementSyntax.CloseParenToken.Span.End : switchStatementSyntax.Expression.Span.End);
		return new BoundSequencePointWithSpan(switchStatementSyntax, base.InstrumentSwitchStatement(original, rewritten), span);
	}

	public override BoundStatement InstrumentSwitchWhenClauseConditionalGotoBody(BoundExpression original, BoundStatement ifConditionGotoBody)
	{
		WhenClauseSyntax whenClauseSyntax = original.Syntax.FirstAncestorOrSelf<WhenClauseSyntax>();
		return new BoundSequencePointWithSpan(whenClauseSyntax, base.InstrumentSwitchWhenClauseConditionalGotoBody(original, ifConditionGotoBody), whenClauseSyntax.Span);
	}

	public override BoundStatement InstrumentUsingTargetCapture(BoundUsingStatement original, BoundStatement usingTargetCapture)
	{
		return AddSequencePoint((UsingStatementSyntax)original.Syntax, base.InstrumentUsingTargetCapture(original, usingTargetCapture));
	}

	public override void InstrumentCatchBlock(BoundCatchBlock original, ref BoundExpression? rewrittenSource, ref BoundStatementList? rewrittenFilterPrologue, ref BoundExpression? rewrittenFilter, ref BoundBlock rewrittenBody, ref TypeSymbol? rewrittenType, SyntheticBoundNodeFactory factory)
	{
		base.InstrumentCatchBlock(original, ref rewrittenSource, ref rewrittenFilterPrologue, ref rewrittenFilter, ref rewrittenBody, ref rewrittenType, factory);
		if (!original.WasCompilerGenerated && rewrittenFilter != null)
		{
			CatchFilterClauseSyntax filter = ((CatchClauseSyntax)original.Syntax).Filter;
			rewrittenFilter = AddConditionSequencePoint(new BoundSequencePointExpression(filter, rewrittenFilter, rewrittenFilter.Type), filter, factory);
		}
	}

	public override BoundExpression InstrumentSwitchStatementExpression(BoundStatement original, BoundExpression rewrittenExpression, SyntheticBoundNodeFactory factory)
	{
		return AddConditionSequencePoint(base.InstrumentSwitchStatementExpression(original, rewrittenExpression, factory), original.Syntax, factory);
	}

	public override BoundExpression InstrumentSwitchExpressionArmExpression(BoundExpression original, BoundExpression rewrittenExpression, SyntheticBoundNodeFactory factory)
	{
		return new BoundSequencePointExpression(original.Syntax, base.InstrumentSwitchExpressionArmExpression(original, rewrittenExpression, factory), rewrittenExpression.Type);
	}

	public override BoundStatement InstrumentSwitchBindCasePatternVariables(BoundStatement bindings)
	{
		return BoundSequencePoint.CreateHidden(base.InstrumentSwitchBindCasePatternVariables(bindings));
	}

	private static BoundStatement AddSequencePoint(BoundStatement node)
	{
		return new BoundSequencePoint(node.Syntax, node);
	}

	internal static BoundStatement AddSequencePoint(VariableDeclaratorSyntax declaratorSyntax, BoundStatement rewrittenStatement)
	{
		GetBreakpointSpan(declaratorSyntax, out SyntaxNode _, out TextSpan? part);
		BoundStatement boundStatement = BoundSequencePoint.Create(declaratorSyntax, part, rewrittenStatement);
		boundStatement.WasCompilerGenerated = rewrittenStatement.WasCompilerGenerated;
		return boundStatement;
	}

	internal static BoundStatement AddSequencePoint(PropertyDeclarationSyntax declarationSyntax, BoundStatement rewrittenStatement)
	{
		int spanStart = declarationSyntax.Initializer.Value.SpanStart;
		int end = declarationSyntax.Initializer.Span.End;
		TextSpan value = TextSpan.FromBounds(spanStart, end);
		BoundStatement boundStatement = BoundSequencePoint.Create(declarationSyntax, value, rewrittenStatement);
		boundStatement.WasCompilerGenerated = rewrittenStatement.WasCompilerGenerated;
		return boundStatement;
	}

	internal static BoundStatement AddSequencePoint(UsingStatementSyntax usingSyntax, BoundStatement rewrittenStatement)
	{
		int start = usingSyntax.Span.Start;
		int end = usingSyntax.CloseParenToken.Span.End;
		TextSpan span = TextSpan.FromBounds(start, end);
		return new BoundSequencePointWithSpan(usingSyntax, rewrittenStatement, span);
	}

	private static TextSpan CreateSpan(ParameterSyntax parameter)
	{
		return CreateSpan(parameter.Modifiers, parameter.Type, parameter.Identifier);
	}

	private static TextSpan CreateSpan(SyntaxTokenList startOpt, SyntaxNodeOrToken startFallbackOpt, SyntaxNodeOrToken endOpt)
	{
		int start = ((startOpt.Count > 0) ? startOpt.First().SpanStart : ((!(startFallbackOpt != default(SyntaxNodeOrToken))) ? endOpt.SpanStart : startFallbackOpt.SpanStart));
		int end = ((!(endOpt != default(SyntaxNodeOrToken))) ? GetEndPosition(startFallbackOpt) : GetEndPosition(endOpt));
		return TextSpan.FromBounds(start, end);
	}

	private static int GetEndPosition(SyntaxNodeOrToken nodeOrToken)
	{
		if (!nodeOrToken.AsNode(out SyntaxNode node))
		{
			return nodeOrToken.Span.End;
		}
		return node.GetLastToken().Span.End;
	}

	internal static void GetBreakpointSpan(VariableDeclaratorSyntax declaratorSyntax, out SyntaxNode node, out TextSpan? part)
	{
		VariableDeclarationSyntax variableDeclarationSyntax = (VariableDeclarationSyntax)declaratorSyntax.Parent;
		if (variableDeclarationSyntax.Variables.First() == declaratorSyntax)
		{
			switch (variableDeclarationSyntax.Parent.Kind())
			{
			case SyntaxKind.FieldDeclaration:
			case SyntaxKind.EventFieldDeclaration:
			{
				SyntaxTokenList modifiers2 = ((BaseFieldDeclarationSyntax)variableDeclarationSyntax.Parent).Modifiers;
				GetFirstLocalOrFieldBreakpointSpan(modifiers2.Any() ? new SyntaxToken?(modifiers2[0]) : ((SyntaxToken?)null), declaratorSyntax, out node, out part);
				break;
			}
			case SyntaxKind.LocalDeclarationStatement:
			{
				LocalDeclarationStatementSyntax localDeclarationStatementSyntax = (LocalDeclarationStatementSyntax)variableDeclarationSyntax.Parent;
				SyntaxTokenList modifiers = localDeclarationStatementSyntax.Modifiers;
				GetFirstLocalOrFieldBreakpointSpan(modifiers.Any() ? new SyntaxToken?(modifiers[0]) : ((localDeclarationStatementSyntax.UsingKeyword == default(SyntaxToken)) ? ((SyntaxToken?)null) : new SyntaxToken?((localDeclarationStatementSyntax.AwaitKeyword == default(SyntaxToken)) ? localDeclarationStatementSyntax.UsingKeyword : localDeclarationStatementSyntax.AwaitKeyword)), declaratorSyntax, out node, out part);
				break;
			}
			case SyntaxKind.ForStatement:
			case SyntaxKind.UsingStatement:
			case SyntaxKind.FixedStatement:
				node = variableDeclarationSyntax;
				part = TextSpan.FromBounds(variableDeclarationSyntax.SpanStart, declaratorSyntax.Span.End);
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(variableDeclarationSyntax.Parent.Kind());
			}
		}
		else
		{
			node = declaratorSyntax;
			part = null;
		}
	}

	internal static void GetFirstLocalOrFieldBreakpointSpan(SyntaxToken? firstToken, VariableDeclaratorSyntax declaratorSyntax, out SyntaxNode node, out TextSpan? part)
	{
		VariableDeclarationSyntax variableDeclarationSyntax = (VariableDeclarationSyntax)declaratorSyntax.Parent;
		int start = firstToken?.SpanStart ?? variableDeclarationSyntax.SpanStart;
		int end = ((variableDeclarationSyntax.Variables.Count != 1) ? declaratorSyntax.Span.End : variableDeclarationSyntax.Parent.Span.End);
		part = TextSpan.FromBounds(start, end);
		node = variableDeclarationSyntax.Parent;
	}

	private static BoundExpression AddConditionSequencePoint(BoundExpression condition, SyntaxNode synthesizedVariableSyntax, SyntheticBoundNodeFactory factory)
	{
		if (!factory.Compilation.Options.EnableEditAndContinue)
		{
			return condition;
		}
		LocalSymbol localSymbol = factory.SynthesizedLocal(condition.Type, synthesizedVariableSyntax, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.ConditionalBranchDiscriminator);
		BoundExpression value = ((condition.ConstantValueOpt == null) ? new BoundSequencePointExpression(null, factory.Local(localSymbol), condition.Type) : condition);
		return new BoundSequence(condition.Syntax, ImmutableArray.Create(localSymbol), ImmutableArray.Create(factory.AssignmentExpression(factory.Local(localSymbol), condition)), value, condition.Type);
	}
}
