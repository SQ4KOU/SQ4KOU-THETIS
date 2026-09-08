using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public abstract class CommandLineParser
{
	private readonly CommonMessageProvider _messageProvider;

	internal readonly bool IsScriptCommandLineParser;

	private static readonly char[] s_searchPatternTrimChars = new char[8] { '\t', '\n', '\v', '\f', '\r', ' ', '\u0085', '\u00a0' };

	internal const string ErrorLogOptionFormat = "<file>[,version={1|1.0|2|2.1}]";

	private static bool s_registeredEncodingProvider = CodePagesEncodingProvider.Instance == null;

	private static readonly char[] s_resourceSeparators = new char[1] { ',' };

	private static readonly char[] s_pathSeparators = new char[2] { ';', ',' };

	private static readonly char[] s_wildcards = new char[2] { '*', '?' };

	internal CommonMessageProvider MessageProvider => _messageProvider;

	protected abstract string RegularFileExtension { get; }

	protected abstract string ScriptFileExtension { get; }

	internal static string MismatchedVersionErrorText => CodeAnalysisResources.MismatchedVersion;

	internal CommandLineParser(CommonMessageProvider messageProvider, bool isScriptCommandLineParser)
	{
		_messageProvider = messageProvider;
		IsScriptCommandLineParser = isScriptCommandLineParser;
	}

	internal virtual TextReader CreateTextFileReader(string fullPath)
	{
		return new StreamReader(new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read), detectEncodingFromByteOrderMarks: true);
	}

	internal virtual IEnumerable<string> EnumerateFiles(string? directory, string fileNamePattern, SearchOption searchOption)
	{
		if (directory == null)
		{
			return SpecializedCollections.EmptyEnumerable<string>();
		}
		return Directory.EnumerateFiles(directory, fileNamePattern, searchOption);
	}

	internal abstract CommandLineArguments CommonParse(IEnumerable<string> args, string baseDirectory, string? sdkDirectory, string? additionalReferenceDirectories);

	public CommandLineArguments Parse(IEnumerable<string> args, string baseDirectory, string? sdkDirectory, string? additionalReferenceDirectories)
	{
		return CommonParse(args, baseDirectory, sdkDirectory, additionalReferenceDirectories);
	}

	internal static bool IsOptionName(string optionName, ReadOnlyMemory<char> value)
	{
		return IsOptionName(optionName, value.Span);
	}

	internal static bool IsOptionName(string shortOptionName, string longOptionName, ReadOnlyMemory<char> value)
	{
		if (!IsOptionName(shortOptionName, value))
		{
			return IsOptionName(longOptionName, value);
		}
		return true;
	}

	internal static bool IsOptionName(string optionName, ReadOnlySpan<char> value)
	{
		if (optionName.Length != value.Length)
		{
			return false;
		}
		for (int i = 0; i < optionName.Length; i++)
		{
			char c = value[i];
			if (c > '\u007f')
			{
				return System.MemoryExtensions.AsSpan(optionName).Equals(value, StringComparison.InvariantCultureIgnoreCase);
			}
			if (optionName[i] != char.ToLowerInvariant(c))
			{
				return false;
			}
		}
		return true;
	}

	internal static bool IsOption(string arg)
	{
		return IsOption(System.MemoryExtensions.AsSpan(arg));
	}

	internal static bool IsOption(ReadOnlySpan<char> arg)
	{
		if (arg.Length > 0)
		{
			if (arg[0] != '/')
			{
				return arg[0] == '-';
			}
			return true;
		}
		return false;
	}

	internal static bool IsOption(string optionName, string arg, out ReadOnlyMemory<char> name, out ReadOnlyMemory<char>? value)
	{
		if (TryParseOption(arg, out name, out value))
		{
			return IsOptionName(optionName, name);
		}
		return false;
	}

	internal static bool TryParseOption(string arg, [NotNullWhen(true)] out string? name, out string? value)
	{
		if (TryParseOption(arg, out ReadOnlyMemory<char> name2, out ReadOnlyMemory<char>? value2))
		{
			name = name2.ToString().ToLowerInvariant();
			value = value2?.ToString();
			return true;
		}
		name = null;
		value = null;
		return false;
	}

	internal static bool TryParseOption(string arg, out ReadOnlyMemory<char> name, out ReadOnlyMemory<char>? value)
	{
		if (!IsOption(arg))
		{
			name = default(ReadOnlyMemory<char>);
			value = null;
			return false;
		}
		if (arg == "-")
		{
			name = System.MemoryExtensions.AsMemory(arg);
			value = null;
			return true;
		}
		int num = arg.IndexOf(':', 1);
		if (arg.Length > 1 && arg[0] != '-' && ((num < 0) ? arg.IndexOf('/', 1) : arg.IndexOf('/', 1, num - 1)) > 0)
		{
			name = default(ReadOnlyMemory<char>);
			value = null;
			return false;
		}
		ReadOnlyMemory<char> readOnlyMemory = System.MemoryExtensions.AsMemory(arg);
		if (num >= 0)
		{
			name = readOnlyMemory.Slice(1, num - 1);
			value = readOnlyMemory.Slice(num + 1);
		}
		else
		{
			name = readOnlyMemory.Slice(1);
			value = null;
		}
		return true;
	}

	internal ErrorLogOptions? ParseErrorLogOptions(ReadOnlyMemory<char> arg, IList<Diagnostic> diagnostics, string? baseDirectory, out bool diagnosticAlreadyReported)
	{
		diagnosticAlreadyReported = false;
		ArrayBuilder<ReadOnlyMemory<char>> instance = ArrayBuilder<ReadOnlyMemory<char>>.GetInstance();
		try
		{
			ParseSeparatedStrings(arg, s_pathSeparators, removeEmptyEntries: true, instance);
			if (instance.Count == 0 || instance[0].Length == 0)
			{
				return null;
			}
			string text = ParseGenericPathToFile(instance[0].ToString(), diagnostics, baseDirectory);
			if (text == null)
			{
				diagnosticAlreadyReported = true;
				return null;
			}
			SarifVersion result = SarifVersion.Sarif1;
			if (instance.Count > 1 && instance[1].Length > 0)
			{
				string text2 = instance[1].ToString();
				string text3 = "version=";
				int length = text3.Length;
				if (text2.Length <= length || !text2.Substring(0, length).Equals(text3, StringComparison.OrdinalIgnoreCase) || !SarifVersionFacts.TryParse(text2.Substring(length), out result))
				{
					return null;
				}
			}
			if (instance.Count > 2)
			{
				return null;
			}
			return new ErrorLogOptions(text, result);
		}
		finally
		{
			instance.Free();
		}
	}

	internal static void ParseAndNormalizeFile(string unquoted, string? baseDirectory, out string? outputFileName, out string? outputDirectory, out string invalidPath)
	{
		outputFileName = null;
		outputDirectory = null;
		invalidPath = unquoted;
		string text = FileUtilities.ResolveRelativePath(unquoted, baseDirectory);
		if (text != null)
		{
			try
			{
				text = (invalidPath = Path.GetFullPath(text));
				outputFileName = Path.GetFileName(text);
				outputDirectory = Path.GetDirectoryName(text);
			}
			catch (Exception)
			{
				text = null;
			}
			if (outputFileName != null)
			{
				outputFileName = RemoveTrailingSpacesAndDots(outputFileName);
			}
		}
		if (text == null || !MetadataHelpers.IsValidMetadataIdentifier(outputDirectory) || !MetadataHelpers.IsValidMetadataIdentifier(outputFileName))
		{
			outputFileName = null;
		}
	}

	[return: NotNullIfNotNull("path")]
	internal static string? RemoveTrailingSpacesAndDots(string? path)
	{
		if (path == null)
		{
			return path;
		}
		int length = path.Length;
		for (int num = length - 1; num >= 0; num--)
		{
			char c = path[num];
			if (!char.IsWhiteSpace(c) && c != '.')
			{
				if (num != length - 1)
				{
					return path.Substring(0, num + 1);
				}
				return path;
			}
		}
		return string.Empty;
	}

	protected ImmutableArray<KeyValuePair<string, string>> ParsePathMap(string pathMap, IList<Diagnostic> errors)
	{
		if (pathMap.IsEmpty())
		{
			return ImmutableArray<KeyValuePair<string, string>>.Empty;
		}
		ArrayBuilder<KeyValuePair<string, string>> instance = ArrayBuilder<KeyValuePair<string, string>>.GetInstance();
		string[] array = SplitWithDoubledSeparatorEscaping(pathMap, ',');
		foreach (string text in array)
		{
			if (text.IsEmpty())
			{
				continue;
			}
			string[] array2 = SplitWithDoubledSeparatorEscaping(text, '=');
			if (array2.Length != 2)
			{
				errors.Add(Diagnostic.Create(_messageProvider, _messageProvider.ERR_InvalidPathMap));
				continue;
			}
			string text2 = array2[0];
			string text3 = array2[1];
			if (text2.Length == 0 || text3.Length == 0)
			{
				errors.Add(Diagnostic.Create(_messageProvider, _messageProvider.ERR_InvalidPathMap));
				continue;
			}
			text2 = PathUtilities.EnsureTrailingSeparator(text2);
			text3 = PathUtilities.EnsureTrailingSeparator(text3);
			instance.Add(new KeyValuePair<string, string>(text2, text3));
		}
		return instance.ToImmutableAndFree();
	}

	internal static string[] SplitWithDoubledSeparatorEscaping(string str, char separator)
	{
		if (str.Length == 0)
		{
			return Array.Empty<string>();
		}
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance();
		PooledStringBuilder instance2 = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance2.Builder;
		int num = 0;
		while (num < str.Length)
		{
			char c = str[num++];
			if (c == separator)
			{
				if (num >= str.Length || str[num] != separator)
				{
					instance.Add(builder.ToString());
					builder.Clear();
					continue;
				}
				num++;
			}
			builder.Append(c);
		}
		instance.Add(builder.ToString());
		instance2.Free();
		return instance.ToArrayAndFree();
	}

	internal void ParseOutputFile(string value, IList<Diagnostic> errors, string? baseDirectory, out string? outputFileName, out string? outputDirectory)
	{
		ParseAndNormalizeFile(RemoveQuotesAndSlashes(value), baseDirectory, out outputFileName, out outputDirectory, out string invalidPath);
		if (outputFileName == null || !MetadataHelpers.IsValidAssemblyOrModuleName(outputFileName))
		{
			errors.Add(Diagnostic.Create(_messageProvider, _messageProvider.FTL_InvalidInputFileName, invalidPath));
			outputFileName = null;
			outputDirectory = baseDirectory;
		}
	}

	internal string? ParsePdbPath(string value, IList<Diagnostic> errors, string? baseDirectory)
	{
		string result = null;
		ParseAndNormalizeFile(RemoveQuotesAndSlashes(value), baseDirectory, out string outputFileName, out string outputDirectory, out string invalidPath);
		if (outputFileName == null || PathUtilities.ChangeExtension(outputFileName, null).Length == 0)
		{
			errors.Add(Diagnostic.Create(_messageProvider, _messageProvider.FTL_InvalidInputFileName, invalidPath));
		}
		else
		{
			result = Path.ChangeExtension(Path.Combine(outputDirectory, outputFileName), ".pdb");
		}
		return result;
	}

	internal string? ParseGenericPathToFile(string unquoted, IList<Diagnostic> errors, string? baseDirectory, bool generateDiagnostic = true)
	{
		string result = null;
		ParseAndNormalizeFile(unquoted, baseDirectory, out string outputFileName, out string outputDirectory, out string invalidPath);
		if (string.IsNullOrWhiteSpace(outputFileName))
		{
			if (generateDiagnostic)
			{
				errors.Add(Diagnostic.Create(_messageProvider, _messageProvider.FTL_InvalidInputFileName, invalidPath));
			}
		}
		else
		{
			result = Path.Combine(outputDirectory, outputFileName);
		}
		return result;
	}

	internal void FlattenArgs(IEnumerable<string> rawArguments, IList<Diagnostic> diagnostics, ArrayBuilder<string> processedArgs, List<string>? scriptArgsOpt, string? baseDirectory, List<string>? responsePaths = null)
	{
		bool parsingScriptArgs = false;
		bool sourceFileSeen = false;
		bool optionsEnded = false;
		foreach (string rawArgument in rawArguments)
		{
			processArg(rawArgument);
		}
		void parseResponseFile(string fullPath)
		{
			PooledStringBuilder instance = PooledStringBuilder.GetInstance();
			List<string> list = new List<string>();
			try
			{
				using TextReader textReader = CreateTextFileReader(fullPath);
				Span<char> span = stackalloc char[256];
				int num = 0;
				while (true)
				{
					int num2 = textReader.Read();
					char? illegalChar;
					bool flag;
					switch (num2)
					{
					case -1:
						if (num > 0)
						{
							instance.Builder.Length = 0;
							CommandLineUtilities.SplitCommandLineIntoArguments(span.Slice(0, num), removeHashComments: true, instance.Builder, list, out illegalChar);
						}
						goto end_IL_002a;
					case 10:
					case 13:
						flag = true;
						break;
					default:
						flag = false;
						break;
					}
					if (flag)
					{
						if (num2 == 13 && textReader.Peek() == 10)
						{
							textReader.Read();
						}
						instance.Builder.Length = 0;
						CommandLineUtilities.SplitCommandLineIntoArguments(span.Slice(0, num), removeHashComments: true, instance.Builder, list, out illegalChar);
						num = 0;
					}
					else
					{
						if (num >= span.Length)
						{
							char[] array = new char[span.Length * 2];
							span.CopyTo(System.MemoryExtensions.AsSpan(array));
							span = array;
						}
						span[num] = (char)num2;
						num++;
					}
					continue;
					end_IL_002a:
					break;
				}
			}
			catch (Exception)
			{
				diagnostics.Add(Diagnostic.Create(_messageProvider, _messageProvider.ERR_OpenResponseFile, fullPath));
				return;
			}
			foreach (string item in list)
			{
				if (!string.Equals(item, "/noconfig", StringComparison.OrdinalIgnoreCase) && !string.Equals(item, "-noconfig", StringComparison.OrdinalIgnoreCase))
				{
					processArg(item);
				}
				else
				{
					diagnostics.Add(Diagnostic.Create(_messageProvider, _messageProvider.WRN_NoConfigNotOnCommandLine));
				}
			}
			instance.Free();
		}
		void processArg(string arg)
		{
			arg = arg.TrimEnd(Array.Empty<char>());
			if (parsingScriptArgs)
			{
				scriptArgsOpt.Add(arg);
			}
			else
			{
				if (scriptArgsOpt != null)
				{
					if (sourceFileSeen)
					{
						parsingScriptArgs = true;
						scriptArgsOpt.Add(arg);
						return;
					}
					if (!optionsEnded && arg == "--")
					{
						optionsEnded = true;
						processedArgs.Add(arg);
						return;
					}
				}
				if (!optionsEnded && arg.StartsWith("@", StringComparison.Ordinal))
				{
					string text = RemoveQuotesAndSlashes(arg.Substring(1)).TrimEnd(null);
					string text2 = FileUtilities.ResolveRelativePath(text, baseDirectory);
					if (text2 != null)
					{
						parseResponseFile(text2);
						if (responsePaths != null)
						{
							string directoryName = PathUtilities.GetDirectoryName(text2);
							if (directoryName == null)
							{
								diagnostics.Add(Diagnostic.Create(_messageProvider, _messageProvider.FTL_InvalidInputFileName, text));
							}
							else
							{
								responsePaths.Add(FileUtilities.NormalizeAbsolutePath(directoryName));
							}
						}
					}
					else
					{
						diagnostics.Add(Diagnostic.Create(_messageProvider, _messageProvider.FTL_InvalidInputFileName, text));
					}
				}
				else
				{
					processedArgs.Add(arg);
					sourceFileSeen |= optionsEnded || !IsOption(arg);
				}
			}
		}
	}

	internal static IEnumerable<string> ParseResponseLines(IEnumerable<string> lines)
	{
		List<string> list = new List<string>();
		foreach (string line in lines)
		{
			list.AddRange(CommandLineUtilities.SplitCommandLineIntoArguments(line, removeHashComments: true));
		}
		return list;
	}

	internal static bool TryParseClientArgs(IEnumerable<string> args, [NotNullWhen(true)] out List<string>? parsedArgs, out bool containsShared, out string? keepAliveValue, out string? pipeName, [NotNullWhen(false)] out string? errorMessage)
	{
		containsShared = false;
		keepAliveValue = null;
		errorMessage = null;
		parsedArgs = null;
		pipeName = null;
		List<string> list = new List<string>();
		foreach (string arg in args)
		{
			if (isClientArgsOption(arg, "keepalive", out var hasValue, out var optionValue))
			{
				if (string.IsNullOrEmpty(optionValue))
				{
					errorMessage = CodeAnalysisResources.MissingKeepAlive;
					return false;
				}
				if (!int.TryParse(optionValue, out var result))
				{
					errorMessage = CodeAnalysisResources.KeepAliveIsNotAnInteger;
					return false;
				}
				if (result < -1)
				{
					errorMessage = CodeAnalysisResources.KeepAliveIsTooSmall;
					return false;
				}
				keepAliveValue = optionValue;
			}
			else if (isClientArgsOption(arg, "shared", out hasValue, out optionValue))
			{
				if (hasValue)
				{
					if (string.IsNullOrEmpty(optionValue))
					{
						errorMessage = CodeAnalysisResources.SharedArgumentMissing;
						return false;
					}
					pipeName = optionValue;
				}
				containsShared = true;
			}
			else
			{
				list.Add(arg);
			}
		}
		if (keepAliveValue != null && !containsShared)
		{
			errorMessage = CodeAnalysisResources.KeepAliveWithoutShared;
			return false;
		}
		parsedArgs = list;
		return true;
		static bool isClientArgsOption(string arg, string optionName, out bool reference, out string? reference2)
		{
			reference = false;
			reference2 = null;
			if (arg.Length == 0 || (arg[0] != '/' && arg[0] != '-'))
			{
				return false;
			}
			arg = arg.Substring(1);
			if (!arg.StartsWith(optionName, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			if (arg.Length > optionName.Length)
			{
				if (arg[optionName.Length] != ':' && arg[optionName.Length] != '=')
				{
					return false;
				}
				reference = true;
				reference2 = arg.Substring(optionName.Length + 1).Trim(new char[1] { '"' });
			}
			return true;
		}
	}

	internal static bool TryParseResourceDescription(ReadOnlyMemory<char> resourceDescriptor, string? baseDirectory, bool skipLeadingSeparators, bool allowEmptyAccessibility, [NotNullWhen(true)] out string? filePath, [NotNullWhen(true)] out string? fullPath, [NotNullWhen(true)] out string? fileName, [NotNullWhen(true)] out string? resourceName, [NotNullWhen(true)] out bool? isPublic, out string? rawAccessibility)
	{
		filePath = null;
		fullPath = null;
		fileName = null;
		resourceName = null;
		isPublic = null;
		rawAccessibility = null;
		ArrayBuilder<ReadOnlyMemory<char>> instance = ArrayBuilder<ReadOnlyMemory<char>>.GetInstance();
		ParseSeparatedStrings(resourceDescriptor, s_resourceSeparators, removeEmptyEntries: false, instance);
		int i = 0;
		int num = instance.Count;
		if (skipLeadingSeparators)
		{
			for (; i < num && instance[i].Length == 0; i++)
			{
			}
			num -= i;
		}
		if (num >= 1)
		{
			filePath = RemoveQuotesAndSlashes(instance[i]);
		}
		if (num >= 2)
		{
			resourceName = RemoveQuotesAndSlashes(instance[i + 1]);
		}
		if (num >= 3)
		{
			rawAccessibility = RemoveQuotesAndSlashes(instance[i + 2]);
		}
		if (rawAccessibility == null || ((rawAccessibility == "") & allowEmptyAccessibility))
		{
			isPublic = true;
		}
		else if (string.Equals(rawAccessibility, "public", StringComparison.OrdinalIgnoreCase))
		{
			isPublic = true;
		}
		else if (string.Equals(rawAccessibility, "private", StringComparison.OrdinalIgnoreCase))
		{
			isPublic = false;
		}
		else
		{
			isPublic = null;
		}
		instance.Free();
		if (!isPublic.HasValue || RoslynString.IsNullOrWhiteSpace(filePath))
		{
			return false;
		}
		fileName = PathUtilities.GetFileName(filePath);
		fullPath = FileUtilities.ResolveRelativePath(filePath, baseDirectory);
		if (!PathUtilities.IsValidFilePath(fullPath))
		{
			return false;
		}
		if (RoslynString.IsNullOrWhiteSpace(resourceName))
		{
			resourceName = fileName;
		}
		return true;
	}

	public static IEnumerable<string> SplitCommandLineIntoArguments(string commandLine, bool removeHashComments)
	{
		return CommandLineUtilities.SplitCommandLineIntoArguments(commandLine, removeHashComments);
	}

	[return: NotNullIfNotNull("arg")]
	internal static string? RemoveQuotesAndSlashes(string? arg)
	{
		if (arg == null)
		{
			return null;
		}
		return RemoveQuotesAndSlashes(System.MemoryExtensions.AsMemory(arg));
	}

	internal static string RemoveQuotesAndSlashes(ReadOnlyMemory<char> argMemory)
	{
		return RemoveQuotesAndSlashesEx(argMemory).ToString();
	}

	internal static string? RemoveQuotesAndSlashes(ReadOnlyMemory<char>? argMemory)
	{
		if (argMemory.HasValue)
		{
			ReadOnlyMemory<char> valueOrDefault = argMemory.GetValueOrDefault();
			return RemoveQuotesAndSlashesEx(valueOrDefault).ToString();
		}
		return null;
	}

	internal static ReadOnlyMemory<char>? RemoveQuotesAndSlashesEx(ReadOnlyMemory<char>? argMemory)
	{
		ReadOnlyMemory<char> value;
		if (argMemory.HasValue)
		{
			ReadOnlyMemory<char> valueOrDefault = argMemory.GetValueOrDefault();
			value = RemoveQuotesAndSlashesEx(valueOrDefault);
		}
		else
		{
			value = null;
		}
		return value;
	}

	internal static ReadOnlyMemory<char> RemoveQuotesAndSlashesEx(ReadOnlyMemory<char> argMemory)
	{
		ReadOnlyMemory<char>? readOnlyMemory = removeFastPath(argMemory);
		if (readOnlyMemory.HasValue)
		{
			return readOnlyMemory.GetValueOrDefault();
		}
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		ReadOnlySpan<char> span = argMemory.Span;
		int i = 0;
		while (i < span.Length)
		{
			char c = span[i];
			switch (c)
			{
			case '\\':
				processSlashes(builder, span, ref i);
				break;
			case '"':
				i++;
				break;
			default:
				builder.Append(c);
				i++;
				break;
			}
		}
		return System.MemoryExtensions.AsMemory(instance.ToStringAndFree());
		static void processSlashes(StringBuilder stringBuilder, ReadOnlySpan<char> arg, ref int reference)
		{
			int num = 0;
			while (reference < arg.Length && arg[reference] == '\\')
			{
				num++;
				reference++;
			}
			if (reference < arg.Length && arg[reference] == '"')
			{
				while (num >= 2)
				{
					stringBuilder.Append('\\');
					num -= 2;
				}
				if (num == 1)
				{
					stringBuilder.Append('"');
				}
				reference++;
			}
			else
			{
				while (num > 0)
				{
					stringBuilder.Append('\\');
					num--;
				}
			}
		}
		static ReadOnlyMemory<char>? removeFastPath(ReadOnlyMemory<char> arg)
		{
			int j = 0;
			int num = arg.Length;
			ReadOnlySpan<char> span2 = arg.Span;
			while (num > 0 && span2[num - 1] == '"')
			{
				num--;
			}
			for (; j < num && span2[j] == '"'; j++)
			{
			}
			for (int k = j; k < num; k++)
			{
				if (span2[k] == '"')
				{
					return null;
				}
			}
			return arg.Slice(j, num - j);
		}
	}

	internal static IEnumerable<string> ParseSeparatedPaths(string arg)
	{
		ArrayBuilder<ReadOnlyMemory<char>> instance = ArrayBuilder<ReadOnlyMemory<char>>.GetInstance();
		ParseSeparatedPathsEx(System.MemoryExtensions.AsMemory(arg), instance);
		return from x in instance.ToArrayAndFree()
			select x.ToString();
	}

	internal static void ParseSeparatedPathsEx(ReadOnlyMemory<char>? str, ArrayBuilder<ReadOnlyMemory<char>> builder)
	{
		ParseSeparatedStrings(str, s_pathSeparators, removeEmptyEntries: true, builder);
		for (int i = 0; i < builder.Count; i++)
		{
			builder[i] = RemoveQuotesAndSlashesEx(builder[i]);
		}
	}

	internal static void ParseSeparatedStrings(ReadOnlyMemory<char>? strMemory, char[] separators, bool removeEmptyEntries, ArrayBuilder<ReadOnlyMemory<char>> builder)
	{
		if (!strMemory.HasValue)
		{
			return;
		}
		int num = 0;
		bool flag = false;
		ReadOnlyMemory<char> value = strMemory.Value;
		ReadOnlySpan<char> span = value.Span;
		for (int i = 0; i < span.Length; i++)
		{
			char c = span[i];
			if (c == '"')
			{
				flag = !flag;
			}
			else if (!flag && IReadOnlyListExtensions.Contains(separators, c))
			{
				ReadOnlyMemory<char> item = value.Slice(num, i - num);
				if (item.Length > 0 || !removeEmptyEntries)
				{
					builder.Add(item);
				}
				num = i + 1;
			}
		}
		ReadOnlyMemory<char> item2 = value.Slice(num);
		if (item2.Length > 0 || !removeEmptyEntries)
		{
			builder.Add(item2);
		}
	}

	internal IEnumerable<string> ResolveRelativePaths(IEnumerable<string> paths, string baseDirectory, IList<Diagnostic> errors)
	{
		foreach (string path in paths)
		{
			string text = FileUtilities.ResolveRelativePath(path, baseDirectory);
			if (text == null)
			{
				errors.Add(Diagnostic.Create(_messageProvider, _messageProvider.FTL_InvalidInputFileName, path));
			}
			else
			{
				yield return text;
			}
		}
	}

	private protected CommandLineSourceFile ToCommandLineSourceFile(string resolvedPath, bool isInputRedirected = false)
	{
		bool isScript = IsScriptCommandLineParser && !PathUtilities.GetExtension(System.MemoryExtensions.AsMemory(resolvedPath)).Span.Equals(System.MemoryExtensions.AsSpan(RegularFileExtension), StringComparison.OrdinalIgnoreCase);
		return new CommandLineSourceFile(resolvedPath, isScript, isInputRedirected);
	}

	internal void ParseFileArgument(ReadOnlyMemory<char> arg, string? baseDirectory, ArrayBuilder<string> filePathBuilder, IList<Diagnostic> errors)
	{
		string text = RemoveQuotesAndSlashes(arg);
		if (text.IndexOfAny(s_wildcards) != -1)
		{
			foreach (string item in ExpandFileNamePattern(text, baseDirectory, SearchOption.TopDirectoryOnly, errors))
			{
				filePathBuilder.Add(item);
			}
			return;
		}
		string text2 = FileUtilities.ResolveRelativePath(text, baseDirectory);
		if (text2 == null)
		{
			errors.Add(Diagnostic.Create(MessageProvider, MessageProvider.FTL_InvalidInputFileName, text));
		}
		else
		{
			filePathBuilder.Add(text2);
		}
	}

	private protected void ParseSeparatedFileArgument(ReadOnlyMemory<char> value, string? baseDirectory, ArrayBuilder<string> filePathBuilder, IList<Diagnostic> errors)
	{
		ArrayBuilder<ReadOnlyMemory<char>> instance = ArrayBuilder<ReadOnlyMemory<char>>.GetInstance();
		ParseSeparatedPathsEx(value, instance);
		foreach (ReadOnlyMemory<char> item in instance)
		{
			if (!item.IsWhiteSpace())
			{
				ParseFileArgument(item, baseDirectory, filePathBuilder, errors);
			}
		}
		instance.Free();
	}

	private protected IEnumerable<string> ParseSeparatedFileArgument(string value, string? baseDirectory, IList<Diagnostic> errors)
	{
		ArrayBuilder<string> builder = ArrayBuilder<string>.GetInstance();
		ParseSeparatedFileArgument(System.MemoryExtensions.AsMemory(value), baseDirectory, builder, errors);
		foreach (string item in builder)
		{
			yield return item;
		}
		builder.Free();
	}

	internal IEnumerable<CommandLineSourceFile> ParseRecurseArgument(string arg, string? baseDirectory, IList<Diagnostic> errors)
	{
		foreach (string item in ExpandFileNamePattern(arg, baseDirectory, SearchOption.AllDirectories, errors))
		{
			yield return ToCommandLineSourceFile(item);
		}
	}

	internal static Encoding? TryParseEncodingName(string arg)
	{
		if (!string.IsNullOrWhiteSpace(arg) && long.TryParse(arg, NumberStyles.None, CultureInfo.InvariantCulture, out var result) && result > 0)
		{
			while (true)
			{
				try
				{
					return Encoding.GetEncoding((int)result);
				}
				catch (NotSupportedException) when (!s_registeredEncodingProvider)
				{
					try
					{
						Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
					}
					catch
					{
					}
					s_registeredEncodingProvider = true;
				}
				catch (Exception)
				{
					return null;
				}
			}
		}
		return null;
	}

	internal static SourceHashAlgorithm TryParseHashAlgorithmName(string arg)
	{
		if (string.Equals("sha1", arg, StringComparison.OrdinalIgnoreCase))
		{
			return SourceHashAlgorithm.Sha1;
		}
		if (string.Equals("sha256", arg, StringComparison.OrdinalIgnoreCase))
		{
			return SourceHashAlgorithm.Sha256;
		}
		return SourceHashAlgorithm.None;
	}

	private IEnumerable<string> ExpandFileNamePattern(string path, string? baseDirectory, SearchOption searchOption, IList<Diagnostic> errors)
	{
		string directoryName = PathUtilities.GetDirectoryName(path);
		string pattern = PathUtilities.GetFileName(path);
		string resolvedDirectoryPath = (string.IsNullOrEmpty(directoryName) ? baseDirectory : FileUtilities.ResolveRelativePath(directoryName, baseDirectory));
		IEnumerator<string> enumerator = null;
		try
		{
			bool yielded = false;
			pattern = pattern.Trim(s_searchPatternTrimChars);
			if (!string.Equals(pattern, ".", StringComparison.Ordinal))
			{
				while (true)
				{
					string text;
					try
					{
						if (enumerator == null)
						{
							enumerator = EnumerateFiles(resolvedDirectoryPath, pattern, searchOption).GetEnumerator();
						}
						if (!enumerator.MoveNext())
						{
							break;
						}
						text = enumerator.Current;
						goto IL_00fd;
					}
					catch
					{
						text = null;
						goto IL_00fd;
					}
					IL_00fd:
					if (text != null)
					{
						text = FileUtilities.ResolveRelativePath(text, baseDirectory);
					}
					if (text == null)
					{
						errors.Add(Diagnostic.Create(MessageProvider, MessageProvider.FTL_InvalidInputFileName, path));
						break;
					}
					yielded = true;
					yield return text;
				}
			}
			if (!yielded)
			{
				if (searchOption == SearchOption.AllDirectories)
				{
					GenerateErrorForNoFilesFoundInRecurse(path, errors);
					yield break;
				}
				errors.Add(Diagnostic.Create(MessageProvider, MessageProvider.ERR_FileNotFound, path));
			}
		}
		finally
		{
			enumerator?.Dispose();
		}
	}

	internal abstract void GenerateErrorForNoFilesFoundInRecurse(string path, IList<Diagnostic> errors);

	internal ReportDiagnostic GetDiagnosticOptionsFromRulesetFile(string? fullPath, out Dictionary<string, ReportDiagnostic> diagnosticOptions, IList<Diagnostic> diagnostics)
	{
		return RuleSet.GetDiagnosticOptionsFromRulesetFile(fullPath, out diagnosticOptions, diagnostics, _messageProvider);
	}

	internal static bool TryParseUInt64(string? value, out ulong result)
	{
		result = 0uL;
		if (RoslynString.IsNullOrEmpty(value))
		{
			return false;
		}
		int fromBase = 10;
		if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
		{
			fromBase = 16;
		}
		else if (value.StartsWith("0", StringComparison.OrdinalIgnoreCase))
		{
			fromBase = 8;
		}
		try
		{
			result = Convert.ToUInt64(value, fromBase);
		}
		catch
		{
			return false;
		}
		return true;
	}

	internal static bool TryParseUInt16(string? value, out ushort result)
	{
		result = 0;
		if (RoslynString.IsNullOrEmpty(value))
		{
			return false;
		}
		int fromBase = 10;
		if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
		{
			fromBase = 16;
		}
		else if (value.StartsWith("0", StringComparison.OrdinalIgnoreCase))
		{
			fromBase = 8;
		}
		try
		{
			result = Convert.ToUInt16(value, fromBase);
		}
		catch
		{
			return false;
		}
		return true;
	}

	internal static ImmutableDictionary<string, string> ParseFeatures(List<string> features)
	{
		ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>();
		CompilerOptionParseUtilities.ParseFeatures(builder, features);
		return builder.ToImmutable();
	}

	internal static ImmutableArray<KeyValuePair<string, string>> SortPathMap(ImmutableArray<KeyValuePair<string, string>> pathMap)
	{
		return pathMap.Sort((KeyValuePair<string, string> x, KeyValuePair<string, string> y) => -x.Key.Length.CompareTo(y.Key.Length));
	}
}
