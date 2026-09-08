using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.RuntimeMembers;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class AnonymousTypeManager : CommonAnonymousTypeManager
{
	private sealed class AnonymousTypeConstructorSymbol : SynthesizedMethodBase
	{
		private readonly ImmutableArray<ParameterSymbol> _parameters;

		internal override bool HasSpecialName => true;

		protected override bool HasSetsRequiredMembersImpl => false;

		public override MethodKind MethodKind => MethodKind.Constructor;

		public override bool ReturnsVoid => true;

		public override RefKind RefKind => RefKind.None;

		public override TypeWithAnnotations ReturnTypeWithAnnotations => TypeWithAnnotations.Create(base.Manager.System_Void);

		public override ImmutableArray<ParameterSymbol> Parameters => _parameters;

		public override bool IsOverride => false;

		internal override bool IsMetadataFinal => false;

		public override ImmutableArray<Location> Locations => ContainingSymbol.Locations;

		internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
		{
			SyntheticBoundNodeFactory syntheticBoundNodeFactory = CreateBoundNodeFactory(compilationState, diagnostics);
			int parameterCount = ParameterCount;
			BoundStatement[] array = new BoundStatement[parameterCount + 2];
			int num = 0;
			BoundExpression boundExpression = Binder.GenerateBaseParameterlessConstructorInitializer(this, diagnostics);
			if (boundExpression == null)
			{
				return;
			}
			array[num++] = syntheticBoundNodeFactory.ExpressionStatement(boundExpression);
			if (parameterCount > 0)
			{
				AnonymousTypeTemplateSymbol anonymousTypeTemplateSymbol = (AnonymousTypeTemplateSymbol)ContainingType;
				for (int i = 0; i < ParameterCount; i++)
				{
					array[num++] = syntheticBoundNodeFactory.Assignment(syntheticBoundNodeFactory.Field(syntheticBoundNodeFactory.This(), anonymousTypeTemplateSymbol.Properties[i].BackingField), syntheticBoundNodeFactory.Parameter(_parameters[i]));
				}
			}
			array[num++] = syntheticBoundNodeFactory.Return();
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.Block(array));
		}

		internal AnonymousTypeConstructorSymbol(NamedTypeSymbol container, ImmutableArray<AnonymousTypePropertySymbol> properties)
			: base(container, ".ctor")
		{
			int length = properties.Length;
			if (length > 0)
			{
				ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance(length);
				for (int i = 0; i < length; i++)
				{
					PropertySymbol propertySymbol = properties[i];
					instance.Add(SynthesizedParameterSymbol.Create(this, propertySymbol.TypeWithAnnotations, i, RefKind.None, propertySymbol.Name));
				}
				_parameters = instance.ToImmutableAndFree();
			}
			else
			{
				_parameters = ImmutableArray<ParameterSymbol>.Empty;
			}
		}

		internal sealed override bool IsMetadataVirtual(IsMetadataVirtualOption option = IsMetadataVirtualOption.None)
		{
			return false;
		}
	}

	private sealed class AnonymousTypePropertyGetAccessorSymbol : SynthesizedMethodBase
	{
		private readonly AnonymousTypePropertySymbol _property;

		internal override bool HasSpecialName => true;

		public override MethodKind MethodKind => MethodKind.PropertyGet;

		public override bool ReturnsVoid => false;

		public override RefKind RefKind => RefKind.None;

		public override TypeWithAnnotations ReturnTypeWithAnnotations => _property.TypeWithAnnotations;

		public override ImmutableArray<ParameterSymbol> Parameters => ImmutableArray<ParameterSymbol>.Empty;

		public override Symbol AssociatedSymbol => _property;

		public override ImmutableArray<Location> Locations => _property.Locations;

		public override bool IsOverride => false;

		internal override bool IsMetadataFinal => false;

		internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
		{
			SyntheticBoundNodeFactory syntheticBoundNodeFactory = CreateBoundNodeFactory(compilationState, diagnostics);
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.Block(syntheticBoundNodeFactory.Return(syntheticBoundNodeFactory.Field(syntheticBoundNodeFactory.This(), _property.BackingField))));
		}

		internal AnonymousTypePropertyGetAccessorSymbol(AnonymousTypePropertySymbol property)
			: base(property.ContainingType, SourcePropertyAccessorSymbol.GetAccessorName(property.Name, getNotSet: true, isWinMdOutput: false))
		{
			_property = property;
		}

		internal sealed override bool IsMetadataVirtual(IsMetadataVirtualOption option = IsMetadataVirtualOption.None)
		{
			return false;
		}

		internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
		{
		}
	}

	private sealed class AnonymousTypeEqualsMethodSymbol : SynthesizedMethodBase
	{
		private readonly ImmutableArray<ParameterSymbol> _parameters;

		internal override bool HasSpecialName => false;

		public override MethodKind MethodKind => MethodKind.Ordinary;

		public override bool ReturnsVoid => false;

		public override RefKind RefKind => RefKind.None;

		public override TypeWithAnnotations ReturnTypeWithAnnotations => TypeWithAnnotations.Create(base.Manager.System_Boolean);

		public override ImmutableArray<ParameterSymbol> Parameters => _parameters;

		public override bool IsOverride => true;

		internal override bool IsMetadataFinal => false;

		internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
		{
			AnonymousTypeManager manager = ((AnonymousTypeTemplateSymbol)ContainingType).Manager;
			SyntheticBoundNodeFactory syntheticBoundNodeFactory = CreateBoundNodeFactory(compilationState, diagnostics);
			AnonymousTypeTemplateSymbol anonymousTypeTemplateSymbol = (AnonymousTypeTemplateSymbol)ContainingType;
			BoundLocal boundLocal = syntheticBoundNodeFactory.StoreToTemp(syntheticBoundNodeFactory.As(syntheticBoundNodeFactory.Parameter(_parameters[0]), anonymousTypeTemplateSymbol), out BoundAssignmentOperator store);
			BoundStatement boundStatement = syntheticBoundNodeFactory.ExpressionStatement(store);
			Conversion conversion = syntheticBoundNodeFactory.ClassifyEmitConversion(boundLocal, manager.System_Object);
			BoundExpression boundExpression = syntheticBoundNodeFactory.Binary(BinaryOperatorKind.ObjectNotEqual, manager.System_Boolean, syntheticBoundNodeFactory.Convert(manager.System_Object, boundLocal, conversion), syntheticBoundNodeFactory.Null(manager.System_Object));
			if (anonymousTypeTemplateSymbol.Properties.Length > 0)
			{
				ArrayBuilder<FieldSymbol> instance = ArrayBuilder<FieldSymbol>.GetInstance(anonymousTypeTemplateSymbol.Properties.Length);
				foreach (AnonymousTypePropertySymbol property in anonymousTypeTemplateSymbol.Properties)
				{
					instance.Add(property.BackingField);
				}
				boundExpression = MethodBodySynthesizer.GenerateFieldEquals(boundExpression, boundLocal, instance, syntheticBoundNodeFactory);
				instance.Free();
			}
			boundExpression = syntheticBoundNodeFactory.LogicalOr(syntheticBoundNodeFactory.ObjectEqual(syntheticBoundNodeFactory.This(), boundLocal), boundExpression);
			BoundStatement boundStatement2 = syntheticBoundNodeFactory.Return(boundExpression);
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.Block(ImmutableArray.Create(boundLocal.LocalSymbol), boundStatement, boundStatement2));
		}

		internal AnonymousTypeEqualsMethodSymbol(NamedTypeSymbol container)
			: base(container, "Equals")
		{
			_parameters = ImmutableArray.Create(SynthesizedParameterSymbol.Create(this, TypeWithAnnotations.Create(base.Manager.System_Object), 0, RefKind.None, "value"));
		}

		internal sealed override bool IsMetadataVirtual(IsMetadataVirtualOption option = IsMetadataVirtualOption.None)
		{
			return true;
		}
	}

	private sealed class AnonymousTypeGetHashCodeMethodSymbol : SynthesizedMethodBase
	{
		internal override bool HasSpecialName => false;

		public override MethodKind MethodKind => MethodKind.Ordinary;

		public override bool ReturnsVoid => false;

		public override RefKind RefKind => RefKind.None;

		public override TypeWithAnnotations ReturnTypeWithAnnotations => TypeWithAnnotations.Create(base.Manager.System_Int32);

		public override ImmutableArray<ParameterSymbol> Parameters => ImmutableArray<ParameterSymbol>.Empty;

		public override bool IsOverride => true;

		internal override bool IsMetadataFinal => false;

		internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
		{
			AnonymousTypeManager manager = ((AnonymousTypeTemplateSymbol)ContainingType).Manager;
			SyntheticBoundNodeFactory syntheticBoundNodeFactory = CreateBoundNodeFactory(compilationState, diagnostics);
			AnonymousTypeTemplateSymbol anonymousTypeTemplateSymbol = (AnonymousTypeTemplateSymbol)ContainingType;
			int num = 0;
			foreach (AnonymousTypePropertySymbol property in anonymousTypeTemplateSymbol.Properties)
			{
				num = num * -1521134295 + Hash.GetFNVHashCode(property.BackingField.Name);
			}
			BoundExpression boundExpression = syntheticBoundNodeFactory.Literal(num);
			MethodSymbol system_Collections_Generic_EqualityComparer_T__GetHashCode = manager.System_Collections_Generic_EqualityComparer_T__GetHashCode;
			MethodSymbol system_Collections_Generic_EqualityComparer_T__get_Default = manager.System_Collections_Generic_EqualityComparer_T__get_Default;
			BoundLiteral boundHashFactor = null;
			for (int i = 0; i < anonymousTypeTemplateSymbol.Properties.Length; i++)
			{
				boundExpression = MethodBodySynthesizer.GenerateHashCombine(boundExpression, system_Collections_Generic_EqualityComparer_T__GetHashCode, system_Collections_Generic_EqualityComparer_T__get_Default, ref boundHashFactor, syntheticBoundNodeFactory.Field(syntheticBoundNodeFactory.This(), anonymousTypeTemplateSymbol.Properties[i].BackingField), syntheticBoundNodeFactory);
			}
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.Block(syntheticBoundNodeFactory.Return(boundExpression)));
		}

		internal AnonymousTypeGetHashCodeMethodSymbol(NamedTypeSymbol container)
			: base(container, "GetHashCode")
		{
		}

		internal sealed override bool IsMetadataVirtual(IsMetadataVirtualOption option = IsMetadataVirtualOption.None)
		{
			return true;
		}
	}

	private sealed class AnonymousTypeToStringMethodSymbol : SynthesizedMethodBase
	{
		internal override bool HasSpecialName => false;

		public override MethodKind MethodKind => MethodKind.Ordinary;

		public override bool ReturnsVoid => false;

		public override RefKind RefKind => RefKind.None;

		public override TypeWithAnnotations ReturnTypeWithAnnotations => TypeWithAnnotations.Create(base.Manager.System_String, NullableAnnotation.NotAnnotated);

		public override ImmutableArray<ParameterSymbol> Parameters => ImmutableArray<ParameterSymbol>.Empty;

		public override bool IsOverride => true;

		internal override bool IsMetadataFinal => false;

		internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
		{
			AnonymousTypeManager manager = ((AnonymousTypeTemplateSymbol)ContainingType).Manager;
			SyntheticBoundNodeFactory syntheticBoundNodeFactory = CreateBoundNodeFactory(compilationState, diagnostics);
			AnonymousTypeTemplateSymbol anonymousTypeTemplateSymbol = (AnonymousTypeTemplateSymbol)ContainingType;
			int length = anonymousTypeTemplateSymbol.Properties.Length;
			BoundExpression boundExpression = null;
			if (length > 0)
			{
				BoundExpression[] array = new BoundExpression[length];
				PooledStringBuilder instance = PooledStringBuilder.GetInstance();
				for (int i = 0; i < length; i++)
				{
					AnonymousTypePropertySymbol anonymousTypePropertySymbol = anonymousTypeTemplateSymbol.Properties[i];
					instance.Builder.AppendFormat((i == 0) ? "{{{{ {0} = {{{1}}}" : ", {0} = {{{1}}}", anonymousTypePropertySymbol.Name, i);
					array[i] = syntheticBoundNodeFactory.Convert(manager.System_Object, new BoundLoweredConditionalAccess(syntheticBoundNodeFactory.Syntax, syntheticBoundNodeFactory.Field(syntheticBoundNodeFactory.This(), anonymousTypePropertySymbol.BackingField), null, syntheticBoundNodeFactory.Call(new BoundConditionalReceiver(syntheticBoundNodeFactory.Syntax, i, anonymousTypePropertySymbol.BackingField.Type), manager.System_Object__ToString), null, i, forceCopyOfNullableValueType: true, manager.System_String), Conversion.ImplicitReference);
				}
				instance.Builder.Append(" }}");
				BoundExpression boundExpression2 = syntheticBoundNodeFactory.Literal(instance.ToStringAndFree());
				MethodSymbol system_String__Format_IFormatProvider = manager.System_String__Format_IFormatProvider;
				boundExpression = syntheticBoundNodeFactory.StaticCall(manager.System_String, system_String__Format_IFormatProvider, syntheticBoundNodeFactory.Null(system_String__Format_IFormatProvider.Parameters[0].Type), boundExpression2, syntheticBoundNodeFactory.ArrayOrEmpty(manager.System_Object, array));
			}
			else
			{
				boundExpression = syntheticBoundNodeFactory.Literal("{ }");
			}
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.Block(syntheticBoundNodeFactory.Return(boundExpression)));
		}

		internal AnonymousTypeToStringMethodSymbol(NamedTypeSymbol container)
			: base(container, "ToString")
		{
		}

		internal sealed override bool IsMetadataVirtual(IsMetadataVirtualOption option = IsMetadataVirtualOption.None)
		{
			return true;
		}
	}

	private readonly struct SynthesizedDelegateKey : IEquatable<SynthesizedDelegateKey>
	{
		internal readonly string? Name;

		internal readonly int ParameterCount;

		internal readonly AnonymousTypeDescriptor TypeDescriptor;

		public SynthesizedDelegateKey(int parameterCount, RefKindVector byRefs, bool returnsVoid, int generation)
		{
			Name = GeneratedNames.MakeSynthesizedDelegateName(byRefs, returnsVoid, generation);
			ParameterCount = parameterCount;
			TypeDescriptor = default(AnonymousTypeDescriptor);
		}

		public SynthesizedDelegateKey(AnonymousTypeDescriptor typeDescr)
		{
			Name = null;
			ParameterCount = -1;
			TypeDescriptor = typeDescr;
		}

		public override bool Equals(object? obj)
		{
			if (obj is SynthesizedDelegateKey)
			{
				return Equals((SynthesizedDelegateKey)obj);
			}
			return false;
		}

		public bool Equals(SynthesizedDelegateKey other)
		{
			if (!string.Equals(Name, other.Name))
			{
				return false;
			}
			if (Name == null)
			{
				return TypeDescriptor.Equals(other.TypeDescriptor);
			}
			return ParameterCount == other.ParameterCount;
		}

		public override int GetHashCode()
		{
			if (Name == null)
			{
				return TypeDescriptor.GetHashCode();
			}
			return Hash.Combine(ParameterCount, Name.GetHashCode());
		}
	}

	private class SynthesizedDelegateSymbolComparer : IComparer<AnonymousDelegateTemplateSymbol>
	{
		public static readonly SynthesizedDelegateSymbolComparer Instance = new SynthesizedDelegateSymbolComparer();

		public int Compare(AnonymousDelegateTemplateSymbol x, AnonymousDelegateTemplateSymbol y)
		{
			return x.MetadataName.CompareTo(y.MetadataName);
		}
	}

	private sealed class AnonymousTypeOrDelegateComparer : IComparer<AnonymousTypeOrDelegateTemplateSymbol>
	{
		private readonly CSharpCompilation _compilation;

		public AnonymousTypeOrDelegateComparer(CSharpCompilation compilation)
		{
			_compilation = compilation;
		}

		public int Compare(AnonymousTypeOrDelegateTemplateSymbol x, AnonymousTypeOrDelegateTemplateSymbol y)
		{
			if ((object)x == y)
			{
				return 0;
			}
			int num = CompareLocations(x.SmallestLocation, y.SmallestLocation);
			if (num == 0)
			{
				num = string.CompareOrdinal(x.TypeDescriptorKey, y.TypeDescriptorKey);
			}
			return num;
		}

		private int CompareLocations(Location x, Location y)
		{
			if (x == y)
			{
				return 0;
			}
			if (x == Location.None)
			{
				return -1;
			}
			if (y == Location.None)
			{
				return 1;
			}
			return _compilation.CompareSourceLocations(x, y);
		}
	}

	internal abstract class AnonymousTypeOrDelegatePublicSymbol : NamedTypeSymbol
	{
		internal readonly AnonymousTypeManager Manager;

		internal readonly AnonymousTypeDescriptor TypeDescriptor;

		internal sealed override bool HasCodeAnalysisEmbeddedAttribute => false;

		internal sealed override bool HasCompilerLoweringPreserveAttribute => false;

		internal sealed override bool IsInterpolatedStringHandlerType => false;

		internal sealed override ParameterSymbol? ExtensionParameter => null;

		public sealed override Symbol ContainingSymbol => Manager.Compilation.SourceModule.GlobalNamespace;

		public sealed override string Name => string.Empty;

		public sealed override string MetadataName => string.Empty;

		internal sealed override bool MangleName => false;

		internal sealed override bool IsFileLocal => false;

		internal sealed override FileIdentifier? AssociatedFileIdentifier => null;

		internal sealed override string? ExtensionGroupingName => null;

		internal sealed override string? ExtensionMarkerName => null;

		public sealed override int Arity => 0;

		public abstract override bool IsImplicitlyDeclared { get; }

		public sealed override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

		internal sealed override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotationsNoUseSiteDiagnostics => ImmutableArray<TypeWithAnnotations>.Empty;

		public sealed override bool IsAbstract => false;

		public sealed override bool IsRefLikeType => false;

		public sealed override bool IsReadOnly => false;

		public sealed override bool IsSealed => true;

		public sealed override bool MightContainExtensionMethods => false;

		internal sealed override bool HasSpecialName => false;

		internal override bool HasDeclaredRequiredMembers => false;

		public sealed override Accessibility DeclaredAccessibility => Accessibility.Internal;

		internal abstract override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics { get; }

		public abstract override TypeKind TypeKind { get; }

		internal sealed override bool IsInterface => false;

		public sealed override ImmutableArray<Location> Locations => ImmutableArray.Create(TypeDescriptor.Location);

		public abstract override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

		public sealed override bool IsStatic => false;

		public sealed override bool IsAnonymousType => true;

		public sealed override NamedTypeSymbol ConstructedFrom => this;

		internal sealed override bool ShouldAddWinRTMembers => false;

		internal sealed override bool IsWindowsRuntimeImport => false;

		internal sealed override bool IsComImport => false;

		internal sealed override ObsoleteAttributeData? ObsoleteAttributeData => null;

		internal sealed override TypeLayout Layout => default(TypeLayout);

		internal sealed override CharSet MarshallingCharSet => base.DefaultMarshallingCharSet;

		public sealed override bool IsSerializable => false;

		public sealed override bool AreLocalsZeroed
		{
			get
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/AnonymousTypes/PublicSymbols/AnonymousManager.TypeOrDelegatePublicSymbol.cs", 242);
			}
		}

		internal sealed override bool HasDeclarativeSecurity => false;

		internal sealed override NamedTypeSymbol? NativeIntegerUnderlyingType => null;

		internal sealed override bool IsRecord => false;

		internal sealed override bool IsRecordStruct => false;

		internal AnonymousTypeOrDelegatePublicSymbol(AnonymousTypeManager manager, AnonymousTypeDescriptor typeDescr)
		{
			Manager = manager;
			TypeDescriptor = typeDescr;
		}

		internal abstract NamedTypeSymbol MapToImplementationSymbol();

		internal abstract AnonymousTypeOrDelegatePublicSymbol SubstituteTypes(AbstractTypeMap typeMap);

		protected override NamedTypeSymbol WithTupleDataCore(TupleExtraData newData)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/AnonymousTypes/PublicSymbols/AnonymousManager.TypeOrDelegatePublicSymbol.cs", 37);
		}

		internal sealed override IEnumerable<FieldSymbol> GetFieldsToEmit()
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/AnonymousTypes/PublicSymbols/AnonymousManager.TypeOrDelegatePublicSymbol.cs", 41);
		}

		internal sealed override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers()
		{
			return GetMembersUnordered();
		}

		internal sealed override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers(string name)
		{
			return GetMembers(name);
		}

		public sealed override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}

		public sealed override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name)
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}

		public sealed override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name, int arity)
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}

		internal sealed override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<TypeSymbol> basesBeingResolved)
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}

		internal sealed override ImmutableArray<NamedTypeSymbol> GetInterfacesToEmit()
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/AnonymousTypes/PublicSymbols/AnonymousManager.TypeOrDelegatePublicSymbol.cs", 168);
		}

		internal sealed override IEnumerable<SecurityAttribute> GetSecurityInformation()
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/AnonymousTypes/PublicSymbols/AnonymousManager.TypeOrDelegatePublicSymbol.cs", 252);
		}

		internal sealed override ImmutableArray<string> GetAppliedConditionalSymbols()
		{
			return ImmutableArray<string>.Empty;
		}

		internal sealed override AttributeUsageInfo GetAttributeUsageInfo()
		{
			return AttributeUsageInfo.Null;
		}

		internal sealed override NamedTypeSymbol GetDeclaredBaseType(ConsList<TypeSymbol> basesBeingResolved)
		{
			return BaseTypeNoUseSiteDiagnostics;
		}

		internal sealed override ImmutableArray<NamedTypeSymbol> GetDeclaredInterfaces(ConsList<TypeSymbol> basesBeingResolved)
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}

		internal sealed override NamedTypeSymbol AsNativeInteger()
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/AnonymousTypes/PublicSymbols/AnonymousManager.TypeOrDelegatePublicSymbol.cs", 275);
		}

		internal abstract override bool Equals(TypeSymbol t2, TypeCompareKind comparison);

		public abstract override int GetHashCode();

		internal sealed override bool HasPossibleWellKnownCloneMethod()
		{
			return false;
		}

		internal sealed override IEnumerable<(MethodSymbol Body, MethodSymbol Implemented)> SynthesizedInterfaceMethodImpls()
		{
			return SpecializedCollections.EmptyEnumerable<(MethodSymbol, MethodSymbol)>();
		}

		internal override bool GetGuidString(out string? guidString)
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

	internal sealed class AnonymousDelegatePublicSymbol : AnonymousTypeOrDelegatePublicSymbol
	{
		private ImmutableArray<Symbol> _lazyMembers;

		public override TypeKind TypeKind => TypeKind.Delegate;

		internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => Manager.System_MulticastDelegate;

		public override IEnumerable<string> MemberNames => GetMembers().SelectAsArray((Symbol member) => member.Name);

		public override bool IsImplicitlyDeclared => true;

		public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

		internal AnonymousDelegatePublicSymbol(AnonymousTypeManager manager, AnonymousTypeDescriptor typeDescr)
			: base(manager, typeDescr)
		{
		}

		internal override NamedTypeSymbol MapToImplementationSymbol()
		{
			return Manager.ConstructAnonymousDelegateImplementationSymbol(this, 0);
		}

		internal override AnonymousTypeOrDelegatePublicSymbol SubstituteTypes(AbstractTypeMap map)
		{
			AnonymousTypeDescriptor typeDescr = TypeDescriptor.SubstituteTypes(map, out var changed);
			if (!changed)
			{
				return this;
			}
			return new AnonymousDelegatePublicSymbol(Manager, typeDescr);
		}

		public override ImmutableArray<Symbol> GetMembers()
		{
			if (_lazyMembers.IsDefault)
			{
				ImmutableInterlocked.InterlockedInitialize(ref _lazyMembers, CreateMembers());
			}
			return _lazyMembers;
		}

		private ImmutableArray<Symbol> CreateMembers()
		{
			SynthesizedDelegateConstructor item = new SynthesizedDelegateConstructor(this, Manager.System_Object, Manager.System_IntPtr);
			ImmutableArray<AnonymousTypeField> fields = TypeDescriptor.Fields;
			int num = fields.Length - 1;
			ArrayBuilder<SynthesizedDelegateInvokeMethod.ParameterDescription> instance = ArrayBuilder<SynthesizedDelegateInvokeMethod.ParameterDescription>.GetInstance(num);
			for (int i = 0; i < num; i++)
			{
				AnonymousTypeField anonymousTypeField = fields[i];
				instance.Add(new SynthesizedDelegateInvokeMethod.ParameterDescription(anonymousTypeField.TypeWithAnnotations, anonymousTypeField.RefKind, anonymousTypeField.Scope, anonymousTypeField.DefaultValue, anonymousTypeField.IsParams, anonymousTypeField.HasUnscopedRefAttribute));
			}
			AnonymousTypeField anonymousTypeField2 = fields.Last();
			SynthesizedDelegateInvokeMethod item2 = new SynthesizedDelegateInvokeMethod(this, instance, anonymousTypeField2.TypeWithAnnotations, anonymousTypeField2.RefKind);
			instance.Free();
			return ImmutableArray.Create((Symbol)item, (Symbol)item2);
		}

		public override ImmutableArray<Symbol> GetMembers(string name)
		{
			return GetMembers().WhereAsArray((Symbol member, string text) => member.Name == text, name);
		}

		internal override bool Equals(TypeSymbol t2, TypeCompareKind comparison)
		{
			if ((object)this == t2)
			{
				return true;
			}
			if (t2 is AnonymousDelegatePublicSymbol anonymousDelegatePublicSymbol)
			{
				return TypeDescriptor.Equals(anonymousDelegatePublicSymbol.TypeDescriptor, comparison);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return TypeDescriptor.GetHashCode();
		}
	}

	internal sealed class AnonymousTypePublicSymbol : AnonymousTypeOrDelegatePublicSymbol
	{
		private readonly ImmutableArray<Symbol> _members;

		internal readonly ImmutableArray<AnonymousTypePropertySymbol> Properties;

		private readonly MultiDictionary<string, Symbol> _nameToSymbols = new MultiDictionary<string, Symbol>();

		public override TypeKind TypeKind => TypeKind.Class;

		internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => Manager.System_Object;

		public override IEnumerable<string> MemberNames => _nameToSymbols.Keys;

		public override bool IsImplicitlyDeclared => false;

		public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => Symbol.GetDeclaringSyntaxReferenceHelper<AnonymousObjectCreationExpressionSyntax>(Locations);

		internal AnonymousTypePublicSymbol(AnonymousTypeManager manager, AnonymousTypeDescriptor typeDescr)
			: base(manager, typeDescr)
		{
			ImmutableArray<AnonymousTypeField> fields = typeDescr.Fields;
			ImmutableArray<AnonymousTypePropertySymbol> properties = fields.SelectAsArray((AnonymousTypeField field, int i, AnonymousTypePublicSymbol type) => new AnonymousTypePropertySymbol(type, field, i), this);
			ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(fields.Length * 2 + 1);
			foreach (AnonymousTypePropertySymbol item in properties)
			{
				instance.Add(item);
				instance.Add(item.GetMethod);
			}
			Properties = properties;
			instance.Add(new AnonymousTypeConstructorSymbol(this, properties));
			_members = instance.ToImmutableAndFree();
			foreach (Symbol member in _members)
			{
				_nameToSymbols.Add(member.Name, member);
			}
		}

		internal override NamedTypeSymbol MapToImplementationSymbol()
		{
			return Manager.ConstructAnonymousTypeImplementationSymbol(this);
		}

		internal override AnonymousTypeOrDelegatePublicSymbol SubstituteTypes(AbstractTypeMap map)
		{
			ImmutableArray<TypeWithAnnotations> immutableArray = TypeDescriptor.Fields.SelectAsArray((AnonymousTypeField f) => f.TypeWithAnnotations);
			ImmutableArray<TypeWithAnnotations> immutableArray2 = map.SubstituteTypes(immutableArray);
			if (!(immutableArray == immutableArray2))
			{
				return new AnonymousTypePublicSymbol(Manager, TypeDescriptor.WithNewFieldsTypes(immutableArray2));
			}
			return this;
		}

		public override ImmutableArray<Symbol> GetMembers()
		{
			return _members;
		}

		public override ImmutableArray<Symbol> GetMembers(string name)
		{
			MultiDictionary<string, Symbol>.ValueSet valueSet = _nameToSymbols[name];
			ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(valueSet.Count);
			foreach (Symbol item in valueSet)
			{
				instance.Add(item);
			}
			return instance.ToImmutableAndFree();
		}

		internal override bool Equals(TypeSymbol t2, TypeCompareKind comparison)
		{
			if ((object)this == t2)
			{
				return true;
			}
			if (t2 is AnonymousTypePublicSymbol anonymousTypePublicSymbol)
			{
				return TypeDescriptor.Equals(anonymousTypePublicSymbol.TypeDescriptor, comparison);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return TypeDescriptor.GetHashCode();
		}
	}

	internal sealed class AnonymousDelegateTemplateSymbol : AnonymousTypeOrDelegateTemplateSymbol
	{
		private readonly ImmutableArray<Symbol> _members;

		internal readonly bool HasIndexedName;

		public new MethodSymbol DelegateInvokeMethod => (MethodSymbol)_members[1];

		internal override string TypeDescriptorKey
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override TypeKind TypeKind => TypeKind.Delegate;

		public override IEnumerable<string> MemberNames => GetMembers().SelectAsArray((Symbol member) => member.Name);

		internal override bool HasDeclaredRequiredMembers => false;

		internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => Manager.System_MulticastDelegate;

		public override ImmutableArray<TypeParameterSymbol> TypeParameters { get; }

		internal AnonymousDelegateTemplateSymbol(AnonymousTypeManager manager, string name, TypeSymbol objectType, TypeSymbol intPtrType, TypeSymbol? voidReturnTypeOpt, int parameterCount, RefKindVector refKinds)
			: base(manager, Location.None)
		{
			HasIndexedName = false;
			TypeParameters = CreateTypeParameters(this, parameterCount, (object)voidReturnTypeOpt != null, hasParamsArray: false);
			base.NameAndIndex = new NameAndIndex(name, 0);
			SynthesizedDelegateConstructor constructor = new SynthesizedDelegateConstructor(this, objectType, intPtrType);
			SynthesizedDelegateInvokeMethod invokeMethod = createInvokeMethod(this, refKinds, voidReturnTypeOpt);
			_members = CreateMembers(constructor, invokeMethod);
			static SynthesizedDelegateInvokeMethod createInvokeMethod(AnonymousDelegateTemplateSymbol containingType, RefKindVector refKindVector, TypeSymbol? typeSymbol)
			{
				ImmutableArray<TypeParameterSymbol> typeParameters = containingType.TypeParameters;
				int num = typeParameters.Length - (((object)typeSymbol == null) ? 1 : 0);
				ArrayBuilder<SynthesizedDelegateInvokeMethod.ParameterDescription> instance = ArrayBuilder<SynthesizedDelegateInvokeMethod.ParameterDescription>.GetInstance(num);
				for (int i = 0; i < num; i++)
				{
					instance.Add(new SynthesizedDelegateInvokeMethod.ParameterDescription(TypeWithAnnotations.Create(typeParameters[i]), (!refKindVector.IsNull) ? refKindVector[i] : RefKind.None, ScopedKind.None, null, isParams: false, hasUnscopedRefAttribute: false));
				}
				TypeWithAnnotations returnType = TypeWithAnnotations.Create(typeSymbol ?? typeParameters[num]);
				RefKind refKind = ((!refKindVector.IsNull && (object)typeSymbol == null) ? refKindVector[num] : RefKind.None);
				SynthesizedDelegateInvokeMethod result = new SynthesizedDelegateInvokeMethod(containingType, instance, returnType, refKind);
				instance.Free();
				return result;
			}
		}

		private static ImmutableArray<TypeParameterSymbol> CreateTypeParameters(AnonymousDelegateTemplateSymbol containingType, int parameterCount, bool returnsVoid, bool hasParamsArray)
		{
			bool runtimeSupportsByRefLikeGenerics = containingType.ContainingAssembly.RuntimeSupportsByRefLikeGenerics;
			ArrayBuilder<TypeParameterSymbol> instance = ArrayBuilder<TypeParameterSymbol>.GetInstance(parameterCount + ((!returnsVoid) ? 1 : 0));
			for (int i = 0; i < parameterCount; i++)
			{
				instance.Add(new AnonymousTypeParameterSymbol(containingType, i, "T" + (i + 1), runtimeSupportsByRefLikeGenerics && (!hasParamsArray || i != parameterCount - 1)));
			}
			if (!returnsVoid)
			{
				instance.Add(new AnonymousTypeParameterSymbol(containingType, parameterCount, "TResult", runtimeSupportsByRefLikeGenerics));
			}
			return instance.ToImmutableAndFree();
		}

		internal AnonymousDelegateTemplateSymbol(AnonymousTypeManager manager, AnonymousTypeDescriptor typeDescr)
			: base(manager, typeDescr.Location)
		{
			HasIndexedName = true;
			int parameterCount = typeDescr.Fields.Length - 1;
			ImmutableArray<AnonymousTypeField> fields = typeDescr.Fields;
			bool returnsVoid = fields[fields.Length - 1].Type.IsVoidType();
			ImmutableArray<AnonymousTypeField> fields2 = typeDescr.Fields;
			int length = fields2.Length;
			TypeParameters = CreateTypeParameters(this, parameterCount, returnsVoid, length >= 2 && fields2[length - 2].IsParams);
			SynthesizedDelegateConstructor constructor = new SynthesizedDelegateConstructor(this, manager.System_Object, manager.System_IntPtr);
			SynthesizedDelegateInvokeMethod invokeMethod = createInvokeMethod(this, typeDescr.Fields);
			_members = CreateMembers(constructor, invokeMethod);
			static SynthesizedDelegateInvokeMethod createInvokeMethod(AnonymousDelegateTemplateSymbol containingType, ImmutableArray<AnonymousTypeField> immutableArray)
			{
				ImmutableArray<TypeParameterSymbol> typeParameters = containingType.TypeParameters;
				AnonymousTypeField anonymousTypeField = immutableArray[immutableArray.Length - 1];
				bool flag = anonymousTypeField.Type.IsVoidType();
				int num = immutableArray.Length - 1;
				ArrayBuilder<SynthesizedDelegateInvokeMethod.ParameterDescription> instance = ArrayBuilder<SynthesizedDelegateInvokeMethod.ParameterDescription>.GetInstance(num);
				for (int i = 0; i < num; i++)
				{
					AnonymousTypeField anonymousTypeField2 = immutableArray[i];
					TypeWithAnnotations typeWithAnnotations = TypeWithAnnotations.Create(typeParameters[i]);
					if (anonymousTypeField2.IsParams)
					{
						typeWithAnnotations = TypeWithAnnotations.Create(ArrayTypeSymbol.CreateSZArray(containingType.ContainingAssembly, typeWithAnnotations));
					}
					instance.Add(new SynthesizedDelegateInvokeMethod.ParameterDescription(typeWithAnnotations, anonymousTypeField2.RefKind, anonymousTypeField2.Scope, anonymousTypeField2.DefaultValue, anonymousTypeField2.IsParams, anonymousTypeField2.HasUnscopedRefAttribute));
				}
				TypeWithAnnotations returnType = TypeWithAnnotations.Create(flag ? anonymousTypeField.Type : typeParameters[num]);
				RefKind refKind = anonymousTypeField.RefKind;
				SynthesizedDelegateInvokeMethod result = new SynthesizedDelegateInvokeMethod(containingType, instance, returnType, refKind);
				instance.Free();
				return result;
			}
		}

		internal AnonymousDelegateTemplateSymbol(AnonymousTypeManager manager, AnonymousTypeDescriptor typeDescr, ImmutableArray<TypeParameterSymbol> typeParametersToSubstitute)
			: base(manager, typeDescr.Location)
		{
			HasIndexedName = true;
			int length = typeParametersToSubstitute.Length;
			TypeMap typeMap;
			if (length == 0)
			{
				TypeParameters = ImmutableArray<TypeParameterSymbol>.Empty;
				typeMap = TypeMap.Empty;
			}
			else
			{
				ArrayBuilder<TypeParameterSymbol> instance = ArrayBuilder<TypeParameterSymbol>.GetInstance(length);
				for (int i = 0; i < length; i++)
				{
					instance.Add(new AnonymousTypeParameterSymbol(this, i, "T" + (i + 1), typeParametersToSubstitute[i].AllowsRefLikeType));
				}
				TypeParameters = instance.ToImmutableAndFree();
				typeMap = new TypeMap(typeParametersToSubstitute, TypeParameters, allowAlpha: true);
			}
			SynthesizedDelegateConstructor constructor = new SynthesizedDelegateConstructor(this, manager.System_Object, manager.System_IntPtr);
			SynthesizedDelegateInvokeMethod invokeMethod = createInvokeMethod(this, typeDescr.Fields, typeMap);
			_members = CreateMembers(constructor, invokeMethod);
			static SynthesizedDelegateInvokeMethod createInvokeMethod(AnonymousDelegateTemplateSymbol containingType, ImmutableArray<AnonymousTypeField> fields, TypeMap typeMap2)
			{
				int num = fields.Length - 1;
				ArrayBuilder<SynthesizedDelegateInvokeMethod.ParameterDescription> instance2 = ArrayBuilder<SynthesizedDelegateInvokeMethod.ParameterDescription>.GetInstance(num);
				for (int j = 0; j < num; j++)
				{
					AnonymousTypeField anonymousTypeField = fields[j];
					instance2.Add(new SynthesizedDelegateInvokeMethod.ParameterDescription(typeMap2.SubstituteType(anonymousTypeField.Type), anonymousTypeField.RefKind, anonymousTypeField.Scope, anonymousTypeField.DefaultValue, anonymousTypeField.IsParams, anonymousTypeField.HasUnscopedRefAttribute));
				}
				AnonymousTypeField anonymousTypeField2 = fields[fields.Length - 1];
				TypeWithAnnotations returnType = typeMap2.SubstituteType(anonymousTypeField2.Type);
				RefKind refKind = anonymousTypeField2.RefKind;
				SynthesizedDelegateInvokeMethod result = new SynthesizedDelegateInvokeMethod(containingType, instance2, returnType, refKind);
				instance2.Free();
				return result;
			}
		}

		private static ImmutableArray<Symbol> CreateMembers(MethodSymbol constructor, MethodSymbol invokeMethod)
		{
			return ImmutableArray.Create((Symbol)constructor, (Symbol)invokeMethod);
		}

		public override ImmutableArray<Symbol> GetMembers()
		{
			return _members;
		}

		public override ImmutableArray<Symbol> GetMembers(string name)
		{
			return GetMembers().WhereAsArray((Symbol member, string text) => member.Name == text, name);
		}

		internal override IEnumerable<FieldSymbol> GetFieldsToEmit()
		{
			return SpecializedCollections.EmptyEnumerable<FieldSymbol>();
		}

		internal override ImmutableArray<NamedTypeSymbol> GetInterfacesToEmit()
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}

		internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<TypeSymbol>? basesBeingResolved = null)
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}

		internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
		{
			base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
			CSharpCompilation declaringCompilation = ContainingSymbol.DeclaringCompilation;
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor));
		}
	}

	private sealed class AnonymousTypeFieldSymbol : FieldSymbol
	{
		private readonly PropertySymbol _property;

		public override RefKind RefKind => RefKind.None;

		public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

		public override string Name => GeneratedNames.MakeAnonymousTypeBackingFieldName(_property.Name);

		public override FlowAnalysisAnnotations FlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

		internal override bool HasSpecialName => false;

		internal override bool HasRuntimeSpecialName => false;

		internal override bool IsNotSerialized => false;

		internal override MarshalPseudoCustomAttributeData MarshallingInformation => null;

		internal override int? TypeLayoutOffset => null;

		public override Symbol AssociatedSymbol => _property;

		public override bool IsReadOnly => true;

		public override bool IsVolatile => false;

		public override bool IsConst => false;

		internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

		public override Symbol ContainingSymbol => _property.ContainingType;

		public override NamedTypeSymbol ContainingType => _property.ContainingType;

		public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

		public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

		public override Accessibility DeclaredAccessibility => Accessibility.Private;

		public override bool IsStatic => false;

		public override bool IsImplicitlyDeclared => true;

		internal override bool IsRequired => false;

		public AnonymousTypeFieldSymbol(PropertySymbol property)
		{
			_property = property;
		}

		internal override TypeWithAnnotations GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
		{
			return _property.TypeWithAnnotations;
		}

		internal override ConstantValue GetConstantValue(ConstantFieldsInProgress inProgress, bool earlyDecodingWellKnownAttributes)
		{
			return null;
		}

		internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
		{
			base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
			AnonymousTypeManager manager = ((AnonymousTypeTemplateSymbol)ContainingSymbol).Manager;
			Symbol.AddSynthesizedAttribute(ref attributes, manager.Compilation.TrySynthesizeAttribute(WellKnownMember.System_Diagnostics_DebuggerBrowsableAttribute__ctor, ImmutableArray.Create(new TypedConstant(manager.System_Diagnostics_DebuggerBrowsableState, TypedConstantKind.Enum, DebuggerBrowsableState.Never))));
		}
	}

	internal sealed class AnonymousTypePropertySymbol : PropertySymbol
	{
		private readonly NamedTypeSymbol _containingType;

		private readonly TypeWithAnnotations _typeWithAnnotations;

		private readonly string _name;

		private readonly int _index;

		private readonly ImmutableArray<Location> _locations;

		private readonly AnonymousTypePropertyGetAccessorSymbol _getMethod;

		private readonly FieldSymbol _backingField;

		internal override int? MemberIndexOpt => _index;

		public override RefKind RefKind => RefKind.None;

		public override TypeWithAnnotations TypeWithAnnotations => _typeWithAnnotations;

		public override string Name => _name;

		internal override bool HasSpecialName => false;

		public override bool IsImplicitlyDeclared => false;

		public override ImmutableArray<Location> Locations => _locations;

		public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => Symbol.GetDeclaringSyntaxReferenceHelper<AnonymousObjectMemberDeclaratorSyntax>(Locations);

		public override bool IsStatic => false;

		public override bool IsOverride => false;

		public override bool IsVirtual => false;

		public override bool IsIndexer => false;

		public override bool IsSealed => false;

		public override bool IsAbstract => false;

		internal override bool IsRequired => false;

		internal sealed override bool HasUnscopedRefAttribute => false;

		internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

		public override ImmutableArray<ParameterSymbol> Parameters => ImmutableArray<ParameterSymbol>.Empty;

		public override MethodSymbol SetMethod => null;

		public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

		internal override Microsoft.Cci.CallingConvention CallingConvention => Microsoft.Cci.CallingConvention.HasThis;

		public override ImmutableArray<PropertySymbol> ExplicitInterfaceImplementations => ImmutableArray<PropertySymbol>.Empty;

		public override Symbol ContainingSymbol => _containingType;

		public override NamedTypeSymbol ContainingType => _containingType;

		public override Accessibility DeclaredAccessibility => Accessibility.Public;

		internal override bool MustCallMethodsDirectly => false;

		public override bool IsExtern => false;

		public override MethodSymbol GetMethod => _getMethod;

		public FieldSymbol BackingField => _backingField;

		internal AnonymousTypePropertySymbol(AnonymousTypeTemplateSymbol container, AnonymousTypeField field, TypeWithAnnotations fieldTypeWithAnnotations, int index)
			: this(container, field, fieldTypeWithAnnotations, index, ImmutableArray<Location>.Empty, includeBackingField: true)
		{
		}

		internal AnonymousTypePropertySymbol(AnonymousTypePublicSymbol container, AnonymousTypeField field, int index)
			: this(container, field, field.TypeWithAnnotations, index, ImmutableArray.Create(field.Location), includeBackingField: false)
		{
		}

		private AnonymousTypePropertySymbol(NamedTypeSymbol container, AnonymousTypeField field, TypeWithAnnotations fieldTypeWithAnnotations, int index, ImmutableArray<Location> locations, bool includeBackingField)
		{
			_containingType = container;
			_typeWithAnnotations = fieldTypeWithAnnotations;
			_name = field.Name;
			_index = index;
			_locations = locations;
			_getMethod = new AnonymousTypePropertyGetAccessorSymbol(this);
			_backingField = (includeBackingField ? new AnonymousTypeFieldSymbol(this) : null);
		}

		internal override int TryGetOverloadResolutionPriority()
		{
			return 0;
		}

		public override bool Equals(Symbol obj, TypeCompareKind compareKind)
		{
			if (obj == null)
			{
				return false;
			}
			if ((object)this == obj)
			{
				return true;
			}
			if (!(obj is AnonymousTypePropertySymbol anonymousTypePropertySymbol))
			{
				return false;
			}
			if ((object)anonymousTypePropertySymbol != null && anonymousTypePropertySymbol.Name == Name)
			{
				return anonymousTypePropertySymbol.ContainingType.Equals(ContainingType, compareKind);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(ContainingType.GetHashCode(), Name.GetHashCode());
		}
	}

	private abstract class SynthesizedMethodBase : SynthesizedMethodSymbol
	{
		private readonly NamedTypeSymbol _containingType;

		private readonly string _name;

		internal sealed override bool GenerateDebugInfo => false;

		public sealed override int Arity => 0;

		public sealed override Symbol ContainingSymbol => _containingType;

		public override NamedTypeSymbol ContainingType => _containingType;

		public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

		public sealed override Accessibility DeclaredAccessibility => Accessibility.Public;

		public sealed override bool IsStatic => false;

		public sealed override bool IsVirtual => false;

		public sealed override bool IsAsync => false;

		internal sealed override MethodImplAttributes ImplementationAttributes => MethodImplAttributes.IL;

		internal sealed override Microsoft.Cci.CallingConvention CallingConvention => Microsoft.Cci.CallingConvention.HasThis;

		public sealed override bool IsExtensionMethod => false;

		public sealed override bool HidesBaseMethodsByName => false;

		public sealed override bool IsVararg => false;

		public sealed override FlowAnalysisAnnotations ReturnTypeFlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

		public sealed override ImmutableHashSet<string> ReturnNotNullIfParameterNotNull => ImmutableHashSet<string>.Empty;

		public sealed override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotations => ImmutableArray<TypeWithAnnotations>.Empty;

		public sealed override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

		internal sealed override bool IsExplicitInterfaceImplementation => false;

		public sealed override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

		internal sealed override bool IsDeclaredReadOnly => false;

		internal sealed override bool IsInitOnly => false;

		public sealed override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

		public override Symbol AssociatedSymbol => null;

		public sealed override bool IsAbstract => false;

		public sealed override bool IsSealed => false;

		public sealed override bool IsExtern => false;

		public sealed override string Name => _name;

		protected AnonymousTypeManager Manager
		{
			get
			{
				if (!(_containingType is AnonymousTypeTemplateSymbol anonymousTypeTemplateSymbol))
				{
					return ((AnonymousTypePublicSymbol)_containingType).Manager;
				}
				return anonymousTypeTemplateSymbol.Manager;
			}
		}

		internal sealed override bool RequiresSecurityObject => false;

		internal sealed override MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation => null;

		internal sealed override bool HasDeclarativeSecurity => false;

		internal override bool SynthesizesLoweredBoundBody => true;

		protected override bool HasSetsRequiredMembersImpl
		{
			get
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/AnonymousTypes/SynthesizedSymbols/AnonymousType.SynthesizedMethodBase.cs", 235);
			}
		}

		public SynthesizedMethodBase(NamedTypeSymbol containingType, string name)
		{
			_containingType = containingType;
			_name = name;
		}

		internal sealed override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
		{
			return false;
		}

		internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
		{
			base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
			Symbol.AddSynthesizedAttribute(ref attributes, Manager.Compilation.TrySynthesizeAttribute(WellKnownMember.System_Diagnostics_DebuggerHiddenAttribute__ctor));
		}

		public sealed override DllImportData GetDllImportData()
		{
			return null;
		}

		internal sealed override IEnumerable<SecurityAttribute> GetSecurityInformation()
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/AnonymousTypes/SynthesizedSymbols/AnonymousType.SynthesizedMethodBase.cs", 207);
		}

		internal sealed override ImmutableArray<string> GetAppliedConditionalSymbols()
		{
			return ImmutableArray<string>.Empty;
		}

		protected SyntheticBoundNodeFactory CreateBoundNodeFactory(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
		{
			return new SyntheticBoundNodeFactory(this, this.GetNonNullSyntaxNode(), compilationState, diagnostics)
			{
				CurrentFunction = this
			};
		}

		internal sealed override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/AnonymousTypes/SynthesizedSymbols/AnonymousType.SynthesizedMethodBase.cs", 232);
		}
	}

	internal sealed class AnonymousTypeTemplateSymbol : AnonymousTypeOrDelegateTemplateSymbol
	{
		private readonly ImmutableArray<TypeParameterSymbol> _typeParameters;

		private readonly ImmutableArray<Symbol> _members;

		internal readonly ImmutableArray<MethodSymbol> SpecialMembers;

		internal readonly ImmutableArray<AnonymousTypePropertySymbol> Properties;

		private readonly MultiDictionary<string, Symbol> _nameToSymbols = new MultiDictionary<string, Symbol>();

		internal override string TypeDescriptorKey { get; }

		public override TypeKind TypeKind => TypeKind.Class;

		internal override bool HasDeclaredRequiredMembers => false;

		internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => Manager.System_Object;

		public override ImmutableArray<TypeParameterSymbol> TypeParameters => _typeParameters;

		public override IEnumerable<string> MemberNames => _nameToSymbols.Keys;

		internal AnonymousTypeTemplateSymbol(AnonymousTypeManager manager, AnonymousTypeDescriptor typeDescr)
			: base(manager, typeDescr.Location)
		{
			TypeDescriptorKey = typeDescr.Key;
			int length = typeDescr.Fields.Length;
			ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(length * 3 + 1);
			ArrayBuilder<AnonymousTypePropertySymbol> instance2 = ArrayBuilder<AnonymousTypePropertySymbol>.GetInstance(length);
			ArrayBuilder<TypeParameterSymbol> instance3 = ArrayBuilder<TypeParameterSymbol>.GetInstance(length);
			for (int i = 0; i < length; i++)
			{
				AnonymousTypeField field = typeDescr.Fields[i];
				AnonymousTypeParameterSymbol anonymousTypeParameterSymbol = new AnonymousTypeParameterSymbol(this, i, GeneratedNames.MakeAnonymousTypeParameterName(field.Name), allowsRefLikeType: false);
				instance3.Add(anonymousTypeParameterSymbol);
				AnonymousTypePropertySymbol anonymousTypePropertySymbol = new AnonymousTypePropertySymbol(this, field, TypeWithAnnotations.Create(anonymousTypeParameterSymbol), i);
				instance2.Add(anonymousTypePropertySymbol);
				instance.Add(anonymousTypePropertySymbol);
				instance.Add(anonymousTypePropertySymbol.BackingField);
				instance.Add(anonymousTypePropertySymbol.GetMethod);
			}
			_typeParameters = instance3.ToImmutableAndFree();
			Properties = instance2.ToImmutableAndFree();
			instance.Add(new AnonymousTypeConstructorSymbol(this, Properties));
			_members = instance.ToImmutableAndFree();
			foreach (Symbol member in _members)
			{
				_nameToSymbols.Add(member.Name, member);
			}
			SpecialMembers = ImmutableArray.Create<MethodSymbol>(new AnonymousTypeEqualsMethodSymbol(this), new AnonymousTypeGetHashCodeMethodSymbol(this), new AnonymousTypeToStringMethodSymbol(this));
		}

		internal AnonymousTypeKey GetAnonymousTypeKey()
		{
			return new AnonymousTypeKey(Properties.SelectAsArray((AnonymousTypePropertySymbol p) => new AnonymousTypeKeyField(p.Name, isKey: false, ignoreCase: false)));
		}

		public override ImmutableArray<Symbol> GetMembers()
		{
			return _members;
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

		public override ImmutableArray<Symbol> GetMembers(string name)
		{
			MultiDictionary<string, Symbol>.ValueSet valueSet = _nameToSymbols[name];
			ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(valueSet.Count);
			foreach (Symbol item in valueSet)
			{
				instance.Add(item);
			}
			return instance.ToImmutableAndFree();
		}

		internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<TypeSymbol> basesBeingResolved)
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}

		internal override ImmutableArray<NamedTypeSymbol> GetInterfacesToEmit()
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}

		internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
		{
			base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
			Symbol.AddSynthesizedAttribute(ref attributes, Manager.Compilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor));
			if (Manager.Compilation.Options.OptimizationLevel == OptimizationLevel.Debug)
			{
				Symbol.AddSynthesizedAttribute(ref attributes, TrySynthesizeDebuggerDisplayAttribute());
			}
		}

		private SynthesizedAttributeData TrySynthesizeDebuggerDisplayAttribute()
		{
			string value;
			if (Properties.Length == 0)
			{
				value = "\\{ }";
			}
			else
			{
				PooledStringBuilder instance = PooledStringBuilder.GetInstance();
				StringBuilder builder = instance.Builder;
				builder.Append("\\{ ");
				int num = Math.Min(Properties.Length, 10);
				for (int i = 0; i < num; i++)
				{
					string name = Properties[i].Name;
					if (i > 0)
					{
						builder.Append(", ");
					}
					builder.Append(name);
					builder.Append(" = {");
					builder.Append(name);
					builder.Append('}');
				}
				if (Properties.Length > num)
				{
					builder.Append(" ...");
				}
				builder.Append(" }");
				value = instance.ToStringAndFree();
			}
			return Manager.Compilation.TrySynthesizeAttribute(WellKnownMember.System_Diagnostics_DebuggerDisplayAttribute__ctor, ImmutableArray.Create(new TypedConstant(Manager.System_String, TypedConstantKind.Primitive, value)), ImmutableArray.Create(new KeyValuePair<WellKnownMember, TypedConstant>(WellKnownMember.System_Diagnostics_DebuggerDisplayAttribute__Type, new TypedConstant(Manager.System_String, TypedConstantKind.Primitive, "<Anonymous Type>"))));
		}
	}

	internal sealed class NameAndIndex
	{
		public readonly string Name;

		public readonly int Index;

		public NameAndIndex(string name, int index)
		{
			Name = name;
			Index = index;
		}
	}

	internal abstract class AnonymousTypeOrDelegateTemplateSymbol : NamedTypeSymbol
	{
		private NameAndIndex? _nameAndIndex;

		private Location _smallestLocation;

		internal readonly AnonymousTypeManager Manager;

		internal abstract string TypeDescriptorKey { get; }

		internal Location SmallestLocation => _smallestLocation;

		internal NameAndIndex? NameAndIndex
		{
			get
			{
				return _nameAndIndex;
			}
			set
			{
				Interlocked.CompareExchange(ref _nameAndIndex, value, null);
			}
		}

		internal sealed override bool HasCodeAnalysisEmbeddedAttribute => false;

		internal sealed override bool HasCompilerLoweringPreserveAttribute => false;

		internal sealed override bool IsInterpolatedStringHandlerType => false;

		internal sealed override ParameterSymbol? ExtensionParameter => null;

		public sealed override Symbol ContainingSymbol => Manager.Compilation.SourceModule.GlobalNamespace;

		public sealed override string Name => _nameAndIndex.Name;

		internal sealed override bool HasSpecialName => false;

		public sealed override bool IsImplicitlyDeclared => true;

		public sealed override bool IsAbstract => false;

		public sealed override bool IsRefLikeType => false;

		internal sealed override string? ExtensionGroupingName => null;

		internal sealed override string? ExtensionMarkerName => null;

		public sealed override bool IsReadOnly => false;

		public sealed override bool IsSealed => true;

		public sealed override bool MightContainExtensionMethods => false;

		public sealed override bool AreLocalsZeroed => ContainingModule.AreLocalsZeroed;

		public sealed override Accessibility DeclaredAccessibility => Accessibility.Internal;

		internal sealed override bool IsInterface => false;

		public sealed override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

		public sealed override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

		public sealed override bool IsStatic => false;

		public sealed override NamedTypeSymbol ConstructedFrom => this;

		internal abstract override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics { get; }

		internal sealed override bool MangleName => Arity > 0;

		internal sealed override bool IsFileLocal => false;

		internal sealed override FileIdentifier? AssociatedFileIdentifier => null;

		internal sealed override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotationsNoUseSiteDiagnostics => GetTypeParametersAsTypeArguments();

		public sealed override int Arity => TypeParameters.Length;

		internal sealed override bool ShouldAddWinRTMembers => false;

		internal sealed override bool IsWindowsRuntimeImport => false;

		internal sealed override bool IsComImport => false;

		internal sealed override ObsoleteAttributeData? ObsoleteAttributeData => null;

		internal sealed override TypeLayout Layout => default(TypeLayout);

		internal sealed override CharSet MarshallingCharSet => base.DefaultMarshallingCharSet;

		public sealed override bool IsSerializable => false;

		internal sealed override bool HasDeclarativeSecurity => false;

		internal sealed override NamedTypeSymbol? NativeIntegerUnderlyingType => null;

		internal sealed override bool IsRecord => false;

		internal sealed override bool IsRecordStruct => false;

		internal AnonymousTypeOrDelegateTemplateSymbol(AnonymousTypeManager manager, Location location)
		{
			Manager = manager;
			_smallestLocation = location;
			_nameAndIndex = null;
		}

		protected sealed override NamedTypeSymbol WithTupleDataCore(TupleExtraData newData)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/AnonymousTypes/SynthesizedSymbols/AnonymousType.TypeOrDelegateTemplateSymbol.cs", 56);
		}

		internal void AdjustLocation(Location location)
		{
			Location smallestLocation;
			do
			{
				smallestLocation = _smallestLocation;
			}
			while ((!(smallestLocation != null) || Manager.Compilation.CompareSourceLocations(smallestLocation, location) >= 0) && (object)Interlocked.CompareExchange(ref _smallestLocation, location, smallestLocation) != smallestLocation);
		}

		internal override bool GetGuidString(out string? guidString)
		{
			guidString = null;
			return false;
		}

		internal sealed override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers()
		{
			return GetMembersUnordered();
		}

		internal sealed override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers(string name)
		{
			return GetMembers(name);
		}

		public sealed override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}

		public sealed override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name)
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}

		public sealed override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name, int arity)
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}

		internal sealed override NamedTypeSymbol GetDeclaredBaseType(ConsList<TypeSymbol> basesBeingResolved)
		{
			return Manager.System_Object;
		}

		internal sealed override ImmutableArray<NamedTypeSymbol> GetDeclaredInterfaces(ConsList<TypeSymbol> basesBeingResolved)
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}

		internal sealed override IEnumerable<SecurityAttribute> GetSecurityInformation()
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/AnonymousTypes/SynthesizedSymbols/AnonymousType.TypeOrDelegateTemplateSymbol.cs", 311);
		}

		internal sealed override ImmutableArray<string> GetAppliedConditionalSymbols()
		{
			return ImmutableArray<string>.Empty;
		}

		internal sealed override AttributeUsageInfo GetAttributeUsageInfo()
		{
			return AttributeUsageInfo.Null;
		}

		internal sealed override NamedTypeSymbol AsNativeInteger()
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/AnonymousTypes/SynthesizedSymbols/AnonymousType.TypeOrDelegateTemplateSymbol.cs", 324);
		}

		internal sealed override bool HasPossibleWellKnownCloneMethod()
		{
			return false;
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

	internal sealed class AnonymousTypeParameterSymbol : TypeParameterSymbol
	{
		private readonly AnonymousTypeOrDelegateTemplateSymbol _container;

		private readonly int _ordinal;

		private readonly string _name;

		private readonly bool _allowsRefLikeType;

		public override TypeParameterKind TypeParameterKind => TypeParameterKind.Type;

		public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

		public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

		public override int Ordinal => _ordinal;

		public override string Name => _name;

		public override bool HasConstructorConstraint => false;

		public override bool HasReferenceTypeConstraint => false;

		public override bool IsReferenceTypeFromConstraintTypes => false;

		internal override bool? ReferenceTypeConstraintIsNullable => false;

		public override bool HasNotNullConstraint => false;

		internal override bool? IsNotNullable => null;

		public override bool HasValueTypeConstraint => false;

		public override bool AllowsRefLikeType => _allowsRefLikeType;

		public override bool IsValueTypeFromConstraintTypes => false;

		public override bool HasUnmanagedTypeConstraint => false;

		public override bool IsImplicitlyDeclared => true;

		public override VarianceKind Variance => VarianceKind.None;

		public override Symbol ContainingSymbol => _container;

		public AnonymousTypeParameterSymbol(AnonymousTypeOrDelegateTemplateSymbol container, int ordinal, string name, bool allowsRefLikeType)
		{
			_container = container;
			_ordinal = ordinal;
			_name = name;
			_allowsRefLikeType = allowsRefLikeType;
		}

		internal override void EnsureAllConstraintsAreResolved()
		{
		}

		internal override ImmutableArray<TypeWithAnnotations> GetConstraintTypes(ConsList<TypeParameterSymbol> inProgress)
		{
			return ImmutableArray<TypeWithAnnotations>.Empty;
		}

		internal override ImmutableArray<NamedTypeSymbol> GetInterfaces(ConsList<TypeParameterSymbol> inProgress)
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}

		internal override NamedTypeSymbol GetEffectiveBaseClass(ConsList<TypeParameterSymbol> inProgress)
		{
			return null;
		}

		internal override TypeSymbol GetDeducedBaseType(ConsList<TypeParameterSymbol> inProgress)
		{
			return null;
		}
	}

	private ConcurrentDictionary<string, AnonymousTypeTemplateSymbol>? _lazyAnonymousTypeTemplates;

	private ConcurrentDictionary<SynthesizedDelegateKey, AnonymousDelegateTemplateSymbol>? _lazyAnonymousDelegates;

	public CSharpCompilation Compilation { get; }

	public NamedTypeSymbol System_Object => Compilation.GetSpecialType(SpecialType.System_Object);

	public NamedTypeSymbol System_Void => Compilation.GetSpecialType(SpecialType.System_Void);

	public NamedTypeSymbol System_Boolean => Compilation.GetSpecialType(SpecialType.System_Boolean);

	public NamedTypeSymbol System_String => Compilation.GetSpecialType(SpecialType.System_String);

	public NamedTypeSymbol System_Int32 => Compilation.GetSpecialType(SpecialType.System_Int32);

	public NamedTypeSymbol System_IntPtr => Compilation.GetSpecialType(SpecialType.System_IntPtr);

	public NamedTypeSymbol System_MulticastDelegate => Compilation.GetSpecialType(SpecialType.System_MulticastDelegate);

	public NamedTypeSymbol System_Diagnostics_DebuggerBrowsableState => Compilation.GetWellKnownType(WellKnownType.System_Diagnostics_DebuggerBrowsableState);

	public MethodSymbol System_Object__Equals => Compilation.GetSpecialTypeMember(SpecialMember.System_Object__Equals) as MethodSymbol;

	public MethodSymbol System_Object__ToString => Compilation.GetSpecialTypeMember(SpecialMember.System_Object__ToString) as MethodSymbol;

	public MethodSymbol System_Object__GetHashCode => Compilation.GetSpecialTypeMember(SpecialMember.System_Object__GetHashCode) as MethodSymbol;

	public MethodSymbol System_Collections_Generic_EqualityComparer_T__Equals => Compilation.GetWellKnownTypeMember(WellKnownMember.System_Collections_Generic_EqualityComparer_T__Equals) as MethodSymbol;

	public MethodSymbol System_Collections_Generic_EqualityComparer_T__GetHashCode => Compilation.GetWellKnownTypeMember(WellKnownMember.System_Collections_Generic_EqualityComparer_T__GetHashCode) as MethodSymbol;

	public MethodSymbol System_Collections_Generic_EqualityComparer_T__get_Default => Compilation.GetWellKnownTypeMember(WellKnownMember.System_Collections_Generic_EqualityComparer_T__get_Default) as MethodSymbol;

	public MethodSymbol System_String__Format_IFormatProvider => Compilation.GetSpecialTypeMember(SpecialMember.System_String__Format_IFormatProvider) as MethodSymbol;

	private ConcurrentDictionary<string, AnonymousTypeTemplateSymbol> AnonymousTypeTemplates
	{
		get
		{
			if (_lazyAnonymousTypeTemplates == null)
			{
				ConcurrentDictionary<string, AnonymousTypeTemplateSymbol> concurrentDictionary = Compilation.PreviousSubmission?.AnonymousTypeManager.AnonymousTypeTemplates;
				Interlocked.CompareExchange(ref _lazyAnonymousTypeTemplates, (concurrentDictionary == null) ? new ConcurrentDictionary<string, AnonymousTypeTemplateSymbol>() : new ConcurrentDictionary<string, AnonymousTypeTemplateSymbol>(concurrentDictionary), null);
			}
			return _lazyAnonymousTypeTemplates;
		}
	}

	private ConcurrentDictionary<SynthesizedDelegateKey, AnonymousDelegateTemplateSymbol> AnonymousDelegates
	{
		get
		{
			if (_lazyAnonymousDelegates == null)
			{
				ConcurrentDictionary<SynthesizedDelegateKey, AnonymousDelegateTemplateSymbol> concurrentDictionary = Compilation.PreviousSubmission?.AnonymousTypeManager._lazyAnonymousDelegates;
				Interlocked.CompareExchange(ref _lazyAnonymousDelegates, (concurrentDictionary == null) ? new ConcurrentDictionary<SynthesizedDelegateKey, AnonymousDelegateTemplateSymbol>() : new ConcurrentDictionary<SynthesizedDelegateKey, AnonymousDelegateTemplateSymbol>(concurrentDictionary), null);
			}
			return _lazyAnonymousDelegates;
		}
	}

	internal AnonymousTypeManager(CSharpCompilation compilation)
	{
		Compilation = compilation;
	}

	public NamedTypeSymbol ConstructAnonymousTypeSymbol(AnonymousTypeDescriptor typeDescr, BindingDiagnosticBag diagnostics)
	{
		if (diagnostics.AccumulatesDependencies)
		{
			BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: false, withDependencies: true);
			ReportMissingOrErroneousSymbols(instance);
			diagnostics.AddRange(instance);
			instance.Free();
		}
		return new AnonymousTypePublicSymbol(this, typeDescr);
	}

	public NamedTypeSymbol ConstructAnonymousDelegateSymbol(AnonymousTypeDescriptor typeDescr)
	{
		return new AnonymousDelegatePublicSymbol(this, typeDescr);
	}

	internal static PropertySymbol GetAnonymousTypeProperty(NamedTypeSymbol type, int index)
	{
		return ((AnonymousTypePublicSymbol)type).Properties[index];
	}

	internal static ImmutableArray<TypeWithAnnotations> GetAnonymousTypeFieldTypes(NamedTypeSymbol type)
	{
		return ((AnonymousTypeOrDelegatePublicSymbol)type).TypeDescriptor.Fields.SelectAsArray((AnonymousTypeField f) => f.TypeWithAnnotations);
	}

	public static NamedTypeSymbol ConstructAnonymousTypeSymbol(NamedTypeSymbol type, ImmutableArray<TypeWithAnnotations> newFieldTypes)
	{
		AnonymousTypePublicSymbol anonymousTypePublicSymbol = (AnonymousTypePublicSymbol)type;
		return anonymousTypePublicSymbol.Manager.ConstructAnonymousTypeSymbol(anonymousTypePublicSymbol.TypeDescriptor.WithNewFieldsTypes(newFieldTypes), BindingDiagnosticBag.Discarded);
	}

	public bool ReportMissingOrErroneousSymbols(BindingDiagnosticBag diagnostics)
	{
		bool hasError = false;
		ReportErrorOnSymbol(System_Object, diagnostics, ref hasError);
		ReportErrorOnSymbol(System_Void, diagnostics, ref hasError);
		ReportErrorOnSymbol(System_Boolean, diagnostics, ref hasError);
		ReportErrorOnSymbol(System_String, diagnostics, ref hasError);
		ReportErrorOnSymbol(System_Int32, diagnostics, ref hasError);
		ReportErrorOnSpecialMember(System_Object__Equals, SpecialMember.System_Object__Equals, diagnostics, ref hasError);
		ReportErrorOnSpecialMember(System_Object__ToString, SpecialMember.System_Object__ToString, diagnostics, ref hasError);
		ReportErrorOnSpecialMember(System_Object__GetHashCode, SpecialMember.System_Object__GetHashCode, diagnostics, ref hasError);
		ReportErrorOnSpecialMember(System_String__Format_IFormatProvider, SpecialMember.System_String__Format_IFormatProvider, diagnostics, ref hasError);
		ReportErrorOnWellKnownMember(System_Collections_Generic_EqualityComparer_T__Equals, WellKnownMember.System_Collections_Generic_EqualityComparer_T__Equals, diagnostics, ref hasError);
		ReportErrorOnWellKnownMember(System_Collections_Generic_EqualityComparer_T__GetHashCode, WellKnownMember.System_Collections_Generic_EqualityComparer_T__GetHashCode, diagnostics, ref hasError);
		ReportErrorOnWellKnownMember(System_Collections_Generic_EqualityComparer_T__get_Default, WellKnownMember.System_Collections_Generic_EqualityComparer_T__get_Default, diagnostics, ref hasError);
		return hasError;
	}

	public bool ReportMissingOrErroneousSymbolsForDelegates(BindingDiagnosticBag diagnostics)
	{
		bool hasError = false;
		ReportErrorOnSymbol(System_Object, diagnostics, ref hasError);
		ReportErrorOnSymbol(System_IntPtr, diagnostics, ref hasError);
		ReportErrorOnSymbol(System_MulticastDelegate, diagnostics, ref hasError);
		return hasError;
	}

	private static void ReportErrorOnSymbol(Symbol symbol, BindingDiagnosticBag diagnostics, ref bool hasError)
	{
		if ((object)symbol != null)
		{
			hasError |= diagnostics.ReportUseSite(symbol, NoLocation.Singleton);
		}
	}

	private static void ReportErrorOnSpecialMember(Symbol symbol, SpecialMember member, BindingDiagnosticBag diagnostics, ref bool hasError)
	{
		if ((object)symbol == null)
		{
			MemberDescriptor descriptor = SpecialMembers.GetDescriptor(member);
			diagnostics.Add(ErrorCode.ERR_MissingPredefinedMember, NoLocation.Singleton, descriptor.DeclaringTypeMetadataName, descriptor.Name);
			hasError = true;
		}
		else
		{
			ReportErrorOnSymbol(symbol, diagnostics, ref hasError);
		}
	}

	private static void ReportErrorOnWellKnownMember(Symbol symbol, WellKnownMember member, BindingDiagnosticBag diagnostics, ref bool hasError)
	{
		if ((object)symbol == null)
		{
			MemberDescriptor descriptor = WellKnownMembers.GetDescriptor(member);
			diagnostics.Add(ErrorCode.ERR_MissingPredefinedMember, NoLocation.Singleton, descriptor.DeclaringTypeMetadataName, descriptor.Name);
			hasError = true;
		}
		else
		{
			ReportErrorOnSymbol(symbol, diagnostics, ref hasError);
			ReportErrorOnSymbol(symbol.ContainingType, diagnostics, ref hasError);
		}
	}

	[Conditional("DEBUG")]
	private void CheckSourceLocationSeen(AnonymousTypePublicSymbol anonymous)
	{
	}

	internal AnonymousDelegateTemplateSymbol SynthesizeDelegate(int parameterCount, RefKindVector refKinds, bool returnsVoid, int generation)
	{
		SynthesizedDelegateKey key = new SynthesizedDelegateKey(parameterCount, refKinds, returnsVoid, generation);
		if (AnonymousDelegates.TryGetValue(key, out AnonymousDelegateTemplateSymbol value))
		{
			return value;
		}
		value = new AnonymousDelegateTemplateSymbol(this, key.Name, System_Object, Compilation.GetSpecialType(SpecialType.System_IntPtr), returnsVoid ? Compilation.GetSpecialType(SpecialType.System_Void) : null, parameterCount, refKinds);
		return AnonymousDelegates.GetOrAdd(key, value);
	}

	private NamedTypeSymbol ConstructAnonymousDelegateImplementationSymbol(AnonymousDelegatePublicSymbol anonymous, int generation)
	{
		AnonymousTypeDescriptor typeDescriptor = anonymous.TypeDescriptor;
		bool useUpdatedEscapeRules = Compilation.SourceModule.UseUpdatedEscapeRules;
		bool runtimeSupportsByRefLikeGenerics = Compilation.SourceAssembly.RuntimeSupportsByRefLikeGenerics;
		if (allValidTypeArguments(useUpdatedEscapeRules, runtimeSupportsByRefLikeGenerics, typeDescriptor, out var needsIndexedName))
		{
			ImmutableArray<AnonymousTypeField> fields = typeDescriptor.Fields;
			bool flag = fields[fields.Length - 1].Type.IsVoidType();
			int num = fields.Length - (flag ? 1 : 0);
			ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance(num);
			for (int i = 0; i < num; i++)
			{
				AnonymousTypeField anonymousTypeField = fields[i];
				if (anonymousTypeField.IsParams)
				{
					instance.Add(((ArrayTypeSymbol)anonymousTypeField.Type).ElementTypeWithAnnotations);
				}
				else
				{
					instance.Add(anonymousTypeField.TypeWithAnnotations);
				}
			}
			ImmutableArray<TypeWithAnnotations> typeArguments = instance.ToImmutableAndFree();
			if (needsIndexedName)
			{
				ImmutableArray<TypeWithAnnotations> newFieldTypes = IndexedTypeParameterSymbol.Take(num);
				int length = fields.Length;
				if (length >= 2)
				{
					AnonymousTypeField anonymousTypeField2 = fields[length - 2];
					if (anonymousTypeField2.IsParams)
					{
						int index = fields.Length - 2;
						TypeWithAnnotations elementTypeWithAnnotations = TypeWithAnnotations.Create(newFieldTypes[index].Type);
						TypeWithAnnotations item = TypeWithAnnotations.Create(((ArrayTypeSymbol)anonymousTypeField2.Type).WithElementType(elementTypeWithAnnotations));
						newFieldTypes = newFieldTypes.SetItem(index, item);
					}
				}
				if (flag)
				{
					newFieldTypes = newFieldTypes.Add(fields[fields.Length - 1].TypeWithAnnotations);
				}
				AnonymousTypeDescriptor typeDescr = typeDescriptor.WithNewFieldsTypes(newFieldTypes);
				return ConcurrentDictionaryExtensions.GetOrAdd(key: new SynthesizedDelegateKey(typeDescr), dictionary: AnonymousDelegates, valueFactory: (SynthesizedDelegateKey synthesizedDelegateKey, AnonymousTypeManager @this) => new AnonymousDelegateTemplateSymbol(@this, synthesizedDelegateKey.TypeDescriptor), factoryArgument: this).Construct(typeArguments);
			}
			RefKindVector refKinds = default(RefKindVector);
			if (fields.Any((AnonymousTypeField f) => f.RefKind != RefKind.None))
			{
				refKinds = RefKindVector.Create(num);
				for (int num2 = 0; num2 < num; num2++)
				{
					refKinds[num2] = fields[num2].RefKind;
				}
			}
			AnonymousDelegateTemplateSymbol anonymousDelegateTemplateSymbol = SynthesizeDelegate(fields.Length - 1, refKinds, flag, generation);
			if (typeArguments.Length != 0)
			{
				return anonymousDelegateTemplateSymbol.Construct(typeArguments);
			}
			return anonymousDelegateTemplateSymbol;
		}
		ImmutableArray<TypeParameterSymbol> referencedTypeParameters = GetReferencedTypeParameters(typeDescriptor);
		SynthesizedDelegateKey key = getTemplateKey(typeDescriptor, referencedTypeParameters);
		if (!AnonymousDelegates.TryGetValue(key, out AnonymousDelegateTemplateSymbol value))
		{
			value = AnonymousDelegates.GetOrAdd(key, new AnonymousDelegateTemplateSymbol(this, typeDescriptor, referencedTypeParameters));
		}
		if (value.Manager == this)
		{
			value.AdjustLocation(typeDescriptor.Location);
		}
		if (referencedTypeParameters.Length != 0)
		{
			return value.Construct(referencedTypeParameters);
		}
		return value;
		static bool allValidTypeArguments(bool useUpdatedEscapeRules2, bool runtimeSupportsByRefLikeGenerics2, AnonymousTypeDescriptor anonymousTypeDescriptor, out bool reference)
		{
			reference = false;
			ImmutableArray<AnonymousTypeField> fields2 = anonymousTypeDescriptor.Fields;
			int length2 = fields2.Length;
			for (int j = 0; j < length2 - 1; j++)
			{
				if (!isValidTypeArgument(useUpdatedEscapeRules2, runtimeSupportsByRefLikeGenerics2, fields2[j], ref reference))
				{
					return false;
				}
			}
			AnonymousTypeField field = fields2[length2 - 1];
			if (!field.Type.IsVoidType())
			{
				return isValidTypeArgument(useUpdatedEscapeRules2, runtimeSupportsByRefLikeGenerics2, field, ref reference);
			}
			return true;
		}
		static SynthesizedDelegateKey getTemplateKey(AnonymousTypeDescriptor typeDescr2, ImmutableArray<TypeParameterSymbol> typeParameters)
		{
			if (typeParameters.Length > 0)
			{
				TypeMap map = new TypeMap(typeParameters, IndexedTypeParameterSymbol.Take(typeParameters.Length), allowAlpha: true);
				typeDescr2 = typeDescr2.SubstituteTypes(map, out var _);
			}
			return new SynthesizedDelegateKey(typeDescr2);
		}
		static bool hasDefaultScope(bool useUpdatedEscapeRules2, AnonymousTypeField field)
		{
			if (field.HasUnscopedRefAttribute)
			{
				return false;
			}
			ScopedKind scope = field.Scope;
			bool flag2 = ParameterHelpers.IsRefScopedByDefault(useUpdatedEscapeRules2, field.RefKind);
			switch (scope)
			{
			case ScopedKind.None:
				if (!flag2)
				{
					return true;
				}
				break;
			case ScopedKind.ScopedRef:
				if (flag2)
				{
					return true;
				}
				break;
			}
			return false;
		}
		static bool isValidTypeArgument(bool useUpdatedEscapeRules2, bool ignoreSpanLikeTypes, AnonymousTypeField field, ref bool reference)
		{
			reference = reference || field.IsParams || (object)field.DefaultValue != null;
			if (hasDefaultScope(useUpdatedEscapeRules2, field))
			{
				TypeSymbol type = field.Type;
				if ((object)type != null && !type.IsPointerOrFunctionPointer() && (type.IsTypeParameter() || !type.IsRestrictedType(ignoreSpanLikeTypes)))
				{
					if (field.IsParams)
					{
						return field.Type.IsSZArray();
					}
					return true;
				}
			}
			return false;
		}
	}

	private static ImmutableArray<TypeParameterSymbol> GetReferencedTypeParameters(AnonymousTypeDescriptor typeDescr)
	{
		PooledHashSet<TypeParameterSymbol> instance = PooledHashSet<TypeParameterSymbol>.GetInstance();
		foreach (AnonymousTypeField field in typeDescr.Fields)
		{
			field.TypeWithAnnotations.VisitType(null, null, delegate(TypeSymbol type, PooledHashSet<TypeParameterSymbol> referenced, bool _)
			{
				if (type is TypeParameterSymbol item)
				{
					referenced.Add(item);
				}
				return false;
			}, instance, canDigThroughNullable: false, useDefaultType: false, visitCustomModifiers: true);
		}
		ImmutableArray<TypeParameterSymbol> result;
		if (instance.Count == 0)
		{
			result = ImmutableArray<TypeParameterSymbol>.Empty;
		}
		else
		{
			ArrayBuilder<TypeParameterSymbol> instance2 = ArrayBuilder<TypeParameterSymbol>.GetInstance();
			instance2.AddRange(instance);
			instance2.Sort((TypeParameterSymbol x, TypeParameterSymbol y) => compareTypeParameters(x, y));
			result = instance2.ToImmutableAndFree();
		}
		instance.Free();
		return result;
		static int compareTypeParameters(TypeParameterSymbol x, TypeParameterSymbol y)
		{
			Symbol containingSymbol = x.ContainingSymbol;
			Symbol containingSymbol2 = y.ContainingSymbol;
			if (containingSymbol.Equals(containingSymbol2))
			{
				return x.Ordinal - y.Ordinal;
			}
			if (isContainedIn(containingSymbol, containingSymbol2))
			{
				return 1;
			}
			return -1;
		}
		static bool isContainedIn(Symbol symbol, Symbol container)
		{
			Symbol containingSymbol = symbol.ContainingSymbol;
			while ((object)containingSymbol != null)
			{
				if (containingSymbol.Equals(container))
				{
					return true;
				}
				containingSymbol = containingSymbol.ContainingSymbol;
			}
			return false;
		}
	}

	private NamedTypeSymbol ConstructAnonymousTypeImplementationSymbol(AnonymousTypePublicSymbol anonymous)
	{
		AnonymousTypeDescriptor typeDescriptor = anonymous.TypeDescriptor;
		if (!AnonymousTypeTemplates.TryGetValue(typeDescriptor.Key, out AnonymousTypeTemplateSymbol value))
		{
			value = AnonymousTypeTemplates.GetOrAdd(typeDescriptor.Key, new AnonymousTypeTemplateSymbol(this, typeDescriptor));
		}
		if (value.Manager == this)
		{
			value.AdjustLocation(typeDescriptor.Location);
		}
		if (value.Arity == 0)
		{
			return value;
		}
		ImmutableArray<TypeSymbol> typeArguments = typeDescriptor.Fields.SelectAsArray((AnonymousTypeField f) => f.Type);
		return value.Construct(typeArguments);
	}

	public void AssignTemplatesNamesAndCompile(MethodCompiler compiler, PEModuleBuilder moduleBeingBuilt, BindingDiagnosticBag diagnostics)
	{
		ArrayBuilder<AnonymousTypeTemplateSymbol> instance = ArrayBuilder<AnonymousTypeTemplateSymbol>.GetInstance();
		ArrayBuilder<AnonymousDelegateTemplateSymbol> instance2 = ArrayBuilder<AnonymousDelegateTemplateSymbol>.GetInstance();
		ArrayBuilder<AnonymousDelegateTemplateSymbol> instance3 = ArrayBuilder<AnonymousDelegateTemplateSymbol>.GetInstance();
		GetCreatedAnonymousTypeTemplates(instance);
		GetCreatedAnonymousDelegates(instance2);
		GetCreatedAnonymousDelegatesWithIndexedNames(instance3);
		string moduleId;
		int submissionSlotIndex;
		if (!base.AreTemplatesSealed)
		{
			moduleId = getModuleId();
			submissionSlotIndex = Compilation.GetSubmissionSlotIndex();
			assignIndexedNames(instance, moduleBeingBuilt.GetNextAnonymousTypeIndex(), isDelegate: false);
			assignIndexedNames(instance3, moduleBeingBuilt.GetNextAnonymousDelegateIndex(), isDelegate: true);
			SealTemplates();
		}
		if (instance.Count > 0 && !ReportMissingOrErroneousSymbols(diagnostics))
		{
			foreach (AnonymousTypeTemplateSymbol item in instance)
			{
				foreach (MethodSymbol specialMember in item.SpecialMembers)
				{
					moduleBeingBuilt.AddSynthesizedDefinition(item, specialMember.GetCciAdapter());
				}
				compiler.Visit(item);
			}
		}
		if (instance3.Count > 0 || instance2.Count > 0)
		{
			ReportMissingOrErroneousSymbolsForDelegates(diagnostics);
			foreach (AnonymousDelegateTemplateSymbol item2 in instance3)
			{
				compiler.Visit(item2);
			}
			foreach (AnonymousDelegateTemplateSymbol item3 in instance2)
			{
				compiler.Visit(item3);
			}
		}
		instance.Free();
		instance2.Free();
		instance3.Free();
		void assignIndexedNames(IReadOnlyList<AnonymousTypeOrDelegateTemplateSymbol> templates, int nextIndex, bool isDelegate)
		{
			foreach (AnonymousTypeOrDelegateTemplateSymbol template in templates)
			{
				int index;
				string name;
				if (moduleBeingBuilt.TryGetPreviousAnonymousTypeValue(template, out var typeValue))
				{
					index = typeValue.UniqueIndex;
					name = typeValue.Name;
				}
				else
				{
					index = nextIndex++;
					name = GeneratedNames.MakeAnonymousTypeOrDelegateTemplateName(index, submissionSlotIndex, moduleId, isDelegate);
				}
				template.NameAndIndex = new NameAndIndex(name, index);
			}
		}
		string getModuleId()
		{
			if (moduleBeingBuilt.OutputKind == OutputKind.NetModule)
			{
				string defaultExtension = OutputKind.NetModule.GetDefaultExtension();
				string text = moduleBeingBuilt.Name;
				if (text.EndsWith(defaultExtension, StringComparison.OrdinalIgnoreCase))
				{
					string text2 = text;
					int length = defaultExtension.Length;
					text = text2.Substring(0, text2.Length - length);
				}
				return MetadataHelpers.MangleForTypeNameIfNeeded(text);
			}
			return string.Empty;
		}
	}

	private void GetCreatedAnonymousTypeTemplates(ArrayBuilder<AnonymousTypeTemplateSymbol> builder)
	{
		ConcurrentDictionary<string, AnonymousTypeTemplateSymbol> lazyAnonymousTypeTemplates = _lazyAnonymousTypeTemplates;
		if (lazyAnonymousTypeTemplates == null)
		{
			return;
		}
		foreach (AnonymousTypeTemplateSymbol value in lazyAnonymousTypeTemplates.Values)
		{
			if (value.Manager == this)
			{
				builder.Add(value);
			}
		}
		builder.Sort(new AnonymousTypeOrDelegateComparer(Compilation));
	}

	private void GetCreatedAnonymousDelegatesWithIndexedNames(ArrayBuilder<AnonymousDelegateTemplateSymbol> builder)
	{
		ConcurrentDictionary<SynthesizedDelegateKey, AnonymousDelegateTemplateSymbol> lazyAnonymousDelegates = _lazyAnonymousDelegates;
		if (lazyAnonymousDelegates == null)
		{
			return;
		}
		foreach (AnonymousDelegateTemplateSymbol value in lazyAnonymousDelegates.Values)
		{
			if (value.Manager == this && value.HasIndexedName)
			{
				builder.Add(value);
			}
		}
		builder.Sort(new AnonymousTypeOrDelegateComparer(Compilation));
	}

	private void GetCreatedAnonymousDelegates(ArrayBuilder<AnonymousDelegateTemplateSymbol> builder)
	{
		ConcurrentDictionary<SynthesizedDelegateKey, AnonymousDelegateTemplateSymbol> lazyAnonymousDelegates = _lazyAnonymousDelegates;
		if (lazyAnonymousDelegates == null)
		{
			return;
		}
		foreach (AnonymousDelegateTemplateSymbol value in lazyAnonymousDelegates.Values)
		{
			if (value.Manager == this && !value.HasIndexedName)
			{
				builder.Add(value);
			}
		}
		builder.Sort(SynthesizedDelegateSymbolComparer.Instance);
	}

	internal ImmutableSegmentedDictionary<Microsoft.CodeAnalysis.Emit.SynthesizedDelegateKey, SynthesizedDelegateValue> GetAnonymousDelegates()
	{
		ArrayBuilder<AnonymousDelegateTemplateSymbol> instance = ArrayBuilder<AnonymousDelegateTemplateSymbol>.GetInstance();
		GetCreatedAnonymousDelegates(instance);
		ImmutableSegmentedDictionary<Microsoft.CodeAnalysis.Emit.SynthesizedDelegateKey, SynthesizedDelegateValue> result = instance.ToImmutableSegmentedDictionary((AnonymousDelegateTemplateSymbol delegateSymbol) => new Microsoft.CodeAnalysis.Emit.SynthesizedDelegateKey(delegateSymbol.MetadataName), (AnonymousDelegateTemplateSymbol delegateSymbol) => new SynthesizedDelegateValue(delegateSymbol.GetCciAdapter()));
		instance.Free();
		return result;
	}

	internal ImmutableSegmentedDictionary<AnonymousTypeKey, AnonymousTypeValue> GetAnonymousTypeMap()
	{
		ArrayBuilder<AnonymousTypeTemplateSymbol> instance = ArrayBuilder<AnonymousTypeTemplateSymbol>.GetInstance();
		GetCreatedAnonymousTypeTemplates(instance);
		ImmutableSegmentedDictionary<AnonymousTypeKey, AnonymousTypeValue> result = instance.ToImmutableSegmentedDictionary((AnonymousTypeTemplateSymbol template) => template.GetAnonymousTypeKey(), (AnonymousTypeTemplateSymbol template) => new AnonymousTypeValue(template.NameAndIndex.Name, template.NameAndIndex.Index, template.GetCciAdapter()));
		instance.Free();
		return result;
	}

	internal ImmutableSegmentedDictionary<AnonymousDelegateWithIndexedNamePartialKey, ImmutableArray<AnonymousTypeValue>> GetAnonymousDelegatesWithIndexedNames()
	{
		ArrayBuilder<AnonymousDelegateTemplateSymbol> instance = ArrayBuilder<AnonymousDelegateTemplateSymbol>.GetInstance();
		GetCreatedAnonymousDelegatesWithIndexedNames(instance);
		ImmutableSegmentedDictionary<AnonymousDelegateWithIndexedNamePartialKey, ImmutableArray<AnonymousTypeValue>> result = (from template in instance
			group new AnonymousTypeValue(template.NameAndIndex.Name, template.NameAndIndex.Index, template.GetCciAdapter()) by new AnonymousDelegateWithIndexedNamePartialKey(template.Arity, template.DelegateInvokeMethod.ParameterCount)).ToImmutableSegmentedDictionary((IGrouping<AnonymousDelegateWithIndexedNamePartialKey, AnonymousTypeValue> grouping) => grouping.Key, (IGrouping<AnonymousDelegateWithIndexedNamePartialKey, AnonymousTypeValue> grouping) => grouping.ToImmutableArray());
		instance.Free();
		return result;
	}

	internal ImmutableArray<NamedTypeSymbol> GetAllCreatedTemplates()
	{
		ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		ArrayBuilder<AnonymousTypeTemplateSymbol> instance2 = ArrayBuilder<AnonymousTypeTemplateSymbol>.GetInstance();
		GetCreatedAnonymousTypeTemplates(instance2);
		instance.AddRange(instance2);
		instance2.Free();
		ArrayBuilder<AnonymousDelegateTemplateSymbol> instance3 = ArrayBuilder<AnonymousDelegateTemplateSymbol>.GetInstance();
		GetCreatedAnonymousDelegatesWithIndexedNames(instance3);
		instance.AddRange(instance3);
		instance3.Free();
		ArrayBuilder<AnonymousDelegateTemplateSymbol> instance4 = ArrayBuilder<AnonymousDelegateTemplateSymbol>.GetInstance();
		GetCreatedAnonymousDelegates(instance4);
		instance.AddRange(instance4);
		instance4.Free();
		return instance.ToImmutableAndFree();
	}

	internal override SynthesizedTypeMaps GetSynthesizedTypeMaps()
	{
		return new SynthesizedTypeMaps(GetAnonymousTypeMap(), GetAnonymousDelegates(), GetAnonymousDelegatesWithIndexedNames());
	}

	internal static bool IsAnonymousTypeTemplate(NamedTypeSymbol type)
	{
		return type is AnonymousTypeTemplateSymbol;
	}

	internal static ImmutableArray<MethodSymbol> GetAnonymousTypeHiddenMethods(NamedTypeSymbol type)
	{
		return ((AnonymousTypeTemplateSymbol)type).SpecialMembers;
	}

	internal static NamedTypeSymbol TranslateAnonymousTypeSymbol(NamedTypeSymbol type)
	{
		return ((AnonymousTypeOrDelegatePublicSymbol)type).MapToImplementationSymbol();
	}

	internal static MethodSymbol TranslateAnonymousTypeMethodSymbol(MethodSymbol method)
	{
		NamedTypeSymbol namedTypeSymbol = TranslateAnonymousTypeSymbol(method.ContainingType);
		foreach (Symbol member in namedTypeSymbol.OriginalDefinition.GetMembers(method.Name))
		{
			if (member.Kind == SymbolKind.Method)
			{
				return ((MethodSymbol)member).AsMember(namedTypeSymbol);
			}
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/AnonymousTypes/AnonymousTypeManager.Templates.cs", 755);
	}
}
