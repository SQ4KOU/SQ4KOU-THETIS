using System;
using System.IO;
using System.Threading;

namespace Thetis;

public class WaveFileWriter
{
	public bool DitherEnabled;

	public float DitherAmount;

	private readonly Random rnd = new Random();

	private int _id;

	private BinaryWriter _writer;

	private volatile bool _record;

	private readonly Thread _thread;

	private short _channels;

	private short _format_tag;

	private short _bit_depth;

	private int _length_counter;

	private RingBufferFloat _rb_l;

	private RingBufferFloat _rb_r;

	private float[] _in_buf_l;

	private float[] _in_buf_r;

	private float[] _out_buf_l;

	private float[] _out_buf_r;

	private float[] _out_buf;

	private byte[] _byte_buf;

	private const int IN_BLOCK = 2048;

	private unsafe void* _rcvr_resamp_l = null;

	private unsafe void* _rcvr_resamp_r = null;

	private unsafe void* _xmtr_resamp_l = null;

	private unsafe void* _xmtr_resamp_r = null;

	private int _sample_rate;

	private int _rcvr_rate;

	private int _rcvr_size;

	private int _xmtr_rate;

	private int _xmtr_size;

	private volatile bool _mox;

	private volatile float _fInverseGain = 1f;

	private readonly string _file;

	private readonly Action<string, Exception> _finished;

	private Exception _failure;

	public unsafe void* RcvrResampL => _rcvr_resamp_l;

	public unsafe void* RcvrResampR => _rcvr_resamp_r;

	public unsafe void* XmtrResampL => _xmtr_resamp_l;

	public unsafe void* XmtrResampR => _xmtr_resamp_r;

	public int BaseRate => _sample_rate;

	public int RcvrRate => _rcvr_rate;

	public int RcvrSize => _rcvr_size;

	public int XmtrRate => _xmtr_rate;

	public int XmtrSize => _xmtr_size;

	public float RecordGain
	{
		set
		{
			UpdateMox();
			if (value <= 0f)
			{
				_fInverseGain = 0f;
				return;
			}
			if (value > 1f)
			{
				value = 1f;
			}
			_fInverseGain = 1f / value;
		}
	}

	public unsafe WaveFileWriter(int wfw_id, short chan, int samp_rate, string file, bool recordRxPreProcessed, bool recordTxPreProcessed, short formatTag, short bitDepth, Action<string, Exception> finished)
	{
		_id = wfw_id;
		_file = file;
		_finished = finished;
		WaveThing.wrecorder[_id].RxPre = recordRxPreProcessed;
		WaveThing.wrecorder[_id].TxPre = recordTxPreProcessed;
		if (recordRxPreProcessed)
		{
			_rcvr_rate = cmaster.GetInputRate(0, _id);
			_rcvr_size = cmaster.GetBuffSize(_rcvr_rate);
		}
		else
		{
			_rcvr_rate = cmaster.GetChannelOutputRate(0, _id);
			_rcvr_size = cmaster.GetBuffSize(_rcvr_rate);
		}
		if (recordTxPreProcessed)
		{
			_xmtr_rate = cmaster.GetInputRate(1, 0);
			_xmtr_size = cmaster.GetBuffSize(_xmtr_rate);
		}
		else
		{
			_xmtr_rate = cmaster.GetChannelOutputRate(1, 0);
			_xmtr_size = cmaster.GetBuffSize(_xmtr_rate);
		}
		switch (wfw_id)
		{
		case 0:
			RecordGain = (float)Audio.console.radio.GetDSPRX(0, 0).RXOutputGain;
			break;
		case 1:
			RecordGain = (float)Audio.console.radio.GetDSPRX(1, 0).RXOutputGain;
			break;
		}
		_channels = chan;
		_sample_rate = samp_rate;
		_format_tag = formatTag;
		_bit_depth = bitDepth;
		int num = (int)Math.Ceiling(2048.0 * (double)_sample_rate / (double)Math.Min(_rcvr_size, _xmtr_size));
		_rb_l = new RingBufferFloat(32768);
		_rb_r = new RingBufferFloat(32768);
		_in_buf_l = new float[2048];
		_in_buf_r = new float[2048];
		_out_buf_l = new float[num];
		_out_buf_r = new float[num];
		_out_buf = new float[num * 2];
		_byte_buf = new byte[num * 2 * 4];
		_length_counter = 0;
		_record = true;
		if (_sample_rate != _rcvr_rate)
		{
			_rcvr_resamp_l = WDSP.create_resampleFV(_rcvr_rate, _sample_rate);
			_rcvr_resamp_r = WDSP.create_resampleFV(_rcvr_rate, _sample_rate);
		}
		if (_sample_rate != _xmtr_rate)
		{
			_xmtr_resamp_l = WDSP.create_resampleFV(_xmtr_rate, _sample_rate);
			_xmtr_resamp_r = WDSP.create_resampleFV(_xmtr_rate, _sample_rate);
		}
		_writer = new BinaryWriter(File.Open(file, FileMode.Create));
		_thread = new Thread(ProcessRecordBuffers);
		_thread.Name = "Wave File Write Thread";
		_thread.IsBackground = true;
		_thread.Priority = ThreadPriority.Normal;
		_thread.Start();
	}

	public void UpdateMox()
	{
		switch (_id)
		{
		case 0:
			_mox = Audio.MOX && (Audio.console.VFOATX || (Audio.console.VFOBTX && !Audio.console.RX2Enabled));
			break;
		case 1:
			_mox = Audio.MOX && Audio.console.RX2Enabled && Audio.console.VFOBTX;
			break;
		default:
			_mox = Audio.MOX;
			break;
		}
	}

	private void ProcessRecordBuffers()
	{
		try
		{
			WriteWaveHeader(ref _writer, _channels, _sample_rate, _format_tag, _bit_depth, 0);
			while ((_record || _rb_l.ReadSpace() > 0) && _failure == null)
			{
				while ((_rb_l.ReadSpace() > 2048 || (!_record && _rb_l.ReadSpace() > 0)) && _failure == null)
				{
					WriteBuffer(ref _writer, ref _length_counter);
				}
				if (_failure == null)
				{
					Thread.Sleep(3);
				}
			}
		}
		catch (Exception failure)
		{
			_failure = failure;
			_record = false;
		}
		try
		{
			if (_writer != null)
			{
				if (_failure == null)
				{
					_writer.Seek(0, SeekOrigin.Begin);
					WriteWaveHeader(ref _writer, _channels, _sample_rate, _format_tag, _bit_depth, _length_counter);
					_writer.Flush();
				}
				_writer.Close();
			}
		}
		catch (Exception failure2)
		{
			if (_failure == null)
			{
				_failure = failure2;
			}
			try
			{
				_writer.Close();
			}
			catch
			{
			}
		}
		try
		{
			_finished?.Invoke(_file, _failure);
		}
		catch
		{
		}
	}

	public unsafe void AddWriteBuffer(float* left, float* right, int nsamps)
	{
		if (_record && _failure == null)
		{
			_rb_l.WritePtr(left, nsamps);
			_rb_r.WritePtr(right, nsamps);
		}
	}

	public void Stop()
	{
		_record = false;
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

	private void WriteBuffer(ref BinaryWriter w, ref int count)
	{
		float num = ((!_mox) ? _fInverseGain : 1f);
		int num2 = _rb_l.Read(_in_buf_l, 2048);
		int num3 = _rb_r.Read(_in_buf_r, 2048);
		if (num2 != num3)
		{
			return;
		}
		int num4 = num2;
		_in_buf_l.CopyTo(_out_buf_l, 0);
		_in_buf_r.CopyTo(_out_buf_r, 0);
		if (_channels > 1)
		{
			for (int i = 0; i < num4; i++)
			{
				_out_buf[i * 2] = _out_buf_l[i] * num;
				if (_out_buf[i * 2] > 1f)
				{
					_out_buf[i * 2] = 1f;
				}
				else if (_out_buf[i * 2] < -1f)
				{
					_out_buf[i * 2] = -1f;
				}
				_out_buf[i * 2 + 1] = _out_buf_r[i] * num;
				if (_out_buf[i * 2 + 1] > 1f)
				{
					_out_buf[i * 2 + 1] = 1f;
				}
				else if (_out_buf[i * 2 + 1] < -1f)
				{
					_out_buf[i * 2 + 1] = -1f;
				}
			}
		}
		else
		{
			for (int j = 0; j < num4; j++)
			{
				_out_buf_l[j] *= num;
				if (_out_buf_l[j] > 1f)
				{
					_out_buf_l[j] = 1f;
				}
				else if (_out_buf_l[j] < -1f)
				{
					_out_buf_l[j] = -1f;
				}
			}
			_out_buf_l.CopyTo(_out_buf, 0);
		}
		int num5 = num4;
		if (_channels > 1)
		{
			num5 *= 2;
		}
		if (_bit_depth == 32)
		{
			Write_32(num5, ref count, num4, w);
		}
		else if (_bit_depth == 24)
		{
			Write_24(num5, ref count, num4, w);
		}
		else if (_bit_depth == 16)
		{
			Write_16(num5, ref count, num4, w);
		}
		else if (_bit_depth == 8)
		{
			Write_8(num5, ref count, num4, w);
		}
	}

	private void Write_32(int length, ref int count, int out_cnt, BinaryWriter w)
	{
		for (int i = 0; i < length; i++)
		{
			if (_format_tag == 3)
			{
				byte[] bytes = BitConverter.GetBytes(_out_buf[i]);
				_byte_buf[i * 4] = bytes[0];
				_byte_buf[i * 4 + 1] = bytes[1];
				_byte_buf[i * 4 + 2] = bytes[2];
				_byte_buf[i * 4 + 3] = bytes[3];
			}
			else
			{
				int num = dither32(_out_buf[i] * 2.1474836E+09f);
				_byte_buf[i * 4 + 3] = (byte)(num >> 24);
				_byte_buf[i * 4 + 2] = (byte)((num >>> 16) & 0xFF);
				_byte_buf[i * 4 + 1] = (byte)((num >>> 8) & 0xFF);
				_byte_buf[i * 4] = (byte)(num & 0xFF);
			}
		}
		w.Write(_byte_buf, 0, out_cnt * 2 * 4);
		count += out_cnt * 2 * 4;
	}

	private void Write_24(int length, ref int count, int out_cnt, BinaryWriter w)
	{
		for (int i = 0; i < length; i++)
		{
			int num = dither24(_out_buf[i] * 8388608f);
			_byte_buf[i * 3 + 2] = (byte)(num >> 16);
			_byte_buf[i * 3 + 1] = (byte)((num >>> 8) & 0xFF);
			_byte_buf[i * 3] = (byte)(num & 0xFF);
		}
		w.Write(_byte_buf, 0, out_cnt * 2 * 3);
		count += out_cnt * 2 * 3;
	}

	private void Write_16(int length, ref int count, int out_cnt, BinaryWriter w)
	{
		for (int i = 0; i < length; i++)
		{
			int num = dither16(_out_buf[i] * 32768f);
			_byte_buf[i * 2 + 1] = (byte)(num >> 8);
			_byte_buf[i * 2] = (byte)(num & 0xFF);
		}
		w.Write(_byte_buf, 0, out_cnt * 2 * 2);
		count += out_cnt * 2 * 2;
	}

	private void Write_8(int length, ref int count, int out_cnt, BinaryWriter w)
	{
		for (int i = 0; i < length; i++)
		{
			sbyte b = dither8(_out_buf[i] * 128f);
			_byte_buf[i] = (byte)(b + 128);
		}
		w.Write(_byte_buf, 0, out_cnt * 2);
		count += out_cnt * 2;
	}

	private float getDitherAmp()
	{
		float num = DitherAmount;
		if (num < 0.1f)
		{
			num = 0.1f;
		}
		if (num > 1f)
		{
			num = 1f;
		}
		return num;
	}

	private int dither32(float sample)
	{
		if (DitherEnabled)
		{
			sample += (float)rnd.NextDouble() * getDitherAmp();
		}
		if (sample >= 2.1474836E+09f)
		{
			return int.MaxValue;
		}
		if (sample <= -2.1474836E+09f)
		{
			return int.MinValue;
		}
		return (int)((sample < 0f) ? (sample - 0.5f) : (sample + 0.5f));
	}

	private int dither24(float sample)
	{
		if (DitherEnabled)
		{
			sample += (float)rnd.NextDouble() * getDitherAmp();
		}
		if (sample >= 8388607f)
		{
			return 8388607;
		}
		if (sample <= -8388608f)
		{
			return -8388608;
		}
		return (int)((sample < 0f) ? (sample - 0.5f) : (sample + 0.5f));
	}

	private int dither16(float sample)
	{
		if (DitherEnabled)
		{
			sample += (float)rnd.NextDouble() * getDitherAmp();
		}
		if (sample >= 32767f)
		{
			return 32767;
		}
		if (sample <= -32768f)
		{
			return -32768;
		}
		return (int)((sample < 0f) ? (sample - 0.5f) : (sample + 0.5f));
	}

	private sbyte dither8(float sample)
	{
		if (DitherEnabled)
		{
			sample += (float)rnd.NextDouble() * getDitherAmp();
		}
		if (sample >= 127f)
		{
			return sbyte.MaxValue;
		}
		if (sample <= -128f)
		{
			return sbyte.MinValue;
		}
		return (sbyte)((sample < 0f) ? (sample - 0.5f) : (sample + 0.5f));
	}

	private static void WriteWaveHeader(ref BinaryWriter w, short channels, int sample_rate, short format_tag, short bit_depth, int data_length)
	{
		w.Write(1179011410);
		w.Write(data_length + 36);
		w.Write(1163280727);
		w.Write(544501094);
		w.Write(16);
		w.Write(format_tag);
		w.Write(channels);
		w.Write(sample_rate);
		w.Write(channels * sample_rate * bit_depth / 8);
		w.Write((short)(channels * bit_depth / 8));
		w.Write(bit_depth);
		w.Write(1635017060);
		w.Write(data_length);
		w.Flush();
	}
}
