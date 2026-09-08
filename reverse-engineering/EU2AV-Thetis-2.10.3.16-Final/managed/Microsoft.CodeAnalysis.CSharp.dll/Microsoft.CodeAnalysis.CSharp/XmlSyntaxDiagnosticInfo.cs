using System;
using System.Globalization;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class XmlSyntaxDiagnosticInfo : SyntaxDiagnosticInfo
{
	private readonly XmlParseErrorCode _xmlErrorCode;

	internal XmlSyntaxDiagnosticInfo(XmlParseErrorCode code, params object[] args)
		: this(0, 0, code, args)
	{
	}

	internal XmlSyntaxDiagnosticInfo(int offset, int width, XmlParseErrorCode code, params object[] args)
		: base(offset, width, ErrorCode.WRN_XMLParseError, args)
	{
		_xmlErrorCode = code;
	}

	private XmlSyntaxDiagnosticInfo(XmlSyntaxDiagnosticInfo original, DiagnosticSeverity severity)
		: base(original, severity)
	{
		_xmlErrorCode = original._xmlErrorCode;
	}

	protected override DiagnosticInfo GetInstanceWithSeverityCore(DiagnosticSeverity severity)
	{
		return new XmlSyntaxDiagnosticInfo(this, severity);
	}

	public override string GetMessage(IFormatProvider? formatProvider = null)
	{
		CultureInfo cultureInfo = formatProvider as CultureInfo;
		string format = base.MessageProvider.LoadMessage(base.Code, cultureInfo);
		string message = ErrorFacts.GetMessage(_xmlErrorCode, cultureInfo);
		if (base.Arguments == null || base.Arguments.Length == 0)
		{
			return string.Format(formatProvider, format, message);
		}
		return string.Format(formatProvider, string.Format(formatProvider, format, message), GetArgumentsToUse(formatProvider));
	}
}
