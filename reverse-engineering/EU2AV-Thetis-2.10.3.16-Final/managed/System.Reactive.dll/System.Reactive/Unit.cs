using System.Runtime.InteropServices;

namespace System.Reactive;

[Serializable]
[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct Unit : IEquatable<Unit>
{
	public static Unit Default => default(Unit);

	public bool Equals(Unit other)
	{
		return true;
	}

	public override bool Equals(object? obj)
	{
		return obj is Unit;
	}

	public override int GetHashCode()
	{
		return 0;
	}

	public override string ToString()
	{
		return "()";
	}

	public static bool operator ==(Unit first, Unit second)
	{
		return true;
	}

	public static bool operator !=(Unit first, Unit second)
	{
		return false;
	}
}
