using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Diagnostics.Telemetry;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class AnalysisResultBuilder
{
	private static readonly ImmutableDictionary<string, OneOrMany<AdditionalText>> s_emptyPathToAdditionalTextMap = ImmutableDictionary<string, OneOrMany<AdditionalText>>.Empty.WithComparers(PathUtilities.Comparer);

	private readonly object _gate = new object();

	private readonly Dictionary<DiagnosticAnalyzer, TimeSpan>? _analyzerExecutionTimeOpt;

	private readonly HashSet<DiagnosticAnalyzer> _completedAnalyzersForCompilation;

	private readonly Dictionary<SyntaxTree, HashSet<DiagnosticAnalyzer>> _completedSyntaxAnalyzersByTree;

	private readonly Dictionary<SyntaxTree, HashSet<DiagnosticAnalyzer>> _completedSemanticAnalyzersByTree;

	private readonly Dictionary<AdditionalText, HashSet<DiagnosticAnalyzer>> _completedSyntaxAnalyzersByAdditionalFile;

	private readonly Dictionary<DiagnosticAnalyzer, AnalyzerActionCounts> _analyzerActionCounts;

	private readonly ImmutableDictionary<string, OneOrMany<AdditionalText>> _pathToAdditionalTextMap;

	private Dictionary<SyntaxTree, Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>>? _localSemanticDiagnosticsOpt;

	private Dictionary<SyntaxTree, Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>>? _localSyntaxDiagnosticsOpt;

	private Dictionary<AdditionalText, Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>>? _localAdditionalFileDiagnosticsOpt;

	private Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>? _nonLocalDiagnosticsOpt;

	internal AnalysisResultBuilder(bool logAnalyzerExecutionTime, ImmutableArray<DiagnosticAnalyzer> analyzers, ImmutableArray<AdditionalText> additionalFiles)
	{
		_analyzerExecutionTimeOpt = (logAnalyzerExecutionTime ? CreateAnalyzerExecutionTimeMap(analyzers) : null);
		_completedAnalyzersForCompilation = new HashSet<DiagnosticAnalyzer>();
		_completedSyntaxAnalyzersByTree = new Dictionary<SyntaxTree, HashSet<DiagnosticAnalyzer>>();
		_completedSemanticAnalyzersByTree = new Dictionary<SyntaxTree, HashSet<DiagnosticAnalyzer>>();
		_completedSyntaxAnalyzersByAdditionalFile = new Dictionary<AdditionalText, HashSet<DiagnosticAnalyzer>>();
		_analyzerActionCounts = new Dictionary<DiagnosticAnalyzer, AnalyzerActionCounts>(analyzers.Length);
		_pathToAdditionalTextMap = CreatePathToAdditionalTextMap(additionalFiles);
	}

	private static Dictionary<DiagnosticAnalyzer, TimeSpan> CreateAnalyzerExecutionTimeMap(ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		Dictionary<DiagnosticAnalyzer, TimeSpan> dictionary = new Dictionary<DiagnosticAnalyzer, TimeSpan>(analyzers.Length);
		foreach (DiagnosticAnalyzer item in analyzers)
		{
			dictionary[item] = default(TimeSpan);
		}
		return dictionary;
	}

	private static ImmutableDictionary<string, OneOrMany<AdditionalText>> CreatePathToAdditionalTextMap(ImmutableArray<AdditionalText> additionalFiles)
	{
		if (additionalFiles.IsEmpty)
		{
			return s_emptyPathToAdditionalTextMap;
		}
		ImmutableDictionary<string, OneOrMany<AdditionalText>>.Builder builder = ImmutableDictionary.CreateBuilder<string, OneOrMany<AdditionalText>>(PathUtilities.Comparer);
		foreach (AdditionalText item in additionalFiles)
		{
			string key = item.Path ?? string.Empty;
			OneOrMany<AdditionalText> value = (builder[key] = ((!builder.TryGetValue(key, out value)) ? new OneOrMany<AdditionalText>(item) : value.Add(item)));
		}
		return builder.ToImmutable();
	}

	public TimeSpan GetAnalyzerExecutionTime(DiagnosticAnalyzer analyzer)
	{
		lock (_gate)
		{
			return _analyzerExecutionTimeOpt[analyzer];
		}
	}

	private HashSet<DiagnosticAnalyzer>? GetCompletedAnalyzersForFile_NoLock(SourceOrAdditionalFile filterFile, bool syntax)
	{
		SyntaxTree sourceTree = filterFile.SourceTree;
		if (sourceTree != null)
		{
			if ((syntax ? _completedSyntaxAnalyzersByTree : _completedSemanticAnalyzersByTree).TryGetValue(sourceTree, out HashSet<DiagnosticAnalyzer> value))
			{
				return value;
			}
		}
		else
		{
			AdditionalText additionalFile = filterFile.AdditionalFile;
			if (additionalFile == null)
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/DiagnosticAnalyzer/AnalysisResultBuilder.cs", 124);
			}
			if (_completedSyntaxAnalyzersByAdditionalFile.TryGetValue(additionalFile, out HashSet<DiagnosticAnalyzer> value2))
			{
				return value2;
			}
		}
		return null;
	}

	private void AddCompletedAnalyzerForFile_NoLock(SourceOrAdditionalFile filterFile, bool syntax, DiagnosticAnalyzer analyzer)
	{
		HashSet<DiagnosticAnalyzer> value = new HashSet<DiagnosticAnalyzer> { analyzer };
		SyntaxTree sourceTree = filterFile.SourceTree;
		if (sourceTree != null)
		{
			(syntax ? _completedSyntaxAnalyzersByTree : _completedSemanticAnalyzersByTree).Add(sourceTree, value);
			return;
		}
		AdditionalText additionalFile = filterFile.AdditionalFile;
		if (additionalFile != null)
		{
			_completedSyntaxAnalyzersByAdditionalFile.Add(additionalFile, value);
			return;
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/DiagnosticAnalyzer/AnalysisResultBuilder.cs", 144);
	}

	public ImmutableArray<DiagnosticAnalyzer> GetPendingAnalyzers(ImmutableArray<DiagnosticAnalyzer> analyzers, (SourceOrAdditionalFile file, bool syntax)? filterScope)
	{
		lock (_gate)
		{
			HashSet<DiagnosticAnalyzer> item = (filterScope.HasValue ? GetCompletedAnalyzersForFile_NoLock(filterScope.Value.file, filterScope.Value.syntax) : null);
			return analyzers.WhereAsArray((DiagnosticAnalyzer analyzer, (AnalysisResultBuilder self, HashSet<DiagnosticAnalyzer> completedAnalyzersForFile) arg) => (!arg.self._completedAnalyzersForCompilation.Contains(analyzer) && (arg.completedAnalyzersForFile == null || !arg.completedAnalyzersForFile.Contains(analyzer))) ? true : false, (this, item));
		}
	}

	public void ApplySuppressionsAndStoreAnalysisResult(AnalysisScope analysisScope, AnalyzerDriver driver, Compilation compilation, Func<DiagnosticAnalyzer, AnalyzerActionCounts> getAnalyzerActionCounts, CancellationToken cancellationToken)
	{
		foreach (DiagnosticAnalyzer analyzer in analysisScope.Analyzers)
		{
			ImmutableArray<Diagnostic> diagnostics = driver.DequeueLocalDiagnosticsAndApplySuppressions(analyzer, syntax: true, compilation, cancellationToken);
			ImmutableArray<Diagnostic> diagnostics2 = driver.DequeueLocalDiagnosticsAndApplySuppressions(analyzer, syntax: false, compilation, cancellationToken);
			ImmutableArray<Diagnostic> diagnostics3 = driver.DequeueNonLocalDiagnosticsAndApplySuppressions(analyzer, compilation, cancellationToken);
			lock (_gate)
			{
				if (_completedAnalyzersForCompilation.Contains(analyzer))
				{
					continue;
				}
				bool overwrite = false;
				bool overwrite2 = false;
				bool overwrite3 = false;
				bool flag = false;
				if (analysisScope.FilterFileOpt.HasValue)
				{
					HashSet<DiagnosticAnalyzer> completedAnalyzersForFile_NoLock = GetCompletedAnalyzersForFile_NoLock(analysisScope.FilterFileOpt.Value, analysisScope.IsSyntacticSingleFileAnalysis);
					if (completedAnalyzersForFile_NoLock != null && completedAnalyzersForFile_NoLock.Contains(analyzer))
					{
						continue;
					}
					if (!analysisScope.FilterSpanOpt.HasValue && !analysisScope.OriginalFilterSpan.HasValue)
					{
						if (completedAnalyzersForFile_NoLock != null)
						{
							completedAnalyzersForFile_NoLock.Add(analyzer);
						}
						else
						{
							AddCompletedAnalyzerForFile_NoLock(analysisScope.FilterFileOpt.Value, analysisScope.IsSyntacticSingleFileAnalysis, analyzer);
						}
						if (analysisScope.IsSyntacticSingleFileAnalysis)
						{
							if (analysisScope.FilterFileOpt.Value.SourceTree != null)
							{
								overwrite = true;
							}
							else
							{
								overwrite2 = true;
							}
						}
						else
						{
							overwrite3 = true;
						}
					}
					goto IL_019c;
				}
				_completedAnalyzersForCompilation.Add(analyzer);
				flag = true;
				overwrite = true;
				overwrite2 = true;
				overwrite3 = true;
				goto IL_019c;
				IL_019c:
				if (!diagnostics.IsEmpty)
				{
					UpdateLocalDiagnostics_NoLock(analyzer, diagnostics, overwrite, getSourceTree, ref _localSyntaxDiagnosticsOpt);
					UpdateLocalDiagnostics_NoLock(analyzer, diagnostics, overwrite2, getAdditionalTextKey, ref _localAdditionalFileDiagnosticsOpt);
				}
				if (!diagnostics2.IsEmpty)
				{
					UpdateLocalDiagnostics_NoLock(analyzer, diagnostics2, overwrite3, getSourceTree, ref _localSemanticDiagnosticsOpt);
				}
				if (!diagnostics3.IsEmpty)
				{
					UpdateNonLocalDiagnostics_NoLock(analyzer, diagnostics3, flag);
				}
				if (_analyzerExecutionTimeOpt != null)
				{
					TimeSpan timeSpan = driver.ResetAnalyzerExecutionTime(analyzer);
					_analyzerExecutionTimeOpt[analyzer] = (flag ? timeSpan : (_analyzerExecutionTimeOpt[analyzer] + timeSpan));
				}
				if (!_analyzerActionCounts.ContainsKey(analyzer))
				{
					_analyzerActionCounts.Add(analyzer, getAnalyzerActionCounts(analyzer));
				}
			}
		}
		AdditionalText? getAdditionalTextKey(Diagnostic diagnostic)
		{
			if (diagnostic.Location is ExternalFileLocation externalFileLocation && _pathToAdditionalTextMap.TryGetValue(externalFileLocation.GetLineSpan().Path, out OneOrMany<AdditionalText> value))
			{
				foreach (AdditionalText item in value)
				{
					if (analysisScope.AdditionalFiles.Contains(item))
					{
						return item;
					}
				}
			}
			return null;
		}
		static SyntaxTree? getSourceTree(Diagnostic diagnostic)
		{
			return diagnostic.Location.SourceTree;
		}
	}

	private void UpdateLocalDiagnostics_NoLock<TKey>(DiagnosticAnalyzer analyzer, ImmutableArray<Diagnostic> diagnostics, bool overwrite, Func<Diagnostic, TKey?> getKeyFunc, ref Dictionary<TKey, Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>>? lazyLocalDiagnostics) where TKey : class
	{
		if (diagnostics.IsEmpty)
		{
			return;
		}
		lazyLocalDiagnostics = lazyLocalDiagnostics ?? new Dictionary<TKey, Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>>();
		foreach (IGrouping<TKey, Diagnostic> item in diagnostics.GroupBy(getKeyFunc))
		{
			TKey key = item.Key;
			if (key != null)
			{
				if (!lazyLocalDiagnostics.TryGetValue(key, out Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder> value))
				{
					value = new Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>();
					lazyLocalDiagnostics[key] = value;
				}
				if (!value.TryGetValue(analyzer, out var value2))
				{
					value2 = (value[analyzer] = ImmutableArray.CreateBuilder<Diagnostic>());
				}
				UpdateDiagnosticsCore_NoLock(value2, item, overwrite);
			}
		}
	}

	private void UpdateNonLocalDiagnostics_NoLock(DiagnosticAnalyzer analyzer, ImmutableArray<Diagnostic> diagnostics, bool overwrite)
	{
		if (!diagnostics.IsEmpty)
		{
			_nonLocalDiagnosticsOpt = _nonLocalDiagnosticsOpt ?? new Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>();
			if (!_nonLocalDiagnosticsOpt.TryGetValue(analyzer, out ImmutableArray<Diagnostic>.Builder value))
			{
				value = ImmutableArray.CreateBuilder<Diagnostic>();
				_nonLocalDiagnosticsOpt[analyzer] = value;
			}
			UpdateDiagnosticsCore_NoLock(value, diagnostics, overwrite);
		}
	}

	private static void UpdateDiagnosticsCore_NoLock(ImmutableArray<Diagnostic>.Builder currentDiagnostics, IEnumerable<Diagnostic> diagnostics, bool overwrite)
	{
		if (overwrite)
		{
			currentDiagnostics.Clear();
		}
		else
		{
			diagnostics = diagnostics.Where((Diagnostic d) => !currentDiagnostics.Contains(d));
		}
		currentDiagnostics.AddRange(diagnostics);
	}

	internal ImmutableArray<Diagnostic> GetDiagnostics(AnalysisScope analysisScope, bool getLocalDiagnostics, bool getNonLocalDiagnostics)
	{
		lock (_gate)
		{
			return GetDiagnostics_NoLock(analysisScope, getLocalDiagnostics, getNonLocalDiagnostics);
		}
	}

	private ImmutableArray<Diagnostic> GetDiagnostics_NoLock(AnalysisScope analysisScope, bool getLocalDiagnostics, bool getNonLocalDiagnostics)
	{
		ImmutableArray<Diagnostic>.Builder builder = ImmutableArray.CreateBuilder<Diagnostic>();
		if (getLocalDiagnostics)
		{
			if (!analysisScope.IsSingleFileAnalysis)
			{
				AddAllLocalDiagnostics_NoLock(_localSyntaxDiagnosticsOpt, analysisScope, builder);
				AddAllLocalDiagnostics_NoLock(_localSemanticDiagnosticsOpt, analysisScope, builder);
				AddAllLocalDiagnostics_NoLock(_localAdditionalFileDiagnosticsOpt, analysisScope, builder);
			}
			else if (analysisScope.IsSyntacticSingleFileAnalysis)
			{
				AddLocalDiagnosticsForPartialAnalysis_NoLock(_localSyntaxDiagnosticsOpt, analysisScope, builder);
				AddLocalDiagnosticsForPartialAnalysis_NoLock(_localAdditionalFileDiagnosticsOpt, analysisScope, builder);
			}
			else
			{
				AddLocalDiagnosticsForPartialAnalysis_NoLock(_localSemanticDiagnosticsOpt, analysisScope, builder);
			}
		}
		if (getNonLocalDiagnostics && _nonLocalDiagnosticsOpt != null)
		{
			AddDiagnostics_NoLock(_nonLocalDiagnosticsOpt, analysisScope.Analyzers, builder);
		}
		return builder.ToImmutableArray();
	}

	private static void AddAllLocalDiagnostics_NoLock<TKey>(Dictionary<TKey, Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>>? lazyLocalDiagnostics, AnalysisScope analysisScope, ImmutableArray<Diagnostic>.Builder builder) where TKey : class
	{
		if (lazyLocalDiagnostics == null)
		{
			return;
		}
		foreach (Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder> value in lazyLocalDiagnostics.Values)
		{
			AddDiagnostics_NoLock(value, analysisScope.Analyzers, builder);
		}
	}

	private static void AddLocalDiagnosticsForPartialAnalysis_NoLock(Dictionary<SyntaxTree, Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>>? localDiagnostics, AnalysisScope analysisScope, ImmutableArray<Diagnostic>.Builder builder)
	{
		AddLocalDiagnosticsForPartialAnalysis_NoLock(localDiagnostics, analysisScope.FilterFileOpt.Value.SourceTree, analysisScope.Analyzers, builder);
	}

	private static void AddLocalDiagnosticsForPartialAnalysis_NoLock(Dictionary<AdditionalText, Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>>? localDiagnostics, AnalysisScope analysisScope, ImmutableArray<Diagnostic>.Builder builder)
	{
		AddLocalDiagnosticsForPartialAnalysis_NoLock(localDiagnostics, analysisScope.FilterFileOpt.Value.AdditionalFile, analysisScope.Analyzers, builder);
	}

	private static void AddLocalDiagnosticsForPartialAnalysis_NoLock<TKey>(Dictionary<TKey, Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>>? localDiagnostics, TKey? key, ImmutableArray<DiagnosticAnalyzer> analyzers, ImmutableArray<Diagnostic>.Builder builder) where TKey : class
	{
		if (key != null && localDiagnostics != null && localDiagnostics.TryGetValue(key, out Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder> value))
		{
			AddDiagnostics_NoLock(value, analyzers, builder);
		}
	}

	private static void AddDiagnostics_NoLock(Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder> diagnostics, ImmutableArray<DiagnosticAnalyzer> analyzers, ImmutableArray<Diagnostic>.Builder builder)
	{
		foreach (DiagnosticAnalyzer item in analyzers)
		{
			if (diagnostics.TryGetValue(item, out ImmutableArray<Diagnostic>.Builder value))
			{
				builder.AddRange(value);
			}
		}
	}

	internal AnalysisResult ToAnalysisResult(ImmutableArray<DiagnosticAnalyzer> analyzers, AnalysisScope analysisScope, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		ImmutableHashSet<DiagnosticAnalyzer> analyzers2 = analyzers.ToImmutableHashSet();
		Func<Diagnostic, bool> shouldInclude = analysisScope.ShouldInclude;
		ImmutableDictionary<SyntaxTree, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>> immutable;
		ImmutableDictionary<SyntaxTree, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>> immutable2;
		ImmutableDictionary<AdditionalText, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>> immutable3;
		ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>> immutable4;
		lock (_gate)
		{
			immutable = GetImmutable(analyzers2, shouldInclude, _localSyntaxDiagnosticsOpt);
			immutable2 = GetImmutable(analyzers2, shouldInclude, _localSemanticDiagnosticsOpt);
			immutable3 = GetImmutable(analyzers2, shouldInclude, _localAdditionalFileDiagnosticsOpt);
			immutable4 = GetImmutable(analyzers2, shouldInclude, _nonLocalDiagnosticsOpt);
		}
		cancellationToken.ThrowIfCancellationRequested();
		ImmutableDictionary<DiagnosticAnalyzer, AnalyzerTelemetryInfo> telemetryInfo = GetTelemetryInfo(analyzers);
		return new AnalysisResult(analyzers, immutable, immutable2, immutable3, immutable4, telemetryInfo);
	}

	private static ImmutableDictionary<TKey, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>> GetImmutable<TKey>(ImmutableHashSet<DiagnosticAnalyzer> analyzers, Func<Diagnostic, bool> shouldInclude, Dictionary<TKey, Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>>? localDiagnosticsOpt) where TKey : class
	{
		if (localDiagnosticsOpt == null)
		{
			return ImmutableDictionary<TKey, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>>.Empty;
		}
		ImmutableDictionary<TKey, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>>.Builder builder = ImmutableDictionary.CreateBuilder<TKey, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>>();
		ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>.Builder builder2 = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>();
		foreach (KeyValuePair<TKey, Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>> item in localDiagnosticsOpt)
		{
			TKey key = item.Key;
			foreach (KeyValuePair<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder> item2 in item.Value)
			{
				if (analyzers.Contains(item2.Key))
				{
					ImmutableArray<Diagnostic> value = item2.Value.Where(shouldInclude).ToImmutableArray();
					if (!value.IsEmpty)
					{
						builder2.Add(item2.Key, value);
					}
				}
			}
			builder.Add(key, builder2.ToImmutable());
			builder2.Clear();
		}
		return builder.ToImmutable();
	}

	private static ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>> GetImmutable(ImmutableHashSet<DiagnosticAnalyzer> analyzers, Func<Diagnostic, bool> shouldInclude, Dictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder>? nonLocalDiagnosticsOpt)
	{
		if (nonLocalDiagnosticsOpt == null)
		{
			return ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>.Empty;
		}
		ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>.Builder builder = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, ImmutableArray<Diagnostic>>();
		foreach (KeyValuePair<DiagnosticAnalyzer, ImmutableArray<Diagnostic>.Builder> item in nonLocalDiagnosticsOpt)
		{
			if (analyzers.Contains(item.Key))
			{
				ImmutableArray<Diagnostic> value = item.Value.Where(shouldInclude).ToImmutableArray();
				if (!value.IsEmpty)
				{
					builder.Add(item.Key, value);
				}
			}
		}
		return builder.ToImmutable();
	}

	private ImmutableDictionary<DiagnosticAnalyzer, AnalyzerTelemetryInfo> GetTelemetryInfo(ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		ImmutableDictionary<DiagnosticAnalyzer, AnalyzerTelemetryInfo>.Builder builder = ImmutableDictionary.CreateBuilder<DiagnosticAnalyzer, AnalyzerTelemetryInfo>();
		lock (_gate)
		{
			foreach (DiagnosticAnalyzer item in analyzers)
			{
				if (!_analyzerActionCounts.TryGetValue(item, out AnalyzerActionCounts value))
				{
					value = AnalyzerActionCounts.Empty;
				}
				int suppressionActionCounts = ((item is DiagnosticSuppressor) ? 1 : 0);
				TimeSpan executionTime = ((_analyzerExecutionTimeOpt != null) ? _analyzerExecutionTimeOpt[item] : default(TimeSpan));
				AnalyzerTelemetryInfo value2 = new AnalyzerTelemetryInfo(value, suppressionActionCounts, executionTime);
				builder.Add(item, value2);
			}
		}
		return builder.ToImmutable();
	}
}
