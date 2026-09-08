using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Roslyn.Utilities;

internal static class RoslynDebug
{
	[InterpolatedStringHandler]
	public struct AssertInterpolatedStringHandler
	{
		private readonly StringBuilder? _builder = null;

		public AssertInterpolatedStringHandler(int literalLength, int formattedCount, bool condition, out bool shouldAppend)
		{
			shouldAppend = !condition;
			if (shouldAppend)
			{
				_builder = new StringBuilder(literalLength + formattedCount);
			}
		}

		internal string ToStringAndClear()
		{
			return _builder.ToString();
		}

		public void AppendLiteral(string value)
		{
			_builder.Append(value);
		}

		public void AppendFormatted<T>(T value)
		{
			_builder.Append(value);
		}

		public void AppendFormatted(ReadOnlySpan<char> value)
		{
			_builder.Append(value.ToString());
		}
	}

	[Conditional("DEBUG")]
	public static void Assert([DoesNotReturnIf(false)] bool condition)
	{
	}

	[Conditional("DEBUG")]
	public static void Assert([DoesNotReturnIf(false)] bool condition, string message)
	{
	}

	[Conditional("DEBUG")]
	public static void Assert([DoesNotReturnIf(false)] bool condition, [InterpolatedStringHandlerArgument("condition")] ref AssertInterpolatedStringHandler message)
	{
	}

	[Conditional("DEBUG")]
	public static void AssertNotNull<T>([NotNull] T value)
	{
	}

	[Conditional("DEBUG")]
	internal static void AssertOrFailFast([DoesNotReturnIf(false)] bool condition, string? message = null)
	{
		if (!condition && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HELIX_DUMP_FOLDER")))
		{
			if (message == null)
			{
				message = "AssertOrFailFast failed";
			}
			StackTrace value = new StackTrace();
			Console.WriteLine(message);
			Console.WriteLine(value);
			Environment.FailFast(message);
		}
	}
}
