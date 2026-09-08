using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Diagnostics.Telemetry;
using Microsoft.CodeAnalysis.ErrorReporting;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal abstract class AnalyzerDriver : IDisposable
{
	internal sealed class CompilationData
	{
		public CachingSemanticModelProvider SemanticModelProvider { get; }

		public SuppressMessageAttributeState SuppressMessageAttributeState { get; }

		public CompilationData(Compilation compilation)
		{
			SemanticModelProvider = (CachingSemanticModelProvider)compilation.SemanticModelProvider;
			SuppressMessageAttributeState = new SuppressMessageAttributeState(compilation);
		}
	}

	internal readonly struct DeclarationAnalysisData(SyntaxNode declaringReferenceSyntax, SyntaxNode topmostNodeForAnalysis, ImmutableArray<DeclarationInfo> declarationsInNodeBuilder, bool isPartialAnalysis)
	{
		public readonly SyntaxNode DeclaringReferenceSyntax = declaringReferenceSyntax;

		public readonly SyntaxNode TopmostNodeForAnalysis = topmostNodeForAnalysis;

		public readonly ImmutableArray<DeclarationInfo> DeclarationsInNode = declarationsInNodeBuilder;

		public readonly ArrayBuilder<SyntaxNode> DescendantNodesToAnalyze = ArrayBuilder<SyntaxNode>.GetInstance();

		public readonly bool IsPartialAnalysis = isPartialAnalysis;

		public void Free()
		{
			DescendantNodesToAnalyze.Free();
		}
	}

	private sealed class EventProcessedState
	{
		public static readonly EventProcessedState Processed = new EventProcessedState(EventProcessedStateKind.Processed);

		public static readonly EventProcessedState NotProcessed = new EventProcessedState(EventProcessedStateKind.NotProcessed);

		public EventProcessedStateKind Kind { get; }

		public ImmutableArray<DiagnosticAnalyzer> SubsetProcessedAnalyzers { get; }

		private EventProcessedState(EventProcessedStateKind kind)
		{
			Kind = kind;
			SubsetProcessedAnalyzers = default(ImmutableArray<DiagnosticAnalyzer>);
		}

		private EventProcessedState(ImmutableArray<DiagnosticAnalyzer> subsetProcessedAnalyzers)
		{
			SubsetProcessedAnalyzers = subsetProcessedAnalyzers;
			Kind = EventProcessedStateKind.PartiallyProcessed;
		}

		public static EventProcessedState CreatePartiallyProcessed(ImmutableArray<DiagnosticAnalyzer> subsetProcessedAnalyzers)
		{
			return new EventProcessedState(subsetProcessedAnalyzers);
		}
	}

	private enum EventProcessedStateKind
	{
		Processed,
		NotProcessed,
		PartiallyProcessed
	}

	protected interface IGroupedAnalyzerActions
	{
		bool IsEmpty { get; }

		AnalyzerActions AnalyzerActions { get; }

		IGroupedAnalyzerActions Append(IGroupedAnalyzerActions groupedAnalyzerActions);
	}

	private const int MaxSymbolKind = 100;

	private static readonly Func<DiagnosticAnalyzer, bool> s_IsCompilerAnalyzerFunc = IsCompilerAnalyzer;

	private static readonly Func<ISymbol, SyntaxReference, Compilation, CancellationToken, SyntaxNode> s_getTopmostNodeForAnalysis = GetTopmostNodeForAnalysis;

	private static readonly ObjectPool<ArrayBuilder<DiagnosticAnalyzer>> s_diagnosticAnalyzerPool = new ObjectPool<ArrayBuilder<DiagnosticAnalyzer>>(() => new ArrayBuilder<DiagnosticAnalyzer>());

	private readonly Func<SyntaxTree, CancellationToken, bool> _isGeneratedCode;

	private readonly ConcurrentSet<Suppression>? _programmaticSuppressions;

	private readonly ConcurrentSet<Diagnostic>? _diagnosticsProcessedForProgrammaticSuppressions;

	internal readonly bool HasDiagnosticSuppressors;

	private readonly SeverityFilter _severityFilter;

	private CancellationTokenRegistration? _lazyQueueRegistration;

	private AnalyzerExecutor? _lazyAnalyzerExecutor;

	private CompilationData? _lazyCurrentCompilationData;

	private ImmutableHashSet<DiagnosticAnalyzer>? _lazyUnsuppressedAnalyzers;

	private ConcurrentDictionary<(INamespaceOrTypeSymbol, DiagnosticAnalyzer), IGroupedAnalyzerActions>? _lazyPerSymbolAnalyzerActionsCache;

	private ImmutableArray<(DiagnosticAnalyzer, ImmutableArray<ImmutableArray<SymbolAnalyzerAction>>)> _lazySymbolActionsByKind;

	private ImmutableArray<(DiagnosticAnalyzer, ImmutableArray<SemanticModelAnalyzerAction>)> _lazySemanticModelActions;

	private ImmutableArray<(DiagnosticAnalyzer, ImmutableArray<SyntaxTreeAnalyzerAction>)> _lazySyntaxTreeActions;

	private ImmutableArray<(DiagnosticAnalyzer, ImmutableArray<AdditionalFileAnalyzerAction>)> _lazyAdditionalFileActions;

	private ImmutableArray<(DiagnosticAnalyzer, ImmutableArray<CompilationAnalyzerAction>)> _lazyCompilationActions;

	private ImmutableArray<(DiagnosticAnalyzer, ImmutableArray<CompilationAnalyzerAction>)> _lazyCompilationEndActions;

	private ImmutableHashSet<DiagnosticAnalyzer>? _lazyCompilationEndAnalyzers;

	internal const GeneratedCodeAnalysisFlags DefaultGeneratedCodeAnalysisFlags = GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics;

	private ImmutableSegmentedDictionary<DiagnosticAnalyzer, SemaphoreSlim> _lazyAnalyzerGateMap;

	private ImmutableSegmentedDictionary<DiagnosticAnalyzer, GeneratedCodeAnalysisFlags> _lazyGeneratedCodeAnalysisFlagsMap;

	private AnalyzerActions _lazyAnalyzerActions;

	private ImmutableHashSet<DiagnosticAnalyzer>? _lazyNonConfigurableAndCustomConfigurableAnalyzers;

	private ImmutableHashSet<DiagnosticAnalyzer>? _lazySymbolStartAnalyzers;

	private bool? _lazyTreatAllCodeAsNonGeneratedCode;

	private bool? _lazyDoNotAnalyzeGeneratedCode;

	private ConcurrentDictionary<SyntaxTree, bool>? _lazyGeneratedCodeFilesMap;

	private Dictionary<SyntaxTree, ImmutableHashSet<ISymbol>>? _lazyGeneratedCodeSymbolsForTreeMap;

	private ConcurrentDictionary<SyntaxTree, ImmutableHashSet<DiagnosticAnalyzer>>? _lazySuppressedAnalyzersForTreeMap;

	private ConcurrentSet<string>? _lazySuppressedDiagnosticIdsForUnsuppressedAnalyzers;

	private ConcurrentDictionary<ISymbol, bool>? _lazyIsGeneratedCodeSymbolMap;

	private ConcurrentDictionary<SyntaxTree, bool>? _lazyTreesWithHiddenRegionsMap;

	private INamedTypeSymbol? _lazyGeneratedCodeAttribute;

	private Task? _lazyInitializeTask;

	private bool _initializeSucceeded;

	private Task? _lazyPrimaryTask;

	private readonly int _workerCount = Environment.ProcessorCount;

	private AsyncQueue<CompilationEvent>? _lazyCompilationEventQueue;

	private DiagnosticQueue? _lazyDiagnosticQueue;

	protected ImmutableArray<DiagnosticAnalyzer> Analyzers { get; }

	protected AnalyzerManager AnalyzerManager { get; }

	protected AnalyzerExecutor AnalyzerExecutor => _lazyAnalyzerExecutor;

	protected CompilationData CurrentCompilationData => _lazyCurrentCompilationData;

	protected CachingSemanticModelProvider SemanticModelProvider => CurrentCompilationData.SemanticModelProvider;

	protected ref readonly AnalyzerActions AnalyzerActions => ref _lazyAnalyzerActions;

	protected ImmutableHashSet<DiagnosticAnalyzer> UnsuppressedAnalyzers => _lazyUnsuppressedAnalyzers;

	private ConcurrentDictionary<(INamespaceOrTypeSymbol, DiagnosticAnalyzer), IGroupedAnalyzerActions> PerSymbolAnalyzerActionsCache => _lazyPerSymbolAnalyzerActionsCache;

	private ImmutableHashSet<DiagnosticAnalyzer> CompilationEndAnalyzers => _lazyCompilationEndAnalyzers;

	private ImmutableSegmentedDictionary<DiagnosticAnalyzer, SemaphoreSlim> AnalyzerGateMap => _lazyAnalyzerGateMap;

	private ImmutableSegmentedDictionary<DiagnosticAnalyzer, GeneratedCodeAnalysisFlags> GeneratedCodeAnalysisFlagsMap => _lazyGeneratedCodeAnalysisFlagsMap;

	private ImmutableHashSet<DiagnosticAnalyzer> NonConfigurableAndCustomConfigurableAnalyzers => _lazyNonConfigurableAndCustomConfigurableAnalyzers;

	private ImmutableHashSet<DiagnosticAnalyzer> SymbolStartAnalyzers => _lazySymbolStartAnalyzers;

	private bool TreatAllCodeAsNonGeneratedCode => _lazyTreatAllCodeAsNonGeneratedCode.Value;

	private ConcurrentDictionary<SyntaxTree, bool> GeneratedCodeFilesMap => _lazyGeneratedCodeFilesMap;

	private Dictionary<SyntaxTree, ImmutableHashSet<ISymbol>> GeneratedCodeSymbolsForTreeMap => _lazyGeneratedCodeSymbolsForTreeMap;

	private ConcurrentDictionary<SyntaxTree, ImmutableHashSet<DiagnosticAnalyzer>> SuppressedAnalyzersForTreeMap => _lazySuppressedAnalyzersForTreeMap;

	private ConcurrentSet<string> SuppressedDiagnosticIdsForUnsuppressedAnalyzers => _lazySuppressedDiagnosticIdsForUnsuppressedAnalyzers;

	private ConcurrentDictionary<ISymbol, bool> IsGeneratedCodeSymbolMap => _lazyIsGeneratedCodeSymbolMap;

	public AsyncQueue<CompilationEvent> CompilationEventQueue => _lazyCompilationEventQueue;

	public DiagnosticQueue DiagnosticQueue => _lazyDiagnosticQueue;

	public bool IsInitialized => _lazyInitializeTask != null;

	public Task WhenInitializedTask => _lazyInitializeTask;

	public Task WhenCompletedTask => _lazyPrimaryTask;

	internal ImmutableDictionary<DiagnosticAnalyzer, TimeSpan> AnalyzerExecutionTimes => AnalyzerExecutor.AnalyzerExecutionTimes;

	protected bool DoNotAnalyzeGeneratedCode => _lazyDoNotAnalyzeGeneratedCode.Value;

	protected abstract IGroupedAnalyzerActions EmptyGroupedActions { get; }

	protected AnalyzerDriver(ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerManager analyzerManager, SeverityFilter severityFilter, Func<SyntaxTrivia, bool> isComment)
	{
		Analyzers = analyzers;
		AnalyzerManager = analyzerManager;
		_isGeneratedCode = (SyntaxTree tree, CancellationToken ct) => GeneratedCodeUtilities.IsGeneratedCode(tree, isComment, ct);
		_severityFilter = severityFilter;
		HasDiagnosticSuppressors = Analyzers.Any((DiagnosticAnalyzer a) => a is DiagnosticSuppressor);
		_programmaticSuppressions = (HasDiagnosticSuppressors ? new ConcurrentSet<Suppression>() : null);
		_diagnosticsProcessedForProgrammaticSuppressions = (HasDiagnosticSuppressors ? new ConcurrentSet<Diagnostic>(ReferenceEqualityComparer.Instance) : null);
		_lazyAnalyzerGateMap = ImmutableSegmentedDictionary<DiagnosticAnalyzer, SemaphoreSlim>.Empty;
	}

	private void Initialize(AnalyzerExecutor analyzerExecutor, DiagnosticQueue diagnosticQueue, CompilationData compilationData, AnalysisScope analysisScope, ConcurrentSet<string>? suppressedDiagnosticIds, CancellationToken cancellationToken)
	{
		try
		{
			_lazyAnalyzerExecutor = analyzerExecutor;
			_lazyCurrentCompilationData = compilationData;
			_lazyDiagnosticQueue = diagnosticQueue;
			_lazySuppressedDiagnosticIdsForUnsuppressedAnalyzers = suppressedDiagnosticIds;
			_lazyInitializeTask = Task.Run(async delegate
			{
				(AnalyzerActions, ImmutableHashSet<DiagnosticAnalyzer>) tuple = await GetAnalyzerActionsAsync(Analyzers, AnalyzerManager, analyzerExecutor, analysisScope, _severityFilter, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_lazyAnalyzerActions = tuple.Item1;
				_lazyUnsuppressedAnalyzers = tuple.Item2;
				ImmutableSegmentedDictionary<DiagnosticAnalyzer, SemaphoreSlim> lazyAnalyzerGateMap = await CreateAnalyzerGateMapAsync(UnsuppressedAnalyzers, AnalyzerManager, analyzerExecutor, analysisScope, _severityFilter, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_lazyAnalyzerGateMap = lazyAnalyzerGateMap;
				_lazyNonConfigurableAndCustomConfigurableAnalyzers = ComputeNonConfigurableAndCustomConfigurableAnalyzers(UnsuppressedAnalyzers, cancellationToken);
				_lazySymbolStartAnalyzers = ComputeSymbolStartAnalyzers(UnsuppressedAnalyzers);
				ImmutableSegmentedDictionary<DiagnosticAnalyzer, GeneratedCodeAnalysisFlags> lazyGeneratedCodeAnalysisFlagsMap = await CreateGeneratedCodeAnalysisFlagsMapAsync(UnsuppressedAnalyzers, AnalyzerManager, analyzerExecutor, analysisScope, _severityFilter, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_lazyGeneratedCodeAnalysisFlagsMap = lazyGeneratedCodeAnalysisFlagsMap;
				_lazyTreatAllCodeAsNonGeneratedCode = ComputeShouldTreatAllCodeAsNonGeneratedCode(UnsuppressedAnalyzers, GeneratedCodeAnalysisFlagsMap);
				_lazyDoNotAnalyzeGeneratedCode = ComputeShouldSkipAnalysisOnGeneratedCode(UnsuppressedAnalyzers, GeneratedCodeAnalysisFlagsMap, TreatAllCodeAsNonGeneratedCode);
				_lazyGeneratedCodeFilesMap = new ConcurrentDictionary<SyntaxTree, bool>();
				_lazyGeneratedCodeSymbolsForTreeMap = new Dictionary<SyntaxTree, ImmutableHashSet<ISymbol>>();
				_lazyIsGeneratedCodeSymbolMap = new ConcurrentDictionary<ISymbol, bool>();
				_lazyTreesWithHiddenRegionsMap = new ConcurrentDictionary<SyntaxTree, bool>();
				_lazySuppressedAnalyzersForTreeMap = new ConcurrentDictionary<SyntaxTree, ImmutableHashSet<DiagnosticAnalyzer>>();
				_lazyGeneratedCodeAttribute = analyzerExecutor.Compilation?.GetTypeByMetadataName("System.CodeDom.Compiler.GeneratedCodeAttribute");
				_lazySymbolActionsByKind = MakeSymbolActionsByKind(in AnalyzerActions);
				_lazySemanticModelActions = MakeActionsByAnalyzer<SemanticModelAnalyzerAction>(AnalyzerActions.SemanticModelActions);
				_lazySyntaxTreeActions = MakeActionsByAnalyzer<SyntaxTreeAnalyzerAction>(AnalyzerActions.SyntaxTreeActions);
				_lazyAdditionalFileActions = MakeActionsByAnalyzer<AdditionalFileAnalyzerAction>(AnalyzerActions.AdditionalFileActions);
				_lazyCompilationActions = MakeActionsByAnalyzer<CompilationAnalyzerAction>(AnalyzerActions.CompilationActions);
				_lazyCompilationEndActions = MakeActionsByAnalyzer<CompilationAnalyzerAction>(AnalyzerActions.CompilationEndActions);
				_lazyCompilationEndAnalyzers = MakeCompilationEndAnalyzers(_lazyCompilationEndActions);
				if (AnalyzerActions.SymbolStartActionsCount > 0)
				{
					_lazyPerSymbolAnalyzerActionsCache = new ConcurrentDictionary<(INamespaceOrTypeSymbol, DiagnosticAnalyzer), IGroupedAnalyzerActions>();
				}
			}, cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			_initializeSucceeded = true;
		}
		finally
		{
			if (_lazyInitializeTask == null)
			{
				_lazyInitializeTask = Task.FromCanceled(new CancellationToken(canceled: true));
				_lazyPrimaryTask = Task.FromCanceled(new CancellationToken(canceled: true));
				DiagnosticQueue.TryComplete();
			}
		}
	}

	internal void Initialize(Compilation compilation, CompilationWithAnalyzersOptions analysisOptions, CompilationData compilationData, AnalysisScope analysisScope, bool categorizeDiagnostics, bool trackSuppressedDiagnosticIds, CancellationToken cancellationToken)
	{
		DiagnosticQueue diagnosticQueue = Microsoft.CodeAnalysis.Diagnostics.DiagnosticQueue.Create(categorizeDiagnostics);
		ConcurrentSet<string> suppressedDiagnosticIds = (trackSuppressedDiagnosticIds ? new ConcurrentSet<string>() : null);
		Action<Diagnostic, AnalyzerOptions, CancellationToken> addNotCategorizedDiagnostic = null;
		Action<Diagnostic, DiagnosticAnalyzer, AnalyzerOptions, bool, CancellationToken> addCategorizedLocalDiagnostic = null;
		Action<Diagnostic, DiagnosticAnalyzer, AnalyzerOptions, CancellationToken> addCategorizedNonLocalDiagnostic = null;
		if (categorizeDiagnostics)
		{
			addCategorizedLocalDiagnostic = GetDiagnosticSink(diagnosticQueue.EnqueueLocal, compilation, _severityFilter, suppressedDiagnosticIds);
			addCategorizedNonLocalDiagnostic = GetDiagnosticSink(diagnosticQueue.EnqueueNonLocal, compilation, _severityFilter, suppressedDiagnosticIds);
		}
		else
		{
			addNotCategorizedDiagnostic = GetDiagnosticSink(diagnosticQueue.Enqueue, compilation, _severityFilter, suppressedDiagnosticIds);
		}
		AnalyzerOptions options = analysisOptions.Options ?? AnalyzerOptions.Empty;
		Action<Exception, DiagnosticAnalyzer, Diagnostic, CancellationToken> onAnalyzerException = delegate(Exception ex, DiagnosticAnalyzer analyzer, Diagnostic diagnostic, CancellationToken cancellationToken2)
		{
			Diagnostic filteredDiagnostic = GetFilteredDiagnostic(diagnostic, compilation, options, _severityFilter, suppressedDiagnosticIds, cancellationToken2);
			if (filteredDiagnostic != null)
			{
				if (analysisOptions.OnAnalyzerException != null)
				{
					analysisOptions.OnAnalyzerException(ex, analyzer, filteredDiagnostic);
				}
				else if (categorizeDiagnostics)
				{
					addCategorizedNonLocalDiagnostic(filteredDiagnostic, analyzer, options, cancellationToken2);
				}
				else
				{
					addNotCategorizedDiagnostic(filteredDiagnostic, options, cancellationToken2);
				}
			}
		};
		AnalyzerExecutor analyzerExecutor = Microsoft.CodeAnalysis.Diagnostics.AnalyzerExecutor.Create(compilation, options, addNotCategorizedDiagnostic, onAnalyzerException, analysisOptions.AnalyzerExceptionFilter, IsCompilerAnalyzer, Analyzers, analysisOptions.GetAnalyzerConfigOptionsProvider, AnalyzerManager, ShouldSkipAnalysisOnGeneratedCode, ShouldSuppressGeneratedCodeDiagnostic, IsGeneratedOrHiddenCodeLocation, IsAnalyzerSuppressedForTree, GetAnalyzerGate, GetOrCreateSemanticModel, _severityFilter, analysisOptions.LogAnalyzerExecutionTime, addCategorizedLocalDiagnostic, addCategorizedNonLocalDiagnostic, delegate(Suppression s)
		{
			_programmaticSuppressions.Add(s);
		});
		Initialize(analyzerExecutor, diagnosticQueue, compilationData, analysisScope, suppressedDiagnosticIds, cancellationToken);
	}

	private SemaphoreSlim? GetAnalyzerGate(DiagnosticAnalyzer analyzer)
	{
		if (AnalyzerGateMap.TryGetValue(analyzer, out SemaphoreSlim value))
		{
			return value;
		}
		return null;
	}

	private ImmutableHashSet<DiagnosticAnalyzer> ComputeNonConfigurableAndCustomConfigurableAnalyzers(ImmutableHashSet<DiagnosticAnalyzer> unsuppressedAnalyzers, CancellationToken cancellationToken)
	{
		ImmutableHashSet<DiagnosticAnalyzer>.Builder builder = ImmutableHashSet.CreateBuilder<DiagnosticAnalyzer>();
		foreach (DiagnosticAnalyzer unsuppressedAnalyzer in unsuppressedAnalyzers)
		{
			foreach (DiagnosticDescriptor supportedDiagnosticDescriptor in AnalyzerManager.GetSupportedDiagnosticDescriptors(unsuppressedAnalyzer, AnalyzerExecutor, cancellationToken))
			{
				if (supportedDiagnosticDescriptor.IsNotConfigurable() || supportedDiagnosticDescriptor.IsCustomSeverityConfigurable())
				{
					builder.Add(unsuppressedAnalyzer);
					break;
				}
			}
		}
		return builder.ToImmutableHashSet();
	}

	private ImmutableHashSet<DiagnosticAnalyzer> ComputeSymbolStartAnalyzers(ImmutableHashSet<DiagnosticAnalyzer> unsuppressedAnalyzers)
	{
		ImmutableHashSet<DiagnosticAnalyzer>.Builder builder = ImmutableHashSet.CreateBuilder<DiagnosticAnalyzer>();
		foreach (SymbolStartAnalyzerAction symbolStartAction in AnalyzerActions.SymbolStartActions)
		{
			if (unsuppressedAnalyzers.Contains(symbolStartAction.Analyzer))
			{
				builder.Add(symbolStartAction.Analyzer);
			}
		}
		return builder.ToImmutableHashSet();
	}

	private static bool ComputeShouldSkipAnalysisOnGeneratedCode(ImmutableHashSet<DiagnosticAnalyzer> analyzers, ImmutableSegmentedDictionary<DiagnosticAnalyzer, GeneratedCodeAnalysisFlags> generatedCodeAnalysisFlagsMap, bool treatAllCodeAsNonGeneratedCode)
	{
		foreach (DiagnosticAnalyzer analyzer in analyzers)
		{
			if (!ShouldSkipAnalysisOnGeneratedCode(analyzer, generatedCodeAnalysisFlagsMap, treatAllCodeAsNonGeneratedCode))
			{
				return false;
			}
		}
		return true;
	}

	private static bool ComputeShouldTreatAllCodeAsNonGeneratedCode(ImmutableHashSet<DiagnosticAnalyzer> analyzers, ImmutableSegmentedDictionary<DiagnosticAnalyzer, GeneratedCodeAnalysisFlags> generatedCodeAnalysisFlagsMap)
	{
		foreach (DiagnosticAnalyzer analyzer in analyzers)
		{
			GeneratedCodeAnalysisFlags num = generatedCodeAnalysisFlagsMap[analyzer];
			bool flag = (num & GeneratedCodeAnalysisFlags.Analyze) != 0;
			bool flag2 = (num & GeneratedCodeAnalysisFlags.ReportDiagnostics) != 0;
			if (!flag || !flag2)
			{
				return false;
			}
		}
		return true;
	}

	private bool ShouldSkipAnalysisOnGeneratedCode(DiagnosticAnalyzer analyzer)
	{
		return ShouldSkipAnalysisOnGeneratedCode(analyzer, GeneratedCodeAnalysisFlagsMap, TreatAllCodeAsNonGeneratedCode);
	}

	private static bool ShouldSkipAnalysisOnGeneratedCode(DiagnosticAnalyzer analyzer, ImmutableSegmentedDictionary<DiagnosticAnalyzer, GeneratedCodeAnalysisFlags> generatedCodeAnalysisFlagsMap, bool treatAllCodeAsNonGeneratedCode)
	{
		if (treatAllCodeAsNonGeneratedCode)
		{
			return false;
		}
		return (generatedCodeAnalysisFlagsMap[analyzer] & GeneratedCodeAnalysisFlags.Analyze) == 0;
	}

	private bool ShouldSuppressGeneratedCodeDiagnostic(Diagnostic diagnostic, DiagnosticAnalyzer analyzer, Compilation compilation, CancellationToken cancellationToken)
	{
		if (TreatAllCodeAsNonGeneratedCode)
		{
			return false;
		}
		if ((GeneratedCodeAnalysisFlagsMap[analyzer] & GeneratedCodeAnalysisFlags.ReportDiagnostics) == 0)
		{
			return IsInGeneratedCode(diagnostic.Location, compilation, cancellationToken);
		}
		return false;
	}

	internal async Task AttachQueueAndProcessAllEventsAsync(AsyncQueue<CompilationEvent> eventQueue, AnalysisScope analysisScope, CancellationToken cancellationToken)
	{
		try
		{
			if (_initializeSucceeded)
			{
				_lazyCompilationEventQueue = eventQueue;
				_lazyQueueRegistration = default(CancellationTokenRegistration);
				await ExecutePrimaryAnalysisTaskAsync(analysisScope, usingPrePopulatedEventQueue: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_lazyPrimaryTask = Task.FromResult(result: true);
			}
		}
		finally
		{
			if (_lazyPrimaryTask == null)
			{
				_lazyPrimaryTask = Task.FromCanceled(new CancellationToken(canceled: true));
			}
		}
	}

	internal void AttachQueueAndStartProcessingEvents(AsyncQueue<CompilationEvent> eventQueue, AnalysisScope analysisScope, bool usingPrePopulatedEventQueue, CancellationToken cancellationToken)
	{
		try
		{
			if (_initializeSucceeded)
			{
				_lazyCompilationEventQueue = eventQueue;
				_lazyQueueRegistration = cancellationToken.Register(delegate
				{
					CompilationEventQueue.TryComplete();
					DiagnosticQueue.TryComplete();
				});
				_lazyPrimaryTask = ExecutePrimaryAnalysisTaskAsync(analysisScope, usingPrePopulatedEventQueue, cancellationToken).ContinueWith((Task c) => DiagnosticQueue.TryComplete(), cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
			}
		}
		finally
		{
			if (_lazyPrimaryTask == null)
			{
				_lazyPrimaryTask = Task.FromCanceled(new CancellationToken(canceled: true));
				DiagnosticQueue.TryComplete();
			}
		}
	}

	private async Task ExecutePrimaryAnalysisTaskAsync(AnalysisScope analysisScope, bool usingPrePopulatedEventQueue, CancellationToken cancellationToken)
	{
		await WhenInitializedTask.ConfigureAwait(continueOnCapturedContext: false);
		if (WhenInitializedTask.IsFaulted)
		{
			OnDriverException(WhenInitializedTask, AnalyzerExecutor, analysisScope.Analyzers, cancellationToken);
		}
		else if (!WhenInitializedTask.IsCanceled)
		{
			await ProcessCompilationEventsAsync(analysisScope, usingPrePopulatedEventQueue, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			_ = usingPrePopulatedEventQueue;
		}
	}

	private static void OnDriverException(Task faultedTask, AnalyzerExecutor analyzerExecutor, ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		Exception ex = faultedTask.Exception?.InnerException;
		if (ex != null && !(ex is OperationCanceledException))
		{
			Diagnostic arg = Microsoft.CodeAnalysis.Diagnostics.AnalyzerExecutor.CreateDriverExceptionDiagnostic(ex);
			DiagnosticAnalyzer arg2 = analyzers[0];
			analyzerExecutor.OnAnalyzerException(ex, arg2, arg, cancellationToken);
		}
	}

	private void ExecuteSyntaxTreeActions(AnalysisScope analysisScope, CancellationToken cancellationToken)
	{
		if (analysisScope.IsSingleFileAnalysis && !analysisScope.IsSyntacticSingleFileAnalysis)
		{
			return;
		}
		foreach (SyntaxTree syntaxTree in analysisScope.SyntaxTrees)
		{
			bool flag = IsGeneratedCode(syntaxTree, cancellationToken);
			SourceOrAdditionalFile file = new SourceOrAdditionalFile(syntaxTree);
			if (flag && DoNotAnalyzeGeneratedCode)
			{
				continue;
			}
			foreach (var (analyzer, syntaxTreeActions) in _lazySyntaxTreeActions)
			{
				if (analysisScope.Contains(analyzer))
				{
					cancellationToken.ThrowIfCancellationRequested();
					AnalyzerExecutor.ExecuteSyntaxTreeActions(syntaxTreeActions, analyzer, file, analysisScope.FilterSpanOpt, flag, cancellationToken);
				}
			}
		}
	}

	private void ExecuteAdditionalFileActions(AnalysisScope analysisScope, CancellationToken cancellationToken)
	{
		if (analysisScope.IsSingleFileAnalysis && !analysisScope.IsSyntacticSingleFileAnalysis)
		{
			return;
		}
		foreach (AdditionalText additionalFile in analysisScope.AdditionalFiles)
		{
			SourceOrAdditionalFile file = new SourceOrAdditionalFile(additionalFile);
			foreach (var (analyzer, additionalFileActions) in _lazyAdditionalFileActions)
			{
				if (analysisScope.Contains(analyzer))
				{
					cancellationToken.ThrowIfCancellationRequested();
					AnalyzerExecutor.ExecuteAdditionalFileActions(additionalFileActions, analyzer, file, analysisScope.FilterSpanOpt, cancellationToken);
				}
			}
		}
	}

	public static AnalyzerDriver CreateAndAttachToCompilation(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerOptions options, AnalyzerManager analyzerManager, Action<Diagnostic> addExceptionDiagnostic, bool reportAnalyzer, SeverityFilter severityFilter, bool trackSuppressedDiagnosticIds, out Compilation newCompilation, CancellationToken cancellationToken)
	{
		Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException = delegate(Exception ex, DiagnosticAnalyzer analyzer, Diagnostic diagnostic)
		{
			addExceptionDiagnostic?.Invoke(diagnostic);
		};
		Func<Exception, bool> analyzerExceptionFilter = null;
		return CreateAndAttachToCompilation(compilation, analyzers, options, analyzerManager, onAnalyzerException, analyzerExceptionFilter, reportAnalyzer, severityFilter, trackSuppressedDiagnosticIds, out newCompilation, cancellationToken);
	}

	internal static AnalyzerDriver CreateAndAttachToCompilation(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerOptions options, AnalyzerManager analyzerManager, Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException, Func<Exception, bool>? analyzerExceptionFilter, bool reportAnalyzer, SeverityFilter severityFilter, bool trackSuppressedDiagnosticIds, out Compilation newCompilation, CancellationToken cancellationToken)
	{
		AnalyzerDriver analyzerDriver = compilation.CreateAnalyzerDriver(analyzers, analyzerManager, severityFilter);
		newCompilation = compilation.WithSemanticModelProvider(CachingSemanticModelProvider.Instance).WithEventQueue(new AsyncQueue<CompilationEvent>());
		bool categorizeDiagnostics = false;
		CompilationWithAnalyzersOptions analysisOptions = new CompilationWithAnalyzersOptions(options, onAnalyzerException, concurrentAnalysis: true, reportAnalyzer, reportSuppressedDiagnostics: false, analyzerExceptionFilter);
		AnalysisScope analysisScope = AnalysisScope.CreateForBatchCompile(newCompilation, options.GetAdditionalFiles(), analyzers);
		analyzerDriver.Initialize(newCompilation, analysisOptions, new CompilationData(newCompilation), analysisScope, categorizeDiagnostics, trackSuppressedDiagnosticIds, cancellationToken);
		analyzerDriver.AttachQueueAndStartProcessingEvents(newCompilation.EventQueue, analysisScope, usingPrePopulatedEventQueue: false, cancellationToken);
		return analyzerDriver;
	}

	public async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(Compilation compilation, CancellationToken cancellationToken)
	{
		DiagnosticBag allDiagnostics = DiagnosticBag.GetInstance();
		if (CompilationEventQueue.IsCompleted)
		{
			await WhenCompletedTask.ConfigureAwait(continueOnCapturedContext: false);
			if (WhenCompletedTask.IsFaulted)
			{
				OnDriverException(WhenCompletedTask, AnalyzerExecutor, Analyzers, cancellationToken);
			}
		}
		SuppressMessageAttributeState suppressMessageAttributeState = CurrentCompilationData.SuppressMessageAttributeState;
		bool reportSuppressedDiagnostics = compilation.Options.ReportSuppressedDiagnostics;
		Diagnostic d;
		while (DiagnosticQueue.TryDequeue(out d))
		{
			d = suppressMessageAttributeState.ApplySourceSuppressions(d);
			if (reportSuppressedDiagnostics || !d.IsSuppressed)
			{
				allDiagnostics.Add(d);
			}
		}
		return allDiagnostics.ToReadOnlyAndFree();
	}

	public ImmutableArray<(DiagnosticDescriptor Descriptor, DiagnosticDescriptorErrorLoggerInfo Info)> GetAllDiagnosticDescriptorsWithInfo(CancellationToken cancellationToken, out double totalAnalyzerExecutionTime)
	{
		PooledHashSet<string> instance = PooledHashSet<string>.GetInstance();
		ImmutableHashSet<DiagnosticAnalyzer> immutableHashSet = SuppressedAnalyzersForTreeMap.SelectMany<KeyValuePair<SyntaxTree, ImmutableHashSet<DiagnosticAnalyzer>>, DiagnosticAnalyzer>((KeyValuePair<SyntaxTree, ImmutableHashSet<DiagnosticAnalyzer>> kvp) => kvp.Value).ToImmutableHashSet();
		totalAnalyzerExecutionTime = AnalyzerExecutionTimes.Sum<KeyValuePair<DiagnosticAnalyzer, TimeSpan>>((KeyValuePair<DiagnosticAnalyzer, TimeSpan> kvp) => kvp.Value.TotalSeconds);
		ArrayBuilder<(DiagnosticDescriptor, DiagnosticDescriptorErrorLoggerInfo)> instance2 = ArrayBuilder<(DiagnosticDescriptor, DiagnosticDescriptorErrorLoggerInfo)>.GetInstance();
		foreach (DiagnosticAnalyzer analyzer in Analyzers)
		{
			ImmutableArray<DiagnosticDescriptor> supportedDiagnosticDescriptors = AnalyzerManager.GetSupportedDiagnosticDescriptors(analyzer, AnalyzerExecutor, cancellationToken);
			bool flag = !UnsuppressedAnalyzers.Contains(analyzer) || immutableHashSet.Contains(analyzer);
			double num = 0.0;
			if (AnalyzerExecutionTimes.TryGetValue(analyzer, out var value))
			{
				num = value.TotalSeconds;
			}
			int executionPercentage = (int)(num * 100.0 / totalAnalyzerExecutionTime);
			foreach (DiagnosticDescriptor item3 in supportedDiagnosticDescriptors)
			{
				if (instance.Add(item3.Id))
				{
					bool hasAnyExternalSuppression = flag || SuppressedDiagnosticIdsForUnsuppressedAnalyzers.Contains(item3.Id);
					ImmutableHashSet<ReportDiagnostic> effectiveSeverities = GetEffectiveSeverities(item3, AnalyzerExecutor.Compilation, AnalyzerExecutor.AnalyzerOptions, cancellationToken);
					DiagnosticDescriptorErrorLoggerInfo item = new DiagnosticDescriptorErrorLoggerInfo(num, executionPercentage, effectiveSeverities, hasAnyExternalSuppression);
					instance2.Add((item3, item));
				}
			}
		}
		instance.Free();
		return instance2.ToImmutableAndFree();
		static ImmutableHashSet<ReportDiagnostic> GetEffectiveSeverities(DiagnosticDescriptor descriptor, Compilation compilation, AnalyzerOptions analyzerOptions, CancellationToken cancellationToken2)
		{
			ReportDiagnostic reportDiagnostic = (descriptor.IsEnabledByDefault ? DiagnosticDescriptor.MapSeverityToReport(descriptor.DefaultSeverity) : ReportDiagnostic.Suppress);
			if (descriptor.IsNotConfigurable())
			{
				return ImmutableHashSet.Create(reportDiagnostic);
			}
			if (!compilation.Options.SpecificDiagnosticOptions.TryGetValue(descriptor.Id, out var value2))
			{
				SyntaxTreeOptionsProvider? syntaxTreeOptionsProvider = compilation.Options.SyntaxTreeOptionsProvider;
				if (syntaxTreeOptionsProvider == null || !syntaxTreeOptionsProvider.TryGetGlobalDiagnosticValue(descriptor.Id, cancellationToken2, out value2))
				{
					goto IL_0067;
				}
			}
			if (value2 != ReportDiagnostic.Default)
			{
				reportDiagnostic = value2;
			}
			goto IL_0067;
			IL_0067:
			if (reportDiagnostic == ReportDiagnostic.Warn && compilation.Options.GeneralDiagnosticOption == ReportDiagnostic.Error)
			{
				reportDiagnostic = ReportDiagnostic.Error;
			}
			SyntaxTreeOptionsProvider syntaxTreeOptionsProvider2 = compilation.Options.SyntaxTreeOptionsProvider;
			if (syntaxTreeOptionsProvider2 == null || compilation.SyntaxTrees.IsEmpty())
			{
				return ImmutableHashSet.Create(reportDiagnostic);
			}
			ImmutableHashSet<ReportDiagnostic>.Builder builder = ImmutableHashSet.CreateBuilder<ReportDiagnostic>();
			foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
			{
				ReportDiagnostic item2 = reportDiagnostic;
				if (syntaxTreeOptionsProvider2.TryGetDiagnosticValue(syntaxTree, descriptor.Id, cancellationToken2, out value2) || analyzerOptions.TryGetSeverityFromBulkConfiguration(syntaxTree, compilation, descriptor, cancellationToken2, out value2))
				{
					if (value2 == ReportDiagnostic.Warn && compilation.Options.GeneralDiagnosticOption == ReportDiagnostic.Error)
					{
						value2 = ReportDiagnostic.Error;
					}
					item2 = value2;
				}
				builder.Add(item2);
			}
			return builder.ToImmutable();
		}
	}

	private SemanticModel GetOrCreateSemanticModel(SyntaxTree tree)
	{
		return GetOrCreateSemanticModel(tree, AnalyzerExecutor.Compilation);
	}

	protected SemanticModel GetOrCreateSemanticModel(SyntaxTree tree, Compilation compilation)
	{
		return SemanticModelProvider.GetSemanticModel(tree, compilation);
	}

	public void ApplyProgrammaticSuppressions(DiagnosticBag reportedDiagnostics, Compilation compilation, CancellationToken cancellationToken)
	{
		if (HasDiagnosticSuppressors)
		{
			ImmutableArray<Diagnostic> diagnostics = ApplyProgrammaticSuppressionsCore(reportedDiagnostics.ToReadOnly(), compilation, cancellationToken);
			reportedDiagnostics.Clear();
			reportedDiagnostics.AddRange(diagnostics);
		}
	}

	public ImmutableArray<Diagnostic> ApplyProgrammaticSuppressions(ImmutableArray<Diagnostic> reportedDiagnostics, Compilation compilation, CancellationToken cancellationToken)
	{
		if (reportedDiagnostics.IsEmpty || !HasDiagnosticSuppressors)
		{
			return reportedDiagnostics;
		}
		return ApplyProgrammaticSuppressionsCore(reportedDiagnostics, compilation, cancellationToken);
	}

	private ImmutableArray<Diagnostic> ApplyProgrammaticSuppressionsCore(ImmutableArray<Diagnostic> reportedDiagnostics, Compilation compilation, CancellationToken cancellationToken)
	{
		try
		{
			IEnumerable<Diagnostic> enumerable = reportedDiagnostics.Where((Diagnostic d) => !d.IsSuppressed && !d.IsNotConfigurable() && d.DefaultSeverity != DiagnosticSeverity.Error && !_diagnosticsProcessedForProgrammaticSuppressions.Contains(d));
			if (enumerable.IsEmpty())
			{
				return reportedDiagnostics;
			}
			executeSuppressionActions(enumerable, compilation.Options.ConcurrentBuild);
			if (_programmaticSuppressions.IsEmpty)
			{
				return reportedDiagnostics;
			}
			ArrayBuilder<Diagnostic> instance = ArrayBuilder<Diagnostic>.GetInstance(reportedDiagnostics.Length);
			ImmutableDictionary<Diagnostic, ProgrammaticSuppressionInfo> immutableDictionary = createProgrammaticSuppressionsByDiagnosticMap(_programmaticSuppressions);
			foreach (Diagnostic item3 in reportedDiagnostics)
			{
				if (immutableDictionary.TryGetValue(item3, out var value))
				{
					Diagnostic item = item3.WithProgrammaticSuppression(value);
					instance.Add(item);
				}
				else
				{
					instance.Add(item3);
				}
			}
			return instance.ToImmutableAndFree();
		}
		finally
		{
			_diagnosticsProcessedForProgrammaticSuppressions.AddRange(reportedDiagnostics);
		}
		static ImmutableDictionary<Diagnostic, ProgrammaticSuppressionInfo> createProgrammaticSuppressionsByDiagnosticMap(ConcurrentSet<Suppression> programmaticSuppressions)
		{
			PooledDictionary<Diagnostic, ArrayBuilder<Suppression>> instance2 = PooledDictionary<Diagnostic, ArrayBuilder<Suppression>>.GetInstance();
			foreach (Suppression programmaticSuppression in programmaticSuppressions)
			{
				if (!instance2.TryGetValue(programmaticSuppression.SuppressedDiagnostic, out var value2))
				{
					value2 = ArrayBuilder<Suppression>.GetInstance();
					instance2.Add(programmaticSuppression.SuppressedDiagnostic, value2);
				}
				if (!value2.Contains(programmaticSuppression))
				{
					value2.Add(programmaticSuppression);
				}
			}
			ImmutableDictionary<Diagnostic, ProgrammaticSuppressionInfo>.Builder builder = ImmutableDictionary.CreateBuilder<Diagnostic, ProgrammaticSuppressionInfo>();
			foreach (var (key, arrayBuilder2) in instance2)
			{
				builder.Add(key, new ProgrammaticSuppressionInfo(arrayBuilder2.ToImmutableAndFree()));
			}
			instance2.Free();
			return builder.ToImmutable();
		}
		void executeSuppressionActions(IEnumerable<Diagnostic> enumerable3, bool concurrent)
		{
			IEnumerable<DiagnosticSuppressor> enumerable2 = Analyzers.OfType<DiagnosticSuppressor>();
			if (concurrent)
			{
				ArrayBuilder<Task> instance2 = ArrayBuilder<Task>.GetInstance();
				try
				{
					foreach (DiagnosticSuppressor suppressor in enumerable2)
					{
						ImmutableArray<Diagnostic> suppressableDiagnostics = getSuppressableDiagnostics(suppressor);
						if (!suppressableDiagnostics.IsEmpty)
						{
							Task item2 = Task.Run(delegate
							{
								AnalyzerExecutor.ExecuteSuppressionAction(suppressor, suppressableDiagnostics, cancellationToken);
							}, cancellationToken);
							instance2.Add(item2);
						}
					}
					Task.WaitAll(instance2.ToArray(), cancellationToken);
					return;
				}
				finally
				{
					instance2.Free();
				}
			}
			foreach (DiagnosticSuppressor item4 in enumerable2)
			{
				AnalyzerExecutor.ExecuteSuppressionAction(item4, getSuppressableDiagnostics(item4), cancellationToken);
			}
		}
		ImmutableArray<Diagnostic> getSuppressableDiagnostics(DiagnosticSuppressor suppressor)
		{
			ImmutableArray<SuppressionDescriptor> supportedSuppressionDescriptors = AnalyzerManager.GetSupportedSuppressionDescriptors(suppressor, AnalyzerExecutor, cancellationToken);
			if (supportedSuppressionDescriptors.IsEmpty)
			{
				return ImmutableArray<Diagnostic>.Empty;
			}
			using TemporaryArray<Diagnostic> temporaryArray = TemporaryArray<Diagnostic>.Empty;
			foreach (Diagnostic diagnostic in P_1.reportedDiagnostics)
			{
				if (supportedSuppressionDescriptors.Contains((SuppressionDescriptor s) => s.SuppressedDiagnosticId == diagnostic.Id))
				{
					temporaryArray.Add(diagnostic);
				}
			}
			return temporaryArray.ToImmutableAndClear();
		}
	}

	public ImmutableArray<Diagnostic> DequeueLocalDiagnosticsAndApplySuppressions(DiagnosticAnalyzer analyzer, bool syntax, Compilation compilation, CancellationToken cancellationToken)
	{
		ImmutableArray<Diagnostic> diagnostics = (syntax ? DiagnosticQueue.DequeueLocalSyntaxDiagnostics(analyzer) : DiagnosticQueue.DequeueLocalSemanticDiagnostics(analyzer));
		return FilterDiagnosticsSuppressedInSourceOrByAnalyzers(diagnostics, compilation, cancellationToken);
	}

	public ImmutableArray<Diagnostic> DequeueNonLocalDiagnosticsAndApplySuppressions(DiagnosticAnalyzer analyzer, Compilation compilation, CancellationToken cancellationToken)
	{
		ImmutableArray<Diagnostic> diagnostics = DiagnosticQueue.DequeueNonLocalDiagnostics(analyzer);
		return FilterDiagnosticsSuppressedInSourceOrByAnalyzers(diagnostics, compilation, cancellationToken);
	}

	private ImmutableArray<Diagnostic> FilterDiagnosticsSuppressedInSourceOrByAnalyzers(ImmutableArray<Diagnostic> diagnostics, Compilation compilation, CancellationToken cancellationToken)
	{
		diagnostics = FilterDiagnosticsSuppressedInSource(diagnostics, compilation, CurrentCompilationData.SuppressMessageAttributeState);
		return ApplyProgrammaticSuppressionsAndFilterDiagnostics(diagnostics, compilation, cancellationToken);
	}

	private static ImmutableArray<Diagnostic> FilterDiagnosticsSuppressedInSource(ImmutableArray<Diagnostic> diagnostics, Compilation compilation, SuppressMessageAttributeState suppressMessageState)
	{
		if (diagnostics.IsEmpty)
		{
			return diagnostics;
		}
		bool reportSuppressedDiagnostics = compilation.Options.ReportSuppressedDiagnostics;
		ImmutableArray<Diagnostic>.Builder builder = ImmutableArray.CreateBuilder<Diagnostic>();
		for (int i = 0; i < diagnostics.Length; i++)
		{
			Diagnostic diagnostic = suppressMessageState.ApplySourceSuppressions(diagnostics[i]);
			if (reportSuppressedDiagnostics || !diagnostic.IsSuppressed)
			{
				builder.Add(diagnostic);
			}
		}
		return builder.ToImmutable();
	}

	internal ImmutableArray<Diagnostic> ApplyProgrammaticSuppressionsAndFilterDiagnostics(ImmutableArray<Diagnostic> reportedDiagnostics, Compilation compilation, CancellationToken cancellationToken)
	{
		if (reportedDiagnostics.IsEmpty)
		{
			return reportedDiagnostics;
		}
		ImmutableArray<Diagnostic> immutableArray = ApplyProgrammaticSuppressions(reportedDiagnostics, compilation, cancellationToken);
		if (compilation.Options.ReportSuppressedDiagnostics || immutableArray.All((Diagnostic d) => !d.IsSuppressed))
		{
			return immutableArray;
		}
		return immutableArray.WhereAsArray((Diagnostic d) => !d.IsSuppressed);
	}

	private bool IsInGeneratedCode(Location location, Compilation compilation, CancellationToken cancellationToken)
	{
		if (!location.IsInSource)
		{
			return false;
		}
		if (IsGeneratedOrHiddenCodeLocation(location.SourceTree, location.SourceSpan, cancellationToken))
		{
			return true;
		}
		if (_lazyGeneratedCodeAttribute != null)
		{
			ImmutableHashSet<ISymbol> immutableHashSet = getOrComputeGeneratedCodeSymbolsInTree(location.SourceTree, compilation, cancellationToken);
			if (immutableHashSet.Count > 0)
			{
				SemanticModel semanticModel = compilation.GetSemanticModel(location.SourceTree);
				for (SyntaxNode syntaxNode = location.SourceTree.GetRoot(cancellationToken).FindNode(location.SourceSpan, findInsideTrivia: false, getInnermostNodeForTie: true); syntaxNode != null; syntaxNode = syntaxNode.Parent)
				{
					foreach (ISymbol item in semanticModel.GetDeclaredSymbolsForNode(syntaxNode, cancellationToken))
					{
						if (immutableHashSet.Contains(item))
						{
							return true;
						}
					}
				}
			}
		}
		return false;
		static ImmutableHashSet<ISymbol> computeGeneratedCodeSymbolsInTree(SyntaxTree tree, Compilation compilation2, INamedTypeSymbol generatedCodeAttribute, CancellationToken cancellationToken2)
		{
			SyntaxNode root = tree.GetRoot(cancellationToken2);
			if (!containsGeneratedCodeToken(root))
			{
				return ImmutableHashSet<ISymbol>.Empty;
			}
			SemanticModel semanticModel2 = compilation2.GetSemanticModel(tree);
			TextSpan fullSpan = root.FullSpan;
			ArrayBuilder<DeclarationInfo> instance = ArrayBuilder<DeclarationInfo>.GetInstance();
			semanticModel2.ComputeDeclarationsInSpan(fullSpan, getSymbol: true, instance, cancellationToken2);
			ImmutableHashSet<ISymbol>.Builder builder = null;
			foreach (DeclarationInfo item2 in instance)
			{
				ISymbol declaredSymbol = item2.DeclaredSymbol;
				if (declaredSymbol != null && GeneratedCodeUtilities.IsGeneratedSymbolWithGeneratedCodeAttribute(declaredSymbol, generatedCodeAttribute))
				{
					if (builder == null)
					{
						builder = ImmutableHashSet.CreateBuilder<ISymbol>();
					}
					builder.Add(declaredSymbol);
				}
			}
			instance.Free();
			if (builder == null)
			{
				return ImmutableHashSet<ISymbol>.Empty;
			}
			return builder.ToImmutable();
		}
		static bool containsGeneratedCodeToken(SyntaxNode root)
		{
			return root.DescendantTokens().Any((SyntaxToken token) => string.Equals(token.ValueText, "GeneratedCode", StringComparison.Ordinal) || string.Equals(token.ValueText, "GeneratedCodeAttribute", StringComparison.Ordinal));
		}
		ImmutableHashSet<ISymbol> getOrComputeGeneratedCodeSymbolsInTree(SyntaxTree tree, Compilation compilation2, CancellationToken cancellationToken2)
		{
			ImmutableHashSet<ISymbol> value;
			lock (GeneratedCodeSymbolsForTreeMap)
			{
				if (GeneratedCodeSymbolsForTreeMap.TryGetValue(tree, out value))
				{
					return value;
				}
			}
			value = computeGeneratedCodeSymbolsInTree(tree, compilation2, _lazyGeneratedCodeAttribute, cancellationToken2);
			lock (GeneratedCodeSymbolsForTreeMap)
			{
				if (!GeneratedCodeSymbolsForTreeMap.TryGetValue(tree, out ImmutableHashSet<ISymbol> _))
				{
					GeneratedCodeSymbolsForTreeMap.Add(tree, value);
				}
			}
			return value;
		}
	}

	private bool IsAnalyzerSuppressedForTree(DiagnosticAnalyzer analyzer, SyntaxTree tree, SyntaxTreeOptionsProvider? options, CancellationToken cancellationToken)
	{
		if (!SuppressedAnalyzersForTreeMap.TryGetValue(tree, out ImmutableHashSet<DiagnosticAnalyzer> value))
		{
			value = SuppressedAnalyzersForTreeMap.GetOrAdd(tree, ComputeSuppressedAnalyzersForTree(tree, options, cancellationToken));
		}
		return value.Contains(analyzer);
	}

	private ImmutableHashSet<DiagnosticAnalyzer> ComputeSuppressedAnalyzersForTree(SyntaxTree tree, SyntaxTreeOptionsProvider? options, CancellationToken cancellationToken)
	{
		if (options == null)
		{
			return ImmutableHashSet<DiagnosticAnalyzer>.Empty;
		}
		ImmutableHashSet<DiagnosticAnalyzer>.Builder builder = null;
		foreach (DiagnosticAnalyzer unsuppressedAnalyzer in UnsuppressedAnalyzers)
		{
			if (NonConfigurableAndCustomConfigurableAnalyzers.Contains(unsuppressedAnalyzer) || ((SymbolStartAnalyzers.Contains(unsuppressedAnalyzer) || CompilationEndAnalyzers.Contains(unsuppressedAnalyzer)) && !ShouldSkipAnalysisOnGeneratedCode(unsuppressedAnalyzer)))
			{
				continue;
			}
			ImmutableArray<DiagnosticDescriptor> supportedDiagnosticDescriptors = AnalyzerManager.GetSupportedDiagnosticDescriptors(unsuppressedAnalyzer, AnalyzerExecutor, cancellationToken);
			bool flag = false;
			foreach (DiagnosticDescriptor item in supportedDiagnosticDescriptors)
			{
				ReportDiagnostic reportDiagnostic = item.GetEffectiveSeverity(AnalyzerExecutor.Compilation.Options);
				if (options.TryGetDiagnosticValue(tree, item.Id, cancellationToken, out var severity) || options.TryGetGlobalDiagnosticValue(item.Id, cancellationToken, out severity))
				{
					reportDiagnostic = severity;
				}
				if (!item.IsEnabledByDefault && reportDiagnostic == ReportDiagnostic.Default)
				{
					reportDiagnostic = ReportDiagnostic.Suppress;
				}
				if (reportDiagnostic != ReportDiagnostic.Suppress)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (builder == null)
				{
					builder = ImmutableHashSet.CreateBuilder<DiagnosticAnalyzer>();
				}
				builder.Add(unsuppressedAnalyzer);
			}
		}
		if (builder == null)
		{
			return ImmutableHashSet<DiagnosticAnalyzer>.Empty;
		}
		return builder.ToImmutable();
	}

	internal TimeSpan ResetAnalyzerExecutionTime(DiagnosticAnalyzer analyzer)
	{
		return AnalyzerExecutor.ResetAnalyzerExecutionTime(analyzer);
	}

	private static ImmutableArray<(DiagnosticAnalyzer, ImmutableArray<ImmutableArray<SymbolAnalyzerAction>>)> MakeSymbolActionsByKind(in AnalyzerActions analyzerActions)
	{
		ArrayBuilder<(DiagnosticAnalyzer, ImmutableArray<ImmutableArray<SymbolAnalyzerAction>>)> instance = ArrayBuilder<(DiagnosticAnalyzer, ImmutableArray<ImmutableArray<SymbolAnalyzerAction>>)>.GetInstance();
		IEnumerable<IGrouping<DiagnosticAnalyzer, SymbolAnalyzerAction>> enumerable = from action in analyzerActions.SymbolActions
			group action by action.Analyzer;
		ArrayBuilder<ArrayBuilder<SymbolAnalyzerAction>> instance2 = ArrayBuilder<ArrayBuilder<SymbolAnalyzerAction>>.GetInstance();
		foreach (IGrouping<DiagnosticAnalyzer, SymbolAnalyzerAction> item2 in enumerable)
		{
			instance2.Clear();
			foreach (SymbolAnalyzerAction item3 in item2)
			{
				foreach (SymbolKind item4 in item3.Kinds.Distinct())
				{
					if (item4 <= (SymbolKind)100)
					{
						while ((int)item4 >= instance2.Count)
						{
							instance2.Add(ArrayBuilder<SymbolAnalyzerAction>.GetInstance());
						}
						instance2[(int)item4].Add(item3);
					}
				}
			}
			ImmutableArray<ImmutableArray<SymbolAnalyzerAction>> item = instance2.Select((ArrayBuilder<SymbolAnalyzerAction> a) => a.ToImmutableAndFree()).ToImmutableArray();
			instance.Add((item2.Key, item));
		}
		instance2.Free();
		return instance.ToImmutableAndFree();
	}

	private static ImmutableArray<(DiagnosticAnalyzer, ImmutableArray<TAnalyzerAction>)> MakeActionsByAnalyzer<TAnalyzerAction>(in ImmutableArray<TAnalyzerAction> analyzerActions) where TAnalyzerAction : AnalyzerAction
	{
		ArrayBuilder<(DiagnosticAnalyzer, ImmutableArray<TAnalyzerAction>)> instance = ArrayBuilder<(DiagnosticAnalyzer, ImmutableArray<TAnalyzerAction>)>.GetInstance();
		foreach (IGrouping<DiagnosticAnalyzer, TAnalyzerAction> item in from action in analyzerActions
			group action by action.Analyzer)
		{
			instance.Add((item.Key, item.ToImmutableArray()));
		}
		return instance.ToImmutableAndFree();
	}

	private static ImmutableHashSet<DiagnosticAnalyzer> MakeCompilationEndAnalyzers(ImmutableArray<(DiagnosticAnalyzer, ImmutableArray<CompilationAnalyzerAction>)> compilationEndActionsByAnalyzer)
	{
		ImmutableHashSet<DiagnosticAnalyzer>.Builder builder = ImmutableHashSet.CreateBuilder<DiagnosticAnalyzer>();
		foreach (var item2 in compilationEndActionsByAnalyzer)
		{
			DiagnosticAnalyzer item = item2.Item1;
			builder.Add(item);
		}
		return builder.ToImmutable();
	}

	private async Task ProcessCompilationEventsAsync(AnalysisScope analysisScope, bool prePopulatedEventQueue, CancellationToken cancellationToken)
	{
		try
		{
			CompilationCompletedEvent completedEvent = null;
			if (analysisScope.ConcurrentAnalysis)
			{
				int workerCount = (prePopulatedEventQueue ? Math.Min(CompilationEventQueue.Count, _workerCount) : _workerCount);
				Task<CompilationCompletedEvent?>[] workerTasks = new Task<CompilationCompletedEvent>[workerCount];
				for (int i = 0; i < workerCount; i++)
				{
					workerTasks[i] = Task.Run(async () => await ProcessCompilationEventsCoreAsync(analysisScope, prePopulatedEventQueue, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
				}
				cancellationToken.ThrowIfCancellationRequested();
				Task task = (analysisScope.SyntaxTrees.Any() ? Task.Run(delegate
				{
					ExecuteSyntaxTreeActions(analysisScope, cancellationToken);
				}, cancellationToken) : Task.CompletedTask);
				Task task2 = (analysisScope.AdditionalFiles.Any() ? Task.Run(delegate
				{
					ExecuteAdditionalFileActions(analysisScope, cancellationToken);
				}, cancellationToken) : Task.CompletedTask);
				if (workerTasks.Length != 0 || task.Status != TaskStatus.RanToCompletion || task2.Status != TaskStatus.RanToCompletion)
				{
					await Task.WhenAll(workerTasks.Concat(task).Concat(task2)).ConfigureAwait(continueOnCapturedContext: false);
				}
				for (int num = 0; num < workerCount; num++)
				{
					if (workerTasks[num].Status == TaskStatus.RanToCompletion && workerTasks[num].Result != null)
					{
						completedEvent = workerTasks[num].Result;
						break;
					}
				}
			}
			else
			{
				completedEvent = await ProcessCompilationEventsCoreAsync(analysisScope, prePopulatedEventQueue, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				ExecuteSyntaxTreeActions(analysisScope, cancellationToken);
				ExecuteAdditionalFileActions(analysisScope, cancellationToken);
			}
			if (completedEvent != null)
			{
				await ProcessEventAsync(completedEvent, analysisScope, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		catch (Exception exception) when (FatalError.ReportAndPropagateUnlessCanceled(exception, cancellationToken))
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/DiagnosticAnalyzer/AnalyzerDriver.cs", 1581);
		}
	}

	private async Task<CompilationCompletedEvent?> ProcessCompilationEventsCoreAsync(AnalysisScope analysisScope, bool prePopulatedEventQueue, CancellationToken cancellationToken)
	{
		_ = 1;
		try
		{
			CompilationCompletedEvent completedEvent = null;
			while (true)
			{
				cancellationToken.ThrowIfCancellationRequested();
				if ((prePopulatedEventQueue || CompilationEventQueue.IsCompleted) && CompilationEventQueue.Count == 0)
				{
					break;
				}
				if (!CompilationEventQueue.TryDequeue(out CompilationEvent d))
				{
					if (prePopulatedEventQueue)
					{
						return completedEvent;
					}
					Optional<CompilationEvent> optional = await CompilationEventQueue.TryDequeueAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					if (!optional.HasValue)
					{
						break;
					}
					d = optional.Value;
				}
				if (!(d is CompilationCompletedEvent compilationCompletedEvent))
				{
					await ProcessEventAsync(d, analysisScope, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				else
				{
					completedEvent = compilationCompletedEvent;
				}
			}
			return completedEvent;
		}
		catch (Exception exception) when (FatalError.ReportAndPropagateUnlessCanceled(exception, cancellationToken))
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/DiagnosticAnalyzer/AnalyzerDriver.cs", 1640);
		}
	}

	private async Task ProcessEventAsync(CompilationEvent e, AnalysisScope analysisScope, CancellationToken cancellationToken)
	{
		EventProcessedState eventProcessedState = await TryProcessEventCoreAsync(e, analysisScope, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		ImmutableArray<DiagnosticAnalyzer> processedAnalyzers;
		switch (eventProcessedState.Kind)
		{
		default:
			return;
		case EventProcessedStateKind.Processed:
			processedAnalyzers = analysisScope.Analyzers;
			break;
		case EventProcessedStateKind.PartiallyProcessed:
			processedAnalyzers = eventProcessedState.SubsetProcessedAnalyzers;
			break;
		}
		await OnEventProcessedCoreAsync(e, processedAnalyzers, analysisScope, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task OnEventProcessedCoreAsync(CompilationEvent compilationEvent, ImmutableArray<DiagnosticAnalyzer> processedAnalyzers, AnalysisScope analysisScope, CancellationToken cancellationToken)
	{
		if (!(compilationEvent is SymbolDeclaredCompilationEvent symbolDeclaredCompilationEvent))
		{
			if (!(compilationEvent is CompilationUnitCompletedEvent compilationUnitCompletedEvent))
			{
				if (compilationEvent is CompilationCompletedEvent compilationCompletedEvent)
				{
					CompilationCompletedEvent compilationCompletedEvent2 = compilationCompletedEvent;
					SemanticModelProvider.ClearCache(compilationCompletedEvent2.Compilation);
				}
			}
			else
			{
				CompilationUnitCompletedEvent compilationUnitCompletedEvent2 = compilationUnitCompletedEvent;
				if (!compilationUnitCompletedEvent2.FilterSpan.HasValue)
				{
					SemanticModelProvider.ClearCache(compilationUnitCompletedEvent2.CompilationUnit, compilationUnitCompletedEvent2.Compilation);
				}
			}
			return;
		}
		SymbolDeclaredCompilationEvent symbolDeclaredEvent = symbolDeclaredCompilationEvent;
		if (AnalyzerActions.SymbolStartActionsCount > 0)
		{
			foreach (DiagnosticAnalyzer item2 in processedAnalyzers)
			{
				await onSymbolAndMembersProcessedAsync(symbolDeclaredEvent.Symbol, item2).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		async Task onSymbolAndMembersProcessedAsync(ISymbol symbol, DiagnosticAnalyzer analyzer)
		{
			if (AnalyzerActions.SymbolStartActionsCount != 0 && !symbol.IsImplicitlyDeclared)
			{
				if (symbol is INamespaceOrTypeSymbol item)
				{
					PerSymbolAnalyzerActionsCache.TryRemove((item, analyzer), out IGroupedAnalyzerActions _);
				}
				await processContainerOnMemberCompletedAsync(symbol.ContainingNamespace, symbol, analyzer).ConfigureAwait(continueOnCapturedContext: false);
				for (INamedTypeSymbol type = symbol.ContainingType; type != null; type = type.ContainingType)
				{
					await processContainerOnMemberCompletedAsync(type, symbol, analyzer).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
		}
		async Task processContainerOnMemberCompletedAsync(INamespaceOrTypeSymbol containerSymbol, ISymbol processedMemberSymbol, DiagnosticAnalyzer analyzer)
		{
			if (containerSymbol != null && AnalyzerExecutor.TryExecuteSymbolEndActionsForContainer(containerSymbol, processedMemberSymbol, analyzer, s_getTopmostNodeForAnalysis, IsGeneratedCodeSymbol(containerSymbol, cancellationToken), analysisScope.OriginalFilterFile?.SourceTree, analysisScope.OriginalFilterSpan, cancellationToken, out SymbolDeclaredCompilationEvent containingSymbolDeclaredEvent))
			{
				await OnEventProcessedCoreAsync(containingSymbolDeclaredEvent, ImmutableArray.Create(analyzer), analysisScope, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	private async ValueTask<EventProcessedState> TryProcessEventCoreAsync(CompilationEvent compilationEvent, AnalysisScope analysisScope, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (!(compilationEvent is SymbolDeclaredCompilationEvent symbolEvent))
		{
			if (!(compilationEvent is CompilationUnitCompletedEvent completedEvent))
			{
				if (!(compilationEvent is CompilationCompletedEvent endEvent))
				{
					if (compilationEvent is CompilationStartedEvent startedEvent)
					{
						ProcessCompilationStarted(startedEvent, analysisScope, cancellationToken);
						return EventProcessedState.Processed;
					}
					throw new InvalidOperationException("Unexpected compilation event of type " + compilationEvent.GetType().Name);
				}
				ProcessCompilationCompleted(endEvent, analysisScope, cancellationToken);
				return EventProcessedState.Processed;
			}
			ProcessCompilationUnitCompleted(completedEvent, analysisScope, cancellationToken);
			return EventProcessedState.Processed;
		}
		return await TryProcessSymbolDeclaredAsync(symbolEvent, analysisScope, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async ValueTask<EventProcessedState> TryProcessSymbolDeclaredAsync(SymbolDeclaredCompilationEvent symbolEvent, AnalysisScope analysisScope, CancellationToken cancellationToken)
	{
		ISymbol symbol = symbolEvent.Symbol;
		bool isGeneratedCodeSymbol = IsGeneratedCodeSymbol(symbol, cancellationToken);
		bool skipSymbolAnalysis = AnalysisScope.ShouldSkipSymbolAnalysis(symbolEvent);
		bool skipDeclarationAnalysis = AnalysisScope.ShouldSkipDeclarationAnalysis(symbol);
		bool hasPerSymbolActions = AnalyzerActions.SymbolStartActionsCount > 0 && (!skipSymbolAnalysis || !skipDeclarationAnalysis);
		IGroupedAnalyzerActions groupedAnalyzerActions = ((!hasPerSymbolActions) ? EmptyGroupedActions : (await GetPerSymbolAnalyzerActionsAsync(symbol, analysisScope, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)));
		IGroupedAnalyzerActions groupedAnalyzerActions2 = groupedAnalyzerActions;
		if (!skipSymbolAnalysis)
		{
			ExecuteSymbolActions(symbolEvent, analysisScope, isGeneratedCodeSymbol, cancellationToken);
		}
		if (!skipDeclarationAnalysis)
		{
			ExecuteDeclaringReferenceActions(symbolEvent, analysisScope, isGeneratedCodeSymbol, groupedAnalyzerActions2, cancellationToken);
		}
		if (hasPerSymbolActions && !TryExecuteSymbolEndActions(groupedAnalyzerActions2.AnalyzerActions, symbolEvent, analysisScope, isGeneratedCodeSymbol, cancellationToken, out ImmutableArray<DiagnosticAnalyzer> subsetProcessedAnalyzers))
		{
			return subsetProcessedAnalyzers.IsEmpty ? EventProcessedState.NotProcessed : EventProcessedState.CreatePartiallyProcessed(subsetProcessedAnalyzers);
		}
		return EventProcessedState.Processed;
	}

	private void ExecuteSymbolActions(SymbolDeclaredCompilationEvent symbolEvent, AnalysisScope analysisScope, bool isGeneratedCodeSymbol, CancellationToken cancellationToken)
	{
		ISymbol symbol = symbolEvent.Symbol;
		if (!analysisScope.ShouldAnalyze(symbolEvent, s_getTopmostNodeForAnalysis, cancellationToken))
		{
			return;
		}
		foreach (var (analyzer, immutableArray) in _lazySymbolActionsByKind)
		{
			if (analysisScope.Contains(analyzer) && (int)symbol.Kind < immutableArray.Length)
			{
				AnalyzerExecutor.ExecuteSymbolActions(immutableArray[(int)symbol.Kind], analyzer, symbolEvent, s_getTopmostNodeForAnalysis, isGeneratedCodeSymbol, analysisScope.FilterFileOpt?.SourceTree, analysisScope.FilterSpanOpt, cancellationToken);
			}
		}
	}

	private bool TryExecuteSymbolEndActions(in AnalyzerActions perSymbolActions, SymbolDeclaredCompilationEvent symbolEvent, AnalysisScope analysisScope, bool isGeneratedCodeSymbol, CancellationToken cancellationToken, out ImmutableArray<DiagnosticAnalyzer> subsetProcessedAnalyzers)
	{
		ISymbol symbol = symbolEvent.Symbol;
		ImmutableArray<SymbolEndAnalyzerAction> symbolEndActions = perSymbolActions.SymbolEndActions;
		if (symbolEndActions.IsEmpty || !analysisScope.ShouldAnalyze(symbolEvent, s_getTopmostNodeForAnalysis, cancellationToken))
		{
			subsetProcessedAnalyzers = ImmutableArray<DiagnosticAnalyzer>.Empty;
			return true;
		}
		bool flag = true;
		ArrayBuilder<DiagnosticAnalyzer> arrayBuilder = s_diagnosticAnalyzerPool.Allocate();
		PooledHashSet<DiagnosticAnalyzer> instance = PooledHashSet<DiagnosticAnalyzer>.GetInstance();
		try
		{
			foreach (IGrouping<DiagnosticAnalyzer, SymbolEndAnalyzerAction> item in from a in symbolEndActions
				group a by a.Analyzer)
			{
				DiagnosticAnalyzer key = item.Key;
				if (analysisScope.Contains(key))
				{
					instance.Add(key);
					ImmutableArray<SymbolEndAnalyzerAction> symbolEndActions2 = item.ToImmutableArrayOrEmpty();
					if (!symbolEndActions2.IsEmpty && !AnalyzerExecutor.TryExecuteSymbolEndActions(symbolEndActions2, key, symbolEvent, s_getTopmostNodeForAnalysis, isGeneratedCodeSymbol, analysisScope.OriginalFilterFile?.SourceTree, analysisScope.OriginalFilterSpan, cancellationToken))
					{
						flag = false;
						continue;
					}
					AnalyzerManager.MarkSymbolEndAnalysisComplete(symbol, key);
					arrayBuilder.Add(key);
				}
			}
			if (instance.Count < analysisScope.Analyzers.Length)
			{
				foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
				{
					if (!instance.Contains(analyzer))
					{
						AnalyzerManager.MarkSymbolEndAnalysisComplete(symbol, analyzer);
						arrayBuilder.Add(analyzer);
					}
				}
			}
			if (!flag)
			{
				subsetProcessedAnalyzers = arrayBuilder.ToImmutable();
				return false;
			}
			subsetProcessedAnalyzers = ImmutableArray<DiagnosticAnalyzer>.Empty;
			return true;
		}
		finally
		{
			instance.Free();
			arrayBuilder.Clear();
			s_diagnosticAnalyzerPool.Free(arrayBuilder);
		}
	}

	private static SyntaxNode GetTopmostNodeForAnalysis(ISymbol symbol, SyntaxReference syntaxReference, Compilation compilation, CancellationToken cancellationToken)
	{
		return compilation.GetSemanticModel(syntaxReference.SyntaxTree).GetTopmostNodeForDiagnosticAnalysis(symbol, syntaxReference.GetSyntax(cancellationToken));
	}

	protected abstract void ExecuteDeclaringReferenceActions(SymbolDeclaredCompilationEvent symbolEvent, AnalysisScope analysisScope, bool isGeneratedCodeSymbol, IGroupedAnalyzerActions additionalPerSymbolActions, CancellationToken cancellationToken);

	private void ProcessCompilationUnitCompleted(CompilationUnitCompletedEvent completedEvent, AnalysisScope analysisScope, CancellationToken cancellationToken)
	{
		SemanticModel orCreateSemanticModel = GetOrCreateSemanticModel(completedEvent.CompilationUnit, completedEvent.Compilation);
		if (!analysisScope.ShouldAnalyze(orCreateSemanticModel.SyntaxTree))
		{
			return;
		}
		bool flag = IsGeneratedCode(orCreateSemanticModel.SyntaxTree, cancellationToken);
		if (flag && DoNotAnalyzeGeneratedCode)
		{
			return;
		}
		foreach (var (analyzer, semanticModelActions) in _lazySemanticModelActions)
		{
			if (analysisScope.Contains(analyzer))
			{
				AnalyzerExecutor.ExecuteSemanticModelActions(semanticModelActions, analyzer, orCreateSemanticModel, analysisScope.FilterSpanOpt, flag, cancellationToken);
			}
		}
	}

	private void ProcessCompilationStarted(CompilationStartedEvent startedEvent, AnalysisScope analysisScope, CancellationToken cancellationToken)
	{
		ExecuteCompilationActions(_lazyCompilationActions, startedEvent, analysisScope, cancellationToken);
	}

	private void ProcessCompilationCompleted(CompilationCompletedEvent endEvent, AnalysisScope analysisScope, CancellationToken cancellationToken)
	{
		ExecuteCompilationActions(_lazyCompilationEndActions, endEvent, analysisScope, cancellationToken);
	}

	private void ExecuteCompilationActions(ImmutableArray<(DiagnosticAnalyzer, ImmutableArray<CompilationAnalyzerAction>)> compilationActionsMap, CompilationEvent compilationEvent, AnalysisScope analysisScope, CancellationToken cancellationToken)
	{
		foreach (var (analyzer, compilationActions) in compilationActionsMap)
		{
			if (analysisScope.Contains(analyzer))
			{
				AnalyzerExecutor.ExecuteCompilationActions(compilationActions, analyzer, compilationEvent, cancellationToken);
			}
		}
	}

	internal static Action<Diagnostic, AnalyzerOptions, CancellationToken> GetDiagnosticSink(Action<Diagnostic> addDiagnosticCore, Compilation compilation, SeverityFilter severityFilter, ConcurrentSet<string>? suppressedDiagnosticIds)
	{
		return delegate(Diagnostic diagnostic, AnalyzerOptions analyzerOptions, CancellationToken cancellationToken)
		{
			Diagnostic filteredDiagnostic = GetFilteredDiagnostic(diagnostic, compilation, analyzerOptions, severityFilter, suppressedDiagnosticIds, cancellationToken);
			if (filteredDiagnostic != null)
			{
				addDiagnosticCore(filteredDiagnostic);
			}
		};
	}

	internal static Action<Diagnostic, DiagnosticAnalyzer, AnalyzerOptions, bool, CancellationToken> GetDiagnosticSink(Action<Diagnostic, DiagnosticAnalyzer, bool> addLocalDiagnosticCore, Compilation compilation, SeverityFilter severityFilter, ConcurrentSet<string>? suppressedDiagnosticIds)
	{
		return delegate(Diagnostic diagnostic, DiagnosticAnalyzer analyzer, AnalyzerOptions analyzerOptions, bool isSyntaxDiagnostic, CancellationToken cancellationToken)
		{
			Diagnostic filteredDiagnostic = GetFilteredDiagnostic(diagnostic, compilation, analyzerOptions, severityFilter, suppressedDiagnosticIds, cancellationToken);
			if (filteredDiagnostic != null)
			{
				addLocalDiagnosticCore(filteredDiagnostic, analyzer, isSyntaxDiagnostic);
			}
		};
	}

	internal static Action<Diagnostic, DiagnosticAnalyzer, AnalyzerOptions?, CancellationToken> GetDiagnosticSink(Action<Diagnostic, DiagnosticAnalyzer> addDiagnosticCore, Compilation compilation, SeverityFilter severityFilter, ConcurrentSet<string>? suppressedDiagnosticIds)
	{
		return delegate(Diagnostic diagnostic, DiagnosticAnalyzer analyzer, AnalyzerOptions? analyzerOptions, CancellationToken cancellationToken)
		{
			Diagnostic filteredDiagnostic = GetFilteredDiagnostic(diagnostic, compilation, analyzerOptions, severityFilter, suppressedDiagnosticIds, cancellationToken);
			if (filteredDiagnostic != null)
			{
				addDiagnosticCore(filteredDiagnostic, analyzer);
			}
		};
	}

	private static Diagnostic? GetFilteredDiagnostic(Diagnostic diagnostic, Compilation compilation, AnalyzerOptions? analyzerOptions, SeverityFilter severityFilter, ConcurrentSet<string>? suppressedDiagnosticIds, CancellationToken cancellationToken)
	{
		Diagnostic? diagnostic2 = applyFurtherFiltering(compilation.Options.FilterDiagnostic(diagnostic, cancellationToken));
		if (diagnostic2 == null)
		{
			suppressedDiagnosticIds?.Add(diagnostic.Id);
		}
		return diagnostic2;
		Diagnostic? applyFurtherFiltering(Diagnostic? diagnostic3)
		{
			SyntaxTree syntaxTree = diagnostic3?.Location.SourceTree;
			if (syntaxTree != null && analyzerOptions.TryGetSeverityFromBulkConfiguration(syntaxTree, compilation, diagnostic3.Descriptor, cancellationToken, out var severity))
			{
				diagnostic3 = diagnostic3.WithReportDiagnostic(severity);
			}
			if (diagnostic3 != null && severityFilter.Contains(DiagnosticDescriptor.MapSeverityToReport(diagnostic3.Severity)))
			{
				return null;
			}
			return diagnostic3;
		}
	}

	private static async Task<(AnalyzerActions actions, ImmutableHashSet<DiagnosticAnalyzer> unsuppressedAnalyzers)> GetAnalyzerActionsAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerManager analyzerManager, AnalyzerExecutor analyzerExecutor, AnalysisScope analysisScope, SeverityFilter severityFilter, CancellationToken cancellationToken)
	{
		AnalyzerActions allAnalyzerActions = AnalyzerActions.Empty;
		PooledHashSet<DiagnosticAnalyzer> unsuppressedAnalyzersBuilder = PooledHashSet<DiagnosticAnalyzer>.GetInstance();
		foreach (DiagnosticAnalyzer item2 in analyzers)
		{
			if (!IsDiagnosticAnalyzerSuppressed(item2, analyzerExecutor.Compilation.Options, analyzerManager, analyzerExecutor, analysisScope, severityFilter, cancellationToken))
			{
				unsuppressedAnalyzersBuilder.Add(item2);
				allAnalyzerActions = allAnalyzerActions.Append(await analyzerManager.GetAnalyzerActionsAsync(item2, analyzerExecutor, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
			}
		}
		ImmutableHashSet<DiagnosticAnalyzer> item = unsuppressedAnalyzersBuilder.ToImmutableHashSet();
		unsuppressedAnalyzersBuilder.Free();
		return (actions: allAnalyzerActions, unsuppressedAnalyzers: item);
	}

	public bool HasSymbolStartedActions(AnalysisScope analysisScope)
	{
		if (AnalyzerActions.SymbolStartActionsCount == 0)
		{
			return false;
		}
		if (analysisScope.Analyzers.Length == Analyzers.Length)
		{
			return true;
		}
		if (analysisScope.Analyzers.Length == 1)
		{
			DiagnosticAnalyzer diagnosticAnalyzer = analysisScope.Analyzers[0];
			foreach (SymbolStartAnalyzerAction symbolStartAction in AnalyzerActions.SymbolStartActions)
			{
				if (symbolStartAction.Analyzer == diagnosticAnalyzer)
				{
					return true;
				}
			}
			return false;
		}
		PooledHashSet<DiagnosticAnalyzer> instance = PooledHashSet<DiagnosticAnalyzer>.GetInstance();
		try
		{
			foreach (SymbolStartAnalyzerAction symbolStartAction2 in AnalyzerActions.SymbolStartActions)
			{
				instance.Add(symbolStartAction2.Analyzer);
			}
			foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
			{
				if (instance.Contains(analyzer))
				{
					return true;
				}
			}
			return false;
		}
		finally
		{
			instance.Free();
		}
	}

	private async ValueTask<IGroupedAnalyzerActions> GetPerSymbolAnalyzerActionsAsync(ISymbol symbol, AnalysisScope analysisScope, CancellationToken cancellationToken)
	{
		if (AnalyzerActions.SymbolStartActionsCount == 0 || symbol.IsImplicitlyDeclared)
		{
			return EmptyGroupedActions;
		}
		IGroupedAnalyzerActions allActions = EmptyGroupedActions;
		foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
		{
			if (SymbolStartAnalyzers.Contains(analyzer))
			{
				IGroupedAnalyzerActions groupedAnalyzerActions = await GetPerSymbolAnalyzerActionsAsync(symbol, analyzer, analysisScope.OriginalFilterFile?.SourceTree, analysisScope.OriginalFilterSpan, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (!groupedAnalyzerActions.IsEmpty)
				{
					allActions = allActions.Append(groupedAnalyzerActions);
				}
			}
		}
		return allActions;
	}

	private async ValueTask<IGroupedAnalyzerActions> GetPerSymbolAnalyzerActionsAsync(ISymbol symbol, DiagnosticAnalyzer analyzer, SyntaxTree? filterTree, TextSpan? filterSpan, CancellationToken cancellationToken)
	{
		if (symbol.IsImplicitlyDeclared)
		{
			return EmptyGroupedActions;
		}
		if (!(symbol is INamespaceOrTypeSymbol namespaceOrType))
		{
			return await getAllActionsAsync(this, symbol, analyzer, filterTree, filterSpan, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (PerSymbolAnalyzerActionsCache.TryGetValue((namespaceOrType, analyzer), out IGroupedAnalyzerActions value))
		{
			return value;
		}
		IGroupedAnalyzerActions value2 = await getAllActionsAsync(this, symbol, analyzer, filterTree, filterSpan, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return PerSymbolAnalyzerActionsCache.GetOrAdd((namespaceOrType, analyzer), value2);
		async ValueTask<IGroupedAnalyzerActions> getAllActionsAsync(AnalyzerDriver driver, ISymbol symbol2, DiagnosticAnalyzer analyzer2, SyntaxTree? filterTree2, TextSpan? filterSpan2, CancellationToken cancellationToken2)
		{
			IGroupedAnalyzerActions inheritedActions = await getInheritedActionsAsync(driver, symbol2, analyzer2, filterTree2, filterSpan2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			AnalyzerActions otherActions = await getSymbolActionsCoreAsync(driver, symbol2, analyzer2, filterTree2, filterSpan2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			if (otherActions.IsEmpty)
			{
				return inheritedActions;
			}
			return CreateGroupedActions(analyzer2, inheritedActions.AnalyzerActions.Append(in otherActions));
		}
		async ValueTask<IGroupedAnalyzerActions> getInheritedActionsAsync(AnalyzerDriver driver, ISymbol symbol2, DiagnosticAnalyzer analyzer2, SyntaxTree? filterTree2, TextSpan? filterSpan2, CancellationToken cancellationToken2)
		{
			if (symbol2.ContainingSymbol != null)
			{
				IGroupedAnalyzerActions groupedAnalyzerActions = await driver.GetPerSymbolAnalyzerActionsAsync(symbol2.ContainingSymbol, analyzer2, filterTree2, filterSpan2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				if (!groupedAnalyzerActions.IsEmpty && symbol2.ContainingSymbol.Kind != symbol2.Kind)
				{
					return CreateGroupedActions(analyzer2, AnalyzerActions.Empty.Append(groupedAnalyzerActions.AnalyzerActions, appendSymbolStartAndSymbolEndActions: false));
				}
			}
			return EmptyGroupedActions;
		}
		static async ValueTask<AnalyzerActions> getSymbolActionsCoreAsync(AnalyzerDriver driver, ISymbol symbol2, DiagnosticAnalyzer diagnosticAnalyzer, SyntaxTree? filterTree2, TextSpan? filterSpan2, CancellationToken cancellationToken2)
		{
			if (!driver.UnsuppressedAnalyzers.Contains(diagnosticAnalyzer))
			{
				return AnalyzerActions.Empty;
			}
			bool flag = driver.IsGeneratedCodeSymbol(symbol2, cancellationToken2);
			if (flag && driver.ShouldSkipAnalysisOnGeneratedCode(diagnosticAnalyzer))
			{
				return AnalyzerActions.Empty;
			}
			return await driver.AnalyzerManager.GetPerSymbolAnalyzerActionsAsync(symbol2, flag, filterTree2, filterSpan2, diagnosticAnalyzer, driver.AnalyzerExecutor, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private static async Task<ImmutableSegmentedDictionary<DiagnosticAnalyzer, SemaphoreSlim>> CreateAnalyzerGateMapAsync(ImmutableHashSet<DiagnosticAnalyzer> analyzers, AnalyzerManager analyzerManager, AnalyzerExecutor analyzerExecutor, AnalysisScope analysisScope, SeverityFilter severityFilter, CancellationToken cancellationToken)
	{
		ImmutableSegmentedDictionary<DiagnosticAnalyzer, SemaphoreSlim>.Builder builder = ImmutableSegmentedDictionary.CreateBuilder<DiagnosticAnalyzer, SemaphoreSlim>();
		foreach (DiagnosticAnalyzer analyzer in analyzers)
		{
			if (!(await analyzerManager.IsConcurrentAnalyzerAsync(analyzer, analyzerExecutor, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
			{
				SemaphoreSlim value = new SemaphoreSlim(1);
				builder.Add(analyzer, value);
			}
		}
		return builder.ToImmutable();
	}

	private static async Task<ImmutableSegmentedDictionary<DiagnosticAnalyzer, GeneratedCodeAnalysisFlags>> CreateGeneratedCodeAnalysisFlagsMapAsync(ImmutableHashSet<DiagnosticAnalyzer> analyzers, AnalyzerManager analyzerManager, AnalyzerExecutor analyzerExecutor, AnalysisScope analysisScope, SeverityFilter severityFilter, CancellationToken cancellationToken)
	{
		ImmutableSegmentedDictionary<DiagnosticAnalyzer, GeneratedCodeAnalysisFlags>.Builder builder = ImmutableSegmentedDictionary.CreateBuilder<DiagnosticAnalyzer, GeneratedCodeAnalysisFlags>();
		foreach (DiagnosticAnalyzer analyzer in analyzers)
		{
			builder.Add(analyzer, await analyzerManager.GetGeneratedCodeAnalysisFlagsAsync(analyzer, analyzerExecutor, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
		}
		return builder.ToImmutable();
	}

	private bool IsGeneratedCodeSymbol(ISymbol symbol, CancellationToken cancellationToken)
	{
		if (!IsGeneratedCodeSymbolMap.TryGetValue(symbol, out var value))
		{
			return IsGeneratedCodeSymbolMap.GetOrAdd(symbol, computeIsGeneratedCodeSymbol());
		}
		return value;
		bool computeIsGeneratedCodeSymbol()
		{
			if (_lazyGeneratedCodeAttribute != null && GeneratedCodeUtilities.IsGeneratedSymbolWithGeneratedCodeAttribute(symbol, _lazyGeneratedCodeAttribute))
			{
				return true;
			}
			foreach (SyntaxReference declaringSyntaxReference in symbol.DeclaringSyntaxReferences)
			{
				if (!IsGeneratedOrHiddenCodeLocation(declaringSyntaxReference.SyntaxTree, declaringSyntaxReference.Span, cancellationToken))
				{
					return false;
				}
			}
			return true;
		}
	}

	protected bool IsGeneratedCode(SyntaxTree tree, CancellationToken cancellationToken)
	{
		if (!GeneratedCodeFilesMap.TryGetValue(tree, out var value))
		{
			value = computeIsGeneratedCode();
			GeneratedCodeFilesMap.TryAdd(tree, value);
		}
		return value;
		bool computeIsGeneratedCode()
		{
			return GeneratedCodeUtilities.GetGeneratedCodeKindFromOptions(AnalyzerExecutor.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(tree)).ToNullable() ?? _isGeneratedCode(tree, cancellationToken);
		}
	}

	protected bool IsGeneratedOrHiddenCodeLocation(SyntaxTree syntaxTree, TextSpan span, CancellationToken cancellationToken)
	{
		if (!IsGeneratedCode(syntaxTree, cancellationToken))
		{
			return IsHiddenSourceLocation(syntaxTree, span);
		}
		return true;
	}

	protected bool IsHiddenSourceLocation(SyntaxTree syntaxTree, TextSpan span)
	{
		if (HasHiddenRegions(syntaxTree))
		{
			return syntaxTree.IsHiddenPosition(span.Start);
		}
		return false;
	}

	private bool HasHiddenRegions(SyntaxTree tree)
	{
		if (_lazyTreesWithHiddenRegionsMap == null)
		{
			return false;
		}
		if (!_lazyTreesWithHiddenRegionsMap.TryGetValue(tree, out var value))
		{
			value = tree.HasHiddenRegions();
			_lazyTreesWithHiddenRegionsMap.TryAdd(tree, value);
		}
		return value;
	}

	internal async Task<AnalyzerActionCounts> GetAnalyzerActionCountsAsync(DiagnosticAnalyzer analyzer, CompilationOptions compilationOptions, AnalysisScope analysisScope, CancellationToken cancellationToken)
	{
		if (IsDiagnosticAnalyzerSuppressed(analyzer, compilationOptions, AnalyzerManager, AnalyzerExecutor, analysisScope, _severityFilter, cancellationToken))
		{
			return AnalyzerActionCounts.Empty;
		}
		AnalyzerActions analyzerActions = await AnalyzerManager.GetAnalyzerActionsAsync(analyzer, AnalyzerExecutor, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (analyzerActions.IsEmpty)
		{
			return AnalyzerActionCounts.Empty;
		}
		return new AnalyzerActionCounts(in analyzerActions);
	}

	internal static bool IsDiagnosticAnalyzerSuppressed(DiagnosticAnalyzer analyzer, CompilationOptions options, AnalyzerManager analyzerManager, AnalyzerExecutor analyzerExecutor, AnalysisScope analysisScope, SeverityFilter severityFilter, CancellationToken cancellationToken)
	{
		return analyzerManager.IsDiagnosticAnalyzerSuppressed(analyzer, options, s_IsCompilerAnalyzerFunc, analyzerExecutor, analysisScope, severityFilter, cancellationToken);
	}

	internal static bool IsCompilerAnalyzer(DiagnosticAnalyzer analyzer)
	{
		return analyzer is CompilerDiagnosticAnalyzer;
	}

	public void Dispose()
	{
		_lazyCompilationEventQueue?.TryComplete();
		_lazyDiagnosticQueue?.TryComplete();
		_lazyQueueRegistration?.Dispose();
	}

	protected abstract IGroupedAnalyzerActions CreateGroupedActions(DiagnosticAnalyzer analyzer, in AnalyzerActions analyzerActions);
}
internal class AnalyzerDriver<TLanguageKindEnum> : AnalyzerDriver where TLanguageKindEnum : struct
{
	[StructLayout(LayoutKind.Auto)]
	private struct ExecutableCodeBlockAnalyzerActions
	{
		public DiagnosticAnalyzer Analyzer;

		public ImmutableArray<CodeBlockStartAnalyzerAction<TLanguageKindEnum>> CodeBlockStartActions;

		public ImmutableArray<CodeBlockAnalyzerAction> CodeBlockActions;

		public ImmutableArray<CodeBlockAnalyzerAction> CodeBlockEndActions;

		public ImmutableArray<OperationBlockStartAnalyzerAction> OperationBlockStartActions;

		public ImmutableArray<OperationBlockAnalyzerAction> OperationBlockActions;

		public ImmutableArray<OperationBlockAnalyzerAction> OperationBlockEndActions;
	}

	private sealed class GroupedAnalyzerActions : IGroupedAnalyzerActions
	{
		public static readonly GroupedAnalyzerActions Empty = new GroupedAnalyzerActions(ImmutableArray<(DiagnosticAnalyzer, GroupedAnalyzerActionsForAnalyzer)>.Empty, ImmutableSegmentedDictionary<TLanguageKindEnum, ImmutableArray<DiagnosticAnalyzer>>.Empty, in AnalyzerActions.Empty);

		public ImmutableArray<(DiagnosticAnalyzer analyzer, GroupedAnalyzerActionsForAnalyzer groupedActions)> GroupedActionsByAnalyzer { get; }

		public ImmutableSegmentedDictionary<TLanguageKindEnum, ImmutableArray<DiagnosticAnalyzer>> AnalyzersByKind { get; }

		public AnalyzerActions AnalyzerActions { get; }

		public bool IsEmpty => this == Empty;

		private GroupedAnalyzerActions(ImmutableArray<(DiagnosticAnalyzer, GroupedAnalyzerActionsForAnalyzer)> groupedActionsAndAnalyzers, ImmutableSegmentedDictionary<TLanguageKindEnum, ImmutableArray<DiagnosticAnalyzer>> analyzersByKind, in AnalyzerActions analyzerActions)
		{
			GroupedActionsByAnalyzer = groupedActionsAndAnalyzers;
			AnalyzerActions = analyzerActions;
			AnalyzersByKind = analyzersByKind;
		}

		public static GroupedAnalyzerActions Create(DiagnosticAnalyzer analyzer, in AnalyzerActions analyzerActions)
		{
			if (analyzerActions.IsEmpty)
			{
				return Empty;
			}
			GroupedAnalyzerActionsForAnalyzer item = new GroupedAnalyzerActionsForAnalyzer(analyzer, in analyzerActions, analyzerActionsNeedFiltering: false);
			ImmutableArray<(DiagnosticAnalyzer, GroupedAnalyzerActionsForAnalyzer)> groupedActionsAndAnalyzers = ImmutableArray<(DiagnosticAnalyzer, GroupedAnalyzerActionsForAnalyzer)>.Empty.Add((analyzer, item));
			ImmutableSegmentedDictionary<TLanguageKindEnum, ImmutableArray<DiagnosticAnalyzer>> analyzersByKind = CreateAnalyzersByKind(groupedActionsAndAnalyzers);
			return new GroupedAnalyzerActions(groupedActionsAndAnalyzers, analyzersByKind, in analyzerActions);
		}

		public static GroupedAnalyzerActions Create(ImmutableArray<DiagnosticAnalyzer> analyzers, in AnalyzerActions analyzerActions)
		{
			ImmutableArray<(DiagnosticAnalyzer analyzer, GroupedAnalyzerActionsForAnalyzer)> groupedActionsAndAnalyzers = analyzers.SelectAsArray((DiagnosticAnalyzer analyzer, AnalyzerActions analyzerActions2) => (analyzer: analyzer, new GroupedAnalyzerActionsForAnalyzer(analyzer, in analyzerActions2, analyzerActionsNeedFiltering: true)), analyzerActions);
			ImmutableSegmentedDictionary<TLanguageKindEnum, ImmutableArray<DiagnosticAnalyzer>> analyzersByKind = CreateAnalyzersByKind(groupedActionsAndAnalyzers);
			return new GroupedAnalyzerActions(groupedActionsAndAnalyzers, analyzersByKind, in analyzerActions);
		}

		IGroupedAnalyzerActions IGroupedAnalyzerActions.Append(IGroupedAnalyzerActions igroupedAnalyzerActions)
		{
			GroupedAnalyzerActions groupedAnalyzerActions = (GroupedAnalyzerActions)igroupedAnalyzerActions;
			ImmutableArray<(DiagnosticAnalyzer analyzer, GroupedAnalyzerActionsForAnalyzer groupedActions)> groupedActionsAndAnalyzers = GroupedActionsByAnalyzer.AddRange(groupedAnalyzerActions.GroupedActionsByAnalyzer);
			AnalyzerActions analyzerActions = AnalyzerActions.Append(groupedAnalyzerActions.AnalyzerActions);
			ImmutableSegmentedDictionary<TLanguageKindEnum, ImmutableArray<DiagnosticAnalyzer>> analyzersByKind = CreateAnalyzersByKind(groupedActionsAndAnalyzers);
			return new GroupedAnalyzerActions(groupedActionsAndAnalyzers, analyzersByKind, in analyzerActions);
		}

		private static ImmutableSegmentedDictionary<TLanguageKindEnum, ImmutableArray<DiagnosticAnalyzer>> CreateAnalyzersByKind(ImmutableArray<(DiagnosticAnalyzer, GroupedAnalyzerActionsForAnalyzer)> groupedActionsAndAnalyzers)
		{
			PooledDictionary<TLanguageKindEnum, ArrayBuilder<DiagnosticAnalyzer>> instance = PooledDictionary<TLanguageKindEnum, ArrayBuilder<DiagnosticAnalyzer>>.GetInstance();
			foreach (var item in groupedActionsAndAnalyzers)
			{
				var (value, _) = item;
				foreach (var (key, _) in item.Item2.NodeActionsByAnalyzerAndKind)
				{
					instance.AddPooled(key, value);
				}
			}
			return instance.ToImmutableSegmentedDictionaryAndFree();
		}
	}

	private sealed class GroupedAnalyzerActionsForAnalyzer
	{
		private readonly DiagnosticAnalyzer _analyzer;

		private readonly bool _analyzerActionsNeedFiltering;

		private ImmutableSegmentedDictionary<TLanguageKindEnum, ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>> _lazyNodeActionsByKind;

		private ImmutableSegmentedDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> _lazyOperationActionsByKind;

		private ImmutableArray<CodeBlockStartAnalyzerAction<TLanguageKindEnum>> _lazyCodeBlockStartActions;

		private ImmutableArray<CodeBlockAnalyzerAction> _lazyCodeBlockEndActions;

		private ImmutableArray<CodeBlockAnalyzerAction> _lazyCodeBlockActions;

		private ImmutableArray<OperationBlockStartAnalyzerAction> _lazyOperationBlockStartActions;

		private ImmutableArray<OperationBlockAnalyzerAction> _lazyOperationBlockActions;

		private ImmutableArray<OperationBlockAnalyzerAction> _lazyOperationBlockEndActions;

		public AnalyzerActions AnalyzerActions { get; }

		public ImmutableSegmentedDictionary<TLanguageKindEnum, ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>> NodeActionsByAnalyzerAndKind
		{
			get
			{
				if (_lazyNodeActionsByKind == null)
				{
					ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>> instance = ArrayBuilder<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>.GetInstance();
					if (_analyzerActionsNeedFiltering)
					{
						AnalyzerActions.AddSyntaxNodeActions(_analyzer, instance);
					}
					else
					{
						AnalyzerActions.AddSyntaxNodeActions(instance);
					}
					RoslynImmutableInterlocked.InterlockedInitialize(ref _lazyNodeActionsByKind, Microsoft.CodeAnalysis.Diagnostics.AnalyzerExecutor.GetNodeActionsByKind(instance));
					instance.Free();
				}
				return _lazyNodeActionsByKind;
			}
		}

		public ImmutableSegmentedDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> OperationActionsByAnalyzerAndKind
		{
			get
			{
				if (_lazyOperationActionsByKind == null)
				{
					ArrayBuilder<OperationAnalyzerAction> instance = ArrayBuilder<OperationAnalyzerAction>.GetInstance();
					AddFilteredActions(AnalyzerActions.OperationActions, instance);
					RoslynImmutableInterlocked.InterlockedInitialize(ref _lazyOperationActionsByKind, Microsoft.CodeAnalysis.Diagnostics.AnalyzerExecutor.GetOperationActionsByKind(instance));
					instance.Free();
				}
				return _lazyOperationActionsByKind;
			}
		}

		private ImmutableArray<CodeBlockStartAnalyzerAction<TLanguageKindEnum>> CodeBlockStartActions
		{
			get
			{
				if (_lazyCodeBlockStartActions.IsDefault)
				{
					ArrayBuilder<CodeBlockStartAnalyzerAction<TLanguageKindEnum>> instance = ArrayBuilder<CodeBlockStartAnalyzerAction<TLanguageKindEnum>>.GetInstance();
					AddFilteredActions(AnalyzerActions.GetCodeBlockStartActions<TLanguageKindEnum>(), instance);
					ImmutableInterlocked.InterlockedInitialize(ref _lazyCodeBlockStartActions, instance.ToImmutableAndFree());
				}
				return _lazyCodeBlockStartActions;
			}
		}

		private ImmutableArray<CodeBlockAnalyzerAction> CodeBlockEndActions => GetExecutableCodeActions(ref _lazyCodeBlockEndActions, AnalyzerActions.CodeBlockEndActions, _analyzer, _analyzerActionsNeedFiltering);

		private ImmutableArray<CodeBlockAnalyzerAction> CodeBlockActions => GetExecutableCodeActions(ref _lazyCodeBlockActions, AnalyzerActions.CodeBlockActions, _analyzer, _analyzerActionsNeedFiltering);

		private ImmutableArray<OperationBlockStartAnalyzerAction> OperationBlockStartActions => GetExecutableCodeActions(ref _lazyOperationBlockStartActions, AnalyzerActions.OperationBlockStartActions, _analyzer, _analyzerActionsNeedFiltering);

		private ImmutableArray<OperationBlockAnalyzerAction> OperationBlockEndActions => GetExecutableCodeActions(ref _lazyOperationBlockEndActions, AnalyzerActions.OperationBlockEndActions, _analyzer, _analyzerActionsNeedFiltering);

		private ImmutableArray<OperationBlockAnalyzerAction> OperationBlockActions => GetExecutableCodeActions(ref _lazyOperationBlockActions, AnalyzerActions.OperationBlockActions, _analyzer, _analyzerActionsNeedFiltering);

		public bool HasCodeBlockStartActions => !CodeBlockStartActions.IsEmpty;

		public bool HasOperationBlockStartActions => !OperationBlockStartActions.IsEmpty;

		public GroupedAnalyzerActionsForAnalyzer(DiagnosticAnalyzer analyzer, in AnalyzerActions analyzerActions, bool analyzerActionsNeedFiltering)
		{
			_analyzer = analyzer;
			AnalyzerActions = analyzerActions;
			_analyzerActionsNeedFiltering = analyzerActionsNeedFiltering;
		}

		[Conditional("DEBUG")]
		private static void VerifyActions<TAnalyzerAction>(ArrayBuilder<TAnalyzerAction> actions, DiagnosticAnalyzer analyzer) where TAnalyzerAction : AnalyzerAction
		{
			foreach (TAnalyzerAction action in actions)
			{
				_ = action;
			}
		}

		private void AddFilteredActions<TAnalyzerAction>(ImmutableArray<TAnalyzerAction> actions, ArrayBuilder<TAnalyzerAction> builder) where TAnalyzerAction : AnalyzerAction
		{
			AddFilteredActions(in actions, _analyzer, _analyzerActionsNeedFiltering, builder);
		}

		private static void AddFilteredActions<TAnalyzerAction>(in ImmutableArray<TAnalyzerAction> actions, DiagnosticAnalyzer analyzer, bool analyzerActionsNeedFiltering, ArrayBuilder<TAnalyzerAction> builder) where TAnalyzerAction : AnalyzerAction
		{
			if (!analyzerActionsNeedFiltering)
			{
				builder.AddRange(actions);
				return;
			}
			foreach (TAnalyzerAction action in actions)
			{
				if (action.Analyzer == analyzer)
				{
					builder.Add(action);
				}
			}
		}

		private static ImmutableArray<ActionType> GetExecutableCodeActions<ActionType>(ref ImmutableArray<ActionType> lazyCodeBlockActions, ImmutableArray<ActionType> codeBlockActions, DiagnosticAnalyzer analyzer, bool analyzerActionsNeedFiltering) where ActionType : AnalyzerAction
		{
			if (lazyCodeBlockActions.IsDefault)
			{
				ArrayBuilder<ActionType> instance = ArrayBuilder<ActionType>.GetInstance();
				AddFilteredActions(in codeBlockActions, analyzer, analyzerActionsNeedFiltering, instance);
				ImmutableInterlocked.InterlockedInitialize(ref lazyCodeBlockActions, instance.ToImmutableAndFree());
			}
			return lazyCodeBlockActions;
		}

		public bool TryGetExecutableCodeBlockActions(out ExecutableCodeBlockAnalyzerActions actions)
		{
			if (!OperationBlockStartActions.IsEmpty || !OperationBlockActions.IsEmpty || !OperationBlockEndActions.IsEmpty || !CodeBlockStartActions.IsEmpty || !CodeBlockActions.IsEmpty || !CodeBlockEndActions.IsEmpty)
			{
				actions = new ExecutableCodeBlockAnalyzerActions
				{
					Analyzer = _analyzer,
					CodeBlockStartActions = CodeBlockStartActions,
					CodeBlockActions = CodeBlockActions,
					CodeBlockEndActions = CodeBlockEndActions,
					OperationBlockStartActions = OperationBlockStartActions,
					OperationBlockActions = OperationBlockActions,
					OperationBlockEndActions = OperationBlockEndActions
				};
				return true;
			}
			actions = default(ExecutableCodeBlockAnalyzerActions);
			return false;
		}
	}

	private readonly Func<SyntaxNode, TLanguageKindEnum> _getKind;

	private GroupedAnalyzerActions? _lazyCoreActions;

	protected override IGroupedAnalyzerActions EmptyGroupedActions => GroupedAnalyzerActions.Empty;

	internal AnalyzerDriver(ImmutableArray<DiagnosticAnalyzer> analyzers, Func<SyntaxNode, TLanguageKindEnum> getKind, AnalyzerManager analyzerManager, SeverityFilter severityFilter, Func<SyntaxTrivia, bool> isComment)
		: base(analyzers, analyzerManager, severityFilter, isComment)
	{
		_getKind = getKind;
	}

	private GroupedAnalyzerActions GetOrCreateCoreActions()
	{
		if (_lazyCoreActions == null)
		{
			Interlocked.CompareExchange(ref _lazyCoreActions, createCoreActions(), null);
		}
		return _lazyCoreActions;
		GroupedAnalyzerActions createCoreActions()
		{
			if (base.AnalyzerActions.IsEmpty)
			{
				return GroupedAnalyzerActions.Empty;
			}
			return GroupedAnalyzerActions.Create(base.Analyzers.WhereAsArray(base.UnsuppressedAnalyzers.Contains), in base.AnalyzerActions);
		}
	}

	private static void ComputeShouldExecuteActions(in AnalyzerActions coreActions, in AnalyzerActions additionalActions, ISymbol symbol, out bool executeSyntaxNodeActions, out bool executeCodeBlockActions, out bool executeOperationActions, out bool executeOperationBlockActions)
	{
		executeSyntaxNodeActions = false;
		executeCodeBlockActions = false;
		executeOperationActions = false;
		executeOperationBlockActions = false;
		bool canHaveExecutableCodeBlock = Microsoft.CodeAnalysis.Diagnostics.AnalyzerExecutor.CanHaveExecutableCodeBlock(symbol);
		computeShouldExecuteActions(coreActions, canHaveExecutableCodeBlock, ref executeSyntaxNodeActions, ref executeCodeBlockActions, ref executeOperationActions, ref executeOperationBlockActions);
		computeShouldExecuteActions(additionalActions, canHaveExecutableCodeBlock, ref executeSyntaxNodeActions, ref executeCodeBlockActions, ref executeOperationActions, ref executeOperationBlockActions);
		static void computeShouldExecuteActions(AnalyzerActions analyzerActions, bool flag, ref bool reference, ref bool reference3, ref bool reference2, ref bool reference4)
		{
			if (!analyzerActions.IsEmpty)
			{
				reference |= analyzerActions.SyntaxNodeActionsCount > 0;
				reference2 |= analyzerActions.OperationActionsCount > 0;
				if (flag)
				{
					reference3 |= analyzerActions.CodeBlockStartActionsCount > 0 || analyzerActions.CodeBlockActionsCount > 0;
					reference4 |= analyzerActions.OperationBlockStartActionsCount > 0 || analyzerActions.OperationBlockActionsCount > 0;
				}
			}
		}
	}

	protected override IGroupedAnalyzerActions CreateGroupedActions(DiagnosticAnalyzer analyzer, in AnalyzerActions analyzerActions)
	{
		return GroupedAnalyzerActions.Create(analyzer, in analyzerActions);
	}

	protected override void ExecuteDeclaringReferenceActions(SymbolDeclaredCompilationEvent symbolEvent, AnalysisScope analysisScope, bool isGeneratedCodeSymbol, IGroupedAnalyzerActions additionalPerSymbolActions, CancellationToken cancellationToken)
	{
		ISymbol symbol = symbolEvent.Symbol;
		ComputeShouldExecuteActions(in base.AnalyzerActions, additionalPerSymbolActions.AnalyzerActions, symbol, out var executeSyntaxNodeActions, out var executeCodeBlockActions, out var executeOperationActions, out var executeOperationBlockActions);
		if (!(executeSyntaxNodeActions | executeOperationActions | executeCodeBlockActions | executeOperationBlockActions))
		{
			return;
		}
		ImmutableArray<SyntaxReference> declaringSyntaxReferences = symbolEvent.DeclaringSyntaxReferences;
		GroupedAnalyzerActions orCreateCoreActions = GetOrCreateCoreActions();
		foreach (SyntaxReference item in declaringSyntaxReferences)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (!analysisScope.FilterFileOpt.HasValue || analysisScope.FilterFileOpt?.SourceTree == item.SyntaxTree)
			{
				bool flag = isGeneratedCodeSymbol || IsGeneratedOrHiddenCodeLocation(item.SyntaxTree, item.Span, cancellationToken);
				if (!flag || !base.DoNotAnalyzeGeneratedCode)
				{
					ExecuteDeclaringReferenceActions(item, symbolEvent, analysisScope, orCreateCoreActions, (GroupedAnalyzerActions)additionalPerSymbolActions, executeSyntaxNodeActions, executeOperationActions, executeCodeBlockActions, executeOperationBlockActions, flag, cancellationToken);
				}
			}
		}
	}

	private static DeclarationAnalysisData ComputeDeclarationAnalysisData(ISymbol symbol, SyntaxReference declaration, SemanticModel semanticModel, AnalysisScope analysisScope, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		ArrayBuilder<DeclarationInfo> instance = ArrayBuilder<DeclarationInfo>.GetInstance();
		SyntaxNode syntax = declaration.GetSyntax(cancellationToken);
		SyntaxNode topmostNodeForDiagnosticAnalysis = semanticModel.GetTopmostNodeForDiagnosticAnalysis(symbol, syntax);
		ComputeDeclarationsInNode(semanticModel, symbol, syntax, topmostNodeForDiagnosticAnalysis, instance, cancellationToken);
		ImmutableArray<DeclarationInfo> immutableArray = instance.ToImmutableAndFree();
		bool isPartialAnalysis = analysisScope.FilterSpanOpt.HasValue && !analysisScope.ContainsSpan(topmostNodeForDiagnosticAnalysis.FullSpan);
		DeclarationAnalysisData result = new DeclarationAnalysisData(syntax, topmostNodeForDiagnosticAnalysis, immutableArray, isPartialAnalysis);
		AddSyntaxNodesToAnalyze(topmostNodeForDiagnosticAnalysis, symbol, immutableArray, semanticModel, result.DescendantNodesToAnalyze, cancellationToken);
		return result;
	}

	private static void ComputeDeclarationsInNode(SemanticModel semanticModel, ISymbol declaredSymbol, SyntaxNode declaringReferenceSyntax, SyntaxNode topmostNodeForAnalysis, ArrayBuilder<DeclarationInfo> builder, CancellationToken cancellationToken)
	{
		int? levelsToCompute = 2;
		bool getSymbol = topmostNodeForAnalysis != declaringReferenceSyntax || declaredSymbol.Kind == SymbolKind.Namespace;
		semanticModel.ComputeDeclarationsInNode(topmostNodeForAnalysis, declaredSymbol, getSymbol, builder, cancellationToken, levelsToCompute);
	}

	private void ExecuteDeclaringReferenceActions(SyntaxReference decl, SymbolDeclaredCompilationEvent symbolEvent, AnalysisScope analysisScope, GroupedAnalyzerActions coreActions, GroupedAnalyzerActions additionalPerSymbolActions, bool shouldExecuteSyntaxNodeActions, bool shouldExecuteOperationActions, bool shouldExecuteCodeBlockActions, bool shouldExecuteOperationBlockActions, bool isInGeneratedCode, CancellationToken cancellationToken)
	{
		ISymbol symbol = symbolEvent.Symbol;
		SemanticModel semanticModel = symbolEvent.SemanticModelWithCachedBoundNodes ?? GetOrCreateSemanticModel(decl.SyntaxTree, symbolEvent.Compilation);
		DeclarationAnalysisData declarationAnalysisData = ComputeDeclarationAnalysisData(symbol, decl, semanticModel, analysisScope, cancellationToken);
		if (analysisScope.ShouldAnalyze(declarationAnalysisData.TopmostNodeForAnalysis))
		{
			executeNodeActions();
			executeExecutableCodeActions();
		}
		declarationAnalysisData.Free();
		static void addExecutableCodeBlockAnalyzerActions(GroupedAnalyzerActions groupedActions, AnalysisScope analysisScope2, ArrayBuilder<ExecutableCodeBlockAnalyzerActions> builder)
		{
			foreach (var (analyzer, groupedAnalyzerActionsForAnalyzer) in groupedActions.GroupedActionsByAnalyzer)
			{
				if (analysisScope2.Contains(analyzer) && groupedAnalyzerActionsForAnalyzer.TryGetExecutableCodeBlockActions(out var actions))
				{
					builder.Add(actions);
				}
			}
		}
		void executeCodeBlockActions(ImmutableArray<SyntaxNode> executableCodeBlocks, ArrayBuilder<ExecutableCodeBlockAnalyzerActions> codeBlockActions)
		{
			if (!executableCodeBlocks.IsEmpty && shouldExecuteCodeBlockActions)
			{
				foreach (ExecutableCodeBlockAnalyzerActions codeBlockAction in codeBlockActions)
				{
					if ((!codeBlockAction.CodeBlockStartActions.IsEmpty || !codeBlockAction.CodeBlockActions.IsEmpty || !codeBlockAction.CodeBlockEndActions.IsEmpty) && analysisScope.Contains(codeBlockAction.Analyzer))
					{
						base.AnalyzerExecutor.ExecuteCodeBlockActions(codeBlockAction.CodeBlockStartActions, codeBlockAction.CodeBlockActions, codeBlockAction.CodeBlockEndActions, codeBlockAction.Analyzer, declarationAnalysisData.TopmostNodeForAnalysis, symbol, executableCodeBlocks, semanticModel, _getKind, analysisScope.FilterSpanOpt, isInGeneratedCode, cancellationToken);
					}
				}
			}
		}
		void executeExecutableCodeActions()
		{
			if (!shouldExecuteCodeBlockActions && !shouldExecuteOperationActions && !shouldExecuteOperationBlockActions)
			{
				return;
			}
			ImmutableArray<SyntaxNode> immutableArray = ImmutableArray<SyntaxNode>.Empty;
			ArrayBuilder<ExecutableCodeBlockAnalyzerActions> instance = ArrayBuilder<ExecutableCodeBlockAnalyzerActions>.GetInstance();
			try
			{
				foreach (DeclarationInfo item3 in declarationAnalysisData.DeclarationsInNode)
				{
					if (item3.DeclaredNode == declarationAnalysisData.TopmostNodeForAnalysis || item3.DeclaredNode == declarationAnalysisData.DeclaringReferenceSyntax)
					{
						immutableArray = item3.ExecutableCodeBlocks;
						if (!immutableArray.IsEmpty)
						{
							if (shouldExecuteCodeBlockActions | shouldExecuteOperationBlockActions)
							{
								addExecutableCodeBlockAnalyzerActions(coreActions, analysisScope, instance);
								addExecutableCodeBlockAnalyzerActions(additionalPerSymbolActions, analysisScope, instance);
							}
							if (shouldExecuteOperationActions | shouldExecuteOperationBlockActions)
							{
								ImmutableArray<IOperation> operationBlocksToAnalyze = GetOperationBlocksToAnalyze(immutableArray, semanticModel, cancellationToken);
								ImmutableArray<IOperation> operationsToAnalyze = getOperationsToAnalyzeWithStackGuard(operationBlocksToAnalyze);
								if (!operationsToAnalyze.IsEmpty)
								{
									try
									{
										executeOperationsActions(operationsToAnalyze);
										executeOperationsBlockActions(operationBlocksToAnalyze, operationsToAnalyze, instance);
									}
									finally
									{
										base.AnalyzerExecutor.OnOperationBlockActionsExecuted(operationBlocksToAnalyze);
									}
								}
							}
							break;
						}
					}
				}
				executeCodeBlockActions(immutableArray, instance);
			}
			finally
			{
				instance.Free();
			}
		}
		void executeNodeActions()
		{
			if (shouldExecuteSyntaxNodeActions)
			{
				ArrayBuilder<SyntaxNode> descendantNodesToAnalyze = declarationAnalysisData.DescendantNodesToAnalyze;
				executeNodeActionsByKind(descendantNodesToAnalyze, coreActions, arePerSymbolActions: false);
				executeNodeActionsByKind(descendantNodesToAnalyze, additionalPerSymbolActions, arePerSymbolActions: true);
			}
		}
		void executeNodeActionsByKind(ArrayBuilder<SyntaxNode> nodesToAnalyze, GroupedAnalyzerActions groupedActions, bool arePerSymbolActions)
		{
			if (groupedActions.GroupedActionsByAnalyzer.Length != 0)
			{
				PooledHashSet<DiagnosticAnalyzer> instance = PooledHashSet<DiagnosticAnalyzer>.GetInstance();
				foreach (SyntaxNode item4 in nodesToAnalyze)
				{
					if (groupedActions.AnalyzersByKind.TryGetValue(_getKind(item4), out ImmutableArray<DiagnosticAnalyzer> value))
					{
						foreach (DiagnosticAnalyzer item5 in value)
						{
							instance.Add(item5);
						}
					}
				}
				foreach (var (diagnosticAnalyzer, groupedAnalyzerActionsForAnalyzer) in groupedActions.GroupedActionsByAnalyzer)
				{
					if (instance.Contains(diagnosticAnalyzer) && analysisScope.Contains(diagnosticAnalyzer))
					{
						if (declarationAnalysisData.IsPartialAnalysis && !groupedAnalyzerActionsForAnalyzer.HasCodeBlockStartActions)
						{
							ArrayBuilder<SyntaxNode> instance2 = ArrayBuilder<SyntaxNode>.GetInstance(nodesToAnalyze.Count);
							foreach (SyntaxNode item6 in nodesToAnalyze)
							{
								if (analysisScope.ShouldAnalyze(item6))
								{
									instance2.Add(item6);
								}
							}
							executeSyntaxNodeActions(diagnosticAnalyzer, groupedAnalyzerActionsForAnalyzer, instance2);
							instance2.Free();
						}
						else
						{
							executeSyntaxNodeActions(diagnosticAnalyzer, groupedAnalyzerActionsForAnalyzer, nodesToAnalyze);
						}
					}
				}
				instance.Free();
			}
		}
		void executeOperationsActions(ImmutableArray<IOperation> operationsToAnalyze)
		{
			if (shouldExecuteOperationActions)
			{
				executeOperationsActionsByKind(operationsToAnalyze, coreActions, arePerSymbolActions: false);
				executeOperationsActionsByKind(operationsToAnalyze, additionalPerSymbolActions, arePerSymbolActions: true);
			}
		}
		void executeOperationsActionsByKind(ImmutableArray<IOperation> operationsToAnalyze, GroupedAnalyzerActions groupedActions, bool arePerSymbolActions)
		{
			foreach (var item7 in groupedActions.GroupedActionsByAnalyzer)
			{
				DiagnosticAnalyzer item = item7.analyzer;
				GroupedAnalyzerActionsForAnalyzer item2 = item7.groupedActions;
				ImmutableSegmentedDictionary<OperationKind, ImmutableArray<OperationAnalyzerAction>> operationActionsByAnalyzerAndKind = item2.OperationActionsByAnalyzerAndKind;
				if (!operationActionsByAnalyzerAndKind.IsEmpty && analysisScope.Contains(item))
				{
					ImmutableArray<IOperation> operationsToAnalyze2 = ((declarationAnalysisData.IsPartialAnalysis && !item2.HasOperationBlockStartActions) ? operationsToAnalyze.WhereAsArray((IOperation operation) => analysisScope.ShouldAnalyze(operation.Syntax)) : operationsToAnalyze);
					base.AnalyzerExecutor.ExecuteOperationActions(operationsToAnalyze2, operationActionsByAnalyzerAndKind, item, semanticModel, declarationAnalysisData.TopmostNodeForAnalysis.FullSpan, symbol, analysisScope.FilterSpanOpt, isInGeneratedCode, item2.HasOperationBlockStartActions | arePerSymbolActions, cancellationToken);
				}
			}
		}
		void executeOperationsBlockActions(ImmutableArray<IOperation> operationBlocksToAnalyze, ImmutableArray<IOperation> operationsToAnalyze, ArrayBuilder<ExecutableCodeBlockAnalyzerActions> codeBlockActions)
		{
			if (shouldExecuteOperationBlockActions)
			{
				foreach (ExecutableCodeBlockAnalyzerActions codeBlockAction2 in codeBlockActions)
				{
					if ((!codeBlockAction2.OperationBlockStartActions.IsEmpty || !codeBlockAction2.OperationBlockActions.IsEmpty || !codeBlockAction2.OperationBlockEndActions.IsEmpty) && analysisScope.Contains(codeBlockAction2.Analyzer))
					{
						base.AnalyzerExecutor.ExecuteOperationBlockActions(codeBlockAction2.OperationBlockStartActions, codeBlockAction2.OperationBlockActions, codeBlockAction2.OperationBlockEndActions, codeBlockAction2.Analyzer, declarationAnalysisData.TopmostNodeForAnalysis, symbol, operationBlocksToAnalyze, operationsToAnalyze, semanticModel, analysisScope.FilterSpanOpt, isInGeneratedCode, cancellationToken);
					}
				}
			}
		}
		void executeSyntaxNodeActions(DiagnosticAnalyzer analyzer, GroupedAnalyzerActionsForAnalyzer groupedActionsForAnalyzer, ArrayBuilder<SyntaxNode> filteredNodesToAnalyze)
		{
			base.AnalyzerExecutor.ExecuteSyntaxNodeActions(filteredNodesToAnalyze, groupedActionsForAnalyzer.NodeActionsByAnalyzerAndKind, analyzer, semanticModel, _getKind, declarationAnalysisData.TopmostNodeForAnalysis.FullSpan, symbol, analysisScope.FilterSpanOpt, isInGeneratedCode, groupedActionsForAnalyzer.HasCodeBlockStartActions | P_3.arePerSymbolActions, cancellationToken);
		}
		ImmutableArray<IOperation> getOperationsToAnalyzeWithStackGuard(ImmutableArray<IOperation> operationBlocksToAnalyze)
		{
			try
			{
				return GetOperationsToAnalyze(operationBlocksToAnalyze);
			}
			catch (Exception ex) when (ex is InsufficientExecutionStackException)
			{
				Diagnostic arg = Microsoft.CodeAnalysis.Diagnostics.AnalyzerExecutor.CreateDriverExceptionDiagnostic(ex);
				DiagnosticAnalyzer arg2 = base.Analyzers[0];
				base.AnalyzerExecutor.OnAnalyzerException(ex, arg2, arg, cancellationToken);
				return ImmutableArray<IOperation>.Empty;
			}
		}
	}

	private static void AddSyntaxNodesToAnalyze(SyntaxNode declaredNode, ISymbol declaredSymbol, ImmutableArray<DeclarationInfo> declarationsInNode, SemanticModel semanticModel, ArrayBuilder<SyntaxNode> nodesToAnalyze, CancellationToken cancellationToken)
	{
		HashSet<SyntaxNode> descendantDeclsToSkip = null;
		bool flag = true;
		foreach (DeclarationInfo item2 in declarationsInNode)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (item2.DeclaredNode != declaredNode)
			{
				if (IsEquivalentSymbol(declaredSymbol, item2.DeclaredSymbol))
				{
					if (flag)
					{
						break;
					}
					return;
				}
				SyntaxNode item = item2.DeclaredNode;
				ISymbol symbol = item2.DeclaredSymbol ?? semanticModel.GetDeclaredSymbol(item2.DeclaredNode, cancellationToken);
				if (symbol != null)
				{
					item = semanticModel.GetTopmostNodeForDiagnosticAnalysis(symbol, item2.DeclaredNode);
				}
				if (descendantDeclsToSkip == null)
				{
					descendantDeclsToSkip = new HashSet<SyntaxNode>();
				}
				descendantDeclsToSkip.Add(item);
			}
			flag = false;
		}
		Func<SyntaxNode, bool> additionalFilter = semanticModel.GetSyntaxNodesToAnalyzeFilter(declaredNode, declaredSymbol);
		foreach (SyntaxNode item3 in declaredNode.DescendantNodesAndSelf(shouldAddNode, descendIntoTrivia: true))
		{
			if (shouldAddNode(item3) && !semanticModel.ShouldSkipSyntaxNodeAnalysis(item3, declaredSymbol))
			{
				nodesToAnalyze.Add(item3);
			}
		}
		bool shouldAddNode(SyntaxNode node)
		{
			if (descendantDeclsToSkip == null || !descendantDeclsToSkip.Contains(node))
			{
				if (additionalFilter != null)
				{
					return additionalFilter(node);
				}
				return true;
			}
			return false;
		}
	}

	private static bool IsEquivalentSymbol(ISymbol declaredSymbol, ISymbol? otherSymbol)
	{
		if (declaredSymbol.Equals(otherSymbol))
		{
			return true;
		}
		if (otherSymbol != null && declaredSymbol.Kind == SymbolKind.Namespace && otherSymbol.Kind == SymbolKind.Namespace && declaredSymbol.Name == otherSymbol.Name)
		{
			return declaredSymbol.ToDisplayString() == otherSymbol.ToDisplayString();
		}
		return false;
	}

	private static ImmutableArray<IOperation> GetOperationBlocksToAnalyze(ImmutableArray<SyntaxNode> executableBlocks, SemanticModel semanticModel, CancellationToken cancellationToken)
	{
		ArrayBuilder<IOperation> instance = ArrayBuilder<IOperation>.GetInstance();
		foreach (SyntaxNode item in executableBlocks)
		{
			IOperation operation = semanticModel.GetOperation(item, cancellationToken);
			if (operation != null)
			{
				instance.AddRange(operation);
			}
		}
		return instance.ToImmutableAndFree();
	}

	private static ImmutableArray<IOperation> GetOperationsToAnalyze(ImmutableArray<IOperation> operationBlocks)
	{
		ArrayBuilder<IOperation> instance = ArrayBuilder<IOperation>.GetInstance();
		bool flag = true;
		foreach (IOperation item in operationBlocks)
		{
			if (flag && item.Parent != null)
			{
				switch (item.Parent.Kind)
				{
				case OperationKind.MethodBody:
				case OperationKind.ConstructorBody:
					instance.Add(item.Parent);
					break;
				case OperationKind.ExpressionStatement:
					instance.Add(item.Parent.Parent);
					break;
				}
				flag = false;
			}
			instance.AddRange(item.DescendantsAndSelf());
		}
		return instance.ToImmutableAndFree();
	}
}
