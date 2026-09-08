using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeAnalysis;

internal static class SharedInputNodes
{
	public static readonly InputNode<Compilation> Compilation = new InputNode<Compilation>((DriverStateTable.Builder b) => ImmutableArray.Create(b.Compilation));

	public static readonly InputNode<CompilationOptions> CompilationOptions = new InputNode<CompilationOptions>((DriverStateTable.Builder b) => ImmutableArray.Create(b.Compilation.Options), ReferenceEqualityComparer.Instance);

	public static readonly InputNode<ParseOptions> ParseOptions = new InputNode<ParseOptions>((DriverStateTable.Builder b) => ImmutableArray.Create(b.DriverState.ParseOptions));

	public static readonly InputNode<AdditionalText> AdditionalTexts = new InputNode<AdditionalText>((DriverStateTable.Builder b) => b.DriverState.AdditionalTexts);

	public static readonly InputNode<SyntaxTree> SyntaxTrees = new InputNode<SyntaxTree>((DriverStateTable.Builder b) => b.Compilation.SyntaxTrees.ToImmutableArray());

	public static readonly InputNode<AnalyzerConfigOptionsProvider> AnalyzerConfigOptions = new InputNode<AnalyzerConfigOptionsProvider>((DriverStateTable.Builder b) => ImmutableArray.Create(b.DriverState.OptionsProvider));

	public static readonly InputNode<MetadataReference> MetadataReferences = new InputNode<MetadataReference>((DriverStateTable.Builder b) => b.Compilation.ExternalReferences);
}
