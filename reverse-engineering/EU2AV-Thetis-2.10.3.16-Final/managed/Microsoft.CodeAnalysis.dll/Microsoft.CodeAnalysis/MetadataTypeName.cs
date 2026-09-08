using System;
using System.Collections.Immutable;
using System.Globalization;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

[NonCopyable]
internal struct MetadataTypeName
{
	public readonly struct Key : IEquatable<Key>
	{
		private readonly string _namespaceOrFullyQualifiedName;

		private readonly string _typeName;

		private readonly byte _useCLSCompliantNameArityEncoding;

		private readonly short _forcedArity;

		private bool HasFullyQualifiedName => _typeName == null;

		internal Key(in MetadataTypeName mdTypeName)
		{
			if (mdTypeName.IsNull)
			{
				_namespaceOrFullyQualifiedName = null;
				_typeName = null;
				_useCLSCompliantNameArityEncoding = 0;
				_forcedArity = 0;
				return;
			}
			if (mdTypeName._fullName != null)
			{
				_namespaceOrFullyQualifiedName = mdTypeName._fullName;
				_typeName = null;
			}
			else
			{
				_namespaceOrFullyQualifiedName = mdTypeName._namespaceName;
				_typeName = mdTypeName._typeName;
			}
			_useCLSCompliantNameArityEncoding = (mdTypeName.UseCLSCompliantNameArityEncoding ? ((byte)1) : ((byte)0));
			_forcedArity = mdTypeName._forcedArity;
		}

		public bool Equals(Key other)
		{
			if (_useCLSCompliantNameArityEncoding == other._useCLSCompliantNameArityEncoding && _forcedArity == other._forcedArity)
			{
				return EqualNames(ref other);
			}
			return false;
		}

		private bool EqualNames(ref Key other)
		{
			if (_typeName == other._typeName)
			{
				return _namespaceOrFullyQualifiedName == other._namespaceOrFullyQualifiedName;
			}
			if (HasFullyQualifiedName)
			{
				return MetadataHelpers.SplitNameEqualsFullyQualifiedName(other._namespaceOrFullyQualifiedName, other._typeName, _namespaceOrFullyQualifiedName);
			}
			if (other.HasFullyQualifiedName)
			{
				return MetadataHelpers.SplitNameEqualsFullyQualifiedName(_namespaceOrFullyQualifiedName, _typeName, other._namespaceOrFullyQualifiedName);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Key)
			{
				return Equals((Key)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(GetHashCodeName(), Hash.Combine(_useCLSCompliantNameArityEncoding != 0, _forcedArity));
		}

		private int GetHashCodeName()
		{
			int num = Hash.GetFNVHashCode(_namespaceOrFullyQualifiedName);
			if (!HasFullyQualifiedName)
			{
				num = Hash.CombineFNVHash(num, '.');
				num = Hash.CombineFNVHash(num, _typeName);
			}
			return num;
		}
	}

	private string _fullName;

	private string _namespaceName;

	private ReadOnlyMemory<char> _namespaceNameMemory;

	private string _typeName;

	private ReadOnlyMemory<char> _typeNameMemory;

	private string _unmangledTypeName;

	private ReadOnlyMemory<char> _unmangledTypeNameMemory;

	private short _inferredArity;

	private short _forcedArity;

	private bool _useCLSCompliantNameArityEncoding;

	private ImmutableArray<string> _namespaceSegments;

	private ImmutableArray<ReadOnlyMemory<char>> _namespaceSegmentsMemory;

	public string FullName
	{
		get
		{
			if (_fullName == null)
			{
				_fullName = MetadataHelpers.BuildQualifiedName(_namespaceName, _typeName);
			}
			return _fullName;
		}
	}

	public ReadOnlyMemory<char> NamespaceNameMemory
	{
		get
		{
			if (_namespaceNameMemory.Equals(default(ReadOnlyMemory<char>)))
			{
				_typeNameMemory = MetadataHelpers.SplitQualifiedName(_fullName, out _namespaceNameMemory);
			}
			return _namespaceNameMemory;
		}
	}

	public string NamespaceName => _namespaceName ?? (_namespaceName = NamespaceNameMemory.ToString());

	public ReadOnlyMemory<char> TypeNameMemory
	{
		get
		{
			if (_typeNameMemory.Equals(default(ReadOnlyMemory<char>)))
			{
				_typeNameMemory = MetadataHelpers.SplitQualifiedName(_fullName, out _namespaceNameMemory);
			}
			return _typeNameMemory;
		}
	}

	public string TypeName => _typeName ?? (_typeName = TypeNameMemory.ToString());

	public ReadOnlyMemory<char> UnmangledTypeNameMemory
	{
		get
		{
			if (_unmangledTypeNameMemory.Equals(default(ReadOnlyMemory<char>)))
			{
				_unmangledTypeNameMemory = MetadataHelpers.InferTypeArityAndUnmangleMetadataName(TypeNameMemory, out _inferredArity);
			}
			return _unmangledTypeNameMemory;
		}
	}

	public string UnmangledTypeName
	{
		get
		{
			if (_unmangledTypeName == null)
			{
				_unmangledTypeName = (UnmangledTypeNameMemory.Equals(TypeNameMemory) ? TypeName : UnmangledTypeNameMemory.ToString());
			}
			return _unmangledTypeName;
		}
	}

	public int InferredArity
	{
		get
		{
			if (_inferredArity == -1)
			{
				_unmangledTypeNameMemory = MetadataHelpers.InferTypeArityAndUnmangleMetadataName(TypeNameMemory, out _inferredArity);
			}
			return _inferredArity;
		}
	}

	public bool IsMangled => InferredArity > 0;

	public readonly bool UseCLSCompliantNameArityEncoding => _useCLSCompliantNameArityEncoding;

	public readonly int ForcedArity => _forcedArity;

	public ImmutableArray<ReadOnlyMemory<char>> NamespaceSegmentsMemory
	{
		get
		{
			if (_namespaceSegmentsMemory.IsDefault)
			{
				_namespaceSegmentsMemory = MetadataHelpers.SplitQualifiedName(NamespaceNameMemory);
			}
			return _namespaceSegmentsMemory;
		}
	}

	public ImmutableArray<string> NamespaceSegments
	{
		get
		{
			if (_namespaceSegments.IsDefault)
			{
				_namespaceSegments = NamespaceSegmentsMemory.SelectAsArray((ReadOnlyMemory<char> s) => s.ToString());
			}
			return _namespaceSegments;
		}
	}

	public readonly bool IsNull
	{
		get
		{
			if (_typeName == null)
			{
				return _fullName == null;
			}
			return false;
		}
	}

	public static MetadataTypeName FromFullName(string fullName, bool useCLSCompliantNameArityEncoding = false, int forcedArity = -1)
	{
		MetadataTypeName result = default(MetadataTypeName);
		result._fullName = fullName;
		result._namespaceName = null;
		result._namespaceNameMemory = default(ReadOnlyMemory<char>);
		result._typeName = null;
		result._typeNameMemory = default(ReadOnlyMemory<char>);
		result._unmangledTypeName = null;
		result._unmangledTypeNameMemory = default(ReadOnlyMemory<char>);
		result._inferredArity = -1;
		result._useCLSCompliantNameArityEncoding = useCLSCompliantNameArityEncoding;
		result._forcedArity = (short)forcedArity;
		result._namespaceSegments = default(ImmutableArray<string>);
		result._namespaceSegmentsMemory = default(ImmutableArray<ReadOnlyMemory<char>>);
		return result;
	}

	public static MetadataTypeName FromNamespaceAndTypeName(string namespaceName, string typeName, bool useCLSCompliantNameArityEncoding = false, int forcedArity = -1)
	{
		MetadataTypeName result = default(MetadataTypeName);
		result._fullName = null;
		result._namespaceName = namespaceName;
		result._namespaceNameMemory = System.MemoryExtensions.AsMemory(namespaceName);
		result._typeName = typeName;
		result._typeNameMemory = System.MemoryExtensions.AsMemory(typeName);
		result._unmangledTypeName = null;
		result._unmangledTypeNameMemory = default(ReadOnlyMemory<char>);
		result._inferredArity = -1;
		result._useCLSCompliantNameArityEncoding = useCLSCompliantNameArityEncoding;
		result._forcedArity = (short)forcedArity;
		result._namespaceSegments = default(ImmutableArray<string>);
		result._namespaceSegmentsMemory = default(ImmutableArray<ReadOnlyMemory<char>>);
		return result;
	}

	public static MetadataTypeName FromTypeName(string typeName, bool useCLSCompliantNameArityEncoding = false, int forcedArity = -1)
	{
		MetadataTypeName result = default(MetadataTypeName);
		result._fullName = typeName;
		result._namespaceName = string.Empty;
		result._namespaceNameMemory = System.MemoryExtensions.AsMemory(string.Empty);
		result._typeName = typeName;
		result._typeNameMemory = System.MemoryExtensions.AsMemory(typeName);
		result._unmangledTypeName = null;
		result._unmangledTypeNameMemory = default(ReadOnlyMemory<char>);
		result._inferredArity = -1;
		result._useCLSCompliantNameArityEncoding = useCLSCompliantNameArityEncoding;
		result._forcedArity = (short)forcedArity;
		result._namespaceSegments = ImmutableArray<string>.Empty;
		result._namespaceSegmentsMemory = ImmutableArray<ReadOnlyMemory<char>>.Empty;
		return result;
	}

	public override string ToString()
	{
		if (IsNull)
		{
			return "{Null}";
		}
		return $"{{{NamespaceName},{TypeName},{UseCLSCompliantNameArityEncoding.ToString()},{_forcedArity.ToString(CultureInfo.InvariantCulture)}}}";
	}

	public readonly Key ToKey()
	{
		return new Key(this);
	}
}
