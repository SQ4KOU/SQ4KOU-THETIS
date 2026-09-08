using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SynthesizedContainer : NamedTypeSymbol
{
	private readonly ImmutableArray<TypeParameterSymbol> _typeParameters;

	private readonly ImmutableArray<TypeParameterSymbol> _constructedFromTypeParameters;

	internal TypeMap TypeMap { get; }

	internal virtual MethodSymbol Constructor => null;

	internal sealed override bool IsInterface => TypeKind == TypeKind.Interface;

	internal sealed override ParameterSymbol? ExtensionParameter => null;

	internal sealed override string? ExtensionGroupingName => null;

	internal sealed override string? ExtensionMarkerName => null;

	internal ImmutableArray<TypeParameterSymbol> ConstructedFromTypeParameters => _constructedFromTypeParameters;

	public sealed override ImmutableArray<TypeParameterSymbol> TypeParameters => _typeParameters;

	public sealed override string Name { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override IEnumerable<string> MemberNames => SpecializedCollections.EmptyEnumerable<string>();

	public override NamedTypeSymbol ConstructedFrom => this;

	public override bool IsSealed => true;

	public override bool IsAbstract
	{
		get
		{
			if ((object)Constructor == null)
			{
				return TypeKind != TypeKind.Struct;
			}
			return false;
		}
	}

	internal override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotationsNoUseSiteDiagnostics => GetTypeParametersAsTypeArguments();

	internal override bool HasCodeAnalysisEmbeddedAttribute => false;

	internal override bool HasCompilerLoweringPreserveAttribute => false;

	internal sealed override bool IsInterpolatedStringHandlerType => false;

	internal sealed override bool HasDeclaredRequiredMembers => false;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public override bool IsStatic => false;

	public sealed override bool IsRefLikeType => false;

	public sealed override bool IsReadOnly => false;

	internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => ContainingAssembly.GetSpecialType((TypeKind != TypeKind.Struct) ? SpecialType.System_Object : SpecialType.System_ValueType);

	public override bool MightContainExtensionMethods => false;

	public override int Arity => TypeParameters.Length;

	internal override bool MangleName => Arity > 0;

	internal sealed override bool IsFileLocal => false;

	internal sealed override FileIdentifier? AssociatedFileIdentifier => null;

	public override bool IsImplicitlyDeclared => true;

	internal override bool ShouldAddWinRTMembers => false;

	internal override bool IsWindowsRuntimeImport => false;

	internal override bool IsComImport => false;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	internal override bool HasDeclarativeSecurity => false;

	internal override CharSet MarshallingCharSet => base.DefaultMarshallingCharSet;

	public override bool IsSerializable => false;

	internal override TypeLayout Layout => default(TypeLayout);

	internal override bool HasSpecialName => false;

	internal sealed override NamedTypeSymbol NativeIntegerUnderlyingType => null;

	protected SynthesizedContainer(string name, ImmutableArray<TypeParameterSymbol> typeParametersToAlphaRename)
	{
		Name = name;
		_constructedFromTypeParameters = typeParametersToAlphaRename;
		TypeMap = TypeMap.Empty.WithAlphaRename(typeParametersToAlphaRename, this, propagateAttributes: false, out _typeParameters);
	}

	protected SynthesizedContainer(string name)
	{
		Name = name;
		_typeParameters = ImmutableArray<TypeParameterSymbol>.Empty;
		TypeMap = TypeMap.Empty;
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		if (ContainingSymbol.Kind != SymbolKind.NamedType || !ContainingSymbol.IsImplicitlyDeclared)
		{
			CSharpCompilation declaringCompilation = ContainingSymbol.DeclaringCompilation;
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor));
		}
	}

	protected override NamedTypeSymbol WithTupleDataCore(TupleExtraData newData)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedContainer.cs", 76);
	}

	internal override bool GetGuidString(out string guidString)
	{
		guidString = null;
		return false;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		Symbol constructor = Constructor;
		if ((object)constructor != null)
		{
			return ImmutableArray.Create(constructor);
		}
		return ImmutableArray<Symbol>.Empty;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		MethodSymbol constructor = Constructor;
		if ((object)constructor == null || !(name == constructor.Name))
		{
			return ImmutableArray<Symbol>.Empty;
		}
		return ImmutableArray.Create((Symbol)constructor);
	}

	internal override IEnumerable<FieldSymbol> GetFieldsToEmit()
	{
		foreach (Symbol member in GetMembers())
		{
			if (member.Kind == SymbolKind.Field)
			{
				yield return (FieldSymbol)member;
			}
		}
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers()
	{
		return GetMembersUnordered();
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers(string name)
	{
		return GetMembers(name);
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name, int arity)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<TypeSymbol> basesBeingResolved)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetInterfacesToEmit()
	{
		return CalculateInterfacesToEmit();
	}

	internal override NamedTypeSymbol GetDeclaredBaseType(ConsList<TypeSymbol> basesBeingResolved)
	{
		return BaseTypeNoUseSiteDiagnostics;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetDeclaredInterfaces(ConsList<TypeSymbol> basesBeingResolved)
	{
		return InterfacesNoUseSiteDiagnostics(basesBeingResolved);
	}

	internal sealed override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return ImmutableArray<string>.Empty;
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedContainer.cs", 199);
	}

	internal override AttributeUsageInfo GetAttributeUsageInfo()
	{
		return default(AttributeUsageInfo);
	}

	internal sealed override NamedTypeSymbol AsNativeInteger()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedContainer.cs", 208);
	}

	internal sealed override IEnumerable<(MethodSymbol Body, MethodSymbol Implemented)> SynthesizedInterfaceMethodImpls()
	{
		return SpecializedCollections.EmptyEnumerable<(MethodSymbol, MethodSymbol)>();
	}

	internal sealed override bool HasInlineArrayAttribute(out int length)
	{
		length = 0;
		return false;
	}

	internal sealed override bool HasCollectionBuilderAttribute(out TypeSymbol? builderType, out string? methodName)
	{
		builderType = null;
		methodName = null;
		return false;
	}

	internal sealed override bool HasAsyncMethodBuilderAttribute(out TypeSymbol? builderArgument)
	{
		builderArgument = null;
		return false;
	}
}
