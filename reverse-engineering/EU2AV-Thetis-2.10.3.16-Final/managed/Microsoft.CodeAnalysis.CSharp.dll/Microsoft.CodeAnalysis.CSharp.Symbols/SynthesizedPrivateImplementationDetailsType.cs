using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Collections;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedPrivateImplementationDetailsType : NamedTypeSymbol
{
	private readonly PrivateImplementationDetails _privateImplementationDetails;

	private readonly NamespaceSymbol _globalNamespace;

	private readonly NamedTypeSymbol _objectType;

	public PrivateImplementationDetails PrivateImplementationDetails => _privateImplementationDetails;

	public override bool IsImplicitlyDeclared => true;

	public override int Arity => 0;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public override NamedTypeSymbol ConstructedFrom => this;

	public override bool MightContainExtensionMethods => false;

	public override string Name => _privateImplementationDetails.Name;

	public override IEnumerable<string> MemberNames => SpecializedCollections.EmptyEnumerable<string>();

	public override Accessibility DeclaredAccessibility => Accessibility.Internal;

	public override bool IsSerializable => false;

	public override bool AreLocalsZeroed => false;

	public override TypeKind TypeKind => TypeKind.Class;

	public override bool IsRefLikeType => false;

	internal override string? ExtensionGroupingName => null;

	internal override string? ExtensionMarkerName => null;

	public override bool IsReadOnly => false;

	public override Symbol ContainingSymbol => _globalNamespace;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override bool IsStatic
	{
		get
		{
			if (_privateImplementationDetails.IsSealed)
			{
				return _privateImplementationDetails.IsAbstract;
			}
			return false;
		}
	}

	public override bool IsAbstract
	{
		get
		{
			if (_privateImplementationDetails.IsAbstract)
			{
				return !_privateImplementationDetails.IsSealed;
			}
			return false;
		}
	}

	public override bool IsSealed
	{
		get
		{
			if (_privateImplementationDetails.IsSealed)
			{
				return !_privateImplementationDetails.IsAbstract;
			}
			return false;
		}
	}

	internal override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotationsNoUseSiteDiagnostics => ImmutableArray<TypeWithAnnotations>.Empty;

	internal override bool IsFileLocal => false;

	internal override FileIdentifier? AssociatedFileIdentifier => null;

	internal override bool MangleName => false;

	internal override bool HasDeclaredRequiredMembers => false;

	internal override bool HasCodeAnalysisEmbeddedAttribute => false;

	internal override bool HasCompilerLoweringPreserveAttribute => false;

	internal override bool IsInterpolatedStringHandlerType => false;

	internal sealed override ParameterSymbol? ExtensionParameter => null;

	internal override bool HasSpecialName => _privateImplementationDetails.IsSpecialName;

	internal override bool IsComImport => false;

	internal override bool IsWindowsRuntimeImport => false;

	internal override bool ShouldAddWinRTMembers => false;

	internal override TypeLayout Layout => new TypeLayout(_privateImplementationDetails.Layout, (int)_privateImplementationDetails.SizeOf, (byte)_privateImplementationDetails.Alignment);

	internal override CharSet MarshallingCharSet => _privateImplementationDetails.StringFormat;

	internal override bool HasDeclarativeSecurity => false;

	internal override bool IsInterface => false;

	internal override NamedTypeSymbol? NativeIntegerUnderlyingType => null;

	internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => _objectType;

	internal override bool IsRecord => false;

	internal override bool IsRecordStruct => false;

	internal override ObsoleteAttributeData? ObsoleteAttributeData => null;

	public SynthesizedPrivateImplementationDetailsType(PrivateImplementationDetails privateImplementationDetails, NamespaceSymbol globalNamespace, NamedTypeSymbol objectType)
	{
		_privateImplementationDetails = privateImplementationDetails;
		_globalNamespace = globalNamespace;
		_objectType = objectType;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return ImmutableArray<Symbol>.Empty;
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
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedPrivateImplementationDetailsType.cs", 135);
	}

	internal override NamedTypeSymbol AsNativeInteger()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedPrivateImplementationDetailsType.cs", 140);
	}

	internal override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return ImmutableArray<string>.Empty;
	}

	internal override AttributeUsageInfo GetAttributeUsageInfo()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedPrivateImplementationDetailsType.cs", 147);
	}

	internal override NamedTypeSymbol GetDeclaredBaseType(ConsList<TypeSymbol> basesBeingResolved)
	{
		return _objectType;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetDeclaredInterfaces(ConsList<TypeSymbol> basesBeingResolved)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers(string name)
	{
		return ImmutableArray<Symbol>.Empty;
	}

	internal override IEnumerable<FieldSymbol> GetFieldsToEmit()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedPrivateImplementationDetailsType.cs", 158);
	}

	internal override bool GetGuidString(out string? guidString)
	{
		guidString = null;
		return false;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetInterfacesToEmit()
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		return SpecializedCollections.EmptyEnumerable<SecurityAttribute>();
	}

	internal override bool HasAsyncMethodBuilderAttribute(out TypeSymbol? builderArgument)
	{
		builderArgument = null;
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
}
