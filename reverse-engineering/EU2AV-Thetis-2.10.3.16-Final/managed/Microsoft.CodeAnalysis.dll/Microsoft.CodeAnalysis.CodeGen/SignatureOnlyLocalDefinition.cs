using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class SignatureOnlyLocalDefinition : ILocalDefinition, INamedEntity
{
	private readonly byte[] _signature;

	private readonly int _slot;

	public MetadataConstant CompileTimeValue
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/CodeGen/SignatureOnlyLocalDefinition.cs", 31);
		}
	}

	public ImmutableArray<ICustomModifier> CustomModifiers
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/CodeGen/SignatureOnlyLocalDefinition.cs", 36);
		}
	}

	public ImmutableArray<bool> DynamicTransformFlags => ImmutableArray<bool>.Empty;

	public ImmutableArray<string> TupleElementNames => ImmutableArray<string>.Empty;

	public LocalVariableAttributes PdbAttributes => LocalVariableAttributes.DebuggerHidden;

	public bool IsPinned
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/CodeGen/SignatureOnlyLocalDefinition.cs", 57);
		}
	}

	public bool IsReference
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/CodeGen/SignatureOnlyLocalDefinition.cs", 62);
		}
	}

	public LocalSlotConstraints Constraints
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/CodeGen/SignatureOnlyLocalDefinition.cs", 67);
		}
	}

	public Location Location => Microsoft.CodeAnalysis.Location.None;

	public string? Name => null;

	public int SlotIndex => _slot;

	public ITypeReference Type
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/CodeGen/SignatureOnlyLocalDefinition.cs", 78);
		}
	}

	public byte[] Signature => _signature;

	public LocalSlotDebugInfo SlotInfo => new LocalSlotDebugInfo(SynthesizedLocalKind.EmitterTemp, LocalDebugId.None);

	internal SignatureOnlyLocalDefinition(byte[] signature, int slot)
	{
		_signature = signature;
		_slot = slot;
	}
}
