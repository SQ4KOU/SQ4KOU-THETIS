using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

public abstract class ComponentTypeConverter : ITypeConverter<IComponentInteractionData>
{
	public abstract bool CanConvertTo(Type type);

	public abstract Task<TypeConverterResult> ReadAsync(IInteractionContext context, IComponentInteractionData option, IServiceProvider services);
}
public abstract class ComponentTypeConverter<T> : ComponentTypeConverter
{
	public sealed override bool CanConvertTo(Type type)
	{
		return typeof(T).IsAssignableFrom(type);
	}
}
