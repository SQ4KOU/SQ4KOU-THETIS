using System.Diagnostics.CodeAnalysis;
using System.Formats.Nrbf;
using System.Runtime.Serialization;

namespace System.Resources.Extensions.BinaryFormat.Deserializer;

internal abstract class ClassRecordDeserializer : ObjectRecordDeserializer
{
	private readonly bool _onlyAllowPrimitives;

	private protected ClassRecordDeserializer(ClassRecord classRecord, object @object, IDeserializer deserializer)
		: base(classRecord, deserializer)
	{
		base.Object = @object;
		_onlyAllowPrimitives = @object is IObjectReference;
	}

	[RequiresUnreferencedCode("Calls System.Windows.Forms.BinaryFormat.BinaryFormattedObject.TypeResolver.GetType(TypeName)")]
	internal static ObjectRecordDeserializer Create(ClassRecord classRecord, IDeserializer deserializer)
	{
		Type type = deserializer.TypeResolver.GetType(classRecord.TypeName);
		_ = classRecord.Id;
		ISerializationSurrogate surrogate = deserializer.GetSurrogate(type);
		if (!type.IsSerializable && surrogate == null)
		{
			throw new SerializationException(System.SR.Format(System.SR.Serialization_TypeNotSerializable, type));
		}
		object uninitializedObject = FormatterServices.GetUninitializedObject(type);
		SerializationEvents.GetOnDeserializingForType(type, uninitializedObject)?.Invoke(deserializer.Options.StreamingContext);
		if (surrogate != null || typeof(ISerializable).IsAssignableFrom(type))
		{
			return new ClassRecordSerializationInfoDeserializer(classRecord, uninitializedObject, type, surrogate, deserializer);
		}
		return new ClassRecordFieldInfoDeserializer(classRecord, uninitializedObject, type, deserializer);
	}

	private protected override void ValidateNewMemberObjectValue(object value)
	{
		if (_onlyAllowPrimitives)
		{
			Type type = value.GetType();
			if (type.IsArray)
			{
				type = type.GetElementType();
			}
			if (!type.IsPrimitive && !type.IsEnum && !(type == typeof(string)))
			{
				throw new SerializationException(System.SR.Format(System.SR.Serialization_IObjectReferenceOnlyPrimivite, type));
			}
		}
	}
}
