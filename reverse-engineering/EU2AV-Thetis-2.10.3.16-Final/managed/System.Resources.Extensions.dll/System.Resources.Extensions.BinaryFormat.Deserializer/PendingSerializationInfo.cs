using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Nrbf;
using System.Reflection;
using System.Runtime.Serialization;

namespace System.Resources.Extensions.BinaryFormat.Deserializer;

internal sealed class PendingSerializationInfo
{
	private readonly ISerializationSurrogate _surrogate;

	private readonly SerializationInfo _info;

	internal SerializationRecordId ObjectId { get; }

	internal PendingSerializationInfo(SerializationRecordId objectId, SerializationInfo info, ISerializationSurrogate surrogate)
	{
		ObjectId = objectId;
		_surrogate = surrogate;
		_info = info;
	}

	[RequiresUnreferencedCode("We can't guarantee that the ctor will be present, as the type is not known up-front.")]
	internal void Populate(IDictionary<SerializationRecordId, object> objects, StreamingContext context)
	{
		object obj = objects[ObjectId];
		Type type = obj.GetType();
		if (_surrogate != null)
		{
			object obj2 = _surrogate.SetObjectData(obj, _info, context, null);
			if (obj2 != null)
			{
				if (!type.IsValueType && obj2 != obj)
				{
					throw new SerializationException(System.SR.Serialization_Surrogates);
				}
				objects[ObjectId] = obj2;
			}
		}
		else
		{
			GetDeserializationConstructor(type).Invoke(obj, new object[2] { _info, context });
		}
	}

	private static ConstructorInfo GetDeserializationConstructor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type type)
	{
		ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (ConstructorInfo constructorInfo in constructors)
		{
			ParameterInfo[] parameters = constructorInfo.GetParameters();
			if (parameters.Length == 2 && parameters[0].ParameterType == typeof(SerializationInfo) && parameters[1].ParameterType == typeof(StreamingContext))
			{
				return constructorInfo;
			}
		}
		throw new SerializationException(System.SR.Format(System.SR.Serialization_MissingCtor, type.FullName));
	}
}
