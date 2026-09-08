using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection.Metadata;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal static class EmitHelpers
{
	internal static EmitDifferenceResult EmitDifference(CSharpCompilation compilation, EmitBaseline baseline, IEnumerable<SemanticEdit> edits, Func<ISymbol, bool> isAddedSymbol, Stream metadataStream, Stream ilStream, Stream pdbStream, EmitDifferenceOptions options, CompilationTestData? testData, CancellationToken cancellationToken)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		EmitOptions emitOptions = EmitOptions.Default.WithDebugInformationFormat((!baseline.HasPortablePdb) ? DebugInformationFormat.Pdb : DebugInformationFormat.PortablePdb);
		string runtimeMetadataVersion = compilation.GetRuntimeMetadataVersion(emitOptions, instance);
		ModulePropertiesForSerialization serializationProperties = compilation.ConstructModuleSerializationProperties(emitOptions, runtimeMetadataVersion, baseline.ModuleVersionId);
		IEnumerable<ResourceDescription> manifestResources = SpecializedCollections.EmptyEnumerable<ResourceDescription>();
		if (!GetPredefinedHotReloadExceptionTypeConstructor(compilation, instance, out MethodSymbol constructor))
		{
			return new EmitDifferenceResult(success: false, instance.ToReadOnlyAndFree(), null, ImmutableArray<MethodDefinitionHandle>.Empty, ImmutableArray<TypeDefinitionHandle>.Empty);
		}
		CSharpDefinitionMap definitionMap;
		CSharpSymbolChanges changes;
		PEDeltaAssemblyBuilder pEDeltaAssemblyBuilder;
		try
		{
			SourceAssemblySymbol sourceAssembly = compilation.SourceAssembly;
			EmitBaseline initialBaseline = baseline.InitialBaseline;
			SourceAssemblySymbol sourceAssembly2 = ((CSharpCompilation)baseline.Compilation).SourceAssembly;
			EmitBaseline.MetadataSymbols orCreateMetadataSymbols = PEDeltaAssemblyBuilder.GetOrCreateMetadataSymbols(initialBaseline, sourceAssembly.DeclaringCompilation);
			MetadataDecoder metadataDecoder = (MetadataDecoder)orCreateMetadataSymbols.MetadataDecoder;
			PEAssemblySymbol otherAssembly = (PEAssemblySymbol)metadataDecoder.ModuleSymbol.ContainingAssembly;
			CSharpSymbolMatcher sourceToMetadata = new CSharpSymbolMatcher(sourceAssembly, otherAssembly, orCreateMetadataSymbols.SynthesizedTypes);
			CSharpSymbolMatcher previousSourceToMetadata = new CSharpSymbolMatcher(sourceAssembly2, otherAssembly, orCreateMetadataSymbols.SynthesizedTypes);
			CSharpSymbolMatcher sourceToPreviousSource = null;
			if (baseline.Ordinal > 0)
			{
				sourceToPreviousSource = new CSharpSymbolMatcher(sourceAssembly, sourceAssembly2, baseline.SynthesizedTypes, baseline.SynthesizedMembers, baseline.DeletedMembers);
			}
			definitionMap = new CSharpDefinitionMap(edits, metadataDecoder, previousSourceToMetadata, sourceToMetadata, sourceToPreviousSource, baseline);
			changes = new CSharpSymbolChanges(definitionMap, edits, isAddedSymbol);
			pEDeltaAssemblyBuilder = new PEDeltaAssemblyBuilder(compilation.SourceAssembly, changes, emitOptions, options, compilation.Options.OutputKind, serializationProperties, manifestResources, constructor);
		}
		catch (NotSupportedException ex)
		{
			instance.Add(ErrorCode.ERR_ModuleEmitFailure, NoLocation.Singleton, compilation.AssemblyName, ex.Message);
			return new EmitDifferenceResult(success: false, instance.ToReadOnlyAndFree(), null, ImmutableArray<MethodDefinitionHandle>.Empty, ImmutableArray<TypeDefinitionHandle>.Empty);
		}
		if (testData != null)
		{
			pEDeltaAssemblyBuilder.SetTestData(testData);
		}
		EmitBaseline emitBaseline = null;
		ArrayBuilder<MethodDefinitionHandle> instance2 = ArrayBuilder<MethodDefinitionHandle>.GetInstance();
		ArrayBuilder<TypeDefinitionHandle> instance3 = ArrayBuilder<TypeDefinitionHandle>.GetInstance();
		if (compilation.Compile(pEDeltaAssemblyBuilder, emittingPdb: true, instance, (ISymbolInternal s) => changes.RequiresCompilation(s), cancellationToken))
		{
			emitBaseline = compilation.SerializeToDeltaStreams(pEDeltaAssemblyBuilder, definitionMap, metadataStream, ilStream, pdbStream, instance2, instance3, instance, testData?.SymWriterFactory, emitOptions.PdbFilePath, cancellationToken);
		}
		return new EmitDifferenceResult(emitBaseline != null, instance.ToReadOnlyAndFree(), emitBaseline, instance2.ToImmutableAndFree(), instance3.ToImmutableAndFree());
	}

	private static bool GetPredefinedHotReloadExceptionTypeConstructor(CSharpCompilation compilation, DiagnosticBag diagnostics, out MethodSymbol? constructor)
	{
		constructor = compilation.GetWellKnownTypeMember(WellKnownMember.System_Runtime_CompilerServices_HotReloadException__ctorStringInt32) as MethodSymbol;
		if ((object)constructor != null)
		{
			return true;
		}
		NamedTypeSymbol wellKnownType = compilation.GetWellKnownType(WellKnownType.System_Runtime_CompilerServices_HotReloadException);
		if (wellKnownType.Kind == SymbolKind.ErrorType)
		{
			return true;
		}
		diagnostics.Add(ErrorCode.ERR_ModuleEmitFailure, NoLocation.Singleton, compilation.AssemblyName, string.Format(CodeAnalysisResources.Type0DoesNotHaveExpectedConstructor, wellKnownType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
		return false;
	}
}
