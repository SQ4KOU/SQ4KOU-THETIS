using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.CodeAnalysis.ErrorReporting;

internal static class FatalError
{
	public delegate void ErrorReporterHandler(Exception exception, ErrorSeverity severity, bool forceDump);

	private static ErrorReporterHandler? s_handler;

	private static ErrorReporterHandler? s_nonFatalHandler;

	private static Exception? s_reportedException;

	private static string? s_reportedExceptionMessage;

	private static readonly object s_reportedMarker = Guid.NewGuid();

	public static void SetHandlers(ErrorReporterHandler handler, ErrorReporterHandler? nonFatalHandler)
	{
		if ((Delegate?)s_handler != (Delegate?)handler)
		{
			s_handler = handler;
			s_nonFatalHandler = nonFatalHandler;
		}
	}

	public static void OverwriteHandler(ErrorReporterHandler? value)
	{
		s_handler = value;
	}

	public static void CopyHandlersTo(Assembly assembly)
	{
		copyHandlerTo(assembly, s_handler, "s_handler");
		copyHandlerTo(assembly, s_nonFatalHandler, "s_nonFatalHandler");
		static void copyHandlerTo(Assembly assembly2, ErrorReporterHandler? handler, string handlerName)
		{
			FieldInfo field = assembly2.GetType(typeof(FatalError).FullName, throwOnError: true).GetField(handlerName, BindingFlags.Static | BindingFlags.NonPublic);
			if (handler != null)
			{
				Delegate value = Delegate.CreateDelegate(field.FieldType, handler.Target, handler.Method);
				field.SetValue(null, value);
			}
			else
			{
				field.SetValue(null, null);
			}
		}
	}

	[DebuggerHidden]
	public static bool ReportAndPropagate(Exception exception, ErrorSeverity severity = ErrorSeverity.Uncategorized)
	{
		Report(exception, severity);
		return false;
	}

	[DebuggerHidden]
	public static bool ReportAndPropagateUnlessCanceled(Exception exception, ErrorSeverity severity = ErrorSeverity.Uncategorized)
	{
		if (exception is OperationCanceledException)
		{
			return false;
		}
		return ReportAndPropagate(exception, severity);
	}

	[DebuggerHidden]
	public static bool ReportAndPropagateUnlessCanceled(Exception exception, CancellationToken contextCancellationToken, ErrorSeverity severity = ErrorSeverity.Uncategorized)
	{
		if (ExceptionUtilities.IsCurrentOperationBeingCancelled(exception, contextCancellationToken) || exception is OperationCanceledIgnoringCallerTokenException)
		{
			return false;
		}
		return ReportAndPropagate(exception, severity);
	}

	[DebuggerHidden]
	public static bool ReportAndCatch(Exception exception, ErrorSeverity severity = ErrorSeverity.Uncategorized)
	{
		Report(exception, severity);
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Report(Exception exception, ErrorSeverity severity = ErrorSeverity.Uncategorized, bool forceDump = false)
	{
		ReportException(exception, severity, forceDump, s_handler);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static void ReportNonFatalError(Exception exception, ErrorSeverity severity = ErrorSeverity.Uncategorized, bool forceDump = false)
	{
		ReportException(exception, severity, forceDump, s_nonFatalHandler);
	}

	private static void ReportException(Exception exception, ErrorSeverity severity, bool forceDump, ErrorReporterHandler? handler)
	{
		s_reportedException = exception;
		s_reportedExceptionMessage = exception.ToString();
		if (handler != null && exception.Data[s_reportedMarker] == null && (!(exception is AggregateException ex) || ex.InnerExceptions.Count != 1 || ex.InnerExceptions[0].Data[s_reportedMarker] == null))
		{
			if (!exception.Data.IsReadOnly)
			{
				exception.Data[s_reportedMarker] = s_reportedMarker;
			}
			handler(exception, severity, forceDump);
		}
	}
}
