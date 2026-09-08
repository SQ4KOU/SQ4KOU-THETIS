using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract class AbstractTypeParameterMap : AbstractTypeMap
{
	protected readonly SmallDictionary<TypeParameterSymbol, TypeWithAnnotations> Mapping;

	protected AbstractTypeParameterMap(SmallDictionary<TypeParameterSymbol, TypeWithAnnotations> mapping)
	{
		Mapping = mapping;
	}

	protected sealed override TypeWithAnnotations SubstituteTypeParameter(TypeParameterSymbol typeParameter)
	{
		if (Mapping.TryGetValue(typeParameter, out var value))
		{
			return value;
		}
		return TypeWithAnnotations.Create(typeParameter);
	}

	private string GetDebuggerDisplay()
	{
		StringBuilder stringBuilder = new StringBuilder("[");
		stringBuilder.Append(GetType().Name);
		foreach (KeyValuePair<TypeParameterSymbol, TypeWithAnnotations> item in Mapping)
		{
			stringBuilder.Append(' ').Append(item.Key).Append(':')
				.Append(item.Value.Type);
		}
		return stringBuilder.Append(']').ToString();
	}
}
