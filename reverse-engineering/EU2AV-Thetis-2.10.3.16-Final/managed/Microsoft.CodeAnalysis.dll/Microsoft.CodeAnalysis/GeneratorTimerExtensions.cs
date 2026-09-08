using System;
using System.Diagnostics.Tracing;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal static class GeneratorTimerExtensions
{
	internal readonly struct RunTimer : IDisposable
	{
		private readonly SharedStopwatch _timer = SharedStopwatch.StartNew();

		private readonly Action<TimeSpan>? _callback = null;

		private readonly Func<TimeSpan, TimeSpan>? _adjustRunTime = null;

		public TimeSpan Elapsed
		{
			get
			{
				if (_adjustRunTime == null)
				{
					return _timer.Elapsed;
				}
				return _adjustRunTime(_timer.Elapsed);
			}
		}

		public RunTimer()
		{
		}

		public RunTimer(Func<TimeSpan, TimeSpan>? adjustRunTime)
			: this()
		{
			_adjustRunTime = adjustRunTime;
		}

		public RunTimer(Action<TimeSpan> callback, Func<TimeSpan, TimeSpan>? adjustRunTime = null)
			: this(adjustRunTime)
		{
			_callback = callback;
		}

		public void Dispose()
		{
			if (_callback != null)
			{
				_callback(Elapsed);
			}
		}
	}

	public static RunTimer CreateGeneratorDriverRunTimer(this CodeAnalysisEventSource eventSource)
	{
		if (eventSource.IsEnabled(EventLevel.Informational, (EventKeywords)1L))
		{
			string id = Guid.NewGuid().ToString();
			eventSource.StartGeneratorDriverRunTime(id);
			return new RunTimer(delegate(TimeSpan t)
			{
				eventSource.StopGeneratorDriverRunTime(t.Ticks, id);
			});
		}
		return new RunTimer();
	}

	public static RunTimer CreateSingleGeneratorRunTimer(this CodeAnalysisEventSource eventSource, ISourceGenerator generator, Func<TimeSpan, TimeSpan> adjustRunTime)
	{
		if (eventSource.IsEnabled(EventLevel.Informational, (EventKeywords)1L))
		{
			string id = Guid.NewGuid().ToString();
			Type type = generator.GetGeneratorType();
			eventSource.StartSingleGeneratorRunTime(type.FullName, type.Assembly.Location, id);
			return new RunTimer(delegate(TimeSpan t)
			{
				eventSource.StopSingleGeneratorRunTime(type.FullName, type.Assembly.Location, t.Ticks, id);
			}, adjustRunTime);
		}
		return new RunTimer(adjustRunTime);
	}
}
