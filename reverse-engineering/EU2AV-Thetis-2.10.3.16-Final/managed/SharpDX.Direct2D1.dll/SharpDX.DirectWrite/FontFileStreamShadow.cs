using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

internal class FontFileStreamShadow : ComObjectShadow
{
	private class FontFileStreamVtbl : ComObjectVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int ReadFileFragmentDelegate(IntPtr thisPtr, out IntPtr fragmentStart, long fileOffset, long fragmentSize, out IntPtr fragmentContext);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void ReleaseFileFragmentDelegate(IntPtr thisPtr, IntPtr fragmentContext);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int GetFileSizeDelegate(IntPtr thisPtr, out long fileSize);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int GetLastWriteTimeDelegate(IntPtr thisPtr, out long lastWriteTime);

		public FontFileStreamVtbl()
			: base(4)
		{
			AddMethod(new ReadFileFragmentDelegate(ReadFileFragmentImpl));
			AddMethod(new ReleaseFileFragmentDelegate(ReleaseFileFragmentImpl));
			AddMethod(new GetFileSizeDelegate(GetFileSizeImpl));
			AddMethod(new GetLastWriteTimeDelegate(GetLastWriteTimeImpl));
		}

		private static int ReadFileFragmentImpl(IntPtr thisPtr, out IntPtr fragmentStart, long fileOffset, long fragmentSize, out IntPtr fragmentContext)
		{
			fragmentStart = IntPtr.Zero;
			fragmentContext = IntPtr.Zero;
			try
			{
				((FontFileStream)CppObjectShadow.ToShadow<FontFileStreamShadow>(thisPtr).Callback).ReadFileFragment(out fragmentStart, fileOffset, fragmentSize, out fragmentContext);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static void ReleaseFileFragmentImpl(IntPtr thisPtr, IntPtr fragmentContext)
		{
			((FontFileStream)CppObjectShadow.ToShadow<FontFileStreamShadow>(thisPtr).Callback).ReleaseFileFragment(fragmentContext);
		}

		private static int GetFileSizeImpl(IntPtr thisPtr, out long fileSize)
		{
			fileSize = 0L;
			try
			{
				FontFileStream fontFileStream = (FontFileStream)CppObjectShadow.ToShadow<FontFileStreamShadow>(thisPtr).Callback;
				fileSize = fontFileStream.GetFileSize();
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int GetLastWriteTimeImpl(IntPtr thisPtr, out long lastWriteTime)
		{
			lastWriteTime = 0L;
			try
			{
				FontFileStream fontFileStream = (FontFileStream)CppObjectShadow.ToShadow<FontFileStreamShadow>(thisPtr).Callback;
				lastWriteTime = fontFileStream.GetLastWriteTime();
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly FontFileStreamVtbl Vtbl = new FontFileStreamVtbl();

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(FontFileStream callback)
	{
		return CppObject.ToCallbackPtr<FontFileStream>(callback);
	}
}
