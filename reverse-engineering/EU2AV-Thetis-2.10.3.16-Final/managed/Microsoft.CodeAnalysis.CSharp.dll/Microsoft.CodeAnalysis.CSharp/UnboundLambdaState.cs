using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class UnboundLambdaState
{
	private sealed class ReturnInferenceCacheKey
	{
		public readonly ImmutableArray<TypeWithAnnotations> ParameterTypes;

		public readonly ImmutableArray<RefKind> ParameterRefKinds;

		public readonly NamedTypeSymbol? TaskLikeReturnTypeOpt;

		public static readonly ReturnInferenceCacheKey Empty = new ReturnInferenceCacheKey(ImmutableArray<TypeWithAnnotations>.Empty, ImmutableArray<Microsoft.CodeAnalysis.RefKind>.Empty, null);

		private ReturnInferenceCacheKey(ImmutableArray<TypeWithAnnotations> parameterTypes, ImmutableArray<RefKind> parameterRefKinds, NamedTypeSymbol? taskLikeReturnTypeOpt)
		{
			ParameterTypes = parameterTypes;
			ParameterRefKinds = parameterRefKinds;
			TaskLikeReturnTypeOpt = taskLikeReturnTypeOpt;
		}

		public override bool Equals(object? obj)
		{
			if (this == obj)
			{
				return true;
			}
			if (!(obj is ReturnInferenceCacheKey returnInferenceCacheKey) || returnInferenceCacheKey.ParameterTypes.Length != ParameterTypes.Length || !TypeSymbol.Equals(returnInferenceCacheKey.TaskLikeReturnTypeOpt, TaskLikeReturnTypeOpt, TypeCompareKind.ConsiderEverything))
			{
				return false;
			}
			for (int i = 0; i < ParameterTypes.Length; i++)
			{
				if (!returnInferenceCacheKey.ParameterTypes[i].Equals(ParameterTypes[i], TypeCompareKind.ConsiderEverything) || returnInferenceCacheKey.ParameterRefKinds[i] != ParameterRefKinds[i])
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			int num = TaskLikeReturnTypeOpt?.GetHashCode() ?? 0;
			foreach (TypeWithAnnotations parameterType in ParameterTypes)
			{
				num = Hash.Combine(parameterType.Type, num);
			}
			return num;
		}

		public static ReturnInferenceCacheKey Create(NamedTypeSymbol? delegateType, bool isAsync)
		{
			GetFields(delegateType, isAsync, out ImmutableArray<TypeWithAnnotations> parameterTypes, out ImmutableArray<RefKind> parameterRefKinds, out NamedTypeSymbol taskLikeReturnTypeOpt);
			if (parameterTypes.IsEmpty && parameterRefKinds.IsEmpty && (object)taskLikeReturnTypeOpt == null)
			{
				return Empty;
			}
			return new ReturnInferenceCacheKey(parameterTypes, parameterRefKinds, taskLikeReturnTypeOpt);
		}

		public static void GetFields(NamedTypeSymbol? delegateType, bool isAsync, out ImmutableArray<TypeWithAnnotations> parameterTypes, out ImmutableArray<RefKind> parameterRefKinds, out NamedTypeSymbol? taskLikeReturnTypeOpt)
		{
			parameterTypes = ImmutableArray<TypeWithAnnotations>.Empty;
			parameterRefKinds = ImmutableArray<Microsoft.CodeAnalysis.RefKind>.Empty;
			taskLikeReturnTypeOpt = null;
			MethodSymbol methodSymbol = DelegateInvokeMethod(delegateType);
			if ((object)methodSymbol == null)
			{
				return;
			}
			int parameterCount = methodSymbol.ParameterCount;
			if (parameterCount > 0)
			{
				ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance(parameterCount);
				ArrayBuilder<RefKind> instance2 = ArrayBuilder<Microsoft.CodeAnalysis.RefKind>.GetInstance(parameterCount);
				foreach (ParameterSymbol parameter in methodSymbol.Parameters)
				{
					instance2.Add(parameter.RefKind);
					instance.Add(parameter.TypeWithAnnotations);
				}
				parameterTypes = instance.ToImmutableAndFree();
				parameterRefKinds = instance2.ToImmutableAndFree();
			}
			if (isAsync && methodSymbol.ReturnType is NamedTypeSymbol namedTypeSymbol && !namedTypeSymbol.IsVoidType() && namedTypeSymbol.IsCustomTaskType(out TypeSymbol _))
			{
				taskLikeReturnTypeOpt = namedTypeSymbol.ConstructedFrom;
			}
		}
	}

	private sealed class BindingCacheComparer : IEqualityComparer<(NamedTypeSymbol Type, bool IsExpressionTree)>
	{
		public static readonly BindingCacheComparer Instance = new BindingCacheComparer();

		public bool Equals([AllowNull] (NamedTypeSymbol Type, bool IsExpressionTree) x, [AllowNull] (NamedTypeSymbol Type, bool IsExpressionTree) y)
		{
			if (x.IsExpressionTree == y.IsExpressionTree)
			{
				return Symbol.Equals(x.Type, y.Type, TypeCompareKind.ConsiderEverything);
			}
			return false;
		}

		public int GetHashCode([DisallowNull] (NamedTypeSymbol Type, bool IsExpressionTree) obj)
		{
			return Hash.Combine(obj.Type, obj.IsExpressionTree.GetHashCode());
		}
	}

	private UnboundLambda _unboundLambda;

	internal readonly Binder Binder;

	private ImmutableDictionary<(NamedTypeSymbol Type, bool IsExpressionLambda), BoundLambda>? _bindingCache;

	private ImmutableDictionary<ReturnInferenceCacheKey, BoundLambda>? _returnInferenceCache;

	private BoundLambda? _errorBinding;

	public UnboundLambda UnboundLambda => _unboundLambda;

	public abstract MessageID MessageID { get; }

	public abstract bool HasSignature { get; }

	public abstract bool HasExplicitlyTypedParameterList { get; }

	public abstract int ParameterCount { get; }

	public abstract bool IsAsync { get; }

	public abstract bool IsStatic { get; }

	public UnboundLambdaState(Binder binder, bool includeCache)
	{
		if (includeCache)
		{
			_bindingCache = ImmutableDictionary<(NamedTypeSymbol, bool), BoundLambda>.Empty.WithComparers(BindingCacheComparer.Instance);
			_returnInferenceCache = ImmutableDictionary<ReturnInferenceCacheKey, BoundLambda>.Empty;
		}
		Binder = binder;
	}

	public void SetUnboundLambda(UnboundLambda unbound)
	{
		_unboundLambda = unbound;
	}

	protected abstract UnboundLambdaState WithCachingCore(bool includeCache);

	internal UnboundLambdaState WithCaching(bool includeCache)
	{
		if (_bindingCache == null != includeCache)
		{
			return this;
		}
		return WithCachingCore(includeCache);
	}

	public abstract string ParameterName(int index);

	public abstract bool ParameterIsDiscard(int index);

	public abstract SyntaxList<AttributeListSyntax> ParameterAttributes(int index);

	public abstract bool HasExplicitReturnType(out RefKind refKind, out ImmutableArray<CustomModifier> refCustomModifiers, out TypeWithAnnotations returnType);

	public abstract Location ParameterLocation(int index);

	public abstract TypeWithAnnotations ParameterTypeWithAnnotations(int index);

	public abstract RefKind RefKind(int index);

	public abstract ScopedKind DeclaredScope(int index);

	public abstract ParameterSyntax? ParameterSyntax(int i);

	protected BoundBlock BindLambdaBody(LambdaSymbol lambdaSymbol, Binder lambdaBodyBinder, BindingDiagnosticBag diagnostics)
	{
		if (lambdaSymbol.DeclaringCompilation?.TestOnlyCompilationData is LambdaBindingData lambdaBindingData)
		{
			Interlocked.Increment(ref lambdaBindingData.LambdaBindingCount);
		}
		Microsoft.CodeAnalysis.CSharp.Binder.RecordLambdaBinding(UnboundLambda.Syntax);
		return BindLambdaBodyCore(lambdaSymbol, lambdaBodyBinder, diagnostics);
	}

	protected abstract BoundBlock BindLambdaBodyCore(LambdaSymbol lambdaSymbol, Binder lambdaBodyBinder, BindingDiagnosticBag diagnostics);

	protected abstract BoundExpression? GetLambdaExpressionBody(BoundBlock body);

	protected abstract BoundBlock CreateBlockFromLambdaExpressionBody(Binder lambdaBodyBinder, BoundExpression expression, BindingDiagnosticBag diagnostics);

	public virtual void GenerateAnonymousFunctionConversionError(BindingDiagnosticBag diagnostics, TypeSymbol targetType)
	{
		Binder.GenerateAnonymousFunctionConversionError(diagnostics, _unboundLambda.Syntax, _unboundLambda, targetType);
	}

	public BoundLambda Bind(NamedTypeSymbol delegateType, bool isTargetExpressionTree)
	{
		bool flag = Binder.InExpressionTree | isTargetExpressionTree;
		if (!_bindingCache.TryGetValue((delegateType, flag), out BoundLambda value))
		{
			value = ReallyBind(delegateType, flag);
			return ImmutableInterlocked.GetOrAdd(ref _bindingCache, (delegateType, flag), value);
		}
		return value;
	}

	internal IEnumerable<TypeSymbol> InferredReturnTypes()
	{
		bool any = false;
		foreach (BoundLambda value in _returnInferenceCache.Values)
		{
			TypeWithAnnotations typeWithAnnotations = value.InferredReturnType.TypeWithAnnotations;
			if (typeWithAnnotations.HasType)
			{
				any = true;
				yield return typeWithAnnotations.Type;
			}
		}
		if (!any)
		{
			TypeWithAnnotations typeWithAnnotations2 = BindForErrorRecovery().InferredReturnType.TypeWithAnnotations;
			if (typeWithAnnotations2.HasType)
			{
				yield return typeWithAnnotations2.Type;
			}
		}
	}

	private static MethodSymbol? DelegateInvokeMethod(NamedTypeSymbol? delegateType)
	{
		return delegateType.GetDelegateType()?.DelegateInvokeMethod;
	}

	private static TypeWithAnnotations DelegateReturnTypeWithAnnotations(MethodSymbol? invokeMethod, out RefKind refKind, out ImmutableArray<CustomModifier> refCustomModifiers)
	{
		if ((object)invokeMethod == null)
		{
			refKind = Microsoft.CodeAnalysis.RefKind.None;
			refCustomModifiers = ImmutableArray<CustomModifier>.Empty;
			return default(TypeWithAnnotations);
		}
		refKind = invokeMethod.RefKind;
		refCustomModifiers = invokeMethod.RefCustomModifiers;
		return invokeMethod.ReturnTypeWithAnnotations;
	}

	internal (ImmutableArray<RefKind>, ArrayBuilder<ScopedKind>, ImmutableArray<TypeWithAnnotations>, bool) CollectParameterProperties()
	{
		ArrayBuilder<RefKind> instance = ArrayBuilder<Microsoft.CodeAnalysis.RefKind>.GetInstance(ParameterCount);
		ArrayBuilder<ScopedKind> instance2 = ArrayBuilder<ScopedKind>.GetInstance(ParameterCount);
		ArrayBuilder<TypeWithAnnotations> instance3 = ArrayBuilder<TypeWithAnnotations>.GetInstance(ParameterCount);
		bool item = false;
		for (int i = 0; i < ParameterCount; i++)
		{
			RefKind refKind = RefKind(i);
			ScopedKind scopedKind = DeclaredScope(i);
			TypeWithAnnotations item2 = ParameterTypeWithAnnotations(i);
			switch (scopedKind)
			{
			case ScopedKind.None:
				if (ParameterHelpers.IsRefScopedByDefault(Binder.UseUpdatedEscapeRules, refKind))
				{
					scopedKind = ScopedKind.ScopedRef;
					if (_unboundLambda.ParameterAttributes(i).Any())
					{
						item = true;
					}
				}
				else
				{
					if (!item2.IsRefLikeOrAllowsRefLikeType())
					{
						break;
					}
					ParameterSyntax? parameterSyntax = ParameterSyntax(i);
					if (parameterSyntax != null && parameterSyntax.Modifiers.Any(SyntaxKind.ParamsKeyword))
					{
						scopedKind = ScopedKind.ScopedValue;
						if (_unboundLambda.ParameterAttributes(i).Any())
						{
							item = true;
						}
					}
				}
				break;
			case ScopedKind.ScopedValue:
				if (_unboundLambda.ParameterAttributes(i).Any())
				{
					item = true;
				}
				break;
			}
			instance.Add(refKind);
			instance2.Add(scopedKind);
			instance3.Add(item2);
		}
		ImmutableArray<RefKind> item3 = instance.ToImmutableAndFree();
		ImmutableArray<TypeWithAnnotations> item4 = instance3.ToImmutableAndFree();
		return (item3, instance2, item4, item);
	}

	internal NamedTypeSymbol? InferDelegateType()
	{
		if (!HasExplicitlyTypedParameterList)
		{
			return null;
		}
		(ImmutableArray<RefKind>, ArrayBuilder<ScopedKind>, ImmutableArray<TypeWithAnnotations>, bool) tuple = CollectParameterProperties();
		ImmutableArray<RefKind> item = tuple.Item1;
		ArrayBuilder<ScopedKind> item2 = tuple.Item2;
		ImmutableArray<TypeWithAnnotations> item3 = tuple.Item3;
		bool item4 = tuple.Item4;
		Symbol? containingMemberOrLambda = Binder.ContainingMemberOrLambda;
		TypeWithAnnotations definitionElementType = default(TypeWithAnnotations);
		LambdaSymbol lambdaSymbol = CreateLambdaSymbol(containingMemberOrLambda, definitionElementType, item3, item, Microsoft.CodeAnalysis.RefKind.None, ImmutableArray<CustomModifier>.Empty);
		if (!HasExplicitReturnType(out RefKind refKind, out ImmutableArray<CustomModifier> _, out TypeWithAnnotations returnType))
		{
			ExecutableCodeBinder executableCodeBinder = new ExecutableCodeBinder(_unboundLambda.Syntax, lambdaSymbol, GetWithParametersBinder(lambdaSymbol, Binder));
			BoundBlock block = BindLambdaBody(lambdaSymbol, executableCodeBinder, BindingDiagnosticBag.Discarded);
			ArrayBuilder<(BoundReturnStatement, TypeWithAnnotations)> instance = ArrayBuilder<(BoundReturnStatement, TypeWithAnnotations)>.GetInstance();
			BoundLambda.BlockReturns.GetReturnTypes(instance, block);
			InferredLambdaReturnType inferredLambdaReturnType = BoundLambda.InferReturnType(instance, _unboundLambda, executableCodeBinder, null, IsAsync, Binder.Conversions);
			returnType = inferredLambdaReturnType.TypeWithAnnotations;
			refKind = inferredLambdaReturnType.RefKind;
			if (!returnType.HasType && inferredLambdaReturnType.NumExpressions > 0)
			{
				return null;
			}
		}
		if (item4)
		{
			for (int i = 0; i < ParameterCount; i++)
			{
				if (((DeclaredScope(i) == ScopedKind.None && item2[i] == ScopedKind.ScopedRef) || DeclaredScope(i) == ScopedKind.ScopedValue || item2[i] == ScopedKind.ScopedValue) && _unboundLambda.ParameterAttributes(i).Any())
				{
					item2[i] = lambdaSymbol.Parameters[i].EffectiveScope;
				}
			}
		}
		if (!returnType.HasType)
		{
			returnType = TypeWithAnnotations.Create(Binder.Compilation.GetSpecialType(SpecialType.System_Void));
		}
		return Binder.GetMethodGroupOrLambdaDelegateType(_unboundLambda.Syntax, lambdaSymbol, OverloadResolution.IsValidParams(Binder, lambdaSymbol, disallowExpandedNonArrayParams: false, out definitionElementType), item2.ToImmutableAndFree(), lambdaSymbol.Parameters.SelectAsArray((ParameterSymbol p) => p.HasUnscopedRefAttribute && p.UseUpdatedEscapeRules), refKind, returnType);
	}

	private BoundLambda ReallyBind(NamedTypeSymbol delegateType, bool inExpressionTree)
	{
		TypeWithAnnotations typeWithAnnotations = DelegateReturnTypeWithAnnotations(DelegateInvokeMethod(delegateType), out RefKind refKind, out ImmutableArray<CustomModifier> refCustomModifiers);
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: true, _unboundLambda.WithDependencies);
		CSharpCompilation compilation = Binder.Compilation;
		ReturnInferenceCacheKey returnInferenceCacheKey = ReturnInferenceCacheKey.Create(delegateType, IsAsync);
		LambdaSymbol lambdaSymbol;
		Binder binder;
		BoundBlock boundBlock;
		if (!inExpressionTree && refKind == Microsoft.CodeAnalysis.RefKind.None && _returnInferenceCache.TryGetValue(returnInferenceCacheKey, out BoundLambda value))
		{
			BoundExpression lambdaExpressionBody = GetLambdaExpressionBody(value.Body);
			if (lambdaExpressionBody != null && (lambdaSymbol = (LambdaSymbol)value.Symbol).RefKind == refKind && (object)LambdaSymbol.InferenceFailureReturnType != lambdaSymbol.ReturnType && lambdaSymbol.ReturnTypeWithAnnotations.Equals(typeWithAnnotations, TypeCompareKind.ConsiderEverything) && lambdaSymbol.RefCustomModifiers.SequenceEqual(refCustomModifiers))
			{
				binder = value.Binder;
				boundBlock = CreateBlockFromLambdaExpressionBody(binder, lambdaExpressionBody, instance);
				instance.AddRange(value.Diagnostics);
				goto IL_0139;
			}
		}
		lambdaSymbol = CreateLambdaSymbol(Binder.ContainingMemberOrLambda, typeWithAnnotations, returnInferenceCacheKey.ParameterTypes, returnInferenceCacheKey.ParameterRefKinds, refKind, refCustomModifiers);
		binder = new ExecutableCodeBinder(_unboundLambda.Syntax, lambdaSymbol, GetWithParametersBinder(lambdaSymbol, Binder), inExpressionTree ? BinderFlags.InExpressionTree : BinderFlags.None);
		boundBlock = BindLambdaBody(lambdaSymbol, binder, instance);
		goto IL_0139;
		IL_0139:
		lambdaSymbol.GetDeclarationDiagnostics(instance);
		if (lambdaSymbol.RefKind == Microsoft.CodeAnalysis.RefKind.In)
		{
			compilation.EnsureIsReadOnlyAttributeExists(instance, lambdaSymbol.DiagnosticLocation, modifyCompilation: false);
		}
		ImmutableArray<ParameterSymbol> parameters = lambdaSymbol.Parameters;
		ParameterHelpers.EnsureRefKindAttributesExist(compilation, parameters, instance, modifyCompilation: false);
		ParameterHelpers.EnsureParamCollectionAttributeExists(compilation, parameters, instance, modifyCompilation: false);
		if (typeWithAnnotations.HasType)
		{
			if (compilation.ShouldEmitNativeIntegerAttributes(typeWithAnnotations.Type))
			{
				compilation.EnsureNativeIntegerAttributeExists(instance, lambdaSymbol.DiagnosticLocation, modifyCompilation: false);
			}
			if (compilation.ShouldEmitNullableAttributes(lambdaSymbol) && typeWithAnnotations.NeedsNullableAttribute())
			{
				compilation.EnsureNullableAttributeExists(instance, lambdaSymbol.DiagnosticLocation, modifyCompilation: false);
			}
		}
		ParameterHelpers.EnsureNativeIntegerAttributeExists(compilation, parameters, instance, modifyCompilation: false);
		ParameterHelpers.EnsureScopedRefAttributeExists(compilation, parameters, instance, modifyCompilation: false);
		ParameterHelpers.EnsureNullableAttributeExists(compilation, lambdaSymbol, parameters, instance, modifyCompilation: false);
		ValidateUnsafeParameters(instance, returnInferenceCacheKey.ParameterTypes);
		if (ControlFlowPass.Analyze(compilation, lambdaSymbol, boundBlock, instance.DiagnosticBag))
		{
			if (Microsoft.CodeAnalysis.CSharp.Binder.MethodOrLambdaRequiresValue(lambdaSymbol, Binder.Compilation))
			{
				instance.Add(ErrorCode.ERR_AnonymousReturnExpected, lambdaSymbol.DiagnosticLocation, MessageID.Localize(), delegateType);
			}
			else
			{
				boundBlock = FlowAnalysisPass.AppendImplicitReturn(boundBlock, lambdaSymbol);
			}
		}
		if (IsAsync && !ErrorFacts.PreventsSuccessfulDelegateConversion(instance.DiagnosticBag) && typeWithAnnotations.HasType && !typeWithAnnotations.IsVoidType() && !lambdaSymbol.IsAsyncEffectivelyReturningTask(compilation) && !lambdaSymbol.IsAsyncEffectivelyReturningGenericTask(compilation))
		{
			instance.Add(ErrorCode.ERR_CantConvAsyncAnonFuncReturns, lambdaSymbol.DiagnosticLocation, lambdaSymbol.MessageID.Localize(), delegateType);
		}
		return new BoundLambda(_unboundLambda.Syntax, _unboundLambda, boundBlock, instance.ToReadOnlyAndFree(), binder, delegateType, default(InferredLambdaReturnType))
		{
			WasCompilerGenerated = _unboundLambda.WasCompilerGenerated
		};
	}

	internal LambdaSymbol CreateLambdaSymbol(Symbol containingSymbol, TypeWithAnnotations returnType, ImmutableArray<TypeWithAnnotations> parameterTypes, ImmutableArray<RefKind> parameterRefKinds, RefKind refKind, ImmutableArray<CustomModifier> refCustomModifiers)
	{
		return new LambdaSymbol(Binder, Binder.Compilation, containingSymbol, _unboundLambda, parameterTypes, parameterRefKinds, refKind, refCustomModifiers, returnType);
	}

	internal LambdaSymbol CreateLambdaSymbol(NamedTypeSymbol delegateType, Symbol containingSymbol)
	{
		TypeWithAnnotations returnType = DelegateReturnTypeWithAnnotations(DelegateInvokeMethod(delegateType), out RefKind refKind, out ImmutableArray<CustomModifier> refCustomModifiers);
		ReturnInferenceCacheKey.GetFields(delegateType, IsAsync, out ImmutableArray<TypeWithAnnotations> parameterTypes, out ImmutableArray<RefKind> parameterRefKinds, out NamedTypeSymbol _);
		return CreateLambdaSymbol(containingSymbol, returnType, parameterTypes, parameterRefKinds, refKind, refCustomModifiers);
	}

	private void ValidateUnsafeParameters(BindingDiagnosticBag diagnostics, ImmutableArray<TypeWithAnnotations> targetParameterTypes)
	{
		if (!HasSignature)
		{
			return;
		}
		int num = Math.Min(targetParameterTypes.Length, ParameterCount);
		for (int i = 0; i < num; i++)
		{
			if (targetParameterTypes[i].Type.ContainsPointerOrFunctionPointer())
			{
				Binder.ReportUnsafeIfNotAllowed(ParameterLocation(i), diagnostics);
			}
		}
	}

	private BoundLambda ReallyInferReturnType(NamedTypeSymbol? delegateType, ImmutableArray<TypeWithAnnotations> parameterTypes, ImmutableArray<RefKind> parameterRefKinds)
	{
		bool flag = HasExplicitReturnType(out RefKind refKind, out ImmutableArray<CustomModifier> refCustomModifiers, out TypeWithAnnotations returnType);
		var (lambdaSymbol, boundBlock, executableCodeBinder, bindingDiagnosticBag) = BindWithParameterAndReturnType(parameterTypes, parameterRefKinds, returnType, refKind, refCustomModifiers);
		InferredLambdaReturnType inferredReturnType;
		if (flag)
		{
			inferredReturnType = new InferredLambdaReturnType(0, isExplicitType: true, hadExpressionlessReturn: false, refKind, refCustomModifiers, returnType, inferredFromFunctionType: false, ImmutableArray<DiagnosticInfo>.Empty, ImmutableArray<AssemblySymbol>.Empty);
		}
		else
		{
			ArrayBuilder<(BoundReturnStatement, TypeWithAnnotations)> instance = ArrayBuilder<(BoundReturnStatement, TypeWithAnnotations)>.GetInstance();
			BoundLambda.BlockReturns.GetReturnTypes(instance, boundBlock);
			inferredReturnType = BoundLambda.InferReturnType(instance, _unboundLambda, executableCodeBinder, delegateType, lambdaSymbol.IsAsync, executableCodeBinder.Conversions);
			refKind = inferredReturnType.RefKind;
			refCustomModifiers = inferredReturnType.RefCustomModifiers;
			returnType = inferredReturnType.TypeWithAnnotations;
			if (!returnType.HasType)
			{
				returnType = (((object)delegateType == null && instance.Count == 0) ? TypeWithAnnotations.Create(Binder.Compilation.GetSpecialType(SpecialType.System_Void)) : TypeWithAnnotations.Create(LambdaSymbol.InferenceFailureReturnType));
			}
			instance.Free();
		}
		BoundLambda result = new BoundLambda(_unboundLambda.Syntax, _unboundLambda, boundBlock, bindingDiagnosticBag.ToReadOnlyAndFree(), executableCodeBinder, delegateType, inferredReturnType)
		{
			WasCompilerGenerated = _unboundLambda.WasCompilerGenerated
		};
		if (!flag)
		{
			lambdaSymbol.SetInferredReturnType(refKind, returnType);
		}
		return result;
	}

	private (LambdaSymbol lambdaSymbol, BoundBlock block, ExecutableCodeBinder lambdaBodyBinder, BindingDiagnosticBag diagnostics) BindWithParameterAndReturnType(ImmutableArray<TypeWithAnnotations> parameterTypes, ImmutableArray<RefKind> parameterRefKinds, TypeWithAnnotations returnType, RefKind refKind, ImmutableArray<CustomModifier> refCustomModifiers)
	{
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: true, _unboundLambda.WithDependencies);
		LambdaSymbol lambdaSymbol = CreateLambdaSymbol(Binder.ContainingMemberOrLambda, returnType, parameterTypes, parameterRefKinds, refKind, refCustomModifiers);
		ExecutableCodeBinder executableCodeBinder = new ExecutableCodeBinder(_unboundLambda.Syntax, lambdaSymbol, GetWithParametersBinder(lambdaSymbol, Binder));
		BoundBlock item = BindLambdaBody(lambdaSymbol, executableCodeBinder, instance);
		lambdaSymbol.GetDeclarationDiagnostics(instance);
		return (lambdaSymbol: lambdaSymbol, block: item, lambdaBodyBinder: executableCodeBinder, diagnostics: instance);
	}

	public BoundLambda BindForReturnTypeInference(NamedTypeSymbol delegateType)
	{
		ReturnInferenceCacheKey returnInferenceCacheKey = ReturnInferenceCacheKey.Create(delegateType, IsAsync);
		if (!_returnInferenceCache.TryGetValue(returnInferenceCacheKey, out BoundLambda value))
		{
			value = ReallyInferReturnType(delegateType, returnInferenceCacheKey.ParameterTypes, returnInferenceCacheKey.ParameterRefKinds);
			return ImmutableInterlocked.GetOrAdd(ref _returnInferenceCache, returnInferenceCacheKey, value);
		}
		return value;
	}

	public virtual Binder GetWithParametersBinder(LambdaSymbol lambdaSymbol, Binder binder)
	{
		return new WithLambdaParametersBinder(lambdaSymbol, binder);
	}

	public BoundLambda BindForErrorRecovery()
	{
		if (_errorBinding == null)
		{
			Interlocked.CompareExchange(ref _errorBinding, ReallyBindForErrorRecovery(), null);
		}
		return _errorBinding;
	}

	private BoundLambda ReallyBindForErrorRecovery()
	{
		return GuessBestBoundLambda(_bindingCache) ?? rebind(GuessBestBoundLambda(_returnInferenceCache)) ?? rebind(ReallyInferReturnType(null, ImmutableArray<TypeWithAnnotations>.Empty, ImmutableArray<Microsoft.CodeAnalysis.RefKind>.Empty));
		[return: NotNullIfNotNull("lambda")]
		BoundLambda? rebind(BoundLambda? lambda)
		{
			if (lambda == null)
			{
				return null;
			}
			NamedTypeSymbol delegateType = (NamedTypeSymbol)lambda.Type;
			ReturnInferenceCacheKey.GetFields(delegateType, IsAsync, out ImmutableArray<TypeWithAnnotations> parameterTypes, out ImmutableArray<RefKind> parameterRefKinds, out NamedTypeSymbol _);
			return ReallyBindForErrorRecovery(delegateType, lambda.InferredReturnType, parameterTypes, parameterRefKinds);
		}
	}

	private BoundLambda ReallyBindForErrorRecovery(NamedTypeSymbol? delegateType, InferredLambdaReturnType inferredReturnType, ImmutableArray<TypeWithAnnotations> parameterTypes, ImmutableArray<RefKind> parameterRefKinds)
	{
		TypeWithAnnotations typeWithAnnotations = inferredReturnType.TypeWithAnnotations;
		RefKind refKind = inferredReturnType.RefKind;
		ImmutableArray<CustomModifier> refCustomModifiers = inferredReturnType.RefCustomModifiers;
		if (!typeWithAnnotations.HasType)
		{
			typeWithAnnotations = DelegateReturnTypeWithAnnotations(DelegateInvokeMethod(delegateType), out refKind, out refCustomModifiers);
			if (!typeWithAnnotations.HasType || typeWithAnnotations.Type.ContainsTypeParameter())
			{
				typeWithAnnotations = TypeWithAnnotations.Create((inferredReturnType.HadExpressionlessReturn || inferredReturnType.NumExpressions == 0) ? Binder.Compilation.GetSpecialType(SpecialType.System_Void) : Binder.CreateErrorType());
				refKind = Microsoft.CodeAnalysis.RefKind.None;
			}
		}
		(LambdaSymbol lambdaSymbol, BoundBlock block, ExecutableCodeBinder lambdaBodyBinder, BindingDiagnosticBag diagnostics) tuple = BindWithParameterAndReturnType(parameterTypes, parameterRefKinds, typeWithAnnotations, refKind, refCustomModifiers);
		BoundBlock item = tuple.block;
		ExecutableCodeBinder item2 = tuple.lambdaBodyBinder;
		BindingDiagnosticBag item3 = tuple.diagnostics;
		return new BoundLambda(_unboundLambda.Syntax, _unboundLambda, item, item3.ToReadOnlyAndFree(), item2, delegateType, new InferredLambdaReturnType(inferredReturnType.NumExpressions, inferredReturnType.IsExplicitType, inferredReturnType.HadExpressionlessReturn, refKind, refCustomModifiers, typeWithAnnotations, inferredReturnType.InferredFromFunctionType, ImmutableArray<DiagnosticInfo>.Empty, ImmutableArray<AssemblySymbol>.Empty))
		{
			WasCompilerGenerated = _unboundLambda.WasCompilerGenerated
		};
	}

	private static BoundLambda? GuessBestBoundLambda<T>(ImmutableDictionary<T, BoundLambda> candidates) where T : notnull
	{
		return candidates.Count switch
		{
			0 => null, 
			1 => candidates.First().Value, 
			_ => (from lambda in (from lambda in candidates
					group lambda by lambda.Value.Diagnostics.Diagnostics.Length into @group
					orderby @group.Key
					select @group).First()
				orderby GetLambdaSortString((LambdaSymbol)lambda.Value.Symbol)
				select lambda).FirstOrDefault().Value, 
		};
	}

	private static string GetLambdaSortString(LambdaSymbol lambda)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		foreach (ParameterSymbol parameter in lambda.Parameters)
		{
			instance.Builder.Append(parameter.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageNoParameterNamesFormat));
		}
		if (lambda.ReturnTypeWithAnnotations.HasType)
		{
			instance.Builder.Append(lambda.ReturnTypeWithAnnotations.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
		}
		return instance.ToStringAndFree();
	}

	public bool GenerateSummaryErrors(BindingDiagnosticBag diagnostics)
	{
		IEnumerable<ReadOnlyBindingDiagnostic<AssemblySymbol>> first = _bindingCache.Select<KeyValuePair<(NamedTypeSymbol, bool), BoundLambda>, ReadOnlyBindingDiagnostic<AssemblySymbol>>(delegate(KeyValuePair<(NamedTypeSymbol Type, bool IsExpressionLambda), BoundLambda> boundLambda)
		{
			KeyValuePair<(NamedTypeSymbol, bool), BoundLambda> keyValuePair = boundLambda;
			return keyValuePair.Value.Diagnostics;
		});
		IEnumerable<ReadOnlyBindingDiagnostic<AssemblySymbol>> second = _returnInferenceCache.Values.Select((BoundLambda boundLambda) => boundLambda.Diagnostics);
		IEnumerable<ReadOnlyBindingDiagnostic<AssemblySymbol>> enumerable = first.Concat(second);
		FirstAmongEqualsSet<Diagnostic> firstAmongEqualsSet = null;
		foreach (ReadOnlyBindingDiagnostic<AssemblySymbol> item in enumerable)
		{
			if (firstAmongEqualsSet == null)
			{
				firstAmongEqualsSet = CreateFirstAmongEqualsSet(item.Diagnostics);
			}
			else
			{
				firstAmongEqualsSet.IntersectWith(item.Diagnostics);
			}
		}
		if (firstAmongEqualsSet != null && PreventsSuccessfulDelegateConversion(firstAmongEqualsSet))
		{
			diagnostics.AddRange(firstAmongEqualsSet);
			return true;
		}
		FirstAmongEqualsSet<Diagnostic> firstAmongEqualsSet2 = null;
		foreach (ReadOnlyBindingDiagnostic<AssemblySymbol> item2 in enumerable)
		{
			if (firstAmongEqualsSet2 == null)
			{
				firstAmongEqualsSet2 = CreateFirstAmongEqualsSet(item2.Diagnostics);
			}
			else
			{
				firstAmongEqualsSet2.UnionWith(item2.Diagnostics);
			}
		}
		if (firstAmongEqualsSet2 != null && PreventsSuccessfulDelegateConversion(firstAmongEqualsSet2))
		{
			diagnostics.AddRange(firstAmongEqualsSet2);
			return true;
		}
		return false;
	}

	private static bool PreventsSuccessfulDelegateConversion(FirstAmongEqualsSet<Diagnostic> set)
	{
		foreach (Diagnostic item in set)
		{
			if (ErrorFacts.PreventsSuccessfulDelegateConversion((ErrorCode)item.Code))
			{
				return true;
			}
		}
		return false;
	}

	private static FirstAmongEqualsSet<Diagnostic> CreateFirstAmongEqualsSet(ImmutableArray<Diagnostic> bag)
	{
		return new FirstAmongEqualsSet<Diagnostic>(bag, CommonDiagnosticComparer.Instance, CanonicallyCompareDiagnostics);
	}

	private static int CanonicallyCompareDiagnostics(Diagnostic x, Diagnostic y)
	{
		if (x.Code != y.Code)
		{
			return x.Code - y.Code;
		}
		int num = x.Arguments?.Count ?? 0;
		int num2 = y.Arguments?.Count ?? 0;
		int i = 0;
		for (int num3 = Math.Min(num, num2); i < num3; i++)
		{
			object? obj = x.Arguments[i];
			int num4 = string.CompareOrdinal(strB: y.Arguments[i]?.ToString(), strA: obj?.ToString());
			if (num4 != 0)
			{
				return num4;
			}
		}
		return num - num2;
	}
}
