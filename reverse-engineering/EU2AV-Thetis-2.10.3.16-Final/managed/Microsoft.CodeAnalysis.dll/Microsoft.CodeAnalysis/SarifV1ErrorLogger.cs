using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class SarifV1ErrorLogger : SarifErrorLogger, IDisposable
{
	private sealed class DiagnosticDescriptorSet
	{
		private readonly Dictionary<string, int> _counters = new Dictionary<string, int>();

		private readonly Dictionary<DiagnosticDescriptor, string> _keys = new Dictionary<DiagnosticDescriptor, string>(SarifDiagnosticComparer.Instance);

		public int Count => _keys.Count;

		public string Add(DiagnosticDescriptor descriptor)
		{
			if (_keys.TryGetValue(descriptor, out string value))
			{
				return value;
			}
			if (!_counters.TryGetValue(descriptor.Id, out var value2))
			{
				_counters.Add(descriptor.Id, 0);
				_keys.Add(descriptor, descriptor.Id);
				return descriptor.Id;
			}
			do
			{
				value2 = (_counters[descriptor.Id] = value2 + 1);
				value = descriptor.Id + "-" + value2.ToString("000", CultureInfo.InvariantCulture);
			}
			while (_counters.ContainsKey(value));
			_keys.Add(descriptor, value);
			return value;
		}

		public List<KeyValuePair<string, DiagnosticDescriptor>> ToSortedList()
		{
			List<KeyValuePair<string, DiagnosticDescriptor>> list = new List<KeyValuePair<string, DiagnosticDescriptor>>(Count);
			foreach (KeyValuePair<DiagnosticDescriptor, string> key in _keys)
			{
				list.Add(new KeyValuePair<string, DiagnosticDescriptor>(key.Value, key.Key));
			}
			list.Sort((KeyValuePair<string, DiagnosticDescriptor> x, KeyValuePair<string, DiagnosticDescriptor> y) => string.CompareOrdinal(x.Key, y.Key));
			return list;
		}
	}

	private readonly DiagnosticDescriptorSet _descriptors;

	protected override string PrimaryLocationPropertyName => "resultFile";

	public SarifV1ErrorLogger(Stream stream, string toolName, string toolFileVersion, Version toolAssemblyVersion, CultureInfo culture)
		: base(stream, culture)
	{
		_descriptors = new DiagnosticDescriptorSet();
		base._writer.WriteObjectStart();
		base._writer.Write("$schema", "http://json.schemastore.org/sarif-1.0.0");
		base._writer.Write("version", "1.0.0");
		base._writer.WriteArrayStart("runs");
		base._writer.WriteObjectStart();
		base._writer.WriteObjectStart("tool");
		base._writer.Write("name", toolName);
		base._writer.Write("version", toolAssemblyVersion.ToString());
		base._writer.Write("fileVersion", toolFileVersion);
		base._writer.Write("semanticVersion", toolAssemblyVersion.ToString(3));
		if (culture.Name.Length > 0)
		{
			base._writer.Write("language", culture.Name);
		}
		base._writer.WriteObjectEnd();
		base._writer.WriteArrayStart("results");
	}

	public override void LogDiagnostic(Diagnostic diagnostic, SuppressionInfo? suppressionInfo)
	{
		base._writer.WriteObjectStart();
		base._writer.Write("ruleId", diagnostic.Id);
		string text = _descriptors.Add(diagnostic.Descriptor);
		if (text != diagnostic.Id)
		{
			base._writer.Write("ruleKey", text);
		}
		base._writer.Write("level", SarifErrorLogger.GetLevel(diagnostic.Severity));
		string message = diagnostic.GetMessage(base._culture);
		if (!RoslynString.IsNullOrEmpty(message))
		{
			base._writer.Write("message", message);
		}
		if (diagnostic.IsSuppressed)
		{
			base._writer.WriteArrayStart("suppressionStates");
			base._writer.Write("suppressedInSource");
			base._writer.WriteArrayEnd();
		}
		WriteLocations(diagnostic.Location, diagnostic.AdditionalLocations);
		WriteResultProperties(diagnostic);
		base._writer.WriteObjectEnd();
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

	public override void AddAnalyzerDescriptorsAndExecutionTime(ImmutableArray<(DiagnosticDescriptor Descriptor, DiagnosticDescriptorErrorLoggerInfo Info)> descriptors, double totalAnalyzerExecutionTime)
	{
	}

	protected override void WritePhysicalLocation(Location location)
	{
		FileLinePositionSpan mappedLineSpan = location.GetMappedLineSpan();
		base._writer.WriteObjectStart();
		base._writer.Write("uri", SarifErrorLogger.GetUri(mappedLineSpan.Path));
		WriteRegion(mappedLineSpan);
		base._writer.WriteObjectEnd();
	}

	private void WriteRules()
	{
		if (_descriptors.Count <= 0)
		{
			return;
		}
		base._writer.WriteObjectStart("rules");
		foreach (KeyValuePair<string, DiagnosticDescriptor> item in _descriptors.ToSortedList())
		{
			DiagnosticDescriptor value = item.Value;
			base._writer.WriteObjectStart(item.Key);
			base._writer.Write("id", value.Id);
			string value2 = value.Title.ToString(base._culture);
			if (!RoslynString.IsNullOrEmpty(value2))
			{
				base._writer.Write("shortDescription", value2);
			}
			string value3 = value.Description.ToString(base._culture);
			if (!RoslynString.IsNullOrEmpty(value3))
			{
				base._writer.Write("fullDescription", value3);
			}
			base._writer.Write("defaultLevel", SarifErrorLogger.GetLevel(value.DefaultSeverity));
			if (!string.IsNullOrEmpty(value.HelpLinkUri))
			{
				base._writer.Write("helpUri", value.HelpLinkUri);
			}
			base._writer.WriteObjectStart("properties");
			if (!string.IsNullOrEmpty(value.Category))
			{
				base._writer.Write("category", value.Category);
			}
			base._writer.Write("isEnabledByDefault", value.IsEnabledByDefault);
			if (value.ImmutableCustomTags.Any())
			{
				base._writer.WriteArrayStart("tags");
				foreach (string immutableCustomTag in value.ImmutableCustomTags)
				{
					base._writer.Write(immutableCustomTag);
				}
				base._writer.WriteArrayEnd();
			}
			base._writer.WriteObjectEnd();
			base._writer.WriteObjectEnd();
		}
		base._writer.WriteObjectEnd();
	}

	public override void Dispose()
	{
		base._writer.WriteArrayEnd();
		WriteRules();
		base._writer.WriteObjectEnd();
		base._writer.WriteArrayEnd();
		base._writer.WriteObjectEnd();
		base.Dispose();
	}
}
