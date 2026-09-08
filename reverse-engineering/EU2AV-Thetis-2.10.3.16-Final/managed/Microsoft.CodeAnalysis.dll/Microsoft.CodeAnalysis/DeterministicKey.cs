using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis;

internal static class DeterministicKey
{
	public static string GetDeterministicKey(CompilationOptions compilationOptions, ImmutableArray<SyntaxTree> syntaxTrees, ImmutableArray<MetadataReference> references, ImmutableArray<byte> publicKey = default(ImmutableArray<byte>), ImmutableArray<AdditionalText> additionalTexts = default(ImmutableArray<AdditionalText>), ImmutableArray<DiagnosticAnalyzer> analyzers = default(ImmutableArray<DiagnosticAnalyzer>), ImmutableArray<ISourceGenerator> generators = default(ImmutableArray<ISourceGenerator>), ImmutableArray<KeyValuePair<string, string>> pathMap = default(ImmutableArray<KeyValuePair<string, string>>), EmitOptions? emitOptions = null, DeterministicKeyOptions options = DeterministicKeyOptions.Default, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDeterministicKey(compilationOptions, syntaxTrees.SelectAsArray((SyntaxTree t) => SyntaxTreeKey.Create(t)), references, publicKey, additionalTexts, analyzers, generators, pathMap, emitOptions, options, cancellationToken);
	}

	public static string GetDeterministicKey(CompilationOptions compilationOptions, ImmutableArray<SyntaxTreeKey> syntaxTrees, ImmutableArray<MetadataReference> references, ImmutableArray<byte> publicKey, ImmutableArray<AdditionalText> additionalTexts = default(ImmutableArray<AdditionalText>), ImmutableArray<DiagnosticAnalyzer> analyzers = default(ImmutableArray<DiagnosticAnalyzer>), ImmutableArray<ISourceGenerator> generators = default(ImmutableArray<ISourceGenerator>), ImmutableArray<KeyValuePair<string, string>> pathMap = default(ImmutableArray<KeyValuePair<string, string>>), EmitOptions? emitOptions = null, DeterministicKeyOptions options = DeterministicKeyOptions.Default, CancellationToken cancellationToken = default(CancellationToken))
	{
		return compilationOptions.CreateDeterministicKeyBuilder().GetKey(compilationOptions, syntaxTrees, references, publicKey, additionalTexts.NullToEmpty(), analyzers.NullToEmpty(), generators.NullToEmpty(), pathMap.NullToEmpty(), emitOptions, options, cancellationToken);
	}
}
