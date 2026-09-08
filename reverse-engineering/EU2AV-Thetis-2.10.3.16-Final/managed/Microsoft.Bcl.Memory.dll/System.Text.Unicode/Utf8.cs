using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Text.Unicode;

public static class Utf8
{
	public unsafe static OperationStatus FromUtf16(ReadOnlySpan<char> source, Span<byte> destination, out int charsRead, out int bytesWritten, bool replaceInvalidSequences = true, bool isFinalBlock = true)
	{
		fixed (char* reference = &MemoryMarshal.GetReference(source))
		{
			fixed (byte* reference2 = &MemoryMarshal.GetReference(destination))
			{
				OperationStatus operationStatus = OperationStatus.Done;
				char* pInputBufferRemaining = reference;
				byte* pOutputBufferRemaining = reference2;
				while (!source.IsEmpty)
				{
					operationStatus = System.Text.Unicode.Utf8Utility.TranscodeToUtf8((char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(source)), source.Length, (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(destination)), destination.Length, out pInputBufferRemaining, out pOutputBufferRemaining);
					if (operationStatus <= OperationStatus.DestinationTooSmall || (operationStatus == OperationStatus.NeedMoreData && !isFinalBlock))
					{
						break;
					}
					if (!replaceInvalidSequences)
					{
						operationStatus = OperationStatus.InvalidData;
						break;
					}
					destination = destination.Slice((int)(pOutputBufferRemaining - (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(destination))));
					if (destination.Length <= 2)
					{
						operationStatus = OperationStatus.DestinationTooSmall;
						break;
					}
					destination[0] = 239;
					destination[1] = 191;
					destination[2] = 189;
					destination = destination.Slice(3);
					source = source.Slice((int)(pInputBufferRemaining - (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(source))) + 1);
					operationStatus = OperationStatus.Done;
					pInputBufferRemaining = (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(source));
					pOutputBufferRemaining = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(destination));
				}
				charsRead = (int)(pInputBufferRemaining - reference);
				bytesWritten = (int)(pOutputBufferRemaining - reference2);
				return operationStatus;
			}
		}
	}

	public unsafe static OperationStatus ToUtf16(ReadOnlySpan<byte> source, Span<char> destination, out int bytesRead, out int charsWritten, bool replaceInvalidSequences = true, bool isFinalBlock = true)
	{
		fixed (byte* reference = &MemoryMarshal.GetReference(source))
		{
			fixed (char* reference2 = &MemoryMarshal.GetReference(destination))
			{
				OperationStatus operationStatus = OperationStatus.Done;
				byte* pInputBufferRemaining = reference;
				char* pOutputBufferRemaining = reference2;
				while (!source.IsEmpty)
				{
					operationStatus = System.Text.Unicode.Utf8Utility.TranscodeToUtf16((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(source)), source.Length, (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(destination)), destination.Length, out pInputBufferRemaining, out pOutputBufferRemaining);
					if (operationStatus <= OperationStatus.DestinationTooSmall || (operationStatus == OperationStatus.NeedMoreData && !isFinalBlock))
					{
						break;
					}
					if (!replaceInvalidSequences)
					{
						operationStatus = OperationStatus.InvalidData;
						break;
					}
					destination = destination.Slice((int)(pOutputBufferRemaining - (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(destination))));
					if (destination.IsEmpty)
					{
						operationStatus = OperationStatus.DestinationTooSmall;
						break;
					}
					destination[0] = '\ufffd';
					destination = destination.Slice(1);
					source = source.Slice((int)(pInputBufferRemaining - (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(source))));
					Rune.DecodeFromUtf8(source, out var _, out var bytesConsumed);
					source = source.Slice(bytesConsumed);
					operationStatus = OperationStatus.Done;
					pInputBufferRemaining = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(source));
					pOutputBufferRemaining = (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(destination));
				}
				bytesRead = (int)(pInputBufferRemaining - reference);
				charsWritten = (int)(pOutputBufferRemaining - reference2);
				return operationStatus;
			}
		}
	}

	public static bool IsValid(ReadOnlySpan<byte> value)
	{
		bool isAscii;
		return System.Text.Unicode.Utf8Utility.GetIndexOfFirstInvalidUtf8Sequence(value, out isAscii) < 0;
	}
}
