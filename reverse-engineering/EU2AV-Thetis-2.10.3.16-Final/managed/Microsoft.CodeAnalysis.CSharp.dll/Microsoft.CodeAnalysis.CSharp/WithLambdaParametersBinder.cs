using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class WithLambdaParametersBinder : LocalScopeBinder
{
	protected readonly LambdaSymbol lambdaSymbol;

	protected readonly MultiDictionary<string, ParameterSymbol> parameterMap;

	private readonly SmallDictionary<string, ParameterSymbol> _definitionMap;

	internal override Symbol ContainingMemberOrLambda => lambdaSymbol;

	internal override bool IsNestedFunctionBinder => true;

	internal override bool IsDirectlyInIterator => false;

	public WithLambdaParametersBinder(LambdaSymbol lambdaSymbol, Binder enclosing)
		: base(enclosing)
	{
		this.lambdaSymbol = lambdaSymbol;
		parameterMap = new MultiDictionary<string, ParameterSymbol>();
		ImmutableArray<ParameterSymbol> parameters = lambdaSymbol.Parameters;
		if (parameters.IsDefaultOrEmpty)
		{
			return;
		}
		_definitionMap = new SmallDictionary<string, ParameterSymbol>();
		foreach (ParameterSymbol item in parameters)
		{
			if (!item.IsDiscard)
			{
				string name = item.Name;
				parameterMap.Add(name, item);
				if (!_definitionMap.ContainsKey(name))
				{
					_definitionMap.Add(name, item);
				}
			}
		}
	}

	protected override TypeSymbol GetCurrentReturnType(out RefKind refKind)
	{
		refKind = lambdaSymbol.RefKind;
		return lambdaSymbol.ReturnType;
	}

	internal override TypeWithAnnotations GetIteratorElementType()
	{
		return TypeWithAnnotations.Create(CreateErrorType());
	}

	protected override void ValidateYield(YieldStatementSyntax node, BindingDiagnosticBag diagnostics)
	{
		if (node != null)
		{
			diagnostics.Add(ErrorCode.ERR_YieldInAnonMeth, node.YieldKeyword.GetLocation());
		}
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, int arity, ConsList<TypeSymbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if ((options & LookupOptions.NamespaceAliasesOnly) != LookupOptions.Default)
		{
			return;
		}
		foreach (ParameterSymbol item in parameterMap[name])
		{
			result.MergeEqual(originalBinder.CheckViability(item, arity, options, null, diagnose, ref useSiteInfo));
		}
	}

	internal override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		if (!options.CanConsiderMembers())
		{
			return;
		}
		foreach (ParameterSymbol parameter in lambdaSymbol.Parameters)
		{
			if (originalBinder.CanAddLookupSymbolInfo(parameter, options, result, null))
			{
				result.AddSymbol(parameter, parameter.Name, 0);
			}
		}
	}

	private static bool ReportConflictWithParameter(ParameterSymbol parameter, Symbol newSymbol, string name, Location newLocation, BindingDiagnosticBag diagnostics)
	{
		if (parameter.GetFirstLocation() == newLocation)
		{
			return false;
		}
		switch (newSymbol?.Kind ?? SymbolKind.Parameter)
		{
		case SymbolKind.ErrorType:
			return true;
		case SymbolKind.Local:
		case SymbolKind.Parameter:
			diagnostics.Add(ErrorCode.ERR_LocalIllegallyOverrides, newLocation, name);
			return true;
		case SymbolKind.Method:
			return false;
		case SymbolKind.TypeParameter:
			return false;
		case SymbolKind.RangeVariable:
			diagnostics.Add(ErrorCode.ERR_QueryRangeVariableOverrides, newLocation, name);
			return true;
		default:
			diagnostics.Add(ErrorCode.ERR_InternalError, newLocation);
			return false;
		}
	}

	internal override bool EnsureSingleDefinition(Symbol symbol, string name, Location location, BindingDiagnosticBag diagnostics)
	{
		SmallDictionary<string, ParameterSymbol> definitionMap = _definitionMap;
		if (definitionMap != null && definitionMap.TryGetValue(name, out var value))
		{
			return ReportConflictWithParameter(value, symbol, name, location, diagnostics);
		}
		return false;
	}

	internal override ImmutableArray<LocalSymbol> GetDeclaredLocalsForScope(SyntaxNode scopeDesignator)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/WithLambdaParametersBinder.cs", 172);
	}

	internal override ImmutableArray<LocalFunctionSymbol> GetDeclaredLocalFunctionsForScope(CSharpSyntaxNode scopeDesignator)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/WithLambdaParametersBinder.cs", 177);
	}
}
