using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Microsoft.CodeAnalysis;

public static class IncrementalValueProviderExtensions
{
	public static IncrementalValueProvider<TResult> Select<TSource, TResult>(this IncrementalValueProvider<TSource> source, Func<TSource, CancellationToken, TResult> selector)
	{
		return new IncrementalValueProvider<TResult>(new TransformNode<TSource, TResult>(source.Node, selector, source.CatchAnalyzerExceptions), source.CatchAnalyzerExceptions);
	}

	public static IncrementalValuesProvider<TResult> Select<TSource, TResult>(this IncrementalValuesProvider<TSource> source, Func<TSource, CancellationToken, TResult> selector)
	{
		return new IncrementalValuesProvider<TResult>(new TransformNode<TSource, TResult>(source.Node, selector, source.CatchAnalyzerExceptions), source.CatchAnalyzerExceptions);
	}

	public static IncrementalValuesProvider<TResult> SelectMany<TSource, TResult>(this IncrementalValueProvider<TSource> source, Func<TSource, CancellationToken, ImmutableArray<TResult>> selector)
	{
		return new IncrementalValuesProvider<TResult>(new TransformNode<TSource, TResult>(source.Node, selector, source.CatchAnalyzerExceptions), source.CatchAnalyzerExceptions);
	}

	public static IncrementalValuesProvider<TResult> SelectMany<TSource, TResult>(this IncrementalValueProvider<TSource> source, Func<TSource, CancellationToken, IEnumerable<TResult>> selector)
	{
		return new IncrementalValuesProvider<TResult>(new TransformNode<TSource, TResult>(source.Node, selector.WrapUserFunctionAsImmutableArray(source.CatchAnalyzerExceptions)), source.CatchAnalyzerExceptions);
	}

	public static IncrementalValuesProvider<TResult> SelectMany<TSource, TResult>(this IncrementalValuesProvider<TSource> source, Func<TSource, CancellationToken, ImmutableArray<TResult>> selector)
	{
		return new IncrementalValuesProvider<TResult>(new TransformNode<TSource, TResult>(source.Node, selector, source.CatchAnalyzerExceptions), source.CatchAnalyzerExceptions);
	}

	public static IncrementalValuesProvider<TResult> SelectMany<TSource, TResult>(this IncrementalValuesProvider<TSource> source, Func<TSource, CancellationToken, IEnumerable<TResult>> selector)
	{
		return new IncrementalValuesProvider<TResult>(new TransformNode<TSource, TResult>(source.Node, selector.WrapUserFunctionAsImmutableArray(source.CatchAnalyzerExceptions)), source.CatchAnalyzerExceptions);
	}

	public static IncrementalValueProvider<ImmutableArray<TSource>> Collect<TSource>(this IncrementalValuesProvider<TSource> source)
	{
		return new IncrementalValueProvider<ImmutableArray<TSource>>(new BatchNode<TSource>(source.Node), source.CatchAnalyzerExceptions);
	}

	public static IncrementalValuesProvider<(TLeft Left, TRight Right)> Combine<TLeft, TRight>(this IncrementalValuesProvider<TLeft> provider1, IncrementalValueProvider<TRight> provider2)
	{
		return new IncrementalValuesProvider<(TLeft, TRight)>(new CombineNode<TLeft, TRight>(provider1.Node, provider2.Node), provider1.CatchAnalyzerExceptions);
	}

	public static IncrementalValueProvider<(TLeft Left, TRight Right)> Combine<TLeft, TRight>(this IncrementalValueProvider<TLeft> provider1, IncrementalValueProvider<TRight> provider2)
	{
		return new IncrementalValueProvider<(TLeft, TRight)>(new CombineNode<TLeft, TRight>(provider1.Node, provider2.Node), provider1.CatchAnalyzerExceptions);
	}

	public static IncrementalValuesProvider<TSource> Where<TSource>(this IncrementalValuesProvider<TSource> source, Func<TSource, bool> predicate)
	{
		return source.SelectMany((TSource item, CancellationToken _) => (!predicate(item)) ? ImmutableArray<TSource>.Empty : ImmutableArray.Create(item));
	}

	internal static IncrementalValuesProvider<TSource> Where<TSource>(this IncrementalValuesProvider<TSource> source, Func<TSource, CancellationToken, bool> predicate)
	{
		return source.SelectMany((TSource item, CancellationToken c) => (!predicate(item, c)) ? ImmutableArray<TSource>.Empty : ImmutableArray.Create(item));
	}

	public static IncrementalValueProvider<TSource> WithComparer<TSource>(this IncrementalValueProvider<TSource> source, IEqualityComparer<TSource> comparer)
	{
		return new IncrementalValueProvider<TSource>(source.Node.WithComparer(comparer.WrapUserComparer(source.CatchAnalyzerExceptions)), source.CatchAnalyzerExceptions);
	}

	public static IncrementalValuesProvider<TSource> WithComparer<TSource>(this IncrementalValuesProvider<TSource> source, IEqualityComparer<TSource> comparer)
	{
		return new IncrementalValuesProvider<TSource>(source.Node.WithComparer(comparer.WrapUserComparer(source.CatchAnalyzerExceptions)), source.CatchAnalyzerExceptions);
	}

	public static IncrementalValueProvider<TSource> WithTrackingName<TSource>(this IncrementalValueProvider<TSource> source, string name)
	{
		return new IncrementalValueProvider<TSource>(source.Node.WithTrackingName(name), source.CatchAnalyzerExceptions);
	}

	public static IncrementalValuesProvider<TSource> WithTrackingName<TSource>(this IncrementalValuesProvider<TSource> source, string name)
	{
		return new IncrementalValuesProvider<TSource>(source.Node.WithTrackingName(name), source.CatchAnalyzerExceptions);
	}
}
