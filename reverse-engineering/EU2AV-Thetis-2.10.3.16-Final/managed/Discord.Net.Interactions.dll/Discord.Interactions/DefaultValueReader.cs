using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal sealed class DefaultValueReader<T> : TypeReader<T> where T : IConvertible
{
	public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, string option, IServiceProvider services)
	{
		try
		{
			return Task.FromResult(TypeConverterResult.FromSuccess(Convert.ChangeType(option, typeof(T))));
		}
		catch (Exception ex) when (((ex is FormatException || ex is InvalidCastException) ? 1 : 0) != 0)
		{
			return Task.FromResult(TypeConverterResult.FromError(ex));
		}
	}
}
