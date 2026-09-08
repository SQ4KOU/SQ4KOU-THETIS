using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class CSharpDiagnosticFilter
{
	private static readonly ErrorCode[] s_alinkWarnings = new ErrorCode[3]
	{
		ErrorCode.WRN_ConflictingMachineAssembly,
		ErrorCode.WRN_RefCultureMismatch,
		ErrorCode.WRN_InvalidVersionFormat
	};

	internal static Diagnostic? Filter(Diagnostic d, int warningLevelOption, NullableContextOptions nullableOption, ReportDiagnostic generalDiagnosticOption, IDictionary<string, ReportDiagnostic> specificDiagnosticOptions, SyntaxTreeOptionsProvider? syntaxTreeOptions, CancellationToken cancellationToken)
	{
		if (d == null)
		{
			return d;
		}
		if (d.IsNotConfigurable())
		{
			if (d.IsEnabledByDefault)
			{
				return d;
			}
			return null;
		}
		if (d.Severity == (DiagnosticSeverity)(-2))
		{
			return null;
		}
		ReportDiagnostic reportAction = ((!s_alinkWarnings.Contains((ErrorCode)d.Code) || !specificDiagnosticOptions.Keys.Contains(MessageProvider.Instance.GetIdForErrorCode(1607))) ? GetDiagnosticReport(d.Severity, d.IsEnabledByDefault, d.Code, d.Id, d.WarningLevel, d.Location, d.CustomTags, warningLevelOption, nullableOption, generalDiagnosticOption, specificDiagnosticOptions, syntaxTreeOptions, cancellationToken, out var hasPragmaSuppression) : GetDiagnosticReport(ErrorFacts.GetSeverity(ErrorCode.WRN_ALinkWarn), d.IsEnabledByDefault, d.Code, MessageProvider.Instance.GetIdForErrorCode(1607), ErrorFacts.GetWarningLevel(ErrorCode.WRN_ALinkWarn), d.Location, d.CustomTags, warningLevelOption, nullableOption, generalDiagnosticOption, specificDiagnosticOptions, syntaxTreeOptions, cancellationToken, out hasPragmaSuppression));
		if (hasPragmaSuppression)
		{
			d = d.WithIsSuppressed(isSuppressed: true);
		}
		return d.WithReportDiagnostic(reportAction);
	}

	internal static ReportDiagnostic GetDiagnosticReport(DiagnosticSeverity severity, bool isEnabledByDefault, int errorCode, string id, int diagnosticWarningLevel, Location location, ImmutableArray<string> customTags, int warningLevelOption, NullableContextOptions nullableOption, ReportDiagnostic generalDiagnosticOption, IDictionary<string, ReportDiagnostic> specificDiagnosticOptions, SyntaxTreeOptionsProvider? syntaxTreeOptions, CancellationToken cancellationToken, out bool hasPragmaSuppression)
	{
		hasPragmaSuppression = false;
		CSharpSyntaxTree cSharpSyntaxTree = location.SourceTree as CSharpSyntaxTree;
		int start = location.SourceSpan.Start;
		if (ErrorFacts.NullableWarnings.Contains(id))
		{
			NullableContextState.State? state = cSharpSyntaxTree?.GetNullableContextState(start).WarningsState;
			if (state switch
			{
				NullableContextState.State.Enabled => 1, 
				NullableContextState.State.Disabled => 0, 
				NullableContextState.State.ExplicitlyRestored => nullableOption.WarningsEnabled() ? 1 : 0, 
				NullableContextState.State.Unknown => (nullableOption.WarningsEnabled() && (cSharpSyntaxTree == null || !cSharpSyntaxTree.IsGeneratedCode(syntaxTreeOptions, cancellationToken))) ? 1 : 0, 
				null => nullableOption.WarningsEnabled() ? 1 : 0, 
				_ => throw ExceptionUtilities.UnexpectedValue(state), 
			} == 0)
			{
				return ReportDiagnostic.Suppress;
			}
		}
		if (diagnosticWarningLevel > warningLevelOption)
		{
			return ReportDiagnostic.Suppress;
		}
		bool isSpecified = false;
		bool flag = false;
		if (specificDiagnosticOptions.TryGetValue(id, out var value))
		{
			isSpecified = true;
			if (value == ReportDiagnostic.Default)
			{
				flag = true;
			}
		}
		bool flag2 = false;
		if (AnalyzerManager.HasCustomSeverityConfigurableTag(customTags))
		{
			flag2 = true;
			if (!isSpecified | flag)
			{
				isSpecified = true;
				value = DiagnosticDescriptor.MapSeverityToReport(severity);
				if (value == ReportDiagnostic.Warn && generalDiagnosticOption == ReportDiagnostic.Error && !flag)
				{
					value = ReportDiagnostic.Error;
				}
			}
		}
		if (syntaxTreeOptions != null && !flag2 && (!isSpecified | flag) && ((cSharpSyntaxTree != null && syntaxTreeOptions.TryGetDiagnosticValue(cSharpSyntaxTree, id, cancellationToken, out var severity2)) || syntaxTreeOptions.TryGetGlobalDiagnosticValue(id, cancellationToken, out severity2)) && (!flag || severity != DiagnosticSeverity.Warning || severity2 != ReportDiagnostic.Error))
		{
			isSpecified = true;
			value = severity2;
			if (!flag && value == ReportDiagnostic.Warn && generalDiagnosticOption == ReportDiagnostic.Error)
			{
				value = ReportDiagnostic.Error;
			}
		}
		if (!isSpecified)
		{
			value = ((!isEnabledByDefault) ? ReportDiagnostic.Suppress : ReportDiagnostic.Default);
		}
		if (value == ReportDiagnostic.Suppress)
		{
			return ReportDiagnostic.Suppress;
		}
		PragmaWarningState num = cSharpSyntaxTree?.GetPragmaDirectiveWarningState(id, start) ?? PragmaWarningState.Default;
		if (num == PragmaWarningState.Disabled)
		{
			hasPragmaSuppression = true;
		}
		if (num == PragmaWarningState.Enabled)
		{
			switch (value)
			{
			case ReportDiagnostic.Error:
			case ReportDiagnostic.Warn:
			case ReportDiagnostic.Info:
			case ReportDiagnostic.Hidden:
				return value;
			case ReportDiagnostic.Suppress:
				return ReportDiagnostic.Default;
			case ReportDiagnostic.Default:
				if (generalDiagnosticOption == ReportDiagnostic.Error && promoteToAnError())
				{
					return ReportDiagnostic.Error;
				}
				return ReportDiagnostic.Default;
			default:
				throw ExceptionUtilities.UnexpectedValue(value);
			}
		}
		switch (value)
		{
		case ReportDiagnostic.Suppress:
			return ReportDiagnostic.Suppress;
		case ReportDiagnostic.Default:
			switch (generalDiagnosticOption)
			{
			case ReportDiagnostic.Error:
				if (promoteToAnError())
				{
					return ReportDiagnostic.Error;
				}
				break;
			case ReportDiagnostic.Suppress:
				if (severity == DiagnosticSeverity.Warning || severity == DiagnosticSeverity.Info)
				{
					value = ReportDiagnostic.Suppress;
					isSpecified = true;
				}
				break;
			}
			break;
		}
		bool flag3 = !isSpecified;
		if (flag3)
		{
			bool flag4 = ((errorCode == 9204 || errorCode == 9268) ? true : false);
			flag3 = flag4;
		}
		if (flag3)
		{
			value = ReportDiagnostic.Error;
		}
		return value;
		bool promoteToAnError()
		{
			if (severity == DiagnosticSeverity.Warning)
			{
				return !isSpecified;
			}
			return false;
		}
	}
}
