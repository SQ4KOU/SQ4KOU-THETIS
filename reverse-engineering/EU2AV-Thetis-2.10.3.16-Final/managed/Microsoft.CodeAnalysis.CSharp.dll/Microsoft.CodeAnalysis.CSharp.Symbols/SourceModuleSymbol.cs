using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceModuleSymbol : NonMissingModuleSymbol, IAttributeTargetSymbol
{
	private readonly SourceAssemblySymbol _assemblySymbol;

	private ImmutableArray<AssemblySymbol> _lazyAssembliesToEmbedTypesFrom;

	private ThreeState _lazyContainsExplicitDefinitionOfNoPiaLocalTypes;

	private readonly DeclarationTable _sources;

	private SymbolCompletionState _state;

	private CustomAttributesBag<CSharpAttributeData> _lazyCustomAttributesBag;

	private ImmutableArray<Location> _locations;

	private NamespaceSymbol _globalNamespace;

	private bool _hasBadAttributes;

	private ThreeState _lazyUseUpdatedEscapeRules;

	private ThreeState _lazyRequiresRefSafetyRulesAttribute;

	private readonly string _name;

	internal bool HasBadAttributes => _hasBadAttributes;

	internal override int Ordinal => 0;

	internal override Machine Machine => DeclaringCompilation.Options.Platform switch
	{
		Platform.Arm => Machine.ArmThumb2, 
		Platform.X64 => Machine.Amd64, 
		Platform.Arm64 => Machine.Arm64, 
		Platform.Itanium => Machine.IA64, 
		_ => Machine.I386, 
	};

	internal override bool Bit32Required => DeclaringCompilation.Options.Platform == Platform.X86;

	internal bool AnyReferencedAssembliesAreLinked => GetAssembliesToEmbedTypesFrom().Length > 0;

	internal bool ContainsExplicitDefinitionOfNoPiaLocalTypes
	{
		get
		{
			if (_lazyContainsExplicitDefinitionOfNoPiaLocalTypes == ThreeState.Unknown)
			{
				_lazyContainsExplicitDefinitionOfNoPiaLocalTypes = NamespaceContainsExplicitDefinitionOfNoPiaLocalTypes(GlobalNamespace).ToThreeState();
			}
			return _lazyContainsExplicitDefinitionOfNoPiaLocalTypes == ThreeState.True;
		}
	}

	public override NamespaceSymbol GlobalNamespace
	{
		get
		{
			if ((object)_globalNamespace == null)
			{
				BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
				SourceNamespaceSymbol value = new SourceNamespaceSymbol(this, this, DeclaringCompilation.MergedRootDeclaration, instance);
				if (Interlocked.CompareExchange(ref _globalNamespace, value, null) == null)
				{
					AddDeclarationDiagnostics(instance);
				}
				instance.Free();
			}
			return _globalNamespace;
		}
	}

	internal sealed override bool RequiresCompletion => true;

	public override ImmutableArray<Location> Locations
	{
		get
		{
			if (_locations.IsDefault)
			{
				ImmutableInterlocked.InterlockedInitialize(ref _locations, DeclaringCompilation.MergedRootDeclaration.Declarations.SelectAsArray((Func<SingleNamespaceDeclaration, Location>)((SingleNamespaceDeclaration d) => d.Location)));
			}
			return _locations;
		}
	}

	public override string Name => _name;

	public override Symbol ContainingSymbol => _assemblySymbol;

	public override AssemblySymbol ContainingAssembly => _assemblySymbol;

	internal SourceAssemblySymbol ContainingSourceAssembly => _assemblySymbol;

	internal override CSharpCompilation DeclaringCompilation => _assemblySymbol.DeclaringCompilation;

	internal override ICollection<string> TypeNames => _sources.TypeNames;

	internal override ICollection<string> NamespaceNames => _sources.NamespaceNames;

	IAttributeTargetSymbol IAttributeTargetSymbol.AttributesOwner => _assemblySymbol;

	AttributeLocation IAttributeTargetSymbol.DefaultAttributeLocation => AttributeLocation.Module;

	AttributeLocation IAttributeTargetSymbol.AllowedAttributeLocations
	{
		get
		{
			if (!ContainingAssembly.IsInteractive)
			{
				return AttributeLocation.Assembly | AttributeLocation.Module;
			}
			return AttributeLocation.None;
		}
	}

	internal override bool HasAssemblyCompilationRelaxationsAttribute => ((SourceAssemblySymbol)ContainingAssembly).GetSourceDecodedWellKnownAttributeData()?.HasCompilationRelaxationsAttribute ?? false;

	internal override bool HasAssemblyRuntimeCompatibilityAttribute => ((SourceAssemblySymbol)ContainingAssembly).GetSourceDecodedWellKnownAttributeData()?.HasRuntimeCompatibilityAttribute ?? false;

	internal override CharSet? DefaultMarshallingCharSet
	{
		get
		{
			ModuleWellKnownAttributeData decodedWellKnownAttributeData = GetDecodedWellKnownAttributeData();
			if (decodedWellKnownAttributeData == null || !decodedWellKnownAttributeData.HasDefaultCharSetAttribute)
			{
				return null;
			}
			return decodedWellKnownAttributeData.DefaultCharacterSet;
		}
	}

	public sealed override bool AreLocalsZeroed
	{
		get
		{
			ModuleWellKnownAttributeData decodedWellKnownAttributeData = GetDecodedWellKnownAttributeData();
			if (decodedWellKnownAttributeData == null)
			{
				return true;
			}
			return !decodedWellKnownAttributeData.HasSkipLocalsInitAttribute;
		}
	}

	internal override bool UseUpdatedEscapeRules
	{
		get
		{
			if (_lazyUseUpdatedEscapeRules == ThreeState.Unknown)
			{
				bool value = _assemblySymbol.DeclaringCompilation.IsFeatureEnabled(MessageID.IDS_FeatureRefFields) || _assemblySymbol.RuntimeSupportsByRefFields;
				_lazyUseUpdatedEscapeRules = value.ToThreeState();
			}
			return _lazyUseUpdatedEscapeRules == ThreeState.True;
		}
	}

	internal sealed override ObsoleteAttributeData? ObsoleteAttributeData
	{
		get
		{
			CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = _lazyCustomAttributesBag;
			if (lazyCustomAttributesBag != null && lazyCustomAttributesBag.IsDecodedWellKnownAttributeDataComputed)
			{
				return ((ModuleWellKnownAttributeData)lazyCustomAttributesBag.DecodedWellKnownAttributeData)?.ExperimentalAttributeData;
			}
			if (((SourceAssemblySymbol)ContainingAssembly).GetAttributeDeclarations().IsEmpty)
			{
				return null;
			}
			return Microsoft.CodeAnalysis.ObsoleteAttributeData.Uninitialized;
		}
	}

	internal SourceModuleSymbol(SourceAssemblySymbol assemblySymbol, DeclarationTable declarations, string moduleName)
	{
		_assemblySymbol = assemblySymbol;
		_sources = declarations;
		_name = moduleName;
	}

	internal void RecordPresenceOfBadAttributes()
	{
		_hasBadAttributes = true;
	}

	internal bool MightContainNoPiaLocalTypes()
	{
		if (!AnyReferencedAssembliesAreLinked)
		{
			return ContainsExplicitDefinitionOfNoPiaLocalTypes;
		}
		return true;
	}

	internal ImmutableArray<AssemblySymbol> GetAssembliesToEmbedTypesFrom()
	{
		if (_lazyAssembliesToEmbedTypesFrom.IsDefault)
		{
			ArrayBuilder<AssemblySymbol> instance = ArrayBuilder<AssemblySymbol>.GetInstance();
			foreach (AssemblySymbol referencedAssemblySymbol in GetReferencedAssemblySymbols())
			{
				if (referencedAssemblySymbol.IsLinked)
				{
					instance.Add(referencedAssemblySymbol);
				}
			}
			ImmutableInterlocked.InterlockedCompareExchange(ref _lazyAssembliesToEmbedTypesFrom, instance.ToImmutableAndFree(), default(ImmutableArray<AssemblySymbol>));
		}
		return _lazyAssembliesToEmbedTypesFrom;
	}

	private static bool NamespaceContainsExplicitDefinitionOfNoPiaLocalTypes(NamespaceSymbol ns)
	{
		foreach (Symbol item in ns.GetMembersUnordered())
		{
			switch (item.Kind)
			{
			case SymbolKind.Namespace:
				if (NamespaceContainsExplicitDefinitionOfNoPiaLocalTypes((NamespaceSymbol)item))
				{
					return true;
				}
				break;
			case SymbolKind.NamedType:
				if (((NamedTypeSymbol)item).IsExplicitDefinitionOfNoPiaLocalType)
				{
					return true;
				}
				break;
			}
		}
		return false;
	}

	internal sealed override bool HasComplete(CompletionPart part)
	{
		return _state.HasComplete(part);
	}

	internal override void ForceComplete(SourceLocation? locationOpt, Predicate<Symbol>? filter, CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			CompletionPart nextIncompletePart = _state.NextIncompletePart;
			switch (nextIncompletePart)
			{
			case CompletionPart.Attributes:
				GetAttributes();
				break;
			case CompletionPart.StartBaseType:
			{
				BindingDiagnosticBag bindingDiagnosticBag = null;
				if (AnyReferencedAssembliesAreLinked)
				{
					bindingDiagnosticBag = BindingDiagnosticBag.GetInstance();
					ValidateLinkedAssemblies(bindingDiagnosticBag, cancellationToken);
				}
				if (DeclaringCompilation.DataSectionStringLiteralThreshold.HasValue)
				{
					if (bindingDiagnosticBag == null)
					{
						bindingDiagnosticBag = BindingDiagnosticBag.GetInstance();
					}
					Binder.GetWellKnownTypeMember(DeclaringCompilation, WellKnownMember.System_Text_Encoding__get_UTF8, bindingDiagnosticBag, NoLocation.Singleton);
					Binder.GetWellKnownTypeMember(DeclaringCompilation, WellKnownMember.System_Text_Encoding__GetString, bindingDiagnosticBag, NoLocation.Singleton);
				}
				if (_state.NotePartComplete(CompletionPart.StartBaseType))
				{
					if (bindingDiagnosticBag != null)
					{
						_assemblySymbol.AddDeclarationDiagnostics(bindingDiagnosticBag);
					}
					_state.NotePartComplete(CompletionPart.FinishBaseType);
				}
				bindingDiagnosticBag?.Free();
				break;
			}
			case CompletionPart.FinishBaseType:
				_state.SpinWaitComplete(CompletionPart.FinishBaseType, cancellationToken);
				break;
			case CompletionPart.MembersCompleted:
				GlobalNamespace.ForceComplete(locationOpt, filter, cancellationToken);
				if (GlobalNamespace.HasComplete(CompletionPart.MembersCompleted))
				{
					Volatile.Write(ref DeclaringCompilation.InterceptorsDiscoveryComplete, value: true);
					_state.NotePartComplete(CompletionPart.MembersCompleted);
					break;
				}
				return;
			case CompletionPart.None:
				return;
			default:
				_state.NotePartComplete(nextIncompletePart);
				break;
			}
			_state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	private void ValidateLinkedAssemblies(BindingDiagnosticBag diagnostics, CancellationToken cancellationToken)
	{
		foreach (AssemblySymbol referencedAssemblySymbol in GetReferencedAssemblySymbols())
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (!referencedAssemblySymbol.IsMissing && referencedAssemblySymbol.IsLinked)
			{
				if (!referencedAssemblySymbol.GetGuidString(out var _))
				{
					diagnostics.Add(ErrorCode.ERR_NoPIAAssemblyMissingAttribute, NoLocation.Singleton, referencedAssemblySymbol, AttributeDescription.GuidAttribute.FullName);
				}
				if (!referencedAssemblySymbol.HasImportedFromTypeLibAttribute && !referencedAssemblySymbol.HasPrimaryInteropAssemblyAttribute)
				{
					diagnostics.Add(ErrorCode.ERR_NoPIAAssemblyMissingAttributes, NoLocation.Singleton, referencedAssemblySymbol, AttributeDescription.ImportedFromTypeLibAttribute.FullName, AttributeDescription.PrimaryInteropAssemblyAttribute.FullName);
				}
			}
		}
	}

	internal void DiscoverInterceptorsIfNeeded()
	{
		if (!Volatile.Read(in DeclaringCompilation.InterceptorsDiscoveryComplete))
		{
			discoverInterceptors();
			Volatile.Write(ref DeclaringCompilation.InterceptorsDiscoveryComplete, value: true);
		}
		void discoverInterceptors()
		{
			Location firstLocationOrNone = GlobalNamespace.GetFirstLocationOrNone();
			if (firstLocationOrNone.IsInSource)
			{
				ArrayBuilder<NamespaceOrTypeSymbol> instance = ArrayBuilder<NamespaceOrTypeSymbol>.GetInstance();
				foreach (ImmutableArray<string> interceptorsNamespace in ((CSharpParseOptions)firstLocationOrNone.SourceTree.Options).InterceptorsNamespaces)
				{
					if (interceptorsNamespace.Length == 1 && interceptorsNamespace[0] == "global")
					{
						instance.Clear();
						instance.Add(GlobalNamespace);
						break;
					}
					NamespaceSymbol namespaceSymbol = GlobalNamespace;
					foreach (string item2 in interceptorsNamespace)
					{
						namespaceSymbol = namespaceSymbol.GetNestedNamespace(item2);
						if ((object)namespaceSymbol == null)
						{
							break;
						}
					}
					if ((object)namespaceSymbol != null)
					{
						instance.Add(namespaceSymbol);
					}
				}
				while (instance.Count > 0)
				{
					NamespaceOrTypeSymbol namespaceOrTypeSymbol = instance.Pop();
					if (namespaceOrTypeSymbol is SourceMemberContainerTypeSymbol sourceMemberContainerTypeSymbol)
					{
						sourceMemberContainerTypeSymbol.DiscoverInterceptors(instance);
					}
					else
					{
						if (!(namespaceOrTypeSymbol is SourceNamespaceSymbol sourceNamespaceSymbol))
						{
							throw ExceptionUtilities.UnexpectedValue(namespaceOrTypeSymbol);
						}
						foreach (Symbol member in sourceNamespaceSymbol.GetMembers())
						{
							if (!(member is NamespaceOrTypeSymbol item))
							{
								throw ExceptionUtilities.UnexpectedValue(member);
							}
							instance.Add(item);
						}
					}
				}
				instance.Free();
			}
		}
	}

	private CustomAttributesBag<CSharpAttributeData> GetAttributesBag()
	{
		if (_lazyCustomAttributesBag == null || !_lazyCustomAttributesBag.IsSealed)
		{
			ImmutableArray<SyntaxList<AttributeListSyntax>> attributeDeclarations = ((SourceAssemblySymbol)ContainingAssembly).GetAttributeDeclarations();
			if (LoadAndValidateAttributes(OneOrMany.Create(attributeDeclarations), ref _lazyCustomAttributesBag))
			{
				_state.NotePartComplete(CompletionPart.Attributes);
			}
		}
		return _lazyCustomAttributesBag;
	}

	public sealed override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return GetAttributesBag().Attributes;
	}

	private ModuleWellKnownAttributeData GetDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (ModuleWellKnownAttributeData)customAttributesBag.DecodedWellKnownAttributeData;
	}

	protected override void DecodeWellKnownAttributeImpl(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		CSharpAttributeData attribute = arguments.Attribute;
		if (attribute.IsTargetAttribute(AttributeDescription.DefaultCharSetAttribute))
		{
			CharSet constructorArgument = attribute.GetConstructorArgument<CharSet>(0, SpecialType.System_Enum);
			if (!CommonModuleWellKnownAttributeData.IsValidCharSet(constructorArgument))
			{
				((BindingDiagnosticBag)arguments.Diagnostics).Add(ErrorCode.ERR_InvalidAttributeArgument, attribute.GetAttributeArgumentLocation(0), arguments.AttributeSyntaxOpt.GetErrorDisplayName());
			}
			else
			{
				arguments.GetOrCreateData<ModuleWellKnownAttributeData>().DefaultCharacterSet = constructorArgument;
			}
		}
		else if (!ReportExplicitUseOfReservedAttributes(in arguments, ReservedAttributes.NullableContextAttribute | ReservedAttributes.NullablePublicOnlyAttribute | ReservedAttributes.RefSafetyRulesAttribute | ReservedAttributes.ExtensionMarkerAttribute))
		{
			if (attribute.IsTargetAttribute(AttributeDescription.SkipLocalsInitAttribute))
			{
				CSharpAttributeData.DecodeSkipLocalsInitAttribute<ModuleWellKnownAttributeData>(DeclaringCompilation, ref arguments);
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.ExperimentalAttribute))
			{
				arguments.GetOrCreateData<ModuleWellKnownAttributeData>().ExperimentalAttributeData = attribute.DecodeExperimentalAttribute();
			}
		}
	}

	internal bool RequiresRefSafetyRulesAttribute()
	{
		if (_lazyRequiresRefSafetyRulesAttribute == ThreeState.Unknown)
		{
			bool value = UseUpdatedEscapeRules && !isFeatureDisabled(_assemblySymbol.DeclaringCompilation) && namespaceIncludesTypeDeclarations(GlobalNamespace);
			_lazyRequiresRefSafetyRulesAttribute = value.ToThreeState();
		}
		return _lazyRequiresRefSafetyRulesAttribute.Value();
		static bool isFeatureDisabled(CSharpCompilation compilation)
		{
			return ((CSharpParseOptions)(compilation.SyntaxTrees.FirstOrDefault()?.Options))?.HasFeature("noRefSafetyRulesAttribute") ?? false;
		}
		static bool namespaceIncludesTypeDeclarations(NamespaceSymbol ns)
		{
			foreach (Symbol item in ns.GetMembersUnordered())
			{
				switch (item.Kind)
				{
				case SymbolKind.Namespace:
					if (namespaceIncludesTypeDeclarations((NamespaceSymbol)item))
					{
						return true;
					}
					break;
				case SymbolKind.NamedType:
					return true;
				}
			}
			return false;
		}
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		CSharpCompilation declaringCompilation = _assemblySymbol.DeclaringCompilation;
		if (declaringCompilation.Options.AllowUnsafe && !(declaringCompilation.GetWellKnownType(WellKnownType.System_Security_UnverifiableCodeAttribute) is MissingMetadataTypeSymbol))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Security_UnverifiableCodeAttribute__ctor));
		}
		if (RequiresRefSafetyRulesAttribute())
		{
			ImmutableArray<TypedConstant> arguments = ImmutableArray.Create(new TypedConstant(declaringCompilation.GetSpecialType(SpecialType.System_Int32), TypedConstantKind.Primitive, 11));
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeRefSafetyRulesAttribute(arguments));
		}
		if (moduleBuilder.ShouldEmitNullablePublicOnlyAttribute())
		{
			ImmutableArray<TypedConstant> arguments2 = ImmutableArray.Create(new TypedConstant(declaringCompilation.GetSpecialType(SpecialType.System_Boolean), TypedConstantKind.Primitive, _assemblySymbol.InternalsAreVisible));
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeNullablePublicOnlyAttribute(arguments2));
		}
	}

	public override ModuleMetadata? GetMetadata()
	{
		return null;
	}
}
