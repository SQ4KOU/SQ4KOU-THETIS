using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace System.Reflection.Metadata;

[DebuggerDisplay("{AssemblyQualifiedName}")]
public sealed class TypeName
{
	private readonly int _rankOrModifier;

	private readonly int _nestedNameLength;

	private readonly TypeName _elementOrGenericType;

	private readonly TypeName _declaringType;

	private readonly ImmutableArray<TypeName> _genericArguments;

	private string _name;

	private string _namespace;

	private string _fullName;

	private string _assemblyQualifiedName;

	public string AssemblyQualifiedName
	{
		get
		{
			if (_assemblyQualifiedName == null)
			{
				if (_fullName != null && AssemblyName == null)
				{
					_assemblyQualifiedName = FullName;
				}
				else
				{
					Span<char> initialBuffer = stackalloc char[256];
					ValueStringBuilder builder = new ValueStringBuilder(initialBuffer);
					AppendFullName(ref builder);
					if (AssemblyName != null)
					{
						builder.Append(", ");
						AssemblyName.AppendFullName(ref builder);
					}
					_assemblyQualifiedName = builder.ToString();
					if (AssemblyName == null)
					{
						_fullName = _assemblyQualifiedName;
					}
				}
			}
			return _assemblyQualifiedName;
		}
	}

	public AssemblyNameInfo? AssemblyName { get; }

	public TypeName DeclaringType
	{
		get
		{
			if (_declaringType == null)
			{
				TypeNameParserHelpers.ThrowInvalidOperation_NotNestedType();
			}
			return _declaringType;
		}
	}

	public string FullName
	{
		get
		{
			if (_fullName == null)
			{
				Span<char> initialBuffer = stackalloc char[128];
				ValueStringBuilder builder = new ValueStringBuilder(initialBuffer);
				AppendFullName(ref builder);
				_fullName = builder.ToString();
			}
			else if (_nestedNameLength > 0 && _fullName.Length > _nestedNameLength)
			{
				_fullName = _fullName.Substring(0, _nestedNameLength);
			}
			return _fullName;
		}
	}

	public bool IsArray
	{
		get
		{
			if (_rankOrModifier != -1)
			{
				return _rankOrModifier > 0;
			}
			return true;
		}
	}

	public bool IsConstructedGenericType => _genericArguments.Length > 0;

	[System.Diagnostics.CodeAnalysis.MemberNotNullWhen(false, "_elementOrGenericType")]
	public bool IsSimple
	{
		[System.Diagnostics.CodeAnalysis.MemberNotNullWhen(false, "_elementOrGenericType")]
		get
		{
			return _elementOrGenericType == null;
		}
	}

	public bool IsByRef => _rankOrModifier == -3;

	[System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, "_declaringType")]
	public bool IsNested
	{
		[System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, "_declaringType")]
		get
		{
			return _declaringType != null;
		}
	}

	public bool IsSZArray => _rankOrModifier == -1;

	public bool IsPointer => _rankOrModifier == -2;

	public bool IsVariableBoundArrayType => _rankOrModifier >= 1;

	public string Name
	{
		get
		{
			if (_name == null)
			{
				Span<char> initialBuffer = stackalloc char[64];
				ValueStringBuilder builder = new ValueStringBuilder(initialBuffer);
				AppendName(ref builder);
				_name = builder.ToString();
			}
			return _name;
		}
	}

	public string Namespace
	{
		get
		{
			if (_namespace == null)
			{
				TypeName typeName = this;
				while (!typeName.IsSimple)
				{
					typeName = typeName._elementOrGenericType;
				}
				if (typeName.IsNested)
				{
					TypeNameParserHelpers.ThrowInvalidOperation_NestedTypeNamespace();
				}
				if (typeName._namespace == null)
				{
					ReadOnlySpan<char> fullName = typeName._fullName.AsSpan();
					if (typeName._nestedNameLength > 0)
					{
						fullName = fullName.Slice(0, typeName._nestedNameLength);
					}
					int num = TypeNameParserHelpers.IndexOfNamespaceDelimiter(fullName);
					if (num >= 0)
					{
						typeName._namespace = fullName.Slice(0, num).ToString();
					}
					else
					{
						typeName._namespace = string.Empty;
					}
				}
				_namespace = typeName._namespace;
			}
			return _namespace;
		}
	}

	internal TypeName(string? fullName, AssemblyNameInfo? assemblyName, TypeName? elementOrGenericType = null, TypeName? declaringType = null, ImmutableArray<TypeName>.Builder? genericTypeArguments = null, int rankOrModifier = 0, int nestedNameLength = -1)
	{
		_fullName = fullName;
		AssemblyName = assemblyName;
		_rankOrModifier = rankOrModifier;
		_elementOrGenericType = elementOrGenericType;
		_declaringType = declaringType;
		_nestedNameLength = nestedNameLength;
		_genericArguments = ((genericTypeArguments == null) ? ImmutableArray<TypeName>.Empty : ((genericTypeArguments.Count == genericTypeArguments.Capacity) ? genericTypeArguments.MoveToImmutable() : genericTypeArguments.ToImmutableArray()));
	}

	private TypeName(string fullName, AssemblyNameInfo assemblyName, TypeName elementOrGenericType, TypeName declaringType, ImmutableArray<TypeName> genericTypeArguments, int rankOrModifier = 0, int nestedNameLength = -1)
	{
		_fullName = fullName;
		AssemblyName = assemblyName;
		_elementOrGenericType = elementOrGenericType;
		_declaringType = declaringType;
		_genericArguments = genericTypeArguments;
		_rankOrModifier = rankOrModifier;
		_nestedNameLength = nestedNameLength;
	}

	private void AppendFullName(ref ValueStringBuilder builder)
	{
		if (_fullName == null)
		{
			if (IsConstructedGenericType)
			{
				GetGenericTypeDefinition().AppendFullName(ref builder);
				builder.Append('[');
				foreach (TypeName genericArgument in GetGenericArguments())
				{
					builder.Append('[');
					genericArgument.AppendFullName(ref builder);
					if (genericArgument.AssemblyName != null)
					{
						builder.Append(", ");
						genericArgument.AssemblyName.AppendFullName(ref builder);
					}
					builder.Append("],");
				}
				builder[builder.Length - 1] = ']';
			}
			else if (IsArray || IsPointer || IsByRef)
			{
				GetElementType().AppendFullName(ref builder);
				TypeNameParserHelpers.AppendRankOrModifierStringRepresentation(_rankOrModifier, ref builder);
			}
		}
		else if (_nestedNameLength > 0 && _fullName.Length > _nestedNameLength)
		{
			builder.Append(_fullName.AsSpan(0, _nestedNameLength));
		}
		else
		{
			builder.Append(_fullName);
		}
	}

	private void AppendName(ref ValueStringBuilder builder)
	{
		if (IsConstructedGenericType)
		{
			GetGenericTypeDefinition().AppendName(ref builder);
			return;
		}
		if (IsPointer || IsByRef || IsArray)
		{
			GetElementType().AppendName(ref builder);
			TypeNameParserHelpers.AppendRankOrModifierStringRepresentation(_rankOrModifier, ref builder);
			return;
		}
		ReadOnlySpan<char> readOnlySpan = _fullName.AsSpan();
		if (_nestedNameLength > 0)
		{
			readOnlySpan = readOnlySpan.Slice(0, _nestedNameLength);
		}
		if (IsNested)
		{
			readOnlySpan = readOnlySpan.Slice(_declaringType._nestedNameLength + 1);
		}
		else
		{
			int num = TypeNameParserHelpers.IndexOfNamespaceDelimiter(readOnlySpan);
			if (num >= 0)
			{
				readOnlySpan = readOnlySpan.Slice(num + 1);
			}
		}
		builder.Append(readOnlySpan);
	}

	public int GetNodeCount()
	{
		int num = 1;
		checked
		{
			if (IsArray || IsPointer || IsByRef)
			{
				num += GetElementType().GetNodeCount();
			}
			else if (IsConstructedGenericType)
			{
				num += GetGenericTypeDefinition().GetNodeCount();
				foreach (TypeName genericArgument in GetGenericArguments())
				{
					num += genericArgument.GetNodeCount();
				}
			}
			else if (IsNested)
			{
				num += DeclaringType.GetNodeCount();
			}
			return num;
		}
	}

	public TypeName GetElementType()
	{
		if (!IsArray && !IsPointer && !IsByRef)
		{
			TypeNameParserHelpers.ThrowInvalidOperation_NoElement();
		}
		return _elementOrGenericType;
	}

	public TypeName GetGenericTypeDefinition()
	{
		if (!IsConstructedGenericType)
		{
			TypeNameParserHelpers.ThrowInvalidOperation_NotGenericType();
		}
		return _elementOrGenericType;
	}

	public static TypeName Parse(ReadOnlySpan<char> typeName, TypeNameParseOptions? options = null)
	{
		return TypeNameParser.Parse(typeName, throwOnError: true, options);
	}

	public static bool TryParse(ReadOnlySpan<char> typeName, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out TypeName? result, TypeNameParseOptions? options = null)
	{
		result = TypeNameParser.Parse(typeName, throwOnError: false, options);
		return result != null;
	}

	public static string Unescape(string name)
	{
		if (name == null)
		{
			TypeNameParserHelpers.ThrowArgumentNullException("name");
		}
		return TypeNameParserHelpers.Unescape(name);
	}

	public int GetArrayRank()
	{
		if (_rankOrModifier != -1 && _rankOrModifier <= 0)
		{
			TypeNameParserHelpers.ThrowInvalidOperation_HasToBeArrayClass();
		}
		if (_rankOrModifier != -1)
		{
			return _rankOrModifier;
		}
		return 1;
	}

	public ImmutableArray<TypeName> GetGenericArguments()
	{
		return _genericArguments;
	}

	public TypeName WithAssemblyName(AssemblyNameInfo? assemblyName)
	{
		if (!IsSimple)
		{
			TypeNameParserHelpers.ThrowInvalidOperation_NotSimpleName(FullName);
		}
		TypeName declaringType = (IsNested ? DeclaringType.WithAssemblyName(assemblyName) : null);
		return new TypeName(_fullName, assemblyName, null, declaringType, ImmutableArray<TypeName>.Empty, 0, _nestedNameLength);
	}

	public TypeName MakeSZArrayTypeName()
	{
		return MakeElementTypeName(-1);
	}

	public TypeName MakeArrayTypeName(int rank)
	{
		if (rank > 0)
		{
			return MakeElementTypeName(rank);
		}
		throw new ArgumentOutOfRangeException("rank");
	}

	public TypeName MakePointerTypeName()
	{
		return MakeElementTypeName(-2);
	}

	public TypeName MakeByRefTypeName()
	{
		return MakeElementTypeName(-3);
	}

	public TypeName MakeGenericTypeName(ImmutableArray<TypeName> typeArguments)
	{
		if (!IsSimple)
		{
			TypeNameParserHelpers.ThrowInvalidOperation_NotSimpleName(FullName);
		}
		return new TypeName(null, AssemblyName, this, _declaringType, typeArguments);
	}

	private TypeName MakeElementTypeName(int rankOrModifier)
	{
		return new TypeName(null, AssemblyName, this, null, ImmutableArray<TypeName>.Empty, rankOrModifier);
	}
}
