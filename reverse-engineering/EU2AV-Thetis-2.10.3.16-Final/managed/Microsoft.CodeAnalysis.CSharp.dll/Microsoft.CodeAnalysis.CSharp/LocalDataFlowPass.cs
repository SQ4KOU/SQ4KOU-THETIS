using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class LocalDataFlowPass<TLocalState, TLocalFunctionState> : AbstractFlowPass<TLocalState, TLocalFunctionState> where TLocalState : LocalDataFlowPass<TLocalState, TLocalFunctionState>.ILocalDataFlowState where TLocalFunctionState : AbstractFlowPass<TLocalState, TLocalFunctionState>.AbstractLocalFunctionState
{
	internal readonly struct VariableIdentifier(Symbol symbol, int containingSlot = 0) : IEquatable<VariableIdentifier>
	{
		public readonly Symbol Symbol = symbol;

		public readonly int ContainingSlot = containingSlot;

		public bool Exists => (object)Symbol != null;

		public override int GetHashCode()
		{
			int containingSlot = ContainingSlot;
			int? memberIndexOpt = Symbol.MemberIndexOpt;
			if (!memberIndexOpt.HasValue)
			{
				return Hash.Combine(Symbol.OriginalDefinition, containingSlot);
			}
			return Hash.Combine(memberIndexOpt.GetValueOrDefault(), containingSlot);
		}

		public bool Equals(VariableIdentifier other)
		{
			if (ContainingSlot != other.ContainingSlot)
			{
				return false;
			}
			int? memberIndexOpt = Symbol.MemberIndexOpt;
			int? memberIndexOpt2 = other.Symbol.MemberIndexOpt;
			if (memberIndexOpt != memberIndexOpt2)
			{
				return false;
			}
			if (memberIndexOpt.HasValue)
			{
				return true;
			}
			return Symbol.Equals(other.Symbol, TypeCompareKind.AllIgnoreOptions);
		}

		public override bool Equals(object? obj)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/DefiniteAssignment.VariableIdentifier.cs", 90);
		}

		[Obsolete]
		public static bool operator ==(VariableIdentifier left, VariableIdentifier right)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/DefiniteAssignment.VariableIdentifier.cs", 96);
		}

		[Obsolete]
		public static bool operator !=(VariableIdentifier left, VariableIdentifier right)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/FlowAnalysis/DefiniteAssignment.VariableIdentifier.cs", 102);
		}

		public override string ToString()
		{
			return $"ContainingSlot={ContainingSlot}, Symbol={Symbol.GetDebuggerDisplay()}";
		}
	}

	internal interface ILocalDataFlowState : ILocalState
	{
		bool NormalizeToBottom { get; }
	}

	protected readonly EmptyStructTypeCache _emptyStructTypeCache;

	protected LocalDataFlowPass(CSharpCompilation compilation, Symbol? member, BoundNode node, EmptyStructTypeCache emptyStructs, bool trackUnassignments)
		: base(compilation, member, node, (BoundNode)null, (BoundNode)null, false, trackUnassignments)
	{
		_emptyStructTypeCache = emptyStructs;
	}

	protected LocalDataFlowPass(CSharpCompilation compilation, Symbol member, BoundNode node, EmptyStructTypeCache emptyStructs, BoundNode firstInRegion, BoundNode lastInRegion, bool trackRegions, bool trackUnassignments)
		: base(compilation, member, node, firstInRegion, lastInRegion, trackRegions, trackUnassignments)
	{
		_emptyStructTypeCache = emptyStructs;
	}

	protected abstract bool TryGetVariable(VariableIdentifier identifier, out int slot);

	protected abstract int AddVariable(VariableIdentifier identifier);

	protected int VariableSlot(Symbol symbol, int containingSlot = 0)
	{
		if (symbol is LocalSymbol { SynthesizedKind: SynthesizedLocalKind.LocalStoreTracker })
		{
			return -1;
		}
		containingSlot = DescendThroughTupleRestFields(ref symbol, containingSlot, forceContainingSlotsToExist: false);
		if (!TryGetVariable(new VariableIdentifier(symbol, containingSlot), out var slot))
		{
			return -1;
		}
		return slot;
	}

	protected virtual bool IsEmptyStructType(TypeSymbol type)
	{
		return _emptyStructTypeCache.IsEmptyStructType(type);
	}

	protected virtual int GetOrCreateSlot(Symbol symbol, int containingSlot = 0, bool forceSlotEvenIfEmpty = false, bool createIfMissing = true)
	{
		if (symbol.Kind == SymbolKind.RangeVariable)
		{
			return -1;
		}
		containingSlot = DescendThroughTupleRestFields(ref symbol, containingSlot, forceContainingSlotsToExist: true);
		if (containingSlot < 0)
		{
			return -1;
		}
		VariableIdentifier identifier = new VariableIdentifier(symbol, containingSlot);
		if (!TryGetVariable(identifier, out var slot))
		{
			if (!createIfMissing)
			{
				return -1;
			}
			TypeSymbol type = symbol.GetTypeOrReturnType().Type;
			if (!forceSlotEvenIfEmpty && IsEmptyStructType(type))
			{
				return -1;
			}
			slot = AddVariable(identifier);
		}
		if (IsConditionalState)
		{
			Normalize(ref StateWhenTrue);
			Normalize(ref StateWhenFalse);
		}
		else
		{
			Normalize(ref State);
		}
		return slot;
	}

	protected abstract void Normalize(ref TLocalState state);

	private int DescendThroughTupleRestFields(ref Symbol symbol, int containingSlot, bool forceContainingSlotsToExist)
	{
		if (symbol is TupleElementFieldSymbol tupleElementFieldSymbol)
		{
			TypeSymbol typeSymbol = symbol.ContainingType;
			symbol = tupleElementFieldSymbol.TupleUnderlyingField;
			while (!TypeSymbol.Equals(typeSymbol, symbol.ContainingType, TypeCompareKind.ConsiderEverything))
			{
				if (!(typeSymbol.GetMembers("Rest").FirstOrDefault((Symbol s) => !(s is TupleVirtualElementFieldSymbol)) is FieldSymbol fieldSymbol))
				{
					return -1;
				}
				if (forceContainingSlotsToExist)
				{
					containingSlot = GetOrCreateSlot(fieldSymbol, containingSlot);
					if (containingSlot < 0)
					{
						return -1;
					}
				}
				else if (!TryGetVariable(new VariableIdentifier(fieldSymbol, containingSlot), out containingSlot))
				{
					return -1;
				}
				typeSymbol = fieldSymbol.Type;
			}
		}
		return containingSlot;
	}

	protected abstract bool TryGetReceiverAndMember(BoundExpression expr, out BoundExpression? receiver, [NotNullWhen(true)] out Symbol? member);

	protected virtual int MakeSlot(BoundExpression node)
	{
		switch (node.Kind)
		{
		case BoundKind.ThisReference:
		case BoundKind.BaseReference:
			if ((object)base.MethodThisParameter == null)
			{
				return -1;
			}
			return GetOrCreateSlot(base.MethodThisParameter);
		case BoundKind.Local:
			return GetOrCreateSlot(((BoundLocal)node).LocalSymbol);
		case BoundKind.Parameter:
			return GetOrCreateSlot(((BoundParameter)node).ParameterSymbol);
		case BoundKind.RangeVariable:
			return MakeSlot(((BoundRangeVariable)node).Value);
		case BoundKind.FieldAccess:
		case BoundKind.PropertyAccess:
		case BoundKind.EventAccess:
		{
			if (TryGetReceiverAndMember(node, out BoundExpression receiver, out Symbol member))
			{
				return MakeMemberSlot(receiver, member);
			}
			break;
		}
		case BoundKind.AssignmentOperator:
			return MakeSlot(((BoundAssignmentOperator)node).Left);
		}
		return -1;
	}

	protected int MakeMemberSlot(BoundExpression? receiverOpt, Symbol member)
	{
		int num;
		if (member.RequiresInstanceReceiver())
		{
			if (receiverOpt == null)
			{
				return -1;
			}
			num = MakeSlot(receiverOpt);
			if (num < 0)
			{
				return -1;
			}
		}
		else
		{
			num = 0;
		}
		return GetOrCreateSlot(member, num);
	}

	protected static bool HasInitializer(Symbol field)
	{
		if (!(field is SourceMemberFieldSymbol { HasInitializer: var hasInitializer }))
		{
			if (!(field is SynthesizedBackingFieldSymbolBase { HasInitializer: var hasInitializer2 }))
			{
				if (field is SourceFieldLikeEventSymbol sourceFieldLikeEventSymbol)
				{
					return sourceFieldLikeEventSymbol.AssociatedEventField?.HasInitializer ?? false;
				}
				return false;
			}
			return hasInitializer2;
		}
		return hasInitializer;
	}
}
