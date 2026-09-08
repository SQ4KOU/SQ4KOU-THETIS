namespace System.Formats.Nrbf;

public abstract class SZArrayRecord<T> : ArrayRecord
{
	public int Length => base.ArrayInfo.GetSZArrayLength();

	public override ReadOnlySpan<int> Lengths => new int[1] { Length };

	private protected SZArrayRecord(ArrayInfo arrayInfo)
		: base(arrayInfo)
	{
	}

	public abstract T?[] GetArray(bool allowNulls = true);

	private protected override Array Deserialize(Type arrayType, bool allowNulls)
	{
		return GetArray(allowNulls);
	}
}
