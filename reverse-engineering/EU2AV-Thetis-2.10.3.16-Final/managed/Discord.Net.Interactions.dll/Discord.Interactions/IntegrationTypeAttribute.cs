using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class IntegrationTypeAttribute : Attribute
{
	public IReadOnlyCollection<ApplicationIntegrationType> IntegrationTypes { get; }

	public IntegrationTypeAttribute(params ApplicationIntegrationType[] integrationTypes)
	{
		if (integrationTypes == null)
		{
			throw new ArgumentNullException("integrationTypes");
		}
		IntegrationTypes = integrationTypes.Distinct().ToImmutableArray();
		if (integrationTypes.Length == 0)
		{
			throw new ArgumentException("A command must have at least one integration type.", "integrationTypes");
		}
	}
}
