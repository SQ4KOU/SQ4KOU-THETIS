using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class LocalSlotManager
{
	private readonly struct LocalSignature : IEquatable<LocalSignature>
	{
		private readonly ITypeReference _type;

		private readonly LocalSlotConstraints _constraints;

		internal LocalSignature(ITypeReference valType, LocalSlotConstraints constraints)
		{
			_constraints = constraints;
			_type = valType;
		}

		public bool Equals(LocalSignature other)
		{
			if (_constraints == other._constraints)
			{
				return SymbolEquivalentEqualityComparer.Instance.Equals(_type, other._type);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(SymbolEquivalentEqualityComparer.Instance.GetHashCode(_type), (int)_constraints);
		}

		public override bool Equals(object? obj)
		{
			if (obj is LocalSignature other)
			{
				return Equals(other);
			}
			return false;
		}
	}

	private Dictionary<ILocalSymbolInternal, LocalDefinition>? _localMap;

	private KeyedStack<LocalSignature, LocalDefinition>? _freeSlots;

	private HashSet<LocalDefinition>? _nonReusableLocals;

	private ArrayBuilder<LocalDefinition>? _addressedLocals;

	private int _addressedLocalScopes;

	private ArrayBuilder<ILocalDefinition>? _lazyAllLocals;

	private readonly VariableSlotAllocator? _slotAllocator;

	private Dictionary<ILocalSymbolInternal, LocalDefinition> LocalMap
	{
		get
		{
			Dictionary<ILocalSymbolInternal, LocalDefinition> dictionary = _localMap;
			if (dictionary == null)
			{
				dictionary = (_localMap = new Dictionary<ILocalSymbolInternal, LocalDefinition>(ReferenceEqualityComparer.Instance));
			}
			return dictionary;
		}
	}

	private KeyedStack<LocalSignature, LocalDefinition> FreeSlots
	{
		get
		{
			KeyedStack<LocalSignature, LocalDefinition> keyedStack = _freeSlots;
			if (keyedStack == null)
			{
				keyedStack = (_freeSlots = new KeyedStack<LocalSignature, LocalDefinition>());
			}
			return keyedStack;
		}
	}

	public LocalSlotManager(VariableSlotAllocator? slotAllocator)
	{
		_slotAllocator = slotAllocator;
		if (slotAllocator != null)
		{
			_lazyAllLocals = new ArrayBuilder<ILocalDefinition>();
			slotAllocator.AddPreviousLocals(_lazyAllLocals);
		}
	}

	internal LocalDefinition DeclareLocal(ITypeReference type, ILocalSymbolInternal symbol, string name, SynthesizedLocalKind kind, LocalDebugId id, LocalVariableAttributes pdbAttributes, LocalSlotConstraints constraints, ImmutableArray<bool> dynamicTransformFlags, ImmutableArray<string> tupleElementNames, bool isSlotReusable)
	{
		if (!isSlotReusable || !FreeSlots.TryPop(new LocalSignature(type, constraints), out LocalDefinition value))
		{
			value = DeclareLocalImpl(type, symbol, name, kind, id, pdbAttributes, constraints, dynamicTransformFlags, tupleElementNames);
		}
		LocalMap.Add(symbol, value);
		return value;
	}

	internal LocalDefinition GetLocal(ILocalSymbolInternal symbol)
	{
		return LocalMap[symbol];
	}

	internal void FreeLocal(ILocalSymbolInternal symbol)
	{
		LocalDefinition local = GetLocal(symbol);
		LocalMap.Remove(symbol);
		FreeSlot(local);
	}

	internal LocalDefinition AllocateSlot(ITypeReference type, LocalSlotConstraints constraints, ImmutableArray<bool> dynamicTransformFlags = default(ImmutableArray<bool>), ImmutableArray<string> tupleElementNames = default(ImmutableArray<string>))
	{
		if (!FreeSlots.TryPop(new LocalSignature(type, constraints), out LocalDefinition value))
		{
			return DeclareLocalImpl(type, null, null, SynthesizedLocalKind.EmitterTemp, LocalDebugId.None, LocalVariableAttributes.DebuggerHidden, constraints, dynamicTransformFlags, tupleElementNames);
		}
		return value;
	}

	private LocalDefinition DeclareLocalImpl(ITypeReference type, ILocalSymbolInternal? symbol, string? name, SynthesizedLocalKind kind, LocalDebugId id, LocalVariableAttributes pdbAttributes, LocalSlotConstraints constraints, ImmutableArray<bool> dynamicTransformFlags, ImmutableArray<string> tupleElementNames)
	{
		if (_lazyAllLocals == null)
		{
			_lazyAllLocals = new ArrayBuilder<ILocalDefinition>(1);
		}
		LocalDefinition previousLocal;
		if (symbol != null && _slotAllocator != null)
		{
			previousLocal = _slotAllocator.GetPreviousLocal(type, symbol, name, kind, id, pdbAttributes, constraints, dynamicTransformFlags, tupleElementNames);
			if (previousLocal != null)
			{
				int slotIndex = previousLocal.SlotIndex;
				_lazyAllLocals[slotIndex] = previousLocal;
				return previousLocal;
			}
		}
		previousLocal = new LocalDefinition(symbol, name, type, _lazyAllLocals.Count, kind, id, pdbAttributes, constraints, dynamicTransformFlags, tupleElementNames);
		_lazyAllLocals.Add(previousLocal);
		return previousLocal;
	}

	internal void FreeSlot(LocalDefinition slot)
	{
		HashSet<LocalDefinition>? nonReusableLocals = _nonReusableLocals;
		if (nonReusableLocals == null || !nonReusableLocals.Remove(slot))
		{
			FreeSlots.Push(new LocalSignature(slot.Type, slot.Constraints), slot);
		}
	}

	internal int StartScopeOfTrackingAddressedLocals()
	{
		if (_addressedLocals == null)
		{
			_addressedLocals = ArrayBuilder<LocalDefinition>.GetInstance();
		}
		_addressedLocalScopes++;
		return _addressedLocals.Count;
	}

	internal void AddAddressedLocal(LocalDefinition localDef, OptimizationLevel optimizations)
	{
		if (localDef != null)
		{
			ILocalSymbolInternal? symbolOpt = localDef.SymbolOpt;
			if (symbolOpt == null || symbolOpt.SynthesizedKind.IsSlotReusable(optimizations))
			{
				_addressedLocals?.Add(localDef);
			}
		}
	}

	internal void EndScopeOfTrackingAddressedLocals(int countBefore, bool markAsNotReusable)
	{
		if (markAsNotReusable && countBefore < _addressedLocals.Count)
		{
			if (_nonReusableLocals == null)
			{
				_nonReusableLocals = new HashSet<LocalDefinition>(ReferenceEqualityComparer.Instance);
			}
			for (int i = countBefore; i < _addressedLocals.Count; i++)
			{
				_nonReusableLocals.Add(_addressedLocals[i]);
			}
		}
		_addressedLocalScopes--;
		if (_addressedLocalScopes > 0)
		{
			_addressedLocals.Count = countBefore;
			return;
		}
		_addressedLocals.Free();
		_addressedLocals = null;
	}

	public ImmutableArray<ILocalDefinition> LocalsInOrder()
	{
		if (_lazyAllLocals == null)
		{
			return ImmutableArray<ILocalDefinition>.Empty;
		}
		return _lazyAllLocals.ToImmutable();
	}
}
