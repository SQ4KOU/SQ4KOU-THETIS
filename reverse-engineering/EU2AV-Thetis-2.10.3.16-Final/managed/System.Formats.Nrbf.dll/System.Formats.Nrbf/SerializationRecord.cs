using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace System.Formats.Nrbf;

[DebuggerDisplay("{RecordType}, {Id}")]
public abstract class SerializationRecord
{
	public abstract SerializationRecordType RecordType { get; }

	public abstract SerializationRecordId Id { get; }

	public abstract TypeName TypeName { get; }

	internal SerializationRecord()
	{
	}

	public bool TypeNameMatches(Type type)
	{
		System.ExceptionPolyfills.ThrowIfNull(type, "type");
		return Matches(type, TypeName);
	}

	private static bool Matches(Type type, TypeName typeName)
	{
		if (type.IsPointer || type.IsByRef)
		{
			return false;
		}
		if (type.IsArray != typeName.IsArray || type.IsConstructedGenericType != typeName.IsConstructedGenericType || type.IsNested != typeName.IsNested || (type.IsArray && type.GetArrayRank() != typeName.GetArrayRank()) || (type.IsArray && type.Name != typeName.Name))
		{
			return false;
		}
		if (type.FullName == typeName.FullName)
		{
			return true;
		}
		if (typeName.IsArray)
		{
			return Matches(type.GetElementType(), typeName.GetElementType());
		}
		if (type.IsConstructedGenericType)
		{
			if (!Matches(type.GetGenericTypeDefinition(), typeName.GetGenericTypeDefinition()))
			{
				return false;
			}
			ImmutableArray<TypeName> genericArguments = typeName.GetGenericArguments();
			Type[] genericArguments2 = type.GetGenericArguments();
			if (genericArguments.Length != genericArguments2.Length)
			{
				return false;
			}
			for (int i = 0; i < genericArguments2.Length; i++)
			{
				if (!Matches(genericArguments2[i], genericArguments[i]))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	internal virtual object GetValue()
	{
		return this;
	}

	internal virtual void HandleNextRecord(SerializationRecord nextRecord, NextInfo info)
	{
		throw new InvalidOperationException();
	}

	internal virtual void HandleNextValue(object value, NextInfo info)
	{
		throw new InvalidOperationException();
	}
}
