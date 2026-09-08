using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class DictionaryAnalyzerConfigOptions : AnalyzerConfigOptions
{
	internal static readonly ImmutableDictionary<string, string> EmptyDictionary = ImmutableDictionary.Create<string, string>(AnalyzerConfigOptions.KeyComparer);

	internal readonly ImmutableDictionary<string, string> Options;

	public static DictionaryAnalyzerConfigOptions Empty { get; } = new DictionaryAnalyzerConfigOptions(EmptyDictionary);

	public override IEnumerable<string> Keys => Options.Keys;

	public DictionaryAnalyzerConfigOptions(ImmutableDictionary<string, string> options)
	{
		Options = options;
	}

	public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
	{
		return Options.TryGetValue(key, out value);
	}
}
