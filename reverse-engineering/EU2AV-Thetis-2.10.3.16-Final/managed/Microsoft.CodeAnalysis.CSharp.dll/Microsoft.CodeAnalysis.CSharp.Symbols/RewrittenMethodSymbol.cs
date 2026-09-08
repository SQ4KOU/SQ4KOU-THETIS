using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class RewrittenMethodSymbol : WrappedMethodSymbol
{
	protected sealed class RewrittenMethodParameterSymbol : RewrittenMethodParameterSymbolBase
	{
		internal RewrittenMethodParameterSymbol(RewrittenMethodSymbol containingMethod, ParameterSymbol originalParameter)
			: base(containingMethod, originalParameter)
		{
		}

		internal sealed override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Extensions/RewrittenMethodSymbol.cs", 128);
		}
	}

	protected abstract class RewrittenMethodParameterSymbolBase : RewrittenParameterSymbol
	{
		protected readonly RewrittenMethodSymbol _containingMethod;

		public sealed override Symbol ContainingSymbol => _containingMethod;

		public override TypeWithAnnotations TypeWithAnnotations => _containingMethod._typeMap.SubstituteType(_underlyingParameter.TypeWithAnnotations);

		public override ImmutableArray<CustomModifier> RefCustomModifiers => _containingMethod._typeMap.SubstituteCustomModifiers(_underlyingParameter.RefCustomModifiers);

		internal sealed override bool HasEnumeratorCancellationAttribute => _underlyingParameter.HasEnumeratorCancellationAttribute;

		protected RewrittenMethodParameterSymbolBase(RewrittenMethodSymbol containingMethod, ParameterSymbol originalParameter)
			: base(originalParameter)
		{
			_containingMethod = containingMethod;
		}
	}

	protected readonly MethodSymbol _originalMethod;

	private readonly TypeMap _typeMap;

	private readonly ImmutableArray<TypeParameterSymbol> _typeParameters;

	private ImmutableArray<ParameterSymbol> _lazyParameters;

	public TypeMap TypeMap => _typeMap;

	public sealed override MethodSymbol UnderlyingMethod => _originalMethod;

	public sealed override ImmutableArray<TypeParameterSymbol> TypeParameters => _typeParameters;

	public sealed override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotations => GetTypeParametersAsTypeArguments();

	public sealed override TypeWithAnnotations ReturnTypeWithAnnotations => _typeMap.SubstituteType(_originalMethod.ReturnTypeWithAnnotations);

	internal override TypeWithAnnotations IteratorElementTypeWithAnnotations
	{
		get
		{
			TypeWithAnnotations iteratorElementTypeWithAnnotations = _originalMethod.IteratorElementTypeWithAnnotations;
			if (iteratorElementTypeWithAnnotations.HasType)
			{
				return _typeMap.SubstituteType(iteratorElementTypeWithAnnotations);
			}
			return iteratorElementTypeWithAnnotations;
		}
	}

	internal override bool IsIterator => _originalMethod.IsIterator;

	public sealed override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

	internal sealed override bool HasSpecialNameAttribute
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Extensions/RewrittenMethodSymbol.cs", 79);
		}
	}

	public sealed override ImmutableArray<CustomModifier> RefCustomModifiers => _typeMap.SubstituteCustomModifiers(_originalMethod.RefCustomModifiers);

	public sealed override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			if (_lazyParameters.IsDefault)
			{
				ImmutableInterlocked.InterlockedInitialize(ref _lazyParameters, MakeParameters());
			}
			return _lazyParameters;
		}
	}

	protected RewrittenMethodSymbol(MethodSymbol originalMethod, TypeMap typeMap, ImmutableArray<TypeParameterSymbol> typeParametersToAlphaRename)
	{
		_originalMethod = originalMethod;
		_typeMap = typeMap.WithAlphaRename(typeParametersToAlphaRename, this, propagateAttributes: true, out _typeParameters);
	}

	internal sealed override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		return _originalMethod.CalculateLocalSyntaxOffset(localPosition, localTree);
	}

	internal sealed override bool IsNullableAnalysisEnabled()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Extensions/RewrittenMethodSymbol.cs", 72);
	}

	internal sealed override UnmanagedCallersOnlyAttributeData? GetUnmanagedCallersOnlyAttributeData(bool forceComplete)
	{
		return _originalMethod.GetUnmanagedCallersOnlyAttributeData(forceComplete);
	}

	public sealed override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return _originalMethod.GetAttributes();
	}

	public sealed override ImmutableArray<CSharpAttributeData> GetReturnTypeAttributes()
	{
		return _originalMethod.GetReturnTypeAttributes();
	}

	internal sealed override UseSiteInfo<AssemblySymbol> GetUseSiteInfo()
	{
		return _originalMethod.GetUseSiteInfo();
	}

	protected abstract ImmutableArray<ParameterSymbol> MakeParameters();

	internal sealed override bool HasAsyncMethodBuilderAttribute(out TypeSymbol? builderArgument)
	{
		return _originalMethod.HasAsyncMethodBuilderAttribute(out builderArgument);
	}
}
