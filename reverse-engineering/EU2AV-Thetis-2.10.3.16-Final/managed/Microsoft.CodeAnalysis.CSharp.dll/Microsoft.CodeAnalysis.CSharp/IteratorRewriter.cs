using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal class IteratorRewriter : StateMachineRewriter
{
	private readonly TypeWithAnnotations _elementType;

	private readonly bool _isEnumerable;

	private FieldSymbol _currentField;

	protected override bool PreserveInitialParameterValuesAndThreadId => _isEnumerable;

	private IteratorRewriter(BoundStatement body, MethodSymbol method, bool isEnumerable, IteratorStateMachine stateMachineType, ArrayBuilder<StateMachineStateDebugInfo> stateMachineStateDebugInfoBuilder, VariableSlotAllocator slotAllocatorOpt, TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
		: base(body, method, stateMachineType, stateMachineStateDebugInfoBuilder, slotAllocatorOpt, compilationState, diagnostics)
	{
		_elementType = stateMachineType.ElementType;
		_isEnumerable = isEnumerable;
	}

	internal static BoundStatement Rewrite(BoundStatement body, MethodSymbol method, int methodOrdinal, ArrayBuilder<StateMachineStateDebugInfo> stateMachineStateDebugInfoBuilder, VariableSlotAllocator slotAllocatorOpt, TypeCompilationState compilationState, BindingDiagnosticBag diagnostics, out IteratorStateMachine stateMachineType)
	{
		TypeWithAnnotations iteratorElementTypeWithAnnotations = method.IteratorElementTypeWithAnnotations;
		if (iteratorElementTypeWithAnnotations.IsDefault || method.IsAsync)
		{
			stateMachineType = null;
			return body;
		}
		bool isEnumerable;
		switch (method.ReturnType.OriginalDefinition.SpecialType)
		{
		case SpecialType.System_Collections_IEnumerable:
		case SpecialType.System_Collections_Generic_IEnumerable_T:
			isEnumerable = true;
			break;
		case SpecialType.System_Collections_IEnumerator:
		case SpecialType.System_Collections_Generic_IEnumerator_T:
			isEnumerable = false;
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(method.ReturnType.OriginalDefinition.SpecialType);
		}
		stateMachineType = new IteratorStateMachine(slotAllocatorOpt, compilationState, method, methodOrdinal, isEnumerable, iteratorElementTypeWithAnnotations);
		compilationState.ModuleBuilderOpt.CompilationState.SetStateMachineType(method, stateMachineType);
		IteratorRewriter iteratorRewriter = new IteratorRewriter(body, method, isEnumerable, stateMachineType, stateMachineStateDebugInfoBuilder, slotAllocatorOpt, compilationState, diagnostics);
		if (!iteratorRewriter.VerifyPresenceOfRequiredAPIs())
		{
			return (BoundStatement)body.WithHasErrors();
		}
		return iteratorRewriter.Rewrite();
	}

	protected bool VerifyPresenceOfRequiredAPIs()
	{
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: true, diagnostics.AccumulatesDependencies);
		EnsureSpecialType(SpecialType.System_Int32, instance);
		EnsureSpecialType(SpecialType.System_IDisposable, instance);
		EnsureSpecialMember(SpecialMember.System_IDisposable__Dispose, instance);
		EnsureSpecialType(SpecialType.System_Collections_IEnumerator, instance);
		EnsureSpecialPropertyGetter(SpecialMember.System_Collections_IEnumerator__Current, instance);
		EnsureSpecialMember(SpecialMember.System_Collections_IEnumerator__MoveNext, instance);
		EnsureSpecialMember(SpecialMember.System_Collections_IEnumerator__Reset, instance);
		EnsureSpecialType(SpecialType.System_Collections_Generic_IEnumerator_T, instance);
		EnsureSpecialPropertyGetter(SpecialMember.System_Collections_Generic_IEnumerator_T__Current, instance);
		if (_isEnumerable)
		{
			EnsureSpecialType(SpecialType.System_Collections_IEnumerable, instance);
			EnsureSpecialMember(SpecialMember.System_Collections_IEnumerable__GetEnumerator, instance);
			EnsureSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T, instance);
			EnsureSpecialMember(SpecialMember.System_Collections_Generic_IEnumerable_T__GetEnumerator, instance);
		}
		bool num = instance.HasAnyErrors();
		if (!num)
		{
			diagnostics.AddDependencies(instance);
		}
		else
		{
			diagnostics.AddRange(instance);
		}
		instance.Free();
		return !num;
	}

	private Symbol EnsureSpecialMember(SpecialMember member, BindingDiagnosticBag bag)
	{
		Binder.TryGetSpecialTypeMember<Symbol>(F.Compilation, member, body.Syntax, bag, out var symbol);
		return symbol;
	}

	private void EnsureSpecialType(SpecialType type, BindingDiagnosticBag bag)
	{
		Binder.GetSpecialType(F.Compilation, type, body.Syntax, bag);
	}

	private void EnsureSpecialPropertyGetter(SpecialMember member, BindingDiagnosticBag bag)
	{
		PropertySymbol propertySymbol = (PropertySymbol)EnsureSpecialMember(member, bag);
		if ((object)propertySymbol != null)
		{
			MethodSymbol getMethod = propertySymbol.GetMethod;
			if ((object)getMethod == null)
			{
				Binder.Error(bag, ErrorCode.ERR_PropertyLacksGet, body.Syntax, propertySymbol);
			}
			else
			{
				bag.ReportUseSite(getMethod, body.Syntax.Location);
			}
		}
	}

	protected override void GenerateControlFields()
	{
		stateField = F.StateMachineField(F.SpecialType(SpecialType.System_Int32), GeneratedNames.MakeStateMachineStateFieldName());
		if (F.ModuleBuilderOpt.GetMethodBodyInstrumentations(method).Kinds.Contains((InstrumentationKind)(-1)))
		{
			instanceIdField = F.StateMachineField(F.SpecialType(SpecialType.System_UInt64), GeneratedNames.MakeStateMachineStateIdFieldName());
		}
		_currentField = F.StateMachineField(_elementType, GeneratedNames.MakeIteratorCurrentFieldName());
	}

	protected override void GenerateMethodImplementations()
	{
		try
		{
			BoundExpression managedThreadId = null;
			GenerateEnumeratorImplementation();
			if (_isEnumerable)
			{
				GenerateEnumerableImplementation(ref managedThreadId);
			}
			GenerateConstructor(managedThreadId);
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
		}
	}

	private void GenerateEnumeratorImplementation()
	{
		MethodSymbol methodToImplement = F.SpecialMethod(SpecialMember.System_IDisposable__Dispose);
		MethodSymbol methodToImplement2 = F.SpecialMethod(SpecialMember.System_Collections_IEnumerator__MoveNext);
		MethodSymbol methodToImplement3 = F.SpecialMethod(SpecialMember.System_Collections_IEnumerator__Reset);
		MethodSymbol getMethod = F.SpecialProperty(SpecialMember.System_Collections_IEnumerator__Current).GetMethod;
		NamedTypeSymbol newOwner = F.SpecialType(SpecialType.System_Collections_Generic_IEnumerator_T).Construct(ImmutableArray.Create(_elementType));
		MethodSymbol getterToImplement = F.SpecialProperty(SpecialMember.System_Collections_Generic_IEnumerator_T__Current).GetMethod.AsMember(newOwner);
		SynthesizedImplementationMethod disposeMethod = OpenMethodImplementation(methodToImplement, null, hasMethodBodyDependency: true);
		SynthesizedImplementationMethod moveNextMethod = OpenMoveNextMethodImplementation(methodToImplement2);
		GenerateMoveNextAndDispose(moveNextMethod, disposeMethod);
		OpenPropertyImplementation(getterToImplement);
		F.CloseMethod(F.Return(F.Field(F.This(), _currentField)));
		OpenMethodImplementation(methodToImplement3);
		F.CloseMethod(F.Throw(F.New(F.WellKnownType(WellKnownType.System_NotSupportedException))));
		OpenPropertyImplementation(getMethod);
		F.CloseMethod(F.Return(F.Field(F.This(), _currentField)));
	}

	private void GenerateEnumerableImplementation(ref BoundExpression managedThreadId)
	{
		MethodSymbol methodToImplement = F.SpecialMethod(SpecialMember.System_Collections_IEnumerable__GetEnumerator);
		NamedTypeSymbol newOwner = F.SpecialType(SpecialType.System_Collections_Generic_IEnumerable_T).Construct(_elementType.Type);
		MethodSymbol getEnumeratorMethod = F.SpecialMethod(SpecialMember.System_Collections_Generic_IEnumerable_T__GetEnumerator).AsMember(newOwner);
		SynthesizedImplementationMethod synthesizedImplementationMethod = GenerateIteratorGetEnumerator(getEnumeratorMethod, ref managedThreadId, StateMachineState.FirstUnusedState);
		OpenMethodImplementation(methodToImplement);
		F.CloseMethod(F.Return(F.Call(F.This(), synthesizedImplementationMethod)));
	}

	private void GenerateConstructor(BoundExpression managedThreadId)
	{
		F.CurrentFunction = stateMachineType.Constructor;
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		instance.Add(F.BaseInitialization());
		instance.Add(F.Assignment(F.Field(F.This(), stateField), F.Parameter(F.CurrentFunction.Parameters[0])));
		if (managedThreadId != null)
		{
			instance.Add(F.Assignment(F.Field(F.This(), initialThreadIdField), managedThreadId));
		}
		if ((object)instanceIdField != null)
		{
			MethodSymbol methodSymbol = F.WellKnownMethod(WellKnownMember.Microsoft_CodeAnalysis_Runtime_LocalStoreTracker__GetNewStateMachineInstanceId);
			if ((object)methodSymbol != null)
			{
				instance.Add(F.Assignment(F.InstanceField(instanceIdField), F.Call(null, methodSymbol)));
			}
		}
		instance.Add(F.Return());
		F.CloseMethod(F.Block(instance.ToImmutableAndFree()));
		instance = null;
	}

	protected override void InitializeStateMachine(ArrayBuilder<BoundStatement> bodyBuilder, NamedTypeSymbol frameType, LocalSymbol stateMachineLocal)
	{
		StateMachineState value = (_isEnumerable ? StateMachineState.FinishedState : StateMachineState.FirstUnusedState);
		bodyBuilder.Add(F.Assignment(F.Local(stateMachineLocal), F.New(stateMachineType.Constructor.AsMember(frameType), F.Literal(value))));
	}

	protected override BoundStatement GenerateStateMachineCreation(LocalSymbol stateMachineVariable, NamedTypeSymbol frameType, IReadOnlyDictionary<Symbol, CapturedSymbolReplacement> proxies)
	{
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		instance.Add(GenerateParameterStorage(stateMachineVariable, proxies));
		instance.Add(F.Return(F.Local(stateMachineVariable)));
		return F.Block(instance.ToImmutableAndFree());
	}

	private void GenerateMoveNextAndDispose(SynthesizedImplementationMethod moveNextMethod, SynthesizedImplementationMethod disposeMethod)
	{
		new IteratorMethodToStateMachineRewriter(F, method, stateField, _currentField, instanceIdField, hoistedVariables, nonReusableLocalProxies, nonReusableFieldsForCleanup, synthesizedLocalOrdinals, stateMachineStateDebugInfoBuilder, slotAllocatorOpt, nextFreeHoistedLocalSlot, diagnostics).GenerateMoveNextAndDispose(body, moveNextMethod, disposeMethod);
	}
}
