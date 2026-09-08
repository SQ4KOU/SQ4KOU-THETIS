using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Markdig.Helpers;

[ExcludeFromCodeCoverage]
internal static class ThrowHelper
{
	[DoesNotReturn]
	public static void ArgumentNullException(string paramName)
	{
		throw new ArgumentNullException(paramName);
	}

	[DoesNotReturn]
	public static void ArgumentNullException_item()
	{
		throw new ArgumentNullException("item");
	}

	[DoesNotReturn]
	public static void ArgumentNullException_text()
	{
		throw new ArgumentNullException("text");
	}

	[DoesNotReturn]
	public static void ArgumentNullException_label()
	{
		throw new ArgumentNullException("label");
	}

	[DoesNotReturn]
	public static void ArgumentNullException_key()
	{
		throw new ArgumentNullException("key");
	}

	[DoesNotReturn]
	public static void ArgumentNullException_name()
	{
		throw new ArgumentNullException("name");
	}

	[DoesNotReturn]
	public static void ArgumentNullException_markdown()
	{
		throw new ArgumentNullException("markdown");
	}

	[DoesNotReturn]
	public static void ArgumentNullException_writer()
	{
		throw new ArgumentNullException("writer");
	}

	[DoesNotReturn]
	public static void ArgumentNullException_leafBlock()
	{
		throw new ArgumentNullException("leafBlock");
	}

	[DoesNotReturn]
	public static void ArgumentNullException_markdownObject()
	{
		throw new ArgumentNullException("markdownObject");
	}

	[DoesNotReturn]
	public static void ArgumentException(string message)
	{
		throw new ArgumentException(message);
	}

	[DoesNotReturn]
	public static void ArgumentException(string message, string paramName)
	{
		throw new ArgumentException(message, paramName);
	}

	[DoesNotReturn]
	public static void ArgumentOutOfRangeException(string paramName)
	{
		throw new ArgumentOutOfRangeException(paramName);
	}

	[DoesNotReturn]
	public static void ArgumentOutOfRangeException(string message, string paramName)
	{
		throw new ArgumentOutOfRangeException(paramName, message);
	}

	[DoesNotReturn]
	public static void ArgumentOutOfRangeException_index()
	{
		throw new ArgumentOutOfRangeException("index");
	}

	[DoesNotReturn]
	public static void InvalidOperationException(string message)
	{
		throw new InvalidOperationException(message);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void CheckDepthLimit(int depth, bool useLargeLimit = false)
	{
		int num = (useLargeLimit ? 10240 : 128);
		if (depth > num)
		{
			DepthLimitExceeded();
		}
		[DoesNotReturn]
		static void DepthLimitExceeded()
		{
			throw new ArgumentException("Markdown elements in the input are too deeply nested - depth limit exceeded. Input is most likely not sensible or is a very large table.");
		}
	}

	[DoesNotReturn]
	public static void ThrowArgumentNullException(ExceptionArgument argument)
	{
		throw new ArgumentNullException(argument.ToString());
	}

	[DoesNotReturn]
	public static void ThrowArgumentException(ExceptionArgument argument, ExceptionReason reason)
	{
		throw new ArgumentException(argument.ToString(), GetExceptionReason(reason));
	}

	[DoesNotReturn]
	public static void ThrowArgumentOutOfRangeException(ExceptionArgument argument, ExceptionReason reason)
	{
		throw new ArgumentOutOfRangeException(argument.ToString(), GetExceptionReason(reason));
	}

	[DoesNotReturn]
	public static void ThrowIndexOutOfRangeException()
	{
		throw new IndexOutOfRangeException();
	}

	private static string GetExceptionReason(ExceptionReason reason)
	{
		return reason switch
		{
			ExceptionReason.String_Empty => "String must not be empty.", 
			ExceptionReason.SmallCapacity => "Capacity was less than the current size.", 
			ExceptionReason.InvalidOffsetLength => "Offset and length must refer to a position in the string.", 
			ExceptionReason.DuplicateKey => "The given key is already present in the dictionary.", 
			_ => "", 
		};
	}
}
