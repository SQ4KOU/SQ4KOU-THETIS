using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class WithExtensionParameterBinder : Binder
{
	private readonly NamedTypeSymbol _type;

	internal WithExtensionParameterBinder(NamedTypeSymbol type, Binder next)
		: base(next)
	{
		_type = type;
	}

	internal override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		if (options.CanConsiderMembers())
		{
			ParameterSymbol extensionParameter = _type.ExtensionParameter;
			if ((object)extensionParameter != null && !(extensionParameter.Name == "") && originalBinder.CanAddLookupSymbolInfo(extensionParameter, options, result, null))
			{
				result.AddSymbol(extensionParameter, extensionParameter.Name, 0);
			}
		}
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, int arity, ConsList<TypeSymbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if ((options & (LookupOptions.NamespaceAliasesOnly | LookupOptions.NamespacesOrTypesOnly)) == 0)
		{
			ParameterSymbol extensionParameter = _type.ExtensionParameter;
			if ((object)extensionParameter != null && !(extensionParameter.Name == "") && extensionParameter.Name == name)
			{
				result.MergeEqual(originalBinder.CheckViability(extensionParameter, arity, options, null, diagnose, ref useSiteInfo));
			}
		}
	}
}
