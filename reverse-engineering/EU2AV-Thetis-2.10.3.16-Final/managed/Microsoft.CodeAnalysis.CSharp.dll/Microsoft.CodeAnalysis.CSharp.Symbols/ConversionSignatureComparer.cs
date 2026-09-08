using System.Collections.Generic;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class ConversionSignatureComparer : IEqualityComparer<MethodSymbol>
{
	private static readonly ConversionSignatureComparer s_comparer = new ConversionSignatureComparer();

	public static ConversionSignatureComparer Comparer => s_comparer;

	private ConversionSignatureComparer()
	{
	}

	public bool Equals(MethodSymbol member1, MethodSymbol member2)
	{
		if ((object)member1 == member2)
		{
			return true;
		}
		if ((object)member1 == null || (object)member2 == null)
		{
			return false;
		}
		if (member1.ParameterCount != 1 || member2.ParameterCount != 1)
		{
			return false;
		}
		if (member1.ReturnType.Equals(member2.ReturnType, TypeCompareKind.IgnoreDynamicAndTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes) && member1.ParameterTypesWithAnnotations[0].Equals(member2.ParameterTypesWithAnnotations[0], TypeCompareKind.IgnoreDynamicAndTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes))
		{
			if (!(member1.Name == "op_Implicit") && !(member2.Name == "op_Implicit"))
			{
				return member1.Name == member2.Name;
			}
			return true;
		}
		return false;
	}

	public int GetHashCode(MethodSymbol member)
	{
		if ((object)member == null)
		{
			return 0;
		}
		int currentKey = 1;
		currentKey = Hash.Combine(member.ReturnType.GetHashCode(), currentKey);
		if (member.ParameterCount != 1)
		{
			return currentKey;
		}
		return Hash.Combine(member.GetParameterType(0).GetHashCode(), currentKey);
	}
}
