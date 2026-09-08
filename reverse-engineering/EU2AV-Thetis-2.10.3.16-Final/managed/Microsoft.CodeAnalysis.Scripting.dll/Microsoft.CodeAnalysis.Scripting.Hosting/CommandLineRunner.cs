using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal sealed class CommandLineRunner
{
	private readonly ScriptCompiler _scriptCompiler;

	private readonly ObjectFormatter _objectFormatter;

	private readonly Func<string, PEStreamOptions, MetadataReferenceProperties, MetadataImageReference> _createFromFileFunc;

	internal ConsoleIO Console { get; }

	internal CommonCompiler Compiler { get; }

	internal CommandLineRunner(ConsoleIO console, CommonCompiler compiler, ScriptCompiler scriptCompiler, ObjectFormatter objectFormatter, Func<string, PEStreamOptions, MetadataReferenceProperties, MetadataImageReference>? createFromFileFunc = null)
	{
		Console = console;
		Compiler = compiler;
		_scriptCompiler = scriptCompiler;
		_objectFormatter = objectFormatter;
		_createFromFileFunc = createFromFileFunc ?? new Func<string, PEStreamOptions, MetadataReferenceProperties, MetadataImageReference>(Script.CreateFromFile);
	}

	internal int RunInteractive()
	{
		SarifErrorLogger sarifErrorLogger = null;
		if (Compiler.Arguments.ErrorLogOptions?.Path != null)
		{
			sarifErrorLogger = Compiler.GetErrorLogger(Console.Error);
			if (sarifErrorLogger == null)
			{
				return 1;
			}
		}
		using (sarifErrorLogger)
		{
			return RunInteractiveCore(sarifErrorLogger);
		}
	}

	private int RunInteractiveCore(ErrorLogger? errorLogger)
	{
		ImmutableArray<CommandLineSourceFile> sourceFiles = Compiler.Arguments.SourceFiles;
		if (Compiler.Arguments.DisplayVersion)
		{
			Compiler.PrintVersion(Console.Out);
			return 0;
		}
		if (Compiler.Arguments.DisplayLangVersions)
		{
			Compiler.PrintLangVersions(Console.Out);
			return 0;
		}
		if (sourceFiles.IsEmpty && Compiler.Arguments.DisplayLogo)
		{
			Compiler.PrintLogo(Console.Out);
			if (!Compiler.Arguments.DisplayHelp)
			{
				Console.Out.WriteLine(ScriptingResources.HelpPrompt);
			}
		}
		if (Compiler.Arguments.DisplayHelp)
		{
			Compiler.PrintHelp(Console.Out);
			return 0;
		}
		SourceText sourceText = null;
		List<DiagnosticInfo> list = new List<DiagnosticInfo>();
		if (!sourceFiles.IsEmpty)
		{
			if (sourceFiles.Length > 1 || !sourceFiles[0].IsScript)
			{
				list.Add(new DiagnosticInfo(Compiler.MessageProvider, Compiler.MessageProvider.ERR_ExpectedSingleScript));
			}
			else
			{
				sourceText = Compiler.TryReadFileContent(sourceFiles[0], list);
			}
		}
		bool emitDebugInformation = !Compiler.Arguments.InteractiveMode;
		string scriptPathOpt = (sourceFiles.IsEmpty ? null : sourceFiles[0].Path);
		ScriptOptions scriptOptions = GetScriptOptions(Compiler.Arguments, scriptPathOpt, Compiler.MessageProvider, list, emitDebugInformation);
		IEnumerable<Diagnostic> diagnostics = Compiler.Arguments.Errors.Concat(list.Select(Diagnostic.Create));
		if (Compiler.ReportDiagnostics(diagnostics, Console.Error, errorLogger, null))
		{
			return 1;
		}
		CancellationToken cancellationToken = default(CancellationToken);
		if (Compiler.Arguments.InteractiveMode)
		{
			RunInteractiveLoop(scriptOptions, sourceText?.ToString(), cancellationToken);
			return 0;
		}
		return RunScript(scriptOptions, sourceText, errorLogger, cancellationToken);
	}

	private ScriptOptions? GetScriptOptions(CommandLineArguments arguments, string? scriptPathOpt, CommonMessageProvider messageProvider, List<DiagnosticInfo> diagnostics, bool emitDebugInformation)
	{
		TouchedFileLogger loggerOpt = ((arguments.TouchedFilesPath != null) ? new TouchedFileLogger() : null);
		MetadataReferenceResolver metadataReferenceResolver = GetMetadataReferenceResolver(arguments, loggerOpt, _createFromFileFunc);
		SourceReferenceResolver sourceReferenceResolver = GetSourceReferenceResolver(arguments, loggerOpt);
		List<MetadataReference> list = new List<MetadataReference>();
		if (!arguments.ResolveMetadataReferences(metadataReferenceResolver, diagnostics, messageProvider, list))
		{
			return null;
		}
		return new ScriptOptions(scriptPathOpt ?? "", ImmutableArray.CreateRange(list), CommandLineHelpers.GetImports(arguments), metadataReferenceResolver, sourceReferenceResolver, emitDebugInformation, null, OptimizationLevel.Debug, checkOverflow: false, allowUnsafe: true, 4, arguments.ParseOptions, _createFromFileFunc);
	}

	internal static MetadataReferenceResolver GetMetadataReferenceResolver(CommandLineArguments arguments, TouchedFileLogger? loggerOpt, Func<string, PEStreamOptions, MetadataReferenceProperties, MetadataImageReference> createFromFileFunc)
	{
		return RuntimeMetadataReferenceResolver.CreateCurrentPlatformResolver(arguments.ReferencePaths, arguments.BaseDirectory, delegate(string path, MetadataReferenceProperties properties)
		{
			loggerOpt?.AddRead(path);
			return createFromFileFunc(path, PEStreamOptions.PrefetchEntireImage, properties);
		});
	}

	internal static SourceReferenceResolver GetSourceReferenceResolver(CommandLineArguments arguments, TouchedFileLogger? loggerOpt)
	{
		return new CommonCompiler.LoggingSourceFileResolver(arguments.SourcePaths, arguments.BaseDirectory, ImmutableArray<KeyValuePair<string, string>>.Empty, loggerOpt);
	}

	private int RunScript(ScriptOptions? options, SourceText? code, ErrorLogger? errorLogger, CancellationToken cancellationToken)
	{
		CommandLineScriptGlobals commandLineScriptGlobals = new CommandLineScriptGlobals(Console.Out, _objectFormatter);
		commandLineScriptGlobals.Args.AddRange<string>(Compiler.Arguments.ScriptArguments);
		Script<int> script = Script.CreateInitialScript<int>(_scriptCompiler, code, options, commandLineScriptGlobals.GetType(), null);
		try
		{
			return script.RunAsync(commandLineScriptGlobals, cancellationToken).GetAwaiter().GetResult()
				.ReturnValue;
		}
		catch (CompilationErrorException ex)
		{
			Compiler.ReportDiagnostics(ex.Diagnostics, Console.Error, errorLogger, null);
			return 1;
		}
		catch (Exception ex2)
		{
			DisplayException(ex2);
			return ex2.HResult;
		}
	}

	private void RunInteractiveLoop(ScriptOptions options, string? initialScriptCodeOpt, CancellationToken cancellationToken)
	{
		InteractiveScriptGlobals interactiveScriptGlobals = new InteractiveScriptGlobals(Console.Out, _objectFormatter);
		interactiveScriptGlobals.Args.AddRange<string>(Compiler.Arguments.ScriptArguments);
		ScriptState<object> state = null;
		if (initialScriptCodeOpt != null)
		{
			Script<object> newScript = Script.CreateInitialScript<object>(_scriptCompiler, SourceText.From(initialScriptCodeOpt), options, interactiveScriptGlobals.GetType(), null);
			BuildAndRun(newScript, interactiveScriptGlobals, ref state, ref options, displayResult: false, cancellationToken);
		}
		while (true)
		{
			Console.Out.Write("> ");
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			while (true)
			{
				string text = Console.In.ReadLine();
				if (text == null)
				{
					if (stringBuilder.Length == 0)
					{
						return;
					}
					flag = true;
					break;
				}
				stringBuilder.AppendLine(text);
				SyntaxTree tree = _scriptCompiler.ParseSubmission(SourceText.From(stringBuilder.ToString()), options.ParseOptions, cancellationToken);
				if (_scriptCompiler.IsCompleteSubmission(tree))
				{
					break;
				}
				Console.Out.Write(". ");
			}
			if (!flag)
			{
				string text2 = stringBuilder.ToString();
				if (IsHelpCommand(text2))
				{
					DisplayHelpText();
					continue;
				}
				Script<object> newScript2 = ((state != null) ? state.Script.ContinueWith(text2, options) : Script.CreateInitialScript<object>(_scriptCompiler, SourceText.From(text2 ?? string.Empty), options, interactiveScriptGlobals.GetType(), null));
				BuildAndRun(newScript2, interactiveScriptGlobals, ref state, ref options, displayResult: true, cancellationToken);
			}
		}
	}

	private void BuildAndRun(Script<object> newScript, InteractiveScriptGlobals globals, ref ScriptState<object>? state, ref ScriptOptions options, bool displayResult, CancellationToken cancellationToken)
	{
		ImmutableArray<Diagnostic> diagnostics = newScript.Compile(cancellationToken);
		DisplayDiagnostics(diagnostics);
		if (!diagnostics.HasAnyErrors())
		{
			Task<ScriptState<object>> task = ((state == null) ? newScript.RunAsync(globals, (Exception e) => true, cancellationToken) : newScript.RunFromAsync(state, (Exception e) => true, cancellationToken));
			state = task.GetAwaiter().GetResult();
			if (state.Exception != null)
			{
				DisplayException(state.Exception);
			}
			else if (displayResult && newScript.HasReturnValue())
			{
				globals.Print(state.ReturnValue);
			}
			options = UpdateOptions(options, globals);
		}
	}

	private static ScriptOptions UpdateOptions(ScriptOptions options, InteractiveScriptGlobals globals)
	{
		RuntimeMetadataReferenceResolver runtimeMetadataReferenceResolver = (RuntimeMetadataReferenceResolver)options.MetadataResolver;
		CommonCompiler.LoggingSourceFileResolver loggingSourceFileResolver = (CommonCompiler.LoggingSourceFileResolver)options.SourceResolver;
		string currentDirectory = Directory.GetCurrentDirectory();
		ImmutableArray<string> searchPaths = ImmutableArray.CreateRange(globals.ReferencePaths);
		ImmutableArray<string> value = ImmutableArray.CreateRange(globals.SourcePaths);
		return options.RemoveImportsAndReferences().WithMetadataResolver(runtimeMetadataReferenceResolver.WithRelativePathResolver(runtimeMetadataReferenceResolver.PathResolver.WithBaseDirectory(currentDirectory).WithSearchPaths(searchPaths))).WithSourceResolver(loggingSourceFileResolver.WithBaseDirectory(currentDirectory).WithSearchPaths(value));
	}

	private void DisplayException(Exception e)
	{
		try
		{
			Console.SetForegroundColor(ConsoleColor.Red);
			if (e is FileLoadException && e.InnerException is InteractiveAssemblyLoaderException)
			{
				Console.Error.WriteLine(e.InnerException.Message);
			}
			else
			{
				Console.Error.Write(_objectFormatter.FormatException(e));
			}
		}
		finally
		{
			Console.ResetColor();
		}
	}

	private static bool IsHelpCommand(string text)
	{
		return text.Trim() == "#help";
	}

	private void DisplayHelpText()
	{
		Console.Out.Write(ScriptingResources.HelpText);
		Console.Out.WriteLine();
	}

	private void DisplayDiagnostics(ImmutableArray<Diagnostic> diagnostics)
	{
		IOrderedEnumerable<Diagnostic> source = diagnostics.OrderBy(delegate(Diagnostic d1, Diagnostic d2)
		{
			int num2 = d2.Severity - d1.Severity;
			return (num2 == 0) ? (d1.Location.SourceSpan.Start - d2.Location.SourceSpan.Start) : num2;
		});
		try
		{
			foreach (Diagnostic item in source.Take(5))
			{
				Console.SetForegroundColor((item.Severity == DiagnosticSeverity.Error) ? ConsoleColor.Red : ConsoleColor.Yellow);
				Console.Error.WriteLine(item.ToString());
			}
			if (diagnostics.Length > 5)
			{
				int num = diagnostics.Length - 5;
				Console.SetForegroundColor(ConsoleColor.DarkRed);
				Console.Error.WriteLine(string.Format(ScriptingResources.PlusAdditionalError, num));
			}
		}
		finally
		{
			Console.ResetColor();
		}
	}
}
