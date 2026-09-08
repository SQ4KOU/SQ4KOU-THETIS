using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class LocalStateTracingInstrumenter : CompoundInstrumenter
{
	private sealed class Scope
	{
		public LocalSymbol ContextVariable;

		private ArrayBuilder<LocalSymbol>? _lazyPreviousContextVariables;

		public Scope(LocalSymbol contextVariable)
		{
			ContextVariable = contextVariable;
		}

		public void Open(LocalSymbol local)
		{
			if (_lazyPreviousContextVariables == null)
			{
				_lazyPreviousContextVariables = ArrayBuilder<LocalSymbol>.GetInstance();
			}
			_lazyPreviousContextVariables.Push(ContextVariable);
			ContextVariable = local;
		}

		public void Close(bool isMethodBody)
		{
			ArrayBuilder<LocalSymbol> lazyPreviousContextVariables = _lazyPreviousContextVariables;
			if (lazyPreviousContextVariables != null && lazyPreviousContextVariables.Count > 0)
			{
				ContextVariable = _lazyPreviousContextVariables.Pop();
			}
			if (isMethodBody)
			{
				_lazyPreviousContextVariables?.Free();
				_lazyPreviousContextVariables = null;
			}
		}
	}

	private readonly Scope _scope;

	private readonly SyntheticBoundNodeFactory _factory;

	private readonly BindingDiagnosticBag _diagnostics;

	private readonly TypeSymbol _contextType;

	private LocalStateTracingInstrumenter(Scope scope, TypeSymbol contextType, SyntheticBoundNodeFactory factory, BindingDiagnosticBag diagnostics, Instrumenter previous)
		: base(previous)
	{
		_scope = scope;
		_contextType = contextType;
		_factory = factory;
		_diagnostics = diagnostics;
	}

	protected override CompoundInstrumenter WithPreviousImpl(Instrumenter previous)
	{
		return new LocalStateTracingInstrumenter(_scope, _contextType, _factory, _diagnostics, previous);
	}

	public static bool TryCreate(MethodSymbol method, BoundStatement methodBody, SyntheticBoundNodeFactory factory, BindingDiagnosticBag diagnostics, Instrumenter previous, [NotNullWhen(true)] out LocalStateTracingInstrumenter? instrumenter)
	{
		instrumenter = null;
		if (method.IsImplicitlyDeclared && !method.IsImplicitConstructor)
		{
			return false;
		}
		if (method is SourceMemberMethodSymbol sourceMemberMethodSymbol)
		{
			(BlockSyntax, ArrowExpressionClauseSyntax) bodies = sourceMemberMethodSymbol.Bodies;
			if (bodies.Item2 == null && bodies.Item1 == null && !(sourceMemberMethodSymbol is SynthesizedSimpleProgramEntryPointSymbol))
			{
				return false;
			}
		}
		NamedTypeSymbol wellKnownType = factory.Compilation.GetWellKnownType(WellKnownType.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker);
		if (IsSameOrNestedType(method.ContainingType, wellKnownType))
		{
			return false;
		}
		Scope scope = new Scope(factory.SynthesizedLocal(wellKnownType, methodBody.Syntax, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.LocalStoreTracker));
		instrumenter = new LocalStateTracingInstrumenter(scope, wellKnownType, factory, diagnostics, previous);
		return true;
	}

	private static bool IsSameOrNestedType(NamedTypeSymbol type, NamedTypeSymbol otherType)
	{
		while (true)
		{
			if (type.Equals(otherType))
			{
				return true;
			}
			if ((object)type.ContainingType == null)
			{
				break;
			}
			type = type.ContainingType;
		}
		return false;
	}

	private MethodSymbol? GetLocalOrParameterStoreLogger(TypeSymbol variableType, Symbol targetSymbol, bool? refAssignmentSourceIsLocal, SyntaxNode syntax)
	{
		int num = ((targetSymbol.Kind == SymbolKind.Parameter) ? 13 : 0);
		WellKnownMember? wellKnownMember;
		if (refAssignmentSourceIsLocal.HasValue)
		{
			wellKnownMember = ((refAssignmentSourceIsLocal != true) ? new WellKnownMember?(WellKnownMember.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker__LogLocalStoreParameterAlias) : new WellKnownMember?(WellKnownMember.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker__LogLocalStoreLocalAlias));
		}
		else
		{
			WellKnownMember? wellKnownMember2;
			switch (variableType.EnumUnderlyingTypeOrSelf().SpecialType)
			{
			case SpecialType.System_Boolean:
				wellKnownMember2 = WellKnownMember.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker__LogLocalStoreBoolean;
				break;
			case SpecialType.System_SByte:
			case SpecialType.System_Byte:
				wellKnownMember2 = WellKnownMember.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker__LogLocalStoreByte;
				break;
			case SpecialType.System_Char:
			case SpecialType.System_Int16:
			case SpecialType.System_UInt16:
				wellKnownMember2 = WellKnownMember.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker__LogLocalStoreUInt16;
				break;
			case SpecialType.System_Int32:
			case SpecialType.System_UInt32:
				wellKnownMember2 = WellKnownMember.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker__LogLocalStoreUInt32;
				break;
			case SpecialType.System_Int64:
			case SpecialType.System_UInt64:
				wellKnownMember2 = WellKnownMember.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker__LogLocalStoreUInt64;
				break;
			case SpecialType.System_Single:
				wellKnownMember2 = WellKnownMember.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker__LogLocalStoreSingle;
				break;
			case SpecialType.System_Double:
				wellKnownMember2 = WellKnownMember.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker__LogLocalStoreDouble;
				break;
			case SpecialType.System_Decimal:
				wellKnownMember2 = WellKnownMember.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker__LogLocalStoreDecimal;
				break;
			case SpecialType.System_String:
				wellKnownMember2 = WellKnownMember.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker__LogLocalStoreString;
				break;
			default:
				wellKnownMember2 = ((!variableType.IsPointerOrFunctionPointer()) ? (variableType.IsManagedTypeNoUseSiteDiagnostics ? ((variableType.IsRefLikeType && !hasOverriddenToString(variableType)) ? ((WellKnownMember?)null) : ((variableType is TypeParameterSymbol { AllowsRefLikeType: not false }) ? ((WellKnownMember?)null) : ((variableType.TypeKind != TypeKind.Struct) ? new WellKnownMember?(WellKnownMember.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker__LogLocalStoreObject) : new WellKnownMember?(WellKnownMember.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker__LogLocalStoreString)))) : new WellKnownMember?(WellKnownMember.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker__LogLocalStoreUnmanaged)) : new WellKnownMember?(WellKnownMember.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker__LogLocalStorePointer));
				break;
			}
			wellKnownMember = wellKnownMember2;
		}
		WellKnownMember? wellKnownMember3 = wellKnownMember;
		if (!wellKnownMember3.HasValue)
		{
			return null;
		}
		WellKnownMember overload = wellKnownMember3.Value + num;
		return GetWellKnownMethodSymbol(overload, syntax);
		static bool hasOverriddenToString(TypeSymbol typeSymbol)
		{
			return typeSymbol.GetMembers("ToString").Any((Symbol m) => (object)m.GetOverriddenMember() != null);
		}
	}

	private MethodSymbol? GetWellKnownMethodSymbol(WellKnownMember overload, SyntaxNode syntax)
	{
		return (MethodSymbol)Binder.GetWellKnownTypeMember(_factory.Compilation, overload, _diagnostics, null, syntax);
	}

	private MethodSymbol? GetSpecialMethodSymbol(SpecialMember overload, SyntaxNode syntax)
	{
		return (MethodSymbol)Binder.GetSpecialTypeMember(_factory.Compilation, overload, _diagnostics, syntax);
	}

	public override void PreInstrumentBlock(BoundBlock original, LocalRewriter rewriter)
	{
		base.Previous.PreInstrumentBlock(original, rewriter);
		if (rewriter.CurrentLambdaBody == original)
		{
			_scope.Open(_factory.SynthesizedLocal(_contextType, original.Syntax, isPinned: false, isKnownToReferToTempIfReferenceType: false, RefKind.None, SynthesizedLocalKind.LocalStoreTracker));
		}
	}

	public override void InstrumentBlock(BoundBlock original, LocalRewriter rewriter, ref TemporaryArray<LocalSymbol> additionalLocals, out BoundStatement? prologue, out BoundStatement? epilogue, out BoundBlockInstrumentation? instrumentation)
	{
		base.InstrumentBlock(original, rewriter, ref additionalLocals, out BoundStatement prologue2, out epilogue, out instrumentation);
		bool flag = rewriter.CurrentMethodBody == original;
		bool flag2 = rewriter.CurrentLambdaBody == original;
		if (!flag && !flag2)
		{
			prologue = prologue2;
			return;
		}
		bool flag3 = _factory.CurrentFunction.IsAsync || _factory.CurrentFunction.IsIterator;
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance(_factory.CurrentFunction.ParameterCount);
		foreach (ParameterSymbol item3 in _factory.CurrentFunction.GetParametersIncludingExtensionParameter(skipExtensionIfStatic: true))
		{
			if (item3.RefKind != RefKind.Out && !item3.IsDiscard)
			{
				MethodSymbol localOrParameterStoreLogger = GetLocalOrParameterStoreLogger(item3.Type, item3, null, _factory.Syntax);
				if (localOrParameterStoreLogger != null)
				{
					int num = (item3.ContainingSymbol.IsExtensionBlockMember() ? SourceExtensionImplementationMethodSymbol.GetImplementationParameterOrdinal(item3) : item3.Ordinal);
					instance.Add(_factory.ExpressionStatement(_factory.Call(_factory.Local(_scope.ContextVariable), localOrParameterStoreLogger, MakeStoreLoggerArguments(localOrParameterStoreLogger.Parameters[0], item3, item3.Type, _factory.Parameter(item3), null, _factory.Literal((ushort)num)))));
				}
			}
		}
		if (prologue2 != null)
		{
			instance.Add(prologue2);
		}
		prologue = _factory.StatementList(instance.ToImmutableAndFree());
		(WellKnownMember, BoundExpression[]) tuple = ((!flag2) ? (flag3 ? (WellKnownMember.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker__LogStateMachineMethodEntry, new BoundExpression[2]
		{
			_factory.MethodDefIndex(_factory.TopLevelMethod),
			_factory.StateMachineInstanceId()
		}) : (WellKnownMember.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker__LogMethodEntry, new BoundExpression[1] { _factory.MethodDefIndex(_factory.TopLevelMethod) })) : (flag3 ? (WellKnownMember.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker__LogStateMachineLambdaEntry, new BoundExpression[3]
		{
			_factory.MethodDefIndex(_factory.TopLevelMethod),
			_factory.MethodDefIndex(_factory.CurrentFunction),
			_factory.StateMachineInstanceId()
		}) : (WellKnownMember.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker__LogLambdaEntry, new BoundExpression[2]
		{
			_factory.MethodDefIndex(_factory.TopLevelMethod),
			_factory.MethodDefIndex(_factory.CurrentFunction)
		})));
		(WellKnownMember, BoundExpression[]) tuple2 = tuple;
		WellKnownMember item = tuple2.Item1;
		BoundExpression[] item2 = tuple2.Item2;
		MethodSymbol wellKnownMethodSymbol = GetWellKnownMethodSymbol(item, _factory.Syntax);
		BoundStatement prologue3 = ((wellKnownMethodSymbol != null) ? _factory.Assignment(_factory.Local(_scope.ContextVariable), _factory.Call(null, wellKnownMethodSymbol, item2)) : _factory.NoOp(NoOpStatementFlavor.Default));
		MethodSymbol wellKnownMethodSymbol2 = GetWellKnownMethodSymbol(WellKnownMember.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker__LogReturn, _factory.Syntax);
		BoundStatement epilogue2 = ((wellKnownMethodSymbol2 != null) ? _factory.ExpressionStatement(_factory.Call(_factory.Local(_scope.ContextVariable), wellKnownMethodSymbol2)) : _factory.NoOp(NoOpStatementFlavor.Default));
		instrumentation = _factory.CombineInstrumentation(instrumentation, _scope.ContextVariable, prologue3, epilogue2);
		_scope.Close(flag);
	}

	public override BoundExpression InstrumentUserDefinedLocalAssignment(BoundAssignmentOperator original)
	{
		BoundExpression boundExpression = base.InstrumentUserDefinedLocalAssignment(original);
		bool? refAssignmentSourceIsLocal;
		BoundExpression refAssignmentSourceIndex;
		if (original.IsRef)
		{
			if (original.Right is BoundLocal boundLocal)
			{
				LocalSymbol localSymbol = boundLocal.LocalSymbol;
				if ((object)localSymbol != null && localSymbol.SynthesizedKind == SynthesizedLocalKind.UserDefined)
				{
					refAssignmentSourceIsLocal = true;
					refAssignmentSourceIndex = _factory.LocalId(boundLocal.LocalSymbol);
					goto IL_008e;
				}
			}
			if (!(original.Right is BoundParameter boundParameter))
			{
				return boundExpression;
			}
			refAssignmentSourceIsLocal = false;
			refAssignmentSourceIndex = _factory.ParameterId(boundParameter.ParameterSymbol);
		}
		else
		{
			refAssignmentSourceIsLocal = null;
			refAssignmentSourceIndex = null;
		}
		goto IL_008e;
		IL_008e:
		if (!TryGetLocalOrParameterInfo(original.Left, out Symbol symbol, out TypeSymbol type, out BoundExpression indexExpression))
		{
			throw ExceptionUtilities.UnexpectedValue(original.Left);
		}
		MethodSymbol localOrParameterStoreLogger = GetLocalOrParameterStoreLogger(type, symbol, refAssignmentSourceIsLocal, original.Syntax);
		if ((object)localOrParameterStoreLogger == null)
		{
			return boundExpression;
		}
		SyntheticBoundNodeFactory factory = _factory;
		BoundExpression[] sideEffects = new BoundCall[1] { _factory.Call(_factory.Local(_scope.ContextVariable), localOrParameterStoreLogger, MakeStoreLoggerArguments(localOrParameterStoreLogger.Parameters[0], symbol, type, boundExpression, refAssignmentSourceIndex, indexExpression)) };
		return factory.Sequence(sideEffects, VariableRead(symbol));
	}

	private bool TryGetLocalOrParameterInfo(BoundNode node, [NotNullWhen(true)] out Symbol? symbol, [NotNullWhen(true)] out TypeSymbol? type, [NotNullWhen(true)] out BoundExpression? indexExpression)
	{
		if (node is BoundLocal boundLocal)
		{
			LocalSymbol localSymbol = (LocalSymbol)(symbol = boundLocal.LocalSymbol);
			type = localSymbol.Type;
			indexExpression = _factory.LocalId(localSymbol);
			return true;
		}
		if (node is BoundParameter boundParameter)
		{
			ParameterSymbol parameterSymbol = (ParameterSymbol)(symbol = boundParameter.ParameterSymbol);
			type = parameterSymbol.Type;
			indexExpression = _factory.ParameterId(parameterSymbol);
			return true;
		}
		symbol = null;
		indexExpression = null;
		type = null;
		return false;
	}

	private ImmutableArray<BoundExpression> MakeStoreLoggerArguments(ParameterSymbol parameter, Symbol targetSymbol, TypeSymbol targetType, BoundExpression value, BoundExpression? refAssignmentSourceIndex, BoundExpression index)
	{
		if (refAssignmentSourceIndex != null)
		{
			return ImmutableArray.Create(_factory.Sequence(new BoundExpression[1] { value }, refAssignmentSourceIndex), index);
		}
		if (parameter.Type.IsVoidPointer() && !targetType.IsPointerOrFunctionPointer())
		{
			bool flag = ((value is BoundLocal || value is BoundParameter) ? true : false);
			return ImmutableArray.Create(flag ? ((BoundExpression)new BoundAddressOfOperator(_factory.Syntax, value, isManaged: false, parameter.Type)) : ((BoundExpression)_factory.Sequence(new BoundExpression[1] { value }, new BoundAddressOfOperator(_factory.Syntax, VariableRead(targetSymbol), isManaged: false, parameter.Type))), _factory.Sizeof(targetType), index);
		}
		if (parameter.Type.SpecialType == SpecialType.System_String && targetType.SpecialType != SpecialType.System_String)
		{
			MethodSymbol specialMethodSymbol = GetSpecialMethodSymbol(SpecialMember.System_Object__ToString, value.Syntax);
			BoundExpression item = (((object)specialMethodSymbol != null) ? ((BoundExpression)_factory.Call(value, specialMethodSymbol)) : ((BoundExpression)_factory.Literal("")));
			return ImmutableArray.Create(item, index);
		}
		Conversion conversion = _factory.ClassifyEmitConversion(value, parameter.Type);
		return ImmutableArray.Create(_factory.Convert(parameter.Type, value, conversion), index);
	}

	private BoundExpression VariableRead(Symbol localOrParameterSymbol)
	{
		if (!(localOrParameterSymbol is LocalSymbol local))
		{
			if (localOrParameterSymbol is ParameterSymbol p)
			{
				return _factory.Parameter(p);
			}
			throw ExceptionUtilities.UnexpectedValue(localOrParameterSymbol);
		}
		return _factory.Local(local);
	}

	public override void InstrumentCatchBlock(BoundCatchBlock original, ref BoundExpression? rewrittenSource, ref BoundStatementList? rewrittenFilterPrologue, ref BoundExpression? rewrittenFilter, ref BoundBlock rewrittenBody, ref TypeSymbol? rewrittenType, SyntheticBoundNodeFactory factory)
	{
		base.InstrumentCatchBlock(original, ref rewrittenSource, ref rewrittenFilterPrologue, ref rewrittenFilter, ref rewrittenBody, ref rewrittenType, factory);
		if (original.WasCompilerGenerated)
		{
			return;
		}
		LocalSymbol localSymbol = original.Locals.FirstOrDefault((LocalSymbol l) => l.SynthesizedKind == SynthesizedLocalKind.UserDefined);
		if ((object)localSymbol != null)
		{
			TypeSymbol type = localSymbol.Type;
			BoundExpression index = _factory.LocalId(localSymbol);
			MethodSymbol localOrParameterStoreLogger = GetLocalOrParameterStoreLogger(type, localSymbol, null, original.Syntax);
			if ((object)localOrParameterStoreLogger != null)
			{
				BoundExpressionStatement boundExpressionStatement = _factory.ExpressionStatement(_factory.Call(_factory.Local(_scope.ContextVariable), localOrParameterStoreLogger, MakeStoreLoggerArguments(localOrParameterStoreLogger.Parameters[0], localSymbol, type, VariableRead(localSymbol), null, index)));
				rewrittenFilterPrologue = _factory.StatementList((rewrittenFilterPrologue != null) ? ImmutableArray.Create((BoundStatement)boundExpressionStatement, (BoundStatement)rewrittenFilterPrologue) : ImmutableArray.Create((BoundStatement)boundExpressionStatement));
			}
		}
	}

	public override BoundExpression InstrumentCall(BoundCall original, BoundExpression rewritten)
	{
		ImmutableArray<BoundExpression> immutableArray = original.Arguments;
		MethodSymbol method = original.Method;
		bool flag = method.IsExtensionBlockMember() && !method.IsStatic;
		ImmutableArray<RefKind> argumentRefKinds = NullableWalker.GetArgumentRefKinds(original.ArgumentRefKindsOpt, flag, method, immutableArray.Length);
		if (flag)
		{
			BoundExpression receiverOpt = original.ReceiverOpt;
			ImmutableArray<BoundExpression> immutableArray2 = immutableArray;
			int num = 0;
			BoundExpression[] array = new BoundExpression[1 + immutableArray2.Length];
			array[num] = receiverOpt;
			num++;
			ReadOnlySpan<BoundExpression> readOnlySpan = immutableArray2.AsSpan();
			readOnlySpan.CopyTo(new Span<BoundExpression>(array).Slice(num, readOnlySpan.Length));
			num += readOnlySpan.Length;
			immutableArray = ImmutableCollectionsMarshal.AsImmutableArray(array);
		}
		return InstrumentCall(base.InstrumentCall(original, rewritten), immutableArray, argumentRefKinds);
	}

	public override BoundExpression InstrumentObjectCreationExpression(BoundObjectCreationExpression original, BoundExpression rewritten)
	{
		return InstrumentCall(base.InstrumentObjectCreationExpression(original, rewritten), original.Arguments, original.ArgumentRefKindsOpt);
	}

	public override BoundExpression InstrumentFunctionPointerInvocation(BoundFunctionPointerInvocation original, BoundExpression rewritten)
	{
		return InstrumentCall(base.InstrumentFunctionPointerInvocation(original, rewritten), original.Arguments, original.ArgumentRefKindsOpt);
	}

	private BoundExpression InstrumentCall(BoundExpression invocation, ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> refKinds)
	{
		if (refKinds.IsDefaultOrEmpty)
		{
			return invocation;
		}
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		BoundLocal boundLocal = null;
		if (invocation.Type.SpecialType != SpecialType.System_Void)
		{
			boundLocal = _factory.StoreToTemp(invocation, out BoundAssignmentOperator store);
			instance.Add(store);
		}
		else
		{
			instance.Add(invocation);
		}
		for (int i = 0; i < arguments.Length; i++)
		{
			RefKind refKind = refKinds[i];
			bool flag = refKind - 1 <= RefKind.Ref;
			if (flag && TryGetLocalOrParameterInfo(arguments[i], out Symbol symbol, out TypeSymbol type, out BoundExpression indexExpression))
			{
				MethodSymbol localOrParameterStoreLogger = GetLocalOrParameterStoreLogger(type, symbol, null, invocation.Syntax);
				if ((object)localOrParameterStoreLogger != null)
				{
					instance.Add(_factory.Call(_factory.Local(_scope.ContextVariable), localOrParameterStoreLogger, MakeStoreLoggerArguments(localOrParameterStoreLogger.Parameters[0], symbol, type, VariableRead(symbol), null, indexExpression)));
				}
			}
		}
		if (boundLocal != null)
		{
			return _factory.Sequence(ImmutableArray.Create(boundLocal.LocalSymbol), instance.ToImmutableAndFree(), boundLocal);
		}
		BoundExpression result = instance.Last();
		instance.RemoveLast();
		return _factory.Sequence(ImmutableArray<LocalSymbol>.Empty, instance.ToImmutableAndFree(), result);
	}
}
