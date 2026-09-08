using System;
using System.Globalization;
using System.IO;

namespace Microsoft.CodeAnalysis;

internal sealed class ProgramFilesAnalyzerPathResolver : IAnalyzerPathResolver
{
	internal static readonly IAnalyzerPathResolver Instance = new ProgramFilesAnalyzerPathResolver();

	private string DotNetPath { get; }

	private ProgramFilesAnalyzerPathResolver()
	{
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
		DotNetPath = Path.Combine(folderPath, "dotnet");
	}

	public bool IsAnalyzerPathHandled(string analyzerPath)
	{
		return analyzerPath.StartsWith(DotNetPath, StringComparison.OrdinalIgnoreCase);
	}

	public string GetResolvedAnalyzerPath(string originalAnalyzerPath)
	{
		return originalAnalyzerPath;
	}

	public string? GetResolvedSatellitePath(string originalAnalyzerPath, CultureInfo cultureInfo)
	{
		return AnalyzerAssemblyLoader.GetSatelliteAssemblyPath(originalAnalyzerPath, cultureInfo);
	}
}
