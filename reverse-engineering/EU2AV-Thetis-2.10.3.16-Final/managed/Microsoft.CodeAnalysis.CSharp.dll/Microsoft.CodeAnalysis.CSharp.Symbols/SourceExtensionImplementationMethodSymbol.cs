using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceExtensionImplementationMethodSymbol : RewrittenMethodSymbol
{
	private sealed class ExtensionMetadataMethodParameterSymbol : RewrittenMethodParameterSymbolBase
	{
		public override bool IsImplicitlyDeclared => true;

		public override int Ordinal => GetImplementationParameterOrdinal(_underlyingParameter);

		internal sealed override ImmutableArray<int> InterpolatedStringHandlerArgumentIndexes
		{
			get
			{
				ImmutableArray<int> interpolatedStringHandlerArgumentIndexes = _underlyingParameter.InterpolatedStringHandlerArgumentIndexes;
				if (interpolatedStringHandlerArgumentIndexes.IsDefaultOrEmpty || _underlyingParameter.ContainingSymbol.IsStatic)
				{
					return interpolatedStringHandlerArgumentIndexes;
				}
				return interpolatedStringHandlerArgumentIndexes.SelectAsArray((int index) => (index < 0) ? (index switch
				{
					-1 => throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Extensions/SourceExtensionImplementationMethodSymbol.cs", 226), 
					-2 => 0, 
					-3 => -3, 
					-4 => -4, 
					_ => throw ExceptionUtilities.UnexpectedValue(index), 
				}) : (index + 1));
			}
		}

		internal sealed override bool HasInterpolatedStringHandlerArgumentError => _underlyingParameter.HasInterpolatedStringHandlerArgumentError;

		public ExtensionMetadataMethodParameterSymbol(SourceExtensionImplementationMethodSymbol containingMethod, ParameterSymbol sourceParameter)
			: base(containingMethod, sourceParameter)
		{
		}

		internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
		{
			if (_underlyingParameter is SynthesizedAccessorValueParameterSymbol synthesizedAccessorValueParameterSymbol)
			{
				synthesizedAccessorValueParameterSymbol.AddSynthesizedFlowAnalysisAttributes(ref attributes);
			}
			SourceParameterSymbolBase.AddSynthesizedAttributes(this, moduleBuilder, ref attributes);
		}
	}

	private string? lazyDocComment;

	private StrongBox<byte?>? lazyNullableContext;

	public override int Arity => TypeParameters.Length;

	public override bool IsGenericMethod => Arity != 0;

	public override MethodKind MethodKind => MethodKind.Ordinary;

	public override bool IsImplicitlyDeclared => true;

	internal override bool HasSpecialName => _originalMethod.HasSpecialNameAttribute;

	internal override int ParameterCount => _originalMethod.ParameterCount + ((!_originalMethod.IsStatic) ? 1 : 0);

	public sealed override bool IsExtensionMethod
	{
		get
		{
			if (!_originalMethod.IsStatic)
			{
				return _originalMethod.MethodKind == MethodKind.Ordinary;
			}
			return false;
		}
	}

	public sealed override bool IsVirtual => false;

	public sealed override bool IsOverride => false;

	public sealed override bool IsAbstract => false;

	public sealed override bool IsSealed => false;

	internal sealed override bool IsMetadataFinal => false;

	internal sealed override bool IsAccessCheckedOnOverride => false;

	public sealed override bool IsExtern => _originalMethod.IsExtern;

	internal sealed override bool IsExternal => _originalMethod.IsExternal;

	internal sealed override bool IsDeclaredReadOnly => false;

	public sealed override Symbol ContainingSymbol => _originalMethod.ContainingType.ContainingSymbol;

	public override bool IsStatic => true;

	public override bool RequiresInstanceReceiver => false;

	internal override CallingConvention CallingConvention => (CallingConvention)((int)(_originalMethod.CallingConvention & ~CallingConvention.HasThis) | ((Arity != 0) ? 16 : 0));

	public override Symbol? AssociatedSymbol => null;

	public SourceExtensionImplementationMethodSymbol(MethodSymbol sourceMethod)
		: base(sourceMethod, Microsoft.CodeAnalysis.CSharp.Symbols.TypeMap.Empty, sourceMethod.ContainingType.TypeParameters.Concat(sourceMethod.TypeParameters))
	{
	}

	internal sealed override bool IsMetadataVirtual(IsMetadataVirtualOption option = IsMetadataVirtualOption.None)
	{
		return false;
	}

	internal sealed override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	public sealed override DllImportData? GetDllImportData()
	{
		return _originalMethod.GetDllImportData();
	}

	internal sealed override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		if (_originalMethod is SourcePropertyAccessorSymbol { AssociatedSymbol: SourcePropertySymbolBase associatedSymbol })
		{
			foreach (CSharpAttributeData attribute in associatedSymbol.GetAttributes())
			{
				if (attribute.IsTargetAttribute(AttributeDescription.OverloadResolutionPriorityAttribute))
				{
					Symbol.AddSynthesizedAttribute(ref attributes, attribute);
				}
			}
		}
		SourceMethodSymbol.AddSynthesizedAttributes(this, moduleBuilder, ref attributes);
	}

	internal override void AddSynthesizedReturnTypeAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		if (_originalMethod is SourcePropertyAccessorSymbol sourcePropertyAccessorSymbol)
		{
			sourcePropertyAccessorSymbol.AddSynthesizedReturnTypeFlowAnalysisAttributes(ref attributes);
		}
		base.AddSynthesizedReturnTypeAttributes(moduleBuilder, ref attributes);
	}

	internal override byte? GetLocalNullableContextValue()
	{
		if (lazyNullableContext == null)
		{
			byte? value = SourceMemberMethodSymbol.ComputeNullableContextValue(this);
			Interlocked.CompareExchange(ref lazyNullableContext, new StrongBox<byte?>(value), null);
		}
		return lazyNullableContext.Value;
	}

	protected override ImmutableArray<ParameterSymbol> MakeParameters()
	{
		ImmutableArray<ParameterSymbol> parameters = _originalMethod.Parameters;
		ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance(ParameterCount);
		if (!_originalMethod.IsStatic)
		{
			instance.Add(new ExtensionMetadataMethodParameterSymbol(this, ((SourceNamedTypeSymbol)_originalMethod.ContainingType).ExtensionParameter));
		}
		foreach (ParameterSymbol item in parameters)
		{
			instance.Add(new ExtensionMetadataMethodParameterSymbol(this, item));
		}
		return instance.ToImmutableAndFree();
	}

	internal static int GetImplementationParameterOrdinal(ParameterSymbol underlyingParameter)
	{
		if (underlyingParameter.ContainingSymbol is NamedTypeSymbol)
		{
			return 0;
		}
		int ordinal = underlyingParameter.Ordinal;
		if (underlyingParameter.ContainingSymbol.IsStatic)
		{
			return ordinal;
		}
		return ordinal + 1;
	}

	internal override bool TryGetThisParameter(out ParameterSymbol? thisParameter)
	{
		thisParameter = null;
		return true;
	}

	internal override int TryGetOverloadResolutionPriority()
	{
		if (UnderlyingMethod is SourcePropertyAccessorSymbol { AssociatedSymbol: SourcePropertySymbol associatedSymbol })
		{
			return associatedSymbol.TryGetOverloadResolutionPriority();
		}
		return UnderlyingMethod.TryGetOverloadResolutionPriority();
	}

	public override string GetDocumentationCommentXml(CultureInfo? preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return SourceDocumentationCommentUtils.GetAndCacheDocumentationComment(this, expandIncludes: false, ref lazyDocComment);
	}
}
