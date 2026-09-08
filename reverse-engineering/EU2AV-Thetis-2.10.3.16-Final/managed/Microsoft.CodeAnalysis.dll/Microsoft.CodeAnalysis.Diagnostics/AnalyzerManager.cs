using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal class AnalyzerManager
{
	private sealed class AnalyzerExecutionContext
	{
		private static ImmutableDictionary<LocalizableString, Exception?> s_localizableStringToException = ImmutableDictionary<LocalizableString, Exception>.Empty.WithComparers(ReferenceEqualityComparer.Instance);

		private readonly DiagnosticAnalyzer _analyzer;

		private readonly object _gate;

		private Dictionary<ISymbol, HashSet<ISymbol>?>? _lazyPendingMemberSymbolsMap;

		private Dictionary<ISymbol, (ImmutableArray<SymbolEndAnalyzerAction>, SymbolDeclaredCompilationEvent)>? _lazyPendingSymbolEndActionsMap;

		private Task<HostSessionStartAnalysisScope>? _lazySessionScopeTask;

		private Task<HostCompilationStartAnalysisScope>? _lazyCompilationScopeTask;

		private Dictionary<ISymbol, Task<HostSymbolStartAnalysisScope>>? _lazySymbolScopeTasks;

		private ImmutableArray<DiagnosticDescriptor> _lazyDiagnosticDescriptors;

		private ImmutableArray<SuppressionDescriptor> _lazySuppressionDescriptors;

		public AnalyzerExecutionContext(DiagnosticAnalyzer analyzer)
		{
			_analyzer = analyzer;
			_gate = new object();
		}

		public Task<HostSessionStartAnalysisScope> GetSessionAnalysisScopeAsync(AnalyzerExecutor analyzerExecutor, CancellationToken cancellationToken)
		{
			lock (_gate)
			{
				if (_lazySessionScopeTask != null)
				{
					return _lazySessionScopeTask;
				}
				return _lazySessionScopeTask = getSessionAnalysisScopeTaskSlowAsync(this, analyzerExecutor, cancellationToken);
			}
			static Task<HostSessionStartAnalysisScope> getSessionAnalysisScopeTaskSlowAsync(AnalyzerExecutionContext context, AnalyzerExecutor executor, CancellationToken cancellationToken2)
			{
				return Task.Run(delegate
				{
					HostSessionStartAnalysisScope hostSessionStartAnalysisScope = new HostSessionStartAnalysisScope(context._analyzer);
					executor.ExecuteInitializeMethod(hostSessionStartAnalysisScope, executor.SeverityFilter, cancellationToken2);
					return hostSessionStartAnalysisScope;
				}, cancellationToken2);
			}
		}

		public void ClearSessionScopeTask()
		{
			lock (_gate)
			{
				_lazySessionScopeTask = null;
			}
		}

		public Task<HostCompilationStartAnalysisScope> GetCompilationAnalysisScopeAsync(HostSessionStartAnalysisScope sessionScope, AnalyzerExecutor analyzerExecutor, CancellationToken cancellationToken)
		{
			lock (_gate)
			{
				if (_lazyCompilationScopeTask == null)
				{
					_lazyCompilationScopeTask = Task.Run(delegate
					{
						HostCompilationStartAnalysisScope hostCompilationStartAnalysisScope = new HostCompilationStartAnalysisScope(sessionScope);
						analyzerExecutor.ExecuteCompilationStartActions(sessionScope.GetAnalyzerActions().CompilationStartActions, hostCompilationStartAnalysisScope, cancellationToken);
						return hostCompilationStartAnalysisScope;
					}, cancellationToken);
				}
				return _lazyCompilationScopeTask;
			}
		}

		public void ClearCompilationScopeTask()
		{
			lock (_gate)
			{
				_lazyCompilationScopeTask = null;
			}
		}

		public Task<HostSymbolStartAnalysisScope> GetSymbolAnalysisScopeAsync(ISymbol symbol, bool isGeneratedCodeSymbol, SyntaxTree? filterTree, TextSpan? filterSpan, ImmutableArray<SymbolStartAnalyzerAction> symbolStartActions, AnalyzerExecutor analyzerExecutor, CancellationToken cancellationToken)
		{
			lock (_gate)
			{
				if (_lazySymbolScopeTasks == null)
				{
					_lazySymbolScopeTasks = new Dictionary<ISymbol, Task<HostSymbolStartAnalysisScope>>();
				}
				if (!_lazySymbolScopeTasks.TryGetValue(symbol, out Task<HostSymbolStartAnalysisScope> value))
				{
					value = Task.Run(() => getSymbolAnalysisScopeCore(), cancellationToken);
					_lazySymbolScopeTasks.Add(symbol, value);
				}
				return value;
			}
			HashSet<ISymbol>? getDependentSymbols()
			{
				HashSet<ISymbol> memberSet = null;
				switch (symbol.Kind)
				{
				case SymbolKind.NamedType:
					processMembers(((INamedTypeSymbol)symbol).GetMembers());
					break;
				case SymbolKind.Namespace:
					processMembers(((INamespaceSymbol)symbol).GetMembers());
					break;
				}
				return memberSet;
				void processMembers(IEnumerable<ISymbol> members)
				{
					foreach (ISymbol member in members)
					{
						if (!member.IsImplicitlyDeclared && member.IsInSource())
						{
							if (memberSet == null)
							{
								memberSet = new HashSet<ISymbol>();
							}
							memberSet.Add(member);
							if (member is IMethodSymbol methodSymbol)
							{
								IMethodSymbol partialImplementationPart = methodSymbol.PartialImplementationPart;
								if (partialImplementationPart != null)
								{
									memberSet.Add(partialImplementationPart);
									goto IL_0090;
								}
							}
							if (member is IPropertySymbol propertySymbol)
							{
								IPropertySymbol partialImplementationPart2 = propertySymbol.PartialImplementationPart;
								if (partialImplementationPart2 != null)
								{
									memberSet.Add(partialImplementationPart2);
								}
							}
						}
						goto IL_0090;
						IL_0090:
						if (member is INamedTypeSymbol namedTypeSymbol)
						{
							processMembers(namedTypeSymbol.GetMembers());
						}
					}
				}
			}
			HostSymbolStartAnalysisScope getSymbolAnalysisScopeCore()
			{
				HostSymbolStartAnalysisScope hostSymbolStartAnalysisScope = new HostSymbolStartAnalysisScope(_analyzer);
				analyzerExecutor.ExecuteSymbolStartActions(symbol, symbolStartActions, hostSymbolStartAnalysisScope, isGeneratedCodeSymbol, filterTree, filterSpan, cancellationToken);
				if (hostSymbolStartAnalysisScope.GetAnalyzerActions().SymbolEndActionsCount > 0)
				{
					HashSet<ISymbol> value2 = getDependentSymbols();
					lock (_gate)
					{
						if (_lazyPendingMemberSymbolsMap == null)
						{
							_lazyPendingMemberSymbolsMap = new Dictionary<ISymbol, HashSet<ISymbol>>();
						}
						_lazyPendingMemberSymbolsMap[symbol] = value2;
					}
				}
				return hostSymbolStartAnalysisScope;
			}
		}

		[Conditional("DEBUG")]
		private void VerifyNewEntryForPendingMemberSymbolsMap(ISymbol symbol, HashSet<ISymbol>? dependentSymbols)
		{
			if (!_lazyPendingMemberSymbolsMap.TryGetValue(symbol, out HashSet<ISymbol> _))
			{
			}
		}

		public void ClearSymbolScopeTask(ISymbol symbol)
		{
			lock (_gate)
			{
				_lazySymbolScopeTasks?.Remove(symbol);
			}
		}

		public ImmutableArray<DiagnosticDescriptor> GetOrComputeDiagnosticDescriptors(DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor, CancellationToken cancellationToken)
		{
			return GetOrComputeDescriptors(ref _lazyDiagnosticDescriptors, ComputeDiagnosticDescriptors_NoLock, analyzer, analyzerExecutor, _gate, cancellationToken);
		}

		public ImmutableArray<SuppressionDescriptor> GetOrComputeSuppressionDescriptors(DiagnosticSuppressor suppressor, AnalyzerExecutor analyzerExecutor, CancellationToken cancellationToken)
		{
			return GetOrComputeDescriptors(ref _lazySuppressionDescriptors, ComputeSuppressionDescriptors_NoLock, suppressor, analyzerExecutor, _gate, cancellationToken);
		}

		private static ImmutableArray<TDescriptor> GetOrComputeDescriptors<TDescriptor>(ref ImmutableArray<TDescriptor> lazyDescriptors, Func<DiagnosticAnalyzer, AnalyzerExecutor, CancellationToken, ImmutableArray<TDescriptor>> computeDescriptorsNoLock, DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor, object gate, CancellationToken cancellationToken)
		{
			if (!lazyDescriptors.IsDefault)
			{
				return lazyDescriptors;
			}
			lock (gate)
			{
				if (lazyDescriptors.IsDefault)
				{
					lazyDescriptors = computeDescriptorsNoLock(analyzer, analyzerExecutor, cancellationToken);
				}
				return lazyDescriptors;
			}
		}

		private static ImmutableArray<DiagnosticDescriptor> ComputeDiagnosticDescriptors_NoLock(DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor, CancellationToken cancellationToken)
		{
			ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = ImmutableArray<DiagnosticDescriptor>.Empty;
			analyzerExecutor.ExecuteAndCatchIfThrows<object>(analyzer, delegate
			{
				ImmutableArray<DiagnosticDescriptor> supportedDiagnostics2 = analyzer.SupportedDiagnostics;
				if (!supportedDiagnostics2.IsDefaultOrEmpty)
				{
					foreach (DiagnosticDescriptor item in supportedDiagnostics2)
					{
						if (item == null)
						{
							throw new ArgumentException(string.Format(CodeAnalysisResources.SupportedDiagnosticsHasNullDescriptor, analyzer.ToString()), "SupportedDiagnostics");
						}
					}
					supportedDiagnostics = supportedDiagnostics2;
				}
			}, null, null, cancellationToken);
			Action<Exception, DiagnosticAnalyzer, Diagnostic, CancellationToken> onAnalyzerException = analyzerExecutor.OnAnalyzerException;
			if (onAnalyzerException != null)
			{
				foreach (DiagnosticDescriptor item2 in supportedDiagnostics)
				{
					forceLocalizableStringExceptions(item2.Title);
					forceLocalizableStringExceptions(item2.MessageFormat);
					forceLocalizableStringExceptions(item2.Description);
				}
			}
			return supportedDiagnostics;
			static Exception? computeException(LocalizableString localizableString)
			{
				Exception localException = null;
				EventHandler<Exception> value = delegate(object _, Exception ex)
				{
					localException = ex;
				};
				localizableString.OnException += value;
				localizableString.ToString();
				localizableString.OnException -= value;
				return localException;
			}
			void forceLocalizableStringExceptions(LocalizableString localizableString)
			{
				Exception ex = getAndCacheToStringException(localizableString);
				if (ex != null)
				{
					Diagnostic arg = AnalyzerExecutor.CreateAnalyzerExceptionDiagnostic(analyzer, ex);
					onAnalyzerException(ex, analyzer, arg, cancellationToken);
				}
			}
			static Exception? getAndCacheToStringException(LocalizableString localizableString)
			{
				if (!localizableString.CanThrowExceptions)
				{
					return null;
				}
				return ImmutableInterlocked.GetOrAdd(ref s_localizableStringToException, localizableString, computeException);
			}
		}

		private static ImmutableArray<SuppressionDescriptor> ComputeSuppressionDescriptors_NoLock(DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor, CancellationToken cancellationToken)
		{
			ImmutableArray<SuppressionDescriptor> descriptors = ImmutableArray<SuppressionDescriptor>.Empty;
			DiagnosticSuppressor suppressor = analyzer as DiagnosticSuppressor;
			if (suppressor != null)
			{
				analyzerExecutor.ExecuteAndCatchIfThrows<object>(analyzer, delegate
				{
					ImmutableArray<SuppressionDescriptor> supportedSuppressions = suppressor.SupportedSuppressions;
					if (!supportedSuppressions.IsDefaultOrEmpty)
					{
						foreach (SuppressionDescriptor item in supportedSuppressions)
						{
							if (item == null)
							{
								throw new ArgumentException(string.Format(CodeAnalysisResources.SupportedSuppressionsHasNullDescriptor, analyzer.ToString()), "SupportedSuppressions");
							}
						}
						descriptors = supportedSuppressions;
					}
				}, null, null, cancellationToken);
			}
			return descriptors;
		}

		public bool TryProcessCompletedMemberAndGetPendingSymbolEndActionsForContainer(ISymbol containingSymbol, ISymbol processedMemberSymbol, out (ImmutableArray<SymbolEndAnalyzerAction> symbolEndActions, SymbolDeclaredCompilationEvent symbolDeclaredEvent) containerEndActionsAndEvent)
		{
			containerEndActionsAndEvent = default((ImmutableArray<SymbolEndAnalyzerAction>, SymbolDeclaredCompilationEvent));
			lock (_gate)
			{
				if (_lazyPendingMemberSymbolsMap == null || !_lazyPendingMemberSymbolsMap.TryGetValue(containingSymbol, out HashSet<ISymbol> value))
				{
					return false;
				}
				value.Remove(processedMemberSymbol);
				if (value.Count > 0 || _lazyPendingSymbolEndActionsMap == null || !_lazyPendingSymbolEndActionsMap.TryGetValue(containingSymbol, out containerEndActionsAndEvent))
				{
					return false;
				}
				_lazyPendingSymbolEndActionsMap.Remove(containingSymbol);
				return true;
			}
		}

		public bool TryStartExecuteSymbolEndActions(ImmutableArray<SymbolEndAnalyzerAction> symbolEndActions, SymbolDeclaredCompilationEvent symbolDeclaredEvent)
		{
			ISymbol symbol = symbolDeclaredEvent.Symbol;
			lock (_gate)
			{
				if (_lazyPendingMemberSymbolsMap.TryGetValue(symbol, out HashSet<ISymbol> value) && value != null && value.Count > 0)
				{
					MarkSymbolEndAnalysisPending_NoLock(symbol, symbolEndActions, symbolDeclaredEvent);
					return false;
				}
				_lazyPendingSymbolEndActionsMap?.Remove(symbol);
				return true;
			}
		}

		public void MarkSymbolEndAnalysisComplete(ISymbol symbol)
		{
			lock (_gate)
			{
				_lazyPendingMemberSymbolsMap?.Remove(symbol);
			}
		}

		public void MarkSymbolEndAnalysisPending(ISymbol symbol, ImmutableArray<SymbolEndAnalyzerAction> symbolEndActions, SymbolDeclaredCompilationEvent symbolDeclaredEvent)
		{
			lock (_gate)
			{
				MarkSymbolEndAnalysisPending_NoLock(symbol, symbolEndActions, symbolDeclaredEvent);
			}
		}

		private void MarkSymbolEndAnalysisPending_NoLock(ISymbol symbol, ImmutableArray<SymbolEndAnalyzerAction> symbolEndActions, SymbolDeclaredCompilationEvent symbolDeclaredEvent)
		{
			if (_lazyPendingSymbolEndActionsMap == null)
			{
				_lazyPendingSymbolEndActionsMap = new Dictionary<ISymbol, (ImmutableArray<SymbolEndAnalyzerAction>, SymbolDeclaredCompilationEvent)>();
			}
			_lazyPendingSymbolEndActionsMap[symbol] = (symbolEndActions, symbolDeclaredEvent);
		}

		[Conditional("DEBUG")]
		public void VerifyAllSymbolEndActionsExecuted()
		{
			lock (_gate)
			{
			}
		}
	}

	private readonly Dictionary<DiagnosticAnalyzer, AnalyzerExecutionContext> _analyzerExecutionContextMap;

	public AnalyzerManager(ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		_analyzerExecutionContextMap = CreateAnalyzerExecutionContextMap(analyzers);
	}

	public AnalyzerManager(DiagnosticAnalyzer analyzer)
	{
		_analyzerExecutionContextMap = CreateAnalyzerExecutionContextMap(SpecializedCollections.SingletonEnumerable(analyzer));
	}

	private Dictionary<DiagnosticAnalyzer, AnalyzerExecutionContext> CreateAnalyzerExecutionContextMap(IEnumerable<DiagnosticAnalyzer> analyzers)
	{
		Dictionary<DiagnosticAnalyzer, AnalyzerExecutionContext> dictionary = new Dictionary<DiagnosticAnalyzer, AnalyzerExecutionContext>();
		foreach (DiagnosticAnalyzer analyzer in analyzers)
		{
			dictionary.Add(analyzer, new AnalyzerExecutionContext(analyzer));
		}
		return dictionary;
	}

	private AnalyzerExecutionContext GetAnalyzerExecutionContext(DiagnosticAnalyzer analyzer)
	{
		return _analyzerExecutionContextMap[analyzer];
	}

	private async ValueTask<HostCompilationStartAnalysisScope> GetCompilationAnalysisScopeAsync(HostSessionStartAnalysisScope sessionScope, AnalyzerExecutor analyzerExecutor, CancellationToken cancellationToken)
	{
		AnalyzerExecutionContext analyzerExecutionContext = GetAnalyzerExecutionContext(sessionScope.Analyzer);
		return await GetCompilationAnalysisScopeCoreAsync(sessionScope, analyzerExecutor, analyzerExecutionContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async ValueTask<HostCompilationStartAnalysisScope> GetCompilationAnalysisScopeCoreAsync(HostSessionStartAnalysisScope sessionScope, AnalyzerExecutor analyzerExecutor, AnalyzerExecutionContext analyzerExecutionContext, CancellationToken cancellationToken)
	{
		try
		{
			return await analyzerExecutionContext.GetCompilationAnalysisScopeAsync(sessionScope, analyzerExecutor, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (OperationCanceledException)
		{
			analyzerExecutionContext.ClearCompilationScopeTask();
			cancellationToken.ThrowIfCancellationRequested();
			return await GetCompilationAnalysisScopeCoreAsync(sessionScope, analyzerExecutor, analyzerExecutionContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task<HostSymbolStartAnalysisScope> GetSymbolAnalysisScopeAsync(ISymbol symbol, bool isGeneratedCodeSymbol, SyntaxTree? filterTree, TextSpan? filterSpan, DiagnosticAnalyzer analyzer, ImmutableArray<SymbolStartAnalyzerAction> symbolStartActions, AnalyzerExecutor analyzerExecutor, CancellationToken cancellationToken)
	{
		AnalyzerExecutionContext analyzerExecutionContext = GetAnalyzerExecutionContext(analyzer);
		return await GetSymbolAnalysisScopeCoreAsync(symbol, isGeneratedCodeSymbol, filterTree, filterSpan, symbolStartActions, analyzerExecutor, analyzerExecutionContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<HostSymbolStartAnalysisScope> GetSymbolAnalysisScopeCoreAsync(ISymbol symbol, bool isGeneratedCodeSymbol, SyntaxTree? filterTree, TextSpan? filterSpan, ImmutableArray<SymbolStartAnalyzerAction> symbolStartActions, AnalyzerExecutor analyzerExecutor, AnalyzerExecutionContext analyzerExecutionContext, CancellationToken cancellationToken)
	{
		try
		{
			return await analyzerExecutionContext.GetSymbolAnalysisScopeAsync(symbol, isGeneratedCodeSymbol, filterTree, filterSpan, symbolStartActions, analyzerExecutor, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (OperationCanceledException)
		{
			analyzerExecutionContext.ClearSymbolScopeTask(symbol);
			cancellationToken.ThrowIfCancellationRequested();
			return await GetSymbolAnalysisScopeCoreAsync(symbol, isGeneratedCodeSymbol, filterTree, filterSpan, symbolStartActions, analyzerExecutor, analyzerExecutionContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async ValueTask<HostSessionStartAnalysisScope> GetSessionAnalysisScopeAsync(DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor, CancellationToken cancellationToken)
	{
		AnalyzerExecutionContext analyzerExecutionContext = GetAnalyzerExecutionContext(analyzer);
		return await GetSessionAnalysisScopeCoreAsync(analyzerExecutor, analyzerExecutionContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async ValueTask<HostSessionStartAnalysisScope> GetSessionAnalysisScopeCoreAsync(AnalyzerExecutor analyzerExecutor, AnalyzerExecutionContext analyzerExecutionContext, CancellationToken cancellationToken)
	{
		try
		{
			return await analyzerExecutionContext.GetSessionAnalysisScopeAsync(analyzerExecutor, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (OperationCanceledException)
		{
			analyzerExecutionContext.ClearSessionScopeTask();
			cancellationToken.ThrowIfCancellationRequested();
			return await GetSessionAnalysisScopeCoreAsync(analyzerExecutor, analyzerExecutionContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public async ValueTask<AnalyzerActions> GetAnalyzerActionsAsync(DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor, CancellationToken cancellationToken)
	{
		HostSessionStartAnalysisScope hostSessionStartAnalysisScope = await GetSessionAnalysisScopeAsync(analyzer, analyzerExecutor, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (hostSessionStartAnalysisScope.GetAnalyzerActions().CompilationStartActionsCount > 0 && analyzerExecutor.Compilation != null)
		{
			return (await GetCompilationAnalysisScopeAsync(hostSessionStartAnalysisScope, analyzerExecutor, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetAnalyzerActions();
		}
		return hostSessionStartAnalysisScope.GetAnalyzerActions();
	}

	public async ValueTask<AnalyzerActions> GetPerSymbolAnalyzerActionsAsync(ISymbol symbol, bool isGeneratedCodeSymbol, SyntaxTree? filterTree, TextSpan? filterSpan, DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor, CancellationToken cancellationToken)
	{
		AnalyzerActions analyzerActions = await GetAnalyzerActionsAsync(analyzer, analyzerExecutor, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (analyzerActions.SymbolStartActionsCount > 0)
		{
			ImmutableArray<SymbolStartAnalyzerAction> symbolStartActions = getFilteredActionsByKind(analyzerActions.SymbolStartActions);
			if (symbolStartActions.Length > 0)
			{
				return (await GetSymbolAnalysisScopeAsync(symbol, isGeneratedCodeSymbol, filterTree, filterSpan, analyzer, symbolStartActions, analyzerExecutor, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetAnalyzerActions();
			}
		}
		return AnalyzerActions.Empty;
		ImmutableArray<SymbolStartAnalyzerAction> getFilteredActionsByKind(ImmutableArray<SymbolStartAnalyzerAction> immutableArray)
		{
			ArrayBuilder<SymbolStartAnalyzerAction> arrayBuilder = null;
			for (int i = 0; i < immutableArray.Length; i++)
			{
				SymbolStartAnalyzerAction symbolStartAnalyzerAction = immutableArray[i];
				if (symbolStartAnalyzerAction.Kind != symbol.Kind)
				{
					if (arrayBuilder == null)
					{
						arrayBuilder = ArrayBuilder<SymbolStartAnalyzerAction>.GetInstance();
						arrayBuilder.AddRange(immutableArray, i);
					}
				}
				else
				{
					arrayBuilder?.Add(symbolStartAnalyzerAction);
				}
			}
			return arrayBuilder?.ToImmutableAndFree() ?? immutableArray;
		}
	}

	public async Task<bool> IsConcurrentAnalyzerAsync(DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor, CancellationToken cancellationToken)
	{
		return (await GetSessionAnalysisScopeAsync(analyzer, analyzerExecutor, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).IsConcurrentAnalyzer();
	}

	public async Task<GeneratedCodeAnalysisFlags> GetGeneratedCodeAnalysisFlagsAsync(DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor, CancellationToken cancellationToken)
	{
		return (await GetSessionAnalysisScopeAsync(analyzer, analyzerExecutor, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetGeneratedCodeAnalysisFlags();
	}

	public ImmutableArray<DiagnosticDescriptor> GetSupportedDiagnosticDescriptors(DiagnosticAnalyzer analyzer, AnalyzerExecutor analyzerExecutor, CancellationToken cancellationToken)
	{
		return GetAnalyzerExecutionContext(analyzer).GetOrComputeDiagnosticDescriptors(analyzer, analyzerExecutor, cancellationToken);
	}

	public ImmutableArray<SuppressionDescriptor> GetSupportedSuppressionDescriptors(DiagnosticSuppressor suppressor, AnalyzerExecutor analyzerExecutor, CancellationToken cancellationToken)
	{
		return GetAnalyzerExecutionContext(suppressor).GetOrComputeSuppressionDescriptors(suppressor, analyzerExecutor, cancellationToken);
	}

	internal bool IsSupportedDiagnostic(DiagnosticAnalyzer analyzer, Diagnostic diagnostic, Func<DiagnosticAnalyzer, bool> isCompilerAnalyzer, AnalyzerExecutor analyzerExecutor, CancellationToken cancellationToken)
	{
		if (isCompilerAnalyzer(analyzer))
		{
			return true;
		}
		foreach (DiagnosticDescriptor supportedDiagnosticDescriptor in GetSupportedDiagnosticDescriptors(analyzer, analyzerExecutor, cancellationToken))
		{
			if (supportedDiagnosticDescriptor.Id.Equals(diagnostic.Id, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	internal bool IsDiagnosticAnalyzerSuppressed(DiagnosticAnalyzer analyzer, CompilationOptions options, Func<DiagnosticAnalyzer, bool> isCompilerAnalyzer, AnalyzerExecutor analyzerExecutor, AnalysisScope analysisScope, SeverityFilter severityFilter, CancellationToken cancellationToken)
	{
		Func<DiagnosticAnalyzer, ImmutableArray<DiagnosticDescriptor>> getSupportedDiagnosticDescriptors = (DiagnosticAnalyzer analyzer2) => GetSupportedDiagnosticDescriptors(analyzer2, analyzerExecutor, cancellationToken);
		Func<DiagnosticSuppressor, ImmutableArray<SuppressionDescriptor>> getSupportedSuppressionDescriptors = (DiagnosticSuppressor suppressor) => GetSupportedSuppressionDescriptors(suppressor, analyzerExecutor, cancellationToken);
		return IsDiagnosticAnalyzerSuppressed(analyzer, options, isCompilerAnalyzer, severityFilter, isEnabledWithAnalyzerConfigOptions, getSupportedDiagnosticDescriptors, getSupportedSuppressionDescriptors, cancellationToken);
		bool isEnabledWithAnalyzerConfigOptions(DiagnosticDescriptor descriptor)
		{
			SyntaxTreeOptionsProvider syntaxTreeOptionsProvider = analyzerExecutor.Compilation.Options.SyntaxTreeOptionsProvider;
			if (syntaxTreeOptionsProvider != null)
			{
				foreach (SyntaxTree syntaxTree in analysisScope.SyntaxTrees)
				{
					if ((syntaxTreeOptionsProvider.TryGetDiagnosticValue(syntaxTree, descriptor.Id, cancellationToken, out var severity) || analyzerExecutor.AnalyzerOptions.TryGetSeverityFromBulkConfiguration(syntaxTree, analyzerExecutor.Compilation, descriptor, cancellationToken, out severity)) && severity != ReportDiagnostic.Suppress && !severityFilter.Contains(severity))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	internal static bool IsDiagnosticAnalyzerSuppressed(DiagnosticAnalyzer analyzer, CompilationOptions options, Func<DiagnosticAnalyzer, bool> isCompilerAnalyzer, SeverityFilter severityFilter, Func<DiagnosticDescriptor, bool> isEnabledWithAnalyzerConfigOptions, Func<DiagnosticAnalyzer, ImmutableArray<DiagnosticDescriptor>> getSupportedDiagnosticDescriptors, Func<DiagnosticSuppressor, ImmutableArray<SuppressionDescriptor>> getSupportedSuppressionDescriptors, CancellationToken cancellationToken)
	{
		if (isCompilerAnalyzer(analyzer))
		{
			return false;
		}
		ImmutableArray<DiagnosticDescriptor> immutableArray = getSupportedDiagnosticDescriptors(analyzer);
		ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions = options.SpecificDiagnosticOptions;
		foreach (DiagnosticDescriptor item in immutableArray)
		{
			if (item.IsNotConfigurable())
			{
				if (item.IsEnabledByDefault)
				{
					return false;
				}
				continue;
			}
			if (item.IsCustomSeverityConfigurable())
			{
				return false;
			}
			bool flag = !item.IsEnabledByDefault;
			if ((specificDiagnosticOptions.TryGetValue(item.Id, out var value) && value != ReportDiagnostic.Default) || (options.SyntaxTreeOptionsProvider != null && options.SyntaxTreeOptionsProvider.TryGetGlobalDiagnosticValue(item.Id, cancellationToken, out value)))
			{
				flag = value == ReportDiagnostic.Suppress;
			}
			else
			{
				value = (flag ? ReportDiagnostic.Suppress : DiagnosticDescriptor.MapSeverityToReport(item.DefaultSeverity));
			}
			if (severityFilter.Contains(value))
			{
				flag = true;
			}
			if (flag && isEnabledWithAnalyzerConfigOptions(item))
			{
				flag = false;
			}
			if (!flag)
			{
				return false;
			}
		}
		if (analyzer is DiagnosticSuppressor arg)
		{
			foreach (SuppressionDescriptor item2 in getSupportedSuppressionDescriptors(arg))
			{
				if (!item2.IsDisabled(options))
				{
					return false;
				}
			}
		}
		return true;
	}

	internal static bool HasCompilerOrNotConfigurableTagOrCustomConfigurableTag(ImmutableArray<string> customTags)
	{
		foreach (string item in customTags)
		{
			bool flag;
			switch (item)
			{
			case "Compiler":
			case "NotConfigurable":
			case "CustomSeverityConfigurable":
				flag = true;
				break;
			default:
				flag = false;
				break;
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	internal static bool HasNotConfigurableTag(ImmutableArray<string> customTags)
	{
		return HasCustomTag(customTags, "NotConfigurable");
	}

	internal static bool HasCustomSeverityConfigurableTag(ImmutableArray<string> customTags)
	{
		return HasCustomTag(customTags, "CustomSeverityConfigurable");
	}

	private static bool HasCustomTag(ImmutableArray<string> customTags, string tagToFind)
	{
		foreach (string item in customTags)
		{
			if (item == tagToFind)
			{
				return true;
			}
		}
		return false;
	}

	public bool TryProcessCompletedMemberAndGetPendingSymbolEndActionsForContainer(ISymbol containingSymbol, ISymbol processedMemberSymbol, DiagnosticAnalyzer analyzer, out (ImmutableArray<SymbolEndAnalyzerAction> symbolEndActions, SymbolDeclaredCompilationEvent symbolDeclaredEvent) containerEndActionsAndEvent)
	{
		return GetAnalyzerExecutionContext(analyzer).TryProcessCompletedMemberAndGetPendingSymbolEndActionsForContainer(containingSymbol, processedMemberSymbol, out containerEndActionsAndEvent);
	}

	public bool TryStartExecuteSymbolEndActions(ImmutableArray<SymbolEndAnalyzerAction> symbolEndActions, DiagnosticAnalyzer analyzer, SymbolDeclaredCompilationEvent symbolDeclaredEvent)
	{
		return GetAnalyzerExecutionContext(analyzer).TryStartExecuteSymbolEndActions(symbolEndActions, symbolDeclaredEvent);
	}

	public void MarkSymbolEndAnalysisPending(ISymbol symbol, DiagnosticAnalyzer analyzer, ImmutableArray<SymbolEndAnalyzerAction> symbolEndActions, SymbolDeclaredCompilationEvent symbolDeclaredEvent)
	{
		GetAnalyzerExecutionContext(analyzer).MarkSymbolEndAnalysisPending(symbol, symbolEndActions, symbolDeclaredEvent);
	}

	public void MarkSymbolEndAnalysisComplete(ISymbol symbol, DiagnosticAnalyzer analyzer)
	{
		GetAnalyzerExecutionContext(analyzer).MarkSymbolEndAnalysisComplete(symbol);
	}

	[Conditional("DEBUG")]
	public void VerifyAllSymbolEndActionsExecuted()
	{
		foreach (AnalyzerExecutionContext value in _analyzerExecutionContextMap.Values)
		{
			_ = value;
		}
	}
}
