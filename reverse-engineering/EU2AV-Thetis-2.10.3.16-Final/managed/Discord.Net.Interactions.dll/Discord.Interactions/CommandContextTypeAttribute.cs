using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class CommandContextTypeAttribute : Attribute
{
	public IReadOnlyCollection<InteractionContextType> ContextTypes { get; }

	public CommandContextTypeAttribute(params InteractionContextType[] contextTypes)
	{
		if (contextTypes == null)
		{
			throw new ArgumentNullException("contextTypes");
		}
		ContextTypes = contextTypes.Distinct().ToImmutableArray();
		if (ContextTypes.Count == 0)
		{
			throw new ArgumentException("A command must have at least one supported context type.", "contextTypes");
		}
	}
}
