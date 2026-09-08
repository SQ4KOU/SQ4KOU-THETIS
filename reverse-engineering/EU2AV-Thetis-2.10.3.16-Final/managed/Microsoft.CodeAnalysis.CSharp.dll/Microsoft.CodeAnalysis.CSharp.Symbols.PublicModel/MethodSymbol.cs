using System;
using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel;

internal sealed class MethodSymbol : Symbol, IMethodSymbol, ISymbol, IEquatable<ISymbol?>
{
	private readonly Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol _underlying;

	private ITypeSymbol _lazyReturnType;

	private ImmutableArray<ITypeSymbol> _lazyTypeArguments;

	private ImmutableArray<IParameterSymbol> _lazyParameters;

	private ITypeSymbol _lazyReceiverType;

	internal override Microsoft.CodeAnalysis.CSharp.Symbol UnderlyingSymbol => _underlying;

	internal Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol UnderlyingMethodSymbol => _underlying;

	MethodKind IMethodSymbol.MethodKind => _underlying.MethodKind switch
	{
		MethodKind.AnonymousFunction => MethodKind.AnonymousFunction, 
		MethodKind.Constructor => MethodKind.Constructor, 
		MethodKind.Conversion => MethodKind.Conversion, 
		MethodKind.DelegateInvoke => MethodKind.DelegateInvoke, 
		MethodKind.Destructor => MethodKind.Destructor, 
		MethodKind.EventAdd => MethodKind.EventAdd, 
		MethodKind.EventRemove => MethodKind.EventRemove, 
		MethodKind.ExplicitInterfaceImplementation => MethodKind.ExplicitInterfaceImplementation, 
		MethodKind.UserDefinedOperator => MethodKind.UserDefinedOperator, 
		MethodKind.BuiltinOperator => MethodKind.BuiltinOperator, 
		MethodKind.Ordinary => MethodKind.Ordinary, 
		MethodKind.PropertyGet => MethodKind.PropertyGet, 
		MethodKind.PropertySet => MethodKind.PropertySet, 
		MethodKind.ReducedExtension => MethodKind.ReducedExtension, 
		MethodKind.StaticConstructor => MethodKind.StaticConstructor, 
		MethodKind.LocalFunction => MethodKind.LocalFunction, 
		MethodKind.FunctionPointerSignature => MethodKind.FunctionPointerSignature, 
		_ => throw ExceptionUtilities.UnexpectedValue(_underlying.MethodKind), 
	};

	ITypeSymbol IMethodSymbol.ReturnType
	{
		get
		{
			if (_lazyReturnType == null)
			{
				Interlocked.CompareExchange(ref _lazyReturnType, _underlying.ReturnTypeWithAnnotations.GetPublicSymbol(), null);
			}
			return _lazyReturnType;
		}
	}

	Microsoft.CodeAnalysis.NullableAnnotation IMethodSymbol.ReturnNullableAnnotation => _underlying.ReturnTypeWithAnnotations.ToPublicAnnotation();

	ImmutableArray<ITypeSymbol> IMethodSymbol.TypeArguments => InterlockedOperations.Initialize(ref _lazyTypeArguments, (Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol underlying) => underlying.TypeArgumentsWithAnnotations.GetPublicSymbols(), _underlying);

	ImmutableArray<Microsoft.CodeAnalysis.NullableAnnotation> IMethodSymbol.TypeArgumentNullableAnnotations => _underlying.TypeArgumentsWithAnnotations.ToPublicAnnotations();

	ImmutableArray<ITypeParameterSymbol> IMethodSymbol.TypeParameters => _underlying.TypeParameters.GetPublicSymbols();

	ImmutableArray<IParameterSymbol> IMethodSymbol.Parameters => InterlockedOperations.Initialize(ref _lazyParameters, (Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol underlying) => underlying.Parameters.GetPublicSymbols(), _underlying);

	IMethodSymbol IMethodSymbol.ConstructedFrom => _underlying.ConstructedFrom.GetPublicSymbol();

	bool IMethodSymbol.IsReadOnly => _underlying.IsEffectivelyReadOnly;

	bool IMethodSymbol.IsInitOnly => _underlying.IsInitOnly;

	IMethodSymbol IMethodSymbol.OriginalDefinition => _underlying.OriginalDefinition.GetPublicSymbol();

	IMethodSymbol IMethodSymbol.OverriddenMethod => _underlying.OverriddenMethod.GetPublicSymbol();

	ITypeSymbol IMethodSymbol.ReceiverType
	{
		get
		{
			if (_lazyReceiverType == null)
			{
				Interlocked.CompareExchange(ref _lazyReceiverType, _underlying.ReceiverType?.GetITypeSymbol(_underlying.ReceiverNullableAnnotation), null);
			}
			return _lazyReceiverType;
		}
	}

	Microsoft.CodeAnalysis.NullableAnnotation IMethodSymbol.ReceiverNullableAnnotation => _underlying.ReceiverNullableAnnotation;

	IMethodSymbol IMethodSymbol.ReducedFrom => _underlying.ReducedFrom.GetPublicSymbol();

	ImmutableArray<IMethodSymbol> IMethodSymbol.ExplicitInterfaceImplementations => _underlying.ExplicitInterfaceImplementations.GetPublicSymbols();

	ISymbol IMethodSymbol.AssociatedSymbol => _underlying.AssociatedSymbol.GetPublicSymbol();

	bool IMethodSymbol.IsGenericMethod => _underlying.IsGenericMethod;

	bool IMethodSymbol.IsAsync => _underlying.IsAsync;

	bool IMethodSymbol.HidesBaseMethodsByName => _underlying.HidesBaseMethodsByName;

	ImmutableArray<CustomModifier> IMethodSymbol.ReturnTypeCustomModifiers => _underlying.ReturnTypeWithAnnotations.CustomModifiers;

	ImmutableArray<CustomModifier> IMethodSymbol.RefCustomModifiers => _underlying.RefCustomModifiers;

	SignatureCallingConvention IMethodSymbol.CallingConvention => _underlying.CallingConvention.ToSignatureConvention();

	ImmutableArray<INamedTypeSymbol> IMethodSymbol.UnmanagedCallingConventionTypes => _underlying.UnmanagedCallingConventionTypes.SelectAsArray((Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol t) => t.GetPublicSymbol());

	IMethodSymbol IMethodSymbol.PartialImplementationPart => _underlying.PartialImplementationPart.GetPublicSymbol();

	IMethodSymbol IMethodSymbol.PartialDefinitionPart => _underlying.PartialDefinitionPart.GetPublicSymbol();

	bool IMethodSymbol.IsPartialDefinition
	{
		get
		{
			if (_underlying.IsDefinition)
			{
				return _underlying.IsPartialDefinition();
			}
			return false;
		}
	}

	INamedTypeSymbol IMethodSymbol.AssociatedAnonymousDelegate => null;

	int IMethodSymbol.Arity => _underlying.Arity;

	bool IMethodSymbol.IsExtensionMethod => _underlying.IsExtensionMethod;

	MethodImplAttributes IMethodSymbol.MethodImplementationFlags => _underlying.ImplementationAttributes;

	bool IMethodSymbol.IsVararg => _underlying.IsVararg;

	bool IMethodSymbol.IsCheckedBuiltin => _underlying.IsCheckedBuiltin;

	bool IMethodSymbol.ReturnsVoid => _underlying.ReturnsVoid;

	bool IMethodSymbol.ReturnsByRef => _underlying.ReturnsByRef;

	bool IMethodSymbol.ReturnsByRefReadonly => _underlying.ReturnsByRefReadonly;

	RefKind IMethodSymbol.RefKind => _underlying.RefKind;

	bool IMethodSymbol.IsConditional => _underlying.IsConditional;

	bool IMethodSymbol.IsIterator => _underlying.IsIterator;

	IMethodSymbol? IMethodSymbol.AssociatedExtensionImplementation
	{
		get
		{
			if (!_underlying.IsExtensionBlockMember())
			{
				return null;
			}
			Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol methodSymbol = _underlying.OriginalDefinition.TryGetCorrespondingExtensionImplementationMethod();
			if ((object)methodSymbol == null)
			{
				return null;
			}
			Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol containingType = _underlying.ContainingType.ContainingType;
			Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol methodSymbol2 = methodSymbol.AsMember(containingType);
			if (methodSymbol2.Arity != 0)
			{
				ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance(methodSymbol2.Arity);
				instance.AddRange(_underlying.ContainingType.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics);
				instance.AddRange(_underlying.TypeArgumentsWithAnnotations);
				methodSymbol2 = methodSymbol2.Construct(instance.ToImmutableAndFree());
			}
			return methodSymbol2.GetPublicSymbol();
		}
	}

	public MethodSymbol(Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol underlying)
	{
		_underlying = underlying;
	}

	ITypeSymbol IMethodSymbol.GetTypeInferredDuringReduction(ITypeParameterSymbol reducedFromTypeParameter)
	{
		return _underlying.GetTypeInferredDuringReduction(reducedFromTypeParameter.EnsureCSharpSymbolOrNull("reducedFromTypeParameter")).GetPublicSymbol();
	}

	IMethodSymbol? IMethodSymbol.ReduceExtensionMethod(ITypeSymbol receiverType)
	{
		return _underlying.ReduceExtensionMethod(receiverType.EnsureCSharpSymbolOrNull("receiverType"), null).GetPublicSymbol();
	}

	IMethodSymbol? IMethodSymbol.ReduceExtensionMember(ITypeSymbol receiverType)
	{
		if (_underlying.IsExtensionBlockMember() && SourceMemberContainerTypeSymbol.IsAllowedExtensionMember(_underlying))
		{
			Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol receiverType2 = receiverType.EnsureCSharpSymbolOrNull("receiverType");
			bool wasExtensionFullyInferred;
			return (IMethodSymbol)SourceNamedTypeSymbol.ReduceExtensionMember(null, _underlying, receiverType2, out wasExtensionFullyInferred).GetPublicSymbol();
		}
		return null;
	}

	ImmutableArray<AttributeData> IMethodSymbol.GetReturnTypeAttributes()
	{
		return _underlying.GetReturnTypeAttributes().Cast<CSharpAttributeData, AttributeData>();
	}

	IMethodSymbol IMethodSymbol.Construct(params ITypeSymbol[] typeArguments)
	{
		return _underlying.Construct(Symbol.ConstructTypeArguments(typeArguments)).GetPublicSymbol();
	}

	IMethodSymbol IMethodSymbol.Construct(ImmutableArray<ITypeSymbol> typeArguments, ImmutableArray<Microsoft.CodeAnalysis.NullableAnnotation> typeArgumentNullableAnnotations)
	{
		return _underlying.Construct(Symbol.ConstructTypeArguments(typeArguments, typeArgumentNullableAnnotations)).GetPublicSymbol();
	}

	DllImportData IMethodSymbol.GetDllImportData()
	{
		return _underlying.GetDllImportData();
	}

	protected override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitMethod(this);
	}

	protected override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitMethod(this);
	}

	protected override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitMethod(this, argument);
	}
}
