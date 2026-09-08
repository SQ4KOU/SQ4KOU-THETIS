using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal class OverloadResolutionResult<TMember> where TMember : Symbol
{
	private MemberResolutionResult<TMember> _bestResult;

	private ThreeState _bestResultState;

	internal readonly ArrayBuilder<MemberResolutionResult<TMember>> ResultsBuilder;

	private static readonly ObjectPool<OverloadResolutionResult<TMember>> s_pool = CreatePool();

	public bool Succeeded
	{
		get
		{
			EnsureBestResultLoaded();
			if (_bestResultState == ThreeState.True)
			{
				return _bestResult.Result.IsValid;
			}
			return false;
		}
	}

	public MemberResolutionResult<TMember> ValidResult
	{
		get
		{
			EnsureBestResultLoaded();
			return _bestResult;
		}
	}

	public MemberResolutionResult<TMember> BestResult
	{
		get
		{
			EnsureBestResultLoaded();
			return _bestResult;
		}
	}

	public ImmutableArray<MemberResolutionResult<TMember>> Results => ResultsBuilder.ToImmutable();

	internal bool HasAnyApplicableMember
	{
		get
		{
			foreach (MemberResolutionResult<TMember> item in ResultsBuilder)
			{
				if (item.Result.IsApplicable)
				{
					return true;
				}
			}
			return false;
		}
	}

	internal OverloadResolutionResult()
	{
		ResultsBuilder = new ArrayBuilder<MemberResolutionResult<TMember>>();
	}

	internal void Clear()
	{
		_bestResult = default(MemberResolutionResult<TMember>);
		_bestResultState = ThreeState.Unknown;
		ResultsBuilder.Clear();
	}

	private void EnsureBestResultLoaded()
	{
		if (!_bestResultState.HasValue())
		{
			_bestResultState = TryGetBestResult(ResultsBuilder, out _bestResult);
		}
	}

	internal TMember PickRepresentativeMember()
	{
		if (Succeeded)
		{
			return BestResult.Member;
		}
		TMember member = ResultsBuilder.FirstOrDefault((MemberResolutionResult<TMember> r) => r.Result.Kind == MemberResolutionKind.Worse).Member;
		if ((object)member != null)
		{
			return member;
		}
		return GetAllApplicableMembers()[0];
	}

	internal ImmutableArray<TMember> GetAllApplicableMembers()
	{
		ArrayBuilder<TMember> instance = ArrayBuilder<TMember>.GetInstance();
		foreach (MemberResolutionResult<TMember> item in ResultsBuilder)
		{
			if (item.Result.IsApplicable)
			{
				instance.Add(item.Member);
			}
		}
		return instance.ToImmutableAndFree();
	}

	private static ThreeState TryGetBestResult(ArrayBuilder<MemberResolutionResult<TMember>> allResults, out MemberResolutionResult<TMember> best)
	{
		best = default(MemberResolutionResult<TMember>);
		ThreeState threeState = ThreeState.False;
		foreach (MemberResolutionResult<TMember> allResult in allResults)
		{
			if (allResult.Result.IsValid)
			{
				if (threeState == ThreeState.True)
				{
					best = default(MemberResolutionResult<TMember>);
					return ThreeState.False;
				}
				threeState = ThreeState.True;
				best = allResult;
			}
		}
		return threeState;
	}

	internal void ReportDiagnostics<T>(Binder binder, Location location, SyntaxNode nodeOpt, BindingDiagnosticBag diagnostics, string name, BoundExpression receiver, SyntaxNode invokedExpression, AnalyzedArguments arguments, ImmutableArray<T> memberGroup, NamedTypeSymbol typeContainingConstructor, NamedTypeSymbol delegateTypeBeingInvoked, CSharpSyntaxNode queryClause = null, bool isMethodGroupConversion = false, RefKind? returnRefKind = null, TypeSymbol delegateOrFunctionPointerType = null, bool isParamsModifierValidation = false, bool isExtension = false) where T : Symbol
	{
		ImmutableArray<Symbol> symbols = StaticCast<Symbol>.From(memberGroup);
		if (HadAmbiguousBestMethods(binder.Compilation, diagnostics, symbols, location, isExtension) || HadAmbiguousWorseMethods(binder.Compilation, diagnostics, symbols, location, queryClause != null, receiver, name, isExtension) || HadLambdaConversionError(diagnostics, arguments) || HadStaticInstanceMismatch(diagnostics, symbols, invokedExpression?.GetLocation() ?? location, binder, receiver, nodeOpt, delegateOrFunctionPointerType) || (isMethodGroupConversion && returnRefKind.HasValue && HadReturnMismatch(location, diagnostics, delegateOrFunctionPointerType)) || HadConstraintFailure(location, diagnostics) || HadBadArguments(diagnostics, binder, name, receiver, arguments, symbols, location, binder.Flags, isMethodGroupConversion) || HadConstructedParameterFailedConstraintCheck(binder.Conversions, binder.Compilation, diagnostics, location) || InaccessibleTypeArgument(diagnostics, symbols, location) || TypeInferenceFailed(binder, diagnostics, symbols, receiver, arguments, location, queryClause) || UseSiteError())
		{
			return;
		}
		bool flag = false;
		MemberResolutionResult<TMember> memberResolutionResult = default(MemberResolutionResult<TMember>);
		MemberResolutionResult<TMember> firstUnsupported = default(MemberResolutionResult<TMember>);
		MemberResolutionResult<TMember>[] array = new MemberResolutionResult<TMember>[7];
		foreach (MemberResolutionResult<TMember> item2 in ResultsBuilder)
		{
			switch (item2.Result.Kind)
			{
			case MemberResolutionKind.UnsupportedMetadata:
				if (memberResolutionResult.IsNull)
				{
					firstUnsupported = item2;
				}
				break;
			case MemberResolutionKind.NoCorrespondingNamedParameter:
				if (array[3].IsNull || item2.Result.FirstBadArgument > array[3].Result.FirstBadArgument)
				{
					array[3] = item2;
				}
				break;
			case MemberResolutionKind.NoCorrespondingParameter:
				if (array[4].IsNull)
				{
					array[4] = item2;
				}
				break;
			case MemberResolutionKind.RequiredParameterMissing:
				if (array[1].IsNull)
				{
					array[1] = item2;
				}
				else
				{
					flag = true;
				}
				break;
			case MemberResolutionKind.NameUsedForPositional:
				if (array[2].IsNull || item2.Result.FirstBadArgument > array[2].Result.FirstBadArgument)
				{
					array[2] = item2;
				}
				break;
			case MemberResolutionKind.BadNonTrailingNamedArgument:
				if (array[5].IsNull || item2.Result.FirstBadArgument > array[5].Result.FirstBadArgument)
				{
					array[5] = item2;
				}
				break;
			case MemberResolutionKind.DuplicateNamedArgument:
				if (array[0].IsNull || item2.Result.FirstBadArgument > array[0].Result.FirstBadArgument)
				{
					array[0] = item2;
				}
				break;
			case MemberResolutionKind.WrongCallingConvention:
				if (array[6].IsNull)
				{
					array[6] = item2;
				}
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(item2.Result.Kind);
			}
		}
		MemberResolutionResult<TMember>[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			MemberResolutionResult<TMember> memberResolutionResult2 = array2[i];
			if (memberResolutionResult2.IsNotNull)
			{
				memberResolutionResult = memberResolutionResult2;
				break;
			}
		}
		if (memberResolutionResult.IsNotNull)
		{
			if (memberResolutionResult.Member is FunctionPointerMethodSymbol && memberResolutionResult.Result.Kind == MemberResolutionKind.NoCorrespondingNamedParameter)
			{
				int firstBadArgument = memberResolutionResult.Result.FirstBadArgument;
				Location item = arguments.Names[firstBadArgument].GetValueOrDefault().Location;
				diagnostics.Add(ErrorCode.ERR_FunctionPointersCannotBeCalledWithNamedArguments, item);
				return;
			}
			if (!((memberResolutionResult.Result.Kind == MemberResolutionKind.RequiredParameterMissing) & flag) && !isMethodGroupConversion && !(memberResolutionResult.Member is FunctionPointerMethodSymbol))
			{
				switch (memberResolutionResult.Result.Kind)
				{
				case MemberResolutionKind.NameUsedForPositional:
					ReportNameUsedForPositional(memberResolutionResult, diagnostics, arguments, symbols);
					return;
				case MemberResolutionKind.NoCorrespondingNamedParameter:
					ReportNoCorrespondingNamedParameter(memberResolutionResult, name, diagnostics, arguments, delegateTypeBeingInvoked, symbols);
					return;
				case MemberResolutionKind.RequiredParameterMissing:
					if (((uint)binder.Flags & 0x80000000u) != 0)
					{
						if (receiver == null)
						{
							diagnostics.Add(isParamsModifierValidation ? ErrorCode.ERR_ParamsCollectionMissingConstructor : ErrorCode.ERR_CollectionExpressionMissingConstructor, location);
							return;
						}
						diagnostics.Add(ErrorCode.ERR_CollectionExpressionMissingAdd, location, receiver.Type);
					}
					else
					{
						ReportMissingRequiredParameter(memberResolutionResult, diagnostics, delegateTypeBeingInvoked, symbols, location);
					}
					return;
				case MemberResolutionKind.BadNonTrailingNamedArgument:
					ReportBadNonTrailingNamedArgument(memberResolutionResult, diagnostics, arguments, symbols);
					return;
				case MemberResolutionKind.DuplicateNamedArgument:
					ReportDuplicateNamedArgument(memberResolutionResult, diagnostics, arguments);
					return;
				}
			}
			else if (memberResolutionResult.Result.Kind == MemberResolutionKind.WrongCallingConvention)
			{
				ReportWrongCallingConvention(location, diagnostics, symbols, memberResolutionResult, ((FunctionPointerTypeSymbol)delegateOrFunctionPointerType).Signature);
				return;
			}
		}
		else if (firstUnsupported.IsNotNull)
		{
			ReportUnsupportedMetadata(location, diagnostics, symbols, firstUnsupported);
			return;
		}
		if (!isMethodGroupConversion)
		{
			ReportBadParameterCount(diagnostics, name, arguments, symbols, location, typeContainingConstructor, delegateTypeBeingInvoked);
		}
	}

	private static void ReportUnsupportedMetadata(Location location, BindingDiagnosticBag diagnostics, ImmutableArray<Symbol> symbols, MemberResolutionResult<TMember> firstUnsupported)
	{
		DiagnosticInfo diagnosticInfo = firstUnsupported.Member.GetUseSiteInfo().DiagnosticInfo;
		diagnosticInfo = new DiagnosticInfoWithSymbols((ErrorCode)diagnosticInfo.Code, diagnosticInfo.Arguments, symbols);
		Symbol.ReportUseSiteDiagnostic(diagnosticInfo, diagnostics, location);
	}

	private static void ReportWrongCallingConvention(Location location, BindingDiagnosticBag diagnostics, ImmutableArray<Symbol> symbols, MemberResolutionResult<TMember> firstSupported, MethodSymbol target)
	{
		diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_WrongFuncPtrCallingConvention, new object[2] { firstSupported.Member, target.CallingConvention }, symbols), location);
	}

	private bool UseSiteError()
	{
		if (GetFirstMemberKind(MemberResolutionKind.UseSiteError).IsNull)
		{
			return false;
		}
		return true;
	}

	private bool InaccessibleTypeArgument(BindingDiagnosticBag diagnostics, ImmutableArray<Symbol> symbols, Location location)
	{
		MemberResolutionResult<TMember> firstMemberKind = GetFirstMemberKind(MemberResolutionKind.InaccessibleTypeArgument);
		if (firstMemberKind.IsNull)
		{
			return false;
		}
		diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_BadAccess, new object[1] { firstMemberKind.Member }, symbols), location);
		return true;
	}

	private bool HadStaticInstanceMismatch(BindingDiagnosticBag diagnostics, ImmutableArray<Symbol> symbols, Location location, Binder binder, BoundExpression receiverOpt, SyntaxNode nodeOpt, TypeSymbol delegateOrFunctionPointerType)
	{
		MemberResolutionResult<TMember> firstMemberKind = GetFirstMemberKind(MemberResolutionKind.StaticInstanceMismatch);
		if (firstMemberKind.IsNull)
		{
			return false;
		}
		if (receiverOpt == null || !receiverOpt.HasErrors)
		{
			Symbol member = firstMemberKind.Member;
			if (receiverOpt != null && receiverOpt.Kind == BoundKind.QueryClause)
			{
				diagnostics.Add(ErrorCode.ERR_QueryNoProvider, location, receiverOpt.Type, member.Name);
			}
			else if (binder.Flags.Includes(BinderFlags.CollectionInitializerAddMethod))
			{
				diagnostics.Add(ErrorCode.ERR_InitializerAddHasWrongSignature, location, member);
			}
			else if (nodeOpt != null && nodeOpt.Kind() == SyntaxKind.AwaitExpression && member.Name == "GetAwaiter")
			{
				diagnostics.Add(ErrorCode.ERR_BadAwaitArg, location, receiverOpt.Type);
			}
			else if (delegateOrFunctionPointerType is FunctionPointerTypeSymbol)
			{
				diagnostics.Add(ErrorCode.ERR_FuncPtrMethMustBeStatic, location, member);
			}
			else
			{
				ErrorCode errorCode = ((!member.RequiresInstanceReceiver()) ? ErrorCode.ERR_ObjectProhibited : ((Binder.WasImplicitReceiver(receiverOpt) && binder.InFieldInitializer && !binder.BindingTopLevelScriptCode) ? ErrorCode.ERR_FieldInitRefNonstatic : ErrorCode.ERR_ObjectRequired));
				diagnostics.Add(new DiagnosticInfoWithSymbols(errorCode, new object[1] { member }, symbols), location);
			}
		}
		return true;
	}

	private bool HadReturnMismatch(Location location, BindingDiagnosticBag diagnostics, TypeSymbol delegateOrFunctionPointerType)
	{
		MemberResolutionResult<TMember> firstMemberKind = GetFirstMemberKind(MemberResolutionKind.WrongRefKind);
		if (!firstMemberKind.IsNull)
		{
			diagnostics.Add(delegateOrFunctionPointerType.IsFunctionPointer() ? ErrorCode.ERR_FuncPtrRefMismatch : ErrorCode.ERR_DelegateRefMismatch, location, firstMemberKind.Member, delegateOrFunctionPointerType);
			return true;
		}
		firstMemberKind = GetFirstMemberKind(MemberResolutionKind.WrongReturnType);
		if (!firstMemberKind.IsNull)
		{
			MethodSymbol methodSymbol = (MethodSymbol)(object)firstMemberKind.Member;
			diagnostics.Add(ErrorCode.ERR_BadRetType, location, methodSymbol, methodSymbol.ReturnType);
			return true;
		}
		return false;
	}

	private bool HadConstraintFailure(Location location, BindingDiagnosticBag diagnostics)
	{
		MemberResolutionResult<TMember> firstMemberKind = GetFirstMemberKind(MemberResolutionKind.ConstraintFailure);
		if (firstMemberKind.IsNull)
		{
			return false;
		}
		foreach (TypeParameterDiagnosticInfo constraintFailureDiagnostic in firstMemberKind.Result.ConstraintFailureDiagnostics)
		{
			if (constraintFailureDiagnostic.UseSiteInfo.DiagnosticInfo != null)
			{
				diagnostics.Add(new CSDiagnostic(constraintFailureDiagnostic.UseSiteInfo.DiagnosticInfo, location));
			}
		}
		return true;
	}

	private bool TypeInferenceFailed(Binder binder, BindingDiagnosticBag diagnostics, ImmutableArray<Symbol> symbols, BoundExpression receiver, AnalyzedArguments arguments, Location location, CSharpSyntaxNode queryClause = null)
	{
		MemberResolutionResult<TMember> firstMemberKind = GetFirstMemberKind(MemberResolutionKind.TypeInferenceFailed);
		if (firstMemberKind.IsNotNull)
		{
			if (queryClause != null)
			{
				Binder.ReportQueryInferenceFailed(queryClause, firstMemberKind.Member.Name, receiver, arguments, symbols, diagnostics);
			}
			else
			{
				diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_CantInferMethTypeArgs, new object[1] { firstMemberKind.Member }, symbols), location);
			}
			return true;
		}
		firstMemberKind = GetFirstMemberKind(MemberResolutionKind.TypeInferenceExtensionInstanceArgument);
		if (firstMemberKind.IsNotNull)
		{
			BoundExpression boundExpression = arguments.Arguments[0];
			if (queryClause != null)
			{
				binder.ReportQueryLookupFailed(queryClause, boundExpression, firstMemberKind.Member.Name, symbols, diagnostics);
			}
			else if (firstMemberKind.Member.Kind == SymbolKind.Method)
			{
				diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_CantInferMethTypeArgs, new object[1] { firstMemberKind.Member }, symbols), location);
			}
			else
			{
				diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_NoSuchMemberOrExtension, new object[2]
				{
					boundExpression.Type,
					firstMemberKind.Member.Name
				}, symbols), location);
			}
			return true;
		}
		return false;
	}

	private static void ReportNameUsedForPositional(MemberResolutionResult<TMember> bad, BindingDiagnosticBag diagnostics, AnalyzedArguments arguments, ImmutableArray<Symbol> symbols)
	{
		int firstBadArgument = bad.Result.FirstBadArgument;
		var (text, location) = arguments.Names[firstBadArgument].GetValueOrDefault();
		diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_NamedArgumentUsedInPositional, new object[1] { text }, symbols), location);
	}

	private static void ReportBadNonTrailingNamedArgument(MemberResolutionResult<TMember> bad, BindingDiagnosticBag diagnostics, AnalyzedArguments arguments, ImmutableArray<Symbol> symbols)
	{
		int firstBadArgument = bad.Result.FirstBadArgument;
		var (text, location) = arguments.Names[firstBadArgument].GetValueOrDefault();
		diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_BadNonTrailingNamedArgument, new object[1] { text }, symbols), location);
	}

	private static void ReportDuplicateNamedArgument(MemberResolutionResult<TMember> result, BindingDiagnosticBag diagnostics, AnalyzedArguments arguments)
	{
		var (text, location) = arguments.Names[result.Result.FirstBadArgument].GetValueOrDefault();
		diagnostics.Add(new CSDiagnosticInfo(ErrorCode.ERR_DuplicateNamedArgument, text), location);
	}

	private static void ReportNoCorrespondingNamedParameter(MemberResolutionResult<TMember> bad, string methodName, BindingDiagnosticBag diagnostics, AnalyzedArguments arguments, NamedTypeSymbol delegateTypeBeingInvoked, ImmutableArray<Symbol> symbols)
	{
		int firstBadArgument = bad.Result.FirstBadArgument;
		(string Name, Location Location) valueOrDefault = arguments.Names[firstBadArgument].GetValueOrDefault();
		string item = valueOrDefault.Name;
		Location item2 = valueOrDefault.Location;
		ErrorCode errorCode = (((object)delegateTypeBeingInvoked != null) ? ErrorCode.ERR_BadNamedArgumentForDelegateInvoke : ErrorCode.ERR_BadNamedArgument);
		object obj = ((object)delegateTypeBeingInvoked) ?? ((object)methodName);
		diagnostics.Add(new DiagnosticInfoWithSymbols(errorCode, new object[2] { obj, item }, symbols), item2);
	}

	private static void ReportMissingRequiredParameter(MemberResolutionResult<TMember> bad, BindingDiagnosticBag diagnostics, NamedTypeSymbol delegateTypeBeingInvoked, ImmutableArray<Symbol> symbols, Location location)
	{
		TMember member = bad.Member;
		ImmutableArray<ParameterSymbol> parametersIncludingExtensionParameter = member.GetParametersIncludingExtensionParameter(skipExtensionIfStatic: false);
		int badParameter = bad.Result.BadParameter;
		string text = ((badParameter != parametersIncludingExtensionParameter.Length) ? parametersIncludingExtensionParameter[badParameter].Name : SyntaxFacts.GetText(SyntaxKind.ArgListKeyword));
		object obj = ((object)delegateTypeBeingInvoked) ?? ((object)member);
		diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_NoCorrespondingArgument, new object[2] { text, obj }, symbols), location);
	}

	private static void ReportBadParameterCount(BindingDiagnosticBag diagnostics, string name, AnalyzedArguments arguments, ImmutableArray<Symbol> symbols, Location location, NamedTypeSymbol typeContainingConstructor, NamedTypeSymbol delegateTypeBeingInvoked)
	{
		FunctionPointerMethodSymbol functionPointerMethodSymbol = ((symbols.IsDefault || symbols.Length != 1) ? null : (symbols[0] as FunctionPointerMethodSymbol));
		(ErrorCode, object) tuple;
		if ((object)typeContainingConstructor == null)
		{
			if ((object)delegateTypeBeingInvoked == null)
			{
				object obj = functionPointerMethodSymbol;
				tuple = ((obj == null) ? (ErrorCode.ERR_BadArgCount, name) : (ErrorCode.ERR_BadFuncPointerArgCount, obj));
			}
			else
			{
				tuple = (ErrorCode.ERR_BadDelArgCount, delegateTypeBeingInvoked);
			}
		}
		else
		{
			tuple = (ErrorCode.ERR_BadCtorArgCount, typeContainingConstructor);
		}
		(ErrorCode, object) tuple2 = tuple;
		ErrorCode item = tuple2.Item1;
		object item2 = tuple2.Item2;
		int num = arguments.Arguments.Count;
		if (arguments.IncludesReceiverAsArgument)
		{
			num--;
		}
		diagnostics.Add(new DiagnosticInfoWithSymbols(item, new object[2] { item2, num }, symbols), location);
	}

	private bool HadConstructedParameterFailedConstraintCheck(ConversionsBase conversions, CSharpCompilation compilation, BindingDiagnosticBag diagnostics, Location location)
	{
		MemberResolutionResult<TMember> firstMemberKind = GetFirstMemberKind(MemberResolutionKind.ConstructedParameterFailedConstraintCheck);
		if (firstMemberKind.IsNull)
		{
			return false;
		}
		MethodSymbol methodSymbol = (MethodSymbol)(object)firstMemberKind.Member;
		if (!methodSymbol.CheckConstraints(new ConstraintsHelper.CheckConstraintsArgs(compilation, conversions, includeNullability: false, location, diagnostics)))
		{
			return true;
		}
		TypeSymbol parameterType = methodSymbol.GetParameterType(firstMemberKind.Result.BadParameter);
		ConstraintsHelper.CheckConstraintsArgsBoxed checkConstraintsArgsBoxed = ConstraintsHelper.CheckConstraintsArgsBoxed.Allocate(compilation, conversions, includeNullability: false, location, diagnostics);
		parameterType.CheckAllConstraints(checkConstraintsArgsBoxed);
		checkConstraintsArgsBoxed.Free();
		return true;
	}

	private static bool HadLambdaConversionError(BindingDiagnosticBag diagnostics, AnalyzedArguments arguments)
	{
		bool flag = false;
		foreach (BoundExpression argument in arguments.Arguments)
		{
			if (argument.Kind == BoundKind.UnboundLambda)
			{
				flag |= ((UnboundLambda)argument).GenerateSummaryErrors(diagnostics);
			}
		}
		return flag;
	}

	private bool HadBadArguments(BindingDiagnosticBag diagnostics, Binder binder, string name, BoundExpression receiver, AnalyzedArguments arguments, ImmutableArray<Symbol> symbols, Location location, BinderFlags flags, bool isMethodGroupConversion)
	{
		MemberResolutionResult<TMember> firstMemberKind = GetFirstMemberKind(MemberResolutionKind.BadArgumentConversion);
		if (firstMemberKind.IsNull)
		{
			return false;
		}
		if (isMethodGroupConversion)
		{
			return true;
		}
		TMember member = firstMemberKind.Member;
		if (flags.Includes(BinderFlags.CollectionInitializerAddMethod))
		{
			bool num = arguments.IncludesReceiverAsArgument;
			ImmutableArray<ParameterSymbol> parametersIncludingExtensionParameter = member.GetParametersIncludingExtensionParameter(skipExtensionIfStatic: false);
			for (int i = (num ? 1 : 0); i < parametersIncludingExtensionParameter.Length; i++)
			{
				if (parametersIncludingExtensionParameter[i].RefKind != RefKind.None)
				{
					diagnostics.Add(ErrorCode.ERR_InitializerAddHasParamModifiers, location, symbols, member);
					return true;
				}
			}
			if (flags.Includes(BinderFlags.CollectionExpressionConversionValidation))
			{
				diagnostics.Add(ErrorCode.ERR_CollectionExpressionMissingAdd, location, receiver.Type);
			}
			else
			{
				diagnostics.Add(ErrorCode.ERR_BadArgTypesForCollectionAdd, location, symbols, member);
			}
		}
		foreach (int item in firstMemberKind.Result.BadArgumentsOpt.TrueBits())
		{
			ReportBadArgumentError(diagnostics, binder, name, arguments, symbols, firstMemberKind, member, item);
		}
		return true;
	}

	private static void ReportBadArgumentError(BindingDiagnosticBag diagnostics, Binder binder, string name, AnalyzedArguments arguments, ImmutableArray<Symbol> symbols, MemberResolutionResult<TMember> badArg, TMember method, int arg)
	{
		BoundExpression boundExpression = arguments.Argument(arg);
		if (boundExpression.HasAnyErrors)
		{
			return;
		}
		int num = badArg.Result.ParameterFromArgument(arg);
		SourceLocation location = new SourceLocation(boundExpression.Syntax);
		ImmutableArray<ParameterSymbol> parametersIncludingExtensionParameter = method.GetParametersIncludingExtensionParameter(skipExtensionIfStatic: false);
		if (method.GetIsVararg() && num == parametersIncludingExtensionParameter.Length)
		{
			diagnostics.Add(ErrorCode.ERR_BadArgType, location, symbols, arg + 1, boundExpression.Display, "__arglist");
			return;
		}
		ParameterSymbol parameterSymbol = parametersIncludingExtensionParameter[num];
		bool isLastParameter = parametersIncludingExtensionParameter.Length == num + 1;
		RefKind refKind = arguments.RefKind(arg);
		RefKind refKind2 = parameterSymbol.RefKind;
		if (arguments.IsExtensionMethodReceiverArgument(arg) && (refKind2 == RefKind.Ref || refKind2 == RefKind.In))
		{
			refKind = refKind2;
		}
		if (!boundExpression.HasExpressionType() && boundExpression.Kind != BoundKind.OutDeconstructVarPendingInference && boundExpression.Kind != BoundKind.OutVariablePendingInference && boundExpression.Kind != BoundKind.DiscardExpression)
		{
			TypeSymbol typeSymbol = ((unwrapIfParamsCollection(badArg, parameterSymbol, isLastParameter) is TypeSymbol typeSymbol2) ? typeSymbol2 : parameterSymbol.Type);
			if (boundExpression.Kind == BoundKind.UnboundLambda && refKind == refKind2)
			{
				((UnboundLambda)boundExpression).GenerateAnonymousFunctionConversionError(diagnostics, typeSymbol);
			}
			else
			{
				if (boundExpression.Kind == BoundKind.MethodGroup && typeSymbol.TypeKind == TypeKind.Delegate && Conversions.ReportDelegateOrFunctionPointerMethodGroupDiagnostics(binder, (BoundMethodGroup)boundExpression, typeSymbol, diagnostics))
				{
					return;
				}
				if (boundExpression.Kind == BoundKind.MethodGroup && typeSymbol.TypeKind == TypeKind.FunctionPointer)
				{
					diagnostics.Add(ErrorCode.ERR_MissingAddressOf, location);
				}
				else if (boundExpression.Kind != BoundKind.UnconvertedAddressOfOperator || !Conversions.ReportDelegateOrFunctionPointerMethodGroupDiagnostics(binder, ((BoundUnconvertedAddressOfOperator)boundExpression).Operand, typeSymbol, diagnostics))
				{
					if (boundExpression.Kind == BoundKind.UnconvertedCollectionExpression)
					{
						binder.GenerateImplicitConversionErrorForCollectionExpression((BoundUnconvertedCollectionExpression)boundExpression, typeSymbol, diagnostics);
						return;
					}
					diagnostics.Add(ErrorCode.ERR_BadArgType, location, symbols, arg + 1, boundExpression.Display, new FormattedSymbol(unwrapIfParamsCollection(badArg, parameterSymbol, isLastParameter), SymbolDisplayFormat.CSharpErrorMessageNoParameterNamesFormat));
				}
			}
			return;
		}
		bool flag = refKind != refKind2 && (refKind != RefKind.None || refKind2 != RefKind.In) && (refKind != RefKind.Ref || refKind2 != RefKind.In || !binder.Compilation.IsFeatureEnabled(MessageID.IDS_FeatureRefReadonlyParameters));
		if (flag)
		{
			bool flag2 = refKind2 == RefKind.RefReadOnlyParameter;
			if (flag2)
			{
				bool flag3 = ((refKind <= RefKind.Ref || refKind == RefKind.In) ? true : false);
				flag2 = flag3;
			}
			flag = !flag2;
		}
		if (flag)
		{
			if (isStringLiteralToInterpolatedStringHandlerArgumentConversion(boundExpression, parameterSymbol) && refKind2 != RefKind.Out)
			{
				diagnostics.Add(ErrorCode.ERR_ExpectedInterpolatedString, location);
			}
			else if (refKind == RefKind.Ref && refKind2 == RefKind.In && !binder.Compilation.IsFeatureEnabled(MessageID.IDS_FeatureRefReadonlyParameters))
			{
				diagnostics.Add(ErrorCode.ERR_BadArgExtraRefLangVersion, location, symbols, arg + 1, binder.Compilation.LanguageVersion.ToDisplayString(), new CSharpRequiredLanguageVersion(MessageID.IDS_FeatureRefReadonlyParameters.RequiredVersion()));
			}
			else if ((refKind2 == RefKind.None || refKind2 - 3 <= RefKind.Ref) ? true : false)
			{
				diagnostics.Add(ErrorCode.ERR_BadArgExtraRef, location, symbols, arg + 1, refKind.ToArgumentDisplayString());
			}
			else
			{
				diagnostics.Add(ErrorCode.ERR_BadArgRef, location, symbols, arg + 1, refKind2.ToParameterDisplayString());
			}
		}
		else if (arguments.IsExtensionMethodReceiverArgument(arg))
		{
			diagnostics.Add(ErrorCode.ERR_BadInstanceArgType, location, symbols, boundExpression.Display, name, method, new FormattedSymbol(parameterSymbol, SymbolDisplayFormat.CSharpErrorMessageNoParameterNamesFormat));
		}
		else if (boundExpression.Display is TypeSymbol typeSymbol3)
		{
			if (isStringLiteralToInterpolatedStringHandlerArgumentConversion(boundExpression, parameterSymbol))
			{
				diagnostics.Add(ErrorCode.ERR_ExpectedInterpolatedString, location);
				return;
			}
			SignatureOnlyParameterSymbol symbol = new SignatureOnlyParameterSymbol(TypeWithAnnotations.Create(typeSymbol3), ImmutableArray<CustomModifier>.Empty, isParamsArray: false, isParamsCollection: false, refKind);
			SymbolDistinguisher symbolDistinguisher = new SymbolDistinguisher(binder.Compilation, symbol, unwrapIfParamsCollection(badArg, parameterSymbol, isLastParameter));
			diagnostics.Add(ErrorCode.ERR_BadArgType, location, symbols, arg + 1, symbolDistinguisher.First, symbolDistinguisher.Second);
		}
		else
		{
			diagnostics.Add(ErrorCode.ERR_BadArgType, location, symbols, arg + 1, boundExpression.Display, new FormattedSymbol(unwrapIfParamsCollection(badArg, parameterSymbol, isLastParameter), SymbolDisplayFormat.CSharpErrorMessageNoParameterNamesFormat));
		}
		static bool isStringLiteralToInterpolatedStringHandlerArgumentConversion(BoundExpression argument, ParameterSymbol parameter)
		{
			if (argument is BoundLiteral)
			{
				TypeSymbol type = argument.Type;
				if ((object)type != null && type.SpecialType == SpecialType.System_String)
				{
					if (parameter.Type is NamedTypeSymbol namedTypeSymbol)
					{
						return namedTypeSymbol.IsInterpolatedStringHandlerType;
					}
					return false;
				}
			}
			return false;
		}
		static Symbol unwrapIfParamsCollection(MemberResolutionResult<TMember> memberResolutionResult, ParameterSymbol parameter, bool flag4)
		{
			if (flag4 && memberResolutionResult.Result.ParamsElementTypeOpt.HasType)
			{
				return memberResolutionResult.Result.ParamsElementTypeOpt.Type;
			}
			return parameter;
		}
	}

	private bool HadAmbiguousWorseMethods(CSharpCompilation compilation, BindingDiagnosticBag diagnostics, ImmutableArray<Symbol> symbols, Location location, bool isQuery, BoundExpression receiver, string name, bool isExtension)
	{
		if (TryGetFirstTwoWorseResults(out var first, out var second) <= 1)
		{
			return false;
		}
		if (isQuery)
		{
			diagnostics.Add(ErrorCode.ERR_QueryMultipleProviders, location, receiver.Type, name);
		}
		else
		{
			diagnostics.Add(CreateAmbiguousCallDiagnosticInfo(compilation, first.LeastOverriddenMember.ConstructedFrom(), second.LeastOverriddenMember.ConstructedFrom(), symbols, isExtension), location);
		}
		return true;
	}

	private int TryGetFirstTwoWorseResults(out MemberResolutionResult<TMember> first, out MemberResolutionResult<TMember> second)
	{
		int num = 0;
		bool flag = false;
		bool flag2 = false;
		first = default(MemberResolutionResult<TMember>);
		second = default(MemberResolutionResult<TMember>);
		foreach (MemberResolutionResult<TMember> item in ResultsBuilder)
		{
			if (item.Result.Kind == MemberResolutionKind.Worse)
			{
				num++;
				if (!flag)
				{
					first = item;
					flag = true;
				}
				else if (!flag2)
				{
					second = item;
					flag2 = true;
				}
			}
		}
		return num;
	}

	private bool HadAmbiguousBestMethods(CSharpCompilation compilation, BindingDiagnosticBag diagnostics, ImmutableArray<Symbol> symbols, Location location, bool isExtension)
	{
		if (TryGetFirstTwoValidResults(out var first, out var second) <= 1)
		{
			return false;
		}
		diagnostics.Add(CreateAmbiguousCallDiagnosticInfo(compilation, first.LeastOverriddenMember.ConstructedFrom(), second.LeastOverriddenMember.ConstructedFrom(), symbols, isExtension), location);
		return true;
	}

	private int TryGetFirstTwoValidResults(out MemberResolutionResult<TMember> first, out MemberResolutionResult<TMember> second)
	{
		int num = 0;
		bool flag = false;
		bool flag2 = false;
		first = default(MemberResolutionResult<TMember>);
		second = default(MemberResolutionResult<TMember>);
		foreach (MemberResolutionResult<TMember> item in ResultsBuilder)
		{
			if (item.Result.IsValid)
			{
				num++;
				if (!flag)
				{
					first = item;
					flag = true;
				}
				else if (!flag2)
				{
					second = item;
					flag2 = true;
				}
			}
		}
		return num;
	}

	internal static DiagnosticInfoWithSymbols CreateAmbiguousCallDiagnosticInfo(CSharpCompilation compilation, Symbol first, Symbol second, ImmutableArray<Symbol> symbols, bool isExtension)
	{
		SymbolDistinguisher symbolDistinguisher = new SymbolDistinguisher(compilation, first, second);
		return new DiagnosticInfoWithSymbols(isExtension ? ErrorCode.ERR_AmbigExtension : ErrorCode.ERR_AmbigCall, new object[2] { symbolDistinguisher.First, symbolDistinguisher.Second }, symbols);
	}

	[Conditional("DEBUG")]
	private void AssertNone(MemberResolutionKind kind)
	{
		foreach (MemberResolutionResult<TMember> item in ResultsBuilder)
		{
			if (item.Result.Kind == kind)
			{
				throw ExceptionUtilities.UnexpectedValue(kind);
			}
		}
	}

	private MemberResolutionResult<TMember> GetFirstMemberKind(MemberResolutionKind kind)
	{
		foreach (MemberResolutionResult<TMember> item in ResultsBuilder)
		{
			if (item.Result.Kind == kind)
			{
				return item;
			}
		}
		return default(MemberResolutionResult<TMember>);
	}

	internal static OverloadResolutionResult<TMember> GetInstance()
	{
		return s_pool.Allocate();
	}

	internal void Free()
	{
		Clear();
		s_pool.Free(this);
	}

	private static ObjectPool<OverloadResolutionResult<TMember>> CreatePool()
	{
		return new ObjectPool<OverloadResolutionResult<TMember>>(() => new OverloadResolutionResult<TMember>(), 10);
	}
}
