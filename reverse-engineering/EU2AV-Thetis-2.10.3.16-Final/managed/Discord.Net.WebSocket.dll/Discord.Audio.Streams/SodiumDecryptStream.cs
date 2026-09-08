using System;
using System.Threading;
using System.Threading.Tasks;

namespace Discord.Audio.Streams;

public class SodiumDecryptStream : AudioOutStream
{
	private readonly AudioClient _client;

	private readonly AudioStream _next;

	private readonly byte[] _nonce;

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => true;

	public SodiumDecryptStream(AudioStream next, IAudioClient client)
	{
		_next = next;
		_client = (AudioClient)client;
		_nonce = new byte[24];
	}

	public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancelToken)
	{
		cancelToken.ThrowIfCancellationRequested();
		if (_client.SecretKey == null)
		{
			return Task.CompletedTask;
		}
		Buffer.BlockCopy(buffer, 0, _nonce, 0, 12);
		count = SecretBox.Decrypt(buffer, offset + 12, count - 12, buffer, offset + 12, _nonce, _client.SecretKey);
		return _next.WriteAsync(buffer, 0, count + 12, cancelToken);
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
			_next.Dispose();
		}
		base.Dispose(disposing);
	}
}
