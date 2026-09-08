using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAudio.Dmo.Effect;

public class DmoGargle : IDmoEffector<DmoGargle.Params>, IDisposable
{
	public struct Params
	{
		public const uint RateHzMin = 1u;

		public const uint RateHzMax = 1000u;

		public const uint RateHzDefault = 20u;

		public const GargleWaveShape WaveShapeDefault = GargleWaveShape.Triangle;

		private readonly IDirectSoundFXGargle fxGargle;

		public uint RateHz
		{
			get
			{
				return GetAllParameters().RateHz;
			}
			set
			{
				DsFxGargle allParameters = GetAllParameters();
				allParameters.RateHz = Math.Max(Math.Min(1000u, value), 1u);
				SetAllParameters(allParameters);
			}
		}

		public GargleWaveShape WaveShape
		{
			get
			{
				return GetAllParameters().WaveShape;
			}
			set
			{
				DsFxGargle allParameters = GetAllParameters();
				if (Enum.IsDefined(typeof(GargleWaveShape), value))
				{
					allParameters.WaveShape = value;
				}
				SetAllParameters(allParameters);
			}
		}

		internal Params(IDirectSoundFXGargle dsFxObject)
		{
			fxGargle = dsFxObject;
		}

		private void SetAllParameters(DsFxGargle param)
		{
			Marshal.ThrowExceptionForHR(fxGargle.SetAllParameters(ref param));
		}

		private DsFxGargle GetAllParameters()
		{
			Marshal.ThrowExceptionForHR(fxGargle.GetAllParameters(out var param));
			return param;
		}
	}

	private static readonly Guid Id_Gargle = new Guid("DAFD8210-5711-4B91-9FE3-F75B7AE279BF");

	public MediaObject MediaObject { get; }

	public MediaObjectInPlace MediaObjectInPlace { get; }

	public Params EffectParams { get; }

	public DmoGargle()
	{
		DmoDescriptor dmoDescriptor = DmoEnumerator.GetAudioEffectNames().First((DmoDescriptor descriptor) => object.Equals(descriptor.Clsid, Id_Gargle));
		if (dmoDescriptor != null)
		{
			object obj = Activator.CreateInstance(Type.GetTypeFromCLSID(dmoDescriptor.Clsid));
			MediaObject = new MediaObject((IMediaObject)obj);
			MediaObjectInPlace = new MediaObjectInPlace((IMediaObjectInPlace)obj);
			EffectParams = new Params((IDirectSoundFXGargle)obj);
		}
	}

	public void Dispose()
	{
		MediaObjectInPlace?.Dispose();
		MediaObject?.Dispose();
	}
}
