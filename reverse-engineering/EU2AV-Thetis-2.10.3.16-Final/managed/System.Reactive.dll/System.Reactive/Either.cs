using System.Collections.Generic;
using System.Globalization;

namespace System.Reactive;

internal abstract class Either<TLeft, TRight>
{
	public sealed class Left : Either<TLeft, TRight>, IEquatable<Left>
	{
		public TLeft Value { get; }

		public Left(TLeft value)
		{
			Value = value;
		}

		public override TResult Switch<TResult>(Func<TLeft, TResult> caseLeft, Func<TRight, TResult> caseRight)
		{
			return caseLeft(Value);
		}

		public override void Switch(Action<TLeft> caseLeft, Action<TRight> caseRight)
		{
			caseLeft(Value);
		}

		public bool Equals(Left? other)
		{
			if (other == this)
			{
				return true;
			}
			if (other == null)
			{
				return false;
			}
			return EqualityComparer<TLeft>.Default.Equals(Value, other.Value);
		}

		public override bool Equals(object? obj)
		{
			return Equals(obj as Either<TLeft, TRight>.Left);
		}

		public override int GetHashCode()
		{
			TLeft value = Value;
			if (value == null)
			{
				return 0;
			}
			return value.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, "Left({0})", Value);
		}
	}

	public sealed class Right : Either<TLeft, TRight>, IEquatable<Right>
	{
		public TRight Value { get; }

		public Right(TRight value)
		{
			Value = value;
		}

		public override TResult Switch<TResult>(Func<TLeft, TResult> caseLeft, Func<TRight, TResult> caseRight)
		{
			return caseRight(Value);
		}

		public override void Switch(Action<TLeft> caseLeft, Action<TRight> caseRight)
		{
			caseRight(Value);
		}

		public bool Equals(Right? other)
		{
			if (other == this)
			{
				return true;
			}
			if (other == null)
			{
				return false;
			}
			return EqualityComparer<TRight>.Default.Equals(Value, other.Value);
		}

		public override bool Equals(object? obj)
		{
			return Equals(obj as Either<TLeft, TRight>.Right);
		}

		public override int GetHashCode()
		{
			TRight value = Value;
			if (value == null)
			{
				return 0;
			}
			return value.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, "Right({0})", Value);
		}
	}

	private Either()
	{
	}

	public static Either<TLeft, TRight> CreateLeft(TLeft value)
	{
		return new Left(value);
	}

	public static Either<TLeft, TRight> CreateRight(TRight value)
	{
		return new Right(value);
	}

	public abstract TResult Switch<TResult>(Func<TLeft, TResult> caseLeft, Func<TRight, TResult> caseRight);

	public abstract void Switch(Action<TLeft> caseLeft, Action<TRight> caseRight);
}
