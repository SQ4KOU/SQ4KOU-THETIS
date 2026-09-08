using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class LambdaSymbol : SourceMethodSymbol
{
	private readonly Binder _binder;

	private readonly Symbol _containingSymbol;

	private readonly MessageID _messageID;

	private readonly SyntaxNode _syntax;

	private readonly ImmutableArray<ParameterSymbol> _parameters;

	private RefKind _refKind;

	private ImmutableArray<CustomModifier> _refCustomModifiers;

	private TypeWithAnnotations _returnType;

	private readonly bool _isSynthesized;

	private readonly bool _isAsync;

	private readonly bool _isStatic;

	private readonly DiagnosticBag _declarationDiagnostics;

	private readonly HashSet<AssemblySymbol> _declarationDependencies;

	internal static readonly TypeSymbol ReturnTypeIsBeingInferred = new UnsupportedMetadataTypeSymbol();

	internal static readonly TypeSymbol InferenceFailureReturnType = new UnsupportedMetadataTypeSymbol();

	public MessageID MessageID => _messageID;

	public override MethodKind MethodKind => MethodKind.AnonymousFunction;

	public override bool IsExtern => false;

	public override bool IsSealed => false;

	public override bool IsAbstract => false;

	public override bool IsVirtual => false;

	public override bool IsOverride => false;

	public override bool IsStatic => _isStatic;

	public override bool IsAsync => _isAsync;

	internal override bool IsMetadataFinal => false;

	public override bool IsVararg => false;

	internal override bool HasSpecialName => false;

	public override bool ReturnsVoid
	{
		get
		{
			if (ReturnTypeWithAnnotations.HasType)
			{
				return base.ReturnType.IsVoidType();
			}
			return false;
		}
	}

	public override RefKind RefKind => _refKind;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => _refCustomModifiers;

	public override TypeWithAnnotations ReturnTypeWithAnnotations => _returnType;

	internal override bool IsExplicitInterfaceImplementation => false;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

	public override Symbol? AssociatedSymbol => null;

	public override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotations => ImmutableArray<TypeWithAnnotations>.Empty;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public override int Arity => 0;

	public override ImmutableArray<ParameterSymbol> Parameters => _parameters;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public override ImmutableArray<Location> Locations => ImmutableArray.Create(_syntax.Location);

	internal Location DiagnosticLocation
	{
		get
		{
			SyntaxNode syntax = _syntax;
			if (!(syntax is AnonymousMethodExpressionSyntax { DelegateKeyword: var delegateKeyword }))
			{
				if (syntax is LambdaExpressionSyntax { ArrowToken: var arrowToken })
				{
					return arrowToken.GetLocation();
				}
				return GetFirstLocation();
			}
			return delegateKeyword.GetLocation();
		}
	}

	private bool HasExplicitReturnType
	{
		get
		{
			if (_syntax is ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpressionSyntax)
			{
				return parenthesizedLambdaExpressionSyntax.ReturnType != null;
			}
			return false;
		}
	}

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(syntaxReferenceOpt);

	public override Symbol ContainingSymbol => _containingSymbol;

	internal override CallingConvention CallingConvention => CallingConvention.Default;

	public override bool IsExtensionMethod => false;

	internal override Binder OuterBinder => _binder;

	internal override Binder WithTypeParametersBinder => _binder;

	public override bool IsImplicitlyDeclared => _isSynthesized;

	internal override bool GenerateDebugInfo => true;

	internal override bool IsDeclaredReadOnly => false;

	internal override bool IsInitOnly => false;

	public LambdaSymbol(Binder binder, CSharpCompilation compilation, Symbol containingSymbol, UnboundLambda unboundLambda, ImmutableArray<TypeWithAnnotations> parameterTypes, ImmutableArray<RefKind> parameterRefKinds, RefKind refKind, ImmutableArray<CustomModifier> refCustomModifiers, TypeWithAnnotations returnType)
		: base(unboundLambda.Syntax.GetReference())
	{
		_binder = binder;
		_containingSymbol = containingSymbol;
		_messageID = unboundLambda.Data.MessageID;
		_syntax = unboundLambda.Syntax;
		if (!unboundLambda.HasExplicitReturnType(out _refKind, out _refCustomModifiers, out _returnType))
		{
			_refKind = refKind;
			_refCustomModifiers = refCustomModifiers;
			_returnType = ((!returnType.HasType) ? TypeWithAnnotations.Create(ReturnTypeIsBeingInferred) : returnType);
		}
		_isSynthesized = unboundLambda.WasCompilerGenerated;
		_isAsync = unboundLambda.IsAsync;
		_isStatic = unboundLambda.IsStatic;
		_parameters = MakeParameters(compilation, unboundLambda, parameterTypes, parameterRefKinds);
		_declarationDiagnostics = new DiagnosticBag();
		_declarationDependencies = new HashSet<AssemblySymbol>();
	}

	internal sealed override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	internal sealed override bool IsMetadataVirtual(IsMetadataVirtualOption option = IsMetadataVirtualOption.None)
	{
		return false;
	}

	internal void SetInferredReturnType(RefKind refKind, TypeWithAnnotations inferredReturnType)
	{
		_refKind = refKind;
		_refCustomModifiers = ImmutableArray<CustomModifier>.Empty;
		_returnType = inferredReturnType;
	}

	internal override bool TryGetThisParameter(out ParameterSymbol? thisParameter)
	{
		thisParameter = null;
		return true;
	}

	internal override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		if (!(_syntax is LambdaExpressionSyntax lambdaExpressionSyntax))
		{
			return default(OneOrMany<SyntaxList<AttributeListSyntax>>);
		}
		return OneOrMany.Create(lambdaExpressionSyntax.AttributeLists);
	}

	internal void GetDeclarationDiagnostics(BindingDiagnosticBag addTo)
	{
		foreach (ParameterSymbol parameter in _parameters)
		{
			parameter.ForceComplete(null, null, default(CancellationToken));
		}
		GetAttributes();
		GetReturnTypeAttributes();
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
		AsyncMethodChecks(HasExplicitReturnType, DiagnosticLocation, instance);
		if (!HasExplicitReturnType && HasAsyncMethodBuilderAttribute(out TypeSymbol _))
		{
			addTo.Add(ErrorCode.ERR_BuilderAttributeDisallowed, DiagnosticLocation);
		}
		_declarationDiagnostics.AddRange(instance.DiagnosticBag);
		_declarationDependencies.AddAll(instance.DependenciesBag);
		instance.Free();
		addTo.AddRange(_declarationDiagnostics);
		addTo.AddDependencies((IReadOnlyCollection<AssemblySymbol>?)_declarationDependencies);
	}

	internal override void AddDeclarationDiagnostics(BindingDiagnosticBag diagnostics)
	{
		DiagnosticBag diagnosticBag = diagnostics.DiagnosticBag;
		if (diagnosticBag != null)
		{
			_declarationDiagnostics.AddRange(diagnosticBag);
		}
		ICollection<AssemblySymbol> dependenciesBag = diagnostics.DependenciesBag;
		if (dependenciesBag != null)
		{
			_declarationDependencies.AddAll(dependenciesBag);
		}
	}

	private ImmutableArray<ParameterSymbol> MakeParameters(CSharpCompilation compilation, UnboundLambda unboundLambda, ImmutableArray<TypeWithAnnotations> parameterTypes, ImmutableArray<RefKind> parameterRefKinds)
	{
		if (!unboundLambda.HasSignature || unboundLambda.ParameterCount == 0)
		{
			return parameterTypes.SelectAsArray((TypeWithAnnotations type, int ordinal, (LambdaSymbol owner, ImmutableArray<RefKind> refKinds) arg) => SynthesizedParameterSymbol.Create(arg.owner, type, ordinal, arg.refKinds[ordinal], GeneratedNames.LambdaCopyParameterName(ordinal)), (this, parameterRefKinds));
		}
		ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance(unboundLambda.ParameterCount);
		bool hasExplicitlyTypedParameterList = unboundLambda.HasExplicitlyTypedParameterList;
		int length = parameterTypes.Length;
		for (int num = 0; num < unboundLambda.ParameterCount; num++)
		{
			RefKind refKind = unboundLambda.RefKind(num);
			ScopedKind scope = unboundLambda.DeclaredScope(num);
			ParameterSyntax parameterSyntax = unboundLambda.ParameterSyntax(num);
			TypeWithAnnotations parameterType = (hasExplicitlyTypedParameterList ? unboundLambda.ParameterTypeWithAnnotations(num) : ((num < length) ? parameterTypes[num] : TypeWithAnnotations.Create(new ExtendedErrorTypeSymbol(compilation, string.Empty, 0, null))));
			SyntaxList<AttributeListSyntax> attributeLists = unboundLambda.ParameterAttributes(num);
			string name = unboundLambda.ParameterName(num);
			Location location = unboundLambda.ParameterLocation(num);
			bool hasParamsModifier = parameterSyntax != null && parameterSyntax.Modifiers.Any((SyntaxToken m) => m.IsKind(SyntaxKind.ParamsKeyword));
			LambdaParameterSymbol item = new LambdaParameterSymbol(this, parameterSyntax?.GetReference(), attributeLists, parameterType, num, refKind, scope, name, unboundLambda.ParameterIsDiscard(num), hasParamsModifier, location);
			instance.Add(item);
		}
		return instance.ToImmutableAndFree();
	}

	public sealed override bool Equals(Symbol symbol, TypeCompareKind compareKind)
	{
		if ((object)this == symbol)
		{
			return true;
		}
		if (symbol is LambdaSymbol lambdaSymbol && lambdaSymbol._syntax == _syntax && lambdaSymbol._refKind == _refKind && lambdaSymbol._refCustomModifiers.SequenceEqual(_refCustomModifiers) && TypeSymbol.Equals(lambdaSymbol.ReturnType, base.ReturnType, compareKind) && base.ParameterTypesWithAnnotations.SequenceEqual(lambdaSymbol.ParameterTypesWithAnnotations, compareKind, (TypeWithAnnotations p1, TypeWithAnnotations p2, TypeCompareKind comparison) => p1.Equals(p2, comparison)))
		{
			return lambdaSymbol.ContainingSymbol.Equals(ContainingSymbol, compareKind);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _syntax.GetHashCode();
	}

	public override ImmutableArray<ImmutableArray<TypeWithAnnotations>> GetTypeParameterConstraintTypes()
	{
		return ImmutableArray<ImmutableArray<TypeWithAnnotations>>.Empty;
	}

	public override ImmutableArray<TypeParameterConstraintKind> GetTypeParameterConstraintKinds()
	{
		return ImmutableArray<TypeParameterConstraintKind>.Empty;
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/LambdaSymbol.cs", 425);
	}

	internal override bool IsNullableAnalysisEnabled()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/LambdaSymbol.cs", 428);
	}

	protected override void NoteAttributesComplete(bool forReturnType)
	{
	}
}
