using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SynthesizedImplementationMethod : SynthesizedMethodSymbol
{
	protected readonly MethodSymbol _interfaceMethod;

	private readonly NamedTypeSymbol _implementingType;

	private readonly bool _generateDebugInfo;

	private readonly PropertySymbol _associatedProperty;

	private readonly ImmutableArray<MethodSymbol> _explicitInterfaceImplementations;

	private readonly ImmutableArray<TypeParameterSymbol> _typeParameters;

	private readonly ImmutableArray<ParameterSymbol> _parameters;

	private readonly string _name;

	public sealed override bool IsVararg => _interfaceMethod.IsVararg;

	public sealed override int Arity => _interfaceMethod.Arity;

	public sealed override bool ReturnsVoid => _interfaceMethod.ReturnsVoid;

	internal sealed override CallingConvention CallingConvention => _interfaceMethod.CallingConvention;

	public sealed override ImmutableArray<CustomModifier> RefCustomModifiers => _interfaceMethod.RefCustomModifiers;

	internal sealed override bool GenerateDebugInfo => _generateDebugInfo;

	public sealed override ImmutableArray<TypeParameterSymbol> TypeParameters => _typeParameters;

	public sealed override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotations => GetTypeParametersAsTypeArguments();

	public sealed override RefKind RefKind => _interfaceMethod.RefKind;

	public sealed override TypeWithAnnotations ReturnTypeWithAnnotations => _interfaceMethod.ReturnTypeWithAnnotations;

	public sealed override FlowAnalysisAnnotations ReturnTypeFlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

	public sealed override ImmutableHashSet<string> ReturnNotNullIfParameterNotNull => ImmutableHashSet<string>.Empty;

	public sealed override ImmutableArray<ParameterSymbol> Parameters => _parameters;

	public sealed override Symbol ContainingSymbol => _implementingType;

	public sealed override NamedTypeSymbol ContainingType => _implementingType;

	internal sealed override bool IsExplicitInterfaceImplementation => true;

	public sealed override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => _explicitInterfaceImplementations;

	public override MethodKind MethodKind => MethodKind.ExplicitInterfaceImplementation;

	public sealed override Accessibility DeclaredAccessibility => Accessibility.Private;

	public sealed override Symbol AssociatedSymbol => _associatedProperty;

	public sealed override bool HidesBaseMethodsByName => false;

	public sealed override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override bool IsStatic => false;

	public sealed override bool IsAsync => false;

	public sealed override bool IsVirtual => false;

	public sealed override bool IsOverride => false;

	public sealed override bool IsAbstract => false;

	public sealed override bool IsSealed => false;

	public sealed override bool IsExtern => false;

	public sealed override bool IsExtensionMethod => false;

	public sealed override string Name => _name;

	internal override bool HasSpecialName => _interfaceMethod.HasSpecialName;

	internal sealed override MethodImplAttributes ImplementationAttributes => MethodImplAttributes.IL;

	internal sealed override bool RequiresSecurityObject => _interfaceMethod.RequiresSecurityObject;

	internal sealed override bool IsMetadataFinal => !IsStatic;

	internal sealed override MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation => null;

	internal sealed override bool HasDeclarativeSecurity => false;

	protected sealed override bool HasSetsRequiredMembersImpl
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedImplementationMethod.cs", 269);
		}
	}

	public SynthesizedImplementationMethod(MethodSymbol interfaceMethod, NamedTypeSymbol implementingType, string name = null, bool generateDebugInfo = true, PropertySymbol associatedProperty = null)
	{
		_name = name ?? ExplicitInterfaceHelpers.GetMemberName(interfaceMethod.Name, interfaceMethod.ContainingType, null);
		_implementingType = implementingType;
		_generateDebugInfo = generateDebugInfo;
		_associatedProperty = associatedProperty;
		_explicitInterfaceImplementations = ImmutableArray.Create(interfaceMethod);
		(interfaceMethod.ContainingType.TypeSubstitution ?? TypeMap.Empty).WithAlphaRename(interfaceMethod, this, propagateAttributes: false, out _typeParameters);
		_interfaceMethod = interfaceMethod.ConstructIfGeneric(TypeArgumentsWithAnnotations);
		_parameters = SynthesizedParameterSymbol.DeriveParameters(_interfaceMethod, this);
	}

	internal sealed override bool IsMetadataVirtual(IsMetadataVirtualOption option = IsMetadataVirtualOption.None)
	{
		return !IsStatic;
	}

	internal sealed override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		return !IsStatic;
	}

	public sealed override DllImportData GetDllImportData()
	{
		return null;
	}

	internal sealed override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedImplementationMethod.cs", 261);
	}

	internal sealed override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return ImmutableArray<string>.Empty;
	}
}
