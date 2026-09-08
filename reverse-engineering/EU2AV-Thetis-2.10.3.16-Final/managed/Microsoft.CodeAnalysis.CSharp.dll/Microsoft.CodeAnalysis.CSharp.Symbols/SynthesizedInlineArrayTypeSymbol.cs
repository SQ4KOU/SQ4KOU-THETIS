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

internal sealed class SynthesizedInlineArrayTypeSymbol : NamedTypeSymbol
{
	private sealed class InlineArrayTypeParameterSymbol : TypeParameterSymbol
	{
		private readonly SynthesizedInlineArrayTypeSymbol _container;

		public override string Name => "T";

		public override int Ordinal => 0;

		public override bool HasConstructorConstraint => false;

		public override TypeParameterKind TypeParameterKind => TypeParameterKind.Type;

		public override bool HasReferenceTypeConstraint => false;

		public override bool IsReferenceTypeFromConstraintTypes => false;

		public override bool HasNotNullConstraint => false;

		public override bool HasValueTypeConstraint => false;

		public override bool AllowsRefLikeType => false;

		public override bool IsValueTypeFromConstraintTypes => false;

		public override bool HasUnmanagedTypeConstraint => false;

		public override VarianceKind Variance => VarianceKind.None;

		public override Symbol ContainingSymbol => _container;

		public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

		public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

		internal override bool? IsNotNullable => null;

		internal override bool? ReferenceTypeConstraintIsNullable => null;

		internal InlineArrayTypeParameterSymbol(SynthesizedInlineArrayTypeSymbol container)
		{
			_container = container;
		}

		internal override void EnsureAllConstraintsAreResolved()
		{
		}

		internal override ImmutableArray<TypeWithAnnotations> GetConstraintTypes(ConsList<TypeParameterSymbol> inProgress)
		{
			return ImmutableArray<TypeWithAnnotations>.Empty;
		}

		internal override TypeSymbol GetDeducedBaseType(ConsList<TypeParameterSymbol> inProgress)
		{
			return ContainingAssembly.GetSpecialType(SpecialType.System_Object);
		}

		internal override NamedTypeSymbol GetEffectiveBaseClass(ConsList<TypeParameterSymbol> inProgress)
		{
			return ContainingAssembly.GetSpecialType(SpecialType.System_Object);
		}

		internal override ImmutableArray<NamedTypeSymbol> GetInterfaces(ConsList<TypeParameterSymbol> inProgress)
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}
	}

	private readonly ModuleSymbol _containingModule;

	private readonly int _arrayLength;

	private readonly MethodSymbol _inlineArrayAttributeConstructor;

	private readonly ImmutableArray<FieldSymbol> _fields;

	public override int Arity => 1;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters { get; }

	public override NamedTypeSymbol ConstructedFrom => this;

	public override bool MightContainExtensionMethods => false;

	public override string Name { get; }

	public override IEnumerable<string> MemberNames => GetMembers().SelectAsArray((Symbol m) => m.Name);

	public override Accessibility DeclaredAccessibility => Accessibility.Internal;

	public override bool IsSerializable => false;

	public override bool AreLocalsZeroed => true;

	public override TypeKind TypeKind => TypeKind.Struct;

	public override bool IsRefLikeType => false;

	internal override string? ExtensionGroupingName => null;

	internal override string? ExtensionMarkerName => null;

	public override bool IsReadOnly => true;

	public override Symbol? ContainingSymbol => _containingModule.GlobalNamespace;

	internal override ModuleSymbol ContainingModule => _containingModule;

	public override AssemblySymbol ContainingAssembly => _containingModule.ContainingAssembly;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override bool IsStatic => false;

	public override bool IsAbstract => false;

	public override bool IsSealed => true;

	internal override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotationsNoUseSiteDiagnostics => GetTypeParametersAsTypeArguments();

	internal override bool IsFileLocal => false;

	internal override FileIdentifier? AssociatedFileIdentifier => null;

	internal override bool MangleName => true;

	internal override bool HasDeclaredRequiredMembers => false;

	internal override bool HasCodeAnalysisEmbeddedAttribute => false;

	internal override bool HasCompilerLoweringPreserveAttribute => false;

	internal override bool IsInterpolatedStringHandlerType => false;

	internal sealed override ParameterSymbol? ExtensionParameter => null;

	internal override bool HasSpecialName => false;

	internal override bool IsComImport => false;

	internal override bool IsWindowsRuntimeImport => false;

	internal override bool ShouldAddWinRTMembers => false;

	internal override TypeLayout Layout => default(TypeLayout);

	internal override CharSet MarshallingCharSet => base.DefaultMarshallingCharSet;

	internal override bool HasDeclarativeSecurity => false;

	internal override bool IsInterface => false;

	internal override NamedTypeSymbol? NativeIntegerUnderlyingType => null;

	internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => ContainingAssembly.GetSpecialType(SpecialType.System_ValueType);

	internal override bool IsRecord => false;

	internal override bool IsRecordStruct => false;

	internal override ObsoleteAttributeData? ObsoleteAttributeData => null;

	internal SynthesizedInlineArrayTypeSymbol(SourceModuleSymbol containingModule, string name, int arrayLength, MethodSymbol inlineArrayAttributeConstructor)
	{
		InlineArrayTypeParameterSymbol inlineArrayTypeParameterSymbol = new InlineArrayTypeParameterSymbol(this);
		SynthesizedFieldSymbol item = new SynthesizedFieldSymbol(this, inlineArrayTypeParameterSymbol, "_element0");
		_containingModule = containingModule;
		_arrayLength = arrayLength;
		_inlineArrayAttributeConstructor = inlineArrayAttributeConstructor;
		_fields = ImmutableArray.Create((FieldSymbol)item);
		Name = name;
		TypeParameters = ImmutableArray.Create((TypeParameterSymbol)inlineArrayTypeParameterSymbol);
	}

	internal override bool GetGuidString(out string? guidString)
	{
		guidString = null;
		return false;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return ImmutableArray<Symbol>.CastUp(_fields);
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return GetMembers().WhereAsArray((Symbol m) => m.Name == name);
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name, int arity)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	protected override NamedTypeSymbol WithTupleDataCore(TupleExtraData newData)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedInlineArrayTypeSymbol.cs", 150);
	}

	internal override NamedTypeSymbol AsNativeInteger()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedInlineArrayTypeSymbol.cs", 152);
	}

	internal override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return ImmutableArray<string>.Empty;
	}

	internal override AttributeUsageInfo GetAttributeUsageInfo()
	{
		return default(AttributeUsageInfo);
	}

	internal override NamedTypeSymbol GetDeclaredBaseType(ConsList<TypeSymbol> basesBeingResolved)
	{
		return BaseTypeNoUseSiteDiagnostics;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetDeclaredInterfaces(ConsList<TypeSymbol> basesBeingResolved)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedInlineArrayTypeSymbol.cs", 162);
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers(string name)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedInlineArrayTypeSymbol.cs", 164);
	}

	internal override IEnumerable<FieldSymbol> GetFieldsToEmit()
	{
		return _fields;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetInterfacesToEmit()
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		return SpecializedCollections.EmptyEnumerable<SecurityAttribute>();
	}

	internal override bool HasCollectionBuilderAttribute(out TypeSymbol? builderType, out string? methodName)
	{
		builderType = null;
		methodName = null;
		return false;
	}

	internal override bool HasInlineArrayAttribute(out int length)
	{
		length = _arrayLength;
		return true;
	}

	internal sealed override bool HasAsyncMethodBuilderAttribute(out TypeSymbol? builderArgument)
	{
		builderArgument = null;
		return false;
	}

	internal override bool HasPossibleWellKnownCloneMethod()
	{
		return false;
	}

	internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<TypeSymbol>? basesBeingResolved = null)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override IEnumerable<(MethodSymbol Body, MethodSymbol Implemented)> SynthesizedInterfaceMethodImpls()
	{
		return SpecializedCollections.EmptyEnumerable<(MethodSymbol, MethodSymbol)>();
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		CSharpCompilation declaringCompilation = _containingModule.DeclaringCompilation;
		Symbol.AddSynthesizedAttribute(ref attributes, SynthesizedAttributeData.Create(moduleBuilder.Compilation, _inlineArrayAttributeConstructor, ImmutableArray.Create(new TypedConstant(declaringCompilation.GetSpecialType(SpecialType.System_Int32), TypedConstantKind.Primitive, _arrayLength)), ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty));
	}
}
