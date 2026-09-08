using Microsoft.Cci;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CodeGen;

internal abstract class SynthesizedStaticField : SynthesizedStaticFieldBase
{
	private readonly ITypeReference _type;

	internal ITypeReference Type => _type;

	internal SynthesizedStaticField(string name, INamedTypeDefinition containingType, ITypeReference type)
		: base(name, containingType)
	{
		_type = type;
	}

	public override string ToString()
	{
		return $"{((object)_type.GetInternalSymbol()) ?? ((object)_type)} {((object)base.ContainingTypeDefinition.GetInternalSymbol()) ?? ((object)base.ContainingTypeDefinition)}.{base.Name}";
	}

	public override ITypeReference GetType(EmitContext context)
	{
		return _type;
	}
}
