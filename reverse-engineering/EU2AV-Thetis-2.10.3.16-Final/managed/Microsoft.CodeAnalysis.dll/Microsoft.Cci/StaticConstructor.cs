using System.Collections.Immutable;

namespace Microsoft.Cci;

internal sealed class StaticConstructor : MethodDefinitionBase
{
	public override string Name => ".cctor";

	public override TypeMemberVisibility Visibility => TypeMemberVisibility.Private;

	public override bool IsRuntimeSpecial => true;

	public override bool IsSpecialName => true;

	public StaticConstructor(ITypeDefinition containingTypeDefinition, ushort maxStack, ImmutableArray<byte> il)
		: base(containingTypeDefinition, maxStack, il)
	{
	}
}
