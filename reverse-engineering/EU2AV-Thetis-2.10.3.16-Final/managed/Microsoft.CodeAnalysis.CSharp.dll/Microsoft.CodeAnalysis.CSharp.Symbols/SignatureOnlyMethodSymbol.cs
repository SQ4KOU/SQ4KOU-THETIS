using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SignatureOnlyMethodSymbol : MethodSymbol
{
	private readonly string _name;

	private readonly TypeSymbol _containingType;

	private readonly MethodKind _methodKind;

	private readonly CallingConvention _callingConvention;

	private readonly ImmutableArray<TypeParameterSymbol> _typeParameters;

	private readonly ImmutableArray<ParameterSymbol> _parameters;

	private readonly RefKind _refKind;

	private readonly bool _isInitOnly;

	private readonly bool _isStatic;

	private readonly TypeWithAnnotations _returnType;

	private readonly ImmutableArray<CustomModifier> _refCustomModifiers;

	private readonly ImmutableArray<MethodSymbol> _explicitInterfaceImplementations;

	internal override CallingConvention CallingConvention => _callingConvention;

	public override bool IsVararg => new SignatureHeader((byte)_callingConvention).CallingConvention == SignatureCallingConvention.VarArgs;

	public override bool IsGenericMethod => Arity > 0;

	public override int Arity => _typeParameters.Length;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => _typeParameters;

	public override bool ReturnsVoid => _returnType.IsVoidType();

	public override RefKind RefKind => _refKind;

	public override TypeWithAnnotations ReturnTypeWithAnnotations => _returnType;

	public override FlowAnalysisAnnotations ReturnTypeFlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

	public override ImmutableHashSet<string> ReturnNotNullIfParameterNotNull => ImmutableHashSet<string>.Empty;

	public override FlowAnalysisAnnotations FlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => _refCustomModifiers;

	public override ImmutableArray<ParameterSymbol> Parameters => _parameters;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => _explicitInterfaceImplementations;

	public override Symbol ContainingSymbol => _containingType;

	public override MethodKind MethodKind => _methodKind;

	public override string Name => _name;

	internal override bool GenerateDebugInfo
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 107);
		}
	}

	internal override bool HasSpecialName
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 109);
		}
	}

	internal override MethodImplAttributes ImplementationAttributes
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 111);
		}
	}

	internal override bool RequiresSecurityObject
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 113);
		}
	}

	internal override MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 117);
		}
	}

	internal override bool HasDeclarativeSecurity
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 119);
		}
	}

	internal override ObsoleteAttributeData ObsoleteAttributeData
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 123);
		}
	}

	internal sealed override bool HasSpecialNameAttribute
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 127);
		}
	}

	public override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotations
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 131);
		}
	}

	public override Symbol AssociatedSymbol
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 133);
		}
	}

	public override bool IsExtensionMethod
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 135);
		}
	}

	public override bool HidesBaseMethodsByName
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 137);
		}
	}

	public override ImmutableArray<Location> Locations
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 139);
		}
	}

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 141);
		}
	}

	public override Accessibility DeclaredAccessibility
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 143);
		}
	}

	public override bool IsStatic => _isStatic;

	public override bool IsAsync
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 147);
		}
	}

	public override bool IsVirtual
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 149);
		}
	}

	public override bool IsOverride
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 151);
		}
	}

	public override bool IsAbstract
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 153);
		}
	}

	public override bool IsSealed
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 155);
		}
	}

	public override bool IsExtern
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 157);
		}
	}

	public override bool AreLocalsZeroed
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 159);
		}
	}

	public override AssemblySymbol ContainingAssembly
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 161);
		}
	}

	internal override ModuleSymbol ContainingModule
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 163);
		}
	}

	internal override bool IsMetadataFinal
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 173);
		}
	}

	internal override bool IsDeclaredReadOnly => false;

	internal override bool IsInitOnly => _isInitOnly;

	protected sealed override bool HasSetsRequiredMembersImpl
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 183);
		}
	}

	internal sealed override bool HasUnscopedRefAttribute => false;

	internal sealed override bool UseUpdatedEscapeRules => true;

	public SignatureOnlyMethodSymbol(string name, TypeSymbol containingType, MethodKind methodKind, CallingConvention callingConvention, ImmutableArray<TypeParameterSymbol> typeParameters, ImmutableArray<ParameterSymbol> parameters, RefKind refKind, bool isInitOnly, bool isStatic, TypeWithAnnotations returnType, ImmutableArray<CustomModifier> refCustomModifiers, ImmutableArray<MethodSymbol> explicitInterfaceImplementations)
	{
		_callingConvention = callingConvention;
		_typeParameters = typeParameters;
		_refKind = refKind;
		_isInitOnly = isInitOnly;
		_isStatic = isStatic;
		_returnType = returnType;
		_refCustomModifiers = refCustomModifiers;
		_parameters = parameters;
		_explicitInterfaceImplementations = explicitInterfaceImplementations.NullToEmpty();
		_containingType = containingType;
		_methodKind = methodKind;
		_name = name;
	}

	internal sealed override bool IsNullableAnalysisEnabled()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 97);
	}

	internal sealed override bool HasAsyncMethodBuilderAttribute(out TypeSymbol builderArgument)
	{
		builderArgument = null;
		return false;
	}

	public override DllImportData GetDllImportData()
	{
		return null;
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 121);
	}

	internal sealed override UnmanagedCallersOnlyAttributeData GetUnmanagedCallersOnlyAttributeData(bool forceComplete)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 125);
	}

	internal override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 129);
	}

	internal sealed override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 165);
	}

	internal sealed override bool IsMetadataVirtual(IsMetadataVirtualOption option = IsMetadataVirtualOption.None)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 167);
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 181);
	}

	internal sealed override int TryGetOverloadResolutionPriority()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyMethodSymbol.cs", 189);
	}
}
