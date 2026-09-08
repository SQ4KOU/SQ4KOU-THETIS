namespace Microsoft.CodeAnalysis.Emit;

internal enum HotReloadExceptionCode
{
	DeletedLambdaInvoked = 1,
	DeletedMethodInvoked,
	CannotResumeSuspendedIteratorMethod,
	CannotResumeSuspendedAsyncMethod,
	UnsupportedChangeToCapturedVariables
}
