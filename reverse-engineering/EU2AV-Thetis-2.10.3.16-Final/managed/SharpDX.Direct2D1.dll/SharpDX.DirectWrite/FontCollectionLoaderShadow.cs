using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

internal class FontCollectionLoaderShadow : ComObjectShadow
{
	private class FontCollectionLoaderVtbl : ComObjectVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int CreateEnumeratorFromKeyDelegate(IntPtr thisPtr, IntPtr factory, IntPtr collectionKey, int collectionKeySize, out IntPtr fontFileEnumerator);

		public FontCollectionLoaderVtbl()
			: base(1)
		{
			AddMethod(new CreateEnumeratorFromKeyDelegate(CreateEnumeratorFromKeyImpl));
		}

		private static int CreateEnumeratorFromKeyImpl(IntPtr thisPtr, IntPtr factory, IntPtr collectionKey, int collectionKeySize, out IntPtr fontFileEnumerator)
		{
			fontFileEnumerator = IntPtr.Zero;
			try
			{
				FontCollectionLoaderShadow fontCollectionLoaderShadow = CppObjectShadow.ToShadow<FontCollectionLoaderShadow>(thisPtr);
				FontFileEnumerator callback = ((FontCollectionLoader)fontCollectionLoaderShadow.Callback).CreateEnumeratorFromKey(fontCollectionLoaderShadow._factory, new DataPointer(collectionKey, collectionKeySize));
				fontFileEnumerator = FontFileEnumeratorShadow.ToIntPtr(callback);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly FontCollectionLoaderVtbl Vtbl = new FontCollectionLoaderVtbl();

	private Factory _factory;

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(FontCollectionLoader loader)
	{
		return CppObject.ToCallbackPtr<FontCollectionLoader>(loader);
	}

	public static void SetFactory(FontCollectionLoader loader, Factory factory)
	{
		CppObjectShadow.ToShadow<FontCollectionLoaderShadow>(ToIntPtr(loader))._factory = factory;
	}
}
