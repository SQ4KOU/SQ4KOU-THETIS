using System;

namespace Microsoft.CodeAnalysis.CSharp;

internal class SyntaxDiagnosticInfo : DiagnosticInfo
{
	internal readonly int Offset;

	internal readonly int Width;

	internal SyntaxDiagnosticInfo(int offset, int width, ErrorCode code, params object[] args)
		: base(Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance, (int)code, args)
	{
		Offset = offset;
		Width = width;
	}

	internal SyntaxDiagnosticInfo(int offset, int width, ErrorCode code)
		: this(offset, width, code, Array.Empty<object>())
	{
	}

	internal SyntaxDiagnosticInfo(ErrorCode code, params object[] args)
		: this(0, 0, code, args)
	{
	}

	internal SyntaxDiagnosticInfo(ErrorCode code)
		: this(0, 0, code)
	{
	}

	public SyntaxDiagnosticInfo WithOffset(int offset)
	{
		return new SyntaxDiagnosticInfo(offset, Width, (ErrorCode)base.Code, base.Arguments);
	}

	protected SyntaxDiagnosticInfo(SyntaxDiagnosticInfo original, DiagnosticSeverity severity)
		: base(original, severity)
	{
		Offset = original.Offset;
		Width = original.Width;
	}

	protected override DiagnosticInfo GetInstanceWithSeverityCore(DiagnosticSeverity severity)
	{
		return new SyntaxDiagnosticInfo(this, severity);
	}
}
