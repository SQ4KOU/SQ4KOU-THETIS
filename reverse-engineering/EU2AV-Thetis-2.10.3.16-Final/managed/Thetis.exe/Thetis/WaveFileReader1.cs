using System;
using System.IO;
using System.Threading;

namespace Thetis;

public class WaveFileReader1
{
	private int id;

	private int rcvr_rate;

	private int xmtr_rate;

	private int rcvr_size;

	private int xmtr_size;

	private BinaryReader reader;

	private int format;

	private int sample_rate;

	private int source_channels;

	private int channels;

	private int bitdepth;

	private volatile bool playback;

	private readonly Thread _thread;

	private RingBufferFloat rb_l;

	private RingBufferFloat rb_r;

	private float[] buf_l_in;

	private float[] buf_r_in;

	private float[] buf_l_out;

	private float[] buf_r_out;

	private int IN_BLOCK;

	private int OUT_BLOCK;

	private byte[] io_buf;

	private int io_buf_size;

	private bool eof;

	private int total_samps_written;

	private int total_samps_read;

	private unsafe void* rcvr_resamp_l;

	private unsafe void* rcvr_resamp_r;

	private unsafe void* xmtr_resamp_l;

	private unsafe void* xmtr_resamp_r;

	private readonly Action<Exception> _finished;

	private Exception _failure;

	private int _finish_started;

	private bool _fade_enabled;

	private int _fade_ms;

	private int _fade_frames;

	private float[] _fade_gain;

	private long _total_frames;

	private long _frames_read;

	private int _bytes_per_frame;

	private float _mono_gain = 1f;

	public unsafe WaveFileReader1(int wfr_id, int fmt, int samp_rate, int chan, int bit_depth, long data_length_bytes, bool fade_enabled, int fade_ms, double mono_gain_db, BinaryReader binread, Action<Exception> finished)
	{
		id = wfr_id;
		format = fmt;
		sample_rate = samp_rate;
		source_channels = chan;
		channels = chan;
		bitdepth = bit_depth;
		reader = binread;
		_finished = finished;
		_fade_enabled = fade_enabled;
		_fade_ms = fade_ms;
		if (_fade_ms < 0)
		{
			_fade_ms = 0;
		}
		if (source_channels < 1)
		{
			source_channels = 1;
		}
		if (source_channels > 2)
		{
			source_channels = 2;
		}
		if (channels < 1)
		{
			channels = 1;
		}
		if (channels > 2)
		{
			channels = 2;
		}
		if (channels == 1)
		{
			channels = 2;
		}
		if (source_channels == 1)
		{
			_mono_gain = (float)Math.Pow(10.0, mono_gain_db / 20.0);
		}
		if (format == 3)
		{
			_bytes_per_frame = source_channels * 4;
		}
		else
		{
			int num = bitdepth / 8;
			if (num < 1)
			{
				num = 1;
			}
			_bytes_per_frame = source_channels * num;
		}
		if (_bytes_per_frame < 1)
		{
			_bytes_per_frame = 1;
		}
		_total_frames = 0L;
		if (data_length_bytes > 0)
		{
			_total_frames = data_length_bytes / _bytes_per_frame;
		}
		_frames_read = 0L;
		_fade_frames = 0;
		_fade_gain = null;
		if (_fade_enabled && _fade_ms > 0 && sample_rate > 0 && _total_frames > 0)
		{
			_fade_frames = (int)Math.Round((double)sample_rate * ((double)_fade_ms / 1000.0));
			if (_fade_frames < 0)
			{
				_fade_frames = 0;
			}
			if (_fade_frames * 2 > _total_frames)
			{
				_fade_frames = (int)(_total_frames / 2);
				if (_fade_frames < 0)
				{
					_fade_frames = 0;
				}
			}
			if (_fade_frames > 0)
			{
				_fade_gain = buildCosineFade(_fade_frames);
			}
		}
		rcvr_rate = cmaster.GetInputRate(0, id);
		xmtr_rate = cmaster.GetInputRate(1, 0);
		int num2 = ((rcvr_rate >= xmtr_rate) ? rcvr_rate : xmtr_rate);
		rcvr_size = cmaster.GetBuffSize(rcvr_rate);
		xmtr_size = cmaster.GetBuffSize(xmtr_rate);
		IN_BLOCK = 2048;
		OUT_BLOCK = (int)Math.Ceiling((double)IN_BLOCK * (double)num2 / (double)sample_rate);
		rb_l = new RingBufferFloat(16 * OUT_BLOCK);
		rb_r = new RingBufferFloat(16 * OUT_BLOCK);
		buf_l_in = new float[IN_BLOCK];
		buf_r_in = new float[IN_BLOCK];
		buf_l_out = new float[OUT_BLOCK];
		buf_r_out = new float[OUT_BLOCK];
		if (format == 1)
		{
			if (bitdepth == 32)
			{
				io_buf_size = IN_BLOCK * 4 * source_channels;
			}
			else if (bitdepth == 24)
			{
				io_buf_size = IN_BLOCK * 3 * source_channels;
			}
			else if (bitdepth == 16)
			{
				io_buf_size = IN_BLOCK * 2 * source_channels;
			}
			else if (bitdepth == 8)
			{
				io_buf_size = IN_BLOCK * source_channels;
			}
			else
			{
				io_buf_size = IN_BLOCK * 2 * source_channels;
			}
		}
		else
		{
			io_buf_size = IN_BLOCK * 4 * source_channels;
		}
		if (sample_rate != rcvr_rate)
		{
			rcvr_resamp_l = WDSP.create_resampleFV(sample_rate, rcvr_rate);
			if (channels > 1)
			{
				rcvr_resamp_r = WDSP.create_resampleFV(sample_rate, rcvr_rate);
			}
		}
		if (sample_rate != xmtr_rate)
		{
			xmtr_resamp_l = WDSP.create_resampleFV(sample_rate, xmtr_rate);
			if (channels > 1)
			{
				xmtr_resamp_r = WDSP.create_resampleFV(sample_rate, xmtr_rate);
			}
		}
		io_buf = new byte[io_buf_size];
		playback = true;
		eof = false;
		total_samps_written = 0;
		total_samps_read = 0;
		do
		{
			ReadBuffer(ref reader);
		}
		while (rb_l.WriteSpace() > OUT_BLOCK && !eof);
		_thread = new Thread(ProcessBuffers);
		_thread.Name = "Wave File Read Thread";
		_thread.IsBackground = true;
		_thread.Priority = ThreadPriority.Normal;
		_thread.Start();
	}

	private static float[] buildCosineFade(int frames)
	{
		if (frames < 1)
		{
			return new float[0];
		}
		float[] array = new float[frames];
		if (frames == 1)
		{
			array[0] = 1f;
			return array;
		}
		for (int i = 0; i < frames; i++)
		{
			double num = (double)i / (double)(frames - 1);
			double num2 = 0.5 - 0.5 * Math.Cos(Math.PI * num);
			array[i] = (float)num2;
		}
		return array;
	}

	public void Stop()
	{
		playback = false;
	}

	public bool WaitForStop(int timeout_ms)
	{
		if (_thread == null)
		{
			return true;
		}
		if (Thread.CurrentThread == _thread)
		{
			return true;
		}
		return _thread.Join(timeout_ms);
	}

	private void ProcessBuffers()
	{
		try
		{
			while (playback)
			{
				while (rb_l.WriteSpace() >= OUT_BLOCK && !eof)
				{
					ReadBuffer(ref reader);
					if (!playback)
					{
						break;
					}
				}
				if (playback)
				{
					Thread.Sleep(10);
					continue;
				}
				break;
			}
		}
		catch (Exception failure)
		{
			_failure = failure;
			playback = false;
		}
		try
		{
			reader.Close();
		}
		catch
		{
		}
		if (_failure != null)
		{
			queueFinish(_failure);
		}
	}

	private unsafe void ReadBuffer(ref BinaryReader r)
	{
		int num = IN_BLOCK;
		int num2 = r.Read(io_buf, 0, io_buf_size);
		if (num2 < io_buf_size)
		{
			eof = true;
			num = num2 / _bytes_per_frame;
		}
		for (int i = 0; i < num; i++)
		{
			if (format == 1)
			{
				if (bitdepth == 32)
				{
					int num3 = i * _bytes_per_frame;
					buf_l_in[i] = (float)((io_buf[num3 + 3] << 24) | ((io_buf[num3 + 2] & 0xFF) << 16) | ((io_buf[num3 + 1] & 0xFF) << 8) | (io_buf[num3] & 0xFF)) / 2.1474836E+09f;
					if (source_channels > 1)
					{
						buf_r_in[i] = (float)((io_buf[num3 + 7] << 24) | ((io_buf[num3 + 6] & 0xFF) << 16) | ((io_buf[num3 + 5] & 0xFF) << 8) | (io_buf[num3 + 4] & 0xFF)) / 2.1474836E+09f;
					}
					else
					{
						buf_r_in[i] = buf_l_in[i];
					}
				}
				else if (bitdepth == 24)
				{
					int num4 = i * _bytes_per_frame;
					buf_l_in[i] = (float)(((io_buf[num4 + 2] << 24) | ((io_buf[num4 + 1] & 0xFF) << 16) | ((io_buf[num4] & 0xFF) << 8)) >> 8) / 8388608f;
					if (source_channels > 1)
					{
						buf_r_in[i] = (float)(((io_buf[num4 + 5] << 24) | ((io_buf[num4 + 4] & 0xFF) << 16) | ((io_buf[num4 + 3] & 0xFF) << 8)) >> 8) / 8388608f;
					}
					else
					{
						buf_r_in[i] = buf_l_in[i];
					}
				}
				else if (bitdepth == 16)
				{
					int num5 = i * _bytes_per_frame;
					buf_l_in[i] = (float)((double)BitConverter.ToInt16(io_buf, num5) / 32767.0);
					if (source_channels > 1)
					{
						buf_r_in[i] = (float)((double)BitConverter.ToInt16(io_buf, num5 + 2) / 32767.0);
					}
					else
					{
						buf_r_in[i] = buf_l_in[i];
					}
				}
				else
				{
					int num6 = i * _bytes_per_frame;
					buf_l_in[i] = (float)((io_buf[num6] & 0xFF) - 128) / 128f;
					if (source_channels > 1)
					{
						buf_r_in[i] = (float)((io_buf[num6 + 1] & 0xFF) - 128) / 128f;
					}
					else
					{
						buf_r_in[i] = buf_l_in[i];
					}
				}
			}
			else
			{
				int num7 = i * _bytes_per_frame;
				buf_l_in[i] = BitConverter.ToSingle(io_buf, num7);
				if (source_channels > 1)
				{
					buf_r_in[i] = BitConverter.ToSingle(io_buf, num7 + 4);
				}
				else
				{
					buf_r_in[i] = buf_l_in[i];
				}
			}
		}
		if (source_channels == 1 && _mono_gain != 1f)
		{
			for (int j = 0; j < num; j++)
			{
				buf_l_in[j] *= _mono_gain;
				buf_r_in[j] *= _mono_gain;
			}
		}
		if (_fade_enabled && _fade_frames > 0 && _fade_gain != null && _fade_gain.Length == _fade_frames && _total_frames > 0)
		{
			long num8 = _total_frames - _fade_frames;
			for (int k = 0; k < num; k++)
			{
				long num9 = _frames_read + k;
				float num10 = 1f;
				if (num9 < _fade_frames)
				{
					num10 *= _fade_gain[(int)num9];
				}
				if (num9 >= num8)
				{
					long num11 = _total_frames - 1 - num9;
					if (num11 < 0)
					{
						num11 = 0L;
					}
					if (num11 >= _fade_frames)
					{
						num11 = _fade_frames - 1;
					}
					num10 *= _fade_gain[(int)num11];
				}
				buf_l_in[k] *= num10;
				buf_r_in[k] *= num10;
			}
		}
		_frames_read += num;
		if (num < IN_BLOCK)
		{
			for (int l = num; l < IN_BLOCK; l++)
			{
				buf_l_in[l] = 0f;
				buf_r_in[l] = 0f;
			}
			playback = false;
			try
			{
				r.Close();
			}
			catch
			{
			}
		}
		int iN_BLOCK = IN_BLOCK;
		if (!Audio.MOX)
		{
			if (sample_rate != rcvr_rate)
			{
				fixed (float* ptr = &buf_l_in[0])
				{
					float* input = ptr;
					fixed (float* output = &buf_l_out[0])
					{
						WDSP.xresampleFV(input, output, IN_BLOCK, &iN_BLOCK, rcvr_resamp_l);
					}
				}
				fixed (float* ptr2 = &buf_r_in[0])
				{
					float* input2 = ptr2;
					fixed (float* output2 = &buf_r_out[0])
					{
						WDSP.xresampleFV(input2, output2, IN_BLOCK, &iN_BLOCK, rcvr_resamp_r);
					}
				}
			}
			else
			{
				buf_l_in.CopyTo(buf_l_out, 0);
				buf_r_in.CopyTo(buf_r_out, 0);
			}
		}
		else if (sample_rate != xmtr_rate)
		{
			fixed (float* ptr = &buf_l_in[0])
			{
				float* input3 = ptr;
				fixed (float* output3 = &buf_l_out[0])
				{
					WDSP.xresampleFV(input3, output3, IN_BLOCK, &iN_BLOCK, xmtr_resamp_l);
				}
			}
			fixed (float* ptr2 = &buf_r_in[0])
			{
				float* input4 = ptr2;
				fixed (float* output4 = &buf_r_out[0])
				{
					WDSP.xresampleFV(input4, output4, IN_BLOCK, &iN_BLOCK, xmtr_resamp_r);
				}
			}
		}
		else
		{
			buf_l_in.CopyTo(buf_l_out, 0);
			buf_r_in.CopyTo(buf_r_out, 0);
		}
		rb_l.Write(buf_l_out, iN_BLOCK);
		rb_r.Write(buf_r_out, iN_BLOCK);
		total_samps_written += iN_BLOCK;
	}

	public unsafe void GetPlayBuffer(float* left, float* right)
	{
		int num = rb_l.ReadSpace();
		if (num == 0)
		{
			return;
		}
		int num2 = ((!Audio.MOX) ? rcvr_size : xmtr_size);
		if (num > num2)
		{
			num = num2;
		}
		rb_l.ReadPtr(left, num);
		rb_r.ReadPtr(right, num);
		if (num < num2)
		{
			for (int i = num; i < num2; i++)
			{
				left[i] = 0f;
				right[i] = 0f;
			}
		}
		total_samps_read += num2;
		if (total_samps_read >= total_samps_written)
		{
			queueFinish(null);
		}
	}

	private void queueFinish(Exception ex)
	{
		if (ex != null && _failure == null)
		{
			_failure = ex;
		}
		if (Interlocked.Exchange(ref _finish_started, 1) == 0)
		{
			Thread thread = new Thread(finishPlayback);
			thread.Name = "Wave File Finish Playback";
			thread.IsBackground = true;
			thread.Priority = ThreadPriority.Normal;
			thread.Start();
		}
	}

	private void finishPlayback()
	{
		Thread.Sleep(50);
		_finished?.Invoke(_failure);
	}
}
