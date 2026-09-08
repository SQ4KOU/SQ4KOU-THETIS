using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedEmbeddedExtensionMarkerAttributeSymbol : SynthesizedEmbeddedAttributeSymbolBase
{
	private sealed class NamePropertySymbol : PropertySymbol
	{
		internal readonly SynthesizedFieldSymbol _backingField;

		public override string Name => "Name";

		public override TypeWithAnnotations TypeWithAnnotations => _backingField.TypeWithAnnotations;

		public override RefKind RefKind => RefKind.None;

		public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

		public override MethodSymbol GetMethod { get; }

		public override MethodSymbol? SetMethod => null;

		public override Symbol ContainingSymbol => _backingField.ContainingSymbol;

		public override Accessibility DeclaredAccessibility => Accessibility.Public;

		public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

		public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

		public override ImmutableArray<PropertySymbol> ExplicitInterfaceImplementations => ImmutableArray<PropertySymbol>.Empty;

		public override ImmutableArray<ParameterSymbol> Parameters => ImmutableArray<ParameterSymbol>.Empty;

		public override bool IsIndexer => false;

		public override bool IsStatic => false;

		public override bool IsVirtual => false;

		public override bool IsOverride => false;

		public override bool IsAbstract => false;

		public override bool IsSealed => false;

		public override bool IsExtern => false;

		internal override bool IsRequired => false;

		internal override bool HasSpecialName => false;

		internal override Microsoft.Cci.CallingConvention CallingConvention => Microsoft.Cci.CallingConvention.HasThis;

		internal override bool MustCallMethodsDirectly => false;

		internal override bool HasUnscopedRefAttribute => false;

		internal override ObsoleteAttributeData? ObsoleteAttributeData => null;

		public NamePropertySymbol(SynthesizedFieldSymbol backingField)
		{
			_backingField = backingField;
			GetMethod = new NameGetAccessorMethodSymbol(this);
		}

		internal override int TryGetOverloadResolutionPriority()
		{
			return 0;
		}
	}

	private sealed class NameGetAccessorMethodSymbol : SynthesizedMethodSymbol
	{
		private readonly NamePropertySymbol _nameProperty;

		public override string Name => "get_Name";

		internal override bool HasSpecialName => true;

		public override MethodKind MethodKind => MethodKind.PropertyGet;

		public override Symbol AssociatedSymbol => _nameProperty;

		public override Symbol ContainingSymbol => _nameProperty.ContainingSymbol;

		internal override bool SynthesizesLoweredBoundBody => true;

		public override bool IsStatic => false;

		public override int Arity => 0;

		public override bool IsExtensionMethod => false;

		public override bool HidesBaseMethodsByName => false;

		public override bool IsVararg => false;

		public override bool ReturnsVoid => false;

		public override bool IsAsync => false;

		public override RefKind RefKind => RefKind.None;

		public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

		public override TypeWithAnnotations ReturnTypeWithAnnotations => _nameProperty.TypeWithAnnotations;

		public override FlowAnalysisAnnotations ReturnTypeFlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

		public override ImmutableHashSet<string> ReturnNotNullIfParameterNotNull => ImmutableHashSet.Create(default(ReadOnlySpan<string>));

		public override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotations => ImmutableArray<TypeWithAnnotations>.Empty;

		public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

		public override ImmutableArray<ParameterSymbol> Parameters => ImmutableArray<ParameterSymbol>.Empty;

		public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

		public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

		public override Accessibility DeclaredAccessibility => _nameProperty.DeclaredAccessibility;

		public override bool IsVirtual => false;

		public override bool IsOverride => false;

		public override bool IsAbstract => false;

		public override bool IsSealed => false;

		public override bool IsExtern => false;

		protected override bool HasSetsRequiredMembersImpl => false;

		internal override MethodImplAttributes ImplementationAttributes => MethodImplAttributes.IL;

		internal override bool HasDeclarativeSecurity => false;

		internal override MarshalPseudoCustomAttributeData? ReturnValueMarshallingInformation => null;

		internal override bool RequiresSecurityObject => false;

		internal override Microsoft.Cci.CallingConvention CallingConvention => Microsoft.Cci.CallingConvention.HasThis;

		internal override bool GenerateDebugInfo => false;

		public NameGetAccessorMethodSymbol(NamePropertySymbol nameProperty)
		{
			_nameProperty = nameProperty;
		}

		internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
		{
			SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, CSharpSyntaxTree.Dummy.GetRoot(), compilationState, diagnostics);
			syntheticBoundNodeFactory.CurrentFunction = OriginalDefinition;
			try
			{
				syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.Return(syntheticBoundNodeFactory.Field(syntheticBoundNodeFactory.This(), _nameProperty._backingField)));
			}
			catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
			{
				syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
				diagnostics.Add(missingPredefinedMember.Diagnostic);
			}
		}

		public override DllImportData? GetDllImportData()
		{
			return null;
		}

		internal override ImmutableArray<string> GetAppliedConditionalSymbols()
		{
			return ImmutableArray<string>.Empty;
		}

		internal override IEnumerable<SecurityAttribute>? GetSecurityInformation()
		{
			return null;
		}

		internal override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
		{
			return false;
		}

		internal override bool IsMetadataVirtual(IsMetadataVirtualOption option = IsMetadataVirtualOption.None)
		{
			return false;
		}
	}

	private readonly ImmutableArray<MethodSymbol> _constructors;

	private readonly SynthesizedFieldSymbol _nameField;

	private readonly NamePropertySymbol _nameProperty;

	private const string PropertyName = "Name";

	private const string FieldName = "<Name>k__BackingField";

	public override ImmutableArray<MethodSymbol> Constructors => _constructors;

	public override IEnumerable<string> MemberNames => new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "<Name>k__BackingField", "Name", ".ctor" });

	public SynthesizedEmbeddedExtensionMarkerAttributeSymbol(string name, NamespaceSymbol containingNamespace, ModuleSymbol containingModule, NamedTypeSymbol systemAttributeType, TypeSymbol systemStringType)
		: base(name, containingNamespace, containingModule, systemAttributeType)
	{
		SynthesizedEmbeddedExtensionMarkerAttributeSymbol synthesizedEmbeddedExtensionMarkerAttributeSymbol = this;
		_nameField = new SynthesizedFieldSymbol(this, systemStringType, "<Name>k__BackingField", DeclarationModifiers.Private, isReadOnly: true);
		_nameProperty = new NamePropertySymbol(_nameField);
		_constructors = ImmutableCollectionsMarshal.AsImmutableArray(new MethodSymbol[1]
		{
			new SynthesizedEmbeddedAttributeConstructorWithBodySymbol(this, getConstructorParameters, getConstructorBody)
		});
		void getConstructorBody(SyntheticBoundNodeFactory f, ArrayBuilder<BoundStatement> statements, ImmutableArray<ParameterSymbol> parameters)
		{
			statements.Add(f.Assignment(f.Field(f.This(), synthesizedEmbeddedExtensionMarkerAttributeSymbol._nameField), f.Parameter(parameters[0])));
		}
		ImmutableArray<ParameterSymbol> getConstructorParameters(MethodSymbol ctor)
		{
			return ImmutableCollectionsMarshal.AsImmutableArray(new ParameterSymbol[1] { SynthesizedParameterSymbol.Create(ctor, TypeWithAnnotations.Create(systemStringType), 0, RefKind.None, "name") });
		}
	}

	internal override AttributeUsageInfo GetAttributeUsageInfo()
	{
		return new AttributeUsageInfo(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, allowMultiple: false, inherited: false);
	}

	internal override IEnumerable<FieldSymbol> GetFieldsToEmit()
	{
		return new _003C_003Ez__ReadOnlySingleElementList<FieldSymbol>(_nameField);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return ImmutableCollectionsMarshal.AsImmutableArray(new Symbol[4]
		{
			_nameField,
			_nameProperty,
			_nameProperty.GetMethod,
			_constructors[0]
		});
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return name switch
		{
			"<Name>k__BackingField" => ImmutableCollectionsMarshal.AsImmutableArray(new Symbol[1] { _nameField }), 
			"Name" => ImmutableCollectionsMarshal.AsImmutableArray(new Symbol[1] { _nameProperty }), 
			".ctor" => ImmutableArray<Symbol>.CastUp(_constructors), 
			_ => ImmutableArray<Symbol>.Empty, 
		};
	}
}
