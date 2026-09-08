using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

[RequiresUnreferencedCode("Newtonsoft.Json relies on reflection over types that may be removed when trimming.")]
[RequiresDynamicCode("Newtonsoft.Json relies on dynamically creating types that may not be available with Ahead of Time compilation.")]
internal static class CachedAttributeGetter<T> where T : Attribute
{
	private static readonly ThreadSafeStore<object, T?> TypeAttributeCache = new ThreadSafeStore<object, T>(JsonTypeReflector.GetAttribute<T>);

	public static T? GetAttribute(object type)
	{
		return TypeAttributeCache.Get(type);
	}
}
