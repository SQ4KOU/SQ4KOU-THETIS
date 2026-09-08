using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.Emit;

public readonly struct EditAndContinueMethodDebugInformation
{
	internal readonly int MethodOrdinal;

	internal readonly ImmutableArray<LocalSlotDebugInfo> LocalSlots;

	internal readonly ImmutableArray<LambdaDebugInfo> Lambdas;

	internal readonly ImmutableArray<ClosureDebugInfo> Closures;

	internal readonly ImmutableArray<StateMachineStateDebugInfo> StateMachineStates;

	private const byte SyntaxOffsetBaseline = byte.MaxValue;

	internal EditAndContinueMethodDebugInformation(int methodOrdinal, ImmutableArray<LocalSlotDebugInfo> localSlots, ImmutableArray<ClosureDebugInfo> closures, ImmutableArray<LambdaDebugInfo> lambdas, ImmutableArray<StateMachineStateDebugInfo> stateMachineStates)
	{
		MethodOrdinal = methodOrdinal;
		LocalSlots = localSlots;
		Lambdas = lambdas;
		Closures = closures;
		StateMachineStates = stateMachineStates;
	}

	public static EditAndContinueMethodDebugInformation Create(ImmutableArray<byte> compressedSlotMap, ImmutableArray<byte> compressedLambdaMap)
	{
		return Create(compressedSlotMap, compressedLambdaMap, default(ImmutableArray<byte>));
	}

	public static EditAndContinueMethodDebugInformation Create(ImmutableArray<byte> compressedSlotMap, ImmutableArray<byte> compressedLambdaMap, ImmutableArray<byte> compressedStateMachineStateMap)
	{
		UncompressLambdaMap(compressedLambdaMap, out var methodOrdinal, out var closures, out var lambdas);
		return new EditAndContinueMethodDebugInformation(methodOrdinal, UncompressSlotMap(compressedSlotMap), closures, lambdas, UncompressStateMachineStates(compressedStateMachineStateMap));
	}

	private static InvalidDataException CreateInvalidDataException(ImmutableArray<byte> data, int offset)
	{
		int num = Math.Max(0, offset - 512);
		int num2 = Math.Min(data.Length, offset + 512);
		byte[] array = new byte[offset - num];
		data.CopyTo(num, array, 0, array.Length);
		byte[] array2 = new byte[num2 - offset];
		data.CopyTo(offset, array2, 0, array2.Length);
		throw new InvalidDataException(string.Format(CodeAnalysisResources.InvalidDataAtOffset, offset, (num != 0) ? "..." : "", BitConverter.ToString(array), BitConverter.ToString(array2), (num2 != data.Length) ? "..." : ""));
	}

	private unsafe static ImmutableArray<LocalSlotDebugInfo> UncompressSlotMap(ImmutableArray<byte> compressedSlotMap)
	{
		if (compressedSlotMap.IsDefaultOrEmpty)
		{
			return default(ImmutableArray<LocalSlotDebugInfo>);
		}
		ArrayBuilder<LocalSlotDebugInfo> instance = ArrayBuilder<LocalSlotDebugInfo>.GetInstance();
		int num = -1;
		fixed (byte* buffer = &compressedSlotMap.ToArray()[0])
		{
			BlobReader blobReader = new BlobReader(buffer, compressedSlotMap.Length);
			while (blobReader.RemainingBytes > 0)
			{
				try
				{
					byte b = blobReader.ReadByte();
					switch (b)
					{
					case byte.MaxValue:
						num = -blobReader.ReadCompressedInteger();
						continue;
					case 0:
						instance.Add(new LocalSlotDebugInfo(SynthesizedLocalKind.LoweringTemp, default(LocalDebugId)));
						continue;
					}
					SynthesizedLocalKind synthesizedKind = (SynthesizedLocalKind)((b & 0x3F) - 1);
					bool num2 = (b & 0x80) != 0;
					int syntaxOffset = blobReader.ReadCompressedInteger() + num;
					int ordinal = (num2 ? blobReader.ReadCompressedInteger() : 0);
					instance.Add(new LocalSlotDebugInfo(synthesizedKind, new LocalDebugId(syntaxOffset, ordinal)));
				}
				catch (BadImageFormatException)
				{
					throw CreateInvalidDataException(compressedSlotMap, blobReader.Offset);
				}
			}
		}
		return instance.ToImmutableAndFree();
	}

	internal void SerializeLocalSlots(BlobBuilder writer)
	{
		int num = -1;
		foreach (LocalSlotDebugInfo localSlot in LocalSlots)
		{
			if (localSlot.Id.SyntaxOffset < num)
			{
				num = localSlot.Id.SyntaxOffset;
			}
		}
		if (num != -1)
		{
			writer.WriteByte(byte.MaxValue);
			writer.WriteCompressedInteger(-num);
		}
		foreach (LocalSlotDebugInfo localSlot2 in LocalSlots)
		{
			SynthesizedLocalKind synthesizedKind = localSlot2.SynthesizedKind;
			if (!synthesizedKind.IsLongLived())
			{
				writer.WriteByte(0);
				continue;
			}
			byte b = (byte)(synthesizedKind + 1);
			bool num2 = localSlot2.Id.Ordinal > 0;
			if (num2)
			{
				b |= 0x80;
			}
			writer.WriteByte(b);
			writer.WriteCompressedInteger(localSlot2.Id.SyntaxOffset - num);
			if (num2)
			{
				writer.WriteCompressedInteger(localSlot2.Id.Ordinal);
			}
		}
	}

	private unsafe static void UncompressLambdaMap(ImmutableArray<byte> compressedLambdaMap, out int methodOrdinal, out ImmutableArray<ClosureDebugInfo> closures, out ImmutableArray<LambdaDebugInfo> lambdas)
	{
		methodOrdinal = -1;
		closures = default(ImmutableArray<ClosureDebugInfo>);
		lambdas = default(ImmutableArray<LambdaDebugInfo>);
		if (compressedLambdaMap.IsDefaultOrEmpty)
		{
			return;
		}
		ArrayBuilder<ClosureDebugInfo> instance = ArrayBuilder<ClosureDebugInfo>.GetInstance();
		ArrayBuilder<LambdaDebugInfo> instance2 = ArrayBuilder<LambdaDebugInfo>.GetInstance();
		fixed (byte* buffer = &compressedLambdaMap.ToArray()[0])
		{
			BlobReader blobReader = new BlobReader(buffer, compressedLambdaMap.Length);
			try
			{
				methodOrdinal = blobReader.ReadCompressedInteger() - 1;
				int num = -blobReader.ReadCompressedInteger();
				int num2 = blobReader.ReadCompressedInteger();
				for (int i = 0; i < num2; i++)
				{
					int num3 = blobReader.ReadCompressedInteger();
					DebugId closureId = new DebugId(instance.Count, 0);
					instance.Add(new ClosureDebugInfo(num3 + num, closureId));
				}
				while (blobReader.RemainingBytes > 0)
				{
					int num4 = blobReader.ReadCompressedInteger();
					int num5 = blobReader.ReadCompressedInteger() + -2;
					if (num5 >= num2)
					{
						throw CreateInvalidDataException(compressedLambdaMap, blobReader.Offset);
					}
					DebugId lambdaId = new DebugId(instance2.Count, 0);
					instance2.Add(new LambdaDebugInfo(num4 + num, lambdaId, num5));
				}
			}
			catch (BadImageFormatException)
			{
				throw CreateInvalidDataException(compressedLambdaMap, blobReader.Offset);
			}
		}
		closures = instance.ToImmutableAndFree();
		lambdas = instance2.ToImmutableAndFree();
	}

	internal void SerializeLambdaMap(BlobBuilder writer)
	{
		writer.WriteCompressedInteger(MethodOrdinal + 1);
		int num = -1;
		foreach (ClosureDebugInfo closure in Closures)
		{
			if (closure.SyntaxOffset < num)
			{
				num = closure.SyntaxOffset;
			}
		}
		foreach (LambdaDebugInfo lambda in Lambdas)
		{
			if (lambda.SyntaxOffset < num)
			{
				num = lambda.SyntaxOffset;
			}
		}
		writer.WriteCompressedInteger(-num);
		writer.WriteCompressedInteger(Closures.Length);
		foreach (ClosureDebugInfo closure2 in Closures)
		{
			writer.WriteCompressedInteger(closure2.SyntaxOffset - num);
		}
		foreach (LambdaDebugInfo lambda2 in Lambdas)
		{
			writer.WriteCompressedInteger(lambda2.SyntaxOffset - num);
			writer.WriteCompressedInteger(lambda2.ClosureOrdinal - -2);
		}
	}

	private unsafe static ImmutableArray<StateMachineStateDebugInfo> UncompressStateMachineStates(ImmutableArray<byte> compressedStateMachineStates)
	{
		if (compressedStateMachineStates.IsDefaultOrEmpty)
		{
			return default(ImmutableArray<StateMachineStateDebugInfo>);
		}
		ArrayBuilder<StateMachineStateDebugInfo> instance = ArrayBuilder<StateMachineStateDebugInfo>.GetInstance();
		fixed (byte* buffer = &compressedStateMachineStates.ToArray()[0])
		{
			BlobReader blobReader = new BlobReader(buffer, compressedStateMachineStates.Length);
			try
			{
				int num = blobReader.ReadCompressedInteger();
				if (num > 0)
				{
					int num2 = -blobReader.ReadCompressedInteger();
					int num3 = int.MinValue;
					int num4 = 0;
					while (num > 0)
					{
						int stateNumber = blobReader.ReadCompressedSignedInteger();
						int num5 = num2 + blobReader.ReadCompressedInteger();
						if (num5 < num3)
						{
							throw CreateInvalidDataException(compressedStateMachineStates, blobReader.Offset);
						}
						num4 = ((num5 == num3) ? (num4 + 1) : 0);
						if (num4 > 255)
						{
							throw CreateInvalidDataException(compressedStateMachineStates, blobReader.Offset);
						}
						instance.Add(new StateMachineStateDebugInfo(num5, new AwaitDebugId((byte)num4), (StateMachineState)stateNumber));
						num--;
						num3 = num5;
					}
				}
			}
			catch (BadImageFormatException)
			{
				throw CreateInvalidDataException(compressedStateMachineStates, blobReader.Offset);
			}
		}
		return instance.ToImmutableAndFree();
	}

	internal void SerializeStateMachineStates(BlobBuilder writer)
	{
		writer.WriteCompressedInteger(StateMachineStates.Length);
		if (StateMachineStates.Length <= 0)
		{
			return;
		}
		int num = Math.Min(StateMachineStates.Min((StateMachineStateDebugInfo state) => state.SyntaxOffset), 0);
		writer.WriteCompressedInteger(-num);
		foreach (StateMachineStateDebugInfo item in from s in StateMachineStates
			orderby s.SyntaxOffset, s.AwaitId.RelativeStateOrdinal
			select s)
		{
			writer.WriteCompressedSignedInteger((int)item.StateNumber);
			writer.WriteCompressedInteger(item.SyntaxOffset - num);
		}
	}
}
