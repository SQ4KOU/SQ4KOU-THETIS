using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal interface IImportScope
{
	IImportScope Parent { get; }

	ImmutableArray<UsedNamespaceOrType> GetUsedNamespaces(EmitContext context);
}
