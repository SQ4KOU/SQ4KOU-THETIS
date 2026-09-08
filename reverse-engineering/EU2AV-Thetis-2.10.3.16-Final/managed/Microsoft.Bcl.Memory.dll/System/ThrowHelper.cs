using System.Diagnostics.CodeAnalysis;

namespace System;

internal static class ThrowHelper
{
	[DoesNotReturn]
	internal static void ThrowArgumentException_DestinationTooShort()
	{
		throw new ArgumentException(System.SR.Argument_DestinationTooShort, "destination");
	}

	[DoesNotReturn]
	internal static void ThrowArgumentNullException(System.ExceptionArgument argument)
	{
		throw new ArgumentNullException(GetArgumentName(argument));
	}

	[DoesNotReturn]
	internal static void ThrowArgumentOutOfRangeException(System.ExceptionArgument argument)
	{
		throw new ArgumentOutOfRangeException(GetArgumentName(argument));
	}

	private static string GetArgumentName(System.ExceptionArgument argument)
	{
		return argument switch
		{
			System.ExceptionArgument.ch => "ch", 
			System.ExceptionArgument.culture => "culture", 
			System.ExceptionArgument.index => "index", 
			System.ExceptionArgument.input => "input", 
			System.ExceptionArgument.value => "value", 
			_ => "", 
		};
	}
}
