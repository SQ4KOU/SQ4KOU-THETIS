using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal sealed class ReturnValueParameter : ParameterDefinitionBase
{
	private readonly IMethodDefinition _containingMethod;

	public override ImmutableArray<ICustomModifier> RefCustomModifiers => _containingMethod.RefCustomModifiers;

	public override ImmutableArray<ICustomModifier> CustomModifiers => _containingMethod.ReturnValueCustomModifiers;

	public override ushort Index => 0;

	public override bool IsByReference => _containingMethod.ReturnValueIsByRef;

	public override bool IsMarshalledExplicitly => _containingMethod.ReturnValueIsMarshalledExplicitly;

	public override IMarshallingInformation MarshallingInformation => _containingMethod.ReturnValueMarshallingInformation;

	public override ImmutableArray<byte> MarshallingDescriptor => _containingMethod.ReturnValueMarshallingDescriptor;

	public override string Name => string.Empty;

	internal ReturnValueParameter(IMethodDefinition containingMethod)
	{
		_containingMethod = containingMethod;
	}

	public override IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
	{
		return _containingMethod.GetReturnValueAttributes(context);
	}

	public override ITypeReference GetType(EmitContext context)
	{
		return _containingMethod.GetType(context);
	}
}
