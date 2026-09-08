using System;
using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class NativeIntegerMethodSymbol : WrappedMethodSymbol, IReference
{
	private readonly NativeIntegerTypeSymbol _container;

	private readonly NativeIntegerPropertySymbol? _associatedSymbol;

	private ImmutableArray<ParameterSymbol> _lazyParameters;

	public override Symbol ContainingSymbol => _container;

	public override MethodSymbol UnderlyingMethod { get; }

	public override TypeWithAnnotations ReturnTypeWithAnnotations => _container.SubstituteUnderlyingType(UnderlyingMethod.ReturnTypeWithAnnotations);

	public override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotations => ImmutableArray<TypeWithAnnotations>.Empty;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			if (_lazyParameters.IsDefault)
			{
				ImmutableArray<ParameterSymbol> value = UnderlyingMethod.Parameters.SelectAsArray((Func<ParameterSymbol, NativeIntegerMethodSymbol, ParameterSymbol>)((ParameterSymbol p, NativeIntegerMethodSymbol m) => new NativeIntegerParameterSymbol(m._container, m, p)), this);
				ImmutableInterlocked.InterlockedInitialize(ref _lazyParameters, value);
			}
			return _lazyParameters;
		}
	}

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => UnderlyingMethod.RefCustomModifiers;

	internal sealed override bool HasSpecialNameAttribute
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/NativeIntegerTypeSymbol.cs", 396);
		}
	}

	public override Symbol? AssociatedSymbol => _associatedSymbol;

	internal NativeIntegerMethodSymbol(NativeIntegerTypeSymbol container, MethodSymbol underlyingMethod, NativeIntegerPropertySymbol? associatedSymbol)
	{
		_container = container;
		_associatedSymbol = associatedSymbol;
		UnderlyingMethod = underlyingMethod;
	}

	internal override bool TryGetThisParameter(out ParameterSymbol? thisParameter)
	{
		if (UnderlyingMethod.TryGetThisParameter(out ParameterSymbol thisParameter2))
		{
			thisParameter = ((thisParameter2 != null) ? new ThisParameterSymbol(this, _container) : null);
			return true;
		}
		thisParameter = null;
		return false;
	}

	internal override UnmanagedCallersOnlyAttributeData? GetUnmanagedCallersOnlyAttributeData(bool forceComplete)
	{
		return UnderlyingMethod.GetUnmanagedCallersOnlyAttributeData(forceComplete);
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/NativeIntegerTypeSymbol.cs", 402);
	}

	internal override bool IsNullableAnalysisEnabled()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/NativeIntegerTypeSymbol.cs", 405);
	}

	public override bool Equals(Symbol? other, TypeCompareKind comparison)
	{
		return NativeIntegerTypeSymbol.EqualsHelper(this, other, comparison, (NativeIntegerMethodSymbol symbol) => symbol.UnderlyingMethod);
	}

	public override int GetHashCode()
	{
		return UnderlyingMethod.GetHashCode();
	}

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/NativeIntegerTypeSymbol.cs", 415);
	}

	internal sealed override bool HasAsyncMethodBuilderAttribute(out TypeSymbol? builderArgument)
	{
		builderArgument = null;
		return false;
	}

	internal override int TryGetOverloadResolutionPriority()
	{
		return UnderlyingMethod.TryGetOverloadResolutionPriority();
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/NativeIntegerTypeSymbol.cs", 429);
	}
}
