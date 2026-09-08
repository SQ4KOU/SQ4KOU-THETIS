using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAudio.Dmo.Effect;

public class DmoDistortion : IDmoEffector<DmoDistortion.Params>, IDisposable
{
	public struct Params
	{
		public const float GainMin = -60f;

		public const float GainMax = 0f;

		public const float GainDefault = -18f;

		public const float EdgeMin = 0f;

		public const float EdgeMax = 100f;

		public const float EdgeDefault = 15f;

		public const float PostEqCenterFrequencyMin = 100f;

		public const float PostEqCenterFrequencyMax = 8000f;

		public const float PostEqCenterFrequencyDefault = 2400f;

		public const float PostEqBandWidthMin = 100f;

		public const float PostEqBandWidthMax = 8000f;

		public const float PostEqBandWidthDefault = 2400f;

		public const float PreLowPassCutoffMin = 100f;

		public const float PreLowPassCutoffMax = 8000f;

		public const float PreLowPassCutoffDefault = 8000f;

		private readonly IDirectSoundFXDistortion fxDistortion;

		public float Gain
		{
			get
			{
				return GetAllParameters().Gain;
			}
			set
			{
				DsFxDistortion allParameters = GetAllParameters();
				allParameters.Gain = Math.Max(Math.Min(0f, value), -60f);
				SetAllParameters(allParameters);
			}
		}

		public float Edge
		{
			get
			{
				return GetAllParameters().Edge;
			}
			set
			{
				DsFxDistortion allParameters = GetAllParameters();
				allParameters.Edge = Math.Max(Math.Min(100f, value), 0f);
				SetAllParameters(allParameters);
			}
		}

		public float PostEqCenterFrequency
		{
			get
			{
				return GetAllParameters().PostEqCenterFrequency;
			}
			set
			{
				DsFxDistortion allParameters = GetAllParameters();
				allParameters.PostEqCenterFrequency = Math.Max(Math.Min(8000f, value), 100f);
				SetAllParameters(allParameters);
			}
		}

		public float PostEqBandWidth
		{
			get
			{
				return GetAllParameters().PostEqBandWidth;
			}
			set
			{
				DsFxDistortion allParameters = GetAllParameters();
				allParameters.PostEqBandWidth = Math.Max(Math.Min(8000f, value), 100f);
				SetAllParameters(allParameters);
			}
		}

		public float PreLowPassCutoff
		{
			get
			{
				return GetAllParameters().PreLowPassCutoff;
			}
			set
			{
				DsFxDistortion allParameters = GetAllParameters();
				allParameters.PreLowPassCutoff = Math.Max(Math.Min(8000f, value), 100f);
				SetAllParameters(allParameters);
			}
		}

		internal Params(IDirectSoundFXDistortion dsFxObject)
		{
			fxDistortion = dsFxObject;
		}

		private void SetAllParameters(DsFxDistortion param)
		{
			Marshal.ThrowExceptionForHR(fxDistortion.SetAllParameters(ref param));
		}

		private DsFxDistortion GetAllParameters()
		{
			Marshal.ThrowExceptionForHR(fxDistortion.GetAllParameters(out var param));
			return param;
		}
	}

	private static readonly Guid Id_Distortion = new Guid("EF114C90-CD1D-484E-96E5-09CFAF912A21");

	public MediaObject MediaObject { get; }

	public MediaObjectInPlace MediaObjectInPlace { get; }

	public Params EffectParams { get; }

	public DmoDistortion()
	{
		DmoDescriptor dmoDescriptor = DmoEnumerator.GetAudioEffectNames().First((DmoDescriptor descriptor) => object.Equals(descriptor.Clsid, Id_Distortion));
		if (dmoDescriptor != null)
		{
			object obj = Activator.CreateInstance(Type.GetTypeFromCLSID(dmoDescriptor.Clsid));
			MediaObject = new MediaObject((IMediaObject)obj);
			MediaObjectInPlace = new MediaObjectInPlace((IMediaObjectInPlace)obj);
			EffectParams = new Params((IDirectSoundFXDistortion)obj);
		}
	}

	public void Dispose()
	{
		MediaObjectInPlace?.Dispose();
		MediaObject?.Dispose();
	}
}
