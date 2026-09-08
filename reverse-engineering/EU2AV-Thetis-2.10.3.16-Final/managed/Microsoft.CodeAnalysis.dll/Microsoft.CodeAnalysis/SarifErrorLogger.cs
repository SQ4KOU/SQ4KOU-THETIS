using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal abstract class SarifErrorLogger : ErrorLogger, IDisposable
{
	private static readonly Uri s_fileRoot = new Uri("file:///");

	protected JsonWriter _writer { get; }

	protected CultureInfo _culture { get; }

	protected abstract string PrimaryLocationPropertyName { get; }

	protected SarifErrorLogger(Stream stream, CultureInfo culture)
	{
		_writer = new JsonWriter(new StreamWriter(stream));
		_culture = culture;
	}

	protected abstract void WritePhysicalLocation(Location diagnosticLocation);

	public virtual void Dispose()
	{
		_writer.Dispose();
	}

	protected void WriteRegion(FileLinePositionSpan span)
	{
		_writer.WriteObjectStart("region");
		_writer.Write("startLine", span.StartLinePosition.Line + 1);
		_writer.Write("startColumn", span.StartLinePosition.Character + 1);
		_writer.Write("endLine", span.EndLinePosition.Line + 1);
		_writer.Write("endColumn", span.EndLinePosition.Character + 1);
		_writer.WriteObjectEnd();
	}

	protected static string GetLevel(DiagnosticSeverity severity)
	{
		switch (severity)
		{
		case DiagnosticSeverity.Hidden:
		case DiagnosticSeverity.Info:
			return "note";
		case DiagnosticSeverity.Error:
			return "error";
		default:
			return "warning";
		}
	}

	protected void WriteResultProperties(Diagnostic diagnostic)
	{
		if (diagnostic.WarningLevel <= 0 && diagnostic.Properties.Count <= 0)
		{
			return;
		}
		_writer.WriteObjectStart("properties");
		if (diagnostic.WarningLevel > 0)
		{
			_writer.Write("warningLevel", diagnostic.WarningLevel);
		}
		if (diagnostic.Properties.Count > 0)
		{
			_writer.WriteObjectStart("customProperties");
			foreach (KeyValuePair<string, string> item in diagnostic.Properties.OrderBy<KeyValuePair<string, string>, string>((KeyValuePair<string, string> x) => x.Key, StringComparer.Ordinal))
			{
				_writer.Write(item.Key, item.Value);
			}
			_writer.WriteObjectEnd();
		}
		_writer.WriteObjectEnd();
	}

	protected static bool HasPath(Location location)
	{
		return !string.IsNullOrEmpty(location.GetLineSpan().Path);
	}

	protected static string GetUri(string path)
	{
		if (Path.IsPathRooted(path))
		{
			if (Uri.TryCreate(Path.GetFullPath(path), UriKind.Absolute, out Uri result))
			{
				return result.AbsoluteUri;
			}
		}
		else
		{
			if (!PathUtilities.IsUnixLikePlatform)
			{
				path = path.Replace("\\\\", "\\");
				path = PathUtilities.NormalizeWithForwardSlash(path);
			}
			if (Uri.TryCreate(path, UriKind.Relative, out Uri result2))
			{
				return s_fileRoot.MakeRelativeUri(new Uri(s_fileRoot, result2)).ToString();
			}
		}
		return WebUtility.UrlEncode(path);
	}
}
