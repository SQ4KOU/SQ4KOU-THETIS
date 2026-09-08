using System.Collections.Immutable;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SignatureOnlyPropertySymbol : PropertySymbol
{
	private readonly string _name;

	private readonly TypeSymbol _containingType;

	private readonly ImmutableArray<ParameterSymbol> _parameters;

	private readonly RefKind _refKind;

	private readonly TypeWithAnnotations _type;

	private readonly ImmutableArray<CustomModifier> _refCustomModifiers;

	private readonly bool _isStatic;

	private readonly ImmutableArray<PropertySymbol> _explicitInterfaceImplementations;

	public override RefKind RefKind => _refKind;

	public override TypeWithAnnotations TypeWithAnnotations => _type;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => _refCustomModifiers;

	public override bool IsStatic => _isStatic;

	public override ImmutableArray<ParameterSymbol> Parameters => _parameters;

	public override ImmutableArray<PropertySymbol> ExplicitInterfaceImplementations => _explicitInterfaceImplementations;

	public override Symbol ContainingSymbol => _containingType;

	public override string Name => _name;

	internal sealed override bool HasUnscopedRefAttribute => false;

	internal override bool HasSpecialName
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyPropertySymbol.cs", 70);
		}
	}

	internal override CallingConvention CallingConvention
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyPropertySymbol.cs", 72);
		}
	}

	public override ImmutableArray<Location> Locations
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyPropertySymbol.cs", 74);
		}
	}

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyPropertySymbol.cs", 76);
		}
	}

	public override Accessibility DeclaredAccessibility
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyPropertySymbol.cs", 78);
		}
	}

	public override bool IsVirtual
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyPropertySymbol.cs", 80);
		}
	}

	public override bool IsOverride => false;

	public override bool IsAbstract
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyPropertySymbol.cs", 84);
		}
	}

	public override bool IsSealed
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyPropertySymbol.cs", 86);
		}
	}

	public override bool IsExtern
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyPropertySymbol.cs", 88);
		}
	}

	internal override bool IsRequired
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyPropertySymbol.cs", 90);
		}
	}

	internal override ObsoleteAttributeData ObsoleteAttributeData
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyPropertySymbol.cs", 92);
		}
	}

	public override AssemblySymbol ContainingAssembly
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyPropertySymbol.cs", 94);
		}
	}

	internal override ModuleSymbol ContainingModule
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyPropertySymbol.cs", 96);
		}
	}

	internal override bool MustCallMethodsDirectly
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyPropertySymbol.cs", 98);
		}
	}

	public override MethodSymbol SetMethod
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyPropertySymbol.cs", 100);
		}
	}

	public override MethodSymbol GetMethod
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyPropertySymbol.cs", 102);
		}
	}

	public override bool IsIndexer
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyPropertySymbol.cs", 104);
		}
	}

	public SignatureOnlyPropertySymbol(string name, TypeSymbol containingType, ImmutableArray<ParameterSymbol> parameters, RefKind refKind, TypeWithAnnotations type, ImmutableArray<CustomModifier> refCustomModifiers, bool isStatic, ImmutableArray<PropertySymbol> explicitInterfaceImplementations)
	{
		_refKind = refKind;
		_type = type;
		_refCustomModifiers = refCustomModifiers;
		_isStatic = isStatic;
		_parameters = parameters;
		_explicitInterfaceImplementations = explicitInterfaceImplementations.NullToEmpty();
		_containingType = containingType;
		_name = name;
	}

	internal override int TryGetOverloadResolutionPriority()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyPropertySymbol.cs", 106);
	}
}
