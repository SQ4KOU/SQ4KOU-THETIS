using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal class AnalyzerExecutor
{
	private sealed class AnalyzerDiagnosticReporter
	{
		public readonly Action<Diagnostic> AddDiagnosticAction;

		private static readonly ObjectPool<AnalyzerDiagnosticReporter> s_objectPool = new ObjectPool<AnalyzerDiagnosticReporter>(() => new AnalyzerDiagnosticReporter(), 10);

		private SourceOrAdditionalFile? _contextFile;

		private Compilation _compilation;

		private DiagnosticAnalyzer _analyzer;

		private AnalyzerOptions _analyzerOptions;

		private bool _isSyntaxDiagnostic;

		private Action<Diagnostic, AnalyzerOptions, CancellationToken>? _addNonCategorizedDiagnostic;

		private Action<Diagnostic, DiagnosticAnalyzer, AnalyzerOptions, bool, CancellationToken>? _addCategorizedLocalDiagnostic;

		private Action<Diagnostic, DiagnosticAnalyzer, AnalyzerOptions, CancellationToken>? _addCategorizedNonLocalDiagnostic;

		private Func<Diagnostic, DiagnosticAnalyzer, Compilation, CancellationToken, bool> _shouldSuppressGeneratedCodeDiagnostic;

		private CancellationToken _cancellationToken;

		public TextSpan? FilterSpanForLocalDiagnostics;

		public static AnalyzerDiagnosticReporter GetInstance(SourceOrAdditionalFile contextFile, TextSpan? span, Compilation compilation, DiagnosticAnalyzer analyzer, AnalyzerOptions analyzerOptions, bool isSyntaxDiagnostic, Action<Diagnostic, AnalyzerOptions, CancellationToken>? addNonCategorizedDiagnostic, Action<Diagnostic, DiagnosticAnalyzer, AnalyzerOptions, bool, CancellationToken>? addCategorizedLocalDiagnostic, Action<Diagnostic, DiagnosticAnalyzer, AnalyzerOptions, CancellationToken>? addCategorizedNonLocalDiagnostic, Func<Diagnostic, DiagnosticAnalyzer, Compilation, CancellationToken, bool> shouldSuppressGeneratedCodeDiagnostic, CancellationToken cancellationToken)
		{
			AnalyzerDiagnosticReporter analyzerDiagnosticReporter = s_objectPool.Allocate();
			analyzerDiagnosticReporter._contextFile = contextFile;
			analyzerDiagnosticReporter.FilterSpanForLocalDiagnostics = span;
			analyzerDiagnosticReporter._compilation = compilation;
			analyzerDiagnosticReporter._analyzer = analyzer;
			analyzerDiagnosticReporter._analyzerOptions = analyzerOptions;
			analyzerDiagnosticReporter._isSyntaxDiagnostic = isSyntaxDiagnostic;
			analyzerDiagnosticReporter._addNonCategorizedDiagnostic = addNonCategorizedDiagnostic;
			analyzerDiagnosticReporter._addCategorizedLocalDiagnostic = addCategorizedLocalDiagnostic;
			analyzerDiagnosticReporter._addCategorizedNonLocalDiagnostic = addCategorizedNonLocalDiagnostic;
			analyzerDiagnosticReporter._shouldSuppressGeneratedCodeDiagnostic = shouldSuppressGeneratedCodeDiagnostic;
			analyzerDiagnosticReporter._cancellationToken = cancellationToken;
			return analyzerDiagnosticReporter;
		}

		public void Free()
		{
			_contextFile = null;
			FilterSpanForLocalDiagnostics = null;
			_compilation = null;
			_analyzer = null;
			_analyzerOptions = null;
			_isSyntaxDiagnostic = false;
			_addNonCategorizedDiagnostic = null;
			_addCategorizedLocalDiagnostic = null;
			_addCategorizedNonLocalDiagnostic = null;
			_shouldSuppressGeneratedCodeDiagnostic = null;
			_cancellationToken = default(CancellationToken);
			s_objectPool.Free(this);
		}

		private AnalyzerDiagnosticReporter()
		{
			AddDiagnosticAction = AddDiagnostic;
		}

		private void AddDiagnostic(Diagnostic diagnostic)
		{
			if (!_shouldSuppressGeneratedCodeDiagnostic(diagnostic, _analyzer, _compilation, _cancellationToken))
			{
				if (_addCategorizedLocalDiagnostic == null)
				{
					_addNonCategorizedDiagnostic(diagnostic, _analyzerOptions, _cancellationToken);
				}
				else if (isLocalDiagnostic(diagnostic) && (!FilterSpanForLocalDiagnostics.HasValue || FilterSpanForLocalDiagnostics.Value.IntersectsWith(diagnostic.Location.SourceSpan)))
				{
					_addCategorizedLocalDiagnostic(diagnostic, _analyzer, _analyzerOptions, _isSyntaxDiagnostic, _cancellationToken);
				}
				else
				{
					_addCategorizedNonLocalDiagnostic(diagnostic, _analyzer, _analyzerOptions, _cancellationToken);
				}
			}
			bool isLocalDiagnostic(Diagnostic diagnostic2)
			{
				if (diagnostic2.Location.IsInSource)
				{
					if (_contextFile?.SourceTree != null)
					{
						return _contextFile.Value.SourceTree == diagnostic2.Location.SourceTree;
					}
					return false;
				}
				if (_contextFile?.AdditionalFile != null && diagnostic2.Location is ExternalFileLocation externalFileLocation)
				{
					return PathUtilities.Comparer.Equals(_contextFile.Value.AdditionalFile.Path, externalFileLocation.GetLineSpan().Path);
				}
				return false;
			}
		}
	}

	private readonly struct ExecutionData(DiagnosticAnalyzer analyzer, AnalyzerOptions analyzerOptions, ISymbol declaredSymbol, SemanticModel semanticModel, TextSpan? filterSpan, bool isGeneratedCode)
	{
		public readonly DiagnosticAnalyzer Analyzer = analyzer;

		public readonly AnalyzerOptions AnalyzerOptions = analyzerOptions;

		public readonly ISymbol DeclaredSymbol = declaredSymbol;

		public readonly SemanticModel SemanticModel = semanticModel;

		public readonly TextSpan? FilterSpan = filterSpan;

		public readonly bool IsGeneratedCode = isGeneratedCode;
	}

	private const string DiagnosticCategory = "Compiler";

	internal const string AnalyzerExceptionDiagnosticId = "AD0001";

	internal const string AnalyzerDriverExceptionDiagnosticId = "AD0002";

	private readonly Action<Diagnostic, AnalyzerOptions, CancellationToken>? _addNonCategorizedDiagnostic;

	private readonly Action<Diagnostic, DiagnosticAnalyzer, AnalyzerOptions, bool, CancellationToken>? _addCategorizedLocalDiagnostic;

	private readonly Action<Diagnostic, DiagnosticAnalyzer, AnalyzerOptions, CancellationToken>? _addCategorizedNonLocalDiagnostic;

	private readonly Action<Suppression>? _addSuppression;

	private readonly Func<Exception, bool>? _analyzerExceptionFilter;

	private readonly AnalyzerManager _analyzerManager;

	private readonly Func<DiagnosticAnalyzer, bool> _isCompilerAnalyzer;

	private readonly Func<DiagnosticAnalyzer, object?> _getAnalyzerGate;

	private readonly Func<SyntaxTree, SemanticModel> _getSemanticModel;

	private readonly Func<DiagnosticAnalyzer, bool> _shouldSkipAnalysisOnGeneratedCode;

	private readonly Func<Diagnostic, DiagnosticAnalyzer, Compilation, CancellationToken, bool> _shouldSuppressGeneratedCodeDiagnostic;

	private readonly Func<SyntaxTree, TextSpan, CancellationToken, bool> _isGeneratedCodeLocation;

	private readonly Func<DiagnosticAnalyzer, SyntaxTree, SyntaxTreeOptionsProvider?, CancellationToken, bool> _isAnalyzerSuppressedForTree;

	private readonly ConcurrentDictionary<DiagnosticAnalyzer, StrongBox<long>>? _analyzerExecutionTimeMap;

	private readonly CompilationAnalysisValueProviderFactory _compilationAnalysisValueProviderFactory;

	private readonly Dictionary<DiagnosticAnalyzer, AnalyzerOptions>? _analyzerToCachedOptions;

	private Func<IOperation, ControlFlowGraph>? _lazyGetControlFlowGraph;

	private ConcurrentDictionary<IOperation, ControlFlowGraph>? _lazyControlFlowGraphMap;

	private Func<IOperation, ControlFlowGraph> GetControlFlowGraph => GetControlFlowGraphImpl;

	internal Compilation Compilation { get; }

	internal AnalyzerOptions AnalyzerOptions { get; }

	internal SeverityFilter SeverityFilter { get; }

	internal Action<Exception, DiagnosticAnalyzer, Diagnostic, CancellationToken> OnAnalyzerException { get; }

	internal ImmutableDictionary<DiagnosticAnalyzer, TimeSpan> AnalyzerExecutionTimes => _analyzerExecutionTimeMap.ToImmutableDictionary<KeyValuePair<DiagnosticAnalyzer, StrongBox<long>>, DiagnosticAnalyzer, TimeSpan>((KeyValuePair<DiagnosticAnalyzer, StrongBox<long>> pair) => pair.Key, (KeyValuePair<DiagnosticAnalyzer, StrongBox<long>> pair) => TimeSpan.FromTicks(pair.Value.Value));

	private bool IsAnalyzerSuppressedForTree(DiagnosticAnalyzer analyzer, SyntaxTree tree, CancellationToken cancellationToken)
	{
		return _isAnalyzerSuppressedForTree(analyzer, tree, Compilation.Options.SyntaxTreeOptionsProvider, cancellationToken);
	}

	public static AnalyzerExecutor Create(Compilation compilation, AnalyzerOptions analyzerOptions, Action<Diagnostic, AnalyzerOptions, CancellationToken>? addNonCategorizedDiagnostic, Action<Exception, DiagnosticAnalyzer, Diagnostic, CancellationToken> onAnalyzerException, Func<Exception, bool>? analyzerExceptionFilter, Func<DiagnosticAnalyzer, bool> isCompilerAnalyzer, ImmutableArray<DiagnosticAnalyzer> diagnosticAnalyzers, Func<DiagnosticAnalyzer, AnalyzerConfigOptionsProvider>? getAnalyzerConfigOptionsProvider, AnalyzerManager analyzerManager, Func<DiagnosticAnalyzer, bool> shouldSkipAnalysisOnGeneratedCode, Func<Diagnostic, DiagnosticAnalyzer, Compilation, CancellationToken, bool> shouldSuppressGeneratedCodeDiagnostic, Func<SyntaxTree, TextSpan, CancellationToken, bool> isGeneratedCodeLocation, Func<DiagnosticAnalyzer, SyntaxTree, SyntaxTreeOptionsProvider?, CancellationToken, bool> isAnalyzerSuppressedForTree, Func<DiagnosticAnalyzer, object?> getAnalyzerGate, Func<SyntaxTree, SemanticModel> getSemanticModel, SeverityFilter severityFilter, bool logExecutionTime = false, Action<Diagnostic, DiagnosticAnalyzer, AnalyzerOptions, bool, CancellationToken>? addCategorizedLocalDiagnostic = null, Action<Diagnostic, DiagnosticAnalyzer, AnalyzerOptions, CancellationToken>? addCategorizedNonLocalDiagnostic = null, Action<Suppression>? addSuppression = null)
	{
		ConcurrentDictionary<DiagnosticAnalyzer, StrongBox<long>> analyzerExecutionTimeMap = (logExecutionTime ? new ConcurrentDictionary<DiagnosticAnalyzer, StrongBox<long>>() : null);
		return new AnalyzerExecutor(compilation, analyzerOptions, addNonCategorizedDiagnostic, onAnalyzerException, analyzerExceptionFilter, isCompilerAnalyzer, diagnosticAnalyzers, getAnalyzerConfigOptionsProvider, analyzerManager, shouldSkipAnalysisOnGeneratedCode, shouldSuppressGeneratedCodeDiagnostic, isGeneratedCodeLocation, isAnalyzerSuppressedForTree, getAnalyzerGate, getSemanticModel, severityFilter, analyzerExecutionTimeMap, addCategorizedLocalDiagnostic, addCategorizedNonLocalDiagnostic, addSuppression);
	}

	private AnalyzerExecutor(Compilation compilation, AnalyzerOptions analyzerOptions, Action<Diagnostic, AnalyzerOptions, CancellationToken>? addNonCategorizedDiagnosticOpt, Action<Exception, DiagnosticAnalyzer, Diagnostic, CancellationToken> onAnalyzerException, Func<Exception, bool>? analyzerExceptionFilter, Func<DiagnosticAnalyzer, bool> isCompilerAnalyzer, ImmutableArray<DiagnosticAnalyzer> diagnosticAnalyzers, Func<DiagnosticAnalyzer, AnalyzerConfigOptionsProvider>? getAnalyzerConfigOptionsProvider, AnalyzerManager analyzerManager, Func<DiagnosticAnalyzer, bool> shouldSkipAnalysisOnGeneratedCode, Func<Diagnostic, DiagnosticAnalyzer, Compilation, CancellationToken, bool> shouldSuppressGeneratedCodeDiagnostic, Func<SyntaxTree, TextSpan, CancellationToken, bool> isGeneratedCodeLocation, Func<DiagnosticAnalyzer, SyntaxTree, SyntaxTreeOptionsProvider?, CancellationToken, bool> isAnalyzerSuppressedForTree, Func<DiagnosticAnalyzer, object?> getAnalyzerGate, Func<SyntaxTree, SemanticModel> getSemanticModel, SeverityFilter severityFilter, ConcurrentDictionary<DiagnosticAnalyzer, StrongBox<long>>? analyzerExecutionTimeMap, Action<Diagnostic, DiagnosticAnalyzer, AnalyzerOptions, bool, CancellationToken>? addCategorizedLocalDiagnostic, Action<Diagnostic, DiagnosticAnalyzer, AnalyzerOptions, CancellationToken>? addCategorizedNonLocalDiagnostic, Action<Suppression>? addSuppression)
	{
		Compilation = compilation;
		AnalyzerOptions = analyzerOptions;
		_addNonCategorizedDiagnostic = addNonCategorizedDiagnosticOpt;
		OnAnalyzerException = onAnalyzerException;
		_analyzerExceptionFilter = analyzerExceptionFilter;
		_isCompilerAnalyzer = isCompilerAnalyzer;
		_analyzerManager = analyzerManager;
		_shouldSkipAnalysisOnGeneratedCode = shouldSkipAnalysisOnGeneratedCode;
		_shouldSuppressGeneratedCodeDiagnostic = shouldSuppressGeneratedCodeDiagnostic;
		_isGeneratedCodeLocation = isGeneratedCodeLocation;
		_isAnalyzerSuppressedForTree = isAnalyzerSuppressedForTree;
		_getAnalyzerGate = getAnalyzerGate;
		_getSemanticModel = getSemanticModel;
		SeverityFilter = severityFilter;
		_analyzerExecutionTimeMap = analyzerExecutionTimeMap;
		_addCategorizedLocalDiagnostic = addCategorizedLocalDiagnostic;
		_addCategorizedNonLocalDiagnostic = addCategorizedNonLocalDiagnostic;
		_addSuppression = addSuppression;
		_compilationAnalysisValueProviderFactory = new CompilationAnalysisValueProviderFactory();
		if (getAnalyzerConfigOptionsProvider == null)
		{
			return;
		}
		bool flag = false;
		Dictionary<DiagnosticAnalyzer, AnalyzerOptions> dictionary = new Dictionary<DiagnosticAnalyzer, AnalyzerOptions>(diagnosticAnalyzers.Length, ReferenceEqualityComparer.Instance);
		Dictionary<AnalyzerConfigOptionsProvider, AnalyzerOptions> dictionary2 = new Dictionary<AnalyzerConfigOptionsProvider, AnalyzerOptions>(ReferenceEqualityComparer.Instance);
		foreach (DiagnosticAnalyzer item in diagnosticAnalyzers)
		{
			AnalyzerConfigOptionsProvider specificOptionsProvider = getAnalyzerConfigOptionsProvider(item);
			AnalyzerOptions analyzerOptions2 = (dictionary[item] = dictionary2.GetOrAdd(specificOptionsProvider, () => analyzerOptions.WithAnalyzerConfigOptionsProvider(specificOptionsProvider)));
			if (analyzerOptions2 != analyzerOptions)
			{
				flag = true;
			}
		}
		if (flag)
		{
			_analyzerToCachedOptions = dictionary;
		}
	}

	public void ExecuteInitializeMethod(HostSessionStartAnalysisScope sessionScope, SeverityFilter severityFilter, CancellationToken cancellationToken)
	{
		AnalyzerAnalysisContext item = new AnalyzerAnalysisContext(sessionScope, severityFilter);
		ExecuteAndCatchIfThrows(sessionScope.Analyzer, delegate((HostSessionStartAnalysisScope sessionScope, AnalyzerAnalysisContext context) data)
		{
			data.sessionScope.Analyzer.Initialize(data.context);
		}, (sessionScope, item), null, cancellationToken);
	}

	public void ExecuteCompilationStartActions(ImmutableArray<CompilationStartAnalyzerAction> actions, HostCompilationStartAnalysisScope compilationScope, CancellationToken cancellationToken)
	{
		AnalyzerCompilationStartAnalysisContext analyzerCompilationStartAnalysisContext = new AnalyzerCompilationStartAnalysisContext(compilationScope, Compilation, AnalyzerOptions, _compilationAnalysisValueProviderFactory, cancellationToken);
		AnalysisContextInfo value = new AnalysisContextInfo(Compilation);
		foreach (CompilationStartAnalyzerAction item in actions)
		{
			analyzerCompilationStartAnalysisContext = WithAnalyzerSpecificOptions(analyzerCompilationStartAnalysisContext, item.Analyzer, (AnalyzerCompilationStartAnalysisContext context, AnalyzerOptions options) => context.WithOptions(options));
			ExecuteAndCatchIfThrows(item.Analyzer, delegate((CompilationStartAnalyzerAction startAction, AnalyzerCompilationStartAnalysisContext context) data)
			{
				data.startAction.Action(data.context);
			}, (item, analyzerCompilationStartAnalysisContext), value, cancellationToken);
		}
	}

	private TAnalysisContext WithAnalyzerSpecificOptions<TAnalysisContext>(TAnalysisContext context, DiagnosticAnalyzer analyzer, Func<TAnalysisContext, AnalyzerOptions, TAnalysisContext> withOptions)
	{
		if (_analyzerToCachedOptions == null)
		{
			return context;
		}
		return withOptions(context, GetAnalyzerSpecificOptions(analyzer));
	}

	private AnalyzerOptions GetAnalyzerSpecificOptions(DiagnosticAnalyzer analyzer)
	{
		return _analyzerToCachedOptions?[analyzer] ?? AnalyzerOptions;
	}

	public void ExecuteSymbolStartActions(ISymbol symbol, ImmutableArray<SymbolStartAnalyzerAction> actions, HostSymbolStartAnalysisScope symbolScope, bool isGeneratedCodeSymbol, SyntaxTree? filterTree, TextSpan? filterSpan, CancellationToken cancellationToken)
	{
		if ((isGeneratedCodeSymbol && _shouldSkipAnalysisOnGeneratedCode(symbolScope.Analyzer)) || IsAnalyzerSuppressedForSymbol(symbolScope.Analyzer, symbol, cancellationToken))
		{
			return;
		}
		AnalyzerSymbolStartAnalysisContext analyzerSymbolStartAnalysisContext = new AnalyzerSymbolStartAnalysisContext(symbolScope, symbol, Compilation, AnalyzerOptions, isGeneratedCodeSymbol, filterTree, filterSpan, cancellationToken);
		AnalysisContextInfo value = new AnalysisContextInfo(Compilation, symbol);
		foreach (SymbolStartAnalyzerAction item in actions)
		{
			analyzerSymbolStartAnalysisContext = WithAnalyzerSpecificOptions(analyzerSymbolStartAnalysisContext, item.Analyzer, (AnalyzerSymbolStartAnalysisContext context, AnalyzerOptions options) => context.WithOptions(options));
			ExecuteAndCatchIfThrows(item.Analyzer, delegate((SymbolStartAnalyzerAction startAction, AnalyzerSymbolStartAnalysisContext context) data)
			{
				data.startAction.Action(data.context);
			}, (item, analyzerSymbolStartAnalysisContext), value, cancellationToken);
		}
	}

	public void ExecuteSuppressionAction(DiagnosticSuppressor suppressor, ImmutableArray<Diagnostic> reportedDiagnostics, CancellationToken cancellationToken)
	{
		if (reportedDiagnostics.IsEmpty)
		{
			return;
		}
		cancellationToken.ThrowIfCancellationRequested();
		ImmutableArray<SuppressionDescriptor> supportedSuppressionDescriptors = _analyzerManager.GetSupportedSuppressionDescriptors(suppressor, this, cancellationToken);
		Func<SuppressionDescriptor, bool> boundFunction;
		using (PooledDelegates.GetPooledFunction((SuppressionDescriptor d, ImmutableArray<SuppressionDescriptor> supportedSuppressions) => supportedSuppressions.Contains(d), supportedSuppressionDescriptors, out boundFunction))
		{
			AnalyzerOptions analyzerSpecificOptions = GetAnalyzerSpecificOptions(suppressor);
			SuppressionAnalysisContext item = new SuppressionAnalysisContext(Compilation, analyzerSpecificOptions, reportedDiagnostics, _addSuppression, boundFunction, _getSemanticModel, cancellationToken);
			ExecuteAndCatchIfThrows(suppressor, delegate((DiagnosticSuppressor suppressor, SuppressionAnalysisContext context) data)
			{
				data.suppressor.ReportSuppressions(data.context);
			}, (suppressor, item), new AnalysisContextInfo(Compilation), cancellationToken);
		}
	}

	public void ExecuteCompilationActions(ImmutableArray<CompilationAnalyzerAction> compilationActions, DiagnosticAnalyzer analyzer, CompilationEvent compilationEvent, CancellationToken cancellationToken)
	{
		AnalyzerOptions analyzerSpecificOptions = GetAnalyzerSpecificOptions(analyzer);
		Action<Diagnostic> addCompilationDiagnostic = GetAddCompilationDiagnostic(analyzer, analyzerSpecificOptions, cancellationToken);
		Func<Diagnostic, CancellationToken, bool> boundFunction;
		using (PooledDelegates.GetPooledFunction((Diagnostic d, CancellationToken ct, (AnalyzerExecutor self, DiagnosticAnalyzer analyzer) arg) => arg.self.IsSupportedDiagnostic(arg.analyzer, d, ct), (this, analyzer), out boundFunction))
		{
			CompilationAnalysisContext item = new CompilationAnalysisContext(Compilation, analyzerSpecificOptions, addCompilationDiagnostic, boundFunction, _compilationAnalysisValueProviderFactory, cancellationToken);
			AnalysisContextInfo value = new AnalysisContextInfo(Compilation);
			foreach (CompilationAnalyzerAction item2 in compilationActions)
			{
				ExecuteAndCatchIfThrows(item2.Analyzer, delegate((CompilationAnalyzerAction endAction, CompilationAnalysisContext context) data)
				{
					data.endAction.Action(data.context);
				}, (item2, item), value, cancellationToken);
			}
		}
	}

	public void ExecuteSymbolActions(ImmutableArray<SymbolAnalyzerAction> symbolActions, DiagnosticAnalyzer analyzer, SymbolDeclaredCompilationEvent symbolDeclaredEvent, Func<ISymbol, SyntaxReference, Compilation, CancellationToken, SyntaxNode> getTopMostNodeForAnalysis, bool isGeneratedCodeSymbol, SyntaxTree? filterTree, TextSpan? filterSpan, CancellationToken cancellationToken)
	{
		if ((isGeneratedCodeSymbol && _shouldSkipAnalysisOnGeneratedCode(analyzer)) || IsAnalyzerSuppressedForSymbol(analyzer, symbolDeclaredEvent.Symbol, cancellationToken))
		{
			return;
		}
		ISymbol symbol = symbolDeclaredEvent.Symbol;
		AnalyzerOptions analyzerSpecificOptions = GetAnalyzerSpecificOptions(analyzer);
		Action<Diagnostic> boundAction;
		using (PooledDelegates.GetPooledAction(delegate(Diagnostic diagnostic, (AnalyzerExecutor self, DiagnosticAnalyzer analyzer, SymbolDeclaredCompilationEvent symbolDeclaredEvent, AnalyzerOptions analyzerOptions, Func<ISymbol, SyntaxReference, Compilation, CancellationToken, SyntaxNode> getTopMostNodeForAnalysis, CancellationToken cancellationToken) tuple)
		{
			(AnalyzerExecutor self, DiagnosticAnalyzer analyzer, SymbolDeclaredCompilationEvent symbolDeclaredEvent, AnalyzerOptions analyzerOptions, Func<ISymbol, SyntaxReference, Compilation, CancellationToken, SyntaxNode> getTopMostNodeForAnalysis, CancellationToken cancellationToken) tuple2 = tuple;
			DiagnosticAnalyzer item2 = tuple2.analyzer;
			SymbolDeclaredCompilationEvent item3 = tuple2.symbolDeclaredEvent;
			AnalyzerOptions item4 = tuple2.analyzerOptions;
			Func<ISymbol, SyntaxReference, Compilation, CancellationToken, SyntaxNode> item5 = tuple2.getTopMostNodeForAnalysis;
			CancellationToken item6 = tuple2.cancellationToken;
			tuple.self.AddSymbolDiagnostic(item3, diagnostic, item2, item4, item5, item6);
		}, (this, analyzer, symbolDeclaredEvent, analyzerSpecificOptions, getTopMostNodeForAnalysis, cancellationToken), out boundAction))
		{
			Func<Diagnostic, CancellationToken, bool> boundFunction;
			using (PooledDelegates.GetPooledFunction((Diagnostic d, CancellationToken ct, (AnalyzerExecutor self, DiagnosticAnalyzer analyzer) arg) => arg.self.IsSupportedDiagnostic(arg.analyzer, d, ct), (this, analyzer), out boundFunction))
			{
				SymbolAnalysisContext item = new SymbolAnalysisContext(symbol, Compilation, analyzerSpecificOptions, boundAction, boundFunction, isGeneratedCodeSymbol, filterTree, filterSpan, cancellationToken);
				AnalysisContextInfo value = new AnalysisContextInfo(Compilation, symbol);
				foreach (SymbolAnalyzerAction item7 in symbolActions)
				{
					cancellationToken.ThrowIfCancellationRequested();
					if (item7.Kinds.Contains(symbol.Kind))
					{
						ExecuteAndCatchIfThrows(item7.Analyzer, delegate((SymbolAnalyzerAction symbolAction, SymbolAnalysisContext context) data)
						{
							data.symbolAction.Action(data.context);
						}, (item7, item), value, cancellationToken);
					}
				}
			}
		}
	}

	public bool TryExecuteSymbolEndActionsForContainer(INamespaceOrTypeSymbol containingSymbol, ISymbol processedMemberSymbol, DiagnosticAnalyzer analyzer, Func<ISymbol, SyntaxReference, Compilation, CancellationToken, SyntaxNode> getTopMostNodeForAnalysis, bool isGeneratedCode, SyntaxTree? filterTree, TextSpan? filterSpan, CancellationToken cancellationToken, [NotNullWhen(true)] out SymbolDeclaredCompilationEvent? containingSymbolDeclaredEvent)
	{
		containingSymbolDeclaredEvent = null;
		if (!_analyzerManager.TryProcessCompletedMemberAndGetPendingSymbolEndActionsForContainer(containingSymbol, processedMemberSymbol, analyzer, out (ImmutableArray<SymbolEndAnalyzerAction>, SymbolDeclaredCompilationEvent) containerEndActionsAndEvent))
		{
			return false;
		}
		ImmutableArray<SymbolEndAnalyzerAction> symbolEndActions;
		(symbolEndActions, containingSymbolDeclaredEvent) = containerEndActionsAndEvent;
		ExecuteSymbolEndActionsCore(symbolEndActions, analyzer, containingSymbolDeclaredEvent, getTopMostNodeForAnalysis, isGeneratedCode, filterTree, filterSpan, cancellationToken);
		return true;
	}

	public bool TryExecuteSymbolEndActions(ImmutableArray<SymbolEndAnalyzerAction> symbolEndActions, DiagnosticAnalyzer analyzer, SymbolDeclaredCompilationEvent symbolDeclaredEvent, Func<ISymbol, SyntaxReference, Compilation, CancellationToken, SyntaxNode> getTopMostNodeForAnalysis, bool isGeneratedCode, SyntaxTree? filterTree, TextSpan? filterSpan, CancellationToken cancellationToken)
	{
		if (!_analyzerManager.TryStartExecuteSymbolEndActions(symbolEndActions, analyzer, symbolDeclaredEvent))
		{
			return false;
		}
		ExecuteSymbolEndActionsCore(symbolEndActions, analyzer, symbolDeclaredEvent, getTopMostNodeForAnalysis, isGeneratedCode, filterTree, filterSpan, cancellationToken);
		return true;
	}

	private void ExecuteSymbolEndActionsCore(ImmutableArray<SymbolEndAnalyzerAction> symbolEndActions, DiagnosticAnalyzer analyzer, SymbolDeclaredCompilationEvent symbolDeclaredEvent, Func<ISymbol, SyntaxReference, Compilation, CancellationToken, SyntaxNode> getTopMostNodeForAnalysis, bool isGeneratedCode, SyntaxTree? filterTree, TextSpan? filterSpan, CancellationToken cancellationToken)
	{
		ISymbol symbol = symbolDeclaredEvent.Symbol;
		AnalyzerOptions analyzerSpecificOptions = GetAnalyzerSpecificOptions(analyzer);
		Action<Diagnostic> boundAction;
		using (PooledDelegates.GetPooledAction(delegate(Diagnostic diagnostic, (AnalyzerExecutor self, DiagnosticAnalyzer analyzer, SymbolDeclaredCompilationEvent symbolDeclaredEvent, AnalyzerOptions analyzerOptions, Func<ISymbol, SyntaxReference, Compilation, CancellationToken, SyntaxNode> getTopMostNodeForAnalysis, CancellationToken cancellationToken) tuple)
		{
			(AnalyzerExecutor self, DiagnosticAnalyzer analyzer, SymbolDeclaredCompilationEvent symbolDeclaredEvent, AnalyzerOptions analyzerOptions, Func<ISymbol, SyntaxReference, Compilation, CancellationToken, SyntaxNode> getTopMostNodeForAnalysis, CancellationToken cancellationToken) tuple2 = tuple;
			DiagnosticAnalyzer item2 = tuple2.analyzer;
			SymbolDeclaredCompilationEvent item3 = tuple2.symbolDeclaredEvent;
			AnalyzerOptions item4 = tuple2.analyzerOptions;
			Func<ISymbol, SyntaxReference, Compilation, CancellationToken, SyntaxNode> item5 = tuple2.getTopMostNodeForAnalysis;
			CancellationToken item6 = tuple2.cancellationToken;
			tuple.self.AddSymbolDiagnostic(item3, diagnostic, item2, item4, item5, item6);
		}, (this, analyzer, symbolDeclaredEvent, analyzerSpecificOptions, getTopMostNodeForAnalysis, cancellationToken), out boundAction))
		{
			Func<Diagnostic, CancellationToken, bool> boundFunction;
			using (PooledDelegates.GetPooledFunction((Diagnostic d, CancellationToken ct, (AnalyzerExecutor self, DiagnosticAnalyzer analyzer) arg) => arg.self.IsSupportedDiagnostic(arg.analyzer, d, ct), (this, analyzer), out boundFunction))
			{
				SymbolAnalysisContext item = new SymbolAnalysisContext(symbol, Compilation, analyzerSpecificOptions, boundAction, boundFunction, isGeneratedCode, filterTree, filterSpan, cancellationToken);
				AnalysisContextInfo value = new AnalysisContextInfo(Compilation, symbol);
				foreach (SymbolEndAnalyzerAction item7 in symbolEndActions)
				{
					ExecuteAndCatchIfThrows(item7.Analyzer, delegate((SymbolEndAnalyzerAction symbolAction, SymbolAnalysisContext context) data)
					{
						data.symbolAction.Action(data.context);
					}, (item7, item), value, cancellationToken);
				}
				_analyzerManager.MarkSymbolEndAnalysisComplete(symbol, analyzer);
			}
		}
	}

	public void ExecuteSemanticModelActions(ImmutableArray<SemanticModelAnalyzerAction> semanticModelActions, DiagnosticAnalyzer analyzer, SemanticModel semanticModel, TextSpan? filterSpan, bool isGeneratedCode, CancellationToken cancellationToken)
	{
		if ((isGeneratedCode && _shouldSkipAnalysisOnGeneratedCode(analyzer)) || IsAnalyzerSuppressedForTree(analyzer, semanticModel.SyntaxTree, cancellationToken))
		{
			return;
		}
		AnalyzerOptions analyzerSpecificOptions = GetAnalyzerSpecificOptions(analyzer);
		AnalyzerDiagnosticReporter addSemanticDiagnostic = GetAddSemanticDiagnostic(semanticModel.SyntaxTree, analyzer, analyzerSpecificOptions, cancellationToken);
		Func<Diagnostic, CancellationToken, bool> boundFunction;
		using (PooledDelegates.GetPooledFunction((Diagnostic d, CancellationToken ct, (AnalyzerExecutor self, DiagnosticAnalyzer analyzer) arg) => arg.self.IsSupportedDiagnostic(arg.analyzer, d, ct), (this, analyzer), out boundFunction))
		{
			SemanticModelAnalysisContext item = new SemanticModelAnalysisContext(semanticModel, analyzerSpecificOptions, addSemanticDiagnostic.AddDiagnosticAction, boundFunction, filterSpan, isGeneratedCode, cancellationToken);
			AnalysisContextInfo value = new AnalysisContextInfo(semanticModel);
			foreach (SemanticModelAnalyzerAction item2 in semanticModelActions)
			{
				ExecuteAndCatchIfThrows(item2.Analyzer, delegate((SemanticModelAnalyzerAction semanticModelAction, SemanticModelAnalysisContext context) data)
				{
					data.semanticModelAction.Action(data.context);
				}, (item2, item), value, cancellationToken);
			}
			addSemanticDiagnostic.Free();
		}
	}

	public void ExecuteSyntaxTreeActions(ImmutableArray<SyntaxTreeAnalyzerAction> syntaxTreeActions, DiagnosticAnalyzer analyzer, SourceOrAdditionalFile file, TextSpan? filterSpan, bool isGeneratedCode, CancellationToken cancellationToken)
	{
		SyntaxTree sourceTree = file.SourceTree;
		if ((isGeneratedCode && _shouldSkipAnalysisOnGeneratedCode(analyzer)) || IsAnalyzerSuppressedForTree(analyzer, sourceTree, cancellationToken))
		{
			return;
		}
		AnalyzerOptions analyzerSpecificOptions = GetAnalyzerSpecificOptions(analyzer);
		AnalyzerDiagnosticReporter addSyntaxDiagnostic = GetAddSyntaxDiagnostic(file, analyzer, analyzerSpecificOptions, cancellationToken);
		Func<Diagnostic, CancellationToken, bool> boundFunction;
		using (PooledDelegates.GetPooledFunction((Diagnostic d, CancellationToken ct, (AnalyzerExecutor self, DiagnosticAnalyzer analyzer) arg) => arg.self.IsSupportedDiagnostic(arg.analyzer, d, ct), (this, analyzer), out boundFunction))
		{
			SyntaxTreeAnalysisContext item = new SyntaxTreeAnalysisContext(sourceTree, analyzerSpecificOptions, addSyntaxDiagnostic.AddDiagnosticAction, boundFunction, Compilation, filterSpan, isGeneratedCode, cancellationToken);
			AnalysisContextInfo value = new AnalysisContextInfo(Compilation, file);
			foreach (SyntaxTreeAnalyzerAction item2 in syntaxTreeActions)
			{
				ExecuteAndCatchIfThrows(item2.Analyzer, delegate((SyntaxTreeAnalyzerAction syntaxTreeAction, SyntaxTreeAnalysisContext context) data)
				{
					data.syntaxTreeAction.Action(data.context);
				}, (item2, item), value, cancellationToken);
			}
			addSyntaxDiagnostic.Free();
		}
	}

	public void ExecuteAdditionalFileActions(ImmutableArray<AdditionalFileAnalyzerAction> additionalFileActions, DiagnosticAnalyzer analyzer, SourceOrAdditionalFile file, TextSpan? filterSpan, CancellationToken cancellationToken)
	{
		AdditionalText additionalFile = file.AdditionalFile;
		AnalyzerOptions analyzerSpecificOptions = GetAnalyzerSpecificOptions(analyzer);
		AnalyzerDiagnosticReporter addSyntaxDiagnostic = GetAddSyntaxDiagnostic(file, analyzer, analyzerSpecificOptions, cancellationToken);
		Func<Diagnostic, CancellationToken, bool> boundFunction;
		using (PooledDelegates.GetPooledFunction((Diagnostic d, CancellationToken ct, (AnalyzerExecutor self, DiagnosticAnalyzer analyzer) arg) => arg.self.IsSupportedDiagnostic(arg.analyzer, d, ct), (this, analyzer), out boundFunction))
		{
			AdditionalFileAnalysisContext item = new AdditionalFileAnalysisContext(additionalFile, analyzerSpecificOptions, addSyntaxDiagnostic.AddDiagnosticAction, boundFunction, Compilation, filterSpan, cancellationToken);
			AnalysisContextInfo value = new AnalysisContextInfo(Compilation, file);
			foreach (AdditionalFileAnalyzerAction item2 in additionalFileActions)
			{
				ExecuteAndCatchIfThrows(item2.Analyzer, delegate((AdditionalFileAnalyzerAction additionalFileAction, AdditionalFileAnalysisContext context) data)
				{
					data.additionalFileAction.Action(data.context);
				}, (item2, item), value, cancellationToken);
			}
			addSyntaxDiagnostic.Free();
		}
	}

	private void ExecuteSyntaxNodeAction<TLanguageKindEnum>(SyntaxNodeAnalyzerAction<TLanguageKindEnum> syntaxNodeAction, SyntaxNode node, ExecutionData executionData, Action<Diagnostic> addDiagnostic, Func<Diagnostic, CancellationToken, bool> isSupportedDiagnostic, CancellationToken cancellationToken) where TLanguageKindEnum : struct
	{
		ExecuteAndCatchIfThrows(argument: (syntaxNodeAction, new SyntaxNodeAnalysisContext(node, executionData.DeclaredSymbol, executionData.SemanticModel, GetAnalyzerSpecificOptions(syntaxNodeAction.Analyzer), addDiagnostic, isSupportedDiagnostic, executionData.FilterSpan, executionData.IsGeneratedCode, cancellationToken)), analyzer: syntaxNodeAction.Analyzer, analyze: delegate((SyntaxNodeAnalyzerAction<TLanguageKindEnum> syntaxNodeAction, SyntaxNodeAnalysisContext syntaxNodeContext) data)
		{
			data.syntaxNodeAction.Action(data.syntaxNodeContext);
		}, contextInfo: new AnalysisContextInfo(Compilation, node), cancellationToken: cancellationToken);
	}

	private void ExecuteOperationAction(OperationAnalyzerAction operationAction, IOperation operation, ExecutionData executionData, Action<Diagnostic> addDiagnostic, Func<Diagnostic, CancellationToken, bool> isSupportedDiagnostic, CancellationToken cancellationToken)
	{
		ExecuteAndCatchIfThrows(argument: (operationAction, new OperationAnalysisContext(operation, executionData.DeclaredSymbol, executionData.SemanticModel.Compilation, GetAnalyzerSpecificOptions(operationAction.Analyzer), addDiagnostic, isSupportedDiagnostic, GetControlFlowGraph, executionData.FilterSpan, executionData.IsGeneratedCode, cancellationToken)), analyzer: operationAction.Analyzer, analyze: delegate((OperationAnalyzerAction operationAction, OperationAnalysisContext operationContext) data)
		{
			data.operationAction.Action(data.operationContext);
		}, contextInfo: new AnalysisContextInfo(Compilation, operation), cancellationToken: cancellationToken);
	}

	public void ExecuteCodeBlockActions<TLanguageKindEnum>(ImmutableArray<CodeBlockStartAnalyzerAction<TLanguageKindEnum>> startActions, ImmutableArray<CodeBlockAnalyzerAction> actions, ImmutableArray<CodeBlockAnalyzerAction> endActions, DiagnosticAnalyzer analyzer, SyntaxNode declaredNode, ISymbol declaredSymbol, ImmutableArray<SyntaxNode> executableCodeBlocks, SemanticModel semanticModel, Func<SyntaxNode, TLanguageKindEnum> getKind, TextSpan? filterSpan, bool isGeneratedCode, CancellationToken cancellationToken) where TLanguageKindEnum : struct
	{
		ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>> instance = ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>.GetInstance();
		AnalyzerOptions analyzerSpecificOptions = GetAnalyzerSpecificOptions(analyzer);
		ExecuteBlockActionsCore<CodeBlockStartAnalyzerAction<TLanguageKindEnum>, CodeBlockAnalyzerAction, (AnalyzerExecutor, ImmutableArray<CodeBlockStartAnalyzerAction<TLanguageKindEnum>>, ImmutableArray<SyntaxNode>, SyntaxNode, Func<SyntaxNode, TLanguageKindEnum>, ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>)>(startActions, actions, endActions, declaredNode, new ExecutionData(analyzer, analyzerSpecificOptions, declaredSymbol, semanticModel, filterSpan, isGeneratedCode), (Action<CodeBlockStartAnalyzerAction<TLanguageKindEnum>, HashSet<CodeBlockAnalyzerAction>, ExecutionData, (AnalyzerExecutor, ImmutableArray<CodeBlockStartAnalyzerAction<TLanguageKindEnum>>, ImmutableArray<SyntaxNode>, SyntaxNode, Func<SyntaxNode, TLanguageKindEnum>, ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>), CancellationToken>)delegate(CodeBlockStartAnalyzerAction<TLanguageKindEnum> startAction, HashSet<CodeBlockAnalyzerAction> set, ExecutionData executionData, (AnalyzerExecutor @this, ImmutableArray<CodeBlockStartAnalyzerAction<TLanguageKindEnum>> startActions, ImmutableArray<SyntaxNode> executableCodeBlocks, SyntaxNode declaredNode, Func<SyntaxNode, TLanguageKindEnum> getKind, ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>> ephemeralActions) args, CancellationToken cancellationToken2)
		{
			(AnalyzerExecutor @this, ImmutableArray<CodeBlockStartAnalyzerAction<TLanguageKindEnum>> startActions, ImmutableArray<SyntaxNode> executableCodeBlocks, SyntaxNode declaredNode, Func<SyntaxNode, TLanguageKindEnum> getKind, ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>> ephemeralActions) tuple = args;
			AnalyzerExecutor item = tuple.@this;
			SyntaxNode item2 = tuple.declaredNode;
			ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>> item3 = tuple.ephemeralActions;
			HostCodeBlockStartAnalysisScope<TLanguageKindEnum> hostCodeBlockStartAnalysisScope = new HostCodeBlockStartAnalysisScope<TLanguageKindEnum>(startAction.Analyzer);
			item.ExecuteAndCatchIfThrows(argument: (startAction, new AnalyzerCodeBlockStartAnalysisContext<TLanguageKindEnum>(hostCodeBlockStartAnalysisScope, item2, executionData.DeclaredSymbol, executionData.SemanticModel, executionData.AnalyzerOptions, executionData.FilterSpan, executionData.IsGeneratedCode, cancellationToken2)), analyzer: startAction.Analyzer, analyze: delegate((CodeBlockStartAnalyzerAction<TLanguageKindEnum> startAction, AnalyzerCodeBlockStartAnalysisContext<TLanguageKindEnum> startContext) tuple2)
			{
				tuple2.startAction.Action(tuple2.startContext);
			}, contextInfo: new AnalysisContextInfo(item.Compilation, executionData.DeclaredSymbol, item2), cancellationToken: cancellationToken2);
			set.AddAll(hostCodeBlockStartAnalysisScope.CodeBlockEndActions);
			item3.AddRange(hostCodeBlockStartAnalysisScope.SyntaxNodeActions);
		}, (Action<AnalyzerDiagnosticReporter, Func<Diagnostic, CancellationToken, bool>, ExecutionData, (AnalyzerExecutor, ImmutableArray<CodeBlockStartAnalyzerAction<TLanguageKindEnum>>, ImmutableArray<SyntaxNode>, SyntaxNode, Func<SyntaxNode, TLanguageKindEnum>, ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>), CancellationToken>)delegate(AnalyzerDiagnosticReporter diagReporter, Func<Diagnostic, CancellationToken, bool> isSupportedDiagnostic, ExecutionData executionData, (AnalyzerExecutor @this, ImmutableArray<CodeBlockStartAnalyzerAction<TLanguageKindEnum>> startActions, ImmutableArray<SyntaxNode> executableCodeBlocks, SyntaxNode declaredNode, Func<SyntaxNode, TLanguageKindEnum> getKind, ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>> ephemeralActions) args, CancellationToken cancellationToken2)
		{
			var (analyzerExecutor, immutableArray, immutableArray2, _, getKind2, arrayBuilder) = args;
			if (arrayBuilder.Any())
			{
				ImmutableSegmentedDictionary<TLanguageKindEnum, ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>> nodeActionsByKind = GetNodeActionsByKind(arrayBuilder);
				ArrayBuilder<SyntaxNode> instance2 = ArrayBuilder<SyntaxNode>.GetInstance();
				foreach (SyntaxNode item4 in immutableArray2)
				{
					Func<SyntaxNode, bool> syntaxNodesToAnalyzeFilter = executionData.SemanticModel.GetSyntaxNodesToAnalyzeFilter(item4, executionData.DeclaredSymbol);
					if (syntaxNodesToAnalyzeFilter != null)
					{
						foreach (SyntaxNode item5 in item4.DescendantNodesAndSelf(syntaxNodesToAnalyzeFilter))
						{
							if (syntaxNodesToAnalyzeFilter(item5))
							{
								instance2.Add(item5);
							}
						}
					}
					else
					{
						instance2.AddRange(item4.DescendantNodesAndSelf());
					}
				}
				analyzerExecutor.ExecuteSyntaxNodeActions(instance2, nodeActionsByKind, executionData, getKind2, diagReporter, isSupportedDiagnostic, immutableArray.Any(), cancellationToken2);
				instance2.Free();
			}
		}, (Action<HashSet<CodeBlockAnalyzerAction>, AnalyzerDiagnosticReporter, Func<Diagnostic, CancellationToken, bool>, ExecutionData, (AnalyzerExecutor, ImmutableArray<CodeBlockStartAnalyzerAction<TLanguageKindEnum>>, ImmutableArray<SyntaxNode>, SyntaxNode, Func<SyntaxNode, TLanguageKindEnum>, ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>), CancellationToken>)delegate(HashSet<CodeBlockAnalyzerAction> blockActions, AnalyzerDiagnosticReporter diagReporter, Func<Diagnostic, CancellationToken, bool> isSupportedDiagnostic, ExecutionData executionData, (AnalyzerExecutor @this, ImmutableArray<CodeBlockStartAnalyzerAction<TLanguageKindEnum>> startActions, ImmutableArray<SyntaxNode> executableCodeBlocks, SyntaxNode declaredNode, Func<SyntaxNode, TLanguageKindEnum> getKind, ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>> ephemeralActions) args, CancellationToken cancellationToken2)
		{
			(AnalyzerExecutor @this, ImmutableArray<CodeBlockStartAnalyzerAction<TLanguageKindEnum>> startActions, ImmutableArray<SyntaxNode> executableCodeBlocks, SyntaxNode declaredNode, Func<SyntaxNode, TLanguageKindEnum> getKind, ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>> ephemeralActions) tuple = args;
			AnalyzerExecutor item = tuple.@this;
			SyntaxNode item2 = tuple.declaredNode;
			CodeBlockAnalysisContext item3 = new CodeBlockAnalysisContext(item2, executionData.DeclaredSymbol, executionData.SemanticModel, executionData.AnalyzerOptions, diagReporter.AddDiagnosticAction, isSupportedDiagnostic, executionData.FilterSpan, executionData.IsGeneratedCode, cancellationToken2);
			foreach (CodeBlockAnalyzerAction blockAction in blockActions)
			{
				item.ExecuteAndCatchIfThrows(blockAction.Analyzer, delegate((CodeBlockAnalyzerAction blockAction, CodeBlockAnalysisContext context) data)
				{
					data.blockAction.Action(data.context);
				}, (blockAction, item3), new AnalysisContextInfo(item.Compilation, executionData.DeclaredSymbol, item2), cancellationToken2);
			}
		}, (this, startActions, executableCodeBlocks, declaredNode, getKind, instance), cancellationToken);
		instance.Free();
	}

	public void ExecuteOperationBlockActions(ImmutableArray<OperationBlockStartAnalyzerAction> startActions, ImmutableArray<OperationBlockAnalyzerAction> actions, ImmutableArray<OperationBlockAnalyzerAction> endActions, DiagnosticAnalyzer analyzer, SyntaxNode declaredNode, ISymbol declaredSymbol, ImmutableArray<IOperation> operationBlocks, ImmutableArray<IOperation> operations, SemanticModel semanticModel, TextSpan? filterSpan, bool isGeneratedCode, CancellationToken cancellationToken)
	{
		ArrayBuilder<OperationAnalyzerAction> instance = ArrayBuilder<OperationAnalyzerAction>.GetInstance();
		AnalyzerOptions analyzerSpecificOptions = GetAnalyzerSpecificOptions(analyzer);
		ExecuteBlockActionsCore<OperationBlockStartAnalyzerAction, OperationBlockAnalyzerAction, (AnalyzerExecutor, ImmutableArray<OperationBlockStartAnalyzerAction>, SyntaxNode, ImmutableArray<IOperation>, ImmutableArray<IOperation>, ArrayBuilder<OperationAnalyzerAction>)>(startActions, actions, endActions, declaredNode, new ExecutionData(analyzer, analyzerSpecificOptions, declaredSymbol, semanticModel, filterSpan, isGeneratedCode), (Action<OperationBlockStartAnalyzerAction, HashSet<OperationBlockAnalyzerAction>, ExecutionData, (AnalyzerExecutor, ImmutableArray<OperationBlockStartAnalyzerAction>, SyntaxNode, ImmutableArray<IOperation>, ImmutableArray<IOperation>, ArrayBuilder<OperationAnalyzerAction>), CancellationToken>)delegate(OperationBlockStartAnalyzerAction startAction, HashSet<OperationBlockAnalyzerAction> set, ExecutionData executionData, (AnalyzerExecutor @this, ImmutableArray<OperationBlockStartAnalyzerAction> startActions, SyntaxNode declaredNode, ImmutableArray<IOperation> operationBlocks, ImmutableArray<IOperation> operations, ArrayBuilder<OperationAnalyzerAction> ephemeralActions) args, CancellationToken cancellationToken2)
		{
			(AnalyzerExecutor @this, ImmutableArray<OperationBlockStartAnalyzerAction> startActions, SyntaxNode declaredNode, ImmutableArray<IOperation> operationBlocks, ImmutableArray<IOperation> operations, ArrayBuilder<OperationAnalyzerAction> ephemeralActions) tuple = args;
			AnalyzerExecutor item = tuple.@this;
			SyntaxNode item2 = tuple.declaredNode;
			ImmutableArray<IOperation> item3 = tuple.operationBlocks;
			ArrayBuilder<OperationAnalyzerAction> item4 = tuple.ephemeralActions;
			HostOperationBlockStartAnalysisScope hostOperationBlockStartAnalysisScope = new HostOperationBlockStartAnalysisScope(startAction.Analyzer);
			item.ExecuteAndCatchIfThrows(argument: (startAction, new AnalyzerOperationBlockStartAnalysisContext(hostOperationBlockStartAnalysisScope, item3, executionData.DeclaredSymbol, executionData.SemanticModel.Compilation, executionData.AnalyzerOptions, item.GetControlFlowGraph, item2.SyntaxTree, executionData.FilterSpan, executionData.IsGeneratedCode, cancellationToken2)), analyzer: startAction.Analyzer, analyze: delegate((OperationBlockStartAnalyzerAction startAction, AnalyzerOperationBlockStartAnalysisContext startContext) tuple2)
			{
				tuple2.startAction.Action(tuple2.startContext);
			}, contextInfo: new AnalysisContextInfo(item.Compilation, executionData.DeclaredSymbol), cancellationToken: cancellationToken2);
			set.AddAll(hostOperationBlockStartAnalysisScope.OperationBlockEndActions);
			item4.AddRange(hostOperationBlockStartAnalysisScope.OperationActions);
		}, (Action<AnalyzerDiagnosticReporter, Func<Diagnostic, CancellationToken, bool>, ExecutionData, (AnalyzerExecutor, ImmutableArray<OperationBlockStartAnalyzerAction>, SyntaxNode, ImmutableArray<IOperation>, ImmutableArray<IOperation>, ArrayBuilder<OperationAnalyzerAction>), CancellationToken>)delegate(AnalyzerDiagnosticReporter diagReporter, Func<Diagnostic, CancellationToken, bool> isSupportedDiagnostic, ExecutionData executionData, (AnalyzerExecutor @this, ImmutableArray<OperationBlockStartAnalyzerAction> startActions, SyntaxNode declaredNode, ImmutableArray<IOperation> operationBlocks, ImmutableArray<IOperation> operations, ArrayBuilder<OperationAnalyzerAction> ephemeralActions) args, CancellationToken cancellationToken2)
		{
			var (analyzerExecutor, immutableArray, _, _, operationsToAnalyze, arrayBuilder) = args;
			if (arrayBuilder.Any())
			{
				analyzerExecutor.ExecuteOperationActions(operationsToAnalyze, GetOperationActionsByKind(arrayBuilder), executionData, diagReporter, isSupportedDiagnostic, immutableArray.Any(), cancellationToken2);
			}
		}, (Action<HashSet<OperationBlockAnalyzerAction>, AnalyzerDiagnosticReporter, Func<Diagnostic, CancellationToken, bool>, ExecutionData, (AnalyzerExecutor, ImmutableArray<OperationBlockStartAnalyzerAction>, SyntaxNode, ImmutableArray<IOperation>, ImmutableArray<IOperation>, ArrayBuilder<OperationAnalyzerAction>), CancellationToken>)delegate(HashSet<OperationBlockAnalyzerAction> blockActions, AnalyzerDiagnosticReporter diagReporter, Func<Diagnostic, CancellationToken, bool> isSupportedDiagnostic, ExecutionData executionData, (AnalyzerExecutor @this, ImmutableArray<OperationBlockStartAnalyzerAction> startActions, SyntaxNode declaredNode, ImmutableArray<IOperation> operationBlocks, ImmutableArray<IOperation> operations, ArrayBuilder<OperationAnalyzerAction> ephemeralActions) args, CancellationToken cancellationToken2)
		{
			(AnalyzerExecutor @this, ImmutableArray<OperationBlockStartAnalyzerAction> startActions, SyntaxNode declaredNode, ImmutableArray<IOperation> operationBlocks, ImmutableArray<IOperation> operations, ArrayBuilder<OperationAnalyzerAction> ephemeralActions) tuple = args;
			AnalyzerExecutor item = tuple.@this;
			SyntaxNode item2 = tuple.declaredNode;
			ImmutableArray<IOperation> item3 = tuple.operationBlocks;
			OperationBlockAnalysisContext item4 = new OperationBlockAnalysisContext(item3, executionData.DeclaredSymbol, item.Compilation, executionData.AnalyzerOptions, diagReporter.AddDiagnosticAction, isSupportedDiagnostic, item.GetControlFlowGraph, item2.SyntaxTree, executionData.FilterSpan, executionData.IsGeneratedCode, cancellationToken2);
			foreach (OperationBlockAnalyzerAction blockAction in blockActions)
			{
				item.ExecuteAndCatchIfThrows(blockAction.Analyzer, delegate((OperationBlockAnalyzerAction blockAction, OperationBlockAnalysisContext context) data)
				{
					data.blockAction.Action(data.context);
				}, (blockAction, item4), new AnalysisContextInfo(item.Compilation, executionData.DeclaredSymbol), cancellationToken2);
			}
		}, (this, startActions, declaredNode, operationBlocks, operations, instance), cancellationToken);
		instance.Free();
	}

	private void ExecuteBlockActionsCore<TBlockStartAction, TBlockAction, TArgs>(ImmutableArray<TBlockStartAction> startActions, ImmutableArray<TBlockAction> actions, ImmutableArray<TBlockAction> endActions, SyntaxNode declaredNode, ExecutionData executionData, Action<TBlockStartAction, HashSet<TBlockAction>, ExecutionData, TArgs, CancellationToken> addActions, Action<AnalyzerDiagnosticReporter, Func<Diagnostic, CancellationToken, bool>, ExecutionData, TArgs, CancellationToken> executeActions, Action<HashSet<TBlockAction>, AnalyzerDiagnosticReporter, Func<Diagnostic, CancellationToken, bool>, ExecutionData, TArgs, CancellationToken> executeBlockActions, TArgs argument, CancellationToken cancellationToken) where TBlockStartAction : AnalyzerAction where TBlockAction : AnalyzerAction where TArgs : struct
	{
		if ((executionData.IsGeneratedCode && _shouldSkipAnalysisOnGeneratedCode(executionData.Analyzer)) || IsAnalyzerSuppressedForTree(executionData.Analyzer, declaredNode.SyntaxTree, cancellationToken))
		{
			return;
		}
		PooledHashSet<TBlockAction> instance = PooledHashSet<TBlockAction>.GetInstance();
		PooledHashSet<TBlockAction> instance2 = PooledHashSet<TBlockAction>.GetInstance();
		instance2.AddAll(actions);
		instance.AddAll(endActions);
		AnalyzerDiagnosticReporter addSemanticDiagnostic = GetAddSemanticDiagnostic(executionData.SemanticModel.SyntaxTree, declaredNode.FullSpan, executionData.Analyzer, executionData.AnalyzerOptions, cancellationToken);
		foreach (TBlockStartAction item in startActions)
		{
			addActions(item, instance, executionData, argument, cancellationToken);
		}
		Func<Diagnostic, CancellationToken, bool> boundFunction;
		using (PooledDelegates.GetPooledFunction((Diagnostic d, CancellationToken ct, (AnalyzerExecutor self, DiagnosticAnalyzer Analyzer) arg) => arg.self.IsSupportedDiagnostic(arg.Analyzer, d, ct), (this, executionData.Analyzer), out boundFunction))
		{
			executeActions(addSemanticDiagnostic, boundFunction, executionData, argument, cancellationToken);
			executeBlockActions(instance2, addSemanticDiagnostic, boundFunction, executionData, argument, cancellationToken);
			executeBlockActions(instance, addSemanticDiagnostic, boundFunction, executionData, argument, cancellationToken);
			addSemanticDiagnostic.Free();
			instance2.Free();
			instance.Free();
		}
	}

	internal static ImmutableSegmentedDictionary<TLanguageKindEnum, ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>> GetNodeActionsByKind<TLanguageKindEnum>(ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>> nodeActions) where TLanguageKindEnum : struct
	{
		if (nodeActions.IsEmpty)
		{
			return ImmutableSegmentedDictionary<TLanguageKindEnum, ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>>.Empty;
		}
		PooledDictionary<TLanguageKindEnum, ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>> instance = PooledDictionary<TLanguageKindEnum, ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>>.GetInstance();
		foreach (SyntaxNodeAnalyzerAction<TLanguageKindEnum> nodeAction in nodeActions)
		{
			foreach (TLanguageKindEnum kind in nodeAction.Kinds)
			{
				instance.AddPooled(kind, nodeAction);
			}
		}
		return instance.ToImmutableSegmentedDictionaryAndFree();
	}

	public void ExecuteSyntaxNodeActions<TLanguageKindEnum>(ArrayBuilder<SyntaxNode> nodesToAnalyze, ImmutableSegmentedDictionary<TLanguageKindEnum, ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>> nodeActionsByKind, DiagnosticAnalyzer analyzer, SemanticModel model, Func<SyntaxNode, TLanguageKindEnum> getKind, TextSpan spanForContainingTopmostNodeForAnalysis, ISymbol declaredSymbol, TextSpan? filterSpan, bool isGeneratedCode, bool hasCodeBlockStartOrSymbolStartActions, CancellationToken cancellationToken) where TLanguageKindEnum : struct
	{
		if ((isGeneratedCode && _shouldSkipAnalysisOnGeneratedCode(analyzer)) || IsAnalyzerSuppressedForTree(analyzer, model.SyntaxTree, cancellationToken))
		{
			return;
		}
		AnalyzerOptions analyzerSpecificOptions = GetAnalyzerSpecificOptions(analyzer);
		AnalyzerDiagnosticReporter addSemanticDiagnostic = GetAddSemanticDiagnostic(model.SyntaxTree, spanForContainingTopmostNodeForAnalysis, analyzer, analyzerSpecificOptions, cancellationToken);
		Func<Diagnostic, CancellationToken, bool> boundFunction;
		using (PooledDelegates.GetPooledFunction((Diagnostic d, CancellationToken ct, (AnalyzerExecutor self, DiagnosticAnalyzer analyzer) arg) => arg.self.IsSupportedDiagnostic(arg.analyzer, d, ct), (this, analyzer), out boundFunction))
		{
			ExecuteSyntaxNodeActions(nodesToAnalyze, nodeActionsByKind, new ExecutionData(analyzer, analyzerSpecificOptions, declaredSymbol, model, filterSpan, isGeneratedCode), getKind, addSemanticDiagnostic, boundFunction, hasCodeBlockStartOrSymbolStartActions, cancellationToken);
			addSemanticDiagnostic.Free();
		}
	}

	private void ExecuteSyntaxNodeActions<TLanguageKindEnum>(ArrayBuilder<SyntaxNode> nodesToAnalyze, ImmutableSegmentedDictionary<TLanguageKindEnum, ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>> nodeActionsByKind, ExecutionData executionData, Func<SyntaxNode, TLanguageKindEnum> getKind, AnalyzerDiagnosticReporter diagReporter, Func<Diagnostic, CancellationToken, bool> isSupportedDiagnostic, bool hasCodeBlockStartOrSymbolStartActions, CancellationToken cancellationToken) where TLanguageKindEnum : struct
	{
		foreach (SyntaxNode item in nodesToAnalyze)
		{
			if (nodeActionsByKind.TryGetValue(getKind(item), out ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>> value) && ShouldExecuteNode(item, executionData.Analyzer, cancellationToken))
			{
				if (!hasCodeBlockStartOrSymbolStartActions)
				{
					diagReporter.FilterSpanForLocalDiagnostics = item.FullSpan;
				}
				foreach (SyntaxNodeAnalyzerAction<TLanguageKindEnum> item2 in value)
				{
					ExecuteSyntaxNodeAction(item2, item, executionData, diagReporter.AddDiagnosticAction, isSupportedDiagnostic, cancellationToken);
				}
			}
		}
	}

	internal static ImmutableSegmentedDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> GetOperationActionsByKind(ArrayBuilder<OperationAnalyzerAction> operationActions)
	{
		if (operationActions.IsEmpty)
		{
			return ImmutableSegmentedDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>>.Empty;
		}
		PooledDictionary<OperationKind, ArrayBuilder<OperationAnalyzerAction>> instance = PooledDictionary<OperationKind, ArrayBuilder<OperationAnalyzerAction>>.GetInstance();
		foreach (OperationAnalyzerAction operationAction in operationActions)
		{
			foreach (OperationKind kind in operationAction.Kinds)
			{
				instance.AddPooled(kind, operationAction);
			}
		}
		return instance.ToImmutableSegmentedDictionaryAndFree();
	}

	public void ExecuteOperationActions(ImmutableArray<IOperation> operationsToAnalyze, ImmutableSegmentedDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> operationActionsByKind, DiagnosticAnalyzer analyzer, SemanticModel model, TextSpan spanForContainingOperationBlock, ISymbol declaredSymbol, TextSpan? filterSpan, bool isGeneratedCode, bool hasOperationBlockStartOrSymbolStartActions, CancellationToken cancellationToken)
	{
		if ((isGeneratedCode && _shouldSkipAnalysisOnGeneratedCode(analyzer)) || IsAnalyzerSuppressedForTree(analyzer, model.SyntaxTree, cancellationToken))
		{
			return;
		}
		AnalyzerOptions analyzerSpecificOptions = GetAnalyzerSpecificOptions(analyzer);
		AnalyzerDiagnosticReporter addSemanticDiagnostic = GetAddSemanticDiagnostic(model.SyntaxTree, spanForContainingOperationBlock, analyzer, analyzerSpecificOptions, cancellationToken);
		Func<Diagnostic, CancellationToken, bool> boundFunction;
		using (PooledDelegates.GetPooledFunction((Diagnostic d, CancellationToken ct, (AnalyzerExecutor self, DiagnosticAnalyzer analyzer) arg) => arg.self.IsSupportedDiagnostic(arg.analyzer, d, ct), (this, analyzer), out boundFunction))
		{
			ExecuteOperationActions(operationsToAnalyze, operationActionsByKind, new ExecutionData(analyzer, analyzerSpecificOptions, declaredSymbol, model, filterSpan, isGeneratedCode), addSemanticDiagnostic, boundFunction, hasOperationBlockStartOrSymbolStartActions, cancellationToken);
			addSemanticDiagnostic.Free();
		}
	}

	private void ExecuteOperationActions(ImmutableArray<IOperation> operationsToAnalyze, ImmutableSegmentedDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> operationActionsByKind, ExecutionData executionData, AnalyzerDiagnosticReporter diagReporter, Func<Diagnostic, CancellationToken, bool> isSupportedDiagnostic, bool hasOperationBlockStartOrSymbolStartActions, CancellationToken cancellationToken)
	{
		foreach (IOperation item in operationsToAnalyze)
		{
			if (operationActionsByKind.TryGetValue(item.Kind, out ImmutableArray<OperationAnalyzerAction> value) && ShouldExecuteOperation(item, executionData.Analyzer, cancellationToken))
			{
				if (!hasOperationBlockStartOrSymbolStartActions)
				{
					diagReporter.FilterSpanForLocalDiagnostics = item.Syntax.FullSpan;
				}
				foreach (OperationAnalyzerAction item2 in value)
				{
					ExecuteOperationAction(item2, item, executionData, diagReporter.AddDiagnosticAction, isSupportedDiagnostic, cancellationToken);
				}
			}
		}
	}

	internal static bool CanHaveExecutableCodeBlock(ISymbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.Event:
		case SymbolKind.Method:
		case SymbolKind.NamedType:
		case SymbolKind.Namespace:
		case SymbolKind.Property:
			return true;
		case SymbolKind.Field:
			return true;
		default:
			return false;
		}
	}

	internal void ExecuteAndCatchIfThrows<TArg>(DiagnosticAnalyzer analyzer, Action<TArg> analyze, TArg argument, AnalysisContextInfo? contextInfo, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		SharedStopwatch sharedStopwatch = default(SharedStopwatch);
		if (_analyzerExecutionTimeMap != null)
		{
			sharedStopwatch = SharedStopwatch.StartNew();
		}
		object obj = _getAnalyzerGate(analyzer);
		if (obj != null)
		{
			lock (obj)
			{
				ExecuteAndCatchIfThrows_NoLock(analyzer, analyze, argument, contextInfo, cancellationToken);
			}
		}
		else
		{
			ExecuteAndCatchIfThrows_NoLock(analyzer, analyze, argument, contextInfo, cancellationToken);
		}
		if (_analyzerExecutionTimeMap != null)
		{
			long ticks = sharedStopwatch.Elapsed.Ticks;
			Interlocked.Add(ref _analyzerExecutionTimeMap.GetOrAdd(analyzer, (DiagnosticAnalyzer _) => new StrongBox<long>(0L)).Value, ticks);
		}
	}

	private void ExecuteAndCatchIfThrows_NoLock<TArg>(DiagnosticAnalyzer analyzer, Action<TArg> analyze, TArg argument, AnalysisContextInfo? info, CancellationToken cancellationToken)
	{
		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			analyze(argument);
		}
		catch (Exception ex) when (HandleAnalyzerException(analyzer, ex, in info) && HandleAnalyzerException(ex, analyzer, info, OnAnalyzerException, _analyzerExceptionFilter, cancellationToken))
		{
		}
	}

	private bool HandleAnalyzerException(DiagnosticAnalyzer analyzer, Exception ex, in AnalysisContextInfo? info)
	{
		if (!Compilation.CatchAnalyzerExceptions)
		{
			Environment.FailFast(CreateAnalyzerExceptionDiagnostic(analyzer, ex, info).ToString());
			return false;
		}
		return true;
	}

	internal static bool HandleAnalyzerException(Exception exception, DiagnosticAnalyzer analyzer, AnalysisContextInfo? info, Action<Exception, DiagnosticAnalyzer, Diagnostic, CancellationToken> onAnalyzerException, Func<Exception, bool>? analyzerExceptionFilter, CancellationToken cancellationToken)
	{
		if (!exceptionFilter(exception, analyzerExceptionFilter, cancellationToken))
		{
			return false;
		}
		Diagnostic arg = CreateAnalyzerExceptionDiagnostic(analyzer, exception, info);
		try
		{
			onAnalyzerException(exception, analyzer, arg, cancellationToken);
		}
		catch (Exception)
		{
		}
		return true;
		static bool exceptionFilter(Exception ex2, Func<Exception, bool>? func, CancellationToken cancellationToken2)
		{
			OperationCanceledException obj = ex2 as OperationCanceledException;
			if (obj != null && obj.CancellationToken == cancellationToken2)
			{
				return false;
			}
			return func?.Invoke(ex2) ?? true;
		}
	}

	internal static Diagnostic CreateAnalyzerExceptionDiagnostic(DiagnosticAnalyzer analyzer, Exception e, AnalysisContextInfo? info = null)
	{
		string text = analyzer.ToString();
		string compilerAnalyzerFailure = CodeAnalysisResources.CompilerAnalyzerFailure;
		string compilerAnalyzerThrows = CodeAnalysisResources.CompilerAnalyzerThrows;
		string text2 = string.Join(Environment.NewLine, CreateDiagnosticDescription(info, e), CreateDisablingMessage(analyzer, text)).Trim();
		string[] array = new string[4]
		{
			text,
			e.GetType().ToString(),
			e.Message,
			text2
		};
		DiagnosticDescriptor analyzerExceptionDiagnosticDescriptor = GetAnalyzerExceptionDiagnosticDescriptor("AD0001", compilerAnalyzerFailure, compilerAnalyzerThrows);
		Location none = Location.None;
		object[] messageArgs = array;
		return Diagnostic.Create(analyzerExceptionDiagnosticDescriptor, none, messageArgs);
	}

	private static string CreateDiagnosticDescription(AnalysisContextInfo? info, Exception e)
	{
		if (!info.HasValue)
		{
			return e.CreateDiagnosticDescription();
		}
		return string.Join(Environment.NewLine, string.Format(CodeAnalysisResources.ExceptionContext, info?.GetContext()), e.CreateDiagnosticDescription());
	}

	private static string CreateDisablingMessage(DiagnosticAnalyzer analyzer, string analyzerName)
	{
		ImmutableSortedSet<string> immutableSortedSet = ImmutableSortedSet<string>.Empty.WithComparer(StringComparer.OrdinalIgnoreCase);
		try
		{
			foreach (DiagnosticDescriptor supportedDiagnostic in analyzer.SupportedDiagnostics)
			{
				if (supportedDiagnostic != null)
				{
					immutableSortedSet = immutableSortedSet.Add(supportedDiagnostic.Id);
				}
			}
		}
		catch (Exception ex)
		{
			return string.Format(CodeAnalysisResources.CompilerAnalyzerThrows, analyzerName, ex.GetType().ToString(), ex.Message, ex.CreateDiagnosticDescription());
		}
		if (immutableSortedSet.IsEmpty)
		{
			return "";
		}
		return string.Format(CodeAnalysisResources.DisableAnalyzerDiagnosticsMessage, string.Join(", ", immutableSortedSet));
	}

	internal static Diagnostic CreateDriverExceptionDiagnostic(Exception e)
	{
		string analyzerDriverFailure = CodeAnalysisResources.AnalyzerDriverFailure;
		string analyzerDriverThrows = CodeAnalysisResources.AnalyzerDriverThrows;
		string[] array = new string[3]
		{
			e.GetType().ToString(),
			e.Message,
			e.CreateDiagnosticDescription()
		};
		DiagnosticDescriptor analyzerExceptionDiagnosticDescriptor = GetAnalyzerExceptionDiagnosticDescriptor("AD0002", analyzerDriverFailure, analyzerDriverThrows);
		Location none = Location.None;
		object[] messageArgs = array;
		return Diagnostic.Create(analyzerExceptionDiagnosticDescriptor, none, messageArgs);
	}

	internal static DiagnosticDescriptor GetAnalyzerExceptionDiagnosticDescriptor(string? id = null, string? title = null, string? messageFormat = null)
	{
		if (id == null)
		{
			id = "AD0001";
		}
		if (title == null)
		{
			title = CodeAnalysisResources.CompilerAnalyzerFailure;
		}
		if (messageFormat == null)
		{
			messageFormat = CodeAnalysisResources.CompilerAnalyzerThrows;
		}
		return new DiagnosticDescriptor(id, title, messageFormat, "Compiler", DiagnosticSeverity.Warning, true, null, null, "AnalyzerException");
	}

	internal static bool IsAnalyzerExceptionDiagnostic(Diagnostic diagnostic)
	{
		if (diagnostic.Id == "AD0001" || diagnostic.Id == "AD0002")
		{
			foreach (string immutableCustomTag in diagnostic.Descriptor.ImmutableCustomTags)
			{
				if (immutableCustomTag == "AnalyzerException")
				{
					return true;
				}
			}
		}
		return false;
	}

	internal static bool AreEquivalentAnalyzerExceptionDiagnostics(Diagnostic exceptionDiagnostic, Diagnostic other)
	{
		if (!IsAnalyzerExceptionDiagnostic(other))
		{
			return false;
		}
		if (exceptionDiagnostic.Id == other.Id && exceptionDiagnostic.Severity == other.Severity)
		{
			return exceptionDiagnostic.GetMessage() == other.GetMessage();
		}
		return false;
	}

	private bool IsSupportedDiagnostic(DiagnosticAnalyzer analyzer, Diagnostic diagnostic, CancellationToken cancellationToken)
	{
		if (diagnostic is DiagnosticWithInfo)
		{
			return true;
		}
		return _analyzerManager.IsSupportedDiagnostic(analyzer, diagnostic, _isCompilerAnalyzer, this, cancellationToken);
	}

	private void AddSymbolDiagnostic(SymbolDeclaredCompilationEvent symbolDeclaredEvent, Diagnostic diagnostic, DiagnosticAnalyzer analyzer, AnalyzerOptions options, Func<ISymbol, SyntaxReference, Compilation, CancellationToken, SyntaxNode> getTopMostNodeForAnalysis, CancellationToken cancellationToken)
	{
		if (_shouldSuppressGeneratedCodeDiagnostic(diagnostic, analyzer, Compilation, cancellationToken))
		{
			return;
		}
		if (_addCategorizedLocalDiagnostic == null)
		{
			_addNonCategorizedDiagnostic(diagnostic, options, cancellationToken);
			return;
		}
		if (diagnostic.Location.IsInSource)
		{
			ISymbol symbol = symbolDeclaredEvent.Symbol;
			foreach (SyntaxReference declaringSyntaxReference in symbolDeclaredEvent.DeclaringSyntaxReferences)
			{
				if (declaringSyntaxReference.SyntaxTree == diagnostic.Location.SourceTree)
				{
					SyntaxNode syntaxNode = getTopMostNodeForAnalysis(symbol, declaringSyntaxReference, Compilation, cancellationToken);
					if (diagnostic.Location.SourceSpan.IntersectsWith(syntaxNode.FullSpan))
					{
						_addCategorizedLocalDiagnostic(diagnostic, analyzer, options, arg4: false, cancellationToken);
						return;
					}
				}
			}
		}
		_addCategorizedNonLocalDiagnostic(diagnostic, analyzer, options, cancellationToken);
	}

	private Action<Diagnostic> GetAddCompilationDiagnostic(DiagnosticAnalyzer analyzer, AnalyzerOptions analyzerOptions, CancellationToken cancellationToken)
	{
		return delegate(Diagnostic diagnostic)
		{
			if (!_shouldSuppressGeneratedCodeDiagnostic(diagnostic, analyzer, Compilation, cancellationToken))
			{
				if (_addCategorizedNonLocalDiagnostic == null)
				{
					_addNonCategorizedDiagnostic(diagnostic, analyzerOptions, cancellationToken);
				}
				else
				{
					_addCategorizedNonLocalDiagnostic(diagnostic, analyzer, analyzerOptions, cancellationToken);
				}
			}
		};
	}

	private AnalyzerDiagnosticReporter GetAddSemanticDiagnostic(SyntaxTree tree, DiagnosticAnalyzer analyzer, AnalyzerOptions analyzerOptions, CancellationToken cancellationToken)
	{
		return AnalyzerDiagnosticReporter.GetInstance(new SourceOrAdditionalFile(tree), null, Compilation, analyzer, analyzerOptions, isSyntaxDiagnostic: false, _addNonCategorizedDiagnostic, _addCategorizedLocalDiagnostic, _addCategorizedNonLocalDiagnostic, _shouldSuppressGeneratedCodeDiagnostic, cancellationToken);
	}

	private AnalyzerDiagnosticReporter GetAddSemanticDiagnostic(SyntaxTree tree, TextSpan? span, DiagnosticAnalyzer analyzer, AnalyzerOptions analyzerOptions, CancellationToken cancellationToken)
	{
		return AnalyzerDiagnosticReporter.GetInstance(new SourceOrAdditionalFile(tree), span, Compilation, analyzer, analyzerOptions, isSyntaxDiagnostic: false, _addNonCategorizedDiagnostic, _addCategorizedLocalDiagnostic, _addCategorizedNonLocalDiagnostic, _shouldSuppressGeneratedCodeDiagnostic, cancellationToken);
	}

	private AnalyzerDiagnosticReporter GetAddSyntaxDiagnostic(SourceOrAdditionalFile file, DiagnosticAnalyzer analyzer, AnalyzerOptions analyzerOptions, CancellationToken cancellationToken)
	{
		return AnalyzerDiagnosticReporter.GetInstance(file, null, Compilation, analyzer, analyzerOptions, isSyntaxDiagnostic: true, _addNonCategorizedDiagnostic, _addCategorizedLocalDiagnostic, _addCategorizedNonLocalDiagnostic, _shouldSuppressGeneratedCodeDiagnostic, cancellationToken);
	}

	private bool ShouldExecuteNode(SyntaxNode node, DiagnosticAnalyzer analyzer, CancellationToken cancellationToken)
	{
		if (_shouldSkipAnalysisOnGeneratedCode(analyzer) && _isGeneratedCodeLocation(node.SyntaxTree, node.Span, cancellationToken))
		{
			return false;
		}
		return true;
	}

	private bool ShouldExecuteOperation(IOperation operation, DiagnosticAnalyzer analyzer, CancellationToken cancellationToken)
	{
		if (operation.Syntax != null && _shouldSkipAnalysisOnGeneratedCode(analyzer) && _isGeneratedCodeLocation(operation.Syntax.SyntaxTree, operation.Syntax.Span, cancellationToken))
		{
			return false;
		}
		return true;
	}

	internal TimeSpan ResetAnalyzerExecutionTime(DiagnosticAnalyzer analyzer)
	{
		if (!_analyzerExecutionTimeMap.TryRemove(analyzer, out StrongBox<long> value))
		{
			return TimeSpan.Zero;
		}
		return TimeSpan.FromTicks(value.Value);
	}

	private ControlFlowGraph GetControlFlowGraphImpl(IOperation operation)
	{
		if (_lazyControlFlowGraphMap == null)
		{
			Interlocked.CompareExchange(ref _lazyControlFlowGraphMap, new ConcurrentDictionary<IOperation, ControlFlowGraph>(), null);
		}
		return _lazyControlFlowGraphMap.GetOrAdd(operation, (IOperation op) => ControlFlowGraphBuilder.Create(op, null, null, null, default(ControlFlowGraphBuilder.Context)));
	}

	private bool IsAnalyzerSuppressedForSymbol(DiagnosticAnalyzer analyzer, ISymbol symbol, CancellationToken cancellationToken)
	{
		foreach (Location location in symbol.Locations)
		{
			if (location.SourceTree != null && !IsAnalyzerSuppressedForTree(analyzer, location.SourceTree, cancellationToken))
			{
				return false;
			}
		}
		return true;
	}

	public void OnOperationBlockActionsExecuted(ImmutableArray<IOperation> operationBlocks)
	{
		ConcurrentDictionary<IOperation, ControlFlowGraph>? lazyControlFlowGraphMap = _lazyControlFlowGraphMap;
		if (lazyControlFlowGraphMap != null && lazyControlFlowGraphMap.Count > 0)
		{
			foreach (IOperation item in operationBlocks)
			{
				IOperation rootOperation = item.GetRootOperation();
				_lazyControlFlowGraphMap.TryRemove(rootOperation, out ControlFlowGraph _);
			}
		}
	}
}
