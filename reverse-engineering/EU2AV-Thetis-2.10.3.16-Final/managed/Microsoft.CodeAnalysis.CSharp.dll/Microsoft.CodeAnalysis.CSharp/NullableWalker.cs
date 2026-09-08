using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal sealed class NullableWalker : LocalDataFlowPass<NullableWalker.LocalState, NullableWalker.LocalFunctionState>
{
	internal sealed class NullableAnalysisData
	{
		internal readonly int MaxRecursionDepth;

		internal readonly ConcurrentDictionary<object, Data> Data;

		internal NullableAnalysisData(int maxRecursionDepth = -1)
		{
			MaxRecursionDepth = maxRecursionDepth;
			Data = new ConcurrentDictionary<object, Data>();
		}
	}

	internal readonly struct GetterNullResilienceData(SynthesizedBackingFieldSymbol field, NullableAnnotation assumedAnnotation)
	{
		public readonly SynthesizedBackingFieldSymbol field = field;

		public readonly NullableAnnotation assumedAnnotation = assumedAnnotation;

		public void Deconstruct(out SynthesizedBackingFieldSymbol field, out NullableAnnotation assumedAnnotation)
		{
			field = this.field;
			assumedAnnotation = this.assumedAnnotation;
		}
	}

	internal sealed class VariableState
	{
		internal readonly VariablesSnapshot Variables;

		internal readonly LocalStateSnapshot VariableNullableStates;

		internal VariableState(VariablesSnapshot variables, LocalStateSnapshot variableNullableStates)
		{
			Variables = variables;
			VariableNullableStates = variableNullableStates;
		}
	}

	internal readonly struct Data
	{
		internal readonly int TrackedEntries;

		internal readonly bool RequiredAnalysis;

		internal Data(int trackedEntries, bool requiredAnalysis)
		{
			TrackedEntries = trackedEntries;
			RequiredAnalysis = requiredAnalysis;
		}
	}

	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	private readonly struct VisitResult
	{
		public readonly TypeWithState RValueType;

		public readonly TypeWithAnnotations LValueType;

		public readonly Optional<LocalState> StateForLambda;

		public readonly VisitResult[]? NestedVisitResults;

		public VisitResult(TypeWithState rValueType, TypeWithAnnotations lValueType)
		{
			StateForLambda = default(Optional<LocalState>);
			NestedVisitResults = null;
			RValueType = rValueType;
			LValueType = lValueType;
		}

		public VisitResult(TypeWithState rValueType, TypeWithAnnotations lValueType, Optional<LocalState> stateForLambda)
			: this(rValueType, lValueType)
		{
			StateForLambda = stateForLambda;
		}

		public VisitResult(TypeSymbol? type, NullableAnnotation annotation, NullableFlowState state)
		{
			StateForLambda = default(Optional<LocalState>);
			NestedVisitResults = null;
			RValueType = TypeWithState.Create(type, state);
			LValueType = TypeWithAnnotations.Create(type, annotation);
		}

		public VisitResult(TypeWithState rValueType, TypeWithAnnotations lValueType, VisitResult[] nestedVisitResults)
			: this(rValueType, lValueType)
		{
			NestedVisitResults = nestedVisitResults;
		}

		internal VisitResult WithLValueType(TypeWithAnnotations lvalueType)
		{
			if (NestedVisitResults != null)
			{
				return new VisitResult(RValueType, lvalueType, NestedVisitResults);
			}
			return new VisitResult(RValueType, lvalueType, StateForLambda);
		}

		internal string GetDebuggerDisplay()
		{
			if (NestedVisitResults == null)
			{
				return "{LValue: " + LValueType.GetDebuggerDisplay() + ", RValue: " + RValueType.GetDebuggerDisplay() + "}";
			}
			return "Collection: " + string.Join(", ", NestedVisitResults.Select((VisitResult r) => r.GetDebuggerDisplay()));
		}
	}

	private enum AssignmentKind
	{
		Assignment,
		Return,
		Argument,
		ForEachIterationVariable
	}

	private readonly struct CompareExchangeInfo(ImmutableArray<BoundExpression> arguments, ImmutableArray<VisitResult> results, ImmutableArray<int> argsToParamsOpt)
	{
		public readonly ImmutableArray<BoundExpression> Arguments = arguments;

		public readonly ImmutableArray<VisitResult> Results = results;

		public readonly ImmutableArray<int> ArgsToParamsOpt = argsToParamsOpt;

		public bool IsDefault
		{
			get
			{
				if (!Arguments.IsDefault)
				{
					return Results.IsDefault;
				}
				return true;
			}
		}
	}

	private delegate(TMember? member, bool returnNotNull) ArgumentsCompletionDelegate<TMember>(ImmutableArray<VisitResult> argumentResults, ImmutableArray<ParameterSymbol> parametersOpt, TMember? member) where TMember : Symbol;

	private sealed class MethodInferenceExtensions : MethodTypeInferrer.Extensions
	{
		private readonly NullableWalker _walker;

		internal MethodInferenceExtensions(NullableWalker walker)
		{
			_walker = walker;
		}

		internal override TypeWithAnnotations GetTypeWithAnnotations(BoundExpression expr)
		{
			return TypeWithAnnotations.Create(expr.GetTypeOrFunctionType(), GetNullableAnnotation(expr));
		}

		private static NullableAnnotation GetNullableAnnotation(BoundExpression expr)
		{
			switch (expr.Kind)
			{
			case BoundKind.DefaultLiteral:
			case BoundKind.DefaultExpression:
			case BoundKind.Literal:
				if (!(expr.ConstantValueOpt == null) && expr.ConstantValueOpt.IsNull && !expr.IsSuppressed)
				{
					return NullableAnnotation.Annotated;
				}
				return NullableAnnotation.NotAnnotated;
			case BoundKind.ExpressionWithNullability:
				return ((BoundExpressionWithNullability)expr).NullableAnnotation;
			case BoundKind.MethodGroup:
			case BoundKind.UnconvertedObjectCreationExpression:
			case BoundKind.UnconvertedCollectionExpression:
			case BoundKind.ConvertedTupleLiteral:
			case BoundKind.UnboundLambda:
				return NullableAnnotation.NotAnnotated;
			default:
				return NullableAnnotation.Oblivious;
			}
		}

		internal override TypeWithAnnotations GetMethodGroupResultType(BoundMethodGroup group, MethodSymbol method)
		{
			if (_walker.TryGetMethodGroupReceiverNullability(group.ReceiverOpt, out var type) && !method.IsStatic)
			{
				method = (MethodSymbol)AsMemberOfType(type.Type, method);
			}
			return method.ReturnTypeWithAnnotations;
		}
	}

	private readonly struct DeconstructionVariable
	{
		internal readonly BoundExpression Expression;

		internal readonly TypeWithAnnotations Type;

		internal readonly ArrayBuilder<DeconstructionVariable>? NestedVariables;

		internal DeconstructionVariable(BoundExpression expression, TypeWithAnnotations type)
		{
			Expression = expression;
			Type = type;
			NestedVariables = null;
		}

		internal DeconstructionVariable(BoundExpression expression, ArrayBuilder<DeconstructionVariable> nestedVariables)
		{
			Expression = expression;
			Type = default(TypeWithAnnotations);
			NestedVariables = nestedVariables;
		}
	}

	internal sealed class LocalStateSnapshot
	{
		internal readonly int Id;

		internal readonly LocalStateSnapshot? Container;

		internal readonly BitVector State;

		internal LocalStateSnapshot(int id, LocalStateSnapshot? container, BitVector state)
		{
			Id = id;
			Container = container;
			State = state;
		}
	}

	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	internal struct LocalState : ILocalDataFlowState, ILocalState
	{
		private sealed class Boxed
		{
			internal LocalState Value;

			internal Boxed(LocalState value)
			{
				Value = value;
			}
		}

		internal readonly int Id;

		private readonly Boxed? _container;

		private BitVector _state;

		public bool Reachable => _state[0];

		public bool NormalizeToBottom => false;

		private int Capacity => _state.Capacity / 2;

		public NullableFlowState this[int slot]
		{
			get
			{
				var (id, index) = Variables.DeconstructSlot(slot);
				return GetValue(id, index);
			}
			set
			{
				var (id, index) = Variables.DeconstructSlot(slot);
				SetValue(id, index, value);
			}
		}

		private LocalState(int id, Boxed? container, BitVector state)
		{
			Id = id;
			_container = container;
			_state = state;
		}

		internal static LocalState Create(LocalStateSnapshot snapshot)
		{
			Boxed container = ((snapshot.Container == null) ? null : new Boxed(Create(snapshot.Container)));
			return new LocalState(snapshot.Id, container, snapshot.State.Clone());
		}

		internal LocalStateSnapshot CreateSnapshot()
		{
			return new LocalStateSnapshot(Id, _container?.Value.CreateSnapshot(), _state.Clone());
		}

		public static LocalState ReachableState(Variables variables)
		{
			return CreateReachableOrUnreachableState(variables, reachable: true);
		}

		public static LocalState UnreachableState(Variables variables)
		{
			return CreateReachableOrUnreachableState(variables, reachable: false);
		}

		public static LocalState ReachableStateWithNotNulls(Variables variables)
		{
			Boxed container = ((variables.Container == null) ? null : new Boxed(ReachableStateWithNotNulls(variables.Container)));
			int nextAvailableIndex = variables.NextAvailableIndex;
			return new LocalState(variables.Id, container, createBitVectorWithNotNulls(nextAvailableIndex, reachable: true));
			static BitVector createBitVectorWithNotNulls(int capacity, bool reachable)
			{
				BitVector result = BitVector.Create(capacity * 2);
				result[0] = reachable;
				for (int i = 1; i < capacity; i++)
				{
					int num = i * 2;
					result[num] = true;
					result[num + 1] = true;
				}
				return result;
			}
		}

		private static LocalState CreateReachableOrUnreachableState(Variables variables, bool reachable)
		{
			Boxed container = ((variables.Container == null) ? null : new Boxed(CreateReachableOrUnreachableState(variables.Container, reachable)));
			return new LocalState(variables.Id, container, CreateBitVector(reachable));
		}

		public LocalState CreateNestedMethodState(Variables variables)
		{
			return new LocalState(variables.Id, new Boxed(this), CreateBitVector(reachable: true));
		}

		private static BitVector CreateBitVector(bool reachable)
		{
			BitVector result = BitVector.Create(2);
			result[0] = reachable;
			return result;
		}

		private void EnsureCapacity(int capacity)
		{
			_state.EnsureCapacity(capacity * 2);
		}

		public bool HasVariable(int slot)
		{
			if (slot <= 0)
			{
				return false;
			}
			var (id, index) = Variables.DeconstructSlot(slot);
			return hasVariableCore(ref this, id, index);
			static bool hasVariableCore(ref LocalState state, int num, int index2)
			{
				if (state.Id > num)
				{
					return hasVariableCore(ref state._container.Value, num, index2);
				}
				return state.Id == num;
			}
		}

		public void NormalizeIfNeeded(int slot, NullableWalker walker, Variables variables, bool useNotNullsAsDefault = false)
		{
			if (!hasValue(ref this, slot))
			{
				Normalize(walker, variables, useNotNullsAsDefault);
			}
			static bool hasValue(ref LocalState state, int num)
			{
				if (num <= 0)
				{
					return false;
				}
				var (id, index) = Variables.DeconstructSlot(num);
				return hasValueCore(ref state, id, index);
			}
			static bool hasValueCore(ref LocalState state, int id, int index)
			{
				if (state.Id != id)
				{
					return hasValueCore(ref state._container.Value, id, index);
				}
				return index < state.Capacity;
			}
		}

		public void Normalize(NullableWalker walker, Variables variables, bool useNotNullsAsDefault = false)
		{
			if (Id != variables.Id)
			{
				Normalize(walker, variables.Container, useNotNullsAsDefault);
				return;
			}
			_container?.Value.Normalize(walker, variables.Container, useNotNullsAsDefault);
			int capacity = Capacity;
			EnsureCapacity(variables.NextAvailableIndex);
			Populate(walker, capacity, useNotNullsAsDefault);
		}

		public void PopulateAll(NullableWalker walker)
		{
			_container?.Value.PopulateAll(walker);
			Populate(walker, 1, useNotNullsAsDefault: false);
		}

		private void Populate(NullableWalker walker, int start, bool useNotNullsAsDefault)
		{
			int capacity = Capacity;
			for (int i = start; i < capacity; i++)
			{
				int slot = Variables.ConstructSlot(Id, i);
				SetValue(Id, i, (!useNotNullsAsDefault) ? walker.GetDefaultState(ref this, slot) : NullableFlowState.NotNull);
			}
		}

		private NullableFlowState GetValue(int id, int index)
		{
			if (Id != id)
			{
				return _container.Value.GetValue(id, index);
			}
			return GetValue(index);
		}

		private NullableFlowState GetValue(int index)
		{
			if (!Reachable)
			{
				return NullableFlowState.NotNull;
			}
			index *= 2;
			bool num = _state[index];
			bool flag = _state[index + 1];
			if (!num)
			{
				if (!flag)
				{
					return NullableFlowState.NotNull;
				}
				return NullableFlowState.MaybeDefault;
			}
			if (!flag)
			{
				return NullableFlowState.MaybeNull;
			}
			return NullableFlowState.NotNull;
		}

		private void SetValue(int id, int index, NullableFlowState value)
		{
			if (Id != id)
			{
				_container.Value.SetValue(id, index, value);
			}
			else
			{
				SetValue(index, value);
			}
		}

		private void SetValue(int index, NullableFlowState value)
		{
			if (Reachable)
			{
				index *= 2;
				(_state[index], _state[index + 1]) = value switch
				{
					NullableFlowState.MaybeNull => (true, false), 
					NullableFlowState.MaybeDefault => (false, true), 
					NullableFlowState.NotNull => (true, true), 
					_ => throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/NullableWalker.cs", 13440), 
				};
			}
		}

		internal void ForEach<TArg>(Action<int, TArg> action, TArg arg)
		{
			_container?.Value.ForEach(action, arg);
			for (int i = 1; i < Capacity; i++)
			{
				action(Variables.ConstructSlot(Id, i), arg);
			}
		}

		internal LocalState GetStateForVariables(int id)
		{
			LocalState result = this;
			while (result.Id != id)
			{
				result = result._container.Value;
			}
			return result;
		}

		public LocalState Clone()
		{
			Boxed container = ((_container == null) ? null : new Boxed(_container.Value.Clone()));
			return new LocalState(Id, container, _state.Clone());
		}

		public bool Join(in LocalState other)
		{
			bool flag = false;
			if (_container != null && _container.Value.Join(in other._container.Value))
			{
				flag = true;
			}
			bool reachable = Reachable;
			bool flag2 = reachable | other.Reachable;
			_state[0] = flag2;
			flag |= reachable != flag2;
			for (int i = 1; i < Capacity; i++)
			{
				NullableFlowState nullableFlowState = (reachable ? GetValue(i) : NullableFlowState.NotNull);
				NullableFlowState nullableFlowState2 = nullableFlowState.Join(other.GetValue(i));
				SetValue(i, nullableFlowState2);
				flag |= nullableFlowState != nullableFlowState2;
			}
			return flag;
		}

		public bool Meet(in LocalState other)
		{
			bool flag = false;
			if (_container != null && _container.Value.Meet(in other._container.Value))
			{
				flag = true;
			}
			bool reachable = Reachable;
			bool flag2 = reachable & other.Reachable;
			_state[0] = flag2;
			flag |= reachable != flag2;
			for (int i = 1; i < Capacity; i++)
			{
				NullableFlowState value = GetValue(i);
				NullableFlowState nullableFlowState = value.Meet(other.GetValue(i));
				SetValue(i, nullableFlowState);
				flag |= value != nullableFlowState;
			}
			return flag;
		}

		internal string GetDebuggerDisplay()
		{
			PooledStringBuilder instance = PooledStringBuilder.GetInstance();
			StringBuilder builder = instance.Builder;
			builder.Append(' ');
			for (int num = Math.Min(Capacity, 8) - 1; num >= 0; num--)
			{
				NullableFlowState value = GetValue(num);
				bool flag = ((value == NullableFlowState.MaybeNull || value == NullableFlowState.MaybeDefault) ? true : false);
				bool flag2 = flag;
				builder.Append(flag2 ? '?' : '!');
			}
			return instance.ToStringAndFree();
		}

		internal string Dump(Variables variables)
		{
			if (!Reachable)
			{
				return "unreachable";
			}
			if (Id != variables.Id)
			{
				return "invalid";
			}
			PooledStringBuilder instance = PooledStringBuilder.GetInstance();
			Dump(instance, variables);
			return instance.ToStringAndFree();
		}

		private void Dump(StringBuilder builder, Variables variables)
		{
			_container?.Value.Dump(builder, variables.Container);
			for (int i = 1; i < Capacity; i++)
			{
				string text = getName(Variables.ConstructSlot(Id, i));
				if (text != null)
				{
					builder.Append(text);
					builder.Append(GetValue(Id, i) switch
					{
						NullableFlowState.MaybeNull => "?", 
						NullableFlowState.MaybeDefault => "??", 
						_ => "!", 
					});
				}
			}
			string? getName(int slot)
			{
				VariableIdentifier variableIdentifier = variables[slot];
				string name = variableIdentifier.Symbol.Name;
				int containingSlot = variableIdentifier.ContainingSlot;
				if (containingSlot <= 0)
				{
					return name;
				}
				return getName(containingSlot) + "." + name;
			}
		}
	}

	internal sealed class LocalFunctionState : AbstractLocalFunctionState
	{
		public LocalState StartingState;

		public LocalFunctionState(LocalState unreachableState)
			: base(unreachableState.Clone(), unreachableState.Clone())
		{
			StartingState = unreachableState;
		}
	}

	private sealed class NullabilityInfoTypeComparer : IEqualityComparer<(NullabilityInfo info, TypeSymbol? type)>
	{
		public static readonly NullabilityInfoTypeComparer Instance = new NullabilityInfoTypeComparer();

		public bool Equals((NullabilityInfo info, TypeSymbol? type) x, (NullabilityInfo info, TypeSymbol? type) y)
		{
			if (x.info.Equals(y.info))
			{
				return Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(x.type, y.type);
			}
			return false;
		}

		public int GetHashCode((NullabilityInfo info, TypeSymbol? type) obj)
		{
			return obj.GetHashCode();
		}
	}

	private sealed class ExpressionAndSymbolEqualityComparer : IEqualityComparer<(BoundNode? expr, Symbol symbol)>
	{
		internal static readonly ExpressionAndSymbolEqualityComparer Instance = new ExpressionAndSymbolEqualityComparer();

		private ExpressionAndSymbolEqualityComparer()
		{
		}

		public bool Equals((BoundNode? expr, Symbol symbol) x, (BoundNode? expr, Symbol symbol) y)
		{
			if (x.expr == y.expr)
			{
				return (object)x.symbol == y.symbol;
			}
			return false;
		}

		public int GetHashCode((BoundNode? expr, Symbol symbol) obj)
		{
			return Hash.Combine(obj.expr, obj.symbol.GetHashCode());
		}
	}

	private sealed class PlaceholderLocal : LocalSymbol
	{
		private readonly Symbol _containingSymbol;

		private readonly TypeWithAnnotations _type;

		private readonly object _identifier;

		internal override SyntaxNode ScopeDesignatorOpt => null;

		public override Symbol ContainingSymbol => _containingSymbol;

		public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

		public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

		public override TypeWithAnnotations TypeWithAnnotations => _type;

		internal override LocalDeclarationKind DeclarationKind => LocalDeclarationKind.None;

		internal override SyntaxToken IdentifierToken
		{
			get
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/NullableWalker.PlaceholderLocal.cs", 54);
			}
		}

		internal override bool IsCompilerGenerated => true;

		internal override bool IsImportedFromMetadata => false;

		internal override bool IsPinned => false;

		internal override bool IsKnownToReferToTempIfReferenceType => false;

		public override RefKind RefKind => RefKind.None;

		internal override SynthesizedLocalKind SynthesizedKind
		{
			get
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/NullableWalker.PlaceholderLocal.cs", 60);
			}
		}

		internal override bool HasSourceLocation => false;

		internal override ScopedKind Scope => ScopedKind.None;

		public PlaceholderLocal(Symbol containingSymbol, object identifier, TypeWithAnnotations type)
		{
			_containingSymbol = containingSymbol;
			_type = type;
			_identifier = identifier;
		}

		public override bool Equals(Symbol obj, TypeCompareKind compareKind)
		{
			if ((object)this == obj)
			{
				return true;
			}
			if (obj is PlaceholderLocal placeholderLocal)
			{
				return _identifier.Equals(placeholderLocal._identifier);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return _identifier.GetHashCode();
		}

		internal override ConstantValue GetConstantValue(SyntaxNode node, LocalSymbol inProgress, BindingDiagnosticBag diagnostics = null)
		{
			return null;
		}

		internal override ReadOnlyBindingDiagnostic<AssemblySymbol> GetConstantValueDiagnostics(BoundExpression boundInitValue)
		{
			return ReadOnlyBindingDiagnostic<AssemblySymbol>.Empty;
		}

		internal override SyntaxNode GetDeclaratorSyntax()
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/NullableWalker.PlaceholderLocal.cs", 63);
		}

		internal override LocalSymbol WithSynthesizedLocalKindAndSyntax(SynthesizedLocalKind kind, SyntaxNode syntax)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/NullableWalker.PlaceholderLocal.cs", 72);
		}
	}

	internal sealed class SnapshotManager
	{
		internal sealed class Builder
		{
			private readonly ImmutableDictionary<(BoundNode?, Symbol), Symbol>.Builder _updatedSymbolMap = ImmutableDictionary.CreateBuilder(ExpressionAndSymbolEqualityComparer.Instance, Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything);

			private readonly ArrayBuilder<SharedWalkerState> _walkerStates = ArrayBuilder<SharedWalkerState>.GetInstance();

			private readonly SortedDictionary<int, Snapshot> _incrementalSnapshots = new SortedDictionary<int, Snapshot>();

			private readonly PooledDictionary<Symbol, int> _symbolToSlot = PooledDictionary<Symbol, int>.GetInstance();

			private int _currentWalkerSlot = -1;

			internal SnapshotManager ToManagerAndFree()
			{
				_symbolToSlot.Free();
				ImmutableArray<(int, Snapshot)> incrementalSnapshots = _incrementalSnapshots.SelectAsArray((KeyValuePair<int, Snapshot> kvp) => (kvp.Key, kvp.Value));
				ImmutableDictionary<(BoundNode, Symbol), Symbol> updatedSymbolsMap = _updatedSymbolMap.ToImmutable();
				return new SnapshotManager(_walkerStates.ToImmutableAndFree(), incrementalSnapshots, updatedSymbolsMap);
			}

			internal int EnterNewWalker(Symbol symbol)
			{
				int currentWalkerSlot = _currentWalkerSlot;
				if (_symbolToSlot.TryGetValue(symbol, out var value))
				{
					_currentWalkerSlot = value;
					return currentWalkerSlot;
				}
				_currentWalkerSlot = _symbolToSlot.Count;
				_symbolToSlot.Add(symbol, _currentWalkerSlot);
				return currentWalkerSlot;
			}

			internal void ExitWalker(SharedWalkerState stableState, int previousSlot)
			{
				_walkerStates.SetItem(_currentWalkerSlot, stableState);
				_currentWalkerSlot = previousSlot;
			}

			internal void TakeIncrementalSnapshot(BoundNode? node, LocalState currentState)
			{
				if (node != null && !node.WasCompilerGenerated)
				{
					_incrementalSnapshots[node.Syntax.SpanStart] = new Snapshot(currentState.CreateSnapshot(), _currentWalkerSlot);
				}
			}

			internal void SetUpdatedSymbol(BoundNode node, Symbol originalSymbol, Symbol updatedSymbol)
			{
				_updatedSymbolMap[GetKey(node, originalSymbol)] = updatedSymbol;
			}

			internal void RemoveSymbolIfPresent(BoundNode node, Symbol symbol)
			{
				_updatedSymbolMap.Remove(GetKey(node, symbol));
			}

			private static (BoundNode?, Symbol) GetKey(BoundNode node, Symbol symbol)
			{
				if (node is BoundLambda && symbol is LambdaSymbol)
				{
					return (null, symbol);
				}
				return (node, symbol);
			}
		}

		private readonly ImmutableArray<SharedWalkerState> _walkerSharedStates;

		private readonly ImmutableArray<(int position, Snapshot snapshot)> _incrementalSnapshots;

		private readonly ImmutableDictionary<(BoundNode?, Symbol), Symbol> _updatedSymbolsMap;

		private static readonly Func<(int position, Snapshot snapshot), int, int> BinarySearchComparer = ((int position, Snapshot snapshot) current, int target) => current.position.CompareTo(target);

		private SnapshotManager(ImmutableArray<SharedWalkerState> walkerSharedStates, ImmutableArray<(int position, Snapshot snapshot)> incrementalSnapshots, ImmutableDictionary<(BoundNode?, Symbol), Symbol> updatedSymbolsMap)
		{
			_walkerSharedStates = walkerSharedStates;
			_incrementalSnapshots = incrementalSnapshots;
			_updatedSymbolsMap = updatedSymbolsMap;
		}

		internal (VariablesSnapshot, LocalStateSnapshot) GetSnapshot(int position)
		{
			Snapshot snapshotForPosition = GetSnapshotForPosition(position);
			return (_walkerSharedStates[snapshotForPosition.SharedStateIndex].Variables, snapshotForPosition.VariableState);
		}

		internal TypeWithAnnotations? GetUpdatedTypeForLocalSymbol(SourceLocalSymbol symbol)
		{
			Snapshot snapshotForPosition = GetSnapshotForPosition(symbol.IdentifierToken.SpanStart);
			if (_walkerSharedStates[snapshotForPosition.SharedStateIndex].Variables.TryGetType(symbol, out var type))
			{
				return type;
			}
			return null;
		}

		internal NamedTypeSymbol? GetUpdatedDelegateTypeForLambda(LambdaSymbol lambda)
		{
			if (_updatedSymbolsMap.TryGetValue((null, lambda), out Symbol value))
			{
				return (NamedTypeSymbol)value;
			}
			return null;
		}

		internal bool TryGetUpdatedSymbol(BoundNode node, Symbol symbol, [NotNullWhen(true)] out Symbol? updatedSymbol)
		{
			return _updatedSymbolsMap.TryGetValue((node, symbol), out updatedSymbol);
		}

		private Snapshot GetSnapshotForPosition(int position)
		{
			int num = _incrementalSnapshots.BinarySearch(position, BinarySearchComparer);
			if (num < 0)
			{
				num = ~num - 1;
				if (num < 0)
				{
					num = 0;
				}
			}
			return _incrementalSnapshots[num].snapshot;
		}
	}

	internal readonly struct SharedWalkerState
	{
		internal readonly VariablesSnapshot Variables;

		internal SharedWalkerState(VariablesSnapshot variables)
		{
			Variables = variables;
		}
	}

	private readonly struct Snapshot
	{
		internal readonly LocalStateSnapshot VariableState;

		internal readonly int SharedStateIndex;

		internal Snapshot(LocalStateSnapshot variableState, int sharedStateIndex)
		{
			VariableState = variableState;
			SharedStateIndex = sharedStateIndex;
		}
	}

	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	internal sealed class VariablesSnapshot
	{
		internal readonly int Id;

		internal readonly VariablesSnapshot? Container;

		internal readonly Symbol? Symbol;

		internal readonly ImmutableArray<KeyValuePair<VariableIdentifier, int>> VariableSlot;

		internal readonly ImmutableDictionary<Symbol, TypeWithAnnotations> VariableTypes;

		internal VariablesSnapshot(int id, VariablesSnapshot? container, Symbol? symbol, ImmutableArray<KeyValuePair<VariableIdentifier, int>> variableSlot, ImmutableDictionary<Symbol, TypeWithAnnotations> variableTypes)
		{
			Id = id;
			Container = container;
			Symbol = symbol;
			VariableSlot = variableSlot;
			VariableTypes = variableTypes;
		}

		internal bool TryGetType(Symbol symbol, out TypeWithAnnotations type)
		{
			return VariableTypes.TryGetValue(symbol, out type);
		}

		private string GetDebuggerDisplay()
		{
			object arg = ((object)Symbol) ?? ((object)"<null>");
			return $"Id={Id}, Symbol={arg}, Count={VariableSlot.Length}";
		}
	}

	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	internal sealed class Variables
	{
		private const int MaxSlotDepth = 5;

		private const int IdOffset = 16;

		private const int IdMask = 32767;

		private const int IndexMask = 65535;

		internal readonly int Id;

		internal readonly Variables? Container;

		internal readonly Symbol? Symbol;

		private readonly PooledDictionary<VariableIdentifier, int> _variableSlot = PooledDictionary<LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier, int>.GetInstance();

		private readonly PooledDictionary<Symbol, TypeWithAnnotations> _variableTypes = SpecializedSymbolCollections.GetPooledSymbolDictionaryInstance<Symbol, TypeWithAnnotations>();

		private readonly ArrayBuilder<VariableIdentifier> _variableBySlot = ArrayBuilder<LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier>.GetInstance(1, default(LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier));

		internal VariableIdentifier this[int slot]
		{
			get
			{
				var (id, index) = DeconstructSlot(slot);
				return GetVariablesForId(id)._variableBySlot[index];
			}
		}

		internal int NextAvailableIndex => _variableBySlot.Count;

		internal static Variables Create(Symbol? symbol)
		{
			return new Variables(0, null, symbol);
		}

		internal static Variables Create(VariablesSnapshot snapshot)
		{
			Variables container = ((snapshot.Container == null) ? null : Create(snapshot.Container));
			Variables variables = new Variables(snapshot.Id, container, snapshot.Symbol);
			variables.Populate(snapshot);
			return variables;
		}

		private int GetNextId()
		{
			return Id + 1;
		}

		private void Populate(VariablesSnapshot snapshot)
		{
			_variableBySlot.AddMany(default(LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier), snapshot.VariableSlot.Length);
			foreach (KeyValuePair<LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier, int> item in snapshot.VariableSlot)
			{
				LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier key = item.Key;
				int value = item.Value;
				_variableSlot.Add(key, value);
				_variableBySlot[value] = key;
			}
			foreach (KeyValuePair<Symbol, TypeWithAnnotations> variableType in snapshot.VariableTypes)
			{
				_variableTypes.Add(variableType.Key, variableType.Value);
			}
		}

		private Variables(int id, Variables? container, Symbol? symbol)
		{
			Id = id;
			Container = container;
			Symbol = symbol;
		}

		internal void Free()
		{
			Container?.Free();
			_variableBySlot.Free();
			_variableTypes.Free();
			_variableSlot.Free();
		}

		internal VariablesSnapshot CreateSnapshot()
		{
			return new VariablesSnapshot(Id, Container?.CreateSnapshot(), Symbol, ImmutableArray.CreateRange(_variableSlot), ImmutableDictionary.CreateRange(_variableTypes));
		}

		internal Variables CreateNestedMethodScope(MethodSymbol method)
		{
			return new Variables(GetNextId(), this, method);
		}

		internal int RootSlot(int slot)
		{
			while (true)
			{
				int containingSlot = this[slot].ContainingSlot;
				if (containingSlot == 0)
				{
					break;
				}
				slot = containingSlot;
			}
			return slot;
		}

		internal bool TryGetValue(VariableIdentifier identifier, out int slot)
		{
			return GetVariablesForVariable(identifier).TryGetValueInternal(identifier, out slot);
		}

		private bool TryGetValueInternal(VariableIdentifier identifier, out int slot)
		{
			if (_variableSlot.TryGetValue(identifier, out var value))
			{
				slot = ConstructSlot(Id, value);
				return true;
			}
			slot = -1;
			return false;
		}

		internal int Add(VariableIdentifier identifier)
		{
			return GetVariablesForVariable(identifier).AddInternal(identifier);
		}

		private int AddInternal(VariableIdentifier identifier)
		{
			if (getSlotDepth(identifier.ContainingSlot) >= 5)
			{
				return -1;
			}
			int nextAvailableIndex = NextAvailableIndex;
			if (nextAvailableIndex > 65535)
			{
				return -1;
			}
			_variableSlot.Add(identifier, nextAvailableIndex);
			_variableBySlot.Add(identifier);
			return ConstructSlot(Id, nextAvailableIndex);
			int getSlotDepth(int slot)
			{
				int num = 0;
				while (slot > 0)
				{
					num++;
					int item = DeconstructSlot(slot).Index;
					slot = _variableBySlot[item].ContainingSlot;
				}
				return num;
			}
		}

		internal bool TryGetType(Symbol symbol, out TypeWithAnnotations type)
		{
			return GetVariablesContainingSymbol(symbol)._variableTypes.TryGetValue(symbol, out type);
		}

		internal void SetType(Symbol symbol, TypeWithAnnotations type)
		{
			GetVariablesContainingSymbol(symbol)._variableTypes[symbol] = type;
		}

		internal int GetTotalVariableCount()
		{
			return (Container?.GetTotalVariableCount() ?? 0) + _variableSlot.Count;
		}

		internal void GetMembers(ArrayBuilder<(VariableIdentifier, int)> builder, int containingSlot)
		{
			(int Id, int Index) tuple = DeconstructSlot(containingSlot);
			int item = tuple.Id;
			int item2 = tuple.Index;
			ArrayBuilder<LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier> variableBySlot = GetVariablesForId(item)._variableBySlot;
			for (item2++; item2 < variableBySlot.Count; item2++)
			{
				LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier item3 = variableBySlot[item2];
				if (item3.ContainingSlot == containingSlot)
				{
					builder.Add((item3, ConstructSlot(item, item2)));
				}
			}
		}

		private Variables GetVariablesForVariable(VariableIdentifier identifier)
		{
			int containingSlot = identifier.ContainingSlot;
			if (containingSlot > 0)
			{
				return GetVariablesForId(DeconstructSlot(containingSlot).Id);
			}
			return GetVariablesContainingSymbol(identifier.Symbol);
		}

		private Variables GetVariablesContainingSymbol(Symbol symbol)
		{
			if ((symbol is LocalSymbol || symbol is ParameterSymbol) && symbol.ContainingSymbol is MethodSymbol method)
			{
				Variables variablesForMethodScope = GetVariablesForMethodScope(method);
				if (variablesForMethodScope != null)
				{
					return variablesForMethodScope;
				}
			}
			return GetRootScope();
		}

		internal Variables GetRootScope()
		{
			Variables variables = this;
			while (true)
			{
				Variables container = variables.Container;
				if (container == null)
				{
					break;
				}
				variables = container;
			}
			return variables;
		}

		private Variables? GetVariablesForId(int id)
		{
			Variables variables = this;
			do
			{
				if (variables.Id == id)
				{
					return variables;
				}
				variables = variables.Container;
			}
			while (variables != null);
			return null;
		}

		internal Variables? GetVariablesForMethodScope(MethodSymbol method)
		{
			Variables variables = this;
			do
			{
				if ((object)method == variables.Symbol)
				{
					return variables;
				}
				variables = variables.Container;
			}
			while (variables != null);
			return null;
		}

		internal static int ConstructSlot(int id, int index)
		{
			if (index >= 0)
			{
				return (id << 16) | index;
			}
			return index;
		}

		internal static (int Id, int Index) DeconstructSlot(int slot)
		{
			if (slot >= 0)
			{
				return (Id: (slot >> 16) & 0x7FFF, Index: slot & 0xFFFF);
			}
			return (Id: 0, Index: slot);
		}

		private string GetDebuggerDisplay()
		{
			object arg = ((object)Symbol) ?? ((object)"<null>");
			return $"Id={Id}, Symbol={arg}, Count={_variableSlot.Count}";
		}
	}

	private struct PossiblyConditionalState
	{
		public LocalState State;

		public LocalState StateWhenTrue;

		public LocalState StateWhenFalse;

		public bool IsConditionalState;

		public PossiblyConditionalState(LocalState stateWhenTrue, LocalState stateWhenFalse)
		{
			StateWhenTrue = stateWhenTrue.Clone();
			StateWhenFalse = stateWhenFalse.Clone();
			IsConditionalState = true;
			State = default(LocalState);
		}

		public PossiblyConditionalState(LocalState state)
		{
			StateWhenTrue = (StateWhenFalse = default(LocalState));
			IsConditionalState = false;
			State = state.Clone();
		}

		public static PossiblyConditionalState Create(NullableWalker nullableWalker)
		{
			if (!nullableWalker.IsConditionalState)
			{
				return new PossiblyConditionalState(nullableWalker.State);
			}
			return new PossiblyConditionalState(nullableWalker.StateWhenTrue, nullableWalker.StateWhenFalse);
		}

		public PossiblyConditionalState Clone()
		{
			if (!IsConditionalState)
			{
				return new PossiblyConditionalState(State);
			}
			return new PossiblyConditionalState(StateWhenTrue, StateWhenFalse);
		}
	}

	private Variables _variables;

	private readonly Binder _binder;

	private readonly Conversions _conversions;

	private readonly bool _useConstructorExitWarnings;

	private readonly GetterNullResilienceData? _getterNullResilienceData;

	private bool _useDelegateInvokeParameterTypes;

	private bool _useDelegateInvokeReturnType;

	private MethodSymbol? _delegateInvokeMethod;

	private ArrayBuilder<(BoundReturnStatement, TypeWithAnnotations)>? _returnTypesOpt;

	private static readonly TypeWithState _invalidType = TypeWithState.Create(new UnsupportedMetadataTypeSymbol(), NullableFlowState.NotNull);

	private readonly ImmutableDictionary<BoundExpression, (NullabilityInfo Info, TypeSymbol? Type)>.Builder? _analyzedNullabilityMapOpt;

	private readonly SnapshotManager.Builder? _snapshotBuilderOpt;

	private bool _disableNullabilityAnalysis;

	private PooledDictionary<BoundExpression, TypeWithState>? _methodGroupReceiverMapOpt;

	private PooledDictionary<BoundValuePlaceholderBase, (BoundExpression? Replacement, VisitResult Result)>? _resultForPlaceholdersOpt;

	private PooledDictionary<MethodSymbol, Variables>? _nestedFunctionVariables;

	private PooledDictionary<BoundExpression, Func<TypeWithAnnotations, TypeWithState>>? _targetTypedAnalysisCompletionOpt;

	private readonly bool _isSpeculative;

	private readonly bool _hasInitialState;

	private readonly MethodSymbol? _baseOrThisInitializer;

	private VisitResult _visitResult;

	private VisitResult _currentConditionalReceiverVisitResult;

	private PooledDictionary<object, PlaceholderLocal>? _placeholderLocalsOpt;

	private bool _disableDiagnostics;

	private bool _expressionIsRead = true;

	private int _lastConditionalAccessSlot = -1;

	private PooledDictionary<BoundExpression, Func<TypeWithAnnotations, TypeWithState>> TargetTypedAnalysisCompletion => _targetTypedAnalysisCompletionOpt ?? (_targetTypedAnalysisCompletionOpt = PooledDictionary<BoundExpression, Func<TypeWithAnnotations, TypeWithState>>.GetInstance());

	private TypeWithState ResultType => _visitResult.RValueType;

	private TypeWithAnnotations LvalueResultType => _visitResult.LValueType;

	private bool IsAnalyzingAttribute => methodMainNode.Kind == BoundKind.Attribute;

	public sealed override bool AwaitUsingAndForeachAddsPendingBranch => true;

	private void SetResultType(BoundExpression? expression, TypeWithState type, bool updateAnalyzedNullability = true)
	{
		SetResult(expression, type, type.ToTypeWithAnnotations(compilation), updateAnalyzedNullability);
	}

	private void SetAnalyzedNullability(BoundExpression? expression, TypeWithState type)
	{
		SetAnalyzedNullability(expression, type, type.ToTypeWithAnnotations(compilation));
	}

	private void UseRvalueOnly(BoundExpression? expression)
	{
		VisitResult visitResult = _visitResult.WithLValueType(ResultType.ToTypeWithAnnotations(compilation));
		SetResult(expression, visitResult, updateAnalyzedNullability: true, false);
	}

	private void SetLvalueResultType(BoundExpression? expression, TypeWithAnnotations type)
	{
		SetResult(expression, type.ToTypeWithState(), type);
	}

	private void UseLvalueOnly(BoundExpression? expression)
	{
		SetResult(expression, LvalueResultType.ToTypeWithState(), LvalueResultType, updateAnalyzedNullability: true, true);
	}

	private void SetInvalidResult()
	{
		SetResult(null, _invalidType, _invalidType.ToTypeWithAnnotations(compilation), updateAnalyzedNullability: false);
	}

	private void SetResult(BoundExpression? expression, TypeWithState resultType, TypeWithAnnotations lvalueType, bool updateAnalyzedNullability = true, bool? isLvalue = null)
	{
		SetResult(expression, new VisitResult(resultType, lvalueType), updateAnalyzedNullability, isLvalue);
	}

	private void SetResult(BoundExpression? expression, VisitResult visitResult, bool updateAnalyzedNullability, bool? isLvalue)
	{
		_visitResult = visitResult;
		if (updateAnalyzedNullability)
		{
			SetAnalyzedNullability(expression, _visitResult, isLvalue);
		}
	}

	private void SetAnalyzedNullability(BoundExpression? expression, TypeWithState resultType, TypeWithAnnotations lvalueType, bool? isLvalue = null)
	{
		SetAnalyzedNullability(expression, new VisitResult(resultType, lvalueType), isLvalue);
	}

	private void SetAnalyzedNullability(BoundExpression? expr, VisitResult result, bool? isLvalue = null)
	{
		if (expr != null && expr.Kind != BoundKind.ExpressionWithNullability && !_disableNullabilityAnalysis && _analyzedNullabilityMapOpt != null)
		{
			ImmutableDictionary<BoundExpression, (NullabilityInfo Info, TypeSymbol? Type)>.Builder? analyzedNullabilityMapOpt = _analyzedNullabilityMapOpt;
			NullabilityInfo item = new NullabilityInfo(result.LValueType.ToPublicAnnotation(), result.RValueType.State.ToPublicFlowState());
			TypeSymbol? type = expr.Type;
			analyzedNullabilityMapOpt[expr] = (item, ((object)type != null && type.Equals(result.RValueType.Type, TypeCompareKind.AllIgnoreOptions)) ? result.RValueType.Type : expr.Type);
		}
	}

	protected override void Free()
	{
		_nestedFunctionVariables?.Free();
		_resultForPlaceholdersOpt?.Free();
		_methodGroupReceiverMapOpt?.Free();
		_placeholderLocalsOpt?.Free();
		_variables.Free();
		_targetTypedAnalysisCompletionOpt?.Free();
		base.Free();
	}

	private NullableWalker(CSharpCompilation compilation, Symbol? symbol, bool useConstructorExitWarnings, GetterNullResilienceData? getterNullResilienceData, bool useDelegateInvokeParameterTypes, bool useDelegateInvokeReturnType, MethodSymbol? delegateInvokeMethodOpt, BoundNode node, Binder binder, Conversions conversions, Variables? variables, MethodSymbol? baseOrThisInitializer, ArrayBuilder<(BoundReturnStatement, TypeWithAnnotations)>? returnTypesOpt, ImmutableDictionary<BoundExpression, (NullabilityInfo, TypeSymbol?)>.Builder? analyzedNullabilityMapOpt, SnapshotManager.Builder? snapshotBuilderOpt, bool isSpeculative = false)
		: base(compilation, symbol, node, EmptyStructTypeCache.CreatePrecise(), true)
	{
		_variables = variables ?? Variables.Create(symbol);
		_binder = binder;
		_conversions = conversions.WithNullability(includeNullability: true);
		_useConstructorExitWarnings = useConstructorExitWarnings;
		_getterNullResilienceData = getterNullResilienceData;
		_useDelegateInvokeParameterTypes = useDelegateInvokeParameterTypes;
		_useDelegateInvokeReturnType = useDelegateInvokeReturnType;
		_delegateInvokeMethod = delegateInvokeMethodOpt;
		_analyzedNullabilityMapOpt = analyzedNullabilityMapOpt;
		_returnTypesOpt = returnTypesOpt;
		_snapshotBuilderOpt = snapshotBuilderOpt;
		_isSpeculative = isSpeculative;
		_hasInitialState = variables != null;
		_baseOrThisInitializer = baseOrThisInitializer;
	}

	public string GetDebuggerDisplay()
	{
		if (IsConditionalState)
		{
			return "{" + GetType().Name + " WhenTrue:" + Dump(StateWhenTrue) + " WhenFalse:" + Dump(StateWhenFalse) + "}";
		}
		return "{" + GetType().Name + " " + Dump(State) + "}";
	}

	protected override void EnsureSufficientExecutionStack(int recursionDepth)
	{
		if (recursionDepth > 20 && compilation.TestOnlyCompilationData is NullableAnalysisData { MaxRecursionDepth: var maxRecursionDepth } && maxRecursionDepth > 0 && recursionDepth > maxRecursionDepth)
		{
			throw new InsufficientExecutionStackException();
		}
		base.EnsureSufficientExecutionStack(recursionDepth);
	}

	protected override bool ConvertInsufficientExecutionStackExceptionToCancelledByStackGuardException()
	{
		return true;
	}

	protected override bool TryGetVariable(VariableIdentifier identifier, out int slot)
	{
		return _variables.TryGetValue(identifier, out slot);
	}

	protected override int AddVariable(VariableIdentifier identifier)
	{
		return _variables.Add(identifier);
	}

	[Conditional("DEBUG")]
	private void AssertNoPlaceholderReplacements()
	{
		_ = _resultForPlaceholdersOpt;
	}

	private void AddPlaceholderReplacement(BoundValuePlaceholderBase placeholder, BoundExpression? expression, VisitResult result)
	{
		if (_resultForPlaceholdersOpt == null)
		{
			_resultForPlaceholdersOpt = PooledDictionary<BoundValuePlaceholderBase, (BoundExpression, VisitResult)>.GetInstance();
		}
		_resultForPlaceholdersOpt.Add(placeholder, (expression, result));
	}

	private void RemovePlaceholderReplacement(BoundValuePlaceholderBase placeholder)
	{
		_resultForPlaceholdersOpt.Remove(placeholder);
	}

	[Conditional("DEBUG")]
	private static void AssertPlaceholderAllowedWithoutRegistration(BoundValuePlaceholderBase placeholder)
	{
		switch (placeholder.Kind)
		{
		case BoundKind.DeconstructValuePlaceholder:
		case BoundKind.AwaitableValuePlaceholder:
		case BoundKind.ObjectOrCollectionValuePlaceholder:
		case BoundKind.ImplicitIndexerValuePlaceholder:
		case BoundKind.InterpolatedStringHandlerPlaceholder:
		case BoundKind.InterpolatedStringArgumentPlaceholder:
			return;
		}
		throw ExceptionUtilities.UnexpectedValue(placeholder.Kind);
	}

	protected override ImmutableArray<PendingBranch> Scan(ref bool badRegion)
	{
		if (_returnTypesOpt != null)
		{
			_returnTypesOpt.Clear();
		}
		base.Diagnostics.Clear();
		regionPlace = RegionPlace.Before;
		if (!_isSpeculative)
		{
			ParameterSymbol methodThisParameter = base.MethodThisParameter;
			EnterParameters();
			if ((object)methodThisParameter != null)
			{
				EnterParameter(methodThisParameter, methodThisParameter.TypeWithAnnotations);
			}
			if (_symbol.TryGetInstanceExtensionParameter(out ParameterSymbol extensionParameter))
			{
				EnterParameter(extensionParameter, extensionParameter.TypeWithAnnotations);
			}
			makeNotNullMembersMaybeNull();
			_snapshotBuilderOpt?.TakeIncrementalSnapshot(methodMainNode, State);
		}
		ImmutableArray<AbstractFlowPass<LocalState, LocalFunctionState>.PendingBranch> result = base.Scan(ref badRegion);
		MethodSymbol obj = _symbol as MethodSymbol;
		if ((object)obj == null || !obj.IsConstructor() || _useConstructorExitWarnings)
		{
			EnforceDoesNotReturn(null);
			enforceMemberNotNull(null, State);
			EnforceParameterNotNullOnExit(null, State);
			foreach (AbstractFlowPass<LocalState, LocalFunctionState>.PendingBranch item in result)
			{
				enforceMemberNotNull(item.Branch.Syntax, item.State);
				if (item.Branch is BoundReturnStatement boundReturnStatement)
				{
					EnforceParameterNotNullOnExit(boundReturnStatement.Syntax, item.State);
					EnforceNotNullWhenForPendingReturn(item, boundReturnStatement);
					EnforceMemberNotNullWhenForPendingReturn(item, boundReturnStatement);
				}
			}
		}
		return result;
		void checkMemberStateOnConstructorExit(MethodSymbol constructor, Symbol member, LocalState state, int thisSlot, Location? exitLocation, ImmutableArray<string> membersWithStateEnforcedByRequiredMembers, bool forcePropertyAnalysis)
		{
			bool flag = !constructor.RequiresInstanceReceiver();
			if (member.IsStatic == flag && (!LocalDataFlowPass<LocalState, LocalFunctionState>.HasInitializer(member) || !constructor.IncludeFieldInitializersInBody()))
			{
				FieldSymbol fieldSymbol2;
				Symbol symbol;
				TypeWithAnnotations declaredType;
				if (!(member is FieldSymbol fieldSymbol))
				{
					if (!(member is EventSymbol eventSymbol))
					{
						if (!(member is PropertySymbol propertySymbol) || !forcePropertyAnalysis)
						{
							return;
						}
						declaredType = propertySymbol.TypeWithAnnotations;
						fieldSymbol2 = null;
						symbol = propertySymbol;
					}
					else
					{
						declaredType = eventSymbol.TypeWithAnnotations;
						fieldSymbol2 = eventSymbol.AssociatedField;
						symbol = eventSymbol;
						if ((object)fieldSymbol2 == null)
						{
							return;
						}
					}
				}
				else
				{
					declaredType = GetTypeOrReturnTypeWithAnnotations(fieldSymbol);
					fieldSymbol2 = fieldSymbol;
					symbol = (Symbol)(((object)(fieldSymbol.AssociatedSymbol as PropertySymbol)) ?? ((object)fieldSymbol));
				}
				if (((object)fieldSymbol2 == null || !fieldSymbol2.IsConst) && !declaredType.Type.IsValueType && !declaredType.Type.IsErrorType() && ((!symbol.IsRequired() && !membersWithStateEnforcedByRequiredMembers.Contains(symbol.Name)) || !constructor.ShouldCheckRequiredMembers()))
				{
					bool flag2 = symbol is SourcePropertySymbolBase sourcePropertySymbolBase && sourcePropertySymbolBase.UsesFieldKeyword;
					FlowAnalysisAnnotations flowAnalysisAnnotations = (flag2 ? fieldSymbol2.FlowAnalysisAnnotations : symbol.GetFlowAnalysisAnnotations());
					if ((flowAnalysisAnnotations & FlowAnalysisAnnotations.AllowNull) == 0)
					{
						declaredType = ApplyUnconditionalAnnotations(declaredType, flowAnalysisAnnotations);
						if (declaredType.NullableAnnotation.IsNotAnnotated())
						{
							int orCreateSlot = GetOrCreateSlot(symbol, thisSlot);
							if (orCreateSlot >= 0)
							{
								NullableFlowState state2 = GetState(ref state, orCreateSlot);
								NullableFlowState nullableFlowState = ((!declaredType.Type.IsPossiblyNullableReferenceTypeTypeParameter() || (flowAnalysisAnnotations & FlowAnalysisAnnotations.NotNull) != FlowAnalysisAnnotations.None) ? NullableFlowState.MaybeNull : NullableFlowState.MaybeDefault);
								if ((int)state2 >= (int)nullableFlowState)
								{
									CSDiagnosticInfo info = new CSDiagnosticInfo(flag2 ? ErrorCode.WRN_UninitializedNonNullableBackingField : ErrorCode.WRN_UninitializedNonNullableField, new object[2]
									{
										symbol.Kind.Localize(),
										symbol.Name
									}, ImmutableArray<Symbol>.Empty, symbol.Locations);
									base.Diagnostics.Add(info, exitLocation ?? symbol.GetFirstLocationOrNone());
								}
							}
						}
					}
				}
			}
		}
		void enforceMemberNotNull(SyntaxNode? syntaxOpt, LocalState state)
		{
			if (state.Reachable)
			{
				MethodSymbol methodSymbol = _symbol as MethodSymbol;
				if ((object)methodSymbol != null)
				{
					if (methodSymbol.IsConstructor())
					{
						int thisSlot = 0;
						if (methodSymbol.TryGetThisParameter(out ParameterSymbol thisParameter) && (object)thisParameter != null)
						{
							thisSlot = GetOrCreateSlot(thisParameter);
						}
						Location exitLocation = ((methodSymbol is SynthesizedPrimaryConstructor || methodSymbol.DeclaringSyntaxReferences.IsEmpty) ? null : methodSymbol.TryGetFirstLocation());
						bool flag = methodSymbol.ShouldCheckRequiredMembers();
						ImmutableArray<string> membersWithStateEnforcedByRequiredMembers = (flag ? methodSymbol.ContainingType.GetMembersUnordered().SelectManyAsArray((Symbol symbol2) => symbol2 is PropertySymbol propertySymbol && propertySymbol.IsRequired, delegate(Symbol symbol2)
						{
							PropertySymbol propertySymbol = (PropertySymbol)symbol2;
							return propertySymbol.SetMethod?.NotNullMembers ?? propertySymbol.NotNullMembers;
						}) : ImmutableArray<string>.Empty);
						PooledHashSet<Symbol> instance = PooledHashSet<Symbol>.GetInstance();
						foreach (Symbol item2 in methodSymbol.ContainingType.GetMembersUnordered())
						{
							bool forcePropertyAnalysis = !flag && !(item2 is SourcePropertySymbolBase { BackingField: not null }) && item2.IsRequired();
							checkMemberStateOnConstructorExit(methodSymbol, item2, state, thisSlot, exitLocation, membersWithStateEnforcedByRequiredMembers, forcePropertyAnalysis);
						}
						MethodSymbol? baseOrThisInitializer = GetBaseOrThisInitializer();
						if ((object)baseOrThisInitializer != null && baseOrThisInitializer.ShouldCheckRequiredMembers() && !flag)
						{
							NamedTypeSymbol baseTypeNoUseSiteDiagnostics = methodSymbol.ContainingType.BaseTypeNoUseSiteDiagnostics;
							if ((object)baseTypeNoUseSiteDiagnostics != null)
							{
								foreach (var (_, member) in baseTypeNoUseSiteDiagnostics.AllRequiredMembers)
								{
									checkMemberStateOnConstructorExit(methodSymbol, member, state, thisSlot, exitLocation, ImmutableArray<string>.Empty, forcePropertyAnalysis: true);
								}
							}
						}
						instance.Free();
					}
					else
					{
						do
						{
							foreach (string notNullMember in methodSymbol.NotNullMembers)
							{
								EnforceMemberNotNullOnMember(syntaxOpt, state, methodSymbol, notNullMember);
							}
							methodSymbol = methodSymbol.OverriddenMethod;
						}
						while (methodSymbol != null);
					}
				}
			}
		}
		static OneOrMany<Symbol> getAllMembersToBeDefaulted(Symbol requiredMember, bool filterOverridingProperties)
		{
			if (requiredMember is FieldSymbol)
			{
				return OneOrMany.Create(requiredMember);
			}
			PropertySymbol propertySymbol = (PropertySymbol)requiredMember;
			if (filterOverridingProperties && isFilterableOverrideOfAbstractProperty(propertySymbol))
			{
				return OneOrMany<Symbol>.Empty;
			}
			OneOrMany<Symbol> result2 = OneOrMany.Create(getFieldSymbolToBeInitialized(propertySymbol));
			foreach (string item3 in propertySymbol.SetMethod?.NotNullMembers ?? propertySymbol.NotNullMembers)
			{
				foreach (Symbol member2 in propertySymbol.ContainingType.GetMembers(item3))
				{
					result2 = result2.Add(getFieldSymbolToBeInitialized(member2));
				}
			}
			return result2;
		}
		static ImmutableArray<Symbol> getAllTypeAndRequiredMembers(TypeSymbol containingType)
		{
			ImmutableArray<Symbol> membersUnordered = containingType.GetMembersUnordered();
			ImmutableSegmentedDictionary<string, Symbol> immutableSegmentedDictionary = containingType.BaseTypeNoUseSiteDiagnostics?.AllRequiredMembers ?? ImmutableSegmentedDictionary<string, Symbol>.Empty;
			if (immutableSegmentedDictionary.IsEmpty)
			{
				return membersUnordered;
			}
			ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(membersUnordered.Length + immutableSegmentedDictionary.Count);
			instance.AddRange(membersUnordered);
			foreach (var (_, symbol2) in immutableSegmentedDictionary)
			{
				if (symbol2 is PropertySymbol && symbol2.IsAbstract)
				{
					Symbol symbol3 = membersUnordered.FirstOrDefault((Symbol thisMember, Symbol baseMember) => thisMember.IsOverride && (object)thisMember.GetOverriddenMember() == baseMember, symbol2);
					if ((object)symbol3 != null && isFilterableOverrideOfAbstractProperty((PropertySymbol)symbol3))
					{
						continue;
					}
				}
				instance.AddRange(getAllMembersToBeDefaulted(symbol2, filterOverridingProperties: false));
			}
			return instance.ToImmutableAndFree();
		}
		static Symbol getFieldSymbolToBeInitialized(Symbol requiredMember)
		{
			if (!(requiredMember is SourcePropertySymbolBase { BackingField: { } backingField }))
			{
				return requiredMember;
			}
			return backingField;
		}
		static bool isFilterableOverrideOfAbstractProperty(PropertySymbol property)
		{
			PropertySymbol overriddenProperty = property.OverriddenProperty;
			if ((object)overriddenProperty == null || !overriddenProperty.IsAbstract)
			{
				return false;
			}
			FlowAnalysisAnnotations annotations = ((property is SourcePropertySymbolBase { UsesFieldKeyword: not false, BackingField: { } backingField }) ? backingField.FlowAnalysisAnnotations : property.GetFlowAnalysisAnnotations());
			TypeWithAnnotations typeWithAnnotations = ApplyUnconditionalAnnotations(property.TypeWithAnnotations, annotations);
			if (!typeWithAnnotations.NullableAnnotation.IsNotAnnotated())
			{
				return false;
			}
			FlowAnalysisAnnotations flowAnalysisAnnotations = overriddenProperty.GetFlowAnalysisAnnotations();
			return ApplyUnconditionalAnnotations(overriddenProperty.TypeWithAnnotations, flowAnalysisAnnotations).NullableAnnotation == typeWithAnnotations.NullableAnnotation;
		}
		void makeNotNullMembersMaybeNull()
		{
			Symbol symbol = _symbol;
			MethodSymbol method = symbol as MethodSymbol;
			if ((object)method != null)
			{
				if (method.IsConstructor())
				{
					foreach (Symbol item4 in getMembersNeedingDefaultInitialState())
					{
						if (item4.IsStatic == method.IsStatic)
						{
							Symbol symbol2 = item4;
							if (item4 is PropertySymbol propertySymbol)
							{
								if (!propertySymbol.IsRequired)
								{
									continue;
								}
							}
							else if (item4 is FieldSymbol fieldSymbol)
							{
								if (fieldSymbol.OriginalDefinition is SynthesizedPrimaryConstructorParameterBackingFieldSymbol || fieldSymbol.IsConst)
								{
									continue;
								}
								if (fieldSymbol.AssociatedSymbol is SourcePropertySymbolBase { UsesFieldKeyword: false } sourcePropertySymbolBase)
								{
									if (IsPropertyOutputMoreStrictThanInput(sourcePropertySymbolBase))
									{
										continue;
									}
									symbol2 = sourcePropertySymbolBase;
								}
							}
							int slotForMemberPostCondition = GetSlotForMemberPostCondition(symbol2);
							if (slotForMemberPostCondition > 0)
							{
								TypeWithAnnotations typeOrReturnTypeWithAnnotations = GetTypeOrReturnTypeWithAnnotations(symbol2);
								if (!typeOrReturnTypeWithAnnotations.NullableAnnotation.IsOblivious())
								{
									SetState(ref State, slotForMemberPostCondition, (!typeOrReturnTypeWithAnnotations.Type.IsPossiblyNullableReferenceTypeTypeParameter()) ? NullableFlowState.MaybeNull : NullableFlowState.MaybeDefault);
								}
							}
						}
					}
				}
				else
				{
					do
					{
						MakeMembersMaybeNull(method, method.NotNullMembers);
						MakeMembersMaybeNull(method, method.NotNullWhenTrueMembers);
						MakeMembersMaybeNull(method, method.NotNullWhenFalseMembers);
						method = method.OverriddenMethod;
					}
					while (method != null);
				}
			}
			ImmutableArray<Symbol> getMembersNeedingDefaultInitialState()
			{
				if (_hasInitialState)
				{
					return ImmutableArray<Symbol>.Empty;
				}
				bool includeCurrentTypeRequiredMembers = true;
				bool flag = true;
				bool flag2 = false;
				if (method is SourceMemberMethodSymbol { SyntaxNode: ConstructorDeclarationSyntax syntaxNode })
				{
					ConstructorInitializerSyntax initializer = syntaxNode.Initializer;
					if (initializer != null)
					{
						int rawKind = initializer.RawKind;
						flag = GetBaseOrThisInitializer()?.ShouldCheckRequiredMembers() ?? true;
						switch (rawKind)
						{
						case 8890:
							flag2 = true;
							includeCurrentTypeRequiredMembers = flag;
							break;
						case 8889:
							includeCurrentTypeRequiredMembers = true;
							break;
						}
					}
				}
				if (!flag2 && (!method.ContainingType.IsValueType || method.IsStatic || compilation.IsFeatureEnabled(MessageID.IDS_FeatureAutoDefaultStructs)))
				{
					return membersToBeInitialized(method.ContainingType, includeAllMembers: true, includeCurrentTypeRequiredMembers, flag);
				}
				return membersToBeInitialized(method.ContainingType, method.IncludeFieldInitializersInBody(), includeCurrentTypeRequiredMembers, flag);
			}
		}
		static ImmutableArray<Symbol> membersToBeInitialized(NamedTypeSymbol containingType, bool includeAllMembers, bool includeCurrentTypeRequiredMembers, bool includeBaseRequiredMembers)
		{
			if (!includeAllMembers)
			{
				if (includeCurrentTypeRequiredMembers)
				{
					if (!includeBaseRequiredMembers)
					{
						return containingType.GetMembersUnordered().SelectManyAsArray(SymbolExtensions.IsRequired, (Symbol symbol) => getAllMembersToBeDefaulted(symbol, filterOverridingProperties: true));
					}
					return containingType.AllRequiredMembers.SelectManyAsArray<KeyValuePair<string, Symbol>, Symbol>((KeyValuePair<string, Symbol> kvp) => getAllMembersToBeDefaulted(kvp.Value, filterOverridingProperties: true));
				}
				if (!includeBaseRequiredMembers)
				{
					return ImmutableArray<Symbol>.Empty;
				}
			}
			else
			{
				if (!includeBaseRequiredMembers)
				{
					return containingType.GetMembersUnordered().SelectManyAsArray(delegate(Symbol symbol)
					{
						Symbol symbol2 = getFieldSymbolToBeInitialized(symbol);
						PropertySymbol propertySymbol = (symbol2 as PropertySymbol) ?? ((symbol2 as FieldSymbol)?.AssociatedSymbol as PropertySymbol);
						return ((object)propertySymbol != null && isFilterableOverrideOfAbstractProperty(propertySymbol)) ? OneOrMany<Symbol>.Empty : OneOrMany.Create(symbol2);
					});
				}
				if (includeCurrentTypeRequiredMembers)
				{
					return getAllTypeAndRequiredMembers(containingType);
				}
			}
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/NullableWalker.cs", 977);
		}
	}

	private void EnforceMemberNotNullOnMember(SyntaxNode? syntaxOpt, LocalState state, MethodSymbol method, string memberName)
	{
		foreach (Symbol member in method.ContainingType.GetMembers(memberName))
		{
			if (!FailsMemberNotNullExpectation(member, state))
			{
				continue;
			}
			SyntaxNodeOrToken syntaxNodeOrToken;
			if (!(syntaxOpt is BlockSyntax blockSyntax))
			{
				if (syntaxOpt is LocalFunctionStatementSyntax localFunctionStatementSyntax)
				{
					syntaxNodeOrToken = localFunctionStatementSyntax.GetLastToken();
				}
				else
				{
					syntaxNodeOrToken = ((syntaxOpt != null) ? ((SyntaxNodeOrToken)syntaxOpt) : ((SyntaxNodeOrToken)methodMainNode.Syntax.GetLastToken()));
				}
			}
			else
			{
				syntaxNodeOrToken = blockSyntax.CloseBraceToken;
			}
			SyntaxNodeOrToken syntaxNodeOrToken2 = syntaxNodeOrToken;
			base.Diagnostics.Add(ErrorCode.WRN_MemberNotNull, syntaxNodeOrToken2.GetLocation(), member.Name);
		}
	}

	private void EnforceMemberNotNullWhenForPendingReturn(PendingBranch pendingReturn, BoundReturnStatement returnStatement)
	{
		if (!pendingReturn.IsConditionalState)
		{
			return;
		}
		BoundExpression expressionOpt = returnStatement.ExpressionOpt;
		if (expressionOpt != null)
		{
			ConstantValue constantValueOpt = expressionOpt.ConstantValueOpt;
			if ((object)constantValueOpt != null && constantValueOpt.IsBoolean)
			{
				bool booleanValue = constantValueOpt.BooleanValue;
				enforceMemberNotNullWhen(returnStatement.Syntax, booleanValue, pendingReturn.State);
				return;
			}
		}
		if (pendingReturn.StateWhenTrue.Reachable && pendingReturn.StateWhenFalse.Reachable && _symbol is MethodSymbol { NotNullWhenTrueMembers: var notNullWhenTrueMembers } methodSymbol)
		{
			foreach (string item in notNullWhenTrueMembers)
			{
				enforceMemberNotNullWhenIfAffected(returnStatement.Syntax, sense: true, methodSymbol.ContainingType.GetMembers(item), pendingReturn.StateWhenTrue, pendingReturn.StateWhenFalse);
			}
			foreach (string notNullWhenFalseMember in methodSymbol.NotNullWhenFalseMembers)
			{
				enforceMemberNotNullWhenIfAffected(returnStatement.Syntax, sense: false, methodSymbol.ContainingType.GetMembers(notNullWhenFalseMember), pendingReturn.StateWhenFalse, pendingReturn.StateWhenTrue);
			}
		}
		void enforceMemberNotNullWhen(SyntaxNode? syntaxOpt, bool sense, LocalState state)
		{
			if (_symbol is MethodSymbol methodSymbol2)
			{
				foreach (string item2 in sense ? methodSymbol2.NotNullWhenTrueMembers : methodSymbol2.NotNullWhenFalseMembers)
				{
					foreach (Symbol member in methodSymbol2.ContainingType.GetMembers(item2))
					{
						ReportFailedMemberNotNullIfNeeded(syntaxOpt, sense, member, state);
					}
				}
			}
		}
		void enforceMemberNotNullWhenIfAffected(SyntaxNode? syntaxOpt, bool sense, ImmutableArray<Symbol> members, LocalState state, LocalState otherState)
		{
			foreach (Symbol item3 in members)
			{
				if (FailsMemberNotNullExpectation(item3, state) != FailsMemberNotNullExpectation(item3, otherState))
				{
					ReportFailedMemberNotNullIfNeeded(syntaxOpt, sense, item3, state);
				}
			}
		}
	}

	private void ReportFailedMemberNotNullIfNeeded(SyntaxNode? syntaxOpt, bool sense, Symbol member, LocalState state)
	{
		if (FailsMemberNotNullExpectation(member, state))
		{
			base.Diagnostics.Add(ErrorCode.WRN_MemberNotNullWhen, syntaxOpt?.GetLocation() ?? methodMainNode.Syntax.GetLastToken().GetLocation(), member.Name, sense ? "true" : "false");
		}
	}

	private bool FailsMemberNotNullExpectation(Symbol member, LocalState state)
	{
		switch (member.Kind)
		{
		case SymbolKind.Field:
		case SymbolKind.Property:
		{
			int slotForMemberPostCondition = GetSlotForMemberPostCondition(member);
			if (slotForMemberPostCondition > 0)
			{
				return !GetState(ref state, slotForMemberPostCondition).IsNotNull();
			}
			return false;
		}
		default:
			return false;
		}
	}

	private void MakeMembersMaybeNull(MethodSymbol method, ImmutableArray<string> members)
	{
		foreach (string item in members)
		{
			makeMemberMaybeNull(method, item);
		}
		void makeMemberMaybeNull(MethodSymbol methodSymbol, string memberName)
		{
			foreach (Symbol member in methodSymbol.ContainingType.GetMembers(memberName))
			{
				int slotForMemberPostCondition = GetSlotForMemberPostCondition(member);
				if (slotForMemberPostCondition > 0)
				{
					SetState(ref State, slotForMemberPostCondition, NullableFlowState.MaybeNull);
				}
			}
		}
	}

	private int GetSlotForMemberPostCondition(Symbol member)
	{
		if (member.Kind != SymbolKind.Field && member.Kind != SymbolKind.Property && member.Kind != SymbolKind.Event)
		{
			return -1;
		}
		int num;
		if (member.IsStatic)
		{
			num = 0;
		}
		else
		{
			num = GetReceiverSlotForMemberPostConditions(_symbol as MethodSymbol);
			if (num <= 0)
			{
				return -1;
			}
		}
		return GetOrCreateSlot(member, num);
	}

	private MethodSymbol? GetBaseOrThisInitializer()
	{
		return _baseOrThisInitializer ?? GetConstructorThisOrBaseSymbol(methodMainNode);
	}

	private void EnforceNotNullWhenForPendingReturn(PendingBranch pendingReturn, BoundReturnStatement returnStatement)
	{
		if (!(_symbol is MethodSymbol symbol))
		{
			return;
		}
		ImmutableArray<ParameterSymbol> parametersIncludingExtensionParameter = symbol.GetParametersIncludingExtensionParameter(skipExtensionIfStatic: true);
		if (parametersIncludingExtensionParameter.IsEmpty)
		{
			return;
		}
		BoundExpression expressionOpt;
		if (pendingReturn.IsConditionalState)
		{
			expressionOpt = returnStatement.ExpressionOpt;
			if (expressionOpt != null)
			{
				ConstantValue constantValueOpt = expressionOpt.ConstantValueOpt;
				if ((object)constantValueOpt != null && constantValueOpt.IsBoolean)
				{
					bool booleanValue = constantValueOpt.BooleanValue;
					EnforceParameterNotNullWhenOnExit(returnStatement.Syntax, parametersIncludingExtensionParameter, booleanValue, pendingReturn.State);
					return;
				}
			}
			if (!pendingReturn.StateWhenTrue.Reachable || !pendingReturn.StateWhenFalse.Reachable)
			{
				return;
			}
			foreach (ParameterSymbol item in parametersIncludingExtensionParameter)
			{
				int orCreateSlot = GetOrCreateSlot(item);
				if (orCreateSlot > 0 && GetState(ref pendingReturn.StateWhenTrue, orCreateSlot) != GetState(ref pendingReturn.StateWhenFalse, orCreateSlot))
				{
					ReportParameterIfBadConditionalState(returnStatement.Syntax, item, sense: true, pendingReturn.StateWhenTrue);
					ReportParameterIfBadConditionalState(returnStatement.Syntax, item, sense: false, pendingReturn.StateWhenFalse);
				}
			}
			return;
		}
		expressionOpt = returnStatement.ExpressionOpt;
		if (expressionOpt != null)
		{
			ConstantValue constantValueOpt = expressionOpt.ConstantValueOpt;
			if ((object)constantValueOpt != null && constantValueOpt.IsBoolean)
			{
				bool booleanValue2 = constantValueOpt.BooleanValue;
				EnforceParameterNotNullWhenOnExit(returnStatement.Syntax, parametersIncludingExtensionParameter, booleanValue2, pendingReturn.State);
			}
		}
	}

	private void EnforceParameterNotNullOnExit(SyntaxNode? syntaxOpt, LocalState state)
	{
		if (!state.Reachable || !(_symbol is MethodSymbol symbol))
		{
			return;
		}
		ImmutableArray<ParameterSymbol> parametersIncludingExtensionParameter = symbol.GetParametersIncludingExtensionParameter(skipExtensionIfStatic: true);
		foreach (ParameterSymbol item in parametersIncludingExtensionParameter)
		{
			int orCreateSlot = GetOrCreateSlot(item);
			if (orCreateSlot > 0)
			{
				bool num = (item.FlowAnalysisAnnotations & FlowAnalysisAnnotations.NotNull) == FlowAnalysisAnnotations.NotNull;
				NullableFlowState state2 = GetState(ref state, orCreateSlot);
				if (num && state2.MayBeNull())
				{
					DiagnosticBagExtensions.Add(location: (!(syntaxOpt is BlockSyntax { CloseBraceToken: var closeBraceToken })) ? (syntaxOpt?.GetLocation() ?? methodMainNode.Syntax.GetLastToken().GetLocation()) : closeBraceToken.GetLocation(), diagnostics: base.Diagnostics, code: ErrorCode.WRN_ParameterDisallowsNull, args: new object[1] { item.Name });
				}
				else
				{
					EnforceNotNullIfNotNull(syntaxOpt, state, parametersIncludingExtensionParameter, item.NotNullIfParameterNotNull, state2, item);
				}
			}
		}
	}

	private void EnforceParameterNotNullWhenOnExit(SyntaxNode syntax, ImmutableArray<ParameterSymbol> parameters, bool sense, LocalState stateWhen)
	{
		if (stateWhen.Reachable)
		{
			foreach (ParameterSymbol item in parameters)
			{
				ReportParameterIfBadConditionalState(syntax, item, sense, stateWhen);
			}
		}
	}

	private void ReportParameterIfBadConditionalState(SyntaxNode syntax, ParameterSymbol parameter, bool sense, LocalState stateWhen)
	{
		if (parameterHasBadConditionalState(parameter, sense, stateWhen))
		{
			base.Diagnostics.Add(ErrorCode.WRN_ParameterConditionallyDisallowsNull, syntax.Location, parameter.Name, sense ? "true" : "false");
		}
		bool parameterHasBadConditionalState(ParameterSymbol parameterSymbol, bool flag, LocalState state2)
		{
			RefKind refKind = parameterSymbol.RefKind;
			if (refKind != RefKind.Out && refKind != RefKind.Ref)
			{
				return false;
			}
			int orCreateSlot = GetOrCreateSlot(parameterSymbol);
			if (orCreateSlot > 0)
			{
				NullableFlowState state = GetState(ref state2, orCreateSlot);
				FlowAnalysisAnnotations flowAnalysisAnnotations = parameterSymbol.FlowAnalysisAnnotations;
				if (flag)
				{
					bool flag2 = (flowAnalysisAnnotations & FlowAnalysisAnnotations.NotNull) == FlowAnalysisAnnotations.NotNullWhenTrue;
					bool flag3 = (flowAnalysisAnnotations & FlowAnalysisAnnotations.MaybeNull) == FlowAnalysisAnnotations.MaybeNullWhenFalse;
					if (!flag2 || !state.MayBeNull())
					{
						if (flag3)
						{
							return ShouldReportNullableAssignment(parameterSymbol.TypeWithAnnotations, state);
						}
						return false;
					}
					return true;
				}
				bool flag4 = (flowAnalysisAnnotations & FlowAnalysisAnnotations.NotNull) == FlowAnalysisAnnotations.NotNullWhenFalse;
				bool flag5 = (flowAnalysisAnnotations & FlowAnalysisAnnotations.MaybeNull) == FlowAnalysisAnnotations.MaybeNullWhenTrue;
				if (!flag4 || !state.MayBeNull())
				{
					if (flag5)
					{
						return ShouldReportNullableAssignment(parameterSymbol.TypeWithAnnotations, state);
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	private void EnforceNotNullIfNotNull(SyntaxNode? syntaxOpt, LocalState state, ImmutableArray<ParameterSymbol> parameters, ImmutableHashSet<string> inputParamNames, NullableFlowState outputState, ParameterSymbol? outputParam)
	{
		if (inputParamNames.IsEmpty || outputState.IsNotNull())
		{
			return;
		}
		foreach (ParameterSymbol item in parameters)
		{
			if (!inputParamNames.Contains(item.Name))
			{
				continue;
			}
			int orCreateSlot = GetOrCreateSlot(item);
			if (orCreateSlot > 0 && GetState(ref state, orCreateSlot).IsNotNull())
			{
				Location location = syntaxOpt?.GetLocation() ?? methodMainNode.Syntax.GetLastToken().GetLocation();
				if ((object)outputParam != null)
				{
					base.Diagnostics.Add(ErrorCode.WRN_ParameterNotNullIfNotNull, location, outputParam.Name, item.Name);
				}
				else if (CurrentSymbol is MethodSymbol { IsAsync: false })
				{
					base.Diagnostics.Add(ErrorCode.WRN_ReturnNotNullIfNotNull, location, item.Name);
				}
				break;
			}
		}
	}

	private void EnforceDoesNotReturn(SyntaxNode? syntaxOpt)
	{
		if (CurrentSymbol is MethodSymbol methodSymbol && (methodSymbol.FlowAnalysisAnnotations & FlowAnalysisAnnotations.DoesNotReturn) == FlowAnalysisAnnotations.DoesNotReturn && IsReachable())
		{
			ReportDiagnostic(ErrorCode.WRN_ShouldNotReturn, syntaxOpt?.GetLocation() ?? methodMainNode.Syntax.GetLastToken().GetLocation());
		}
	}

	internal static void AnalyzeIfNeeded(CSharpCompilation compilation, MethodSymbol method, BoundNode node, DiagnosticBag diagnostics, bool useConstructorExitWarnings, VariableState? initialNullableState, bool getFinalNullableState, MethodSymbol? baseOrThisInitializer, out VariableState? finalNullableState)
	{
		if (!HasRequiredLanguageVersion(compilation) || !compilation.IsNullableAnalysisEnabledIn(method))
		{
			if (compilation.IsNullableAnalysisEnabledAlways)
			{
				Analyze(compilation, method, node, new DiagnosticBag(), useConstructorExitWarnings: false, null, getFinalNullableState: false, baseOrThisInitializer, out VariableState _, requiresAnalysis: false);
			}
			finalNullableState = null;
		}
		else
		{
			Analyze(compilation, method, node, diagnostics, useConstructorExitWarnings, initialNullableState, getFinalNullableState, baseOrThisInitializer, out finalNullableState);
		}
	}

	private static void Analyze(CSharpCompilation compilation, MethodSymbol method, BoundNode node, DiagnosticBag diagnostics, bool useConstructorExitWarnings, VariableState? initialNullableState, bool getFinalNullableState, MethodSymbol? baseOrThisInitializer, out VariableState? finalNullableState, bool requiresAnalysis = true)
	{
		if (method.IsImplicitlyDeclared && !method.IsImplicitConstructor && !method.IsScriptInitializer)
		{
			finalNullableState = null;
			return;
		}
		Binder binder = ((method is SynthesizedSimpleProgramEntryPointSymbol synthesizedSimpleProgramEntryPointSymbol) ? synthesizedSimpleProgramEntryPointSymbol.GetBodyBinder(ignoreAccessibility: false) : compilation.GetBinderFactory(node.SyntaxTree).GetBinder(node.Syntax));
		Conversions conversions = binder.Conversions;
		Analyze(compilation, method, node, binder, conversions, diagnostics, useConstructorExitWarnings, null, useDelegateInvokeParameterTypes: false, useDelegateInvokeReturnType: false, null, initialNullableState, baseOrThisInitializer, null, null, null, getFinalNullableState, out finalNullableState, requiresAnalysis);
	}

	internal static VariableState? GetAfterInitializersState(CSharpCompilation compilation, Symbol? symbol, BoundNode constructorBody)
	{
		if (symbol is MethodSymbol methodSymbol && methodSymbol.IncludeFieldInitializersInBody() && methodSymbol.ContainingType is SourceMemberContainerTypeSymbol sourceMemberContainerTypeSymbol)
		{
			Binder.ProcessedFieldInitializers processedInitializers = default(Binder.ProcessedFieldInitializers);
			Binder.BindFieldInitializers(compilation, null, methodSymbol.IsStatic ? sourceMemberContainerTypeSymbol.StaticInitializers : sourceMemberContainerTypeSymbol.InstanceInitializers, BindingDiagnosticBag.Discarded, ref processedInitializers);
			return GetAfterInitializersState(compilation, methodSymbol, InitializerRewriter.RewriteConstructor(processedInitializers.BoundInitializers, methodSymbol), constructorBody, BindingDiagnosticBag.Discarded);
		}
		return null;
	}

	internal static VariableState? GetAfterInitializersState(CSharpCompilation compilation, MethodSymbol method, BoundNode nodeToAnalyze, BoundNode? constructorBody, BindingDiagnosticBag diagnostics)
	{
		DiagnosticBag diagnosticBag;
		bool flag;
		if (diagnostics.DiagnosticBag == null)
		{
			diagnostics = BindingDiagnosticBag.Discarded;
			diagnosticBag = DiagnosticBag.GetInstance();
			flag = true;
		}
		else
		{
			diagnosticBag = diagnostics.DiagnosticBag;
			flag = false;
		}
		MethodSymbol constructorThisOrBaseSymbol = GetConstructorThisOrBaseSymbol(constructorBody);
		AnalyzeIfNeeded(compilation, method, nodeToAnalyze, diagnosticBag, useConstructorExitWarnings: false, null, getFinalNullableState: true, constructorThisOrBaseSymbol, out VariableState finalNullableState);
		if (flag)
		{
			diagnosticBag.Free();
		}
		return finalNullableState;
	}

	private static MethodSymbol? GetConstructorThisOrBaseSymbol(BoundNode? constructorBody)
	{
		if (constructorBody is BoundConstructorMethodBody { Initializer: BoundExpressionStatement { Expression: BoundCall expression } })
		{
			MethodSymbol method = expression.Method;
			if ((object)method != null && method.MethodKind == MethodKind.Constructor)
			{
				return method;
			}
		}
		return null;
	}

	internal static void AnalyzeWithoutRewrite(CSharpCompilation compilation, Symbol? symbol, BoundNode node, Binder binder, DiagnosticBag diagnostics, bool createSnapshots)
	{
		AnalyzeWithSemanticInfo(compilation, symbol, node, binder, GetAfterInitializersState(compilation, symbol, node), diagnostics, createSnapshots, requiresAnalysis: false);
	}

	internal static BoundNode AnalyzeAndRewrite(CSharpCompilation compilation, Symbol? symbol, BoundNode node, Binder binder, VariableState? initialState, DiagnosticBag diagnostics, bool createSnapshots, out SnapshotManager? snapshotManager, ref ImmutableDictionary<Symbol, Symbol>? remappedSymbols)
	{
		(SnapshotManager, ImmutableDictionary<BoundExpression, (NullabilityInfo, TypeSymbol)>) tuple = AnalyzeWithSemanticInfo(compilation, symbol, node, binder, initialState, diagnostics, createSnapshots, requiresAnalysis: true);
		(snapshotManager, _) = tuple;
		return Rewrite(tuple.Item2, snapshotManager, node, ref remappedSymbols);
	}

	private static (SnapshotManager?, ImmutableDictionary<BoundExpression, (NullabilityInfo, TypeSymbol?)>) AnalyzeWithSemanticInfo(CSharpCompilation compilation, Symbol? symbol, BoundNode node, Binder binder, VariableState? initialState, DiagnosticBag diagnostics, bool createSnapshots, bool requiresAnalysis)
	{
		ImmutableDictionary<BoundExpression, (NullabilityInfo, TypeSymbol)>.Builder builder = ImmutableDictionary.CreateBuilder(EqualityComparer<BoundExpression>.Default, NullabilityInfoTypeComparer.Instance);
		SnapshotManager.Builder builder2 = ((createSnapshots && symbol != null) ? new SnapshotManager.Builder() : null);
		Analyze(compilation, symbol, node, binder, binder.Conversions, diagnostics, useConstructorExitWarnings: true, null, useDelegateInvokeParameterTypes: false, useDelegateInvokeReturnType: false, null, initialState, null, builder, builder2, null, getFinalNullableState: false, out VariableState _, requiresAnalysis);
		ImmutableDictionary<BoundExpression, (NullabilityInfo, TypeSymbol)> item = builder.ToImmutable();
		return (builder2?.ToManagerAndFree(), item);
	}

	internal static BoundNode AnalyzeAndRewriteSpeculation(int position, BoundNode node, Binder binder, SnapshotManager originalSnapshots, out SnapshotManager newSnapshots, ref ImmutableDictionary<Symbol, Symbol>? remappedSymbols)
	{
		ImmutableDictionary<BoundExpression, (NullabilityInfo, TypeSymbol)>.Builder builder = ImmutableDictionary.CreateBuilder(EqualityComparer<BoundExpression>.Default, NullabilityInfoTypeComparer.Instance);
		SnapshotManager.Builder builder2 = new SnapshotManager.Builder();
		(VariablesSnapshot, LocalStateSnapshot) snapshot = originalSnapshots.GetSnapshot(position);
		VariablesSnapshot item = snapshot.Item1;
		LocalStateSnapshot item2 = snapshot.Item2;
		Symbol symbol = item.Symbol;
		NullableWalker nullableWalker = new NullableWalker(binder.Compilation, symbol, useConstructorExitWarnings: false, null, useDelegateInvokeParameterTypes: false, useDelegateInvokeReturnType: false, null, node, binder, binder.Conversions, Variables.Create(item), null, null, builder, builder2, isSpeculative: true);
		try
		{
			Analyze(nullableWalker, symbol, null, LocalState.Create(item2), builder2);
		}
		finally
		{
			nullableWalker.Free();
		}
		ImmutableDictionary<BoundExpression, (NullabilityInfo, TypeSymbol)> updatedNullabilities = builder.ToImmutable();
		newSnapshots = builder2.ToManagerAndFree();
		return Rewrite(updatedNullabilities, newSnapshots, node, ref remappedSymbols);
	}

	private static BoundNode Rewrite(ImmutableDictionary<BoundExpression, (NullabilityInfo, TypeSymbol?)> updatedNullabilities, SnapshotManager? snapshotManager, BoundNode node, ref ImmutableDictionary<Symbol, Symbol>? remappedSymbols)
	{
		ImmutableDictionary<Symbol, Symbol>.Builder builder = ImmutableDictionary.CreateBuilder(Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything, Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything);
		if (remappedSymbols != null)
		{
			builder.AddRange(remappedSymbols);
		}
		BoundNode result = new NullabilityRewriter(updatedNullabilities, snapshotManager, builder).Visit(node);
		remappedSymbols = builder.ToImmutable();
		return result;
	}

	private static bool HasRequiredLanguageVersion(CSharpCompilation compilation)
	{
		return compilation.LanguageVersion >= MessageID.IDS_FeatureNullableReferenceTypes.RequiredVersion();
	}

	internal static bool NeedsAnalysis(CSharpCompilation compilation, SyntaxNode syntaxNode)
	{
		if (HasRequiredLanguageVersion(compilation))
		{
			if (!compilation.IsNullableAnalysisEnabledIn(syntaxNode))
			{
				return compilation.IsNullableAnalysisEnabledAlways;
			}
			return true;
		}
		return false;
	}

	internal static void AnalyzeIfNeeded(Binder binder, BoundNode node, SyntaxNode syntax, DiagnosticBag diagnostics, (SourcePropertyAccessorSymbol symbol, GetterNullResilienceData getterNullResilienceData)? symbolAndGetterNullResilienceData = null)
	{
		bool requiresAnalysis = true;
		CSharpCompilation cSharpCompilation = binder.Compilation;
		if (!HasRequiredLanguageVersion(cSharpCompilation) || !cSharpCompilation.IsNullableAnalysisEnabledIn(syntax))
		{
			if (!cSharpCompilation.IsNullableAnalysisEnabledAlways)
			{
				return;
			}
			diagnostics = new DiagnosticBag();
			requiresAnalysis = false;
		}
		Analyze(cSharpCompilation, symbolAndGetterNullResilienceData?.symbol, node, binder, binder.Conversions, diagnostics, useConstructorExitWarnings: false, symbolAndGetterNullResilienceData?.getterNullResilienceData, useDelegateInvokeParameterTypes: false, useDelegateInvokeReturnType: false, null, null, null, null, null, null, getFinalNullableState: false, out VariableState _, requiresAnalysis);
	}

	internal static void Analyze(CSharpCompilation compilation, BoundLambda lambda, Conversions conversions, DiagnosticBag diagnostics, MethodSymbol? delegateInvokeMethodOpt, VariableState initialState, ArrayBuilder<(BoundReturnStatement, TypeWithAnnotations)>? returnTypesOpt, GetterNullResilienceData? getterNullResilienceData)
	{
		MethodSymbol symbol = lambda.Symbol;
		Variables variables = Variables.Create(initialState.Variables).CreateNestedMethodScope(symbol);
		UseDelegateInvokeParameterAndReturnTypes(lambda, delegateInvokeMethodOpt, out var useDelegateInvokeParameterTypes, out var useDelegateInvokeReturnType);
		NullableWalker nullableWalker = new NullableWalker(compilation, symbol, useConstructorExitWarnings: false, getterNullResilienceData, useDelegateInvokeParameterTypes, useDelegateInvokeReturnType, delegateInvokeMethodOpt, lambda.Body, lambda.Binder, conversions, variables, null, returnTypesOpt, null, null);
		try
		{
			LocalState localState = LocalState.Create(initialState.VariableNullableStates).CreateNestedMethodState(variables);
			Analyze(nullableWalker, symbol, diagnostics, localState, null);
		}
		finally
		{
			nullableWalker.Free();
		}
	}

	private static void Analyze(CSharpCompilation compilation, Symbol? symbol, BoundNode node, Binder binder, Conversions conversions, DiagnosticBag diagnostics, bool useConstructorExitWarnings, GetterNullResilienceData? getterNullResilienceData, bool useDelegateInvokeParameterTypes, bool useDelegateInvokeReturnType, MethodSymbol? delegateInvokeMethodOpt, VariableState? initialState, MethodSymbol? baseOrThisInitializer, ImmutableDictionary<BoundExpression, (NullabilityInfo, TypeSymbol?)>.Builder? analyzedNullabilityMapOpt, SnapshotManager.Builder? snapshotBuilderOpt, ArrayBuilder<(BoundReturnStatement, TypeWithAnnotations)>? returnTypesOpt, bool getFinalNullableState, out VariableState? finalNullableState, bool requiresAnalysis = true)
	{
		NullableWalker nullableWalker = new NullableWalker(compilation, symbol, useConstructorExitWarnings, getterNullResilienceData, useDelegateInvokeParameterTypes, useDelegateInvokeReturnType, delegateInvokeMethodOpt, node, binder, conversions, (initialState == null) ? null : Variables.Create(initialState.Variables), baseOrThisInitializer, returnTypesOpt, analyzedNullabilityMapOpt, snapshotBuilderOpt);
		finalNullableState = null;
		try
		{
			Analyze(nullableWalker, symbol, diagnostics, (initialState == null) ? default(Optional<LocalState>) : ((Optional<LocalState>)LocalState.Create(initialState.VariableNullableStates)), snapshotBuilderOpt, requiresAnalysis);
			if (getFinalNullableState)
			{
				finalNullableState = GetVariableState(nullableWalker._variables, nullableWalker.State);
			}
		}
		finally
		{
			nullableWalker.Free();
		}
	}

	private static void Analyze(NullableWalker walker, Symbol? symbol, DiagnosticBag? diagnostics, Optional<LocalState> initialState, SnapshotManager.Builder? snapshotBuilderOpt, bool requiresAnalysis = true)
	{
		int previousSlot = snapshotBuilderOpt?.EnterNewWalker(symbol) ?? (-1);
		try
		{
			bool badRegion = false;
			walker.Analyze(ref badRegion, initialState);
			diagnostics?.AddRange(walker.Diagnostics);
		}
		catch (CancelledByStackGuardException ex) when (diagnostics != null)
		{
			ex.AddAnError(diagnostics);
		}
		finally
		{
			snapshotBuilderOpt?.ExitWalker(walker.SaveSharedState(), previousSlot);
		}
		walker.RecordNullableAnalysisData(symbol, requiresAnalysis);
	}

	private void RecordNullableAnalysisData(Symbol? symbol, bool requiredAnalysis)
	{
		if (!(compilation.TestOnlyCompilationData is NullableAnalysisData nullableAnalysisData))
		{
			return;
		}
		ConcurrentDictionary<object, Data> data = nullableAnalysisData.Data;
		if (data != null)
		{
			object key = ((object)symbol) ?? ((object)methodMainNode.Syntax);
			if (!data.TryGetValue(key, out var _))
			{
				data.TryAdd(key, new Data(_variables.GetTotalVariableCount(), requiredAnalysis));
			}
		}
	}

	private SharedWalkerState SaveSharedState()
	{
		return new SharedWalkerState(_variables.CreateSnapshot());
	}

	private void TakeIncrementalSnapshot(BoundNode? node)
	{
		_snapshotBuilderOpt?.TakeIncrementalSnapshot(node, State);
	}

	private void SetUpdatedSymbol(BoundNode node, Symbol originalSymbol, Symbol updatedSymbol)
	{
		if (_snapshotBuilderOpt == null)
		{
			return;
		}
		bool flag = false;
		if (node is BoundLambda boundLambda && originalSymbol is LambdaSymbol l && updatedSymbol is NamedTypeSymbol n)
		{
			if (!AreLambdaAndNewDelegateSimilar(l, n))
			{
				return;
			}
			flag = updatedSymbol.Equals(boundLambda.Type.GetDelegateType(), TypeCompareKind.ConsiderEverything);
		}
		if (flag || Symbol.Equals(originalSymbol, updatedSymbol, TypeCompareKind.ConsiderEverything))
		{
			_snapshotBuilderOpt.RemoveSymbolIfPresent(node, originalSymbol);
		}
		else
		{
			_snapshotBuilderOpt.SetUpdatedSymbol(node, originalSymbol, updatedSymbol);
		}
	}

	private NullableFlowState GetState(ref LocalState state, int slot)
	{
		if (!state.Reachable)
		{
			return NullableFlowState.NotNull;
		}
		NormalizeIfNeeded(ref state, slot, useNotNullsAsDefault: false);
		return state[slot];
	}

	private void SetState(ref LocalState state, int slot, NullableFlowState value, bool useNotNullsAsDefault = false)
	{
		if (state.Reachable)
		{
			NormalizeIfNeeded(ref state, slot, useNotNullsAsDefault);
			state[slot] = value;
		}
	}

	private void NormalizeIfNeeded(ref LocalState state, int slot, bool useNotNullsAsDefault)
	{
		state.NormalizeIfNeeded(slot, this, _variables, useNotNullsAsDefault);
	}

	protected override void Normalize(ref LocalState state)
	{
		if (state.Reachable)
		{
			state.Normalize(this, _variables);
		}
	}

	private NullableFlowState GetDefaultState(ref LocalState state, int slot)
	{
		if (!state.Reachable)
		{
			return NullableFlowState.NotNull;
		}
		Symbol symbol = _variables[slot].Symbol;
		switch (symbol.Kind)
		{
		case SymbolKind.Local:
		{
			LocalSymbol localSymbol = (LocalSymbol)symbol;
			if (!_variables.TryGetType(localSymbol, out var type2))
			{
				type2 = localSymbol.TypeWithAnnotations;
			}
			return type2.ToTypeWithState().State;
		}
		case SymbolKind.Parameter:
		{
			ParameterSymbol parameterSymbol = (ParameterSymbol)symbol;
			if (!_variables.TryGetType(parameterSymbol, out var type))
			{
				type = parameterSymbol.TypeWithAnnotations;
			}
			return GetParameterState(type, parameterSymbol.FlowAnalysisAnnotations).State;
		}
		case SymbolKind.Event:
		case SymbolKind.Field:
		case SymbolKind.Property:
			return GetDefaultState(symbol);
		case SymbolKind.ErrorType:
			return NullableFlowState.NotNull;
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
		}
	}

	protected override bool TryGetReceiverAndMember(BoundExpression expr, out BoundExpression? receiver, [NotNullWhen(true)] out Symbol? member)
	{
		receiver = null;
		member = null;
		switch (expr.Kind)
		{
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)expr;
			FieldSymbol fieldSymbol = (FieldSymbol)(member = boundFieldAccess.FieldSymbol);
			if (fieldSymbol.IsFixedSizeBuffer)
			{
				return false;
			}
			if (fieldSymbol.IsStatic)
			{
				return true;
			}
			receiver = boundFieldAccess.ReceiverOpt;
			break;
		}
		case BoundKind.EventAccess:
		{
			BoundEventAccess boundEventAccess = (BoundEventAccess)expr;
			if ((member = boundEventAccess.EventSymbol).IsStatic)
			{
				return true;
			}
			receiver = boundEventAccess.ReceiverOpt;
			break;
		}
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)expr;
			if ((member = boundPropertyAccess.PropertySymbol).IsStatic)
			{
				return true;
			}
			receiver = boundPropertyAccess.ReceiverOpt;
			break;
		}
		}
		if ((object)member != null && receiver != null && receiver.Kind != BoundKind.TypeExpression)
		{
			return (object)receiver.Type != null;
		}
		return false;
	}

	protected override int MakeSlot(BoundExpression node)
	{
		return makeSlot(node);
		int getPlaceholderSlot(BoundExpression expr)
		{
			if (_placeholderLocalsOpt != null && _placeholderLocalsOpt.TryGetValue(expr, out PlaceholderLocal value))
			{
				return GetOrCreateSlot(value);
			}
			return -1;
		}
		static MethodSymbol? getTopLevelMethod(MethodSymbol? method)
		{
			while ((object)method != null)
			{
				Symbol containingSymbol = method.ContainingSymbol;
				if (containingSymbol.Kind == SymbolKind.NamedType)
				{
					return method;
				}
				method = containingSymbol as MethodSymbol;
			}
			return null;
		}
		int makeSlot(BoundExpression boundExpression)
		{
			BoundConversion boundConversion;
			switch (boundExpression.Kind)
			{
			case BoundKind.ThisReference:
			case BoundKind.BaseReference:
			{
				ParameterSymbol parameterSymbol = getTopLevelMethod(_symbol as MethodSymbol)?.ThisParameter;
				if ((object)parameterSymbol == null)
				{
					return -1;
				}
				return GetOrCreateSlot(parameterSymbol);
			}
			case BoundKind.Conversion:
			{
				int num2 = getPlaceholderSlot(boundExpression);
				if (num2 > 0)
				{
					return num2;
				}
				boundConversion = (BoundConversion)boundExpression;
				ConversionKind kind = boundConversion.Conversion.Kind;
				if (kind <= ConversionKind.Boxing)
				{
					if (kind == ConversionKind.Identity || kind == ConversionKind.ImplicitTupleLiteral || kind - 12 <= ConversionKind.NoConversion)
					{
						goto IL_01a1;
					}
				}
				else if (kind <= ConversionKind.ConditionalExpression)
				{
					if (kind != ConversionKind.ExplicitNullable)
					{
						if (kind - 35 <= ConversionKind.NoConversion)
						{
							goto IL_017b;
						}
					}
					else
					{
						BoundExpression operand = boundConversion.Operand;
						TypeSymbol type = operand.Type;
						TypeSymbol type2 = boundConversion.Type;
						if (AreNullableAndUnderlyingTypes(type, type2, out var _))
						{
							int num3 = MakeSlot(operand);
							Symbol valueProperty;
							if (num3 >= 0)
							{
								return GetNullableOfTValueSlot(type, num3, out valueProperty);
							}
							return -1;
						}
					}
				}
				else
				{
					if (kind == ConversionKind.DefaultLiteral)
					{
						goto IL_01a1;
					}
					if (kind == ConversionKind.ObjectCreation)
					{
						goto IL_017b;
					}
				}
				goto IL_01de;
			}
			case BoundKind.DefaultLiteral:
			case BoundKind.DefaultExpression:
			case BoundKind.ObjectCreationExpression:
			case BoundKind.TupleLiteral:
			case BoundKind.ConvertedTupleLiteral:
			case BoundKind.DynamicObjectCreationExpression:
			case BoundKind.AnonymousObjectCreationExpression:
			case BoundKind.NewT:
				return getPlaceholderSlot(boundExpression);
			case BoundKind.ConditionalAccess:
				return getPlaceholderSlot(boundExpression);
			case BoundKind.ConditionalReceiver:
				return _lastConditionalAccessSlot;
			default:
				{
					int num = getPlaceholderSlot(boundExpression);
					if (num <= 0)
					{
						return base.MakeSlot(boundExpression);
					}
					return num;
				}
				IL_01a1:
				return MakeSlot(boundConversion.Operand);
				IL_017b:
				if (IsTargetTypedExpression(boundConversion.Operand) && TypeSymbol.Equals(boundConversion.Type, boundConversion.Operand.Type, TypeCompareKind.IgnoreNullableModifiersForReferenceTypes))
				{
					goto IL_01a1;
				}
				goto IL_01de;
				IL_01de:
				return -1;
			}
		}
	}

	protected override int GetOrCreateSlot(Symbol symbol, int containingSlot = 0, bool forceSlotEvenIfEmpty = false, bool createIfMissing = true)
	{
		if (containingSlot > 0 && !IsSlotMember(containingSlot, symbol))
		{
			return -1;
		}
		if (symbol is ParameterSymbol key && symbol.ContainingSymbol is SynthesizedPrimaryConstructor synthesizedPrimaryConstructor && synthesizedPrimaryConstructor.GetCapturedParameters().TryGetValue(key, out FieldSymbol value))
		{
			MethodSymbol methodSymbol = _symbol as MethodSymbol;
			while (true)
			{
				bool flag;
				switch (methodSymbol?.MethodKind)
				{
				case MethodKind.AnonymousFunction:
				case MethodKind.LocalFunction:
					flag = true;
					break;
				default:
					flag = false;
					break;
				}
				if (!flag)
				{
					break;
				}
				methodSymbol = methodSymbol.ContainingSymbol as MethodSymbol;
			}
			if ((object)methodSymbol != null && methodSymbol.TryGetThisParameter(out ParameterSymbol thisParameter) && (object)thisParameter?.ContainingSymbol.ContainingSymbol == synthesizedPrimaryConstructor.ContainingSymbol)
			{
				int orCreateSlot = GetOrCreateSlot(thisParameter);
				if (orCreateSlot >= 0)
				{
					symbol = value;
					containingSlot = orCreateSlot;
				}
			}
		}
		if (_symbol is MethodSymbol methodSymbol2 && methodSymbol2.IsConstructor() && methodSymbol2.IsStatic == symbol.IsStatic && ((methodSymbol2.IsStatic && containingSlot == 0 && methodSymbol2.ContainingType.Equals(symbol.ContainingType)) || (!methodSymbol2.IsStatic && containingSlot > 0 && _variables[containingSlot].Symbol is ThisParameterSymbol)))
		{
			if (symbol is SynthesizedBackingFieldSymbol { AssociatedSymbol: SourcePropertySymbolBase { UsesFieldKeyword: false } associatedSymbol })
			{
				symbol = associatedSymbol;
			}
			else if (symbol is SourcePropertySymbolBase { UsesFieldKeyword: not false, BackingField: { } backingField })
			{
				symbol = backingField;
			}
			else if (symbol is SourceEventFieldSymbol sourceEventFieldSymbol)
			{
				symbol = sourceEventFieldSymbol.AssociatedSymbol;
			}
		}
		return base.GetOrCreateSlot(symbol, containingSlot, forceSlotEvenIfEmpty, createIfMissing);
	}

	private void VisitAndUnsplitAll<T>(ImmutableArray<T> nodes) where T : BoundNode
	{
		if (!nodes.IsDefault)
		{
			foreach (T item in nodes)
			{
				Visit(item);
				Unsplit();
			}
		}
	}

	private void VisitWithoutDiagnostics(BoundNode? node)
	{
		bool disableDiagnostics = _disableDiagnostics;
		_disableDiagnostics = true;
		Visit(node);
		_disableDiagnostics = disableDiagnostics;
	}

	protected override void VisitRvalue(BoundExpression? node, bool isKnownToBeAnLvalue = false)
	{
		Visit(node);
		VisitRvalueEpilogue(node);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void VisitRvalueEpilogue(BoundExpression? node)
	{
		Unsplit();
		UseRvalueOnly(node);
	}

	private TypeWithState VisitRvalueWithState(BoundExpression? node)
	{
		VisitRvalue(node);
		return ResultType;
	}

	private TypeWithAnnotations VisitLvalueWithAnnotations(BoundExpression node)
	{
		VisitLValue(node);
		Unsplit();
		return LvalueResultType;
	}

	private static object GetTypeAsDiagnosticArgument(TypeSymbol? typeOpt)
	{
		return ((object)typeOpt) ?? ((object)"<null>");
	}

	private static object GetParameterAsDiagnosticArgument(ParameterSymbol? parameterOpt)
	{
		if ((object)parameterOpt != null)
		{
			return new FormattedSymbol(parameterOpt, SymbolDisplayFormat.ShortFormat);
		}
		return "";
	}

	private static object GetContainingSymbolAsDiagnosticArgument(ParameterSymbol? parameterOpt)
	{
		Symbol symbol = parameterOpt?.ContainingSymbol;
		if ((object)symbol != null)
		{
			return new FormattedSymbol(symbol, SymbolDisplayFormat.MinimallyQualifiedFormat);
		}
		return "";
	}

	private static bool ShouldReportNullableAssignment(TypeWithAnnotations type, NullableFlowState state)
	{
		if (!type.HasType || type.Type.IsValueType)
		{
			return false;
		}
		NullableAnnotation nullableAnnotation = type.NullableAnnotation;
		if (nullableAnnotation - 1 <= NullableAnnotation.Oblivious)
		{
			return false;
		}
		switch (state)
		{
		case NullableFlowState.NotNull:
			return false;
		case NullableFlowState.MaybeNull:
			if (type.Type.IsTypeParameterDisallowingAnnotationInCSharp8() && (!(type.Type is TypeParameterSymbol { IsNotNullable: var isNotNullable }) || !(isNotNullable ?? false)))
			{
				return false;
			}
			break;
		}
		return true;
	}

	private void ReportNullableAssignmentIfNecessary(BoundExpression? value, TypeWithAnnotations targetType, TypeWithState valueType, bool useLegacyWarnings, AssignmentKind assignmentKind = AssignmentKind.Assignment, ParameterSymbol? parameterOpt = null, Location? location = null)
	{
		if ((targetType.HasType && !targetType.Type.Equals(valueType.Type, TypeCompareKind.AllIgnoreOptions)) || value == null || !ShouldReportNullableAssignment(targetType, valueType.State))
		{
			return;
		}
		if ((object)location == null)
		{
			location = value.Syntax.GetLocation();
		}
		if (SkipReferenceConversions(value).IsSuppressed)
		{
			return;
		}
		ConstantValue? constantValueOpt = value.ConstantValueOpt;
		if ((object)constantValueOpt != null && constantValueOpt.IsNull && !useLegacyWarnings)
		{
			ReportDiagnostic((assignmentKind == AssignmentKind.Return) ? ErrorCode.WRN_NullReferenceReturn : ErrorCode.WRN_NullAsNonNullable, location);
		}
		else if (assignmentKind == AssignmentKind.Argument)
		{
			ReportDiagnostic(ErrorCode.WRN_NullReferenceArgument, location, GetParameterAsDiagnosticArgument(parameterOpt), GetContainingSymbolAsDiagnosticArgument(parameterOpt));
			LearnFromNonNullTest(value, ref State);
		}
		else if (useLegacyWarnings)
		{
			if (!isMaybeDefaultValue(valueType) || allowUnconstrainedTypeParameterAnnotations(compilation))
			{
				ReportNonSafetyDiagnostic(location);
			}
		}
		else
		{
			ReportDiagnostic((assignmentKind == AssignmentKind.Return) ? ErrorCode.WRN_NullReferenceReturn : ErrorCode.WRN_NullReferenceAssignment, location);
		}
		static bool allowUnconstrainedTypeParameterAnnotations(CSharpCompilation compilation)
		{
			return MessageID.IDS_FeatureDefaultTypeParameterConstraint.RequiredVersion() <= compilation.LanguageVersion;
		}
		static bool isMaybeDefaultValue(TypeWithState typeWithState)
		{
			TypeSymbol? type = typeWithState.Type;
			if ((object)type != null && type.TypeKind == TypeKind.TypeParameter)
			{
				return typeWithState.State == NullableFlowState.MaybeDefault;
			}
			return false;
		}
	}

	internal static bool AreParameterAnnotationsCompatible(RefKind refKind, TypeWithAnnotations overriddenType, FlowAnalysisAnnotations overriddenAnnotations, TypeWithAnnotations overridingType, FlowAnalysisAnnotations overridingAnnotations, bool forRef = false)
	{
		bool flag;
		switch (refKind)
		{
		case RefKind.Ref:
			if (AreParameterAnnotationsCompatible(RefKind.None, overriddenType, overriddenAnnotations, overridingType, overridingAnnotations, forRef: true))
			{
				return AreParameterAnnotationsCompatible(RefKind.Out, overriddenType, overriddenAnnotations, overridingType, overridingAnnotations);
			}
			return false;
		case RefKind.None:
		case RefKind.In:
		case RefKind.RefReadOnlyParameter:
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			if (isBadAssignment(GetParameterState(overriddenType, overriddenAnnotations), overridingType, overridingAnnotations))
			{
				return false;
			}
			bool flag2 = (overridingAnnotations & FlowAnalysisAnnotations.NotNull) == FlowAnalysisAnnotations.NotNull;
			if ((overriddenAnnotations & FlowAnalysisAnnotations.NotNull) == FlowAnalysisAnnotations.NotNull && !flag2 && !forRef)
			{
				return false;
			}
			bool flag3 = (overridingAnnotations & FlowAnalysisAnnotations.MaybeNull) == FlowAnalysisAnnotations.MaybeNull;
			if ((overriddenAnnotations & FlowAnalysisAnnotations.MaybeNull) == FlowAnalysisAnnotations.MaybeNull && !flag3 && !forRef)
			{
				return false;
			}
		}
		if (refKind == RefKind.Out && (!canAssignOutputValueWhen(sense: true) || !canAssignOutputValueWhen(sense: false)))
		{
			return false;
		}
		return true;
		bool canAssignOutputValueWhen(bool sense)
		{
			if (isBadAssignment(ApplyUnconditionalAnnotations(overridingType.ToTypeWithState(), makeUnconditionalAnnotation(overridingAnnotations, sense)), destinationAnnotations: ToInwardAnnotations(makeUnconditionalAnnotation(overriddenAnnotations, sense)), destinationType: overriddenType))
			{
				return false;
			}
			return true;
		}
		static bool isBadAssignment(TypeWithState valueState, TypeWithAnnotations destinationType, FlowAnalysisAnnotations destinationAnnotations)
		{
			if (ShouldReportNullableAssignment(ApplyLValueAnnotations(destinationType, destinationAnnotations), valueState.State))
			{
				return true;
			}
			if (IsDisallowedNullAssignment(valueState, destinationAnnotations))
			{
				return true;
			}
			return false;
		}
		static FlowAnalysisAnnotations makeUnconditionalAnnotation(FlowAnalysisAnnotations annotations, bool sense)
		{
			if (sense)
			{
				return makeUnconditionalAnnotationCore(makeUnconditionalAnnotationCore(annotations, FlowAnalysisAnnotations.NotNullWhenTrue, FlowAnalysisAnnotations.NotNull), FlowAnalysisAnnotations.MaybeNullWhenTrue, FlowAnalysisAnnotations.MaybeNull);
			}
			return makeUnconditionalAnnotationCore(makeUnconditionalAnnotationCore(annotations, FlowAnalysisAnnotations.NotNullWhenFalse, FlowAnalysisAnnotations.NotNull), FlowAnalysisAnnotations.MaybeNullWhenFalse, FlowAnalysisAnnotations.MaybeNull);
		}
		static FlowAnalysisAnnotations makeUnconditionalAnnotationCore(FlowAnalysisAnnotations annotations, FlowAnalysisAnnotations conditionalAnnotation, FlowAnalysisAnnotations replacementAnnotation)
		{
			if ((annotations & conditionalAnnotation) != FlowAnalysisAnnotations.None)
			{
				return annotations | replacementAnnotation;
			}
			return annotations & ~replacementAnnotation;
		}
	}

	private static bool IsDefaultValue(BoundExpression expr)
	{
		switch (expr.Kind)
		{
		case BoundKind.Conversion:
		{
			BoundConversion boundConversion = (BoundConversion)expr;
			ConversionKind kind = boundConversion.Conversion.Kind;
			if (kind == ConversionKind.DefaultLiteral || kind == ConversionKind.NullLiteral)
			{
				return IsDefaultValue(boundConversion.Operand);
			}
			return false;
		}
		case BoundKind.DefaultLiteral:
		case BoundKind.DefaultExpression:
			return true;
		default:
			return false;
		}
	}

	private void ReportNullabilityMismatchInAssignment(SyntaxNode syntaxNode, object sourceType, object destinationType)
	{
		ReportDiagnostic(ErrorCode.WRN_NullabilityMismatchInAssignment, syntaxNode, sourceType, destinationType);
	}

	private void ReportNullabilityMismatchInAssignment(Location location, object sourceType, object destinationType)
	{
		ReportDiagnostic(ErrorCode.WRN_NullabilityMismatchInAssignment, location, sourceType, destinationType);
	}

	private void TrackNullableStateForAssignment(BoundExpression? valueOpt, TypeWithAnnotations targetType, int targetSlot, TypeWithState valueType, int valueSlot = -1)
	{
		if (!State.Reachable || !targetType.HasType || targetSlot <= 0 || targetSlot == valueSlot)
		{
			return;
		}
		NullableFlowState state = valueType.State;
		SetStateAndTrackForFinally(ref State, targetSlot, state);
		InheritDefaultState(targetType.Type, targetSlot);
		if (!areEquivalentTypes(targetType, valueType))
		{
			return;
		}
		if (targetType.Type.IsReferenceType || targetType.TypeKind == TypeKind.TypeParameter || targetType.IsNullableType())
		{
			if (valueSlot > 0)
			{
				InheritNullableStateOfTrackableType(targetSlot, valueSlot, targetSlot);
			}
		}
		else if (EmptyStructTypeCache.IsTrackableStructType(targetType.Type))
		{
			InheritNullableStateOfTrackableStruct(targetType.Type, targetSlot, valueSlot, valueOpt != null && IsDefaultValue(valueOpt), targetSlot);
		}
		static bool areEquivalentTypes(TypeWithAnnotations target, TypeWithState assignedValue)
		{
			return target.Type.Equals(assignedValue.Type, TypeCompareKind.AllIgnoreOptions);
		}
	}

	private void ReportNonSafetyDiagnostic(Location location)
	{
		ReportDiagnostic(ErrorCode.WRN_ConvertingNullableToNonNullable, location);
	}

	private void ReportDiagnostic(ErrorCode errorCode, SyntaxNode syntaxNode, params object[] arguments)
	{
		ReportDiagnostic(errorCode, syntaxNode.GetLocation(), arguments);
	}

	private void ReportDiagnostic(ErrorCode errorCode, Location location, params object[] arguments)
	{
		if (IsReachable() && !_disableDiagnostics)
		{
			base.Diagnostics.Add(errorCode, location, arguments);
		}
	}

	private void InheritNullableStateOfTrackableStruct(TypeSymbol targetType, int targetSlot, int valueSlot, bool isDefaultValue, int skipSlot = -1)
	{
		if (skipSlot < 0)
		{
			skipSlot = targetSlot;
		}
		if (!isDefaultValue && valueSlot > 0)
		{
			InheritNullableStateOfTrackableType(targetSlot, valueSlot, skipSlot);
			return;
		}
		foreach (FieldSymbol structInstanceField in _emptyStructTypeCache.GetStructInstanceFields(targetType))
		{
			InheritNullableStateOfMember(targetSlot, valueSlot, structInstanceField, isDefaultValue, skipSlot);
		}
	}

	private bool IsSlotMember(int slot, Symbol possibleMember)
	{
		TypeSymbol containingType = possibleMember.ContainingType;
		TypeSymbol source = NominalSlotType(slot);
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
		Conversions conversions = _conversions.WithNullability(includeNullability: false);
		if (!conversions.HasIdentityOrImplicitReferenceConversion(source, containingType, ref useSiteInfo))
		{
			return conversions.HasBoxingConversion(source, containingType, ref useSiteInfo);
		}
		return true;
	}

	private void InheritNullableStateOfMember(int targetContainerSlot, int valueContainerSlot, Symbol member, bool isDefaultValue, int skipSlot)
	{
		if (!IsSlotMember(targetContainerSlot, member) || (member is SynthesizedBackingFieldSymbol backingField && !isUsable(backingField)))
		{
			return;
		}
		TypeWithAnnotations typeOrReturnTypeWithAnnotations = GetTypeOrReturnTypeWithAnnotations(member);
		if (typeOrReturnTypeWithAnnotations.Type.IsReferenceType || typeOrReturnTypeWithAnnotations.TypeKind == TypeKind.TypeParameter || typeOrReturnTypeWithAnnotations.IsNullableType())
		{
			int orCreateSlot = GetOrCreateSlot(member, targetContainerSlot);
			if (orCreateSlot <= 0)
			{
				return;
			}
			NullableFlowState newState = (isDefaultValue ? NullableFlowState.MaybeNull : typeOrReturnTypeWithAnnotations.ToTypeWithState().State);
			int num = -1;
			if (valueContainerSlot > 0)
			{
				num = VariableSlot(member, valueContainerSlot);
				if (num == skipSlot)
				{
					return;
				}
				newState = ((num > 0) ? GetState(ref State, num) : NullableFlowState.NotNull);
			}
			SetStateAndTrackForFinally(ref State, orCreateSlot, newState);
			if (num > 0)
			{
				InheritNullableStateOfTrackableType(orCreateSlot, num, skipSlot);
			}
		}
		else
		{
			if (!EmptyStructTypeCache.IsTrackableStructType(typeOrReturnTypeWithAnnotations.Type))
			{
				return;
			}
			int orCreateSlot2 = GetOrCreateSlot(member, targetContainerSlot);
			if (orCreateSlot2 > 0)
			{
				int num2 = ((valueContainerSlot > 0) ? GetOrCreateSlot(member, valueContainerSlot) : (-1));
				if (num2 != skipSlot)
				{
					InheritNullableStateOfTrackableStruct(typeOrReturnTypeWithAnnotations.Type, orCreateSlot2, num2, isDefaultValue, skipSlot);
				}
			}
		}
		bool isUsable(SynthesizedBackingFieldSymbol synthesizedBackingFieldSymbol)
		{
			if (!(_symbol is MethodSymbol methodSymbol))
			{
				return false;
			}
			if (methodSymbol.IsConstructor() && methodSymbol.IsStatic == synthesizedBackingFieldSymbol.IsStatic)
			{
				return true;
			}
			if (methodSymbol is SourcePropertyAccessorSymbol { AssociatedSymbol: PropertySymbol associatedSymbol } && (object)synthesizedBackingFieldSymbol.AssociatedSymbol == associatedSymbol)
			{
				return true;
			}
			return false;
		}
	}

	private TypeSymbol NominalSlotType(int slot)
	{
		return GetTypeOrReturnType(_variables[slot].Symbol);
	}

	private void SetStateAndTrackForFinally(ref LocalState state, int slot, NullableFlowState newState)
	{
		SetState(ref state, slot, newState);
		if (newState != NullableFlowState.NotNull && NonMonotonicState.HasValue)
		{
			LocalState state2 = NonMonotonicState.Value;
			if (state2.HasVariable(slot))
			{
				SetState(ref state2, slot, newState.Join(GetState(ref state2, slot)), useNotNullsAsDefault: true);
				NonMonotonicState = state2;
			}
		}
	}

	protected override void JoinTryBlockState(ref LocalState self, ref LocalState other)
	{
		LocalState other2 = other.GetStateForVariables(self.Id);
		Join(ref self, ref other2);
	}

	private void InheritDefaultState(TypeSymbol targetType, int targetSlot)
	{
		ArrayBuilder<(LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier, int)> instance = ArrayBuilder<(LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier, int)>.GetInstance();
		_variables.GetMembers(instance, targetSlot);
		foreach (var item3 in instance)
		{
			LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier item = item3.Item1;
			int item2 = item3.Item2;
			Symbol symbol = AsMemberOfType(targetType, item.Symbol);
			SetStateAndTrackForFinally(ref State, item2, GetDefaultState(symbol));
			InheritDefaultState(GetTypeOrReturnType(symbol), item2);
		}
		instance.Free();
	}

	private static TypeSymbol GetTypeOrReturnType(Symbol symbol)
	{
		return symbol.GetTypeOrReturnType().Type;
	}

	private TypeWithAnnotations GetTypeOrReturnTypeWithAnnotations(Symbol symbol)
	{
		TypeWithAnnotations result = symbol.GetTypeOrReturnType();
		if (symbol is SynthesizedBackingFieldSymbol { InfersNullableAnnotation: not false } synthesizedBackingFieldSymbol)
		{
			GetterNullResilienceData? getterNullResilienceData = _getterNullResilienceData;
			NullableAnnotation nullableAnnotation;
			if (getterNullResilienceData.HasValue)
			{
				getterNullResilienceData.GetValueOrDefault().Deconstruct(out SynthesizedBackingFieldSymbol field, out NullableAnnotation assumedAnnotation);
				nullableAnnotation = (((object)field == synthesizedBackingFieldSymbol) ? assumedAnnotation : synthesizedBackingFieldSymbol.TypeWithAnnotations.NullableAnnotation);
			}
			else
			{
				nullableAnnotation = synthesizedBackingFieldSymbol.GetInferredNullableAnnotation();
			}
			result = TypeWithAnnotations.Create(result.Type, nullableAnnotation);
		}
		return result;
	}

	private NullableFlowState GetDefaultState(Symbol symbol)
	{
		return ApplyUnconditionalAnnotations(GetTypeOrReturnTypeWithAnnotations(symbol).ToTypeWithState(), GetRValueAnnotations(symbol)).State;
	}

	private void InheritNullableStateOfTrackableType(int targetSlot, int valueSlot, int skipSlot)
	{
		ArrayBuilder<(LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier, int)> instance = ArrayBuilder<(LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier, int)>.GetInstance();
		_variables.GetMembers(instance, valueSlot);
		foreach (var item2 in instance)
		{
			LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier item = item2.Item1;
			Symbol symbol = item.Symbol;
			InheritNullableStateOfMember(targetSlot, valueSlot, symbol, isDefaultValue: false, skipSlot);
		}
		instance.Free();
	}

	protected override LocalState TopState()
	{
		LocalState result = LocalState.ReachableState(_variables);
		result.PopulateAll(this);
		return result;
	}

	protected override LocalState UnreachableState()
	{
		return LocalState.UnreachableState(_variables);
	}

	protected override LocalState ReachableBottomState()
	{
		return LocalState.ReachableStateWithNotNulls(_variables);
	}

	private void EnterParameters()
	{
		if (!(CurrentSymbol is MethodSymbol methodSymbol))
		{
			return;
		}
		if (methodSymbol is SynthesizedPrimaryConstructor)
		{
			if (_hasInitialState)
			{
				return;
			}
		}
		else if (methodSymbol.IsConstructor() && !_hasInitialState)
		{
			return;
		}
		ImmutableArray<ParameterSymbol> parameters = methodSymbol.Parameters;
		ImmutableArray<ParameterSymbol> parameters2 = (_useDelegateInvokeParameterTypes ? _delegateInvokeMethod : methodSymbol).Parameters;
		LocalState other = State.Clone();
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterSymbol parameterSymbol = parameters[i];
			TypeWithAnnotations parameterType = ((i >= parameters2.Length) ? parameterSymbol.TypeWithAnnotations : parameters2[i].TypeWithAnnotations);
			EnterParameter(parameterSymbol, parameterType);
		}
		Join(ref State, ref other);
	}

	private void EnterParameter(ParameterSymbol parameter, TypeWithAnnotations parameterType)
	{
		_variables.SetType(parameter, parameterType);
		if (parameter.RefKind == RefKind.Out)
		{
			return;
		}
		int orCreateSlot = GetOrCreateSlot(parameter);
		if (orCreateSlot > 0)
		{
			NullableFlowState state = GetParameterState(parameterType, parameter.FlowAnalysisAnnotations).State;
			SetState(ref State, orCreateSlot, state);
			if (EmptyStructTypeCache.IsTrackableStructType(parameterType.Type))
			{
				InheritNullableStateOfTrackableStruct(parameterType.Type, orCreateSlot, -1, parameter.ExplicitDefaultConstantValue?.IsNull ?? false);
			}
		}
	}

	public override BoundNode? VisitParameterEqualsValue(BoundParameterEqualsValue equalsValue)
	{
		ParameterSymbol parameter = equalsValue.Parameter;
		FlowAnalysisAnnotations parameterAnnotations = GetParameterAnnotations(parameter);
		TypeWithAnnotations targetTypeOpt = ApplyLValueAnnotations(parameter.TypeWithAnnotations, parameterAnnotations);
		TypeWithState state = VisitOptionalImplicitConversion(equalsValue.Value, targetTypeOpt, useLegacyWarnings: false, trackMembers: false, AssignmentKind.Assignment);
		Unsplit();
		CheckDisallowedNullAssignment(state, parameterAnnotations, equalsValue.Value.Syntax);
		return null;
	}

	internal static TypeWithState GetParameterState(TypeWithAnnotations parameterType, FlowAnalysisAnnotations parameterAnnotations)
	{
		if ((parameterAnnotations & FlowAnalysisAnnotations.AllowNull) != FlowAnalysisAnnotations.None)
		{
			return TypeWithState.Create(parameterType.Type, NullableFlowState.MaybeDefault);
		}
		if ((parameterAnnotations & FlowAnalysisAnnotations.DisallowNull) != FlowAnalysisAnnotations.None)
		{
			return TypeWithState.Create(parameterType.Type, NullableFlowState.NotNull);
		}
		return parameterType.ToTypeWithState();
	}

	public sealed override BoundNode? VisitReturnStatement(BoundReturnStatement node)
	{
		BoundExpression expressionOpt = node.ExpressionOpt;
		if (expressionOpt == null)
		{
			EnforceDoesNotReturn(node.Syntax);
			base.PendingBranches.Add(new AbstractFlowPass<LocalState, LocalFunctionState>.PendingBranch(node, State, null));
			SetUnreachable();
			return null;
		}
		if (_returnTypesOpt == null && TryGetReturnType(out var type, out var annotations))
		{
			if (node.RefKind == RefKind.None && type.Type.SpecialType == SpecialType.System_Boolean)
			{
				Visit(expressionOpt);
			}
			else
			{
				TypeWithState state = ((node.RefKind != RefKind.None) ? VisitRefExpression(expressionOpt, type) : VisitOptionalImplicitConversion(expressionOpt, type, useLegacyWarnings: false, trackMembers: false, AssignmentKind.Return));
				CheckDisallowedNullAssignment(state, ToInwardAnnotations(annotations), node.Syntax, expressionOpt);
			}
		}
		else
		{
			TypeWithState typeWithState = VisitRvalueWithState(expressionOpt);
			if (_returnTypesOpt != null)
			{
				_returnTypesOpt.Add((node, typeWithState.ToTypeWithAnnotations(compilation)));
			}
		}
		EnforceDoesNotReturn(node.Syntax);
		if (IsConditionalState)
		{
			LocalState self = StateWhenTrue.Clone();
			Join(ref self, ref StateWhenFalse);
			base.PendingBranches.Add(new AbstractFlowPass<LocalState, LocalFunctionState>.PendingBranch(node, self, null, IsConditionalState, StateWhenTrue, StateWhenFalse));
		}
		else
		{
			base.PendingBranches.Add(new AbstractFlowPass<LocalState, LocalFunctionState>.PendingBranch(node, State, null));
		}
		Unsplit();
		if (CurrentSymbol is MethodSymbol methodSymbol)
		{
			ImmutableArray<ParameterSymbol> parametersIncludingExtensionParameter = methodSymbol.GetParametersIncludingExtensionParameter(skipExtensionIfStatic: true);
			EnforceNotNullIfNotNull(node.Syntax, State, parametersIncludingExtensionParameter, methodSymbol.ReturnNotNullIfParameterNotNull, ResultType.State, null);
		}
		SetUnreachable();
		return null;
	}

	private TypeWithState VisitRefExpression(BoundExpression expr, TypeWithAnnotations destinationType)
	{
		Visit(expr);
		TypeWithState resultType = ResultType;
		if (!expr.IsSuppressed && RemoveConversion(expr, includeExplicitConversions: false).expression.Kind != BoundKind.ThrowExpression)
		{
			TypeWithAnnotations lvalueResultType = LvalueResultType;
			if (IsNullabilityMismatch(lvalueResultType, destinationType))
			{
				ReportNullabilityMismatchInAssignment(expr.Syntax, lvalueResultType, destinationType);
			}
			else
			{
				ReportNullableAssignmentIfNecessary(expr, destinationType, resultType, useLegacyWarnings: false);
			}
		}
		return resultType;
	}

	private bool TryGetReturnType(out TypeWithAnnotations type, out FlowAnalysisAnnotations annotations)
	{
		if (!(CurrentSymbol is MethodSymbol methodSymbol))
		{
			type = default(TypeWithAnnotations);
			annotations = FlowAnalysisAnnotations.None;
			return false;
		}
		TypeWithAnnotations returnTypeWithAnnotations = (_useDelegateInvokeReturnType ? _delegateInvokeMethod : methodSymbol).ReturnTypeWithAnnotations;
		if (returnTypeWithAnnotations.IsVoidType())
		{
			type = default(TypeWithAnnotations);
			annotations = FlowAnalysisAnnotations.None;
			return false;
		}
		if (!methodSymbol.IsAsync)
		{
			annotations = methodSymbol.ReturnTypeFlowAnalysisAnnotations;
			type = ApplyUnconditionalAnnotations(returnTypeWithAnnotations, annotations);
			return true;
		}
		if (methodSymbol.IsAsyncEffectivelyReturningGenericTask(compilation))
		{
			type = ((NamedTypeSymbol)returnTypeWithAnnotations.Type).TypeArgumentsWithAnnotationsNoUseSiteDiagnostics.Single();
			annotations = FlowAnalysisAnnotations.None;
			return true;
		}
		type = default(TypeWithAnnotations);
		annotations = FlowAnalysisAnnotations.None;
		return false;
	}

	public override BoundNode? VisitLocal(BoundLocal node)
	{
		if ((object)node.Type == compilation.ImplicitlyTypedVariableUsedInForbiddenZoneType)
		{
			SetResultType(node, TypeWithState.ForType(node.Type));
			return null;
		}
		LocalSymbol localSymbol = node.LocalSymbol;
		int orCreateSlot = GetOrCreateSlot(localSymbol);
		TypeWithAnnotations lvalueType = GetDeclaredLocalResult(localSymbol);
		if (!node.Type.Equals(lvalueType.Type, TypeCompareKind.IgnoreDynamicAndTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes))
		{
			lvalueType = TypeWithAnnotations.Create(node.Type, lvalueType.NullableAnnotation);
		}
		SetResult(node, GetAdjustedResult(lvalueType.ToTypeWithState(), orCreateSlot), lvalueType);
		SplitIfBooleanConstant(node);
		return null;
	}

	public override BoundNode? VisitBlock(BoundBlock node)
	{
		DeclareLocals(node.Locals);
		VisitStatementsWithLocalFunctions(node);
		return null;
	}

	private void VisitStatementsWithLocalFunctions(BoundBlock block)
	{
		if (!block.LocalFunctions.IsDefaultOrEmpty)
		{
			ArrayBuilder<BoundLocalFunctionStatement> instance = ArrayBuilder<BoundLocalFunctionStatement>.GetInstance();
			foreach (BoundStatement statement in block.Statements)
			{
				if (statement is BoundLocalFunctionStatement item)
				{
					instance.Add(item);
				}
				else
				{
					VisitStatement(statement);
				}
			}
			int num = instance.Count;
			bool flag = true;
			while (flag)
			{
				flag = false;
				int num2 = 0;
				while (num != 0 && num2 < instance.Count)
				{
					BoundLocalFunctionStatement boundLocalFunctionStatement = instance[num2];
					if (boundLocalFunctionStatement != null && HasLocalFuncUsagesCreated((LocalFunctionSymbol)boundLocalFunctionStatement.Symbol))
					{
						instance[num2] = null;
						num--;
						flag = true;
						TakeIncrementalSnapshot(boundLocalFunctionStatement);
						VisitLocalFunctionStatement(boundLocalFunctionStatement);
					}
					num2++;
				}
				if (flag || num == 0)
				{
					continue;
				}
				for (int i = 0; i < instance.Count; i++)
				{
					BoundLocalFunctionStatement boundLocalFunctionStatement2 = instance[i];
					if (boundLocalFunctionStatement2 != null)
					{
						instance[i] = null;
						num--;
						flag = true;
						TakeIncrementalSnapshot(boundLocalFunctionStatement2);
						VisitLocalFunctionStatement(boundLocalFunctionStatement2);
						break;
					}
				}
			}
			instance.Free();
		}
		else
		{
			foreach (BoundStatement statement2 in block.Statements)
			{
				VisitStatement(statement2);
			}
		}
	}

	public override BoundNode? VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
	{
		LocalFunctionSymbol localFunc = (LocalFunctionSymbol)node.Symbol;
		bool num = HasLocalFuncUsagesCreated(localFunc);
		LocalFunctionState orCreateLocalFuncUsages = GetOrCreateLocalFuncUsages(localFunc);
		LocalState state = TopState();
		if (!num)
		{
			state.Normalize(this, _variables);
			state.ForEach(delegate(int slot, Variables variables)
			{
				if (Symbol.IsCaptured(variables[slot].Symbol, localFunc))
				{
					SetState(ref state, slot, NullableFlowState.NotNull);
				}
			}, _variables);
			orCreateLocalFuncUsages.StartingState = state.Clone();
		}
		else
		{
			LocalState startingState = orCreateLocalFuncUsages.StartingState;
			startingState.ForEach(delegate(int slot, Variables variables)
			{
				if (Symbol.IsCaptured(variables[variables.RootSlot(slot)].Symbol, localFunc))
				{
					SetState(ref state, slot, GetState(ref startingState, slot));
				}
			}, _variables);
		}
		orCreateLocalFuncUsages.Visited = true;
		AnalyzeLocalFunctionOrLambda(node, localFunc, state, null, useDelegateInvokeParameterTypes: false, useDelegateInvokeReturnType: false);
		SetInvalidResult();
		return null;
	}

	private Variables GetOrCreateNestedFunctionVariables(Variables container, MethodSymbol lambdaOrLocalFunction)
	{
		if (_nestedFunctionVariables == null)
		{
			_nestedFunctionVariables = PooledDictionary<MethodSymbol, Variables>.GetInstance();
		}
		if (!_nestedFunctionVariables.TryGetValue(lambdaOrLocalFunction, out Variables value))
		{
			value = container.CreateNestedMethodScope(lambdaOrLocalFunction);
			_nestedFunctionVariables.Add(lambdaOrLocalFunction, value);
		}
		return value;
	}

	private void AnalyzeLocalFunctionOrLambda(IBoundLambdaOrFunction lambdaOrFunction, MethodSymbol lambdaOrFunctionSymbol, LocalState state, MethodSymbol? delegateInvokeMethod, bool useDelegateInvokeParameterTypes, bool useDelegateInvokeReturnType)
	{
		Symbol symbol = _symbol;
		_symbol = lambdaOrFunctionSymbol;
		Symbol currentSymbol = CurrentSymbol;
		CurrentSymbol = lambdaOrFunctionSymbol;
		MethodSymbol delegateInvokeMethod2 = _delegateInvokeMethod;
		_delegateInvokeMethod = delegateInvokeMethod;
		bool useDelegateInvokeParameterTypes2 = _useDelegateInvokeParameterTypes;
		_useDelegateInvokeParameterTypes = useDelegateInvokeParameterTypes;
		bool useDelegateInvokeReturnType2 = _useDelegateInvokeReturnType;
		_useDelegateInvokeReturnType = useDelegateInvokeReturnType;
		ArrayBuilder<(BoundReturnStatement, TypeWithAnnotations)> returnTypesOpt = _returnTypesOpt;
		_returnTypesOpt = null;
		LocalState state2 = State;
		_variables = GetOrCreateNestedFunctionVariables(_variables, lambdaOrFunctionSymbol);
		State = state.CreateNestedMethodState(_variables);
		int previousSlot = _snapshotBuilderOpt?.EnterNewWalker(lambdaOrFunctionSymbol) ?? (-1);
		try
		{
			AbstractFlowPass<LocalState, LocalFunctionState>.SavedPending oldPending = SavePending();
			EnterParameters();
			bool flag = lambdaOrFunctionSymbol is LocalFunctionSymbol;
			if (flag)
			{
				MakeMembersMaybeNull(lambdaOrFunctionSymbol, lambdaOrFunctionSymbol.NotNullMembers);
				MakeMembersMaybeNull(lambdaOrFunctionSymbol, lambdaOrFunctionSymbol.NotNullWhenTrueMembers);
				MakeMembersMaybeNull(lambdaOrFunctionSymbol, lambdaOrFunctionSymbol.NotNullWhenFalseMembers);
			}
			AbstractFlowPass<LocalState, LocalFunctionState>.SavedPending oldPending2 = SavePending();
			if (lambdaOrFunctionSymbol.IsIterator)
			{
				base.PendingBranches.Add(new AbstractFlowPass<LocalState, LocalFunctionState>.PendingBranch(null, State, null));
			}
			VisitAlways(lambdaOrFunction.Body);
			EnforceDoesNotReturn(null);
			if (flag)
			{
				enforceMemberNotNull(((LocalFunctionSymbol)lambdaOrFunctionSymbol).Syntax, State);
			}
			EnforceParameterNotNullOnExit(null, State);
			RestorePending(oldPending2);
			foreach (AbstractFlowPass<LocalState, LocalFunctionState>.PendingBranch item in RemoveReturns())
			{
				if (flag)
				{
					enforceMemberNotNull(item.Branch?.Syntax, item.State);
				}
				if (item.Branch is BoundReturnStatement boundReturnStatement)
				{
					EnforceParameterNotNullOnExit(boundReturnStatement.Syntax, item.State);
					EnforceNotNullWhenForPendingReturn(item, boundReturnStatement);
					if (flag)
					{
						EnforceMemberNotNullWhenForPendingReturn(item, boundReturnStatement);
					}
				}
			}
			RestorePending(oldPending);
		}
		finally
		{
			_snapshotBuilderOpt?.ExitWalker(SaveSharedState(), previousSlot);
		}
		_variables = _variables.Container;
		State = state2;
		_returnTypesOpt = returnTypesOpt;
		_useDelegateInvokeReturnType = useDelegateInvokeReturnType2;
		_useDelegateInvokeParameterTypes = useDelegateInvokeParameterTypes2;
		_delegateInvokeMethod = delegateInvokeMethod2;
		CurrentSymbol = currentSymbol;
		_symbol = symbol;
		void enforceMemberNotNull(SyntaxNode? syntax, LocalState state3)
		{
			if (state3.Reachable)
			{
				LocalFunctionSymbol localFunctionSymbol = (LocalFunctionSymbol)_symbol;
				foreach (string notNullMember in localFunctionSymbol.NotNullMembers)
				{
					EnforceMemberNotNullOnMember(syntax, state3, localFunctionSymbol, notNullMember);
				}
			}
		}
	}

	protected override void VisitLocalFunctionUse(LocalFunctionSymbol symbol, LocalFunctionState localFunctionState, SyntaxNode syntax, bool isCall)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/NullableWalker.cs", 3535);
	}

	private void VisitLocalFunctionUse(LocalFunctionSymbol symbol)
	{
		LocalFunctionState orCreateLocalFuncUsages = GetOrCreateLocalFuncUsages(symbol);
		LocalState other = State.GetStateForVariables(orCreateLocalFuncUsages.StartingState.Id);
		if (Join(ref orCreateLocalFuncUsages.StartingState, ref other) && orCreateLocalFuncUsages.Visited)
		{
			stateChangedAfterUse = true;
		}
	}

	public override BoundNode? VisitDoStatement(BoundDoStatement node)
	{
		DeclareLocals(node.Locals);
		return base.VisitDoStatement(node);
	}

	public override BoundNode? VisitWhileStatement(BoundWhileStatement node)
	{
		DeclareLocals(node.Locals);
		return base.VisitWhileStatement(node);
	}

	public override BoundNode? VisitWithExpression(BoundWithExpression withExpr)
	{
		BoundExpression receiver = withExpr.Receiver;
		VisitRvalue(receiver);
		CheckPossibleNullReceiver(receiver);
		TypeWithAnnotations typeWithAnnotations = ResultType.ToTypeWithAnnotations(compilation);
		TypeWithState typeWithState = ApplyUnconditionalAnnotations(typeWithAnnotations.ToTypeWithState(), GetRValueAnnotations(withExpr.CloneMethod));
		int orCreatePlaceholderSlot = GetOrCreatePlaceholderSlot(withExpr);
		TrackNullableStateForAssignment(receiver, typeWithAnnotations, orCreatePlaceholderSlot, typeWithState, MakeSlot(receiver));
		SetResult(withExpr, typeWithState, typeWithAnnotations);
		VisitObjectCreationInitializer(orCreatePlaceholderSlot, typeWithAnnotations.Type, withExpr.InitializerExpression, delayCompletionForType: false);
		return null;
	}

	public override BoundNode? VisitForStatement(BoundForStatement node)
	{
		DeclareLocals(node.OuterLocals);
		DeclareLocals(node.InnerLocals);
		return base.VisitForStatement(node);
	}

	public override BoundNode? VisitForEachStatement(BoundForEachStatement node)
	{
		DeclareLocals(node.IterationVariables);
		return base.VisitForEachStatement(node);
	}

	public override BoundNode? VisitUsingStatement(BoundUsingStatement node)
	{
		DeclareLocals(node.Locals);
		Visit(node.AwaitOpt);
		return base.VisitUsingStatement(node);
	}

	public override BoundNode? VisitUsingLocalDeclarations(BoundUsingLocalDeclarations node)
	{
		Visit(node.AwaitOpt);
		return base.VisitUsingLocalDeclarations(node);
	}

	public override BoundNode? VisitFixedStatement(BoundFixedStatement node)
	{
		DeclareLocals(node.Locals);
		return base.VisitFixedStatement(node);
	}

	public override BoundNode? VisitConstructorMethodBody(BoundConstructorMethodBody node)
	{
		DeclareLocals(node.Locals);
		return base.VisitConstructorMethodBody(node);
	}

	private void DeclareLocal(LocalSymbol local)
	{
		if (local.DeclarationKind != LocalDeclarationKind.None)
		{
			int orCreateSlot = GetOrCreateSlot(local);
			if (orCreateSlot > 0)
			{
				SetState(ref State, orCreateSlot, GetDefaultState(ref State, orCreateSlot));
				InheritDefaultState(GetDeclaredLocalResult(local).Type, orCreateSlot);
			}
		}
	}

	private void DeclareLocals(ImmutableArray<LocalSymbol> locals)
	{
		foreach (LocalSymbol item in locals)
		{
			DeclareLocal(item);
		}
	}

	public override BoundNode? VisitLocalDeclaration(BoundLocalDeclaration node)
	{
		LocalSymbol localSymbol = node.LocalSymbol;
		int orCreateSlot = GetOrCreateSlot(localSymbol);
		bool disableDiagnostics = _disableDiagnostics;
		_disableDiagnostics = true;
		LocalState state = State;
		VisitAndUnsplitAll(node.ArgumentsOpt);
		_disableDiagnostics = disableDiagnostics;
		SetState(state);
		if (node.DeclaredTypeOpt != null)
		{
			VisitTypeExpression(node.DeclaredTypeOpt);
		}
		BoundExpression initializerOpt = node.InitializerOpt;
		if (initializerOpt == null)
		{
			return null;
		}
		TypeWithAnnotations typeWithAnnotations = localSymbol.TypeWithAnnotations;
		bool inferredType = node.InferredType;
		TypeWithState valueType;
		if (localSymbol.IsRef)
		{
			valueType = VisitRefExpression(initializerOpt, typeWithAnnotations);
		}
		else
		{
			valueType = VisitOptionalImplicitConversion(initializerOpt, inferredType ? default(TypeWithAnnotations) : typeWithAnnotations, useLegacyWarnings: true, trackMembers: true, AssignmentKind.Assignment);
			Unsplit();
		}
		if (inferredType)
		{
			if (valueType.HasNullType)
			{
				valueType = typeWithAnnotations.ToTypeWithState();
			}
			typeWithAnnotations = valueType.ToAnnotatedTypeWithAnnotations(compilation);
			_variables.SetType(localSymbol, typeWithAnnotations);
			if (node.DeclaredTypeOpt != null)
			{
				SetAnalyzedNullability(node.DeclaredTypeOpt, new VisitResult(typeWithAnnotations.ToTypeWithState(), typeWithAnnotations), true);
			}
		}
		TrackNullableStateForAssignment(initializerOpt, typeWithAnnotations, orCreateSlot, valueType, MakeSlot(initializerOpt));
		return null;
	}

	protected override BoundNode? VisitExpressionOrPatternWithoutStackGuard(BoundNode node)
	{
		SetInvalidResult();
		base.VisitExpressionOrPatternWithoutStackGuard(node);
		if (node is BoundExpression node2)
		{
			VisitExpressionWithoutStackGuardEpilogue(node2);
		}
		return null;
	}

	private void VisitExpressionWithoutStackGuardEpilogue(BoundExpression node)
	{
		TypeWithState resultType = ResultType;
		if (shouldMakeNotNullRvalue(node) && _visitResult.NestedVisitResults == null && !_visitResult.StateForLambda.HasValue)
		{
			TypeWithState resultType2 = resultType.WithNotNullState();
			SetResult(node, resultType2, LvalueResultType);
		}
		bool shouldMakeNotNullRvalue(BoundExpression boundExpression)
		{
			if (!boundExpression.IsSuppressed && !boundExpression.HasAnyErrors)
			{
				return !IsReachable();
			}
			return true;
		}
	}

	private static bool AreLambdaAndNewDelegateSimilar(LambdaSymbol l, NamedTypeSymbol n)
	{
		MethodSymbol delegateInvokeMethod = n.DelegateInvokeMethod;
		if (delegateInvokeMethod.Parameters.SequenceEqual(l.Parameters, (ParameterSymbol p1, ParameterSymbol p2) => p1.Type.Equals(p2.Type, TypeCompareKind.AllNullableIgnoreOptions | TypeCompareKind.IgnoreTupleNames)))
		{
			return delegateInvokeMethod.ReturnType.Equals(l.ReturnType, TypeCompareKind.AllNullableIgnoreOptions | TypeCompareKind.IgnoreTupleNames);
		}
		return false;
	}

	public override BoundNode? Visit(BoundNode? node)
	{
		return Visit(node, expressionIsRead: true);
	}

	private BoundNode VisitLValue(BoundNode node)
	{
		return Visit(node, expressionIsRead: false);
	}

	private static bool TypeAllowsConditionalState(TypeSymbol? type)
	{
		if ((object)type != null)
		{
			if (type.SpecialType != SpecialType.System_Boolean && !type.IsDynamic())
			{
				return type.IsErrorType();
			}
			return true;
		}
		return false;
	}

	private void UnsplitIfNeeded(TypeSymbol? type)
	{
		if (!TypeAllowsConditionalState(type))
		{
			Unsplit();
		}
	}

	private BoundNode Visit(BoundNode? node, bool expressionIsRead)
	{
		bool expressionIsRead2 = _expressionIsRead;
		_expressionIsRead = expressionIsRead;
		TakeIncrementalSnapshot(node);
		BoundNode result = base.Visit(node);
		_expressionIsRead = expressionIsRead2;
		return result;
	}

	protected override void VisitStatement(BoundStatement statement)
	{
		SetInvalidResult();
		base.VisitStatement(statement);
		SetInvalidResult();
	}

	public override BoundNode? VisitObjectCreationExpression(BoundObjectCreationExpression node)
	{
		VisitObjectCreationExpressionBase(node);
		return null;
	}

	public override BoundNode? VisitUnconvertedObjectCreationExpression(BoundUnconvertedObjectCreationExpression node)
	{
		SetResultType(node, TypeWithState.Create(null, NullableFlowState.NotNull));
		return null;
	}

	public override BoundNode? VisitUnconvertedCollectionExpression(BoundUnconvertedCollectionExpression node)
	{
		BoundNode result = base.VisitUnconvertedCollectionExpression(node);
		SetResultType(node, TypeWithState.Create(null, NullableFlowState.NotNull));
		return result;
	}

	public override BoundNode? VisitCollectionExpression(BoundCollectionExpression node)
	{
		TypeWithAnnotations item = getCollectionDetails(node, node.Type).Item2;
		ArrayBuilder<VisitResult> instance = ArrayBuilder<VisitResult>.GetInstance(node.Elements.Length);
		ArrayBuilder<Func<TypeWithAnnotations, TypeSymbol, TypeWithState>> elementConversionCompletions = ArrayBuilder<Func<TypeWithAnnotations, TypeSymbol, TypeWithState>>.GetInstance();
		foreach (BoundNode element in node.Elements)
		{
			visitElement(element, node, item, elementConversionCompletions);
			instance.Add(_visitResult);
		}
		if (node.WasTargetTyped)
		{
			TargetTypedAnalysisCompletion[node] = (TypeWithAnnotations resultTypeWithAnnotations) => convertCollection(node, resultTypeWithAnnotations, elementConversionCompletions);
		}
		else
		{
			elementConversionCompletions.Free();
		}
		TypeWithAnnotations typeWithAnnotations = TypeWithAnnotations.Create(node.Type);
		SetResult(visitResult: new VisitResult(TypeWithState.Create(typeWithAnnotations), typeWithAnnotations, instance.ToArrayAndFree()), expression: node, updateAnalyzedNullability: !node.WasTargetTyped, isLvalue: false);
		return null;
		TypeWithState convertCollection(BoundCollectionExpression boundCollectionExpression, TypeWithAnnotations targetCollectionType, ArrayBuilder<Func<TypeWithAnnotations, TypeSymbol, TypeWithState>> completions)
		{
			TypeSymbol typeSymbol = targetCollectionType.Type.StrippedType();
			(CollectionExpressionTypeKind, TypeWithAnnotations) tuple = getCollectionDetails(boundCollectionExpression, typeSymbol);
			CollectionExpressionTypeKind item2 = tuple.Item1;
			TypeWithAnnotations item3 = tuple.Item2;
			foreach (Func<TypeWithAnnotations, TypeSymbol, TypeWithState> completion2 in completions)
			{
				completion2(item3, typeSymbol);
			}
			completions.Free();
			NullableFlowState defaultState = getResultState(boundCollectionExpression, item2);
			TypeWithState typeWithState = TypeWithState.Create(typeSymbol, defaultState);
			SetAnalyzedNullability(boundCollectionExpression, typeWithState);
			return typeWithState;
		}
		(CollectionExpressionTypeKind, TypeWithAnnotations) getCollectionDetails(BoundCollectionExpression boundCollectionExpression, TypeSymbol collectionType)
		{
			CollectionExpressionTypeKind collectionExpressionTypeKind = ConversionsBase.GetCollectionExpressionTypeKind(compilation, collectionType, out var elementType);
			switch (collectionExpressionTypeKind)
			{
			case CollectionExpressionTypeKind.CollectionBuilder:
				if ((object)boundCollectionExpression.CollectionBuilderMethod != null)
				{
					_binder.TryGetCollectionIterationType((ExpressionSyntax)boundCollectionExpression.Syntax, collectionType, out elementType);
				}
				break;
			case CollectionExpressionTypeKind.ImplementsIEnumerable:
				_binder.TryGetCollectionIterationType(boundCollectionExpression.Syntax, collectionType, out elementType);
				break;
			}
			return (collectionExpressionTypeKind, elementType);
		}
		static NullableFlowState getResultState(BoundCollectionExpression boundCollectionExpression, CollectionExpressionTypeKind collectionKind)
		{
			if (collectionKind == CollectionExpressionTypeKind.CollectionBuilder)
			{
				MethodSymbol collectionBuilderMethod = boundCollectionExpression.CollectionBuilderMethod;
				if ((object)collectionBuilderMethod != null)
				{
					FlowAnalysisAnnotations flowAnalysisAnnotations = collectionBuilderMethod.GetFlowAnalysisAnnotations();
					return ApplyUnconditionalAnnotations(collectionBuilderMethod.ReturnTypeWithAnnotations, flowAnalysisAnnotations).ToTypeWithState().State;
				}
			}
			return NullableFlowState.NotNull;
		}
		void visitElement(BoundNode element, BoundCollectionExpression boundCollectionExpression, TypeWithAnnotations targetElementType, ArrayBuilder<Func<TypeWithAnnotations, TypeSymbol, TypeWithState>> arrayBuilder)
		{
			BoundCollectionElementInitializer boundCollectionElementInitializer = element as BoundCollectionElementInitializer;
			if (boundCollectionElementInitializer == null)
			{
				if (element is BoundCollectionExpressionSpreadElement boundCollectionExpressionSpreadElement)
				{
					Visit(boundCollectionExpressionSpreadElement);
					if (targetElementType.HasType)
					{
						BoundValuePlaceholder elementPlaceholder = boundCollectionExpressionSpreadElement.ElementPlaceholder;
						if (elementPlaceholder != null && boundCollectionExpressionSpreadElement.IteratorBody != null)
						{
							VisitResult result = ((boundCollectionExpressionSpreadElement.EnumeratorInfoOpt == null) ? default(VisitResult) : _visitResult);
							BoundExpression expression = ((BoundExpressionStatement)boundCollectionExpressionSpreadElement.IteratorBody).Expression;
							AddPlaceholderReplacement(elementPlaceholder, elementPlaceholder, result);
							visitElement(expression, boundCollectionExpression, targetElementType, arrayBuilder);
							RemovePlaceholderReplacement(elementPlaceholder);
						}
					}
				}
				else
				{
					BoundExpression boundExpression = (BoundExpression)element;
					if (!targetElementType.HasType)
					{
						VisitRvalueWithState(boundExpression);
					}
					else
					{
						Func<TypeWithAnnotations, TypeWithState> completion = VisitOptionalImplicitConversion(boundExpression, targetElementType, useLegacyWarnings: false, trackMembers: false, AssignmentKind.Assignment, delayCompletionForTargetType: true).completion;
						arrayBuilder.Add((TypeWithAnnotations elementType, TypeSymbol _) => completion(elementType));
					}
				}
			}
			else
			{
				SetUnknownResultNullability(boundCollectionElementInitializer);
				SetUnknownResultNullability(boundCollectionExpression.Placeholder);
				int argIndex = (boundCollectionElementInitializer.AddMethod.IsExtensionMethod ? 1 : 0);
				BoundExpression addArgument = boundCollectionElementInitializer.Arguments[argIndex];
				VisitRvalue(addArgument);
				VisitResult addArgumentResult = _visitResult;
				arrayBuilder.Add(delegate(TypeWithAnnotations _, TypeSymbol targetCollectionType)
				{
					MethodSymbol addMethod = boundCollectionElementInitializer.AddMethod;
					MethodSymbol methodSymbol = ((addMethod.IsExtensionMethod || addMethod.IsExtensionBlockMember()) ? addMethod : ((MethodSymbol)AsMemberOfType(targetCollectionType, addMethod)));
					ParameterSymbol parameterSymbol = methodSymbol.Parameters[argIndex];
					return VisitConversion(null, addArgument, Conversion.Identity, parameterSymbol.TypeWithAnnotations, addArgumentResult.RValueType, checkConversion: true, fromExplicitCast: false, useLegacyWarnings: false, AssignmentKind.Argument, parameterSymbol);
				});
			}
		}
	}

	public override BoundNode? VisitCollectionExpressionSpreadElement(BoundCollectionExpressionSpreadElement node)
	{
		VisitRvalue(node.Expression);
		if (node.Conversion is BoundConversion { Conversion: var conversion })
		{
			AddPlaceholderReplacement(node.ExpressionPlaceholder, node.Expression, _visitResult);
			VisitForEachExpression(node, node.Conversion, conversion, node.ExpressionPlaceholder, node.EnumeratorInfoOpt);
			RemovePlaceholderReplacement(node.ExpressionPlaceholder);
		}
		return null;
	}

	private void VisitObjectCreationExpressionBase(BoundObjectCreationExpressionBase node)
	{
		bool wasTargetTyped = node.WasTargetTyped;
		MethodSymbol methodSymbol = getConstructor(node, node.Type);
		ImmutableArray<BoundExpression> arguments = node.Arguments;
		(MethodSymbol? member, ImmutableArray<VisitResult> results, bool returnNotNull, ArgumentsCompletionDelegate<MethodSymbol>? completion) tuple = VisitArguments(node, arguments, node.ArgumentRefKindsOpt, methodSymbol?.Parameters ?? default(ImmutableArray<ParameterSymbol>), node.ArgsToParamsOpt, node.DefaultArguments, node.Expanded, invokedAsExtensionMethod: false, methodSymbol, wasTargetTyped);
		ImmutableArray<VisitResult> item = tuple.results;
		ArgumentsCompletionDelegate<MethodSymbol> item2 = tuple.completion;
		TypeSymbol type = node.Type;
		BoundObjectInitializerExpressionBase initializerExpressionOpt = node.InitializerExpressionOpt;
		(int slot, NullableFlowState resultState, Func<TypeSymbol, MethodSymbol?, int>? completion) tuple2 = inferInitialObjectState(node, type, methodSymbol, arguments, item, wasTargetTyped, initializerExpressionOpt != null);
		int item3 = tuple2.slot;
		NullableFlowState item4 = tuple2.resultState;
		Func<TypeSymbol, MethodSymbol, int> item5 = tuple2.completion;
		Action<int, TypeSymbol> initializerCompletion = null;
		if (initializerExpressionOpt != null)
		{
			initializerCompletion = VisitObjectCreationInitializer(item3, type, initializerExpressionOpt, wasTargetTyped);
		}
		TypeWithState type2 = setAnalyzedNullability(node, type, item, item2, item5, initializerCompletion, item4, wasTargetTyped);
		SetResultType(node, type2, updateAnalyzedNullability: false);
		static MethodSymbol? getConstructor(BoundObjectCreationExpressionBase boundObjectCreationExpressionBase, TypeSymbol type3)
		{
			MethodSymbol methodSymbol2 = boundObjectCreationExpressionBase.Constructor;
			if ((object)methodSymbol2 != null && !type3.IsInterfaceType())
			{
				methodSymbol2 = (MethodSymbol)AsMemberOfType(type3, methodSymbol2);
			}
			return methodSymbol2;
		}
		(int slot, NullableFlowState resultState, Func<TypeSymbol, MethodSymbol?, int>? completion) inferInitialObjectState(BoundExpression boundExpression, TypeSymbol typeSymbol, MethodSymbol? constructor, ImmutableArray<BoundExpression> immutableArray, ImmutableArray<VisitResult> argumentResults, bool isTargetTyped, bool hasObjectInitializer)
		{
			if (isTargetTyped)
			{
				return (slot: -1, resultState: NullableFlowState.NotNull, completion: inferInitialObjectStateAsContinuation(boundExpression, immutableArray, argumentResults, hasObjectInitializer));
			}
			int num = -1;
			NullableFlowState nullableFlowState = NullableFlowState.NotNull;
			if ((object)typeSymbol != null && (hasObjectInitializer || typeSymbol.IsStructType()))
			{
				num = GetOrCreatePlaceholderSlot(boundExpression);
				if (num > 0)
				{
					bool flag = constructor?.IsDefaultValueTypeConstructor() ?? false;
					if (EmptyStructTypeCache.IsTrackableStructType(typeSymbol))
					{
						NamedTypeSymbol namedTypeSymbol = constructor?.ContainingType;
						if ((object)namedTypeSymbol != null && namedTypeSymbol.IsTupleType && !flag)
						{
							ImmutableArray<TypeWithState> types = argumentResults.SelectAsArray((VisitResult ar) => ar.RValueType);
							TrackNullableStateOfTupleElements(num, namedTypeSymbol, immutableArray, types, ((BoundObjectCreationExpression)boundExpression).ArgsToParamsOpt, useRestField: true);
						}
						else
						{
							InheritNullableStateOfTrackableStruct(typeSymbol, num, -1, flag);
						}
					}
					else if (typeSymbol.IsNullableType())
					{
						TypeWithAnnotations underlyingTypeWithAnnotations;
						if (flag)
						{
							nullableFlowState = NullableFlowState.MaybeNull;
						}
						else if ((object)constructor != null && constructor.ParameterCount == 1 && AreNullableAndUnderlyingTypes(typeSymbol, constructor.ParameterTypesWithAnnotations[0].Type, out underlyingTypeWithAnnotations))
						{
							BoundExpression boundExpression2 = immutableArray[0];
							int num2 = MakeSlot(boundExpression2);
							if (num2 > 0)
							{
								TrackNullableStateOfNullableValue(num, typeSymbol, boundExpression2, underlyingTypeWithAnnotations.ToTypeWithState(), num2);
							}
						}
					}
					SetState(ref State, num, nullableFlowState);
				}
			}
			return (slot: num, resultState: nullableFlowState, completion: null);
		}
		Func<TypeSymbol, MethodSymbol?, int> inferInitialObjectStateAsContinuation(BoundExpression node2, ImmutableArray<BoundExpression> arguments2, ImmutableArray<VisitResult> argumentResults, bool hasObjectInitializer)
		{
			return (TypeSymbol type3, MethodSymbol? constructor) => inferInitialObjectState(node2, type3, constructor, arguments2, argumentResults, isTargetTyped: false, hasObjectInitializer).slot;
		}
		TypeWithState setAnalyzedNullability(BoundObjectCreationExpressionBase boundObjectCreationExpressionBase, TypeSymbol? type3, ImmutableArray<VisitResult> argumentResults, ArgumentsCompletionDelegate<MethodSymbol>? argumentsCompletion, Func<TypeSymbol, MethodSymbol?, int>? initialStateInferenceCompletion, Action<int, TypeSymbol>? initializerCompletion2, NullableFlowState resultState, bool isTargetTyped)
		{
			TypeWithState typeWithState = TypeWithState.Create(type3, resultState);
			if (isTargetTyped)
			{
				setAnalyzedNullabilityAsContinuation(boundObjectCreationExpressionBase, argumentResults, argumentsCompletion, initialStateInferenceCompletion, initializerCompletion2, resultState);
			}
			else
			{
				SetAnalyzedNullability(boundObjectCreationExpressionBase, typeWithState);
			}
			return typeWithState;
		}
		void setAnalyzedNullabilityAsContinuation(BoundObjectCreationExpressionBase boundObjectCreationExpressionBase, ImmutableArray<VisitResult> argumentResults, ArgumentsCompletionDelegate<MethodSymbol> argumentsCompletion, Func<TypeSymbol, MethodSymbol?, int> initialStateInferenceCompletion, Action<int, TypeSymbol>? action, NullableFlowState resultState)
		{
			TargetTypedAnalysisCompletion[boundObjectCreationExpressionBase] = delegate(TypeWithAnnotations resultTypeWithAnnotations)
			{
				TypeSymbol type3 = resultTypeWithAnnotations.Type;
				MethodSymbol methodSymbol2 = getConstructor(boundObjectCreationExpressionBase, type3);
				argumentsCompletion(argumentResults, methodSymbol2?.Parameters ?? default(ImmutableArray<ParameterSymbol>), methodSymbol2);
				int arg = initialStateInferenceCompletion(type3, methodSymbol2);
				action?.Invoke(arg, type3);
				return setAnalyzedNullability(boundObjectCreationExpressionBase, type3, argumentResults, null, null, null, resultState, isTargetTyped: false);
			};
		}
	}

	private Action<int, TypeSymbol>? VisitObjectCreationInitializer(int containingSlot, TypeSymbol containingType, BoundObjectInitializerExpressionBase node, bool delayCompletionForType)
	{
		Action<int, TypeSymbol> action = null;
		TakeIncrementalSnapshot(node);
		if (!(node is BoundObjectInitializerExpression boundObjectInitializerExpression))
		{
			if (node is BoundCollectionInitializerExpression boundCollectionInitializerExpression)
			{
				foreach (BoundExpression initializer in boundCollectionInitializerExpression.Initializers)
				{
					if (initializer.Kind == BoundKind.CollectionElementInitializer)
					{
						action = (Action<int, TypeSymbol>)Delegate.Combine(action, VisitCollectionElementInitializer((BoundCollectionElementInitializer)initializer, containingType, delayCompletionForType));
					}
					else
					{
						VisitRvalue(initializer);
					}
				}
				SetNotNullResult(boundCollectionInitializerExpression.Placeholder);
			}
			else
			{
				ExceptionUtilities.UnexpectedValue(node.Kind);
			}
		}
		else
		{
			foreach (BoundExpression initializer2 in boundObjectInitializerExpression.Initializers)
			{
				if (initializer2.Kind == BoundKind.AssignmentOperator)
				{
					action = (Action<int, TypeSymbol>)Delegate.Combine(action, VisitObjectElementInitializer(containingSlot, containingType, (BoundAssignmentOperator)initializer2, delayCompletionForType));
				}
				else
				{
					VisitRvalue(initializer2);
				}
			}
			SetNotNullResult(boundObjectInitializerExpression.Placeholder);
		}
		return action;
	}

	private Action<int, TypeSymbol>? VisitObjectElementInitializer(int containingSlot, TypeSymbol containingType, BoundAssignmentOperator node, bool delayCompletionForType)
	{
		TakeIncrementalSnapshot(node);
		BoundExpression left = node.Left;
		if (left.Kind == BoundKind.ObjectInitializerMember)
		{
			TakeIncrementalSnapshot(left);
			return visitMemberInitializer(containingSlot, containingType, node, delayCompletionForType);
		}
		VisitRvalue(node);
		return null;
		Action<int, Symbol>? completeNestedInitializerAnalysis(Symbol symbol, BoundObjectInitializerExpressionBase initializer, int slot, Action<int, TypeSymbol>? nestedCompletion, bool flag)
		{
			if (flag)
			{
				return completeNestedInitializerAnalysisAsContinuation(initializer, nestedCompletion);
			}
			if (slot >= 0 && !initializer.Initializers.IsEmpty && !initializer.Type.IsValueType && GetState(ref State, slot).MayBeNull())
			{
				ReportDiagnostic(ErrorCode.WRN_NullReferenceInitializer, initializer.Syntax, symbol);
			}
			return null;
		}
		Action<int, Symbol>? completeNestedInitializerAnalysisAsContinuation(BoundObjectInitializerExpressionBase initializer, Action<int, TypeSymbol>? nestedCompletion)
		{
			return delegate(int containingSlot2, Symbol symbol)
			{
				int num = getOrCreateSlot(containingSlot2, symbol);
				completeNestedInitializerAnalysis(symbol, initializer, num, null, delayCompletionForType: false);
				nestedCompletion?.Invoke(num, GetTypeOrReturnType(symbol));
			};
		}
		int getOrCreateSlot(int num, Symbol symbol)
		{
			if (num >= 0 && IsSlotMember(num, symbol))
			{
				return GetOrCreateSlot(symbol, num);
			}
			return -1;
		}
		Symbol? getTargetMember(TypeSymbol type, BoundObjectInitializerMember objectInitializer)
		{
			Symbol memberSymbol = objectInitializer.MemberSymbol;
			if (memberSymbol == null)
			{
				return null;
			}
			if (!memberSymbol.IsExtensionBlockMember())
			{
				return AsMemberOfType(type, memberSymbol);
			}
			NamedTypeSymbol containingType2 = memberSymbol.OriginalDefinition.ContainingType;
			if (containingType2.Arity == 0)
			{
				return memberSymbol;
			}
			PropertySymbol propertySymbol = memberSymbol as PropertySymbol;
			if ((object)propertySymbol != null && !memberSymbol.IsStatic)
			{
				ParameterSymbol extensionParameter = containingType2.ExtensionParameter;
				if ((object)extensionParameter != null)
				{
					CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
					MethodTypeInferenceResult methodTypeInferenceResult = MethodTypeInferrer.Infer(_binder, _conversions, containingType2.TypeParameters, containingType2, ImmutableCollectionsMarshal.AsImmutableArray(new TypeWithAnnotations[1] { extensionParameter.TypeWithAnnotations }), ImmutableCollectionsMarshal.AsImmutableArray(new RefKind[1] { extensionParameter.RefKind }), ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1]
					{
						new BoundExpressionWithNullability(objectInitializer.Syntax, objectInitializer, NullableAnnotation.NotAnnotated, type)
					}), ref useSiteInfo, new MethodInferenceExtensions(this));
					if (methodTypeInferenceResult.Success)
					{
						containingType2 = containingType2.Construct(methodTypeInferenceResult.InferredTypeArguments);
						propertySymbol = propertySymbol.OriginalDefinition.AsMember(containingType2);
						SetUpdatedSymbol(objectInitializer, memberSymbol, propertySymbol);
					}
					return propertySymbol;
				}
			}
			return memberSymbol;
		}
		Action<int, TypeSymbol>? setAnalyzedNullability(BoundAssignmentOperator boundAssignmentOperator, ImmutableArray<VisitResult> argumentResults, ArgumentsCompletionDelegate<Symbol>? argumentsCompletion, Action<int, Symbol>? initializationCompletion, bool flag)
		{
			if (flag)
			{
				return setAnalyzedNullabilityAsContinuation(boundAssignmentOperator, argumentResults, argumentsCompletion, initializationCompletion);
			}
			BoundObjectInitializerMember boundObjectInitializerMember = (BoundObjectInitializerMember)boundAssignmentOperator.Left;
			VisitResult result = new VisitResult(boundObjectInitializerMember.Type, NullableAnnotation.NotAnnotated, NullableFlowState.NotNull);
			SetAnalyzedNullability(boundObjectInitializerMember, result);
			SetAnalyzedNullability(boundAssignmentOperator, result);
			return null;
		}
		Action<int, TypeSymbol>? setAnalyzedNullabilityAsContinuation(BoundAssignmentOperator boundAssignmentOperator, ImmutableArray<VisitResult> argumentResults, ArgumentsCompletionDelegate<Symbol>? argumentsCompletion, Action<int, Symbol>? initializationCompletion)
		{
			return delegate(int arg, TypeSymbol containingType2)
			{
				Symbol symbol = getTargetMember(containingType2, (BoundObjectInitializerMember)boundAssignmentOperator.Left);
				argumentsCompletion?.Invoke(argumentResults, ((PropertySymbol)symbol)?.Parameters ?? default(ImmutableArray<ParameterSymbol>), null);
				initializationCompletion?.Invoke(arg, symbol);
				setAnalyzedNullability(boundAssignmentOperator, argumentResults, null, null, delayCompletionForType: false);
			};
		}
		Action<int, Symbol>? visitMemberAssignment(BoundAssignmentOperator boundAssignmentOperator, int containingSlot2, Symbol symbol, bool flag, Func<TypeWithAnnotations, TypeWithState>? conversionCompletion = null)
		{
			if (!flag && conversionCompletion == null)
			{
				TakeIncrementalSnapshot(boundAssignmentOperator.Right);
			}
			TypeWithAnnotations typeWithAnnotations = ApplyLValueAnnotations(GetTypeOrReturnTypeWithAnnotations(symbol), GetObjectInitializerMemberLValueAnnotations(symbol));
			TypeWithState valueType;
			if (conversionCompletion == null)
			{
				(valueType, conversionCompletion) = VisitOptionalImplicitConversion(boundAssignmentOperator.Right, typeWithAnnotations, useLegacyWarnings: false, trackMembers: true, AssignmentKind.Assignment, flag);
			}
			else
			{
				TypeWithState typeWithState = conversionCompletion(typeWithAnnotations);
				conversionCompletion = null;
				valueType = typeWithState;
			}
			Unsplit();
			if (flag)
			{
				return visitMemberAssignmentAsContinuation(boundAssignmentOperator, conversionCompletion);
			}
			int targetSlot = getOrCreateSlot(containingSlot2, symbol);
			TrackNullableStateForAssignment(boundAssignmentOperator.Right, typeWithAnnotations, targetSlot, valueType, MakeSlot(boundAssignmentOperator.Right));
			return null;
		}
		Action<int, Symbol>? visitMemberAssignmentAsContinuation(BoundAssignmentOperator node2, Func<TypeWithAnnotations, TypeWithState> conversionCompletion)
		{
			return delegate(int containingSlot2, Symbol symbol)
			{
				visitMemberAssignment(node2, containingSlot2, symbol, delayCompletionForType: false, conversionCompletion);
			};
		}
		Action<int, TypeSymbol>? visitMemberInitializer(int containingSlot2, TypeSymbol containingType2, BoundAssignmentOperator boundAssignmentOperator, bool flag)
		{
			BoundObjectInitializerMember boundObjectInitializerMember = (BoundObjectInitializerMember)boundAssignmentOperator.Left;
			Symbol symbol = getTargetMember(containingType2, boundObjectInitializerMember);
			ImmutableArray<VisitResult> argumentResults = default(ImmutableArray<VisitResult>);
			ArgumentsCompletionDelegate<Symbol> argumentsCompletion = null;
			if (!boundObjectInitializerMember.Arguments.IsDefaultOrEmpty)
			{
				(Symbol? member, ImmutableArray<VisitResult> results, bool returnNotNull, ArgumentsCompletionDelegate<Symbol>? completion) tuple = VisitArguments<Symbol>(boundObjectInitializerMember, boundObjectInitializerMember.Arguments, boundObjectInitializerMember.ArgumentRefKindsOpt, ((PropertySymbol)symbol)?.Parameters ?? default(ImmutableArray<ParameterSymbol>), boundObjectInitializerMember.ArgsToParamsOpt, boundObjectInitializerMember.DefaultArguments, boundObjectInitializerMember.Expanded, invokedAsExtensionMethod: false, null, flag);
				argumentResults = tuple.results;
				argumentsCompletion = tuple.completion;
			}
			Action<int, Symbol> initializationCompletion = null;
			if ((object)symbol != null)
			{
				if (boundAssignmentOperator.Right is BoundObjectInitializerExpressionBase initializer)
				{
					initializationCompletion = visitNestedInitializer(containingSlot2, containingType2, symbol, initializer, flag);
				}
				else
				{
					TakeIncrementalSnapshot(boundAssignmentOperator.Right);
					initializationCompletion = visitMemberAssignment(boundAssignmentOperator, containingSlot2, symbol, flag);
				}
			}
			return setAnalyzedNullability(boundAssignmentOperator, argumentResults, argumentsCompletion, initializationCompletion, flag);
		}
		Action<int, Symbol>? visitNestedInitializer(int containingSlot2, TypeSymbol typeSymbol, Symbol symbol, BoundObjectInitializerExpressionBase initializer, bool delayCompletionForType2)
		{
			int num = getOrCreateSlot(containingSlot2, symbol);
			Action<int, TypeSymbol> nestedCompletion = VisitObjectCreationInitializer(num, GetTypeOrReturnType(symbol), initializer, delayCompletionForType2);
			return completeNestedInitializerAnalysis(symbol, initializer, num, nestedCompletion, delayCompletionForType2);
		}
	}

	[Obsolete("Use VisitCollectionElementInitializer(BoundCollectionElementInitializer node, TypeSymbol containingType, bool delayCompletionForType) instead.", true)]
	private new void VisitCollectionElementInitializer(BoundCollectionElementInitializer node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/NullableWalker.cs", 4571);
	}

	private Action<int, TypeSymbol>? VisitCollectionElementInitializer(BoundCollectionElementInitializer node, TypeSymbol containingType, bool delayCompletionForType)
	{
		ImmutableArray<VisitResult> argumentResults = default(ImmutableArray<VisitResult>);
		MethodSymbol methodSymbol = addMethodAsMemberOfContainingType(node, containingType, ref argumentResults);
		MethodSymbol reinferredMethod;
		ArgumentsCompletionDelegate<MethodSymbol> visitArgumentsCompletion;
		(reinferredMethod, argumentResults, _, visitArgumentsCompletion) = VisitArguments(node, node.Arguments, default(ImmutableArray<RefKind>), methodSymbol.Parameters, node.ArgsToParamsOpt, node.DefaultArguments, node.Expanded, node.InvokedAsExtensionMethod, methodSymbol, delayCompletionForType);
		return setUpdatedSymbol(node, containingType, reinferredMethod, argumentResults, visitArgumentsCompletion, delayCompletionForType);
		static MethodSymbol addMethodAsMemberOfContainingType(BoundCollectionElementInitializer boundCollectionElementInitializer, TypeSymbol typeSymbol, ref ImmutableArray<VisitResult> reference)
		{
			MethodSymbol methodSymbol2 = boundCollectionElementInitializer.AddMethod;
			if (boundCollectionElementInitializer.InvokedAsExtensionMethod)
			{
				if (!reference.IsDefault)
				{
					VisitResult visitResult = reference[0];
					ArrayBuilder<VisitResult> instance = ArrayBuilder<VisitResult>.GetInstance(reference.Length);
					instance.Add(new VisitResult(TypeWithState.Create(typeSymbol, visitResult.RValueType.State), visitResult.LValueType.WithType(typeSymbol), visitResult.StateForLambda));
					instance.AddRange(reference, 1, reference.Length - 1);
					reference = instance.ToImmutableAndFree();
				}
			}
			else if (!methodSymbol2.IsExtensionBlockMember())
			{
				methodSymbol2 = (MethodSymbol)AsMemberOfType(typeSymbol, methodSymbol2);
			}
			return methodSymbol2;
		}
		Action<int, TypeSymbol>? setUpdatedSymbol(BoundCollectionElementInitializer boundCollectionElementInitializer, TypeSymbol typeSymbol, MethodSymbol? updatedSymbol, ImmutableArray<VisitResult> argumentResults2, ArgumentsCompletionDelegate<MethodSymbol>? visitArgumentsCompletion2, bool flag)
		{
			if (flag)
			{
				return setUpdatedSymbolAsContinuation(boundCollectionElementInitializer, argumentResults2, visitArgumentsCompletion2);
			}
			if (boundCollectionElementInitializer.ImplicitReceiverOpt != null)
			{
				SetAnalyzedNullability(boundCollectionElementInitializer.ImplicitReceiverOpt, new VisitResult(boundCollectionElementInitializer.ImplicitReceiverOpt.Type, NullableAnnotation.NotAnnotated, NullableFlowState.NotNull));
			}
			SetUnknownResultNullability(boundCollectionElementInitializer);
			SetUpdatedSymbol(boundCollectionElementInitializer, boundCollectionElementInitializer.AddMethod, updatedSymbol);
			return null;
		}
		Action<int, TypeSymbol>? setUpdatedSymbolAsContinuation(BoundCollectionElementInitializer node2, ImmutableArray<VisitResult> argumentResults2, ArgumentsCompletionDelegate<MethodSymbol> argumentsCompletionDelegate)
		{
			return delegate(int containingSlot, TypeSymbol containingType2)
			{
				MethodSymbol methodSymbol2 = addMethodAsMemberOfContainingType(node2, containingType2, ref argumentResults2);
				setUpdatedSymbol(node2, containingType2, argumentsCompletionDelegate(argumentResults2, methodSymbol2.Parameters, methodSymbol2).member, argumentResults2, null, delayCompletionForType: false);
			};
		}
	}

	private void SetNotNullResult(BoundExpression node)
	{
		SetResultType(node, TypeWithState.Create(node.Type, NullableFlowState.NotNull));
	}

	private void SetNotNullResultForLambda(BoundExpression node, LocalState stateForLambda)
	{
		TypeWithState rValueType = TypeWithState.Create(node.Type, NullableFlowState.NotNull);
		TypeWithAnnotations lValueType = rValueType.ToTypeWithAnnotations(compilation);
		SetResult(node, new VisitResult(rValueType, lValueType, stateForLambda), updateAnalyzedNullability: true, null);
	}

	protected override bool IsEmptyStructType(TypeSymbol type)
	{
		if (type.TypeKind != TypeKind.Struct)
		{
			return false;
		}
		if (!_emptyStructTypeCache.IsEmptyStructType(type))
		{
			return false;
		}
		if (type.SpecialType.CanOptimizeBehavior())
		{
			return true;
		}
		ImmutableArray<Symbol> membersUnordered = ((NamedTypeSymbol)type).GetMembersUnordered();
		if (membersUnordered.Any((Symbol m) => m.Kind == SymbolKind.Field))
		{
			return true;
		}
		if (membersUnordered.Any((Symbol m) => m.Kind == SymbolKind.Property))
		{
			return false;
		}
		return true;
	}

	private int GetOrCreatePlaceholderSlot(BoundExpression node)
	{
		if (IsEmptyStructType(node.Type))
		{
			return -1;
		}
		return GetOrCreatePlaceholderSlot(node, TypeWithAnnotations.Create(node.Type, NullableAnnotation.NotAnnotated));
	}

	private int GetOrCreatePlaceholderSlot(object identifier, TypeWithAnnotations type)
	{
		if (_placeholderLocalsOpt == null)
		{
			_placeholderLocalsOpt = PooledDictionary<object, PlaceholderLocal>.GetInstance();
		}
		if (!_placeholderLocalsOpt.TryGetValue(identifier, out PlaceholderLocal value))
		{
			value = new PlaceholderLocal(CurrentSymbol, identifier, type);
			_placeholderLocalsOpt.Add(identifier, value);
		}
		return GetOrCreateSlot(value, 0, forceSlotEvenIfEmpty: true);
	}

	public override BoundNode? VisitAnonymousObjectCreationExpression(BoundAnonymousObjectCreationExpression node)
	{
		NamedTypeSymbol type = (NamedTypeSymbol)node.Type;
		ImmutableArray<BoundExpression> arguments = node.Arguments;
		ImmutableArray<TypeWithState> items = arguments.SelectAsArray((BoundExpression arg, NullableWalker self) => self.VisitRvalueWithState(arg), this);
		ImmutableArray<TypeWithAnnotations> immutableArray = items.SelectAsArray((TypeWithState arg) => arg.ToTypeWithAnnotations(compilation));
		if (immutableArray.All((TypeWithAnnotations argType) => argType.HasType))
		{
			type = AnonymousTypeManager.ConstructAnonymousTypeSymbol(type, immutableArray);
			int orCreatePlaceholderSlot = GetOrCreatePlaceholderSlot(node);
			int currentDeclarationIndex = 0;
			for (int num = 0; num < arguments.Length; num++)
			{
				BoundExpression boundExpression = arguments[num];
				TypeWithState typeWithState = items[num];
				PropertySymbol anonymousTypeProperty = AnonymousTypeManager.GetAnonymousTypeProperty(type, num);
				if (anonymousTypeProperty.Type.SpecialType != SpecialType.System_Void)
				{
					int orCreateSlot = GetOrCreateSlot(anonymousTypeProperty, orCreatePlaceholderSlot);
					TrackNullableStateForAssignment(boundExpression, anonymousTypeProperty.TypeWithAnnotations, orCreateSlot, typeWithState, MakeSlot(boundExpression));
					BoundAnonymousPropertyDeclaration boundAnonymousPropertyDeclaration = getDeclaration(node, anonymousTypeProperty, ref currentDeclarationIndex);
					if (boundAnonymousPropertyDeclaration != null)
					{
						TakeIncrementalSnapshot(boundAnonymousPropertyDeclaration);
						SetAnalyzedNullability(boundAnonymousPropertyDeclaration, new VisitResult(typeWithState, anonymousTypeProperty.TypeWithAnnotations));
					}
				}
			}
		}
		SetResultType(node, TypeWithState.Create(type, NullableFlowState.NotNull));
		return null;
		static BoundAnonymousPropertyDeclaration? getDeclaration(BoundAnonymousObjectCreationExpression boundAnonymousObjectCreationExpression, PropertySymbol currentProperty, ref int reference)
		{
			if (reference >= boundAnonymousObjectCreationExpression.Declarations.Length)
			{
				return null;
			}
			BoundAnonymousPropertyDeclaration boundAnonymousPropertyDeclaration2 = boundAnonymousObjectCreationExpression.Declarations[reference];
			if (boundAnonymousPropertyDeclaration2.Property.MemberIndexOpt == currentProperty.MemberIndexOpt)
			{
				reference++;
				return boundAnonymousPropertyDeclaration2;
			}
			return null;
		}
	}

	public override BoundNode? VisitArrayCreation(BoundArrayCreation node)
	{
		foreach (BoundExpression bound in node.Bounds)
		{
			VisitRvalue(bound);
		}
		BoundArrayInitialization initializerOpt = node.InitializerOpt;
		if (initializerOpt == null)
		{
			SetResultType(node, TypeWithState.Create(node.Type, NullableFlowState.NotNull));
			return null;
		}
		TypeSymbol type = VisitArrayInitialization(node.Type, initializerOpt, node.HasErrors);
		SetResultType(node, TypeWithState.Create(type, NullableFlowState.NotNull));
		return null;
	}

	private TypeSymbol VisitArrayInitialization(TypeSymbol type, BoundArrayInitialization initialization, bool hasErrors)
	{
		TakeIncrementalSnapshot(initialization);
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(initialization.Initializers.Length);
		GetArrayElements(initialization, instance);
		int count = instance.Count;
		TypeWithAnnotations typeWithAnnotations;
		if (!(type is ArrayTypeSymbol arrayTypeSymbol))
		{
			if (!(type is PointerTypeSymbol pointerTypeSymbol))
			{
				if (!(type is NamedTypeSymbol namedType))
				{
					throw ExceptionUtilities.UnexpectedValue(type.TypeKind);
				}
				typeWithAnnotations = getSpanElementType(namedType);
			}
			else
			{
				typeWithAnnotations = pointerTypeSymbol.PointedAtTypeWithAnnotations;
			}
		}
		else
		{
			typeWithAnnotations = arrayTypeSymbol.ElementTypeWithAnnotations;
		}
		TypeWithAnnotations targetTypeOpt = typeWithAnnotations;
		TypeSymbol result = type;
		if (!initialization.IsInferred)
		{
			foreach (BoundExpression item3 in instance)
			{
				VisitOptionalImplicitConversion(item3, targetTypeOpt, useLegacyWarnings: false, trackMembers: false, AssignmentKind.Assignment);
				Unsplit();
			}
		}
		else
		{
			ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance(count);
			ArrayBuilder<Conversion> instance3 = ArrayBuilder<Conversion>.GetInstance(count);
			ArrayBuilder<TypeWithState> instance4 = ArrayBuilder<TypeWithState>.GetInstance(count);
			ArrayBuilder<BoundExpression> instance5 = ArrayBuilder<BoundExpression>.GetInstance(count);
			foreach (BoundExpression item4 in instance)
			{
				var (boundExpression, item) = RemoveConversion(item4, includeExplicitConversions: false);
				instance2.Add(boundExpression);
				instance3.Add(item);
				SnapshotWalkerThroughConversionGroup(item4, boundExpression);
				TypeWithState item2 = VisitRvalueWithState(boundExpression);
				instance4.Add(item2);
				if (!IsTargetTypedExpression(boundExpression))
				{
					instance5.Add(CreatePlaceholderIfNecessary(boundExpression, item2.ToTypeWithAnnotations(compilation)));
				}
			}
			ImmutableArray<BoundExpression> exprs = instance5.ToImmutableAndFree();
			TypeSymbol typeSymbol = null;
			if (!hasErrors)
			{
				CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
				typeSymbol = BestTypeInferrer.InferBestType(exprs, _conversions, ref useSiteInfo, out var _);
			}
			TypeWithAnnotations targetTypeWithNullability = (((object)typeSymbol == null) ? targetTypeOpt.SetUnknownNullabilityForReferenceTypes() : TypeWithAnnotations.Create(typeSymbol));
			for (int i = 0; i < count; i++)
			{
				BoundExpression boundExpression2 = instance2[i];
				BoundConversion conversionIfApplicable = GetConversionIfApplicable(instance[i], boundExpression2);
				instance4[i] = VisitConversion(conversionIfApplicable, boundExpression2, instance3[i], targetTypeWithNullability, instance4[i], checkConversion: true, fromExplicitCast: false, useLegacyWarnings: false, AssignmentKind.Assignment, null, reportTopLevelWarnings: false);
				Unsplit();
			}
			NullableFlowState nullableState = BestTypeInferrer.GetNullableState(instance4);
			targetTypeWithNullability = TypeWithState.Create(targetTypeWithNullability.Type, nullableState).ToTypeWithAnnotations(compilation);
			for (int j = 0; j < count; j++)
			{
				VisitConversion(null, instance2[j], Conversion.Identity, targetTypeWithNullability, instance4[j], checkConversion: true, fromExplicitCast: false, useLegacyWarnings: false, AssignmentKind.Assignment, null, reportTopLevelWarnings: true, reportRemainingWarnings: false);
			}
			instance2.Free();
			instance3.Free();
			instance4.Free();
			TypeSymbol typeSymbol2;
			if (!(type is ArrayTypeSymbol arrayTypeSymbol2))
			{
				if (!(type is PointerTypeSymbol pointerTypeSymbol2))
				{
					if (!(type is NamedTypeSymbol namedType2))
					{
						throw ExceptionUtilities.UnexpectedValue(type.TypeKind);
					}
					typeSymbol2 = setSpanElementType(namedType2, targetTypeWithNullability);
				}
				else
				{
					typeSymbol2 = pointerTypeSymbol2.WithPointedAtType(targetTypeWithNullability);
				}
			}
			else
			{
				typeSymbol2 = arrayTypeSymbol2.WithElementType(targetTypeWithNullability);
			}
			result = typeSymbol2;
		}
		instance.Free();
		return result;
		static TypeWithAnnotations getSpanElementType(NamedTypeSymbol namedTypeSymbol)
		{
			return namedTypeSymbol.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[0];
		}
		static TypeSymbol setSpanElementType(NamedTypeSymbol namedTypeSymbol, TypeWithAnnotations elementType)
		{
			return namedTypeSymbol.OriginalDefinition.Construct(ImmutableArray.Create(elementType));
		}
	}

	internal static bool IsTargetTypedExpression(BoundExpression node)
	{
		if (node is BoundConditionalOperator boundConditionalOperator)
		{
			if (boundConditionalOperator.WasTargetTyped)
			{
				goto IL_0065;
			}
		}
		else if (node is BoundConvertedSwitchExpression boundConvertedSwitchExpression)
		{
			if (boundConvertedSwitchExpression.WasTargetTyped)
			{
				goto IL_0065;
			}
		}
		else if (node is BoundObjectCreationExpressionBase boundObjectCreationExpressionBase)
		{
			if (boundObjectCreationExpressionBase.WasTargetTyped)
			{
				goto IL_0065;
			}
		}
		else if (node is BoundDelegateCreationExpression boundDelegateCreationExpression)
		{
			if (boundDelegateCreationExpression.WasTargetTyped)
			{
				goto IL_0065;
			}
		}
		else if (node is BoundCollectionExpression { WasTargetTyped: not false })
		{
			goto IL_0065;
		}
		return false;
		IL_0065:
		return true;
	}

	internal static TypeWithAnnotations BestTypeForLambdaReturns(ArrayBuilder<(BoundExpression expr, TypeWithAnnotations resultType, bool isChecked)> returns, Binder binder, BoundNode node, Conversions conversions, out bool inferredFromFunctionType)
	{
		NullableWalker nullableWalker = new NullableWalker(binder.Compilation, null, useConstructorExitWarnings: false, null, useDelegateInvokeParameterTypes: false, useDelegateInvokeReturnType: false, null, node, binder, conversions, null, null, null, null, null);
		int count = returns.Count;
		ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance(count);
		ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance(count);
		for (int i = 0; i < count; i++)
		{
			var (expr, typeWithAnnotations, _) = returns[i];
			instance.Add(typeWithAnnotations);
			instance2.Add(CreatePlaceholderIfNecessary(expr, typeWithAnnotations));
		}
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
		ImmutableArray<BoundExpression> exprs = instance2.ToImmutableAndFree();
		TypeSymbol typeSymbol = BestTypeInferrer.InferBestType(exprs, nullableWalker._conversions, ref useSiteInfo, out inferredFromFunctionType);
		TypeWithAnnotations result;
		if ((object)typeSymbol != null)
		{
			TypeWithAnnotations targetTypeWithNullability = TypeWithAnnotations.Create(typeSymbol);
			Conversions conversions2 = nullableWalker._conversions.WithNullability(includeNullability: false);
			for (int j = 0; j < count; j++)
			{
				BoundExpression boundExpression = exprs[j];
				Conversion conversion = conversions2.ClassifyConversionFromExpression(boundExpression, typeSymbol, returns[j].isChecked, ref useSiteInfo);
				instance[j] = nullableWalker.VisitConversion(null, boundExpression, conversion, targetTypeWithNullability, instance[j].ToTypeWithState(), checkConversion: false, fromExplicitCast: false, useLegacyWarnings: false, AssignmentKind.Return, null, reportTopLevelWarnings: false, reportRemainingWarnings: false).ToTypeWithAnnotations(binder.Compilation);
			}
			result = TypeWithAnnotations.Create(typeSymbol, BestTypeInferrer.GetNullableAnnotation(instance));
		}
		else
		{
			result = default(TypeWithAnnotations);
		}
		instance.Free();
		nullableWalker.Free();
		return result;
	}

	private static void GetArrayElements(BoundArrayInitialization node, ArrayBuilder<BoundExpression> builder)
	{
		foreach (BoundExpression initializer in node.Initializers)
		{
			if (initializer.Kind == BoundKind.ArrayInitialization)
			{
				GetArrayElements((BoundArrayInitialization)initializer, builder);
			}
			else
			{
				builder.Add(initializer);
			}
		}
	}

	public override BoundNode? VisitArrayAccess(BoundArrayAccess node)
	{
		Visit(node.Expression);
		CheckPossibleNullReceiver(node.Expression);
		ArrayTypeSymbol arrayTypeSymbol = ResultType.Type as ArrayTypeSymbol;
		foreach (BoundExpression index in node.Indices)
		{
			VisitRvalue(index);
		}
		TypeWithAnnotations type = ((node.Indices.Length != 1 || !TypeSymbol.Equals(node.Indices[0].Type, compilation.GetWellKnownType(WellKnownType.System_Range), TypeCompareKind.ConsiderEverything)) ? (arrayTypeSymbol?.ElementTypeWithAnnotations ?? default(TypeWithAnnotations)) : TypeWithAnnotations.Create(arrayTypeSymbol));
		SetLvalueResultType(node, type);
		return null;
	}

	public override BoundNode? VisitInlineArrayAccess(BoundInlineArrayAccess node)
	{
		TypeSymbol? type = VisitRvalueWithState(node.Expression).Type;
		VisitRvalue(node.Argument);
		TypeWithAnnotations typeWithAnnotations = type.TryGetInlineArrayElementField().TypeWithAnnotations;
		WellKnownMember getItemOrSliceHelper = node.GetItemOrSliceHelper;
		if ((getItemOrSliceHelper == WellKnownMember.System_Span_T__Slice_Int_Int || getItemOrSliceHelper == WellKnownMember.System_ReadOnlySpan_T__Slice_Int_Int) ? true : false)
		{
			typeWithAnnotations = TypeWithAnnotations.Create(((NamedTypeSymbol)node.Type).OriginalDefinition.Construct(ImmutableArray.Create(typeWithAnnotations)));
		}
		SetResult(node, typeWithAnnotations.ToTypeWithState(), typeWithAnnotations);
		return null;
	}

	private TypeWithState InferResultNullability(BinaryOperatorKind operatorKind, MethodSymbol? methodOpt, TypeSymbol resultType, TypeWithState leftType, TypeWithState rightType)
	{
		NullableFlowState defaultState = NullableFlowState.NotNull;
		if (operatorKind.IsUserDefined())
		{
			if ((object)methodOpt != null && methodOpt.ParameterCount == 2)
			{
				if (operatorKind.IsLifted() && !operatorKind.IsComparison())
				{
					return GetLiftedReturnType(methodOpt.ReturnTypeWithAnnotations, leftType.State.Join(rightType.State));
				}
				TypeWithState result = GetReturnTypeWithState(methodOpt);
				if ((leftType.IsNotNull && methodOpt.ReturnNotNullIfParameterNotNull.Contains(methodOpt.Parameters[0].Name)) || (rightType.IsNotNull && methodOpt.ReturnNotNullIfParameterNotNull.Contains(methodOpt.Parameters[1].Name)))
				{
					result = result.WithNotNullState();
				}
				return result;
			}
		}
		else if (!operatorKind.IsDynamic() && !resultType.IsValueType)
		{
			defaultState = (operatorKind.Operator() | operatorKind.OperandTypes()) switch
			{
				BinaryOperatorKind.DelegateCombination => leftType.State.Meet(rightType.State), 
				BinaryOperatorKind.DelegateRemoval => NullableFlowState.MaybeNull, 
				_ => NullableFlowState.NotNull, 
			};
		}
		if (operatorKind.IsLifted() && !operatorKind.IsComparison())
		{
			defaultState = leftType.State.Join(rightType.State);
		}
		return TypeWithState.Create(resultType, defaultState);
	}

	protected override void VisitBinaryOperatorChildren(ArrayBuilder<BoundBinaryOperator> stack)
	{
		BoundBinaryOperator binary = stack.Pop();
		(BoundExpression, Conversion) tuple = RemoveConversion(binary.Left, includeExplicitConversions: false);
		BoundExpression leftOperand = tuple.Item1;
		Conversion leftConversion = tuple.Item2;
		bool flag = VisitPossibleConditionalAccess(leftOperand, out var stateWhenNotNull) && AbstractFlowPass<LocalState, LocalFunctionState>.CanPropagateStateWhenNotNull(leftConversion);
		if (flag)
		{
			BinaryOperatorKind binaryOperatorKind = binary.OperatorKind.Operator();
			bool flag2 = ((binaryOperatorKind == BinaryOperatorKind.Equal || binaryOperatorKind == BinaryOperatorKind.NotEqual) ? true : false);
			flag = flag2;
		}
		if (flag)
		{
			TypeWithState resultType = ResultType;
			(BoundExpression expression, Conversion conversion) tuple2 = RemoveConversion(binary.Right, includeExplicitConversions: false);
			BoundExpression item = tuple2.expression;
			Conversion item2 = tuple2.conversion;
			bool disableDiagnostics = _disableDiagnostics;
			_disableDiagnostics = true;
			LocalState state = State;
			SetState(getUnconditionalStateWhenNotNull(item, stateWhenNotNull));
			VisitRvalue(item);
			LocalState state2 = State;
			_disableDiagnostics = disableDiagnostics;
			SetState(state);
			TypeWithState typeWithState = VisitRvalueWithState(item);
			ReinferBinaryOperatorAndSetResult(leftOperand, leftConversion, resultType, item, item2, typeWithState, binary);
			if (isKnownNullOrNotNull(item, typeWithState))
			{
				bool flag3 = item.ConstantValueOpt?.IsNull ?? false;
				SetConditionalState((flag3 == isEquals(binary)) ? (whenTrue: State, whenFalse: state2) : (whenTrue: state2, whenFalse: State));
			}
			if (stack.Count == 0)
			{
				return;
			}
			leftOperand = binary;
			leftConversion = Conversion.Identity;
			binary = stack.Pop();
		}
		while (true)
		{
			if (!learnFromConditionalAccessOrBoolConstant())
			{
				Unsplit();
				UseRvalueOnly(leftOperand);
				AfterLeftChildHasBeenVisited(leftOperand, leftConversion, binary);
			}
			if (stack.Count != 0)
			{
				leftOperand = binary;
				leftConversion = Conversion.Identity;
				binary = stack.Pop();
				continue;
			}
			break;
		}
		LocalState getUnconditionalStateWhenNotNull(BoundExpression otherOperand, PossiblyConditionalState conditionalStateWhenNotNull)
		{
			LocalState self;
			if (!conditionalStateWhenNotNull.IsConditionalState)
			{
				self = conditionalStateWhenNotNull.State;
			}
			else
			{
				if (isEquals(binary))
				{
					ConstantValue constantValueOpt = otherOperand.ConstantValueOpt;
					if ((object)constantValueOpt != null && constantValueOpt.IsBoolean)
					{
						self = (constantValueOpt.BooleanValue ? conditionalStateWhenNotNull.StateWhenTrue : conditionalStateWhenNotNull.StateWhenFalse);
						goto IL_0062;
					}
				}
				self = conditionalStateWhenNotNull.StateWhenTrue;
				Join(ref self, ref conditionalStateWhenNotNull.StateWhenFalse);
			}
			goto IL_0062;
			IL_0062:
			return self;
		}
		static bool isEquals(BoundBinaryOperator boundBinaryOperator)
		{
			return boundBinaryOperator.OperatorKind.Operator() == BinaryOperatorKind.Equal;
		}
		static bool isKnownNullOrNotNull(BoundExpression expr, TypeWithState typeWithState2)
		{
			if (!typeWithState2.State.IsNotNull())
			{
				return (object)expr.ConstantValueOpt != null;
			}
			return true;
		}
		bool learnFromConditionalAccessOrBoolConstant()
		{
			BinaryOperatorKind binaryOperatorKind2 = binary.OperatorKind.Operator();
			if ((binaryOperatorKind2 != BinaryOperatorKind.Equal && binaryOperatorKind2 != BinaryOperatorKind.NotEqual) || 1 == 0)
			{
				return false;
			}
			TypeWithState resultType2 = ResultType;
			var (boundExpression, conversion) = RemoveConversion(binary.Right, includeExplicitConversions: false);
			if (isKnownNullOrNotNull(leftOperand, resultType2) && AbstractFlowPass<LocalState, LocalFunctionState>.CanPropagateStateWhenNotNull(conversion) && TryVisitConditionalAccess(boundExpression, out var stateWhenNotNull2))
			{
				ReinferBinaryOperatorAndSetResult(leftOperand, leftConversion, resultType2, boundExpression, conversion, ResultType, binary);
				LocalState localState = getUnconditionalStateWhenNotNull(leftOperand, stateWhenNotNull2);
				bool flag4 = leftOperand.ConstantValueOpt?.IsNull ?? false;
				SetConditionalState((flag4 == isEquals(binary)) ? (whenTrue: State, whenFalse: localState) : (whenTrue: localState, whenFalse: State));
				return true;
			}
			if (binary.OperatorKind.IsUserDefined())
			{
				return false;
			}
			if (IsConditionalState)
			{
				ConstantValue constantValueOpt = binary.Right.ConstantValueOpt;
				if ((object)constantValueOpt != null && constantValueOpt.IsBoolean)
				{
					LocalState localState2 = StateWhenTrue.Clone();
					LocalState localState3 = StateWhenFalse.Clone();
					LocalState localState4 = localState2;
					Unsplit();
					Visit(binary.Right);
					UseRvalueOnly(binary.Right);
					SetConditionalState((isEquals(binary) == constantValueOpt.BooleanValue) ? (whenTrue: localState4, whenFalse: localState3) : (whenTrue: localState3, whenFalse: localState4));
					goto IL_0226;
				}
			}
			ConstantValue constantValueOpt2 = binary.Left.ConstantValueOpt;
			if ((object)constantValueOpt2 == null || !constantValueOpt2.IsBoolean)
			{
				return false;
			}
			Unsplit();
			Visit(binary.Right);
			UseRvalueOnly(binary.Right);
			if (IsConditionalState && isEquals(binary) != constantValueOpt2.BooleanValue)
			{
				SetConditionalState(StateWhenFalse, StateWhenTrue);
			}
			goto IL_0226;
			IL_0226:
			SetResult(binary, TypeWithState.ForType(binary.Type), TypeWithAnnotations.Create(binary.Type));
			return true;
		}
	}

	private void ReinferBinaryOperatorAndSetResult(BoundExpression leftOperand, Conversion leftConversion, TypeWithState leftType, BoundExpression rightOperand, Conversion rightConversion, TypeWithState rightType, BoundBinaryOperator binary)
	{
		TypeWithState resultType = ReinferAndVisitBinaryOperator(binary, binary.OperatorKind, binary.BinaryOperatorMethod, binary.Type, binary.Left, leftOperand, leftConversion, leftType, binary.Right, rightOperand, rightConversion, rightType);
		SetResult(binary, resultType, resultType.ToTypeWithAnnotations(compilation));
	}

	private TypeWithState ReinferAndVisitBinaryOperator(BoundExpression binary, BinaryOperatorKind operatorKind, MethodSymbol? method, TypeSymbol returnType, BoundExpression left, BoundExpression leftOperand, Conversion leftConversion, TypeWithState leftType, BoundExpression right, BoundExpression rightOperand, Conversion rightConversion, TypeWithState rightType)
	{
		if (operatorKind.IsUserDefined() && (object)method != null && method.ParameterCount == 2)
		{
			bool isLifted = operatorKind.IsLifted();
			TypeWithState nullableUnderlyingTypeIfNecessary = GetNullableUnderlyingTypeIfNecessary(isLifted, leftType);
			TypeWithState nullableUnderlyingTypeIfNecessary2 = GetNullableUnderlyingTypeIfNecessary(isLifted, rightType);
			MethodSymbol methodSymbol = ReInferBinaryOperator(binary.Syntax, method, leftOperand, rightOperand, nullableUnderlyingTypeIfNecessary, nullableUnderlyingTypeIfNecessary2);
			SetUpdatedSymbol(binary, method, methodSymbol);
			method = methodSymbol;
			ImmutableArray<ParameterSymbol> parameters = method.Parameters;
			VisitBinaryOperatorOperandConversionAndPostConditions(left, leftOperand, leftConversion, parameters[0], nullableUnderlyingTypeIfNecessary, isLifted);
			VisitBinaryOperatorOperandConversionAndPostConditions(right, rightOperand, rightConversion, parameters[1], nullableUnderlyingTypeIfNecessary2, isLifted);
		}
		else
		{
			visitOperandConversion(left, leftOperand, leftConversion, leftType);
			visitOperandConversion(right, rightOperand, rightConversion, rightType);
		}
		bool flag = operatorKind.IsLifted();
		if (flag)
		{
			bool flag2;
			switch (operatorKind.Operator())
			{
			case BinaryOperatorKind.GreaterThan:
			case BinaryOperatorKind.LessThan:
			case BinaryOperatorKind.GreaterThanOrEqual:
			case BinaryOperatorKind.LessThanOrEqual:
				flag2 = true;
				break;
			default:
				flag2 = false;
				break;
			}
			flag = flag2;
		}
		if (flag)
		{
			SplitAndLearnFromNonNullTest(left, whenTrue: true);
			SplitAndLearnFromNonNullTest(right, whenTrue: true);
		}
		return InferResultNullability(operatorKind, method, returnType, leftType, rightType);
		void visitOperandConversion(BoundExpression expr, BoundExpression operand, Conversion conversion, TypeWithState operandType)
		{
			if ((object)expr.Type != null)
			{
				VisitConversion(expr as BoundConversion, operand, conversion, TypeWithAnnotations.Create(expr.Type), operandType, checkConversion: true, fromExplicitCast: false, useLegacyWarnings: false, AssignmentKind.Argument);
			}
		}
	}

	private MethodSymbol ReInferBinaryOperator(SyntaxNode syntax, MethodSymbol method, BoundExpression leftOperand, BoundExpression rightOperand, TypeWithState leftUnderlyingType, TypeWithState rightUnderlyingType)
	{
		TypeSymbol containingType = method.ContainingType;
		if (!method.IsExtensionBlockMember())
		{
			return (MethodSymbol)AsMemberOfType(getTypeIfContainingType(containingType, leftUnderlyingType.Type, leftOperand) ?? getTypeIfContainingType(containingType, rightUnderlyingType.Type, rightOperand) ?? containingType, method);
		}
		if (method.ContainingType.Arity != 0)
		{
			NamedTypeSymbol containingType2 = method.OriginalDefinition.ContainingType;
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
			MethodTypeInferenceResult methodTypeInferenceResult = MethodTypeInferrer.Infer(_binder, _conversions, containingType2.TypeParameters, containingType2, method.OriginalDefinition.ParameterTypesWithAnnotations, method.OriginalDefinition.ParameterRefKinds, ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[2]
			{
				new BoundExpressionWithNullability(leftOperand.Syntax, leftOperand, leftUnderlyingType.ToTypeWithAnnotations(compilation).NullableAnnotation, leftUnderlyingType.Type),
				new BoundExpressionWithNullability(rightOperand.Syntax, rightOperand, rightUnderlyingType.ToTypeWithAnnotations(compilation).NullableAnnotation, rightUnderlyingType.Type)
			}), ref useSiteInfo, new MethodInferenceExtensions(this));
			if (methodTypeInferenceResult.Success)
			{
				containingType2 = containingType2.Construct(methodTypeInferenceResult.InferredTypeArguments);
				method = method.OriginalDefinition.AsMember(containingType2);
			}
			CheckMethodConstraints(syntax, method);
			return method;
		}
		return method;
		TypeSymbol? getTypeIfContainingType(TypeSymbol baseType, TypeSymbol? derivedType, BoundExpression operand)
		{
			if ((object)derivedType == null || IsTargetTypedExpression(operand))
			{
				return null;
			}
			derivedType = derivedType.StrippedType();
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2 = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
			ConversionKind kind = _conversions.ClassifyBuiltInConversion(derivedType, baseType, isChecked: false, ref useSiteInfo2).Kind;
			if ((kind == ConversionKind.Identity || kind == ConversionKind.ImplicitReference) ? true : false)
			{
				return derivedType;
			}
			return null;
		}
	}

	private TypeWithState VisitBinaryOperatorOperandConversion(BoundExpression expr, BoundExpression operand, Conversion conversion, ParameterSymbol parameter, TypeWithState operandType, bool isLifted, out FlowAnalysisAnnotations parameterAnnotations)
	{
		parameterAnnotations = GetParameterAnnotations(parameter);
		TypeWithAnnotations typeWithAnnotations = ApplyLValueAnnotations(parameter.TypeWithAnnotations, parameterAnnotations);
		if (isLifted && typeWithAnnotations.Type.IsNonNullableValueType())
		{
			typeWithAnnotations = TypeWithAnnotations.Create(MakeNullableOf(typeWithAnnotations));
		}
		return VisitConversion(expr as BoundConversion, operand, conversion, typeWithAnnotations, operandType, checkConversion: true, fromExplicitCast: false, useLegacyWarnings: false, AssignmentKind.Argument, parameter);
	}

	private void VisitBinaryOperatorOperandConversionAndPostConditions(BoundExpression expr, BoundExpression operand, Conversion conversion, ParameterSymbol parameter, TypeWithState operandType, bool isLifted)
	{
		TypeWithState state = VisitBinaryOperatorOperandConversion(expr, operand, conversion, parameter, operandType, isLifted, out var parameterAnnotations);
		if (CheckDisallowedNullAssignment(state, parameterAnnotations, expr.Syntax, operand))
		{
			LearnFromNonNullTest(operand, ref State);
		}
		LearnFromPostConditions(operand, parameterAnnotations);
	}

	private void AfterLeftChildHasBeenVisited(BoundExpression leftOperand, Conversion leftConversion, BoundBinaryOperator binary)
	{
		TypeWithState resultType = ResultType;
		var (boundExpression, rightConversion) = RemoveConversion(binary.Right, includeExplicitConversions: false);
		VisitRvalue(boundExpression);
		TypeWithState resultType2 = ResultType;
		ReinferBinaryOperatorAndSetResult(leftOperand, leftConversion, resultType, boundExpression, rightConversion, resultType2, binary);
		BinaryOperatorKind binaryOperatorKind = binary.OperatorKind.Operator();
		if (binaryOperatorKind == BinaryOperatorKind.Equal || binaryOperatorKind == BinaryOperatorKind.NotEqual)
		{
			BoundExpression boundExpression2 = null;
			ConstantValue? constantValueOpt = binary.Right.ConstantValueOpt;
			if ((object)constantValueOpt != null && constantValueOpt.IsNull)
			{
				boundExpression2 = binary.Left;
			}
			else
			{
				ConstantValue? constantValueOpt2 = binary.Left.ConstantValueOpt;
				if ((object)constantValueOpt2 != null && constantValueOpt2.IsNull)
				{
					boundExpression2 = binary.Right;
				}
			}
			if (boundExpression2 != null)
			{
				bool flag = binaryOperatorKind != BinaryOperatorKind.Equal;
				SplitAndLearnFromNonNullTest(boundExpression2, flag);
				LearnFromNullTest(boundExpression2, ref flag ? ref StateWhenFalse : ref StateWhenTrue);
				return;
			}
		}
		BoundExpression boundExpression3 = null;
		if (resultType.IsNotNull && resultType2.MayBeNull)
		{
			boundExpression3 = binary.Right;
		}
		else if (resultType2.IsNotNull && resultType.MayBeNull)
		{
			boundExpression3 = binary.Left;
		}
		if (boundExpression3 != null)
		{
			switch (binaryOperatorKind)
			{
			case BinaryOperatorKind.Equal:
			case BinaryOperatorKind.GreaterThan:
			case BinaryOperatorKind.LessThan:
			case BinaryOperatorKind.GreaterThanOrEqual:
			case BinaryOperatorKind.LessThanOrEqual:
				boundExpression3 = SkipReferenceConversions(boundExpression3);
				SplitAndLearnFromNonNullTest(boundExpression3, whenTrue: true);
				break;
			case BinaryOperatorKind.NotEqual:
				boundExpression3 = SkipReferenceConversions(boundExpression3);
				SplitAndLearnFromNonNullTest(boundExpression3, whenTrue: false);
				break;
			}
		}
	}

	private void SplitAndLearnFromNonNullTest(BoundExpression operandComparedToNonNull, bool whenTrue)
	{
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance();
		GetSlotsToMarkAsNotNullable(operandComparedToNonNull, instance);
		if (instance.Count != 0)
		{
			Split();
			MarkSlotsAsNotNull(instance, ref whenTrue ? ref StateWhenTrue : ref StateWhenFalse);
		}
		instance.Free();
	}

	protected override bool VisitInterpolatedStringHandlerParts(BoundInterpolatedStringBase node, bool usesBoolReturns, bool firstPartIsConditional, ref LocalState shortCircuitState)
	{
		bool result = base.VisitInterpolatedStringHandlerParts(node, usesBoolReturns, firstPartIsConditional, ref shortCircuitState);
		SetNotNullResult(node);
		return result;
	}

	protected override void VisitInterpolatedStringBinaryOperatorNode(BoundBinaryOperator node)
	{
		SetNotNullResult(node);
	}

	private void GetSlotsToMarkAsNotNullable(BoundExpression operand, ArrayBuilder<int> slotBuilder)
	{
		int lastConditionalAccessSlot = _lastConditionalAccessSlot;
		try
		{
			while (true)
			{
				switch (operand.Kind)
				{
				case BoundKind.Conversion:
					operand = ((BoundConversion)operand).Operand;
					break;
				case BoundKind.AsOperator:
					operand = ((BoundAsOperator)operand).Operand;
					break;
				case BoundKind.ConditionalAccess:
				{
					BoundConditionalAccess boundConditionalAccess = (BoundConditionalAccess)operand;
					GetSlotsToMarkAsNotNullable(boundConditionalAccess.Receiver, slotBuilder);
					int num = MakeSlot(boundConditionalAccess.Receiver);
					if (num > 0)
					{
						TypeSymbol type = boundConditionalAccess.Receiver.Type;
						if (type.IsNullableType())
						{
							num = GetNullableOfTValueSlot(type, num, out Symbol _);
						}
					}
					if (num > 0)
					{
						_lastConditionalAccessSlot = num;
						operand = boundConditionalAccess.AccessExpression;
						break;
					}
					return;
				}
				default:
				{
					int num = MakeSlot(operand);
					if (num > 0 && PossiblyNullableType(operand.Type))
					{
						slotBuilder.Add(num);
					}
					return;
				}
				}
			}
		}
		finally
		{
			_lastConditionalAccessSlot = lastConditionalAccessSlot;
		}
	}

	private static bool PossiblyNullableType([NotNullWhen(true)] TypeSymbol? operandType)
	{
		return operandType?.CanContainNull() ?? false;
	}

	private void MarkSlotsAsNotNull(ArrayBuilder<int> slots, ref LocalState stateToUpdate)
	{
		foreach (int slot in slots)
		{
			SetState(ref stateToUpdate, slot, NullableFlowState.NotNull);
		}
	}

	private void LearnFromNonNullTest(BoundExpression expression, ref LocalState state)
	{
		if (expression is BoundValuePlaceholderBase key)
		{
			if (_resultForPlaceholdersOpt == null || !_resultForPlaceholdersOpt.TryGetValue(key, out (BoundExpression, VisitResult) value) || value.Item1 == null)
			{
				return;
			}
			(expression, _) = value;
		}
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance();
		GetSlotsToMarkAsNotNullable(expression, instance);
		MarkSlotsAsNotNull(instance, ref state);
		instance.Free();
	}

	private void LearnFromNonNullTest(int slot, ref LocalState state)
	{
		SetState(ref state, slot, NullableFlowState.NotNull);
	}

	private void LearnFromNullTest(BoundExpression expression, ref LocalState state)
	{
		if (!(expression.ConstantValueOpt != null))
		{
			BoundExpression item = RemoveConversion(expression, includeExplicitConversions: true).expression;
			int slot = MakeSlot(item);
			LearnFromNullTest(slot, item.Type, ref state, markDependentSlotsNotNull: false);
		}
	}

	private void LearnFromNullTest(int slot, TypeSymbol? expressionType, ref LocalState state, bool markDependentSlotsNotNull)
	{
		if (slot > 0 && PossiblyNullableType(expressionType))
		{
			if (GetState(ref state, slot) == NullableFlowState.NotNull)
			{
				SetState(ref state, slot, NullableFlowState.MaybeNull);
			}
			if (markDependentSlotsNotNull)
			{
				MarkDependentSlotsNotNull(slot, expressionType, ref state);
			}
		}
	}

	private void MarkDependentSlotsNotNull(int slot, TypeSymbol expressionType, ref LocalState state, int depth = 2)
	{
		if (depth <= 0)
		{
			return;
		}
		foreach (Symbol member in getMembers(expressionType))
		{
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
			NamedTypeSymbol namedTypeSymbol = _symbol?.ContainingType;
			if ((member is PropertySymbol { IsIndexedProperty: false } || member.Kind == SymbolKind.Field) && member.RequiresInstanceReceiver() && ((object)namedTypeSymbol == null || AccessCheck.IsSymbolAccessible(member, namedTypeSymbol, ref useSiteInfo)))
			{
				int orCreateSlot = GetOrCreateSlot(member, slot, forceSlotEvenIfEmpty: true, createIfMissing: false);
				if (orCreateSlot > 0)
				{
					SetState(ref state, orCreateSlot, NullableFlowState.NotNull);
					MarkDependentSlotsNotNull(orCreateSlot, GetTypeOrReturnType(member), ref state, depth - 1);
				}
			}
		}
		static NamedTypeSymbol effectiveBase(TypeSymbol type)
		{
			if (!(type is TypeParameterSymbol { EffectiveBaseClassNoUseSiteDiagnostics: var effectiveBaseClassNoUseSiteDiagnostics }))
			{
				return type.BaseTypeNoUseSiteDiagnostics;
			}
			return effectiveBaseClassNoUseSiteDiagnostics;
		}
		static IEnumerable<Symbol> getMembers(TypeSymbol type)
		{
			foreach (Symbol member2 in type.GetMembers())
			{
				yield return member2;
			}
			NamedTypeSymbol baseType = effectiveBase(type);
			while ((object)baseType != null)
			{
				foreach (Symbol member3 in baseType.GetMembers())
				{
					yield return member3;
				}
				baseType = baseType.BaseTypeNoUseSiteDiagnostics;
			}
			foreach (NamedTypeSymbol item in inheritedInterfaces(type))
			{
				foreach (Symbol member4 in item.GetMembers())
				{
					yield return member4;
				}
			}
		}
		static ImmutableArray<NamedTypeSymbol> inheritedInterfaces(TypeSymbol type)
		{
			if (!(type is TypeParameterSymbol { AllEffectiveInterfacesNoUseSiteDiagnostics: var allEffectiveInterfacesNoUseSiteDiagnostics }))
			{
				if ((object)type != null && type.TypeKind == TypeKind.Interface)
				{
					return type.AllInterfacesNoUseSiteDiagnostics;
				}
				return ImmutableArray<NamedTypeSymbol>.Empty;
			}
			return allEffectiveInterfacesNoUseSiteDiagnostics;
		}
	}

	private static BoundExpression SkipReferenceConversions(BoundExpression possiblyConversion)
	{
		while (possiblyConversion.Kind == BoundKind.Conversion)
		{
			BoundConversion boundConversion = (BoundConversion)possiblyConversion;
			ConversionKind conversionKind = boundConversion.ConversionKind;
			if (conversionKind == ConversionKind.ImplicitReference || conversionKind == ConversionKind.ExplicitReference)
			{
				possiblyConversion = boundConversion.Operand;
				continue;
			}
			return possiblyConversion;
		}
		return possiblyConversion;
	}

	public override BoundNode? VisitNullCoalescingAssignmentOperator(BoundNullCoalescingAssignmentOperator node)
	{
		BoundExpression leftOperand = node.LeftOperand;
		BoundExpression rightOperand = node.RightOperand;
		int num = MakeSlot(leftOperand);
		TypeWithAnnotations typeWithAnnotations = VisitLvalueWithAnnotations(leftOperand);
		LocalState state = State.Clone();
		LearnFromNonNullTest(leftOperand, ref state);
		LearnFromNullTest(leftOperand, ref State);
		if (node.IsNullableValueTypeAssignment)
		{
			if (num > 0)
			{
				SetState(ref State, num, NullableFlowState.NotNull);
				num = GetNullableOfTValueSlot(typeWithAnnotations.Type, num, out Symbol _);
			}
			typeWithAnnotations = TypeWithAnnotations.Create(node.Type, NullableAnnotation.NotAnnotated);
		}
		TypeWithState valueType = VisitOptionalImplicitConversion(rightOperand, typeWithAnnotations, UseLegacyWarnings(leftOperand), trackMembers: false, AssignmentKind.Assignment);
		TrackNullableStateForAssignment(rightOperand, typeWithAnnotations, num, valueType, MakeSlot(rightOperand));
		Join(ref State, ref state);
		TypeWithState type = TypeWithState.Create(typeWithAnnotations.Type, valueType.State);
		SetResultType(node, type);
		return null;
	}

	public override BoundNode? VisitNullCoalescingOperator(BoundNullCoalescingOperator node)
	{
		BoundExpression leftOperand = node.LeftOperand;
		BoundExpression rightOperand = node.RightOperand;
		if (AbstractFlowPass<LocalState, LocalFunctionState>.IsConstantNull(leftOperand))
		{
			VisitRvalue(leftOperand);
			Visit(rightOperand);
			TypeWithState resultType = ResultType;
			SetResultType(node, TypeWithState.Create(node.Type, resultType.State));
			return null;
		}
		VisitPossibleConditionalAccess(leftOperand, out var stateWhenNotNull);
		TypeWithState resultType2 = ResultType;
		Unsplit();
		LearnFromNullTest(leftOperand, ref State);
		if (leftOperand.ConstantValueOpt != null)
		{
			SetUnreachable();
		}
		Visit(rightOperand);
		TypeWithState resultType3 = ResultType;
		Join(ref stateWhenNotNull);
		TypeSymbol type = resultType2.Type;
		TypeSymbol type2 = resultType3.Type;
		var (type3, b) = node.OperatorResultKind switch
		{
			BoundNullCoalescingOperatorResultKind.NoCommonType => (node.Type, NullableFlowState.NotNull), 
			BoundNullCoalescingOperatorResultKind.LeftType => getLeftResultType(type, type2), 
			BoundNullCoalescingOperatorResultKind.LeftUnwrappedType => getLeftResultType(type.StrippedType(), type2), 
			BoundNullCoalescingOperatorResultKind.RightType => getResultStateWithRightType(type, type2), 
			BoundNullCoalescingOperatorResultKind.LeftUnwrappedRightType => getResultStateWithRightType(type.StrippedType(), type2), 
			BoundNullCoalescingOperatorResultKind.RightDynamicType => (type2, NullableFlowState.NotNull), 
			_ => throw ExceptionUtilities.UnexpectedValue(node.OperatorResultKind), 
		};
		SetResultType(node, TypeWithState.Create(type3, resultType3.State.Join(b)));
		return null;
		(TypeSymbol ResultType, NullableFlowState LeftState) getLeftResultType(TypeSymbol leftType, TypeSymbol rightType)
		{
			BoundConversion obj = node.RightOperand as BoundConversion;
			if ((obj == null || obj.ExplicitCastInCode) && GenerateConversionForConditionalOperator(node.LeftOperand, leftType, rightType, reportMismatch: false, node.Checked).Exists)
			{
				return (ResultType: rightType, LeftState: NullableFlowState.NotNull);
			}
			Conversion conversion = GenerateConversionForConditionalOperator(node.RightOperand, rightType, leftType, reportMismatch: true, node.Checked);
			return (ResultType: leftType, LeftState: NullableFlowState.NotNull);
		}
		(TypeSymbol ResultType, NullableFlowState LeftState) getResultStateWithRightType(TypeSymbol leftType, TypeSymbol rightType)
		{
			Conversion conversion = GenerateConversionForConditionalOperator(node.LeftOperand, leftType, rightType, reportMismatch: true, node.Checked);
			if (conversion.IsUserDefined)
			{
				TypeWithState typeWithState = VisitConversion(null, node.LeftOperand, conversion, TypeWithAnnotations.Create(rightType), TypeWithState.Create(leftType, NullableFlowState.NotNull), checkConversion: false, fromExplicitCast: false, useLegacyWarnings: false, AssignmentKind.Assignment, null, reportTopLevelWarnings: false, reportRemainingWarnings: false);
				return (ResultType: typeWithState.Type, LeftState: typeWithState.State);
			}
			return (ResultType: rightType, LeftState: NullableFlowState.NotNull);
		}
	}

	private bool TryVisitConditionalAccess(BoundExpression node, out PossiblyConditionalState stateWhenNotNull)
	{
		var (boundExpression, conversion) = RemoveConversion(node, includeExplicitConversions: true);
		if (!(boundExpression is BoundConditionalAccess boundConditionalAccess) || !AbstractFlowPass<LocalState, LocalFunctionState>.CanPropagateStateWhenNotNull(conversion))
		{
			stateWhenNotNull = default(PossiblyConditionalState);
			return false;
		}
		Unsplit();
		VisitConditionalAccess(boundConditionalAccess, out stateWhenNotNull);
		if (node is BoundConversion boundConversion)
		{
			TypeWithState resultType = ResultType;
			TypeWithAnnotations typeWithAnnotations = boundConversion.ConversionGroupOpt?.ExplicitType ?? default(TypeWithAnnotations);
			bool hasType = typeWithAnnotations.HasType;
			TypeWithAnnotations targetTypeWithNullability = (hasType ? typeWithAnnotations : TypeWithAnnotations.Create(boundConversion.Type));
			TypeWithState type = VisitConversion(boundConversion, boundConditionalAccess, conversion, targetTypeWithNullability, resultType, checkConversion: true, hasType, useLegacyWarnings: true, AssignmentKind.Assignment);
			SetResultType(boundConversion, type);
		}
		return true;
	}

	private bool VisitPossibleConditionalAccess(BoundExpression node, out PossiblyConditionalState stateWhenNotNull)
	{
		if (TryVisitConditionalAccess(node, out stateWhenNotNull))
		{
			return true;
		}
		Visit(node);
		stateWhenNotNull = PossiblyConditionalState.Create(this);
		node = RemoveConversion(node, includeExplicitConversions: true).expression;
		int num = MakeSlot(node);
		if (num > -1)
		{
			if (IsConditionalState)
			{
				LearnFromNonNullTest(num, ref stateWhenNotNull.StateWhenTrue);
				LearnFromNonNullTest(num, ref stateWhenNotNull.StateWhenFalse);
			}
			else
			{
				LearnFromNonNullTest(num, ref stateWhenNotNull.State);
			}
		}
		return false;
	}

	private void VisitConditionalAccess(BoundConditionalAccess node, out PossiblyConditionalState stateWhenNotNull)
	{
		BoundExpression receiver = node.Receiver;
		VisitPossibleConditionalAccess(receiver, out stateWhenNotNull);
		Unsplit();
		_currentConditionalReceiverVisitResult = _visitResult;
		int lastConditionalAccessSlot = _lastConditionalAccessSlot;
		ConstantValue constantValueOpt = receiver.ConstantValueOpt;
		if ((object)constantValueOpt != null && !constantValueOpt.IsNull)
		{
			VisitPossibleConditionalAccess(node.AccessExpression, out stateWhenNotNull);
			Unsplit();
		}
		else
		{
			LocalState state = State.Clone();
			if (AbstractFlowPass<LocalState, LocalFunctionState>.IsConstantNull(receiver))
			{
				SetUnreachable();
				_lastConditionalAccessSlot = -1;
			}
			else
			{
				LearnFromNullTest(receiver, ref state);
				makeAndAdjustReceiverSlot(receiver);
				SetPossiblyConditionalState(in stateWhenNotNull);
			}
			BoundExpression accessExpression;
			for (accessExpression = node.AccessExpression; accessExpression is BoundConditionalAccess boundConditionalAccess; accessExpression = boundConditionalAccess.AccessExpression)
			{
				VisitRvalue(boundConditionalAccess.Receiver);
				_currentConditionalReceiverVisitResult = _visitResult;
				makeAndAdjustReceiverSlot(boundConditionalAccess.Receiver);
				Join(ref state, ref State);
			}
			Visit(accessExpression);
			for (accessExpression = node.AccessExpression; accessExpression is BoundConditionalAccess boundConditionalAccess2; accessExpression = boundConditionalAccess2.AccessExpression)
			{
				SetAnalyzedNullability(boundConditionalAccess2, _visitResult);
			}
			int num = MakeSlot(accessExpression);
			if (num > -1)
			{
				if (IsConditionalState)
				{
					LearnFromNonNullTest(num, ref StateWhenTrue);
					LearnFromNonNullTest(num, ref StateWhenFalse);
				}
				else
				{
					LearnFromNonNullTest(num, ref State);
				}
			}
			stateWhenNotNull = PossiblyConditionalState.Create(this);
			Unsplit();
			Join(ref State, ref state);
		}
		TypeWithAnnotations lvalueResultType = LvalueResultType;
		TypeSymbol type = lvalueResultType.Type;
		TypeSymbol type2 = node.Type;
		TypeSymbol type3 = ((type2.IsVoidType() || type2.IsErrorType()) ? type2 : ((type2.IsNullableType() && !type.IsNullableType()) ? MakeNullableOf(lvalueResultType) : type));
		SetResultType(node, TypeWithState.Create(type3, NullableFlowState.MaybeDefault));
		_currentConditionalReceiverVisitResult = default(VisitResult);
		_lastConditionalAccessSlot = lastConditionalAccessSlot;
		void makeAndAdjustReceiverSlot(BoundExpression boundExpression)
		{
			int num2 = MakeSlot(boundExpression);
			if (num2 > -1)
			{
				LearnFromNonNullTest(num2, ref State);
			}
			if (num2 > 0)
			{
				TypeSymbol? type4 = boundExpression.Type;
				if ((object)type4 != null && type4.IsNullableType())
				{
					num2 = GetNullableOfTValueSlot(boundExpression.Type, num2, out Symbol _);
				}
			}
			_lastConditionalAccessSlot = num2;
		}
	}

	public override BoundNode? VisitConditionalAccess(BoundConditionalAccess node)
	{
		VisitConditionalAccess(node, out var _);
		return null;
	}

	protected override BoundNode? VisitConditionalOperatorCore(BoundExpression node, bool isRef, BoundExpression condition, BoundExpression originalConsequence, BoundExpression originalAlternative)
	{
		VisitCondition(condition);
		LocalState stateWhenTrue = StateWhenTrue;
		LocalState stateWhenFalse = StateWhenFalse;
		bool hadMultipleCandidates;
		TypeWithState item2;
		TypeWithState typeWithState;
		if (isRef)
		{
			(TypeWithAnnotations LValueType, TypeWithState RValueType) tuple = visitConditionalRefOperand(stateWhenTrue, originalConsequence);
			TypeWithAnnotations item = tuple.LValueType;
			item2 = tuple.RValueType;
			stateWhenTrue = State;
			TypeWithAnnotations typeWithAnnotations;
			(typeWithAnnotations, typeWithState) = visitConditionalRefOperand(stateWhenFalse, originalAlternative);
			Join(ref State, ref stateWhenTrue);
			NullableAnnotation nullableAnnotation = item.NullableAnnotation.EnsureCompatible(typeWithAnnotations.NullableAnnotation);
			NullableFlowState defaultState = item2.State.Join(typeWithState.State);
			TypeSymbol typeSymbol = node.Type?.SetUnknownNullabilityForReferenceTypes();
			if (IsNullabilityMismatch(item, typeWithAnnotations))
			{
				BoundExpression expr = CreatePlaceholderIfNecessary(originalConsequence, item);
				BoundExpression expr2 = CreatePlaceholderIfNecessary(originalAlternative, typeWithAnnotations);
				CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
				typeSymbol = BestTypeInferrer.InferBestTypeForConditionalOperator(expr, expr2, _conversions, out hadMultipleCandidates, ref useSiteInfo);
				TypeWithAnnotations destination = TypeWithAnnotations.Create(typeSymbol, nullableAnnotation);
				reportMismatchIfNecessary(originalConsequence, item, destination);
				reportMismatchIfNecessary(originalAlternative, typeWithAnnotations, destination);
			}
			else if (!node.HasErrors)
			{
				typeSymbol = item2.Type.MergeEquivalentTypes(typeWithState.Type, VarianceKind.None);
			}
			SetResult(node, TypeWithState.Create(typeSymbol, defaultState), TypeWithAnnotations.Create(typeSymbol, nullableAnnotation));
			return null;
		}
		(BoundExpression, Conversion, TypeWithState) tuple3 = visitConditionalOperand(stateWhenTrue, originalConsequence);
		BoundExpression item3 = tuple3.Item1;
		Conversion item4 = tuple3.Item2;
		item2 = tuple3.Item3;
		PossiblyConditionalState conditionalState = PossiblyConditionalState.Create(this);
		stateWhenTrue = CloneAndUnsplit(ref conditionalState);
		bool reachable = stateWhenTrue.Reachable;
		(BoundExpression, Conversion, TypeWithState) tuple4 = visitConditionalOperand(stateWhenFalse, originalAlternative);
		BoundExpression item5 = tuple4.Item1;
		Conversion item6 = tuple4.Item2;
		typeWithState = tuple4.Item3;
		PossiblyConditionalState conditionalState2 = PossiblyConditionalState.Create(this);
		stateWhenFalse = CloneAndUnsplit(ref conditionalState2);
		bool reachable2 = stateWhenFalse.Reachable;
		SetPossiblyConditionalState(in conditionalState);
		Join(ref conditionalState2);
		bool flag = node is BoundConditionalOperator boundConditionalOperator && boundConditionalOperator.WasTargetTyped;
		TypeSymbol typeSymbol2;
		if (flag)
		{
			typeSymbol2 = null;
		}
		else if (IsTargetTypedExpression(item3))
		{
			typeSymbol2 = typeWithState.Type;
		}
		else if (IsTargetTypedExpression(item5))
		{
			typeSymbol2 = item2.Type;
		}
		else
		{
			BoundExpression expr3 = CreatePlaceholderIfNecessary(item3, item2.ToTypeWithAnnotations(compilation));
			BoundExpression expr4 = CreatePlaceholderIfNecessary(item5, typeWithState.ToTypeWithAnnotations(compilation));
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2 = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
			typeSymbol2 = BestTypeInferrer.InferBestTypeForConditionalOperator(expr3, expr4, _conversions, out hadMultipleCandidates, ref useSiteInfo2);
		}
		if ((object)typeSymbol2 == null)
		{
			typeSymbol2 = node.Type?.SetUnknownNullabilityForReferenceTypes();
		}
		UnsplitIfNeeded(typeSymbol2);
		TypeWithAnnotations resultTypeWithAnnotations;
		if ((object)typeSymbol2 == null)
		{
			if (!flag)
			{
				SetResultType(node, TypeWithState.Create(typeSymbol2, NullableFlowState.NotNull));
				return null;
			}
			resultTypeWithAnnotations = default(TypeWithAnnotations);
		}
		else
		{
			resultTypeWithAnnotations = TypeWithAnnotations.Create(typeSymbol2);
		}
		TypeWithState type = convertArms(node, originalConsequence, originalAlternative, stateWhenTrue, stateWhenFalse, item2, typeWithState, item3, item4, reachable, item5, item6, reachable2, resultTypeWithAnnotations, flag);
		SetResultType(node, type, updateAnalyzedNullability: false);
		return null;
		void addConvertArmsAsCompletion(BoundExpression boundExpression, BoundExpression originalConsequence2, BoundExpression originalAlternative2, LocalState consequenceState, LocalState alternativeState, TypeWithState consequenceRValue, TypeWithState alternativeRValue, BoundExpression consequence, Conversion consequenceConversion, bool consequenceEndReachable, BoundExpression alternative, Conversion alternativeConversion, bool alternativeEndReachable)
		{
			TargetTypedAnalysisCompletion[boundExpression] = (TypeWithAnnotations resultTypeWithAnnotations2) => convertArms(boundExpression, originalConsequence2, originalAlternative2, consequenceState, alternativeState, consequenceRValue, alternativeRValue, consequence, consequenceConversion, consequenceEndReachable, alternative, alternativeConversion, alternativeEndReachable, resultTypeWithAnnotations2, wasTargetTyped: false);
		}
		TypeWithState convertArms(BoundExpression boundExpression3, BoundExpression boundExpression, BoundExpression boundExpression2, LocalState consequenceState, LocalState alternativeState, TypeWithState consequenceRValue, TypeWithState alternativeRValue, BoundExpression consequence, Conversion consequenceConversion, bool consequenceEndReachable, BoundExpression alternative, Conversion alternativeConversion, bool alternativeEndReachable, TypeWithAnnotations targetType, bool wasTargetTyped)
		{
			NullableFlowState defaultState2;
			if (!wasTargetTyped)
			{
				TypeWithState typeWithState2 = ConvertConditionalOperandOrSwitchExpressionArmResult(boundExpression, consequence, consequenceConversion, targetType, consequenceRValue, consequenceState, consequenceEndReachable);
				TypeWithState typeWithState3 = ConvertConditionalOperandOrSwitchExpressionArmResult(boundExpression2, alternative, alternativeConversion, targetType, alternativeRValue, alternativeState, alternativeEndReachable);
				defaultState2 = typeWithState2.State.Join(typeWithState3.State);
				TypeWithState typeWithState4 = TypeWithState.Create(targetType.Type, defaultState2);
				SetAnalyzedNullability(boundExpression3, typeWithState4);
				return typeWithState4;
			}
			addConvertArmsAsCompletion(boundExpression3, boundExpression, boundExpression2, consequenceState, alternativeState, consequenceRValue, alternativeRValue, consequence, consequenceConversion, consequenceEndReachable, alternative, alternativeConversion, alternativeEndReachable);
			defaultState2 = consequenceRValue.State.Join(alternativeRValue.State);
			return TypeWithState.Create(targetType.Type, defaultState2);
		}
		void reportMismatchIfNecessary(BoundExpression boundExpression, TypeWithAnnotations source, TypeWithAnnotations typeWithAnnotations2)
		{
			if (!boundExpression.IsSuppressed && IsNullabilityMismatch(source, typeWithAnnotations2))
			{
				ReportNullabilityMismatchInAssignment(boundExpression.Syntax, source, typeWithAnnotations2);
			}
		}
		(BoundExpression, Conversion, TypeWithState) visitConditionalOperand(LocalState state, BoundExpression operand)
		{
			SetState(state);
			var (boundExpression, item7) = RemoveConversion(operand, includeExplicitConversions: false);
			SnapshotWalkerThroughConversionGroup(operand, boundExpression);
			Visit(boundExpression);
			return (boundExpression, item7, ResultType);
		}
		(TypeWithAnnotations LValueType, TypeWithState RValueType) visitConditionalRefOperand(LocalState state, BoundExpression operand)
		{
			SetState(state);
			return (LValueType: VisitLvalueWithAnnotations(operand), RValueType: ResultType);
		}
	}

	private TypeWithState ConvertConditionalOperandOrSwitchExpressionArmResult(BoundExpression node, BoundExpression operand, Conversion conversion, TypeWithAnnotations targetType, TypeWithState operandType, LocalState state, bool isReachable)
	{
		PossiblyConditionalState conditionalState = PossiblyConditionalState.Create(this);
		SetState(state);
		bool disableDiagnostics = _disableDiagnostics;
		if (!isReachable)
		{
			_disableDiagnostics = true;
		}
		TypeWithState result = VisitConversion(GetConversionIfApplicable(node, operand), operand, conversion, targetType, operandType, checkConversion: true, fromExplicitCast: false, useLegacyWarnings: false, AssignmentKind.Assignment, null, reportTopLevelWarnings: false);
		if (!isReachable)
		{
			result = default(TypeWithState);
			_disableDiagnostics = disableDiagnostics;
		}
		SetPossiblyConditionalState(in conditionalState);
		return result;
	}

	private bool IsReachable()
	{
		if (!IsConditionalState)
		{
			return State.Reachable;
		}
		if (!StateWhenTrue.Reachable)
		{
			return StateWhenFalse.Reachable;
		}
		return true;
	}

	private static BoundExpression CreatePlaceholderIfNecessary(BoundExpression expr, TypeWithAnnotations type)
	{
		if (type.HasType)
		{
			return new BoundExpressionWithNullability(expr.Syntax, expr, type.NullableAnnotation, type.Type);
		}
		return expr;
	}

	public override BoundNode? VisitConditionalReceiver(BoundConditionalReceiver node)
	{
		TypeSymbol typeSymbol = _currentConditionalReceiverVisitResult.RValueType.Type;
		if ((object)typeSymbol != null && typeSymbol.IsNullableType())
		{
			typeSymbol = typeSymbol.GetNullableUnderlyingType();
		}
		SetResultType(node, TypeWithState.Create(typeSymbol, NullableFlowState.NotNull));
		return null;
	}

	public override BoundNode? VisitCall(BoundCall node)
	{
		if (tryGetReceiver(node, out var receiver))
		{
			ArrayBuilder<BoundCall> instance = ArrayBuilder<BoundCall>.GetInstance();
			instance.Push(node);
			node = receiver;
			bool expressionIsRead = _expressionIsRead;
			_expressionIsRead = true;
			BoundCall receiver2;
			while (tryGetReceiver(node, out receiver2))
			{
				TakeIncrementalSnapshot(node);
				instance.Push(node);
				node = receiver2;
			}
			TakeIncrementalSnapshot(node);
			TypeWithState receiverType = visitAndCheckReceiver(node);
			VisitResult? firstArgumentResult = null;
			while (true)
			{
				reinferMethodAndVisitArguments(node, receiverType, firstArgumentResult);
				receiver = node;
				if (!instance.TryPop(out node))
				{
					break;
				}
				VisitExpressionWithoutStackGuardEpilogue(receiver);
				if (node.IsErroneousNode)
				{
					VisitRvalueEpilogue(receiver);
					if (node.ReceiverOpt != null)
					{
						receiverType = ResultType;
						firstArgumentResult = null;
					}
					else
					{
						firstArgumentResult = _visitResult;
						receiverType = default(TypeWithState);
					}
					continue;
				}
				bool flag = node.Method.IsExtensionBlockMember();
				if (node.ReceiverOpt != null && !flag)
				{
					VisitRvalueEpilogue(receiver);
					receiverType = ResultType;
					CheckCallReceiver(receiver, receiverType, node.Method);
					firstArgumentResult = null;
					continue;
				}
				RefKind refKind = (flag ? GetExtensionReceiverRefKind(node.Method) : AbstractFlowPass<LocalState, LocalFunctionState>.GetRefKind(node.ArgumentRefKindsOpt, 0));
				FlowAnalysisAnnotations annotations;
				if (flag)
				{
					annotations = node.Method.ContainingType.ExtensionParameter.FlowAnalysisAnnotations;
				}
				else
				{
					TypeWithAnnotations paramsIterationType = default(TypeWithAnnotations);
					annotations = GetCorrespondingParameter(0, node.Method.Parameters, node.ArgsToParamsOpt, node.Expanded, ref paramsIterationType).Annotations;
				}
				firstArgumentResult = VisitArgumentEvaluateEpilogue(receiver, refKind, annotations);
				receiverType = default(TypeWithState);
			}
			_expressionIsRead = expressionIsRead;
			instance.Free();
		}
		else
		{
			TypeWithState receiverType2 = visitAndCheckReceiver(node);
			reinferMethodAndVisitArguments(node, receiverType2);
		}
		return null;
		void reinferMethodAndVisitArguments(BoundCall boundCall, TypeWithState receiverType3, VisitResult? firstArgumentResult2 = null)
		{
			if (boundCall.IsErroneousNode)
			{
				for (int i = 0; i < boundCall.Arguments.Length; i++)
				{
					if (i != 0 || !firstArgumentResult2.HasValue)
					{
						BoundExpression child = boundCall.Arguments[i];
						VisitBadExpressionChild(child);
					}
				}
				TypeWithAnnotations type = TypeWithAnnotations.Create(boundCall.Type);
				SetLvalueResultType(boundCall, type);
				SetUpdatedSymbol(boundCall, boundCall.Method, boundCall.Method);
			}
			else
			{
				var (methodSymbol, results, flag2) = ReInferMethodAndVisitArguments(boundCall, boundCall.ReceiverOpt, receiverType3, boundCall.Method, boundCall.Arguments, boundCall.ArgumentRefKindsOpt, boundCall.ArgsToParamsOpt, boundCall.DefaultArguments, boundCall.Expanded, boundCall.InvokedAsExtensionMethod, firstArgumentResult2);
				LearnFromEqualsMethod(methodSymbol, boundCall, receiverType3, results);
				TypeWithState resultType = GetReturnTypeWithState(methodSymbol);
				if (flag2)
				{
					resultType = resultType.WithNotNullState();
				}
				SetResult(boundCall, resultType, methodSymbol.ReturnTypeWithAnnotations);
				SetUpdatedSymbol(boundCall, boundCall.Method, methodSymbol);
			}
		}
		bool tryGetReceiver(BoundCall boundCall2, [MaybeNullWhen(false)] out BoundCall reference)
		{
			if (boundCall2.ReceiverOpt is BoundCall boundCall)
			{
				reference = boundCall;
				return true;
			}
			if (boundCall2.InvokedAsExtensionMethod)
			{
				ImmutableArray<BoundExpression> arguments = boundCall2.Arguments;
				if (arguments.Length >= 1 && arguments[0] is BoundCall boundCall3 && !VisitArgumentEvaluateNeedsCloningState(boundCall3))
				{
					reference = boundCall3;
					return true;
				}
			}
			reference = null;
			return false;
		}
		TypeWithState visitAndCheckReceiver(BoundCall boundCall)
		{
			if (boundCall.IsErroneousNode)
			{
				return VisitBadExpressionChild(boundCall.ReceiverOpt);
			}
			return VisitAndCheckReceiver(boundCall.ReceiverOpt, boundCall.Method);
		}
	}

	private TypeWithState VisitAndCheckReceiver(BoundExpression? receiverOpt, MethodSymbol method)
	{
		TypeWithState typeWithState = default(TypeWithState);
		if (receiverOpt != null)
		{
			if (!method.IsExtensionBlockMember())
			{
				typeWithState = VisitRvalueWithState(receiverOpt);
				CheckCallReceiver(receiverOpt, typeWithState, method);
			}
		}
		return typeWithState;
	}

	private (MethodSymbol method, ImmutableArray<VisitResult> results, bool returnNotNull) ReInferMethodAndVisitArguments(BoundNode node, BoundExpression? receiverOpt, TypeWithState receiverType, MethodSymbol method, ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> refKindsOpt, ImmutableArray<int> argsToParamsOpt, BitVector defaultArguments, bool expanded, bool invokedAsExtensionMethod, VisitResult? firstArgumentResult = null)
	{
		bool flag = method.IsExtensionBlockMember();
		refKindsOpt = GetArgumentRefKinds(refKindsOpt, flag, method, arguments.Length);
		if (!method.IsExtensionBlockMember() && !receiverType.HasNullType)
		{
			method = (MethodSymbol)AsMemberOfType(receiverType.Type, method);
		}
		arguments = getArguments(arguments, flag, receiverOpt);
		ImmutableArray<ParameterSymbol> parametersOpt = getParameters(method.Parameters, flag, method);
		argsToParamsOpt = GetArgsToParamsOpt(argsToParamsOpt, flag);
		var (methodSymbol, item, item2) = VisitArguments(node, arguments, refKindsOpt, parametersOpt, argsToParamsOpt, defaultArguments, expanded, invokedAsExtensionMethod, method, firstArgumentResult);
		ApplyMemberPostConditions(receiverOpt, methodSymbol);
		return (method: methodSymbol, results: item, returnNotNull: item2);
		static ImmutableArray<BoundExpression> getArguments(ImmutableArray<BoundExpression> immutableArray2, bool isExtensionBlockMethod, BoundExpression? receiver)
		{
			if (isExtensionBlockMethod && receiver != null)
			{
				ImmutableArray<BoundExpression> immutableArray = immutableArray2;
				int num = 0;
				BoundExpression[] array = new BoundExpression[1 + immutableArray.Length];
				array[num] = receiver;
				num++;
				ReadOnlySpan<BoundExpression> readOnlySpan = immutableArray.AsSpan();
				readOnlySpan.CopyTo(new Span<BoundExpression>(array).Slice(num, readOnlySpan.Length));
				num += readOnlySpan.Length;
				return ImmutableCollectionsMarshal.AsImmutableArray(array);
			}
			return immutableArray2;
		}
		static ImmutableArray<ParameterSymbol> getParameters(ImmutableArray<ParameterSymbol> parameters, bool isExtensionBlockMethod, MethodSymbol methodSymbol2)
		{
			if (!isExtensionBlockMethod)
			{
				return parameters;
			}
			ParameterSymbol extensionParameter = methodSymbol2.ContainingType.ExtensionParameter;
			ImmutableArray<ParameterSymbol> immutableArray = parameters;
			int num = 0;
			ParameterSymbol[] array = new ParameterSymbol[1 + immutableArray.Length];
			array[num] = extensionParameter;
			num++;
			ReadOnlySpan<ParameterSymbol> readOnlySpan = immutableArray.AsSpan();
			readOnlySpan.CopyTo(new Span<ParameterSymbol>(array).Slice(num, readOnlySpan.Length));
			num += readOnlySpan.Length;
			return ImmutableCollectionsMarshal.AsImmutableArray(array);
		}
	}

	internal static ImmutableArray<RefKind> GetArgumentRefKinds(ImmutableArray<RefKind> argumentRefKindsOpt, bool adjustForExtensionBlockMethod, MethodSymbol method, int argumentCount)
	{
		if (!adjustForExtensionBlockMethod)
		{
			return argumentRefKindsOpt;
		}
		RefKind extensionReceiverRefKind = GetExtensionReceiverRefKind(method);
		if (argumentRefKindsOpt.IsDefault)
		{
			if (extensionReceiverRefKind == RefKind.None)
			{
				return argumentRefKindsOpt;
			}
			ArrayBuilder<RefKind> instance = ArrayBuilder<RefKind>.GetInstance(argumentCount + 1, RefKind.None);
			instance[0] = extensionReceiverRefKind;
			return instance.ToImmutableAndFree();
		}
		RefKind refKind = extensionReceiverRefKind;
		ImmutableArray<RefKind> immutableArray = argumentRefKindsOpt;
		int num = 0;
		RefKind[] array = new RefKind[1 + immutableArray.Length];
		array[num] = refKind;
		num++;
		ReadOnlySpan<RefKind> readOnlySpan = immutableArray.AsSpan();
		readOnlySpan.CopyTo(new Span<RefKind>(array).Slice(num, readOnlySpan.Length));
		num += readOnlySpan.Length;
		return ImmutableCollectionsMarshal.AsImmutableArray(array);
	}

	private static RefKind GetExtensionReceiverRefKind(MethodSymbol method)
	{
		if (method.ContainingType.ExtensionParameter.RefKind != RefKind.Ref)
		{
			return RefKind.None;
		}
		return RefKind.Ref;
	}

	private static ImmutableArray<int> GetArgsToParamsOpt(ImmutableArray<int> argsToParamsOpt, bool isExtensionBlockMethod)
	{
		if (!isExtensionBlockMethod)
		{
			return argsToParamsOpt;
		}
		if (argsToParamsOpt.IsDefault)
		{
			return argsToParamsOpt;
		}
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance(argsToParamsOpt.Length + 1);
		instance.Add(0);
		for (int i = 0; i < argsToParamsOpt.Length; i++)
		{
			instance.Add(argsToParamsOpt[i] + 1);
		}
		return instance.ToImmutableAndFree();
	}

	private void LearnFromEqualsMethod(MethodSymbol method, BoundCall node, TypeWithState receiverType, ImmutableArray<VisitResult> results)
	{
		int parameterCount = method.ParameterCount;
		ImmutableArray<BoundExpression> arguments = node.Arguments;
		if (node.HasErrors || (parameterCount != 1 && parameterCount != 2) || parameterCount != arguments.Length || method.MethodKind != MethodKind.Ordinary || method.ReturnType.SpecialType != SpecialType.System_Boolean || (method.Name != SpecialMembers.GetDescriptor(SpecialMember.System_Object__Equals).Name && method.Name != SpecialMembers.GetDescriptor(SpecialMember.System_Object__ReferenceEquals).Name && !anyOverriddenMethodHasExplicitImplementation(method)))
		{
			return;
		}
		if (method.Equals(compilation.GetSpecialTypeMember(SpecialMember.System_Object__EqualsObjectObject)) || method.Equals(compilation.GetSpecialTypeMember(SpecialMember.System_Object__ReferenceEquals)) || isWellKnownEqualityMethodOrImplementation(compilation, method, receiverType.Type, WellKnownMember.System_Collections_Generic_IEqualityComparer_T__Equals))
		{
			learnFromEqualsMethodArguments(arguments[0], results[0].RValueType, arguments[1], results[1].RValueType);
			return;
		}
		bool flag = method.GetLeastOverriddenMethod(null).Equals(compilation.GetSpecialTypeMember(SpecialMember.System_Object__Equals));
		BoundExpression receiverOpt = node.ReceiverOpt;
		if (receiverOpt != null && (flag || isWellKnownEqualityMethodOrImplementation(compilation, method, receiverType.Type, WellKnownMember.System_IEquatable_T__Equals)))
		{
			learnFromEqualsMethodArguments(receiverOpt, receiverType, arguments[0], results[0].RValueType);
		}
		static bool anyOverriddenMethodHasExplicitImplementation(MethodSymbol methodSymbol2)
		{
			MethodSymbol methodSymbol = methodSymbol2;
			while ((object)methodSymbol != null)
			{
				if (methodSymbol.IsExplicitInterfaceImplementation)
				{
					return true;
				}
				methodSymbol = methodSymbol.OverriddenMethod;
			}
			return false;
		}
		static bool isWellKnownEqualityMethodOrImplementation(CSharpCompilation compilation, MethodSymbol overriddenMethod, TypeSymbol? typeSymbol, WellKnownMember wellKnownMember)
		{
			MethodSymbol methodSymbol = (MethodSymbol)compilation.GetWellKnownTypeMember(wellKnownMember);
			if ((object)methodSymbol == null || (object)typeSymbol == null)
			{
				return false;
			}
			NamedTypeSymbol containingType = methodSymbol.ContainingType;
			TypeWithAnnotations typeWithAnnotations = overriddenMethod.Parameters[0].TypeWithAnnotations;
			NamedTypeSymbol newOwner = containingType.Construct(ImmutableArray.Create(typeWithAnnotations));
			MethodSymbol methodSymbol2 = methodSymbol.AsMember(newOwner);
			if (methodSymbol2.Equals(overriddenMethod))
			{
				return true;
			}
			TypeSymbol typeSymbol2 = typeSymbol;
			while ((object)typeSymbol2 != null && (object)overriddenMethod != null)
			{
				Symbol symbol = typeSymbol2.FindImplementationForInterfaceMember(methodSymbol2);
				if ((object)symbol == null)
				{
					return false;
				}
				if (symbol.ContainingType.IsInterface)
				{
					return false;
				}
				MethodSymbol methodSymbol3 = overriddenMethod;
				while ((object)methodSymbol3 != null)
				{
					if (methodSymbol3.Equals(symbol))
					{
						return true;
					}
					methodSymbol3 = methodSymbol3.OverriddenMethod;
				}
				while (!typeSymbol2.Equals(symbol.ContainingType) && (object)overriddenMethod != null)
				{
					if (typeSymbol2.Equals(overriddenMethod.ContainingType))
					{
						overriddenMethod = overriddenMethod.OverriddenMethod;
					}
					typeSymbol2 = typeSymbol2.BaseTypeNoUseSiteDiagnostics;
				}
				if ((object)overriddenMethod != null && typeSymbol2.Equals(overriddenMethod.ContainingType))
				{
					overriddenMethod = overriddenMethod.OverriddenMethod;
				}
				typeSymbol2 = typeSymbol2.BaseTypeNoUseSiteDiagnostics;
			}
			return false;
		}
		void learnFromEqualsMethodArguments(BoundExpression left, TypeWithState leftType, BoundExpression right, TypeWithState rightType)
		{
			ConstantValue? constantValueOpt = left.ConstantValueOpt;
			if ((object)constantValueOpt != null && constantValueOpt.IsNull)
			{
				Split();
				LearnFromNullTest(right, ref StateWhenTrue);
				LearnFromNonNullTest(right, ref StateWhenFalse);
			}
			else
			{
				ConstantValue? constantValueOpt2 = right.ConstantValueOpt;
				if ((object)constantValueOpt2 != null && constantValueOpt2.IsNull)
				{
					Split();
					LearnFromNullTest(left, ref StateWhenTrue);
					LearnFromNonNullTest(left, ref StateWhenFalse);
				}
				else if (leftType.MayBeNull && rightType.IsNotNull)
				{
					Split();
					LearnFromNonNullTest(left, ref StateWhenTrue);
				}
				else if (rightType.MayBeNull && leftType.IsNotNull)
				{
					Split();
					LearnFromNonNullTest(right, ref StateWhenTrue);
				}
			}
		}
	}

	private bool IsCompareExchangeMethod(MethodSymbol? method)
	{
		if ((object)method == null)
		{
			return false;
		}
		if (!method.Equals(compilation.GetWellKnownTypeMember(WellKnownMember.System_Threading_Interlocked__CompareExchange), SymbolEqualityComparer.ConsiderEverything.CompareKind))
		{
			return method.OriginalDefinition.Equals(compilation.GetWellKnownTypeMember(WellKnownMember.System_Threading_Interlocked__CompareExchange_T), SymbolEqualityComparer.ConsiderEverything.CompareKind);
		}
		return true;
	}

	private NullableFlowState LearnFromCompareExchangeMethod(in CompareExchangeInfo compareExchangeInfo)
	{
		if (compareExchangeInfo.Arguments.Length != 3)
		{
			return NullableFlowState.NotNull;
		}
		ImmutableArray<int> argsToParamsOpt = compareExchangeInfo.ArgsToParamsOpt;
		int index;
		int index2;
		int index3;
		if (!argsToParamsOpt.IsDefault)
		{
			int num = argsToParamsOpt.IndexOf(2);
			int num2 = argsToParamsOpt.IndexOf(1);
			int num3 = argsToParamsOpt.IndexOf(0);
			index = num3;
			index2 = num2;
			index3 = num;
		}
		else
		{
			index3 = 2;
			index2 = 1;
			index = 0;
		}
		BoundExpression boundExpression = compareExchangeInfo.Arguments[index3];
		NullableFlowState nullableFlowState = compareExchangeInfo.Results[index2].RValueType.State;
		ConstantValue? constantValueOpt = boundExpression.ConstantValueOpt;
		if ((object)constantValueOpt == null || !constantValueOpt.IsNull)
		{
			NullableFlowState state = compareExchangeInfo.Results[index].RValueType.State;
			nullableFlowState = nullableFlowState.Join(state);
		}
		return nullableFlowState;
	}

	private void CheckCallReceiver(BoundExpression? receiverOpt, TypeWithState receiverType, MethodSymbol method)
	{
		if (!method.IsExtensionBlockMember())
		{
			bool checkNullableValueType = false;
			TypeSymbol type = receiverType.Type;
			if (method.RequiresInstanceReceiver && (object)type != null && type.IsNullableType() && method.ContainingType.IsReferenceType)
			{
				checkNullableValueType = true;
			}
			else if (method.OriginalDefinition == compilation.GetSpecialTypeMember(SpecialMember.System_Nullable_T_get_Value))
			{
				checkNullableValueType = true;
			}
			CheckPossibleNullReceiver(receiverOpt, receiverType, checkNullableValueType);
		}
	}

	private TypeWithState GetReturnTypeWithState(MethodSymbol method)
	{
		return TypeWithState.Create(method.ReturnTypeWithAnnotations, GetRValueAnnotations(method));
	}

	private FlowAnalysisAnnotations GetRValueAnnotations(Symbol? symbol)
	{
		if (IsAnalyzingAttribute)
		{
			return FlowAnalysisAnnotations.None;
		}
		return symbol.GetFlowAnalysisAnnotations() & (FlowAnalysisAnnotations.MaybeNull | FlowAnalysisAnnotations.NotNull);
	}

	private FlowAnalysisAnnotations GetParameterAnnotations(ParameterSymbol parameter)
	{
		if (IsAnalyzingAttribute)
		{
			return FlowAnalysisAnnotations.None;
		}
		FlowAnalysisAnnotations flowAnalysisAnnotations = parameter.FlowAnalysisAnnotations;
		if (!parameter.IsExtensionParameter() && GetTypeOrReturnType(parameter.ContainingSymbol).SpecialType != SpecialType.System_Boolean)
		{
			bool flag = (flowAnalysisAnnotations & FlowAnalysisAnnotations.NotNull) == FlowAnalysisAnnotations.NotNullWhenTrue;
			bool flag2 = (flowAnalysisAnnotations & FlowAnalysisAnnotations.NotNull) == FlowAnalysisAnnotations.NotNullWhenFalse;
			if (flag ^ flag2)
			{
				flowAnalysisAnnotations &= ~FlowAnalysisAnnotations.NotNull;
			}
			bool flag3 = (flowAnalysisAnnotations & FlowAnalysisAnnotations.MaybeNull) == FlowAnalysisAnnotations.MaybeNullWhenTrue;
			bool flag4 = (flowAnalysisAnnotations & FlowAnalysisAnnotations.MaybeNull) == FlowAnalysisAnnotations.MaybeNullWhenFalse;
			if (flag3 ^ flag4)
			{
				flowAnalysisAnnotations &= ~FlowAnalysisAnnotations.MaybeNull;
			}
		}
		return flowAnalysisAnnotations;
	}

	private static TypeWithAnnotations ApplyLValueAnnotations(TypeWithAnnotations declaredType, FlowAnalysisAnnotations flowAnalysisAnnotations)
	{
		if ((flowAnalysisAnnotations & FlowAnalysisAnnotations.DisallowNull) == FlowAnalysisAnnotations.DisallowNull)
		{
			return declaredType.AsNotAnnotated();
		}
		if ((flowAnalysisAnnotations & FlowAnalysisAnnotations.AllowNull) == FlowAnalysisAnnotations.AllowNull)
		{
			return declaredType.AsAnnotated();
		}
		return declaredType;
	}

	private static TypeWithState ApplyUnconditionalAnnotations(TypeWithState typeWithState, FlowAnalysisAnnotations annotations)
	{
		if ((annotations & FlowAnalysisAnnotations.NotNull) == FlowAnalysisAnnotations.NotNull)
		{
			return TypeWithState.Create(typeWithState.Type, NullableFlowState.NotNull);
		}
		if ((annotations & FlowAnalysisAnnotations.MaybeNull) == FlowAnalysisAnnotations.MaybeNull)
		{
			return TypeWithState.Create(typeWithState.Type, NullableFlowState.MaybeDefault);
		}
		return typeWithState;
	}

	private static TypeWithAnnotations ApplyUnconditionalAnnotations(TypeWithAnnotations declaredType, FlowAnalysisAnnotations annotations)
	{
		if ((annotations & FlowAnalysisAnnotations.MaybeNull) == FlowAnalysisAnnotations.MaybeNull)
		{
			return declaredType.AsAnnotated();
		}
		if ((annotations & FlowAnalysisAnnotations.NotNull) == FlowAnalysisAnnotations.NotNull)
		{
			return declaredType.AsNotAnnotated();
		}
		return declaredType;
	}

	private static bool HasImplicitTypeArguments(BoundNode node)
	{
		if ((node is BoundCollectionElementInitializer || node is BoundForEachStatement || node is BoundPropertyAccess || node is BoundIncrementOperator || node is BoundCompoundAssignmentOperator || node is BoundDagPropertyEvaluation) ? true : false)
		{
			return true;
		}
		SyntaxNode syntax = node.Syntax;
		if (syntax.Kind() != SyntaxKind.InvocationExpression)
		{
			return false;
		}
		return HasImplicitTypeArguments(((InvocationExpressionSyntax)syntax).Expression);
	}

	private static bool HasImplicitTypeArguments(SyntaxNode syntax)
	{
		NameSyntax nameSyntax = Binder.GetNameSyntax(syntax, out var _);
		if (nameSyntax == null)
		{
			return false;
		}
		nameSyntax = nameSyntax.GetUnqualifiedName();
		return nameSyntax.Kind() != SyntaxKind.GenericName;
	}

	protected override void VisitArguments(ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> refKindsOpt, MethodSymbol method, ImmutableArray<int> argsToParamsOpt, bool expanded)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/NullableWalker.cs", 7231);
	}

	private (MethodSymbol? method, ImmutableArray<VisitResult> results, bool returnNotNull) VisitArguments(BoundExpression node, ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> refKindsOpt, MethodSymbol? method, ImmutableArray<int> argsToParamsOpt, BitVector defaultArguments, bool expanded, bool invokedAsExtensionMethod)
	{
		return VisitArguments(node, arguments, refKindsOpt, method?.Parameters ?? default(ImmutableArray<ParameterSymbol>), argsToParamsOpt, defaultArguments, expanded, invokedAsExtensionMethod, method);
	}

	private ImmutableArray<VisitResult> VisitArguments(BoundExpression node, ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> refKindsOpt, PropertySymbol? property, ImmutableArray<int> argsToParamsOpt, BitVector defaultArguments, bool expanded)
	{
		return VisitArguments<PropertySymbol>(node, arguments, refKindsOpt, property?.Parameters ?? default(ImmutableArray<ParameterSymbol>), argsToParamsOpt, defaultArguments, expanded, invokedAsExtensionMethod: false).results;
	}

	private (TMember? member, ImmutableArray<VisitResult> results, bool returnNotNull) VisitArguments<TMember>(BoundNode node, ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> refKindsOpt, ImmutableArray<ParameterSymbol> parametersOpt, ImmutableArray<int> argsToParamsOpt, BitVector defaultArguments, bool expanded, bool invokedAsExtensionMethod, TMember? member = null, VisitResult? firstArgumentResult = null) where TMember : Symbol
	{
		(TMember, ImmutableArray<VisitResult>, bool, ArgumentsCompletionDelegate<TMember>) tuple = VisitArguments(node, arguments, refKindsOpt, parametersOpt, argsToParamsOpt, defaultArguments, expanded, invokedAsExtensionMethod, member, delayCompletionForTargetMember: false, firstArgumentResult);
		return (member: tuple.Item1, results: tuple.Item2, returnNotNull: tuple.Item3);
	}

	private (TMember? member, ImmutableArray<VisitResult> results, bool returnNotNull, ArgumentsCompletionDelegate<TMember>? completion) VisitArguments<TMember>(BoundNode node, ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> refKindsOpt, ImmutableArray<ParameterSymbol> parametersOpt, ImmutableArray<int> argsToParamsOpt, BitVector defaultArguments, bool expanded, bool invokedAsExtensionMethod, TMember? member, bool delayCompletionForTargetMember, VisitResult? firstArgumentResult = null) where TMember : Symbol
	{
		if (expanded)
		{
			expandParamsCollection(ref arguments, ref refKindsOpt, parametersOpt, ref argsToParamsOpt, ref defaultArguments);
		}
		(ImmutableArray<BoundExpression> arguments, ImmutableArray<Conversion> conversions) tuple = RemoveArgumentConversions(arguments, refKindsOpt);
		ImmutableArray<BoundExpression> item = tuple.arguments;
		ImmutableArray<Conversion> item2 = tuple.conversions;
		ImmutableArray<VisitResult> results = VisitArgumentsEvaluate(item, refKindsOpt, GetParametersAnnotations(arguments, parametersOpt, argsToParamsOpt, expanded), defaultArguments, firstArgumentResult);
		return visitArguments(node, arguments, item, item2, results, refKindsOpt, parametersOpt, argsToParamsOpt, defaultArguments, expanded, invokedAsExtensionMethod, member, delayCompletionForTargetMember);
		static void expandParamsCollection(ref ImmutableArray<BoundExpression> reference, ref ImmutableArray<RefKind> reference3, ImmutableArray<ParameterSymbol> immutableArray, ref ImmutableArray<int> reference2, ref BitVector reference4)
		{
			for (int i = 0; i < reference.Length; i++)
			{
				BoundExpression boundExpression = reference[i];
				if (boundExpression.IsParamsArrayOrCollection)
				{
					ImmutableArray<BoundExpression> items = ((!(boundExpression is BoundArrayCreation boundArrayCreation)) ? ((BoundCollectionExpression)((BoundConversion)boundExpression).Operand).UnconvertedCollectionExpression.Elements.CastArray<BoundExpression>() : boundArrayCreation.InitializerOpt.Initializers);
					if (items.Length == 0)
					{
						reference = reference.RemoveAt(i);
						if (!reference2.IsDefault)
						{
							reference2 = reference2.RemoveAt(i);
						}
						if (!reference3.IsDefaultOrEmpty)
						{
							reference3 = reference3.RemoveAt(i);
						}
					}
					else
					{
						ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(reference.Length + items.Length - 1);
						instance.AddRange(reference, i);
						instance.AddRange(items);
						instance.AddRange(reference, i + 1, reference.Length - (i + 1));
						if (!reference2.IsDefault)
						{
							ArrayBuilder<int> instance2 = ArrayBuilder<int>.GetInstance(reference2.Length + items.Length - 1);
							instance2.AddRange(reference2, i);
							instance2.AddMany(reference.Length - 1, items.Length);
							instance2.AddRange(reference2, i + 1, reference2.Length - (i + 1));
							reference2 = instance2.ToImmutableAndFree();
						}
						if (!reference3.IsDefaultOrEmpty)
						{
							ArrayBuilder<RefKind> instance3 = ArrayBuilder<RefKind>.GetInstance(reference3.Length + items.Length - 1);
							instance3.AddRange(reference3, i);
							instance3.AddMany(RefKind.None, items.Length);
							instance3.AddRange(reference3, i + 1, reference3.Length - (i + 1));
							reference3 = instance3.ToImmutableAndFree();
						}
						reference = instance.ToImmutableAndFree();
					}
					break;
				}
			}
		}
		bool tryShortCircuitTargetTypedExpression(BoundExpression argument, BoundExpression argumentNoConversion)
		{
			Func<TypeWithAnnotations, TypeWithState> value = default(Func<TypeWithAnnotations, TypeWithState>);
			if (IsTargetTypedExpression(argumentNoConversion) && (_targetTypedAnalysisCompletionOpt?.TryGetValue(argumentNoConversion, out value) ?? false))
			{
				value(TypeWithAnnotations.Create(argument.Type));
				TargetTypedAnalysisCompletion.Remove(argumentNoConversion);
				return true;
			}
			return false;
		}
		(TMember? member, ImmutableArray<VisitResult> results, bool returnNotNull, ArgumentsCompletionDelegate<TMember>? completion) visitArguments(BoundNode boundNode, ImmutableArray<BoundExpression> arguments2, ImmutableArray<BoundExpression> argumentsNoConversions, ImmutableArray<Conversion> conversions, ImmutableArray<VisitResult> immutableArray, ImmutableArray<RefKind> immutableArray2, ImmutableArray<ParameterSymbol> parametersIncludingExtensionParameter, ImmutableArray<int> argsToParamsOpt2, BitVector defaultArguments2, bool expanded2, bool flag2, TMember? val, bool flag)
		{
			bool item3 = false;
			if (flag)
			{
				return (member: val, results: immutableArray, returnNotNull: item3, completion: visitArgumentsAsContinuation(boundNode, arguments2, argumentsNoConversions, conversions, immutableArray2, argsToParamsOpt2, defaultArguments2, expanded2, flag2));
			}
			if ((object)val != null && val.GetMemberArityIncludingExtension() > 0)
			{
				if (HasImplicitTypeArguments(boundNode))
				{
					val = InferMemberTypeArguments(val, GetArgumentsForMethodTypeInference(immutableArray, argumentsNoConversions), immutableArray2, argsToParamsOpt2, expanded2);
					parametersIncludingExtensionParameter = val.GetParametersIncludingExtensionParameter(skipExtensionIfStatic: false);
				}
				SyntaxNode syntax = boundNode.Syntax;
				SyntaxNode syntaxNode;
				if (syntax is InvocationExpressionSyntax invocationExpressionSyntax)
				{
					ExpressionSyntax expression = invocationExpressionSyntax.Expression;
					syntaxNode = expression;
				}
				else if (syntax is ForEachStatementSyntax forEachStatementSyntax)
				{
					ExpressionSyntax expression2 = forEachStatementSyntax.Expression;
					syntaxNode = expression2;
				}
				else
				{
					syntaxNode = boundNode.Syntax;
				}
				SyntaxNode syntax2 = syntaxNode;
				if (val is MethodSymbol method)
				{
					if (ConstraintsHelper.RequiresChecking(method))
					{
						CheckMethodConstraints(syntax2, method);
					}
				}
				else
				{
					if (!val.IsExtensionBlockMember())
					{
						throw ExceptionUtilities.UnexpectedValue(val);
					}
					NamedTypeSymbol containingType = val.ContainingType;
					if ((object)containingType != null && ConstraintsHelper.RequiresChecking(containingType))
					{
						CheckExtensionConstraints(syntax2, containingType);
					}
				}
			}
			MethodSymbol methodSymbol = val as MethodSymbol;
			ArrayBuilder<ParameterSymbol> arrayBuilder = ((!IsAnalyzingAttribute && !parametersIncludingExtensionParameter.IsDefault && parametersIncludingExtensionParameter.Any((ParameterSymbol p) => !p.NotNullIfParameterNotNull.IsEmpty)) ? ArrayBuilder<ParameterSymbol>.GetInstance() : null);
			ArrayBuilder<VisitResult> instance = ArrayBuilder<VisitResult>.GetInstance(immutableArray.Length);
			if (!parametersIncludingExtensionParameter.IsDefault)
			{
				TypeWithAnnotations paramsIterationType = default(TypeWithAnnotations);
				ImmutableHashSet<string> immutableHashSet = (IsAnalyzingAttribute ? null : methodSymbol?.ReturnNotNullIfParameterNotNull);
				for (int num = 0; num < immutableArray.Length; num++)
				{
					BoundExpression boundExpression = argumentsNoConversions[num];
					BoundExpression boundExpression2 = ((num < arguments2.Length) ? arguments2[num] : boundExpression);
					var (parameterSymbol, parameterType, parameterAnnotations, flag3) = GetCorrespondingParameter(num, parametersIncludingExtensionParameter, argsToParamsOpt2, expanded2, ref paramsIterationType);
					if ((object)parameterSymbol == null)
					{
						if (!tryShortCircuitTargetTypedExpression(boundExpression2, boundExpression))
						{
						}
					}
					else if (!(boundNode is BoundCall boundCall) || !boundNode.HasErrors || boundCall.ArgumentNamesOpt.IsDefaultOrEmpty || !boundCall.ArgsToParamsOpt.IsDefault || !tryShortCircuitTargetTypedExpression(boundExpression2, boundExpression))
					{
						bool disableDiagnostics = _disableDiagnostics;
						_disableDiagnostics |= boundNode.HasErrors || defaultArguments2[num];
						VisitArgumentConversionAndInboundAssignmentsAndPreConditions(GetConversionIfApplicable(boundExpression2, boundExpression), boundExpression, (conversions.IsDefault || num >= conversions.Length) ? Conversion.Identity : conversions[num], AbstractFlowPass<LocalState, LocalFunctionState>.GetRefKind(immutableArray2, num), parameterSymbol, parameterType, parameterAnnotations, immutableArray[num], instance, flag2 && num == 0);
						_disableDiagnostics = disableDiagnostics;
						if (((object)val == null || !val.IsExtensionBlockMember() || !val.IsStatic || num != 0) && (immutableArray[num].RValueType.IsNotNull | flag3))
						{
							arrayBuilder?.Add(parameterSymbol);
							if (immutableHashSet != null && immutableHashSet.Contains(parameterSymbol.Name))
							{
								item3 = true;
							}
						}
					}
				}
			}
			instance.Free();
			if (boundNode is BoundCall boundCall2)
			{
				MethodSymbol method2 = boundCall2.Method;
				if ((object)method2 != null && method2.OriginalDefinition is LocalFunctionSymbol symbol)
				{
					VisitLocalFunctionUse(symbol);
				}
			}
			if (!boundNode.HasErrors && !parametersIncludingExtensionParameter.IsDefault)
			{
				CompareExchangeInfo compareExchangeInfo = (IsCompareExchangeMethod(methodSymbol) ? new CompareExchangeInfo(arguments2, immutableArray, argsToParamsOpt2) : default(CompareExchangeInfo));
				TypeWithAnnotations paramsIterationType2 = default(TypeWithAnnotations);
				for (int num2 = 0; num2 < arguments2.Length; num2++)
				{
					var (parameterSymbol2, parameterType2, parameterAnnotations2, _) = GetCorrespondingParameter(num2, parametersIncludingExtensionParameter, argsToParamsOpt2, expanded2, ref paramsIterationType2);
					if ((object)parameterSymbol2 != null)
					{
						VisitArgumentOutboundAssignmentsAndPostConditions(arguments2[num2], AbstractFlowPass<LocalState, LocalFunctionState>.GetRefKind(immutableArray2, num2), parameterSymbol2, parameterType2, parameterAnnotations2, immutableArray[num2], arrayBuilder, (!compareExchangeInfo.IsDefault && parameterSymbol2.Ordinal == 0) ? compareExchangeInfo : default(CompareExchangeInfo));
					}
				}
			}
			else
			{
				for (int num3 = 0; num3 < arguments2.Length; num3++)
				{
					BoundExpression boundExpression3 = arguments2[num3];
					VisitResult visitResult = immutableArray[num3];
					BoundExpression convertedNode = argumentsNoConversions[num3];
					TrackAnalyzedNullabilityThroughConversionGroup(TypeWithState.Create(boundExpression3.Type, visitResult.RValueType.State), boundExpression3 as BoundConversion, convertedNode);
				}
			}
			if (!IsAnalyzingAttribute && (object)methodSymbol != null && (methodSymbol.FlowAnalysisAnnotations & FlowAnalysisAnnotations.DoesNotReturn) == FlowAnalysisAnnotations.DoesNotReturn)
			{
				SetUnreachable();
			}
			arrayBuilder?.Free();
			return (member: val, results: immutableArray, returnNotNull: item3, completion: null);
		}
		ArgumentsCompletionDelegate<TMember> visitArgumentsAsContinuation(BoundNode node2, ImmutableArray<BoundExpression> arguments2, ImmutableArray<BoundExpression> argumentsNoConversions, ImmutableArray<Conversion> conversions, ImmutableArray<RefKind> refKindsOpt2, ImmutableArray<int> argsToParamsOpt2, BitVector defaultArguments2, bool expanded2, bool invokedAsExtensionMethod2)
		{
			return delegate(ImmutableArray<VisitResult> results2, ImmutableArray<ParameterSymbol> parametersOpt2, TMember? member2)
			{
				(TMember, ImmutableArray<VisitResult>, bool, ArgumentsCompletionDelegate<TMember>) tuple2 = visitArguments(node2, arguments2, argumentsNoConversions, conversions, results2, refKindsOpt2, parametersOpt2, argsToParamsOpt2, defaultArguments2, expanded2, invokedAsExtensionMethod2, member2, delayCompletionForTargetMember: false);
				return (member: tuple2.Item1, returnNotNull: tuple2.Item3);
			};
		}
	}

	private void ApplyMemberPostConditions(BoundExpression? receiverOpt, MethodSymbol? method)
	{
		if ((object)method != null)
		{
			int num = ((receiverOpt != null && !method.IsStatic) ? MakeSlot(receiverOpt) : GetReceiverSlotForMemberPostConditions(method));
			if (num >= 0)
			{
				ApplyMemberPostConditions(num, method);
			}
		}
	}

	private int GetReceiverSlotForMemberPostConditions(MethodSymbol? method)
	{
		if ((object)method == null)
		{
			return -1;
		}
		if (method.IsStatic)
		{
			return 0;
		}
		MethodSymbol methodSymbol = method;
		while (methodSymbol.ContainingSymbol is MethodSymbol methodSymbol2)
		{
			methodSymbol = methodSymbol2;
			if (methodSymbol2.IsStatic)
			{
				return 0;
			}
		}
		if (methodSymbol.TryGetThisParameter(out ParameterSymbol thisParameter) && (object)thisParameter != null)
		{
			return GetOrCreateSlot(thisParameter);
		}
		return 0;
	}

	private void ApplyMemberPostConditions(int receiverSlot, MethodSymbol method)
	{
		if (method.IsExtensionBlockMember())
		{
			return;
		}
		do
		{
			NamedTypeSymbol containingType = method.ContainingType;
			ImmutableArray<string> notNullMembers = method.NotNullMembers;
			ImmutableArray<string> notNullWhenTrueMembers = method.NotNullWhenTrueMembers;
			ImmutableArray<string> notNullWhenFalseMembers = method.NotNullWhenFalseMembers;
			if (IsConditionalState)
			{
				applyMemberPostConditions(receiverSlot, containingType, notNullMembers, ref StateWhenTrue);
				applyMemberPostConditions(receiverSlot, containingType, notNullMembers, ref StateWhenFalse);
			}
			else
			{
				applyMemberPostConditions(receiverSlot, containingType, notNullMembers, ref State);
			}
			if (method.ReturnType.SpecialType == SpecialType.System_Boolean && (!notNullWhenTrueMembers.IsEmpty || !notNullWhenFalseMembers.IsEmpty))
			{
				Split();
				applyMemberPostConditions(receiverSlot, containingType, notNullWhenTrueMembers, ref StateWhenTrue);
				applyMemberPostConditions(receiverSlot, containingType, notNullWhenFalseMembers, ref StateWhenFalse);
			}
			method = method.OverriddenMethod;
		}
		while (method != null);
		void applyMemberPostConditions(int receiverSlot2, TypeSymbol type, ImmutableArray<string> members, ref LocalState state)
		{
			if (!members.IsEmpty)
			{
				foreach (string item in members)
				{
					markMembersAsNotNull(receiverSlot2, type, item, ref state);
				}
			}
		}
		void markMembersAsNotNull(int num, TypeSymbol type, string memberName, ref LocalState state)
		{
			foreach (Symbol member in type.GetMembers(memberName))
			{
				if (member.IsStatic)
				{
					num = 0;
				}
				else if (num == 0)
				{
					continue;
				}
				switch (member.Kind)
				{
				case SymbolKind.Field:
				case SymbolKind.Property:
				{
					int orCreateSlot = GetOrCreateSlot(member, num);
					if (orCreateSlot > 0)
					{
						SetState(ref state, orCreateSlot, NullableFlowState.NotNull);
					}
					break;
				}
				}
			}
		}
	}

	private ImmutableArray<VisitResult> VisitArgumentsEvaluate(ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> refKindsOpt, ImmutableArray<FlowAnalysisAnnotations> parameterAnnotationsOpt, BitVector defaultArguments, VisitResult? firstArgumentResult = null)
	{
		int length = arguments.Length;
		if (length == 0 && parameterAnnotationsOpt.IsDefaultOrEmpty)
		{
			return ImmutableArray<VisitResult>.Empty;
		}
		ArrayBuilder<VisitResult> instance = ArrayBuilder<VisitResult>.GetInstance(length);
		bool disableDiagnostics = _disableDiagnostics;
		for (int i = 0; i < length; i++)
		{
			_disableDiagnostics = defaultArguments[i] | disableDiagnostics;
			if (i == 0 && firstArgumentResult.HasValue)
			{
				VisitResult valueOrDefault = firstArgumentResult.GetValueOrDefault();
				instance.Add(valueOrDefault);
			}
			else
			{
				instance.Add(VisitArgumentEvaluate(arguments[i], AbstractFlowPass<LocalState, LocalFunctionState>.GetRefKind(refKindsOpt, i), (!parameterAnnotationsOpt.IsDefault) ? parameterAnnotationsOpt[i] : FlowAnalysisAnnotations.None));
			}
		}
		_disableDiagnostics = disableDiagnostics;
		SetInvalidResult();
		return instance.ToImmutableAndFree();
	}

	private ImmutableArray<FlowAnalysisAnnotations> GetParametersAnnotations(ImmutableArray<BoundExpression> arguments, ImmutableArray<ParameterSymbol> parametersOpt, ImmutableArray<int> argsToParamsOpt, bool expanded)
	{
		ImmutableArray<FlowAnalysisAnnotations> result = default(ImmutableArray<FlowAnalysisAnnotations>);
		if (!parametersOpt.IsDefault)
		{
			if (expanded)
			{
				TypeWithAnnotations paramsIterationType = default(TypeWithAnnotations);
				return arguments.SelectAsArray((BoundExpression argument, int i, (NullableWalker self, ImmutableArray<ParameterSymbol> parametersOpt, ImmutableArray<int> argsToParamsOpt) arg) => arg.self.GetCorrespondingParameter(i, arg.parametersOpt, arg.argsToParamsOpt, expanded: true, ref paramsIterationType).Annotations, (this, parametersOpt, argsToParamsOpt));
			}
			return arguments.SelectAsArray(delegate(BoundExpression argument, int i, (NullableWalker self, ImmutableArray<ParameterSymbol> parametersOpt, ImmutableArray<int> argsToParamsOpt) arg)
			{
				TypeWithAnnotations paramsIterationType2 = default(TypeWithAnnotations);
				return arg.self.GetCorrespondingParameter(i, arg.parametersOpt, arg.argsToParamsOpt, expanded: false, ref paramsIterationType2).Annotations;
			}, (this, parametersOpt, argsToParamsOpt));
		}
		return result;
	}

	private VisitResult VisitArgumentEvaluate(BoundExpression argument, RefKind refKind, FlowAnalysisAnnotations annotations)
	{
		Visit(argument);
		return VisitArgumentEvaluateEpilogue(argument, refKind, annotations);
	}

	private bool VisitArgumentEvaluateNeedsCloningState(BoundExpression argument)
	{
		return argument.Kind == BoundKind.Lambda;
	}

	private VisitResult VisitArgumentEvaluateEpilogue(BoundExpression argument, RefKind refKind, FlowAnalysisAnnotations annotations)
	{
		switch (refKind)
		{
		case RefKind.Ref:
			Unsplit();
			break;
		case RefKind.None:
		case RefKind.In:
			switch (annotations & FlowAnalysisAnnotations.DoesNotReturn)
			{
			case FlowAnalysisAnnotations.DoesNotReturnIfTrue:
				if (IsConditionalState)
				{
					SetState(StateWhenFalse);
				}
				break;
			case FlowAnalysisAnnotations.DoesNotReturnIfFalse:
				if (IsConditionalState)
				{
					SetState(StateWhenTrue);
				}
				break;
			default:
				VisitRvalueEpilogue(argument);
				break;
			}
			break;
		case RefKind.Out:
			Unsplit();
			UseLvalueOnly(argument);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(refKind);
		}
		return _visitResult;
	}

	private void VisitArgumentConversionAndInboundAssignmentsAndPreConditions(BoundConversion? conversionOpt, BoundExpression argumentNoConversion, Conversion conversion, RefKind refKind, ParameterSymbol parameter, TypeWithAnnotations parameterType, FlowAnalysisAnnotations parameterAnnotations, VisitResult result, ArrayBuilder<VisitResult>? conversionResultsBuilder, bool extensionMethodThisArgument)
	{
		TypeWithState rValueType = result.RValueType;
		switch (refKind)
		{
		case RefKind.None:
		case RefKind.In:
		{
			if (conversion.IsValid && conversion.Kind == ConversionKind.ImplicitUserDefined)
			{
				TypeSymbol type = rValueType.Type;
				conversion = GenerateConversion(_conversions, argumentNoConversion, type, parameterType.Type, fromExplicitCast: false, extensionMethodThisArgument: false, conversionOpt?.Checked ?? false);
				if (!conversion.Exists && !argumentNoConversion.IsSuppressed)
				{
					ReportNullabilityMismatchInArgument(argumentNoConversion.Syntax, type, parameter, parameterType.Type, forOutput: false);
				}
			}
			TypeWithState typeWithState = VisitConversion(conversionOpt, argumentNoConversion, conversion, ApplyLValueAnnotations(parameterType, parameterAnnotations), rValueType, checkConversion: true, fromExplicitCast: false, useLegacyWarnings: false, AssignmentKind.Argument, parameter, reportTopLevelWarnings: true, reportRemainingWarnings: true, isSuppressed: false, extensionMethodThisArgument, result.StateForLambda, trackMembers: false, null, conversionResultsBuilder);
			if (CheckDisallowedNullAssignment(typeWithState, parameterAnnotations, argumentNoConversion.Syntax))
			{
				LearnFromNonNullTest(argumentNoConversion, ref State);
			}
			SetResultType(argumentNoConversion, typeWithState, updateAnalyzedNullability: false);
			conversionResultsBuilder?.Add(_visitResult);
			break;
		}
		case RefKind.Ref:
			if (!argumentNoConversion.IsSuppressed)
			{
				TypeWithAnnotations lValueType = result.LValueType;
				if (IsNullabilityMismatch(lValueType.Type, parameterType.Type))
				{
					ReportNullabilityMismatchInRefArgument(argumentNoConversion, lValueType.Type, parameter, parameterType.Type);
				}
				else
				{
					ReportNullableAssignmentIfNecessary(argumentNoConversion, ApplyLValueAnnotations(parameterType, parameterAnnotations), rValueType, useLegacyWarnings: false);
					CheckDisallowedNullAssignment(rValueType, parameterAnnotations, argumentNoConversion.Syntax);
				}
			}
			conversionResultsBuilder?.Add(result);
			break;
		case RefKind.Out:
			conversionResultsBuilder?.Add(result);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(refKind);
		}
	}

	private bool CheckDisallowedNullAssignment(TypeWithState state, FlowAnalysisAnnotations annotations, SyntaxNode node, BoundExpression? boundValueOpt = null)
	{
		if (boundValueOpt != null && boundValueOpt.WasCompilerGenerated)
		{
			return false;
		}
		if (IsDisallowedNullAssignment(state, annotations))
		{
			ReportDiagnostic(ErrorCode.WRN_DisallowNullAttributeForbidsMaybeNullAssignment, node.Location);
			return true;
		}
		return false;
	}

	private static bool IsDisallowedNullAssignment(TypeWithState valueState, FlowAnalysisAnnotations targetAnnotations)
	{
		if ((targetAnnotations & FlowAnalysisAnnotations.DisallowNull) != FlowAnalysisAnnotations.None && hasNoNonNullableCounterpart(valueState.Type))
		{
			return valueState.MayBeNull;
		}
		return false;
		static bool hasNoNonNullableCounterpart(TypeSymbol? type)
		{
			if ((object)type == null)
			{
				return false;
			}
			if (type.Kind != SymbolKind.TypeParameter || type.IsReferenceType)
			{
				return type.IsNullableTypeOrTypeParameter();
			}
			return true;
		}
	}

	private void VisitArgumentOutboundAssignmentsAndPostConditions(BoundExpression argument, RefKind refKind, ParameterSymbol parameter, TypeWithAnnotations parameterType, FlowAnalysisAnnotations parameterAnnotations, VisitResult result, ArrayBuilder<ParameterSymbol>? notNullParametersOpt, CompareExchangeInfo compareExchangeInfoOpt)
	{
		switch (refKind)
		{
		case RefKind.None:
		case RefKind.In:
			LearnFromPostConditions(argument, parameterAnnotations);
			break;
		case RefKind.Ref:
		{
			parameterAnnotations = notNullBasedOnParameters(parameterAnnotations, notNullParametersOpt, parameter);
			TypeWithState typeWithState = TypeWithState.Create(parameterType, parameterAnnotations);
			if (!compareExchangeInfoOpt.IsDefault)
			{
				NullableFlowState defaultState = LearnFromCompareExchangeMethod(in compareExchangeInfoOpt);
				typeWithState = TypeWithState.Create(parameterType.Type, defaultState);
			}
			BoundParameter boundParameter2 = new BoundParameter(argument.Syntax, parameter);
			TypeWithAnnotations lValueType2 = result.LValueType;
			trackNullableStateForAssignment(boundParameter2, lValueType2, MakeSlot(argument), typeWithState, argument.IsSuppressed, parameterAnnotations);
			if (!argument.IsSuppressed)
			{
				FlowAnalysisAnnotations lValueAnnotations2 = GetLValueAnnotations(argument);
				ReportNullableAssignmentIfNecessary(boundParameter2, ApplyLValueAnnotations(lValueType2, lValueAnnotations2), applyPostConditionsUnconditionally(typeWithState, parameterAnnotations), UseLegacyWarnings(argument));
			}
			break;
		}
		case RefKind.Out:
		{
			parameterAnnotations = notNullBasedOnParameters(parameterAnnotations, notNullParametersOpt, parameter);
			TypeWithState rightState = TypeWithState.Create(parameterType, parameterAnnotations);
			TypeWithState valueType = applyPostConditionsUnconditionally(rightState, parameterAnnotations);
			TypeWithAnnotations lValueType = result.LValueType;
			FlowAnalysisAnnotations lValueAnnotations = GetLValueAnnotations(argument);
			TypeWithAnnotations typeWithAnnotations = ApplyLValueAnnotations(lValueType, lValueAnnotations);
			if (argument is BoundLocal { DeclarationKind: BoundLocalDeclarationKind.WithInferredType } boundLocal)
			{
				TypeWithAnnotations typeWithAnnotations2 = valueType.ToAnnotatedTypeWithAnnotations(compilation);
				_variables.SetType(boundLocal.LocalSymbol, typeWithAnnotations2);
				typeWithAnnotations = typeWithAnnotations2;
			}
			else if (argument is BoundDiscardExpression { IsInferred: not false } boundDiscardExpression)
			{
				SetAnalyzedNullability(boundDiscardExpression, new VisitResult(rightState, rightState.ToTypeWithAnnotations(compilation)), true);
			}
			BoundParameter boundParameter = new BoundParameter(argument.Syntax, parameter);
			CheckDisallowedNullAssignment(rightState, lValueAnnotations, argument.Syntax);
			AdjustSetValue(argument, ref rightState);
			trackNullableStateForAssignment(boundParameter, typeWithAnnotations, MakeSlot(argument), rightState, argument.IsSuppressed, parameterAnnotations);
			if (!argument.IsSuppressed)
			{
				ReportNullableAssignmentIfNecessary(boundParameter, typeWithAnnotations, valueType, UseLegacyWarnings(argument));
				CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
				if (!_conversions.HasIdentityOrImplicitReferenceConversion(parameterType.Type, typeWithAnnotations.Type, ref useSiteInfo))
				{
					ReportNullabilityMismatchInArgument(argument.Syntax, typeWithAnnotations.Type, parameter, parameterType.Type, forOutput: true);
				}
			}
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(refKind);
		}
		static TypeWithState applyPostConditionsUnconditionally(TypeWithState result2, FlowAnalysisAnnotations annotations)
		{
			if ((annotations & FlowAnalysisAnnotations.MaybeNull) != FlowAnalysisAnnotations.None)
			{
				return TypeWithState.Create(result2.Type, NullableFlowState.MaybeDefault);
			}
			if ((annotations & FlowAnalysisAnnotations.NotNull) == FlowAnalysisAnnotations.NotNull)
			{
				return TypeWithState.Create(result2.Type, NullableFlowState.NotNull);
			}
			return result2;
		}
		static TypeWithState applyPostConditionsWhenFalse(TypeWithState result2, FlowAnalysisAnnotations annotations)
		{
			bool flag = (annotations & FlowAnalysisAnnotations.NotNullWhenFalse) != 0;
			bool flag2 = (annotations & FlowAnalysisAnnotations.MaybeNullWhenTrue) != 0;
			if ((annotations & FlowAnalysisAnnotations.MaybeNullWhenFalse) != FlowAnalysisAnnotations.None && !(flag2 & flag))
			{
				return TypeWithState.Create(result2.Type, NullableFlowState.MaybeDefault);
			}
			if (flag)
			{
				return TypeWithState.Create(result2.Type, NullableFlowState.NotNull);
			}
			return result2;
		}
		static TypeWithState applyPostConditionsWhenTrue(TypeWithState result2, FlowAnalysisAnnotations annotations)
		{
			bool flag = (annotations & FlowAnalysisAnnotations.NotNullWhenTrue) != 0;
			bool num = (annotations & FlowAnalysisAnnotations.MaybeNullWhenTrue) != 0;
			bool flag2 = (annotations & FlowAnalysisAnnotations.MaybeNullWhenFalse) != 0;
			if (num && !(flag2 & flag))
			{
				return TypeWithState.Create(result2.Type, NullableFlowState.MaybeDefault);
			}
			if (flag)
			{
				return TypeWithState.Create(result2.Type, NullableFlowState.NotNull);
			}
			return result2;
		}
		static bool hasConditionalPostCondition(FlowAnalysisAnnotations annotations)
		{
			if (!(((annotations & FlowAnalysisAnnotations.MaybeNullWhenTrue) != 0) ^ ((annotations & FlowAnalysisAnnotations.MaybeNullWhenFalse) != 0)))
			{
				return ((annotations & FlowAnalysisAnnotations.NotNullWhenTrue) != 0) ^ ((annotations & FlowAnalysisAnnotations.NotNullWhenFalse) != 0);
			}
			return true;
		}
		FlowAnalysisAnnotations notNullBasedOnParameters(FlowAnalysisAnnotations result2, ArrayBuilder<ParameterSymbol>? arrayBuilder, ParameterSymbol parameterSymbol)
		{
			if (!IsAnalyzingAttribute && arrayBuilder != null)
			{
				ImmutableHashSet<string> notNullIfParameterNotNull = parameterSymbol.NotNullIfParameterNotNull;
				if (!notNullIfParameterNotNull.IsEmpty)
				{
					foreach (ParameterSymbol item in arrayBuilder)
					{
						if (notNullIfParameterNotNull.Contains(item.Name))
						{
							return FlowAnalysisAnnotations.NotNull;
						}
					}
				}
			}
			return result2;
		}
		void trackNullableStateForAssignment(BoundExpression parameterValue, TypeWithAnnotations targetType, int targetSlot, TypeWithState parameterWithState, bool isSuppressed, FlowAnalysisAnnotations annotations)
		{
			if (!IsConditionalState && !hasConditionalPostCondition(annotations))
			{
				TrackNullableStateForAssignment(parameterValue, targetType, targetSlot, parameterWithState.WithSuppression(isSuppressed));
			}
			else
			{
				Split();
				LocalState state = StateWhenFalse.Clone();
				SetState(StateWhenTrue);
				TrackNullableStateForAssignment(parameterValue, targetType, targetSlot, applyPostConditionsWhenTrue(parameterWithState, annotations).WithSuppression(isSuppressed));
				LocalState whenTrue = State.Clone();
				SetState(state);
				TrackNullableStateForAssignment(parameterValue, targetType, targetSlot, applyPostConditionsWhenFalse(parameterWithState, annotations).WithSuppression(isSuppressed));
				SetConditionalState(whenTrue, State);
			}
		}
	}

	private void LearnFromPostConditions(BoundExpression argument, FlowAnalysisAnnotations parameterAnnotations)
	{
		bool flag = (parameterAnnotations & FlowAnalysisAnnotations.NotNullWhenTrue) != 0;
		bool flag2 = (parameterAnnotations & FlowAnalysisAnnotations.NotNullWhenFalse) != 0;
		bool flag3 = (parameterAnnotations & FlowAnalysisAnnotations.MaybeNullWhenTrue) != 0;
		bool flag4 = (parameterAnnotations & FlowAnalysisAnnotations.MaybeNullWhenFalse) != 0;
		if ((flag3 & flag4) && !IsConditionalState && !(flag & flag2))
		{
			LearnFromNullTest(argument, ref State);
		}
		else if ((flag & flag2) && !IsConditionalState && !(flag3 | flag4))
		{
			LearnFromNonNullTest(argument, ref State);
		}
		else if (flag | flag2 | flag3 | flag4)
		{
			Split();
			if (flag)
			{
				LearnFromNonNullTest(argument, ref StateWhenTrue);
			}
			if (flag2)
			{
				LearnFromNonNullTest(argument, ref StateWhenFalse);
			}
			if (flag3)
			{
				LearnFromNullTest(argument, ref StateWhenTrue);
			}
			if (flag4)
			{
				LearnFromNullTest(argument, ref StateWhenFalse);
			}
		}
	}

	private (ImmutableArray<BoundExpression> arguments, ImmutableArray<Conversion> conversions) RemoveArgumentConversions(ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> refKindsOpt)
	{
		int length = arguments.Length;
		ImmutableArray<Conversion> item = default(ImmutableArray<Conversion>);
		if (length > 0)
		{
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(length);
			ArrayBuilder<Conversion> instance2 = ArrayBuilder<Conversion>.GetInstance(length);
			bool flag = false;
			for (int i = 0; i < length; i++)
			{
				RefKind refKind = AbstractFlowPass<LocalState, LocalFunctionState>.GetRefKind(refKindsOpt, i);
				BoundExpression boundExpression = arguments[i];
				Conversion item2 = Conversion.Identity;
				if (refKind == RefKind.None)
				{
					BoundExpression boundExpression2 = boundExpression;
					(boundExpression, item2) = RemoveConversion(boundExpression, includeExplicitConversions: false);
					if (boundExpression != boundExpression2)
					{
						SnapshotWalkerThroughConversionGroup(boundExpression2, boundExpression);
						flag = true;
					}
				}
				instance.Add(boundExpression);
				instance2.Add(item2);
			}
			if (flag)
			{
				arguments = instance.ToImmutable();
				item = instance2.ToImmutable();
			}
			instance.Free();
			instance2.Free();
		}
		return (arguments: arguments, conversions: item);
	}

	private static VariableState GetVariableState(Variables variables, LocalState localState)
	{
		return new VariableState(variables.CreateSnapshot(), localState.CreateSnapshot());
	}

	private (ParameterSymbol? Parameter, TypeWithAnnotations Type, FlowAnalysisAnnotations Annotations, bool isExpandedParamsArgument) GetCorrespondingParameter(int argumentOrdinal, ImmutableArray<ParameterSymbol> parametersOpt, ImmutableArray<int> argsToParamsOpt, bool expanded, ref TypeWithAnnotations paramsIterationType)
	{
		if (parametersOpt.IsDefault)
		{
			return default((ParameterSymbol, TypeWithAnnotations, FlowAnalysisAnnotations, bool));
		}
		ParameterSymbol correspondingParameter = Binder.GetCorrespondingParameter(argumentOrdinal, parametersOpt, argsToParamsOpt, expanded);
		if ((object)correspondingParameter == null)
		{
			return default((ParameterSymbol, TypeWithAnnotations, FlowAnalysisAnnotations, bool));
		}
		TypeWithAnnotations typeWithAnnotations = correspondingParameter.TypeWithAnnotations;
		if (expanded)
		{
			if ((object)correspondingParameter == parametersOpt[parametersOpt.Length - 1])
			{
				if (!paramsIterationType.HasType)
				{
					OverloadResolution.TryInferParamsCollectionIterationType(_binder, typeWithAnnotations.Type, out paramsIterationType);
				}
				return (Parameter: correspondingParameter, Type: paramsIterationType, Annotations: FlowAnalysisAnnotations.None, isExpandedParamsArgument: true);
			}
		}
		return (Parameter: correspondingParameter, Type: typeWithAnnotations, Annotations: GetParameterAnnotations(correspondingParameter), isExpandedParamsArgument: false);
	}

	private TMember InferMemberTypeArguments<TMember>(TMember member, ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> argumentRefKindsOpt, ImmutableArray<int> argsToParamsOpt, bool expanded) where TMember : Symbol
	{
		Symbol symbol = (member.IsExtensionBlockMember() ? member.OriginalDefinition : member.ConstructedFrom());
		ArrayBuilder<RefKind> instance = ArrayBuilder<RefKind>.GetInstance();
		if (argumentRefKindsOpt != null)
		{
			instance.AddRange(argumentRefKindsOpt);
		}
		OverloadResolution.GetEffectiveParameterTypes(symbol, arguments.Length, argsToParamsOpt, instance, isMethodGroupConversion: false, allowRefOmittedArguments: true, _binder, expanded, out var parameterTypes, out var parameterRefKinds);
		instance.Free();
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
		ImmutableArray<TypeParameterSymbol> typeParametersIncludingExtension = symbol.GetTypeParametersIncludingExtension();
		Dictionary<TypeParameterSymbol, int> ordinals = symbol.MakeAdjustedTypeParameterOrdinalsIfNeeded(typeParametersIncludingExtension);
		MethodTypeInferenceResult methodTypeInferenceResult = MethodTypeInferrer.Infer(_binder, _conversions, typeParametersIncludingExtension, symbol.ContainingType, parameterTypes, parameterRefKinds, arguments, ref useSiteInfo, new MethodInferenceExtensions(this), ordinals);
		if (!methodTypeInferenceResult.Success)
		{
			return member;
		}
		return (TMember)symbol.ConstructIncludingExtension(methodTypeInferenceResult.InferredTypeArguments);
	}

	private ImmutableArray<BoundExpression> GetArgumentsForMethodTypeInference(ImmutableArray<VisitResult> argumentResults, ImmutableArray<BoundExpression> arguments)
	{
		int length = argumentResults.Length;
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(length);
		for (int i = 0; i < length; i++)
		{
			VisitResult visitResult = argumentResults[i];
			instance.Add(getArgumentForMethodTypeInference(arguments[i], visitResult));
		}
		return instance.ToImmutableAndFree();
		BoundExpression getArgumentForMethodTypeInference(BoundExpression argument, VisitResult visitResult2)
		{
			Optional<LocalState> stateForLambda = visitResult2.StateForLambda;
			if (argument.Kind == BoundKind.Lambda)
			{
				return getUnboundLambda((BoundLambda)argument, GetVariableState(_variables, stateForLambda.Value), _getterNullResilienceData);
			}
			if (argument.Kind == BoundKind.CollectionExpression)
			{
				VisitResult[] nestedVisitResults = visitResult2.NestedVisitResults;
				BoundCollectionExpression boundCollectionExpression = (BoundCollectionExpression)argument;
				ArrayBuilder<BoundNode> instance2 = ArrayBuilder<BoundNode>.GetInstance(nestedVisitResults.Length);
				for (int j = 0; j < nestedVisitResults.Length; j++)
				{
					if (boundCollectionExpression.Elements[j] is BoundExpression expr)
					{
						BoundExpression item = RemoveConversion(expr, includeExplicitConversions: false).expression;
						instance2.Add(getArgumentForMethodTypeInference(item, nestedVisitResults[j]));
					}
					else
					{
						instance2.Add(boundCollectionExpression.Elements[j]);
					}
				}
				return new BoundUnconvertedCollectionExpression(boundCollectionExpression.Syntax, instance2.ToImmutableAndFree())
				{
					WasCompilerGenerated = true
				};
			}
			TypeWithAnnotations typeWithAnnotations = visitResult2.RValueType.ToTypeWithAnnotations(compilation);
			if (!typeWithAnnotations.HasType)
			{
				return argument;
			}
			if (argument is BoundLocal { DeclarationKind: BoundLocalDeclarationKind.WithInferredType } || IsTargetTypedExpression(argument))
			{
				return new BoundExpressionWithNullability(argument.Syntax, argument, NullableAnnotation.Oblivious, null);
			}
			return new BoundExpressionWithNullability(argument.Syntax, argument, typeWithAnnotations.NullableAnnotation, typeWithAnnotations.Type);
		}
		static UnboundLambda getUnboundLambda(BoundLambda expr, VariableState variableState, GetterNullResilienceData? getterNullResilienceData)
		{
			return expr.UnboundLambda.WithNullabilityInfo(variableState, getterNullResilienceData);
		}
	}

	private void CheckMethodConstraints(SyntaxNode syntax, MethodSymbol method)
	{
		if (_disableDiagnostics)
		{
			return;
		}
		ArrayBuilder<TypeParameterDiagnosticInfo> instance = ArrayBuilder<TypeParameterDiagnosticInfo>.GetInstance();
		ArrayBuilder<TypeParameterDiagnosticInfo> instance2 = ArrayBuilder<TypeParameterDiagnosticInfo>.GetInstance();
		ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder = null;
		ConstraintsHelper.CheckMethodConstraints(method, new ConstraintsHelper.CheckConstraintsArgs(compilation, _conversions, includeNullability: true, NoLocation.Singleton, null, CompoundUseSiteInfo<AssemblySymbol>.Discarded), instance, instance2, ref useSiteDiagnosticsBuilder);
		foreach (TypeParameterDiagnosticInfo item in instance2)
		{
			if (item.UseSiteInfo.DiagnosticInfo != null)
			{
				base.Diagnostics.Add(item.UseSiteInfo.DiagnosticInfo, syntax.Location);
			}
		}
		useSiteDiagnosticsBuilder?.Free();
		instance2.Free();
		instance.Free();
	}

	private void CheckExtensionConstraints(SyntaxNode syntax, NamedTypeSymbol extension)
	{
		if (_disableDiagnostics)
		{
			return;
		}
		ArrayBuilder<TypeParameterDiagnosticInfo> instance = ArrayBuilder<TypeParameterDiagnosticInfo>.GetInstance();
		ArrayBuilder<TypeParameterDiagnosticInfo> instance2 = ArrayBuilder<TypeParameterDiagnosticInfo>.GetInstance();
		ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder = null;
		extension.CheckConstraints(new ConstraintsHelper.CheckConstraintsArgs(compilation, _conversions, includeNullability: false, NoLocation.Singleton, null, CompoundUseSiteInfo<AssemblySymbol>.Discarded), extension.TypeSubstitution, extension.TypeParameters, extension.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics, instance, instance2, ref useSiteDiagnosticsBuilder);
		foreach (TypeParameterDiagnosticInfo item in instance2)
		{
			if (item.UseSiteInfo.DiagnosticInfo != null)
			{
				base.Diagnostics.Add(item.UseSiteInfo.DiagnosticInfo, syntax.Location);
			}
		}
		useSiteDiagnosticsBuilder?.Free();
		instance2.Free();
		instance.Free();
	}

	private static (BoundExpression expression, Conversion conversion) RemoveConversion(BoundExpression expr, bool includeExplicitConversions)
	{
		ConversionGroup conversionGroup = null;
		while (expr.Kind == BoundKind.Conversion)
		{
			BoundConversion boundConversion = (BoundConversion)expr;
			if (conversionGroup != boundConversion.ConversionGroupOpt && conversionGroup != null)
			{
				break;
			}
			conversionGroup = boundConversion.ConversionGroupOpt;
			if (!includeExplicitConversions && conversionGroup != null && conversionGroup.IsExplicitConversion)
			{
				return (expression: expr, conversion: Conversion.Identity);
			}
			expr = boundConversion.Operand;
			if (conversionGroup == null)
			{
				return (expression: expr, conversion: boundConversion.Conversion);
			}
		}
		return (expression: expr, conversion: conversionGroup?.Conversion ?? Conversion.Identity);
	}

	private Conversion GenerateConversionForConditionalOperator(BoundExpression sourceExpression, TypeSymbol? sourceType, TypeSymbol destinationType, bool reportMismatch, bool isChecked)
	{
		Conversion result = GenerateConversion(_conversions, sourceExpression, sourceType, destinationType, fromExplicitCast: false, extensionMethodThisArgument: false, isChecked);
		if ((!result.Exists & reportMismatch) && !sourceExpression.IsSuppressed)
		{
			ReportNullabilityMismatchInAssignment(sourceExpression.Syntax, GetTypeAsDiagnosticArgument(sourceType), destinationType);
		}
		return result;
	}

	private Conversion GenerateConversion(Conversions conversions, BoundExpression? sourceExpression, TypeSymbol? sourceType, TypeSymbol destinationType, bool fromExplicitCast, bool extensionMethodThisArgument, bool isChecked)
	{
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
		bool flag = (object)sourceType == null || UseExpressionForConversion(sourceExpression);
		if (extensionMethodThisArgument)
		{
			return conversions.ClassifyImplicitExtensionMethodThisArgConversion(flag ? sourceExpression : null, sourceType, destinationType, ref useSiteInfo, isMethodGroupConversion: false);
		}
		if (!flag)
		{
			if (!fromExplicitCast)
			{
				return conversions.ClassifyImplicitConversionFromType(sourceType, destinationType, ref useSiteInfo);
			}
			return conversions.ClassifyConversionFromType(sourceType, destinationType, isChecked, ref useSiteInfo, forCast: true);
		}
		if (!fromExplicitCast)
		{
			return conversions.ClassifyImplicitConversionFromExpression(sourceExpression, destinationType, ref useSiteInfo);
		}
		return conversions.ClassifyConversionFromExpression(sourceExpression, destinationType, isChecked, ref useSiteInfo, forCast: true);
	}

	private bool UseExpressionForConversion([NotNullWhen(true)] BoundExpression? value)
	{
		if (value == null)
		{
			return false;
		}
		if ((object)value.Type == null || value.Type.IsDynamic() || value.ConstantValueOpt != null)
		{
			return true;
		}
		if (value.Kind == BoundKind.InterpolatedString)
		{
			return true;
		}
		if (!_binder.InAttributeArgument && !_binder.InParameterDefaultValue && value.Type.HasInlineArrayAttribute(out var _) && (object)value.Type.TryGetInlineArrayElementField() != null)
		{
			return true;
		}
		return false;
	}

	private TypeWithState GetAdjustedResult(TypeWithState type, int slot)
	{
		if (slot > 0)
		{
			NullableFlowState state = GetState(ref State, slot);
			return TypeWithState.Create(type.Type, state);
		}
		return type;
	}

	private static Symbol AsMemberOfType(TypeSymbol? type, Symbol symbol)
	{
		NamedTypeSymbol namedTypeSymbol = type as NamedTypeSymbol;
		if ((object)namedTypeSymbol == null || namedTypeSymbol.IsErrorType() || symbol is ErrorMethodSymbol)
		{
			return symbol;
		}
		if (symbol.Kind == SymbolKind.Method && ((MethodSymbol)symbol).MethodKind == MethodKind.LocalFunction)
		{
			return symbol;
		}
		if ((symbol is TupleElementFieldSymbol || symbol is TupleErrorFieldSymbol) ? true : false)
		{
			return symbol.SymbolAsMember(namedTypeSymbol);
		}
		NamedTypeSymbol symbolContainer = symbol.ContainingType;
		if (symbolContainer.IsAnonymousType)
		{
			int? num = ((symbol.Kind == SymbolKind.Property) ? symbol.MemberIndexOpt : ((int?)null));
			if (!num.HasValue)
			{
				return symbol;
			}
			return AnonymousTypeManager.GetAnonymousTypeProperty(namedTypeSymbol, num.GetValueOrDefault());
		}
		if (!symbolContainer.IsGenericType)
		{
			return symbol;
		}
		if (!namedTypeSymbol.IsGenericType)
		{
			return symbol;
		}
		if (symbolContainer.IsInterface)
		{
			if (tryAsMemberOfSingleType(namedTypeSymbol, out var result))
			{
				return result;
			}
			foreach (NamedTypeSymbol allInterfacesNoUseSiteDiagnostic in namedTypeSymbol.AllInterfacesNoUseSiteDiagnostics)
			{
				if (tryAsMemberOfSingleType(allInterfacesNoUseSiteDiagnostic, out result))
				{
					return result;
				}
			}
		}
		else
		{
			do
			{
				if (tryAsMemberOfSingleType(namedTypeSymbol, out var result2))
				{
					return result2;
				}
				namedTypeSymbol = namedTypeSymbol.BaseTypeNoUseSiteDiagnostics;
			}
			while ((object)namedTypeSymbol != null);
		}
		return symbol;
		bool tryAsMemberOfSingleType(NamedTypeSymbol singleType, [NotNullWhen(true)] out Symbol? reference)
		{
			if (!singleType.Equals(symbolContainer, TypeCompareKind.AllIgnoreOptions))
			{
				reference = null;
				return false;
			}
			Symbol originalDefinition = symbol.OriginalDefinition;
			reference = originalDefinition.SymbolAsMember(singleType);
			if (reference is MethodSymbol { IsGenericMethod: not false } methodSymbol)
			{
				reference = methodSymbol.Construct(((MethodSymbol)symbol).TypeArgumentsWithAnnotations);
			}
			return true;
		}
	}

	public override BoundNode? VisitConversion(BoundConversion node)
	{
		TypeWithAnnotations typeWithAnnotations = node.ConversionGroupOpt?.ExplicitType ?? default(TypeWithAnnotations);
		bool hasType = typeWithAnnotations.HasType;
		TypeWithAnnotations typeWithAnnotations2 = (hasType ? typeWithAnnotations : TypeWithAnnotations.Create(node.Type));
		var (boundExpression, conversion) = RemoveConversion(node, includeExplicitConversions: true);
		SnapshotWalkerThroughConversionGroup(node, boundExpression);
		if (TypeAllowsConditionalState(typeWithAnnotations2.Type) && TypeAllowsConditionalState(boundExpression.Type))
		{
			Visit(boundExpression);
		}
		else
		{
			VisitRvalue(boundExpression);
		}
		TypeWithAnnotations targetTypeWithNullability = typeWithAnnotations2;
		TypeWithState resultType = ResultType;
		bool trackMembers = !IsConditionalState;
		SetResultType(node, VisitConversion(node, boundExpression, conversion, targetTypeWithNullability, resultType, checkConversion: true, hasType, hasType, AssignmentKind.Assignment, null, hasType, reportRemainingWarnings: true, isSuppressed: false, extensionMethodThisArgument: false, default(Optional<LocalState>), trackMembers));
		return null;
	}

	private TypeWithState VisitOptionalImplicitConversion(BoundExpression expr, TypeWithAnnotations targetTypeOpt, bool useLegacyWarnings, bool trackMembers, AssignmentKind assignmentKind)
	{
		if (!targetTypeOpt.HasType)
		{
			return VisitRvalueWithState(expr);
		}
		return VisitOptionalImplicitConversion(expr, targetTypeOpt, useLegacyWarnings, trackMembers, assignmentKind, delayCompletionForTargetType: false).resultType;
	}

	private (TypeWithState resultType, Func<TypeWithAnnotations, TypeWithState>? completion) VisitOptionalImplicitConversion(BoundExpression expr, TypeWithAnnotations targetTypeOpt, bool useLegacyWarnings, bool trackMembers, AssignmentKind assignmentKind, bool delayCompletionForTargetType)
	{
		var (boundExpression, conversion) = RemoveConversion(expr, includeExplicitConversions: false);
		SnapshotWalkerThroughConversionGroup(expr, boundExpression);
		TypeWithState operandType = VisitRvalueWithState(boundExpression);
		return visitConversion(expr, targetTypeOpt, useLegacyWarnings, trackMembers, assignmentKind, boundExpression, conversion, operandType, delayCompletionForTargetType);
		(TypeWithState resultType, Func<TypeWithAnnotations, TypeWithState>? completion) visitConversion(BoundExpression boundExpression2, TypeWithAnnotations typeWithAnnotations, bool useLegacyWarnings2, bool flag2, AssignmentKind assignmentKind2, BoundExpression operand, Conversion conversion2, TypeWithState operandType2, bool flag)
		{
			if (flag)
			{
				return (resultType: TypeWithState.Create(typeWithAnnotations), completion: visitConversionAsContinuation(boundExpression2, useLegacyWarnings2, flag2, assignmentKind2, operand, conversion2, operandType2));
			}
			bool reportRemainingWarnings = !conversion2.IsExplicit;
			BoundConversion? conversionIfApplicable = GetConversionIfApplicable(boundExpression2, operand);
			Conversion conversion3 = conversion2;
			bool trackMembers2 = flag2;
			return (resultType: VisitConversion(conversionIfApplicable, operand, conversion3, typeWithAnnotations, operandType2, checkConversion: true, fromExplicitCast: false, useLegacyWarnings2, assignmentKind2, null, reportTopLevelWarnings: true, reportRemainingWarnings, isSuppressed: false, extensionMethodThisArgument: false, default(Optional<LocalState>), trackMembers2), completion: null);
		}
		Func<TypeWithAnnotations, TypeWithState> visitConversionAsContinuation(BoundExpression expr2, bool useLegacyWarnings2, bool trackMembers2, AssignmentKind assignmentKind2, BoundExpression operand, Conversion conversion2, TypeWithState operandType2)
		{
			return (TypeWithAnnotations targetTypeOpt2) => visitConversion(expr2, targetTypeOpt2, useLegacyWarnings2, trackMembers2, assignmentKind2, operand, conversion2, operandType2, delayCompletionForTargetType: false).resultType;
		}
	}

	private static bool AreNullableAndUnderlyingTypes([NotNullWhen(true)] TypeSymbol? nullableTypeOpt, [NotNullWhen(true)] TypeSymbol? underlyingTypeOpt, out TypeWithAnnotations underlyingTypeWithAnnotations)
	{
		if ((object)nullableTypeOpt != null && nullableTypeOpt.IsNullableType() && (object)underlyingTypeOpt != null && !underlyingTypeOpt.IsNullableType())
		{
			TypeWithAnnotations nullableUnderlyingTypeWithAnnotations = nullableTypeOpt.GetNullableUnderlyingTypeWithAnnotations();
			if (nullableUnderlyingTypeWithAnnotations.Type.Equals(underlyingTypeOpt, TypeCompareKind.AllIgnoreOptions))
			{
				underlyingTypeWithAnnotations = nullableUnderlyingTypeWithAnnotations;
				return true;
			}
		}
		underlyingTypeWithAnnotations = default(TypeWithAnnotations);
		return false;
	}

	public override BoundNode? VisitTupleLiteral(BoundTupleLiteral node)
	{
		VisitTupleExpression(node);
		return null;
	}

	public override BoundNode? VisitConvertedTupleLiteral(BoundConvertedTupleLiteral node)
	{
		LocalState state = State.Clone();
		VisitWithoutDiagnostics(node.SourceTuple);
		SetState(state);
		VisitTupleExpression(node);
		return null;
	}

	private void VisitTupleExpression(BoundTupleExpression node)
	{
		ImmutableArray<BoundExpression> arguments = node.Arguments;
		ImmutableArray<TypeWithState> immutableArray = arguments.SelectAsArray((BoundExpression a, NullableWalker w) => w.VisitRvalueWithState(a), this);
		ImmutableArray<TypeWithAnnotations> newElementTypes = immutableArray.SelectAsArray((TypeWithState a) => a.ToTypeWithAnnotations(compilation));
		NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)node.Type;
		if ((object)namedTypeSymbol == null)
		{
			SetResultType(node, TypeWithState.Create(null, NullableFlowState.NotNull));
			return;
		}
		int orCreatePlaceholderSlot = GetOrCreatePlaceholderSlot(node);
		if (orCreatePlaceholderSlot > 0)
		{
			SetState(ref State, orCreatePlaceholderSlot, NullableFlowState.NotNull);
			TrackNullableStateOfTupleElements(orCreatePlaceholderSlot, namedTypeSymbol, arguments, immutableArray, default(ImmutableArray<int>), useRestField: false);
		}
		namedTypeSymbol = namedTypeSymbol.WithElementTypes(newElementTypes);
		if (!_disableDiagnostics)
		{
			ImmutableArray<Location> elementLocations = namedTypeSymbol.TupleElements.SelectAsArray((FieldSymbol element, Location location) => element.TryGetFirstLocation() ?? location, node.Syntax.Location);
			BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: true, withDependencies: false);
			namedTypeSymbol.CheckConstraints(new ConstraintsHelper.CheckConstraintsArgs(compilation, _conversions, includeNullability: true, node.Syntax.Location, null), node.Syntax, elementLocations, instance);
			base.Diagnostics.AddRange(instance.DiagnosticBag);
			instance.Free();
		}
		SetResultType(node, TypeWithState.Create(namedTypeSymbol, NullableFlowState.NotNull));
	}

	private void TrackNullableStateOfTupleElements(int slot, NamedTypeSymbol tupleType, ImmutableArray<BoundExpression> values, ImmutableArray<TypeWithState> types, ImmutableArray<int> argsToParamsOpt, bool useRestField)
	{
		if (slot > 0)
		{
			ImmutableArray<FieldSymbol> tupleElements = tupleType.TupleElements;
			int num = values.Length;
			if (useRestField)
			{
				num = Math.Min(num, 7);
			}
			for (int i = 0; i < num; i++)
			{
				int index = getArgumentOrdinalFromParameterOrdinal(i);
				trackState(values[index], tupleElements[i], types[index]);
			}
			if (useRestField && values.Length == 8 && tupleType.GetMembers("Rest").FirstOrDefault() is FieldSymbol field)
			{
				int index2 = getArgumentOrdinalFromParameterOrdinal(7);
				trackState(values[index2], field, types[index2]);
			}
		}
		int getArgumentOrdinalFromParameterOrdinal(int parameterOrdinal)
		{
			if (!argsToParamsOpt.IsDefault)
			{
				return argsToParamsOpt.IndexOf(parameterOrdinal);
			}
			return parameterOrdinal;
		}
		void trackState(BoundExpression value, FieldSymbol fieldSymbol, TypeWithState valueType)
		{
			int orCreateSlot = GetOrCreateSlot(fieldSymbol, slot);
			TrackNullableStateForAssignment(value, fieldSymbol.TypeWithAnnotations, orCreateSlot, valueType, MakeSlot(value));
		}
	}

	private void TrackNullableStateOfNullableValue(int containingSlot, TypeSymbol containingType, BoundExpression? value, TypeWithState valueType, int valueSlot)
	{
		int nullableOfTValueSlot = GetNullableOfTValueSlot(containingType, containingSlot, out Symbol valueProperty);
		if (nullableOfTValueSlot > 0)
		{
			TrackNullableStateForAssignment(value, GetTypeOrReturnTypeWithAnnotations(valueProperty), nullableOfTValueSlot, valueType, valueSlot);
		}
	}

	private void TrackNullableStateOfTupleConversion(BoundConversion? conversionOpt, BoundExpression convertedNode, Conversion conversion, TypeSymbol targetType, TypeSymbol operandType, int slot, int valueSlot, AssignmentKind assignmentKind, ParameterSymbol? parameterOpt, bool reportWarnings)
	{
		if (operandType is NamedTypeSymbol { IsTupleType: not false } namedTypeSymbol)
		{
			ImmutableArray<Conversion> underlyingConversions = conversion.UnderlyingConversions;
			ImmutableArray<FieldSymbol> tupleElements = ((NamedTypeSymbol)targetType).TupleElements;
			ImmutableArray<FieldSymbol> tupleElements2 = namedTypeSymbol.TupleElements;
			int length = tupleElements2.Length;
			for (int i = 0; i < length; i++)
			{
				trackConvertedValue(tupleElements[i], underlyingConversions[i], tupleElements2[i]);
			}
		}
		void trackConvertedValue(FieldSymbol targetField, Conversion conversion2, FieldSymbol valueField)
		{
			switch (conversion2.Kind)
			{
			case ConversionKind.Identity:
			case ConversionKind.NullLiteral:
			case ConversionKind.ImplicitReference:
			case ConversionKind.Boxing:
			case ConversionKind.ExplicitReference:
			case ConversionKind.Unboxing:
			case ConversionKind.DefaultLiteral:
				InheritNullableStateOfMember(slot, valueSlot, valueField, isDefaultValue: false, slot);
				break;
			case ConversionKind.ImplicitTupleLiteral:
			case ConversionKind.ImplicitTuple:
			case ConversionKind.ExplicitTupleLiteral:
			case ConversionKind.ExplicitTuple:
			{
				int orCreateSlot4 = GetOrCreateSlot(targetField, slot);
				if (orCreateSlot4 > 0)
				{
					SetState(ref State, orCreateSlot4, NullableFlowState.NotNull);
					int orCreateSlot5 = GetOrCreateSlot(valueField, valueSlot);
					if (orCreateSlot5 > 0)
					{
						TrackNullableStateOfTupleConversion(conversionOpt, convertedNode, conversion2, targetField.Type, valueField.Type, orCreateSlot4, orCreateSlot5, assignmentKind, parameterOpt, reportWarnings);
					}
				}
				break;
			}
			case ConversionKind.ImplicitNullable:
			case ConversionKind.ExplicitNullable:
			{
				if (AreNullableAndUnderlyingTypes(targetField.Type, valueField.Type, out var _))
				{
					int orCreateSlot2 = GetOrCreateSlot(targetField, slot);
					if (orCreateSlot2 > 0)
					{
						SetState(ref State, orCreateSlot2, NullableFlowState.NotNull);
						int orCreateSlot3 = GetOrCreateSlot(valueField, valueSlot);
						if (orCreateSlot3 > 0)
						{
							TrackNullableStateOfNullableValue(orCreateSlot2, targetField.Type, null, valueField.TypeWithAnnotations.ToTypeWithState(), orCreateSlot3);
						}
					}
				}
				break;
			}
			case ConversionKind.ImplicitUserDefined:
			case ConversionKind.ExplicitUserDefined:
			{
				TypeWithState typeWithState = VisitUserDefinedConversion(conversionOpt, convertedNode, conversion2, targetField.TypeWithAnnotations, valueField.TypeWithAnnotations.ToTypeWithState(), useLegacyWarnings: false, assignmentKind, parameterOpt, reportWarnings, reportWarnings, (conversionOpt ?? convertedNode).Syntax.GetLocation());
				int orCreateSlot = GetOrCreateSlot(targetField, slot);
				if (orCreateSlot > 0)
				{
					SetState(ref State, orCreateSlot, typeWithState.State);
				}
				break;
			}
			}
		}
	}

	public override BoundNode? VisitTupleBinaryOperator(BoundTupleBinaryOperator node)
	{
		base.VisitTupleBinaryOperator(node);
		SetNotNullResult(node);
		return null;
	}

	private void ReportNullabilityMismatchWithTargetDelegate(Location location, TypeSymbol targetType, MethodSymbol targetInvokeMethod, MethodSymbol sourceInvokeMethod, bool invokedAsExtensionMethod)
	{
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: true, withDependencies: false);
		SourceMemberContainerTypeSymbol.CheckValidNullableMethodOverride(compilation, targetInvokeMethod, sourceInvokeMethod, instance, reportBadDelegateReturn, reportBadDelegateParameter, (targetType, location), invokedAsExtensionMethod);
		base.Diagnostics.AddRange(instance.DiagnosticBag);
		instance.Free();
		void reportBadDelegateParameter(BindingDiagnosticBag bag, MethodSymbol methodSymbol, MethodSymbol methodSymbol2, ParameterSymbol parameter, bool topLevel, (TypeSymbol targetType, Location location) arg)
		{
			ReportDiagnostic(ErrorCode.WRN_NullabilityMismatchInParameterTypeOfTargetDelegate, arg.location, GetParameterAsDiagnosticArgument(parameter), GetContainingSymbolAsDiagnosticArgument(parameter), arg.targetType);
		}
		void reportBadDelegateReturn(BindingDiagnosticBag bag, MethodSymbol methodSymbol, MethodSymbol symbol, bool topLevel, (TypeSymbol targetType, Location location) arg)
		{
			ReportDiagnostic(ErrorCode.WRN_NullabilityMismatchInReturnTypeOfTargetDelegate, arg.location, new FormattedSymbol(symbol, SymbolDisplayFormat.MinimallyQualifiedFormat), arg.targetType);
		}
	}

	private void ReportNullabilityMismatchWithTargetDelegate(Location location, NamedTypeSymbol delegateType, BoundLambda lambda)
	{
		MethodSymbol delegateInvokeMethod = delegateType.DelegateInvokeMethod;
		LambdaSymbol lambdaSymbol = (LambdaSymbol)lambda.Symbol;
		UnboundLambda unboundLambda = lambda.UnboundLambda;
		if ((object)delegateInvokeMethod != null && delegateInvokeMethod.ParameterCount == lambdaSymbol.ParameterCount)
		{
			if (lambda.Syntax is LambdaExpressionSyntax { SpanStart: var spanStart } lambdaExpressionSyntax)
			{
				location = Location.Create(lambdaExpressionSyntax.SyntaxTree, new TextSpan(spanStart, lambdaExpressionSyntax.ArrowToken.Span.End - spanStart));
			}
			BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: true, withDependencies: false);
			if (SourceMemberContainerTypeSymbol.CheckValidNullableMethodOverride(compilation, delegateInvokeMethod, lambdaSymbol, instance, reportBadDelegateReturn, reportBadDelegateParameter, location))
			{
				base.Diagnostics.AddRange(instance.DiagnosticBag);
				instance.Free();
			}
			else
			{
				SourceMemberContainerTypeSymbol.CheckValidNullableMethodOverride(compilation, lambdaSymbol, delegateInvokeMethod, instance, reportBadDelegateReturn, reportBadDelegateParameter, location);
				base.Diagnostics.AddRange(instance.DiagnosticBag);
				instance.Free();
			}
		}
		void reportBadDelegateParameter(BindingDiagnosticBag bag, MethodSymbol sourceInvokeMethod, MethodSymbol targetInvokeMethod, ParameterSymbol parameterSymbol, bool topLevel, Location location2)
		{
			if (unboundLambda.HasSignature)
			{
				ReportDiagnostic(ErrorCode.WRN_NullabilityMismatchInParameterTypeOfTargetDelegate, location2, unboundLambda.ParameterName(parameterSymbol.Ordinal), unboundLambda.MessageID.Localize(), delegateType);
			}
		}
		void reportBadDelegateReturn(BindingDiagnosticBag bag, MethodSymbol targetInvokeMethod, MethodSymbol sourceInvokeMethod, bool topLevel, Location location2)
		{
			ReportDiagnostic(ErrorCode.WRN_NullabilityMismatchInReturnTypeOfTargetDelegate, location2, unboundLambda.MessageID.Localize(), delegateType);
		}
	}

	private static BoundConversion? GetConversionIfApplicable(BoundExpression? conversionOpt, BoundExpression convertedNode)
	{
		if (conversionOpt != convertedNode)
		{
			return (BoundConversion)conversionOpt;
		}
		return null;
	}

	private TypeWithState VisitConversion(BoundConversion? conversionOpt, BoundExpression conversionOperand, Conversion conversion, TypeWithAnnotations targetTypeWithNullability, TypeWithState operandType, bool checkConversion, bool fromExplicitCast, bool useLegacyWarnings, AssignmentKind assignmentKind, ParameterSymbol? parameterOpt = null, bool reportTopLevelWarnings = true, bool reportRemainingWarnings = true, bool isSuppressed = false, bool extensionMethodThisArgument = false, Optional<LocalState> stateForLambda = default(Optional<LocalState>), bool trackMembers = false, Location? diagnosticLocation = null, ArrayBuilder<VisitResult>? previousArgumentConversionResults = null)
	{
		if (IsTargetTypedExpression(conversionOperand) && TargetTypedAnalysisCompletion.TryGetValue(conversionOperand, out Func<TypeWithAnnotations, TypeWithState> value))
		{
			TargetTypedAnalysisCompletion.Remove(conversionOperand);
			if (conversionOperand is BoundObjectCreationExpressionBase && targetTypeWithNullability.IsNullableType())
			{
				operandType = value(targetTypeWithNullability.Type.GetNullableUnderlyingTypeWithAnnotations());
				conversion = Conversion.MakeNullableConversion(ConversionKind.ImplicitNullable, Conversion.Identity);
			}
			else
			{
				operandType = value(targetTypeWithNullability);
			}
		}
		NullableFlowState resultState = NullableFlowState.NotNull;
		bool flag = true;
		if (isSuppressed || conversionOperand.IsSuppressed)
		{
			reportTopLevelWarnings = false;
			reportRemainingWarnings = false;
			isSuppressed = true;
		}
		TypeSymbol type = targetTypeWithNullability.Type;
		switch (conversion.Kind)
		{
		case ConversionKind.MethodGroup:
		{
			BoundMethodGroup boundMethodGroup = conversionOperand as BoundMethodGroup;
			(MethodSymbol invokeSignature, ImmutableArray<ParameterSymbol>) tuple = getDelegateOrFunctionPointerInfo(type);
			MethodSymbol item = tuple.invokeSignature;
			ImmutableArray<ParameterSymbol> item2 = tuple.Item2;
			MethodSymbol methodSymbol = conversion.Method;
			if (boundMethodGroup != null)
			{
				if (methodSymbol?.OriginalDefinition is LocalFunctionSymbol symbol)
				{
					VisitLocalFunctionUse(symbol);
				}
				methodSymbol = CheckMethodGroupReceiverNullability(boundMethodGroup, item2, methodSymbol, conversion.IsExtensionMethod);
			}
			if (reportRemainingWarnings && item != null)
			{
				ReportNullabilityMismatchWithTargetDelegate(getDiagnosticLocation(), type, item, methodSymbol, conversion.IsExtensionMethod);
			}
			resultState = NullableFlowState.NotNull;
			break;
		}
		case ConversionKind.AnonymousFunction:
			if (conversionOperand is BoundLambda boundLambda)
			{
				NamedTypeSymbol delegateType = type.GetDelegateType();
				VisitLambda(boundLambda, delegateType, stateForLambda);
				if (reportRemainingWarnings && (object)delegateType != null)
				{
					ReportNullabilityMismatchWithTargetDelegate(getDiagnosticLocation(), delegateType, boundLambda);
				}
				TrackAnalyzedNullabilityThroughConversionGroup(targetTypeWithNullability.ToTypeWithState(), conversionOpt, conversionOperand);
				return TypeWithState.Create(type, NullableFlowState.NotNull);
			}
			break;
		case ConversionKind.FunctionType:
			resultState = NullableFlowState.NotNull;
			break;
		case ConversionKind.InterpolatedString:
			resultState = NullableFlowState.NotNull;
			break;
		case ConversionKind.InterpolatedStringHandler:
			visitInterpolatedStringHandlerConstructor();
			resultState = NullableFlowState.NotNull;
			break;
		case ConversionKind.SwitchExpression:
		case ConversionKind.ConditionalExpression:
		case ConversionKind.ObjectCreation:
		case ConversionKind.CollectionExpression:
			resultState = getConversionResultState(operandType);
			break;
		case ConversionKind.ImplicitUserDefined:
		case ConversionKind.ExplicitUserDefined:
			return VisitUserDefinedConversion(conversionOpt, conversionOperand, conversion, targetTypeWithNullability, operandType, useLegacyWarnings, assignmentKind, parameterOpt, reportTopLevelWarnings, reportRemainingWarnings, getDiagnosticLocation());
		case ConversionKind.ImplicitDynamic:
		case ConversionKind.ExplicitDynamic:
			resultState = getConversionResultState(operandType);
			break;
		case ConversionKind.Boxing:
			resultState = getBoxingConversionResultState(targetTypeWithNullability, operandType);
			break;
		case ConversionKind.Unboxing:
			if (type.IsNonNullableValueType())
			{
				if (!operandType.IsNotNull & reportRemainingWarnings)
				{
					ReportDiagnostic(ErrorCode.WRN_UnboxPossibleNull, getDiagnosticLocation());
				}
				LearnFromNonNullTest(conversionOperand, ref State);
			}
			else
			{
				resultState = getUnboxingConversionResultState(operandType);
			}
			break;
		case ConversionKind.ImplicitThrow:
			resultState = NullableFlowState.NotNull;
			break;
		case ConversionKind.NoConversion:
			resultState = getConversionResultState(operandType);
			break;
		case ConversionKind.NullLiteral:
		case ConversionKind.DefaultLiteral:
			checkConversion = false;
			goto case ConversionKind.Identity;
		case ConversionKind.Identity:
		{
			if (useLegacyWarnings && conversionOperand is BoundConversion boundConversion && !boundConversion.ConversionKind.IsUserDefinedConversion())
			{
				TypeWithAnnotations? typeWithAnnotations = boundConversion.ConversionGroupOpt?.ExplicitType;
				if (typeWithAnnotations.HasValue && typeWithAnnotations.GetValueOrDefault().Equals(targetTypeWithNullability, TypeCompareKind.ConsiderEverything))
				{
					TrackAnalyzedNullabilityThroughConversionGroup(calculateResultType(targetTypeWithNullability, fromExplicitCast, operandType.State, isSuppressed, type), conversionOpt, conversionOperand);
					return operandType;
				}
			}
			TypeSymbol? type3 = operandType.Type;
			if (((object)type3 == null || !type3.IsTupleType) && conversionOperand.Kind != BoundKind.TupleLiteral)
			{
				goto case ConversionKind.ImplicitReference;
			}
			goto case ConversionKind.ImplicitTupleLiteral;
		}
		case ConversionKind.ImplicitReference:
		case ConversionKind.ExplicitReference:
			if (checkConversion)
			{
				conversion = GenerateConversion(_conversions, conversionOperand, operandType.Type, type, fromExplicitCast, extensionMethodThisArgument, conversionOpt?.Checked ?? false);
				flag = conversion.Exists;
			}
			resultState = (conversion.IsReference ? getReferenceConversionResultState(targetTypeWithNullability, operandType) : operandType.State);
			break;
		case ConversionKind.ImplicitNullable:
		{
			if (trackMembers && AreNullableAndUnderlyingTypes(type, operandType.Type, out var underlyingTypeWithAnnotations))
			{
				int num2 = MakeSlot(conversionOperand);
				if (num2 > 0)
				{
					int orCreatePlaceholderSlot2 = GetOrCreatePlaceholderSlot(conversionOpt);
					TrackNullableStateOfNullableValue(orCreatePlaceholderSlot2, type, conversionOperand, underlyingTypeWithAnnotations.ToTypeWithState(), num2);
				}
			}
			if (checkConversion)
			{
				conversion = GenerateConversion(_conversions, conversionOperand, operandType.Type, type, fromExplicitCast, extensionMethodThisArgument, conversionOpt?.Checked ?? false);
				flag = conversion.Exists;
			}
			resultState = operandType.State;
			break;
		}
		case ConversionKind.ExplicitNullable:
		{
			TypeSymbol? type2 = operandType.Type;
			if ((object)type2 != null && type2.IsNullableType() && !type.IsNullableType())
			{
				if (reportTopLevelWarnings && operandType.MayBeNull)
				{
					ReportDiagnostic(ErrorCode.WRN_NullableValueTypeMayBeNull, getDiagnosticLocation());
				}
				if (conversionOperand != null)
				{
					LearnFromNonNullTest(conversionOperand, ref State);
				}
			}
			goto case ConversionKind.ImplicitNullable;
		}
		case ConversionKind.ImplicitTupleLiteral:
		case ConversionKind.ImplicitTuple:
		case ConversionKind.ExplicitTupleLiteral:
		case ConversionKind.ExplicitTuple:
			if (trackMembers)
			{
				ConversionKind kind = conversion.Kind;
				if (kind == ConversionKind.ImplicitTuple || kind == ConversionKind.ExplicitTuple)
				{
					int num = MakeSlot(conversionOperand);
					if (num > 0)
					{
						int orCreatePlaceholderSlot = GetOrCreatePlaceholderSlot(conversionOpt);
						if (orCreatePlaceholderSlot > 0)
						{
							TrackNullableStateOfTupleConversion(conversionOpt, conversionOperand, conversion, type, operandType.Type, orCreatePlaceholderSlot, num, assignmentKind, parameterOpt, reportRemainingWarnings);
						}
					}
				}
			}
			if (checkConversion && !type.IsErrorType())
			{
				conversion = GenerateConversion(_conversions, conversionOperand, operandType.Type, type, fromExplicitCast, extensionMethodThisArgument, conversionOpt?.Checked ?? false);
				flag = conversion.Exists;
			}
			resultState = NullableFlowState.NotNull;
			break;
		case ConversionKind.InlineArray:
			if (checkConversion)
			{
				conversion = GenerateConversion(_conversions, conversionOperand, operandType.Type, type, fromExplicitCast, extensionMethodThisArgument, conversionOpt?.Checked ?? false);
				flag = conversion.Exists;
			}
			break;
		case ConversionKind.ImplicitSpan:
		case ConversionKind.ExplicitSpan:
			if (checkConversion)
			{
				_ = conversion.Kind;
				conversion = GenerateConversion(_conversions, conversionOperand, operandType.Type, type, fromExplicitCast, extensionMethodThisArgument, conversionOpt?.Checked ?? false);
				flag = conversion.Exists && conversion.IsSpan;
			}
			break;
		}
		TypeWithState typeWithState = calculateResultType(targetTypeWithNullability, fromExplicitCast, resultState, isSuppressed, type);
		if (!conversionOperand.HasErrors && !type.IsErrorType())
		{
			if (reportTopLevelWarnings)
			{
				ReportNullableAssignmentIfNecessary(conversionOperand, targetTypeWithNullability, typeWithState, useLegacyWarnings, assignmentKind, parameterOpt, getDiagnosticLocation());
			}
			if (reportRemainingWarnings && !flag)
			{
				if (assignmentKind == AssignmentKind.Argument)
				{
					ReportNullabilityMismatchInArgument(getDiagnosticLocation(), operandType.Type, parameterOpt, type, forOutput: false);
				}
				else
				{
					ReportNullabilityMismatchInAssignment(getDiagnosticLocation(), GetTypeAsDiagnosticArgument(operandType.Type), type);
				}
			}
		}
		TrackAnalyzedNullabilityThroughConversionGroup(typeWithState, conversionOpt, conversionOperand);
		return typeWithState;
		static TypeWithState calculateResultType(TypeWithAnnotations typeWithAnnotations2, bool flag3, NullableFlowState defaultState, bool flag2, TypeSymbol targetType)
		{
			if (flag2)
			{
				defaultState = NullableFlowState.NotNull;
			}
			else if (flag3 && typeWithAnnotations2.NullableAnnotation.IsAnnotated() && !targetType.IsNullableType())
			{
				defaultState = (((object)targetType == null || !targetType.IsTypeParameterDisallowingAnnotationInCSharp8()) ? NullableFlowState.MaybeNull : NullableFlowState.MaybeDefault);
			}
			return TypeWithState.Create(targetType, defaultState);
		}
		static bool dependsOnTypeParameter(TypeParameterSymbol typeParameter1, TypeParameterSymbol typeParameter2, NullableAnnotation typeParameter1Annotation, out NullableAnnotation annotation)
		{
			if (typeParameter1.Equals(typeParameter2, TypeCompareKind.AllIgnoreOptions))
			{
				annotation = typeParameter1Annotation;
				return true;
			}
			bool flag2 = false;
			NullableAnnotation a = NullableAnnotation.Annotated;
			foreach (TypeWithAnnotations constraintTypesNoUseSiteDiagnostic in typeParameter1.ConstraintTypesNoUseSiteDiagnostics)
			{
				if (constraintTypesNoUseSiteDiagnostic.Type is TypeParameterSymbol typeParameter3 && dependsOnTypeParameter(typeParameter3, typeParameter2, constraintTypesNoUseSiteDiagnostic.NullableAnnotation, out var annotation2))
				{
					flag2 = true;
					a = a.Meet(annotation2);
				}
			}
			if (flag2)
			{
				annotation = a.Join(typeParameter1Annotation);
				return true;
			}
			annotation = NullableAnnotation.NotAnnotated;
			return false;
		}
		static NullableFlowState getBoxingConversionResultState(TypeWithAnnotations targetType, TypeWithState typeWithState2)
		{
			NullableFlowState state = typeWithState2.State;
			if (state == NullableFlowState.MaybeNull)
			{
				TypeSymbol type4 = typeWithState2.Type;
				if ((object)type4 == null || !type4.IsTypeParameterDisallowingAnnotationInCSharp8())
				{
					return NullableFlowState.MaybeDefault;
				}
				if (targetType.NullableAnnotation.IsNotAnnotated() && type4 is TypeParameterSymbol typeParameter && targetType.Type is TypeParameterSymbol typeParameter2 && dependsOnTypeParameter(typeParameter, typeParameter2, NullableAnnotation.NotAnnotated, out var annotation))
				{
					if (annotation != NullableAnnotation.Annotated)
					{
						return NullableFlowState.MaybeNull;
					}
					return NullableFlowState.MaybeDefault;
				}
			}
			return state;
		}
		static NullableFlowState getConversionResultState(TypeWithState typeWithState2)
		{
			NullableFlowState state = typeWithState2.State;
			if (state == NullableFlowState.MaybeNull)
			{
				return NullableFlowState.MaybeDefault;
			}
			return state;
		}
		static (MethodSymbol invokeSignature, ImmutableArray<ParameterSymbol>) getDelegateOrFunctionPointerInfo(TypeSymbol targetType)
		{
			if (targetType is NamedTypeSymbol namedTypeSymbol)
			{
				if (targetType.TypeKind == TypeKind.Delegate)
				{
					MethodSymbol delegateInvokeMethod = namedTypeSymbol.DelegateInvokeMethod;
					if ((object)delegateInvokeMethod != null)
					{
						ImmutableArray<ParameterSymbol> parameters = delegateInvokeMethod.Parameters;
						return (invokeSignature: delegateInvokeMethod, parameters);
					}
				}
			}
			else if (targetType is FunctionPointerTypeSymbol functionPointerTypeSymbol)
			{
				FunctionPointerMethodSymbol signature = functionPointerTypeSymbol.Signature;
				if ((object)signature != null)
				{
					ImmutableArray<ParameterSymbol> parameters2 = signature.Parameters;
					return (invokeSignature: signature, parameters2);
				}
			}
			return (invokeSignature: null, ImmutableArray<ParameterSymbol>.Empty);
		}
		Location getDiagnosticLocation()
		{
			if ((object)diagnosticLocation == null)
			{
				diagnosticLocation = (conversionOpt ?? conversionOperand).Syntax.GetLocation();
			}
			return diagnosticLocation;
		}
		static NullableFlowState getReferenceConversionResultState(TypeWithAnnotations targetType, TypeWithState typeWithState2)
		{
			NullableFlowState state = typeWithState2.State;
			switch (state)
			{
			case NullableFlowState.MaybeNull:
			{
				TypeSymbol type5 = targetType.Type;
				if ((object)type5 != null && type5.IsTypeParameterDisallowingAnnotationInCSharp8())
				{
					TypeSymbol type6 = typeWithState2.Type;
					if ((object)type6 == null || !type6.IsTypeParameterDisallowingAnnotationInCSharp8())
					{
						return NullableFlowState.MaybeDefault;
					}
					if (targetType.NullableAnnotation.IsNotAnnotated() && type6 is TypeParameterSymbol typeParameter && dependsOnTypeParameter(typeParameter, (TypeParameterSymbol)targetType.Type, NullableAnnotation.NotAnnotated, out var annotation))
					{
						if (annotation != NullableAnnotation.Annotated)
						{
							return NullableFlowState.MaybeNull;
						}
						return NullableFlowState.MaybeDefault;
					}
				}
				break;
			}
			case NullableFlowState.MaybeDefault:
			{
				TypeSymbol type4 = targetType.Type;
				if ((object)type4 != null && !type4.IsTypeParameterDisallowingAnnotationInCSharp8())
				{
					return NullableFlowState.MaybeNull;
				}
				break;
			}
			}
			return state;
		}
		static NullableFlowState getUnboxingConversionResultState(TypeWithState typeWithState2)
		{
			NullableFlowState state = typeWithState2.State;
			if (state == NullableFlowState.MaybeNull)
			{
				return NullableFlowState.MaybeDefault;
			}
			return state;
		}
		void visitHandlerConstruction(InterpolatedStringHandlerData handlerData)
		{
			VisitRvalue(handlerData.Construction);
		}
		void visitInterpolatedStringHandlerConstructor()
		{
			InterpolatedStringHandlerData interpolatedStringHandlerData = conversionOperand.GetInterpolatedStringHandlerData(throwOnMissing: false);
			if (!interpolatedStringHandlerData.IsDefault)
			{
				if (previousArgumentConversionResults == null)
				{
					visitHandlerConstruction(interpolatedStringHandlerData);
				}
				else
				{
					int num3 = ((parameterOpt?.ContainingType.IsExtension ?? false) ? 1 : 0);
					bool flag2 = false;
					foreach (BoundInterpolatedStringArgumentPlaceholder argumentPlaceholder in interpolatedStringHandlerData.ArgumentPlaceholders)
					{
						switch (argumentPlaceholder.ArgumentIndex)
						{
						case -2:
							AddPlaceholderReplacement(argumentPlaceholder, null, previousArgumentConversionResults[0]);
							flag2 = true;
							break;
						default:
							if (previousArgumentConversionResults.Count > argumentPlaceholder.ArgumentIndex)
							{
								AddPlaceholderReplacement(argumentPlaceholder, null, previousArgumentConversionResults[argumentPlaceholder.ArgumentIndex + num3]);
								flag2 = true;
							}
							break;
						case -4:
						case -3:
						case -1:
							break;
						}
					}
					visitHandlerConstruction(interpolatedStringHandlerData);
					if (flag2)
					{
						foreach (BoundInterpolatedStringArgumentPlaceholder argumentPlaceholder2 in interpolatedStringHandlerData.ArgumentPlaceholders)
						{
							bool flag3 = argumentPlaceholder2.ArgumentIndex < previousArgumentConversionResults.Count;
							if (flag3)
							{
								int argumentIndex = argumentPlaceholder2.ArgumentIndex;
								bool flag4 = ((argumentIndex >= 0 || argumentIndex == -2) ? true : false);
								flag3 = flag4;
							}
							if (flag3)
							{
								RemovePlaceholderReplacement(argumentPlaceholder2);
							}
						}
					}
				}
			}
		}
	}

	private TypeWithState VisitUserDefinedConversion(BoundConversion? conversionOpt, BoundExpression conversionOperand, Conversion conversion, TypeWithAnnotations targetTypeWithNullability, TypeWithState operandType, bool useLegacyWarnings, AssignmentKind assignmentKind, ParameterSymbol? parameterOpt, bool reportTopLevelWarnings, bool reportRemainingWarnings, Location diagnosticLocation)
	{
		TypeSymbol type = targetTypeWithNullability.Type;
		if (!conversion.IsValid)
		{
			TypeWithState typeWithState = TypeWithState.Create(type, NullableFlowState.NotNull);
			TrackAnalyzedNullabilityThroughConversionGroup(typeWithState, conversionOpt, conversionOperand);
			return typeWithState;
		}
		operandType = VisitConversion(conversionOpt, conversionOperand, conversion.UserDefinedFromConversion, TypeWithAnnotations.Create(conversion.BestUserDefinedConversionAnalysis.FromType), operandType, checkConversion: true, fromExplicitCast: false, useLegacyWarnings, assignmentKind, parameterOpt, reportTopLevelWarnings, reportRemainingWarnings, isSuppressed: false, extensionMethodThisArgument: false, default(Optional<LocalState>), trackMembers: false, diagnosticLocation);
		MethodSymbol method = conversion.Method;
		ParameterSymbol parameterSymbol = method.Parameters[0];
		FlowAnalysisAnnotations parameterAnnotations = GetParameterAnnotations(parameterSymbol);
		TypeWithAnnotations targetType = ApplyLValueAnnotations(parameterSymbol.TypeWithAnnotations, parameterAnnotations);
		TypeWithState typeWithState2 = default(TypeWithState);
		bool flag = false;
		if (operandType.Type.IsNullableType() && !targetType.IsNullableType())
		{
			TypeWithAnnotations nullableUnderlyingTypeWithAnnotations = operandType.Type.GetNullableUnderlyingTypeWithAnnotations();
			typeWithState2 = nullableUnderlyingTypeWithAnnotations.ToTypeWithState();
			flag = targetType.Equals(nullableUnderlyingTypeWithAnnotations, TypeCompareKind.AllIgnoreOptions);
		}
		NullableFlowState state = operandType.State;
		Location location = conversionOperand.Syntax.GetLocation();
		ClassifyAndVisitConversion(conversionOperand, targetType, flag ? typeWithState2 : operandType, useLegacyWarnings, AssignmentKind.Argument, parameterSymbol, reportRemainingWarnings, fromExplicitCast: false, location);
		if (!flag && CheckDisallowedNullAssignment(operandType, parameterAnnotations, conversionOperand.Syntax))
		{
			LearnFromNonNullTest(conversionOperand, ref State);
		}
		TypeWithAnnotations returnTypeWithAnnotations = method.ReturnTypeWithAnnotations;
		operandType = GetLiftedReturnTypeIfNecessary(flag, returnTypeWithAnnotations, state);
		if (!flag || state.IsNotNull())
		{
			operandType = ((!state.IsNotNull() || !method.ReturnNotNullIfParameterNotNull.Contains(parameterSymbol.Name)) ? ApplyUnconditionalAnnotations(operandType, GetRValueAnnotations(method)) : operandType.WithNotNullState());
		}
		operandType = ClassifyAndVisitConversion(conversionOperand, TypeWithAnnotations.Create(conversion.BestUserDefinedConversionAnalysis.ToType), operandType, useLegacyWarnings, assignmentKind, parameterOpt, reportRemainingWarnings, fromExplicitCast: false, location);
		operandType = ClassifyAndVisitConversion(conversionOpt ?? conversionOperand, targetTypeWithNullability, operandType, useLegacyWarnings, assignmentKind, parameterOpt, reportRemainingWarnings, conversionOpt?.ExplicitCastInCode ?? false, diagnosticLocation);
		LearnFromPostConditions(conversionOperand, parameterAnnotations);
		TrackAnalyzedNullabilityThroughConversionGroup(operandType, conversionOpt, conversionOperand);
		return operandType;
	}

	private void SnapshotWalkerThroughConversionGroup(BoundExpression conversionExpression, BoundExpression convertedNode)
	{
		if (_snapshotBuilderOpt != null)
		{
			BoundConversion boundConversion = conversionExpression as BoundConversion;
			_ = boundConversion?.ConversionGroupOpt;
			while (boundConversion != null && boundConversion != convertedNode && boundConversion.Syntax.SpanStart != convertedNode.Syntax.SpanStart)
			{
				TakeIncrementalSnapshot(boundConversion);
				boundConversion = boundConversion.Operand as BoundConversion;
			}
		}
	}

	private void TrackAnalyzedNullabilityThroughConversionGroup(TypeWithState resultType, BoundConversion? conversionOpt, BoundExpression convertedNode)
	{
		VisitResult result = new VisitResult(resultType, resultType.ToTypeWithAnnotations(compilation));
		_ = conversionOpt?.ConversionGroupOpt;
		while (conversionOpt != null && conversionOpt != convertedNode)
		{
			SetAnalyzedNullability(conversionOpt, result);
			conversionOpt = conversionOpt.Operand as BoundConversion;
		}
	}

	private TypeWithState GetLiftedReturnType(TypeWithAnnotations returnType, NullableFlowState operandState)
	{
		TypeSymbol type = (returnType.Type.IsNonNullableValueType() ? MakeNullableOf(returnType) : returnType.Type);
		NullableFlowState defaultState = returnType.ToTypeWithState().State.Join(operandState);
		return TypeWithState.Create(type, defaultState);
	}

	private static TypeWithState GetNullableUnderlyingTypeIfNecessary(bool isLifted, TypeWithState typeWithState)
	{
		if (isLifted)
		{
			TypeSymbol type = typeWithState.Type;
			if ((object)type != null && type.IsNullableType())
			{
				return type.GetNullableUnderlyingTypeWithAnnotations().ToTypeWithState();
			}
		}
		return typeWithState;
	}

	private TypeWithState GetLiftedReturnTypeIfNecessary(bool isLifted, TypeWithAnnotations returnType, NullableFlowState operandState)
	{
		if (!isLifted)
		{
			return returnType.ToTypeWithState();
		}
		return GetLiftedReturnType(returnType, operandState);
	}

	private TypeSymbol MakeNullableOf(TypeWithAnnotations underlying)
	{
		return compilation.GetSpecialType(SpecialType.System_Nullable_T).Construct(ImmutableArray.Create(underlying));
	}

	private TypeWithState ClassifyAndVisitConversion(BoundExpression node, TypeWithAnnotations targetType, TypeWithState operandType, bool useLegacyWarnings, AssignmentKind assignmentKind, ParameterSymbol? parameterOpt, bool reportWarnings, bool fromExplicitCast, Location diagnosticLocation)
	{
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
		Conversion conversion = _conversions.ClassifyStandardConversion(operandType.Type, targetType.Type, ref useSiteInfo);
		if (reportWarnings && !conversion.Exists)
		{
			if (assignmentKind == AssignmentKind.Argument)
			{
				ReportNullabilityMismatchInArgument(diagnosticLocation, operandType.Type, parameterOpt, targetType.Type, forOutput: false);
			}
			else
			{
				ReportNullabilityMismatchInAssignment(diagnosticLocation, operandType.Type, targetType.Type);
			}
		}
		return VisitConversion(null, node, conversion, targetType, operandType, checkConversion: false, fromExplicitCast, useLegacyWarnings, assignmentKind, parameterOpt, reportWarnings, !fromExplicitCast & reportWarnings, isSuppressed: false, extensionMethodThisArgument: false, default(Optional<LocalState>), trackMembers: false, diagnosticLocation);
	}

	public override BoundNode? VisitDelegateCreationExpression(BoundDelegateCreationExpression node)
	{
		if (node.MethodOpt?.OriginalDefinition is LocalFunctionSymbol symbol)
		{
			VisitLocalFunctionUse(symbol);
		}
		NamedTypeSymbol delegateType = (NamedTypeSymbol)node.Type;
		BoundExpression argument = node.Argument;
		Action<NamedTypeSymbol> analysisCompletion;
		if (!(argument is BoundMethodGroup boundMethodGroup))
		{
			if (!(argument is BoundLambda lambda))
			{
				if (argument != null)
				{
					TypeSymbol type = argument.Type;
					if ((object)type != null && type.TypeKind == TypeKind.Delegate)
					{
						analysisCompletion = visitDelegateArgument(delegateType, argument, node.WasTargetTyped);
						goto IL_00ad;
					}
				}
				VisitRvalue(node.Argument);
				analysisCompletion = null;
			}
			else
			{
				analysisCompletion = visitLambdaArgument(delegateType, lambda, node.WasTargetTyped);
			}
		}
		else
		{
			analysisCompletion = visitMethodGroupArgument(node, delegateType, boundMethodGroup);
		}
		goto IL_00ad;
		IL_00ad:
		TypeWithState type2 = setAnalyzedNullability(node, delegateType, analysisCompletion, node.WasTargetTyped);
		SetResultType(node, type2, updateAnalyzedNullability: false);
		return null;
		Action<NamedTypeSymbol>? analyzeDelegateConversion(NamedTypeSymbol namedTypeSymbol, BoundExpression arg, bool isTargetTyped)
		{
			if (isTargetTyped)
			{
				return analyzeDelegateConversionAsContinuation(arg);
			}
			TypeSymbol type3 = arg.Type;
			if (!arg.IsSuppressed)
			{
				MethodSymbol delegateInvokeMethod = namedTypeSymbol.DelegateInvokeMethod;
				if ((object)delegateInvokeMethod != null)
				{
					MethodSymbol methodSymbol = type3.DelegateInvokeMethod();
					if ((object)methodSymbol != null)
					{
						ReportNullabilityMismatchWithTargetDelegate(arg.Syntax.Location, namedTypeSymbol, delegateInvokeMethod, methodSymbol, invokedAsExtensionMethod: false);
					}
				}
			}
			return null;
		}
		Action<NamedTypeSymbol> analyzeDelegateConversionAsContinuation(BoundExpression arg)
		{
			return delegate(NamedTypeSymbol delegateType2)
			{
				analyzeDelegateConversion(delegateType2, arg, isTargetTyped: false);
			};
		}
		Action<NamedTypeSymbol>? analyzeLambdaConversion(NamedTypeSymbol namedTypeSymbol, BoundLambda boundLambda, bool isTargetTyped)
		{
			if (isTargetTyped)
			{
				return analyzeLambdaConversionAsContinuation(boundLambda);
			}
			VisitLambda(boundLambda, namedTypeSymbol);
			if (!boundLambda.IsSuppressed)
			{
				ReportNullabilityMismatchWithTargetDelegate(((LambdaSymbol)boundLambda.Symbol).DiagnosticLocation, namedTypeSymbol, boundLambda);
			}
			return null;
		}
		Action<NamedTypeSymbol> analyzeLambdaConversionAsContinuation(BoundLambda lambda2)
		{
			return delegate(NamedTypeSymbol delegateType2)
			{
				analyzeLambdaConversion(delegateType2, lambda2, isTargetTyped: false);
			};
		}
		Action<NamedTypeSymbol>? analyzeMethodGroupConversion(BoundDelegateCreationExpression boundDelegateCreationExpression, NamedTypeSymbol namedTypeSymbol, BoundMethodGroup group, bool isTargetTyped)
		{
			if (isTargetTyped)
			{
				return analyzeMethodGroupConversionAsContinuation(boundDelegateCreationExpression, group);
			}
			MethodSymbol methodOpt = boundDelegateCreationExpression.MethodOpt;
			if ((object)methodOpt != null)
			{
				MethodSymbol delegateInvokeMethod = namedTypeSymbol.DelegateInvokeMethod;
				if ((object)delegateInvokeMethod != null)
				{
					methodOpt = CheckMethodGroupReceiverNullability(group, delegateInvokeMethod.Parameters, methodOpt, boundDelegateCreationExpression.IsExtensionMethod);
					if (!group.IsSuppressed)
					{
						ReportNullabilityMismatchWithTargetDelegate(group.Syntax.Location, namedTypeSymbol, delegateInvokeMethod, methodOpt, boundDelegateCreationExpression.IsExtensionMethod);
					}
				}
			}
			return null;
		}
		Action<NamedTypeSymbol>? analyzeMethodGroupConversionAsContinuation(BoundDelegateCreationExpression node2, BoundMethodGroup group)
		{
			return delegate(NamedTypeSymbol delegateType2)
			{
				analyzeMethodGroupConversion(node2, delegateType2, group, isTargetTyped: false);
			};
		}
		TypeWithState setAnalyzedNullability(BoundDelegateCreationExpression boundDelegateCreationExpression, NamedTypeSymbol type3, Action<NamedTypeSymbol>? analysisCompletion2, bool isTargetTyped)
		{
			TypeWithState typeWithState = TypeWithState.Create(type3, NullableFlowState.NotNull);
			if (isTargetTyped)
			{
				setAnalyzedNullabilityAsContinuation(boundDelegateCreationExpression, analysisCompletion2);
			}
			else
			{
				SetAnalyzedNullability(boundDelegateCreationExpression, typeWithState);
			}
			return typeWithState;
		}
		void setAnalyzedNullabilityAsContinuation(BoundDelegateCreationExpression boundDelegateCreationExpression, Action<NamedTypeSymbol>? action)
		{
			TargetTypedAnalysisCompletion[boundDelegateCreationExpression] = delegate(TypeWithAnnotations resultTypeWithAnnotations)
			{
				NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)resultTypeWithAnnotations.Type;
				action?.Invoke(namedTypeSymbol);
				return setAnalyzedNullability(boundDelegateCreationExpression, namedTypeSymbol, null, isTargetTyped: false);
			};
		}
		Action<NamedTypeSymbol>? visitDelegateArgument(NamedTypeSymbol delegateType2, BoundExpression arg, bool isTargetTyped)
		{
			TypeWithAnnotations targetType = TypeWithAnnotations.Create(arg.Type, NullableAnnotation.NotAnnotated);
			TypeWithState valueType = VisitRvalueWithState(arg);
			ReportNullableAssignmentIfNecessary(arg, targetType, valueType, useLegacyWarnings: false);
			LearnFromNonNullTest(arg, ref State);
			return analyzeDelegateConversion(delegateType2, arg, isTargetTyped);
		}
		Action<NamedTypeSymbol>? visitLambdaArgument(NamedTypeSymbol delegateType2, BoundLambda boundLambda, bool isTargetTyped)
		{
			SetNotNullResult(boundLambda);
			return analyzeLambdaConversion(delegateType2, boundLambda, isTargetTyped);
		}
		Action<NamedTypeSymbol>? visitMethodGroupArgument(BoundDelegateCreationExpression boundDelegateCreationExpression, NamedTypeSymbol delegateType2, BoundMethodGroup group)
		{
			VisitMethodGroup(group);
			SetAnalyzedNullability(group, default(TypeWithState));
			return analyzeMethodGroupConversion(boundDelegateCreationExpression, delegateType2, group, boundDelegateCreationExpression.WasTargetTyped);
		}
	}

	public override BoundNode? VisitMethodGroup(BoundMethodGroup node)
	{
		BoundExpression receiverOpt = node.ReceiverOpt;
		if (receiverOpt != null)
		{
			VisitRvalue(receiverOpt);
			SetMethodGroupReceiverNullability(receiverOpt, ResultType);
		}
		SetNotNullResult(node);
		return null;
	}

	private bool TryGetMethodGroupReceiverNullability([NotNullWhen(true)] BoundExpression? receiverOpt, out TypeWithState type)
	{
		if (receiverOpt != null && _methodGroupReceiverMapOpt != null && _methodGroupReceiverMapOpt.TryGetValue(receiverOpt, out type))
		{
			return true;
		}
		type = default(TypeWithState);
		return false;
	}

	private void SetMethodGroupReceiverNullability(BoundExpression receiver, TypeWithState type)
	{
		if (_methodGroupReceiverMapOpt == null)
		{
			_methodGroupReceiverMapOpt = PooledDictionary<BoundExpression, TypeWithState>.GetInstance();
		}
		_methodGroupReceiverMapOpt[receiver] = type;
	}

	private MethodSymbol CheckMethodGroupReceiverNullability(BoundMethodGroup group, ImmutableArray<ParameterSymbol> parameters, MethodSymbol method, bool invokedAsExtensionMethod)
	{
		BoundExpression receiverOpt = group.ReceiverOpt;
		bool flag = method.IsExtensionBlockMember();
		if (TryGetMethodGroupReceiverNullability(receiverOpt, out var type))
		{
			SyntaxNode syntax = group.Syntax;
			if (!invokedAsExtensionMethod && !flag)
			{
				method = (MethodSymbol)AsMemberOfType(type.Type, method);
			}
			if (method.GetMemberArityIncludingExtension() != 0 && HasImplicitTypeArguments(group.Syntax))
			{
				ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
				if (invokedAsExtensionMethod | flag)
				{
					instance.Add(CreatePlaceholderIfNecessary(receiverOpt, type.ToTypeWithAnnotations(compilation)));
				}
				foreach (ParameterSymbol item in parameters)
				{
					TypeWithAnnotations typeWithAnnotations = item.TypeWithAnnotations;
					instance.Add(new BoundExpressionWithNullability(syntax, new BoundParameter(syntax, item), typeWithAnnotations.NullableAnnotation, typeWithAnnotations.Type));
				}
				method = InferMemberTypeArguments(method, instance.ToImmutableAndFree(), default(ImmutableArray<RefKind>), default(ImmutableArray<int>), expanded: false);
			}
			if (!flag || !method.IsStatic)
			{
				if (invokedAsExtensionMethod | flag)
				{
					ParameterSymbol parameter = (flag ? method.ContainingType.ExtensionParameter : method.Parameters[0]);
					CheckExtensionMethodThisNullability(receiverOpt, Conversion.Identity, parameter, type);
				}
				else
				{
					CheckPossibleNullReceiver(receiverOpt, type, checkNullableValueType: false);
				}
			}
			if (ConstraintsHelper.RequiresChecking(method))
			{
				CheckMethodConstraints(syntax, method);
			}
		}
		return method;
	}

	public override BoundNode? VisitLambda(BoundLambda node)
	{
		LocalState stateForLambda = State.Clone();
		if (!node.InAnonymousFunctionConversion)
		{
			VisitLambda(node, null);
		}
		SetNotNullResultForLambda(node, stateForLambda);
		return null;
	}

	private void VisitLambda(BoundLambda node, NamedTypeSymbol? delegateTypeOpt, Optional<LocalState> initialState = default(Optional<LocalState>))
	{
		MethodSymbol delegateInvokeMethod = delegateTypeOpt?.DelegateInvokeMethod;
		UseDelegateInvokeParameterAndReturnTypes(node, delegateInvokeMethod, out var useDelegateInvokeParameterTypes, out var useDelegateInvokeReturnType);
		if (useDelegateInvokeParameterTypes && _snapshotBuilderOpt != null)
		{
			SetUpdatedSymbol(node, node.Symbol, delegateTypeOpt);
		}
		AnalyzeLocalFunctionOrLambda(node, node.Symbol, initialState.HasValue ? initialState.Value : State.Clone(), delegateInvokeMethod, useDelegateInvokeParameterTypes, useDelegateInvokeReturnType);
	}

	private static void UseDelegateInvokeParameterAndReturnTypes(BoundLambda lambda, MethodSymbol? delegateInvokeMethod, out bool useDelegateInvokeParameterTypes, out bool useDelegateInvokeReturnType)
	{
		if ((object)delegateInvokeMethod == null)
		{
			useDelegateInvokeParameterTypes = false;
			useDelegateInvokeReturnType = false;
		}
		else
		{
			UnboundLambda unboundLambda = lambda.UnboundLambda;
			useDelegateInvokeParameterTypes = !unboundLambda.HasExplicitlyTypedParameterList;
			useDelegateInvokeReturnType = !unboundLambda.HasExplicitReturnType(out RefKind _, out ImmutableArray<CustomModifier> _, out TypeWithAnnotations _);
		}
	}

	public override BoundNode? VisitUnboundLambda(UnboundLambda node)
	{
		BoundLambda node2 = node.BindForErrorRecovery();
		VisitLambda(node2, null);
		SetNotNullResult(node);
		return null;
	}

	public override BoundNode? VisitThisReference(BoundThisReference node)
	{
		VisitThisOrBaseReference(node);
		return null;
	}

	private void VisitThisOrBaseReference(BoundExpression node)
	{
		TypeWithState resultType = TypeWithState.Create(node.Type, NullableFlowState.NotNull);
		TypeWithAnnotations lvalueType = TypeWithAnnotations.Create(node.Type, NullableAnnotation.NotAnnotated);
		SetResult(node, resultType, lvalueType);
	}

	public override BoundNode? VisitParameter(BoundParameter node)
	{
		ParameterSymbol parameterSymbol = node.ParameterSymbol;
		int orCreateSlot = GetOrCreateSlot(parameterSymbol);
		TypeWithAnnotations declaredParameterResult = GetDeclaredParameterResult(parameterSymbol);
		TypeWithState parameterState = GetParameterState(declaredParameterResult, parameterSymbol.FlowAnalysisAnnotations);
		SetResult(node, GetAdjustedResult(parameterState, orCreateSlot), declaredParameterResult);
		return null;
	}

	public override BoundNode? VisitAssignmentOperator(BoundAssignmentOperator node)
	{
		BoundExpression left = node.Left;
		BoundExpression right = node.Right;
		VisitLValue(left);
		Unsplit();
		FlowAnalysisAnnotations flowAnalysisAnnotations;
		TypeWithAnnotations typeWithAnnotations;
		if (left is BoundPropertyAccess { PropertySymbol: SourcePropertySymbolBase { SetMethod: null, UsesFieldKeyword: not false, BackingField: var backingField } })
		{
			flowAnalysisAnnotations = backingField.FlowAnalysisAnnotations;
			typeWithAnnotations = ApplyLValueAnnotations(GetTypeOrReturnTypeWithAnnotations(backingField), flowAnalysisAnnotations);
		}
		else
		{
			flowAnalysisAnnotations = GetLValueAnnotations(left);
			typeWithAnnotations = ApplyLValueAnnotations(LvalueResultType, flowAnalysisAnnotations);
		}
		if (left.Kind == BoundKind.EventAccess && ((BoundEventAccess)left).EventSymbol.IsWindowsRuntimeEvent)
		{
			VisitRvalue(right);
			SetNotNullResult(node);
		}
		else
		{
			TypeWithState rightState;
			if (!node.IsRef)
			{
				bool flag = left is BoundDiscardExpression;
				rightState = VisitOptionalImplicitConversion(right, flag ? default(TypeWithAnnotations) : typeWithAnnotations, UseLegacyWarnings(left), trackMembers: true, AssignmentKind.Assignment);
				Unsplit();
			}
			else
			{
				rightState = VisitRefExpression(right, typeWithAnnotations);
			}
			CheckDisallowedNullAssignment(rightState, flowAnalysisAnnotations, right.Syntax);
			if (left is BoundDiscardExpression)
			{
				TypeWithAnnotations lvalueType = rightState.ToTypeWithAnnotations(compilation);
				SetResult(left, rightState, lvalueType, updateAnalyzedNullability: true, true);
				SetResult(node, rightState, lvalueType);
			}
			else
			{
				SetResult(node, TypeWithState.Create(typeWithAnnotations.Type, rightState.State), typeWithAnnotations);
			}
			AdjustSetValue(left, ref rightState);
			TrackNullableStateForAssignment(right, typeWithAnnotations, MakeSlot(left), rightState, MakeSlot(right));
		}
		return null;
	}

	private bool IsPropertyOutputMoreStrictThanInput(PropertySymbol property)
	{
		TypeWithAnnotations typeWithAnnotations = property.TypeWithAnnotations;
		FlowAnalysisAnnotations flowAnalysisAnnotations = ((!IsAnalyzingAttribute) ? property.GetFlowAnalysisAnnotations() : FlowAnalysisAnnotations.None);
		TypeWithAnnotations typeWithAnnotations2 = ApplyLValueAnnotations(typeWithAnnotations, flowAnalysisAnnotations);
		if (typeWithAnnotations2.NullableAnnotation.IsOblivious() || !typeWithAnnotations2.CanBeAssignedNull)
		{
			return false;
		}
		return ApplyUnconditionalAnnotations(typeWithAnnotations.ToTypeWithState(), flowAnalysisAnnotations).IsNotNull;
	}

	private void AdjustSetValue(BoundExpression left, ref TypeWithState rightState)
	{
		PropertySymbol propertySymbol = ((left is BoundPropertyAccess boundPropertyAccess) ? boundPropertyAccess.PropertySymbol : ((!(left is BoundIndexerAccess boundIndexerAccess)) ? null : boundIndexerAccess.Indexer));
		PropertySymbol propertySymbol2 = propertySymbol;
		if ((object)propertySymbol2 != null && IsPropertyOutputMoreStrictThanInput(propertySymbol2))
		{
			rightState = rightState.WithNotNullState();
		}
	}

	private FlowAnalysisAnnotations GetLValueAnnotations(BoundExpression expr)
	{
		if (IsAnalyzingAttribute)
		{
			return FlowAnalysisAnnotations.None;
		}
		FlowAnalysisAnnotations flowAnalysisAnnotations;
		if (!(expr is BoundPropertyAccess boundPropertyAccess))
		{
			if (!(expr is BoundIndexerAccess boundIndexerAccess))
			{
				if (!(expr is BoundFieldAccess boundFieldAccess))
				{
					if (expr is BoundParameter boundParameter)
					{
						ParameterSymbol parameterSymbol = boundParameter.ParameterSymbol;
						if ((object)parameterSymbol != null)
						{
							flowAnalysisAnnotations = ToInwardAnnotations(GetParameterAnnotations(parameterSymbol) & ~FlowAnalysisAnnotations.NotNull);
							goto IL_0084;
						}
					}
					flowAnalysisAnnotations = FlowAnalysisAnnotations.None;
				}
				else
				{
					flowAnalysisAnnotations = GetFieldAnnotations(boundFieldAccess.FieldSymbol);
				}
			}
			else
			{
				flowAnalysisAnnotations = boundIndexerAccess.Indexer.GetFlowAnalysisAnnotations();
			}
		}
		else
		{
			flowAnalysisAnnotations = boundPropertyAccess.PropertySymbol.GetFlowAnalysisAnnotations();
		}
		goto IL_0084;
		IL_0084:
		return flowAnalysisAnnotations & (FlowAnalysisAnnotations.AllowNull | FlowAnalysisAnnotations.DisallowNull);
	}

	private static FlowAnalysisAnnotations GetFieldAnnotations(FieldSymbol field)
	{
		if (!(field.AssociatedSymbol is SourcePropertySymbolBase { UsesFieldKeyword: false } sourcePropertySymbolBase))
		{
			return field.FlowAnalysisAnnotations;
		}
		return sourcePropertySymbolBase.GetFlowAnalysisAnnotations();
	}

	private FlowAnalysisAnnotations GetObjectInitializerMemberLValueAnnotations(Symbol memberSymbol)
	{
		if (IsAnalyzingAttribute)
		{
			return FlowAnalysisAnnotations.None;
		}
		FlowAnalysisAnnotations flowAnalysisAnnotations = ((memberSymbol is PropertySymbol property) ? property.GetFlowAnalysisAnnotations() : ((memberSymbol is FieldSymbol field) ? GetFieldAnnotations(field) : FlowAnalysisAnnotations.None));
		return flowAnalysisAnnotations & (FlowAnalysisAnnotations.AllowNull | FlowAnalysisAnnotations.DisallowNull);
	}

	private static FlowAnalysisAnnotations ToInwardAnnotations(FlowAnalysisAnnotations outwardAnnotations)
	{
		FlowAnalysisAnnotations flowAnalysisAnnotations = FlowAnalysisAnnotations.None;
		if ((outwardAnnotations & FlowAnalysisAnnotations.MaybeNull) != FlowAnalysisAnnotations.None)
		{
			flowAnalysisAnnotations |= FlowAnalysisAnnotations.AllowNull;
		}
		if ((outwardAnnotations & FlowAnalysisAnnotations.NotNull) == FlowAnalysisAnnotations.NotNull)
		{
			flowAnalysisAnnotations |= FlowAnalysisAnnotations.DisallowNull;
		}
		return flowAnalysisAnnotations;
	}

	private static bool UseLegacyWarnings(BoundExpression expr)
	{
		if (expr is BoundLocal boundLocal)
		{
			LocalSymbol localSymbol = boundLocal.LocalSymbol;
			if ((object)localSymbol != null && localSymbol.RefKind == RefKind.None)
			{
				goto IL_0061;
			}
		}
		else if (expr is BoundParameter boundParameter)
		{
			ParameterSymbol parameterSymbol = boundParameter.ParameterSymbol;
			if ((object)parameterSymbol != null && parameterSymbol.RefKind == RefKind.None && (!(parameterSymbol.ContainingSymbol is SynthesizedPrimaryConstructor synthesizedPrimaryConstructor) || !synthesizedPrimaryConstructor.GetCapturedParameters().ContainsKey(parameterSymbol)))
			{
				goto IL_0061;
			}
		}
		return false;
		IL_0061:
		return true;
	}

	public override BoundNode? VisitDeconstructionAssignmentOperator(BoundDeconstructionAssignmentOperator node)
	{
		return VisitDeconstructionAssignmentOperator(node, null);
	}

	private BoundNode? VisitDeconstructionAssignmentOperator(BoundDeconstructionAssignmentOperator node, TypeWithState? rightResultOpt)
	{
		bool disableNullabilityAnalysis = _disableNullabilityAnalysis;
		_disableNullabilityAnalysis = true;
		BoundTupleExpression left = node.Left;
		BoundConversion right = node.Right;
		ArrayBuilder<DeconstructionVariable> deconstructionAssignmentVariables = GetDeconstructionAssignmentVariables(left);
		if (node.HasErrors)
		{
			VisitRvalue(right.Operand);
		}
		else
		{
			VisitDeconstructionArguments(deconstructionAssignmentVariables, right.Conversion, right.Operand, rightResultOpt);
		}
		deconstructionAssignmentVariables.FreeAll((DeconstructionVariable v) => v.NestedVariables);
		SetNotNullResult(node);
		_disableNullabilityAnalysis = disableNullabilityAnalysis;
		return null;
	}

	private void VisitDeconstructionArguments(ArrayBuilder<DeconstructionVariable> variables, Conversion conversion, BoundExpression right, TypeWithState? rightResultOpt = null)
	{
		if (!conversion.DeconstructionInfo.IsDefault)
		{
			VisitDeconstructMethodArguments(variables, conversion, right, rightResultOpt);
		}
		else
		{
			VisitTupleDeconstructionArguments(variables, conversion.DeconstructConversionInfo, right, rightResultOpt);
		}
	}

	private void VisitDeconstructMethodArguments(ArrayBuilder<DeconstructionVariable> variables, Conversion conversion, BoundExpression right, TypeWithState? rightResultOpt)
	{
		VisitRvalue(right);
		if (rightResultOpt.HasValue)
		{
			SetResultType(right, rightResultOpt.Value);
		}
		TypeWithState resultType = ResultType;
		if (!(conversion.DeconstructionInfo.Invocation is BoundCall boundCall))
		{
			return;
		}
		MethodSymbol methodSymbol = boundCall.Method;
		if ((object)methodSymbol == null || boundCall.IsErroneousNode)
		{
			return;
		}
		int count = variables.Count;
		bool flag = methodSymbol.IsExtensionBlockMember();
		if (!boundCall.InvokedAsExtensionMethod && !flag)
		{
			CheckPossibleNullReceiver(right);
			if (methodSymbol.OriginalDefinition != methodSymbol)
			{
				methodSymbol = (MethodSymbol)AsMemberOfType(resultType.Type, methodSymbol);
			}
		}
		else if (methodSymbol.GetMemberArityIncludingExtension() != 0)
		{
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(count + 1);
			instance.Add(CreatePlaceholderIfNecessary(right, resultType.ToTypeWithAnnotations(compilation)));
			for (int i = 0; i < count; i++)
			{
				instance.Add(new BoundExpressionWithNullability(variables[i].Expression.Syntax, variables[i].Expression, NullableAnnotation.Oblivious, conversion.DeconstructionInfo.OutputPlaceholders[i].Type));
			}
			ImmutableArray<RefKind> argumentRefKinds = GetArgumentRefKinds(boundCall.ArgumentRefKindsOpt, flag, methodSymbol, count);
			ImmutableArray<int> argsToParamsOpt = GetArgsToParamsOpt(boundCall.ArgsToParamsOpt, flag);
			methodSymbol = InferMemberTypeArguments(methodSymbol, instance.ToImmutableAndFree(), argumentRefKinds, argsToParamsOpt, boundCall.Expanded);
			if (ConstraintsHelper.RequiresChecking(methodSymbol))
			{
				CheckMethodConstraints(boundCall.Syntax, methodSymbol);
			}
		}
		ImmutableArray<ParameterSymbol> parameters = methodSymbol.Parameters;
		int num = (boundCall.InvokedAsExtensionMethod ? 1 : 0);
		if (boundCall.InvokedAsExtensionMethod | flag)
		{
			Conversion item = RemoveConversion(boundCall.Arguments[0], includeExplicitConversions: false).conversion;
			ParameterSymbol parameter = (flag ? methodSymbol.ContainingType.ExtensionParameter : methodSymbol.Parameters[0]);
			CheckExtensionMethodThisNullability(right, item, parameter, resultType);
		}
		for (int j = 0; j < count; j++)
		{
			DeconstructionVariable deconstructionVariable = variables[j];
			ParameterSymbol parameterSymbol = parameters[j + num];
			(BoundValuePlaceholder? placeholder, BoundExpression? conversion) tuple = conversion.DeconstructConversionInfo[j];
			Conversion conversion2 = BoundNode.GetConversion(placeholder: tuple.placeholder, conversion: tuple.conversion);
			ArrayBuilder<DeconstructionVariable> nestedVariables = deconstructionVariable.NestedVariables;
			if (nestedVariables != null)
			{
				BoundExpression right2 = CreatePlaceholderIfNecessary(boundCall.Arguments[j + num], parameterSymbol.TypeWithAnnotations);
				VisitDeconstructionArguments(nestedVariables, conversion2, right2);
			}
			else
			{
				VisitArgumentConversionAndInboundAssignmentsAndPreConditions(null, deconstructionVariable.Expression, conversion2, parameterSymbol.RefKind, parameterSymbol, parameterSymbol.TypeWithAnnotations, GetParameterAnnotations(parameterSymbol), new VisitResult(deconstructionVariable.Type.ToTypeWithState(), deconstructionVariable.Type), null, extensionMethodThisArgument: false);
			}
		}
		for (int k = 0; k < count; k++)
		{
			DeconstructionVariable deconstructionVariable2 = variables[k];
			ParameterSymbol parameterSymbol2 = parameters[k + num];
			if (deconstructionVariable2.NestedVariables == null)
			{
				VisitArgumentOutboundAssignmentsAndPostConditions(deconstructionVariable2.Expression, parameterSymbol2.RefKind, parameterSymbol2, parameterSymbol2.TypeWithAnnotations, GetRValueAnnotations(parameterSymbol2), new VisitResult(deconstructionVariable2.Type.ToTypeWithState(), deconstructionVariable2.Type), null, default(CompareExchangeInfo));
			}
		}
	}

	private void VisitTupleDeconstructionArguments(ArrayBuilder<DeconstructionVariable> variables, ImmutableArray<(BoundValuePlaceholder? placeholder, BoundExpression? conversion)> deconstructConversionInfo, BoundExpression right, TypeWithState? rightResultOpt)
	{
		int count = variables.Count;
		ImmutableArray<BoundExpression> deconstructionRightParts = GetDeconstructionRightParts(right, rightResultOpt);
		for (int i = 0; i < count; i++)
		{
			DeconstructionVariable deconstructionVariable = variables[i];
			(BoundValuePlaceholder? placeholder, BoundExpression? conversion) tuple = deconstructConversionInfo[i];
			Conversion conversion = BoundNode.GetConversion(placeholder: tuple.placeholder, conversion: tuple.conversion);
			BoundExpression boundExpression = deconstructionRightParts[i];
			ArrayBuilder<DeconstructionVariable> nestedVariables = deconstructionVariable.NestedVariables;
			if (nestedVariables != null)
			{
				VisitDeconstructionArguments(nestedVariables, conversion, boundExpression);
				continue;
			}
			TypeWithAnnotations type = deconstructionVariable.Type;
			FlowAnalysisAnnotations lValueAnnotations = GetLValueAnnotations(deconstructionVariable.Expression);
			type = ApplyLValueAnnotations(type, lValueAnnotations);
			TypeWithState rightState;
			TypeWithState operandType;
			int valueSlot;
			if (conversion.IsIdentity)
			{
				if (deconstructionVariable.Expression is BoundLocal { DeclarationKind: BoundLocalDeclarationKind.WithInferredType } boundLocal)
				{
					rightState = (operandType = VisitRvalueWithState(boundExpression));
					_variables.SetType(boundLocal.LocalSymbol, operandType.ToAnnotatedTypeWithAnnotations(compilation));
				}
				else
				{
					operandType = default(TypeWithState);
					rightState = VisitOptionalImplicitConversion(boundExpression, type, useLegacyWarnings: true, trackMembers: true, AssignmentKind.Assignment);
					Unsplit();
				}
				valueSlot = MakeSlot(boundExpression);
			}
			else
			{
				operandType = VisitRvalueWithState(boundExpression);
				rightState = VisitConversion(null, boundExpression, conversion, type, operandType, checkConversion: true, fromExplicitCast: false, useLegacyWarnings: true, AssignmentKind.Assignment);
				valueSlot = -1;
			}
			CheckDisallowedNullAssignment(rightState, lValueAnnotations, right.Syntax);
			int num = MakeSlot(deconstructionVariable.Expression);
			AdjustSetValue(deconstructionVariable.Expression, ref rightState);
			TrackNullableStateForAssignment(boundExpression, type, num, rightState, valueSlot);
			if (num > 0 && conversion.Kind == ConversionKind.ImplicitNullable && AreNullableAndUnderlyingTypes(type.Type, operandType.Type, out var underlyingTypeWithAnnotations))
			{
				valueSlot = MakeSlot(boundExpression);
				if (valueSlot > 0)
				{
					TypeWithState valueType = TypeWithState.Create(underlyingTypeWithAnnotations.Type, NullableFlowState.NotNull);
					TrackNullableStateOfNullableValue(num, type.Type, boundExpression, valueType, valueSlot);
				}
			}
		}
	}

	private ArrayBuilder<DeconstructionVariable> GetDeconstructionAssignmentVariables(BoundTupleExpression tuple)
	{
		ImmutableArray<BoundExpression> arguments = tuple.Arguments;
		ArrayBuilder<DeconstructionVariable> instance = ArrayBuilder<DeconstructionVariable>.GetInstance(arguments.Length);
		foreach (BoundExpression item in arguments)
		{
			instance.Add(getDeconstructionAssignmentVariable(item));
		}
		return instance;
		DeconstructionVariable getDeconstructionAssignmentVariable(BoundExpression expr)
		{
			BoundKind kind = expr.Kind;
			if (kind - 171 <= BoundKind.PropertyEqualsValue)
			{
				return new DeconstructionVariable(expr, GetDeconstructionAssignmentVariables((BoundTupleExpression)expr));
			}
			VisitLValue(expr);
			return new DeconstructionVariable(expr, LvalueResultType);
		}
	}

	private ImmutableArray<BoundExpression> GetDeconstructionRightParts(BoundExpression expr, TypeWithState? rightResultOpt)
	{
		switch (expr.Kind)
		{
		case BoundKind.TupleLiteral:
		case BoundKind.ConvertedTupleLiteral:
			return ((BoundTupleExpression)expr).Arguments;
		case BoundKind.Conversion:
		{
			BoundConversion boundConversion = (BoundConversion)expr;
			ConversionKind conversionKind = boundConversion.ConversionKind;
			if (conversionKind == ConversionKind.Identity || conversionKind == ConversionKind.ImplicitTupleLiteral)
			{
				return GetDeconstructionRightParts(boundConversion.Operand, null);
			}
			break;
		}
		}
		if (rightResultOpt.HasValue)
		{
			expr = CreatePlaceholderIfNecessary(expr, rightResultOpt.GetValueOrDefault().ToTypeWithAnnotations(compilation));
		}
		if (expr.Type is NamedTypeSymbol { IsTupleType: not false } namedTypeSymbol)
		{
			return namedTypeSymbol.TupleElements.SelectAsArray((Func<FieldSymbol, BoundExpression, BoundExpression>)((FieldSymbol f, BoundExpression e) => new BoundFieldAccess(e.Syntax, e, f, null)), expr);
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/NullableWalker.cs", 11080);
	}

	public override BoundNode? VisitIncrementOperator(BoundIncrementOperator node)
	{
		MethodSymbol methodOpt = node.MethodOpt;
		bool num;
		if ((object)methodOpt == null)
		{
			if (node.OriginalUserDefinedOperatorsOpt.IsDefaultOrEmpty)
			{
				goto IL_01b3;
			}
			num = !node.OriginalUserDefinedOperatorsOpt[0].IsStatic;
		}
		else
		{
			num = !methodOpt.IsStatic;
		}
		if (num)
		{
			MethodSymbol methodOpt2 = node.MethodOpt;
			if ((object)methodOpt2 != null)
			{
				int num2 = -1;
				if (methodOpt2.IsExtensionBlockMember())
				{
					int num3 = MakeSlot(node.Operand);
					num2 = ((num3 > 0) ? num3 : GetOrCreatePlaceholderSlot(node.Operand));
				}
				TypeWithState receiverType = VisitAndCheckReceiver(node.Operand, methodOpt2);
				ImmutableArray<VisitResult> immutableArray;
				(methodOpt2, immutableArray, _) = ReInferMethodAndVisitArguments(node, node.Operand, receiverType, methodOpt2, ImmutableArray<BoundExpression>.Empty, default(ImmutableArray<RefKind>), default(ImmutableArray<int>), default(BitVector), expanded: false, invokedAsExtensionMethod: false);
				if (node.Type.IsVoidType())
				{
					SetNotNullResult(node);
				}
				else if (!methodOpt2.IsExtensionBlockMember())
				{
					SetResultType(node, TypeWithState.Create(receiverType.Type, NullableFlowState.NotNull));
				}
				else if (num2 > 0)
				{
					SetResultType(node, TypeWithState.Create(immutableArray[0].RValueType.Type, GetState(ref State, num2)));
				}
				else
				{
					SetResult(node, immutableArray[0], updateAnalyzedNullability: true, false);
				}
				SetUpdatedSymbol(node, node.MethodOpt, methodOpt2);
			}
			else
			{
				TypeWithState typeWithState = VisitRvalueWithState(node.Operand);
				if (node.Type.IsVoidType())
				{
					SetNotNullResult(node);
				}
				else
				{
					SetResultType(node, TypeWithState.Create(typeWithState.Type, NullableFlowState.NotNull));
				}
			}
			return null;
		}
		goto IL_01b3;
		IL_02f1:
		TypeWithAnnotations typeWithAnnotations;
		TypeWithState typeWithState3;
		AssignmentKind assignmentKind;
		ParameterSymbol parameterOpt;
		TypeWithState typeWithState2 = ((!typeWithAnnotations.HasType) ? typeWithState3 : VisitConversion(null, node.Operand, BoundNode.GetConversion(node.OperandConversion, node.OperandPlaceholder), typeWithAnnotations, typeWithState3, checkConversion: true, fromExplicitCast: false, useLegacyWarnings: false, assignmentKind, parameterOpt));
		MethodSymbol methodSymbol;
		bool flag;
		TypeWithState operandType = (((object)methodSymbol != null) ? GetLiftedReturnTypeIfNecessary(flag, methodSymbol.ReturnTypeWithAnnotations, typeWithState3.State) : typeWithState2);
		TypeWithAnnotations targetTypeWithNullability = typeWithState3.ToTypeWithAnnotations(compilation);
		operandType = VisitConversion(null, node, BoundNode.GetConversion(node.ResultConversion, node.ResultPlaceholder), targetTypeWithNullability, operandType, checkConversion: true, fromExplicitCast: false, useLegacyWarnings: false, AssignmentKind.Assignment);
		bool flag2;
		TypeWithAnnotations lvalueResultType;
		if (!node.HasErrors)
		{
			UnaryOperatorKind unaryOperatorKind = node.OperatorKind.Operator();
			TypeWithState type = ((unaryOperatorKind == UnaryOperatorKind.PrefixIncrement || unaryOperatorKind == UnaryOperatorKind.PrefixDecrement) ? operandType : typeWithState3);
			SetResultType(node, type);
			flag2 = true;
			TrackNullableStateForAssignment(node, lvalueResultType, MakeSlot(node.Operand), operandType);
		}
		goto IL_03f1;
		IL_03f1:
		if (!flag2)
		{
			SetNotNullResult(node);
		}
		return null;
		IL_0213:
		object obj;
		methodSymbol = (MethodSymbol)obj;
		if ((object)methodSymbol != null)
		{
			methodSymbol = ReInferUnaryOperator(node.Syntax, methodSymbol, node.Operand, GetNullableUnderlyingTypeIfNecessary(flag, typeWithState3));
			SetUpdatedSymbol(node, node.MethodOpt, methodSymbol);
		}
		assignmentKind = AssignmentKind.Assignment;
		parameterOpt = null;
		if (node.OperandConversion is BoundConversion { Conversion: { IsUserDefined: not false } conversion })
		{
			MethodSymbol? method = conversion.Method;
			if ((object)method != null && method.ParameterCount == 1)
			{
				typeWithAnnotations = conversion.Method.ReturnTypeWithAnnotations;
				goto IL_02f1;
			}
		}
		if ((object)methodSymbol != null)
		{
			typeWithAnnotations = methodSymbol.Parameters[0].TypeWithAnnotations;
			if (flag)
			{
				typeWithAnnotations = TypeWithAnnotations.Create(MakeNullableOf(typeWithAnnotations));
			}
			assignmentKind = AssignmentKind.Argument;
			parameterOpt = methodSymbol.Parameters[0];
		}
		else
		{
			typeWithAnnotations = default(TypeWithAnnotations);
		}
		goto IL_02f1;
		IL_01b3:
		typeWithState3 = VisitRvalueWithState(node.Operand);
		lvalueResultType = LvalueResultType;
		flag2 = false;
		if (State.Reachable)
		{
			flag = node.OperatorKind.IsLifted();
			if (node.OperatorKind.IsUserDefined())
			{
				MethodSymbol? methodOpt3 = node.MethodOpt;
				if ((object)methodOpt3 != null && methodOpt3.ParameterCount == 1)
				{
					obj = node.MethodOpt;
					goto IL_0213;
				}
			}
			obj = null;
			goto IL_0213;
		}
		goto IL_03f1;
	}

	public override BoundNode? VisitCompoundAssignmentOperator(BoundCompoundAssignmentOperator node)
	{
		MethodSymbol method = node.Operator.Method;
		bool num;
		if ((object)method == null)
		{
			if (node.OriginalUserDefinedOperatorsOpt.IsDefaultOrEmpty)
			{
				goto IL_01eb;
			}
			num = !node.OriginalUserDefinedOperatorsOpt[0].IsStatic;
		}
		else
		{
			num = !method.IsStatic;
		}
		if (num)
		{
			MethodSymbol method2 = node.Operator.Method;
			if ((object)method2 != null)
			{
				int num2 = -1;
				if (method2.IsExtensionBlockMember())
				{
					int num3 = MakeSlot(node.Left);
					num2 = ((num3 > 0) ? num3 : GetOrCreatePlaceholderSlot(node.Left));
				}
				TypeWithState receiverType = VisitAndCheckReceiver(node.Left, method2);
				ImmutableArray<VisitResult> immutableArray;
				(method2, immutableArray, _) = ReInferMethodAndVisitArguments(node, node.Left, receiverType, method2, ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1] { node.Right }), default(ImmutableArray<RefKind>), default(ImmutableArray<int>), default(BitVector), expanded: false, invokedAsExtensionMethod: false);
				if (node.Type.IsVoidType())
				{
					SetNotNullResult(node);
				}
				else if (!method2.IsExtensionBlockMember())
				{
					SetResultType(node, TypeWithState.Create(receiverType.Type, NullableFlowState.NotNull));
				}
				else if (num2 > 0)
				{
					SetResultType(node, TypeWithState.Create(immutableArray[0].RValueType.Type, GetState(ref State, num2)));
				}
				else
				{
					SetResult(node, immutableArray[0], updateAnalyzedNullability: true, false);
				}
				SetUpdatedSymbol(node, node.Operator.Method, method2);
			}
			else
			{
				Visit(node.Left);
				TypeWithState resultType = ResultType;
				Unsplit();
				VisitRvalue(node.Right);
				if (node.Type.IsVoidType())
				{
					SetNotNullResult(node);
				}
				else
				{
					SetResultType(node, TypeWithState.Create(resultType.Type, NullableFlowState.NotNull));
				}
			}
			return null;
		}
		goto IL_01eb;
		IL_01eb:
		Visit(node.Left);
		Unsplit();
		TypeWithState resultType2 = ResultType;
		TypeWithAnnotations lvalueResultType = LvalueResultType;
		(BoundExpression expression, Conversion conversion) tuple2 = RemoveConversion(node.Right, includeExplicitConversions: false);
		BoundExpression item = tuple2.expression;
		Conversion item2 = tuple2.conversion;
		TypeWithState rightType = VisitRvalueWithState(item);
		TypeWithState operandType = ReinferAndVisitBinaryOperator(node, node.Operator.Kind, node.Operator.Method, node.Operator.ReturnType ?? node.Type, (node.LeftConversion as BoundConversion) ?? node.Left, node.Left, BoundNode.GetConversion(node.LeftConversion, node.LeftPlaceholder), resultType2, node.Right, item, item2, rightType);
		FlowAnalysisAnnotations lValueAnnotations = GetLValueAnnotations(node.Left);
		lvalueResultType = ApplyLValueAnnotations(lvalueResultType, lValueAnnotations);
		operandType = VisitConversion(null, node, BoundNode.GetConversion(node.FinalConversion, node.FinalPlaceholder), lvalueResultType, operandType, checkConversion: true, fromExplicitCast: false, useLegacyWarnings: false, AssignmentKind.Assignment);
		CheckDisallowedNullAssignment(operandType, lValueAnnotations, node.Syntax);
		SetResultType(node, operandType);
		AdjustSetValue(node.Left, ref operandType);
		TrackNullableStateForAssignment(node, lvalueResultType, MakeSlot(node.Left), operandType);
		return null;
	}

	public override BoundNode? VisitFixedLocalCollectionInitializer(BoundFixedLocalCollectionInitializer node)
	{
		BoundExpression boundExpression = node.Expression;
		if (boundExpression.Kind == BoundKind.AddressOfOperator)
		{
			boundExpression = ((BoundAddressOfOperator)boundExpression).Operand;
		}
		VisitRvalue(boundExpression);
		if (node.Expression.Kind == BoundKind.AddressOfOperator)
		{
			SetResultType(node.Expression, TypeWithState.Create(node.Expression.Type, ResultType.State));
		}
		SetNotNullResult(node);
		return null;
	}

	public override BoundNode? VisitAddressOfOperator(BoundAddressOfOperator node)
	{
		Visit(node.Operand);
		SetNotNullResult(node);
		return null;
	}

	private void ReportNullabilityMismatchInRefArgument(BoundExpression argument, TypeSymbol argumentType, ParameterSymbol parameter, TypeSymbol parameterType)
	{
		ReportDiagnostic(ErrorCode.WRN_NullabilityMismatchInArgument, argument.Syntax, argumentType, parameterType, GetParameterAsDiagnosticArgument(parameter), GetContainingSymbolAsDiagnosticArgument(parameter));
	}

	private void ReportNullabilityMismatchInArgument(SyntaxNode argument, TypeSymbol argumentType, ParameterSymbol parameter, TypeSymbol parameterType, bool forOutput)
	{
		ReportNullabilityMismatchInArgument(argument.GetLocation(), argumentType, parameter, parameterType, forOutput);
	}

	private void ReportNullabilityMismatchInArgument(Location argumentLocation, TypeSymbol argumentType, ParameterSymbol? parameterOpt, TypeSymbol parameterType, bool forOutput)
	{
		ReportDiagnostic(forOutput ? ErrorCode.WRN_NullabilityMismatchInArgumentForOutput : ErrorCode.WRN_NullabilityMismatchInArgument, argumentLocation, argumentType, ((object)parameterOpt != null && parameterOpt.Type.IsNonNullableValueType() && parameterType.IsNullableType()) ? parameterOpt.Type : parameterType, GetParameterAsDiagnosticArgument(parameterOpt), GetContainingSymbolAsDiagnosticArgument(parameterOpt));
	}

	private TypeWithAnnotations GetDeclaredLocalResult(LocalSymbol local)
	{
		if (!_variables.TryGetType(local, out var type))
		{
			return local.TypeWithAnnotations;
		}
		return type;
	}

	private TypeWithAnnotations GetDeclaredParameterResult(ParameterSymbol parameter)
	{
		if (!_variables.TryGetType(parameter, out var type))
		{
			return parameter.TypeWithAnnotations;
		}
		return type;
	}

	public override BoundNode? VisitBaseReference(BoundBaseReference node)
	{
		VisitThisOrBaseReference(node);
		return null;
	}

	public override BoundNode? VisitFieldAccess(BoundFieldAccess node)
	{
		Symbol updatedSymbol = VisitMemberAccess(node, node.ReceiverOpt, node.FieldSymbol);
		SplitIfBooleanConstant(node);
		SetUpdatedSymbol(node, node.FieldSymbol, updatedSymbol);
		return null;
	}

	private (PropertySymbol updatedProperty, bool returnNotNull) ReInferAndVisitExtensionPropertyAccess(BoundNode node, PropertySymbol property, BoundExpression receiver)
	{
		ImmutableArray<BoundExpression> arguments = ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1] { receiver });
		ParameterSymbol extensionParameter = property.ContainingType.ExtensionParameter;
		ImmutableArray<ParameterSymbol> parametersOpt = ImmutableCollectionsMarshal.AsImmutableArray(new ParameterSymbol[1] { extensionParameter });
		ImmutableArray<RefKind> refKindsOpt = ((extensionParameter.RefKind == RefKind.Ref) ? ImmutableCollectionsMarshal.AsImmutableArray(new RefKind[1] { RefKind.Ref }) : default(ImmutableArray<RefKind>));
		var (item, _, item2) = VisitArguments(node, arguments, refKindsOpt, parametersOpt, default(ImmutableArray<int>), default(BitVector), expanded: false, invokedAsExtensionMethod: false, property);
		return (updatedProperty: item, returnNotNull: item2);
	}

	public override BoundNode? VisitPropertyAccess(BoundPropertyAccess node)
	{
		PropertySymbol propertySymbol = node.PropertySymbol;
		Symbol symbol;
		if (propertySymbol.IsExtensionBlockMember())
		{
			symbol = ReInferAndVisitExtensionPropertyAccess(node, propertySymbol, node.ReceiverOpt).updatedProperty;
			TypeWithAnnotations typeOrReturnTypeWithAnnotations = GetTypeOrReturnTypeWithAnnotations(symbol);
			FlowAnalysisAnnotations rValueAnnotations = GetRValueAnnotations(symbol);
			TypeWithState resultType = ApplyUnconditionalAnnotations(typeOrReturnTypeWithAnnotations.ToTypeWithState(), rValueAnnotations);
			SetResult(node, resultType, typeOrReturnTypeWithAnnotations);
		}
		else
		{
			symbol = VisitMemberAccess(node, node.ReceiverOpt, propertySymbol);
		}
		if (!IsAnalyzingAttribute)
		{
			if (_expressionIsRead)
			{
				ApplyMemberPostConditions(node.ReceiverOpt, propertySymbol.GetMethod);
			}
			else
			{
				ApplyMemberPostConditions(node.ReceiverOpt, propertySymbol.SetMethod);
			}
		}
		SetUpdatedSymbol(node, propertySymbol, symbol);
		return null;
	}

	public override BoundNode? VisitIndexerAccess(BoundIndexerAccess node)
	{
		BoundExpression receiverOpt = node.ReceiverOpt;
		TypeSymbol type = VisitRvalueWithState(receiverOpt).Type;
		CheckPossibleNullReceiver(receiverOpt);
		PropertySymbol propertySymbol = node.Indexer;
		if ((object)type != null)
		{
			propertySymbol = (PropertySymbol)AsMemberOfType(type, propertySymbol);
		}
		VisitArguments(node, node.Arguments, node.ArgumentRefKindsOpt, propertySymbol, node.ArgsToParamsOpt, node.DefaultArguments, node.Expanded);
		TypeWithState resultType = ApplyUnconditionalAnnotations(propertySymbol.TypeWithAnnotations.ToTypeWithState(), GetRValueAnnotations(propertySymbol));
		SetResult(node, resultType, propertySymbol.TypeWithAnnotations);
		SetUpdatedSymbol(node, node.Indexer, propertySymbol);
		return null;
	}

	public override BoundNode? VisitImplicitIndexerAccess(BoundImplicitIndexerAccess node)
	{
		VisitRvalue(node.Receiver);
		VisitResult visitResult = _visitResult;
		VisitRvalue(node.Argument);
		AddPlaceholderReplacement(node.ReceiverPlaceholder, node.Receiver, visitResult);
		VisitRvalue(node.IndexerOrSliceAccess);
		RemovePlaceholderReplacement(node.ReceiverPlaceholder);
		SetResult(node, ResultType, LvalueResultType);
		return null;
	}

	public override BoundNode? VisitImplicitIndexerValuePlaceholder(BoundImplicitIndexerValuePlaceholder node)
	{
		SetNotNullResult(node);
		return null;
	}

	public override BoundNode? VisitImplicitIndexerReceiverPlaceholder(BoundImplicitIndexerReceiverPlaceholder node)
	{
		VisitPlaceholderWithReplacement(node);
		return null;
	}

	public override BoundNode? VisitCollectionExpressionSpreadExpressionPlaceholder(BoundCollectionExpressionSpreadExpressionPlaceholder node)
	{
		VisitPlaceholderWithReplacement(node);
		return null;
	}

	public override BoundNode? VisitValuePlaceholder(BoundValuePlaceholder node)
	{
		VisitPlaceholderWithReplacement(node);
		return null;
	}

	public override BoundNode? VisitEventAccess(BoundEventAccess node)
	{
		Symbol updatedSymbol = VisitMemberAccess(node, node.ReceiverOpt, node.EventSymbol);
		SetUpdatedSymbol(node, node.EventSymbol, updatedSymbol);
		return null;
	}

	private Symbol VisitMemberAccess(BoundExpression node, BoundExpression? receiverOpt, Symbol member)
	{
		TypeWithState typeWithState = ((receiverOpt != null) ? VisitRvalueWithState(receiverOpt) : default(TypeWithState));
		SpecialMember? specialMember = null;
		if (member.RequiresInstanceReceiver())
		{
			member = AsMemberOfType(typeWithState.Type, member);
			specialMember = GetNullableOfTMember(member);
			bool flag = specialMember != SpecialMember.System_Nullable_T_get_Value;
			CheckPossibleNullReceiver(receiverOpt, !flag);
		}
		TypeWithAnnotations typeOrReturnTypeWithAnnotations = GetTypeOrReturnTypeWithAnnotations(member);
		FlowAnalysisAnnotations rValueAnnotations = GetRValueAnnotations(member);
		TypeWithState resultType = ApplyUnconditionalAnnotations(typeOrReturnTypeWithAnnotations.ToTypeWithState(), rValueAnnotations);
		if (PossiblyNullableType(resultType.Type))
		{
			int num = MakeMemberSlot(receiverOpt, member);
			if (num > 0)
			{
				NullableFlowState state = GetState(ref State, num);
				resultType = TypeWithState.Create(resultType.Type, state);
			}
		}
		if (specialMember == SpecialMember.System_Nullable_T_get_HasValue && receiverOpt != null)
		{
			int num2 = MakeSlot(receiverOpt);
			if (num2 > 0)
			{
				Split();
				SetState(ref StateWhenTrue, num2, NullableFlowState.NotNull);
			}
		}
		SetResult(node, resultType, typeOrReturnTypeWithAnnotations);
		return member;
	}

	private SpecialMember? GetNullableOfTMember(Symbol member)
	{
		if (member.Kind == SymbolKind.Property)
		{
			MethodSymbol getMethod = ((PropertySymbol)member.OriginalDefinition).GetMethod;
			if ((object)getMethod != null && getMethod.ContainingType.SpecialType == SpecialType.System_Nullable_T)
			{
				if (getMethod == compilation.GetSpecialTypeMember(SpecialMember.System_Nullable_T_get_Value))
				{
					return SpecialMember.System_Nullable_T_get_Value;
				}
				if (getMethod == compilation.GetSpecialTypeMember(SpecialMember.System_Nullable_T_get_HasValue))
				{
					return SpecialMember.System_Nullable_T_get_HasValue;
				}
			}
		}
		return null;
	}

	private int GetNullableOfTValueSlot(TypeSymbol containingType, int containingSlot, out Symbol? valueProperty, bool forceSlotEvenIfEmpty = false)
	{
		valueProperty = ((MethodSymbol)compilation.GetSpecialTypeMember(SpecialMember.System_Nullable_T_get_Value))?.AsMember((NamedTypeSymbol)containingType)?.AssociatedSymbol;
		if ((object)valueProperty != null)
		{
			return GetOrCreateSlot(valueProperty, containingSlot, forceSlotEvenIfEmpty);
		}
		return -1;
	}

	protected override void VisitForEachExpression(BoundForEachStatement node)
	{
		if (node.Expression.Kind != BoundKind.Conversion)
		{
			VisitRvalue(node.Expression);
			Visit(node.EnumeratorInfoOpt?.MoveNextAwaitableInfo);
			return;
		}
		var (boundExpression, conversion) = RemoveConversion(node.Expression, includeExplicitConversions: false);
		SnapshotWalkerThroughConversionGroup(node.Expression, boundExpression);
		VisitForEachExpression(node, node.Expression, conversion, boundExpression, node.EnumeratorInfoOpt);
	}

	private void VisitForEachExpression(BoundNode node, BoundExpression collectionExpression, Conversion conversion, BoundExpression expr, ForEachEnumeratorInfo? enumeratorInfoOpt)
	{
		TypeWithState typeWithState = VisitRvalueWithState(expr);
		TypeSymbol type = typeWithState.Type;
		SetAnalyzedNullability(expr, _visitResult);
		MethodSymbol methodSymbol = null;
		MethodArgumentInfo methodArgumentInfo = enumeratorInfoOpt?.GetEnumeratorInfo;
		TypeWithAnnotations targetTypeWithNullability;
		if (methodArgumentInfo != null && (methodArgumentInfo.Method.IsExtensionMethod || methodArgumentInfo.Method.IsExtensionBlockMember()))
		{
			(MethodSymbol method, ImmutableArray<VisitResult> results, bool returnNotNull) tuple = ReInferMethodAndVisitArguments(node, expr, typeWithState, methodArgumentInfo.Method, methodArgumentInfo.Arguments, default(ImmutableArray<RefKind>), default(ImmutableArray<int>), methodArgumentInfo.DefaultArguments, methodArgumentInfo.Expanded, invokedAsExtensionMethod: true, _visitResult);
			methodSymbol = tuple.method;
			ImmutableArray<VisitResult> item = tuple.results;
			targetTypeWithNullability = item[0].LValueType;
		}
		else if (conversion.IsIdentity || (conversion.Kind == ConversionKind.ExplicitReference && type.SpecialType == SpecialType.System_String))
		{
			targetTypeWithNullability = typeWithState.ToTypeWithAnnotations(compilation);
		}
		else
		{
			if (!conversion.IsImplicit)
			{
				return;
			}
			bool isAsync = enumeratorInfoOpt?.MoveNextAwaitableInfo != null;
			if (collectionExpression.Type.SpecialType == SpecialType.System_Collections_IEnumerable)
			{
				targetTypeWithNullability = TypeWithAnnotations.Create(collectionExpression.Type);
			}
			else
			{
				if (!Binder.IsIEnumerableT(collectionExpression.Type.OriginalDefinition, isAsync, compilation))
				{
					return;
				}
				CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
				targetTypeWithNullability = TypeWithAnnotations.Create(Binder.GetIEnumerableOfT(type, isAsync, compilation, ref useSiteInfo, out var _, out var _));
			}
		}
		TypeWithState rValueType = VisitConversion(GetConversionIfApplicable(collectionExpression, expr), expr, conversion, targetTypeWithNullability, typeWithState, checkConversion: true, fromExplicitCast: false, useLegacyWarnings: false, AssignmentKind.Assignment);
		MethodSymbol methodSymbol2 = enumeratorInfoOpt?.GetEnumeratorInfo.Method;
		bool flag = ((object)methodSymbol2 == null || (!methodSymbol2.IsExtensionMethod && !methodSymbol2.IsExtensionBlockMember())) && CheckPossibleNullReceiver(expr);
		SetAnalyzedNullability(collectionExpression, new VisitResult(rValueType, rValueType.ToTypeWithAnnotations(compilation)));
		TypeWithState type2;
		TypeSymbol type3;
		if (enumeratorInfoOpt == null)
		{
			type2 = default(TypeWithState);
		}
		else if (type is ArrayTypeSymbol { ElementTypeWithAnnotations: var elementTypeWithAnnotations })
		{
			type2 = elementTypeWithAnnotations.ToTypeWithState();
		}
		else
		{
			if (type.SpecialType != SpecialType.System_String)
			{
				if ((object)methodSymbol == null)
				{
					if (enumeratorInfoOpt != null)
					{
						WellKnownType inlineArraySpanType = enumeratorInfoOpt.InlineArraySpanType;
						if (inlineArraySpanType != WellKnownType.Unknown)
						{
							type3 = compilation.GetWellKnownType(inlineArraySpanType).Construct(ImmutableArray.Create(rValueType.Type.TryGetInlineArrayElementField().TypeWithAnnotations));
							goto IL_02bf;
						}
					}
					type3 = rValueType.Type;
					goto IL_02bf;
				}
				goto IL_02d8;
			}
			type2 = TypeWithAnnotations.Create(enumeratorInfoOpt.ElementType, NullableAnnotation.NotAnnotated).ToTypeWithState();
		}
		goto IL_045b;
		IL_045b:
		SetResultType(null, type2);
		return;
		IL_02d8:
		TypeWithState returnTypeWithState = GetReturnTypeWithState(methodSymbol);
		if (returnTypeWithState.State != NullableFlowState.NotNull && !flag)
		{
			if (collectionExpression is BoundConversion boundConversion)
			{
				BoundExpression operand = boundConversion.Operand;
				if (operand != null && operand.IsSuppressed)
				{
					goto IL_0329;
				}
			}
			ReportDiagnostic(ErrorCode.WRN_NullReferenceReceiver, expr.Syntax.GetLocation());
		}
		goto IL_0329;
		IL_0329:
		MethodSymbol methodSymbol3 = (MethodSymbol)AsMemberOfType(returnTypeWithState.Type, enumeratorInfoOpt.CurrentPropertyGetter);
		type2 = ApplyUnconditionalAnnotations(methodSymbol3.ReturnTypeWithAnnotations.ToTypeWithState(), methodSymbol3.ReturnTypeFlowAnalysisAnnotations);
		if (enumeratorInfoOpt != null)
		{
			BoundAwaitableInfo moveNextAwaitableInfo = enumeratorInfoOpt.MoveNextAwaitableInfo;
			if (moveNextAwaitableInfo != null)
			{
				BoundAwaitableValuePlaceholder awaitableInstancePlaceholder = moveNextAwaitableInfo.AwaitableInstancePlaceholder;
				if (awaitableInstancePlaceholder != null)
				{
					MethodSymbol methodSymbol4 = (MethodSymbol)AsMemberOfType(methodSymbol.ReturnType, enumeratorInfoOpt.MoveNextInfo.Method);
					VisitResult result = new VisitResult(GetReturnTypeWithState(methodSymbol4), methodSymbol4.ReturnTypeWithAnnotations);
					AddPlaceholderReplacement(awaitableInstancePlaceholder, awaitableInstancePlaceholder, result);
					Visit(moveNextAwaitableInfo);
					RemovePlaceholderReplacement(awaitableInstancePlaceholder);
				}
			}
		}
		if (enumeratorInfoOpt != null && enumeratorInfoOpt.NeedsDisposal)
		{
			BoundAwaitableInfo disposeAwaitableInfo = enumeratorInfoOpt.DisposeAwaitableInfo;
			if (disposeAwaitableInfo != null)
			{
				BoundAwaitableValuePlaceholder awaitableInstancePlaceholder2 = disposeAwaitableInfo.AwaitableInstancePlaceholder;
				bool flag2 = false;
				MethodArgumentInfo patternDisposeInfo = enumeratorInfoOpt.PatternDisposeInfo;
				if (patternDisposeInfo != null)
				{
					MethodSymbol method = patternDisposeInfo.Method;
					MethodSymbol methodSymbol5 = (MethodSymbol)AsMemberOfType(methodSymbol.ReturnType, method);
					VisitResult result2 = new VisitResult(GetReturnTypeWithState(methodSymbol5), methodSymbol5.ReturnTypeWithAnnotations);
					AddPlaceholderReplacement(awaitableInstancePlaceholder2, awaitableInstancePlaceholder2, result2);
					flag2 = true;
				}
				Visit(disposeAwaitableInfo);
				if (flag2)
				{
					RemovePlaceholderReplacement(awaitableInstancePlaceholder2);
				}
			}
		}
		goto IL_045b;
		IL_02bf:
		methodSymbol = (MethodSymbol)AsMemberOfType(type3, enumeratorInfoOpt.GetEnumeratorInfo.Method);
		goto IL_02d8;
	}

	public override void VisitForEachIterationVariables(BoundForEachStatement node)
	{
		TypeWithState typeWithState = ((node.EnumeratorInfoOpt == null) ? default(TypeWithState) : ResultType);
		TypeWithAnnotations typeWithAnnotations = typeWithState.ToTypeWithAnnotations(compilation);
		SyntaxNode syntax = node.Syntax;
		Location location;
		if (!(syntax is ForEachStatementSyntax { Identifier: var identifier }))
		{
			if (!(syntax is ForEachVariableStatementSyntax forEachVariableStatementSyntax))
			{
				throw ExceptionUtilities.UnexpectedValue(node.Syntax);
			}
			location = forEachVariableStatementSyntax.Variable.GetLocation();
		}
		else
		{
			location = identifier.GetLocation();
		}
		Location location2 = location;
		if (node.DeconstructionOpt != null)
		{
			BoundDeconstructionAssignmentOperator deconstructionAssignment = node.DeconstructionOpt.DeconstructionAssignment;
			VisitDeconstructionAssignmentOperator(deconstructionAssignment, typeWithState.HasNullType ? ((TypeWithState?)null) : new TypeWithState?(typeWithState));
			Visit(node.IterationVariableType);
			return;
		}
		Visit(node.IterationVariableType);
		foreach (LocalSymbol iterationVariable in node.IterationVariables)
		{
			NullableFlowState value = NullableFlowState.NotNull;
			TypeWithAnnotations typeWithAnnotations2;
			TypeWithState typeWithState2;
			TypeWithState rValueType;
			BoundTypeExpression iterationVariableType;
			Conversion conversion2;
			TypeWithAnnotations targetTypeWithNullability;
			TypeWithState operandType;
			bool fromExplicitCast;
			int isSuppressed;
			if (!typeWithState.HasNullType)
			{
				typeWithAnnotations2 = iterationVariable.TypeWithAnnotations;
				typeWithState2 = typeWithState;
				rValueType = typeWithState;
				if (iterationVariable.IsRef)
				{
					if (node.Expression is BoundConversion boundConversion)
					{
						BoundExpression operand = boundConversion.Operand;
						if (operand != null && operand.IsSuppressed)
						{
							goto IL_025e;
						}
					}
					if (IsNullabilityMismatch(typeWithAnnotations, typeWithAnnotations2))
					{
						ForEachStatementSyntax forEachStatementSyntax2 = (ForEachStatementSyntax)node.Syntax;
						ReportNullabilityMismatchInAssignment(forEachStatementSyntax2.Type, typeWithAnnotations, typeWithAnnotations2);
					}
				}
				else
				{
					if (!(iterationVariable is SourceLocalSymbol { IsVar: not false }))
					{
						CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
						Conversion conversion = BoundNode.GetConversion(node.ElementConversion, node.ElementPlaceholder);
						if (conversion.Kind == ConversionKind.NoConversion)
						{
							conversion = _conversions.ClassifyImplicitConversionFromType(typeWithAnnotations.Type, typeWithAnnotations2.Type, ref useSiteInfo);
						}
						iterationVariableType = node.IterationVariableType;
						conversion2 = conversion;
						targetTypeWithNullability = typeWithAnnotations2;
						operandType = typeWithState;
						fromExplicitCast = !conversion.IsImplicit;
						if (node.Expression is BoundConversion boundConversion2)
						{
							BoundExpression operand = boundConversion2.Operand;
							if (operand != null)
							{
								isSuppressed = (operand.IsSuppressed ? 1 : 0);
								goto IL_0245;
							}
						}
						isSuppressed = 0;
						goto IL_0245;
					}
					typeWithAnnotations2 = typeWithState.ToAnnotatedTypeWithAnnotations(compilation);
					_variables.SetType(iterationVariable, typeWithAnnotations2);
					rValueType = typeWithAnnotations2.ToTypeWithState();
				}
				goto IL_025e;
			}
			goto IL_0282;
			IL_0245:
			location = location2;
			typeWithState2 = VisitConversion(null, iterationVariableType, conversion2, targetTypeWithNullability, operandType, checkConversion: true, fromExplicitCast, useLegacyWarnings: true, AssignmentKind.ForEachIterationVariable, null, reportTopLevelWarnings: true, reportRemainingWarnings: true, (byte)isSuppressed != 0, extensionMethodThisArgument: false, default(Optional<LocalState>), trackMembers: false, location);
			goto IL_025e;
			IL_0282:
			int orCreateSlot = GetOrCreateSlot(iterationVariable);
			if (orCreateSlot > 0)
			{
				SetState(ref State, orCreateSlot, value);
			}
			continue;
			IL_025e:
			SetAnalyzedNullability(node.IterationVariableType, new VisitResult(rValueType, typeWithAnnotations2), true);
			value = typeWithState2.State;
			goto IL_0282;
		}
	}

	public override BoundNode? VisitFromEndIndexExpression(BoundFromEndIndexExpression node)
	{
		BoundNode result = base.VisitFromEndIndexExpression(node);
		SetNotNullResult(node);
		return result;
	}

	public override BoundNode? VisitObjectInitializerMember(BoundObjectInitializerMember node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/NullableWalker.cs", 12015);
	}

	public override BoundNode? VisitDynamicObjectInitializerMember(BoundDynamicObjectInitializerMember node)
	{
		SetNotNullResult(node);
		return null;
	}

	public override BoundNode? VisitBadExpression(BoundBadExpression node)
	{
		foreach (BoundExpression childBoundNode in node.ChildBoundNodes)
		{
			VisitBadExpressionChild(childBoundNode);
		}
		TypeWithAnnotations type = TypeWithAnnotations.Create(node.Type);
		SetLvalueResultType(node, type);
		return null;
	}

	private TypeWithState VisitBadExpressionChild(BoundExpression? child)
	{
		if (child is BoundLambda node)
		{
			TakeIncrementalSnapshot(node);
			VisitLambda(node, null);
			VisitRvalueEpilogue(node);
		}
		else
		{
			VisitRvalue(child);
		}
		return ResultType;
	}

	public override BoundNode? VisitTypeExpression(BoundTypeExpression node)
	{
		BoundNode result = base.VisitTypeExpression(node);
		if (node.BoundContainingTypeOpt != null)
		{
			VisitTypeExpression(node.BoundContainingTypeOpt);
		}
		SetNotNullResult(node);
		return result;
	}

	public override BoundNode? VisitTypeOrValueExpression(BoundTypeOrValueExpression node)
	{
		BoundNode result = base.VisitTypeOrValueExpression(node);
		SetNotNullResult(node);
		return result;
	}

	public override BoundNode? VisitUnaryOperator(BoundUnaryOperator node)
	{
		TypeWithState type;
		switch (node.OperatorKind)
		{
		case UnaryOperatorKind.BoolLogicalNegation:
			Visit(node.Operand);
			if (IsConditionalState)
			{
				SetConditionalState(StateWhenFalse, StateWhenTrue);
			}
			type = adjustForLifting(ResultType);
			break;
		case UnaryOperatorKind.DynamicTrue:
			Visit(node.Operand);
			type = adjustForLifting(ResultType);
			break;
		case UnaryOperatorKind.DynamicLogicalNegation:
			Visit(node.Operand);
			if (IsConditionalState)
			{
				SetConditionalState(StateWhenFalse, StateWhenTrue);
			}
			type = adjustForLifting(ResultType);
			break;
		default:
			if (node.OperatorKind.IsUserDefined())
			{
				MethodSymbol methodOpt = node.MethodOpt;
				if ((object)methodOpt != null && methodOpt.ParameterCount == 1)
				{
					var (boundExpression, conversion) = RemoveConversion(node.Operand, includeExplicitConversions: false);
					VisitRvalue(boundExpression);
					TypeWithState resultType = ResultType;
					bool isLifted = node.OperatorKind.IsLifted();
					TypeWithState nullableUnderlyingTypeIfNecessary = GetNullableUnderlyingTypeIfNecessary(isLifted, resultType);
					methodOpt = ReInferUnaryOperator(node.Syntax, methodOpt, boundExpression, nullableUnderlyingTypeIfNecessary);
					ParameterSymbol parameterSymbol = methodOpt.Parameters[0];
					VisitConversion(node.Operand as BoundConversion, boundExpression, conversion, parameterSymbol.TypeWithAnnotations, nullableUnderlyingTypeIfNecessary, checkConversion: true, fromExplicitCast: false, useLegacyWarnings: false, AssignmentKind.Argument, parameterSymbol);
					type = GetLiftedReturnTypeIfNecessary(isLifted, methodOpt.ReturnTypeWithAnnotations, resultType.State);
					SetUpdatedSymbol(node, node.MethodOpt, methodOpt);
					break;
				}
			}
			VisitRvalue(node.Operand);
			type = adjustForLifting(ResultType);
			break;
		}
		SetResultType(node, type);
		return null;
		TypeWithState adjustForLifting(TypeWithState argumentResult)
		{
			return TypeWithState.Create(node.Type, node.OperatorKind.IsLifted() ? argumentResult.State : NullableFlowState.NotNull);
		}
	}

	private MethodSymbol ReInferUnaryOperator(SyntaxNode syntax, MethodSymbol method, BoundExpression operand, TypeWithState operandType)
	{
		if (!method.IsExtensionBlockMember())
		{
			method = (MethodSymbol)AsMemberOfType(operandType.Type.StrippedType(), method);
		}
		else if (method.ContainingType.Arity != 0)
		{
			NamedTypeSymbol containingType = method.OriginalDefinition.ContainingType;
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
			MethodTypeInferenceResult methodTypeInferenceResult = MethodTypeInferrer.Infer(_binder, _conversions, containingType.TypeParameters, containingType, method.OriginalDefinition.ParameterTypesWithAnnotations, method.OriginalDefinition.ParameterRefKinds, ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1]
			{
				new BoundExpressionWithNullability(operand.Syntax, operand, operandType.ToTypeWithAnnotations(compilation).NullableAnnotation, operandType.Type)
			}), ref useSiteInfo, new MethodInferenceExtensions(this));
			if (methodTypeInferenceResult.Success)
			{
				containingType = containingType.Construct(methodTypeInferenceResult.InferredTypeArguments);
				method = method.OriginalDefinition.AsMember(containingType);
			}
			CheckMethodConstraints(syntax, method);
		}
		return method;
	}

	public override BoundNode? VisitPointerIndirectionOperator(BoundPointerIndirectionOperator node)
	{
		BoundNode result = base.VisitPointerIndirectionOperator(node);
		TypeWithAnnotations type = TypeWithAnnotations.Create(node.Type);
		SetLvalueResultType(node, type);
		return result;
	}

	public override BoundNode? VisitPointerElementAccess(BoundPointerElementAccess node)
	{
		BoundNode result = base.VisitPointerElementAccess(node);
		TypeWithAnnotations type = TypeWithAnnotations.Create(node.Type);
		SetLvalueResultType(node, type);
		return result;
	}

	public override BoundNode? VisitRefTypeOperator(BoundRefTypeOperator node)
	{
		VisitRvalue(node.Operand);
		SetNotNullResult(node);
		return null;
	}

	public override BoundNode? VisitMakeRefOperator(BoundMakeRefOperator node)
	{
		BoundNode result = base.VisitMakeRefOperator(node);
		SetNotNullResult(node);
		return result;
	}

	public override BoundNode? VisitRefValueOperator(BoundRefValueOperator node)
	{
		BoundNode result = base.VisitRefValueOperator(node);
		TypeWithAnnotations type = TypeWithAnnotations.Create(node.Type, node.NullableAnnotation);
		SetLvalueResultType(node, type);
		return result;
	}

	protected override void VisitBinaryLogicalOperatorChildren(ArrayBuilder<BoundExpression> stack)
	{
		BoundExpression boundExpression = stack.Pop();
		BoundExpression boundExpression2 = null;
		Conversion leftConversion = Conversion.Identity;
		if (!(boundExpression is BoundBinaryOperator boundBinaryOperator))
		{
			if (!(boundExpression is BoundUserDefinedConditionalLogicalOperator boundUserDefinedConditionalLogicalOperator))
			{
				throw ExceptionUtilities.UnexpectedValue(boundExpression.Kind);
			}
			(boundExpression2, leftConversion) = RemoveConversion(boundUserDefinedConditionalLogicalOperator.Left, includeExplicitConversions: false);
			Visit(boundExpression2);
		}
		else
		{
			VisitCondition(boundBinaryOperator.Left);
		}
		while (true)
		{
			if (!(boundExpression is BoundBinaryOperator node))
			{
				if (!(boundExpression is BoundUserDefinedConditionalLogicalOperator binary))
				{
					throw ExceptionUtilities.UnexpectedValue(boundExpression.Kind);
				}
				afterLeftChildOfBoundUserDefinedConditionalLogicalOperatorHasBeenVisited(binary, boundExpression2, leftConversion);
			}
			else
			{
				afterLeftChildOfBoundBinaryOperatorHasBeenVisited(node);
			}
			if (stack.Count != 0)
			{
				AdjustConditionalState(boundExpression);
				boundExpression2 = boundExpression;
				leftConversion = Conversion.Identity;
				boundExpression = stack.Pop();
				continue;
			}
			break;
		}
		void afterLeftChildOfBoundBinaryOperatorHasBeenVisited(BoundBinaryOperator boundBinaryOperator2)
		{
			TypeWithState resultType = ResultType;
			getBinaryConditionalOperatorInfo(boundBinaryOperator2.OperatorKind, out var isAnd, out var isBool);
			LocalState leftTrue = StateWhenTrue;
			LocalState leftFalse = StateWhenFalse;
			SetState(isAnd ? leftTrue : leftFalse);
			Visit(boundBinaryOperator2.Right);
			TypeWithState resultType2 = ResultType;
			SetResultType(boundBinaryOperator2, InferResultNullability(boundBinaryOperator2.OperatorKind, boundBinaryOperator2.BinaryOperatorMethod, boundBinaryOperator2.Type, resultType, resultType2));
			AfterRightChildOfBinaryLogicalOperatorHasBeenVisited(boundBinaryOperator2.Right, isAnd, isBool, ref leftTrue, ref leftFalse);
		}
		void afterLeftChildOfBoundUserDefinedConditionalLogicalOperatorHasBeenVisited(BoundUserDefinedConditionalLogicalOperator boundUserDefinedConditionalLogicalOperator2, BoundExpression leftOperand, Conversion conversion2)
		{
			TypeWithState resultType = ResultType;
			Unsplit();
			Split();
			LocalState leftTrue = StateWhenTrue;
			LocalState leftFalse = StateWhenFalse;
			getBinaryConditionalOperatorInfo(boundUserDefinedConditionalLogicalOperator2.OperatorKind, out var isAnd, out var isBool);
			SetState(isAnd ? leftTrue : leftFalse);
			var (boundExpression3, conversion) = RemoveConversion(boundUserDefinedConditionalLogicalOperator2.Right, includeExplicitConversions: false);
			VisitRvalue(boundExpression3);
			TypeWithState resultType2 = ResultType;
			Unsplit();
			LocalState state = State.Clone();
			bool isLifted = boundUserDefinedConditionalLogicalOperator2.OperatorKind.IsLifted();
			TypeWithState nullableUnderlyingTypeIfNecessary = GetNullableUnderlyingTypeIfNecessary(isLifted, resultType);
			TypeWithState nullableUnderlyingTypeIfNecessary2 = GetNullableUnderlyingTypeIfNecessary(isLifted, resultType2);
			MethodSymbol logicalOperator = boundUserDefinedConditionalLogicalOperator2.LogicalOperator;
			MethodSymbol methodSymbol = ReInferBinaryOperator(boundUserDefinedConditionalLogicalOperator2.Syntax, logicalOperator, leftOperand, boundExpression3, nullableUnderlyingTypeIfNecessary, nullableUnderlyingTypeIfNecessary2);
			SetUpdatedSymbol(boundUserDefinedConditionalLogicalOperator2, logicalOperator, methodSymbol);
			logicalOperator = methodSymbol;
			LocalState state2 = (isAnd ? leftFalse : leftTrue);
			SetState(state2);
			ImmutableArray<ParameterSymbol> parameters = logicalOperator.Parameters;
			resultType = VisitBinaryOperatorOperandConversion(boundUserDefinedConditionalLogicalOperator2.Left, leftOperand, conversion2, parameters[0], nullableUnderlyingTypeIfNecessary, isLifted, out var parameterAnnotations);
			MethodSymbol methodSymbol2 = (isAnd ? boundUserDefinedConditionalLogicalOperator2.FalseOperator : boundUserDefinedConditionalLogicalOperator2.TrueOperator);
			MethodSymbol methodSymbol3 = ReInferUnaryOperator(leftOperand.Syntax, methodSymbol2, boundUserDefinedConditionalLogicalOperator2.Left, resultType);
			SetUpdatedSymbol(boundUserDefinedConditionalLogicalOperator2, methodSymbol2, methodSymbol3);
			methodSymbol2 = methodSymbol3;
			ParameterSymbol parameterSymbol = methodSymbol2.Parameters[0];
			VisitConversion(null, boundUserDefinedConditionalLogicalOperator2.Left, BoundNode.GetConversion(boundUserDefinedConditionalLogicalOperator2.TrueFalseOperandConversion, boundUserDefinedConditionalLogicalOperator2.TrueFalseOperandPlaceholder), parameterSymbol.TypeWithAnnotations, resultType, checkConversion: true, fromExplicitCast: false, useLegacyWarnings: false, AssignmentKind.Argument, parameterSymbol);
			SetState(state);
			resultType2 = VisitBinaryOperatorOperandConversion(boundUserDefinedConditionalLogicalOperator2.Right, boundExpression3, conversion, parameters[1], nullableUnderlyingTypeIfNecessary2, isLifted, out parameterAnnotations);
			SetResultType(boundUserDefinedConditionalLogicalOperator2, InferResultNullability(boundUserDefinedConditionalLogicalOperator2.OperatorKind, logicalOperator, boundUserDefinedConditionalLogicalOperator2.Type, resultType, resultType2));
			AfterRightChildOfBinaryLogicalOperatorHasBeenVisited(boundUserDefinedConditionalLogicalOperator2.Right, isAnd, isBool, ref leftTrue, ref leftFalse);
		}
		static void getBinaryConditionalOperatorInfo(BinaryOperatorKind kind, out bool isAnd, out bool isBool)
		{
			BinaryOperatorKind binaryOperatorKind = kind.Operator();
			isAnd = binaryOperatorKind == BinaryOperatorKind.And;
			isBool = kind.OperandTypes() == BinaryOperatorKind.Bool;
		}
	}

	protected override void AfterLeftChildOfBinaryLogicalOperatorHasBeenVisited(BoundExpression node, BoundExpression right, bool isAnd, bool isBool, ref LocalState leftTrue, ref LocalState leftFalse)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/NullableWalker.cs", 12375);
	}

	public override BoundNode? VisitAwaitExpression(BoundAwaitExpression node)
	{
		BoundNode result = base.VisitAwaitExpression(node);
		BoundAwaitableInfo awaitableInfo = node.AwaitableInfo;
		BoundAwaitableValuePlaceholder awaitableInstancePlaceholder = awaitableInfo.AwaitableInstancePlaceholder;
		AddPlaceholderReplacement(awaitableInstancePlaceholder, node.Expression, _visitResult);
		Visit(awaitableInfo);
		RemovePlaceholderReplacement(awaitableInstancePlaceholder);
		if (node.Type.IsValueType || node.HasErrors || (object)awaitableInfo.GetResult == null)
		{
			SetNotNullResult(node);
			return result;
		}
		MethodSymbol getResult = awaitableInfo.GetResult;
		MethodSymbol methodSymbol = ((_visitResult.RValueType.Type is NamedTypeSymbol newOwner) ? getResult.OriginalDefinition.AsMember(newOwner) : getResult);
		SetResultType(node, methodSymbol.ReturnTypeWithAnnotations.ToTypeWithState());
		return result;
	}

	public override BoundNode? VisitTypeOfOperator(BoundTypeOfOperator node)
	{
		BoundNode result = base.VisitTypeOfOperator(node);
		SetResultType(node, TypeWithState.Create(node.Type, NullableFlowState.NotNull));
		return result;
	}

	public override BoundNode? VisitMethodInfo(BoundMethodInfo node)
	{
		BoundNode result = base.VisitMethodInfo(node);
		SetNotNullResult(node);
		return result;
	}

	public override BoundNode? VisitFieldInfo(BoundFieldInfo node)
	{
		BoundNode result = base.VisitFieldInfo(node);
		SetNotNullResult(node);
		return result;
	}

	public override BoundNode? VisitDefaultLiteral(BoundDefaultLiteral node)
	{
		BoundNode result = base.VisitDefaultLiteral(node);
		SetResultType(node, TypeWithState.Create(node.Type, NullableFlowState.MaybeDefault));
		return result;
	}

	public override BoundNode? VisitDefaultExpression(BoundDefaultExpression node)
	{
		BoundNode result = base.VisitDefaultExpression(node);
		TypeSymbol type = node.Type;
		if (EmptyStructTypeCache.IsTrackableStructType(type))
		{
			int orCreatePlaceholderSlot = GetOrCreatePlaceholderSlot(node);
			if (orCreatePlaceholderSlot > 0)
			{
				SetState(ref State, orCreatePlaceholderSlot, NullableFlowState.NotNull);
				InheritNullableStateOfTrackableStruct(type, orCreatePlaceholderSlot, -1, isDefaultValue: true);
			}
		}
		SetResultType(node, TypeWithState.ForType(type));
		return result;
	}

	public override BoundNode? VisitIsOperator(BoundIsOperator node)
	{
		BoundExpression operand = node.Operand;
		BoundTypeExpression targetType = node.TargetType;
		VisitPossibleConditionalAccess(operand, out var stateWhenNotNull);
		Unsplit();
		LocalState self;
		if (!stateWhenNotNull.IsConditionalState)
		{
			self = stateWhenNotNull.State;
		}
		else
		{
			self = stateWhenNotNull.StateWhenTrue;
			Join(ref self, ref stateWhenNotNull.StateWhenFalse);
		}
		SetConditionalState(self, State);
		TypeSymbol type = targetType.Type;
		if ((object)type != null && type.SpecialType == SpecialType.System_Object)
		{
			LearnFromNullTest(operand, ref StateWhenFalse);
		}
		VisitTypeExpression(targetType);
		SetNotNullResult(node);
		return null;
	}

	public override BoundNode? VisitAsOperator(BoundAsOperator node)
	{
		TypeWithState typeWithState = VisitRvalueWithState(node.Operand);
		NullableFlowState defaultState = NullableFlowState.NotNull;
		TypeSymbol type = node.Type;
		if (type.CanContainNull())
		{
			ConversionKind kind = BoundNode.GetConversion(node.OperandConversion, node.OperandPlaceholder).Kind;
			defaultState = ((kind != ConversionKind.Identity && kind != ConversionKind.ImplicitNullable && kind - 12 > ConversionKind.NoConversion) ? NullableFlowState.MaybeDefault : typeWithState.State);
		}
		VisitTypeExpression(node.TargetType);
		SetResultType(node, TypeWithState.Create(type, defaultState));
		return null;
	}

	public override BoundNode? VisitSizeOfOperator(BoundSizeOfOperator node)
	{
		BoundNode result = base.VisitSizeOfOperator(node);
		VisitTypeExpression(node.SourceType);
		SetNotNullResult(node);
		return result;
	}

	public override BoundNode? VisitArgList(BoundArgList node)
	{
		BoundNode result = base.VisitArgList(node);
		SetNotNullResult(node);
		return result;
	}

	public override BoundNode? VisitArgListOperator(BoundArgListOperator node)
	{
		VisitArgumentsEvaluate(node.Arguments, node.ArgumentRefKindsOpt, default(ImmutableArray<FlowAnalysisAnnotations>), default(BitVector));
		SetNotNullResult(node);
		return null;
	}

	public override BoundNode? VisitLiteral(BoundLiteral node)
	{
		BoundNode result = base.VisitLiteral(node);
		TypeSymbol? type = node.Type;
		TypeSymbol? type2 = node.Type;
		int defaultState;
		if ((object)type2 == null || type2.CanContainNull())
		{
			ConstantValue? constantValueOpt = node.ConstantValueOpt;
			if ((object)constantValueOpt != null && constantValueOpt.IsNull)
			{
				defaultState = 3;
				goto IL_003b;
			}
		}
		defaultState = 0;
		goto IL_003b;
		IL_003b:
		SetResultType(node, TypeWithState.Create(type, (NullableFlowState)defaultState));
		return result;
	}

	public override BoundNode? VisitUtf8String(BoundUtf8String node)
	{
		BoundNode result = base.VisitUtf8String(node);
		SetNotNullResult(node);
		return result;
	}

	public override BoundNode? VisitPreviousSubmissionReference(BoundPreviousSubmissionReference node)
	{
		BoundNode result = base.VisitPreviousSubmissionReference(node);
		SetNotNullResult(node);
		return result;
	}

	public override BoundNode? VisitHostObjectMemberReference(BoundHostObjectMemberReference node)
	{
		BoundNode result = base.VisitHostObjectMemberReference(node);
		SetNotNullResult(node);
		return result;
	}

	public override BoundNode? VisitPseudoVariable(BoundPseudoVariable node)
	{
		BoundNode? result = base.VisitPseudoVariable(node);
		SetNotNullResult(node);
		return result;
	}

	public override BoundNode? VisitRangeExpression(BoundRangeExpression node)
	{
		BoundNode result = base.VisitRangeExpression(node);
		SetNotNullResult(node);
		return result;
	}

	public override BoundNode? VisitRangeVariable(BoundRangeVariable node)
	{
		VisitWithoutDiagnostics(node.Value);
		SetNotNullResult(node);
		return null;
	}

	public override BoundNode? VisitLabel(BoundLabel node)
	{
		BoundNode? result = base.VisitLabel(node);
		SetUnknownResultNullability(node);
		return result;
	}

	public override BoundNode? VisitDynamicMemberAccess(BoundDynamicMemberAccess node)
	{
		BoundExpression receiver = node.Receiver;
		VisitRvalue(receiver);
		CheckPossibleNullReceiver(receiver);
		TypeWithAnnotations type = TypeWithAnnotations.Create(node.Type);
		SetLvalueResultType(node, type);
		return null;
	}

	public override BoundNode? VisitDynamicInvocation(BoundDynamicInvocation node)
	{
		BoundExpression expression = node.Expression;
		VisitRvalue(expression);
		BoundExpression receiverOpt = (expression as BoundMethodGroup)?.ReceiverOpt;
		if (TryGetMethodGroupReceiverNullability(receiverOpt, out var type))
		{
			CheckPossibleNullReceiver(receiverOpt, type, checkNullableValueType: false);
		}
		VisitArgumentsEvaluate(node.Arguments, node.ArgumentRefKindsOpt, default(ImmutableArray<FlowAnalysisAnnotations>), default(BitVector));
		TypeWithAnnotations type2 = TypeWithAnnotations.Create(node.Type);
		SetLvalueResultType(node, type2);
		return null;
	}

	public override BoundNode? VisitEventAssignmentOperator(BoundEventAssignmentOperator node)
	{
		BoundExpression receiverOpt = node.ReceiverOpt;
		VisitRvalue(receiverOpt);
		EventSymbol eventSymbol = node.Event;
		if (!eventSymbol.IsStatic)
		{
			eventSymbol = (EventSymbol)AsMemberOfType(ResultType.Type, eventSymbol);
			CheckPossibleNullReceiver(receiverOpt);
			SetUpdatedSymbol(node, node.Event, eventSymbol);
		}
		VisitRvalue(node.Argument);
		ConstantValue? constantValueOpt = node.Argument.ConstantValueOpt;
		if ((object)constantValueOpt == null || !constantValueOpt.IsNull)
		{
			int num = MakeMemberSlot(receiverOpt, eventSymbol);
			if (num > 0)
			{
				SetState(ref State, num, (!node.IsAddition) ? NullableFlowState.MaybeNull : GetState(ref State, num).Meet(ResultType.State));
			}
		}
		SetNotNullResult(node);
		return null;
	}

	public override BoundNode? VisitDynamicObjectCreationExpression(BoundDynamicObjectCreationExpression node)
	{
		VisitObjectCreationExpressionBase(node);
		return null;
	}

	public override BoundNode? VisitObjectInitializerExpression(BoundObjectInitializerExpression node)
	{
		SetNotNullResult(node);
		return null;
	}

	public override BoundNode? VisitCollectionInitializerExpression(BoundCollectionInitializerExpression node)
	{
		SetNotNullResult(node);
		return null;
	}

	public override BoundNode? VisitDynamicCollectionElementInitializer(BoundDynamicCollectionElementInitializer node)
	{
		SetNotNullResult(node);
		return null;
	}

	public override BoundNode? VisitImplicitReceiver(BoundImplicitReceiver node)
	{
		BoundNode result = base.VisitImplicitReceiver(node);
		SetNotNullResult(node);
		return result;
	}

	public override BoundNode? VisitAnonymousPropertyDeclaration(BoundAnonymousPropertyDeclaration node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/NullableWalker.cs", 12701);
	}

	public override BoundNode? VisitNoPiaObjectCreationExpression(BoundNoPiaObjectCreationExpression node)
	{
		VisitObjectCreationExpressionBase(node);
		return null;
	}

	public override BoundNode? VisitNewT(BoundNewT node)
	{
		VisitObjectCreationExpressionBase(node);
		return null;
	}

	public override BoundNode? VisitArrayInitialization(BoundArrayInitialization node)
	{
		BoundNode result = base.VisitArrayInitialization(node);
		SetNotNullResult(node);
		return result;
	}

	private void SetUnknownResultNullability(BoundExpression expression)
	{
		SetResultType(expression, TypeWithState.Create(expression.Type, NullableFlowState.NotNull));
	}

	public override BoundNode? VisitDynamicIndexerAccess(BoundDynamicIndexerAccess node)
	{
		BoundExpression receiver = node.Receiver;
		VisitRvalue(receiver);
		CheckPossibleNullReceiver(receiver);
		VisitArgumentsEvaluate(node.Arguments, node.ArgumentRefKindsOpt, default(ImmutableArray<FlowAnalysisAnnotations>), default(BitVector));
		TypeWithAnnotations type = TypeWithAnnotations.Create(node.Type);
		SetLvalueResultType(node, type);
		return null;
	}

	private bool CheckPossibleNullReceiver(BoundExpression? receiverOpt, bool checkNullableValueType = false)
	{
		return CheckPossibleNullReceiver(receiverOpt, ResultType, checkNullableValueType);
	}

	private bool CheckPossibleNullReceiver(BoundExpression? receiverOpt, TypeWithState resultType, bool checkNullableValueType)
	{
		bool reportedDiagnostic = false;
		if (receiverOpt != null && State.Reachable)
		{
			TypeSymbol type = resultType.Type;
			if ((object)type == null)
			{
				return false;
			}
			if (!ReportPossibleNullReceiverIfNeeded(type, resultType.State, checkNullableValueType, receiverOpt.Syntax, out reportedDiagnostic))
			{
				return reportedDiagnostic;
			}
			LearnFromNonNullTest(receiverOpt, ref State);
		}
		return reportedDiagnostic;
	}

	private bool ReportPossibleNullReceiverIfNeeded(TypeSymbol type, NullableFlowState state, bool checkNullableValueType, SyntaxNode syntax, out bool reportedDiagnostic)
	{
		reportedDiagnostic = false;
		if (state.MayBeNull())
		{
			bool isValueType = type.IsValueType;
			if (isValueType && (!checkNullableValueType || !type.IsNullableTypeOrTypeParameter() || type.GetNullableUnderlyingType().IsErrorType()))
			{
				return false;
			}
			ReportDiagnostic(isValueType ? ErrorCode.WRN_NullableValueTypeMayBeNull : ErrorCode.WRN_NullReferenceReceiver, syntax);
			reportedDiagnostic = true;
		}
		return true;
	}

	private void CheckExtensionMethodThisNullability(BoundExpression expr, Conversion conversion, ParameterSymbol parameter, TypeWithState result)
	{
		VisitArgumentConversionAndInboundAssignmentsAndPreConditions(null, expr, conversion, parameter.RefKind, parameter, parameter.TypeWithAnnotations, GetParameterAnnotations(parameter), new VisitResult(result, result.ToTypeWithAnnotations(compilation)), null, extensionMethodThisArgument: true);
	}

	private static bool IsNullabilityMismatch(TypeWithAnnotations type1, TypeWithAnnotations type2)
	{
		if (type1.Equals(type2, TypeCompareKind.AllIgnoreOptions))
		{
			return !type1.Equals(type2, TypeCompareKind.AllIgnoreOptionsPlusNullableWithObliviousMatchesAny);
		}
		return false;
	}

	private static bool IsNullabilityMismatch(TypeSymbol type1, TypeSymbol type2)
	{
		if (type1.Equals(type2, TypeCompareKind.AllIgnoreOptions))
		{
			return !type1.Equals(type2, TypeCompareKind.AllIgnoreOptionsPlusNullableWithObliviousMatchesAny);
		}
		return false;
	}

	public override BoundNode? VisitQueryClause(BoundQueryClause node)
	{
		BoundNode result = base.VisitQueryClause(node);
		SetNotNullResult(node);
		return result;
	}

	public override BoundNode? VisitNameOfOperator(BoundNameOfOperator node)
	{
		BoundNode result = base.VisitNameOfOperator(node);
		SetResultType(node, TypeWithState.Create(node.Type, NullableFlowState.NotNull));
		return result;
	}

	public override BoundNode? VisitNamespaceExpression(BoundNamespaceExpression node)
	{
		BoundNode result = base.VisitNamespaceExpression(node);
		SetUnknownResultNullability(node);
		return result;
	}

	public override BoundNode? VisitUnconvertedInterpolatedString(BoundUnconvertedInterpolatedString node)
	{
		BoundNode result = base.VisitUnconvertedInterpolatedString(node);
		SetResultType(node, TypeWithState.Create(node.Type, NullableFlowState.NotNull));
		return result;
	}

	public override BoundNode? VisitStringInsert(BoundStringInsert node)
	{
		BoundNode result = base.VisitStringInsert(node);
		SetUnknownResultNullability(node);
		return result;
	}

	protected override void VisitInterpolatedStringHandlerConstructor(BoundExpression? constructor)
	{
	}

	public override BoundNode? VisitInterpolatedStringHandlerPlaceholder(BoundInterpolatedStringHandlerPlaceholder node)
	{
		SetNotNullResult(node);
		return null;
	}

	public override BoundNode? VisitInterpolatedStringArgumentPlaceholder(BoundInterpolatedStringArgumentPlaceholder node)
	{
		VisitPlaceholderWithReplacement(node);
		return null;
	}

	public override BoundNode? VisitStackAllocArrayCreation(BoundStackAllocArrayCreation node)
	{
		return VisitStackAllocArrayCreationBase(node);
	}

	public override BoundNode? VisitConvertedStackAllocExpression(BoundConvertedStackAllocExpression node)
	{
		return VisitStackAllocArrayCreationBase(node);
	}

	private BoundNode? VisitStackAllocArrayCreationBase(BoundStackAllocArrayCreationBase node)
	{
		VisitRvalue(node.Count);
		BoundArrayInitialization initializerOpt = node.InitializerOpt;
		if (initializerOpt == null)
		{
			SetResultType(node, TypeWithState.Create(node.Type, NullableFlowState.NotNull));
			return null;
		}
		TypeSymbol type = VisitArrayInitialization(node.Type, initializerOpt, node.HasErrors);
		SetResultType(node, TypeWithState.Create(type, NullableFlowState.NotNull));
		return null;
	}

	public override BoundNode? VisitDiscardExpression(BoundDiscardExpression node)
	{
		TypeWithAnnotations lvalueType = TypeWithAnnotations.Create(node.Type, node.IsInferred ? NullableAnnotation.Annotated : node.NullableAnnotation);
		TypeWithState resultType = TypeWithState.ForType(node.Type);
		SetResult(node, resultType, lvalueType);
		return null;
	}

	public override BoundNode? VisitThrowExpression(BoundThrowExpression node)
	{
		VisitThrow(node.Expression);
		SetResultType(node, default(TypeWithState));
		return null;
	}

	public override BoundNode? VisitThrowStatement(BoundThrowStatement node)
	{
		VisitThrow(node.ExpressionOpt);
		return null;
	}

	private void VisitThrow(BoundExpression? expr)
	{
		if (expr != null && VisitRvalueWithState(expr).MayBeNull)
		{
			ReportDiagnostic(ErrorCode.WRN_ThrowPossibleNull, expr.Syntax);
		}
		SetUnreachable();
	}

	public override BoundNode? VisitYieldReturnStatement(BoundYieldReturnStatement node)
	{
		BoundExpression expression = node.Expression;
		if (expression == null)
		{
			return null;
		}
		MethodSymbol methodSymbol = (MethodSymbol)CurrentSymbol;
		TypeWithAnnotations iteratorElementTypeFromReturnType = InMethodBinder.GetIteratorElementTypeFromReturnType(compilation, RefKind.None, methodSymbol.ReturnType, null, null);
		VisitOptionalImplicitConversion(expression, iteratorElementTypeFromReturnType, useLegacyWarnings: false, trackMembers: false, AssignmentKind.Return);
		Unsplit();
		return null;
	}

	public override BoundNode? VisitCatchBlock(BoundCatchBlock node)
	{
		TakeIncrementalSnapshot(node);
		if (node.Locals.Length > 0)
		{
			LocalSymbol localSymbol = node.Locals[0];
			if (localSymbol.DeclarationKind == LocalDeclarationKind.CatchVariable)
			{
				int orCreateSlot = GetOrCreateSlot(localSymbol);
				if (orCreateSlot > 0)
				{
					SetState(ref State, orCreateSlot, NullableFlowState.NotNull);
				}
			}
		}
		if (node.ExceptionSourceOpt != null)
		{
			VisitWithoutDiagnostics(node.ExceptionSourceOpt);
		}
		base.VisitCatchBlock(node);
		return null;
	}

	public override BoundNode? VisitLockStatement(BoundLockStatement node)
	{
		VisitRvalue(node.Argument);
		CheckPossibleNullReceiver(node.Argument);
		VisitStatement(node.Body);
		return null;
	}

	public override BoundNode? VisitAttribute(BoundAttribute node)
	{
		VisitArguments(node, node.ConstructorArguments, ImmutableArray<RefKind>.Empty, node.Constructor, node.ConstructorArgumentsToParamsOpt, node.ConstructorDefaultArguments, node.ConstructorExpanded, invokedAsExtensionMethod: false);
		foreach (BoundAssignmentOperator namedArgument in node.NamedArguments)
		{
			Visit(namedArgument);
		}
		SetNotNullResult(node);
		return null;
	}

	public override BoundNode? VisitExpressionWithNullability(BoundExpressionWithNullability node)
	{
		TypeWithAnnotations lvalueType = TypeWithAnnotations.Create(node.Type, node.NullableAnnotation);
		SetResult(node.Expression, lvalueType.ToTypeWithState(), lvalueType);
		return null;
	}

	public override BoundNode? VisitDeconstructValuePlaceholder(BoundDeconstructValuePlaceholder node)
	{
		SetNotNullResult(node);
		return null;
	}

	public override BoundNode? VisitObjectOrCollectionValuePlaceholder(BoundObjectOrCollectionValuePlaceholder node)
	{
		SetNotNullResult(node);
		return null;
	}

	public override BoundNode? VisitAwaitableValuePlaceholder(BoundAwaitableValuePlaceholder node)
	{
		VisitPlaceholderWithReplacement(node);
		return null;
	}

	private void VisitPlaceholderWithReplacement(BoundValuePlaceholderBase node)
	{
		if (_resultForPlaceholdersOpt != null && _resultForPlaceholdersOpt.TryGetValue(node, out (BoundExpression, VisitResult) value))
		{
			VisitResult item = value.Item2;
			SetResult(node, item.RValueType, item.LValueType);
		}
		else
		{
			SetNotNullResult(node);
		}
	}

	public override BoundNode? VisitAwaitableInfo(BoundAwaitableInfo node)
	{
		Visit(node.AwaitableInstancePlaceholder);
		Visit(node.GetAwaiter);
		return null;
	}

	public override BoundNode? VisitFunctionPointerInvocation(BoundFunctionPointerInvocation node)
	{
		Visit(node.InvokedExpression);
		VisitArguments(node, node.Arguments, node.ArgumentRefKindsOpt, node.FunctionPointer.Signature, default(ImmutableArray<int>), default(BitVector), expanded: false, invokedAsExtensionMethod: false);
		TypeWithAnnotations returnTypeWithAnnotations = node.FunctionPointer.Signature.ReturnTypeWithAnnotations;
		SetResult(node, returnTypeWithAnnotations.ToTypeWithState(), returnTypeWithAnnotations);
		return null;
	}

	protected override string Dump(LocalState state)
	{
		return state.Dump(_variables);
	}

	protected override bool Meet(ref LocalState self, ref LocalState other)
	{
		if (!self.Reachable)
		{
			return false;
		}
		if (!other.Reachable)
		{
			self = other.Clone();
			return true;
		}
		Normalize(ref self);
		Normalize(ref other);
		return self.Meet(in other);
	}

	protected override bool Join(ref LocalState self, ref LocalState other)
	{
		if (!other.Reachable)
		{
			return false;
		}
		if (!self.Reachable)
		{
			self = other.Clone();
			return true;
		}
		Normalize(ref self);
		Normalize(ref other);
		return self.Join(in other);
	}

	private void Join(ref PossiblyConditionalState other)
	{
		bool isConditionalState = other.IsConditionalState;
		if (isConditionalState)
		{
			Split();
		}
		if (IsConditionalState)
		{
			Join(ref StateWhenTrue, ref isConditionalState ? ref other.StateWhenTrue : ref other.State);
			Join(ref StateWhenFalse, ref isConditionalState ? ref other.StateWhenFalse : ref other.State);
		}
		else
		{
			Join(ref State, ref other.State);
		}
	}

	private LocalState CloneAndUnsplit(ref PossiblyConditionalState conditionalState)
	{
		if (!conditionalState.IsConditionalState)
		{
			return conditionalState.State.Clone();
		}
		LocalState self = conditionalState.StateWhenTrue.Clone();
		Join(ref self, ref conditionalState.StateWhenFalse);
		return self;
	}

	private void SetPossiblyConditionalState(in PossiblyConditionalState conditionalState)
	{
		if (!conditionalState.IsConditionalState)
		{
			SetState(conditionalState.State);
		}
		else
		{
			SetConditionalState(conditionalState.StateWhenTrue, conditionalState.StateWhenFalse);
		}
	}

	protected override LocalFunctionState CreateLocalFunctionState(LocalFunctionSymbol symbol)
	{
		return new LocalFunctionState(LocalState.UnreachableState(((symbol.ContainingSymbol is MethodSymbol method) ? _variables.GetVariablesForMethodScope(method) : null) ?? _variables.GetRootScope()));
	}

	private void LearnFromAnyNullPatterns(BoundExpression expression, BoundPattern pattern)
	{
		int inputSlot = MakeSlot(expression);
		LearnFromAnyNullPatterns(inputSlot, expression.Type, pattern);
	}

	private void VisitForRewriting(BoundNode node)
	{
		LocalState state = State;
		VisitWithoutDiagnostics(node);
		SetState(state);
	}

	public override BoundNode VisitPositionalSubpattern(BoundPositionalSubpattern node)
	{
		Visit(node.Pattern);
		return null;
	}

	public override BoundNode VisitPropertySubpattern(BoundPropertySubpattern node)
	{
		Visit(node.Pattern);
		return null;
	}

	public override BoundNode VisitRecursivePattern(BoundRecursivePattern node)
	{
		Visit(node.DeclaredType);
		VisitAndUnsplitAll(node.Deconstruction);
		VisitAndUnsplitAll(node.Properties);
		Visit(node.VariableAccess);
		return null;
	}

	public override BoundNode VisitConstantPattern(BoundConstantPattern node)
	{
		VisitRvalue(node.Value);
		return null;
	}

	public override BoundNode VisitDeclarationPattern(BoundDeclarationPattern node)
	{
		Visit(node.VariableAccess);
		Visit(node.DeclaredType);
		return null;
	}

	public override BoundNode VisitDiscardPattern(BoundDiscardPattern node)
	{
		return null;
	}

	public override BoundNode VisitSlicePattern(BoundSlicePattern node)
	{
		Visit(node.Pattern);
		return null;
	}

	public override BoundNode VisitListPattern(BoundListPattern node)
	{
		VisitAndUnsplitAll(node.Subpatterns);
		Visit(node.VariableAccess);
		return null;
	}

	public override BoundNode VisitTypePattern(BoundTypePattern node)
	{
		Visit(node.DeclaredType);
		return null;
	}

	public override BoundNode VisitRelationalPattern(BoundRelationalPattern node)
	{
		Visit(node.Value);
		return null;
	}

	public override BoundNode VisitNegatedPattern(BoundNegatedPattern node)
	{
		Visit(node.Negated);
		return null;
	}

	public override BoundNode VisitBinaryPattern(BoundBinaryPattern node)
	{
		ArrayBuilder<BoundBinaryPattern> instance = ArrayBuilder<BoundBinaryPattern>.GetInstance();
		BoundBinaryPattern boundBinaryPattern = node;
		do
		{
			instance.Push(boundBinaryPattern);
			boundBinaryPattern = boundBinaryPattern.Left as BoundBinaryPattern;
		}
		while (boundBinaryPattern != null);
		boundBinaryPattern = instance.Pop();
		TakeIncrementalSnapshot(boundBinaryPattern);
		Visit(boundBinaryPattern.Left);
		do
		{
			Visit(boundBinaryPattern.Right);
		}
		while (instance.TryPop(out boundBinaryPattern));
		instance.Free();
		return null;
	}

	public override BoundNode VisitITuplePattern(BoundITuplePattern node)
	{
		VisitAndUnsplitAll(node.Subpatterns);
		return null;
	}

	private void LearnFromAnyNullPatterns(int inputSlot, TypeSymbol inputType, BoundPattern pattern)
	{
		if (inputSlot <= 0)
		{
			return;
		}
		VisitForRewriting(pattern);
		if (!(pattern is BoundConstantPattern boundConstantPattern))
		{
			if (pattern is BoundDeclarationPattern || pattern is BoundDiscardPattern || pattern is BoundITuplePattern || pattern is BoundRelationalPattern || pattern is BoundSlicePattern || pattern is BoundListPattern)
			{
				return;
			}
			if (!(pattern is BoundTypePattern boundTypePattern))
			{
				if (!(pattern is BoundRecursivePattern boundRecursivePattern))
				{
					if (!(pattern is BoundNegatedPattern boundNegatedPattern))
					{
						if (!(pattern is BoundBinaryPattern boundBinaryPattern))
						{
							throw ExceptionUtilities.UnexpectedValue(pattern);
						}
						BoundBinaryPattern boundBinaryPattern2 = boundBinaryPattern;
						while (true)
						{
							LearnFromAnyNullPatterns(inputSlot, inputType, boundBinaryPattern2.Right);
							if (!(boundBinaryPattern2.Left is BoundBinaryPattern boundBinaryPattern3))
							{
								break;
							}
							boundBinaryPattern2 = boundBinaryPattern3;
							VisitForRewriting(boundBinaryPattern2);
						}
						LearnFromAnyNullPatterns(inputSlot, inputType, boundBinaryPattern2.Left);
					}
					else
					{
						LearnFromAnyNullPatterns(inputSlot, inputType, boundNegatedPattern.Negated);
					}
					return;
				}
				if (boundRecursivePattern.IsExplicitNotNullTest)
				{
					LearnFromNullTest(inputSlot, inputType, ref State, markDependentSlotsNotNull: false);
				}
				if ((object)boundRecursivePattern.DeconstructMethod == null && !boundRecursivePattern.Deconstruction.IsDefault)
				{
					ImmutableArray<FieldSymbol> tupleElements = inputType.TupleElements;
					int i = 0;
					for (int num = Math.Min(boundRecursivePattern.Deconstruction.Length, (!tupleElements.IsDefault) ? tupleElements.Length : 0); i < num; i++)
					{
						BoundSubpattern boundSubpattern = boundRecursivePattern.Deconstruction[i];
						FieldSymbol fieldSymbol = tupleElements[i];
						LearnFromAnyNullPatterns(GetOrCreateSlot(fieldSymbol, inputSlot), fieldSymbol.Type, boundSubpattern.Pattern);
					}
				}
				if (boundRecursivePattern.Properties.IsDefault)
				{
					return;
				}
				foreach (BoundPropertySubpattern property in boundRecursivePattern.Properties)
				{
					BoundPropertySubpatternMember member = property.Member;
					if (member != null)
					{
						LearnFromAnyNullPatterns(getExtendedPropertySlot(member, inputSlot), member.Type, property.Pattern);
					}
				}
			}
			else if (boundTypePattern.IsExplicitNotNullTest)
			{
				LearnFromNullTest(inputSlot, inputType, ref State, markDependentSlotsNotNull: false);
			}
		}
		else if (boundConstantPattern.Value.ConstantValueOpt == ConstantValue.Null)
		{
			LearnFromNullTest(inputSlot, inputType, ref State, markDependentSlotsNotNull: false);
		}
		int getExtendedPropertySlot(BoundPropertySubpatternMember boundPropertySubpatternMember, int num2)
		{
			if ((object)boundPropertySubpatternMember.Symbol == null)
			{
				return -1;
			}
			if (boundPropertySubpatternMember.Receiver != null)
			{
				num2 = getExtendedPropertySlot(boundPropertySubpatternMember.Receiver, num2);
			}
			if (num2 < 0)
			{
				return num2;
			}
			SymbolKind kind = boundPropertySubpatternMember.Symbol.Kind;
			if ((kind != SymbolKind.Field && kind != SymbolKind.Property) || 1 == 0)
			{
				return -1;
			}
			return GetOrCreateSlot(boundPropertySubpatternMember.Symbol, num2);
		}
	}

	protected override LocalState VisitSwitchStatementDispatch(BoundSwitchStatement node)
	{
		int slotForSwitchInputValue = GetSlotForSwitchInputValue(node.Expression);
		if (slotForSwitchInputValue > 0)
		{
			TypeSymbol type = node.Expression.Type;
			foreach (BoundSwitchSection switchSection in node.SwitchSections)
			{
				foreach (BoundSwitchLabel switchLabel in switchSection.SwitchLabels)
				{
					LearnFromAnyNullPatterns(slotForSwitchInputValue, type, switchLabel.Pattern);
				}
			}
		}
		DeclareLocals(node.InnerLocals);
		foreach (BoundSwitchSection switchSection2 in node.SwitchSections)
		{
			DeclareLocals(switchSection2.Locals);
		}
		Visit(node.Expression);
		TypeWithState resultType = ResultType;
		PooledDictionary<LabelSymbol, (LocalState, bool)> pooledDictionary = LearnFromDecisionDag(node.Syntax, node.ReachabilityDecisionDag, node.Expression, resultType, null);
		foreach (BoundSwitchSection switchSection3 in node.SwitchSections)
		{
			foreach (BoundSwitchLabel switchLabel2 in switchSection3.SwitchLabels)
			{
				SetState((pooledDictionary.TryGetValue(switchLabel2.Label, out var value) ? value : (UnreachableState(), false)).Item1);
				base.PendingBranches.Add(new AbstractFlowPass<LocalState, LocalFunctionState>.PendingBranch(switchLabel2, State, switchLabel2.Label));
			}
		}
		LocalState result = (pooledDictionary.TryGetValue(node.BreakLabel, out var value2) ? value2.Item1 : UnreachableState());
		pooledDictionary.Free();
		return result;
	}

	protected override void VisitSwitchSection(BoundSwitchSection node, bool isLastSection)
	{
		TakeIncrementalSnapshot(node);
		SetState(UnreachableState());
		foreach (BoundSwitchLabel switchLabel in node.SwitchLabels)
		{
			TakeIncrementalSnapshot(switchLabel);
			VisitForRewriting(switchLabel.Pattern);
			if (!State.Reachable && switchLabel.WhenClause != null)
			{
				VisitForRewriting(switchLabel.WhenClause);
			}
			VisitLabel(switchLabel.Label, node);
		}
		VisitStatementList(node);
	}

	private PooledDictionary<LabelSymbol, (LocalState state, bool believedReachable)> LearnFromDecisionDag(SyntaxNode node, BoundDecisionDag decisionDag, BoundExpression expression, TypeWithState expressionTypeWithState, PossiblyConditionalState? stateWhenNotNullOpt)
	{
		BoundDagTemp boundDagTemp = BoundDagTemp.ForOriginalInput(expression);
		int originalInputSlot = MakeSlot(expression);
		TypeWithAnnotations typeWithAnnotations = expressionTypeWithState.ToTypeWithAnnotations(compilation);
		if (originalInputSlot <= 0)
		{
			originalInputSlot = makeDagTempSlot(typeWithAnnotations, boundDagTemp);
			if (!IsConditionalState)
			{
				TrackNullableStateForAssignment(null, typeWithAnnotations, originalInputSlot, expressionTypeWithState);
			}
		}
		ImmutableArray<int> immutableArray = ((expression is BoundTupleExpression boundTupleExpression) ? boundTupleExpression.Arguments.SelectAsArray((BoundExpression a, NullableWalker w) => w.GetSlotForSwitchInputValue(a), this) : default(ImmutableArray<int>));
		PooledDictionary<int, BoundExpression> originalInputMap = PooledDictionary<int, BoundExpression>.GetInstance();
		originalInputMap.Add(originalInputSlot, expression);
		PooledDictionary<BoundDagTemp, (int slot, TypeSymbol type)> tempMap = PooledDictionary<BoundDagTemp, (int, TypeSymbol)>.GetInstance();
		tempMap.Add(boundDagTemp, (originalInputSlot, expressionTypeWithState.Type));
		PooledDictionary<BoundDecisionDagNode, (PossiblyConditionalState state, bool believedReachable)> nodeStateMap = PooledDictionary<BoundDecisionDagNode, (PossiblyConditionalState, bool)>.GetInstance();
		nodeStateMap.Add(decisionDag.RootNode, (PossiblyConditionalState.Create(this), true));
		PooledDictionary<LabelSymbol, (LocalState, bool)> instance = PooledDictionary<LabelSymbol, (LocalState, bool)>.GetInstance();
		foreach (BoundDecisionDagNode topologicallySortedNode in decisionDag.TopologicallySortedNodes)
		{
			nodeStateMap.TryGetValue(topologicallySortedNode, out (PossiblyConditionalState, bool) value);
			var (possiblyConditionalState, flag) = value;
			if (possiblyConditionalState.IsConditionalState)
			{
				SetConditionalState(possiblyConditionalState.StateWhenTrue, possiblyConditionalState.StateWhenFalse);
			}
			else
			{
				SetState(possiblyConditionalState.State);
			}
			if (!(topologicallySortedNode is BoundEvaluationDecisionDagNode boundEvaluationDecisionDagNode))
			{
				if (!(topologicallySortedNode is BoundTestDecisionDagNode boundTestDecisionDagNode))
				{
					if (!(topologicallySortedNode is BoundLeafDecisionDagNode boundLeafDecisionDagNode))
					{
						if (!(topologicallySortedNode is BoundWhenDecisionDagNode boundWhenDecisionDagNode))
						{
							throw ExceptionUtilities.UnexpectedValue(topologicallySortedNode.Kind);
						}
						Unsplit();
						foreach (BoundPatternBinding binding in boundWhenDecisionDagNode.Bindings)
						{
							BoundExpression variableAccess = binding.VariableAccess;
							BoundDagTemp tempContainingValue = binding.TempContainingValue;
							if (!tempMap.TryGetValue(tempContainingValue, out (int, TypeSymbol) value2))
							{
								continue;
							}
							(int, TypeSymbol) tuple2 = value2;
							int item = tuple2.Item1;
							TypeSymbol item2 = tuple2.Item2;
							NullableFlowState state = GetState(ref State, item);
							if (variableAccess is BoundLocal { LocalSymbol: SourceLocalSymbol localSymbol } boundLocal)
							{
								TypeWithAnnotations typeWithAnnotations2 = TypeWithState.Create(item2, state).ToTypeWithAnnotations(compilation, boundLocal.DeclarationKind == BoundLocalDeclarationKind.WithInferredType);
								if (_variables.TryGetType(localSymbol, out var type))
								{
									typeWithAnnotations2 = TypeWithAnnotations.Create(typeWithAnnotations2.Type, type.NullableAnnotation.Join(typeWithAnnotations2.NullableAnnotation));
								}
								_variables.SetType(localSymbol, typeWithAnnotations2);
								int orCreateSlot = GetOrCreateSlot(localSymbol, 0, forceSlotEvenIfEmpty: true);
								if (orCreateSlot > 0)
								{
									TrackNullableStateForAssignment(null, typeWithAnnotations2, orCreateSlot, TypeWithState.Create(item2, state), item);
								}
							}
						}
						if (boundWhenDecisionDagNode.WhenExpression != null && boundWhenDecisionDagNode.WhenExpression.ConstantValueOpt != ConstantValue.True)
						{
							VisitCondition(boundWhenDecisionDagNode.WhenExpression);
							gotoNode(boundWhenDecisionDagNode.WhenTrue, StateWhenTrue, flag);
							gotoNode(boundWhenDecisionDagNode.WhenFalse, StateWhenFalse, flag);
						}
						else
						{
							gotoNode(boundWhenDecisionDagNode.WhenTrue, State, flag);
						}
					}
					else
					{
						Unsplit();
						instance.Add(boundLeafDecisionDagNode.Label, (State, flag));
					}
					continue;
				}
				BoundDagTest test = boundTestDecisionDagNode.Test;
				tempMap.TryGetValue(test.Input, out (int, TypeSymbol) value3);
				var (num, expressionType) = value3;
				Split();
				if (!(test is BoundDagTypeTest))
				{
					if (!(test is BoundDagNonNullTest boundDagNonNullTest))
					{
						if (!(test is BoundDagExplicitNullTest))
						{
							if (!(test is BoundDagValueTest boundDagValueTest))
							{
								if (test is BoundDagRelationalTest)
								{
									if (num > 0)
									{
										learnFromNonNullTest(num, ref StateWhenTrue);
									}
									gotoNode(boundTestDecisionDagNode.WhenTrue, StateWhenTrue, flag);
									gotoNode(boundTestDecisionDagNode.WhenFalse, StateWhenFalse, flag);
									continue;
								}
								throw ExceptionUtilities.UnexpectedValue(test.Kind);
							}
							if (stateWhenNotNullOpt.HasValue)
							{
								PossiblyConditionalState conditionalState = stateWhenNotNullOpt.GetValueOrDefault();
								BoundDagEvaluation source = boundDagValueTest.Input.Source;
								if (source is BoundDagTypeEvaluation)
								{
									BoundDagTemp input = source.Input;
									if (input != null && input.IsOriginalInput)
									{
										SetPossiblyConditionalState(in conditionalState);
										Split();
										goto IL_08b8;
									}
								}
							}
							if (num > 0)
							{
								learnFromNonNullTest(num, ref StateWhenTrue);
							}
							goto IL_08b8;
						}
						if (num > 0)
						{
							LearnFromNullTest(num, expressionType, ref StateWhenTrue, markDependentSlotsNotNull: true);
							learnFromNonNullTest(num, ref StateWhenFalse);
						}
						gotoNode(boundTestDecisionDagNode.WhenTrue, StateWhenTrue, flag);
						gotoNode(boundTestDecisionDagNode.WhenFalse, StateWhenFalse, flag);
						continue;
					}
					bool flag2 = GetState(ref StateWhenTrue, num).MayBeNull();
					if (num > 0)
					{
						MarkDependentSlotsNotNull(num, expressionType, ref StateWhenFalse);
						if (boundDagNonNullTest.IsExplicitTest)
						{
							LearnFromNullTest(num, expressionType, ref StateWhenFalse, markDependentSlotsNotNull: false);
						}
						learnFromNonNullTest(num, ref StateWhenTrue);
					}
					gotoNode(boundTestDecisionDagNode.WhenTrue, StateWhenTrue, flag);
					gotoNode(boundTestDecisionDagNode.WhenFalse, StateWhenFalse, flag & flag2);
					continue;
				}
				if (num > 0)
				{
					learnFromNonNullTest(num, ref StateWhenTrue);
				}
				gotoNode(boundTestDecisionDagNode.WhenTrue, StateWhenTrue, flag);
				gotoNode(boundTestDecisionDagNode.WhenFalse, StateWhenFalse, flag);
				continue;
			}
			BoundDagEvaluation evaluation = boundEvaluationDecisionDagNode.Evaluation;
			if (!tempMap.TryGetValue(evaluation.Input, out (int, TypeSymbol) value4))
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/NullableWalker_Patterns.cs", 437);
			}
			var (num2, typeSymbol) = value4;
			BoundDagTemp boundDagTemp2;
			int num3;
			if (!(evaluation is BoundDagDeconstructEvaluation boundDagDeconstructEvaluation))
			{
				if (evaluation is BoundDagTypeEvaluation boundDagTypeEvaluation)
				{
					boundDagTemp2 = new BoundDagTemp(boundDagTypeEvaluation.Syntax, boundDagTypeEvaluation.Type, boundDagTypeEvaluation);
					CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
					ConversionKind kind = _conversions.WithNullability(includeNullability: false).ClassifyConversionFromType(typeSymbol, boundDagTypeEvaluation.Type, isChecked: false, ref useSiteInfo).Kind;
					if (kind != ConversionKind.Identity && kind != ConversionKind.ImplicitReference)
					{
						if (kind == ConversionKind.ExplicitNullable && AreNullableAndUnderlyingTypes(typeSymbol, boundDagTypeEvaluation.Type, out var _))
						{
							num3 = GetNullableOfTValueSlot(typeSymbol, num2, out Symbol _, forceSlotEvenIfEmpty: true);
							if (num3 >= 0)
							{
								goto IL_03d1;
							}
						}
						num3 = makeDagTempSlot(TypeWithAnnotations.Create(boundDagTypeEvaluation.Type, NullableAnnotation.NotAnnotated), boundDagTemp2);
					}
					else
					{
						num3 = num2;
					}
					goto IL_03d1;
				}
				if (!(evaluation is BoundDagFieldEvaluation boundDagFieldEvaluation))
				{
					if (!(evaluation is BoundDagPropertyEvaluation boundDagPropertyEvaluation))
					{
						if (!(evaluation is BoundDagIndexEvaluation boundDagIndexEvaluation))
						{
							if (!(evaluation is BoundDagIndexerEvaluation boundDagIndexerEvaluation))
							{
								if (!(evaluation is BoundDagSliceEvaluation boundDagSliceEvaluation))
								{
									if (!(evaluation is BoundDagAssignmentEvaluation))
									{
										throw ExceptionUtilities.UnexpectedValue(boundEvaluationDecisionDagNode.Evaluation.Kind);
									}
								}
								else
								{
									TypeWithAnnotations type2 = getIndexerOutputType(typeSymbol, boundDagSliceEvaluation.IndexerAccess, isSlice: true);
									BoundDagTemp boundDagTemp3 = new BoundDagTemp(boundDagSliceEvaluation.Syntax, type2.Type, boundDagSliceEvaluation);
									int slot = makeDagTempSlot(type2, boundDagTemp3);
									addToTempMap(boundDagTemp3, slot, type2.Type);
									SetState(ref State, slot, NullableFlowState.NotNull);
								}
							}
							else
							{
								TypeWithAnnotations typeWithAnnotations3 = getIndexerOutputType(typeSymbol, boundDagIndexerEvaluation.IndexerAccess, isSlice: false);
								BoundDagTemp boundDagTemp4 = new BoundDagTemp(boundDagIndexerEvaluation.Syntax, typeWithAnnotations3.Type, boundDagIndexerEvaluation);
								int num4 = makeDagTempSlot(typeWithAnnotations3, boundDagTemp4);
								TrackNullableStateForAssignment(null, typeWithAnnotations3, num4, typeWithAnnotations3.ToTypeWithState());
								addToTempMap(boundDagTemp4, num4, typeWithAnnotations3.Type);
							}
						}
						else
						{
							addTemp(boundDagIndexEvaluation, boundDagIndexEvaluation.Property.Type);
						}
					}
					else
					{
						PropertySymbol propertySymbol = (boundDagPropertyEvaluation.Property.IsExtensionBlockMember() ? ReInferAndVisitExtensionPropertyAccess(boundDagPropertyEvaluation, boundDagPropertyEvaluation.Property, new BoundExpressionWithNullability(boundDagPropertyEvaluation.Syntax, expression, NullableAnnotation.NotAnnotated, typeSymbol)).updatedProperty : ((PropertySymbol)AsMemberOfType(typeSymbol, boundDagPropertyEvaluation.Property)));
						TypeWithAnnotations typeWithAnnotations4 = propertySymbol.TypeWithAnnotations;
						BoundDagTemp boundDagTemp5 = new BoundDagTemp(boundDagPropertyEvaluation.Syntax, typeWithAnnotations4.Type, boundDagPropertyEvaluation);
						int num5 = GetOrCreateSlot(propertySymbol, num2, forceSlotEvenIfEmpty: true);
						if (num5 <= 0)
						{
							num5 = makeDagTempSlot(typeWithAnnotations4, boundDagTemp5);
						}
						addToTempMap(boundDagTemp5, num5, typeWithAnnotations4.Type);
						if ((object)propertySymbol.GetMethod != null)
						{
							ApplyMemberPostConditions(num2, propertySymbol.GetMethod);
						}
					}
				}
				else
				{
					FieldSymbol fieldSymbol = (FieldSymbol)AsMemberOfType(typeSymbol, boundDagFieldEvaluation.Field);
					TypeWithAnnotations typeWithAnnotations5 = fieldSymbol.TypeWithAnnotations;
					BoundDagTemp boundDagTemp6 = new BoundDagTemp(boundDagFieldEvaluation.Syntax, typeWithAnnotations5.Type, boundDagFieldEvaluation);
					int num6 = -1;
					FieldSymbol fieldSymbol2 = ((boundDagFieldEvaluation.Input.IsOriginalInput && !immutableArray.IsDefault) ? fieldSymbol : null);
					if ((object)fieldSymbol2 != null)
					{
						num6 = immutableArray[fieldSymbol2.TupleElementIndex];
					}
					if (num6 <= 0)
					{
						num6 = GetOrCreateSlot(fieldSymbol, num2, forceSlotEvenIfEmpty: true);
						if ((object)fieldSymbol2 != null && num6 > 0 && !originalInputMap.ContainsKey(num6))
						{
							originalInputMap.Add(num6, ((BoundTupleExpression)expression).Arguments[fieldSymbol2.TupleElementIndex]);
						}
					}
					if (num6 <= 0)
					{
						num6 = makeDagTempSlot(typeWithAnnotations5, boundDagTemp6);
					}
					addToTempMap(boundDagTemp6, num6, typeWithAnnotations5.Type);
				}
			}
			else
			{
				MethodSymbol deconstructMethod = boundDagDeconstructEvaluation.DeconstructMethod;
				int num7 = ((!deconstructMethod.RequiresInstanceReceiver) ? 1 : 0);
				for (int num8 = 0; num8 < deconstructMethod.ParameterCount - num7; num8++)
				{
					TypeWithAnnotations typeWithAnnotations6 = deconstructMethod.Parameters[num8 + num7].TypeWithAnnotations;
					BoundDagTemp boundDagTemp7 = new BoundDagTemp(boundDagDeconstructEvaluation.Syntax, typeWithAnnotations6.Type, boundDagDeconstructEvaluation, num8);
					int slot2 = makeDagTempSlot(typeWithAnnotations6, boundDagTemp7);
					addToTempMap(boundDagTemp7, slot2, typeWithAnnotations6.Type);
				}
			}
			goto IL_0690;
			IL_08b8:
			bool flag3 = boundDagValueTest.Value == ConstantValue.False;
			gotoNode(boundTestDecisionDagNode.WhenTrue, flag3 ? StateWhenFalse : StateWhenTrue, flag);
			gotoNode(boundTestDecisionDagNode.WhenFalse, flag3 ? StateWhenTrue : StateWhenFalse, flag);
			continue;
			IL_03d1:
			Unsplit();
			SetState(ref State, num3, NullableFlowState.NotNull);
			addToTempMap(boundDagTemp2, num3, boundDagTypeEvaluation.Type);
			goto IL_0690;
			IL_0690:
			gotoNodeWithCurrentState(boundEvaluationDecisionDagNode.Next, flag);
		}
		SetUnreachable();
		originalInputMap.Free();
		tempMap.Free();
		nodeStateMap.Free();
		return instance;
		void addTemp(BoundDagEvaluation e, TypeSymbol t, int index = 0)
		{
			TypeWithAnnotations type3 = TypeWithAnnotations.Create(t, NullableAnnotation.Annotated);
			BoundDagTemp boundDagTemp8 = new BoundDagTemp(e.Syntax, type3.Type, e, index);
			int slot3 = makeDagTempSlot(type3, boundDagTemp8);
			addToTempMap(boundDagTemp8, slot3, type3.Type);
		}
		void addToTempMap(BoundDagTemp output, int item3, TypeSymbol item4)
		{
			if (!tempMap.TryGetValue(output, out (int, TypeSymbol) _))
			{
				tempMap.Add(output, (item3, item4));
			}
		}
		static TypeWithAnnotations getIndexerOutputType(TypeSymbol inputType, BoundExpression e, bool isSlice)
		{
			if (e is BoundIndexerAccess boundIndexerAccess)
			{
				return AsMemberOfType(inputType, boundIndexerAccess.Indexer).GetTypeOrReturnType();
			}
			if (e is BoundCall boundCall)
			{
				return AsMemberOfType(inputType, boundCall.Method).GetTypeOrReturnType();
			}
			if (e is BoundArrayAccess)
			{
				return isSlice ? TypeWithAnnotations.Create(isNullableEnabled: true, inputType) : ((ArrayTypeSymbol)inputType).ElementTypeWithAnnotations;
			}
			if (!(e is BoundImplicitIndexerAccess boundImplicitIndexerAccess))
			{
				throw ExceptionUtilities.UnexpectedValue(e.Kind);
			}
			return getIndexerOutputType(inputType, boundImplicitIndexerAccess.IndexerOrSliceAccess, isSlice);
		}
		void gotoNode(BoundDecisionDagNode key, LocalState other, bool believedReachable)
		{
			PossiblyConditionalState item3;
			if (nodeStateMap.TryGetValue(key, out (PossiblyConditionalState, bool) value5))
			{
				(item3, _) = value5;
				if (item3.IsConditionalState)
				{
					Join(ref item3.StateWhenTrue, ref other);
					Join(ref item3.StateWhenFalse, ref other);
				}
				else
				{
					Join(ref item3.State, ref other);
				}
				believedReachable |= value5.Item2;
			}
			else
			{
				item3 = new PossiblyConditionalState(other);
			}
			nodeStateMap[key] = (item3, believedReachable);
		}
		void gotoNodeWithCurrentState(BoundDecisionDagNode key, bool believedReachable)
		{
			if (nodeStateMap.TryGetValue(key, out (PossiblyConditionalState, bool) value5))
			{
				bool isConditionalState = IsConditionalState;
				bool isConditionalState2 = value5.Item1.IsConditionalState;
				if (isConditionalState)
				{
					if (isConditionalState2)
					{
						Join(ref StateWhenTrue, ref value5.Item1.StateWhenTrue);
						Join(ref StateWhenFalse, ref value5.Item1.StateWhenFalse);
					}
					else
					{
						Join(ref StateWhenTrue, ref value5.Item1.State);
						Join(ref StateWhenFalse, ref value5.Item1.State);
					}
				}
				else if (isConditionalState2)
				{
					Split();
					Join(ref StateWhenTrue, ref value5.Item1.StateWhenTrue);
					Join(ref StateWhenFalse, ref value5.Item1.StateWhenFalse);
				}
				else
				{
					Join(ref State, ref value5.Item1.State);
				}
				believedReachable |= value5.Item2;
			}
			nodeStateMap[key] = (PossiblyConditionalState.Create(this), believedReachable);
		}
		void learnFromNonNullTest(int inputSlot, ref LocalState reference)
		{
			if (stateWhenNotNullOpt.HasValue)
			{
				PossiblyConditionalState conditionalState2 = stateWhenNotNullOpt.GetValueOrDefault();
				if (inputSlot == originalInputSlot)
				{
					reference = CloneAndUnsplit(ref conditionalState2);
				}
			}
			LearnFromNonNullTest(inputSlot, ref reference);
			if (originalInputMap.TryGetValue(inputSlot, out var value5))
			{
				LearnFromNonNullTest(value5, ref reference);
			}
		}
		int makeDagTempSlot(TypeWithAnnotations type3, BoundDagTemp temp)
		{
			object identifier = (node, temp);
			return GetOrCreatePlaceholderSlot(identifier, type3);
		}
	}

	public override BoundNode VisitConvertedSwitchExpression(BoundConvertedSwitchExpression node)
	{
		bool inferType = !node.WasTargetTyped;
		VisitSwitchExpressionCore(node, inferType);
		return null;
	}

	public override BoundNode VisitUnconvertedSwitchExpression(BoundUnconvertedSwitchExpression node)
	{
		VisitSwitchExpressionCore(node, inferType: true);
		return null;
	}

	private void VisitSwitchExpressionCore(BoundSwitchExpression node, bool inferType)
	{
		int slotForSwitchInputValue = GetSlotForSwitchInputValue(node.Expression);
		if (slotForSwitchInputValue > 0)
		{
			TypeSymbol type = node.Expression.Type;
			foreach (BoundSwitchExpressionArm switchArm in node.SwitchArms)
			{
				LearnFromAnyNullPatterns(slotForSwitchInputValue, type, switchArm.Pattern);
			}
		}
		Visit(node.Expression);
		TypeWithState resultType = ResultType;
		PooledDictionary<LabelSymbol, (LocalState, bool)> pooledDictionary = LearnFromDecisionDag(node.Syntax, node.ReachabilityDecisionDag, node.Expression, resultType, null);
		LocalState self = UnreachableState();
		bool unnamedEnumValue;
		if (!node.ReportedNotExhaustive && node.DefaultLabel != null && pooledDictionary.TryGetValue(node.DefaultLabel, out var value) && value.Item2)
		{
			SetState(value.Item1);
			ImmutableArray<BoundDecisionDagNode> topologicallySortedNodes = node.ReachabilityDecisionDag.TopologicallySortedNodes;
			BoundDecisionDagNode targetNode = topologicallySortedNodes.Where((BoundDecisionDagNode n) => n is BoundLeafDecisionDagNode boundLeafDecisionDagNode && boundLeafDecisionDagNode.Label == node.DefaultLabel).First();
			string text = PatternExplainer.SamplePatternForPathToDagNode(BoundDagTemp.ForOriginalInput(node.Expression), topologicallySortedNodes, targetNode, nullPaths: true, out var requiresFalseWhenClause, out unnamedEnumValue);
			ErrorCode errorCode = (requiresFalseWhenClause ? ErrorCode.WRN_SwitchExpressionNotExhaustiveForNullWithWhen : ErrorCode.WRN_SwitchExpressionNotExhaustiveForNull);
			ReportDiagnostic(errorCode, ((SwitchExpressionSyntax)node.Syntax).SwitchKeyword.GetLocation(), text);
		}
		int length = node.SwitchArms.Length;
		ArrayBuilder<Conversion> instance = ArrayBuilder<Conversion>.GetInstance(length);
		ArrayBuilder<TypeWithState> instance2 = ArrayBuilder<TypeWithState>.GetInstance(length);
		ArrayBuilder<BoundExpression> instance3 = ArrayBuilder<BoundExpression>.GetInstance(length);
		ArrayBuilder<BoundExpression> instance4 = ArrayBuilder<BoundExpression>.GetInstance(length);
		foreach (BoundSwitchExpressionArm switchArm2 in node.SwitchArms)
		{
			SetState(getStateForArm(switchArm2, pooledDictionary));
			TakeIncrementalSnapshot(switchArm2);
			VisitForRewriting(switchArm2.Pattern);
			if (!State.Reachable && switchArm2.WhenClause != null)
			{
				VisitForRewriting(switchArm2.WhenClause);
			}
			var (boundExpression, item) = RemoveConversion(switchArm2.Value, includeExplicitConversions: false);
			SnapshotWalkerThroughConversionGroup(switchArm2.Value, boundExpression);
			instance3.Add(boundExpression);
			instance.Add(item);
			TypeWithState item2 = VisitRvalueWithState(boundExpression);
			instance2.Add(item2);
			Join(ref self, ref State);
			if (!IsTargetTypedExpression(boundExpression))
			{
				instance4.Add(CreatePlaceholderIfNecessary(boundExpression, item2.ToTypeWithAnnotations(compilation)));
			}
		}
		SetState(self);
		ImmutableArray<BoundExpression> exprs = instance4.ToImmutableAndFree();
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
		TypeSymbol typeSymbol = (inferType ? BestTypeInferrer.InferBestType(exprs, _conversions, ref useSiteInfo, out unnamedEnumValue) : null) ?? node.Type?.SetUnknownNullabilityForReferenceTypes();
		TypeWithAnnotations typeWithAnnotations = TypeWithAnnotations.Create(typeSymbol);
		if (inferType && (object)typeSymbol == null)
		{
			NullableFlowState defaultState = NullableFlowState.NotNull;
			TypeWithState resultType2 = TypeWithState.Create(typeSymbol, defaultState);
			instance.Free();
			instance2.Free();
			instance3.Free();
			pooledDictionary.Free();
			SetResult(node, resultType2, typeWithAnnotations);
		}
		else
		{
			TypeWithState resultType2 = convertArms(node, pooledDictionary, instance, instance2, instance3, typeWithAnnotations, !inferType);
			SetResult(node, resultType2, typeWithAnnotations, updateAnalyzedNullability: false);
		}
		void addConvertArmsAsCompletion(BoundSwitchExpression boundSwitchExpression, PooledDictionary<LabelSymbol, (LocalState state, bool believedReachable)> labelStateMap, ArrayBuilder<Conversion> conversions, ArrayBuilder<TypeWithState> resultTypes, ArrayBuilder<BoundExpression> expressions)
		{
			TargetTypedAnalysisCompletion[boundSwitchExpression] = (TypeWithAnnotations inferredTypeWithAnnotations) => convertArms(boundSwitchExpression, labelStateMap, conversions, resultTypes, expressions, inferredTypeWithAnnotations, isTargetTyped: false);
		}
		TypeWithState convertArms(BoundSwitchExpression boundSwitchExpression, PooledDictionary<LabelSymbol, (LocalState state, bool believedReachable)> labelStateMap, ArrayBuilder<Conversion> conversions, ArrayBuilder<TypeWithState> resultTypes, ArrayBuilder<BoundExpression> expressions, TypeWithAnnotations inferredTypeWithAnnotations, bool isTargetTyped)
		{
			if (!isTargetTyped)
			{
				int length2 = boundSwitchExpression.SwitchArms.Length;
				for (int i = 0; i < length2; i++)
				{
					BoundExpression operand = expressions[i];
					BoundSwitchExpressionArm boundSwitchExpressionArm = boundSwitchExpression.SwitchArms[i];
					LocalState state = getStateForArm(boundSwitchExpressionArm, labelStateMap);
					resultTypes[i] = ConvertConditionalOperandOrSwitchExpressionArmResult(boundSwitchExpressionArm.Value, operand, conversions[i], inferredTypeWithAnnotations, resultTypes[i], state, state.Reachable);
				}
			}
			NullableFlowState nullableState = BestTypeInferrer.GetNullableState(resultTypes);
			if (!isTargetTyped)
			{
				conversions.Free();
				resultTypes.Free();
				expressions.Free();
				labelStateMap.Free();
			}
			else
			{
				addConvertArmsAsCompletion(boundSwitchExpression, labelStateMap, conversions, resultTypes, expressions);
			}
			TypeWithState typeWithState = TypeWithState.Create(inferredTypeWithAnnotations.Type, nullableState);
			if (!isTargetTyped)
			{
				SetAnalyzedNullability(boundSwitchExpression, typeWithState);
			}
			return typeWithState;
		}
		LocalState getStateForArm(BoundSwitchExpressionArm arm, PooledDictionary<LabelSymbol, (LocalState state, bool believedReachable)> labelStateMap)
		{
			if (arm.Pattern.HasErrors || !labelStateMap.TryGetValue(arm.Label, out (LocalState, bool) value2))
			{
				return UnreachableState();
			}
			return value2.Item1;
		}
	}

	private int GetSlotForSwitchInputValue(BoundExpression node)
	{
		if (!node.IsSuppressed)
		{
			return MakeSlot(node);
		}
		return GetOrCreatePlaceholderSlot(node);
	}

	public override BoundNode VisitIsPatternExpression(BoundIsPatternExpression node)
	{
		LearnFromAnyNullPatterns(node.Expression, node.Pattern);
		VisitForRewriting(node.Pattern);
		bool flag = VisitPossibleConditionalAccess(node.Expression, out var stateWhenNotNull);
		TypeWithState resultType = ResultType;
		PooledDictionary<LabelSymbol, (LocalState state, bool believedReachable)> pooledDictionary = LearnFromDecisionDag(node.Syntax, node.ReachabilityDecisionDag, node.Expression, resultType, flag ? new PossiblyConditionalState?(stateWhenNotNull) : ((PossiblyConditionalState?)null));
		LocalState whenTrue = (pooledDictionary.TryGetValue(node.IsNegated ? node.WhenFalseLabel : node.WhenTrueLabel, out (LocalState, bool) value) ? value.Item1 : UnreachableState());
		LocalState whenFalse = (pooledDictionary.TryGetValue(node.IsNegated ? node.WhenTrueLabel : node.WhenFalseLabel, out (LocalState, bool) value2) ? value2.Item1 : UnreachableState());
		pooledDictionary.Free();
		SetConditionalState(whenTrue, whenFalse);
		SetNotNullResult(node);
		return null;
	}
}
