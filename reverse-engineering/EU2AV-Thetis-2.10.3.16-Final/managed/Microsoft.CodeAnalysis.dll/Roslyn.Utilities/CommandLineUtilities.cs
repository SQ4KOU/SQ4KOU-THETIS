using System;
using System.Collections.Generic;
using System.Text;

namespace Roslyn.Utilities;

internal static class CommandLineUtilities
{
	public static List<string> SplitCommandLineIntoArguments(string commandLine, bool removeHashComments)
	{
		char? illegalChar;
		return SplitCommandLineIntoArguments(commandLine, removeHashComments, out illegalChar);
	}

	public static List<string> SplitCommandLineIntoArguments(string commandLine, bool removeHashComments, out char? illegalChar)
	{
		List<string> list = new List<string>();
		SplitCommandLineIntoArguments(MemoryExtensions.AsSpan(commandLine), removeHashComments, new StringBuilder(), list, out illegalChar);
		return list;
	}

	public static void SplitCommandLineIntoArguments(ReadOnlySpan<char> commandLine, bool removeHashComments, StringBuilder builder, List<string> list, out char? illegalChar)
	{
		int i = 0;
		builder.Length = 0;
		illegalChar = null;
		while (i < commandLine.Length)
		{
			for (; i < commandLine.Length && char.IsWhiteSpace(commandLine[i]); i++)
			{
			}
			if (i == commandLine.Length || ((commandLine[i] == '#') & removeHashComments))
			{
				break;
			}
			int num = 0;
			builder.Length = 0;
			while (i < commandLine.Length && (!char.IsWhiteSpace(commandLine[i]) || num % 2 != 0))
			{
				char c = commandLine[i];
				if (c != '"')
				{
					if (c == '\\')
					{
						int num2 = 0;
						do
						{
							builder.Append(commandLine[i]);
							i++;
							num2++;
						}
						while (i < commandLine.Length && commandLine[i] == '\\');
						if (i < commandLine.Length && commandLine[i] == '"')
						{
							if (num2 % 2 == 0)
							{
								num++;
							}
							builder.Append('"');
							i++;
						}
						continue;
					}
					if ((c >= '\u0001' && c <= '\u001f') || c == '|')
					{
						if (!illegalChar.HasValue)
						{
							illegalChar = c;
						}
					}
					else
					{
						builder.Append(c);
					}
					i++;
				}
				else
				{
					builder.Append(c);
					num++;
					i++;
				}
			}
			if (num == 2 && builder[0] == '"' && builder[builder.Length - 1] == '"')
			{
				builder.Remove(0, 1);
				builder.Remove(builder.Length - 1, 1);
			}
			if (builder.Length > 0)
			{
				list.Add(builder.ToString());
			}
		}
	}
}
