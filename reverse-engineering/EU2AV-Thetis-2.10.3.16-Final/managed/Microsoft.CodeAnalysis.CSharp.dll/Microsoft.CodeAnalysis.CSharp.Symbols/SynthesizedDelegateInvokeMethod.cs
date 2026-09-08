using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedDelegateInvokeMethod : SynthesizedMethodSymbol
{
	internal readonly struct ParameterDescription
	{
		internal readonly TypeWithAnnotations Type;

		internal readonly RefKind RefKind;

		internal readonly ScopedKind Scope;

		internal readonly ConstantValue? DefaultValue;

		internal readonly bool IsParams;

		internal readonly bool HasUnscopedRefAttribute;

		internal ParameterDescription(TypeWithAnnotations type, RefKind refKind, ScopedKind scope, ConstantValue? defaultValue, bool isParams, bool hasUnscopedRefAttribute)
		{
			Type = type;
			RefKind = refKind;
			Scope = scope;
			DefaultValue = defaultValue;
			IsParams = isParams;
			HasUnscopedRefAttribute = hasUnscopedRefAttribute;
		}
	}

	private readonly NamedTypeSymbol _containingType;

	public override string Name => "Invoke";

	internal override bool IsMetadataFinal => false;

	public override MethodKind MethodKind => MethodKind.DelegateInvoke;

	public override int Arity => 0;

	public override bool IsExtensionMethod => false;

	internal override bool HasSpecialName => false;

	internal override MethodImplAttributes ImplementationAttributes => MethodImplAttributes.CodeTypeMask;

	internal override bool HasDeclarativeSecurity => false;

	internal override MarshalPseudoCustomAttributeData? ReturnValueMarshallingInformation => null;

	internal override bool RequiresSecurityObject => false;

	public override bool HidesBaseMethodsByName => false;

	public override bool IsVararg => false;

	public override bool ReturnsVoid => base.ReturnType.IsVoidType();

	public override bool IsAsync => false;

	public override RefKind RefKind { get; }

	public override ImmutableArray<CustomModifier> RefCustomModifiers { get; }

	public override TypeWithAnnotations ReturnTypeWithAnnotations { get; }

	public override FlowAnalysisAnnotations ReturnTypeFlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

	public override ImmutableHashSet<string> ReturnNotNullIfParameterNotNull => ImmutableHashSet<string>.Empty;

	public override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotations => ImmutableArray<TypeWithAnnotations>.Empty;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public override ImmutableArray<ParameterSymbol> Parameters { get; }

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

	public override Symbol? AssociatedSymbol => null;

	internal override Microsoft.Cci.CallingConvention CallingConvention => Microsoft.Cci.CallingConvention.HasThis;

	internal override bool GenerateDebugInfo => false;

	public override Symbol ContainingSymbol => _containingType;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override Accessibility DeclaredAccessibility => Accessibility.Public;

	public override bool IsStatic => false;

	public override bool IsVirtual => true;

	public override bool IsOverride => false;

	public override bool IsAbstract => false;

	public override bool IsSealed => false;

	public override bool IsExtern => false;

	protected sealed override bool HasSetsRequiredMembersImpl
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedDelegateSymbol.cs", 263);
		}
	}

	internal SynthesizedDelegateInvokeMethod(NamedTypeSymbol containingType, ArrayBuilder<ParameterDescription> parameterDescriptions, TypeWithAnnotations returnType, RefKind refKind)
	{
		_containingType = containingType;
		Parameters = parameterDescriptions.SelectAsArrayWithIndex(delegate(ParameterDescription p, int i, (SynthesizedDelegateInvokeMethod Method, int ParameterCount) a)
		{
			SynthesizedDelegateInvokeMethod item = a.Method;
			TypeWithAnnotations type = p.Type;
			RefKind refKind2 = p.RefKind;
			string name = GeneratedNames.AnonymousDelegateParameterName(i, a.ParameterCount);
			ScopedKind scope = p.Scope;
			ConstantValue? defaultValue = p.DefaultValue;
			bool isParams = p.IsParams;
			bool hasUnscopedRefAttribute = p.HasUnscopedRefAttribute;
			return SynthesizedParameterSymbol.Create(item, type, i, refKind2, name, scope, defaultValue, default(ImmutableArray<CustomModifier>), null, isParams, hasUnscopedRefAttribute);
		}, (this, parameterDescriptions.Count));
		ReturnTypeWithAnnotations = returnType;
		RefKind = refKind;
		RefCustomModifiers = ((refKind == RefKind.In) ? ImmutableCollectionsMarshal.AsImmutableArray(new CustomModifier[1] { CSharpCustomModifier.CreateRequired(DeclaringCompilation.GetWellKnownType(WellKnownType.System_Runtime_InteropServices_InAttribute)) }) : ImmutableArray<CustomModifier>.Empty);
	}

	internal override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		return true;
	}

	internal override bool IsMetadataVirtual(IsMetadataVirtualOption option = IsMetadataVirtualOption.None)
	{
		return true;
	}

	public override DllImportData? GetDllImportData()
	{
		return null;
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedDelegateSymbol.cs", 133);
	}

	internal override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return ImmutableArray<string>.Empty;
	}
}
