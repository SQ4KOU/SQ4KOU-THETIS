using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedHotReloadExceptionSymbol : NamedTypeSymbol
{
	public const string NamespaceName = "System.Runtime.CompilerServices";

	public const string TypeName = "HotReloadException";

	public const string CodeFieldName = "Code";

	public const string CreatedActionFieldName = "Created";

	private readonly NamedTypeSymbol _baseType;

	private readonly NamespaceSymbol _namespace;

	private readonly ImmutableArray<Symbol> _members;

	public MethodSymbol Constructor => (MethodSymbol)_members[0];

	public FieldSymbol CodeField => (FieldSymbol)_members[1];

	public FieldSymbol CreatedActionField => (FieldSymbol)_members[2];

	public override IEnumerable<string> MemberNames => _members.Select((Symbol m) => m.Name);

	public override string Name => "HotReloadException";

	public override int Arity => 0;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public override bool IsImplicitlyDeclared => true;

	public override NamedTypeSymbol ConstructedFrom => this;

	public override bool MightContainExtensionMethods => false;

	internal override bool HasDeclaredRequiredMembers => false;

	public override Accessibility DeclaredAccessibility => Accessibility.Internal;

	public override TypeKind TypeKind => TypeKind.Class;

	public override Symbol ContainingSymbol => _namespace;

	public override NamespaceSymbol ContainingNamespace => _namespace;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override bool IsStatic => false;

	public override bool IsRefLikeType => false;

	internal override string? ExtensionGroupingName => null;

	internal override string? ExtensionMarkerName => null;

	public override bool IsReadOnly => false;

	public override bool IsAbstract => false;

	public override bool IsSealed => true;

	internal override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotationsNoUseSiteDiagnostics => ImmutableArray<TypeWithAnnotations>.Empty;

	internal override bool MangleName => false;

	internal sealed override bool IsFileLocal => false;

	internal sealed override FileIdentifier? AssociatedFileIdentifier => null;

	internal override bool HasCodeAnalysisEmbeddedAttribute => true;

	internal override bool HasCompilerLoweringPreserveAttribute => false;

	internal override bool IsInterpolatedStringHandlerType => false;

	internal sealed override ParameterSymbol? ExtensionParameter => null;

	internal override bool HasSpecialName => false;

	internal override bool IsComImport => false;

	internal override bool IsWindowsRuntimeImport => false;

	internal override bool ShouldAddWinRTMembers => false;

	public override bool IsSerializable => false;

	public sealed override bool AreLocalsZeroed => ContainingModule.AreLocalsZeroed;

	internal override TypeLayout Layout => default(TypeLayout);

	internal override CharSet MarshallingCharSet => base.DefaultMarshallingCharSet;

	internal override bool HasDeclarativeSecurity => false;

	internal override bool IsInterface => false;

	internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => _baseType;

	internal override ObsoleteAttributeData? ObsoleteAttributeData => null;

	internal sealed override bool IsRecord => false;

	internal sealed override bool IsRecordStruct => false;

	internal sealed override NamedTypeSymbol? NativeIntegerUnderlyingType => null;

	public SynthesizedHotReloadExceptionSymbol(NamespaceSymbol containingNamespace, NamedTypeSymbol exceptionType, NamedTypeSymbol actionOfTType, TypeSymbol stringType, TypeSymbol intType)
	{
		_namespace = containingNamespace;
		_baseType = exceptionType;
		_members = ImmutableCollectionsMarshal.AsImmutableArray(new Symbol[3]
		{
			new SynthesizedHotReloadExceptionConstructorSymbol(this, stringType, intType),
			new SynthesizedFieldSymbol(this, intType, "Code", DeclarationModifiers.Public, isReadOnly: true),
			new SynthesizedFieldSymbol(this, actionOfTType.Construct(exceptionType), "Created", DeclarationModifiers.Private, isReadOnly: false, isStatic: true)
		});
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return _members;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return name switch
		{
			".ctor" => ImmutableCollectionsMarshal.AsImmutableArray(new Symbol[1] { Constructor }), 
			"Code" => ImmutableCollectionsMarshal.AsImmutableArray(new Symbol[1] { CodeField }), 
			"Created" => ImmutableCollectionsMarshal.AsImmutableArray(new Symbol[1] { CreatedActionField }), 
			_ => ImmutableArray<Symbol>.Empty, 
		};
	}

	internal override IEnumerable<FieldSymbol> GetFieldsToEmit()
	{
		return new _003C_003Ez__ReadOnlyArray<FieldSymbol>(new FieldSymbol[2] { CodeField, CreatedActionField });
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

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.Compilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor));
	}

	internal override ManagedKind GetManagedKind(ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return ManagedKind.Managed;
	}

	protected override NamedTypeSymbol WithTupleDataCore(TupleExtraData newData)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedHotReloadExceptionSymbol.cs", 138);
	}

	internal override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return ImmutableArray<string>.Empty;
	}

	internal override AttributeUsageInfo GetAttributeUsageInfo()
	{
		return AttributeUsageInfo.Default;
	}

	internal override NamedTypeSymbol GetDeclaredBaseType(ConsList<TypeSymbol> basesBeingResolved)
	{
		return _baseType;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetDeclaredInterfaces(ConsList<TypeSymbol> basesBeingResolved)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers()
	{
		return GetMembers();
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers(string name)
	{
		return GetMembers(name);
	}

	internal override ImmutableArray<NamedTypeSymbol> GetInterfacesToEmit()
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override IEnumerable<SecurityAttribute>? GetSecurityInformation()
	{
		return null;
	}

	internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<TypeSymbol>? basesBeingResolved = null)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal sealed override bool HasPossibleWellKnownCloneMethod()
	{
		return false;
	}

	internal sealed override NamedTypeSymbol AsNativeInteger()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedHotReloadExceptionSymbol.cs", 151);
	}

	internal sealed override IEnumerable<(MethodSymbol Body, MethodSymbol Implemented)> SynthesizedInterfaceMethodImpls()
	{
		return Array.Empty<(MethodSymbol, MethodSymbol)>();
	}

	internal override bool GetGuidString([NotNullWhen(true)] out string? guidString)
	{
		guidString = null;
		return false;
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
