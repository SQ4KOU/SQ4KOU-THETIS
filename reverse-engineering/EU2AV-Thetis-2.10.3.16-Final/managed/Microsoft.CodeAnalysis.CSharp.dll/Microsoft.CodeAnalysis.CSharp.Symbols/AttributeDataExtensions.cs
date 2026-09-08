using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class AttributeDataExtensions
{
	internal static int IndexOfAttribute(this ImmutableArray<CSharpAttributeData> attributes, AttributeDescription description)
	{
		for (int i = 0; i < attributes.Length; i++)
		{
			if (attributes[i].IsTargetAttribute(description))
			{
				return i;
			}
		}
		return -1;
	}

	internal static string? DecodeNotNullIfNotNullAttribute(this CSharpAttributeData attribute)
	{
		ImmutableArray<TypedConstant> commonConstructorArguments = attribute.CommonConstructorArguments;
		if (commonConstructorArguments.Length != 1 || !commonConstructorArguments[0].TryDecodeValue<string>(SpecialType.System_String, out var value))
		{
			return null;
		}
		return value;
	}
}
