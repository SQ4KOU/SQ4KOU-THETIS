namespace System.Reflection.Internal;

internal sealed class ExternalMemoryBlockProvider : System.Reflection.Internal.MemoryBlockProvider
{
	private unsafe byte* _memory;

	private int _size;

	public override int Size => _size;

	public unsafe byte* Pointer => _memory;

	public unsafe ExternalMemoryBlockProvider(byte* memory, int size)
	{
		_memory = memory;
		_size = size;
	}

	protected unsafe override System.Reflection.Internal.AbstractMemoryBlock GetMemoryBlockImpl(int start, int size)
	{
		return new System.Reflection.Internal.ExternalMemoryBlock(this, _memory + start, size);
	}

	protected unsafe override void Dispose(bool disposing)
	{
		_memory = null;
		_size = 0;
	}
}
