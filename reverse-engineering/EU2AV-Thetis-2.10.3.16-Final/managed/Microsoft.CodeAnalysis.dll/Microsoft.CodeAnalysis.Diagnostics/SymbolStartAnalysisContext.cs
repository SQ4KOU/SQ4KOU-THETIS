using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Diagnostics;

public abstract class SymbolStartAnalysisContext
{
	public ISymbol Symbol { get; }

	public Compilation Compilation { get; }

	public AnalyzerOptions Options { get; }

	public bool IsGeneratedCode { get; }

	public SyntaxTree? FilterTree { get; }

	public TextSpan? FilterSpan { get; }

	public CancellationToken CancellationToken { get; }

	[Obsolete("Use CompilationWithAnalyzers instead. See https://github.com/dotnet/roslyn/issues/63440 for more details.")]
	public SymbolStartAnalysisContext(ISymbol symbol, Compilation compilation, AnalyzerOptions options, CancellationToken cancellationToken)
		: this(symbol, compilation, options, isGeneratedCode: false, null, null, cancellationToken)
	{
	}

	internal SymbolStartAnalysisContext(ISymbol symbol, Compilation compilation, AnalyzerOptions options, bool isGeneratedCode, SyntaxTree? filterTree, TextSpan? filterSpan, CancellationToken cancellationToken)
	{
		Symbol = symbol;
		Compilation = compilation;
		Options = options;
		IsGeneratedCode = isGeneratedCode;
		FilterTree = filterTree;
		FilterSpan = filterSpan;
		CancellationToken = cancellationToken;
	}

	public abstract void RegisterSymbolEndAction(Action<SymbolAnalysisContext> action);

	public abstract void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action) where TLanguageKindEnum : struct;

	public abstract void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action);

	public void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, params TLanguageKindEnum[] syntaxKinds) where TLanguageKindEnum : struct
	{
		RegisterSyntaxNodeAction(action, syntaxKinds.AsImmutableOrEmpty());
	}

	public abstract void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds) where TLanguageKindEnum : struct;

	public abstract void RegisterOperationBlockStartAction(Action<OperationBlockStartAnalysisContext> action);

	public abstract void RegisterOperationBlockAction(Action<OperationBlockAnalysisContext> action);

	public void RegisterOperationAction(Action<OperationAnalysisContext> action, params OperationKind[] operationKinds)
	{
		RegisterOperationAction(action, operationKinds.AsImmutableOrEmpty());
	}

	public abstract void RegisterOperationAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds);
}
