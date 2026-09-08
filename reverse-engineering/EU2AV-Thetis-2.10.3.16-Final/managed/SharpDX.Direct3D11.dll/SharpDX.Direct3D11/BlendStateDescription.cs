using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

public struct BlendStateDescription
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public RawBool AlphaToCoverageEnable;

		public RawBool IndependentBlendEnable;

		public RenderTargetBlendDescription RenderTarget;

		public RenderTargetBlendDescription __RenderTarget1;

		public RenderTargetBlendDescription __RenderTarget2;

		public RenderTargetBlendDescription __RenderTarget3;

		public RenderTargetBlendDescription __RenderTarget4;

		public RenderTargetBlendDescription __RenderTarget5;

		public RenderTargetBlendDescription __RenderTarget6;

		public RenderTargetBlendDescription __RenderTarget7;
	}

	public RawBool AlphaToCoverageEnable;

	public RawBool IndependentBlendEnable;

	internal RenderTargetBlendDescription[] _RenderTarget;

	public RenderTargetBlendDescription[] RenderTarget
	{
		get
		{
			return _RenderTarget ?? (_RenderTarget = new RenderTargetBlendDescription[8]);
		}
		private set
		{
			_RenderTarget = value;
		}
	}

	public static BlendStateDescription Default()
	{
		BlendStateDescription result = new BlendStateDescription
		{
			AlphaToCoverageEnable = false,
			IndependentBlendEnable = false
		};
		RenderTargetBlendDescription[] renderTarget = result.RenderTarget;
		for (int i = 0; i < renderTarget.Length; i++)
		{
			renderTarget[i].IsBlendEnabled = false;
			renderTarget[i].SourceBlend = BlendOption.One;
			renderTarget[i].DestinationBlend = BlendOption.Zero;
			renderTarget[i].BlendOperation = BlendOperation.Add;
			renderTarget[i].SourceAlphaBlend = BlendOption.One;
			renderTarget[i].DestinationAlphaBlend = BlendOption.Zero;
			renderTarget[i].AlphaBlendOperation = BlendOperation.Add;
			renderTarget[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
		}
		return result;
	}

	public BlendStateDescription Clone()
	{
		BlendStateDescription result = new BlendStateDescription
		{
			AlphaToCoverageEnable = AlphaToCoverageEnable,
			IndependentBlendEnable = IndependentBlendEnable
		};
		RenderTargetBlendDescription[] renderTarget = RenderTarget;
		RenderTargetBlendDescription[] renderTarget2 = result.RenderTarget;
		for (int i = 0; i < renderTarget.Length; i++)
		{
			renderTarget2[i] = renderTarget[i];
		}
		return result;
	}

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal unsafe void __MarshalFrom(ref __Native @ref)
	{
		AlphaToCoverageEnable = @ref.AlphaToCoverageEnable;
		IndependentBlendEnable = @ref.IndependentBlendEnable;
		fixed (RenderTargetBlendDescription* ptr = &RenderTarget[0])
		{
			void* ptr2 = ptr;
			fixed (RenderTargetBlendDescription* renderTarget = &@ref.RenderTarget)
			{
				void* ptr3 = renderTarget;
				Utilities.CopyMemory((IntPtr)ptr2, (IntPtr)ptr3, 8 * sizeof(RenderTargetBlendDescription));
			}
		}
	}

	internal unsafe void __MarshalTo(ref __Native @ref)
	{
		@ref.AlphaToCoverageEnable = AlphaToCoverageEnable;
		@ref.IndependentBlendEnable = IndependentBlendEnable;
		fixed (RenderTargetBlendDescription* ptr = &RenderTarget[0])
		{
			void* ptr2 = ptr;
			fixed (RenderTargetBlendDescription* renderTarget = &@ref.RenderTarget)
			{
				void* ptr3 = renderTarget;
				Utilities.CopyMemory((IntPtr)ptr3, (IntPtr)ptr2, 8 * sizeof(RenderTargetBlendDescription));
			}
		}
	}
}
