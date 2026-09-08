using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.InternalUtilities;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal static class AnalyzerOptionsExtensions
{
	private const string DotnetAnalyzerDiagnosticPrefix = "dotnet_analyzer_diagnostic";

	private const string CategoryPrefix = "category";

	private const string SeveritySuffix = "severity";

	private const string DotnetAnalyzerDiagnosticSeverityKey = "dotnet_analyzer_diagnostic.severity";

	private static readonly ConcurrentLruCache<string, string> s_categoryToSeverityKeyMap = new ConcurrentLruCache<string, string>(50);

	private static string GetCategoryBasedDotnetAnalyzerDiagnosticSeverityKey(string category)
	{
		return s_categoryToSeverityKeyMap.GetOrAdd(category, category, (string text) => "dotnet_analyzer_diagnostic.category-" + text + ".severity");
	}

	public static ImmutableArray<AdditionalText> GetAdditionalFiles(this AnalyzerOptions? analyzerOptions)
	{
		return analyzerOptions?.AdditionalFiles ?? ImmutableArray<AdditionalText>.Empty;
	}

	public static bool TryGetSeverityFromBulkConfiguration(this AnalyzerOptions? analyzerOptions, SyntaxTree tree, Compilation compilation, DiagnosticDescriptor descriptor, CancellationToken cancellationToken, out ReportDiagnostic severity)
	{
		if (analyzerOptions == null || !descriptor.IsEnabledByDefault || descriptor.IsCompilerOrNotConfigurableOrCustomConfigurable())
		{
			severity = ReportDiagnostic.Default;
			return false;
		}
		if (!compilation.Options.SpecificDiagnosticOptions.ContainsKey(descriptor.Id))
		{
			SyntaxTreeOptionsProvider? syntaxTreeOptionsProvider = compilation.Options.SyntaxTreeOptionsProvider;
			if (syntaxTreeOptionsProvider == null || !syntaxTreeOptionsProvider.TryGetDiagnosticValue(tree, descriptor.Id, cancellationToken, out var severity2))
			{
				SyntaxTreeOptionsProvider? syntaxTreeOptionsProvider2 = compilation.Options.SyntaxTreeOptionsProvider;
				if (syntaxTreeOptionsProvider2 == null || !syntaxTreeOptionsProvider2.TryGetGlobalDiagnosticValue(descriptor.Id, cancellationToken, out severity2))
				{
					AnalyzerConfigOptions options = analyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(tree);
					string categoryBasedDotnetAnalyzerDiagnosticSeverityKey = GetCategoryBasedDotnetAnalyzerDiagnosticSeverityKey(descriptor.Category);
					if (options.TryGetValue(categoryBasedDotnetAnalyzerDiagnosticSeverityKey, out string value) && AnalyzerConfigSet.TryParseSeverity(value, out severity))
					{
						if (severity == ReportDiagnostic.Warn && compilation.Options.GeneralDiagnosticOption == ReportDiagnostic.Error)
						{
							severity = ReportDiagnostic.Error;
						}
						return true;
					}
					if (options.TryGetValue("dotnet_analyzer_diagnostic.severity", out value) && AnalyzerConfigSet.TryParseSeverity(value, out severity))
					{
						if (severity == ReportDiagnostic.Warn && compilation.Options.GeneralDiagnosticOption == ReportDiagnostic.Error)
						{
							severity = ReportDiagnostic.Error;
						}
						return true;
					}
					severity = ReportDiagnostic.Default;
					return false;
				}
			}
		}
		severity = ReportDiagnostic.Default;
		return false;
	}
}
