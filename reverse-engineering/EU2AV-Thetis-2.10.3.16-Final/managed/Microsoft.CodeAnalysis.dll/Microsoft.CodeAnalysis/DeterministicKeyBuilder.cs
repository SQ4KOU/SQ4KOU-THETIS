using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal abstract class DeterministicKeyBuilder
{
	protected void WriteFilePath(JsonWriter writer, string propertyName, string? filePath, ImmutableArray<KeyValuePair<string, string>> pathMap, DeterministicKeyOptions options)
	{
		if ((options & DeterministicKeyOptions.IgnorePaths) != DeterministicKeyOptions.Default)
		{
			filePath = Path.GetFileName(filePath);
		}
		else if (filePath != null)
		{
			filePath = PathUtilities.NormalizePathPrefix(filePath, pathMap);
		}
		writer.Write(propertyName, filePath);
	}

	internal static string EncodeByteArrayValue(ReadOnlySpan<byte> value)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		EncodeByteArrayValue(value, instance.Builder);
		return instance.ToStringAndFree();
	}

	internal static void EncodeByteArrayValue(ReadOnlySpan<byte> value, StringBuilder builder)
	{
		ReadOnlySpan<byte> readOnlySpan = value;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			byte b = readOnlySpan[i];
			builder.Append(b.ToString("x"));
		}
	}

	protected static void WriteByteArrayValue(JsonWriter writer, string name, ReadOnlySpan<byte> value)
	{
		writer.Write(name, EncodeByteArrayValue(value));
	}

	protected static void WriteVersion(JsonWriter writer, string key, Version version)
	{
		writer.WriteKey(key);
		writer.WriteObjectStart();
		writer.Write("major", version.Major);
		writer.Write("minor", version.Minor);
		writer.Write("build", version.Build);
		writer.Write("revision", version.Revision);
		writer.WriteObjectEnd();
	}

	protected void WriteType(JsonWriter writer, string key, Type? type)
	{
		writer.WriteKey(key);
		WriteType(writer, type);
	}

	protected void WriteType(JsonWriter writer, Type? type)
	{
		if ((object)type == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectStart();
		writer.Write("fullName", type.FullName);
		writer.Write("assemblyName", type.Assembly.FullName);
		writer.Write("mvid", GetGuidValue(type.Assembly.ManifestModule.ModuleVersionId));
		writer.WriteObjectEnd();
	}

	private (JsonWriter, PooledStringBuilder) CreateWriter()
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		return (new JsonWriter(new StringWriter(instance)), instance);
	}

	internal string GetKey(CompilationOptions compilationOptions, ImmutableArray<SyntaxTreeKey> syntaxTrees, ImmutableArray<MetadataReference> references, ImmutableArray<byte> publicKey, ImmutableArray<AdditionalText> additionalTexts, ImmutableArray<DiagnosticAnalyzer> analyzers, ImmutableArray<ISourceGenerator> generators, ImmutableArray<KeyValuePair<string, string>> pathMap, EmitOptions? emitOptions, DeterministicKeyOptions options, CancellationToken cancellationToken)
	{
		additionalTexts = additionalTexts.NullToEmpty();
		analyzers = analyzers.NullToEmpty();
		generators = generators.NullToEmpty();
		var (writer, pooledStringBuilder) = CreateWriter();
		writer.WriteObjectStart();
		writer.WriteKey("compilation");
		WriteCompilation(writer, compilationOptions, syntaxTrees, references, publicKey, pathMap, options, cancellationToken);
		writer.WriteKey("additionalTexts");
		writeAdditionalTexts();
		writer.WriteKey("analyzers");
		writeAnalyzers();
		writer.WriteKey("generators");
		writeGenerators();
		writer.WriteKey("emitOptions");
		WriteEmitOptions(writer, emitOptions, pathMap, options);
		writer.WriteObjectEnd();
		return pooledStringBuilder.ToStringAndFree();
		void writeAdditionalTexts()
		{
			writer.WriteArrayStart();
			foreach (AdditionalText item in additionalTexts)
			{
				cancellationToken.ThrowIfCancellationRequested();
				writer.WriteObjectStart();
				WriteFilePath(writer, "fileName", item.Path, pathMap, options);
				writer.WriteKey("text");
				WriteSourceText(writer, item.GetText(cancellationToken));
				writer.WriteObjectEnd();
			}
			writer.WriteArrayEnd();
		}
		void writeAnalyzers()
		{
			writer.WriteArrayStart();
			foreach (DiagnosticAnalyzer item2 in analyzers)
			{
				cancellationToken.ThrowIfCancellationRequested();
				WriteType(writer, item2.GetType());
			}
			writer.WriteArrayEnd();
		}
		void writeGenerators()
		{
			writer.WriteArrayStart();
			foreach (ISourceGenerator item3 in generators)
			{
				cancellationToken.ThrowIfCancellationRequested();
				WriteType(writer, item3.GetGeneratorType());
			}
			writer.WriteArrayEnd();
		}
	}

	internal static string GetGuidValue(in Guid guid)
	{
		Guid guid2 = guid;
		return guid2.ToString("D");
	}

	private void WriteCompilation(JsonWriter writer, CompilationOptions compilationOptions, ImmutableArray<SyntaxTreeKey> syntaxTrees, ImmutableArray<MetadataReference> references, ImmutableArray<byte> publicKey, ImmutableArray<KeyValuePair<string, string>> pathMap, DeterministicKeyOptions options, CancellationToken cancellationToken)
	{
		writer.WriteObjectStart();
		writeToolsVersions();
		WriteByteArrayValue(writer, "publicKey", publicKey.AsSpan());
		writer.WriteKey("options");
		WriteCompilationOptions(writer, compilationOptions);
		writer.WriteKey("syntaxTrees");
		writer.WriteArrayStart();
		foreach (SyntaxTreeKey item in syntaxTrees)
		{
			cancellationToken.ThrowIfCancellationRequested();
			WriteSyntaxTree(writer, item, pathMap, options, cancellationToken);
		}
		writer.WriteArrayEnd();
		writer.WriteKey("references");
		writer.WriteArrayStart();
		foreach (MetadataReference item2 in references)
		{
			cancellationToken.ThrowIfCancellationRequested();
			WriteMetadataReference(writer, item2, pathMap, options, cancellationToken);
		}
		writer.WriteArrayEnd();
		writer.WriteObjectEnd();
		void writeToolsVersions()
		{
			writer.WriteKey("toolsVersions");
			writer.WriteObjectStart();
			if ((options & DeterministicKeyOptions.IgnoreToolVersions) == 0)
			{
				string value = typeof(Compilation).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
				writer.Write("compilerVersion", value);
				string value2 = typeof(object).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
				writer.Write("runtimeVersion", value2);
				writer.Write("frameworkDescription", RuntimeInformation.FrameworkDescription);
				writer.Write("osDescription", RuntimeInformation.OSDescription);
			}
			writer.WriteObjectEnd();
		}
	}

	private void WriteSyntaxTree(JsonWriter writer, SyntaxTreeKey syntaxTree, ImmutableArray<KeyValuePair<string, string>> pathMap, DeterministicKeyOptions options, CancellationToken cancellationToken)
	{
		writer.WriteObjectStart();
		WriteFilePath(writer, "fileName", syntaxTree.FilePath, pathMap, options);
		writer.WriteKey("text");
		WriteSourceText(writer, syntaxTree.GetText(cancellationToken));
		writer.WriteKey("parseOptions");
		WriteParseOptions(writer, syntaxTree.Options);
		writer.WriteObjectEnd();
	}

	private void WriteSourceText(JsonWriter writer, SourceText? sourceText)
	{
		if (sourceText == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectStart();
		WriteByteArrayValue(writer, "checksum", sourceText.GetChecksum().AsSpan());
		writer.Write("checksumAlgorithm", sourceText.ChecksumAlgorithm);
		writer.Write("encodingName", sourceText.Encoding?.EncodingName);
		writer.WriteObjectEnd();
	}

	internal void WriteMetadataReference(JsonWriter writer, MetadataReference reference, ImmutableArray<KeyValuePair<string, string>> pathMap, DeterministicKeyOptions deterministicKeyOptions, CancellationToken cancellationToken)
	{
		writer.WriteObjectStart();
		if (reference is PortableExecutableReference portableExecutableReference)
		{
			Metadata metadata = portableExecutableReference.GetMetadata();
			if (!(metadata is AssemblyMetadata assemblyMetadata))
			{
				if (!(metadata is ModuleMetadata moduleMetadata))
				{
					throw ExceptionUtilities.UnexpectedValue(metadata);
				}
				writeModuleMetadata(moduleMetadata);
			}
			else
			{
				ImmutableArray<ModuleMetadata> modules = assemblyMetadata.GetModules();
				writeModuleMetadata(modules[0]);
				writer.WriteKey("secondaryModules");
				writer.WriteArrayStart();
				for (int i = 1; i < modules.Length; i++)
				{
					writer.WriteObjectStart();
					writeModuleMetadata(modules[i]);
					writer.WriteObjectEnd();
				}
				writer.WriteArrayEnd();
			}
			writer.WriteKey("properties");
			writeMetadataReferenceProperties(writer, reference.Properties);
		}
		else
		{
			if (!(reference is CompilationReference compilationReference))
			{
				throw ExceptionUtilities.UnexpectedValue(reference);
			}
			writer.WriteKey("compilation");
			Compilation compilation = compilationReference.Compilation;
			compilation.Options.CreateDeterministicKeyBuilder().WriteCompilation(writer, compilation.Options, compilation.SyntaxTrees.SelectAsArray((SyntaxTree x) => SyntaxTreeKey.Create(x)), compilation.References.AsImmutable(), compilation.Assembly.Identity.PublicKey, pathMap, deterministicKeyOptions, cancellationToken);
		}
		writer.WriteObjectEnd();
		static void writeMetadataReferenceProperties(JsonWriter jsonWriter, MetadataReferenceProperties properties)
		{
			jsonWriter.WriteObjectStart();
			jsonWriter.Write("kind", properties.Kind);
			jsonWriter.Write("embedInteropTypes", properties.EmbedInteropTypes);
			jsonWriter.WriteKey("aliases");
			jsonWriter.WriteArrayStart();
			foreach (string alias in properties.Aliases)
			{
				jsonWriter.Write(alias);
			}
			jsonWriter.WriteArrayEnd();
			jsonWriter.WriteObjectEnd();
		}
		void writeModuleMetadata(ModuleMetadata moduleMetadata2)
		{
			MetadataReader metadataReader = moduleMetadata2.GetMetadataReader();
			if (metadataReader.IsAssembly)
			{
				AssemblyDefinition assemblyDefinition = metadataReader.GetAssemblyDefinition();
				writer.Write("name", metadataReader.GetString(assemblyDefinition.Name));
				WriteVersion(writer, "version", assemblyDefinition.Version);
				WriteByteArrayValue(writer, "publicKey", System.MemoryExtensions.AsSpan(metadataReader.GetBlobBytes(assemblyDefinition.PublicKey)));
			}
			else
			{
				ModuleDefinition moduleDefinition = metadataReader.GetModuleDefinition();
				writer.Write("name", metadataReader.GetString(moduleDefinition.Name));
			}
			writer.Write("mvid", GetGuidValue(moduleMetadata2.GetModuleVersionId()));
		}
	}

	private void WriteEmitOptions(JsonWriter writer, EmitOptions? options, ImmutableArray<KeyValuePair<string, string>> pathMap, DeterministicKeyOptions deterministicKeyOptions)
	{
		if ((object)options == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectStart();
		writer.Write("emitMetadataOnly", options.EmitMetadataOnly);
		writer.Write("tolerateErrors", options.TolerateErrors);
		writer.Write("includePrivateMembers", options.IncludePrivateMembers);
		writer.WriteKey("instrumentationKinds");
		writer.WriteArrayStart();
		if (!options.InstrumentationKinds.IsDefault)
		{
			foreach (InstrumentationKind instrumentationKind in options.InstrumentationKinds)
			{
				writer.Write(instrumentationKind);
			}
		}
		writer.WriteArrayEnd();
		writeSubsystemVersion(writer, options.SubsystemVersion);
		writer.Write("fileAlignment", options.FileAlignment);
		writer.Write("highEntropyVirtualAddressSpace", options.HighEntropyVirtualAddressSpace);
		writer.WriteInvariant("baseAddress", options.BaseAddress);
		writer.Write("debugInformationFormat", options.DebugInformationFormat);
		writer.Write("outputNameOverride", options.OutputNameOverride);
		WriteFilePath(writer, "pdbFilePath", options.PdbFilePath, pathMap, deterministicKeyOptions);
		writer.Write("pdbChecksumAlgorithm", options.PdbChecksumAlgorithm.Name);
		writer.Write("runtimeMetadataVersion", options.RuntimeMetadataVersion);
		writer.Write("defaultSourceFileEncoding", options.DefaultSourceFileEncoding?.CodePage);
		writer.Write("fallbackSourceFileEncoding", options.FallbackSourceFileEncoding?.CodePage);
		writer.WriteObjectEnd();
		static void writeSubsystemVersion(JsonWriter jsonWriter, SubsystemVersion version)
		{
			jsonWriter.WriteKey("subsystemVersion");
			jsonWriter.WriteObjectStart();
			jsonWriter.Write("major", version.Major);
			jsonWriter.Write("minor", version.Minor);
			jsonWriter.WriteObjectEnd();
		}
	}

	private void WriteCompilationOptions(JsonWriter writer, CompilationOptions options)
	{
		writer.WriteObjectStart();
		WriteCompilationOptionsCore(writer, options);
		writer.WriteObjectEnd();
	}

	protected virtual void WriteCompilationOptionsCore(JsonWriter writer, CompilationOptions options)
	{
		writer.Write("outputKind", options.OutputKind);
		writer.Write("moduleName", options.ModuleName);
		writer.Write("scriptClassName", options.ScriptClassName);
		writer.Write("mainTypeName", options.MainTypeName);
		WriteByteArrayValue(writer, "cryptoPublicKey", options.CryptoPublicKey.AsSpan());
		writer.Write("cryptoKeyFile", options.CryptoKeyFile);
		writer.Write("delaySign", options.DelaySign);
		writer.Write("publicSign", options.PublicSign);
		writer.Write("checkOverflow", options.CheckOverflow);
		writer.Write("platform", options.Platform);
		writer.Write("optimizationLevel", options.OptimizationLevel);
		writer.Write("generalDiagnosticOption", options.GeneralDiagnosticOption);
		writer.Write("warningLevel", options.WarningLevel);
		writer.Write("deterministic", options.Deterministic);
		writer.Write("debugPlusMode", options.DebugPlusMode);
		writer.Write("referencesSupersedeLowerVersions", options.ReferencesSupersedeLowerVersions);
		writer.Write("reportSuppressedDiagnostics", options.ReportSuppressedDiagnostics);
		writer.Write("nullableContextOptions", options.NullableContextOptions);
		writer.WriteKey("specificDiagnosticOptions");
		writer.WriteArrayStart();
		foreach (string item in options.SpecificDiagnosticOptions.Keys.OrderBy((IComparer<string>?)StringComparer.Ordinal))
		{
			writer.WriteObjectStart();
			writer.Write(item, options.SpecificDiagnosticOptions[item]);
			writer.WriteObjectEnd();
		}
		writer.WriteArrayEnd();
		if (options.Deterministic)
		{
			writer.Write("deterministic", value: true);
			writer.WriteNull("localtime");
		}
		else
		{
			writer.Write("deterministic", value: false);
			writer.WriteInvariant("localtime", options.CurrentLocalTime);
			writer.Write("nondeterministicMvid", GetGuidValue(Guid.NewGuid()));
		}
		writer.WriteKey("extensions");
		writer.WriteObjectStart();
		WriteType(writer, "syntaxTreeOptionsProvider", options.SyntaxTreeOptionsProvider?.GetType());
		WriteType(writer, "metadataReferenceResolver", options.MetadataReferenceResolver?.GetType());
		WriteType(writer, "xmlReferenceResolver", options.XmlReferenceResolver?.GetType());
		WriteType(writer, "sourceReferenceResolver", options.SourceReferenceResolver?.GetType());
		WriteType(writer, "strongNameProvider", options.StrongNameProvider?.GetType());
		WriteType(writer, "assemblyIdentityComparer", options.AssemblyIdentityComparer?.GetType());
		writer.WriteObjectEnd();
	}

	protected void WriteParseOptions(JsonWriter writer, ParseOptions parseOptions)
	{
		writer.WriteObjectStart();
		WriteParseOptionsCore(writer, parseOptions);
		writer.WriteObjectEnd();
	}

	protected virtual void WriteParseOptionsCore(JsonWriter writer, ParseOptions parseOptions)
	{
		writer.Write("kind", parseOptions.Kind);
		writer.Write("specifiedKind", parseOptions.SpecifiedKind);
		writer.Write("documentationMode", parseOptions.DocumentationMode);
		writer.Write("language", parseOptions.Language);
		writer.WriteKey("features");
		IReadOnlyDictionary<string, string> features = parseOptions.Features;
		writer.WriteObjectStart();
		foreach (string item in features.Keys.OrderBy((IComparer<string>?)StringComparer.Ordinal))
		{
			writer.Write(item, features[item]);
		}
		writer.WriteObjectEnd();
	}
}
