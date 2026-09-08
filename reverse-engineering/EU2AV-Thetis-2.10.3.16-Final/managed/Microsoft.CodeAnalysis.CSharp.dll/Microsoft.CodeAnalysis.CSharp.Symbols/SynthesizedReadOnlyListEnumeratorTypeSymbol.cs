using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedReadOnlyListEnumeratorTypeSymbol : NamedTypeSymbol
{
	private readonly SynthesizedReadOnlyListTypeSymbol _containingType;

	private readonly ImmutableArray<NamedTypeSymbol> _interfaces;

	private readonly ImmutableArray<Symbol> _members;

	private readonly FieldSymbol _itemField;

	private readonly FieldSymbol _moveNextCalledField;

	public override int Arity => 0;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public override NamedTypeSymbol ConstructedFrom => this;

	public override bool MightContainExtensionMethods => false;

	public override string Name => "Enumerator";

	public override IEnumerable<string> MemberNames => from m in GetMembers()
		select m.Name;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public override bool IsSerializable => false;

	public override bool AreLocalsZeroed => true;

	public override TypeKind TypeKind => TypeKind.Class;

	public override bool IsRefLikeType => false;

	internal override string? ExtensionGroupingName => null;

	internal sealed override string? ExtensionMarkerName => null;

	public override bool IsReadOnly => false;

	public override Symbol ContainingSymbol => _containingType;

	internal override ModuleSymbol ContainingModule => _containingType.ContainingModule;

	public override AssemblySymbol ContainingAssembly => _containingType.ContainingAssembly;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override bool IsStatic => false;

	public override bool IsAbstract => false;

	public override bool IsSealed => true;

	internal override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotationsNoUseSiteDiagnostics => GetTypeParametersAsTypeArguments();

	internal override bool IsFileLocal => false;

	internal override FileIdentifier? AssociatedFileIdentifier => null;

	internal override bool MangleName => false;

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

	internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => ContainingAssembly.GetSpecialType(SpecialType.System_Object);

	internal override bool IsRecord => false;

	internal override bool IsRecordStruct => false;

	internal override ObsoleteAttributeData? ObsoleteAttributeData => null;

	public SynthesizedReadOnlyListEnumeratorTypeSymbol(SynthesizedReadOnlyListTypeSymbol containingType, SynthesizedReadOnlyListTypeParameterSymbol typeParameter)
	{
		_containingType = containingType;
		CSharpCompilation declaringCompilation = containingType.DeclaringCompilation;
		ImmutableArray<TypeWithAnnotations> typeArgumentsWithAnnotationsNoUseSiteDiagnostics = containingType.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics;
		_itemField = new SynthesizedFieldSymbol(this, typeParameter, "_item", DeclarationModifiers.Private, isReadOnly: true);
		_moveNextCalledField = new SynthesizedFieldSymbol(this, declaringCompilation.GetSpecialType(SpecialType.System_Boolean), "_moveNextCalled");
		NamedTypeSymbol specialType = declaringCompilation.GetSpecialType(SpecialType.System_IDisposable);
		NamedTypeSymbol specialType2 = declaringCompilation.GetSpecialType(SpecialType.System_Collections_IEnumerator);
		NamedTypeSymbol namedTypeSymbol = declaringCompilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerator_T).Construct(typeArgumentsWithAnnotationsNoUseSiteDiagnostics);
		_interfaces = ImmutableArray.Create(specialType, specialType2, namedTypeSymbol);
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		instance.Add(_itemField);
		instance.Add(_moveNextCalledField);
		instance.Add(new SynthesizedReadOnlyListEnumeratorConstructor(this, typeParameter));
		addProperty(instance, new SynthesizedReadOnlyListProperty(this, (PropertySymbol)declaringCompilation.GetSpecialTypeMember(SpecialMember.System_Collections_IEnumerator__Current), delegate(SyntheticBoundNodeFactory f, MethodSymbol method, MethodSymbol interfaceMethod)
		{
			FieldSymbol itemField = ((SynthesizedReadOnlyListEnumeratorTypeSymbol)method.ContainingType)._itemField;
			BoundFieldAccess arg = f.Field(f.This(), itemField);
			Conversion conversion = f.ClassifyEmitConversion(arg, method.ReturnType);
			return f.Return(f.Convert(method.ReturnType, arg, conversion));
		}));
		addProperty(instance, new SynthesizedReadOnlyListProperty(this, ((PropertySymbol)declaringCompilation.GetSpecialTypeMember(SpecialMember.System_Collections_Generic_IEnumerator_T__Current)).AsMember(namedTypeSymbol), delegate(SyntheticBoundNodeFactory f, MethodSymbol method, MethodSymbol interfaceMethod)
		{
			FieldSymbol itemField = ((SynthesizedReadOnlyListEnumeratorTypeSymbol)method.ContainingType)._itemField;
			BoundFieldAccess expression = f.Field(f.This(), itemField);
			return f.Return(expression);
		}));
		instance.Add(new SynthesizedReadOnlyListMethod(this, (MethodSymbol)declaringCompilation.GetSpecialTypeMember(SpecialMember.System_Collections_IEnumerator__MoveNext), delegate(SyntheticBoundNodeFactory f, MethodSymbol method, MethodSymbol interfaceMethod)
		{
			FieldSymbol moveNextCalledField = ((SynthesizedReadOnlyListEnumeratorTypeSymbol)method.ContainingType)._moveNextCalledField;
			BoundFieldAccess boundFieldAccess = f.Field(f.This(), moveNextCalledField);
			return f.Return(f.Conditional(boundFieldAccess, f.Literal(value: false), f.AssignmentExpression(boundFieldAccess, f.Literal(value: true)), method.ReturnType));
		}));
		instance.Add(new SynthesizedReadOnlyListMethod(this, (MethodSymbol)declaringCompilation.GetSpecialTypeMember(SpecialMember.System_Collections_IEnumerator__Reset), delegate(SyntheticBoundNodeFactory f, MethodSymbol method, MethodSymbol interfaceMethod)
		{
			FieldSymbol moveNextCalledField = ((SynthesizedReadOnlyListEnumeratorTypeSymbol)method.ContainingType)._moveNextCalledField;
			BoundFieldAccess left = f.Field(f.This(), moveNextCalledField);
			return f.Block(f.Assignment(left, f.Literal(value: false)), f.Return());
		}));
		instance.Add(new SynthesizedReadOnlyListMethod(this, (MethodSymbol)declaringCompilation.GetSpecialTypeMember(SpecialMember.System_IDisposable__Dispose), (SyntheticBoundNodeFactory f, MethodSymbol method, MethodSymbol interfaceMethod) => f.Return()));
		_members = instance.ToImmutableAndFree();
		static void addProperty(ArrayBuilder<Symbol> builder, PropertySymbol property)
		{
			builder.Add(property);
			builder.Add(property.GetMethod);
		}
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return _members;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return GetMembers().WhereAsArray((Symbol m, string text) => m.Name == text, name);
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
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/ReadOnlyListType/SynthesizedReadOnlyListEnumeratorTypeSymbol.cs", 227);
	}

	internal override NamedTypeSymbol AsNativeInteger()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/ReadOnlyListType/SynthesizedReadOnlyListEnumeratorTypeSymbol.cs", 229);
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
		return _interfaces;
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/ReadOnlyListType/SynthesizedReadOnlyListEnumeratorTypeSymbol.cs", 239);
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers(string name)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/ReadOnlyListType/SynthesizedReadOnlyListEnumeratorTypeSymbol.cs", 241);
	}

	internal override IEnumerable<FieldSymbol> GetFieldsToEmit()
	{
		return _members.OfType<FieldSymbol>();
	}

	internal override ImmutableArray<NamedTypeSymbol> GetInterfacesToEmit()
	{
		return _interfaces;
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		return SpecializedCollections.EmptyEnumerable<SecurityAttribute>();
	}

	internal override bool GetGuidString(out string? guidString)
	{
		guidString = null;
		return false;
	}

	internal override bool HasCollectionBuilderAttribute(out TypeSymbol? builderType, out string? methodName)
	{
		builderType = null;
		methodName = null;
		return false;
	}

	internal override bool HasInlineArrayAttribute(out int length)
	{
		length = 0;
		return false;
	}

	internal override bool HasAsyncMethodBuilderAttribute(out TypeSymbol? builderArgument)
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
		return _interfaces;
	}

	internal override IEnumerable<(MethodSymbol Body, MethodSymbol Implemented)> SynthesizedInterfaceMethodImpls()
	{
		return SpecializedCollections.EmptyEnumerable<(MethodSymbol, MethodSymbol)>();
	}
}
