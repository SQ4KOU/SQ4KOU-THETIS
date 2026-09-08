using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class UsingStatementBinder : LockOrUsingBinder
{
	private readonly UsingStatementSyntax _syntax;

	protected override ExpressionSyntax TargetExpressionSyntax => _syntax.Expression;

	internal override SyntaxNode ScopeDesignator => _syntax;

	public UsingStatementBinder(Binder enclosing, UsingStatementSyntax syntax)
		: base(enclosing)
	{
		_syntax = syntax;
	}

	protected override ImmutableArray<LocalSymbol> BuildLocals()
	{
		ExpressionSyntax targetExpressionSyntax = TargetExpressionSyntax;
		VariableDeclarationSyntax declaration = _syntax.Declaration;
		if (targetExpressionSyntax != null)
		{
			ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
			ExpressionVariableFinder.FindExpressionVariables(this, instance, targetExpressionSyntax);
			return instance.ToImmutableAndFree();
		}
		ArrayBuilder<LocalSymbol> instance2 = ArrayBuilder<LocalSymbol>.GetInstance(declaration.Variables.Count);
		declaration.Type.VisitRankSpecifiers(delegate(ArrayRankSpecifierSyntax rankSpecifier, (UsingStatementBinder binder, ArrayBuilder<LocalSymbol> locals) args)
		{
			foreach (ExpressionSyntax size in rankSpecifier.Sizes)
			{
				if (size.Kind() != SyntaxKind.OmittedArraySizeExpression)
				{
					ExpressionVariableFinder.FindExpressionVariables(args.binder, args.locals, size);
				}
			}
		}, (this, instance2));
		foreach (VariableDeclaratorSyntax variable in declaration.Variables)
		{
			instance2.Add(MakeLocal(declaration, variable, LocalDeclarationKind.UsingVariable, allowScoped: true));
			ExpressionVariableFinder.FindExpressionVariables(this, instance2, variable);
		}
		return instance2.ToImmutableAndFree();
	}

	internal override BoundStatement BindUsingStatementParts(BindingDiagnosticBag diagnostics, Binder originalBinder)
	{
		object obj = TargetExpressionSyntax;
		VariableDeclarationSyntax declaration = _syntax.Declaration;
		_syntax.AwaitKeyword.Kind();
		if (obj == null)
		{
			obj = declaration;
		}
		return BindUsingStatementOrDeclarationFromParts((SyntaxNode)obj, _syntax.UsingKeyword, _syntax.AwaitKeyword, originalBinder, this, diagnostics);
	}

	internal static BoundStatement BindUsingStatementOrDeclarationFromParts(SyntaxNode syntax, SyntaxToken usingKeyword, SyntaxToken awaitKeyword, Binder originalBinder, UsingStatementBinder? usingBinderOpt, BindingDiagnosticBag diagnostics)
	{
		bool flag = syntax.Kind() == SyntaxKind.LocalDeclarationStatement;
		bool flag2 = !flag && syntax.Kind() != SyntaxKind.VariableDeclaration;
		bool hasAwait = awaitKeyword != default(SyntaxToken);
		if (flag)
		{
			Binder.CheckFeatureAvailability(usingKeyword, MessageID.IDS_FeatureUsingDeclarations, diagnostics);
		}
		else if (hasAwait)
		{
			Binder.CheckFeatureAvailability(awaitKeyword, MessageID.IDS_FeatureAsyncUsing, diagnostics);
		}
		bool hasErrors = false;
		ImmutableArray<BoundLocalDeclaration> declarationsOpt = default(ImmutableArray<BoundLocalDeclaration>);
		BoundMultipleLocalDeclarations declarationsOpt2 = null;
		BoundExpression expressionOpt = null;
		TypeSymbol declarationTypeOpt = null;
		MethodArgumentInfo patternDisposeInfo;
		TypeSymbol awaitableType;
		if (flag2)
		{
			expressionOpt = usingBinderOpt.BindTargetExpression(diagnostics, originalBinder);
			hasErrors |= !bindDisposable(fromExpression: true, out patternDisposeInfo, out awaitableType);
			if ((object)expressionOpt.Type != null)
			{
				Binder.CheckRestrictedTypeInAsyncMethod(originalBinder.ContainingMemberOrLambda, expressionOpt.Type, diagnostics, expressionOpt.Syntax);
			}
		}
		else
		{
			VariableDeclarationSyntax variableDeclarationSyntax = (flag ? ((LocalDeclarationStatementSyntax)syntax).Declaration : ((VariableDeclarationSyntax)syntax));
			originalBinder.BindForOrUsingOrFixedDeclarations(variableDeclarationSyntax, LocalDeclarationKind.UsingVariable, diagnostics, out declarationsOpt);
			declarationsOpt2 = new BoundMultipleLocalDeclarations(variableDeclarationSyntax, declarationsOpt);
			declarationTypeOpt = declarationsOpt[0].DeclaredTypeOpt.Type;
			if (declarationTypeOpt.IsDynamic())
			{
				patternDisposeInfo = null;
				awaitableType = null;
			}
			else
			{
				hasErrors |= !bindDisposable(fromExpression: false, out patternDisposeInfo, out awaitableType);
			}
		}
		BoundAwaitableInfo awaitOpt = null;
		if (hasAwait)
		{
			originalBinder.ReportBadAwaitDiagnostics(awaitKeyword, diagnostics, ref hasErrors);
			if ((object)awaitableType == null)
			{
				awaitOpt = new BoundAwaitableInfo(syntax, null, isDynamic: true, null, null, null, null, null)
				{
					WasCompilerGenerated = true
				};
			}
			else
			{
				hasErrors |= Binder.ReportUseSite(awaitableType, diagnostics, awaitKeyword);
				BoundAwaitableValuePlaceholder getAwaiterPlaceholder = new BoundAwaitableValuePlaceholder(syntax, awaitableType).MakeCompilerGenerated();
				awaitOpt = originalBinder.BindAwaitInfo(getAwaiterPlaceholder, syntax, diagnostics, ref hasErrors);
			}
		}
		if (flag)
		{
			return new BoundUsingLocalDeclarations(syntax, patternDisposeInfo, awaitOpt, declarationsOpt, hasErrors);
		}
		BoundStatement body = originalBinder.BindPossibleEmbeddedStatement(usingBinderOpt._syntax.Statement, diagnostics);
		return new BoundUsingStatement(usingBinderOpt._syntax, usingBinderOpt.Locals, declarationsOpt2, expressionOpt, body, awaitOpt, patternDisposeInfo, hasErrors);
		bool bindDisposable(bool fromExpression, out MethodArgumentInfo? reference, out TypeSymbol? reference2)
		{
			reference = null;
			reference2 = null;
			TypeSymbol typeSymbol = (fromExpression ? expressionOpt.Type : declarationTypeOpt);
			if ((object)typeSymbol != null && (typeSymbol.IsRefLikeType | hasAwait))
			{
				BoundExpression expr = (fromExpression ? expressionOpt : new BoundLocal(syntax, declarationsOpt[0].LocalSymbol, null, typeSymbol)
				{
					WasCompilerGenerated = true
				});
				BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(diagnostics);
				MethodSymbol methodSymbol = originalBinder.TryFindDisposePatternMethod(expr, syntax, hasAwait, instance, out var isExpanded);
				if ((object)methodSymbol != null)
				{
					diagnostics.AddRangeAndFree(instance);
					MessageID.IDS_FeatureDisposalPattern.CheckFeatureAvailability(diagnostics, originalBinder.Compilation, syntax.Location);
					ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance(methodSymbol.ParameterCount);
					ImmutableArray<int> argsToParamsOpt = default(ImmutableArray<int>);
					originalBinder.BindDefaultArguments(usingBinderOpt?._syntax ?? syntax, methodSymbol.Parameters, null, instance2, null, null, ref argsToParamsOpt, out var defaultArguments, isExpanded, enableCallerInfo: true, diagnostics);
					reference = new MethodArgumentInfo(methodSymbol, instance2.ToImmutableAndFree(), defaultArguments, isExpanded);
					if (hasAwait)
					{
						reference2 = methodSymbol.ReturnType;
					}
					return true;
				}
				instance.Free();
			}
			NamedTypeSymbol namedTypeSymbol = getDisposableInterface(hasAwait);
			if (implementsInterface(fromExpression, namedTypeSymbol, diagnostics))
			{
				if (hasAwait)
				{
					reference2 = originalBinder.Compilation.GetWellKnownType(WellKnownType.System_Threading_Tasks_ValueTask);
				}
				return !Binder.ReportUseSite(namedTypeSymbol, diagnostics, hasAwait ? awaitKeyword : usingKeyword);
			}
			if ((object)typeSymbol == null || !typeSymbol.IsErrorType())
			{
				NamedTypeSymbol targetInterface = getDisposableInterface(!hasAwait);
				ErrorCode code = ((!implementsInterface(fromExpression, targetInterface, BindingDiagnosticBag.Discarded)) ? (hasAwait ? ErrorCode.ERR_NoConvToIAsyncDisp : ErrorCode.ERR_NoConvToIDisp) : (hasAwait ? ErrorCode.ERR_NoConvToIAsyncDispWrongAsync : ErrorCode.ERR_NoConvToIDispWrongAsync));
				Binder.Error(diagnostics, code, syntax, declarationTypeOpt ?? expressionOpt.Display);
			}
			return false;
		}
		NamedTypeSymbol getDisposableInterface(bool isAsync)
		{
			if (!isAsync)
			{
				return originalBinder.Compilation.GetSpecialType(SpecialType.System_IDisposable);
			}
			return originalBinder.Compilation.GetWellKnownType(WellKnownType.System_IAsyncDisposable);
		}
		bool implementsInterface(bool fromExpression, NamedTypeSymbol targetInterface, BindingDiagnosticBag bindingDiagnosticBag)
		{
			Conversions conversions = originalBinder.Conversions;
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = originalBinder.GetNewCompoundUseSiteInfo(bindingDiagnosticBag);
			bool result = ((!fromExpression) ? conversions.HasImplicitConversionToOrImplementsVarianceCompatibleInterface(declarationTypeOpt, targetInterface, ref useSiteInfo, out var needSupportForRefStructInterfaces) : conversions.HasImplicitConversionToOrImplementsVarianceCompatibleInterface(expressionOpt, targetInterface, ref useSiteInfo, out needSupportForRefStructInterfaces));
			bindingDiagnosticBag.Add(syntax, useSiteInfo);
			if (needSupportForRefStructInterfaces && (fromExpression ? expressionOpt.Type : declarationTypeOpt).ContainingModule != originalBinder.Compilation.SourceModule)
			{
				Binder.CheckFeatureAvailability(syntax, MessageID.IDS_FeatureRefStructInterfaces, bindingDiagnosticBag);
			}
			return result;
		}
	}

	internal override ImmutableArray<LocalSymbol> GetDeclaredLocalsForScope(SyntaxNode scopeDesignator)
	{
		if (_syntax == scopeDesignator)
		{
			return Locals;
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/UsingStatementBinder.cs", 317);
	}

	internal override ImmutableArray<LocalFunctionSymbol> GetDeclaredLocalFunctionsForScope(CSharpSyntaxNode scopeDesignator)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/UsingStatementBinder.cs", 322);
	}
}
