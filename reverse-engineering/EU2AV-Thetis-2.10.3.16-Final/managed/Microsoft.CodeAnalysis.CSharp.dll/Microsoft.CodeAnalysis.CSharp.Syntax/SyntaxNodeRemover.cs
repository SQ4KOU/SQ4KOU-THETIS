using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class SyntaxNodeRemover
{
	private sealed class SyntaxRemover : CSharpSyntaxRewriter
	{
		private readonly HashSet<SyntaxNode> _nodesToRemove;

		private readonly SyntaxRemoveOptions _options;

		private readonly TextSpan _searchSpan;

		private readonly SyntaxTriviaListBuilder _residualTrivia;

		private HashSet<SyntaxNode>? _directivesToKeep;

		internal SyntaxTriviaList ResidualTrivia
		{
			get
			{
				if (_residualTrivia != null)
				{
					return _residualTrivia.ToList();
				}
				return default(SyntaxTriviaList);
			}
		}

		public SyntaxRemover(SyntaxNode[] nodesToRemove, SyntaxRemoveOptions options)
			: base(nodesToRemove.Any((SyntaxNode n) => n.IsPartOfStructuredTrivia()))
		{
			_nodesToRemove = new HashSet<SyntaxNode>(nodesToRemove);
			_options = options;
			_searchSpan = ComputeTotalSpan(nodesToRemove);
			_residualTrivia = SyntaxTriviaListBuilder.Create();
		}

		private static TextSpan ComputeTotalSpan(SyntaxNode[] nodes)
		{
			TextSpan fullSpan = nodes[0].FullSpan;
			int num = fullSpan.Start;
			int num2 = fullSpan.End;
			for (int i = 1; i < nodes.Length; i++)
			{
				TextSpan fullSpan2 = nodes[i].FullSpan;
				num = Math.Min(num, fullSpan2.Start);
				num2 = Math.Max(num2, fullSpan2.End);
			}
			return new TextSpan(num, num2 - num);
		}

		private void AddResidualTrivia(SyntaxTriviaList trivia, bool requiresNewLine = false)
		{
			if (requiresNewLine)
			{
				AddEndOfLine(GetEndOfLine(trivia) ?? SyntaxFactory.CarriageReturnLineFeed);
			}
			_residualTrivia.Add(in trivia);
		}

		private void AddEndOfLine(SyntaxTrivia? eolTrivia)
		{
			if (eolTrivia.HasValue && (_residualTrivia.Count == 0 || !IsEndOfLine(_residualTrivia[_residualTrivia.Count - 1])))
			{
				_residualTrivia.Add(eolTrivia.Value);
			}
		}

		private static bool IsEndOfLine(SyntaxTrivia trivia)
		{
			if (trivia.Kind() != SyntaxKind.EndOfLineTrivia && trivia.Kind() != SyntaxKind.SingleLineCommentTrivia)
			{
				return trivia.IsDirective;
			}
			return true;
		}

		private static SyntaxTrivia? GetEndOfLine(SyntaxTriviaList list)
		{
			foreach (SyntaxTrivia item in list)
			{
				if (item.Kind() == SyntaxKind.EndOfLineTrivia)
				{
					return item;
				}
				if (item.IsDirective && item.GetStructure() is DirectiveTriviaSyntax { EndOfDirectiveToken: var endOfDirectiveToken })
				{
					return GetEndOfLine(endOfDirectiveToken.TrailingTrivia);
				}
			}
			return null;
		}

		private bool IsForRemoval(SyntaxNode node)
		{
			return _nodesToRemove.Contains(node);
		}

		private bool ShouldVisit(SyntaxNode node)
		{
			if (!node.FullSpan.IntersectsWith(_searchSpan))
			{
				if (_residualTrivia != null)
				{
					return _residualTrivia.Count > 0;
				}
				return false;
			}
			return true;
		}

		[return: NotNullIfNotNull("node")]
		public override SyntaxNode? Visit(SyntaxNode? node)
		{
			SyntaxNode result = node;
			if (node != null)
			{
				if (IsForRemoval(node))
				{
					AddTrivia(node);
					result = null;
				}
				else if (ShouldVisit(node))
				{
					result = base.Visit(node);
				}
			}
			return result;
		}

		public override SyntaxToken VisitToken(SyntaxToken token)
		{
			SyntaxToken syntaxToken = token;
			if (VisitIntoStructuredTrivia)
			{
				syntaxToken = base.VisitToken(token);
			}
			if (syntaxToken.Kind() != SyntaxKind.None && _residualTrivia != null && _residualTrivia.Count > 0)
			{
				_residualTrivia.Add(syntaxToken.LeadingTrivia);
				syntaxToken = syntaxToken.WithLeadingTrivia(_residualTrivia.ToList());
				_residualTrivia.Clear();
			}
			return syntaxToken;
		}

		public override SeparatedSyntaxList<TNode> VisitList<TNode>(SeparatedSyntaxList<TNode> list)
		{
			SyntaxNodeOrTokenList withSeparators = list.GetWithSeparators();
			bool flag = false;
			SyntaxNodeOrTokenListBuilder syntaxNodeOrTokenListBuilder = null;
			int i = 0;
			for (int count = withSeparators.Count; i < count; i++)
			{
				SyntaxNodeOrToken syntaxNodeOrToken = withSeparators[i];
				SyntaxNodeOrToken item;
				if (syntaxNodeOrToken.IsToken)
				{
					if (flag)
					{
						flag = false;
						item = default(SyntaxNodeOrToken);
					}
					else
					{
						item = VisitListSeparator(syntaxNodeOrToken.AsToken());
					}
				}
				else
				{
					TNode node = (TNode)syntaxNodeOrToken.AsNode();
					if (IsForRemoval(node))
					{
						if (syntaxNodeOrTokenListBuilder == null)
						{
							syntaxNodeOrTokenListBuilder = new SyntaxNodeOrTokenListBuilder(count);
							syntaxNodeOrTokenListBuilder.Add(withSeparators, 0, i);
						}
						CommonSyntaxNodeRemover.GetSeparatorInfo(withSeparators, i, 8539, out var nextTokenIsSeparator, out var nextSeparatorBelongsToNode);
						if (!nextSeparatorBelongsToNode && syntaxNodeOrTokenListBuilder.Count > 0 && syntaxNodeOrTokenListBuilder[syntaxNodeOrTokenListBuilder.Count - 1].IsToken)
						{
							SyntaxToken token = syntaxNodeOrTokenListBuilder[syntaxNodeOrTokenListBuilder.Count - 1].AsToken();
							AddTrivia(token, node);
							syntaxNodeOrTokenListBuilder.RemoveLast();
						}
						else if (nextTokenIsSeparator)
						{
							SyntaxToken token2 = withSeparators[i + 1].AsToken();
							AddTrivia(node, token2);
							flag = true;
						}
						else
						{
							AddTrivia(node);
						}
						item = default(SyntaxNodeOrToken);
					}
					else
					{
						item = VisitListElement(node);
					}
				}
				if (syntaxNodeOrToken != item && syntaxNodeOrTokenListBuilder == null)
				{
					syntaxNodeOrTokenListBuilder = new SyntaxNodeOrTokenListBuilder(count);
					syntaxNodeOrTokenListBuilder.Add(withSeparators, 0, i);
				}
				if (syntaxNodeOrTokenListBuilder != null && item.Kind() != SyntaxKind.None)
				{
					syntaxNodeOrTokenListBuilder.Add(in item);
				}
			}
			return syntaxNodeOrTokenListBuilder?.ToList().AsSeparatedList<TNode>() ?? list;
		}

		private void AddTrivia(SyntaxNode node)
		{
			if ((_options & SyntaxRemoveOptions.KeepLeadingTrivia) != SyntaxRemoveOptions.KeepNoTrivia)
			{
				AddResidualTrivia(node.GetLeadingTrivia());
			}
			else if ((_options & SyntaxRemoveOptions.KeepEndOfLine) != SyntaxRemoveOptions.KeepNoTrivia)
			{
				AddEndOfLine(GetEndOfLine(node.GetLeadingTrivia()));
			}
			if ((_options & (SyntaxRemoveOptions.KeepUnbalancedDirectives | SyntaxRemoveOptions.KeepDirectives)) != SyntaxRemoveOptions.KeepNoTrivia)
			{
				AddDirectives(node, GetRemovedSpan(node.Span, node.FullSpan));
			}
			if ((_options & SyntaxRemoveOptions.KeepTrailingTrivia) != SyntaxRemoveOptions.KeepNoTrivia)
			{
				AddResidualTrivia(node.GetTrailingTrivia());
			}
			else if ((_options & SyntaxRemoveOptions.KeepEndOfLine) != SyntaxRemoveOptions.KeepNoTrivia)
			{
				AddEndOfLine(GetEndOfLine(node.GetTrailingTrivia()));
			}
			if ((_options & SyntaxRemoveOptions.AddElasticMarker) != SyntaxRemoveOptions.KeepNoTrivia)
			{
				AddResidualTrivia(SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));
			}
		}

		private void AddTrivia(SyntaxToken token, SyntaxNode node)
		{
			if ((_options & SyntaxRemoveOptions.KeepLeadingTrivia) != SyntaxRemoveOptions.KeepNoTrivia)
			{
				AddResidualTrivia(token.LeadingTrivia);
				AddResidualTrivia(token.TrailingTrivia);
				AddResidualTrivia(node.GetLeadingTrivia());
			}
			else if ((_options & SyntaxRemoveOptions.KeepEndOfLine) != SyntaxRemoveOptions.KeepNoTrivia)
			{
				SyntaxTrivia? eolTrivia = GetEndOfLine(token.LeadingTrivia) ?? GetEndOfLine(token.TrailingTrivia);
				AddEndOfLine(eolTrivia);
			}
			if ((_options & (SyntaxRemoveOptions.KeepUnbalancedDirectives | SyntaxRemoveOptions.KeepDirectives)) != SyntaxRemoveOptions.KeepNoTrivia)
			{
				TextSpan span = TextSpan.FromBounds(token.Span.Start, node.Span.End);
				TextSpan fullSpan = TextSpan.FromBounds(token.FullSpan.Start, node.FullSpan.End);
				AddDirectives(node.Parent, GetRemovedSpan(span, fullSpan));
			}
			if ((_options & SyntaxRemoveOptions.KeepTrailingTrivia) != SyntaxRemoveOptions.KeepNoTrivia)
			{
				AddResidualTrivia(node.GetTrailingTrivia());
			}
			else if ((_options & SyntaxRemoveOptions.KeepEndOfLine) != SyntaxRemoveOptions.KeepNoTrivia)
			{
				AddEndOfLine(GetEndOfLine(node.GetTrailingTrivia()));
			}
			if ((_options & SyntaxRemoveOptions.AddElasticMarker) != SyntaxRemoveOptions.KeepNoTrivia)
			{
				AddResidualTrivia(SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));
			}
		}

		private void AddTrivia(SyntaxNode node, SyntaxToken token)
		{
			if ((_options & SyntaxRemoveOptions.KeepLeadingTrivia) != SyntaxRemoveOptions.KeepNoTrivia)
			{
				AddResidualTrivia(node.GetLeadingTrivia());
			}
			else if ((_options & SyntaxRemoveOptions.KeepEndOfLine) != SyntaxRemoveOptions.KeepNoTrivia)
			{
				AddEndOfLine(GetEndOfLine(node.GetLeadingTrivia()));
			}
			if ((_options & (SyntaxRemoveOptions.KeepUnbalancedDirectives | SyntaxRemoveOptions.KeepDirectives)) != SyntaxRemoveOptions.KeepNoTrivia)
			{
				TextSpan span = TextSpan.FromBounds(node.Span.Start, token.Span.End);
				TextSpan fullSpan = TextSpan.FromBounds(node.FullSpan.Start, token.FullSpan.End);
				AddDirectives(node.Parent, GetRemovedSpan(span, fullSpan));
			}
			if ((_options & SyntaxRemoveOptions.KeepTrailingTrivia) != SyntaxRemoveOptions.KeepNoTrivia)
			{
				AddResidualTrivia(node.GetTrailingTrivia());
				AddResidualTrivia(token.LeadingTrivia);
				AddResidualTrivia(token.TrailingTrivia);
			}
			else if ((_options & SyntaxRemoveOptions.KeepEndOfLine) != SyntaxRemoveOptions.KeepNoTrivia)
			{
				SyntaxTrivia? eolTrivia = GetEndOfLine(node.GetTrailingTrivia()) ?? GetEndOfLine(token.TrailingTrivia);
				AddEndOfLine(eolTrivia);
			}
			if ((_options & SyntaxRemoveOptions.AddElasticMarker) != SyntaxRemoveOptions.KeepNoTrivia)
			{
				AddResidualTrivia(SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));
			}
		}

		private TextSpan GetRemovedSpan(TextSpan span, TextSpan fullSpan)
		{
			TextSpan result = fullSpan;
			if ((_options & SyntaxRemoveOptions.KeepLeadingTrivia) != SyntaxRemoveOptions.KeepNoTrivia)
			{
				result = TextSpan.FromBounds(span.Start, result.End);
			}
			if ((_options & SyntaxRemoveOptions.KeepTrailingTrivia) != SyntaxRemoveOptions.KeepNoTrivia)
			{
				result = TextSpan.FromBounds(result.Start, span.End);
			}
			return result;
		}

		private void AddDirectives(SyntaxNode node, TextSpan span)
		{
			if (!node.ContainsDirectives)
			{
				return;
			}
			if (_directivesToKeep == null)
			{
				_directivesToKeep = new HashSet<SyntaxNode>();
			}
			else
			{
				_directivesToKeep.Clear();
			}
			foreach (DirectiveTriviaSyntax item in from tr in node.DescendantTrivia(span, (SyntaxNode n) => n.ContainsDirectives, descendIntoTrivia: true)
				where tr.IsDirective
				select (DirectiveTriviaSyntax)tr.GetStructure())
			{
				if ((_options & SyntaxRemoveOptions.KeepDirectives) != SyntaxRemoveOptions.KeepNoTrivia)
				{
					_directivesToKeep.Add(item);
				}
				else if (item.Kind() == SyntaxKind.DefineDirectiveTrivia || item.Kind() == SyntaxKind.UndefDirectiveTrivia)
				{
					_directivesToKeep.Add(item);
				}
				else if (HasRelatedDirectives(item))
				{
					List<DirectiveTriviaSyntax> relatedDirectives = item.GetRelatedDirectives();
					if (!relatedDirectives.All((DirectiveTriviaSyntax rd) => rd.FullSpan.OverlapsWith(span)))
					{
						foreach (DirectiveTriviaSyntax item2 in relatedDirectives.Where((DirectiveTriviaSyntax rd) => rd.FullSpan.OverlapsWith(span)))
						{
							_directivesToKeep.Add(item2);
						}
					}
				}
				if (!_directivesToKeep.Contains(item))
				{
					continue;
				}
				SyntaxTrivia parentTrivia = item.ParentTrivia;
				var (syntaxTriviaList, num) = getTriviaListAndIndex(parentTrivia);
				if (num >= 1)
				{
					SyntaxTrivia syntaxTrivia = syntaxTriviaList[num - 1];
					if (syntaxTrivia.RawKind == 8540)
					{
						AddResidualTrivia(SyntaxFactory.TriviaList(syntaxTrivia, parentTrivia), requiresNewLine: true);
						continue;
					}
				}
				AddResidualTrivia(SyntaxFactory.TriviaList(parentTrivia), requiresNewLine: true);
			}
			static (SyntaxTriviaList triviaList, int index) getTriviaListAndIndex(SyntaxTrivia trivia)
			{
				SyntaxToken token = trivia.Token;
				int num2 = token.LeadingTrivia.IndexOf(trivia);
				if (num2 < 0)
				{
					return (triviaList: token.TrailingTrivia, index: token.TrailingTrivia.IndexOf(trivia));
				}
				return (triviaList: token.LeadingTrivia, index: num2);
			}
		}

		private static bool HasRelatedDirectives(DirectiveTriviaSyntax directive)
		{
			SyntaxKind syntaxKind = directive.Kind();
			if (syntaxKind - 8548 <= (SyntaxKind)5)
			{
				return true;
			}
			return false;
		}
	}

	internal static TRoot? RemoveNodes<TRoot>(TRoot root, IEnumerable<SyntaxNode> nodes, SyntaxRemoveOptions options) where TRoot : SyntaxNode
	{
		if (nodes == null)
		{
			return root;
		}
		if (nodes.ToArray().Length == 0)
		{
			return root;
		}
		SyntaxRemover syntaxRemover = new SyntaxRemover(nodes.ToArray(), options);
		SyntaxNode syntaxNode = syntaxRemover.Visit(root);
		SyntaxTriviaList residualTrivia = syntaxRemover.ResidualTrivia;
		if (syntaxNode != null && residualTrivia.Count > 0)
		{
			syntaxNode = syntaxNode.WithTrailingTrivia(syntaxNode.GetTrailingTrivia().Concat(residualTrivia));
		}
		return (TRoot)syntaxNode;
	}
}
