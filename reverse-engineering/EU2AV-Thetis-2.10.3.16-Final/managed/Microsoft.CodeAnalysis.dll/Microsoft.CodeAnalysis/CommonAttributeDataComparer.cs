using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class CommonAttributeDataComparer : IEqualityComparer<AttributeData>
{
	private class TypedConstantComparer : IEqualityComparer<TypedConstant>
	{
		public static readonly TypedConstantComparer IgnoreAll = new TypedConstantComparer();

		private TypedConstantComparer()
		{
		}

		public bool Equals(TypedConstant x, TypedConstant y)
		{
			return equals(x, y);
			static bool equals(TypedConstant typedConstant, TypedConstant other)
			{
				if (typedConstant.Kind == TypedConstantKind.Type && other.Kind == TypedConstantKind.Type)
				{
					if (typedConstant.ValueInternal is ISymbolInternal symbolInternal && other.ValueInternal is ISymbolInternal other2)
					{
						return symbolInternal.Equals(other2, TypeCompareKind.AllIgnoreOptions);
					}
					return false;
				}
				return typedConstant.Equals(other);
			}
		}

		public int GetHashCode(TypedConstant obj)
		{
			return obj.GetHashCode();
		}
	}

	private class NamedArgumentComparer : IEqualityComparer<KeyValuePair<string, TypedConstant>>
	{
		public static readonly NamedArgumentComparer IgnoreAll = new NamedArgumentComparer();

		private NamedArgumentComparer()
		{
		}

		public bool Equals(KeyValuePair<string, TypedConstant> pair1, KeyValuePair<string, TypedConstant> pair2)
		{
			if (pair1.Key == pair2.Key)
			{
				return TypedConstantComparer.IgnoreAll.Equals(pair1.Value, pair2.Value);
			}
			return false;
		}

		public int GetHashCode(KeyValuePair<string, TypedConstant> pair)
		{
			return pair.GetHashCode();
		}
	}

	public static CommonAttributeDataComparer Instance = new CommonAttributeDataComparer(considerNamedArgumentsOrder: true);

	public static CommonAttributeDataComparer InstanceIgnoringNamedArgumentOrder = new CommonAttributeDataComparer(considerNamedArgumentsOrder: false);

	private readonly bool _considerNamedArgumentsOrder;

	private CommonAttributeDataComparer(bool considerNamedArgumentsOrder)
	{
		_considerNamedArgumentsOrder = considerNamedArgumentsOrder;
	}

	public bool Equals(AttributeData attr1, AttributeData attr2)
	{
		TypedConstantComparer ignoreAll = TypedConstantComparer.IgnoreAll;
		NamedArgumentComparer ignoreAll2 = NamedArgumentComparer.IgnoreAll;
		if (attr1.AttributeClass == attr2.AttributeClass && attr1.AttributeConstructor == attr2.AttributeConstructor && attr1.HasErrors == attr2.HasErrors && attr1.IsConditionallyOmitted == attr2.IsConditionallyOmitted && attr1.CommonConstructorArguments.SequenceEqual(attr2.CommonConstructorArguments, ignoreAll))
		{
			if (!_considerNamedArgumentsOrder)
			{
				return attr1.NamedArguments.SetEquals<KeyValuePair<string, TypedConstant>>(attr2.NamedArguments, ignoreAll2);
			}
			return attr1.NamedArguments.SequenceEqual<KeyValuePair<string, TypedConstant>, KeyValuePair<string, TypedConstant>>(attr2.NamedArguments, ignoreAll2);
		}
		return false;
	}

	public int GetHashCode(AttributeData attr)
	{
		int num = attr.AttributeClass?.GetHashCode() ?? 0;
		num = ((attr.AttributeConstructor != null) ? Hash.Combine(attr.AttributeConstructor.GetHashCode(), num) : num);
		num = Hash.Combine(attr.HasErrors, num);
		num = Hash.Combine(attr.IsConditionallyOmitted, num);
		num = Hash.Combine(GetHashCodeForConstructorArguments(attr.CommonConstructorArguments), num);
		return Hash.Combine(GetHashCodeForNamedArguments(attr.NamedArguments), num);
	}

	private static int GetHashCodeForConstructorArguments(ImmutableArray<TypedConstant> constructorArguments)
	{
		int num = 0;
		foreach (TypedConstant item in constructorArguments)
		{
			num = Hash.Combine(item.GetHashCode(), num);
		}
		return num;
	}

	private int GetHashCodeForNamedArguments(ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments)
	{
		int num = 0;
		foreach (KeyValuePair<string, TypedConstant> item in namedArguments)
		{
			if (item.Key != null)
			{
				num = hashCombine(item.Key.GetHashCode(), num, _considerNamedArgumentsOrder);
			}
			num = hashCombine(item.Value.GetHashCode(), num, _considerNamedArgumentsOrder);
		}
		return num;
		static int hashCombine(int value, int currentHash, bool considerNamedArgumentsOrder)
		{
			if (!considerNamedArgumentsOrder)
			{
				return value ^ currentHash;
			}
			return Hash.Combine(value, currentHash);
		}
	}
}
