using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.RuntimeMembers;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class LocalRewriter : BoundTreeRewriterWithStackGuard
{
	private abstract class DecisionDagRewriter : PatternLocalRewriter
	{
		protected sealed class WhenClauseMightAssignPatternVariableWalker : BoundTreeWalkerWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator
		{
			private bool _mightAssignSomething;

			public bool MightAssignSomething(BoundExpression expr)
			{
				if (expr == null)
				{
					return false;
				}
				_mightAssignSomething = false;
				Visit(expr);
				return _mightAssignSomething;
			}

			public override BoundNode Visit(BoundNode node)
			{
				if (node is BoundExpression { ConstantValueOpt: not null })
				{
					return null;
				}
				if (!_mightAssignSomething)
				{
					return base.Visit(node);
				}
				return null;
			}

			protected override void VisitArguments(BoundCall node)
			{
				if (node.Method.MethodKind == MethodKind.LocalFunction || !node.ArgumentRefKindsOpt.IsDefault || MethodMayMutateReceiver(node.ReceiverOpt, node.Method))
				{
					_mightAssignSomething = true;
				}
				else
				{
					base.VisitArguments(node);
				}
			}

			private static bool MethodMayMutateReceiver(BoundExpression receiver, MethodSymbol method)
			{
				if (method != null && !method.IsStatic && !method.IsEffectivelyReadOnly)
				{
					TypeSymbol? type = receiver.Type;
					if ((object)type != null && !type.IsReferenceType)
					{
						return !method.ContainingType.SpecialType.IsPrimitiveRecursiveStruct();
					}
				}
				return false;
			}

			public override BoundNode VisitPropertyAccess(BoundPropertyAccess node)
			{
				if (MethodMayMutateReceiver(node.ReceiverOpt, node.PropertySymbol.GetMethod))
				{
					_mightAssignSomething = true;
				}
				else
				{
					base.VisitPropertyAccess(node);
				}
				return null;
			}

			public override BoundNode VisitAssignmentOperator(BoundAssignmentOperator node)
			{
				_mightAssignSomething = true;
				return null;
			}

			public override BoundNode VisitCompoundAssignmentOperator(BoundCompoundAssignmentOperator node)
			{
				_mightAssignSomething = true;
				return null;
			}

			public override BoundNode VisitConversion(BoundConversion node)
			{
				visitConversion(node.Conversion);
				if (!_mightAssignSomething)
				{
					base.VisitConversion(node);
				}
				return null;
				void visitConversion(Conversion conversion)
				{
					if (conversion.Kind == ConversionKind.MethodGroup)
					{
						if (conversion.Method.MethodKind == MethodKind.LocalFunction)
						{
							_mightAssignSomething = true;
						}
					}
					else if (!conversion.UnderlyingConversions.IsDefault)
					{
						foreach (Conversion underlyingConversion in conversion.UnderlyingConversions)
						{
							visitConversion(underlyingConversion);
							if (_mightAssignSomething)
							{
								break;
							}
						}
					}
				}
			}

			public override BoundNode VisitDelegateCreationExpression(BoundDelegateCreationExpression node)
			{
				MethodSymbol? methodOpt = node.MethodOpt;
				if ((object)methodOpt != null && methodOpt.MethodKind == MethodKind.LocalFunction)
				{
					_mightAssignSomething = true;
				}
				else
				{
					base.VisitDelegateCreationExpression(node);
				}
				return null;
			}

			public override BoundNode VisitAddressOfOperator(BoundAddressOfOperator node)
			{
				_mightAssignSomething = true;
				return null;
			}

			public override BoundNode VisitDeconstructionAssignmentOperator(BoundDeconstructionAssignmentOperator node)
			{
				_mightAssignSomething = true;
				return null;
			}

			public override BoundNode VisitIncrementOperator(BoundIncrementOperator node)
			{
				_mightAssignSomething = true;
				return null;
			}

			public override BoundNode VisitDynamicInvocation(BoundDynamicInvocation node)
			{
				if (!node.ArgumentRefKindsOpt.IsDefault)
				{
					_mightAssignSomething = true;
				}
				else
				{
					base.VisitDynamicInvocation(node);
				}
				return null;
			}

			public override BoundNode VisitObjectCreationExpression(BoundObjectCreationExpression node)
			{
				if (!node.ArgumentRefKindsOpt.IsDefault)
				{
					_mightAssignSomething = true;
				}
				else
				{
					base.VisitObjectCreationExpression(node);
				}
				return null;
			}

			public override BoundNode VisitDynamicObjectCreationExpression(BoundDynamicObjectCreationExpression node)
			{
				if (!node.ArgumentRefKindsOpt.IsDefault)
				{
					_mightAssignSomething = true;
				}
				else
				{
					base.VisitDynamicObjectCreationExpression(node);
				}
				return null;
			}

			public override BoundNode VisitObjectInitializerMember(BoundObjectInitializerMember node)
			{
				if (!node.ArgumentRefKindsOpt.IsDefault)
				{
					_mightAssignSomething = true;
				}
				else
				{
					base.VisitObjectInitializerMember(node);
				}
				return null;
			}

			public override BoundNode VisitIndexerAccess(BoundIndexerAccess node)
			{
				if (!node.ArgumentRefKindsOpt.IsDefault || MethodMayMutateReceiver(node.ReceiverOpt, node.Indexer.GetMethod))
				{
					_mightAssignSomething = true;
				}
				else
				{
					base.VisitIndexerAccess(node);
				}
				return null;
			}

			public override BoundNode VisitDynamicIndexerAccess(BoundDynamicIndexerAccess node)
			{
				if (!node.ArgumentRefKindsOpt.IsDefault)
				{
					_mightAssignSomething = true;
				}
				else
				{
					base.VisitDynamicIndexerAccess(node);
				}
				return null;
			}
		}

		private sealed class CasesComparer : IComparer<(ConstantValue value, LabelSymbol label)>
		{
			private readonly IValueSetFactory _fac;

			public CasesComparer(TypeSymbol type)
			{
				_fac = ValueSetFactory.ForType(type);
			}

			int IComparer<(ConstantValue value, LabelSymbol label)>.Compare((ConstantValue value, LabelSymbol label) left, (ConstantValue value, LabelSymbol label) right)
			{
				var (constantValue, _) = left;
				var (constantValue2, _) = right;
				if (!isNaN(constantValue))
				{
					if (!isNaN(constantValue2))
					{
						if (!_fac.Related(BinaryOperatorKind.LessThanOrEqual, constantValue, constantValue2))
						{
							return 1;
						}
						if (!_fac.Related(BinaryOperatorKind.LessThanOrEqual, constantValue2, constantValue))
						{
							return -1;
						}
						return 0;
					}
					return -1;
				}
				return 1;
				static bool isNaN(ConstantValue value)
				{
					if (value.Discriminator == ConstantValueTypeDiscriminator.Single || value.Discriminator == ConstantValueTypeDiscriminator.Double)
					{
						return double.IsNaN(value.DoubleValue);
					}
					return false;
				}
			}
		}

		private enum StringPatternInput
		{
			String,
			SpanChar,
			ReadOnlySpanChar
		}

		private abstract class ValueDispatchNode
		{
			internal sealed class SwitchDispatch : ValueDispatchNode
			{
				public readonly ImmutableArray<(ConstantValue value, LabelSymbol label)> Cases;

				public readonly LabelSymbol Otherwise;

				public SwitchDispatch(SyntaxNode syntax, ImmutableArray<(ConstantValue value, LabelSymbol label)> dispatches, LabelSymbol otherwise)
					: base(syntax)
				{
					Cases = dispatches;
					Otherwise = otherwise;
				}

				public override string ToString()
				{
					return "[" + string.Join(",", Cases.Select(((ConstantValue value, LabelSymbol label) c) => c.value)) + "]";
				}
			}

			internal sealed class LeafDispatchNode : ValueDispatchNode
			{
				public readonly LabelSymbol Label;

				public LeafDispatchNode(SyntaxNode syntax, LabelSymbol Label)
					: base(syntax)
				{
					this.Label = Label;
				}

				public override string ToString()
				{
					return "Leaf";
				}
			}

			internal sealed class RelationalDispatch : ValueDispatchNode
			{
				private int _height;

				public readonly ConstantValue Value;

				public readonly BinaryOperatorKind Operator;

				protected override int Height => _height;

				private ValueDispatchNode Left { get; set; }

				private ValueDispatchNode Right { get; set; }

				public ValueDispatchNode WhenTrue
				{
					get
					{
						if (!IsReversed(Operator))
						{
							return Left;
						}
						return Right;
					}
				}

				public ValueDispatchNode WhenFalse
				{
					get
					{
						if (!IsReversed(Operator))
						{
							return Right;
						}
						return Left;
					}
				}

				private RelationalDispatch(SyntaxNode syntax, ConstantValue value, BinaryOperatorKind op, ValueDispatchNode left, ValueDispatchNode right)
					: base(syntax)
				{
					Value = value;
					Operator = op;
					WithLeftAndRight(left, right);
				}

				public override string ToString()
				{
					return $"RelationalDispatch.{Height}({Left} {Operator.Operator()} {Value} {Right})";
				}

				private static bool IsReversed(BinaryOperatorKind op)
				{
					return op.Operator() switch
					{
						BinaryOperatorKind.GreaterThan => true, 
						BinaryOperatorKind.GreaterThanOrEqual => true, 
						_ => false, 
					};
				}

				private RelationalDispatch WithLeftAndRight(ValueDispatchNode left, ValueDispatchNode right)
				{
					int height = left.Height;
					int height2 = right.Height;
					Left = left;
					Right = right;
					_height = Math.Max(height, height2) + 1;
					return this;
				}

				public RelationalDispatch WithTrueAndFalseChildren(ValueDispatchNode whenTrue, ValueDispatchNode whenFalse)
				{
					if (whenTrue == WhenTrue && whenFalse == WhenFalse)
					{
						return this;
					}
					ValueDispatchNode right;
					ValueDispatchNode left;
					if (!IsReversed(Operator))
					{
						ValueDispatchNode valueDispatchNode = whenFalse;
						right = valueDispatchNode;
						left = whenTrue;
					}
					else
					{
						ValueDispatchNode valueDispatchNode = whenTrue;
						right = valueDispatchNode;
						left = whenFalse;
					}
					return WithLeftAndRight(left, right);
				}

				public static ValueDispatchNode CreateBalanced(SyntaxNode syntax, ConstantValue value, BinaryOperatorKind op, ValueDispatchNode whenTrue, ValueDispatchNode whenFalse)
				{
					ValueDispatchNode right;
					ValueDispatchNode left;
					if (!IsReversed(op))
					{
						ValueDispatchNode valueDispatchNode = whenFalse;
						right = valueDispatchNode;
						left = whenTrue;
					}
					else
					{
						ValueDispatchNode valueDispatchNode = whenTrue;
						right = valueDispatchNode;
						left = whenFalse;
					}
					return CreateBalancedCore(syntax, value, op, left, right);
				}

				private static ValueDispatchNode CreateBalancedCore(SyntaxNode syntax, ConstantValue value, BinaryOperatorKind op, ValueDispatchNode left, ValueDispatchNode right)
				{
					if (left.Height > right.Height + 1)
					{
						RelationalDispatch relationalDispatch = (RelationalDispatch)left;
						ValueDispatchNode valueDispatchNode = CreateBalancedCore(syntax, value, op, relationalDispatch.Right, right);
						SyntaxNode syntax2 = relationalDispatch.Syntax;
						ConstantValue value2 = relationalDispatch.Value;
						BinaryOperatorKind num = relationalDispatch.Operator;
						ValueDispatchNode left2 = relationalDispatch.Left;
						right = valueDispatchNode;
						left = left2;
						op = num;
						value = value2;
						syntax = syntax2;
					}
					else if (right.Height > left.Height + 1)
					{
						RelationalDispatch relationalDispatch2 = (RelationalDispatch)right;
						ValueDispatchNode valueDispatchNode2 = CreateBalancedCore(syntax, value, op, left, relationalDispatch2.Left);
						SyntaxNode syntax3 = relationalDispatch2.Syntax;
						ConstantValue value3 = relationalDispatch2.Value;
						BinaryOperatorKind num2 = relationalDispatch2.Operator;
						right = relationalDispatch2.Right;
						left = valueDispatchNode2;
						op = num2;
						value = value3;
						syntax = syntax3;
					}
					if (left.Height == right.Height + 2)
					{
						RelationalDispatch relationalDispatch3 = (RelationalDispatch)left;
						if (relationalDispatch3.Left.Height == right.Height)
						{
							RelationalDispatch relationalDispatch4 = relationalDispatch3;
							ValueDispatchNode left3 = relationalDispatch4.Left;
							RelationalDispatch obj = (RelationalDispatch)relationalDispatch4.Right;
							ValueDispatchNode left4 = obj.Left;
							ValueDispatchNode right2 = obj.Right;
							ValueDispatchNode right3 = right;
							return obj.WithLeftAndRight(relationalDispatch4.WithLeftAndRight(left3, left4), new RelationalDispatch(syntax, value, op, right2, right3));
						}
						ValueDispatchNode left5 = relationalDispatch3.Left;
						ValueDispatchNode right4 = relationalDispatch3.Right;
						ValueDispatchNode right5 = right;
						return relationalDispatch3.WithLeftAndRight(left5, new RelationalDispatch(syntax, value, op, right4, right5));
					}
					if (right.Height == left.Height + 2)
					{
						RelationalDispatch relationalDispatch5 = (RelationalDispatch)right;
						if (relationalDispatch5.Right.Height == left.Height)
						{
							ValueDispatchNode left6 = left;
							RelationalDispatch relationalDispatch6 = relationalDispatch5;
							RelationalDispatch obj2 = (RelationalDispatch)relationalDispatch6.Left;
							ValueDispatchNode left7 = obj2.Left;
							ValueDispatchNode right6 = obj2.Right;
							return obj2.WithLeftAndRight(right: relationalDispatch6.WithLeftAndRight(right6, relationalDispatch6.Right), left: new RelationalDispatch(syntax, value, op, left6, left7));
						}
						ValueDispatchNode left8 = left;
						ValueDispatchNode left9 = relationalDispatch5.Left;
						return relationalDispatch5.WithLeftAndRight(right: relationalDispatch5.Right, left: new RelationalDispatch(syntax, value, op, left8, left9));
					}
					return new RelationalDispatch(syntax, value, op, left, right);
				}
			}

			public readonly SyntaxNode Syntax;

			protected virtual int Height => 1;

			public ValueDispatchNode(SyntaxNode syntax)
			{
				Syntax = syntax;
			}
		}

		private ArrayBuilder<BoundStatement> _loweredDecisionDag;

		private readonly PooledDictionary<BoundDecisionDagNode, LabelSymbol> _dagNodeLabels = PooledDictionary<BoundDecisionDagNode, LabelSymbol>.GetInstance();

		internal LocalSymbol? _whenNodeIdentifierLocal;

		protected abstract ArrayBuilder<BoundStatement> BuilderForSection(SyntaxNode section);

		protected DecisionDagRewriter(SyntaxNode node, LocalRewriter localRewriter, bool generateInstrumentation)
			: base(node, localRewriter, generateInstrumentation)
		{
		}

		private void ComputeLabelSet(BoundDecisionDag decisionDag)
		{
			PooledHashSet<BoundDecisionDagNode> hasPredecessor = PooledHashSet<BoundDecisionDagNode>.GetInstance();
			foreach (BoundDecisionDagNode topologicallySortedNode in decisionDag.TopologicallySortedNodes)
			{
				if (!(topologicallySortedNode is BoundWhenDecisionDagNode boundWhenDecisionDagNode))
				{
					if (!(topologicallySortedNode is BoundLeafDecisionDagNode boundLeafDecisionDagNode))
					{
						if (!(topologicallySortedNode is BoundEvaluationDecisionDagNode boundEvaluationDecisionDagNode))
						{
							if (!(topologicallySortedNode is BoundTestDecisionDagNode boundTestDecisionDagNode))
							{
								throw ExceptionUtilities.UnexpectedValue(topologicallySortedNode.Kind);
							}
							notePredecessor(boundTestDecisionDagNode.WhenTrue);
							notePredecessor(boundTestDecisionDagNode.WhenFalse);
						}
						else
						{
							notePredecessor(boundEvaluationDecisionDagNode.Next);
						}
					}
					else
					{
						_dagNodeLabels[topologicallySortedNode] = boundLeafDecisionDagNode.Label;
					}
				}
				else
				{
					GetDagNodeLabel(topologicallySortedNode);
					if (boundWhenDecisionDagNode.WhenFalse != null)
					{
						GetDagNodeLabel(boundWhenDecisionDagNode.WhenFalse);
					}
				}
			}
			hasPredecessor.Free();
			void notePredecessor(BoundDecisionDagNode successor)
			{
				if (successor != null && !hasPredecessor.Add(successor))
				{
					GetDagNodeLabel(successor);
				}
			}
		}

		protected new void Free()
		{
			_dagNodeLabels.Free();
			base.Free();
		}

		protected virtual LabelSymbol GetDagNodeLabel(BoundDecisionDagNode dag)
		{
			if (!_dagNodeLabels.TryGetValue(dag, out var value))
			{
				_dagNodeLabels.Add(dag, value = ((dag is BoundLeafDecisionDagNode boundLeafDecisionDagNode) ? boundLeafDecisionDagNode.Label : _factory.GenerateLabel("dagNode")));
			}
			return value;
		}

		protected BoundDecisionDag ShareTempsIfPossibleAndEvaluateInput(BoundDecisionDag decisionDag, BoundExpression loweredSwitchGoverningExpression, ArrayBuilder<BoundStatement> result, out BoundExpression savedInputExpression)
		{
			WhenClauseMightAssignPatternVariableWalker arg = new WhenClauseMightAssignPatternVariableWalker();
			if (!decisionDag.TopologicallySortedNodes.Any((BoundDecisionDagNode node, WhenClauseMightAssignPatternVariableWalker mightAssignWalker) => node is BoundWhenDecisionDagNode boundWhenDecisionDagNode && mightAssignWalker.MightAssignSomething(boundWhenDecisionDagNode.WhenExpression), arg))
			{
				decisionDag = ShareTempsAndEvaluateInput(loweredSwitchGoverningExpression, decisionDag, delegate(BoundExpression expr)
				{
					result.Add(_factory.ExpressionStatement(expr));
				}, out savedInputExpression);
			}
			else
			{
				BoundExpression temp = _tempAllocator.GetTemp(BoundDagTemp.ForOriginalInput(loweredSwitchGoverningExpression));
				result.Add(_factory.Assignment(temp, loweredSwitchGoverningExpression));
				savedInputExpression = temp;
			}
			return decisionDag;
		}

		protected ImmutableArray<BoundStatement> LowerDecisionDagCore(BoundDecisionDag decisionDag)
		{
			_loweredDecisionDag = ArrayBuilder<BoundStatement>.GetInstance();
			ComputeLabelSet(decisionDag);
			ImmutableArray<BoundDecisionDagNode> topologicallySortedNodes = decisionDag.TopologicallySortedNodes;
			BoundDecisionDagNode boundDecisionDagNode = topologicallySortedNodes[0];
			if (boundDecisionDagNode is BoundWhenDecisionDagNode || boundDecisionDagNode is BoundLeafDecisionDagNode)
			{
				_loweredDecisionDag.Add(_factory.Goto(GetDagNodeLabel(boundDecisionDagNode)));
			}
			LowerWhenClauses(topologicallySortedNodes);
			ImmutableArray<BoundDecisionDagNode> nodesToLower = topologicallySortedNodes.WhereAsArray((BoundDecisionDagNode n) => n.Kind != BoundKind.WhenDecisionDagNode && n.Kind != BoundKind.LeafDecisionDagNode);
			PooledHashSet<BoundDecisionDagNode> instance = PooledHashSet<BoundDecisionDagNode>.GetInstance();
			int num = 0;
			for (int length = nodesToLower.Length; num < length; num++)
			{
				BoundDecisionDagNode boundDecisionDagNode2 = nodesToLower[num];
				bool flag = instance.Contains(boundDecisionDagNode2);
				if (flag && !_dagNodeLabels.TryGetValue(boundDecisionDagNode2, out var _))
				{
					continue;
				}
				if (_dagNodeLabels.TryGetValue(boundDecisionDagNode2, out var value2))
				{
					_loweredDecisionDag.Add(_factory.Label(value2));
				}
				if ((flag || !GenerateSwitchDispatch(boundDecisionDagNode2, instance)) && !GenerateTypeTestAndCast(boundDecisionDagNode2, instance, nodesToLower, num))
				{
					BoundDecisionDagNode boundDecisionDagNode3 = ((num + 1 < length) ? nodesToLower[num + 1] : null);
					if (boundDecisionDagNode3 != null && instance.Contains(boundDecisionDagNode3))
					{
						boundDecisionDagNode3 = null;
					}
					LowerDecisionDagNode(boundDecisionDagNode2, boundDecisionDagNode3);
				}
			}
			instance.Free();
			ImmutableArray<BoundStatement> result = _loweredDecisionDag.ToImmutableAndFree();
			_loweredDecisionDag = null;
			return result;
		}

		private bool GenerateTypeTestAndCast(BoundDecisionDagNode node, HashSet<BoundDecisionDagNode> loweredNodes, ImmutableArray<BoundDecisionDagNode> nodesToLower, int indexOfNode)
		{
			if (node is BoundTestDecisionDagNode { WhenTrue: BoundEvaluationDecisionDagNode whenTrue } boundTestDecisionDagNode && TryLowerTypeTestAndCast(boundTestDecisionDagNode.Test, whenTrue.Evaluation, out var sideEffect, out var testExpression))
			{
				BoundDecisionDagNode next = whenTrue.Next;
				BoundDecisionDagNode whenFalse = boundTestDecisionDagNode.WhenFalse;
				bool flag = !_dagNodeLabels.ContainsKey(whenTrue);
				if (flag)
				{
					loweredNodes.Add(whenTrue);
				}
				BoundDecisionDagNode nextNode = ((((indexOfNode + 2 < nodesToLower.Length) & flag) && nodesToLower[indexOfNode + 1] == whenTrue && !loweredNodes.Contains(nodesToLower[indexOfNode + 2])) ? nodesToLower[indexOfNode + 2] : null);
				_loweredDecisionDag.Add(_factory.ExpressionStatement(sideEffect));
				GenerateTest(testExpression, next, whenFalse, nextNode);
				return true;
			}
			return false;
		}

		private void GenerateTest(BoundExpression test, BoundDecisionDagNode whenTrue, BoundDecisionDagNode whenFalse, BoundDecisionDagNode nextNode)
		{
			_factory.Syntax = test.Syntax;
			if (nextNode == whenFalse)
			{
				_loweredDecisionDag.Add(_factory.ConditionalGoto(test, GetDagNodeLabel(whenTrue), jumpIfTrue: true));
				return;
			}
			if (nextNode == whenTrue)
			{
				_loweredDecisionDag.Add(_factory.ConditionalGoto(test, GetDagNodeLabel(whenFalse), jumpIfTrue: false));
				return;
			}
			_loweredDecisionDag.Add(_factory.ConditionalGoto(test, GetDagNodeLabel(whenTrue), jumpIfTrue: true));
			_loweredDecisionDag.Add(_factory.Goto(GetDagNodeLabel(whenFalse)));
		}

		private bool GenerateSwitchDispatch(BoundDecisionDagNode node, HashSet<BoundDecisionDagNode> loweredNodes)
		{
			if (!canGenerateSwitchDispatch(node))
			{
				return false;
			}
			BoundDagTemp input = ((BoundTestDecisionDagNode)node).Test.Input;
			ValueDispatchNode n = GatherValueDispatchNodes(node, loweredNodes, input);
			LowerValueDispatchNode(n, _tempAllocator.GetTemp(input));
			return true;
			bool canDispatch(BoundTestDecisionDagNode test1, BoundTestDecisionDagNode test2)
			{
				if (_dagNodeLabels.ContainsKey(test2))
				{
					return false;
				}
				BoundDagTest test3 = test1.Test;
				BoundDagTest test4 = test2.Test;
				if (!(test3 is BoundDagValueTest) && !(test3 is BoundDagRelationalTest))
				{
					return false;
				}
				if (!(test4 is BoundDagValueTest) && !(test4 is BoundDagRelationalTest))
				{
					return false;
				}
				if (!test3.Input.Equals(test4.Input))
				{
					return false;
				}
				SpecialType specialType = test3.Input.Type.SpecialType;
				if ((uint)(specialType - 18) <= 1u)
				{
					return false;
				}
				return true;
			}
			bool canGenerateSwitchDispatch(BoundDecisionDagNode boundDecisionDagNode)
			{
				if (boundDecisionDagNode is BoundTestDecisionDagNode boundTestDecisionDagNode)
				{
					if (boundTestDecisionDagNode.WhenFalse is BoundTestDecisionDagNode test)
					{
						return canDispatch(boundTestDecisionDagNode, test);
					}
					if (boundTestDecisionDagNode.WhenTrue is BoundTestDecisionDagNode test2)
					{
						BoundTestDecisionDagNode test3 = boundTestDecisionDagNode;
						return canDispatch(test3, test2);
					}
				}
				return false;
			}
		}

		private ValueDispatchNode GatherValueDispatchNodes(BoundDecisionDagNode node, HashSet<BoundDecisionDagNode> loweredNodes, BoundDagTemp input)
		{
			IValueSetFactory fac = ValueSetFactory.ForInput(input);
			return GatherValueDispatchNodes(node, loweredNodes, input, fac);
		}

		private ValueDispatchNode GatherValueDispatchNodes(BoundDecisionDagNode node, HashSet<BoundDecisionDagNode> loweredNodes, BoundDagTemp input, IValueSetFactory fac)
		{
			if (loweredNodes.Contains(node))
			{
				_dagNodeLabels.TryGetValue(node, out var value);
				return new ValueDispatchNode.LeafDispatchNode(node.Syntax, value);
			}
			if (!(node is BoundTestDecisionDagNode boundTestDecisionDagNode) || !boundTestDecisionDagNode.Test.Input.Equals(input))
			{
				LabelSymbol dagNodeLabel = GetDagNodeLabel(node);
				return new ValueDispatchNode.LeafDispatchNode(node.Syntax, dagNodeLabel);
			}
			BoundDagTest test = boundTestDecisionDagNode.Test;
			if (!(test is BoundDagRelationalTest boundDagRelationalTest))
			{
				if (test is BoundDagValueTest boundDagValueTest)
				{
					loweredNodes.Add(boundTestDecisionDagNode);
					ArrayBuilder<(ConstantValue, LabelSymbol)> instance = ArrayBuilder<(ConstantValue, LabelSymbol)>.GetInstance();
					instance.Add((boundDagValueTest.Value, GetDagNodeLabel(boundTestDecisionDagNode.WhenTrue)));
					BoundTestDecisionDagNode boundTestDecisionDagNode2 = boundTestDecisionDagNode;
					while (boundTestDecisionDagNode2.WhenFalse is BoundTestDecisionDagNode { Test: BoundDagValueTest test2 } boundTestDecisionDagNode3 && test2.Input.Equals(input) && !_dagNodeLabels.ContainsKey(boundTestDecisionDagNode3) && !loweredNodes.Contains(boundTestDecisionDagNode3))
					{
						instance.Add((test2.Value, GetDagNodeLabel(boundTestDecisionDagNode3.WhenTrue)));
						loweredNodes.Add(boundTestDecisionDagNode3);
						boundTestDecisionDagNode2 = boundTestDecisionDagNode3;
					}
					ValueDispatchNode otherwise = GatherValueDispatchNodes(boundTestDecisionDagNode2.WhenFalse, loweredNodes, input, fac);
					return PushEqualityTestsIntoTree(boundDagValueTest.Syntax, otherwise, instance.ToImmutableAndFree(), fac);
				}
				LabelSymbol dagNodeLabel2 = GetDagNodeLabel(node);
				return new ValueDispatchNode.LeafDispatchNode(node.Syntax, dagNodeLabel2);
			}
			loweredNodes.Add(boundTestDecisionDagNode);
			ValueDispatchNode whenTrue = GatherValueDispatchNodes(boundTestDecisionDagNode.WhenTrue, loweredNodes, input, fac);
			ValueDispatchNode whenFalse = GatherValueDispatchNodes(boundTestDecisionDagNode.WhenFalse, loweredNodes, input, fac);
			return ValueDispatchNode.RelationalDispatch.CreateBalanced(boundTestDecisionDagNode.Syntax, boundDagRelationalTest.Value, boundDagRelationalTest.OperatorKind, whenTrue, whenFalse);
		}

		private ValueDispatchNode PushEqualityTestsIntoTree(SyntaxNode syntax, ValueDispatchNode otherwise, ImmutableArray<(ConstantValue value, LabelSymbol label)> cases, IValueSetFactory fac)
		{
			if (cases.IsEmpty)
			{
				return otherwise;
			}
			if (!(otherwise is ValueDispatchNode.LeafDispatchNode leafDispatchNode))
			{
				if (!(otherwise is ValueDispatchNode.SwitchDispatch switchDispatch))
				{
					if (otherwise is ValueDispatchNode.RelationalDispatch { Operator: var op, Value: var value, WhenTrue: var whenTrue, WhenFalse: var whenFalse } relationalDispatch)
					{
						(ImmutableArray<(ConstantValue value, LabelSymbol label)> whenTrueCases, ImmutableArray<(ConstantValue value, LabelSymbol label)> whenFalseCases) tuple = splitCases(cases, op, value);
						ImmutableArray<(ConstantValue, LabelSymbol)> item = tuple.whenTrueCases;
						ImmutableArray<(ConstantValue, LabelSymbol)> item2 = tuple.whenFalseCases;
						ValueDispatchNode whenTrue2 = PushEqualityTestsIntoTree(syntax, whenTrue, item, fac);
						ValueDispatchNode whenFalse2 = PushEqualityTestsIntoTree(syntax, whenFalse, item2, fac);
						return relationalDispatch.WithTrueAndFalseChildren(whenTrue2, whenFalse2);
					}
					throw ExceptionUtilities.UnexpectedValue(otherwise);
				}
				return new ValueDispatchNode.SwitchDispatch(switchDispatch.Syntax, switchDispatch.Cases.Concat(cases), switchDispatch.Otherwise);
			}
			return new ValueDispatchNode.SwitchDispatch(syntax, cases, leafDispatchNode.Label);
			(ImmutableArray<(ConstantValue value, LabelSymbol label)> whenTrueCases, ImmutableArray<(ConstantValue value, LabelSymbol label)> whenFalseCases) splitCases(ImmutableArray<(ConstantValue value, LabelSymbol label)> immutableArray, BinaryOperatorKind binaryOperatorKind, ConstantValue right)
			{
				ArrayBuilder<(ConstantValue, LabelSymbol)> instance = ArrayBuilder<(ConstantValue, LabelSymbol)>.GetInstance();
				ArrayBuilder<(ConstantValue, LabelSymbol)> instance2 = ArrayBuilder<(ConstantValue, LabelSymbol)>.GetInstance();
				binaryOperatorKind = binaryOperatorKind.Operator();
				foreach (var item3 in immutableArray)
				{
					(fac.Related(binaryOperatorKind, item3.value, right) ? instance : instance2).Add(item3);
				}
				return (whenTrueCases: instance.ToImmutableAndFree(), whenFalseCases: instance2.ToImmutableAndFree());
			}
		}

		private void LowerValueDispatchNode(ValueDispatchNode n, BoundExpression input)
		{
			if (!(n is ValueDispatchNode.LeafDispatchNode leafDispatchNode))
			{
				if (!(n is ValueDispatchNode.SwitchDispatch node))
				{
					if (!(n is ValueDispatchNode.RelationalDispatch rel))
					{
						throw ExceptionUtilities.UnexpectedValue(n);
					}
					LowerRelationalDispatchNode(rel, input);
				}
				else
				{
					LowerSwitchDispatchNode(node, input);
				}
			}
			else
			{
				_loweredDecisionDag.Add(_factory.Goto(leafDispatchNode.Label));
			}
		}

		private void LowerRelationalDispatchNode(ValueDispatchNode.RelationalDispatch rel, BoundExpression input)
		{
			BoundExpression condition = MakeRelationalTest(rel.Syntax, input, rel.Operator, rel.Value);
			if (rel.WhenTrue is ValueDispatchNode.LeafDispatchNode { Label: var label })
			{
				_loweredDecisionDag.Add(_factory.ConditionalGoto(condition, label, jumpIfTrue: true));
				LowerValueDispatchNode(rel.WhenFalse, input);
				return;
			}
			if (rel.WhenFalse is ValueDispatchNode.LeafDispatchNode { Label: var label2 })
			{
				_loweredDecisionDag.Add(_factory.ConditionalGoto(condition, label2, jumpIfTrue: false));
				LowerValueDispatchNode(rel.WhenTrue, input);
				return;
			}
			LabelSymbol label3 = _factory.GenerateLabel("relationalDispatch");
			_loweredDecisionDag.Add(_factory.ConditionalGoto(condition, label3, jumpIfTrue: false));
			LowerValueDispatchNode(rel.WhenTrue, input);
			_loweredDecisionDag.Add(_factory.Label(label3));
			LowerValueDispatchNode(rel.WhenFalse, input);
		}

		private void LowerSwitchDispatchNode(ValueDispatchNode.SwitchDispatch node, BoundExpression input)
		{
			DecisionDagRewriter decisionDagRewriter = this;
			ValueDispatchNode.SwitchDispatch node2 = node;
			BoundExpression input2 = input;
			LabelSymbol defaultLabel = node2.Otherwise;
			bool flag;
			LengthBasedStringSwitchData lengthBasedStringSwitchDataOpt;
			if (input2.Type.IsValidV6SwitchGoverningType() || input2.Type.IsSpanOrReadOnlySpanChar())
			{
				flag = input2.Type.SpecialType == SpecialType.System_String;
				bool flag2 = input2.Type.IsSpanChar();
				bool flag3 = input2.Type.IsReadOnlySpanChar();
				lengthBasedStringSwitchDataOpt = null;
				if (flag | flag2 | flag3)
				{
					StringPatternInput stringPatternInput = ((!flag) ? (flag2 ? StringPatternInput.SpanChar : StringPatternInput.ReadOnlySpanChar) : StringPatternInput.String);
					if (!_localRewriter._compilation.FeatureDisableLengthBasedSwitch && _factory.Compilation.Options.OptimizationLevel == OptimizationLevel.Release)
					{
						LengthBasedStringSwitchData lengthBasedStringSwitchData = LengthBasedStringSwitchData.Create(node2.Cases);
						if (lengthBasedStringSwitchData.ShouldGenerateLengthBasedSwitch(node2.Cases.Length) && hasLengthBasedDispatchRequiredMembers(stringPatternInput))
						{
							lengthBasedStringSwitchDataOpt = lengthBasedStringSwitchData;
							goto IL_012f;
						}
					}
					EnsureStringHashFunction(node2.Cases.Length, node2.Syntax, stringPatternInput);
					goto IL_012f;
				}
				goto IL_014e;
			}
			BinaryOperatorKind lessThanOrEqualOperator;
			ImmutableArray<(ConstantValue value, LabelSymbol label)> cases2;
			if (input2.Type.IsNativeIntegerType)
			{
				ImmutableArray<(ConstantValue, LabelSymbol)> cases;
				switch (input2.Type.SpecialType)
				{
				case SpecialType.System_IntPtr:
				{
					NamedTypeSymbol namedTypeSymbol2 = _factory.SpecialType(SpecialType.System_Int64);
					Conversion conversion2 = _factory.ClassifyEmitConversion(input2, namedTypeSymbol2);
					input2 = _factory.Convert(namedTypeSymbol2, input2, conversion2);
					cases = node2.Cases.SelectAsArray(((ConstantValue value, LabelSymbol label) p) => (ConstantValue.Create((long)p.value.Int32Value), label: p.label));
					break;
				}
				case SpecialType.System_UIntPtr:
				{
					NamedTypeSymbol namedTypeSymbol = _factory.SpecialType(SpecialType.System_UInt64);
					Conversion conversion = _factory.ClassifyEmitConversion(input2, namedTypeSymbol);
					input2 = _factory.Convert(namedTypeSymbol, input2, conversion);
					cases = node2.Cases.SelectAsArray(((ConstantValue value, LabelSymbol label) p) => (ConstantValue.Create((ulong)p.value.UInt32Value), label: p.label));
					break;
				}
				default:
					throw ExceptionUtilities.UnexpectedValue(input2.Type);
				}
				BoundSwitchDispatch item = new BoundSwitchDispatch(node2.Syntax, input2, cases, defaultLabel, null);
				_loweredDecisionDag.Add(item);
			}
			else
			{
				lessThanOrEqualOperator = input2.Type.SpecialType switch
				{
					SpecialType.System_Single => BinaryOperatorKind.FloatLessThanOrEqual, 
					SpecialType.System_Double => BinaryOperatorKind.DoubleLessThanOrEqual, 
					SpecialType.System_Decimal => BinaryOperatorKind.DecimalLessThanOrEqual, 
					_ => throw ExceptionUtilities.UnexpectedValue(input2.Type.SpecialType), 
				};
				cases2 = node2.Cases.Sort(new CasesComparer(input2.Type));
				lowerFloatDispatch(0, cases2.Length);
			}
			return;
			IL_014e:
			BoundSwitchDispatch item2 = new BoundSwitchDispatch(node2.Syntax, input2, node2.Cases, defaultLabel, lengthBasedStringSwitchDataOpt);
			_loweredDecisionDag.Add(item2);
			return;
			IL_012f:
			if (flag)
			{
				_localRewriter.TryGetSpecialTypeMethod(node2.Syntax, SpecialMember.System_String__op_Equality, out MethodSymbol _);
			}
			goto IL_014e;
			bool hasLengthBasedDispatchRequiredMembers(StringPatternInput stringPatternInput2)
			{
				CSharpCompilation compilation = _localRewriter._compilation;
				Symbol symbol = stringPatternInput2 switch
				{
					StringPatternInput.String => compilation.GetSpecialTypeMember(SpecialMember.System_String__Length), 
					StringPatternInput.SpanChar => compilation.GetWellKnownTypeMember(WellKnownMember.System_Span_T__get_Length), 
					StringPatternInput.ReadOnlySpanChar => compilation.GetWellKnownTypeMember(WellKnownMember.System_ReadOnlySpan_T__get_Length), 
					_ => throw ExceptionUtilities.UnexpectedValue(stringPatternInput2), 
				};
				if ((object)symbol == null || symbol.HasUseSiteError)
				{
					return false;
				}
				Symbol symbol2 = stringPatternInput2 switch
				{
					StringPatternInput.String => compilation.GetSpecialTypeMember(SpecialMember.System_String__Chars), 
					StringPatternInput.SpanChar => compilation.GetWellKnownTypeMember(WellKnownMember.System_Span_T__get_Item), 
					StringPatternInput.ReadOnlySpanChar => compilation.GetWellKnownTypeMember(WellKnownMember.System_ReadOnlySpan_T__get_Item), 
					_ => throw ExceptionUtilities.UnexpectedValue(stringPatternInput2), 
				};
				if ((object)symbol2 == null || symbol2.HasUseSiteError)
				{
					return false;
				}
				return true;
			}
			void lowerFloatDispatch(int firstIndex, int count)
			{
				if (count <= 3)
				{
					int i = firstIndex;
					for (int num = firstIndex + count; i < num; i++)
					{
						_loweredDecisionDag.Add(_factory.ConditionalGoto(MakeValueTest(node2.Syntax, input2, cases2[i].value), cases2[i].label, jumpIfTrue: true));
					}
					_loweredDecisionDag.Add(_factory.Goto(defaultLabel));
				}
				else
				{
					int num2 = count / 2;
					GeneratedLabelSymbol label = _factory.GenerateLabel("greaterThanMidpoint");
					_loweredDecisionDag.Add(_factory.ConditionalGoto(MakeRelationalTest(node2.Syntax, input2, lessThanOrEqualOperator, cases2[firstIndex + num2 - 1].value), label, jumpIfTrue: false));
					lowerFloatDispatch(firstIndex, num2);
					_loweredDecisionDag.Add(_factory.Label(label));
					lowerFloatDispatch(firstIndex + num2, count - num2);
				}
			}
		}

		private void EnsureStringHashFunction(int labelsCount, SyntaxNode syntaxNode, StringPatternInput stringPatternInput)
		{
			PEModuleBuilder emitModule = _localRewriter.EmitModule;
			if (emitModule == null || !SwitchStringJumpTableEmitter.ShouldGenerateHashTableSwitch(labelsCount))
			{
				return;
			}
			SynthesizedPrivateImplementationDetailsType privateImplClass = emitModule.GetPrivateImplClass(syntaxNode, _localRewriter._diagnostics.DiagnosticBag);
			PrivateImplementationDetails privateImplementationDetails = privateImplClass.PrivateImplementationDetails;
			if (privateImplementationDetails.GetMethod(stringPatternInput switch
			{
				StringPatternInput.String => "ComputeStringHash", 
				StringPatternInput.SpanChar => "ComputeReadOnlySpanHash", 
				StringPatternInput.ReadOnlySpanChar => "ComputeSpanHash", 
				_ => throw ExceptionUtilities.UnexpectedValue(stringPatternInput), 
			}) == null)
			{
				Symbol symbol = stringPatternInput switch
				{
					StringPatternInput.String => _localRewriter._compilation.GetSpecialTypeMember(SpecialMember.System_String__Chars), 
					StringPatternInput.SpanChar => _localRewriter._compilation.GetWellKnownTypeMember(WellKnownMember.System_Span_T__get_Item), 
					StringPatternInput.ReadOnlySpanChar => _localRewriter._compilation.GetWellKnownTypeMember(WellKnownMember.System_ReadOnlySpan_T__get_Item), 
					_ => throw ExceptionUtilities.UnexpectedValue(stringPatternInput), 
				};
				if ((object)symbol != null && !symbol.HasUseSiteError)
				{
					TypeSymbol returnType = _factory.SpecialType(SpecialType.System_UInt32);
					TypeSymbol paramType = stringPatternInput switch
					{
						StringPatternInput.String => _factory.SpecialType(SpecialType.System_String), 
						StringPatternInput.SpanChar => _factory.WellKnownType(WellKnownType.System_Span_T).Construct(_factory.SpecialType(SpecialType.System_Char)), 
						StringPatternInput.ReadOnlySpanChar => _factory.WellKnownType(WellKnownType.System_ReadOnlySpan_T).Construct(_factory.SpecialType(SpecialType.System_Char)), 
						_ => throw ExceptionUtilities.UnexpectedValue(stringPatternInput), 
					};
					SynthesizedGlobalMethodSymbol synthesizedGlobalMethodSymbol = stringPatternInput switch
					{
						StringPatternInput.String => new SynthesizedStringSwitchHashMethod(privateImplClass, returnType, paramType), 
						StringPatternInput.SpanChar => new SynthesizedSpanSwitchHashMethod(privateImplClass, returnType, paramType, isReadOnlySpan: false), 
						StringPatternInput.ReadOnlySpanChar => new SynthesizedSpanSwitchHashMethod(privateImplClass, returnType, paramType, isReadOnlySpan: true), 
						_ => throw ExceptionUtilities.UnexpectedValue(stringPatternInput), 
					};
					privateImplClass.PrivateImplementationDetails.TryAddSynthesizedMethod(synthesizedGlobalMethodSymbol.GetCciAdapter());
				}
			}
		}

		private void LowerWhenClauses(ImmutableArray<BoundDecisionDagNode> sortedNodes)
		{
			if (!sortedNodes.Any((BoundDecisionDagNode n) => n.Kind == BoundKind.WhenDecisionDagNode))
			{
				return;
			}
			int num = 0;
			PooledDictionary<BoundExpression, (LabelSymbol LabelToWhenExpression, ArrayBuilder<BoundWhenDecisionDagNode> WhenNodes)> whenExpressionMap = PooledDictionary<BoundExpression, (LabelSymbol, ArrayBuilder<BoundWhenDecisionDagNode>)>.GetInstance();
			PooledDictionary<BoundWhenDecisionDagNode, (LabelSymbol LabelToWhenExpression, int WhenNodeIdentifier)> whenNodeMap = PooledDictionary<BoundWhenDecisionDagNode, (LabelSymbol, int)>.GetInstance();
			foreach (BoundDecisionDagNode item3 in sortedNodes)
			{
				if (!(item3 is BoundWhenDecisionDagNode boundWhenDecisionDagNode))
				{
					continue;
				}
				BoundExpression whenExpression = boundWhenDecisionDagNode.WhenExpression;
				if (whenExpression != null && whenExpression.ConstantValueOpt != ConstantValue.True)
				{
					LabelSymbol item;
					if (whenExpressionMap.TryGetValue(whenExpression, out (LabelSymbol, ArrayBuilder<BoundWhenDecisionDagNode>) value))
					{
						(item, _) = value;
						value.Item2.Add(boundWhenDecisionDagNode);
					}
					else
					{
						item = _factory.GenerateLabel("sharedWhenExpression");
						ArrayBuilder<BoundWhenDecisionDagNode> instance = ArrayBuilder<BoundWhenDecisionDagNode>.GetInstance();
						instance.Add(boundWhenDecisionDagNode);
						whenExpressionMap.Add(whenExpression, (item, instance));
					}
					whenNodeMap.Add(boundWhenDecisionDagNode, (item, num++));
				}
			}
			foreach (BoundDecisionDagNode item4 in sortedNodes)
			{
				if (item4 is BoundWhenDecisionDagNode boundWhenDecisionDagNode2 && !tryLowerAsJumpToSharedWhenExpression(boundWhenDecisionDagNode2))
				{
					lowerWhenClause(boundWhenDecisionDagNode2);
				}
			}
			foreach (KeyValuePair<BoundExpression, (LabelSymbol, ArrayBuilder<BoundWhenDecisionDagNode>)> item5 in whenExpressionMap)
			{
				RoslynKeyValuePairExtensions.Deconstruct(item5, out var key, out var value2);
				(LabelSymbol, ArrayBuilder<BoundWhenDecisionDagNode>) tuple2 = value2;
				BoundExpression whenExpression2 = key;
				var (labelToWhenExpression, arrayBuilder) = tuple2;
				lowerWhenExpressionIfShared(whenExpression2, labelToWhenExpression, arrayBuilder);
				arrayBuilder.Free();
			}
			whenExpressionMap.Free();
			whenNodeMap.Free();
			void addConditionalGoto(BoundExpression boundExpression, SyntaxNode whenClauseSyntax, LabelSymbol whenTrueLabel, ArrayBuilder<BoundStatement> sectionBuilder)
			{
				_factory.Syntax = whenClauseSyntax;
				BoundStatement boundStatement = _factory.ConditionalGoto(_localRewriter.VisitExpression(boundExpression), whenTrueLabel, jumpIfTrue: true);
				if (base.GenerateInstrumentation && !boundExpression.WasCompilerGenerated)
				{
					boundStatement = _localRewriter.Instrumenter.InstrumentSwitchWhenClauseConditionalGotoBody(boundExpression, boundStatement);
				}
				sectionBuilder.Add(boundStatement);
			}
			bool isSharedWhenExpression(BoundExpression? boundExpression)
			{
				if (boundExpression != null && whenExpressionMap.TryGetValue(boundExpression, out (LabelSymbol, ArrayBuilder<BoundWhenDecisionDagNode>) value3))
				{
					return value3.Item2.Count > 1;
				}
				return false;
			}
			void lowerBindings(ImmutableArray<BoundPatternBinding> bindings, ArrayBuilder<BoundStatement> sectionBuilder)
			{
				foreach (BoundPatternBinding item6 in bindings)
				{
					BoundExpression boundExpression = _localRewriter.VisitExpression(item6.VariableAccess);
					BoundExpression temp = _tempAllocator.GetTemp(item6.TempContainingValue);
					if (boundExpression != temp)
					{
						sectionBuilder.Add(_factory.Assignment(boundExpression, temp));
					}
				}
			}
			void lowerWhenClause(BoundWhenDecisionDagNode whenClause)
			{
				BoundLeafDecisionDagNode dag = (BoundLeafDecisionDagNode)whenClause.WhenTrue;
				LabelSymbol dagNodeLabel = GetDagNodeLabel(whenClause);
				ArrayBuilder<BoundStatement> arrayBuilder2 = BuilderForSection(whenClause.Syntax);
				arrayBuilder2.Add(_factory.Label(dagNodeLabel));
				lowerBindings(whenClause.Bindings, arrayBuilder2);
				BoundDecisionDagNode whenFalse = whenClause.WhenFalse;
				LabelSymbol dagNodeLabel2 = GetDagNodeLabel(dag);
				if (whenClause.WhenExpression != null && whenClause.WhenExpression.ConstantValueOpt != ConstantValue.True)
				{
					addConditionalGoto(whenClause.WhenExpression, whenClause.Syntax, dagNodeLabel2, arrayBuilder2);
					BoundStatement boundStatement = _factory.Goto(GetDagNodeLabel(whenFalse));
					arrayBuilder2.Add(base.GenerateInstrumentation ? _factory.HiddenSequencePoint(boundStatement) : boundStatement);
				}
				else
				{
					arrayBuilder2.Add(_factory.Goto(dagNodeLabel2));
				}
			}
			void lowerWhenExpressionIfShared(BoundExpression whenExpression3, LabelSymbol label, ArrayBuilder<BoundWhenDecisionDagNode> whenNodes)
			{
				if (isSharedWhenExpression(whenExpression3))
				{
					SyntaxNode syntax = whenNodes[0].Syntax;
					LabelSymbol dagNodeLabel = GetDagNodeLabel(whenNodes[0].WhenTrue);
					ArrayBuilder<BoundStatement> arrayBuilder2 = BuilderForSection(syntax);
					arrayBuilder2.Add(_factory.Label(label));
					lowerBindings(whenNodes[0].Bindings, arrayBuilder2);
					addConditionalGoto(whenExpression3, syntax, dagNodeLabel, arrayBuilder2);
					ArrayBuilder<SyntheticBoundNodeFactory.SyntheticSwitchSection> instance2 = ArrayBuilder<SyntheticBoundNodeFactory.SyntheticSwitchSection>.GetInstance();
					foreach (BoundWhenDecisionDagNode whenNode in whenNodes)
					{
						int item2 = whenNodeMap[whenNode].WhenNodeIdentifier;
						instance2.Add(_factory.SwitchSection(item2, _factory.Goto(GetDagNodeLabel(whenNode.WhenFalse))));
					}
					BoundStatement boundStatement = _factory.Switch(_factory.Local(_whenNodeIdentifierLocal), instance2.ToImmutableAndFree());
					arrayBuilder2.Add(base.GenerateInstrumentation ? _factory.HiddenSequencePoint(boundStatement) : boundStatement);
				}
			}
			bool tryLowerAsJumpToSharedWhenExpression(BoundWhenDecisionDagNode whenNode)
			{
				BoundExpression whenExpression3 = whenNode.WhenExpression;
				if (!isSharedWhenExpression(whenExpression3))
				{
					return false;
				}
				LabelSymbol dagNodeLabel = GetDagNodeLabel(whenNode);
				ArrayBuilder<BoundStatement> arrayBuilder2 = BuilderForSection(whenNode.Syntax);
				arrayBuilder2.Add(_factory.Label(dagNodeLabel));
				if ((object)_whenNodeIdentifierLocal == null)
				{
					_whenNodeIdentifierLocal = _factory.SynthesizedLocal(_factory.SpecialType(SpecialType.System_Int32));
				}
				whenNodeMap.TryGetValue(whenNode, out (LabelSymbol, int) value3);
				arrayBuilder2.Add(_factory.Assignment(_factory.Local(_whenNodeIdentifierLocal), _factory.Literal(value3.Item2)));
				arrayBuilder2.Add(_factory.Goto(value3.Item1));
				return true;
			}
		}

		private void LowerDecisionDagNode(BoundDecisionDagNode node, BoundDecisionDagNode nextNode)
		{
			_factory.Syntax = node.Syntax;
			if (!(node is BoundEvaluationDecisionDagNode boundEvaluationDecisionDagNode))
			{
				if (node is BoundTestDecisionDagNode boundTestDecisionDagNode)
				{
					BoundExpression test = LowerTest(boundTestDecisionDagNode.Test);
					GenerateTest(test, boundTestDecisionDagNode.WhenTrue, boundTestDecisionDagNode.WhenFalse, nextNode);
					return;
				}
				throw ExceptionUtilities.UnexpectedValue(node.Kind);
			}
			BoundExpression expr = LowerEvaluation(boundEvaluationDecisionDagNode.Evaluation);
			_loweredDecisionDag.Add(_factory.ExpressionStatement(expr));
			if (base.GenerateInstrumentation)
			{
				_loweredDecisionDag.Add(_factory.HiddenSequencePoint());
			}
			if (nextNode != boundEvaluationDecisionDagNode.Next)
			{
				_loweredDecisionDag.Add(_factory.Goto(GetDagNodeLabel(boundEvaluationDecisionDagNode.Next)));
			}
		}
	}

	private abstract class PatternLocalRewriter
	{
		public sealed class DagTempAllocator
		{
			private readonly SyntheticBoundNodeFactory _factory;

			private readonly PooledDictionary<BoundDagTemp, BoundExpression> _map = PooledDictionary<BoundDagTemp, BoundExpression>.GetInstance();

			private readonly ArrayBuilder<LocalSymbol> _temps = ArrayBuilder<LocalSymbol>.GetInstance();

			private readonly SyntaxNode _node;

			private readonly bool _generateSequencePoints;

			public DagTempAllocator(SyntheticBoundNodeFactory factory, SyntaxNode node, bool generateSequencePoints)
			{
				_factory = factory;
				_node = node;
				_generateSequencePoints = generateSequencePoints;
			}

			public void Free()
			{
				_temps.Free();
				_map.Free();
			}

			public BoundExpression GetTemp(BoundDagTemp dagTemp)
			{
				if (!_map.TryGetValue(dagTemp, out var value))
				{
					SynthesizedLocalKind kind = (_generateSequencePoints ? SynthesizedLocalKind.SwitchCasePatternMatching : SynthesizedLocalKind.LoweringTemp);
					LocalSymbol localSymbol = _factory.SynthesizedLocal(dagTemp.Type, _node, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, kind);
					value = _factory.Local(localSymbol);
					_map.Add(dagTemp, value);
					_temps.Add(localSymbol);
				}
				return value;
			}

			public bool TrySetTemp(BoundDagTemp dagTemp, BoundExpression translation)
			{
				if (!_map.ContainsKey(dagTemp))
				{
					_map.Add(dagTemp, translation);
					return true;
				}
				return false;
			}

			public ImmutableArray<LocalSymbol> AllTemps()
			{
				return _temps.ToImmutableArray();
			}
		}

		protected readonly LocalRewriter _localRewriter;

		protected readonly SyntheticBoundNodeFactory _factory;

		protected readonly DagTempAllocator _tempAllocator;

		protected bool GenerateInstrumentation { get; }

		public PatternLocalRewriter(SyntaxNode node, LocalRewriter localRewriter, bool generateInstrumentation)
		{
			_localRewriter = localRewriter;
			_factory = localRewriter._factory;
			GenerateInstrumentation = generateInstrumentation;
			_tempAllocator = new DagTempAllocator(_factory, node, generateInstrumentation);
		}

		public void Free()
		{
			_tempAllocator.Free();
		}

		protected BoundExpression LowerEvaluation(BoundDagEvaluation evaluation)
		{
			BoundExpression boundExpression = _tempAllocator.GetTemp(evaluation.Input);
			ArrayBuilder<RefKind> refKindBuilder;
			ArrayBuilder<BoundExpression> argBuilder;
			if (!(evaluation is BoundDagFieldEvaluation boundDagFieldEvaluation))
			{
				if (!(evaluation is BoundDagPropertyEvaluation boundDagPropertyEvaluation))
				{
					if (!(evaluation is BoundDagDeconstructEvaluation boundDagDeconstructEvaluation))
					{
						if (!(evaluation is BoundDagTypeEvaluation boundDagTypeEvaluation))
						{
							if (!(evaluation is BoundDagIndexEvaluation boundDagIndexEvaluation))
							{
								if (!(evaluation is BoundDagIndexerEvaluation boundDagIndexerEvaluation))
								{
									if (!(evaluation is BoundDagSliceEvaluation boundDagSliceEvaluation))
									{
										if (!(evaluation is BoundDagAssignmentEvaluation))
										{
										}
										throw ExceptionUtilities.UnexpectedValue(evaluation);
									}
									BoundExpression boundExpression2 = boundDagSliceEvaluation.IndexerAccess;
									if (boundExpression2 is BoundImplicitIndexerAccess boundImplicitIndexerAccess)
									{
										boundExpression2 = boundImplicitIndexerAccess.WithLengthOrCountAccess(_tempAllocator.GetTemp(boundDagSliceEvaluation.LengthTemp));
									}
									PooledDictionary<BoundEarlyValuePlaceholderBase, BoundExpression> instance = PooledDictionary<BoundEarlyValuePlaceholderBase, BoundExpression>.GetInstance();
									instance.Add(boundDagSliceEvaluation.ReceiverPlaceholder, boundExpression);
									instance.Add(boundDagSliceEvaluation.ArgumentPlaceholder, makeUnloweredRangeArgument(boundDagSliceEvaluation));
									boundExpression2 = PlaceholderReplacer.Replace(instance, boundExpression2);
									instance.Free();
									BoundExpression right = (BoundExpression)_localRewriter.Visit(boundExpression2);
									BoundDagTemp dagTemp = new BoundDagTemp(boundDagSliceEvaluation.Syntax, boundDagSliceEvaluation.SliceType, boundDagSliceEvaluation);
									BoundExpression temp = _tempAllocator.GetTemp(dagTemp);
									return _factory.AssignmentExpression(temp, right);
								}
								BoundExpression boundExpression3 = boundDagIndexerEvaluation.IndexerAccess;
								if (boundExpression3 is BoundImplicitIndexerAccess boundImplicitIndexerAccess2)
								{
									boundExpression3 = boundImplicitIndexerAccess2.WithLengthOrCountAccess(_tempAllocator.GetTemp(boundDagIndexerEvaluation.LengthTemp));
								}
								PooledDictionary<BoundEarlyValuePlaceholderBase, BoundExpression> instance2 = PooledDictionary<BoundEarlyValuePlaceholderBase, BoundExpression>.GetInstance();
								instance2.Add(boundDagIndexerEvaluation.ReceiverPlaceholder, boundExpression);
								instance2.Add(boundDagIndexerEvaluation.ArgumentPlaceholder, makeUnloweredIndexArgument(boundDagIndexerEvaluation.Index));
								boundExpression3 = PlaceholderReplacer.Replace(instance2, boundExpression3);
								instance2.Free();
								BoundExpression right2 = (BoundExpression)_localRewriter.Visit(boundExpression3);
								BoundDagTemp dagTemp2 = new BoundDagTemp(boundDagIndexerEvaluation.Syntax, boundDagIndexerEvaluation.IndexerType, boundDagIndexerEvaluation);
								BoundExpression temp2 = _tempAllocator.GetTemp(dagTemp2);
								return _factory.AssignmentExpression(temp2, right2);
							}
							TypeSymbol returnType = boundDagIndexEvaluation.Property.GetMethod.ReturnType;
							BoundDagTemp dagTemp3 = new BoundDagTemp(boundDagIndexEvaluation.Syntax, returnType, boundDagIndexEvaluation);
							BoundExpression temp3 = _tempAllocator.GetTemp(dagTemp3);
							return _factory.AssignmentExpression(temp3, _factory.Indexer(boundExpression, boundDagIndexEvaluation.Property, _factory.Literal(boundDagIndexEvaluation.Index)));
						}
						TypeSymbol typeSymbol = boundExpression.Type;
						if (typeSymbol.IsDynamic())
						{
							typeSymbol = _factory.SpecialType(SpecialType.System_Object);
							boundExpression = _factory.Convert(typeSymbol, boundExpression, Conversion.Identity);
						}
						TypeSymbol type = boundDagTypeEvaluation.Type;
						BoundDagTemp dagTemp4 = new BoundDagTemp(boundDagTypeEvaluation.Syntax, type, boundDagTypeEvaluation);
						BoundExpression temp4 = _tempAllocator.GetTemp(dagTemp4);
						CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = _localRewriter.GetNewCompoundUseSiteInfo();
						Conversion conversion = _factory.Compilation.Conversions.ClassifyBuiltInConversion(typeSymbol, temp4.Type, isChecked: false, ref useSiteInfo);
						_localRewriter._diagnostics.Add(boundDagTypeEvaluation.Syntax, useSiteInfo);
						BoundExpression right3 = ((!conversion.Exists) ? _factory.As(boundExpression, type) : ((conversion.Kind != ConversionKind.ExplicitNullable || !typeSymbol.GetNullableUnderlyingType().Equals(temp4.Type, TypeCompareKind.AllIgnoreOptions) || !_localRewriter.TryGetNullableMethod(boundDagTypeEvaluation.Syntax, typeSymbol, SpecialMember.System_Nullable_T_GetValueOrDefault, out MethodSymbol result)) ? _localRewriter.MakeConversionNode(boundDagTypeEvaluation.Syntax, boundExpression, conversion, type, @checked: false) : _factory.Call(boundExpression, result)));
						return _factory.AssignmentExpression(temp4, right3);
					}
					MethodSymbol deconstructMethod = boundDagDeconstructEvaluation.DeconstructMethod;
					refKindBuilder = ArrayBuilder<RefKind>.GetInstance();
					argBuilder = ArrayBuilder<BoundExpression>.GetInstance();
					int num;
					BoundExpression receiver;
					if (deconstructMethod.IsStatic)
					{
						receiver = _factory.Type(deconstructMethod.ContainingType);
						addArg(deconstructMethod.ParameterRefKinds[0], boundExpression);
						num = 1;
					}
					else
					{
						receiver = boundExpression;
						num = 0;
					}
					for (int i = num; i < deconstructMethod.ParameterCount; i++)
					{
						ParameterSymbol parameterSymbol = deconstructMethod.Parameters[i];
						BoundDagTemp dagTemp5 = new BoundDagTemp(boundDagDeconstructEvaluation.Syntax, parameterSymbol.Type, boundDagDeconstructEvaluation, i - num);
						addArg(RefKind.Out, _tempAllocator.GetTemp(dagTemp5));
					}
					receiver = _localRewriter.ConvertReceiverForExtensionMemberIfNeeded(deconstructMethod, receiver, markAsChecked: true);
					return _factory.Call(receiver, deconstructMethod, refKindBuilder.ToImmutableAndFree(), argBuilder.ToImmutableAndFree());
				}
				PropertySymbol property = boundDagPropertyEvaluation.Property;
				BoundDagTemp dagTemp6 = new BoundDagTemp(boundDagPropertyEvaluation.Syntax, property.Type, boundDagPropertyEvaluation);
				BoundExpression temp5 = _tempAllocator.GetTemp(dagTemp6);
				boundExpression = _localRewriter.ConvertReceiverForExtensionMemberIfNeeded(property, boundExpression, markAsChecked: true);
				return _factory.AssignmentExpression(temp5, _localRewriter.MakePropertyAccess(_factory.Syntax, boundExpression, property, LookupResultKind.Viable, property.Type, isLeftOfAssignment: false));
			}
			FieldSymbol field = boundDagFieldEvaluation.Field;
			BoundDagTemp dagTemp7 = new BoundDagTemp(boundDagFieldEvaluation.Syntax, field.Type, boundDagFieldEvaluation);
			BoundExpression temp6 = _tempAllocator.GetTemp(dagTemp7);
			BoundExpression boundExpression4 = _localRewriter.MakeFieldAccess(boundDagFieldEvaluation.Syntax, boundExpression, field, null, LookupResultKind.Viable, field.Type);
			boundExpression4.WasCompilerGenerated = true;
			return _factory.AssignmentExpression(temp6, boundExpression4);
			void addArg(RefKind refKind, BoundExpression expression)
			{
				refKindBuilder.Add(refKind);
				argBuilder.Add(expression);
			}
			BoundExpression makeUnloweredIndexArgument(int index)
			{
				MethodSymbol methodSymbol = (MethodSymbol)_factory.WellKnownMember(WellKnownMember.System_Index__ctor);
				if (index < 0)
				{
					return new BoundFromEndIndexExpression(_factory.Syntax, _factory.Literal(-index), methodSymbol, _factory.WellKnownType(WellKnownType.System_Index));
				}
				return _factory.New(methodSymbol, _factory.Literal(index), _factory.Literal(value: false));
			}
			BoundExpression makeUnloweredRangeArgument(BoundDagSliceEvaluation e)
			{
				MethodSymbol methodOpt = (MethodSymbol)_factory.WellKnownMember(WellKnownMember.System_Index__ctor);
				BoundFromEndIndexExpression rightOperandOpt = new BoundFromEndIndexExpression(_factory.Syntax, _factory.Literal(-e.EndIndex), methodOpt, _factory.WellKnownType(WellKnownType.System_Index));
				MethodSymbol methodOpt2 = (MethodSymbol)_factory.WellKnownMember(WellKnownMember.System_Range__ctor);
				return new BoundRangeExpression(e.Syntax, makeUnloweredIndexArgument(e.StartIndex), rightOperandOpt, methodOpt2, _factory.WellKnownType(WellKnownType.System_Range));
			}
		}

		protected BoundExpression LowerTest(BoundDagTest test)
		{
			_factory.Syntax = test.Syntax;
			BoundExpression temp = _tempAllocator.GetTemp(test.Input);
			if (!(test is BoundDagNonNullTest boundDagNonNullTest))
			{
				if (!(test is BoundDagTypeTest boundDagTypeTest))
				{
					if (!(test is BoundDagExplicitNullTest boundDagExplicitNullTest))
					{
						if (!(test is BoundDagValueTest boundDagValueTest))
						{
							if (test is BoundDagRelationalTest boundDagRelationalTest)
							{
								return MakeRelationalTest(boundDagRelationalTest.Syntax, temp, boundDagRelationalTest.OperatorKind, boundDagRelationalTest.Value);
							}
							throw ExceptionUtilities.UnexpectedValue(test);
						}
						return MakeValueTest(boundDagValueTest.Syntax, temp, boundDagValueTest.Value);
					}
					return MakeNullCheck(boundDagExplicitNullTest.Syntax, temp, temp.Type.IsNullableType() ? BinaryOperatorKind.NullableNullEqual : BinaryOperatorKind.Equal);
				}
				return _factory.Is(temp, boundDagTypeTest.Type);
			}
			return MakeNullCheck(boundDagNonNullTest.Syntax, temp, temp.Type.IsNullableType() ? BinaryOperatorKind.NullableNullNotEqual : BinaryOperatorKind.NotEqual);
		}

		private BoundExpression MakeNullCheck(SyntaxNode syntax, BoundExpression rewrittenExpr, BinaryOperatorKind operatorKind)
		{
			if (rewrittenExpr.Type.IsPointerOrFunctionPointer())
			{
				TypeSymbol type = _factory.SpecialType(SpecialType.System_Object);
				PointerTypeSymbol type2 = new PointerTypeSymbol(TypeWithAnnotations.Create(_factory.SpecialType(SpecialType.System_Void)));
				return _localRewriter.MakeBinaryOperator(syntax, operatorKind, _factory.Convert(type2, rewrittenExpr, Conversion.PointerToVoid), _factory.Convert(type2, new BoundLiteral(syntax, ConstantValue.Null, type), Conversion.NullToPointer), _factory.SpecialType(SpecialType.System_Boolean), null, null);
			}
			return _localRewriter.MakeNullCheck(syntax, rewrittenExpr, operatorKind);
		}

		protected BoundExpression MakeValueTest(SyntaxNode syntax, BoundExpression input, ConstantValue value)
		{
			if (value.IsString && input.Type.IsSpanOrReadOnlySpanChar())
			{
				return MakeSpanStringTest(input, value);
			}
			BinaryOperatorKind binaryOperatorKind = Binder.RelationalOperatorType(input.Type.EnumUnderlyingTypeOrSelf());
			BinaryOperatorKind operatorKind = BinaryOperatorKind.Equal | binaryOperatorKind;
			return MakeRelationalTest(syntax, input, operatorKind, value);
		}

		protected BoundExpression MakeRelationalTest(SyntaxNode syntax, BoundExpression input, BinaryOperatorKind operatorKind, ConstantValue value)
		{
			if ((input.Type.SpecialType == SpecialType.System_Double && double.IsNaN(value.DoubleValue)) || (input.Type.SpecialType == SpecialType.System_Single && float.IsNaN(value.SingleValue)))
			{
				return _factory.MakeIsNotANumberTest(input);
			}
			BoundExpression boundExpression = _localRewriter.MakeLiteral(syntax, value, input.Type);
			TypeSymbol typeSymbol = input.Type.EnumUnderlyingTypeOrSelf();
			if (operatorKind.OperandTypes() == BinaryOperatorKind.Int && typeSymbol.SpecialType != SpecialType.System_Int32)
			{
				typeSymbol = _factory.SpecialType(SpecialType.System_Int32);
				Conversion conversion = _factory.ClassifyEmitConversion(input, typeSymbol);
				input = _factory.Convert(typeSymbol, input, conversion);
				conversion = _factory.ClassifyEmitConversion(boundExpression, typeSymbol);
				boundExpression = _factory.Convert(typeSymbol, boundExpression, conversion);
			}
			return _localRewriter.MakeBinaryOperator(_factory.Syntax, operatorKind, input, boundExpression, _factory.SpecialType(SpecialType.System_Boolean), null, null);
		}

		private BoundExpression MakeSpanStringTest(BoundExpression input, ConstantValue value)
		{
			bool flag = input.Type.IsReadOnlySpanChar();
			MethodSymbol method = ((MethodSymbol)_factory.WellKnownMember(flag ? WellKnownMember.System_MemoryExtensions__SequenceEqual_ReadOnlySpan_T : WellKnownMember.System_MemoryExtensions__SequenceEqual_Span_T)).Construct(_factory.SpecialType(SpecialType.System_Char));
			MethodSymbol method2 = (MethodSymbol)_factory.WellKnownMember(WellKnownMember.System_MemoryExtensions__AsSpan_String);
			return _factory.Call(null, method, input, _factory.Call(null, method2, _factory.StringLiteral(value)));
		}

		protected bool TryLowerTypeTestAndCast(BoundDagTest test, BoundDagEvaluation evaluation, [NotNullWhen(true)] out BoundExpression sideEffect, [NotNullWhen(true)] out BoundExpression testExpression)
		{
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = _localRewriter.GetNewCompoundUseSiteInfo();
			if (test is BoundDagTypeTest boundDagTypeTest && evaluation is BoundDagTypeEvaluation boundDagTypeEvaluation && boundDagTypeTest.Type.IsReferenceType && boundDagTypeEvaluation.Type.Equals(boundDagTypeTest.Type, TypeCompareKind.AllIgnoreOptions) && boundDagTypeEvaluation.Input == boundDagTypeTest.Input)
			{
				BoundExpression temp = _tempAllocator.GetTemp(test.Input);
				BoundExpression temp2 = _tempAllocator.GetTemp(new BoundDagTemp(evaluation.Syntax, boundDagTypeEvaluation.Type, evaluation));
				sideEffect = _factory.AssignmentExpression(temp2, _factory.As(temp, boundDagTypeEvaluation.Type));
				testExpression = _factory.ObjectNotEqual(temp2, _factory.Null(temp2.Type));
				return true;
			}
			if (test is BoundDagNonNullTest boundDagNonNullTest && evaluation is BoundDagTypeEvaluation boundDagTypeEvaluation2)
			{
				Conversion conversion = _factory.Compilation.Conversions.ClassifyBuiltInConversion(test.Input.Type, boundDagTypeEvaluation2.Type, isChecked: false, ref useSiteInfo);
				if ((conversion.IsIdentity || conversion.Kind == ConversionKind.ImplicitReference || conversion.IsBoxing) && boundDagTypeEvaluation2.Input == boundDagNonNullTest.Input)
				{
					BoundExpression temp3 = _tempAllocator.GetTemp(test.Input);
					TypeSymbol type = boundDagTypeEvaluation2.Type;
					BoundExpression temp4 = _tempAllocator.GetTemp(new BoundDagTemp(evaluation.Syntax, type, evaluation));
					sideEffect = _factory.AssignmentExpression(temp4, _factory.Convert(type, temp3, conversion));
					testExpression = _factory.ObjectNotEqual(temp4, _factory.Null(type));
					_localRewriter._diagnostics.Add(test.Syntax, useSiteInfo);
					return true;
				}
			}
			sideEffect = (testExpression = null);
			return false;
		}

		protected BoundDecisionDag ShareTempsAndEvaluateInput(BoundExpression loweredInput, BoundDecisionDag decisionDag, Action<BoundExpression> addCode, out BoundExpression savedInputExpression)
		{
			bool flag = decisionDag.TopologicallySortedNodes.Any(delegate(BoundDecisionDagNode node)
			{
				if (node is BoundWhenDecisionDagNode boundWhenDecisionDagNode2)
				{
					BoundExpression whenExpression = boundWhenDecisionDagNode2.WhenExpression;
					if (whenExpression != null)
					{
						return (object)whenExpression.ConstantValueOpt == null;
					}
				}
				return false;
			});
			BoundDagTemp dagTemp = BoundDagTemp.ForOriginalInput(loweredInput);
			if ((loweredInput.Kind == BoundKind.Local || loweredInput.Kind == BoundKind.Parameter) && loweredInput.GetRefKind() == RefKind.None && !flag)
			{
				_tempAllocator.TrySetTemp(dagTemp, loweredInput);
			}
			foreach (BoundDecisionDagNode topologicallySortedNode in decisionDag.TopologicallySortedNodes)
			{
				if (!(topologicallySortedNode is BoundWhenDecisionDagNode { Bindings: var bindings }))
				{
					continue;
				}
				foreach (BoundPatternBinding item in bindings)
				{
					if (item.VariableAccess is BoundLocal)
					{
						_tempAllocator.TrySetTemp(item.TempContainingValue, item.VariableAccess);
					}
				}
			}
			if (loweredInput.Type.IsTupleType && !loweredInput.Type.OriginalDefinition.Equals(_factory.Compilation.GetWellKnownType(WellKnownType.System_ValueTuple_TRest)) && loweredInput.Syntax.Kind() == SyntaxKind.TupleExpression && loweredInput is BoundObjectCreationExpression loweredInput2 && !decisionDag.TopologicallySortedNodes.Any((BoundDecisionDagNode n) => usesOriginalInput(n)))
			{
				decisionDag = RewriteTupleInput(decisionDag, loweredInput2, addCode, !flag, out savedInputExpression);
			}
			else
			{
				BoundExpression boundExpression = (savedInputExpression = _tempAllocator.GetTemp(dagTemp));
				if (boundExpression != loweredInput)
				{
					addCode(_factory.AssignmentExpression(boundExpression, loweredInput));
				}
			}
			return decisionDag;
			static bool usesOriginalInput(BoundDecisionDagNode node)
			{
				if (node is BoundWhenDecisionDagNode boundWhenDecisionDagNode2)
				{
					return boundWhenDecisionDagNode2.Bindings.Any((BoundPatternBinding b) => b.TempContainingValue.IsOriginalInput);
				}
				if (node is BoundTestDecisionDagNode boundTestDecisionDagNode)
				{
					return boundTestDecisionDagNode.Test.Input.IsOriginalInput;
				}
				if (node is BoundEvaluationDecisionDagNode boundEvaluationDecisionDagNode)
				{
					if (boundEvaluationDecisionDagNode.Evaluation is BoundDagFieldEvaluation boundDagFieldEvaluation)
					{
						if (boundDagFieldEvaluation.Input.IsOriginalInput)
						{
							return !boundDagFieldEvaluation.Field.IsTupleElement();
						}
						return false;
					}
					return boundEvaluationDecisionDagNode.Evaluation.Input.IsOriginalInput;
				}
				return false;
			}
		}

		private BoundDecisionDag RewriteTupleInput(BoundDecisionDag decisionDag, BoundObjectCreationExpression loweredInput, Action<BoundExpression> addCode, bool canShareInputs, out BoundExpression savedInputExpression)
		{
			int length = loweredInput.Arguments.Length;
			BoundDagTemp input = BoundDagTemp.ForOriginalInput(loweredInput.Syntax, loweredInput.Type);
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(loweredInput.Arguments.Length);
			for (int i = 0; i < length; i++)
			{
				FieldSymbol correspondingTupleField = loweredInput.Type.TupleElements[i].CorrespondingTupleField;
				BoundExpression boundExpression = loweredInput.Arguments[i];
				BoundDagFieldEvaluation source = new BoundDagFieldEvaluation(boundExpression.Syntax, correspondingTupleField, input);
				BoundDagTemp boundDagTemp = new BoundDagTemp(boundExpression.Syntax, boundExpression.Type, source);
				storeToTemp(boundDagTemp, boundExpression);
				instance.Add(_tempAllocator.GetTemp(boundDagTemp));
			}
			BoundDecisionDag result = decisionDag.Rewrite(makeReplacement);
			savedInputExpression = loweredInput.Update(loweredInput.Constructor, instance.ToImmutableAndFree(), loweredInput.ArgumentNamesOpt, loweredInput.ArgumentRefKindsOpt, loweredInput.Expanded, loweredInput.ArgsToParamsOpt, loweredInput.DefaultArguments, loweredInput.ConstantValueOpt, loweredInput.InitializerExpressionOpt, loweredInput.Type);
			return result;
			static BoundDecisionDagNode makeReplacement(BoundDecisionDagNode node, IReadOnlyDictionary<BoundDecisionDagNode, BoundDecisionDagNode> replacement)
			{
				if (!(node is BoundEvaluationDecisionDagNode boundEvaluationDecisionDagNode))
				{
					if (node is BoundTestDecisionDagNode)
					{
					}
				}
				else if (boundEvaluationDecisionDagNode.Evaluation is BoundDagFieldEvaluation boundDagFieldEvaluation && boundDagFieldEvaluation.Input.IsOriginalInput)
				{
					FieldSymbol field = boundDagFieldEvaluation.Field;
					if (field.CorrespondingTupleField != null)
					{
						_ = field.TupleElementIndex;
						return replacement[boundEvaluationDecisionDagNode.Next];
					}
				}
				return BoundDecisionDag.TrivialReplacement(node, replacement);
			}
			void storeToTemp(BoundDagTemp temp, BoundExpression expr)
			{
				if (!canShareInputs || (expr.Kind != BoundKind.Parameter && expr.Kind != BoundKind.Local) || !_tempAllocator.TrySetTemp(temp, expr))
				{
					BoundExpression temp2 = _tempAllocator.GetTemp(temp);
					addCode(_factory.AssignmentExpression(temp2, expr));
				}
			}
		}
	}

	private sealed class PlaceholderReplacer : BoundTreeRewriterWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator
	{
		private readonly Dictionary<BoundEarlyValuePlaceholderBase, BoundExpression> _placeholders;

		private PlaceholderReplacer(Dictionary<BoundEarlyValuePlaceholderBase, BoundExpression> placeholders)
		{
			_placeholders = placeholders;
		}

		public static BoundExpression Replace(Dictionary<BoundEarlyValuePlaceholderBase, BoundExpression> placeholders, BoundExpression expr)
		{
			return (BoundExpression)new PlaceholderReplacer(placeholders).Visit(expr);
		}

		private BoundNode ReplacePlaceholder(BoundEarlyValuePlaceholderBase placeholder)
		{
			return _placeholders[placeholder];
		}

		public override BoundNode VisitListPatternReceiverPlaceholder(BoundListPatternReceiverPlaceholder node)
		{
			return ReplacePlaceholder(node);
		}

		public override BoundNode VisitListPatternIndexPlaceholder(BoundListPatternIndexPlaceholder node)
		{
			return ReplacePlaceholder(node);
		}

		public override BoundNode VisitSlicePatternReceiverPlaceholder(BoundSlicePatternReceiverPlaceholder node)
		{
			return ReplacePlaceholder(node);
		}

		public override BoundNode VisitSlicePatternRangePlaceholder(BoundSlicePatternRangePlaceholder node)
		{
			return ReplacePlaceholder(node);
		}
	}

	private enum AssignmentKind
	{
		SimpleAssignment,
		CompoundAssignment,
		IncrementDecrement,
		Deconstruction,
		NullCoalescingAssignment
	}

	private abstract class BaseSwitchLocalRewriter : DecisionDagRewriter
	{
		private readonly PooledDictionary<SyntaxNode, ArrayBuilder<BoundStatement>> _switchArms = PooledDictionary<SyntaxNode, ArrayBuilder<BoundStatement>>.GetInstance();

		protected override ArrayBuilder<BoundStatement> BuilderForSection(SyntaxNode whenClauseSyntax)
		{
			SyntaxNode key = ((whenClauseSyntax is SwitchLabelSyntax switchLabelSyntax) ? switchLabelSyntax.Parent : whenClauseSyntax);
			if (!_switchArms.TryGetValue(key, out ArrayBuilder<BoundStatement> value) || value == null)
			{
				throw new InvalidOperationException();
			}
			return value;
		}

		protected BaseSwitchLocalRewriter(SyntaxNode node, LocalRewriter localRewriter, ImmutableArray<SyntaxNode> arms, bool generateInstrumentation)
			: base(node, localRewriter, generateInstrumentation)
		{
			foreach (SyntaxNode item in arms)
			{
				ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
				if (base.GenerateInstrumentation)
				{
					instance.Add(_factory.HiddenSequencePoint());
				}
				_switchArms.Add(item, instance);
			}
		}

		protected new void Free()
		{
			_switchArms.Free();
			base.Free();
		}

		protected (ImmutableArray<BoundStatement> loweredDag, ImmutableDictionary<SyntaxNode, ImmutableArray<BoundStatement>> switchSections) LowerDecisionDag(BoundDecisionDag decisionDag)
		{
			ImmutableArray<BoundStatement> item = LowerDecisionDagCore(decisionDag);
			ImmutableDictionary<SyntaxNode, ImmutableArray<BoundStatement>> item2 = _switchArms.ToImmutableDictionary<KeyValuePair<SyntaxNode, ArrayBuilder<BoundStatement>>, SyntaxNode, ImmutableArray<BoundStatement>>((KeyValuePair<SyntaxNode, ArrayBuilder<BoundStatement>> kv) => kv.Key, (KeyValuePair<SyntaxNode, ArrayBuilder<BoundStatement>> kv) => kv.Value.ToImmutableAndFree());
			_switchArms.Clear();
			return (loweredDag: item, switchSections: item2);
		}
	}

	private delegate BoundExpression ParamsArrayElementRewriter<TArg>(BoundExpression element, ref TArg arg);

	private enum ConditionalAccessLoweringKind
	{
		LoweredConditionalAccess,
		Conditional,
		ConditionalCaptureReceiverByVal
	}

	private class DeconstructionSideEffects
	{
		internal ArrayBuilder<BoundExpression> init;

		internal ArrayBuilder<BoundExpression> deconstructions;

		internal ArrayBuilder<BoundExpression> conversions;

		internal ArrayBuilder<BoundExpression> assignments;

		internal static DeconstructionSideEffects GetInstance()
		{
			return new DeconstructionSideEffects
			{
				init = ArrayBuilder<BoundExpression>.GetInstance(),
				deconstructions = ArrayBuilder<BoundExpression>.GetInstance(),
				conversions = ArrayBuilder<BoundExpression>.GetInstance(),
				assignments = ArrayBuilder<BoundExpression>.GetInstance()
			};
		}

		internal void Consolidate()
		{
			init.AddRange(deconstructions);
			init.AddRange(conversions);
			init.AddRange(assignments);
			deconstructions.Free();
			conversions.Free();
			assignments.Free();
		}

		internal BoundExpression? PopLast()
		{
			if (init.Count == 0)
			{
				return null;
			}
			BoundExpression result = init.Last();
			init.RemoveLast();
			return result;
		}

		internal ImmutableArray<BoundExpression> ToImmutableAndFree()
		{
			return init.ToImmutableAndFree();
		}

		internal void Free()
		{
			init.Free();
		}
	}

	private enum EventAssignmentKind
	{
		Assignment,
		Addition,
		Subtraction
	}

	private delegate BoundStatement? GetForEachStatementAsForPreamble(LocalRewriter rewriter, SyntaxNode syntax, ForEachEnumeratorInfo enumeratorInfo, ref BoundExpression rewrittenExpression, out LocalSymbol? preambleLocal, out RefKind collectionTempRefKind);

	private delegate BoundExpression GetForEachStatementAsForItem<TArg>(LocalRewriter rewriter, SyntaxNode syntax, ForEachEnumeratorInfo enumeratorInfo, BoundLocal boundArrayVar, BoundLocal boundPositionVar, TArg arg);

	private delegate BoundExpression GetForEachStatementAsForLength<TArg>(LocalRewriter rewriter, SyntaxNode syntax, BoundLocal boundArrayVar, TArg arg);

	private enum PatternIndexOffsetLoweringStrategy
	{
		Zero,
		Length,
		SubtractFromLength,
		UseAsIs,
		UseGetOffsetAPI
	}

	private sealed class IsPatternExpressionGeneralLocalRewriter(SyntaxNode node, LocalRewriter localRewriter) : DecisionDagRewriter(node, localRewriter, generateInstrumentation: false)
	{
		private readonly ArrayBuilder<BoundStatement> _statements = ArrayBuilder<BoundStatement>.GetInstance();

		protected override ArrayBuilder<BoundStatement> BuilderForSection(SyntaxNode section)
		{
			return _statements;
		}

		public new void Free()
		{
			base.Free();
			_statements.Free();
		}

		internal BoundExpression LowerGeneralIsPattern(BoundIsPatternExpression node, BoundDecisionDag decisionDag)
		{
			_factory.Syntax = node.Syntax;
			ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
			BoundExpression loweredSwitchGoverningExpression = _localRewriter.VisitExpression(node.Expression);
			decisionDag = ShareTempsIfPossibleAndEvaluateInput(decisionDag, loweredSwitchGoverningExpression, instance, out var _);
			ImmutableArray<BoundStatement> statements = LowerDecisionDagCore(decisionDag);
			instance.Add(_factory.Block(statements));
			LocalSymbol localSymbol = _factory.SynthesizedLocal(node.Type, node.Syntax);
			LabelSymbol label = _factory.GenerateLabel("afterIsPatternExpression");
			LabelSymbol whenTrueLabel = node.WhenTrueLabel;
			LabelSymbol whenFalseLabel = node.WhenFalseLabel;
			if (_statements.Count != 0)
			{
				instance.Add(_factory.Block(_statements.ToArray()));
			}
			instance.Add(_factory.Label(whenTrueLabel));
			instance.Add(_factory.Assignment(_factory.Local(localSymbol), _factory.Literal(value: true)));
			instance.Add(_factory.Goto(label));
			instance.Add(_factory.Label(whenFalseLabel));
			instance.Add(_factory.Assignment(_factory.Local(localSymbol), _factory.Literal(value: false)));
			instance.Add(_factory.Label(label));
			_localRewriter._needsSpilling = true;
			return _factory.SpillSequence(_tempAllocator.AllTemps().Add(localSymbol), instance.ToImmutableAndFree(), _factory.Local(localSymbol));
		}
	}

	private sealed class IsPatternExpressionLinearLocalRewriter : PatternLocalRewriter
	{
		private readonly ArrayBuilder<BoundExpression> _sideEffectBuilder;

		private readonly ArrayBuilder<BoundExpression> _conjunctBuilder;

		public IsPatternExpressionLinearLocalRewriter(BoundIsPatternExpression node, LocalRewriter localRewriter)
			: base(node.Syntax, localRewriter, generateInstrumentation: false)
		{
			_conjunctBuilder = ArrayBuilder<BoundExpression>.GetInstance();
			_sideEffectBuilder = ArrayBuilder<BoundExpression>.GetInstance();
		}

		public new void Free()
		{
			_conjunctBuilder.Free();
			_sideEffectBuilder.Free();
			base.Free();
		}

		private void AddConjunct(BoundExpression test)
		{
			TypeSymbol? type = test.Type;
			if ((object)type != null && !type.IsErrorType())
			{
				if (_sideEffectBuilder.Count != 0)
				{
					test = _factory.Sequence(ImmutableArray<LocalSymbol>.Empty, _sideEffectBuilder.ToImmutable(), test);
					_sideEffectBuilder.Clear();
				}
				_conjunctBuilder.Add(test);
			}
		}

		private void LowerOneTest(BoundDagTest test, bool invert = false)
		{
			_factory.Syntax = test.Syntax;
			if (test is BoundDagEvaluation evaluation)
			{
				BoundExpression item = LowerEvaluation(evaluation);
				_sideEffectBuilder.Add(item);
				return;
			}
			BoundExpression boundExpression = LowerTest(test);
			if (boundExpression != null)
			{
				if (invert)
				{
					boundExpression = _factory.Not(boundExpression);
				}
				AddConjunct(boundExpression);
			}
		}

		public BoundExpression LowerIsPatternAsLinearTestSequence(BoundIsPatternExpression isPatternExpression, BoundDecisionDag decisionDag, LabelSymbol whenTrueLabel, LabelSymbol whenFalseLabel)
		{
			BoundExpression loweredInput = _localRewriter.VisitExpression(isPatternExpression.Expression);
			decisionDag = ShareTempsAndEvaluateInput(loweredInput, decisionDag, delegate(BoundExpression expr)
			{
				_sideEffectBuilder.Add(expr);
			}, out var _);
			BoundDecisionDagNode rootNode = decisionDag.RootNode;
			return ProduceLinearTestSequence(rootNode, whenTrueLabel, whenFalseLabel);
		}

		private BoundExpression ProduceLinearTestSequence(BoundDecisionDagNode node, LabelSymbol whenTrueLabel, LabelSymbol whenFalseLabel)
		{
			while (node.Kind != BoundKind.LeafDecisionDagNode && node.Kind != BoundKind.WhenDecisionDagNode)
			{
				if (!(node is BoundEvaluationDecisionDagNode boundEvaluationDecisionDagNode))
				{
					if (node is BoundTestDecisionDagNode boundTestDecisionDagNode)
					{
						if (boundTestDecisionDagNode.WhenTrue is BoundEvaluationDecisionDagNode boundEvaluationDecisionDagNode2 && TryLowerTypeTestAndCast(boundTestDecisionDagNode.Test, boundEvaluationDecisionDagNode2.Evaluation, out var sideEffect, out var testExpression))
						{
							_sideEffectBuilder.Add(sideEffect);
							AddConjunct(testExpression);
							node = boundEvaluationDecisionDagNode2.Next;
						}
						else
						{
							bool flag = IsFailureNode(boundTestDecisionDagNode.WhenTrue, whenFalseLabel);
							LowerOneTest(boundTestDecisionDagNode.Test, flag);
							node = (flag ? boundTestDecisionDagNode.WhenFalse : boundTestDecisionDagNode.WhenTrue);
						}
					}
				}
				else
				{
					LowerOneTest(boundEvaluationDecisionDagNode.Evaluation);
					node = boundEvaluationDecisionDagNode.Next;
				}
			}
			if (!(node is BoundLeafDecisionDagNode))
			{
				if (!(node is BoundWhenDecisionDagNode { Bindings: var bindings }))
				{
					throw ExceptionUtilities.UnexpectedValue(node.Kind);
				}
				foreach (BoundPatternBinding item in bindings)
				{
					BoundExpression boundExpression = _localRewriter.VisitExpression(item.VariableAccess);
					BoundExpression temp = _tempAllocator.GetTemp(item.TempContainingValue);
					if (boundExpression != temp)
					{
						_sideEffectBuilder.Add(_factory.AssignmentExpression(boundExpression, temp));
					}
				}
			}
			if (_sideEffectBuilder.Count > 0 || _conjunctBuilder.Count == 0)
			{
				AddConjunct(_factory.Literal(value: true));
			}
			BoundExpression boundExpression2 = null;
			foreach (BoundExpression item2 in _conjunctBuilder)
			{
				boundExpression2 = ((boundExpression2 == null) ? item2 : _factory.LogicalAnd(boundExpression2, item2));
			}
			_conjunctBuilder.Clear();
			ImmutableArray<LocalSymbol> locals = _tempAllocator.AllTemps();
			if (locals.Length > 0)
			{
				boundExpression2 = _factory.Sequence(locals, ImmutableArray<BoundExpression>.Empty, boundExpression2);
			}
			return boundExpression2;
		}
	}

	private sealed class SwitchStatementLocalRewriter : BaseSwitchLocalRewriter
	{
		private readonly Dictionary<SyntaxNode, LabelSymbol> _sectionLabels = PooledDictionary<SyntaxNode, LabelSymbol>.GetInstance();

		public static BoundStatement Rewrite(LocalRewriter localRewriter, BoundSwitchStatement node)
		{
			SwitchStatementLocalRewriter switchStatementLocalRewriter = new SwitchStatementLocalRewriter(node, localRewriter);
			BoundStatement result = switchStatementLocalRewriter.LowerSwitchStatement(node);
			switchStatementLocalRewriter.Free();
			return result;
		}

		protected override LabelSymbol GetDagNodeLabel(BoundDecisionDagNode dag)
		{
			LabelSymbol dagNodeLabel = base.GetDagNodeLabel(dag);
			if (dag is BoundLeafDecisionDagNode boundLeafDecisionDagNode)
			{
				SyntaxNode parent = boundLeafDecisionDagNode.Syntax.Parent;
				if (parent != null && parent.Kind() == SyntaxKind.SwitchSection)
				{
					if (_sectionLabels.TryGetValue(parent, out LabelSymbol value))
					{
						return value;
					}
					_sectionLabels.Add(parent, dagNodeLabel);
				}
			}
			return dagNodeLabel;
		}

		private SwitchStatementLocalRewriter(BoundSwitchStatement node, LocalRewriter localRewriter)
			: base(node.Syntax, localRewriter, node.SwitchSections.SelectAsArray((BoundSwitchSection section) => section.Syntax), localRewriter.Instrument && !node.WasCompilerGenerated)
		{
		}

		private BoundStatement LowerSwitchStatement(BoundSwitchStatement node)
		{
			_factory.Syntax = node.Syntax;
			ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
			ArrayBuilder<LocalSymbol> instance2 = ArrayBuilder<LocalSymbol>.GetInstance();
			BoundExpression boundExpression = _localRewriter.VisitExpression(node.Expression);
			if (!node.WasCompilerGenerated && _localRewriter.Instrument)
			{
				BoundExpression boundExpression2 = _localRewriter.Instrumenter.InstrumentSwitchStatementExpression(node, boundExpression, _factory);
				if (boundExpression.ConstantValueOpt == null)
				{
					boundExpression = boundExpression2;
				}
				else
				{
					instance.Add(_factory.ExpressionStatement(boundExpression2));
				}
			}
			instance2.AddRange(node.InnerLocals);
			BoundDecisionDag decisionDag = ShareTempsIfPossibleAndEvaluateInput(node.GetDecisionDagForLowering(_factory.Compilation), boundExpression, instance, out var _);
			if (base.GenerateInstrumentation)
			{
				if (instance.Count == 0)
				{
					instance.Add(_factory.NoOp(NoOpStatementFlavor.Default));
				}
				instance.Add(_factory.HiddenSequencePoint());
			}
			var (statements, immutableDictionary) = LowerDecisionDag(decisionDag);
			if ((object)_whenNodeIdentifierLocal != null)
			{
				instance2.Add(_whenNodeIdentifierLocal);
			}
			instance.Add(_factory.Block(statements));
			foreach (BoundSwitchSection switchSection in node.SwitchSections)
			{
				_factory.Syntax = switchSection.Syntax;
				ArrayBuilder<BoundStatement> instance3 = ArrayBuilder<BoundStatement>.GetInstance();
				instance3.AddRange(immutableDictionary[switchSection.Syntax]);
				foreach (BoundSwitchLabel switchLabel in switchSection.SwitchLabels)
				{
					instance3.Add(_factory.Label(switchLabel.Label));
				}
				instance3.AddRange(_localRewriter.VisitList(switchSection.Statements));
				ImmutableArray<BoundStatement> statements2 = instance3.ToImmutableAndFree();
				if (switchSection.Locals.IsEmpty)
				{
					instance.Add(_factory.StatementList(statements2));
					continue;
				}
				instance2.AddRange(switchSection.Locals);
				instance.Add(new BoundScope(switchSection.Syntax, switchSection.Locals, statements2));
			}
			instance2.AddRange(_tempAllocator.AllTemps());
			_factory.Syntax = node.Syntax;
			if (base.GenerateInstrumentation)
			{
				instance.Add(_factory.HiddenSequencePoint());
			}
			instance.Add(_factory.Label(node.BreakLabel));
			BoundStatement boundStatement = _factory.Block(instance2.ToImmutableAndFree(), node.InnerLocalFunctions, instance.ToImmutableAndFree());
			if (base.GenerateInstrumentation)
			{
				boundStatement = _localRewriter.Instrumenter.InstrumentSwitchStatement(node, boundStatement);
			}
			return boundStatement;
		}
	}

	private enum StringConcatenationRewriteKind
	{
		AllStrings,
		AllStringsOrChars,
		InvolvesObjects
	}

	private struct WellKnownConcatRelatedMethods(CSharpCompilation compilation)
	{
		private readonly CSharpCompilation _compilation = compilation;

		private MethodSymbol? _concatStringString = ErrorMethodSymbol.UnknownMethod;

		private MethodSymbol? _concatStringStringString = ErrorMethodSymbol.UnknownMethod;

		private MethodSymbol? _concatStringStringStringString = ErrorMethodSymbol.UnknownMethod;

		private MethodSymbol? _concatStringArray = ErrorMethodSymbol.UnknownMethod;

		private MethodSymbol? _objectToString = ErrorMethodSymbol.UnknownMethod;

		public bool IsWellKnownConcatMethod(BoundCall call, out ImmutableArray<BoundExpression> arguments)
		{
			if (!call.ArgsToParamsOpt.IsDefault)
			{
				arguments = default(ImmutableArray<BoundExpression>);
				return false;
			}
			if (IsConcatNonArray(call, ref _concatStringString, SpecialMember.System_String__ConcatStringString, out arguments) || IsConcatNonArray(call, ref _concatStringStringString, SpecialMember.System_String__ConcatStringStringString, out arguments) || IsConcatNonArray(call, ref _concatStringStringStringString, SpecialMember.System_String__ConcatStringStringStringString, out arguments))
			{
				return true;
			}
			InitializeField(ref _concatStringArray, SpecialMember.System_String__ConcatStringArray);
			if ((object)call.Method == _concatStringArray && call.Arguments[0] is BoundArrayCreation boundArrayCreation)
			{
				arguments = boundArrayCreation.InitializerOpt?.Initializers ?? ImmutableArray<BoundExpression>.Empty;
				return true;
			}
			arguments = default(ImmutableArray<BoundExpression>);
			return false;
		}

		public bool IsCharToString(BoundCall call, [NotNullWhen(true)] out BoundExpression? charExpression)
		{
			InitializeField(ref _objectToString, SpecialMember.System_Object__ToString);
			if (call != null && call.Arguments.Length == 0)
			{
				BoundExpression receiverOpt = call.ReceiverOpt;
				if (receiverOpt != null)
				{
					TypeSymbol type = receiverOpt.Type;
					if (type is NamedTypeSymbol accessingTypeOpt && type.SpecialType == SpecialType.System_Char)
					{
						MethodSymbol method = call.Method;
						if ((object)method != null && method.Name == "ToString" && (object)method.GetLeastOverriddenMethod(accessingTypeOpt) == _objectToString)
						{
							charExpression = call.ReceiverOpt;
							return true;
						}
					}
				}
			}
			charExpression = null;
			return false;
		}

		private readonly void InitializeField(ref MethodSymbol? member, SpecialMember specialMember)
		{
			if ((object)member == ErrorMethodSymbol.UnknownMethod)
			{
				member = _compilation.GetSpecialTypeMember(specialMember) as MethodSymbol;
			}
		}

		private readonly bool IsConcatNonArray(BoundCall call, ref MethodSymbol? concatMethod, SpecialMember concatSpecialMember, out ImmutableArray<BoundExpression> arguments)
		{
			InitializeField(ref concatMethod, concatSpecialMember);
			if ((object)call.Method == concatMethod)
			{
				arguments = call.Arguments;
				return true;
			}
			arguments = default(ImmutableArray<BoundExpression>);
			return false;
		}
	}

	private readonly struct InterpolationHandlerResult
	{
		private readonly ImmutableArray<BoundStatement> _statements;

		private readonly ImmutableArray<BoundExpression> _expressions;

		private readonly LocalRewriter _rewriter;

		private readonly LocalSymbol? _outTemp;

		public readonly BoundLocal HandlerTemp;

		public InterpolationHandlerResult(ImmutableArray<BoundStatement> statements, BoundLocal handlerTemp, LocalSymbol outTemp, LocalRewriter rewriter)
		{
			_statements = statements;
			_expressions = default(ImmutableArray<BoundExpression>);
			_outTemp = outTemp;
			HandlerTemp = handlerTemp;
			_rewriter = rewriter;
		}

		public InterpolationHandlerResult(ImmutableArray<BoundExpression> expressions, BoundLocal handlerTemp, LocalSymbol? outTemp, LocalRewriter rewriter)
		{
			_statements = default(ImmutableArray<BoundStatement>);
			_expressions = expressions;
			_outTemp = outTemp;
			HandlerTemp = handlerTemp;
			_rewriter = rewriter;
		}

		public BoundExpression WithFinalResult(BoundExpression result)
		{
			ImmutableArray<LocalSymbol> locals = ((_outTemp != null) ? ImmutableArray.Create<LocalSymbol>(HandlerTemp.LocalSymbol, _outTemp) : ImmutableArray.Create(HandlerTemp.LocalSymbol));
			if (_statements.IsDefault)
			{
				return _rewriter._factory.Sequence(locals, _expressions, result);
			}
			_rewriter._needsSpilling = true;
			return _rewriter._factory.SpillSequence(locals, _statements, result);
		}
	}

	private sealed class SwitchExpressionLocalRewriter : BaseSwitchLocalRewriter
	{
		private SwitchExpressionLocalRewriter(BoundConvertedSwitchExpression node, LocalRewriter localRewriter)
			: base(node.Syntax, localRewriter, node.SwitchArms.SelectAsArray((BoundSwitchExpressionArm arm) => arm.Syntax), !node.WasCompilerGenerated && localRewriter.Instrument)
		{
		}

		public static BoundExpression Rewrite(LocalRewriter localRewriter, BoundConvertedSwitchExpression node)
		{
			SwitchExpressionLocalRewriter switchExpressionLocalRewriter = new SwitchExpressionLocalRewriter(node, localRewriter);
			BoundExpression result = switchExpressionLocalRewriter.LowerSwitchExpression(node);
			switchExpressionLocalRewriter.Free();
			return result;
		}

		private BoundExpression LowerSwitchExpression(BoundConvertedSwitchExpression node)
		{
			bool flag = base.GenerateInstrumentation && _localRewriter._compilation.Options.OptimizationLevel != OptimizationLevel.Release;
			_factory.Syntax = node.Syntax;
			ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
			ArrayBuilder<LocalSymbol> instance2 = ArrayBuilder<LocalSymbol>.GetInstance();
			BoundExpression loweredSwitchGoverningExpression = _localRewriter.VisitExpression(node.Expression);
			BoundDecisionDag decisionDag = ShareTempsIfPossibleAndEvaluateInput(node.GetDecisionDagForLowering(_factory.Compilation, out LabelSymbol defaultLabel), loweredSwitchGoverningExpression, instance, out var savedInputExpression);
			object identifier = new object();
			object identifier2 = new object();
			var (statements, immutableDictionary) = LowerDecisionDag(decisionDag);
			if ((object)_whenNodeIdentifierLocal != null)
			{
				instance2.Add(_whenNodeIdentifierLocal);
			}
			if (flag)
			{
				SwitchExpressionSyntax switchExpressionSyntax = (SwitchExpressionSyntax)node.Syntax;
				instance.Add(new BoundSavePreviousSequencePoint(switchExpressionSyntax, identifier));
				int start = switchExpressionSyntax.SwitchKeyword.Span.Start;
				int end = switchExpressionSyntax.Span.End;
				instance.Add(new BoundStepThroughSequencePoint(span: new TextSpan(start, end - start), syntax: node.Syntax));
				instance.Add(new BoundSavePreviousSequencePoint(switchExpressionSyntax, identifier2));
			}
			instance.Add(_factory.Block(statements));
			LocalSymbol localSymbol = _factory.SynthesizedLocal(node.Type, node.Syntax);
			LabelSymbol label = _factory.GenerateLabel("afterSwitchExpression");
			foreach (BoundSwitchExpressionArm switchArm in node.SwitchArms)
			{
				_factory.Syntax = switchArm.Syntax;
				ArrayBuilder<BoundStatement> instance3 = ArrayBuilder<BoundStatement>.GetInstance();
				instance3.AddRange(immutableDictionary[switchArm.Syntax]);
				instance3.Add(_factory.Label(switchArm.Label));
				BoundExpression boundExpression = _localRewriter.VisitExpression(switchArm.Value);
				if (base.GenerateInstrumentation)
				{
					boundExpression = _localRewriter.Instrumenter.InstrumentSwitchExpressionArmExpression(switchArm.Value, boundExpression, _factory);
				}
				instance3.Add(_factory.Assignment(_factory.Local(localSymbol), boundExpression));
				instance3.Add(_factory.Goto(label));
				ImmutableArray<BoundStatement> statements2 = instance3.ToImmutableAndFree();
				if (switchArm.Locals.IsEmpty)
				{
					instance.Add(_factory.StatementList(statements2));
					continue;
				}
				instance2.AddRange(switchArm.Locals);
				instance.Add(new BoundScope(switchArm.Syntax, switchArm.Locals, statements2));
			}
			_factory.Syntax = node.Syntax;
			BoundStatement item;
			if ((object)defaultLabel != null)
			{
				instance.Add(_factory.Label(defaultLabel));
				if (flag)
				{
					instance.Add(new BoundRestorePreviousSequencePoint(node.Syntax, identifier2));
				}
				NamedTypeSymbol type = _factory.SpecialType(SpecialType.System_Object);
				Conversion? conversion = tryGetImplicitConversion(savedInputExpression, type);
				if (conversion.HasValue)
				{
					Conversion valueOrDefault = conversion.GetValueOrDefault();
					if (_factory.WellKnownMember(WellKnownMember.System_Runtime_CompilerServices_SwitchExpressionException__ctorObject, isOptional: true) is MethodSymbol)
					{
						item = ConstructThrowSwitchExpressionExceptionHelperCall(_factory, _factory.Convert(type, savedInputExpression, valueOrDefault));
						goto IL_037b;
					}
				}
				item = ((_factory.WellKnownMember(WellKnownMember.System_Runtime_CompilerServices_SwitchExpressionException__ctor, isOptional: true) is MethodSymbol) ? ConstructThrowSwitchExpressionExceptionParameterlessHelperCall(_factory) : ConstructThrowInvalidOperationExceptionHelperCall(_factory));
				goto IL_037b;
			}
			goto IL_0383;
			IL_0383:
			if (base.GenerateInstrumentation)
			{
				instance.Add(_factory.HiddenSequencePoint());
			}
			instance.Add(_factory.Label(label));
			if (flag)
			{
				instance.Add(new BoundRestorePreviousSequencePoint(node.Syntax, identifier));
			}
			instance2.Add(localSymbol);
			instance2.AddRange(_tempAllocator.AllTemps());
			return _factory.SpillSequence(instance2.ToImmutableAndFree(), instance.ToImmutableAndFree(), _factory.Local(localSymbol));
			IL_037b:
			instance.Add(item);
			goto IL_0383;
			Conversion? tryGetImplicitConversion(BoundExpression expression, TypeSymbol destination)
			{
				CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
				Conversion value = _localRewriter._compilation.Conversions.ClassifyConversionFromExpression(expression, destination, isChecked: false, ref useSiteInfo);
				if (value.IsImplicit)
				{
					return value;
				}
				return null;
			}
		}

		private static BoundStatement ConstructThrowSwitchExpressionExceptionHelperCall(SyntheticBoundNodeFactory factory, BoundExpression unmatchedValue)
		{
			PEModuleBuilder? moduleBuilderOpt = factory.ModuleBuilderOpt;
			CSharpSyntaxNode nonNullSyntaxNode = factory.CurrentFunction.GetNonNullSyntaxNode();
			DiagnosticBag diagnosticBag = factory.Diagnostics.DiagnosticBag;
			MethodSymbol method = moduleBuilderOpt.EnsureThrowSwitchExpressionExceptionExists(nonNullSyntaxNode, factory, diagnosticBag);
			BoundCall expr = factory.Call(null, method, unmatchedValue);
			return factory.HiddenSequencePoint(factory.ExpressionStatement(expr));
		}

		private static BoundStatement ConstructThrowSwitchExpressionExceptionParameterlessHelperCall(SyntheticBoundNodeFactory factory)
		{
			PEModuleBuilder? moduleBuilderOpt = factory.ModuleBuilderOpt;
			CSharpSyntaxNode nonNullSyntaxNode = factory.CurrentFunction.GetNonNullSyntaxNode();
			DiagnosticBag diagnosticBag = factory.Diagnostics.DiagnosticBag;
			MethodSymbol method = moduleBuilderOpt.EnsureThrowSwitchExpressionExceptionParameterlessExists(nonNullSyntaxNode, factory, diagnosticBag);
			BoundCall expr = factory.Call(null, method);
			return factory.HiddenSequencePoint(factory.ExpressionStatement(expr));
		}

		private static BoundStatement ConstructThrowInvalidOperationExceptionHelperCall(SyntheticBoundNodeFactory factory)
		{
			PEModuleBuilder? moduleBuilderOpt = factory.ModuleBuilderOpt;
			CSharpSyntaxNode nonNullSyntaxNode = factory.CurrentFunction.GetNonNullSyntaxNode();
			DiagnosticBag diagnosticBag = factory.Diagnostics.DiagnosticBag;
			MethodSymbol method = moduleBuilderOpt.EnsureThrowInvalidOperationExceptionExists(nonNullSyntaxNode, factory, diagnosticBag);
			BoundCall expr = factory.Call(null, method);
			return factory.HiddenSequencePoint(factory.ExpressionStatement(expr));
		}
	}

	private readonly CSharpCompilation _compilation;

	private readonly SyntheticBoundNodeFactory _factory;

	private readonly SynthesizedSubmissionFields _previousSubmissionFields;

	private readonly bool _allowOmissionOfConditionalCalls;

	private LoweredDynamicOperationFactory _dynamicFactory;

	private bool _sawLambdas;

	private int _availableLocalFunctionOrdinal;

	private readonly int _topLevelMethodOrdinal;

	private DelegateCacheRewriter? _lazyDelegateCacheRewriter;

	private bool _inExpressionLambda;

	private ArrayBuilder<LocalSymbol>? _additionalLocals;

	private BoundBlock? _currentLambdaBody;

	private bool _sawAwait;

	private bool _sawAwaitInExceptionHandler;

	private bool _needsSpilling;

	private readonly BindingDiagnosticBag _diagnostics;

	private readonly BoundStatement _rootStatement;

	private Dictionary<BoundValuePlaceholderBase, BoundExpression>? _placeholderReplacementMapDoNotUseDirectly;

	private BoundExpression? _currentConditionalAccessTarget;

	private int _currentConditionalAccessID;

	private Dictionary<BoundNode, HashSet<LabelSymbol>>? _lazyUnmatchedLabelCache;

	private static readonly AwaitDebugId s_moveNextAsyncAwaitId = new AwaitDebugId(0);

	private static readonly AwaitDebugId s_disposeAsyncAwaitId = new AwaitDebugId(1);

	internal SyntheticBoundNodeFactory Factory => _factory;

	internal BoundBlock? CurrentLambdaBody => _currentLambdaBody;

	internal BoundStatement CurrentMethodBody => _rootStatement;

	private InstrumentationState InstrumentationState => _factory.InstrumentationState;

	private bool Instrument => !InstrumentationState.IsSuppressed;

	private Instrumenter Instrumenter => InstrumentationState.Instrumenter;

	private PEModuleBuilder? EmitModule => _factory.CompilationState.ModuleBuilderOpt;

	private bool IsLambdaOrExpressionBodiedMember
	{
		get
		{
			MethodSymbol currentFunction = _factory.CurrentFunction;
			if (currentFunction is LambdaSymbol)
			{
				return true;
			}
			return (currentFunction as SourceMemberMethodSymbol)?.IsExpressionBodied ?? (currentFunction as LocalFunctionSymbol)?.IsExpressionBodied ?? false;
		}
	}

	private LocalRewriter(CSharpCompilation compilation, MethodSymbol containingMethod, int containingMethodOrdinal, BoundStatement rootStatement, NamedTypeSymbol? containingType, SyntheticBoundNodeFactory factory, SynthesizedSubmissionFields previousSubmissionFields, bool allowOmissionOfConditionalCalls, BindingDiagnosticBag diagnostics)
	{
		_compilation = compilation;
		_factory = factory;
		_factory.CurrentFunction = containingMethod;
		_dynamicFactory = new LoweredDynamicOperationFactory(factory, containingMethodOrdinal);
		_previousSubmissionFields = previousSubmissionFields;
		_allowOmissionOfConditionalCalls = allowOmissionOfConditionalCalls;
		_topLevelMethodOrdinal = containingMethodOrdinal;
		_diagnostics = diagnostics;
		_rootStatement = rootStatement;
	}

	public static BoundStatement Rewrite(CSharpCompilation compilation, MethodSymbol method, int methodOrdinal, NamedTypeSymbol containingType, BoundStatement statement, TypeCompilationState compilationState, SynthesizedSubmissionFields previousSubmissionFields, bool allowOmissionOfConditionalCalls, MethodInstrumentation instrumentation, DebugDocumentProvider debugDocumentProvider, BindingDiagnosticBag diagnostics, out ImmutableArray<SourceSpan> codeCoverageSpans, out bool sawLambdas, out bool sawLocalFunctions, out bool sawAwaitInExceptionHandler)
	{
		try
		{
			InstrumentationState instrumentationState = new InstrumentationState();
			SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(method, statement.Syntax, compilationState, diagnostics, instrumentationState);
			Instrumenter previous = Microsoft.CodeAnalysis.CSharp.Instrumenter.NoOp;
			if (instrumentation.Kinds.Contains((InstrumentationKind)(-1)) && LocalStateTracingInstrumenter.TryCreate(method, statement, syntheticBoundNodeFactory, diagnostics, previous, out LocalStateTracingInstrumenter instrumenter))
			{
				previous = instrumenter;
			}
			CodeCoverageInstrumenter instrumenter2 = null;
			if (instrumentation.Kinds.Contains(InstrumentationKind.TestCoverage) && CodeCoverageInstrumenter.TryCreate(method, statement, syntheticBoundNodeFactory, diagnostics, debugDocumentProvider, previous, out instrumenter2))
			{
				previous = instrumenter2;
			}
			StackOverflowProbingInstrumenter instrumenter3 = null;
			if (instrumentation.Kinds.Contains(InstrumentationKind.StackOverflowProbing) && StackOverflowProbingInstrumenter.TryCreate(method, syntheticBoundNodeFactory, previous, out instrumenter3))
			{
				previous = instrumenter3;
			}
			ModuleCancellationInstrumenter instrumenter4 = null;
			if (instrumentation.Kinds.Contains(InstrumentationKind.ModuleCancellation) && ModuleCancellationInstrumenter.TryCreate(method, syntheticBoundNodeFactory, previous, out instrumenter4))
			{
				previous = instrumenter4;
			}
			instrumentationState.Instrumenter = DebugInfoInjector.Create(previous);
			LocalRewriter localRewriter = new LocalRewriter(compilation, method, methodOrdinal, statement, containingType, syntheticBoundNodeFactory, previousSubmissionFields, allowOmissionOfConditionalCalls, diagnostics);
			BoundStatement boundStatement = localRewriter.VisitStatement(statement);
			sawLambdas = localRewriter._sawLambdas;
			sawLocalFunctions = localRewriter._availableLocalFunctionOrdinal != 0;
			sawAwaitInExceptionHandler = localRewriter._sawAwaitInExceptionHandler;
			if (localRewriter._needsSpilling && !boundStatement.HasErrors)
			{
				boundStatement = SpillSequenceSpiller.Rewrite(boundStatement, method, compilationState, diagnostics);
			}
			codeCoverageSpans = instrumenter2?.DynamicAnalysisSpans ?? ImmutableArray<SourceSpan>.Empty;
			return boundStatement;
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
			sawLambdas = (sawLocalFunctions = (sawAwaitInExceptionHandler = false));
			codeCoverageSpans = ImmutableArray<SourceSpan>.Empty;
			return new BoundBadStatement(statement.Syntax, ImmutableArray.Create((BoundNode)statement), hasErrors: true);
		}
	}

	public override BoundNode? Visit(BoundNode? node)
	{
		if (node == null)
		{
			return node;
		}
		if (node is BoundExpression node2)
		{
			return VisitExpressionImpl(node2);
		}
		return node.Accept(this);
	}

	[return: NotNullIfNotNull("node")]
	private BoundExpression? VisitExpression(BoundExpression? node)
	{
		if (node == null)
		{
			return node;
		}
		return VisitExpressionImpl(node);
	}

	private BoundStatement? VisitStatement(BoundStatement? node)
	{
		if (node == null)
		{
			return node;
		}
		return (BoundStatement)node.Accept(this);
	}

	private BoundExpression? VisitExpressionImpl(BoundExpression node)
	{
		if (node is BoundNameOfOperator boundNameOfOperator)
		{
			IdentifierNameSyntax identifierNameSyntax = (IdentifierNameSyntax)((InvocationExpressionSyntax)boundNameOfOperator.Syntax).Expression;
			if (_compilation.TryGetInterceptor(identifierNameSyntax).HasValue)
			{
				_diagnostics.Add(ErrorCode.ERR_InterceptorCannotInterceptNameof, identifierNameSyntax.Location);
			}
		}
		ConstantValue constantValueOpt = node.ConstantValueOpt;
		if (constantValueOpt != null)
		{
			TypeSymbol type = node.Type;
			if ((object)type == null || !type.IsNullableType())
			{
				BoundExpression boundExpression = MakeLiteral(node.Syntax, constantValueOpt, type);
				if (node.WasCompilerGenerated)
				{
					boundExpression.MakeCompilerGenerated();
				}
				return boundExpression;
			}
		}
		BoundExpression boundExpression2 = (BoundExpression)VisitExpressionOrPatternWithStackGuard(node);
		bool flag = boundExpression2 != null && boundExpression2 != node;
		if (flag)
		{
			BoundKind kind = node.Kind;
			bool flag2 = ((kind == BoundKind.ValuePlaceholder || kind == BoundKind.ObjectOrCollectionValuePlaceholder || kind == BoundKind.ImplicitReceiver) ? true : false);
			flag = !flag2;
		}
		if (flag && !CanBePassedByReference(node) && CanBePassedByReference(boundExpression2))
		{
			boundExpression2 = RefAccessMustMakeCopy(boundExpression2);
		}
		return boundExpression2;
	}

	private static BoundExpression RefAccessMustMakeCopy(BoundExpression visited)
	{
		visited = new BoundPassByCopy(visited.Syntax, visited, visited.Type);
		return visited;
	}

	private static bool IsUnusedDeconstruction(BoundExpression node)
	{
		if (node.Kind == BoundKind.DeconstructionAssignmentOperator)
		{
			return !((BoundDeconstructionAssignmentOperator)node).IsUsed;
		}
		return false;
	}

	public override BoundNode? VisitParameter(BoundParameter node)
	{
		if (node.ParameterSymbol.ContainingSymbol is SynthesizedPrimaryConstructor synthesizedPrimaryConstructor && synthesizedPrimaryConstructor.GetCapturedParameters().TryGetValue(node.ParameterSymbol, out FieldSymbol value))
		{
			return new BoundFieldAccess(node.Syntax, new BoundThisReference(node.Syntax, synthesizedPrimaryConstructor.ContainingType), value, null, LookupResultKind.Viable, node.Type);
		}
		return base.VisitParameter(node);
	}

	public override BoundNode VisitLambda(BoundLambda node)
	{
		NamedTypeSymbol delegateType = node.Type.GetDelegateType();
		if ((object)delegateType != null && delegateType.IsAnonymousType && delegateType.ContainingModule == _compilation.SourceModule)
		{
			MethodSymbol methodSymbol = delegateType.DelegateInvokeMethod();
			if ((object)methodSymbol != null && methodSymbol.Parameters.Any((ParameterSymbol p) => p.IsParamsCollection))
			{
				ParameterSymbol parameterSymbol = node.Symbol.Parameters.LastOrDefault((ParameterSymbol p) => p.IsParamsCollection);
				Location location = (((object)parameterSymbol == null) ? node.Syntax.Location : ParameterHelpers.GetParameterLocation(parameterSymbol));
				_factory.ModuleBuilderOpt.EnsureParamCollectionAttributeExists(_diagnostics, location);
			}
		}
		_sawLambdas = true;
		MethodSymbol symbol = node.Symbol;
		CheckRefReadOnlySymbols(symbol);
		MethodSymbol currentFunction = _factory.CurrentFunction;
		Instrumenter instrumenter = InstrumentationState.Instrumenter;
		BoundBlock currentLambdaBody = _currentLambdaBody;
		ArrayBuilder<LocalSymbol> additionalLocals = _additionalLocals;
		try
		{
			_currentLambdaBody = node.Body;
			_additionalLocals = null;
			_factory.CurrentFunction = symbol;
			if (symbol.IsDirectlyExcludedFromCodeCoverage)
			{
				InstrumentationState.RemoveCodeCoverageInstrumenter();
			}
			return base.VisitLambda(node);
		}
		finally
		{
			_factory.CurrentFunction = currentFunction;
			InstrumentationState.Instrumenter = instrumenter;
			_currentLambdaBody = currentLambdaBody;
			_additionalLocals = additionalLocals;
		}
	}

	public override BoundNode VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
	{
		int localFunctionOrdinal = _availableLocalFunctionOrdinal++;
		MethodSymbol symbol = node.Symbol;
		CheckRefReadOnlySymbols(symbol);
		PEModuleBuilder moduleBuilderOpt = _factory.CompilationState.ModuleBuilderOpt;
		if (moduleBuilderOpt != null)
		{
			ImmutableArray<TypeParameterSymbol> typeParameters = symbol.TypeParameters;
			if (typeParameters.Any((TypeParameterSymbol typeParameter) => typeParameter.HasUnmanagedTypeConstraint))
			{
				moduleBuilderOpt.EnsureIsUnmanagedAttributeExists();
			}
			if (_compilation.ShouldEmitNativeIntegerAttributes() && (hasReturnTypeOrParameter(symbol, (TypeWithAnnotations t) => t.ContainsNativeIntegerWrapperType()) || typeParameters.Any((TypeParameterSymbol t) => t.ConstraintTypesNoUseSiteDiagnostics.Any((TypeWithAnnotations type) => type.ContainsNativeIntegerWrapperType()))))
			{
				moduleBuilderOpt.EnsureNativeIntegerAttributeExists();
			}
			if (_factory.CompilationState.Compilation.ShouldEmitNullableAttributes(symbol) && (typeParameters.Any((TypeParameterSymbol typeParameter) => ((SourceTypeParameterSymbol)typeParameter).ConstraintsNeedNullableAttribute()) || hasReturnTypeOrParameter(symbol, (TypeWithAnnotations t) => t.NeedsNullableAttribute())))
			{
				moduleBuilderOpt.EnsureNullableAttributeExists();
			}
		}
		MethodSymbol currentFunction = _factory.CurrentFunction;
		Instrumenter instrumenter = InstrumentationState.Instrumenter;
		LoweredDynamicOperationFactory dynamicFactory = _dynamicFactory;
		BoundBlock currentLambdaBody = _currentLambdaBody;
		ArrayBuilder<LocalSymbol> additionalLocals = _additionalLocals;
		try
		{
			_currentLambdaBody = node.Body;
			_additionalLocals = null;
			_factory.CurrentFunction = symbol;
			if (symbol.IsDirectlyExcludedFromCodeCoverage)
			{
				InstrumentationState.RemoveCodeCoverageInstrumenter();
			}
			if (symbol.IsGenericMethod)
			{
				_dynamicFactory = new LoweredDynamicOperationFactory(_factory, _dynamicFactory.MethodOrdinal, localFunctionOrdinal);
			}
			return base.VisitLocalFunctionStatement(node);
		}
		finally
		{
			_factory.CurrentFunction = currentFunction;
			InstrumentationState.Instrumenter = instrumenter;
			_dynamicFactory = dynamicFactory;
			_currentLambdaBody = currentLambdaBody;
			_additionalLocals = additionalLocals;
		}
		static bool hasReturnTypeOrParameter(MethodSymbol localFunction, Func<TypeWithAnnotations, bool> predicate)
		{
			if (!predicate(localFunction.ReturnTypeWithAnnotations))
			{
				return localFunction.ParameterTypesWithAnnotations.Any(predicate);
			}
			return true;
		}
	}

	public override BoundNode VisitDefaultLiteral(BoundDefaultLiteral node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter.cs", 461);
	}

	public override BoundNode VisitUnconvertedObjectCreationExpression(BoundUnconvertedObjectCreationExpression node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter.cs", 466);
	}

	public override BoundNode VisitValuePlaceholder(BoundValuePlaceholder node)
	{
		return PlaceholderReplacement(node);
	}

	public override BoundNode VisitDeconstructValuePlaceholder(BoundDeconstructValuePlaceholder node)
	{
		return PlaceholderReplacement(node);
	}

	public override BoundNode VisitObjectOrCollectionValuePlaceholder(BoundObjectOrCollectionValuePlaceholder node)
	{
		if (_inExpressionLambda)
		{
			return node;
		}
		return PlaceholderReplacement(node);
	}

	public override BoundNode VisitInterpolatedStringArgumentPlaceholder(BoundInterpolatedStringArgumentPlaceholder node)
	{
		return PlaceholderReplacement(node);
	}

	public override BoundNode? VisitInterpolatedStringHandlerPlaceholder(BoundInterpolatedStringHandlerPlaceholder node)
	{
		return PlaceholderReplacement(node);
	}

	public override BoundNode? VisitCollectionExpressionSpreadExpressionPlaceholder(BoundCollectionExpressionSpreadExpressionPlaceholder node)
	{
		return PlaceholderReplacement(node);
	}

	private BoundExpression PlaceholderReplacement(BoundValuePlaceholderBase placeholder)
	{
		return _placeholderReplacementMapDoNotUseDirectly[placeholder];
	}

	[Conditional("DEBUG")]
	private static void AssertPlaceholderReplacement(BoundValuePlaceholderBase placeholder, BoundExpression value)
	{
	}

	private void AddPlaceholderReplacement(BoundValuePlaceholderBase placeholder, BoundExpression value)
	{
		if (_placeholderReplacementMapDoNotUseDirectly == null)
		{
			_placeholderReplacementMapDoNotUseDirectly = new Dictionary<BoundValuePlaceholderBase, BoundExpression>();
		}
		_placeholderReplacementMapDoNotUseDirectly.Add(placeholder, value);
	}

	private void RemovePlaceholderReplacement(BoundValuePlaceholderBase placeholder)
	{
		_placeholderReplacementMapDoNotUseDirectly.Remove(placeholder);
	}

	public sealed override BoundNode VisitOutDeconstructVarPendingInference(OutDeconstructVarPendingInference node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter.cs", 563);
	}

	public override BoundNode VisitDeconstructionVariablePendingInference(DeconstructionVariablePendingInference node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter.cs", 569);
	}

	public override BoundNode VisitBadExpression(BoundBadExpression node)
	{
		return node;
	}

	private static BoundExpression BadExpression(BoundExpression node)
	{
		return BadExpression(node.Syntax, node.Type, ImmutableArray.Create(node));
	}

	private static BoundExpression BadExpression(SyntaxNode syntax, TypeSymbol resultType, BoundExpression child)
	{
		return BadExpression(syntax, resultType, ImmutableArray.Create(child));
	}

	private static BoundExpression BadExpression(SyntaxNode syntax, TypeSymbol resultType, BoundExpression child1, BoundExpression child2)
	{
		return BadExpression(syntax, resultType, ImmutableArray.Create(child1, child2));
	}

	private static BoundExpression BadExpression(SyntaxNode syntax, TypeSymbol resultType, ImmutableArray<BoundExpression> children)
	{
		return new BoundBadExpression(syntax, LookupResultKind.NotReferencable, ImmutableArray<Symbol>.Empty, children, resultType);
	}

	private bool TryGetWellKnownTypeMember<TSymbol>(SyntaxNode? syntax, WellKnownMember member, [NotNullWhen(true)] out TSymbol? symbol, bool isOptional = false, Location? location = null) where TSymbol : Symbol
	{
		CSharpCompilation compilation = _compilation;
		BindingDiagnosticBag diagnostics = _diagnostics;
		bool isOptional2 = isOptional;
		symbol = (TSymbol)Binder.GetWellKnownTypeMember(compilation, member, diagnostics, location, syntax, isOptional2);
		return (object)symbol != null;
	}

	private MethodSymbol UnsafeGetSpecialTypeMethod(SyntaxNode syntax, SpecialMember specialMember)
	{
		return UnsafeGetSpecialTypeMethod(syntax, specialMember, _compilation, _diagnostics);
	}

	private static MethodSymbol UnsafeGetSpecialTypeMethod(SyntaxNode syntax, SpecialMember specialMember, CSharpCompilation compilation, BindingDiagnosticBag diagnostics)
	{
		if (TryGetSpecialTypeMethod(syntax, specialMember, compilation, diagnostics, out MethodSymbol method))
		{
			return method;
		}
		MemberDescriptor descriptor = SpecialMembers.GetDescriptor(specialMember);
		ExtendedSpecialType declaringSpecialType = descriptor.DeclaringSpecialType;
		NamedTypeSymbol specialType = compilation.Assembly.GetSpecialType(declaringSpecialType);
		TypeSymbol returnType = new ExtendedErrorTypeSymbol(compilation, descriptor.Name, descriptor.Arity, null);
		return new ErrorMethodSymbol(specialType, returnType, "Missing");
	}

	private bool TryGetSpecialTypeMethod(SyntaxNode syntax, SpecialMember specialMember, out MethodSymbol method, bool isOptional = false)
	{
		return TryGetSpecialTypeMethod(syntax, specialMember, _compilation, _diagnostics, out method, isOptional);
	}

	private static bool TryGetSpecialTypeMethod(SyntaxNode syntax, SpecialMember specialMember, CSharpCompilation compilation, BindingDiagnosticBag diagnostics, out MethodSymbol method, bool isOptional = false)
	{
		return Binder.TryGetSpecialTypeMember<MethodSymbol>(compilation, specialMember, syntax, diagnostics, out method, isOptional);
	}

	public override BoundNode VisitTypeOfOperator(BoundTypeOfOperator node)
	{
		BoundTypeExpression sourceType = (BoundTypeExpression)Visit(node.SourceType);
		TypeSymbol type = VisitType(node.Type);
		if (!((!(node.Type.ExtendedSpecialType == InternalSpecialType.System_Type)) ? TryGetWellKnownTypeMember<MethodSymbol>(node.Syntax, WellKnownMember.System_Type__GetTypeFromHandle, out MethodSymbol symbol) : TryGetSpecialTypeMethod(node.Syntax, SpecialMember.System_Type__GetTypeFromHandle, out symbol)))
		{
			return new BoundTypeOfOperator(node.Syntax, sourceType, null, type, hasErrors: true);
		}
		return node.Update(sourceType, symbol, type);
	}

	public override BoundNode VisitRefTypeOperator(BoundRefTypeOperator node)
	{
		BoundExpression operand = VisitExpression(node.Operand);
		TypeSymbol type = VisitType(node.Type);
		if (!TryGetWellKnownTypeMember<MethodSymbol>(node.Syntax, WellKnownMember.System_Type__GetTypeFromHandle, out MethodSymbol symbol))
		{
			return new BoundRefTypeOperator(node.Syntax, operand, null, type, hasErrors: true);
		}
		return node.Update(operand, symbol, type);
	}

	private BoundStatement? RewriteFieldOrPropertyInitializer(BoundStatement initializer)
	{
		ArrayBuilder<LocalSymbol> additionalLocals = _additionalLocals;
		if (additionalLocals == null)
		{
			_additionalLocals = ArrayBuilder<LocalSymbol>.GetInstance();
		}
		try
		{
			if (initializer.Kind == BoundKind.Block)
			{
				BoundBlock boundBlock = (BoundBlock)initializer;
				BoundStatement item = RewriteExpressionStatement((BoundExpressionStatement)boundBlock.Statements.Single(), suppressInstrumentation: true);
				ImmutableArray<LocalSymbol> locals = boundBlock.Locals;
				if (additionalLocals == null)
				{
					locals = locals.AddRange(_additionalLocals);
				}
				return boundBlock.Update(locals, boundBlock.LocalFunctions, boundBlock.HasUnsafeModifier, boundBlock.Instrumentation, ImmutableArray.Create(item));
			}
			BoundStatement boundStatement = RewriteExpressionStatement((BoundExpressionStatement)initializer, suppressInstrumentation: true);
			if (boundStatement == null || additionalLocals != null || _additionalLocals.Count == 0)
			{
				return boundStatement;
			}
			return new BoundBlock(boundStatement.Syntax, _additionalLocals.ToImmutable(), ImmutableArray.Create(boundStatement));
		}
		finally
		{
			if (additionalLocals == null)
			{
				_additionalLocals.Free();
				_additionalLocals = additionalLocals;
			}
		}
	}

	public override BoundNode VisitTypeOrInstanceInitializers(BoundTypeOrInstanceInitializers node)
	{
		ImmutableArray<BoundStatement> statements = node.Statements;
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance(node.Statements.Length);
		foreach (BoundStatement item in statements)
		{
			if (IsFieldOrPropertyInitializer(item))
			{
				instance.Add(RewriteFieldOrPropertyInitializer(item));
			}
			else
			{
				instance.Add(VisitStatement(item));
			}
		}
		int num = 0;
		bool flag = _compilation.Options.OptimizationLevel == OptimizationLevel.Release;
		for (int i = 0; i < instance.Count; i++)
		{
			BoundStatement boundStatement = instance[i];
			if (boundStatement == null || (flag && IsFieldOrPropertyInitializer(statements[i]) && ShouldOptimizeOutInitializer(boundStatement)))
			{
				num++;
				MethodSymbol? currentFunction = _factory.CurrentFunction;
				if ((object)currentFunction != null && !currentFunction.IsStatic)
				{
					instance[i] = null;
				}
			}
		}
		ImmutableArray<BoundStatement> statements2;
		if (num == instance.Count)
		{
			statements2 = ImmutableArray<BoundStatement>.Empty;
			instance.Free();
		}
		else
		{
			int num2 = 0;
			for (int j = 0; j < instance.Count; j++)
			{
				BoundStatement boundStatement2 = instance[j];
				if (boundStatement2 == null)
				{
					continue;
				}
				if (IsFieldOrPropertyInitializer(statements[j]))
				{
					BoundStatement boundStatement3 = statements[j];
					if (Instrument && !boundStatement3.WasCompilerGenerated)
					{
						boundStatement2 = Instrumenter.InstrumentFieldOrPropertyInitializer(boundStatement3, boundStatement2);
					}
				}
				instance[num2] = boundStatement2;
				num2++;
			}
			instance.Count = num2;
			statements2 = instance.ToImmutableAndFree();
		}
		return new BoundStatementList(node.Syntax, statements2, node.HasErrors);
	}

	public override BoundNode VisitArrayAccess(BoundArrayAccess node)
	{
		if (node.Indices.Length != 1)
		{
			return base.VisitArrayAccess(node);
		}
		TypeSymbol? left = VisitType(node.Indices[0].Type);
		SyntheticBoundNodeFactory factory = _factory;
		if (TypeSymbol.Equals(left, _compilation.GetWellKnownType(WellKnownType.System_Range), TypeCompareKind.ConsiderEverything))
		{
			TypeWithAnnotations elementTypeWithAnnotations = ((ArrayTypeSymbol)node.Expression.Type).ElementTypeWithAnnotations;
			return factory.Call(null, factory.WellKnownMethod(WellKnownMember.System_Runtime_CompilerServices_RuntimeHelpers__GetSubArray_T).Construct(ImmutableArray.Create(elementTypeWithAnnotations)), ImmutableArray.Create(VisitExpression(node.Expression), VisitExpression(node.Indices[0])));
		}
		return base.VisitArrayAccess(node);
	}

	internal static bool IsFieldOrPropertyInitializer(BoundStatement initializer)
	{
		SyntaxNode syntax = initializer.Syntax;
		if (syntax.IsKind(SyntaxKind.Parameter))
		{
			return true;
		}
		if (syntax is ExpressionSyntax expressionSyntax)
		{
			CSharpSyntaxNode parent = expressionSyntax.Parent;
			if (parent != null && parent.Kind() == SyntaxKind.EqualsValueClause)
			{
				SyntaxKind syntaxKind = parent.Parent.Kind();
				if (syntaxKind == SyntaxKind.VariableDeclarator || syntaxKind == SyntaxKind.PropertyDeclaration)
				{
					BoundKind kind = initializer.Kind;
					if (kind != BoundKind.Block)
					{
						if (kind == BoundKind.ExpressionStatement)
						{
							goto IL_00a2;
						}
					}
					else
					{
						BoundBlock boundBlock = (BoundBlock)initializer;
						if (boundBlock.Statements.Length == 1)
						{
							initializer = boundBlock.Statements.First();
							if (initializer.Kind == BoundKind.ExpressionStatement)
							{
								goto IL_00a2;
							}
						}
					}
				}
			}
		}
		return false;
		IL_00a2:
		return ((BoundExpressionStatement)initializer).Expression.Kind == BoundKind.AssignmentOperator;
	}

	private static bool ShouldOptimizeOutInitializer(BoundStatement initializer)
	{
		if (initializer.Kind != BoundKind.ExpressionStatement)
		{
			return false;
		}
		if (!(((BoundExpressionStatement)initializer).Expression is BoundAssignmentOperator boundAssignmentOperator))
		{
			return false;
		}
		FieldSymbol fieldSymbol = ((BoundFieldAccess)boundAssignmentOperator.Left).FieldSymbol;
		if (!fieldSymbol.IsStatic && fieldSymbol.ContainingType.IsStructType())
		{
			return false;
		}
		return boundAssignmentOperator.Right.IsDefaultValue();
	}

	internal static bool CanBePassedByReference(BoundExpression expr)
	{
		if (expr.ConstantValueOpt != null)
		{
			return false;
		}
		switch (expr.Kind)
		{
		case BoundKind.PointerIndirectionOperator:
		case BoundKind.PointerElementAccess:
		case BoundKind.RefValueOperator:
		case BoundKind.ArrayAccess:
		case BoundKind.ThisReference:
		case BoundKind.Local:
		case BoundKind.PseudoVariable:
		case BoundKind.Parameter:
		case BoundKind.DiscardExpression:
			return true;
		case BoundKind.DeconstructValuePlaceholder:
			return true;
		case BoundKind.InterpolatedStringArgumentPlaceholder:
			return true;
		case BoundKind.InterpolatedStringHandlerPlaceholder:
			return true;
		case BoundKind.CollectionExpressionSpreadExpressionPlaceholder:
			return true;
		case BoundKind.AwaitableValuePlaceholder:
			return false;
		case BoundKind.EventAccess:
		{
			BoundEventAccess boundEventAccess = (BoundEventAccess)expr;
			if (boundEventAccess.IsUsableAsField)
			{
				if (boundEventAccess.EventSymbol.IsStatic)
				{
					return true;
				}
				if (boundEventAccess.ReceiverOpt.Type.IsValueType)
				{
					return CanBePassedByReference(boundEventAccess.ReceiverOpt);
				}
				return true;
			}
			return false;
		}
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)expr;
			if (!boundFieldAccess.FieldSymbol.IsStatic)
			{
				if (boundFieldAccess.ReceiverOpt.Type.IsValueType)
				{
					return CanBePassedByReference(boundFieldAccess.ReceiverOpt);
				}
				return true;
			}
			return true;
		}
		case BoundKind.Sequence:
			return CanBePassedByReference(((BoundSequence)expr).Value);
		case BoundKind.AssignmentOperator:
			return ((BoundAssignmentOperator)expr).IsRef;
		case BoundKind.ConditionalOperator:
			return ((BoundConditionalOperator)expr).IsRef;
		case BoundKind.Call:
			return ((BoundCall)expr).Method.RefKind != RefKind.None;
		case BoundKind.PropertyAccess:
			return ((BoundPropertyAccess)expr).PropertySymbol.RefKind != RefKind.None;
		case BoundKind.IndexerAccess:
			return ((BoundIndexerAccess)expr).Indexer.RefKind != RefKind.None;
		case BoundKind.ImplicitIndexerAccess:
			return CanBePassedByReference(((BoundImplicitIndexerAccess)expr).IndexerOrSliceAccess);
		case BoundKind.ImplicitIndexerReceiverPlaceholder:
			return true;
		case BoundKind.InlineArrayAccess:
		{
			BoundInlineArrayAccess boundInlineArrayAccess = (BoundInlineArrayAccess)expr;
			if (boundInlineArrayAccess != null && !boundInlineArrayAccess.IsValue)
			{
				WellKnownMember getItemOrSliceHelper = boundInlineArrayAccess.GetItemOrSliceHelper;
				if (getItemOrSliceHelper == WellKnownMember.System_Span_T__get_Item || getItemOrSliceHelper == WellKnownMember.System_ReadOnlySpan_T__get_Item)
				{
					return true;
				}
			}
			return false;
		}
		case BoundKind.ImplicitIndexerValuePlaceholder:
			return false;
		case BoundKind.ListPatternReceiverPlaceholder:
		case BoundKind.ListPatternIndexPlaceholder:
		case BoundKind.SlicePatternReceiverPlaceholder:
		case BoundKind.SlicePatternRangePlaceholder:
			throw ExceptionUtilities.UnexpectedValue(expr.Kind);
		case BoundKind.Conversion:
			if (expr is BoundConversion { Conversion: { IsInterpolatedStringHandler: not false } } boundConversion)
			{
				TypeSymbol type = boundConversion.Type;
				if ((object)type != null)
				{
					return type.IsValueType;
				}
			}
			return false;
		default:
			return false;
		}
	}

	private void CheckRefReadOnlySymbols(MethodSymbol symbol)
	{
		if (symbol.ReturnsByRefReadonly || symbol.Parameters.Any((ParameterSymbol p) => p.RefKind == RefKind.In))
		{
			_factory.CompilationState.ModuleBuilderOpt?.EnsureIsReadOnlyAttributeExists();
		}
	}

	private CompoundUseSiteInfo<AssemblySymbol> GetNewCompoundUseSiteInfo()
	{
		return new CompoundUseSiteInfo<AssemblySymbol>(_diagnostics, _compilation.Assembly);
	}

	private BoundExpression ConvertReceiverForExtensionMemberIfNeeded(Symbol member, BoundExpression receiver, bool markAsChecked)
	{
		if (member.IsExtensionBlockMember())
		{
			ParameterSymbol extensionParameter = member.ContainingType.ExtensionParameter;
			return MakeConversionNode(receiver, extensionParameter.Type, @checked: false, acceptFailingConversion: false, markAsChecked);
		}
		return receiver;
	}

	public override BoundNode VisitAnonymousObjectCreationExpression(BoundAnonymousObjectCreationExpression node)
	{
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		return new BoundObjectCreationExpression(node.Syntax, node.Constructor, arguments, default(ImmutableArray<string>), default(ImmutableArray<RefKind>), expanded: false, default(ImmutableArray<int>), default(BitVector), null, null, node.Type);
	}

	public override BoundNode VisitAsOperator(BoundAsOperator node)
	{
		BoundExpression rewrittenOperand = VisitExpression(node.Operand);
		BoundTypeExpression rewrittenTargetType = (BoundTypeExpression)VisitTypeExpression(node.TargetType);
		TypeSymbol rewrittenType = VisitType(node.Type);
		return MakeAsOperator(node, node.Syntax, rewrittenOperand, rewrittenTargetType, node.OperandPlaceholder, node.OperandConversion, rewrittenType);
	}

	public override BoundNode VisitTypeExpression(BoundTypeExpression node)
	{
		return base.VisitTypeExpression(node);
	}

	private BoundExpression MakeAsOperator(BoundAsOperator oldNode, SyntaxNode syntax, BoundExpression rewrittenOperand, BoundTypeExpression rewrittenTargetType, BoundValuePlaceholder? operandPlaceholder, BoundExpression? operandConversion, TypeSymbol rewrittenType)
	{
		if (!_inExpressionLambda)
		{
			Conversion conversion = BoundNode.GetConversion(operandConversion, operandPlaceholder);
			ConstantValue asOperatorConstantResult = Binder.GetAsOperatorConstantResult(rewrittenOperand.Type, rewrittenType, conversion.Kind, rewrittenOperand.ConstantValueOpt);
			if (asOperatorConstantResult != null)
			{
				if (asOperatorConstantResult.IsBad)
				{
					throw ExceptionUtilities.UnexpectedValue(asOperatorConstantResult);
				}
				BoundExpression boundExpression = (rewrittenType.IsNullableType() ? new BoundDefaultExpression(syntax, rewrittenType) : MakeLiteral(syntax, asOperatorConstantResult, rewrittenType));
				if (rewrittenOperand.ConstantValueOpt != null)
				{
					return boundExpression;
				}
				return new BoundSequence(syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(rewrittenOperand), boundExpression, rewrittenType);
			}
			if (conversion.IsImplicit)
			{
				AddPlaceholderReplacement(operandPlaceholder, rewrittenOperand);
				BoundExpression? result = VisitExpression(operandConversion);
				RemovePlaceholderReplacement(operandPlaceholder);
				return result;
			}
		}
		return oldNode.Update(rewrittenOperand, rewrittenTargetType, null, null, rewrittenType);
	}

	public override BoundNode VisitAssignmentOperator(BoundAssignmentOperator node)
	{
		return VisitAssignmentOperator(node, used: true);
	}

	private BoundExpression VisitAssignmentOperator(BoundAssignmentOperator node, bool used)
	{
		BoundExpression boundExpression = VisitExpression(node.Right);
		BoundExpression left = node.Left;
		BoundExpression rewrittenLeft;
		switch (left.Kind)
		{
		case BoundKind.PropertyAccess:
			rewrittenLeft = VisitPropertyAccess((BoundPropertyAccess)left, isLeftOfAssignment: true);
			break;
		case BoundKind.IndexerAccess:
			rewrittenLeft = VisitIndexerAccess((BoundIndexerAccess)left, isLeftOfAssignment: true);
			break;
		case BoundKind.ImplicitIndexerAccess:
			rewrittenLeft = VisitImplicitIndexerAccess((BoundImplicitIndexerAccess)left, isLeftOfAssignment: true);
			break;
		case BoundKind.EventAccess:
		{
			BoundEventAccess boundEventAccess = (BoundEventAccess)left;
			if (boundEventAccess.EventSymbol.IsWindowsRuntimeEvent)
			{
				return VisitWindowsRuntimeEventFieldAssignmentOperator(node.Syntax, boundEventAccess, boundExpression);
			}
			goto default;
		}
		case BoundKind.DynamicMemberAccess:
		{
			BoundDynamicMemberAccess boundDynamicMemberAccess = (BoundDynamicMemberAccess)left;
			BoundExpression loweredReceiver2 = VisitExpression(boundDynamicMemberAccess.Receiver);
			return _dynamicFactory.MakeDynamicSetMember(loweredReceiver2, boundDynamicMemberAccess.Name, boundExpression).ToExpression();
		}
		case BoundKind.DynamicIndexerAccess:
		{
			BoundDynamicIndexerAccess boundDynamicIndexerAccess = (BoundDynamicIndexerAccess)left;
			BoundExpression loweredReceiver = VisitExpression(boundDynamicIndexerAccess.Receiver);
			ImmutableArray<BoundExpression> loweredArguments = VisitList(boundDynamicIndexerAccess.Arguments);
			return MakeDynamicSetIndex(boundDynamicIndexerAccess, loweredReceiver, loweredArguments, boundDynamicIndexerAccess.ArgumentNamesOpt, boundDynamicIndexerAccess.ArgumentRefKindsOpt, boundExpression);
		}
		default:
			rewrittenLeft = VisitExpression(left);
			break;
		}
		return MakeStaticAssignmentOperator(node.Syntax, rewrittenLeft, boundExpression, node.IsRef, used, AssignmentKind.SimpleAssignment);
	}

	private BoundExpression MakeAssignmentOperator(SyntaxNode syntax, BoundExpression rewrittenLeft, BoundExpression rewrittenRight, bool used, bool isChecked, AssignmentKind assignmentKind)
	{
		switch (rewrittenLeft.Kind)
		{
		case BoundKind.DynamicIndexerAccess:
		{
			BoundDynamicIndexerAccess boundDynamicIndexerAccess = (BoundDynamicIndexerAccess)rewrittenLeft;
			return MakeDynamicSetIndex(boundDynamicIndexerAccess, boundDynamicIndexerAccess.Receiver, boundDynamicIndexerAccess.Arguments, boundDynamicIndexerAccess.ArgumentNamesOpt, boundDynamicIndexerAccess.ArgumentRefKindsOpt, rewrittenRight, assignmentKind == AssignmentKind.CompoundAssignment, isChecked);
		}
		case BoundKind.DynamicMemberAccess:
		{
			BoundDynamicMemberAccess boundDynamicMemberAccess = (BoundDynamicMemberAccess)rewrittenLeft;
			return _dynamicFactory.MakeDynamicSetMember(boundDynamicMemberAccess.Receiver, boundDynamicMemberAccess.Name, rewrittenRight, assignmentKind == AssignmentKind.CompoundAssignment, isChecked).ToExpression();
		}
		case BoundKind.EventAccess:
		{
			BoundEventAccess boundEventAccess = (BoundEventAccess)rewrittenLeft;
			if (boundEventAccess.EventSymbol.IsWindowsRuntimeEvent)
			{
				return RewriteWindowsRuntimeEventAssignmentOperator(boundEventAccess.Syntax, boundEventAccess.EventSymbol, EventAssignmentKind.Assignment, boundEventAccess.ReceiverOpt, rewrittenRight);
			}
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_AssignmentOperator.cs", 141);
		}
		default:
			return MakeStaticAssignmentOperator(syntax, rewrittenLeft, rewrittenRight, isRef: false, used, assignmentKind);
		}
	}

	private BoundExpression MakeDynamicSetIndex(BoundDynamicIndexerAccess indexerAccess, BoundExpression loweredReceiver, ImmutableArray<BoundExpression> loweredArguments, ImmutableArray<string?> argumentNames, ImmutableArray<RefKind> refKinds, BoundExpression loweredRight, bool isCompoundAssignment = false, bool isChecked = false)
	{
		EmbedIfNeedTo(loweredReceiver, indexerAccess.ApplicableIndexers, indexerAccess.Syntax);
		return _dynamicFactory.MakeDynamicSetIndex(MakeDynamicIndexerAccessReceiver(indexerAccess, loweredReceiver), loweredArguments, argumentNames, refKinds, loweredRight, isCompoundAssignment, isChecked).ToExpression();
	}

	private BoundExpression MakeStaticAssignmentOperator(SyntaxNode syntax, BoundExpression rewrittenLeft, BoundExpression rewrittenRight, bool isRef, bool used, AssignmentKind assignmentKind)
	{
		switch (rewrittenLeft.Kind)
		{
		case BoundKind.DynamicMemberAccess:
		case BoundKind.DynamicIndexerAccess:
			throw ExceptionUtilities.UnexpectedValue(rewrittenLeft.Kind);
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess obj = (BoundPropertyAccess)rewrittenLeft;
			BoundExpression receiverOpt2 = obj.ReceiverOpt;
			PropertySymbol propertySymbol = obj.PropertySymbol;
			return MakePropertyAssignment(syntax, receiverOpt2, propertySymbol, ImmutableArray<BoundExpression>.Empty, default(ImmutableArray<RefKind>), expanded: false, default(ImmutableArray<int>), rewrittenRight, used, assignmentKind);
		}
		case BoundKind.IndexerAccess:
		{
			BoundIndexerAccess boundIndexerAccess = (BoundIndexerAccess)rewrittenLeft;
			BoundExpression receiverOpt = boundIndexerAccess.ReceiverOpt;
			ImmutableArray<BoundExpression> arguments = boundIndexerAccess.Arguments;
			PropertySymbol indexer = boundIndexerAccess.Indexer;
			return MakePropertyAssignment(syntax, receiverOpt, indexer, arguments, boundIndexerAccess.ArgumentRefKindsOpt, boundIndexerAccess.Expanded, boundIndexerAccess.ArgsToParamsOpt, rewrittenRight, used, assignmentKind);
		}
		case BoundKind.Local:
		case BoundKind.Parameter:
		case BoundKind.FieldAccess:
			return _factory.AssignmentExpression(syntax, rewrittenLeft, rewrittenRight, isRef);
		case BoundKind.DiscardExpression:
			return rewrittenRight;
		case BoundKind.Sequence:
		{
			BoundSequence boundSequence = (BoundSequence)rewrittenLeft;
			if (boundSequence.Value.Kind == BoundKind.IndexerAccess)
			{
				return boundSequence.Update(boundSequence.Locals, boundSequence.SideEffects, MakeStaticAssignmentOperator(syntax, boundSequence.Value, rewrittenRight, isRef, used, assignmentKind), boundSequence.Type);
			}
			break;
		}
		}
		return _factory.AssignmentExpression(syntax, rewrittenLeft, rewrittenRight);
	}

	private bool IsExtensionPropertyWithByValPossiblyStructReceiverWhichHasHomeAndCanChangeValueBetweenReads(BoundExpression rewrittenReceiver, PropertySymbol property)
	{
		if (CanChangeValueBetweenReads(rewrittenReceiver, localsMayBeAssignedOrCaptured: true, structThisCanChangeValueBetweenReads: true) && IsExtensionBlockMemberWithByValPossiblyStructReceiver(property))
		{
			return CodeGenerator.HasHome(rewrittenReceiver, CodeGenerator.AddressKind.ReadOnlyStrict, _factory.CurrentFunction, peVerifyCompatEnabled: false, null);
		}
		return false;
	}

	private BoundExpression MakePropertyAssignment(SyntaxNode syntax, BoundExpression? rewrittenReceiver, PropertySymbol property, ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> argumentRefKindsOpt, bool expanded, ImmutableArray<int> argsToParamsOpt, BoundExpression rewrittenRight, bool used, AssignmentKind assignmentKind)
	{
		MethodSymbol ownOrInheritedSetMethod = property.GetOwnOrInheritedSetMethod();
		if ((object)ownOrInheritedSetMethod == null)
		{
			SynthesizedBackingFieldSymbol backingField = ((SourcePropertySymbolBase)property.OriginalDefinition).BackingField;
			return _factory.AssignmentExpression(_factory.Field(rewrittenReceiver, backingField), rewrittenRight);
		}
		ArrayBuilder<LocalSymbol> tempsOpt = null;
		bool flag = false;
		ArrayBuilder<BoundExpression> arrayBuilder = null;
		bool flag2 = rewrittenReceiver != null;
		if (flag2)
		{
			bool flag3 = (uint)(assignmentKind - 1) <= 3u;
			flag2 = !flag3;
		}
		if (flag2 && IsExtensionPropertyWithByValPossiblyStructReceiverWhichHasHomeAndCanChangeValueBetweenReads(rewrittenReceiver, property) && (arguments.Length != 0 || !IsSafeForReordering(rewrittenRight, RefKind.None)))
		{
			flag = true;
			arrayBuilder = ArrayBuilder<BoundExpression>.GetInstance();
		}
		arguments = VisitArgumentsAndCaptureReceiverIfNeeded(ref rewrittenReceiver, flag, arguments, property, argsToParamsOpt, argumentRefKindsOpt, arrayBuilder, ref tempsOpt);
		if (flag)
		{
			arguments = ExtractSideEffectsFromArguments(arguments, property, expanded, argsToParamsOpt, ref argumentRefKindsOpt, arrayBuilder, tempsOpt);
			if (!IsSafeForReordering(rewrittenRight, RefKind.None))
			{
				BoundLocal boundLocal = _factory.StoreToTemp(rewrittenRight, out BoundAssignmentOperator store);
				tempsOpt.Add(boundLocal.LocalSymbol);
				arrayBuilder.Add(store);
				rewrittenRight = boundLocal;
			}
		}
		else
		{
			arguments = MakeArguments(arguments, property, expanded, argsToParamsOpt, ref argumentRefKindsOpt, ref tempsOpt);
		}
		ImmutableArray<BoundExpression> sideEffects = arrayBuilder?.ToImmutableAndFree() ?? ImmutableArray<BoundExpression>.Empty;
		ImmutableArray<LocalSymbol> immutableArray = tempsOpt.ToImmutableAndFree();
		if (used)
		{
			TypeSymbol type = rewrittenRight.Type;
			LocalSymbol localSymbol = _factory.SynthesizedLocal(type);
			BoundExpression boundExpression = new BoundLocal(syntax, localSymbol, null, type);
			BoundExpression newElement = new BoundAssignmentOperator(syntax, boundExpression, rewrittenRight, type);
			BoundExpression item = BoundCall.Synthesized(syntax, rewrittenReceiver, ThreeState.Unknown, ownOrInheritedSetMethod, AppendToPossibleNull(arguments, newElement));
			return new BoundSequence(syntax, AppendToPossibleNull(immutableArray, localSymbol), sideEffects.Add(item), boundExpression, localSymbol.Type);
		}
		BoundCall boundCall = BoundCall.Synthesized(syntax, rewrittenReceiver, ThreeState.Unknown, ownOrInheritedSetMethod, AppendToPossibleNull(arguments, rewrittenRight));
		if (immutableArray.IsDefaultOrEmpty)
		{
			return boundCall;
		}
		return new BoundSequence(syntax, immutableArray, sideEffects, boundCall, ownOrInheritedSetMethod.ReturnType);
	}

	private static ImmutableArray<T> AppendToPossibleNull<T>(ImmutableArray<T> possibleNull, T newElement) where T : notnull
	{
		return possibleNull.NullToEmpty().Add(newElement);
	}

	public override BoundNode VisitAwaitExpression(BoundAwaitExpression node)
	{
		return VisitAwaitExpression(node, used: true);
	}

	public BoundExpression VisitAwaitExpression(BoundAwaitExpression node, bool used)
	{
		return RewriteAwaitExpression((BoundExpression)base.VisitAwaitExpression(node), used);
	}

	private BoundExpression RewriteAwaitExpression(SyntaxNode syntax, BoundExpression rewrittenExpression, BoundAwaitableInfo awaitableInfo, TypeSymbol type, BoundAwaitExpressionDebugInfo debugInfo, bool used)
	{
		return RewriteAwaitExpression(new BoundAwaitExpression(syntax, rewrittenExpression, awaitableInfo, debugInfo, type)
		{
			WasCompilerGenerated = true
		}, used);
	}

	private BoundExpression RewriteAwaitExpression(BoundExpression rewrittenAwait, bool used)
	{
		_sawAwait = true;
		if (!used)
		{
			return rewrittenAwait;
		}
		_needsSpilling = true;
		BoundLocal boundLocal = _factory.StoreToTemp(rewrittenAwait, out BoundAssignmentOperator store, RefKind.None, SynthesizedLocalKind.Spill, isKnownToReferToTempIfReferenceType: false, rewrittenAwait.Syntax);
		return new BoundSpillSequence(rewrittenAwait.Syntax, ImmutableArray.Create(boundLocal.LocalSymbol), ImmutableArray.Create((BoundExpression)store), boundLocal, boundLocal.Type);
	}

	public override BoundNode VisitBinaryOperator(BoundBinaryOperator node)
	{
		return VisitBinaryOperator(node, null);
	}

	public override BoundNode VisitUserDefinedConditionalLogicalOperator(BoundUserDefinedConditionalLogicalOperator node)
	{
		SyntaxNode syntax = node.Syntax;
		BinaryOperatorKind operatorKind = node.OperatorKind;
		TypeSymbol type = node.Type;
		BoundExpression boundExpression = VisitExpression(node.Left);
		BoundExpression boundExpression2 = VisitExpression(node.Right);
		if (_inExpressionLambda)
		{
			return node.Update(operatorKind, node.LogicalOperator, node.TrueOperator, node.FalseOperator, null, null, node.ConstrainedToTypeOpt, node.ResultKind, default(ImmutableArray<MethodSymbol>), boundExpression, boundExpression2, type);
		}
		BoundLocal boundLocal = _factory.StoreToTemp(boundExpression, out BoundAssignmentOperator store);
		BoundCall rewrittenCondition = BoundCall.Synthesized(syntax, ((object)node.ConstrainedToTypeOpt == null) ? null : new BoundTypeExpression(syntax, null, node.ConstrainedToTypeOpt), ThreeState.Unknown, (operatorKind.Operator() == BinaryOperatorKind.And) ? node.FalseOperator : node.TrueOperator, ApplyConversionIfNotIdentity(node.TrueFalseOperandConversion, node.TrueFalseOperandPlaceholder, boundLocal));
		BoundExpression rewrittenAlternative = LowerUserDefinedBinaryOperator(syntax, operatorKind & ~BinaryOperatorKind.Logical, boundLocal, boundExpression2, type, node.LogicalOperator, node.ConstrainedToTypeOpt);
		BoundExpression value = RewriteConditionalOperator(syntax, rewrittenCondition, boundLocal, rewrittenAlternative, null, type, isRef: false);
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal.LocalSymbol), ImmutableArray.Create((BoundExpression)store), value, type);
	}

	public BoundExpression VisitBinaryOperator(BoundBinaryOperator node, BoundUnaryOperator? applyParentUnaryOperator)
	{
		InterpolatedStringHandlerData? interpolatedStringHandlerData = node.InterpolatedStringHandlerData;
		if (interpolatedStringHandlerData.HasValue)
		{
			InterpolatedStringHandlerData valueOrDefault = interpolatedStringHandlerData.GetValueOrDefault();
			ImmutableArray<BoundExpression> parts = CollectBinaryOperatorInterpolatedStringParts(node);
			return LowerPartsToString(valueOrDefault, parts, node.Syntax, node.Type);
		}
		if (node.OperatorKind == BinaryOperatorKind.Utf8Addition)
		{
			return VisitUtf8Addition(node);
		}
		if (IsBinaryStringConcatenation(node))
		{
			return VisitStringConcatenation(node);
		}
		ArrayBuilder<BoundBinaryOperator> instance = ArrayBuilder<BoundBinaryOperator>.GetInstance();
		BoundBinaryOperator boundBinaryOperator = node;
		while (boundBinaryOperator != null && boundBinaryOperator.ConstantValueOpt == null && !boundBinaryOperator.InterpolatedStringHandlerData.HasValue && boundBinaryOperator.OperatorKind != BinaryOperatorKind.Utf8Addition && !IsBinaryStringConcatenation(boundBinaryOperator))
		{
			instance.Push(boundBinaryOperator);
			boundBinaryOperator = boundBinaryOperator.Left as BoundBinaryOperator;
		}
		BoundExpression boundExpression = VisitExpression(instance.Peek().Left);
		while (instance.Count > 0)
		{
			BoundBinaryOperator boundBinaryOperator2 = instance.Pop();
			BoundExpression loweredRight = VisitExpression(boundBinaryOperator2.Right);
			boundExpression = MakeBinaryOperator(boundBinaryOperator2, boundBinaryOperator2.Syntax, boundBinaryOperator2.OperatorKind, boundExpression, loweredRight, boundBinaryOperator2.Type, boundBinaryOperator2.LeftTruthOperatorMethod ?? boundBinaryOperator2.BinaryOperatorMethod, boundBinaryOperator2.ConstrainedToType, isPointerElementAccess: false, isCompoundAssignment: false, (instance.Count == 0) ? applyParentUnaryOperator : null);
		}
		instance.Free();
		return boundExpression;
	}

	private static ImmutableArray<BoundExpression> CollectBinaryOperatorInterpolatedStringParts(BoundBinaryOperator node)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		node.VisitBinaryOperatorInterpolatedString(instance, delegate(BoundInterpolatedString interpolatedString, ArrayBuilder<BoundExpression> partsBuilder)
		{
			partsBuilder.AddRange(interpolatedString.Parts);
			return true;
		});
		return instance.ToImmutableAndFree();
	}

	private BoundExpression MakeBinaryOperator(SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol type, MethodSymbol? method, TypeSymbol? constrainedToTypeOpt, bool isPointerElementAccess = false, bool isCompoundAssignment = false, BoundUnaryOperator? applyParentUnaryOperator = null)
	{
		return MakeBinaryOperator(null, syntax, operatorKind, loweredLeft, loweredRight, type, method, constrainedToTypeOpt, isPointerElementAccess, isCompoundAssignment, applyParentUnaryOperator);
	}

	private BoundExpression MakeBinaryOperator(BoundBinaryOperator? oldNode, SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol type, MethodSymbol? method, TypeSymbol? constrainedToTypeOpt, bool isPointerElementAccess = false, bool isCompoundAssignment = false, BoundUnaryOperator? applyParentUnaryOperator = null)
	{
		if (_inExpressionLambda)
		{
			switch (operatorKind.Operator() | operatorKind.OperandTypes())
			{
			case BinaryOperatorKind.StringConcatenation:
			case BinaryOperatorKind.StringAndObjectConcatenation:
			case BinaryOperatorKind.ObjectAndStringConcatenation:
				throw ExceptionUtilities.UnexpectedValue(operatorKind);
			case BinaryOperatorKind.DelegateCombination:
				return RewriteDelegateOperation(syntax, operatorKind, loweredLeft, loweredRight, type, SpecialMember.System_Delegate__Combine);
			case BinaryOperatorKind.DelegateRemoval:
				return RewriteDelegateOperation(syntax, operatorKind, loweredLeft, loweredRight, type, SpecialMember.System_Delegate__Remove);
			case BinaryOperatorKind.DelegateEqual:
				return RewriteDelegateOperation(syntax, operatorKind, loweredLeft, loweredRight, type, SpecialMember.System_Delegate__op_Equality);
			case BinaryOperatorKind.DelegateNotEqual:
				return RewriteDelegateOperation(syntax, operatorKind, loweredLeft, loweredRight, type, SpecialMember.System_Delegate__op_Inequality);
			}
		}
		else
		{
			if (operatorKind.IsDynamic())
			{
				if (operatorKind.IsLogical())
				{
					return MakeDynamicLogicalBinaryOperator(syntax, operatorKind, loweredLeft, loweredRight, method, constrainedToTypeOpt, type, isCompoundAssignment, applyParentUnaryOperator);
				}
				return _dynamicFactory.MakeDynamicBinaryOperator(operatorKind, loweredLeft, loweredRight, isCompoundAssignment, type).ToExpression();
			}
			if (operatorKind.IsLifted())
			{
				return RewriteLiftedBinaryOperator(syntax, operatorKind, loweredLeft, loweredRight, type, method, constrainedToTypeOpt);
			}
			if (operatorKind.IsUserDefined())
			{
				return LowerUserDefinedBinaryOperator(syntax, operatorKind, loweredLeft, loweredRight, type, method, constrainedToTypeOpt);
			}
			switch (operatorKind.OperatorWithLogical() | operatorKind.OperandTypes())
			{
			case BinaryOperatorKind.NullableNullEqual:
			case BinaryOperatorKind.NullableNullNotEqual:
				return _factory.RewriteNullableNullEquality(syntax, operatorKind, loweredLeft, loweredRight, type);
			case BinaryOperatorKind.StringConcatenation:
			case BinaryOperatorKind.StringAndObjectConcatenation:
			case BinaryOperatorKind.ObjectAndStringConcatenation:
				throw ExceptionUtilities.UnexpectedValue(operatorKind);
			case BinaryOperatorKind.StringEqual:
				return RewriteStringEquality(oldNode, syntax, operatorKind, loweredLeft, loweredRight, type, SpecialMember.System_String__op_Equality);
			case BinaryOperatorKind.StringNotEqual:
				return RewriteStringEquality(oldNode, syntax, operatorKind, loweredLeft, loweredRight, type, SpecialMember.System_String__op_Inequality);
			case BinaryOperatorKind.DelegateCombination:
				return RewriteDelegateOperation(syntax, operatorKind, loweredLeft, loweredRight, type, SpecialMember.System_Delegate__Combine);
			case BinaryOperatorKind.DelegateRemoval:
				return RewriteDelegateOperation(syntax, operatorKind, loweredLeft, loweredRight, type, SpecialMember.System_Delegate__Remove);
			case BinaryOperatorKind.DelegateEqual:
				return RewriteDelegateOperation(syntax, operatorKind, loweredLeft, loweredRight, type, SpecialMember.System_Delegate__op_Equality);
			case BinaryOperatorKind.DelegateNotEqual:
				return RewriteDelegateOperation(syntax, operatorKind, loweredLeft, loweredRight, type, SpecialMember.System_Delegate__op_Inequality);
			case BinaryOperatorKind.LogicalBoolAnd:
				if (loweredRight.ConstantValueOpt == ConstantValue.True)
				{
					return loweredLeft;
				}
				if (loweredLeft.ConstantValueOpt == ConstantValue.True)
				{
					return loweredRight;
				}
				if (loweredLeft.ConstantValueOpt == ConstantValue.False)
				{
					return loweredLeft;
				}
				if (loweredRight.Kind == BoundKind.Local || loweredRight.Kind == BoundKind.Parameter)
				{
					operatorKind &= ~BinaryOperatorKind.Logical;
				}
				break;
			case BinaryOperatorKind.LogicalBoolOr:
				if (loweredRight.ConstantValueOpt == ConstantValue.False)
				{
					return loweredLeft;
				}
				if (loweredLeft.ConstantValueOpt == ConstantValue.False)
				{
					return loweredRight;
				}
				if (loweredLeft.ConstantValueOpt == ConstantValue.True)
				{
					return loweredLeft;
				}
				if (loweredRight.Kind == BoundKind.Local || loweredRight.Kind == BoundKind.Parameter)
				{
					operatorKind &= ~BinaryOperatorKind.Logical;
				}
				break;
			case BinaryOperatorKind.BoolAnd:
				if (loweredRight.ConstantValueOpt == ConstantValue.True)
				{
					return loweredLeft;
				}
				if (loweredLeft.ConstantValueOpt == ConstantValue.True)
				{
					return loweredRight;
				}
				if (loweredLeft.IsDefaultValue())
				{
					return _factory.MakeSequence(loweredRight, loweredLeft);
				}
				if (loweredRight.IsDefaultValue())
				{
					return _factory.MakeSequence(loweredLeft, loweredRight);
				}
				break;
			case BinaryOperatorKind.BoolOr:
				if (loweredRight.ConstantValueOpt == ConstantValue.False)
				{
					return loweredLeft;
				}
				if (loweredLeft.ConstantValueOpt == ConstantValue.False)
				{
					return loweredRight;
				}
				break;
			case BinaryOperatorKind.BoolEqual:
				if (loweredLeft.ConstantValueOpt == ConstantValue.True)
				{
					return loweredRight;
				}
				if (loweredRight.ConstantValueOpt == ConstantValue.True)
				{
					return loweredLeft;
				}
				if (loweredLeft.ConstantValueOpt == ConstantValue.False)
				{
					return MakeUnaryOperator(UnaryOperatorKind.BoolLogicalNegation, syntax, null, null, loweredRight, loweredRight.Type);
				}
				if (loweredRight.ConstantValueOpt == ConstantValue.False)
				{
					return MakeUnaryOperator(UnaryOperatorKind.BoolLogicalNegation, syntax, null, null, loweredLeft, loweredLeft.Type);
				}
				break;
			case BinaryOperatorKind.BoolNotEqual:
				if (loweredLeft.ConstantValueOpt == ConstantValue.False)
				{
					return loweredRight;
				}
				if (loweredRight.ConstantValueOpt == ConstantValue.False)
				{
					return loweredLeft;
				}
				if (loweredLeft.ConstantValueOpt == ConstantValue.True)
				{
					return MakeUnaryOperator(UnaryOperatorKind.BoolLogicalNegation, syntax, null, null, loweredRight, loweredRight.Type);
				}
				if (loweredRight.ConstantValueOpt == ConstantValue.True)
				{
					return MakeUnaryOperator(UnaryOperatorKind.BoolLogicalNegation, syntax, null, null, loweredLeft, loweredLeft.Type);
				}
				break;
			case BinaryOperatorKind.BoolXor:
				if (loweredLeft.ConstantValueOpt == ConstantValue.False)
				{
					return loweredRight;
				}
				if (loweredRight.ConstantValueOpt == ConstantValue.False)
				{
					return loweredLeft;
				}
				if (loweredLeft.ConstantValueOpt == ConstantValue.True)
				{
					return MakeUnaryOperator(UnaryOperatorKind.BoolLogicalNegation, syntax, null, null, loweredRight, loweredRight.Type);
				}
				if (loweredRight.ConstantValueOpt == ConstantValue.True)
				{
					return MakeUnaryOperator(UnaryOperatorKind.BoolLogicalNegation, syntax, null, null, loweredLeft, loweredLeft.Type);
				}
				break;
			case BinaryOperatorKind.IntLeftShift:
			case BinaryOperatorKind.UIntLeftShift:
			case BinaryOperatorKind.IntRightShift:
			case BinaryOperatorKind.UIntRightShift:
			case BinaryOperatorKind.IntUnsignedRightShift:
			case BinaryOperatorKind.UIntUnsignedRightShift:
				return RewriteBuiltInShiftOperation(oldNode, syntax, operatorKind, loweredLeft, loweredRight, type, 31);
			case BinaryOperatorKind.LongLeftShift:
			case BinaryOperatorKind.ULongLeftShift:
			case BinaryOperatorKind.LongRightShift:
			case BinaryOperatorKind.ULongRightShift:
			case BinaryOperatorKind.LongUnsignedRightShift:
			case BinaryOperatorKind.ULongUnsignedRightShift:
				return RewriteBuiltInShiftOperation(oldNode, syntax, operatorKind, loweredLeft, loweredRight, type, 63);
			case BinaryOperatorKind.NIntLeftShift:
			case BinaryOperatorKind.NUIntLeftShift:
			case BinaryOperatorKind.NIntRightShift:
			case BinaryOperatorKind.NUIntRightShift:
			case BinaryOperatorKind.NIntUnsignedRightShift:
			case BinaryOperatorKind.NUIntUnsignedRightShift:
				return RewriteBuiltInNativeShiftOperation(oldNode, syntax, operatorKind, loweredLeft, loweredRight, type);
			case BinaryOperatorKind.DecimalMultiplication:
			case BinaryOperatorKind.DecimalAddition:
			case BinaryOperatorKind.DecimalSubtraction:
			case BinaryOperatorKind.DecimalDivision:
			case BinaryOperatorKind.DecimalRemainder:
			case BinaryOperatorKind.DecimalEqual:
			case BinaryOperatorKind.DecimalNotEqual:
			case BinaryOperatorKind.DecimalGreaterThan:
			case BinaryOperatorKind.DecimalLessThan:
			case BinaryOperatorKind.DecimalGreaterThanOrEqual:
			case BinaryOperatorKind.DecimalLessThanOrEqual:
				return RewriteDecimalBinaryOperation(syntax, loweredLeft, loweredRight, operatorKind);
			case BinaryOperatorKind.PointerAndIntAddition:
			case BinaryOperatorKind.PointerAndUIntAddition:
			case BinaryOperatorKind.PointerAndLongAddition:
			case BinaryOperatorKind.PointerAndULongAddition:
			case BinaryOperatorKind.PointerAndIntSubtraction:
			case BinaryOperatorKind.PointerAndUIntSubtraction:
			case BinaryOperatorKind.PointerAndLongSubtraction:
			case BinaryOperatorKind.PointerAndULongSubtraction:
				if (loweredRight.IsDefaultValue())
				{
					return loweredLeft;
				}
				return RewritePointerNumericOperator(syntax, operatorKind, loweredLeft, loweredRight, type, isPointerElementAccess, isLeftPointer: true);
			case BinaryOperatorKind.IntAndPointerAddition:
			case BinaryOperatorKind.UIntAndPointerAddition:
			case BinaryOperatorKind.LongAndPointerAddition:
			case BinaryOperatorKind.ULongAndPointerAddition:
				if (loweredLeft.IsDefaultValue())
				{
					return loweredRight;
				}
				return RewritePointerNumericOperator(syntax, operatorKind, loweredLeft, loweredRight, type, isPointerElementAccess, isLeftPointer: false);
			case BinaryOperatorKind.PointerSubtraction:
				return RewritePointerSubtraction(operatorKind, loweredLeft, loweredRight, type);
			case BinaryOperatorKind.IntAddition:
			case BinaryOperatorKind.UIntAddition:
			case BinaryOperatorKind.LongAddition:
			case BinaryOperatorKind.ULongAddition:
				if (loweredLeft.IsDefaultValue())
				{
					return loweredRight;
				}
				if (loweredRight.IsDefaultValue())
				{
					return loweredLeft;
				}
				break;
			case BinaryOperatorKind.IntSubtraction:
			case BinaryOperatorKind.UIntSubtraction:
			case BinaryOperatorKind.LongSubtraction:
			case BinaryOperatorKind.ULongSubtraction:
				if (loweredRight.IsDefaultValue())
				{
					return loweredLeft;
				}
				break;
			case BinaryOperatorKind.IntMultiplication:
			case BinaryOperatorKind.UIntMultiplication:
			case BinaryOperatorKind.LongMultiplication:
			case BinaryOperatorKind.ULongMultiplication:
			{
				if (loweredLeft.IsDefaultValue())
				{
					return _factory.MakeSequence(loweredRight, loweredLeft);
				}
				if (loweredRight.IsDefaultValue())
				{
					return _factory.MakeSequence(loweredLeft, loweredRight);
				}
				ConstantValue? constantValueOpt = loweredLeft.ConstantValueOpt;
				if ((object)constantValueOpt != null && constantValueOpt.UInt64Value == 1)
				{
					return loweredRight;
				}
				ConstantValue? constantValueOpt2 = loweredRight.ConstantValueOpt;
				if ((object)constantValueOpt2 != null && constantValueOpt2.UInt64Value == 1)
				{
					return loweredLeft;
				}
				break;
			}
			case BinaryOperatorKind.IntGreaterThan:
			case BinaryOperatorKind.IntLessThanOrEqual:
				if (loweredLeft.Kind == BoundKind.ArrayLength && loweredRight.IsDefaultValue())
				{
					BinaryOperatorKind binaryOperatorKind2 = ((operatorKind == BinaryOperatorKind.IntGreaterThan) ? BinaryOperatorKind.NotEqual : BinaryOperatorKind.Equal);
					operatorKind &= ~BinaryOperatorKind.OpMask;
					operatorKind |= binaryOperatorKind2;
					loweredLeft = UnconvertArrayLength((BoundArrayLength)loweredLeft);
				}
				break;
			case BinaryOperatorKind.IntLessThan:
			case BinaryOperatorKind.IntGreaterThanOrEqual:
				if (loweredRight.Kind == BoundKind.ArrayLength && loweredLeft.IsDefaultValue())
				{
					BinaryOperatorKind binaryOperatorKind = ((operatorKind == BinaryOperatorKind.IntLessThan) ? BinaryOperatorKind.NotEqual : BinaryOperatorKind.Equal);
					operatorKind &= ~BinaryOperatorKind.OpMask;
					operatorKind |= binaryOperatorKind;
					loweredRight = UnconvertArrayLength((BoundArrayLength)loweredRight);
				}
				break;
			case BinaryOperatorKind.IntEqual:
			case BinaryOperatorKind.IntNotEqual:
				if (loweredLeft.Kind == BoundKind.ArrayLength && loweredRight.IsDefaultValue())
				{
					loweredLeft = UnconvertArrayLength((BoundArrayLength)loweredLeft);
				}
				else if (loweredRight.Kind == BoundKind.ArrayLength && loweredLeft.IsDefaultValue())
				{
					loweredRight = UnconvertArrayLength((BoundArrayLength)loweredRight);
				}
				break;
			case BinaryOperatorKind.Utf8Addition:
				throw ExceptionUtilities.UnexpectedValue(operatorKind);
			}
		}
		if (oldNode == null)
		{
			return new BoundBinaryOperator(syntax, operatorKind, null, null, null, LookupResultKind.Viable, loweredLeft, loweredRight, type);
		}
		return oldNode.Update(operatorKind, oldNode.ConstantValueOpt, oldNode.BinaryOperatorMethod, oldNode.ConstrainedToType, oldNode.ResultKind, loweredLeft, loweredRight, type);
	}

	private BoundExpression RewriteLiftedBinaryOperator(SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol type, MethodSymbol? method, TypeSymbol? constrainedToTypeOpt)
	{
		BoundLoweredConditionalAccess boundLoweredConditionalAccess = loweredLeft as BoundLoweredConditionalAccess;
		int num;
		if (boundLoweredConditionalAccess != null && operatorKind != BinaryOperatorKind.LiftedBoolOr && operatorKind != BinaryOperatorKind.LiftedBoolAnd && !ReadIsSideeffecting(loweredRight))
		{
			if (boundLoweredConditionalAccess.WhenNullOpt != null)
			{
				num = (boundLoweredConditionalAccess.WhenNullOpt.IsDefaultValue() ? 1 : 0);
				if (num == 0)
				{
					goto IL_0047;
				}
			}
			else
			{
				num = 1;
			}
			loweredLeft = boundLoweredConditionalAccess.WhenNotNull;
		}
		else
		{
			num = 0;
		}
		goto IL_0047;
		IL_0047:
		BoundExpression boundExpression = ((!operatorKind.IsComparison()) ? LowerLiftedBinaryArithmeticOperator(syntax, operatorKind, loweredLeft, loweredRight, type, method, constrainedToTypeOpt) : (operatorKind.IsUserDefined() ? LowerLiftedUserDefinedComparisonOperator(syntax, operatorKind, loweredLeft, loweredRight, method, constrainedToTypeOpt) : LowerLiftedBuiltInComparisonOperator(syntax, operatorKind, loweredLeft, loweredRight)));
		if (num != 0)
		{
			BoundExpression whenNullOpt = null;
			if (operatorKind.Operator() == BinaryOperatorKind.NotEqual || operatorKind.Operator() == BinaryOperatorKind.Equal)
			{
				whenNullOpt = RewriteLiftedBinaryOperator(syntax, operatorKind, _factory.Default(loweredLeft.Type), loweredRight, type, method, constrainedToTypeOpt);
			}
			boundExpression = boundLoweredConditionalAccess.Update(boundLoweredConditionalAccess.Receiver, boundLoweredConditionalAccess.HasValueMethodOpt, boundExpression, whenNullOpt, boundLoweredConditionalAccess.Id, boundLoweredConditionalAccess.ForceCopyOfNullableValueType, boundExpression.Type);
		}
		return boundExpression;
	}

	private BoundExpression UnconvertArrayLength(BoundArrayLength arrLength)
	{
		return arrLength.Update(arrLength.Expression, _factory.SpecialType(SpecialType.System_UIntPtr));
	}

	private BoundExpression MakeDynamicLogicalBinaryOperator(SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression loweredLeft, BoundExpression loweredRight, MethodSymbol? leftTruthOperator, TypeSymbol? constrainedToTypeOpt, TypeSymbol type, bool isCompoundAssignment, BoundUnaryOperator? applyParentUnaryOperator)
	{
		bool flag = operatorKind.Operator() == BinaryOperatorKind.And;
		UnaryOperatorKind unaryOperatorKind = (flag ? UnaryOperatorKind.DynamicFalse : UnaryOperatorKind.DynamicTrue);
		ConstantValue constantValue = loweredLeft.ConstantValueOpt ?? UnboxConstant(loweredLeft);
		if ((unaryOperatorKind == UnaryOperatorKind.DynamicFalse && constantValue == ConstantValue.False) || (unaryOperatorKind == UnaryOperatorKind.DynamicTrue && constantValue == ConstantValue.True))
		{
			if (applyParentUnaryOperator != null)
			{
				return _factory.Literal(value: true);
			}
			return MakeConversionNode(loweredLeft, type, @checked: false);
		}
		NamedTypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
		BoundAssignmentOperator boundAssignmentOperator;
		BoundLocal boundLocal;
		if (constantValue == null && loweredLeft.Kind != BoundKind.Local && loweredLeft.Kind != BoundKind.Parameter)
		{
			BoundExpression boundExpression = (loweredLeft = _factory.StoreToTemp(loweredLeft, out BoundAssignmentOperator store));
			boundAssignmentOperator = store;
			boundLocal = (BoundLocal)boundExpression;
		}
		else
		{
			boundAssignmentOperator = null;
			boundLocal = null;
		}
		BoundExpression boundExpression2 = _dynamicFactory.MakeDynamicBinaryOperator(operatorKind, loweredLeft, loweredRight, isCompoundAssignment, type).ToExpression();
		bool flag2 = (unaryOperatorKind == UnaryOperatorKind.DynamicFalse && constantValue == ConstantValue.True) || (unaryOperatorKind == UnaryOperatorKind.DynamicTrue && constantValue == ConstantValue.False);
		BoundExpression boundExpression3;
		if (applyParentUnaryOperator != null)
		{
			boundExpression3 = _dynamicFactory.MakeDynamicUnaryOperator(unaryOperatorKind, boundExpression2, specialType).ToExpression();
			if (!flag2)
			{
				BoundExpression left = MakeTruthTestForDynamicLogicalOperator(syntax, operatorKind, loweredLeft, specialType, leftTruthOperator, constrainedToTypeOpt, flag);
				boundExpression3 = _factory.Binary(BinaryOperatorKind.LogicalOr, specialType, left, boundExpression3);
			}
		}
		else if (flag2)
		{
			boundExpression3 = boundExpression2;
		}
		else
		{
			BoundExpression condition = MakeTruthTestForDynamicLogicalOperator(syntax, operatorKind, loweredLeft, specialType, leftTruthOperator, constrainedToTypeOpt, flag);
			BoundExpression consequence = MakeConversionNode(loweredLeft, type, @checked: false);
			boundExpression3 = _factory.Conditional(condition, consequence, boundExpression2, type);
		}
		if (boundAssignmentOperator != null)
		{
			return _factory.Sequence(ImmutableArray.Create(boundLocal.LocalSymbol), ImmutableArray.Create((BoundExpression)boundAssignmentOperator), boundExpression3);
		}
		return boundExpression3;
	}

	private static ConstantValue? UnboxConstant(BoundExpression expression)
	{
		if (expression.Kind == BoundKind.Conversion)
		{
			BoundConversion boundConversion = (BoundConversion)expression;
			if (boundConversion.ConversionKind == ConversionKind.Boxing)
			{
				return boundConversion.Operand.ConstantValueOpt;
			}
		}
		return null;
	}

	private BoundExpression MakeTruthTestForDynamicLogicalOperator(SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression loweredLeft, TypeSymbol boolean, MethodSymbol? leftTruthOperator, TypeSymbol? constrainedToTypeOpt, bool negative)
	{
		if (loweredLeft.HasDynamicType())
		{
			return _dynamicFactory.MakeDynamicUnaryOperator(negative ? UnaryOperatorKind.DynamicFalse : UnaryOperatorKind.DynamicTrue, loweredLeft, boolean).ToExpression();
		}
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = GetNewCompoundUseSiteInfo();
		if (_compilation.Conversions.ClassifyConversionFromExpression(loweredLeft, boolean, isChecked: false, ref useSiteInfo).IsImplicit)
		{
			_diagnostics.Add(loweredLeft.Syntax, useSiteInfo);
			BoundExpression boundExpression = MakeConversionNode(loweredLeft, boolean, @checked: false, acceptFailingConversion: false, markAsChecked: true);
			if (negative)
			{
				return new BoundUnaryOperator(syntax, UnaryOperatorKind.BoolLogicalNegation, boundExpression, null, null, null, LookupResultKind.Viable, boolean)
				{
					WasCompilerGenerated = true
				};
			}
			return boundExpression;
		}
		TypeSymbol type = leftTruthOperator.Parameters[0].Type;
		Conversion conversion = _compilation.Conversions.ClassifyConversionFromType(loweredLeft.Type, type, operatorKind.IsChecked(), ref useSiteInfo);
		loweredLeft = MakeConversionNode(loweredLeft, type, operatorKind.IsChecked(), acceptFailingConversion: false, markAsChecked: true);
		_diagnostics.Add(loweredLeft.Syntax, useSiteInfo);
		return BoundCall.Synthesized(syntax, ((object)constrainedToTypeOpt == null) ? null : new BoundTypeExpression(syntax, null, constrainedToTypeOpt), ThreeState.Unknown, leftTruthOperator, loweredLeft);
	}

	private BoundExpression LowerUserDefinedBinaryOperator(SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol type, MethodSymbol? method, TypeSymbol? constrainedToTypeOpt)
	{
		if (operatorKind.IsLifted())
		{
			return RewriteLiftedBinaryOperator(syntax, operatorKind, loweredLeft, loweredRight, type, method, constrainedToTypeOpt);
		}
		return BoundCall.Synthesized(syntax, ((object)constrainedToTypeOpt == null) ? null : new BoundTypeExpression(syntax, null, constrainedToTypeOpt), ThreeState.Unknown, method, loweredLeft, loweredRight);
	}

	private BoundExpression? TrivialLiftedComparisonOperatorOptimizations(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression left, BoundExpression right, MethodSymbol? method, TypeSymbol? constrainedToTypeOpt)
	{
		bool flag = NullableNeverHasValue(left);
		bool flag2 = NullableNeverHasValue(right);
		TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
		if (flag & flag2)
		{
			return MakeLiteral(syntax, ConstantValue.Create(kind.Operator() == BinaryOperatorKind.Equal), specialType);
		}
		BoundExpression boundExpression = NullableAlwaysHasValue(left);
		BoundExpression boundExpression2 = NullableAlwaysHasValue(right);
		if (boundExpression != null && boundExpression2 != null)
		{
			return MakeBinaryOperator(syntax, kind.Unlifted(), boundExpression, boundExpression2, specialType, method, constrainedToTypeOpt);
		}
		BinaryOperatorKind binaryOperatorKind = kind.Operator();
		if ((flag && boundExpression2 != null) || (flag2 && boundExpression != null))
		{
			BoundExpression boundExpression3 = MakeLiteral(syntax, ConstantValue.Create(binaryOperatorKind == BinaryOperatorKind.NotEqual), specialType);
			BoundExpression boundExpression4 = (flag ? boundExpression2 : boundExpression);
			if (ReadIsSideeffecting(boundExpression4))
			{
				boundExpression3 = new BoundSequence(syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(boundExpression4), boundExpression3, specialType);
			}
			return boundExpression3;
		}
		if (flag | flag2)
		{
			BoundExpression boundExpression5 = (flag ? right : left);
			if (binaryOperatorKind == BinaryOperatorKind.Equal || binaryOperatorKind == BinaryOperatorKind.NotEqual)
			{
				BoundExpression boundExpression6 = _factory.MakeNullableHasValue(syntax, boundExpression5);
				if (binaryOperatorKind != BinaryOperatorKind.Equal)
				{
					return boundExpression6;
				}
				return MakeUnaryOperator(UnaryOperatorKind.BoolLogicalNegation, syntax, null, null, boundExpression6, specialType);
			}
			BoundExpression boundExpression7 = MakeBooleanConstant(syntax, binaryOperatorKind == BinaryOperatorKind.NotEqual);
			return _factory.MakeSequence(boundExpression5, boundExpression7);
		}
		return null;
	}

	private BoundExpression MakeOptimizedGetValueOrDefault(SyntaxNode syntax, BoundExpression expression)
	{
		if (expression.Type.IsNullableType())
		{
			return BoundCall.Synthesized(syntax, expression, ThreeState.Unknown, UnsafeGetNullableMethod(syntax, expression.Type, SpecialMember.System_Nullable_T_GetValueOrDefault));
		}
		return expression;
	}

	private BoundExpression MakeBooleanConstant(SyntaxNode syntax, bool value)
	{
		return MakeLiteral(syntax, ConstantValue.Create(value), _compilation.GetSpecialType(SpecialType.System_Boolean));
	}

	private BoundExpression MakeOptimizedHasValue(SyntaxNode syntax, BoundExpression expression)
	{
		if (expression.Type.IsNullableType())
		{
			return _factory.MakeNullableHasValue(syntax, expression);
		}
		return MakeBooleanConstant(syntax, value: true);
	}

	private BoundExpression MakeNullableHasValue(SyntaxNode syntax, BoundExpression expression)
	{
		return BoundCall.Synthesized(syntax, expression, ThreeState.Unknown, UnsafeGetNullableMethod(syntax, expression.Type, SpecialMember.System_Nullable_T_get_HasValue));
	}

	private BoundExpression LowerLiftedBuiltInComparisonOperator(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression loweredLeft, BoundExpression loweredRight)
	{
		BoundExpression boundExpression = TrivialLiftedComparisonOperatorOptimizations(syntax, kind, loweredLeft, loweredRight, null, null);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		BoundExpression boundExpression2 = NullableAlwaysHasValue(loweredLeft);
		BoundExpression boundExpression3 = NullableAlwaysHasValue(loweredRight);
		TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
		BinaryOperatorKind binaryOperatorKind = kind.Operator();
		if ((binaryOperatorKind == BinaryOperatorKind.Equal || binaryOperatorKind == BinaryOperatorKind.NotEqual) ? true : false)
		{
			if (canNotBeEqualToDefaultValue(boundExpression2?.ConstantValueOpt))
			{
				return MakeBinaryOperator(syntax, kind.Unlifted(), boundExpression2, MakeOptimizedGetValueOrDefault(syntax, loweredRight), specialType, null, null);
			}
			if (canNotBeEqualToDefaultValue(boundExpression3?.ConstantValueOpt))
			{
				return MakeBinaryOperator(syntax, kind.Unlifted(), MakeOptimizedGetValueOrDefault(syntax, loweredLeft), boundExpression3, specialType, null, null);
			}
		}
		BoundLocal boundLocal = _factory.StoreToTemp(boundExpression2 ?? loweredLeft, out BoundAssignmentOperator store);
		BoundLocal boundLocal2 = _factory.StoreToTemp(boundExpression3 ?? loweredRight, out BoundAssignmentOperator store2);
		BoundExpression loweredLeft2 = MakeOptimizedGetValueOrDefault(syntax, boundLocal);
		BoundExpression loweredRight2 = MakeOptimizedGetValueOrDefault(syntax, boundLocal2);
		BoundExpression loweredLeft3 = MakeOptimizedHasValue(syntax, boundLocal);
		BoundExpression loweredRight3 = MakeOptimizedHasValue(syntax, boundLocal2);
		BinaryOperatorKind binaryOperatorKind2 = kind.Operator();
		BinaryOperatorKind kind2;
		BinaryOperatorKind operatorKind;
		if (binaryOperatorKind2 == BinaryOperatorKind.Equal || binaryOperatorKind2 == BinaryOperatorKind.NotEqual)
		{
			kind2 = BinaryOperatorKind.Equal;
			operatorKind = BinaryOperatorKind.BoolEqual;
		}
		else
		{
			kind2 = binaryOperatorKind2;
			operatorKind = BinaryOperatorKind.BoolAnd;
		}
		BoundExpression loweredLeft4 = MakeBinaryOperator(syntax, kind2.WithType(kind.OperandTypes()), loweredLeft2, loweredRight2, specialType, null, null);
		BoundExpression loweredRight4 = MakeBinaryOperator(syntax, operatorKind, loweredLeft3, loweredRight3, specialType, null, null);
		BoundExpression boundExpression4 = MakeBinaryOperator(syntax, BinaryOperatorKind.BoolAnd, loweredLeft4, loweredRight4, specialType, null, null);
		if (binaryOperatorKind2 == BinaryOperatorKind.NotEqual)
		{
			boundExpression4 = _factory.Not(boundExpression4);
		}
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal.LocalSymbol, boundLocal2.LocalSymbol), ImmutableArray.Create((BoundExpression)store, (BoundExpression)store2), boundExpression4, specialType);
		static bool canNotBeEqualToDefaultValue([NotNullWhen(true)] ConstantValue? constantValue)
		{
			if ((object)constantValue != null && !constantValue.IsDefaultValue)
			{
				ConstantValueTypeDiscriminator discriminator = constantValue.Discriminator;
				if (discriminator - 6 <= ConstantValueTypeDiscriminator.UInt16 || discriminator - 13 <= ConstantValueTypeDiscriminator.SByte)
				{
					return true;
				}
			}
			return false;
		}
	}

	private BoundExpression LowerLiftedUserDefinedComparisonOperator(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression loweredLeft, BoundExpression loweredRight, MethodSymbol? method, TypeSymbol? constrainedToTypeOpt)
	{
		BoundExpression boundExpression = TrivialLiftedComparisonOperatorOptimizations(syntax, kind, loweredLeft, loweredRight, method, constrainedToTypeOpt);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		BoundExpression boundExpression2 = NullableAlwaysHasValue(loweredLeft);
		BoundExpression boundExpression3 = NullableAlwaysHasValue(loweredRight);
		BoundLocal boundLocal = _factory.StoreToTemp(boundExpression2 ?? loweredLeft, out BoundAssignmentOperator store);
		BoundLocal boundLocal2 = _factory.StoreToTemp(boundExpression3 ?? loweredRight, out BoundAssignmentOperator store2);
		BoundExpression loweredLeft2 = MakeOptimizedGetValueOrDefault(syntax, boundLocal);
		BoundExpression loweredRight2 = MakeOptimizedGetValueOrDefault(syntax, boundLocal2);
		BoundExpression boundExpression4 = MakeOptimizedHasValue(syntax, boundLocal);
		BoundExpression loweredRight3 = MakeOptimizedHasValue(syntax, boundLocal2);
		BinaryOperatorKind binaryOperatorKind = kind.Operator();
		BinaryOperatorKind operatorKind = ((binaryOperatorKind != BinaryOperatorKind.Equal && binaryOperatorKind != BinaryOperatorKind.NotEqual) ? BinaryOperatorKind.BoolAnd : BinaryOperatorKind.BoolEqual);
		TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
		BoundExpression rewrittenCondition = MakeBinaryOperator(syntax, operatorKind, boundExpression4, loweredRight3, specialType, null, null);
		BoundExpression boundExpression5 = MakeBinaryOperator(syntax, kind.Unlifted(), loweredLeft2, loweredRight2, specialType, method, constrainedToTypeOpt);
		BoundExpression rewrittenConsequence = ((binaryOperatorKind != BinaryOperatorKind.Equal && binaryOperatorKind != BinaryOperatorKind.NotEqual) ? boundExpression5 : ((boundExpression2 == null && boundExpression3 == null) ? RewriteConditionalOperator(syntax, boundExpression4, boundExpression5, MakeLiteral(syntax, ConstantValue.Create(binaryOperatorKind == BinaryOperatorKind.Equal), specialType), null, specialType, isRef: false) : boundExpression5));
		BoundExpression rewrittenAlternative = MakeBooleanConstant(syntax, binaryOperatorKind == BinaryOperatorKind.NotEqual);
		BoundExpression value = RewriteConditionalOperator(syntax, rewrittenCondition, rewrittenConsequence, rewrittenAlternative, null, specialType, isRef: false);
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal.LocalSymbol, boundLocal2.LocalSymbol), ImmutableArray.Create((BoundExpression)store, (BoundExpression)store2), value, specialType);
	}

	private BoundExpression? TrivialLiftedBinaryArithmeticOptimizations(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression left, BoundExpression right, TypeSymbol type, MethodSymbol? method, TypeSymbol? constrainedToTypeOpt)
	{
		bool num = NullableNeverHasValue(left);
		bool flag = NullableNeverHasValue(right);
		if (num & flag)
		{
			return new BoundDefaultExpression(syntax, type);
		}
		BoundExpression boundExpression = NullableAlwaysHasValue(left);
		BoundExpression boundExpression2 = NullableAlwaysHasValue(right);
		if (boundExpression != null && boundExpression2 != null)
		{
			return MakeLiftedBinaryOperatorConsequence(syntax, kind, boundExpression, boundExpression2, type, method, constrainedToTypeOpt);
		}
		return null;
	}

	private BoundExpression MakeLiftedBinaryOperatorConsequence(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression left, BoundExpression right, TypeSymbol type, MethodSymbol? method, TypeSymbol? constrainedToTypeOpt)
	{
		BoundExpression boundExpression = MakeBinaryOperator(syntax, kind.Unlifted(), left, right, type.GetNullableUnderlyingType(), method, constrainedToTypeOpt);
		return new BoundObjectCreationExpression(syntax, UnsafeGetNullableMethod(syntax, type, SpecialMember.System_Nullable_T__ctor), boundExpression);
	}

	private static BoundExpression? OptimizeLiftedArithmeticOperatorOneNull(SyntaxNode syntax, BoundExpression left, BoundExpression right, TypeSymbol type)
	{
		bool flag = NullableNeverHasValue(left);
		bool flag2 = NullableNeverHasValue(right);
		if (!(flag | flag2))
		{
			return null;
		}
		BoundExpression boundExpression = (flag ? right : left);
		BoundExpression boundExpression2 = NullableAlwaysHasValue(boundExpression) ?? boundExpression;
		if (boundExpression2.ConstantValueOpt != null)
		{
			return new BoundDefaultExpression(syntax, type);
		}
		return new BoundSequence(syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(boundExpression2), new BoundDefaultExpression(syntax, type), type);
	}

	private BoundExpression LowerLiftedBinaryArithmeticOperator(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol type, MethodSymbol? method, TypeSymbol? constrainedToTypeOpt)
	{
		BoundExpression boundExpression = OptimizeLiftedBinaryArithmetic(syntax, kind, loweredLeft, loweredRight, type, method, constrainedToTypeOpt);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		ArrayBuilder<LocalSymbol> instance2 = ArrayBuilder<LocalSymbol>.GetInstance();
		BoundExpression boundExpression2 = NullableAlwaysHasValue(loweredLeft);
		BoundExpression boundExpression3 = NullableAlwaysHasValue(loweredRight);
		if (boundExpression2 == null)
		{
			boundExpression2 = loweredLeft;
		}
		BoundExpression operand = boundExpression2;
		operand = CaptureExpressionInTempIfNeeded(operand, instance, instance2);
		BoundExpression operand2 = boundExpression3 ?? loweredRight;
		operand2 = CaptureExpressionInTempIfNeeded(operand2, instance, instance2);
		BoundExpression left = MakeOptimizedGetValueOrDefault(syntax, operand);
		BoundExpression right = MakeOptimizedGetValueOrDefault(syntax, operand2);
		BoundExpression loweredLeft2 = MakeOptimizedHasValue(syntax, operand);
		BoundExpression loweredRight2 = MakeOptimizedHasValue(syntax, operand2);
		TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
		BoundExpression rewrittenCondition = MakeBinaryOperator(syntax, BinaryOperatorKind.BoolAnd, loweredLeft2, loweredRight2, specialType, null, null);
		BoundExpression rewrittenConsequence = MakeLiftedBinaryOperatorConsequence(syntax, kind, left, right, type, method, constrainedToTypeOpt);
		BoundExpression rewrittenAlternative = new BoundDefaultExpression(syntax, type);
		BoundExpression value = RewriteConditionalOperator(syntax, rewrittenCondition, rewrittenConsequence, rewrittenAlternative, null, type, isRef: false);
		return new BoundSequence(syntax, instance2.ToImmutableAndFree(), instance.ToImmutableAndFree(), value, type);
	}

	private BoundExpression CaptureExpressionInTempIfNeeded(BoundExpression operand, ArrayBuilder<BoundExpression> sideeffects, ArrayBuilder<LocalSymbol> locals, SynthesizedLocalKind kind = SynthesizedLocalKind.LoweringTemp)
	{
		if (CanChangeValueBetweenReads(operand))
		{
			BoundLocal boundLocal = _factory.StoreToTemp(operand, out BoundAssignmentOperator store, RefKind.None, kind);
			sideeffects.Add(store);
			locals.Add(boundLocal.LocalSymbol);
			operand = boundLocal;
		}
		return operand;
	}

	private BoundExpression? OptimizeLiftedBinaryArithmetic(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression left, BoundExpression right, TypeSymbol type, MethodSymbol? method, TypeSymbol? constrainedToTypeOpt)
	{
		BoundExpression boundExpression = TrivialLiftedBinaryArithmeticOptimizations(syntax, kind, left, right, type, method, constrainedToTypeOpt);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		if (kind == BinaryOperatorKind.LiftedBoolAnd || kind == BinaryOperatorKind.LiftedBoolOr)
		{
			return LowerLiftedBooleanOperator(syntax, kind, left, right);
		}
		boundExpression = OptimizeLiftedArithmeticOperatorOneNull(syntax, left, right, type);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		BoundExpression boundExpression2 = NullableAlwaysHasValue(right);
		if (boundExpression2 != null && boundExpression2.ConstantValueOpt != null && left.Kind == BoundKind.Sequence)
		{
			BoundSequence boundSequence = (BoundSequence)left;
			if (boundSequence.Value.Kind == BoundKind.ConditionalOperator)
			{
				BoundConditionalOperator boundConditionalOperator = (BoundConditionalOperator)boundSequence.Value;
				if (NullableAlwaysHasValue(boundConditionalOperator.Consequence) != null && NullableNeverHasValue(boundConditionalOperator.Alternative))
				{
					return new BoundSequence(syntax, boundSequence.Locals, boundSequence.SideEffects, RewriteConditionalOperator(syntax, boundConditionalOperator.Condition, MakeBinaryOperator(syntax, kind, boundConditionalOperator.Consequence, right, type, method, constrainedToTypeOpt), MakeBinaryOperator(syntax, kind, boundConditionalOperator.Alternative, right, type, method, constrainedToTypeOpt), null, type, isRef: false), type);
				}
			}
		}
		return null;
	}

	private BoundExpression MakeNewNullableBoolean(SyntaxNode syntax, bool? value)
	{
		TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
		NamedTypeSymbol orCreateNullableType = _compilation.GetOrCreateNullableType(specialType);
		if (!value.HasValue)
		{
			return new BoundDefaultExpression(syntax, orCreateNullableType);
		}
		return new BoundObjectCreationExpression(syntax, UnsafeGetNullableMethod(syntax, orCreateNullableType, SpecialMember.System_Nullable_T__ctor), MakeBooleanConstant(syntax, value == true));
	}

	private BoundExpression? OptimizeLiftedBooleanOperatorOneNull(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression left, BoundExpression right)
	{
		bool flag = NullableNeverHasValue(left);
		bool flag2 = NullableNeverHasValue(right);
		if (!(flag | flag2))
		{
			return null;
		}
		BoundExpression boundExpression = (flag ? left : right);
		BoundExpression boundExpression2 = (flag ? right : left);
		BoundExpression boundExpression3 = NullableAlwaysHasValue(boundExpression2);
		BoundExpression boundExpression4 = new BoundDefaultExpression(syntax, boundExpression.Type);
		if (boundExpression3 != null)
		{
			BoundExpression boundExpression5 = MakeNewNullableBoolean(syntax, kind == BinaryOperatorKind.LiftedBoolOr);
			return RewriteConditionalOperator(syntax, boundExpression3, (kind == BinaryOperatorKind.LiftedBoolAnd) ? boundExpression4 : boundExpression5, (kind == BinaryOperatorKind.LiftedBoolAnd) ? boundExpression5 : boundExpression4, null, boundExpression.Type, isRef: false);
		}
		BoundLocal boundLocal = _factory.StoreToTemp(boundExpression2, out BoundAssignmentOperator store);
		BoundExpression rewrittenCondition = MakeOptimizedGetValueOrDefault(syntax, boundLocal);
		BoundExpression rewrittenConsequence = ((kind == BinaryOperatorKind.LiftedBoolAnd) ? boundExpression4 : boundLocal);
		BoundExpression rewrittenAlternative = ((kind == BinaryOperatorKind.LiftedBoolAnd) ? boundLocal : boundExpression4);
		BoundExpression boundExpression6 = RewriteConditionalOperator(syntax, rewrittenCondition, rewrittenConsequence, rewrittenAlternative, null, boundExpression.Type, isRef: false);
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal.LocalSymbol), ImmutableArray.Create((BoundExpression)store), boundExpression6, boundExpression6.Type);
	}

	private BoundExpression? OptimizeLiftedBooleanOperatorOneNonNull(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression left, BoundExpression right)
	{
		BoundExpression boundExpression = NullableAlwaysHasValue(left);
		BoundExpression boundExpression2 = NullableAlwaysHasValue(right);
		if (boundExpression == null && boundExpression2 == null)
		{
			return null;
		}
		BoundLocal boundLocal = _factory.StoreToTemp(boundExpression ?? left, out BoundAssignmentOperator store);
		BoundLocal boundLocal2 = _factory.StoreToTemp(boundExpression2 ?? right, out BoundAssignmentOperator store2);
		BoundLocal obj = ((boundExpression == null) ? boundLocal2 : boundLocal);
		BoundExpression boundExpression3 = ((boundExpression == null) ? boundLocal : boundLocal2);
		BoundExpression rewrittenCondition = obj;
		BoundExpression boundExpression4 = MakeNewNullableBoolean(syntax, kind == BinaryOperatorKind.LiftedBoolOr);
		BoundExpression rewrittenConsequence = ((kind == BinaryOperatorKind.LiftedBoolOr) ? boundExpression4 : boundExpression3);
		BoundExpression rewrittenAlternative = ((kind == BinaryOperatorKind.LiftedBoolOr) ? boundExpression3 : boundExpression4);
		BoundExpression boundExpression5 = RewriteConditionalOperator(syntax, rewrittenCondition, rewrittenConsequence, rewrittenAlternative, null, boundExpression4.Type, isRef: false);
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal.LocalSymbol, boundLocal2.LocalSymbol), ImmutableArray.Create((BoundExpression)store, (BoundExpression)store2), boundExpression5, boundExpression5.Type);
	}

	private BoundExpression LowerLiftedBooleanOperator(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression loweredLeft, BoundExpression loweredRight)
	{
		BoundExpression boundExpression = OptimizeLiftedBooleanOperatorOneNull(syntax, kind, loweredLeft, loweredRight);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		boundExpression = OptimizeLiftedBooleanOperatorOneNonNull(syntax, kind, loweredLeft, loweredRight);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		BoundLocal boundLocal = _factory.StoreToTemp(loweredLeft, out BoundAssignmentOperator store);
		BoundLocal boundLocal2 = _factory.StoreToTemp(loweredRight, out BoundAssignmentOperator store2);
		TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
		MethodSymbol method = UnsafeGetNullableMethod(syntax, boundLocal.Type, SpecialMember.System_Nullable_T_GetValueOrDefault);
		MethodSymbol method2 = UnsafeGetNullableMethod(syntax, boundLocal2.Type, SpecialMember.System_Nullable_T_GetValueOrDefault);
		BoundExpression loweredLeft2 = BoundCall.Synthesized(syntax, boundLocal, ThreeState.Unknown, method);
		BoundExpression loweredLeft3 = BoundCall.Synthesized(syntax, boundLocal2, ThreeState.Unknown, method2);
		BoundExpression loweredRight2 = _factory.MakeNullableHasValue(syntax, boundLocal);
		BoundExpression loweredOperand = MakeBinaryOperator(syntax, BinaryOperatorKind.LogicalBoolOr, loweredLeft3, loweredRight2, specialType, null, null);
		BoundExpression loweredRight3 = MakeUnaryOperator(UnaryOperatorKind.BoolLogicalNegation, syntax, null, null, loweredOperand, specialType);
		BoundExpression rewrittenCondition = MakeBinaryOperator(syntax, BinaryOperatorKind.LogicalBoolOr, loweredLeft2, loweredRight3, specialType, null, null);
		BoundExpression rewrittenConsequence = ((kind == BinaryOperatorKind.LiftedBoolAnd) ? boundLocal2 : boundLocal);
		BoundExpression boundExpression2 = ((kind == BinaryOperatorKind.LiftedBoolAnd) ? boundLocal : boundLocal2);
		BoundExpression boundExpression3 = RewriteConditionalOperator(syntax, rewrittenCondition, rewrittenConsequence, boundExpression2, null, boundExpression2.Type, isRef: false);
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal.LocalSymbol, boundLocal2.LocalSymbol), ImmutableArray.Create((BoundExpression)store, (BoundExpression)store2), boundExpression3, boundExpression3.Type);
	}

	private MethodSymbol UnsafeGetNullableMethod(SyntaxNode syntax, TypeSymbol nullableType, SpecialMember member)
	{
		return UnsafeGetNullableMethod(syntax, nullableType, member, _compilation, _diagnostics);
	}

	internal static MethodSymbol UnsafeGetNullableMethod(SyntaxNode syntax, TypeSymbol nullableType, SpecialMember member, CSharpCompilation compilation, BindingDiagnosticBag diagnostics)
	{
		NamedTypeSymbol newOwner = nullableType as NamedTypeSymbol;
		return UnsafeGetSpecialTypeMethod(syntax, member, compilation, diagnostics).AsMember(newOwner);
	}

	private bool TryGetNullableMethod(SyntaxNode syntax, TypeSymbol nullableType, SpecialMember member, out MethodSymbol result, bool isOptional = false)
	{
		NamedTypeSymbol newOwner = (NamedTypeSymbol)nullableType;
		if (TryGetSpecialTypeMethod(syntax, member, out result, isOptional))
		{
			result = result.AsMember(newOwner);
			return true;
		}
		return false;
	}

	private BoundExpression RewriteNullableNullEquality(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol returnType)
	{
		BoundExpression boundExpression = (loweredRight.IsLiteralNull() ? loweredLeft : loweredRight);
		if (NullableNeverHasValue(boundExpression))
		{
			return MakeLiteral(syntax, ConstantValue.Create(kind == BinaryOperatorKind.NullableNullEqual), returnType);
		}
		BoundExpression boundExpression2 = NullableAlwaysHasValue(boundExpression);
		if (boundExpression2 != null)
		{
			return new BoundSequence(syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(boundExpression2), MakeBooleanConstant(syntax, kind == BinaryOperatorKind.NullableNullNotEqual), returnType);
		}
		if (boundExpression is BoundLoweredConditionalAccess boundLoweredConditionalAccess && (boundLoweredConditionalAccess.WhenNullOpt == null || boundLoweredConditionalAccess.WhenNullOpt.IsDefaultValue()))
		{
			BoundExpression boundExpression3 = RewriteNullableNullEquality(syntax, kind, boundLoweredConditionalAccess.WhenNotNull, loweredLeft.IsLiteralNull() ? loweredLeft : loweredRight, returnType);
			BoundExpression whenNullOpt = ((kind == BinaryOperatorKind.NullableNullEqual) ? MakeBooleanConstant(syntax, value: true) : null);
			return boundLoweredConditionalAccess.Update(boundLoweredConditionalAccess.Receiver, boundLoweredConditionalAccess.HasValueMethodOpt, boundExpression3, whenNullOpt, boundLoweredConditionalAccess.Id, boundLoweredConditionalAccess.ForceCopyOfNullableValueType, boundExpression3.Type);
		}
		BoundExpression boundExpression4 = MakeNullableHasValue(syntax, boundExpression);
		if (kind != BinaryOperatorKind.NullableNullNotEqual)
		{
			return new BoundUnaryOperator(syntax, UnaryOperatorKind.BoolLogicalNegation, boundExpression4, null, null, null, LookupResultKind.Viable, returnType);
		}
		return boundExpression4;
	}

	private BoundExpression RewriteStringEquality(BoundBinaryOperator? oldNode, SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol type, SpecialMember member)
	{
		if (oldNode != null && (loweredLeft.ConstantValueOpt == ConstantValue.Null || loweredRight.ConstantValueOpt == ConstantValue.Null))
		{
			return oldNode.Update(operatorKind, oldNode.ConstantValueOpt, oldNode.BinaryOperatorMethod, oldNode.ConstrainedToType, oldNode.ResultKind, loweredLeft, loweredRight, type);
		}
		MethodSymbol method = UnsafeGetSpecialTypeMethod(syntax, member);
		return BoundCall.Synthesized(syntax, null, ThreeState.Unknown, method, loweredLeft, loweredRight);
	}

	private BoundExpression RewriteDelegateOperation(SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol type, SpecialMember member)
	{
		MethodSymbol methodSymbol;
		if (operatorKind == BinaryOperatorKind.DelegateEqual || operatorKind == BinaryOperatorKind.DelegateNotEqual)
		{
			methodSymbol = (MethodSymbol)_compilation.Assembly.GetSpecialTypeMember(member);
			if (loweredRight.IsLiteralNull() || loweredLeft.IsLiteralNull() || (object)(methodSymbol = (MethodSymbol)_compilation.Assembly.GetSpecialTypeMember(member)) == null)
			{
				operatorKind = (operatorKind & ~BinaryOperatorKind.Delegate) | BinaryOperatorKind.Object;
				return new BoundBinaryOperator(syntax, operatorKind, null, null, null, LookupResultKind.Empty, loweredLeft, loweredRight, type);
			}
		}
		else
		{
			methodSymbol = UnsafeGetSpecialTypeMethod(syntax, member);
		}
		BoundExpression boundExpression = (_inExpressionLambda ? ((BoundExpression)new BoundBinaryOperator(syntax, operatorKind, null, methodSymbol, null, LookupResultKind.Empty, loweredLeft, loweredRight, methodSymbol.ReturnType)) : ((BoundExpression)BoundCall.Synthesized(syntax, null, ThreeState.Unknown, methodSymbol, loweredLeft, loweredRight)));
		if (methodSymbol.ReturnType.SpecialType != SpecialType.System_Delegate)
		{
			return boundExpression;
		}
		return MakeConversionNode(syntax, boundExpression, Conversion.ExplicitReference, type, @checked: false);
	}

	private BoundExpression RewriteDecimalBinaryOperation(SyntaxNode syntax, BoundExpression loweredLeft, BoundExpression loweredRight, BinaryOperatorKind operatorKind)
	{
		MethodSymbol method = UnsafeGetSpecialTypeMethod(syntax, operatorKind switch
		{
			BinaryOperatorKind.DecimalAddition => SpecialMember.System_Decimal__op_Addition, 
			BinaryOperatorKind.DecimalSubtraction => SpecialMember.System_Decimal__op_Subtraction, 
			BinaryOperatorKind.DecimalMultiplication => SpecialMember.System_Decimal__op_Multiply, 
			BinaryOperatorKind.DecimalDivision => SpecialMember.System_Decimal__op_Division, 
			BinaryOperatorKind.DecimalRemainder => SpecialMember.System_Decimal__op_Modulus, 
			BinaryOperatorKind.DecimalEqual => SpecialMember.System_Decimal__op_Equality, 
			BinaryOperatorKind.DecimalNotEqual => SpecialMember.System_Decimal__op_Inequality, 
			BinaryOperatorKind.DecimalLessThan => SpecialMember.System_Decimal__op_LessThan, 
			BinaryOperatorKind.DecimalLessThanOrEqual => SpecialMember.System_Decimal__op_LessThanOrEqual, 
			BinaryOperatorKind.DecimalGreaterThan => SpecialMember.System_Decimal__op_GreaterThan, 
			BinaryOperatorKind.DecimalGreaterThanOrEqual => SpecialMember.System_Decimal__op_GreaterThanOrEqual, 
			_ => throw ExceptionUtilities.UnexpectedValue(operatorKind), 
		});
		return BoundCall.Synthesized(syntax, null, ThreeState.Unknown, method, loweredLeft, loweredRight);
	}

	private BoundExpression MakeNullCheck(SyntaxNode syntax, BoundExpression rewrittenExpr, BinaryOperatorKind operatorKind)
	{
		TypeSymbol type = rewrittenExpr.Type;
		TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
		if (rewrittenExpr.ConstantValueOpt != null)
		{
			switch (operatorKind)
			{
			case BinaryOperatorKind.Equal:
				return MakeLiteral(syntax, ConstantValue.Create(rewrittenExpr.ConstantValueOpt.IsNull, ConstantValueTypeDiscriminator.Boolean), specialType);
			case BinaryOperatorKind.NotEqual:
				return MakeLiteral(syntax, ConstantValue.Create(!rewrittenExpr.ConstantValueOpt.IsNull, ConstantValueTypeDiscriminator.Boolean), specialType);
			}
		}
		TypeSymbol specialType2 = _compilation.GetSpecialType(SpecialType.System_Object);
		if ((object)type != null)
		{
			if (type.Kind == SymbolKind.TypeParameter)
			{
				rewrittenExpr = MakeConversionNode(syntax, rewrittenExpr, Conversion.Boxing, specialType2, @checked: false);
			}
			else if (type.IsNullableType())
			{
				operatorKind |= BinaryOperatorKind.NullableNull;
			}
		}
		return MakeBinaryOperator(syntax, operatorKind, rewrittenExpr, MakeLiteral(syntax, ConstantValue.Null, specialType2), specialType, null, null);
	}

	private BoundExpression RewriteBuiltInShiftOperation(BoundBinaryOperator? oldNode, SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol type, int rightMask)
	{
		SyntaxNode syntax2 = loweredRight.Syntax;
		ConstantValue constantValueOpt = loweredRight.ConstantValueOpt;
		TypeSymbol type2 = loweredRight.Type;
		if (constantValueOpt != null && constantValueOpt.IsIntegral)
		{
			int num = constantValueOpt.Int32Value & rightMask;
			if (num == 0)
			{
				return loweredLeft;
			}
			loweredRight = MakeLiteral(syntax2, ConstantValue.Create(num), type2);
		}
		else
		{
			BinaryOperatorKind operatorKind2 = (operatorKind & ~BinaryOperatorKind.OpMask) | BinaryOperatorKind.And;
			loweredRight = new BoundBinaryOperator(syntax2, operatorKind2, null, null, null, LookupResultKind.Viable, loweredRight, MakeLiteral(syntax2, ConstantValue.Create(rightMask), type2), type2);
		}
		if (oldNode != null)
		{
			return oldNode.Update(operatorKind, null, null, null, oldNode.ResultKind, loweredLeft, loweredRight, type);
		}
		return new BoundBinaryOperator(syntax, operatorKind, null, null, null, LookupResultKind.Viable, loweredLeft, loweredRight, type);
	}

	private BoundExpression RewriteBuiltInNativeShiftOperation(BoundBinaryOperator? oldNode, SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol type)
	{
		TypeSymbol type2 = loweredLeft.Type;
		ConstantValue constantValueOpt = loweredRight.ConstantValueOpt;
		TypeSymbol type3 = loweredRight.Type;
		SyntaxNode syntax2 = _factory.Syntax;
		_factory.Syntax = loweredRight.Syntax;
		if (constantValueOpt != null && constantValueOpt.Discriminator == ConstantValueTypeDiscriminator.Int32)
		{
			int int32Value = constantValueOpt.Int32Value;
			if (int32Value >= 0 && int32Value <= 31)
			{
				int int32Value2 = constantValueOpt.Int32Value;
				if (int32Value2 == 0)
				{
					return loweredLeft;
				}
				loweredRight = _factory.Literal(int32Value2);
				goto IL_00d6;
			}
		}
		BinaryOperatorKind kind = (operatorKind & ~BinaryOperatorKind.OpMask) | BinaryOperatorKind.And;
		loweredRight = _factory.Binary(kind, type3, loweredRight, _factory.IntSubtract(_factory.IntMultiply(_factory.Sizeof(type2), _factory.Literal(8)), _factory.Literal(1)));
		goto IL_00d6;
		IL_00d6:
		_factory.Syntax = syntax;
		BoundBinaryOperator result = ((oldNode == null) ? _factory.Binary(operatorKind, type, loweredLeft, loweredRight) : oldNode.Update(operatorKind, null, null, null, oldNode.ResultKind, loweredLeft, loweredRight, type));
		_factory.Syntax = syntax2;
		return result;
	}

	private BoundExpression RewritePointerNumericOperator(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol returnType, bool isPointerElementAccess, bool isLeftPointer)
	{
		if (isLeftPointer)
		{
			loweredRight = MakeSizeOfMultiplication(loweredRight, (PointerTypeSymbol)loweredLeft.Type, kind.IsChecked());
		}
		else
		{
			loweredLeft = MakeSizeOfMultiplication(loweredLeft, (PointerTypeSymbol)loweredRight.Type, kind.IsChecked());
		}
		if (isPointerElementAccess)
		{
			kind &= ~BinaryOperatorKind.Checked;
		}
		return new BoundBinaryOperator(syntax, kind, null, null, null, LookupResultKind.Viable, loweredLeft, loweredRight, returnType);
	}

	private BoundExpression MakeSizeOfMultiplication(BoundExpression numericOperand, PointerTypeSymbol pointerType, bool isChecked)
	{
		BoundExpression boundExpression = _factory.Sizeof(pointerType.PointedAtType);
		ConstantValue? constantValueOpt = numericOperand.ConstantValueOpt;
		if ((object)constantValueOpt != null && constantValueOpt.UInt64Value == 1)
		{
			return boundExpression;
		}
		SpecialType specialType = numericOperand.Type.SpecialType;
		ConstantValue? constantValueOpt2 = boundExpression.ConstantValueOpt;
		if ((object)constantValueOpt2 != null && constantValueOpt2.Int32Value == 1)
		{
			SpecialType specialType2 = specialType;
			switch (specialType)
			{
			case SpecialType.System_Int32:
				if (isChecked)
				{
					ConstantValue constantValueOpt4 = numericOperand.ConstantValueOpt;
					if (constantValueOpt4 == null || constantValueOpt4.Int32Value < 0)
					{
						specialType2 = SpecialType.System_IntPtr;
					}
				}
				break;
			case SpecialType.System_UInt32:
			{
				ConstantValue constantValueOpt3 = numericOperand.ConstantValueOpt;
				if (constantValueOpt3 == null || constantValueOpt3.UInt32Value > int.MaxValue)
				{
					specialType2 = SpecialType.System_UIntPtr;
				}
				break;
			}
			case SpecialType.System_Int64:
				specialType2 = SpecialType.System_IntPtr;
				break;
			case SpecialType.System_UInt64:
				specialType2 = SpecialType.System_UIntPtr;
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(specialType);
			}
			if (specialType2 != specialType)
			{
				return _factory.Convert(_factory.SpecialType(specialType2), numericOperand, Conversion.IntegerToPointer);
			}
			return numericOperand;
		}
		BinaryOperatorKind binaryOperatorKind = BinaryOperatorKind.Multiplication;
		TypeSymbol typeSymbol2;
		TypeSymbol typeSymbol3;
		switch (specialType)
		{
		case SpecialType.System_Int32:
		{
			TypeSymbol typeSymbol6 = _factory.SpecialType(SpecialType.System_IntPtr);
			numericOperand = _factory.Convert(typeSymbol6, numericOperand, Conversion.IntegerToPointer, isChecked);
			binaryOperatorKind |= BinaryOperatorKind.Int;
			typeSymbol2 = typeSymbol6;
			typeSymbol3 = typeSymbol6;
			break;
		}
		case SpecialType.System_UInt32:
		{
			TypeSymbol typeSymbol5 = _factory.SpecialType(SpecialType.System_Int64);
			NamedTypeSymbol namedTypeSymbol3 = _factory.SpecialType(SpecialType.System_IntPtr);
			numericOperand = _factory.Convert(typeSymbol5, numericOperand, Conversion.ExplicitNumeric, isChecked);
			boundExpression = _factory.Convert(typeSymbol5, boundExpression, Conversion.ExplicitNumeric, isChecked);
			binaryOperatorKind |= BinaryOperatorKind.Long;
			typeSymbol2 = typeSymbol5;
			typeSymbol3 = namedTypeSymbol3;
			break;
		}
		case SpecialType.System_Int64:
		{
			TypeSymbol typeSymbol4 = _factory.SpecialType(SpecialType.System_Int64);
			NamedTypeSymbol namedTypeSymbol2 = _factory.SpecialType(SpecialType.System_IntPtr);
			boundExpression = _factory.Convert(typeSymbol4, boundExpression, Conversion.ExplicitNumeric, isChecked);
			binaryOperatorKind |= BinaryOperatorKind.Long;
			typeSymbol2 = typeSymbol4;
			typeSymbol3 = namedTypeSymbol2;
			break;
		}
		case SpecialType.System_UInt64:
		{
			TypeSymbol typeSymbol = _factory.SpecialType(SpecialType.System_UInt64);
			NamedTypeSymbol namedTypeSymbol = _factory.SpecialType(SpecialType.System_UIntPtr);
			boundExpression = _factory.Convert(typeSymbol, boundExpression, Conversion.ExplicitNumeric, isChecked);
			binaryOperatorKind |= BinaryOperatorKind.ULong;
			typeSymbol2 = typeSymbol;
			typeSymbol3 = namedTypeSymbol;
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(specialType);
		}
		if (isChecked)
		{
			binaryOperatorKind |= BinaryOperatorKind.Checked;
		}
		BoundBinaryOperator boundBinaryOperator = _factory.Binary(binaryOperatorKind, typeSymbol2, numericOperand, boundExpression);
		if (!TypeSymbol.Equals(typeSymbol3, typeSymbol2, TypeCompareKind.ConsiderEverything))
		{
			return _factory.Convert(typeSymbol3, boundBinaryOperator, Conversion.IntegerToPointer);
		}
		return boundBinaryOperator;
	}

	private BoundExpression RewritePointerSubtraction(BinaryOperatorKind kind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol returnType)
	{
		PointerTypeSymbol pointerTypeSymbol = (PointerTypeSymbol)loweredLeft.Type;
		BoundExpression right = _factory.Sizeof(pointerTypeSymbol.PointedAtType);
		return _factory.Convert(returnType, _factory.Binary(BinaryOperatorKind.Division, _factory.SpecialType(SpecialType.System_IntPtr), _factory.Binary(kind & ~BinaryOperatorKind.Checked, returnType, loweredLeft, loweredRight), right), Conversion.PointerToInteger);
	}

	public override BoundNode VisitBlock(BoundBlock node)
	{
		if (Instrument)
		{
			Instrumenter.PreInstrumentBlock(node, this);
		}
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		ArrayBuilder<LocalSymbol> additionalLocals = _additionalLocals;
		if (additionalLocals == null)
		{
			_additionalLocals = ArrayBuilder<LocalSymbol>.GetInstance();
		}
		try
		{
			VisitStatementSubList(instance, node.Statements);
			TemporaryArray<LocalSymbol> additionalLocals2 = TemporaryArray<LocalSymbol>.Empty;
			BoundBlockInstrumentation instrumentation = null;
			if (Instrument)
			{
				Instrumenter.InstrumentBlock(node, this, ref additionalLocals2, out BoundStatement prologue, out BoundStatement epilogue, out instrumentation);
				if (prologue != null)
				{
					instance.Insert(0, prologue);
				}
				if (epilogue != null)
				{
					instance.Add(epilogue);
				}
			}
			ImmutableArray<LocalSymbol> self = node.Locals;
			if (additionalLocals == null)
			{
				self = self.AddRange(_additionalLocals);
			}
			self = self.AddRange(in additionalLocals2);
			return new BoundBlock(node.Syntax, self, node.LocalFunctions, node.HasUnsafeModifier, instrumentation, instance.ToImmutableAndFree(), node.HasErrors);
		}
		finally
		{
			if (additionalLocals == null)
			{
				_additionalLocals.Free();
				_additionalLocals = additionalLocals;
			}
		}
	}

	public void VisitStatementSubList(ArrayBuilder<BoundStatement> builder, ImmutableArray<BoundStatement> statements, int startIndex = 0)
	{
		for (int i = startIndex; i < statements.Length; i++)
		{
			BoundStatement boundStatement = VisitPossibleUsingDeclaration(statements[i], statements, i, out var replacedLocalDeclarations);
			if (boundStatement != null)
			{
				builder.Add(boundStatement);
			}
			if (replacedLocalDeclarations)
			{
				break;
			}
		}
	}

	public BoundStatement? VisitPossibleUsingDeclaration(BoundStatement node, ImmutableArray<BoundStatement> statements, int statementIndex, out bool replacedLocalDeclarations)
	{
		switch (node.Kind)
		{
		case BoundKind.LabeledStatement:
		{
			BoundLabeledStatement boundLabeledStatement = (BoundLabeledStatement)node;
			return MakeLabeledStatement(boundLabeledStatement, VisitPossibleUsingDeclaration(boundLabeledStatement.Body, statements, statementIndex, out replacedLocalDeclarations));
		}
		case BoundKind.UsingLocalDeclarations:
		{
			ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
			VisitStatementSubList(instance, statements, statementIndex + 1);
			replacedLocalDeclarations = true;
			return MakeLocalUsingDeclarationStatement((BoundUsingLocalDeclarations)node, instance.ToImmutableAndFree());
		}
		default:
			replacedLocalDeclarations = false;
			return VisitStatement(node);
		}
	}

	public override BoundNode VisitNoOpStatement(BoundNoOpStatement node)
	{
		if (!node.WasCompilerGenerated && Instrument)
		{
			return Instrumenter.InstrumentNoOpStatement(node, node);
		}
		return new BoundBlock(node.Syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray<BoundStatement>.Empty);
	}

	public override BoundNode VisitBreakStatement(BoundBreakStatement node)
	{
		BoundStatement boundStatement = new BoundGotoStatement(node.Syntax, node.Label, node.HasErrors);
		if (Instrument && !node.WasCompilerGenerated)
		{
			boundStatement = Instrumenter.InstrumentBreakStatement(node, boundStatement);
		}
		return boundStatement;
	}

	public override BoundNode VisitDynamicInvocation(BoundDynamicInvocation node)
	{
		return VisitDynamicInvocation(node, resultDiscarded: false);
	}

	public BoundExpression VisitDynamicInvocation(BoundDynamicInvocation node, bool resultDiscarded)
	{
		ImmutableArray<BoundExpression> loweredArguments = VisitList(node.Arguments);
		BoundMethodGroup boundMethodGroup;
		ImmutableArray<TypeWithAnnotations> typeArgumentsOpt;
		string name;
		bool flag;
		BoundExpression boundExpression;
		switch (node.Expression.Kind)
		{
		case BoundKind.MethodGroup:
			boundMethodGroup = (BoundMethodGroup)node.Expression;
			typeArgumentsOpt = boundMethodGroup.TypeArgumentsOpt;
			name = boundMethodGroup.Name;
			flag = ((uint?)boundMethodGroup.Flags & 2u) != 0;
			if (boundMethodGroup.ReceiverOpt == null)
			{
				NamedTypeSymbol containingType = node.ApplicableMethods.First().ContainingType;
				boundExpression = new BoundTypeExpression(node.Syntax, null, containingType);
			}
			else
			{
				if (flag)
				{
					MethodSymbol topLevelMethod = _factory.TopLevelMethod;
					if ((object)topLevelMethod != null && !topLevelMethod.RequiresInstanceReceiver)
					{
						boundExpression = new BoundTypeExpression(node.Syntax, null, _factory.CurrentType);
						goto IL_010b;
					}
				}
				boundExpression = VisitExpression(boundMethodGroup.ReceiverOpt);
			}
			goto IL_010b;
		case BoundKind.DynamicMemberAccess:
		{
			BoundDynamicMemberAccess boundDynamicMemberAccess = (BoundDynamicMemberAccess)node.Expression;
			name = boundDynamicMemberAccess.Name;
			typeArgumentsOpt = boundDynamicMemberAccess.TypeArgumentsOpt;
			boundExpression = VisitExpression(boundDynamicMemberAccess.Receiver);
			flag = false;
			break;
		}
		default:
			{
				BoundExpression loweredReceiver = VisitExpression(node.Expression);
				return _dynamicFactory.MakeDynamicInvocation(loweredReceiver, loweredArguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, resultDiscarded).ToExpression();
			}
			IL_010b:
			EmbedIfNeedTo(boundExpression, boundMethodGroup.Methods, node.Syntax);
			break;
		}
		return _dynamicFactory.MakeDynamicMemberInvocation(name, boundExpression, typeArgumentsOpt, loweredArguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, flag, resultDiscarded).ToExpression();
	}

	private void EmbedIfNeedTo(BoundExpression receiver, ImmutableArray<MethodSymbol> methods, SyntaxNode syntaxNode)
	{
		PEModuleBuilder emitModule = EmitModule;
		if (emitModule == null || receiver == null || (object)receiver.Type == null)
		{
			return;
		}
		AssemblySymbol containingAssembly = receiver.Type.ContainingAssembly;
		if ((object)containingAssembly != null && containingAssembly.IsLinked)
		{
			foreach (MethodSymbol item in methods)
			{
				emitModule.EmbeddedTypesManagerOpt.EmbedMethodIfNeedTo(item.OriginalDefinition.GetCciAdapter(), syntaxNode, _diagnostics.DiagnosticBag);
			}
		}
	}

	private void EmbedIfNeedTo(BoundExpression receiver, ImmutableArray<PropertySymbol> properties, SyntaxNode syntaxNode)
	{
		PEModuleBuilder emitModule = EmitModule;
		if (emitModule == null || receiver == null || (object)receiver.Type == null)
		{
			return;
		}
		AssemblySymbol containingAssembly = receiver.Type.ContainingAssembly;
		if ((object)containingAssembly != null && containingAssembly.IsLinked)
		{
			foreach (PropertySymbol item in properties)
			{
				emitModule.EmbeddedTypesManagerOpt.EmbedPropertyIfNeedTo(item.OriginalDefinition.GetCciAdapter(), syntaxNode, _diagnostics.DiagnosticBag);
			}
		}
	}

	private void InterceptCallAndAdjustArguments(ref MethodSymbol method, ref BoundExpression? receiverOpt, ref ImmutableArray<BoundExpression> arguments, ref ImmutableArray<RefKind> argumentRefKindsOpt, ref ArrayBuilder<LocalSymbol> temps, bool invokedAsExtensionMethod, SimpleNameSyntax? nameSyntax)
	{
		(Location, MethodSymbol)? tuple = _compilation.TryGetInterceptor(nameSyntax);
		if (!tuple.HasValue)
		{
			return;
		}
		var (location, methodSymbol) = tuple.GetValueOrDefault();
		if (methodSymbol.IsExtensionBlockMember())
		{
			MethodSymbol methodSymbol2 = methodSymbol.TryGetCorrespondingExtensionImplementationMethod();
			if ((object)methodSymbol2 == null)
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_Call.cs", 159);
			}
			methodSymbol = methodSymbol2;
		}
		if (methodSymbol.Arity != 0)
		{
			ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance();
			method.ContainingType.GetAllTypeArgumentsNoUseSiteDiagnostics(instance);
			instance.AddRange(method.TypeArgumentsWithAnnotations);
			int count = instance.Count;
			if (count == 0)
			{
				_diagnostics.Add(ErrorCode.ERR_InterceptorCannotBeGeneric, location, methodSymbol, method);
				instance.Free();
				return;
			}
			if (methodSymbol.Arity != count)
			{
				_diagnostics.Add(ErrorCode.ERR_InterceptorArityNotCompatible, location, methodSymbol, count, method);
				instance.Free();
				return;
			}
			methodSymbol = methodSymbol.Construct(instance.ToImmutableAndFree());
			if (!methodSymbol.CheckConstraints(new ConstraintsHelper.CheckConstraintsArgs(_compilation, _compilation.Conversions, includeNullability: true, location, _diagnostics)))
			{
				return;
			}
		}
		if (method.MethodKind != MethodKind.Ordinary)
		{
			_diagnostics.Add(ErrorCode.ERR_InterceptableMethodMustBeOrdinary, location, nameSyntax.Identifier.ValueText);
			return;
		}
		MethodSymbol currentFunction = _factory.CurrentFunction;
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = GetNewCompoundUseSiteInfo();
		bool num = AccessCheck.IsSymbolAccessible(methodSymbol, currentFunction.ContainingType, ref useSiteInfo);
		_diagnostics.Add(location, useSiteInfo);
		if (!num)
		{
			_diagnostics.Add(ErrorCode.ERR_InterceptorNotAccessible, location, methodSymbol, currentFunction);
			return;
		}
		BoundExpression boundExpression = receiverOpt;
		bool wasFullyInferred = ((boundExpression == null || boundExpression is BoundTypeExpression) ? true : false);
		bool flag = !wasFullyInferred && methodSymbol.IsExtensionMethod;
		MethodSymbol methodSymbol3 = (flag ? ReducedExtensionMethodSymbol.Create(methodSymbol, receiverOpt.Type, _compilation, out wasFullyInferred) : methodSymbol);
		if (!MemberSignatureComparer.InterceptorsComparer.Equals(method, methodSymbol3))
		{
			_diagnostics.Add(ErrorCode.ERR_InterceptorSignatureMismatch, location, method, methodSymbol);
			return;
		}
		SourceMemberContainerTypeSymbol.CheckValidNullableMethodOverride(_compilation, method, methodSymbol3, _diagnostics, delegate(BindingDiagnosticBag diagnostics, MethodSymbol methodSymbol4, MethodSymbol interceptor, bool topLevel, Location attributeLocation)
		{
			diagnostics.Add(ErrorCode.WRN_NullabilityMismatchInReturnTypeOnInterceptor, attributeLocation, methodSymbol4);
		}, delegate(BindingDiagnosticBag diagnostics, MethodSymbol methodSymbol4, MethodSymbol interceptor, ParameterSymbol implementingParameter, bool blameAttributes, Location attributeLocation)
		{
			diagnostics.Add(ErrorCode.WRN_NullabilityMismatchInParameterTypeOnInterceptor, attributeLocation, new FormattedSymbol(implementingParameter, SymbolDisplayFormat.ShortFormat), methodSymbol4);
		}, location);
		if (!MemberSignatureComparer.InterceptorsStrictComparer.Equals(method, methodSymbol3))
		{
			_diagnostics.Add(ErrorCode.WRN_InterceptorSignatureMismatch, location, method, methodSymbol);
		}
		if (method.TryGetInstanceExtensionParameter(out ParameterSymbol extensionParameter))
		{
			_ = 1;
		}
		else
			method.TryGetThisParameter(out extensionParameter);
		ParameterSymbol parameterSymbol = (flag ? methodSymbol.Parameters[0] : (methodSymbol.TryGetThisParameter(out ParameterSymbol thisParameter) ? thisParameter : null));
		ParameterSymbol parameterSymbol2 = extensionParameter;
		ParameterSymbol parameterSymbol3 = parameterSymbol;
		if ((object)parameterSymbol2 == null)
		{
			if ((object)parameterSymbol3 != null)
			{
				_diagnostics.Add(ErrorCode.ERR_InterceptorMustNotHaveThisParameter, location, method);
				return;
			}
		}
		else if ((object)parameterSymbol3 == null || !extensionParameter.Type.Equals(parameterSymbol.Type, TypeCompareKind.ObliviousNullableModifierMatchesAny) || extensionParameter.RefKind != parameterSymbol.RefKind)
		{
			_diagnostics.Add(ErrorCode.ERR_InterceptorMustHaveMatchingThisParameter, location, extensionParameter, method);
			return;
		}
		if (invokedAsExtensionMethod && methodSymbol.IsStatic && !methodSymbol.IsExtensionMethod)
		{
			_diagnostics.Add(ErrorCode.ERR_InterceptorMustHaveMatchingThisParameter, location, method.Parameters[0], method);
		}
		else
		{
			if (SourceMemberContainerTypeSymbol.CheckValidScopedOverride(method, methodSymbol3, _diagnostics, delegate(BindingDiagnosticBag diagnostics, MethodSymbol methodSymbol4, MethodSymbol symbolForCompare, ParameterSymbol implementingParameter, bool blameAttributes, Location attributeLocation)
			{
				diagnostics.Add(ErrorCode.ERR_InterceptorScopedMismatch, attributeLocation, methodSymbol4, symbolForCompare);
			}, location, allowVariance: true, invokedAsExtensionMethod: false))
			{
				return;
			}
			if (flag)
			{
				receiverOpt = MakeConversionNode(receiverOpt, methodSymbol.Parameters[0].Type, @checked: false, acceptFailingConversion: false, markAsChecked: true);
				RefKind refKind = extensionParameter.RefKind;
				if (refKind != RefKind.None && !CodeGenerator.HasHome(receiverOpt, (refKind != RefKind.Ref) ? CodeGenerator.AddressKind.ReadOnlyStrict : CodeGenerator.AddressKind.Writeable, _factory.CurrentFunction, peVerifyCompatEnabled: false, null))
				{
					BoundLocal boundLocal = _factory.StoreToTemp(receiverOpt, out BoundAssignmentOperator store);
					temps.Add(boundLocal.LocalSymbol);
					receiverOpt = _factory.Sequence(ImmutableArray<LocalSymbol>.Empty, ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1] { store }), boundLocal);
				}
				arguments = arguments.Insert(0, receiverOpt);
				receiverOpt = null;
				if (argumentRefKindsOpt.IsDefault && refKind != RefKind.None)
				{
					argumentRefKindsOpt = method.Parameters.SelectAsArray((ParameterSymbol param) => param.RefKind);
				}
				if (!argumentRefKindsOpt.IsDefault)
				{
					argumentRefKindsOpt = argumentRefKindsOpt.Insert(0, refKind);
				}
			}
			method = methodSymbol;
		}
	}

	public override BoundNode VisitCall(BoundCall node)
	{
		BoundExpression boundExpression;
		if (TryGetReceiver(node, out BoundCall receiver))
		{
			ArrayBuilder<BoundCall> instance = ArrayBuilder<BoundCall>.GetInstance();
			instance.Push(node);
			node = receiver;
			BoundCall receiver2;
			while (TryGetReceiver(node, out receiver2))
			{
				instance.Push(node);
				node = receiver2;
			}
			BoundExpression rewrittenReceiver = VisitExpression(node.ReceiverOpt);
			do
			{
				boundExpression = visitArgumentsAndFinishRewrite(node, rewrittenReceiver);
				rewrittenReceiver = boundExpression;
			}
			while (instance.TryPop(out node));
			instance.Free();
		}
		else
		{
			BoundExpression rewrittenReceiver2 = VisitExpression(node.ReceiverOpt);
			boundExpression = visitArgumentsAndFinishRewrite(node, rewrittenReceiver2);
		}
		return boundExpression;
		BoundExpression visitArgumentsAndFinishRewrite(BoundCall boundCall, BoundExpression? rewrittenReceiver3)
		{
			MethodSymbol method = boundCall.Method;
			ImmutableArray<int> argsToParamsOpt = boundCall.ArgsToParamsOpt;
			ImmutableArray<RefKind> argumentRefKindsOpt = boundCall.ArgumentRefKindsOpt;
			ImmutableArray<BoundExpression> arguments = boundCall.Arguments;
			bool invokedAsExtensionMethod = boundCall.InvokedAsExtensionMethod;
			BoundExpression firstRewrittenArgument = null;
			if (rewrittenReceiver3 != null && boundCall.ReceiverOpt == null)
			{
				firstRewrittenArgument = rewrittenReceiver3;
				rewrittenReceiver3 = null;
			}
			ArrayBuilder<LocalSymbol> tempsOpt = null;
			ImmutableArray<BoundExpression> rewrittenArguments = VisitArgumentsAndCaptureReceiverIfNeeded(ref rewrittenReceiver3, forceReceiverCapturing: false, arguments, method, argsToParamsOpt, argumentRefKindsOpt, null, ref tempsOpt, firstRewrittenArgument);
			rewrittenArguments = MakeArguments(rewrittenArguments, method, boundCall.Expanded, argsToParamsOpt, ref argumentRefKindsOpt, ref tempsOpt, invokedAsExtensionMethod);
			InterceptCallAndAdjustArguments(ref method, ref rewrittenReceiver3, ref rewrittenArguments, ref argumentRefKindsOpt, ref tempsOpt, invokedAsExtensionMethod, boundCall.InterceptableNameSyntax);
			if (Instrument)
			{
				Instrumenter.InterceptCallAndAdjustArguments(ref method, ref rewrittenReceiver3, ref rewrittenArguments, ref argumentRefKindsOpt);
			}
			BoundExpression boundExpression2 = MakeCall(boundCall, boundCall.Syntax, rewrittenReceiver3, method, rewrittenArguments, argumentRefKindsOpt, boundCall.ResultKind, tempsOpt.ToImmutableAndFree());
			if (Instrument)
			{
				boundExpression2 = Instrumenter.InstrumentCall(boundCall, boundExpression2);
			}
			return boundExpression2;
		}
	}

	internal static bool TryGetReceiver(BoundCall node, [MaybeNullWhen(false)] out BoundCall receiver)
	{
		if (node.ReceiverOpt is BoundCall boundCall)
		{
			receiver = boundCall;
			return true;
		}
		if (node.InvokedAsExtensionMethod)
		{
			ImmutableArray<BoundExpression> arguments = node.Arguments;
			if (arguments.Length >= 1 && arguments[0] is BoundCall boundCall2)
			{
				receiver = boundCall2;
				return true;
			}
		}
		receiver = null;
		return false;
	}

	private BoundExpression MakeCall(BoundCall? node, SyntaxNode syntax, BoundExpression? rewrittenReceiver, MethodSymbol method, ImmutableArray<BoundExpression> rewrittenArguments, ImmutableArray<RefKind> argumentRefKinds, LookupResultKind resultKind, ImmutableArray<LocalSymbol> temps)
	{
		BoundExpression boundExpression = ((method.IsStatic && method.ContainingType.IsObjectType() && !_inExpressionLambda && (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_Object__ReferenceEquals)) ? ((BoundExpression)new BoundBinaryOperator(syntax, BinaryOperatorKind.ObjectEqual, null, null, null, resultKind, rewrittenArguments[0], rewrittenArguments[1], method.ReturnType)) : ((BoundExpression)((node != null) ? node.Update(rewrittenReceiver, ThreeState.Unknown, method, rewrittenArguments, default(ImmutableArray<string>), argumentRefKinds, node.IsDelegateCall, expanded: false, invokedAsExtensionMethod: false, default(ImmutableArray<int>), default(BitVector), node.ResultKind, method.ReturnType) : new BoundCall(syntax, rewrittenReceiver, ThreeState.Unknown, method, rewrittenArguments, default(ImmutableArray<string>), argumentRefKinds, isDelegateCall: false, expanded: false, invokedAsExtensionMethod: false, default(ImmutableArray<int>), default(BitVector), resultKind, method.ReturnType))));
		if (!temps.IsDefaultOrEmpty)
		{
			return new BoundSequence(syntax, temps, ImmutableArray<BoundExpression>.Empty, boundExpression, boundExpression.Type);
		}
		return boundExpression;
	}

	private BoundExpression MakeCall(SyntaxNode syntax, BoundExpression? rewrittenReceiver, MethodSymbol method, ImmutableArray<BoundExpression> rewrittenArguments)
	{
		return MakeCall(null, syntax, rewrittenReceiver, method, rewrittenArguments, default(ImmutableArray<RefKind>), LookupResultKind.Viable, default(ImmutableArray<LocalSymbol>));
	}

	private static bool IsSafeForReordering(BoundExpression expression, RefKind kind)
	{
		BoundExpression boundExpression = expression;
		while (!(boundExpression.ConstantValueOpt != null))
		{
			switch (boundExpression.Kind)
			{
			default:
				return false;
			case BoundKind.Local:
			case BoundKind.Parameter:
				return kind != RefKind.None;
			case BoundKind.PassByCopy:
				return IsSafeForReordering(((BoundPassByCopy)boundExpression).Expression, kind);
			case BoundKind.Conversion:
			{
				BoundConversion boundConversion = (BoundConversion)boundExpression;
				switch (boundConversion.ConversionKind)
				{
				case ConversionKind.NullLiteral:
				case ConversionKind.ImplicitConstant:
				case ConversionKind.AnonymousFunction:
				case ConversionKind.MethodGroup:
				case ConversionKind.DefaultLiteral:
					return true;
				case ConversionKind.Identity:
				case ConversionKind.ImplicitNumeric:
				case ConversionKind.ImplicitEnumeration:
				case ConversionKind.ImplicitNullable:
				case ConversionKind.ImplicitReference:
				case ConversionKind.Boxing:
				case ConversionKind.ImplicitPointerToVoid:
				case ConversionKind.ImplicitNullToPointer:
				case ConversionKind.ImplicitDynamic:
				case ConversionKind.ExplicitDynamic:
				case ConversionKind.ExplicitNumeric:
				case ConversionKind.ExplicitEnumeration:
				case ConversionKind.ExplicitNullable:
				case ConversionKind.ExplicitReference:
				case ConversionKind.Unboxing:
				case ConversionKind.ExplicitPointerToPointer:
				case ConversionKind.ExplicitIntegerToPointer:
				case ConversionKind.ExplicitPointerToInteger:
					break;
				case ConversionKind.ImplicitThrow:
				case ConversionKind.ImplicitUserDefined:
				case ConversionKind.ExplicitUserDefined:
				case ConversionKind.IntPtr:
					return false;
				default:
					return false;
				}
				boundExpression = boundConversion.Operand;
				break;
			}
			}
		}
		return true;
	}

	internal static bool IsCapturedPrimaryConstructorParameter(BoundExpression expression)
	{
		if (expression is BoundParameter boundParameter)
		{
			ParameterSymbol parameterSymbol = boundParameter.ParameterSymbol;
			if ((object)parameterSymbol != null && parameterSymbol.ContainingSymbol is SynthesizedPrimaryConstructor synthesizedPrimaryConstructor)
			{
				return synthesizedPrimaryConstructor.GetCapturedParameters().ContainsKey(parameterSymbol);
			}
		}
		return false;
	}

	private ImmutableArray<BoundExpression> VisitArgumentsAndCaptureReceiverIfNeeded([NotNullIfNotNull("rewrittenReceiver")] ref BoundExpression? rewrittenReceiver, bool forceReceiverCapturing, ImmutableArray<BoundExpression> arguments, Symbol methodOrIndexer, ImmutableArray<int> argsToParamsOpt, ImmutableArray<RefKind> argumentRefKindsOpt, ArrayBuilder<BoundExpression>? storesOpt, ref ArrayBuilder<LocalSymbol>? tempsOpt, BoundExpression? firstRewrittenArgument = null)
	{
		bool flag = methodOrIndexer.RequiresInstanceReceiver();
		if (flag)
		{
			bool flag2 = ((!(methodOrIndexer is MethodSymbol methodSymbol) || (methodSymbol.MethodKind != MethodKind.Constructor && !(methodOrIndexer is FunctionPointerMethodSymbol))) ? true : false);
			flag = flag2;
		}
		bool flag3 = flag;
		BoundLocal boundLocal = null;
		BoundAssignmentOperator store = null;
		if (forceReceiverCapturing || (flag3 && arguments.Any((BoundExpression a) => usesReceiver(a))))
		{
			RefKind refKind;
			if (methodOrIndexer.IsExtensionBlockMember())
			{
				refKind = GetExtensionBlockMemberReceiverCaptureRefKind(rewrittenReceiver, methodOrIndexer);
			}
			else if (forceReceiverCapturing)
			{
				refKind = ((rewrittenReceiver.Type.IsValueType || rewrittenReceiver.Type.Kind == SymbolKind.TypeParameter) ? RefKind.Ref : RefKind.None);
			}
			else if (rewrittenReceiver.Type.IsReferenceType)
			{
				refKind = RefKind.None;
			}
			else
			{
				refKind = rewrittenReceiver.GetRefKind();
				if (refKind == RefKind.None && CodeGenerator.HasHome(rewrittenReceiver, CodeGenerator.AddressKind.Constrained, _factory.CurrentFunction, peVerifyCompatEnabled: false, null))
				{
					refKind = RefKind.Ref;
				}
			}
			boundLocal = _factory.StoreToTemp(rewrittenReceiver, out store, (refKind == RefKind.RefReadOnlyParameter) ? RefKind.In : refKind);
			if (tempsOpt == null)
			{
				tempsOpt = ArrayBuilder<LocalSymbol>.GetInstance();
			}
			tempsOpt.Add(boundLocal.LocalSymbol);
		}
		ImmutableArray<BoundExpression> immutableArray;
		if (arguments.IsEmpty)
		{
			immutableArray = arguments;
		}
		else
		{
			BitVector argumentsAssignedToTemp = BitVector.Null;
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(arguments.Length);
			ImmutableArray<ParameterSymbol> parameters = methodOrIndexer.GetParameters();
			for (int num = 0; num < arguments.Length; num++)
			{
				BoundExpression boundExpression = arguments[num];
				if (boundExpression is BoundDiscardExpression node)
				{
					ensureTempTrackingSetup(ref tempsOpt, ref argumentsAssignedToTemp);
					instance.Add(_factory.MakeTempForDiscard(node, tempsOpt));
					argumentsAssignedToTemp[num] = true;
					continue;
				}
				ImmutableArray<BoundInterpolatedStringArgumentPlaceholder> immutableArray2 = addInterpolationPlaceholderReplacements(parameters, instance, num, boundLocal, ref tempsOpt, ref argumentsAssignedToTemp);
				instance.Add((num == 0 && firstRewrittenArgument != null) ? firstRewrittenArgument : VisitExpression(boundExpression));
				foreach (BoundInterpolatedStringArgumentPlaceholder item in immutableArray2)
				{
					if (item.ArgumentIndex != -3)
					{
						RemovePlaceholderReplacement(item);
					}
				}
			}
			immutableArray = instance.ToImmutableAndFree();
		}
		if (boundLocal != null)
		{
			BoundAssignmentOperator extraRefInitialization = null;
			if (boundLocal.LocalSymbol.IsRef && (methodOrIndexer.IsExtensionBlockMember() ? (!boundLocal.Type.IsValueType) : CodeGenerator.IsPossibleReferenceTypeReceiverOfConstrainedCall(boundLocal)) && !CodeGenerator.ReceiverIsKnownToReferToTempIfReferenceType(boundLocal) && (forceReceiverCapturing || !CodeGenerator.IsSafeToDereferenceReceiverRefAfterEvaluatingArguments(immutableArray)))
			{
				ReferToTempIfReferenceTypeReceiver(boundLocal, ref store, out extraRefInitialization, tempsOpt);
			}
			if (storesOpt != null)
			{
				if (extraRefInitialization != null)
				{
					storesOpt.Add(extraRefInitialization);
				}
				storesOpt.Add(store);
				rewrittenReceiver = boundLocal;
			}
			else
			{
				rewrittenReceiver = _factory.Sequence(ImmutableArray<LocalSymbol>.Empty, (extraRefInitialization != null) ? ImmutableArray.Create((BoundExpression)extraRefInitialization, (BoundExpression)store) : ImmutableArray.Create((BoundExpression)store), boundLocal);
			}
		}
		return immutableArray;
		ImmutableArray<BoundInterpolatedStringArgumentPlaceholder> addInterpolationPlaceholderReplacements(ImmutableArray<ParameterSymbol> immutableArray3, ArrayBuilder<BoundExpression> visitedArgumentsBuilder, int argumentIndex, BoundLocal? receiverTemp, ref ArrayBuilder<LocalSymbol>? reference, ref BitVector reference2)
		{
			BoundConversion boundConversion = arguments[argumentIndex] as BoundConversion;
			bool flag4;
			if (boundConversion != null && boundConversion.ConversionKind == ConversionKind.InterpolatedStringHandler)
			{
				BoundExpression operand = boundConversion.Operand;
				if (operand is BoundInterpolatedString || operand is BoundBinaryOperator)
				{
					flag4 = true;
					goto IL_003d;
				}
			}
			flag4 = false;
			goto IL_003d;
			IL_003d:
			if (flag4)
			{
				InterpolatedStringHandlerData interpolatedStringHandlerData = boundConversion.Operand.GetInterpolatedStringHandlerData();
				if (interpolatedStringHandlerData.ArgumentPlaceholders.Length > (interpolatedStringHandlerData.HasTrailingHandlerValidityParameter ? 1 : 0))
				{
					ensureTempTrackingSetup(ref reference, ref reference2);
					foreach (BoundInterpolatedStringArgumentPlaceholder argumentPlaceholder in interpolatedStringHandlerData.ArgumentPlaceholders)
					{
						int argumentIndex2 = argumentPlaceholder.ArgumentIndex;
						int num2 = argumentIndex2;
						BoundLocal boundLocal2;
						BoundExpression boundExpression2;
						if (num2 < 0)
						{
							if (num2 == -3)
							{
								continue;
							}
							if ((uint)(num2 - -2) > 1u)
							{
								throw ExceptionUtilities.UnexpectedValue(argumentIndex2);
							}
							boundLocal2 = receiverTemp;
						}
						else if (reference2[argumentIndex2])
						{
							boundExpression2 = visitedArgumentsBuilder[argumentIndex2];
							BoundLocal boundLocal4;
							if (boundExpression2 is BoundSequence boundSequence)
							{
								if (!(boundSequence.Value is BoundLocal boundLocal3))
								{
									goto IL_0113;
								}
								boundLocal4 = boundLocal3;
							}
							else
							{
								if (!(boundExpression2 is BoundLocal boundLocal5))
								{
									goto IL_0113;
								}
								boundLocal4 = boundLocal5;
							}
							boundLocal2 = boundLocal4;
						}
						else
						{
							int index = (argsToParamsOpt.IsDefault ? argumentIndex2 : argsToParamsOpt[argumentIndex2]);
							RefKind refKind2 = argumentRefKindsOpt.RefKinds(argumentIndex2);
							RefKind refKind3 = immutableArray3[index].RefKind;
							BoundExpression boundExpression3 = visitedArgumentsBuilder[argumentIndex2];
							SyntheticBoundNodeFactory factory = _factory;
							BoundExpression operand = boundExpression3;
							flag4 = refKind3 - 3 <= RefKind.Ref;
							boundLocal2 = factory.StoreToTemp(operand, out BoundAssignmentOperator store2, flag4 ? RefKind.In : refKind2);
							reference.Add(boundLocal2.LocalSymbol);
							visitedArgumentsBuilder[argumentIndex2] = _factory.Sequence(ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create((BoundExpression)store2), boundLocal2);
							reference2[argumentIndex2] = true;
						}
						AddPlaceholderReplacement(argumentPlaceholder, boundLocal2);
						continue;
						IL_0113:
						throw ExceptionUtilities.UnexpectedValue(boundExpression2.Kind);
					}
					return interpolatedStringHandlerData.ArgumentPlaceholders;
				}
			}
			return ImmutableArray<BoundInterpolatedStringArgumentPlaceholder>.Empty;
		}
		void ensureTempTrackingSetup([NotNull] ref ArrayBuilder<LocalSymbol>? reference, ref BitVector positionsAssignedToTemp)
		{
			if (reference == null)
			{
				reference = ArrayBuilder<LocalSymbol>.GetInstance();
			}
			if (positionsAssignedToTemp.IsNull)
			{
				positionsAssignedToTemp = BitVector.Create(arguments.Length);
			}
		}
		static bool usesReceiver(BoundExpression argument)
		{
			BoundConversion boundConversion = argument as BoundConversion;
			bool flag4;
			if (boundConversion != null && boundConversion.ConversionKind == ConversionKind.InterpolatedStringHandler)
			{
				BoundExpression operand = boundConversion.Operand;
				if (operand is BoundInterpolatedString || operand is BoundBinaryOperator)
				{
					flag4 = true;
					goto IL_0031;
				}
			}
			flag4 = false;
			goto IL_0031;
			IL_0031:
			if (flag4)
			{
				InterpolatedStringHandlerData interpolatedStringHandlerData = boundConversion.Operand.GetInterpolatedStringHandlerData();
				if (interpolatedStringHandlerData.ArgumentPlaceholders.Length > (interpolatedStringHandlerData.HasTrailingHandlerValidityParameter ? 1 : 0))
				{
					foreach (BoundInterpolatedStringArgumentPlaceholder argumentPlaceholder2 in interpolatedStringHandlerData.ArgumentPlaceholders)
					{
						int argumentIndex = argumentPlaceholder2.ArgumentIndex;
						if ((uint)(argumentIndex - -2) <= 1u)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}

	private RefKind GetExtensionBlockMemberReceiverCaptureRefKind(BoundExpression rewrittenReceiver, Symbol methodOrIndexer)
	{
		RefKind refKind = methodOrIndexer.ContainingType.ExtensionParameter.RefKind;
		bool flag = refKind == RefKind.None;
		if (rewrittenReceiver.Type.IsReferenceType || (flag && methodOrIndexer is MethodSymbol))
		{
			return RefKind.None;
		}
		if (flag)
		{
			if (CodeGenerator.HasHome(rewrittenReceiver, CodeGenerator.AddressKind.ReadOnlyStrict, _factory.CurrentFunction, peVerifyCompatEnabled: false, null))
			{
				return (RefKind)5;
			}
			return RefKind.None;
		}
		RefKind refKind2 = ExtensionMethodReferenceRewriter.ReceiverArgumentRefKindFromReceiverRefKind(refKind);
		if (CodeGenerator.HasHome(rewrittenReceiver, CodeGenerator.GetArgumentAddressKind(refKind2), _factory.CurrentFunction, peVerifyCompatEnabled: false, null))
		{
			return refKind2;
		}
		return RefKind.None;
	}

	private void ReferToTempIfReferenceTypeReceiver(BoundLocal receiverTemp, ref BoundAssignmentOperator assignmentToTemp, out BoundAssignmentOperator? extraRefInitialization, ArrayBuilder<LocalSymbol> temps)
	{
		TypeSymbol type = receiverTemp.Type;
		BoundLocal boundLocal = _factory.Local(_factory.SynthesizedLocal(type));
		temps.Add(boundLocal.LocalSymbol);
		if (!type.IsReferenceType)
		{
			BoundLocal boundLocal2 = _factory.Local(_factory.SynthesizedLocal(type, null, isPinned: false, isKnownToReferToTempIfReferenceType: false, receiverTemp.LocalSymbol.RefKind));
			temps.Add(boundLocal2.LocalSymbol);
			extraRefInitialization = assignmentToTemp.Update(boundLocal2, assignmentToTemp.Right, assignmentToTemp.IsRef, assignmentToTemp.Type);
			assignmentToTemp = assignmentToTemp.Update(assignmentToTemp.Left, new BoundComplexConditionalReceiver(receiverTemp.Syntax, boundLocal2, _factory.Sequence(new BoundExpression[1] { _factory.AssignmentExpression(boundLocal, boundLocal2) }, boundLocal), type)
			{
				WasCompilerGenerated = true
			}, assignmentToTemp.IsRef, assignmentToTemp.Type);
		}
		else
		{
			extraRefInitialization = null;
			assignmentToTemp = assignmentToTemp.Update(assignmentToTemp.Left, _factory.Sequence(new BoundExpression[1] { _factory.AssignmentExpression(boundLocal, assignmentToTemp.Right) }, boundLocal), assignmentToTemp.IsRef, assignmentToTemp.Type);
		}
		((SynthesizedLocal)receiverTemp.LocalSymbol).SetIsKnownToReferToTempIfReferenceType();
	}

	private ImmutableArray<BoundExpression> MakeArguments(ImmutableArray<BoundExpression> rewrittenArguments, Symbol methodOrIndexer, bool expanded, ImmutableArray<int> argsToParamsOpt, ref ImmutableArray<RefKind> argumentRefKindsOpt, [NotNull] ref ArrayBuilder<LocalSymbol>? temps, bool invokedAsExtensionMethod = false)
	{
		if (temps == null)
		{
			temps = ArrayBuilder<LocalSymbol>.GetInstance();
		}
		ImmutableArray<ParameterSymbol> parameters = methodOrIndexer.GetParameters();
		BoundExpression optimized;
		if (CanSkipRewriting(rewrittenArguments, methodOrIndexer, argsToParamsOpt, invokedAsExtensionMethod, ignoreComReceiver: false, out var isComReceiver))
		{
			argumentRefKindsOpt = GetEffectiveArgumentRefKinds(argumentRefKindsOpt, parameters);
			if (expanded && TryOptimizeParamsArray(rewrittenArguments[rewrittenArguments.Length - 1], out optimized))
			{
				return rewrittenArguments.SetItem(rewrittenArguments.Length - 1, optimized);
			}
			return rewrittenArguments;
		}
		BoundExpression[] array = new BoundExpression[parameters.Length];
		ArrayBuilder<BoundAssignmentOperator> instance = ArrayBuilder<BoundAssignmentOperator>.GetInstance(rewrittenArguments.Length);
		ArrayBuilder<RefKind> instance2 = ArrayBuilder<RefKind>.GetInstance(parameters.Length, RefKind.None);
		BuildStoresToTemps(expanded, argsToParamsOpt, parameters, argumentRefKindsOpt, rewrittenArguments, forceLambdaSpilling: false, array, instance2, instance);
		OptimizeTemporaries(array, instance, temps);
		instance.Free();
		if (expanded && TryOptimizeParamsArray(array[^1], out optimized))
		{
			array[^1] = optimized;
		}
		if (isComReceiver)
		{
			RewriteArgumentsForComCall(parameters, array, instance2, temps);
		}
		argumentRefKindsOpt = GetRefKindsOrNull(instance2);
		instance2.Free();
		return array.AsImmutableOrNull();
	}

	private bool TryOptimizeParamsArray(BoundExpression possibleParamsArray, [NotNullWhen(true)] out BoundExpression? optimized)
	{
		if (possibleParamsArray.IsParamsArrayOrCollection && !_inExpressionLambda)
		{
			ImmutableArray<BoundExpression> bounds = ((BoundArrayCreation)possibleParamsArray).Bounds;
			if (bounds.Length == 1 && bounds[0] is BoundLiteral boundLiteral)
			{
				ConstantValue constantValueOpt = boundLiteral.ConstantValueOpt;
				if ((object)constantValueOpt != null)
				{
					object value = constantValueOpt.Value;
					if (value is int && (int)value == 0)
					{
						optimized = CreateArrayEmptyCallIfAvailable(possibleParamsArray.Syntax, ((ArrayTypeSymbol)possibleParamsArray.Type).ElementType);
						if (optimized != null)
						{
							return true;
						}
					}
				}
			}
		}
		optimized = null;
		return false;
	}

	private static ImmutableArray<RefKind> GetEffectiveArgumentRefKinds(ImmutableArray<RefKind> argumentRefKindsOpt, ImmutableArray<ParameterSymbol> parameters)
	{
		ArrayBuilder<RefKind> refKindsBuilder = null;
		for (int i = 0; i < parameters.Length; i++)
		{
			RefKind refKind = parameters[i].RefKind;
			RefKind refKind2 = ((!argumentRefKindsOpt.IsDefault) ? argumentRefKindsOpt[i] : RefKind.None);
			RefKind effectiveRefKind = GetEffectiveRefKind(refKind, refKind2, parameters[i].Type, comRefKindMismatchPossible: false);
			if (refKind2 != effectiveRefKind)
			{
				fillRefKindsBuilder(argumentRefKindsOpt, parameters, ref refKindsBuilder);
				refKindsBuilder[i] = effectiveRefKind;
			}
		}
		if (refKindsBuilder != null)
		{
			argumentRefKindsOpt = refKindsBuilder.ToImmutableAndFree();
		}
		return argumentRefKindsOpt;
		static void fillRefKindsBuilder(ImmutableArray<RefKind> items, ImmutableArray<ParameterSymbol> immutableArray, [NotNull] ref ArrayBuilder<RefKind>? reference)
		{
			if (reference == null)
			{
				if (!items.IsDefault)
				{
					reference = ArrayBuilder<RefKind>.GetInstance(immutableArray.Length);
					reference.AddRange(items);
				}
				else
				{
					reference = ArrayBuilder<RefKind>.GetInstance(immutableArray.Length, RefKind.None);
				}
			}
		}
	}

	internal static RefKind GetEffectiveRefKind(RefKind paramRefKind, RefKind initialArgRefKind, TypeSymbol paramType, bool comRefKindMismatchPossible)
	{
		if (paramRefKind - 3 <= RefKind.Ref)
		{
			if (initialArgRefKind != RefKind.None)
			{
				return (RefKind)5;
			}
			return RefKind.In;
		}
		if (paramRefKind == RefKind.Ref && initialArgRefKind == RefKind.None && paramType is NamedTypeSymbol { IsInterpolatedStringHandlerType: not false, IsValueType: not false })
		{
			return RefKind.Ref;
		}
		return initialArgRefKind;
	}

	internal static bool CanSkipRewriting(ImmutableArray<BoundExpression> rewrittenArguments, Symbol methodOrIndexer, ImmutableArray<int> argsToParamsOpt, bool invokedAsExtensionMethod, bool ignoreComReceiver, out bool isComReceiver)
	{
		isComReceiver = false;
		if (methodOrIndexer.GetIsVararg())
		{
			return true;
		}
		if (!ignoreComReceiver)
		{
			isComReceiver = tryGetReceiverNamedType(methodOrIndexer, invokedAsExtensionMethod)?.IsComImport ?? false;
		}
		if (rewrittenArguments.Length == methodOrIndexer.GetParameterCount() && argsToParamsOpt.IsDefault)
		{
			return !isComReceiver;
		}
		return false;
		static NamedTypeSymbol? tryGetReceiverNamedType(Symbol symbol, bool flag)
		{
			if (flag)
			{
				return ((MethodSymbol)symbol).Parameters[0].Type as NamedTypeSymbol;
			}
			if (symbol.IsExtensionBlockMember())
			{
				return symbol.ContainingType.ExtensionParameter.Type as NamedTypeSymbol;
			}
			return symbol.ContainingType;
		}
	}

	private static ImmutableArray<RefKind> GetRefKindsOrNull(ArrayBuilder<RefKind> refKinds)
	{
		foreach (RefKind refKind in refKinds)
		{
			if (refKind != RefKind.None)
			{
				return refKinds.ToImmutable();
			}
		}
		return default(ImmutableArray<RefKind>);
	}

	private static BoundExpression RewriteParamsArray<TArg>(BoundExpression paramsArray, ParamsArrayElementRewriter<TArg> elementRewriter, ref TArg arg)
	{
		if (paramsArray is BoundArrayCreation boundArrayCreation)
		{
			ImmutableArray<BoundExpression> bounds = boundArrayCreation.Bounds;
			if (bounds.Length == 1 && bounds[0] is BoundLiteral)
			{
				BoundArrayInitialization initializerOpt = boundArrayCreation.InitializerOpt;
				if (initializerOpt != null)
				{
					ImmutableArray<BoundExpression> initializers = initializerOpt.Initializers;
					ArrayBuilder<BoundExpression> arrayBuilder = null;
					for (int i = 0; i < initializers.Length; i++)
					{
						BoundExpression boundExpression = initializers[i];
						BoundExpression boundExpression2 = elementRewriter(boundExpression, ref arg);
						if (boundExpression != boundExpression2)
						{
							if (arrayBuilder == null)
							{
								arrayBuilder = ArrayBuilder<BoundExpression>.GetInstance(initializers.Length);
								arrayBuilder.AddRange(initializers, i);
							}
							arrayBuilder.Add(boundExpression2);
						}
						else
						{
							arrayBuilder?.Add(boundExpression2);
						}
					}
					if (arrayBuilder != null)
					{
						return boundArrayCreation.Update(bounds, initializerOpt.Update(arrayBuilder.ToImmutableAndFree()), boundArrayCreation.Type);
					}
					return boundArrayCreation;
				}
			}
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_Call.cs", 1393);
	}

	private void BuildStoresToTemps(bool expanded, ImmutableArray<int> argsToParamsOpt, ImmutableArray<ParameterSymbol> parameters, ImmutableArray<RefKind> argumentRefKinds, ImmutableArray<BoundExpression> rewrittenArguments, bool forceLambdaSpilling, BoundExpression[] arguments, ArrayBuilder<RefKind> refKinds, ArrayBuilder<BoundAssignmentOperator> storesToTemps)
	{
		for (int i = 0; i < rewrittenArguments.Length; i++)
		{
			BoundExpression boundExpression = rewrittenArguments[i];
			int num = ((!argsToParamsOpt.IsDefault) ? argsToParamsOpt[i] : i);
			RefKind refKind = argumentRefKinds.RefKinds(i);
			RefKind refKind2 = parameters[num].RefKind;
			if (boundExpression.IsParamsArrayOrCollection)
			{
				refKinds[num] = refKind;
				if (i == rewrittenArguments.Length - 1)
				{
					arguments[num] = boundExpression;
					continue;
				}
				(LocalRewriter, bool, ArrayBuilder<BoundAssignmentOperator>) arg = (this, forceLambdaSpilling, storesToTemps);
				arguments[num] = RewriteParamsArray<(LocalRewriter, bool, ArrayBuilder<BoundAssignmentOperator>)>(boundExpression, delegate(BoundExpression element, ref (LocalRewriter rewriter, bool forceLambdaSpilling, ArrayBuilder<BoundAssignmentOperator> storesToTemps) reference)
				{
					return reference.rewriter.StoreArgumentToTempIfNecessary(reference.forceLambdaSpilling, reference.storesToTemps, element, RefKind.None, RefKind.None);
				}, ref arg);
			}
			else
			{
				arguments[num] = StoreArgumentToTempIfNecessary(forceLambdaSpilling, storesToTemps, boundExpression, refKind, refKind2);
				refKinds[num] = GetEffectiveRefKind(refKind2, refKind, parameters[num].Type, comRefKindMismatchPossible: true);
			}
		}
	}

	private BoundExpression StoreArgumentToTempIfNecessary(bool forceLambdaSpilling, ArrayBuilder<BoundAssignmentOperator> storesToTemps, BoundExpression argument, RefKind argRefKind, RefKind paramRefKind)
	{
		if ((!forceLambdaSpilling || !isLambdaConversion(argument)) && IsSafeForReordering(argument, argRefKind))
		{
			return argument;
		}
		SyntheticBoundNodeFactory factory = _factory;
		bool flag = paramRefKind - 3 <= RefKind.Ref;
		BoundLocal result = factory.StoreToTemp(argument, out BoundAssignmentOperator store, (!flag) ? argRefKind : ((argRefKind == RefKind.None) ? RefKind.In : ((RefKind)5)));
		storesToTemps.Add(store);
		return result;
		static bool isLambdaConversion(BoundExpression expr)
		{
			if (expr is BoundConversion boundConversion)
			{
				return boundConversion.ConversionKind == ConversionKind.AnonymousFunction;
			}
			return false;
		}
	}

	private BoundExpression CreateEmptyArray(SyntaxNode syntax, ArrayTypeSymbol arrayType)
	{
		BoundExpression boundExpression = CreateArrayEmptyCallIfAvailable(syntax, arrayType.ElementType);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		return new BoundArrayCreation(syntax, ImmutableArray.Create((BoundExpression)new BoundLiteral(syntax, ConstantValue.Create(0), _compilation.GetSpecialType(SpecialType.System_Int32))), null, arrayType)
		{
			WasCompilerGenerated = true
		};
	}

	private BoundExpression? CreateArrayEmptyCallIfAvailable(SyntaxNode syntax, TypeSymbol elementType)
	{
		if (elementType.IsPointerOrFunctionPointer())
		{
			return null;
		}
		if (!(_compilation.GetSpecialTypeMember(SpecialMember.System_Array__Empty) is MethodSymbol methodSymbol))
		{
			return null;
		}
		_diagnostics.ReportUseSite(methodSymbol, syntax);
		MethodSymbol methodSymbol2 = methodSymbol.Construct(ImmutableArray.Create(elementType));
		return new BoundCall(syntax, null, ThreeState.Unknown, methodSymbol2, ImmutableArray<BoundExpression>.Empty, default(ImmutableArray<string>), default(ImmutableArray<RefKind>), isDelegateCall: false, expanded: false, invokedAsExtensionMethod: false, default(ImmutableArray<int>), default(BitVector), LookupResultKind.Viable, methodSymbol2.ReturnType);
	}

	private static void OptimizeTemporaries(BoundExpression[] arguments, ArrayBuilder<BoundAssignmentOperator> storesToTemps, ArrayBuilder<LocalSymbol> temporariesBuilder)
	{
		if (storesToTemps.Count <= 0 || MergeArgumentsAndSideEffects(arguments, storesToTemps) <= 0)
		{
			return;
		}
		foreach (BoundAssignmentOperator storesToTemp in storesToTemps)
		{
			if (storesToTemp != null)
			{
				temporariesBuilder.Add(((BoundLocal)storesToTemp.Left).LocalSymbol);
			}
		}
	}

	private static int MergeArgumentsAndSideEffects(BoundExpression[] arguments, ArrayBuilder<BoundAssignmentOperator> tempStores)
	{
		int tempsRemainedInUse = tempStores.Count;
		int firstUnclaimedStore = 0;
		for (int i = 0; i < arguments.Length; i++)
		{
			BoundExpression boundExpression = arguments[i];
			if (boundExpression.IsParamsArrayOrCollection)
			{
				(ArrayBuilder<BoundAssignmentOperator>, int, int) arg = (tempStores, tempsRemainedInUse, firstUnclaimedStore);
				arguments[i] = RewriteParamsArray<(ArrayBuilder<BoundAssignmentOperator>, int, int)>(boundExpression, delegate(BoundExpression element, ref (ArrayBuilder<BoundAssignmentOperator> tempStores, int tempsRemainedInUse, int firstUnclaimedStore) reference)
				{
					return mergeArgumentAndSideEffect(element, reference.tempStores, ref reference.tempsRemainedInUse, ref reference.firstUnclaimedStore);
				}, ref arg);
				tempsRemainedInUse = arg.Item2;
				firstUnclaimedStore = arg.Item3;
			}
			else
			{
				arguments[i] = mergeArgumentAndSideEffect(boundExpression, tempStores, ref tempsRemainedInUse, ref firstUnclaimedStore);
			}
		}
		return tempsRemainedInUse;
		static BoundExpression mergeArgumentAndSideEffect(BoundExpression argument, ArrayBuilder<BoundAssignmentOperator> arrayBuilder, ref int reference2, ref int reference)
		{
			if (argument.Kind == BoundKind.Local)
			{
				int num = -1;
				for (int j = reference; j < arrayBuilder.Count; j++)
				{
					if (arrayBuilder[j].Left == argument)
					{
						num = j;
						break;
					}
				}
				if (num != -1)
				{
					BoundExpression right = arrayBuilder[num].Right;
					arrayBuilder[num] = null;
					reference2--;
					if (num == reference)
					{
						argument = right;
					}
					else
					{
						BoundExpression[] array = new BoundExpression[num - reference];
						for (int k = 0; k < array.Length; k++)
						{
							array[k] = arrayBuilder[reference + k];
						}
						argument = new BoundSequence(right.Syntax, ImmutableArray<LocalSymbol>.Empty, array.AsImmutableOrNull(), right, right.Type);
					}
					reference = num + 1;
				}
			}
			return argument;
		}
	}

	private void RewriteArgumentsForComCall(ImmutableArray<ParameterSymbol> parameters, BoundExpression[] actualArguments, ArrayBuilder<RefKind> argsRefKindsBuilder, ArrayBuilder<LocalSymbol> temporariesBuilder)
	{
		int num = actualArguments.Length;
		for (int i = 0; i < num; i++)
		{
			RefKind refKind = parameters[i].RefKind;
			if (argsRefKindsBuilder[i] == RefKind.None && refKind == RefKind.Ref)
			{
				BoundExpression boundExpression = actualArguments[i];
				if (boundExpression.Kind != BoundKind.Local || ((BoundLocal)boundExpression).LocalSymbol.RefKind != RefKind.Ref)
				{
					BoundLocal boundLocal = _factory.StoreToTemp(boundExpression, out BoundAssignmentOperator store);
					actualArguments[i] = new BoundSequence(boundExpression.Syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create((BoundExpression)store), boundLocal, boundLocal.Type);
					argsRefKindsBuilder[i] = RefKind.Ref;
					temporariesBuilder.Add(boundLocal.LocalSymbol);
				}
			}
		}
	}

	public override BoundNode VisitDynamicMemberAccess(BoundDynamicMemberAccess node)
	{
		if (node.Invoked)
		{
			return node;
		}
		BoundExpression loweredReceiver = VisitExpression(node.Receiver);
		return _dynamicFactory.MakeDynamicGetMember(loweredReceiver, node.Name, node.Indexed).ToExpression();
	}

	public override BoundNode? VisitCollectionExpression(BoundCollectionExpression node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_CollectionExpression.cs", 21);
	}

	public override BoundNode? VisitUnconvertedCollectionExpression(BoundUnconvertedCollectionExpression node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_CollectionExpression.cs", 26);
	}

	private BoundExpression RewriteCollectionExpressionConversion(Conversion conversion, BoundCollectionExpression node)
	{
		SyntaxNode syntax = _factory.Syntax;
		_factory.Syntax = node.Syntax;
		try
		{
			CollectionExpressionTypeKind collectionExpressionTypeKind = conversion.GetCollectionExpressionTypeKind(out TypeSymbol _, out MethodSymbol _, out bool _);
			switch (collectionExpressionTypeKind)
			{
			case CollectionExpressionTypeKind.ImplementsIEnumerable:
			{
				if (ConversionsBase.IsSpanOrListType(_compilation, node.Type, WellKnownType.System_Collections_Generic_List_T, out var elementType2))
				{
					if (TryRewriteSingleElementSpreadToList(node, elementType2, out BoundExpression result))
					{
						return result;
					}
					if (useListOptimization(_compilation, node))
					{
						return CreateAndPopulateList(node, elementType2, node.Elements.SelectAsArray((BoundNode element, BoundCollectionExpression node2) => unwrapListElement(node2, element), node));
					}
				}
				return VisitCollectionInitializerCollectionExpression(node, node.Type);
			}
			case CollectionExpressionTypeKind.Array:
			case CollectionExpressionTypeKind.Span:
			case CollectionExpressionTypeKind.ReadOnlySpan:
				return VisitArrayOrSpanCollectionExpression(node, node.Type);
			case CollectionExpressionTypeKind.CollectionBuilder:
				if ((object)node.Type.OriginalDefinition == _compilation.GetWellKnownType(WellKnownType.System_Collections_Immutable_ImmutableArray_T))
				{
					return VisitArrayOrSpanCollectionExpression(node, node.Type);
				}
				return VisitCollectionBuilderCollectionExpression(node);
			case CollectionExpressionTypeKind.ArrayInterface:
				return VisitListInterfaceCollectionExpression(node);
			default:
				throw ExceptionUtilities.UnexpectedValue(collectionExpressionTypeKind);
			}
		}
		finally
		{
			_factory.Syntax = syntax;
		}
		static bool canOptimizeListElement(BoundNode element, MethodSymbol addMethod)
		{
			BoundExpression boundExpression = ((!(element is BoundCollectionExpressionSpreadElement boundCollectionExpressionSpreadElement)) ? ((BoundExpression)element) : ((BoundExpressionStatement)boundCollectionExpressionSpreadElement.IteratorBody).Expression);
			if (boundExpression is BoundCollectionElementInitializer boundCollectionElementInitializer)
			{
				return addMethod.Equals(boundCollectionElementInitializer.AddMethod.OriginalDefinition);
			}
			return false;
		}
		static BoundNode unwrapListElement(BoundCollectionExpression expr, BoundNode element)
		{
			if (element is BoundCollectionExpressionSpreadElement boundCollectionExpressionSpreadElement)
			{
				BoundExpression underlyingCollectionExpressionElement = Binder.GetUnderlyingCollectionExpressionElement(expr, ((BoundExpressionStatement)boundCollectionExpressionSpreadElement.IteratorBody).Expression, throwOnErrors: true);
				return boundCollectionExpressionSpreadElement.Update(boundCollectionExpressionSpreadElement.Expression, boundCollectionExpressionSpreadElement.ExpressionPlaceholder, boundCollectionExpressionSpreadElement.Conversion, boundCollectionExpressionSpreadElement.EnumeratorInfoOpt, boundCollectionExpressionSpreadElement.LengthOrCount, boundCollectionExpressionSpreadElement.ElementPlaceholder, new BoundExpressionStatement(underlyingCollectionExpressionElement.Syntax, underlyingCollectionExpressionElement));
			}
			return Binder.GetUnderlyingCollectionExpressionElement(expr, (BoundExpression)element, throwOnErrors: true);
		}
		static bool useListOptimization(CSharpCompilation compilation, BoundCollectionExpression boundCollectionExpression)
		{
			ImmutableArray<BoundNode> elements = boundCollectionExpression.Elements;
			if (elements.Length == 0)
			{
				return true;
			}
			MethodSymbol methodSymbol = (MethodSymbol)compilation.GetWellKnownTypeMember(WellKnownMember.System_Collections_Generic_List_T__Add);
			if ((object)methodSymbol == null)
			{
				return false;
			}
			return elements.All(canOptimizeListElement, methodSymbol);
		}
	}

	private bool TryRewriteSingleElementSpreadToList(BoundCollectionExpression node, TypeWithAnnotations listElementType, [NotNullWhen(true)] out BoundExpression? result)
	{
		result = null;
		ImmutableArray<BoundNode> elements = node.Elements;
		if (elements.Length != 1 || !(elements[0] is BoundCollectionExpressionSpreadElement boundCollectionExpressionSpreadElement))
		{
			return false;
		}
		if (!TryGetWellKnownTypeMember<MethodSymbol>(node.Syntax, WellKnownMember.System_Linq_Enumerable__ToList, out MethodSymbol symbol, isOptional: true))
		{
			return false;
		}
		MethodSymbol methodSymbol = symbol.Construct(ImmutableCollectionsMarshal.AsImmutableArray(new TypeWithAnnotations[1] { listElementType }));
		if (!ShouldUseIEnumerableBulkAddMethod(boundCollectionExpressionSpreadElement.Expression.Type, methodSymbol.Parameters[0].Type, boundCollectionExpressionSpreadElement.EnumeratorInfoOpt?.GetEnumeratorInfo.Method))
		{
			return false;
		}
		BoundExpression arg = VisitExpression(boundCollectionExpressionSpreadElement.Expression);
		result = _factory.Call(null, methodSymbol, arg);
		return true;
	}

	private bool ShouldUseIEnumerableBulkAddMethod(TypeSymbol spreadType, TypeSymbol targetEnumerableType, MethodSymbol? getEnumeratorMethod)
	{
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
		ConversionKind kind;
		if ((object)getEnumeratorMethod != null && getEnumeratorMethod.ReturnType.IsValueType)
		{
			NamedTypeSymbol destination = _compilation.GetSpecialType(SpecialType.System_Collections_Generic_ICollection_T).Construct(((NamedTypeSymbol)targetEnumerableType).TypeArgumentsWithAnnotationsNoUseSiteDiagnostics);
			kind = _compilation.Conversions.ClassifyBuiltInConversion(spreadType, destination, isChecked: false, ref useSiteInfo).Kind;
			if ((kind != ConversionKind.Identity && kind != ConversionKind.ImplicitReference) || 1 == 0)
			{
				return false;
			}
		}
		kind = _compilation.Conversions.ClassifyImplicitConversionFromType(spreadType, targetEnumerableType, ref useSiteInfo).Kind;
		if (kind == ConversionKind.Identity || kind == ConversionKind.ImplicitReference)
		{
			return true;
		}
		return false;
	}

	private static bool CanOptimizeSingleSpreadAsCollectionBuilderArgument(BoundCollectionExpression node, [NotNullWhen(true)] out BoundExpression? spreadExpression)
	{
		spreadExpression = null;
		if (node != null)
		{
			MethodSymbol collectionBuilderMethod = node.CollectionBuilderMethod;
			if ((object)collectionBuilderMethod != null)
			{
				ImmutableArray<BoundNode> elements = node.Elements;
				if (elements.Length == 1 && elements[0] is BoundCollectionExpressionSpreadElement boundCollectionExpressionSpreadElement)
				{
					BoundExpression expression = boundCollectionExpressionSpreadElement.Expression;
					if (expression != null && expression.Type is NamedTypeSymbol type && ConversionsBase.HasIdentityConversion(collectionBuilderMethod.Parameters[0].Type, type) && (!collectionBuilderMethod.ReturnType.IsRefLikeType || collectionBuilderMethod.Parameters[0].EffectiveScope == ScopedKind.ScopedValue))
					{
						spreadExpression = expression;
					}
				}
			}
		}
		return spreadExpression != null;
	}

	private BoundExpression VisitArrayOrSpanCollectionExpression(BoundCollectionExpression node, TypeSymbol collectionType)
	{
		if (collectionType is ArrayTypeSymbol arrayType)
		{
			return createArray(node, arrayType, targetsReadOnlyCollection: false);
		}
		NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)collectionType;
		if (namedTypeSymbol.OriginalDefinition.Equals(_compilation.GetWellKnownType(WellKnownType.System_Collections_Immutable_ImmutableArray_T)))
		{
			return createImmutableArray(node, namedTypeSymbol);
		}
		return createSpan(node, namedTypeSymbol, namedTypeSymbol.OriginalDefinition.Equals(_compilation.GetWellKnownType(WellKnownType.System_ReadOnlySpan_T)));
		BoundExpression createArray(BoundCollectionExpression boundCollectionExpression, ArrayTypeSymbol arrayTypeSymbol, bool targetsReadOnlyCollection)
		{
			BoundExpression boundExpression = TryOptimizeSingleSpreadToArray_NoConversionApplied(boundCollectionExpression, targetsReadOnlyCollection, arrayTypeSymbol);
			if (boundExpression != null)
			{
				return boundExpression;
			}
			if (ShouldUseKnownLength(boundCollectionExpression, out var _))
			{
				return CreateAndPopulateArray(boundCollectionExpression, arrayTypeSymbol);
			}
			BoundExpression boundExpression2 = CreateAndPopulateList(boundCollectionExpression, arrayTypeSymbol.ElementTypeWithAnnotations, boundCollectionExpression.Elements);
			MethodSymbol method = ((MethodSymbol)_factory.WellKnownMember(WellKnownMember.System_Collections_Generic_List_T__ToArray)).AsMember((NamedTypeSymbol)boundExpression2.Type);
			return _factory.Call(boundExpression2, method);
		}
		BoundExpression createImmutableArray(BoundCollectionExpression boundCollectionExpression, NamedTypeSymbol immutableArrayType)
		{
			if (boundCollectionExpression.Elements.IsEmpty && _factory.WellKnownMember(WellKnownMember.System_Collections_Immutable_ImmutableArray_T__Empty, isOptional: true) is FieldSymbol fieldSymbol)
			{
				FieldSymbol f = fieldSymbol.AsMember(immutableArrayType);
				return _factory.Field(null, f);
			}
			if (CanOptimizeSingleSpreadAsCollectionBuilderArgument(boundCollectionExpression, out BoundExpression _))
			{
				return VisitCollectionBuilderCollectionExpression(boundCollectionExpression);
			}
			MethodSymbol methodSymbol = _factory.WellKnownMethod(WellKnownMember.System_Runtime_InteropServices_ImmutableCollectionsMarshal__AsImmutableArray_T, isOptional: true);
			if ((object)methodSymbol != null)
			{
				ArrayTypeSymbol arrayTypeSymbol = getBackingArrayType(immutableArrayType);
				BoundExpression boundExpression = createArray(boundCollectionExpression, arrayTypeSymbol, targetsReadOnlyCollection: true);
				return _factory.StaticCall(methodSymbol.Construct(ImmutableCollectionsMarshal.AsImmutableArray(new TypeWithAnnotations[1] { arrayTypeSymbol.ElementTypeWithAnnotations })), ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1] { boundExpression }));
			}
			return VisitCollectionBuilderCollectionExpression(boundCollectionExpression);
		}
		BoundExpression createSpan(BoundCollectionExpression node2, NamedTypeSymbol spanType, bool isReadOnlySpan)
		{
			BoundExpression boundExpression = tryCreateNonArrayBackedSpan(node2, spanType, isReadOnlySpan);
			if (boundExpression != null)
			{
				return boundExpression;
			}
			ArrayTypeSymbol arrayType2 = getBackingArrayType(spanType);
			BoundExpression boundExpression2 = createArray(node2, arrayType2, isReadOnlySpan);
			WellKnownMember wm = (isReadOnlySpan ? WellKnownMember.System_ReadOnlySpan_T__ctor_Array : WellKnownMember.System_Span_T__ctor_Array);
			MethodSymbol ctor = _factory.WellKnownMethod(wm).AsMember(spanType);
			return _factory.New(ctor, boundExpression2);
		}
		ArrayTypeSymbol getBackingArrayType(NamedTypeSymbol namedTypeSymbol2)
		{
			TypeWithAnnotations elementType = namedTypeSymbol2.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[0];
			return ArrayTypeSymbol.CreateSZArray(_compilation.Assembly, elementType);
		}
		BoundExpression? tryCreateNonArrayBackedSpan(BoundCollectionExpression boundCollectionExpression, NamedTypeSymbol spanType, bool isReadOnlySpan)
		{
			TypeWithAnnotations elementType = spanType.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[0];
			ImmutableArray<BoundNode> elements = boundCollectionExpression.Elements;
			if (elements.Length == 0)
			{
				return _factory.Default(spanType);
			}
			if (isReadOnlySpan && ShouldUseRuntimeHelpersCreateSpan(boundCollectionExpression, elementType.Type))
			{
				MethodSymbol ctor = ((MethodSymbol)_factory.WellKnownMember(WellKnownMember.System_ReadOnlySpan_T__ctor_Array)).AsMember(spanType);
				ImmutableArray<BoundExpression> elements2 = elements.SelectAsArray((BoundNode element, LocalRewriter rewriter) => rewriter.VisitExpression((BoundExpression)element), this);
				return _factory.New(ctor, _factory.Array(elementType.Type, elements2));
			}
			if (ShouldUseInlineArray(boundCollectionExpression, _compilation) && _additionalLocals != null)
			{
				return CreateAndPopulateSpanFromInlineArray(boundCollectionExpression.Syntax, elementType, elements, isReadOnlySpan);
			}
			return null;
		}
	}

	private BoundExpression VisitCollectionInitializerCollectionExpression(BoundCollectionExpression node, TypeSymbol collectionType)
	{
		ImmutableArray<BoundNode> elements = node.Elements;
		BoundExpression argument = VisitExpression(node.CollectionCreation);
		BoundLocal temp = _factory.StoreToTemp(argument, out BoundAssignmentOperator store);
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(elements.Length + 1);
		instance.Add(store);
		BoundObjectOrCollectionValuePlaceholder placeholder = node.Placeholder;
		AddPlaceholderReplacement(placeholder, temp);
		foreach (BoundNode item in elements)
		{
			BoundExpression boundExpression = ((item is BoundCollectionExpressionSpreadElement boundCollectionExpressionSpreadElement) ? MakeCollectionExpressionSpreadElement(boundCollectionExpressionSpreadElement, VisitExpression(boundCollectionExpressionSpreadElement.Expression), delegate(BoundStatement iteratorBody)
			{
				SyntaxNode syntax = iteratorBody.Syntax;
				BoundExpression boundExpression2 = rewriteCollectionInitializer(temp, ((BoundExpressionStatement)iteratorBody).Expression);
				return (boundExpression2 == null) ? ((BoundStatement)new BoundNoOpStatement(syntax, NoOpStatementFlavor.Default)) : ((BoundStatement)new BoundExpressionStatement(syntax, boundExpression2));
			}) : rewriteCollectionInitializer(temp, (BoundExpression)item));
			if (boundExpression != null)
			{
				instance.Add(boundExpression);
			}
		}
		RemovePlaceholderReplacement(placeholder);
		return new BoundSequence(node.Syntax, ImmutableArray.Create(temp.LocalSymbol), instance.ToImmutableAndFree(), temp, collectionType);
		BoundExpression? rewriteCollectionInitializer(BoundLocal rewrittenReceiver, BoundExpression expressionElement)
		{
			if (expressionElement is BoundCollectionElementInitializer initializer)
			{
				return MakeCollectionInitializer(initializer);
			}
			if (!(expressionElement is BoundDynamicCollectionElementInitializer initializer2))
			{
				throw ExceptionUtilities.UnexpectedValue(expressionElement);
			}
			return MakeDynamicCollectionInitializer(rewrittenReceiver, initializer2);
		}
	}

	private BoundExpression VisitListInterfaceCollectionExpression(BoundCollectionExpression node)
	{
		SyntaxNode syntax = node.Syntax;
		NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)node.Type;
		TypeWithAnnotations typeWithAnnotations = namedTypeSymbol.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics.Single();
		ImmutableArray<BoundNode> elements = node.Elements;
		SpecialType specialType = namedTypeSymbol.OriginalDefinition.SpecialType;
		BoundExpression arg;
		if ((specialType == SpecialType.System_Collections_Generic_IEnumerable_T || (uint)(specialType - 30) <= 1u) ? true : false)
		{
			bool flag = ShouldUseKnownLength(node, out var numberIncludingLastSpread);
			if (elements.Length == 0)
			{
				arg = CreateEmptyArray(syntax, ArrayTypeSymbol.CreateSZArray(_compilation.Assembly, typeWithAnnotations));
			}
			else
			{
				ImmutableArray<TypeWithAnnotations> typeArguments = ImmutableArray.Create(typeWithAnnotations);
				SynthesizedReadOnlyListKind synthesizedReadOnlyListKind = ((!flag) ? SynthesizedReadOnlyListKind.List : ((numberIncludingLastSpread != 0 || elements.Length != 1 || !SynthesizedReadOnlyListTypeSymbol.CanCreateSingleElement(_compilation)) ? SynthesizedReadOnlyListKind.Array : SynthesizedReadOnlyListKind.SingleElement));
				NamedTypeSymbol namedTypeSymbol2 = _factory.ModuleBuilderOpt.EnsureReadOnlyListTypeExists(syntax, synthesizedReadOnlyListKind, _diagnostics.DiagnosticBag).Construct(typeArguments);
				if (namedTypeSymbol2.IsErrorType())
				{
					return BadExpression(node);
				}
				BoundExpression boundExpression = synthesizedReadOnlyListKind switch
				{
					SynthesizedReadOnlyListKind.SingleElement => VisitExpression((BoundExpression)elements.Single()), 
					SynthesizedReadOnlyListKind.Array => createArray(node, ArrayTypeSymbol.CreateSZArray(_compilation.Assembly, typeWithAnnotations)), 
					SynthesizedReadOnlyListKind.List => CreateAndPopulateList(node, typeWithAnnotations, elements), 
					_ => throw ExceptionUtilities.UnexpectedValue(synthesizedReadOnlyListKind), 
				};
				arg = new BoundObjectCreationExpression(syntax, namedTypeSymbol2.Constructors.Single(), boundExpression)
				{
					WasCompilerGenerated = true
				};
			}
		}
		else
		{
			arg = CreateAndPopulateList(node, typeWithAnnotations, elements);
		}
		Conversion conversion = _factory.ClassifyEmitConversion(arg, namedTypeSymbol);
		return _factory.Convert(namedTypeSymbol, arg, conversion);
		BoundExpression createArray(BoundCollectionExpression node2, ArrayTypeSymbol arrayType)
		{
			BoundExpression boundExpression2 = TryOptimizeSingleSpreadToArray_NoConversionApplied(node2, targetsReadOnlyCollection: true, arrayType);
			if (boundExpression2 != null)
			{
				return boundExpression2;
			}
			return CreateAndPopulateArray(node2, arrayType);
		}
	}

	private BoundExpression VisitCollectionBuilderCollectionExpression(BoundCollectionExpression node)
	{
		MethodSymbol collectionBuilderMethod = node.CollectionBuilderMethod;
		NamedTypeSymbol collectionType = (NamedTypeSymbol)collectionBuilderMethod.Parameters[0].Type;
		BoundExpression item = (CanOptimizeSingleSpreadAsCollectionBuilderArgument(node, out BoundExpression spreadExpression) ? VisitExpression(spreadExpression) : VisitArrayOrSpanCollectionExpression(node, collectionType));
		BoundCall value = new BoundCall(node.Syntax, null, ThreeState.Unknown, collectionBuilderMethod, ImmutableArray.Create(item), default(ImmutableArray<string>), default(ImmutableArray<RefKind>), isDelegateCall: false, expanded: false, invokedAsExtensionMethod: false, default(ImmutableArray<int>), default(BitVector), LookupResultKind.Viable, collectionBuilderMethod.ReturnType);
		BoundValuePlaceholder collectionBuilderInvocationPlaceholder = node.CollectionBuilderInvocationPlaceholder;
		AddPlaceholderReplacement(collectionBuilderInvocationPlaceholder, value);
		BoundExpression? result = VisitExpression(node.CollectionBuilderInvocationConversion);
		RemovePlaceholderReplacement(collectionBuilderInvocationPlaceholder);
		return result;
	}

	internal static bool IsAllocatingRefStructCollectionExpression(BoundCollectionExpressionBase node, CollectionExpressionTypeKind collectionKind, TypeSymbol? elementType, CSharpCompilation compilation)
	{
		bool flag = (uint)(collectionKind - 2) <= 1u;
		if (flag && node.Elements.Length > 0 && (object)elementType != null && (collectionKind != CollectionExpressionTypeKind.ReadOnlySpan || !ShouldUseRuntimeHelpersCreateSpan(node, elementType)))
		{
			return !ShouldUseInlineArray(node, compilation);
		}
		return false;
	}

	internal static bool ShouldUseRuntimeHelpersCreateSpan(BoundCollectionExpressionBase node, TypeSymbol elementType)
	{
		if (!node.HasSpreadElements(out var _, out var _) && node.Elements.Length > 0 && CodeGenerator.IsTypeAllowedInBlobWrapper(elementType.EnumUnderlyingTypeOrSelf().SpecialType))
		{
			return node.Elements.All((BoundNode e) => (object)((BoundExpression)e).ConstantValueOpt != null);
		}
		return false;
	}

	private static bool ShouldUseInlineArray(BoundCollectionExpressionBase node, CSharpCompilation compilation)
	{
		if (!node.HasSpreadElements(out var _, out var _) && node.Elements.Length > 0)
		{
			return compilation.Assembly.RuntimeSupportsInlineArrayTypes;
		}
		return false;
	}

	private BoundExpression CreateAndPopulateSpanFromInlineArray(SyntaxNode syntax, TypeWithAnnotations elementType, ImmutableArray<BoundNode> elements, bool asReadOnlySpan)
	{
		int length = elements.Length;
		if (length == 1 && _factory.WellKnownMember(asReadOnlySpan ? WellKnownMember.System_ReadOnlySpan_T__ctor_ref_readonly_T : WellKnownMember.System_Span_T__ctor_ref_T, isOptional: true) is MethodSymbol methodSymbol)
		{
			NamedTypeSymbol newOwner = _factory.WellKnownType(asReadOnlySpan ? WellKnownType.System_ReadOnlySpan_T : WellKnownType.System_Span_T).Construct(ImmutableCollectionsMarshal.AsImmutableArray(new TypeWithAnnotations[1] { elementType }));
			MethodSymbol constructor = methodSymbol.AsMember(newOwner);
			BoundExpression argument = VisitExpression((BoundExpression)elements[0]);
			BoundLocal boundLocal = _factory.StoreToTemp(argument, out BoundAssignmentOperator store);
			_additionalLocals.Add(boundLocal.LocalSymbol);
			BoundObjectCreationExpression result = _factory.New(constructor, ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1] { boundLocal }), ImmutableCollectionsMarshal.AsImmutableArray(new RefKind[1] { (!asReadOnlySpan) ? RefKind.Ref : ((RefKind)5) }));
			return _factory.Sequence(new BoundExpression[1] { store }, result);
		}
		NamedTypeSymbol namedTypeSymbol = _factory.ModuleBuilderOpt.EnsureInlineArrayTypeExists(syntax, _factory, length, _diagnostics).Construct(ImmutableArray.Create(elementType));
		NamedTypeSymbol intType = _factory.SpecialType(SpecialType.System_Int32);
		MethodSymbol method = _factory.ModuleBuilderOpt.EnsureInlineArrayElementRefExists(syntax, intType, _diagnostics.DiagnosticBag).Construct(ImmutableArray.Create(TypeWithAnnotations.Create(namedTypeSymbol), elementType));
		BoundLocal boundLocal2 = _factory.StoreToTemp(new BoundDefaultExpression(syntax, namedTypeSymbol), out BoundAssignmentOperator store2);
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		instance.Add(store2);
		_additionalLocals.Add(boundLocal2.LocalSymbol);
		for (int i = 0; i < length; i++)
		{
			BoundExpression right = VisitExpression((BoundExpression)elements[i]);
			BoundCall boundCall = _factory.Call(null, method, boundLocal2, _factory.Literal(i), useStrictArgumentRefKinds: true);
			BoundAssignmentOperator item = new BoundAssignmentOperator(syntax, boundCall, right, boundCall.Type)
			{
				WasCompilerGenerated = true
			};
			instance.Add(item);
		}
		MethodSymbol methodSymbol2 = (asReadOnlySpan ? _factory.ModuleBuilderOpt.EnsureInlineArrayAsReadOnlySpanExists(syntax, _factory.WellKnownType(WellKnownType.System_ReadOnlySpan_T), intType, _diagnostics.DiagnosticBag) : _factory.ModuleBuilderOpt.EnsureInlineArrayAsSpanExists(syntax, _factory.WellKnownType(WellKnownType.System_Span_T), intType, _diagnostics.DiagnosticBag));
		methodSymbol2 = methodSymbol2.Construct(ImmutableArray.Create(TypeWithAnnotations.Create(namedTypeSymbol), elementType));
		BoundCall boundCall2 = _factory.Call(null, methodSymbol2, boundLocal2, _factory.Literal(length), useStrictArgumentRefKinds: true);
		return new BoundSequence(syntax, ImmutableArray<LocalSymbol>.Empty, instance.ToImmutableAndFree(), boundCall2, boundCall2.Type);
	}

	private static bool ShouldUseKnownLength(BoundCollectionExpression node, out int numberIncludingLastSpread)
	{
		node.HasSpreadElements(out var numberIncludingLastSpread2, out var hasKnownLength);
		if (hasKnownLength && numberIncludingLastSpread2 <= 3)
		{
			numberIncludingLastSpread = numberIncludingLastSpread2;
			return true;
		}
		numberIncludingLastSpread = 0;
		return false;
	}

	private BoundExpression? TryOptimizeSingleSpreadToArray_NoConversionApplied(BoundCollectionExpression node, bool targetsReadOnlyCollection, ArrayTypeSymbol arrayType)
	{
		if (node != null)
		{
			ImmutableArray<BoundNode> elements = node.Elements;
			if (elements.Length == 1 && elements[0] is BoundCollectionExpressionSpreadElement boundCollectionExpressionSpreadElement)
			{
				BoundExpression expression = boundCollectionExpressionSpreadElement.Expression;
				if (expression != null && boundCollectionExpressionSpreadElement.IteratorBody is BoundExpressionStatement boundExpressionStatement)
				{
					Conversion conversion = ((boundExpressionStatement.Expression is BoundConversion { Conversion: var conversion2 }) ? conversion2 : Conversion.Identity);
					bool flag2;
					if (targetsReadOnlyCollection)
					{
						ConversionKind kind = conversion.Kind;
						bool flag = ((kind == ConversionKind.Identity || kind == ConversionKind.ImplicitReference) ? true : false);
						flag2 = flag;
					}
					else
					{
						flag2 = conversion.Kind == ConversionKind.Identity;
					}
					bool flag3 = flag2;
					TypeSymbol originalDefinition = expression.Type.OriginalDefinition;
					if (flag3 && tryGetToArrayMethod(originalDefinition, WellKnownType.System_Collections_Generic_List_T, WellKnownMember.System_Collections_Generic_List_T__ToArray, out var toArrayMethod))
					{
						BoundExpression receiver = VisitExpression(expression);
						return _factory.Call(receiver, toArrayMethod.AsMember((NamedTypeSymbol)expression.Type));
					}
					MethodSymbol methodSymbol = _factory.WellKnownMethod(WellKnownMember.System_Linq_Enumerable__ToArray, isOptional: true);
					if ((object)methodSymbol != null)
					{
						MethodSymbol methodSymbol2 = methodSymbol.Construct(ImmutableCollectionsMarshal.AsImmutableArray(new TypeWithAnnotations[1] { arrayType.ElementTypeWithAnnotations }));
						if (methodSymbol2.CheckConstraints(new ConstraintsHelper.CheckConstraintsArgs(_compilation, _compilation.Conversions, Location.None, BindingDiagnosticBag.Discarded)) && ShouldUseIEnumerableBulkAddMethod(expression.Type, methodSymbol2.Parameters[0].Type, boundCollectionExpressionSpreadElement.EnumeratorInfoOpt?.GetEnumeratorInfo.Method))
						{
							return _factory.Call(null, methodSymbol2, VisitExpression(expression));
						}
					}
					if (flag3 && TryGetSpanConversion(expression.Type, writableOnly: false, out MethodSymbol asSpanMethod))
					{
						TypeSymbol originalDefinition2 = CallAsSpanMethod(expression, asSpanMethod).Type.OriginalDefinition;
						if (tryGetToArrayMethod(originalDefinition2, WellKnownType.System_ReadOnlySpan_T, WellKnownMember.System_ReadOnlySpan_T__ToArray, out var toArrayMethod2) || tryGetToArrayMethod(originalDefinition2, WellKnownType.System_Span_T, WellKnownMember.System_Span_T__ToArray, out toArrayMethod2))
						{
							BoundExpression boundExpression = CallAsSpanMethod(VisitExpression(expression), asSpanMethod);
							return _factory.Call(boundExpression, toArrayMethod2.AsMember((NamedTypeSymbol)boundExpression.Type));
						}
					}
				}
			}
		}
		return null;
		bool tryGetToArrayMethod(TypeSymbol spreadTypeOriginalDefinition, WellKnownType wellKnownType, WellKnownMember wellKnownMember, [NotNullWhen(true)] out MethodSymbol? reference)
		{
			if (TypeSymbol.Equals(spreadTypeOriginalDefinition, _compilation.GetWellKnownType(wellKnownType), TypeCompareKind.AllIgnoreOptions))
			{
				reference = _factory.WellKnownMethod(wellKnownMember, isOptional: true);
				return (object)reference != null;
			}
			reference = null;
			return false;
		}
	}

	private BoundExpression CreateAndPopulateArray(BoundCollectionExpression node, ArrayTypeSymbol arrayType)
	{
		SyntaxNode syntax = node.Syntax;
		ImmutableArray<BoundNode> elements = node.Elements;
		if (!ShouldUseKnownLength(node, out var numberIncludingLastSpread))
		{
			throw ExceptionUtilities.UnexpectedValue(node);
		}
		if (numberIncludingLastSpread == 0)
		{
			int length = elements.Length;
			if (length == 0)
			{
				return CreateEmptyArray(syntax, arrayType);
			}
			BoundArrayInitialization initializerOpt = new BoundArrayInitialization(syntax, isInferred: false, elements.SelectAsArray((BoundNode element, LocalRewriter rewriter) => rewriter.VisitExpression((BoundExpression)element), this));
			return new BoundArrayCreation(syntax, ImmutableArray.Create((BoundExpression)new BoundLiteral(syntax, ConstantValue.Create(length), _compilation.GetSpecialType(SpecialType.System_Int32))), initializerOpt, arrayType)
			{
				WasCompilerGenerated = true
			};
		}
		ArrayBuilder<BoundLocal> localsBuilder = ArrayBuilder<BoundLocal>.GetInstance();
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		RewriteCollectionExpressionElementsIntoTemporaries(elements, numberIncludingLastSpread, localsBuilder, instance);
		BoundLocal indexTemp = null;
		BoundAssignmentOperator store;
		if (numberIncludingLastSpread != 0)
		{
			indexTemp = _factory.StoreToTemp(_factory.Literal(0), out store);
			localsBuilder.Add(indexTemp);
			instance.Add(store);
		}
		BoundLocal boundLocal = _factory.StoreToTemp(new BoundArrayCreation(syntax, ImmutableArray.Create(GetKnownLengthExpression(elements, numberIncludingLastSpread, localsBuilder)), null, arrayType), out store);
		localsBuilder.Add(boundLocal);
		instance.Add(store);
		int currentElementIndex = 0;
		AddCollectionExpressionElements(elements, boundLocal, localsBuilder, numberIncludingLastSpread, instance, delegate(ArrayBuilder<BoundExpression> expressions, BoundExpression arrayTemp, BoundExpression rewrittenValue, bool isLastElement)
		{
			SyntaxNode syntax2 = rewrittenValue.Syntax;
			TypeSymbol elementType = ((ArrayTypeSymbol)arrayTemp.Type).ElementType;
			if (indexTemp == null)
			{
				expressions.Add(new BoundAssignmentOperator(syntax2, _factory.ArrayAccess(arrayTemp, _factory.Literal(currentElementIndex)), rewrittenValue, isRef: false, elementType));
				currentElementIndex++;
			}
			else
			{
				expressions.Add(new BoundAssignmentOperator(syntax2, _factory.ArrayAccess(arrayTemp, indexTemp), rewrittenValue, isRef: false, elementType));
				if (!isLastElement)
				{
					expressions.Add(new BoundAssignmentOperator(syntax2, indexTemp, _factory.Binary(BinaryOperatorKind.Addition, indexTemp.Type, indexTemp, _factory.Literal(1)), isRef: false, indexTemp.Type));
				}
			}
		}, delegate(ArrayBuilder<BoundExpression> sideEffects, BoundExpression arrayTemp, BoundCollectionExpressionSpreadElement spreadElement, BoundExpression rewrittenSpreadOperand)
		{
			(MethodSymbol, BoundExpression, MethodSymbol, MethodSymbol)? tuple = PrepareCopyToOptimization(spreadElement, rewrittenSpreadOperand);
			if (!tuple.HasValue)
			{
				return false;
			}
			var (spanSliceMethod, spreadOperandAsSpan, getLengthMethod, copyToMethod) = tuple.GetValueOrDefault();
			if (!TryConvertToSpan(arrayTemp, writableOnly: true, out BoundExpression span))
			{
				return false;
			}
			PerformCopyToOptimization(sideEffects, localsBuilder, indexTemp, span, rewrittenSpreadOperand, spanSliceMethod, spreadOperandAsSpan, getLengthMethod, copyToMethod);
			return true;
		});
		ImmutableArray<LocalSymbol> locals = localsBuilder.SelectAsArray((BoundLocal l) => l.LocalSymbol);
		localsBuilder.Free();
		return new BoundSequence(syntax, locals, instance.ToImmutableAndFree(), boundLocal, arrayType);
	}

	private bool TryGetSpanConversion(TypeSymbol type, bool writableOnly, out MethodSymbol? asSpanMethod)
	{
		if (type is ArrayTypeSymbol { IsSZArray: not false } arrayTypeSymbol)
		{
			MethodSymbol methodSymbol = _factory.WellKnownMethod(writableOnly ? WellKnownMember.System_Span_T__ctor_Array : WellKnownMember.System_ReadOnlySpan_T__ctor_Array, isOptional: true);
			if ((object)methodSymbol != null)
			{
				NamedTypeSymbol namedTypeSymbol = methodSymbol.ContainingType.Construct(arrayTypeSymbol.ElementType);
				if (namedTypeSymbol.CheckConstraints(new ConstraintsHelper.CheckConstraintsArgs(_compilation, _compilation.Conversions, Location.None, BindingDiagnosticBag.Discarded)))
				{
					asSpanMethod = methodSymbol.AsMember(namedTypeSymbol);
					return true;
				}
			}
		}
		if (!(type is NamedTypeSymbol namedTypeSymbol2))
		{
			asSpanMethod = null;
			return false;
		}
		if ((!writableOnly && namedTypeSymbol2.OriginalDefinition.Equals(_compilation.GetWellKnownType(WellKnownType.System_ReadOnlySpan_T), TypeCompareKind.ConsiderEverything)) || namedTypeSymbol2.OriginalDefinition.Equals(_compilation.GetWellKnownType(WellKnownType.System_Span_T), TypeCompareKind.ConsiderEverything))
		{
			asSpanMethod = null;
			return true;
		}
		if (!writableOnly && namedTypeSymbol2.OriginalDefinition.Equals(_compilation.GetWellKnownType(WellKnownType.System_Collections_Immutable_ImmutableArray_T), TypeCompareKind.ConsiderEverything))
		{
			MethodSymbol methodSymbol2 = _factory.WellKnownMethod(WellKnownMember.System_Collections_Immutable_ImmutableArray_T__AsSpan, isOptional: true);
			if ((object)methodSymbol2 != null)
			{
				asSpanMethod = methodSymbol2.AsMember(namedTypeSymbol2);
				return true;
			}
		}
		if (namedTypeSymbol2.OriginalDefinition.Equals(_compilation.GetWellKnownType(WellKnownType.System_Collections_Generic_List_T), TypeCompareKind.ConsiderEverything))
		{
			MethodSymbol methodSymbol3 = _factory.WellKnownMethod(WellKnownMember.System_Runtime_InteropServices_CollectionsMarshal__AsSpan_T, isOptional: true);
			if ((object)methodSymbol3 != null)
			{
				asSpanMethod = methodSymbol3.Construct(namedTypeSymbol2.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[0].Type);
				return true;
			}
		}
		asSpanMethod = null;
		return false;
	}

	private bool TryConvertToSpan(BoundExpression expression, bool writableOnly, [NotNullWhen(true)] out BoundExpression? span)
	{
		TypeSymbol type = expression.Type;
		if (!TryGetSpanConversion(type, writableOnly, out MethodSymbol asSpanMethod))
		{
			span = null;
			return false;
		}
		span = CallAsSpanMethod(expression, asSpanMethod);
		return true;
	}

	private BoundExpression CallAsSpanMethod(BoundExpression spreadExpression, MethodSymbol? asSpanMethod)
	{
		if ((object)asSpanMethod == null)
		{
			return spreadExpression;
		}
		if ((object)asSpanMethod != null && asSpanMethod.MethodKind == MethodKind.Constructor)
		{
			return _factory.New(asSpanMethod, spreadExpression);
		}
		if ((object)asSpanMethod != null && asSpanMethod.IsStatic && asSpanMethod.ParameterCount == 1)
		{
			return _factory.Call(null, asSpanMethod, spreadExpression);
		}
		return _factory.Call(spreadExpression, asSpanMethod);
	}

	private (MethodSymbol spanSliceMethod, BoundExpression spreadElementAsSpan, MethodSymbol getLengthMethod, MethodSymbol copyToMethod)? PrepareCopyToOptimization(BoundCollectionExpressionSpreadElement spreadElement, BoundExpression rewrittenSpreadOperand)
	{
		if (!(spreadElement.IteratorBody is BoundExpressionStatement boundExpressionStatement) || boundExpressionStatement.Expression is BoundConversion { ConversionKind: not ConversionKind.Identity })
		{
			return null;
		}
		MethodSymbol methodSymbol = _factory.WellKnownMethod(WellKnownMember.System_Span_T__Slice_Int_Int, isOptional: true);
		if ((object)methodSymbol == null)
		{
			return null;
		}
		if (!TryConvertToSpan(rewrittenSpreadOperand, writableOnly: false, out BoundExpression spreadOperandAsSpan))
		{
			return null;
		}
		(MethodSymbol, MethodSymbol)? tuple = getSpanMethodsForSpread(WellKnownType.System_ReadOnlySpan_T, WellKnownMember.System_ReadOnlySpan_T__get_Length, WellKnownMember.System_ReadOnlySpan_T__CopyTo_Span_T) ?? getSpanMethodsForSpread(WellKnownType.System_Span_T, WellKnownMember.System_Span_T__get_Length, WellKnownMember.System_Span_T__CopyTo_Span_T);
		if (!tuple.HasValue)
		{
			return null;
		}
		var (item, item2) = tuple.GetValueOrDefault();
		return (methodSymbol, spreadOperandAsSpan, item, item2);
		(MethodSymbol getLengthMethod, MethodSymbol copyToMethod)? getSpanMethodsForSpread(WellKnownType wellKnownSpanType, WellKnownMember getLengthMember, WellKnownMember copyToMember)
		{
			if (spreadOperandAsSpan.Type.OriginalDefinition.Equals(_compilation.GetWellKnownType(wellKnownSpanType)))
			{
				MethodSymbol methodSymbol2 = _factory.WellKnownMethod(getLengthMember, isOptional: true);
				if ((object)methodSymbol2 != null)
				{
					MethodSymbol methodSymbol3 = _factory.WellKnownMethod(copyToMember, isOptional: true);
					if ((object)methodSymbol3 != null)
					{
						return (methodSymbol2, methodSymbol3);
					}
				}
			}
			return null;
		}
	}

	private void PerformCopyToOptimization(ArrayBuilder<BoundExpression> sideEffects, ArrayBuilder<BoundLocal> localsBuilder, BoundLocal indexTemp, BoundExpression spanTemp, BoundExpression rewrittenSpreadOperand, MethodSymbol spanSliceMethod, BoundExpression spreadOperandAsSpan, MethodSymbol getLengthMethod, MethodSymbol copyToMethod)
	{
		if (spreadOperandAsSpan != rewrittenSpreadOperand)
		{
			spreadOperandAsSpan = _factory.StoreToTemp(spreadOperandAsSpan, out BoundAssignmentOperator store);
			sideEffects.Add(store);
			localsBuilder.Add((BoundLocal)spreadOperandAsSpan);
		}
		BoundCall boundCall = _factory.Call(spreadOperandAsSpan, getLengthMethod.AsMember((NamedTypeSymbol)spreadOperandAsSpan.Type));
		BoundCall arg = _factory.Call(spanTemp, spanSliceMethod.AsMember((NamedTypeSymbol)spanTemp.Type), indexTemp, boundCall);
		sideEffects.Add(_factory.Call(spreadOperandAsSpan, copyToMethod.AsMember((NamedTypeSymbol)spreadOperandAsSpan.Type), arg));
		sideEffects.Add(new BoundAssignmentOperator(rewrittenSpreadOperand.Syntax, indexTemp, _factory.Binary(BinaryOperatorKind.Addition, indexTemp.Type, indexTemp, boundCall), isRef: false, indexTemp.Type));
	}

	private BoundExpression CreateAndPopulateList(BoundCollectionExpression node, TypeWithAnnotations elementType, ImmutableArray<BoundNode> elements)
	{
		ImmutableArray<TypeWithAnnotations> typeArguments = ImmutableArray.Create(elementType);
		NamedTypeSymbol namedTypeSymbol = _factory.WellKnownType(WellKnownType.System_Collections_Generic_List_T).Construct(typeArguments);
		ArrayBuilder<BoundLocal> localsBuilder = ArrayBuilder<BoundLocal>.GetInstance();
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(elements.Length + 1);
		bool num = ShouldUseKnownLength(node, out var numberIncludingLastSpread);
		RewriteCollectionExpressionElementsIntoTemporaries(elements, numberIncludingLastSpread, localsBuilder, instance);
		bool flag = false;
		MethodSymbol methodSymbol = null;
		MethodSymbol methodSymbol2 = null;
		if (num && elements.Length > 0)
		{
			MethodSymbol? currentFunction = _factory.CurrentFunction;
			if ((object)currentFunction != null && !currentFunction.IsAsync)
			{
				methodSymbol = ((MethodSymbol)_compilation.GetWellKnownTypeMember(WellKnownMember.System_Runtime_InteropServices_CollectionsMarshal__SetCount_T))?.Construct(typeArguments);
				methodSymbol2 = ((MethodSymbol)_compilation.GetWellKnownTypeMember(WellKnownMember.System_Runtime_InteropServices_CollectionsMarshal__AsSpan_T))?.Construct(typeArguments);
				if ((object)methodSymbol != null && (object)methodSymbol2 != null)
				{
					flag = true;
				}
			}
		}
		BoundLocal boundLocal = null;
		BoundAssignmentOperator store;
		BoundObjectCreationExpression argument;
		if (num && elements.Length > 0)
		{
			MethodSymbol ctor = ((MethodSymbol)_factory.WellKnownMember(WellKnownMember.System_Collections_Generic_List_T__ctorInt32)).AsMember(namedTypeSymbol);
			BoundExpression knownLengthExpression = GetKnownLengthExpression(elements, numberIncludingLastSpread, localsBuilder);
			if (flag)
			{
				boundLocal = _factory.StoreToTemp(knownLengthExpression, out store);
				localsBuilder.Add(boundLocal);
				instance.Add(store);
				argument = _factory.New(ctor, ImmutableArray.Create((BoundExpression)boundLocal));
			}
			else
			{
				argument = _factory.New(ctor, ImmutableArray.Create(knownLengthExpression));
			}
		}
		else
		{
			MethodSymbol ctor2 = ((MethodSymbol)_factory.WellKnownMember(WellKnownMember.System_Collections_Generic_List_T__ctor)).AsMember(namedTypeSymbol);
			argument = _factory.New(ctor2, ImmutableArray<BoundExpression>.Empty);
		}
		BoundLocal boundLocal2 = _factory.StoreToTemp(argument, out store);
		localsBuilder.Add(boundLocal2);
		instance.Add(store);
		if (flag)
		{
			instance.Add(_factory.Call(null, methodSymbol, boundLocal2, boundLocal));
			BoundLocal boundLocal3 = _factory.StoreToTemp(_factory.Call(null, methodSymbol2, boundLocal2), out store);
			localsBuilder.Add(boundLocal3);
			instance.Add(store);
			MethodSymbol spanGetItem = ((MethodSymbol)_factory.WellKnownMember(WellKnownMember.System_Span_T__get_Item)).AsMember((NamedTypeSymbol)boundLocal3.Type);
			BoundLocal indexTemp = null;
			if (numberIncludingLastSpread != 0)
			{
				indexTemp = _factory.StoreToTemp(_factory.Literal(0), out store);
				localsBuilder.Add(indexTemp);
				instance.Add(store);
			}
			int currentElementIndex = 0;
			AddCollectionExpressionElements(elements, boundLocal3, localsBuilder, numberIncludingLastSpread, instance, delegate(ArrayBuilder<BoundExpression> expressions, BoundExpression spanTemp, BoundExpression rewrittenValue, bool isLastElement)
			{
				SyntaxNode syntax = rewrittenValue.Syntax;
				TypeSymbol type = ((NamedTypeSymbol)spanTemp.Type).TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[0].Type;
				if (indexTemp == null)
				{
					expressions.Add(new BoundAssignmentOperator(syntax, _factory.Call(spanTemp, spanGetItem, _factory.Literal(currentElementIndex)), rewrittenValue, isRef: false, type));
					currentElementIndex++;
				}
				else
				{
					expressions.Add(new BoundAssignmentOperator(syntax, _factory.Call(spanTemp, spanGetItem, indexTemp), rewrittenValue, isRef: false, type));
					if (!isLastElement)
					{
						expressions.Add(new BoundAssignmentOperator(syntax, indexTemp, _factory.Binary(BinaryOperatorKind.Addition, indexTemp.Type, indexTemp, _factory.Literal(1)), isRef: false, indexTemp.Type));
					}
				}
			}, delegate(ArrayBuilder<BoundExpression> sideEffects, BoundExpression spanTemp, BoundCollectionExpressionSpreadElement spreadElement, BoundExpression rewrittenSpreadOperand)
			{
				(MethodSymbol, BoundExpression, MethodSymbol, MethodSymbol)? tuple = PrepareCopyToOptimization(spreadElement, rewrittenSpreadOperand);
				if (!tuple.HasValue)
				{
					return false;
				}
				var (spanSliceMethod, spreadOperandAsSpan, getLengthMethod, copyToMethod) = tuple.GetValueOrDefault();
				PerformCopyToOptimization(sideEffects, localsBuilder, indexTemp, spanTemp, rewrittenSpreadOperand, spanSliceMethod, spreadOperandAsSpan, getLengthMethod, copyToMethod);
				return true;
			});
		}
		else
		{
			MethodSymbol addMethod = _factory.WellKnownMethod(WellKnownMember.System_Collections_Generic_List_T__Add).AsMember(namedTypeSymbol);
			MethodSymbol addRangeMethod = _factory.WellKnownMethod(WellKnownMember.System_Collections_Generic_List_T__AddRange, isOptional: true)?.AsMember(namedTypeSymbol);
			AddCollectionExpressionElements(elements, boundLocal2, localsBuilder, numberIncludingLastSpread, instance, delegate(ArrayBuilder<BoundExpression> expressions, BoundExpression listTemp, BoundExpression rewrittenValue, bool isLastElement)
			{
				expressions.Add(_factory.Call(listTemp, addMethod, rewrittenValue));
			}, delegate(ArrayBuilder<BoundExpression> sideEffects, BoundExpression listTemp, BoundCollectionExpressionSpreadElement spreadElement, BoundExpression rewrittenSpreadOperand)
			{
				if ((object)addRangeMethod == null)
				{
					return false;
				}
				if (!ShouldUseIEnumerableBulkAddMethod(rewrittenSpreadOperand.Type, addRangeMethod.Parameters[0].Type, spreadElement.EnumeratorInfoOpt?.GetEnumeratorInfo.Method))
				{
					return false;
				}
				sideEffects.Add(_factory.Call(listTemp, addRangeMethod, rewrittenSpreadOperand));
				return true;
			});
		}
		ImmutableArray<LocalSymbol> locals = localsBuilder.SelectAsArray((BoundLocal l) => l.LocalSymbol);
		localsBuilder.Free();
		return new BoundSequence(node.Syntax, locals, instance.ToImmutableAndFree(), boundLocal2, namedTypeSymbol);
	}

	private BoundExpression RewriteCollectionExpressionElementExpression(BoundNode element)
	{
		BoundExpression node = ((element is BoundCollectionExpressionSpreadElement boundCollectionExpressionSpreadElement) ? boundCollectionExpressionSpreadElement.Expression : ((BoundExpression)element));
		return VisitExpression(node);
	}

	private void RewriteCollectionExpressionElementsIntoTemporaries(ImmutableArray<BoundNode> elements, int numberIncludingLastSpread, ArrayBuilder<BoundLocal> locals, ArrayBuilder<BoundExpression> sideEffects)
	{
		for (int i = 0; i < numberIncludingLastSpread; i++)
		{
			BoundExpression argument = RewriteCollectionExpressionElementExpression(elements[i]);
			BoundLocal item = _factory.StoreToTemp(argument, out BoundAssignmentOperator store);
			locals.Add(item);
			sideEffects.Add(store);
		}
	}

	private void AddCollectionExpressionElements(ImmutableArray<BoundNode> elements, BoundExpression rewrittenReceiver, ArrayBuilder<BoundLocal> rewrittenExpressions, int numberIncludingLastSpread, ArrayBuilder<BoundExpression> sideEffects, Action<ArrayBuilder<BoundExpression>, BoundExpression, BoundExpression, bool> addElement, Func<ArrayBuilder<BoundExpression>, BoundExpression, BoundCollectionExpressionSpreadElement, BoundExpression, bool> tryOptimizeSpreadElement)
	{
		for (int i = 0; i < elements.Length; i++)
		{
			BoundNode boundNode = elements[i];
			BoundExpression boundExpression = ((i < numberIncludingLastSpread) ? rewrittenExpressions[i] : RewriteCollectionExpressionElementExpression(boundNode));
			if (boundNode is BoundCollectionExpressionSpreadElement boundCollectionExpressionSpreadElement)
			{
				if (tryOptimizeSpreadElement(sideEffects, rewrittenReceiver, boundCollectionExpressionSpreadElement, boundExpression))
				{
					continue;
				}
				BoundExpression item = MakeCollectionExpressionSpreadElement(boundCollectionExpressionSpreadElement, boundExpression, delegate(BoundStatement iteratorBody)
				{
					BoundExpression arg2 = VisitExpression(((BoundExpressionStatement)iteratorBody).Expression);
					ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
					addElement(instance, rewrittenReceiver, arg2, arg4: false);
					ImmutableArray<BoundStatement> statements = instance.SelectAsArray((Func<BoundExpression, BoundStatement>)((BoundExpression expr) => new BoundExpressionStatement(expr.Syntax, expr)));
					instance.Free();
					return (statements.Length != 1) ? new BoundBlock(iteratorBody.Syntax, ImmutableArray<LocalSymbol>.Empty, statements) : statements[0];
				});
				sideEffects.Add(item);
			}
			else
			{
				bool arg = i == elements.Length - 1;
				addElement(sideEffects, rewrittenReceiver, boundExpression, arg);
			}
		}
	}

	private BoundExpression GetKnownLengthExpression(ImmutableArray<BoundNode> elements, int numberIncludingLastSpread, ArrayBuilder<BoundLocal> rewrittenExpressions)
	{
		int num = 0;
		BoundExpression boundExpression = null;
		for (int i = 0; i < numberIncludingLastSpread; i++)
		{
			if (elements[i] is BoundCollectionExpressionSpreadElement boundCollectionExpressionSpreadElement)
			{
				BoundCollectionExpressionSpreadExpressionPlaceholder expressionPlaceholder = boundCollectionExpressionSpreadElement.ExpressionPlaceholder;
				AddPlaceholderReplacement(expressionPlaceholder, rewrittenExpressions[i]);
				BoundExpression value = VisitExpression(boundCollectionExpressionSpreadElement.LengthOrCount);
				RemovePlaceholderReplacement(expressionPlaceholder);
				boundExpression = add(boundExpression, value);
			}
			else
			{
				num++;
			}
		}
		num += elements.Length - numberIncludingLastSpread;
		if (num > 0)
		{
			BoundLiteral boundLiteral = _factory.Literal(num);
			boundExpression = ((boundExpression == null) ? boundLiteral : add(boundLiteral, boundExpression));
		}
		return boundExpression;
		BoundExpression add(BoundExpression? sum, BoundExpression boundExpression2)
		{
			if (sum != null)
			{
				return _factory.Binary(BinaryOperatorKind.Addition, sum.Type, sum, boundExpression2);
			}
			return boundExpression2;
		}
	}

	private BoundExpression MakeCollectionExpressionSpreadElement(BoundCollectionExpressionSpreadElement node, BoundExpression rewrittenExpression, Func<BoundStatement, BoundStatement> rewriteBody)
	{
		ForEachEnumeratorInfo enumeratorInfoOpt = node.EnumeratorInfoOpt;
		BoundConversion boundConversion = (BoundConversion)node.Conversion;
		BoundCollectionExpressionSpreadExpressionPlaceholder expressionPlaceholder = node.ExpressionPlaceholder;
		BoundValuePlaceholder elementPlaceholder = node.ElementPlaceholder;
		BoundStatement iteratorBody = node.IteratorBody;
		AddPlaceholderReplacement(expressionPlaceholder, rewrittenExpression);
		LocalSymbol localSymbol = _factory.SynthesizedLocal(enumeratorInfoOpt.ElementType, node.Syntax);
		BoundLocal value = _factory.Local(localSymbol);
		AddPlaceholderReplacement(elementPlaceholder, value);
		BoundStatement rewrittenBody = rewriteBody(iteratorBody);
		RemovePlaceholderReplacement(elementPlaceholder);
		ImmutableArray<LocalSymbol> iterationVariables = ImmutableArray.Create(localSymbol);
		GeneratedLabelSymbol breakLabel = new GeneratedLabelSymbol("break");
		GeneratedLabelSymbol continueLabel = new GeneratedLabelSymbol("continue");
		BoundStatement item = ((!(boundConversion.Operand.Type is ArrayTypeSymbol arrayTypeSymbol)) ? ((enumeratorInfoOpt == null || enumeratorInfoOpt.InlineArraySpanType == WellKnownType.Unknown) ? RewriteForEachEnumerator(node, boundConversion, enumeratorInfoOpt, null, null, iterationVariables, null, breakLabel, continueLabel, rewrittenBody) : RewriteForEachStatementAsFor(node, GetInlineArrayForEachStatementPreambleDelegate(), GetInlineArrayForEachStatementGetItemDelegate(), GetInlineArrayForEachStatementGetLengthDelegate(), null, boundConversion.Operand, enumeratorInfoOpt, null, null, iterationVariables, null, breakLabel, continueLabel, rewrittenBody)) : ((!arrayTypeSymbol.IsSZArray) ? RewriteMultiDimensionalArrayForEachEnumerator(node, boundConversion.Operand, null, null, iterationVariables, null, breakLabel, continueLabel, rewrittenBody) : RewriteSingleDimensionalArrayForEachEnumerator(node, boundConversion.Operand, null, null, iterationVariables, null, breakLabel, continueLabel, rewrittenBody)));
		RemovePlaceholderReplacement(expressionPlaceholder);
		_needsSpilling = true;
		return _factory.SpillSequence(ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(item), _factory.Literal(0));
	}

	public override BoundNode VisitCompoundAssignmentOperator(BoundCompoundAssignmentOperator node)
	{
		return VisitCompoundAssignmentOperator(node, used: true);
	}

	private BoundExpression VisitCompoundAssignmentOperator(BoundCompoundAssignmentOperator node, bool used)
	{
		MethodSymbol method = node.Operator.Method;
		if ((object)method != null && !method.IsStatic)
		{
			return VisitInstanceCompoundAssignmentOperator(node, used);
		}
		return VisitBuiltInOrStaticCompoundAssignmentOperator(node, used);
	}

	private BoundExpression VisitInstanceCompoundAssignmentOperator(BoundCompoundAssignmentOperator node, bool used)
	{
		SyntaxNode syntax = node.Syntax;
		if (!used)
		{
			return BoundCall.Synthesized(syntax, ApplyConversionIfNotIdentity(node.LeftConversion, node.LeftPlaceholder, VisitExpression(node.Left)), ThreeState.False, node.Operator.Method, VisitExpression(node.Right));
		}
		TypeSymbol type = node.Left.Type;
		if (type.IsReferenceType)
		{
			BoundLocal boundLocal = _factory.StoreToTemp(VisitExpression(node.Left), out BoundAssignmentOperator store);
			return new BoundSequence(syntax, ImmutableCollectionsMarshal.AsImmutableArray(new LocalSymbol[1] { boundLocal.LocalSymbol }), ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[2]
			{
				store,
				BoundCall.Synthesized(syntax, ApplyConversionIfNotIdentity(node.LeftConversion, node.LeftPlaceholder, boundLocal), ThreeState.False, node.Operator.Method, VisitExpression(node.Right))
			}), boundLocal, type);
		}
		return MakeInstanceCompoundAssignmentOperatorResult(node.Syntax, node.Left, node.Right, node.Operator.Method, node.Operator.Kind.IsChecked(), AssignmentKind.CompoundAssignment);
	}

	private BoundExpression VisitBuiltInOrStaticCompoundAssignmentOperator(BoundCompoundAssignmentOperator node, bool used)
	{
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance();
		BinaryOperatorKind kind = node.Operator.Kind;
		bool isChecked = kind.IsChecked();
		bool isDynamic = kind.IsDynamic();
		BinaryOperatorKind binaryOperatorKind = kind.Operator();
		BoundExpression transformedLHS = TransformCompoundAssignmentLHS(node.Left, instance2, instance, isDynamic);
		BoundExpression boundExpression = MakeRValue(transformedLHS);
		BoundExpression boundExpression3;
		if (node.Left.Kind == BoundKind.DynamicMemberAccess && (binaryOperatorKind == BinaryOperatorKind.Addition || binaryOperatorKind == BinaryOperatorKind.Subtraction))
		{
			ArrayBuilder<LocalSymbol> instance3 = ArrayBuilder<LocalSymbol>.GetInstance();
			ArrayBuilder<BoundExpression> instance4 = ArrayBuilder<BoundExpression>.GetInstance();
			BoundDynamicMemberAccess boundDynamicMemberAccess = (BoundDynamicMemberAccess)transformedLHS;
			BoundLocal boundLocal = _factory.StoreToTemp(_dynamicFactory.MakeDynamicIsEventTest(boundDynamicMemberAccess.Name, boundDynamicMemberAccess.Receiver).ToExpression(), out BoundAssignmentOperator store);
			instance3.Add(boundLocal.LocalSymbol);
			instance4.Add(store);
			boundExpression = _factory.StoreToTemp(boundExpression, out BoundAssignmentOperator store2);
			instance3.Add(((BoundLocal)boundExpression).LocalSymbol);
			BoundLocal boundLocal2 = _factory.StoreToTemp(_factory.Conditional(_factory.Not(boundLocal), store2, _factory.Null(store2.Type), store2.Type), out BoundAssignmentOperator store3);
			instance3.Add(boundLocal2.LocalSymbol);
			instance4.Add(store3);
			BoundExpression boundExpression2 = VisitExpression(node.Right);
			if (CanChangeValueBetweenReads(boundExpression2))
			{
				boundExpression2 = _factory.StoreToTemp(boundExpression2, out BoundAssignmentOperator store4);
				instance3.Add(((BoundLocal)boundExpression2).LocalSymbol);
				instance4.Add(store4);
			}
			LoweredDynamicOperation loweredDynamicOperation = _dynamicFactory.MakeDynamicEventAccessorInvocation(((binaryOperatorKind == BinaryOperatorKind.Addition) ? "add_" : "remove_") + boundDynamicMemberAccess.Name, boundDynamicMemberAccess.Receiver, boundExpression2);
			boundExpression3 = rewriteAssignment(boundExpression, boundExpression2, rightIsVisited: true);
			BoundExpression boundExpression4 = _factory.Conditional(boundLocal, loweredDynamicOperation.ToExpression(), boundExpression3, boundExpression3.Type);
			boundExpression3 = new BoundSequence(node.Syntax, instance3.ToImmutableAndFree(), instance4.ToImmutableAndFree(), boundExpression4, boundExpression4.Type);
		}
		else
		{
			boundExpression3 = rewriteAssignment(boundExpression, node.Right, rightIsVisited: false);
		}
		BoundExpression result = ((instance.Count == 0 && instance2.Count == 0) ? boundExpression3 : new BoundSequence(node.Syntax, instance.ToImmutable(), instance2.ToImmutable(), boundExpression3, boundExpression3.Type));
		instance.Free();
		instance2.Free();
		return result;
		BoundExpression rewriteAssignment(BoundExpression leftRead, BoundExpression right, bool rightIsVisited)
		{
			SyntaxNode syntax = node.Syntax;
			BoundExpression boundExpression5 = leftRead;
			if (!isDynamic && node.LeftConversion != null)
			{
				AddPlaceholderReplacement(node.LeftPlaceholder, leftRead);
				boundExpression5 = VisitExpression(node.LeftConversion);
				RemovePlaceholderReplacement(node.LeftPlaceholder);
			}
			BoundExpression boundExpression6;
			if (IsBinaryStringConcatenation(node.Operator.Kind))
			{
				boundExpression6 = VisitCompoundAssignmentStringConcatenation(boundExpression5, right, node.Operator.Kind, node.Syntax);
			}
			else
			{
				BoundExpression loweredRight = (rightIsVisited ? right : VisitExpression(right));
				boundExpression6 = MakeBinaryOperator(syntax, node.Operator.Kind, boundExpression5, loweredRight, node.Operator.ReturnType, node.Operator.Method, node.Operator.ConstrainedToTypeOpt, isPointerElementAccess: false, isCompoundAssignment: true);
			}
			BoundExpression boundExpression7 = boundExpression6;
			if (node.FinalConversion != null)
			{
				AddPlaceholderReplacement(node.FinalPlaceholder, boundExpression6);
				boundExpression7 = VisitExpression(node.FinalConversion);
				RemovePlaceholderReplacement(node.FinalPlaceholder);
			}
			if (IsExtensionBlockMemberAccessWithByValPossiblyStructReceiver(transformedLHS))
			{
				BoundLocal boundLocal3 = _factory.StoreToTemp(boundExpression7, out BoundAssignmentOperator store5);
				BoundExpression boundExpression8 = MakeAssignmentOperator(syntax, transformedLHS, boundLocal3, used, isChecked, AssignmentKind.CompoundAssignment);
				return new BoundSequence(syntax, ImmutableCollectionsMarshal.AsImmutableArray(new LocalSymbol[1] { boundLocal3.LocalSymbol }), ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1] { store5 }), boundExpression8, boundExpression8.Type);
			}
			return MakeAssignmentOperator(syntax, transformedLHS, boundExpression7, used, isChecked, AssignmentKind.CompoundAssignment);
		}
	}

	private static bool IsExtensionBlockMemberAccessWithByValPossiblyStructReceiver(BoundExpression transformedLHS)
	{
		if (transformedLHS is BoundPropertyAccess boundPropertyAccess)
		{
			PropertySymbol propertySymbol = boundPropertyAccess.PropertySymbol;
			if ((object)propertySymbol != null)
			{
				return IsExtensionBlockMemberWithByValPossiblyStructReceiver(propertySymbol);
			}
		}
		else if (transformedLHS is BoundIndexerAccess boundIndexerAccess)
		{
			PropertySymbol indexer = boundIndexerAccess.Indexer;
			if ((object)indexer != null)
			{
				return IsExtensionBlockMemberWithByValPossiblyStructReceiver(indexer);
			}
		}
		return false;
	}

	private static bool IsExtensionBlockMemberWithByValPossiblyStructReceiver(Symbol symbol)
	{
		if (symbol.IsExtensionBlockMember() && !symbol.IsStatic)
		{
			ParameterSymbol extensionParameter = symbol.ContainingType.ExtensionParameter;
			if ((object)extensionParameter != null && extensionParameter.RefKind == RefKind.None)
			{
				TypeSymbol type = extensionParameter.Type;
				if ((object)type != null)
				{
					return !type.IsReferenceType;
				}
			}
			return false;
		}
		return false;
	}

	private BoundExpression? TransformPropertyOrEventReceiver(Symbol propertyOrEvent, BoundExpression? receiverOpt, ArrayBuilder<BoundExpression> stores, ArrayBuilder<LocalSymbol> temps)
	{
		if (receiverOpt == null || propertyOrEvent.IsStatic || !CanChangeValueBetweenReads(receiverOpt))
		{
			return VisitExpression(receiverOpt);
		}
		BoundExpression boundExpression = VisitExpression(receiverOpt);
		bool isKnownToReferToTempIfReferenceType = false;
		RefKind refKind;
		if (propertyOrEvent.IsExtensionBlockMember())
		{
			refKind = GetExtensionBlockMemberReceiverCaptureRefKind(boundExpression, propertyOrEvent);
			goto IL_008b;
		}
		int num;
		int num2;
		if (!boundExpression.Type.IsValueType)
		{
			num = ((boundExpression.Type.Kind == SymbolKind.TypeParameter) ? 1 : 0);
			if (num == 0)
			{
				num2 = 0;
				goto IL_0060;
			}
		}
		else
		{
			num = 1;
		}
		num2 = 1;
		goto IL_0060;
		IL_008b:
		BoundLocal boundLocal = _factory.StoreToTemp(boundExpression, out BoundAssignmentOperator store, (refKind == RefKind.RefReadOnlyParameter) ? RefKind.In : refKind, SynthesizedLocalKind.LoweringTemp, isKnownToReferToTempIfReferenceType);
		temps.Add(boundLocal.LocalSymbol);
		if (boundLocal.LocalSymbol.IsRef && (propertyOrEvent.IsExtensionBlockMember() ? (!boundLocal.Type.IsValueType) : CodeGenerator.IsPossibleReferenceTypeReceiverOfConstrainedCall(boundLocal)) && !CodeGenerator.ReceiverIsKnownToReferToTempIfReferenceType(boundLocal))
		{
			ReferToTempIfReferenceTypeReceiver(boundLocal, ref store, out BoundAssignmentOperator extraRefInitialization, temps);
			if (extraRefInitialization != null)
			{
				stores.Add(extraRefInitialization);
			}
		}
		stores.Add(store);
		return boundLocal;
		IL_0060:
		refKind = (RefKind)num2;
		isKnownToReferToTempIfReferenceType = num == 0 || boundExpression.Type.IsValueType || !CodeGenerator.HasHome(boundExpression, CodeGenerator.AddressKind.Constrained, _factory.CurrentFunction, peVerifyCompatEnabled: false, null);
		goto IL_008b;
	}

	private BoundDynamicMemberAccess TransformDynamicMemberAccess(BoundDynamicMemberAccess memberAccess, ArrayBuilder<BoundExpression> stores, ArrayBuilder<LocalSymbol> temps)
	{
		if (!CanChangeValueBetweenReads(memberAccess.Receiver))
		{
			return memberAccess;
		}
		BoundExpression argument = VisitExpression(memberAccess.Receiver);
		BoundLocal boundLocal = _factory.StoreToTemp(argument, out BoundAssignmentOperator store);
		stores.Add(store);
		temps.Add(boundLocal.LocalSymbol);
		return new BoundDynamicMemberAccess(memberAccess.Syntax, boundLocal, memberAccess.TypeArgumentsOpt, memberAccess.Name, memberAccess.Invoked, memberAccess.Indexed, memberAccess.Type);
	}

	private BoundIndexerAccess TransformIndexerAccess(BoundIndexerAccess indexerAccess, ArrayBuilder<BoundExpression> stores, ArrayBuilder<LocalSymbol> temps)
	{
		BoundExpression receiverOpt = indexerAccess.ReceiverOpt;
		BoundExpression rewrittenReceiver = VisitExpression(receiverOpt);
		ImmutableArray<BoundExpression> rewrittenArguments = VisitArgumentsAndCaptureReceiverIfNeeded(ref rewrittenReceiver, CanChangeValueBetweenReads(receiverOpt), indexerAccess.Arguments, indexerAccess.Indexer, indexerAccess.ArgsToParamsOpt, indexerAccess.ArgumentRefKindsOpt, stores, ref temps);
		return TransformIndexerAccessContinued(indexerAccess, rewrittenReceiver, rewrittenArguments, stores, temps);
	}

	private BoundIndexerAccess TransformIndexerAccessContinued(BoundIndexerAccess indexerAccess, BoundExpression transformedReceiver, ImmutableArray<BoundExpression> rewrittenArguments, ArrayBuilder<BoundExpression> stores, ArrayBuilder<LocalSymbol> temps)
	{
		SyntaxNode syntax = indexerAccess.Syntax;
		ImmutableArray<int> argsToParamsOpt = indexerAccess.ArgsToParamsOpt;
		ImmutableArray<RefKind> argumentRefKinds = indexerAccess.ArgumentRefKindsOpt;
		PropertySymbol indexer = indexerAccess.Indexer;
		bool expanded = indexerAccess.Expanded;
		rewrittenArguments = ExtractSideEffectsFromArguments(rewrittenArguments, indexer, expanded, argsToParamsOpt, ref argumentRefKinds, stores, temps);
		return new BoundIndexerAccess(syntax, transformedReceiver, ThreeState.Unknown, indexer, rewrittenArguments, default(ImmutableArray<string>), argumentRefKinds, expanded: false, indexerAccess.AccessorKind, default(ImmutableArray<int>), default(BitVector), indexerAccess.Type);
	}

	private ImmutableArray<BoundExpression> ExtractSideEffectsFromArguments(ImmutableArray<BoundExpression> rewrittenArguments, PropertySymbol indexer, bool expanded, ImmutableArray<int> argsToParamsOpt, ref ImmutableArray<RefKind> argumentRefKinds, ArrayBuilder<BoundExpression> stores, ArrayBuilder<LocalSymbol> temps)
	{
		ImmutableArray<ParameterSymbol> parameters = indexer.Parameters;
		BoundExpression[] array = new BoundExpression[parameters.Length];
		ArrayBuilder<BoundAssignmentOperator> instance = ArrayBuilder<BoundAssignmentOperator>.GetInstance(rewrittenArguments.Length);
		ArrayBuilder<RefKind> instance2 = ArrayBuilder<RefKind>.GetInstance(parameters.Length, RefKind.None);
		BuildStoresToTemps(expanded, argsToParamsOpt, parameters, argumentRefKinds, rewrittenArguments, forceLambdaSpilling: true, array, instance2, instance);
		if (expanded)
		{
			BoundExpression boundExpression = array[^1];
			if (boundExpression != null && boundExpression.IsParamsArrayOrCollection)
			{
				if (TryOptimizeParamsArray(boundExpression, out BoundExpression optimized))
				{
					boundExpression = optimized;
				}
				BoundLocal boundLocal = _factory.StoreToTemp(boundExpression, out BoundAssignmentOperator store);
				instance.Add(store);
				array[^1] = boundLocal;
			}
		}
		if (indexer.ContainingType.IsComImport)
		{
			RewriteArgumentsForComCall(parameters, array, instance2, temps);
		}
		rewrittenArguments = array.AsImmutableOrNull();
		foreach (BoundAssignmentOperator item in instance)
		{
			temps.Add(((BoundLocal)item.Left).LocalSymbol);
			stores.Add(item);
		}
		instance.Free();
		argumentRefKinds = GetRefKindsOrNull(instance2);
		instance2.Free();
		return rewrittenArguments;
	}

	private BoundExpression TransformImplicitIndexerAccess(BoundImplicitIndexerAccess indexerAccess, ArrayBuilder<BoundExpression> stores, ArrayBuilder<LocalSymbol> temps, bool isDynamicAssignment)
	{
		if (TypeSymbol.Equals(indexerAccess.Argument.Type, _compilation.GetWellKnownType(WellKnownType.System_Index), TypeCompareKind.ConsiderEverything))
		{
			return TransformIndexPatternIndexerAccess(indexerAccess, stores, temps, isDynamicAssignment);
		}
		throw ExceptionUtilities.UnexpectedValue(indexerAccess.Argument.Type);
	}

	private BoundExpression TransformIndexPatternIndexerAccess(BoundImplicitIndexerAccess implicitIndexerAccess, ArrayBuilder<BoundExpression> stores, ArrayBuilder<LocalSymbol> temps, bool isDynamicAssignment)
	{
		BoundExpression underlyingIndexerOrSliceAccess = GetUnderlyingIndexerOrSliceAccess(implicitIndexerAccess, isLeftOfAssignment: true, isRegularAssignment: false, cacheAllArgumentsOnly: false, stores, temps);
		if (underlyingIndexerOrSliceAccess is BoundIndexerAccess boundIndexerAccess)
		{
			return TransformIndexerAccessContinued(boundIndexerAccess, boundIndexerAccess.ReceiverOpt, boundIndexerAccess.Arguments, stores, temps);
		}
		BoundArrayAccess boundArrayAccess = (BoundArrayAccess)underlyingIndexerOrSliceAccess;
		if (isDynamicAssignment || !IsInvariantArray(boundArrayAccess.Expression.Type))
		{
			return SpillArrayElementAccess(boundArrayAccess.Expression, boundArrayAccess.Indices, stores, temps);
		}
		BoundLocal boundLocal = _factory.StoreToTemp(boundArrayAccess, out BoundAssignmentOperator store, RefKind.Ref);
		stores.Add(store);
		temps.Add(boundLocal.LocalSymbol);
		return boundLocal;
	}

	private bool TransformCompoundAssignmentFieldOrEventAccessReceiver(Symbol fieldOrEvent, ref BoundExpression? receiver, ArrayBuilder<BoundExpression> stores, ArrayBuilder<LocalSymbol> temps)
	{
		if (fieldOrEvent.IsStatic)
		{
			return true;
		}
		if (!CanChangeValueBetweenReads(receiver))
		{
			return true;
		}
		if (!receiver.Type.IsReferenceType)
		{
			return false;
		}
		BoundExpression boundExpression = VisitExpression(receiver);
		if (boundExpression.Type.IsTypeParameter())
		{
			NamedTypeSymbol containingType = fieldOrEvent.ContainingType;
			boundExpression = BoxReceiver(boundExpression, containingType);
		}
		BoundLocal boundLocal = _factory.StoreToTemp(boundExpression, out BoundAssignmentOperator store);
		stores.Add(store);
		temps.Add(boundLocal.LocalSymbol);
		receiver = boundLocal;
		return true;
	}

	private BoundDynamicIndexerAccess TransformDynamicIndexerAccess(BoundDynamicIndexerAccess indexerAccess, ArrayBuilder<BoundExpression> stores, ArrayBuilder<LocalSymbol> temps)
	{
		BoundExpression receiver;
		if (CanChangeValueBetweenReads(indexerAccess.Receiver))
		{
			BoundLocal boundLocal = _factory.StoreToTemp(VisitExpression(indexerAccess.Receiver), out BoundAssignmentOperator store);
			stores.Add(store);
			temps.Add(boundLocal.LocalSymbol);
			receiver = boundLocal;
		}
		else
		{
			receiver = indexerAccess.Receiver;
		}
		ImmutableArray<BoundExpression> arguments = indexerAccess.Arguments;
		BoundExpression[] array = new BoundExpression[arguments.Length];
		for (int i = 0; i < arguments.Length; i++)
		{
			if (CanChangeValueBetweenReads(arguments[i]))
			{
				BoundLocal boundLocal2 = _factory.StoreToTemp(VisitExpression(arguments[i]), out BoundAssignmentOperator store2, (indexerAccess.ArgumentRefKindsOpt.RefKinds(i) != RefKind.None) ? RefKind.Ref : RefKind.None);
				stores.Add(store2);
				temps.Add(boundLocal2.LocalSymbol);
				array[i] = boundLocal2;
			}
			else
			{
				array[i] = arguments[i];
			}
		}
		return new BoundDynamicIndexerAccess(indexerAccess.Syntax, receiver, array.AsImmutableOrNull(), indexerAccess.ArgumentNamesOpt, indexerAccess.ArgumentRefKindsOpt, indexerAccess.ApplicableIndexers, indexerAccess.Type);
	}

	private BoundExpression TransformCompoundAssignmentLHS(BoundExpression originalLHS, ArrayBuilder<BoundExpression> stores, ArrayBuilder<LocalSymbol> temps, bool isDynamicAssignment)
	{
		switch (originalLHS.Kind)
		{
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)originalLHS;
			if (boundPropertyAccess.PropertySymbol.RefKind == RefKind.None)
			{
				return boundPropertyAccess.Update(TransformPropertyOrEventReceiver(boundPropertyAccess.PropertySymbol, boundPropertyAccess.ReceiverOpt, stores, temps), boundPropertyAccess.InitialBindingReceiverIsSubjectToCloning, boundPropertyAccess.PropertySymbol, boundPropertyAccess.AutoPropertyAccessorKind, boundPropertyAccess.ResultKind, boundPropertyAccess.Type);
			}
			break;
		}
		case BoundKind.IndexerAccess:
			if (((BoundIndexerAccess)originalLHS).GetRefKind() == RefKind.None)
			{
				return TransformIndexerAccess((BoundIndexerAccess)originalLHS, stores, temps);
			}
			break;
		case BoundKind.ImplicitIndexerAccess:
		{
			BoundImplicitIndexerAccess boundImplicitIndexerAccess = (BoundImplicitIndexerAccess)originalLHS;
			if (boundImplicitIndexerAccess.GetRefKind() == RefKind.None)
			{
				return TransformImplicitIndexerAccess(boundImplicitIndexerAccess, stores, temps, isDynamicAssignment);
			}
			break;
		}
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)originalLHS;
			BoundExpression receiver2 = boundFieldAccess.ReceiverOpt;
			if (TransformCompoundAssignmentFieldOrEventAccessReceiver(boundFieldAccess.FieldSymbol, ref receiver2, stores, temps))
			{
				return MakeFieldAccess(boundFieldAccess.Syntax, receiver2, boundFieldAccess.FieldSymbol, boundFieldAccess.ConstantValueOpt, boundFieldAccess.ResultKind, boundFieldAccess.Type, boundFieldAccess);
			}
			break;
		}
		case BoundKind.ArrayAccess:
		{
			BoundArrayAccess boundArrayAccess = (BoundArrayAccess)originalLHS;
			if (isDynamicAssignment || !IsInvariantArray(boundArrayAccess.Expression.Type))
			{
				BoundExpression loweredExpression = VisitExpression(boundArrayAccess.Expression);
				ImmutableArray<BoundExpression> loweredIndices = VisitList(boundArrayAccess.Indices);
				return SpillArrayElementAccess(loweredExpression, loweredIndices, stores, temps);
			}
			break;
		}
		case BoundKind.DynamicMemberAccess:
			return TransformDynamicMemberAccess((BoundDynamicMemberAccess)originalLHS, stores, temps);
		case BoundKind.DynamicIndexerAccess:
			return TransformDynamicIndexerAccess((BoundDynamicIndexerAccess)originalLHS, stores, temps);
		case BoundKind.ThisReference:
		case BoundKind.Local:
		case BoundKind.PseudoVariable:
		case BoundKind.Parameter:
			return VisitExpression(originalLHS);
		case BoundKind.EventAccess:
		{
			BoundEventAccess boundEventAccess = (BoundEventAccess)originalLHS;
			BoundExpression receiver = boundEventAccess.ReceiverOpt;
			if (boundEventAccess.EventSymbol.IsWindowsRuntimeEvent)
			{
				return boundEventAccess.Update(TransformPropertyOrEventReceiver(boundEventAccess.EventSymbol, boundEventAccess.ReceiverOpt, stores, temps), boundEventAccess.EventSymbol, boundEventAccess.IsUsableAsField, boundEventAccess.ResultKind, boundEventAccess.Type);
			}
			if (TransformCompoundAssignmentFieldOrEventAccessReceiver(boundEventAccess.EventSymbol, ref receiver, stores, temps))
			{
				return MakeEventAccess(boundEventAccess.Syntax, receiver, boundEventAccess.EventSymbol, boundEventAccess.ConstantValueOpt, boundEventAccess.ResultKind, boundEventAccess.Type);
			}
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(originalLHS.Kind);
		case BoundKind.PointerIndirectionOperator:
		case BoundKind.PointerElementAccess:
		case BoundKind.FunctionPointerInvocation:
		case BoundKind.RefValueOperator:
		case BoundKind.AssignmentOperator:
		case BoundKind.ConditionalOperator:
		case BoundKind.Call:
		case BoundKind.InlineArrayAccess:
			break;
		}
		BoundExpression argument = VisitExpression(originalLHS);
		BoundLocal boundLocal = _factory.StoreToTemp(argument, out BoundAssignmentOperator store, RefKind.Ref);
		stores.Add(store);
		temps.Add(boundLocal.LocalSymbol);
		return boundLocal;
	}

	private static bool IsInvariantArray(TypeSymbol? type)
	{
		return (type as ArrayTypeSymbol)?.ElementType.IsSealed ?? false;
	}

	private BoundExpression BoxReceiver(BoundExpression rewrittenReceiver, NamedTypeSymbol memberContainingType)
	{
		return MakeConversionNode(rewrittenReceiver.Syntax, rewrittenReceiver, Conversion.Boxing, memberContainingType, @checked: false, explicitCastInCode: false, rewrittenReceiver.ConstantValueOpt);
	}

	private BoundExpression SpillArrayElementAccess(BoundExpression loweredExpression, ImmutableArray<BoundExpression> loweredIndices, ArrayBuilder<BoundExpression> stores, ArrayBuilder<LocalSymbol> temps)
	{
		BoundLocal boundLocal = _factory.StoreToTemp(loweredExpression, out BoundAssignmentOperator store);
		stores.Add(store);
		temps.Add(boundLocal.LocalSymbol);
		BoundLocal array = boundLocal;
		BoundExpression[] array2 = new BoundExpression[loweredIndices.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			if (CanChangeValueBetweenReads(loweredIndices[i]))
			{
				BoundLocal boundLocal2 = _factory.StoreToTemp(loweredIndices[i], out BoundAssignmentOperator store2);
				stores.Add(store2);
				temps.Add(boundLocal2.LocalSymbol);
				array2[i] = boundLocal2;
			}
			else
			{
				array2[i] = loweredIndices[i];
			}
		}
		return _factory.ArrayAccess(array, array2);
	}

	internal static bool CanChangeValueBetweenReads(BoundExpression expression, bool localsMayBeAssignedOrCaptured = true, bool structThisCanChangeValueBetweenReads = false)
	{
		if (expression.IsDefaultValue())
		{
			return false;
		}
		if (expression.ConstantValueOpt != null)
		{
			return !ConstantValueIsTrivial(expression.Type);
		}
		switch (expression.Kind)
		{
		case BoundKind.ThisReference:
			if (structThisCanChangeValueBetweenReads)
			{
				return ((BoundThisReference)expression).Type.IsStructType();
			}
			return false;
		case BoundKind.BaseReference:
			return false;
		case BoundKind.Literal:
			return !ConstantValueIsTrivial(expression.Type);
		case BoundKind.Parameter:
			if (!localsMayBeAssignedOrCaptured && ((BoundParameter)expression).ParameterSymbol.RefKind == RefKind.None)
			{
				return IsCapturedPrimaryConstructorParameter(expression);
			}
			return true;
		case BoundKind.Local:
			if (!localsMayBeAssignedOrCaptured)
			{
				return ((BoundLocal)expression).LocalSymbol.RefKind != RefKind.None;
			}
			return true;
		case BoundKind.TypeExpression:
			return false;
		default:
			return true;
		}
	}

	internal static bool ReadIsSideeffecting(BoundExpression expression)
	{
		if (expression.ConstantValueOpt != null)
		{
			return false;
		}
		if (expression.IsDefaultValue())
		{
			return false;
		}
		switch (expression.Kind)
		{
		case BoundKind.Literal:
		case BoundKind.ThisReference:
		case BoundKind.BaseReference:
		case BoundKind.Local:
		case BoundKind.Parameter:
		case BoundKind.Lambda:
			return false;
		case BoundKind.Conversion:
		{
			BoundConversion boundConversion = (BoundConversion)expression;
			if (!boundConversion.ConversionHasSideEffects())
			{
				return ReadIsSideeffecting(boundConversion.Operand);
			}
			return true;
		}
		case BoundKind.PassByCopy:
			return ReadIsSideeffecting(((BoundPassByCopy)expression).Expression);
		case BoundKind.ObjectCreationExpression:
			if (expression.Type.IsNullableType())
			{
				BoundObjectCreationExpression boundObjectCreationExpression = (BoundObjectCreationExpression)expression;
				if (boundObjectCreationExpression.Arguments.Length == 1)
				{
					return ReadIsSideeffecting(boundObjectCreationExpression.Arguments[0]);
				}
				return false;
			}
			return true;
		case BoundKind.Call:
		{
			BoundCall boundCall = (BoundCall)expression;
			MethodSymbol method = boundCall.Method;
			NamedTypeSymbol containingType = method.ContainingType;
			if ((object)containingType != null && containingType.IsNullableType() && (IsSpecialMember(method, SpecialMember.System_Nullable_T_GetValueOrDefault) || IsSpecialMember(method, SpecialMember.System_Nullable_T_get_HasValue)))
			{
				return ReadIsSideeffecting(boundCall.ReceiverOpt);
			}
			return true;
		}
		default:
			return true;
		}
	}

	private static bool IsSpecialMember(MethodSymbol method, SpecialMember specialMember)
	{
		method = method.OriginalDefinition;
		return method.ContainingAssembly?.GetSpecialTypeMember(specialMember) == method;
	}

	private static bool ConstantValueIsTrivial(TypeSymbol? type)
	{
		if ((object)type != null && !type.SpecialType.IsClrInteger() && !type.IsReferenceType)
		{
			return type.IsEnumType();
		}
		return true;
	}

	public override BoundNode VisitConditionalAccess(BoundConditionalAccess node)
	{
		return RewriteConditionalAccess(node, used: true);
	}

	public override BoundNode VisitLoweredConditionalAccess(BoundLoweredConditionalAccess node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_ConditionalAccess.cs", 21);
	}

	internal BoundExpression? RewriteConditionalAccess(BoundConditionalAccess node, bool used)
	{
		BoundExpression boundExpression = VisitExpression(node.Receiver);
		TypeSymbol type = boundExpression.Type;
		if (boundExpression.IsDefaultValue() && type.IsReferenceType)
		{
			return _factory.Default(node.Type);
		}
		ConditionalAccessLoweringKind conditionalAccessLoweringKind = (node.AccessExpression.Type.IsDynamic() ? ((!CanChangeValueBetweenReads(boundExpression)) ? ConditionalAccessLoweringKind.Conditional : ConditionalAccessLoweringKind.ConditionalCaptureReceiverByVal) : ConditionalAccessLoweringKind.LoweredConditionalAccess);
		BoundExpression currentConditionalAccessTarget = _currentConditionalAccessTarget;
		int id = ++_currentConditionalAccessID;
		LocalSymbol localSymbol = null;
		switch (conditionalAccessLoweringKind)
		{
		case ConditionalAccessLoweringKind.LoweredConditionalAccess:
			_currentConditionalAccessTarget = new BoundConditionalReceiver(boundExpression.Syntax, id, type);
			break;
		case ConditionalAccessLoweringKind.Conditional:
			_currentConditionalAccessTarget = boundExpression;
			break;
		case ConditionalAccessLoweringKind.ConditionalCaptureReceiverByVal:
			localSymbol = _factory.SynthesizedLocal(type);
			_currentConditionalAccessTarget = _factory.Local(localSymbol);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(conditionalAccessLoweringKind);
		}
		BoundExpression boundExpression2;
		if (used)
		{
			boundExpression2 = VisitExpression(node.AccessExpression);
		}
		else
		{
			boundExpression2 = VisitUnusedExpression(node.AccessExpression);
			if (boundExpression2 == null)
			{
				return null;
			}
		}
		_currentConditionalAccessTarget = currentConditionalAccessTarget;
		TypeSymbol type2 = VisitType(node.Type);
		TypeSymbol typeSymbol = node.Type;
		TypeSymbol type3 = boundExpression2.Type;
		if (type3.IsVoidType())
		{
			type2 = (typeSymbol = type3);
		}
		if (!TypeSymbol.Equals(type3, typeSymbol, TypeCompareKind.ConsiderEverything) && typeSymbol.IsNullableType())
		{
			boundExpression2 = _factory.New((NamedTypeSymbol)typeSymbol, boundExpression2);
		}
		BoundExpression boundExpression3;
		switch (conditionalAccessLoweringKind)
		{
		case ConditionalAccessLoweringKind.LoweredConditionalAccess:
			boundExpression3 = new BoundLoweredConditionalAccess(node.Syntax, boundExpression, type.IsNullableType() ? UnsafeGetNullableMethod(node.Syntax, boundExpression.Type, SpecialMember.System_Nullable_T_get_HasValue) : null, boundExpression2, null, id, forceCopyOfNullableValueType: true, type2);
			break;
		case ConditionalAccessLoweringKind.ConditionalCaptureReceiverByVal:
			boundExpression = _factory.MakeSequence(_factory.AssignmentExpression(_factory.Local(localSymbol), boundExpression), _factory.Local(localSymbol));
			goto case ConditionalAccessLoweringKind.Conditional;
		case ConditionalAccessLoweringKind.Conditional:
		{
			BoundExpression rewrittenCondition = _factory.IsNotNullReference(boundExpression);
			BoundExpression rewrittenConsequence = boundExpression2;
			boundExpression3 = RewriteConditionalOperator(node.Syntax, rewrittenCondition, rewrittenConsequence, _factory.Default(typeSymbol), null, typeSymbol, isRef: false);
			if (localSymbol != null)
			{
				boundExpression3 = _factory.MakeSequence(localSymbol, boundExpression3);
			}
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(conditionalAccessLoweringKind);
		}
		return boundExpression3;
	}

	public override BoundNode VisitConditionalReceiver(BoundConditionalReceiver node)
	{
		BoundExpression boundExpression = _currentConditionalAccessTarget;
		if (boundExpression.Type.IsNullableType())
		{
			boundExpression = MakeOptimizedGetValueOrDefault(node.Syntax, boundExpression);
		}
		return boundExpression;
	}

	public override BoundNode VisitConditionalOperator(BoundConditionalOperator node)
	{
		BoundExpression boundExpression = VisitExpression(node.Condition);
		BoundExpression boundExpression2 = VisitExpression(node.Consequence);
		BoundExpression boundExpression3 = VisitExpression(node.Alternative);
		if (boundExpression.ConstantValueOpt == null)
		{
			return node.Update(node.IsRef, boundExpression, boundExpression2, boundExpression3, node.ConstantValueOpt, node.NaturalTypeOpt, node.WasTargetTyped, node.Type);
		}
		return RewriteConditionalOperator(node.Syntax, boundExpression, boundExpression2, boundExpression3, node.ConstantValueOpt, node.Type, node.IsRef);
	}

	private static BoundExpression RewriteConditionalOperator(SyntaxNode syntax, BoundExpression rewrittenCondition, BoundExpression rewrittenConsequence, BoundExpression rewrittenAlternative, ConstantValue? constantValueOpt, TypeSymbol rewrittenType, bool isRef)
	{
		ConstantValue constantValueOpt2 = rewrittenCondition.ConstantValueOpt;
		if (constantValueOpt2 == ConstantValue.True)
		{
			return rewrittenConsequence;
		}
		if (constantValueOpt2 == ConstantValue.False)
		{
			return rewrittenAlternative;
		}
		return new BoundConditionalOperator(syntax, isRef, rewrittenCondition, rewrittenConsequence, rewrittenAlternative, constantValueOpt, rewrittenType, wasTargetTyped: false, rewrittenType);
	}

	public override BoundNode VisitContinueStatement(BoundContinueStatement node)
	{
		BoundStatement boundStatement = new BoundGotoStatement(node.Syntax, node.Label, node.HasErrors);
		if (Instrument && !node.WasCompilerGenerated)
		{
			boundStatement = Instrumenter.InstrumentContinueStatement(node, boundStatement);
		}
		return boundStatement;
	}

	public override BoundNode VisitConversion(BoundConversion node)
	{
		(InterpolatedStringHandlerData, ImmutableArray<BoundExpression>) tuple;
		(InterpolatedStringHandlerData, ImmutableArray<BoundExpression>) tuple2;
		InterpolatedStringHandlerData item;
		ImmutableArray<BoundExpression> item2;
		InterpolationHandlerResult interpolationHandlerResult;
		switch (node.ConversionKind)
		{
		case ConversionKind.InterpolatedString:
			return RewriteInterpolatedStringConversion(node);
		case ConversionKind.InterpolatedStringHandler:
		{
			BoundExpression operand = node.Operand;
			if (operand is BoundInterpolatedString { InterpolationData: var interpolationData } boundInterpolatedString)
			{
				if (interpolationData.HasValue)
				{
					InterpolatedStringHandlerData valueOrDefault = interpolationData.GetValueOrDefault();
					if ((object)valueOrDefault.BuilderType != null)
					{
						ImmutableArray<BoundExpression> parts = boundInterpolatedString.Parts;
						tuple = (valueOrDefault, parts);
						goto IL_00f7;
					}
				}
			}
			else if (operand is BoundBinaryOperator { InterpolatedStringHandlerData: { } interpolatedStringHandlerData } boundBinaryOperator)
			{
				tuple = (interpolatedStringHandlerData, CollectBinaryOperatorInterpolatedStringParts(boundBinaryOperator));
				goto IL_00f7;
			}
			throw ExceptionUtilities.UnexpectedValue(node.Operand.Kind);
		}
		case ConversionKind.SwitchExpression:
			return Visit(node.Operand);
		case ConversionKind.ConditionalExpression:
			return Visit(node.Operand);
		case ConversionKind.ObjectCreation:
		{
			BoundExpression boundExpression = VisitExpression(node.Operand);
			if (node.Type.IsNullableType())
			{
				return ConvertToNullable(node.Syntax, node.Type, boundExpression);
			}
			return boundExpression;
		}
		case ConversionKind.ImplicitNullable:
			if (node.Conversion.UnderlyingConversions[0].Kind == ConversionKind.CollectionExpression)
			{
				BoundExpression underlyingValue = RewriteCollectionExpressionConversion(node.Conversion.UnderlyingConversions[0], (BoundCollectionExpression)node.Operand);
				return ConvertToNullable(node.Syntax, node.Type, underlyingValue);
			}
			break;
		case ConversionKind.CollectionExpression:
			{
				return RewriteCollectionExpressionConversion(node.Conversion, (BoundCollectionExpression)node.Operand);
			}
			IL_00f7:
			tuple2 = tuple;
			item = tuple2.Item1;
			item2 = tuple2.Item2;
			interpolationHandlerResult = RewriteToInterpolatedStringHandlerPattern(item, item2, node.Operand.Syntax);
			return interpolationHandlerResult.WithFinalResult(interpolationHandlerResult.HandlerTemp);
		}
		TypeSymbol typeSymbol = VisitType(node.Type);
		bool inExpressionLambda = _inExpressionLambda;
		_inExpressionLambda = _inExpressionLambda || (node.ConversionKind == ConversionKind.AnonymousFunction && !inExpressionLambda && typeSymbol.IsExpressionTree());
		InstrumentationState.IsSuppressed = _inExpressionLambda;
		BoundExpression rewrittenOperand = VisitExpression(node.Operand);
		_inExpressionLambda = inExpressionLambda;
		InstrumentationState.IsSuppressed = _inExpressionLambda;
		BoundExpression result = MakeConversionNode(node, node.Syntax, rewrittenOperand, node.Conversion, node.Checked, node.ExplicitCastInCode, node.ConstantValueOpt, typeSymbol);
		_ = node.Type;
		return result;
	}

	public override BoundNode VisitUtf8String(BoundUtf8String node)
	{
		return MakeUtf8Span(node, GetUtf8ByteRepresentation(node));
	}

	private BoundExpression MakeUtf8Span(BoundExpression node, IReadOnlyList<byte>? bytes)
	{
		TypeSymbol type = ((NamedTypeSymbol)node.Type).TypeArgumentsWithAnnotationsNoUseSiteDiagnostics.Single().Type;
		SyntaxNode syntax = _factory.Syntax;
		_factory.Syntax = node.Syntax;
		int length = 0;
		ArrayTypeSymbol arrayTypeSymbol = ArrayTypeSymbol.CreateSZArray(_compilation.Assembly, TypeWithAnnotations.Create(type));
		BoundExpression boundExpression = ((bytes == null) ? BadExpression(node.Syntax, arrayTypeSymbol, ImmutableArray<BoundExpression>.Empty) : MakeUnderlyingArrayForUtf8Span(node.Syntax, arrayTypeSymbol, bytes, out length));
		BoundExpression result = (TryGetWellKnownTypeMember<MethodSymbol>(node.Syntax, WellKnownMember.System_ReadOnlySpan_T__ctor_Array_Start_Length, out MethodSymbol symbol) ? new BoundObjectCreationExpression(node.Syntax, symbol.AsMember((NamedTypeSymbol)node.Type), boundExpression, _factory.Literal(0), _factory.Literal(length)) : BadExpression(node.Syntax, node.Type, ImmutableArray<BoundExpression>.Empty));
		_factory.Syntax = syntax;
		return result;
	}

	private byte[]? GetUtf8ByteRepresentation(BoundUtf8String node)
	{
		if (node.Value.TryGetUtf8ByteRepresentation(out byte[] result, out string error))
		{
			return result;
		}
		_diagnostics.Add(ErrorCode.ERR_CannotBeConvertedToUtf8, node.Syntax.Location, error);
		return null;
	}

	private BoundArrayCreation MakeUnderlyingArrayForUtf8Span(SyntaxNode syntax, ArrayTypeSymbol byteArray, IReadOnlyList<byte> bytes, out int length)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(bytes.Count + 1);
		foreach (byte @byte in bytes)
		{
			instance.Add(_factory.Literal(@byte));
		}
		length = instance.Count;
		instance.Add(_factory.Literal((byte)0));
		return new BoundArrayCreation(syntax, ImmutableArray.Create((BoundExpression)_factory.Literal(instance.Count)), new BoundArrayInitialization(syntax, isInferred: false, instance.ToImmutableAndFree()), byteArray);
	}

	private BoundExpression VisitUtf8Addition(BoundBinaryOperator node)
	{
		ArrayBuilder<byte> instance = ArrayBuilder<byte>.GetInstance();
		bool flag = false;
		ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance();
		instance2.Add(node);
		while (instance2.Count != 0)
		{
			BoundExpression boundExpression = instance2.Pop();
			if (!(boundExpression is BoundUtf8String node2))
			{
				if (boundExpression is BoundBinaryOperator boundBinaryOperator)
				{
					instance2.Push(boundBinaryOperator.Right);
					instance2.Push(boundBinaryOperator.Left);
					continue;
				}
				throw ExceptionUtilities.UnexpectedValue(boundExpression);
			}
			byte[] utf8ByteRepresentation = GetUtf8ByteRepresentation(node2);
			if (utf8ByteRepresentation == null)
			{
				flag = true;
			}
			else if (!flag)
			{
				instance.AddRange(utf8ByteRepresentation);
			}
		}
		instance2.Free();
		BoundExpression result = MakeUtf8Span(node, flag ? null : instance);
		instance.Free();
		return result;
	}

	private static bool IsFloatingPointExpressionOfUnknownPrecision(BoundExpression rewrittenNode)
	{
		if (rewrittenNode == null)
		{
			return false;
		}
		if (rewrittenNode.ConstantValueOpt != null)
		{
			return false;
		}
		TypeSymbol type = rewrittenNode.Type;
		if (type.SpecialType != SpecialType.System_Double && type.SpecialType != SpecialType.System_Single)
		{
			return false;
		}
		switch (rewrittenNode.Kind)
		{
		case BoundKind.Sequence:
			return IsFloatingPointExpressionOfUnknownPrecision(((BoundSequence)rewrittenNode).Value);
		case BoundKind.Conversion:
		{
			BoundConversion boundConversion = (BoundConversion)rewrittenNode;
			if (boundConversion.ConversionKind == ConversionKind.Identity)
			{
				return !boundConversion.ExplicitCastInCode;
			}
			return false;
		}
		default:
			return true;
		}
	}

	private BoundExpression MakeConversionNode(BoundConversion? oldNodeOpt, SyntaxNode syntax, BoundExpression rewrittenOperand, Conversion conversion, bool @checked, bool explicitCastInCode, ConstantValue? constantValueOpt, TypeSymbol rewrittenType)
	{
		BoundExpression boundExpression = MakeConversionNodeCore(oldNodeOpt, syntax, rewrittenOperand, conversion, @checked, explicitCastInCode, constantValueOpt, rewrittenType);
		if ((!_inExpressionLambda & explicitCastInCode) && IsFloatingPointExpressionOfUnknownPrecision(boundExpression))
		{
			boundExpression = new BoundConversion(syntax, boundExpression, Conversion.Identity, isBaseConversion: false, @checked: false, explicitCastInCode: true, null, null, boundExpression.Type);
		}
		return boundExpression;
	}

	private BoundExpression MakeConversionNodeCore(BoundConversion? oldNodeOpt, SyntaxNode syntax, BoundExpression rewrittenOperand, Conversion conversion, bool @checked, bool explicitCastInCode, ConstantValue? constantValueOpt, TypeSymbol rewrittenType)
	{
		if (_inExpressionLambda && !conversion.IsUserDefined)
		{
			@checked = @checked && NeedsCheckedConversionInExpressionTree(rewrittenOperand.Type, rewrittenType, explicitCastInCode);
		}
		ConversionGroup conversionGroupOpt;
		switch (conversion.Kind)
		{
		case ConversionKind.Identity:
			if (!_inExpressionLambda && rewrittenOperand.Type.Equals(rewrittenType, TypeCompareKind.ConsiderEverything))
			{
				if (!explicitCastInCode)
				{
					return rewrittenOperand;
				}
				if (!IsFloatingPointExpressionOfUnknownPrecision(rewrittenOperand))
				{
					return rewrittenOperand;
				}
			}
			break;
		case ConversionKind.ImplicitUserDefined:
		case ConversionKind.ExplicitUserDefined:
			return RewriteUserDefinedConversion(syntax, rewrittenOperand, conversion, @checked, rewrittenType);
		case ConversionKind.IntPtr:
			return RewriteIntPtrConversion(syntax, rewrittenOperand, conversion, @checked, explicitCastInCode, constantValueOpt, rewrittenType);
		case ConversionKind.ImplicitNullable:
		case ConversionKind.ExplicitNullable:
			return RewriteNullableConversion(syntax, rewrittenOperand, conversion, @checked, explicitCastInCode, rewrittenType);
		case ConversionKind.Boxing:
			if (!_inExpressionLambda)
			{
				if (NullableNeverHasValue(rewrittenOperand))
				{
					return new BoundDefaultExpression(syntax, rewrittenType);
				}
				BoundExpression boundExpression = NullableAlwaysHasValue(rewrittenOperand);
				if (boundExpression != null)
				{
					return MakeConversionNode(oldNodeOpt, syntax, boundExpression, conversion, @checked, explicitCastInCode, constantValueOpt, rewrittenType);
				}
			}
			break;
		case ConversionKind.NullLiteral:
		case ConversionKind.DefaultLiteral:
			if (!_inExpressionLambda || !explicitCastInCode)
			{
				return new BoundDefaultExpression(syntax, rewrittenType);
			}
			break;
		case ConversionKind.ImplicitReference:
		case ConversionKind.ExplicitReference:
			if (rewrittenOperand.IsDefaultValue() && (!_inExpressionLambda || !explicitCastInCode))
			{
				return new BoundDefaultExpression(syntax, rewrittenType);
			}
			break;
		case ConversionKind.ImplicitConstant:
			conversion = Conversion.ExplicitNumeric;
			@checked = false;
			goto case ConversionKind.ImplicitNumeric;
		case ConversionKind.ImplicitNumeric:
		case ConversionKind.ExplicitNumeric:
			if (rewrittenOperand.IsDefaultValue() && (!_inExpressionLambda || !explicitCastInCode))
			{
				return new BoundDefaultExpression(syntax, rewrittenType);
			}
			if (rewrittenType.SpecialType == SpecialType.System_Decimal || rewrittenOperand.Type.SpecialType == SpecialType.System_Decimal)
			{
				return RewriteDecimalConversion(syntax, rewrittenOperand, rewrittenOperand.Type, rewrittenType, @checked, conversion.Kind.IsImplicitConversion(), constantValueOpt);
			}
			break;
		case ConversionKind.ImplicitTupleLiteral:
		case ConversionKind.ExplicitTupleLiteral:
			return rewrittenOperand;
		case ConversionKind.ImplicitThrow:
		{
			BoundThrowExpression boundThrowExpression = (BoundThrowExpression)rewrittenOperand;
			return _factory.ThrowExpression(boundThrowExpression.Expression, rewrittenType);
		}
		case ConversionKind.ImplicitEnumeration:
			if (rewrittenType.IsNullableType())
			{
				BoundExpression rewrittenOperand2 = MakeConversionNode(oldNodeOpt, syntax, rewrittenOperand, conversion, @checked, explicitCastInCode, constantValueOpt, rewrittenType.GetNullableUnderlyingType());
				Conversion implicitNullableWithIdentityUnderlying = Conversion.ImplicitNullableWithIdentityUnderlying;
				return MakeConversionNode(oldNodeOpt, syntax, rewrittenOperand2, implicitNullableWithIdentityUnderlying, @checked, explicitCastInCode, constantValueOpt, rewrittenType);
			}
			goto case ConversionKind.ExplicitEnumeration;
		case ConversionKind.ExplicitEnumeration:
			if (!rewrittenType.IsNullableType() && rewrittenOperand.IsDefaultValue() && (!_inExpressionLambda || !explicitCastInCode))
			{
				return new BoundDefaultExpression(syntax, rewrittenType);
			}
			if (rewrittenType.SpecialType == SpecialType.System_Decimal)
			{
				NamedTypeSymbol enumUnderlyingType = rewrittenOperand.Type.GetEnumUnderlyingType();
				rewrittenOperand = MakeConversionNode(rewrittenOperand, enumUnderlyingType, @checked: false);
				return RewriteDecimalConversion(syntax, rewrittenOperand, enumUnderlyingType, rewrittenType, @checked, isImplicit: false, constantValueOpt);
			}
			if (rewrittenOperand.Type.SpecialType == SpecialType.System_Decimal)
			{
				NamedTypeSymbol enumUnderlyingType2 = rewrittenType.GetEnumUnderlyingType();
				BoundExpression operand = RewriteDecimalConversion(syntax, rewrittenOperand, rewrittenOperand.Type, enumUnderlyingType2, @checked, isImplicit: false, constantValueOpt);
				Conversion conversion3 = conversion;
				conversionGroupOpt = oldNodeOpt?.ConversionGroupOpt;
				return new BoundConversion(syntax, operand, conversion3, isBaseConversion: false, @checked: false, explicitCastInCode, constantValueOpt, conversionGroupOpt, rewrittenType);
			}
			break;
		case ConversionKind.ImplicitDynamic:
		case ConversionKind.ExplicitDynamic:
			return _dynamicFactory.MakeDynamicConversion(rewrittenOperand, explicitCastInCode || conversion.Kind == ConversionKind.ExplicitDynamic, conversion.IsArrayIndex, @checked, rewrittenType).ToExpression();
		case ConversionKind.ImplicitTuple:
		case ConversionKind.ExplicitTuple:
			return RewriteTupleConversion(syntax, rewrittenOperand, conversion, @checked, explicitCastInCode, (NamedTypeSymbol)rewrittenType);
		case ConversionKind.MethodGroup:
		{
			if (oldNodeOpt != null)
			{
				TypeSymbol type3 = oldNodeOpt.Type;
				if ((object)type3 != null && type3.TypeKind == TypeKind.FunctionPointer)
				{
					BoundMethodGroup boundMethodGroup = (BoundMethodGroup)rewrittenOperand;
					MethodSymbol symbolOpt = oldNodeOpt.SymbolOpt;
					return new BoundFunctionPointerLoad(oldNodeOpt.Syntax, symbolOpt, (!symbolOpt.IsStatic || (!symbolOpt.IsAbstract && !symbolOpt.IsVirtual)) ? null : boundMethodGroup.ReceiverOpt?.Type, type3, hasErrors: false);
				}
			}
			BoundMethodGroup boundMethodGroup2 = (BoundMethodGroup)rewrittenOperand;
			MethodSymbol symbolOpt2 = oldNodeOpt.SymbolOpt;
			SyntaxNode syntax2 = _factory.Syntax;
			_factory.Syntax = (boundMethodGroup2.ReceiverOpt ?? boundMethodGroup2).Syntax;
			BoundExpression argument = ((!symbolOpt2.RequiresInstanceReceiver && !oldNodeOpt.IsExtensionMethod && !symbolOpt2.IsAbstract && !symbolOpt2.IsVirtual) ? _factory.Type(symbolOpt2.ContainingType) : boundMethodGroup2.ReceiverOpt);
			_factory.Syntax = syntax2;
			BoundDelegateCreationExpression boundDelegateCreationExpression = new BoundDelegateCreationExpression(syntax, argument, symbolOpt2, oldNodeOpt.IsExtensionMethod, wasTargetTyped: false, rewrittenType);
			EnsureParamCollectionAttributeExists(rewrittenOperand.Syntax, rewrittenType);
			if (_factory.Compilation.LanguageVersion >= MessageID.IDS_FeatureCacheStaticMethodGroupConversion.RequiredVersion() && !_inExpressionLambda && _factory.TopLevelMethod.MethodKind != MethodKind.StaticConstructor && DelegateCacheRewriter.CanRewrite(boundDelegateCreationExpression))
			{
				return (_lazyDelegateCacheRewriter ?? (_lazyDelegateCacheRewriter = new DelegateCacheRewriter(_factory, _topLevelMethodOrdinal))).Rewrite(boundDelegateCreationExpression);
			}
			return boundDelegateCreationExpression;
		}
		case ConversionKind.InlineArray:
		{
			NamedTypeSymbol namedTypeSymbol2 = (NamedTypeSymbol)rewrittenType;
			MethodSymbol methodSymbol4 = ((!namedTypeSymbol2.OriginalDefinition.Equals(_compilation.GetWellKnownType(WellKnownType.System_ReadOnlySpan_T), TypeCompareKind.AllIgnoreOptions)) ? _factory.ModuleBuilderOpt.EnsureInlineArrayAsSpanExists(syntax, namedTypeSymbol2.OriginalDefinition, _factory.SpecialType(SpecialType.System_Int32), _diagnostics.DiagnosticBag) : _factory.ModuleBuilderOpt.EnsureInlineArrayAsReadOnlySpanExists(syntax, namedTypeSymbol2.OriginalDefinition, _factory.SpecialType(SpecialType.System_Int32), _diagnostics.DiagnosticBag));
			methodSymbol4 = methodSymbol4.Construct(rewrittenOperand.Type, namedTypeSymbol2.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics.Single().Type);
			rewrittenOperand.Type.HasInlineArrayAttribute(out var length);
			return _factory.Call(null, methodSymbol4, rewrittenOperand, _factory.Literal(length), useStrictArgumentRefKinds: true);
		}
		case ConversionKind.ImplicitSpan:
		case ConversionKind.ExplicitSpan:
		{
			TypeSymbol type = rewrittenOperand.Type;
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)rewrittenType;
			if (type is ArrayTypeSymbol)
			{
				MethodSymbol methodSymbol = (Binder.TryFindImplicitOperatorFromArray(namedTypeSymbol.OriginalDefinition) ?? throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_Conversion.cs", 628)).AsMember(namedTypeSymbol);
				TypeSymbol type2 = methodSymbol.ParameterTypesWithAnnotations[0].Type;
				Conversion conversion2 = _factory.ClassifyEmitConversion(rewrittenOperand, type2);
				rewrittenOperand = _factory.Convert(type2, rewrittenOperand, conversion2);
				if (!_inExpressionLambda && _compilation.IsReadOnlySpanType(namedTypeSymbol))
				{
					return new BoundReadOnlySpanFromArray(syntax, rewrittenOperand, methodSymbol, namedTypeSymbol)
					{
						WasCompilerGenerated = true
					};
				}
				return _factory.Call(null, methodSymbol, rewrittenOperand);
			}
			if (type.IsSpan())
			{
				MethodSymbol methodSymbol2 = (Binder.TryFindImplicitOperatorFromSpan(type.OriginalDefinition, namedTypeSymbol.OriginalDefinition) ?? throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_Conversion.cs", 657)).AsMember((NamedTypeSymbol)type);
				rewrittenOperand = _factory.Call(null, methodSymbol2, rewrittenOperand);
				if (Binder.NeedsSpanCastUp(type, namedTypeSymbol))
				{
					MethodSymbol? obj = Binder.TryFindCastUpMethod(methodSymbol2.ReturnType.OriginalDefinition, namedTypeSymbol.OriginalDefinition) ?? throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_Conversion.cs", 669);
					TypeWithAnnotations typeWithAnnotations = ((NamedTypeSymbol)type).TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[0];
					MethodSymbol method = obj.AsMember(namedTypeSymbol).Construct(ImmutableCollectionsMarshal.AsImmutableArray(new TypeWithAnnotations[1] { typeWithAnnotations }));
					return _factory.Call(null, method, rewrittenOperand);
				}
				return rewrittenOperand;
			}
			if (type.IsReadOnlySpan())
			{
				MethodSymbol? obj2 = Binder.TryFindCastUpMethod(type.OriginalDefinition, namedTypeSymbol.OriginalDefinition) ?? throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_Conversion.cs", 690);
				TypeWithAnnotations typeWithAnnotations2 = ((NamedTypeSymbol)type).TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[0];
				MethodSymbol method2 = obj2.AsMember(namedTypeSymbol).Construct(ImmutableCollectionsMarshal.AsImmutableArray(new TypeWithAnnotations[1] { typeWithAnnotations2 }));
				return _factory.Call(null, method2, rewrittenOperand);
			}
			if (type.IsStringType())
			{
				MethodSymbol methodSymbol3 = Binder.TryFindAsSpanCharMethod(_compilation, namedTypeSymbol);
				if ((object)methodSymbol3 == null)
				{
					throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_Conversion.cs", 708);
				}
				return _factory.Call(null, methodSymbol3, rewrittenOperand);
			}
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_Conversion.cs", 716);
		}
		}
		if (oldNodeOpt == null)
		{
			return new BoundConversion(syntax, rewrittenOperand, conversion, isBaseConversion: false, @checked, explicitCastInCode, constantValueOpt, null, rewrittenType);
		}
		BoundExpression operand2 = rewrittenOperand;
		Conversion conversion4 = conversion;
		bool isBaseConversion = oldNodeOpt.IsBaseConversion;
		bool num = @checked;
		conversionGroupOpt = oldNodeOpt.ConversionGroupOpt;
		return oldNodeOpt.Update(operand2, conversion4, isBaseConversion, num, explicitCastInCode, constantValueOpt, conversionGroupOpt, rewrittenType);
	}

	private void EnsureParamCollectionAttributeExists(SyntaxNode node, TypeSymbol delegateType)
	{
		if (delegateType.IsAnonymousType && delegateType.ContainingModule == _compilation.SourceModule)
		{
			MethodSymbol methodSymbol = delegateType.DelegateInvokeMethod();
			if ((object)methodSymbol != null && methodSymbol.Parameters.Any((ParameterSymbol p) => p.IsParamsCollection))
			{
				_factory.ModuleBuilderOpt.EnsureParamCollectionAttributeExists(_diagnostics, node.Location);
			}
		}
	}

	private static bool NeedsCheckedConversionInExpressionTree(TypeSymbol? source, TypeSymbol target, bool explicitCastInCode)
	{
		if ((object)source == null)
		{
			return false;
		}
		SpecialType specialType = GetUnderlyingSpecialType(source);
		SpecialType specialType2 = GetUnderlyingSpecialType(target);
		if ((explicitCastInCode || specialType != specialType2) && IsInRange(specialType, SpecialType.System_Char, SpecialType.System_Double))
		{
			return IsInRange(specialType2, SpecialType.System_Char, SpecialType.System_UInt64);
		}
		return false;
		static SpecialType GetUnderlyingSpecialType(TypeSymbol type)
		{
			return type.StrippedType().EnumUnderlyingTypeOrSelf().SpecialType;
		}
		static bool IsInRange(SpecialType type, SpecialType low, SpecialType high)
		{
			if (low <= type)
			{
				return type <= high;
			}
			return false;
		}
	}

	private BoundExpression MakeConversionNode(BoundExpression rewrittenOperand, TypeSymbol rewrittenType, bool @checked, bool acceptFailingConversion = false, bool markAsChecked = false)
	{
		Conversion conversion = MakeConversion(rewrittenOperand, rewrittenType, @checked, _compilation, _diagnostics, acceptFailingConversion);
		if (!conversion.IsValid)
		{
			return _factory.NullOrDefault(rewrittenType);
		}
		return MakeConversionNode(rewrittenOperand.Syntax, rewrittenOperand, conversion, rewrittenType, @checked);
	}

	private static Conversion MakeConversion(BoundExpression rewrittenOperand, TypeSymbol rewrittenType, bool @checked, CSharpCompilation compilation, BindingDiagnosticBag diagnostics, bool acceptFailingConversion)
	{
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, compilation.Assembly);
		Conversion result = compilation.Conversions.ClassifyConversionFromType(rewrittenOperand.Type, rewrittenType, @checked, ref useSiteInfo);
		diagnostics.Add(rewrittenOperand.Syntax, useSiteInfo);
		if (!result.IsValid && (!acceptFailingConversion || (rewrittenOperand.Type.SpecialType != SpecialType.System_Decimal && rewrittenOperand.Type.SpecialType != SpecialType.System_DateTime)))
		{
			diagnostics.Add(ErrorCode.ERR_NoImplicitConv, rewrittenOperand.Syntax.Location, rewrittenOperand.Type, rewrittenType);
		}
		return result;
	}

	internal BoundExpression MakeConversionNode(SyntaxNode syntax, BoundExpression rewrittenOperand, Conversion conversion, TypeSymbol rewrittenType, bool @checked, bool explicitCastInCode = false, ConstantValue? constantValueOpt = null)
	{
		if (conversion.Kind.IsUserDefinedConversion())
		{
			if (!TypeSymbol.Equals(rewrittenOperand.Type, conversion.BestUserDefinedConversionAnalysis.FromType, TypeCompareKind.ConsiderEverything))
			{
				rewrittenOperand = MakeConversionNode(syntax, rewrittenOperand, conversion.UserDefinedFromConversion, conversion.BestUserDefinedConversionAnalysis.FromType, @checked);
			}
			if (!TypeSymbol.Equals(rewrittenOperand.Type, conversion.Method.GetParameterType(0), TypeCompareKind.ConsiderEverything))
			{
				rewrittenOperand = MakeConversionNode(rewrittenOperand, conversion.BestUserDefinedConversionAnalysis.FromType, @checked, acceptFailingConversion: false, markAsChecked: true);
			}
			TypeSymbol typeSymbol = conversion.Method.ReturnType;
			if (rewrittenOperand.Type.IsNullableType() && conversion.Method.GetParameterType(0).Equals(rewrittenOperand.Type.GetNullableUnderlyingType(), TypeCompareKind.AllIgnoreOptions) && !typeSymbol.IsNullableType() && typeSymbol.IsValueType)
			{
				typeSymbol = ((NamedTypeSymbol)rewrittenOperand.Type.OriginalDefinition).Construct(typeSymbol);
			}
			BoundExpression boundExpression = RewriteUserDefinedConversion(syntax, rewrittenOperand, conversion, @checked, typeSymbol);
			if (!TypeSymbol.Equals(boundExpression.Type, conversion.BestUserDefinedConversionAnalysis.ToType, TypeCompareKind.ConsiderEverything))
			{
				boundExpression = MakeConversionNode(boundExpression, conversion.BestUserDefinedConversionAnalysis.ToType, @checked, acceptFailingConversion: false, markAsChecked: true);
			}
			if (!TypeSymbol.Equals(boundExpression.Type, rewrittenType, TypeCompareKind.ConsiderEverything))
			{
				boundExpression = MakeConversionNode(syntax, boundExpression, conversion.UserDefinedToConversion, rewrittenType, @checked);
			}
			return boundExpression;
		}
		return MakeConversionNode(null, syntax, rewrittenOperand, conversion, @checked, explicitCastInCode, constantValueOpt, rewrittenType);
	}

	private BoundExpression RewriteTupleConversion(SyntaxNode syntax, BoundExpression rewrittenOperand, Conversion conversion, bool @checked, bool explicitCastInCode, NamedTypeSymbol rewrittenType)
	{
		ImmutableArray<TypeWithAnnotations> tupleElementTypesWithAnnotations = rewrittenType.TupleElementTypesWithAnnotations;
		int length = tupleElementTypesWithAnnotations.Length;
		ImmutableArray<FieldSymbol> tupleElements = ((NamedTypeSymbol)rewrittenOperand.Type).TupleElements;
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(length);
		BoundLocal boundLocal = _factory.StoreToTemp(rewrittenOperand, out BoundAssignmentOperator store);
		ImmutableArray<Conversion> underlyingConversions = conversion.UnderlyingConversions;
		for (int i = 0; i < length; i++)
		{
			BoundExpression rewrittenOperand2 = MakeTupleFieldAccessAndReportUseSiteDiagnostics(boundLocal, syntax, tupleElements[i]);
			BoundExpression item = MakeConversionNode(syntax, rewrittenOperand2, underlyingConversions[i], tupleElementTypesWithAnnotations[i].Type, @checked, explicitCastInCode);
			instance.Add(item);
		}
		BoundExpression boundExpression = MakeTupleCreationExpression(syntax, rewrittenType, instance.ToImmutableAndFree());
		return _factory.MakeSequence(boundLocal.LocalSymbol, store, boundExpression);
	}

	internal static bool NullableNeverHasValue(BoundExpression expression)
	{
		return expression.NullableNeverHasValue();
	}

	internal static BoundExpression? NullableAlwaysHasValue(BoundExpression expression)
	{
		if (!expression.Type.IsNullableType())
		{
			return null;
		}
		if (expression is BoundObjectCreationExpression boundObjectCreationExpression)
		{
			ImmutableArray<BoundExpression> arguments = boundObjectCreationExpression.Arguments;
			if (arguments.Length == 1)
			{
				return arguments[0];
			}
		}
		else if (expression is BoundConversion { Conversion: { Kind: ConversionKind.ImplicitNullable } conversion } boundConversion)
		{
			BoundExpression operand = boundConversion.Operand;
			if (operand.Type.Equals(expression.Type.StrippedType(), TypeCompareKind.AllIgnoreOptions))
			{
				return operand;
			}
			ImmutableArray<Conversion> underlyingConversions = conversion.UnderlyingConversions;
			BoundExpression boundExpression = operand;
			if (underlyingConversions.Length == 1 && underlyingConversions[0].Kind == ConversionKind.ImplicitTuple && !boundExpression.Type.IsNullableType())
			{
				return new BoundConversion(expression.Syntax, boundExpression, underlyingConversions[0], boundConversion.Checked, boundConversion.ExplicitCastInCode, null, null, boundConversion.Type.StrippedType(), boundConversion.HasErrors);
			}
		}
		return null;
	}

	private BoundExpression RewriteNullableConversion(SyntaxNode syntax, BoundExpression rewrittenOperand, Conversion conversion, bool @checked, bool explicitCastInCode, TypeSymbol rewrittenType)
	{
		if (_inExpressionLambda)
		{
			return RewriteLiftedConversionInExpressionTree(syntax, rewrittenOperand, conversion, @checked, explicitCastInCode, rewrittenType);
		}
		TypeSymbol type = rewrittenOperand.Type;
		if (type.IsNullableType() && rewrittenType.IsNullableType())
		{
			return RewriteFullyLiftedBuiltInConversion(syntax, rewrittenOperand, conversion, @checked, rewrittenType);
		}
		if (rewrittenType.IsNullableType())
		{
			BoundExpression boundExpression = MakeConversionNode(syntax, rewrittenOperand, conversion.UnderlyingConversions[0], rewrittenType.GetNullableUnderlyingType(), @checked);
			MethodSymbol constructor = UnsafeGetNullableMethod(syntax, rewrittenType, SpecialMember.System_Nullable_T__ctor);
			return new BoundObjectCreationExpression(syntax, constructor, boundExpression);
		}
		BoundExpression boundExpression2 = NullableAlwaysHasValue(rewrittenOperand);
		if (boundExpression2 == null)
		{
			MethodSymbol method = UnsafeGetNullableMethod(syntax, type, SpecialMember.System_Nullable_T_get_Value);
			boundExpression2 = BoundCall.Synthesized(syntax, rewrittenOperand, ThreeState.Unknown, method);
		}
		return MakeConversionNode(syntax, boundExpression2, conversion.UnderlyingConversions[0], rewrittenType, @checked);
	}

	private BoundExpression RewriteLiftedConversionInExpressionTree(SyntaxNode syntax, BoundExpression rewrittenOperand, Conversion conversion, bool @checked, bool explicitCastInCode, TypeSymbol rewrittenType)
	{
		TypeSymbol type = rewrittenOperand.Type;
		ConversionGroup conversionGroupOpt = null;
		TypeSymbol typeSymbol = type.StrippedType();
		TypeSymbol typeSymbol2 = rewrittenType.StrippedType();
		if (!TypeSymbol.Equals(typeSymbol, typeSymbol2, TypeCompareKind.ConsiderEverything) && (typeSymbol.SpecialType == SpecialType.System_Decimal || typeSymbol2.SpecialType == SpecialType.System_Decimal))
		{
			TypeSymbol typeSymbol3 = typeSymbol;
			TypeSymbol typeTo = typeSymbol2;
			if (typeSymbol.IsEnumType())
			{
				typeSymbol3 = typeSymbol.GetEnumUnderlyingType();
				type = (type.IsNullableType() ? ((NamedTypeSymbol)type.OriginalDefinition).Construct(typeSymbol3) : typeSymbol3);
				rewrittenOperand = BoundConversion.SynthesizedNonUserDefined(syntax, rewrittenOperand, Conversion.ImplicitEnumeration, type);
			}
			else if (typeSymbol2.IsEnumType())
			{
				typeTo = typeSymbol2.GetEnumUnderlyingType();
			}
			if (!TryGetSpecialTypeMethod(syntax, DecimalConversionMethod(typeSymbol3, typeTo), out MethodSymbol method))
			{
				return BadExpression(syntax, rewrittenType, rewrittenOperand);
			}
			ConversionKind kind = (conversion.Kind.IsImplicitConversion() ? ConversionKind.ImplicitUserDefined : ConversionKind.ExplicitUserDefined);
			return new BoundConversion(syntax, rewrittenOperand, new Conversion(kind, method, isExtensionMethod: false), @checked, explicitCastInCode, conversionGroupOpt, null, rewrittenType);
		}
		return new BoundConversion(syntax, rewrittenOperand, conversion, @checked, explicitCastInCode, conversionGroupOpt, null, rewrittenType);
	}

	private BoundExpression RewriteFullyLiftedBuiltInConversion(SyntaxNode syntax, BoundExpression operand, Conversion conversion, bool @checked, TypeSymbol type)
	{
		BoundExpression boundExpression = OptimizeLiftedBuiltInConversion(syntax, operand, conversion, @checked, type);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		BoundLocal boundLocal = _factory.StoreToTemp(operand, out BoundAssignmentOperator store);
		if (!TryGetNullableMethod(syntax, boundLocal.Type, SpecialMember.System_Nullable_T_GetValueOrDefault, out MethodSymbol result))
		{
			return BadExpression(syntax, type, operand);
		}
		BoundExpression rewrittenCondition = MakeNullableHasValue(syntax, boundLocal);
		BoundExpression rewrittenConsequence = new BoundObjectCreationExpression(syntax, UnsafeGetNullableMethod(syntax, type, SpecialMember.System_Nullable_T__ctor), MakeConversionNode(syntax, BoundCall.Synthesized(syntax, boundLocal, ThreeState.Unknown, result), conversion.UnderlyingConversions[0], type.GetNullableUnderlyingType(), @checked));
		BoundExpression rewrittenAlternative = new BoundDefaultExpression(syntax, type);
		BoundExpression value = RewriteConditionalOperator(syntax, rewrittenCondition, rewrittenConsequence, rewrittenAlternative, null, type, isRef: false);
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal.LocalSymbol), ImmutableArray.Create((BoundExpression)store), value, type);
	}

	private BoundExpression? OptimizeLiftedUserDefinedConversion(SyntaxNode syntax, BoundExpression operand, Conversion conversion, TypeSymbol type)
	{
		if (NullableNeverHasValue(operand))
		{
			return new BoundDefaultExpression(syntax, type);
		}
		BoundExpression boundExpression = NullableAlwaysHasValue(operand);
		if (boundExpression != null)
		{
			TypeParameterSymbol constrainedToTypeOpt = conversion.ConstrainedToTypeOpt;
			return MakeLiftedUserDefinedConversionConsequence(BoundCall.Synthesized(syntax, ((object)constrainedToTypeOpt == null) ? null : new BoundTypeExpression(syntax, null, constrainedToTypeOpt), ThreeState.Unknown, conversion.Method, boundExpression), type);
		}
		return DistributeLiftedConversionIntoLiftedOperand(syntax, operand, conversion, @checked: false, type);
	}

	private BoundExpression? OptimizeLiftedBuiltInConversion(SyntaxNode syntax, BoundExpression operand, Conversion conversion, bool @checked, TypeSymbol type)
	{
		if (NullableNeverHasValue(operand))
		{
			return new BoundDefaultExpression(syntax, type);
		}
		BoundExpression boundExpression = NullableAlwaysHasValue(operand);
		if (boundExpression != null)
		{
			return new BoundObjectCreationExpression(syntax, UnsafeGetNullableMethod(syntax, type, SpecialMember.System_Nullable_T__ctor), MakeConversionNode(syntax, boundExpression, conversion.UnderlyingConversions[0], type.GetNullableUnderlyingType(), @checked));
		}
		return DistributeLiftedConversionIntoLiftedOperand(syntax, operand, conversion, @checked, type);
	}

	private BoundExpression? DistributeLiftedConversionIntoLiftedOperand(SyntaxNode syntax, BoundExpression operand, Conversion conversion, bool @checked, TypeSymbol type)
	{
		if (operand.Kind == BoundKind.Sequence)
		{
			BoundSequence boundSequence = (BoundSequence)operand;
			if (boundSequence.Value.Kind == BoundKind.ConditionalOperator)
			{
				BoundConditionalOperator boundConditionalOperator = (BoundConditionalOperator)boundSequence.Value;
				if (NullableAlwaysHasValue(boundConditionalOperator.Consequence) != null && NullableNeverHasValue(boundConditionalOperator.Alternative))
				{
					return new BoundSequence(boundSequence.Syntax, boundSequence.Locals, boundSequence.SideEffects, RewriteConditionalOperator(boundConditionalOperator.Syntax, boundConditionalOperator.Condition, MakeConversionNode(null, syntax, boundConditionalOperator.Consequence, conversion, @checked, explicitCastInCode: false, null, type), MakeConversionNode(null, syntax, boundConditionalOperator.Alternative, conversion, @checked, explicitCastInCode: false, null, type), null, type, isRef: false), type);
				}
			}
		}
		return null;
	}

	private BoundExpression RewriteUserDefinedConversion(SyntaxNode syntax, BoundExpression rewrittenOperand, Conversion conversion, bool @checked, TypeSymbol rewrittenType)
	{
		if (rewrittenOperand.Type.IsNullableType())
		{
			TypeSymbol parameterType = conversion.Method.GetParameterType(0);
			if (parameterType.Equals(rewrittenOperand.Type.GetNullableUnderlyingType(), TypeCompareKind.AllIgnoreOptions) && !parameterType.IsNullableType() && parameterType.IsValueType)
			{
				return RewriteLiftedUserDefinedConversion(syntax, rewrittenOperand, conversion, @checked, rewrittenType);
			}
		}
		if (_inExpressionLambda)
		{
			return BoundConversion.Synthesized(syntax, rewrittenOperand, conversion, @checked, explicitCastInCode: true, null, null, rewrittenType);
		}
		if (rewrittenOperand.Type.IsArray() && _compilation.IsReadOnlySpanType(rewrittenType))
		{
			return new BoundReadOnlySpanFromArray(syntax, rewrittenOperand, conversion.Method, rewrittenType)
			{
				WasCompilerGenerated = true
			};
		}
		TypeParameterSymbol constrainedToTypeOpt = conversion.ConstrainedToTypeOpt;
		return BoundCall.Synthesized(syntax, ((object)constrainedToTypeOpt == null) ? null : new BoundTypeExpression(syntax, null, constrainedToTypeOpt), ThreeState.Unknown, conversion.Method, rewrittenOperand);
	}

	private BoundExpression MakeLiftedUserDefinedConversionConsequence(BoundCall call, TypeSymbol resultType)
	{
		if (call.Method.ReturnType.IsValidNullableTypeArgument())
		{
			MethodSymbol constructor = UnsafeGetNullableMethod(call.Syntax, resultType, SpecialMember.System_Nullable_T__ctor);
			return new BoundObjectCreationExpression(call.Syntax, constructor, call);
		}
		return call;
	}

	private BoundExpression RewriteLiftedUserDefinedConversion(SyntaxNode syntax, BoundExpression rewrittenOperand, Conversion conversion, bool @checked, TypeSymbol rewrittenType)
	{
		if (_inExpressionLambda)
		{
			Conversion conversion2 = TryMakeConversion(syntax, conversion, rewrittenOperand.Type, rewrittenType, @checked);
			return BoundConversion.Synthesized(syntax, rewrittenOperand, conversion2, @checked, explicitCastInCode: true, null, null, rewrittenType);
		}
		BoundExpression boundExpression = OptimizeLiftedUserDefinedConversion(syntax, rewrittenOperand, conversion, rewrittenType);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		BoundLocal boundLocal = _factory.StoreToTemp(rewrittenOperand, out BoundAssignmentOperator store);
		MethodSymbol method = UnsafeGetNullableMethod(syntax, boundLocal.Type, SpecialMember.System_Nullable_T_GetValueOrDefault);
		BoundExpression rewrittenCondition = _factory.MakeNullableHasValue(syntax, boundLocal);
		BoundCall arg = BoundCall.Synthesized(syntax, boundLocal, ThreeState.Unknown, method);
		TypeParameterSymbol constrainedToTypeOpt = conversion.ConstrainedToTypeOpt;
		BoundCall call = BoundCall.Synthesized(syntax, ((object)constrainedToTypeOpt == null) ? null : new BoundTypeExpression(syntax, null, constrainedToTypeOpt), ThreeState.Unknown, conversion.Method, arg);
		BoundExpression rewrittenConsequence = MakeLiftedUserDefinedConversionConsequence(call, rewrittenType);
		BoundExpression rewrittenAlternative = new BoundDefaultExpression(syntax, rewrittenType);
		BoundExpression value = RewriteConditionalOperator(syntax, rewrittenCondition, rewrittenConsequence, rewrittenAlternative, null, rewrittenType, isRef: false);
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal.LocalSymbol), ImmutableArray.Create((BoundExpression)store), value, rewrittenType);
	}

	private BoundExpression RewriteIntPtrConversion(SyntaxNode syntax, BoundExpression rewrittenOperand, Conversion conversion, bool @checked, bool explicitCastInCode, ConstantValue? constantValueOpt, TypeSymbol rewrittenType)
	{
		TypeSymbol type = rewrittenOperand.Type;
		SpecialMember intPtrConversionMethod = GetIntPtrConversionMethod(type, rewrittenType);
		if (!TryGetSpecialTypeMethod(syntax, intPtrConversionMethod, out MethodSymbol method))
		{
			return BadExpression(syntax, rewrittenType, rewrittenOperand);
		}
		conversion = conversion.SetConversionMethod(method);
		if (type.IsNullableType() && rewrittenType.IsNullableType())
		{
			return RewriteLiftedUserDefinedConversion(syntax, rewrittenOperand, conversion, @checked, rewrittenType);
		}
		if (type.IsNullableType())
		{
			rewrittenOperand = MakeConversionNode(rewrittenOperand, type.StrippedType(), @checked, acceptFailingConversion: false, markAsChecked: true);
		}
		rewrittenOperand = MakeConversionNode(rewrittenOperand, method.GetParameterType(0), @checked);
		if (_inExpressionLambda)
		{
			return BoundConversion.Synthesized(syntax, rewrittenOperand, conversion, @checked, explicitCastInCode, null, constantValueOpt, rewrittenType);
		}
		BoundExpression rewrittenOperand2 = MakeCall(syntax, null, method, ImmutableArray.Create(rewrittenOperand));
		return MakeConversionNode(rewrittenOperand2, rewrittenType, @checked, acceptFailingConversion: false, markAsChecked: true);
	}

	public static SpecialMember GetIntPtrConversionMethod(TypeSymbol source, TypeSymbol target)
	{
		TypeSymbol typeSymbol = target.StrippedType();
		TypeSymbol typeSymbol2 = source.StrippedType();
		SpecialType specialType = (typeSymbol.IsEnumType() ? typeSymbol.GetEnumUnderlyingType().SpecialType : typeSymbol.SpecialType);
		SpecialType specialType2 = (typeSymbol2.IsEnumType() ? typeSymbol2.GetEnumUnderlyingType().SpecialType : typeSymbol2.SpecialType);
		switch (specialType)
		{
		case SpecialType.System_IntPtr:
			if (source.IsPointerOrFunctionPointer())
			{
				return SpecialMember.System_IntPtr__op_Explicit_FromPointer;
			}
			switch (specialType2)
			{
			case SpecialType.System_Char:
			case SpecialType.System_SByte:
			case SpecialType.System_Byte:
			case SpecialType.System_Int16:
			case SpecialType.System_UInt16:
			case SpecialType.System_Int32:
				return SpecialMember.System_IntPtr__op_Explicit_FromInt32;
			case SpecialType.System_UInt32:
			case SpecialType.System_Int64:
			case SpecialType.System_UInt64:
			case SpecialType.System_Decimal:
			case SpecialType.System_Single:
			case SpecialType.System_Double:
				return SpecialMember.System_IntPtr__op_Explicit_FromInt64;
			}
			break;
		case SpecialType.System_UIntPtr:
			if (source.IsPointerOrFunctionPointer())
			{
				return SpecialMember.System_UIntPtr__op_Explicit_FromPointer;
			}
			switch (specialType2)
			{
			case SpecialType.System_Char:
			case SpecialType.System_Byte:
			case SpecialType.System_UInt16:
			case SpecialType.System_UInt32:
				return SpecialMember.System_UIntPtr__op_Explicit_FromUInt32;
			case SpecialType.System_SByte:
			case SpecialType.System_Int16:
			case SpecialType.System_Int32:
			case SpecialType.System_Int64:
			case SpecialType.System_UInt64:
			case SpecialType.System_Decimal:
			case SpecialType.System_Single:
			case SpecialType.System_Double:
				return SpecialMember.System_UIntPtr__op_Explicit_FromUInt64;
			}
			break;
		default:
			switch (specialType2)
			{
			case SpecialType.System_IntPtr:
				if (target.IsPointerOrFunctionPointer())
				{
					return SpecialMember.System_IntPtr__op_Explicit_ToPointer;
				}
				switch (specialType)
				{
				case SpecialType.System_Char:
				case SpecialType.System_SByte:
				case SpecialType.System_Byte:
				case SpecialType.System_Int16:
				case SpecialType.System_UInt16:
				case SpecialType.System_Int32:
				case SpecialType.System_UInt32:
					return SpecialMember.System_IntPtr__op_Explicit_ToInt32;
				case SpecialType.System_Int64:
				case SpecialType.System_UInt64:
				case SpecialType.System_Decimal:
				case SpecialType.System_Single:
				case SpecialType.System_Double:
					return SpecialMember.System_IntPtr__op_Explicit_ToInt64;
				}
				break;
			case SpecialType.System_UIntPtr:
				if (target.IsPointerOrFunctionPointer())
				{
					return SpecialMember.System_UIntPtr__op_Explicit_ToPointer;
				}
				switch (specialType)
				{
				case SpecialType.System_Char:
				case SpecialType.System_SByte:
				case SpecialType.System_Byte:
				case SpecialType.System_Int16:
				case SpecialType.System_UInt16:
				case SpecialType.System_Int32:
				case SpecialType.System_UInt32:
					return SpecialMember.System_UIntPtr__op_Explicit_ToUInt32;
				case SpecialType.System_Int64:
				case SpecialType.System_UInt64:
				case SpecialType.System_Decimal:
				case SpecialType.System_Single:
				case SpecialType.System_Double:
					return SpecialMember.System_UIntPtr__op_Explicit_ToUInt64;
				}
				break;
			}
			break;
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_Conversion.cs", 1656);
	}

	private static SpecialMember DecimalConversionMethod(TypeSymbol typeFrom, TypeSymbol typeTo)
	{
		if (typeFrom.SpecialType == SpecialType.System_Decimal)
		{
			return typeTo.SpecialType switch
			{
				SpecialType.System_Char => SpecialMember.System_Decimal__op_Explicit_ToChar, 
				SpecialType.System_SByte => SpecialMember.System_Decimal__op_Explicit_ToSByte, 
				SpecialType.System_Byte => SpecialMember.System_Decimal__op_Explicit_ToByte, 
				SpecialType.System_Int16 => SpecialMember.System_Decimal__op_Explicit_ToInt16, 
				SpecialType.System_UInt16 => SpecialMember.System_Decimal__op_Explicit_ToUInt16, 
				SpecialType.System_Int32 => SpecialMember.System_Decimal__op_Explicit_ToInt32, 
				SpecialType.System_UInt32 => SpecialMember.System_Decimal__op_Explicit_ToUInt32, 
				SpecialType.System_Int64 => SpecialMember.System_Decimal__op_Explicit_ToInt64, 
				SpecialType.System_UInt64 => SpecialMember.System_Decimal__op_Explicit_ToUInt64, 
				SpecialType.System_Single => SpecialMember.System_Decimal__op_Explicit_ToSingle, 
				SpecialType.System_Double => SpecialMember.System_Decimal__op_Explicit_ToDouble, 
				_ => throw ExceptionUtilities.UnexpectedValue(typeTo.SpecialType), 
			};
		}
		return typeFrom.SpecialType switch
		{
			SpecialType.System_Char => SpecialMember.System_Decimal__op_Implicit_FromChar, 
			SpecialType.System_SByte => SpecialMember.System_Decimal__op_Implicit_FromSByte, 
			SpecialType.System_Byte => SpecialMember.System_Decimal__op_Implicit_FromByte, 
			SpecialType.System_Int16 => SpecialMember.System_Decimal__op_Implicit_FromInt16, 
			SpecialType.System_UInt16 => SpecialMember.System_Decimal__op_Implicit_FromUInt16, 
			SpecialType.System_Int32 => SpecialMember.System_Decimal__op_Implicit_FromInt32, 
			SpecialType.System_UInt32 => SpecialMember.System_Decimal__op_Implicit_FromUInt32, 
			SpecialType.System_Int64 => SpecialMember.System_Decimal__op_Implicit_FromInt64, 
			SpecialType.System_UInt64 => SpecialMember.System_Decimal__op_Implicit_FromUInt64, 
			SpecialType.System_Single => SpecialMember.System_Decimal__op_Explicit_FromSingle, 
			SpecialType.System_Double => SpecialMember.System_Decimal__op_Explicit_FromDouble, 
			_ => throw ExceptionUtilities.UnexpectedValue(typeFrom.SpecialType), 
		};
	}

	private BoundExpression RewriteDecimalConversion(SyntaxNode syntax, BoundExpression operand, TypeSymbol fromType, TypeSymbol toType, bool @checked, bool isImplicit, ConstantValue? constantValueOpt)
	{
		if (fromType.SpecialType == SpecialType.System_Decimal)
		{
			SpecialType specialType = toType.SpecialType;
			if ((uint)(specialType - 21) <= 1u)
			{
				operand = RewriteDecimalConversionCore(syntax, operand, fromType, get64BitType(_compilation, toType.SpecialType == SpecialType.System_IntPtr), isImplicit, constantValueOpt);
				return MakeConversionNode(operand, toType, @checked);
			}
		}
		else
		{
			SpecialType specialType = fromType.SpecialType;
			if ((uint)(specialType - 21) <= 1u)
			{
				operand = MakeConversionNode(operand, get64BitType(_compilation, fromType.SpecialType == SpecialType.System_IntPtr), @checked);
				return RewriteDecimalConversionCore(syntax, operand, operand.Type, toType, isImplicit, constantValueOpt);
			}
		}
		return RewriteDecimalConversionCore(syntax, operand, fromType, toType, isImplicit, constantValueOpt);
		static TypeSymbol get64BitType(CSharpCompilation compilation, bool signed)
		{
			return compilation.GetSpecialType(signed ? SpecialType.System_Int64 : SpecialType.System_UInt64);
		}
	}

	private BoundExpression RewriteDecimalConversionCore(SyntaxNode syntax, BoundExpression operand, TypeSymbol fromType, TypeSymbol toType, bool isImplicit, ConstantValue? constantValueOpt)
	{
		SpecialMember specialMember = DecimalConversionMethod(fromType, toType);
		if (!TryGetSpecialTypeMethod(syntax, specialMember, out MethodSymbol method))
		{
			return BadExpression(syntax, toType, operand);
		}
		if (_inExpressionLambda)
		{
			ConversionKind kind = (isImplicit ? ConversionKind.ImplicitUserDefined : ConversionKind.ExplicitUserDefined);
			Conversion conversion = new Conversion(kind, method, isExtensionMethod: false);
			return new BoundConversion(syntax, operand, conversion, @checked: false, explicitCastInCode: false, null, constantValueOpt, toType);
		}
		return BoundCall.Synthesized(syntax, null, ThreeState.Unknown, method, operand);
	}

	private Conversion TryMakeConversion(SyntaxNode syntax, Conversion conversion, TypeSymbol fromType, TypeSymbol toType, bool @checked)
	{
		switch (conversion.Kind)
		{
		case ConversionKind.ImplicitUserDefined:
		case ConversionKind.ExplicitUserDefined:
		{
			MethodSymbol method5 = conversion.Method;
			Conversion conversion2 = TryMakeConversion(syntax, conversion.UserDefinedFromConversion, fromType, method5.Parameters[0].Type, @checked);
			if (!conversion2.Exists)
			{
				return Conversion.NoConversion;
			}
			Conversion conversion3 = TryMakeConversion(syntax, conversion.UserDefinedToConversion, method5.ReturnType, toType, @checked);
			if (!conversion3.Exists)
			{
				return Conversion.NoConversion;
			}
			if (conversion2 == conversion.UserDefinedFromConversion && conversion3 == conversion.UserDefinedToConversion)
			{
				return conversion;
			}
			UserDefinedConversionResult conversionResult = UserDefinedConversionResult.Valid(ImmutableArray.Create(UserDefinedConversionAnalysis.Normal(conversion.ConstrainedToTypeOpt, method5, conversion2, conversion3, fromType, toType)), 0);
			return new Conversion(conversionResult, conversion.IsImplicit);
		}
		case ConversionKind.IntPtr:
		{
			SpecialMember intPtrConversionMethod = GetIntPtrConversionMethod(fromType, toType);
			if (!TryGetSpecialTypeMethod(syntax, intPtrConversionMethod, out MethodSymbol method4))
			{
				return Conversion.NoConversion;
			}
			return TryMakeUserDefinedConversion(syntax, method4, fromType, toType, @checked, conversion.IsImplicit);
		}
		case ConversionKind.ImplicitNumeric:
		case ConversionKind.ExplicitNumeric:
			if (fromType.SpecialType == SpecialType.System_Decimal || toType.SpecialType == SpecialType.System_Decimal)
			{
				SpecialMember specialMember3 = DecimalConversionMethod(fromType, toType);
				if (!TryGetSpecialTypeMethod(syntax, specialMember3, out MethodSymbol method3))
				{
					return Conversion.NoConversion;
				}
				return TryMakeUserDefinedConversion(syntax, method3, fromType, toType, @checked, conversion.IsImplicit);
			}
			return conversion;
		case ConversionKind.ImplicitEnumeration:
		case ConversionKind.ExplicitEnumeration:
			if (fromType.SpecialType == SpecialType.System_Decimal)
			{
				NamedTypeSymbol enumUnderlyingType = toType.GetEnumUnderlyingType();
				SpecialMember specialMember = DecimalConversionMethod(fromType, enumUnderlyingType);
				if (!TryGetSpecialTypeMethod(syntax, specialMember, out MethodSymbol method))
				{
					return Conversion.NoConversion;
				}
				return TryMakeUserDefinedConversion(syntax, method, fromType, toType, @checked, conversion.IsImplicit);
			}
			if (toType.SpecialType == SpecialType.System_Decimal)
			{
				SpecialMember specialMember2 = DecimalConversionMethod(fromType.GetEnumUnderlyingType(), toType);
				if (!TryGetSpecialTypeMethod(syntax, specialMember2, out MethodSymbol method2))
				{
					return Conversion.NoConversion;
				}
				return TryMakeUserDefinedConversion(syntax, method2, fromType, toType, @checked, conversion.IsImplicit);
			}
			return conversion;
		default:
			return conversion;
		}
	}

	private Conversion TryMakeConversion(SyntaxNode syntax, TypeSymbol fromType, TypeSymbol toType, bool @checked)
	{
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = GetNewCompoundUseSiteInfo();
		Conversion result = TryMakeConversion(syntax, _compilation.Conversions.ClassifyConversionFromType(fromType, toType, @checked, ref useSiteInfo), fromType, toType, @checked);
		_diagnostics.Add(syntax, useSiteInfo);
		return result;
	}

	private Conversion TryMakeUserDefinedConversion(SyntaxNode syntax, MethodSymbol meth, TypeSymbol fromType, TypeSymbol toType, bool @checked, bool isImplicit)
	{
		Conversion sourceConversion = TryMakeConversion(syntax, fromType, meth.Parameters[0].Type, @checked);
		if (!sourceConversion.Exists)
		{
			return Conversion.NoConversion;
		}
		Conversion targetConversion = TryMakeConversion(syntax, meth.ReturnType, toType, @checked);
		if (!targetConversion.Exists)
		{
			return Conversion.NoConversion;
		}
		return new Conversion(UserDefinedConversionResult.Valid(ImmutableArray.Create(UserDefinedConversionAnalysis.Normal(null, meth, sourceConversion, targetConversion, fromType, toType)), 0), isImplicit);
	}

	public override BoundNode? VisitDeconstructionAssignmentOperator(BoundDeconstructionAssignmentOperator node)
	{
		BoundConversion right = node.Right;
		return RewriteDeconstruction(node.Left, right.Conversion, right.Operand, node.IsUsed);
	}

	private BoundExpression? RewriteDeconstruction(BoundTupleExpression left, Conversion conversion, BoundExpression right, bool isUsed)
	{
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance();
		ArrayBuilder<Binder.DeconstructionVariable> assignmentTargetsAndSideEffects = GetAssignmentTargetsAndSideEffects(left, instance, instance2);
		BoundExpression boundExpression = RewriteDeconstruction(assignmentTargetsAndSideEffects, conversion, left.Type, right, isUsed);
		Binder.DeconstructionVariable.FreeDeconstructionVariables(assignmentTargetsAndSideEffects);
		if (boundExpression == null)
		{
			instance.Free();
			instance2.Free();
			return null;
		}
		return _factory.Sequence(instance.ToImmutableAndFree(), instance2.ToImmutableAndFree(), boundExpression);
	}

	private BoundExpression? RewriteDeconstruction(ArrayBuilder<Binder.DeconstructionVariable> lhsTargets, Conversion conversion, TypeSymbol leftType, BoundExpression right, bool isUsed)
	{
		if (right.Kind == BoundKind.ConditionalOperator)
		{
			BoundConditionalOperator boundConditionalOperator = (BoundConditionalOperator)right;
			return boundConditionalOperator.Update(boundConditionalOperator.IsRef, VisitExpression(boundConditionalOperator.Condition), RewriteDeconstruction(lhsTargets, conversion, leftType, boundConditionalOperator.Consequence, isUsed: true), RewriteDeconstruction(lhsTargets, conversion, leftType, boundConditionalOperator.Alternative, isUsed: true), boundConditionalOperator.ConstantValueOpt, leftType, wasTargetTyped: true, leftType);
		}
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		DeconstructionSideEffects effects = DeconstructionSideEffects.GetInstance();
		BoundExpression boundExpression = ApplyDeconstructionConversion(lhsTargets, right, conversion, instance, effects, isUsed, inInit: true);
		reverseAssignmentsToTargetsIfApplicable();
		effects.Consolidate();
		if (!isUsed)
		{
			BoundExpression boundExpression2 = effects.PopLast();
			if (boundExpression2 == null)
			{
				instance.Free();
				effects.Free();
				return null;
			}
			return _factory.Sequence(instance.ToImmutableAndFree(), effects.ToImmutableAndFree(), boundExpression2);
		}
		if (!boundExpression.HasErrors)
		{
			boundExpression = VisitExpression(boundExpression);
		}
		return _factory.Sequence(instance.ToImmutableAndFree(), effects.ToImmutableAndFree(), boundExpression);
		static bool canReorderTargetAssignments(ArrayBuilder<Binder.DeconstructionVariable> targets, ref PooledHashSet<Symbol>? visitedSymbols)
		{
			foreach (Binder.DeconstructionVariable target in targets)
			{
				BoundExpression single = target.Single;
				Symbol item;
				if (single != null)
				{
					if (single is BoundLocal boundLocal)
					{
						LocalSymbol localSymbol = boundLocal.LocalSymbol;
						if ((object)localSymbol != null && localSymbol.RefKind == RefKind.None)
						{
							item = localSymbol;
							goto IL_007e;
						}
					}
					else if (single is BoundParameter boundParameter)
					{
						ParameterSymbol parameterSymbol = boundParameter.ParameterSymbol;
						if ((object)parameterSymbol != null && parameterSymbol.RefKind == RefKind.None)
						{
							item = parameterSymbol;
							goto IL_007e;
						}
					}
					else if (single is BoundDiscardExpression)
					{
						continue;
					}
					return false;
				}
				if (!canReorderTargetAssignments(target.NestedVariables, ref visitedSymbols))
				{
					return false;
				}
				continue;
				IL_007e:
				if (visitedSymbols == null)
				{
					visitedSymbols = PooledHashSet<Symbol>.GetInstance();
				}
				if (!visitedSymbols.Add(item))
				{
					return false;
				}
			}
			return true;
		}
		void reverseAssignmentsToTargetsIfApplicable()
		{
			PooledHashSet<Symbol> visitedSymbols = null;
			if (right != null)
			{
				if (right.Kind == BoundKind.ConvertedTupleLiteral)
				{
					goto IL_0042;
				}
				if (right is BoundConversion boundConversion)
				{
					BoundExpression operand = boundConversion.Operand;
					if (operand != null && operand.Kind == BoundKind.ConvertedTupleLiteral)
					{
						goto IL_0042;
					}
				}
			}
			bool flag = false;
			goto IL_0048;
			IL_0048:
			if (flag && effects.init.Any() && canReorderTargetAssignments(lhsTargets, ref visitedSymbols))
			{
				effects.assignments.ReverseContents();
			}
			visitedSymbols?.Free();
			return;
			IL_0042:
			flag = true;
			goto IL_0048;
		}
	}

	private BoundExpression? ApplyDeconstructionConversion(ArrayBuilder<Binder.DeconstructionVariable> leftTargets, BoundExpression right, Conversion conversion, ArrayBuilder<LocalSymbol> temps, DeconstructionSideEffects effects, bool isUsed, bool inInit)
	{
		ImmutableArray<BoundExpression> rightParts = GetRightParts(right, conversion, temps, effects, ref inInit);
		ImmutableArray<(BoundValuePlaceholder, BoundExpression)> deconstructConversionInfo = conversion.DeconstructConversionInfo;
		ArrayBuilder<BoundExpression> arrayBuilder = (isUsed ? ArrayBuilder<BoundExpression>.GetInstance(leftTargets.Count) : null);
		for (int i = 0; i < leftTargets.Count; i++)
		{
			(BoundValuePlaceholder, BoundExpression) tuple = deconstructConversionInfo[i];
			BoundValuePlaceholder item = tuple.Item1;
			BoundExpression item2 = tuple.Item2;
			ArrayBuilder<Binder.DeconstructionVariable> nestedVariables = leftTargets[i].NestedVariables;
			BoundExpression boundExpression;
			if (nestedVariables != null)
			{
				boundExpression = ApplyDeconstructionConversion(nestedVariables, rightParts[i], BoundNode.GetConversion(item2, item), temps, effects, isUsed, inInit);
			}
			else
			{
				BoundExpression boundExpression2 = rightParts[i];
				if (inInit)
				{
					boundExpression2 = EvaluateSideEffectingArgumentToTemp(boundExpression2, effects.init, temps);
				}
				BoundExpression single = leftTargets[i].Single;
				boundExpression = EvaluateConversionToTemp(boundExpression2, item, item2, temps, effects.conversions);
				if (single.Kind != BoundKind.DiscardExpression)
				{
					effects.assignments.Add(MakeAssignmentOperator(boundExpression.Syntax, single, boundExpression, used: false, isChecked: false, AssignmentKind.Deconstruction));
				}
			}
			arrayBuilder?.Add(boundExpression);
		}
		if (isUsed)
		{
			NamedTypeSymbol type = NamedTypeSymbol.CreateTuple(null, arrayBuilder.SelectAsArray((BoundExpression e) => TypeWithAnnotations.Create(e.Type)), default(ImmutableArray<Location>), default(ImmutableArray<string>), _compilation, shouldCheckConstraints: false, includeNullability: false, default(ImmutableArray<bool>), (CSharpSyntaxNode)right.Syntax, _diagnostics);
			return new BoundConvertedTupleLiteral(right.Syntax, null, wasTargetTyped: false, arrayBuilder.ToImmutableAndFree(), default(ImmutableArray<string>), default(ImmutableArray<bool>), type);
		}
		return null;
	}

	private ImmutableArray<BoundExpression> GetRightParts(BoundExpression right, Conversion conversion, ArrayBuilder<LocalSymbol> temps, DeconstructionSideEffects effects, ref bool inInit)
	{
		DeconstructMethodInfo deconstructionInfo = conversion.DeconstructionInfo;
		if (!deconstructionInfo.IsDefault)
		{
			BoundExpression target = EvaluateSideEffectingArgumentToTemp(right, inInit ? effects.init : effects.deconstructions, temps);
			inInit = false;
			return InvokeDeconstructMethod(deconstructionInfo, target, effects.deconstructions, temps);
		}
		if (IsTupleExpression(right.Kind))
		{
			return ((BoundTupleExpression)right).Arguments;
		}
		if (right.Kind == BoundKind.Conversion)
		{
			BoundConversion boundConversion = (BoundConversion)right;
			if ((boundConversion.Conversion.Kind == ConversionKind.ImplicitTupleLiteral || boundConversion.Conversion.Kind == ConversionKind.Identity) && IsTupleExpression(boundConversion.Operand.Kind))
			{
				return ((BoundTupleExpression)boundConversion.Operand).Arguments;
			}
		}
		if (right.Type.IsTupleType)
		{
			inInit = false;
			return AccessTupleFields(VisitExpression(right), temps, effects.deconstructions);
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_DeconstructionAssignmentOperator.cs", 325);
	}

	private static bool IsTupleExpression(BoundKind kind)
	{
		if (kind != BoundKind.TupleLiteral)
		{
			return kind == BoundKind.ConvertedTupleLiteral;
		}
		return true;
	}

	private ImmutableArray<BoundExpression> AccessTupleFields(BoundExpression expression, ArrayBuilder<LocalSymbol> temps, ArrayBuilder<BoundExpression> effects)
	{
		TypeSymbol? type = expression.Type;
		int length = type.TupleElementTypesWithAnnotations.Length;
		BoundExpression tuple;
		if (CanChangeValueBetweenReads(expression))
		{
			BoundLocal boundLocal = _factory.StoreToTemp(expression, out BoundAssignmentOperator store);
			effects.Add(store);
			temps.Add(boundLocal.LocalSymbol);
			tuple = boundLocal;
		}
		else
		{
			tuple = expression;
		}
		ImmutableArray<FieldSymbol> tupleElements = type.TupleElements;
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(length);
		for (int i = 0; i < length; i++)
		{
			BoundExpression item = MakeTupleFieldAccessAndReportUseSiteDiagnostics(tuple, expression.Syntax, tupleElements[i]);
			instance.Add(item);
		}
		return instance.ToImmutableAndFree();
	}

	private BoundExpression EvaluateConversionToTemp(BoundExpression expression, BoundValuePlaceholder placeholder, BoundExpression conversion, ArrayBuilder<LocalSymbol> temps, ArrayBuilder<BoundExpression> effects)
	{
		if (BoundNode.GetConversion(conversion, placeholder).IsIdentity)
		{
			return expression;
		}
		return EvaluateSideEffectingArgumentToTemp(ApplyConversion(conversion, placeholder, expression), effects, temps);
	}

	private ImmutableArray<BoundExpression> InvokeDeconstructMethod(DeconstructMethodInfo deconstruction, BoundExpression target, ArrayBuilder<BoundExpression> effects, ArrayBuilder<LocalSymbol> temps)
	{
		AddPlaceholderReplacement(deconstruction.InputPlaceholder, target);
		ImmutableArray<BoundDeconstructValuePlaceholder> outputPlaceholders = deconstruction.OutputPlaceholders;
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(outputPlaceholders.Length);
		foreach (BoundDeconstructValuePlaceholder item in outputPlaceholders)
		{
			SynthesizedLocal synthesizedLocal = new SynthesizedLocal(_factory.CurrentFunction, TypeWithAnnotations.Create(item.Type), SynthesizedLocalKind.LoweringTemp);
			BoundLocal boundLocal = new BoundLocal(target.Syntax, synthesizedLocal, null, item.Type)
			{
				WasCompilerGenerated = true
			};
			temps.Add(synthesizedLocal);
			AddPlaceholderReplacement(item, boundLocal);
			instance.Add(boundLocal);
		}
		effects.Add(VisitExpression(deconstruction.Invocation));
		RemovePlaceholderReplacement(deconstruction.InputPlaceholder);
		foreach (BoundDeconstructValuePlaceholder item2 in outputPlaceholders)
		{
			RemovePlaceholderReplacement(item2);
		}
		return instance.ToImmutableAndFree();
	}

	private BoundExpression EvaluateSideEffectingArgumentToTemp(BoundExpression arg, ArrayBuilder<BoundExpression> effects, ArrayBuilder<LocalSymbol> temps)
	{
		BoundExpression boundExpression = VisitExpression(arg);
		if (CanChangeValueBetweenReads(boundExpression, localsMayBeAssignedOrCaptured: true, structThisCanChangeValueBetweenReads: true))
		{
			BoundLocal boundLocal = _factory.StoreToTemp(boundExpression, out BoundAssignmentOperator store);
			temps.Add(boundLocal.LocalSymbol);
			effects.Add(store);
			return boundLocal;
		}
		return boundExpression;
	}

	private ArrayBuilder<Binder.DeconstructionVariable> GetAssignmentTargetsAndSideEffects(BoundTupleExpression variables, ArrayBuilder<LocalSymbol> temps, ArrayBuilder<BoundExpression> effects)
	{
		ArrayBuilder<Binder.DeconstructionVariable> instance = ArrayBuilder<Binder.DeconstructionVariable>.GetInstance(variables.Arguments.Length);
		foreach (BoundExpression argument in variables.Arguments)
		{
			switch (argument.Kind)
			{
			case BoundKind.DiscardExpression:
				instance.Add(new Binder.DeconstructionVariable(argument, argument.Syntax));
				break;
			case BoundKind.TupleLiteral:
			case BoundKind.ConvertedTupleLiteral:
			{
				BoundTupleExpression boundTupleExpression = (BoundTupleExpression)argument;
				instance.Add(new Binder.DeconstructionVariable(GetAssignmentTargetsAndSideEffects(boundTupleExpression, temps, effects), boundTupleExpression.Syntax));
				break;
			}
			default:
			{
				BoundExpression variable = TransformCompoundAssignmentLHS(argument, effects, temps, argument.Type.IsDynamic());
				instance.Add(new Binder.DeconstructionVariable(variable, argument.Syntax));
				break;
			}
			}
		}
		return instance;
	}

	public override BoundNode VisitDelegateCreationExpression(BoundDelegateCreationExpression node)
	{
		if (node.Argument.HasDynamicType())
		{
			BoundExpression loweredOperand = VisitExpression(node.Argument);
			BoundExpression argument = _dynamicFactory.MakeDynamicConversion(loweredOperand, isExplicit: false, isArrayIndex: false, isChecked: false, node.Type).ToExpression();
			return new BoundDelegateCreationExpression(node.Syntax, argument, null, isExtensionMethod: false, node.WasTargetTyped, node.Type);
		}
		if (node.Argument.Kind == BoundKind.MethodGroup)
		{
			BoundMethodGroup boundMethodGroup = (BoundMethodGroup)node.Argument;
			MethodSymbol methodOpt = node.MethodOpt;
			SyntaxNode syntax = _factory.Syntax;
			_factory.Syntax = (boundMethodGroup.ReceiverOpt ?? boundMethodGroup).Syntax;
			BoundExpression argument2 = ((!methodOpt.RequiresInstanceReceiver && !node.IsExtensionMethod && !methodOpt.IsAbstract && !methodOpt.IsVirtual) ? _factory.Type(methodOpt.ContainingType) : VisitExpression(boundMethodGroup.ReceiverOpt));
			_factory.Syntax = syntax;
			return node.Update(argument2, methodOpt, node.IsExtensionMethod, node.WasTargetTyped, node.Type);
		}
		return base.VisitDelegateCreationExpression(node);
	}

	public override BoundNode VisitDoStatement(BoundDoStatement node)
	{
		BoundExpression boundExpression = VisitExpression(node.Condition);
		BoundStatement boundStatement = VisitStatement(node.Body);
		GeneratedLabelSymbol label = new GeneratedLabelSymbol("start");
		SyntaxNode syntax = node.Syntax;
		if (!node.WasCompilerGenerated && Instrument)
		{
			boundExpression = Instrumenter.InstrumentDoStatementCondition(node, boundExpression, _factory);
		}
		BoundStatement boundStatement2 = new BoundConditionalGoto(syntax, boundExpression, jumpIfTrue: true, label);
		if (!node.WasCompilerGenerated && Instrument)
		{
			boundStatement2 = Instrumenter.InstrumentDoStatementConditionalGotoStart(node, boundStatement2);
		}
		if (node.Locals.IsEmpty)
		{
			return BoundStatementList.Synthesized(syntax, node.HasErrors, new BoundLabelStatement(syntax, label), boundStatement, new BoundLabelStatement(syntax, node.ContinueLabel), boundStatement2, new BoundLabelStatement(syntax, node.BreakLabel));
		}
		return BoundStatementList.Synthesized(syntax, node.HasErrors, new BoundLabelStatement(syntax, label), new BoundBlock(syntax, node.Locals, ImmutableArray.Create(boundStatement, new BoundLabelStatement(syntax, node.ContinueLabel), boundStatement2)), new BoundLabelStatement(syntax, node.BreakLabel));
	}

	public override BoundNode VisitEventAssignmentOperator(BoundEventAssignmentOperator node)
	{
		BoundExpression boundExpression = VisitExpression(node.ReceiverOpt);
		BoundExpression boundExpression2 = VisitExpression(node.Argument);
		if (boundExpression != null && node.Event.ContainingAssembly.IsLinked && node.Event.ContainingType.IsInterfaceType())
		{
			foreach (CSharpAttributeData attribute in node.Event.ContainingType.GetAttributes())
			{
				if (attribute.GetTargetAttributeSignatureIndex(AttributeDescription.ComEventInterfaceAttribute) == 0)
				{
					DiagnosticInfo errorInfo = attribute.ErrorInfo;
					if (errorInfo != null)
					{
						_diagnostics.Add(errorInfo, node.Syntax.Location);
					}
					if (!attribute.HasErrors)
					{
						return RewriteNoPiaEventAssignmentOperator(node, boundExpression, boundExpression2);
					}
				}
			}
		}
		if (node.Event.IsWindowsRuntimeEvent)
		{
			EventAssignmentKind kind = (node.IsAddition ? EventAssignmentKind.Addition : EventAssignmentKind.Subtraction);
			return RewriteWindowsRuntimeEventAssignmentOperator(node.Syntax, node.Event, kind, boundExpression, boundExpression2);
		}
		ImmutableArray<BoundExpression> rewrittenArguments = ImmutableArray.Create(boundExpression2);
		MethodSymbol method = (node.IsAddition ? node.Event.AddMethod : node.Event.RemoveMethod);
		return MakeCall(node.Syntax, boundExpression, method, rewrittenArguments);
	}

	private BoundExpression RewriteWindowsRuntimeEventAssignmentOperator(SyntaxNode syntax, EventSymbol eventSymbol, EventAssignmentKind kind, BoundExpression? rewrittenReceiverOpt, BoundExpression rewrittenArgument)
	{
		BoundAssignmentOperator store = null;
		BoundLocal boundLocal = null;
		if (!eventSymbol.IsStatic && CanChangeValueBetweenReads(rewrittenReceiverOpt))
		{
			boundLocal = _factory.StoreToTemp(rewrittenReceiverOpt, out store);
		}
		NamedTypeSymbol namedTypeSymbol = _factory.WellKnownType(WellKnownType.System_Runtime_InteropServices_WindowsRuntime_EventRegistrationToken);
		_factory.WellKnownType(WellKnownType.System_Runtime_InteropServices_WindowsRuntime_WindowsRuntimeMarshal);
		NamedTypeSymbol type = _factory.WellKnownType(WellKnownType.System_Action_T).Construct(namedTypeSymbol);
		TypeSymbol type2 = eventSymbol.Type;
		BoundExpression argument = boundLocal ?? rewrittenReceiverOpt ?? _factory.Type(type2);
		BoundDelegateCreationExpression boundDelegateCreationExpression = new BoundDelegateCreationExpression(syntax, argument, eventSymbol.RemoveMethod, isExtensionMethod: false, wasTargetTyped: false, type);
		BoundExpression boundExpression = null;
		if (kind == EventAssignmentKind.Assignment)
		{
			boundExpression = ((!TryGetWellKnownTypeMember<MethodSymbol>(syntax, WellKnownMember.System_Runtime_InteropServices_WindowsRuntime_WindowsRuntimeMarshal__RemoveAllEventHandlers, out MethodSymbol symbol)) ? new BoundBadExpression(syntax, LookupResultKind.NotInvocable, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundExpression)boundDelegateCreationExpression), ErrorTypeSymbol.UnknownResultType) : MakeCall(syntax, null, symbol, ImmutableArray.Create((BoundExpression)boundDelegateCreationExpression)));
		}
		WellKnownMember member;
		ImmutableArray<BoundExpression> immutableArray;
		if (kind == EventAssignmentKind.Subtraction)
		{
			member = WellKnownMember.System_Runtime_InteropServices_WindowsRuntime_WindowsRuntimeMarshal__RemoveEventHandler_T;
			immutableArray = ImmutableArray.Create(boundDelegateCreationExpression, rewrittenArgument);
		}
		else
		{
			NamedTypeSymbol type3 = _factory.WellKnownType(WellKnownType.System_Func_T2).Construct(type2, namedTypeSymbol);
			BoundDelegateCreationExpression item = new BoundDelegateCreationExpression(syntax, argument, eventSymbol.AddMethod, isExtensionMethod: false, wasTargetTyped: false, type3);
			member = WellKnownMember.System_Runtime_InteropServices_WindowsRuntime_WindowsRuntimeMarshal__AddEventHandler_T;
			immutableArray = ImmutableArray.Create(item, boundDelegateCreationExpression, rewrittenArgument);
		}
		BoundExpression boundExpression2;
		if (TryGetWellKnownTypeMember<MethodSymbol>(syntax, member, out MethodSymbol symbol2))
		{
			symbol2 = symbol2.Construct(type2);
			boundExpression2 = MakeCall(syntax, null, symbol2, immutableArray);
		}
		else
		{
			boundExpression2 = new BoundBadExpression(syntax, LookupResultKind.NotInvocable, ImmutableArray<Symbol>.Empty, immutableArray, ErrorTypeSymbol.UnknownResultType);
		}
		if (boundLocal == null && boundExpression == null)
		{
			return boundExpression2;
		}
		ImmutableArray<LocalSymbol> locals = ((boundLocal == null) ? ImmutableArray<LocalSymbol>.Empty : ImmutableArray.Create(boundLocal.LocalSymbol));
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(2);
		if (boundExpression != null)
		{
			instance.Add(boundExpression);
		}
		if (store != null)
		{
			instance.Add(store);
		}
		return new BoundSequence(syntax, locals, instance.ToImmutableAndFree(), boundExpression2, boundExpression2.Type);
	}

	private BoundExpression VisitWindowsRuntimeEventFieldAssignmentOperator(SyntaxNode syntax, BoundEventAccess left, BoundExpression rewrittenRight)
	{
		EventSymbol eventSymbol = left.EventSymbol;
		BoundExpression rewrittenReceiverOpt = VisitExpression(left.ReceiverOpt);
		return RewriteWindowsRuntimeEventAssignmentOperator(syntax, eventSymbol, EventAssignmentKind.Assignment, rewrittenReceiverOpt, rewrittenRight);
	}

	public override BoundNode VisitEventAccess(BoundEventAccess node)
	{
		BoundExpression rewrittenReceiver = VisitExpression(node.ReceiverOpt);
		return MakeEventAccess(node.Syntax, rewrittenReceiver, node.EventSymbol, node.ConstantValueOpt, node.ResultKind, node.Type);
	}

	private BoundExpression MakeEventAccess(SyntaxNode syntax, BoundExpression? rewrittenReceiver, EventSymbol eventSymbol, ConstantValue? constantValueOpt, LookupResultKind resultKind, TypeSymbol type)
	{
		FieldSymbol associatedField = eventSymbol.AssociatedField;
		if (!eventSymbol.IsWindowsRuntimeEvent)
		{
			return MakeFieldAccess(syntax, rewrittenReceiver, associatedField, constantValueOpt, resultKind, type);
		}
		NamedTypeSymbol newOwner = (NamedTypeSymbol)associatedField.Type;
		BoundFieldAccess boundFieldAccess = new BoundFieldAccess(syntax, associatedField.IsStatic ? null : rewrittenReceiver, associatedField, null)
		{
			WasCompilerGenerated = true
		};
		BoundExpression boundExpression;
		if (TryGetWellKnownTypeMember<MethodSymbol>(syntax, WellKnownMember.System_Runtime_InteropServices_WindowsRuntime_EventRegistrationTokenTable_T__GetOrCreateEventRegistrationTokenTable, out MethodSymbol symbol))
		{
			symbol = symbol.AsMember(newOwner);
			boundExpression = BoundCall.Synthesized(syntax, null, ThreeState.Unknown, symbol, boundFieldAccess);
		}
		else
		{
			boundExpression = new BoundBadExpression(syntax, LookupResultKind.NotInvocable, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundExpression)boundFieldAccess), ErrorTypeSymbol.UnknownResultType);
		}
		if (TryGetWellKnownTypeMember<PropertySymbol>(syntax, WellKnownMember.System_Runtime_InteropServices_WindowsRuntime_EventRegistrationTokenTable_T__InvocationList, out PropertySymbol symbol2))
		{
			MethodSymbol getMethod = symbol2.GetMethod;
			if ((object)getMethod != null)
			{
				getMethod = getMethod.AsMember(newOwner);
				return _factory.Call(boundExpression, getMethod);
			}
			string accessorName = SourcePropertyAccessorSymbol.GetAccessorName(symbol2.Name, getNotSet: true, symbol2.IsCompilationOutputWinMdObj());
			_diagnostics.Add(new CSDiagnosticInfo(ErrorCode.ERR_MissingPredefinedMember, symbol2.ContainingType, accessorName), syntax.Location);
		}
		return new BoundBadExpression(syntax, LookupResultKind.NotInvocable, ImmutableArray<Symbol>.Empty, ImmutableArray.Create(boundExpression), ErrorTypeSymbol.UnknownResultType);
	}

	private BoundExpression RewriteNoPiaEventAssignmentOperator(BoundEventAssignmentOperator node, BoundExpression rewrittenReceiver, BoundExpression rewrittenArgument)
	{
		BoundExpression boundExpression = null;
		SyntaxNode syntax = _factory.Syntax;
		_factory.Syntax = node.Syntax;
		MethodSymbol methodSymbol = _factory.WellKnownMethod(WellKnownMember.System_Runtime_InteropServices_ComAwareEventInfo__ctor);
		if ((object)methodSymbol != null)
		{
			MethodSymbol methodSymbol2 = _factory.WellKnownMethod(node.IsAddition ? WellKnownMember.System_Runtime_InteropServices_ComAwareEventInfo__AddEventHandler : WellKnownMember.System_Runtime_InteropServices_ComAwareEventInfo__RemoveEventHandler);
			if ((object)methodSymbol2 != null)
			{
				TypeSymbol type = methodSymbol2.Parameters[0].Type;
				Conversion conversion = _factory.ClassifyEmitConversion(rewrittenReceiver, type);
				TypeSymbol type2 = methodSymbol2.Parameters[1].Type;
				Conversion conversion2 = _factory.ClassifyEmitConversion(rewrittenArgument, type2);
				BoundExpression receiver = _factory.New(methodSymbol, _factory.Typeof(node.Event.ContainingType, methodSymbol.Parameters[0].Type), _factory.Literal(node.Event.MetadataName));
				boundExpression = _factory.Call(receiver, methodSymbol2, _factory.Convert(type, rewrittenReceiver, conversion), _factory.Convert(type2, rewrittenArgument, conversion2));
			}
		}
		_factory.Syntax = syntax;
		EmitModule?.EmbeddedTypesManagerOpt.EmbedEventIfNeedTo(node.Event.GetCciAdapter(), node.Syntax, _diagnostics.DiagnosticBag, isUsedForComAwareEventBinding: true);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		return new BoundBadExpression(node.Syntax, LookupResultKind.NotCreatable, ImmutableArray.Create((Symbol)node.Event), ImmutableArray.Create(rewrittenReceiver, rewrittenArgument), ErrorTypeSymbol.UnknownResultType);
	}

	public override BoundNode VisitExpressionStatement(BoundExpressionStatement node)
	{
		return RewriteExpressionStatement(node) ?? BoundStatementList.Synthesized(node.Syntax);
	}

	private BoundStatement? RewriteExpressionStatement(BoundExpressionStatement node, bool suppressInstrumentation = false)
	{
		BoundExpression boundExpression = VisitUnusedExpression(node.Expression);
		if (boundExpression == null)
		{
			return null;
		}
		BoundStatement boundStatement = node.Update(boundExpression);
		if (!suppressInstrumentation && Instrument && !node.WasCompilerGenerated)
		{
			boundStatement = Instrumenter.InstrumentExpressionStatement(node, boundStatement);
		}
		return boundStatement;
	}

	private BoundExpression? VisitUnusedExpression(BoundExpression expression)
	{
		if (expression.HasErrors)
		{
			return expression;
		}
		switch (expression.Kind)
		{
		case BoundKind.AwaitExpression:
			return VisitAwaitExpression((BoundAwaitExpression)expression, used: false);
		case BoundKind.AssignmentOperator:
			return VisitAssignmentOperator((BoundAssignmentOperator)expression, used: false);
		case BoundKind.CompoundAssignmentOperator:
			return VisitCompoundAssignmentOperator((BoundCompoundAssignmentOperator)expression, used: false);
		case BoundKind.Call:
			if (_allowOmissionOfConditionalCalls)
			{
				BoundCall boundCall = (BoundCall)expression;
				if (boundCall.Method.CallsAreOmitted(boundCall.SyntaxTree))
				{
					return null;
				}
			}
			break;
		case BoundKind.DynamicInvocation:
			return VisitDynamicInvocation((BoundDynamicInvocation)expression, resultDiscarded: true);
		case BoundKind.ConditionalAccess:
			return RewriteConditionalAccess((BoundConditionalAccess)expression, used: false);
		case BoundKind.IncrementOperator:
			return VisitIncrementOperator((BoundIncrementOperator)expression, used: false);
		}
		return VisitExpression(expression);
	}

	public override BoundNode VisitFieldAccess(BoundFieldAccess node)
	{
		BoundExpression rewrittenReceiver = VisitExpression(node.ReceiverOpt);
		return MakeFieldAccess(node.Syntax, rewrittenReceiver, node.FieldSymbol, node.ConstantValueOpt, node.ResultKind, node.Type, node);
	}

	private BoundExpression MakeFieldAccess(SyntaxNode syntax, BoundExpression? rewrittenReceiver, FieldSymbol fieldSymbol, ConstantValue? constantValueOpt, LookupResultKind resultKind, TypeSymbol type, BoundFieldAccess? oldNodeOpt = null)
	{
		if (fieldSymbol.ContainingType.IsTupleType)
		{
			return MakeTupleFieldAccess(syntax, fieldSymbol, rewrittenReceiver);
		}
		BoundExpression boundExpression = ((oldNodeOpt != null) ? oldNodeOpt.Update(rewrittenReceiver, fieldSymbol, constantValueOpt, resultKind, type) : new BoundFieldAccess(syntax, rewrittenReceiver, fieldSymbol, constantValueOpt, resultKind, type));
		if (fieldSymbol.IsFixedSizeBuffer)
		{
			boundExpression = new BoundAddressOfOperator(syntax, boundExpression, type);
		}
		return boundExpression;
	}

	private BoundExpression MakeTupleFieldAccess(SyntaxNode syntax, FieldSymbol tupleField, BoundExpression? rewrittenReceiver)
	{
		NamedTypeSymbol namedTypeSymbol = tupleField.ContainingType;
		FieldSymbol tupleUnderlyingField = tupleField.TupleUnderlyingField;
		if ((object)tupleUnderlyingField == null)
		{
			return _factory.BadExpression(tupleField.Type);
		}
		if (rewrittenReceiver != null && rewrittenReceiver.Kind == BoundKind.DefaultExpression)
		{
			return new BoundDefaultExpression(syntax, tupleField.Type);
		}
		if (!TypeSymbol.Equals(tupleUnderlyingField.ContainingType, namedTypeSymbol, TypeCompareKind.ConsiderEverything))
		{
			WellKnownMember tupleTypeMember = NamedTypeSymbol.GetTupleTypeMember(8, 8);
			FieldSymbol fieldSymbol = (FieldSymbol)NamedTypeSymbol.GetWellKnownMemberInType(namedTypeSymbol.OriginalDefinition, tupleTypeMember, _diagnostics, syntax);
			if ((object)fieldSymbol == null)
			{
				return _factory.BadExpression(tupleField.Type);
			}
			do
			{
				FieldSymbol f = fieldSymbol.AsMember(namedTypeSymbol);
				rewrittenReceiver = _factory.Field(rewrittenReceiver, f);
				namedTypeSymbol = (NamedTypeSymbol)namedTypeSymbol.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[7].Type;
			}
			while (!TypeSymbol.Equals(tupleUnderlyingField.ContainingType, namedTypeSymbol, TypeCompareKind.ConsiderEverything));
		}
		return _factory.Field(rewrittenReceiver, tupleUnderlyingField);
	}

	private BoundExpression MakeTupleFieldAccessAndReportUseSiteDiagnostics(BoundExpression tuple, SyntaxNode syntax, FieldSymbol field)
	{
		field = field.CorrespondingTupleField ?? field;
		UseSiteInfo<AssemblySymbol> useSiteInfo = field.GetUseSiteInfo();
		DiagnosticInfo? diagnosticInfo = useSiteInfo.DiagnosticInfo;
		if (diagnosticInfo == null || diagnosticInfo.Severity != DiagnosticSeverity.Error)
		{
			useSiteInfo = useSiteInfo.AdjustDiagnosticInfo(null);
		}
		_diagnostics.Add(useSiteInfo, syntax);
		return MakeTupleFieldAccess(syntax, field, tuple);
	}

	public override BoundNode VisitFixedStatement(BoundFixedStatement node)
	{
		ImmutableArray<BoundLocalDeclaration> localDeclarations = node.Declarations.LocalDeclarations;
		int length = localDeclarations.Length;
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance(node.Locals.Length);
		instance.AddRange(node.Locals);
		ArrayBuilder<BoundStatement> instance2 = ArrayBuilder<BoundStatement>.GetInstance(length + 1 + 1);
		BoundStatement[] array = new BoundStatement[length];
		for (int i = 0; i < length; i++)
		{
			BoundLocalDeclaration localDecl = localDeclarations[i];
			instance2.Add(InitializeFixedStatementLocal(localDecl, _factory, out LocalSymbol pinnedTemp));
			instance.Add(pinnedTemp);
			if (pinnedTemp.RefKind == RefKind.None)
			{
				array[i] = _factory.Assignment(_factory.Local(pinnedTemp), _factory.Null(pinnedTemp.Type));
			}
			else
			{
				array[i] = _factory.Assignment(_factory.Local(pinnedTemp), _factory.NullRef(pinnedTemp.TypeWithAnnotations), isRef: true);
			}
		}
		BoundStatement boundStatement = VisitStatement(node.Body);
		instance2.Add(boundStatement);
		instance2.Add(_factory.HiddenSequencePoint());
		if (IsInTryBlock(node) || HasGotoOut(boundStatement))
		{
			return _factory.Block(instance.ToImmutableAndFree(), new BoundTryStatement(_factory.Syntax, _factory.Block(instance2.ToImmutableAndFree()), ImmutableArray<BoundCatchBlock>.Empty, _factory.Block(array)));
		}
		instance2.AddRange(array);
		return _factory.Block(instance.ToImmutableAndFree(), instance2.ToImmutableAndFree());
	}

	private static bool IsInTryBlock(BoundFixedStatement boundFixed)
	{
		SyntaxNode parent = boundFixed.Syntax.Parent;
		while (parent != null)
		{
			switch (parent.Kind())
			{
			case SyntaxKind.TryStatement:
				return true;
			case SyntaxKind.UsingStatement:
				return true;
			case SyntaxKind.ForEachStatement:
			case SyntaxKind.ForEachVariableStatement:
				return true;
			case SyntaxKind.AnonymousMethodExpression:
			case SyntaxKind.SimpleLambdaExpression:
			case SyntaxKind.ParenthesizedLambdaExpression:
				return false;
			case SyntaxKind.CatchClause:
				if (((TryStatementSyntax)parent.Parent).Finally != null)
				{
					return true;
				}
				goto case SyntaxKind.FinallyClause;
			case SyntaxKind.FinallyClause:
				parent = parent.Parent;
				parent = parent.Parent;
				continue;
			}
			if (parent is MemberDeclarationSyntax)
			{
				return false;
			}
			parent = parent.Parent;
		}
		return false;
	}

	private bool HasGotoOut(BoundNode node)
	{
		if (_lazyUnmatchedLabelCache == null)
		{
			_lazyUnmatchedLabelCache = new Dictionary<BoundNode, HashSet<LabelSymbol>>();
		}
		HashSet<LabelSymbol> hashSet = UnmatchedGotoFinder.Find(node, _lazyUnmatchedLabelCache, base.RecursionDepth);
		_lazyUnmatchedLabelCache.Add(node, hashSet);
		if (hashSet != null)
		{
			return hashSet.Count > 0;
		}
		return false;
	}

	public override BoundNode VisitFixedLocalCollectionInitializer(BoundFixedLocalCollectionInitializer node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_FixedStatement.cs", 191);
	}

	private BoundStatement InitializeFixedStatementLocal(BoundLocalDeclaration localDecl, SyntheticBoundNodeFactory factory, out LocalSymbol pinnedTemp)
	{
		BoundExpression? initializerOpt = localDecl.InitializerOpt;
		LocalSymbol localSymbol = localDecl.LocalSymbol;
		BoundFixedLocalCollectionInitializer boundFixedLocalCollectionInitializer = (BoundFixedLocalCollectionInitializer)initializerOpt;
		if ((object)boundFixedLocalCollectionInitializer.GetPinnableOpt != null)
		{
			return InitializeFixedStatementGetPinnable(localDecl, localSymbol, boundFixedLocalCollectionInitializer, factory, out pinnedTemp);
		}
		TypeSymbol type = boundFixedLocalCollectionInitializer.Expression.Type;
		if ((object)type != null && type.SpecialType == SpecialType.System_String)
		{
			return InitializeFixedStatementStringLocal(localDecl, localSymbol, boundFixedLocalCollectionInitializer, factory, out pinnedTemp);
		}
		type = boundFixedLocalCollectionInitializer.Expression.Type;
		if ((object)type != null && type.TypeKind == TypeKind.Array)
		{
			return InitializeFixedStatementArrayLocal(localDecl, localSymbol, boundFixedLocalCollectionInitializer, factory, out pinnedTemp);
		}
		return InitializeFixedStatementRegularLocal(localDecl, localSymbol, boundFixedLocalCollectionInitializer, factory, out pinnedTemp);
	}

	private BoundStatement InitializeFixedStatementRegularLocal(BoundLocalDeclaration localDecl, LocalSymbol localSymbol, BoundFixedLocalCollectionInitializer fixedInitializer, SyntheticBoundNodeFactory factory, out LocalSymbol pinnedTemp)
	{
		_ = localSymbol.Type;
		BoundExpression boundExpression = VisitExpression(fixedInitializer.Expression);
		TypeSymbol pointedAtType = ((PointerTypeSymbol)boundExpression.Type).PointedAtType;
		boundExpression = ((BoundAddressOfOperator)boundExpression).Operand;
		VariableDeclaratorSyntax syntax = fixedInitializer.Syntax.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
		pinnedTemp = factory.SynthesizedLocal(pointedAtType, syntax, isPinned: true, isKnownToReferToTempIfReferenceType: false, RefKind.In, SynthesizedLocalKind.FixedReference);
		BoundStatement boundStatement = factory.Assignment(factory.Local(pinnedTemp), boundExpression, isRef: true);
		BoundAddressOfOperator replacement = new BoundAddressOfOperator(factory.Syntax, factory.Local(pinnedTemp), fixedInitializer.ElementPointerType);
		BoundExpression right = ApplyConversionIfNotIdentity(fixedInitializer.ElementPointerConversion, fixedInitializer.ElementPointerPlaceholder, replacement);
		BoundStatement boundStatement2 = InstrumentLocalDeclarationIfNecessary(localDecl, localSymbol, factory.Assignment(factory.Local(localSymbol), right));
		return factory.Block(boundStatement, boundStatement2);
	}

	private BoundStatement InitializeFixedStatementGetPinnable(BoundLocalDeclaration localDecl, LocalSymbol localSymbol, BoundFixedLocalCollectionInitializer fixedInitializer, SyntheticBoundNodeFactory factory, out LocalSymbol pinnedTemp)
	{
		TypeSymbol type = localSymbol.Type;
		BoundExpression boundExpression = VisitExpression(fixedInitializer.Expression);
		TypeSymbol type2 = boundExpression.Type;
		SyntaxNode syntax = boundExpression.Syntax;
		MethodSymbol getPinnableOpt = fixedInitializer.GetPinnableOpt;
		VariableDeclaratorSyntax syntax2 = fixedInitializer.Syntax.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
		pinnedTemp = factory.SynthesizedLocal(getPinnableOpt.ReturnType, syntax2, isPinned: true, isKnownToReferToTempIfReferenceType: false, RefKind.In, SynthesizedLocalKind.FixedReference);
		int id = 0;
		bool num = !type2.IsValueType || type2.IsNullableType();
		BoundAssignmentOperator store = null;
		BoundLocal boundLocal = null;
		BoundExpression receiver;
		if (num)
		{
			if (type2.IsNullableType())
			{
				boundLocal = factory.StoreToTemp(boundExpression, out store);
				receiver = boundLocal;
			}
			else
			{
				id = ++_currentConditionalAccessID;
				receiver = new BoundConditionalReceiver(syntax, id, type2);
			}
		}
		else
		{
			receiver = boundExpression;
		}
		receiver = ConvertReceiverForExtensionMemberIfNeeded(getPinnableOpt, receiver, markAsChecked: true);
		BoundExpression item = factory.AssignmentExpression(right: getPinnableOpt.IsStatic ? factory.Call(null, getPinnableOpt, receiver) : factory.Call(receiver, getPinnableOpt), left: factory.Local(pinnedTemp), isRef: true);
		BoundExpression boundExpression2 = factory.Sequence(result: ApplyConversionIfNotIdentity(replacement: new BoundAddressOfOperator(factory.Syntax, factory.Local(pinnedTemp), fixedInitializer.ElementPointerType), conversion: fixedInitializer.ElementPointerConversion, placeholder: fixedInitializer.ElementPointerPlaceholder), locals: ImmutableArray<LocalSymbol>.Empty, sideEffects: ImmutableArray.Create(item));
		if (num)
		{
			if (type2.IsNullableType())
			{
				boundExpression2 = RewriteConditionalOperator(syntax, factory.MakeNullableHasValue(syntax, boundLocal), boundExpression2, _factory.Default(type), null, type, isRef: false);
				boundExpression2 = factory.Sequence(ImmutableArray.Create(boundLocal.LocalSymbol), ImmutableArray.Create((BoundExpression)store), boundExpression2);
			}
			else
			{
				boundExpression2 = new BoundLoweredConditionalAccess(syntax, boundExpression, null, boundExpression2, null, id, forceCopyOfNullableValueType: false, type);
			}
		}
		return InstrumentLocalDeclarationIfNecessary(localDecl, localSymbol, factory.Assignment(factory.Local(localSymbol), boundExpression2));
	}

	private BoundStatement InitializeFixedStatementStringLocal(BoundLocalDeclaration localDecl, LocalSymbol localSymbol, BoundFixedLocalCollectionInitializer fixedInitializer, SyntheticBoundNodeFactory factory, out LocalSymbol pinnedTemp)
	{
		TypeSymbol type = localSymbol.Type;
		BoundExpression boundExpression = VisitExpression(fixedInitializer.Expression);
		TypeSymbol type2 = boundExpression.Type;
		VariableDeclaratorSyntax syntax = fixedInitializer.Syntax.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
		pinnedTemp = factory.SynthesizedLocal(type2, syntax, isPinned: true, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.FixedReference);
		BoundStatement boundStatement = factory.Assignment(factory.Local(pinnedTemp), boundExpression);
		BoundExpression replacement = factory.Convert(fixedInitializer.ElementPointerType, factory.Local(pinnedTemp), Conversion.PinnedObjectToPointer);
		BoundExpression right = ApplyConversionIfNotIdentity(fixedInitializer.ElementPointerConversion, fixedInitializer.ElementPointerPlaceholder, replacement);
		BoundStatement boundStatement2 = InstrumentLocalDeclarationIfNecessary(localDecl, localSymbol, factory.Assignment(factory.Local(localSymbol), right));
		BoundExpression condition = _factory.MakeNullCheck(factory.Syntax, factory.Local(localSymbol), BinaryOperatorKind.NotEqual);
		BoundExpression right2 = factory.Binary(right: (!TryGetWellKnownTypeMember<MethodSymbol>(fixedInitializer.Syntax, WellKnownMember.System_Runtime_CompilerServices_RuntimeHelpers__get_OffsetToStringData, out MethodSymbol symbol)) ? ((BoundExpression)new BoundBadExpression(fixedInitializer.Syntax, LookupResultKind.NotInvocable, ImmutableArray<Symbol>.Empty, ImmutableArray<BoundExpression>.Empty, ErrorTypeSymbol.UnknownResultType)) : ((BoundExpression)factory.Call(null, symbol)), kind: BinaryOperatorKind.PointerAndIntAddition, type: type, left: factory.Local(localSymbol));
		BoundStatement boundStatement3 = factory.If(condition, factory.Assignment(factory.Local(localSymbol), right2));
		return factory.Block(boundStatement, boundStatement2, boundStatement3);
	}

	private BoundStatement InitializeFixedStatementArrayLocal(BoundLocalDeclaration localDecl, LocalSymbol localSymbol, BoundFixedLocalCollectionInitializer fixedInitializer, SyntheticBoundNodeFactory factory, out LocalSymbol pinnedTemp)
	{
		TypeSymbol type = localSymbol.Type;
		BoundExpression boundExpression = VisitExpression(fixedInitializer.Expression);
		TypeSymbol type2 = boundExpression.Type;
		pinnedTemp = factory.SynthesizedLocal(type2, null, isPinned: true);
		ArrayTypeSymbol obj = (ArrayTypeSymbol)pinnedTemp.Type;
		TypeWithAnnotations elementTypeWithAnnotations = obj.ElementTypeWithAnnotations;
		BoundExpression rewrittenExpr = factory.AssignmentExpression(factory.Local(pinnedTemp), boundExpression);
		BoundExpression left = _factory.MakeNullCheck(factory.Syntax, rewrittenExpr, BinaryOperatorKind.NotEqual);
		BoundExpression right = factory.Binary(left: obj.IsSZArray ? factory.ArrayLength(factory.Local(pinnedTemp)) : ((!TryGetSpecialTypeMethod(fixedInitializer.Syntax, SpecialMember.System_Array__get_Length, out MethodSymbol method)) ? ((BoundExpression)new BoundBadExpression(fixedInitializer.Syntax, LookupResultKind.NotInvocable, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundExpression)factory.Local(pinnedTemp)), ErrorTypeSymbol.UnknownResultType)) : ((BoundExpression)factory.Call(factory.Local(pinnedTemp), method))), kind: BinaryOperatorKind.IntNotEqual, type: factory.SpecialType(SpecialType.System_Boolean), right: factory.Literal(0));
		BoundExpression condition = factory.Binary(BinaryOperatorKind.LogicalBoolAnd, factory.SpecialType(SpecialType.System_Boolean), left, right);
		BoundExpression operand = factory.ArrayAccessFirstElement(factory.Local(pinnedTemp));
		BoundExpression replacement = new BoundAddressOfOperator(factory.Syntax, operand, new PointerTypeSymbol(elementTypeWithAnnotations));
		BoundExpression right2 = ApplyConversionIfNotIdentity(fixedInitializer.ElementPointerConversion, fixedInitializer.ElementPointerPlaceholder, replacement);
		BoundExpression consequence = factory.AssignmentExpression(factory.Local(localSymbol), right2);
		BoundExpression alternative = factory.AssignmentExpression(factory.Local(localSymbol), factory.Null(type));
		BoundStatement rewrittenLocalDeclaration = factory.ExpressionStatement(new BoundConditionalOperator(factory.Syntax, isRef: false, condition, consequence, alternative, null, type, wasTargetTyped: false, type));
		return InstrumentLocalDeclarationIfNecessary(localDecl, localSymbol, rewrittenLocalDeclaration);
	}

	public override BoundNode VisitForEachStatement(BoundForEachStatement node)
	{
		if (node.HasErrors)
		{
			return node;
		}
		TypeSymbol type = GetUnconvertedCollectionExpression(node, out var _).Type;
		if (type.Kind == SymbolKind.ArrayType)
		{
			if (((ArrayTypeSymbol)type).IsSZArray)
			{
				return RewriteSingleDimensionalArrayForEachStatement(node);
			}
			return RewriteMultiDimensionalArrayForEachStatement(node);
		}
		ForEachEnumeratorInfo enumeratorInfoOpt = node.EnumeratorInfoOpt;
		if (enumeratorInfoOpt != null && enumeratorInfoOpt.InlineArraySpanType != WellKnownType.Unknown)
		{
			return RewriteInlineArrayForEachStatementAsFor(node);
		}
		if (node.EnumeratorInfoOpt?.MoveNextAwaitableInfo == null && CanRewriteForEachAsFor(node.Syntax, type, out MethodSymbol indexerGet, out MethodSymbol lengthGet))
		{
			return RewriteForEachStatementAsFor(node, indexerGet, lengthGet);
		}
		return RewriteEnumeratorForEachStatement(node);
	}

	private bool CanRewriteForEachAsFor(SyntaxNode forEachSyntax, TypeSymbol nodeExpressionType, [NotNullWhen(true)] out MethodSymbol? indexerGet, [NotNullWhen(true)] out MethodSymbol? lengthGet)
	{
		return CanRewriteForEachAsFor(_compilation, forEachSyntax, nodeExpressionType, out indexerGet, out lengthGet, _diagnostics);
	}

	internal static bool CanRewriteForEachAsFor(CSharpCompilation compilation, SyntaxNode forEachSyntax, TypeSymbol nodeExpressionType, [NotNullWhen(true)] out MethodSymbol? indexerGet, [NotNullWhen(true)] out MethodSymbol? lengthGet, BindingDiagnosticBag diagnostics)
	{
		lengthGet = (indexerGet = null);
		TypeSymbol originalDefinition = nodeExpressionType.OriginalDefinition;
		if (originalDefinition.SpecialType == SpecialType.System_String)
		{
			lengthGet = UnsafeGetSpecialTypeMethod(forEachSyntax, SpecialMember.System_String__Length, compilation, diagnostics);
			indexerGet = UnsafeGetSpecialTypeMethod(forEachSyntax, SpecialMember.System_String__Chars, compilation, diagnostics);
		}
		else if ((object)originalDefinition == compilation.GetWellKnownType(WellKnownType.System_Span_T))
		{
			NamedTypeSymbol newOwner = (NamedTypeSymbol)nodeExpressionType;
			lengthGet = (MethodSymbol)(Binder.GetWellKnownTypeMember(compilation, WellKnownMember.System_Span_T__get_Length, diagnostics, null, forEachSyntax, isOptional: true)?.SymbolAsMember(newOwner));
			indexerGet = (MethodSymbol)(Binder.GetWellKnownTypeMember(compilation, WellKnownMember.System_Span_T__get_Item, diagnostics, null, forEachSyntax, isOptional: true)?.SymbolAsMember(newOwner));
		}
		else if ((object)originalDefinition == compilation.GetWellKnownType(WellKnownType.System_ReadOnlySpan_T))
		{
			NamedTypeSymbol newOwner2 = (NamedTypeSymbol)nodeExpressionType;
			lengthGet = (MethodSymbol)(Binder.GetWellKnownTypeMember(compilation, WellKnownMember.System_ReadOnlySpan_T__get_Length, diagnostics, null, forEachSyntax, isOptional: true)?.SymbolAsMember(newOwner2));
			indexerGet = (MethodSymbol)(Binder.GetWellKnownTypeMember(compilation, WellKnownMember.System_ReadOnlySpan_T__get_Item, diagnostics, null, forEachSyntax, isOptional: true)?.SymbolAsMember(newOwner2));
		}
		if ((object)lengthGet != null)
		{
			return (object)indexerGet != null;
		}
		return false;
	}

	private BoundStatement RewriteEnumeratorForEachStatement(BoundForEachStatement node)
	{
		ForEachEnumeratorInfo enumeratorInfoOpt = node.EnumeratorInfoOpt;
		BoundStatement rewrittenBody = VisitStatement(node.Body);
		return RewriteForEachEnumerator(node, (BoundConversion)node.Expression, enumeratorInfoOpt, node.ElementPlaceholder, node.ElementConversion, node.IterationVariables, node.DeconstructionOpt, node.BreakLabel, node.ContinueLabel, rewrittenBody);
	}

	private BoundStatement RewriteForEachEnumerator(BoundNode node, BoundConversion convertedCollection, ForEachEnumeratorInfo enumeratorInfo, BoundValuePlaceholder? elementPlaceholder, BoundExpression? elementConversion, ImmutableArray<LocalSymbol> iterationVariables, BoundForEachDeconstructStep? deconstruction, LabelSymbol breakLabel, LabelSymbol continueLabel, BoundStatement rewrittenBody)
	{
		CSharpSyntaxNode cSharpSyntaxNode = (CSharpSyntaxNode)node.Syntax;
		bool num = enumeratorInfo.MoveNextAwaitableInfo != null;
		BoundExpression receiver = VisitExpression(convertedCollection.Operand);
		MethodArgumentInfo methodArgumentInfo = enumeratorInfo.GetEnumeratorInfo;
		TypeSymbol returnType = methodArgumentInfo.Method.ReturnType;
		LocalSymbol localSymbol = _factory.SynthesizedLocal(returnType, cSharpSyntaxNode, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.ForEachEnumerator);
		BoundLocal boundLocal = MakeBoundLocal(cSharpSyntaxNode, localSymbol, returnType);
		BoundExpression boundExpression = ConvertReceiverForInvocation(cSharpSyntaxNode, receiver, methodArgumentInfo.Method, convertedCollection.Conversion, enumeratorInfo.CollectionType);
		BoundExpression boundExpression2 = null;
		if (methodArgumentInfo.Method.IsExtensionMethod)
		{
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(methodArgumentInfo.Arguments.Length);
			boundExpression2 = boundExpression;
			instance.Add(boundExpression2);
			instance.AddRange(methodArgumentInfo.Arguments, 1, methodArgumentInfo.Arguments.Length - 1);
			methodArgumentInfo = new MethodArgumentInfo(methodArgumentInfo.Method, instance.ToImmutableAndFree(), default(BitVector), methodArgumentInfo.Expanded);
			boundExpression = null;
		}
		BoundExpression rewrittenInitialValue = MakeCall(methodArgumentInfo, cSharpSyntaxNode, boundExpression, boundExpression2);
		BoundStatement collectionVarDecl = MakeLocalDeclaration(cSharpSyntaxNode, localSymbol, rewrittenInitialValue);
		InstrumentForEachStatementCollectionVarDeclaration(node, ref collectionVarDecl);
		BoundExpression iterationVarValue = ApplyConversionIfNotIdentity(elementConversion, elementPlaceholder, ApplyConversionIfNotIdentity(enumeratorInfo.CurrentConversion, enumeratorInfo.CurrentPlaceholder, BoundCall.Synthesized(cSharpSyntaxNode, boundLocal, ThreeState.Unknown, enumeratorInfo.CurrentPropertyGetter)));
		BoundStatement iterationVarDecl = LocalOrDeconstructionDeclaration(cSharpSyntaxNode, deconstruction, iterationVariables, iterationVarValue);
		InstrumentForEachStatementIterationVarDeclaration(node, ref iterationVarDecl);
		BoundBlock rewrittenBody2 = CreateBlockDeclaringIterationVariables(iterationVariables, iterationVarDecl, rewrittenBody, cSharpSyntaxNode);
		BoundExpression boundExpression3 = MakeCall(enumeratorInfo.MoveNextInfo, cSharpSyntaxNode, boundLocal, null);
		BoundBlock disposalFinallyBlock = GetDisposalFinallyBlock(cSharpSyntaxNode, enumeratorInfo, returnType, boundLocal, out var hasAsyncDisposal);
		if (num)
		{
			BoundAwaitableInfo moveNextAwaitableInfo = enumeratorInfo.MoveNextAwaitableInfo;
			boundExpression3 = RewriteAwaitExpression(debugInfo: new BoundAwaitExpressionDebugInfo(s_moveNextAsyncAwaitId, (!hasAsyncDisposal) ? ((byte)1) : ((byte)0)), syntax: cSharpSyntaxNode, rewrittenExpression: boundExpression3, awaitableInfo: moveNextAwaitableInfo, type: (moveNextAwaitableInfo.GetResult ?? moveNextAwaitableInfo.RuntimeAsyncAwaitCall.Method).ReturnType, used: true);
		}
		BoundStatement boundStatement = RewriteWhileStatement(node, boundExpression3, rewrittenBody2, breakLabel, continueLabel, hasErrors: false);
		BoundStatement result;
		if (disposalFinallyBlock != null)
		{
			BoundStatement item = new BoundTryStatement(cSharpSyntaxNode, new BoundBlock(cSharpSyntaxNode, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(boundStatement)), ImmutableArray<BoundCatchBlock>.Empty, disposalFinallyBlock);
			result = new BoundBlock(cSharpSyntaxNode, ImmutableArray.Create(localSymbol), ImmutableArray.Create(collectionVarDecl, item));
		}
		else
		{
			result = new BoundBlock(cSharpSyntaxNode, ImmutableArray.Create(localSymbol), ImmutableArray.Create(collectionVarDecl, boundStatement));
		}
		InstrumentForEachStatement(node, ref result);
		return result;
	}

	private bool TryGetDisposeMethod(SyntaxNode forEachSyntax, ForEachEnumeratorInfo enumeratorInfo, out MethodSymbol disposeMethod)
	{
		if (enumeratorInfo.IsAsync)
		{
			disposeMethod = (MethodSymbol)Binder.GetWellKnownTypeMember(_compilation, WellKnownMember.System_IAsyncDisposable__DisposeAsync, _diagnostics, null, forEachSyntax);
			return (object)disposeMethod != null;
		}
		return Binder.TryGetSpecialTypeMember<MethodSymbol>(_compilation, SpecialMember.System_IDisposable__Dispose, forEachSyntax, _diagnostics, out disposeMethod);
	}

	private BoundBlock? GetDisposalFinallyBlock(CSharpSyntaxNode forEachSyntax, ForEachEnumeratorInfo enumeratorInfo, TypeSymbol enumeratorType, BoundLocal boundEnumeratorVar, out bool hasAsyncDisposal)
	{
		hasAsyncDisposal = false;
		if (!enumeratorInfo.NeedsDisposal)
		{
			return null;
		}
		NamedTypeSymbol namedTypeSymbol = null;
		bool flag = false;
		MethodSymbol disposeMethod = enumeratorInfo.PatternDisposeInfo?.Method;
		if ((object)disposeMethod == null)
		{
			TryGetDisposeMethod(forEachSyntax, enumeratorInfo, out disposeMethod);
			if ((object)disposeMethod == null)
			{
				return null;
			}
			namedTypeSymbol = disposeMethod.ContainingType;
			TypeConversions typeConversions = _factory.CurrentFunction.ContainingAssembly.CorLibrary.TypeConversions;
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = GetNewCompoundUseSiteInfo();
			flag = typeConversions.HasImplicitConversionToOrImplementsVarianceCompatibleInterface(enumeratorType, namedTypeSymbol, ref useSiteInfo, out var _);
			_diagnostics.Add(forEachSyntax, useSiteInfo);
		}
		Binder.ReportDiagnosticsIfObsolete(_diagnostics, disposeMethod, forEachSyntax, hasBaseReceiver: false, _factory.CurrentFunction, _factory.CurrentType, enumeratorInfo.Location);
		if (flag || enumeratorInfo.PatternDisposeInfo != null)
		{
			Conversion receiverConversion = (enumeratorType.IsStructType() ? Conversion.Boxing : Conversion.ImplicitReference);
			MethodArgumentInfo methodArgumentInfo = enumeratorInfo.PatternDisposeInfo;
			BoundExpression expression;
			if (methodArgumentInfo == null)
			{
				methodArgumentInfo = MethodArgumentInfo.CreateParameterlessMethod(disposeMethod);
				expression = ConvertReceiverForInvocation(forEachSyntax, boundEnumeratorVar, disposeMethod, receiverConversion, namedTypeSymbol);
			}
			else
			{
				expression = boundEnumeratorVar;
			}
			BoundExpression boundExpression = MakeCall(methodArgumentInfo, forEachSyntax, expression, null);
			BoundAwaitableInfo disposeAwaitableInfo = enumeratorInfo.DisposeAwaitableInfo;
			BoundStatement boundStatement;
			if (disposeAwaitableInfo != null)
			{
				boundStatement = WrapWithAwait(forEachSyntax, boundExpression, disposeAwaitableInfo);
				_sawAwaitInExceptionHandler = true;
				hasAsyncDisposal = true;
			}
			else
			{
				boundStatement = new BoundExpressionStatement(forEachSyntax, boundExpression);
			}
			BoundStatement item;
			if (enumeratorType.IsValueType)
			{
				item = boundStatement;
			}
			else
			{
				_factory.SpecialType(SpecialType.System_Object);
				item = RewriteIfStatement(forEachSyntax, _factory.IsNotNullReference(boundEnumeratorVar), boundStatement, hasErrors: false);
			}
			return new BoundBlock(forEachSyntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(item));
		}
		LocalSymbol localSymbol = _factory.SynthesizedLocal(namedTypeSymbol);
		BoundLocal boundLocal = MakeBoundLocal(forEachSyntax, localSymbol, namedTypeSymbol);
		BoundTypeExpression targetType = new BoundTypeExpression(forEachSyntax, null, namedTypeSymbol);
		BoundExpression rewrittenInitialValue = new BoundAsOperator(forEachSyntax, boundEnumeratorVar, targetType, null, null, namedTypeSymbol);
		BoundStatement item2 = MakeLocalDeclaration(forEachSyntax, localSymbol, rewrittenInitialValue);
		BoundExpression expression2 = BoundCall.Synthesized(forEachSyntax, boundLocal, ThreeState.Unknown, disposeMethod);
		BoundStatement rewrittenConsequence = new BoundExpressionStatement(forEachSyntax, expression2);
		BoundStatement item3 = RewriteIfStatement(forEachSyntax, new BoundBinaryOperator(forEachSyntax, BinaryOperatorKind.NotEqual, null, null, null, LookupResultKind.Viable, boundLocal, MakeLiteral(forEachSyntax, ConstantValue.Null, null), _compilation.GetSpecialType(SpecialType.System_Boolean)), rewrittenConsequence, hasErrors: false);
		return new BoundBlock(forEachSyntax, ImmutableArray.Create(localSymbol), ImmutableArray.Create(item2, item3));
	}

	private BoundStatement WrapWithAwait(SyntaxNode forEachSyntax, BoundExpression disposeCall, BoundAwaitableInfo disposeAwaitableInfoOpt)
	{
		TypeSymbol type = disposeAwaitableInfoOpt.GetResult?.ReturnType ?? _compilation.DynamicType;
		BoundAwaitExpressionDebugInfo debugInfo = new BoundAwaitExpressionDebugInfo(s_disposeAsyncAwaitId, 0);
		BoundExpression expression = RewriteAwaitExpression(forEachSyntax, disposeCall, disposeAwaitableInfoOpt, type, debugInfo, used: false);
		return new BoundExpressionStatement(forEachSyntax, expression);
	}

	private BoundExpression ConvertReceiverForInvocation(CSharpSyntaxNode syntax, BoundExpression receiver, MethodSymbol method, Conversion receiverConversion, TypeSymbol convertedReceiverType)
	{
		if (receiver.Type.IsReferenceType || !method.ContainingType.IsInterface)
		{
			receiver = MakeConversionNode(syntax, receiver, receiverConversion, convertedReceiverType, @checked: false);
		}
		return receiver;
	}

	private BoundStatement RewriteForEachStatementAsFor<TArg>(BoundForEachStatement node, GetForEachStatementAsForPreamble? getPreamble, GetForEachStatementAsForItem<TArg> getItem, GetForEachStatementAsForLength<TArg> getLength, TArg arg)
	{
		BoundExpression unconvertedCollectionExpression = GetUnconvertedCollectionExpression(node, out var _);
		BoundStatement rewrittenBody = VisitStatement(node.Body);
		return RewriteForEachStatementAsFor(node, getPreamble, getItem, getLength, arg, unconvertedCollectionExpression, node.EnumeratorInfoOpt, node.ElementPlaceholder, node.ElementConversion, node.IterationVariables, node.DeconstructionOpt, node.BreakLabel, node.ContinueLabel, rewrittenBody);
	}

	private BoundStatement RewriteForEachStatementAsFor<TArg>(BoundNode node, GetForEachStatementAsForPreamble? getPreamble, GetForEachStatementAsForItem<TArg> getItem, GetForEachStatementAsForLength<TArg> getLength, TArg arg, BoundExpression collectionExpression, ForEachEnumeratorInfo enumeratorInfo, BoundValuePlaceholder? elementPlaceholder, BoundExpression? elementConversion, ImmutableArray<LocalSymbol> iterationVariables, BoundForEachDeconstructStep? deconstructionOpt, LabelSymbol breakLabel, LabelSymbol continueLabel, BoundStatement rewrittenBody)
	{
		NamedTypeSymbol type = (NamedTypeSymbol)collectionExpression.Type;
		TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Int32);
		TypeSymbol specialType2 = _compilation.GetSpecialType(SpecialType.System_Boolean);
		BoundExpression rewrittenExpression = VisitExpression(collectionExpression);
		CSharpSyntaxNode cSharpSyntaxNode = (CSharpSyntaxNode)node.Syntax;
		LocalSymbol preambleLocal = null;
		RefKind collectionTempRefKind = RefKind.None;
		BoundStatement boundStatement = getPreamble?.Invoke(this, cSharpSyntaxNode, enumeratorInfo, ref rewrittenExpression, out preambleLocal, out collectionTempRefKind);
		LocalSymbol localSymbol = _factory.SynthesizedLocal(type, cSharpSyntaxNode, isPinned: false, isKnownToReferToTempIfReferenceType: false, collectionTempRefKind, SynthesizedLocalKind.ForEachArray);
		BoundStatement collectionVarDecl = MakeLocalDeclaration(cSharpSyntaxNode, localSymbol, rewrittenExpression);
		if (boundStatement != null)
		{
			collectionVarDecl = new BoundStatementList(collectionVarDecl.Syntax, ImmutableArray.Create(boundStatement, collectionVarDecl)).MakeCompilerGenerated();
		}
		InstrumentForEachStatementCollectionVarDeclaration(node, ref collectionVarDecl);
		BoundLocal boundArrayVar = MakeBoundLocal(cSharpSyntaxNode, localSymbol, type);
		LocalSymbol localSymbol2 = _factory.SynthesizedLocal(specialType, cSharpSyntaxNode, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.ForEachArrayIndex);
		BoundLocal boundLocal = MakeBoundLocal(cSharpSyntaxNode, localSymbol2, specialType);
		BoundStatement item = MakeLocalDeclaration(cSharpSyntaxNode, localSymbol2, MakeLiteral(cSharpSyntaxNode, ConstantValue.Default(SpecialType.System_Int32), specialType));
		BoundExpression iterationVarValue = ApplyConversionIfNotIdentity(elementConversion, elementPlaceholder, getItem(this, cSharpSyntaxNode, enumeratorInfo, boundArrayVar, boundLocal, arg));
		BoundStatement iterationVarDecl = LocalOrDeconstructionDeclaration(cSharpSyntaxNode, deconstructionOpt, iterationVariables, iterationVarValue);
		InstrumentForEachStatementIterationVarDeclaration(node, ref iterationVarDecl);
		BoundStatement rewrittenInitializer = new BoundStatementList(cSharpSyntaxNode, ImmutableArray.Create(collectionVarDecl, item));
		BoundExpression right = getLength(this, cSharpSyntaxNode, boundArrayVar, arg);
		BoundExpression rewrittenCondition = new BoundBinaryOperator(cSharpSyntaxNode, BinaryOperatorKind.IntLessThan, null, null, null, LookupResultKind.Viable, boundLocal, right, specialType2);
		BoundStatement rewrittenIncrement = MakePositionIncrement(cSharpSyntaxNode, boundLocal, specialType);
		BoundStatement rewrittenBody2 = CreateBlockDeclaringIterationVariables(iterationVariables, iterationVarDecl, rewrittenBody, cSharpSyntaxNode);
		BoundStatement result = RewriteForStatementWithoutInnerLocals(node, ((object)preambleLocal == null) ? ImmutableArray.Create(localSymbol, localSymbol2) : ImmutableArray.Create(preambleLocal, localSymbol, localSymbol2), rewrittenInitializer, rewrittenCondition, rewrittenIncrement, rewrittenBody2, breakLabel, continueLabel, node.HasErrors);
		InstrumentForEachStatement(node, ref result);
		return result;
	}

	private BoundStatement RewriteForEachStatementAsFor(BoundForEachStatement node, MethodSymbol indexerGet, MethodSymbol lengthGet)
	{
		return RewriteForEachStatementAsFor(node, null, (LocalRewriter rewriter, SyntaxNode syntax, ForEachEnumeratorInfo enumeratorInfo, BoundLocal boundArrayVar, BoundLocal boundPositionVar, (MethodSymbol indexerGet, MethodSymbol lengthGet) arg) => BoundCall.Synthesized(syntax, boundArrayVar, ThreeState.Unknown, arg.indexerGet, boundPositionVar), (LocalRewriter rewriter, SyntaxNode syntax, BoundLocal boundArrayVar, (MethodSymbol indexerGet, MethodSymbol lengthGet) arg) => BoundCall.Synthesized(syntax, boundArrayVar, ThreeState.Unknown, arg.lengthGet), (indexerGet, lengthGet));
	}

	private BoundStatement RewriteInlineArrayForEachStatementAsFor(BoundForEachStatement node)
	{
		return RewriteForEachStatementAsFor(node, GetInlineArrayForEachStatementPreambleDelegate(), GetInlineArrayForEachStatementGetItemDelegate(), GetInlineArrayForEachStatementGetLengthDelegate(), null);
	}

	private static GetForEachStatementAsForPreamble GetInlineArrayForEachStatementPreambleDelegate()
	{
		return delegate(LocalRewriter rewriter, SyntaxNode syntax, ForEachEnumeratorInfo enumeratorInfo, ref BoundExpression rewrittenExpression, out LocalSymbol? preambleLocal, out RefKind collectionTempRefKind)
		{
			BoundStatement result = null;
			preambleLocal = null;
			if (enumeratorInfo.InlineArrayUsedAsValue)
			{
				BoundLocal boundLocal = (BoundLocal)(rewrittenExpression = rewriter._factory.StoreToTemp(rewrittenExpression, out BoundAssignmentOperator store));
				result = rewriter._factory.ExpressionStatement(store);
				preambleLocal = boundLocal.LocalSymbol;
			}
			collectionTempRefKind = ((enumeratorInfo.InlineArraySpanType == WellKnownType.System_Span_T) ? RefKind.Ref : ((RefKind)5));
			return result;
		};
	}

	private static GetForEachStatementAsForItem<object?> GetInlineArrayForEachStatementGetItemDelegate()
	{
		return delegate(LocalRewriter rewriter, SyntaxNode syntax, ForEachEnumeratorInfo enumeratorInfo, BoundLocal boundArrayVar, BoundLocal boundPositionVar, object? _)
		{
			NamedTypeSymbol intType = rewriter._factory.SpecialType(SpecialType.System_Int32);
			MethodSymbol methodSymbol = ((enumeratorInfo.InlineArraySpanType != WellKnownType.System_Span_T) ? rewriter._factory.ModuleBuilderOpt.EnsureInlineArrayElementRefReadOnlyExists(syntax, intType, rewriter._diagnostics.DiagnosticBag) : rewriter._factory.ModuleBuilderOpt.EnsureInlineArrayElementRefExists(syntax, intType, rewriter._diagnostics.DiagnosticBag));
			TypeSymbol type = boundArrayVar.Type;
			methodSymbol = methodSymbol.Construct(type, type.TryGetInlineArrayElementField().Type);
			return rewriter._factory.Call(null, methodSymbol, boundArrayVar, boundPositionVar, useStrictArgumentRefKinds: true);
		};
	}

	private static GetForEachStatementAsForLength<object?> GetInlineArrayForEachStatementGetLengthDelegate()
	{
		return delegate(LocalRewriter rewriter, SyntaxNode syntax, BoundLocal boundArrayVar, object? _)
		{
			_ = boundArrayVar.Type.HasInlineArrayAttribute(out var length);
			return rewriter._factory.Literal(length);
		};
	}

	private BoundStatement LocalOrDeconstructionDeclaration(CSharpSyntaxNode syntax, BoundForEachDeconstructStep? deconstruction, ImmutableArray<LocalSymbol> iterationVariables, BoundExpression iterationVarValue)
	{
		BoundStatement result;
		if (deconstruction == null)
		{
			result = MakeLocalDeclaration(syntax, iterationVariables[0], iterationVarValue);
		}
		else
		{
			BoundDeconstructionAssignmentOperator deconstructionAssignment = deconstruction.DeconstructionAssignment;
			AddPlaceholderReplacement(deconstruction.TargetPlaceholder, iterationVarValue);
			BoundExpression expression = VisitExpression(deconstructionAssignment);
			result = new BoundExpressionStatement(deconstructionAssignment.Syntax, expression);
			RemovePlaceholderReplacement(deconstruction.TargetPlaceholder);
		}
		return result;
	}

	private static BoundBlock CreateBlockDeclaringIterationVariables(ImmutableArray<LocalSymbol> iterationVariables, BoundStatement iteratorVariableInitialization, BoundStatement rewrittenBody, SyntaxNode forEachSyntax)
	{
		return new BoundBlock(forEachSyntax, iterationVariables, ImmutableArray.Create(iteratorVariableInitialization, rewrittenBody));
	}

	private BoundStatement RewriteSingleDimensionalArrayForEachStatement(BoundForEachStatement node)
	{
		BoundExpression unconvertedCollectionExpression = GetUnconvertedCollectionExpression(node, out var _);
		BoundStatement rewrittenBody = VisitStatement(node.Body);
		return RewriteSingleDimensionalArrayForEachEnumerator(node, unconvertedCollectionExpression, node.ElementPlaceholder, node.ElementConversion, node.IterationVariables, node.DeconstructionOpt, node.BreakLabel, node.ContinueLabel, rewrittenBody);
	}

	private BoundStatement RewriteSingleDimensionalArrayForEachEnumerator(BoundNode node, BoundExpression collectionExpression, BoundValuePlaceholder? elementPlaceholder, BoundExpression? elementConversion, ImmutableArray<LocalSymbol> iterationVariables, BoundForEachDeconstructStep? deconstruction, LabelSymbol breakLabel, LabelSymbol continueLabel, BoundStatement rewrittenBody)
	{
		CSharpSyntaxNode cSharpSyntaxNode = (CSharpSyntaxNode)node.Syntax;
		ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)collectionExpression.Type;
		BoundExpression rewrittenInitialValue = VisitExpression(collectionExpression);
		TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Int32);
		TypeSymbol specialType2 = _compilation.GetSpecialType(SpecialType.System_Boolean);
		LocalSymbol localSymbol = _factory.SynthesizedLocal(arrayTypeSymbol, cSharpSyntaxNode, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.ForEachArray);
		BoundStatement collectionVarDecl = MakeLocalDeclaration(cSharpSyntaxNode, localSymbol, rewrittenInitialValue);
		InstrumentForEachStatementCollectionVarDeclaration(node, ref collectionVarDecl);
		BoundLocal expression = MakeBoundLocal(cSharpSyntaxNode, localSymbol, arrayTypeSymbol);
		LocalSymbol localSymbol2 = _factory.SynthesizedLocal(specialType, cSharpSyntaxNode, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.ForEachArrayIndex);
		BoundLocal boundLocal = MakeBoundLocal(cSharpSyntaxNode, localSymbol2, specialType);
		BoundStatement item = MakeLocalDeclaration(cSharpSyntaxNode, localSymbol2, MakeLiteral(cSharpSyntaxNode, ConstantValue.Default(SpecialType.System_Int32), specialType));
		BoundExpression iterationVarValue = ApplyConversionIfNotIdentity(elementConversion, elementPlaceholder, new BoundArrayAccess(cSharpSyntaxNode, expression, ImmutableArray.Create((BoundExpression)boundLocal), arrayTypeSymbol.ElementType));
		BoundStatement iterationVarDecl = LocalOrDeconstructionDeclaration(cSharpSyntaxNode, deconstruction, iterationVariables, iterationVarValue);
		InstrumentForEachStatementIterationVarDeclaration(node, ref iterationVarDecl);
		BoundStatement rewrittenInitializer = new BoundStatementList(cSharpSyntaxNode, ImmutableArray.Create(collectionVarDecl, item));
		BoundExpression right = new BoundArrayLength(cSharpSyntaxNode, expression, specialType);
		BoundExpression rewrittenCondition = new BoundBinaryOperator(cSharpSyntaxNode, BinaryOperatorKind.IntLessThan, null, null, null, LookupResultKind.Viable, boundLocal, right, specialType2);
		BoundStatement rewrittenIncrement = MakePositionIncrement(cSharpSyntaxNode, boundLocal, specialType);
		BoundStatement rewrittenBody2 = CreateBlockDeclaringIterationVariables(iterationVariables, iterationVarDecl, rewrittenBody, cSharpSyntaxNode);
		BoundStatement result = RewriteForStatementWithoutInnerLocals(node, ImmutableArray.Create(localSymbol, localSymbol2), rewrittenInitializer, rewrittenCondition, rewrittenIncrement, rewrittenBody2, breakLabel, continueLabel, node.HasErrors);
		InstrumentForEachStatement(node, ref result);
		return result;
	}

	private BoundStatement RewriteMultiDimensionalArrayForEachStatement(BoundForEachStatement node)
	{
		BoundExpression unconvertedCollectionExpression = GetUnconvertedCollectionExpression(node, out var _);
		BoundStatement rewrittenBody = VisitStatement(node.Body);
		return RewriteMultiDimensionalArrayForEachEnumerator(node, unconvertedCollectionExpression, node.ElementPlaceholder, node.ElementConversion, node.IterationVariables, node.DeconstructionOpt, node.BreakLabel, node.ContinueLabel, rewrittenBody);
	}

	private BoundStatement RewriteMultiDimensionalArrayForEachEnumerator(BoundNode node, BoundExpression collectionExpression, BoundValuePlaceholder? elementPlaceholder, BoundExpression? elementConversion, ImmutableArray<LocalSymbol> iterationVariables, BoundForEachDeconstructStep? deconstruction, LabelSymbol breakLabel, LabelSymbol continueLabel, BoundStatement rewrittenBody)
	{
		CSharpSyntaxNode cSharpSyntaxNode = (CSharpSyntaxNode)node.Syntax;
		ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)collectionExpression.Type;
		int rank = arrayTypeSymbol.Rank;
		TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Int32);
		TypeSymbol specialType2 = _compilation.GetSpecialType(SpecialType.System_Boolean);
		MethodSymbol method = UnsafeGetSpecialTypeMethod(cSharpSyntaxNode, SpecialMember.System_Array__GetLowerBound);
		MethodSymbol method2 = UnsafeGetSpecialTypeMethod(cSharpSyntaxNode, SpecialMember.System_Array__GetUpperBound);
		BoundExpression rewrittenInitialValue = VisitExpression(collectionExpression);
		LocalSymbol localSymbol = _factory.SynthesizedLocal(arrayTypeSymbol, cSharpSyntaxNode, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.ForEachArray);
		BoundLocal boundLocal = MakeBoundLocal(cSharpSyntaxNode, localSymbol, arrayTypeSymbol);
		BoundStatement collectionVarDecl = MakeLocalDeclaration(cSharpSyntaxNode, localSymbol, rewrittenInitialValue);
		InstrumentForEachStatementCollectionVarDeclaration(node, ref collectionVarDecl);
		LocalSymbol[] array = new LocalSymbol[rank];
		BoundLocal[] array2 = new BoundLocal[rank];
		BoundStatement[] array3 = new BoundStatement[rank];
		for (int i = 0; i < rank; i++)
		{
			array[i] = _factory.SynthesizedLocal(specialType, cSharpSyntaxNode, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.ForEachArrayLimit);
			array2[i] = MakeBoundLocal(cSharpSyntaxNode, array[i], specialType);
			ImmutableArray<BoundExpression> arguments = ImmutableArray.Create(MakeLiteral(cSharpSyntaxNode, ConstantValue.Create(i, ConstantValueTypeDiscriminator.Int32), specialType));
			BoundExpression rewrittenInitialValue2 = BoundCall.Synthesized(cSharpSyntaxNode, boundLocal, ThreeState.Unknown, method2, arguments);
			array3[i] = MakeLocalDeclaration(cSharpSyntaxNode, array[i], rewrittenInitialValue2);
		}
		LocalSymbol[] array4 = new LocalSymbol[rank];
		BoundLocal[] array5 = new BoundLocal[rank];
		for (int j = 0; j < rank; j++)
		{
			array4[j] = _factory.SynthesizedLocal(specialType, cSharpSyntaxNode, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.ForEachArrayIndex);
			array5[j] = MakeBoundLocal(cSharpSyntaxNode, array4[j], specialType);
		}
		BoundExpression[] items = array5;
		BoundExpression iterationVarValue = ApplyConversionIfNotIdentity(elementConversion, elementPlaceholder, new BoundArrayAccess(cSharpSyntaxNode, boundLocal, ImmutableArray.Create(items), arrayTypeSymbol.ElementType));
		BoundStatement iterationVarDecl = LocalOrDeconstructionDeclaration(cSharpSyntaxNode, deconstruction, iterationVariables, iterationVarValue);
		InstrumentForEachStatementIterationVarDeclaration(node, ref iterationVarDecl);
		BoundStatement boundStatement = CreateBlockDeclaringIterationVariables(iterationVariables, iterationVarDecl, rewrittenBody, cSharpSyntaxNode);
		BoundStatement boundStatement2 = null;
		for (int num = rank - 1; num >= 0; num--)
		{
			ImmutableArray<BoundExpression> arguments2 = ImmutableArray.Create(MakeLiteral(cSharpSyntaxNode, ConstantValue.Create(num, ConstantValueTypeDiscriminator.Int32), specialType));
			BoundExpression rewrittenInitialValue3 = BoundCall.Synthesized(cSharpSyntaxNode, boundLocal, ThreeState.Unknown, method, arguments2);
			BoundStatement rewrittenInitializer = MakeLocalDeclaration(cSharpSyntaxNode, array4[num], rewrittenInitialValue3);
			LabelSymbol breakLabel2 = ((num == 0) ? breakLabel : new GeneratedLabelSymbol("break"));
			BoundExpression rewrittenCondition = new BoundBinaryOperator(cSharpSyntaxNode, BinaryOperatorKind.IntLessThanOrEqual, null, null, null, LookupResultKind.Viable, array5[num], array2[num], specialType2);
			BoundStatement rewrittenIncrement = MakePositionIncrement(cSharpSyntaxNode, array5[num], specialType);
			BoundStatement rewrittenBody2;
			LabelSymbol continueLabel2;
			if (boundStatement2 == null)
			{
				rewrittenBody2 = boundStatement;
				continueLabel2 = continueLabel;
			}
			else
			{
				rewrittenBody2 = boundStatement2;
				continueLabel2 = new GeneratedLabelSymbol("continue");
			}
			boundStatement2 = RewriteForStatementWithoutInnerLocals(node, ImmutableArray.Create(array4[num]), rewrittenInitializer, rewrittenCondition, rewrittenIncrement, rewrittenBody2, breakLabel2, continueLabel2, node.HasErrors);
		}
		BoundStatement result = new BoundBlock(cSharpSyntaxNode, ImmutableArray.Create(localSymbol).Concat(array.AsImmutableOrNull()), ImmutableArray.Create(collectionVarDecl).Concat(array3.AsImmutableOrNull()).Add(boundStatement2));
		InstrumentForEachStatement(node, ref result);
		return result;
	}

	private static BoundExpression GetUnconvertedCollectionExpression(BoundForEachStatement node, out Conversion collectionConversion)
	{
		BoundConversion boundConversion = (BoundConversion)node.Expression;
		collectionConversion = boundConversion.Conversion;
		return boundConversion.Operand;
	}

	private static BoundLocal MakeBoundLocal(CSharpSyntaxNode syntax, LocalSymbol local, TypeSymbol type)
	{
		return new BoundLocal(syntax, local, null, type);
	}

	private BoundStatement MakeLocalDeclaration(CSharpSyntaxNode syntax, LocalSymbol local, BoundExpression rewrittenInitialValue)
	{
		return RewriteLocalDeclaration(null, syntax, local, rewrittenInitialValue);
	}

	private BoundStatement MakePositionIncrement(CSharpSyntaxNode syntax, BoundLocal boundPositionVar, TypeSymbol intType)
	{
		return BoundSequencePoint.CreateHidden(new BoundExpressionStatement(syntax, new BoundAssignmentOperator(syntax, boundPositionVar, new BoundBinaryOperator(syntax, BinaryOperatorKind.IntAddition, null, null, null, LookupResultKind.Viable, boundPositionVar, MakeLiteral(syntax, ConstantValue.Create(1), intType), intType), intType)));
	}

	private void InstrumentForEachStatementCollectionVarDeclaration(BoundNode node, [NotNullIfNotNull("collectionVarDecl")] ref BoundStatement? collectionVarDecl)
	{
		if (Instrument && node is BoundForEachStatement original)
		{
			collectionVarDecl = Instrumenter.InstrumentForEachStatementCollectionVarDeclaration(original, collectionVarDecl);
		}
	}

	private void InstrumentForEachStatementIterationVarDeclaration(BoundNode node, ref BoundStatement iterationVarDecl)
	{
		if (Instrument && node is BoundForEachStatement boundForEachStatement)
		{
			if (((CommonForEachStatementSyntax)boundForEachStatement.Syntax) is ForEachVariableStatementSyntax)
			{
				iterationVarDecl = Instrumenter.InstrumentForEachStatementDeconstructionVariablesDeclaration(boundForEachStatement, iterationVarDecl);
			}
			else
			{
				iterationVarDecl = Instrumenter.InstrumentForEachStatementIterationVarDeclaration(boundForEachStatement, iterationVarDecl);
			}
		}
	}

	private void InstrumentForEachStatement(BoundNode node, ref BoundStatement result)
	{
		if (Instrument && node is BoundForEachStatement original)
		{
			result = Instrumenter.InstrumentForEachStatement(original, result);
		}
	}

	public override BoundNode VisitForStatement(BoundForStatement node)
	{
		BoundStatement rewrittenInitializer = VisitStatement(node.Initializer);
		BoundExpression boundExpression = VisitExpression(node.Condition);
		BoundStatement rewrittenIncrement = VisitStatement(node.Increment);
		BoundStatement rewrittenBody = VisitStatement(node.Body);
		if (boundExpression != null && Instrument)
		{
			boundExpression = Instrumenter.InstrumentForStatementCondition(node, boundExpression, _factory);
		}
		return RewriteForStatement(node, rewrittenInitializer, boundExpression, rewrittenIncrement, rewrittenBody);
	}

	private BoundStatement RewriteForStatementWithoutInnerLocals(BoundNode original, ImmutableArray<LocalSymbol> outerLocals, BoundStatement? rewrittenInitializer, BoundExpression? rewrittenCondition, BoundStatement? rewrittenIncrement, BoundStatement rewrittenBody, LabelSymbol breakLabel, LabelSymbol continueLabel, bool hasErrors)
	{
		SyntaxNode syntax = original.Syntax;
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		if (rewrittenInitializer != null)
		{
			instance.Add(rewrittenInitializer);
		}
		GeneratedLabelSymbol label = new GeneratedLabelSymbol("start");
		GeneratedLabelSymbol label2 = new GeneratedLabelSymbol("end");
		BoundStatement boundStatement = new BoundGotoStatement(syntax, label2);
		if (Instrument)
		{
			boundStatement = BoundSequencePoint.CreateHidden(boundStatement);
		}
		instance.Add(boundStatement);
		instance.Add(new BoundLabelStatement(syntax, label));
		instance.Add(rewrittenBody);
		instance.Add(new BoundLabelStatement(syntax, continueLabel));
		if (rewrittenIncrement != null)
		{
			instance.Add(rewrittenIncrement);
		}
		instance.Add(new BoundLabelStatement(syntax, label2));
		BoundStatement boundStatement2 = null;
		boundStatement2 = ((rewrittenCondition == null) ? ((BoundStatement)new BoundGotoStatement(syntax, label)) : ((BoundStatement)new BoundConditionalGoto(rewrittenCondition.Syntax, rewrittenCondition, jumpIfTrue: true, label)));
		if (Instrument)
		{
			switch (original.Kind)
			{
			case BoundKind.ForEachStatement:
				boundStatement2 = Instrumenter.InstrumentForEachStatementConditionalGotoStart((BoundForEachStatement)original, boundStatement2);
				break;
			case BoundKind.ForStatement:
				boundStatement2 = Instrumenter.InstrumentForStatementConditionalGotoStartOrBreak((BoundForStatement)original, boundStatement2);
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(original.Kind);
			case BoundKind.CollectionExpressionSpreadElement:
				break;
			}
		}
		instance.Add(boundStatement2);
		instance.Add(new BoundLabelStatement(syntax, breakLabel));
		ImmutableArray<BoundStatement> statements = instance.ToImmutableAndFree();
		return new BoundBlock(syntax, outerLocals, statements, hasErrors);
	}

	private BoundStatement RewriteForStatement(BoundForStatement node, BoundStatement? rewrittenInitializer, BoundExpression? rewrittenCondition, BoundStatement? rewrittenIncrement, BoundStatement rewrittenBody)
	{
		if (node.InnerLocals.IsEmpty)
		{
			return RewriteForStatementWithoutInnerLocals(node, node.OuterLocals, rewrittenInitializer, rewrittenCondition, rewrittenIncrement, rewrittenBody, node.BreakLabel, node.ContinueLabel, node.HasErrors);
		}
		SyntaxNode syntax = node.Syntax;
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		if (rewrittenInitializer != null)
		{
			instance.Add(rewrittenInitializer);
		}
		GeneratedLabelSymbol label = new GeneratedLabelSymbol("start");
		BoundStatement boundStatement = new BoundLabelStatement(syntax, label);
		if (Instrument)
		{
			boundStatement = BoundSequencePoint.CreateHidden(boundStatement);
		}
		instance.Add(boundStatement);
		ArrayBuilder<BoundStatement> instance2 = ArrayBuilder<BoundStatement>.GetInstance();
		if (rewrittenCondition != null)
		{
			BoundStatement boundStatement2 = new BoundConditionalGoto(rewrittenCondition.Syntax, rewrittenCondition, jumpIfTrue: false, node.BreakLabel);
			if (Instrument)
			{
				boundStatement2 = Instrumenter.InstrumentForStatementConditionalGotoStartOrBreak(node, boundStatement2);
			}
			instance2.Add(boundStatement2);
		}
		instance2.Add(rewrittenBody);
		instance2.Add(new BoundLabelStatement(syntax, node.ContinueLabel));
		if (rewrittenIncrement != null)
		{
			instance2.Add(rewrittenIncrement);
		}
		instance2.Add(new BoundGotoStatement(syntax, label));
		instance.Add(new BoundBlock(syntax, node.InnerLocals, instance2.ToImmutableAndFree()));
		instance.Add(new BoundLabelStatement(syntax, node.BreakLabel));
		ImmutableArray<BoundStatement> statements = instance.ToImmutableAndFree();
		return new BoundBlock(syntax, node.OuterLocals, statements, node.HasErrors);
	}

	public override BoundNode? VisitFunctionPointerInvocation(BoundFunctionPointerInvocation node)
	{
		BoundExpression invokedExpression = VisitExpression(node.InvokedExpression);
		MethodSymbol signature = node.FunctionPointer.Signature;
		ImmutableArray<RefKind> argumentRefKindsOpt = node.ArgumentRefKindsOpt;
		BoundExpression rewrittenReceiver = null;
		ArrayBuilder<LocalSymbol> tempsOpt = null;
		ImmutableArray<BoundExpression> rewrittenArguments = VisitArgumentsAndCaptureReceiverIfNeeded(ref rewrittenReceiver, forceReceiverCapturing: false, node.Arguments, signature, default(ImmutableArray<int>), argumentRefKindsOpt, null, ref tempsOpt);
		SimpleNameSyntax interceptableNameSyntax = node.InterceptableNameSyntax;
		if (interceptableNameSyntax != null)
		{
			(Location, MethodSymbol)? tuple = _compilation.TryGetInterceptor(interceptableNameSyntax);
			if (tuple.HasValue)
			{
				Location item = tuple.GetValueOrDefault().Item1;
				_diagnostics.Add(ErrorCode.ERR_InterceptableMethodMustBeOrdinary, item, interceptableNameSyntax.Identifier.ValueText);
			}
		}
		rewrittenArguments = MakeArguments(rewrittenArguments, signature, expanded: false, default(ImmutableArray<int>), ref argumentRefKindsOpt, ref tempsOpt);
		BoundExpression boundExpression = node.Update(invokedExpression, rewrittenArguments, argumentRefKindsOpt, node.ResultKind, node.Type);
		if (tempsOpt.Count == 0)
		{
			tempsOpt.Free();
		}
		else
		{
			boundExpression = new BoundSequence(boundExpression.Syntax, tempsOpt.ToImmutableAndFree(), ImmutableArray<BoundExpression>.Empty, boundExpression, node.Type);
		}
		if (Instrument)
		{
			boundExpression = Instrumenter.InstrumentFunctionPointerInvocation(node, boundExpression);
		}
		return boundExpression;
	}

	public override BoundNode VisitGotoStatement(BoundGotoStatement node)
	{
		BoundExpression caseExpressionOpt = null;
		BoundLabel labelExpressionOpt = null;
		BoundStatement boundStatement = node.Update(node.Label, caseExpressionOpt, labelExpressionOpt);
		if (Instrument && !node.WasCompilerGenerated)
		{
			boundStatement = Instrumenter.InstrumentGotoStatement(node, boundStatement);
		}
		return boundStatement;
	}

	public override BoundNode? VisitLabel(BoundLabel node)
	{
		return null;
	}

	public override BoundNode VisitHostObjectMemberReference(BoundHostObjectMemberReference node)
	{
		SyntaxNode syntax = node.Syntax;
		FieldSymbol hostObjectField = _previousSubmissionFields.GetHostObjectField();
		BoundThisReference receiver = new BoundThisReference(syntax, _factory.CurrentType);
		return new BoundFieldAccess(syntax, receiver, hostObjectField, null);
	}

	public override BoundNode VisitIfStatement(BoundIfStatement node)
	{
		ArrayBuilder<(BoundIfStatement, GeneratedLabelSymbol, int)> instance = ArrayBuilder<(BoundIfStatement, GeneratedLabelSymbol, int)>.GetInstance();
		ArrayBuilder<BoundStatement> instance2 = ArrayBuilder<BoundStatement>.GetInstance();
		while (true)
		{
			BoundExpression boundExpression = VisitExpression(node.Condition);
			BoundStatement item = VisitStatement(node.Consequence);
			if (Instrument && !node.WasCompilerGenerated)
			{
				boundExpression = Instrumenter.InstrumentIfStatementCondition(node, boundExpression, _factory);
			}
			BoundIfStatement boundIfStatement = node.AlternativeOpt as BoundIfStatement;
			BoundStatement boundStatement = null;
			if (boundIfStatement == null)
			{
				boundStatement = VisitStatement(node.AlternativeOpt);
			}
			GeneratedLabelSymbol generatedLabelSymbol = new GeneratedLabelSymbol("afterif");
			instance.Push((node, generatedLabelSymbol, instance2.Count));
			if (boundIfStatement == null && boundStatement == null)
			{
				instance2.Add(new BoundConditionalGoto(boundExpression.Syntax, boundExpression, jumpIfTrue: false, generatedLabelSymbol));
				instance2.Add(item);
				break;
			}
			GeneratedLabelSymbol label = new GeneratedLabelSymbol("alternative");
			instance2.Add(new BoundConditionalGoto(boundExpression.Syntax, boundExpression, jumpIfTrue: false, label));
			instance2.Add(item);
			instance2.Add(BoundSequencePoint.CreateHidden());
			IfStatementSyntax syntax = (IfStatementSyntax)node.Syntax;
			instance2.Add(new BoundGotoStatement(syntax, generatedLabelSymbol));
			instance2.Add(new BoundLabelStatement(syntax, label));
			if (boundStatement != null)
			{
				instance2.Add(boundStatement);
				break;
			}
			node = boundIfStatement;
		}
		do
		{
			(BoundIfStatement, GeneratedLabelSymbol, int) tuple = instance.Pop();
			node = tuple.Item1;
			GeneratedLabelSymbol item2 = tuple.Item2;
			int item3 = tuple.Item3;
			IfStatementSyntax syntax2 = (IfStatementSyntax)node.Syntax;
			instance2.Add(BoundSequencePoint.CreateHidden());
			instance2.Add(new BoundLabelStatement(syntax2, item2));
			if (Instrument && !node.WasCompilerGenerated)
			{
				instance2[item3] = Instrumenter.InstrumentIfStatementConditionalGoto(node, instance2[item3]);
			}
		}
		while (instance.Any());
		instance.Free();
		return new BoundStatementList(node.Syntax, instance2.ToImmutableAndFree(), node.HasErrors);
	}

	private static BoundStatement RewriteIfStatement(SyntaxNode syntax, BoundExpression rewrittenCondition, BoundStatement rewrittenConsequence, bool hasErrors)
	{
		GeneratedLabelSymbol label = new GeneratedLabelSymbol("afterif");
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		instance.Add(new BoundConditionalGoto(rewrittenCondition.Syntax, rewrittenCondition, jumpIfTrue: false, label));
		instance.Add(rewrittenConsequence);
		instance.Add(BoundSequencePoint.CreateHidden());
		instance.Add(new BoundLabelStatement(syntax, label));
		ImmutableArray<BoundStatement> statements = instance.ToImmutableAndFree();
		return new BoundStatementList(syntax, statements, hasErrors);
	}

	public override BoundNode VisitFromEndIndexExpression(BoundFromEndIndexExpression node)
	{
		NamedTypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
		BoundExpression boundExpression = MakeLiteral(node.Syntax, ConstantValue.Create(value: true), specialType);
		BoundExpression boundExpression2 = VisitExpression(node.Operand);
		if (NullableNeverHasValue(boundExpression2))
		{
			boundExpression2 = new BoundDefaultExpression(boundExpression2.Syntax, boundExpression2.Type.GetNullableUnderlyingType());
		}
		boundExpression2 = NullableAlwaysHasValue(boundExpression2) ?? boundExpression2;
		if (!node.Type.IsNullableType())
		{
			return new BoundObjectCreationExpression(node.Syntax, node.MethodOpt, boundExpression2, boundExpression);
		}
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		ArrayBuilder<LocalSymbol> instance2 = ArrayBuilder<LocalSymbol>.GetInstance();
		boundExpression2 = CaptureExpressionInTempIfNeeded(boundExpression2, instance, instance2);
		BoundExpression rewrittenCondition = MakeOptimizedHasValue(boundExpression2.Syntax, boundExpression2);
		BoundExpression boundExpression3 = MakeOptimizedGetValueOrDefault(boundExpression2.Syntax, boundExpression2);
		BoundExpression underlyingValue = new BoundObjectCreationExpression(node.Syntax, node.MethodOpt, boundExpression3, boundExpression);
		BoundExpression rewrittenConsequence = ConvertToNullable(node.Syntax, node.Type, underlyingValue);
		BoundExpression rewrittenAlternative = new BoundDefaultExpression(node.Syntax, node.Type);
		BoundExpression value = RewriteConditionalOperator(node.Syntax, rewrittenCondition, rewrittenConsequence, rewrittenAlternative, null, node.Type, isRef: false);
		return new BoundSequence(node.Syntax, instance2.ToImmutableAndFree(), instance.ToImmutableAndFree(), value, node.Type);
	}

	private BoundExpression ConvertToNullable(SyntaxNode syntax, TypeSymbol targetNullableType, BoundExpression underlyingValue)
	{
		if (!TryGetNullableMethod(syntax, targetNullableType, SpecialMember.System_Nullable_T__ctor, out MethodSymbol result))
		{
			return BadExpression(syntax, targetNullableType, underlyingValue);
		}
		return new BoundObjectCreationExpression(syntax, result, underlyingValue);
	}

	private BoundExpression MakeDynamicIndexerAccessReceiver(BoundDynamicIndexerAccess indexerAccess, BoundExpression loweredReceiver)
	{
		string text = indexerAccess.TryGetIndexedPropertyName();
		if (text != null)
		{
			return _dynamicFactory.MakeDynamicGetMember(loweredReceiver, text, resultIndexed: true).ToExpression();
		}
		return loweredReceiver;
	}

	public override BoundNode VisitDynamicIndexerAccess(BoundDynamicIndexerAccess node)
	{
		BoundExpression loweredReceiver = VisitExpression(node.Receiver);
		ImmutableArray<BoundExpression> loweredArguments = VisitList(node.Arguments);
		return MakeDynamicGetIndex(node, loweredReceiver, loweredArguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt);
	}

	private BoundExpression MakeDynamicGetIndex(BoundDynamicIndexerAccess node, BoundExpression loweredReceiver, ImmutableArray<BoundExpression> loweredArguments, ImmutableArray<string?> argumentNames, ImmutableArray<RefKind> refKinds)
	{
		EmbedIfNeedTo(loweredReceiver, node.ApplicableIndexers, node.Syntax);
		return _dynamicFactory.MakeDynamicGetIndex(MakeDynamicIndexerAccessReceiver(node, loweredReceiver), loweredArguments, argumentNames, refKinds).ToExpression();
	}

	public override BoundNode VisitIndexerAccess(BoundIndexerAccess node)
	{
		return VisitIndexerAccess(node, isLeftOfAssignment: false);
	}

	private BoundExpression VisitIndexerAccess(BoundIndexerAccess node, bool isLeftOfAssignment)
	{
		PropertySymbol indexer = node.Indexer;
		BoundExpression rewrittenReceiver = VisitExpression(node.ReceiverOpt);
		return MakeIndexerAccess(node.Syntax, rewrittenReceiver, indexer, node.Arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, node.Expanded, node.ArgsToParamsOpt, node.DefaultArguments, node, isLeftOfAssignment);
	}

	private BoundExpression MakeIndexerAccess(SyntaxNode syntax, BoundExpression rewrittenReceiver, PropertySymbol indexer, ImmutableArray<BoundExpression> arguments, ImmutableArray<string?> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, bool expanded, ImmutableArray<int> argsToParamsOpt, BitVector defaultArguments, BoundExpression oldNode, bool isLeftOfAssignment)
	{
		if (isLeftOfAssignment && indexer.RefKind == RefKind.None)
		{
			TypeSymbol type = indexer.Type;
			if (!(oldNode is BoundIndexerAccess boundIndexerAccess))
			{
				if (oldNode is BoundObjectInitializerMember boundObjectInitializerMember)
				{
					return new BoundIndexerAccess(syntax, rewrittenReceiver, ThreeState.Unknown, indexer, arguments, argumentNamesOpt, argumentRefKindsOpt, expanded, boundObjectInitializerMember.AccessorKind, argsToParamsOpt, defaultArguments, type);
				}
				throw ExceptionUtilities.UnexpectedValue(oldNode);
			}
			return boundIndexerAccess.Update(rewrittenReceiver, ThreeState.Unknown, indexer, arguments, argumentNamesOpt, argumentRefKindsOpt, expanded, boundIndexerAccess.AccessorKind, argsToParamsOpt, defaultArguments, type);
		}
		MethodSymbol ownOrInheritedGetMethod = indexer.GetOwnOrInheritedGetMethod();
		ArrayBuilder<LocalSymbol> tempsOpt = null;
		bool flag = false;
		ArrayBuilder<BoundExpression> arrayBuilder = null;
		if (IsExtensionPropertyWithByValPossiblyStructReceiverWhichHasHomeAndCanChangeValueBetweenReads(rewrittenReceiver, indexer))
		{
			flag = true;
			arrayBuilder = ArrayBuilder<BoundExpression>.GetInstance();
		}
		ImmutableArray<BoundExpression> rewrittenArguments = VisitArgumentsAndCaptureReceiverIfNeeded(ref rewrittenReceiver, flag, arguments, indexer, argsToParamsOpt, argumentRefKindsOpt, arrayBuilder, ref tempsOpt);
		rewrittenArguments = ((!flag) ? MakeArguments(rewrittenArguments, indexer, expanded, argsToParamsOpt, ref argumentRefKindsOpt, ref tempsOpt) : ExtractSideEffectsFromArguments(rewrittenArguments, indexer, expanded, argsToParamsOpt, ref argumentRefKindsOpt, arrayBuilder, tempsOpt));
		ImmutableArray<BoundExpression> sideEffects = arrayBuilder?.ToImmutableAndFree() ?? ImmutableArray<BoundExpression>.Empty;
		BoundExpression boundExpression = MakePropertyGetAccess(syntax, rewrittenReceiver, indexer, rewrittenArguments, argumentRefKindsOpt, ownOrInheritedGetMethod);
		if (tempsOpt.Count == 0)
		{
			tempsOpt.Free();
			return boundExpression;
		}
		return new BoundSequence(syntax, tempsOpt.ToImmutableAndFree(), sideEffects, boundExpression, boundExpression.Type);
	}

	public override BoundNode? VisitInlineArrayAccess(BoundInlineArrayAccess node)
	{
		BoundExpression boundExpression = VisitExpression(node.Expression);
		BoundAssignmentOperator store = null;
		if (node.IsValue && node.GetItemOrSliceHelper == WellKnownMember.System_ReadOnlySpan_T__get_Item)
		{
			boundExpression = _factory.StoreToTemp(boundExpression, out store);
		}
		MethodSymbol methodSymbol = (MethodSymbol)_compilation.GetWellKnownTypeMember(node.GetItemOrSliceHelper);
		node.Expression.Type.HasInlineArrayAttribute(out var length);
		ArrayBuilder<LocalSymbol> instance;
		ArrayBuilder<BoundExpression> instance2;
		BoundExpression result;
		if (node.Argument.Type.SpecialType == SpecialType.System_Int32)
		{
			result = getElementRef(node, boundExpression, VisitExpression(node.Argument), methodSymbol, length);
		}
		else
		{
			if (!TypeSymbol.Equals(node.Argument.Type, _compilation.GetWellKnownType(WellKnownType.System_Index), TypeCompareKind.AllIgnoreOptions))
			{
				MethodSymbol methodSymbol2 = getCreateSpanHelper(node, methodSymbol.ContainingType, (NamedTypeSymbol)methodSymbol.Parameters[0].Type);
				methodSymbol = methodSymbol.AsMember((NamedTypeSymbol)methodSymbol2.ReturnType);
				RewriteRangeParts(node.Argument, out BoundRangeExpression rangeExpr, out BoundExpression startMakeOffsetInput, out PatternIndexOffsetLoweringStrategy startStrategy, out BoundExpression endMakeOffsetInput, out PatternIndexOffsetLoweringStrategy endStrategy, out BoundExpression rewrittenRangeArg);
				instance = ArrayBuilder<LocalSymbol>.GetInstance();
				instance2 = ArrayBuilder<BoundExpression>.GetInstance();
				BoundExpression startExpr;
				BoundExpression rangeSizeExpr;
				if (rangeExpr != null)
				{
					startExpr = makePatternIndexOffsetExpression(startMakeOffsetInput, length, startStrategy);
					BoundExpression endExpr = makePatternIndexOffsetExpression(endMakeOffsetInput, length, endStrategy);
					rangeSizeExpr = MakeRangeSize(ref startExpr, endExpr, instance, instance2);
				}
				else
				{
					DeconstructRange(rewrittenRangeArg, _factory.Literal(length), instance, instance2, out startExpr, out rangeSizeExpr);
				}
				BoundExpression boundExpression2 = boundExpression;
				if (instance2.Count != 0)
				{
					boundExpression2 = _factory.StoreToTemp(boundExpression2, out BoundAssignmentOperator store2, (methodSymbol2.Parameters[0].RefKind != RefKind.In) ? RefKind.Ref : ((RefKind)5));
					instance.Insert(0, ((BoundLocal)boundExpression2).LocalSymbol);
					instance2.Insert(0, store2);
				}
				ConstantValue constantValueOpt = startExpr.ConstantValueOpt;
				if ((object)constantValueOpt != null && constantValueOpt.SpecialType == SpecialType.System_Int32 && constantValueOpt.Int32Value == 0)
				{
					constantValueOpt = rangeSizeExpr.ConstantValueOpt;
					if ((object)constantValueOpt != null && constantValueOpt.SpecialType == SpecialType.System_Int32)
					{
						int int32Value = constantValueOpt.Int32Value;
						if (int32Value >= 0 && int32Value <= length)
						{
							result = _factory.Call(null, methodSymbol2, boundExpression2, rangeSizeExpr, useStrictArgumentRefKinds: true);
							goto IL_0288;
						}
					}
				}
				result = _factory.Call(_factory.Call(null, methodSymbol2, boundExpression2, _factory.Literal(length), useStrictArgumentRefKinds: true), methodSymbol, startExpr, rangeSizeExpr);
				goto IL_0288;
			}
			BoundExpression makeOffsetInput = DetermineMakePatternIndexOffsetExpressionStrategy(node.Argument, out var strategy);
			BoundExpression index = makePatternIndexOffsetExpression(makeOffsetInput, length, strategy);
			result = getElementRef(node, boundExpression, index, methodSymbol, length);
		}
		goto IL_02a3;
		IL_02a3:
		if (store != null)
		{
			result = _factory.Sequence(ImmutableArray.Create(((BoundLocal)boundExpression).LocalSymbol), ImmutableArray.Create((BoundExpression)store), result);
		}
		return result;
		IL_0288:
		result = _factory.Sequence(instance.ToImmutableAndFree(), instance2.ToImmutableAndFree(), result);
		goto IL_02a3;
		MethodSymbol getCreateSpanHelper(BoundInlineArrayAccess boundInlineArrayAccess, NamedTypeSymbol spanType, NamedTypeSymbol intType)
		{
			WellKnownMember getItemOrSliceHelper = boundInlineArrayAccess.GetItemOrSliceHelper;
			bool flag = ((getItemOrSliceHelper == WellKnownMember.System_ReadOnlySpan_T__get_Item || getItemOrSliceHelper == WellKnownMember.System_ReadOnlySpan_T__Slice_Int_Int) ? true : false);
			MethodSymbol methodSymbol3 = ((!flag) ? _factory.ModuleBuilderOpt.EnsureInlineArrayAsSpanExists(boundInlineArrayAccess.Syntax, spanType, intType, _diagnostics.DiagnosticBag) : _factory.ModuleBuilderOpt.EnsureInlineArrayAsReadOnlySpanExists(boundInlineArrayAccess.Syntax, spanType, intType, _diagnostics.DiagnosticBag));
			return methodSymbol3.Construct(boundInlineArrayAccess.Expression.Type, boundInlineArrayAccess.Expression.Type.TryGetInlineArrayElementField().Type);
		}
		BoundExpression getElementRef(BoundInlineArrayAccess boundInlineArrayAccess, BoundExpression rewrittenReceiver, BoundExpression boundExpression3, MethodSymbol getItemOrSliceHelper, int num)
		{
			NamedTypeSymbol intType = (NamedTypeSymbol)boundExpression3.Type;
			ConstantValue constantValueOpt2 = boundExpression3.ConstantValueOpt;
			if ((object)constantValueOpt2 != null && constantValueOpt2.SpecialType == SpecialType.System_Int32)
			{
				int int32Value2 = constantValueOpt2.Int32Value;
				if (int32Value2 == 0)
				{
					MethodSymbol methodSymbol3 = ((boundInlineArrayAccess.GetItemOrSliceHelper != WellKnownMember.System_Span_T__get_Item) ? _factory.ModuleBuilderOpt.EnsureInlineArrayFirstElementRefReadOnlyExists(boundInlineArrayAccess.Syntax, _diagnostics.DiagnosticBag) : _factory.ModuleBuilderOpt.EnsureInlineArrayFirstElementRefExists(boundInlineArrayAccess.Syntax, _diagnostics.DiagnosticBag));
					methodSymbol3 = methodSymbol3.Construct(boundInlineArrayAccess.Expression.Type, boundInlineArrayAccess.Expression.Type.TryGetInlineArrayElementField().Type);
					return _factory.Call(null, methodSymbol3, rewrittenReceiver, useStrictArgumentRefKinds: true);
				}
				if (int32Value2 > 0 && int32Value2 < num)
				{
					MethodSymbol methodSymbol4 = ((boundInlineArrayAccess.GetItemOrSliceHelper != WellKnownMember.System_Span_T__get_Item) ? _factory.ModuleBuilderOpt.EnsureInlineArrayElementRefReadOnlyExists(boundInlineArrayAccess.Syntax, intType, _diagnostics.DiagnosticBag) : _factory.ModuleBuilderOpt.EnsureInlineArrayElementRefExists(boundInlineArrayAccess.Syntax, intType, _diagnostics.DiagnosticBag));
					methodSymbol4 = methodSymbol4.Construct(boundInlineArrayAccess.Expression.Type, boundInlineArrayAccess.Expression.Type.TryGetInlineArrayElementField().Type);
					return _factory.Call(null, methodSymbol4, rewrittenReceiver, boundExpression3, useStrictArgumentRefKinds: true);
				}
			}
			NamedTypeSymbol containingType = getItemOrSliceHelper.ContainingType;
			MethodSymbol methodSymbol5 = getCreateSpanHelper(boundInlineArrayAccess, containingType, intType);
			getItemOrSliceHelper = getItemOrSliceHelper.AsMember((NamedTypeSymbol)methodSymbol5.ReturnType);
			return _factory.Call(_factory.Call(null, methodSymbol5, rewrittenReceiver, _factory.Literal(num), useStrictArgumentRefKinds: true), getItemOrSliceHelper, boundExpression3);
		}
		BoundExpression makePatternIndexOffsetExpression(BoundExpression? boundExpression3, int num, PatternIndexOffsetLoweringStrategy patternIndexOffsetLoweringStrategy)
		{
			if (patternIndexOffsetLoweringStrategy == PatternIndexOffsetLoweringStrategy.SubtractFromLength && boundExpression3 != null)
			{
				ConstantValue constantValueOpt2 = boundExpression3.ConstantValueOpt;
				if ((object)constantValueOpt2 != null)
				{
					int int32Value2 = constantValueOpt2.Int32Value;
					return _factory.Literal(num - int32Value2);
				}
			}
			return MakePatternIndexOffsetExpression(boundExpression3, _factory.Literal(num), patternIndexOffsetLoweringStrategy);
		}
	}

	public override BoundNode? VisitListPatternIndexPlaceholder(BoundListPatternIndexPlaceholder node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_IndexerAccess.cs", 439);
	}

	public override BoundNode? VisitListPatternReceiverPlaceholder(BoundListPatternReceiverPlaceholder node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_IndexerAccess.cs", 444);
	}

	public override BoundNode? VisitSlicePatternRangePlaceholder(BoundSlicePatternRangePlaceholder node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_IndexerAccess.cs", 449);
	}

	public override BoundNode? VisitSlicePatternReceiverPlaceholder(BoundSlicePatternReceiverPlaceholder node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_IndexerAccess.cs", 454);
	}

	public override BoundNode? VisitImplicitIndexerReceiverPlaceholder(BoundImplicitIndexerReceiverPlaceholder node)
	{
		return PlaceholderReplacement(node);
	}

	public override BoundNode? VisitImplicitIndexerValuePlaceholder(BoundImplicitIndexerValuePlaceholder node)
	{
		return PlaceholderReplacement(node);
	}

	public override BoundNode VisitImplicitIndexerAccess(BoundImplicitIndexerAccess node)
	{
		return VisitImplicitIndexerAccess(node, isLeftOfAssignment: false);
	}

	private BoundExpression VisitImplicitIndexerAccess(BoundImplicitIndexerAccess node, bool isLeftOfAssignment)
	{
		if (TypeSymbol.Equals(node.Argument.Type, _compilation.GetWellKnownType(WellKnownType.System_Index), TypeCompareKind.ConsiderEverything))
		{
			return VisitIndexPatternIndexerAccess(node, isLeftOfAssignment);
		}
		return VisitRangePatternIndexerAccess(node);
	}

	private BoundExpression VisitIndexPatternIndexerAccess(BoundImplicitIndexerAccess node, bool isLeftOfAssignment)
	{
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance(2);
		ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance(2);
		BoundExpression underlyingIndexerOrSliceAccess = GetUnderlyingIndexerOrSliceAccess(node, isLeftOfAssignment, isLeftOfAssignment, cacheAllArgumentsOnly: false, instance2, instance);
		return _factory.Sequence(instance.ToImmutableAndFree(), instance2.ToImmutableAndFree(), underlyingIndexerOrSliceAccess);
	}

	private BoundExpression GetUnderlyingIndexerOrSliceAccess(BoundImplicitIndexerAccess node, bool isLeftOfAssignment, bool isRegularAssignment, bool cacheAllArgumentsOnly, ArrayBuilder<BoundExpression> sideeffects, ArrayBuilder<LocalSymbol> locals)
	{
		SyntheticBoundNodeFactory factory = _factory;
		BoundExpression boundExpression = DetermineMakePatternIndexOffsetExpressionStrategy(node.Argument, out var strategy);
		BoundExpression rewrittenReceiver = VisitExpression(node.Receiver);
		if (!cacheAllArgumentsOnly)
		{
			bool flag = node.LengthOrCountAccess.Kind != BoundKind.Local;
			if (!flag)
			{
				BoundKind kind = rewrittenReceiver.Kind;
				bool flag2 = ((kind == BoundKind.Local || kind == BoundKind.Parameter) ? true : false);
				flag = !flag2;
			}
			if (flag)
			{
				BoundLocal boundLocal = factory.StoreToTemp(rewrittenReceiver, out BoundAssignmentOperator store, (!rewrittenReceiver.Type.IsReferenceType) ? RefKind.Ref : RefKind.None);
				locals.Add(boundLocal.LocalSymbol);
				if (boundLocal.LocalSymbol.IsRef && CodeGenerator.IsPossibleReferenceTypeReceiverOfConstrainedCall(boundLocal) && !CodeGenerator.ReceiverIsKnownToReferToTempIfReferenceType(boundLocal) && ((isLeftOfAssignment && !isRegularAssignment) || !CodeGenerator.IsSafeToDereferenceReceiverRefAfterEvaluatingArguments(ImmutableArray.Create(boundExpression))))
				{
					ReferToTempIfReferenceTypeReceiver(boundLocal, ref store, out BoundAssignmentOperator extraRefInitialization, locals);
					if (extraRefInitialization != null)
					{
						sideeffects.Add(extraRefInitialization);
					}
				}
				sideeffects.Add(store);
				rewrittenReceiver = boundLocal;
			}
		}
		AddPlaceholderReplacement(node.ReceiverPlaceholder, rewrittenReceiver);
		BoundExpression boundExpression2;
		switch (strategy)
		{
		case PatternIndexOffsetLoweringStrategy.SubtractFromLength:
		{
			BoundExpression boundExpression3 = VisitExpression(node.LengthOrCountAccess);
			if ((object)boundExpression.ConstantValueOpt == null && boundExpression3.Kind != BoundKind.ArrayLength)
			{
				boundExpression = factory.StoreToTemp(boundExpression, out BoundAssignmentOperator store2);
				locals.Add(((BoundLocal)boundExpression).LocalSymbol);
				sideeffects.Add(store2);
			}
			boundExpression2 = MakePatternIndexOffsetExpression(boundExpression, boundExpression3, strategy);
			break;
		}
		case PatternIndexOffsetLoweringStrategy.UseAsIs:
			boundExpression2 = MakePatternIndexOffsetExpression(boundExpression, null, strategy);
			break;
		case PatternIndexOffsetLoweringStrategy.UseGetOffsetAPI:
			boundExpression2 = MakePatternIndexOffsetExpression(boundExpression, VisitExpression(node.LengthOrCountAccess), strategy);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(strategy);
		}
		BoundImplicitIndexerValuePlaceholder placeholder = node.ArgumentPlaceholders[0];
		BoundExpression result;
		if (node.IndexerOrSliceAccess is BoundIndexerAccess boundIndexerAccess)
		{
			if (isLeftOfAssignment && boundIndexerAccess.GetRefKind() == RefKind.None)
			{
				AddPlaceholderReplacement(placeholder, boundExpression2);
				ImmutableArray<BoundExpression> arguments = VisitArgumentsAndCaptureReceiverIfNeeded(ref rewrittenReceiver, forceReceiverCapturing: false, boundIndexerAccess.Arguments, boundIndexerAccess.Indexer, boundIndexerAccess.ArgsToParamsOpt, boundIndexerAccess.ArgumentRefKindsOpt, null, ref locals);
				result = boundIndexerAccess.Update(rewrittenReceiver, ThreeState.Unknown, boundIndexerAccess.Indexer, arguments, boundIndexerAccess.ArgumentNamesOpt, boundIndexerAccess.ArgumentRefKindsOpt, boundIndexerAccess.Expanded, boundIndexerAccess.AccessorKind, boundIndexerAccess.ArgsToParamsOpt, boundIndexerAccess.DefaultArguments, boundIndexerAccess.Type);
			}
			else
			{
				if (cacheAllArgumentsOnly)
				{
					BoundLocal boundLocal2 = factory.StoreToTemp(boundExpression2, out BoundAssignmentOperator store3);
					locals.Add(boundLocal2.LocalSymbol);
					sideeffects.Add(store3);
					boundExpression2 = boundLocal2;
				}
				AddPlaceholderReplacement(placeholder, boundExpression2);
				result = VisitIndexerAccess(boundIndexerAccess, isLeftOfAssignment);
			}
		}
		else
		{
			if (cacheAllArgumentsOnly)
			{
				BoundLocal boundLocal3 = factory.StoreToTemp(boundExpression2, out BoundAssignmentOperator store4);
				locals.Add(boundLocal3.LocalSymbol);
				sideeffects.Add(store4);
				boundExpression2 = boundLocal3;
			}
			AddPlaceholderReplacement(placeholder, boundExpression2);
			result = (BoundExpression)VisitArrayAccess((BoundArrayAccess)node.IndexerOrSliceAccess);
		}
		RemovePlaceholderReplacement(placeholder);
		RemovePlaceholderReplacement(node.ReceiverPlaceholder);
		return result;
	}

	private BoundExpression MakePatternIndexOffsetExpression(BoundExpression? loweredExpr, BoundExpression? lengthAccess, PatternIndexOffsetLoweringStrategy strategy)
	{
		switch (strategy)
		{
		case PatternIndexOffsetLoweringStrategy.Zero:
			return _factory.Literal(0);
		case PatternIndexOffsetLoweringStrategy.Length:
			return lengthAccess;
		case PatternIndexOffsetLoweringStrategy.SubtractFromLength:
		{
			ConstantValue? constantValueOpt = loweredExpr.ConstantValueOpt;
			if ((object)constantValueOpt != null && constantValueOpt.Int32Value == 0)
			{
				return lengthAccess;
			}
			return _factory.IntSubtract(lengthAccess, loweredExpr);
		}
		case PatternIndexOffsetLoweringStrategy.UseAsIs:
			return loweredExpr;
		case PatternIndexOffsetLoweringStrategy.UseGetOffsetAPI:
			return _factory.Call(loweredExpr, WellKnownMember.System_Index__GetOffset, lengthAccess);
		default:
			throw ExceptionUtilities.UnexpectedValue(strategy);
		}
	}

	private BoundExpression DetermineMakePatternIndexOffsetExpressionStrategy(BoundExpression unloweredExpr, out PatternIndexOffsetLoweringStrategy strategy)
	{
		if (unloweredExpr is BoundFromEndIndexExpression boundFromEndIndexExpression)
		{
			strategy = PatternIndexOffsetLoweringStrategy.SubtractFromLength;
			return VisitExpression(boundFromEndIndexExpression.Operand);
		}
		if (unloweredExpr is BoundConversion boundConversion)
		{
			BoundExpression operand = boundConversion.Operand;
			if (operand != null)
			{
				TypeSymbol type = operand.Type;
				if ((object)type != null && type.SpecialType == SpecialType.System_Int32)
				{
					strategy = PatternIndexOffsetLoweringStrategy.UseAsIs;
					return VisitExpression(operand);
				}
			}
		}
		if (unloweredExpr is BoundObjectCreationExpression boundObjectCreationExpression)
		{
			MethodSymbol constructor = boundObjectCreationExpression.Constructor;
			if ((object)constructor != null)
			{
				ImmutableArray<BoundExpression> arguments = boundObjectCreationExpression.Arguments;
				if (arguments.Length == 2 && boundObjectCreationExpression.ArgsToParamsOpt.IsDefaultOrEmpty && boundObjectCreationExpression.InitializerExpressionOpt == null && (object)constructor == _compilation.GetWellKnownTypeMember(WellKnownMember.System_Index__ctor))
				{
					BoundExpression boundExpression = arguments[0];
					if (boundExpression != null)
					{
						TypeSymbol type = boundExpression.Type;
						if ((object)type != null && type.SpecialType == SpecialType.System_Int32)
						{
							ConstantValue constantValueOpt = boundExpression.ConstantValueOpt;
							if ((object)constantValueOpt != null)
							{
								object value = constantValueOpt.Value;
								if (value is int && (int)value >= 0)
								{
									BoundExpression boundExpression2 = arguments[1];
									if (boundExpression2 != null)
									{
										type = boundExpression2.Type;
										if ((object)type != null && type.SpecialType == SpecialType.System_Boolean)
										{
											constantValueOpt = boundExpression2.ConstantValueOpt;
											if ((object)constantValueOpt != null)
											{
												value = constantValueOpt.Value;
												if (value is bool)
												{
													if ((bool)value)
													{
														strategy = PatternIndexOffsetLoweringStrategy.SubtractFromLength;
													}
													else
													{
														strategy = PatternIndexOffsetLoweringStrategy.UseAsIs;
													}
													return VisitExpression(boundExpression);
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
		strategy = PatternIndexOffsetLoweringStrategy.UseGetOffsetAPI;
		return VisitExpression(unloweredExpr);
	}

	private BoundExpression VisitRangePatternIndexerAccess(BoundImplicitIndexerAccess node)
	{
		SyntheticBoundNodeFactory factory = _factory;
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance();
		return factory.Sequence(result: VisitRangePatternIndexerAccess(node, instance, instance2, cacheAllArgumentsOnly: false), locals: instance.ToImmutableAndFree(), sideEffects: instance2.ToImmutableAndFree());
	}

	private BoundExpression VisitRangePatternIndexerAccess(BoundImplicitIndexerAccess node, ArrayBuilder<LocalSymbol> localsBuilder, ArrayBuilder<BoundExpression> sideEffectsBuilder, bool cacheAllArgumentsOnly)
	{
		SyntheticBoundNodeFactory factory = _factory;
		BoundExpression boundExpression = VisitExpression(node.Receiver);
		BoundExpression argument = node.Argument;
		RewriteRangeParts(argument, out BoundRangeExpression rangeExpr, out BoundExpression startMakeOffsetInput, out PatternIndexOffsetLoweringStrategy startStrategy, out BoundExpression endMakeOffsetInput, out PatternIndexOffsetLoweringStrategy endStrategy, out BoundExpression rewrittenRangeArg);
		bool flag = node.LengthOrCountAccess.Kind != BoundKind.Local;
		if (!flag)
		{
			BoundKind kind = boundExpression.Kind;
			bool flag2 = ((kind == BoundKind.Local || kind == BoundKind.Parameter) ? true : false);
			flag = !flag2;
		}
		if (flag)
		{
			BoundLocal boundLocal = factory.StoreToTemp(boundExpression, out BoundAssignmentOperator store, (!boundExpression.Type.IsReferenceType) ? RefKind.Ref : RefKind.None);
			localsBuilder.Add(boundLocal.LocalSymbol);
			if (boundLocal.LocalSymbol.IsRef && CodeGenerator.IsPossibleReferenceTypeReceiverOfConstrainedCall(boundLocal) && !CodeGenerator.ReceiverIsKnownToReferToTempIfReferenceType(boundLocal))
			{
				ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(2);
				if (startMakeOffsetInput != null)
				{
					instance.Add(startMakeOffsetInput);
				}
				if (endMakeOffsetInput != null)
				{
					instance.Add(endMakeOffsetInput);
				}
				if (rewrittenRangeArg != null)
				{
					instance.Add(rewrittenRangeArg);
				}
				if (!CodeGenerator.IsSafeToDereferenceReceiverRefAfterEvaluatingArguments(instance.ToImmutableAndFree()))
				{
					ReferToTempIfReferenceTypeReceiver(boundLocal, ref store, out BoundAssignmentOperator extraRefInitialization, localsBuilder);
					if (extraRefInitialization != null)
					{
						sideEffectsBuilder.Add(extraRefInitialization);
					}
				}
			}
			sideEffectsBuilder.Add(store);
			boundExpression = boundLocal;
		}
		AddPlaceholderReplacement(node.ReceiverPlaceholder, boundExpression);
		BoundExpression startExpr;
		BoundExpression rangeSizeExpr;
		if (rangeExpr != null)
		{
			int num;
			switch (startStrategy)
			{
			case PatternIndexOffsetLoweringStrategy.Zero:
				switch (endStrategy)
				{
				case PatternIndexOffsetLoweringStrategy.Length:
				case PatternIndexOffsetLoweringStrategy.UseGetOffsetAPI:
					break;
				case PatternIndexOffsetLoweringStrategy.SubtractFromLength:
					goto IL_01c5;
				case PatternIndexOffsetLoweringStrategy.UseAsIs:
					goto IL_01ca;
				default:
					goto IL_01ef;
				}
				num = 4;
				break;
			case PatternIndexOffsetLoweringStrategy.UseAsIs:
				switch (endStrategy)
				{
				case PatternIndexOffsetLoweringStrategy.Length:
				case PatternIndexOffsetLoweringStrategy.UseGetOffsetAPI:
					break;
				case PatternIndexOffsetLoweringStrategy.SubtractFromLength:
					goto IL_01d4;
				case PatternIndexOffsetLoweringStrategy.UseAsIs:
					goto IL_01d9;
				default:
					goto IL_01ef;
				}
				num = 4;
				break;
			case PatternIndexOffsetLoweringStrategy.SubtractFromLength:
				switch (endStrategy)
				{
				case PatternIndexOffsetLoweringStrategy.Length:
					break;
				case PatternIndexOffsetLoweringStrategy.SubtractFromLength:
				case PatternIndexOffsetLoweringStrategy.UseGetOffsetAPI:
					goto IL_01e4;
				case PatternIndexOffsetLoweringStrategy.UseAsIs:
					goto IL_01ea;
				default:
					goto IL_01ef;
				}
				goto IL_01de;
			case PatternIndexOffsetLoweringStrategy.UseGetOffsetAPI:
				switch (endStrategy)
				{
				case PatternIndexOffsetLoweringStrategy.Length:
					break;
				case PatternIndexOffsetLoweringStrategy.SubtractFromLength:
				case PatternIndexOffsetLoweringStrategy.UseGetOffsetAPI:
					goto IL_01e4;
				case PatternIndexOffsetLoweringStrategy.UseAsIs:
					goto IL_01ea;
				default:
					goto IL_01ef;
				}
				goto IL_01de;
			default:
				goto IL_01ef;
				IL_01ea:
				num = 7;
				break;
				IL_01e4:
				num = 15;
				break;
				IL_01de:
				num = 13;
				break;
				IL_01d9:
				num = 0;
				break;
				IL_01d4:
				num = 7;
				break;
				IL_01c5:
				num = 6;
				break;
				IL_01ef:
				throw ExceptionUtilities.UnexpectedValue(startStrategy);
				IL_01ca:
				num = 0;
				break;
			}
			if ((num & 1) != 0 && (object)startMakeOffsetInput.ConstantValueOpt == null)
			{
				startMakeOffsetInput = factory.StoreToTemp(startMakeOffsetInput, out BoundAssignmentOperator store2);
				localsBuilder.Add(((BoundLocal)startMakeOffsetInput).LocalSymbol);
				sideEffectsBuilder.Add(store2);
			}
			if ((num & 2) != 0 && (object)endMakeOffsetInput.ConstantValueOpt == null)
			{
				endMakeOffsetInput = factory.StoreToTemp(endMakeOffsetInput, out BoundAssignmentOperator store3);
				localsBuilder.Add(((BoundLocal)endMakeOffsetInput).LocalSymbol);
				sideEffectsBuilder.Add(store3);
			}
			BoundExpression boundExpression2 = null;
			if ((num & 4) != 0)
			{
				boundExpression2 = VisitExpression(node.LengthOrCountAccess);
				if ((num & 8) != 0 && boundExpression2.Kind != BoundKind.Local)
				{
					BoundLocal boundLocal2 = factory.StoreToTemp(boundExpression2, out BoundAssignmentOperator store4);
					localsBuilder.Add(boundLocal2.LocalSymbol);
					sideEffectsBuilder.Add(store4);
					boundExpression2 = boundLocal2;
				}
			}
			startExpr = MakePatternIndexOffsetExpression(startMakeOffsetInput, boundExpression2, startStrategy);
			BoundExpression endExpr = MakePatternIndexOffsetExpression(endMakeOffsetInput, boundExpression2, endStrategy);
			rangeSizeExpr = MakeRangeSize(ref startExpr, endExpr, localsBuilder, sideEffectsBuilder);
			if (cacheAllArgumentsOnly)
			{
				BoundLocal boundLocal3 = factory.StoreToTemp(startExpr, out BoundAssignmentOperator store5);
				localsBuilder.Add(boundLocal3.LocalSymbol);
				sideEffectsBuilder.Add(store5);
				startExpr = boundLocal3;
				BoundLocal boundLocal4 = factory.StoreToTemp(rangeSizeExpr, out BoundAssignmentOperator store6);
				localsBuilder.Add(boundLocal4.LocalSymbol);
				sideEffectsBuilder.Add(store6);
				rangeSizeExpr = boundLocal3;
			}
		}
		else
		{
			DeconstructRange(rewrittenRangeArg, VisitExpression(node.LengthOrCountAccess), localsBuilder, sideEffectsBuilder, out startExpr, out rangeSizeExpr);
		}
		AddPlaceholderReplacement(node.ArgumentPlaceholders[0], startExpr);
		AddPlaceholderReplacement(node.ArgumentPlaceholders[1], rangeSizeExpr);
		BoundCall node2 = (BoundCall)node.IndexerOrSliceAccess;
		BoundExpression? result = VisitExpression(node2);
		RemovePlaceholderReplacement(node.ArgumentPlaceholders[0]);
		RemovePlaceholderReplacement(node.ArgumentPlaceholders[1]);
		RemovePlaceholderReplacement(node.ReceiverPlaceholder);
		return result;
	}

	private BoundExpression MakeRangeSize(ref BoundExpression startExpr, BoundExpression endExpr, ArrayBuilder<LocalSymbol> localsBuilder, ArrayBuilder<BoundExpression> sideEffectsBuilder)
	{
		SyntheticBoundNodeFactory factory = _factory;
		ConstantValue? constantValueOpt = startExpr.ConstantValueOpt;
		if ((object)constantValueOpt != null && constantValueOpt.Int32Value == 0)
		{
			return endExpr;
		}
		ConstantValue constantValueOpt2 = startExpr.ConstantValueOpt;
		if ((object)constantValueOpt2 != null)
		{
			int int32Value = constantValueOpt2.Int32Value;
			constantValueOpt2 = endExpr.ConstantValueOpt;
			if ((object)constantValueOpt2 != null)
			{
				int int32Value2 = constantValueOpt2.Int32Value;
				return factory.Literal(int32Value2 - int32Value);
			}
		}
		if ((object)startExpr.ConstantValueOpt == null)
		{
			if (startExpr is BoundLocal boundLocal)
			{
				LocalSymbol localSymbol = boundLocal.LocalSymbol;
				if ((object)localSymbol != null && localSymbol.SynthesizedKind != SynthesizedLocalKind.UserDefined)
				{
					goto IL_00b2;
				}
			}
			BoundLocal boundLocal2 = factory.StoreToTemp(startExpr, out BoundAssignmentOperator store);
			localsBuilder.Add(boundLocal2.LocalSymbol);
			sideEffectsBuilder.Add(store);
			startExpr = boundLocal2;
		}
		goto IL_00b2;
		IL_00b2:
		return factory.IntSubtract(endExpr, startExpr);
	}

	private void DeconstructRange(BoundExpression rewrittenRangeArg, BoundExpression lengthAccess, ArrayBuilder<LocalSymbol> localsBuilder, ArrayBuilder<BoundExpression> sideEffectsBuilder, out BoundExpression startExpr, out BoundExpression rangeSizeExpr)
	{
		SyntheticBoundNodeFactory factory = _factory;
		BoundLocal boundLocal = factory.StoreToTemp(rewrittenRangeArg, out BoundAssignmentOperator store);
		localsBuilder.Add(boundLocal.LocalSymbol);
		sideEffectsBuilder.Add(store);
		if ((object)lengthAccess.ConstantValueOpt == null)
		{
			BoundLocal boundLocal2 = factory.StoreToTemp(lengthAccess, out BoundAssignmentOperator store2);
			localsBuilder.Add(boundLocal2.LocalSymbol);
			sideEffectsBuilder.Add(store2);
			lengthAccess = boundLocal2;
		}
		BoundLocal boundLocal3 = factory.StoreToTemp(factory.Call(factory.Call(boundLocal, factory.WellKnownMethod(WellKnownMember.System_Range__get_Start)), factory.WellKnownMethod(WellKnownMember.System_Index__GetOffset), lengthAccess), out BoundAssignmentOperator store3);
		localsBuilder.Add(boundLocal3.LocalSymbol);
		sideEffectsBuilder.Add(store3);
		startExpr = boundLocal3;
		BoundLocal boundLocal4 = factory.StoreToTemp(factory.IntSubtract(factory.Call(factory.Call(boundLocal, factory.WellKnownMethod(WellKnownMember.System_Range__get_End)), factory.WellKnownMethod(WellKnownMember.System_Index__GetOffset), lengthAccess), startExpr), out BoundAssignmentOperator store4);
		localsBuilder.Add(boundLocal4.LocalSymbol);
		sideEffectsBuilder.Add(store4);
		rangeSizeExpr = boundLocal4;
	}

	private void RewriteRangeParts(BoundExpression rangeArg, out BoundRangeExpression? rangeExpr, out BoundExpression? startMakeOffsetInput, out PatternIndexOffsetLoweringStrategy startStrategy, out BoundExpression? endMakeOffsetInput, out PatternIndexOffsetLoweringStrategy endStrategy, out BoundExpression? rewrittenRangeArg)
	{
		startMakeOffsetInput = null;
		startStrategy = PatternIndexOffsetLoweringStrategy.Zero;
		endMakeOffsetInput = null;
		endStrategy = PatternIndexOffsetLoweringStrategy.Zero;
		rewrittenRangeArg = null;
		rangeExpr = rangeArg as BoundRangeExpression;
		if (rangeExpr != null)
		{
			BoundExpression leftOperandOpt = rangeExpr.LeftOperandOpt;
			if (leftOperandOpt != null)
			{
				startMakeOffsetInput = DetermineMakePatternIndexOffsetExpressionStrategy(leftOperandOpt, out startStrategy);
			}
			else
			{
				startStrategy = PatternIndexOffsetLoweringStrategy.Zero;
				startMakeOffsetInput = null;
			}
			BoundExpression rightOperandOpt = rangeExpr.RightOperandOpt;
			if (rightOperandOpt != null)
			{
				endMakeOffsetInput = DetermineMakePatternIndexOffsetExpressionStrategy(rightOperandOpt, out endStrategy);
				return;
			}
			endStrategy = PatternIndexOffsetLoweringStrategy.Length;
			endMakeOffsetInput = null;
		}
		else
		{
			rewrittenRangeArg = VisitExpression(rangeArg);
		}
	}

	public override BoundNode VisitIsOperator(BoundIsOperator node)
	{
		BoundExpression rewrittenOperand = VisitExpression(node.Operand);
		BoundTypeExpression rewrittenTargetType = (BoundTypeExpression)VisitTypeExpression(node.TargetType);
		TypeSymbol rewrittenType = VisitType(node.Type);
		return MakeIsOperator(node, node.Syntax, rewrittenOperand, rewrittenTargetType, node.ConversionKind, rewrittenType);
	}

	private BoundExpression MakeIsOperator(BoundIsOperator oldNode, SyntaxNode syntax, BoundExpression rewrittenOperand, BoundTypeExpression rewrittenTargetType, ConversionKind conversionKind, TypeSymbol rewrittenType)
	{
		if (rewrittenOperand.Kind == BoundKind.MethodGroup)
		{
			BoundExpression receiverOpt = ((BoundMethodGroup)rewrittenOperand).ReceiverOpt;
			if (receiverOpt != null && receiverOpt.Kind != BoundKind.ThisReference)
			{
				return RewriteConstantIsOperator(receiverOpt.Syntax, receiverOpt, ConstantValue.False, rewrittenType);
			}
			return MakeLiteral(syntax, ConstantValue.False, rewrittenType);
		}
		TypeSymbol type = rewrittenOperand.Type;
		TypeSymbol type2 = rewrittenTargetType.Type;
		if (!_inExpressionLambda)
		{
			ConstantValue isOperatorConstantResult = Binder.GetIsOperatorConstantResult(type, type2, conversionKind, rewrittenOperand.ConstantValueOpt);
			if (isOperatorConstantResult != null)
			{
				if (isOperatorConstantResult.IsBad)
				{
					throw ExceptionUtilities.UnexpectedValue(isOperatorConstantResult);
				}
				return RewriteConstantIsOperator(syntax, rewrittenOperand, isOperatorConstantResult, rewrittenType);
			}
			if (conversionKind.IsImplicitConversion())
			{
				return _factory.MakeNullCheck(syntax, rewrittenOperand, BinaryOperatorKind.NotEqual);
			}
		}
		return oldNode.Update(rewrittenOperand, rewrittenTargetType, conversionKind, rewrittenType);
	}

	private BoundExpression RewriteConstantIsOperator(SyntaxNode syntax, BoundExpression loweredOperand, ConstantValue constantValue, TypeSymbol type)
	{
		return new BoundSequence(syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(loweredOperand), MakeLiteral(syntax, constantValue, type), type);
	}

	public override BoundNode VisitIsPatternExpression(BoundIsPatternExpression node)
	{
		BoundDecisionDag decisionDagForLowering = node.GetDecisionDagForLowering(_factory.Compilation);
		bool flag = node.IsNegated;
		BoundExpression boundExpression;
		if (canProduceLinearSequence(decisionDagForLowering.RootNode, node.WhenTrueLabel, node.WhenFalseLabel))
		{
			IsPatternExpressionLinearLocalRewriter isPatternExpressionLinearLocalRewriter = new IsPatternExpressionLinearLocalRewriter(node, this);
			boundExpression = isPatternExpressionLinearLocalRewriter.LowerIsPatternAsLinearTestSequence(node, decisionDagForLowering, node.WhenTrueLabel, node.WhenFalseLabel);
			isPatternExpressionLinearLocalRewriter.Free();
		}
		else if (IsFailureNode(decisionDagForLowering.RootNode, node.WhenFalseLabel))
		{
			flag = !flag;
			IsPatternExpressionLinearLocalRewriter isPatternExpressionLinearLocalRewriter2 = new IsPatternExpressionLinearLocalRewriter(node, this);
			boundExpression = isPatternExpressionLinearLocalRewriter2.LowerIsPatternAsLinearTestSequence(node, decisionDagForLowering, node.WhenFalseLabel, node.WhenTrueLabel);
			isPatternExpressionLinearLocalRewriter2.Free();
		}
		else
		{
			IsPatternExpressionGeneralLocalRewriter isPatternExpressionGeneralLocalRewriter = new IsPatternExpressionGeneralLocalRewriter(node.Syntax, this);
			boundExpression = isPatternExpressionGeneralLocalRewriter.LowerGeneralIsPattern(node, decisionDagForLowering);
			isPatternExpressionGeneralLocalRewriter.Free();
		}
		if (flag)
		{
			boundExpression = _factory.Not(boundExpression);
		}
		return boundExpression;
		static bool canProduceLinearSequence(BoundDecisionDagNode boundDecisionDagNode, LabelSymbol whenTrueLabel, LabelSymbol whenFalseLabel)
		{
			while (true)
			{
				if (!(boundDecisionDagNode is BoundWhenDecisionDagNode boundWhenDecisionDagNode))
				{
					if (boundDecisionDagNode is BoundLeafDecisionDagNode boundLeafDecisionDagNode)
					{
						return boundLeafDecisionDagNode.Label == whenTrueLabel;
					}
					if (!(boundDecisionDagNode is BoundEvaluationDecisionDagNode boundEvaluationDecisionDagNode))
					{
						if (!(boundDecisionDagNode is BoundTestDecisionDagNode boundTestDecisionDagNode))
						{
							break;
						}
						bool flag2 = IsFailureNode(boundTestDecisionDagNode.WhenFalse, whenFalseLabel);
						if (flag2 == IsFailureNode(boundTestDecisionDagNode.WhenTrue, whenFalseLabel))
						{
							return false;
						}
						boundDecisionDagNode = (flag2 ? boundTestDecisionDagNode.WhenTrue : boundTestDecisionDagNode.WhenFalse);
					}
					else
					{
						boundDecisionDagNode = boundEvaluationDecisionDagNode.Next;
					}
				}
				else
				{
					boundDecisionDagNode = boundWhenDecisionDagNode.WhenTrue;
				}
			}
			throw ExceptionUtilities.UnexpectedValue(boundDecisionDagNode);
		}
	}

	private static bool IsFailureNode(BoundDecisionDagNode node, LabelSymbol whenFalseLabel)
	{
		if (node is BoundWhenDecisionDagNode boundWhenDecisionDagNode)
		{
			node = boundWhenDecisionDagNode.WhenTrue;
		}
		if (node is BoundLeafDecisionDagNode boundLeafDecisionDagNode)
		{
			return boundLeafDecisionDagNode.Label == whenFalseLabel;
		}
		return false;
	}

	public override BoundNode VisitLabeledStatement(BoundLabeledStatement node)
	{
		BoundStatement rewrittenBody = VisitStatement(node.Body);
		return MakeLabeledStatement(node, rewrittenBody);
	}

	private BoundStatement MakeLabeledStatement(BoundLabeledStatement node, BoundStatement? rewrittenBody)
	{
		BoundStatement boundStatement = new BoundLabelStatement(node.Syntax, node.Label);
		if (Instrument && node.Syntax is LabeledStatementSyntax)
		{
			boundStatement = Instrumenter.InstrumentLabelStatement(node, boundStatement);
		}
		if (rewrittenBody == null)
		{
			return boundStatement;
		}
		return BoundStatementList.Synthesized(node.Syntax, boundStatement, rewrittenBody);
	}

	public override BoundNode VisitLiteral(BoundLiteral node)
	{
		return MakeLiteral(node.Syntax, node.ConstantValueOpt, node.Type, node);
	}

	private BoundExpression MakeLiteral(SyntaxNode syntax, ConstantValue constantValue, TypeSymbol? type, BoundLiteral? oldNodeOpt = null)
	{
		if (constantValue.IsDecimal)
		{
			return MakeDecimalLiteral(syntax, constantValue);
		}
		if (constantValue.IsDateTime)
		{
			return MakeDateTimeLiteral(syntax, constantValue);
		}
		if (oldNodeOpt != null)
		{
			return oldNodeOpt.Update(constantValue, type);
		}
		return new BoundLiteral(syntax, constantValue, type, constantValue.IsBad);
	}

	private BoundExpression MakeDecimalLiteral(SyntaxNode syntax, ConstantValue constantValue)
	{
		decimal decimalValue = constantValue.DecimalValue;
		decimalValue.GetBits(out var isNegative, out var scale, out var low, out var mid, out var high);
		ArrayBuilder<BoundExpression> arrayBuilder = new ArrayBuilder<BoundExpression>();
		SpecialMember member;
		if (scale == 0 && -2147483648m <= decimalValue && decimalValue <= 2147483647m)
		{
			MethodSymbol currentFunction = _factory.CurrentFunction;
			if ((currentFunction.MethodKind != MethodKind.StaticConstructor || currentFunction.ContainingType.SpecialType != SpecialType.System_Decimal) && !_inExpressionLambda)
			{
				Symbol symbol = null;
				if (decimalValue == 0m)
				{
					symbol = _compilation.GetSpecialTypeMember(SpecialMember.System_Decimal__Zero);
				}
				else if (decimalValue == 1m)
				{
					symbol = _compilation.GetSpecialTypeMember(SpecialMember.System_Decimal__One);
				}
				else if (decimalValue == -1m)
				{
					symbol = _compilation.GetSpecialTypeMember(SpecialMember.System_Decimal__MinusOne);
				}
				if ((object)symbol != null && !symbol.HasUseSiteError)
				{
					NamedTypeSymbol containingType = symbol.ContainingType;
					if ((object)containingType != null && !containingType.HasUseSiteError)
					{
						FieldSymbol fieldSymbol = (FieldSymbol)symbol;
						return new BoundFieldAccess(syntax, null, fieldSymbol, constantValue);
					}
				}
			}
			member = SpecialMember.System_Decimal__CtorInt32;
			arrayBuilder.Add(new BoundLiteral(syntax, ConstantValue.Create((int)decimalValue), _compilation.GetSpecialType(SpecialType.System_Int32)));
		}
		else if (scale == 0 && 0m <= decimalValue && decimalValue <= 4294967295m)
		{
			member = SpecialMember.System_Decimal__CtorUInt32;
			arrayBuilder.Add(new BoundLiteral(syntax, ConstantValue.Create((uint)decimalValue), _compilation.GetSpecialType(SpecialType.System_UInt32)));
		}
		else if (scale == 0 && -9223372036854775808m <= decimalValue && decimalValue <= 9223372036854775807m)
		{
			member = SpecialMember.System_Decimal__CtorInt64;
			arrayBuilder.Add(new BoundLiteral(syntax, ConstantValue.Create((long)decimalValue), _compilation.GetSpecialType(SpecialType.System_Int64)));
		}
		else if (scale == 0 && 0m <= decimalValue && decimalValue <= 18446744073709551615m)
		{
			member = SpecialMember.System_Decimal__CtorUInt64;
			arrayBuilder.Add(new BoundLiteral(syntax, ConstantValue.Create((ulong)decimalValue), _compilation.GetSpecialType(SpecialType.System_UInt64)));
		}
		else
		{
			member = SpecialMember.System_Decimal__CtorInt32Int32Int32BooleanByte;
			arrayBuilder.Add(new BoundLiteral(syntax, ConstantValue.Create(low), _compilation.GetSpecialType(SpecialType.System_Int32)));
			arrayBuilder.Add(new BoundLiteral(syntax, ConstantValue.Create(mid), _compilation.GetSpecialType(SpecialType.System_Int32)));
			arrayBuilder.Add(new BoundLiteral(syntax, ConstantValue.Create(high), _compilation.GetSpecialType(SpecialType.System_Int32)));
			arrayBuilder.Add(new BoundLiteral(syntax, ConstantValue.Create(isNegative), _compilation.GetSpecialType(SpecialType.System_Boolean)));
			arrayBuilder.Add(new BoundLiteral(syntax, ConstantValue.Create(scale), _compilation.GetSpecialType(SpecialType.System_Byte)));
		}
		MethodSymbol methodSymbol = (MethodSymbol)_compilation.Assembly.GetSpecialTypeMember(member);
		return new BoundObjectCreationExpression(syntax, methodSymbol, arrayBuilder.ToImmutableAndFree(), default(ImmutableArray<string>), default(ImmutableArray<RefKind>), expanded: false, default(ImmutableArray<int>), default(BitVector), constantValue, null, methodSymbol.ContainingType);
	}

	private BoundExpression MakeDateTimeLiteral(SyntaxNode syntax, ConstantValue constantValue)
	{
		ArrayBuilder<BoundExpression> arrayBuilder = new ArrayBuilder<BoundExpression>();
		arrayBuilder.Add(new BoundLiteral(syntax, ConstantValue.Create(constantValue.DateTimeValue.Ticks), _compilation.GetSpecialType(SpecialType.System_Int64)));
		MethodSymbol methodSymbol = (MethodSymbol)_compilation.Assembly.GetSpecialTypeMember(SpecialMember.System_DateTime__CtorInt64);
		return new BoundObjectCreationExpression(syntax, methodSymbol, arrayBuilder.ToImmutableAndFree(), default(ImmutableArray<string>), default(ImmutableArray<RefKind>), expanded: false, default(ImmutableArray<int>), default(BitVector), null, null, methodSymbol.ContainingType);
	}

	public override BoundNode? VisitLocalDeclaration(BoundLocalDeclaration node)
	{
		return RewriteLocalDeclaration(node, node.Syntax, node.LocalSymbol, VisitExpression(node.InitializerOpt), node.HasErrors);
	}

	private BoundStatement? RewriteLocalDeclaration(BoundLocalDeclaration? originalOpt, SyntaxNode syntax, LocalSymbol localSymbol, BoundExpression? rewrittenInitializer, bool hasErrors = false)
	{
		if (rewrittenInitializer == null)
		{
			return null;
		}
		if (localSymbol.IsConst)
		{
			if (localSymbol.Type.IsReferenceType || localSymbol.ConstantValue != null)
			{
				return null;
			}
			hasErrors = true;
		}
		if (syntax is LocalDeclarationStatementSyntax localDeclarationStatementSyntax)
		{
			syntax = localDeclarationStatementSyntax.Declaration.Variables[0];
		}
		BoundStatement rewrittenLocalDeclaration = new BoundExpressionStatement(syntax, _factory.AssignmentExpression(syntax, new BoundLocal(syntax, localSymbol, null, localSymbol.Type), rewrittenInitializer, localSymbol.IsRef), hasErrors);
		return InstrumentLocalDeclarationIfNecessary(originalOpt, localSymbol, rewrittenLocalDeclaration);
	}

	private BoundStatement InstrumentLocalDeclarationIfNecessary(BoundLocalDeclaration? originalOpt, LocalSymbol localSymbol, BoundStatement rewrittenLocalDeclaration)
	{
		if (Instrument && originalOpt != null && !originalOpt.WasCompilerGenerated && !localSymbol.IsConst && (originalOpt.Syntax.Kind() == SyntaxKind.VariableDeclarator || (originalOpt.Syntax.Kind() == SyntaxKind.LocalDeclarationStatement && ((LocalDeclarationStatementSyntax)originalOpt.Syntax).Declaration.Variables.Count == 1)))
		{
			rewrittenLocalDeclaration = Instrumenter.InstrumentUserDefinedLocalInitialization(originalOpt, rewrittenLocalDeclaration);
		}
		return rewrittenLocalDeclaration;
	}

	public sealed override BoundNode VisitOutVariablePendingInference(OutVariablePendingInference node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_LocalDeclaration.cs", 88);
	}

	public override BoundNode VisitLockStatement(BoundLockStatement node)
	{
		LockStatementSyntax lockStatementSyntax = (LockStatementSyntax)node.Syntax;
		BoundExpression boundExpression = VisitExpression(node.Argument);
		BoundStatement boundStatement = VisitStatement(node.Body);
		TypeSymbol typeSymbol = boundExpression.Type;
		if ((object)typeSymbol == null)
		{
			typeSymbol = _compilation.GetSpecialType(SpecialType.System_Object);
			boundExpression = MakeLiteral(boundExpression.Syntax, boundExpression.ConstantValueOpt, typeSymbol);
		}
		if (typeSymbol.IsWellKnownTypeLock())
		{
			(MethodSymbol, TypeSymbol, MethodSymbol)? tuple = LockBinder.TryFindLockTypeInfo(typeSymbol, _diagnostics, boundExpression.Syntax);
			if (tuple.HasValue)
			{
				(MethodSymbol, TypeSymbol, MethodSymbol) valueOrDefault = tuple.GetValueOrDefault();
				BoundBlock tryBlock = ((boundStatement is BoundBlock boundBlock) ? boundBlock : BoundBlock.SynthesizedNoLocals(lockStatementSyntax, boundStatement));
				BoundCall argument = BoundCall.Synthesized(boundExpression.Syntax, boundExpression, ThreeState.Unknown, valueOrDefault.Item1);
				BoundLocal boundLocal = _factory.StoreToTemp(argument, out BoundAssignmentOperator store, RefKind.None, SynthesizedLocalKind.Using, isKnownToReferToTempIfReferenceType: false, lockStatementSyntax);
				BoundExpressionStatement item = new BoundExpressionStatement(boundExpression.Syntax, store);
				BoundStatement item2 = RewriteUsingStatementTryFinally(boundExpression.Syntax, boundExpression.Syntax, tryBlock, boundLocal, default(SyntaxToken), null, MethodArgumentInfo.CreateParameterlessMethod(valueOrDefault.Item3));
				return new BoundBlock(lockStatementSyntax, ImmutableArray.Create(boundLocal.LocalSymbol), ImmutableArray.Create(item, item2));
			}
			return node.Update(boundExpression, boundStatement).WithHasErrors();
		}
		if (typeSymbol.Kind == SymbolKind.TypeParameter)
		{
			typeSymbol = _compilation.GetSpecialType(SpecialType.System_Object);
			boundExpression = MakeConversionNode(boundExpression.Syntax, boundExpression, Conversion.Boxing, typeSymbol, @checked: false, explicitCastInCode: false, boundExpression.ConstantValueOpt);
		}
		BoundLocal boundLocal2 = _factory.StoreToTemp(boundExpression, out BoundAssignmentOperator store2, RefKind.None, SynthesizedLocalKind.Lock, isKnownToReferToTempIfReferenceType: false, lockStatementSyntax);
		BoundStatement lockTargetCapture = new BoundExpressionStatement(lockStatementSyntax, store2);
		BoundExpression expression = ((!TryGetWellKnownTypeMember<MethodSymbol>(lockStatementSyntax, WellKnownMember.System_Threading_Monitor__Exit, out MethodSymbol symbol)) ? ((BoundExpression)new BoundBadExpression(lockStatementSyntax, LookupResultKind.NotInvocable, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundExpression)boundLocal2), ErrorTypeSymbol.UnknownResultType)) : ((BoundExpression)BoundCall.Synthesized(lockStatementSyntax, null, ThreeState.Unknown, symbol, boundLocal2)));
		BoundStatement boundStatement2 = new BoundExpressionStatement(lockStatementSyntax, expression);
		if ((TryGetWellKnownTypeMember<MethodSymbol>(lockStatementSyntax, WellKnownMember.System_Threading_Monitor__Enter2, out MethodSymbol symbol2, isOptional: true) || TryGetWellKnownTypeMember<MethodSymbol>(lockStatementSyntax, WellKnownMember.System_Threading_Monitor__Enter, out symbol2)) && symbol2.ParameterCount == 2)
		{
			TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
			BoundLocal boundLocal3 = _factory.StoreToTemp(MakeLiteral(boundExpression.Syntax, ConstantValue.False, specialType), out BoundAssignmentOperator store3, RefKind.None, SynthesizedLocalKind.LockTaken, isKnownToReferToTempIfReferenceType: false, lockStatementSyntax);
			BoundStatement item3 = new BoundExpressionStatement(lockStatementSyntax, store3);
			BoundStatement item4 = new BoundExpressionStatement(lockStatementSyntax, BoundCall.Synthesized(lockStatementSyntax, null, ThreeState.Unknown, symbol2, boundLocal2, boundLocal3));
			boundStatement2 = RewriteIfStatement(lockStatementSyntax, boundLocal3, boundStatement2, node.HasErrors);
			return new BoundBlock(lockStatementSyntax, ImmutableArray.Create(boundLocal2.LocalSymbol, boundLocal3.LocalSymbol), ImmutableArray.Create(InstrumentLockTargetCapture(node, lockTargetCapture), item3, new BoundTryStatement(lockStatementSyntax, BoundBlock.SynthesizedNoLocals(lockStatementSyntax, ImmutableArray.Create(item4, boundStatement)), ImmutableArray<BoundCatchBlock>.Empty, BoundBlock.SynthesizedNoLocals(lockStatementSyntax, boundStatement2))));
		}
		BoundExpression expression2 = (((object)symbol2 == null) ? ((BoundExpression)new BoundBadExpression(lockStatementSyntax, LookupResultKind.NotInvocable, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundExpression)boundLocal2), ErrorTypeSymbol.UnknownResultType)) : ((BoundExpression)BoundCall.Synthesized(lockStatementSyntax, null, ThreeState.Unknown, symbol2, boundLocal2)));
		BoundStatement item5 = new BoundExpressionStatement(lockStatementSyntax, expression2);
		return new BoundBlock(lockStatementSyntax, ImmutableArray.Create(boundLocal2.LocalSymbol), ImmutableArray.Create(InstrumentLockTargetCapture(node, lockTargetCapture), item5, new BoundTryStatement(lockStatementSyntax, BoundBlock.SynthesizedNoLocals(lockStatementSyntax, boundStatement), ImmutableArray<BoundCatchBlock>.Empty, BoundBlock.SynthesizedNoLocals(lockStatementSyntax, boundStatement2))));
	}

	private BoundStatement InstrumentLockTargetCapture(BoundLockStatement original, BoundStatement lockTargetCapture)
	{
		if (!Instrument)
		{
			return lockTargetCapture;
		}
		return Instrumenter.InstrumentLockTargetCapture(original, lockTargetCapture);
	}

	public override BoundNode? VisitMultipleLocalDeclarations(BoundMultipleLocalDeclarations node)
	{
		return VisitMultipleLocalDeclarationsBase(node);
	}

	public override BoundNode? VisitUsingLocalDeclarations(BoundUsingLocalDeclarations node)
	{
		return VisitMultipleLocalDeclarationsBase(node);
	}

	private BoundNode? VisitMultipleLocalDeclarationsBase(BoundMultipleLocalDeclarationsBase node)
	{
		ArrayBuilder<BoundStatement> arrayBuilder = null;
		foreach (BoundLocalDeclaration localDeclaration in node.LocalDeclarations)
		{
			BoundNode boundNode = VisitLocalDeclaration(localDeclaration);
			if (boundNode != null)
			{
				if (arrayBuilder == null)
				{
					arrayBuilder = ArrayBuilder<BoundStatement>.GetInstance();
				}
				arrayBuilder.Add((BoundStatement)boundNode);
			}
		}
		if (arrayBuilder != null)
		{
			return BoundStatementList.Synthesized(node.Syntax, node.HasErrors, arrayBuilder.ToImmutableAndFree());
		}
		return null;
	}

	public override BoundNode VisitNullCoalescingAssignmentOperator(BoundNullCoalescingAssignmentOperator node)
	{
		SyntaxNode syntax = node.Syntax;
		ArrayBuilder<LocalSymbol> temps = ArrayBuilder<LocalSymbol>.GetInstance();
		ArrayBuilder<BoundExpression> stores = ArrayBuilder<BoundExpression>.GetInstance();
		BoundExpression transformedLHS = TransformCompoundAssignmentLHS(node.LeftOperand, stores, temps, node.LeftOperand.HasDynamicType());
		BoundExpression lhsRead = MakeRValue(transformedLHS);
		BoundExpression loweredRight = VisitExpression(node.RightOperand);
		if (!node.IsNullableValueTypeAssignment)
		{
			return rewriteNullCoalscingAssignmentStandard();
		}
		return rewriteNullCoalescingAssignmentForValueType();
		BoundExpression rewriteNullCoalescingAssignmentForValueType()
		{
			BoundExpression leftOperand = node.LeftOperand;
			if (!TryGetNullableMethod(leftOperand.Syntax, leftOperand.Type, SpecialMember.System_Nullable_T_GetValueOrDefault, out MethodSymbol result))
			{
				return BadExpression(node);
			}
			if (!TryGetNullableMethod(leftOperand.Syntax, leftOperand.Type, SpecialMember.System_Nullable_T_get_HasValue, out MethodSymbol result2))
			{
				return BadExpression(node);
			}
			if (lhsRead.Kind == BoundKind.Call)
			{
				BoundLocal boundLocal = _factory.StoreToTemp(lhsRead, out BoundAssignmentOperator store);
				stores.Add(store);
				temps.Add(boundLocal.LocalSymbol);
				lhsRead = boundLocal;
			}
			BoundLocal boundLocal2 = _factory.StoreToTemp(BoundCall.Synthesized(leftOperand.Syntax, lhsRead, ThreeState.Unknown, result), out BoundAssignmentOperator store2);
			stores.Add(store2);
			temps.Add(boundLocal2.LocalSymbol);
			BoundExpression item = MakeAssignmentOperator(node.Syntax, boundLocal2, loweredRight, used: true, isChecked: false, AssignmentKind.SimpleAssignment);
			BoundExpression item2 = MakeAssignmentOperator(node.Syntax, transformedLHS, MakeConversionNode(boundLocal2, transformedLHS.Type, @checked: false, acceptFailingConversion: false, markAsChecked: true), used: true, isChecked: false, AssignmentKind.NullCoalescingAssignment);
			BoundCall condition = BoundCall.Synthesized(leftOperand.Syntax, lhsRead, ThreeState.Unknown, result2);
			BoundExpression alternative = _factory.Sequence(ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(item, item2), boundLocal2);
			BoundExpression result3 = _factory.Conditional(condition, boundLocal2, alternative, boundLocal2.Type);
			return _factory.Sequence(temps.ToImmutableAndFree(), stores.ToImmutableAndFree(), result3);
		}
		BoundExpression rewriteNullCoalscingAssignmentStandard()
		{
			BoundExpression boundExpression;
			if (IsExtensionBlockMemberAccessWithByValPossiblyStructReceiver(transformedLHS))
			{
				BoundLocal boundLocal = _factory.StoreToTemp(loweredRight, out BoundAssignmentOperator store);
				boundExpression = MakeAssignmentOperator(syntax, transformedLHS, boundLocal, used: true, isChecked: false, AssignmentKind.NullCoalescingAssignment);
				boundExpression = new BoundSequence(syntax, ImmutableCollectionsMarshal.AsImmutableArray(new LocalSymbol[1] { boundLocal.LocalSymbol }), ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1] { store }), boundExpression, boundExpression.Type);
			}
			else
			{
				boundExpression = MakeAssignmentOperator(syntax, transformedLHS, loweredRight, used: true, isChecked: false, AssignmentKind.NullCoalescingAssignment);
			}
			BoundValuePlaceholder boundValuePlaceholder = new BoundValuePlaceholder(lhsRead.Syntax, lhsRead.Type);
			BoundExpression boundExpression2 = MakeNullCoalescingOperator(syntax, lhsRead, boundExpression, boundValuePlaceholder, boundValuePlaceholder, BoundNullCoalescingOperatorResultKind.LeftType, node.LeftOperand.Type);
			if (temps.Count != 0 || stores.Count != 0)
			{
				return new BoundSequence(syntax, temps.ToImmutableAndFree(), stores.ToImmutableAndFree(), boundExpression2, boundExpression2.Type);
			}
			return boundExpression2;
		}
	}

	public override BoundNode VisitNullCoalescingOperator(BoundNullCoalescingOperator node)
	{
		BoundExpression rewrittenLeft = VisitExpression(node.LeftOperand);
		BoundExpression rewrittenRight = VisitExpression(node.RightOperand);
		TypeSymbol rewrittenResultType = VisitType(node.Type);
		return MakeNullCoalescingOperator(node.Syntax, rewrittenLeft, rewrittenRight, node.LeftPlaceholder, node.LeftConversion, node.OperatorResultKind, rewrittenResultType);
	}

	private BoundExpression MakeNullCoalescingOperator(SyntaxNode syntax, BoundExpression rewrittenLeft, BoundExpression rewrittenRight, BoundValuePlaceholder? leftPlaceholder, BoundExpression? leftConversion, BoundNullCoalescingOperatorResultKind resultKind, TypeSymbol? rewrittenResultType)
	{
		if (_inExpressionLambda)
		{
			if (leftConversion is BoundConversion { Conversion: { IsIdentity: false } })
			{
				leftConversion = ApplyConversion(leftConversion, leftPlaceholder, leftPlaceholder);
				if (!(leftConversion is BoundConversion { Conversion: { Exists: not false } }))
				{
					return BadExpression(syntax, rewrittenResultType, rewrittenLeft, rewrittenRight);
				}
			}
			return new BoundNullCoalescingOperator(syntax, rewrittenLeft, rewrittenRight, leftPlaceholder, leftConversion, resultKind, @checked: false, rewrittenResultType);
		}
		TypeSymbol type = rewrittenLeft.Type;
		if ((object)type == null || type.IsReferenceType || type.IsValueType)
		{
			if (rewrittenLeft.IsDefaultValue())
			{
				return rewrittenRight;
			}
			if (rewrittenLeft.ConstantValueOpt != null)
			{
				return GetConvertedLeftForNullCoalescingOperator(rewrittenLeft, leftPlaceholder, leftConversion, rewrittenResultType);
			}
		}
		if (IsStringConcat(rewrittenLeft))
		{
			return GetConvertedLeftForNullCoalescingOperator(rewrittenLeft, leftPlaceholder, leftConversion, rewrittenResultType);
		}
		bool flag = rewrittenLeft.Type.IsReferenceType;
		if (flag)
		{
			ConversionKind kind = BoundNode.GetConversion(leftConversion, leftPlaceholder).Kind;
			bool flag2 = ((kind == ConversionKind.Identity || kind == ConversionKind.ImplicitReference) ? true : false);
			flag = flag2;
		}
		if (flag)
		{
			rewrittenLeft = ApplyConversionIfNotIdentity(leftConversion, leftPlaceholder, rewrittenLeft);
			return new BoundNullCoalescingOperator(syntax, rewrittenLeft, rewrittenRight, null, null, resultKind, @checked: false, rewrittenResultType);
		}
		Conversion conversion3 = BoundNode.GetConversion(leftConversion, leftPlaceholder);
		flag = ((conversion3.IsIdentity || conversion3.Kind == ConversionKind.ExplicitNullable) ? true : false);
		if (flag && rewrittenLeft is BoundLoweredConditionalAccess boundLoweredConditionalAccess && (boundLoweredConditionalAccess.WhenNullOpt == null || NullableNeverHasValue(boundLoweredConditionalAccess.WhenNullOpt)))
		{
			BoundExpression boundExpression = NullableAlwaysHasValue(boundLoweredConditionalAccess.WhenNotNull);
			if (boundExpression != null)
			{
				BoundExpression boundExpression2 = rewrittenRight;
				if (boundExpression2.Type.IsNullableType())
				{
					boundExpression = boundLoweredConditionalAccess.WhenNotNull;
				}
				if (boundExpression2.IsDefaultValue() && boundExpression2.Type.SpecialType != SpecialType.System_Decimal)
				{
					boundExpression2 = null;
				}
				return boundLoweredConditionalAccess.Update(boundLoweredConditionalAccess.Receiver, boundLoweredConditionalAccess.HasValueMethodOpt, boundExpression, boundExpression2, boundLoweredConditionalAccess.Id, boundLoweredConditionalAccess.ForceCopyOfNullableValueType, rewrittenResultType);
			}
		}
		if (rewrittenLeft.Type.IsNullableType() && rewrittenRight.Type.Equals(rewrittenLeft.Type.GetNullableUnderlyingType(), TypeCompareKind.AllIgnoreOptions))
		{
			BoundExpression boundExpression3 = RemoveIdentityConversions(rewrittenRight);
			if (boundExpression3.IsDefaultValue() && TryGetNullableMethod(rewrittenLeft.Syntax, rewrittenLeft.Type, SpecialMember.System_Nullable_T_GetValueOrDefault, out MethodSymbol result, isOptional: true))
			{
				return BoundCall.Synthesized(rewrittenLeft.Syntax, rewrittenLeft, ThreeState.Unknown, result);
			}
			if (boundExpression3 != null)
			{
				if ((object)boundExpression3.ConstantValueOpt != null)
				{
					goto IL_02b4;
				}
				if (boundExpression3 is BoundLocal boundLocal)
				{
					LocalSymbol localSymbol = boundLocal.LocalSymbol;
					if ((object)localSymbol != null && !localSymbol.IsRef)
					{
						goto IL_02b4;
					}
				}
				else if (boundExpression3 is BoundParameter boundParameter)
				{
					ParameterSymbol parameterSymbol = boundParameter.ParameterSymbol;
					if ((object)parameterSymbol != null && parameterSymbol.RefKind == RefKind.None)
					{
						goto IL_02b4;
					}
				}
			}
			flag = false;
			goto IL_02bc;
		}
		goto IL_02ea;
		IL_02b4:
		flag = true;
		goto IL_02bc;
		IL_02bc:
		if (flag && TryGetNullableMethod(rewrittenLeft.Syntax, rewrittenLeft.Type, SpecialMember.System_Nullable_T_GetValueOrDefaultDefaultValue, out MethodSymbol result2, isOptional: true))
		{
			return BoundCall.Synthesized(rewrittenLeft.Syntax, rewrittenLeft, ThreeState.Unknown, result2, rewrittenRight);
		}
		goto IL_02ea;
		IL_02ea:
		BoundLocal boundLocal2 = _factory.StoreToTemp(rewrittenLeft, out BoundAssignmentOperator store);
		BoundExpression rewrittenCondition = _factory.MakeNullCheck(syntax, boundLocal2, BinaryOperatorKind.NotEqual);
		BoundExpression convertedLeftForNullCoalescingOperator = GetConvertedLeftForNullCoalescingOperator(boundLocal2, leftPlaceholder, leftConversion, rewrittenResultType);
		BoundExpression value = RewriteConditionalOperator(syntax, rewrittenCondition, convertedLeftForNullCoalescingOperator, rewrittenRight, null, rewrittenResultType, isRef: false);
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal2.LocalSymbol), ImmutableArray.Create((BoundExpression)store), value, rewrittenResultType);
	}

	private bool IsStringConcat(BoundExpression expression)
	{
		if (expression.Kind != BoundKind.Call)
		{
			return false;
		}
		MethodSymbol method = ((BoundCall)expression).Method;
		if (method.IsStatic && method.ContainingType.SpecialType == SpecialType.System_String && ((object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatStringString) || (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatStringStringString) || (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatStringStringStringString) || (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatObject) || (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatObjectObject) || (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatObjectObjectObject) || (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatStringArray) || (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatObjectArray)))
		{
			return true;
		}
		return false;
	}

	private static BoundExpression RemoveIdentityConversions(BoundExpression expression)
	{
		while (expression.Kind == BoundKind.Conversion)
		{
			BoundConversion boundConversion = (BoundConversion)expression;
			if (boundConversion.ConversionKind != ConversionKind.Identity)
			{
				return expression;
			}
			expression = boundConversion.Operand;
		}
		return expression;
	}

	private BoundExpression GetConvertedLeftForNullCoalescingOperator(BoundExpression rewrittenLeft, BoundValuePlaceholder? leftPlaceholder, BoundExpression? leftConversion, TypeSymbol rewrittenResultType)
	{
		TypeSymbol type = rewrittenLeft.Type;
		bool flag = leftPlaceholder != null && leftPlaceholder.Type?.IsNullableType() == true;
		if (!TypeSymbol.Equals(type, rewrittenResultType, TypeCompareKind.ConsiderEverything) && type.IsNullableType() && !flag)
		{
			TypeSymbol nullableUnderlyingType = type.GetNullableUnderlyingType();
			rewrittenLeft = BoundCall.Synthesized(method: UnsafeGetNullableMethod(rewrittenLeft.Syntax, type, SpecialMember.System_Nullable_T_GetValueOrDefault), syntax: rewrittenLeft.Syntax, receiverOpt: rewrittenLeft, initialBindingReceiverIsSubjectToCloning: ThreeState.Unknown);
			if (TypeSymbol.Equals(nullableUnderlyingType, rewrittenResultType, TypeCompareKind.ConsiderEverything))
			{
				return rewrittenLeft;
			}
		}
		rewrittenLeft = ApplyConversionIfNotIdentity(leftConversion, leftPlaceholder, rewrittenLeft);
		return rewrittenLeft;
	}

	public override BoundNode VisitDynamicObjectCreationExpression(BoundDynamicObjectCreationExpression node)
	{
		ImmutableArray<BoundExpression> loweredArguments = VisitList(node.Arguments);
		BoundExpression boundExpression = _dynamicFactory.MakeDynamicConstructorInvocation(node.Syntax, node.Type, loweredArguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt).ToExpression();
		if (node.InitializerExpressionOpt == null || node.InitializerExpressionOpt.HasErrors)
		{
			return boundExpression;
		}
		return MakeExpressionWithInitializer(node.Syntax, boundExpression, node.InitializerExpressionOpt, node.Type);
	}

	public override BoundNode VisitObjectCreationExpression(BoundObjectCreationExpression node)
	{
		MethodSymbol method = node.Constructor;
		BoundExpression rewrittenReceiver = null;
		ImmutableArray<RefKind> argumentRefKindsOpt = node.ArgumentRefKindsOpt;
		ArrayBuilder<LocalSymbol> tempsOpt = null;
		ImmutableArray<BoundExpression> rewrittenArguments = VisitArgumentsAndCaptureReceiverIfNeeded(ref rewrittenReceiver, forceReceiverCapturing: false, node.Arguments, method, node.ArgsToParamsOpt, argumentRefKindsOpt, null, ref tempsOpt);
		rewrittenArguments = MakeArguments(rewrittenArguments, method, node.Expanded, node.ArgsToParamsOpt, ref argumentRefKindsOpt, ref tempsOpt);
		ImmutableArray<LocalSymbol> locals = tempsOpt.ToImmutableAndFree();
		BoundExpression boundExpression;
		if (_inExpressionLambda)
		{
			if (!locals.IsDefaultOrEmpty)
			{
				throw ExceptionUtilities.UnexpectedValue(locals.Length);
			}
			boundExpression = node.Update(method, rewrittenArguments, argumentRefKindsOpt, MakeObjectCreationInitializerForExpressionTree(node.InitializerExpressionOpt), method.ContainingType);
			if (node.Type.IsInterfaceType())
			{
				boundExpression = MakeConversionNode(boundExpression, node.Type, @checked: false);
			}
			return boundExpression;
		}
		if (Instrument)
		{
			BoundExpression receiver = null;
			Instrumenter.InterceptCallAndAdjustArguments(ref method, ref receiver, ref rewrittenArguments, ref argumentRefKindsOpt);
		}
		boundExpression = node.Update(method, rewrittenArguments, argumentRefKindsOpt, null, method.ContainingType);
		if (method.IsDefaultValueTypeConstructor())
		{
			boundExpression = new BoundDefaultExpression(boundExpression.Syntax, boundExpression.Type);
		}
		if (!locals.IsDefaultOrEmpty)
		{
			boundExpression = new BoundSequence(node.Syntax, locals, ImmutableArray<BoundExpression>.Empty, boundExpression, node.Type);
		}
		if (node.Type.IsInterfaceType())
		{
			boundExpression = MakeConversionNode(boundExpression, node.Type, @checked: false);
		}
		if (Instrument)
		{
			boundExpression = Instrumenter.InstrumentObjectCreationExpression(node, boundExpression);
		}
		if (node.InitializerExpressionOpt == null || node.InitializerExpressionOpt.HasErrors)
		{
			return boundExpression;
		}
		return MakeExpressionWithInitializer(node.Syntax, boundExpression, node.InitializerExpressionOpt, node.Type);
	}

	public override BoundNode VisitWithExpression(BoundWithExpression withExpr)
	{
		TypeSymbol type = withExpr.Type;
		BoundExpression receiver = withExpr.Receiver;
		BoundExpression boundExpression = VisitExpression(receiver);
		if (type.IsAnonymousType)
		{
			AnonymousTypeManager.AnonymousTypePublicSymbol anonymousTypePublicSymbol = (AnonymousTypeManager.AnonymousTypePublicSymbol)type;
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
			ArrayBuilder<LocalSymbol> instance2 = ArrayBuilder<LocalSymbol>.GetInstance();
			BoundLocal boundLocal = _factory.StoreToTemp(boundExpression, out BoundAssignmentOperator store);
			instance2.Add(boundLocal.LocalSymbol);
			instance.Add(store);
			BoundExpression value = _factory.New(anonymousTypePublicSymbol, getAnonymousTypeValues(withExpr, boundLocal, anonymousTypePublicSymbol, instance, instance2));
			return new BoundSequence(withExpr.Syntax, instance2.ToImmutableAndFree(), instance.ToImmutableAndFree(), value, type);
		}
		BoundExpression rewrittenExpression;
		if (type.IsValueType)
		{
			rewrittenExpression = boundExpression;
		}
		else
		{
			BoundCall arg = _factory.Call(boundExpression, withExpr.CloneMethod);
			Conversion conversion = _factory.ClassifyEmitConversion(arg, type);
			rewrittenExpression = _factory.Convert(type, arg, conversion);
		}
		return MakeExpressionWithInitializer(withExpr.Syntax, rewrittenExpression, withExpr.InitializerExpression, type);
		ImmutableArray<BoundExpression> getAnonymousTypeValues(BoundWithExpression boundWithExpression, BoundExpression oldValue, AnonymousTypeManager.AnonymousTypePublicSymbol anonymousType, ArrayBuilder<BoundExpression> sideEffects, ArrayBuilder<LocalSymbol> temps)
		{
			ArrayBuilder<BoundExpression> instance3 = ArrayBuilder<BoundExpression>.GetInstance(anonymousType.Properties.Length, null);
			foreach (BoundAssignmentOperator initializer in boundWithExpression.InitializerExpression.Initializers)
			{
				BoundObjectInitializerMember obj = (BoundObjectInitializerMember)initializer.Left;
				BoundExpression argument = VisitExpression(initializer.Right);
				BoundLocal boundLocal2 = _factory.StoreToTemp(argument, out BoundAssignmentOperator store2);
				temps.Add(boundLocal2.LocalSymbol);
				sideEffects.Add(store2);
				Symbol memberSymbol = obj.MemberSymbol;
				instance3[memberSymbol.MemberIndexOpt.Value] = boundLocal2;
			}
			ArrayBuilder<BoundExpression> instance4 = ArrayBuilder<BoundExpression>.GetInstance(anonymousType.Properties.Length);
			foreach (AnonymousTypeManager.AnonymousTypePropertySymbol property in anonymousType.Properties)
			{
				BoundExpression boundExpression2 = instance3[property.MemberIndexOpt.Value];
				if (boundExpression2 != null)
				{
					instance4.Add(boundExpression2);
				}
				else
				{
					instance4.Add(_factory.Property(oldValue, property));
				}
			}
			instance3.Free();
			return instance4.ToImmutableAndFree();
		}
	}

	[return: NotNullIfNotNull("initializerExpressionOpt")]
	private BoundObjectInitializerExpressionBase? MakeObjectCreationInitializerForExpressionTree(BoundObjectInitializerExpressionBase? initializerExpressionOpt)
	{
		if (initializerExpressionOpt != null && !initializerExpressionOpt.HasErrors)
		{
			ImmutableArray<BoundExpression> newInitializers = MakeObjectOrCollectionInitializersForExpressionTree(initializerExpressionOpt);
			return UpdateInitializers(initializerExpressionOpt, newInitializers);
		}
		return null;
	}

	private BoundExpression MakeExpressionWithInitializer(SyntaxNode syntax, BoundExpression rewrittenExpression, BoundExpression initializerExpression, TypeSymbol type)
	{
		BoundLocal boundLocal = _factory.StoreToTemp(rewrittenExpression, out BoundAssignmentOperator store, RefKind.None, SynthesizedLocalKind.LoweringTemp, isKnownToReferToTempIfReferenceType: true);
		ArrayBuilder<BoundExpression> dynamicSiteInitializers = null;
		ArrayBuilder<LocalSymbol> temps = null;
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		AddObjectOrCollectionInitializers(ref dynamicSiteInitializers, ref temps, instance, boundLocal, initializerExpression);
		int num = dynamicSiteInitializers?.Count ?? 0;
		ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance(1 + num + instance.Count);
		instance2.Add(store);
		if (num > 0)
		{
			instance2.AddRange(dynamicSiteInitializers);
			dynamicSiteInitializers.Free();
		}
		instance2.AddRange(instance);
		instance.Free();
		ImmutableArray<LocalSymbol> locals;
		if (temps == null)
		{
			locals = ImmutableArray.Create(boundLocal.LocalSymbol);
		}
		else
		{
			temps.Insert(0, boundLocal.LocalSymbol);
			locals = temps.ToImmutableAndFree();
		}
		return new BoundSequence(syntax, locals, instance2.ToImmutableAndFree(), boundLocal, type);
	}

	public override BoundNode VisitNewT(BoundNewT node)
	{
		if (_inExpressionLambda)
		{
			return node.Update(MakeObjectCreationInitializerForExpressionTree(node.InitializerExpressionOpt), node.WasTargetTyped, node.Type);
		}
		BoundExpression boundExpression = MakeNewT(node.Syntax, (TypeParameterSymbol)node.Type);
		if (node.InitializerExpressionOpt == null || node.InitializerExpressionOpt.HasErrors)
		{
			return boundExpression;
		}
		return MakeExpressionWithInitializer(node.Syntax, boundExpression, node.InitializerExpressionOpt, boundExpression.Type);
	}

	private BoundExpression MakeNewT(SyntaxNode syntax, TypeParameterSymbol typeParameter)
	{
		if (!TryGetWellKnownTypeMember<MethodSymbol>(syntax, WellKnownMember.System_Activator__CreateInstance_T, out MethodSymbol symbol))
		{
			return new BoundDefaultExpression(syntax, typeParameter, hasErrors: true);
		}
		symbol = symbol.Construct(ImmutableArray.Create((TypeSymbol)typeParameter));
		symbol.CheckConstraints(new ConstraintsHelper.CheckConstraintsArgs(_compilation, _compilation.Conversions, syntax.GetLocation(), _diagnostics));
		return new BoundCall(syntax, null, ThreeState.Unknown, symbol, ImmutableArray<BoundExpression>.Empty, default(ImmutableArray<string>), default(ImmutableArray<RefKind>), isDelegateCall: false, expanded: false, invokedAsExtensionMethod: false, default(ImmutableArray<int>), default(BitVector), LookupResultKind.Viable, typeParameter);
	}

	public override BoundNode VisitNoPiaObjectCreationExpression(BoundNoPiaObjectCreationExpression node)
	{
		SyntaxNode syntax = _factory.Syntax;
		_factory.Syntax = node.Syntax;
		MethodSymbol methodSymbol = _factory.WellKnownMethod(WellKnownMember.System_Guid__ctor);
		BoundExpression arg = (((object)methodSymbol == null) ? ((BoundExpression)new BoundBadExpression(node.Syntax, LookupResultKind.NotCreatable, ImmutableArray<Symbol>.Empty, ImmutableArray<BoundExpression>.Empty, ErrorTypeSymbol.UnknownResultType)) : ((BoundExpression)_factory.New(methodSymbol, _factory.Literal(node.GuidString))));
		MethodSymbol methodSymbol2 = _factory.WellKnownMethod(WellKnownMember.System_Runtime_InteropServices_Marshal__GetTypeFromCLSID, isOptional: true);
		if ((object)methodSymbol2 == null)
		{
			methodSymbol2 = _factory.WellKnownMethod(WellKnownMember.System_Type__GetTypeFromCLSID);
		}
		BoundExpression arg2 = (((object)methodSymbol2 == null) ? ((BoundExpression)new BoundBadExpression(node.Syntax, LookupResultKind.OverloadResolutionFailure, ImmutableArray<Symbol>.Empty, ImmutableArray<BoundExpression>.Empty, ErrorTypeSymbol.UnknownResultType)) : ((BoundExpression)_factory.Call(null, methodSymbol2, arg)));
		MethodSymbol methodSymbol3 = _factory.WellKnownMethod(WellKnownMember.System_Activator__CreateInstance);
		BoundExpression boundExpression;
		if ((object)methodSymbol3 != null)
		{
			BoundCall arg3 = _factory.Call(null, methodSymbol3, arg2);
			Conversion conversion = _factory.ClassifyEmitConversion(arg3, node.Type);
			boundExpression = _factory.Convert(node.Type, arg3, conversion);
		}
		else
		{
			boundExpression = new BoundBadExpression(node.Syntax, LookupResultKind.OverloadResolutionFailure, ImmutableArray<Symbol>.Empty, ImmutableArray<BoundExpression>.Empty, node.Type);
		}
		_factory.Syntax = syntax;
		if (node.InitializerExpressionOpt == null || node.InitializerExpressionOpt.HasErrors)
		{
			return boundExpression;
		}
		return MakeExpressionWithInitializer(node.Syntax, boundExpression, node.InitializerExpressionOpt, node.Type);
	}

	private static BoundObjectInitializerExpressionBase UpdateInitializers(BoundObjectInitializerExpressionBase initializerExpression, ImmutableArray<BoundExpression> newInitializers)
	{
		if (!(initializerExpression is BoundObjectInitializerExpression boundObjectInitializerExpression))
		{
			if (initializerExpression is BoundCollectionInitializerExpression boundCollectionInitializerExpression)
			{
				return boundCollectionInitializerExpression.Update(boundCollectionInitializerExpression.Placeholder, newInitializers, initializerExpression.Type);
			}
			throw ExceptionUtilities.UnexpectedValue(initializerExpression.Kind);
		}
		return boundObjectInitializerExpression.Update(boundObjectInitializerExpression.Placeholder, newInitializers, initializerExpression.Type);
	}

	private void AddObjectOrCollectionInitializers(ref ArrayBuilder<BoundExpression>? dynamicSiteInitializers, ref ArrayBuilder<LocalSymbol>? temps, ArrayBuilder<BoundExpression> result, BoundExpression rewrittenReceiver, BoundExpression initializerExpression)
	{
		if (!(initializerExpression is BoundObjectInitializerExpression boundObjectInitializerExpression))
		{
			if (!(initializerExpression is BoundCollectionInitializerExpression boundCollectionInitializerExpression))
			{
				throw ExceptionUtilities.UnexpectedValue(initializerExpression.Kind);
			}
			BoundObjectOrCollectionValuePlaceholder placeholder = boundCollectionInitializerExpression.Placeholder;
			AddPlaceholderReplacement(placeholder, rewrittenReceiver);
			AddCollectionInitializers(result, rewrittenReceiver, boundCollectionInitializerExpression.Initializers);
			RemovePlaceholderReplacement(placeholder);
		}
		else
		{
			BoundObjectOrCollectionValuePlaceholder placeholder2 = boundObjectInitializerExpression.Placeholder;
			AddPlaceholderReplacement(placeholder2, rewrittenReceiver);
			AddObjectInitializers(ref dynamicSiteInitializers, ref temps, result, rewrittenReceiver, boundObjectInitializerExpression.Initializers);
			RemovePlaceholderReplacement(placeholder2);
		}
	}

	private ImmutableArray<BoundExpression> MakeObjectOrCollectionInitializersForExpressionTree(BoundExpression initializerExpression)
	{
		switch (initializerExpression.Kind)
		{
		case BoundKind.ObjectInitializerExpression:
			return VisitList(((BoundObjectInitializerExpression)initializerExpression).Initializers);
		case BoundKind.CollectionInitializerExpression:
		{
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
			addCollectionInitializersForExpressionTree(instance, ((BoundCollectionInitializerExpression)initializerExpression).Initializers);
			return instance.ToImmutableAndFree();
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(initializerExpression.Kind);
		}
		void addCollectionInitializersForExpressionTree(ArrayBuilder<BoundExpression> result, ImmutableArray<BoundExpression> initializers)
		{
			foreach (BoundExpression item in initializers)
			{
				if (item.Kind != BoundKind.CollectionElementInitializer)
				{
					throw ExceptionUtilities.UnexpectedValue(item.Kind);
				}
				BoundCollectionElementInitializer boundCollectionElementInitializer = (BoundCollectionElementInitializer)item;
				result.Add(VisitExpression(boundCollectionElementInitializer.Update(boundCollectionElementInitializer.AddMethod, boundCollectionElementInitializer.Arguments, null, boundCollectionElementInitializer.Expanded, boundCollectionElementInitializer.ArgsToParamsOpt, boundCollectionElementInitializer.DefaultArguments, boundCollectionElementInitializer.InvokedAsExtensionMethod, boundCollectionElementInitializer.ResultKind, boundCollectionElementInitializer.Type)));
			}
		}
	}

	private void AddCollectionInitializers(ArrayBuilder<BoundExpression> result, BoundExpression rewrittenReceiver, ImmutableArray<BoundExpression> initializers)
	{
		foreach (BoundExpression item in initializers)
		{
			BoundExpression boundExpression = ((item.Kind != BoundKind.CollectionElementInitializer) ? MakeDynamicCollectionInitializer(rewrittenReceiver, (BoundDynamicCollectionElementInitializer)item) : MakeCollectionInitializer((BoundCollectionElementInitializer)item));
			if (boundExpression != null)
			{
				result.Add(boundExpression);
			}
		}
	}

	private BoundExpression MakeDynamicCollectionInitializer(BoundExpression rewrittenReceiver, BoundDynamicCollectionElementInitializer initializer)
	{
		ImmutableArray<BoundExpression> loweredArguments = VisitList(initializer.Arguments);
		EmbedIfNeedTo(rewrittenReceiver, initializer.ApplicableMethods, initializer.Syntax);
		return _dynamicFactory.MakeDynamicMemberInvocation("Add", rewrittenReceiver, ImmutableArray<TypeWithAnnotations>.Empty, loweredArguments, default(ImmutableArray<string>), default(ImmutableArray<RefKind>), hasImplicitReceiver: false, resultDiscarded: true).ToExpression();
	}

	private BoundExpression? MakeCollectionInitializer(BoundCollectionElementInitializer initializer)
	{
		MethodSymbol method = initializer.AddMethod;
		SyntaxNode syntax = initializer.Syntax;
		if (_allowOmissionOfConditionalCalls && method.CallsAreOmitted(initializer.SyntaxTree))
		{
			return null;
		}
		BoundExpression rewrittenReceiver = VisitExpression(initializer.ImplicitReceiverOpt);
		ImmutableArray<RefKind> argumentRefKindsOpt = default(ImmutableArray<RefKind>);
		if (initializer.InvokedAsExtensionMethod && method.Parameters[0].RefKind == RefKind.Ref)
		{
			ArrayBuilder<RefKind> instance = ArrayBuilder<RefKind>.GetInstance(method.Parameters.Length, RefKind.None);
			instance[0] = RefKind.Ref;
			argumentRefKindsOpt = instance.ToImmutableAndFree();
		}
		ArrayBuilder<LocalSymbol> tempsOpt = null;
		ImmutableArray<BoundExpression> rewrittenArguments = VisitArgumentsAndCaptureReceiverIfNeeded(ref rewrittenReceiver, forceReceiverCapturing: false, initializer.Arguments, method, initializer.ArgsToParamsOpt, argumentRefKindsOpt, null, ref tempsOpt);
		rewrittenArguments = MakeArguments(rewrittenArguments, method, initializer.Expanded, initializer.ArgsToParamsOpt, ref argumentRefKindsOpt, ref tempsOpt);
		VisitType(initializer.Type);
		if (Instrument)
		{
			Instrumenter.InterceptCallAndAdjustArguments(ref method, ref rewrittenReceiver, ref rewrittenArguments, ref argumentRefKindsOpt);
		}
		return MakeCall(null, syntax, rewrittenReceiver, method, rewrittenArguments, argumentRefKindsOpt, initializer.ResultKind, tempsOpt.ToImmutableAndFree());
	}

	private BoundExpression VisitObjectInitializerMember(BoundObjectInitializerMember node, ref BoundExpression rewrittenReceiver, ArrayBuilder<BoundExpression> sideEffects, ref ArrayBuilder<LocalSymbol>? temps)
	{
		if ((object)node.MemberSymbol == null)
		{
			return (BoundExpression)VisitObjectInitializerMember(node);
		}
		BoundExpression obj = rewrittenReceiver;
		ArrayBuilder<LocalSymbol> tempsOpt = null;
		ImmutableArray<BoundExpression> arguments = VisitArgumentsAndCaptureReceiverIfNeeded(ref rewrittenReceiver, forceReceiverCapturing: false, node.Arguments, node.MemberSymbol, node.ArgsToParamsOpt, node.ArgumentRefKindsOpt, null, ref tempsOpt);
		if (tempsOpt != null)
		{
			if (temps == null)
			{
				temps = tempsOpt;
			}
			else
			{
				temps.AddRange(tempsOpt);
				tempsOpt.Free();
			}
		}
		if (obj != rewrittenReceiver && rewrittenReceiver is BoundSequence boundSequence)
		{
			temps.AddRange(boundSequence.Locals);
			sideEffects.AddRange(boundSequence.SideEffects);
			rewrittenReceiver = boundSequence.Value;
		}
		return node.Update(node.MemberSymbol, arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, node.Expanded, node.ArgsToParamsOpt, node.DefaultArguments, node.ResultKind, node.AccessorKind, node.ReceiverType, node.Type);
	}

	private void AddObjectInitializers(ref ArrayBuilder<BoundExpression>? dynamicSiteInitializers, ref ArrayBuilder<LocalSymbol>? temps, ArrayBuilder<BoundExpression> result, BoundExpression rewrittenReceiver, ImmutableArray<BoundExpression> initializers)
	{
		foreach (BoundExpression item in initializers)
		{
			AddObjectInitializer(ref dynamicSiteInitializers, ref temps, result, rewrittenReceiver, (BoundAssignmentOperator)item);
		}
	}

	private void AddObjectInitializer(ref ArrayBuilder<BoundExpression>? dynamicSiteInitializers, ref ArrayBuilder<LocalSymbol>? temps, ArrayBuilder<BoundExpression> result, BoundExpression rewrittenReceiver, BoundAssignmentOperator assignment)
	{
		BoundExpression left = assignment.Left;
		BoundExpression right = assignment.Right;
		BoundKind kind = right.Kind;
		bool flag = ((kind == BoundKind.ObjectInitializerExpression || kind == BoundKind.CollectionInitializerExpression) ? true : false);
		bool flag2 = flag;
		if (flag2 && onlyContainsEmptyLeafNestedInitializers(assignment))
		{
			addIndexes(result, assignment);
			return;
		}
		BoundExpression boundExpression;
		switch (left.Kind)
		{
		case BoundKind.ObjectInitializerMember:
		{
			BoundObjectInitializerMember boundObjectInitializerMember = (BoundObjectInitializerMember)VisitObjectInitializerMember((BoundObjectInitializerMember)left, ref rewrittenReceiver, result, ref temps);
			if (!boundObjectInitializerMember.Arguments.IsDefaultOrEmpty)
			{
				ImmutableArray<BoundExpression> arguments = EvaluateSideEffectingArgumentsToTemps(boundObjectInitializerMember.Arguments, boundObjectInitializerMember.MemberSymbol?.GetParameterRefKinds() ?? default(ImmutableArray<RefKind>), result, ref temps);
				boundObjectInitializerMember = boundObjectInitializerMember.Update(boundObjectInitializerMember.MemberSymbol, arguments, boundObjectInitializerMember.ArgumentNamesOpt, boundObjectInitializerMember.ArgumentRefKindsOpt, boundObjectInitializerMember.Expanded, boundObjectInitializerMember.ArgsToParamsOpt, boundObjectInitializerMember.DefaultArguments, boundObjectInitializerMember.ResultKind, boundObjectInitializerMember.AccessorKind, boundObjectInitializerMember.ReceiverType, boundObjectInitializerMember.Type);
			}
			if (boundObjectInitializerMember.MemberSymbol == null && boundObjectInitializerMember.Type.IsDynamic())
			{
				if (dynamicSiteInitializers == null)
				{
					dynamicSiteInitializers = ArrayBuilder<BoundExpression>.GetInstance();
				}
				if (!flag2)
				{
					BoundExpression loweredRight2 = VisitExpression(right);
					LoweredDynamicOperation loweredDynamicOperation3 = _dynamicFactory.MakeDynamicSetIndex(rewrittenReceiver, boundObjectInitializerMember.Arguments, boundObjectInitializerMember.ArgumentNamesOpt, boundObjectInitializerMember.ArgumentRefKindsOpt, loweredRight2);
					dynamicSiteInitializers.Add(loweredDynamicOperation3.SiteInitialization);
					result.Add(loweredDynamicOperation3.SiteInvocation);
					return;
				}
				LoweredDynamicOperation loweredDynamicOperation4 = _dynamicFactory.MakeDynamicGetIndex(rewrittenReceiver, boundObjectInitializerMember.Arguments, boundObjectInitializerMember.ArgumentNamesOpt, boundObjectInitializerMember.ArgumentRefKindsOpt);
				dynamicSiteInitializers.Add(loweredDynamicOperation4.SiteInitialization);
				boundExpression = loweredDynamicOperation4.SiteInvocation;
			}
			else
			{
				boundExpression = MakeObjectInitializerMemberAccess(rewrittenReceiver, boundObjectInitializerMember, flag2);
				if (!flag2)
				{
					BoundExpression rewrittenRight4 = VisitExpression(right);
					result.Add(MakeStaticAssignmentOperator(assignment.Syntax, boundExpression, rewrittenRight4, assignment.IsRef, used: false, AssignmentKind.SimpleAssignment));
					return;
				}
			}
			break;
		}
		case BoundKind.DynamicObjectInitializerMember:
		{
			BoundDynamicObjectInitializerMember boundDynamicObjectInitializerMember = (BoundDynamicObjectInitializerMember)VisitDynamicObjectInitializerMember((BoundDynamicObjectInitializerMember)left);
			if (dynamicSiteInitializers == null)
			{
				dynamicSiteInitializers = ArrayBuilder<BoundExpression>.GetInstance();
			}
			if (!flag2)
			{
				BoundExpression loweredRight = VisitExpression(right);
				LoweredDynamicOperation loweredDynamicOperation = _dynamicFactory.MakeDynamicSetMember(rewrittenReceiver, boundDynamicObjectInitializerMember.MemberName, loweredRight);
				dynamicSiteInitializers.Add(loweredDynamicOperation.SiteInitialization);
				result.Add(loweredDynamicOperation.SiteInvocation);
				return;
			}
			LoweredDynamicOperation loweredDynamicOperation2 = _dynamicFactory.MakeDynamicGetMember(rewrittenReceiver, boundDynamicObjectInitializerMember.MemberName, resultIndexed: false);
			dynamicSiteInitializers.Add(loweredDynamicOperation2.SiteInitialization);
			boundExpression = loweredDynamicOperation2.SiteInvocation;
			break;
		}
		case BoundKind.ArrayAccess:
		{
			BoundNode boundNode = VisitArrayAccess((BoundArrayAccess)left);
			if (boundNode is BoundArrayAccess boundArrayAccess)
			{
				ImmutableArray<BoundExpression> indices = EvaluateSideEffectingArgumentsToTemps(boundArrayAccess.Indices, default(ImmutableArray<RefKind>), result, ref temps);
				boundExpression = boundArrayAccess.Update(rewrittenReceiver, indices, boundArrayAccess.Type);
			}
			else
			{
				if (!(boundNode is BoundCall boundCall))
				{
					throw ExceptionUtilities.UnexpectedValue(boundNode.Kind);
				}
				BoundExpression argument = boundCall.Arguments[1];
				BoundLocal boundLocal = _factory.StoreToTemp(argument, out BoundAssignmentOperator store);
				if (temps == null)
				{
					temps = ArrayBuilder<LocalSymbol>.GetInstance();
				}
				temps.Add(boundLocal.LocalSymbol);
				result.Add(store);
				boundExpression = boundCall.Update(ImmutableArray.Create(boundCall.Arguments[0], boundLocal));
			}
			if (!flag2)
			{
				BoundExpression rewrittenRight2 = VisitExpression(right);
				result.Add(MakeStaticAssignmentOperator(assignment.Syntax, boundExpression, rewrittenRight2, isRef: false, used: false, AssignmentKind.SimpleAssignment));
				return;
			}
			break;
		}
		case BoundKind.PointerElementAccess:
		{
			BoundPointerElementAccess boundPointerElementAccess = (BoundPointerElementAccess)left;
			BoundExpression boundExpression2 = VisitExpression(boundPointerElementAccess.Index);
			if (CanChangeValueBetweenReads(boundExpression2))
			{
				BoundLocal boundLocal2 = _factory.StoreToTemp(boundExpression2, out BoundAssignmentOperator store2);
				boundExpression2 = boundLocal2;
				if (temps == null)
				{
					temps = ArrayBuilder<LocalSymbol>.GetInstance();
				}
				temps.Add(boundLocal2.LocalSymbol);
				result.Add(store2);
			}
			boundExpression = RewritePointerElementAccess(boundPointerElementAccess, rewrittenReceiver, boundExpression2);
			if (!flag2)
			{
				BoundExpression rewrittenRight3 = VisitExpression(right);
				result.Add(MakeStaticAssignmentOperator(assignment.Syntax, boundExpression, rewrittenRight3, isRef: false, used: false, AssignmentKind.SimpleAssignment));
				return;
			}
			break;
		}
		case BoundKind.ImplicitIndexerAccess:
		{
			BoundImplicitIndexerAccess boundImplicitIndexerAccess = (BoundImplicitIndexerAccess)left;
			if (temps == null)
			{
				temps = ArrayBuilder<LocalSymbol>.GetInstance();
			}
			if (TypeSymbol.Equals(boundImplicitIndexerAccess.Argument.Type, _compilation.GetWellKnownType(WellKnownType.System_Index), TypeCompareKind.ConsiderEverything))
			{
				boundExpression = GetUnderlyingIndexerOrSliceAccess(boundImplicitIndexerAccess, !flag2, isRegularAssignment: true, cacheAllArgumentsOnly: true, result, temps);
				if (boundExpression is BoundIndexerAccess boundIndexerAccess)
				{
					boundExpression = TransformIndexerAccessContinued(boundIndexerAccess, boundIndexerAccess.ReceiverOpt, boundIndexerAccess.Arguments, result, temps);
				}
			}
			else
			{
				boundExpression = VisitRangePatternIndexerAccess(boundImplicitIndexerAccess, temps, result, cacheAllArgumentsOnly: true);
			}
			if (!flag2)
			{
				BoundExpression rewrittenRight = VisitExpression(right);
				result.Add(MakeStaticAssignmentOperator(assignment.Syntax, boundExpression, rewrittenRight, isRef: false, used: false, AssignmentKind.SimpleAssignment));
				return;
			}
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(left.Kind);
		}
		AddObjectOrCollectionInitializers(ref dynamicSiteInitializers, ref temps, result, boundExpression, right);
		void addIndexes(ArrayBuilder<BoundExpression> arrayBuilder, BoundAssignmentOperator boundAssignmentOperator)
		{
			BoundExpression left2 = boundAssignmentOperator.Left;
			if (left2 is BoundObjectInitializerMember boundObjectInitializerMember2)
			{
				foreach (BoundExpression argument2 in boundObjectInitializerMember2.Arguments)
				{
					if (argument2 is BoundArrayCreation { IsParamsArrayOrCollection: not false } boundArrayCreation)
					{
						BoundArrayInitialization initializerOpt = boundArrayCreation.InitializerOpt;
						foreach (BoundExpression initializer in initializerOpt.Initializers)
						{
							arrayBuilder.Add(VisitExpression(initializer));
						}
					}
					else
					{
						arrayBuilder.Add(VisitExpression(argument2));
					}
				}
			}
			else if (left2 is BoundImplicitIndexerAccess boundImplicitIndexerAccess2)
			{
				arrayBuilder.Add(VisitExpression(boundImplicitIndexerAccess2.Argument));
			}
			else if (left2 is BoundArrayAccess boundArrayAccess2)
			{
				foreach (BoundExpression index in boundArrayAccess2.Indices)
				{
					arrayBuilder.Add(VisitExpression(index));
				}
			}
			else
			{
				if (!(left2 is BoundPointerElementAccess boundPointerElementAccess2))
				{
					throw ExceptionUtilities.UnexpectedValue(left2.Kind);
				}
				arrayBuilder.Add(VisitExpression(boundPointerElementAccess2.Index));
			}
			foreach (BoundExpression initializer2 in ((BoundObjectInitializerExpression)boundAssignmentOperator.Right).Initializers)
			{
				addIndexes(arrayBuilder, (BoundAssignmentOperator)initializer2);
			}
		}
		static bool onlyContainsEmptyLeafNestedInitializers(BoundAssignmentOperator boundAssignmentOperator)
		{
			BoundExpression left2 = boundAssignmentOperator.Left;
			if ((left2 is BoundObjectInitializerMember || left2 is BoundImplicitIndexerAccess || left2 is BoundArrayAccess || left2 is BoundPointerElementAccess) ? true : false)
			{
				if (boundAssignmentOperator.Right is BoundObjectInitializerExpression boundObjectInitializerExpression)
				{
					return boundObjectInitializerExpression.Initializers.All((BoundExpression e) => e is BoundAssignmentOperator assignment2 && onlyContainsEmptyLeafNestedInitializers(assignment2));
				}
				return false;
			}
			return false;
		}
	}

	private ImmutableArray<BoundExpression> EvaluateSideEffectingArgumentsToTemps(ImmutableArray<BoundExpression> args, ImmutableArray<RefKind> paramRefKindsOpt, ArrayBuilder<BoundExpression> sideeffects, ref ArrayBuilder<LocalSymbol>? temps)
	{
		ArrayBuilder<BoundExpression> arrayBuilder = null;
		for (int i = 0; i < args.Length; i++)
		{
			BoundExpression boundExpression = args[i];
			BoundExpression boundExpression2;
			if (boundExpression.IsParamsArrayOrCollection)
			{
				(LocalRewriter, ArrayBuilder<BoundExpression>, ArrayBuilder<LocalSymbol>) arg = (this, sideeffects, temps);
				boundExpression2 = RewriteParamsArray<(LocalRewriter, ArrayBuilder<BoundExpression>, ArrayBuilder<LocalSymbol>)>(boundExpression, delegate(BoundExpression element, ref (LocalRewriter rewriter, ArrayBuilder<BoundExpression> sideeffects, ArrayBuilder<LocalSymbol>? temps) elementArg)
				{
					return elementArg.rewriter.EvaluateSideEffects(element, RefKind.None, elementArg.sideeffects, ref elementArg.temps);
				}, ref arg);
				temps = arg.Item3;
			}
			else
			{
				boundExpression2 = EvaluateSideEffects(boundExpression, paramRefKindsOpt.RefKinds(i), sideeffects, ref temps);
			}
			if (boundExpression2 != boundExpression)
			{
				if (arrayBuilder == null)
				{
					arrayBuilder = ArrayBuilder<BoundExpression>.GetInstance(args.Length);
					arrayBuilder.AddRange(args, i);
				}
				arrayBuilder.Add(boundExpression2);
			}
			else
			{
				arrayBuilder?.Add(boundExpression);
			}
		}
		return arrayBuilder?.ToImmutableAndFree() ?? args;
	}

	private BoundExpression EvaluateSideEffects(BoundExpression arg, RefKind refKind, ArrayBuilder<BoundExpression> sideeffects, ref ArrayBuilder<LocalSymbol>? temps)
	{
		if (CanChangeValueBetweenReads(arg))
		{
			BoundLocal boundLocal = _factory.StoreToTemp(arg, out BoundAssignmentOperator store, refKind);
			if (temps == null)
			{
				temps = ArrayBuilder<LocalSymbol>.GetInstance();
			}
			temps.Add(boundLocal.LocalSymbol);
			sideeffects.Add(store);
			return boundLocal;
		}
		return arg;
	}

	private BoundExpression MakeObjectInitializerMemberAccess(BoundExpression rewrittenReceiver, BoundObjectInitializerMember rewrittenLeft, bool isRhsNestedInitializer)
	{
		Symbol memberSymbol = rewrittenLeft.MemberSymbol;
		rewrittenReceiver = ConvertReceiverForExtensionMemberIfNeeded(memberSymbol, rewrittenReceiver, markAsChecked: true);
		switch (memberSymbol.Kind)
		{
		case SymbolKind.Field:
		{
			FieldSymbol fieldSymbol = (FieldSymbol)memberSymbol;
			return MakeFieldAccess(rewrittenLeft.Syntax, rewrittenReceiver, fieldSymbol, null, rewrittenLeft.ResultKind, fieldSymbol.Type);
		}
		case SymbolKind.Property:
		{
			PropertySymbol propertySymbol = (PropertySymbol)memberSymbol;
			if (!rewrittenLeft.Arguments.IsEmpty || propertySymbol.IsIndexedProperty)
			{
				return MakeIndexerAccess(rewrittenLeft.Syntax, rewrittenReceiver, propertySymbol, rewrittenLeft.Arguments, rewrittenLeft.ArgumentNamesOpt, rewrittenLeft.ArgumentRefKindsOpt, rewrittenLeft.Expanded, rewrittenLeft.ArgsToParamsOpt, rewrittenLeft.DefaultArguments, rewrittenLeft, !isRhsNestedInitializer);
			}
			return MakePropertyAccess(rewrittenLeft.Syntax, rewrittenReceiver, propertySymbol, rewrittenLeft.ResultKind, propertySymbol.Type, !isRhsNestedInitializer);
		}
		case SymbolKind.Event:
		{
			EventSymbol eventSymbol = (EventSymbol)memberSymbol;
			return MakeEventAccess(rewrittenLeft.Syntax, rewrittenReceiver, eventSymbol, null, rewrittenLeft.ResultKind, eventSymbol.Type);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(memberSymbol.Kind);
		}
	}

	public override BoundNode VisitSwitchStatement(BoundSwitchStatement node)
	{
		return SwitchStatementLocalRewriter.Rewrite(this, node);
	}

	public override BoundNode VisitPointerElementAccess(BoundPointerElementAccess node)
	{
		BoundExpression rewrittenExpression = LowerReceiverOfPointerElementAccess(node.Expression);
		BoundExpression rewrittenIndex = VisitExpression(node.Index);
		return RewritePointerElementAccess(node, rewrittenExpression, rewrittenIndex);
	}

	private BoundExpression LowerReceiverOfPointerElementAccess(BoundExpression receiver)
	{
		if (receiver is BoundFieldAccess boundFieldAccess && boundFieldAccess.FieldSymbol.IsFixedSizeBuffer)
		{
			BoundExpression receiver2 = VisitExpression(boundFieldAccess.ReceiverOpt);
			BoundFieldAccess boundFieldAccess2 = boundFieldAccess.Update(receiver2, boundFieldAccess.FieldSymbol, boundFieldAccess.ConstantValueOpt, boundFieldAccess.ResultKind, boundFieldAccess.Type);
			return new BoundAddressOfOperator(receiver.Syntax, boundFieldAccess2, isManaged: true, boundFieldAccess2.Type);
		}
		return VisitExpression(receiver);
	}

	private BoundExpression RewritePointerElementAccess(BoundPointerElementAccess node, BoundExpression rewrittenExpression, BoundExpression rewrittenIndex)
	{
		if (rewrittenIndex.IsDefaultValue())
		{
			return new BoundPointerIndirectionOperator(node.Syntax, rewrittenExpression, node.RefersToLocation, node.Type);
		}
		BinaryOperatorKind binaryOperatorKind = BinaryOperatorKind.Addition;
		binaryOperatorKind = rewrittenIndex.Type.SpecialType switch
		{
			SpecialType.System_Int32 => binaryOperatorKind | BinaryOperatorKind.PointerAndIntAddition, 
			SpecialType.System_UInt32 => binaryOperatorKind | BinaryOperatorKind.PointerAndUIntAddition, 
			SpecialType.System_Int64 => binaryOperatorKind | BinaryOperatorKind.PointerAndLongAddition, 
			SpecialType.System_UInt64 => binaryOperatorKind | BinaryOperatorKind.PointerAndULongAddition, 
			_ => throw ExceptionUtilities.UnexpectedValue(rewrittenIndex.Type.SpecialType), 
		};
		if (node.Checked)
		{
			binaryOperatorKind |= BinaryOperatorKind.Checked;
		}
		return new BoundPointerIndirectionOperator(node.Syntax, MakeBinaryOperator(node.Syntax, binaryOperatorKind, rewrittenExpression, rewrittenIndex, rewrittenExpression.Type, null, null, isPointerElementAccess: true), node.RefersToLocation, node.Type);
	}

	public override BoundNode VisitPreviousSubmissionReference(BoundPreviousSubmissionReference node)
	{
		ImplicitNamedTypeSymbol previousSubmissionType = (ImplicitNamedTypeSymbol)node.Type;
		SyntaxNode syntax = node.Syntax;
		FieldSymbol orMakeField = _previousSubmissionFields.GetOrMakeField(previousSubmissionType);
		BoundThisReference receiver = new BoundThisReference(syntax, _factory.CurrentType);
		return new BoundFieldAccess(syntax, receiver, orMakeField, null);
	}

	public override BoundNode VisitPropertyAccess(BoundPropertyAccess node)
	{
		return VisitPropertyAccess(node, isLeftOfAssignment: false);
	}

	private BoundExpression VisitPropertyAccess(BoundPropertyAccess node, bool isLeftOfAssignment)
	{
		BoundExpression rewrittenReceiverOpt = VisitExpression(node.ReceiverOpt);
		return MakePropertyAccess(node.Syntax, rewrittenReceiverOpt, node.PropertySymbol, node.ResultKind, node.Type, isLeftOfAssignment, node);
	}

	private BoundExpression MakePropertyAccess(SyntaxNode syntax, BoundExpression? rewrittenReceiverOpt, PropertySymbol propertySymbol, LookupResultKind resultKind, TypeSymbol type, bool isLeftOfAssignment, BoundPropertyAccess? oldNodeOpt = null)
	{
		if (rewrittenReceiverOpt != null)
		{
			TypeSymbol type2 = rewrittenReceiverOpt.Type;
			if ((object)type2 != null && type2.TypeKind == TypeKind.Array && !isLeftOfAssignment && ((ArrayTypeSymbol)rewrittenReceiverOpt.Type).IsSZArray && ((object)propertySymbol == _compilation.GetSpecialTypeMember(SpecialMember.System_Array__Length) || (!_inExpressionLambda && (object)propertySymbol == _compilation.GetSpecialTypeMember(SpecialMember.System_Array__LongLength))))
			{
				return new BoundArrayLength(syntax, rewrittenReceiverOpt, type);
			}
		}
		if (isLeftOfAssignment && propertySymbol.RefKind == RefKind.None)
		{
			if (oldNodeOpt == null)
			{
				return new BoundPropertyAccess(syntax, rewrittenReceiverOpt, ThreeState.Unknown, propertySymbol, AccessorKind.Unknown, resultKind, type);
			}
			return oldNodeOpt.Update(rewrittenReceiverOpt, ThreeState.Unknown, propertySymbol, AccessorKind.Unknown, resultKind, type);
		}
		return MakePropertyGetAccess(syntax, rewrittenReceiverOpt, propertySymbol, oldNodeOpt);
	}

	private BoundExpression MakePropertyGetAccess(SyntaxNode syntax, BoundExpression? rewrittenReceiver, PropertySymbol property, BoundPropertyAccess? oldNodeOpt)
	{
		return MakePropertyGetAccess(syntax, rewrittenReceiver, property, ImmutableArray<BoundExpression>.Empty, default(ImmutableArray<RefKind>), null, oldNodeOpt);
	}

	private BoundExpression MakePropertyGetAccess(SyntaxNode syntax, BoundExpression? rewrittenReceiver, PropertySymbol property, ImmutableArray<BoundExpression> rewrittenArguments, ImmutableArray<RefKind> argumentRefKindsOpt, MethodSymbol? getMethodOpt = null, BoundPropertyAccess? oldNodeOpt = null)
	{
		if (_inExpressionLambda && rewrittenArguments.IsEmpty)
		{
			if (oldNodeOpt == null)
			{
				return new BoundPropertyAccess(syntax, rewrittenReceiver, ThreeState.Unknown, property, AccessorKind.Unknown, LookupResultKind.Viable, property.Type);
			}
			return oldNodeOpt.Update(rewrittenReceiver, ThreeState.Unknown, property, AccessorKind.Unknown, LookupResultKind.Viable, property.Type);
		}
		MethodSymbol method = getMethodOpt ?? property.GetOwnOrInheritedGetMethod();
		return BoundCall.Synthesized(syntax, rewrittenReceiver, ThreeState.Unknown, method, rewrittenArguments, argumentRefKindsOpt);
	}

	public override BoundNode VisitRangeVariable(BoundRangeVariable node)
	{
		return VisitExpression(node.Value);
	}

	public override BoundNode VisitQueryClause(BoundQueryClause node)
	{
		return VisitExpression(node.Value);
	}

	public override BoundNode VisitRangeExpression(BoundRangeExpression node)
	{
		bool needLifting = false;
		_ = _factory;
		BoundExpression boundExpression = node.LeftOperandOpt;
		if (boundExpression != null)
		{
			boundExpression = tryOptimizeOperand(boundExpression);
		}
		BoundExpression boundExpression2 = node.RightOperandOpt;
		if (boundExpression2 != null)
		{
			boundExpression2 = tryOptimizeOperand(boundExpression2);
		}
		if (needLifting)
		{
			return LiftRangeExpression(node, boundExpression, boundExpression2);
		}
		BoundExpression boundExpression3 = MakeRangeExpression(node.MethodOpt, boundExpression, boundExpression2);
		if (node.Type.IsNullableType())
		{
			return ConvertToNullable(node.Syntax, node.Type, boundExpression3);
		}
		return boundExpression3;
		BoundExpression tryOptimizeOperand(BoundExpression operand)
		{
			operand = VisitExpression(operand);
			if (NullableNeverHasValue(operand))
			{
				operand = new BoundDefaultExpression(operand.Syntax, operand.Type.GetNullableUnderlyingType());
			}
			else
			{
				operand = NullableAlwaysHasValue(operand) ?? operand;
				if (operand.Type.IsNullableType())
				{
					needLifting = true;
				}
			}
			return operand;
		}
	}

	private BoundExpression LiftRangeExpression(BoundRangeExpression node, BoundExpression? left, BoundExpression? right)
	{
		ArrayBuilder<BoundExpression> sideeffects = ArrayBuilder<BoundExpression>.GetInstance();
		ArrayBuilder<LocalSymbol> locals = ArrayBuilder<LocalSymbol>.GetInstance();
		BoundExpression condition = null;
		left = getIndexFromPossibleNullable(left);
		right = getIndexFromPossibleNullable(right);
		BoundExpression boundExpression = MakeRangeExpression(node.MethodOpt, left, right);
		if (!TryGetNullableMethod(node.Syntax, node.Type, SpecialMember.System_Nullable_T__ctor, out MethodSymbol result))
		{
			return BadExpression(node.Syntax, node.Type, node);
		}
		BoundExpression rewrittenConsequence = new BoundObjectCreationExpression(node.Syntax, result, boundExpression);
		BoundExpression rewrittenAlternative = new BoundDefaultExpression(node.Syntax, node.Type);
		BoundExpression value = RewriteConditionalOperator(node.Syntax, condition, rewrittenConsequence, rewrittenAlternative, null, node.Type, isRef: false);
		return new BoundSequence(node.Syntax, locals.ToImmutableAndFree(), sideeffects.ToImmutableAndFree(), value, node.Type);
		BoundExpression? getIndexFromPossibleNullable(BoundExpression? arg)
		{
			if (arg == null)
			{
				return null;
			}
			BoundExpression boundExpression2 = CaptureExpressionInTempIfNeeded(arg, sideeffects, locals);
			if (boundExpression2.Type.IsNullableType())
			{
				BoundExpression boundExpression3 = MakeOptimizedHasValue(boundExpression2.Syntax, boundExpression2);
				if (condition == null)
				{
					condition = boundExpression3;
				}
				else
				{
					TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
					condition = MakeBinaryOperator(node.Syntax, BinaryOperatorKind.BoolAnd, condition, boundExpression3, specialType, null, null);
				}
				return MakeOptimizedGetValueOrDefault(boundExpression2.Syntax, boundExpression2);
			}
			return boundExpression2;
		}
	}

	private BoundExpression MakeRangeExpression(MethodSymbol constructionMethod, BoundExpression? left, BoundExpression? right)
	{
		SyntheticBoundNodeFactory F = _factory;
		switch (constructionMethod.MethodKind)
		{
		case MethodKind.Constructor:
			left = left ?? newIndexZero(fromEnd: false);
			right = right ?? newIndexZero(fromEnd: true);
			return F.New(constructionMethod, ImmutableArray.Create<BoundExpression>(left, right));
		case MethodKind.Ordinary:
		{
			BoundExpression item = left ?? right;
			return F.StaticCall(constructionMethod, ImmutableArray.Create(item));
		}
		case MethodKind.PropertyGet:
			return F.StaticCall(constructionMethod, ImmutableArray<BoundExpression>.Empty);
		default:
			throw ExceptionUtilities.UnexpectedValue(constructionMethod.MethodKind);
		}
		BoundExpression newIndexZero(bool fromEnd)
		{
			return F.New(WellKnownMember.System_Index__ctor, ImmutableArray.Create((BoundExpression)F.Literal(0), (BoundExpression)F.Literal(fromEnd)));
		}
	}

	public override BoundNode VisitReturnStatement(BoundReturnStatement node)
	{
		BoundStatement boundStatement = (BoundStatement)base.VisitReturnStatement(node);
		bool num;
		if (Instrument)
		{
			if (!node.WasCompilerGenerated)
			{
				goto IL_005e;
			}
			if (node.ExpressionOpt != null)
			{
				num = IsLambdaOrExpressionBodiedMember;
				goto IL_005c;
			}
			if (node.Syntax.Kind() == SyntaxKind.Block)
			{
				MethodSymbol? currentFunction = _factory.CurrentFunction;
				if ((object)currentFunction != null)
				{
					num = !currentFunction.IsAsync;
					goto IL_005c;
				}
			}
		}
		goto IL_006c;
		IL_006c:
		return boundStatement;
		IL_005e:
		boundStatement = Instrumenter.InstrumentReturnStatement(node, boundStatement);
		goto IL_006c;
		IL_005c:
		if (num)
		{
			goto IL_005e;
		}
		goto IL_006c;
	}

	public override BoundNode VisitConvertedStackAllocExpression(BoundConvertedStackAllocExpression stackAllocNode)
	{
		return VisitStackAllocArrayCreationBase(stackAllocNode);
	}

	public override BoundNode VisitStackAllocArrayCreation(BoundStackAllocArrayCreation stackAllocNode)
	{
		return VisitStackAllocArrayCreationBase(stackAllocNode);
	}

	private BoundNode VisitStackAllocArrayCreationBase(BoundStackAllocArrayCreationBase stackAllocNode)
	{
		BoundExpression boundExpression = VisitExpression(stackAllocNode.Count);
		TypeSymbol type = stackAllocNode.Type;
		ConstantValue? constantValueOpt = boundExpression.ConstantValueOpt;
		if ((object)constantValueOpt != null && constantValueOpt.Int32Value == 0)
		{
			return _factory.Default(type);
		}
		TypeSymbol elementType = stackAllocNode.ElementType;
		BoundArrayInitialization boundArrayInitialization = stackAllocNode.InitializerOpt;
		if (boundArrayInitialization != null)
		{
			boundArrayInitialization = boundArrayInitialization.Update(VisitList(boundArrayInitialization.Initializers));
		}
		if (type.IsPointerType())
		{
			BoundExpression count = RewriteStackAllocCountToSize(boundExpression, elementType);
			return new BoundConvertedStackAllocExpression(stackAllocNode.Syntax, elementType, count, boundArrayInitialization, type);
		}
		if (TypeSymbol.Equals(type.OriginalDefinition, _compilation.GetWellKnownType(WellKnownType.System_Span_T), TypeCompareKind.ConsiderEverything))
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)type;
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
			ArrayBuilder<LocalSymbol> instance2 = ArrayBuilder<LocalSymbol>.GetInstance();
			BoundExpression boundExpression2 = CaptureExpressionInTempIfNeeded(boundExpression, instance, instance2, SynthesizedLocalKind.Spill);
			BoundExpression count2 = RewriteStackAllocCountToSize(boundExpression2, elementType);
			stackAllocNode = new BoundConvertedStackAllocExpression(stackAllocNode.Syntax, elementType, count2, boundArrayInitialization, _compilation.CreatePointerTypeSymbol(elementType));
			BoundExpression argument = ((!TryGetWellKnownTypeMember<MethodSymbol>(stackAllocNode.Syntax, WellKnownMember.System_Span_T__ctor_Pointer, out MethodSymbol symbol)) ? ((BoundExpression)new BoundBadExpression(stackAllocNode.Syntax, LookupResultKind.NotInvocable, ImmutableArray<Symbol>.Empty, ImmutableArray<BoundExpression>.Empty, ErrorTypeSymbol.UnknownResultType)) : ((BoundExpression)_factory.New((MethodSymbol)symbol.SymbolAsMember(namedTypeSymbol), stackAllocNode, boundExpression2)));
			_needsSpilling = true;
			BoundLocal boundLocal = _factory.StoreToTemp(argument, out BoundAssignmentOperator store, RefKind.None, SynthesizedLocalKind.LoweringTemp, isKnownToReferToTempIfReferenceType: false, stackAllocNode.Syntax);
			instance.Add(store);
			instance2.Add(boundLocal.LocalSymbol);
			return new BoundSpillSequence(stackAllocNode.Syntax, instance2.ToImmutableAndFree(), instance.ToImmutableAndFree(), boundLocal, namedTypeSymbol);
		}
		throw ExceptionUtilities.UnexpectedValue(type);
	}

	private BoundExpression RewriteStackAllocCountToSize(BoundExpression countExpression, TypeSymbol elementType)
	{
		TypeSymbol type = _factory.SpecialType(SpecialType.System_UInt32);
		TypeSymbol type2 = _factory.SpecialType(SpecialType.System_UIntPtr);
		BoundExpression boundExpression = _factory.Sizeof(elementType);
		ConstantValue constantValueOpt = boundExpression.ConstantValueOpt;
		if (constantValueOpt != null)
		{
			int int32Value = constantValueOpt.Int32Value;
			ConstantValue constantValueOpt2 = countExpression.ConstantValueOpt;
			if (constantValueOpt2 != null)
			{
				long num = (uint)constantValueOpt2.Int32Value * int32Value;
				if (num < uint.MaxValue)
				{
					return _factory.Convert(type2, _factory.Literal((uint)num), Conversion.IntegerToPointer);
				}
			}
		}
		BoundExpression arg = _factory.Convert(type, countExpression, Conversion.ExplicitNumeric);
		arg = _factory.Convert(type2, arg, Conversion.IntegerToPointer);
		if ((object)constantValueOpt != null && constantValueOpt.Int32Value == 1)
		{
			return arg;
		}
		BinaryOperatorKind kind = BinaryOperatorKind.UIntMultiplication | BinaryOperatorKind.Checked;
		return _factory.Binary(kind, type2, arg, boundExpression);
	}

	private static bool IsBinaryStringConcatenation([NotNullWhen(true)] BoundBinaryOperator? binaryOperator)
	{
		if (binaryOperator != null)
		{
			BinaryOperatorKind operatorKind = binaryOperator.OperatorKind;
			return IsBinaryStringConcatenation(operatorKind);
		}
		return false;
	}

	private static bool IsBinaryStringConcatenation(BinaryOperatorKind binaryOperator)
	{
		if ((uint)(binaryOperator - 4369) <= 2u)
		{
			return true;
		}
		return false;
	}

	private BoundExpression VisitCompoundAssignmentStringConcatenation(BoundExpression left, BoundExpression unvisitedRight, BinaryOperatorKind operatorKind, SyntaxNode syntax)
	{
		ArrayBuilder<BoundExpression> destinationArguments;
		if (unvisitedRight is BoundBinaryOperator { InterpolatedStringHandlerData: null } boundBinaryOperator && IsBinaryStringConcatenation(boundBinaryOperator))
		{
			CollectAndVisitConcatArguments(boundBinaryOperator, left, out destinationArguments);
		}
		else
		{
			destinationArguments = ArrayBuilder<BoundExpression>.GetInstance();
			WellKnownConcatRelatedMethods wellKnownConcatOptimizationMethods = new WellKnownConcatRelatedMethods(_compilation);
			VisitAndAddConcatArgumentInReverseOrder(unvisitedRight, argumentAlreadyVisited: false, destinationArguments, ref wellKnownConcatOptimizationMethods);
			VisitAndAddConcatArgumentInReverseOrder(left, argumentAlreadyVisited: true, destinationArguments, ref wellKnownConcatOptimizationMethods);
			destinationArguments.ReverseContents();
		}
		return CreateStringConcat(syntax, destinationArguments);
	}

	private BoundExpression VisitStringConcatenation(BoundBinaryOperator originalOperator)
	{
		if (_inExpressionLambda)
		{
			return RewriteStringConcatInExpressionLambda(originalOperator);
		}
		CollectAndVisitConcatArguments(originalOperator, null, out ArrayBuilder<BoundExpression> destinationArguments);
		return CreateStringConcat(originalOperator.Syntax, destinationArguments);
	}

	private BoundExpression CreateStringConcat(SyntaxNode originalSyntax, ArrayBuilder<BoundExpression> visitedArguments)
	{
		if (visitedArguments != null)
		{
			switch (visitedArguments.Count)
			{
			case 1:
			{
				BoundExpression boundExpression = visitedArguments[0];
				if (boundExpression == null)
				{
					break;
				}
				ConstantValue constantValueOpt = boundExpression.ConstantValueOpt;
				if ((object)constantValueOpt != null)
				{
					if (constantValueOpt.IsString)
					{
						visitedArguments.Free();
						return boundExpression;
					}
					if (constantValueOpt.IsChar)
					{
						char charValue = constantValueOpt.CharValue;
						visitedArguments.Free();
						return _factory.StringLiteral(charValue.ToString());
					}
				}
				break;
			}
			case 0:
				visitedArguments.Free();
				return _factory.StringLiteral(string.Empty);
			}
		}
		StringConcatenationRewriteKind stringConcatenationRewriteKind = StringConcatenationRewriteKind.AllStrings;
		foreach (BoundExpression visitedArgument in visitedArguments)
		{
			switch (visitedArgument.Type.SpecialType)
			{
			case SpecialType.System_Char:
				if (stringConcatenationRewriteKind == StringConcatenationRewriteKind.AllStrings)
				{
					ConstantValue constantValueOpt = visitedArgument.ConstantValueOpt;
					if ((object)constantValueOpt == null || !constantValueOpt.IsChar)
					{
						stringConcatenationRewriteKind = StringConcatenationRewriteKind.AllStringsOrChars;
					}
				}
				continue;
			case SpecialType.System_String:
				continue;
			}
			stringConcatenationRewriteKind = StringConcatenationRewriteKind.InvolvesObjects;
			break;
		}
		int count = visitedArguments.Count;
		switch (count)
		{
		default:
			switch (stringConcatenationRewriteKind)
			{
			case StringConcatenationRewriteKind.AllStringsOrChars:
				if (count <= 4)
				{
					SpecialMember specialMember = visitedArguments.Count switch
					{
						2 => SpecialMember.System_String__Concat_2ReadOnlySpans, 
						3 => SpecialMember.System_String__Concat_3ReadOnlySpans, 
						4 => SpecialMember.System_String__Concat_4ReadOnlySpans, 
						_ => throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_StringConcat.cs", 150), 
					};
					bool needsImplicitConversionFromStringToSpan = visitedArguments.Any(delegate(BoundExpression arg)
					{
						TypeSymbol type = arg.Type;
						return (object)type != null && type.SpecialType == SpecialType.System_String;
					});
					NamedTypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Char);
					if (TryGetSpecialTypeMethod(originalSyntax, specialMember, out MethodSymbol method, isOptional: true) && TryGetNeededToSpanMembers(this, originalSyntax, needsImplicitConversionFromStringToSpan, specialType, out MethodSymbol readOnlySpanCtorRefParamChar, out MethodSymbol stringImplicitConversionToReadOnlySpan))
					{
						return RewriteStringConcatenationWithSpanBasedConcat(originalSyntax, _factory, method, stringImplicitConversionToReadOnlySpan, readOnlySpanCtorRefParamChar, visitedArguments);
					}
				}
				goto case StringConcatenationRewriteKind.AllStrings;
			case StringConcatenationRewriteKind.AllStrings:
			case StringConcatenationRewriteKind.InvolvesObjects:
			{
				int count2 = visitedArguments.Count;
				SpecialMember specialMember2 = ((count2 >= 5) ? SpecialMember.System_String__ConcatStringArray : (count2 switch
				{
					2 => SpecialMember.System_String__ConcatStringString, 
					3 => SpecialMember.System_String__ConcatStringStringString, 
					4 => SpecialMember.System_String__ConcatStringStringStringString, 
					_ => throw ExceptionUtilities.UnexpectedValue(visitedArguments.Count), 
				}));
				SpecialMember specialMember = specialMember2;
				for (int num = 0; num < visitedArguments.Count; num++)
				{
					visitedArguments[num] = ConvertConcatExprToString(visitedArguments[num]);
				}
				ImmutableArray<BoundExpression> immutableArray = visitedArguments.ToImmutableAndFree();
				if (immutableArray.Length > 4)
				{
					BoundExpression boundExpression2 = _factory.ArrayOrEmpty(_factory.SpecialType(SpecialType.System_String), immutableArray);
					immutableArray = ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1] { boundExpression2 });
				}
				MethodSymbol method2 = UnsafeGetSpecialTypeMethod(originalSyntax, specialMember);
				return BoundCall.Synthesized(originalSyntax, null, ThreeState.Unknown, method2, immutableArray);
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(stringConcatenationRewriteKind);
			}
		case 0:
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_StringConcat.cs", 135);
		case 1:
		{
			BoundExpression left = ConvertConcatExprToString(visitedArguments[0]);
			visitedArguments.Free();
			return _factory.Coalesce(left, _factory.StringLiteral(string.Empty));
		}
		}
	}

	private void CollectAndVisitConcatArguments(BoundBinaryOperator originalOperator, BoundExpression? visitedCompoundAssignmentLeftRead, out ArrayBuilder<BoundExpression> destinationArguments)
	{
		destinationArguments = ArrayBuilder<BoundExpression>.GetInstance();
		WellKnownConcatRelatedMethods concatMethods = new WellKnownConcatRelatedMethods(_compilation);
		pushArguments(this, originalOperator, destinationArguments, ref concatMethods);
		if (visitedCompoundAssignmentLeftRead != null)
		{
			destinationArguments.Add(visitedCompoundAssignmentLeftRead);
		}
		destinationArguments.ReverseContents();
		static void pushArguments(LocalRewriter self, BoundBinaryOperator binaryOperator, ArrayBuilder<BoundExpression> arguments, ref WellKnownConcatRelatedMethods reference)
		{
			while (true)
			{
				if (shouldRecurse(binaryOperator.Right, out var binaryOperator2))
				{
					pushArguments(self, binaryOperator2, arguments, ref reference);
				}
				else
				{
					self.VisitAndAddConcatArgumentInReverseOrder(binaryOperator.Right, argumentAlreadyVisited: false, arguments, ref reference);
				}
				if (!shouldRecurse(binaryOperator.Left, out var binaryOperator3))
				{
					break;
				}
				binaryOperator = binaryOperator3;
			}
			self.VisitAndAddConcatArgumentInReverseOrder(binaryOperator.Left, argumentAlreadyVisited: false, arguments, ref reference);
		}
		static bool shouldRecurse(BoundExpression expr, [NotNullWhen(true)] out BoundBinaryOperator? binaryOperator)
		{
			binaryOperator = expr as BoundBinaryOperator;
			if (IsBinaryStringConcatenation(binaryOperator) && !binaryOperator.InterpolatedStringHandlerData.HasValue)
			{
				return true;
			}
			binaryOperator = null;
			return false;
		}
	}

	private (BoundExpression? singleConcatArgument, ImmutableArray<BoundExpression> nestedConcatArguments) SimplifyConcatArgument(BoundExpression argument, [NotNullIfNotNull("followingArgument")] ref BoundExpression? followingArgument, ref WellKnownConcatRelatedMethods wellKnownConcatOptimizationMethods)
	{
		if (argument is BoundConversion { ConversionKind: ConversionKind.Boxing } boundConversion)
		{
			TypeSymbol type = boundConversion.Type;
			if ((object)type != null && type.SpecialType == SpecialType.System_Object)
			{
				BoundExpression operand = boundConversion.Operand;
				if (operand != null)
				{
					TypeSymbol type2 = operand.Type;
					if ((object)type2 != null && type2.SpecialType == SpecialType.System_Char)
					{
						argument = operand;
						goto IL_00ef;
					}
				}
			}
		}
		ConstantValue constantValueOpt;
		if (argument is BoundCall call)
		{
			if (wellKnownConcatOptimizationMethods.IsWellKnownConcatMethod(call, out ImmutableArray<BoundExpression> arguments))
			{
				return (singleConcatArgument: null, nestedConcatArguments: arguments);
			}
			if (wellKnownConcatOptimizationMethods.IsCharToString(call, out BoundExpression charExpression))
			{
				argument = charExpression;
			}
		}
		else if (argument is BoundNullCoalescingOperator boundNullCoalescingOperator)
		{
			BoundExpression leftOperand = boundNullCoalescingOperator.LeftOperand;
			if (leftOperand != null)
			{
				TypeSymbol type2 = leftOperand.Type;
				if ((object)type2 != null && type2.SpecialType == SpecialType.System_String && boundNullCoalescingOperator.RightOperand is BoundLiteral boundLiteral)
				{
					constantValueOpt = boundLiteral.ConstantValueOpt;
					if ((object)constantValueOpt != null && constantValueOpt.IsString)
					{
						Rope ropeValue = constantValueOpt.RopeValue;
						if (ropeValue != null && ropeValue.IsEmpty)
						{
							argument = leftOperand;
						}
					}
				}
			}
		}
		goto IL_00ef;
		IL_00ef:
		constantValueOpt = argument.ConstantValueOpt;
		bool flag;
		if ((object)constantValueOpt != null)
		{
			if (constantValueOpt.IsNull)
			{
				goto IL_0136;
			}
			if (constantValueOpt.IsString)
			{
				Rope ropeValue = constantValueOpt.RopeValue;
				if (ropeValue != null && ropeValue.IsEmpty)
				{
					goto IL_0136;
				}
			}
			else if (!constantValueOpt.IsChar)
			{
				goto IL_01c1;
			}
			BoundExpression boundExpression = followingArgument;
			if (boundExpression != null)
			{
				ConstantValue constantValueOpt2 = boundExpression.ConstantValueOpt;
				if ((object)constantValueOpt2 != null && (constantValueOpt2.IsString || constantValueOpt2.IsChar))
				{
					flag = true;
					goto IL_0176;
				}
			}
			flag = false;
			goto IL_0176;
		}
		goto IL_01c1;
		IL_01c1:
		return (singleConcatArgument: argument, nestedConcatArguments: default(ImmutableArray<BoundExpression>));
		IL_0176:
		if (flag)
		{
			Rope r = getRope(followingArgument.ConstantValueOpt);
			Rope r2 = getRope(argument.ConstantValueOpt);
			followingArgument = _factory.StringLiteral(ConstantValue.CreateFromRope(Rope.Concat(r2, r)));
			return (singleConcatArgument: null, nestedConcatArguments: default(ImmutableArray<BoundExpression>));
		}
		goto IL_01c1;
		IL_0136:
		return (singleConcatArgument: null, nestedConcatArguments: default(ImmutableArray<BoundExpression>));
		static Rope getRope(ConstantValue constantValue)
		{
			if (constantValue.IsString)
			{
				return constantValue.RopeValue;
			}
			return Rope.ForString(constantValue.CharValue.ToString());
		}
	}

	private void VisitAndAddConcatArgumentInReverseOrder(BoundExpression argument, bool argumentAlreadyVisited, ArrayBuilder<BoundExpression> finalArguments, ref WellKnownConcatRelatedMethods wellKnownConcatOptimizationMethods)
	{
		if (!argumentAlreadyVisited)
		{
			argument = VisitExpression(argument);
		}
		object obj;
		if (finalArguments.Count <= 0)
		{
			obj = null;
		}
		else
		{
			obj = finalArguments[finalArguments.Count - 1];
		}
		BoundExpression followingArgument = (BoundExpression)obj;
		var (boundExpression, immutableArray) = SimplifyConcatArgument(argument, ref followingArgument, ref wellKnownConcatOptimizationMethods);
		if (boundExpression == null && immutableArray.IsDefault)
		{
			if (followingArgument != null)
			{
				finalArguments[finalArguments.Count - 1] = followingArgument;
			}
			return;
		}
		if (boundExpression != null)
		{
			finalArguments.Add(boundExpression);
			return;
		}
		for (int num = immutableArray.Length - 1; num >= 0; num--)
		{
			VisitAndAddConcatArgumentInReverseOrder(immutableArray[num], argumentAlreadyVisited: true, finalArguments, ref wellKnownConcatOptimizationMethods);
		}
	}

	private static bool TryGetNeededToSpanMembers(LocalRewriter self, SyntaxNode syntax, bool needsImplicitConversionFromStringToSpan, NamedTypeSymbol charType, [NotNullWhen(true)] out MethodSymbol? readOnlySpanCtorRefParamChar, out MethodSymbol? stringImplicitConversionToReadOnlySpan)
	{
		readOnlySpanCtorRefParamChar = null;
		stringImplicitConversionToReadOnlySpan = null;
		if (self.TryGetSpecialTypeMethod(syntax, SpecialMember.System_ReadOnlySpan_T__ctor_Reference, out MethodSymbol method, isOptional: true) && method.Parameters[0].RefKind != RefKind.Out)
		{
			NamedTypeSymbol newOwner = method.ContainingType.Construct(charType);
			readOnlySpanCtorRefParamChar = method.AsMember(newOwner);
			if (needsImplicitConversionFromStringToSpan)
			{
				return self.TryGetSpecialTypeMethod(syntax, SpecialMember.System_String__op_Implicit_ToReadOnlySpanOfChar, out stringImplicitConversionToReadOnlySpan, isOptional: true);
			}
			return true;
		}
		return false;
	}

	private static BoundExpression RewriteStringConcatenationWithSpanBasedConcat(SyntaxNode syntax, SyntheticBoundNodeFactory factory, MethodSymbol spanConcat, MethodSymbol? stringImplicitConversionToReadOnlySpan, MethodSymbol readOnlySpanCtorRefParamChar, ArrayBuilder<BoundExpression> args)
	{
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		for (int i = 0; i < args.Count; i++)
		{
			BoundExpression boundExpression = args[i];
			if (boundExpression.Type.SpecialType == SpecialType.System_Char)
			{
				BoundLocal boundLocal = factory.StoreToTemp(boundExpression, out BoundAssignmentOperator store);
				instance.Add(boundLocal.LocalSymbol);
				BoundObjectCreationExpression boundObjectCreationExpression = new BoundObjectCreationExpression(boundExpression.Syntax, readOnlySpanCtorRefParamChar, ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1] { boundLocal }), default(ImmutableArray<string>), ImmutableCollectionsMarshal.AsImmutableArray(new RefKind[1] { (readOnlySpanCtorRefParamChar.Parameters[0].RefKind == RefKind.Ref) ? RefKind.Ref : ((RefKind)5) }), expanded: false, default(ImmutableArray<int>), default(BitVector), null, null, readOnlySpanCtorRefParamChar.ContainingType);
				args[i] = new BoundSequence(boundExpression.Syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1] { store }), boundObjectCreationExpression, boundObjectCreationExpression.Type);
			}
			else
			{
				args[i] = BoundCall.Synthesized(boundExpression.Syntax, null, ThreeState.Unknown, stringImplicitConversionToReadOnlySpan, boundExpression);
			}
		}
		BoundCall result = BoundCall.Synthesized(syntax, null, ThreeState.Unknown, spanConcat, args.ToImmutableAndFree());
		SyntaxNode syntax2 = factory.Syntax;
		factory.Syntax = syntax;
		BoundExpression result2 = factory.Sequence(instance.ToImmutableAndFree(), ImmutableArray<BoundExpression>.Empty, result);
		factory.Syntax = syntax2;
		return result2;
	}

	private BoundExpression RewriteStringConcatInExpressionLambda(BoundBinaryOperator original)
	{
		BoundBinaryOperator result = original;
		ArrayBuilder<BoundBinaryOperator> instance = ArrayBuilder<BoundBinaryOperator>.GetInstance();
		while (true)
		{
			instance.Push(result);
			if (!(result.Left is BoundBinaryOperator boundBinaryOperator) || !IsBinaryStringConcatenation(boundBinaryOperator))
			{
				break;
			}
			result = boundBinaryOperator;
		}
		BoundExpression boundExpression = VisitExpression(instance.Peek().Left);
		while (instance.TryPop(out result))
		{
			BoundExpression right = VisitExpression(result.Right);
			SpecialMember specialMember = ((result.OperatorKind == BinaryOperatorKind.StringConcatenation) ? SpecialMember.System_String__ConcatStringString : SpecialMember.System_String__ConcatObjectObject);
			MethodSymbol methodOpt = UnsafeGetSpecialTypeMethod(result.Syntax, specialMember);
			boundExpression = new BoundBinaryOperator(result.Syntax, result.OperatorKind, null, methodOpt, null, LookupResultKind.Empty, boundExpression, right, result.Type);
		}
		instance.Free();
		return boundExpression;
	}

	private BoundExpression ConvertConcatExprToString(BoundExpression expr)
	{
		SyntaxNode syntax = expr.Syntax;
		if (expr.Kind == BoundKind.Conversion)
		{
			BoundConversion boundConversion = (BoundConversion)expr;
			if (boundConversion.ConversionKind == ConversionKind.Boxing)
			{
				expr = boundConversion.Operand;
			}
		}
		if (expr != null)
		{
			ConstantValue constantValueOpt = expr.ConstantValueOpt;
			if ((object)constantValueOpt != null)
			{
				if (constantValueOpt.SpecialType == SpecialType.System_Char)
				{
					return _factory.StringLiteral(constantValueOpt.CharValue.ToString());
				}
				if (constantValueOpt.IsNull)
				{
					throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_StringConcat.cs", 648);
				}
			}
		}
		if (expr.Type.IsStringType())
		{
			return expr;
		}
		MethodSymbol objectToStringMethod = UnsafeGetSpecialTypeMethod(syntax, SpecialMember.System_Object__ToString);
		MethodSymbol methodSymbol = null;
		if (expr.Type.IsValueType && !expr.Type.IsTypeParameter())
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)expr.Type;
			foreach (Symbol member in namedTypeSymbol.GetMembers(objectToStringMethod.Name))
			{
				if (member is MethodSymbol methodSymbol2 && (object)methodSymbol2.GetLeastOverriddenMethod(namedTypeSymbol) == objectToStringMethod)
				{
					methodSymbol = methodSymbol2;
					break;
				}
			}
		}
		if (methodSymbol != null && expr.Type.SpecialType.CanOptimizeBehavior() && !isFieldOfMarshalByRef(expr, _compilation))
		{
			return BoundCall.Synthesized(syntax, expr, ThreeState.Unknown, methodSymbol);
		}
		bool flag = expr.Type.IsReferenceType || expr.ConstantValueOpt != null || (methodSymbol == null && !expr.Type.IsTypeParameter()) || (methodSymbol?.IsEffectivelyReadOnly ?? false);
		if (expr.Type.IsValueType)
		{
			if (!flag)
			{
				expr = new BoundPassByCopy(syntax, expr, expr.Type);
			}
			return BoundCall.Synthesized(syntax, expr, ThreeState.Unknown, objectToStringMethod);
		}
		if (flag)
		{
			return makeConditionalAccess(expr);
		}
		BoundLocal boundLocal = _factory.StoreToTemp(expr, out BoundAssignmentOperator store);
		return _factory.Sequence(ImmutableArray.Create(boundLocal.LocalSymbol), ImmutableArray.Create((BoundExpression)store), makeConditionalAccess(boundLocal));
		static bool isFieldOfMarshalByRef(BoundExpression boundExpression, CSharpCompilation compilation)
		{
			if (boundExpression is BoundFieldAccess fieldAccess)
			{
				return DiagnosticsPass.IsNonAgileFieldAccess(fieldAccess, compilation);
			}
			return false;
		}
		BoundExpression makeConditionalAccess(BoundExpression receiver)
		{
			int id = ++_currentConditionalAccessID;
			return new BoundLoweredConditionalAccess(syntax, receiver, null, BoundCall.Synthesized(syntax, new BoundConditionalReceiver(syntax, id, expr.Type), ThreeState.Unknown, objectToStringMethod), null, id, forceCopyOfNullableValueType: false, _compilation.GetSpecialType(SpecialType.System_String));
		}
	}

	private BoundExpression RewriteInterpolatedStringConversion(BoundConversion conversion)
	{
		BoundInterpolatedString boundInterpolatedString = (BoundInterpolatedString)conversion.Operand;
		return VisitExpression(boundInterpolatedString.InterpolationData.GetValueOrDefault().Construction);
	}

	private InterpolationHandlerResult RewriteToInterpolatedStringHandlerPattern(InterpolatedStringHandlerData data, ImmutableArray<BoundExpression> parts, SyntaxNode syntax)
	{
		LocalSymbol local = _factory.InterpolatedStringHandlerLocal(data.BuilderType, syntax);
		BoundLocal boundLocal = _factory.Local(local);
		BoundObjectCreationExpression node = (BoundObjectCreationExpression)data.Construction;
		BoundLocal boundLocal2 = null;
		if (data.HasTrailingHandlerValidityParameter)
		{
			ImmutableArray<BoundInterpolatedStringArgumentPlaceholder> argumentPlaceholders = data.ArgumentPlaceholders;
			BoundInterpolatedStringArgumentPlaceholder boundInterpolatedStringArgumentPlaceholder = argumentPlaceholders[argumentPlaceholders.Length - 1];
			TypeSymbol type = boundInterpolatedStringArgumentPlaceholder.Type;
			LocalSymbol local2 = _factory.SynthesizedLocal(type);
			boundLocal2 = _factory.Local(local2);
			AddPlaceholderReplacement(boundInterpolatedStringArgumentPlaceholder, boundLocal2);
		}
		BoundExpression boundExpression = _factory.AssignmentExpression(boundLocal, (BoundExpression)VisitObjectCreationExpression(node));
		AddPlaceholderReplacement(data.ReceiverPlaceholder, boundLocal);
		bool usesBoolReturns = data.UsesBoolReturns;
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(parts.Length + 1);
		foreach (BoundExpression item2 in parts)
		{
			if (item2 is BoundCall node2)
			{
				instance.Add((BoundExpression)VisitCall(node2));
				continue;
			}
			if (item2 is BoundDynamicInvocation node3)
			{
				instance.Add(VisitDynamicInvocation(node3, !usesBoolReturns));
				continue;
			}
			throw ExceptionUtilities.UnexpectedValue(item2.Kind);
		}
		RemovePlaceholderReplacement(data.ReceiverPlaceholder);
		if (boundLocal2 != null)
		{
			ImmutableArray<BoundInterpolatedStringArgumentPlaceholder> argumentPlaceholders = data.ArgumentPlaceholders;
			RemovePlaceholderReplacement(argumentPlaceholders[argumentPlaceholders.Length - 1]);
		}
		if (usesBoolReturns)
		{
			BoundExpression boundExpression2 = boundLocal2;
			NamedTypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
			foreach (BoundExpression item3 in instance)
			{
				BoundExpression boundExpression3 = item3;
				if (boundExpression3.Type.IsDynamic())
				{
					boundExpression3 = _dynamicFactory.MakeDynamicConversion(boundExpression3, isExplicit: false, isArrayIndex: false, isChecked: false, specialType).ToExpression();
				}
				boundExpression2 = ((boundExpression2 == null) ? boundExpression3 : _factory.LogicalAnd(boundExpression2, boundExpression3));
			}
			instance.Clear();
			instance.Add(boundExpression);
			instance.Add(boundExpression2);
		}
		else
		{
			if (boundLocal2 != null && instance.Count > 0)
			{
				ImmutableArray<BoundStatement> statements = instance.SelectAsArray((Func<BoundExpression, LocalRewriter, BoundStatement>)((BoundExpression appendCall, LocalRewriter @this) => @this._factory.ExpressionStatement(appendCall)), this);
				instance.Free();
				BoundStatement item = _factory.If(boundLocal2, _factory.StatementList(statements));
				return new InterpolationHandlerResult(ImmutableArray.Create(_factory.ExpressionStatement(boundExpression), item), boundLocal, boundLocal2.LocalSymbol, this);
			}
			instance.Insert(0, boundExpression);
		}
		return new InterpolationHandlerResult(instance.ToImmutableAndFree(), boundLocal, boundLocal2?.LocalSymbol, this);
	}

	public override BoundNode VisitInterpolatedString(BoundInterpolatedString node)
	{
		InterpolatedStringHandlerData? interpolationData = node.InterpolationData;
		if (interpolationData.HasValue)
		{
			InterpolatedStringHandlerData valueOrDefault = interpolationData.GetValueOrDefault();
			if ((object)valueOrDefault.BuilderType != null)
			{
				return LowerPartsToString(valueOrDefault, node.Parts, node.Syntax, node.Type);
			}
		}
		BoundExpression boundExpression;
		bool flag;
		bool flag2;
		if (!node.InterpolationData.HasValue)
		{
			int length = node.Parts.Length;
			if (length == 0)
			{
				return _factory.StringLiteral("");
			}
			boundExpression = null;
			for (int i = 0; i < length; i++)
			{
				BoundExpression boundExpression2 = node.Parts[i];
				boundExpression2 = ((!(boundExpression2 is BoundStringInsert boundStringInsert)) ? _factory.StringLiteral(boundExpression2.ConstantValueOpt.StringValue) : boundStringInsert.Value);
				boundExpression = ((boundExpression == null) ? boundExpression2 : _factory.Binary(BinaryOperatorKind.StringConcatenation, node.Type, boundExpression, boundExpression2));
			}
			flag = length == 1;
			if (flag)
			{
				if (boundExpression == null)
				{
					goto IL_011c;
				}
				if (boundExpression.Kind != BoundKind.InterpolatedString)
				{
					ConstantValue constantValueOpt = boundExpression.ConstantValueOpt;
					if ((object)constantValueOpt == null || !constantValueOpt.IsString)
					{
						goto IL_011c;
					}
				}
				flag2 = true;
				goto IL_011f;
			}
			goto IL_0126;
		}
		return VisitExpression(node.InterpolationData.GetValueOrDefault().Construction);
		IL_011c:
		flag2 = false;
		goto IL_011f;
		IL_011f:
		flag = !flag2;
		goto IL_0126;
		IL_0126:
		if (flag)
		{
			BoundValuePlaceholder boundValuePlaceholder = new BoundValuePlaceholder(boundExpression.Syntax, boundExpression.Type);
			boundExpression = new BoundNullCoalescingOperator(boundExpression.Syntax, boundExpression, _factory.StringLiteral(""), boundValuePlaceholder, boundValuePlaceholder, BoundNullCoalescingOperatorResultKind.LeftType, @checked: false, boundExpression.Type)
			{
				WasCompilerGenerated = true
			};
		}
		return VisitExpression(boundExpression);
	}

	private BoundExpression LowerPartsToString(InterpolatedStringHandlerData data, ImmutableArray<BoundExpression> parts, SyntaxNode syntax, TypeSymbol type)
	{
		InterpolationHandlerResult interpolationHandlerResult = RewriteToInterpolatedStringHandlerPattern(data, parts, syntax);
		MethodSymbol methodSymbol = (MethodSymbol)Binder.GetWellKnownTypeMember(_compilation, WellKnownMember.System_Runtime_CompilerServices_DefaultInterpolatedStringHandler__ToStringAndClear, _diagnostics, null, syntax);
		BoundExpression result = (((object)methodSymbol != null) ? ((BoundExpression)BoundCall.Synthesized(syntax, interpolationHandlerResult.HandlerTemp, ThreeState.Unknown, methodSymbol)) : ((BoundExpression)new BoundBadExpression(syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray<BoundExpression>.Empty, type)));
		return interpolationHandlerResult.WithFinalResult(result);
	}

	[Conditional("DEBUG")]
	private static void AssertNoImplicitInterpolatedStringHandlerConversions(ImmutableArray<BoundExpression> arguments, bool allowConversionsWithNoContext = false)
	{
		if (!allowConversionsWithNoContext)
		{
			return;
		}
		foreach (BoundExpression item in arguments)
		{
			if (item is BoundConversion { Conversion: { Kind: ConversionKind.InterpolatedStringHandler }, ExplicitCastInCode: false } boundConversion)
			{
				BoundExpression operand = boundConversion.Operand;
				operand.GetInterpolatedStringHandlerData();
			}
		}
	}

	public override BoundNode VisitConvertedSwitchExpression(BoundConvertedSwitchExpression node)
	{
		_needsSpilling = true;
		return SwitchExpressionLocalRewriter.Rewrite(this, node);
	}

	public override BoundNode VisitThrowStatement(BoundThrowStatement node)
	{
		BoundStatement boundStatement = (BoundStatement)base.VisitThrowStatement(node);
		if (Instrument && !node.WasCompilerGenerated)
		{
			boundStatement = Instrumenter.InstrumentThrowStatement(node, boundStatement);
		}
		return boundStatement;
	}

	public override BoundNode VisitTryStatement(BoundTryStatement node)
	{
		BoundBlock boundBlock = (BoundBlock)Visit(node.TryBlock);
		bool sawAwait = _sawAwait;
		_sawAwait = false;
		bool num = _compilation.Options.OptimizationLevel == OptimizationLevel.Release;
		ImmutableArray<BoundCatchBlock> catchBlocks = ((num && !HasSideEffects(boundBlock)) ? ImmutableArray<BoundCatchBlock>.Empty : VisitList(node.CatchBlocks));
		BoundBlock boundBlock2 = (BoundBlock)Visit(node.FinallyBlockOpt);
		_sawAwaitInExceptionHandler |= _sawAwait;
		_sawAwait |= sawAwait;
		if (num && !HasSideEffects(boundBlock2))
		{
			boundBlock2 = null;
		}
		if (!catchBlocks.IsDefaultOrEmpty || boundBlock2 != null)
		{
			return node.Update(boundBlock, catchBlocks, boundBlock2, node.FinallyLabelOpt, node.PreferFaultHandler);
		}
		return boundBlock;
	}

	private static bool HasSideEffects([NotNullWhen(true)] BoundStatement? statement)
	{
		if (statement == null)
		{
			return false;
		}
		switch (statement.Kind)
		{
		case BoundKind.NoOpStatement:
			return false;
		case BoundKind.Block:
			foreach (BoundStatement statement2 in ((BoundBlock)statement).Statements)
			{
				if (HasSideEffects(statement2))
				{
					return true;
				}
			}
			return false;
		case BoundKind.SequencePoint:
			return HasSideEffects(((BoundSequencePoint)statement).StatementOpt);
		case BoundKind.SequencePointWithSpan:
			return HasSideEffects(((BoundSequencePointWithSpan)statement).StatementOpt);
		default:
			return true;
		}
	}

	public override BoundNode? VisitCatchBlock(BoundCatchBlock node)
	{
		BoundExpression? exceptionFilterOpt = node.ExceptionFilterOpt;
		if (exceptionFilterOpt != null && exceptionFilterOpt.ConstantValueOpt?.BooleanValue == false)
		{
			return null;
		}
		BoundExpression rewrittenSource = (BoundExpression)Visit(node.ExceptionSourceOpt);
		BoundStatementList rewrittenFilterPrologue = (BoundStatementList)Visit(node.ExceptionFilterPrologueOpt);
		BoundExpression rewrittenFilter = (BoundExpression)Visit(node.ExceptionFilterOpt);
		BoundBlock rewrittenBody = (BoundBlock)Visit(node.Body);
		TypeSymbol rewrittenType = VisitType(node.ExceptionTypeOpt);
		if (Instrument)
		{
			Instrumenter.InstrumentCatchBlock(node, ref rewrittenSource, ref rewrittenFilterPrologue, ref rewrittenFilter, ref rewrittenBody, ref rewrittenType, _factory);
		}
		return node.Update(node.Locals, rewrittenSource, rewrittenType, rewrittenFilterPrologue, rewrittenFilter, rewrittenBody, node.IsSynthesizedAsyncCatchAll);
	}

	public override BoundNode VisitTupleBinaryOperator(BoundTupleBinaryOperator node)
	{
		TypeSymbol type = node.Type;
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		ArrayBuilder<LocalSymbol> instance2 = ArrayBuilder<LocalSymbol>.GetInstance();
		BoundExpression left = ReplaceTerminalElementsWithTemps(node.Left, node.Operators, instance, instance2);
		BoundExpression right = ReplaceTerminalElementsWithTemps(node.Right, node.Operators, instance, instance2);
		BoundExpression result = RewriteTupleNestedOperators(node.Operators, left, right, type, instance2, node.OperatorKind);
		return _factory.Sequence(instance2.ToImmutableAndFree(), instance.ToImmutableAndFree(), result);
	}

	private bool IsLikeTupleExpression(BoundExpression expr, [NotNullWhen(true)] out BoundTupleExpression? tuple)
	{
		if (!(expr is BoundTupleExpression boundTupleExpression))
		{
			if (expr is BoundConversion { Conversion: { Kind: var kind } conversion } boundConversion)
			{
				BoundExpression operand;
				switch (kind)
				{
				case ConversionKind.Identity:
					operand = boundConversion.Operand;
					return IsLikeTupleExpression(operand, out tuple);
				case ConversionKind.ImplicitTupleLiteral:
				{
					operand = boundConversion.Operand;
					BoundExpression expr2 = operand;
					return IsLikeTupleExpression(expr2, out tuple);
				}
				}
				operand = boundConversion.Operand;
				BoundExpression expr3 = operand;
				if (conversion.IsTupleConversion || conversion.IsTupleLiteralConversion)
				{
					if (!IsLikeTupleExpression(expr3, out tuple))
					{
						return false;
					}
					ImmutableArray<Conversion> underlyingConversions = conversion.UnderlyingConversions;
					ImmutableArray<TypeWithAnnotations> tupleElementTypesWithAnnotations = boundConversion.Type.TupleElementTypesWithAnnotations;
					ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(tuple.Arguments.Length);
					for (int i = 0; i < tuple.Arguments.Length; i++)
					{
						BoundExpression operand2 = tuple.Arguments[i];
						Conversion conversion2 = underlyingConversions[i];
						TypeSymbol type = tupleElementTypesWithAnnotations[i].Type;
						BoundConversion item = new BoundConversion(expr.Syntax, operand2, conversion2, boundConversion.Checked, boundConversion.ExplicitCastInCode, null, null, type, boundConversion.HasErrors);
						instance.Add(item);
					}
					ImmutableArray<BoundExpression> arguments = instance.ToImmutableAndFree();
					tuple = new BoundConvertedTupleLiteral(tuple.Syntax, null, wasTargetTyped: true, arguments, ImmutableArray<string>.Empty, ImmutableArray<bool>.Empty, boundConversion.Type, boundConversion.HasErrors);
					return true;
				}
				ConversionKind conversionKind = kind;
				BoundExpression boundExpression = operand;
				if (conversionKind == ConversionKind.ImplicitNullable || conversionKind == ConversionKind.ExplicitNullable)
				{
					TypeSymbol type2 = expr.Type;
					if ((object)type2 != null && type2.IsNullableType() && type2.StrippedType().Equals(boundExpression.Type, TypeCompareKind.AllIgnoreOptions))
					{
						return IsLikeTupleExpression(boundExpression, out tuple);
					}
				}
			}
			tuple = null;
			return false;
		}
		tuple = boundTupleExpression;
		return true;
	}

	private BoundExpression PushDownImplicitTupleConversion(BoundExpression expr, ArrayBuilder<BoundExpression> initEffects, ArrayBuilder<LocalSymbol> temps)
	{
		if (expr is BoundConversion { ConversionKind: ConversionKind.ImplicitTuple, Conversion: var conversion } boundConversion)
		{
			SyntaxNode syntax = boundConversion.Syntax;
			ImmutableArray<TypeWithAnnotations> tupleElementTypesWithAnnotations = expr.Type.TupleElementTypesWithAnnotations;
			int length = tupleElementTypesWithAnnotations.Length;
			ImmutableArray<FieldSymbol> tupleElements = boundConversion.Operand.Type.TupleElements;
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(length);
			BoundExpression tuple = DeferSideEffectingArgumentToTempForTupleEquality(LowerConversions(boundConversion.Operand), initEffects, temps);
			ImmutableArray<Conversion> underlyingConversions = conversion.UnderlyingConversions;
			for (int i = 0; i < length; i++)
			{
				BoundExpression operand = MakeTupleFieldAccessAndReportUseSiteDiagnostics(tuple, syntax, tupleElements[i]);
				BoundConversion item = new BoundConversion(syntax, operand, underlyingConversions[i], boundConversion.Checked, boundConversion.ExplicitCastInCode, null, null, tupleElementTypesWithAnnotations[i].Type, boundConversion.HasErrors);
				instance.Add(item);
			}
			return new BoundConvertedTupleLiteral(syntax, null, wasTargetTyped: true, instance.ToImmutableAndFree(), ImmutableArray<string>.Empty, ImmutableArray<bool>.Empty, expr.Type, expr.HasErrors);
		}
		return expr;
	}

	private BoundExpression ReplaceTerminalElementsWithTemps(BoundExpression expr, TupleBinaryOperatorInfo operators, ArrayBuilder<BoundExpression> initEffects, ArrayBuilder<LocalSymbol> temps)
	{
		if (operators.InfoKind == TupleBinaryOperatorInfoKind.Multiple)
		{
			expr = PushDownImplicitTupleConversion(expr, initEffects, temps);
			if (IsLikeTupleExpression(expr, out BoundTupleExpression tuple))
			{
				TupleBinaryOperatorInfo.Multiple multiple = (TupleBinaryOperatorInfo.Multiple)operators;
				ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(tuple.Arguments.Length);
				for (int i = 0; i < tuple.Arguments.Length; i++)
				{
					BoundExpression expr2 = tuple.Arguments[i];
					BoundExpression item = ReplaceTerminalElementsWithTemps(expr2, multiple.Operators[i], initEffects, temps);
					instance.Add(item);
				}
				ImmutableArray<BoundExpression> arguments = instance.ToImmutableAndFree();
				return new BoundConvertedTupleLiteral(tuple.Syntax, null, wasTargetTyped: false, arguments, ImmutableArray<string>.Empty, ImmutableArray<bool>.Empty, tuple.Type, tuple.HasErrors);
			}
		}
		return DeferSideEffectingArgumentToTempForTupleEquality(expr, initEffects, temps);
	}

	private BoundExpression DeferSideEffectingArgumentToTempForTupleEquality(BoundExpression expr, ArrayBuilder<BoundExpression> effects, ArrayBuilder<LocalSymbol> temps, bool enclosingConversionWasExplicit = false)
	{
		if (expr != null)
		{
			if ((object)expr.ConstantValueOpt != null)
			{
				return VisitExpression(expr);
			}
			if (expr is BoundConversion { Conversion: { Kind: var kind } conversion } boundConversion)
			{
				if (kind == ConversionKind.DefaultLiteral || conversion.IsTupleConversion)
				{
					return EvaluateSideEffectingArgumentToTemp(expr, effects, temps);
				}
				if (!conversionMustBePerformedOnOriginalExpression(kind))
				{
					if (conversion.IsUserDefined && (boundConversion.ExplicitCastInCode | enclosingConversionWasExplicit))
					{
						return EvaluateSideEffectingArgumentToTemp(expr, effects, temps);
					}
					BoundConversion boundConversion2 = boundConversion;
					BoundExpression operand = DeferSideEffectingArgumentToTempForTupleEquality(boundConversion2.Operand, effects, temps, boundConversion2.ExplicitCastInCode | enclosingConversionWasExplicit);
					return boundConversion2.UpdateOperand(operand);
				}
				return EvaluateSideEffectingArgumentToTemp(expr, effects, temps);
			}
			if (expr is BoundObjectCreationExpression boundObjectCreationExpression)
			{
				switch (boundObjectCreationExpression.Arguments.Length)
				{
				case 0:
				{
					TypeSymbol type = boundObjectCreationExpression.Type;
					if ((object)type == null || !type.IsNullableType())
					{
						break;
					}
					return new BoundLiteral(expr.Syntax, ConstantValue.Null, expr.Type);
				}
				case 1:
				{
					TypeSymbol type = boundObjectCreationExpression.Type;
					if ((object)type != null)
					{
						TypeSymbol type2 = type;
						BoundObjectCreationExpression boundObjectCreationExpression2 = boundObjectCreationExpression;
						if (type2.IsNullableType())
						{
							BoundExpression operand2 = DeferSideEffectingArgumentToTempForTupleEquality(boundObjectCreationExpression2.Arguments[0], effects, temps, enclosingConversionWasExplicit: true);
							Conversion conversion2 = Conversion.MakeNullableConversion(ConversionKind.ImplicitNullable, Conversion.Identity);
							return new BoundConversion(expr.Syntax, operand2, conversion2, @checked: false, explicitCastInCode: true, null, null, type2, expr.HasErrors);
						}
					}
					break;
				}
				}
			}
		}
		return EvaluateSideEffectingArgumentToTemp(expr, effects, temps);
		static bool conversionMustBePerformedOnOriginalExpression(ConversionKind conversionKind)
		{
			switch (conversionKind)
			{
			case ConversionKind.AnonymousFunction:
			case ConversionKind.MethodGroup:
			case ConversionKind.InterpolatedString:
			case ConversionKind.SwitchExpression:
			case ConversionKind.ConditionalExpression:
			case ConversionKind.StackAllocToPointerType:
			case ConversionKind.StackAllocToSpanType:
			case ConversionKind.ObjectCreation:
				return true;
			default:
				return false;
			}
		}
	}

	private BoundExpression RewriteTupleOperator(TupleBinaryOperatorInfo @operator, BoundExpression left, BoundExpression right, TypeSymbol boolType, ArrayBuilder<LocalSymbol> temps, BinaryOperatorKind operatorKind)
	{
		switch (@operator.InfoKind)
		{
		case TupleBinaryOperatorInfoKind.Multiple:
			return RewriteTupleNestedOperators((TupleBinaryOperatorInfo.Multiple)@operator, left, right, boolType, temps, operatorKind);
		case TupleBinaryOperatorInfoKind.Single:
			return RewriteTupleSingleOperator((TupleBinaryOperatorInfo.Single)@operator, left, right, boolType, operatorKind);
		case TupleBinaryOperatorInfoKind.NullNull:
		{
			TupleBinaryOperatorInfo.NullNull nullNull = (TupleBinaryOperatorInfo.NullNull)@operator;
			return new BoundLiteral(left.Syntax, ConstantValue.Create(nullNull.Kind == BinaryOperatorKind.Equal), boolType);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(@operator.InfoKind);
		}
	}

	private BoundExpression RewriteTupleNestedOperators(TupleBinaryOperatorInfo.Multiple operators, BoundExpression left, BoundExpression right, TypeSymbol boolType, ArrayBuilder<LocalSymbol> temps, BinaryOperatorKind operatorKind)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance();
		MakeNullableParts(left, temps, instance2, instance, saveHasValue: true, out BoundExpression hasValue, out BoundExpression value, out bool isNullable);
		MakeNullableParts(right, temps, instance2, instance, saveHasValue: false, out BoundExpression hasValue2, out BoundExpression value2, out bool isNullable2);
		BoundExpression result = RewriteNonNullableNestedTupleOperators(operators, value, value2, boolType, temps, operatorKind);
		BoundExpression boundExpression = _factory.Sequence(ImmutableArray<LocalSymbol>.Empty, instance2.ToImmutableAndFree(), result);
		if (!isNullable && !isNullable2)
		{
			return boundExpression;
		}
		bool flag = operatorKind == BinaryOperatorKind.Equal;
		if (hasValue2.ConstantValueOpt == ConstantValue.False)
		{
			return _factory.Sequence(ImmutableArray<LocalSymbol>.Empty, instance.ToImmutableAndFree(), flag ? _factory.Not(hasValue) : hasValue);
		}
		if (hasValue.ConstantValueOpt == ConstantValue.False)
		{
			return _factory.Sequence(ImmutableArray<LocalSymbol>.Empty, instance.ToImmutableAndFree(), flag ? _factory.Not(hasValue2) : hasValue2);
		}
		return _factory.Sequence(ImmutableArray<LocalSymbol>.Empty, instance.ToImmutableAndFree(), _factory.Conditional(_factory.Binary(BinaryOperatorKind.Equal, boolType, hasValue, hasValue2), _factory.Conditional(hasValue, boundExpression, MakeBooleanConstant(right.Syntax, flag), boolType), MakeBooleanConstant(right.Syntax, !flag), boolType));
	}

	private void MakeNullableParts(BoundExpression expr, ArrayBuilder<LocalSymbol> temps, ArrayBuilder<BoundExpression> innerEffects, ArrayBuilder<BoundExpression> outerEffects, bool saveHasValue, out BoundExpression hasValue, out BoundExpression value, out bool isNullable)
	{
		isNullable = !(expr is BoundTupleExpression) && (object)expr.Type != null && expr.Type.IsNullableType();
		if (!isNullable)
		{
			hasValue = MakeBooleanConstant(expr.Syntax, value: true);
			expr = PushDownImplicitTupleConversion(expr, innerEffects, temps);
			value = expr;
			return;
		}
		if (NullableNeverHasValue(expr))
		{
			hasValue = MakeBooleanConstant(expr.Syntax, value: false);
			value = new BoundDefaultExpression(expr.Syntax, expr.Type.StrippedType());
			return;
		}
		BoundExpression boundExpression = NullableAlwaysHasValue(expr);
		if (boundExpression != null)
		{
			hasValue = MakeBooleanConstant(expr.Syntax, value: true);
			value = PushDownImplicitTupleConversion(boundExpression, innerEffects, temps);
			value = LowerConversions(value);
			isNullable = false;
			return;
		}
		hasValue = makeNullableHasValue(expr);
		if (saveHasValue)
		{
			hasValue = MakeTemp(hasValue, temps, outerEffects);
		}
		value = MakeValueOrDefaultTemp(expr, temps, innerEffects);
		BoundExpression makeNullableHasValue(BoundExpression boundExpression2)
		{
			if (boundExpression2 is BoundConversion { Conversion: var conversion } boundConversion)
			{
				if (conversion.IsIdentity)
				{
					BoundExpression operand = boundConversion.Operand;
					return makeNullableHasValue(operand);
				}
				if (conversion.IsNullable)
				{
					ImmutableArray<Conversion> underlyingConversions = conversion.UnderlyingConversions;
					BoundExpression operand = boundConversion.Operand;
					BoundExpression boundExpression3 = operand;
					if (boundExpression2.Type.IsNullableType() && (object)boundExpression3.Type != null && boundExpression3.Type.IsNullableType() && !underlyingConversions[0].IsUserDefined)
					{
						return makeNullableHasValue(boundExpression3);
					}
				}
			}
			return _factory.MakeNullableHasValue(boundExpression2.Syntax, boundExpression2);
		}
	}

	private BoundLocal MakeTemp(BoundExpression loweredExpression, ArrayBuilder<LocalSymbol> temps, ArrayBuilder<BoundExpression> effects)
	{
		BoundLocal boundLocal = _factory.StoreToTemp(loweredExpression, out BoundAssignmentOperator store);
		effects.Add(store);
		temps.Add(boundLocal.LocalSymbol);
		return boundLocal;
	}

	private BoundExpression MakeValueOrDefaultTemp(BoundExpression expr, ArrayBuilder<LocalSymbol> temps, ArrayBuilder<BoundExpression> effects)
	{
		if (expr is BoundConversion { Conversion: var conversion } boundConversion)
		{
			if (conversion.IsIdentity)
			{
				BoundExpression operand = boundConversion.Operand;
				return MakeValueOrDefaultTemp(operand, temps, effects);
			}
			if (conversion.IsNullable)
			{
				ImmutableArray<Conversion> underlyingConversions = conversion.UnderlyingConversions;
				BoundExpression operand = boundConversion.Operand;
				BoundExpression boundExpression = operand;
				TypeSymbol type = expr.Type;
				if ((object)type != null && type.IsNullableType() && (object)boundExpression.Type != null && boundExpression.Type.IsNullableType())
				{
					Conversion conversion2 = underlyingConversions[0];
					if (conversion2.IsTupleConversion)
					{
						BoundExpression boundExpression2 = MakeValueOrDefaultTemp(boundExpression, temps, effects);
						ImmutableArray<TypeWithAnnotations> tupleElementTypesWithAnnotations = expr.Type.GetNullableUnderlyingType().TupleElementTypesWithAnnotations;
						int length = boundExpression2.Type.TupleElementTypesWithAnnotations.Length;
						ImmutableArray<Conversion> underlyingConversions2 = conversion2.UnderlyingConversions;
						ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(length);
						for (int i = 0; i < length; i++)
						{
							instance.Add(MakeBoundConversion(GetTuplePart(boundExpression2, i), underlyingConversions2[i], tupleElementTypesWithAnnotations[i], boundConversion));
						}
						return new BoundConvertedTupleLiteral(boundExpression2.Syntax, null, wasTargetTyped: false, instance.ToImmutableAndFree(), ImmutableArray<string>.Empty, ImmutableArray<bool>.Empty, expr.Type, expr.HasErrors).WithSuppression(expr.IsSuppressed);
					}
				}
			}
		}
		BoundExpression loweredExpression = MakeOptimizedGetValueOrDefault(expr.Syntax, expr);
		return MakeTemp(loweredExpression, temps, effects);
		static BoundExpression MakeBoundConversion(BoundExpression boundExpression3, Conversion conversion3, TypeWithAnnotations typeWithAnnotations, BoundConversion enclosing)
		{
			return new BoundConversion(boundExpression3.Syntax, boundExpression3, conversion3, enclosing.Checked, enclosing.ExplicitCastInCode, null, null, typeWithAnnotations.Type);
		}
	}

	private BoundExpression RewriteNonNullableNestedTupleOperators(TupleBinaryOperatorInfo.Multiple operators, BoundExpression left, BoundExpression right, TypeSymbol type, ArrayBuilder<LocalSymbol> temps, BinaryOperatorKind operatorKind)
	{
		ImmutableArray<TupleBinaryOperatorInfo> operators2 = operators.Operators;
		BoundExpression boundExpression = null;
		for (int i = 0; i < operators2.Length; i++)
		{
			BoundExpression tuplePart = GetTuplePart(left, i);
			BoundExpression tuplePart2 = GetTuplePart(right, i);
			BoundExpression boundExpression2 = RewriteTupleOperator(operators2[i], tuplePart, tuplePart2, type, temps, operatorKind);
			if (boundExpression == null)
			{
				boundExpression = boundExpression2;
				continue;
			}
			BinaryOperatorKind kind = ((operatorKind == BinaryOperatorKind.Equal) ? BinaryOperatorKind.LogicalBoolAnd : BinaryOperatorKind.LogicalBoolOr);
			boundExpression = _factory.Binary(kind, type, boundExpression, boundExpression2);
		}
		return boundExpression;
	}

	private BoundExpression GetTuplePart(BoundExpression tuple, int i)
	{
		if (tuple is BoundTupleExpression boundTupleExpression)
		{
			return boundTupleExpression.Arguments[i];
		}
		return MakeTupleFieldAccessAndReportUseSiteDiagnostics(tuple, tuple.Syntax, tuple.Type.TupleElements[i]);
	}

	private BoundExpression RewriteTupleSingleOperator(TupleBinaryOperatorInfo.Single single, BoundExpression left, BoundExpression right, TypeSymbol boolType, BinaryOperatorKind operatorKind)
	{
		left = LowerConversions(left);
		right = LowerConversions(right);
		if (single.Kind.IsDynamic())
		{
			BoundExpression loweredOperand = _dynamicFactory.MakeDynamicBinaryOperator(single.Kind, left, right, isCompoundAssignment: false, _compilation.DynamicType).ToExpression();
			if (operatorKind == BinaryOperatorKind.Equal)
			{
				return _factory.Not(MakeUnaryOperator(UnaryOperatorKind.DynamicFalse, left.Syntax, null, null, loweredOperand, boolType));
			}
			return MakeUnaryOperator(UnaryOperatorKind.DynamicTrue, left.Syntax, null, null, loweredOperand, boolType);
		}
		if (left.IsLiteralNull() && right.IsLiteralNull())
		{
			return new BoundLiteral(left.Syntax, ConstantValue.Create(operatorKind == BinaryOperatorKind.Equal), boolType);
		}
		BoundExpression boundExpression = MakeBinaryOperator(_factory.Syntax, single.Kind, left, right, single.MethodSymbolOpt?.ReturnType ?? boolType, single.MethodSymbolOpt, single.ConstrainedToTypeOpt);
		UnaryOperatorSignature boolOperator = single.BoolOperator;
		BoundExpression boundExpression2 = ApplyConversionIfNotIdentity(single.ConversionForBool, single.ConversionForBoolPlaceholder, boundExpression);
		BoundExpression boundExpression3;
		if (boolOperator.Kind != UnaryOperatorKind.Error)
		{
			boundExpression3 = MakeUnaryOperator(boolOperator.Kind, boundExpression.Syntax, boolOperator.Method, boolOperator.ConstrainedToTypeOpt, boundExpression2, boolType);
			if (operatorKind == BinaryOperatorKind.Equal)
			{
				boundExpression3 = _factory.Not(boundExpression3);
			}
		}
		else
		{
			boundExpression3 = boundExpression2;
		}
		return boundExpression3;
	}

	private BoundExpression LowerConversions(BoundExpression expr)
	{
		if (!(expr is BoundConversion boundConversion))
		{
			return expr;
		}
		return MakeConversionNode(boundConversion, boundConversion.Syntax, LowerConversions(boundConversion.Operand), boundConversion.Conversion, boundConversion.Checked, boundConversion.ExplicitCastInCode, boundConversion.ConstantValueOpt, boundConversion.Type);
	}

	public override BoundNode VisitTupleLiteral(BoundTupleLiteral node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_TupleCreationExpression.cs", 17);
	}

	public override BoundNode VisitConvertedTupleLiteral(BoundConvertedTupleLiteral node)
	{
		return VisitTupleExpression(node);
	}

	private BoundNode VisitTupleExpression(BoundTupleExpression node)
	{
		ImmutableArray<BoundExpression> rewrittenArguments = VisitList(node.Arguments);
		return RewriteTupleCreationExpression(node, rewrittenArguments);
	}

	private BoundExpression RewriteTupleCreationExpression(BoundTupleExpression node, ImmutableArray<BoundExpression> rewrittenArguments)
	{
		return MakeTupleCreationExpression(node.Syntax, (NamedTypeSymbol)node.Type, rewrittenArguments);
	}

	private BoundExpression MakeTupleCreationExpression(SyntaxNode syntax, NamedTypeSymbol type, ImmutableArray<BoundExpression> rewrittenArguments)
	{
		ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		NamedTypeSymbol.GetUnderlyingTypeChain(type, instance);
		try
		{
			NamedTypeSymbol namedTypeSymbol = instance.Pop();
			ImmutableArray<BoundExpression> arguments = ImmutableArray.Create(rewrittenArguments, instance.Count * 7, namedTypeSymbol.Arity);
			MethodSymbol methodSymbol = (MethodSymbol)NamedTypeSymbol.GetWellKnownMemberInType(namedTypeSymbol.OriginalDefinition, NamedTypeSymbol.GetTupleCtor(namedTypeSymbol.Arity), _diagnostics, syntax);
			if ((object)methodSymbol == null)
			{
				return _factory.BadExpression(type);
			}
			MethodSymbol constructor = methodSymbol.AsMember(namedTypeSymbol);
			BoundObjectCreationExpression boundObjectCreationExpression = new BoundObjectCreationExpression(syntax, constructor, arguments);
			Binder.CheckRequiredMembersInObjectInitializer(constructor, ImmutableArray<BoundExpression>.Empty, syntax, _diagnostics);
			if (instance.Count > 0)
			{
				MethodSymbol methodSymbol2 = (MethodSymbol)NamedTypeSymbol.GetWellKnownMemberInType(instance.Peek().OriginalDefinition, NamedTypeSymbol.GetTupleCtor(8), _diagnostics, syntax);
				if ((object)methodSymbol2 == null)
				{
					return _factory.BadExpression(type);
				}
				Binder.CheckRequiredMembersInObjectInitializer(methodSymbol2, ImmutableArray<BoundExpression>.Empty, syntax, _diagnostics);
				do
				{
					ImmutableArray<BoundExpression> arguments2 = ImmutableArray.Create(rewrittenArguments, (instance.Count - 1) * 7, 7).Add(boundObjectCreationExpression);
					MethodSymbol constructor2 = methodSymbol2.AsMember(instance.Pop());
					boundObjectCreationExpression = new BoundObjectCreationExpression(syntax, constructor2, arguments2);
				}
				while (instance.Count > 0);
			}
			return boundObjectCreationExpression.Update(boundObjectCreationExpression.Constructor, boundObjectCreationExpression.Arguments, boundObjectCreationExpression.ArgumentNamesOpt, boundObjectCreationExpression.ArgumentRefKindsOpt, boundObjectCreationExpression.Expanded, boundObjectCreationExpression.ArgsToParamsOpt, boundObjectCreationExpression.DefaultArguments, boundObjectCreationExpression.ConstantValueOpt, boundObjectCreationExpression.InitializerExpressionOpt, type);
		}
		finally
		{
			instance.Free();
		}
	}

	public override BoundNode VisitUnaryOperator(BoundUnaryOperator node)
	{
		switch (node.OperatorKind.Operator())
		{
		case UnaryOperatorKind.PostfixIncrement:
		case UnaryOperatorKind.PostfixDecrement:
		case UnaryOperatorKind.PrefixIncrement:
		case UnaryOperatorKind.PrefixDecrement:
			return base.VisitUnaryOperator(node);
		default:
		{
			if (node.Operand.Kind == BoundKind.BinaryOperator)
			{
				BoundBinaryOperator boundBinaryOperator = (BoundBinaryOperator)node.Operand;
				if ((node.OperatorKind == UnaryOperatorKind.DynamicTrue && boundBinaryOperator.OperatorKind == BinaryOperatorKind.DynamicLogicalOr) || (node.OperatorKind == UnaryOperatorKind.DynamicFalse && boundBinaryOperator.OperatorKind == BinaryOperatorKind.DynamicLogicalAnd))
				{
					return VisitBinaryOperator(boundBinaryOperator, node);
				}
			}
			BoundExpression loweredOperand = VisitExpression(node.Operand);
			return MakeUnaryOperator(node, node.OperatorKind, node.Syntax, node.MethodOpt, node.ConstrainedToTypeOpt, loweredOperand, node.Type);
		}
		}
	}

	private BoundExpression MakeUnaryOperator(UnaryOperatorKind kind, SyntaxNode syntax, MethodSymbol? method, TypeSymbol? constrainedToTypeOpt, BoundExpression loweredOperand, TypeSymbol type)
	{
		return MakeUnaryOperator(null, kind, syntax, method, constrainedToTypeOpt, loweredOperand, type);
	}

	private BoundExpression MakeUnaryOperator(BoundUnaryOperator? oldNode, UnaryOperatorKind kind, SyntaxNode syntax, MethodSymbol? method, TypeSymbol? constrainedToTypeOpt, BoundExpression loweredOperand, TypeSymbol type)
	{
		if (kind.IsDynamic())
		{
			ConstantValue constantValue = UnboxConstant(loweredOperand);
			if (constantValue == ConstantValue.True || constantValue == ConstantValue.False)
			{
				switch (kind)
				{
				case UnaryOperatorKind.DynamicTrue:
					return _factory.Literal(constantValue.BooleanValue);
				case UnaryOperatorKind.DynamicLogicalNegation:
					return MakeConversionNode(_factory.Literal(!constantValue.BooleanValue), type, @checked: false);
				}
			}
			return _dynamicFactory.MakeDynamicUnaryOperator(kind, loweredOperand, type).ToExpression();
		}
		if (kind.IsLifted())
		{
			if (!_inExpressionLambda)
			{
				return LowerLiftedUnaryOperator(kind, syntax, method, constrainedToTypeOpt, loweredOperand, type);
			}
		}
		else if (kind.IsUserDefined())
		{
			if (!_inExpressionLambda || kind == UnaryOperatorKind.UserDefinedTrue || kind == UnaryOperatorKind.UserDefinedFalse)
			{
				return BoundCall.Synthesized(syntax, ((object)constrainedToTypeOpt == null) ? null : new BoundTypeExpression(syntax, null, constrainedToTypeOpt), ThreeState.Unknown, method, loweredOperand);
			}
		}
		else if (kind.Operator() == UnaryOperatorKind.UnaryPlus)
		{
			return loweredOperand;
		}
		switch (kind)
		{
		case UnaryOperatorKind.EnumBitwiseComplement:
		{
			NamedTypeSymbol enumUnderlyingType = loweredOperand.Type.GetEnumUnderlyingType();
			SpecialType enumPromotedType = Binder.GetEnumPromotedType(enumUnderlyingType.SpecialType);
			NamedTypeSymbol namedTypeSymbol = ((enumPromotedType == enumUnderlyingType.SpecialType) ? enumUnderlyingType : _compilation.GetSpecialType(enumPromotedType));
			BoundExpression boundExpression = MakeConversionNode(loweredOperand, namedTypeSymbol, @checked: false);
			UnaryOperatorKind operatorKind = kind.Operator().WithType(enumPromotedType);
			BoundUnaryOperator boundUnaryOperator = ((oldNode != null) ? oldNode.Update(operatorKind, boundExpression, oldNode.ConstantValueOpt, method, constrainedToTypeOpt, boundExpression.ResultKind, namedTypeSymbol) : new BoundUnaryOperator(syntax, operatorKind, boundExpression, null, method, constrainedToTypeOpt, LookupResultKind.Viable, namedTypeSymbol));
			return MakeConversionNode(boundUnaryOperator.Syntax, boundUnaryOperator, Conversion.ExplicitEnumeration, type, @checked: false);
		}
		case UnaryOperatorKind.DecimalUnaryMinus:
			method = (MethodSymbol)_compilation.Assembly.GetSpecialTypeMember(SpecialMember.System_Decimal__op_UnaryNegation);
			if (!_inExpressionLambda)
			{
				return BoundCall.Synthesized(syntax, null, ThreeState.Unknown, method, loweredOperand);
			}
			break;
		}
		if (oldNode == null)
		{
			return new BoundUnaryOperator(syntax, kind, loweredOperand, null, method, constrainedToTypeOpt, LookupResultKind.Viable, type);
		}
		return oldNode.Update(kind, loweredOperand, oldNode.ConstantValueOpt, method, constrainedToTypeOpt, oldNode.ResultKind, type);
	}

	private BoundExpression LowerLiftedUnaryOperator(UnaryOperatorKind kind, SyntaxNode syntax, MethodSymbol? method, TypeSymbol? constrainedToTypeOpt, BoundExpression loweredOperand, TypeSymbol type)
	{
		BoundExpression boundExpression = OptimizeLiftedUnaryOperator(kind, syntax, method, constrainedToTypeOpt, loweredOperand, type);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		BoundLocal boundLocal = _factory.StoreToTemp(loweredOperand, out BoundAssignmentOperator store);
		MethodSymbol method2 = UnsafeGetNullableMethod(syntax, boundLocal.Type, SpecialMember.System_Nullable_T_GetValueOrDefault);
		BoundExpression rewrittenCondition = _factory.MakeNullableHasValue(syntax, boundLocal);
		BoundExpression nonNullOperand = BoundCall.Synthesized(syntax, boundLocal, ThreeState.Unknown, method2);
		BoundExpression liftedUnaryOperatorConsequence = GetLiftedUnaryOperatorConsequence(kind, syntax, method, constrainedToTypeOpt, type, nonNullOperand);
		BoundExpression rewrittenAlternative = new BoundDefaultExpression(syntax, type);
		BoundExpression value = RewriteConditionalOperator(syntax, rewrittenCondition, liftedUnaryOperatorConsequence, rewrittenAlternative, null, type, isRef: false);
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal.LocalSymbol), ImmutableArray.Create((BoundExpression)store), value, type);
	}

	private BoundExpression? OptimizeLiftedUnaryOperator(UnaryOperatorKind operatorKind, SyntaxNode syntax, MethodSymbol? method, TypeSymbol? constrainedToTypeOpt, BoundExpression loweredOperand, TypeSymbol type)
	{
		if (NullableNeverHasValue(loweredOperand))
		{
			return new BoundDefaultExpression(syntax, type);
		}
		BoundExpression boundExpression = NullableAlwaysHasValue(loweredOperand);
		if (boundExpression != null)
		{
			return GetLiftedUnaryOperatorConsequence(operatorKind, syntax, method, constrainedToTypeOpt, type, boundExpression);
		}
		if (loweredOperand is BoundLoweredConditionalAccess boundLoweredConditionalAccess && (boundLoweredConditionalAccess.WhenNullOpt == null || boundLoweredConditionalAccess.WhenNullOpt.IsDefaultValue()))
		{
			BoundExpression boundExpression2 = LowerLiftedUnaryOperator(operatorKind, syntax, method, constrainedToTypeOpt, boundLoweredConditionalAccess.WhenNotNull, type);
			return boundLoweredConditionalAccess.Update(boundLoweredConditionalAccess.Receiver, boundLoweredConditionalAccess.HasValueMethodOpt, boundExpression2, null, boundLoweredConditionalAccess.Id, boundLoweredConditionalAccess.ForceCopyOfNullableValueType, boundExpression2.Type);
		}
		if (loweredOperand.Kind == BoundKind.Sequence)
		{
			BoundSequence boundSequence = (BoundSequence)loweredOperand;
			if (boundSequence.Value.Kind == BoundKind.ConditionalOperator)
			{
				BoundConditionalOperator boundConditionalOperator = (BoundConditionalOperator)boundSequence.Value;
				if (NullableAlwaysHasValue(boundConditionalOperator.Consequence) != null && NullableNeverHasValue(boundConditionalOperator.Alternative))
				{
					return new BoundSequence(syntax, boundSequence.Locals, boundSequence.SideEffects, RewriteConditionalOperator(syntax, boundConditionalOperator.Condition, MakeUnaryOperator(operatorKind, syntax, method, constrainedToTypeOpt, boundConditionalOperator.Consequence, type), MakeUnaryOperator(operatorKind, syntax, method, constrainedToTypeOpt, boundConditionalOperator.Alternative, type), null, type, isRef: false), type);
				}
			}
		}
		return null;
	}

	private BoundExpression GetLiftedUnaryOperatorConsequence(UnaryOperatorKind kind, SyntaxNode syntax, MethodSymbol? method, TypeSymbol? constrainedToTypeOpt, TypeSymbol type, BoundExpression nonNullOperand)
	{
		MethodSymbol constructor = UnsafeGetNullableMethod(syntax, type, SpecialMember.System_Nullable_T__ctor);
		BoundExpression boundExpression = MakeUnaryOperator(null, kind.Unlifted(), syntax, method, constrainedToTypeOpt, nonNullOperand, type.GetNullableUnderlyingType());
		return new BoundObjectCreationExpression(syntax, constructor, boundExpression);
	}

	private static bool IsIncrement(BoundIncrementOperator node)
	{
		UnaryOperatorKind unaryOperatorKind = node.OperatorKind.Operator();
		if (unaryOperatorKind != UnaryOperatorKind.PostfixIncrement)
		{
			return unaryOperatorKind == UnaryOperatorKind.PrefixIncrement;
		}
		return true;
	}

	private static bool IsPrefix(BoundIncrementOperator node)
	{
		UnaryOperatorKind unaryOperatorKind = node.OperatorKind.Operator();
		if (unaryOperatorKind != UnaryOperatorKind.PrefixIncrement)
		{
			return unaryOperatorKind == UnaryOperatorKind.PrefixDecrement;
		}
		return true;
	}

	public override BoundNode VisitIncrementOperator(BoundIncrementOperator node)
	{
		return VisitIncrementOperator(node, used: true);
	}

	private BoundExpression VisitIncrementOperator(BoundIncrementOperator node, bool used)
	{
		MethodSymbol? methodOpt = node.MethodOpt;
		if ((object)methodOpt != null && !methodOpt.IsStatic)
		{
			return VisitInstanceIncrementOperator(node, used);
		}
		return VisitBuiltInOrStaticIncrementOperator(node);
	}

	private BoundExpression VisitInstanceIncrementOperator(BoundIncrementOperator node, bool used)
	{
		SyntaxNode syntax = node.Syntax;
		if (!used)
		{
			return BoundCall.Synthesized(syntax, ApplyConversionIfNotIdentity(node.OperandConversion, node.OperandPlaceholder, VisitExpression(node.Operand)), ThreeState.False, node.MethodOpt);
		}
		TypeSymbol type = node.Operand.Type;
		if (!IsPrefix(node))
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/LocalRewriter_UnaryOperator.cs", 427);
		}
		if (type.IsReferenceType)
		{
			BoundLocal boundLocal = _factory.StoreToTemp(VisitExpression(node.Operand), out BoundAssignmentOperator store);
			return new BoundSequence(syntax, ImmutableCollectionsMarshal.AsImmutableArray(new LocalSymbol[1] { boundLocal.LocalSymbol }), ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[2]
			{
				store,
				BoundCall.Synthesized(syntax, ApplyConversionIfNotIdentity(node.OperandConversion, node.OperandPlaceholder, boundLocal), ThreeState.False, node.MethodOpt)
			}), boundLocal, type);
		}
		return MakeInstanceCompoundAssignmentOperatorResult(node.Syntax, node.Operand, null, node.MethodOpt, node.OperatorKind.IsChecked(), AssignmentKind.IncrementDecrement);
	}

	private BoundExpression MakeInstanceCompoundAssignmentOperatorResult(SyntaxNode syntax, BoundExpression left, BoundExpression? rightOpt, MethodSymbol operatorMethod, bool isChecked, AssignmentKind assignmentKind)
	{
		TypeSymbol type = left.Type;
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance();
		BoundExpression boundExpression = TransformCompoundAssignmentLHS(left, instance2, instance, isDynamicAssignment: false);
		BoundLocal boundLocal = _factory.StoreToTemp(boundExpression, out BoundAssignmentOperator store);
		instance.Add(boundLocal.LocalSymbol);
		instance2.Add(store);
		rightOpt = VisitExpression(rightOpt);
		if (type.IsValueType)
		{
			BoundCall item = makeIncrementCall(syntax, boundLocal, rightOpt, operatorMethod);
			BoundExpression item2 = makeAssignmentBack(syntax, boundExpression, boundLocal, isChecked, assignmentKind);
			instance2.Add(item);
			instance2.Add(item2);
		}
		else
		{
			if (rightOpt != null)
			{
				BoundLocal boundLocal2 = _factory.StoreToTemp(rightOpt, out store);
				instance.Add(boundLocal2.LocalSymbol);
				instance2.Add(store);
				rightOpt = boundLocal2;
			}
			BoundCall boundCall = makeIncrementCall(syntax, boundLocal, rightOpt, operatorMethod);
			BoundExpression boundExpression2 = makeAssignmentBack(syntax, boundExpression, boundLocal, isChecked, assignmentKind);
			BoundExpression condition = _factory.IsNotNullReference(_factory.Default(type));
			instance2.Add(_factory.Conditional(condition, new BoundSequence(syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[2] { boundCall, boundExpression2 }), boundLocal, type), boundCall, type));
		}
		return new BoundSequence(syntax, instance.ToImmutableAndFree(), instance2.ToImmutableAndFree(), boundLocal, type);
		BoundExpression makeAssignmentBack(SyntaxNode syntax2, BoundExpression transformedLHS, BoundLocal boundTemp, bool isChecked2, AssignmentKind assignmentKind2)
		{
			return MakeAssignmentOperator(syntax2, transformedLHS, boundTemp, used: false, isChecked2, assignmentKind2);
		}
		static BoundCall makeIncrementCall(SyntaxNode syntax2, BoundLocal boundTemp, BoundExpression? boundExpression3, MethodSymbol method)
		{
			return BoundCall.Synthesized(syntax2, boundTemp, ThreeState.False, method, (boundExpression3 == null) ? ImmutableArray<BoundExpression>.Empty : ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1] { boundExpression3 }));
		}
	}

	public BoundExpression VisitBuiltInOrStaticIncrementOperator(BoundIncrementOperator node)
	{
		bool flag = IsPrefix(node);
		bool isDynamicAssignment = node.OperatorKind.IsDynamic();
		bool isChecked = node.OperatorKind.IsChecked();
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance();
		SyntaxNode syntax = node.Syntax;
		BoundExpression boundExpression = TransformCompoundAssignmentLHS(node.Operand, instance2, instance, isDynamicAssignment);
		TypeSymbol type = boundExpression.Type;
		LocalSymbol localSymbol = _factory.SynthesizedLocal(type);
		instance.Add(localSymbol);
		BoundExpression boundExpression2 = new BoundLocal(syntax, localSymbol, null, type);
		BoundExpression newValue = makeBuiltInOrStaticIncrementOperator(node, flag ? MakeRValue(boundExpression) : boundExpression2);
		if (isIndirectOrInstanceField(boundExpression))
		{
			return rewriteWithRefOperand(flag, isChecked, instance, instance2, syntax, boundExpression, boundExpression2, newValue);
		}
		return rewriteWithNotRefOperand(flag, isChecked, instance, instance2, syntax, boundExpression, boundExpression2, newValue);
		static bool isIndirectOrInstanceField(BoundExpression expression)
		{
			return expression.Kind switch
			{
				BoundKind.Local => ((BoundLocal)expression).LocalSymbol.RefKind != RefKind.None, 
				BoundKind.Parameter => ((BoundParameter)expression).ParameterSymbol.RefKind != RefKind.None, 
				BoundKind.FieldAccess => !((BoundFieldAccess)expression).FieldSymbol.IsStatic, 
				_ => false, 
			};
		}
		BoundExpression makeBuiltInOrStaticIncrementOperator(BoundIncrementOperator boundIncrementOperator, BoundExpression rewrittenValueToIncrement)
		{
			if (!boundIncrementOperator.OperatorKind.IsDynamic())
			{
				return ApplyConversionIfNotIdentity(replacement: (boundIncrementOperator.OperatorKind.OperandTypes() != UnaryOperatorKind.UserDefined) ? MakeBuiltInIncrementOperator(boundIncrementOperator, rewrittenValueToIncrement) : MakeUserDefinedStaticIncrementOperator(boundIncrementOperator, rewrittenValueToIncrement), conversion: boundIncrementOperator.ResultConversion, placeholder: boundIncrementOperator.ResultPlaceholder);
			}
			return _dynamicFactory.MakeDynamicUnaryOperator(boundIncrementOperator.OperatorKind, rewrittenValueToIncrement, boundIncrementOperator.Type).ToExpression();
		}
		BoundExpression rewriteWithNotRefOperand(bool isPrefix, bool isChecked2, ArrayBuilder<LocalSymbol> tempSymbols, ArrayBuilder<BoundExpression> tempInitializers, SyntaxNode syntax2, BoundExpression transformedLHS, BoundExpression boundTemp, BoundExpression boundExpression3)
		{
			tempInitializers.Add(MakeAssignmentOperator(syntax2, boundTemp, isPrefix ? boundExpression3 : MakeRValue(transformedLHS), used: false, isChecked2, AssignmentKind.SimpleAssignment));
			if (!isPrefix && IsExtensionBlockMemberAccessWithByValPossiblyStructReceiver(transformedLHS))
			{
				BoundLocal boundLocal = _factory.StoreToTemp(boundExpression3, out BoundAssignmentOperator store);
				tempSymbols.Add(boundLocal.LocalSymbol);
				tempInitializers.Add(store);
				tempInitializers.Add(MakeAssignmentOperator(syntax2, transformedLHS, boundLocal, used: false, isChecked2, AssignmentKind.IncrementDecrement));
			}
			else
			{
				tempInitializers.Add(MakeAssignmentOperator(syntax2, transformedLHS, isPrefix ? boundTemp : boundExpression3, used: false, isChecked2, AssignmentKind.IncrementDecrement));
			}
			return new BoundSequence(syntax2, tempSymbols.ToImmutableAndFree(), tempInitializers.ToImmutableAndFree(), boundTemp, boundTemp.Type);
		}
		BoundExpression rewriteWithRefOperand(bool isPrefix, bool isChecked2, ArrayBuilder<LocalSymbol> tempSymbols, ArrayBuilder<BoundExpression> tempInitializers, SyntaxNode syntax2, BoundExpression operand, BoundExpression boundTemp, BoundExpression boundExpression4)
		{
			BoundExpression boundExpression3 = (isPrefix ? boundExpression4 : MakeRValue(operand));
			BoundExpression item = MakeAssignmentOperator(syntax2, boundTemp, boundExpression3, used: false, isChecked2, AssignmentKind.SimpleAssignment);
			BoundExpression value = (isPrefix ? boundTemp : boundExpression4);
			BoundSequence rewrittenRight = new BoundSequence(syntax2, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(item), value, boundExpression3.Type);
			BoundExpression item2 = MakeAssignmentOperator(syntax2, operand, rewrittenRight, used: false, isChecked2, AssignmentKind.IncrementDecrement);
			tempInitializers.Add(item2);
			return new BoundSequence(syntax2, tempSymbols.ToImmutableAndFree(), tempInitializers.ToImmutableAndFree(), boundTemp, boundTemp.Type);
		}
	}

	private BoundExpression ApplyConversionIfNotIdentity(BoundExpression? conversion, BoundValuePlaceholder? placeholder, BoundExpression replacement)
	{
		if (hasNonIdentityConversion(conversion))
		{
			return ApplyConversion(conversion, placeholder, replacement);
		}
		return replacement;
		static bool hasNonIdentityConversion([NotNullWhen(true)] BoundExpression? expression)
		{
			while (expression is BoundConversion { Conversion: var conversion2 } boundConversion)
			{
				if (!conversion2.IsIdentity)
				{
					return true;
				}
				expression = boundConversion.Operand;
			}
			return false;
		}
	}

	private BoundExpression ApplyConversion(BoundExpression conversion, BoundValuePlaceholder placeholder, BoundExpression replacement)
	{
		AddPlaceholderReplacement(placeholder, replacement);
		replacement = VisitExpression(conversion);
		RemovePlaceholderReplacement(placeholder);
		return replacement;
	}

	private BoundExpression MakeUserDefinedStaticIncrementOperator(BoundIncrementOperator node, BoundExpression rewrittenValueToIncrement)
	{
		bool flag = node.OperatorKind.IsLifted();
		node.OperatorKind.IsChecked();
		SyntaxNode syntax = node.Syntax;
		TypeSymbol typeSymbol = node.MethodOpt.GetParameterType(0);
		if (flag)
		{
			typeSymbol = _compilation.GetSpecialType(SpecialType.System_Nullable_T).Construct(typeSymbol);
		}
		BoundExpression boundExpression = ApplyConversionIfNotIdentity(node.OperandConversion, node.OperandPlaceholder, rewrittenValueToIncrement);
		if (!flag)
		{
			return BoundCall.Synthesized(syntax, ((object)node.ConstrainedToTypeOpt == null) ? null : new BoundTypeExpression(syntax, null, node.ConstrainedToTypeOpt), ThreeState.Unknown, node.MethodOpt, boundExpression);
		}
		BoundLocal boundLocal = _factory.StoreToTemp(boundExpression, out BoundAssignmentOperator store);
		MethodSymbol method = UnsafeGetNullableMethod(syntax, typeSymbol, SpecialMember.System_Nullable_T_GetValueOrDefault);
		MethodSymbol constructor = UnsafeGetNullableMethod(syntax, typeSymbol, SpecialMember.System_Nullable_T__ctor);
		BoundExpression rewrittenCondition = _factory.MakeNullableHasValue(node.Syntax, boundLocal);
		BoundExpression arg = BoundCall.Synthesized(syntax, boundLocal, ThreeState.Unknown, method);
		BoundExpression boundExpression2 = BoundCall.Synthesized(syntax, ((object)node.ConstrainedToTypeOpt == null) ? null : new BoundTypeExpression(syntax, null, node.ConstrainedToTypeOpt), ThreeState.Unknown, node.MethodOpt, arg);
		BoundExpression rewrittenConsequence = new BoundObjectCreationExpression(syntax, constructor, boundExpression2);
		BoundExpression rewrittenAlternative = new BoundDefaultExpression(syntax, typeSymbol);
		BoundExpression value = RewriteConditionalOperator(syntax, rewrittenCondition, rewrittenConsequence, rewrittenAlternative, null, typeSymbol, isRef: false);
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal.LocalSymbol), ImmutableArray.Create((BoundExpression)store), value, typeSymbol);
	}

	private BoundExpression MakeBuiltInIncrementOperator(BoundIncrementOperator node, BoundExpression rewrittenValueToIncrement)
	{
		TypeSymbol unaryOperatorType = GetUnaryOperatorType(node);
		BinaryOperatorKind correspondingBinaryOperator = GetCorrespondingBinaryOperator(node);
		correspondingBinaryOperator = (BinaryOperatorKind)((int)correspondingBinaryOperator | (IsIncrement(node) ? 4352 : 4608));
		(TypeSymbol, ConstantValue) constantOneForIncrement = GetConstantOneForIncrement(_compilation, correspondingBinaryOperator);
		TypeSymbol typeSymbol = constantOneForIncrement.Item1;
		ConstantValue item = constantOneForIncrement.Item2;
		BoundExpression boundExpression = MakeLiteral(node.Syntax, item, typeSymbol);
		if (correspondingBinaryOperator.IsLifted())
		{
			typeSymbol = _compilation.GetOrCreateNullableType(typeSymbol);
			MethodSymbol constructor = UnsafeGetNullableMethod(node.Syntax, typeSymbol, SpecialMember.System_Nullable_T__ctor);
			boundExpression = new BoundObjectCreationExpression(node.Syntax, constructor, boundExpression);
		}
		BoundExpression replacement = rewrittenValueToIncrement;
		bool flag = node.OperatorKind.IsChecked();
		replacement = ApplyConversionIfNotIdentity(node.OperandConversion, node.OperandPlaceholder, replacement);
		if (node.OperatorKind.OperandTypes() == UnaryOperatorKind.Pointer)
		{
			return MakeBinaryOperator(node.Syntax, correspondingBinaryOperator, replacement, boundExpression, replacement.Type, null, null);
		}
		replacement = MakeConversionNode(replacement, typeSymbol, flag, acceptFailingConversion: false, markAsChecked: true);
		BoundExpression rewrittenOperand = ((unaryOperatorType.SpecialType == SpecialType.System_Decimal) ? MakeDecimalIncDecOperator(node.Syntax, correspondingBinaryOperator, replacement) : ((!unaryOperatorType.IsNullableType() || unaryOperatorType.GetNullableUnderlyingType().SpecialType != SpecialType.System_Decimal) ? MakeBinaryOperator(node.Syntax, correspondingBinaryOperator, replacement, boundExpression, typeSymbol, null, null) : MakeLiftedDecimalIncDecOperator(node.Syntax, correspondingBinaryOperator, replacement)));
		return MakeConversionNode(rewrittenOperand, unaryOperatorType, flag, acceptFailingConversion: false, markAsChecked: true);
	}

	private MethodSymbol GetDecimalIncDecOperator(BinaryOperatorKind oper)
	{
		SpecialMember member = oper.Operator() switch
		{
			BinaryOperatorKind.Addition => SpecialMember.System_Decimal__op_Increment, 
			BinaryOperatorKind.Subtraction => SpecialMember.System_Decimal__op_Decrement, 
			_ => throw ExceptionUtilities.UnexpectedValue(oper.Operator()), 
		};
		return (MethodSymbol)_compilation.Assembly.GetSpecialTypeMember(member);
	}

	private BoundExpression MakeDecimalIncDecOperator(SyntaxNode syntax, BinaryOperatorKind oper, BoundExpression operand)
	{
		MethodSymbol decimalIncDecOperator = GetDecimalIncDecOperator(oper);
		return BoundCall.Synthesized(syntax, null, ThreeState.Unknown, decimalIncDecOperator, operand);
	}

	private BoundExpression MakeLiftedDecimalIncDecOperator(SyntaxNode syntax, BinaryOperatorKind oper, BoundExpression operand)
	{
		MethodSymbol decimalIncDecOperator = GetDecimalIncDecOperator(oper);
		MethodSymbol method = UnsafeGetNullableMethod(syntax, operand.Type, SpecialMember.System_Nullable_T_GetValueOrDefault);
		MethodSymbol constructor = UnsafeGetNullableMethod(syntax, operand.Type, SpecialMember.System_Nullable_T__ctor);
		BoundExpression rewrittenCondition = _factory.MakeNullableHasValue(syntax, operand);
		BoundExpression arg = BoundCall.Synthesized(syntax, operand, ThreeState.Unknown, method);
		BoundExpression boundExpression = BoundCall.Synthesized(syntax, null, ThreeState.Unknown, decimalIncDecOperator, arg);
		BoundExpression rewrittenConsequence = new BoundObjectCreationExpression(syntax, constructor, boundExpression);
		BoundExpression rewrittenAlternative = new BoundDefaultExpression(syntax, operand.Type);
		return RewriteConditionalOperator(syntax, rewrittenCondition, rewrittenConsequence, rewrittenAlternative, null, operand.Type, isRef: false);
	}

	private BoundExpression MakeRValue(BoundExpression transformedExpression)
	{
		switch (transformedExpression.Kind)
		{
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)transformedExpression;
			return MakePropertyGetAccess(transformedExpression.Syntax, boundPropertyAccess.ReceiverOpt, boundPropertyAccess.PropertySymbol, boundPropertyAccess);
		}
		case BoundKind.DynamicMemberAccess:
		{
			BoundDynamicMemberAccess boundDynamicMemberAccess = (BoundDynamicMemberAccess)transformedExpression;
			return _dynamicFactory.MakeDynamicGetMember(boundDynamicMemberAccess.Receiver, boundDynamicMemberAccess.Name, resultIndexed: false).ToExpression();
		}
		case BoundKind.IndexerAccess:
		{
			BoundIndexerAccess boundIndexerAccess = (BoundIndexerAccess)transformedExpression;
			return MakePropertyGetAccess(transformedExpression.Syntax, boundIndexerAccess.ReceiverOpt, boundIndexerAccess.Indexer, boundIndexerAccess.Arguments, boundIndexerAccess.ArgumentRefKindsOpt);
		}
		case BoundKind.DynamicIndexerAccess:
		{
			BoundDynamicIndexerAccess boundDynamicIndexerAccess = (BoundDynamicIndexerAccess)transformedExpression;
			return MakeDynamicGetIndex(boundDynamicIndexerAccess, boundDynamicIndexerAccess.Receiver, boundDynamicIndexerAccess.Arguments, boundDynamicIndexerAccess.ArgumentNamesOpt, boundDynamicIndexerAccess.ArgumentRefKindsOpt);
		}
		default:
			return transformedExpression;
		}
	}

	private TypeSymbol GetUnaryOperatorType(BoundIncrementOperator node)
	{
		UnaryOperatorKind unaryOperatorKind = node.OperatorKind.OperandTypes();
		SpecialType specialType;
		switch (unaryOperatorKind)
		{
		case UnaryOperatorKind.Enum:
			return node.Type;
		case UnaryOperatorKind.Int:
			specialType = SpecialType.System_Int32;
			break;
		case UnaryOperatorKind.SByte:
			specialType = SpecialType.System_SByte;
			break;
		case UnaryOperatorKind.Short:
			specialType = SpecialType.System_Int16;
			break;
		case UnaryOperatorKind.Byte:
			specialType = SpecialType.System_Byte;
			break;
		case UnaryOperatorKind.UShort:
			specialType = SpecialType.System_UInt16;
			break;
		case UnaryOperatorKind.Char:
			specialType = SpecialType.System_Char;
			break;
		case UnaryOperatorKind.UInt:
			specialType = SpecialType.System_UInt32;
			break;
		case UnaryOperatorKind.Long:
			specialType = SpecialType.System_Int64;
			break;
		case UnaryOperatorKind.ULong:
			specialType = SpecialType.System_UInt64;
			break;
		case UnaryOperatorKind.NInt:
			specialType = SpecialType.System_IntPtr;
			break;
		case UnaryOperatorKind.NUInt:
			specialType = SpecialType.System_UIntPtr;
			break;
		case UnaryOperatorKind.Float:
			specialType = SpecialType.System_Single;
			break;
		case UnaryOperatorKind.Double:
			specialType = SpecialType.System_Double;
			break;
		case UnaryOperatorKind.Decimal:
			specialType = SpecialType.System_Decimal;
			break;
		case UnaryOperatorKind.Pointer:
			return node.Type;
		default:
			throw ExceptionUtilities.UnexpectedValue(unaryOperatorKind);
		}
		NamedTypeSymbol namedTypeSymbol = _compilation.GetSpecialType(specialType);
		if (node.OperatorKind.IsLifted())
		{
			namedTypeSymbol = _compilation.GetSpecialType(SpecialType.System_Nullable_T).Construct(namedTypeSymbol);
		}
		return namedTypeSymbol;
	}

	private static BinaryOperatorKind GetCorrespondingBinaryOperator(BoundIncrementOperator node)
	{
		UnaryOperatorKind operatorKind = node.OperatorKind;
		BinaryOperatorKind binaryOperatorKind;
		switch (operatorKind.OperandTypes())
		{
		case UnaryOperatorKind.SByte:
		case UnaryOperatorKind.Short:
		case UnaryOperatorKind.Int:
			binaryOperatorKind = BinaryOperatorKind.Int;
			break;
		case UnaryOperatorKind.Byte:
		case UnaryOperatorKind.UShort:
		case UnaryOperatorKind.UInt:
		case UnaryOperatorKind.Char:
			binaryOperatorKind = BinaryOperatorKind.UInt;
			break;
		case UnaryOperatorKind.Long:
			binaryOperatorKind = BinaryOperatorKind.Long;
			break;
		case UnaryOperatorKind.ULong:
			binaryOperatorKind = BinaryOperatorKind.ULong;
			break;
		case UnaryOperatorKind.NInt:
			binaryOperatorKind = BinaryOperatorKind.NInt;
			break;
		case UnaryOperatorKind.NUInt:
			binaryOperatorKind = BinaryOperatorKind.NUInt;
			break;
		case UnaryOperatorKind.Float:
			binaryOperatorKind = BinaryOperatorKind.Float;
			break;
		case UnaryOperatorKind.Double:
			binaryOperatorKind = BinaryOperatorKind.Double;
			break;
		case UnaryOperatorKind.Decimal:
			binaryOperatorKind = BinaryOperatorKind.Decimal;
			break;
		case UnaryOperatorKind.Enum:
		{
			TypeSymbol type = node.Type;
			if (type.IsNullableType())
			{
				type = type.GetNullableUnderlyingType();
			}
			type = type.GetEnumUnderlyingType();
			switch (type.SpecialType)
			{
			case SpecialType.System_SByte:
			case SpecialType.System_Int16:
			case SpecialType.System_Int32:
				binaryOperatorKind = BinaryOperatorKind.Int;
				break;
			case SpecialType.System_Byte:
			case SpecialType.System_UInt16:
			case SpecialType.System_UInt32:
				binaryOperatorKind = BinaryOperatorKind.UInt;
				break;
			case SpecialType.System_Int64:
				binaryOperatorKind = BinaryOperatorKind.Long;
				break;
			case SpecialType.System_UInt64:
				binaryOperatorKind = BinaryOperatorKind.ULong;
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(type.SpecialType);
			}
			break;
		}
		case UnaryOperatorKind.Pointer:
			binaryOperatorKind = BinaryOperatorKind.PointerAndInt;
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(operatorKind.OperandTypes());
		}
		if ((uint)(binaryOperatorKind - 5) <= 5u || binaryOperatorKind == BinaryOperatorKind.PointerAndInt)
		{
			binaryOperatorKind = (BinaryOperatorKind)((int)binaryOperatorKind | (int)operatorKind.OverflowChecks());
		}
		if (operatorKind.IsLifted())
		{
			binaryOperatorKind |= BinaryOperatorKind.Lifted;
		}
		return binaryOperatorKind;
	}

	private static (TypeSymbol, ConstantValue) GetConstantOneForIncrement(CSharpCompilation compilation, BinaryOperatorKind binaryOperatorKind)
	{
		ConstantValue constantValue;
		switch (binaryOperatorKind.OperandTypes())
		{
		case BinaryOperatorKind.Int:
		case BinaryOperatorKind.PointerAndInt:
			constantValue = ConstantValue.Create(1);
			break;
		case BinaryOperatorKind.UInt:
			constantValue = ConstantValue.Create(1u);
			break;
		case BinaryOperatorKind.Long:
			constantValue = ConstantValue.Create(1L);
			break;
		case BinaryOperatorKind.ULong:
			constantValue = ConstantValue.Create(1uL);
			break;
		case BinaryOperatorKind.NInt:
			constantValue = ConstantValue.Create(1);
			return (compilation.CreateNativeIntegerTypeSymbol(signed: true), constantValue);
		case BinaryOperatorKind.NUInt:
			constantValue = ConstantValue.Create(1u);
			return (compilation.CreateNativeIntegerTypeSymbol(signed: false), constantValue);
		case BinaryOperatorKind.Float:
			constantValue = ConstantValue.Create(1f);
			break;
		case BinaryOperatorKind.Double:
			constantValue = ConstantValue.Create(1.0);
			break;
		case BinaryOperatorKind.Decimal:
			constantValue = ConstantValue.Create(1m);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(binaryOperatorKind.OperandTypes());
		}
		return (compilation.GetSpecialType(constantValue.SpecialType), constantValue);
	}

	public override BoundNode VisitUsingStatement(BoundUsingStatement node)
	{
		BoundStatement boundStatement = VisitStatement(node.Body);
		BoundBlock boundBlock = ((boundStatement.Kind == BoundKind.Block) ? ((BoundBlock)boundStatement) : BoundBlock.SynthesizedNoLocals(node.Syntax, boundStatement));
		if (node.ExpressionOpt != null)
		{
			return MakeExpressionUsingStatement(node, boundBlock);
		}
		SyntaxToken awaitKeyword = ((node.Syntax.Kind() == SyntaxKind.UsingStatement) ? ((UsingStatementSyntax)node.Syntax).AwaitKeyword : default(SyntaxToken));
		return MakeDeclarationUsingStatement(node.Syntax, boundBlock, node.Locals, node.DeclarationsOpt.LocalDeclarations, node.PatternDisposeInfoOpt, node.AwaitOpt, awaitKeyword);
	}

	private BoundStatement MakeDeclarationUsingStatement(SyntaxNode syntax, BoundBlock body, ImmutableArray<LocalSymbol> locals, ImmutableArray<BoundLocalDeclaration> declarations, MethodArgumentInfo? patternDisposeInfo, BoundAwaitableInfo? awaitOpt, SyntaxToken awaitKeyword)
	{
		BoundBlock boundBlock = body;
		for (int num = declarations.Length - 1; num >= 0; num--)
		{
			boundBlock = RewriteDeclarationUsingStatement(syntax, declarations[num], boundBlock, awaitKeyword, awaitOpt, patternDisposeInfo);
		}
		return new BoundBlock(syntax, locals, ImmutableArray.Create((BoundStatement)boundBlock));
	}

	private BoundStatement MakeLocalUsingDeclarationStatement(BoundUsingLocalDeclarations usingDeclarations, ImmutableArray<BoundStatement> statements)
	{
		LocalDeclarationStatementSyntax localDeclarationStatementSyntax = (LocalDeclarationStatementSyntax)usingDeclarations.Syntax;
		BoundBlock body = new BoundBlock(localDeclarationStatementSyntax, ImmutableArray<LocalSymbol>.Empty, statements);
		return MakeDeclarationUsingStatement(localDeclarationStatementSyntax, body, ImmutableArray<LocalSymbol>.Empty, usingDeclarations.LocalDeclarations, usingDeclarations.PatternDisposeInfoOpt, usingDeclarations.AwaitOpt, localDeclarationStatementSyntax.AwaitKeyword);
	}

	private BoundBlock MakeExpressionUsingStatement(BoundUsingStatement node, BoundBlock tryBlock)
	{
		BoundExpression boundExpression = VisitExpression(node.ExpressionOpt);
		if (boundExpression.ConstantValueOpt == ConstantValue.Null)
		{
			return tryBlock;
		}
		TypeSymbol? type = boundExpression.Type;
		SyntaxNode syntax = boundExpression.Syntax;
		UsingStatementSyntax usingStatementSyntax = (UsingStatementSyntax)node.Syntax;
		BoundLocal boundLocal;
		BoundAssignmentOperator store;
		if (type.IsDynamic())
		{
			TypeSymbol typeSymbol = ((node.AwaitOpt == null) ? _compilation.GetSpecialType(SpecialType.System_IDisposable) : _compilation.GetWellKnownType(WellKnownType.System_IAsyncDisposable));
			_diagnostics.ReportUseSite(typeSymbol, usingStatementSyntax);
			BoundExpression argument = MakeConversionNode(syntax, boundExpression, Conversion.ImplicitDynamic, typeSymbol, @checked: false, explicitCastInCode: false, boundExpression.ConstantValueOpt);
			boundLocal = _factory.StoreToTemp(argument, out store, RefKind.None, SynthesizedLocalKind.Using);
		}
		else
		{
			boundLocal = _factory.StoreToTemp(boundExpression, out store, RefKind.None, SynthesizedLocalKind.Using, isKnownToReferToTempIfReferenceType: false, usingStatementSyntax);
		}
		BoundStatement boundStatement = new BoundExpressionStatement(syntax, store);
		if (Instrument)
		{
			boundStatement = Instrumenter.InstrumentUsingTargetCapture(node, boundStatement);
		}
		BoundStatement item = RewriteUsingStatementTryFinally(usingStatementSyntax, usingStatementSyntax, tryBlock, boundLocal, usingStatementSyntax.AwaitKeyword, node.AwaitOpt, node.PatternDisposeInfoOpt);
		return new BoundBlock(usingStatementSyntax, node.Locals.Add(boundLocal.LocalSymbol), ImmutableArray.Create(boundStatement, item));
	}

	private BoundBlock RewriteDeclarationUsingStatement(SyntaxNode usingSyntax, BoundLocalDeclaration localDeclaration, BoundBlock tryBlock, SyntaxToken awaitKeywordOpt, BoundAwaitableInfo? awaitOpt, MethodArgumentInfo? patternDisposeInfo)
	{
		SyntaxNode syntax = localDeclaration.Syntax;
		LocalSymbol localSymbol = localDeclaration.LocalSymbol;
		TypeSymbol type = localSymbol.Type;
		BoundLocal boundLocal = new BoundLocal(syntax, localSymbol, localDeclaration.InitializerOpt.ConstantValueOpt, type);
		BoundStatement boundStatement = VisitStatement(localDeclaration);
		if (boundLocal.ConstantValueOpt == ConstantValue.Null)
		{
			return BoundBlock.SynthesizedNoLocals(syntax, boundStatement, tryBlock);
		}
		if (type.IsDynamic())
		{
			TypeSymbol typeSymbol = ((awaitOpt == null) ? _compilation.GetSpecialType(SpecialType.System_IDisposable) : _compilation.GetWellKnownType(WellKnownType.System_IAsyncDisposable));
			_diagnostics.ReportUseSite(typeSymbol, usingSyntax);
			BoundExpression argument = MakeConversionNode(syntax, boundLocal, Conversion.ImplicitDynamic, typeSymbol, @checked: false);
			BoundLocal boundLocal2 = _factory.StoreToTemp(argument, out BoundAssignmentOperator store, RefKind.None, SynthesizedLocalKind.Using);
			BoundStatement item = RewriteUsingStatementTryFinally(usingSyntax, syntax, tryBlock, boundLocal2, awaitKeywordOpt, awaitOpt, patternDisposeInfo);
			return new BoundBlock(syntax, ImmutableArray.Create(boundLocal2.LocalSymbol), ImmutableArray.Create(boundStatement, new BoundExpressionStatement(syntax, store), item));
		}
		BoundStatement boundStatement2 = RewriteUsingStatementTryFinally(usingSyntax, syntax, tryBlock, boundLocal, awaitKeywordOpt, awaitOpt, patternDisposeInfo);
		return BoundBlock.SynthesizedNoLocals(syntax, boundStatement, boundStatement2);
	}

	private BoundStatement RewriteUsingStatementTryFinally(SyntaxNode resourceTypeSyntax, SyntaxNode resourceSyntax, BoundBlock tryBlock, BoundLocal local, SyntaxToken awaitKeywordOpt, BoundAwaitableInfo? awaitOpt, MethodArgumentInfo? patternDisposeInfo)
	{
		bool num = local.Type.IsNullableType();
		BoundExpression disposedExpression;
		if (num)
		{
			MethodSymbol method = UnsafeGetNullableMethod(resourceTypeSyntax, local.Type, SpecialMember.System_Nullable_T_GetValueOrDefault);
			disposedExpression = BoundCall.Synthesized(resourceSyntax, local, ThreeState.Unknown, method);
		}
		else
		{
			disposedExpression = local;
		}
		BoundExpression expression = GenerateDisposeCall(resourceTypeSyntax, resourceSyntax, disposedExpression, patternDisposeInfo, awaitOpt, awaitKeywordOpt);
		BoundStatement boundStatement = new BoundExpressionStatement(resourceSyntax, expression);
		BoundExpression boundExpression = (num ? _factory.MakeNullableHasValue(resourceSyntax, local) : ((!local.Type.IsValueType) ? _factory.MakeNullCheck(resourceSyntax, local, BinaryOperatorKind.NotEqual) : null));
		return new BoundTryStatement(finallyBlockOpt: BoundBlock.SynthesizedNoLocals(resourceSyntax, (boundExpression != null) ? RewriteIfStatement(resourceSyntax, boundExpression, boundStatement, hasErrors: false) : boundStatement), syntax: resourceSyntax, tryBlock: tryBlock, catchBlocks: ImmutableArray<BoundCatchBlock>.Empty);
	}

	private BoundExpression GenerateDisposeCall(SyntaxNode resourceTypeSyntax, SyntaxNode resourceSyntax, BoundExpression disposedExpression, MethodArgumentInfo? disposeInfo, BoundAwaitableInfo? awaitOpt, SyntaxToken awaitKeyword)
	{
		MethodSymbol symbol = disposeInfo?.Method;
		if ((object)symbol == null)
		{
			if (awaitOpt == null)
			{
				Binder.TryGetSpecialTypeMember<MethodSymbol>(_compilation, SpecialMember.System_IDisposable__Dispose, resourceTypeSyntax, _diagnostics, out symbol);
			}
			else
			{
				TryGetWellKnownTypeMember<MethodSymbol>(null, WellKnownMember.System_IAsyncDisposable__DisposeAsync, out symbol, isOptional: false, awaitKeyword.GetLocation());
			}
		}
		BoundExpression boundExpression;
		if ((object)symbol == null)
		{
			boundExpression = new BoundBadExpression(resourceSyntax, LookupResultKind.NotInvocable, ImmutableArray<Symbol>.Empty, ImmutableArray.Create(disposedExpression), ErrorTypeSymbol.UnknownResultType);
		}
		else
		{
			if (disposeInfo == null)
			{
				disposeInfo = MethodArgumentInfo.CreateParameterlessMethod(symbol);
			}
			boundExpression = MakeCall(disposeInfo, resourceSyntax, disposedExpression, null);
			if (awaitOpt != null)
			{
				_sawAwaitInExceptionHandler = true;
				TypeSymbol type = awaitOpt.GetResult?.ReturnType ?? _compilation.DynamicType;
				boundExpression = RewriteAwaitExpression(resourceSyntax, boundExpression, awaitOpt, type, default(BoundAwaitExpressionDebugInfo), used: false);
			}
		}
		return boundExpression;
	}

	private BoundExpression MakeCall(MethodArgumentInfo methodArgumentInfo, SyntaxNode syntax, BoundExpression? expression, BoundExpression? firstRewrittenArgument)
	{
		MethodSymbol method = methodArgumentInfo.Method;
		ArrayBuilder<LocalSymbol> tempsOpt = null;
		ImmutableArray<RefKind> argumentRefKindsOpt = default(ImmutableArray<RefKind>);
		ImmutableArray<BoundExpression> rewrittenArguments = VisitArgumentsAndCaptureReceiverIfNeeded(ref expression, forceReceiverCapturing: false, methodArgumentInfo.Arguments, method, default(ImmutableArray<int>), argumentRefKindsOpt, null, ref tempsOpt, firstRewrittenArgument);
		rewrittenArguments = MakeArguments(rewrittenArguments, method, methodArgumentInfo.Expanded, default(ImmutableArray<int>), ref argumentRefKindsOpt, ref tempsOpt, method.IsExtensionMethod);
		return MakeCall(null, syntax, expression, method, rewrittenArguments, argumentRefKindsOpt, LookupResultKind.Viable, tempsOpt.ToImmutableAndFree());
	}

	public override BoundNode VisitWhileStatement(BoundWhileStatement node)
	{
		BoundExpression rewrittenCondition = VisitExpression(node.Condition);
		BoundStatement rewrittenBody = VisitStatement(node.Body);
		if (!node.WasCompilerGenerated && Instrument)
		{
			rewrittenCondition = Instrumenter.InstrumentWhileStatementCondition(node, rewrittenCondition, _factory);
		}
		return RewriteWhileStatement(node, node.Locals, rewrittenCondition, rewrittenBody, node.BreakLabel, node.ContinueLabel, node.HasErrors);
	}

	private BoundStatement RewriteWhileStatement(BoundNode loop, BoundExpression rewrittenCondition, BoundStatement rewrittenBody, LabelSymbol breakLabel, LabelSymbol continueLabel, bool hasErrors)
	{
		SyntaxNode syntax = loop.Syntax;
		GeneratedLabelSymbol label = new GeneratedLabelSymbol("start");
		BoundStatement boundStatement = new BoundConditionalGoto(rewrittenCondition.Syntax, rewrittenCondition, jumpIfTrue: true, label);
		BoundStatement boundStatement2 = new BoundGotoStatement(syntax, continueLabel);
		if (Instrument && !loop.WasCompilerGenerated)
		{
			switch (loop.Kind)
			{
			case BoundKind.WhileStatement:
				boundStatement = Instrumenter.InstrumentWhileStatementConditionalGotoStartOrBreak((BoundWhileStatement)loop, boundStatement);
				break;
			case BoundKind.ForEachStatement:
				boundStatement = Instrumenter.InstrumentForEachStatementConditionalGotoStart((BoundForEachStatement)loop, boundStatement);
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(loop.Kind);
			case BoundKind.CollectionExpressionSpreadElement:
				break;
			}
			boundStatement2 = BoundSequencePoint.CreateHidden(boundStatement2);
		}
		return BoundStatementList.Synthesized(syntax, hasErrors, boundStatement2, new BoundLabelStatement(syntax, label), rewrittenBody, new BoundLabelStatement(syntax, continueLabel), boundStatement, new BoundLabelStatement(syntax, breakLabel));
	}

	private BoundStatement RewriteWhileStatement(BoundWhileStatement loop, ImmutableArray<LocalSymbol> locals, BoundExpression rewrittenCondition, BoundStatement rewrittenBody, LabelSymbol breakLabel, LabelSymbol continueLabel, bool hasErrors)
	{
		if (locals.IsEmpty)
		{
			return RewriteWhileStatement(loop, rewrittenCondition, rewrittenBody, breakLabel, continueLabel, hasErrors);
		}
		SyntaxNode syntax = loop.Syntax;
		BoundStatement boundStatement = new BoundLabelStatement(syntax, continueLabel);
		BoundStatement boundStatement2 = new BoundConditionalGoto(rewrittenCondition.Syntax, rewrittenCondition, jumpIfTrue: false, breakLabel);
		if (Instrument && !loop.WasCompilerGenerated)
		{
			boundStatement2 = Instrumenter.InstrumentWhileStatementConditionalGotoStartOrBreak(loop, boundStatement2);
			boundStatement = BoundSequencePoint.CreateHidden(boundStatement);
		}
		return BoundStatementList.Synthesized(syntax, hasErrors, boundStatement, new BoundBlock(syntax, locals, ImmutableArray.Create(boundStatement2, rewrittenBody, new BoundGotoStatement(syntax, continueLabel))), new BoundLabelStatement(syntax, breakLabel));
	}

	public override BoundNode VisitYieldBreakStatement(BoundYieldBreakStatement node)
	{
		BoundStatement boundStatement = (BoundStatement)base.VisitYieldBreakStatement(node);
		if (Instrument)
		{
			if (!node.WasCompilerGenerated)
			{
				goto IL_004b;
			}
			if (node.Syntax.Kind() == SyntaxKind.Block)
			{
				MethodSymbol? currentFunction = _factory.CurrentFunction;
				if ((object)currentFunction != null && !currentFunction.IsAsync)
				{
					goto IL_004b;
				}
			}
		}
		goto IL_0059;
		IL_004b:
		boundStatement = Instrumenter.InstrumentYieldBreakStatement(node, boundStatement);
		goto IL_0059;
		IL_0059:
		return boundStatement;
	}

	public override BoundNode VisitYieldReturnStatement(BoundYieldReturnStatement node)
	{
		BoundStatement boundStatement = (BoundStatement)base.VisitYieldReturnStatement(node);
		if (Instrument && !node.WasCompilerGenerated)
		{
			boundStatement = Instrumenter.InstrumentYieldReturnStatement(node, boundStatement);
		}
		return boundStatement;
	}
}
