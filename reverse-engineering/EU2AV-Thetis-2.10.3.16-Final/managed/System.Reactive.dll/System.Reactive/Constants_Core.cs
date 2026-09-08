namespace System.Reactive;

internal static class Constants_Core
{
	private const string ObsoleteRefactoring = "This property is no longer supported due to refactoring of the API surface and elimination of platform-specific dependencies.";

	public const string ObsoleteSchedulerNewthread = "This property is no longer supported due to refactoring of the API surface and elimination of platform-specific dependencies. Please use NewThreadScheduler.Default to obtain an instance of this scheduler type.";

	public const string ObsoleteSchedulerTaskpool = "This property is no longer supported due to refactoring of the API surface and elimination of platform-specific dependencies. Please use TaskPoolScheduler.Default to obtain an instance of this scheduler type.";

	public const string ObsoleteSchedulerThreadpool = "This property is no longer supported due to refactoring of the API surface and elimination of platform-specific dependencies. Consider using Scheduler.Default to obtain the platform's most appropriate pool-based scheduler. In order to access a specific pool-based scheduler, please add a reference to the System.Reactive.PlatformServices assembly for your target platform and use the appropriate scheduler in the System.Reactive.Concurrency namespace.";

	public const string ObsoleteSchedulerequired = "This instance property is no longer supported. Use CurrentThreadScheduler.IsScheduleRequired instead.";

	internal const string AsQueryableTrimIncompatibilityMessage = "This type uses Queryable.AsQueryable, which is not compatible with trimming because expressions referencing IQueryable extension methods can get rebound to IEnumerable extension methods, and those IEnumerable methods might be trimmed.";

	internal const string EventReflectionTrimIncompatibilityMessage = "This member uses reflection to discover event members and associated delegate types.";
}
