using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.SourceGeneration;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public readonly struct SyntaxValueProvider
{
	private readonly IncrementalGeneratorInitializationContext _context;

	private readonly ArrayBuilder<SyntaxInputNode> _inputNodes;

	private readonly Action<IIncrementalGeneratorOutputNode> _registerOutput;

	private readonly ISyntaxHelper _syntaxHelper;

	private static readonly char[] s_nestedTypeNameSeparators = new char[1] { '+' };

	private static readonly SymbolDisplayFormat s_metadataDisplayFormat = SymbolDisplayFormat.QualifiedNameArityFormat.AddCompilerInternalOptions(SymbolDisplayCompilerInternalOptions.UsePlusForNestedTypes);

	private static readonly ObjectPool<Stack<string>> s_stringStackPool = new ObjectPool<Stack<string>>(() => new Stack<string>());

	private static readonly ObjectPool<Stack<SyntaxNode>> s_nodeStackPool = new ObjectPool<Stack<SyntaxNode>>(() => new Stack<SyntaxNode>());

	internal SyntaxValueProvider(IncrementalGeneratorInitializationContext context, ArrayBuilder<SyntaxInputNode> inputNodes, Action<IIncrementalGeneratorOutputNode> registerOutput, ISyntaxHelper syntaxHelper)
	{
		_context = context;
		_inputNodes = inputNodes;
		_registerOutput = registerOutput;
		_syntaxHelper = syntaxHelper;
	}

	public IncrementalValuesProvider<T> CreateSyntaxProvider<T>(Func<SyntaxNode, CancellationToken, bool> predicate, Func<GeneratorSyntaxContext, CancellationToken, T> transform)
	{
		return new IncrementalValuesProvider<T>(new SyntaxInputNode<T>(new PredicateSyntaxStrategy<T>(predicate.WrapUserFunction(_context.CatchAnalyzerExceptions), transform.WrapUserFunction(_context.CatchAnalyzerExceptions), _syntaxHelper), RegisterOutputAndDeferredInput), _context.CatchAnalyzerExceptions);
	}

	internal IncrementalValueProvider<ISyntaxContextReceiver?> CreateSyntaxReceiverProvider(SyntaxContextReceiverCreator creator)
	{
		SyntaxInputNode<ISyntaxContextReceiver> syntaxInputNode = new SyntaxInputNode<ISyntaxContextReceiver>(new SyntaxReceiverStrategy<ISyntaxContextReceiver>(creator, _registerOutput, _syntaxHelper), RegisterOutputAndDeferredInput);
		_inputNodes.Add(syntaxInputNode);
		return new IncrementalValueProvider<ISyntaxContextReceiver>(syntaxInputNode, _context.CatchAnalyzerExceptions);
	}

	private void RegisterOutputAndDeferredInput(SyntaxInputNode node, IIncrementalGeneratorOutputNode output)
	{
		_registerOutput(output);
		if (!_inputNodes.Contains(node))
		{
			_inputNodes.Add(node);
		}
	}

	public IncrementalValuesProvider<T> ForAttributeWithMetadataName<T>(string fullyQualifiedMetadataName, Func<SyntaxNode, CancellationToken, bool> predicate, Func<GeneratorAttributeSyntaxContext, CancellationToken, T> transform)
	{
		IncrementalValuesProvider<((SyntaxTree tree, ImmutableArray<SyntaxNode> matches) Left, Compilation Right)> source = ForAttributeWithSimpleName((Enumerable.Contains(fullyQualifiedMetadataName, '+') ? MetadataTypeName.FromFullName(fullyQualifiedMetadataName.Split(s_nestedTypeNameSeparators).Last()) : MetadataTypeName.FromFullName(fullyQualifiedMetadataName)).UnmangledTypeName, predicate).Combine(_context.CompilationProvider).WithTrackingName("compilationAndGroupedNodes_ForAttributeWithMetadataName");
		ISyntaxHelper syntaxHelper = _context.SyntaxHelper;
		return source.SelectMany<((SyntaxTree, ImmutableArray<SyntaxNode>), Compilation), T>(delegate(((SyntaxTree tree, ImmutableArray<SyntaxNode> matches) Left, Compilation Right) tuple, CancellationToken cancellationToken)
		{
			((SyntaxTree tree, ImmutableArray<SyntaxNode> matches) Left, Compilation Right) tuple2 = tuple;
			(SyntaxTree tree, ImmutableArray<SyntaxNode> matches) item = tuple2.Left;
			SyntaxTree item2 = item.tree;
			ImmutableArray<SyntaxNode> item3 = item.matches;
			Compilation item4 = tuple2.Right;
			ArrayBuilder<T> instance = ArrayBuilder<T>.GetInstance();
			try
			{
				if (!item3.IsEmpty)
				{
					SemanticModel semanticModel = item4.GetSemanticModel(item2);
					foreach (SyntaxNode item5 in item3)
					{
						cancellationToken.ThrowIfCancellationRequested();
						object obj;
						if (!(item5 is ICompilationUnitSyntax))
						{
							obj = (syntaxHelper.IsLambdaExpression(item5) ? semanticModel.GetSymbolInfo(item5, cancellationToken).Symbol : semanticModel.GetDeclaredSymbol(item5, cancellationToken));
						}
						else
						{
							ISymbol assembly = semanticModel.Compilation.Assembly;
							obj = assembly;
						}
						ISymbol symbol = (ISymbol)obj;
						if (symbol != null)
						{
							ImmutableArray<AttributeData> attributes = getMatchingAttributes(syntaxHelper, item5, symbol, fullyQualifiedMetadataName, cancellationToken);
							if (attributes.Length > 0)
							{
								instance.Add(transform(new GeneratorAttributeSyntaxContext(item5, symbol, semanticModel, attributes), cancellationToken));
							}
						}
					}
				}
				return instance.ToImmutableAndClear();
			}
			finally
			{
				instance.Free();
			}
		}).WithTrackingName("result_ForAttributeWithMetadataName");
		static ImmutableArray<AttributeData> getMatchingAttributes(ISyntaxHelper syntaxHelper2, SyntaxNode attributeTarget, ISymbol symbol, string text, CancellationToken cancellationToken)
		{
			SyntaxTree targetSyntaxTree = attributeTarget.SyntaxTree;
			ArrayBuilder<AttributeData> result = ArrayBuilder<AttributeData>.GetInstance();
			SyntaxNode remappedTarget = syntaxHelper2.RemapAttributeTarget(attributeTarget);
			addMatchingAttributes(symbol.GetAttributes());
			addMatchingAttributes((symbol as IMethodSymbol)?.GetReturnTypeAttributes());
			if (symbol is IAssemblySymbol assemblySymbol)
			{
				foreach (IModuleSymbol module in assemblySymbol.Modules)
				{
					addMatchingAttributes(module.GetAttributes());
				}
			}
			return result.ToImmutableAndFree();
			void addMatchingAttributes(ImmutableArray<AttributeData>? attributes)
			{
				if (attributes.HasValue)
				{
					foreach (AttributeData item6 in attributes.Value)
					{
						if (item6.ApplicationSyntaxReference?.SyntaxTree == targetSyntaxTree && item6.AttributeClass?.ToDisplayString(s_metadataDisplayFormat) == text)
						{
							SyntaxNode syntax = item6.ApplicationSyntaxReference.GetSyntax(cancellationToken);
							if (syntaxHelper2.GetAttributeOwningNode(syntax) == remappedTarget)
							{
								result.Add(item6);
							}
						}
					}
				}
			}
		}
	}

	internal IncrementalValuesProvider<(SyntaxTree tree, ImmutableArray<SyntaxNode> matches)> ForAttributeWithSimpleName(string simpleName, Func<SyntaxNode, CancellationToken, bool> predicate)
	{
		ISyntaxHelper syntaxHelper = _context.SyntaxHelper;
		IncrementalValuesProvider<(SyntaxTree Tree, SourceGeneratorSyntaxTreeInfo Info)> source = _context.CompilationProvider.SelectMany((Compilation compilation, CancellationToken cancellationToken) => GetSourceGeneratorInfo(syntaxHelper, compilation, cancellationToken)).WithTrackingName("compilationUnit_ForAttribute");
		IncrementalValueProvider<GlobalAliases> provider = source.Where<(SyntaxTree, SourceGeneratorSyntaxTreeInfo)>(((SyntaxTree Tree, SourceGeneratorSyntaxTreeInfo Info) info, CancellationToken _) => info.Info.HasFlag(SourceGeneratorSyntaxTreeInfo.ContainsGlobalAliases)).Select<(SyntaxTree, SourceGeneratorSyntaxTreeInfo), GlobalAliases>(((SyntaxTree Tree, SourceGeneratorSyntaxTreeInfo Info) info, CancellationToken cancellationToken) => getGlobalAliasesInCompilationUnit(syntaxHelper, info.Tree.GetRoot(cancellationToken))).WithTrackingName("individualFileGlobalAliases_ForAttribute")
			.Collect()
			.WithTrackingName("collectedGlobalAliases_ForAttribute")
			.Select((ImmutableArray<GlobalAliases> arrays, CancellationToken _) => GlobalAliases.Create(arrays))
			.WithTrackingName("allUpGlobalAliases_ForAttribute");
		IncrementalValueProvider<GlobalAliases> provider2 = _context.CompilationOptionsProvider.Select(delegate(CompilationOptions o, CancellationToken _)
		{
			ArrayBuilder<(string, string)> instance = ArrayBuilder<(string, string)>.GetInstance();
			syntaxHelper.AddAliases(o, instance);
			return GlobalAliases.Create(instance.ToImmutableAndFree());
		}).WithTrackingName("compilationGlobalAliases_ForAttribute");
		return (from tuple in IncrementalValueProviderExtensions.Combine(provider2: provider.Combine(provider2).Select(((GlobalAliases Left, GlobalAliases Right) tuple, CancellationToken _) => GlobalAliases.Concat(tuple.Left, tuple.Right)).WithTrackingName("allUpIncludingCompilationGlobalAliases_ForAttribute"), provider1: source.Where<(SyntaxTree, SourceGeneratorSyntaxTreeInfo)>(((SyntaxTree Tree, SourceGeneratorSyntaxTreeInfo Info) info, CancellationToken _) => info.Info.HasFlag(SourceGeneratorSyntaxTreeInfo.ContainsAttributeList))).WithTrackingName("compilationUnitAndGlobalAliases_ForAttribute").Select<((SyntaxTree, SourceGeneratorSyntaxTreeInfo), GlobalAliases), (SyntaxTree, ImmutableArray<SyntaxNode>)>((((SyntaxTree Tree, SourceGeneratorSyntaxTreeInfo Info) Left, GlobalAliases Right) tuple, CancellationToken c) => (Tree: tuple.Left.Tree, GetMatchingNodes(syntaxHelper, tuple.Right, tuple.Left.Tree, simpleName, predicate, c)))
			where tuple.Item2.Length > 0
			select tuple).WithTrackingName("result_ForAttributeInternal");
		static GlobalAliases getGlobalAliasesInCompilationUnit(ISyntaxHelper syntaxHelper2, SyntaxNode compilationUnit)
		{
			ArrayBuilder<(string, string)> instance = ArrayBuilder<(string, string)>.GetInstance();
			syntaxHelper2.AddAliases(compilationUnit.Green, instance, global: true);
			return GlobalAliases.Create(instance.ToImmutableAndFree());
		}
	}

	private static ImmutableArray<(SyntaxTree Tree, SourceGeneratorSyntaxTreeInfo Info)> GetSourceGeneratorInfo(ISyntaxHelper syntaxHelper, Compilation compilation, CancellationToken cancellationToken)
	{
		int num = 0;
		foreach (SyntaxTree commonSyntaxTree in compilation.CommonSyntaxTrees)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if ((commonSyntaxTree.GetSourceGeneratorInfo(syntaxHelper, cancellationToken) & SourceGeneratorSyntaxTreeInfo.ContainsGlobalAliasesOrAttributeList) != SourceGeneratorSyntaxTreeInfo.NotComputedYet)
			{
				num++;
			}
		}
		ImmutableArray<(SyntaxTree, SourceGeneratorSyntaxTreeInfo)>.Builder builder = ImmutableArray.CreateBuilder<(SyntaxTree, SourceGeneratorSyntaxTreeInfo)>(num);
		foreach (SyntaxTree commonSyntaxTree2 in compilation.CommonSyntaxTrees)
		{
			SourceGeneratorSyntaxTreeInfo sourceGeneratorInfo = commonSyntaxTree2.GetSourceGeneratorInfo(syntaxHelper, cancellationToken);
			if ((sourceGeneratorInfo & SourceGeneratorSyntaxTreeInfo.ContainsGlobalAliasesOrAttributeList) != SourceGeneratorSyntaxTreeInfo.NotComputedYet)
			{
				builder.Add((commonSyntaxTree2, sourceGeneratorInfo));
			}
		}
		return builder.MoveToImmutable();
	}

	private static ImmutableArray<SyntaxNode> GetMatchingNodes(ISyntaxHelper syntaxHelper, GlobalAliases globalAliases, SyntaxTree syntaxTree, string name, Func<SyntaxNode, CancellationToken, bool> predicate, CancellationToken cancellationToken)
	{
		SyntaxNode root = syntaxTree.GetRoot(cancellationToken);
		bool isCaseSensitive = syntaxHelper.IsCaseSensitive;
		StringComparison comparison = (isCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
		ArrayBuilder<(string aliasName, string symbolName)> localAliases = ArrayBuilder<(string, string)>.GetInstance();
		bool nameHasAttributeSuffix = name.HasAttributeSuffix(isCaseSensitive);
		Stack<string> seenNames = s_stringStackPool.Allocate();
		ArrayBuilder<SyntaxNode> results = ArrayBuilder<SyntaxNode>.GetInstance();
		ArrayBuilder<SyntaxNode> attributeTargets = ArrayBuilder<SyntaxNode>.GetInstance();
		try
		{
			processCompilationUnit(root);
		}
		finally
		{
			localAliases.Free();
			seenNames.Clear();
			s_stringStackPool.Free(seenNames);
			attributeTargets.Free();
		}
		results.RemoveDuplicates();
		return results.ToImmutableAndFree();
		bool matchesAttributeName(string currentAttributeName, bool withAttributeSuffix)
		{
			if (withAttributeSuffix)
			{
				if (nameHasAttributeSuffix && matchesName(currentAttributeName, name, withAttributeSuffix))
				{
					return true;
				}
			}
			else if (matchesName(currentAttributeName, name, withAttributeSuffix: false))
			{
				return true;
			}
			if (seenNames.Contains(currentAttributeName))
			{
				return false;
			}
			seenNames.Push(currentAttributeName);
			try
			{
				foreach (var (matchAgainst, currentAttributeName2) in localAliases)
				{
					if (matchesName(currentAttributeName, matchAgainst, withAttributeSuffix) && matchesAttributeName(currentAttributeName2, withAttributeSuffix: false))
					{
						return true;
					}
				}
				foreach (var (matchAgainst2, currentAttributeName3) in globalAliases.AliasAndSymbolNames)
				{
					if (matchesName(currentAttributeName, matchAgainst2, withAttributeSuffix) && matchesAttributeName(currentAttributeName3, withAttributeSuffix: false))
					{
						return true;
					}
				}
				return false;
			}
			finally
			{
				seenNames.Pop();
			}
		}
		bool matchesName(string text, string matchAgainst, bool withAttributeSuffix)
		{
			if (withAttributeSuffix)
			{
				if (text.Length + "Attribute".Length == matchAgainst.Length && matchAgainst.HasAttributeSuffix(isCaseSensitive))
				{
					return matchAgainst.StartsWith(text, comparison);
				}
				return false;
			}
			return text.Equals(matchAgainst, comparison);
		}
		void processCompilationOrNamespaceMembers(SyntaxNode node)
		{
			cancellationToken.ThrowIfCancellationRequested();
			foreach (SyntaxNodeOrToken item in node.ChildNodesAndTokens())
			{
				if (item.AsNode(out SyntaxNode node2))
				{
					if (syntaxHelper.IsAnyNamespaceBlock(node2))
					{
						processNamespaceBlock(node2);
					}
					else
					{
						processMember(node2);
					}
				}
			}
		}
		void processCompilationUnit(SyntaxNode compilationUnit)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (compilationUnit is ICompilationUnitSyntax)
			{
				syntaxHelper.AddAliases(compilationUnit.Green, localAliases, global: false);
			}
			processCompilationOrNamespaceMembers(compilationUnit);
		}
		void processMember(SyntaxNode member)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (!member.ContainsAttributes)
			{
				return;
			}
			Stack<SyntaxNode> stack = s_nodeStackPool.Allocate();
			stack.Push(member);
			try
			{
				while (stack.Count > 0)
				{
					SyntaxNode syntaxNode = stack.Pop();
					if (syntaxNode.ContainsAttributes)
					{
						if (syntaxHelper.IsAttributeList(syntaxNode))
						{
							foreach (SyntaxNode attributesOfAttribute in syntaxHelper.GetAttributesOfAttributeList(syntaxNode))
							{
								string unqualifiedIdentifierOfName = syntaxHelper.GetUnqualifiedIdentifierOfName(syntaxHelper.GetNameOfAttribute(attributesOfAttribute));
								if (matchesAttributeName(unqualifiedIdentifierOfName, withAttributeSuffix: false) || matchesAttributeName(unqualifiedIdentifierOfName, withAttributeSuffix: true))
								{
									attributeTargets.Clear();
									syntaxHelper.AddAttributeTargets(syntaxNode, attributeTargets);
									foreach (SyntaxNode item2 in attributeTargets)
									{
										if (predicate(item2, cancellationToken))
										{
											results.Add(item2);
										}
									}
									break;
								}
							}
						}
						else
						{
							foreach (SyntaxNodeOrToken item3 in syntaxNode.ChildNodesAndTokens().Reverse())
							{
								if (item3.AsNode(out SyntaxNode node))
								{
									stack.Push(node);
								}
							}
						}
					}
				}
			}
			finally
			{
				stack.Clear();
				s_nodeStackPool.Free(stack);
			}
		}
		void processNamespaceBlock(SyntaxNode namespaceBlock)
		{
			cancellationToken.ThrowIfCancellationRequested();
			int count = localAliases.Count;
			syntaxHelper.AddAliases(namespaceBlock.Green, localAliases, global: false);
			processCompilationOrNamespaceMembers(namespaceBlock);
			localAliases.Count = count;
		}
	}
}
