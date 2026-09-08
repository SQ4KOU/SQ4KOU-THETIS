using System;
using System.Threading;
using System.Threading.Tasks;

namespace Discord.Audio.Streams;

public class OpusEncodeStream : AudioOutStream
{
	public const int SampleRate = 48000;

	private readonly AudioStream _next;

	private readonly OpusEncoder _encoder;

	private readonly byte[] _buffer;

	private int _partialFramePos;

	private ushort _seq;

	private uint _timestamp;

	public OpusEncodeStream(AudioStream next, int bitrate, AudioApplication application, int packetLoss)
	{
		_next = next;
		_encoder = new OpusEncoder(bitrate, application, packetLoss);
		_buffer = new byte[3840];
	}

	public async Task WriteSilentFramesAsync()
	{
		byte[] frameBytes = new byte[3840];
		frameBytes[0] = 248;
		frameBytes[1] = byte.MaxValue;
		frameBytes[2] = 254;
		_partialFramePos = 0;
		for (int i = 0; i < 5; i++)
		{
			await WriteAsync(frameBytes, 0, frameBytes.Length).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancelToken)
	{
		while (count > 0)
		{
			if (_partialFramePos == 0 && count >= 3840)
			{
				int count2 = _encoder.EncodeFrame(buffer, offset, _buffer, 0);
				_next.WriteHeader(_seq, _timestamp, missed: false);
				await _next.WriteAsync(_buffer, 0, count2, cancelToken).ConfigureAwait(continueOnCapturedContext: false);
				offset += 3840;
				count -= 3840;
				_seq++;
				_timestamp += 960u;
				continue;
			}
			if (_partialFramePos + count >= 3840)
			{
				int partialSize = 3840 - _partialFramePos;
				Buffer.BlockCopy(buffer, offset, _buffer, _partialFramePos, partialSize);
				int count3 = _encoder.EncodeFrame(_buffer, 0, _buffer, 0);
				_next.WriteHeader(_seq, _timestamp, missed: false);
				await _next.WriteAsync(_buffer, 0, count3, cancelToken).ConfigureAwait(continueOnCapturedContext: false);
				offset += partialSize;
				count -= partialSize;
				_partialFramePos = 0;
				_seq++;
				_timestamp += 960u;
				continue;
			}
			Buffer.BlockCopy(buffer, offset, _buffer, _partialFramePos, count);
			_partialFramePos += count;
			break;
		}
	}

	public override Task FlushAsync(CancellationToken cancelToken)
	{
		return _next.FlushAsync(cancelToken);
	}

	public override Task ClearAsync(CancellationToken cancelToken)
	{
		return _next.ClearAsync(cancelToken);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_encoder.Dispose();
			_next.Dispose();
		}
		base.Dispose(disposing);
	}
}
