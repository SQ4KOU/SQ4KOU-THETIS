using System;
using System.Diagnostics;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Emit;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal readonly struct EncHoistedLocalInfo : IEquatable<EncHoistedLocalInfo>
{
	public readonly LocalSlotDebugInfo SlotInfo;

	public readonly ITypeReference? Type;

	public bool IsUnused => Type == null;

	public EncHoistedLocalInfo(bool _)
	{
		SlotInfo = new LocalSlotDebugInfo(SynthesizedLocalKind.EmitterTemp, LocalDebugId.None);
		Type = null;
	}

	public EncHoistedLocalInfo(LocalSlotDebugInfo slotInfo, ITypeReference type)
	{
		SlotInfo = slotInfo;
		Type = type;
	}

	public bool Equals(EncHoistedLocalInfo other)
	{
		if (SlotInfo.Equals(other.SlotInfo))
		{
			return SymbolEquivalentEqualityComparer.Instance.Equals(Type, other.Type);
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		if (obj is EncHoistedLocalInfo other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(SymbolEquivalentEqualityComparer.Instance.GetHashCode(Type), SlotInfo.GetHashCode());
	}

	private string GetDebuggerDisplay()
	{
		if (IsUnused)
		{
			return "[invalid]";
		}
		return $"[Id={SlotInfo.Id}, SynthesizedKind={SlotInfo.SynthesizedKind}, Type={Type}]";
	}
}
