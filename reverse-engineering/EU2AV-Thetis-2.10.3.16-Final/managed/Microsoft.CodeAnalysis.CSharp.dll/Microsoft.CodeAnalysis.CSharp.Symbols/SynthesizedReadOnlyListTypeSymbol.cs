using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.RuntimeMembers;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedReadOnlyListTypeSymbol : NamedTypeSymbol
{
	private static readonly SpecialType[] s_requiredSpecialTypes = new SpecialType[4]
	{
		SpecialType.System_Collections_IEnumerable,
		SpecialType.System_Collections_Generic_IEnumerable_T,
		SpecialType.System_Collections_Generic_ICollection_T,
		SpecialType.System_Collections_Generic_IList_T
	};

	private static readonly SpecialType[] s_readOnlyInterfacesSpecialTypes = new SpecialType[2]
	{
		SpecialType.System_Collections_Generic_IReadOnlyCollection_T,
		SpecialType.System_Collections_Generic_IReadOnlyList_T
	};

	private static readonly WellKnownType[] s_requiredWellKnownTypes = new WellKnownType[2]
	{
		WellKnownType.System_Collections_ICollection,
		WellKnownType.System_Collections_IList
	};

	private static readonly SpecialMember[] s_requiredSpecialMembers = new SpecialMember[13]
	{
		SpecialMember.System_Collections_IEnumerable__GetEnumerator,
		SpecialMember.System_Collections_Generic_IEnumerable_T__GetEnumerator,
		SpecialMember.System_Collections_Generic_ICollection_T__Count,
		SpecialMember.System_Collections_Generic_ICollection_T__IsReadOnly,
		SpecialMember.System_Collections_Generic_ICollection_T__Add,
		SpecialMember.System_Collections_Generic_ICollection_T__Clear,
		SpecialMember.System_Collections_Generic_ICollection_T__Contains,
		SpecialMember.System_Collections_Generic_ICollection_T__CopyTo,
		SpecialMember.System_Collections_Generic_ICollection_T__Remove,
		SpecialMember.System_Collections_Generic_IList_T__get_Item,
		SpecialMember.System_Collections_Generic_IList_T__IndexOf,
		SpecialMember.System_Collections_Generic_IList_T__Insert,
		SpecialMember.System_Collections_Generic_IList_T__RemoveAt
	};

	private static readonly WellKnownMember[] s_requiredWellKnownMembers = new WellKnownMember[15]
	{
		WellKnownMember.System_Collections_ICollection__Count,
		WellKnownMember.System_Collections_ICollection__IsSynchronized,
		WellKnownMember.System_Collections_ICollection__SyncRoot,
		WellKnownMember.System_Collections_ICollection__CopyTo,
		WellKnownMember.System_Collections_IList__get_Item,
		WellKnownMember.System_Collections_IList__IsFixedSize,
		WellKnownMember.System_Collections_IList__IsReadOnly,
		WellKnownMember.System_Collections_IList__Add,
		WellKnownMember.System_Collections_IList__Clear,
		WellKnownMember.System_Collections_IList__Contains,
		WellKnownMember.System_Collections_IList__IndexOf,
		WellKnownMember.System_Collections_IList__Insert,
		WellKnownMember.System_Collections_IList__Remove,
		WellKnownMember.System_Collections_IList__RemoveAt,
		WellKnownMember.System_NotSupportedException__ctor
	};

	private static readonly SpecialMember[] s_readOnlyInterfacesWellKnownMembers = new SpecialMember[2]
	{
		SpecialMember.System_Collections_Generic_IReadOnlyCollection_T__Count,
		SpecialMember.System_Collections_Generic_IReadOnlyList_T__get_Item
	};

	private static readonly WellKnownMember[] s_requiredWellKnownMembersUnknownLength = new WellKnownMember[5]
	{
		WellKnownMember.System_Collections_Generic_List_T__Count,
		WellKnownMember.System_Collections_Generic_List_T__Contains,
		WellKnownMember.System_Collections_Generic_List_T__CopyTo,
		WellKnownMember.System_Collections_Generic_List_T__get_Item,
		WellKnownMember.System_Collections_Generic_List_T__IndexOf
	};

	private readonly ModuleSymbol _containingModule;

	private readonly ImmutableArray<NamedTypeSymbol> _interfaces;

	private readonly ImmutableArray<Symbol> _members;

	private readonly FieldSymbol _field;

	private readonly NamedTypeSymbol? _enumeratorType;

	private bool IsSingleElement => _field.Type.IsTypeParameter();

	private bool IsArray => _field.Type.IsArray();

	public override int Arity => 1;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters { get; }

	public override NamedTypeSymbol ConstructedFrom => this;

	public override bool MightContainExtensionMethods => false;

	public override string Name { get; }

	public override IEnumerable<string> MemberNames => from m in GetMembers()
		select m.Name;

	public override Accessibility DeclaredAccessibility => Accessibility.Internal;

	public override bool IsSerializable => false;

	public override bool AreLocalsZeroed => true;

	public override TypeKind TypeKind => TypeKind.Class;

	public override bool IsRefLikeType => false;

	internal override string? ExtensionGroupingName => null;

	internal override string? ExtensionMarkerName => null;

	public override bool IsReadOnly => false;

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

	internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => ContainingAssembly.GetSpecialType(SpecialType.System_Object);

	internal override bool IsRecord => false;

	internal override bool IsRecordStruct => false;

	internal override ObsoleteAttributeData? ObsoleteAttributeData => null;

	internal static NamedTypeSymbol Create(SourceModuleSymbol containingModule, string name, SynthesizedReadOnlyListKind kind)
	{
		CSharpCompilation declaringCompilation = containingModule.DeclaringCompilation;
		DiagnosticInfo diagnosticInfo = null;
		bool flag = !(declaringCompilation.GetSpecialType(SpecialType.System_Collections_Generic_IReadOnlyCollection_T) is MissingMetadataTypeSymbol) && !(declaringCompilation.GetSpecialType(SpecialType.System_Collections_Generic_IReadOnlyList_T) is MissingMetadataTypeSymbol);
		SpecialType[] array = s_requiredSpecialTypes;
		foreach (SpecialType specialType in array)
		{
			diagnosticInfo = declaringCompilation.GetSpecialType(specialType).GetUseSiteInfo().DiagnosticInfo;
			if (diagnosticInfo != null)
			{
				break;
			}
		}
		if (flag && diagnosticInfo == null)
		{
			array = s_readOnlyInterfacesSpecialTypes;
			foreach (SpecialType specialType2 in array)
			{
				diagnosticInfo = declaringCompilation.GetSpecialType(specialType2).GetUseSiteInfo().DiagnosticInfo;
				if (diagnosticInfo != null)
				{
					break;
				}
			}
		}
		if (diagnosticInfo == null)
		{
			WellKnownType[] array2 = s_requiredWellKnownTypes;
			foreach (WellKnownType type in array2)
			{
				diagnosticInfo = declaringCompilation.GetWellKnownType(type).GetUseSiteInfo().DiagnosticInfo;
				if (diagnosticInfo != null)
				{
					break;
				}
			}
		}
		if (diagnosticInfo == null)
		{
			SpecialMember[] array3 = s_requiredSpecialMembers;
			foreach (SpecialMember member in array3)
			{
				diagnosticInfo = getSpecialTypeMemberDiagnosticInfo(declaringCompilation, member);
				if (diagnosticInfo != null)
				{
					break;
				}
			}
		}
		if (diagnosticInfo == null)
		{
			WellKnownMember[] array4 = s_requiredWellKnownMembers;
			foreach (WellKnownMember member2 in array4)
			{
				diagnosticInfo = getWellKnownTypeMemberDiagnosticInfo(declaringCompilation, member2);
				if (diagnosticInfo != null)
				{
					break;
				}
			}
		}
		if (flag && diagnosticInfo == null)
		{
			SpecialMember[] array3 = s_readOnlyInterfacesWellKnownMembers;
			foreach (SpecialMember member3 in array3)
			{
				diagnosticInfo = getSpecialTypeMemberDiagnosticInfo(declaringCompilation, member3);
				if (diagnosticInfo != null)
				{
					break;
				}
			}
		}
		if (kind == SynthesizedReadOnlyListKind.List)
		{
			if (diagnosticInfo == null)
			{
				diagnosticInfo = declaringCompilation.GetWellKnownType(WellKnownType.System_Collections_Generic_List_T).GetUseSiteInfo().DiagnosticInfo;
			}
			if (diagnosticInfo == null)
			{
				WellKnownMember[] array4 = s_requiredWellKnownMembersUnknownLength;
				foreach (WellKnownMember member4 in array4)
				{
					diagnosticInfo = getWellKnownTypeMemberDiagnosticInfo(declaringCompilation, member4);
					if (diagnosticInfo != null)
					{
						break;
					}
				}
			}
		}
		if (diagnosticInfo != null)
		{
			return new ExtendedErrorTypeSymbol(declaringCompilation, name, 1, diagnosticInfo, unreported: true);
		}
		return new SynthesizedReadOnlyListTypeSymbol(containingModule, name, kind, flag);
		static DiagnosticInfo? getSpecialTypeMemberDiagnosticInfo(CSharpCompilation compilation, SpecialMember specialMember)
		{
			if ((object)compilation.GetSpecialTypeMember(specialMember) != null)
			{
				return null;
			}
			MemberDescriptor descriptor = SpecialMembers.GetDescriptor(specialMember);
			return new CSDiagnosticInfo(ErrorCode.ERR_MissingPredefinedMember, descriptor.DeclaringTypeMetadataName, descriptor.Name);
		}
		static DiagnosticInfo? getWellKnownTypeMemberDiagnosticInfo(CSharpCompilation compilation, WellKnownMember member5)
		{
			if ((object)Binder.GetWellKnownTypeMember(compilation, member5, out var useSiteInfo) != null)
			{
				return null;
			}
			return useSiteInfo.DiagnosticInfo;
		}
	}

	private SynthesizedReadOnlyListTypeSymbol(SourceModuleSymbol containingModule, string name, SynthesizedReadOnlyListKind kind, bool hasReadOnlyInterfaces)
	{
		CSharpCompilation declaringCompilation = containingModule.DeclaringCompilation;
		_containingModule = containingModule;
		Name = name;
		SynthesizedReadOnlyListTypeParameterSymbol synthesizedReadOnlyListTypeParameterSymbol = new SynthesizedReadOnlyListTypeParameterSymbol(this);
		TypeParameters = ImmutableArray.Create((TypeParameterSymbol)synthesizedReadOnlyListTypeParameterSymbol);
		ImmutableArray<TypeWithAnnotations> typeArgumentsWithAnnotationsNoUseSiteDiagnostics = TypeArgumentsWithAnnotationsNoUseSiteDiagnostics;
		TypeSymbol typeSymbol = kind switch
		{
			SynthesizedReadOnlyListKind.SingleElement => synthesizedReadOnlyListTypeParameterSymbol, 
			SynthesizedReadOnlyListKind.Array => declaringCompilation.CreateArrayTypeSymbol(synthesizedReadOnlyListTypeParameterSymbol), 
			SynthesizedReadOnlyListKind.List => declaringCompilation.GetWellKnownType(WellKnownType.System_Collections_Generic_List_T).Construct(typeArgumentsWithAnnotationsNoUseSiteDiagnostics), 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
		_enumeratorType = ((kind == SynthesizedReadOnlyListKind.SingleElement) ? new SynthesizedReadOnlyListEnumeratorTypeSymbol(this, synthesizedReadOnlyListTypeParameterSymbol) : null);
		_field = new SynthesizedFieldSymbol(this, typeSymbol, (kind == SynthesizedReadOnlyListKind.SingleElement) ? "_item" : "_items", DeclarationModifiers.Private, isReadOnly: true);
		NamedTypeSymbol specialType = declaringCompilation.GetSpecialType(SpecialType.System_Collections_IEnumerable);
		NamedTypeSymbol wellKnownType = declaringCompilation.GetWellKnownType(WellKnownType.System_Collections_ICollection);
		NamedTypeSymbol wellKnownType2 = declaringCompilation.GetWellKnownType(WellKnownType.System_Collections_IList);
		NamedTypeSymbol namedTypeSymbol = declaringCompilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T).Construct(typeArgumentsWithAnnotationsNoUseSiteDiagnostics);
		NamedTypeSymbol namedTypeSymbol2 = declaringCompilation.GetSpecialType(SpecialType.System_Collections_Generic_IReadOnlyCollection_T).Construct(typeArgumentsWithAnnotationsNoUseSiteDiagnostics);
		NamedTypeSymbol namedTypeSymbol3 = declaringCompilation.GetSpecialType(SpecialType.System_Collections_Generic_IReadOnlyList_T).Construct(typeArgumentsWithAnnotationsNoUseSiteDiagnostics);
		NamedTypeSymbol namedTypeSymbol4 = declaringCompilation.GetSpecialType(SpecialType.System_Collections_Generic_ICollection_T).Construct(typeArgumentsWithAnnotationsNoUseSiteDiagnostics);
		NamedTypeSymbol namedTypeSymbol5 = declaringCompilation.GetSpecialType(SpecialType.System_Collections_Generic_IList_T).Construct(typeArgumentsWithAnnotationsNoUseSiteDiagnostics);
		_interfaces = (hasReadOnlyInterfaces ? ImmutableArray.Create(new ReadOnlySpan<NamedTypeSymbol>(new NamedTypeSymbol[8] { specialType, wellKnownType, wellKnownType2, namedTypeSymbol, namedTypeSymbol2, namedTypeSymbol3, namedTypeSymbol4, namedTypeSymbol5 })) : ImmutableArray.Create(new ReadOnlySpan<NamedTypeSymbol>(new NamedTypeSymbol[6] { specialType, wellKnownType, wellKnownType2, namedTypeSymbol, namedTypeSymbol4, namedTypeSymbol5 })));
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		instance.Add(_field);
		instance.AddIfNotNull(_enumeratorType);
		instance.Add(new SynthesizedReadOnlyListConstructor(this, typeSymbol, (kind == SynthesizedReadOnlyListKind.SingleElement) ? "item" : "items"));
		instance.Add(new SynthesizedReadOnlyListMethod(this, (MethodSymbol)declaringCompilation.GetSpecialTypeMember(SpecialMember.System_Collections_IEnumerable__GetEnumerator), generateGetEnumerator));
		addProperty(instance, new SynthesizedReadOnlyListProperty(this, (PropertySymbol)declaringCompilation.GetWellKnownTypeMember(WellKnownMember.System_Collections_ICollection__Count), generateCount));
		addProperty(instance, new SynthesizedReadOnlyListProperty(this, (PropertySymbol)declaringCompilation.GetWellKnownTypeMember(WellKnownMember.System_Collections_ICollection__IsSynchronized), generateIsSynchronized));
		addProperty(instance, new SynthesizedReadOnlyListProperty(this, (PropertySymbol)declaringCompilation.GetWellKnownTypeMember(WellKnownMember.System_Collections_ICollection__SyncRoot), generateSyncRoot));
		instance.Add(new SynthesizedReadOnlyListMethod(this, (MethodSymbol)declaringCompilation.GetWellKnownTypeMember(WellKnownMember.System_Collections_ICollection__CopyTo), generateCopyTo));
		addProperty(instance, new SynthesizedReadOnlyListProperty(this, (PropertySymbol)((MethodSymbol)declaringCompilation.GetWellKnownTypeMember(WellKnownMember.System_Collections_IList__get_Item)).AssociatedSymbol, generateIndexer, generateNotSupportedException));
		addProperty(instance, new SynthesizedReadOnlyListProperty(this, (PropertySymbol)declaringCompilation.GetWellKnownTypeMember(WellKnownMember.System_Collections_IList__IsFixedSize), generateIsFixedSize));
		addProperty(instance, new SynthesizedReadOnlyListProperty(this, (PropertySymbol)declaringCompilation.GetWellKnownTypeMember(WellKnownMember.System_Collections_IList__IsReadOnly), generateIsReadOnly));
		instance.Add(new SynthesizedReadOnlyListMethod(this, (MethodSymbol)declaringCompilation.GetWellKnownTypeMember(WellKnownMember.System_Collections_IList__Add), generateNotSupportedException));
		instance.Add(new SynthesizedReadOnlyListMethod(this, (MethodSymbol)declaringCompilation.GetWellKnownTypeMember(WellKnownMember.System_Collections_IList__Clear), generateNotSupportedException));
		instance.Add(new SynthesizedReadOnlyListMethod(this, (MethodSymbol)declaringCompilation.GetWellKnownTypeMember(WellKnownMember.System_Collections_IList__Contains), generateContains));
		instance.Add(new SynthesizedReadOnlyListMethod(this, (MethodSymbol)declaringCompilation.GetWellKnownTypeMember(WellKnownMember.System_Collections_IList__IndexOf), generateIndexOf));
		instance.Add(new SynthesizedReadOnlyListMethod(this, (MethodSymbol)declaringCompilation.GetWellKnownTypeMember(WellKnownMember.System_Collections_IList__Insert), generateNotSupportedException));
		instance.Add(new SynthesizedReadOnlyListMethod(this, (MethodSymbol)declaringCompilation.GetWellKnownTypeMember(WellKnownMember.System_Collections_IList__Remove), generateNotSupportedException));
		instance.Add(new SynthesizedReadOnlyListMethod(this, (MethodSymbol)declaringCompilation.GetWellKnownTypeMember(WellKnownMember.System_Collections_IList__RemoveAt), generateNotSupportedException));
		instance.Add(new SynthesizedReadOnlyListMethod(this, ((MethodSymbol)declaringCompilation.GetSpecialTypeMember(SpecialMember.System_Collections_Generic_IEnumerable_T__GetEnumerator)).AsMember(namedTypeSymbol), generateGetEnumerator));
		if (hasReadOnlyInterfaces)
		{
			addProperty(instance, new SynthesizedReadOnlyListProperty(this, ((PropertySymbol)declaringCompilation.GetSpecialTypeMember(SpecialMember.System_Collections_Generic_IReadOnlyCollection_T__Count)).AsMember(namedTypeSymbol2), generateCount));
			addProperty(instance, new SynthesizedReadOnlyListProperty(this, ((PropertySymbol)((MethodSymbol)declaringCompilation.GetSpecialTypeMember(SpecialMember.System_Collections_Generic_IReadOnlyList_T__get_Item)).AssociatedSymbol).AsMember(namedTypeSymbol3), generateIndexer));
		}
		addProperty(instance, new SynthesizedReadOnlyListProperty(this, ((PropertySymbol)declaringCompilation.GetSpecialTypeMember(SpecialMember.System_Collections_Generic_ICollection_T__Count)).AsMember(namedTypeSymbol4), generateCount));
		addProperty(instance, new SynthesizedReadOnlyListProperty(this, ((PropertySymbol)declaringCompilation.GetSpecialTypeMember(SpecialMember.System_Collections_Generic_ICollection_T__IsReadOnly)).AsMember(namedTypeSymbol4), generateIsReadOnly));
		instance.Add(new SynthesizedReadOnlyListMethod(this, ((MethodSymbol)declaringCompilation.GetSpecialTypeMember(SpecialMember.System_Collections_Generic_ICollection_T__Add)).AsMember(namedTypeSymbol4), generateNotSupportedException));
		instance.Add(new SynthesizedReadOnlyListMethod(this, ((MethodSymbol)declaringCompilation.GetSpecialTypeMember(SpecialMember.System_Collections_Generic_ICollection_T__Clear)).AsMember(namedTypeSymbol4), generateNotSupportedException));
		instance.Add(new SynthesizedReadOnlyListMethod(this, ((MethodSymbol)declaringCompilation.GetSpecialTypeMember(SpecialMember.System_Collections_Generic_ICollection_T__Contains)).AsMember(namedTypeSymbol4), generateContains));
		instance.Add(new SynthesizedReadOnlyListMethod(this, ((MethodSymbol)declaringCompilation.GetSpecialTypeMember(SpecialMember.System_Collections_Generic_ICollection_T__CopyTo)).AsMember(namedTypeSymbol4), generateCopyTo));
		instance.Add(new SynthesizedReadOnlyListMethod(this, ((MethodSymbol)declaringCompilation.GetSpecialTypeMember(SpecialMember.System_Collections_Generic_ICollection_T__Remove)).AsMember(namedTypeSymbol4), generateNotSupportedException));
		addProperty(instance, new SynthesizedReadOnlyListProperty(this, ((PropertySymbol)((MethodSymbol)declaringCompilation.GetSpecialTypeMember(SpecialMember.System_Collections_Generic_IList_T__get_Item)).AssociatedSymbol).AsMember(namedTypeSymbol5), generateIndexer, generateNotSupportedException));
		instance.Add(new SynthesizedReadOnlyListMethod(this, ((MethodSymbol)declaringCompilation.GetSpecialTypeMember(SpecialMember.System_Collections_Generic_IList_T__IndexOf)).AsMember(namedTypeSymbol5), generateIndexOf));
		instance.Add(new SynthesizedReadOnlyListMethod(this, ((MethodSymbol)declaringCompilation.GetSpecialTypeMember(SpecialMember.System_Collections_Generic_IList_T__Insert)).AsMember(namedTypeSymbol5), generateNotSupportedException));
		instance.Add(new SynthesizedReadOnlyListMethod(this, ((MethodSymbol)declaringCompilation.GetSpecialTypeMember(SpecialMember.System_Collections_Generic_IList_T__RemoveAt)).AsMember(namedTypeSymbol5), generateNotSupportedException));
		_members = instance.ToImmutableAndFree();
		static void addProperty(ArrayBuilder<Symbol> builder, PropertySymbol property)
		{
			builder.Add(property);
			builder.AddIfNotNull(property.GetMethod);
			builder.AddIfNotNull(property.SetMethod);
		}
		static BoundStatement generateContains(SyntheticBoundNodeFactory f, MethodSymbol method, MethodSymbol interfaceMethod)
		{
			SynthesizedReadOnlyListTypeSymbol synthesizedReadOnlyListTypeSymbol = (SynthesizedReadOnlyListTypeSymbol)method.ContainingType;
			FieldSymbol field = synthesizedReadOnlyListTypeSymbol._field;
			BoundFieldAccess boundFieldAccess = f.Field(f.This(), field);
			BoundParameter boundParameter = f.Parameter(method.Parameters[0]);
			if (synthesizedReadOnlyListTypeSymbol.IsSingleElement)
			{
				return f.Return(makeEqualityComparerDefaultEquals(f, boundFieldAccess, boundParameter));
			}
			if (synthesizedReadOnlyListTypeSymbol.IsArray || !interfaceMethod.ContainingType.IsGenericType)
			{
				NamedTypeSymbol containingType = interfaceMethod.ContainingType;
				Conversion conversion = f.ClassifyEmitConversion(boundFieldAccess, containingType);
				return f.Return(f.Call(f.Convert(containingType, boundFieldAccess, conversion), interfaceMethod, boundParameter));
			}
			MethodSymbol method2 = (MethodSymbol)synthesizedReadOnlyListTypeSymbol.GetFieldTypeMember(WellKnownMember.System_Collections_Generic_List_T__Contains);
			return f.Return(f.Call(boundFieldAccess, method2, boundParameter));
		}
		static BoundStatement generateCopyTo(SyntheticBoundNodeFactory f, MethodSymbol method, MethodSymbol interfaceMethod)
		{
			SynthesizedReadOnlyListTypeSymbol synthesizedReadOnlyListTypeSymbol = (SynthesizedReadOnlyListTypeSymbol)method.ContainingType;
			FieldSymbol field = synthesizedReadOnlyListTypeSymbol._field;
			BoundFieldAccess boundFieldAccess = f.Field(f.This(), field);
			BoundParameter boundParameter = f.Parameter(method.Parameters[0]);
			BoundParameter boundParameter2 = f.Parameter(method.Parameters[1]);
			BoundStatement boundStatement;
			if (synthesizedReadOnlyListTypeSymbol.IsSingleElement)
			{
				if (!interfaceMethod.ContainingType.IsGenericType)
				{
					MethodSymbol method2 = (MethodSymbol)method.DeclaringCompilation.GetSpecialTypeMember(SpecialMember.System_Array__SetValue);
					NamedTypeSymbol namedTypeSymbol6 = f.SpecialType(SpecialType.System_Object);
					Conversion conversion = f.ClassifyEmitConversion(boundFieldAccess, namedTypeSymbol6);
					boundStatement = f.ExpressionStatement(f.Call(boundParameter, method2, f.Convert(namedTypeSymbol6, boundFieldAccess, conversion), boundParameter2));
				}
				else
				{
					boundStatement = f.Assignment(f.ArrayAccess(boundParameter, boundParameter2), boundFieldAccess);
				}
			}
			else if (synthesizedReadOnlyListTypeSymbol.IsArray || !interfaceMethod.ContainingType.IsGenericType)
			{
				NamedTypeSymbol containingType = interfaceMethod.ContainingType;
				Conversion conversion2 = f.ClassifyEmitConversion(boundFieldAccess, containingType);
				boundStatement = f.ExpressionStatement(f.Call(f.Convert(containingType, boundFieldAccess, conversion2), interfaceMethod, boundParameter, boundParameter2));
			}
			else
			{
				MethodSymbol method3 = (MethodSymbol)synthesizedReadOnlyListTypeSymbol.GetFieldTypeMember(WellKnownMember.System_Collections_Generic_List_T__CopyTo);
				boundStatement = f.ExpressionStatement(f.Call(boundFieldAccess, method3, boundParameter, boundParameter2));
			}
			return f.Block(boundStatement, f.Return());
		}
		static BoundStatement generateCount(SyntheticBoundNodeFactory f, MethodSymbol method, MethodSymbol interfaceMethod)
		{
			SynthesizedReadOnlyListTypeSymbol synthesizedReadOnlyListTypeSymbol = (SynthesizedReadOnlyListTypeSymbol)method.ContainingType;
			if (synthesizedReadOnlyListTypeSymbol.IsSingleElement)
			{
				return f.Return(f.Literal(1));
			}
			FieldSymbol field = synthesizedReadOnlyListTypeSymbol._field;
			BoundFieldAccess boundFieldAccess = f.Field(f.This(), field);
			if (synthesizedReadOnlyListTypeSymbol.IsArray)
			{
				return f.Return(f.ArrayLength(boundFieldAccess));
			}
			PropertySymbol property = (PropertySymbol)synthesizedReadOnlyListTypeSymbol.GetFieldTypeMember(WellKnownMember.System_Collections_Generic_List_T__Count);
			return f.Return(f.Property(boundFieldAccess, property));
		}
		static BoundStatement generateGetEnumerator(SyntheticBoundNodeFactory f, MethodSymbol method, MethodSymbol interfaceMethod)
		{
			SynthesizedReadOnlyListTypeSymbol synthesizedReadOnlyListTypeSymbol = (SynthesizedReadOnlyListTypeSymbol)method.ContainingType;
			FieldSymbol field = synthesizedReadOnlyListTypeSymbol._field;
			BoundFieldAccess boundFieldAccess = f.Field(f.This(), field);
			if (synthesizedReadOnlyListTypeSymbol.IsSingleElement)
			{
				NamedTypeSymbol enumeratorType = synthesizedReadOnlyListTypeSymbol._enumeratorType;
				return f.Return(f.New(enumeratorType, boundFieldAccess));
			}
			NamedTypeSymbol containingType = interfaceMethod.ContainingType;
			Conversion conversion = f.ClassifyEmitConversion(boundFieldAccess, containingType);
			return f.Return(f.Call(f.Convert(containingType, boundFieldAccess, conversion), interfaceMethod));
		}
		static BoundStatement generateIndexOf(SyntheticBoundNodeFactory f, MethodSymbol method, MethodSymbol interfaceMethod)
		{
			SynthesizedReadOnlyListTypeSymbol synthesizedReadOnlyListTypeSymbol = (SynthesizedReadOnlyListTypeSymbol)method.ContainingType;
			FieldSymbol field = synthesizedReadOnlyListTypeSymbol._field;
			BoundFieldAccess boundFieldAccess = f.Field(f.This(), field);
			BoundParameter boundParameter = f.Parameter(method.Parameters[0]);
			if (synthesizedReadOnlyListTypeSymbol.IsSingleElement)
			{
				return f.Return(f.Conditional(makeEqualityComparerDefaultEquals(f, boundFieldAccess, boundParameter), f.Literal(0), f.Literal(-1), method.ReturnType));
			}
			if (synthesizedReadOnlyListTypeSymbol.IsArray || !interfaceMethod.ContainingType.IsGenericType)
			{
				NamedTypeSymbol containingType = interfaceMethod.ContainingType;
				Conversion conversion = f.ClassifyEmitConversion(boundFieldAccess, containingType);
				return f.Return(f.Call(f.Convert(containingType, boundFieldAccess, conversion), interfaceMethod, boundParameter));
			}
			MethodSymbol method2 = (MethodSymbol)synthesizedReadOnlyListTypeSymbol.GetFieldTypeMember(WellKnownMember.System_Collections_Generic_List_T__IndexOf);
			return f.Return(f.Call(boundFieldAccess, method2, boundParameter));
		}
		static BoundStatement generateIndexer(SyntheticBoundNodeFactory f, MethodSymbol method, MethodSymbol interfaceMethod)
		{
			SynthesizedReadOnlyListTypeSymbol synthesizedReadOnlyListTypeSymbol = (SynthesizedReadOnlyListTypeSymbol)method.ContainingType;
			FieldSymbol field = synthesizedReadOnlyListTypeSymbol._field;
			BoundFieldAccess boundFieldAccess = f.Field(f.This(), field);
			BoundParameter boundParameter = f.Parameter(method.Parameters[0]);
			if (synthesizedReadOnlyListTypeSymbol.IsSingleElement)
			{
				MethodSymbol ctor = (MethodSymbol)method.DeclaringCompilation.GetWellKnownTypeMember(WellKnownMember.System_IndexOutOfRangeException__ctor);
				return f.Block(f.If(f.IntNotEqual(boundParameter, f.Literal(0)), f.Throw(f.New(ctor))), f.Return(boundFieldAccess));
			}
			if (synthesizedReadOnlyListTypeSymbol.IsArray)
			{
				return f.Return(f.ArrayAccess(boundFieldAccess, boundParameter));
			}
			PropertySymbol property = (PropertySymbol)((MethodSymbol)synthesizedReadOnlyListTypeSymbol.GetFieldTypeMember(WellKnownMember.System_Collections_Generic_List_T__get_Item)).AssociatedSymbol;
			return f.Return(f.Indexer(boundFieldAccess, property, boundParameter));
		}
		static BoundStatement generateIsFixedSize(SyntheticBoundNodeFactory f, MethodSymbol method, MethodSymbol interfaceMethod)
		{
			return f.Return(f.Literal(value: true));
		}
		static BoundStatement generateIsReadOnly(SyntheticBoundNodeFactory f, MethodSymbol method, MethodSymbol interfaceMethod)
		{
			return f.Return(f.Literal(value: true));
		}
		static BoundStatement generateIsSynchronized(SyntheticBoundNodeFactory f, MethodSymbol method, MethodSymbol interfaceMethod)
		{
			return f.Return(f.Literal(value: false));
		}
		static BoundStatement generateNotSupportedException(SyntheticBoundNodeFactory f, MethodSymbol method, MethodSymbol interfaceMethod)
		{
			MethodSymbol ctor = (MethodSymbol)method.DeclaringCompilation.GetWellKnownTypeMember(WellKnownMember.System_NotSupportedException__ctor);
			return f.Throw(f.New(ctor));
		}
		static BoundStatement generateSyncRoot(SyntheticBoundNodeFactory f, MethodSymbol method, MethodSymbol interfaceMethod)
		{
			BoundThisReference arg = f.This();
			TypeSymbol returnType = interfaceMethod.ReturnType;
			Conversion conversion = f.ClassifyEmitConversion(arg, returnType);
			return f.Return(f.Convert(returnType, arg, conversion));
		}
		static BoundCall makeEqualityComparerDefaultEquals(SyntheticBoundNodeFactory f, BoundFieldAccess fieldReference, BoundParameter parameterReference)
		{
			TypeSymbol type = fieldReference.Type;
			MethodSymbol methodSymbol = f.WellKnownMethod(WellKnownMember.System_Collections_Generic_EqualityComparer_T__get_Default);
			MethodSymbol methodSymbol2 = f.WellKnownMethod(WellKnownMember.System_Collections_Generic_EqualityComparer_T__Equals);
			NamedTypeSymbol namedTypeSymbol6 = methodSymbol2.ContainingType.Construct(type);
			Conversion conversion = f.ClassifyEmitConversion(parameterReference, type);
			return f.Call(f.StaticCall(namedTypeSymbol6, methodSymbol.AsMember(namedTypeSymbol6)), methodSymbol2.AsMember(namedTypeSymbol6), fieldReference, f.Convert(type, parameterReference, conversion));
		}
	}

	private Symbol GetFieldTypeMember(WellKnownMember member)
	{
		return DeclaringCompilation.GetWellKnownTypeMember(member).SymbolAsMember((NamedTypeSymbol)_field.Type);
	}

	public static bool CanCreateSingleElement(CSharpCompilation compilation)
	{
		if (!(compilation.GetWellKnownType(WellKnownType.System_IndexOutOfRangeException) is MissingMetadataTypeSymbol) && !(compilation.GetWellKnownType(WellKnownType.System_Collections_Generic_EqualityComparer_T) is MissingMetadataTypeSymbol) && (object)compilation.GetWellKnownTypeMember(WellKnownMember.System_IndexOutOfRangeException__ctor) != null && (object)compilation.GetSpecialTypeMember(SpecialMember.System_Array__SetValue) != null && (object)compilation.GetWellKnownTypeMember(WellKnownMember.System_Collections_Generic_EqualityComparer_T__get_Default) != null && (object)compilation.GetWellKnownTypeMember(WellKnownMember.System_Collections_Generic_EqualityComparer_T__Equals) != null && !(compilation.GetSpecialType(SpecialType.System_IDisposable) is MissingMetadataTypeSymbol) && !(compilation.GetSpecialType(SpecialType.System_Collections_IEnumerator) is MissingMetadataTypeSymbol) && !(compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerator_T) is MissingMetadataTypeSymbol) && (object)compilation.GetSpecialTypeMember(SpecialMember.System_Collections_Generic_IEnumerator_T__Current) != null && (object)compilation.GetSpecialTypeMember(SpecialMember.System_Collections_IEnumerator__Current) != null && (object)compilation.GetSpecialTypeMember(SpecialMember.System_Collections_IEnumerator__MoveNext) != null && (object)compilation.GetSpecialTypeMember(SpecialMember.System_Collections_IEnumerator__Reset) != null)
		{
			return (object)compilation.GetSpecialTypeMember(SpecialMember.System_IDisposable__Dispose) != null;
		}
		return false;
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
		if ((object)_enumeratorType == null)
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}
		return ImmutableArray.Create(_enumeratorType);
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name, int arity)
	{
		return GetTypeMembers(name).WhereAsArray((NamedTypeSymbol type, int num) => type.Arity == num, arity);
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name)
	{
		return GetTypeMembers().WhereAsArray((NamedTypeSymbol type, ReadOnlyMemory<char> readOnlyMemory) => System.MemoryExtensions.AsSpan(type.Name).SequenceEqual(readOnlyMemory.Span), name);
	}

	protected override NamedTypeSymbol WithTupleDataCore(TupleExtraData newData)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/ReadOnlyListType/SynthesizedReadOnlyListTypeSymbol.cs", 945);
	}

	internal override NamedTypeSymbol AsNativeInteger()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/ReadOnlyListType/SynthesizedReadOnlyListTypeSymbol.cs", 947);
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
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/ReadOnlyListType/SynthesizedReadOnlyListTypeSymbol.cs", 957);
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers(string name)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/ReadOnlyListType/SynthesizedReadOnlyListTypeSymbol.cs", 959);
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

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		Symbol.AddSynthesizedAttribute(ref attributes, DeclaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor));
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
