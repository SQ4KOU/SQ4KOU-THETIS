namespace System.Collections.Generic;

internal static class RoslynEqualityComparerExtensions
{
	extension<T>(EqualityComparer<T>)
	{
		public static EqualityComparer<T> Create(Func<T?, T?, bool> equals, Func<T, int>? getHashCode = null)
		{
			if (getHashCode == null)
			{
				getHashCode = delegate
				{
					throw new NotSupportedException();
				};
			}
			return new DelegateEqualityComparer<T>(equals, getHashCode);
		}
	}

	private sealed class DelegateEqualityComparer<T>(Func<T?, T?, bool> equals, Func<T, int> getHashCode) : EqualityComparer<T>
	{
		private readonly Func<T?, T?, bool> _equals = equals;

		private readonly Func<T, int> _getHashCode = getHashCode;

		public override bool Equals(T? x, T? y)
		{
			return _equals(x, y);
		}

		public override int GetHashCode(T obj)
		{
			return _getHashCode(obj);
		}

		public override bool Equals(object? obj)
		{
			if (obj is DelegateEqualityComparer<T> delegateEqualityComparer && _equals == delegateEqualityComparer._equals)
			{
				return _getHashCode == delegateEqualityComparer._getHashCode;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return _equals.GetHashCode() * -1521134295 + _getHashCode.GetHashCode();
		}
	}
}
