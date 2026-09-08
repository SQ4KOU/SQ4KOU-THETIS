using System.Globalization;

namespace Microsoft.CodeAnalysis;

internal interface IAnalyzerPathResolver
{
	bool IsAnalyzerPathHandled(string analyzerPath);

	string GetResolvedAnalyzerPath(string originalAnalyzerPath);

	string? GetResolvedSatellitePath(string originalAnalyzerPath, CultureInfo cultureInfo);
}
