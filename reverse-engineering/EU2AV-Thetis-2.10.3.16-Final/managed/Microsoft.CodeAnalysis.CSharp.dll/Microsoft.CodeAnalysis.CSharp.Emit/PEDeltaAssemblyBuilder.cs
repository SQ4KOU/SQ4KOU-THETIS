using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection.Metadata;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit.NoPia;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal sealed class PEDeltaAssemblyBuilder : PEAssemblyBuilderBase, IPEDeltaAssemblyBuilder
{
	private readonly SymbolChanges _changes;

	private readonly CSharpSymbolMatcher.DeepTranslator _deepTranslator;

	private readonly MethodSymbol? _predefinedHotReloadExceptionConstructor;

	private readonly EmitDifferenceOptions _options;

	private SynthesizedHotReloadExceptionSymbol? _lazyHotReloadExceptionType;

	private volatile bool _freezeHotReloadExceptionTypeUsage;

	private volatile bool _isHotReloadExceptionTypeUsed;

	public override SymbolChanges? EncSymbolChanges => _changes;

	public override EmitBaseline PreviousGeneration => _changes.DefinitionMap.Baseline;

	public override bool FieldRvaSupported => _options.EmitFieldRva;

	public override bool MethodImplSupported => _options.MethodImplEntriesSupported;

	internal CSharpDefinitionMap PreviousDefinitions => (CSharpDefinitionMap)_changes.DefinitionMap;

	public PEDeltaAssemblyBuilder(SourceAssemblySymbol sourceAssembly, CSharpSymbolChanges changes, EmitOptions emitOptions, EmitDifferenceOptions options, OutputKind outputKind, ModulePropertiesForSerialization serializationProperties, IEnumerable<ResourceDescription> manifestResources, MethodSymbol? predefinedHotReloadExceptionConstructor)
		: base(sourceAssembly, emitOptions, outputKind, serializationProperties, manifestResources, ImmutableArray<NamedTypeSymbol>.Empty)
	{
		_changes = changes;
		_options = options;
		_deepTranslator = new CSharpSymbolMatcher.DeepTranslator(sourceAssembly.GetSpecialType(SpecialType.System_Object));
		_predefinedHotReloadExceptionConstructor = predefinedHotReloadExceptionConstructor;
	}

	internal override ITypeReference EncTranslateLocalVariableType(TypeSymbol type, DiagnosticBag diagnostics)
	{
		TypeSymbol typeSymbol = (TypeSymbol)_deepTranslator.Visit(type);
		return Translate(typeSymbol ?? type, null, diagnostics);
	}

	internal static EmitBaseline.MetadataSymbols GetOrCreateMetadataSymbols(EmitBaseline initialBaseline, CSharpCompilation compilation)
	{
		if (initialBaseline.LazyMetadataSymbols != null)
		{
			return initialBaseline.LazyMetadataSymbols;
		}
		ModuleMetadata originalMetadata = initialBaseline.OriginalMetadata;
		MetadataDecoder metadataDecoder = new MetadataDecoder(compilation.RemoveAllSyntaxTrees().GetBoundReferenceManager().CreatePEAssemblyForAssemblyMetadata(AssemblyMetadata.Create(originalMetadata), MetadataImportOptions.All, out ImmutableDictionary<AssemblyIdentity, AssemblyIdentity> assemblyReferenceIdentityMap)
			.PrimaryModule);
		EmitBaseline.MetadataSymbols value = new EmitBaseline.MetadataSymbols(GetSynthesizedTypesFromMetadata(originalMetadata.MetadataReader, metadataDecoder), metadataDecoder, assemblyReferenceIdentityMap);
		return InterlockedOperations.Initialize(ref initialBaseline.LazyMetadataSymbols, value);
	}

	internal static SynthesizedTypeMaps GetSynthesizedTypesFromMetadata(MetadataReader reader, MetadataDecoder metadataDecoder)
	{
		ImmutableSegmentedDictionary<AnonymousTypeKey, AnonymousTypeValue>.Builder builder = ImmutableSegmentedDictionary.CreateBuilder<AnonymousTypeKey, AnonymousTypeValue>();
		PooledDictionary<AnonymousDelegateWithIndexedNamePartialKey, ArrayBuilder<AnonymousTypeValue>> instance = PooledDictionary<AnonymousDelegateWithIndexedNamePartialKey, ArrayBuilder<AnonymousTypeValue>>.GetInstance();
		ImmutableSegmentedDictionary<SynthesizedDelegateKey, SynthesizedDelegateValue>.Builder builder2 = ImmutableSegmentedDictionary.CreateBuilder<SynthesizedDelegateKey, SynthesizedDelegateValue>();
		foreach (TypeDefinitionHandle typeDefinition2 in reader.TypeDefinitions)
		{
			TypeDefinition typeDefinition = reader.GetTypeDefinition(typeDefinition2);
			if (!typeDefinition.Namespace.IsNil)
			{
				continue;
			}
			short arity;
			if (reader.StringComparer.StartsWith(typeDefinition.Name, "<>A") || reader.StringComparer.StartsWith(typeDefinition.Name, "<>F"))
			{
				SynthesizedDelegateKey key = new SynthesizedDelegateKey(reader.GetString(typeDefinition.Name));
				NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)metadataDecoder.GetTypeOfToken(typeDefinition2);
				SynthesizedDelegateValue value = new SynthesizedDelegateValue(namedTypeSymbol.GetCciAdapter());
				builder2.Add(key, value);
			}
			else if (reader.StringComparer.StartsWith(typeDefinition.Name, "<>f__AnonymousType"))
			{
				string text = MetadataHelpers.InferTypeArityAndUnmangleMetadataName(reader.GetString(typeDefinition.Name), out arity);
				if (int.TryParse(text.Substring("<>f__AnonymousType".Length), NumberStyles.None, CultureInfo.InvariantCulture, out var result))
				{
					ArrayBuilder<AnonymousTypeKeyField> instance2 = ArrayBuilder<AnonymousTypeKeyField>.GetInstance();
					if (TryGetAnonymousTypeKey(reader, typeDefinition, instance2))
					{
						NamedTypeSymbol namedTypeSymbol2 = (NamedTypeSymbol)metadataDecoder.GetTypeOfToken(typeDefinition2);
						AnonymousTypeKey key2 = new AnonymousTypeKey(instance2.ToImmutable());
						AnonymousTypeValue value2 = new AnonymousTypeValue(text, result, namedTypeSymbol2.GetCciAdapter());
						builder.Add(key2, value2);
					}
					instance2.Free();
				}
			}
			else
			{
				if (!reader.StringComparer.StartsWith(typeDefinition.Name, "<>f__AnonymousDelegate"))
				{
					continue;
				}
				string text2 = MetadataHelpers.InferTypeArityAndUnmangleMetadataName(reader.GetString(typeDefinition.Name), out arity);
				if (!int.TryParse(text2.Substring("<>f__AnonymousDelegate".Length), NumberStyles.None, CultureInfo.InvariantCulture, out var result2))
				{
					continue;
				}
				NamedTypeSymbol namedTypeSymbol3 = (NamedTypeSymbol)metadataDecoder.GetTypeOfToken(typeDefinition2);
				AnonymousTypeValue value3 = new AnonymousTypeValue(text2, result2, namedTypeSymbol3.GetCciAdapter());
				int num = -1;
				foreach (MethodDefinitionHandle method in typeDefinition.GetMethods())
				{
					MethodDefinition methodDefinition = reader.GetMethodDefinition(method);
					if (reader.StringComparer.Equals(methodDefinition.Name, "Invoke"))
					{
						try
						{
							metadataDecoder.DecodeMethodSignatureParameterCountsOrThrow(method, out var parameterCount, out var _);
							num = parameterCount;
						}
						catch (BadImageFormatException)
						{
							continue;
						}
						break;
					}
				}
				if (num >= 0)
				{
					instance.AddPooled(new AnonymousDelegateWithIndexedNamePartialKey(namedTypeSymbol3.Arity, num), value3);
				}
			}
		}
		return new SynthesizedTypeMaps(builder.ToImmutable(), builder2.ToImmutable(), instance.ToImmutableSegmentedDictionaryAndFree());
	}

	private static bool TryGetAnonymousTypeKey(MetadataReader reader, TypeDefinition def, ArrayBuilder<AnonymousTypeKeyField> builder)
	{
		foreach (GenericParameterHandle genericParameter in def.GetGenericParameters())
		{
			if (!GeneratedNameParser.TryParseAnonymousTypeParameterName(reader.GetString(reader.GetGenericParameter(genericParameter).Name), out string propertyName))
			{
				return false;
			}
			builder.Add(new AnonymousTypeKeyField(propertyName, isKey: false, ignoreCase: false));
		}
		return true;
	}

	public override IEnumerable<INamespaceTypeDefinition> GetTopLevelTypeDefinitions(EmitContext context)
	{
		return GetTopLevelTypeDefinitionsExcludingNoPiaAndRootModule(context, includePrivateImplementationDetails: true);
	}

	public override IEnumerable<INamespaceTypeDefinition> GetTopLevelSourceTypeDefinitions(EmitContext context)
	{
		return _changes.GetTopLevelSourceTypeDefinitions(context);
	}

	internal override VariableSlotAllocator? TryCreateVariableSlotAllocator(MethodSymbol method, MethodSymbol topLevelMethod, DiagnosticBag diagnostics)
	{
		return _changes.DefinitionMap.TryCreateVariableSlotAllocator(Compilation, method, topLevelMethod, diagnostics);
	}

	internal override MethodInstrumentation GetMethodBodyInstrumentations(MethodSymbol method)
	{
		return _changes.DefinitionMap.GetMethodBodyInstrumentations(method);
	}

	internal override int GetNextAnonymousTypeIndex()
	{
		return PreviousGeneration.GetNextAnonymousTypeIndex();
	}

	internal override int GetNextAnonymousDelegateIndex()
	{
		return PreviousGeneration.GetNextAnonymousDelegateIndex();
	}

	internal override bool TryGetPreviousAnonymousTypeValue(AnonymousTypeManager.AnonymousTypeOrDelegateTemplateSymbol template, out AnonymousTypeValue typeValue)
	{
		return PreviousDefinitions.TryGetAnonymousTypeValue(template, out typeValue);
	}

	public void OnCreatedIndices(DiagnosticBag diagnostics)
	{
		EmbeddedTypesManager embeddedTypesManagerOpt = EmbeddedTypesManagerOpt;
		if (embeddedTypesManagerOpt == null)
		{
			return;
		}
		foreach (NamedTypeSymbol key in embeddedTypesManagerOpt.EmbeddedTypesMap.Keys)
		{
			diagnostics.Add(new CSDiagnosticInfo(ErrorCode.ERR_EncNoPIAReference, key.AdaptedSymbol), Location.None);
		}
	}

	public override INamedTypeSymbolInternal? TryGetOrCreateSynthesizedHotReloadExceptionType()
	{
		if ((object)_predefinedHotReloadExceptionConstructor != null)
		{
			return null;
		}
		return GetOrCreateSynthesizedHotReloadExceptionType();
	}

	public override IMethodSymbolInternal GetOrCreateHotReloadExceptionConstructorDefinition()
	{
		if ((object)_predefinedHotReloadExceptionConstructor != null)
		{
			return _predefinedHotReloadExceptionConstructor;
		}
		if (_freezeHotReloadExceptionTypeUsage)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Emitter/EditAndContinue/PEDeltaAssemblyBuilder.cs", 288);
		}
		_isHotReloadExceptionTypeUsed = true;
		return GetOrCreateSynthesizedHotReloadExceptionType().Constructor;
	}

	public override INamedTypeSymbolInternal? GetUsedSynthesizedHotReloadExceptionType()
	{
		_freezeHotReloadExceptionTypeUsage = true;
		if (!_isHotReloadExceptionTypeUsed)
		{
			return null;
		}
		return _lazyHotReloadExceptionType;
	}

	private SynthesizedHotReloadExceptionSymbol GetOrCreateSynthesizedHotReloadExceptionType()
	{
		SynthesizedHotReloadExceptionSymbol lazyHotReloadExceptionType = _lazyHotReloadExceptionType;
		if ((object)lazyHotReloadExceptionType != null)
		{
			return lazyHotReloadExceptionType;
		}
		NamedTypeSymbol wellKnownType = Compilation.GetWellKnownType(WellKnownType.System_Exception);
		NamedTypeSymbol wellKnownType2 = Compilation.GetWellKnownType(WellKnownType.System_Action_T);
		NamedTypeSymbol specialType = Compilation.GetSpecialType(SpecialType.System_String);
		NamedTypeSymbol specialType2 = Compilation.GetSpecialType(SpecialType.System_Int32);
		lazyHotReloadExceptionType = new SynthesizedHotReloadExceptionSymbol(GetOrSynthesizeNamespace("System.Runtime.CompilerServices"), wellKnownType, wellKnownType2, specialType, specialType2);
		Interlocked.CompareExchange(ref _lazyHotReloadExceptionType, lazyHotReloadExceptionType, null);
		return _lazyHotReloadExceptionType;
	}
}
