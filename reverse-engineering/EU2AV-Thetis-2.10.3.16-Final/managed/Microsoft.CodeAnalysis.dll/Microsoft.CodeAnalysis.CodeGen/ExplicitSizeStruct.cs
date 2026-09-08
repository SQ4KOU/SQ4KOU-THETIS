using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class ExplicitSizeStruct : NestedTypeDefinition
{
	private readonly uint _size;

	private readonly ushort _alignment;

	private readonly INamedTypeDefinition _containingType;

	private readonly ITypeReference _sysValueType;

	public override ushort Alignment => _alignment;

	public override LayoutKind Layout => LayoutKind.Explicit;

	public override uint SizeOf => _size;

	public override string Name
	{
		get
		{
			if (_alignment != 1)
			{
				return $"__StaticArrayInitTypeSize={_size}_Align={_alignment}";
			}
			return $"__StaticArrayInitTypeSize={_size}";
		}
	}

	public override ITypeDefinition ContainingTypeDefinition => _containingType;

	public override TypeMemberVisibility Visibility => TypeMemberVisibility.Assembly;

	public override bool IsValueType => true;

	internal ExplicitSizeStruct(uint size, ushort alignment, PrivateImplementationDetails containingType, ITypeReference sysValueType)
	{
		_size = size;
		_alignment = alignment;
		_containingType = containingType;
		_sysValueType = sysValueType;
	}

	public override ITypeReference GetBaseClass(EmitContext context)
	{
		return _sysValueType;
	}
}
