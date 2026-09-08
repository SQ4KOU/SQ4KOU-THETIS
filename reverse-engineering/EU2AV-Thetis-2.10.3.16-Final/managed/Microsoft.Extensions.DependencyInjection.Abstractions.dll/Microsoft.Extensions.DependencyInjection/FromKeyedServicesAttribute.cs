using System;

namespace Microsoft.Extensions.DependencyInjection;

[AttributeUsage(AttributeTargets.Parameter)]
public class FromKeyedServicesAttribute : Attribute
{
	public object? Key { get; }

	public ServiceKeyLookupMode LookupMode { get; }

	public FromKeyedServicesAttribute(object? key)
	{
		Key = key;
		LookupMode = ((key == null) ? ServiceKeyLookupMode.NullKey : ServiceKeyLookupMode.ExplicitKey);
	}

	public FromKeyedServicesAttribute()
	{
		Key = null;
		LookupMode = ServiceKeyLookupMode.InheritKey;
	}
}
