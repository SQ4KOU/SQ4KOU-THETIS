using System;
using System.Text;

namespace Discord;

public struct LogMessage
{
	public LogSeverity Severity { get; }

	public string Source { get; }

	public string Message { get; }

	public Exception Exception { get; }

	public LogMessage(LogSeverity severity, string source, string message, Exception exception = null)
	{
		Severity = severity;
		Source = source;
		Message = message;
		Exception = exception;
	}

	public override string ToString()
	{
		return ToString(null, fullException: true, prependTimestamp: true, DateTimeKind.Local, 11);
	}

	public string ToString(StringBuilder builder = null, bool fullException = true, bool prependTimestamp = true, DateTimeKind timestampKind = DateTimeKind.Local, int? padSource = 11)
	{
		string source = Source;
		string message = Message;
		string text = ((!fullException) ? Exception?.Message : Exception?.ToString());
		int capacity = 1 + (prependTimestamp ? 8 : 0) + 1 + (padSource.HasValue ? padSource.Value : (source?.Length ?? 0)) + 1 + (message?.Length ?? 0) + (text?.Length ?? 0) + 3;
		if (builder == null)
		{
			builder = new StringBuilder(capacity);
		}
		else
		{
			builder.Clear();
			builder.EnsureCapacity(capacity);
		}
		if (prependTimestamp)
		{
			DateTime dateTime = ((timestampKind != DateTimeKind.Utc) ? DateTime.Now : DateTime.UtcNow);
			string text2 = "HH:mm:ss";
			builder.Append(dateTime.ToString(text2));
			builder.Append(' ');
		}
		if (source != null)
		{
			if (padSource.HasValue)
			{
				if (source.Length < padSource.Value)
				{
					builder.Append(source);
					builder.Append(' ', padSource.Value - source.Length);
				}
				else if (source.Length > padSource.Value)
				{
					builder.Append(source.Substring(0, padSource.Value));
				}
				else
				{
					builder.Append(source);
				}
			}
			builder.Append(' ');
		}
		if (!string.IsNullOrEmpty(Message))
		{
			foreach (char c in message)
			{
				if (!char.IsControl(c))
				{
					builder.Append(c);
				}
			}
		}
		if (text != null)
		{
			if (!string.IsNullOrEmpty(Message))
			{
				builder.Append(':');
				builder.AppendLine();
			}
			builder.Append(text);
		}
		return builder.ToString();
	}
}
