namespace System.Reactive.PlatformServices;

internal class DefaultSystemClockMonitor : PeriodicTimerSystemClockMonitor
{
	private static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(1.0);

	public DefaultSystemClockMonitor()
		: base(DefaultPeriod)
	{
	}
}
