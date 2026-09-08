using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

public class CSharpCommandLineParser : CommandLineParser
{
	private static readonly char[] s_quoteOrEquals = new char[2] { '"', '=' };

	private static readonly char[] s_warningSeparators = new char[3] { ',', ';', ' ' };

	public static CSharpCommandLineParser Default { get; } = new CSharpCommandLineParser();

	public static CSharpCommandLineParser Script { get; } = new CSharpCommandLineParser(isScriptCommandLineParser: true);

	protected override string RegularFileExtension => ".cs";

	protected override string ScriptFileExtension => ".csx";

	internal CSharpCommandLineParser(bool isScriptCommandLineParser = false)
		: base(Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance, isScriptCommandLineParser)
	{
	}

	internal sealed override CommandLineArguments CommonParse(IEnumerable<string> args, string baseDirectory, string? sdkDirectory, string? additionalReferenceDirectories)
	{
		return Parse(args, baseDirectory, sdkDirectory, additionalReferenceDirectories);
	}

	public new CSharpCommandLineArguments Parse(IEnumerable<string> args, string? baseDirectory, string? sdkDirectory, string? additionalReferenceDirectories = null)
	{
		List<Diagnostic> list = new List<Diagnostic>();
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance();
		List<string> list2 = (IsScriptCommandLineParser ? new List<string>() : null);
		List<string> list3 = (IsScriptCommandLineParser ? new List<string>() : null);
		FlattenArgs(args, list, instance, list2, baseDirectory, list3);
		string appConfigPath = null;
		bool displayLogo = true;
		bool displayHelp = false;
		bool displayVersion = false;
		bool displayLangVersions = false;
		bool flag = false;
		bool flag2 = false;
		NullableContextOptions nullableContextOptions = NullableContextOptions.Disable;
		bool flag3 = false;
		bool flag4 = true;
		bool flag5 = false;
		bool flag6 = false;
		DebugInformationFormat debugInformationFormat = ((!PathUtilities.IsUnixLikePlatform) ? DebugInformationFormat.Pdb : DebugInformationFormat.PortablePdb);
		bool flag7 = false;
		string pdbPath = null;
		bool flag8 = IsScriptCommandLineParser;
		bool flag9 = false;
		string outputDirectory = baseDirectory;
		ImmutableArray<KeyValuePair<string, string>> immutableArray = ImmutableArray<KeyValuePair<string, string>>.Empty;
		string outputFileName = null;
		string text = null;
		bool flag10 = false;
		string generatedFilesOutputDirectory = null;
		string documentationPath = null;
		ErrorLogOptions errorLogOptions = null;
		bool flag11 = false;
		bool utf8Output = false;
		OutputKind outputKind = OutputKind.ConsoleApplication;
		SubsystemVersion subsystemVersion = SubsystemVersion.None;
		LanguageVersion result = LanguageVersion.Default;
		string text2 = null;
		string text3 = null;
		string win32ResourceFile = null;
		string text4 = null;
		bool noWin32Manifest = false;
		Platform platform = Platform.AnyCpu;
		ulong num = 0uL;
		int fileAlignment = 0;
		bool? flag12 = null;
		string text5 = null;
		string text6 = null;
		List<CommandLineResource> list4 = new List<CommandLineResource>();
		List<CommandLineSourceFile> list5 = new List<CommandLineSourceFile>();
		List<CommandLineSourceFile> list6 = new List<CommandLineSourceFile>();
		ArrayBuilder<string> instance2 = ArrayBuilder<string>.GetInstance();
		List<CommandLineSourceFile> list7 = new List<CommandLineSourceFile>();
		bool flag13 = false;
		bool flag14 = false;
		bool flag15 = false;
		Encoding encoding = null;
		SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha256;
		ArrayBuilder<string> instance3 = ArrayBuilder<string>.GetInstance();
		List<CommandLineReference> list8 = new List<CommandLineReference>();
		List<CommandLineAnalyzerReference> list9 = new List<CommandLineAnalyzerReference>();
		List<string> list10 = new List<string>();
		List<string> list11 = new List<string>();
		List<string> list12 = new List<string>();
		List<string> list13 = new List<string>();
		ReportDiagnostic reportDiagnostic = ReportDiagnostic.Default;
		Dictionary<string, ReportDiagnostic> diagnosticOptions = new Dictionary<string, ReportDiagnostic>();
		Dictionary<string, ReportDiagnostic> dictionary = new Dictionary<string, ReportDiagnostic>();
		Dictionary<string, ReportDiagnostic> dictionary2 = new Dictionary<string, ReportDiagnostic>();
		int num2 = 4;
		bool flag16 = false;
		bool printFullPaths = false;
		string moduleAssemblyName = null;
		string moduleName = null;
		List<string> list14 = new List<string>();
		string runtimeMetadataVersion = null;
		bool shouldIncludeErrorEndLocation = false;
		bool reportAnalyzer = false;
		bool skipAnalyzers = false;
		ArrayBuilder<InstrumentationKind> instance4 = ArrayBuilder<InstrumentationKind>.GetInstance();
		CultureInfo cultureInfo = null;
		string touchedFilesPath = null;
		bool flag17 = false;
		bool flag18 = false;
		bool flag19 = false;
		string text7 = null;
		string text8 = null;
		bool reportInternalsVisibleToAttributes = false;
		if (!IsScriptCommandLineParser)
		{
			foreach (string item in instance)
			{
				if (CommandLineParser.IsOption("ruleset", item, out var name, out var value))
				{
					string text9 = CommandLineParser.RemoveQuotesAndSlashes(value);
					if (RoslynString.IsNullOrEmpty(text9))
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, "<text>", name.ToString());
					}
					else
					{
						text8 = ParseGenericPathToFile(text9, list, baseDirectory);
						reportDiagnostic = GetDiagnosticOptionsFromRulesetFile(text8, out diagnosticOptions, list);
					}
				}
			}
		}
		foreach (string item2 in instance)
		{
			if (flag17 || !CommandLineParser.TryParseOption(item2, out ReadOnlyMemory<char> name2, out ReadOnlyMemory<char>? valueMemory))
			{
				ArrayBuilder<string> instance5 = ArrayBuilder<string>.GetInstance();
				ParseFileArgument(System.MemoryExtensions.AsMemory(item2), baseDirectory, instance5, list);
				foreach (string item3 in instance5)
				{
					list5.Add(ToCommandLineSourceFile(item3));
				}
				instance5.Free();
				if (list5.Count > 0)
				{
					flag13 = true;
				}
				continue;
			}
			if (CommandLineParser.IsOptionName("r", "reference", name2))
			{
				ParseAssemblyReferences(item2, valueMemory, list, embedInteropTypes: false, list8);
				continue;
			}
			if (CommandLineParser.IsOptionName("langversion", name2))
			{
				string text10 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
				if (RoslynString.IsNullOrEmpty(text10))
				{
					AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, MessageID.IDS_Text.Localize(), "/langversion:");
				}
				else if (text10.StartsWith("0", StringComparison.Ordinal))
				{
					AddDiagnostic(list, ErrorCode.ERR_LanguageVersionCannotHaveLeadingZeroes, text10);
				}
				else if (text10 == "?")
				{
					displayLangVersions = true;
				}
				else if (!LanguageVersionFacts.TryParse(text10, out result))
				{
					AddDiagnostic(list, ErrorCode.ERR_BadCompatMode, text10);
				}
				continue;
			}
			if (!IsScriptCommandLineParser && CommandLineParser.IsOptionName("a", "analyzer", name2))
			{
				ParseAnalyzers(item2, valueMemory, list9, list);
				continue;
			}
			if (!IsScriptCommandLineParser && CommandLineParser.IsOptionName("nowarn", name2))
			{
				if (!valueMemory.HasValue)
				{
					AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsNumber, name2.ToString());
				}
				else if (valueMemory.Value.Length == 0)
				{
					AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsNumber, name2.ToString());
				}
				else
				{
					AddWarnings(dictionary, ReportDiagnostic.Suppress, valueMemory.Value);
				}
				continue;
			}
			string text11 = name2.Span.ToString().ToLowerInvariant();
			switch (text11)
			{
			case "?":
			case "help":
				displayHelp = true;
				continue;
			case "version":
				displayVersion = true;
				continue;
			case "features":
			{
				string text10 = valueMemoryString();
				if (text10 == null)
				{
					list14.Clear();
				}
				else
				{
					list14.Add(text10.Unquote());
				}
				continue;
			}
			case "libpath":
			case "libpaths":
			case "lib":
				ParseAndResolveReferencePaths(text11, valueMemory, baseDirectory, list10, MessageID.IDS_LIB_OPTION, list);
				continue;
			}
			if (IsScriptCommandLineParser)
			{
				string text10 = valueMemoryString();
				switch (text11)
				{
				case "-":
					if (text10 != null)
					{
						break;
					}
					if (item2 == "-")
					{
						if (Console.IsInputRedirected)
						{
							list5.Add(new CommandLineSourceFile("-", isScript: true, isInputRedirected: true));
							flag13 = true;
						}
						else
						{
							AddDiagnostic(list, ErrorCode.ERR_StdInOptionProvidedButConsoleInputIsNotRedirected);
						}
					}
					else
					{
						flag17 = true;
					}
					continue;
				case "i":
				case "i+":
					if (text10 != null)
					{
						break;
					}
					flag18 = true;
					continue;
				case "i-":
					if (text10 != null)
					{
						break;
					}
					flag18 = false;
					continue;
				case "loadpath":
				case "loadpaths":
					ParseAndResolveReferencePaths(text11, valueMemory, baseDirectory, list11, MessageID.IDS_REFERENCEPATH_OPTION, list);
					continue;
				case "u":
				case "import":
				case "usings":
				case "using":
				case "imports":
					list13.AddRange(ParseUsings(item2, text10, list));
					continue;
				}
			}
			else
			{
				switch (text11)
				{
				case "d":
				case "define":
					if (!valueMemory.HasValue || valueMemory.GetValueOrDefault().Length <= 0)
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, "<text>", item2);
					}
					else
					{
						ParseConditionalCompilationSymbols(CommandLineParser.RemoveQuotesAndSlashesEx(valueMemory.Value), instance3, out IEnumerable<Diagnostic> diagnostics);
						list.AddRange(diagnostics);
					}
					continue;
				case "codepage":
				{
					string text10 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					if (text10 == null)
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, "<text>", text11);
						continue;
					}
					Encoding encoding2 = CommandLineParser.TryParseEncodingName(text10);
					if (encoding2 == null)
					{
						AddDiagnostic(list, ErrorCode.FTL_BadCodepage, text10);
					}
					else
					{
						encoding = encoding2;
					}
					continue;
				}
				case "checksumalgorithm":
				{
					string text10 = valueMemoryString();
					if (string.IsNullOrEmpty(text10))
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, "<text>", text11);
						continue;
					}
					SourceHashAlgorithm sourceHashAlgorithm = CommandLineParser.TryParseHashAlgorithmName(text10);
					if (sourceHashAlgorithm == SourceHashAlgorithm.None)
					{
						AddDiagnostic(list, ErrorCode.FTL_BadChecksumAlgorithm, text10);
					}
					else
					{
						checksumAlgorithm = sourceHashAlgorithm;
					}
					continue;
				}
				case "checked+":
				case "checked":
					if (valueMemory.HasValue)
					{
						break;
					}
					flag2 = true;
					continue;
				case "checked-":
					if (valueMemory.HasValue)
					{
						break;
					}
					flag2 = false;
					continue;
				case "nullable":
				{
					string text10 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					if (text10 != null)
					{
						if (text10.IsEmpty())
						{
							AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, MessageID.IDS_Text.Localize(), text11);
							continue;
						}
						switch (text10.ToLower())
						{
						case "disable":
							nullableContextOptions = NullableContextOptions.Disable;
							break;
						case "enable":
							nullableContextOptions = NullableContextOptions.Enable;
							break;
						case "warnings":
							nullableContextOptions = NullableContextOptions.Warnings;
							break;
						case "annotations":
							nullableContextOptions = NullableContextOptions.Annotations;
							break;
						default:
							AddDiagnostic(list, ErrorCode.ERR_BadNullableContextOption, text10);
							break;
						}
					}
					else
					{
						nullableContextOptions = NullableContextOptions.Enable;
					}
					continue;
				}
				case "nullable+":
					if (valueMemory.HasValue)
					{
						break;
					}
					nullableContextOptions = NullableContextOptions.Enable;
					continue;
				case "nullable-":
					if (valueMemory.HasValue)
					{
						break;
					}
					nullableContextOptions = NullableContextOptions.Disable;
					continue;
				case "instrument":
				{
					string text10 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					if (RoslynString.IsNullOrEmpty(text10))
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, "<text>", text11);
						continue;
					}
					foreach (InstrumentationKind item4 in ParseInstrumentationKinds(text10, list))
					{
						if (!instance4.Contains(item4))
						{
							instance4.Add(item4);
						}
					}
					continue;
				}
				case "sqmsessionguid":
				{
					string text10 = valueMemoryString();
					Guid result4;
					if (text10 == null)
					{
						AddDiagnostic(list, ErrorCode.ERR_MissingGuidForOption, "<text>", text11);
					}
					else if (!Guid.TryParse(text10, out result4))
					{
						AddDiagnostic(list, ErrorCode.ERR_InvalidFormatForGuidForOption, text10, text11);
					}
					continue;
				}
				case "preferreduilang":
				{
					string text10 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					if (RoslynString.IsNullOrEmpty(text10))
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, "<text>", item2);
						continue;
					}
					try
					{
						cultureInfo = new CultureInfo(text10);
						if ((cultureInfo.CultureTypes & CultureTypes.UserCustomCulture) != 0)
						{
							cultureInfo = null;
						}
					}
					catch (CultureNotFoundException)
					{
					}
					if (cultureInfo == null)
					{
						AddDiagnostic(list, ErrorCode.WRN_BadUILang, text10);
					}
					continue;
				}
				case "nosdkpath":
					flag9 = true;
					continue;
				case "sdkpath":
					flag9 = false;
					if (!valueMemory.HasValue || valueMemory.GetValueOrDefault().Length <= 0)
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, "<path>", text11);
					}
					else
					{
						sdkDirectory = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					}
					continue;
				case "out":
				{
					string text10 = valueMemoryString();
					if (RoslynString.IsNullOrWhiteSpace(text10))
					{
						AddDiagnostic(list, ErrorCode.ERR_NoFileSpec, item2);
					}
					else
					{
						ParseOutputFile(text10, list, baseDirectory, out outputFileName, out outputDirectory);
					}
					continue;
				}
				case "refout":
				{
					string text10 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					if (RoslynString.IsNullOrEmpty(text10))
					{
						AddDiagnostic(list, ErrorCode.ERR_NoFileSpec, item2);
					}
					else
					{
						text = ParseGenericPathToFile(text10, list, baseDirectory);
					}
					continue;
				}
				case "refonly":
					if (valueMemory.HasValue)
					{
						break;
					}
					flag10 = true;
					continue;
				case "t":
				case "target":
				{
					string text10 = valueMemoryString();
					if (text10 == null)
					{
						break;
					}
					if (RoslynString.IsNullOrEmpty(text10))
					{
						AddDiagnostic(list, ErrorCode.FTL_InvalidTarget);
					}
					else
					{
						outputKind = ParseTarget(text10, list);
					}
					continue;
				}
				case "moduleassemblyname":
				{
					string text10 = valueMemoryString()?.Unquote();
					if (RoslynString.IsNullOrEmpty(text10))
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, "<text>", item2);
					}
					else if (!MetadataHelpers.IsValidAssemblyOrModuleName(text10))
					{
						AddDiagnostic(list, ErrorCode.ERR_InvalidAssemblyName, "<text>", item2);
					}
					else
					{
						moduleAssemblyName = text10;
					}
					continue;
				}
				case "modulename":
				{
					string text13 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					if (string.IsNullOrEmpty(text13))
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, MessageID.IDS_Text.Localize(), "modulename");
					}
					else
					{
						moduleName = text13;
					}
					continue;
				}
				case "platform":
				{
					string text10 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					if (RoslynString.IsNullOrEmpty(text10))
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, "<string>", item2);
					}
					else
					{
						platform = ParsePlatform(text10, list);
					}
					continue;
				}
				case "recurse":
				{
					string text10 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					if (text10 == null)
					{
						break;
					}
					if (RoslynString.IsNullOrEmpty(text10))
					{
						AddDiagnostic(list, ErrorCode.ERR_NoFileSpec, item2);
						continue;
					}
					int count = list5.Count;
					list5.AddRange(ParseRecurseArgument(text10, baseDirectory, list));
					if (list5.Count > count)
					{
						flag13 = true;
					}
					continue;
				}
				case "generatedfilesout":
				{
					string text10 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					if (string.IsNullOrWhiteSpace(text10))
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, MessageID.IDS_Text.Localize(), item2);
					}
					else
					{
						generatedFilesOutputDirectory = ParseGenericPathToFile(text10, list, baseDirectory);
					}
					continue;
				}
				case "doc":
				{
					flag11 = true;
					string text10 = valueMemoryString();
					if (RoslynString.IsNullOrEmpty(text10))
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, MessageID.IDS_Text.Localize(), item2);
						continue;
					}
					string text12 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					if (RoslynString.IsNullOrEmpty(text12))
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, MessageID.IDS_Text.Localize(), "/doc:");
					}
					else
					{
						documentationPath = ParseGenericPathToFile(text12, list, baseDirectory);
					}
					continue;
				}
				case "addmodule":
				{
					string text10 = valueMemoryString();
					if (text10 == null)
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, MessageID.IDS_Text.Localize(), "/addmodule:");
					}
					else if (text10.Length == 0)
					{
						AddDiagnostic(list, ErrorCode.ERR_NoFileSpec, item2);
					}
					else
					{
						list8.AddRange(from path in CommandLineParser.ParseSeparatedPaths(text10)
							select new CommandLineReference(path, MetadataReferenceProperties.Module));
						flag15 = true;
					}
					continue;
				}
				case "l":
				case "link":
					ParseAssemblyReferences(item2, valueMemory, list, embedInteropTypes: true, list8);
					continue;
				case "win32res":
					win32ResourceFile = GetWin32Setting(item2, valueMemoryString(), list);
					continue;
				case "win32icon":
					text4 = GetWin32Setting(item2, valueMemoryString(), list);
					continue;
				case "win32manifest":
					text3 = GetWin32Setting(item2, valueMemoryString(), list);
					noWin32Manifest = false;
					continue;
				case "nowin32manifest":
					noWin32Manifest = true;
					text3 = null;
					continue;
				case "resource":
				case "res":
				{
					if (!valueMemory.HasValue)
					{
						break;
					}
					if (TryParseResourceDescription(item2, valueMemory.Value, baseDirectory, list, isEmbedded: true, out var resource2))
					{
						list4.Add(resource2);
						flag15 = true;
					}
					continue;
				}
				case "linkres":
				case "linkresource":
				{
					if (!valueMemory.HasValue)
					{
						break;
					}
					if (TryParseResourceDescription(item2, valueMemory.Value, baseDirectory, list, isEmbedded: false, out var resource))
					{
						list4.Add(resource);
						flag15 = true;
					}
					continue;
				}
				case "sourcelink":
				{
					string text10 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					if (RoslynString.IsNullOrEmpty(text10))
					{
						AddDiagnostic(list, ErrorCode.ERR_NoFileSpec, item2);
					}
					else
					{
						text7 = ParseGenericPathToFile(text10, list, baseDirectory);
					}
					continue;
				}
				case "debug":
				{
					flag6 = true;
					string text10 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					if (text10 == null)
					{
						continue;
					}
					if (text10.IsEmpty())
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, MessageID.IDS_Text.Localize(), text11);
						continue;
					}
					switch (text10.ToLower())
					{
					case "full":
					case "pdbonly":
						debugInformationFormat = ((!PathUtilities.IsUnixLikePlatform) ? DebugInformationFormat.Pdb : DebugInformationFormat.PortablePdb);
						break;
					case "portable":
						debugInformationFormat = DebugInformationFormat.PortablePdb;
						break;
					case "embedded":
						debugInformationFormat = DebugInformationFormat.Embedded;
						break;
					default:
						AddDiagnostic(list, ErrorCode.ERR_BadDebugType, text10);
						break;
					}
					continue;
				}
				case "debug+":
					if (valueMemory.HasValue)
					{
						break;
					}
					flag6 = true;
					flag7 = true;
					continue;
				case "debug-":
					if (valueMemory.HasValue)
					{
						break;
					}
					flag6 = false;
					flag7 = false;
					continue;
				case "o":
				case "optimize":
				case "optimize+":
				case "o+":
					if (valueMemory.HasValue)
					{
						break;
					}
					flag = true;
					continue;
				case "optimize-":
				case "o-":
					if (valueMemory.HasValue)
					{
						break;
					}
					flag = false;
					continue;
				case "deterministic+":
				case "deterministic":
					if (valueMemory.HasValue)
					{
						break;
					}
					flag5 = true;
					continue;
				case "deterministic-":
					if (valueMemory.HasValue)
					{
						break;
					}
					flag5 = false;
					continue;
				case "p":
				case "parallel":
				case "parallel+":
				case "p+":
					if (valueMemory.HasValue)
					{
						break;
					}
					flag4 = true;
					continue;
				case "parallel-":
				case "p-":
					if (valueMemory.HasValue)
					{
						break;
					}
					flag4 = false;
					continue;
				case "warnaserror+":
				case "warnaserror":
					if (!valueMemory.HasValue)
					{
						reportDiagnostic = ReportDiagnostic.Error;
						dictionary2.Clear();
						foreach (string key in diagnosticOptions.Keys)
						{
							if (diagnosticOptions[key] == ReportDiagnostic.Warn)
							{
								dictionary2[key] = ReportDiagnostic.Error;
							}
						}
					}
					else if (valueMemory.Value.Length == 0)
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsNumber, text11);
					}
					else
					{
						AddWarnings(dictionary2, ReportDiagnostic.Error, valueMemory.Value);
					}
					continue;
				case "warnaserror-":
				{
					if (!valueMemory.HasValue)
					{
						reportDiagnostic = ReportDiagnostic.Default;
						dictionary2.Clear();
						continue;
					}
					if (!valueMemory.HasValue || valueMemory.GetValueOrDefault().Length <= 0)
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsNumber, text11);
						continue;
					}
					ArrayBuilder<string> instance6 = ArrayBuilder<string>.GetInstance();
					ParseWarnings(valueMemory.Value, instance6);
					foreach (string item5 in instance6)
					{
						if (diagnosticOptions.TryGetValue(item5, out var value2))
						{
							dictionary2[item5] = value2;
						}
						else
						{
							dictionary2[item5] = ReportDiagnostic.Default;
						}
					}
					instance6.Free();
					continue;
				}
				case "w":
				case "warn":
				{
					string text10 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					int result2;
					if (text10 == null)
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsNumber, text11);
					}
					else if (string.IsNullOrEmpty(text10) || !int.TryParse(text10, NumberStyles.Integer, CultureInfo.InvariantCulture, out result2))
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsNumber, text11);
					}
					else if (result2 < 0)
					{
						AddDiagnostic(list, ErrorCode.ERR_BadWarningLevel);
					}
					else
					{
						num2 = result2;
					}
					continue;
				}
				case "unsafe":
				case "unsafe+":
					if (valueMemory.HasValue)
					{
						break;
					}
					flag3 = true;
					continue;
				case "unsafe-":
					if (valueMemory.HasValue)
					{
						break;
					}
					flag3 = false;
					continue;
				case "delaysign":
				case "delaysign+":
					if (valueMemory.HasValue)
					{
						break;
					}
					flag12 = true;
					continue;
				case "delaysign-":
					if (valueMemory.HasValue)
					{
						break;
					}
					flag12 = false;
					continue;
				case "publicsign":
				case "publicsign+":
					if (valueMemory.HasValue)
					{
						break;
					}
					flag19 = true;
					continue;
				case "publicsign-":
					if (valueMemory.HasValue)
					{
						break;
					}
					flag19 = false;
					continue;
				case "keyfile":
				{
					string text10 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					if (string.IsNullOrEmpty(text10))
					{
						AddDiagnostic(list, ErrorCode.ERR_NoFileSpec, "keyfile");
					}
					else
					{
						text5 = text10;
					}
					continue;
				}
				case "keycontainer":
				{
					string text10 = valueMemoryString();
					if (string.IsNullOrEmpty(text10))
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, MessageID.IDS_Text.Localize(), "keycontainer");
					}
					else
					{
						text6 = text10;
					}
					continue;
				}
				case "highentropyva+":
				case "highentropyva":
					if (valueMemory.HasValue)
					{
						break;
					}
					flag16 = true;
					continue;
				case "highentropyva-":
					if (valueMemory.HasValue)
					{
						break;
					}
					flag16 = false;
					continue;
				case "nologo":
					displayLogo = false;
					continue;
				case "baseaddress":
				{
					string text10 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					if (string.IsNullOrEmpty(text10) || !CommandLineParser.TryParseUInt64(text10, out var result5))
					{
						if (RoslynString.IsNullOrEmpty(text10))
						{
							AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsNumber, text11);
						}
						else
						{
							AddDiagnostic(list, ErrorCode.ERR_BadBaseNumber, text10);
						}
					}
					else
					{
						num = result5;
					}
					continue;
				}
				case "subsystemversion":
				{
					string text10 = valueMemoryString();
					if (RoslynString.IsNullOrEmpty(text10))
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, MessageID.IDS_Text.Localize(), "subsystemversion");
						continue;
					}
					SubsystemVersion version = SubsystemVersion.None;
					if (SubsystemVersion.TryParse(text10, out version))
					{
						subsystemVersion = version;
						continue;
					}
					AddDiagnostic(list, ErrorCode.ERR_InvalidSubsystemVersion, text10);
					continue;
				}
				case "touchedfiles":
				{
					string text12 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					if (string.IsNullOrEmpty(text12))
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, MessageID.IDS_Text.Localize(), "touchedfiles");
					}
					else
					{
						touchedFilesPath = text12;
					}
					continue;
				}
				case "bugreport":
					UnimplementedSwitch(list, text11);
					continue;
				case "utf8output":
					if (valueMemory.HasValue)
					{
						break;
					}
					utf8Output = true;
					continue;
				case "m":
				case "main":
				{
					string text12 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					if (string.IsNullOrEmpty(text12))
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, "<text>", text11);
					}
					else
					{
						text2 = text12;
					}
					continue;
				}
				case "fullpaths":
					if (valueMemory.HasValue)
					{
						break;
					}
					printFullPaths = true;
					continue;
				case "pathmap":
				{
					string text12 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					if (text12 == null)
					{
						break;
					}
					immutableArray = immutableArray.Concat<KeyValuePair<string, string>>(ParsePathMap(text12, list));
					continue;
				}
				case "filealign":
				{
					string text10 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					ushort result3;
					if (RoslynString.IsNullOrEmpty(text10))
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsNumber, text11);
					}
					else if (!CommandLineParser.TryParseUInt16(text10, out result3))
					{
						AddDiagnostic(list, ErrorCode.ERR_InvalidFileAlignment, text10);
					}
					else if (!CompilationOptions.IsValidFileAlignment(result3))
					{
						AddDiagnostic(list, ErrorCode.ERR_InvalidFileAlignment, text10);
					}
					else
					{
						fileAlignment = result3;
					}
					continue;
				}
				case "pdb":
				{
					string text10 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					if (RoslynString.IsNullOrEmpty(text10))
					{
						AddDiagnostic(list, ErrorCode.ERR_NoFileSpec, item2);
					}
					else
					{
						pdbPath = ParsePdbPath(text10, list, baseDirectory);
					}
					continue;
				}
				case "errorendlocation":
					shouldIncludeErrorEndLocation = true;
					continue;
				case "reportanalyzer":
					reportAnalyzer = true;
					continue;
				case "skipanalyzers+":
				case "skipanalyzers":
					if (valueMemory.HasValue)
					{
						break;
					}
					skipAnalyzers = true;
					continue;
				case "skipanalyzers-":
					if (valueMemory.HasValue)
					{
						break;
					}
					skipAnalyzers = false;
					continue;
				case "nostdlib":
				case "nostdlib+":
					if (valueMemory.HasValue)
					{
						break;
					}
					flag8 = true;
					continue;
				case "nostdlib-":
					if (valueMemory.HasValue)
					{
						break;
					}
					flag8 = false;
					continue;
				case "errorlog":
				{
					valueMemory = CommandLineParser.RemoveQuotesAndSlashesEx(valueMemory);
					if (!valueMemory.HasValue || valueMemory.GetValueOrDefault().Length <= 0)
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, "<file>[,version={1|1.0|2|2.1}]", CommandLineParser.RemoveQuotesAndSlashes(item2));
						continue;
					}
					errorLogOptions = ParseErrorLogOptions(valueMemory.Value, list, baseDirectory, out var diagnosticAlreadyReported);
					if (errorLogOptions == null && !diagnosticAlreadyReported)
					{
						AddDiagnostic(list, ErrorCode.ERR_BadSwitchValue, valueMemory.Value.ToString(), "/errorlog:", "<file>[,version={1|1.0|2|2.1}]");
					}
					continue;
				}
				case "appconfig":
				{
					string text12 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					if (RoslynString.IsNullOrEmpty(text12))
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, ":<text>", CommandLineParser.RemoveQuotesAndSlashes(item2));
					}
					else
					{
						appConfigPath = ParseGenericPathToFile(text12, list, baseDirectory);
					}
					continue;
				}
				case "runtimemetadataversion":
				{
					string text12 = CommandLineParser.RemoveQuotesAndSlashes(valueMemory);
					if (string.IsNullOrEmpty(text12))
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, "<text>", text11);
					}
					else
					{
						runtimeMetadataVersion = text12;
					}
					continue;
				}
				case "additionalfile":
				{
					if (!valueMemory.HasValue || valueMemory.GetValueOrDefault().Length <= 0)
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, "<file list>", text11);
						continue;
					}
					ArrayBuilder<string> instance5 = ArrayBuilder<string>.GetInstance();
					ParseSeparatedFileArgument(valueMemory.Value, baseDirectory, instance5, list);
					foreach (string item6 in instance5)
					{
						list6.Add(ToCommandLineSourceFile(item6));
					}
					instance5.Free();
					continue;
				}
				case "analyzerconfig":
					if (!valueMemory.HasValue || valueMemory.GetValueOrDefault().Length <= 0)
					{
						AddDiagnostic(list, ErrorCode.ERR_SwitchNeedsString, "<file list>", text11);
					}
					else
					{
						ArrayBuilder<string> instance5 = ArrayBuilder<string>.GetInstance();
						ParseSeparatedFileArgument(valueMemory.Value, baseDirectory, instance5, list);
						instance2.AddRange(instance5);
						instance5.Free();
					}
					continue;
				case "embed":
				{
					string text10 = valueMemoryString();
					if (RoslynString.IsNullOrEmpty(text10))
					{
						flag14 = true;
						continue;
					}
					ArrayBuilder<string> instance5 = ArrayBuilder<string>.GetInstance();
					ParseSeparatedFileArgument(System.MemoryExtensions.AsMemory(text10), baseDirectory, instance5, list);
					foreach (string item7 in instance5)
					{
						list7.Add(ToCommandLineSourceFile(item7));
					}
					instance5.Free();
					continue;
				}
				case "-":
					if (Console.IsInputRedirected)
					{
						list5.Add(new CommandLineSourceFile("-", isScript: false, isInputRedirected: true));
						flag13 = true;
					}
					else
					{
						AddDiagnostic(list, ErrorCode.ERR_StdInOptionProvidedButConsoleInputIsNotRedirected);
					}
					continue;
				case "reportivts":
				case "reportivts+":
					if (valueMemory.HasValue)
					{
						break;
					}
					reportInternalsVisibleToAttributes = true;
					continue;
				case "reportivts-":
					if (valueMemory.HasValue)
					{
						break;
					}
					reportInternalsVisibleToAttributes = false;
					continue;
				case "noconfig":
				case "ruleset":
				case "errorreport":
					continue;
				}
			}
			AddDiagnostic(list, ErrorCode.ERR_BadSwitch, item2);
			string? valueMemoryString()
			{
				if (!valueMemory.HasValue)
				{
					return null;
				}
				return valueMemory.GetValueOrDefault().Span.ToString();
			}
		}
		foreach (KeyValuePair<string, ReportDiagnostic> item8 in dictionary2)
		{
			diagnosticOptions[item8.Key] = item8.Value;
		}
		foreach (KeyValuePair<string, ReportDiagnostic> item9 in dictionary)
		{
			diagnosticOptions[item9.Key] = item9.Value;
		}
		if (flag10 && text != null)
		{
			AddDiagnostic(list, diagnosticOptions, ErrorCode.ERR_NoRefOutWhenRefOnly);
		}
		if (outputKind == OutputKind.NetModule && (flag10 || text != null))
		{
			AddDiagnostic(list, diagnosticOptions, ErrorCode.ERR_NoNetModuleOutputWhenRefOutOrRefOnly);
		}
		if (!IsScriptCommandLineParser && !flag13 && (outputKind.IsNetModule() || !flag15))
		{
			AddDiagnostic(list, diagnosticOptions, ErrorCode.WRN_NoSources);
		}
		if (flag9)
		{
			sdkDirectory = null;
		}
		if (!flag8 && sdkDirectory != null)
		{
			list8.Insert(0, new CommandLineReference(Path.Combine(sdkDirectory, "mscorlib.dll"), MetadataReferenceProperties.Assembly));
		}
		if (!platform.Requires64Bit() && num > 4294934527u)
		{
			AddDiagnostic(list, ErrorCode.ERR_BadBaseNumber, $"0x{num:X}");
			num = 0uL;
		}
		if (!string.IsNullOrEmpty(additionalReferenceDirectories))
		{
			ParseAndResolveReferencePaths(null, System.MemoryExtensions.AsMemory(additionalReferenceDirectories), baseDirectory, list10, MessageID.IDS_LIB_ENV, list);
		}
		ImmutableArray<string> referencePaths = BuildSearchPaths(sdkDirectory, list10, list3);
		ValidateWin32Settings(win32ResourceFile, text4, text3, outputKind, list);
		if (!RoslynString.IsNullOrEmpty(baseDirectory))
		{
			list12.Add(baseDirectory);
		}
		if (RoslynString.IsNullOrEmpty(outputDirectory))
		{
			AddDiagnostic(list, ErrorCode.ERR_NoOutputDirectory);
		}
		else if (baseDirectory != outputDirectory)
		{
			list12.Add(outputDirectory);
		}
		if (flag19 && !RoslynString.IsNullOrEmpty(text5))
		{
			text5 = ParseGenericPathToFile(text5, list, baseDirectory);
		}
		if (text7 != null && !flag6)
		{
			AddDiagnostic(list, ErrorCode.ERR_SourceLinkRequiresPdb);
		}
		if (flag14)
		{
			list7.AddRange(list5);
		}
		if (list7.Count > 0 && !flag6)
		{
			AddDiagnostic(list, ErrorCode.ERR_CannotEmbedWithoutPdb);
		}
		ImmutableDictionary<string, string> features = CommandLineParser.ParseFeatures(list14);
		GetCompilationAndModuleNames(list, outputKind, list5, flag13, moduleAssemblyName, ref outputFileName, ref moduleName, out string compilationName);
		instance.Free();
		CSharpParseOptions cSharpParseOptions = new CSharpParseOptions(result, preprocessorSymbols: instance3.ToImmutableAndFree(), documentationMode: flag11 ? DocumentationMode.Diagnose : DocumentationMode.None, kind: IsScriptCommandLineParser ? SourceCodeKind.Script : SourceCodeKind.Regular, features: features);
		bool reportSuppressedDiagnostics = errorLogOptions != null;
		OutputKind outputKind2 = outputKind;
		string moduleName2 = moduleName;
		string mainTypeName = text2;
		IEnumerable<string> usings = list13;
		OptimizationLevel optimizationLevel = (flag ? OptimizationLevel.Release : OptimizationLevel.Debug);
		bool checkOverflow = flag2;
		NullableContextOptions nullableContextOptions2 = nullableContextOptions;
		bool allowUnsafe = flag3;
		bool deterministic = flag5;
		bool concurrentBuild = flag4;
		string cryptoKeyContainer = text6;
		string cryptoKeyFile = text5;
		bool? delaySign = flag12;
		Platform platform2 = platform;
		ReportDiagnostic generalDiagnosticOption = reportDiagnostic;
		int warningLevel = num2;
		IEnumerable<KeyValuePair<string, ReportDiagnostic>> specificDiagnosticOptions = diagnosticOptions;
		bool publicSign = flag19;
		CSharpCompilationOptions cSharpCompilationOptions = new CSharpCompilationOptions(outputKind2, reportSuppressedDiagnostics, moduleName2, mainTypeName, "Script", usings, optimizationLevel, checkOverflow, allowUnsafe, cryptoKeyContainer, cryptoKeyFile, default(ImmutableArray<byte>), delaySign, platform2, generalDiagnosticOption, warningLevel, specificDiagnosticOptions, concurrentBuild, deterministic, null, null, null, null, null, publicSign, MetadataImportOptions.Public, nullableContextOptions2);
		if (flag7)
		{
			cSharpCompilationOptions = cSharpCompilationOptions.WithDebugPlusMode(flag7);
		}
		bool metadataOnly = flag10;
		publicSign = !flag10 && text == null;
		DebugInformationFormat debugInformationFormat2 = debugInformationFormat;
		ulong baseAddress = num;
		concurrentBuild = flag16;
		EmitOptions emitOptions = new EmitOptions(metadataOnly, debugInformationFormat2, null, null, fileAlignment, baseAddress, concurrentBuild, subsystemVersion, runtimeMetadataVersion, tolerateErrors: false, publicSign, instance4.ToImmutableAndFree(), HashAlgorithmName.SHA256, encoding);
		list.AddRange(cSharpCompilationOptions.Errors);
		list.AddRange(cSharpParseOptions.Errors);
		if (nullableContextOptions != NullableContextOptions.Disable && cSharpParseOptions.LanguageVersion < MessageID.IDS_FeatureNullableReferenceTypes.RequiredVersion())
		{
			list.Add(new CSDiagnostic(new CSDiagnosticInfo(ErrorCode.ERR_NullableOptionNotAvailable, "nullable", nullableContextOptions, cSharpParseOptions.LanguageVersion.ToDisplayString(), new CSharpRequiredLanguageVersion(MessageID.IDS_FeatureNullableReferenceTypes.RequiredVersion())), Location.None));
		}
		immutableArray = CommandLineParser.SortPathMap(immutableArray);
		return new CSharpCommandLineArguments
		{
			IsScriptRunner = IsScriptCommandLineParser,
			InteractiveMode = (flag18 || (IsScriptCommandLineParser && list5.Count == 0)),
			BaseDirectory = baseDirectory,
			PathMap = immutableArray,
			Errors = list.AsImmutable(),
			Utf8Output = utf8Output,
			CompilationName = compilationName,
			OutputFileName = outputFileName,
			OutputRefFilePath = text,
			PdbPath = pdbPath,
			EmitPdb = (flag6 && !flag10),
			SourceLink = text7,
			RuleSetPath = text8,
			OutputDirectory = outputDirectory,
			DocumentationPath = documentationPath,
			GeneratedFilesOutputDirectory = generatedFilesOutputDirectory,
			ErrorLogOptions = errorLogOptions,
			AppConfigPath = appConfigPath,
			SourceFiles = list5.AsImmutable(),
			Encoding = encoding,
			ChecksumAlgorithm = checksumAlgorithm,
			MetadataReferences = list8.AsImmutable(),
			AnalyzerReferences = list9.AsImmutable(),
			AnalyzerConfigPaths = instance2.ToImmutableAndFree(),
			AdditionalFiles = list6.AsImmutable(),
			ReferencePaths = referencePaths,
			SourcePaths = list11.AsImmutable(),
			KeyFileSearchPaths = list12.AsImmutable(),
			Win32ResourceFile = win32ResourceFile,
			Win32Icon = text4,
			Win32Manifest = text3,
			NoWin32Manifest = noWin32Manifest,
			DisplayLogo = displayLogo,
			DisplayHelp = displayHelp,
			DisplayVersion = displayVersion,
			DisplayLangVersions = displayLangVersions,
			ManifestResourceArguments = list4.AsImmutable(),
			CompilationOptions = cSharpCompilationOptions,
			ParseOptions = cSharpParseOptions,
			EmitOptions = emitOptions,
			ScriptArguments = list2.AsImmutableOrEmpty(),
			TouchedFilesPath = touchedFilesPath,
			PrintFullPaths = printFullPaths,
			ShouldIncludeErrorEndLocation = shouldIncludeErrorEndLocation,
			PreferredUILang = cultureInfo,
			ReportAnalyzer = reportAnalyzer,
			SkipAnalyzers = skipAnalyzers,
			EmbeddedFiles = list7.AsImmutable(),
			ReportInternalsVisibleToAttributes = reportInternalsVisibleToAttributes
		};
	}

	private static void ParseAndResolveReferencePaths(string? switchName, ReadOnlyMemory<char>? switchValue, string? baseDirectory, List<string> builder, MessageID origin, List<Diagnostic> diagnostics)
	{
		if (!switchValue.HasValue || switchValue.GetValueOrDefault().Length <= 0)
		{
			AddDiagnostic(diagnostics, ErrorCode.ERR_SwitchNeedsString, MessageID.IDS_PathList.Localize(), switchName);
			return;
		}
		foreach (string item in CommandLineParser.ParseSeparatedPaths(switchValue.Value.ToString()))
		{
			string text = FileUtilities.ResolveRelativePath(item, baseDirectory);
			if (text == null)
			{
				AddDiagnostic(diagnostics, ErrorCode.WRN_InvalidSearchPathDir, item, origin.Localize(), MessageID.IDS_DirectoryHasInvalidPath.Localize());
			}
			else if (!Directory.Exists(text))
			{
				AddDiagnostic(diagnostics, ErrorCode.WRN_InvalidSearchPathDir, item, origin.Localize(), MessageID.IDS_DirectoryDoesNotExist.Localize());
			}
			else
			{
				builder.Add(text);
			}
		}
	}

	private static string? GetWin32Setting(string arg, string? value, List<Diagnostic> diagnostics)
	{
		if (value == null)
		{
			AddDiagnostic(diagnostics, ErrorCode.ERR_NoFileSpec, arg);
		}
		else
		{
			string text = CommandLineParser.RemoveQuotesAndSlashes(value);
			if (!string.IsNullOrWhiteSpace(text))
			{
				return text;
			}
			AddDiagnostic(diagnostics, ErrorCode.ERR_NoFileSpec, arg);
		}
		return null;
	}

	private void GetCompilationAndModuleNames(List<Diagnostic> diagnostics, OutputKind outputKind, List<CommandLineSourceFile> sourceFiles, bool sourceFilesSpecified, string? moduleAssemblyName, ref string? outputFileName, ref string? moduleName, out string? compilationName)
	{
		string text;
		if (outputFileName == null)
		{
			if (!IsScriptCommandLineParser && !sourceFilesSpecified)
			{
				AddDiagnostic(diagnostics, ErrorCode.ERR_OutputNeedsName);
				text = null;
			}
			else if (outputKind.IsApplication())
			{
				text = null;
			}
			else
			{
				text = PathUtilities.RemoveExtension(PathUtilities.GetFileName(sourceFiles.FirstOrDefault().Path));
				outputFileName = text + outputKind.GetDefaultExtension();
				if (text.Length == 0 && !outputKind.IsNetModule())
				{
					AddDiagnostic(diagnostics, ErrorCode.FTL_InvalidInputFileName, outputFileName);
					text = (outputFileName = null);
				}
			}
		}
		else
		{
			text = PathUtilities.RemoveExtension(outputFileName);
			if (text.Length == 0)
			{
				AddDiagnostic(diagnostics, ErrorCode.FTL_InvalidInputFileName, outputFileName);
				text = (outputFileName = null);
			}
		}
		if (outputKind.IsNetModule())
		{
			compilationName = moduleAssemblyName;
		}
		else
		{
			if (moduleAssemblyName != null)
			{
				AddDiagnostic(diagnostics, ErrorCode.ERR_AssemblyNameOnNonModule);
			}
			compilationName = text;
		}
		if (moduleName == null)
		{
			moduleName = outputFileName;
		}
	}

	private ImmutableArray<string> BuildSearchPaths(string? sdkDirectoryOpt, List<string> libPaths, List<string>? responsePathsOpt)
	{
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance();
		if (sdkDirectoryOpt != null)
		{
			instance.Add(sdkDirectoryOpt);
		}
		instance.AddRange(libPaths);
		if (responsePathsOpt != null)
		{
			instance.AddRange(responsePathsOpt);
		}
		return instance.ToImmutableAndFree();
	}

	public static IEnumerable<string> ParseConditionalCompilationSymbols(string value, out IEnumerable<Diagnostic> diagnostics)
	{
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance();
		ParseConditionalCompilationSymbols(System.MemoryExtensions.AsMemory(value), instance, out diagnostics);
		return instance.ToArrayAndFree();
	}

	internal static void ParseConditionalCompilationSymbols(ReadOnlyMemory<char> valueMemory, ArrayBuilder<string> defines, out IEnumerable<Diagnostic> diagnostics)
	{
		DiagnosticBag outputDiagnostics = DiagnosticBag.GetInstance();
		if (valueMemory.IsWhiteSpace())
		{
			outputDiagnostics.Add(Diagnostic.Create(Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance, 2029, valueMemory.ToString()));
			diagnostics = outputDiagnostics.ToReadOnlyAndFree();
			return;
		}
		ReadOnlySpan<char> span = valueMemory.Span;
		int nextIndex = 0;
		int index;
		for (index = 0; index < span.Length; index++)
		{
			char c = span[index];
			if ((c == ',' || c == ';') ? true : false)
			{
				add();
				nextIndex = index + 1;
			}
		}
		if (nextIndex < span.Length)
		{
			add();
		}
		diagnostics = outputDiagnostics.ToReadOnlyAndFree();
		void add()
		{
			string text = valueMemory.Slice(nextIndex, index - nextIndex).Trim().ToString();
			if (SyntaxFacts.IsValidIdentifier(text))
			{
				defines.Add(text);
			}
			else
			{
				outputDiagnostics.Add(Diagnostic.Create(Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance, 2029, text));
			}
		}
	}

	private static Platform ParsePlatform(string value, IList<Diagnostic> diagnostics)
	{
		switch (value.ToLowerInvariant())
		{
		case "x86":
			return Platform.X86;
		case "x64":
			return Platform.X64;
		case "itanium":
			return Platform.Itanium;
		case "anycpu":
			return Platform.AnyCpu;
		case "anycpu32bitpreferred":
			return Platform.AnyCpu32BitPreferred;
		case "arm":
			return Platform.Arm;
		case "arm64":
			return Platform.Arm64;
		default:
			AddDiagnostic(diagnostics, ErrorCode.ERR_BadPlatformType, value);
			return Platform.AnyCpu;
		}
	}

	private static OutputKind ParseTarget(string value, IList<Diagnostic> diagnostics)
	{
		switch (value.ToLowerInvariant())
		{
		case "exe":
			return OutputKind.ConsoleApplication;
		case "winexe":
			return OutputKind.WindowsApplication;
		case "library":
			return OutputKind.DynamicallyLinkedLibrary;
		case "module":
			return OutputKind.NetModule;
		case "appcontainerexe":
			return OutputKind.WindowsRuntimeApplication;
		case "winmdobj":
			return OutputKind.WindowsRuntimeMetadata;
		default:
			AddDiagnostic(diagnostics, ErrorCode.FTL_InvalidTarget);
			return OutputKind.ConsoleApplication;
		}
	}

	private static IEnumerable<string> ParseUsings(string arg, string? value, IList<Diagnostic> diagnostics)
	{
		if (RoslynString.IsNullOrEmpty(value))
		{
			AddDiagnostic(diagnostics, ErrorCode.ERR_SwitchNeedsString, MessageID.IDS_Namespace1.Localize(), arg);
			yield break;
		}
		string[] array = value.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			yield return array[i];
		}
	}

	private static void ParseAnalyzers(string arg, ReadOnlyMemory<char>? valueMemory, List<CommandLineAnalyzerReference> analyzerReferences, List<Diagnostic> diagnostics)
	{
		if (valueMemory.HasValue)
		{
			ReadOnlyMemory<char> valueOrDefault = valueMemory.GetValueOrDefault();
			if (valueOrDefault.Length == 0)
			{
				AddDiagnostic(diagnostics, ErrorCode.ERR_NoFileSpec, arg);
				return;
			}
			ArrayBuilder<ReadOnlyMemory<char>> instance = ArrayBuilder<ReadOnlyMemory<char>>.GetInstance();
			CommandLineParser.ParseSeparatedPathsEx(valueOrDefault, instance);
			foreach (ReadOnlyMemory<char> item in instance)
			{
				if (item.Length != 0)
				{
					analyzerReferences.Add(new CommandLineAnalyzerReference(item.ToString()));
				}
			}
			instance.Free();
		}
		else
		{
			AddDiagnostic(diagnostics, ErrorCode.ERR_SwitchNeedsString, MessageID.IDS_Text.Localize(), arg);
		}
	}

	private static void ParseAssemblyReferences(string arg, ReadOnlyMemory<char>? valueMemory, IList<Diagnostic> diagnostics, bool embedInteropTypes, List<CommandLineReference> commandLineReferences)
	{
		if (!valueMemory.HasValue)
		{
			AddDiagnostic(diagnostics, ErrorCode.ERR_SwitchNeedsString, MessageID.IDS_Text.Localize(), arg);
			return;
		}
		ReadOnlyMemory<char> value = valueMemory.Value;
		if (value.Length == 0)
		{
			AddDiagnostic(diagnostics, ErrorCode.ERR_NoFileSpec, arg);
			return;
		}
		ReadOnlySpan<char> span = value.Span;
		int num = span.IndexOfAny(s_quoteOrEquals);
		string text;
		if (num >= 0 && span[num] == '=')
		{
			text = value.Slice(0, num).ToString();
			value = value.Slice(num + 1);
			if (!SyntaxFacts.IsValidIdentifier(text))
			{
				AddDiagnostic(diagnostics, ErrorCode.ERR_BadExternIdentifier, text);
				return;
			}
		}
		else
		{
			text = null;
		}
		ArrayBuilder<ReadOnlyMemory<char>> instance = ArrayBuilder<ReadOnlyMemory<char>>.GetInstance();
		CommandLineParser.ParseSeparatedPathsEx(value, instance);
		int num2 = 0;
		foreach (ReadOnlyMemory<char> item in instance)
		{
			if (!item.IsWhiteSpace())
			{
				num2++;
				ImmutableArray<string> aliases = ((text != null) ? ImmutableArray.Create(text) : ImmutableArray<string>.Empty);
				commandLineReferences.Add(new CommandLineReference(properties: new MetadataReferenceProperties(MetadataImageKind.Assembly, aliases, embedInteropTypes), reference: item.ToString()));
			}
		}
		instance.Free();
		if (text != null)
		{
			if (num2 > 1)
			{
				commandLineReferences.RemoveRange(commandLineReferences.Count - num2, num2);
				AddDiagnostic(diagnostics, ErrorCode.ERR_OneAliasPerReference);
			}
			else if (num2 == 0)
			{
				AddDiagnostic(diagnostics, ErrorCode.ERR_AliasMissingFile, text);
			}
		}
	}

	private static void ValidateWin32Settings(string? win32ResourceFile, string? win32IconResourceFile, string? win32ManifestFile, OutputKind outputKind, IList<Diagnostic> diagnostics)
	{
		if (win32ResourceFile != null)
		{
			if (win32IconResourceFile != null)
			{
				AddDiagnostic(diagnostics, ErrorCode.ERR_CantHaveWin32ResAndIcon);
			}
			if (win32ManifestFile != null)
			{
				AddDiagnostic(diagnostics, ErrorCode.ERR_CantHaveWin32ResAndManifest);
			}
		}
		if (outputKind.IsNetModule() && win32ManifestFile != null)
		{
			AddDiagnostic(diagnostics, ErrorCode.WRN_CantHaveManifestForModule);
		}
	}

	private static IEnumerable<InstrumentationKind> ParseInstrumentationKinds(string value, IList<Diagnostic> diagnostics)
	{
		string[] array = value.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text.ToLower() == "testcoverage")
			{
				yield return InstrumentationKind.TestCoverage;
				continue;
			}
			AddDiagnostic(diagnostics, ErrorCode.ERR_InvalidInstrumentationKind, text);
		}
	}

	internal static bool TryParseResourceDescription(string argName, ReadOnlyMemory<char> resourceDescriptor, string? baseDirectory, IList<Diagnostic> diagnostics, bool isEmbedded, out CommandLineResource resource)
	{
		if (!CommandLineParser.TryParseResourceDescription(resourceDescriptor, baseDirectory, skipLeadingSeparators: false, allowEmptyAccessibility: false, out string filePath, out string fullPath, out string fileName, out string resourceName, out bool? isPublic, out string rawAccessibility))
		{
			if (!isPublic.HasValue)
			{
				AddDiagnostic(diagnostics, ErrorCode.ERR_BadResourceVis, rawAccessibility ?? "");
			}
			else if (RoslynString.IsNullOrWhiteSpace(filePath))
			{
				AddDiagnostic(diagnostics, ErrorCode.ERR_NoFileSpec, argName);
			}
			else
			{
				AddDiagnostic(diagnostics, ErrorCode.FTL_InvalidInputFileName, filePath);
			}
			resource = default(CommandLineResource);
			return false;
		}
		resource = new CommandLineResource(resourceName, fullPath, isEmbedded ? null : fileName, isPublic.Value);
		return true;
	}

	private static void ParseWarnings(ReadOnlyMemory<char> value, ArrayBuilder<string> ids)
	{
		value = value.Unquote();
		ArrayBuilder<ReadOnlyMemory<char>> instance = ArrayBuilder<ReadOnlyMemory<char>>.GetInstance();
		ReadOnlySpan<char> other = System.MemoryExtensions.AsSpan("nullable");
		CommandLineParser.ParseSeparatedStrings(value, s_warningSeparators, removeEmptyEntries: true, instance);
		foreach (ReadOnlyMemory<char> item in instance)
		{
			if (item.Span.Equals(other, StringComparison.OrdinalIgnoreCase))
			{
				foreach (string nullableWarning in ErrorFacts.NullableWarnings)
				{
					ids.Add(nullableWarning);
				}
				ids.Add(Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance.GetIdForErrorCode(8632));
				ids.Add(Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance.GetIdForErrorCode(8669));
			}
			else
			{
				string text = item.ToString();
				if (ushort.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) && ErrorFacts.IsWarning((ErrorCode)result))
				{
					ids.Add(Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance.GetIdForErrorCode(result));
				}
				else
				{
					ids.Add(text);
				}
			}
		}
		instance.Free();
	}

	private static void AddWarnings(Dictionary<string, ReportDiagnostic> d, ReportDiagnostic kind, ReadOnlyMemory<char> warningArgument)
	{
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance();
		ParseWarnings(warningArgument, instance);
		foreach (string item in instance)
		{
			if (d.TryGetValue(item, out var value))
			{
				if (value != ReportDiagnostic.Suppress)
				{
					d[item] = kind;
				}
			}
			else
			{
				d.Add(item, kind);
			}
		}
		instance.Free();
	}

	private static void UnimplementedSwitch(IList<Diagnostic> diagnostics, string switchName)
	{
		AddDiagnostic(diagnostics, ErrorCode.WRN_UnimplementedCommandLineSwitch, "/" + switchName);
	}

	internal override void GenerateErrorForNoFilesFoundInRecurse(string path, IList<Diagnostic> diagnostics)
	{
	}

	private static void AddDiagnostic(IList<Diagnostic> diagnostics, ErrorCode errorCode)
	{
		diagnostics.Add(Diagnostic.Create(Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance, (int)errorCode));
	}

	private static void AddDiagnostic(IList<Diagnostic> diagnostics, ErrorCode errorCode, params object[] arguments)
	{
		diagnostics.Add(Diagnostic.Create(Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance, (int)errorCode, arguments));
	}

	private static void AddDiagnostic(IList<Diagnostic> diagnostics, Dictionary<string, ReportDiagnostic> warningOptions, ErrorCode errorCode, params object[] arguments)
	{
		warningOptions.TryGetValue(Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance.GetIdForErrorCode((int)errorCode), out var value);
		if (value != ReportDiagnostic.Suppress)
		{
			AddDiagnostic(diagnostics, errorCode, arguments);
		}
	}
}
