using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

[InternalImplementationOnly]
public interface IOperation
{
	[NonDefaultable]
	public readonly struct OperationList : IReadOnlyCollection<IOperation>, IEnumerable<IOperation>, IEnumerable
	{
		[NonDefaultable]
		public struct Enumerator
		{
			private readonly Operation _operation;

			private int _currentSlot;

			private int _currentIndex;

			public IOperation Current => _operation.GetCurrent(_currentSlot, _currentIndex);

			internal Enumerator(Operation operation)
			{
				_operation = operation;
				_currentSlot = -1;
				_currentIndex = -1;
			}

			public bool MoveNext()
			{
				bool result;
				(result, _currentSlot, _currentIndex) = _operation.MoveNext(_currentSlot, _currentIndex);
				return result;
			}

			public void Reset()
			{
				_currentSlot = -1;
				_currentIndex = -1;
			}
		}

		private sealed class EnumeratorImpl : IEnumerator<IOperation>, IEnumerator, IDisposable
		{
			private Enumerator _enumerator;

			public IOperation Current => _enumerator.Current;

			object? IEnumerator.Current => _enumerator.Current;

			public EnumeratorImpl(Enumerator enumerator)
			{
				_enumerator = enumerator;
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				return _enumerator.MoveNext();
			}

			public void Reset()
			{
				_enumerator.Reset();
			}
		}

		[NonDefaultable]
		public readonly struct Reversed : IReadOnlyCollection<IOperation>, IEnumerable<IOperation>, IEnumerable
		{
			[NonDefaultable]
			public struct Enumerator
			{
				private readonly Operation _operation;

				private int _currentSlot;

				private int _currentIndex;

				public IOperation Current => _operation.GetCurrent(_currentSlot, _currentIndex);

				internal Enumerator(Operation operation)
				{
					_operation = operation;
					_currentSlot = int.MaxValue;
					_currentIndex = int.MaxValue;
				}

				public bool MoveNext()
				{
					bool result;
					(result, _currentSlot, _currentIndex) = _operation.MoveNextReversed(_currentSlot, _currentIndex);
					return result;
				}

				public void Reset()
				{
					_currentIndex = int.MaxValue;
					_currentSlot = int.MaxValue;
				}
			}

			private sealed class EnumeratorImpl : IEnumerator<IOperation>, IEnumerator, IDisposable
			{
				private Enumerator _enumerator;

				public IOperation Current => _enumerator.Current;

				object? IEnumerator.Current => _enumerator.Current;

				public EnumeratorImpl(Enumerator enumerator)
				{
					_enumerator = enumerator;
				}

				public void Dispose()
				{
				}

				public bool MoveNext()
				{
					return _enumerator.MoveNext();
				}

				public void Reset()
				{
					_enumerator.Reset();
				}
			}

			private readonly Operation _operation;

			public int Count => _operation.ChildOperationsCount;

			internal Reversed(Operation operation)
			{
				_operation = operation;
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(_operation);
			}

			public ImmutableArray<IOperation> ToImmutableArray()
			{
				GetEnumerator();
				Operation operation = _operation;
				if (operation != null)
				{
					if (operation.ChildOperationsCount == 0)
					{
						return ImmutableArray<IOperation>.Empty;
					}
					if (operation is NoneOperation noneOperation)
					{
						ImmutableArray<IOperation> children = noneOperation.Children;
						return reverseArray(children);
					}
					if (operation is InvalidOperation invalidOperation)
					{
						ImmutableArray<IOperation> children2 = invalidOperation.Children;
						return reverseArray(children2);
					}
				}
				ArrayBuilder<IOperation> instance = ArrayBuilder<IOperation>.GetInstance(Count);
				Enumerator enumerator = GetEnumerator();
				while (enumerator.MoveNext())
				{
					IOperation current = enumerator.Current;
					instance.Add(current);
				}
				return instance.ToImmutableAndFree();
				static ImmutableArray<IOperation> reverseArray(ImmutableArray<IOperation> input)
				{
					ArrayBuilder<IOperation> instance2 = ArrayBuilder<IOperation>.GetInstance(input.Length);
					for (int num = input.Length - 1; num >= 0; num--)
					{
						instance2.Add(input[num]);
					}
					return instance2.ToImmutableAndFree();
				}
			}

			IEnumerator<IOperation> IEnumerable<IOperation>.GetEnumerator()
			{
				if (Count == 0)
				{
					return SpecializedCollections.EmptyEnumerator<IOperation>();
				}
				return new EnumeratorImpl(new Enumerator(_operation));
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<IOperation>)this).GetEnumerator();
			}
		}

		private readonly Operation _operation;

		public int Count => _operation.ChildOperationsCount;

		internal OperationList(Operation operation)
		{
			_operation = operation;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(_operation);
		}

		public ImmutableArray<IOperation> ToImmutableArray()
		{
			Operation operation = _operation;
			if (operation != null)
			{
				if (operation.ChildOperationsCount == 0)
				{
					return ImmutableArray<IOperation>.Empty;
				}
				if (operation is NoneOperation noneOperation)
				{
					return noneOperation.Children;
				}
				if (operation is InvalidOperation invalidOperation)
				{
					return invalidOperation.Children;
				}
			}
			ArrayBuilder<IOperation> instance = ArrayBuilder<IOperation>.GetInstance(Count);
			Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				IOperation current = enumerator.Current;
				instance.Add(current);
			}
			return instance.ToImmutableAndFree();
		}

		IEnumerator<IOperation> IEnumerable<IOperation>.GetEnumerator()
		{
			if (Count == 0)
			{
				return SpecializedCollections.EmptyEnumerator<IOperation>();
			}
			return new EnumeratorImpl(new Enumerator(_operation));
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<IOperation>)this).GetEnumerator();
		}

		public bool Any()
		{
			return Count > 0;
		}

		public IOperation First()
		{
			Enumerator enumerator = GetEnumerator();
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
			throw new InvalidOperationException();
		}

		public Reversed Reverse()
		{
			return new Reversed(_operation);
		}

		public IOperation Last()
		{
			Reversed.Enumerator enumerator = Reverse().GetEnumerator();
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
			throw new InvalidOperationException();
		}
	}

	IOperation? Parent { get; }

	OperationKind Kind { get; }

	SyntaxNode Syntax { get; }

	ITypeSymbol? Type { get; }

	Optional<object?> ConstantValue { get; }

	[Obsolete("This API has performance penalties, please use ChildOperations instead.", false)]
	IEnumerable<IOperation> Children { get; }

	OperationList ChildOperations { get; }

	string Language { get; }

	bool IsImplicit { get; }

	SemanticModel? SemanticModel { get; }

	void Accept(OperationVisitor visitor);

	TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument);
}
