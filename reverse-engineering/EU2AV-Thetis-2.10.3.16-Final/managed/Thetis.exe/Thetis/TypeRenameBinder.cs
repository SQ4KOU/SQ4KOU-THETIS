using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Thetis;

public sealed class TypeRenameBinder : SerializationBinder
{
	private readonly Dictionary<string, Type> _map;

	public TypeRenameBinder(Dictionary<string, Type> map)
	{
		_map = map;
	}

	public TypeRenameBinder Add(string oldFullName, Type newType)
	{
		_map[oldFullName] = newType;
		return this;
	}

	public override Type BindToType(string assemblyName, string typeName)
	{
		if (_map.TryGetValue(typeName, out var value))
		{
			return value;
		}
		return Type.GetType(typeName + ", " + assemblyName, throwOnError: true);
	}

	public static TypeRenameBinder Create()
	{
		return new TypeRenameBinder(new Dictionary<string, Type>(StringComparer.Ordinal));
	}
}
