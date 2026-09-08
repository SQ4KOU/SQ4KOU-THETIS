using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class PatternExplainer
{
	private class NoRemainingValuesException : Exception
	{
	}

	private static ImmutableArray<BoundDecisionDagNode> ShortestPathToNode(ImmutableArray<BoundDecisionDagNode> nodes, BoundDecisionDagNode node, bool nullPaths, out bool requiresFalseWhenClause)
	{
		PooledDictionary<BoundDecisionDagNode, (int distance, BoundDecisionDagNode next)> dist = PooledDictionary<BoundDecisionDagNode, (int, BoundDecisionDagNode)>.GetInstance();
		int length = nodes.Length;
		int infinity = 2 * length + 2;
		PooledDictionary<BoundDecisionDagNode, (int, BoundDecisionDagNode)> pooledDictionary;
		BoundDecisionDagNode key;
		(int, BoundDecisionDagNode) value;
		for (int num = length - 1; num >= 0; pooledDictionary.Add(key, value), num--)
		{
			BoundDecisionDagNode boundDecisionDagNode = nodes[num];
			pooledDictionary = dist;
			key = boundDecisionDagNode;
			BoundDecisionDagNode boundDecisionDagNode2 = boundDecisionDagNode;
			if (!(boundDecisionDagNode2 is BoundEvaluationDecisionDagNode boundEvaluationDecisionDagNode))
			{
				if (boundDecisionDagNode2 is BoundTestDecisionDagNode boundTestDecisionDagNode)
				{
					BoundDagTest test = boundTestDecisionDagNode.Test;
					if (!(test is BoundDagNonNullTest))
					{
						if (test is BoundDagExplicitNullTest)
						{
							BoundTestDecisionDagNode boundTestDecisionDagNode2 = boundTestDecisionDagNode;
							if (!nullPaths)
							{
								value = (1 + distance(boundTestDecisionDagNode2.WhenFalse), boundTestDecisionDagNode2.WhenFalse);
								continue;
							}
						}
					}
					else
					{
						BoundTestDecisionDagNode boundTestDecisionDagNode3 = boundTestDecisionDagNode;
						if (!nullPaths)
						{
							value = (1 + distance(boundTestDecisionDagNode3.WhenTrue), boundTestDecisionDagNode3.WhenTrue);
							continue;
						}
					}
					BoundTestDecisionDagNode boundTestDecisionDagNode4 = boundTestDecisionDagNode;
					int num2 = distance(boundTestDecisionDagNode4.WhenTrue);
					int num3 = distance(boundTestDecisionDagNode4.WhenFalse);
					value = ((num2 <= num3) ? (1 + num2, boundTestDecisionDagNode4.WhenTrue) : (1 + num3, boundTestDecisionDagNode4.WhenFalse));
				}
				else if (boundDecisionDagNode2 is BoundWhenDecisionDagNode boundWhenDecisionDagNode)
				{
					BoundWhenDecisionDagNode boundWhenDecisionDagNode2 = boundWhenDecisionDagNode;
					int num4 = distance(boundWhenDecisionDagNode2.WhenTrue);
					int num5 = distance(boundWhenDecisionDagNode2.WhenFalse);
					value = ((num4 <= num5) ? (1 + num4, boundWhenDecisionDagNode2.WhenTrue) : (1 + ((num5 < length) ? length : 0) + num5, boundWhenDecisionDagNode2.WhenFalse));
				}
				else
				{
					value = ((boundDecisionDagNode == node) ? 1 : infinity, null);
				}
			}
			else
			{
				BoundEvaluationDecisionDagNode boundEvaluationDecisionDagNode2 = boundEvaluationDecisionDagNode;
				value = (distance(boundEvaluationDecisionDagNode2.Next), boundEvaluationDecisionDagNode2.Next);
			}
		}
		int item = dist[nodes[0]].distance;
		requiresFalseWhenClause = item > length;
		ArrayBuilder<BoundDecisionDagNode> instance = ArrayBuilder<BoundDecisionDagNode>.GetInstance(item);
		BoundDecisionDagNode boundDecisionDagNode3 = nodes[0];
		while (boundDecisionDagNode3 != node)
		{
			instance.Add(boundDecisionDagNode3);
			if (!(boundDecisionDagNode3 is BoundEvaluationDecisionDagNode boundEvaluationDecisionDagNode3))
			{
				if (!(boundDecisionDagNode3 is BoundTestDecisionDagNode key2))
				{
					if (!(boundDecisionDagNode3 is BoundWhenDecisionDagNode boundWhenDecisionDagNode3))
					{
						throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/PatternExplainer.cs", 93);
					}
					instance.RemoveLast();
					boundDecisionDagNode3 = boundWhenDecisionDagNode3.WhenFalse;
				}
				else
				{
					boundDecisionDagNode3 = dist[key2].next;
				}
			}
			else
			{
				boundDecisionDagNode3 = boundEvaluationDecisionDagNode3.Next;
			}
		}
		dist.Free();
		return instance.ToImmutableAndFree();
		int distance(BoundDecisionDagNode x)
		{
			if (x == null)
			{
				return infinity;
			}
			if (dist.TryGetValue(x, out (int, BoundDecisionDagNode) value2))
			{
				return value2.Item1;
			}
			return infinity;
		}
	}

	private static void VisitPathsToNode(BoundDecisionDagNode rootNode, BoundDecisionDagNode targetNode, bool nullPaths, Func<ImmutableArray<BoundDecisionDagNode>, bool, bool> handler)
	{
		ArrayBuilder<BoundDecisionDagNode> pathBuilder = ArrayBuilder<BoundDecisionDagNode>.GetInstance();
		ArrayBuilder<BoundDecisionDagNode?> stack = ArrayBuilder<BoundDecisionDagNode>.GetInstance();
		exploreToNode(rootNode, currentRequiresFalseWhenClause: false);
		stack.Free();
		pathBuilder.Free();
		bool exploreToNode(BoundDecisionDagNode? currentNode, bool currentRequiresFalseWhenClause)
		{
			if (currentNode == null)
			{
				return true;
			}
			int count = stack.Count;
			stack.Push(currentNode);
			do
			{
				currentNode = stack.Pop();
				if (currentNode == null)
				{
					pathBuilder.Pop();
				}
				else if (currentNode == targetNode)
				{
					if (!handler(pathBuilder.ToImmutable(), currentRequiresFalseWhenClause))
					{
						stack.Count = count;
						return false;
					}
				}
				else
				{
					pathBuilder.Push(currentNode);
					if (currentNode is BoundLeafDecisionDagNode)
					{
						goto IL_018e;
					}
					if (!(currentNode is BoundTestDecisionDagNode boundTestDecisionDagNode))
					{
						if (!(currentNode is BoundEvaluationDecisionDagNode boundEvaluationDecisionDagNode))
						{
							if (!(currentNode is BoundWhenDecisionDagNode boundWhenDecisionDagNode))
							{
								throw ExceptionUtilities.UnexpectedValue(currentNode.Kind);
							}
							pathBuilder.Pop();
							if (!exploreToNode(boundWhenDecisionDagNode.WhenFalse, currentRequiresFalseWhenClause: true))
							{
								stack.Count = count;
								return false;
							}
						}
						else
						{
							if (boundEvaluationDecisionDagNode.Next == null)
							{
								goto IL_018e;
							}
							stack.Push(null);
							stack.Push(boundEvaluationDecisionDagNode.Next);
						}
					}
					else
					{
						stack.Push(null);
						bool num = boundTestDecisionDagNode.Test is BoundDagExplicitNullTest && !nullPaths;
						if ((!(boundTestDecisionDagNode.Test is BoundDagNonNullTest) || nullPaths) && boundTestDecisionDagNode.WhenFalse != null)
						{
							stack.Push(boundTestDecisionDagNode.WhenFalse);
						}
						if (!num && boundTestDecisionDagNode.WhenTrue != null)
						{
							stack.Push(boundTestDecisionDagNode.WhenTrue);
						}
					}
				}
				continue;
				IL_018e:
				pathBuilder.Pop();
			}
			while (stack.Count > count);
			return true;
		}
	}

	internal static string SamplePatternForPathToDagNode(BoundDagTemp rootIdentifier, ImmutableArray<BoundDecisionDagNode> nodes, BoundDecisionDagNode targetNode, bool nullPaths, out bool requiresFalseWhenClause, out bool unnamedEnumValue)
	{
		unnamedEnumValue = false;
		ImmutableArray<BoundDecisionDagNode> pathToNode = ShortestPathToNode(nodes, targetNode, nullPaths, out requiresFalseWhenClause);
		gatherConstraintsAndEvaluations(targetNode, pathToNode, out var constraints, out var evaluations);
		try
		{
			return SamplePatternForTemp(rootIdentifier, constraints, evaluations, requireExactType: false, ref unnamedEnumValue);
		}
		catch (NoRemainingValuesException)
		{
		}
		return samplePatternFromOtherPaths(rootIdentifier, nodes[0], targetNode, nullPaths, out requiresFalseWhenClause, out unnamedEnumValue);
		static void gatherConstraintsAndEvaluations(BoundDecisionDagNode boundDecisionDagNode3, ImmutableArray<BoundDecisionDagNode> immutableArray, out Dictionary<BoundDagTemp, ArrayBuilder<(BoundDagTest, bool)>> reference, out Dictionary<BoundDagTemp, ArrayBuilder<BoundDagEvaluation>> reference2)
		{
			reference = new Dictionary<BoundDagTemp, ArrayBuilder<(BoundDagTest, bool)>>();
			reference2 = new Dictionary<BoundDagTemp, ArrayBuilder<BoundDagEvaluation>>();
			int i = 0;
			for (int length = immutableArray.Length; i < length; i++)
			{
				BoundDecisionDagNode boundDecisionDagNode = immutableArray[i];
				if (!(boundDecisionDagNode is BoundTestDecisionDagNode boundTestDecisionDagNode))
				{
					if (boundDecisionDagNode is BoundEvaluationDecisionDagNode boundEvaluationDecisionDagNode)
					{
						BoundDagTemp input = boundEvaluationDecisionDagNode.Evaluation.Input;
						if (!reference2.TryGetValue(input, out var value))
						{
							reference2.Add(input, value = new ArrayBuilder<BoundDagEvaluation>());
						}
						value.Add(boundEvaluationDecisionDagNode.Evaluation);
					}
				}
				else
				{
					BoundDecisionDagNode boundDecisionDagNode2 = ((i < length - 1) ? immutableArray[i + 1] : boundDecisionDagNode3);
					bool flag = boundTestDecisionDagNode.WhenTrue == boundDecisionDagNode2 || (boundTestDecisionDagNode.WhenFalse != boundDecisionDagNode2 && boundTestDecisionDagNode.WhenTrue is BoundWhenDecisionDagNode);
					BoundDagTest test = boundTestDecisionDagNode.Test;
					BoundDagTemp input2 = test.Input;
					if (!(test is BoundDagTypeTest) || flag)
					{
						if (!reference.TryGetValue(input2, out var value2))
						{
							reference.Add(input2, value2 = new ArrayBuilder<(BoundDagTest, bool)>());
						}
						value2.Add((test, flag));
					}
				}
			}
		}
		static string samplePatternFromOtherPaths(BoundDagTemp input, BoundDecisionDagNode rootNode, BoundDecisionDagNode targetNode2, bool nullPaths2, out bool reference2, out bool reference)
		{
			string altSamplePatternForTemp = null;
			bool altRequiresFalseWhenClause = false;
			bool altUnnamedEnumValue = false;
			VisitPathsToNode(rootNode, targetNode2, nullPaths2, delegate(ImmutableArray<BoundDecisionDagNode> currentPathToNode, bool currentRequiresFalseWhenClause)
			{
				altRequiresFalseWhenClause = currentRequiresFalseWhenClause;
				gatherConstraintsAndEvaluations(targetNode2, currentPathToNode, out var constraints2, out var evaluations2);
				try
				{
					altUnnamedEnumValue = false;
					altSamplePatternForTemp = SamplePatternForTemp(input, constraints2, evaluations2, requireExactType: false, ref altUnnamedEnumValue);
					return false;
				}
				catch (NoRemainingValuesException)
				{
					return true;
				}
			});
			if (altSamplePatternForTemp != null)
			{
				reference = altUnnamedEnumValue;
				reference2 = altRequiresFalseWhenClause;
				return altSamplePatternForTemp;
			}
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/PatternExplainer.cs", 281);
		}
	}

	private static string SamplePatternForTemp(BoundDagTemp input, Dictionary<BoundDagTemp, ArrayBuilder<(BoundDagTest test, bool sense)>> constraintMap, Dictionary<BoundDagTemp, ArrayBuilder<BoundDagEvaluation>> evaluationMap, bool requireExactType, ref bool unnamedEnumValue)
	{
		ImmutableArray<(BoundDagTest test, bool sense)> constraints = getArray<(BoundDagTest, bool)>(constraintMap, input);
		ImmutableArray<BoundDagEvaluation> evaluations = getArray<BoundDagEvaluation>(evaluationMap, input);
		return tryHandleSingleTest() ?? tryHandleTypeTestAndTypeEvaluation(ref unnamedEnumValue) ?? tryHandleUnboxNullableValueType(ref unnamedEnumValue) ?? tryHandleTuplePattern(ref unnamedEnumValue) ?? tryHandleNumericLimits(ref unnamedEnumValue) ?? tryHandleRecursivePattern(ref unnamedEnumValue) ?? tryHandleListPattern(ref unnamedEnumValue) ?? produceFallbackPattern();
		static IValueSet computeRemainingValues(IValueSetFactory fac, ImmutableArray<(BoundDagTest test, bool sense)> immutableArray)
		{
			IValueSet remainingValues = fac.AllValues;
			foreach (var item2 in immutableArray)
			{
				var (boundDagTest, sense) = item2;
				if (!(boundDagTest is BoundDagValueTest boundDagValueTest))
				{
					if (boundDagTest is BoundDagRelationalTest boundDagRelationalTest)
					{
						addRelation(boundDagRelationalTest.Relation, boundDagRelationalTest.Value);
					}
				}
				else
				{
					addRelation(BinaryOperatorKind.Equal, boundDagValueTest.Value);
				}
				void addRelation(BinaryOperatorKind relation, ConstantValue value)
				{
					if (!value.IsBad)
					{
						IValueSet valueSet = fac.Related(relation, value);
						if (!sense)
						{
							valueSet = valueSet.Complement();
						}
						remainingValues = remainingValues.Intersect(valueSet);
					}
				}
			}
			return remainingValues;
		}
		static ImmutableArray<T> getArray<T>(Dictionary<BoundDagTemp, ArrayBuilder<T>> map, BoundDagTemp temp)
		{
			if (!map.TryGetValue(temp, out var value))
			{
				return ImmutableArray<T>.Empty;
			}
			return value.ToImmutable();
		}
		static bool isNotNullTest((BoundDagTest test, bool sense) constraint)
		{
			var (boundDagTest, _) = constraint;
			if (boundDagTest is BoundDagNonNullTest)
			{
				if (constraint.sense)
				{
					goto IL_0029;
				}
			}
			else if (boundDagTest is BoundDagExplicitNullTest && !constraint.sense)
			{
				goto IL_0029;
			}
			return false;
			IL_0029:
			return true;
		}
		static string makeConjunct(string oldPattern, string newPattern)
		{
			if (oldPattern == "_")
			{
				return newPattern;
			}
			if (newPattern == "_")
			{
				return oldPattern;
			}
			return oldPattern + " and " + newPattern;
		}
		string produceFallbackPattern()
		{
			if (!requireExactType)
			{
				return "_";
			}
			return input.Type.ToDisplayString();
		}
		string tryHandleListPattern(ref bool unnamedEnumValue2)
		{
			if (constraints.IsEmpty && evaluations.IsEmpty)
			{
				return null;
			}
			if (!constraints.All(isNotNullTest))
			{
				return null;
			}
			if (evaluations[0] is BoundDagPropertyEvaluation { IsLengthOrCount: not false } boundDagPropertyEvaluation)
			{
				BoundDagSliceEvaluation boundDagSliceEvaluation = null;
				for (int i = 1; i < evaluations.Length; i++)
				{
					BoundDagEvaluation boundDagEvaluation = evaluations[i];
					if (!(boundDagEvaluation is BoundDagIndexerEvaluation))
					{
						if (!(boundDagEvaluation is BoundDagSliceEvaluation boundDagSliceEvaluation2))
						{
							return null;
						}
						if (boundDagSliceEvaluation != null)
						{
							return null;
						}
						boundDagSliceEvaluation = boundDagSliceEvaluation2;
					}
				}
				BoundDagTemp temp = new BoundDagTemp(boundDagPropertyEvaluation.Syntax, boundDagPropertyEvaluation.Property.Type, boundDagPropertyEvaluation);
				IValueSet<int> valueSet = (IValueSet<int>)computeRemainingValues(ValueSetFactory.ForLength, getArray<(BoundDagTest, bool)>(constraintMap, temp));
				int int32Value = valueSet.Sample.Int32Value;
				if (boundDagSliceEvaluation != null)
				{
					if (valueSet.All(BinaryOperatorKind.Equal, int32Value))
					{
						return null;
					}
					if (boundDagSliceEvaluation.StartIndex - boundDagSliceEvaluation.EndIndex > int32Value)
					{
						return null;
					}
				}
				ArrayBuilder<string> arrayBuilder = new ArrayBuilder<string>(int32Value);
				arrayBuilder.AddMany("_", int32Value);
				for (int j = 1; j < evaluations.Length; j++)
				{
					BoundDagEvaluation boundDagEvaluation2 = evaluations[j];
					if (!(boundDagEvaluation2 is BoundDagIndexerEvaluation boundDagIndexerEvaluation))
					{
						if (!(boundDagEvaluation2 is BoundDagSliceEvaluation))
						{
							throw ExceptionUtilities.UnexpectedValue(boundDagEvaluation2);
						}
					}
					else
					{
						BoundDagTemp input2 = new BoundDagTemp(boundDagIndexerEvaluation.Syntax, boundDagIndexerEvaluation.IndexerType, boundDagIndexerEvaluation);
						int index = boundDagIndexerEvaluation.Index;
						int num = ((index < 0) ? (int32Value + index) : index);
						if (num < 0 || num >= int32Value)
						{
							return null;
						}
						string oldPattern = arrayBuilder[num];
						string newPattern = SamplePatternForTemp(input2, constraintMap, evaluationMap, requireExactType: false, ref unnamedEnumValue2);
						arrayBuilder[num] = makeConjunct(oldPattern, newPattern);
					}
				}
				if (boundDagSliceEvaluation != null)
				{
					string text = SamplePatternForTemp(new BoundDagTemp(boundDagSliceEvaluation.Syntax, boundDagSliceEvaluation.SliceType, boundDagSliceEvaluation), constraintMap, evaluationMap, requireExactType: false, ref unnamedEnumValue2);
					if (text != "_")
					{
						arrayBuilder.Insert(boundDagSliceEvaluation.StartIndex, ".. " + text);
					}
				}
				return "[" + string.Join(", ", arrayBuilder) + "]";
			}
			return null;
		}
		string tryHandleNumericLimits(ref bool unnamedEnumValue2)
		{
			if (evaluations.IsEmpty && constraints.All(delegate((BoundDagTest test, bool sense) t)
			{
				var (boundDagTest, _) = t;
				if (boundDagTest is BoundDagValueTest)
				{
					return true;
				}
				if (boundDagTest is BoundDagRelationalTest)
				{
					return true;
				}
				if (boundDagTest is BoundDagExplicitNullTest)
				{
					if (!t.sense)
					{
						return true;
					}
				}
				else if (boundDagTest is BoundDagNonNullTest && t.sense)
				{
					return true;
				}
				return false;
			}))
			{
				IValueSetFactory valueSetFactory = ValueSetFactory.ForInput(input);
				if (valueSetFactory != null)
				{
					IValueSet valueSet = computeRemainingValues(valueSetFactory, constraints);
					if (valueSet.Complement().IsEmpty)
					{
						return "_";
					}
					return SampleValueString(valueSet, input.Type, requireExactType, ref unnamedEnumValue2);
				}
			}
			return null;
		}
		string tryHandleRecursivePattern(ref bool unnamedEnumValue2)
		{
			if (constraints.IsEmpty && evaluations.IsEmpty)
			{
				return null;
			}
			if (!constraints.All(isNotNullTest))
			{
				return null;
			}
			string text = null;
			Dictionary<Symbol, string> dictionary = new Dictionary<Symbol, string>();
			bool flag = false;
			foreach (BoundDagEvaluation item3 in evaluations)
			{
				if (!(item3 is BoundDagDeconstructEvaluation boundDagDeconstructEvaluation))
				{
					if (!(item3 is BoundDagFieldEvaluation boundDagFieldEvaluation))
					{
						if (!(item3 is BoundDagPropertyEvaluation boundDagPropertyEvaluation))
						{
							return null;
						}
						string value = SamplePatternForTemp(new BoundDagTemp(boundDagPropertyEvaluation.Syntax, boundDagPropertyEvaluation.Property.Type, boundDagPropertyEvaluation), constraintMap, evaluationMap, requireExactType: false, ref unnamedEnumValue2);
						dictionary.Add(boundDagPropertyEvaluation.Property, value);
					}
					else
					{
						string value2 = SamplePatternForTemp(new BoundDagTemp(boundDagFieldEvaluation.Syntax, boundDagFieldEvaluation.Field.Type, boundDagFieldEvaluation), constraintMap, evaluationMap, requireExactType: false, ref unnamedEnumValue2);
						dictionary.Add(boundDagFieldEvaluation.Field, value2);
					}
				}
				else
				{
					MethodSymbol deconstructMethod = boundDagDeconstructEvaluation.DeconstructMethod;
					int num = ((!deconstructMethod.RequiresInstanceReceiver) ? 1 : 0);
					int num2 = deconstructMethod.Parameters.Length - num;
					StringBuilder stringBuilder = new StringBuilder("(");
					for (int i = 0; i < num2; i++)
					{
						string value3 = SamplePatternForTemp(new BoundDagTemp(boundDagDeconstructEvaluation.Syntax, deconstructMethod.Parameters[i + num].Type, boundDagDeconstructEvaluation, i), constraintMap, evaluationMap, requireExactType: false, ref unnamedEnumValue2);
						if (i != 0)
						{
							stringBuilder.Append(", ");
						}
						stringBuilder.Append(value3);
					}
					stringBuilder.Append(')');
					string text2 = stringBuilder.ToString();
					if ((text != null) & flag)
					{
						text += " { }";
						flag = dictionary.Count != 0;
					}
					text = ((text == null) ? text2 : (text + " and " + text2));
					flag |= num2 == 1;
				}
			}
			string text3 = (requireExactType ? input.Type.ToDisplayString() : null);
			string text4 = ((flag | ((text == null && text3 == null) || dictionary.Count != 0)) ? (((text != null) ? " {" : "{") + string.Join(", ", dictionary.Select((KeyValuePair<Symbol, string> kvp) => " " + kvp.Key.Name + ": " + kvp.Value)) + " }") : null);
			return text3 + text + text4;
		}
		string tryHandleSingleTest()
		{
			if (evaluations.IsEmpty && constraints.Length == 1)
			{
				(BoundDagTest, bool) tuple = constraints[0];
				var (boundDagTest, _) = tuple;
				if (boundDagTest is BoundDagNonNullTest)
				{
					if (tuple.Item2)
					{
						if (!requireExactType)
						{
							return "not null";
						}
						return input.Type.ToDisplayString();
					}
					return "null";
				}
				if (boundDagTest is BoundDagExplicitNullTest)
				{
					if (!tuple.Item2)
					{
						if (!requireExactType)
						{
							return "not null";
						}
						return input.Type.ToDisplayString();
					}
					return "null";
				}
				if (boundDagTest is BoundDagTypeTest boundDagTypeTest)
				{
					TypeSymbol type = boundDagTypeTest.Type;
					bool item = tuple.Item2;
					return type.ToDisplayString();
				}
			}
			return null;
		}
		string tryHandleTuplePattern(ref bool unnamedEnumValue2)
		{
			if (input.Type.IsTupleType && constraints.IsEmpty && evaluations.All(delegate(BoundDagEvaluation e)
			{
				if (e is BoundDagFieldEvaluation boundDagFieldEvaluation2)
				{
					FieldSymbol field = boundDagFieldEvaluation2.Field;
					return field.IsTupleElement();
				}
				return false;
			}))
			{
				int length = input.Type.TupleElements.Length;
				ArrayBuilder<string> arrayBuilder = new ArrayBuilder<string>(length);
				arrayBuilder.AddMany("_", length);
				foreach (BoundDagFieldEvaluation item4 in evaluations)
				{
					BoundDagTemp input2 = new BoundDagTemp(item4.Syntax, item4.Field.Type, item4);
					int tupleElementIndex = item4.Field.TupleElementIndex;
					if (tupleElementIndex < 0 || tupleElementIndex >= length)
					{
						return null;
					}
					string oldPattern = arrayBuilder[tupleElementIndex];
					string newPattern = SamplePatternForTemp(input2, constraintMap, evaluationMap, requireExactType: false, ref unnamedEnumValue2);
					arrayBuilder[tupleElementIndex] = makeConjunct(oldPattern, newPattern);
				}
				return "(" + string.Join(", ", arrayBuilder) + ")" + ((arrayBuilder.Count == 1) ? " { }" : null);
			}
			return null;
		}
		string tryHandleTypeTestAndTypeEvaluation(ref bool unnamedEnumValue2)
		{
			if (evaluations.Length == 1 && constraints.Length == 1)
			{
				(BoundDagTest, bool) tuple = constraints[0];
				if (tuple.Item1 is BoundDagTypeTest boundDagTypeTest)
				{
					TypeSymbol type = boundDagTypeTest.Type;
					if (tuple.Item2 && evaluations[0] is BoundDagTypeEvaluation boundDagTypeEvaluation)
					{
						TypeSymbol type2 = boundDagTypeEvaluation.Type;
						if (type.Equals(type2, TypeCompareKind.AllIgnoreOptions))
						{
							return SamplePatternForTemp(new BoundDagTemp(boundDagTypeEvaluation.Syntax, boundDagTypeEvaluation.Type, boundDagTypeEvaluation), constraintMap, evaluationMap, requireExactType: true, ref unnamedEnumValue2);
						}
					}
				}
			}
			return null;
		}
		string tryHandleUnboxNullableValueType(ref bool unnamedEnumValue2)
		{
			if (evaluations.Length == 1 && constraints.Length == 1)
			{
				(BoundDagTest, bool) tuple = constraints[0];
				if (tuple.Item1 is BoundDagNonNullTest && tuple.Item2 && evaluations[0] is BoundDagTypeEvaluation boundDagTypeEvaluation)
				{
					TypeSymbol type = boundDagTypeEvaluation.Type;
					if (input.Type.IsNullableType() && input.Type.GetNullableUnderlyingType().Equals(type, TypeCompareKind.AllIgnoreOptions))
					{
						string text = SamplePatternForTemp(new BoundDagTemp(boundDagTypeEvaluation.Syntax, boundDagTypeEvaluation.Type, boundDagTypeEvaluation), constraintMap, evaluationMap, requireExactType: false, ref unnamedEnumValue2);
						if (!(text == "_"))
						{
							return text;
						}
						return "not null";
					}
				}
			}
			return null;
		}
	}

	private static string SampleValueString(IValueSet remainingValues, TypeSymbol type, bool requireExactType, ref bool unnamedEnumValue)
	{
		if (remainingValues.IsEmpty)
		{
			throw new NoRemainingValuesException();
		}
		if (type is NamedTypeSymbol namedTypeSymbol && type.TypeKind == TypeKind.Enum)
		{
			foreach (Symbol member in namedTypeSymbol.GetMembers())
			{
				if (member is FieldSymbol { IsConst: not false } fieldSymbol && member.IsStatic && member.DeclaredAccessibility == Accessibility.Public)
				{
					ConstantValue constantValue = fieldSymbol.GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false);
					if ((object)constantValue != null && remainingValues.Any(BinaryOperatorKind.Equal, constantValue))
					{
						return fieldSymbol.ToDisplayString();
					}
				}
			}
			unnamedEnumValue = true;
		}
		ConstantValue sample = remainingValues.Sample;
		if (sample != null)
		{
			return ValueString(sample, type, requireExactType);
		}
		TypeSymbol typeSymbol = type.EnumUnderlyingTypeOrSelf();
		if (typeSymbol.SpecialType == SpecialType.System_IntPtr)
		{
			if (remainingValues.Any(BinaryOperatorKind.GreaterThan, ConstantValue.Create(int.MaxValue)))
			{
				return "> (" + type.ToDisplayString() + ")int.MaxValue";
			}
			if (remainingValues.Any(BinaryOperatorKind.LessThan, ConstantValue.Create(int.MinValue)))
			{
				return "< (" + type.ToDisplayString() + ")int.MinValue";
			}
		}
		else if (typeSymbol.SpecialType == SpecialType.System_UIntPtr && remainingValues.Any(BinaryOperatorKind.GreaterThan, ConstantValue.Create(uint.MaxValue)))
		{
			return "> (" + type.ToDisplayString() + ")uint.MaxValue";
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/PatternExplainer.cs", 728);
	}

	private static string ValueString(ConstantValue value, TypeSymbol type, bool requireExactType)
	{
		bool num = ((type.IsEnumType() | requireExactType) || type.IsNativeIntegerType) && (!typeHasExactTypeLiteral(type) || value.IsNull);
		string text = PrimitiveValueString(value, type.EnumUnderlyingTypeOrSelf());
		if (!num)
		{
			return text;
		}
		return "(" + type.ToDisplayString() + ")" + text;
		static bool typeHasExactTypeLiteral(TypeSymbol typeSymbol)
		{
			return typeSymbol.SpecialType switch
			{
				SpecialType.System_Int32 => true, 
				SpecialType.System_Int64 => true, 
				SpecialType.System_UInt32 => true, 
				SpecialType.System_UInt64 => true, 
				SpecialType.System_String => true, 
				SpecialType.System_Decimal => true, 
				SpecialType.System_Single => true, 
				SpecialType.System_Double => true, 
				SpecialType.System_Boolean => true, 
				SpecialType.System_Char => true, 
				_ => false, 
			};
		}
	}

	private static string PrimitiveValueString(ConstantValue value, TypeSymbol type)
	{
		if (value.IsNull)
		{
			return "null";
		}
		switch (type.SpecialType)
		{
		case SpecialType.System_IntPtr:
			if (!type.IsNativeIntegerType)
			{
				break;
			}
			goto case SpecialType.System_Boolean;
		case SpecialType.System_UIntPtr:
			if (!type.IsNativeIntegerType)
			{
				break;
			}
			goto case SpecialType.System_Boolean;
		case SpecialType.System_Boolean:
		case SpecialType.System_Char:
		case SpecialType.System_SByte:
		case SpecialType.System_Byte:
		case SpecialType.System_Int16:
		case SpecialType.System_UInt16:
		case SpecialType.System_Int32:
		case SpecialType.System_UInt32:
		case SpecialType.System_Int64:
		case SpecialType.System_UInt64:
		case SpecialType.System_Decimal:
		case SpecialType.System_String:
			return ObjectDisplay.FormatPrimitive(value.Value, ObjectDisplayOptions.IncludeTypeSuffix | ObjectDisplayOptions.UseQuotes | ObjectDisplayOptions.EscapeNonPrintableCharacters);
		case SpecialType.System_Single:
		{
			float singleValue = value.SingleValue;
			if (!float.IsNaN(singleValue))
			{
				if (singleValue != float.NegativeInfinity)
				{
					if (singleValue == float.PositiveInfinity)
					{
						return "float.PositiveInfinity";
					}
					return ObjectDisplay.FormatPrimitive(singleValue, ObjectDisplayOptions.IncludeTypeSuffix);
				}
				return "float.NegativeInfinity";
			}
			return "float.NaN";
		}
		case SpecialType.System_Double:
		{
			double doubleValue = value.DoubleValue;
			if (!double.IsNaN(doubleValue))
			{
				if (doubleValue != double.NegativeInfinity)
				{
					if (doubleValue == double.PositiveInfinity)
					{
						return "double.PositiveInfinity";
					}
					return ObjectDisplay.FormatPrimitive(doubleValue, ObjectDisplayOptions.IncludeTypeSuffix);
				}
				return "double.NegativeInfinity";
			}
			return "double.NaN";
		}
		}
		return "_";
	}
}
