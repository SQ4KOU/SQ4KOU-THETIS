using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class SpillSequenceSpiller : BoundTreeRewriterWithStackGuard
{
	private sealed class BoundSpillSequenceBuilder : BoundExpression
	{
		public readonly BoundExpression Value;

		private ArrayBuilder<LocalSymbol> _locals;

		private ArrayBuilder<BoundStatement> _statements;

		public bool HasStatements => _statements != null;

		public bool HasLocals => _locals != null;

		public BoundSpillSequenceBuilder(SyntaxNode syntax, BoundExpression value = null)
			: base((BoundKind)byte.MaxValue, syntax, value?.Type)
		{
			Value = value;
		}

		public ImmutableArray<LocalSymbol> GetLocals()
		{
			if (_locals != null)
			{
				return _locals.ToImmutable();
			}
			return ImmutableArray<LocalSymbol>.Empty;
		}

		public ImmutableArray<BoundStatement> GetStatements()
		{
			if (_statements == null)
			{
				return ImmutableArray<BoundStatement>.Empty;
			}
			return _statements.ToImmutable();
		}

		internal BoundSpillSequenceBuilder Update(BoundExpression value)
		{
			return new BoundSpillSequenceBuilder(Syntax, value)
			{
				_locals = _locals,
				_statements = _statements
			};
		}

		public void Free()
		{
			if (_locals != null)
			{
				_locals.Free();
			}
			if (_statements != null)
			{
				_statements.Free();
			}
		}

		internal void Include(BoundSpillSequenceBuilder other)
		{
			if (other != null)
			{
				IncludeAndFree(ref _locals, ref other._locals);
				IncludeAndFree(ref _statements, ref other._statements);
			}
		}

		private static void IncludeAndFree<T>(ref ArrayBuilder<T> left, ref ArrayBuilder<T> right)
		{
			if (right != null)
			{
				if (left == null)
				{
					left = right;
					return;
				}
				left.AddRange(right);
				right.Free();
			}
		}

		public void AddLocal(LocalSymbol local)
		{
			if (_locals == null)
			{
				_locals = ArrayBuilder<LocalSymbol>.GetInstance();
			}
			_locals.Add(local);
		}

		public void AddLocals(ImmutableArray<LocalSymbol> locals)
		{
			foreach (LocalSymbol item in locals)
			{
				AddLocal(item);
			}
		}

		public void AddStatement(BoundStatement statement)
		{
			if (_statements == null)
			{
				_statements = ArrayBuilder<BoundStatement>.GetInstance();
			}
			_statements.Add(statement);
		}

		public void AddStatements(ImmutableArray<BoundStatement> statements)
		{
			foreach (BoundStatement item in statements)
			{
				AddStatement(item);
			}
		}

		internal void AddExpressions(ImmutableArray<BoundExpression> expressions)
		{
			foreach (BoundExpression item in expressions)
			{
				AddStatement(new BoundExpressionStatement(item.Syntax, item)
				{
					WasCompilerGenerated = true
				});
			}
		}
	}

	private sealed class LocalSubstituter : BoundTreeRewriterWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator
	{
		private readonly PooledDictionary<LocalSymbol, LocalSymbol> _tempSubstitution;

		private readonly PooledDictionary<LocalSymbol, BoundComplexConditionalReceiver> _receiverSubstitution;

		private LocalSubstituter(PooledDictionary<LocalSymbol, LocalSymbol> tempSubstitution, PooledDictionary<LocalSymbol, BoundComplexConditionalReceiver> receiverSubstitution, int recursionDepth = 0)
			: base(recursionDepth)
		{
			_tempSubstitution = tempSubstitution;
			_receiverSubstitution = receiverSubstitution;
		}

		public static BoundNode Rewrite(PooledDictionary<LocalSymbol, LocalSymbol> tempSubstitution, PooledDictionary<LocalSymbol, BoundComplexConditionalReceiver> receiverSubstitution, BoundNode node)
		{
			if (tempSubstitution.Count == 0)
			{
				return node;
			}
			return new LocalSubstituter(tempSubstitution, receiverSubstitution).Visit(node);
		}

		public override BoundNode VisitLocal(BoundLocal node)
		{
			if (!node.LocalSymbol.SynthesizedKind.IsLongLived())
			{
				if (_tempSubstitution.TryGetValue(node.LocalSymbol, out var value))
				{
					return node.Update(value, node.ConstantValueOpt, node.Type);
				}
				if (_receiverSubstitution.TryGetValue(node.LocalSymbol, out var value2))
				{
					return Visit(value2);
				}
			}
			return base.VisitLocal(node);
		}
	}

	private sealed class ConditionalReceiverReplacer : BoundTreeRewriterWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator
	{
		private readonly BoundExpression _receiver;

		private readonly int _receiverId;

		private ConditionalReceiverReplacer(BoundExpression receiver, int receiverId, int recursionDepth)
			: base(recursionDepth)
		{
			_receiver = receiver;
			_receiverId = receiverId;
		}

		public static BoundStatement Replace(BoundNode node, BoundExpression receiver, int receiverID, int recursionDepth)
		{
			return (BoundStatement)new ConditionalReceiverReplacer(receiver, receiverID, recursionDepth).Visit(node);
		}

		public override BoundNode VisitConditionalReceiver(BoundConditionalReceiver node)
		{
			if (node.Id == _receiverId)
			{
				return _receiver;
			}
			return node;
		}
	}

	private const BoundKind SpillSequenceBuilderKind = (BoundKind)byte.MaxValue;

	private readonly SyntheticBoundNodeFactory _F;

	private readonly PooledDictionary<LocalSymbol, LocalSymbol> _tempSubstitution;

	private readonly PooledDictionary<LocalSymbol, BoundComplexConditionalReceiver> _receiverSubstitution;

	private SpillSequenceSpiller(MethodSymbol method, SyntaxNode syntaxNode, TypeCompilationState compilationState, PooledDictionary<LocalSymbol, LocalSymbol> tempSubstitution, PooledDictionary<LocalSymbol, BoundComplexConditionalReceiver> receiverSubstitution, BindingDiagnosticBag diagnostics)
	{
		_F = new SyntheticBoundNodeFactory(method, syntaxNode, compilationState, diagnostics);
		_F.CurrentFunction = method;
		_tempSubstitution = tempSubstitution;
		_receiverSubstitution = receiverSubstitution;
	}

	internal static BoundStatement Rewrite(BoundStatement body, MethodSymbol method, TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		PooledDictionary<LocalSymbol, LocalSymbol> instance = PooledDictionary<LocalSymbol, LocalSymbol>.GetInstance();
		PooledDictionary<LocalSymbol, BoundComplexConditionalReceiver> instance2 = PooledDictionary<LocalSymbol, BoundComplexConditionalReceiver>.GetInstance();
		BoundNode node = new SpillSequenceSpiller(method, body.Syntax, compilationState, instance, instance2, diagnostics).Visit(body);
		node = LocalSubstituter.Rewrite(instance, instance2, node);
		instance.Free();
		instance2.Free();
		return (BoundStatement)node;
	}

	private BoundExpression VisitExpression(ref BoundSpillSequenceBuilder builder, BoundExpression expression)
	{
		BoundExpression boundExpression = (BoundExpression)Visit(expression);
		if (boundExpression == null || boundExpression.Kind != (BoundKind)byte.MaxValue)
		{
			return boundExpression;
		}
		BoundSpillSequenceBuilder boundSpillSequenceBuilder = (BoundSpillSequenceBuilder)boundExpression;
		if (builder == null)
		{
			builder = boundSpillSequenceBuilder.Update(null);
		}
		else
		{
			builder.Include(boundSpillSequenceBuilder);
		}
		return boundSpillSequenceBuilder.Value;
	}

	private static BoundExpression UpdateExpression(BoundSpillSequenceBuilder builder, BoundExpression expression)
	{
		if (builder == null)
		{
			return expression;
		}
		if (!builder.HasLocals && !builder.HasStatements)
		{
			builder.Free();
			return expression;
		}
		return builder.Update(expression);
	}

	private BoundStatement UpdateStatement(BoundSpillSequenceBuilder builder, BoundStatement statement)
	{
		if (builder == null)
		{
			return statement;
		}
		if (statement != null)
		{
			builder.AddStatement(statement);
		}
		BoundBlock result = new BoundBlock(statement.Syntax, builder.GetLocals(), builder.GetStatements())
		{
			WasCompilerGenerated = true
		};
		builder.Free();
		return result;
	}

	private BoundExpression Spill(BoundSpillSequenceBuilder builder, BoundExpression expression, RefKind refKind = RefKind.None, bool sideEffectsOnly = false)
	{
		if (builder.Syntax != null)
		{
			_F.Syntax = builder.Syntax;
		}
		while (true)
		{
			switch (expression.Kind)
			{
			case BoundKind.ArrayInitialization:
			{
				BoundArrayInitialization boundArrayInitialization = (BoundArrayInitialization)expression;
				ImmutableArray<BoundExpression> initializers = VisitExpressionList(ref builder, boundArrayInitialization.Initializers, default(ImmutableArray<RefKind>), forceSpill: true);
				return boundArrayInitialization.Update(initializers);
			}
			case BoundKind.ArgListOperator:
			{
				BoundArgListOperator boundArgListOperator = (BoundArgListOperator)expression;
				ImmutableArray<BoundExpression> arguments = VisitExpressionList(ref builder, boundArgListOperator.Arguments, boundArgListOperator.ArgumentRefKindsOpt, forceSpill: true);
				return boundArgListOperator.Update(arguments, boundArgListOperator.ArgumentRefKindsOpt, boundArgListOperator.Type);
			}
			case (BoundKind)byte.MaxValue:
			{
				BoundSpillSequenceBuilder boundSpillSequenceBuilder = (BoundSpillSequenceBuilder)expression;
				builder.Include(boundSpillSequenceBuilder);
				expression = boundSpillSequenceBuilder.Value;
				continue;
			}
			case BoundKind.Sequence:
			{
				if (refKind == RefKind.None)
				{
					TypeSymbol? type = expression.Type;
					if ((object)type == null || !type.IsRefLikeOrAllowsRefLikeType())
					{
						break;
					}
				}
				BoundSequence boundSequence = (BoundSequence)expression;
				PromoteAndAddLocals(builder, boundSequence.Locals);
				builder.AddExpressions(boundSequence.SideEffects);
				expression = boundSequence.Value;
				continue;
			}
			case BoundKind.AssignmentOperator:
			{
				BoundAssignmentOperator boundAssignmentOperator = (BoundAssignmentOperator)expression;
				if (!boundAssignmentOperator.IsRef)
				{
					break;
				}
				if (boundAssignmentOperator != null)
				{
					BoundExpression left = boundAssignmentOperator.Left;
					if (left != null && left.Kind == BoundKind.Local)
					{
						BoundExpression right = boundAssignmentOperator.Right;
						if (right != null && right.Kind == BoundKind.ArrayAccess)
						{
							break;
						}
					}
				}
				if (sideEffectsOnly && IsComplexConditionalInitializationOfReceiverRef(boundAssignmentOperator, out var outReceiverRefLocal, out var outComplexReceiver, out var outValueTypeReceiver, out var outReferenceTypeReceiver))
				{
					builder.AddStatement(_F.ExpressionStatement(outComplexReceiver));
					_receiverSubstitution.Add(outReceiverRefLocal, outComplexReceiver.Update(outValueTypeReceiver, outReferenceTypeReceiver, outComplexReceiver.Type));
					return null;
				}
				BoundExpression left2 = Spill(builder, boundAssignmentOperator.Left, RefKind.Ref);
				BoundExpression right2 = Spill(builder, boundAssignmentOperator.Right, RefKind.Ref);
				expression = boundAssignmentOperator.Update(left2, right2, boundAssignmentOperator.IsRef, boundAssignmentOperator.Type);
				break;
			}
			case BoundKind.ThisReference:
			case BoundKind.BaseReference:
				if (refKind != RefKind.None || expression.Type.IsReferenceType)
				{
					return expression;
				}
				break;
			case BoundKind.Parameter:
				if (refKind != RefKind.None)
				{
					return expression;
				}
				break;
			case BoundKind.Local:
			{
				BoundLocal boundLocal = (BoundLocal)expression;
				if (boundLocal.LocalSymbol.SynthesizedKind == SynthesizedLocalKind.Spill || refKind != RefKind.None)
				{
					return boundLocal;
				}
				break;
			}
			case BoundKind.FieldAccess:
			{
				BoundFieldAccess boundFieldAccess = (BoundFieldAccess)expression;
				FieldSymbol fieldSymbol = boundFieldAccess.FieldSymbol;
				if (fieldSymbol.IsStatic)
				{
					if (refKind != RefKind.None || fieldSymbol.IsReadOnly)
					{
						return boundFieldAccess;
					}
				}
				else if (refKind != RefKind.None)
				{
					BoundExpression receiver = Spill(builder, boundFieldAccess.ReceiverOpt, fieldSymbol.ContainingType.IsValueType ? refKind : RefKind.None);
					return boundFieldAccess.Update(receiver, fieldSymbol, boundFieldAccess.ConstantValueOpt, boundFieldAccess.ResultKind, boundFieldAccess.Type);
				}
				break;
			}
			case BoundKind.TypeExpression:
			case BoundKind.Literal:
				return expression;
			case BoundKind.ConditionalReceiver:
				return expression;
			case BoundKind.Call:
			{
				BoundCall boundCall = (BoundCall)expression;
				if (refKind != RefKind.None)
				{
					MethodSymbol originalDefinition = boundCall.Method.OriginalDefinition;
					if ((originalDefinition is SynthesizedInlineArrayFirstElementRefMethod || originalDefinition is SynthesizedInlineArrayFirstElementRefReadOnlyMethod) ? true : false)
					{
						return boundCall.Update(ImmutableArray.Create(Spill(builder, boundCall.Arguments[0], boundCall.ArgumentRefKindsOpt[0])));
					}
					originalDefinition = boundCall.Method.OriginalDefinition;
					if ((originalDefinition is SynthesizedInlineArrayElementRefMethod || originalDefinition is SynthesizedInlineArrayElementRefReadOnlyMethod) ? true : false)
					{
						return spillInlineArrayHelperWithTwoArguments(builder, boundCall);
					}
					if (boundCall.Method.OriginalDefinition == _F.Compilation.GetWellKnownTypeMember(WellKnownMember.System_Span_T__get_Item) || boundCall.Method.OriginalDefinition == _F.Compilation.GetWellKnownTypeMember(WellKnownMember.System_ReadOnlySpan_T__get_Item))
					{
						return boundCall.Update(Spill(builder, boundCall.ReceiverOpt, ReceiverSpillRefKind(boundCall.ReceiverOpt)), ThreeState.Unknown, boundCall.Method, ImmutableArray.Create(Spill(builder, boundCall.Arguments[0])));
					}
				}
				else
				{
					MethodSymbol originalDefinition = boundCall.Method.OriginalDefinition;
					if ((originalDefinition is SynthesizedInlineArrayAsSpanMethod || originalDefinition is SynthesizedInlineArrayAsReadOnlySpanMethod) ? true : false)
					{
						return spillInlineArrayHelperWithTwoArguments(builder, boundCall);
					}
					if (boundCall.Method.OriginalDefinition == _F.Compilation.GetWellKnownTypeMember(WellKnownMember.System_Span_T__Slice_Int_Int) || boundCall.Method.OriginalDefinition == _F.Compilation.GetWellKnownTypeMember(WellKnownMember.System_ReadOnlySpan_T__Slice_Int_Int))
					{
						return boundCall.Update(Spill(builder, boundCall.ReceiverOpt, ReceiverSpillRefKind(boundCall.ReceiverOpt)), ThreeState.Unknown, boundCall.Method, ImmutableArray.Create(Spill(builder, boundCall.Arguments[0]), Spill(builder, boundCall.Arguments[1])));
					}
					if (boundCall.Method == _F.Compilation.GetSpecialTypeMember(SpecialMember.System_String__op_Implicit_ToReadOnlySpanOfChar))
					{
						return boundCall.Update(ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1] { Spill(builder, boundCall.Arguments[0]) }));
					}
				}
				break;
			}
			case BoundKind.ObjectCreationExpression:
			{
				BoundObjectCreationExpression boundObjectCreationExpression = (BoundObjectCreationExpression)expression;
				if (refKind == RefKind.None && boundObjectCreationExpression.InitializerExpressionOpt == null && boundObjectCreationExpression.Constructor.OriginalDefinition == _F.Compilation.GetSpecialTypeMember(SpecialMember.System_ReadOnlySpan_T__ctor_Reference))
				{
					ImmutableArray<RefKind> argumentRefKindsOpt = boundObjectCreationExpression.ArgumentRefKindsOpt;
					return boundObjectCreationExpression.Update(boundObjectCreationExpression.Constructor, ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1] { Spill(builder, boundObjectCreationExpression.Arguments[0], (!argumentRefKindsOpt.IsDefault) ? argumentRefKindsOpt[0] : RefKind.None) }), boundObjectCreationExpression.ArgumentRefKindsOpt, null);
				}
				break;
			}
			}
			break;
		}
		if (expression.Type.IsVoidType() | sideEffectsOnly)
		{
			builder.AddStatement(_F.ExpressionStatement(expression));
			return null;
		}
		BoundLocal boundLocal2 = _F.StoreToTemp(expression, out BoundAssignmentOperator store, refKind, SynthesizedLocalKind.Spill, isKnownToReferToTempIfReferenceType: false, _F.Syntax);
		builder.AddLocal(boundLocal2.LocalSymbol);
		builder.AddStatement(_F.ExpressionStatement(store));
		return boundLocal2;
		BoundExpression spillInlineArrayHelperWithTwoArguments(BoundSpillSequenceBuilder builder2, BoundCall call)
		{
			return call.Update(ImmutableArray.Create(Spill(builder2, call.Arguments[0], call.ArgumentRefKindsOpt[0]), ((object)call.Arguments[1].ConstantValueOpt != null) ? call.Arguments[1] : Spill(builder2, call.Arguments[1])));
		}
	}

	internal static bool IsComplexConditionalInitializationOfReceiverRef(BoundAssignmentOperator assignment, out LocalSymbol outReceiverRefLocal, out BoundComplexConditionalReceiver outComplexReceiver, out BoundLocal outValueTypeReceiver, out BoundLocal outReferenceTypeReceiver)
	{
		if (assignment != null && assignment.IsRef && assignment.Left is BoundLocal boundLocal)
		{
			LocalSymbol localSymbol = boundLocal.LocalSymbol;
			if ((object)localSymbol != null && localSymbol.SynthesizedKind == SynthesizedLocalKind.LoweringTemp && localSymbol.RefKind != RefKind.None && assignment.Right is BoundComplexConditionalReceiver { ValueTypeReceiver: BoundLocal valueTypeReceiver } boundComplexConditionalReceiver)
			{
				LocalSymbol localSymbol2 = valueTypeReceiver.LocalSymbol;
				if ((object)localSymbol2 != null && localSymbol2.SynthesizedKind == SynthesizedLocalKind.LoweringTemp && localSymbol2.RefKind != RefKind.None && boundComplexConditionalReceiver.ReferenceTypeReceiver is BoundSequence boundSequence && boundSequence.Locals.IsEmpty)
				{
					ImmutableArray<BoundExpression> sideEffects = boundSequence.SideEffects;
					if (sideEffects.Length == 1 && sideEffects[0] is BoundAssignmentOperator { IsRef: false, Left: BoundLocal left } boundAssignmentOperator)
					{
						LocalSymbol localSymbol3 = left.LocalSymbol;
						if ((object)localSymbol3 != null && localSymbol3.SynthesizedKind == SynthesizedLocalKind.LoweringTemp && localSymbol3.RefKind == RefKind.None && boundAssignmentOperator.Right is BoundLocal boundLocal2)
						{
							LocalSymbol localSymbol4 = boundLocal2.LocalSymbol;
							if ((object)localSymbol4 != null && localSymbol4.SynthesizedKind == SynthesizedLocalKind.LoweringTemp && localSymbol4.RefKind != RefKind.None && boundSequence.Value is BoundLocal boundLocal3)
							{
								LocalSymbol localSymbol5 = boundLocal3.LocalSymbol;
								if ((object)localSymbol5 != null && localSymbol5.SynthesizedKind == SynthesizedLocalKind.LoweringTemp && localSymbol5.RefKind == RefKind.None && (object)localSymbol3 == boundLocal3.LocalSymbol && (object)localSymbol4 == valueTypeReceiver.LocalSymbol && (object)localSymbol != valueTypeReceiver.LocalSymbol && (object)localSymbol != localSymbol3 && localSymbol.Type.IsTypeParameter() && !localSymbol.Type.IsReferenceType && !localSymbol.Type.IsValueType && valueTypeReceiver.Type.Equals(localSymbol.Type, TypeCompareKind.AllIgnoreOptions) && localSymbol.RefKind == valueTypeReceiver.LocalSymbol.RefKind && boundLocal3.Type.Equals(localSymbol.Type, TypeCompareKind.AllIgnoreOptions))
								{
									outReceiverRefLocal = localSymbol;
									outComplexReceiver = boundComplexConditionalReceiver;
									outValueTypeReceiver = valueTypeReceiver;
									outReferenceTypeReceiver = boundLocal3;
									return true;
								}
							}
						}
					}
				}
			}
		}
		outReceiverRefLocal = null;
		outComplexReceiver = null;
		outValueTypeReceiver = null;
		outReferenceTypeReceiver = null;
		return false;
	}

	private ImmutableArray<BoundExpression> VisitExpressionList(ref BoundSpillSequenceBuilder builder, ImmutableArray<BoundExpression> args, ImmutableArray<RefKind> refKinds = default(ImmutableArray<RefKind>), bool forceSpill = false, bool sideEffectsOnly = false)
	{
		if (args.Length == 0)
		{
			return args;
		}
		ImmutableArray<BoundExpression> result = VisitList(args);
		int num;
		if (forceSpill)
		{
			num = result.Length;
		}
		else
		{
			num = -1;
			for (int num2 = result.Length - 1; num2 >= 0; num2--)
			{
				if (result[num2].Kind == (BoundKind)byte.MaxValue)
				{
					num = num2;
					break;
				}
			}
		}
		if (num == -1)
		{
			return result;
		}
		if (builder == null)
		{
			builder = new BoundSpillSequenceBuilder((num >= result.Length) ? null : (result[num] as BoundSpillSequenceBuilder)?.Syntax);
		}
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(result.Length);
		for (int i = 0; i < num; i++)
		{
			RefKind refKind = ((!refKinds.IsDefault) ? refKinds[i] : RefKind.None);
			BoundExpression item = Spill(builder, result[i], refKind, sideEffectsOnly);
			if (!sideEffectsOnly)
			{
				instance.Add(item);
			}
		}
		if (num < result.Length)
		{
			BoundSpillSequenceBuilder boundSpillSequenceBuilder = (BoundSpillSequenceBuilder)result[num];
			builder.Include(boundSpillSequenceBuilder);
			instance.Add(boundSpillSequenceBuilder.Value);
			for (int j = num + 1; j < result.Length; j++)
			{
				instance.Add(result[j]);
			}
		}
		return instance.ToImmutableAndFree();
	}

	public override BoundNode VisitSwitchDispatch(BoundSwitchDispatch node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression expression = VisitExpression(ref builder, node.Expression);
		return UpdateStatement(builder, node.Update(expression, node.Cases, node.DefaultLabel, node.LengthBasedStringSwitchDataOpt));
	}

	public override BoundNode VisitThrowStatement(BoundThrowStatement node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression expressionOpt = VisitExpression(ref builder, node.ExpressionOpt);
		return UpdateStatement(builder, node.Update(expressionOpt));
	}

	public override BoundNode VisitExpressionStatement(BoundExpressionStatement node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression expression = VisitExpression(ref builder, node.Expression);
		return UpdateStatement(builder, node.Update(expression));
	}

	public override BoundNode VisitConditionalGoto(BoundConditionalGoto node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression condition = VisitExpression(ref builder, node.Condition);
		return UpdateStatement(builder, node.Update(condition, node.JumpIfTrue, node.Label));
	}

	public override BoundNode VisitReturnStatement(BoundReturnStatement node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression expressionOpt = VisitExpression(ref builder, node.ExpressionOpt);
		return UpdateStatement(builder, node.Update(node.RefKind, expressionOpt, node.Checked));
	}

	public override BoundNode VisitYieldReturnStatement(BoundYieldReturnStatement node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression expression = VisitExpression(ref builder, node.Expression);
		return UpdateStatement(builder, node.Update(expression));
	}

	public override BoundNode VisitCatchBlock(BoundCatchBlock node)
	{
		BoundExpression exceptionSourceOpt = (BoundExpression)Visit(node.ExceptionSourceOpt);
		ImmutableArray<LocalSymbol> locals = node.Locals;
		BoundStatementList boundStatementList = node.ExceptionFilterPrologueOpt;
		if (boundStatementList != null)
		{
			boundStatementList = (BoundStatementList)VisitStatementList(boundStatementList);
		}
		BoundSpillSequenceBuilder builder = null;
		BoundExpression exceptionFilterOpt = VisitExpression(ref builder, node.ExceptionFilterOpt);
		if (builder != null)
		{
			locals = locals.AddRange(builder.GetLocals());
			boundStatementList = new BoundStatementList(node.Syntax, builder.GetStatements());
		}
		BoundBlock body = (BoundBlock)Visit(node.Body);
		TypeSymbol exceptionTypeOpt = VisitType(node.ExceptionTypeOpt);
		return node.Update(locals, exceptionSourceOpt, exceptionTypeOpt, boundStatementList, exceptionFilterOpt, body, node.IsSynthesizedAsyncCatchAll);
	}

	public override BoundNode VisitAwaitExpression(BoundAwaitExpression node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression expression = VisitExpression(ref builder, node.Expression);
		return UpdateExpression(builder, node.Update(expression, node.AwaitableInfo, node.DebugInfo, node.Type));
	}

	public override BoundNode VisitSpillSequence(BoundSpillSequence node)
	{
		BoundSpillSequenceBuilder builder = new BoundSpillSequenceBuilder(node.Syntax);
		_F.Syntax = node.Syntax;
		builder.AddStatements(VisitList(node.SideEffects));
		builder.AddLocals(node.Locals);
		BoundExpression value = VisitExpression(ref builder, node.Value);
		return builder.Update(value);
	}

	public override BoundNode VisitAddressOfOperator(BoundAddressOfOperator node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression operand = VisitExpression(ref builder, node.Operand);
		return UpdateExpression(builder, node.Update(operand, node.IsManaged, node.Type));
	}

	public override BoundNode VisitArgListOperator(BoundArgListOperator node)
	{
		BoundSpillSequenceBuilder builder = null;
		ImmutableArray<BoundExpression> arguments = VisitExpressionList(ref builder, node.Arguments);
		return UpdateExpression(builder, node.Update(arguments, node.ArgumentRefKindsOpt, node.Type));
	}

	public override BoundNode VisitArrayAccess(BoundArrayAccess node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression expression = VisitExpression(ref builder, node.Expression);
		BoundSpillSequenceBuilder builder2 = null;
		ImmutableArray<BoundExpression> indices = VisitExpressionList(ref builder2, node.Indices);
		if (builder2 != null)
		{
			if (builder == null)
			{
				builder = new BoundSpillSequenceBuilder(builder2.Syntax);
			}
			expression = Spill(builder, expression);
		}
		if (builder != null)
		{
			builder.Include(builder2);
			builder2 = builder;
			builder = null;
		}
		return UpdateExpression(builder2, node.Update(expression, indices, node.Type));
	}

	public override BoundNode VisitArrayCreation(BoundArrayCreation node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundArrayInitialization initializerOpt = (BoundArrayInitialization)VisitExpression(ref builder, node.InitializerOpt);
		ImmutableArray<BoundExpression> bounds;
		if (builder == null)
		{
			bounds = VisitExpressionList(ref builder, node.Bounds);
		}
		else
		{
			BoundSpillSequenceBuilder builder2 = new BoundSpillSequenceBuilder(builder.Syntax);
			bounds = VisitExpressionList(ref builder2, node.Bounds, default(ImmutableArray<RefKind>), forceSpill: true);
			builder2.Include(builder);
			builder = builder2;
		}
		return UpdateExpression(builder, node.Update(bounds, initializerOpt, node.Type));
	}

	public override BoundNode VisitArrayInitialization(BoundArrayInitialization node)
	{
		BoundSpillSequenceBuilder builder = null;
		ImmutableArray<BoundExpression> initializers = VisitExpressionList(ref builder, node.Initializers);
		return UpdateExpression(builder, node.Update(initializers));
	}

	public override BoundNode VisitConvertedStackAllocExpression(BoundConvertedStackAllocExpression node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression count = VisitExpression(ref builder, node.Count);
		BoundArrayInitialization initializerOpt = (BoundArrayInitialization)VisitExpression(ref builder, node.InitializerOpt);
		return UpdateExpression(builder, node.Update(node.ElementType, count, initializerOpt, node.Type));
	}

	public override BoundNode VisitArrayLength(BoundArrayLength node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression expression = VisitExpression(ref builder, node.Expression);
		return UpdateExpression(builder, node.Update(expression, node.Type));
	}

	public override BoundNode VisitAsOperator(BoundAsOperator node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression operand = VisitExpression(ref builder, node.Operand);
		return UpdateExpression(builder, node.Update(operand, node.TargetType, node.OperandPlaceholder, node.OperandConversion, node.Type));
	}

	public override BoundNode VisitAssignmentOperator(BoundAssignmentOperator node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression right = VisitExpression(ref builder, node.Right);
		BoundExpression boundExpression = node.Left;
		if (builder == null)
		{
			boundExpression = VisitExpression(ref builder, boundExpression);
		}
		else
		{
			BoundSpillSequenceBuilder leftBuilder = new BoundSpillSequenceBuilder(builder.Syntax);
			switch (boundExpression.Kind)
			{
			case BoundKind.FieldAccess:
			{
				BoundFieldAccess boundFieldAccess = (BoundFieldAccess)boundExpression;
				if (!boundFieldAccess.FieldSymbol.IsStatic)
				{
					boundExpression = fieldWithSpilledReceiver(boundFieldAccess, ref leftBuilder, isAssignmentTarget: true);
				}
				break;
			}
			case BoundKind.ArrayAccess:
			{
				BoundArrayAccess boundArrayAccess = (BoundArrayAccess)boundExpression;
				BoundExpression expression = VisitExpression(ref leftBuilder, boundArrayAccess.Expression);
				expression = Spill(leftBuilder, expression);
				ImmutableArray<BoundExpression> indices = VisitExpressionList(ref leftBuilder, boundArrayAccess.Indices, default(ImmutableArray<RefKind>), forceSpill: true);
				boundExpression = boundArrayAccess.Update(expression, indices, boundArrayAccess.Type);
				break;
			}
			default:
				boundExpression = Spill(leftBuilder, VisitExpression(ref leftBuilder, boundExpression), RefKind.Ref);
				break;
			case BoundKind.Local:
			case BoundKind.Parameter:
				break;
			}
			leftBuilder.Include(builder);
			builder = leftBuilder;
		}
		return UpdateExpression(builder, node.Update(boundExpression, right, node.IsRef, node.Type));
		BoundExpression fieldWithSpilledReceiver(BoundFieldAccess field, ref BoundSpillSequenceBuilder reference, bool isAssignmentTarget)
		{
			bool flag = false;
			if (!field.FieldSymbol.IsStatic)
			{
				BoundExpression boundExpression2;
				if (field.FieldSymbol.ContainingType.IsReferenceType)
				{
					boundExpression2 = Spill(reference, VisitExpression(ref reference, field.ReceiverOpt));
					flag = !isAssignmentTarget;
				}
				else if (field.ReceiverOpt is BoundArrayAccess boundArrayAccess2)
				{
					BoundExpression expression2 = VisitExpression(ref reference, boundArrayAccess2.Expression);
					expression2 = Spill(reference, expression2);
					ImmutableArray<BoundExpression> indices2 = VisitExpressionList(ref reference, boundArrayAccess2.Indices, default(ImmutableArray<RefKind>), forceSpill: true);
					boundExpression2 = boundArrayAccess2.Update(expression2, indices2, boundArrayAccess2.Type);
					Spill(reference, boundExpression2, RefKind.None, sideEffectsOnly: true);
				}
				else
				{
					boundExpression2 = ((!(field.ReceiverOpt is BoundFieldAccess field2)) ? Spill(reference, VisitExpression(ref reference, field.ReceiverOpt), RefKind.Ref) : fieldWithSpilledReceiver(field2, ref reference, isAssignmentTarget: false));
				}
				field = field.Update(boundExpression2, field.FieldSymbol, field.ConstantValueOpt, field.ResultKind, field.Type);
			}
			if (flag)
			{
				Spill(reference, field, RefKind.None, sideEffectsOnly: true);
			}
			return field;
		}
	}

	public override BoundNode VisitBadExpression(BoundBadExpression node)
	{
		return node;
	}

	public override BoundNode VisitUserDefinedConditionalLogicalOperator(BoundUserDefinedConditionalLogicalOperator node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/SpillSequenceSpiller.cs", 988);
	}

	public override BoundNode VisitBinaryOperator(BoundBinaryOperator node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression right = VisitExpression(ref builder, node.Right);
		BoundExpression boundExpression;
		if (builder == null)
		{
			boundExpression = VisitExpression(ref builder, node.Left);
		}
		else
		{
			BoundSpillSequenceBuilder builder2 = new BoundSpillSequenceBuilder(builder.Syntax);
			boundExpression = VisitExpression(ref builder2, node.Left);
			boundExpression = Spill(builder2, boundExpression);
			if (node.OperatorKind == BinaryOperatorKind.LogicalBoolOr || node.OperatorKind == BinaryOperatorKind.LogicalBoolAnd)
			{
				LocalSymbol local = _F.SynthesizedLocal(node.Type, _F.Syntax, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.Spill);
				builder2.AddLocal(local);
				builder2.AddStatement(_F.Assignment(_F.Local(local), boundExpression));
				builder2.AddStatement(_F.If((node.OperatorKind == BinaryOperatorKind.LogicalBoolAnd) ? _F.Local(local) : _F.Not(_F.Local(local)), UpdateStatement(builder, _F.Assignment(_F.Local(local), right))));
				return UpdateExpression(builder2, _F.Local(local));
			}
			builder2.Include(builder);
			builder = builder2;
		}
		return UpdateExpression(builder, node.Update(node.OperatorKind, node.ConstantValueOpt, node.BinaryOperatorMethod, node.ConstrainedToType, node.ResultKind, boundExpression, right, node.Type));
	}

	public override BoundNode VisitCall(BoundCall node)
	{
		BoundSpillSequenceBuilder builder = null;
		ImmutableArray<BoundExpression> arguments = VisitExpressionList(ref builder, node.Arguments, node.ArgumentRefKindsOpt);
		BoundExpression boundExpression = null;
		if (builder == null || node.ReceiverOpt is BoundTypeExpression)
		{
			boundExpression = VisitExpression(ref builder, node.ReceiverOpt);
		}
		else if (node.Method.RequiresInstanceReceiver)
		{
			BoundSpillSequenceBuilder builder2 = new BoundSpillSequenceBuilder(builder.Syntax);
			boundExpression = node.ReceiverOpt;
			RefKind refKind = ReceiverSpillRefKind(boundExpression);
			boundExpression = Spill(builder2, VisitExpression(ref builder2, boundExpression), refKind);
			if (refKind != RefKind.None && CodeGenerator.IsPossibleReferenceTypeReceiverOfConstrainedCall(boundExpression) && !CodeGenerator.ReceiverIsKnownToReferToTempIfReferenceType(boundExpression) && !CodeGenerator.IsSafeToDereferenceReceiverRefAfterEvaluatingArguments(node.Arguments))
			{
				TypeSymbol type = boundExpression.Type;
				SyntaxNode syntax = _F.Syntax;
				_F.Syntax = node.Syntax;
				BoundLocal boundLocal = _F.Local(_F.SynthesizedLocal(type));
				builder2.AddLocal(boundLocal.LocalSymbol);
				builder2.AddStatement(_F.ExpressionStatement(new BoundComplexConditionalReceiver(node.Syntax, boundLocal, _F.Sequence(new BoundExpression[1] { _F.AssignmentExpression(boundLocal, boundExpression) }, boundLocal), type)
				{
					WasCompilerGenerated = true
				}));
				boundExpression = _F.ComplexConditionalReceiver(boundExpression, boundLocal);
				_F.Syntax = syntax;
			}
			builder2.Include(builder);
			builder = builder2;
		}
		return UpdateExpression(builder, node.Update(boundExpression, ThreeState.Unknown, node.Method, arguments));
	}

	private static RefKind ReceiverSpillRefKind(BoundExpression receiver)
	{
		RefKind result = RefKind.None;
		if (!receiver.Type.IsReferenceType && LocalRewriter.CanBePassedByReference(receiver))
		{
			result = ((!receiver.Type.IsReadOnly) ? RefKind.Ref : RefKind.In);
		}
		return result;
	}

	public override BoundNode VisitFunctionPointerInvocation(BoundFunctionPointerInvocation node)
	{
		BoundSpillSequenceBuilder builder = null;
		ImmutableArray<BoundExpression> arguments = VisitExpressionList(ref builder, node.Arguments, node.ArgumentRefKindsOpt);
		BoundExpression invokedExpression;
		if (builder == null)
		{
			invokedExpression = VisitExpression(ref builder, node.InvokedExpression);
		}
		else
		{
			BoundSpillSequenceBuilder builder2 = new BoundSpillSequenceBuilder(builder.Syntax);
			invokedExpression = Spill(builder2, VisitExpression(ref builder2, node.InvokedExpression));
			builder2.Include(builder);
			builder = builder2;
		}
		return UpdateExpression(builder, node.Update(invokedExpression, arguments, node.ArgumentRefKindsOpt, node.ResultKind, node.Type));
	}

	public override BoundNode VisitConditionalOperator(BoundConditionalOperator node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression boundExpression = VisitExpression(ref builder, node.Condition);
		BoundSpillSequenceBuilder builder2 = null;
		BoundExpression boundExpression2 = VisitExpression(ref builder2, node.Consequence);
		BoundSpillSequenceBuilder builder3 = null;
		BoundExpression boundExpression3 = VisitExpression(ref builder3, node.Alternative);
		if (builder2 == null && builder3 == null)
		{
			return UpdateExpression(builder, node.Update(node.IsRef, boundExpression, boundExpression2, boundExpression3, node.ConstantValueOpt, node.NaturalTypeOpt, node.WasTargetTyped, node.Type));
		}
		if (builder == null)
		{
			builder = new BoundSpillSequenceBuilder((builder2 ?? builder3).Syntax);
		}
		if (builder2 == null)
		{
			builder2 = new BoundSpillSequenceBuilder(builder3.Syntax);
		}
		if (builder3 == null)
		{
			builder3 = new BoundSpillSequenceBuilder(builder2.Syntax);
		}
		if (node.Type.IsVoidType())
		{
			builder.AddStatement(_F.If(boundExpression, UpdateStatement(builder2, _F.ExpressionStatement(boundExpression2)), UpdateStatement(builder3, _F.ExpressionStatement(boundExpression3))));
			return builder.Update(_F.Default(node.Type));
		}
		if (!node.IsRef)
		{
			LocalSymbol local = _F.SynthesizedLocal(node.Type, _F.Syntax, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.Spill);
			builder.AddLocal(local);
			builder.AddStatement(_F.If(boundExpression, UpdateStatement(builder2, _F.Assignment(_F.Local(local), boundExpression2)), UpdateStatement(builder3, _F.Assignment(_F.Local(local), boundExpression3))));
			return builder.Update(_F.Local(local));
		}
		LocalSymbol local2 = _F.SynthesizedLocal(boundExpression.Type, _F.Syntax, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.Spill);
		builder.AddLocal(local2);
		builder.AddStatement(_F.Assignment(_F.Local(local2), boundExpression));
		boundExpression = _F.Local(local2);
		builder.AddLocals(builder2.GetLocals());
		builder.AddLocals(builder3.GetLocals());
		builder.AddStatement(_F.If(boundExpression, _F.StatementList(builder2.GetStatements()), _F.StatementList(builder3.GetStatements())));
		builder2.Free();
		builder3.Free();
		return builder.Update(node.Update(node.IsRef, boundExpression, boundExpression2, boundExpression3, node.ConstantValueOpt, node.NaturalTypeOpt, node.WasTargetTyped, node.Type));
	}

	public override BoundNode VisitConversion(BoundConversion node)
	{
		if (node.ConversionKind == ConversionKind.AnonymousFunction && node.Type.IsExpressionTree())
		{
			return node;
		}
		BoundSpillSequenceBuilder builder = null;
		BoundExpression operand = VisitExpression(ref builder, node.Operand);
		return UpdateExpression(builder, node.UpdateOperand(operand));
	}

	public override BoundNode VisitPassByCopy(BoundPassByCopy node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression expression = VisitExpression(ref builder, node.Expression);
		return UpdateExpression(builder, node.Update(expression, node.Type));
	}

	public override BoundNode VisitMethodGroup(BoundMethodGroup node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/SpillSequenceSpiller.cs", 1215);
	}

	public override BoundNode VisitDelegateCreationExpression(BoundDelegateCreationExpression node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression argument = VisitExpression(ref builder, node.Argument);
		return UpdateExpression(builder, node.Update(argument, node.MethodOpt, node.IsExtensionMethod, node.WasTargetTyped, node.Type));
	}

	public override BoundNode VisitFieldAccess(BoundFieldAccess node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression receiver = VisitExpression(ref builder, node.ReceiverOpt);
		return UpdateExpression(builder, node.Update(receiver, node.FieldSymbol, node.ConstantValueOpt, node.ResultKind, node.Type));
	}

	public override BoundNode VisitIsOperator(BoundIsOperator node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression operand = VisitExpression(ref builder, node.Operand);
		return UpdateExpression(builder, node.Update(operand, node.TargetType, node.ConversionKind, node.Type));
	}

	public override BoundNode VisitMakeRefOperator(BoundMakeRefOperator node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression operand = VisitExpression(ref builder, node.Operand);
		return UpdateExpression(builder, node.Update(operand, node.Type));
	}

	public override BoundNode VisitNullCoalescingOperator(BoundNullCoalescingOperator node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression boundExpression = VisitExpression(ref builder, node.RightOperand);
		BoundExpression leftOperand;
		if (builder == null)
		{
			leftOperand = VisitExpression(ref builder, node.LeftOperand);
			return UpdateExpression(builder, node.Update(leftOperand, boundExpression, node.LeftPlaceholder, node.LeftConversion, node.OperatorResultKind, node.Checked, node.Type));
		}
		BoundSpillSequenceBuilder builder2 = new BoundSpillSequenceBuilder(builder.Syntax);
		leftOperand = VisitExpression(ref builder2, node.LeftOperand);
		leftOperand = Spill(builder2, leftOperand);
		LocalSymbol local = _F.SynthesizedLocal(node.Type, _F.Syntax, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.Spill);
		builder2.AddLocal(local);
		builder2.AddStatement(_F.Assignment(_F.Local(local), leftOperand));
		builder2.AddStatement(_F.If(_F.ObjectEqual(_F.Local(local), _F.Null(leftOperand.Type)), UpdateStatement(builder, _F.Assignment(_F.Local(local), boundExpression))));
		return UpdateExpression(builder2, _F.Local(local));
	}

	public override BoundNode VisitLoweredConditionalAccess(BoundLoweredConditionalAccess node)
	{
		RefKind refKind = ReceiverSpillRefKind(node.Receiver);
		BoundSpillSequenceBuilder builder = null;
		BoundExpression boundExpression = VisitExpression(ref builder, node.Receiver);
		BoundSpillSequenceBuilder builder2 = null;
		BoundExpression boundExpression2 = VisitExpression(ref builder2, node.WhenNotNull);
		BoundSpillSequenceBuilder builder3 = null;
		BoundExpression boundExpression3 = VisitExpression(ref builder3, node.WhenNullOpt);
		if (builder2 == null && builder3 == null)
		{
			return UpdateExpression(builder, node.Update(boundExpression, node.HasValueMethodOpt, boundExpression2, boundExpression3, node.Id, node.ForceCopyOfNullableValueType, node.Type));
		}
		if (builder == null)
		{
			builder = new BoundSpillSequenceBuilder((builder2 ?? builder3).Syntax);
		}
		if (builder2 == null)
		{
			builder2 = new BoundSpillSequenceBuilder(builder3.Syntax);
		}
		if (builder3 == null)
		{
			builder3 = new BoundSpillSequenceBuilder(builder2.Syntax);
		}
		BoundExpression condition;
		if (boundExpression.Type.IsReferenceType || boundExpression.Type.IsValueType || refKind == RefKind.None)
		{
			boundExpression = Spill(builder, boundExpression);
			MethodSymbol hasValueMethodOpt = node.HasValueMethodOpt;
			condition = ((!(hasValueMethodOpt == null)) ? _F.Call(boundExpression, hasValueMethodOpt) : _F.IsNotNullReference(boundExpression));
		}
		else
		{
			boundExpression = Spill(builder, boundExpression, RefKind.Ref);
			LocalSymbol local = _F.SynthesizedLocal(boundExpression.Type, _F.Syntax, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.Spill);
			builder.AddLocal(local);
			BoundExpression left = _F.IsNotNullReference(_F.Default(boundExpression.Type));
			condition = _F.LogicalOr(left, _F.MakeSequence(_F.AssignmentExpression(_F.Local(local), boundExpression), _F.IsNotNullReference(_F.Local(local))));
			boundExpression = _F.ComplexConditionalReceiver(boundExpression, _F.Local(local));
		}
		if (node.Type.IsVoidType())
		{
			BoundStatement node2 = UpdateStatement(builder2, _F.ExpressionStatement(boundExpression2));
			node2 = ConditionalReceiverReplacer.Replace(node2, boundExpression, node.Id, base.RecursionDepth);
			builder.AddStatement(_F.If(condition, node2));
			return builder.Update(_F.Default(node.Type));
		}
		LocalSymbol local2 = _F.SynthesizedLocal(node.Type, _F.Syntax, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.Spill);
		BoundStatement node3 = UpdateStatement(builder2, _F.Assignment(_F.Local(local2), boundExpression2));
		node3 = ConditionalReceiverReplacer.Replace(node3, boundExpression, node.Id, base.RecursionDepth);
		boundExpression3 = boundExpression3 ?? _F.Default(node.Type);
		builder.AddLocal(local2);
		builder.AddStatement(_F.If(condition, node3, UpdateStatement(builder3, _F.Assignment(_F.Local(local2), boundExpression3))));
		return builder.Update(_F.Local(local2));
	}

	public override BoundNode VisitLambda(BoundLambda node)
	{
		MethodSymbol currentFunction = _F.CurrentFunction;
		_F.CurrentFunction = node.Symbol;
		BoundNode? result = base.VisitLambda(node);
		_F.CurrentFunction = currentFunction;
		return result;
	}

	public override BoundNode VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
	{
		MethodSymbol currentFunction = _F.CurrentFunction;
		_F.CurrentFunction = node.Symbol;
		BoundNode? result = base.VisitLocalFunctionStatement(node);
		_F.CurrentFunction = currentFunction;
		return result;
	}

	public override BoundNode VisitObjectCreationExpression(BoundObjectCreationExpression node)
	{
		BoundSpillSequenceBuilder builder = null;
		ImmutableArray<BoundExpression> arguments = VisitExpressionList(ref builder, node.Arguments, node.ArgumentRefKindsOpt);
		return UpdateExpression(builder, node.Update(node.Constructor, arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, node.Expanded, node.ArgsToParamsOpt, node.DefaultArguments, node.ConstantValueOpt, node.InitializerExpressionOpt, node.Type));
	}

	public override BoundNode VisitPointerElementAccess(BoundPointerElementAccess node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression index = VisitExpression(ref builder, node.Index);
		BoundExpression expression;
		if (builder == null)
		{
			expression = VisitExpression(ref builder, node.Expression);
		}
		else
		{
			BoundSpillSequenceBuilder builder2 = new BoundSpillSequenceBuilder(builder.Syntax);
			expression = VisitExpression(ref builder2, node.Expression);
			expression = Spill(builder2, expression);
			builder2.Include(builder);
			builder = builder2;
		}
		return UpdateExpression(builder, node.Update(expression, index, node.Checked, node.RefersToLocation, node.Type));
	}

	public override BoundNode VisitPointerIndirectionOperator(BoundPointerIndirectionOperator node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression operand = VisitExpression(ref builder, node.Operand);
		return UpdateExpression(builder, node.Update(operand, node.RefersToLocation, node.Type));
	}

	public override BoundNode VisitSequence(BoundSequence node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression value = VisitExpression(ref builder, node.Value);
		BoundSpillSequenceBuilder builder2 = null;
		ImmutableArray<BoundExpression> sideEffects = node.SideEffects;
		bool forceSpill = builder != null;
		ImmutableArray<BoundExpression> immutableArray = VisitExpressionList(ref builder2, sideEffects, default(ImmutableArray<RefKind>), forceSpill, sideEffectsOnly: true);
		if (builder2 == null && builder == null)
		{
			return node.Update(node.Locals, immutableArray, value, node.Type);
		}
		if (builder2 == null)
		{
			builder2 = new BoundSpillSequenceBuilder(builder.Syntax);
		}
		PromoteAndAddLocals(builder2, node.Locals);
		builder2.AddExpressions(immutableArray);
		builder2.Include(builder);
		return builder2.Update(value);
	}

	public override BoundNode VisitThrowExpression(BoundThrowExpression node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression expression = VisitExpression(ref builder, node.Expression);
		return UpdateExpression(builder, node.Update(expression, node.Type));
	}

	private void PromoteAndAddLocals(BoundSpillSequenceBuilder builder, ImmutableArray<LocalSymbol> locals)
	{
		foreach (LocalSymbol item in locals)
		{
			if (item.SynthesizedKind.IsLongLived())
			{
				builder.AddLocal(item);
			}
			else if (!_receiverSubstitution.ContainsKey(item))
			{
				LocalSymbol localSymbol = item.WithSynthesizedLocalKindAndSyntax(SynthesizedLocalKind.Spill, _F.Syntax);
				_tempSubstitution.Add(item, localSymbol);
				builder.AddLocal(localSymbol);
			}
		}
	}

	public override BoundNode VisitUnaryOperator(BoundUnaryOperator node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression operand = VisitExpression(ref builder, node.Operand);
		return UpdateExpression(builder, node.Update(node.OperatorKind, operand, node.ConstantValueOpt, node.MethodOpt, node.ConstrainedToTypeOpt, node.ResultKind, node.Type));
	}

	public override BoundNode VisitReadOnlySpanFromArray(BoundReadOnlySpanFromArray node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression operand = VisitExpression(ref builder, node.Operand);
		return UpdateExpression(builder, node.Update(operand, node.ConversionMethod, node.Type));
	}

	public override BoundNode VisitSequencePointExpression(BoundSequencePointExpression node)
	{
		BoundSpillSequenceBuilder builder = null;
		BoundExpression expression = VisitExpression(ref builder, node.Expression);
		return UpdateExpression(builder, node.Update(expression, node.Type));
	}
}
