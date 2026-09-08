using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Symbols;

internal interface IAssemblySymbolInternal : ISymbolInternal
{
	Version? AssemblyVersionPattern { get; }

	AssemblyIdentity Identity { get; }

	IAssemblySymbolInternal CorLibrary { get; }

	IEnumerable<ImmutableArray<byte>> GetInternalsVisibleToPublicKeys(string simpleName);

	IEnumerable<string> GetInternalsVisibleToAssemblyNames();

	bool AreInternalsVisibleToThisAssembly(IAssemblySymbolInternal? otherAssembly);
}
