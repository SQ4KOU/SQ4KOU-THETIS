using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class DecisionDagBuilder
{
	private sealed class DecisionDag
	{
		public readonly DagState RootNode;

		public DecisionDag(DagState rootNode)
		{
			RootNode = rootNode;
		}

		private static void AddSuccessor(ref TemporaryArray<DagState> builder, DagState state)
		{
			builder.AddIfNotNull(state.TrueBranch);
			builder.AddIfNotNull(state.FalseBranch);
		}

		public bool TryGetTopologicallySortedReachableStates(out ImmutableArray<DagState> result)
		{
			return TopologicalSort.TryIterativeSort(RootNode, AddSuccessor, out result);
		}
	}

	private readonly struct FrozenArrayBuilder<T>
	{
		private readonly ArrayBuilder<T> _arrayBuilder;

		public int Count => _arrayBuilder.Count;

		public T this[int i] => _arrayBuilder[i];

		public FrozenArrayBuilder(ArrayBuilder<T> arrayBuilder)
		{
			if (arrayBuilder.Capacity >= 128 && arrayBuilder.Count < 128 && arrayBuilder.Capacity >= arrayBuilder.Count * 2)
			{
				arrayBuilder.Capacity = arrayBuilder.Count;
			}
			_arrayBuilder = arrayBuilder;
		}

		public void Free()
		{
			_arrayBuilder.Free();
		}

		public T First()
		{
			return _arrayBuilder.First();
		}

		public ArrayBuilder<T>.Enumerator GetEnumerator()
		{
			return _arrayBuilder.GetEnumerator();
		}

		public FrozenArrayBuilder<T> RemoveAt(int index)
		{
			ArrayBuilder<T> instance = ArrayBuilder<T>.GetInstance(Count - 1);
			for (int i = 0; i < index; i++)
			{
				instance.Add(this[i]);
			}
			int j = index + 1;
			for (int count = Count; j < count; j++)
			{
				instance.Add(this[j]);
			}
			return AsFrozen(instance);
		}
	}

	private sealed class DagState
	{
		private static readonly ObjectPool<DagState> s_dagStatePool = new ObjectPool<DagState>(() => new DagState());

		public BoundDagTest? SelectedTest;

		public DagState? TrueBranch;

		public DagState? FalseBranch;

		public BoundDecisionDagNode? Dag;

		public ImmutableDictionary<BoundDagTemp, IValueSet> RemainingValues { get; private set; }

		public FrozenArrayBuilder<StateForCase> Cases { get; private set; }

		private DagState()
		{
		}

		public static DagState GetInstance(FrozenArrayBuilder<StateForCase> cases, ImmutableDictionary<BoundDagTemp, IValueSet> remainingValues)
		{
			DagState dagState = s_dagStatePool.Allocate();
			dagState.Cases = cases;
			dagState.RemainingValues = remainingValues;
			return dagState;
		}

		public void ClearAndFree()
		{
			Cases.Free();
			Cases = default(FrozenArrayBuilder<StateForCase>);
			RemainingValues = null;
			SelectedTest = null;
			TrueBranch = null;
			FalseBranch = null;
			Dag = null;
			s_dagStatePool.Free(this);
		}

		internal BoundDagTest ComputeSelectedTest()
		{
			return Cases[0].RemainingTests.ComputeSelectedTest();
		}

		internal void UpdateRemainingValues(ImmutableDictionary<BoundDagTemp, IValueSet> newRemainingValues)
		{
			RemainingValues = newRemainingValues;
			SelectedTest = null;
			TrueBranch = null;
			FalseBranch = null;
		}
	}

	private sealed class DagStateEquivalence : IEqualityComparer<DagState>
	{
		public static readonly DagStateEquivalence Instance = new DagStateEquivalence();

		private DagStateEquivalence()
		{
		}

		public bool Equals(DagState? x, DagState? y)
		{
			if (x == y)
			{
				return true;
			}
			if (x.Cases.Count != y.Cases.Count)
			{
				return false;
			}
			int i = 0;
			for (int count = x.Cases.Count; i < count; i++)
			{
				if (!x.Cases[i].Equals(y.Cases[i]))
				{
					return false;
				}
			}
			return true;
		}

		public int GetHashCode(DagState x)
		{
			int num = 0;
			foreach (StateForCase @case in x.Cases)
			{
				num = Hash.Combine(@case.GetHashCode(), num);
			}
			return Hash.Combine(num, x.Cases.Count);
		}
	}

	private readonly struct StateForCase(int Index, SyntaxNode Syntax, Tests RemainingTests, ImmutableArray<BoundPatternBinding> Bindings, BoundExpression? WhenClause, LabelSymbol CaseLabel)
	{
		public readonly int Index = Index;

		public readonly SyntaxNode Syntax = Syntax;

		public readonly Tests RemainingTests = RemainingTests;

		public readonly ImmutableArray<BoundPatternBinding> Bindings = Bindings;

		public readonly BoundExpression? WhenClause = WhenClause;

		public readonly LabelSymbol CaseLabel = CaseLabel;

		public bool IsFullyMatched
		{
			get
			{
				if (RemainingTests is Tests.True)
				{
					if (WhenClause != null)
					{
						return WhenClause.ConstantValueOpt == ConstantValue.True;
					}
					return true;
				}
				return false;
			}
		}

		public bool PatternIsSatisfied => RemainingTests is Tests.True;

		public bool IsImpossible => RemainingTests is Tests.False;

		public override bool Equals(object? obj)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/DecisionDagBuilder.cs", 2071);
		}

		public bool Equals(StateForCase other)
		{
			if (Index == other.Index)
			{
				return RemainingTests.Equals(other.RemainingTests);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(RemainingTests.GetHashCode(), Index);
		}

		public StateForCase WithRemainingTests(Tests newRemainingTests)
		{
			if (!newRemainingTests.Equals(RemainingTests))
			{
				return new StateForCase(Index, Syntax, newRemainingTests, Bindings, WhenClause, CaseLabel);
			}
			return this;
		}

		public StateForCase RewriteNestedLengthTests()
		{
			return WithRemainingTests(RemainingTests.RewriteNestedLengthTests());
		}
	}

	private abstract class Tests
	{
		public sealed class True : Tests
		{
			public static readonly True Instance = new True();

			public override string Dump(Func<BoundDagTest, string> dump)
			{
				return "TRUE";
			}

			public override void Filter(DecisionDagBuilder builder, BoundDagTest test, DagState state, IValueSet? whenTrueValues, IValueSet? whenFalseValues, out Tests whenTrue, out Tests whenFalse, ref bool foundExplicitNullTest)
			{
				whenTrue = (whenFalse = this);
			}
		}

		public sealed class False : Tests
		{
			public static readonly False Instance = new False();

			public override string Dump(Func<BoundDagTest, string> dump)
			{
				return "FALSE";
			}

			public override void Filter(DecisionDagBuilder builder, BoundDagTest test, DagState state, IValueSet? whenTrueValues, IValueSet? whenFalseValues, out Tests whenTrue, out Tests whenFalse, ref bool foundExplicitNullTest)
			{
				whenTrue = (whenFalse = this);
			}
		}

		public sealed class One : Tests
		{
			public readonly BoundDagTest Test;

			public One(BoundDagTest test)
			{
				Test = test;
			}

			public void Deconstruct(out BoundDagTest Test)
			{
				Test = this.Test;
			}

			public override void Filter(DecisionDagBuilder builder, BoundDagTest test, DagState state, IValueSet? whenTrueValues, IValueSet? whenFalseValues, out Tests whenTrue, out Tests whenFalse, ref bool foundExplicitNullTest)
			{
				SyntaxNode syntax = test.Syntax;
				BoundDagTest test2 = Test;
				if (test2 is BoundDagEvaluation || !builder.CheckInputRelation(syntax, state, test, test2, out Tests relationCondition, out Tests relationEffect))
				{
					whenTrue = (whenFalse = this);
					return;
				}
				builder.CheckConsistentDecision(test, test2, whenTrueValues, whenFalseValues, syntax, out var trueTestPermitsTrueOther, out var falseTestPermitsTrueOther, out var trueTestImpliesTrueOther, out var falseTestImpliesTrueOther, ref foundExplicitNullTest);
				whenTrue = rewrite(trueTestImpliesTrueOther, trueTestPermitsTrueOther, relationCondition, relationEffect, this);
				whenFalse = rewrite(falseTestImpliesTrueOther, falseTestPermitsTrueOther, relationCondition, relationEffect, this);
				static Tests rewrite(bool decisionImpliesTrueOther, bool decisionPermitsTrueOther, Tests tests, Tests t, Tests other)
				{
					if (!decisionImpliesTrueOther)
					{
						if (decisionPermitsTrueOther)
						{
							return AndSequence.Create(OrSequence.Create(Not.Create(tests), t), other);
						}
						return AndSequence.Create(Not.Create(AndSequence.Create(tests, t)), other);
					}
					return OrSequence.Create(AndSequence.Create(tests, t), other);
				}
			}

			public override BoundDagTest ComputeSelectedTest()
			{
				return Test;
			}

			public override Tests RemoveEvaluation(BoundDagEvaluation e)
			{
				if (!e.Equals(Test))
				{
					return this;
				}
				return True.Instance;
			}

			public override string Dump(Func<BoundDagTest, string> dump)
			{
				return dump(Test);
			}

			public override bool Equals(object? obj)
			{
				if (this != obj)
				{
					if (obj is One one)
					{
						return Test.Equals(one.Test);
					}
					return false;
				}
				return true;
			}

			public override int GetHashCode()
			{
				return Test.GetHashCode();
			}

			public override Tests RewriteNestedLengthTests()
			{
				BoundDagTest test = Test;
				if (test.Input.Source is BoundDagPropertyEvaluation { IsLengthOrCount: not false } boundDagPropertyEvaluation)
				{
					if (boundDagPropertyEvaluation.Syntax.IsKind(SyntaxKind.ListPattern) && test is BoundDagRelationalTest boundDagRelationalTest && boundDagRelationalTest.Value.Int32Value == 0)
					{
						return True.Instance;
					}
					(BoundDagTemp, int) tuple = TryGetTopLevelLengthTemp(boundDagPropertyEvaluation);
					var (boundDagTemp, _) = tuple;
					if (boundDagTemp != null)
					{
						int item = tuple.Item2;
						BoundDagTest boundDagTest = test;
						if (!(boundDagTest is BoundDagValueTest boundDagValueTest))
						{
							if (boundDagTest is BoundDagRelationalTest boundDagRelationalTest2 && !boundDagRelationalTest2.Value.IsBad)
							{
								return knownResult(boundDagRelationalTest2.Relation, boundDagRelationalTest2.Value, item) ?? new One(new BoundDagRelationalTest(boundDagRelationalTest2.Syntax, boundDagRelationalTest2.OperatorKind, safeAdd(boundDagRelationalTest2.Value, item), boundDagTemp));
							}
						}
						else if (!boundDagValueTest.Value.IsBad)
						{
							return knownResult(BinaryOperatorKind.Equal, boundDagValueTest.Value, item) ?? new One(new BoundDagValueTest(boundDagValueTest.Syntax, safeAdd(boundDagValueTest.Value, item), boundDagTemp));
						}
					}
				}
				return this;
				static Tests? knownResult(BinaryOperatorKind relation, ConstantValue constant, int offset)
				{
					IValueSetFactory<int> forLength = ValueSetFactory.ForLength;
					IValueSet<int> other = forLength.Related(BinaryOperatorKind.LessThanOrEqual, int.MaxValue - offset);
					IValueSet valueSet = forLength.Related(relation, constant);
					if (valueSet.Intersect(other).IsEmpty)
					{
						return False.Instance;
					}
					if (valueSet.Complement().Intersect(other).IsEmpty)
					{
						return True.Instance;
					}
					return null;
				}
				static ConstantValue safeAdd(ConstantValue constant, int offset)
				{
					int int32Value = constant.Int32Value;
					return ConstantValue.Create((offset > int.MaxValue - int32Value) ? int.MaxValue : (int32Value + offset));
				}
			}
		}

		public sealed class Not : Tests
		{
			public readonly Tests Negated;

			private Not(Tests negated)
			{
				Negated = negated;
			}

			public static Tests Create(Tests negated)
			{
				if (!(negated is True))
				{
					if (!(negated is False))
					{
						if (!(negated is Not not))
						{
							if (!(negated is AndSequence negated2))
							{
								if (!(negated is OrSequence orSequence))
								{
									if (negated is One negated3)
									{
										return new Not(negated3);
									}
									throw ExceptionUtilities.UnexpectedValue(negated);
								}
								return AndSequence.Create(NegateSequenceElements(orSequence.RemainingTests));
							}
							return new Not(negated2);
						}
						return not.Negated;
					}
					return True.Instance;
				}
				return False.Instance;
			}

			private static ArrayBuilder<Tests> NegateSequenceElements(ImmutableArray<Tests> seq)
			{
				ArrayBuilder<Tests> instance = ArrayBuilder<Tests>.GetInstance(seq.Length);
				foreach (Tests item in seq)
				{
					instance.Add(Create(item));
				}
				return instance;
			}

			public override Tests RemoveEvaluation(BoundDagEvaluation e)
			{
				return Create(Negated.RemoveEvaluation(e));
			}

			public override Tests RewriteNestedLengthTests()
			{
				return Create(Negated.RewriteNestedLengthTests());
			}

			public override BoundDagTest ComputeSelectedTest()
			{
				return Negated.ComputeSelectedTest();
			}

			public override string Dump(Func<BoundDagTest, string> dump)
			{
				return "Not (" + Negated.Dump(dump) + ")";
			}

			public override void Filter(DecisionDagBuilder builder, BoundDagTest test, DagState state, IValueSet? whenTrueValues, IValueSet? whenFalseValues, out Tests whenTrue, out Tests whenFalse, ref bool foundExplicitNullTest)
			{
				Negated.Filter(builder, test, state, whenTrueValues, whenFalseValues, out Tests whenTrue2, out Tests whenFalse2, ref foundExplicitNullTest);
				whenTrue = Create(whenTrue2);
				whenFalse = Create(whenFalse2);
			}

			public override bool Equals(object? obj)
			{
				if (this != obj)
				{
					if (obj is Not not)
					{
						return Negated.Equals(not.Negated);
					}
					return false;
				}
				return true;
			}

			public override int GetHashCode()
			{
				return Hash.Combine(Negated.GetHashCode(), typeof(Not).GetHashCode());
			}
		}

		public abstract class SequenceTests : Tests
		{
			public readonly ImmutableArray<Tests> RemainingTests;

			protected SequenceTests(ImmutableArray<Tests> remainingTests)
			{
				RemainingTests = remainingTests;
			}

			public abstract Tests Update(ArrayBuilder<Tests> remainingTests);

			public sealed override void Filter(DecisionDagBuilder builder, BoundDagTest test, DagState state, IValueSet? whenTrueValues, IValueSet? whenFalseValues, out Tests whenTrue, out Tests whenFalse, ref bool foundExplicitNullTest)
			{
				ArrayBuilder<Tests> instance = ArrayBuilder<Tests>.GetInstance();
				ArrayBuilder<SequenceTests> instance2 = ArrayBuilder<SequenceTests>.GetInstance();
				ArrayBuilder<Tests> instance3 = ArrayBuilder<Tests>.GetInstance();
				ArrayBuilder<Tests> instance4 = ArrayBuilder<Tests>.GetInstance();
				instance.Push(this);
				do
				{
					Tests tests = instance.Pop();
					if (!(tests is SequenceTests sequenceTests))
					{
						if (tests == null)
						{
							SequenceTests toAssemble = instance2.Pop();
							assemble(toAssemble, instance3);
							assemble(toAssemble, instance4);
						}
						else
						{
							tests.Filter(builder, test, state, whenTrueValues, whenFalseValues, out Tests whenTrue2, out Tests whenFalse2, ref foundExplicitNullTest);
							instance3.Push(whenTrue2);
							instance4.Push(whenFalse2);
						}
					}
					else
					{
						instance2.Push(sequenceTests);
						instance.Push(null);
						for (int num = sequenceTests.RemainingTests.Length - 1; num >= 0; num--)
						{
							instance.Push(sequenceTests.RemainingTests[num]);
						}
					}
				}
				while (instance.Count != 0);
				whenTrue = instance3.Pop();
				whenFalse = instance4.Pop();
				if (!instance3.IsEmpty || !instance4.IsEmpty || !instance2.IsEmpty)
				{
					throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/DecisionDagBuilder.cs", 2429);
				}
				instance.Free();
				instance2.Free();
				instance3.Free();
				instance4.Free();
				static void assemble(SequenceTests sequenceTests2, ArrayBuilder<Tests> arrayBuilder)
				{
					int length = sequenceTests2.RemainingTests.Length;
					ArrayBuilder<Tests> instance5 = ArrayBuilder<Tests>.GetInstance(length, null);
					for (int num2 = length - 1; num2 >= 0; num2--)
					{
						instance5[num2] = arrayBuilder.Pop();
					}
					arrayBuilder.Push(sequenceTests2.Update(instance5));
				}
			}

			public sealed override Tests RemoveEvaluation(BoundDagEvaluation e)
			{
				ArrayBuilder<Tests> instance = ArrayBuilder<Tests>.GetInstance();
				ArrayBuilder<SequenceTests> instance2 = ArrayBuilder<SequenceTests>.GetInstance();
				ArrayBuilder<Tests> instance3 = ArrayBuilder<Tests>.GetInstance();
				instance.Push(this);
				do
				{
					Tests tests = instance.Pop();
					if (!(tests is SequenceTests sequenceTests))
					{
						if (tests == null)
						{
							SequenceTests sequenceTests2 = instance2.Pop();
							int length = sequenceTests2.RemainingTests.Length;
							ArrayBuilder<Tests> instance4 = ArrayBuilder<Tests>.GetInstance(length);
							for (int i = 0; i < length; i++)
							{
								instance4.Add(instance3.Pop());
							}
							instance3.Push(sequenceTests2.Update(instance4));
						}
						else
						{
							instance3.Push(tests.RemoveEvaluation(e));
						}
					}
					else
					{
						instance2.Push(sequenceTests);
						instance.Push(null);
						instance.AddRange(sequenceTests.RemainingTests);
					}
				}
				while (instance.Count != 0);
				Tests result = instance3.Pop();
				if (!instance3.IsEmpty || !instance2.IsEmpty)
				{
					throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/DecisionDagBuilder.cs", 2493);
				}
				instance.Free();
				instance2.Free();
				instance3.Free();
				return result;
			}

			public sealed override Tests RewriteNestedLengthTests()
			{
				ArrayBuilder<Tests> instance = ArrayBuilder<Tests>.GetInstance();
				ArrayBuilder<SequenceTests> instance2 = ArrayBuilder<SequenceTests>.GetInstance();
				ArrayBuilder<Tests> instance3 = ArrayBuilder<Tests>.GetInstance();
				instance.Push(this);
				do
				{
					Tests tests = instance.Pop();
					if (!(tests is SequenceTests sequenceTests))
					{
						if (tests == null)
						{
							SequenceTests sequenceTests2 = instance2.Pop();
							int length = sequenceTests2.RemainingTests.Length;
							ArrayBuilder<Tests> instance4 = ArrayBuilder<Tests>.GetInstance(length);
							for (int i = 0; i < length; i++)
							{
								instance4.Add(instance3.Pop());
							}
							instance3.Push(sequenceTests2.Update(instance4));
						}
						else
						{
							instance3.Push(tests.RewriteNestedLengthTests());
						}
					}
					else
					{
						instance2.Push(sequenceTests);
						instance.Push(null);
						instance.AddRange(sequenceTests.RemainingTests);
					}
				}
				while (instance.Count != 0);
				Tests result = instance3.Pop();
				if (!instance3.IsEmpty || !instance2.IsEmpty)
				{
					throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/DecisionDagBuilder.cs", 2546);
				}
				instance.Free();
				instance2.Free();
				instance3.Free();
				return result;
			}

			public sealed override bool Equals(object? obj)
			{
				bool? flag = equalsEasyOut(this, obj);
				if (flag.HasValue)
				{
					return flag == true;
				}
				ArrayBuilder<Tests> instance = ArrayBuilder<Tests>.GetInstance();
				ArrayBuilder<Tests> instance2 = ArrayBuilder<Tests>.GetInstance();
				instance.AddRange(RemainingTests);
				instance2.AddRange(((SequenceTests)obj).RemainingTests);
				do
				{
					Tests tests = instance.Pop();
					Tests tests2 = instance2.Pop();
					if (tests is SequenceTests sequenceTests)
					{
						flag = equalsEasyOut(sequenceTests, tests2);
						if (flag.HasValue)
						{
							if (flag != true)
							{
								return false;
							}
						}
						else
						{
							instance.AddRange(sequenceTests.RemainingTests);
							instance2.AddRange(((SequenceTests)tests2).RemainingTests);
						}
					}
					else if (!tests.Equals(tests2))
					{
						return false;
					}
				}
				while (instance.Count != 0);
				if (!instance2.IsEmpty)
				{
					throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/DecisionDagBuilder.cs", 2603);
				}
				instance.Free();
				instance2.Free();
				return true;
				static bool? equalsEasyOut(SequenceTests sequence, object? obj2)
				{
					if (sequence == obj2)
					{
						return true;
					}
					if (!(obj2 is SequenceTests sequenceTests2) || sequence.GetType() != sequenceTests2.GetType() || sequence.RemainingTests.Length != sequenceTests2.RemainingTests.Length)
					{
						return false;
					}
					if (!sequence.RemainingTests.Any((Tests t) => t is SequenceTests))
					{
						return sequence.RemainingTests.SequenceEqual(sequenceTests2.RemainingTests);
					}
					return null;
				}
			}

			public sealed override int GetHashCode()
			{
				int? num = getHashCodeEasyOut(this);
				if (num.HasValue)
				{
					return num.GetValueOrDefault();
				}
				int num2 = Hash.Combine(RemainingTests.Length, GetType().GetHashCode());
				ArrayBuilder<Tests> instance = ArrayBuilder<Tests>.GetInstance();
				instance.AddRange(RemainingTests);
				do
				{
					Tests tests = instance.Pop();
					if (tests is SequenceTests sequenceTests)
					{
						num = getHashCodeEasyOut(sequenceTests);
						if (num.HasValue)
						{
							num2 = Hash.Combine(num.GetValueOrDefault(), num2);
							continue;
						}
						num2 = Hash.Combine(Hash.Combine(sequenceTests.RemainingTests.Length, sequenceTests.GetType().GetHashCode()), num2);
						instance.AddRange(sequenceTests.RemainingTests);
					}
					else
					{
						num2 = Hash.Combine(tests.GetHashCode(), num2);
					}
				}
				while (instance.Count != 0);
				instance.Free();
				return num2;
				static int? getHashCodeEasyOut(SequenceTests sequence)
				{
					if (sequence.RemainingTests.Any((Tests t) => t is SequenceTests))
					{
						return null;
					}
					int currentKey = Hash.Combine(sequence.RemainingTests.Length, sequence.GetType().GetHashCode());
					return Hash.Combine(Hash.CombineValues(sequence.RemainingTests), currentKey);
				}
			}

			public sealed override BoundDagTest ComputeSelectedTest()
			{
				SequenceTests sequenceTests = this;
				Tests tests;
				while (true)
				{
					BoundDagTest boundDagTest = sequenceTests.ComputeSelectedTestEasyOut();
					if (boundDagTest != null)
					{
						return boundDagTest;
					}
					tests = sequenceTests.RemainingTests[0];
					if (!(tests is SequenceTests sequenceTests2))
					{
						break;
					}
					sequenceTests = sequenceTests2;
				}
				return tests.ComputeSelectedTest();
			}

			protected virtual BoundDagTest? ComputeSelectedTestEasyOut()
			{
				return null;
			}
		}

		public sealed class AndSequence : SequenceTests
		{
			private AndSequence(ImmutableArray<Tests> remainingTests)
				: base(remainingTests)
			{
			}

			public override Tests Update(ArrayBuilder<Tests> remainingTests)
			{
				return Create(remainingTests);
			}

			public static Tests Create(Tests t1, Tests t2)
			{
				if (t1 is True)
				{
					return t2;
				}
				if (t1 is False)
				{
					return t1;
				}
				ArrayBuilder<Tests> instance = ArrayBuilder<Tests>.GetInstance(2);
				instance.Add(t1);
				instance.Add(t2);
				return Create(instance);
			}

			public static Tests Create(ArrayBuilder<Tests> remainingTests)
			{
				for (int num = remainingTests.Count - 1; num >= 0; num--)
				{
					Tests tests = remainingTests[num];
					if (!(tests is True))
					{
						if (tests is False result)
						{
							remainingTests.Free();
							return result;
						}
						if (tests is AndSequence andSequence)
						{
							ImmutableArray<Tests> remainingTests2 = andSequence.RemainingTests;
							remainingTests.RemoveAt(num);
							int i = 0;
							for (int length = remainingTests2.Length; i < length; i++)
							{
								remainingTests.Insert(num + i, remainingTests2[i]);
							}
						}
					}
					else
					{
						remainingTests.RemoveAt(num);
					}
				}
				object result2 = remainingTests.Count switch
				{
					0 => True.Instance, 
					1 => remainingTests[0], 
					_ => new AndSequence(remainingTests.ToImmutable()), 
				};
				remainingTests.Free();
				return (Tests)result2;
			}

			protected override BoundDagTest? ComputeSelectedTestEasyOut()
			{
				if (RemainingTests[0] is One one)
				{
					BoundDagTest test = one.Test;
					if (test != null && test.Kind == BoundKind.DagNonNullTest && RemainingTests[1] is One one2)
					{
						BoundDagTest test2 = one2.Test;
						if (test2 != null)
						{
							switch (test2.Kind)
							{
							case BoundKind.DagTypeTest:
								if (test.Input != test2.Input)
								{
									return test;
								}
								return test2;
							case BoundKind.DagValueTest:
							{
								BoundDagTest boundDagTest = test2;
								if (test.Input != boundDagTest.Input)
								{
									return test;
								}
								return boundDagTest;
							}
							}
						}
					}
				}
				return null;
			}

			public override string Dump(Func<BoundDagTest, string> dump)
			{
				return "AND(" + string.Join(", ", RemainingTests.Select((Tests t) => t.Dump(dump))) + ")";
			}
		}

		public sealed class OrSequence : SequenceTests
		{
			private OrSequence(ImmutableArray<Tests> remainingTests)
				: base(remainingTests)
			{
			}

			public override Tests Update(ArrayBuilder<Tests> remainingTests)
			{
				return Create(remainingTests);
			}

			public static Tests Create(Tests t1, Tests t2)
			{
				if (t1 is True)
				{
					return t1;
				}
				if (t1 is False)
				{
					return t2;
				}
				ArrayBuilder<Tests> instance = ArrayBuilder<Tests>.GetInstance(2);
				instance.Add(t1);
				instance.Add(t2);
				return Create(instance);
			}

			public static Tests Create(ArrayBuilder<Tests> remainingTests)
			{
				for (int num = remainingTests.Count - 1; num >= 0; num--)
				{
					Tests tests = remainingTests[num];
					if (!(tests is False))
					{
						if (tests is True result)
						{
							remainingTests.Free();
							return result;
						}
						if (tests is OrSequence orSequence)
						{
							remainingTests.RemoveAt(num);
							ImmutableArray<Tests> remainingTests2 = orSequence.RemainingTests;
							int i = 0;
							for (int length = remainingTests2.Length; i < length; i++)
							{
								remainingTests.Insert(num + i, remainingTests2[i]);
							}
						}
					}
					else
					{
						remainingTests.RemoveAt(num);
					}
				}
				object result2 = remainingTests.Count switch
				{
					0 => False.Instance, 
					1 => remainingTests[0], 
					_ => new OrSequence(remainingTests.ToImmutable()), 
				};
				remainingTests.Free();
				return (Tests)result2;
			}

			public override string Dump(Func<BoundDagTest, string> dump)
			{
				return "OR(" + string.Join(", ", RemainingTests.Select((Tests t) => t.Dump(dump))) + ")";
			}
		}

		private Tests()
		{
		}

		public abstract void Filter(DecisionDagBuilder builder, BoundDagTest test, DagState state, IValueSet? whenTrueValues, IValueSet? whenFalseValues, out Tests whenTrue, out Tests whenFalse, ref bool foundExplicitNullTest);

		public virtual BoundDagTest ComputeSelectedTest()
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/DecisionDagBuilder.cs", 2120);
		}

		public virtual Tests RemoveEvaluation(BoundDagEvaluation e)
		{
			return this;
		}

		public virtual Tests RewriteNestedLengthTests()
		{
			return this;
		}

		public abstract string Dump(Func<BoundDagTest, string> dump);
	}

	private readonly struct ReachabilityAnalysisContext(ArrayBuilder<StateForCase> previousCases, int patternIndex, DecisionDagBuilder builder, BoundDagTemp rootIdentifier, SyntaxNode syntax, PooledHashSet<SyntaxNode> redundantNodes)
	{
		public readonly ArrayBuilder<StateForCase> PreviousCases = previousCases;

		public readonly int PatternIndex = patternIndex;

		public readonly DecisionDagBuilder Builder = builder;

		public readonly BoundDagTemp RootIdentifier = rootIdentifier;

		public readonly SyntaxNode Syntax = syntax;

		public readonly PooledHashSet<SyntaxNode> RedundantNodes = redundantNodes;
	}

	private class PatternNormalizer : BoundTreeWalkerWithStackGuard
	{
		private struct OperandOrOperation
		{
			private readonly BoundPattern? _operand;

			private readonly bool? _disjunction;

			private readonly SyntaxNode? _operationSyntax;

			[CompilerGenerated]
			private bool? _003COnTheLeftOfDisjunction_003Ek__BackingField;

			public bool? OnTheLeftOfDisjunction
			{
				[CompilerGenerated]
				readonly get
				{
					return _003COnTheLeftOfDisjunction_003Ek__BackingField;
				}
				set
				{
					_003COnTheLeftOfDisjunction_003Ek__BackingField = value;
				}
			}

			private OperandOrOperation(BoundPattern? operand, bool? disjunction, SyntaxNode? operationSyntax)
			{
				_003COnTheLeftOfDisjunction_003Ek__BackingField = null;
				_operand = operand;
				_disjunction = disjunction;
				_operationSyntax = operationSyntax;
			}

			public static OperandOrOperation CreateOperation(bool disjunction, SyntaxNode operationSyntax)
			{
				return new OperandOrOperation(null, disjunction, operationSyntax);
			}

			public static OperandOrOperation CreateOperand(BoundPattern operand)
			{
				return new OperandOrOperation(operand, null, null);
			}

			public bool IsOperand([NotNullWhen(true)] out BoundPattern? operand)
			{
				if (_operand != null)
				{
					operand = _operand;
					return true;
				}
				operand = null;
				return false;
			}

			public bool IsOperation(out bool disjunction, [NotNullWhen(true)] out SyntaxNode? operationSyntax)
			{
				if (_operand == null)
				{
					disjunction = _disjunction == true;
					operationSyntax = _operationSyntax;
					return true;
				}
				disjunction = false;
				operationSyntax = null;
				return false;
			}

			public OperandOrOperation MakeCompilerGenerated()
			{
				OperandOrOperation result = new OperandOrOperation(_operand.MakeCompilerGenerated(), _disjunction, _operationSyntax);
				result.OnTheLeftOfDisjunction = OnTheLeftOfDisjunction;
				return result;
			}
		}

		private bool _negated;

		private bool? _expectingOperandOfDisjunction;

		private Func<BoundPattern, BoundPattern>? _makeEvaluationSequenceOperand;

		private readonly ArrayBuilder<OperandOrOperation> _evalSequence = ArrayBuilder<OperandOrOperation>.GetInstance();

		private PatternNormalizer()
		{
		}

		internal static BoundPattern Rewrite(BoundPattern pattern, TypeSymbol inputType)
		{
			PatternNormalizer patternNormalizer = new PatternNormalizer();
			patternNormalizer.Visit(pattern);
			return patternNormalizer.GetResult(inputType);
		}

		private BoundPattern GetResult(TypeSymbol inputType)
		{
			ArrayBuilder<BoundPattern> instance = ArrayBuilder<BoundPattern>.GetInstance();
			int num = 0;
			do
			{
				OperandOrOperation operandOrOperation = _evalSequence[num];
				if (operandOrOperation.IsOperation(out bool disjunction, out SyntaxNode operationSyntax))
				{
					BoundPattern boundPattern = instance.Pop();
					BoundPattern boundPattern2 = instance.Pop();
					TypeSymbol narrowedType = narrowedTypeForBinary(boundPattern2, boundPattern, disjunction);
					instance.Push(new BoundBinaryPattern(operationSyntax, disjunction, boundPattern2, boundPattern, boundPattern2.InputType, narrowedType));
				}
				else
				{
					if (!operandOrOperation.IsOperand(out BoundPattern operand))
					{
						throw ExceptionUtilities.UnexpectedValue(operandOrOperation);
					}
					instance.Push(WithInputTypeCheckIfNeeded(operand, inputType));
				}
				bool? onTheLeftOfDisjunction = operandOrOperation.OnTheLeftOfDisjunction;
				if (onTheLeftOfDisjunction.HasValue)
				{
					inputType = ((onTheLeftOfDisjunction != true) ? instance.Peek().NarrowedType : instance.Peek().InputType);
				}
				num++;
			}
			while (num < _evalSequence.Count);
			BoundPattern result = instance.Single();
			instance.Free();
			_evalSequence.Free();
			return result;
			static TypeSymbol narrowedTypeForBinary(BoundPattern resultLeft, BoundPattern resultRight, bool resultDisjunction)
			{
				if (resultDisjunction)
				{
					if (resultRight.NarrowedType.Equals(resultLeft.NarrowedType, TypeCompareKind.AllIgnoreOptions))
					{
						return resultLeft.NarrowedType;
					}
					return resultLeft.InputType;
				}
				return resultRight.NarrowedType;
			}
		}

		public override BoundNode? Visit(BoundNode? node)
		{
			return base.Visit(node);
		}

		public override BoundNode? VisitBinaryPattern(BoundBinaryPattern node)
		{
			bool flag = node.Disjunction;
			if (_negated)
			{
				flag = !flag;
			}
			ArrayBuilder<BoundBinaryPattern> instance = ArrayBuilder<BoundBinaryPattern>.GetInstance();
			BoundBinaryPattern boundBinaryPattern = node;
			do
			{
				instance.Push(boundBinaryPattern);
				boundBinaryPattern = boundBinaryPattern.Left as BoundBinaryPattern;
			}
			while (boundBinaryPattern != null && boundBinaryPattern.Disjunction == node.Disjunction);
			bool? expectingOperandOfDisjunction = _expectingOperandOfDisjunction;
			_expectingOperandOfDisjunction = flag;
			boundBinaryPattern = instance.Pop();
			int count = _evalSequence.Count;
			Visit(boundBinaryPattern.Left);
			int num = _evalSequence.Count - 1;
			do
			{
				if (num < count && instance.IsEmpty)
				{
					_expectingOperandOfDisjunction = expectingOperandOfDisjunction;
				}
				int count2 = _evalSequence.Count;
				Visit(boundBinaryPattern.Right);
				int num2 = _evalSequence.Count - 1;
				if (num >= count && count2 <= num2)
				{
					PushBinaryOperation(boundBinaryPattern.Syntax, num, flag);
				}
				num = num2;
			}
			while (instance.TryPop(out boundBinaryPattern));
			instance.Free();
			_expectingOperandOfDisjunction = expectingOperandOfDisjunction;
			return null;
		}

		private void PushBinaryOperation(SyntaxNode syntax, int endOfLeft, bool disjunction)
		{
			OperandOrOperation value = _evalSequence[endOfLeft];
			value.OnTheLeftOfDisjunction = disjunction;
			_evalSequence[endOfLeft] = value;
			_evalSequence.Push(OperandOrOperation.CreateOperation(disjunction, syntax));
		}

		private void TryPushOperand(BoundPattern pattern)
		{
			bool? expectingOperandOfDisjunction = _expectingOperandOfDisjunction;
			if (expectingOperandOfDisjunction.HasValue)
			{
				if (expectingOperandOfDisjunction == true)
				{
					if (pattern is BoundNegatedPattern boundNegatedPattern && boundNegatedPattern.Negated is BoundDiscardPattern)
					{
						return;
					}
				}
				else if (pattern is BoundDiscardPattern)
				{
					return;
				}
			}
			_evalSequence.Push(OperandOrOperation.CreateOperand(_makeEvaluationSequenceOperand?.Invoke(pattern) ?? pattern));
		}

		public override BoundNode? VisitNegatedPattern(BoundNegatedPattern node)
		{
			bool negated = _negated;
			_negated = !_negated;
			Visit(node.Negated);
			_negated = negated;
			return null;
		}

		public BoundPattern NegateIfNeeded(BoundPattern node)
		{
			if (!_negated)
			{
				return node;
			}
			if (node is BoundNegatedPattern boundNegatedPattern)
			{
				return boundNegatedPattern.Negated;
			}
			BoundNegatedPattern boundNegatedPattern2 = new BoundNegatedPattern(node.Syntax, node, node.InputType, node.InputType);
			if (node.WasCompilerGenerated)
			{
				boundNegatedPattern2.MakeCompilerGenerated();
			}
			return boundNegatedPattern2;
		}

		public override BoundNode? VisitTypePattern(BoundTypePattern node)
		{
			TryPushOperand(NegateIfNeeded(node));
			return null;
		}

		public override BoundNode? VisitConstantPattern(BoundConstantPattern node)
		{
			TryPushOperand(NegateIfNeeded(node));
			return null;
		}

		public override BoundNode? VisitDiscardPattern(BoundDiscardPattern node)
		{
			TryPushOperand(NegateIfNeeded(node));
			return null;
		}

		public override BoundNode? VisitRelationalPattern(BoundRelationalPattern node)
		{
			TryPushOperand(NegateIfNeeded(node));
			return null;
		}

		private static BoundPattern WithInputTypeCheckIfNeeded(BoundPattern pattern, TypeSymbol inputType)
		{
			if (pattern.InputType.Equals(inputType, TypeCompareKind.AllIgnoreOptions))
			{
				return pattern;
			}
			if (pattern is BoundTypePattern boundTypePattern)
			{
				return boundTypePattern.Update(boundTypePattern.DeclaredType, boundTypePattern.IsExplicitNotNullTest, inputType, boundTypePattern.NarrowedType);
			}
			if (pattern is BoundRecursivePattern boundRecursivePattern)
			{
				return boundRecursivePattern.Update(boundRecursivePattern.DeclaredType ?? new BoundTypeExpression(boundRecursivePattern.Syntax, null, boundRecursivePattern.InputType.StrippedType()), boundRecursivePattern.DeconstructMethod, boundRecursivePattern.Deconstruction, boundRecursivePattern.Properties, boundRecursivePattern.IsExplicitNotNullTest, boundRecursivePattern.Variable, boundRecursivePattern.VariableAccess, inputType, boundRecursivePattern.NarrowedType);
			}
			if (pattern is BoundDiscardPattern boundDiscardPattern)
			{
				return boundDiscardPattern.Update(inputType, inputType);
			}
			if (pattern is BoundNegatedPattern boundNegatedPattern)
			{
				return boundNegatedPattern.Update(WithInputTypeCheckIfNeeded(boundNegatedPattern.Negated, inputType), inputType, inputType);
			}
			if (pattern is BoundConstantPattern boundConstantPattern)
			{
				TypeSymbol narrowedType = (boundConstantPattern.ConstantValue.IsNull ? inputType : boundConstantPattern.NarrowedType);
				return boundConstantPattern.Update(boundConstantPattern.Value, boundConstantPattern.ConstantValue, inputType, narrowedType);
			}
			if (pattern is BoundRelationalPattern boundRelationalPattern)
			{
				return boundRelationalPattern.Update(boundRelationalPattern.Relation, boundRelationalPattern.Value, boundRelationalPattern.ConstantValue, inputType, boundRelationalPattern.NarrowedType);
			}
			if (pattern is BoundDeclarationPattern boundDeclarationPattern)
			{
				return boundDeclarationPattern.Update(boundDeclarationPattern.DeclaredType, boundDeclarationPattern.IsVar, null, null, inputType, boundDeclarationPattern.NarrowedType);
			}
			BoundPattern left = new BoundTypePattern(pattern.Syntax, new BoundTypeExpression(pattern.Syntax, null, pattern.InputType), isExplicitNotNullTest: false, inputType, pattern.InputType).MakeCompilerGenerated();
			BoundBinaryPattern boundBinaryPattern = new BoundBinaryPattern(pattern.Syntax, disjunction: false, left, pattern, inputType, pattern.NarrowedType);
			if (pattern.WasCompilerGenerated)
			{
				boundBinaryPattern = boundBinaryPattern.MakeCompilerGenerated();
			}
			return boundBinaryPattern;
		}

		public override BoundNode? VisitDeclarationPattern(BoundDeclarationPattern node)
		{
			BoundDeclarationPattern node2 = new BoundDeclarationPattern(node.Syntax, node.DeclaredType, node.IsVar, node.Variable, node.VariableAccess, node.InputType, node.NarrowedType).MakeCompilerGenerated();
			TryPushOperand(NegateIfNeeded(node2));
			return null;
		}

		public override BoundNode? VisitRecursivePattern(BoundRecursivePattern node)
		{
			bool? expectingOperandOfDisjunction = _expectingOperandOfDisjunction;
			int count = _evalSequence.Count;
			_expectingOperandOfDisjunction = _negated;
			BoundPattern node2;
			if (node.DeclaredType != null)
			{
				node2 = new BoundTypePattern(node.Syntax, node.DeclaredType, node.IsExplicitNotNullTest, node.InputType, node.NarrowedType, node.HasErrors);
			}
			else if (node.InputType.CanContainNull())
			{
				BoundConstantPattern negated = new BoundConstantPattern(node.Syntax, new BoundLiteral(node.Syntax, ConstantValue.Null, node.InputType, hasErrors: false), ConstantValue.Null, node.InputType, node.InputType);
				node2 = new BoundNegatedPattern(node.Syntax, negated, node.InputType, node.InputType);
			}
			else
			{
				node2 = new BoundRecursivePattern(node.Syntax, null, null, default(ImmutableArray<BoundPositionalSubpattern>), ImmutableArray<BoundPropertySubpattern>.Empty, isExplicitNotNullTest: false, null, null, node.InputType, node.InputType);
			}
			TryPushOperand(NegateIfNeeded(node2));
			int count2 = _evalSequence.Count;
			ImmutableArray<BoundPositionalSubpattern> deconstruction = node.Deconstruction;
			if (!deconstruction.IsDefaultOrEmpty)
			{
				ImmutableArray<BoundPositionalSubpattern> discards = deconstruction.SelectAsArray((BoundPositionalSubpattern d) => d.WithPattern(MakeDiscardPattern(d.Syntax, d.Pattern.InputType)));
				Func<BoundPattern, BoundPattern> saveMakeEvaluationSequenceOperand = _makeEvaluationSequenceOperand;
				int i = 0;
				_makeEvaluationSequenceOperand = delegate(BoundPattern newPattern)
				{
					bool wasCompilerGenerated = newPattern.WasCompilerGenerated;
					newPattern = WithInputTypeCheckIfNeeded(newPattern, deconstruction[i].Pattern.InputType);
					BoundPattern boundPattern = new BoundRecursivePattern(deconstruction: discards.SetItem(i, deconstruction[i].WithPattern(newPattern)), syntax: newPattern.Syntax, declaredType: node.DeclaredType, deconstructMethod: node.DeconstructMethod, properties: default(ImmutableArray<BoundPropertySubpattern>), isExplicitNotNullTest: false, variable: null, variableAccess: null, inputType: node.InputType, narrowedType: node.NarrowedType, hasErrors: node.HasErrors);
					if (wasCompilerGenerated)
					{
						boundPattern = boundPattern.MakeCompilerGenerated();
					}
					return saveMakeEvaluationSequenceOperand?.Invoke(boundPattern) ?? boundPattern;
				};
				for (; i < deconstruction.Length; i++)
				{
					VisitPatternAndCombine(node.Syntax, deconstruction[i].Pattern, count);
				}
				_makeEvaluationSequenceOperand = saveMakeEvaluationSequenceOperand;
			}
			if (!node.Properties.IsDefaultOrEmpty)
			{
				Func<BoundPattern, BoundPattern> saveMakeEvaluationSequenceOperand2 = _makeEvaluationSequenceOperand;
				BoundPropertySubpattern property = null;
				_makeEvaluationSequenceOperand = delegate(BoundPattern newPattern)
				{
					bool wasCompilerGenerated = newPattern.WasCompilerGenerated;
					newPattern = WithInputTypeCheckIfNeeded(newPattern, property.Pattern.InputType);
					ImmutableArray<BoundPropertySubpattern> properties = ImmutableCollectionsMarshal.AsImmutableArray(new BoundPropertySubpattern[1] { property.WithPattern(newPattern) });
					BoundPattern boundPattern = new BoundRecursivePattern(newPattern.Syntax, node.DeclaredType, null, default(ImmutableArray<BoundPositionalSubpattern>), properties, isExplicitNotNullTest: false, null, null, node.InputType, node.NarrowedType, node.HasErrors);
					if (wasCompilerGenerated)
					{
						boundPattern = boundPattern.MakeCompilerGenerated();
					}
					return saveMakeEvaluationSequenceOperand2?.Invoke(boundPattern) ?? boundPattern;
				};
				foreach (BoundPropertySubpattern property2 in node.Properties)
				{
					property = property2;
					VisitPatternAndCombine(node.Syntax, property.Pattern, count);
				}
				_makeEvaluationSequenceOperand = saveMakeEvaluationSequenceOperand2;
			}
			_expectingOperandOfDisjunction = expectingOperandOfDisjunction;
			if (_evalSequence.Count > count2 || (object)node.Variable != null)
			{
				_evalSequence[count] = _evalSequence[count].MakeCompilerGenerated();
			}
			return null;
		}

		private void VisitPatternAndCombine(SyntaxNode syntax, BoundPattern pattern, int startOfLeft)
		{
			int num = _evalSequence.Count - 1;
			int count = _evalSequence.Count;
			Visit(pattern);
			int num2 = _evalSequence.Count - 1;
			if (num >= startOfLeft && num2 >= count)
			{
				PushBinaryOperation(syntax, num, _negated);
			}
		}

		public override BoundNode? VisitPropertySubpattern(BoundPropertySubpattern node)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/DecisionDagBuilder_CheckOrReachability.cs", 1206);
		}

		public override BoundNode? VisitPositionalSubpattern(BoundPositionalSubpattern node)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/DecisionDagBuilder_CheckOrReachability.cs", 1211);
		}

		public override BoundNode? VisitITuplePattern(BoundITuplePattern ituplePattern)
		{
			bool? expectingOperandOfDisjunction = _expectingOperandOfDisjunction;
			int count = _evalSequence.Count;
			ImmutableArray<BoundPositionalSubpattern> subpatterns = ituplePattern.Subpatterns;
			ImmutableArray<BoundPositionalSubpattern> discards = subpatterns.SelectAsArray((BoundPositionalSubpattern d) => d.WithPattern(MakeDiscardPattern(d.Syntax, d.Pattern.InputType)));
			_expectingOperandOfDisjunction = _negated;
			BoundITuplePattern node = new BoundITuplePattern(ituplePattern.Syntax, ituplePattern.GetLengthMethod, ituplePattern.GetItemMethod, discards, ituplePattern.InputType, ituplePattern.NarrowedType);
			TryPushOperand(NegateIfNeeded(node));
			int count2 = _evalSequence.Count;
			Func<BoundPattern, BoundPattern> saveMakeEvaluationSequenceOperand = _makeEvaluationSequenceOperand;
			int i = 0;
			_makeEvaluationSequenceOperand = delegate(BoundPattern newPattern)
			{
				bool wasCompilerGenerated = newPattern.WasCompilerGenerated;
				newPattern = WithInputTypeCheckIfNeeded(newPattern, subpatterns[i].Pattern.InputType);
				BoundPattern boundPattern = new BoundITuplePattern(subpatterns: discards.SetItem(i, subpatterns[i].WithPattern(newPattern)), syntax: newPattern.Syntax, getLengthMethod: ituplePattern.GetLengthMethod, getItemMethod: ituplePattern.GetItemMethod, inputType: ituplePattern.InputType, narrowedType: ituplePattern.NarrowedType);
				if (wasCompilerGenerated)
				{
					boundPattern = boundPattern.MakeCompilerGenerated();
				}
				return saveMakeEvaluationSequenceOperand?.Invoke(boundPattern) ?? boundPattern;
			};
			for (; i < subpatterns.Length; i++)
			{
				VisitPatternAndCombine(ituplePattern.Syntax, subpatterns[i].Pattern, count);
			}
			_makeEvaluationSequenceOperand = saveMakeEvaluationSequenceOperand;
			_expectingOperandOfDisjunction = expectingOperandOfDisjunction;
			if (_evalSequence.Count > count2)
			{
				_evalSequence[count] = _evalSequence[count].MakeCompilerGenerated();
			}
			return null;
		}

		public override BoundNode? VisitListPattern(BoundListPattern listPattern)
		{
			bool? expectingOperandOfDisjunction = _expectingOperandOfDisjunction;
			int count = _evalSequence.Count;
			_expectingOperandOfDisjunction = _negated;
			ImmutableArray<BoundPattern> equivalentDefaultPatterns = listPattern.Subpatterns.SelectAsArray(makeEquivalentDefaultPattern);
			BoundListPattern node = listPattern.WithSubpatterns(equivalentDefaultPatterns);
			TryPushOperand(NegateIfNeeded(node));
			int count2 = _evalSequence.Count;
			Func<BoundPattern, BoundPattern> saveMakeEvaluationSequenceOperand = _makeEvaluationSequenceOperand;
			int i = 0;
			bool hasSlice = listPattern.HasSlice;
			Func<BoundPattern, BoundPattern> makeEvaluationSequenceOperand = delegate(BoundPattern newPattern)
			{
				bool wasCompilerGenerated = newPattern.WasCompilerGenerated;
				newPattern = WithInputTypeCheckIfNeeded(newPattern, equivalentDefaultPatterns[i].InputType);
				BoundPattern boundPattern = new BoundListPattern(subpatterns: equivalentDefaultPatterns.SetItem(i, newPattern), syntax: newPattern.Syntax, hasSlice: hasSlice, lengthAccess: listPattern.LengthAccess, indexerAccess: listPattern.IndexerAccess, receiverPlaceholder: listPattern.ReceiverPlaceholder, argumentPlaceholder: listPattern.ArgumentPlaceholder, variable: listPattern.Variable, variableAccess: listPattern.VariableAccess, inputType: listPattern.InputType, narrowedType: listPattern.NarrowedType);
				if (wasCompilerGenerated)
				{
					boundPattern = boundPattern.MakeCompilerGenerated();
				}
				return saveMakeEvaluationSequenceOperand?.Invoke(boundPattern) ?? boundPattern;
			};
			Func<BoundPattern, BoundPattern> makeEvaluationSequenceOperand2 = null;
			if (hasSlice)
			{
				makeEvaluationSequenceOperand2 = delegate(BoundPattern newPattern)
				{
					bool wasCompilerGenerated = newPattern.WasCompilerGenerated;
					BoundSlicePattern boundSlicePattern2 = (BoundSlicePattern)listPattern.Subpatterns[i];
					newPattern = WithInputTypeCheckIfNeeded(newPattern, boundSlicePattern2.Pattern.InputType);
					BoundPattern item = new BoundSlicePattern(newPattern.Syntax, newPattern, boundSlicePattern2.IndexerAccess, boundSlicePattern2.ReceiverPlaceholder, boundSlicePattern2.ArgumentPlaceholder, boundSlicePattern2.InputType, boundSlicePattern2.NarrowedType);
					BoundPattern boundPattern = new BoundListPattern(subpatterns: equivalentDefaultPatterns.SetItem(i, item), syntax: newPattern.Syntax, hasSlice: true, lengthAccess: listPattern.LengthAccess, indexerAccess: listPattern.IndexerAccess, receiverPlaceholder: listPattern.ReceiverPlaceholder, argumentPlaceholder: listPattern.ArgumentPlaceholder, variable: listPattern.Variable, variableAccess: listPattern.VariableAccess, inputType: listPattern.InputType, narrowedType: listPattern.NarrowedType);
					if (wasCompilerGenerated)
					{
						boundPattern = boundPattern.MakeCompilerGenerated();
					}
					return saveMakeEvaluationSequenceOperand?.Invoke(boundPattern) ?? boundPattern;
				};
			}
			for (; i < equivalentDefaultPatterns.Length; i++)
			{
				if (listPattern.Subpatterns[i] is BoundSlicePattern boundSlicePattern)
				{
					if (boundSlicePattern.Pattern != null)
					{
						_makeEvaluationSequenceOperand = makeEvaluationSequenceOperand2;
						VisitPatternAndCombine(listPattern.Syntax, boundSlicePattern.Pattern, count);
					}
				}
				else
				{
					_makeEvaluationSequenceOperand = makeEvaluationSequenceOperand;
					VisitPatternAndCombine(listPattern.Syntax, listPattern.Subpatterns[i], count);
				}
			}
			_makeEvaluationSequenceOperand = saveMakeEvaluationSequenceOperand;
			_expectingOperandOfDisjunction = expectingOperandOfDisjunction;
			if (_evalSequence.Count > count2)
			{
				_evalSequence[count] = _evalSequence[count].MakeCompilerGenerated();
			}
			return null;
			static BoundPattern makeEquivalentDefaultPattern(BoundPattern pattern)
			{
				if (pattern is BoundSlicePattern boundSlicePattern2)
				{
					return boundSlicePattern2.WithPattern(null);
				}
				return MakeDiscardPattern(pattern.Syntax, pattern.InputType);
			}
		}

		public override BoundNode VisitSlicePattern(BoundSlicePattern node)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/DecisionDagBuilder_CheckOrReachability.cs", 1394);
		}

		private static BoundDiscardPattern MakeDiscardPattern(SyntaxNode syntax, TypeSymbol inputType)
		{
			return new BoundDiscardPattern(syntax, inputType, inputType);
		}
	}

	private static readonly ObjectPool<PooledDictionary<DagState, DagState>> s_uniqueStatePool = PooledDictionary<DagState, DagState>.CreatePool(DagStateEquivalence.Instance);

	private readonly CSharpCompilation _compilation;

	private readonly Conversions _conversions;

	private readonly BindingDiagnosticBag _diagnostics;

	private readonly LabelSymbol _defaultLabel;

	private readonly bool _forLowering;

	private DecisionDagBuilder(CSharpCompilation compilation, LabelSymbol defaultLabel, bool forLowering, BindingDiagnosticBag diagnostics)
	{
		_compilation = compilation;
		_conversions = compilation.Conversions;
		_diagnostics = diagnostics;
		_defaultLabel = defaultLabel;
		_forLowering = forLowering;
	}

	public static BoundDecisionDag CreateDecisionDagForSwitchStatement(CSharpCompilation compilation, SyntaxNode syntax, BoundExpression switchGoverningExpression, ImmutableArray<BoundSwitchSection> switchSections, LabelSymbol defaultLabel, BindingDiagnosticBag diagnostics, bool forLowering = false)
	{
		return new DecisionDagBuilder(compilation, defaultLabel, forLowering, diagnostics).CreateDecisionDagForSwitchStatement(syntax, switchGoverningExpression, switchSections);
	}

	public static BoundDecisionDag CreateDecisionDagForSwitchExpression(CSharpCompilation compilation, SyntaxNode syntax, BoundExpression switchExpressionInput, ImmutableArray<BoundSwitchExpressionArm> switchArms, LabelSymbol defaultLabel, BindingDiagnosticBag diagnostics, bool forLowering = false)
	{
		return new DecisionDagBuilder(compilation, defaultLabel, forLowering, diagnostics).CreateDecisionDagForSwitchExpression(syntax, switchExpressionInput, switchArms);
	}

	public static BoundDecisionDag CreateDecisionDagForIsPattern(CSharpCompilation compilation, SyntaxNode syntax, BoundExpression inputExpression, BoundPattern pattern, LabelSymbol whenTrueLabel, LabelSymbol whenFalseLabel, BindingDiagnosticBag diagnostics, bool forLowering = false)
	{
		return new DecisionDagBuilder(compilation, whenFalseLabel, forLowering, diagnostics).CreateDecisionDagForIsPattern(syntax, inputExpression, pattern, whenTrueLabel);
	}

	private BoundDecisionDag CreateDecisionDagForIsPattern(SyntaxNode syntax, BoundExpression inputExpression, BoundPattern pattern, LabelSymbol whenTrueLabel)
	{
		BoundDagTemp input = BoundDagTemp.ForOriginalInput(inputExpression);
		using TemporaryArray<StateForCase> array = TemporaryArray<StateForCase>.Empty;
		array.Add(MakeTestsForPattern(1, pattern.Syntax, input, pattern, null, whenTrueLabel));
		return MakeBoundDecisionDag(syntax, ref TemporaryArrayExtensions.AsRef(in array));
	}

	private BoundDecisionDag CreateDecisionDagForSwitchStatement(SyntaxNode syntax, BoundExpression switchGoverningExpression, ImmutableArray<BoundSwitchSection> switchSections)
	{
		BoundDagTemp input = BoundDagTemp.ForOriginalInput(switchGoverningExpression);
		int num = 0;
		using TemporaryArray<StateForCase> array = TemporaryArray<StateForCase>.GetInstance(switchSections.Length);
		foreach (BoundSwitchSection item in switchSections)
		{
			foreach (BoundSwitchLabel switchLabel in item.SwitchLabels)
			{
				if (switchLabel.Syntax.Kind() != SyntaxKind.DefaultSwitchLabel)
				{
					array.Add(MakeTestsForPattern(++num, switchLabel.Syntax, input, switchLabel.Pattern, switchLabel.WhenClause, switchLabel.Label));
				}
			}
		}
		return MakeBoundDecisionDag(syntax, ref TemporaryArrayExtensions.AsRef(in array));
	}

	private BoundDecisionDag CreateDecisionDagForSwitchExpression(SyntaxNode syntax, BoundExpression switchExpressionInput, ImmutableArray<BoundSwitchExpressionArm> switchArms)
	{
		BoundDagTemp input = BoundDagTemp.ForOriginalInput(switchExpressionInput);
		int num = 0;
		using TemporaryArray<StateForCase> array = TemporaryArray<StateForCase>.GetInstance(switchArms.Length);
		foreach (BoundSwitchExpressionArm item in switchArms)
		{
			array.Add(MakeTestsForPattern(++num, item.Syntax, input, item.Pattern, item.WhenClause, item.Label));
		}
		return MakeBoundDecisionDag(syntax, ref TemporaryArrayExtensions.AsRef(in array));
	}

	private StateForCase MakeTestsForPattern(int index, SyntaxNode syntax, BoundDagTemp input, BoundPattern pattern, BoundExpression? whenClause, LabelSymbol label)
	{
		Tests remainingTests = MakeAndSimplifyTestsAndBindings(input, pattern, out var bindings);
		return new StateForCase(index, syntax, remainingTests, bindings, whenClause, label);
	}

	private Tests MakeAndSimplifyTestsAndBindings(BoundDagTemp input, BoundPattern pattern, out ImmutableArray<BoundPatternBinding> bindings)
	{
		ArrayBuilder<BoundPatternBinding> instance = ArrayBuilder<BoundPatternBinding>.GetInstance();
		Tests result = SimplifyTestsAndBindings(MakeTestsAndBindings(input, pattern, instance), instance);
		bindings = instance.ToImmutableAndFree();
		return result;
	}

	private static Tests SimplifyTestsAndBindings(Tests tests, ArrayBuilder<BoundPatternBinding> bindingsBuilder)
	{
		PooledHashSet<BoundDagEvaluation> instance = PooledHashSet<BoundDagEvaluation>.GetInstance();
		foreach (BoundPatternBinding item in bindingsBuilder)
		{
			BoundDagTemp tempContainingValue = item.TempContainingValue;
			if (tempContainingValue.Source != null)
			{
				instance.Add(tempContainingValue.Source);
			}
		}
		ArrayBuilder<Tests> instance2 = ArrayBuilder<Tests>.GetInstance();
		ArrayBuilder<Tests> instance3 = ArrayBuilder<Tests>.GetInstance();
		ArrayBuilder<Tests> instance4 = ArrayBuilder<Tests>.GetInstance();
		instance2.Push(tests);
		do
		{
			Tests tests2 = instance2.Pop();
			if (!(tests2 is Tests.SequenceTests sequenceTests))
			{
				if (!(tests2 is Tests.True) && !(tests2 is Tests.False))
				{
					if (tests2 is Tests.One one)
					{
						one.Deconstruct(out BoundDagTest Test);
						if (!(Test is BoundDagEvaluation boundDagEvaluation))
						{
							if (Test == null)
							{
								goto IL_020f;
							}
							if (Test.Input.Source != null)
							{
								instance.Add(Test.Input.Source);
							}
							instance4.Push(tests2);
						}
						else if (instance.Contains(boundDagEvaluation))
						{
							if (boundDagEvaluation.Input.Source != null)
							{
								instance.Add(boundDagEvaluation.Input.Source);
							}
							instance4.Push(tests2);
						}
						else
						{
							instance4.Push(Tests.True.Instance);
						}
					}
					else if (!(tests2 is Tests.Not not))
					{
						if (tests2 != null)
						{
							goto IL_020f;
						}
						Tests tests3 = instance3.Pop();
						if (!(tests3 is Tests.SequenceTests sequenceTests2))
						{
							if (tests3 is Tests.Not)
							{
								instance4.Push(Tests.Not.Create(instance4.Pop()));
								continue;
							}
							throw ExceptionUtilities.UnexpectedValue(tests3);
						}
						int length = sequenceTests2.RemainingTests.Length;
						ArrayBuilder<Tests> instance5 = ArrayBuilder<Tests>.GetInstance(length);
						for (int i = 0; i < length; i++)
						{
							instance5.Add(instance4.Pop());
						}
						instance4.Push(sequenceTests2.Update(instance5));
					}
					else
					{
						instance3.Push(not);
						instance2.Push(null);
						instance2.Push(not.Negated);
					}
				}
				else
				{
					instance4.Push(tests2);
				}
			}
			else
			{
				instance3.Push(sequenceTests);
				instance2.Push(null);
				instance2.AddRange(sequenceTests.RemainingTests);
			}
			continue;
			IL_020f:
			throw ExceptionUtilities.UnexpectedValue(tests2);
		}
		while (instance2.Count != 0);
		Tests result = instance4.Pop();
		if (!instance4.IsEmpty || !instance3.IsEmpty)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/DecisionDagBuilder.cs", 303);
		}
		instance2.Free();
		instance3.Free();
		instance4.Free();
		instance.Free();
		return result;
	}

	private Tests MakeTestsAndBindings(BoundDagTemp input, BoundPattern pattern, ArrayBuilder<BoundPatternBinding> bindings)
	{
		BoundDagTemp output;
		return MakeTestsAndBindings(input, pattern, out output, bindings);
	}

	private Tests MakeTestsAndBindings(BoundDagTemp input, BoundPattern pattern, out BoundDagTemp output, ArrayBuilder<BoundPatternBinding> bindings)
	{
		if (!(pattern is BoundDeclarationPattern declaration))
		{
			if (!(pattern is BoundConstantPattern constant))
			{
				if (!(pattern is BoundDiscardPattern) && !(pattern is BoundSlicePattern))
				{
					if (!(pattern is BoundListPattern list))
					{
						if (!(pattern is BoundRecursivePattern recursive))
						{
							if (!(pattern is BoundITuplePattern pattern2))
							{
								if (!(pattern is BoundTypePattern typePattern))
								{
									if (!(pattern is BoundRelationalPattern rel))
									{
										if (!(pattern is BoundNegatedPattern neg))
										{
											if (pattern is BoundBinaryPattern bin)
											{
												return MakeTestsAndBindingsForBinaryPattern(input, bin, out output, bindings);
											}
											throw ExceptionUtilities.UnexpectedValue(pattern.Kind);
										}
										output = input;
										return MakeTestsAndBindingsForNegatedPattern(input, neg, bindings);
									}
									return MakeTestsAndBindingsForRelationalPattern(input, rel, out output);
								}
								return MakeTestsForTypePattern(input, typePattern, out output);
							}
							return MakeTestsAndBindingsForITuplePattern(input, pattern2, out output, bindings);
						}
						return MakeTestsAndBindingsForRecursivePattern(input, recursive, out output, bindings);
					}
					return MakeTestsAndBindingsForListPattern(input, list, out output, bindings);
				}
				output = input;
				return Tests.True.Instance;
			}
			return MakeTestsForConstantPattern(input, constant, out output);
		}
		return MakeTestsAndBindingsForDeclarationPattern(input, declaration, out output, bindings);
	}

	private Tests MakeTestsAndBindingsForITuplePattern(BoundDagTemp input, BoundITuplePattern pattern, out BoundDagTemp output, ArrayBuilder<BoundPatternBinding> bindings)
	{
		SyntaxNode syntax = pattern.Syntax;
		int length = pattern.Subpatterns.Length;
		NamedTypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Object);
		PropertySymbol propertySymbol = (PropertySymbol)pattern.GetLengthMethod.AssociatedSymbol;
		PropertySymbol propertySymbol2 = (PropertySymbol)pattern.GetItemMethod.AssociatedSymbol;
		NamedTypeSymbol containingType = propertySymbol.ContainingType;
		ArrayBuilder<Tests> instance = ArrayBuilder<Tests>.GetInstance(4 + length * 2);
		instance.Add(new Tests.One(new BoundDagTypeTest(syntax, containingType, input)));
		BoundDagTypeEvaluation boundDagTypeEvaluation = new BoundDagTypeEvaluation(syntax, containingType, input);
		instance.Add(new Tests.One(boundDagTypeEvaluation));
		BoundDagTemp input2 = (output = new BoundDagTemp(syntax, containingType, boundDagTypeEvaluation));
		BoundDagPropertyEvaluation boundDagPropertyEvaluation = new BoundDagPropertyEvaluation(syntax, propertySymbol, isLengthOrCount: true, OriginalInput(input2, propertySymbol));
		instance.Add(new Tests.One(boundDagPropertyEvaluation));
		BoundDagTemp input3 = new BoundDagTemp(syntax, _compilation.GetSpecialType(SpecialType.System_Int32), boundDagPropertyEvaluation);
		instance.Add(new Tests.One(new BoundDagValueTest(syntax, ConstantValue.Create(length), input3)));
		BoundDagTemp input4 = OriginalInput(input2, propertySymbol2);
		for (int i = 0; i < length; i++)
		{
			BoundDagIndexEvaluation boundDagIndexEvaluation = new BoundDagIndexEvaluation(syntax, propertySymbol2, i, input4);
			instance.Add(new Tests.One(boundDagIndexEvaluation));
			BoundDagTemp input5 = new BoundDagTemp(syntax, specialType, boundDagIndexEvaluation);
			instance.Add(MakeTestsAndBindings(input5, pattern.Subpatterns[i].Pattern, bindings));
		}
		return Tests.AndSequence.Create(instance);
	}

	private BoundDagTemp OriginalInput(BoundDagTemp input, Symbol symbol)
	{
		while (input.Source is BoundDagTypeEvaluation boundDagTypeEvaluation && isDerivedType(boundDagTypeEvaluation.Input.Type, symbol.ContainingType))
		{
			input = boundDagTypeEvaluation.Input;
		}
		return input;
		bool isDerivedType(TypeSymbol possibleDerived, TypeSymbol possibleBase)
		{
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
			return _conversions.HasIdentityOrImplicitReferenceConversion(possibleDerived, possibleBase, ref useSiteInfo);
		}
	}

	private static BoundDagTemp OriginalInput(BoundDagTemp input)
	{
		while (input.Source is BoundDagTypeEvaluation boundDagTypeEvaluation)
		{
			input = boundDagTypeEvaluation.Input;
		}
		return input;
	}

	private Tests MakeTestsAndBindingsForDeclarationPattern(BoundDagTemp input, BoundDeclarationPattern declaration, out BoundDagTemp output, ArrayBuilder<BoundPatternBinding> bindings)
	{
		TypeSymbol type = declaration.DeclaredType?.Type;
		ArrayBuilder<Tests> instance = ArrayBuilder<Tests>.GetInstance(1);
		if (!declaration.IsVar)
		{
			input = MakeConvertToType(input, declaration.Syntax, type, isExplicitTest: false, instance);
		}
		BoundExpression variableAccess = declaration.VariableAccess;
		if (variableAccess != null)
		{
			bindings.Add(new BoundPatternBinding(variableAccess, input));
		}
		output = input;
		return Tests.AndSequence.Create(instance);
	}

	private Tests MakeTestsForTypePattern(BoundDagTemp input, BoundTypePattern typePattern, out BoundDagTemp output)
	{
		TypeSymbol type = typePattern.DeclaredType.Type;
		ArrayBuilder<Tests> instance = ArrayBuilder<Tests>.GetInstance(4);
		output = MakeConvertToType(input, typePattern.Syntax, type, typePattern.IsExplicitNotNullTest, instance);
		return Tests.AndSequence.Create(instance);
	}

	private static void MakeCheckNotNull(BoundDagTemp input, SyntaxNode syntax, bool isExplicitTest, ArrayBuilder<Tests> tests)
	{
		if (input.Type.CanContainNull() && !(input.Source is BoundDagSliceEvaluation))
		{
			tests.Add(new Tests.One(new BoundDagNonNullTest(syntax, isExplicitTest, input)));
		}
	}

	private BoundDagTemp MakeConvertToType(BoundDagTemp input, SyntaxNode syntax, TypeSymbol type, bool isExplicitTest, ArrayBuilder<Tests> tests)
	{
		MakeCheckNotNull(input, syntax, isExplicitTest, tests);
		if (!input.Type.Equals(type, TypeCompareKind.AllIgnoreOptions))
		{
			TypeSymbol source = input.Type.StrippedType();
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(_diagnostics, _compilation.Assembly);
			Conversion conversion = _conversions.ClassifyBuiltInConversion(source, type, isChecked: false, ref useSiteInfo);
			_diagnostics.Add(syntax, useSiteInfo);
			if (!(input.Type.IsDynamic() ? (type.SpecialType == SpecialType.System_Object) : conversion.IsImplicit))
			{
				tests.Add(new Tests.One(new BoundDagTypeTest(syntax, type, input)));
			}
			BoundDagTypeEvaluation boundDagTypeEvaluation = new BoundDagTypeEvaluation(syntax, type, input);
			input = new BoundDagTemp(syntax, type, boundDagTypeEvaluation);
			tests.Add(new Tests.One(boundDagTypeEvaluation));
		}
		return input;
	}

	private Tests MakeTestsForConstantPattern(BoundDagTemp input, BoundConstantPattern constant, out BoundDagTemp output)
	{
		if (constant.ConstantValue == ConstantValue.Null)
		{
			output = input;
			return new Tests.One(new BoundDagExplicitNullTest(constant.Syntax, input));
		}
		if (constant.ConstantValue.IsString && input.Type.IsSpanOrReadOnlySpanChar())
		{
			output = input;
			return new Tests.One(new BoundDagValueTest(constant.Syntax, constant.ConstantValue, input));
		}
		ArrayBuilder<Tests> instance = ArrayBuilder<Tests>.GetInstance(2);
		TypeSymbol type = constant.Value.Type;
		output = (input = (((object)type != null) ? MakeConvertToType(input, constant.Syntax, type, isExplicitTest: false, instance) : input));
		IValueSetFactory? valueSetFactory = ValueSetFactory.ForInput(input);
		if (valueSetFactory != null && valueSetFactory.Related(BinaryOperatorKind.Equal, constant.ConstantValue).IsEmpty)
		{
			instance.Add(Tests.False.Instance);
		}
		else
		{
			instance.Add(new Tests.One(new BoundDagValueTest(constant.Syntax, constant.ConstantValue, input)));
		}
		return Tests.AndSequence.Create(instance);
	}

	private Tests MakeTestsAndBindingsForRecursivePattern(BoundDagTemp input, BoundRecursivePattern recursive, out BoundDagTemp output, ArrayBuilder<BoundPatternBinding> bindings)
	{
		TypeSymbol typeSymbol = recursive.DeclaredType?.Type ?? input.Type.StrippedType();
		ArrayBuilder<Tests> tests = ArrayBuilder<Tests>.GetInstance(5);
		output = (input = MakeConvertToType(input, recursive.Syntax, typeSymbol, recursive.IsExplicitNotNullTest, tests));
		if (!recursive.Deconstruction.IsDefault)
		{
			if (recursive.DeconstructMethod != null)
			{
				MethodSymbol deconstructMethod = recursive.DeconstructMethod;
				BoundDagDeconstructEvaluation boundDagDeconstructEvaluation = new BoundDagDeconstructEvaluation(recursive.Syntax, deconstructMethod, OriginalInput(input, deconstructMethod));
				tests.Add(new Tests.One(boundDagDeconstructEvaluation));
				int num = (deconstructMethod.IsStatic ? 1 : 0);
				int num2 = Math.Min(deconstructMethod.ParameterCount - num, recursive.Deconstruction.Length);
				for (int i = 0; i < num2; i++)
				{
					BoundPattern pattern = recursive.Deconstruction[i].Pattern;
					BoundDagTemp input2 = new BoundDagTemp(pattern.Syntax, deconstructMethod.Parameters[i + num].Type, boundDagDeconstructEvaluation, i);
					tests.Add(MakeTestsAndBindings(input2, pattern, bindings));
				}
			}
			else if (!Binder.IsZeroElementTupleType(typeSymbol))
			{
				if (typeSymbol.IsTupleType)
				{
					ImmutableArray<FieldSymbol> tupleElements = typeSymbol.TupleElements;
					int num3 = Math.Min(typeSymbol.TupleElementTypesWithAnnotations.Length, recursive.Deconstruction.Length);
					for (int j = 0; j < num3; j++)
					{
						BoundPattern pattern2 = recursive.Deconstruction[j].Pattern;
						SyntaxNode syntax = pattern2.Syntax;
						FieldSymbol fieldSymbol = tupleElements[j];
						BoundDagFieldEvaluation boundDagFieldEvaluation = new BoundDagFieldEvaluation(syntax, fieldSymbol, OriginalInput(input, fieldSymbol));
						tests.Add(new Tests.One(boundDagFieldEvaluation));
						BoundDagTemp input3 = new BoundDagTemp(syntax, fieldSymbol.Type, boundDagFieldEvaluation);
						tests.Add(MakeTestsAndBindings(input3, pattern2, bindings));
					}
				}
				else
				{
					tests.Add(new Tests.One(new BoundDagTypeTest(recursive.Syntax, ErrorType(), input, hasErrors: true)));
				}
			}
		}
		if (!recursive.Properties.IsDefault)
		{
			foreach (BoundPropertySubpattern property in recursive.Properties)
			{
				BoundPattern pattern3 = property.Pattern;
				BoundDagTemp input4 = input;
				if (!tryMakeTestsForSubpatternMember(property.Member, ref input4, property.IsLengthOrCount))
				{
					tests.Add(new Tests.One(new BoundDagTypeTest(recursive.Syntax, ErrorType(), input, hasErrors: true)));
				}
				else
				{
					tests.Add(MakeTestsAndBindings(input4, pattern3, bindings));
				}
			}
		}
		if (recursive.VariableAccess != null)
		{
			bindings.Add(new BoundPatternBinding(recursive.VariableAccess, input));
		}
		return Tests.AndSequence.Create(tests);
		bool tryMakeTestsForSubpatternMember([NotNullWhen(true)] BoundPropertySubpatternMember? member, ref BoundDagTemp reference, bool isLengthOrCount)
		{
			if (member == null)
			{
				return false;
			}
			if (tryMakeTestsForSubpatternMember(member.Receiver, ref reference, isLengthOrCount: false))
			{
				reference = MakeConvertToType(reference, member.Syntax, member.Receiver.Type.StrippedType(), isExplicitTest: false, tests);
			}
			Symbol symbol = member.Symbol;
			BoundDagEvaluation boundDagEvaluation;
			if (!(symbol is PropertySymbol propertySymbol))
			{
				if (!(symbol is FieldSymbol fieldSymbol2))
				{
					return false;
				}
				boundDagEvaluation = new BoundDagFieldEvaluation(member.Syntax, fieldSymbol2, OriginalInput(reference, fieldSymbol2));
			}
			else
			{
				boundDagEvaluation = new BoundDagPropertyEvaluation(member.Syntax, propertySymbol, isLengthOrCount, OriginalInput(reference, propertySymbol));
			}
			tests.Add(new Tests.One(boundDagEvaluation));
			reference = new BoundDagTemp(member.Syntax, member.Type, boundDagEvaluation);
			return true;
		}
	}

	private Tests MakeTestsAndBindingsForNegatedPattern(BoundDagTemp input, BoundNegatedPattern neg, ArrayBuilder<BoundPatternBinding> bindings)
	{
		return Tests.Not.Create(MakeTestsAndBindings(input, neg.Negated, bindings));
	}

	private Tests MakeTestsAndBindingsForBinaryPattern(BoundDagTemp input, BoundBinaryPattern bin, out BoundDagTemp output, ArrayBuilder<BoundPatternBinding> bindings)
	{
		ArrayBuilder<BoundBinaryPattern> instance = ArrayBuilder<BoundBinaryPattern>.GetInstance();
		BoundBinaryPattern boundBinaryPattern = bin;
		do
		{
			instance.Push(boundBinaryPattern);
			boundBinaryPattern = boundBinaryPattern.Left as BoundBinaryPattern;
		}
		while (boundBinaryPattern != null);
		boundBinaryPattern = instance.Pop();
		Tests tests = MakeTestsAndBindings(input, boundBinaryPattern.Left, out output, bindings);
		do
		{
			tests = makeTestsAndBindingsForBinaryPattern(this, tests, output, input, boundBinaryPattern, out output, bindings);
		}
		while (instance.TryPop(out boundBinaryPattern));
		instance.Free();
		return tests;
		static Tests makeTestsAndBindingsForBinaryPattern(DecisionDagBuilder @this, Tests leftTests, BoundDagTemp leftOutput, BoundDagTemp boundDagTemp, BoundBinaryPattern boundBinaryPattern2, out BoundDagTemp reference, ArrayBuilder<BoundPatternBinding> bindings2)
		{
			ArrayBuilder<Tests> instance2 = ArrayBuilder<Tests>.GetInstance(2);
			if (boundBinaryPattern2.Disjunction)
			{
				instance2.Add(leftTests);
				instance2.Add(@this.MakeTestsAndBindings(boundDagTemp, boundBinaryPattern2.Right, bindings2));
				Tests tests2 = Tests.OrSequence.Create(instance2);
				if (boundBinaryPattern2.InputType.Equals(boundBinaryPattern2.NarrowedType))
				{
					reference = boundDagTemp;
					return tests2;
				}
				instance2 = ArrayBuilder<Tests>.GetInstance(2);
				instance2.Add(tests2);
				BoundDagTypeEvaluation boundDagTypeEvaluation = new BoundDagTypeEvaluation(boundBinaryPattern2.Syntax, boundBinaryPattern2.NarrowedType, boundDagTemp);
				reference = new BoundDagTemp(boundBinaryPattern2.Syntax, boundBinaryPattern2.NarrowedType, boundDagTypeEvaluation);
				instance2.Add(new Tests.One(boundDagTypeEvaluation));
				return Tests.AndSequence.Create(instance2);
			}
			instance2.Add(leftTests);
			instance2.Add(@this.MakeTestsAndBindings(leftOutput, boundBinaryPattern2.Right, out BoundDagTemp output2, bindings2));
			reference = output2;
			return Tests.AndSequence.Create(instance2);
		}
	}

	private Tests MakeTestsAndBindingsForRelationalPattern(BoundDagTemp input, BoundRelationalPattern rel, out BoundDagTemp output)
	{
		TypeSymbol type = rel.Value.Type ?? input.Type;
		ArrayBuilder<Tests> instance = ArrayBuilder<Tests>.GetInstance(2);
		output = MakeConvertToType(input, rel.Syntax, type, isExplicitTest: false, instance);
		IValueSet valueSet = ValueSetFactory.ForInput(output)?.Related(rel.Relation.Operator(), rel.ConstantValue);
		if (valueSet != null && valueSet.IsEmpty)
		{
			instance.Add(Tests.False.Instance);
		}
		else if (valueSet == null || !valueSet.Complement().IsEmpty)
		{
			instance.Add(new Tests.One(new BoundDagRelationalTest(rel.Syntax, rel.Relation, rel.ConstantValue, output, rel.HasErrors)));
		}
		return Tests.AndSequence.Create(instance);
	}

	private TypeSymbol ErrorType(string name = "")
	{
		return new ExtendedErrorTypeSymbol(_compilation, name, 0, null);
	}

	private BoundDecisionDag MakeBoundDecisionDag(SyntaxNode syntax, ref TemporaryArray<StateForCase> cases)
	{
		PooledDictionary<DagState, DagState> pooledDictionary = s_uniqueStatePool.Allocate();
		DecisionDag decisionDag = MakeDecisionDag(ref cases, pooledDictionary);
		BoundLeafDecisionDagNode defaultDecision = new BoundLeafDecisionDagNode(syntax, _defaultLabel);
		ComputeBoundDecisionDagNodes(decisionDag, defaultDecision);
		BoundDecisionDagNode dag = decisionDag.RootNode.Dag;
		BoundDecisionDag result = new BoundDecisionDag(dag.Syntax, dag);
		foreach (KeyValuePair<DagState, DagState> item in pooledDictionary)
		{
			item.Key.ClearAndFree();
		}
		pooledDictionary.Free();
		return result;
	}

	private DecisionDag MakeDecisionDag(ref TemporaryArray<StateForCase> casesForRootNode, Dictionary<DagState, DagState> uniqueState)
	{
		TemporaryArray<DagState> workList = TemporaryArray<DagState>.Empty;
		try
		{
			ArrayBuilder<StateForCase> instance = ArrayBuilder<StateForCase>.GetInstance(casesForRootNode.Count);
			foreach (StateForCase item2 in casesForRootNode)
			{
				StateForCase item = item2.RewriteNestedLengthTests();
				if (!item.IsImpossible)
				{
					instance.Add(item);
					if (item.IsFullyMatched)
					{
						break;
					}
				}
			}
			DagState rootNode = uniquifyState(new FrozenArrayBuilder<StateForCase>(instance), ImmutableDictionary<BoundDagTemp, IValueSet>.Empty);
			while (workList.Count != 0)
			{
				DagState dagState = workList.RemoveLast();
				if (dagState.Cases.Count == 0)
				{
					continue;
				}
				StateForCase stateForCase = dagState.Cases[0];
				if (stateForCase.PatternIsSatisfied)
				{
					if (!stateForCase.IsFullyMatched)
					{
						FrozenArrayBuilder<StateForCase> cases = dagState.Cases.RemoveAt(0);
						dagState.FalseBranch = uniquifyState(cases, dagState.RemainingValues);
					}
					continue;
				}
				BoundDagTest boundDagTest = (dagState.SelectedTest = dagState.ComputeSelectedTest());
				BoundDagEvaluation boundDagEvaluation;
				if (!(boundDagTest is BoundDagAssignmentEvaluation boundDagAssignmentEvaluation))
				{
					boundDagEvaluation = boundDagTest as BoundDagEvaluation;
					if (boundDagEvaluation == null)
					{
						if (boundDagTest != null)
						{
							BoundDagTest boundDagTest2 = boundDagTest;
							bool foundExplicitNullTest = false;
							SplitCases(dagState, boundDagTest2, out FrozenArrayBuilder<StateForCase> whenTrue, out ImmutableDictionary<BoundDagTemp, IValueSet> whenTrueValues, out FrozenArrayBuilder<StateForCase> whenFalse, out ImmutableDictionary<BoundDagTemp, IValueSet> whenFalseValues, ref foundExplicitNullTest);
							dagState.TrueBranch = uniquifyState(whenTrue, whenTrueValues);
							dagState.FalseBranch = uniquifyState(whenFalse, whenFalseValues);
							if (foundExplicitNullTest && boundDagTest2 is BoundDagNonNullTest { IsExplicitTest: false } boundDagNonNullTest)
							{
								dagState.SelectedTest = new BoundDagNonNullTest(boundDagNonNullTest.Syntax, isExplicitTest: true, boundDagNonNullTest.Input, boundDagNonNullTest.HasErrors);
							}
							continue;
						}
						throw ExceptionUtilities.UnexpectedValue(boundDagTest.Kind);
					}
				}
				else
				{
					BoundDagAssignmentEvaluation boundDagAssignmentEvaluation2 = boundDagAssignmentEvaluation;
					if (dagState.RemainingValues.TryGetValue(boundDagAssignmentEvaluation2.Input, out IValueSet value))
					{
						if (dagState.RemainingValues.TryGetValue(boundDagAssignmentEvaluation2.Target, out IValueSet value2))
						{
							value = value.Intersect(value2);
						}
						dagState.TrueBranch = uniquifyState(RemoveEvaluation(dagState.Cases, boundDagAssignmentEvaluation2), dagState.RemainingValues.SetItem(boundDagAssignmentEvaluation2.Target, value));
						continue;
					}
					boundDagEvaluation = (BoundDagEvaluation)boundDagTest;
				}
				BoundDagEvaluation e = boundDagEvaluation;
				dagState.TrueBranch = uniquifyState(RemoveEvaluation(dagState.Cases, e), dagState.RemainingValues);
			}
			return new DecisionDag(rootNode);
		}
		finally
		{
			((IDisposable)workList/*cast due to constrained. prefix*/).Dispose();
		}
		DagState uniquifyState(FrozenArrayBuilder<StateForCase> cases2, ImmutableDictionary<BoundDagTemp, IValueSet> remainingValues)
		{
			DagState instance2 = DagState.GetInstance(cases2, remainingValues);
			if (uniqueState.TryGetValue(instance2, out DagState value3))
			{
				instance2.ClearAndFree();
				instance2 = null;
				ImmutableDictionary<BoundDagTemp, IValueSet>.Builder newRemainingValues = ImmutableDictionary.CreateBuilder<BoundDagTemp, IValueSet>();
				foreach (var (key, other) in remainingValues)
				{
					if (value3.RemainingValues.TryGetValue(key, out IValueSet value4))
					{
						IValueSet value5 = value4.Union(other);
						newRemainingValues.Add(key, value5);
					}
				}
				if (value3.RemainingValues.Count != newRemainingValues.Count || !value3.RemainingValues.All<KeyValuePair<BoundDagTemp, IValueSet>>((KeyValuePair<BoundDagTemp, IValueSet> kv) => newRemainingValues.TryGetValue(kv.Key, out IValueSet value6) && kv.Value.Equals(value6)))
				{
					value3.UpdateRemainingValues(newRemainingValues.ToImmutable());
					if (!workList.Contains(value3))
					{
						workList.Add(value3);
					}
				}
				return value3;
			}
			uniqueState.Add(instance2, instance2);
			workList.Add(instance2);
			return instance2;
		}
	}

	private void ComputeBoundDecisionDagNodes(DecisionDag decisionDag, BoundLeafDecisionDagNode defaultDecision)
	{
		if (!decisionDag.TryGetTopologicallySortedReachableStates(out ImmutableArray<DagState> result))
		{
			decisionDag.RootNode.Dag = defaultDecision;
			return;
		}
		PooledDictionary<BoundDecisionDagNode, BoundDecisionDagNode> uniqueNodes = PooledDictionary<BoundDecisionDagNode, BoundDecisionDagNode>.GetInstance();
		uniqifyDagNode(defaultDecision);
		for (int num = result.Length - 1; num >= 0; num--)
		{
			DagState dagState = result[num];
			if (dagState.Cases.Count == 0)
			{
				dagState.Dag = defaultDecision;
			}
			else
			{
				StateForCase stateForCase = dagState.Cases[0];
				if (stateForCase.PatternIsSatisfied)
				{
					if (stateForCase.IsFullyMatched)
					{
						dagState.Dag = finalState(stateForCase.Syntax, stateForCase.CaseLabel, stateForCase.Bindings);
					}
					else
					{
						BoundDecisionDagNode whenTrue = finalState(stateForCase.Syntax, stateForCase.CaseLabel, default(ImmutableArray<BoundPatternBinding>));
						BoundDecisionDagNode dag = dagState.FalseBranch.Dag;
						dagState.Dag = uniqifyDagNode(new BoundWhenDecisionDagNode(stateForCase.Syntax, stateForCase.Bindings, stateForCase.WhenClause, whenTrue, dag));
					}
				}
				else
				{
					BoundDagTest selectedTest = dagState.SelectedTest;
					if (!(selectedTest is BoundDagEvaluation boundDagEvaluation))
					{
						if (selectedTest == null)
						{
							throw ExceptionUtilities.UnexpectedValue(selectedTest?.Kind);
						}
						BoundDecisionDagNode dag2 = dagState.TrueBranch.Dag;
						BoundDecisionDagNode dag3 = dagState.FalseBranch.Dag;
						dagState.Dag = uniqifyDagNode(new BoundTestDecisionDagNode(selectedTest.Syntax, selectedTest, dag2, dag3));
					}
					else
					{
						BoundDecisionDagNode dag4 = dagState.TrueBranch.Dag;
						dagState.Dag = uniqifyDagNode(new BoundEvaluationDecisionDagNode(boundDagEvaluation.Syntax, boundDagEvaluation, dag4));
					}
				}
			}
		}
		uniqueNodes.Free();
		BoundDecisionDagNode finalState(SyntaxNode syntax, LabelSymbol label, ImmutableArray<BoundPatternBinding> bindings)
		{
			BoundDecisionDagNode boundDecisionDagNode = uniqifyDagNode(new BoundLeafDecisionDagNode(syntax, label));
			if (!bindings.IsDefaultOrEmpty)
			{
				return uniqifyDagNode(new BoundWhenDecisionDagNode(syntax, bindings, null, boundDecisionDagNode, null));
			}
			return boundDecisionDagNode;
		}
		BoundDecisionDagNode uniqifyDagNode(BoundDecisionDagNode node)
		{
			return uniqueNodes.GetOrAdd(node, node);
		}
	}

	private void SplitCase(DagState state, StateForCase stateForCase, BoundDagTest test, IValueSet? whenTrueValues, IValueSet? whenFalseValues, out StateForCase whenTrue, out StateForCase whenFalse, ref bool foundExplicitNullTest)
	{
		stateForCase.RemainingTests.Filter(this, test, state, whenTrueValues, whenFalseValues, out Tests whenTrue2, out Tests whenFalse2, ref foundExplicitNullTest);
		whenTrue = stateForCase.WithRemainingTests(whenTrue2);
		whenFalse = stateForCase.WithRemainingTests(whenFalse2);
	}

	private void SplitCases(DagState state, BoundDagTest test, out FrozenArrayBuilder<StateForCase> whenTrue, out ImmutableDictionary<BoundDagTemp, IValueSet> whenTrueValues, out FrozenArrayBuilder<StateForCase> whenFalse, out ImmutableDictionary<BoundDagTemp, IValueSet> whenFalseValues, ref bool foundExplicitNullTest)
	{
		FrozenArrayBuilder<StateForCase> cases = state.Cases;
		ArrayBuilder<StateForCase> instance = ArrayBuilder<StateForCase>.GetInstance(cases.Count);
		ArrayBuilder<StateForCase> instance2 = ArrayBuilder<StateForCase>.GetInstance(cases.Count);
		bool flag;
		bool flag2;
		(whenTrueValues, whenFalseValues, flag, flag2) = SplitValues(state.RemainingValues, test);
		whenTrueValues.TryGetValue(test.Input, out IValueSet value);
		whenFalseValues.TryGetValue(test.Input, out IValueSet value2);
		foreach (StateForCase item in cases)
		{
			SplitCase(state, item, test, value, value2, out var whenTrue2, out var whenFalse2, ref foundExplicitNullTest);
			if (flag && !whenTrue2.IsImpossible && (!instance.Any() || !instance.Last().IsFullyMatched))
			{
				instance.Add(whenTrue2);
			}
			if (flag2 && !whenFalse2.IsImpossible && (!instance2.Any() || !instance2.Last().IsFullyMatched))
			{
				instance2.Add(whenFalse2);
			}
		}
		whenTrue = AsFrozen(instance);
		whenFalse = AsFrozen(instance2);
	}

	private static (ImmutableDictionary<BoundDagTemp, IValueSet> whenTrueValues, ImmutableDictionary<BoundDagTemp, IValueSet> whenFalseValues, bool truePossible, bool falsePossible) SplitValues(ImmutableDictionary<BoundDagTemp, IValueSet> values, BoundDagTest test)
	{
		if (!(test is BoundDagEvaluation) && !(test is BoundDagExplicitNullTest) && !(test is BoundDagNonNullTest) && !(test is BoundDagTypeTest))
		{
			if (!(test is BoundDagValueTest boundDagValueTest))
			{
				if (test is BoundDagRelationalTest boundDagRelationalTest)
				{
					return resultForRelation(boundDagRelationalTest.Relation, boundDagRelationalTest.Value);
				}
				throw ExceptionUtilities.UnexpectedValue(test);
			}
			return resultForRelation(BinaryOperatorKind.Equal, boundDagValueTest.Value);
		}
		return (whenTrueValues: values, whenFalseValues: values, truePossible: true, falsePossible: true);
		(ImmutableDictionary<BoundDagTemp, IValueSet> whenTrueValues, ImmutableDictionary<BoundDagTemp, IValueSet> whenFalseValues, bool truePossible, bool falsePossible) resultForRelation(BinaryOperatorKind relation, ConstantValue value)
		{
			BoundDagTemp input = test.Input;
			IValueSetFactory valueSetFactory = ValueSetFactory.ForInput(input);
			if (valueSetFactory == null || value.IsBad)
			{
				return (whenTrueValues: values, whenFalseValues: values, truePossible: true, falsePossible: true);
			}
			IValueSet valueSet = valueSetFactory.Related(relation.Operator(), value);
			IValueSet valueSet2 = valueSet.Complement();
			if (values.TryGetValue(input, out IValueSet value2))
			{
				valueSet = valueSet.Intersect(value2);
				valueSet2 = valueSet2.Intersect(value2);
			}
			ImmutableDictionary<BoundDagTemp, IValueSet> item = values.SetItem(input, valueSet);
			ImmutableDictionary<BoundDagTemp, IValueSet> item2 = values.SetItem(input, valueSet2);
			return (whenTrueValues: item, whenFalseValues: item2, truePossible: !valueSet.IsEmpty, falsePossible: !valueSet2.IsEmpty);
		}
	}

	private static (BoundDagTemp? lengthTemp, int offset) TryGetTopLevelLengthTemp(BoundDagPropertyEvaluation e)
	{
		int num = 0;
		BoundDagTemp input = e.Input;
		BoundDagTemp item = null;
		while (input.Source is BoundDagSliceEvaluation boundDagSliceEvaluation)
		{
			num += boundDagSliceEvaluation.StartIndex - boundDagSliceEvaluation.EndIndex;
			item = boundDagSliceEvaluation.LengthTemp;
			input = boundDagSliceEvaluation.Input;
		}
		return (lengthTemp: item, offset: num);
	}

	private static (BoundDagTemp input, BoundDagTemp lengthTemp, int index) GetCanonicalInput(BoundDagIndexerEvaluation e)
	{
		int num = e.Index;
		BoundDagTemp input = e.Input;
		BoundDagTemp lengthTemp = e.LengthTemp;
		while (input.Source is BoundDagSliceEvaluation boundDagSliceEvaluation)
		{
			num = ((num < 0) ? (num - boundDagSliceEvaluation.EndIndex) : (num + boundDagSliceEvaluation.StartIndex));
			lengthTemp = boundDagSliceEvaluation.LengthTemp;
			input = boundDagSliceEvaluation.Input;
		}
		return (input: OriginalInput(input), lengthTemp: lengthTemp, index: num);
	}

	private static FrozenArrayBuilder<StateForCase> RemoveEvaluation(FrozenArrayBuilder<StateForCase> cases, BoundDagEvaluation e)
	{
		ArrayBuilder<StateForCase> instance = ArrayBuilder<StateForCase>.GetInstance(cases.Count);
		foreach (StateForCase item in cases)
		{
			Tests tests = item.RemainingTests.RemoveEvaluation(e);
			if (!(tests is Tests.False))
			{
				instance.Add(new StateForCase(item.Index, item.Syntax, tests, item.Bindings, item.WhenClause, item.CaseLabel));
			}
		}
		return AsFrozen(instance);
	}

	private void CheckConsistentDecision(BoundDagTest test, BoundDagTest other, IValueSet? whenTrueValues, IValueSet? whenFalseValues, SyntaxNode syntax, out bool trueTestPermitsTrueOther, out bool falseTestPermitsTrueOther, out bool trueTestImpliesTrueOther, out bool falseTestImpliesTrueOther, ref bool foundExplicitNullTest)
	{
		trueTestPermitsTrueOther = true;
		falseTestPermitsTrueOther = true;
		trueTestImpliesTrueOther = false;
		falseTestImpliesTrueOther = false;
		if (!(test is BoundDagNonNullTest))
		{
			if (!(test is BoundDagTypeTest boundDagTypeTest))
			{
				if (!(test is BoundDagValueTest) && !(test is BoundDagRelationalTest))
				{
					if (!(test is BoundDagExplicitNullTest))
					{
						return;
					}
					foundExplicitNullTest = true;
					if (!(other is BoundDagNonNullTest boundDagNonNullTest))
					{
						if (!(other is BoundDagTypeTest))
						{
							if (!(other is BoundDagExplicitNullTest))
							{
								if (other is BoundDagValueTest)
								{
									trueTestPermitsTrueOther = false;
								}
							}
							else
							{
								foundExplicitNullTest = true;
								trueTestImpliesTrueOther = true;
								falseTestPermitsTrueOther = false;
							}
						}
						else
						{
							trueTestPermitsTrueOther = false;
						}
					}
					else
					{
						if (boundDagNonNullTest.IsExplicitTest)
						{
							foundExplicitNullTest = true;
						}
						trueTestPermitsTrueOther = false;
						falseTestImpliesTrueOther = true;
					}
				}
				else if (!(other is BoundDagNonNullTest boundDagNonNullTest2))
				{
					if (!(other is BoundDagExplicitNullTest))
					{
						if (!(other is BoundDagRelationalTest boundDagRelationalTest))
						{
							if (other is BoundDagValueTest boundDagValueTest)
							{
								handleRelationWithValue(BinaryOperatorKind.Equal, boundDagValueTest.Value, out trueTestPermitsTrueOther, out falseTestPermitsTrueOther, out trueTestImpliesTrueOther, out falseTestImpliesTrueOther);
							}
						}
						else
						{
							handleRelationWithValue(boundDagRelationalTest.Relation, boundDagRelationalTest.Value, out trueTestPermitsTrueOther, out falseTestPermitsTrueOther, out trueTestImpliesTrueOther, out falseTestImpliesTrueOther);
						}
					}
					else
					{
						foundExplicitNullTest = true;
						trueTestPermitsTrueOther = false;
					}
				}
				else
				{
					if (boundDagNonNullTest2.IsExplicitTest)
					{
						foundExplicitNullTest = true;
					}
					trueTestImpliesTrueOther = true;
				}
			}
			else if (!(other is BoundDagNonNullTest boundDagNonNullTest3))
			{
				if (!(other is BoundDagTypeTest boundDagTypeTest2))
				{
					if (other is BoundDagExplicitNullTest)
					{
						foundExplicitNullTest = true;
						trueTestPermitsTrueOther = false;
					}
					return;
				}
				CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(_diagnostics, _compilation.Assembly);
				ConstantValue constantValue = ExpressionOfTypeMatchesPatternTypeForLearningFromSuccessfulTypeTest(boundDagTypeTest.Type, boundDagTypeTest2.Type, ref useSiteInfo);
				if (constantValue == ConstantValue.False)
				{
					trueTestPermitsTrueOther = false;
				}
				else if (constantValue == ConstantValue.True)
				{
					trueTestImpliesTrueOther = true;
				}
				constantValue = Binder.ExpressionOfTypeMatchesPatternType(_conversions, boundDagTypeTest2.Type, boundDagTypeTest.Type, ref useSiteInfo, out var _);
				_diagnostics.Add(syntax, useSiteInfo);
				if (constantValue == ConstantValue.True)
				{
					falseTestPermitsTrueOther = false;
				}
			}
			else
			{
				if (boundDagNonNullTest3.IsExplicitTest)
				{
					foundExplicitNullTest = true;
				}
				trueTestImpliesTrueOther = true;
			}
		}
		else if (!(other is BoundDagValueTest))
		{
			if (!(other is BoundDagExplicitNullTest))
			{
				if (other is BoundDagNonNullTest boundDagNonNullTest4)
				{
					if (boundDagNonNullTest4.IsExplicitTest)
					{
						foundExplicitNullTest = true;
					}
					trueTestImpliesTrueOther = true;
					falseTestPermitsTrueOther = false;
				}
				else
				{
					falseTestPermitsTrueOther = false;
				}
			}
			else
			{
				foundExplicitNullTest = true;
				trueTestPermitsTrueOther = false;
				falseTestImpliesTrueOther = true;
			}
		}
		else
		{
			falseTestPermitsTrueOther = false;
		}
		void handleRelationWithValue(BinaryOperatorKind relation, ConstantValue value, out bool reference, out bool reference3, out bool reference2, out bool reference4)
		{
			bool flag = test.Equals(other);
			reference = whenTrueValues?.Any(relation, value) ?? true;
			reference2 = flag || (reference && (whenTrueValues?.All(relation, value) ?? false));
			reference3 = !flag && (whenFalseValues?.Any(relation, value) ?? true);
			reference4 = reference3 && (whenFalseValues?.All(relation, value) ?? false);
		}
	}

	private bool CheckInputRelation(SyntaxNode syntax, DagState state, BoundDagTest test, BoundDagTest other, out Tests relationCondition, out Tests relationEffect)
	{
		relationCondition = Tests.True.Instance;
		relationEffect = Tests.True.Instance;
		if (test.Input == other.Input)
		{
			return true;
		}
		bool flag = ((test is BoundDagNonNullTest || test is BoundDagExplicitNullTest) ? true : false);
		bool flag2 = !flag;
		if (flag2)
		{
			bool flag3 = ((other is BoundDagNonNullTest || other is BoundDagExplicitNullTest) ? true : false);
			flag2 = !flag3;
		}
		if (flag2 && (!(test is BoundDagTypeTest) || !(other is BoundDagTypeTest)) && !test.Input.Type.Equals(other.Input.Type, TypeCompareKind.AllIgnoreOptions))
		{
			return false;
		}
		BoundDagTemp boundDagTemp = OriginalInput(test.Input);
		BoundDagTemp boundDagTemp2 = OriginalInput(other.Input);
		ArrayBuilder<Tests> arrayBuilder = null;
		while (boundDagTemp.Index == boundDagTemp2.Index)
		{
			BoundDagEvaluation source = boundDagTemp.Source;
			BoundDagEvaluation source2 = boundDagTemp2.Source;
			if (source is BoundDagTypeEvaluation || source2 is BoundDagTypeEvaluation)
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/DecisionDagBuilder.cs", 1496);
			}
			if (source != source2)
			{
				if (source is BoundDagIndexerEvaluation e)
				{
					if (source2 is BoundDagIndexerEvaluation e2)
					{
						BoundDagTemp boundDagTemp3;
						int num;
						(boundDagTemp, boundDagTemp3, num) = GetCanonicalInput(e);
						int num2;
						(boundDagTemp2, _, num2) = GetCanonicalInput(e2);
						if (boundDagTemp.Index != boundDagTemp2.Index)
						{
							break;
						}
						if (num == num2)
						{
							continue;
						}
						if (num < 0 == num2 < 0)
						{
							break;
						}
						IValueSet<int> valueSet = (IValueSet<int>)state.RemainingValues[boundDagTemp3];
						int value = ((num < 0) ? (num2 - num) : (num - num2));
						if (!valueSet.All(BinaryOperatorKind.Equal, value))
						{
							if (_forLowering || !valueSet.Any(BinaryOperatorKind.Equal, value))
							{
								break;
							}
							(arrayBuilder ?? (arrayBuilder = ArrayBuilder<Tests>.GetInstance())).Add(new Tests.One(new BoundDagValueTest(syntax, ConstantValue.Create(value), boundDagTemp3)));
						}
						continue;
					}
				}
				else if (source == null)
				{
					break;
				}
				if (source2 == null)
				{
					break;
				}
				BoundDagEvaluation boundDagEvaluation = source;
				BoundDagEvaluation boundDagEvaluation2 = source2;
				if (!boundDagEvaluation.IsEquivalentTo(boundDagEvaluation2))
				{
					break;
				}
				boundDagTemp = OriginalInput(boundDagEvaluation.Input);
				boundDagTemp2 = OriginalInput(boundDagEvaluation2.Input);
				continue;
			}
			if (arrayBuilder != null)
			{
				relationCondition = Tests.AndSequence.Create(arrayBuilder);
				relationEffect = new Tests.One(new BoundDagAssignmentEvaluation(syntax, other.Input, test.Input));
			}
			return true;
		}
		arrayBuilder?.Free();
		return false;
	}

	private ConstantValue? ExpressionOfTypeMatchesPatternTypeForLearningFromSuccessfulTypeTest(TypeSymbol expressionType, TypeSymbol patternType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ConstantValue result = Binder.ExpressionOfTypeMatchesPatternType(_conversions, expressionType, patternType, ref useSiteInfo, out var conversion);
		if (conversion.Exists || !isRuntimeSimilar(expressionType, patternType))
		{
			return result;
		}
		return null;
		static bool isRuntimeSimilar(TypeSymbol typeSymbol, TypeSymbol typeSymbol2)
		{
			TypeSymbol typeSymbol4;
			TypeSymbol typeSymbol3;
			for (; typeSymbol is ArrayTypeSymbol arrayTypeSymbol && typeSymbol2 is ArrayTypeSymbol arrayTypeSymbol2 && arrayTypeSymbol.IsSZArray == arrayTypeSymbol2.IsSZArray && arrayTypeSymbol.Rank == arrayTypeSymbol2.Rank; typeSymbol2 = typeSymbol4, typeSymbol = typeSymbol3)
			{
				typeSymbol3 = arrayTypeSymbol.ElementType.EnumUnderlyingTypeOrSelf();
				typeSymbol4 = arrayTypeSymbol2.ElementType.EnumUnderlyingTypeOrSelf();
				SpecialType specialType = typeSymbol3.SpecialType;
				SpecialType specialType2 = typeSymbol4.SpecialType;
				if (specialType != specialType2)
				{
					switch (specialType)
					{
					case SpecialType.System_SByte:
						if (specialType2 != SpecialType.System_Byte)
						{
							continue;
						}
						break;
					case SpecialType.System_Byte:
						if (specialType2 != SpecialType.System_SByte)
						{
							continue;
						}
						break;
					case SpecialType.System_Int16:
						if (specialType2 != SpecialType.System_UInt16)
						{
							continue;
						}
						break;
					case SpecialType.System_UInt16:
						if (specialType2 != SpecialType.System_Int16)
						{
							continue;
						}
						break;
					case SpecialType.System_Int32:
						if (specialType2 != SpecialType.System_UInt32 && (uint)(specialType2 - 21) > 1u)
						{
							continue;
						}
						break;
					case SpecialType.System_UInt32:
						if (specialType2 != SpecialType.System_Int32 && (uint)(specialType2 - 21) > 1u)
						{
							continue;
						}
						break;
					case SpecialType.System_Int64:
						if (specialType2 != SpecialType.System_UInt64 && (uint)(specialType2 - 21) > 1u)
						{
							continue;
						}
						break;
					case SpecialType.System_UInt64:
						if (specialType2 != SpecialType.System_Int64 && (uint)(specialType2 - 21) > 1u)
						{
							continue;
						}
						break;
					case SpecialType.System_IntPtr:
						if ((uint)(specialType2 - 13) > 3u && specialType2 != SpecialType.System_UIntPtr)
						{
							continue;
						}
						break;
					case SpecialType.System_UIntPtr:
						if ((uint)(specialType2 - 13) > 3u && specialType2 != SpecialType.System_IntPtr)
						{
							continue;
						}
						break;
					default:
						continue;
					}
				}
				return true;
			}
			return false;
		}
	}

	private static FrozenArrayBuilder<T> AsFrozen<T>(ArrayBuilder<T> builder)
	{
		return new FrozenArrayBuilder<T>(builder);
	}

	internal static void CheckRedundantPatternsForIsPattern(CSharpCompilation compilation, SyntaxNode syntax, BoundExpression inputExpression, BoundPattern pattern, BindingDiagnosticBag diagnostics)
	{
		if (!pattern.HasErrors)
		{
			LabelSymbol defaultLabel = new GeneratedLabelSymbol("defaultLabel");
			DecisionDagBuilder builder = new DecisionDagBuilder(compilation, defaultLabel, forLowering: false, BindingDiagnosticBag.Discarded);
			BoundDagTemp rootIdentifier = BoundDagTemp.ForOriginalInput(inputExpression);
			PooledHashSet<SyntaxNode> instance = PooledHashSet<SyntaxNode>.GetInstance();
			ArrayBuilder<StateForCase> instance2 = ArrayBuilder<StateForCase>.GetInstance(0);
			CheckOrAndAndReachability(instance2, 0, pattern, builder, rootIdentifier, syntax, diagnostics, instance);
			ReportRedundant(instance, diagnostics);
			instance.Free();
			instance2.Free();
		}
	}

	internal static bool EnableRedundantPatternsCheck(CSharpCompilation compilation)
	{
		return compilation.LanguageVersion >= LanguageVersion.CSharp14;
	}

	internal static void CheckRedundantPatternsForSwitchExpression(CSharpCompilation compilation, SyntaxNode syntax, BoundExpression inputExpression, ImmutableArray<BoundSwitchExpressionArm> switchArms, BindingDiagnosticBag diagnostics)
	{
		PooledHashSet<SyntaxNode> instance = PooledHashSet<SyntaxNode>.GetInstance();
		ArrayBuilder<StateForCase> instance2 = ArrayBuilder<StateForCase>.GetInstance(switchArms.Length);
		checkRedundantPatternsForSwitchExpression(compilation, syntax, inputExpression, switchArms, diagnostics, instance, instance2);
		instance2.Free();
		instance.Free();
		static void checkRedundantPatternsForSwitchExpression(CSharpCompilation compilation2, SyntaxNode syntax2, BoundExpression expr, ImmutableArray<BoundSwitchExpressionArm> immutableArray, BindingDiagnosticBag diagnostics2, PooledHashSet<SyntaxNode> redundantNodes, ArrayBuilder<StateForCase> existingCases)
		{
			LabelSymbol defaultLabel = new GeneratedLabelSymbol("defaultLabel");
			DecisionDagBuilder decisionDagBuilder = new DecisionDagBuilder(compilation2, defaultLabel, forLowering: false, BindingDiagnosticBag.Discarded);
			BoundDagTemp boundDagTemp = BoundDagTemp.ForOriginalInput(expr);
			int num = 0;
			foreach (BoundSwitchExpressionArm item in immutableArray)
			{
				if (item.Pattern.HasErrors)
				{
					return;
				}
				existingCases.Add(decisionDagBuilder.MakeTestsForPattern(++num, item.Syntax, boundDagTemp, item.Pattern, item.WhenClause, item.Label));
			}
			for (int i = 0; i < immutableArray.Length; i++)
			{
				CheckOrAndAndReachability(existingCases, i, immutableArray[i].Pattern, decisionDagBuilder, boundDagTemp, syntax2, diagnostics2, redundantNodes);
			}
			ReportRedundant(redundantNodes, diagnostics2);
		}
	}

	internal static void CheckRedundantPatternsForSwitchStatement(CSharpCompilation compilation, SyntaxNode syntax, BoundExpression inputExpression, ImmutableArray<BoundSwitchSection> switchSections, BindingDiagnosticBag diagnostics)
	{
		PooledHashSet<SyntaxNode> instance = PooledHashSet<SyntaxNode>.GetInstance();
		ArrayBuilder<StateForCase> instance2 = ArrayBuilder<StateForCase>.GetInstance();
		checkRedundantPatternsForSwitchStatement(compilation, syntax, inputExpression, switchSections, diagnostics, instance, instance2);
		instance2.Free();
		instance.Free();
		static void checkRedundantPatternsForSwitchStatement(CSharpCompilation compilation2, SyntaxNode syntax2, BoundExpression expr, ImmutableArray<BoundSwitchSection> immutableArray, BindingDiagnosticBag diagnostics2, PooledHashSet<SyntaxNode> redundantNodes, ArrayBuilder<StateForCase> existingCases)
		{
			LabelSymbol defaultLabel = new GeneratedLabelSymbol("defaultLabel");
			DecisionDagBuilder decisionDagBuilder = new DecisionDagBuilder(compilation2, defaultLabel, forLowering: false, BindingDiagnosticBag.Discarded);
			BoundDagTemp boundDagTemp = BoundDagTemp.ForOriginalInput(expr);
			int num = 0;
			foreach (BoundSwitchSection item in immutableArray)
			{
				foreach (BoundSwitchLabel switchLabel in item.SwitchLabels)
				{
					if (switchLabel.Syntax.Kind() != SyntaxKind.DefaultSwitchLabel)
					{
						if (switchLabel.Pattern.HasErrors)
						{
							return;
						}
						existingCases.Add(decisionDagBuilder.MakeTestsForPattern(++num, switchLabel.Syntax, boundDagTemp, switchLabel.Pattern, switchLabel.WhenClause, switchLabel.Label));
					}
				}
			}
			int num2 = 0;
			foreach (BoundSwitchSection item2 in immutableArray)
			{
				foreach (BoundSwitchLabel switchLabel2 in item2.SwitchLabels)
				{
					if (switchLabel2.Syntax.Kind() != SyntaxKind.DefaultSwitchLabel)
					{
						CheckOrAndAndReachability(existingCases, num2, switchLabel2.Pattern, decisionDagBuilder, boundDagTemp, syntax2, diagnostics2, redundantNodes);
						num2++;
					}
				}
			}
			ReportRedundant(redundantNodes, diagnostics2);
		}
	}

	private static void ReportRedundant(PooledHashSet<SyntaxNode> redundantNodes, BindingDiagnosticBag diagnostics)
	{
		foreach (SyntaxNode redundantNode in redundantNodes)
		{
			ErrorCode code = (shouldWarn(redundantNode) ? ErrorCode.WRN_RedundantPattern : ErrorCode.HDN_RedundantPattern);
			diagnostics.Add(code, redundantNode);
		}
		static bool findNotInBinary(SyntaxNode syntax)
		{
			while (syntax is BinaryPatternSyntax binaryPatternSyntax)
			{
				if (findNotInBinary(binaryPatternSyntax.Right))
				{
					return true;
				}
				syntax = binaryPatternSyntax.Left;
			}
			return syntax.Kind() == SyntaxKind.NotPattern;
		}
		static bool shouldWarn(SyntaxNode syntax)
		{
			CSharpSyntaxNode parent = default(CSharpSyntaxNode);
			while (true)
			{
				if (syntax.Parent is ParenthesizedPatternSyntax parenthesizedPatternSyntax)
				{
					syntax = parenthesizedPatternSyntax;
				}
				else if (syntax.Parent is BinaryPatternSyntax binaryPatternSyntax)
				{
					if (binaryPatternSyntax.Right == syntax && findNotInBinary(binaryPatternSyntax.Left))
					{
						return true;
					}
					syntax = binaryPatternSyntax;
				}
				else
				{
					SubpatternSyntax subpatternSyntax = syntax.Parent as SubpatternSyntax;
					bool flag = subpatternSyntax != null;
					if (flag)
					{
						parent = subpatternSyntax.Parent;
						bool flag2 = ((parent is PropertyPatternClauseSyntax || parent is PositionalPatternClauseSyntax) ? true : false);
						flag = flag2;
					}
					if (flag && parent.Parent is RecursivePatternSyntax recursivePatternSyntax)
					{
						syntax = recursivePatternSyntax;
					}
					else if (syntax.Parent is ListPatternSyntax listPatternSyntax)
					{
						syntax = listPatternSyntax;
					}
					else
					{
						if (!(syntax.Parent is SlicePatternSyntax slicePatternSyntax))
						{
							break;
						}
						syntax = slicePatternSyntax;
					}
				}
			}
			return false;
		}
	}

	private static void CheckOrAndAndReachability(ArrayBuilder<StateForCase> previousCases, int patternIndex, BoundPattern pattern, DecisionDagBuilder builder, BoundDagTemp rootIdentifier, SyntaxNode syntax, BindingDiagnosticBag diagnostics, PooledHashSet<SyntaxNode> redundantNodes)
	{
		ReachabilityAnalysisContext context = new ReachabilityAnalysisContext(previousCases, patternIndex, builder, rootIdentifier, syntax, redundantNodes);
		try
		{
			analyze(PatternNormalizer.Rewrite(pattern, rootIdentifier.Type), in context);
			analyze(PatternNormalizer.Rewrite(new BoundNegatedPattern(pattern.Syntax, pattern, pattern.InputType, pattern.InputType), rootIdentifier.Type), in context);
		}
		catch (InsufficientExecutionStackException)
		{
			diagnostics.Add(ErrorCode.HDN_RedundantPatternStackGuard, pattern.Syntax);
		}
		static void addPatternsFromOrTree(BoundPattern boundPattern, ArrayBuilder<BoundPattern> arrayBuilder)
		{
			if (boundPattern is BoundBinaryPattern { Disjunction: not false } boundBinaryPattern)
			{
				ArrayBuilder<BoundBinaryPattern> instance = ArrayBuilder<BoundBinaryPattern>.GetInstance();
				BoundBinaryPattern boundBinaryPattern2 = boundBinaryPattern;
				do
				{
					instance.Push(boundBinaryPattern2);
					boundBinaryPattern2 = boundBinaryPattern2.Left as BoundBinaryPattern;
				}
				while (boundBinaryPattern2 != null && boundBinaryPattern2.Disjunction);
				boundBinaryPattern2 = instance.Pop();
				arrayBuilder.Add(boundBinaryPattern2.Left);
				do
				{
					addPatternsFromOrTree(boundBinaryPattern2.Right, arrayBuilder);
				}
				while (instance.TryPop(out boundBinaryPattern2));
				instance.Free();
			}
			else
			{
				arrayBuilder.Add(boundPattern);
			}
		}
		static void analyze(BoundPattern boundPattern, ref readonly ReachabilityAnalysisContext context2)
		{
			if (boundPattern is BoundBinaryPattern binaryPattern)
			{
				ArrayBuilder<BoundPattern> instance = ArrayBuilder<BoundPattern>.GetInstance();
				analyzeBinary(instance, binaryPattern, null, in context2);
				instance.Free();
			}
		}
		static void analyzeBinary(ArrayBuilder<BoundPattern> currentCases, BoundBinaryPattern binaryPattern, Func<BoundPattern, BoundPattern>? wrapIntoParentAndPattern, ref readonly ReachabilityAnalysisContext context2)
		{
			if (binaryPattern.Disjunction)
			{
				int count = currentCases.Count;
				ArrayBuilder<BoundPattern> instance = ArrayBuilder<BoundPattern>.GetInstance();
				addPatternsFromOrTree(binaryPattern, instance);
				for (int i = 0; i < instance.Count; i++)
				{
					BoundPattern boundPattern = instance[i];
					analyzePattern(currentCases, boundPattern, wrapIntoParentAndPattern, in context2);
					BoundPattern item = wrapIntoParentAndPattern?.Invoke(boundPattern) ?? boundPattern;
					currentCases.Add(item);
				}
				checkReachability(currentCases, in context2);
				currentCases.Count = count;
				instance.Free();
			}
			else
			{
				ArrayBuilder<BoundBinaryPattern> instance2 = ArrayBuilder<BoundBinaryPattern>.GetInstance();
				BoundBinaryPattern current = binaryPattern;
				do
				{
					instance2.Push(current);
					current = current.Left as BoundBinaryPattern;
				}
				while (current != null && !current.Disjunction);
				current = instance2.Pop();
				analyzePattern(currentCases, current.Left, wrapIntoParentAndPattern, in context2);
				Func<BoundPattern, BoundPattern> wrapIntoParentAndPattern2 = delegate(BoundPattern newPattern)
				{
					bool wasCompilerGenerated = newPattern.WasCompilerGenerated;
					BoundBinaryPattern boundBinaryPattern = new BoundBinaryPattern(newPattern.Syntax, disjunction: false, current.Left, newPattern, current.InputType, newPattern.NarrowedType);
					BoundPattern boundPattern2 = wrapIntoParentAndPattern?.Invoke(boundBinaryPattern) ?? boundBinaryPattern;
					if (wasCompilerGenerated)
					{
						boundPattern2 = boundPattern2.MakeCompilerGenerated();
					}
					return boundPattern2;
				};
				do
				{
					analyzePattern(currentCases, current.Right, wrapIntoParentAndPattern2, in context2);
				}
				while (instance2.TryPop(out current));
				instance2.Free();
			}
		}
		static void analyzePattern(ArrayBuilder<BoundPattern> currentCases, BoundPattern boundPattern, Func<BoundPattern, BoundPattern>? wrapIntoParentAndPattern, ref readonly ReachabilityAnalysisContext context2)
		{
			if (boundPattern is BoundBinaryPattern binaryPattern)
			{
				analyzeBinary(currentCases, binaryPattern, wrapIntoParentAndPattern, in context2);
			}
		}
		static void checkReachability(ArrayBuilder<BoundPattern> orCases, ref readonly ReachabilityAnalysisContext reference)
		{
			using TemporaryArray<StateForCase> array = TemporaryArray<StateForCase>.GetInstance(orCases.Count);
			PooledHashSet<LabelSymbol> instance = PooledHashSet<LabelSymbol>.GetInstance();
			populateStateForCases(orCases, instance, ref TemporaryArrayExtensions.AsRef(in array), in reference);
			BoundDecisionDag boundDecisionDag = reference.Builder.MakeBoundDecisionDag(reference.Syntax, ref TemporaryArrayExtensions.AsRef(in array));
			for (int i = 0; i < array.Count; i++)
			{
				StateForCase stateForCase = array[i];
				if (!boundDecisionDag.ReachableLabels.Contains(stateForCase.CaseLabel) && !instance.Contains(stateForCase.CaseLabel))
				{
					reference.RedundantNodes.Add(stateForCase.Syntax);
				}
			}
			instance.Free();
		}
		static void populateStateForCases(ArrayBuilder<BoundPattern> set, PooledHashSet<LabelSymbol> labelsToIgnore, ref TemporaryArray<StateForCase> casesBuilder, ref readonly ReachabilityAnalysisContext reference)
		{
			int patternIndex2 = reference.PatternIndex;
			ArrayBuilder<StateForCase> previousCases2 = reference.PreviousCases;
			for (int i = 0; i < patternIndex2; i++)
			{
				casesBuilder.Add(previousCases2[i]);
			}
			int num = patternIndex2;
			foreach (BoundPattern item2 in set)
			{
				GeneratedLabelSymbol generatedLabelSymbol = new GeneratedLabelSymbol("orCase");
				SyntaxNode syntax2 = item2.Syntax;
				if (item2.WasCompilerGenerated)
				{
					labelsToIgnore.Add(generatedLabelSymbol);
					syntax2 = reference.Syntax;
				}
				casesBuilder.Add(reference.Builder.MakeTestsForPattern(++num, syntax2, reference.RootIdentifier, item2, null, generatedLabelSymbol));
			}
		}
	}

	private Tests MakeTestsAndBindingsForListPattern(BoundDagTemp input, BoundListPattern list, out BoundDagTemp output, ArrayBuilder<BoundPatternBinding> bindings)
	{
		SyntaxNode syntax = list.Syntax;
		ImmutableArray<BoundPattern> subpatterns = list.Subpatterns;
		ArrayBuilder<Tests> instance = ArrayBuilder<Tests>.GetInstance(4 + subpatterns.Length * 2);
		output = (input = MakeConvertToType(input, list.Syntax, list.NarrowedType, isExplicitTest: false, instance));
		if (list.HasErrors)
		{
			instance.Add(new Tests.One(new BoundDagTypeTest(list.Syntax, ErrorType(), input, hasErrors: true)));
		}
		else if (!list.HasSlice || subpatterns.Length != 1 || !(subpatterns[0] is BoundSlicePattern { Pattern: null }))
		{
			PropertySymbol propertySymbol = Binder.GetPropertySymbol(list.LengthAccess, out var _, out var _);
			BoundDagPropertyEvaluation boundDagPropertyEvaluation = new BoundDagPropertyEvaluation(syntax, propertySymbol, isLengthOrCount: true, input);
			instance.Add(new Tests.One(boundDagPropertyEvaluation));
			BoundDagTemp boundDagTemp = new BoundDagTemp(syntax, _compilation.GetSpecialType(SpecialType.System_Int32), boundDagPropertyEvaluation);
			instance.Add(new Tests.One(list.HasSlice ? ((BoundDagTest)new BoundDagRelationalTest(syntax, BinaryOperatorKind.IntGreaterThanOrEqual, ConstantValue.Create(subpatterns.Length - 1), boundDagTemp)) : ((BoundDagTest)new BoundDagValueTest(syntax, ConstantValue.Create(subpatterns.Length), boundDagTemp))));
			int num = 0;
			foreach (BoundPattern item in subpatterns)
			{
				if (item is BoundSlicePattern boundSlicePattern2)
				{
					int startIndex = num;
					num -= subpatterns.Length - 1;
					BoundPattern pattern = boundSlicePattern2.Pattern;
					if (pattern != null)
					{
						BoundDagSliceEvaluation boundDagSliceEvaluation = new BoundDagSliceEvaluation(pattern.Syntax, pattern.InputType, boundDagTemp, startIndex, num, boundSlicePattern2.IndexerAccess, boundSlicePattern2.ReceiverPlaceholder, boundSlicePattern2.ArgumentPlaceholder, input);
						instance.Add(new Tests.One(boundDagSliceEvaluation));
						BoundDagTemp input2 = new BoundDagTemp(pattern.Syntax, pattern.InputType, boundDagSliceEvaluation);
						instance.Add(MakeTestsAndBindings(input2, pattern, bindings));
					}
				}
				else
				{
					BoundDagIndexerEvaluation boundDagIndexerEvaluation = new BoundDagIndexerEvaluation(item.Syntax, item.InputType, boundDagTemp, num++, list.IndexerAccess, list.ReceiverPlaceholder, list.ArgumentPlaceholder, input);
					instance.Add(new Tests.One(boundDagIndexerEvaluation));
					BoundDagTemp input3 = new BoundDagTemp(item.Syntax, item.InputType, boundDagIndexerEvaluation);
					instance.Add(MakeTestsAndBindings(input3, item, bindings));
				}
			}
		}
		if (list.VariableAccess != null)
		{
			bindings.Add(new BoundPatternBinding(list.VariableAccess, input));
		}
		return Tests.AndSequence.Create(instance);
	}
}
