using System;
using SharpDX.Direct2D1;
using SharpDX.Direct2D1.Effects;
using SharpDX.Mathematics.Interop;

namespace Thetis;

public static class WaterfallEffect
{
	private static Saturation _satEffect;

	private static GammaTransfer _gammaEffect;

	private static DeviceContext _cachedDC;

	private static bool _initialised = false;

	private static bool _failed = false;

	private static bool _l2Disabled = false;

	private static Effect _customEffect;

	private static readonly Guid CustomEffectGuid = new Guid("7A2C3F4E-1B5D-4A6E-9C7F-8E0F1A2B3C4D");

	public static bool IsAvailable => !_failed;

	public static bool IsInitialised => _initialised;

	public static void Init(DeviceContext dc)
	{
		if (_initialised || _failed)
		{
			return;
		}
		try
		{
			_satEffect = new Saturation(dc);
			_gammaEffect = new GammaTransfer(dc);
			_gammaEffect.SetInput(0, _satEffect.Output, true);
			_cachedDC = dc;
			_initialised = true;
		}
		catch (Exception ex)
		{
			_failed = true;
			Cleanup();
			LogTool.AddLogEntry("WaterfallEffect.Init failed: " + ex.Message, "D2D");
		}
	}

	public static bool Draw(DeviceContext dc, Bitmap bmp, float xOffset, float yOffset, float opacity, float saturation, float gamma, int toneMapMode, bool ditherEnabled)
	{
		if (_failed)
		{
			return false;
		}
		if (GPUDetector.HasCustomShaders && !_l2Disabled)
		{
			try
			{
				if (_customEffect == null)
				{
					_customEffect = new Effect(dc, CustomEffectGuid);
				}
				_customEffect.SetInput(0, bmp, false);
				_customEffect.SetValueByName("Saturation", saturation);
				_customEffect.SetValueByName("Gamma", gamma);
				_customEffect.SetValueByName("ToneMapMode", toneMapMode);
				_customEffect.SetValueByName("DitherAmount", ditherEnabled ? (1f / 128f) : 0f);
				dc.DrawImage(_customEffect, new RawVector2(xOffset, yOffset), InterpolationMode.NearestNeighbor);
				return true;
			}
			catch (Exception ex)
			{
				LogTool.AddLogEntry("WaterfallEffect L2 failed, disabling: " + ex.Message, "D2D");
				_l2Disabled = true;
				if (_customEffect != null)
				{
					_customEffect.Dispose();
					_customEffect = null;
				}
			}
		}
		if (!_initialised)
		{
			Init(dc);
		}
		if (!_initialised)
		{
			return false;
		}
		try
		{
			_satEffect.SetInput(0, bmp, false);
			_satEffect.Value = saturation;
			float num = 1f / Math.Max(gamma, 0.001f);
			_gammaEffect.RedExponent = num;
			_gammaEffect.GreenExponent = num;
			_gammaEffect.BlueExponent = num;
			_gammaEffect.RedAmplitude = 1f;
			_gammaEffect.GreenAmplitude = 1f;
			_gammaEffect.BlueAmplitude = 1f;
			_gammaEffect.RedOffset = 0f;
			_gammaEffect.GreenOffset = 0f;
			_gammaEffect.BlueOffset = 0f;
			_gammaEffect.RedDisable = false;
			_gammaEffect.GreenDisable = false;
			_gammaEffect.BlueDisable = false;
			_gammaEffect.ClampOutput = true;
			dc.DrawImage(_gammaEffect, new RawVector2(xOffset, yOffset), InterpolationMode.NearestNeighbor);
			return true;
		}
		catch (Exception ex2)
		{
			LogTool.AddLogEntry("WaterfallEffect.Draw L1 failed (fallback to CPU): " + ex2.Message, "D2D");
			_failed = true;
			Cleanup();
			return false;
		}
	}

	public static void Cleanup()
	{
		if (_customEffect != null)
		{
			_customEffect.Dispose();
			_customEffect = null;
		}
		if (_gammaEffect != null)
		{
			_gammaEffect.Dispose();
			_gammaEffect = null;
		}
		if (_satEffect != null)
		{
			_satEffect.Dispose();
			_satEffect = null;
		}
		_initialised = false;
	}

	public static void Reset()
	{
		Cleanup();
		_failed = false;
		_l2Disabled = false;
	}
}
