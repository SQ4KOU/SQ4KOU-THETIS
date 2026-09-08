using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ForLoopBinder : LoopBinder
{
	private readonly ForStatementSyntax _syntax;

	internal override SyntaxNode ScopeDesignator => _syntax;

	public ForLoopBinder(Binder enclosing, ForStatementSyntax syntax)
		: base(enclosing)
	{
		_syntax = syntax;
	}

	protected override ImmutableArray<LocalSymbol> BuildLocals()
	{
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		if (_syntax.Declaration != null)
		{
			_syntax.Declaration.Type.VisitRankSpecifiers(delegate(ArrayRankSpecifierSyntax rankSpecifier, (ForLoopBinder binder, ArrayBuilder<LocalSymbol> locals) args)
			{
				foreach (ExpressionSyntax size in rankSpecifier.Sizes)
				{
					if (size.Kind() != SyntaxKind.OmittedArraySizeExpression)
					{
						ExpressionVariableFinder.FindExpressionVariables(args.binder, args.locals, size);
					}
				}
			}, (this, instance));
			foreach (VariableDeclaratorSyntax variable in _syntax.Declaration.Variables)
			{
				SourceLocalSymbol item = MakeLocal(_syntax.Declaration, variable, LocalDeclarationKind.RegularVariable, allowScoped: true);
				instance.Add(item);
				ExpressionVariableFinder.FindExpressionVariables(this, instance, variable);
			}
		}
		else
		{
			ExpressionVariableFinder.FindExpressionVariables(this, instance, _syntax.Initializers);
		}
		return instance.ToImmutableAndFree();
	}

	internal override BoundForStatement BindForParts(BindingDiagnosticBag diagnostics, Binder originalBinder)
	{
		return BindForParts(_syntax, originalBinder, diagnostics);
	}

	private BoundForStatement BindForParts(ForStatementSyntax node, Binder originalBinder, BindingDiagnosticBag diagnostics)
	{
		BoundStatement initializer;
		if (_syntax.Declaration != null)
		{
			TypeSyntax typeSyntax = _syntax.Declaration.Type.SkipScoped(out var _);
			if (typeSyntax is RefTypeSyntax)
			{
				MessageID.IDS_FeatureRefFor.CheckFeatureAvailability(diagnostics, typeSyntax);
			}
			initializer = originalBinder.BindForOrUsingOrFixedDeclarations(node.Declaration, LocalDeclarationKind.RegularVariable, diagnostics, out var _);
		}
		else
		{
			initializer = originalBinder.BindStatementExpressionList(node.Initializers, diagnostics);
		}
		BoundExpression condition = null;
		ImmutableArray<LocalSymbol> innerLocals = ImmutableArray<LocalSymbol>.Empty;
		ExpressionSyntax condition2 = node.Condition;
		if (condition2 != null)
		{
			originalBinder = originalBinder.GetBinder(condition2);
			condition = originalBinder.BindBooleanExpression(condition2, diagnostics);
			innerLocals = originalBinder.GetDeclaredLocalsForScope(condition2);
		}
		BoundStatement boundStatement = null;
		SeparatedSyntaxList<ExpressionSyntax> incrementors = node.Incrementors;
		if (incrementors.Count > 0)
		{
			ExpressionSyntax expressionSyntax = incrementors.First();
			Binder? binder = originalBinder.GetBinder(expressionSyntax);
			boundStatement = binder.BindStatementExpressionList(incrementors, diagnostics);
			ImmutableArray<LocalSymbol> declaredLocalsForScope = binder.GetDeclaredLocalsForScope(expressionSyntax);
			if (!declaredLocalsForScope.IsEmpty)
			{
				boundStatement = ((boundStatement.Kind != BoundKind.StatementList) ? new BoundBlock(boundStatement.Syntax, declaredLocalsForScope, ImmutableArray.Create(boundStatement))
				{
					WasCompilerGenerated = true
				} : new BoundBlock(expressionSyntax, declaredLocalsForScope, ((BoundStatementList)boundStatement).Statements)
				{
					WasCompilerGenerated = true
				});
			}
		}
		BoundStatement body = originalBinder.BindPossibleEmbeddedStatement(node.Statement, diagnostics);
		return new BoundForStatement(node, Locals, initializer, innerLocals, condition, boundStatement, body, BreakLabel, ContinueLabel);
	}

	internal override ImmutableArray<LocalSymbol> GetDeclaredLocalsForScope(SyntaxNode scopeDesignator)
	{
		if (_syntax == scopeDesignator)
		{
			return Locals;
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/ForLoopBinder.cs", 147);
	}

	internal override ImmutableArray<LocalFunctionSymbol> GetDeclaredLocalFunctionsForScope(CSharpSyntaxNode scopeDesignator)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/ForLoopBinder.cs", 152);
	}
}
