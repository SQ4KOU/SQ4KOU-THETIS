using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace WindowsFirewallHelper.InternalHelpers.Collections;

internal class ComEnumerator<TSource, TTarget> : IEnumerator<TTarget>, IDisposable, IEnumerator
{
	private readonly object[] _buffer = new object[1];

	private readonly IntPtr _bufferLengthPointer;

	private readonly IEnumVARIANT _enumVariant;

	private readonly Func<TSource, TTarget> _resolveFunction;

	private TSource _currentSource;

	object IEnumerator.Current => Current;

	public TTarget Current => _resolveFunction(_currentSource);

	public ComEnumerator(IEnumVARIANT enumVariant, Func<TSource, TTarget> resolveFunction)
	{
		_enumVariant = enumVariant;
		_resolveFunction = resolveFunction;
		_bufferLengthPointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)));
	}

	public void Dispose()
	{
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	public bool MoveNext()
	{
		int num = _enumVariant.Next(_buffer.Length, _buffer, _bufferLengthPointer);
		if (num != 0)
		{
			Marshal.ThrowExceptionForHR(num);
		}
		if (Marshal.ReadInt32(_bufferLengthPointer) > 0)
		{
			_currentSource = (TSource)_buffer[0];
			return true;
		}
		_currentSource = default(TSource);
		return false;
	}

	public void Reset()
	{
		_enumVariant.Reset();
	}

	private void ReleaseUnmanagedResources()
	{
		Marshal.FreeCoTaskMem(_bufferLengthPointer);
	}

	~ComEnumerator()
	{
		ReleaseUnmanagedResources();
	}
}
