using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class CachingSemanticModelProvider : SemanticModelProvider
{
	private sealed class PerCompilationProvider
	{
		private readonly Compilation _compilation;

		private readonly ConcurrentDictionary<SyntaxTree, SemanticModel> _semanticModelsMap;

		private readonly Func<SyntaxTree, SemanticModel> _createSemanticModel;

		public PerCompilationProvider(Compilation compilation)
		{
			_compilation = compilation;
			_semanticModelsMap = new ConcurrentDictionary<SyntaxTree, SemanticModel>();
			_createSemanticModel = (SyntaxTree tree) => compilation.CreateSemanticModel(tree, SemanticModelOptions.None);
		}

		public SemanticModel GetSemanticModel(SyntaxTree tree, SemanticModelOptions options)
		{
			if (options != SemanticModelOptions.None)
			{
				return _compilation.CreateSemanticModel(tree, options);
			}
			return _semanticModelsMap.GetOrAdd(tree, _createSemanticModel);
		}

		public void ClearCachedSemanticModel(SyntaxTree tree)
		{
			_semanticModelsMap.TryRemove(tree, out SemanticModel _);
		}
	}

	private static readonly ConditionalWeakTable<Compilation, PerCompilationProvider>.CreateValueCallback s_createProviderCallback = (Compilation compilation) => new PerCompilationProvider(compilation);

	private static readonly ConditionalWeakTable<Compilation, PerCompilationProvider> s_providerCache = new ConditionalWeakTable<Compilation, PerCompilationProvider>();

	public static CachingSemanticModelProvider Instance { get; } = new CachingSemanticModelProvider();

	private CachingSemanticModelProvider()
	{
	}

	public override SemanticModel GetSemanticModel(SyntaxTree tree, Compilation compilation, SemanticModelOptions options = SemanticModelOptions.None)
	{
		return s_providerCache.GetValue(compilation, s_createProviderCallback).GetSemanticModel(tree, options);
	}

	internal void ClearCache(SyntaxTree tree, Compilation compilation)
	{
		if (s_providerCache.TryGetValue(compilation, out PerCompilationProvider value))
		{
			value.ClearCachedSemanticModel(tree);
		}
	}

	internal void ClearCache(Compilation compilation)
	{
		s_providerCache.Remove(compilation);
	}
}
