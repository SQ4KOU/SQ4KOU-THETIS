using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class SarifV2ErrorLogger : SarifErrorLogger, IDisposable
{
	private sealed class DiagnosticDescriptorSet
	{
		private readonly record struct DescriptorInfoWithIndex(int Index, DiagnosticDescriptorErrorLoggerInfo Info);

		private readonly Dictionary<DiagnosticDescriptor, DescriptorInfoWithIndex> _distinctDescriptors = new Dictionary<DiagnosticDescriptor, DescriptorInfoWithIndex>(SarifDiagnosticComparer.Instance);

		public int Count => _distinctDescriptors.Count;

		public int Add(DiagnosticDescriptor descriptor, DiagnosticDescriptorErrorLoggerInfo? info = null)
		{
			if (_distinctDescriptors.TryGetValue(descriptor, out var value))
			{
				if (info.HasValue)
				{
					DiagnosticDescriptorErrorLoggerInfo info2 = value.Info;
					DiagnosticDescriptorErrorLoggerInfo? diagnosticDescriptorErrorLoggerInfo = info;
					if (info2 != diagnosticDescriptorErrorLoggerInfo)
					{
						value = new DescriptorInfoWithIndex(value.Index, info.Value);
						_distinctDescriptors[descriptor] = value;
					}
				}
				return value.Index;
			}
			_distinctDescriptors.Add(descriptor, new DescriptorInfoWithIndex(Count, info.GetValueOrDefault()));
			return Count - 1;
		}

		public List<(int Index, DiagnosticDescriptor Descriptor, DiagnosticDescriptorErrorLoggerInfo Info)> ToSortedList()
		{
			List<(int, DiagnosticDescriptor, DiagnosticDescriptorErrorLoggerInfo)> list = new List<(int, DiagnosticDescriptor, DiagnosticDescriptorErrorLoggerInfo)>(Count);
			foreach (KeyValuePair<DiagnosticDescriptor, DescriptorInfoWithIndex> distinctDescriptor in _distinctDescriptors)
			{
				list.Add((distinctDescriptor.Value.Index, distinctDescriptor.Key, distinctDescriptor.Value.Info));
			}
			list.Sort(((int Index, DiagnosticDescriptor Descriptor, DiagnosticDescriptorErrorLoggerInfo Info) x, (int Index, DiagnosticDescriptor Descriptor, DiagnosticDescriptorErrorLoggerInfo Info) y) => x.Index.CompareTo(y.Index));
			return list;
		}
	}

	private readonly DiagnosticDescriptorSet _descriptors;

	private readonly HashSet<string> _diagnosticIdsWithAnySourceSuppressions;

	private readonly string _toolName;

	private readonly string _toolFileVersion;

	private readonly Version _toolAssemblyVersion;

	private string? _totalAnalyzerExecutionTime;

	protected override string PrimaryLocationPropertyName => "physicalLocation";

	public SarifV2ErrorLogger(Stream stream, string toolName, string toolFileVersion, Version toolAssemblyVersion, CultureInfo culture)
		: base(stream, culture)
	{
		_descriptors = new DiagnosticDescriptorSet();
		_diagnosticIdsWithAnySourceSuppressions = new HashSet<string>();
		_toolName = toolName;
		_toolFileVersion = toolFileVersion;
		_toolAssemblyVersion = toolAssemblyVersion;
		base._writer.WriteObjectStart();
		base._writer.Write("$schema", "http://json.schemastore.org/sarif-2.1.0");
		base._writer.Write("version", "2.1.0");
		base._writer.WriteArrayStart("runs");
		base._writer.WriteObjectStart();
		base._writer.WriteArrayStart("results");
	}

	public override void LogDiagnostic(Diagnostic diagnostic, SuppressionInfo? suppressionInfo)
	{
		base._writer.WriteObjectStart();
		base._writer.Write("ruleId", diagnostic.Id);
		int value = _descriptors.Add(diagnostic.Descriptor);
		base._writer.Write("ruleIndex", value);
		base._writer.Write("level", SarifErrorLogger.GetLevel(diagnostic.Severity));
		string message = diagnostic.GetMessage(base._culture);
		if (!RoslynString.IsNullOrEmpty(message))
		{
			base._writer.WriteObjectStart("message");
			base._writer.Write("text", message);
			base._writer.WriteObjectEnd();
		}
		if (diagnostic.IsSuppressed)
		{
			_diagnosticIdsWithAnySourceSuppressions.Add(diagnostic.Id);
			base._writer.WriteArrayStart("suppressions");
			base._writer.WriteObjectStart();
			base._writer.Write("kind", "inSource");
			string text = suppressionInfo?.Attribute?.DecodeNamedArgument<string>("Justification", SpecialType.System_String);
			if (text != null)
			{
				base._writer.Write("justification", text);
			}
			string text2 = null;
			ProgrammaticSuppressionInfo programmaticSuppressionInfo = diagnostic.ProgrammaticSuppressionInfo;
			if (programmaticSuppressionInfo != null)
			{
				string text3 = (from suppression in programmaticSuppressionInfo.Suppressions
					orderby suppression.Descriptor.Id
					select $"Suppression Id: {suppression.Descriptor.Id}, Suppression Justification: {suppression.Descriptor.Justification}").Join(", ");
				text2 = "DiagnosticSuppressor { " + text3 + " }";
			}
			else if (suppressionInfo != null)
			{
				text2 = ((suppressionInfo.Attribute != null) ? "SuppressMessageAttribute" : "Pragma Directive");
			}
			if (text2 != null)
			{
				base._writer.WriteObjectStart("properties");
				base._writer.Write("suppressionType", text2);
				base._writer.WriteObjectEnd();
			}
			base._writer.WriteObjectEnd();
			base._writer.WriteArrayEnd();
		}
		WriteLocations(diagnostic.Location, diagnostic.AdditionalLocations);
		WriteResultProperties(diagnostic);
		base._writer.WriteObjectEnd();
	}

	public override void AddAnalyzerDescriptorsAndExecutionTime(ImmutableArray<(DiagnosticDescriptor Descriptor, DiagnosticDescriptorErrorLoggerInfo Info)> descriptors, double totalAnalyzerExecutionTime)
	{
		foreach (var (descriptor, value) in descriptors.OrderBy<(DiagnosticDescriptor, DiagnosticDescriptorErrorLoggerInfo), string>(((DiagnosticDescriptor Descriptor, DiagnosticDescriptorErrorLoggerInfo Info) d) => d.Descriptor.Id))
		{
			_descriptors.Add(descriptor, value);
		}
		_totalAnalyzerExecutionTime = ReportAnalyzerUtil.GetFormattedAnalyzerExecutionTime(totalAnalyzerExecutionTime, base._culture).Trim();
	}

	private void WriteLocations(Location location, IReadOnlyList<Location> additionalLocations)
	{
		if (SarifErrorLogger.HasPath(location))
		{
			base._writer.WriteArrayStart("locations");
			base._writer.WriteObjectStart();
			base._writer.WriteKey(PrimaryLocationPropertyName);
			WritePhysicalLocation(location);
			base._writer.WriteObjectEnd();
			base._writer.WriteArrayEnd();
		}
		if (additionalLocations == null || additionalLocations.Count <= 0 || !additionalLocations.Any((Location l) => SarifErrorLogger.HasPath(l)))
		{
			return;
		}
		base._writer.WriteArrayStart("relatedLocations");
		foreach (Location additionalLocation in additionalLocations)
		{
			if (SarifErrorLogger.HasPath(additionalLocation))
			{
				base._writer.WriteObjectStart();
				base._writer.WriteKey("physicalLocation");
				WritePhysicalLocation(additionalLocation);
				base._writer.WriteObjectEnd();
			}
		}
		base._writer.WriteArrayEnd();
	}

	protected override void WritePhysicalLocation(Location diagnosticLocation)
	{
		FileLinePositionSpan mappedLineSpan = diagnosticLocation.GetMappedLineSpan();
		base._writer.WriteObjectStart();
		base._writer.WriteObjectStart("artifactLocation");
		base._writer.Write("uri", SarifErrorLogger.GetUri(mappedLineSpan.Path));
		base._writer.WriteObjectEnd();
		WriteRegion(mappedLineSpan);
		base._writer.WriteObjectEnd();
	}

	public override void Dispose()
	{
		base._writer.WriteArrayEnd();
		if (!string.IsNullOrEmpty(_totalAnalyzerExecutionTime))
		{
			base._writer.WriteObjectStart("properties");
			base._writer.Write("analyzerExecutionTime", _totalAnalyzerExecutionTime);
			base._writer.WriteObjectEnd();
		}
		WriteTool();
		base._writer.Write("columnKind", "utf16CodeUnits");
		base._writer.WriteObjectEnd();
		base._writer.WriteArrayEnd();
		base._writer.WriteObjectEnd();
		base.Dispose();
	}

	private void WriteTool()
	{
		base._writer.WriteObjectStart("tool");
		base._writer.WriteObjectStart("driver");
		base._writer.Write("name", _toolName);
		base._writer.Write("version", _toolFileVersion);
		base._writer.Write("dottedQuadFileVersion", _toolAssemblyVersion.ToString());
		base._writer.Write("semanticVersion", _toolAssemblyVersion.ToString(3));
		if (base._culture.Name.Length > 0)
		{
			base._writer.Write("language", base._culture.Name);
		}
		ImmutableArray<(string, int, ImmutableHashSet<ReportDiagnostic>)> effectiveSeverities = WriteRules();
		base._writer.WriteObjectEnd();
		base._writer.WriteObjectEnd();
		WriteInvocations(effectiveSeverities);
	}

	private ImmutableArray<(string DescriptorId, int DescriptorIndex, ImmutableHashSet<ReportDiagnostic> EffectiveSeverities)> WriteRules()
	{
		ArrayBuilder<(string, int, ImmutableHashSet<ReportDiagnostic>)> instance = ArrayBuilder<(string, int, ImmutableHashSet<ReportDiagnostic>)>.GetInstance(_descriptors.Count);
		if (_descriptors.Count > 0)
		{
			base._writer.WriteArrayStart("rules");
			bool flag = !string.IsNullOrEmpty(_totalAnalyzerExecutionTime);
			foreach (var (item, diagnosticDescriptor, diagnosticDescriptorErrorLoggerInfo) in _descriptors.ToSortedList())
			{
				base._writer.WriteObjectStart();
				base._writer.Write("id", diagnosticDescriptor.Id);
				string value = diagnosticDescriptor.Title.ToString(base._culture);
				if (!RoslynString.IsNullOrEmpty(value))
				{
					base._writer.WriteObjectStart("shortDescription");
					base._writer.Write("text", value);
					base._writer.WriteObjectEnd();
				}
				string value2 = diagnosticDescriptor.Description.ToString(base._culture);
				if (!RoslynString.IsNullOrEmpty(value2))
				{
					base._writer.WriteObjectStart("fullDescription");
					base._writer.Write("text", value2);
					base._writer.WriteObjectEnd();
				}
				WriteDefaultConfiguration(diagnosticDescriptor);
				if (!string.IsNullOrEmpty(diagnosticDescriptor.HelpLinkUri))
				{
					base._writer.Write("helpUri", diagnosticDescriptor.HelpLinkUri);
				}
				bool flag2 = _diagnosticIdsWithAnySourceSuppressions.Contains(diagnosticDescriptor.Id);
				bool flag3 = diagnosticDescriptorErrorLoggerInfo.HasAnyExternalSuppression | flag2;
				if ((!string.IsNullOrEmpty(diagnosticDescriptor.Category) | flag3 | flag) || diagnosticDescriptor.ImmutableCustomTags.Any())
				{
					base._writer.WriteObjectStart("properties");
					if (!string.IsNullOrEmpty(diagnosticDescriptor.Category))
					{
						base._writer.Write("category", diagnosticDescriptor.Category);
					}
					if (flag3)
					{
						base._writer.Write("isEverSuppressed", "true");
						base._writer.WriteArrayStart("suppressionKinds");
						if (diagnosticDescriptorErrorLoggerInfo.HasAnyExternalSuppression)
						{
							base._writer.Write("external");
						}
						if (flag2)
						{
							base._writer.Write("inSource");
						}
						base._writer.WriteArrayEnd();
					}
					if (flag)
					{
						string value3 = ReportAnalyzerUtil.GetFormattedAnalyzerExecutionTime(diagnosticDescriptorErrorLoggerInfo.ExecutionTime, base._culture).Trim();
						base._writer.Write("executionTimeInSeconds", value3);
						string value4 = ReportAnalyzerUtil.GetFormattedAnalyzerExecutionPercentage(diagnosticDescriptorErrorLoggerInfo.ExecutionPercentage, base._culture).Trim();
						base._writer.Write("executionTimeInPercentage", value4);
					}
					if (diagnosticDescriptor.ImmutableCustomTags.Any())
					{
						base._writer.WriteArrayStart("tags");
						foreach (string immutableCustomTag in diagnosticDescriptor.ImmutableCustomTags)
						{
							base._writer.Write(immutableCustomTag);
						}
						base._writer.WriteArrayEnd();
					}
					base._writer.WriteObjectEnd();
				}
				base._writer.WriteObjectEnd();
				ReportDiagnostic reportDiagnostic = (diagnosticDescriptor.IsEnabledByDefault ? DiagnosticDescriptor.MapSeverityToReport(diagnosticDescriptor.DefaultSeverity) : ReportDiagnostic.Suppress);
				if (diagnosticDescriptorErrorLoggerInfo.EffectiveSeverities != null && (diagnosticDescriptorErrorLoggerInfo.EffectiveSeverities.Count != 1 || diagnosticDescriptorErrorLoggerInfo.EffectiveSeverities.Single() != reportDiagnostic))
				{
					instance.Add((diagnosticDescriptor.Id, item, diagnosticDescriptorErrorLoggerInfo.EffectiveSeverities));
				}
			}
			base._writer.WriteArrayEnd();
		}
		return instance.ToImmutableAndFree();
	}

	private void WriteInvocations(ImmutableArray<(string DescriptorId, int DescriptorIndex, ImmutableHashSet<ReportDiagnostic> EffectiveSeverities)> effectiveSeverities)
	{
		if (effectiveSeverities.IsEmpty)
		{
			return;
		}
		base._writer.WriteArrayStart("invocations");
		base._writer.WriteObjectStart();
		base._writer.Write("executionSuccessful", value: true);
		base._writer.WriteArrayStart("ruleConfigurationOverrides");
		foreach (var item in effectiveSeverities)
		{
			var (value, value2, _) = item;
			foreach (ReportDiagnostic item2 in item.EffectiveSeverities.OrderBy(Comparer<ReportDiagnostic>.Default))
			{
				base._writer.WriteObjectStart();
				base._writer.WriteObjectStart("descriptor");
				base._writer.Write("id", value);
				base._writer.Write("index", value2);
				base._writer.WriteObjectEnd();
				base._writer.WriteObjectStart("configuration");
				DiagnosticSeverity? diagnosticSeverity = DiagnosticDescriptor.MapReportToSeverity(item2);
				if (!diagnosticSeverity.HasValue)
				{
					base._writer.Write("enabled", value: false);
				}
				else
				{
					string level = SarifErrorLogger.GetLevel(diagnosticSeverity.Value);
					base._writer.Write("level", level);
				}
				base._writer.WriteObjectEnd();
				base._writer.WriteObjectEnd();
			}
		}
		base._writer.WriteArrayEnd();
		base._writer.WriteObjectEnd();
		base._writer.WriteArrayEnd();
	}

	private void WriteDefaultConfiguration(DiagnosticDescriptor descriptor)
	{
		string level = SarifErrorLogger.GetLevel(descriptor.DefaultSeverity);
		bool flag = level != "warning";
		bool flag2 = !descriptor.IsEnabledByDefault;
		if (flag | flag2)
		{
			base._writer.WriteObjectStart("defaultConfiguration");
			if (flag)
			{
				base._writer.Write("level", level);
			}
			if (flag2)
			{
				base._writer.Write("enabled", descriptor.IsEnabledByDefault);
			}
			base._writer.WriteObjectEnd();
		}
	}
}
