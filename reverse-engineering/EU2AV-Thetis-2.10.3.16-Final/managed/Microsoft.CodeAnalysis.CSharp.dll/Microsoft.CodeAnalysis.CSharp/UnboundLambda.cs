using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class UnboundLambda : BoundExpression
{
	private readonly NullableWalker.VariableState? _nullableState;

	private readonly NullableWalker.GetterNullResilienceData? _getterNullResilienceData;

	public override object Display => MessageID.Localize();

	public MessageID MessageID => Data.MessageID;

	public bool HasSignature => Data.HasSignature;

	public bool HasExplicitlyTypedParameterList => Data.HasExplicitlyTypedParameterList;

	public int ParameterCount => Data.ParameterCount;

	public bool IsAsync => Data.IsAsync;

	public bool IsStatic => Data.IsStatic;

	public new TypeSymbol? Type => base.Type;

	public UnboundLambdaState Data { get; }

	public FunctionTypeSymbol? FunctionType { get; }

	public bool WithDependencies { get; }

	public static UnboundLambda Create(CSharpSyntaxNode syntax, Binder binder, bool withDependencies, RefKind returnRefKind, ImmutableArray<CustomModifier> refCustomModifiers, TypeWithAnnotations returnType, ImmutableArray<SyntaxList<AttributeListSyntax>> parameterAttributes, ImmutableArray<RefKind> refKinds, ImmutableArray<ScopedKind> declaredScopes, ImmutableArray<TypeWithAnnotations> types, ImmutableArray<string> names, ImmutableArray<bool> discardsOpt, SeparatedSyntaxList<ParameterSyntax>? syntaxList, ImmutableArray<EqualsValueClauseSyntax?> defaultValues, bool isAsync, bool isStatic)
	{
		bool hasErrors = !types.IsDefault && types.Any(delegate(TypeWithAnnotations t)
		{
			TypeSymbol type = t.Type;
			return (object)type != null && type.Kind == SymbolKind.ErrorType;
		});
		FunctionTypeSymbol functionTypeSymbol = FunctionTypeSymbol.CreateIfFeatureEnabled(syntax, binder, (Binder binder2, BoundExpression expr) => ((UnboundLambda)expr).Data.InferDelegateType());
		PlainUnboundLambdaState plainUnboundLambdaState = new PlainUnboundLambdaState(binder, returnRefKind, refCustomModifiers, returnType, parameterAttributes, names, discardsOpt, types, refKinds, declaredScopes, defaultValues, syntaxList, isAsync, isStatic, includeCache: true);
		UnboundLambda unboundLambda = new UnboundLambda(syntax, plainUnboundLambdaState, functionTypeSymbol, withDependencies, hasErrors);
		plainUnboundLambdaState.SetUnboundLambda(unboundLambda);
		functionTypeSymbol?.SetExpression(unboundLambda.WithNoCache());
		return unboundLambda;
	}

	private UnboundLambda(SyntaxNode syntax, UnboundLambdaState state, FunctionTypeSymbol? functionType, bool withDependencies, NullableWalker.VariableState? nullableState, NullableWalker.GetterNullResilienceData? getterNullResilienceData, bool hasErrors)
		: this(syntax, state, functionType, withDependencies, hasErrors)
	{
		_nullableState = nullableState;
		_getterNullResilienceData = getterNullResilienceData;
	}

	internal UnboundLambda WithNullabilityInfo(NullableWalker.VariableState nullableState, NullableWalker.GetterNullResilienceData? getterNullResilienceData)
	{
		UnboundLambdaState unboundLambdaState = Data.WithCaching(includeCache: true);
		UnboundLambda unboundLambda = new UnboundLambda(Syntax, unboundLambdaState, FunctionType, WithDependencies, nullableState, getterNullResilienceData, base.HasErrors);
		unboundLambdaState.SetUnboundLambda(unboundLambda);
		return unboundLambda;
	}

	internal UnboundLambda WithNoCache()
	{
		UnboundLambdaState unboundLambdaState = Data.WithCaching(includeCache: false);
		if (unboundLambdaState == Data)
		{
			return this;
		}
		UnboundLambda unboundLambda = new UnboundLambda(Syntax, unboundLambdaState, FunctionType, WithDependencies, _nullableState, _getterNullResilienceData, base.HasErrors);
		unboundLambdaState.SetUnboundLambda(unboundLambda);
		return unboundLambda;
	}

	public BoundLambda Bind(NamedTypeSymbol delegateType, bool isExpressionTree)
	{
		return SuppressIfNeeded(Data.Bind(delegateType, isExpressionTree));
	}

	public BoundLambda BindForErrorRecovery()
	{
		return SuppressIfNeeded(Data.BindForErrorRecovery());
	}

	public BoundLambda BindForReturnTypeInference(NamedTypeSymbol delegateType)
	{
		return SuppressIfNeeded(Data.BindForReturnTypeInference(delegateType));
	}

	private BoundLambda SuppressIfNeeded(BoundLambda lambda)
	{
		if (!base.IsSuppressed)
		{
			return lambda;
		}
		return (BoundLambda)lambda.WithSuppression();
	}

	public bool HasExplicitReturnType(out RefKind refKind, out ImmutableArray<CustomModifier> refCustomModifiers, out TypeWithAnnotations returnType)
	{
		return Data.HasExplicitReturnType(out refKind, out refCustomModifiers, out returnType);
	}

	public Binder GetWithParametersBinder(LambdaSymbol lambdaSymbol, Binder binder)
	{
		return Data.GetWithParametersBinder(lambdaSymbol, binder);
	}

	public TypeWithAnnotations InferReturnType(ConversionsBase conversions, NamedTypeSymbol delegateType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, out bool inferredFromFunctionType)
	{
		return BindForReturnTypeInference(delegateType).GetInferredReturnType(conversions, _nullableState, _getterNullResilienceData, ref useSiteInfo, out inferredFromFunctionType);
	}

	public RefKind RefKind(int index)
	{
		return Data.RefKind(index);
	}

	public ScopedKind DeclaredScope(int index)
	{
		return Data.DeclaredScope(index);
	}

	public void GenerateAnonymousFunctionConversionError(BindingDiagnosticBag diagnostics, TypeSymbol targetType)
	{
		Data.GenerateAnonymousFunctionConversionError(diagnostics, targetType);
	}

	public bool GenerateSummaryErrors(BindingDiagnosticBag diagnostics)
	{
		return Data.GenerateSummaryErrors(diagnostics);
	}

	public SyntaxList<AttributeListSyntax> ParameterAttributes(int index)
	{
		return Data.ParameterAttributes(index);
	}

	public TypeWithAnnotations ParameterTypeWithAnnotations(int index)
	{
		return Data.ParameterTypeWithAnnotations(index);
	}

	public TypeSymbol ParameterType(int index)
	{
		return ParameterTypeWithAnnotations(index).Type;
	}

	public ParameterSyntax? ParameterSyntax(int index)
	{
		return Data.ParameterSyntax(index);
	}

	public Location ParameterLocation(int index)
	{
		return Data.ParameterLocation(index);
	}

	public string ParameterName(int index)
	{
		return Data.ParameterName(index);
	}

	public bool ParameterIsDiscard(int index)
	{
		return Data.ParameterIsDiscard(index);
	}

	public UnboundLambda(SyntaxNode syntax, UnboundLambdaState data, FunctionTypeSymbol? functionType, bool withDependencies, bool hasErrors)
		: base(BoundKind.UnboundLambda, syntax, null, hasErrors)
	{
		Data = data;
		FunctionType = functionType;
		WithDependencies = withDependencies;
	}

	public UnboundLambda(SyntaxNode syntax, UnboundLambdaState data, FunctionTypeSymbol? functionType, bool withDependencies)
		: base(BoundKind.UnboundLambda, syntax, null)
	{
		Data = data;
		FunctionType = functionType;
		WithDependencies = withDependencies;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitUnboundLambda(this);
	}

	public UnboundLambda Update(UnboundLambdaState data, FunctionTypeSymbol? functionType, bool withDependencies)
	{
		if (data != Data || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(functionType, FunctionType) || withDependencies != WithDependencies)
		{
			UnboundLambda unboundLambda = new UnboundLambda(Syntax, data, functionType, withDependencies, base.HasErrors);
			unboundLambda.CopyAttributes(this);
			return unboundLambda;
		}
		return this;
	}
}
