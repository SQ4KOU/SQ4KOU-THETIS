using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Discord.Interactions.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Discord.Interactions;

public abstract class CommandInfo<TParameter> : ICommandInfo where TParameter : class, IParameterInfo
{
	private readonly ExecuteCallback _action;

	private readonly ILookup<string, PreconditionAttribute> _groupedPreconditions;

	internal IReadOnlyDictionary<string, TParameter> _parameterDictionary { get; }

	public ModuleInfo Module { get; }

	public InteractionService CommandService { get; }

	public string Name { get; }

	public string MethodName { get; }

	public virtual bool IgnoreGroupNames { get; }

	public abstract bool SupportsWildCards { get; }

	public bool IsTopLevelCommand { get; }

	public RunMode RunMode { get; }

	public IReadOnlyCollection<Attribute> Attributes { get; }

	public IReadOnlyCollection<PreconditionAttribute> Preconditions { get; }

	public abstract IReadOnlyList<TParameter> Parameters { get; }

	public bool TreatNameAsRegex { get; }

	IReadOnlyCollection<IParameterInfo> ICommandInfo.Parameters => Parameters;

	internal CommandInfo(ICommandBuilder builder, ModuleInfo module, InteractionService commandService)
	{
		CommandService = commandService;
		Module = module;
		Name = builder.Name;
		MethodName = builder.MethodName;
		IgnoreGroupNames = builder.IgnoreGroupNames;
		IsTopLevelCommand = IgnoreGroupNames || CheckTopLevel(Module);
		RunMode = ((builder.RunMode != RunMode.Default) ? builder.RunMode : commandService._runMode);
		Attributes = builder.Attributes.ToImmutableArray();
		Preconditions = builder.Preconditions.ToImmutableArray();
		TreatNameAsRegex = builder.TreatNameAsRegex && SupportsWildCards;
		_action = builder.Callback;
		_groupedPreconditions = builder.Preconditions.ToLookup((PreconditionAttribute x) => x.Group, (PreconditionAttribute x) => x, StringComparer.Ordinal);
		_parameterDictionary = Parameters?.ToDictionary((TParameter x) => x.Name, (TParameter x) => x).ToImmutableDictionary();
	}

	public virtual Task<IResult> ExecuteAsync(IInteractionContext context, IServiceProvider services)
	{
		switch (RunMode)
		{
		case RunMode.Sync:
			return ExecuteInternalAsync(context, services);
		case RunMode.Async:
			Task.Run(async delegate
			{
				await ExecuteInternalAsync(context, services).ConfigureAwait(continueOnCapturedContext: false);
			});
			return Task.FromResult((IResult)ExecuteResult.FromSuccess());
		default:
			throw new InvalidOperationException($"RunMode {RunMode} is not supported.");
		}
	}

	protected abstract Task<IResult> ParseArgumentsAsync(IInteractionContext context, IServiceProvider services);

	private async Task<IResult> ExecuteInternalAsync(IInteractionContext context, IServiceProvider services)
	{
		await CommandService._cmdLogger.DebugAsync("Executing " + GetLogString(context)).ConfigureAwait(continueOnCapturedContext: false);
		IServiceScope scope = null;
		if (CommandService._autoServiceScopes)
		{
			scope = services?.CreateScope();
			services = scope?.ServiceProvider ?? EmptyServiceProvider.Instance;
		}
		object obj = null;
		int num = 0;
		IResult result = default(IResult);
		try
		{
			try
			{
				PreconditionResult preconditionResult = await CheckPreconditionsAsync(context, services).ConfigureAwait(continueOnCapturedContext: false);
				if (!preconditionResult.IsSuccess)
				{
					result = await InvokeEventAndReturn(context, preconditionResult).ConfigureAwait(continueOnCapturedContext: false);
				}
				else
				{
					IResult result2 = await ParseArgumentsAsync(context, services).ConfigureAwait(continueOnCapturedContext: false);
					if (!result2.IsSuccess)
					{
						result = await InvokeEventAndReturn(context, result2).ConfigureAwait(continueOnCapturedContext: false);
					}
					else if (!(result2 is ParseResult { Args: var args }))
					{
						result = ExecuteResult.FromError(InteractionCommandError.BadArgs, "Complex command parsing failed for an unknown reason.");
					}
					else
					{
						int index = 0;
						foreach (TParameter parameter in Parameters)
						{
							PreconditionResult preconditionResult2 = await parameter.CheckPreconditionsAsync(context, args[index++], services).ConfigureAwait(continueOnCapturedContext: false);
							if (preconditionResult2.IsSuccess)
							{
								continue;
							}
							result = await InvokeEventAndReturn(context, preconditionResult2).ConfigureAwait(continueOnCapturedContext: false);
							goto end_IL_01a6;
						}
						Task task = _action(context, args, services, this);
						if (task is Task<IResult> task2)
						{
							IResult result3 = await task2.ConfigureAwait(continueOnCapturedContext: false);
							await InvokeModuleEvent(context, result3).ConfigureAwait(continueOnCapturedContext: false);
							bool flag = ((result3 is RuntimeResult || result3 is ExecuteResult) ? true : false);
							result = (IResult)((!flag) ? ((object)(await InvokeEventAndReturn(context, ExecuteResult.FromError(InteractionCommandError.Unsuccessful, "Command execution failed for an unknown reason")).ConfigureAwait(continueOnCapturedContext: false))) : result3);
						}
						else
						{
							await task.ConfigureAwait(continueOnCapturedContext: false);
							result = await InvokeEventAndReturn(context, ExecuteResult.FromSuccess()).ConfigureAwait(continueOnCapturedContext: false);
						}
					}
				}
				end_IL_01a6:;
			}
			catch (Exception ex)
			{
				Exception ex2 = ex;
				Exception originalEx = ex2;
				while (ex2 is TargetInvocationException)
				{
					ex2 = ex2.InnerException;
				}
				InteractionException exception = new InteractionException(this, context, ex2);
				await Module.CommandService._cmdLogger.ErrorAsync(exception).ConfigureAwait(continueOnCapturedContext: false);
				ExecuteResult result4 = ExecuteResult.FromError(ex2);
				await InvokeModuleEvent(context, result4).ConfigureAwait(continueOnCapturedContext: false);
				if (Module.CommandService._throwOnError)
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
				scope?.Dispose();
				result = result4;
			}
			num = 1;
		}
		catch (object obj2)
		{
			obj = obj2;
		}
		await CommandService._cmdLogger.VerboseAsync("Executed " + GetLogString(context)).ConfigureAwait(continueOnCapturedContext: false);
		scope?.Dispose();
		object obj3 = obj;
		if (obj3 != null)
		{
			ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
		}
		if (num == 1)
		{
			return result;
		}
		IResult result5 = default(IResult);
		return result5;
	}

	protected abstract Task InvokeModuleEvent(IInteractionContext context, IResult result);

	protected abstract string GetLogString(IInteractionContext context);

	public async Task<PreconditionResult> CheckPreconditionsAsync(IInteractionContext context, IServiceProvider services)
	{
		PreconditionResult preconditionResult = await CheckGroups(Module.GroupedPreconditions, "Module").ConfigureAwait(continueOnCapturedContext: false);
		if (!preconditionResult.IsSuccess)
		{
			return preconditionResult;
		}
		PreconditionResult preconditionResult2 = await CheckGroups(_groupedPreconditions, "Command").ConfigureAwait(continueOnCapturedContext: false);
		return (!preconditionResult2.IsSuccess) ? preconditionResult2 : PreconditionResult.FromSuccess();
		async Task<PreconditionResult> CheckGroups(ILookup<string, PreconditionAttribute> preconditions, string type)
		{
			foreach (IGrouping<string, PreconditionAttribute> preconditionGroup in preconditions)
			{
				if (preconditionGroup.Key == null)
				{
					foreach (PreconditionAttribute item in preconditionGroup)
					{
						PreconditionResult preconditionResult3 = await item.CheckRequirementsAsync(context, this, services).ConfigureAwait(continueOnCapturedContext: false);
						if (!preconditionResult3.IsSuccess)
						{
							return preconditionResult3;
						}
					}
				}
				else
				{
					List<PreconditionResult> results = new List<PreconditionResult>();
					foreach (PreconditionAttribute item2 in preconditionGroup)
					{
						List<PreconditionResult> list = results;
						list.Add(await item2.CheckRequirementsAsync(context, this, services).ConfigureAwait(continueOnCapturedContext: false));
					}
					if (!results.Any((PreconditionResult p) => p.IsSuccess))
					{
						return PreconditionGroupResult.FromError(type + " precondition group " + preconditionGroup.Key + " failed.", results);
					}
				}
			}
			return PreconditionGroupResult.FromSuccess();
		}
	}

	protected async Task<T> InvokeEventAndReturn<T>(IInteractionContext context, T result) where T : IResult
	{
		await InvokeModuleEvent(context, result).ConfigureAwait(continueOnCapturedContext: false);
		return result;
	}

	private static bool CheckTopLevel(ModuleInfo parent)
	{
		for (ModuleInfo moduleInfo = parent; moduleInfo != null; moduleInfo = moduleInfo.Parent)
		{
			if (moduleInfo.IsSlashGroup)
			{
				return false;
			}
		}
		return true;
	}

	public override string ToString()
	{
		List<string> list = new List<string>();
		for (ModuleInfo moduleInfo = Module; moduleInfo != null; moduleInfo = moduleInfo.Parent)
		{
			if (moduleInfo.IsSlashGroup)
			{
				list.Add(moduleInfo.SlashGroupName);
			}
		}
		list.Reverse();
		list.Add(Name);
		return string.Join(" ", list);
	}
}
