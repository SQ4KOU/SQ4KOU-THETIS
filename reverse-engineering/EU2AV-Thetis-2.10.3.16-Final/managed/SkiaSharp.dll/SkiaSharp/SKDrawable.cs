using System;
using System.Threading;

namespace SkiaSharp;

public class SKDrawable : SKObject, ISKReferenceCounted
{
	private static readonly SKManagedDrawableDelegates delegates;

	internal int fromNative;

	public uint GenerationId => SkiaApi.sk_drawable_get_generation_id(Handle);

	public unsafe SKRect Bounds
	{
		get
		{
			SKRect result = default(SKRect);
			SkiaApi.sk_drawable_get_bounds(Handle, &result);
			return result;
		}
	}

	public int ApproximateBytesUsed => (int)SkiaApi.sk_drawable_approximate_bytes_used(Handle);

	static SKDrawable()
	{
		delegates = new SKManagedDrawableDelegates
		{
			fDraw = DelegateProxies.SKManagedDrawableDrawProxy,
			fGetBounds = DelegateProxies.SKManagedDrawableGetBoundsProxy,
			fApproximateBytesUsed = DelegateProxies.SKManagedDrawableApproximateBytesUsedProxy,
			fMakePictureSnapshot = DelegateProxies.SKManagedDrawableMakePictureSnapshotProxy,
			fDestroy = DelegateProxies.SKManagedDrawableDestroyProxy
		};
		SkiaApi.sk_manageddrawable_set_procs(delegates);
	}

	protected SKDrawable()
		: this(owns: true)
	{
	}

	protected unsafe SKDrawable(bool owns)
		: base(IntPtr.Zero, owns)
	{
		IntPtr intPtr = DelegateProxies.CreateUserData(this, makeWeak: true);
		Handle = SkiaApi.sk_manageddrawable_new((void*)intPtr);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKDrawable instance.");
		}
	}

	internal SKDrawable(IntPtr x, bool owns)
		: base(x, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		if (Interlocked.CompareExchange(ref fromNative, 0, 0) == 0)
		{
			SkiaApi.sk_drawable_unref(Handle);
		}
	}

	public unsafe void Draw(SKCanvas canvas, in SKMatrix matrix)
	{
		fixed (SKMatrix* param = &matrix)
		{
			SkiaApi.sk_drawable_draw(Handle, canvas.Handle, param);
		}
	}

	public void Draw(SKCanvas canvas, float x, float y)
	{
		Draw(canvas, SKMatrix.CreateTranslation(x, y));
	}

	public SKPicture Snapshot()
	{
		return SKPicture.GetObject(SkiaApi.sk_drawable_new_picture_snapshot(Handle), owns: true, unrefExisting: false);
	}

	public void NotifyDrawingChanged()
	{
		SkiaApi.sk_drawable_notify_drawing_changed(Handle);
	}

	protected internal virtual void OnDraw(SKCanvas canvas)
	{
	}

	protected internal virtual int OnGetApproximateBytesUsed()
	{
		return 0;
	}

	protected internal virtual SKRect OnGetBounds()
	{
		return SKRect.Empty;
	}

	protected internal virtual SKPicture OnSnapshot()
	{
		using SKPictureRecorder sKPictureRecorder = new SKPictureRecorder();
		SKCanvas canvas = sKPictureRecorder.BeginRecording(Bounds);
		Draw(canvas, 0f, 0f);
		return sKPictureRecorder.EndRecording();
	}

	internal static SKDrawable GetObject(IntPtr handle)
	{
		return SKObject.GetOrAddObject(handle, (IntPtr h, bool o) => new SKDrawable(h, o));
	}
}
