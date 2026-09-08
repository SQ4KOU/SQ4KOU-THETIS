using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities;

internal abstract class ReflectionDelegateFactory
{
	[RequiresDynamicCode("Newtonsoft.Json relies on dynamically creating types that may not be available with Ahead of Time compilation.")]
	public Func<T, object?> CreateGet<T>(MemberInfo memberInfo)
	{
		if (memberInfo is PropertyInfo propertyInfo)
		{
			if (propertyInfo.PropertyType.IsByRef)
			{
				throw new InvalidOperationException("Could not create getter for {0}. ByRef return values are not supported.".FormatWith(CultureInfo.InvariantCulture, propertyInfo));
			}
			return CreateGet<T>(propertyInfo);
		}
		if (memberInfo is FieldInfo fieldInfo)
		{
			return CreateGet<T>(fieldInfo);
		}
		throw new Exception("Could not create getter for {0}.".FormatWith(CultureInfo.InvariantCulture, memberInfo));
	}

	[RequiresDynamicCode("Newtonsoft.Json relies on dynamically creating types that may not be available with Ahead of Time compilation.")]
	public Action<T, object?> CreateSet<T>(MemberInfo memberInfo)
	{
		if (memberInfo is PropertyInfo propertyInfo)
		{
			return CreateSet<T>(propertyInfo);
		}
		if (memberInfo is FieldInfo fieldInfo)
		{
			return CreateSet<T>(fieldInfo);
		}
		throw new Exception("Could not create setter for {0}.".FormatWith(CultureInfo.InvariantCulture, memberInfo));
	}

	[RequiresDynamicCode("Newtonsoft.Json relies on dynamically creating types that may not be available with Ahead of Time compilation.")]
	public abstract MethodCall<T, object?> CreateMethodCall<T>(MethodBase method);

	[RequiresDynamicCode("Newtonsoft.Json relies on dynamically creating types that may not be available with Ahead of Time compilation.")]
	public abstract ObjectConstructor<object> CreateParameterizedConstructor(MethodBase method);

	[RequiresDynamicCode("Newtonsoft.Json relies on dynamically creating types that may not be available with Ahead of Time compilation.")]
	public abstract Func<T> CreateDefaultConstructor<T>([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type type);

	[RequiresDynamicCode("Newtonsoft.Json relies on dynamically creating types that may not be available with Ahead of Time compilation.")]
	public abstract Func<T, object?> CreateGet<T>(PropertyInfo propertyInfo);

	[RequiresDynamicCode("Newtonsoft.Json relies on dynamically creating types that may not be available with Ahead of Time compilation.")]
	public abstract Func<T, object?> CreateGet<T>(FieldInfo fieldInfo);

	[RequiresDynamicCode("Newtonsoft.Json relies on dynamically creating types that may not be available with Ahead of Time compilation.")]
	public abstract Action<T, object?> CreateSet<T>(FieldInfo fieldInfo);

	[RequiresDynamicCode("Newtonsoft.Json relies on dynamically creating types that may not be available with Ahead of Time compilation.")]
	public abstract Action<T, object?> CreateSet<T>(PropertyInfo propertyInfo);
}
