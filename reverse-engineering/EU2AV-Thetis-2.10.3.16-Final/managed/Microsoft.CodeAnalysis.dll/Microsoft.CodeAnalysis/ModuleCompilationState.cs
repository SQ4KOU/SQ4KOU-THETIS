using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis;

internal class ModuleCompilationState<TNamedTypeSymbol, TMethodSymbol> : CommonModuleCompilationState where TNamedTypeSymbol : class, INamedTypeSymbolInternal where TMethodSymbol : class, IMethodSymbolInternal
{
	private Dictionary<TMethodSymbol, TNamedTypeSymbol>? _lazyStateMachineTypes;

	internal void SetStateMachineType(TMethodSymbol method, TNamedTypeSymbol stateMachineClass)
	{
		if (_lazyStateMachineTypes == null)
		{
			Interlocked.CompareExchange(ref _lazyStateMachineTypes, new Dictionary<TMethodSymbol, TNamedTypeSymbol>(), null);
		}
		lock (_lazyStateMachineTypes)
		{
			_lazyStateMachineTypes.Add(method, stateMachineClass);
		}
	}

	internal bool TryGetStateMachineType(TMethodSymbol method, [NotNullWhen(true)] out TNamedTypeSymbol? stateMachineType)
	{
		stateMachineType = null;
		if (_lazyStateMachineTypes != null)
		{
			return _lazyStateMachineTypes.TryGetValue(method, out stateMachineType);
		}
		return false;
	}
}
