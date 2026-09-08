using System;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Roslyn.Utilities;

[NonCopyable]
internal struct SingleInitNullable<T> where T : struct
{
	private int _initialized;

	private T _value;

	public T Initialize<TArg>(Func<TArg, T> valueFactory, TArg arg)
	{
		return ReadIfInitialized() ?? GetOrStore(valueFactory(arg));
	}

	private T? ReadIfInitialized()
	{
		if (Volatile.Read(in _initialized) != 2)
		{
			return null;
		}
		return _value;
	}

	private T GetOrStore(T value)
	{
		SpinWait spinWait = default(SpinWait);
		while (true)
		{
			int num = Interlocked.CompareExchange(ref _initialized, 1, 0);
			switch (num)
			{
			case 0:
				_value = value;
				Volatile.Write(ref _initialized, 2);
				return value;
			case 1:
				break;
			case 2:
				return ReadIfInitialized() ?? throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/InternalUtilities/SingleInitNullable.cs", 77);
			default:
				throw ExceptionUtilities.UnexpectedValue(num);
			}
			spinWait.SpinOnce();
		}
	}
}
