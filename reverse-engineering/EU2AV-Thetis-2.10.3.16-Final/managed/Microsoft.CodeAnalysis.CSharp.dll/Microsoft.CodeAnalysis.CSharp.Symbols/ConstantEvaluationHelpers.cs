using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class ConstantEvaluationHelpers
{
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	internal readonly struct FieldInfo(SourceFieldSymbolWithSyntaxReference field, bool startsCycle)
	{
		public readonly SourceFieldSymbolWithSyntaxReference Field = field;

		public readonly bool StartsCycle = startsCycle;

		private string GetDebuggerDisplay()
		{
			string text = Field.ToString();
			if (StartsCycle)
			{
				text += " [cycle]";
			}
			return text;
		}
	}

	private struct Node<T> where T : class
	{
		public ImmutableHashSet<T> Dependencies;

		public ImmutableHashSet<T> DependedOnBy;
	}

	internal static void OrderAllDependencies(this SourceFieldSymbolWithSyntaxReference field, ArrayBuilder<FieldInfo> order, bool earlyDecodingWellKnownAttributes)
	{
		PooledDictionary<SourceFieldSymbolWithSyntaxReference, Node<SourceFieldSymbolWithSyntaxReference>> instance = PooledDictionary<SourceFieldSymbolWithSyntaxReference, Node<SourceFieldSymbolWithSyntaxReference>>.GetInstance();
		CreateGraph(instance, field, earlyDecodingWellKnownAttributes);
		OrderGraph(instance, order);
		instance.Free();
	}

	private static void CreateGraph(Dictionary<SourceFieldSymbolWithSyntaxReference, Node<SourceFieldSymbolWithSyntaxReference>> graph, SourceFieldSymbolWithSyntaxReference field, bool earlyDecodingWellKnownAttributes)
	{
		ArrayBuilder<SourceFieldSymbolWithSyntaxReference> instance = ArrayBuilder<SourceFieldSymbolWithSyntaxReference>.GetInstance();
		instance.Push(field);
		while (instance.Count > 0)
		{
			field = instance.Pop();
			if (graph.TryGetValue(field, out var value))
			{
				if (value.Dependencies != null)
				{
					continue;
				}
			}
			else
			{
				value = new Node<SourceFieldSymbolWithSyntaxReference>
				{
					DependedOnBy = ImmutableHashSet<SourceFieldSymbolWithSyntaxReference>.Empty
				};
			}
			ImmutableHashSet<SourceFieldSymbolWithSyntaxReference> immutableHashSet = (value.Dependencies = field.GetConstantValueDependencies(earlyDecodingWellKnownAttributes));
			graph[field] = value;
			foreach (SourceFieldSymbolWithSyntaxReference item in immutableHashSet)
			{
				instance.Push(item);
				if (!graph.TryGetValue(item, out value))
				{
					value = new Node<SourceFieldSymbolWithSyntaxReference>
					{
						DependedOnBy = ImmutableHashSet<SourceFieldSymbolWithSyntaxReference>.Empty
					};
				}
				value.DependedOnBy = value.DependedOnBy.Add(field);
				graph[item] = value;
			}
		}
		instance.Free();
	}

	private static void OrderGraph(Dictionary<SourceFieldSymbolWithSyntaxReference, Node<SourceFieldSymbolWithSyntaxReference>> graph, ArrayBuilder<FieldInfo> order)
	{
		PooledHashSet<SourceFieldSymbolWithSyntaxReference> pooledHashSet = null;
		ArrayBuilder<SourceFieldSymbolWithSyntaxReference> fieldsInvolvedInCycles = null;
		while (graph.Count > 0)
		{
			IEnumerable<SourceFieldSymbolWithSyntaxReference> enumerable = pooledHashSet;
			IEnumerable<SourceFieldSymbolWithSyntaxReference> obj = enumerable ?? graph.Keys;
			ArrayBuilder<SourceFieldSymbolWithSyntaxReference> instance = ArrayBuilder<SourceFieldSymbolWithSyntaxReference>.GetInstance();
			foreach (SourceFieldSymbolWithSyntaxReference item in obj)
			{
				if (graph.TryGetValue(item, out var value) && value.Dependencies.Count == 0)
				{
					instance.Add(item);
				}
			}
			pooledHashSet?.Free();
			pooledHashSet = null;
			if (instance.Count > 0)
			{
				PooledHashSet<SourceFieldSymbolWithSyntaxReference> instance2 = PooledHashSet<SourceFieldSymbolWithSyntaxReference>.GetInstance();
				foreach (SourceFieldSymbolWithSyntaxReference item2 in instance)
				{
					foreach (SourceFieldSymbolWithSyntaxReference item3 in graph[item2].DependedOnBy)
					{
						Node<SourceFieldSymbolWithSyntaxReference> value2 = graph[item3];
						value2.Dependencies = value2.Dependencies.Remove(item2);
						graph[item3] = value2;
						instance2.Add(item3);
					}
					graph.Remove(item2);
				}
				foreach (SourceFieldSymbolWithSyntaxReference item4 in instance)
				{
					order.Add(new FieldInfo(item4, startsCycle: false));
				}
				pooledHashSet = instance2;
			}
			else
			{
				SourceFieldSymbolWithSyntaxReference startOfFirstCycle = GetStartOfFirstCycle(graph, ref fieldsInvolvedInCycles);
				foreach (SourceFieldSymbolWithSyntaxReference dependency in graph[startOfFirstCycle].Dependencies)
				{
					Node<SourceFieldSymbolWithSyntaxReference> value3 = graph[dependency];
					value3.DependedOnBy = value3.DependedOnBy.Remove(startOfFirstCycle);
					graph[dependency] = value3;
				}
				Node<SourceFieldSymbolWithSyntaxReference> node = graph[startOfFirstCycle];
				PooledHashSet<SourceFieldSymbolWithSyntaxReference> instance3 = PooledHashSet<SourceFieldSymbolWithSyntaxReference>.GetInstance();
				foreach (SourceFieldSymbolWithSyntaxReference item5 in node.DependedOnBy)
				{
					Node<SourceFieldSymbolWithSyntaxReference> value4 = graph[item5];
					value4.Dependencies = value4.Dependencies.Remove(startOfFirstCycle);
					graph[item5] = value4;
					instance3.Add(item5);
				}
				graph.Remove(startOfFirstCycle);
				order.Add(new FieldInfo(startOfFirstCycle, startsCycle: true));
				pooledHashSet = instance3;
			}
			instance.Free();
		}
		pooledHashSet?.Free();
		fieldsInvolvedInCycles?.Free();
	}

	private static SourceFieldSymbolWithSyntaxReference GetStartOfFirstCycle(Dictionary<SourceFieldSymbolWithSyntaxReference, Node<SourceFieldSymbolWithSyntaxReference>> graph, ref ArrayBuilder<SourceFieldSymbolWithSyntaxReference> fieldsInvolvedInCycles)
	{
		if (fieldsInvolvedInCycles == null)
		{
			fieldsInvolvedInCycles = ArrayBuilder<SourceFieldSymbolWithSyntaxReference>.GetInstance(graph.Count);
			fieldsInvolvedInCycles.AddRange((from f in graph.Keys
				group f by f.DeclaringCompilation).SelectMany((IGrouping<CSharpCompilation, SourceFieldSymbolWithSyntaxReference> g) => g.OrderByDescending((SourceFieldSymbolWithSyntaxReference f1, SourceFieldSymbolWithSyntaxReference f2) => g.Key.CompareSourceLocations(f1.ErrorLocation, f2.ErrorLocation))));
		}
		SourceFieldSymbolWithSyntaxReference sourceFieldSymbolWithSyntaxReference;
		do
		{
			sourceFieldSymbolWithSyntaxReference = fieldsInvolvedInCycles.Pop();
		}
		while (!graph.ContainsKey(sourceFieldSymbolWithSyntaxReference) || !IsPartOfCycle(graph, sourceFieldSymbolWithSyntaxReference));
		return sourceFieldSymbolWithSyntaxReference;
	}

	private static bool IsPartOfCycle(Dictionary<SourceFieldSymbolWithSyntaxReference, Node<SourceFieldSymbolWithSyntaxReference>> graph, SourceFieldSymbolWithSyntaxReference field)
	{
		PooledHashSet<SourceFieldSymbolWithSyntaxReference> instance = PooledHashSet<SourceFieldSymbolWithSyntaxReference>.GetInstance();
		ArrayBuilder<SourceFieldSymbolWithSyntaxReference> instance2 = ArrayBuilder<SourceFieldSymbolWithSyntaxReference>.GetInstance();
		SourceFieldSymbolWithSyntaxReference item = field;
		bool result = false;
		instance2.Push(field);
		while (instance2.Count > 0)
		{
			field = instance2.Pop();
			Node<SourceFieldSymbolWithSyntaxReference> node = graph[field];
			if (node.Dependencies.Contains(item))
			{
				result = true;
				break;
			}
			foreach (SourceFieldSymbolWithSyntaxReference dependency in node.Dependencies)
			{
				if (instance.Add(dependency))
				{
					instance2.Push(dependency);
				}
			}
		}
		instance2.Free();
		instance.Free();
		return result;
	}

	[Conditional("DEBUG")]
	private static void CheckGraph(Dictionary<SourceFieldSymbolWithSyntaxReference, Node<SourceFieldSymbolWithSyntaxReference>> graph)
	{
		int num = 10;
		foreach (KeyValuePair<SourceFieldSymbolWithSyntaxReference, Node<SourceFieldSymbolWithSyntaxReference>> item in graph)
		{
			_ = item.Key;
			Node<SourceFieldSymbolWithSyntaxReference> value = item.Value;
			foreach (SourceFieldSymbolWithSyntaxReference dependency in value.Dependencies)
			{
				graph.TryGetValue(dependency, out var _);
			}
			foreach (SourceFieldSymbolWithSyntaxReference item2 in value.DependedOnBy)
			{
				graph.TryGetValue(item2, out var _);
			}
			num--;
			if (num == 0)
			{
				break;
			}
		}
	}
}
