namespace Microsoft.CodeAnalysis.Emit;

internal static class HotReloadExceptionCodeExtensions
{
	public static string GetExceptionMessage(this HotReloadExceptionCode code)
	{
		return code switch
		{
			HotReloadExceptionCode.DeletedLambdaInvoked => CodeAnalysisResources.EncDeletedLambdaInvoked, 
			HotReloadExceptionCode.DeletedMethodInvoked => CodeAnalysisResources.EncDeletedMethodInvoked, 
			HotReloadExceptionCode.CannotResumeSuspendedIteratorMethod => CodeAnalysisResources.EncCannotResumeSuspendedIteratorMethod, 
			HotReloadExceptionCode.CannotResumeSuspendedAsyncMethod => CodeAnalysisResources.EncCannotResumeSuspendedAsyncMethod, 
			HotReloadExceptionCode.UnsupportedChangeToCapturedVariables => CodeAnalysisResources.EncLambdaRudeEdit_CapturedVariables, 
			_ => throw ExceptionUtilities.UnexpectedValue(code), 
		};
	}

	public static int GetExceptionCodeValue(this HotReloadExceptionCode code)
	{
		return 0 - code;
	}
}
