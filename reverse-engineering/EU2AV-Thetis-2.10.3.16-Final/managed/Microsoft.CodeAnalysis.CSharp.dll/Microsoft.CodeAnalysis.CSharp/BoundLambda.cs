using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundLambda : BoundExpression, IBoundLambdaOrFunction
{
	internal sealed class BlockReturns : BoundTreeWalker
	{
		private readonly ArrayBuilder<(BoundReturnStatement, TypeWithAnnotations)> _builder;

		private BlockReturns(ArrayBuilder<(BoundReturnStatement, TypeWithAnnotations)> builder)
		{
			_builder = builder;
		}

		public static void GetReturnTypes(ArrayBuilder<(BoundReturnStatement, TypeWithAnnotations)> builder, BoundBlock block)
		{
			new BlockReturns(builder).Visit(block);
		}

		public override BoundNode? Visit(BoundNode node)
		{
			if (!(node is BoundExpression))
			{
				return base.Visit(node);
			}
			return null;
		}

		protected override BoundNode VisitExpressionOrPatternWithoutStackGuard(BoundNode node)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/BoundTree/UnboundLambda.cs", 380);
		}

		public override BoundNode? VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
		{
			return null;
		}

		public override BoundNode? VisitReturnStatement(BoundReturnStatement node)
		{
			BoundExpression expressionOpt = node.ExpressionOpt;
			TypeSymbol typeSymbol = ((expressionOpt == null) ? NoReturnExpression : expressionOpt.Type?.SetUnknownNullabilityForReferenceTypes());
			_builder.Add((node, TypeWithAnnotations.Create(typeSymbol)));
			return null;
		}
	}

	internal static readonly TypeSymbol NoReturnExpression = new UnsupportedMetadataTypeSymbol();

	public override Symbol ExpressionSymbol => Symbol;

	public override object Display => MessageID.Localize();

	public MessageID MessageID
	{
		get
		{
			if (Syntax.Kind() != SyntaxKind.AnonymousMethodExpression)
			{
				return MessageID.IDS_Lambda;
			}
			return MessageID.IDS_AnonMethod;
		}
	}

	internal InferredLambdaReturnType InferredReturnType { get; }

	internal bool InAnonymousFunctionConversion { get; private set; }

	MethodSymbol IBoundLambdaOrFunction.Symbol => Symbol;

	SyntaxNode IBoundLambdaOrFunction.Syntax => Syntax;

	public UnboundLambda UnboundLambda { get; }

	public MethodSymbol Symbol { get; }

	public new TypeSymbol? Type => base.Type;

	public BoundBlock Body { get; }

	public ReadOnlyBindingDiagnostic<AssemblySymbol> Diagnostics { get; }

	public Binder Binder { get; }

	public BoundLambda(SyntaxNode syntax, UnboundLambda unboundLambda, BoundBlock body, ReadOnlyBindingDiagnostic<AssemblySymbol> diagnostics, Binder binder, TypeSymbol? delegateType, InferredLambdaReturnType inferredReturnType)
		: this(syntax, unboundLambda.WithNoCache(), (LambdaSymbol)binder.ContainingMemberOrLambda, body, diagnostics, binder, delegateType)
	{
		InferredReturnType = inferredReturnType;
	}

	internal BoundLambda WithInAnonymousFunctionConversion()
	{
		if (InAnonymousFunctionConversion)
		{
			return this;
		}
		BoundLambda obj = (BoundLambda)MemberwiseClone();
		obj.InAnonymousFunctionConversion = true;
		return obj;
	}

	public TypeWithAnnotations GetInferredReturnType(ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, out bool inferredFromFunctionType)
	{
		return GetInferredReturnType(null, null, null, ref useSiteInfo, out inferredFromFunctionType);
	}

	public TypeWithAnnotations GetInferredReturnType(ConversionsBase? conversions, NullableWalker.VariableState? nullableState, NullableWalker.GetterNullResilienceData? getterNullResilienceData, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, out bool inferredFromFunctionType)
	{
		if (!InferredReturnType.UseSiteDiagnostics.IsEmpty)
		{
			useSiteInfo.AddDiagnostics(InferredReturnType.UseSiteDiagnostics);
		}
		if (!InferredReturnType.Dependencies.IsEmpty)
		{
			useSiteInfo.AddDependencies(InferredReturnType.Dependencies);
		}
		InferredLambdaReturnType inferredLambdaReturnType;
		if (nullableState == null || InferredReturnType.IsExplicitType)
		{
			inferredLambdaReturnType = InferredReturnType;
		}
		else
		{
			ArrayBuilder<(BoundReturnStatement, TypeWithAnnotations)> instance = ArrayBuilder<(BoundReturnStatement, TypeWithAnnotations)>.GetInstance();
			DiagnosticBag instance2 = DiagnosticBag.GetInstance();
			NamedTypeSymbol delegateType = Type.GetDelegateType();
			NullableWalker.Analyze(Binder.Compilation, this, (Conversions)conversions, instance2, delegateType?.DelegateInvokeMethod, nullableState, instance, getterNullResilienceData);
			instance2.Free();
			inferredLambdaReturnType = InferReturnType(instance, this, Binder, delegateType, Symbol.IsAsync, conversions);
			instance.Free();
		}
		inferredFromFunctionType = inferredLambdaReturnType.InferredFromFunctionType;
		return inferredLambdaReturnType.TypeWithAnnotations;
	}

	internal LambdaSymbol CreateLambdaSymbol(NamedTypeSymbol delegateType, Symbol containingSymbol)
	{
		return UnboundLambda.Data.CreateLambdaSymbol(delegateType, containingSymbol);
	}

	internal LambdaSymbol CreateLambdaSymbol(Symbol containingSymbol, TypeWithAnnotations returnType, ImmutableArray<TypeWithAnnotations> parameterTypes, ImmutableArray<RefKind> parameterRefKinds, RefKind refKind, ImmutableArray<CustomModifier> refCustomModifiers)
	{
		return UnboundLambda.Data.CreateLambdaSymbol(containingSymbol, returnType, parameterTypes, parameterRefKinds.IsDefault ? Enumerable.Repeat(RefKind.None, parameterTypes.Length).ToImmutableArray() : parameterRefKinds, refKind, refCustomModifiers);
	}

	internal static InferredLambdaReturnType InferReturnType(ArrayBuilder<(BoundReturnStatement, TypeWithAnnotations)> returnTypes, BoundLambda node, Binder binder, TypeSymbol? delegateType, bool isAsync, ConversionsBase conversions)
	{
		return InferReturnTypeImpl(returnTypes, node, binder, delegateType, isAsync, conversions, node.UnboundLambda.WithDependencies);
	}

	internal static InferredLambdaReturnType InferReturnType(ArrayBuilder<(BoundReturnStatement, TypeWithAnnotations)> returnTypes, UnboundLambda node, Binder binder, TypeSymbol? delegateType, bool isAsync, ConversionsBase conversions)
	{
		return InferReturnTypeImpl(returnTypes, node, binder, delegateType, isAsync, conversions, node.WithDependencies);
	}

	private static InferredLambdaReturnType InferReturnTypeImpl(ArrayBuilder<(BoundReturnStatement, TypeWithAnnotations)> returnTypes, BoundNode node, Binder binder, TypeSymbol? delegateType, bool isAsync, ConversionsBase conversions, bool withDependencies)
	{
		ArrayBuilder<(BoundExpression, TypeWithAnnotations, bool)> instance = ArrayBuilder<(BoundExpression, TypeWithAnnotations, bool)>.GetInstance();
		bool hadExpressionlessReturn = false;
		RefKind refKind = RefKind.None;
		foreach (var returnType in returnTypes)
		{
			BoundReturnStatement item = returnType.Item1;
			TypeWithAnnotations item2 = returnType.Item2;
			RefKind refKind2 = item.RefKind;
			if (refKind2 != RefKind.None)
			{
				refKind = refKind2;
			}
			if ((object)item2.Type == NoReturnExpression)
			{
				hadExpressionlessReturn = true;
			}
			else
			{
				instance.Add((item.ExpressionOpt, item2, item.Checked));
			}
		}
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = (withDependencies ? new CompoundUseSiteInfo<AssemblySymbol>(binder.Compilation.Assembly) : CompoundUseSiteInfo<AssemblySymbol>.DiscardedDependencies);
		TypeWithAnnotations typeWithAnnotations = CalculateReturnType(binder, conversions, delegateType, instance, isAsync, node, ref useSiteInfo, out var inferredFromFunctionType);
		int count = instance.Count;
		instance.Free();
		return new InferredLambdaReturnType(count, isExplicitType: false, hadExpressionlessReturn, refKind, ImmutableArray<CustomModifier>.Empty, typeWithAnnotations, inferredFromFunctionType, useSiteInfo.Diagnostics.AsImmutableOrEmpty(), useSiteInfo.AccumulatesDependencies ? useSiteInfo.Dependencies.AsImmutableOrEmpty() : ImmutableArray<AssemblySymbol>.Empty);
	}

	private static TypeWithAnnotations CalculateReturnType(Binder binder, ConversionsBase conversions, TypeSymbol? delegateType, ArrayBuilder<(BoundExpression expr, TypeWithAnnotations resultType, bool isChecked)> returns, bool isAsync, BoundNode node, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, out bool inferredFromFunctionType)
	{
		int count = returns.Count;
		TypeWithAnnotations typeWithAnnotations;
		switch (count)
		{
		case 0:
			inferredFromFunctionType = false;
			typeWithAnnotations = default(TypeWithAnnotations);
			break;
		case 1:
		{
			if (conversions.IncludeNullability)
			{
				inferredFromFunctionType = false;
				typeWithAnnotations = returns[0].resultType;
				break;
			}
			TypeSymbol typeSymbol = returns[0].expr.GetTypeOrFunctionType();
			if (typeSymbol is FunctionTypeSymbol functionTypeSymbol)
			{
				typeSymbol = functionTypeSymbol.GetInternalDelegateType();
				inferredFromFunctionType = (object)typeSymbol != null;
			}
			else
			{
				inferredFromFunctionType = false;
			}
			typeWithAnnotations = TypeWithAnnotations.Create(typeSymbol);
			break;
		}
		default:
			typeWithAnnotations = ((!conversions.IncludeNullability) ? TypeWithAnnotations.Create(BestTypeInferrer.InferBestType(returns.SelectAsArray(((BoundExpression expr, TypeWithAnnotations resultType, bool isChecked) pair) => pair.expr), conversions, ref useSiteInfo, out inferredFromFunctionType)) : NullableWalker.BestTypeForLambdaReturns(returns, binder, node, (Conversions)conversions, out inferredFromFunctionType));
			break;
		}
		if (!isAsync)
		{
			return typeWithAnnotations;
		}
		NamedTypeSymbol namedTypeSymbol = null;
		if (delegateType?.GetDelegateType()?.DelegateInvokeMethod?.ReturnType is NamedTypeSymbol namedTypeSymbol2 && !namedTypeSymbol2.IsVoidType() && namedTypeSymbol2.IsCustomTaskType(out TypeSymbol _))
		{
			namedTypeSymbol = namedTypeSymbol2.ConstructedFrom;
		}
		if (count == 0)
		{
			return TypeWithAnnotations.Create(((object)namedTypeSymbol != null && namedTypeSymbol.Arity == 0) ? namedTypeSymbol : binder.Compilation.GetWellKnownType(WellKnownType.System_Threading_Tasks_Task));
		}
		if (!typeWithAnnotations.HasType || typeWithAnnotations.IsVoidType())
		{
			return default(TypeWithAnnotations);
		}
		return TypeWithAnnotations.Create((((object)namedTypeSymbol != null && namedTypeSymbol.Arity == 1) ? namedTypeSymbol : binder.Compilation.GetWellKnownType(WellKnownType.System_Threading_Tasks_Task_T)).Construct(ImmutableArray.Create(typeWithAnnotations)));
	}

	public BoundLambda(SyntaxNode syntax, UnboundLambda unboundLambda, MethodSymbol symbol, BoundBlock body, ReadOnlyBindingDiagnostic<AssemblySymbol> diagnostics, Binder binder, TypeSymbol? type, bool hasErrors = false)
		: base(BoundKind.Lambda, syntax, type, hasErrors || unboundLambda.HasErrors() || body.HasErrors())
	{
		UnboundLambda = unboundLambda;
		Symbol = symbol;
		Body = body;
		Diagnostics = diagnostics;
		Binder = binder;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitLambda(this);
	}

	public BoundLambda Update(UnboundLambda unboundLambda, MethodSymbol symbol, BoundBlock body, ReadOnlyBindingDiagnostic<AssemblySymbol> diagnostics, Binder binder, TypeSymbol? type)
	{
		if (unboundLambda != UnboundLambda || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(symbol, Symbol) || body != Body || diagnostics != Diagnostics || binder != Binder || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundLambda boundLambda = new BoundLambda(Syntax, unboundLambda, symbol, body, diagnostics, binder, type, base.HasErrors);
			boundLambda.CopyAttributes(this);
			return boundLambda;
		}
		return this;
	}
}
