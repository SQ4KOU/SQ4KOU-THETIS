using System.Diagnostics;
using System.Formats.Nrbf.Utils;
using System.Reflection.Metadata;

namespace System.Formats.Nrbf;

[DebuggerDisplay("{Value}")]
public abstract class PrimitiveTypeRecord : SerializationRecord
{
	public object Value => GetValue();

	private protected PrimitiveTypeRecord()
	{
	}
}
[DebuggerDisplay("{Value}")]
public abstract class PrimitiveTypeRecord<T> : PrimitiveTypeRecord
{
	public new T Value { get; }

	public override TypeName TypeName => TypeNameHelpers.GetPrimitiveTypeName(TypeNameHelpers.GetPrimitiveType<T>());

	private protected PrimitiveTypeRecord(T value)
	{
		Value = value;
	}

	internal override object GetValue()
	{
		return Value;
	}
}
