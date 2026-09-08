using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ForEachLoopBinder : LoopBinder
{
	private readonly CommonForEachStatementSyntax _syntax;

	private SourceLocalSymbol IterationVariable
	{
		get
		{
			if (_syntax.Kind() != SyntaxKind.ForEachStatement)
			{
				return null;
			}
			return (SourceLocalSymbol)Locals[0];
		}
	}

	private bool IsAsync => _syntax.AwaitKeyword != default(SyntaxToken);

	internal override SyntaxNode ScopeDesignator => _syntax;

	public ForEachLoopBinder(Binder enclosing, CommonForEachStatementSyntax syntax)
		: base(enclosing)
	{
		_syntax = syntax;
	}

	internal override ImmutableArray<LocalSymbol> GetDeclaredLocalsForScope(SyntaxNode scopeDesignator)
	{
		if (_syntax == scopeDesignator)
		{
			return Locals;
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/ForEachLoopBinder.cs", 54);
	}

	internal override ImmutableArray<LocalFunctionSymbol> GetDeclaredLocalFunctionsForScope(CSharpSyntaxNode scopeDesignator)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/ForEachLoopBinder.cs", 59);
	}

	protected override ImmutableArray<LocalSymbol> BuildLocals()
	{
		switch (_syntax.Kind())
		{
		case SyntaxKind.ForEachVariableStatement:
		{
			ForEachVariableStatementSyntax forEachVariableStatementSyntax = (ForEachVariableStatementSyntax)_syntax;
			ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
			CollectLocalsFromDeconstruction(forEachVariableStatementSyntax.Variable, LocalDeclarationKind.ForEachIterationVariable, instance, forEachVariableStatementSyntax);
			return instance.ToImmutableAndFree();
		}
		case SyntaxKind.ForEachStatement:
		{
			ForEachStatementSyntax forEachStatementSyntax = (ForEachStatementSyntax)_syntax;
			return ImmutableArray.Create((LocalSymbol)SourceLocalSymbol.MakeForeachLocal((MethodSymbol)ContainingMemberOrLambda, this, forEachStatementSyntax.Type, forEachStatementSyntax.Identifier, forEachStatementSyntax.Expression));
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(_syntax.Kind());
		}
	}

	internal void CollectLocalsFromDeconstruction(ExpressionSyntax declaration, LocalDeclarationKind kind, ArrayBuilder<LocalSymbol> locals, SyntaxNode deconstructionStatement, Binder enclosingBinderOpt = null)
	{
		switch (declaration.Kind())
		{
		case SyntaxKind.TupleExpression:
			foreach (ArgumentSyntax argument in ((TupleExpressionSyntax)declaration).Arguments)
			{
				CollectLocalsFromDeconstruction(argument.Expression, kind, locals, deconstructionStatement, enclosingBinderOpt);
			}
			break;
		case SyntaxKind.DeclarationExpression:
		{
			DeclarationExpressionSyntax declarationExpressionSyntax = (DeclarationExpressionSyntax)declaration;
			CollectLocalsFromDeconstruction(declarationExpressionSyntax.Designation, declarationExpressionSyntax.Type, kind, locals, deconstructionStatement, enclosingBinderOpt);
			break;
		}
		default:
			ExpressionVariableFinder.FindExpressionVariables(this, locals, declaration);
			break;
		case SyntaxKind.IdentifierName:
			break;
		}
	}

	internal void CollectLocalsFromDeconstruction(VariableDesignationSyntax designation, TypeSyntax closestTypeSyntax, LocalDeclarationKind kind, ArrayBuilder<LocalSymbol> locals, SyntaxNode deconstructionStatement, Binder enclosingBinderOpt)
	{
		switch (designation.Kind())
		{
		case SyntaxKind.SingleVariableDesignation:
		{
			SingleVariableDesignationSyntax singleVariableDesignationSyntax = (SingleVariableDesignationSyntax)designation;
			SourceLocalSymbol item = SourceLocalSymbol.MakeDeconstructionLocal(ContainingMemberOrLambda, this, enclosingBinderOpt ?? this, closestTypeSyntax, singleVariableDesignationSyntax.Identifier, kind, deconstructionStatement);
			locals.Add(item);
			break;
		}
		case SyntaxKind.ParenthesizedVariableDesignation:
			foreach (VariableDesignationSyntax variable in ((ParenthesizedVariableDesignationSyntax)designation).Variables)
			{
				CollectLocalsFromDeconstruction(variable, closestTypeSyntax, kind, locals, deconstructionStatement, enclosingBinderOpt);
			}
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(designation.Kind());
		case SyntaxKind.DiscardDesignation:
			break;
		}
	}

	internal override BoundStatement BindForEachParts(BindingDiagnosticBag diagnostics, Binder originalBinder)
	{
		return BindForEachPartsWorker(diagnostics, originalBinder);
	}

	internal override BoundStatement BindForEachDeconstruction(BindingDiagnosticBag diagnostics, Binder originalBinder)
	{
		BoundExpression collectionExpr = originalBinder.GetBinder(_syntax.Expression).BindRValueWithoutTargetType(_syntax.Expression, diagnostics);
		GetEnumeratorInfoAndInferCollectionElementType(_syntax, _syntax.Expression, ref collectionExpr, IsAsync, isSpread: false, diagnostics, out var inferredType, out var _);
		ExpressionSyntax variable = ((ForEachVariableStatementSyntax)_syntax).Variable;
		BoundDeconstructValuePlaceholder rightPlaceholder = new BoundDeconstructValuePlaceholder(_syntax.Expression, null, isDiscardExpression: false, inferredType.Type ?? CreateErrorType("var"));
		DeclarationExpressionSyntax declaration = null;
		ExpressionSyntax expression = null;
		BoundDeconstructionAssignmentOperator expression2 = BindDeconstruction(variable, variable, _syntax.Expression, diagnostics, ref declaration, ref expression, resultIsUsedOverride: false, rightPlaceholder);
		return new BoundExpressionStatement(_syntax, expression2);
	}

	private BoundForEachStatement BindForEachPartsWorker(BindingDiagnosticBag diagnostics, Binder originalBinder)
	{
		if (IsAsync)
		{
			Binder.CheckFeatureAvailability(_syntax.AwaitKeyword, MessageID.IDS_FeatureAsyncStreams, diagnostics);
		}
		BoundExpression collectionExpr = originalBinder.GetBinder(_syntax.Expression).BindRValueWithoutTargetType(_syntax.Expression, diagnostics);
		bool flag = !GetEnumeratorInfoAndInferCollectionElementType(_syntax, _syntax.Expression, ref collectionExpr, IsAsync, isSpread: false, diagnostics, out var inferredType, out var builder);
		flag |= builder.IsIncomplete;
		BoundAwaitableInfo boundAwaitableInfo = null;
		MethodSymbol methodSymbol = builder.GetEnumeratorInfo?.Method;
		if (methodSymbol != null)
		{
			originalBinder.CheckImplicitThisCopyInReadOnlyMember(collectionExpr, methodSymbol, diagnostics);
			if (!flag)
			{
				if (methodSymbol.IsExtensionMethod)
				{
					(IsAsync ? MessageID.IDS_FeatureExtensionGetAsyncEnumerator : MessageID.IDS_FeatureExtensionGetEnumerator).CheckFeatureAvailability(diagnostics, base.Compilation, collectionExpr.Syntax.Location);
					ImmutableArray<RefKind> parameterRefKinds = methodSymbol.ParameterRefKinds;
					if (!parameterRefKinds.IsDefault && parameterRefKinds[0] == RefKind.Ref)
					{
						Binder.Error(diagnostics, ErrorCode.ERR_RefLvalueExpected, collectionExpr.Syntax);
						flag = true;
					}
				}
				else if (methodSymbol.IsExtensionBlockMember() && methodSymbol.ContainingType.ExtensionParameter.RefKind == RefKind.Ref)
				{
					Binder.Error(diagnostics, ErrorCode.ERR_RefLvalueExpected, collectionExpr.Syntax);
					flag = true;
				}
			}
		}
		if (IsAsync)
		{
			ExpressionSyntax expression = _syntax.Expression;
			ReportBadAwaitDiagnostics(_syntax.AwaitKeyword, diagnostics, ref flag);
			BoundAwaitableValuePlaceholder getAwaiterPlaceholder = new BoundAwaitableValuePlaceholder(expression, builder.MoveNextInfo?.Method.ReturnType ?? CreateErrorType());
			boundAwaitableInfo = BindAwaitInfo(getAwaiterPlaceholder, expression, diagnostics, ref flag);
			if (!flag)
			{
				MethodSymbol? obj = boundAwaitableInfo.GetResult ?? boundAwaitableInfo.RuntimeAsyncAwaitCall?.Method;
				if ((object)obj == null || obj.ReturnType.SpecialType != SpecialType.System_Boolean)
				{
					diagnostics.Add(ErrorCode.ERR_BadGetAsyncEnumerator, expression.Location, methodSymbol.ReturnTypeWithAnnotations, methodSymbol);
					flag = true;
				}
			}
		}
		bool flag2 = false;
		BoundForEachDeconstructStep deconstructionOpt = null;
		BoundExpression boundExpression = null;
		TypeWithAnnotations typeWithAnnotations;
		BoundTypeExpression boundTypeExpression;
		switch (_syntax.Kind())
		{
		case SyntaxKind.ForEachStatement:
		{
			ForEachStatementSyntax forEachStatementSyntax = (ForEachStatementSyntax)_syntax;
			flag2 = originalBinder.ValidateDeclarationNameConflictsInScope(IterationVariable, diagnostics);
			TypeSyntax type = forEachStatementSyntax.Type;
			if (type is ScopedTypeSyntax scopedTypeSyntax)
			{
				ModifierUtils.CheckScopedModifierAvailability(type, scopedTypeSyntax.ScopedKeyword, diagnostics);
				type = scopedTypeSyntax.Type;
			}
			if (type is RefTypeSyntax refTypeSyntax)
			{
				MessageID.IDS_FeatureRefForEach.CheckFeatureAvailability(diagnostics, type);
				type = refTypeSyntax.Type;
			}
			TypeWithAnnotations typeWithAnnotations2 = BindTypeOrVarKeyword(type, diagnostics, out var isVar, out var alias);
			if (isVar)
			{
				typeWithAnnotations2 = (inferredType.HasType ? inferredType : TypeWithAnnotations.Create(CreateErrorType("var")));
			}
			typeWithAnnotations = typeWithAnnotations2;
			boundTypeExpression = new BoundTypeExpression(type, alias, typeWithAnnotations);
			SourceLocalSymbol iterationVariable = IterationVariable;
			iterationVariable.SetTypeWithAnnotations(typeWithAnnotations2);
			ReportFieldContextualKeywordConflictIfAny(iterationVariable, forEachStatementSyntax, forEachStatementSyntax.Identifier, diagnostics);
			Binder.CheckRestrictedTypeInAsyncMethod(ContainingMemberOrLambda, typeWithAnnotations2.Type, diagnostics, type);
			if (iterationVariable.Scope == ScopedKind.ScopedValue && !typeWithAnnotations2.Type.IsErrorOrRefLikeOrAllowsRefLikeType())
			{
				diagnostics.Add(ErrorCode.ERR_ScopedRefAndRefStructOnly, type.Location);
			}
			if (iterationVariable.RefKind != RefKind.None && CheckRefLocalInAsyncOrIteratorMethod(iterationVariable.IdentifierToken, diagnostics))
			{
				flag = true;
			}
			if (!flag)
			{
				BindValueKind valueKind = iterationVariable.RefKind switch
				{
					RefKind.None => BindValueKind.RValue, 
					RefKind.Ref => BindValueKind.Assignable | BindValueKind.RefersToLocation, 
					RefKind.In => BindValueKind.RefersToLocation, 
					_ => throw ExceptionUtilities.UnexpectedValue(iterationVariable.RefKind), 
				};
				flag = ((builder.InlineArraySpanType != WellKnownType.Unknown) ? (flag | !CheckValueKind(collectionExpr.Syntax, collectionExpr, valueKind, checkingReceiver: false, diagnostics)) : (flag | !CheckMethodReturnValueKind(builder.CurrentPropertyGetter, null, collectionExpr.Syntax, valueKind, checkingReceiver: false, diagnostics)));
			}
			break;
		}
		case SyntaxKind.ForEachVariableStatement:
		{
			ForEachVariableStatementSyntax forEachVariableStatementSyntax = (ForEachVariableStatementSyntax)_syntax;
			typeWithAnnotations = (inferredType.HasType ? inferredType : TypeWithAnnotations.Create(CreateErrorType("var")));
			ExpressionSyntax variable = forEachVariableStatementSyntax.Variable;
			if (variable.IsDeconstructionLeft())
			{
				BoundDeconstructValuePlaceholder boundDeconstructValuePlaceholder = new BoundDeconstructValuePlaceholder(_syntax.Expression, null, isDiscardExpression: false, typeWithAnnotations.Type).MakeCompilerGenerated();
				DeclarationExpressionSyntax declaration = null;
				ExpressionSyntax expression2 = null;
				BoundDeconstructionAssignmentOperator deconstructionAssignment = BindDeconstruction(variable, variable, _syntax.Expression, diagnostics, ref declaration, ref expression2, resultIsUsedOverride: false, boundDeconstructValuePlaceholder);
				if (expression2 != null)
				{
					Binder.Error(diagnostics, ErrorCode.ERR_MustDeclareForeachIteration, variable);
					flag = true;
				}
				deconstructionOpt = new BoundForEachDeconstructStep(variable, deconstructionAssignment, boundDeconstructValuePlaceholder).MakeCompilerGenerated();
			}
			else
			{
				boundExpression = BindToTypeForErrorRecovery(BindExpression(forEachVariableStatementSyntax.Variable, BindingDiagnosticBag.Discarded));
				if (boundExpression.Kind == BoundKind.DiscardExpression)
				{
					boundExpression = ((BoundDiscardExpression)boundExpression).FailInference(this, null);
				}
				flag = true;
				if (!forEachVariableStatementSyntax.HasErrors)
				{
					Binder.Error(diagnostics, ErrorCode.ERR_MustDeclareForeachIteration, variable);
				}
			}
			boundTypeExpression = new BoundTypeExpression(variable, null, typeWithAnnotations).MakeCompilerGenerated();
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(_syntax.Kind());
		}
		BoundStatement body = originalBinder.BindPossibleEmbeddedStatement(_syntax.Statement, diagnostics);
		ImmutableArray<LocalSymbol> locals = Locals;
		flag = flag || boundTypeExpression.HasErrors || typeWithAnnotations.Type.IsErrorType();
		if (flag)
		{
			return new BoundForEachStatement(_syntax, null, null, null, boundTypeExpression, locals, boundExpression, collectionExpr, deconstructionOpt, body, BreakLabel, ContinueLabel, flag);
		}
		flag |= flag2;
		SyntaxToken forEachKeyword = _syntax.ForEachKeyword;
		ReportDiagnosticsIfObsolete(diagnostics, methodSymbol, forEachKeyword, hasBaseReceiver: false);
		Binder.ReportDiagnosticsIfUnmanagedCallersOnly(diagnostics, methodSymbol, forEachKeyword, isDelegateConversion: false);
		ReportDiagnosticsIfObsolete(diagnostics, builder.MoveNextInfo.Method, forEachKeyword, hasBaseReceiver: false);
		ReportDiagnosticsIfObsolete(diagnostics, builder.CurrentPropertyGetter, forEachKeyword, hasBaseReceiver: false);
		ReportDiagnosticsIfObsolete(diagnostics, builder.CurrentPropertyGetter.AssociatedSymbol, forEachKeyword, hasBaseReceiver: false);
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = GetNewCompoundUseSiteInfo(diagnostics);
		Conversion conversion = base.Conversions.ClassifyConversionFromType(inferredType.Type, typeWithAnnotations.Type, base.CheckOverflowAtRuntime, ref useSiteInfo, forCast: true);
		bool flag3 = conversion.Kind != ConversionKind.Identity;
		if (flag3)
		{
			RefKind refKind = IterationVariable.RefKind;
			bool flag4 = ((refKind == RefKind.Ref || refKind == RefKind.In) ? true : false);
			flag3 = flag4;
		}
		if (flag3)
		{
			Binder.Error(diagnostics, ErrorCode.ERR_RefAssignmentMustHaveIdentityConversion, collectionExpr.Syntax, typeWithAnnotations.Type);
			flag = true;
		}
		BoundValuePlaceholder boundValuePlaceholder = new BoundValuePlaceholder(_syntax, inferredType.Type).MakeCompilerGenerated();
		BindingDiagnosticBag instance;
		if (!conversion.IsValid)
		{
			ImmutableArray<MethodSymbol> originalUserDefinedConversions = conversion.OriginalUserDefinedConversions;
			if (originalUserDefinedConversions.Length > 1)
			{
				diagnostics.Add(ErrorCode.ERR_AmbigUDConv, forEachKeyword.GetLocation(), originalUserDefinedConversions[0], originalUserDefinedConversions[1], inferredType.Type, typeWithAnnotations);
			}
			else
			{
				SymbolDistinguisher symbolDistinguisher = new SymbolDistinguisher(base.Compilation, inferredType.Type, typeWithAnnotations.Type);
				diagnostics.Add(ErrorCode.ERR_NoExplicitConv, forEachKeyword.GetLocation(), symbolDistinguisher.First, symbolDistinguisher.Second);
			}
			flag = true;
			instance = BindingDiagnosticBag.GetInstance(withDiagnostics: false, withDependencies: false);
		}
		else
		{
			instance = BindingDiagnosticBag.GetInstance(diagnostics);
		}
		BoundExpression elementConversion = CreateConversion(_syntax, boundValuePlaceholder, conversion, isCast: false, null, typeWithAnnotations.Type, instance);
		if (instance.AccumulatesDiagnostics && !instance.DiagnosticBag.IsEmptyWithoutResolution)
		{
			diagnostics.AddDependencies(instance);
			Location location = _syntax.ForEachKeyword.GetLocation();
			foreach (Diagnostic item in instance.DiagnosticBag.AsEnumerableWithoutResolution())
			{
				diagnostics.Add(item.WithLocation(location));
			}
		}
		else
		{
			diagnostics.AddRange(instance);
		}
		instance.Free();
		Conversion collectionConversionClassification = base.Conversions.ClassifyConversionFromExpression(collectionExpr, builder.CollectionType, base.CheckOverflowAtRuntime, ref useSiteInfo);
		Conversion conversion2 = base.Conversions.ClassifyConversionFromType(builder.CurrentPropertyGetter.ReturnType, builder.ElementType, base.CheckOverflowAtRuntime, ref useSiteInfo);
		TypeSymbol returnType = methodSymbol.ReturnType;
		if (builder.InlineArraySpanType == WellKnownType.Unknown && returnType.IsRestrictedType() && (IsDirectlyInIterator || IsInAsyncMethod()))
		{
			Binder.CheckFeatureAvailability(forEachKeyword, MessageID.IDS_FeatureRefUnsafeInIteratorAsync, diagnostics);
		}
		diagnostics.Add(_syntax.ForEachKeyword, useSiteInfo);
		BoundExpression expression3 = ConvertForEachCollection(collectionExpr, collectionConversionClassification, builder.CollectionType, diagnostics);
		if (conversion2.IsValid)
		{
			builder.CurrentPlaceholder = new BoundValuePlaceholder(_syntax, builder.CurrentPropertyGetter.ReturnType).MakeCompilerGenerated();
			builder.CurrentConversion = CreateConversion(_syntax, builder.CurrentPlaceholder, conversion2, isCast: false, null, builder.ElementType, diagnostics);
		}
		if (IsAsync)
		{
			builder.MoveNextAwaitableInfo = boundAwaitableInfo;
			if (builder.NeedsDisposal)
			{
				flag |= GetAwaitDisposeAsyncInfo(ref builder, diagnostics);
			}
		}
		return new BoundForEachStatement(_syntax, builder.Build(Flags), boundValuePlaceholder, elementConversion, boundTypeExpression, locals, boundExpression, expression3, deconstructionOpt, body, BreakLabel, ContinueLabel, flag);
	}

	private bool GetAwaitDisposeAsyncInfo(ref ForEachEnumeratorInfo.Builder builder, BindingDiagnosticBag diagnostics)
	{
		TypeSymbol type = ((builder.PatternDisposeInfo == null) ? GetWellKnownType(WellKnownType.System_Threading_Tasks_ValueTask, diagnostics, _syntax) : builder.PatternDisposeInfo.Method.ReturnType);
		bool hasErrors = false;
		ExpressionSyntax expression = _syntax.Expression;
		ReportBadAwaitDiagnostics(_syntax.AwaitKeyword, diagnostics, ref hasErrors);
		BoundAwaitableValuePlaceholder getAwaiterPlaceholder = new BoundAwaitableValuePlaceholder(expression, type);
		builder.DisposeAwaitableInfo = BindAwaitInfo(getAwaiterPlaceholder, expression, diagnostics, ref hasErrors);
		return hasErrors;
	}

	internal TypeWithAnnotations InferCollectionElementType(BindingDiagnosticBag diagnostics, ExpressionSyntax collectionSyntax)
	{
		BoundExpression collectionExpr = GetBinder(collectionSyntax).BindValue(collectionSyntax, diagnostics, BindValueKind.RValue);
		GetEnumeratorInfoAndInferCollectionElementType(_syntax, collectionSyntax, ref collectionExpr, IsAsync, isSpread: false, diagnostics, out var inferredType, out var _);
		return inferredType;
	}
}
