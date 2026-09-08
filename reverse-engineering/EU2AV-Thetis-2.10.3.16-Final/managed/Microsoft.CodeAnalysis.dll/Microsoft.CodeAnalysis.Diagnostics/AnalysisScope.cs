using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal class AnalysisScope
{
	private readonly Lazy<ImmutableHashSet<DiagnosticAnalyzer>> _lazyAnalyzersSet;

	public SourceOrAdditionalFile? FilterFileOpt { get; }

	public TextSpan? FilterSpanOpt { get; }

	public SourceOrAdditionalFile? OriginalFilterFile { get; }

	public TextSpan? OriginalFilterSpan { get; }

	public ImmutableArray<DiagnosticAnalyzer> Analyzers { get; }

	public ImmutableArray<SyntaxTree> SyntaxTrees { get; }

	public ImmutableArray<AdditionalText> AdditionalFiles { get; }

	public bool ConcurrentAnalysis { get; }

	public bool IsSyntacticSingleFileAnalysis { get; }

	public bool IsSingleFileAnalysis => FilterFileOpt.HasValue;

	private bool HasAllAnalyzers { get; }

	public bool IsSingleFileAnalysisForCompilerAnalyzer
	{
		get
		{
			if (IsSingleFileAnalysis)
			{
				ImmutableArray<DiagnosticAnalyzer> analyzers = Analyzers;
				if (analyzers.Length == 1)
				{
					return analyzers[0] is CompilerDiagnosticAnalyzer;
				}
				return false;
			}
			return false;
		}
	}

	public bool IsSemanticSingleFileAnalysisForCompilerAnalyzer
	{
		get
		{
			if (IsSingleFileAnalysisForCompilerAnalyzer)
			{
				return !IsSyntacticSingleFileAnalysis;
			}
			return false;
		}
	}

	public static AnalysisScope Create(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, CompilationWithAnalyzers compilationWithAnalyzers)
	{
		ImmutableArray<AdditionalText> additionalFiles = compilationWithAnalyzers.AnalysisOptions.Options.GetAdditionalFiles();
		bool hasAllAnalyzers = ComputeHasAllAnalyzers(analyzers, compilationWithAnalyzers);
		bool concurrentAnalysis = compilationWithAnalyzers.AnalysisOptions.ConcurrentAnalysis;
		return Create(compilation, additionalFiles, analyzers, hasAllAnalyzers, concurrentAnalysis);
	}

	public static AnalysisScope CreateForBatchCompile(Compilation compilation, ImmutableArray<AdditionalText> additionalFiles, ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		return Create(compilation, additionalFiles, analyzers, hasAllAnalyzers: true, compilation.Options.ConcurrentBuild);
	}

	private static AnalysisScope Create(Compilation compilation, ImmutableArray<AdditionalText> additionalFiles, ImmutableArray<DiagnosticAnalyzer> analyzers, bool hasAllAnalyzers, bool concurrentAnalysis)
	{
		return new AnalysisScope(compilation.CommonSyntaxTrees, additionalFiles, analyzers, hasAllAnalyzers, null, null, null, null, isSyntacticSingleFileAnalysis: false, concurrentAnalysis);
	}

	public static AnalysisScope Create(ImmutableArray<DiagnosticAnalyzer> analyzers, SourceOrAdditionalFile filterFile, TextSpan? filterSpan, bool isSyntacticSingleFileAnalysis, CompilationWithAnalyzers compilationWithAnalyzers)
	{
		return Create(analyzers, filterFile, filterSpan, filterFile, filterSpan, isSyntacticSingleFileAnalysis, compilationWithAnalyzers);
	}

	public static AnalysisScope Create(ImmutableArray<DiagnosticAnalyzer> analyzers, SourceOrAdditionalFile filterFile, TextSpan? filterSpan, SourceOrAdditionalFile originalFilterFile, TextSpan? originalFilterSpan, bool isSyntacticSingleFileAnalysis, CompilationWithAnalyzers compilationWithAnalyzers)
	{
		return new AnalysisScope((filterFile.SourceTree != null) ? ImmutableArray.Create(filterFile.SourceTree) : ImmutableArray<SyntaxTree>.Empty, (filterFile.AdditionalFile != null) ? ImmutableArray.Create(filterFile.AdditionalFile) : ImmutableArray<AdditionalText>.Empty, hasAllAnalyzers: ComputeHasAllAnalyzers(analyzers, compilationWithAnalyzers), concurrentAnalysis: compilationWithAnalyzers.AnalysisOptions.ConcurrentAnalysis, analyzers: analyzers, filterFile: filterFile, filterSpanOpt: filterSpan, originalFilterFile: originalFilterFile, originalFilterSpan: originalFilterSpan, isSyntacticSingleFileAnalysis: isSyntacticSingleFileAnalysis);
	}

	private AnalysisScope(ImmutableArray<SyntaxTree> trees, ImmutableArray<AdditionalText> additionalFiles, ImmutableArray<DiagnosticAnalyzer> analyzers, bool hasAllAnalyzers, SourceOrAdditionalFile? filterFile, TextSpan? filterSpanOpt, SourceOrAdditionalFile? originalFilterFile, TextSpan? originalFilterSpan, bool isSyntacticSingleFileAnalysis, bool concurrentAnalysis)
	{
		SyntaxTrees = trees;
		AdditionalFiles = additionalFiles;
		Analyzers = analyzers;
		HasAllAnalyzers = hasAllAnalyzers;
		FilterFileOpt = filterFile;
		FilterSpanOpt = GetEffectiveFilterSpan(filterSpanOpt, filterFile);
		OriginalFilterFile = originalFilterFile;
		OriginalFilterSpan = GetEffectiveFilterSpan(originalFilterSpan, originalFilterFile);
		IsSyntacticSingleFileAnalysis = isSyntacticSingleFileAnalysis;
		ConcurrentAnalysis = concurrentAnalysis;
		_lazyAnalyzersSet = new Lazy<ImmutableHashSet<DiagnosticAnalyzer>>(CreateAnalyzersSet);
	}

	private static TextSpan? GetEffectiveFilterSpan(TextSpan? filterSpan, SourceOrAdditionalFile? filterFile)
	{
		if (filterSpan.HasValue && filterFile.GetValueOrDefault().SourceTree != null && filterSpan.GetValueOrDefault().Start == 0 && filterSpan.GetValueOrDefault().Length == filterFile.GetValueOrDefault().SourceTree.Length)
		{
			return null;
		}
		return filterSpan;
	}

	private ImmutableHashSet<DiagnosticAnalyzer> CreateAnalyzersSet()
	{
		return Analyzers.ToImmutableHashSet();
	}

	public bool Contains(DiagnosticAnalyzer analyzer)
	{
		if (HasAllAnalyzers)
		{
			return true;
		}
		return _lazyAnalyzersSet.Value.Contains(analyzer);
	}

	public AnalysisScope WithAnalyzers(ImmutableArray<DiagnosticAnalyzer> analyzers, CompilationWithAnalyzers compilationWithAnalyzers)
	{
		bool hasAllAnalyzers = ComputeHasAllAnalyzers(analyzers, compilationWithAnalyzers);
		return new AnalysisScope(SyntaxTrees, AdditionalFiles, analyzers, hasAllAnalyzers, FilterFileOpt, FilterSpanOpt, OriginalFilterFile, OriginalFilterSpan, IsSyntacticSingleFileAnalysis, ConcurrentAnalysis);
	}

	private static bool ComputeHasAllAnalyzers(ImmutableArray<DiagnosticAnalyzer> analyzers, CompilationWithAnalyzers compilationWithAnalyzers)
	{
		return compilationWithAnalyzers.Analyzers.Length == analyzers.Length;
	}

	public AnalysisScope WithFilterSpan(TextSpan? filterSpan)
	{
		return new AnalysisScope(SyntaxTrees, AdditionalFiles, Analyzers, HasAllAnalyzers, FilterFileOpt, filterSpan, OriginalFilterFile, OriginalFilterSpan, IsSyntacticSingleFileAnalysis, ConcurrentAnalysis);
	}

	public static bool ShouldSkipSymbolAnalysis(SymbolDeclaredCompilationEvent symbolEvent)
	{
		if (!symbolEvent.Symbol.IsImplicitlyDeclared)
		{
			return symbolEvent.DeclaringSyntaxReferences.All((SyntaxReference s) => s.SyntaxTree == null);
		}
		return true;
	}

	public static bool ShouldSkipDeclarationAnalysis(ISymbol symbol)
	{
		if (symbol.IsImplicitlyDeclared)
		{
			if (symbol.Kind == SymbolKind.Namespace)
			{
				return !((INamespaceSymbol)symbol).IsGlobalNamespace;
			}
			return true;
		}
		return false;
	}

	public bool ShouldAnalyze(SyntaxTree tree)
	{
		if (FilterFileOpt.HasValue)
		{
			return FilterFileOpt.GetValueOrDefault().SourceTree == tree;
		}
		return true;
	}

	public bool ShouldAnalyze(AdditionalText file)
	{
		if (FilterFileOpt.HasValue)
		{
			return FilterFileOpt.GetValueOrDefault().AdditionalFile == file;
		}
		return true;
	}

	public bool ShouldAnalyze(SymbolDeclaredCompilationEvent symbolEvent, Func<ISymbol, SyntaxReference, Compilation, CancellationToken, SyntaxNode> getTopmostNodeForAnalysis, CancellationToken cancellationToken)
	{
		if (!FilterFileOpt.HasValue)
		{
			return true;
		}
		SyntaxTree sourceTree = FilterFileOpt.GetValueOrDefault().SourceTree;
		if (sourceTree == null)
		{
			return false;
		}
		foreach (SyntaxReference declaringSyntaxReference in symbolEvent.DeclaringSyntaxReferences)
		{
			if (declaringSyntaxReference.SyntaxTree == sourceTree)
			{
				SyntaxNode syntaxNode = getTopmostNodeForAnalysis(symbolEvent.Symbol, declaringSyntaxReference, symbolEvent.Compilation, cancellationToken);
				if (ShouldInclude(syntaxNode.FullSpan))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool ShouldAnalyze(SyntaxNode node)
	{
		if (!FilterFileOpt.HasValue)
		{
			return true;
		}
		if (FilterFileOpt.GetValueOrDefault().SourceTree == null)
		{
			return false;
		}
		return ShouldInclude(node.FullSpan);
	}

	public bool ShouldInclude(TextSpan filterSpan)
	{
		if (FilterSpanOpt.HasValue)
		{
			return FilterSpanOpt.GetValueOrDefault().IntersectsWith(filterSpan);
		}
		return true;
	}

	public bool ContainsSpan(TextSpan filterSpan)
	{
		if (FilterSpanOpt.HasValue)
		{
			return FilterSpanOpt.GetValueOrDefault().Contains(filterSpan);
		}
		return true;
	}

	public bool ShouldInclude(Diagnostic diagnostic)
	{
		if (!FilterFileOpt.HasValue)
		{
			return true;
		}
		SourceOrAdditionalFile valueOrDefault = FilterFileOpt.GetValueOrDefault();
		if (diagnostic.Location.IsInSource)
		{
			if (diagnostic.Location.SourceTree != valueOrDefault.SourceTree)
			{
				return false;
			}
		}
		else if (diagnostic.Location is ExternalFileLocation externalFileLocation && (valueOrDefault.AdditionalFile == null || !PathUtilities.Comparer.Equals(externalFileLocation.GetLineSpan().Path, valueOrDefault.AdditionalFile.Path)))
		{
			return false;
		}
		return ShouldInclude(diagnostic.Location.SourceSpan);
	}
}
