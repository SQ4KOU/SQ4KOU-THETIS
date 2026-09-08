using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedRecordDeconstruct : SynthesizedRecordOrdinaryMethod
{
	private readonly SynthesizedPrimaryConstructor _ctor;

	private readonly ImmutableArray<Symbol> _positionalMembers;

	public SynthesizedRecordDeconstruct(SourceMemberContainerTypeSymbol containingType, SynthesizedPrimaryConstructor ctor, ImmutableArray<Symbol> positionalMembers, int memberOffset)
		: base(containingType, "Deconstruct", memberOffset, (DeclarationModifiers)(0x10 | (IsReadOnly(containingType, positionalMembers) ? 1024 : 0)))
	{
		_ctor = ctor;
		_positionalMembers = positionalMembers;
	}

	protected override (TypeWithAnnotations ReturnType, ImmutableArray<ParameterSymbol> Parameters) MakeParametersAndBindReturnType(BindingDiagnosticBag diagnostics)
	{
		return (ReturnType: TypeWithAnnotations.Create(Binder.GetSpecialType(DeclaringCompilation, location: ReturnTypeLocation, typeId: SpecialType.System_Void, diagnostics: diagnostics)), Parameters: _ctor.Parameters.SelectAsArray((Func<ParameterSymbol, ImmutableArray<Location>, ParameterSymbol>)((ParameterSymbol param, ImmutableArray<Location> locations) => new SourceSimpleParameterSymbol(this, param.TypeWithAnnotations, param.Ordinal, RefKind.Out, param.Name, locations)), Locations));
	}

	protected override int GetParameterCountFromSyntax()
	{
		return _ctor.ParameterCount;
	}

	internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, ContainingType.GetNonNullSyntaxNode(), compilationState, diagnostics);
		if (ParameterCount != _positionalMembers.Length)
		{
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
			return;
		}
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance(_positionalMembers.Length + 1);
		for (int i = 0; i < _positionalMembers.Length; i++)
		{
			ParameterSymbol parameterSymbol = Parameters[i];
			Symbol symbol = _positionalMembers[i];
			TypeSymbol type;
			if (!(symbol is PropertySymbol propertySymbol))
			{
				if (!(symbol is FieldSymbol fieldSymbol))
				{
					throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/Records/SynthesizedRecordDeconstruct.cs", 70);
				}
				type = fieldSymbol.Type;
			}
			else
			{
				type = propertySymbol.Type;
			}
			TypeSymbol t = type;
			if (!parameterSymbol.Type.Equals(t, TypeCompareKind.AllIgnoreOptions))
			{
				instance.Free();
				syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
				return;
			}
			if (!(symbol is PropertySymbol property))
			{
				if (symbol is FieldSymbol f)
				{
					instance.Add(syntheticBoundNodeFactory.Assignment(syntheticBoundNodeFactory.Parameter(parameterSymbol), syntheticBoundNodeFactory.Field(syntheticBoundNodeFactory.This(), f)));
				}
			}
			else
			{
				instance.Add(syntheticBoundNodeFactory.Assignment(syntheticBoundNodeFactory.Parameter(parameterSymbol), syntheticBoundNodeFactory.Property(syntheticBoundNodeFactory.This(), property)));
			}
		}
		instance.Add(syntheticBoundNodeFactory.Return());
		syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.Block(instance.ToImmutableAndFree()));
	}

	private static bool IsReadOnly(SourceMemberContainerTypeSymbol containingType, ImmutableArray<Symbol> positionalMembers)
	{
		if (!containingType.IsReadOnly)
		{
			if (containingType.IsRecordStruct)
			{
				return !positionalMembers.Any((Symbol m) => hasNonReadOnlyGetter(m));
			}
			return false;
		}
		return true;
		static bool hasNonReadOnlyGetter(Symbol m)
		{
			if (m.Kind == SymbolKind.Property)
			{
				PropertySymbol obj = (PropertySymbol)m;
				MethodSymbol getMethod = obj.GetMethod;
				if ((object)obj.GetMethod != null)
				{
					return !getMethod.IsEffectivelyReadOnly;
				}
				return false;
			}
			return false;
		}
	}
}
