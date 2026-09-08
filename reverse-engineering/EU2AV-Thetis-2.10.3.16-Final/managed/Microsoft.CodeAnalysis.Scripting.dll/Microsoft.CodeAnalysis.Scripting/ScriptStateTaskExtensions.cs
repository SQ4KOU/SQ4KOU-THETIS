using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Scripting;

internal static class ScriptStateTaskExtensions
{
	internal static async Task<T> CastAsync<S, T>(this Task<S> task) where S : T
	{
		return (T)(object)(await task.ConfigureAwait(continueOnCapturedContext: true));
	}

	internal static async Task<T> GetEvaluationResultAsync<T>(this Task<ScriptState<T>> task)
	{
		return (await task.ConfigureAwait(continueOnCapturedContext: true)).ReturnValue;
	}
}
