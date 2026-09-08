using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class SymbolDeclaredCompilationEvent : CompilationEvent
{
	private ImmutableArray<SyntaxReference> _lazyCachedDeclaringReferences;

	public ISymbol Symbol => SymbolInternal.GetISymbol();

	public ISymbolInternal SymbolInternal { get; }

	public SemanticModel? SemanticModelWithCachedBoundNodes { get; }

	public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => InterlockedOperations.Initialize(ref _lazyCachedDeclaringReferences, (SymbolDeclaredCompilationEvent self) => self.Symbol.DeclaringSyntaxReferences, this);

	public SymbolDeclaredCompilationEvent(Compilation compilation, ISymbolInternal symbolInternal, SemanticModel? semanticModelWithCachedBoundNodes = null)
		: base(compilation)
	{
		SymbolInternal = symbolInternal;
		SemanticModelWithCachedBoundNodes = semanticModelWithCachedBoundNodes;
		_lazyCachedDeclaringReferences = default(ImmutableArray<SyntaxReference>);
	}

	public override string ToString()
	{
		string text = Symbol.Name;
		if (text == "")
		{
			text = "<empty>";
		}
		string text2 = ((DeclaringSyntaxReferences.Length != 0) ? (" @ " + string.Join(", ", Enumerable.Select(DeclaringSyntaxReferences, (SyntaxReference r) => r.GetLocation().GetLineSpan()))) : null);
		return "SymbolDeclaredCompilationEvent(" + text + " " + Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) + text2 + ")";
	}
}
