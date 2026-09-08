using System.Diagnostics.CodeAnalysis;
using System.Formats.Nrbf;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;

namespace System.Resources.Extensions.BinaryFormat.Deserializer;

internal sealed class ClassRecordFieldInfoDeserializer : ClassRecordDeserializer
{
	private readonly ClassRecord _classRecord;

	private readonly MemberInfo[] _fieldInfo;

	private int _currentFieldIndex;

	private readonly bool _isValueType;

	private bool _hasFixups;

	internal ClassRecordFieldInfoDeserializer(ClassRecord classRecord, object @object, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type, IDeserializer deserializer)
		: base(classRecord, @object, deserializer)
	{
		_classRecord = classRecord;
		_fieldInfo = FormatterServices.GetSerializableMembers(type);
		_isValueType = type.IsValueType;
	}

	internal override SerializationRecordId Continue()
	{
		while (_currentFieldIndex < _fieldInfo.Length)
		{
			FieldInfo fieldInfo = (FieldInfo)_fieldInfo[_currentFieldIndex];
			if (!_classRecord.HasMember(fieldInfo.Name))
			{
				if (base.Deserializer.Options.AssemblyMatching == FormatterAssemblyStyle.Simple || fieldInfo.GetCustomAttribute<OptionalFieldAttribute>() != null)
				{
					_currentFieldIndex++;
					continue;
				}
				throw new SerializationException(System.SR.Format(System.SR.Serialization_MissingField, fieldInfo.Name, fieldInfo.DeclaringType.Name));
			}
			var (obj, serializationRecordId) = UnwrapMemberValue(_classRecord.GetRawValue(fieldInfo.Name));
			if (ObjectRecordDeserializer.s_missingValueSentinel == obj)
			{
				return serializationRecordId;
			}
			fieldInfo.SetValue(base.Object, obj);
			if (obj != null && DoesValueNeedUpdated(obj, serializationRecordId))
			{
				_hasFixups = true;
				base.Deserializer.PendValueUpdater(new FieldValueUpdater(_classRecord.Id, serializationRecordId, fieldInfo));
			}
			_currentFieldIndex++;
		}
		if (!_hasFixups || !_isValueType)
		{
			base.Deserializer.CompleteObject(_classRecord.Id);
		}
		return default(SerializationRecordId);
	}
}
