using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Discord.Interactions;

public abstract class AutocompleteHandler : IAutocompleteHandler
{
	public InteractionService InteractionService { get; set; }

	public abstract Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services);

	protected virtual string GetLogString(IInteractionContext context)
	{
		IAutocompleteInteraction autocompleteInteraction = context.Interaction as IAutocompleteInteraction;
		return autocompleteInteraction.Data.CommandName + ": " + autocompleteInteraction.Data.Current.Name + " Autocomplete";
	}

	public Task<IResult> ExecuteAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
	{
		switch (InteractionService._runMode)
		{
		case RunMode.Sync:
			return ExecuteInternalAsync(context, autocompleteInteraction, parameter, services);
		case RunMode.Async:
			Task.Run(async delegate
			{
				await ExecuteInternalAsync(context, autocompleteInteraction, parameter, services).ConfigureAwait(continueOnCapturedContext: false);
			});
			return Task.FromResult((IResult)ExecuteResult.FromSuccess());
		default:
			throw new InvalidOperationException($"RunMode {InteractionService._runMode} is not supported.");
		}
	}

	private async Task<IResult> ExecuteInternalAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
	{
		object obj = null;
		int num = 0;
		IResult result4 = default(IResult);
		object obj4;
		try
		{
			try
			{
				AsyncServiceScope? scope = ((!InteractionService._autoServiceScopes) ? ((AsyncServiceScope?)null) : services?.CreateAsyncScope());
				object obj2 = null;
				int num2 = 0;
				IResult result3 = default(IResult);
				try
				{
					services = ((!InteractionService._autoServiceScopes) ? services : scope?.ServiceProvider) ?? EmptyServiceProvider.Instance;
					AutocompletionResult result = await GenerateSuggestionsAsync(context, autocompleteInteraction, parameter, services).ConfigureAwait(continueOnCapturedContext: false);
					if (result.IsSuccess)
					{
						Task task = autocompleteInteraction.RespondAsync(result.Suggestions);
						await task;
						if (task is Task<string> { Result: var result2 })
						{
							if (context is IRestInteractionContext { InteractionResponseCallback: not null } restInteractionContext)
							{
								await restInteractionContext.InteractionResponseCallback(result2).ConfigureAwait(continueOnCapturedContext: false);
							}
							else
							{
								await InteractionService._restResponseCallback(context, result2).ConfigureAwait(continueOnCapturedContext: false);
							}
						}
					}
					await InteractionService._autocompleteHandlerExecutedEvent.InvokeAsync(this, context, result).ConfigureAwait(continueOnCapturedContext: false);
					result3 = result;
					num2 = 1;
				}
				catch (object obj3)
				{
					obj2 = obj3;
				}
				if (scope.HasValue)
				{
					await ((IAsyncDisposable)scope.GetValueOrDefault()/*cast due to constrained. prefix*/).DisposeAsync();
				}
				obj4 = obj2;
				if (obj4 != null)
				{
					ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
				}
				if (num2 == 1)
				{
					result4 = result3;
					goto IL_06b5;
				}
			}
			catch (Exception ex)
			{
				Exception ex2 = ex;
				Exception originalEx = ex2;
				while (ex2 is TargetInvocationException)
				{
					ex2 = ex2.InnerException;
				}
				await InteractionService._cmdLogger.ErrorAsync(ex2).ConfigureAwait(continueOnCapturedContext: false);
				ExecuteResult result5 = ExecuteResult.FromError(ex2);
				await InteractionService._autocompleteHandlerExecutedEvent.InvokeAsync(this, context, result5).ConfigureAwait(continueOnCapturedContext: false);
				if (InteractionService._throwOnError)
				{
					if (ex2 == originalEx)
					{
						ExceptionDispatchInfo.Capture((ex as Exception) ?? throw ex).Throw();
					}
					else
					{
						ExceptionDispatchInfo.Capture(ex2).Throw();
					}
				}
				result4 = result5;
				goto IL_06b5;
			}
			goto end_IL_004e;
			IL_06b5:
			num = 1;
			end_IL_004e:;
		}
		catch (object obj3)
		{
			obj = obj3;
		}
		await InteractionService._cmdLogger.VerboseAsync("Executed " + GetLogString(context)).ConfigureAwait(continueOnCapturedContext: false);
		obj4 = obj;
		if (obj4 != null)
		{
			ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
		}
		if (num == 1)
		{
			return result4;
		}
		IResult result6 = default(IResult);
		return result6;
	}
}
