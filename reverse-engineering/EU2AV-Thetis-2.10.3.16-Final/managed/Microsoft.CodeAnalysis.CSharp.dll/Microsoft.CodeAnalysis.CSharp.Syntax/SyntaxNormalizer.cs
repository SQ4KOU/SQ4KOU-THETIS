using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

internal class SyntaxNormalizer : CSharpSyntaxRewriter
{
	private readonly TextSpan _consideredSpan;

	private readonly int _initialDepth;

	private readonly string _indentWhitespace;

	private readonly bool _useElasticTrivia;

	private readonly SyntaxTrivia _eolTrivia;

	private bool _isInStructuredTrivia;

	private SyntaxToken _previousToken;

	private bool _afterLineBreak;

	private bool _afterIndentation;

	private bool _inSingleLineInterpolation;

	private ArrayBuilder<SyntaxTrivia>? _indentations;

	private static readonly SyntaxTrivia s_trimmedDocCommentExterior = SyntaxFactory.DocumentationCommentExterior("///");

	private SyntaxNormalizer(TextSpan consideredSpan, int initialDepth, string indentWhitespace, string eolWhitespace, bool useElasticTrivia)
		: base(visitIntoStructuredTrivia: true)
	{
		_consideredSpan = consideredSpan;
		_initialDepth = initialDepth;
		_indentWhitespace = indentWhitespace;
		_useElasticTrivia = useElasticTrivia;
		_eolTrivia = (useElasticTrivia ? SyntaxFactory.ElasticEndOfLine(eolWhitespace) : SyntaxFactory.EndOfLine(eolWhitespace));
		_afterLineBreak = true;
	}

	internal static TNode Normalize<TNode>(TNode node, string indentWhitespace, string eolWhitespace, bool useElasticTrivia = false) where TNode : SyntaxNode
	{
		SyntaxNormalizer syntaxNormalizer = new SyntaxNormalizer(node.FullSpan, GetDeclarationDepth(node), indentWhitespace, eolWhitespace, useElasticTrivia);
		TNode result = (TNode)syntaxNormalizer.Visit(node);
		syntaxNormalizer.Free();
		return result;
	}

	internal static SyntaxToken Normalize(SyntaxToken token, string indentWhitespace, string eolWhitespace, bool useElasticTrivia = false)
	{
		SyntaxNormalizer syntaxNormalizer = new SyntaxNormalizer(token.FullSpan, GetDeclarationDepth(token), indentWhitespace, eolWhitespace, useElasticTrivia);
		SyntaxToken result = syntaxNormalizer.VisitToken(token);
		syntaxNormalizer.Free();
		return result;
	}

	internal static SyntaxTriviaList Normalize(SyntaxTriviaList trivia, string indentWhitespace, string eolWhitespace, bool useElasticTrivia = false)
	{
		SyntaxNormalizer syntaxNormalizer = new SyntaxNormalizer(trivia.FullSpan, GetDeclarationDepth(trivia.Token), indentWhitespace, eolWhitespace, useElasticTrivia);
		SyntaxTriviaList result = syntaxNormalizer.RewriteTrivia(trivia, GetDeclarationDepth(trivia.ElementAt(0).Token), isTrailing: false, indentAfterLineBreak: false, mustHaveSeparator: false, 0);
		syntaxNormalizer.Free();
		return result;
	}

	private void Free()
	{
		if (_indentations != null)
		{
			_indentations.Free();
		}
	}

	public override SyntaxToken VisitToken(SyntaxToken token)
	{
		if (token.Kind() == SyntaxKind.None || (token.IsMissing && token.FullWidth == 0))
		{
			return token;
		}
		try
		{
			SyntaxToken syntaxToken = token;
			int declarationDepth = GetDeclarationDepth(token);
			syntaxToken = syntaxToken.WithLeadingTrivia(RewriteTrivia(token.LeadingTrivia, declarationDepth, isTrailing: false, NeedsIndentAfterLineBreak(token), mustHaveSeparator: false, lineBreaksAfterLeading(token)));
			SyntaxToken nextRelevantToken = GetNextRelevantToken(token);
			_afterLineBreak = IsLineBreak(token);
			_afterIndentation = false;
			int lineBreaksAfter = LineBreaksAfter(token, nextRelevantToken);
			bool mustHaveSeparator = NeedsSeparator(token, nextRelevantToken);
			return syntaxToken.WithTrailingTrivia(RewriteTrivia(token.TrailingTrivia, declarationDepth, isTrailing: true, indentAfterLineBreak: false, mustHaveSeparator, lineBreaksAfter));
		}
		finally
		{
			_previousToken = token;
		}
		static int lineBreaksAfterLeading(SyntaxToken syntaxToken2)
		{
			if (syntaxToken2.LeadingTrivia.Count < 2)
			{
				return 0;
			}
			SyntaxTriviaList leadingTrivia = syntaxToken2.LeadingTrivia;
			if (leadingTrivia[leadingTrivia.Count - 2].IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
			{
				SyntaxTriviaList leadingTrivia2 = syntaxToken2.LeadingTrivia;
				if (leadingTrivia2[leadingTrivia2.Count - 1].IsKind(SyntaxKind.EndOfLineTrivia))
				{
					return 1;
				}
			}
			return 0;
		}
	}

	private SyntaxToken GetNextRelevantToken(SyntaxToken token)
	{
		SyntaxToken nextToken = token.GetNextToken((SyntaxToken t) => SyntaxToken.NonZeroWidth(t) || t.Kind() == SyntaxKind.EndOfDirectiveToken, (SyntaxTrivia t) => t.Kind() == SyntaxKind.SkippedTokensTrivia);
		if (_consideredSpan.Contains(nextToken.FullSpan))
		{
			return nextToken;
		}
		return default(SyntaxToken);
	}

	private SyntaxTrivia GetIndentation(int count)
	{
		count = Math.Max(count - _initialDepth, 0);
		int capacity = count + 1;
		if (_indentations == null)
		{
			_indentations = ArrayBuilder<SyntaxTrivia>.GetInstance(capacity);
		}
		else
		{
			_indentations.EnsureCapacity(capacity);
		}
		for (int i = _indentations.Count; i <= count; i++)
		{
			string text = ((i == 0) ? "" : (_indentations[i - 1].ToString() + _indentWhitespace));
			_indentations.Add(_useElasticTrivia ? SyntaxFactory.ElasticWhitespace(text) : SyntaxFactory.Whitespace(text));
		}
		return _indentations[count];
	}

	private static bool NeedsIndentAfterLineBreak(SyntaxToken token)
	{
		return !token.IsKind(SyntaxKind.EndOfFileToken);
	}

	private int LineBreaksAfter(SyntaxToken currentToken, SyntaxToken nextToken)
	{
		if (_inSingleLineInterpolation)
		{
			return 0;
		}
		if (currentToken.IsKind(SyntaxKind.EndOfDirectiveToken))
		{
			return 1;
		}
		if (nextToken.Kind() == SyntaxKind.None)
		{
			return 0;
		}
		if (_isInStructuredTrivia)
		{
			return 0;
		}
		if (nextToken.IsKind(SyntaxKind.CloseBraceToken))
		{
			if (IsAccessorListWithoutAccessorsWithBlockBody(currentToken.Parent?.Parent))
			{
				return 0;
			}
			SyntaxNode parent = nextToken.Parent;
			bool flag = ((parent is InitializerExpressionSyntax || parent is AnonymousObjectCreationExpressionSyntax) ? true : false);
			if (flag && !IsSingleLineInitializerContext(nextToken.Parent))
			{
				return 1;
			}
		}
		switch (currentToken.Kind())
		{
		case SyntaxKind.None:
			return 0;
		case SyntaxKind.OpenBraceToken:
			return LineBreaksAfterOpenBrace(currentToken);
		case SyntaxKind.FinallyKeyword:
			return 1;
		case SyntaxKind.CloseBraceToken:
			return LineBreaksAfterCloseBrace(currentToken, nextToken);
		case SyntaxKind.CloseParenToken:
			if (currentToken.Parent is PositionalPatternClauseSyntax)
			{
				return 0;
			}
			if (nextToken.IsKind(SyntaxKind.OpenBraceToken) && IsInitializerInSingleLineContext(nextToken.Parent))
			{
				return 0;
			}
			if ((!(currentToken.Parent is StatementSyntax) || nextToken.Parent == currentToken.Parent) && nextToken.Kind() != SyntaxKind.OpenBraceToken && nextToken.Kind() != SyntaxKind.WhereKeyword)
			{
				return 0;
			}
			return 1;
		case SyntaxKind.CloseBracketToken:
			if (currentToken.Parent is AttributeListSyntax && !(currentToken.Parent.Parent is ParameterSyntax))
			{
				return 1;
			}
			break;
		case SyntaxKind.SemicolonToken:
			return LineBreaksAfterSemicolon(currentToken, nextToken);
		case SyntaxKind.CommaToken:
		{
			SyntaxNode parent = currentToken.Parent;
			bool flag = ((parent is InitializerExpressionSyntax || parent is AnonymousObjectCreationExpressionSyntax) ? true : false);
			if (flag && !IsSingleLineInitializerContext(nextToken.Parent))
			{
				return 1;
			}
			parent = currentToken.Parent;
			flag = ((parent is EnumDeclarationSyntax || parent is SwitchExpressionSyntax) ? true : false);
			return flag ? 1 : 0;
		}
		case SyntaxKind.ElseKeyword:
			return (nextToken.Kind() != SyntaxKind.IfKeyword) ? 1 : 0;
		case SyntaxKind.ColonToken:
			if (currentToken.Parent is LabeledStatementSyntax || currentToken.Parent is SwitchLabelSyntax)
			{
				return 1;
			}
			break;
		case SyntaxKind.SwitchKeyword:
			if (currentToken.Parent is SwitchExpressionSyntax)
			{
				return 1;
			}
			break;
		}
		if ((nextToken.IsKind(SyntaxKind.FromKeyword) && nextToken.Parent.IsKind(SyntaxKind.FromClause)) || (nextToken.IsKind(SyntaxKind.LetKeyword) && nextToken.Parent.IsKind(SyntaxKind.LetClause)) || (nextToken.IsKind(SyntaxKind.WhereKeyword) && nextToken.Parent.IsKind(SyntaxKind.WhereClause)) || (nextToken.IsKind(SyntaxKind.JoinKeyword) && nextToken.Parent.IsKind(SyntaxKind.JoinClause)) || (nextToken.IsKind(SyntaxKind.JoinKeyword) && nextToken.Parent.IsKind(SyntaxKind.JoinIntoClause)) || (nextToken.IsKind(SyntaxKind.OrderByKeyword) && nextToken.Parent.IsKind(SyntaxKind.OrderByClause)) || (nextToken.IsKind(SyntaxKind.SelectKeyword) && nextToken.Parent.IsKind(SyntaxKind.SelectClause)) || (nextToken.IsKind(SyntaxKind.GroupKeyword) && nextToken.Parent.IsKind(SyntaxKind.GroupClause)))
		{
			return 1;
		}
		switch (nextToken.Kind())
		{
		case SyntaxKind.OpenBraceToken:
			return LineBreaksBeforeOpenBrace(nextToken);
		case SyntaxKind.CloseBraceToken:
			return LineBreaksBeforeCloseBrace(nextToken);
		case SyntaxKind.ElseKeyword:
		case SyntaxKind.FinallyKeyword:
			return 1;
		case SyntaxKind.OpenBracketToken:
			if (!(nextToken.Parent is AttributeListSyntax) || nextToken.Parent.Parent is ParameterSyntax)
			{
				return 0;
			}
			return 1;
		case SyntaxKind.WhereKeyword:
			return (currentToken.Parent is TypeParameterListSyntax) ? 1 : 0;
		default:
			return 0;
		}
	}

	private static bool IsAccessorListWithoutAccessorsWithBlockBody(SyntaxNode? node)
	{
		if (node is AccessorListSyntax accessorListSyntax)
		{
			return accessorListSyntax.Accessors.All((AccessorDeclarationSyntax a) => a.Body == null);
		}
		return false;
	}

	private static bool IsAccessorListFollowedByInitializer([NotNullWhen(true)] SyntaxNode? node)
	{
		if (node is AccessorListSyntax { Parent: PropertyDeclarationSyntax parent })
		{
			return parent.Initializer != null;
		}
		return false;
	}

	private static int LineBreaksBeforeOpenBrace(SyntaxToken openBraceToken)
	{
		SyntaxNode parent = openBraceToken.Parent;
		if (parent.IsKind(SyntaxKind.Interpolation) || parent is PropertyPatternClauseSyntax || IsAccessorListWithoutAccessorsWithBlockBody(parent) || IsInitializerInSingleLineContext(parent))
		{
			return 0;
		}
		return 1;
	}

	private static int LineBreaksBeforeCloseBrace(SyntaxToken closeBraceToken)
	{
		SyntaxNode parent = closeBraceToken.Parent;
		if (parent.IsKind(SyntaxKind.Interpolation) || parent is PropertyPatternClauseSyntax || IsInitializerInSingleLineContext(parent))
		{
			return 0;
		}
		return 1;
	}

	private static int LineBreaksAfterOpenBrace(SyntaxToken openBraceToken)
	{
		SyntaxNode parent = openBraceToken.Parent;
		if (parent is PropertyPatternClauseSyntax || parent.IsKind(SyntaxKind.Interpolation) || IsAccessorListWithoutAccessorsWithBlockBody(parent) || IsInitializerInSingleLineContext(parent))
		{
			return 0;
		}
		return 1;
	}

	private static int LineBreaksAfterCloseBrace(SyntaxToken currentToken, SyntaxToken nextToken)
	{
		SyntaxNode parent = currentToken.Parent;
		bool flag = ((parent is SwitchExpressionSyntax || parent is PropertyPatternClauseSyntax) ? true : false);
		bool flag2 = flag || parent.IsKind(SyntaxKind.Interpolation) || parent?.Parent is AnonymousFunctionExpressionSyntax || IsAccessorListFollowedByInitializer(parent) || isCloseBraceFollowedByCommaOrSemicolon(currentToken, nextToken);
		if (!flag2)
		{
			SyntaxNode parent2 = nextToken.Parent;
			bool flag3 = ((parent2 is MemberAccessExpressionSyntax || parent2 is BracketedArgumentListSyntax) ? true : false);
			flag2 = flag3;
		}
		if (flag2 || IsInitializerInSingleLineContext(parent))
		{
			return 0;
		}
		if (parent?.Parent is PropertyDeclarationSyntax property && IsSingleLineProperty(property) && nextToken.Parent is PropertyDeclarationSyntax property2 && IsSingleLineProperty(property2))
		{
			return 1;
		}
		SyntaxKind syntaxKind = nextToken.Kind();
		switch (syntaxKind)
		{
		case SyntaxKind.CloseBraceToken:
		case SyntaxKind.ElseKeyword:
		case SyntaxKind.CatchKeyword:
		case SyntaxKind.FinallyKeyword:
		case SyntaxKind.EndOfFileToken:
			return 1;
		default:
			if (syntaxKind == SyntaxKind.WhileKeyword && nextToken.Parent.IsKind(SyntaxKind.DoStatement))
			{
				return 1;
			}
			return 2;
		}
		static bool isCloseBraceFollowedByCommaOrSemicolon(SyntaxToken token, SyntaxToken token2)
		{
			bool flag4 = token.IsKind(SyntaxKind.CloseBraceToken);
			if (flag4)
			{
				SyntaxKind syntaxKind2 = token2.Kind();
				bool flag5 = ((syntaxKind2 == SyntaxKind.SemicolonToken || syntaxKind2 == SyntaxKind.CommaToken) ? true : false);
				flag4 = flag5;
			}
			return flag4;
		}
	}

	private static int LineBreaksAfterSemicolon(SyntaxToken currentToken, SyntaxToken nextToken)
	{
		if (currentToken.Parent.IsKind(SyntaxKind.ForStatement))
		{
			return 0;
		}
		if (nextToken.Kind() == SyntaxKind.CloseBraceToken)
		{
			return 1;
		}
		if (currentToken.Parent.IsKind(SyntaxKind.UsingDirective))
		{
			if (!nextToken.Parent.IsKind(SyntaxKind.UsingDirective))
			{
				return 2;
			}
			return 1;
		}
		if (currentToken.Parent.IsKind(SyntaxKind.ExternAliasDirective))
		{
			if (!nextToken.Parent.IsKind(SyntaxKind.ExternAliasDirective))
			{
				return 2;
			}
			return 1;
		}
		if (currentToken.Parent is AccessorDeclarationSyntax && IsAccessorListWithoutAccessorsWithBlockBody(currentToken.Parent.Parent))
		{
			return 0;
		}
		if (currentToken.Parent is PropertyDeclarationSyntax property)
		{
			if (IsSingleLineProperty(property) && nextToken.Parent is PropertyDeclarationSyntax property2 && IsSingleLineProperty(property2))
			{
				return 1;
			}
			return 2;
		}
		return 1;
	}

	private static bool NeedsSeparatorForPropertyPattern(SyntaxToken token, SyntaxToken next)
	{
		PropertyPatternClauseSyntax propertyPatternClauseSyntax;
		if (token.Parent.IsKind(SyntaxKind.PropertyPatternClause))
		{
			propertyPatternClauseSyntax = (PropertyPatternClauseSyntax)token.Parent;
		}
		else
		{
			if (!next.Parent.IsKind(SyntaxKind.PropertyPatternClause))
			{
				return false;
			}
			propertyPatternClauseSyntax = (PropertyPatternClauseSyntax)next.Parent;
		}
		bool num = token.IsKind(SyntaxKind.OpenBraceToken);
		bool flag = next.IsKind(SyntaxKind.OpenBraceToken);
		bool flag2 = token.IsKind(SyntaxKind.CloseBraceToken);
		bool flag3 = next.IsKind(SyntaxKind.CloseBraceToken);
		if (num)
		{
			return true;
		}
		if (flag3)
		{
			return true;
		}
		if (propertyPatternClauseSyntax.Parent is RecursivePatternSyntax recursivePatternSyntax)
		{
			if (flag)
			{
				if (recursivePatternSyntax.Type != null || recursivePatternSyntax.PositionalPatternClause != null)
				{
					return true;
				}
				return false;
			}
			if (flag2)
			{
				if (recursivePatternSyntax.Designation == null)
				{
					return false;
				}
				return true;
			}
		}
		return false;
	}

	private static bool NeedsSeparatorForPositionalPattern(SyntaxToken token, SyntaxToken next)
	{
		PositionalPatternClauseSyntax positionalPatternClauseSyntax;
		if (token.Parent.IsKind(SyntaxKind.PositionalPatternClause))
		{
			positionalPatternClauseSyntax = (PositionalPatternClauseSyntax)token.Parent;
		}
		else
		{
			if (!next.Parent.IsKind(SyntaxKind.PositionalPatternClause))
			{
				return false;
			}
			positionalPatternClauseSyntax = (PositionalPatternClauseSyntax)next.Parent;
		}
		bool num = token.IsKind(SyntaxKind.OpenParenToken);
		bool flag = next.IsKind(SyntaxKind.OpenParenToken);
		bool flag2 = token.IsKind(SyntaxKind.CloseParenToken);
		bool flag3 = next.IsKind(SyntaxKind.CloseParenToken);
		if (num)
		{
			return false;
		}
		if (flag3)
		{
			return false;
		}
		if (positionalPatternClauseSyntax.Parent is RecursivePatternSyntax recursivePatternSyntax)
		{
			if (flag)
			{
				if (recursivePatternSyntax.Type != null)
				{
					return true;
				}
				return false;
			}
			if (flag2)
			{
				if (recursivePatternSyntax.PropertyPatternClause != null)
				{
					return false;
				}
				if (recursivePatternSyntax.Designation == null)
				{
					return false;
				}
				return true;
			}
		}
		return false;
	}

	private static bool NeedsSeparatorForListPattern(SyntaxToken token, SyntaxToken next)
	{
		ListPatternSyntax listPatternSyntax = (token.Parent as ListPatternSyntax) ?? (next.Parent as ListPatternSyntax);
		if (listPatternSyntax == null)
		{
			return false;
		}
		if (next.IsKind(SyntaxKind.OpenBracketToken))
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.OpenBracketToken))
		{
			return listPatternSyntax.Designation != null;
		}
		return false;
	}

	private static bool NeedsSeparator(SyntaxToken token, SyntaxToken next)
	{
		if (token.Parent == null || next.Parent == null)
		{
			return false;
		}
		if (IsAccessorListWithoutAccessorsWithBlockBody(next.Parent) || IsAccessorListWithoutAccessorsWithBlockBody(next.Parent.Parent))
		{
			return !next.IsKind(SyntaxKind.SemicolonToken);
		}
		if (IsXmlTextToken(token.Kind()) || IsXmlTextToken(next.Kind()))
		{
			return false;
		}
		if (next.Kind() == SyntaxKind.EndOfDirectiveToken)
		{
			if (IsKeyword(token.Kind()))
			{
				return next.LeadingWidth > 0;
			}
			return false;
		}
		if ((token.Parent is AssignmentExpressionSyntax && AssignmentTokenNeedsSeparator(token.Kind())) || (next.Parent is AssignmentExpressionSyntax && AssignmentTokenNeedsSeparator(next.Kind())) || (token.Parent is BinaryExpressionSyntax && BinaryTokenNeedsSeparator(token.Kind())) || (next.Parent is BinaryExpressionSyntax && BinaryTokenNeedsSeparator(next.Kind())))
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.GreaterThanToken) && token.Parent.IsKind(SyntaxKind.TypeArgumentList) && !SyntaxFacts.IsPunctuation(next.Kind()))
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.GreaterThanToken) && token.Parent.IsKind(SyntaxKind.FunctionPointerParameterList) && !(token.Parent.Parent?.Parent is UsingDirectiveSyntax))
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.CommaToken) && !next.IsKind(SyntaxKind.CommaToken) && !token.Parent.IsKind(SyntaxKind.EnumDeclaration))
		{
			return true;
		}
		if (token.Kind() == SyntaxKind.SemicolonToken && next.Kind() != SyntaxKind.SemicolonToken && next.Kind() != SyntaxKind.CloseParenToken)
		{
			return true;
		}
		if (next.IsKind(SyntaxKind.SwitchKeyword) && next.Parent is SwitchExpressionSyntax)
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.QuestionToken) && (token.Parent.IsKind(SyntaxKind.ConditionalExpression) || token.Parent is TypeSyntax))
		{
			bool flag;
			switch (token.Parent.Parent?.Kind())
			{
			default:
				flag = true;
				break;
			case SyntaxKind.TypeArgumentList:
			case SyntaxKind.UsingDirective:
				flag = false;
				break;
			}
			if (flag)
			{
				return true;
			}
		}
		if (token.IsKind(SyntaxKind.ColonToken))
		{
			if (!token.Parent.IsKind(SyntaxKind.InterpolationFormatClause) && !token.Parent.IsKind(SyntaxKind.XmlPrefix))
			{
				return !token.Parent.IsKind(SyntaxKind.IgnoredDirectiveTrivia);
			}
			return false;
		}
		if (next.IsKind(SyntaxKind.ColonToken) && (next.Parent.IsKind(SyntaxKind.BaseList) || next.Parent.IsKind(SyntaxKind.TypeParameterConstraintClause) || next.Parent is ConstructorInitializerSyntax))
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.CloseBracketToken) && IsWord(next.Kind()))
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.CloseParenToken) && IsWord(next.Kind()) && token.Parent.IsKind(SyntaxKind.TupleType))
		{
			return true;
		}
		if ((next.IsKind(SyntaxKind.QuestionToken) || next.IsKind(SyntaxKind.ColonToken)) && next.Parent.IsKind(SyntaxKind.ConditionalExpression))
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.EqualsToken))
		{
			return !token.Parent.IsKind(SyntaxKind.XmlTextAttribute);
		}
		if (next.IsKind(SyntaxKind.EqualsToken))
		{
			return !next.Parent.IsKind(SyntaxKind.XmlTextAttribute);
		}
		SyntaxKind syntaxKind;
		if (token.Parent.IsKind(SyntaxKind.FunctionPointerType))
		{
			if (next.IsKind(SyntaxKind.AsteriskToken) && token.IsKind(SyntaxKind.DelegateKeyword))
			{
				return false;
			}
			if (token.IsKind(SyntaxKind.AsteriskToken) && next.Parent.IsKind(SyntaxKind.FunctionPointerCallingConvention))
			{
				syntaxKind = next.Kind();
				if (syntaxKind - 8445 <= SyntaxKind.List || syntaxKind == SyntaxKind.IdentifierToken)
				{
					return true;
				}
			}
		}
		if (next.Parent.IsKind(SyntaxKind.FunctionPointerParameterList) && next.IsKind(SyntaxKind.LessThanToken))
		{
			syntaxKind = token.Kind();
			if (syntaxKind == SyntaxKind.AsteriskToken)
			{
				goto IL_0466;
			}
			if (syntaxKind != SyntaxKind.CloseBracketToken)
			{
				if (syntaxKind - 8445 <= SyntaxKind.List)
				{
					goto IL_0466;
				}
			}
			else if (token.Parent.IsKind(SyntaxKind.FunctionPointerUnmanagedCallingConventionList))
			{
				goto IL_0466;
			}
		}
		if (token.Parent.IsKind(SyntaxKind.FunctionPointerCallingConvention) && next.Parent.IsKind(SyntaxKind.FunctionPointerUnmanagedCallingConventionList) && next.IsKind(SyntaxKind.OpenBracketToken))
		{
			return false;
		}
		if (next.Parent.IsKind(SyntaxKind.FunctionPointerUnmanagedCallingConventionList) && token.Parent.IsKind(SyntaxKind.FunctionPointerUnmanagedCallingConventionList))
		{
			if (next.IsKind(SyntaxKind.IdentifierToken))
			{
				if (token.IsKind(SyntaxKind.OpenBracketToken))
				{
					return false;
				}
				if (token.IsKind(SyntaxKind.CommaToken))
				{
					return true;
				}
			}
			if (next.IsKind(SyntaxKind.CommaToken))
			{
				return false;
			}
			if (next.IsKind(SyntaxKind.CloseBracketToken))
			{
				return false;
			}
		}
		if (token.IsKind(SyntaxKind.LessThanToken) && token.Parent.IsKind(SyntaxKind.FunctionPointerParameterList))
		{
			return false;
		}
		if (next.IsKind(SyntaxKind.GreaterThanToken) && next.Parent.IsKind(SyntaxKind.FunctionPointerParameterList))
		{
			return false;
		}
		if (token.IsKind(SyntaxKind.EqualsGreaterThanToken) || next.IsKind(SyntaxKind.EqualsGreaterThanToken))
		{
			return true;
		}
		if (SyntaxFacts.IsLiteral(token.Kind()) && SyntaxFacts.IsLiteral(next.Kind()))
		{
			return true;
		}
		if (next.IsKind(SyntaxKind.AsteriskToken) && next.Parent is PointerTypeSyntax)
		{
			return false;
		}
		if (token.IsKind(SyntaxKind.AsteriskToken) && token.Parent is PointerTypeSyntax && (next.IsKind(SyntaxKind.IdentifierToken) || next.Parent.IsKind(SyntaxKind.IndexerDeclaration)))
		{
			return true;
		}
		if (IsSingleLineInitializerContext(token.Parent))
		{
			SyntaxNode parent = next.Parent;
			bool flag = ((parent is InitializerExpressionSyntax || parent is AnonymousObjectCreationExpressionSyntax) ? true : false);
			if (flag && next.IsKind(SyntaxKind.OpenBraceToken))
			{
				return true;
			}
			parent = token.Parent;
			flag = ((parent is InitializerExpressionSyntax || parent is AnonymousObjectCreationExpressionSyntax) ? true : false);
			if (flag && token.IsKind(SyntaxKind.OpenBraceToken))
			{
				return true;
			}
			parent = next.Parent;
			flag = ((parent is InitializerExpressionSyntax || parent is AnonymousObjectCreationExpressionSyntax) ? true : false);
			if (flag && next.IsKind(SyntaxKind.CloseBraceToken))
			{
				return true;
			}
		}
		if (next.RawKind == 8200)
		{
			SyntaxNode parent = next.Parent;
			if (parent != null && parent.Parent is ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpressionSyntax)
			{
				TypeSyntax? returnType = parenthesizedLambdaExpressionSyntax.ReturnType;
				if (returnType != null && returnType.GetLastToken() == token)
				{
					return true;
				}
			}
		}
		SyntaxNode? parent2 = token.Parent.Parent;
		if (parent2 != null && parent2.Kind() == SyntaxKind.SuppressNullableWarningExpression)
		{
			return false;
		}
		if (IsKeyword(token.Kind()) && !token.IsKind(SyntaxKind.ExtensionKeyword) && !next.IsKind(SyntaxKind.ColonToken) && !next.IsKind(SyntaxKind.DotToken) && !next.IsKind(SyntaxKind.QuestionToken) && !next.IsKind(SyntaxKind.SemicolonToken) && !next.IsKind(SyntaxKind.OpenBracketToken) && (!next.IsKind(SyntaxKind.OpenParenToken) || KeywordNeedsSeparatorBeforeOpenParen(token.Kind()) || next.Parent.IsKind(SyntaxKind.TupleType)) && !next.IsKind(SyntaxKind.CloseParenToken) && !next.IsKind(SyntaxKind.CloseBraceToken) && !next.IsKind(SyntaxKind.ColonColonToken) && !next.IsKind(SyntaxKind.GreaterThanToken) && !next.IsKind(SyntaxKind.CommaToken))
		{
			return true;
		}
		if (IsWordOrLiteral(token.Kind()) && IsWordOrLiteral(next.Kind()))
		{
			return true;
		}
		if (token.Width > 1 && next.Width > 1)
		{
			string text = token.Text;
			char c = text[text.Length - 1];
			char c2 = next.Text[0];
			if (c == c2 && TokenCharacterCanBeDoubled(c))
			{
				return true;
			}
		}
		if (token.Parent is RelationalPatternSyntax)
		{
			return true;
		}
		syntaxKind = next.Kind();
		if (syntaxKind - 8438 <= SyntaxKind.List)
		{
			return true;
		}
		syntaxKind = token.Kind();
		if (syntaxKind - 8438 <= (SyntaxKind)2)
		{
			return true;
		}
		if (NeedsSeparatorForPropertyPattern(token, next))
		{
			return true;
		}
		if (NeedsSeparatorForPositionalPattern(token, next))
		{
			return true;
		}
		if (NeedsSeparatorForListPattern(token, next))
		{
			return true;
		}
		syntaxKind = token.Parent.Kind();
		SyntaxKind syntaxKind2 = next.Parent.Kind();
		if (syntaxKind != SyntaxKind.LineDirectivePosition)
		{
			if (syntaxKind == SyntaxKind.LineSpanDirectiveTrivia && syntaxKind2 == SyntaxKind.LineDirectivePosition)
			{
				goto IL_08d0;
			}
		}
		else if (syntaxKind2 == SyntaxKind.LineSpanDirectiveTrivia)
		{
			goto IL_08d0;
		}
		return false;
		IL_08d0:
		return true;
		IL_0466:
		return false;
	}

	private static bool KeywordNeedsSeparatorBeforeOpenParen(SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.TypeOfKeyword:
		case SyntaxKind.SizeOfKeyword:
		case SyntaxKind.DefaultKeyword:
		case SyntaxKind.NewKeyword:
		case SyntaxKind.ArgListKeyword:
		case SyntaxKind.ThisKeyword:
		case SyntaxKind.BaseKeyword:
		case SyntaxKind.CheckedKeyword:
		case SyntaxKind.UncheckedKeyword:
			return false;
		default:
			return true;
		}
	}

	private static bool IsXmlTextToken(SyntaxKind kind)
	{
		if (kind - 8513 <= SyntaxKind.List)
		{
			return true;
		}
		return false;
	}

	private static bool BinaryTokenNeedsSeparator(SyntaxKind kind)
	{
		if (kind == SyntaxKind.DotToken || kind == SyntaxKind.MinusGreaterThanToken)
		{
			return false;
		}
		return SyntaxFacts.GetBinaryExpression(kind) != SyntaxKind.None;
	}

	private static bool AssignmentTokenNeedsSeparator(SyntaxKind kind)
	{
		return SyntaxFacts.GetAssignmentExpression(kind) != SyntaxKind.None;
	}

	private SyntaxTriviaList RewriteTrivia(SyntaxTriviaList triviaList, int depth, bool isTrailing, bool indentAfterLineBreak, bool mustHaveSeparator, int lineBreaksAfter)
	{
		ArrayBuilder<SyntaxTrivia> instance = ArrayBuilder<SyntaxTrivia>.GetInstance(triviaList.Count);
		try
		{
			foreach (SyntaxTrivia item2 in triviaList)
			{
				if (item2.IsKind(SyntaxKind.WhitespaceTrivia) || item2.IsKind(SyntaxKind.EndOfLineTrivia) || item2.FullWidth == 0)
				{
					continue;
				}
				bool flag = (instance.Count > 0 && NeedsSeparatorBetween(instance.Last())) || ((instance.Count == 0) & isTrailing);
				if ((NeedsLineBreakBefore(item2, isTrailing) || (instance.Count > 0 && NeedsLineBreakBetween(instance.Last(), item2, isTrailing))) && !_afterLineBreak)
				{
					instance.Add(GetEndOfLine());
					_afterLineBreak = true;
					_afterIndentation = false;
				}
				if (_afterLineBreak)
				{
					if (!_afterIndentation && NeedsIndentAfterLineBreak(item2))
					{
						instance.Add(GetIndentation(GetDeclarationDepth(item2)));
						_afterIndentation = true;
					}
				}
				else if (flag)
				{
					instance.Add(GetSpace());
					_afterLineBreak = false;
					_afterIndentation = false;
				}
				if (item2.HasStructure)
				{
					SyntaxTrivia item = VisitStructuredTrivia(item2);
					instance.Add(item);
				}
				else if (item2.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia))
				{
					instance.Add(s_trimmedDocCommentExterior);
				}
				else
				{
					instance.Add(item2);
				}
				if (NeedsLineBreakAfter(item2, isTrailing) && (instance.Count == 0 || !EndsInLineBreak(instance.Last())))
				{
					instance.Add(GetEndOfLine());
					_afterLineBreak = true;
					_afterIndentation = false;
				}
			}
			if (lineBreaksAfter > 0)
			{
				if (instance.Count > 0 && EndsInLineBreak(instance.Last()))
				{
					lineBreaksAfter--;
				}
				for (int i = 0; i < lineBreaksAfter; i++)
				{
					instance.Add(GetEndOfLine());
					_afterLineBreak = true;
					_afterIndentation = false;
				}
			}
			else if (indentAfterLineBreak && _afterLineBreak && !_afterIndentation)
			{
				instance.Add(GetIndentation(depth));
				_afterIndentation = true;
			}
			else if (mustHaveSeparator)
			{
				instance.Add(GetSpace());
				_afterLineBreak = false;
				_afterIndentation = false;
			}
			if (instance.Count == 0)
			{
				return default(SyntaxTriviaList);
			}
			if (instance.Count == 1)
			{
				return SyntaxFactory.TriviaList(instance.First());
			}
			return SyntaxFactory.TriviaList(instance);
		}
		finally
		{
			instance.Free();
		}
	}

	private SyntaxTrivia GetSpace()
	{
		if (!_useElasticTrivia)
		{
			return SyntaxFactory.Space;
		}
		return SyntaxFactory.ElasticSpace;
	}

	private SyntaxTrivia GetEndOfLine()
	{
		return _eolTrivia;
	}

	private SyntaxTrivia VisitStructuredTrivia(SyntaxTrivia trivia)
	{
		bool isInStructuredTrivia = _isInStructuredTrivia;
		_isInStructuredTrivia = true;
		SyntaxToken previousToken = _previousToken;
		_previousToken = default(SyntaxToken);
		SyntaxTrivia result = VisitTrivia(trivia);
		_isInStructuredTrivia = isInStructuredTrivia;
		_previousToken = previousToken;
		return result;
	}

	private static bool NeedsSeparatorBetween(SyntaxTrivia trivia)
	{
		SyntaxKind syntaxKind = trivia.Kind();
		if (syntaxKind == SyntaxKind.None || syntaxKind == SyntaxKind.WhitespaceTrivia || syntaxKind == SyntaxKind.DocumentationCommentExteriorTrivia)
		{
			return false;
		}
		return !SyntaxFacts.IsPreprocessorDirective(trivia.Kind());
	}

	private static bool NeedsLineBreakBetween(SyntaxTrivia trivia, SyntaxTrivia next, bool isTrailingTrivia)
	{
		if (!NeedsLineBreakAfter(trivia, isTrailingTrivia))
		{
			return NeedsLineBreakBefore(next, isTrailingTrivia);
		}
		return true;
	}

	private static bool NeedsLineBreakBefore(SyntaxTrivia trivia, bool isTrailingTrivia)
	{
		SyntaxKind syntaxKind = trivia.Kind();
		if (syntaxKind == SyntaxKind.DocumentationCommentExteriorTrivia)
		{
			return !isTrailingTrivia;
		}
		return SyntaxFacts.IsPreprocessorDirective(syntaxKind);
	}

	private static bool NeedsLineBreakAfter(SyntaxTrivia trivia, bool isTrailingTrivia)
	{
		SyntaxKind syntaxKind = trivia.Kind();
		return syntaxKind switch
		{
			SyntaxKind.SingleLineCommentTrivia => true, 
			SyntaxKind.MultiLineCommentTrivia => !isTrailingTrivia, 
			_ => SyntaxFacts.IsPreprocessorDirective(syntaxKind), 
		};
	}

	private static bool NeedsIndentAfterLineBreak(SyntaxTrivia trivia)
	{
		SyntaxKind syntaxKind = trivia.Kind();
		if (syntaxKind - 8541 <= (SyntaxKind)4)
		{
			return true;
		}
		return false;
	}

	private static bool IsLineBreak(SyntaxToken token)
	{
		return token.Kind() == SyntaxKind.XmlTextLiteralNewLineToken;
	}

	private static bool EndsInLineBreak(SyntaxTrivia trivia)
	{
		if (trivia.Kind() == SyntaxKind.EndOfLineTrivia)
		{
			return true;
		}
		if (trivia.Kind() == SyntaxKind.PreprocessingMessageTrivia || trivia.Kind() == SyntaxKind.DisabledTextTrivia)
		{
			string text = trivia.ToFullString();
			if (text.Length > 0)
			{
				return SyntaxFacts.IsNewLine(text[text.Length - 1]);
			}
			return false;
		}
		if (trivia.HasStructure)
		{
			SyntaxNode structure = trivia.GetStructure();
			SyntaxTriviaList trailingTrivia = structure.GetTrailingTrivia();
			if (trailingTrivia.Count > 0)
			{
				return EndsInLineBreak(trailingTrivia.Last());
			}
			if (!structure.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
			{
				return IsLineBreak(structure.GetLastToken());
			}
			return false;
		}
		return false;
	}

	private static bool IsWord(SyntaxKind kind)
	{
		if (kind != SyntaxKind.IdentifierToken)
		{
			return IsKeyword(kind);
		}
		return true;
	}

	private static bool IsWordOrLiteral(SyntaxKind kind)
	{
		if (!SyntaxFacts.IsLiteral(kind) && !IsKeyword(kind) && kind != SyntaxKind.InterpolatedStringEndToken)
		{
			return kind == SyntaxKind.InterpolatedRawStringEndToken;
		}
		return true;
	}

	private static bool IsKeyword(SyntaxKind kind)
	{
		if (!SyntaxFacts.IsKeywordKind(kind))
		{
			return SyntaxFacts.IsPreprocessorKeyword(kind);
		}
		return true;
	}

	private static bool TokenCharacterCanBeDoubled(char c)
	{
		switch (c)
		{
		case '"':
		case '+':
		case '-':
		case ':':
		case '<':
		case '=':
		case '?':
			return true;
		default:
			return false;
		}
	}

	private static int GetDeclarationDepth(SyntaxToken token)
	{
		return GetDeclarationDepth(token.Parent);
	}

	private static int GetDeclarationDepth(SyntaxTrivia trivia)
	{
		if (SyntaxFacts.IsPreprocessorDirective(trivia.Kind()))
		{
			return 0;
		}
		return GetDeclarationDepth(trivia.Token);
	}

	private static int GetDeclarationDepth(SyntaxNode? node)
	{
		if (node == null)
		{
			return 0;
		}
		if (node.IsStructuredTrivia)
		{
			return GetDeclarationDepth(((StructuredTriviaSyntax)node).ParentTrivia);
		}
		int declarationDepth;
		bool flag;
		if (node.Parent != null)
		{
			if (node.Parent.IsKind(SyntaxKind.CompilationUnit))
			{
				return 0;
			}
			declarationDepth = GetDeclarationDepth(node.Parent);
			SyntaxKind syntaxKind = node.Parent.Kind();
			if ((syntaxKind == SyntaxKind.GlobalStatement || syntaxKind == SyntaxKind.FileScopedNamespaceDeclaration) ? true : false)
			{
				return declarationDepth;
			}
			if (node.IsKind(SyntaxKind.IfStatement) && node.Parent.IsKind(SyntaxKind.ElseClause))
			{
				return declarationDepth;
			}
			if (node.Parent is BlockSyntax)
			{
				return declarationDepth + 1;
			}
			if (node != null)
			{
				SyntaxNode parent = node.Parent;
				if (parent is InitializerExpressionSyntax || parent is AnonymousObjectMemberDeclaratorSyntax)
				{
					flag = true;
					goto IL_00c2;
				}
			}
			flag = false;
			goto IL_00c2;
		}
		return 0;
		IL_00c2:
		if ((flag || (node is AssignmentExpressionSyntax assignmentExpressionSyntax && assignmentExpressionSyntax.Parent is InitializerExpressionSyntax)) && !IsSingleLineInitializerContext(node.Parent))
		{
			return declarationDepth + 1;
		}
		if (node is StatementSyntax && !(node is BlockSyntax))
		{
			if (node is UsingStatementSyntax usingStatementSyntax && usingStatementSyntax.Parent is UsingStatementSyntax)
			{
				return declarationDepth;
			}
			if (node is FixedStatementSyntax fixedStatementSyntax && fixedStatementSyntax.Parent is FixedStatementSyntax)
			{
				return declarationDepth;
			}
			return declarationDepth + 1;
		}
		if (node is MemberDeclarationSyntax || node is AccessorDeclarationSyntax || node is TypeParameterConstraintClauseSyntax || node is SwitchSectionSyntax || node is SwitchExpressionArmSyntax || node is UsingDirectiveSyntax || node is ExternAliasDirectiveSyntax || node is QueryExpressionSyntax || node is QueryContinuationSyntax)
		{
			return declarationDepth + 1;
		}
		return declarationDepth;
	}

	private static bool IsSingleLineInitializerContext(SyntaxNode? node)
	{
		if (node == null)
		{
			return false;
		}
		for (SyntaxNode parent = node.Parent; parent != null; parent = parent.Parent)
		{
			if ((parent is InterpolationSyntax || parent is AttributeArgumentSyntax || parent is ArgumentSyntax) ? true : false)
			{
				return true;
			}
			if ((parent is StatementSyntax || parent is MemberDeclarationSyntax) ? true : false)
			{
				return false;
			}
		}
		return false;
	}

	private static bool IsInitializerInSingleLineContext(SyntaxNode? node)
	{
		if ((!(node is InitializerExpressionSyntax) && !(node is AnonymousObjectCreationExpressionSyntax)) || 1 == 0)
		{
			return false;
		}
		return IsSingleLineInitializerContext(node);
	}

	private static bool IsSingleLineProperty(PropertyDeclarationSyntax property)
	{
		if (property.AccessorList != null)
		{
			return IsAccessorListWithoutAccessorsWithBlockBody(property.AccessorList);
		}
		return true;
	}

	public override SyntaxNode? VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
	{
		if (node.StringStartToken.Kind() == SyntaxKind.InterpolatedStringStartToken)
		{
			bool inSingleLineInterpolation = _inSingleLineInterpolation;
			_inSingleLineInterpolation = true;
			try
			{
				return base.VisitInterpolatedStringExpression(node);
			}
			finally
			{
				_inSingleLineInterpolation = inSingleLineInterpolation;
			}
		}
		return base.VisitInterpolatedStringExpression(node);
	}

	public override SyntaxNode? VisitXmlTextAttribute(XmlTextAttributeSyntax node)
	{
		XmlTextAttributeSyntax xmlTextAttributeSyntax = (XmlTextAttributeSyntax)base.VisitXmlTextAttribute(node);
		if ((xmlTextAttributeSyntax == null || xmlTextAttributeSyntax.HasTrailingTrivia) ? true : false)
		{
			return xmlTextAttributeSyntax;
		}
		SyntaxKind syntaxKind = GetNextRelevantToken(node.EndQuoteToken).Kind();
		if (syntaxKind == SyntaxKind.GreaterThanToken || syntaxKind == SyntaxKind.SlashGreaterThanToken)
		{
			return xmlTextAttributeSyntax;
		}
		return xmlTextAttributeSyntax.WithTrailingTrivia(GetSpace());
	}
}
