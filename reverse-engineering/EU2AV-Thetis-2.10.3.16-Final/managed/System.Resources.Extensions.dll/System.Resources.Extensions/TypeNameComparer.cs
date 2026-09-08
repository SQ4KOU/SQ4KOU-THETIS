using System.Collections.Generic;
using System.Numerics.Hashing;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Resources.Extensions;

internal sealed class TypeNameComparer : IEqualityComparer<string>
{
	private static readonly char[] s_whiteSpaceChars = new char[4] { ' ', '\n', '\r', '\t' };

	public static TypeNameComparer Instance { get; } = new TypeNameComparer();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ReadOnlySpan<char> ReadTypeName(ReadOnlySpan<char> assemblyQualifiedTypeName)
	{
		int num = assemblyQualifiedTypeName.IndexOf(',');
		if (num != -1)
		{
			return assemblyQualifiedTypeName.Slice(0, num);
		}
		return assemblyQualifiedTypeName;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ReadOnlySpan<char> ReadAssemblySimpleName(ReadOnlySpan<char> assemblyName)
	{
		int num = assemblyName.IndexOf(',');
		if (num != -1)
		{
			return assemblyName.Slice(0, num).TrimEnd(s_whiteSpaceChars);
		}
		return assemblyName;
	}

	private static bool IsMscorlib(ReadOnlySpan<char> assemblyName)
	{
		return assemblyName.Equals(MemoryExtensions.AsSpan("mscorlib"), StringComparison.OrdinalIgnoreCase);
	}

	public bool Equals(string assemblyQualifiedTypeName1, string assemblyQualifiedTypeName2)
	{
		if (assemblyQualifiedTypeName1 == null)
		{
			throw new ArgumentNullException("assemblyQualifiedTypeName1");
		}
		if (assemblyQualifiedTypeName2 == null)
		{
			throw new ArgumentNullException("assemblyQualifiedTypeName2");
		}
		if ((object)assemblyQualifiedTypeName1 == assemblyQualifiedTypeName2)
		{
			return true;
		}
		ReadOnlySpan<char> readOnlySpan = MemoryExtensions.AsSpan(assemblyQualifiedTypeName1).TrimStart(s_whiteSpaceChars);
		ReadOnlySpan<char> readOnlySpan2 = MemoryExtensions.AsSpan(assemblyQualifiedTypeName2).TrimStart(s_whiteSpaceChars);
		ReadOnlySpan<char> span = ReadTypeName(readOnlySpan);
		ReadOnlySpan<char> other = ReadTypeName(readOnlySpan2);
		if (!span.Equals(other, StringComparison.Ordinal))
		{
			return false;
		}
		readOnlySpan = ((readOnlySpan.Length > span.Length) ? readOnlySpan.Slice(span.Length + 1).TrimStart(s_whiteSpaceChars) : ReadOnlySpan<char>.Empty);
		readOnlySpan2 = ((readOnlySpan2.Length > other.Length) ? readOnlySpan2.Slice(other.Length + 1).TrimStart(s_whiteSpaceChars) : ReadOnlySpan<char>.Empty);
		ReadOnlySpan<char> readOnlySpan3 = ReadAssemblySimpleName(readOnlySpan);
		ReadOnlySpan<char> readOnlySpan4 = ReadAssemblySimpleName(readOnlySpan2);
		if ((readOnlySpan3.IsEmpty && !readOnlySpan.IsEmpty) || (readOnlySpan4.IsEmpty && !readOnlySpan2.IsEmpty))
		{
			return false;
		}
		if (readOnlySpan3.IsEmpty)
		{
			if (!readOnlySpan4.IsEmpty)
			{
				return IsMscorlib(readOnlySpan4);
			}
			return true;
		}
		if (readOnlySpan4.IsEmpty)
		{
			return IsMscorlib(readOnlySpan3);
		}
		if (!readOnlySpan3.Equals(readOnlySpan4, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (IsMscorlib(readOnlySpan3))
		{
			return true;
		}
		AssemblyName assemblyName = new AssemblyName(readOnlySpan.ToString());
		AssemblyName assemblyName2 = new AssemblyName(readOnlySpan2.ToString());
		if (assemblyName.CultureInfo?.LCID != assemblyName2.CultureInfo?.LCID)
		{
			return false;
		}
		byte[] publicKeyToken = assemblyName.GetPublicKeyToken();
		return MemoryExtensions.SequenceEqual(other: assemblyName2.GetPublicKeyToken(), span: MemoryExtensions.AsSpan(publicKeyToken));
	}

	public int GetHashCode(string assemblyQualifiedTypeName)
	{
		ReadOnlySpan<char> readOnlySpan = ReadTypeName(MemoryExtensions.AsSpan(assemblyQualifiedTypeName).TrimStart(s_whiteSpaceChars));
		int num = 0;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			num = System.Numerics.Hashing.HashHelpers.Combine(num, readOnlySpan[i].GetHashCode());
		}
		return num;
	}
}
