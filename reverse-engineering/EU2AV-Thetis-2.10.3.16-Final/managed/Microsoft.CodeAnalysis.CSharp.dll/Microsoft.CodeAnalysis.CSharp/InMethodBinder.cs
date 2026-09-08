using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class InMethodBinder : LocalScopeBinder
{
	private MultiDictionary<string, ParameterSymbol> _lazyParameterMap;

	private readonly MethodSymbol _methodSymbol;

	private SmallDictionary<string, Symbol> _lazyDefinitionMap;

	protected override bool InExecutableBinder => true;

	internal override Symbol ContainingMemberOrLambda => _methodSymbol;

	internal override bool IsInMethodBody => true;

	internal override bool IsNestedFunctionBinder => _methodSymbol.MethodKind == MethodKind.LocalFunction;

	internal override bool IsDirectlyInIterator => _methodSymbol.IsIterator;

	internal override bool IsIndirectlyInIterator => IsDirectlyInIterator;

	internal override GeneratedLabelSymbol BreakLabel => null;

	internal override GeneratedLabelSymbol ContinueLabel => null;

	public InMethodBinder(MethodSymbol owner, Binder enclosing)
		: base(enclosing, (BinderFlags)((uint)enclosing.Flags & 0xFFC0FFFFu))
	{
		_methodSymbol = owner;
	}

	private static void RecordDefinition<T>(SmallDictionary<string, Symbol> declarationMap, ImmutableArray<T> definitions) where T : Symbol
	{
		foreach (T item in definitions)
		{
			if (!declarationMap.ContainsKey(item.Name))
			{
				declarationMap.Add(item.Name, item);
			}
		}
	}

	protected override SourceLocalSymbol LookupLocal(SyntaxToken nameToken)
	{
		return null;
	}

	protected override LocalFunctionSymbol LookupLocalFunction(SyntaxToken nameToken)
	{
		return null;
	}

	protected override void ValidateYield(YieldStatementSyntax node, BindingDiagnosticBag diagnostics)
	{
	}

	internal override TypeWithAnnotations GetIteratorElementType()
	{
		RefKind refKind = _methodSymbol.RefKind;
		TypeSymbol returnType = _methodSymbol.ReturnType;
		if (!IsDirectlyInIterator)
		{
			TypeWithAnnotations iteratorElementTypeFromReturnType = GetIteratorElementTypeFromReturnType(base.Compilation, refKind, returnType, null, null);
			if (iteratorElementTypeFromReturnType.IsDefault)
			{
				return TypeWithAnnotations.Create(CreateErrorType());
			}
			return iteratorElementTypeFromReturnType;
		}
		return _methodSymbol.IteratorElementTypeWithAnnotations;
	}

	internal static TypeWithAnnotations GetIteratorElementTypeFromReturnType(CSharpCompilation compilation, RefKind refKind, TypeSymbol returnType, Location errorLocation, BindingDiagnosticBag diagnostics)
	{
		if (refKind == RefKind.None && returnType.Kind == SymbolKind.NamedType)
		{
			TypeSymbol originalDefinition = returnType.OriginalDefinition;
			switch (originalDefinition.SpecialType)
			{
			case SpecialType.System_Collections_IEnumerable:
			case SpecialType.System_Collections_IEnumerator:
			{
				NamedTypeSymbol specialType = compilation.GetSpecialType(SpecialType.System_Object);
				if (diagnostics != null)
				{
					Binder.ReportUseSite(specialType, diagnostics, errorLocation);
				}
				return TypeWithAnnotations.Create(specialType);
			}
			case SpecialType.System_Collections_Generic_IEnumerable_T:
			case SpecialType.System_Collections_Generic_IEnumerator_T:
				return ((NamedTypeSymbol)returnType).TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[0];
			}
			if (TypeSymbol.Equals(originalDefinition, compilation.GetWellKnownType(WellKnownType.System_Collections_Generic_IAsyncEnumerable_T), TypeCompareKind.ConsiderEverything) || TypeSymbol.Equals(originalDefinition, compilation.GetWellKnownType(WellKnownType.System_Collections_Generic_IAsyncEnumerator_T), TypeCompareKind.ConsiderEverything))
			{
				return ((NamedTypeSymbol)returnType).TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[0];
			}
		}
		return default(TypeWithAnnotations);
	}

	internal static bool IsAsyncStreamInterface(CSharpCompilation compilation, RefKind refKind, TypeSymbol returnType)
	{
		if (refKind == RefKind.None && returnType.Kind == SymbolKind.NamedType)
		{
			TypeSymbol originalDefinition = returnType.OriginalDefinition;
			if (TypeSymbol.Equals(originalDefinition, compilation.GetWellKnownType(WellKnownType.System_Collections_Generic_IAsyncEnumerable_T), TypeCompareKind.ConsiderEverything) || TypeSymbol.Equals(originalDefinition, compilation.GetWellKnownType(WellKnownType.System_Collections_Generic_IAsyncEnumerator_T), TypeCompareKind.ConsiderEverything))
			{
				return true;
			}
		}
		return false;
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, int arity, ConsList<TypeSymbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (_methodSymbol.ParameterCount == 0 || (options & LookupOptions.NamespaceAliasesOnly) != LookupOptions.Default)
		{
			return;
		}
		MultiDictionary<string, ParameterSymbol> multiDictionary = _lazyParameterMap;
		if (multiDictionary == null)
		{
			ImmutableArray<ParameterSymbol> parameters = _methodSymbol.Parameters;
			multiDictionary = new MultiDictionary<string, ParameterSymbol>(parameters.Length, EqualityComparer<string>.Default);
			foreach (ParameterSymbol item in parameters)
			{
				if ((Flags & BinderFlags.InEEMethodBinder) == 0 || !item.Type.IsDisplayClassType())
				{
					multiDictionary.Add(item.Name, item);
				}
			}
			_lazyParameterMap = multiDictionary;
		}
		foreach (ParameterSymbol item2 in multiDictionary[name])
		{
			result.MergeEqual(originalBinder.CheckViability(item2, arity, options, null, diagnose, ref useSiteInfo));
		}
	}

	internal override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		if (!options.CanConsiderMembers())
		{
			return;
		}
		foreach (ParameterSymbol parameter in _methodSymbol.Parameters)
		{
			if (originalBinder.CanAddLookupSymbolInfo(parameter, options, result, null))
			{
				result.AddSymbol(parameter, parameter.Name, 0);
			}
		}
	}

	private static bool ReportConflictWithParameter(Symbol parameter, Symbol newSymbol, string name, Location newLocation, BindingDiagnosticBag diagnostics)
	{
		SymbolKind kind = parameter.Kind;
		SymbolKind symbolKind = newSymbol?.Kind ?? SymbolKind.Parameter;
		if (symbolKind == SymbolKind.ErrorType)
		{
			return true;
		}
		if (kind == SymbolKind.Parameter)
		{
			switch (symbolKind)
			{
			case SymbolKind.Local:
			case SymbolKind.Parameter:
				diagnostics.Add(ErrorCode.ERR_LocalIllegallyOverrides, newLocation, name);
				return true;
			case SymbolKind.Method:
				if (((MethodSymbol)newSymbol).MethodKind != MethodKind.LocalFunction)
				{
					break;
				}
				goto case SymbolKind.Local;
			case SymbolKind.TypeParameter:
				return false;
			case SymbolKind.RangeVariable:
				diagnostics.Add(ErrorCode.ERR_QueryRangeVariableOverrides, newLocation, name);
				return true;
			}
		}
		if (kind == SymbolKind.TypeParameter)
		{
			switch (symbolKind)
			{
			case SymbolKind.Local:
			case SymbolKind.Parameter:
				if (parameter.ContainingSymbol is NamedTypeSymbol { IsExtension: not false })
				{
					diagnostics.Add(ErrorCode.ERR_LocalSameNameAsExtensionTypeParameter, newLocation, name);
				}
				else
				{
					diagnostics.Add(ErrorCode.ERR_LocalSameNameAsTypeParam, newLocation, name);
				}
				return true;
			case SymbolKind.Method:
				if (((MethodSymbol)newSymbol).MethodKind != MethodKind.LocalFunction)
				{
					break;
				}
				goto case SymbolKind.Local;
			case SymbolKind.TypeParameter:
				return false;
			case SymbolKind.RangeVariable:
				diagnostics.Add(ErrorCode.ERR_QueryRangeVariableSameAsTypeParam, newLocation, name);
				return true;
			}
		}
		diagnostics.Add(ErrorCode.ERR_InternalError, newLocation);
		return true;
	}

	internal override bool EnsureSingleDefinition(Symbol symbol, string name, Location location, BindingDiagnosticBag diagnostics)
	{
		ImmutableArray<ParameterSymbol> definitions = _methodSymbol.Parameters;
		ImmutableArray<TypeParameterSymbol> immutableArray = _methodSymbol.TypeParameters;
		if (_methodSymbol.IsExtensionBlockMember())
		{
			immutableArray = _methodSymbol.ContainingType.TypeParameters.Concat(immutableArray);
			ParameterSymbol extensionParameter = _methodSymbol.ContainingType.ExtensionParameter;
			if ((object)extensionParameter != null && !(extensionParameter.Name == ""))
			{
				definitions = definitions.Insert(0, extensionParameter);
			}
		}
		if (definitions.IsEmpty && immutableArray.IsEmpty)
		{
			return false;
		}
		SmallDictionary<string, Symbol> smallDictionary = _lazyDefinitionMap;
		if (smallDictionary == null)
		{
			smallDictionary = new SmallDictionary<string, Symbol>();
			RecordDefinition(smallDictionary, definitions);
			RecordDefinition(smallDictionary, immutableArray);
			_lazyDefinitionMap = smallDictionary;
		}
		if (smallDictionary.TryGetValue(name, out var value))
		{
			return ReportConflictWithParameter(value, symbol, name, location, diagnostics);
		}
		return false;
	}
}
