using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeAnalysis;

internal static class ReportAnalyzerUtil
{
	public static void Report(TextWriter consoleOutput, AnalyzerDriver? analyzerDriver, GeneratorDriverTimingInfo? driverTimingInfo, CultureInfo culture, bool isConcurrentBuild)
	{
		if (isConcurrentBuild && (analyzerDriver != null || driverTimingInfo.HasValue))
		{
			consoleOutput.WriteLine(CodeAnalysisResources.MultithreadedAnalyzerExecutionNote);
			consoleOutput.WriteLine();
		}
		if (analyzerDriver != null)
		{
			ReportAnalyzerExecutionTime(consoleOutput, analyzerDriver, culture);
		}
		if (driverTimingInfo.HasValue)
		{
			GeneratorDriverTimingInfo valueOrDefault = driverTimingInfo.GetValueOrDefault();
			ReportGeneratorExecutionTime(consoleOutput, valueOrDefault, culture);
		}
	}

	public static string GetFormattedAnalyzerExecutionTime(double executionTime, CultureInfo culture)
	{
		if (!(executionTime < 0.001))
		{
			return string.Format(culture, "{0,8:##0.000}", executionTime);
		}
		return string.Format(culture, "{0,8:<0.000}", 0.001);
	}

	public static string GetFormattedAnalyzerExecutionPercentage(int percentage, CultureInfo culture)
	{
		return string.Format("{0,5}", (percentage < 1) ? "<1" : percentage.ToString(culture));
	}

	private static string GetColumnHeader(string kind)
	{
		string text = $"{CodeAnalysisResources.AnalyzerExecutionTimeColumnHeader,8}";
		string text2 = string.Format("{0,5}", "%");
		return text + text2 + "   " + kind;
	}

	private static string GetColumnEntry(double totalSeconds, int percentage, string? name, CultureInfo culture)
	{
		string formattedAnalyzerExecutionTime = GetFormattedAnalyzerExecutionTime(totalSeconds, culture);
		string formattedAnalyzerExecutionPercentage = GetFormattedAnalyzerExecutionPercentage(percentage, culture);
		return formattedAnalyzerExecutionTime + formattedAnalyzerExecutionPercentage + "   " + name;
	}

	private static void ReportAnalyzerExecutionTime(TextWriter consoleOutput, AnalyzerDriver analyzerDriver, CultureInfo culture)
	{
		if (analyzerDriver.AnalyzerExecutionTimes.IsEmpty)
		{
			return;
		}
		double num = analyzerDriver.AnalyzerExecutionTimes.Sum<KeyValuePair<DiagnosticAnalyzer, TimeSpan>>((KeyValuePair<DiagnosticAnalyzer, TimeSpan> kvp) => kvp.Value.TotalSeconds);
		consoleOutput.WriteLine(string.Format(CodeAnalysisResources.AnalyzerTotalExecutionTime, num.ToString("##0.000", culture)));
		consoleOutput.WriteLine();
		consoleOutput.WriteLine(GetColumnHeader(CodeAnalysisResources.AnalyzerNameColumnHeader));
		foreach (IGrouping<Assembly, KeyValuePair<DiagnosticAnalyzer, TimeSpan>> item in from kvp in analyzerDriver.AnalyzerExecutionTimes
			group kvp by kvp.Key.GetType().Assembly into kvp
			orderby kvp.Sum((KeyValuePair<DiagnosticAnalyzer, TimeSpan> entry) => entry.Value.Ticks) descending
			select kvp)
		{
			double num2 = item.Sum((KeyValuePair<DiagnosticAnalyzer, TimeSpan> kvp) => kvp.Value.TotalSeconds);
			int percentage = (int)(num2 * 100.0 / num);
			consoleOutput.WriteLine(GetColumnEntry(num2, percentage, item.Key.FullName, culture));
			foreach (KeyValuePair<DiagnosticAnalyzer, TimeSpan> item2 in item.OrderByDescending((KeyValuePair<DiagnosticAnalyzer, TimeSpan> kvp) => kvp.Value))
			{
				num2 = item2.Value.TotalSeconds;
				percentage = (int)(num2 * 100.0 / num);
				string arg = string.Join(", ", from id in GetSupportedIds(item2.Key).Distinct()
					orderby id
					select id);
				string name = $"   {item2.Key} ({arg})";
				consoleOutput.WriteLine(GetColumnEntry(num2, percentage, name, culture));
			}
			consoleOutput.WriteLine();
		}
	}

	private static IEnumerable<string> GetSupportedIds(DiagnosticAnalyzer analyzer)
	{
		if (analyzer is DiagnosticSuppressor diagnosticSuppressor)
		{
			return diagnosticSuppressor.SupportedSuppressions.Select((SuppressionDescriptor s) => s.Id);
		}
		return analyzer.SupportedDiagnostics.Select((DiagnosticDescriptor d) => d.Id);
	}

	private static void ReportGeneratorExecutionTime(TextWriter consoleOutput, GeneratorDriverTimingInfo driverTimingInfo, CultureInfo culture)
	{
		if (driverTimingInfo.GeneratorTimes.IsEmpty)
		{
			return;
		}
		double totalSeconds = driverTimingInfo.ElapsedTime.TotalSeconds;
		consoleOutput.WriteLine(string.Format(CodeAnalysisResources.GeneratorTotalExecutionTime, totalSeconds.ToString("##0.000", culture)));
		consoleOutput.WriteLine();
		consoleOutput.WriteLine(GetColumnHeader(CodeAnalysisResources.GeneratorNameColumnHeader));
		foreach (IGrouping<Assembly, GeneratorTimingInfo> item in from t in driverTimingInfo.GeneratorTimes
			group t by t.Generator.GetGeneratorType().Assembly into kvp
			orderby kvp.Sum((GeneratorTimingInfo entry) => entry.ElapsedTime.Ticks) descending
			select kvp)
		{
			double num = item.Sum((GeneratorTimingInfo x) => x.ElapsedTime.TotalSeconds);
			int percentage = (int)(num * 100.0 / totalSeconds);
			consoleOutput.WriteLine(GetColumnEntry(num, percentage, item.Key.FullName, culture));
			foreach (GeneratorTimingInfo item2 in item.OrderByDescending((GeneratorTimingInfo x) => x.ElapsedTime))
			{
				num = item2.ElapsedTime.TotalSeconds;
				percentage = (int)(num * 100.0 / totalSeconds);
				consoleOutput.WriteLine(GetColumnEntry(num, percentage, "   " + item2.Generator.GetGeneratorType().FullName, culture));
			}
		}
	}
}
