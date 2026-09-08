using System;

namespace Thetis;

internal sealed class TCIPendingFloatBuffer
{
	private float[] m_buffer;

	private int m_readIndex;

	private int m_count;

	public int Count => m_count;

	public TCIPendingFloatBuffer(int initialCapacity = 16)
	{
		m_buffer = new float[Math.Max(16, initialCapacity)];
	}

	public void Enqueue(float[] source, int sourceOffset, int count)
	{
		if (source != null && count > 0)
		{
			ensureCapacity(count);
			Array.Copy(source, sourceOffset, m_buffer, m_readIndex + m_count, count);
			m_count += count;
		}
	}

	public void CopyTo(float[] destination, int destinationOffset, int count)
	{
		if (count > 0)
		{
			Array.Copy(m_buffer, m_readIndex, destination, destinationOffset, count);
		}
	}

	public float Peek(int index)
	{
		return m_buffer[m_readIndex + index];
	}

	public void Advance(int count)
	{
		if (count <= 0)
		{
			return;
		}
		if (count >= m_count)
		{
			m_readIndex = 0;
			m_count = 0;
			return;
		}
		m_readIndex += count;
		m_count -= count;
		if (m_count == 0)
		{
			m_readIndex = 0;
		}
		else if (m_readIndex >= m_buffer.Length / 2)
		{
			Array.Copy(m_buffer, m_readIndex, m_buffer, 0, m_count);
			m_readIndex = 0;
		}
	}

	private void ensureCapacity(int additionalCount)
	{
		int num = m_count + additionalCount;
		if (m_readIndex + num <= m_buffer.Length)
		{
			return;
		}
		if (num <= m_buffer.Length)
		{
			Array.Copy(m_buffer, m_readIndex, m_buffer, 0, m_count);
			m_readIndex = 0;
			return;
		}
		int num2;
		for (num2 = m_buffer.Length; num2 < num; num2 *= 2)
		{
		}
		float[] array = new float[num2];
		if (m_count > 0)
		{
			Array.Copy(m_buffer, m_readIndex, array, 0, m_count);
		}
		m_buffer = array;
		m_readIndex = 0;
	}
}
