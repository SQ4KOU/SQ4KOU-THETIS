using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.CodeAnalysis.Collections.Internal;

internal static class ThrowHelper
{
	[DoesNotReturn]
	internal static void ThrowIndexOutOfRangeException()
	{
		throw new IndexOutOfRangeException();
	}

	[DoesNotReturn]
	internal static void ThrowArgumentOutOfRangeException()
	{
		throw new ArgumentOutOfRangeException();
	}

	[DoesNotReturn]
	internal static void ThrowArgumentOutOfRange_IndexMustBeLessException()
	{
		throw GetArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_IndexMustBeLess);
	}

	[DoesNotReturn]
	internal static void ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException()
	{
		throw GetArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_IndexMustBeLessOrEqual);
	}

	[DoesNotReturn]
	internal static void ThrowArgumentException_BadComparer(object? comparer)
	{
		throw new ArgumentException($"Unable to sort because the IComparer.Compare() method returns inconsistent results. Either a value does not compare equal to itself, or one value repeatedly compared to another value yields different results. IComparer: '{comparer}'.");
	}

	[DoesNotReturn]
	internal static void ThrowIndexArgumentOutOfRange_NeedNonNegNumException()
	{
		throw GetArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
	}

	[DoesNotReturn]
	internal static void ThrowLengthArgumentOutOfRange_ArgumentOutOfRange_NeedNonNegNum()
	{
		throw GetArgumentOutOfRangeException(ExceptionArgument.length, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
	}

	[DoesNotReturn]
	internal static void ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_IndexMustBeLessOrEqual()
	{
		throw GetArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_IndexMustBeLessOrEqual);
	}

	[DoesNotReturn]
	internal static void ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_IndexMustBeLess()
	{
		throw GetArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_IndexMustBeLess);
	}

	[DoesNotReturn]
	internal static void ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count()
	{
		throw GetArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count);
	}

	[DoesNotReturn]
	internal static void ThrowWrongKeyTypeArgumentException<T>(T key, Type targetType)
	{
		throw GetWrongKeyTypeArgumentException(key, targetType);
	}

	[DoesNotReturn]
	internal static void ThrowWrongValueTypeArgumentException<T>(T value, Type targetType)
	{
		throw GetWrongValueTypeArgumentException(value, targetType);
	}

	private static ArgumentException GetAddingDuplicateWithKeyArgumentException(object? key)
	{
		return new ArgumentException($"An item with the same key has already been added. Key: {key}");
	}

	[DoesNotReturn]
	internal static void ThrowAddingDuplicateWithKeyArgumentException<T>(T key)
	{
		throw GetAddingDuplicateWithKeyArgumentException(key);
	}

	[DoesNotReturn]
	internal static void ThrowKeyNotFoundException<T>(T key)
	{
		throw GetKeyNotFoundException(key);
	}

	[DoesNotReturn]
	internal static void ThrowArgumentException(ExceptionResource resource)
	{
		throw GetArgumentException(resource);
	}

	private static ArgumentNullException GetArgumentNullException(ExceptionArgument argument)
	{
		return new ArgumentNullException(GetArgumentName(argument));
	}

	[DoesNotReturn]
	internal static void ThrowArgumentNullException(ExceptionArgument argument)
	{
		throw GetArgumentNullException(argument);
	}

	[DoesNotReturn]
	internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument)
	{
		throw new ArgumentOutOfRangeException(GetArgumentName(argument));
	}

	[DoesNotReturn]
	internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument, ExceptionResource resource)
	{
		throw GetArgumentOutOfRangeException(argument, resource);
	}

	[DoesNotReturn]
	internal static void ThrowInvalidOperationException(ExceptionResource resource, Exception e)
	{
		throw new InvalidOperationException(GetResourceString(resource), e);
	}

	[DoesNotReturn]
	internal static void ThrowNotSupportedException(ExceptionResource resource)
	{
		throw new NotSupportedException(GetResourceString(resource));
	}

	[DoesNotReturn]
	internal static void ThrowArgumentException_Argument_IncompatibleArrayType()
	{
		throw new ArgumentException("Target array type is not compatible with the type of items in the collection.");
	}

	[DoesNotReturn]
	internal static void ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion()
	{
		throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
	}

	[DoesNotReturn]
	internal static void ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen()
	{
		throw new InvalidOperationException("Enumeration has either not started or has already finished.");
	}

	[DoesNotReturn]
	internal static void ThrowInvalidOperationException_ConcurrentOperationsNotSupported()
	{
		throw new InvalidOperationException("Operations that change non-concurrent collections must have exclusive access. A concurrent update was performed on this collection and corrupted its state. The collection's state is no longer correct.");
	}

	private static ArgumentException GetArgumentException(ExceptionResource resource)
	{
		return new ArgumentException(GetResourceString(resource));
	}

	private static ArgumentException GetWrongKeyTypeArgumentException(object? key, Type targetType)
	{
		return new ArgumentException($"The value \"{key}\" is not of type \"{targetType}\" and cannot be used in this generic collection.", "key");
	}

	private static ArgumentException GetWrongValueTypeArgumentException(object? value, Type targetType)
	{
		return new ArgumentException($"The value \"{value}\" is not of type \"{targetType}\" and cannot be used in this generic collection.", "value");
	}

	private static KeyNotFoundException GetKeyNotFoundException(object? key)
	{
		return new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");
	}

	private static ArgumentOutOfRangeException GetArgumentOutOfRangeException(ExceptionArgument argument, ExceptionResource resource)
	{
		return new ArgumentOutOfRangeException(GetArgumentName(argument), GetResourceString(resource));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void IfNullAndNullsAreIllegalThenThrow<T>(object? value, ExceptionArgument argName)
	{
		if (default(T) != null && value == null)
		{
			ThrowArgumentNullException(argName);
		}
	}

	private static string GetArgumentName(ExceptionArgument argument)
	{
		return argument switch
		{
			ExceptionArgument.dictionary => "dictionary", 
			ExceptionArgument.array => "array", 
			ExceptionArgument.info => "info", 
			ExceptionArgument.key => "key", 
			ExceptionArgument.value => "value", 
			ExceptionArgument.startIndex => "startIndex", 
			ExceptionArgument.index => "index", 
			ExceptionArgument.capacity => "capacity", 
			ExceptionArgument.collection => "collection", 
			ExceptionArgument.item => "item", 
			ExceptionArgument.converter => "converter", 
			ExceptionArgument.match => "match", 
			ExceptionArgument.count => "count", 
			ExceptionArgument.action => "action", 
			ExceptionArgument.comparison => "comparison", 
			ExceptionArgument.source => "source", 
			ExceptionArgument.length => "length", 
			ExceptionArgument.destinationArray => "destinationArray", 
			ExceptionArgument.other => "other", 
			_ => "", 
		};
	}

	private static string GetResourceString(ExceptionResource resource)
	{
		return resource switch
		{
			ExceptionResource.ArgumentOutOfRange_IndexMustBeLessOrEqual => "Index was out of range. Must be non-negative and less than or equal to the size of the collection.", 
			ExceptionResource.ArgumentOutOfRange_IndexMustBeLess => "Index was out of range. Must be non-negative and less than the size of the collection.", 
			ExceptionResource.ArgumentOutOfRange_Count => "Count must be positive and count must refer to a location within the string/array/collection.", 
			ExceptionResource.Arg_ArrayPlusOffTooSmall => "Destination array is not long enough to copy all the items in the collection. Check array index and length.", 
			ExceptionResource.Arg_RankMultiDimNotSupported => "Only single dimensional arrays are supported for the requested action.", 
			ExceptionResource.Arg_NonZeroLowerBound => "The lower bound of target array must be zero.", 
			ExceptionResource.ArgumentOutOfRange_ListInsert => "Index must be within the bounds of the List.", 
			ExceptionResource.ArgumentOutOfRange_NeedNonNegNum => "Non-negative number required.", 
			ExceptionResource.ArgumentOutOfRange_SmallCapacity => "capacity was less than the current size.", 
			ExceptionResource.Argument_InvalidOffLen => "Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.", 
			ExceptionResource.ArgumentOutOfRange_BiggerThanCollection => "Larger than collection size.", 
			ExceptionResource.NotSupported_KeyCollectionSet => "Mutating a key collection derived from a dictionary is not allowed.", 
			ExceptionResource.NotSupported_ValueCollectionSet => "Mutating a value collection derived from a dictionary is not allowed.", 
			ExceptionResource.InvalidOperation_IComparerFailed => "Failed to compare two elements in the array.", 
			_ => "", 
		};
	}
}
