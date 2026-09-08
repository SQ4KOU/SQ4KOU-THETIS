using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SynthesizedGlobalMethodSymbol : MethodSymbol, ISynthesizedGlobalMethodSymbol
{
	private readonly SynthesizedPrivateImplementationDetailsType _privateImplType;

	private TypeSymbol _returnType;

	private ImmutableArray<ParameterSymbol> _parameters;

	private ImmutableArray<TypeParameterSymbol> _typeParameters;

	private readonly string _name;

	public sealed override bool IsImplicitlyDeclared => true;

	internal sealed override bool GenerateDebugInfo => false;

	public sealed override Symbol ContainingSymbol => _privateImplType;

	public sealed override NamedTypeSymbol ContainingType => _privateImplType;

	public PrivateImplementationDetails ContainingPrivateImplementationDetailsType => _privateImplType.PrivateImplementationDetails;

	public override string Name => _name;

	internal override bool HasSpecialName => false;

	internal override MethodImplAttributes ImplementationAttributes => MethodImplAttributes.IL;

	internal override bool RequiresSecurityObject => false;

	public sealed override FlowAnalysisAnnotations ReturnTypeFlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

	public sealed override ImmutableHashSet<string> ReturnNotNullIfParameterNotNull => ImmutableHashSet<string>.Empty;

	public sealed override bool AreLocalsZeroed => ContainingModule.AreLocalsZeroed;

	internal override MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation => null;

	internal override bool HasDeclarativeSecurity => false;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	internal sealed override bool HasSpecialNameAttribute
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedGlobalMethodSymbol.cs", 148);
		}
	}

	public override bool IsVararg => false;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters
	{
		get
		{
			if (_typeParameters.IsDefault)
			{
				return ImmutableArray<TypeParameterSymbol>.Empty;
			}
			return _typeParameters;
		}
	}

	public override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			if (_parameters.IsDefault)
			{
				return ImmutableArray<ParameterSymbol>.Empty;
			}
			return _parameters;
		}
	}

	public override Accessibility DeclaredAccessibility => Accessibility.Internal;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override RefKind RefKind => RefKind.None;

	public override TypeWithAnnotations ReturnTypeWithAnnotations => TypeWithAnnotations.Create(_returnType);

	public sealed override FlowAnalysisAnnotations FlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotations => ImmutableArray<TypeWithAnnotations>.Empty;

	public override Symbol AssociatedSymbol => null;

	public override int Arity => TypeParameters.Length;

	public override bool ReturnsVoid => base.ReturnType.IsVoidType();

	public override MethodKind MethodKind => MethodKind.Ordinary;

	public override bool IsExtern => false;

	public override bool IsSealed => false;

	public override bool IsAbstract => false;

	public override bool IsOverride => false;

	public override bool IsVirtual => false;

	public override bool IsStatic => true;

	public override bool IsAsync => false;

	public override bool HidesBaseMethodsByName => false;

	internal override bool IsMetadataFinal => false;

	public override bool IsExtensionMethod => false;

	internal override CallingConvention CallingConvention
	{
		get
		{
			if (IsGenericMethod)
			{
				return CallingConvention.Generic;
			}
			return CallingConvention.Default;
		}
	}

	internal override bool IsExplicitInterfaceImplementation => false;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

	internal sealed override bool IsDeclaredReadOnly => false;

	internal sealed override bool IsInitOnly => false;

	internal override bool SynthesizesLoweredBoundBody => true;

	protected sealed override bool HasSetsRequiredMembersImpl
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedGlobalMethodSymbol.cs", 355);
		}
	}

	internal sealed override bool HasUnscopedRefAttribute => false;

	internal sealed override bool UseUpdatedEscapeRules => ContainingModule.UseUpdatedEscapeRules;

	internal SynthesizedGlobalMethodSymbol(SynthesizedPrivateImplementationDetailsType privateImplType, string name)
	{
		_privateImplType = privateImplType;
		_name = name;
	}

	internal SynthesizedGlobalMethodSymbol(SynthesizedPrivateImplementationDetailsType privateImplType, TypeSymbol returnType, string name)
		: this(privateImplType, name)
	{
		_returnType = returnType;
		_typeParameters = ImmutableArray<TypeParameterSymbol>.Empty;
	}

	protected void SetReturnType(TypeSymbol returnType)
	{
		_returnType = returnType;
	}

	protected void SetParameters(ImmutableArray<ParameterSymbol> parameters)
	{
		_parameters = parameters;
	}

	protected void SetTypeParameters(ImmutableArray<TypeParameterSymbol> typeParameters)
	{
		_typeParameters = typeParameters;
	}

	public override DllImportData GetDllImportData()
	{
		return null;
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedGlobalMethodSymbol.cs", 138);
	}

	internal sealed override UnmanagedCallersOnlyAttributeData GetUnmanagedCallersOnlyAttributeData(bool forceComplete)
	{
		return null;
	}

	internal sealed override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return ImmutableArray<string>.Empty;
	}

	internal sealed override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	internal sealed override bool IsMetadataVirtual(IsMetadataVirtualOption option = IsMetadataVirtualOption.None)
	{
		return false;
	}

	internal abstract override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics);

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedGlobalMethodSymbol.cs", 350);
	}

	internal sealed override bool IsNullableAnalysisEnabled()
	{
		return false;
	}

	internal sealed override bool HasAsyncMethodBuilderAttribute(out TypeSymbol builderArgument)
	{
		builderArgument = null;
		return false;
	}

	internal sealed override int TryGetOverloadResolutionPriority()
	{
		return 0;
	}
}
