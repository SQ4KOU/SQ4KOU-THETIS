using System;
using System.IO;

namespace Discord;

public struct FileAttachment : IDisposable
{
	private bool _isDisposed;

	public string FileName { get; set; }

	public string Description { get; set; }

	public bool IsSpoiler { get; set; }

	public bool IsThumbnail { get; set; }

	public double? DurationSeconds { get; set; }

	public byte[] Waveform { get; set; }

	public Stream Stream { get; }

	public FileAttachment(Stream stream, string fileName, string description = null, bool isSpoiler = false, bool isThumbnail = false, double? durationSeconds = null, byte[] waveform = null)
	{
		_isDisposed = false;
		FileName = fileName;
		Description = description;
		Stream = stream;
		IsThumbnail = isThumbnail;
		try
		{
			Stream.Position = 0L;
		}
		catch
		{
		}
		IsSpoiler = isSpoiler;
		DurationSeconds = durationSeconds;
		Waveform = waveform;
	}

	public FileAttachment(string path, string fileName = null, string description = null, bool isSpoiler = false, bool isThumbnail = false, double? durationSeconds = null, byte[] waveform = null)
	{
		_isDisposed = false;
		Stream = File.OpenRead(path);
		FileName = fileName ?? Path.GetFileName(path);
		Description = description;
		IsSpoiler = isSpoiler;
		IsThumbnail = isThumbnail;
		DurationSeconds = durationSeconds;
		Waveform = waveform;
	}

	public void Dispose()
	{
		if (!_isDisposed)
		{
			Stream?.Dispose();
			_isDisposed = true;
		}
	}

	public string GetAttachmentUrl()
	{
		return "attachment://" + FileName;
	}
}
