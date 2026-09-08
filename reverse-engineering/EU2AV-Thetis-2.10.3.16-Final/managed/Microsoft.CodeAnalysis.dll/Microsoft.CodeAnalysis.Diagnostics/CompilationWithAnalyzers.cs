using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics.Telemetry;
using Microsoft.CodeAnalysis.ErrorReporting;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Diagnostics;

public class CompilationWithAnalyzers
{
	private readonly Compilation _compilation;

	private readonly AnalysisScope _compilationAnalysisScope;

	private readonly ImmutableArray<DiagnosticAnalyzer> _analyzers;

	private readonly ImmutableArray<DiagnosticSuppressor> _suppressors;

	private readonly CompilationWithAnalyzersOptions _analysisOptions;

	private readonly AnalysisResultBuilder _analysisResultBuilder;

	private readonly ConcurrentSet<Diagnostic> _exceptionDiagnostics = new ConcurrentSet<Diagnostic>();

	private static readonly AsyncQueue<CompilationEvent> s_EmptyEventQueue = new AsyncQueue<CompilationEvent>();

	public Compilation Compilation => _compilation;

	public ImmutableArray<DiagnosticAnalyzer> Analyzers => _analyzers;

	public CompilationWithAnalyzersOptions AnalysisOptions => _analysisOptions;

	[Obsolete("This CancellationToken is always 'None'", false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public CancellationToken CancellationToken => CancellationToken.None;

	private ImmutableArray<AdditionalText> AdditionalFiles => _analysisOptions.Options.GetAdditionalFiles();

	[Obsolete("Use constructor without a cancellation token")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public CompilationWithAnalyzers(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerOptions? options, CancellationToken cancellationToken)
		: this(compilation, analyzers, options)
	{
	}

	public CompilationWithAnalyzers(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerOptions? options)
		: this(compilation, analyzers, new CompilationWithAnalyzersOptions(options, null, concurrentAnalysis: true, logAnalyzerExecutionTime: true, reportSuppressedDiagnostics: false, null))
	{
	}

	public CompilationWithAnalyzers(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, CompilationWithAnalyzersOptions analysisOptions)
	{
		VerifyArguments(compilation, analyzers, analysisOptions);
		compilation = compilation.WithOptions(compilation.Options.WithReportSuppressedDiagnostics(analysisOptions.ReportSuppressedDiagnostics)).WithSemanticModelProvider(CachingSemanticModelProvider.Instance).WithEventQueue(new AsyncQueue<CompilationEvent>());
		_compilation = compilation;
		_analyzers = analyzers;
		_suppressors = analyzers.OfType<DiagnosticSuppressor>().ToImmutableArrayOrEmpty();
		_analysisOptions = analysisOptions;
		_analysisResultBuilder = new AnalysisResultBuilder(analysisOptions.LogAnalyzerExecutionTime, analyzers, _analysisOptions.Options.GetAdditionalFiles());
		_compilationAnalysisScope = AnalysisScope.Create(_compilation, _analyzers, this);
	}

	private static void VerifyArguments(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, CompilationWithAnalyzersOptions analysisOptions)
	{
		if (compilation == null)
		{
			throw new ArgumentNullException("compilation");
		}
		if (analysisOptions == null)
		{
			throw new ArgumentNullException("analysisOptions");
		}
		VerifyAnalyzersArgumentForStaticApis(analyzers);
	}

	private static void VerifyAnalyzersArgumentForStaticApis(ImmutableArray<DiagnosticAnalyzer> analyzers, bool allowDefaultOrEmpty = false)
	{
		if (analyzers.IsDefaultOrEmpty)
		{
			if (!allowDefaultOrEmpty)
			{
				throw new ArgumentException(CodeAnalysisResources.ArgumentCannotBeEmpty, "analyzers");
			}
			return;
		}
		if (analyzers.Any((DiagnosticAnalyzer a) => a == null))
		{
			throw new ArgumentException(CodeAnalysisResources.ArgumentElementCannotBeNull, "analyzers");
		}
		if (analyzers.HasDuplicates())
		{
			throw new ArgumentException(CodeAnalysisResources.DuplicateAnalyzerInstances, "analyzers");
		}
	}

	private void VerifyAnalyzerArgument(DiagnosticAnalyzer analyzer)
	{
		VerifyAnalyzerArgumentForStaticApis(analyzer);
		if (!_analyzers.Contains(analyzer))
		{
			throw new ArgumentException(CodeAnalysisResources.UnsupportedAnalyzerInstance, "analyzer");
		}
	}

	private static void VerifyAnalyzerArgumentForStaticApis(DiagnosticAnalyzer analyzer)
	{
		if (analyzer == null)
		{
			throw new ArgumentException(CodeAnalysisResources.ArgumentCannotBeEmpty, "analyzer");
		}
	}

	private void VerifyExistingAnalyzersArgument(ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		VerifyAnalyzersArgumentForStaticApis(analyzers);
		if (analyzers.Any((DiagnosticAnalyzer a, CompilationWithAnalyzers self) => !self._analyzers.Contains(a), this))
		{
			throw new ArgumentException(CodeAnalysisResources.UnsupportedAnalyzerInstance, "_analyzers");
		}
		if (analyzers.HasDuplicates())
		{
			throw new ArgumentException(CodeAnalysisResources.DuplicateAnalyzerInstances, "analyzers");
		}
	}

	private void VerifyModel(SemanticModel model)
	{
		if (model == null)
		{
			throw new ArgumentNullException("model");
		}
		if (!_compilation.ContainsSyntaxTree(model.SyntaxTree))
		{
			throw new ArgumentException(CodeAnalysisResources.InvalidTree, "model");
		}
	}

	private void VerifyTree(SyntaxTree tree)
	{
		if (tree == null)
		{
			throw new ArgumentNullException("tree");
		}
		if (!_compilation.ContainsSyntaxTree(tree))
		{
			throw new ArgumentException(CodeAnalysisResources.InvalidTree, "tree");
		}
	}

	private void VerifyAdditionalFile(AdditionalText file)
	{
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		if (!AdditionalFiles.Contains(file))
		{
			throw new ArgumentException(CodeAnalysisResources.InvalidAdditionalFile, "file");
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync()
	{
		return GetAnalyzerDiagnosticsAsync(CancellationToken.None);
	}

	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return await GetAnalyzerDiagnosticsCoreAsync(Analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		VerifyExistingAnalyzersArgument(analyzers);
		return await GetAnalyzerDiagnosticsCoreAsync(analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<AnalysisResult> GetAnalysisResultAsync(CancellationToken cancellationToken)
	{
		return await GetAnalysisResultCoreAsync(Analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<AnalysisResult> GetAnalysisResultAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		VerifyExistingAnalyzersArgument(analyzers);
		return await GetAnalysisResultCoreAsync(analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public Task<ImmutableArray<Diagnostic>> GetAllDiagnosticsAsync()
	{
		return GetAllDiagnosticsAsync(CancellationToken.None);
	}

	public async Task<ImmutableArray<Diagnostic>> GetAllDiagnosticsAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return (await getAllDiagnosticsWithoutStateTrackingAsync(Analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).AddRange(_exceptionDiagnostics);
		async Task<ImmutableArray<Diagnostic>> getAllDiagnosticsWithoutStateTrackingAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken2)
		{
			Compilation compilation = _compilation.WithEventQueue(new AsyncQueue<CompilationEvent>());
			AnalysisScope analysisScope = AnalysisScope.Create(compilation, analyzers, this);
			using AnalyzerDriver driver = await CreateAndInitializeDriverAsync(compilation, _analysisOptions, analysisScope, _suppressors, categorizeDiagnostics: false, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			driver.AttachQueueAndStartProcessingEvents(compilation.EventQueue, analysisScope, usingPrePopulatedEventQueue: false, cancellationToken2);
			ImmutableArray<Diagnostic> reportedDiagnostics = compilation.GetDiagnostics(cancellationToken2).AddRange(await driver.GetDiagnosticsAsync(compilation, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false));
			return driver.ApplyProgrammaticSuppressionsAndFilterDiagnostics(reportedDiagnostics, compilation, cancellationToken2);
		}
	}

	[Obsolete("This API was found to have performance issues and hence has been deprecated. Instead, invoke the API 'GetAnalysisResultAsync' and access the property 'CompilationDiagnostics' on the returned 'AnalysisResult' to fetch the compilation diagnostics.")]
	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerCompilationDiagnosticsAsync(CancellationToken cancellationToken)
	{
		return await GetAnalyzerCompilationDiagnosticsCoreAsync(Analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	[Obsolete("This API was found to have performance issues and hence has been deprecated. Instead, invoke the API 'GetAnalysisResultAsync' and access the property 'CompilationDiagnostics' on the returned 'AnalysisResult' to fetch the compilation diagnostics.")]
	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerCompilationDiagnosticsAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		VerifyExistingAnalyzersArgument(analyzers);
		return await GetAnalyzerCompilationDiagnosticsCoreAsync(analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<ImmutableArray<Diagnostic>> GetAnalyzerCompilationDiagnosticsCoreAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		AnalysisScope analysisScope = _compilationAnalysisScope.WithAnalyzers(analyzers, this);
		await ComputeAnalyzerDiagnosticsAsync(analysisScope, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return _analysisResultBuilder.GetDiagnostics(analysisScope, getLocalDiagnostics: false, getNonLocalDiagnostics: true);
	}

	private async Task<AnalysisResult> GetAnalysisResultCoreAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		AnalysisScope analysisScope = _compilationAnalysisScope.WithAnalyzers(analyzers, this);
		await ComputeAnalyzerDiagnosticsAsync(analysisScope, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return _analysisResultBuilder.ToAnalysisResult(analyzers, analysisScope, cancellationToken);
	}

	private async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsCoreAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		AnalysisScope analysisScope = _compilationAnalysisScope.WithAnalyzers(analyzers, this);
		await ComputeAnalyzerDiagnosticsAsync(analysisScope, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return _analysisResultBuilder.GetDiagnostics(analysisScope, getLocalDiagnostics: true, getNonLocalDiagnostics: true);
	}

	private static async Task<AnalyzerDriver> CreateAndInitializeDriverAsync(Compilation compilation, CompilationWithAnalyzersOptions analysisOptions, AnalysisScope analysisScope, ImmutableArray<DiagnosticSuppressor> suppressors, bool categorizeDiagnostics, CancellationToken cancellationToken)
	{
		ImmutableArray<DiagnosticAnalyzer> analyzers = analysisScope.Analyzers;
		if (!suppressors.IsEmpty)
		{
			ImmutableHashSet<DiagnosticSuppressor> suppressorsInAnalysisScope = analysisScope.Analyzers.OfType<DiagnosticSuppressor>().ToImmutableHashSet();
			analyzers = analyzers.AddRange(suppressors.Where((DiagnosticSuppressor suppressor) => !suppressorsInAnalysisScope.Contains(suppressor)));
		}
		AnalyzerDriver driver = compilation.CreateAnalyzerDriver(analyzers, new AnalyzerManager(analyzers), SeverityFilter.None);
		driver.Initialize(compilation, analysisOptions, new AnalyzerDriver.CompilationData(compilation), analysisScope, categorizeDiagnostics, trackSuppressedDiagnosticIds: false, cancellationToken);
		cancellationToken.ThrowIfCancellationRequested();
		await driver.WhenInitializedTask.ConfigureAwait(continueOnCapturedContext: false);
		return driver;
	}

	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerSyntaxDiagnosticsAsync(SyntaxTree tree, CancellationToken cancellationToken)
	{
		VerifyTree(tree);
		return await GetAnalyzerSyntaxDiagnosticsCoreAsync(tree, Analyzers, null, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerSyntaxDiagnosticsAsync(SyntaxTree tree, TextSpan? filterSpan, CancellationToken cancellationToken)
	{
		VerifyTree(tree);
		return await GetAnalyzerSyntaxDiagnosticsCoreAsync(tree, Analyzers, filterSpan, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerSyntaxDiagnosticsAsync(SyntaxTree tree, ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		VerifyTree(tree);
		VerifyExistingAnalyzersArgument(analyzers);
		return await GetAnalyzerSyntaxDiagnosticsCoreAsync(tree, analyzers, null, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerSyntaxDiagnosticsAsync(SyntaxTree tree, TextSpan? filterSpan, ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		VerifyTree(tree);
		VerifyExistingAnalyzersArgument(analyzers);
		return await GetAnalyzerSyntaxDiagnosticsCoreAsync(tree, analyzers, filterSpan, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public Task<AnalysisResult> GetAnalysisResultAsync(SyntaxTree tree, CancellationToken cancellationToken)
	{
		VerifyTree(tree);
		return GetAnalysisResultCoreAsync(new SourceOrAdditionalFile(tree), Analyzers, null, cancellationToken);
	}

	public Task<AnalysisResult> GetAnalysisResultAsync(SyntaxTree tree, TextSpan? filterSpan, CancellationToken cancellationToken)
	{
		VerifyTree(tree);
		return GetAnalysisResultCoreAsync(new SourceOrAdditionalFile(tree), Analyzers, filterSpan, cancellationToken);
	}

	public Task<AnalysisResult> GetAnalysisResultAsync(SyntaxTree tree, ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		VerifyTree(tree);
		VerifyExistingAnalyzersArgument(analyzers);
		return GetAnalysisResultCoreAsync(new SourceOrAdditionalFile(tree), analyzers, null, cancellationToken);
	}

	public Task<AnalysisResult> GetAnalysisResultAsync(SyntaxTree tree, TextSpan? filterSpan, ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		VerifyTree(tree);
		VerifyExistingAnalyzersArgument(analyzers);
		return GetAnalysisResultCoreAsync(new SourceOrAdditionalFile(tree), analyzers, filterSpan, cancellationToken);
	}

	public async Task<AnalysisResult> GetAnalysisResultAsync(AdditionalText file, CancellationToken cancellationToken)
	{
		VerifyAdditionalFile(file);
		return await GetAnalysisResultCoreAsync(new SourceOrAdditionalFile(file), Analyzers, null, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<AnalysisResult> GetAnalysisResultAsync(AdditionalText file, ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		VerifyAdditionalFile(file);
		VerifyExistingAnalyzersArgument(analyzers);
		return await GetAnalysisResultCoreAsync(new SourceOrAdditionalFile(file), analyzers, null, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<AnalysisResult> GetAnalysisResultAsync(AdditionalText file, TextSpan? filterSpan, CancellationToken cancellationToken)
	{
		VerifyAdditionalFile(file);
		return await GetAnalysisResultCoreAsync(new SourceOrAdditionalFile(file), Analyzers, filterSpan, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<AnalysisResult> GetAnalysisResultAsync(AdditionalText file, TextSpan? filterSpan, ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		VerifyAdditionalFile(file);
		VerifyExistingAnalyzersArgument(analyzers);
		return await GetAnalysisResultCoreAsync(new SourceOrAdditionalFile(file), analyzers, filterSpan, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<AnalysisResult> GetAnalysisResultCoreAsync(SourceOrAdditionalFile file, ImmutableArray<DiagnosticAnalyzer> analyzers, TextSpan? filterSpan, CancellationToken cancellationToken)
	{
		AnalysisScope analysisScope = AnalysisScope.Create(analyzers, file, filterSpan, isSyntacticSingleFileAnalysis: true, this);
		await ComputeAnalyzerDiagnosticsAsync(analysisScope, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return _analysisResultBuilder.ToAnalysisResult(analyzers, analysisScope, cancellationToken);
	}

	private async Task<ImmutableArray<Diagnostic>> GetAnalyzerSyntaxDiagnosticsCoreAsync(SyntaxTree tree, ImmutableArray<DiagnosticAnalyzer> analyzers, TextSpan? filterSpan, CancellationToken cancellationToken)
	{
		AnalysisScope analysisScope = AnalysisScope.Create(analyzers, new SourceOrAdditionalFile(tree), filterSpan, isSyntacticSingleFileAnalysis: true, this);
		await ComputeAnalyzerDiagnosticsAsync(analysisScope, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return _analysisResultBuilder.GetDiagnostics(analysisScope, getLocalDiagnostics: true, getNonLocalDiagnostics: false);
	}

	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerSemanticDiagnosticsAsync(SemanticModel model, TextSpan? filterSpan, CancellationToken cancellationToken)
	{
		VerifyModel(model);
		return await GetAnalyzerSemanticDiagnosticsCoreAsync(model, filterSpan, Analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ImmutableArray<Diagnostic>> GetAnalyzerSemanticDiagnosticsAsync(SemanticModel model, TextSpan? filterSpan, ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		VerifyModel(model);
		VerifyExistingAnalyzersArgument(analyzers);
		return await GetAnalyzerSemanticDiagnosticsCoreAsync(model, filterSpan, analyzers, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public Task<AnalysisResult> GetAnalysisResultAsync(SemanticModel model, TextSpan? filterSpan, CancellationToken cancellationToken)
	{
		VerifyModel(model);
		return GetAnalysisResultCoreAsync(model, filterSpan, Analyzers, cancellationToken);
	}

	public Task<AnalysisResult> GetAnalysisResultAsync(SemanticModel model, TextSpan? filterSpan, ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		VerifyModel(model);
		VerifyExistingAnalyzersArgument(analyzers);
		return GetAnalysisResultCoreAsync(model, filterSpan, analyzers, cancellationToken);
	}

	private async Task<AnalysisResult> GetAnalysisResultCoreAsync(SemanticModel model, TextSpan? filterSpan, ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		AnalysisScope analysisScope = AnalysisScope.Create(analyzers, new SourceOrAdditionalFile(model.SyntaxTree), filterSpan, isSyntacticSingleFileAnalysis: false, this);
		await ComputeAnalyzerDiagnosticsAsync(analysisScope, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return _analysisResultBuilder.ToAnalysisResult(analyzers, analysisScope, cancellationToken);
	}

	private async Task<ImmutableArray<Diagnostic>> GetAnalyzerSemanticDiagnosticsCoreAsync(SemanticModel model, TextSpan? filterSpan, ImmutableArray<DiagnosticAnalyzer> analyzers, CancellationToken cancellationToken)
	{
		AnalysisScope analysisScope = AnalysisScope.Create(analyzers, new SourceOrAdditionalFile(model.SyntaxTree), filterSpan, isSyntacticSingleFileAnalysis: false, this);
		await ComputeAnalyzerDiagnosticsAsync(analysisScope, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return _analysisResultBuilder.GetDiagnostics(analysisScope, getLocalDiagnostics: true, getNonLocalDiagnostics: false);
	}

	private async Task ComputeAnalyzerDiagnosticsAsync(AnalysisScope? analysisScope, CancellationToken cancellationToken)
	{
		_ = 3;
		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			analysisScope = GetPendingAnalysisScope(analysisScope);
			if (analysisScope == null)
			{
				return;
			}
			Compilation compilation = (analysisScope.IsSingleFileAnalysisForCompilerAnalyzer ? _compilation : _compilation.WithSemanticModelProvider(CachingSemanticModelProvider.Instance).WithEventQueue(new AsyncQueue<CompilationEvent>()));
			using AnalyzerDriver driver = await CreateAndInitializeDriverAsync(compilation, _analysisOptions, analysisScope, _suppressors, categorizeDiagnostics: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			(ImmutableDictionary<DiagnosticAnalyzer, AnalyzerActionCounts>, bool) tuple = await getAnalyzerActionCountsAsync(driver, compilation, analysisScope, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			ImmutableDictionary<DiagnosticAnalyzer, AnalyzerActionCounts> analyzerActionCounts = tuple.Item1;
			bool item = tuple.Item2;
			Func<DiagnosticAnalyzer, AnalyzerActionCounts> getAnalyzerActionCounts = (DiagnosticAnalyzer analyzer) => analyzerActionCounts[analyzer];
			if (!analysisScope.IsSingleFileAnalysis)
			{
				driver.AttachQueueAndStartProcessingEvents(compilation.EventQueue, analysisScope, !item, cancellationToken);
				if (item)
				{
					compilation.GetDiagnostics(cancellationToken);
				}
				await driver.WhenCompletedTask.ConfigureAwait(continueOnCapturedContext: false);
				_analysisResultBuilder.ApplySuppressionsAndStoreAnalysisResult(analysisScope, driver, compilation, getAnalyzerActionCounts, cancellationToken);
				return;
			}
			ImmutableArray<CompilationEvent> compilationEventsForSingleFileAnalysis = GetCompilationEventsForSingleFileAnalysis(compilation, analysisScope, AdditionalFiles, item, cancellationToken);
			ArrayBuilder<(AnalysisScope, ImmutableArray<CompilationEvent>)> builder = ArrayBuilder<(AnalysisScope, ImmutableArray<CompilationEvent>)>.GetInstance();
			builder.Add((analysisScope, compilationEventsForSingleFileAnalysis));
			if (compilationEventsForSingleFileAnalysis.Any((CompilationEvent e) => e is SymbolDeclaredCompilationEvent) && driver.HasSymbolStartedActions(analysisScope))
			{
				var (symbolStartAnalyzers, analyzers) = getSymbolStartAnalyzers(analysisScope.Analyzers, analyzerActionCounts);
				builder.Clear();
				if (!analyzers.IsEmpty)
				{
					AnalysisScope item2 = analysisScope.WithAnalyzers(analyzers, this);
					builder.Add((item2, compilationEventsForSingleFileAnalysis));
				}
				processSymbolStartAnalyzers(analysisScope.FilterFileOpt.Value, analysisScope.FilterSpanOpt, compilationEventsForSingleFileAnalysis, symbolStartAnalyzers, compilation, _analysisResultBuilder, builder, AdditionalFiles, cancellationToken);
			}
			await attachQueueAndProcessAllEventsAsync(builder, driver, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			foreach (var item4 in builder)
			{
				AnalysisScope item3 = item4.Item1;
				_analysisResultBuilder.ApplySuppressionsAndStoreAnalysisResult(item3, driver, compilation, getAnalyzerActionCounts, cancellationToken);
			}
		}
		catch (Exception exception) when (FatalError.ReportAndPropagateUnlessCanceled(exception, cancellationToken))
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/DiagnosticAnalyzer/CompilationWithAnalyzers.cs", 809);
		}
		static async Task attachQueueAndProcessAllEventsAsync(ArrayBuilder<(AnalysisScope, ImmutableArray<CompilationEvent>)> arrayBuilder, AnalyzerDriver analyzerDriver, CancellationToken cancellationToken2)
		{
			foreach (var (analysisScope2, compilationEvents) in arrayBuilder)
			{
				cancellationToken2.ThrowIfCancellationRequested();
				AsyncQueue<CompilationEvent> eventQueue = CreateEventsQueue(compilationEvents);
				await analyzerDriver.AttachQueueAndProcessAllEventsAsync(eventQueue, analysisScope2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		static async Task<(ImmutableDictionary<DiagnosticAnalyzer, AnalyzerActionCounts> analyzerActionCounts, bool hasAnyActionsRequiringCompilationEvents)> getAnalyzerActionCountsAsync(AnalyzerDriver analyzerDriver, Compilation compilation2, AnalysisScope analysisScope2, CancellationToken cancellationToken2)
		{
			ImmutableDictionary<DiagnosticAnalyzer, AnalyzerActionCounts>.Builder builder2 = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, AnalyzerActionCounts>();
			bool hasAnyActionsRequiringCompilationEvents = false;
			foreach (DiagnosticAnalyzer analyzer in analysisScope2.Analyzers)
			{
				AnalyzerActionCounts analyzerActionCounts2 = await analyzerDriver.GetAnalyzerActionCountsAsync(analyzer, compilation2.Options, analysisScope2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				builder2.Add(analyzer, analyzerActionCounts2);
				if (analyzerActionCounts2.HasAnyActionsRequiringCompilationEvents)
				{
					hasAnyActionsRequiringCompilationEvents = true;
				}
			}
			return (analyzerActionCounts: builder2.ToImmutable(), hasAnyActionsRequiringCompilationEvents: hasAnyActionsRequiringCompilationEvents);
		}
		static (ImmutableArray<DiagnosticAnalyzer> symbolStartAnalyzers, ImmutableArray<DiagnosticAnalyzer> otherAnalyzers) getSymbolStartAnalyzers(ImmutableArray<DiagnosticAnalyzer> immutableArray, ImmutableDictionary<DiagnosticAnalyzer, AnalyzerActionCounts> immutableDictionary)
		{
			ArrayBuilder<DiagnosticAnalyzer> instance = ArrayBuilder<DiagnosticAnalyzer>.GetInstance();
			ArrayBuilder<DiagnosticAnalyzer> instance2 = ArrayBuilder<DiagnosticAnalyzer>.GetInstance();
			foreach (DiagnosticAnalyzer item5 in immutableArray)
			{
				if (immutableDictionary[item5].SymbolStartActionsCount > 0)
				{
					instance.Add(item5);
				}
				else
				{
					instance2.Add(item5);
				}
			}
			return (symbolStartAnalyzers: instance.ToImmutableAndFree(), otherAnalyzers: instance2.ToImmutableAndFree());
		}
		void processSymbolStartAnalyzers(SourceOrAdditionalFile originalFile, TextSpan? originalSpan, ImmutableArray<CompilationEvent> compilationEventsForTree, ImmutableArray<DiagnosticAnalyzer> analyzers2, Compilation compilation2, AnalysisResultBuilder analysisResultBuilder, ArrayBuilder<(AnalysisScope, ImmutableArray<CompilationEvent>)> arrayBuilder, ImmutableArray<AdditionalText> additionalFiles, CancellationToken cancellationToken2)
		{
			PooledHashSet<SyntaxTree> instance = PooledHashSet<SyntaxTree>.GetInstance();
			SyntaxTree tree = originalFile.SourceTree;
			instance.Add(tree);
			try
			{
				foreach (CompilationEvent item6 in compilationEventsForTree)
				{
					if (item6 is SymbolDeclaredCompilationEvent symbolDeclaredCompilationEvent && symbolDeclaredCompilationEvent.Symbol.Kind != SymbolKind.Namespace)
					{
						foreach (Location location in symbolDeclaredCompilationEvent.Symbol.Locations)
						{
							if (location.SourceTree != null)
							{
								instance.Add(location.SourceTree);
							}
						}
					}
				}
				foreach (SyntaxTree item7 in instance)
				{
					if (tryProcessTree(item7, out var scopeAndEvents))
					{
						arrayBuilder.Add((scopeAndEvents.Value.Item1, scopeAndEvents.Value.Item2));
					}
				}
			}
			finally
			{
				instance.Free();
			}
			bool tryProcessTree(SyntaxTree partialTree, [NotNullWhen(true)] out (AnalysisScope scope, ImmutableArray<CompilationEvent> events)? reference)
			{
				reference = null;
				AnalysisScope analysisScope2 = AnalysisScope.Create(filterFile: new SourceOrAdditionalFile(partialTree), analyzers: analyzers2, filterSpan: null, originalFilterFile: originalFile, originalFilterSpan: originalSpan, isSyntacticSingleFileAnalysis: false, compilationWithAnalyzers: this);
				analysisScope2 = GetPendingAnalysisScope(analysisScope2);
				if (analysisScope2 == null)
				{
					return false;
				}
				ImmutableArray<CompilationEvent> immutableArray = GetCompilationEventsForSingleFileAnalysis(compilation2, analysisScope2, additionalFiles, hasAnyActionsRequiringCompilationEvents: true, cancellationToken2);
				if (partialTree == tree)
				{
					immutableArray = compilationEventsForTree.AddRange(immutableArray);
					immutableArray = immutableArray.WhereAsArray((CompilationEvent e) => !(e is CompilationUnitCompletedEvent { FilterSpan: var filterSpan }) || !filterSpan.HasValue);
				}
				reference = (analysisScope2, immutableArray);
				return true;
			}
		}
	}

	private AnalysisScope? GetPendingAnalysisScope(AnalysisScope analysisScope)
	{
		(SourceOrAdditionalFile, bool)? filterScope = (analysisScope.FilterFileOpt.HasValue ? new(SourceOrAdditionalFile, bool)?((analysisScope.FilterFileOpt.Value, analysisScope.IsSyntacticSingleFileAnalysis)) : (((SourceOrAdditionalFile, bool)?)null));
		ImmutableArray<DiagnosticAnalyzer> pendingAnalyzers = _analysisResultBuilder.GetPendingAnalyzers(analysisScope.Analyzers, filterScope);
		if (pendingAnalyzers.IsEmpty)
		{
			return null;
		}
		if (pendingAnalyzers.Length >= analysisScope.Analyzers.Length)
		{
			return analysisScope;
		}
		return analysisScope.WithAnalyzers(pendingAnalyzers, this);
	}

	private static ImmutableArray<CompilationEvent> GetCompilationEventsForSingleFileAnalysis(Compilation compilation, AnalysisScope analysisScope, ImmutableArray<AdditionalText> additionalFiles, bool hasAnyActionsRequiringCompilationEvents, CancellationToken cancellationToken)
	{
		if (analysisScope.IsSyntacticSingleFileAnalysis || !hasAnyActionsRequiringCompilationEvents)
		{
			return ImmutableArray<CompilationEvent>.Empty;
		}
		if (analysisScope.IsSemanticSingleFileAnalysisForCompilerAnalyzer)
		{
			CompilationStartedEvent compilationStartedEvent = new CompilationStartedEvent(compilation);
			if (!additionalFiles.IsEmpty)
			{
				compilationStartedEvent = compilationStartedEvent.WithAdditionalFiles(additionalFiles);
			}
			CompilationUnitCompletedEvent item = new CompilationUnitCompletedEvent(compilation, analysisScope.FilterFileOpt.Value.SourceTree, analysisScope.FilterSpanOpt);
			return ImmutableArray.Create((CompilationEvent)compilationStartedEvent, (CompilationEvent)item);
		}
		generateCompilationEvents(compilation, analysisScope, cancellationToken);
		return dequeueAndFilterCompilationEvents(compilation, analysisScope, additionalFiles, cancellationToken);
		static ImmutableArray<CompilationEvent> dequeueAndFilterCompilationEvents(Compilation compilation2, AnalysisScope analysisScope2, ImmutableArray<AdditionalText> additionalFiles2, CancellationToken cancellationToken2)
		{
			AsyncQueue<CompilationEvent> eventQueue = compilation2.EventQueue;
			if (eventQueue.Count == 0)
			{
				return ImmutableArray<CompilationEvent>.Empty;
			}
			cancellationToken2.ThrowIfCancellationRequested();
			bool flag = analysisScope2.FilterSpanOpt.HasValue;
			SyntaxTree sourceTree = analysisScope2.FilterFileOpt.Value.SourceTree;
			ArrayBuilder<CompilationEvent> instance = ArrayBuilder<CompilationEvent>.GetInstance();
			CompilationEvent d;
			while (eventQueue.TryDequeue(out d))
			{
				if (!(d is CompilationStartedEvent compilationStartedEvent2))
				{
					if (!(d is CompilationCompletedEvent))
					{
						if (!(d is CompilationUnitCompletedEvent compilationUnitCompletedEvent))
						{
							if (!(d is SymbolDeclaredCompilationEvent symbolDeclaredCompilationEvent))
							{
								throw ExceptionUtilities.UnexpectedValue(d.GetType().ToString());
							}
							if (!shouldIncludeSymbol(symbolDeclaredCompilationEvent.SymbolInternal, sourceTree, cancellationToken2))
							{
								continue;
							}
						}
						else
						{
							if (sourceTree != compilationUnitCompletedEvent.CompilationUnit)
							{
								continue;
							}
							flag = false;
						}
					}
				}
				else if (!additionalFiles2.IsEmpty)
				{
					d = compilationStartedEvent2.WithAdditionalFiles(additionalFiles2);
				}
				instance.Add(d);
			}
			if (flag)
			{
				instance.Add(new CompilationUnitCompletedEvent(compilation2, sourceTree, analysisScope2.FilterSpanOpt));
			}
			return instance.ToImmutableAndFree();
		}
		static void generateCompilationEvents(Compilation compilation2, AnalysisScope analysisScope2, CancellationToken cancellationToken2)
		{
			if (!analysisScope2.FilterFileOpt.HasValue)
			{
				compilation2.GetDiagnostics(cancellationToken2);
			}
			else if (!analysisScope2.IsSyntacticSingleFileAnalysis)
			{
				compilation2.GetSemanticModel(analysisScope2.FilterFileOpt.Value.SourceTree).GetDiagnostics(analysisScope2.FilterSpanOpt, cancellationToken2);
			}
		}
		static bool shouldIncludeSymbol(ISymbolInternal symbol, SyntaxTree tree, CancellationToken cancellationToken2)
		{
			if (symbol.IsDefinedInSourceTree(tree, null, cancellationToken2))
			{
				return true;
			}
			if (symbol is IMethodSymbolInternal methodSymbolInternal)
			{
				IMethodSymbolInternal? partialDefinitionPart = methodSymbolInternal.PartialDefinitionPart;
				if (partialDefinitionPart == null || !partialDefinitionPart.IsDefinedInSourceTree(tree, null, cancellationToken2))
				{
					IMethodSymbolInternal? partialImplementationPart = methodSymbolInternal.PartialImplementationPart;
					if (partialImplementationPart == null || !partialImplementationPart.IsDefinedInSourceTree(tree, null, cancellationToken2))
					{
						goto IL_00a9;
					}
				}
				return true;
			}
			if (symbol is IPropertySymbolInternal propertySymbolInternal)
			{
				IPropertySymbolInternal? partialDefinitionPart2 = propertySymbolInternal.PartialDefinitionPart;
				if (partialDefinitionPart2 == null || !partialDefinitionPart2.IsDefinedInSourceTree(tree, null, cancellationToken2))
				{
					IPropertySymbolInternal? partialImplementationPart2 = propertySymbolInternal.PartialImplementationPart;
					if (partialImplementationPart2 == null || !partialImplementationPart2.IsDefinedInSourceTree(tree, null, cancellationToken2))
					{
						goto IL_00a9;
					}
				}
				return true;
			}
			goto IL_00a9;
			IL_00a9:
			return false;
		}
	}

	private static AsyncQueue<CompilationEvent> CreateEventsQueue(ImmutableArray<CompilationEvent> compilationEvents)
	{
		if (compilationEvents.IsEmpty)
		{
			return s_EmptyEventQueue;
		}
		AsyncQueue<CompilationEvent> asyncQueue = new AsyncQueue<CompilationEvent>();
		foreach (CompilationEvent item in compilationEvents)
		{
			asyncQueue.TryEnqueue(item);
		}
		return asyncQueue;
	}

	public static IEnumerable<Diagnostic> GetEffectiveDiagnostics(IEnumerable<Diagnostic> diagnostics, Compilation compilation)
	{
		return GetEffectiveDiagnostics(diagnostics.AsImmutableOrNull(), compilation);
	}

	public static IEnumerable<Diagnostic> GetEffectiveDiagnostics(ImmutableArray<Diagnostic> diagnostics, Compilation compilation)
	{
		if (diagnostics.IsDefault)
		{
			throw new ArgumentNullException("diagnostics");
		}
		if (compilation == null)
		{
			throw new ArgumentNullException("compilation");
		}
		return GetEffectiveDiagnosticsImpl(diagnostics, compilation);
	}

	private static IEnumerable<Diagnostic> GetEffectiveDiagnosticsImpl(ImmutableArray<Diagnostic> diagnostics, Compilation compilation)
	{
		if (diagnostics.IsEmpty)
		{
			yield break;
		}
		if (compilation.SemanticModelProvider == null)
		{
			compilation = compilation.WithSemanticModelProvider(CachingSemanticModelProvider.Instance);
		}
		SuppressMessageAttributeState suppressMessageState = new SuppressMessageAttributeState(compilation);
		foreach (Diagnostic item in diagnostics)
		{
			if (item != null)
			{
				Diagnostic diagnostic = compilation.Options.FilterDiagnostic(item, CancellationToken.None);
				if (diagnostic != null)
				{
					yield return suppressMessageState.ApplySourceSuppressions(diagnostic);
				}
			}
		}
	}

	[Obsolete("This API is no longer supported. See https://github.com/dotnet/roslyn/issues/67592 for details")]
	public static bool IsDiagnosticAnalyzerSuppressed(DiagnosticAnalyzer analyzer, CompilationOptions options, Action<Exception, DiagnosticAnalyzer, Diagnostic>? onAnalyzerException = null)
	{
		VerifyAnalyzerArgumentForStaticApis(analyzer);
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		new AnalyzerManager(analyzer);
		Action<Exception, DiagnosticAnalyzer, Diagnostic, CancellationToken> wrappedOnAnalyzerException = delegate(Exception ex, DiagnosticAnalyzer arg, Diagnostic diagnostic, CancellationToken _)
		{
			onAnalyzerException?.Invoke(ex, arg, diagnostic);
		};
		Func<DiagnosticAnalyzer, ImmutableArray<DiagnosticDescriptor>> getSupportedDiagnosticDescriptors = delegate(DiagnosticAnalyzer diagnosticAnalyzer)
		{
			try
			{
				return diagnosticAnalyzer.SupportedDiagnostics;
			}
			catch (Exception exception) when (AnalyzerExecutor.HandleAnalyzerException(exception, diagnosticAnalyzer, null, wrappedOnAnalyzerException, null, CancellationToken.None))
			{
				return ImmutableArray<DiagnosticDescriptor>.Empty;
			}
		};
		Func<DiagnosticSuppressor, ImmutableArray<SuppressionDescriptor>> getSupportedSuppressionDescriptors = delegate(DiagnosticSuppressor suppressor)
		{
			try
			{
				return suppressor.SupportedSuppressions;
			}
			catch (Exception exception) when (AnalyzerExecutor.HandleAnalyzerException(exception, suppressor, null, wrappedOnAnalyzerException, null, CancellationToken.None))
			{
				return ImmutableArray<SuppressionDescriptor>.Empty;
			}
		};
		return AnalyzerManager.IsDiagnosticAnalyzerSuppressed(analyzer, options, AnalyzerDriver.IsCompilerAnalyzer, SeverityFilter.None, (DiagnosticDescriptor _) => false, getSupportedDiagnosticDescriptors, getSupportedSuppressionDescriptors, CancellationToken.None);
	}

	[Obsolete("This API is no longer required to be invoked. Analyzer state is automatically cleaned up when CompilationWithAnalyzers instance is released.")]
	public static void ClearAnalyzerState(ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
	}

	public async Task<AnalyzerTelemetryInfo> GetAnalyzerTelemetryInfoAsync(DiagnosticAnalyzer analyzer, CancellationToken cancellationToken)
	{
		VerifyAnalyzerArgument(analyzer);
		try
		{
			AnalyzerActionCounts actionCounts = await GetAnalyzerActionCountsAsync(analyzer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			int suppressionActionCounts = ((analyzer is DiagnosticSuppressor) ? 1 : 0);
			TimeSpan analyzerExecutionTime = GetAnalyzerExecutionTime(analyzer);
			return new AnalyzerTelemetryInfo(actionCounts, suppressionActionCounts, analyzerExecutionTime);
		}
		catch (Exception exception) when (FatalError.ReportAndPropagateUnlessCanceled(exception, cancellationToken))
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/DiagnosticAnalyzer/CompilationWithAnalyzers.cs", 1289);
		}
	}

	private async Task<AnalyzerActionCounts> GetAnalyzerActionCountsAsync(DiagnosticAnalyzer analyzer, CancellationToken cancellationToken)
	{
		AnalysisScope analysisScope = _compilationAnalysisScope.WithAnalyzers(ImmutableArray.Create(analyzer), this);
		using AnalyzerDriver driver = await CreateAndInitializeDriverAsync(_compilation, _analysisOptions, analysisScope, _suppressors, categorizeDiagnostics: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		cancellationToken.ThrowIfCancellationRequested();
		return await driver.GetAnalyzerActionCountsAsync(analyzer, _compilation.Options, analysisScope, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private TimeSpan GetAnalyzerExecutionTime(DiagnosticAnalyzer analyzer)
	{
		if (!_analysisOptions.LogAnalyzerExecutionTime)
		{
			return default(TimeSpan);
		}
		return _analysisResultBuilder.GetAnalyzerExecutionTime(analyzer);
	}
}
