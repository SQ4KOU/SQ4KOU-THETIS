using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Diagnostics;

public abstract class CodeBlockStartAnalysisContext<TLanguageKindEnum> where TLanguageKindEnum : struct
{
	private readonly SyntaxNode _codeBlock;

	private readonly ISymbol _owningSymbol;

	private readonly SemanticModel _semanticModel;

	private readonly AnalyzerOptions _options;

	private readonly CancellationToken _cancellationToken;

	public SyntaxNode CodeBlock => _codeBlock;

	public ISymbol OwningSymbol => _owningSymbol;

	public SemanticModel SemanticModel => _semanticModel;

	public AnalyzerOptions Options => _options;

	public SyntaxTree FilterTree { get; }

	public TextSpan? FilterSpan { get; }

	public bool IsGeneratedCode { get; }

	public CancellationToken CancellationToken => _cancellationToken;

	[Obsolete("Use CompilationWithAnalyzers instead. See https://github.com/dotnet/roslyn/issues/63440 for more details.")]
	protected CodeBlockStartAnalysisContext(SyntaxNode codeBlock, ISymbol owningSymbol, SemanticModel semanticModel, AnalyzerOptions options, CancellationToken cancellationToken)
		: this(codeBlock, owningSymbol, semanticModel, options, (TextSpan?)null, false, cancellationToken)
	{
	}

	private protected CodeBlockStartAnalysisContext(SyntaxNode codeBlock, ISymbol owningSymbol, SemanticModel semanticModel, AnalyzerOptions options, TextSpan? filterSpan, bool isGeneratedCode, CancellationToken cancellationToken)
	{
		_codeBlock = codeBlock;
		_owningSymbol = owningSymbol;
		_semanticModel = semanticModel;
		_options = options;
		FilterTree = codeBlock.SyntaxTree;
		FilterSpan = filterSpan;
		IsGeneratedCode = isGeneratedCode;
		_cancellationToken = cancellationToken;
	}

	public abstract void RegisterCodeBlockEndAction(Action<CodeBlockAnalysisContext> action);

	public void RegisterSyntaxNodeAction(Action<SyntaxNodeAnalysisContext> action, params TLanguageKindEnum[] syntaxKinds)
	{
		RegisterSyntaxNodeAction(action, syntaxKinds.AsImmutableOrEmpty());
	}

	public abstract void RegisterSyntaxNodeAction(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds);
}
