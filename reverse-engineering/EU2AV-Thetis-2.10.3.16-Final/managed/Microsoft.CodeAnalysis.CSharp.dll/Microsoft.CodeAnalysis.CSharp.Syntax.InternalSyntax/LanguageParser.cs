using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class LanguageParser : SyntaxParser
{
	[Flags]
	internal enum TerminatorState
	{
		EndOfFile = 0,
		IsNamespaceMemberStartOrStop = 1,
		IsAttributeDeclarationTerminator = 2,
		IsPossibleAggregateClauseStartOrStop = 4,
		IsPossibleMemberStartOrStop = 8,
		IsEndOfReturnType = 0x10,
		IsEndOfParameterList = 0x20,
		IsEndOfFieldDeclaration = 0x40,
		IsPossibleEndOfVariableDeclaration = 0x80,
		IsEndOfTypeArgumentList = 0x100,
		IsPossibleStatementStartOrStop = 0x200,
		IsEndOfFixedStatement = 0x400,
		IsEndOfTryBlock = 0x800,
		IsEndOfCatchClause = 0x1000,
		IsEndOfFilterClause = 0x2000,
		IsEndOfCatchBlock = 0x4000,
		IsEndOfDoWhileExpression = 0x8000,
		IsEndOfForStatementArgument = 0x10000,
		IsEndOfDeclarationClause = 0x20000,
		IsEndOfArgumentList = 0x40000,
		IsSwitchSectionStart = 0x80000,
		IsEndOfTypeParameterList = 0x100000,
		IsEndOfMethodSignature = 0x200000,
		IsEndOfNameInExplicitInterface = 0x400000,
		IsEndOfFunctionPointerParameterList = 0x800000,
		IsEndOfFunctionPointerParameterListErrored = 0x1000000,
		IsEndOfFunctionPointerCallingConvention = 0x2000000,
		IsEndOfTypeSignature = 0x4000000,
		IsExpressionOrPatternInCaseLabelOfSwitchStatement = 0x8000000,
		IsPatternInSwitchExpressionArm = 0x10000000
	}

	private struct NamespaceBodyBuilder(SyntaxListPool pool)
	{
		public SyntaxListBuilder<ExternAliasDirectiveSyntax> Externs = pool.Allocate<ExternAliasDirectiveSyntax>();

		public SyntaxListBuilder<UsingDirectiveSyntax> Usings = pool.Allocate<UsingDirectiveSyntax>();

		public SyntaxListBuilder<AttributeListSyntax> Attributes = pool.Allocate<AttributeListSyntax>();

		public SyntaxListBuilder<MemberDeclarationSyntax> Members = pool.Allocate<MemberDeclarationSyntax>();

		internal void Free(SyntaxListPool pool)
		{
			pool.Free(Members);
			pool.Free(Attributes);
			pool.Free(Usings);
			pool.Free(Externs);
		}
	}

	private enum NamespaceParts
	{
		None,
		ExternAliases,
		Usings,
		GlobalAttributes,
		MembersAndStatements,
		TypesAndNamespaces,
		TopLevelStatementsAfterTypesAndNamespaces
	}

	private readonly ref struct ParserSyntaxContextResetter : IDisposable
	{
		[Flags]
		private enum LanguageParserState : byte
		{
			IsInAsync = 1,
			IsInQuery = 2,
			IsInFieldKeywordContext = 4,
			ForceConditionalAccessExpression = 8
		}

		private readonly LanguageParser _parser;

		private readonly LanguageParserState _previousState;

		public ParserSyntaxContextResetter(LanguageParser parser, bool? isInAsyncContext = null, bool? isInQueryContext = null, bool? isInFieldKeywordContext = null, bool? forceConditionalAccessExpression = null)
		{
			_parser = parser;
			_previousState = (LanguageParserState)((parser.IsInAsync ? 1 : 0) | (parser.IsInQuery ? 2 : 0) | (parser.IsInFieldKeywordContext ? 4 : 0) | (parser.ForceConditionalAccessExpression ? 8 : 0));
			_parser.IsInAsync = isInAsyncContext ?? parser.IsInAsync;
			_parser.IsInQuery = isInQueryContext ?? parser.IsInQuery;
			_parser.IsInFieldKeywordContext = isInFieldKeywordContext ?? parser.IsInFieldKeywordContext;
			_parser.ForceConditionalAccessExpression = forceConditionalAccessExpression ?? parser.ForceConditionalAccessExpression;
		}

		public void Dispose()
		{
			_parser.IsInAsync = (_previousState & LanguageParserState.IsInAsync) != 0;
			_parser.IsInQuery = (_previousState & LanguageParserState.IsInQuery) != 0;
			_parser.IsInFieldKeywordContext = (_previousState & LanguageParserState.IsInFieldKeywordContext) != 0;
			_parser.ForceConditionalAccessExpression = (_previousState & LanguageParserState.ForceConditionalAccessExpression) != 0;
		}
	}

	private enum AccessorDeclaringKind
	{
		Property,
		Indexer,
		Event
	}

	private enum PostSkipAction
	{
		Continue,
		Abort
	}

	[Flags]
	private enum VariableFlags
	{
		None = 0,
		Fixed = 1,
		Const = 2,
		LocalOrField = 4,
		ForStatement = 8
	}

	[Flags]
	private enum NameOptions
	{
		None = 0,
		InExpression = 1,
		InTypeList = 2,
		PossiblePattern = 4,
		AfterIs = 8,
		DefinitePattern = 0x10,
		AfterOut = 0x20,
		AfterTupleComma = 0x40,
		FirstElementOfPossibleTupleLiteral = 0x80
	}

	private enum ScanTypeArgumentListKind
	{
		NotTypeArgumentList,
		PossibleTypeArgumentList,
		DefiniteTypeArgumentList
	}

	private enum ScanTypeFlags
	{
		NotType,
		MustBeType,
		GenericTypeOrMethod,
		GenericTypeOrExpression,
		NonGenericTypeOrExpression,
		AliasQualifiedName,
		NullableType,
		PointerOrMultiplication,
		TupleType
	}

	private enum ParseTypeMode
	{
		Normal,
		Parameter,
		AfterIs,
		DefinitePattern,
		AfterOut,
		AfterRef,
		AfterTupleComma,
		AsExpression,
		NewExpression,
		FirstElementOfPossibleTupleLiteral
	}

	private enum Precedence : uint
	{
		Expression = 0u,
		Assignment = Expression,
		Lambda = Expression,
		Conditional = 1u,
		Coalescing = 2u,
		ConditionalOr = 3u,
		ConditionalAnd = 4u,
		LogicalOr = 5u,
		LogicalXor = 6u,
		LogicalAnd = 7u,
		Equality = 8u,
		Relational = 9u,
		Shift = 10u,
		Additive = 11u,
		Multiplicative = 12u,
		Switch = 13u,
		Range = 14u,
		Unary = 15u,
		Cast = 16u,
		PointerIndirection = 17u,
		AddressOf = 18u,
		Primary = 19u
	}

	private delegate PostSkipAction SkipBadTokens<TNode>(LanguageParser parser, ref SyntaxToken openToken, SeparatedSyntaxListBuilder<TNode> builder, SyntaxKind expectedKind, SyntaxKind closeTokenKind) where TNode : GreenNode;

	private ref struct DisposableResetPoint(LanguageParser languageParser, bool resetOnDispose, ResetPoint resetPoint)
	{
		private readonly LanguageParser _languageParser = languageParser;

		private readonly bool _resetOnDispose = resetOnDispose;

		private ResetPoint _resetPoint = resetPoint;

		public void Reset()
		{
			_languageParser.Reset(ref _resetPoint);
		}

		public void Dispose()
		{
			if (_resetOnDispose)
			{
				Reset();
			}
			_languageParser.Release(ref _resetPoint);
		}
	}

	private new struct ResetPoint
	{
		internal SyntaxParser.ResetPoint BaseResetPoint;

		internal readonly TerminatorState TerminatorState;

		internal readonly bool IsInAsync;

		internal readonly bool IsInQuery;

		internal readonly bool IsInFieldKeywordContext;

		internal ResetPoint(SyntaxParser.ResetPoint resetPoint, TerminatorState terminatorState, bool isInAsync, bool isInQuery, bool isInFieldKeywordContext)
		{
			BaseResetPoint = resetPoint;
			TerminatorState = terminatorState;
			IsInAsync = isInAsync;
			IsInQuery = isInQuery;
			IsInFieldKeywordContext = isInFieldKeywordContext;
		}
	}

	private readonly SyntaxListPool _pool = new SyntaxListPool();

	private readonly SyntaxFactoryContext _syntaxFactoryContext;

	private readonly ContextAwareSyntax _syntaxFactory;

	private int _recursionDepth;

	private TerminatorState _termState;

	private const int LastTerminatorState = 268435456;

	private bool IsCurrentTokenQueryContextualKeyword => IsTokenQueryContextualKeyword(base.CurrentToken);

	[Obsolete("Use IsIncrementalAndFactoryContextMatches")]
	private new bool IsIncremental
	{
		get
		{
			throw new Exception("Use IsIncrementalAndFactoryContextMatches");
		}
	}

	private bool IsIncrementalAndFactoryContextMatches
	{
		get
		{
			if (!base.IsIncremental)
			{
				return false;
			}
			Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode currentNode = base.CurrentNode;
			if (currentNode != null)
			{
				return MatchesFactoryContext(currentNode.Green, _syntaxFactoryContext);
			}
			return false;
		}
	}

	private bool IsInAsync
	{
		get
		{
			return _syntaxFactoryContext.IsInAsync;
		}
		set
		{
			_syntaxFactoryContext.IsInAsync = value;
		}
	}

	private bool ForceConditionalAccessExpression
	{
		get
		{
			return _syntaxFactoryContext.ForceConditionalAccessExpression;
		}
		set
		{
			_syntaxFactoryContext.ForceConditionalAccessExpression = value;
		}
	}

	private bool IsInQuery
	{
		get
		{
			return _syntaxFactoryContext.IsInQuery;
		}
		set
		{
			_syntaxFactoryContext.IsInQuery = value;
		}
	}

	internal bool IsInFieldKeywordContext
	{
		get
		{
			return _syntaxFactoryContext.IsInFieldKeywordContext;
		}
		set
		{
			_syntaxFactoryContext.IsInFieldKeywordContext = value;
		}
	}

	internal LanguageParser(Lexer lexer, Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode? oldTree, IEnumerable<TextChangeRange>? changes, LexerMode lexerMode = LexerMode.Syntax, CancellationToken cancellationToken = default(CancellationToken))
		: base(lexer, lexerMode, oldTree, changes, allowModeReset: false, preLexIfNotIncremental: true, cancellationToken)
	{
		_syntaxFactoryContext = new SyntaxFactoryContext();
		_syntaxFactory = new ContextAwareSyntax(_syntaxFactoryContext);
	}

	private static bool IsSomeWord(SyntaxKind kind)
	{
		if (kind != SyntaxKind.IdentifierToken)
		{
			return SyntaxFacts.IsKeywordKind(kind);
		}
		return true;
	}

	private bool IsTerminator()
	{
		if (base.CurrentToken.Kind == SyntaxKind.EndOfFileToken)
		{
			return true;
		}
		for (int num = 1; num <= 268435456; num <<= 1)
		{
			switch ((TerminatorState)((uint)_termState & (uint)num))
			{
			case TerminatorState.IsNamespaceMemberStartOrStop:
				if (!IsNamespaceMemberStartOrStop())
				{
					continue;
				}
				break;
			case TerminatorState.IsAttributeDeclarationTerminator:
				if (!IsAttributeDeclarationTerminator())
				{
					continue;
				}
				break;
			case TerminatorState.IsPossibleAggregateClauseStartOrStop:
				if (!IsPossibleAggregateClauseStartOrStop())
				{
					continue;
				}
				break;
			case TerminatorState.IsPossibleMemberStartOrStop:
				if (!IsPossibleMemberStartOrStop())
				{
					continue;
				}
				break;
			case TerminatorState.IsEndOfReturnType:
				if (!IsEndOfReturnType())
				{
					continue;
				}
				break;
			case TerminatorState.IsEndOfParameterList:
				if (!IsEndOfParameterList())
				{
					continue;
				}
				break;
			case TerminatorState.IsEndOfFieldDeclaration:
				if (!IsEndOfFieldDeclaration())
				{
					continue;
				}
				break;
			case TerminatorState.IsPossibleEndOfVariableDeclaration:
				if (!IsPossibleEndOfVariableDeclaration())
				{
					continue;
				}
				break;
			case TerminatorState.IsEndOfTypeArgumentList:
				if (!IsEndOfTypeArgumentList())
				{
					continue;
				}
				break;
			case TerminatorState.IsPossibleStatementStartOrStop:
				if (!IsPossibleStatementStartOrStop())
				{
					continue;
				}
				break;
			case TerminatorState.IsEndOfFixedStatement:
				if (!IsEndOfFixedStatement())
				{
					continue;
				}
				break;
			case TerminatorState.IsEndOfTryBlock:
				if (!IsEndOfTryBlock())
				{
					continue;
				}
				break;
			case TerminatorState.IsEndOfCatchClause:
				if (!IsEndOfCatchClause())
				{
					continue;
				}
				break;
			case TerminatorState.IsEndOfFilterClause:
				if (!IsEndOfFilterClause())
				{
					continue;
				}
				break;
			case TerminatorState.IsEndOfCatchBlock:
				if (!IsEndOfCatchBlock())
				{
					continue;
				}
				break;
			case TerminatorState.IsEndOfDoWhileExpression:
				if (!IsEndOfDoWhileExpression())
				{
					continue;
				}
				break;
			case TerminatorState.IsEndOfForStatementArgument:
				if (!IsEndOfForStatementArgument())
				{
					continue;
				}
				break;
			case TerminatorState.IsEndOfDeclarationClause:
				if (!IsEndOfDeclarationClause())
				{
					continue;
				}
				break;
			case TerminatorState.IsEndOfArgumentList:
				if (!IsEndOfArgumentList())
				{
					continue;
				}
				break;
			case TerminatorState.IsSwitchSectionStart:
				if (!IsPossibleSwitchSection())
				{
					continue;
				}
				break;
			case TerminatorState.IsEndOfTypeParameterList:
				if (!IsEndOfTypeParameterList())
				{
					continue;
				}
				break;
			case TerminatorState.IsEndOfMethodSignature:
				if (!IsEndOfMethodSignature())
				{
					continue;
				}
				break;
			case TerminatorState.IsEndOfNameInExplicitInterface:
				if (!IsEndOfNameInExplicitInterface())
				{
					continue;
				}
				break;
			case TerminatorState.IsEndOfFunctionPointerParameterList:
				if (!IsEndOfFunctionPointerParameterList(errored: false))
				{
					continue;
				}
				break;
			case TerminatorState.IsEndOfFunctionPointerParameterListErrored:
				if (!IsEndOfFunctionPointerParameterList(errored: true))
				{
					continue;
				}
				break;
			case TerminatorState.IsEndOfFunctionPointerCallingConvention:
				if (!IsEndOfFunctionPointerCallingConvention())
				{
					continue;
				}
				break;
			case TerminatorState.IsEndOfTypeSignature:
				if (!IsEndOfTypeSignature())
				{
					continue;
				}
				break;
			default:
				continue;
			}
			return true;
		}
		return false;
	}

	private static Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode? GetOldParent(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode node)
	{
		return node?.Parent;
	}

	internal CompilationUnitSyntax ParseCompilationUnit()
	{
		return ParseWithStackGuard((LanguageParser @this) => @this.ParseCompilationUnitCore(), (LanguageParser @this) => SyntaxFactory.CompilationUnit(default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ExternAliasDirectiveSyntax>), default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<UsingDirectiveSyntax>), default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>), default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax>), SyntaxFactory.Token(SyntaxKind.EndOfFileToken)));
	}

	internal CompilationUnitSyntax ParseCompilationUnitCore()
	{
		SyntaxToken openBraceOrSemicolon = null;
		SyntaxListBuilder initialBadNodes = null;
		NamespaceBodyBuilder body = new NamespaceBodyBuilder(_pool);
		try
		{
			ParseNamespaceBody(ref openBraceOrSemicolon, ref body, ref initialBadNodes, SyntaxKind.CompilationUnit);
			SyntaxToken endOfFileToken = EatToken(SyntaxKind.EndOfFileToken);
			CompilationUnitSyntax compilationUnitSyntax = _syntaxFactory.CompilationUnit(body.Externs, body.Usings, body.Attributes, body.Members, endOfFileToken);
			if (initialBadNodes != null)
			{
				compilationUnitSyntax = AddLeadingSkippedSyntax(compilationUnitSyntax, initialBadNodes.ToListNode());
				_pool.Free(initialBadNodes);
			}
			return compilationUnitSyntax;
		}
		finally
		{
			body.Free(_pool);
		}
	}

	internal TNode ParseWithStackGuard<TNode>(Func<LanguageParser, TNode> parseFunc, Func<LanguageParser, TNode> createEmptyNodeFunc) where TNode : CSharpSyntaxNode
	{
		try
		{
			return parseFunc(this);
		}
		catch (InsufficientExecutionStackException)
		{
			return CreateForGlobalFailure(lexer.TextWindow.Position, createEmptyNodeFunc(this));
		}
	}

	private TNode CreateForGlobalFailure<TNode>(int position, TNode node) where TNode : CSharpSyntaxNode
	{
		SyntaxListBuilder syntaxListBuilder = new SyntaxListBuilder(1);
		syntaxListBuilder.Add(SyntaxFactory.BadToken(null, lexer.TextWindow.Text.ToString(), null));
		SkippedTokensTriviaSyntax skippedSyntax = _syntaxFactory.SkippedTokensTrivia(syntaxListBuilder.ToList<SyntaxToken>());
		node = AddLeadingSkippedSyntax(node, skippedSyntax);
		ForceEndOfFile();
		return AddError(node, position, 0, ErrorCode.ERR_InsufficientStack);
	}

	private BaseNamespaceDeclarationSyntax ParseNamespaceDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxListBuilder modifiers)
	{
		_recursionDepth++;
		StackGuard.EnsureSufficientExecutionStack(_recursionDepth);
		BaseNamespaceDeclarationSyntax result = ParseNamespaceDeclarationCore(attributeLists, modifiers);
		_recursionDepth--;
		return result;
	}

	private BaseNamespaceDeclarationSyntax ParseNamespaceDeclarationCore(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxListBuilder modifiers)
	{
		SyntaxToken syntaxToken = EatToken(SyntaxKind.NamespaceKeyword);
		if (base.IsScript)
		{
			syntaxToken = AddError(syntaxToken, ErrorCode.ERR_NamespaceNotAllowedInScript);
		}
		NameSyntax name = ParseQualifiedName();
		SyntaxToken openBraceOrSemicolon = null;
		SyntaxToken openBraceOrSemicolon2 = null;
		if (base.CurrentToken.Kind != SyntaxKind.SemicolonToken)
		{
			openBraceOrSemicolon = ((base.CurrentToken.Kind != SyntaxKind.OpenBraceToken && !IsPossibleNamespaceMemberDeclaration()) ? ConvertToMissingWithTrailingTrivia(EatTokenEvenWithIncorrectKind(SyntaxKind.OpenBraceToken), SyntaxKind.OpenBraceToken) : EatToken(SyntaxKind.OpenBraceToken));
		}
		else
		{
			openBraceOrSemicolon2 = EatToken(SyntaxKind.SemicolonToken);
		}
		NamespaceBodyBuilder body = new NamespaceBodyBuilder(_pool);
		try
		{
			if (openBraceOrSemicolon == null)
			{
				SyntaxListBuilder initialBadNodes = null;
				ParseNamespaceBody(ref openBraceOrSemicolon2, ref body, ref initialBadNodes, SyntaxKind.FileScopedNamespaceDeclaration);
				return _syntaxFactory.FileScopedNamespaceDeclaration(attributeLists, modifiers.ToList(), syntaxToken, name, openBraceOrSemicolon2, body.Externs, body.Usings, body.Members);
			}
			SyntaxListBuilder initialBadNodes2 = null;
			ParseNamespaceBody(ref openBraceOrSemicolon, ref body, ref initialBadNodes2, SyntaxKind.NamespaceDeclaration);
			return _syntaxFactory.NamespaceDeclaration(attributeLists, modifiers.ToList(), syntaxToken, name, openBraceOrSemicolon, body.Externs, body.Usings, body.Members, EatToken(SyntaxKind.CloseBraceToken), TryEatToken(SyntaxKind.SemicolonToken));
		}
		finally
		{
			body.Free(_pool);
		}
	}

	private static bool IsPossibleStartOfTypeDeclaration(SyntaxKind kind)
	{
		if (!IsTypeModifierOrTypeKeyword(kind))
		{
			return kind == SyntaxKind.OpenBracketToken;
		}
		return true;
	}

	private static bool IsTypeModifierOrTypeKeyword(SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.PublicKeyword:
		case SyntaxKind.PrivateKeyword:
		case SyntaxKind.InternalKeyword:
		case SyntaxKind.ProtectedKeyword:
		case SyntaxKind.StaticKeyword:
		case SyntaxKind.SealedKeyword:
		case SyntaxKind.NewKeyword:
		case SyntaxKind.AbstractKeyword:
		case SyntaxKind.ClassKeyword:
		case SyntaxKind.StructKeyword:
		case SyntaxKind.InterfaceKeyword:
		case SyntaxKind.EnumKeyword:
		case SyntaxKind.DelegateKeyword:
		case SyntaxKind.UnsafeKeyword:
			return true;
		default:
			return false;
		}
	}

	private void AddSkippedNamespaceText(ref SyntaxToken? openBraceOrSemicolon, ref NamespaceBodyBuilder body, ref SyntaxListBuilder? initialBadNodes, CSharpSyntaxNode skippedSyntax)
	{
		if (body.Members.Count > 0)
		{
			AddTrailingSkippedSyntax(body.Members, skippedSyntax);
			return;
		}
		if (body.Attributes.Count > 0)
		{
			AddTrailingSkippedSyntax(body.Attributes, skippedSyntax);
			return;
		}
		if (body.Usings.Count > 0)
		{
			AddTrailingSkippedSyntax(body.Usings, skippedSyntax);
			return;
		}
		if (body.Externs.Count > 0)
		{
			AddTrailingSkippedSyntax(body.Externs, skippedSyntax);
			return;
		}
		if (openBraceOrSemicolon != null)
		{
			openBraceOrSemicolon = AddTrailingSkippedSyntax(openBraceOrSemicolon, skippedSyntax);
			return;
		}
		if (initialBadNodes == null)
		{
			initialBadNodes = _pool.Allocate();
		}
		initialBadNodes.AddRange(skippedSyntax);
	}

	private void ParseNamespaceBody([NotNullIfNotNull("openBraceOrSemicolon")] ref SyntaxToken? openBraceOrSemicolon, ref NamespaceBodyBuilder body, ref SyntaxListBuilder? initialBadNodes, SyntaxKind parentKind)
	{
		ParseNamespaceBodyWorker(ref openBraceOrSemicolon, ref body, ref initialBadNodes, parentKind, out var sawMemberDeclarationOnlyValidWithinTypeDeclaration);
		if (!sawMemberDeclarationOnlyValidWithinTypeDeclaration || (base.IsScript && parentKind == SyntaxKind.CompilationUnit))
		{
			return;
		}
		SyntaxListBuilder<MemberDeclarationSyntax> members = _pool.Allocate<MemberDeclarationSyntax>();
		int num = 0;
		while (num < body.Members.Count)
		{
			MemberDeclarationSyntax memberDeclarationSyntax = body.Members[num];
			if (memberDeclarationSyntax is TypeDeclarationSyntax { SemicolonToken: null } typeDeclarationSyntax)
			{
				SyntaxToken closeBraceToken = typeDeclarationSyntax.CloseBraceToken;
				if (closeBraceToken != null && !closeBraceToken.IsMissing && !closeBraceToken.ContainsDiagnostics)
				{
					(int, int)? tuple = determineSiblingsToMoveIntoType(num, in body);
					if (tuple.HasValue)
					{
						(int, int) valueOrDefault = tuple.GetValueOrDefault();
						int item = valueOrDefault.Item1;
						int item2 = valueOrDefault.Item2;
						TypeDeclarationSyntax node = moveSiblingMembersIntoPrecedingType(typeDeclarationSyntax, in body, item, item2);
						members.Add(node);
						num = item2;
						continue;
					}
				}
			}
			members.Add(memberDeclarationSyntax);
			num++;
		}
		_pool.Free(body.Members);
		body.Members = members;
		static (int firstSiblingToMoveInclusive, int lastSiblingToMoveExclusive)? determineSiblingsToMoveIntoType(int typeDeclarationIndex, in NamespaceBodyBuilder reference)
		{
			int num2 = typeDeclarationIndex + 1;
			if (num2 < reference.Members.Count && IsMemberDeclarationOnlyValidWithinTypeDeclaration(reference.Members[num2]))
			{
				int i;
				for (i = num2 + 1; i < reference.Members.Count && IsMemberDeclarationOnlyValidWithinTypeDeclaration(reference.Members[i]); i++)
				{
				}
				return (num2, i);
			}
			return null;
		}
		TypeDeclarationSyntax moveSiblingMembersIntoPrecedingType(TypeDeclarationSyntax typeDeclaration, in NamespaceBodyBuilder reference, int firstSiblingToMoveInclusive, int lastSiblingToMoveExclusive)
		{
			SyntaxListBuilder<MemberDeclarationSyntax> item3 = _pool.Allocate<MemberDeclarationSyntax>();
			item3.AddRange(typeDeclaration.Members);
			for (int i = firstSiblingToMoveInclusive; i < lastSiblingToMoveExclusive; i++)
			{
				MemberDeclarationSyntax node2 = reference.Members[i];
				if (i == firstSiblingToMoveInclusive)
				{
					node2 = AddLeadingSkippedSyntax(node2, AddError(typeDeclaration.CloseBraceToken, ErrorCode.ERR_InvalidMemberDecl, "}"));
				}
				item3.Add(node2);
			}
			SyntaxToken closeBraceToken2 = ((lastSiblingToMoveExclusive == reference.Members.Count) ? EatToken(SyntaxKind.CloseBraceToken) : AddError(SyntaxFactory.MissingToken(SyntaxKind.CloseBraceToken), ErrorCode.ERR_RbraceExpected));
			Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members2 = _pool.ToListAndFree(item3);
			return typeDeclaration.UpdateCore(typeDeclaration.AttributeLists, typeDeclaration.Modifiers, typeDeclaration.Keyword, typeDeclaration.Identifier, typeDeclaration.TypeParameterList, typeDeclaration.ParameterList, typeDeclaration.BaseList, typeDeclaration.ConstraintClauses, typeDeclaration.OpenBraceToken, members2, closeBraceToken2, typeDeclaration.SemicolonToken);
		}
	}

	private static bool IsMemberDeclarationOnlyValidWithinTypeDeclaration(MemberDeclarationSyntax? memberDeclaration)
	{
		switch (memberDeclaration?.Kind)
		{
		case SyntaxKind.FieldDeclaration:
		case SyntaxKind.EventFieldDeclaration:
		case SyntaxKind.MethodDeclaration:
		case SyntaxKind.OperatorDeclaration:
		case SyntaxKind.ConversionOperatorDeclaration:
		case SyntaxKind.ConstructorDeclaration:
		case SyntaxKind.DestructorDeclaration:
		case SyntaxKind.PropertyDeclaration:
		case SyntaxKind.EventDeclaration:
		case SyntaxKind.IndexerDeclaration:
			return true;
		default:
			return false;
		}
	}

	private void ParseNamespaceBodyWorker([NotNullIfNotNull("openBraceOrSemicolon")] ref SyntaxToken? openBraceOrSemicolon, ref NamespaceBodyBuilder body, ref SyntaxListBuilder? initialBadNodes, SyntaxKind parentKind, out bool sawMemberDeclarationOnlyValidWithinTypeDeclaration)
	{
		bool flag = openBraceOrSemicolon == null;
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsNamespaceMemberStartOrStop;
		NamespaceParts seen = NamespaceParts.None;
		SyntaxListBuilder<MemberDeclarationSyntax> pendingIncompleteMembers = _pool.Allocate<MemberDeclarationSyntax>();
		bool flag2 = true;
		sawMemberDeclarationOnlyValidWithinTypeDeclaration = false;
		try
		{
			while (true)
			{
				switch (base.CurrentToken.Kind)
				{
				case SyntaxKind.NamespaceKeyword:
				{
					AddIncompleteMembers(ref pendingIncompleteMembers, ref body);
					SyntaxListBuilder<AttributeListSyntax> syntaxListBuilder = _pool.Allocate<AttributeListSyntax>();
					SyntaxListBuilder syntaxListBuilder2 = _pool.Allocate();
					body.Members.Add(adjustStateAndReportStatementOutOfOrder(ref seen, ParseNamespaceDeclaration(syntaxListBuilder, syntaxListBuilder2)));
					_pool.Free(syntaxListBuilder);
					_pool.Free(syntaxListBuilder2);
					flag2 = true;
					continue;
				}
				case SyntaxKind.CloseBraceToken:
					if (flag)
					{
						ReduceIncompleteMembers(ref pendingIncompleteMembers, ref openBraceOrSemicolon, ref body, ref initialBadNodes);
						SyntaxToken node2 = EatToken();
						node2 = AddError(node2, base.IsScript ? ErrorCode.ERR_GlobalDefinitionOrStatementExpected : ErrorCode.ERR_EOFExpected);
						AddSkippedNamespaceText(ref openBraceOrSemicolon, ref body, ref initialBadNodes, node2);
						flag2 = true;
						continue;
					}
					return;
				case SyntaxKind.EndOfFileToken:
					return;
				case SyntaxKind.ExternKeyword:
					if (!flag || ScanExternAliasDirective())
					{
						ReduceIncompleteMembers(ref pendingIncompleteMembers, ref openBraceOrSemicolon, ref body, ref initialBadNodes);
						ExternAliasDirectiveSyntax node = ParseExternAliasDirective();
						if (seen > NamespaceParts.ExternAliases)
						{
							node = AddErrorToFirstToken(node, ErrorCode.ERR_ExternAfterElements);
							AddSkippedNamespaceText(ref openBraceOrSemicolon, ref body, ref initialBadNodes, node);
						}
						else
						{
							body.Externs.Add(node);
							seen = NamespaceParts.ExternAliases;
						}
						flag2 = true;
						continue;
					}
					break;
				case SyntaxKind.UsingKeyword:
					if (!flag || (PeekToken(1).Kind != SyntaxKind.OpenParenToken && (base.IsScript || !IsPossibleTopLevelUsingLocalDeclarationStatement())))
					{
						parseUsingDirective(ref openBraceOrSemicolon, ref body, ref initialBadNodes, ref seen, ref pendingIncompleteMembers);
						flag2 = true;
						continue;
					}
					break;
				case SyntaxKind.IdentifierToken:
					if (base.CurrentToken.ContextualKind == SyntaxKind.GlobalKeyword && PeekToken(1).Kind == SyntaxKind.UsingKeyword)
					{
						parseUsingDirective(ref openBraceOrSemicolon, ref body, ref initialBadNodes, ref seen, ref pendingIncompleteMembers);
						flag2 = true;
						continue;
					}
					break;
				case SyntaxKind.OpenBracketToken:
				{
					if (!IsPossibleGlobalAttributeDeclaration())
					{
						break;
					}
					AttributeListSyntax attributeListSyntax = TryParseAttributeDeclaration(parentKind == SyntaxKind.CompilationUnit);
					if (attributeListSyntax != null)
					{
						ReduceIncompleteMembers(ref pendingIncompleteMembers, ref openBraceOrSemicolon, ref body, ref initialBadNodes);
						if (!flag || seen > NamespaceParts.GlobalAttributes)
						{
							attributeListSyntax = attributeListSyntax.Update(attributeListSyntax.OpenBracketToken, attributeListSyntax.Target.Update(AddError(attributeListSyntax.Target.Identifier, ErrorCode.ERR_GlobalAttributesNotFirst), attributeListSyntax.Target.ColonToken), attributeListSyntax.Attributes, attributeListSyntax.CloseBracketToken);
							AddSkippedNamespaceText(ref openBraceOrSemicolon, ref body, ref initialBadNodes, attributeListSyntax);
						}
						else
						{
							body.Attributes.Add(attributeListSyntax);
							seen = NamespaceParts.GlobalAttributes;
						}
						flag2 = true;
						continue;
					}
					break;
				}
				}
				MemberDeclarationSyntax memberDeclarationSyntax = (flag ? ParseMemberDeclarationOrStatement(parentKind) : ParseMemberDeclaration(parentKind));
				sawMemberDeclarationOnlyValidWithinTypeDeclaration |= IsMemberDeclarationOnlyValidWithinTypeDeclaration(memberDeclarationSyntax);
				if (memberDeclarationSyntax == null)
				{
					ReduceIncompleteMembers(ref pendingIncompleteMembers, ref openBraceOrSemicolon, ref body, ref initialBadNodes);
					SyntaxToken syntaxToken = EatToken();
					if (flag2 && !syntaxToken.ContainsDiagnostics)
					{
						syntaxToken = AddError(syntaxToken, base.IsScript ? ErrorCode.ERR_GlobalDefinitionOrStatementExpected : ErrorCode.ERR_EOFExpected);
						flag2 = false;
					}
					AddSkippedNamespaceText(ref openBraceOrSemicolon, ref body, ref initialBadNodes, syntaxToken);
				}
				else if (memberDeclarationSyntax.Kind == SyntaxKind.IncompleteMember && seen < NamespaceParts.MembersAndStatements)
				{
					pendingIncompleteMembers.Add(memberDeclarationSyntax);
					flag2 = true;
				}
				else
				{
					AddIncompleteMembers(ref pendingIncompleteMembers, ref body);
					body.Members.Add(adjustStateAndReportStatementOutOfOrder(ref seen, memberDeclarationSyntax));
					flag2 = true;
				}
			}
		}
		finally
		{
			_termState = termState;
			AddIncompleteMembers(ref pendingIncompleteMembers, ref body);
			_pool.Free(pendingIncompleteMembers);
		}
		MemberDeclarationSyntax adjustStateAndReportStatementOutOfOrder(ref NamespaceParts reference, MemberDeclarationSyntax memberOrStatement)
		{
			switch (memberOrStatement.Kind)
			{
			case SyntaxKind.GlobalStatement:
				if (reference < NamespaceParts.MembersAndStatements)
				{
					reference = NamespaceParts.MembersAndStatements;
				}
				else if (reference == NamespaceParts.TypesAndNamespaces)
				{
					reference = NamespaceParts.TopLevelStatementsAfterTypesAndNamespaces;
					if (!base.IsScript)
					{
						memberOrStatement = AddError(memberOrStatement, ErrorCode.ERR_TopLevelStatementAfterNamespaceOrType);
					}
				}
				break;
			case SyntaxKind.NamespaceDeclaration:
			case SyntaxKind.FileScopedNamespaceDeclaration:
			case SyntaxKind.ClassDeclaration:
			case SyntaxKind.StructDeclaration:
			case SyntaxKind.InterfaceDeclaration:
			case SyntaxKind.EnumDeclaration:
			case SyntaxKind.DelegateDeclaration:
			case SyntaxKind.RecordDeclaration:
			case SyntaxKind.RecordStructDeclaration:
				if (reference < NamespaceParts.TypesAndNamespaces)
				{
					reference = NamespaceParts.TypesAndNamespaces;
				}
				break;
			default:
				if (reference < NamespaceParts.MembersAndStatements)
				{
					reference = NamespaceParts.MembersAndStatements;
				}
				break;
			}
			return memberOrStatement;
		}
		void parseUsingDirective(ref SyntaxToken? openBrace, ref NamespaceBodyBuilder reference, ref SyntaxListBuilder? initialBadNodes2, ref NamespaceParts reference2, ref SyntaxListBuilder<MemberDeclarationSyntax> incompleteMembers)
		{
			ReduceIncompleteMembers(ref incompleteMembers, ref openBrace, ref reference, ref initialBadNodes2);
			UsingDirectiveSyntax node3 = ParseUsingDirective();
			if (reference2 > NamespaceParts.Usings)
			{
				node3 = AddError(node3, ErrorCode.ERR_UsingAfterElements);
				AddSkippedNamespaceText(ref openBrace, ref reference, ref initialBadNodes2, node3);
			}
			else
			{
				reference.Usings.Add(node3);
				reference2 = NamespaceParts.Usings;
			}
		}
	}

	private static void AddIncompleteMembers(ref SyntaxListBuilder<MemberDeclarationSyntax> incompleteMembers, ref NamespaceBodyBuilder body)
	{
		if (incompleteMembers.Count > 0)
		{
			body.Members.AddRange(incompleteMembers);
			incompleteMembers.Clear();
		}
	}

	private void ReduceIncompleteMembers(ref SyntaxListBuilder<MemberDeclarationSyntax> incompleteMembers, ref SyntaxToken? openBraceOrSemicolon, ref NamespaceBodyBuilder body, ref SyntaxListBuilder? initialBadNodes)
	{
		for (int i = 0; i < incompleteMembers.Count; i++)
		{
			AddSkippedNamespaceText(ref openBraceOrSemicolon, ref body, ref initialBadNodes, incompleteMembers[i]);
		}
		incompleteMembers.Clear();
	}

	private bool IsPossibleNamespaceMemberDeclaration()
	{
		switch (base.CurrentToken.Kind)
		{
		case SyntaxKind.ExternKeyword:
		case SyntaxKind.NamespaceKeyword:
		case SyntaxKind.UsingKeyword:
			return true;
		case SyntaxKind.IdentifierToken:
			return IsPartialInNamespaceMemberDeclaration();
		default:
			return IsPossibleStartOfTypeDeclaration(base.CurrentToken.Kind);
		}
	}

	private bool IsPartialInNamespaceMemberDeclaration()
	{
		if (base.CurrentToken.ContextualKind == SyntaxKind.PartialKeyword)
		{
			if (IsPartialType())
			{
				return true;
			}
			if (PeekToken(1).Kind == SyntaxKind.NamespaceKeyword)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsEndOfNamespace()
	{
		return base.CurrentToken.Kind == SyntaxKind.CloseBraceToken;
	}

	public bool IsGobalAttributesTerminator()
	{
		if (!IsEndOfNamespace())
		{
			return IsPossibleNamespaceMemberDeclaration();
		}
		return true;
	}

	private bool IsNamespaceMemberStartOrStop()
	{
		if (!IsEndOfNamespace())
		{
			return IsPossibleNamespaceMemberDeclaration();
		}
		return true;
	}

	private bool ScanExternAliasDirective()
	{
		if (base.CurrentToken.Kind == SyntaxKind.ExternKeyword)
		{
			SyntaxToken syntaxToken = PeekToken(1);
			if (syntaxToken != null && syntaxToken.Kind == SyntaxKind.IdentifierToken && syntaxToken.ContextualKind == SyntaxKind.AliasKeyword && PeekToken(2).Kind == SyntaxKind.IdentifierToken)
			{
				return PeekToken(3).Kind == SyntaxKind.SemicolonToken;
			}
		}
		return false;
	}

	private ExternAliasDirectiveSyntax ParseExternAliasDirective()
	{
		if (IsIncrementalAndFactoryContextMatches && base.CurrentNodeKind == SyntaxKind.ExternAliasDirective)
		{
			return (ExternAliasDirectiveSyntax)EatNode();
		}
		return _syntaxFactory.ExternAliasDirective(EatToken(SyntaxKind.ExternKeyword), EatContextualToken(SyntaxKind.AliasKeyword), ParseIdentifierToken(), EatToken(SyntaxKind.SemicolonToken));
	}

	private NameEqualsSyntax ParseNameEquals()
	{
		return _syntaxFactory.NameEquals(_syntaxFactory.IdentifierName(ParseIdentifierToken()), EatToken(SyntaxKind.EqualsToken));
	}

	private UsingDirectiveSyntax ParseUsingDirective()
	{
		if (IsIncrementalAndFactoryContextMatches && base.CurrentNodeKind == SyntaxKind.UsingDirective)
		{
			return (UsingDirectiveSyntax)EatNode();
		}
		SyntaxToken globalKeyword = ((base.CurrentToken.ContextualKind == SyntaxKind.GlobalKeyword) ? SyntaxParser.ConvertToKeyword(EatToken()) : null);
		SyntaxToken usingKeyword = EatToken(SyntaxKind.UsingKeyword);
		SyntaxToken syntaxToken = TryEatToken(SyntaxKind.StaticKeyword);
		SyntaxToken syntaxToken2 = TryEatToken(SyntaxKind.UnsafeKeyword);
		if (syntaxToken == null && syntaxToken2 != null && base.CurrentToken.Kind == SyntaxKind.StaticKeyword)
		{
			syntaxToken = SyntaxFactory.MissingToken(SyntaxKind.StaticKeyword);
			syntaxToken2 = AddTrailingSkippedSyntax(syntaxToken2, AddError(EatToken(), ErrorCode.ERR_BadStaticAfterUnsafe));
		}
		NameEqualsSyntax nameEqualsSyntax = (IsNamedAssignment() ? ParseNameEquals() : null);
		TypeSyntax typeSyntax;
		SyntaxToken semicolonToken;
		if ((nameEqualsSyntax == null || base.CurrentToken.Kind != SyntaxKind.DelegateKeyword) && IsPossibleNamespaceMemberDeclaration())
		{
			typeSyntax = _syntaxFactory.IdentifierName(CreateMissingToken(SyntaxKind.IdentifierToken, base.CurrentToken.Kind));
			semicolonToken = SyntaxFactory.MissingToken(SyntaxKind.SemicolonToken);
		}
		else
		{
			typeSyntax = ((nameEqualsSyntax == null) ? ParseQualifiedName() : ParseType());
			if (typeSyntax.IsMissing && PeekToken(1).Kind == SyntaxKind.SemicolonToken)
			{
				typeSyntax = AddTrailingSkippedSyntax(typeSyntax, EatToken());
			}
			semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		}
		return _syntaxFactory.UsingDirective(globalKeyword, usingKeyword, syntaxToken, syntaxToken2, nameEqualsSyntax, typeSyntax, semicolonToken);
	}

	private bool IsPossibleGlobalAttributeDeclaration()
	{
		if (base.CurrentToken.Kind == SyntaxKind.OpenBracketToken && IsGlobalAttributeTarget(PeekToken(1)))
		{
			return PeekToken(2).Kind == SyntaxKind.ColonToken;
		}
		return false;
	}

	private static bool IsGlobalAttributeTarget(SyntaxToken token)
	{
		AttributeLocation attributeLocation = token.ToAttributeLocation();
		if ((uint)(attributeLocation - 1) <= 1u)
		{
			return true;
		}
		return false;
	}

	private bool IsPossibleAttributeDeclaration()
	{
		if (base.CurrentToken.Kind != SyntaxKind.OpenBracketToken)
		{
			return false;
		}
		using (GetDisposableResetPoint(resetOnDispose: true))
		{
			EatToken();
			if (IsTrueIdentifier())
			{
				return true;
			}
			if (IsAttributeTarget())
			{
				return true;
			}
			if (SyntaxFacts.IsLiteralExpression(base.CurrentToken.Kind))
			{
				return false;
			}
			return true;
		}
	}

	private Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> ParseAttributeDeclarations(bool inExpressionContext)
	{
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsAttributeDeclarationTerminator;
		if (termState == _termState)
		{
			return default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>);
		}
		SyntaxListBuilder<AttributeListSyntax> item = _pool.Allocate<AttributeListSyntax>();
		while (IsPossibleAttributeDeclaration())
		{
			AttributeListSyntax attributeListSyntax = TryParseAttributeDeclaration(inExpressionContext);
			if (attributeListSyntax == null)
			{
				break;
			}
			item.Add(attributeListSyntax);
		}
		_termState = termState;
		return _pool.ToListAndFree(item);
	}

	private bool IsAttributeDeclarationTerminator()
	{
		if (base.CurrentToken.Kind != SyntaxKind.CloseBracketToken)
		{
			return IsPossibleAttributeDeclaration();
		}
		return true;
	}

	private bool IsAttributeTarget()
	{
		if (IsSomeWord(base.CurrentToken.Kind))
		{
			return PeekToken(1).Kind == SyntaxKind.ColonToken;
		}
		return false;
	}

	private AttributeListSyntax? TryParseAttributeDeclaration(bool inExpressionContext)
	{
		if (IsIncrementalAndFactoryContextMatches && base.CurrentNodeKind == SyntaxKind.AttributeList && !inExpressionContext)
		{
			return (AttributeListSyntax)EatNode();
		}
		using (DisposableResetPoint disposableResetPoint = GetDisposableResetPoint(resetOnDispose: false))
		{
			SyntaxToken openToken = EatToken(SyntaxKind.OpenBracketToken);
			AttributeTargetSpecifierSyntax target = (IsAttributeTarget() ? _syntaxFactory.AttributeTargetSpecifier(SyntaxParser.ConvertToKeyword(EatToken()), EatToken(SyntaxKind.ColonToken)) : null);
			Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<AttributeSyntax> attributes = ParseCommaSeparatedSyntaxList(ref openToken, SyntaxKind.CloseBracketToken, (LanguageParser @this) => @this.IsPossibleAttribute(), (LanguageParser @this) => @this.ParseAttribute(), skipBadAttributeListTokens, allowTrailingSeparator: true, requireOneElement: true, allowSemicolonAsSeparator: false);
			SyntaxToken closeBracketToken = EatToken(SyntaxKind.CloseBracketToken);
			if (inExpressionContext && shouldParseAsCollectionExpression())
			{
				disposableResetPoint.Reset();
				return null;
			}
			return _syntaxFactory.AttributeList(openToken, target, attributes, closeBracketToken);
		}
		bool shouldParseAsCollectionExpression()
		{
			if (base.CurrentToken.Kind == SyntaxKind.DotToken)
			{
				return true;
			}
			if (base.CurrentToken.Kind == SyntaxKind.MinusGreaterThanToken)
			{
				return true;
			}
			if (base.CurrentToken.Kind == SyntaxKind.QuestionToken && PeekToken(1).Kind == SyntaxKind.DotToken)
			{
				return true;
			}
			return false;
		}
		static PostSkipAction skipBadAttributeListTokens(LanguageParser @this, ref SyntaxToken openBracket, SeparatedSyntaxListBuilder<AttributeSyntax> list, SyntaxKind expectedKind, SyntaxKind closeKind)
		{
			return @this.SkipBadSeparatedListTokensWithExpectedKind(ref openBracket, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleAttribute(), (LanguageParser p, SyntaxKind syntaxKind) => p.CurrentToken.Kind == syntaxKind, expectedKind, closeKind);
		}
	}

	private bool IsPossibleAttribute()
	{
		return IsTrueIdentifier();
	}

	private AttributeSyntax ParseAttribute()
	{
		if (IsIncrementalAndFactoryContextMatches && base.CurrentNodeKind == SyntaxKind.Attribute)
		{
			return (AttributeSyntax)EatNode();
		}
		return _syntaxFactory.Attribute(ParseQualifiedName(), ParseAttributeArgumentList());
	}

	internal AttributeArgumentListSyntax? ParseAttributeArgumentList()
	{
		if (IsIncrementalAndFactoryContextMatches && base.CurrentNodeKind == SyntaxKind.AttributeArgumentList)
		{
			return (AttributeArgumentListSyntax)EatNode();
		}
		if (base.CurrentToken.Kind != SyntaxKind.OpenParenToken)
		{
			return null;
		}
		SyntaxToken openToken = EatToken(SyntaxKind.OpenParenToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<AttributeArgumentSyntax> arguments = ParseCommaSeparatedSyntaxList(ref openToken, SyntaxKind.CloseParenToken, (LanguageParser @this) => @this.IsPossibleAttributeArgument(), (LanguageParser @this) => @this.ParseAttributeArgument(), immediatelyAbort, skipBadAttributeArgumentTokens, allowTrailingSeparator: false, requireOneElement: false, allowSemicolonAsSeparator: false);
		return _syntaxFactory.AttributeArgumentList(openToken, arguments, EatToken(SyntaxKind.CloseParenToken));
		static bool immediatelyAbort(AttributeArgumentSyntax argument)
		{
			ExpressionSyntax expression = argument.expression;
			if (expression is LiteralExpressionSyntax literalExpressionSyntax && expression.Kind == SyntaxKind.StringLiteralExpression)
			{
				SyntaxToken token = literalExpressionSyntax.Token;
				if (token.GetDiagnostics().Contains((DiagnosticInfo d) => d.Code == 1010))
				{
					return true;
				}
			}
			if (argument.expression is InterpolatedStringExpressionSyntax interpolatedStringExpressionSyntax)
			{
				SyntaxToken stringStartToken = interpolatedStringExpressionSyntax.StringStartToken;
				if (stringStartToken != null && stringStartToken.Kind == SyntaxKind.InterpolatedStringStartToken)
				{
					SyntaxToken stringEndToken = interpolatedStringExpressionSyntax.StringEndToken;
					if (stringEndToken != null && stringEndToken.IsMissing)
					{
						return true;
					}
				}
			}
			return false;
		}
		static PostSkipAction skipBadAttributeArgumentTokens(LanguageParser @this, ref SyntaxToken openParen, SeparatedSyntaxListBuilder<AttributeArgumentSyntax> list, SyntaxKind expectedKind, SyntaxKind closeKind)
		{
			return @this.SkipBadSeparatedListTokensWithExpectedKind(ref openParen, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleAttributeArgument(), (LanguageParser p, SyntaxKind syntaxKind) => p.CurrentToken.Kind == syntaxKind, expectedKind, closeKind);
		}
	}

	private bool IsPossibleAttributeArgument()
	{
		return IsPossibleExpression();
	}

	private AttributeArgumentSyntax ParseAttributeArgument()
	{
		NameEqualsSyntax nameEquals = null;
		NameColonSyntax nameColon = null;
		if (base.CurrentToken.Kind == SyntaxKind.IdentifierToken)
		{
			switch (PeekToken(1).Kind)
			{
			case SyntaxKind.EqualsToken:
				nameEquals = _syntaxFactory.NameEquals(_syntaxFactory.IdentifierName(ParseIdentifierToken()), EatToken(SyntaxKind.EqualsToken));
				break;
			case SyntaxKind.ColonToken:
				nameColon = _syntaxFactory.NameColon(ParseIdentifierName(), EatToken(SyntaxKind.ColonToken));
				break;
			}
		}
		return _syntaxFactory.AttributeArgument(nameEquals, nameColon, ParseExpressionCore());
	}

	private static DeclarationModifiers GetModifierExcludingScoped(SyntaxToken token)
	{
		return GetModifierExcludingScoped(token.Kind, token.ContextualKind);
	}

	internal static DeclarationModifiers GetModifierExcludingScoped(SyntaxKind kind, SyntaxKind contextualKind)
	{
		switch (kind)
		{
		case SyntaxKind.PublicKeyword:
			return DeclarationModifiers.Public;
		case SyntaxKind.InternalKeyword:
			return DeclarationModifiers.Internal;
		case SyntaxKind.ProtectedKeyword:
			return DeclarationModifiers.Protected;
		case SyntaxKind.PrivateKeyword:
			return DeclarationModifiers.Private;
		case SyntaxKind.SealedKeyword:
			return DeclarationModifiers.Sealed;
		case SyntaxKind.AbstractKeyword:
			return DeclarationModifiers.Abstract;
		case SyntaxKind.StaticKeyword:
			return DeclarationModifiers.Static;
		case SyntaxKind.VirtualKeyword:
			return DeclarationModifiers.Virtual;
		case SyntaxKind.ExternKeyword:
			return DeclarationModifiers.Extern;
		case SyntaxKind.NewKeyword:
			return DeclarationModifiers.New;
		case SyntaxKind.OverrideKeyword:
			return DeclarationModifiers.Override;
		case SyntaxKind.ReadOnlyKeyword:
			return DeclarationModifiers.ReadOnly;
		case SyntaxKind.VolatileKeyword:
			return DeclarationModifiers.Volatile;
		case SyntaxKind.UnsafeKeyword:
			return DeclarationModifiers.Unsafe;
		case SyntaxKind.PartialKeyword:
			return DeclarationModifiers.Partial;
		case SyntaxKind.AsyncKeyword:
			return DeclarationModifiers.Async;
		case SyntaxKind.RefKeyword:
			return DeclarationModifiers.Ref;
		case SyntaxKind.IdentifierToken:
			switch (contextualKind)
			{
			case SyntaxKind.PartialKeyword:
				return DeclarationModifiers.Partial;
			case SyntaxKind.AsyncKeyword:
				return DeclarationModifiers.Async;
			case SyntaxKind.RequiredKeyword:
				return DeclarationModifiers.Required;
			case SyntaxKind.FileKeyword:
				return DeclarationModifiers.File;
			}
			break;
		}
		return DeclarationModifiers.None;
	}

	private void ParseModifiers(SyntaxListBuilder tokens, bool forAccessors, bool forTopLevelStatements, out bool isPossibleTypeDeclaration)
	{
		isPossibleTypeDeclaration = true;
		while (true)
		{
			SyntaxToken item;
			switch (GetModifierExcludingScoped(base.CurrentToken))
			{
			case DeclarationModifiers.None:
				if (!forAccessors)
				{
					SyntaxToken syntaxToken2 = ParsePossibleScopedKeyword(isFunctionPointerParameter: false, isLambdaParameter: false);
					if (syntaxToken2 != null)
					{
						isPossibleTypeDeclaration = false;
						tokens.Add(syntaxToken2);
					}
				}
				return;
			case DeclarationModifiers.Partial:
			{
				SyntaxToken syntaxToken = PeekToken(1);
				if (IsPartialType() || IsPartialMember())
				{
					item = SyntaxParser.ConvertToKeyword(EatToken());
					break;
				}
				if (syntaxToken.Kind == SyntaxKind.NamespaceKeyword)
				{
					item = SyntaxParser.ConvertToKeyword(EatToken());
					break;
				}
				SyntaxKind kind = syntaxToken.Kind;
				bool flag = kind - 8377 <= SyntaxKind.List;
				if (flag || (IsPossibleStartOfTypeDeclaration(syntaxToken.Kind) && GetModifierExcludingScoped(syntaxToken) != DeclarationModifiers.None))
				{
					item = SyntaxParser.ConvertToKeyword(EatToken());
					break;
				}
				return;
			}
			case DeclarationModifiers.Ref:
			{
				SyntaxToken syntaxToken3 = PeekToken(1);
				if (isStructOrRecordKeyword(syntaxToken3) || (syntaxToken3.ContextualKind == SyntaxKind.PartialKeyword && isStructOrRecordKeyword(PeekToken(2))))
				{
					item = EatToken();
					break;
				}
				if (forAccessors && IsPossibleAccessorModifier())
				{
					item = EatToken();
					break;
				}
				return;
			}
			case DeclarationModifiers.File:
				if ((!IsFeatureEnabled(MessageID.IDS_FeatureFileTypes) | forTopLevelStatements) && !ShouldContextualKeywordBeTreatedAsModifier(parsingStatementNotDeclaration: false))
				{
					return;
				}
				item = SyntaxParser.ConvertToKeyword(EatToken());
				break;
			case DeclarationModifiers.Async:
				if (!ShouldContextualKeywordBeTreatedAsModifier(parsingStatementNotDeclaration: false))
				{
					return;
				}
				item = SyntaxParser.ConvertToKeyword(EatToken());
				break;
			case DeclarationModifiers.Required:
				if ((!IsFeatureEnabled(MessageID.IDS_FeatureRequiredMembers) | forTopLevelStatements) && !ShouldContextualKeywordBeTreatedAsModifier(parsingStatementNotDeclaration: false))
				{
					return;
				}
				item = SyntaxParser.ConvertToKeyword(EatToken());
				break;
			default:
				item = EatToken();
				break;
			}
			tokens.Add(item);
		}
		bool isStructOrRecordKeyword(SyntaxToken token)
		{
			if (token.Kind == SyntaxKind.StructKeyword)
			{
				return true;
			}
			if (token.ContextualKind == SyntaxKind.RecordKeyword)
			{
				return IsFeatureEnabled(MessageID.IDS_FeatureRecords);
			}
			return false;
		}
	}

	private bool ShouldContextualKeywordBeTreatedAsModifier(bool parsingStatementNotDeclaration)
	{
		if (IsNonContextualModifier(PeekToken(1)))
		{
			return true;
		}
		bool flag;
		using (GetDisposableResetPoint(resetOnDispose: true))
		{
			EatToken();
			if (!parsingStatementNotDeclaration && base.CurrentToken.ContextualKind == SyntaxKind.PartialKeyword)
			{
				EatToken();
			}
			if (parsingStatementNotDeclaration)
			{
				goto IL_0095;
			}
			SyntaxKind kind = base.CurrentToken.Kind;
			flag = IsTypeModifierOrTypeKeyword(kind) || kind == SyntaxKind.EventKeyword;
			if (!flag)
			{
				bool flag2 = kind - 8383 <= SyntaxKind.List;
				flag = flag2 && PeekToken(1).Kind == SyntaxKind.OperatorKeyword;
			}
			if (!flag)
			{
				goto IL_0095;
			}
			flag = true;
			goto end_IL_0018;
			IL_0127:
			flag = false;
			goto end_IL_0018;
			IL_0095:
			if (ScanType() == ScanTypeFlags.NotType)
			{
				goto IL_0127;
			}
			if (!IsPossibleMemberName())
			{
				SyntaxKind kind2 = base.CurrentToken.Kind;
				switch (kind2)
				{
				case SyntaxKind.EndOfFileToken:
					flag = true;
					goto end_IL_0018;
				case SyntaxKind.CloseBraceToken:
					flag = true;
					goto end_IL_0018;
				default:
					if (SyntaxFacts.IsPredefinedType(base.CurrentToken.Kind))
					{
						flag = true;
					}
					else if (IsNonContextualModifier(base.CurrentToken))
					{
						flag = true;
					}
					else if (IsTypeDeclarationStart())
					{
						flag = true;
					}
					else if (kind2 == SyntaxKind.NamespaceKeyword)
					{
						flag = true;
					}
					else
					{
						if (parsingStatementNotDeclaration || kind2 != SyntaxKind.OperatorKeyword)
						{
							break;
						}
						flag = true;
					}
					goto end_IL_0018;
				}
				goto IL_0127;
			}
			flag = true;
			end_IL_0018:;
		}
		return flag;
	}

	private static bool IsNonContextualModifier(SyntaxToken nextToken)
	{
		if (!SyntaxFacts.IsContextualKeyword(nextToken.ContextualKind))
		{
			return GetModifierExcludingScoped(nextToken) != DeclarationModifiers.None;
		}
		return false;
	}

	private bool IsPartialType()
	{
		SyntaxToken syntaxToken = PeekToken(1);
		SyntaxKind kind = syntaxToken.Kind;
		if (kind - 8374 <= (SyntaxKind)2)
		{
			return true;
		}
		if (syntaxToken.ContextualKind == SyntaxKind.RecordKeyword)
		{
			return IsFeatureEnabled(MessageID.IDS_FeatureRecords);
		}
		return false;
	}

	private bool IsPartialMember()
	{
		if (PeekToken(1).Kind == SyntaxKind.EventKeyword)
		{
			return true;
		}
		if (PeekToken(1).Kind == SyntaxKind.IdentifierToken && PeekToken(2).Kind == SyntaxKind.OpenParenToken)
		{
			return IsFeatureEnabled(MessageID.IDS_FeaturePartialEventsAndConstructors);
		}
		using (GetDisposableResetPoint(resetOnDispose: true))
		{
			EatToken();
			if (ScanType() == ScanTypeFlags.NotType)
			{
				return false;
			}
			return IsPossibleMemberName();
		}
	}

	private bool IsPossibleMemberName()
	{
		switch (base.CurrentToken.Kind)
		{
		case SyntaxKind.IdentifierToken:
			if (base.CurrentToken.ContextualKind == SyntaxKind.GlobalKeyword && PeekToken(1).Kind == SyntaxKind.UsingKeyword)
			{
				return false;
			}
			return true;
		case SyntaxKind.ThisKeyword:
			return true;
		default:
			return false;
		}
	}

	private MemberDeclarationSyntax ParseTypeDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, SyntaxListBuilder modifiers)
	{
		CancellationToken cancellationToken = base.cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		return base.CurrentToken.Kind switch
		{
			SyntaxKind.ClassKeyword => ParseMainTypeDeclaration(attributes, modifiers), 
			SyntaxKind.StructKeyword => ParseMainTypeDeclaration(attributes, modifiers), 
			SyntaxKind.InterfaceKeyword => ParseMainTypeDeclaration(attributes, modifiers), 
			SyntaxKind.DelegateKeyword => ParseDelegateDeclaration(attributes, modifiers), 
			SyntaxKind.EnumKeyword => ParseEnumDeclaration(attributes, modifiers), 
			SyntaxKind.IdentifierToken => ParseMainTypeDeclaration(attributes, modifiers), 
			_ => throw ExceptionUtilities.UnexpectedValue(base.CurrentToken.Kind), 
		};
	}

	private TypeDeclarationSyntax ParseMainTypeDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, SyntaxListBuilder modifiers)
	{
		if (!tryScanRecordStart(out var keyword, out var recordModifier))
		{
			keyword = SyntaxParser.ConvertToKeyword(EatToken());
		}
		bool flag = keyword.Kind == SyntaxKind.ExtensionKeyword;
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfTypeSignature;
		TerminatorState termState2 = _termState;
		_termState |= TerminatorState.IsPossibleAggregateClauseStartOrStop;
		SyntaxToken name;
		if (flag)
		{
			name = null;
			if (base.CurrentToken.Kind == SyntaxKind.IdentifierToken)
			{
				keyword = AddTrailingSkippedSyntax(keyword, AddError(EatToken(), ErrorCode.ERR_ExtensionDisallowsName));
			}
		}
		else
		{
			name = ParseIdentifierToken();
		}
		TypeParameterListSyntax typeParameters = ParseTypeParameterList();
		ParameterListSyntax paramList = (((base.CurrentToken.Kind == SyntaxKind.OpenParenToken) | flag) ? ParseParenthesizedParameterList(flag) : null);
		BaseListSyntax baseList = (flag ? null : ParseBaseList());
		_termState = termState2;
		bool flag2 = true;
		SyntaxListBuilder<MemberDeclarationSyntax> syntaxListBuilder = default(SyntaxListBuilder<MemberDeclarationSyntax>);
		SyntaxListBuilder<TypeParameterConstraintClauseSyntax> syntaxListBuilder2 = default(SyntaxListBuilder<TypeParameterConstraintClauseSyntax>);
		try
		{
			if (base.CurrentToken.ContextualKind == SyntaxKind.WhereKeyword)
			{
				syntaxListBuilder2 = _pool.Allocate<TypeParameterConstraintClauseSyntax>();
				ParseTypeParameterConstraintClauses(syntaxListBuilder2);
			}
			_termState = termState;
			SyntaxToken semicolon;
			SyntaxToken openBrace;
			SyntaxToken closeBrace;
			if (base.CurrentToken.Kind == SyntaxKind.SemicolonToken)
			{
				semicolon = EatToken(SyntaxKind.SemicolonToken);
				openBrace = null;
				closeBrace = null;
			}
			else
			{
				openBrace = EatToken(SyntaxKind.OpenBraceToken);
				if (openBrace.IsMissing)
				{
					flag2 = false;
				}
				if (flag2)
				{
					syntaxListBuilder = _pool.Allocate<MemberDeclarationSyntax>();
					while (true)
					{
						SyntaxKind kind = base.CurrentToken.Kind;
						if (CanStartMember(kind))
						{
							TerminatorState termState3 = _termState;
							_termState |= TerminatorState.IsPossibleMemberStartOrStop;
							MemberDeclarationSyntax memberDeclarationSyntax = ParseMemberDeclaration(keyword.Kind);
							if (memberDeclarationSyntax != null)
							{
								syntaxListBuilder.Add(memberDeclarationSyntax);
							}
							else
							{
								SkipBadMemberListTokens(ref openBrace, syntaxListBuilder);
							}
							_termState = termState3;
						}
						else
						{
							bool flag3 = ((kind == SyntaxKind.CloseBraceToken || kind == SyntaxKind.EndOfFileToken) ? true : false);
							if (flag3 || IsTerminator())
							{
								break;
							}
							SkipBadMemberListTokens(ref openBrace, syntaxListBuilder);
						}
					}
				}
				closeBrace = (openBrace.IsMissing ? CreateMissingToken(SyntaxKind.CloseBraceToken, base.CurrentToken.Kind) : EatToken(SyntaxKind.CloseBraceToken));
				semicolon = TryEatToken(SyntaxKind.SemicolonToken);
			}
			return constructTypeDeclaration(_syntaxFactory, attributes, modifiers, keyword, recordModifier, name, typeParameters, paramList, baseList, syntaxListBuilder2, openBrace, syntaxListBuilder, closeBrace, semicolon);
		}
		finally
		{
			if (!syntaxListBuilder.IsNull)
			{
				_pool.Free(syntaxListBuilder);
			}
			if (!syntaxListBuilder2.IsNull)
			{
				_pool.Free(syntaxListBuilder2);
			}
		}
		static TypeDeclarationSyntax constructTypeDeclaration(ContextAwareSyntax syntaxFactory, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxListBuilder syntaxListBuilder3, SyntaxToken syntaxToken, SyntaxToken? syntaxToken2, SyntaxToken? identifier, TypeParameterListSyntax typeParameterList, ParameterListSyntax? parameterList, BaseListSyntax? baseList2, SyntaxListBuilder<TypeParameterConstraintClauseSyntax> constraints, SyntaxToken? openBraceToken, SyntaxListBuilder<MemberDeclarationSyntax> members, SyntaxToken? closeBraceToken, SyntaxToken semicolonToken)
		{
			Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers2 = syntaxListBuilder3.ToList();
			Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members2 = members;
			Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses = constraints;
			switch (syntaxToken.Kind)
			{
			case SyntaxKind.ClassKeyword:
				return syntaxFactory.ClassDeclaration(attributeLists, modifiers2, syntaxToken, identifier, typeParameterList, parameterList, baseList2, constraintClauses, openBraceToken, members2, closeBraceToken, semicolonToken);
			case SyntaxKind.StructKeyword:
				return syntaxFactory.StructDeclaration(attributeLists, modifiers2, syntaxToken, identifier, typeParameterList, parameterList, baseList2, constraintClauses, openBraceToken, members2, closeBraceToken, semicolonToken);
			case SyntaxKind.InterfaceKeyword:
				return syntaxFactory.InterfaceDeclaration(attributeLists, modifiers2, syntaxToken, identifier, typeParameterList, parameterList, baseList2, constraintClauses, openBraceToken, members2, closeBraceToken, semicolonToken);
			case SyntaxKind.RecordKeyword:
			{
				SyntaxKind kind2 = ((syntaxToken2 != null && syntaxToken2.Kind == SyntaxKind.StructKeyword) ? SyntaxKind.RecordStructDeclaration : SyntaxKind.RecordDeclaration);
				return syntaxFactory.RecordDeclaration(kind2, attributeLists, syntaxListBuilder3.ToList(), syntaxToken, syntaxToken2, identifier, typeParameterList, parameterList, baseList2, constraints, openBraceToken, members, closeBraceToken, semicolonToken);
			}
			case SyntaxKind.ExtensionKeyword:
				return syntaxFactory.ExtensionBlockDeclaration(attributeLists, syntaxListBuilder3.ToList(), syntaxToken, typeParameterList, parameterList, constraints, openBraceToken, members, closeBraceToken, semicolonToken);
			default:
				throw ExceptionUtilities.UnexpectedValue(syntaxToken.Kind);
			}
		}
		bool tryScanRecordStart([NotNullWhen(true)] out SyntaxToken? reference, out SyntaxToken? reference2)
		{
			SyntaxKind kind2;
			bool flag4;
			if (base.CurrentToken.ContextualKind == SyntaxKind.RecordKeyword)
			{
				reference = SyntaxParser.ConvertToKeyword(EatToken());
				kind2 = base.CurrentToken.Kind;
				flag4 = kind2 - 8374 <= SyntaxKind.List;
				reference2 = (flag4 ? EatToken() : null);
				return true;
			}
			kind2 = base.CurrentToken.Kind;
			flag4 = kind2 - 8374 <= SyntaxKind.List;
			if (flag4 && PeekToken(1).ContextualKind == SyntaxKind.RecordKeyword && PeekToken(2).Kind == SyntaxKind.IdentifierToken)
			{
				SyntaxToken syntaxToken = EatToken();
				reference = AddLeadingSkippedSyntax(AddError(SyntaxParser.ConvertToKeyword(EatToken()), ErrorCode.ERR_MisplacedRecord), syntaxToken);
				reference2 = SyntaxFactory.MissingToken(syntaxToken.Kind);
				return true;
			}
			reference = null;
			reference2 = null;
			return false;
		}
	}

	private void SkipBadMemberListTokens(ref SyntaxToken openBrace, SyntaxListBuilder members)
	{
		if (members.Count > 0)
		{
			GreenNode previousNode = members[members.Count - 1];
			SkipBadMemberListTokens(ref previousNode);
			members[members.Count - 1] = previousNode;
		}
		else
		{
			GreenNode previousNode2 = openBrace;
			SkipBadMemberListTokens(ref previousNode2);
			openBrace = (SyntaxToken)previousNode2;
		}
	}

	private void SkipBadMemberListTokens(ref GreenNode previousNode)
	{
		int num = 0;
		SyntaxListBuilder syntaxListBuilder = _pool.Allocate();
		bool flag = false;
		SyntaxToken syntaxToken = EatToken();
		syntaxToken = AddError(syntaxToken, ErrorCode.ERR_InvalidMemberDecl, syntaxToken.Text);
		syntaxListBuilder.Add(syntaxToken);
		while (!flag)
		{
			SyntaxKind kind = base.CurrentToken.Kind;
			bool flag2 = CanStartMember(kind);
			if (flag2)
			{
				bool flag3 = kind == SyntaxKind.DelegateKeyword;
				if (flag3)
				{
					SyntaxKind kind2 = PeekToken(1).Kind;
					bool flag4 = ((kind2 == SyntaxKind.OpenParenToken || kind2 == SyntaxKind.OpenBraceToken) ? true : false);
					flag3 = flag4;
				}
				flag2 = !flag3;
			}
			if (flag2)
			{
				flag = true;
				continue;
			}
			switch (kind)
			{
			case SyntaxKind.OpenBraceToken:
				num++;
				break;
			case SyntaxKind.CloseBraceToken:
				if (num-- == 0)
				{
					flag = true;
					continue;
				}
				break;
			case SyntaxKind.EndOfFileToken:
				flag = true;
				continue;
			}
			syntaxListBuilder.Add(EatToken());
		}
		previousNode = AddTrailingSkippedSyntax((CSharpSyntaxNode)previousNode, _pool.ToTokenListAndFree(syntaxListBuilder).Node);
	}

	private bool IsPossibleMemberStartOrStop()
	{
		if (!IsPossibleMemberStart())
		{
			return base.CurrentToken.Kind == SyntaxKind.CloseBraceToken;
		}
		return true;
	}

	private bool IsPossibleAggregateClauseStartOrStop()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if ((kind != SyntaxKind.OpenBraceToken && kind != SyntaxKind.ColonToken) || 1 == 0)
		{
			return IsCurrentTokenWhereOfConstraintClause();
		}
		return true;
	}

	private BaseListSyntax ParseBaseList()
	{
		SyntaxToken colon = TryEatToken(SyntaxKind.ColonToken);
		if (colon == null)
		{
			return null;
		}
		SeparatedSyntaxListBuilder<BaseTypeSyntax> item = _pool.AllocateSeparated<BaseTypeSyntax>();
		TypeSyntax type = ParseType();
		item.Add((base.CurrentToken.Kind == SyntaxKind.OpenParenToken) ? ((BaseTypeSyntax)_syntaxFactory.PrimaryConstructorBaseType(type, ParseParenthesizedArgumentList())) : ((BaseTypeSyntax)_syntaxFactory.SimpleBaseType(type)));
		while (true)
		{
			SyntaxKind kind = base.CurrentToken.Kind;
			bool flag = ((kind == SyntaxKind.OpenBraceToken || kind == SyntaxKind.SemicolonToken) ? true : false);
			if (flag || IsCurrentTokenWhereOfConstraintClause())
			{
				break;
			}
			if (base.CurrentToken.Kind == SyntaxKind.CommaToken)
			{
				item.AddSeparator(EatToken(SyntaxKind.CommaToken));
				item.Add(_syntaxFactory.SimpleBaseType(ParseType()));
				continue;
			}
			if (GetModifierExcludingScoped(base.CurrentToken) != DeclarationModifiers.None)
			{
				break;
			}
			if (IsPossibleType())
			{
				item.AddSeparator(EatToken(SyntaxKind.CommaToken));
				item.Add(_syntaxFactory.SimpleBaseType(ParseType()));
			}
			else if (skipBadBaseListTokens(ref colon, item, SyntaxKind.CommaToken) == PostSkipAction.Abort)
			{
				break;
			}
		}
		return _syntaxFactory.BaseList(colon, _pool.ToListAndFree(in item));
		PostSkipAction skipBadBaseListTokens(ref SyntaxToken startToken, SeparatedSyntaxListBuilder<BaseTypeSyntax> list, SyntaxKind expected)
		{
			return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleAttribute(), (LanguageParser p, SyntaxKind _) => p.CurrentToken.Kind == SyntaxKind.OpenBraceToken || p.IsCurrentTokenWhereOfConstraintClause(), expected);
		}
	}

	private bool IsCurrentTokenWhereOfConstraintClause()
	{
		if (base.CurrentToken.ContextualKind == SyntaxKind.WhereKeyword && PeekToken(1).Kind == SyntaxKind.IdentifierToken)
		{
			return PeekToken(2).Kind == SyntaxKind.ColonToken;
		}
		return false;
	}

	private void ParseTypeParameterConstraintClauses(SyntaxListBuilder list)
	{
		while (base.CurrentToken.ContextualKind == SyntaxKind.WhereKeyword)
		{
			list.Add(ParseTypeParameterConstraintClause());
		}
	}

	private TypeParameterConstraintClauseSyntax ParseTypeParameterConstraintClause()
	{
		SyntaxToken whereKeyword = EatContextualToken(SyntaxKind.WhereKeyword);
		IdentifierNameSyntax name = ((!IsTrueIdentifier()) ? AddError(CreateMissingIdentifierName(), ErrorCode.ERR_IdentifierExpected) : ParseIdentifierName());
		SyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
		SeparatedSyntaxListBuilder<TypeParameterConstraintSyntax> builder = _pool.AllocateSeparated<TypeParameterConstraintSyntax>();
		if (base.CurrentToken.Kind == SyntaxKind.OpenBraceToken || IsCurrentTokenWhereOfConstraintClause())
		{
			builder.Add(_syntaxFactory.TypeConstraint(AddError(CreateMissingIdentifierName(), ErrorCode.ERR_TypeExpected)));
		}
		else
		{
			TypeParameterConstraintSyntax typeParameterConstraintSyntax = ParseTypeParameterConstraint();
			builder.Add(typeParameterConstraintSyntax);
			while (base.CurrentToken.Kind != SyntaxKind.OpenBraceToken && ((_termState & TerminatorState.IsEndOfTypeSignature) == 0 || base.CurrentToken.Kind != SyntaxKind.SemicolonToken) && base.CurrentToken.Kind != SyntaxKind.EqualsGreaterThanToken && base.CurrentToken.ContextualKind != SyntaxKind.WhereKeyword)
			{
				bool flag;
				if (flag = base.CurrentToken.Kind == SyntaxKind.CommaToken || IsPossibleTypeParameterConstraint())
				{
					SyntaxToken syntaxToken = EatToken(SyntaxKind.CommaToken);
					if (((typeParameterConstraintSyntax.Kind == SyntaxKind.AllowsConstraintClause) & flag) && !IsPossibleTypeParameterConstraint())
					{
						AddTrailingSkippedSyntax((SyntaxListBuilder?)builder, (GreenNode)AddError(syntaxToken, ErrorCode.ERR_UnexpectedToken, SyntaxFacts.GetText(SyntaxKind.CommaToken)));
						break;
					}
					builder.AddSeparator(syntaxToken);
					if (IsCurrentTokenWhereOfConstraintClause())
					{
						builder.Add(_syntaxFactory.TypeConstraint(AddError(CreateMissingIdentifierName(), ErrorCode.ERR_TypeExpected)));
						break;
					}
					typeParameterConstraintSyntax = ParseTypeParameterConstraint();
					builder.Add(typeParameterConstraintSyntax);
				}
				else if (skipBadTypeParameterConstraintTokens(builder, SyntaxKind.CommaToken) == PostSkipAction.Abort)
				{
					break;
				}
			}
		}
		return _syntaxFactory.TypeParameterConstraintClause(whereKeyword, name, colonToken, _pool.ToListAndFree(in builder));
		PostSkipAction skipBadTypeParameterConstraintTokens(SeparatedSyntaxListBuilder<TypeParameterConstraintSyntax> list, SyntaxKind expected)
		{
			CSharpSyntaxNode startToken = null;
			return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleTypeParameterConstraint(), (LanguageParser p, SyntaxKind _) => p.CurrentToken.Kind == SyntaxKind.OpenBraceToken || p.IsCurrentTokenWhereOfConstraintClause(), expected);
		}
	}

	private bool IsPossibleTypeParameterConstraint()
	{
		switch (base.CurrentToken.Kind)
		{
		case SyntaxKind.DefaultKeyword:
		case SyntaxKind.NewKeyword:
		case SyntaxKind.ClassKeyword:
		case SyntaxKind.StructKeyword:
			return true;
		case SyntaxKind.IdentifierToken:
			if (base.CurrentToken.ContextualKind != SyntaxKind.AllowsKeyword || PeekToken(1).Kind != SyntaxKind.RefKeyword)
			{
				return IsTrueIdentifier();
			}
			return true;
		default:
			return IsPredefinedType(base.CurrentToken.Kind);
		}
	}

	private TypeParameterConstraintSyntax ParseTypeParameterConstraint()
	{
		return base.CurrentToken.Kind switch
		{
			SyntaxKind.NewKeyword => _syntaxFactory.ConstructorConstraint(EatToken(), EatToken(SyntaxKind.OpenParenToken), EatToken(SyntaxKind.CloseParenToken)), 
			SyntaxKind.StructKeyword => _syntaxFactory.ClassOrStructConstraint(SyntaxKind.StructConstraint, EatToken(), (base.CurrentToken.Kind == SyntaxKind.QuestionToken) ? AddError(EatToken(), ErrorCode.ERR_UnexpectedToken, SyntaxFacts.GetText(SyntaxKind.QuestionToken)) : null), 
			SyntaxKind.ClassKeyword => _syntaxFactory.ClassOrStructConstraint(SyntaxKind.ClassConstraint, EatToken(), TryEatToken(SyntaxKind.QuestionToken)), 
			SyntaxKind.DefaultKeyword => _syntaxFactory.DefaultConstraint(EatToken()), 
			SyntaxKind.EnumKeyword => _syntaxFactory.TypeConstraint(AddTrailingSkippedSyntax(AddError(CreateMissingIdentifierName(), ErrorCode.ERR_NoEnumConstraint), EatToken())), 
			SyntaxKind.DelegateKeyword => (PeekToken(1).Kind == SyntaxKind.AsteriskToken) ? _syntaxFactory.TypeConstraint(ParseType()) : _syntaxFactory.TypeConstraint(AddTrailingSkippedSyntax(AddError(CreateMissingIdentifierName(), ErrorCode.ERR_NoDelegateConstraint), EatToken())), 
			_ => parseTypeOrAllowsConstraint(), 
		};
		TypeParameterConstraintSyntax parseTypeOrAllowsConstraint()
		{
			if (base.CurrentToken.ContextualKind == SyntaxKind.AllowsKeyword && PeekToken(1).Kind == SyntaxKind.RefKeyword)
			{
				SyntaxToken allowsKeyword = EatContextualToken(SyntaxKind.AllowsKeyword);
				SeparatedSyntaxListBuilder<AllowsConstraintSyntax> item = _pool.AllocateSeparated<AllowsConstraintSyntax>();
				while (true)
				{
					item.Add(_syntaxFactory.RefStructConstraint(EatToken(SyntaxKind.RefKeyword), EatToken(SyntaxKind.StructKeyword)));
					if (base.CurrentToken.Kind != SyntaxKind.CommaToken || PeekToken(1).Kind != SyntaxKind.RefKeyword)
					{
						break;
					}
					item.AddSeparator(EatToken(SyntaxKind.CommaToken));
				}
				return _syntaxFactory.AllowsConstraintClause(allowsKeyword, _pool.ToListAndFree(in item));
			}
			return _syntaxFactory.TypeConstraint(ParseType());
		}
	}

	private bool IsPossibleMemberStart()
	{
		return CanStartMember(base.CurrentToken.Kind);
	}

	private static bool CanStartMember(SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.TildeToken:
		case SyntaxKind.OpenParenToken:
		case SyntaxKind.OpenBracketToken:
		case SyntaxKind.BoolKeyword:
		case SyntaxKind.ByteKeyword:
		case SyntaxKind.SByteKeyword:
		case SyntaxKind.ShortKeyword:
		case SyntaxKind.UShortKeyword:
		case SyntaxKind.IntKeyword:
		case SyntaxKind.UIntKeyword:
		case SyntaxKind.LongKeyword:
		case SyntaxKind.ULongKeyword:
		case SyntaxKind.DoubleKeyword:
		case SyntaxKind.FloatKeyword:
		case SyntaxKind.DecimalKeyword:
		case SyntaxKind.StringKeyword:
		case SyntaxKind.CharKeyword:
		case SyntaxKind.VoidKeyword:
		case SyntaxKind.ObjectKeyword:
		case SyntaxKind.PublicKeyword:
		case SyntaxKind.PrivateKeyword:
		case SyntaxKind.InternalKeyword:
		case SyntaxKind.ProtectedKeyword:
		case SyntaxKind.StaticKeyword:
		case SyntaxKind.ReadOnlyKeyword:
		case SyntaxKind.SealedKeyword:
		case SyntaxKind.ConstKeyword:
		case SyntaxKind.FixedKeyword:
		case SyntaxKind.VolatileKeyword:
		case SyntaxKind.NewKeyword:
		case SyntaxKind.OverrideKeyword:
		case SyntaxKind.AbstractKeyword:
		case SyntaxKind.VirtualKeyword:
		case SyntaxKind.EventKeyword:
		case SyntaxKind.ExternKeyword:
		case SyntaxKind.RefKeyword:
		case SyntaxKind.ClassKeyword:
		case SyntaxKind.StructKeyword:
		case SyntaxKind.InterfaceKeyword:
		case SyntaxKind.EnumKeyword:
		case SyntaxKind.DelegateKeyword:
		case SyntaxKind.UnsafeKeyword:
		case SyntaxKind.ExplicitKeyword:
		case SyntaxKind.ImplicitKeyword:
		case SyntaxKind.IdentifierToken:
			return true;
		default:
			return false;
		}
	}

	private bool IsTypeDeclarationStart()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind - 8374 > (SyntaxKind)3)
		{
			if (kind != SyntaxKind.DelegateKeyword)
			{
				if (kind == SyntaxKind.IdentifierToken)
				{
					if (base.CurrentToken.ContextualKind == SyntaxKind.RecordKeyword)
					{
						return IsFeatureEnabled(MessageID.IDS_FeatureRecords);
					}
					if (IsExtensionContainerStart())
					{
						return true;
					}
					return false;
				}
			}
			else if (!IsFunctionPointerStart())
			{
				goto IL_0030;
			}
			return false;
		}
		goto IL_0030;
		IL_0030:
		return true;
	}

	private bool CanReuseMemberDeclaration(SyntaxKind kind, bool isGlobal)
	{
		switch (kind)
		{
		case SyntaxKind.NamespaceDeclaration:
		case SyntaxKind.FileScopedNamespaceDeclaration:
		case SyntaxKind.ClassDeclaration:
		case SyntaxKind.StructDeclaration:
		case SyntaxKind.InterfaceDeclaration:
		case SyntaxKind.EnumDeclaration:
		case SyntaxKind.DelegateDeclaration:
		case SyntaxKind.EventFieldDeclaration:
		case SyntaxKind.OperatorDeclaration:
		case SyntaxKind.ConversionOperatorDeclaration:
		case SyntaxKind.ConstructorDeclaration:
		case SyntaxKind.DestructorDeclaration:
		case SyntaxKind.PropertyDeclaration:
		case SyntaxKind.EventDeclaration:
		case SyntaxKind.IndexerDeclaration:
		case SyntaxKind.RecordDeclaration:
		case SyntaxKind.RecordStructDeclaration:
			return true;
		case SyntaxKind.FieldDeclaration:
		case SyntaxKind.MethodDeclaration:
			if (!isGlobal || base.IsScript)
			{
				return true;
			}
			return base.CurrentNode.Parent is Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax;
		case SyntaxKind.GlobalStatement:
			return isGlobal;
		default:
			return false;
		}
	}

	public MemberDeclarationSyntax ParseMemberDeclaration()
	{
		return ParseWithStackGuard((LanguageParser @this) => @this.ParseMemberDeclaration(SyntaxKind.StructDeclaration), createEmptyNodeFunc);
		static MemberDeclarationSyntax createEmptyNodeFunc(LanguageParser @this)
		{
			return @this._syntaxFactory.IncompleteMember(default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>), default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>), @this.CreateMissingIdentifierName());
		}
	}

	internal MemberDeclarationSyntax ParseMemberDeclarationOrStatement(SyntaxKind parentKind)
	{
		_recursionDepth++;
		StackGuard.EnsureSufficientExecutionStack(_recursionDepth);
		MemberDeclarationSyntax result = ParseMemberDeclarationOrStatementCore(parentKind);
		_recursionDepth--;
		return result;
	}

	private MemberDeclarationSyntax ParseMemberDeclarationOrStatementCore(SyntaxKind parentKind)
	{
		CancellationToken cancellationToken = base.cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		if (IsIncrementalAndFactoryContextMatches && CanReuseMemberDeclaration(base.CurrentNodeKind, isGlobal: true))
		{
			return (MemberDeclarationSyntax)EatNode();
		}
		TerminatorState termState = _termState;
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> syntaxList = ParseStatementAttributeDeclarations();
		bool flag = syntaxList.Count > 0;
		ResetPoint startPoint = GetResetPoint();
		SyntaxListBuilder modifiers = _pool.Allocate();
		try
		{
			if (!flag || !base.IsScript)
			{
				ParserSyntaxContextResetter parserSyntaxContextResetter = new ParserSyntaxContextResetter(this, IsInAsync || !base.IsScript);
				try
				{
					switch (base.CurrentToken.Kind)
					{
					case SyntaxKind.UnsafeKeyword:
						if (PeekToken(1).Kind == SyntaxKind.OpenBraceToken)
						{
							return _syntaxFactory.GlobalStatement(ParseUnsafeStatement(syntaxList));
						}
						break;
					case SyntaxKind.FixedKeyword:
						if (PeekToken(1).Kind == SyntaxKind.OpenParenToken)
						{
							return _syntaxFactory.GlobalStatement(ParseFixedStatement(syntaxList));
						}
						break;
					case SyntaxKind.DelegateKeyword:
						if (IsAnonymousDelegateExpression())
						{
							return _syntaxFactory.GlobalStatement(ParseExpressionStatement(syntaxList));
						}
						break;
					case SyntaxKind.NewKeyword:
						if (IsPossibleNewExpression())
						{
							return _syntaxFactory.GlobalStatement(ParseExpressionStatement(syntaxList));
						}
						break;
					}
				}
				finally
				{
					parserSyntaxContextResetter.Dispose();
				}
			}
			ParseModifiers(modifiers, forAccessors: false, forTopLevelStatements: true, out var isPossibleTypeDeclaration);
			bool flag2 = modifiers.Count > 0;
			MemberDeclarationSyntax result;
			if (base.CurrentToken.Kind == SyntaxKind.IdentifierToken && PeekToken(1).Kind == SyntaxKind.OpenParenToken && (flag | flag2))
			{
				PredefinedTypeSyntax type = _syntaxFactory.PredefinedType(AddError(SyntaxFactory.MissingToken(SyntaxKind.VoidKeyword), ErrorCode.ERR_MemberNeedsType));
				if (base.IsScript)
				{
					SyntaxToken identifier = EatToken();
					return ParseMethodDeclaration(syntaxList, modifiers, type, null, identifier, null);
				}
				if (tryParseLocalDeclarationStatementFromStartPoint<LocalFunctionStatementSyntax>(syntaxList, ref startPoint, out result))
				{
					return result;
				}
			}
			if (base.CurrentToken.Kind == SyntaxKind.ConstKeyword)
			{
				if (!base.IsScript && tryParseLocalDeclarationStatementFromStartPoint<LocalDeclarationStatementSyntax>(syntaxList, ref startPoint, out result))
				{
					return result;
				}
				return ParseConstantFieldDeclaration(syntaxList, modifiers, parentKind);
			}
			if (base.CurrentToken.Kind == SyntaxKind.EventKeyword)
			{
				return ParseEventDeclaration(syntaxList, modifiers, parentKind);
			}
			if (base.CurrentToken.Kind == SyntaxKind.FixedKeyword)
			{
				return ParseFixedSizeBufferDeclaration(syntaxList, modifiers, parentKind);
			}
			result = TryParseConversionOperatorDeclaration(syntaxList, modifiers);
			if (result != null)
			{
				return result;
			}
			if (base.CurrentToken.Kind == SyntaxKind.NamespaceKeyword)
			{
				return ParseNamespaceDeclaration(syntaxList, modifiers);
			}
			if (isPossibleTypeDeclaration && IsTypeDeclarationStart())
			{
				return ParseTypeDeclaration(syntaxList, modifiers);
			}
			TypeSyntax type2 = ParseReturnType();
			ResetPoint state = GetResetPoint();
			try
			{
				if ((!flag || !base.IsScript) && !flag2 && (type2.Kind == SyntaxKind.RefType || !IsOperatorStart(out var _, advanceParser: false)))
				{
					Reset(ref startPoint);
					SyntaxKind kind = base.CurrentToken.Kind;
					if (kind != SyntaxKind.CloseBraceToken && kind != SyntaxKind.EndOfFileToken && IsPossibleStatement())
					{
						TerminatorState termState2 = _termState;
						_termState |= TerminatorState.IsPossibleStatementStartOrStop;
						ParserSyntaxContextResetter parserSyntaxContextResetter2 = new ParserSyntaxContextResetter(this, IsInAsync || !base.IsScript);
						try
						{
							StatementSyntax statement = ParseStatementCore(syntaxList, isGlobal: true);
							_termState = termState2;
							if (isAcceptableNonDeclarationStatement(statement, base.IsScript))
							{
								return _syntaxFactory.GlobalStatement(statement);
							}
						}
						finally
						{
							parserSyntaxContextResetter2.Dispose();
						}
					}
					Reset(ref state);
				}
				if (IsMisplacedModifier(modifiers, syntaxList, type2, out result))
				{
					return result;
				}
				ExplicitInterfaceSpecifierSyntax explicitInterfaceOpt2;
				SyntaxToken identifierOrThisOpt;
				TypeParameterListSyntax typeParameterListOpt;
				do
				{
					bool isRef = type2.IsRef;
					if (!isRef && IsOperatorStart(out explicitInterfaceOpt2))
					{
						return ParseOperatorDeclaration(syntaxList, modifiers, type2, explicitInterfaceOpt2);
					}
					if ((!isRef || !base.IsScript) && IsFieldDeclaration(isEvent: false, isGlobalScriptLevel: true))
					{
						TerminatorState termState3 = _termState;
						if ((!flag && !flag2) || !base.IsScript)
						{
							_termState |= TerminatorState.IsPossibleStatementStartOrStop;
							if (!base.IsScript)
							{
								Reset(ref startPoint);
								if (tryParseLocalDeclarationStatement<LocalDeclarationStatementSyntax>(syntaxList, out result))
								{
									return result;
								}
								Reset(ref state);
							}
						}
						if (!isRef)
						{
							return ParseNormalFieldDeclaration(syntaxList, modifiers, type2, parentKind);
						}
						_termState = termState3;
					}
					ParseMemberName(out explicitInterfaceOpt2, out identifierOrThisOpt, out typeParameterListOpt, isEvent: false);
					if (!flag2 && !flag && !base.IsScript && explicitInterfaceOpt2 == null && identifierOrThisOpt == null && typeParameterListOpt == null && !type2.IsMissing && type2.Kind != SyntaxKind.RefType && !isFollowedByPossibleUsingDirective() && tryParseLocalDeclarationStatementFromStartPoint<LocalDeclarationStatementSyntax>(syntaxList, ref startPoint, out result))
					{
						return result;
					}
					if (IsNoneOrIncompleteMember(parentKind, syntaxList, modifiers, type2, explicitInterfaceOpt2, identifierOrThisOpt, typeParameterListOpt, out result))
					{
						return result;
					}
				}
				while (ReconsideredTypeAsAsyncModifier(ref modifiers, ref type2, ref state, ref explicitInterfaceOpt2, ref identifierOrThisOpt, ref typeParameterListOpt));
				if (TryParseIndexerOrPropertyDeclaration(syntaxList, modifiers, type2, explicitInterfaceOpt2, identifierOrThisOpt, typeParameterListOpt, out result))
				{
					return result;
				}
				if (!base.IsScript)
				{
					if (explicitInterfaceOpt2 == null && tryParseLocalDeclarationStatementFromStartPoint<LocalFunctionStatementSyntax>(syntaxList, ref startPoint, out result))
					{
						return result;
					}
					if (!flag2 && tryParseStatement(syntaxList, ref startPoint, out result))
					{
						return result;
					}
				}
				return ParseMethodDeclaration(syntaxList, modifiers, type2, explicitInterfaceOpt2, identifierOrThisOpt, typeParameterListOpt);
			}
			finally
			{
				Release(ref state);
			}
		}
		finally
		{
			_pool.Free(modifiers);
			_termState = termState;
			Release(ref startPoint);
		}
		static bool isAcceptableNonDeclarationStatement(StatementSyntax statementSyntax, bool isScript)
		{
			SyntaxKind? syntaxKind = statementSyntax?.Kind;
			if (syntaxKind.HasValue)
			{
				SyntaxKind valueOrDefault = syntaxKind.GetValueOrDefault();
				if (valueOrDefault == SyntaxKind.LocalDeclarationStatement)
				{
					if (!isScript)
					{
						if (statementSyntax is LocalDeclarationStatementSyntax localDeclarationStatementSyntax)
						{
							return localDeclarationStatementSyntax.UsingKeyword != null;
						}
						return false;
					}
					return false;
				}
				if (valueOrDefault != SyntaxKind.ExpressionStatement)
				{
					if (valueOrDefault == SyntaxKind.LocalFunctionStatement)
					{
						goto IL_0081;
					}
				}
				else if (!isScript && statementSyntax is ExpressionStatementSyntax expressionStatementSyntax)
				{
					ExpressionSyntax expression = expressionStatementSyntax.Expression;
					if (expression != null && expression.Kind == SyntaxKind.IdentifierName)
					{
						SyntaxToken semicolonToken = expressionStatementSyntax.SemicolonToken;
						if (semicolonToken != null && semicolonToken.IsMissing)
						{
							goto IL_0081;
						}
					}
				}
				return true;
			}
			goto IL_0081;
			IL_0081:
			return false;
		}
		bool isFollowedByPossibleUsingDirective()
		{
			if (base.CurrentToken.Kind == SyntaxKind.UsingKeyword)
			{
				return !IsPossibleTopLevelUsingLocalDeclarationStatement();
			}
			if (base.CurrentToken.ContextualKind == SyntaxKind.GlobalKeyword && PeekToken(1).Kind == SyntaxKind.UsingKeyword)
			{
				using (GetDisposableResetPoint(resetOnDispose: true))
				{
					EatToken();
					return !IsPossibleTopLevelUsingLocalDeclarationStatement();
				}
			}
			return false;
		}
		bool tryParseLocalDeclarationStatement<DeclarationSyntax>(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, out MemberDeclarationSyntax reference) where DeclarationSyntax : StatementSyntax
		{
			ParserSyntaxContextResetter parserSyntaxContextResetter3 = new ParserSyntaxContextResetter(this, true);
			try
			{
				int lastTokenPosition = -1;
				IsMakingProgress(ref lastTokenPosition);
				if (ParseLocalDeclarationStatement(attributes) is DeclarationSyntax statement2 && IsMakingProgress(ref lastTokenPosition, assertIfFalse: false))
				{
					reference = _syntaxFactory.GlobalStatement(statement2);
					return true;
				}
				reference = null;
				return false;
			}
			finally
			{
				parserSyntaxContextResetter3.Dispose();
			}
		}
		bool tryParseLocalDeclarationStatementFromStartPoint<DeclarationSyntax>(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, ref ResetPoint state2, out MemberDeclarationSyntax result2) where DeclarationSyntax : StatementSyntax
		{
			using DisposableResetPoint disposableResetPoint = GetDisposableResetPoint(resetOnDispose: false);
			Reset(ref state2);
			if (tryParseLocalDeclarationStatement<DeclarationSyntax>(attributes, out result2))
			{
				return true;
			}
			disposableResetPoint.Reset();
			return false;
		}
		bool tryParseStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, ref ResetPoint afterAttributesPoint, out MemberDeclarationSyntax reference)
		{
			using DisposableResetPoint disposableResetPoint = GetDisposableResetPoint(resetOnDispose: false);
			Reset(ref afterAttributesPoint);
			if (IsPossibleStatement())
			{
				TerminatorState termState4 = _termState;
				_termState |= TerminatorState.IsPossibleStatementStartOrStop;
				ParserSyntaxContextResetter parserSyntaxContextResetter3 = new ParserSyntaxContextResetter(this, true);
				try
				{
					StatementSyntax statementSyntax = ParseStatementCore(attributes, isGlobal: true);
					_termState = termState4;
					if (statementSyntax != null)
					{
						reference = _syntaxFactory.GlobalStatement(statementSyntax);
						return true;
					}
				}
				finally
				{
					parserSyntaxContextResetter3.Dispose();
				}
			}
			disposableResetPoint.Reset();
			reference = null;
			return false;
		}
	}

	private bool IsMisplacedModifier(SyntaxListBuilder modifiers, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, TypeSyntax type, out MemberDeclarationSyntax result)
	{
		bool flag = GetModifierExcludingScoped(base.CurrentToken) != DeclarationModifiers.None;
		if (flag)
		{
			bool flag2;
			switch (base.CurrentToken.ContextualKind)
			{
			case SyntaxKind.PartialKeyword:
			case SyntaxKind.AsyncKeyword:
			case SyntaxKind.RequiredKeyword:
			case SyntaxKind.FileKeyword:
				flag2 = true;
				break;
			default:
				flag2 = false;
				break;
			}
			flag = !flag2;
		}
		if (flag && IsComplete(type))
		{
			SyntaxToken currentToken = base.CurrentToken;
			type = AddError(type, type.Width + type.GetTrailingTriviaWidth() + currentToken.GetLeadingTriviaWidth(), currentToken.Width, ErrorCode.ERR_BadModifierLocation, currentToken.Text);
			result = _syntaxFactory.IncompleteMember(attributes, modifiers.ToList(), type);
			return true;
		}
		result = null;
		return false;
	}

	private bool IsNoneOrIncompleteMember(SyntaxKind parentKind, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceOpt, SyntaxToken identifierOrThisOpt, TypeParameterListSyntax typeParameterListOpt, out MemberDeclarationSyntax result)
	{
		if (explicitInterfaceOpt == null && identifierOrThisOpt == null && typeParameterListOpt == null)
		{
			if (looksLikeStartOfPropertyBody())
			{
				result = null;
				return false;
			}
			if (attributes.Count == 0 && modifiers.Count == 0 && type.IsMissing && type.Kind != SyntaxKind.RefType)
			{
				result = null;
				return true;
			}
			IncompleteMemberSyntax incompleteMemberSyntax = _syntaxFactory.IncompleteMember(attributes, modifiers.ToList(), type.IsMissing ? null : type);
			if (ContainsErrorDiagnostic(incompleteMemberSyntax))
			{
				result = incompleteMemberSyntax;
			}
			else
			{
				bool flag = ((parentKind == SyntaxKind.NamespaceDeclaration || parentKind == SyntaxKind.FileScopedNamespaceDeclaration) ? true : false);
				if (flag || (parentKind == SyntaxKind.CompilationUnit && !base.IsScript))
				{
					result = AddErrorToLastToken(incompleteMemberSyntax, ErrorCode.ERR_NamespaceUnexpected);
				}
				else
				{
					result = AddError(incompleteMemberSyntax, incompleteMemberSyntax.Width + incompleteMemberSyntax.GetTrailingTriviaWidth() + base.CurrentToken.GetLeadingTriviaWidth(), base.CurrentToken.Width, ErrorCode.ERR_InvalidMemberDecl, base.CurrentToken.Text);
				}
			}
			return true;
		}
		result = null;
		return false;
		bool looksLikePropertyType()
		{
			TypeSyntax typeSyntax = type;
			if (typeSyntax.IsMissing || ContainsErrorDiagnostic(typeSyntax))
			{
				return false;
			}
			if (typeSyntax is RefTypeSyntax refTypeSyntax)
			{
				typeSyntax = refTypeSyntax.Type;
			}
			if (typeSyntax is IdentifierNameSyntax identifierNameSyntax)
			{
				SyntaxToken identifier = identifierNameSyntax.Identifier;
				if (identifier != null)
				{
					SyntaxKind contextualKind = identifier.ContextualKind;
					if (SyntaxFacts.IsKeywordKind(contextualKind))
					{
						return false;
					}
				}
			}
			return true;
		}
		bool looksLikeStartOfPropertyBody()
		{
			if (modifiers.Count == 0 && !looksLikePropertyType())
			{
				return false;
			}
			if (!IsStartOfPropertyBody(0))
			{
				return false;
			}
			return true;
		}
	}

	private bool ReconsideredTypeAsAsyncModifier(ref SyntaxListBuilder modifiers, ref TypeSyntax type, ref ResetPoint afterTypeResetPoint, ref ExplicitInterfaceSpecifierSyntax explicitInterfaceOpt, ref SyntaxToken identifierOrThisOpt, ref TypeParameterListSyntax typeParameterListOpt)
	{
		if (type.Kind != SyntaxKind.RefType && identifierOrThisOpt != null)
		{
			if (typeParameterListOpt == null || !typeParameterListOpt.ContainsDiagnostics)
			{
				SyntaxKind kind = base.CurrentToken.Kind;
				if (kind == SyntaxKind.OpenParenToken || kind == SyntaxKind.OpenBraceToken || kind == SyntaxKind.EqualsGreaterThanToken)
				{
					goto IL_0083;
				}
			}
			if (ReconsiderTypeAsAsyncModifier(ref modifiers, type, identifierOrThisOpt))
			{
				Reset(ref afterTypeResetPoint);
				explicitInterfaceOpt = null;
				identifierOrThisOpt = null;
				typeParameterListOpt = null;
				Release(ref afterTypeResetPoint);
				type = ParseReturnType();
				afterTypeResetPoint = GetResetPoint();
				return true;
			}
		}
		goto IL_0083;
		IL_0083:
		return false;
	}

	private bool TryParseIndexerOrPropertyDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceOpt, SyntaxToken identifierOrThisOpt, TypeParameterListSyntax typeParameterListOpt, out MemberDeclarationSyntax result)
	{
		if (identifierOrThisOpt != null && identifierOrThisOpt.Kind == SyntaxKind.ThisKeyword)
		{
			result = ParseIndexerDeclaration(attributes, modifiers, type, explicitInterfaceOpt, identifierOrThisOpt, typeParameterListOpt);
			return true;
		}
		if (IsStartOfPropertyBody(0))
		{
			if (identifierOrThisOpt == null)
			{
				identifierOrThisOpt = AddError(CreateMissingIdentifierToken(), ErrorCode.ERR_IdentifierExpected);
			}
			result = ParsePropertyDeclaration(attributes, modifiers, type, explicitInterfaceOpt, identifierOrThisOpt, typeParameterListOpt);
			return true;
		}
		result = null;
		return false;
	}

	private bool IsStartOfPropertyBody(int index)
	{
		SyntaxKind kind = PeekToken(index).Kind;
		if ((kind == SyntaxKind.OpenBraceToken || kind == SyntaxKind.EqualsGreaterThanToken) ? true : false)
		{
			return true;
		}
		if (kind == SyntaxKind.SemicolonToken)
		{
			kind = PeekToken(index + 1).Kind;
			if ((kind == SyntaxKind.OpenBraceToken || kind == SyntaxKind.EqualsGreaterThanToken) ? true : false)
			{
				return true;
			}
		}
		return false;
	}

	internal MemberDeclarationSyntax ParseMemberDeclaration(SyntaxKind parentKind)
	{
		_recursionDepth++;
		StackGuard.EnsureSufficientExecutionStack(_recursionDepth);
		MemberDeclarationSyntax result = ParseMemberDeclarationCore(parentKind);
		_recursionDepth--;
		return result;
	}

	private MemberDeclarationSyntax ParseMemberDeclarationCore(SyntaxKind parentKind)
	{
		CancellationToken cancellationToken = base.cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		if (IsIncrementalAndFactoryContextMatches && CanReuseMemberDeclaration(base.CurrentNodeKind, isGlobal: false))
		{
			return (MemberDeclarationSyntax)EatNode();
		}
		SyntaxListBuilder modifiers = _pool.Allocate();
		TerminatorState termState = _termState;
		try
		{
			Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes = ParseAttributeDeclarations(inExpressionContext: false);
			ParseModifiers(modifiers, forAccessors: false, forTopLevelStatements: false, out var isPossibleTypeDeclaration);
			if (IsExtensionContainerStart())
			{
				return ParseMainTypeDeclaration(attributes, modifiers);
			}
			if (base.CurrentToken.Kind == SyntaxKind.IdentifierToken && PeekToken(1).Kind == SyntaxKind.OpenParenToken)
			{
				return ParseConstructorDeclaration(attributes, modifiers);
			}
			if (base.CurrentToken.Kind == SyntaxKind.TildeToken)
			{
				return ParseDestructorDeclaration(attributes, modifiers);
			}
			if (base.CurrentToken.Kind == SyntaxKind.ConstKeyword)
			{
				return ParseConstantFieldDeclaration(attributes, modifiers, parentKind);
			}
			if (base.CurrentToken.Kind == SyntaxKind.EventKeyword)
			{
				return ParseEventDeclaration(attributes, modifiers, parentKind);
			}
			if (base.CurrentToken.Kind == SyntaxKind.FixedKeyword)
			{
				return ParseFixedSizeBufferDeclaration(attributes, modifiers, parentKind);
			}
			MemberDeclarationSyntax result = TryParseConversionOperatorDeclaration(attributes, modifiers);
			if (result != null)
			{
				return result;
			}
			if (isPossibleTypeDeclaration && IsTypeDeclarationStart())
			{
				return ParseTypeDeclaration(attributes, modifiers);
			}
			TypeSyntax type = ParseReturnType();
			ResetPoint state = GetResetPoint();
			try
			{
				if (IsMisplacedModifier(modifiers, attributes, type, out result))
				{
					return result;
				}
				ExplicitInterfaceSpecifierSyntax explicitInterfaceOpt;
				SyntaxToken identifierOrThisOpt;
				TypeParameterListSyntax typeParameterListOpt;
				do
				{
					if (type.Kind != SyntaxKind.RefType && IsOperatorStart(out explicitInterfaceOpt))
					{
						return ParseOperatorDeclaration(attributes, modifiers, type, explicitInterfaceOpt);
					}
					if (IsFieldDeclaration(isEvent: false, isGlobalScriptLevel: false))
					{
						return ParseNormalFieldDeclaration(attributes, modifiers, type, parentKind);
					}
					ParseMemberName(out explicitInterfaceOpt, out identifierOrThisOpt, out typeParameterListOpt, isEvent: false);
					if (IsNoneOrIncompleteMember(parentKind, attributes, modifiers, type, explicitInterfaceOpt, identifierOrThisOpt, typeParameterListOpt, out result))
					{
						return result;
					}
				}
				while (ReconsideredTypeAsAsyncModifier(ref modifiers, ref type, ref state, ref explicitInterfaceOpt, ref identifierOrThisOpt, ref typeParameterListOpt));
				if (TryParseIndexerOrPropertyDeclaration(attributes, modifiers, type, explicitInterfaceOpt, identifierOrThisOpt, typeParameterListOpt, out result))
				{
					return result;
				}
				return ParseMethodDeclaration(attributes, modifiers, type, explicitInterfaceOpt, identifierOrThisOpt, typeParameterListOpt);
			}
			finally
			{
				Release(ref state);
			}
		}
		finally
		{
			_pool.Free(modifiers);
			_termState = termState;
		}
	}

	private bool IsExtensionContainerStart()
	{
		if (base.CurrentToken.ContextualKind == SyntaxKind.ExtensionKeyword)
		{
			if (!IsFeatureEnabled(MessageID.IDS_FeatureExtensions))
			{
				return PeekToken(1).Kind == SyntaxKind.LessThanToken;
			}
			return true;
		}
		return false;
	}

	private static bool ReconsiderTypeAsAsyncModifier(ref SyntaxListBuilder modifiers, TypeSyntax type, SyntaxToken identifierOrThisOpt)
	{
		if (type.Kind != SyntaxKind.IdentifierName)
		{
			return false;
		}
		if (identifierOrThisOpt.Kind != SyntaxKind.IdentifierToken)
		{
			return false;
		}
		SyntaxToken identifier = ((IdentifierNameSyntax)type).Identifier;
		SyntaxKind contextualKind = identifier.ContextualKind;
		if (contextualKind != SyntaxKind.AsyncKeyword || modifiers.Any((int)contextualKind))
		{
			return false;
		}
		modifiers.Add(SyntaxParser.ConvertToKeyword(identifier));
		return true;
	}

	private bool IsFieldDeclaration(bool isEvent, bool isGlobalScriptLevel)
	{
		if (base.CurrentToken.Kind != SyntaxKind.IdentifierToken)
		{
			return false;
		}
		if (base.CurrentToken.ContextualKind == SyntaxKind.GlobalKeyword && PeekToken(1).Kind == SyntaxKind.UsingKeyword)
		{
			return false;
		}
		if (!isGlobalScriptLevel && IsStartOfPropertyBody(1))
		{
			return false;
		}
		switch (PeekToken(1).Kind)
		{
		case SyntaxKind.OpenBraceToken:
		case SyntaxKind.LessThanToken:
		case SyntaxKind.DotToken:
		case SyntaxKind.ColonColonToken:
		case SyntaxKind.EqualsGreaterThanToken:
			return false;
		case SyntaxKind.OpenParenToken:
			return isEvent;
		default:
			return true;
		}
	}

	private bool IsOperatorKeyword()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind - 8382 <= (SyntaxKind)2)
		{
			return true;
		}
		return false;
	}

	public static bool IsComplete(CSharpSyntaxNode node)
	{
		if (node == null)
		{
			return false;
		}
		foreach (GreenNode item in node.ChildNodesAndTokens().Reverse())
		{
			if (!(item is SyntaxToken syntaxToken))
			{
				return IsComplete((CSharpSyntaxNode)item);
			}
			if (syntaxToken.IsMissing)
			{
				return false;
			}
			if (syntaxToken.Kind != SyntaxKind.None)
			{
				return true;
			}
		}
		return true;
	}

	private ConstructorDeclarationSyntax ParseConstructorDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, SyntaxListBuilder modifiers)
	{
		SyntaxToken identifier = ParseIdentifierToken();
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfMethodSignature;
		try
		{
			ParameterListSyntax parameterList = ParseParenthesizedParameterList(forExtension: false);
			ConstructorInitializerSyntax initializer = TryParseConstructorInitializer();
			ParseBlockAndExpressionBodiesWithSemicolon(out var blockBody, out var expressionBody, out var semicolon);
			return _syntaxFactory.ConstructorDeclaration(attributes, modifiers.ToList(), identifier, parameterList, initializer, blockBody, expressionBody, semicolon);
		}
		finally
		{
			_termState = termState;
		}
	}

	private ConstructorInitializerSyntax? TryParseConstructorInitializer()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		bool flag = kind == SyntaxKind.ColonToken;
		if (!flag)
		{
			bool flag2 = kind == SyntaxKind.EqualsGreaterThanToken;
			if (flag2)
			{
				SyntaxKind kind2 = PeekToken(1).Kind;
				bool flag3 = kind2 - 8370 <= SyntaxKind.List;
				flag2 = flag3;
			}
			flag = flag2 && PeekToken(2).Kind == SyntaxKind.OpenParenToken;
		}
		if (!flag)
		{
			return null;
		}
		return ParseConstructorInitializer();
	}

	private ConstructorInitializerSyntax ParseConstructorInitializer()
	{
		SyntaxToken colonToken = EatTokenAsKind(SyntaxKind.ColonToken);
		SyntaxKind kind = base.CurrentToken.Kind;
		bool flag = kind - 8370 <= SyntaxKind.List;
		SyntaxToken syntaxToken = (flag ? EatToken() : EatToken(SyntaxKind.ThisKeyword, ErrorCode.ERR_ThisOrBaseExpected));
		ArgumentListSyntax argumentList = ((base.CurrentToken.Kind == SyntaxKind.OpenParenToken) ? ParseParenthesizedArgumentList() : _syntaxFactory.ArgumentList(EatToken(SyntaxKind.OpenParenToken, !syntaxToken.ContainsDiagnostics), default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ArgumentSyntax>), EatToken(SyntaxKind.CloseParenToken, !syntaxToken.ContainsDiagnostics)));
		return _syntaxFactory.ConstructorInitializer((syntaxToken.Kind == SyntaxKind.BaseKeyword) ? SyntaxKind.BaseConstructorInitializer : SyntaxKind.ThisConstructorInitializer, colonToken, syntaxToken, argumentList);
	}

	private DestructorDeclarationSyntax ParseDestructorDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, SyntaxListBuilder modifiers)
	{
		SyntaxToken tildeToken = EatToken(SyntaxKind.TildeToken);
		SyntaxToken identifier = ParseIdentifierToken();
		ParameterListSyntax parameterList = _syntaxFactory.ParameterList(EatToken(SyntaxKind.OpenParenToken), default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ParameterSyntax>), EatToken(SyntaxKind.CloseParenToken));
		ParseBlockAndExpressionBodiesWithSemicolon(out var blockBody, out var expressionBody, out var semicolon);
		return _syntaxFactory.DestructorDeclaration(attributes, modifiers.ToList(), tildeToken, identifier, parameterList, blockBody, expressionBody, semicolon);
	}

	private void ParseBlockAndExpressionBodiesWithSemicolon(out BlockSyntax blockBody, out ArrowExpressionClauseSyntax expressionBody, out SyntaxToken semicolon, bool parseSemicolonAfterBlock = true)
	{
		if (base.CurrentToken.Kind == SyntaxKind.SemicolonToken)
		{
			blockBody = null;
			expressionBody = null;
			semicolon = EatToken(SyntaxKind.SemicolonToken);
			return;
		}
		blockBody = ((base.CurrentToken.Kind == SyntaxKind.OpenBraceToken) ? ParseMethodOrAccessorBodyBlock(default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>), isAccessorBody: false) : null);
		expressionBody = ((base.CurrentToken.Kind == SyntaxKind.EqualsGreaterThanToken) ? ParseArrowExpressionClause() : null);
		if (expressionBody != null || blockBody == null)
		{
			semicolon = EatToken(SyntaxKind.SemicolonToken);
		}
		else if (parseSemicolonAfterBlock && base.CurrentToken.Kind == SyntaxKind.SemicolonToken)
		{
			semicolon = EatTokenWithPrejudice(ErrorCode.ERR_UnexpectedSemicolon);
		}
		else
		{
			semicolon = null;
		}
	}

	private bool IsEndOfTypeParameterList()
	{
		if (base.CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			return true;
		}
		if (base.CurrentToken.Kind == SyntaxKind.ColonToken)
		{
			return true;
		}
		if (base.CurrentToken.Kind == SyntaxKind.OpenBraceToken)
		{
			return true;
		}
		if (IsCurrentTokenWhereOfConstraintClause())
		{
			return true;
		}
		return false;
	}

	private bool IsEndOfMethodSignature()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind == SyntaxKind.OpenBraceToken || kind == SyntaxKind.SemicolonToken)
		{
			return true;
		}
		return false;
	}

	private bool IsEndOfTypeSignature()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind == SyntaxKind.OpenBraceToken || kind == SyntaxKind.SemicolonToken)
		{
			return true;
		}
		return false;
	}

	private bool IsEndOfNameInExplicitInterface()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind == SyntaxKind.DotToken || kind == SyntaxKind.ColonColonToken)
		{
			return true;
		}
		return false;
	}

	private bool IsEndOfFunctionPointerParameterList(bool errored)
	{
		return (int)base.CurrentToken.Kind == (errored ? 8201 : 8217);
	}

	private bool IsEndOfFunctionPointerCallingConvention()
	{
		return base.CurrentToken.Kind == SyntaxKind.CloseBracketToken;
	}

	private MethodDeclarationSyntax ParseMethodDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceOpt, SyntaxToken identifier, TypeParameterListSyntax typeParameterList)
	{
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfMethodSignature;
		ParameterListSyntax parameterListSyntax = ParseParenthesizedParameterList(forExtension: false);
		SyntaxListBuilder<TypeParameterConstraintClauseSyntax> syntaxListBuilder = default(SyntaxListBuilder<TypeParameterConstraintClauseSyntax>);
		if (base.CurrentToken.ContextualKind == SyntaxKind.WhereKeyword)
		{
			syntaxListBuilder = _pool.Allocate<TypeParameterConstraintClauseSyntax>();
			ParseTypeParameterConstraintClauses(syntaxListBuilder);
		}
		else if (base.CurrentToken.Kind == SyntaxKind.ColonToken)
		{
			SyntaxToken currentToken = base.CurrentToken;
			ConstructorInitializerSyntax node = ParseConstructorInitializer();
			node = AddErrorToFirstToken(node, ErrorCode.ERR_UnexpectedToken, currentToken.Text);
			parameterListSyntax = AddTrailingSkippedSyntax(parameterListSyntax, node);
		}
		_termState = termState;
		using (new ParserSyntaxContextResetter(this, modifiers.Any(8435)))
		{
			ParseBlockAndExpressionBodiesWithSemicolon(out var blockBody, out var expressionBody, out var semicolon);
			return _syntaxFactory.MethodDeclaration(attributes, modifiers.ToList(), type, explicitInterfaceOpt, identifier, typeParameterList, parameterListSyntax, _pool.ToListAndFree(syntaxListBuilder), blockBody, expressionBody, semicolon);
		}
	}

	private TypeSyntax ParseReturnType()
	{
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfReturnType;
		TypeSyntax result = ParseTypeOrVoid();
		_termState = termState;
		return result;
	}

	private bool IsEndOfReturnType()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind == SyntaxKind.OpenParenToken || kind == SyntaxKind.OpenBraceToken || kind == SyntaxKind.SemicolonToken)
		{
			return true;
		}
		return false;
	}

	private ConversionOperatorDeclarationSyntax TryParseConversionOperatorDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, SyntaxListBuilder modifiers)
	{
		ResetPoint state = GetResetPoint();
		try
		{
			bool flag = false;
			SyntaxKind kind = base.CurrentToken.Kind;
			bool flag3;
			if (kind - 8383 > SyntaxKind.List)
			{
				SyntaxKind syntaxKind = SyntaxKind.None;
				if (base.CurrentToken.Kind == SyntaxKind.IdentifierToken)
				{
					while (base.CurrentToken.Kind != SyntaxKind.OperatorKeyword)
					{
						using (DisposableResetPoint disposableResetPoint = GetDisposableResetPoint(resetOnDispose: false))
						{
							int lastTokenPosition = -1;
							IsMakingProgress(ref lastTokenPosition);
							ScanNamedTypePart();
							if (IsDotOrColonColon() || (IsMakingProgress(ref lastTokenPosition, assertIfFalse: false) && base.CurrentToken.Kind != SyntaxKind.OpenParenToken))
							{
								flag = true;
								if (IsDotOrColonColon())
								{
									syntaxKind = base.CurrentToken.Kind;
									EatToken();
								}
								else
								{
									syntaxKind = SyntaxKind.None;
								}
								continue;
							}
							disposableResetPoint.Reset();
						}
						break;
					}
				}
				bool flag2;
				if (base.CurrentToken.Kind != SyntaxKind.OperatorKeyword || (flag && syntaxKind != SyntaxKind.DotToken))
				{
					flag2 = false;
				}
				else
				{
					kind = PeekToken(1).Kind;
					flag3 = kind - 8379 <= SyntaxKind.List;
					flag2 = ((!flag3) ? (!SyntaxFacts.IsAnyOverloadableOperator(PeekToken(1).Kind)) : (!SyntaxFacts.IsAnyOverloadableOperator(PeekToken(2).Kind)));
				}
				Reset(ref state);
				if (!flag2)
				{
					return null;
				}
			}
			kind = base.CurrentToken.Kind;
			flag3 = kind - 8383 <= SyntaxKind.List;
			SyntaxToken syntaxToken = (flag3 ? EatToken() : EatToken(SyntaxKind.ExplicitKeyword));
			ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifierSyntax = tryParseExplicitInterfaceSpecifier();
			SyntaxToken operatorKeyword;
			TypeSyntax type;
			if (!syntaxToken.IsMissing && explicitInterfaceSpecifierSyntax != null && base.CurrentToken.Kind != SyntaxKind.OperatorKeyword && syntaxToken.TrailingTrivia.Any(8539))
			{
				Reset(ref state);
				syntaxToken = EatToken();
				explicitInterfaceSpecifierSyntax = null;
				operatorKeyword = EatToken(SyntaxKind.OperatorKeyword);
				type = AddError(CreateMissingIdentifierName(), ErrorCode.ERR_IdentifierExpected);
				return _syntaxFactory.ConversionOperatorDeclaration(attributes, modifiers.ToList(), syntaxToken, explicitInterfaceSpecifierSyntax, operatorKeyword, null, type, _syntaxFactory.ParameterList(SyntaxFactory.MissingToken(SyntaxKind.OpenParenToken), default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ParameterSyntax>), SyntaxFactory.MissingToken(SyntaxKind.CloseParenToken)), null, null, SyntaxFactory.MissingToken(SyntaxKind.SemicolonToken));
			}
			operatorKeyword = EatToken(SyntaxKind.OperatorKeyword);
			SyntaxToken checkedKeyword = TryEatCheckedOrHandleUnchecked(ref operatorKeyword);
			Release(ref state);
			state = GetResetPoint();
			bool num = base.CurrentToken.Kind == SyntaxKind.OpenParenToken;
			type = ParseType();
			if (num && type is TupleTypeSyntax tupleTypeSyntax)
			{
				Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TupleElementSyntax> elements = tupleTypeSyntax.Elements;
				if (elements.Count == 2 && elements.SeparatorCount == 1 && tupleTypeSyntax.Elements.GetSeparator(0).IsMissing && tupleTypeSyntax.Elements[1].IsMissing && base.CurrentToken.Kind != SyntaxKind.OpenParenToken)
				{
					Reset(ref state);
					type = ParseIdentifierName();
				}
			}
			ParameterListSyntax parameterList = ParseParenthesizedParameterList(forExtension: false);
			ParseBlockAndExpressionBodiesWithSemicolon(out var blockBody, out var expressionBody, out var semicolon);
			return _syntaxFactory.ConversionOperatorDeclaration(attributes, modifiers.ToList(), syntaxToken, explicitInterfaceSpecifierSyntax, operatorKeyword, checkedKeyword, type, parameterList, blockBody, expressionBody, semicolon);
		}
		finally
		{
			Release(ref state);
		}
		ExplicitInterfaceSpecifierSyntax tryParseExplicitInterfaceSpecifier()
		{
			if (base.CurrentToken.Kind != SyntaxKind.IdentifierToken)
			{
				return null;
			}
			NameSyntax explicitInterfaceName = null;
			SyntaxToken separator = null;
			while (true)
			{
				bool flag4;
				using (GetDisposableResetPoint(resetOnDispose: true))
				{
					if (base.CurrentToken.Kind == SyntaxKind.OperatorKeyword)
					{
						flag4 = false;
					}
					else
					{
						int lastTokenPosition2 = -1;
						IsMakingProgress(ref lastTokenPosition2);
						ScanNamedTypePart();
						flag4 = IsDotOrColonColon() || (IsMakingProgress(ref lastTokenPosition2, assertIfFalse: false) && base.CurrentToken.Kind != SyntaxKind.OpenParenToken);
					}
				}
				if (!flag4)
				{
					break;
				}
				AccumulateExplicitInterfaceName(ref explicitInterfaceName, ref separator);
			}
			if (separator != null && separator.Kind == SyntaxKind.ColonColonToken)
			{
				separator = AddError(separator, ErrorCode.ERR_AliasQualAsExpression);
				separator = ConvertToMissingWithTrailingTrivia(separator, SyntaxKind.DotToken);
			}
			if (explicitInterfaceName == null)
			{
				return null;
			}
			if (separator.Kind != SyntaxKind.DotToken)
			{
				separator = WithAdditionalDiagnostics(separator, GetExpectedTokenError(SyntaxKind.DotToken, separator.Kind, separator.GetLeadingTriviaWidth(), separator.Width));
				separator = ConvertToMissingWithTrailingTrivia(separator, SyntaxKind.DotToken);
			}
			return _syntaxFactory.ExplicitInterfaceSpecifier(explicitInterfaceName, separator);
		}
	}

	private SyntaxToken TryEatCheckedOrHandleUnchecked(ref SyntaxToken operatorKeyword)
	{
		if (base.CurrentToken.Kind == SyntaxKind.UncheckedKeyword)
		{
			SyntaxToken skippedSyntax = AddError(EatToken(), ErrorCode.ERR_MisplacedUnchecked);
			operatorKeyword = AddTrailingSkippedSyntax(operatorKeyword, skippedSyntax);
			return null;
		}
		return TryEatToken(SyntaxKind.CheckedKeyword);
	}

	private MemberDeclarationSyntax ParseOperatorDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceOpt)
	{
		SyntaxToken currentToken = base.CurrentToken;
		SyntaxKind kind = currentToken.Kind;
		bool flag = kind - 8383 <= SyntaxKind.List;
		if (flag && PeekToken(1).Kind == SyntaxKind.OperatorKeyword)
		{
			ConversionOperatorDeclarationSyntax conversionOperatorDeclarationSyntax = TryParseConversionOperatorDeclaration(attributes, modifiers);
			if (conversionOperatorDeclarationSyntax != null)
			{
				SyntaxToken implicitOrExplicitKeyword = AddLeadingSkippedSyntax(conversionOperatorDeclarationSyntax.ImplicitOrExplicitKeyword, AddError(type, ErrorCode.ERR_BadOperatorSyntax, currentToken.Text));
				return conversionOperatorDeclarationSyntax.Update(conversionOperatorDeclarationSyntax.AttributeLists, conversionOperatorDeclarationSyntax.Modifiers, implicitOrExplicitKeyword, conversionOperatorDeclarationSyntax.ExplicitInterfaceSpecifier, conversionOperatorDeclarationSyntax.OperatorKeyword, conversionOperatorDeclarationSyntax.CheckedKeyword, conversionOperatorDeclarationSyntax.Type, conversionOperatorDeclarationSyntax.ParameterList, conversionOperatorDeclarationSyntax.Body, conversionOperatorDeclarationSyntax.ExpressionBody, conversionOperatorDeclarationSyntax.SemicolonToken);
			}
		}
		SyntaxToken operatorKeyword = EatToken(SyntaxKind.OperatorKeyword);
		SyntaxToken checkedKeyword = TryEatCheckedOrHandleUnchecked(ref operatorKeyword);
		SyntaxToken syntaxToken;
		int offset;
		int width;
		if (SyntaxFacts.IsAnyOverloadableOperator(base.CurrentToken.Kind))
		{
			syntaxToken = EatToken();
			offset = syntaxToken.GetLeadingTriviaWidth();
			width = syntaxToken.Width;
		}
		else
		{
			kind = base.CurrentToken.Kind;
			if (kind - 8383 <= SyntaxKind.List)
			{
				width = base.CurrentToken.Width;
				offset = 0;
				syntaxToken = ConvertToMissingWithTrailingTrivia(EatToken(), SyntaxKind.PlusToken);
				if (type.IsMissing)
				{
					SyntaxDiagnosticInfo syntaxDiagnosticInfo = SyntaxParser.MakeError(offset, width, ErrorCode.ERR_BadOperatorSyntax, SyntaxFacts.GetText(SyntaxKind.PlusToken));
					syntaxToken = WithAdditionalDiagnostics(syntaxToken, syntaxDiagnosticInfo);
				}
				else
				{
					type = AddError(type, ErrorCode.ERR_BadOperatorSyntax, SyntaxFacts.GetText(SyntaxKind.PlusToken));
				}
			}
			else
			{
				syntaxToken = (IsAtDotDotToken() ? EatDotDotToken() : EatToken());
				offset = syntaxToken.GetLeadingTriviaWidth();
				width = syntaxToken.Width;
			}
		}
		if (syntaxToken.Kind == SyntaxKind.GreaterThanToken)
		{
			SyntaxToken currentToken2 = base.CurrentToken;
			if (currentToken2.Kind == SyntaxKind.GreaterThanToken)
			{
				if (NoTriviaBetween(syntaxToken, currentToken2))
				{
					SyntaxToken syntaxToken2 = EatToken();
					currentToken2 = base.CurrentToken;
					if (currentToken2.Kind == SyntaxKind.GreaterThanToken && NoTriviaBetween(syntaxToken2, currentToken2))
					{
						syntaxToken2 = EatToken();
						syntaxToken = SyntaxFactory.Token(syntaxToken.GetLeadingTrivia(), SyntaxKind.GreaterThanGreaterThanGreaterThanToken, syntaxToken2.GetTrailingTrivia());
						width = syntaxToken.Width;
					}
					else if (currentToken2.Kind == SyntaxKind.GreaterThanEqualsToken && NoTriviaBetween(syntaxToken2, currentToken2))
					{
						syntaxToken2 = EatToken();
						syntaxToken = SyntaxFactory.Token(syntaxToken.GetLeadingTrivia(), SyntaxKind.GreaterThanGreaterThanGreaterThanEqualsToken, syntaxToken2.GetTrailingTrivia());
						width = syntaxToken.Width;
					}
					else
					{
						syntaxToken = SyntaxFactory.Token(syntaxToken.GetLeadingTrivia(), SyntaxKind.GreaterThanGreaterThanToken, syntaxToken2.GetTrailingTrivia());
						width = syntaxToken.Width;
					}
				}
			}
			else if (currentToken2.Kind == SyntaxKind.GreaterThanEqualsToken && NoTriviaBetween(syntaxToken, currentToken2))
			{
				SyntaxToken syntaxToken3 = EatToken();
				syntaxToken = SyntaxFactory.Token(syntaxToken.GetLeadingTrivia(), SyntaxKind.GreaterThanGreaterThanEqualsToken, syntaxToken3.GetTrailingTrivia());
				width = syntaxToken.Width;
			}
		}
		SyntaxKind kind2 = syntaxToken.Kind;
		ParameterListSyntax parameterListSyntax = ParseParenthesizedParameterList(forExtension: false);
		switch (parameterListSyntax.Parameters.Count)
		{
		case 1:
			if (syntaxToken.IsMissing || (!SyntaxFacts.IsOverloadableUnaryOperator(kind2) && !SyntaxFacts.IsOverloadableCompoundAssignmentOperator(kind2)))
			{
				SyntaxDiagnosticInfo syntaxDiagnosticInfo3 = SyntaxParser.MakeError(offset, width, ErrorCode.ERR_OvlUnaryOperatorExpected);
				syntaxToken = WithAdditionalDiagnostics(syntaxToken, syntaxDiagnosticInfo3);
			}
			break;
		case 2:
			if (syntaxToken.IsMissing || !SyntaxFacts.IsOverloadableBinaryOperator(kind2))
			{
				SyntaxDiagnosticInfo syntaxDiagnosticInfo4 = SyntaxParser.MakeError(offset, width, ErrorCode.ERR_OvlBinaryOperatorExpected);
				syntaxToken = WithAdditionalDiagnostics(syntaxToken, syntaxDiagnosticInfo4);
			}
			break;
		default:
			if (syntaxToken.IsMissing)
			{
				SyntaxDiagnosticInfo syntaxDiagnosticInfo2 = SyntaxParser.MakeError(offset, width, ErrorCode.ERR_OvlOperatorExpected);
				syntaxToken = WithAdditionalDiagnostics(syntaxToken, syntaxDiagnosticInfo2);
			}
			else if (SyntaxFacts.IsOverloadableBinaryOperator(kind2))
			{
				syntaxToken = AddError(syntaxToken, ErrorCode.ERR_BadBinOpArgs, SyntaxFacts.GetText(kind2));
			}
			else if (SyntaxFacts.IsOverloadableUnaryOperator(kind2))
			{
				flag = kind2 - 8262 <= SyntaxKind.List;
				if (!flag || parameterListSyntax.Parameters.Count != 0)
				{
					syntaxToken = AddError(syntaxToken, ErrorCode.ERR_BadUnOpArgs, SyntaxFacts.GetText(kind2));
				}
			}
			else
			{
				syntaxToken = ((!SyntaxFacts.IsOverloadableCompoundAssignmentOperator(kind2)) ? AddError(syntaxToken, ErrorCode.ERR_OvlOperatorExpected) : AddError(syntaxToken, ErrorCode.ERR_BadCompoundAssignmentOpArgs, SyntaxFacts.GetText(kind2)));
			}
			break;
		}
		ParseBlockAndExpressionBodiesWithSemicolon(out var blockBody, out var expressionBody, out var semicolon);
		if (kind2 != SyntaxKind.IsKeyword && !SyntaxFacts.IsOverloadableUnaryOperator(kind2) && !SyntaxFacts.IsOverloadableBinaryOperator(kind2) && !SyntaxFacts.IsOverloadableCompoundAssignmentOperator(kind2))
		{
			syntaxToken = ConvertToMissingWithTrailingTrivia(syntaxToken, SyntaxKind.PlusToken);
		}
		return _syntaxFactory.OperatorDeclaration(attributes, modifiers.ToList(), type, explicitInterfaceOpt, operatorKeyword, checkedKeyword, syntaxToken, parameterListSyntax, blockBody, expressionBody, semicolon);
	}

	private IndexerDeclarationSyntax ParseIndexerDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceOpt, SyntaxToken thisKeyword, TypeParameterListSyntax typeParameterList)
	{
		if (typeParameterList != null)
		{
			thisKeyword = AddTrailingSkippedSyntax(thisKeyword, typeParameterList);
			thisKeyword = AddError(thisKeyword, ErrorCode.ERR_UnexpectedGenericName);
		}
		BracketedParameterListSyntax parameterList = ParseBracketedParameterList();
		AccessorListSyntax accessorList = null;
		ArrowExpressionClauseSyntax expressionBody = null;
		SyntaxToken syntaxToken = null;
		if (base.CurrentToken.Kind == SyntaxKind.EqualsGreaterThanToken)
		{
			expressionBody = ParseArrowExpressionClause();
			syntaxToken = EatToken(SyntaxKind.SemicolonToken);
		}
		else
		{
			accessorList = ParseAccessorList(AccessorDeclaringKind.Indexer);
			if (base.CurrentToken.Kind == SyntaxKind.SemicolonToken)
			{
				syntaxToken = EatTokenWithPrejudice(ErrorCode.ERR_UnexpectedSemicolon);
			}
		}
		if (base.CurrentToken.Kind == SyntaxKind.EqualsGreaterThanToken && syntaxToken == null)
		{
			expressionBody = ParseArrowExpressionClause();
			syntaxToken = EatToken(SyntaxKind.SemicolonToken);
		}
		return _syntaxFactory.IndexerDeclaration(attributes, modifiers.ToList(), type, explicitInterfaceOpt, thisKeyword, parameterList, accessorList, expressionBody, syntaxToken);
	}

	private PropertyDeclarationSyntax ParsePropertyDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceOpt, SyntaxToken identifier, TypeParameterListSyntax typeParameterList)
	{
		if (typeParameterList != null)
		{
			identifier = AddTrailingSkippedSyntax(identifier, typeParameterList);
			identifier = AddError(identifier, ErrorCode.ERR_UnexpectedGenericName);
		}
		if (base.CurrentToken.Kind == SyntaxKind.SemicolonToken)
		{
			identifier = AddTrailingSkippedSyntax(identifier, EatTokenEvenWithIncorrectKind(SyntaxKind.OpenBraceToken));
		}
		AccessorListSyntax accessorList = ((base.CurrentToken.Kind == SyntaxKind.OpenBraceToken) ? ParseAccessorList(AccessorDeclaringKind.Property) : null);
		ArrowExpressionClauseSyntax arrowExpressionClauseSyntax = null;
		EqualsValueClauseSyntax equalsValueClauseSyntax = null;
		if (base.CurrentToken.Kind == SyntaxKind.EqualsGreaterThanToken)
		{
			bool? isInFieldKeywordContext = true;
			using (new ParserSyntaxContextResetter(this, null, null, isInFieldKeywordContext))
			{
				arrowExpressionClauseSyntax = ParseArrowExpressionClause();
			}
		}
		else if (base.CurrentToken.Kind == SyntaxKind.EqualsToken)
		{
			SyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
			ExpressionSyntax value = ParseVariableInitializer();
			equalsValueClauseSyntax = _syntaxFactory.EqualsValueClause(equalsToken, value);
		}
		SyntaxToken semicolonToken = null;
		if (arrowExpressionClauseSyntax != null || equalsValueClauseSyntax != null)
		{
			semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		}
		else if (base.CurrentToken.Kind == SyntaxKind.SemicolonToken)
		{
			semicolonToken = EatTokenWithPrejudice(ErrorCode.ERR_UnexpectedSemicolon);
		}
		return _syntaxFactory.PropertyDeclaration(attributes, modifiers.ToList(), type, explicitInterfaceOpt, identifier, accessorList, arrowExpressionClauseSyntax, equalsValueClauseSyntax, semicolonToken);
	}

	private AccessorListSyntax ParseAccessorList(AccessorDeclaringKind declaringKind)
	{
		SyntaxToken openBrace = EatToken(SyntaxKind.OpenBraceToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AccessorDeclarationSyntax> accessors = default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AccessorDeclarationSyntax>);
		if (!openBrace.IsMissing || !IsTerminator())
		{
			SyntaxListBuilder<AccessorDeclarationSyntax> syntaxListBuilder = _pool.Allocate<AccessorDeclarationSyntax>();
			while (base.CurrentToken.Kind != SyntaxKind.CloseBraceToken)
			{
				if (IsPossibleAccessor())
				{
					AccessorDeclarationSyntax node = ParseAccessorDeclaration(declaringKind);
					syntaxListBuilder.Add(node);
				}
				else if (SkipBadAccessorListTokens(ref openBrace, syntaxListBuilder, (declaringKind == AccessorDeclaringKind.Event) ? ErrorCode.ERR_AddOrRemoveExpected : ErrorCode.ERR_GetOrSetExpected) == PostSkipAction.Abort)
				{
					break;
				}
			}
			accessors = _pool.ToListAndFree(syntaxListBuilder);
		}
		return _syntaxFactory.AccessorList(openBrace, accessors, EatToken(SyntaxKind.CloseBraceToken));
	}

	private ArrowExpressionClauseSyntax ParseArrowExpressionClause()
	{
		return _syntaxFactory.ArrowExpressionClause(EatToken(SyntaxKind.EqualsGreaterThanToken), ParsePossibleRefExpression());
	}

	private ExpressionSyntax ParsePossibleRefExpression()
	{
		SyntaxToken syntaxToken = ((base.CurrentToken.Kind == SyntaxKind.RefKeyword && !IsPossibleLambdaExpression(Precedence.Expression)) ? EatToken() : null);
		ExpressionSyntax expressionSyntax = ParseExpressionCore();
		if (syntaxToken != null)
		{
			return _syntaxFactory.RefExpression(syntaxToken, expressionSyntax);
		}
		return expressionSyntax;
	}

	private PostSkipAction SkipBadAccessorListTokens(ref SyntaxToken openBrace, SyntaxListBuilder<AccessorDeclarationSyntax> list, ErrorCode error)
	{
		return SkipBadListTokensWithErrorCode(ref openBrace, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CloseBraceToken && !p.IsPossibleAccessor(), (LanguageParser p) => p.IsTerminator(), error);
	}

	private bool IsPossibleAccessor()
	{
		if (base.CurrentToken.Kind != SyntaxKind.IdentifierToken && !IsPossibleAttributeDeclaration() && SyntaxFacts.GetAccessorDeclarationKind(base.CurrentToken.ContextualKind) == SyntaxKind.None && base.CurrentToken.Kind != SyntaxKind.OpenBraceToken && base.CurrentToken.Kind != SyntaxKind.SemicolonToken)
		{
			return IsPossibleAccessorModifier();
		}
		return true;
	}

	private bool IsPossibleAccessorModifier()
	{
		if (GetModifierExcludingScoped(base.CurrentToken) == DeclarationModifiers.None)
		{
			return false;
		}
		int i;
		for (i = 1; GetModifierExcludingScoped(PeekToken(i)) != DeclarationModifiers.None; i++)
		{
		}
		SyntaxToken syntaxToken = PeekToken(i);
		SyntaxKind kind = syntaxToken.Kind;
		if ((kind == SyntaxKind.CloseBraceToken || kind == SyntaxKind.EndOfFileToken) ? true : false)
		{
			return true;
		}
		kind = syntaxToken.ContextualKind;
		if (kind - 8417 <= (SyntaxKind)3 || kind == SyntaxKind.InitKeyword)
		{
			return true;
		}
		return false;
	}

	private PostSkipAction SkipBadSeparatedListTokensWithExpectedKind<T, TNode>(ref T startToken, SeparatedSyntaxListBuilder<TNode> list, Func<LanguageParser, bool> isNotExpectedFunction, Func<LanguageParser, SyntaxKind, bool> abortFunction, SyntaxKind expected, SyntaxKind closeKind = SyntaxKind.None) where T : CSharpSyntaxNode where TNode : CSharpSyntaxNode
	{
		PostSkipAction result = SkipBadListTokensWithExpectedKindHelper(list.UnderlyingBuilder, isNotExpectedFunction, abortFunction, expected, closeKind, out var trailingTrivia);
		if (trailingTrivia != null)
		{
			startToken = AddTrailingSkippedSyntax(startToken, trailingTrivia);
		}
		return result;
	}

	private PostSkipAction SkipBadListTokensWithErrorCode<T, TNode>(ref T startToken, SyntaxListBuilder<TNode> list, Func<LanguageParser, bool> isNotExpectedFunction, Func<LanguageParser, bool> abortFunction, ErrorCode error) where T : CSharpSyntaxNode where TNode : CSharpSyntaxNode
	{
		PostSkipAction result = SkipBadListTokensWithErrorCodeHelper(list, isNotExpectedFunction, abortFunction, error, out var trailingTrivia);
		if (trailingTrivia != null)
		{
			startToken = AddTrailingSkippedSyntax(startToken, trailingTrivia);
		}
		return result;
	}

	private PostSkipAction SkipBadListTokensWithExpectedKindHelper(SyntaxListBuilder list, Func<LanguageParser, bool> isNotExpectedFunction, Func<LanguageParser, SyntaxKind, bool> abortFunction, SyntaxKind expected, SyntaxKind closeKind, out GreenNode trailingTrivia)
	{
		if (list.Count == 0)
		{
			return SkipBadTokensWithExpectedKind(isNotExpectedFunction, abortFunction, expected, closeKind, out trailingTrivia);
		}
		PostSkipAction result = SkipBadTokensWithExpectedKind(isNotExpectedFunction, abortFunction, expected, closeKind, out var trailingTrivia2);
		if (trailingTrivia2 != null)
		{
			AddTrailingSkippedSyntax(list, trailingTrivia2);
		}
		trailingTrivia = null;
		return result;
	}

	private PostSkipAction SkipBadListTokensWithErrorCodeHelper<TNode>(SyntaxListBuilder<TNode> list, Func<LanguageParser, bool> isNotExpectedFunction, Func<LanguageParser, bool> abortFunction, ErrorCode error, out GreenNode trailingTrivia) where TNode : CSharpSyntaxNode
	{
		if (list.Count == 0)
		{
			return SkipBadTokensWithErrorCode(isNotExpectedFunction, abortFunction, error, out trailingTrivia);
		}
		PostSkipAction result = SkipBadTokensWithErrorCode(isNotExpectedFunction, abortFunction, error, out var trailingTrivia2);
		if (trailingTrivia2 != null)
		{
			AddTrailingSkippedSyntax(list, trailingTrivia2);
		}
		trailingTrivia = null;
		return result;
	}

	private PostSkipAction SkipBadTokensWithExpectedKind(Func<LanguageParser, bool> isNotExpectedFunction, Func<LanguageParser, SyntaxKind, bool> abortFunction, SyntaxKind expected, SyntaxKind closeKind, out GreenNode trailingTrivia)
	{
		SyntaxListBuilder syntaxListBuilder = _pool.Allocate();
		bool flag = true;
		PostSkipAction result = PostSkipAction.Continue;
		while (isNotExpectedFunction(this))
		{
			if (abortFunction(this, closeKind) || IsTerminator())
			{
				result = PostSkipAction.Abort;
				break;
			}
			SyntaxToken item = ((flag && !base.CurrentToken.ContainsDiagnostics) ? EatTokenEvenWithIncorrectKind(expected) : EatToken());
			flag = false;
			syntaxListBuilder.Add(item);
		}
		trailingTrivia = _pool.ToTokenListAndFree(syntaxListBuilder).Node;
		return result;
	}

	private PostSkipAction SkipBadTokensWithErrorCode(Func<LanguageParser, bool> isNotExpectedFunction, Func<LanguageParser, bool> abortFunction, ErrorCode errorCode, out GreenNode trailingTrivia)
	{
		SyntaxListBuilder syntaxListBuilder = _pool.Allocate();
		bool flag = true;
		PostSkipAction result = PostSkipAction.Continue;
		while (isNotExpectedFunction(this))
		{
			if (abortFunction(this))
			{
				result = PostSkipAction.Abort;
				break;
			}
			SyntaxToken item = ((flag && !base.CurrentToken.ContainsDiagnostics) ? EatTokenWithPrejudice(errorCode) : EatToken());
			flag = false;
			syntaxListBuilder.Add(item);
		}
		trailingTrivia = _pool.ToTokenListAndFree(syntaxListBuilder).Node;
		return result;
	}

	private AccessorDeclarationSyntax ParseAccessorDeclaration(AccessorDeclaringKind declaringKind)
	{
		if (IsIncrementalAndFactoryContextMatches && SyntaxFacts.IsAccessorDeclaration(base.CurrentNodeKind))
		{
			return (AccessorDeclarationSyntax)EatNode();
		}
		bool? isInFieldKeywordContext = declaringKind == AccessorDeclaringKind.Property;
		using (new ParserSyntaxContextResetter(this, null, null, isInFieldKeywordContext))
		{
			SyntaxListBuilder syntaxListBuilder = _pool.Allocate();
			Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = ParseAttributeDeclarations(inExpressionContext: false);
			ParseModifiers(syntaxListBuilder, forAccessors: true, forTopLevelStatements: false, out var _);
			SyntaxToken syntaxToken = EatToken(SyntaxKind.IdentifierToken, (declaringKind == AccessorDeclaringKind.Event) ? ErrorCode.ERR_AddOrRemoveExpected : ErrorCode.ERR_GetOrSetExpected);
			SyntaxKind accessorKind = GetAccessorKind(syntaxToken);
			if (accessorKind == SyntaxKind.UnknownAccessorDeclaration)
			{
				if (!syntaxToken.IsMissing)
				{
					syntaxToken = AddError(syntaxToken, (declaringKind == AccessorDeclaringKind.Event) ? ErrorCode.ERR_AddOrRemoveExpected : ErrorCode.ERR_GetOrSetExpected);
				}
			}
			else
			{
				syntaxToken = SyntaxParser.ConvertToKeyword(syntaxToken);
			}
			BlockSyntax blockBody = null;
			ArrowExpressionClauseSyntax expressionBody = null;
			SyntaxToken semicolon = null;
			bool flag = base.CurrentToken.Kind == SyntaxKind.SemicolonToken;
			bool flag2 = base.CurrentToken.Kind == SyntaxKind.EqualsGreaterThanToken;
			if ((base.CurrentToken.Kind == SyntaxKind.OpenBraceToken) | flag2)
			{
				ParseBlockAndExpressionBodiesWithSemicolon(out blockBody, out expressionBody, out semicolon);
			}
			else if (flag)
			{
				semicolon = EatAccessorSemicolon();
			}
			else if (accessorKind != SyntaxKind.UnknownAccessorDeclaration)
			{
				if (!IsTerminator())
				{
					blockBody = ParseMethodOrAccessorBodyBlock(default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>), isAccessorBody: true);
				}
				else
				{
					semicolon = EatAccessorSemicolon();
				}
			}
			return _syntaxFactory.AccessorDeclaration(accessorKind, attributeLists, _pool.ToTokenListAndFree(syntaxListBuilder), syntaxToken, blockBody, expressionBody, semicolon);
		}
	}

	private SyntaxToken EatAccessorSemicolon()
	{
		return EatToken(SyntaxKind.SemicolonToken, IsFeatureEnabled(MessageID.IDS_FeatureExpressionBodiedAccessor) ? ErrorCode.ERR_SemiOrLBraceOrArrowExpected : ErrorCode.ERR_SemiOrLBraceExpected);
	}

	private static SyntaxKind GetAccessorKind(SyntaxToken accessorName)
	{
		return accessorName.ContextualKind switch
		{
			SyntaxKind.GetKeyword => SyntaxKind.GetAccessorDeclaration, 
			SyntaxKind.SetKeyword => SyntaxKind.SetAccessorDeclaration, 
			SyntaxKind.InitKeyword => SyntaxKind.InitAccessorDeclaration, 
			SyntaxKind.AddKeyword => SyntaxKind.AddAccessorDeclaration, 
			SyntaxKind.RemoveKeyword => SyntaxKind.RemoveAccessorDeclaration, 
			_ => SyntaxKind.UnknownAccessorDeclaration, 
		};
	}

	internal ParameterListSyntax ParseParenthesizedParameterList(bool forExtension)
	{
		if (IsIncrementalAndFactoryContextMatches && CanReuseParameterList(base.CurrentNode as Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax, forExtension))
		{
			return (ParameterListSyntax)EatNode();
		}
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ParameterSyntax> parameters = ParseParameterList(out var open, out var close, SyntaxKind.OpenParenToken, SyntaxKind.CloseParenToken, forExtension);
		return _syntaxFactory.ParameterList(open, parameters, close);
	}

	internal BracketedParameterListSyntax ParseBracketedParameterList()
	{
		if (IsIncrementalAndFactoryContextMatches && CanReuseBracketedParameterList(base.CurrentNode as Microsoft.CodeAnalysis.CSharp.Syntax.BracketedParameterListSyntax))
		{
			return (BracketedParameterListSyntax)EatNode();
		}
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ParameterSyntax> parameters = ParseParameterList(out var open, out var close, SyntaxKind.OpenBracketToken, SyntaxKind.CloseBracketToken, forExtension: false);
		return _syntaxFactory.BracketedParameterList(open, parameters, close);
	}

	private static bool CanReuseParameterList(Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax list, bool allowOptionalIdentifier)
	{
		if (list == null)
		{
			return false;
		}
		if (list.OpenParenToken.IsMissing)
		{
			return false;
		}
		if (list.CloseParenToken.IsMissing)
		{
			return false;
		}
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax parameter in list.Parameters)
		{
			if (!CanReuseParameter(parameter, allowOptionalIdentifier))
			{
				return false;
			}
		}
		return true;
	}

	private static bool CanReuseBracketedParameterList(Microsoft.CodeAnalysis.CSharp.Syntax.BracketedParameterListSyntax list)
	{
		if (list == null)
		{
			return false;
		}
		if (list.OpenBracketToken.IsMissing)
		{
			return false;
		}
		if (list.CloseBracketToken.IsMissing)
		{
			return false;
		}
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax parameter in list.Parameters)
		{
			if (!CanReuseParameter(parameter, allowOptionalIdentifier: false))
			{
				return false;
			}
		}
		return true;
	}

	private Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ParameterSyntax> ParseParameterList(out SyntaxToken open, out SyntaxToken close, SyntaxKind openKind, SyntaxKind closeKind, bool forExtension)
	{
		open = EatToken(openKind);
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfParameterList;
		Func<LanguageParser, ParameterSyntax> parseElement = (forExtension ? ((Func<LanguageParser, ParameterSyntax>)((LanguageParser @this) => @this.ParseParameter(allowOptionalIdentifier: true))) : ((Func<LanguageParser, ParameterSyntax>)((LanguageParser @this) => @this.ParseParameter(allowOptionalIdentifier: false))));
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ParameterSyntax> result = ParseCommaSeparatedSyntaxList(ref open, closeKind, (LanguageParser @this) => @this.IsPossibleParameter(), parseElement, skipBadParameterListTokens, allowTrailingSeparator: false, forExtension, allowSemicolonAsSeparator: false);
		_termState = termState;
		close = EatToken(closeKind);
		return result;
		static PostSkipAction skipBadParameterListTokens(LanguageParser @this, ref SyntaxToken startToken, SeparatedSyntaxListBuilder<ParameterSyntax> list, SyntaxKind expectedKind, SyntaxKind closeKind2)
		{
			return @this.SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleParameter(), (LanguageParser p, SyntaxKind syntaxKind) => p.CurrentToken.Kind == syntaxKind, expectedKind, closeKind2);
		}
	}

	private bool IsEndOfParameterList()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind == SyntaxKind.CloseParenToken || kind == SyntaxKind.CloseBracketToken || kind == SyntaxKind.SemicolonToken)
		{
			return true;
		}
		return false;
	}

	private bool IsPossibleParameter()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind <= SyntaxKind.OpenBracketToken)
		{
			if (kind == SyntaxKind.OpenParenToken || kind == SyntaxKind.OpenBracketToken)
			{
				goto IL_0048;
			}
		}
		else
		{
			if (kind == SyntaxKind.ArgListKeyword)
			{
				goto IL_0048;
			}
			if (kind != SyntaxKind.DelegateKeyword)
			{
				if (kind == SyntaxKind.IdentifierToken)
				{
					return IsTrueIdentifier();
				}
			}
			else if (IsFunctionPointerStart())
			{
				goto IL_0048;
			}
		}
		if (!IsParameterModifierExcludingScoped(base.CurrentToken) && !IsDefiniteScopedModifier(isFunctionPointerParameter: false, isLambdaParameter: false))
		{
			return IsPredefinedType(base.CurrentToken.Kind);
		}
		return true;
		IL_0048:
		return true;
	}

	private static bool CanReuseParameter(Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax parameter, bool allowOptionalIdentifier)
	{
		if (parameter == null)
		{
			return false;
		}
		if (parameter.Default != null)
		{
			return false;
		}
		Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode parent = parameter.Parent;
		if (parent != null)
		{
			if (parent.Kind() == SyntaxKind.SimpleLambdaExpression)
			{
				return false;
			}
			Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode parent2 = parent.Parent;
			if (parent2 != null && parent2.Kind() == SyntaxKind.ParenthesizedLambdaExpression)
			{
				return false;
			}
		}
		if (!allowOptionalIdentifier && parameter.Identifier.Kind() == SyntaxKind.None)
		{
			return false;
		}
		return true;
	}

	private ParameterSyntax ParseParameter(bool allowOptionalIdentifier)
	{
		if (IsIncrementalAndFactoryContextMatches && CanReuseParameter(base.CurrentNode as Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax, allowOptionalIdentifier))
		{
			return (ParameterSyntax)EatNode();
		}
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = ParseAttributeDeclarations(inExpressionContext: false);
		SyntaxListBuilder syntaxListBuilder = _pool.Allocate();
		ParseParameterModifiers(syntaxListBuilder, isFunctionPointerParameter: false, isLambdaParameter: false);
		if (base.CurrentToken.Kind == SyntaxKind.ArgListKeyword)
		{
			return _syntaxFactory.Parameter(attributeLists, syntaxListBuilder.ToList(), null, EatToken(SyntaxKind.ArgListKeyword), null);
		}
		TypeSyntax type = ParseType(ParseTypeMode.Parameter);
		SyntaxToken syntaxToken = ((base.CurrentToken.Kind != SyntaxKind.IdentifierToken || !IsCurrentTokenWhereOfConstraintClause()) ? ((allowOptionalIdentifier && base.CurrentToken.Kind != SyntaxKind.IdentifierToken) ? null : ParseIdentifierToken()) : (allowOptionalIdentifier ? null : AddError(CreateMissingIdentifierToken(), ErrorCode.ERR_IdentifierExpected)));
		if (syntaxToken != null && base.CurrentToken.Kind == SyntaxKind.OpenBracketToken && PeekToken(1).Kind == SyntaxKind.CloseBracketToken)
		{
			syntaxToken = AddTrailingSkippedSyntax(syntaxToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(AddError(EatToken(), ErrorCode.ERR_BadArraySyntax), EatToken()));
		}
		SyntaxToken syntaxToken2 = TryEatToken(SyntaxKind.EqualsToken);
		return _syntaxFactory.Parameter(attributeLists, _pool.ToTokenListAndFree(syntaxListBuilder), type, syntaxToken, (syntaxToken2 == null) ? null : _syntaxFactory.EqualsValueClause(syntaxToken2, ParseExpressionCore()));
	}

	internal static bool NoTriviaBetween(SyntaxToken token1, SyntaxToken token2)
	{
		if (token1.GetTrailingTriviaWidth() == 0)
		{
			return token2.GetLeadingTriviaWidth() == 0;
		}
		return false;
	}

	private static bool IsParameterModifierIncludingScoped(SyntaxToken token)
	{
		if (!IsParameterModifierExcludingScoped(token))
		{
			return token.ContextualKind == SyntaxKind.ScopedKeyword;
		}
		return true;
	}

	private static bool IsParameterModifierExcludingScoped(SyntaxToken token)
	{
		switch (token.Kind)
		{
		case SyntaxKind.ReadOnlyKeyword:
		case SyntaxKind.RefKeyword:
		case SyntaxKind.OutKeyword:
		case SyntaxKind.InKeyword:
		case SyntaxKind.ParamsKeyword:
		case SyntaxKind.ThisKeyword:
			return true;
		default:
			return false;
		}
	}

	private void ParseParameterModifiers(SyntaxListBuilder modifiers, bool isFunctionPointerParameter, bool isLambdaParameter)
	{
		bool flag = false;
		while (true)
		{
			if (IsParameterModifierExcludingScoped(base.CurrentToken))
			{
				modifiers.Add(EatToken());
				continue;
			}
			if (!IsDefiniteScopedModifier(isFunctionPointerParameter, isLambdaParameter))
			{
				break;
			}
			if (!flag)
			{
				flag = true;
				modifiers.Add(EatContextualToken(SyntaxKind.ScopedKeyword));
				continue;
			}
			SyntaxKind kind = PeekToken(1).Kind;
			if ((kind != SyntaxKind.CloseParenToken && kind != SyntaxKind.EqualsToken && kind != SyntaxKind.CommaToken) || 1 == 0)
			{
				modifiers.Add(EatContextualToken(SyntaxKind.ScopedKeyword));
				continue;
			}
			break;
		}
	}

	private FieldDeclarationSyntax ParseFixedSizeBufferDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, SyntaxKind parentKind)
	{
		modifiers.Add(EatToken());
		TypeSyntax type = ParseType();
		return _syntaxFactory.FieldDeclaration(attributes, modifiers.ToList(), _syntaxFactory.VariableDeclaration(type, ParseFieldDeclarationVariableDeclarators(type, VariableFlags.Fixed, parentKind)), EatToken(SyntaxKind.SemicolonToken));
	}

	private MemberDeclarationSyntax ParseEventDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, SyntaxKind parentKind)
	{
		SyntaxToken eventToken = EatToken();
		TypeSyntax type = ParseType();
		if (!IsFieldDeclaration(isEvent: true, parentKind == SyntaxKind.CompilationUnit))
		{
			return ParseEventDeclarationWithAccessors(attributes, modifiers, eventToken, type);
		}
		return ParseEventFieldDeclaration(attributes, modifiers, eventToken, type, parentKind);
	}

	private EventDeclarationSyntax ParseEventDeclarationWithAccessors(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, SyntaxToken eventToken, TypeSyntax type)
	{
		ParseMemberName(out var explicitInterfaceOpt, out var identifierOrThisOpt, out var typeParameterListOpt, isEvent: true);
		if (explicitInterfaceOpt != null)
		{
			SyntaxKind kind = base.CurrentToken.Kind;
			if (kind != SyntaxKind.OpenBraceToken && kind != SyntaxKind.SemicolonToken)
			{
				return _syntaxFactory.EventDeclaration(attributes, modifiers.ToList(), eventToken, type, explicitInterfaceOpt, (identifierOrThisOpt == null) ? CreateMissingIdentifierToken() : identifierOrThisOpt, _syntaxFactory.AccessorList(SyntaxFactory.MissingToken(SyntaxKind.OpenBraceToken), default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AccessorDeclarationSyntax>), SyntaxFactory.MissingToken(SyntaxKind.CloseBraceToken)), null);
			}
		}
		SyntaxToken syntaxToken = ((identifierOrThisOpt == null) ? CreateMissingIdentifierToken() : ((identifierOrThisOpt.Kind == SyntaxKind.IdentifierToken) ? identifierOrThisOpt : ConvertToMissingWithTrailingTrivia(identifierOrThisOpt, SyntaxKind.IdentifierToken)));
		if (syntaxToken.IsMissing && !type.IsMissing)
		{
			syntaxToken = AddError(syntaxToken, ErrorCode.ERR_IdentifierExpected);
		}
		if (typeParameterListOpt != null)
		{
			syntaxToken = AddTrailingSkippedSyntax(syntaxToken, typeParameterListOpt);
			syntaxToken = AddError(syntaxToken, ErrorCode.ERR_UnexpectedGenericName);
		}
		AccessorListSyntax accessorList = null;
		SyntaxToken semicolonToken = null;
		if (explicitInterfaceOpt != null && base.CurrentToken.Kind == SyntaxKind.SemicolonToken)
		{
			semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		}
		else
		{
			accessorList = ParseAccessorList(AccessorDeclaringKind.Event);
		}
		EventDeclarationSyntax decl = _syntaxFactory.EventDeclaration(attributes, modifiers.ToList(), eventToken, type, explicitInterfaceOpt, syntaxToken, accessorList, semicolonToken);
		return EatUnexpectedTrailingSemicolon(decl);
	}

	private TNode EatUnexpectedTrailingSemicolon<TNode>(TNode decl) where TNode : CSharpSyntaxNode
	{
		if (base.CurrentToken.Kind == SyntaxKind.SemicolonToken)
		{
			SyntaxToken node = EatToken();
			node = AddError(node, ErrorCode.ERR_UnexpectedSemicolon);
			decl = AddTrailingSkippedSyntax(decl, node);
		}
		return decl;
	}

	private FieldDeclarationSyntax ParseNormalFieldDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, TypeSyntax type, SyntaxKind parentKind)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<VariableDeclaratorSyntax> variables = ParseFieldDeclarationVariableDeclarators(type, VariableFlags.LocalOrField, parentKind);
		if (modifiers != null)
		{
			int count = modifiers.Count;
			if (count >= 1 && modifiers[count - 1] is SyntaxToken { Kind: SyntaxKind.ScopedKeyword } syntaxToken)
			{
				type = _syntaxFactory.ScopedType(syntaxToken, type);
				modifiers.RemoveLast();
			}
		}
		return _syntaxFactory.FieldDeclaration(attributes, modifiers.ToList(), _syntaxFactory.VariableDeclaration(type, variables), EatToken(SyntaxKind.SemicolonToken));
	}

	private EventFieldDeclarationSyntax ParseEventFieldDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, SyntaxToken eventToken, TypeSyntax type, SyntaxKind parentKind)
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<VariableDeclaratorSyntax> variables = ParseFieldDeclarationVariableDeclarators(type, VariableFlags.None, parentKind);
		if (base.CurrentToken.Kind == SyntaxKind.DotToken)
		{
			eventToken = AddError(eventToken, ErrorCode.ERR_ExplicitEventFieldImpl);
		}
		return _syntaxFactory.EventFieldDeclaration(attributes, modifiers.ToList(), eventToken, _syntaxFactory.VariableDeclaration(type, variables), EatToken(SyntaxKind.SemicolonToken));
	}

	private bool IsEndOfFieldDeclaration()
	{
		return base.CurrentToken.Kind == SyntaxKind.SemicolonToken;
	}

	private Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<VariableDeclaratorSyntax> ParseFieldDeclarationVariableDeclarators(TypeSyntax type, VariableFlags flags, SyntaxKind parentKind)
	{
		int num;
		switch (parentKind)
		{
		case SyntaxKind.CompilationUnit:
			num = (base.IsScript ? 1 : 0);
			break;
		default:
			num = 1;
			break;
		case SyntaxKind.NamespaceDeclaration:
		case SyntaxKind.FileScopedNamespaceDeclaration:
			num = 0;
			break;
		}
		bool variableDeclarationsExpected = (byte)num != 0;
		SeparatedSyntaxListBuilder<VariableDeclaratorSyntax> item = _pool.AllocateSeparated<VariableDeclaratorSyntax>();
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfFieldDeclaration;
		ParseVariableDeclarators(type, flags, item, variableDeclarationsExpected, allowLocalFunctions: false, stopOnCloseParen: false, default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>), default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>), out var _);
		_termState = termState;
		return _pool.ToListAndFree(in item);
	}

	private void ParseVariableDeclarators(TypeSyntax type, VariableFlags flags, SeparatedSyntaxListBuilder<VariableDeclaratorSyntax> variables, bool variableDeclarationsExpected, bool allowLocalFunctions, bool stopOnCloseParen, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> mods, out LocalFunctionStatementSyntax localFunction)
	{
		variables.Add(ParseVariableDeclarator(type, flags, isFirst: true, allowLocalFunctions, attributes, mods, out localFunction));
		if (localFunction != null)
		{
			return;
		}
		while (base.CurrentToken.Kind != SyntaxKind.SemicolonToken && (!stopOnCloseParen || base.CurrentToken.Kind != SyntaxKind.CloseParenToken))
		{
			if (base.CurrentToken.Kind == SyntaxKind.CommaToken)
			{
				if (flags.HasFlag(VariableFlags.ForStatement) && PeekToken(1).Kind != SyntaxKind.SemicolonToken)
				{
					bool flag = IsTrueIdentifier(PeekToken(1));
					if (flag)
					{
						SyntaxKind kind = PeekToken(2).Kind;
						bool flag2 = ((kind == SyntaxKind.EqualsToken || kind == SyntaxKind.SemicolonToken || kind == SyntaxKind.CommaToken) ? true : false);
						flag = flag2;
					}
					if (!flag)
					{
						break;
					}
				}
				variables.AddSeparator(EatToken(SyntaxKind.CommaToken));
				variables.Add(ParseVariableDeclarator(type, flags, isFirst: false, allowLocalFunctions: false, attributes, mods, out localFunction));
			}
			else if (!variableDeclarationsExpected || SkipBadVariableListTokens(variables, SyntaxKind.CommaToken) == PostSkipAction.Abort)
			{
				break;
			}
		}
	}

	private PostSkipAction SkipBadVariableListTokens(SeparatedSyntaxListBuilder<VariableDeclaratorSyntax> list, SyntaxKind expected)
	{
		CSharpSyntaxNode startToken = null;
		return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken, (LanguageParser p, SyntaxKind _) => p.CurrentToken.Kind == SyntaxKind.SemicolonToken, expected);
	}

	private static SyntaxTokenList GetOriginalModifiers(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode decl)
	{
		if (decl != null)
		{
			switch (decl.Kind())
			{
			case SyntaxKind.FieldDeclaration:
				return ((Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax)decl).Modifiers;
			case SyntaxKind.MethodDeclaration:
				return ((Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)decl).Modifiers;
			case SyntaxKind.ConstructorDeclaration:
				return ((Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorDeclarationSyntax)decl).Modifiers;
			case SyntaxKind.DestructorDeclaration:
				return ((Microsoft.CodeAnalysis.CSharp.Syntax.DestructorDeclarationSyntax)decl).Modifiers;
			case SyntaxKind.PropertyDeclaration:
				return ((Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax)decl).Modifiers;
			case SyntaxKind.EventFieldDeclaration:
				return ((Microsoft.CodeAnalysis.CSharp.Syntax.EventFieldDeclarationSyntax)decl).Modifiers;
			case SyntaxKind.GetAccessorDeclaration:
			case SyntaxKind.SetAccessorDeclaration:
			case SyntaxKind.AddAccessorDeclaration:
			case SyntaxKind.RemoveAccessorDeclaration:
			case SyntaxKind.InitAccessorDeclaration:
				return ((Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax)decl).Modifiers;
			case SyntaxKind.ClassDeclaration:
			case SyntaxKind.StructDeclaration:
			case SyntaxKind.InterfaceDeclaration:
			case SyntaxKind.RecordDeclaration:
			case SyntaxKind.RecordStructDeclaration:
				return ((Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax)decl).Modifiers;
			case SyntaxKind.DelegateDeclaration:
				return ((Microsoft.CodeAnalysis.CSharp.Syntax.DelegateDeclarationSyntax)decl).Modifiers;
			}
		}
		return default(SyntaxTokenList);
	}

	private static bool WasFirstVariable(Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax variable)
	{
		if (GetOldParent(variable) is Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax variableDeclarationSyntax)
		{
			return variableDeclarationSyntax.Variables[0] == variable;
		}
		return false;
	}

	private static VariableFlags GetOriginalVariableFlags(Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax old)
	{
		Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode oldParent = GetOldParent(old);
		SyntaxTokenList originalModifiers = GetOriginalModifiers(oldParent);
		VariableFlags variableFlags = VariableFlags.None;
		if (originalModifiers.Any(SyntaxKind.FixedKeyword))
		{
			variableFlags |= VariableFlags.Fixed;
		}
		if (originalModifiers.Any(SyntaxKind.ConstKeyword))
		{
			variableFlags |= VariableFlags.Const;
		}
		if (oldParent != null && (oldParent.Kind() == SyntaxKind.VariableDeclaration || oldParent.Kind() == SyntaxKind.LocalDeclarationStatement))
		{
			variableFlags |= VariableFlags.LocalOrField;
		}
		return variableFlags;
	}

	private static bool CanReuseVariableDeclarator(Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax old, VariableFlags flags, bool isFirst)
	{
		if (old == null)
		{
			return false;
		}
		SyntaxKind syntaxKind;
		if (flags == GetOriginalVariableFlags(old) && isFirst == WasFirstVariable(old) && old.Initializer == null && (syntaxKind = GetOldParent(old).Kind()) != SyntaxKind.VariableDeclaration)
		{
			return syntaxKind != SyntaxKind.LocalDeclarationStatement;
		}
		return false;
	}

	private VariableDeclaratorSyntax ParseVariableDeclarator(TypeSyntax parentType, VariableFlags flags, bool isFirst, bool allowLocalFunctions, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> mods, out LocalFunctionStatementSyntax localFunction, bool isExpressionContext = false)
	{
		LanguageParser languageParser = this;
		if (IsIncrementalAndFactoryContextMatches && CanReuseVariableDeclarator(base.CurrentNode as Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax, flags, isFirst))
		{
			localFunction = null;
			return (VariableDeclaratorSyntax)EatNode();
		}
		if (!isExpressionContext)
		{
			using (GetDisposableResetPoint(resetOnDispose: true))
			{
				SyntaxKind kind = base.CurrentToken.Kind;
				if (kind == SyntaxKind.IdentifierToken && !parentType.IsMissing && parentType.GetLastToken().TrailingTrivia.Any(8539))
				{
					SyntaxToken syntaxToken = CreateMissingIdentifierToken();
					var (offset, length) = GetDiagnosticSpanForMissingNodeOrToken(syntaxToken);
					EatToken();
					kind = base.CurrentToken.Kind;
					bool flag = kind != SyntaxKind.EqualsToken && SyntaxFacts.IsBinaryExpressionOperatorToken(kind);
					bool flag2 = ((kind == SyntaxKind.OpenParenToken || kind == SyntaxKind.DotToken || kind == SyntaxKind.MinusGreaterThanToken) ? true : false);
					if (flag2 | flag)
					{
						flag2 = ((kind == SyntaxKind.OpenParenToken || kind == SyntaxKind.LessThanToken) ? true : false);
						if (!flag2 || !IsLocalFunctionAfterIdentifier())
						{
							syntaxToken = AddError(syntaxToken, offset, length, ErrorCode.ERR_IdentifierExpected);
							localFunction = null;
							return _syntaxFactory.VariableDeclarator(syntaxToken, null, null);
						}
					}
				}
			}
		}
		SyntaxToken name = ParseIdentifierToken();
		BracketedArgumentListSyntax bracketedArgumentListSyntax = null;
		EqualsValueClauseSyntax initializer = null;
		TerminatorState termState = _termState;
		bool flag3 = (flags & VariableFlags.Fixed) != 0;
		bool flag4 = (flags & VariableFlags.Const) != 0;
		bool flag5 = (flags & VariableFlags.LocalOrField) != 0;
		if (!isFirst && IsTrueIdentifier())
		{
			name = AddError(name, ErrorCode.ERR_MultiTypeInDeclaration);
		}
		SyntaxKind kind2 = base.CurrentToken.Kind;
		if (kind2 <= SyntaxKind.EqualsToken)
		{
			if (kind2 == SyntaxKind.OpenParenToken)
			{
				if (allowLocalFunctions & isFirst)
				{
					localFunction = TryParseLocalFunctionStatementBody(attributes, mods, parentType, name);
					if (localFunction != null)
					{
						return null;
					}
				}
				_termState |= TerminatorState.IsPossibleEndOfVariableDeclaration;
				bracketedArgumentListSyntax = ParseBracketedArgumentList();
				_termState = termState;
				bracketedArgumentListSyntax = AddError(bracketedArgumentListSyntax, ErrorCode.ERR_BadVarDecl);
				goto IL_0493;
			}
			if (kind2 == SyntaxKind.EqualsToken)
			{
				goto IL_01fb;
			}
		}
		else
		{
			if (kind2 == SyntaxKind.OpenBracketToken)
			{
				goto IL_02e4;
			}
			if (kind2 == SyntaxKind.LessThanToken && (allowLocalFunctions & isFirst))
			{
				localFunction = TryParseLocalFunctionStatementBody(attributes, mods, parentType, name);
				if (localFunction != null)
				{
					return null;
				}
			}
		}
		goto IL_040c;
		IL_02e4:
		_termState |= TerminatorState.IsPossibleEndOfVariableDeclaration;
		ArrayRankSpecifierSyntax arrayRankSpecifierSyntax = ParseArrayRankSpecifier(out var sawNonOmittedSize);
		_termState = termState;
		SyntaxToken openBracketToken = arrayRankSpecifierSyntax.OpenBracketToken;
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> sizes = arrayRankSpecifierSyntax.Sizes;
		SyntaxToken syntaxToken2 = arrayRankSpecifierSyntax.CloseBracketToken;
		if (flag3 && !sawNonOmittedSize)
		{
			syntaxToken2 = AddError(syntaxToken2, ErrorCode.ERR_ValueExpected);
		}
		SeparatedSyntaxListBuilder<ArgumentSyntax> item = _pool.AllocateSeparated<ArgumentSyntax>();
		foreach (GreenNode withSeparator in sizes.GetWithSeparators())
		{
			ExpressionSyntax expressionSyntax = withSeparator as ExpressionSyntax;
			if (expressionSyntax != null)
			{
				bool flag6 = expressionSyntax.Kind == SyntaxKind.OmittedArraySizeExpression;
				if (!flag3 && !flag6)
				{
					expressionSyntax = AddError(expressionSyntax, ErrorCode.ERR_ArraySizeInDeclaration);
				}
				item.Add(_syntaxFactory.Argument(null, null, expressionSyntax));
			}
			else
			{
				item.AddSeparator((SyntaxToken)withSeparator);
			}
		}
		bracketedArgumentListSyntax = _syntaxFactory.BracketedArgumentList(openBracketToken, _pool.ToListAndFree(in item), syntaxToken2);
		if (!flag3)
		{
			bracketedArgumentListSyntax = AddError(bracketedArgumentListSyntax, ErrorCode.ERR_CStyleArray);
			if (base.CurrentToken.Kind == SyntaxKind.EqualsToken)
			{
				goto IL_01fb;
			}
		}
		goto IL_0493;
		IL_0493:
		localFunction = null;
		return _syntaxFactory.VariableDeclarator(name, bracketedArgumentListSyntax, initializer);
		IL_01fb:
		if (flag3)
		{
			goto IL_040c;
		}
		SyntaxToken equalsToken = EatToken();
		SyntaxToken syntaxToken3 = ((flag5 && !flag4 && base.CurrentToken.Kind == SyntaxKind.RefKeyword && !IsPossibleLambdaExpression(Precedence.Expression)) ? EatToken() : null);
		ExpressionSyntax expressionSyntax2 = ParseVariableInitializer();
		initializer = _syntaxFactory.EqualsValueClause(equalsToken, (syntaxToken3 == null) ? expressionSyntax2 : _syntaxFactory.RefExpression(syntaxToken3, expressionSyntax2));
		goto IL_0493;
		IL_040c:
		if (looksLikeVariableInitializer())
		{
			localFunction = null;
			return _syntaxFactory.VariableDeclarator(name, null, _syntaxFactory.EqualsValueClause(EatToken(SyntaxKind.EqualsToken), ParseVariableInitializer()));
		}
		if (flag4)
		{
			name = AddError(name, ErrorCode.ERR_ConstValueRequired);
		}
		else if (flag3)
		{
			if (parentType.Kind != SyntaxKind.ArrayType)
			{
				goto IL_02e4;
			}
			name = AddError(name, ErrorCode.ERR_FixedDimsRequired);
		}
		goto IL_0493;
		bool looksLikeVariableInitializer()
		{
			if (base.CurrentToken.Kind == SyntaxKind.EqualsToken)
			{
				return false;
			}
			bool flag7 = base.CurrentToken.Kind == SyntaxKind.IdentifierToken;
			if (flag7)
			{
				bool flag8;
				switch (PeekToken(1).Kind)
				{
				case SyntaxKind.CloseParenToken:
				case SyntaxKind.EqualsToken:
				case SyntaxKind.SemicolonToken:
				case SyntaxKind.CommaToken:
				case SyntaxKind.EndOfFileToken:
				case SyntaxKind.IdentifierToken:
					flag8 = true;
					break;
				default:
					flag8 = false;
					break;
				}
				flag7 = flag8;
			}
			if (flag7)
			{
				return false;
			}
			if (ContainsErrorDiagnostic(name))
			{
				return false;
			}
			if (!CanStartExpression())
			{
				return false;
			}
			using (GetDisposableResetPoint(resetOnDispose: true))
			{
				ExpressionSyntax expressionSyntax3 = ParseExpressionCore();
				flag7 = !(expressionSyntax3 is TypeSyntax) && !ContainsErrorDiagnostic(expressionSyntax3);
			}
			return flag7;
		}
	}

	private bool IsLocalFunctionAfterIdentifier()
	{
		bool flag;
		using (GetDisposableResetPoint(resetOnDispose: true))
		{
			ParseTypeParameterList();
			flag = !ParseParenthesizedParameterList(forExtension: false).IsMissing;
			if (flag)
			{
				SyntaxKind kind = base.CurrentToken.Kind;
				bool flag2 = ((kind == SyntaxKind.OpenBraceToken || kind == SyntaxKind.EqualsGreaterThanToken) ? true : false);
				flag = flag2 || base.CurrentToken.ContextualKind == SyntaxKind.WhereKeyword;
			}
			flag = (flag ? true : false);
		}
		return flag;
	}

	private bool IsPossibleEndOfVariableDeclaration()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind == SyntaxKind.SemicolonToken || kind == SyntaxKind.CommaToken)
		{
			return true;
		}
		return false;
	}

	private ExpressionSyntax ParseVariableInitializer()
	{
		if (base.CurrentToken.Kind != SyntaxKind.OpenBraceToken)
		{
			return ParseExpressionCore();
		}
		return ParseArrayInitializer();
	}

	private bool IsPossibleVariableInitializer()
	{
		if (base.CurrentToken.Kind != SyntaxKind.OpenBraceToken)
		{
			return IsPossibleExpression();
		}
		return true;
	}

	private FieldDeclarationSyntax ParseConstantFieldDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, SyntaxListBuilder modifiers, SyntaxKind parentKind)
	{
		modifiers.Add(EatToken(SyntaxKind.ConstKeyword));
		TypeSyntax type = ParseType();
		return _syntaxFactory.FieldDeclaration(attributes, modifiers.ToList(), _syntaxFactory.VariableDeclaration(type, ParseFieldDeclarationVariableDeclarators(type, VariableFlags.Const, parentKind)), EatToken(SyntaxKind.SemicolonToken));
	}

	private DelegateDeclarationSyntax ParseDelegateDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, SyntaxListBuilder modifiers)
	{
		SyntaxToken delegateKeyword = EatToken(SyntaxKind.DelegateKeyword);
		TypeSyntax returnType = ParseReturnType();
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfMethodSignature;
		SyntaxToken identifier = ParseIdentifierToken();
		TypeParameterListSyntax typeParameterList = ParseTypeParameterList();
		ParameterListSyntax parameterList = ParseParenthesizedParameterList(forExtension: false);
		SyntaxListBuilder<TypeParameterConstraintClauseSyntax> syntaxListBuilder = default(SyntaxListBuilder<TypeParameterConstraintClauseSyntax>);
		if (base.CurrentToken.ContextualKind == SyntaxKind.WhereKeyword)
		{
			syntaxListBuilder = _pool.Allocate<TypeParameterConstraintClauseSyntax>();
			ParseTypeParameterConstraintClauses(syntaxListBuilder);
		}
		_termState = termState;
		return _syntaxFactory.DelegateDeclaration(attributes, modifiers.ToList(), delegateKeyword, returnType, identifier, typeParameterList, parameterList, _pool.ToListAndFree(syntaxListBuilder), EatToken(SyntaxKind.SemicolonToken));
	}

	private EnumDeclarationSyntax ParseEnumDeclaration(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, SyntaxListBuilder modifiers)
	{
		SyntaxToken enumKeyword = EatToken(SyntaxKind.EnumKeyword);
		SyntaxToken syntaxToken = ParseIdentifierToken();
		TypeParameterListSyntax typeParameterListSyntax = ParseTypeParameterList();
		if (typeParameterListSyntax != null)
		{
			syntaxToken = AddTrailingSkippedSyntax(syntaxToken, typeParameterListSyntax);
			syntaxToken = AddError(syntaxToken, ErrorCode.ERR_UnexpectedGenericName);
		}
		BaseListSyntax baseList = null;
		if (base.CurrentToken.Kind == SyntaxKind.ColonToken)
		{
			SyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
			TypeSyntax type = ParseType();
			SeparatedSyntaxListBuilder<BaseTypeSyntax> item = _pool.AllocateSeparated<BaseTypeSyntax>();
			item.Add(_syntaxFactory.SimpleBaseType(type));
			baseList = _syntaxFactory.BaseList(colonToken, _pool.ToListAndFree(in item));
		}
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<EnumMemberDeclarationSyntax> members = default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<EnumMemberDeclarationSyntax>);
		SyntaxToken semicolonToken;
		SyntaxToken openToken;
		SyntaxToken closeBraceToken;
		if (base.CurrentToken.Kind == SyntaxKind.SemicolonToken)
		{
			semicolonToken = EatToken(SyntaxKind.SemicolonToken);
			openToken = null;
			closeBraceToken = null;
		}
		else
		{
			openToken = EatToken(SyntaxKind.OpenBraceToken);
			if (!openToken.IsMissing)
			{
				members = ParseCommaSeparatedSyntaxList(ref openToken, SyntaxKind.CloseBraceToken, (LanguageParser @this) => @this.IsPossibleEnumMemberDeclaration(), (LanguageParser @this) => @this.ParseEnumMemberDeclaration(), skipBadEnumMemberListTokens, allowTrailingSeparator: true, requireOneElement: false, allowSemicolonAsSeparator: true);
			}
			closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
			semicolonToken = TryEatToken(SyntaxKind.SemicolonToken);
		}
		return _syntaxFactory.EnumDeclaration(attributes, modifiers.ToList(), enumKeyword, syntaxToken, baseList, openToken, members, closeBraceToken, semicolonToken);
		static PostSkipAction skipBadEnumMemberListTokens(LanguageParser @this, ref SyntaxToken openBrace, SeparatedSyntaxListBuilder<EnumMemberDeclarationSyntax> list, SyntaxKind expectedKind, SyntaxKind closeKind)
		{
			return @this.SkipBadSeparatedListTokensWithExpectedKind(ref openBrace, list, delegate(LanguageParser p)
			{
				SyntaxKind kind = p.CurrentToken.Kind;
				return kind != SyntaxKind.CommaToken && kind != SyntaxKind.SemicolonToken && !p.IsPossibleEnumMemberDeclaration();
			}, (LanguageParser p, SyntaxKind syntaxKind) => p.CurrentToken.Kind == syntaxKind, expectedKind, closeKind);
		}
	}

	private EnumMemberDeclarationSyntax ParseEnumMemberDeclaration()
	{
		if (IsIncrementalAndFactoryContextMatches && base.CurrentNodeKind == SyntaxKind.EnumMemberDeclaration)
		{
			return (EnumMemberDeclarationSyntax)EatNode();
		}
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = ParseAttributeDeclarations(inExpressionContext: false);
		SyntaxToken identifier = ParseIdentifierToken();
		EqualsValueClauseSyntax equalsValue = null;
		if (base.CurrentToken.Kind == SyntaxKind.EqualsToken)
		{
			ContextAwareSyntax syntaxFactory = _syntaxFactory;
			SyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
			SyntaxKind kind = base.CurrentToken.Kind;
			bool flag = ((kind == SyntaxKind.CloseBraceToken || kind == SyntaxKind.CommaToken) ? true : false);
			equalsValue = syntaxFactory.EqualsValueClause(equalsToken, flag ? ParseIdentifierName(ErrorCode.ERR_ConstantExpected) : ParseExpressionCore());
		}
		return _syntaxFactory.EnumMemberDeclaration(attributeLists, default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>), identifier, equalsValue);
	}

	private bool IsPossibleEnumMemberDeclaration()
	{
		if (base.CurrentToken.Kind != SyntaxKind.OpenBracketToken)
		{
			return IsTrueIdentifier();
		}
		return true;
	}

	private bool IsDotOrColonColon()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind == SyntaxKind.DotToken || kind == SyntaxKind.ColonColonToken)
		{
			return true;
		}
		return false;
	}

	public NameSyntax ParseName()
	{
		return ParseQualifiedName();
	}

	private IdentifierNameSyntax CreateMissingIdentifierName()
	{
		return _syntaxFactory.IdentifierName(CreateMissingIdentifierToken());
	}

	private static SyntaxToken CreateMissingIdentifierToken()
	{
		return SyntaxFactory.MissingToken(SyntaxKind.IdentifierToken);
	}

	private bool IsTrueIdentifier()
	{
		if (base.CurrentToken.Kind == SyntaxKind.IdentifierToken && !IsCurrentTokenPartialKeywordOfPartialMemberOrType() && !IsCurrentTokenQueryKeywordInQuery() && !IsCurrentTokenWhereOfConstraintClause())
		{
			return true;
		}
		return false;
	}

	private bool IsTrueIdentifier(SyntaxToken token)
	{
		if (token.Kind == SyntaxKind.IdentifierToken)
		{
			if (IsInQuery)
			{
				return !IsTokenQueryContextualKeyword(token);
			}
			return true;
		}
		return false;
	}

	private IdentifierNameSyntax ParseIdentifierName(ErrorCode code = ErrorCode.ERR_IdentifierExpected)
	{
		if (IsIncrementalAndFactoryContextMatches && base.CurrentNodeKind == SyntaxKind.IdentifierName && !SyntaxFacts.IsContextualKeyword(((Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax)base.CurrentNode).Identifier.Kind()))
		{
			return (IdentifierNameSyntax)EatNode();
		}
		return SyntaxFactory.IdentifierName(ParseIdentifierToken(code));
	}

	private SyntaxToken ParseIdentifierToken(ErrorCode code = ErrorCode.ERR_IdentifierExpected)
	{
		if (base.CurrentToken.Kind == SyntaxKind.IdentifierToken)
		{
			if (IsCurrentTokenPartialKeywordOfPartialMemberOrType() || IsCurrentTokenQueryKeywordInQuery())
			{
				SyntaxToken nodeOrToken = CreateMissingIdentifierToken();
				return AddError(nodeOrToken, ErrorCode.ERR_InvalidExprTerm, base.CurrentToken.Text);
			}
			SyntaxToken syntaxToken = EatToken();
			if (IsInAsync && syntaxToken.ContextualKind == SyntaxKind.AwaitKeyword)
			{
				syntaxToken = AddError(syntaxToken, ErrorCode.ERR_BadAwaitAsIdentifier);
			}
			return syntaxToken;
		}
		return AddError(CreateMissingIdentifierToken(), code);
	}

	private bool IsCurrentTokenQueryKeywordInQuery()
	{
		if (IsInQuery)
		{
			return IsCurrentTokenQueryContextualKeyword;
		}
		return false;
	}

	private bool IsCurrentTokenPartialKeywordOfPartialMemberOrType()
	{
		if (base.CurrentToken.ContextualKind == SyntaxKind.PartialKeyword && (IsPartialType() || IsPartialMember()))
		{
			return true;
		}
		return false;
	}

	private bool IsCurrentTokenFieldInKeywordContext()
	{
		if (base.CurrentToken.ContextualKind == SyntaxKind.FieldKeyword && IsInFieldKeywordContext)
		{
			return IsFeatureEnabled(MessageID.IDS_FeatureFieldKeyword);
		}
		return false;
	}

	private TypeParameterListSyntax ParseTypeParameterList()
	{
		if (base.CurrentToken.Kind != SyntaxKind.LessThanToken)
		{
			return null;
		}
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfTypeParameterList;
		SyntaxToken openToken = EatToken(SyntaxKind.LessThanToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TypeParameterSyntax> parameters = ParseCommaSeparatedSyntaxList(ref openToken, SyntaxKind.GreaterThanToken, (LanguageParser @this) => @this.IsStartOfTypeParameter(), (LanguageParser @this) => @this.ParseTypeParameter(), skipBadTypeParameterListTokens, allowTrailingSeparator: false, requireOneElement: true, allowSemicolonAsSeparator: false);
		_termState = termState;
		return _syntaxFactory.TypeParameterList(openToken, parameters, EatToken(SyntaxKind.GreaterThanToken));
		static PostSkipAction skipBadTypeParameterListTokens(LanguageParser @this, ref SyntaxToken open, SeparatedSyntaxListBuilder<TypeParameterSyntax> list, SyntaxKind expectedKind, SyntaxKind closeKind)
		{
			return @this.SkipBadSeparatedListTokensWithExpectedKind(ref open, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken, (LanguageParser p, SyntaxKind syntaxKind) => p.CurrentToken.Kind == syntaxKind, expectedKind, closeKind);
		}
	}

	private bool IsStartOfTypeParameter()
	{
		if (IsCurrentTokenWhereOfConstraintClause())
		{
			return false;
		}
		SyntaxKind kind = base.CurrentToken.Kind;
		if ((kind == SyntaxKind.OpenBracketToken || kind - 8361 <= SyntaxKind.List) ? true : false)
		{
			return true;
		}
		return IsTrueIdentifier();
	}

	private TypeParameterSyntax ParseTypeParameter()
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> syntaxList = default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>);
		if (base.CurrentToken.Kind == SyntaxKind.OpenBracketToken)
		{
			TerminatorState termState = _termState;
			_termState = TerminatorState.IsEndOfTypeArgumentList;
			syntaxList = ParseAttributeDeclarations(inExpressionContext: false);
			_termState = termState;
		}
		if (IsCurrentTokenWhereOfConstraintClause() || IsCurrentTokenPartialKeywordOfPartialMemberOrType())
		{
			return _syntaxFactory.TypeParameter(syntaxList, null, AddError(CreateMissingIdentifierToken(), ErrorCode.ERR_IdentifierExpected));
		}
		ContextAwareSyntax syntaxFactory = _syntaxFactory;
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = syntaxList;
		SyntaxKind kind = base.CurrentToken.Kind;
		bool flag = kind - 8361 <= SyntaxKind.List;
		return syntaxFactory.TypeParameter(attributeLists, flag ? EatToken() : null, ParseIdentifierToken());
	}

	private SimpleNameSyntax ParseSimpleName(NameOptions options = NameOptions.None)
	{
		IdentifierNameSyntax identifierNameSyntax = ParseIdentifierName();
		if (identifierNameSyntax.Identifier.IsMissing)
		{
			return identifierNameSyntax;
		}
		SimpleNameSyntax result = identifierNameSyntax;
		if (base.CurrentToken.Kind == SyntaxKind.LessThanToken)
		{
			ScanTypeArgumentListKind scanTypeArgumentListKind;
			using (GetDisposableResetPoint(resetOnDispose: true))
			{
				scanTypeArgumentListKind = ScanTypeArgumentList(options);
			}
			if (scanTypeArgumentListKind == ScanTypeArgumentListKind.DefiniteTypeArgumentList || (scanTypeArgumentListKind == ScanTypeArgumentListKind.PossibleTypeArgumentList && (options & NameOptions.InTypeList) != NameOptions.None))
			{
				SeparatedSyntaxListBuilder<TypeSyntax> item = _pool.AllocateSeparated<TypeSyntax>();
				ParseTypeArgumentList(out var open, item, out var close);
				result = _syntaxFactory.GenericName(identifierNameSyntax.Identifier, _syntaxFactory.TypeArgumentList(open, _pool.ToListAndFree(in item), close));
			}
		}
		return result;
	}

	private ScanTypeArgumentListKind ScanTypeArgumentList(NameOptions options)
	{
		if (base.CurrentToken.Kind != SyntaxKind.LessThanToken)
		{
			return ScanTypeArgumentListKind.NotTypeArgumentList;
		}
		if ((options & NameOptions.InExpression) == 0)
		{
			return ScanTypeArgumentListKind.DefiniteTypeArgumentList;
		}
		if (ScanPossibleTypeArgumentList(out var _, out var isDefinitelyTypeArgumentList) == ScanTypeFlags.NotType)
		{
			return ScanTypeArgumentListKind.NotTypeArgumentList;
		}
		if (isDefinitelyTypeArgumentList)
		{
			return ScanTypeArgumentListKind.DefiniteTypeArgumentList;
		}
		switch (base.CurrentToken.Kind)
		{
		case SyntaxKind.CaretToken:
		case SyntaxKind.OpenParenToken:
		case SyntaxKind.CloseParenToken:
		case SyntaxKind.CloseBraceToken:
		case SyntaxKind.CloseBracketToken:
		case SyntaxKind.BarToken:
		case SyntaxKind.ColonToken:
		case SyntaxKind.SemicolonToken:
		case SyntaxKind.CommaToken:
		case SyntaxKind.DotToken:
		case SyntaxKind.QuestionToken:
		case SyntaxKind.ExclamationEqualsToken:
		case SyntaxKind.EqualsEqualsToken:
			return ScanTypeArgumentListKind.DefiniteTypeArgumentList;
		case SyntaxKind.AmpersandToken:
		case SyntaxKind.OpenBracketToken:
		case SyntaxKind.LessThanToken:
		case SyntaxKind.BarBarToken:
		case SyntaxKind.AmpersandAmpersandToken:
		case SyntaxKind.LessThanEqualsToken:
		case SyntaxKind.GreaterThanEqualsToken:
		case SyntaxKind.IsKeyword:
		case SyntaxKind.AsKeyword:
			return ScanTypeArgumentListKind.DefiniteTypeArgumentList;
		case SyntaxKind.OpenBraceToken:
			return ScanTypeArgumentListKind.DefiniteTypeArgumentList;
		case SyntaxKind.GreaterThanToken:
			if ((options & NameOptions.AfterIs) != NameOptions.None && PeekToken(1).Kind != SyntaxKind.GreaterThanToken)
			{
				return ScanTypeArgumentListKind.DefiniteTypeArgumentList;
			}
			break;
		case SyntaxKind.IdentifierToken:
		{
			bool flag = (options & (NameOptions.AfterIs | NameOptions.DefinitePattern | NameOptions.AfterOut)) != 0;
			if (!flag)
			{
				bool flag2 = (options & NameOptions.AfterTupleComma) != 0;
				if (flag2)
				{
					SyntaxKind kind = PeekToken(1).Kind;
					bool flag3 = ((kind == SyntaxKind.CloseParenToken || kind == SyntaxKind.CommaToken) ? true : false);
					flag2 = flag3;
				}
				flag = flag2;
			}
			if (flag || ((options & NameOptions.FirstElementOfPossibleTupleLiteral) != NameOptions.None && PeekToken(1).Kind == SyntaxKind.CommaToken))
			{
				return ScanTypeArgumentListKind.DefiniteTypeArgumentList;
			}
			return ScanTypeArgumentListKind.PossibleTypeArgumentList;
		}
		case SyntaxKind.EndOfFileToken:
			return ScanTypeArgumentListKind.DefiniteTypeArgumentList;
		case SyntaxKind.EqualsGreaterThanToken:
			return ScanTypeArgumentListKind.DefiniteTypeArgumentList;
		}
		return ScanTypeArgumentListKind.PossibleTypeArgumentList;
	}

	private ScanTypeFlags ScanPossibleTypeArgumentList(out SyntaxToken greaterThanToken, out bool isDefinitelyTypeArgumentList)
	{
		isDefinitelyTypeArgumentList = false;
		if (IsOpenName())
		{
			isDefinitelyTypeArgumentList = true;
			EatToken();
			while (base.CurrentToken.Kind == SyntaxKind.CommaToken)
			{
				EatToken();
			}
			greaterThanToken = EatToken();
			return ScanTypeFlags.GenericTypeOrMethod;
		}
		ScanTypeFlags result = ScanTypeFlags.GenericTypeOrExpression;
		ScanTypeFlags scanTypeFlags;
		do
		{
			EatToken();
			if (base.CurrentToken.Kind == SyntaxKind.OpenBracketToken)
			{
				greaterThanToken = null;
				return ScanTypeFlags.NotType;
			}
			if (base.CurrentToken.Kind == SyntaxKind.GreaterThanToken)
			{
				greaterThanToken = EatToken();
				return result;
			}
			if (base.CurrentToken.Kind == SyntaxKind.CommaToken)
			{
				scanTypeFlags = ScanTypeFlags.NotType;
				continue;
			}
			scanTypeFlags = ScanType(out var _);
			switch (scanTypeFlags)
			{
			case ScanTypeFlags.NotType:
				greaterThanToken = null;
				return ScanTypeFlags.NotType;
			case ScanTypeFlags.MustBeType:
			{
				bool flag = isDefinitelyTypeArgumentList;
				if (!flag)
				{
					SyntaxKind kind = base.CurrentToken.Kind;
					bool flag2 = kind - 8216 <= SyntaxKind.List;
					flag = flag2;
				}
				isDefinitelyTypeArgumentList = flag;
				result = ScanTypeFlags.GenericTypeOrMethod;
				break;
			}
			case ScanTypeFlags.NullableType:
			{
				bool flag = isDefinitelyTypeArgumentList;
				if (!flag)
				{
					SyntaxKind kind = base.CurrentToken.Kind;
					bool flag2 = kind - 8216 <= SyntaxKind.List;
					flag = flag2;
				}
				isDefinitelyTypeArgumentList = flag;
				if (isDefinitelyTypeArgumentList)
				{
					result = ScanTypeFlags.GenericTypeOrMethod;
				}
				break;
			}
			case ScanTypeFlags.GenericTypeOrExpression:
				if (!isDefinitelyTypeArgumentList)
				{
					isDefinitelyTypeArgumentList = base.CurrentToken.Kind == SyntaxKind.CommaToken;
					result = ScanTypeFlags.GenericTypeOrMethod;
				}
				break;
			case ScanTypeFlags.GenericTypeOrMethod:
				result = ScanTypeFlags.GenericTypeOrMethod;
				break;
			}
		}
		while (base.CurrentToken.Kind == SyntaxKind.CommaToken);
		if (base.CurrentToken.Kind != SyntaxKind.GreaterThanToken)
		{
			if (base.CurrentToken.Kind == SyntaxKind.IdentifierToken)
			{
				greaterThanToken = EatToken(SyntaxKind.GreaterThanToken);
				return result;
			}
			if (scanTypeFlags == ScanTypeFlags.TupleType && base.CurrentToken.Kind == SyntaxKind.OpenParenToken)
			{
				greaterThanToken = EatToken(SyntaxKind.GreaterThanToken);
				return result;
			}
			greaterThanToken = null;
			return ScanTypeFlags.NotType;
		}
		greaterThanToken = EatToken();
		isDefinitelyTypeArgumentList = isDefinitelyTypeArgumentList || base.CurrentToken.Kind == SyntaxKind.CloseParenToken;
		if (isDefinitelyTypeArgumentList)
		{
			result = ScanTypeFlags.GenericTypeOrMethod;
		}
		return result;
	}

	private void ParseTypeArgumentList(out SyntaxToken open, SeparatedSyntaxListBuilder<TypeSyntax> types, out SyntaxToken close)
	{
		bool num = IsOpenName();
		open = EatToken(SyntaxKind.LessThanToken);
		open = CheckFeatureAvailability(open, MessageID.IDS_FeatureGenerics);
		if (num)
		{
			OmittedTypeArgumentSyntax node = _syntaxFactory.OmittedTypeArgument(SyntaxFactory.Token(SyntaxKind.OmittedTypeArgumentToken));
			types.Add(node);
			while (base.CurrentToken.Kind == SyntaxKind.CommaToken)
			{
				types.AddSeparator(EatToken(SyntaxKind.CommaToken));
				types.Add(node);
			}
			close = EatToken(SyntaxKind.GreaterThanToken);
			return;
		}
		types.Add(ParseTypeArgument());
		while (base.CurrentToken.Kind != SyntaxKind.GreaterThanToken && !tokenBreaksTypeArgumentList(base.CurrentToken) && (base.CurrentToken.Kind != SyntaxKind.IdentifierToken || !tokenBreaksTypeArgumentList(PeekToken(1))) && (base.CurrentToken.Kind != SyntaxKind.IdentifierToken || PeekToken(1).Kind != SyntaxKind.CloseBracketToken))
		{
			if (base.CurrentToken.Kind == SyntaxKind.CommaToken || IsPossibleType())
			{
				types.AddSeparator(EatToken(SyntaxKind.CommaToken));
				types.Add(ParseTypeArgument());
			}
			else if (SkipBadTypeArgumentListTokens(types, SyntaxKind.CommaToken) == PostSkipAction.Abort)
			{
				break;
			}
		}
		close = EatToken(SyntaxKind.GreaterThanToken);
		static bool tokenBreaksTypeArgumentList(SyntaxToken token)
		{
			SyntaxKind contextualKeywordKind = SyntaxFacts.GetContextualKeywordKind(token.ValueText);
			if (contextualKeywordKind - 8438 <= SyntaxKind.List)
			{
				return true;
			}
			switch (token.Kind)
			{
			case SyntaxKind.OpenParenToken:
			case SyntaxKind.CloseParenToken:
			case SyntaxKind.EqualsToken:
			case SyntaxKind.OpenBraceToken:
			case SyntaxKind.CloseBraceToken:
			case SyntaxKind.SemicolonToken:
			case SyntaxKind.LessThanToken:
			case SyntaxKind.EqualsGreaterThanToken:
			case SyntaxKind.ThisKeyword:
			case SyntaxKind.OperatorKeyword:
				return true;
			default:
				return false;
			}
		}
	}

	private PostSkipAction SkipBadTypeArgumentListTokens(SeparatedSyntaxListBuilder<TypeSyntax> list, SyntaxKind expected)
	{
		CSharpSyntaxNode startToken = null;
		return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleType(), (LanguageParser p, SyntaxKind _) => p.CurrentToken.Kind == SyntaxKind.GreaterThanToken, expected);
	}

	private TypeSyntax ParseTypeArgument()
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> syntaxList = default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>);
		if (base.CurrentToken.Kind == SyntaxKind.OpenBracketToken && PeekToken(1).Kind != SyntaxKind.CloseBracketToken)
		{
			TerminatorState termState = _termState;
			_termState = TerminatorState.IsEndOfTypeArgumentList;
			syntaxList = ParseAttributeDeclarations(inExpressionContext: false);
			_termState = termState;
		}
		SyntaxKind kind = base.CurrentToken.Kind;
		bool flag = kind - 8361 <= SyntaxKind.List;
		SyntaxToken syntaxToken = (flag ? AddError(EatToken(), ErrorCode.ERR_IllegalVarianceSyntax) : null);
		TypeSyntax typeSyntax = ParseType();
		int num;
		if (typeSyntax.IsMissing)
		{
			kind = base.CurrentToken.Kind;
			num = ((kind != SyntaxKind.CommaToken && kind != SyntaxKind.GreaterThanToken) ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		flag = (byte)num != 0;
		if (flag)
		{
			kind = PeekToken(1).Kind;
			bool flag2 = kind - 8216 <= SyntaxKind.List;
			flag = flag2;
		}
		if (flag)
		{
			typeSyntax = AddTrailingSkippedSyntax(typeSyntax, EatToken());
		}
		if (syntaxToken != null)
		{
			typeSyntax = AddLeadingSkippedSyntax(typeSyntax, syntaxToken);
		}
		if (syntaxList.Count > 0)
		{
			typeSyntax = AddLeadingSkippedSyntax(typeSyntax, syntaxList.Node);
			typeSyntax = AddError(typeSyntax, ErrorCode.ERR_TypeExpected);
		}
		return typeSyntax;
	}

	private bool IsEndOfTypeArgumentList()
	{
		return base.CurrentToken.Kind == SyntaxKind.GreaterThanToken;
	}

	private bool IsOpenName()
	{
		int i;
		for (i = 1; PeekToken(i).Kind == SyntaxKind.CommaToken; i++)
		{
		}
		return PeekToken(i).Kind == SyntaxKind.GreaterThanToken;
	}

	private void ParseMemberName(out ExplicitInterfaceSpecifierSyntax explicitInterfaceOpt, out SyntaxToken identifierOrThisOpt, out TypeParameterListSyntax typeParameterListOpt, bool isEvent)
	{
		identifierOrThisOpt = null;
		explicitInterfaceOpt = null;
		typeParameterListOpt = null;
		if (!IsPossibleMemberName())
		{
			return;
		}
		NameSyntax explicitInterfaceName = null;
		SyntaxToken separator = null;
		ResetPoint state = default(ResetPoint);
		bool flag = false;
		try
		{
			while (true)
			{
				if (base.CurrentToken.Kind == SyntaxKind.ThisKeyword)
				{
					state = GetResetPoint();
					flag = true;
					identifierOrThisOpt = EatToken();
					typeParameterListOpt = ParseTypeParameterList();
					break;
				}
				bool flag2;
				using (GetDisposableResetPoint(resetOnDispose: true))
				{
					ScanNamedTypePart();
					flag2 = !IsDotOrColonColon();
				}
				if (flag2)
				{
					state = GetResetPoint();
					flag = true;
					if (separator != null && separator.Kind == SyntaxKind.ColonColonToken)
					{
						separator = AddError(separator, ErrorCode.ERR_AliasQualAsExpression);
						separator = ConvertToMissingWithTrailingTrivia(separator, SyntaxKind.DotToken);
					}
					identifierOrThisOpt = ParseIdentifierToken();
					typeParameterListOpt = ParseTypeParameterList();
					break;
				}
				AccumulateExplicitInterfaceName(ref explicitInterfaceName, ref separator);
			}
			if (explicitInterfaceName == null)
			{
				return;
			}
			if (separator.Kind != SyntaxKind.DotToken)
			{
				separator = WithAdditionalDiagnostics(separator, GetExpectedTokenError(SyntaxKind.DotToken, separator.Kind, separator.GetLeadingTriviaWidth(), separator.Width));
				separator = ConvertToMissingWithTrailingTrivia(separator, SyntaxKind.DotToken);
			}
			if (isEvent)
			{
				SyntaxKind kind = base.CurrentToken.Kind;
				if (kind != SyntaxKind.OpenBraceToken && kind != SyntaxKind.SemicolonToken)
				{
					explicitInterfaceOpt = _syntaxFactory.ExplicitInterfaceSpecifier(explicitInterfaceName, AddError(separator, ErrorCode.ERR_ExplicitEventFieldImpl));
					if (separator.TrailingTrivia.Any(8539))
					{
						Reset(ref state);
						identifierOrThisOpt = null;
						typeParameterListOpt = null;
					}
					return;
				}
			}
			explicitInterfaceOpt = _syntaxFactory.ExplicitInterfaceSpecifier(explicitInterfaceName, separator);
		}
		finally
		{
			if (flag)
			{
				Release(ref state);
			}
		}
	}

	private void AccumulateExplicitInterfaceName(ref NameSyntax explicitInterfaceName, ref SyntaxToken separator)
	{
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfNameInExplicitInterface;
		if (explicitInterfaceName == null)
		{
			explicitInterfaceName = ParseSimpleName(NameOptions.InTypeList);
			separator = ((base.CurrentToken.Kind == SyntaxKind.ColonColonToken) ? EatToken() : EatToken(SyntaxKind.DotToken));
		}
		else
		{
			NameSyntax nameSyntax = ParseQualifiedNameRight(NameOptions.InTypeList, explicitInterfaceName, separator);
			explicitInterfaceName = nameSyntax;
			if (base.CurrentToken.Kind == SyntaxKind.ColonColonToken)
			{
				separator = EatToken();
				separator = AddError(separator, ErrorCode.ERR_UnexpectedAliasedName);
				separator = ConvertToMissingWithTrailingTrivia(separator, SyntaxKind.DotToken);
			}
			else
			{
				separator = EatToken(SyntaxKind.DotToken);
			}
		}
		_termState = termState;
	}

	private bool IsOperatorStart(out ExplicitInterfaceSpecifierSyntax explicitInterfaceOpt, bool advanceParser = true)
	{
		explicitInterfaceOpt = null;
		if (IsOperatorKeyword())
		{
			return true;
		}
		if (base.CurrentToken.Kind != SyntaxKind.IdentifierToken)
		{
			return false;
		}
		NameSyntax explicitInterfaceName = null;
		SyntaxToken separator = null;
		using DisposableResetPoint disposableResetPoint = GetDisposableResetPoint(resetOnDispose: false);
		while (true)
		{
			bool flag;
			using (GetDisposableResetPoint(resetOnDispose: true))
			{
				if (IsOperatorKeyword())
				{
					flag = false;
				}
				else
				{
					ScanNamedTypePart();
					flag = IsDotOrColonColon() || IsOperatorKeyword();
				}
			}
			if (!flag)
			{
				break;
			}
			AccumulateExplicitInterfaceName(ref explicitInterfaceName, ref separator);
		}
		if (separator != null && separator.Kind == SyntaxKind.ColonColonToken)
		{
			separator = AddError(separator, ErrorCode.ERR_AliasQualAsExpression);
			separator = ConvertToMissingWithTrailingTrivia(separator, SyntaxKind.DotToken);
		}
		if (!IsOperatorKeyword() || explicitInterfaceName == null)
		{
			disposableResetPoint.Reset();
			return false;
		}
		if (!advanceParser)
		{
			disposableResetPoint.Reset();
			return true;
		}
		if (separator.Kind != SyntaxKind.DotToken)
		{
			separator = WithAdditionalDiagnostics(separator, GetExpectedTokenError(SyntaxKind.DotToken, separator.Kind, separator.GetLeadingTriviaWidth(), separator.Width));
			separator = ConvertToMissingWithTrailingTrivia(separator, SyntaxKind.DotToken);
		}
		explicitInterfaceOpt = _syntaxFactory.ExplicitInterfaceSpecifier(explicitInterfaceName, separator);
		return true;
	}

	private NameSyntax ParseAliasQualifiedName(NameOptions allowedParts = NameOptions.None)
	{
		SimpleNameSyntax simpleNameSyntax = ParseSimpleName(allowedParts);
		if (base.CurrentToken.Kind != SyntaxKind.ColonColonToken)
		{
			return simpleNameSyntax;
		}
		return ParseQualifiedNameRight(allowedParts, simpleNameSyntax, EatToken());
	}

	private NameSyntax ParseQualifiedName(NameOptions options = NameOptions.None)
	{
		NameSyntax nameSyntax = ParseAliasQualifiedName(options);
		while (IsDotOrColonColon() && PeekToken(1).Kind != SyntaxKind.ThisKeyword)
		{
			SyntaxToken separator = EatToken();
			nameSyntax = ParseQualifiedNameRight(options, nameSyntax, separator);
		}
		return nameSyntax;
	}

	private NameSyntax ParseQualifiedNameRight(NameOptions options, NameSyntax left, SyntaxToken separator)
	{
		SimpleNameSyntax simpleNameSyntax = ParseSimpleName(options);
		switch (separator.Kind)
		{
		case SyntaxKind.DotToken:
			return _syntaxFactory.QualifiedName(left, separator, simpleNameSyntax);
		case SyntaxKind.ColonColonToken:
		{
			if (left.Kind != SyntaxKind.IdentifierName)
			{
				separator = AddError(separator, ErrorCode.ERR_UnexpectedAliasedName);
			}
			IdentifierNameSyntax identifierNameSyntax = left as IdentifierNameSyntax;
			if (identifierNameSyntax == null)
			{
				separator = ConvertToMissingWithTrailingTrivia(separator, SyntaxKind.DotToken);
				return _syntaxFactory.QualifiedName(left, separator, simpleNameSyntax);
			}
			if (identifierNameSyntax.Identifier.ContextualKind == SyntaxKind.GlobalKeyword)
			{
				identifierNameSyntax = _syntaxFactory.IdentifierName(SyntaxParser.ConvertToKeyword(identifierNameSyntax.Identifier));
			}
			return WithAdditionalDiagnostics(_syntaxFactory.AliasQualifiedName(identifierNameSyntax, separator, simpleNameSyntax), left.GetDiagnostics());
		}
		default:
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Parser/LanguageParser.cs", 7061);
		}
	}

	private SyntaxToken ConvertToMissingWithTrailingTrivia(SyntaxToken token, SyntaxKind expectedKind)
	{
		SyntaxToken node = SyntaxFactory.MissingToken(expectedKind);
		return AddTrailingSkippedSyntax(node, token);
	}

	private bool IsPossibleType()
	{
		if (!IsPredefinedType(base.CurrentToken.Kind))
		{
			return IsTrueIdentifier();
		}
		return true;
	}

	private ScanTypeFlags ScanType(bool forPattern = false)
	{
		SyntaxToken lastTokenOfType;
		return ScanType(out lastTokenOfType, forPattern);
	}

	private ScanTypeFlags ScanType(out SyntaxToken lastTokenOfType, bool forPattern = false)
	{
		return ScanType(forPattern ? ParseTypeMode.DefinitePattern : ParseTypeMode.Normal, out lastTokenOfType);
	}

	private void ScanNamedTypePart()
	{
		ScanNamedTypePart(out var _);
	}

	private ScanTypeFlags ScanNamedTypePart(out SyntaxToken lastTokenOfType)
	{
		if (base.CurrentToken.Kind != SyntaxKind.IdentifierToken || !IsTrueIdentifier())
		{
			lastTokenOfType = null;
			return ScanTypeFlags.NotType;
		}
		lastTokenOfType = EatToken();
		bool isDefinitelyTypeArgumentList;
		if (base.CurrentToken.Kind == SyntaxKind.LessThanToken)
		{
			return ScanPossibleTypeArgumentList(out lastTokenOfType, out isDefinitelyTypeArgumentList);
		}
		return ScanTypeFlags.NonGenericTypeOrExpression;
	}

	private ScanTypeFlags ScanType(ParseTypeMode mode, out SyntaxToken lastTokenOfType)
	{
		if (base.CurrentToken.Kind == SyntaxKind.RefKeyword)
		{
			EatToken();
			if (base.CurrentToken.Kind == SyntaxKind.ReadOnlyKeyword)
			{
				EatToken();
			}
		}
		SyntaxKind kind = base.CurrentToken.Kind;
		ScanTypeFlags scanTypeFlags;
		if ((kind == SyntaxKind.ColonColonToken || kind == SyntaxKind.IdentifierToken) ? true : false)
		{
			bool flag;
			if (base.CurrentToken.Kind == SyntaxKind.ColonColonToken)
			{
				scanTypeFlags = ScanTypeFlags.NonGenericTypeOrExpression;
				flag = true;
				lastTokenOfType = null;
			}
			else
			{
				flag = PeekToken(1).Kind == SyntaxKind.ColonColonToken;
				scanTypeFlags = ScanNamedTypePart(out lastTokenOfType);
				if (scanTypeFlags == ScanTypeFlags.NotType)
				{
					return ScanTypeFlags.NotType;
				}
			}
			bool flag2 = true;
			while (IsDotOrColonColon())
			{
				if (!flag2)
				{
					flag = false;
				}
				EatToken();
				scanTypeFlags = ScanNamedTypePart(out lastTokenOfType);
				if (scanTypeFlags == ScanTypeFlags.NotType)
				{
					return ScanTypeFlags.NotType;
				}
				flag2 = false;
			}
			if (flag)
			{
				scanTypeFlags = ScanTypeFlags.AliasQualifiedName;
			}
		}
		else if (IsPredefinedType(base.CurrentToken.Kind))
		{
			lastTokenOfType = EatToken();
			scanTypeFlags = ScanTypeFlags.MustBeType;
		}
		else if (base.CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			lastTokenOfType = EatToken();
			scanTypeFlags = ScanTupleType(out lastTokenOfType);
			if (scanTypeFlags == ScanTypeFlags.NotType || (mode == ParseTypeMode.DefinitePattern && base.CurrentToken.Kind != SyntaxKind.OpenBracketToken))
			{
				return ScanTypeFlags.NotType;
			}
		}
		else
		{
			if (!IsFunctionPointerStart())
			{
				lastTokenOfType = null;
				return ScanTypeFlags.NotType;
			}
			scanTypeFlags = ScanFunctionPointerType(out lastTokenOfType);
		}
		int lastTokenPosition = -1;
		while (IsMakingProgress(ref lastTokenPosition))
		{
			switch (base.CurrentToken.Kind)
			{
			case SyntaxKind.QuestionToken:
			{
				SyntaxKind kind2 = lastTokenOfType.Kind;
				if (kind2 != SyntaxKind.QuestionToken && kind2 != SyntaxKind.AsteriskToken)
				{
					lastTokenOfType = EatToken();
					scanTypeFlags = ScanTypeFlags.NullableType;
					continue;
				}
				break;
			}
			case SyntaxKind.AsteriskToken:
				if (mode != ParseTypeMode.DefinitePattern && ((mode != ParseTypeMode.AfterTupleComma && mode != ParseTypeMode.FirstElementOfPossibleTupleLiteral) || PointerTypeModsFollowedByRankAndDimensionSpecifier()))
				{
					lastTokenOfType = EatToken();
					if ((uint)(scanTypeFlags - 3) <= 1u)
					{
						scanTypeFlags = ScanTypeFlags.PointerOrMultiplication;
					}
					else if (scanTypeFlags == ScanTypeFlags.GenericTypeOrMethod)
					{
						scanTypeFlags = ScanTypeFlags.MustBeType;
					}
					continue;
				}
				break;
			case SyntaxKind.OpenBracketToken:
				EatToken();
				while (base.CurrentToken.Kind == SyntaxKind.CommaToken)
				{
					EatToken();
				}
				if (base.CurrentToken.Kind != SyntaxKind.CloseBracketToken)
				{
					lastTokenOfType = null;
					return ScanTypeFlags.NotType;
				}
				lastTokenOfType = EatToken();
				scanTypeFlags = ScanTypeFlags.MustBeType;
				continue;
			}
			break;
		}
		return scanTypeFlags;
	}

	private ScanTypeFlags ScanTupleType(out SyntaxToken lastTokenOfType)
	{
		if (ScanType(out lastTokenOfType) != ScanTypeFlags.NotType)
		{
			if (IsTrueIdentifier())
			{
				lastTokenOfType = EatToken();
			}
			if (base.CurrentToken.Kind == SyntaxKind.CommaToken)
			{
				do
				{
					lastTokenOfType = EatToken();
					if (ScanType(out lastTokenOfType) == ScanTypeFlags.NotType)
					{
						lastTokenOfType = EatToken();
						return ScanTypeFlags.NotType;
					}
					if (IsTrueIdentifier())
					{
						lastTokenOfType = EatToken();
					}
				}
				while (base.CurrentToken.Kind == SyntaxKind.CommaToken);
				if (base.CurrentToken.Kind == SyntaxKind.CloseParenToken)
				{
					lastTokenOfType = EatToken();
					return ScanTypeFlags.TupleType;
				}
			}
		}
		lastTokenOfType = null;
		return ScanTypeFlags.NotType;
	}

	private ScanTypeFlags ScanFunctionPointerType(out SyntaxToken lastTokenOfType)
	{
		EatToken(SyntaxKind.DelegateKeyword);
		lastTokenOfType = EatToken(SyntaxKind.AsteriskToken);
		SyntaxToken lastTokenOfType2;
		if (base.CurrentToken.Kind == SyntaxKind.IdentifierToken)
		{
			SyntaxToken syntaxToken = PeekToken(1);
			lastTokenOfType2 = base.CurrentToken;
			if (lastTokenOfType2 != null)
			{
				SyntaxKind contextualKind = lastTokenOfType2.ContextualKind;
				if (contextualKind - 8445 <= SyntaxKind.List)
				{
					goto IL_006b;
				}
			}
			if (!IsPossibleFunctionPointerParameterListStart(syntaxToken) && syntaxToken.Kind != SyntaxKind.OpenBracketToken)
			{
				return ScanTypeFlags.MustBeType;
			}
			goto IL_006b;
		}
		goto IL_00f2;
		IL_006b:
		lastTokenOfType = EatToken();
		TerminatorState termState;
		if (base.CurrentToken.Kind == SyntaxKind.OpenBracketToken)
		{
			lastTokenOfType = EatToken(SyntaxKind.OpenBracketToken);
			termState = _termState;
			_termState |= TerminatorState.IsEndOfFunctionPointerCallingConvention;
			try
			{
				while (true)
				{
					lastTokenOfType = TryEatToken(SyntaxKind.IdentifierToken) ?? lastTokenOfType;
					if (skipBadFunctionPointerTokens() == PostSkipAction.Abort)
					{
						break;
					}
					lastTokenOfType = EatToken();
				}
				lastTokenOfType = TryEatToken(SyntaxKind.CloseBracketToken) ?? lastTokenOfType;
			}
			finally
			{
				_termState = termState;
			}
		}
		goto IL_00f2;
		IL_00f2:
		if (!IsPossibleFunctionPointerParameterListStart(base.CurrentToken))
		{
			return ScanTypeFlags.MustBeType;
		}
		bool flag = EatToken().Kind == SyntaxKind.LessThanToken;
		termState = _termState;
		_termState |= (TerminatorState)(flag ? 8388608 : 16777216);
		SyntaxListBuilder<SyntaxToken> syntaxListBuilder = _pool.Allocate<SyntaxToken>();
		try
		{
			while (true)
			{
				ParseParameterModifiers(syntaxListBuilder, isFunctionPointerParameter: true, isLambdaParameter: false);
				syntaxListBuilder.Clear();
				ScanType(out lastTokenOfType2);
				if (skipBadFunctionPointerTokens() == PostSkipAction.Abort)
				{
					break;
				}
				EatToken(SyntaxKind.CommaToken);
			}
		}
		finally
		{
			_termState = termState;
			_pool.Free(syntaxListBuilder);
		}
		if (!flag && base.CurrentToken.Kind == SyntaxKind.CloseParenToken)
		{
			lastTokenOfType = EatTokenAsKind(SyntaxKind.GreaterThanToken);
		}
		else
		{
			lastTokenOfType = EatToken(SyntaxKind.GreaterThanToken);
		}
		return ScanTypeFlags.MustBeType;
		PostSkipAction skipBadFunctionPointerTokens()
		{
			GreenNode trailingTrivia;
			return SkipBadTokensWithExpectedKind((LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken, (LanguageParser p, SyntaxKind _) => p.IsTerminator(), SyntaxKind.CommaToken, SyntaxKind.None, out trailingTrivia);
		}
	}

	private static bool IsPredefinedType(SyntaxKind keyword)
	{
		return SyntaxFacts.IsPredefinedType(keyword);
	}

	public TypeSyntax ParseTypeName()
	{
		return ParseType();
	}

	private TypeSyntax ParseTypeOrVoid()
	{
		if (base.CurrentToken.Kind == SyntaxKind.VoidKeyword && PeekToken(1).Kind != SyntaxKind.AsteriskToken)
		{
			return _syntaxFactory.PredefinedType(EatToken());
		}
		return ParseType();
	}

	private TypeSyntax ParseType(ParseTypeMode mode = ParseTypeMode.Normal)
	{
		if (base.CurrentToken.Kind == SyntaxKind.RefKeyword)
		{
			return _syntaxFactory.RefType(EatToken(), (base.CurrentToken.Kind == SyntaxKind.ReadOnlyKeyword) ? EatToken() : null, ParseTypeCore(ParseTypeMode.AfterRef));
		}
		return ParseTypeCore(mode);
	}

	private TypeSyntax ParseTypeCore(ParseTypeMode mode)
	{
		NameOptions options;
		switch (mode)
		{
		case ParseTypeMode.AfterIs:
			options = NameOptions.InExpression | NameOptions.PossiblePattern | NameOptions.AfterIs;
			break;
		case ParseTypeMode.DefinitePattern:
			options = NameOptions.InExpression | NameOptions.PossiblePattern | NameOptions.DefinitePattern;
			break;
		case ParseTypeMode.AfterOut:
			options = NameOptions.InExpression | NameOptions.AfterOut;
			break;
		case ParseTypeMode.AfterTupleComma:
			options = NameOptions.InExpression | NameOptions.AfterTupleComma;
			break;
		case ParseTypeMode.FirstElementOfPossibleTupleLiteral:
			options = NameOptions.InExpression | NameOptions.FirstElementOfPossibleTupleLiteral;
			break;
		case ParseTypeMode.Normal:
		case ParseTypeMode.Parameter:
		case ParseTypeMode.AfterRef:
		case ParseTypeMode.AsExpression:
		case ParseTypeMode.NewExpression:
			options = NameOptions.None;
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(mode);
		}
		TypeSyntax typeSyntax = ParseUnderlyingType(mode, options);
		int lastTokenPosition = -1;
		while (IsMakingProgress(ref lastTokenPosition))
		{
			switch (base.CurrentToken.Kind)
			{
			case SyntaxKind.QuestionToken:
			{
				SyntaxToken syntaxToken = TryEatNullableQualifierIfApplicable(typeSyntax, mode);
				if (syntaxToken != null)
				{
					typeSyntax = _syntaxFactory.NullableType(typeSyntax, syntaxToken);
					continue;
				}
				break;
			}
			case SyntaxKind.AsteriskToken:
				switch (mode)
				{
				case ParseTypeMode.AfterIs:
				case ParseTypeMode.DefinitePattern:
				case ParseTypeMode.AfterTupleComma:
				case ParseTypeMode.FirstElementOfPossibleTupleLiteral:
					if (PointerTypeModsFollowedByRankAndDimensionSpecifier())
					{
						typeSyntax = ParsePointerTypeMods(typeSyntax);
						continue;
					}
					break;
				case ParseTypeMode.Normal:
				case ParseTypeMode.Parameter:
				case ParseTypeMode.AfterOut:
				case ParseTypeMode.AfterRef:
				case ParseTypeMode.AsExpression:
				case ParseTypeMode.NewExpression:
					typeSyntax = ParsePointerTypeMods(typeSyntax);
					continue;
				}
				break;
			case SyntaxKind.OpenBracketToken:
			{
				SyntaxListBuilder<ArrayRankSpecifierSyntax> item = _pool.Allocate<ArrayRankSpecifierSyntax>();
				do
				{
					item.Add(ParseArrayRankSpecifier(out var _));
				}
				while (base.CurrentToken.Kind == SyntaxKind.OpenBracketToken);
				typeSyntax = _syntaxFactory.ArrayType(typeSyntax, _pool.ToListAndFree(item));
				continue;
			}
			}
			break;
		}
		return typeSyntax;
	}

	private SyntaxToken TryEatNullableQualifierIfApplicable(TypeSyntax typeParsedSoFar, ParseTypeMode mode)
	{
		SyntaxKind kind = typeParsedSoFar.Kind;
		if (kind - 8624 <= SyntaxKind.List)
		{
			return null;
		}
		using (DisposableResetPoint disposableResetPoint = GetDisposableResetPoint(resetOnDispose: false))
		{
			SyntaxToken result = EatToken();
			if (!canFollowNullableType())
			{
				disposableResetPoint.Reset();
				return null;
			}
			return result;
		}
		bool canFollowNullableType()
		{
			if (mode == ParseTypeMode.AfterIs && base.CurrentToken.Kind == SyntaxKind.OpenBracketToken)
			{
				switch (PeekToken(1).Kind)
				{
				case SyntaxKind.CommaToken:
					return true;
				case SyntaxKind.CloseBracketToken:
					using (GetDisposableResetPoint(resetOnDispose: true))
					{
						ParsePossibleRefExpression();
						return base.CurrentToken.Kind != SyntaxKind.ColonToken;
					}
				default:
					return false;
				}
			}
			switch (mode)
			{
			case ParseTypeMode.AfterIs:
			case ParseTypeMode.DefinitePattern:
			case ParseTypeMode.AsExpression:
				if (IsTrueIdentifier(base.CurrentToken))
				{
					SyntaxKind kind2 = base.CurrentToken.ContextualKind;
					if ((kind2 == SyntaxKind.FromKeyword || kind2 - 8435 <= SyntaxKind.List) ? true : false)
					{
						return false;
					}
					SyntaxToken syntaxToken = PeekToken(1);
					if (syntaxToken.ContextualKind == SyntaxKind.WithKeyword)
					{
						return false;
					}
					SyntaxKind kind3 = syntaxToken.Kind;
					if (SyntaxFacts.IsLiteral(kind3))
					{
						return true;
					}
					if (SyntaxFacts.IsPredefinedType(kind3))
					{
						return true;
					}
					if ((kind3 == SyntaxKind.CloseParenToken || kind3 == SyntaxKind.CloseBraceToken || kind3 == SyntaxKind.CloseBracketToken) ? true : false)
					{
						return true;
					}
					return kind3 switch
					{
						SyntaxKind.OpenBraceToken => true, 
						SyntaxKind.CommaToken => true, 
						SyntaxKind.SemicolonToken => true, 
						SyntaxKind.EndOfFileToken => true, 
						_ => false, 
					};
				}
				if (base.CurrentToken.Kind == SyntaxKind.OpenBracketToken)
				{
					return true;
				}
				return !CanStartExpression();
			case ParseTypeMode.NewExpression:
			{
				SyntaxKind kind2 = base.CurrentToken.Kind;
				if (kind2 == SyntaxKind.OpenParenToken || kind2 == SyntaxKind.OpenBraceToken || kind2 == SyntaxKind.OpenBracketToken)
				{
					return true;
				}
				return false;
			}
			default:
				return true;
			}
		}
	}

	private bool PointerTypeModsFollowedByRankAndDimensionSpecifier()
	{
		int num = 0;
		while (true)
		{
			switch (PeekToken(num).Kind)
			{
			case SyntaxKind.OpenBracketToken:
				return true;
			default:
				return false;
			case SyntaxKind.AsteriskToken:
				break;
			}
			num++;
		}
	}

	private ArrayRankSpecifierSyntax ParseArrayRankSpecifier(out bool sawNonOmittedSize)
	{
		sawNonOmittedSize = false;
		bool flag = false;
		SyntaxToken openBracket = EatToken(SyntaxKind.OpenBracketToken);
		SeparatedSyntaxListBuilder<ExpressionSyntax> item = _pool.AllocateSeparated<ExpressionSyntax>();
		OmittedArraySizeExpressionSyntax node = _syntaxFactory.OmittedArraySizeExpression(SyntaxFactory.Token(SyntaxKind.OmittedArraySizeExpressionToken));
		int lastTokenPosition = -1;
		while (IsMakingProgress(ref lastTokenPosition) && base.CurrentToken.Kind != SyntaxKind.CloseBracketToken)
		{
			if (base.CurrentToken.Kind == SyntaxKind.CommaToken)
			{
				flag = true;
				item.Add(node);
				item.AddSeparator(EatToken());
			}
			else if (IsPossibleExpression())
			{
				ExpressionSyntax node2 = ParseExpressionCore();
				sawNonOmittedSize = true;
				item.Add(node2);
				if (base.CurrentToken.Kind != SyntaxKind.CloseBracketToken)
				{
					item.AddSeparator(EatToken(SyntaxKind.CommaToken));
				}
			}
			else if (SkipBadArrayRankSpecifierTokens(ref openBracket, item, SyntaxKind.CommaToken) == PostSkipAction.Abort)
			{
				break;
			}
		}
		if ((item.Count & 1) == 0)
		{
			flag = true;
			item.Add(node);
		}
		if (flag & sawNonOmittedSize)
		{
			for (int i = 0; i < item.Count; i++)
			{
				if (item[i].RawKind == 8654)
				{
					item[i] = AddError(CreateMissingIdentifierName(), 0, item[i].Width, ErrorCode.ERR_ValueExpected);
				}
			}
		}
		return _syntaxFactory.ArrayRankSpecifier(openBracket, _pool.ToListAndFree(in item), EatToken(SyntaxKind.CloseBracketToken));
	}

	private TupleTypeSyntax ParseTupleType()
	{
		SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
		SeparatedSyntaxListBuilder<TupleElementSyntax> item = _pool.AllocateSeparated<TupleElementSyntax>();
		if (base.CurrentToken.Kind != SyntaxKind.CloseParenToken)
		{
			item.Add(ParseTupleElement());
			while (base.CurrentToken.Kind == SyntaxKind.CommaToken)
			{
				item.AddSeparator(EatToken(SyntaxKind.CommaToken));
				item.Add(ParseTupleElement());
			}
		}
		if (item.Count < 2)
		{
			if (item.Count < 1)
			{
				item.Add(_syntaxFactory.TupleElement(CreateMissingIdentifierName(), null));
			}
			item.AddSeparator(SyntaxFactory.MissingToken(SyntaxKind.CommaToken));
			IdentifierNameSyntax type = AddError(CreateMissingIdentifierName(), ErrorCode.ERR_TupleTooFewElements);
			item.Add(_syntaxFactory.TupleElement(type, null));
		}
		return _syntaxFactory.TupleType(openParenToken, _pool.ToListAndFree(in item), EatToken(SyntaxKind.CloseParenToken));
	}

	private TupleElementSyntax ParseTupleElement()
	{
		return _syntaxFactory.TupleElement(ParseType(), IsTrueIdentifier() ? ParseIdentifierToken() : null);
	}

	private PostSkipAction SkipBadArrayRankSpecifierTokens(ref SyntaxToken openBracket, SeparatedSyntaxListBuilder<ExpressionSyntax> list, SyntaxKind expected)
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref openBracket, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleExpression(), (LanguageParser p, SyntaxKind _) => p.CurrentToken.Kind == SyntaxKind.CloseBracketToken, expected);
	}

	private TypeSyntax ParseUnderlyingType(ParseTypeMode mode, NameOptions options = NameOptions.None)
	{
		if (IsPredefinedType(base.CurrentToken.Kind))
		{
			SyntaxToken syntaxToken = EatToken();
			if (syntaxToken.Kind == SyntaxKind.VoidKeyword && base.CurrentToken.Kind != SyntaxKind.AsteriskToken)
			{
				syntaxToken = AddError(syntaxToken, (mode == ParseTypeMode.Parameter) ? ErrorCode.ERR_NoVoidParameter : ErrorCode.ERR_NoVoidHere);
			}
			return _syntaxFactory.PredefinedType(syntaxToken);
		}
		if (IsTrueIdentifier() || base.CurrentToken.Kind == SyntaxKind.ColonColonToken)
		{
			return ParseQualifiedName(options);
		}
		if (base.CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			return ParseTupleType();
		}
		if (IsFunctionPointerStart())
		{
			return ParseFunctionPointerTypeSyntax();
		}
		return AddError(CreateMissingIdentifierName(), (mode == ParseTypeMode.NewExpression) ? ErrorCode.ERR_BadNewExpr : ErrorCode.ERR_TypeExpected);
	}

	private FunctionPointerTypeSyntax ParseFunctionPointerTypeSyntax()
	{
		SyntaxToken delegateKeyword = EatToken(SyntaxKind.DelegateKeyword);
		SyntaxToken asteriskToken = EatToken(SyntaxKind.AsteriskToken);
		FunctionPointerCallingConventionSyntax callingConvention = parseCallingConvention();
		if (!IsPossibleFunctionPointerParameterListStart(base.CurrentToken))
		{
			SyntaxToken lessThanToken = CreateMissingToken(SyntaxKind.LessThanToken, SyntaxKind.None);
			SeparatedSyntaxListBuilder<FunctionPointerParameterSyntax> item = _pool.AllocateSeparated<FunctionPointerParameterSyntax>();
			FunctionPointerParameterSyntax node = SyntaxFactory.FunctionPointerParameter(default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>), default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>), CreateMissingIdentifierName());
			item.Add(node);
			return SyntaxFactory.FunctionPointerType(delegateKeyword, asteriskToken, callingConvention, SyntaxFactory.FunctionPointerParameterList(lessThanToken, _pool.ToListAndFree(in item), TryEatToken(SyntaxKind.GreaterThanToken) ?? SyntaxFactory.MissingToken(SyntaxKind.GreaterThanToken)));
		}
		SyntaxToken syntaxToken = EatTokenAsKind(SyntaxKind.LessThanToken);
		TerminatorState termState = _termState;
		_termState |= (TerminatorState)(syntaxToken.IsMissing ? 16777216 : 8388608);
		SeparatedSyntaxListBuilder<FunctionPointerParameterSyntax> item2 = _pool.AllocateSeparated<FunctionPointerParameterSyntax>();
		try
		{
			while (true)
			{
				SyntaxListBuilder<SyntaxToken> syntaxListBuilder = _pool.Allocate<SyntaxToken>();
				ParseParameterModifiers(syntaxListBuilder, isFunctionPointerParameter: true, isLambdaParameter: false);
				item2.Add(SyntaxFactory.FunctionPointerParameter(default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>), _pool.ToTokenListAndFree(syntaxListBuilder), ParseTypeOrVoid()));
				if (skipBadFunctionPointerTokens<FunctionPointerParameterSyntax>(item2) == PostSkipAction.Abort)
				{
					break;
				}
				item2.AddSeparator(EatToken(SyntaxKind.CommaToken));
			}
			return SyntaxFactory.FunctionPointerType(delegateKeyword, asteriskToken, callingConvention, SyntaxFactory.FunctionPointerParameterList(syntaxToken, _pool.ToListAndFree(in item2), (syntaxToken.IsMissing && base.CurrentToken.Kind == SyntaxKind.CloseParenToken) ? EatTokenAsKind(SyntaxKind.GreaterThanToken) : EatToken(SyntaxKind.GreaterThanToken)));
		}
		finally
		{
			_termState = termState;
		}
		FunctionPointerCallingConventionSyntax? parseCallingConvention()
		{
			if (base.CurrentToken.Kind != SyntaxKind.IdentifierToken)
			{
				return null;
			}
			SyntaxToken syntaxToken2 = PeekToken(1);
			SyntaxToken currentToken = base.CurrentToken;
			SyntaxToken syntaxToken3;
			if (currentToken != null)
			{
				SyntaxKind contextualKind = currentToken.ContextualKind;
				if (contextualKind - 8445 <= SyntaxKind.List)
				{
					syntaxToken3 = EatContextualToken(base.CurrentToken.ContextualKind);
					goto IL_0081;
				}
			}
			if (IsPossibleFunctionPointerParameterListStart(syntaxToken2))
			{
				syntaxToken3 = EatTokenAsKind(SyntaxKind.ManagedKeyword);
			}
			else
			{
				if (syntaxToken2.Kind != SyntaxKind.OpenBracketToken)
				{
					return null;
				}
				syntaxToken3 = EatTokenAsKind(SyntaxKind.UnmanagedKeyword);
			}
			goto IL_0081;
			IL_0081:
			FunctionPointerUnmanagedCallingConventionListSyntax functionPointerUnmanagedCallingConventionListSyntax = null;
			if (base.CurrentToken.Kind == SyntaxKind.OpenBracketToken)
			{
				SyntaxToken openBracketToken = EatToken(SyntaxKind.OpenBracketToken);
				SeparatedSyntaxListBuilder<FunctionPointerUnmanagedCallingConventionSyntax> item3 = _pool.AllocateSeparated<FunctionPointerUnmanagedCallingConventionSyntax>();
				TerminatorState termState2 = _termState;
				_termState |= TerminatorState.IsEndOfFunctionPointerCallingConvention;
				try
				{
					while (true)
					{
						item3.Add(SyntaxFactory.FunctionPointerUnmanagedCallingConvention(EatToken(SyntaxKind.IdentifierToken)));
						if (skipBadFunctionPointerTokens<FunctionPointerUnmanagedCallingConventionSyntax>(item3) == PostSkipAction.Abort)
						{
							break;
						}
						item3.AddSeparator(EatToken(SyntaxKind.CommaToken));
					}
					SyntaxToken closeBracketToken = EatToken(SyntaxKind.CloseBracketToken);
					functionPointerUnmanagedCallingConventionListSyntax = SyntaxFactory.FunctionPointerUnmanagedCallingConventionList(openBracketToken, _pool.ToListAndFree(in item3), closeBracketToken);
				}
				finally
				{
					_termState = termState2;
				}
			}
			if (syntaxToken3.Kind == SyntaxKind.ManagedKeyword && functionPointerUnmanagedCallingConventionListSyntax != null)
			{
				functionPointerUnmanagedCallingConventionListSyntax = AddError(functionPointerUnmanagedCallingConventionListSyntax, ErrorCode.ERR_CannotSpecifyManagedWithUnmanagedSpecifiers);
			}
			return SyntaxFactory.FunctionPointerCallingConvention(syntaxToken3, functionPointerUnmanagedCallingConventionListSyntax);
		}
		PostSkipAction skipBadFunctionPointerTokens<T>(SeparatedSyntaxListBuilder<T> list) where T : CSharpSyntaxNode
		{
			CSharpSyntaxNode startToken = null;
			return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken, (LanguageParser p, SyntaxKind _) => false, SyntaxKind.CommaToken);
		}
	}

	private bool IsFunctionPointerStart()
	{
		if (base.CurrentToken.Kind == SyntaxKind.DelegateKeyword)
		{
			return PeekToken(1).Kind == SyntaxKind.AsteriskToken;
		}
		return false;
	}

	private static bool IsPossibleFunctionPointerParameterListStart(SyntaxToken token)
	{
		if (token.Kind != SyntaxKind.LessThanToken)
		{
			return token.Kind == SyntaxKind.OpenParenToken;
		}
		return true;
	}

	private TypeSyntax ParsePointerTypeMods(TypeSyntax type)
	{
		while (base.CurrentToken.Kind == SyntaxKind.AsteriskToken)
		{
			type = _syntaxFactory.PointerType(type, EatToken());
		}
		return type;
	}

	public StatementSyntax ParseStatement()
	{
		return ParseWithStackGuard((LanguageParser @this) => @this.ParsePossiblyAttributedStatement() ?? @this.ParseExpressionStatement(default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>)), (LanguageParser @this) => SyntaxFactory.EmptyStatement(default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>), SyntaxFactory.MissingToken(SyntaxKind.SemicolonToken)));
	}

	private StatementSyntax ParsePossiblyAttributedStatement()
	{
		return ParseStatementCore(ParseStatementAttributeDeclarations(), isGlobal: false);
	}

	private Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> ParseStatementAttributeDeclarations()
	{
		if (base.CurrentToken.Kind != SyntaxKind.OpenBracketToken)
		{
			return default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>);
		}
		ResetPoint state = GetResetPoint();
		ParseCollectionExpression();
		bool flag = false;
		while (base.CurrentToken.Kind == SyntaxKind.OpenBracketToken)
		{
			ParseBracketedArgumentList();
			flag = true;
		}
		bool flag2;
		switch (base.CurrentToken.Kind)
		{
		case SyntaxKind.ExclamationToken:
		case SyntaxKind.DotToken:
		case SyntaxKind.QuestionToken:
		case SyntaxKind.MinusMinusToken:
		case SyntaxKind.PlusPlusToken:
		case SyntaxKind.MinusGreaterThanToken:
			flag2 = true;
			break;
		default:
			flag2 = false;
			break;
		}
		flag2 = flag2 || IsExpectedBinaryOperator(base.CurrentToken.Kind) || IsExpectedAssignmentOperator(base.CurrentToken.Kind);
		if (!flag2)
		{
			SyntaxKind contextualKind = base.CurrentToken.ContextualKind;
			bool flag3 = ((contextualKind == SyntaxKind.SwitchKeyword || contextualKind == SyntaxKind.WithKeyword) ? true : false);
			flag2 = flag3 && PeekToken(1).Kind == SyntaxKind.OpenBraceToken;
		}
		bool flag4 = flag2;
		if ((!flag4 & flag) && base.CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			flag4 = ContainsErrorDiagnostic(ParseReturnType()) || !IsTrueIdentifier();
		}
		Reset(ref state);
		ValueType result = (flag4 ? ((ValueType)default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>)) : ((ValueType)ParseAttributeDeclarations(inExpressionContext: true)));
		Release(ref state);
		return (Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>)result;
	}

	private StatementSyntax ParseStatementCore(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, bool isGlobal)
	{
		StatementSyntax statementSyntax = TryReuseStatement(attributes, isGlobal);
		if (statementSyntax != null)
		{
			return statementSyntax;
		}
		ResetPoint resetPointBeforeStatement = GetResetPoint();
		try
		{
			_recursionDepth++;
			StackGuard.EnsureSufficientExecutionStack(_recursionDepth);
			switch (base.CurrentToken.Kind)
			{
			case SyntaxKind.FixedKeyword:
				return ParseFixedStatement(attributes);
			case SyntaxKind.BreakKeyword:
				return ParseBreakStatement(attributes);
			case SyntaxKind.ContinueKeyword:
				return ParseContinueStatement(attributes);
			case SyntaxKind.TryKeyword:
			case SyntaxKind.CatchKeyword:
			case SyntaxKind.FinallyKeyword:
				return ParseTryStatement(attributes);
			case SyntaxKind.CheckedKeyword:
			case SyntaxKind.UncheckedKeyword:
				return ParseCheckedStatement(attributes);
			case SyntaxKind.DoKeyword:
				return ParseDoStatement(attributes);
			case SyntaxKind.ForKeyword:
				return ParseForOrForEachStatement(attributes);
			case SyntaxKind.ForEachKeyword:
				return ParseForEachStatement(attributes, null);
			case SyntaxKind.GotoKeyword:
				return ParseGotoStatement(attributes);
			case SyntaxKind.IfKeyword:
				return ParseIfStatement(attributes);
			case SyntaxKind.ElseKeyword:
				return ParseMisplacedElse(attributes);
			case SyntaxKind.LockKeyword:
				return ParseLockStatement(attributes);
			case SyntaxKind.ReturnKeyword:
				return ParseReturnStatement(attributes);
			case SyntaxKind.SwitchKeyword:
			case SyntaxKind.CaseKeyword:
				return ParseSwitchStatement(attributes);
			case SyntaxKind.ThrowKeyword:
				return ParseThrowStatement(attributes);
			case SyntaxKind.UnsafeKeyword:
			{
				StatementSyntax statementSyntax2 = TryParseStatementStartingWithUnsafe(attributes);
				if (statementSyntax2 != null)
				{
					return statementSyntax2;
				}
				break;
			}
			case SyntaxKind.UsingKeyword:
				return ParseStatementStartingWithUsing(attributes);
			case SyntaxKind.WhileKeyword:
				return ParseWhileStatement(attributes);
			case SyntaxKind.OpenBraceToken:
				return ParseBlock(attributes);
			case SyntaxKind.SemicolonToken:
				return _syntaxFactory.EmptyStatement(attributes, EatToken());
			case SyntaxKind.IdentifierToken:
			{
				StatementSyntax statementSyntax2 = TryParseStatementStartingWithIdentifier(attributes, isGlobal);
				if (statementSyntax2 != null)
				{
					return statementSyntax2;
				}
				break;
			}
			}
			return ParseStatementCoreRest(attributes, isGlobal, ref resetPointBeforeStatement);
		}
		finally
		{
			_recursionDepth--;
			Release(ref resetPointBeforeStatement);
		}
	}

	private StatementSyntax TryReuseStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, bool isGlobal)
	{
		if (IsIncrementalAndFactoryContextMatches && base.CurrentNode is Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax && !isGlobal && attributes.Count == 0)
		{
			return (StatementSyntax)EatNode();
		}
		return null;
	}

	private StatementSyntax ParseStatementCoreRest(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, bool isGlobal, ref ResetPoint resetPointBeforeStatement)
	{
		isGlobal = isGlobal && base.IsScript;
		if (!IsPossibleLocalDeclarationStatement(isGlobal))
		{
			return ParseExpressionStatement(attributes);
		}
		if (isGlobal)
		{
			return null;
		}
		bool flag = base.CurrentToken.ContextualKind == SyntaxKind.AwaitKeyword;
		StatementSyntax statementSyntax = ParseLocalDeclarationStatement(attributes);
		if (statementSyntax == null)
		{
			Reset(ref resetPointBeforeStatement);
			return null;
		}
		if ((statementSyntax.ContainsDiagnostics & flag) && !IsInAsync)
		{
			Reset(ref resetPointBeforeStatement);
			ParserSyntaxContextResetter parserSyntaxContextResetter = new ParserSyntaxContextResetter(this, true);
			try
			{
				statementSyntax = ParseExpressionStatement(attributes);
			}
			finally
			{
				parserSyntaxContextResetter.Dispose();
			}
		}
		return statementSyntax;
	}

	private StatementSyntax TryParseStatementStartingWithIdentifier(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, bool isGlobal)
	{
		if (base.CurrentToken.ContextualKind == SyntaxKind.AwaitKeyword && PeekToken(1).Kind == SyntaxKind.ForEachKeyword)
		{
			return ParseForEachStatement(attributes, EatContextualToken(SyntaxKind.AwaitKeyword));
		}
		if (IsPossibleAwaitUsing())
		{
			if (PeekToken(2).Kind == SyntaxKind.OpenParenToken)
			{
				return ParseUsingStatement(attributes, EatContextualToken(SyntaxKind.AwaitKeyword));
			}
		}
		else
		{
			if (IsPossibleLabeledStatement())
			{
				return ParseLabeledStatement(attributes);
			}
			if (IsPossibleYieldStatement())
			{
				return ParseYieldStatement(attributes);
			}
			if (IsPossibleAwaitExpressionStatement())
			{
				return ParseExpressionStatement(attributes);
			}
			if (IsQueryExpression(mayBeVariableDeclaration: true, isGlobal && base.IsScript))
			{
				return ParseExpressionStatement(attributes, ParseQueryExpression(Precedence.Expression));
			}
		}
		return null;
	}

	private StatementSyntax ParseStatementStartingWithUsing(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		if (PeekToken(1).Kind != SyntaxKind.OpenParenToken)
		{
			return ParseLocalDeclarationStatement(attributes);
		}
		return ParseUsingStatement(attributes);
	}

	private StatementSyntax TryParseStatementStartingWithUnsafe(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		if (!IsPossibleUnsafeStatement())
		{
			return null;
		}
		return ParseUnsafeStatement(attributes);
	}

	private bool IsPossibleAwaitUsing()
	{
		if (base.CurrentToken.ContextualKind == SyntaxKind.AwaitKeyword)
		{
			return PeekToken(1).Kind == SyntaxKind.UsingKeyword;
		}
		return false;
	}

	private bool IsPossibleLabeledStatement()
	{
		if (PeekToken(1).Kind == SyntaxKind.ColonToken)
		{
			return IsTrueIdentifier();
		}
		return false;
	}

	private bool IsPossibleUnsafeStatement()
	{
		return PeekToken(1).Kind == SyntaxKind.OpenBraceToken;
	}

	private bool IsPossibleYieldStatement()
	{
		bool flag = base.CurrentToken.ContextualKind == SyntaxKind.YieldKeyword;
		if (flag)
		{
			SyntaxKind kind = PeekToken(1).Kind;
			bool flag2 = ((kind == SyntaxKind.BreakKeyword || kind == SyntaxKind.ReturnKeyword) ? true : false);
			flag = flag2;
		}
		return flag;
	}

	private bool IsPossibleLocalDeclarationStatement(bool isGlobalScriptLevel)
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind != SyntaxKind.RefKeyword && !IsDeclarationModifier(kind))
		{
			if (SyntaxFacts.IsPredefinedType(kind))
			{
				SyntaxKind kind2 = PeekToken(1).Kind;
				if (kind2 != SyntaxKind.DotToken && kind2 != SyntaxKind.OpenParenToken)
				{
					goto IL_0041;
				}
			}
			if (kind == SyntaxKind.UsingKeyword)
			{
				return true;
			}
			if (IsPossibleAwaitUsing())
			{
				return true;
			}
			if (IsDefiniteScopedModifier(isFunctionPointerParameter: false, isLambdaParameter: false))
			{
				return true;
			}
			kind = base.CurrentToken.ContextualKind;
			bool flag = IsAdditionalLocalFunctionModifier(kind);
			if (flag)
			{
				bool flag2 = ((kind == SyntaxKind.AsyncKeyword || kind == SyntaxKind.ScopedKeyword) ? true : false);
				flag = !flag2 || ShouldContextualKeywordBeTreatedAsModifier(parsingStatementNotDeclaration: true);
			}
			if (flag)
			{
				return true;
			}
			return IsPossibleFirstTypedIdentifierInLocalDeclarationStatement(isGlobalScriptLevel);
		}
		goto IL_0041;
		IL_0041:
		return true;
	}

	private bool IsPossibleFirstTypedIdentifierInLocalDeclarationStatement(bool isGlobalScriptLevel)
	{
		bool? flag = IsPossibleTypedIdentifierStart(base.CurrentToken, PeekToken(1), allowThisKeyword: false);
		if (flag.HasValue)
		{
			return flag.Value;
		}
		if (base.CurrentToken.ContextualKind == SyntaxKind.IdentifierToken)
		{
			SyntaxToken syntaxToken = PeekToken(1);
			if (syntaxToken.Kind == SyntaxKind.DotToken && syntaxToken.TrailingTrivia.Any(8539) && PeekToken(2).Kind == SyntaxKind.IdentifierToken && PeekToken(3).Kind == SyntaxKind.IdentifierToken)
			{
				SyntaxKind kind = PeekToken(4).Kind;
				if (kind != SyntaxKind.SemicolonToken && kind != SyntaxKind.EqualsToken && kind != SyntaxKind.CommaToken && kind != SyntaxKind.OpenParenToken && kind != SyntaxKind.LessThanToken)
				{
					return false;
				}
			}
		}
		using (GetDisposableResetPoint(resetOnDispose: true))
		{
			ScanTypeFlags scanTypeFlags = ScanType();
			if (scanTypeFlags == ScanTypeFlags.MustBeType)
			{
				SyntaxKind kind2 = base.CurrentToken.Kind;
				if (kind2 != SyntaxKind.DotToken && kind2 != SyntaxKind.OpenParenToken)
				{
					return true;
				}
			}
			if (scanTypeFlags == ScanTypeFlags.NotType)
			{
				return false;
			}
			if (base.CurrentToken.Kind != SyntaxKind.IdentifierToken)
			{
				return scanTypeFlags == ScanTypeFlags.GenericTypeOrExpression && (IsDefiniteStatement() || IsTypeDeclarationStart() || IsAccessibilityModifier(base.CurrentToken.Kind));
			}
			if (isGlobalScriptLevel)
			{
				switch (scanTypeFlags)
				{
				case ScanTypeFlags.PointerOrMultiplication:
					return false;
				case ScanTypeFlags.NullableType:
					return IsPossibleDeclarationStatementFollowingNullableType(isGlobalScriptLevel);
				}
			}
			return true;
		}
	}

	private bool IsPossibleTopLevelUsingLocalDeclarationStatement()
	{
		if (base.CurrentToken.Kind != SyntaxKind.UsingKeyword)
		{
			return false;
		}
		SyntaxKind kind = PeekToken(1).Kind;
		if (kind == SyntaxKind.RefKeyword)
		{
			return true;
		}
		if (IsDeclarationModifier(kind))
		{
			if (kind != SyntaxKind.StaticKeyword)
			{
				return true;
			}
		}
		else if (SyntaxFacts.IsPredefinedType(kind))
		{
			return true;
		}
		using (GetDisposableResetPoint(resetOnDispose: true))
		{
			EatToken();
			if (IsDefiniteScopedModifier(isFunctionPointerParameter: false, isLambdaParameter: false))
			{
				return true;
			}
			if (kind == SyntaxKind.StaticKeyword)
			{
				EatToken();
			}
			return IsPossibleFirstTypedIdentifierInLocalDeclarationStatement(isGlobalScriptLevel: false);
		}
	}

	private bool IsPossibleDeclarationStatementFollowingNullableType(bool isGlobalScriptLevel)
	{
		if (IsFieldDeclaration(isEvent: false, isGlobalScriptLevel))
		{
			return IsPossibleFieldDeclarationFollowingNullableType();
		}
		ParseMemberName(out var explicitInterfaceOpt, out var identifierOrThisOpt, out var typeParameterListOpt, isEvent: false);
		if (explicitInterfaceOpt == null && identifierOrThisOpt == null && typeParameterListOpt == null)
		{
			return false;
		}
		if (base.CurrentToken.Kind == SyntaxKind.OpenBraceToken)
		{
			return true;
		}
		if (identifierOrThisOpt.Kind == SyntaxKind.ThisKeyword)
		{
			return false;
		}
		return IsPossibleMethodDeclarationFollowingNullableType();
	}

	private bool IsPossibleFieldDeclarationFollowingNullableType()
	{
		if (base.CurrentToken.Kind != SyntaxKind.IdentifierToken)
		{
			return false;
		}
		EatToken();
		if (base.CurrentToken.Kind == SyntaxKind.EqualsToken)
		{
			TerminatorState termState = _termState;
			_termState |= TerminatorState.IsEndOfFieldDeclaration;
			EatToken();
			ParseVariableInitializer();
			_termState = termState;
		}
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind == SyntaxKind.SemicolonToken || kind == SyntaxKind.CommaToken)
		{
			return true;
		}
		return false;
	}

	private bool IsPossibleMethodDeclarationFollowingNullableType()
	{
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfMethodSignature;
		ParameterListSyntax parameterListSyntax = ParseParenthesizedParameterList(forExtension: false);
		_termState = termState;
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<GreenNode> withSeparators = parameterListSyntax.Parameters.GetWithSeparators();
		if (!parameterListSyntax.CloseParenToken.IsMissing)
		{
			if (base.CurrentToken.Kind == SyntaxKind.OpenBraceToken || base.CurrentToken.ContextualKind == SyntaxKind.WhereKeyword)
			{
				return true;
			}
			if (base.CurrentToken.Kind == SyntaxKind.ColonToken)
			{
				return false;
			}
		}
		if (withSeparators.Count == 0)
		{
			return false;
		}
		ParameterSyntax parameterSyntax = (ParameterSyntax)withSeparators[0];
		if (parameterSyntax.AttributeLists.Count > 0)
		{
			return true;
		}
		for (int i = 0; i < parameterSyntax.Modifiers.Count; i++)
		{
			if (parameterSyntax.Modifiers[i].Kind == SyntaxKind.ParamsKeyword)
			{
				return true;
			}
		}
		if (parameterSyntax.Type == null)
		{
			if (parameterSyntax.Identifier.Kind == SyntaxKind.ArgListKeyword)
			{
				return true;
			}
		}
		else if (parameterSyntax.Type.Kind == SyntaxKind.NullableType)
		{
			if (parameterSyntax.Modifiers.Count > 0)
			{
				return true;
			}
			if (!parameterSyntax.Identifier.IsMissing && ((withSeparators.Count >= 2 && !withSeparators[1].IsMissing) || (withSeparators.Count == 1 && !parameterListSyntax.CloseParenToken.IsMissing)))
			{
				return true;
			}
		}
		else
		{
			if (parameterSyntax.Type.Kind == SyntaxKind.IdentifierName && ((IdentifierNameSyntax)parameterSyntax.Type).Identifier.ContextualKind == SyntaxKind.FromKeyword)
			{
				return false;
			}
			if (!parameterSyntax.Identifier.IsMissing)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsAnonymousDelegateExpression()
	{
		SyntaxToken syntaxToken = PeekToken(1);
		if (syntaxToken.Kind == SyntaxKind.OpenBraceToken)
		{
			return true;
		}
		if (syntaxToken.Kind != SyntaxKind.OpenParenToken)
		{
			return false;
		}
		using (GetDisposableResetPoint(resetOnDispose: true))
		{
			EatToken();
			EatToken();
			if (ScanTupleType(out var _) == ScanTypeFlags.TupleType && base.CurrentToken.Kind == SyntaxKind.IdentifierToken)
			{
				return false;
			}
			return true;
		}
	}

	private bool IsPossibleNewExpression()
	{
		SyntaxToken syntaxToken = PeekToken(1);
		SyntaxKind kind = syntaxToken.Kind;
		if (kind == SyntaxKind.OpenBraceToken || kind == SyntaxKind.OpenBracketToken)
		{
			return true;
		}
		if (SyntaxFacts.GetBaseTypeDeclarationKind(syntaxToken.Kind) != SyntaxKind.None)
		{
			return false;
		}
		switch (GetModifierExcludingScoped(syntaxToken))
		{
		case DeclarationModifiers.Partial:
			if (SyntaxFacts.IsPredefinedType(PeekToken(2).Kind))
			{
				return false;
			}
			if (IsTypeModifierOrTypeKeyword(PeekToken(2).Kind))
			{
				return false;
			}
			break;
		default:
			return false;
		case DeclarationModifiers.None:
			break;
		}
		bool? flag = IsPossibleTypedIdentifierStart(syntaxToken, PeekToken(2), allowThisKeyword: true);
		if (flag.HasValue)
		{
			return !flag.Value;
		}
		using (GetDisposableResetPoint(resetOnDispose: true))
		{
			EatToken();
			ScanTypeFlags scanTypeFlags = ScanType();
			return !IsPossibleMemberName() || scanTypeFlags == ScanTypeFlags.NotType;
		}
	}

	private bool? IsPossibleTypedIdentifierStart(SyntaxToken current, SyntaxToken next, bool allowThisKeyword)
	{
		if (IsTrueIdentifier(current))
		{
			switch (next.Kind)
			{
			case SyntaxKind.AsteriskToken:
			case SyntaxKind.OpenBracketToken:
			case SyntaxKind.LessThanToken:
			case SyntaxKind.DotToken:
			case SyntaxKind.QuestionToken:
			case SyntaxKind.ColonColonToken:
				return null;
			case SyntaxKind.OpenParenToken:
				if (current.IsIdentifierVar())
				{
					return null;
				}
				return false;
			case SyntaxKind.IdentifierToken:
				return IsTrueIdentifier(next);
			case SyntaxKind.ThisKeyword:
				return allowThisKeyword;
			default:
				return false;
			}
		}
		return null;
	}

	private BlockSyntax ParsePossiblyAttributedBlock()
	{
		return ParseBlock(ParseAttributeDeclarations(inExpressionContext: false));
	}

	private BlockSyntax ParseMethodOrAccessorBodyBlock(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, bool isAccessorBody)
	{
		if (IsIncrementalAndFactoryContextMatches && base.CurrentNodeKind == SyntaxKind.Block && attributes.Count == 0)
		{
			return (BlockSyntax)EatNode();
		}
		CSharpSyntaxNode previousNode = ((isAccessorBody && base.CurrentToken.Kind != SyntaxKind.OpenBraceToken) ? AddError(SyntaxFactory.MissingToken(SyntaxKind.OpenBraceToken), IsFeatureEnabled(MessageID.IDS_FeatureExpressionBodiedAccessor) ? ErrorCode.ERR_SemiOrLBraceOrArrowExpected : ErrorCode.ERR_SemiOrLBraceExpected) : EatToken(SyntaxKind.OpenBraceToken));
		SyntaxListBuilder<StatementSyntax> syntaxListBuilder = _pool.Allocate<StatementSyntax>();
		ParseStatements(ref previousNode, syntaxListBuilder, stopOnSwitchSections: false);
		BlockSyntax result = _syntaxFactory.Block(attributes, (SyntaxToken)previousNode, IsLargeEnoughNonEmptyStatementList(syntaxListBuilder) ? new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<StatementSyntax>(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(((SyntaxListBuilder)syntaxListBuilder).ToArray())) : ((Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<StatementSyntax>)syntaxListBuilder), EatToken(SyntaxKind.CloseBraceToken));
		_pool.Free(syntaxListBuilder);
		return result;
	}

	private BlockSyntax ParseBlock(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		if (IsIncrementalAndFactoryContextMatches && base.CurrentNodeKind == SyntaxKind.Block && attributes.Count == 0)
		{
			return (BlockSyntax)EatNode();
		}
		CSharpSyntaxNode previousNode = EatToken(SyntaxKind.OpenBraceToken);
		SyntaxListBuilder<StatementSyntax> syntaxListBuilder = _pool.Allocate<StatementSyntax>();
		ParseStatements(ref previousNode, syntaxListBuilder, stopOnSwitchSections: false);
		return _syntaxFactory.Block(attributes, (SyntaxToken)previousNode, _pool.ToListAndFree(syntaxListBuilder), EatToken(SyntaxKind.CloseBraceToken));
	}

	private static bool IsLargeEnoughNonEmptyStatementList(SyntaxListBuilder<StatementSyntax> statements)
	{
		if (statements.Count == 0)
		{
			return false;
		}
		if (statements.Count == 1)
		{
			return statements[0].Width > 60;
		}
		return true;
	}

	private void ParseStatements(ref CSharpSyntaxNode previousNode, SyntaxListBuilder<StatementSyntax> statements, bool stopOnSwitchSections)
	{
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsPossibleStatementStartOrStop;
		if (stopOnSwitchSections)
		{
			_termState |= TerminatorState.IsSwitchSectionStart;
		}
		int lastTokenPosition = -1;
		PostSkipAction num;
		do
		{
			IL_006e:
			SyntaxKind kind = base.CurrentToken.Kind;
			if (kind == SyntaxKind.CloseBraceToken || kind == SyntaxKind.EndOfFileToken || (stopOnSwitchSections && IsPossibleSwitchSection()) || !IsMakingProgress(ref lastTokenPosition))
			{
				break;
			}
			if (IsPossibleStatement())
			{
				StatementSyntax statementSyntax = ParsePossiblyAttributedStatement();
				if (statementSyntax != null)
				{
					statements.Add(statementSyntax);
					goto IL_006e;
				}
			}
			num = SkipBadStatementListTokens(statements, SyntaxKind.CloseBraceToken, out var trailingTrivia);
			if (trailingTrivia != null)
			{
				previousNode = AddTrailingSkippedSyntax(previousNode, trailingTrivia);
			}
		}
		while (num != PostSkipAction.Abort);
		_termState = termState;
	}

	private bool IsPossibleStatementStartOrStop()
	{
		if (base.CurrentToken.Kind != SyntaxKind.SemicolonToken)
		{
			return IsPossibleStatement();
		}
		return true;
	}

	private PostSkipAction SkipBadStatementListTokens(SyntaxListBuilder<StatementSyntax> statements, SyntaxKind expected, out GreenNode trailingTrivia)
	{
		return SkipBadListTokensWithExpectedKindHelper(statements, (LanguageParser p) => !p.IsPossibleStatement(), (LanguageParser p, SyntaxKind _) => p.CurrentToken.Kind == SyntaxKind.CloseBraceToken, expected, SyntaxKind.None, out trailingTrivia);
	}

	private bool IsDefiniteStatement()
	{
		switch (base.CurrentToken.Kind)
		{
		case SyntaxKind.IfKeyword:
		case SyntaxKind.ElseKeyword:
		case SyntaxKind.WhileKeyword:
		case SyntaxKind.ForKeyword:
		case SyntaxKind.ForEachKeyword:
		case SyntaxKind.DoKeyword:
		case SyntaxKind.CaseKeyword:
		case SyntaxKind.TryKeyword:
		case SyntaxKind.LockKeyword:
		case SyntaxKind.GotoKeyword:
		case SyntaxKind.BreakKeyword:
		case SyntaxKind.ContinueKeyword:
		case SyntaxKind.ReturnKeyword:
		case SyntaxKind.ConstKeyword:
		case SyntaxKind.FixedKeyword:
		case SyntaxKind.VolatileKeyword:
		case SyntaxKind.ExternKeyword:
		case SyntaxKind.UsingKeyword:
		case SyntaxKind.UnsafeKeyword:
			return true;
		default:
			return false;
		}
	}

	private bool IsPossibleStatement()
	{
		if (IsDefiniteStatement())
		{
			return true;
		}
		SyntaxKind kind = base.CurrentToken.Kind;
		switch (kind)
		{
		case SyntaxKind.OpenBraceToken:
		case SyntaxKind.OpenBracketToken:
		case SyntaxKind.SemicolonToken:
		case SyntaxKind.SwitchKeyword:
		case SyntaxKind.ThrowKeyword:
		case SyntaxKind.StaticKeyword:
		case SyntaxKind.ReadOnlyKeyword:
		case SyntaxKind.RefKeyword:
		case SyntaxKind.CheckedKeyword:
		case SyntaxKind.UncheckedKeyword:
			return true;
		case SyntaxKind.IdentifierToken:
			return IsTrueIdentifier();
		default:
			if (!IsPredefinedType(kind))
			{
				return IsPossibleExpression();
			}
			return true;
		}
	}

	private FixedStatementSyntax ParseFixedStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		SyntaxToken fixedKeyword = EatToken(SyntaxKind.FixedKeyword);
		SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfFixedStatement;
		VariableDeclarationSyntax declaration = ParseParenthesizedVariableDeclaration(VariableFlags.None, null);
		_termState = termState;
		return _syntaxFactory.FixedStatement(attributes, fixedKeyword, openParenToken, declaration, EatToken(SyntaxKind.CloseParenToken), ParseEmbeddedStatement());
	}

	private bool IsEndOfFixedStatement()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind == SyntaxKind.CloseParenToken || kind == SyntaxKind.OpenBraceToken || kind == SyntaxKind.SemicolonToken)
		{
			return true;
		}
		return false;
	}

	private StatementSyntax ParseEmbeddedStatement()
	{
		return parseEmbeddedStatementRest(ParsePossiblyAttributedStatement());
		StatementSyntax parseEmbeddedStatementRest(StatementSyntax statement)
		{
			if (statement == null)
			{
				return SyntaxFactory.EmptyStatement(default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>), EatToken(SyntaxKind.SemicolonToken));
			}
			if (statement.Kind == SyntaxKind.ExpressionStatement && base.IsScript)
			{
				ExpressionStatementSyntax expressionStatementSyntax = (ExpressionStatementSyntax)statement;
				SyntaxToken semicolonToken = expressionStatementSyntax.SemicolonToken;
				if (semicolonToken.IsMissing && !semicolonToken.GetDiagnostics().Contains((DiagnosticInfo diagnosticInfo) => diagnosticInfo.Code == 1002))
				{
					semicolonToken = AddError(semicolonToken, ErrorCode.ERR_SemicolonExpected);
					return expressionStatementSyntax.Update(expressionStatementSyntax.AttributeLists, expressionStatementSyntax.Expression, semicolonToken);
				}
			}
			return statement;
		}
	}

	private BreakStatementSyntax ParseBreakStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		return _syntaxFactory.BreakStatement(attributes, EatToken(SyntaxKind.BreakKeyword), EatToken(SyntaxKind.SemicolonToken));
	}

	private ContinueStatementSyntax ParseContinueStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		return _syntaxFactory.ContinueStatement(attributes, EatToken(SyntaxKind.ContinueKeyword), EatToken(SyntaxKind.SemicolonToken));
	}

	private TryStatementSyntax ParseTryStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		SyntaxToken syntaxToken = EatToken(SyntaxKind.TryKeyword);
		BlockSyntax blockSyntax;
		if (syntaxToken.IsMissing)
		{
			blockSyntax = missingBlock();
		}
		else
		{
			TerminatorState termState = _termState;
			_termState |= TerminatorState.IsEndOfTryBlock;
			blockSyntax = ParsePossiblyAttributedBlock();
			_termState = termState;
		}
		SyntaxListBuilder<CatchClauseSyntax> item = default(SyntaxListBuilder<CatchClauseSyntax>);
		FinallyClauseSyntax finallyClauseSyntax = null;
		if (base.CurrentToken.Kind == SyntaxKind.CatchKeyword)
		{
			item = _pool.Allocate<CatchClauseSyntax>();
			while (base.CurrentToken.Kind == SyntaxKind.CatchKeyword)
			{
				item.Add(ParseCatchClause());
			}
		}
		if (base.CurrentToken.Kind == SyntaxKind.FinallyKeyword)
		{
			finallyClauseSyntax = _syntaxFactory.FinallyClause(EatToken(), ParsePossiblyAttributedBlock());
		}
		if (item.IsNull && finallyClauseSyntax == null)
		{
			if (!ContainsErrorDiagnostic(blockSyntax))
			{
				blockSyntax = AddErrorToLastToken(blockSyntax, ErrorCode.ERR_ExpectedEndTry);
			}
			finallyClauseSyntax = _syntaxFactory.FinallyClause(SyntaxFactory.MissingToken(SyntaxKind.FinallyKeyword), missingBlock());
		}
		return _syntaxFactory.TryStatement(attributes, syntaxToken, blockSyntax, _pool.ToListAndFree(item), finallyClauseSyntax);
		BlockSyntax missingBlock()
		{
			return _syntaxFactory.Block(default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>), SyntaxFactory.MissingToken(SyntaxKind.OpenBraceToken), default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<StatementSyntax>), SyntaxFactory.MissingToken(SyntaxKind.CloseBraceToken));
		}
	}

	private bool IsEndOfTryBlock()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind == SyntaxKind.CloseBraceToken || kind - 8335 <= SyntaxKind.List)
		{
			return true;
		}
		return false;
	}

	private CatchClauseSyntax ParseCatchClause()
	{
		SyntaxToken catchKeyword = EatToken();
		CatchDeclarationSyntax declaration = null;
		TerminatorState termState = _termState;
		if (base.CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			SyntaxToken openParenToken = EatToken();
			_termState |= TerminatorState.IsEndOfCatchClause;
			TypeSyntax type = ParseType();
			SyntaxToken identifier = null;
			if (IsTrueIdentifier())
			{
				identifier = ParseIdentifierToken();
			}
			_termState = termState;
			SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
			declaration = _syntaxFactory.CatchDeclaration(openParenToken, type, identifier, closeParenToken);
		}
		CatchFilterClauseSyntax filter = null;
		SyntaxKind contextualKind = base.CurrentToken.ContextualKind;
		if (contextualKind == SyntaxKind.WhenKeyword || contextualKind == SyntaxKind.IfKeyword)
		{
			SyntaxToken syntaxToken = EatContextualToken(SyntaxKind.WhenKeyword);
			if (contextualKind == SyntaxKind.IfKeyword)
			{
				syntaxToken = AddTrailingSkippedSyntax(syntaxToken, EatToken());
			}
			_termState |= TerminatorState.IsEndOfFilterClause;
			SyntaxToken openParenToken2 = EatToken(SyntaxKind.OpenParenToken);
			ExpressionSyntax filterExpression = ParseExpressionForParenthesizedConstruct();
			_termState = termState;
			SyntaxToken closeParenToken2 = EatToken(SyntaxKind.CloseParenToken);
			filter = _syntaxFactory.CatchFilterClause(syntaxToken, openParenToken2, filterExpression, closeParenToken2);
		}
		_termState |= TerminatorState.IsEndOfCatchBlock;
		BlockSyntax block = ParsePossiblyAttributedBlock();
		_termState = termState;
		return _syntaxFactory.CatchClause(catchKeyword, declaration, filter, block);
	}

	private bool IsEndOfCatchClause()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind == SyntaxKind.CloseParenToken || kind - 8205 <= SyntaxKind.List || kind - 8335 <= SyntaxKind.List)
		{
			return true;
		}
		return false;
	}

	private bool IsEndOfFilterClause()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind == SyntaxKind.CloseParenToken || kind - 8205 <= SyntaxKind.List || kind - 8335 <= SyntaxKind.List)
		{
			return true;
		}
		return false;
	}

	private bool IsEndOfCatchBlock()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind == SyntaxKind.CloseBraceToken || kind - 8335 <= SyntaxKind.List)
		{
			return true;
		}
		return false;
	}

	private StatementSyntax ParseCheckedStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		if (PeekToken(1).Kind == SyntaxKind.OpenParenToken)
		{
			return ParseExpressionStatement(attributes);
		}
		SyntaxToken syntaxToken = EatToken();
		return _syntaxFactory.CheckedStatement(SyntaxFacts.GetCheckStatement(syntaxToken.Kind), attributes, syntaxToken, ParsePossiblyAttributedBlock());
	}

	private DoStatementSyntax ParseDoStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		SyntaxToken doKeyword = EatToken(SyntaxKind.DoKeyword);
		StatementSyntax statement = ParseEmbeddedStatement();
		SyntaxToken whileKeyword = EatToken(SyntaxKind.WhileKeyword);
		SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfDoWhileExpression;
		ExpressionSyntax condition = ParseExpressionForParenthesizedConstruct();
		_termState = termState;
		return _syntaxFactory.DoStatement(attributes, doKeyword, statement, whileKeyword, openParenToken, condition, EatToken(SyntaxKind.CloseParenToken), EatToken(SyntaxKind.SemicolonToken));
	}

	private bool IsEndOfDoWhileExpression()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind == SyntaxKind.CloseParenToken || kind == SyntaxKind.SemicolonToken)
		{
			return true;
		}
		return false;
	}

	private StatementSyntax ParseForOrForEachStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		using DisposableResetPoint disposableResetPoint = GetDisposableResetPoint(resetOnDispose: false);
		EatToken();
		if (EatToken().Kind == SyntaxKind.OpenParenToken && ScanType() != ScanTypeFlags.NotType && EatToken().Kind == SyntaxKind.IdentifierToken && EatToken().Kind == SyntaxKind.InKeyword)
		{
			disposableResetPoint.Reset();
			return ParseForEachStatement(attributes, null);
		}
		disposableResetPoint.Reset();
		return ParseForStatement(attributes);
	}

	private ForStatementSyntax ParseForStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfForStatementArgument;
		SyntaxToken forKeyword = EatToken(SyntaxKind.ForKeyword);
		SyntaxToken openParen = EatToken(SyntaxKind.OpenParenToken);
		(VariableDeclarationSyntax variableDeclaration, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> initializers) tuple = eatVariableDeclarationOrInitializers();
		VariableDeclarationSyntax item = tuple.variableDeclaration;
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> item2 = tuple.initializers;
		SyntaxToken firstSemicolonToken = eatCommaOrSemicolon();
		SyntaxKind kind = base.CurrentToken.Kind;
		ExpressionSyntax condition = ((kind != SyntaxKind.SemicolonToken && kind != SyntaxKind.CommaToken) ? ParseExpressionCore() : null);
		SyntaxToken startToken = eatCommaOrSemicolon();
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> incrementors = ((base.CurrentToken.Kind != SyntaxKind.CloseParenToken) ? parseForStatementExpressionList(ref startToken, allowSemicolonAsSeparator: true) : default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax>));
		ForStatementSyntax result = _syntaxFactory.ForStatement(attributes, forKeyword, openParen, item, item2, firstSemicolonToken, condition, startToken, incrementors, eatUnexpectedTokensAndCloseParenToken(), ParseEmbeddedStatement());
		_termState = termState;
		return result;
		SyntaxToken eatCommaOrSemicolon()
		{
			if (base.CurrentToken.Kind != SyntaxKind.CommaToken)
			{
				return EatToken(SyntaxKind.SemicolonToken);
			}
			return EatTokenAsKind(SyntaxKind.SemicolonToken);
		}
		SyntaxToken eatUnexpectedTokensAndCloseParenToken()
		{
			SyntaxListBuilder syntaxListBuilder = _pool.Allocate();
			while (true)
			{
				SyntaxKind kind2 = base.CurrentToken.Kind;
				if ((kind2 != SyntaxKind.SemicolonToken && kind2 != SyntaxKind.CommaToken) || 1 == 0)
				{
					break;
				}
				syntaxListBuilder.Add(EatTokenEvenWithIncorrectKind(SyntaxKind.CloseParenToken));
			}
			SyntaxToken node = EatToken(SyntaxKind.CloseParenToken);
			return AddLeadingSkippedSyntax(node, _pool.ToTokenListAndFree(syntaxListBuilder).Node);
		}
		(VariableDeclarationSyntax variableDeclaration, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> initializers) eatVariableDeclarationOrInitializers()
		{
			using DisposableResetPoint disposableResetPoint = GetDisposableResetPoint(resetOnDispose: false);
			bool flag = false;
			if (base.CurrentToken.ContextualKind == SyntaxKind.ScopedKeyword)
			{
				if (PeekToken(1).Kind == SyntaxKind.RefKeyword)
				{
					flag = true;
				}
				else
				{
					EatToken();
					flag = ScanType() != ScanTypeFlags.NotType && base.CurrentToken.Kind == SyntaxKind.IdentifierToken;
					disposableResetPoint.Reset();
				}
			}
			else if (base.CurrentToken.Kind == SyntaxKind.RefKeyword)
			{
				flag = true;
			}
			if (!flag)
			{
				flag = !IsQueryExpression(mayBeVariableDeclaration: true, mayBeMemberDeclaration: false) && ScanType() != ScanTypeFlags.NotType && IsTrueIdentifier();
				disposableResetPoint.Reset();
			}
			if (flag)
			{
				return (variableDeclaration: ParseParenthesizedVariableDeclaration(VariableFlags.ForStatement, ParsePossibleScopedKeyword(isFunctionPointerParameter: false, isLambdaParameter: false)), initializers: default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax>));
			}
			if (base.CurrentToken.Kind != SyntaxKind.SemicolonToken)
			{
				return (variableDeclaration: null, initializers: parseForStatementExpressionList(ref openParen, allowSemicolonAsSeparator: false));
			}
			return default((VariableDeclarationSyntax, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax>));
		}
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> parseForStatementExpressionList(ref SyntaxToken openToken, bool allowSemicolonAsSeparator)
		{
			return ParseCommaSeparatedSyntaxList(ref openToken, SyntaxKind.CloseParenToken, (LanguageParser @this) => @this.IsPossibleExpression(), (LanguageParser @this) => @this.ParseExpressionCore(), skipBadForStatementExpressionListTokens, allowTrailingSeparator: false, requireOneElement: false, allowSemicolonAsSeparator);
		}
		static PostSkipAction skipBadForStatementExpressionListTokens(LanguageParser @this, ref SyntaxToken startToken2, SeparatedSyntaxListBuilder<ExpressionSyntax> list, SyntaxKind expectedKind, SyntaxKind closeKind)
		{
			SyntaxKind kind2 = @this.CurrentToken.Kind;
			if ((kind2 == SyntaxKind.CloseParenToken || kind2 == SyntaxKind.SemicolonToken) ? true : false)
			{
				return PostSkipAction.Abort;
			}
			return @this.SkipBadSeparatedListTokensWithExpectedKind(ref startToken2, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleExpression(), (LanguageParser p, SyntaxKind syntaxKind) => p.CurrentToken.Kind == syntaxKind || p.CurrentToken.Kind == SyntaxKind.SemicolonToken, expectedKind, closeKind);
		}
	}

	private bool IsEndOfForStatementArgument()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind == SyntaxKind.CloseParenToken || kind == SyntaxKind.OpenBraceToken || kind == SyntaxKind.SemicolonToken)
		{
			return true;
		}
		return false;
	}

	private CommonForEachStatementSyntax ParseForEachStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, SyntaxToken awaitTokenOpt)
	{
		SyntaxToken forEachKeyword;
		if (base.CurrentToken.Kind == SyntaxKind.ForKeyword)
		{
			SyntaxToken nodeOrToken = EatToken();
			nodeOrToken = AddError(nodeOrToken, ErrorCode.ERR_SyntaxError, SyntaxFacts.GetText(SyntaxKind.ForEachKeyword));
			forEachKeyword = ConvertToMissingWithTrailingTrivia(nodeOrToken, SyntaxKind.ForEachKeyword);
		}
		else
		{
			forEachKeyword = EatToken(SyntaxKind.ForEachKeyword);
		}
		SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
		ExpressionSyntax expressionSyntax = ParseExpressionOrDeclaration(ParseTypeMode.Normal, permitTupleDesignation: true);
		SyntaxToken syntaxToken = EatToken(SyntaxKind.InKeyword, ErrorCode.ERR_InExpected);
		if (!IsValidForeachVariable(expressionSyntax))
		{
			syntaxToken = AddError(syntaxToken, ErrorCode.ERR_BadForeachDecl);
		}
		ExpressionSyntax expression = ParseExpressionCore();
		SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
		StatementSyntax statement = ParseEmbeddedStatement();
		if (expressionSyntax is DeclarationExpressionSyntax declarationExpressionSyntax && declarationExpressionSyntax.designation.Kind != SyntaxKind.ParenthesizedVariableDesignation)
		{
			SyntaxToken identifier;
			switch (declarationExpressionSyntax.designation.Kind)
			{
			case SyntaxKind.SingleVariableDesignation:
				identifier = ((SingleVariableDesignationSyntax)declarationExpressionSyntax.designation).identifier;
				break;
			case SyntaxKind.DiscardDesignation:
			{
				SyntaxToken underscoreToken = ((DiscardDesignationSyntax)declarationExpressionSyntax.designation).underscoreToken;
				identifier = SyntaxToken.WithValue(SyntaxKind.IdentifierToken, underscoreToken.LeadingTrivia.Node, underscoreToken.Text, underscoreToken.ValueText, underscoreToken.TrailingTrivia.Node);
				break;
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(declarationExpressionSyntax.designation.Kind);
			}
			return _syntaxFactory.ForEachStatement(attributes, awaitTokenOpt, forEachKeyword, openParenToken, declarationExpressionSyntax.Type, identifier, syntaxToken, expression, closeParenToken, statement);
		}
		return _syntaxFactory.ForEachVariableStatement(attributes, awaitTokenOpt, forEachKeyword, openParenToken, expressionSyntax, syntaxToken, expression, closeParenToken, statement);
	}

	private ExpressionSyntax ParseExpressionOrDeclaration(ParseTypeMode mode, bool permitTupleDesignation)
	{
		if (!IsPossibleDeclarationExpression(mode, permitTupleDesignation, out var isScoped))
		{
			return ParseSubExpression(Precedence.Expression);
		}
		return ParseDeclarationExpression(mode, isScoped);
	}

	private bool IsPossibleDeclarationExpression(ParseTypeMode mode, bool permitTupleDesignation, out bool isScoped)
	{
		isScoped = false;
		if (IsInAsync && base.CurrentToken.ContextualKind == SyntaxKind.AwaitKeyword)
		{
			return false;
		}
		using DisposableResetPoint disposableResetPoint = GetDisposableResetPoint(resetOnDispose: true);
		if (base.CurrentToken.ContextualKind == SyntaxKind.ScopedKeyword)
		{
			EatToken();
			if (ScanType() != ScanTypeFlags.NotType && base.CurrentToken.Kind == SyntaxKind.IdentifierToken)
			{
				switch (mode)
				{
				case ParseTypeMode.FirstElementOfPossibleTupleLiteral:
					if (PeekToken(1).Kind == SyntaxKind.CommaToken)
					{
						isScoped = true;
						return true;
					}
					break;
				case ParseTypeMode.AfterTupleComma:
				{
					SyntaxKind kind = PeekToken(1).Kind;
					if ((kind == SyntaxKind.CloseParenToken || kind == SyntaxKind.CommaToken) ? true : false)
					{
						isScoped = true;
						return true;
					}
					break;
				}
				default:
					isScoped = true;
					return true;
				}
			}
			disposableResetPoint.Reset();
		}
		bool flag = IsVarType();
		if (ScanType(mode, out var lastTokenOfType) == ScanTypeFlags.NotType)
		{
			return false;
		}
		if (!ScanDesignation(permitTupleDesignation && (flag || IsPredefinedType(lastTokenOfType.Kind))))
		{
			return false;
		}
		switch (mode)
		{
		case ParseTypeMode.FirstElementOfPossibleTupleLiteral:
			return base.CurrentToken.Kind == SyntaxKind.CommaToken;
		case ParseTypeMode.AfterTupleComma:
		{
			SyntaxKind kind = base.CurrentToken.Kind;
			return (kind == SyntaxKind.CloseParenToken || kind == SyntaxKind.CommaToken) ? true : false;
		}
		default:
			return true;
		}
	}

	private bool IsVarType()
	{
		if (!base.CurrentToken.IsIdentifierVar())
		{
			return false;
		}
		switch (PeekToken(1).Kind)
		{
		case SyntaxKind.AsteriskToken:
		case SyntaxKind.OpenBracketToken:
		case SyntaxKind.LessThanToken:
		case SyntaxKind.DotToken:
		case SyntaxKind.QuestionToken:
		case SyntaxKind.ColonColonToken:
			return false;
		default:
			return true;
		}
	}

	private static bool IsValidForeachVariable(ExpressionSyntax variable)
	{
		return variable.Kind switch
		{
			SyntaxKind.DeclarationExpression => true, 
			SyntaxKind.TupleExpression => true, 
			SyntaxKind.IdentifierName => ((IdentifierNameSyntax)variable).Identifier.ContextualKind == SyntaxKind.UnderscoreToken, 
			_ => false, 
		};
	}

	private GotoStatementSyntax ParseGotoStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		SyntaxToken gotoKeyword = EatToken(SyntaxKind.GotoKeyword);
		SyntaxToken syntaxToken = null;
		ExpressionSyntax expression = null;
		SyntaxKind kind = base.CurrentToken.Kind;
		SyntaxKind kind2;
		if (kind - 8332 <= SyntaxKind.List)
		{
			syntaxToken = EatToken();
			if (syntaxToken.Kind == SyntaxKind.CaseKeyword)
			{
				kind2 = SyntaxKind.GotoCaseStatement;
				expression = ParseExpressionCore();
			}
			else
			{
				kind2 = SyntaxKind.GotoDefaultStatement;
			}
		}
		else
		{
			kind2 = SyntaxKind.GotoStatement;
			expression = ParseIdentifierName();
		}
		return _syntaxFactory.GotoStatement(kind2, attributes, gotoKeyword, syntaxToken, expression, EatToken(SyntaxKind.SemicolonToken));
	}

	private ExpressionSyntax ParseExpressionForParenthesizedConstruct()
	{
		return ParseErrantExpressionWhenNoCloseParenToken(ParseExpressionCore());
	}

	private ExpressionSyntax ParseErrantExpressionWhenNoCloseParenToken(ExpressionSyntax expression)
	{
		if (base.CurrentToken.Kind != SyntaxKind.CloseParenToken)
		{
			using DisposableResetPoint disposableResetPoint = GetDisposableResetPoint(resetOnDispose: false);
			ExpressionSyntax expressionSyntax = ParseExpressionCore();
			if (base.CurrentToken.Kind == SyntaxKind.CloseParenToken && PeekToken(1).Kind != SyntaxKind.EqualsGreaterThanToken && !expressionSyntax.GetLastToken().IsMissing)
			{
				expression = AddTrailingSkippedSyntax(expression, AddErrorToFirstToken(expressionSyntax, ErrorCode.ERR_UnexpectedToken, expressionSyntax.GetFirstToken().Text));
			}
			else
			{
				disposableResetPoint.Reset();
			}
		}
		return expression;
	}

	private IfStatementSyntax ParseIfStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		ArrayBuilder<(SyntaxToken, SyntaxToken, ExpressionSyntax, SyntaxToken, StatementSyntax, SyntaxToken)> instance = ArrayBuilder<(SyntaxToken, SyntaxToken, ExpressionSyntax, SyntaxToken, StatementSyntax, SyntaxToken)>.GetInstance();
		StatementSyntax statementSyntax = null;
		do
		{
			SyntaxToken item = EatToken(SyntaxKind.IfKeyword);
			SyntaxToken item2 = EatToken(SyntaxKind.OpenParenToken);
			ExpressionSyntax item3 = ParseExpressionForParenthesizedConstruct();
			SyntaxToken item4 = EatToken(SyntaxKind.CloseParenToken);
			StatementSyntax item5 = ParseEmbeddedStatement();
			SyntaxToken syntaxToken = ((base.CurrentToken.Kind != SyntaxKind.ElseKeyword) ? null : EatToken(SyntaxKind.ElseKeyword));
			instance.Push((item, item2, item3, item4, item5, syntaxToken));
			if (syntaxToken == null)
			{
				statementSyntax = null;
				break;
			}
			if (base.CurrentToken.Kind != SyntaxKind.IfKeyword)
			{
				statementSyntax = ParseEmbeddedStatement();
				break;
			}
			statementSyntax = TryReuseStatement(default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>), isGlobal: false);
		}
		while (statementSyntax == null);
		IfStatementSyntax ifStatementSyntax;
		do
		{
			(SyntaxToken, SyntaxToken, ExpressionSyntax, SyntaxToken, StatementSyntax, SyntaxToken) tuple = instance.Pop();
			SyntaxToken item6 = tuple.Item1;
			SyntaxToken item7 = tuple.Item2;
			ExpressionSyntax item8 = tuple.Item3;
			SyntaxToken item9 = tuple.Item4;
			StatementSyntax item10 = tuple.Item5;
			SyntaxToken item11 = tuple.Item6;
			ElseClauseSyntax elseClauseSyntax = ((statementSyntax == null) ? null : _syntaxFactory.ElseClause(item11, statementSyntax));
			ifStatementSyntax = _syntaxFactory.IfStatement(instance.Any() ? default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>) : attributes, item6, item7, item8, item9, item10, elseClauseSyntax);
			statementSyntax = ifStatementSyntax;
		}
		while (instance.Any());
		instance.Free();
		return ifStatementSyntax;
	}

	private IfStatementSyntax ParseMisplacedElse(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		return _syntaxFactory.IfStatement(attributes, EatToken(SyntaxKind.IfKeyword, ErrorCode.ERR_ElseCannotStartStatement), EatToken(SyntaxKind.OpenParenToken), ParseExpressionCore(), EatToken(SyntaxKind.CloseParenToken), ParseExpressionStatement(default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>)), ParseElseClauseOpt());
	}

	private ElseClauseSyntax ParseElseClauseOpt()
	{
		if (base.CurrentToken.Kind == SyntaxKind.ElseKeyword)
		{
			return _syntaxFactory.ElseClause(EatToken(SyntaxKind.ElseKeyword), ParseEmbeddedStatement());
		}
		return null;
	}

	private LockStatementSyntax ParseLockStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		return _syntaxFactory.LockStatement(attributes, EatToken(SyntaxKind.LockKeyword), EatToken(SyntaxKind.OpenParenToken), ParseExpressionForParenthesizedConstruct(), EatToken(SyntaxKind.CloseParenToken), ParseEmbeddedStatement());
	}

	private ReturnStatementSyntax ParseReturnStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		return _syntaxFactory.ReturnStatement(attributes, EatToken(SyntaxKind.ReturnKeyword), (base.CurrentToken.Kind != SyntaxKind.SemicolonToken) ? ParsePossibleRefExpression() : null, EatToken(SyntaxKind.SemicolonToken));
	}

	private YieldStatementSyntax ParseYieldStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		SyntaxToken yieldKeyword = SyntaxParser.ConvertToKeyword(EatToken());
		ExpressionSyntax expression = null;
		SyntaxKind kind;
		SyntaxToken syntaxToken;
		if (base.CurrentToken.Kind == SyntaxKind.BreakKeyword)
		{
			kind = SyntaxKind.YieldBreakStatement;
			syntaxToken = EatToken();
		}
		else
		{
			kind = SyntaxKind.YieldReturnStatement;
			syntaxToken = EatToken(SyntaxKind.ReturnKeyword);
			if (base.CurrentToken.Kind == SyntaxKind.SemicolonToken)
			{
				syntaxToken = AddError(syntaxToken, ErrorCode.ERR_EmptyYield);
			}
			else
			{
				expression = ParseExpressionCore();
			}
		}
		return _syntaxFactory.YieldStatement(kind, attributes, yieldKeyword, syntaxToken, expression, EatToken(SyntaxKind.SemicolonToken));
	}

	private SwitchStatementSyntax ParseSwitchStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		parseSwitchHeader(out var switchKeyword, out var openParen, out var expression, out var closeParen, out var openBrace);
		SyntaxListBuilder<SwitchSectionSyntax> item = _pool.Allocate<SwitchSectionSyntax>();
		while (IsPossibleSwitchSection())
		{
			item.Add(ParseSwitchSection());
		}
		return _syntaxFactory.SwitchStatement(attributes, switchKeyword, openParen, expression, closeParen, openBrace, _pool.ToListAndFree(item), EatToken(SyntaxKind.CloseBraceToken));
		void parseSwitchHeader(out SyntaxToken reference, out SyntaxToken reference2, out ExpressionSyntax reference3, out SyntaxToken reference4, out SyntaxToken reference5)
		{
			if (base.CurrentToken.Kind == SyntaxKind.CaseKeyword)
			{
				reference = EatToken(SyntaxKind.SwitchKeyword);
				reference2 = SyntaxFactory.MissingToken(SyntaxKind.OpenParenToken);
				reference3 = CreateMissingIdentifierName();
				reference4 = SyntaxFactory.MissingToken(SyntaxKind.CloseParenToken);
				reference5 = SyntaxFactory.MissingToken(SyntaxKind.OpenBraceToken);
			}
			else
			{
				reference = EatToken(SyntaxKind.SwitchKeyword);
				reference3 = ParseExpressionCore();
				if (reference3.Kind == SyntaxKind.ParenthesizedExpression)
				{
					ParenthesizedExpressionSyntax parenthesizedExpressionSyntax = (ParenthesizedExpressionSyntax)reference3;
					reference2 = parenthesizedExpressionSyntax.OpenParenToken;
					reference3 = parenthesizedExpressionSyntax.Expression;
					reference4 = parenthesizedExpressionSyntax.CloseParenToken;
				}
				else if (reference3.Kind == SyntaxKind.TupleExpression)
				{
					reference2 = (reference4 = null);
				}
				else
				{
					reference2 = SyntaxFactory.MissingToken(SyntaxKind.OpenParenToken);
					reference3 = AddError(reference3, ErrorCode.ERR_SwitchGoverningExpressionRequiresParens);
					reference4 = SyntaxFactory.MissingToken(SyntaxKind.CloseParenToken);
				}
				reference5 = EatToken(SyntaxKind.OpenBraceToken);
			}
		}
	}

	private bool IsPossibleSwitchSection()
	{
		if (base.CurrentToken.Kind != SyntaxKind.CaseKeyword)
		{
			if (base.CurrentToken.Kind == SyntaxKind.DefaultKeyword)
			{
				return PeekToken(1).Kind != SyntaxKind.OpenParenToken;
			}
			return false;
		}
		return true;
	}

	private SwitchSectionSyntax ParseSwitchSection()
	{
		SyntaxListBuilder<SwitchLabelSyntax> item = _pool.Allocate<SwitchLabelSyntax>();
		SyntaxListBuilder<StatementSyntax> syntaxListBuilder = _pool.Allocate<StatementSyntax>();
		do
		{
			SwitchLabelSyntax node;
			if (base.CurrentToken.Kind == SyntaxKind.CaseKeyword)
			{
				SyntaxToken keyword = EatToken();
				if (base.CurrentToken.Kind == SyntaxKind.ColonToken)
				{
					node = _syntaxFactory.CaseSwitchLabel(keyword, ParseIdentifierName(ErrorCode.ERR_ConstantExpected), EatToken(SyntaxKind.ColonToken));
				}
				else
				{
					CSharpSyntaxNode cSharpSyntaxNode = ParseExpressionOrPatternForSwitchStatement();
					if (base.CurrentToken.ContextualKind == SyntaxKind.WhenKeyword && cSharpSyntaxNode is ExpressionSyntax expression)
					{
						cSharpSyntaxNode = _syntaxFactory.ConstantPattern(expression);
					}
					if (cSharpSyntaxNode.Kind == SyntaxKind.DiscardPattern)
					{
						cSharpSyntaxNode = AddError(cSharpSyntaxNode, ErrorCode.ERR_DiscardPatternInSwitchStatement);
					}
					node = ((!(cSharpSyntaxNode is PatternSyntax pattern)) ? ((SwitchLabelSyntax)_syntaxFactory.CaseSwitchLabel(keyword, (ExpressionSyntax)cSharpSyntaxNode, EatToken(SyntaxKind.ColonToken))) : ((SwitchLabelSyntax)_syntaxFactory.CasePatternSwitchLabel(keyword, pattern, ParseWhenClause(Precedence.Expression), EatToken(SyntaxKind.ColonToken))));
				}
			}
			else
			{
				node = _syntaxFactory.DefaultSwitchLabel(EatToken(SyntaxKind.DefaultKeyword), EatToken(SyntaxKind.ColonToken));
			}
			item.Add(node);
		}
		while (IsPossibleSwitchSection());
		CSharpSyntaxNode previousNode = item[item.Count - 1];
		ParseStatements(ref previousNode, syntaxListBuilder, stopOnSwitchSections: true);
		item[item.Count - 1] = (SwitchLabelSyntax)previousNode;
		return _syntaxFactory.SwitchSection(_pool.ToListAndFree(item), _pool.ToListAndFree(syntaxListBuilder));
	}

	private ThrowStatementSyntax ParseThrowStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		return _syntaxFactory.ThrowStatement(attributes, EatToken(SyntaxKind.ThrowKeyword), (base.CurrentToken.Kind != SyntaxKind.SemicolonToken) ? ParseExpressionCore() : null, EatToken(SyntaxKind.SemicolonToken));
	}

	private UnsafeStatementSyntax ParseUnsafeStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		return _syntaxFactory.UnsafeStatement(attributes, EatToken(SyntaxKind.UnsafeKeyword), ParsePossiblyAttributedBlock());
	}

	private UsingStatementSyntax ParseUsingStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, SyntaxToken awaitTokenOpt = null)
	{
		SyntaxToken usingKeyword = EatToken(SyntaxKind.UsingKeyword);
		SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
		VariableDeclarationSyntax declaration = null;
		ExpressionSyntax expression = null;
		ResetPoint resetPoint = GetResetPoint();
		ParseUsingExpression(ref declaration, ref expression, ref resetPoint);
		Release(ref resetPoint);
		return _syntaxFactory.UsingStatement(attributes, awaitTokenOpt, usingKeyword, openParenToken, declaration, expression, EatToken(SyntaxKind.CloseParenToken), ParseEmbeddedStatement());
	}

	private void ParseUsingExpression(ref VariableDeclarationSyntax declaration, ref ExpressionSyntax expression, ref ResetPoint resetPoint)
	{
		if (IsAwaitExpression())
		{
			expression = ParseExpressionCore();
			return;
		}
		ScanTypeFlags scanTypeFlags;
		if (IsQueryExpression(mayBeVariableDeclaration: true, mayBeMemberDeclaration: false))
		{
			scanTypeFlags = ScanTypeFlags.NotType;
		}
		else
		{
			SyntaxToken syntaxToken = ParsePossibleScopedKeyword(isFunctionPointerParameter: false, isLambdaParameter: false);
			if (syntaxToken != null)
			{
				declaration = ParseParenthesizedVariableDeclaration(VariableFlags.None, syntaxToken);
				return;
			}
			scanTypeFlags = ScanType();
		}
		if (scanTypeFlags == ScanTypeFlags.NullableType)
		{
			if (base.CurrentToken.Kind != SyntaxKind.IdentifierToken)
			{
				Reset(ref resetPoint);
				expression = ParseExpressionCore();
				return;
			}
			switch (PeekToken(1).Kind)
			{
			default:
				Reset(ref resetPoint);
				expression = ParseExpressionCore();
				break;
			case SyntaxKind.CloseParenToken:
			case SyntaxKind.CommaToken:
				Reset(ref resetPoint);
				declaration = ParseParenthesizedVariableDeclaration(VariableFlags.None, null);
				break;
			case SyntaxKind.EqualsToken:
				Reset(ref resetPoint);
				declaration = ParseParenthesizedVariableDeclaration(VariableFlags.None, null);
				if (base.CurrentToken.Kind == SyntaxKind.ColonToken && declaration.Type.Kind == SyntaxKind.NullableType && SyntaxFacts.IsName(((NullableTypeSyntax)declaration.Type).ElementType.Kind) && declaration.Variables.Count == 1)
				{
					Reset(ref resetPoint);
					declaration = null;
					expression = ParseExpressionCore();
				}
				break;
			}
		}
		else if (IsUsingStatementVariableDeclaration(scanTypeFlags))
		{
			Reset(ref resetPoint);
			declaration = ParseParenthesizedVariableDeclaration(VariableFlags.None, null);
		}
		else
		{
			Reset(ref resetPoint);
			expression = ParseExpressionCore();
		}
	}

	private bool IsUsingStatementVariableDeclaration(ScanTypeFlags st)
	{
		bool num = st == ScanTypeFlags.MustBeType && base.CurrentToken.Kind != SyntaxKind.DotToken;
		bool flag = st != ScanTypeFlags.NotType && base.CurrentToken.Kind == SyntaxKind.IdentifierToken;
		bool flag2 = st == ScanTypeFlags.NonGenericTypeOrExpression || PeekToken(1).Kind == SyntaxKind.EqualsToken;
		if (!num)
		{
			return flag & flag2;
		}
		return true;
	}

	private WhileStatementSyntax ParseWhileStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		return _syntaxFactory.WhileStatement(attributes, EatToken(SyntaxKind.WhileKeyword), EatToken(SyntaxKind.OpenParenToken), ParseExpressionForParenthesizedConstruct(), EatToken(SyntaxKind.CloseParenToken), ParseEmbeddedStatement());
	}

	private LabeledStatementSyntax ParseLabeledStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		return _syntaxFactory.LabeledStatement(attributes, ParseIdentifierToken(), EatToken(SyntaxKind.ColonToken), ParsePossiblyAttributedStatement() ?? SyntaxFactory.EmptyStatement(default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>), EatToken(SyntaxKind.SemicolonToken)));
	}

	private StatementSyntax ParseLocalDeclarationStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		bool flag = false;
		SyntaxToken awaitKeyword;
		SyntaxToken syntaxToken;
		if (IsPossibleAwaitUsing())
		{
			awaitKeyword = EatContextualToken(SyntaxKind.AwaitKeyword);
			syntaxToken = EatToken();
		}
		else if (base.CurrentToken.Kind == SyntaxKind.UsingKeyword)
		{
			awaitKeyword = null;
			syntaxToken = EatToken();
		}
		else
		{
			awaitKeyword = null;
			syntaxToken = null;
			flag = true;
		}
		SyntaxListBuilder syntaxListBuilder = _pool.Allocate();
		ParseLocalDeclarationStatementModifiers(syntaxListBuilder, syntaxToken != null);
		SeparatedSyntaxListBuilder<VariableDeclaratorSyntax> item = _pool.AllocateSeparated<VariableDeclaratorSyntax>();
		try
		{
			SyntaxToken syntaxToken2 = ParsePossibleScopedKeyword(isFunctionPointerParameter: false, isLambdaParameter: false);
			if (syntaxToken2 != null)
			{
				syntaxListBuilder.Add(syntaxToken2);
			}
			ParseLocalDeclaration(item, flag, stopOnCloseParen: false, attributes, syntaxListBuilder.ToList(), null, VariableFlags.None, out TypeSyntax type, out LocalFunctionStatementSyntax localFunction);
			if (localFunction != null)
			{
				return localFunction;
			}
			if (flag && attributes.Count == 0 && syntaxListBuilder.Count > 0 && IsAccessibilityModifier(((SyntaxToken)syntaxListBuilder[0]).ContextualKind))
			{
				return null;
			}
			if (syntaxToken2 != null)
			{
				syntaxListBuilder.RemoveLast();
				type = _syntaxFactory.ScopedType(syntaxToken2, type);
			}
			if (syntaxToken == null)
			{
				for (int i = 0; i < syntaxListBuilder.Count; i++)
				{
					SyntaxToken syntaxToken3 = (SyntaxToken)syntaxListBuilder[i];
					if (IsAdditionalLocalFunctionModifier(syntaxToken3.ContextualKind))
					{
						syntaxListBuilder[i] = AddError(syntaxToken3, ErrorCode.ERR_BadMemberFlag, syntaxToken3.Text);
					}
				}
			}
			return _syntaxFactory.LocalDeclarationStatement(attributes, awaitKeyword, syntaxToken, syntaxListBuilder.ToList(), _syntaxFactory.VariableDeclaration(type, item.ToList()), EatToken(SyntaxKind.SemicolonToken));
		}
		finally
		{
			_pool.Free(in item);
			_pool.Free(syntaxListBuilder);
		}
	}

	private bool IsDefiniteScopedModifier(bool isFunctionPointerParameter, bool isLambdaParameter)
	{
		if (base.CurrentToken.ContextualKind != SyntaxKind.ScopedKeyword)
		{
			return false;
		}
		if (isLambdaParameter && IsFeatureEnabled(MessageID.IDS_FeatureSimpleLambdaParameterModifiers))
		{
			return true;
		}
		using (GetDisposableResetPoint(resetOnDispose: true))
		{
			EatContextualToken(SyntaxKind.ScopedKeyword);
			if (IsParameterModifierExcludingScoped(base.CurrentToken))
			{
				return true;
			}
			return ScanType() != ScanTypeFlags.NotType && isValidScopedTypeCase();
		}
		bool isValidScopedTypeCase()
		{
			if (isFunctionPointerParameter)
			{
				SyntaxKind kind = base.CurrentToken.Kind;
				if (kind - 8216 <= SyntaxKind.List)
				{
					return true;
				}
				return false;
			}
			if (base.CurrentToken.Kind == SyntaxKind.IdentifierToken)
			{
				return true;
			}
			return false;
		}
	}

	private SyntaxToken ParsePossibleScopedKeyword(bool isFunctionPointerParameter, bool isLambdaParameter)
	{
		if (!IsDefiniteScopedModifier(isFunctionPointerParameter, isLambdaParameter))
		{
			return null;
		}
		return EatContextualToken(SyntaxKind.ScopedKeyword);
	}

	private VariableDesignationSyntax ParseDesignation(bool forPattern)
	{
		if (base.CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			SyntaxToken openParenToken = EatToken(SyntaxKind.OpenParenToken);
			SeparatedSyntaxListBuilder<VariableDesignationSyntax> item = _pool.AllocateSeparated<VariableDesignationSyntax>();
			bool flag = false;
			if (forPattern)
			{
				flag = base.CurrentToken.Kind == SyntaxKind.CloseParenToken;
			}
			else
			{
				item.Add(ParseDesignation(forPattern));
				item.AddSeparator(EatToken(SyntaxKind.CommaToken));
			}
			if (!flag)
			{
				while (true)
				{
					item.Add(ParseDesignation(forPattern));
					if (base.CurrentToken.Kind != SyntaxKind.CommaToken)
					{
						break;
					}
					item.AddSeparator(EatToken(SyntaxKind.CommaToken));
				}
			}
			return _syntaxFactory.ParenthesizedVariableDesignation(openParenToken, _pool.ToListAndFree(in item), EatToken(SyntaxKind.CloseParenToken));
		}
		return ParseSimpleDesignation();
	}

	private VariableDesignationSyntax ParseSimpleDesignation()
	{
		if (base.CurrentToken.ContextualKind != SyntaxKind.UnderscoreToken)
		{
			return _syntaxFactory.SingleVariableDesignation(EatToken(SyntaxKind.IdentifierToken));
		}
		return _syntaxFactory.DiscardDesignation(EatContextualToken(SyntaxKind.UnderscoreToken));
	}

	private WhenClauseSyntax ParseWhenClause(Precedence precedence)
	{
		if (base.CurrentToken.ContextualKind != SyntaxKind.WhenKeyword)
		{
			return null;
		}
		return _syntaxFactory.WhenClause(EatContextualToken(SyntaxKind.WhenKeyword), ParseSubExpression(precedence));
	}

	private VariableDeclarationSyntax ParseParenthesizedVariableDeclaration(VariableFlags initialFlags, SyntaxToken? scopedKeyword)
	{
		SeparatedSyntaxListBuilder<VariableDeclaratorSyntax> item = _pool.AllocateSeparated<VariableDeclaratorSyntax>();
		ParseLocalDeclaration(item, allowLocalFunctions: false, stopOnCloseParen: true, default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>), default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>), scopedKeyword, initialFlags, out TypeSyntax type, out LocalFunctionStatementSyntax _);
		return _syntaxFactory.VariableDeclaration(type, _pool.ToListAndFree(in item));
	}

	private void ParseLocalDeclaration(SeparatedSyntaxListBuilder<VariableDeclaratorSyntax> variables, bool allowLocalFunctions, bool stopOnCloseParen, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> mods, SyntaxToken? scopedKeyword, VariableFlags initialFlags, out TypeSyntax type, out LocalFunctionStatementSyntax localFunction)
	{
		type = (allowLocalFunctions ? ParseReturnType() : ParseType());
		if (scopedKeyword != null)
		{
			type = _syntaxFactory.ScopedType(scopedKeyword, type);
		}
		VariableFlags variableFlags = initialFlags | VariableFlags.LocalOrField;
		if (mods.Any(8350))
		{
			variableFlags |= VariableFlags.Const;
		}
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfDeclarationClause;
		ParseVariableDeclarators(type, variableFlags, variables, variableDeclarationsExpected: true, allowLocalFunctions, stopOnCloseParen, attributes, mods, out localFunction);
		_termState = termState;
		if (allowLocalFunctions && localFunction == null && type is PredefinedTypeSyntax predefinedTypeSyntax)
		{
			SyntaxToken keyword = predefinedTypeSyntax.Keyword;
			if (keyword != null && keyword.Kind == SyntaxKind.VoidKeyword)
			{
				type = AddError(type, ErrorCode.ERR_NoVoidHere);
			}
		}
	}

	private bool IsEndOfDeclarationClause()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind - 8211 <= SyntaxKind.List)
		{
			return true;
		}
		return false;
	}

	private void ParseLocalDeclarationStatementModifiers(SyntaxListBuilder list, bool isUsingDeclaration)
	{
		SyntaxKind contextualKind;
		while (IsDeclarationModifier(contextualKind = base.CurrentToken.ContextualKind) || IsAdditionalLocalFunctionModifier(contextualKind))
		{
			SyntaxToken syntaxToken;
			if (contextualKind == SyntaxKind.AsyncKeyword)
			{
				if (!shouldTreatAsModifier())
				{
					break;
				}
				syntaxToken = EatContextualToken(contextualKind);
			}
			else
			{
				syntaxToken = EatToken();
			}
			if (isUsingDeclaration)
			{
				syntaxToken = AddError(syntaxToken, ErrorCode.ERR_NoModifiersOnUsing);
			}
			else if ((contextualKind == SyntaxKind.ReadOnlyKeyword || contextualKind == SyntaxKind.VolatileKeyword) ? true : false)
			{
				syntaxToken = AddError(syntaxToken, ErrorCode.ERR_BadMemberFlag, syntaxToken.Text);
			}
			list.Add(syntaxToken);
		}
		bool shouldTreatAsModifier()
		{
			using (GetDisposableResetPoint(resetOnDispose: true))
			{
				do
				{
					EatToken();
					if (IsDeclarationModifier(base.CurrentToken.Kind) || IsAdditionalLocalFunctionModifier(base.CurrentToken.Kind))
					{
						return true;
					}
					using (GetDisposableResetPoint(resetOnDispose: true))
					{
						if (ScanType() != ScanTypeFlags.NotType && base.CurrentToken.Kind == SyntaxKind.IdentifierToken)
						{
							return true;
						}
					}
				}
				while (IsAdditionalLocalFunctionModifier(base.CurrentToken.ContextualKind));
				return false;
			}
		}
	}

	private static bool IsDeclarationModifier(SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.StaticKeyword:
		case SyntaxKind.ReadOnlyKeyword:
		case SyntaxKind.ConstKeyword:
		case SyntaxKind.VolatileKeyword:
			return true;
		default:
			return false;
		}
	}

	private static bool IsAdditionalLocalFunctionModifier(SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.PublicKeyword:
		case SyntaxKind.PrivateKeyword:
		case SyntaxKind.InternalKeyword:
		case SyntaxKind.ProtectedKeyword:
		case SyntaxKind.StaticKeyword:
		case SyntaxKind.ExternKeyword:
		case SyntaxKind.UnsafeKeyword:
		case SyntaxKind.AsyncKeyword:
			return true;
		default:
			return false;
		}
	}

	private static bool IsAccessibilityModifier(SyntaxKind kind)
	{
		if (kind - 8343 <= (SyntaxKind)3)
		{
			return true;
		}
		return false;
	}

	private LocalFunctionStatementSyntax TryParseLocalFunctionStatementBody(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax type, SyntaxToken identifier)
	{
		using DisposableResetPoint disposableResetPoint = GetDisposableResetPoint(resetOnDispose: false);
		bool flag = true;
		if (type.Kind == SyntaxKind.IdentifierName)
		{
			flag = ((IdentifierNameSyntax)type).Identifier.ContextualKind != SyntaxKind.AwaitKeyword;
		}
		SyntaxListBuilder syntaxListBuilder = null;
		for (int i = 0; i < modifiers.Count; i++)
		{
			SyntaxToken syntaxToken = modifiers[i];
			switch (syntaxToken.ContextualKind)
			{
			case SyntaxKind.AsyncKeyword:
				flag = true;
				continue;
			case SyntaxKind.UnsafeKeyword:
				flag = true;
				continue;
			case SyntaxKind.StaticKeyword:
			case SyntaxKind.ReadOnlyKeyword:
			case SyntaxKind.VolatileKeyword:
			case SyntaxKind.ExternKeyword:
				continue;
			}
			syntaxToken = AddError(syntaxToken, ErrorCode.ERR_BadMemberFlag, syntaxToken.Text);
			if (syntaxListBuilder == null)
			{
				syntaxListBuilder = _pool.Allocate();
				syntaxListBuilder.AddRange(modifiers);
			}
			syntaxListBuilder[i] = syntaxToken;
		}
		if (syntaxListBuilder != null)
		{
			modifiers = syntaxListBuilder.ToList();
			_pool.Free(syntaxListBuilder);
		}
		using (new ParserSyntaxContextResetter(this, modifiers.Any(8435)))
		{
			TypeParameterListSyntax typeParameterList = ParseTypeParameterList();
			ParameterListSyntax parameterListSyntax = ParseParenthesizedParameterList(forExtension: false);
			if (!flag)
			{
				Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ParameterSyntax> parameters = parameterListSyntax.Parameters;
				for (int j = 0; j < parameters.Count; j++)
				{
					flag |= !parameters[j].ContainsDiagnostics;
					if (flag)
					{
						break;
					}
				}
			}
			SyntaxListBuilder<TypeParameterConstraintClauseSyntax> syntaxListBuilder2 = default(SyntaxListBuilder<TypeParameterConstraintClauseSyntax>);
			if (base.CurrentToken.ContextualKind == SyntaxKind.WhereKeyword)
			{
				syntaxListBuilder2 = _pool.Allocate<TypeParameterConstraintClauseSyntax>();
				ParseTypeParameterConstraintClauses(syntaxListBuilder2);
				flag = true;
			}
			ParseBlockAndExpressionBodiesWithSemicolon(out var blockBody, out var expressionBody, out var semicolon, parseSemicolonAfterBlock: false);
			if (!flag && blockBody == null && expressionBody == null)
			{
				disposableResetPoint.Reset();
				return null;
			}
			return _syntaxFactory.LocalFunctionStatement(attributes, modifiers, type, identifier, typeParameterList, parameterListSyntax, syntaxListBuilder2, blockBody, expressionBody, semicolon);
		}
	}

	private ExpressionStatementSyntax ParseExpressionStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes)
	{
		return ParseExpressionStatement(attributes, ParseExpressionCore());
	}

	private ExpressionStatementSyntax ParseExpressionStatement(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes, ExpressionSyntax expression)
	{
		SyntaxToken semicolonToken = ((!base.IsScript || base.CurrentToken.Kind != SyntaxKind.EndOfFileToken) ? EatToken(SyntaxKind.SemicolonToken) : SyntaxFactory.MissingToken(SyntaxKind.SemicolonToken));
		return _syntaxFactory.ExpressionStatement(attributes, expression, semicolonToken);
	}

	public ExpressionSyntax ParseExpression()
	{
		return ParseWithStackGuard((LanguageParser @this) => @this.ParseExpressionCore(), (LanguageParser @this) => @this.CreateMissingIdentifierName());
	}

	private ExpressionSyntax ParseExpressionCore()
	{
		return ParseSubExpression(Precedence.Expression);
	}

	private bool CanStartExpression()
	{
		return IsPossibleExpression(allowBinaryExpressions: false, allowAssignmentExpressions: false);
	}

	private bool IsPossibleExpression()
	{
		return IsPossibleExpression(allowBinaryExpressions: true, allowAssignmentExpressions: true);
	}

	private bool IsPossibleExpression(bool allowBinaryExpressions, bool allowAssignmentExpressions)
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		switch (kind)
		{
		case SyntaxKind.OpenParenToken:
		case SyntaxKind.OpenBracketToken:
		case SyntaxKind.ColonColonToken:
		case SyntaxKind.TypeOfKeyword:
		case SyntaxKind.SizeOfKeyword:
		case SyntaxKind.NullKeyword:
		case SyntaxKind.TrueKeyword:
		case SyntaxKind.FalseKeyword:
		case SyntaxKind.DefaultKeyword:
		case SyntaxKind.ThrowKeyword:
		case SyntaxKind.StackAllocKeyword:
		case SyntaxKind.NewKeyword:
		case SyntaxKind.RefKeyword:
		case SyntaxKind.ArgListKeyword:
		case SyntaxKind.MakeRefKeyword:
		case SyntaxKind.RefTypeKeyword:
		case SyntaxKind.RefValueKeyword:
		case SyntaxKind.ThisKeyword:
		case SyntaxKind.BaseKeyword:
		case SyntaxKind.DelegateKeyword:
		case SyntaxKind.CheckedKeyword:
		case SyntaxKind.UncheckedKeyword:
		case SyntaxKind.InterpolatedStringStartToken:
		case SyntaxKind.InterpolatedVerbatimStringStartToken:
		case SyntaxKind.NumericLiteralToken:
		case SyntaxKind.CharacterLiteralToken:
		case SyntaxKind.StringLiteralToken:
		case SyntaxKind.InterpolatedStringToken:
		case SyntaxKind.SingleLineRawStringLiteralToken:
		case SyntaxKind.MultiLineRawStringLiteralToken:
		case SyntaxKind.Utf8StringLiteralToken:
		case SyntaxKind.Utf8SingleLineRawStringLiteralToken:
		case SyntaxKind.Utf8MultiLineRawStringLiteralToken:
		case SyntaxKind.InterpolatedSingleLineRawStringStartToken:
		case SyntaxKind.InterpolatedMultiLineRawStringStartToken:
			return true;
		case SyntaxKind.DotToken:
			if (IsAtDotDotToken())
			{
				return true;
			}
			break;
		case SyntaxKind.StaticKeyword:
			if (!IsPossibleAnonymousMethodExpression())
			{
				return IsPossibleLambdaExpression(Precedence.Expression);
			}
			return true;
		case SyntaxKind.IdentifierToken:
			if (!IsTrueIdentifier())
			{
				return base.CurrentToken.ContextualKind == SyntaxKind.FromKeyword;
			}
			return true;
		}
		if (!IsPredefinedType(kind) && !SyntaxFacts.IsAnyUnaryExpression(kind) && (!allowBinaryExpressions || !SyntaxFacts.IsBinaryExpression(kind)))
		{
			if (allowAssignmentExpressions)
			{
				return SyntaxFacts.IsAssignmentExpressionOperatorToken(kind);
			}
			return false;
		}
		return true;
	}

	private static bool IsInvalidSubExpression(SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.IfKeyword:
		case SyntaxKind.ElseKeyword:
		case SyntaxKind.WhileKeyword:
		case SyntaxKind.ForKeyword:
		case SyntaxKind.ForEachKeyword:
		case SyntaxKind.DoKeyword:
		case SyntaxKind.SwitchKeyword:
		case SyntaxKind.CaseKeyword:
		case SyntaxKind.TryKeyword:
		case SyntaxKind.CatchKeyword:
		case SyntaxKind.FinallyKeyword:
		case SyntaxKind.LockKeyword:
		case SyntaxKind.GotoKeyword:
		case SyntaxKind.BreakKeyword:
		case SyntaxKind.ContinueKeyword:
		case SyntaxKind.ReturnKeyword:
		case SyntaxKind.ConstKeyword:
		case SyntaxKind.UsingKeyword:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsRightAssociative(SyntaxKind op)
	{
		if (op == SyntaxKind.CoalesceExpression || op - 8714 <= (SyntaxKind)12)
		{
			return true;
		}
		return false;
	}

	private static Precedence GetPrecedence(SyntaxKind op)
	{
		switch (op)
		{
		case SyntaxKind.QueryExpression:
			return Precedence.Expression;
		case SyntaxKind.AnonymousMethodExpression:
		case SyntaxKind.SimpleLambdaExpression:
		case SyntaxKind.ParenthesizedLambdaExpression:
			return Precedence.Expression;
		case SyntaxKind.SimpleAssignmentExpression:
		case SyntaxKind.AddAssignmentExpression:
		case SyntaxKind.SubtractAssignmentExpression:
		case SyntaxKind.MultiplyAssignmentExpression:
		case SyntaxKind.DivideAssignmentExpression:
		case SyntaxKind.ModuloAssignmentExpression:
		case SyntaxKind.AndAssignmentExpression:
		case SyntaxKind.ExclusiveOrAssignmentExpression:
		case SyntaxKind.OrAssignmentExpression:
		case SyntaxKind.LeftShiftAssignmentExpression:
		case SyntaxKind.RightShiftAssignmentExpression:
		case SyntaxKind.CoalesceAssignmentExpression:
		case SyntaxKind.UnsignedRightShiftAssignmentExpression:
			return Precedence.Expression;
		case SyntaxKind.CoalesceExpression:
		case SyntaxKind.ThrowExpression:
			return Precedence.Coalescing;
		case SyntaxKind.LogicalOrExpression:
			return Precedence.ConditionalOr;
		case SyntaxKind.LogicalAndExpression:
			return Precedence.ConditionalAnd;
		case SyntaxKind.BitwiseOrExpression:
			return Precedence.LogicalOr;
		case SyntaxKind.ExclusiveOrExpression:
			return Precedence.LogicalXor;
		case SyntaxKind.BitwiseAndExpression:
			return Precedence.LogicalAnd;
		case SyntaxKind.EqualsExpression:
		case SyntaxKind.NotEqualsExpression:
			return Precedence.Equality;
		case SyntaxKind.IsPatternExpression:
		case SyntaxKind.LessThanExpression:
		case SyntaxKind.LessThanOrEqualExpression:
		case SyntaxKind.GreaterThanExpression:
		case SyntaxKind.GreaterThanOrEqualExpression:
		case SyntaxKind.IsExpression:
		case SyntaxKind.AsExpression:
			return Precedence.Relational;
		case SyntaxKind.SwitchExpression:
		case SyntaxKind.WithExpression:
			return Precedence.Switch;
		case SyntaxKind.LeftShiftExpression:
		case SyntaxKind.RightShiftExpression:
		case SyntaxKind.UnsignedRightShiftExpression:
			return Precedence.Shift;
		case SyntaxKind.AddExpression:
		case SyntaxKind.SubtractExpression:
			return Precedence.Additive;
		case SyntaxKind.MultiplyExpression:
		case SyntaxKind.DivideExpression:
		case SyntaxKind.ModuloExpression:
			return Precedence.Multiplicative;
		case SyntaxKind.UnaryPlusExpression:
		case SyntaxKind.UnaryMinusExpression:
		case SyntaxKind.BitwiseNotExpression:
		case SyntaxKind.LogicalNotExpression:
		case SyntaxKind.PreIncrementExpression:
		case SyntaxKind.PreDecrementExpression:
		case SyntaxKind.AwaitExpression:
		case SyntaxKind.IndexExpression:
		case SyntaxKind.TypeOfExpression:
		case SyntaxKind.SizeOfExpression:
		case SyntaxKind.CheckedExpression:
		case SyntaxKind.UncheckedExpression:
		case SyntaxKind.MakeRefExpression:
		case SyntaxKind.RefValueExpression:
		case SyntaxKind.RefTypeExpression:
			return Precedence.Unary;
		case SyntaxKind.CastExpression:
			return Precedence.Cast;
		case SyntaxKind.PointerIndirectionExpression:
			return Precedence.PointerIndirection;
		case SyntaxKind.AddressOfExpression:
			return Precedence.AddressOf;
		case SyntaxKind.RangeExpression:
			return Precedence.Range;
		case SyntaxKind.ConditionalExpression:
			return Precedence.Expression;
		case SyntaxKind.IdentifierName:
		case SyntaxKind.GenericName:
		case SyntaxKind.AliasQualifiedName:
		case SyntaxKind.PredefinedType:
		case SyntaxKind.ParenthesizedExpression:
		case SyntaxKind.InvocationExpression:
		case SyntaxKind.ElementAccessExpression:
		case SyntaxKind.ObjectCreationExpression:
		case SyntaxKind.AnonymousObjectCreationExpression:
		case SyntaxKind.ArrayCreationExpression:
		case SyntaxKind.ImplicitArrayCreationExpression:
		case SyntaxKind.StackAllocArrayCreationExpression:
		case SyntaxKind.InterpolatedStringExpression:
		case SyntaxKind.ImplicitObjectCreationExpression:
		case SyntaxKind.SimpleMemberAccessExpression:
		case SyntaxKind.PointerMemberAccessExpression:
		case SyntaxKind.ConditionalAccessExpression:
		case SyntaxKind.PostIncrementExpression:
		case SyntaxKind.PostDecrementExpression:
		case SyntaxKind.ThisExpression:
		case SyntaxKind.BaseExpression:
		case SyntaxKind.ArgListExpression:
		case SyntaxKind.NumericLiteralExpression:
		case SyntaxKind.StringLiteralExpression:
		case SyntaxKind.CharacterLiteralExpression:
		case SyntaxKind.TrueLiteralExpression:
		case SyntaxKind.FalseLiteralExpression:
		case SyntaxKind.NullLiteralExpression:
		case SyntaxKind.DefaultLiteralExpression:
		case SyntaxKind.Utf8StringLiteralExpression:
		case SyntaxKind.FieldExpression:
		case SyntaxKind.DefaultExpression:
		case SyntaxKind.TupleExpression:
		case SyntaxKind.DeclarationExpression:
		case SyntaxKind.RefExpression:
		case SyntaxKind.ImplicitStackAllocArrayCreationExpression:
		case SyntaxKind.SuppressNullableWarningExpression:
		case SyntaxKind.CollectionExpression:
			return Precedence.Primary;
		default:
			throw ExceptionUtilities.UnexpectedValue(op);
		}
	}

	private static bool IsExpectedPrefixUnaryOperator(SyntaxKind kind)
	{
		if (SyntaxFacts.IsPrefixUnaryExpression(kind))
		{
			if (kind != SyntaxKind.RefKeyword)
			{
				return kind != SyntaxKind.OutKeyword;
			}
			return false;
		}
		return false;
	}

	private static bool IsExpectedBinaryOperator(SyntaxKind kind)
	{
		return SyntaxFacts.IsBinaryExpression(kind);
	}

	private static bool IsExpectedAssignmentOperator(SyntaxKind kind)
	{
		return SyntaxFacts.IsAssignmentExpressionOperatorToken(kind);
	}

	private bool IsPossibleAwaitExpressionStatement()
	{
		if (base.IsScript || IsInAsync)
		{
			return base.CurrentToken.ContextualKind == SyntaxKind.AwaitKeyword;
		}
		return false;
	}

	private bool IsAwaitExpression()
	{
		if (base.CurrentToken.ContextualKind == SyntaxKind.AwaitKeyword)
		{
			if (IsInAsync)
			{
				return true;
			}
			SyntaxToken syntaxToken = PeekToken(1);
			switch (syntaxToken.Kind)
			{
			case SyntaxKind.IdentifierToken:
				return syntaxToken.ContextualKind != SyntaxKind.WithKeyword;
			case SyntaxKind.TypeOfKeyword:
			case SyntaxKind.NullKeyword:
			case SyntaxKind.TrueKeyword:
			case SyntaxKind.FalseKeyword:
			case SyntaxKind.DefaultKeyword:
			case SyntaxKind.NewKeyword:
			case SyntaxKind.ThisKeyword:
			case SyntaxKind.BaseKeyword:
			case SyntaxKind.DelegateKeyword:
			case SyntaxKind.CheckedKeyword:
			case SyntaxKind.UncheckedKeyword:
			case SyntaxKind.InterpolatedStringStartToken:
			case SyntaxKind.InterpolatedVerbatimStringStartToken:
			case SyntaxKind.NumericLiteralToken:
			case SyntaxKind.CharacterLiteralToken:
			case SyntaxKind.StringLiteralToken:
			case SyntaxKind.InterpolatedStringToken:
			case SyntaxKind.SingleLineRawStringLiteralToken:
			case SyntaxKind.MultiLineRawStringLiteralToken:
			case SyntaxKind.Utf8StringLiteralToken:
			case SyntaxKind.Utf8SingleLineRawStringLiteralToken:
			case SyntaxKind.Utf8MultiLineRawStringLiteralToken:
			case SyntaxKind.InterpolatedSingleLineRawStringStartToken:
			case SyntaxKind.InterpolatedMultiLineRawStringStartToken:
				return true;
			}
		}
		return false;
	}

	private ExpressionSyntax ParseSubExpression(Precedence precedence)
	{
		_recursionDepth++;
		StackGuard.EnsureSufficientExecutionStack(_recursionDepth);
		ExpressionSyntax result = ParseSubExpressionCore(precedence);
		_recursionDepth--;
		return result;
	}

	private ExpressionSyntax ParseSubExpressionCore(Precedence precedence)
	{
		SyntaxKind tk = base.CurrentToken.Kind;
		if (IsInvalidSubExpression(tk))
		{
			return AddError(CreateMissingIdentifierName(), ErrorCode.ERR_InvalidExprTerm, SyntaxFacts.GetText(tk));
		}
		return ParseExpressionContinued(parseUnaryOrPrimaryExpression(precedence), precedence);
		ExpressionSyntax parseUnaryOrPrimaryExpression(Precedence precedence2)
		{
			if (IsExpectedPrefixUnaryOperator(tk))
			{
				SyntaxKind prefixUnaryExpression = SyntaxFacts.GetPrefixUnaryExpression(tk);
				return _syntaxFactory.PrefixUnaryExpression(prefixUnaryExpression, EatToken(), ParseSubExpression(GetPrecedence(prefixUnaryExpression)));
			}
			if (IsAtDotDotToken())
			{
				return _syntaxFactory.RangeExpression(null, EatDotDotToken(), CanStartExpression() ? ParseSubExpression(Precedence.Range) : null);
			}
			if (IsAwaitExpression())
			{
				return _syntaxFactory.AwaitExpression(EatContextualToken(SyntaxKind.AwaitKeyword), ParseSubExpression(GetPrecedence(SyntaxKind.AwaitExpression)));
			}
			if (IsQueryExpression(mayBeVariableDeclaration: false, mayBeMemberDeclaration: false))
			{
				return ParseQueryExpression(precedence2);
			}
			if (base.CurrentToken.ContextualKind == SyntaxKind.FromKeyword && IsInQuery)
			{
				return AddTrailingSkippedSyntax(CreateMissingIdentifierName(), AddError(EatToken(), ErrorCode.ERR_InvalidExprTerm, base.CurrentToken.Text));
			}
			if (tk == SyntaxKind.ThrowKeyword)
			{
				ExpressionSyntax expressionSyntax = ParseThrowExpression();
				if (precedence2 > Precedence.Coalescing)
				{
					return AddError(expressionSyntax, ErrorCode.ERR_InvalidExprTerm, SyntaxFacts.GetText(tk));
				}
				return expressionSyntax;
			}
			if (IsPossibleDeconstructionLeft(precedence2))
			{
				return ParseDeclarationExpression(ParseTypeMode.Normal, isScoped: false);
			}
			return ParsePrimaryExpression(precedence2);
		}
	}

	private ExpressionSyntax ParseExpressionContinued(ExpressionSyntax unaryOrPrimaryExpression, Precedence precedence)
	{
		ExpressionSyntax expressionSyntax = unaryOrPrimaryExpression;
		while (true)
		{
			ExpressionSyntax expressionSyntax2 = tryExpandExpression(expressionSyntax, precedence);
			if (expressionSyntax2 == null)
			{
				break;
			}
			expressionSyntax = expressionSyntax2;
		}
		if (base.CurrentToken.Kind == SyntaxKind.QuestionToken && precedence <= Precedence.Conditional)
		{
			return consumeConditionalExpression(expressionSyntax);
		}
		return expressionSyntax;
		ConditionalExpressionSyntax consumeConditionalExpression(ExpressionSyntax leftOperand)
		{
			SyntaxToken questionToken = EatToken();
			using DisposableResetPoint disposableResetPoint = GetDisposableResetPoint(resetOnDispose: false);
			ExpressionSyntax expressionSyntax3 = ParsePossibleRefExpression();
			if (base.CurrentToken.Kind != SyntaxKind.ColonToken && !ForceConditionalAccessExpression && containsTernaryCollectionToReinterpret(expressionSyntax3))
			{
				using DisposableResetPoint disposableResetPoint2 = GetDisposableResetPoint(resetOnDispose: false);
				disposableResetPoint.Reset();
				bool? forceConditionalAccessExpression = true;
				ExpressionSyntax expressionSyntax4;
				using (new ParserSyntaxContextResetter(this, null, null, null, forceConditionalAccessExpression))
				{
					expressionSyntax4 = ParsePossibleRefExpression();
				}
				if (base.CurrentToken.Kind == SyntaxKind.ColonToken)
				{
					expressionSyntax3 = expressionSyntax4;
				}
				else
				{
					disposableResetPoint2.Reset();
				}
			}
			if (base.CurrentToken.Kind == SyntaxKind.EndOfFileToken && lexer.InterpolationFollowedByColon)
			{
				ConditionalExpressionSyntax node = _syntaxFactory.ConditionalExpression(leftOperand, questionToken, expressionSyntax3, SyntaxFactory.MissingToken(SyntaxKind.ColonToken), _syntaxFactory.IdentifierName(SyntaxFactory.MissingToken(SyntaxKind.IdentifierToken)));
				return AddError(node, ErrorCode.ERR_ConditionalInInterpolation);
			}
			return _syntaxFactory.ConditionalExpression(leftOperand, questionToken, expressionSyntax3, EatToken(SyntaxKind.ColonToken), ParsePossibleRefExpression());
		}
		static bool containsTernaryCollectionToReinterpret(ExpressionSyntax expression)
		{
			ArrayBuilder<GreenNode> instance = ArrayBuilder<GreenNode>.GetInstance();
			instance.Push(expression);
			while (instance.Count > 0)
			{
				GreenNode greenNode = instance.Pop();
				if (greenNode is ConditionalExpressionSyntax conditionalExpressionSyntax && conditionalExpressionSyntax.WhenTrue.GetFirstToken().Kind == SyntaxKind.OpenBracketToken)
				{
					instance.Free();
					return true;
				}
				foreach (GreenNode item in greenNode.ChildNodesAndTokens())
				{
					instance.Push(item);
				}
			}
			instance.Free();
			return false;
		}
		ExpressionSyntax? tryExpandExpression(ExpressionSyntax leftOperand, Precedence precedence3)
		{
			var (syntaxKind, syntaxKind2) = GetExpressionOperatorTokenKindAndExpressionKind();
			if (syntaxKind == SyntaxKind.None)
			{
				return null;
			}
			Precedence precedence2 = GetPrecedence(syntaxKind2);
			if (precedence2 < precedence3)
			{
				return null;
			}
			if (precedence2 == precedence3 && !IsRightAssociative(syntaxKind2))
			{
				return null;
			}
			SyntaxToken syntaxToken = EatExpressionOperatorToken(syntaxKind);
			if (precedence2 > GetPrecedence(leftOperand.Kind))
			{
				syntaxToken = AddError(syntaxToken, (leftOperand.Kind == SyntaxKind.IsPatternExpression) ? ErrorCode.ERR_UnexpectedToken : ErrorCode.WRN_PrecedenceInversion, syntaxToken.Text);
			}
			switch (syntaxKind2)
			{
			case SyntaxKind.AsExpression:
				return _syntaxFactory.BinaryExpression(syntaxKind2, leftOperand, syntaxToken, ParseType(ParseTypeMode.AsExpression));
			case SyntaxKind.IsExpression:
				return ParseIsExpression(leftOperand, syntaxToken);
			case SyntaxKind.SwitchExpression:
				return ParseSwitchExpression(leftOperand, syntaxToken);
			case SyntaxKind.WithExpression:
				return ParseWithExpression(leftOperand, syntaxToken);
			case SyntaxKind.RangeExpression:
				return _syntaxFactory.RangeExpression(leftOperand, syntaxToken, CanStartExpression() ? ParseSubExpression(Precedence.Range) : null);
			default:
				if (IsExpectedAssignmentOperator(syntaxToken.Kind))
				{
					return ParseAssignmentExpression(syntaxKind2, leftOperand, syntaxToken);
				}
				if (IsExpectedBinaryOperator(syntaxToken.Kind))
				{
					return _syntaxFactory.BinaryExpression(syntaxKind2, leftOperand, syntaxToken, ParseSubExpression(precedence2));
				}
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Parser/LanguageParser.cs", 11579);
			}
		}
	}

	private (SyntaxKind operatorTokenKind, SyntaxKind operatorExpressionKind) GetExpressionOperatorTokenKindAndExpressionKind()
	{
		SyntaxToken currentToken = base.CurrentToken;
		SyntaxKind syntaxKind = currentToken.ContextualKind;
		if (IsAtDotDotToken())
		{
			return (operatorTokenKind: SyntaxKind.DotDotToken, operatorExpressionKind: SyntaxKind.RangeExpression);
		}
		bool flag = syntaxKind == SyntaxKind.GreaterThanToken;
		SyntaxToken syntaxToken = default(SyntaxToken);
		bool flag2;
		if (flag)
		{
			syntaxToken = PeekToken(1);
			if (syntaxToken != null)
			{
				SyntaxKind kind = syntaxToken.Kind;
				if (kind == SyntaxKind.GreaterThanToken || kind == SyntaxKind.GreaterThanEqualsToken)
				{
					flag2 = true;
					goto IL_005f;
				}
			}
			flag2 = false;
			goto IL_005f;
		}
		goto IL_0062;
		IL_00a9:
		SyntaxToken syntaxToken2;
		syntaxKind = ((!flag || !NoTriviaBetween(syntaxToken, syntaxToken2)) ? SyntaxKind.GreaterThanGreaterThanToken : ((syntaxToken2.Kind == SyntaxKind.GreaterThanToken) ? SyntaxKind.GreaterThanGreaterThanGreaterThanToken : SyntaxKind.GreaterThanGreaterThanGreaterThanEqualsToken));
		goto IL_00e1;
		IL_005f:
		flag = flag2;
		goto IL_0062;
		IL_00e1:
		if (IsExpectedBinaryOperator(syntaxKind))
		{
			return (operatorTokenKind: syntaxKind, operatorExpressionKind: SyntaxFacts.GetBinaryExpression(syntaxKind));
		}
		if (IsExpectedAssignmentOperator(syntaxKind))
		{
			return (operatorTokenKind: syntaxKind, operatorExpressionKind: SyntaxFacts.GetAssignmentExpression(syntaxKind));
		}
		if (syntaxKind == SyntaxKind.SwitchKeyword && PeekToken(1).Kind == SyntaxKind.OpenBraceToken)
		{
			return (operatorTokenKind: syntaxKind, operatorExpressionKind: SyntaxKind.SwitchExpression);
		}
		if (syntaxKind == SyntaxKind.WithKeyword && PeekToken(1).Kind == SyntaxKind.OpenBraceToken)
		{
			return (operatorTokenKind: syntaxKind, operatorExpressionKind: SyntaxKind.WithExpression);
		}
		return (operatorTokenKind: SyntaxKind.None, operatorExpressionKind: SyntaxKind.None);
		IL_0062:
		if (flag && NoTriviaBetween(currentToken, syntaxToken))
		{
			if (syntaxToken.Kind == SyntaxKind.GreaterThanToken)
			{
				syntaxToken2 = PeekToken(2);
				if (syntaxToken2 != null)
				{
					SyntaxKind kind = syntaxToken2.Kind;
					if (kind == SyntaxKind.GreaterThanToken || kind == SyntaxKind.GreaterThanEqualsToken)
					{
						flag = true;
						goto IL_00a9;
					}
				}
				flag = false;
				goto IL_00a9;
			}
			syntaxKind = SyntaxKind.GreaterThanGreaterThanEqualsToken;
		}
		goto IL_00e1;
	}

	private SyntaxToken EatExpressionOperatorToken(SyntaxKind operatorTokenKind)
	{
		bool flag;
		switch (operatorTokenKind)
		{
		case SyntaxKind.DotDotToken:
			return EatDotDotToken();
		case SyntaxKind.GreaterThanGreaterThanToken:
		case SyntaxKind.GreaterThanGreaterThanEqualsToken:
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			SyntaxToken syntaxToken = EatToken();
			return SyntaxFactory.Token(trailing: EatToken().GetTrailingTrivia(), leading: syntaxToken.GetLeadingTrivia(), kind: operatorTokenKind);
		}
		if (operatorTokenKind - 8286 <= SyntaxKind.List)
		{
			SyntaxToken syntaxToken2 = EatToken();
			EatToken();
			return SyntaxFactory.Token(trailing: EatToken().GetTrailingTrivia(), leading: syntaxToken2.GetLeadingTrivia(), kind: operatorTokenKind);
		}
		return EatContextualToken(operatorTokenKind);
	}

	private AssignmentExpressionSyntax ParseAssignmentExpression(SyntaxKind operatorExpressionKind, ExpressionSyntax leftOperand, SyntaxToken operatorToken)
	{
		ExpressionSyntax right = ((operatorExpressionKind != SyntaxKind.SimpleAssignmentExpression || base.CurrentToken.Kind != SyntaxKind.RefKeyword || IsPossibleLambdaExpression(Precedence.Expression)) ? ParseSubExpression(Precedence.Expression) : _syntaxFactory.RefExpression(EatToken(), ParseExpressionCore()));
		return _syntaxFactory.AssignmentExpression(operatorExpressionKind, leftOperand, operatorToken, right);
	}

	public bool IsAtDotDotToken()
	{
		if (base.CurrentToken.Kind != SyntaxKind.DotToken)
		{
			return false;
		}
		SyntaxToken syntaxToken = PeekToken(1);
		if (syntaxToken.Kind == SyntaxKind.DotToken)
		{
			return NoTriviaBetween(base.CurrentToken, syntaxToken);
		}
		return false;
	}

	public static bool IsAtDotDotToken(SyntaxToken token1, SyntaxToken token2)
	{
		if (token1.Kind == SyntaxKind.DotToken && token2.Kind == SyntaxKind.DotToken)
		{
			return NoTriviaBetween(token1, token2);
		}
		return false;
	}

	public SyntaxToken EatDotDotToken()
	{
		SyntaxToken syntaxToken = EatToken();
		SyntaxToken syntaxToken2 = EatToken();
		SyntaxToken syntaxToken3 = SyntaxFactory.Token(syntaxToken.GetLeadingTrivia(), SyntaxKind.DotDotToken, syntaxToken2.GetTrailingTrivia());
		SyntaxToken currentToken = base.CurrentToken;
		if (currentToken != null && currentToken.Kind == SyntaxKind.DotToken && NoTriviaBetween(syntaxToken2, currentToken))
		{
			syntaxToken3 = AddError(syntaxToken3, syntaxToken3.GetLeadingTriviaWidth(), 0, ErrorCode.ERR_TripleDotNotAllowed);
			SyntaxToken syntaxToken4 = PeekToken(1);
			if (syntaxToken4 == null || syntaxToken4.Kind != SyntaxKind.DotToken || !NoTriviaBetween(currentToken, syntaxToken4))
			{
				syntaxToken3 = AddSkippedSyntax(syntaxToken3, EatToken(), trailing: true);
			}
		}
		return syntaxToken3;
	}

	private DeclarationExpressionSyntax ParseDeclarationExpression(ParseTypeMode mode, bool isScoped)
	{
		SyntaxToken syntaxToken = (isScoped ? EatContextualToken(SyntaxKind.ScopedKeyword) : null);
		TypeSyntax typeSyntax = ParseType(mode);
		return _syntaxFactory.DeclarationExpression((syntaxToken == null) ? typeSyntax : _syntaxFactory.ScopedType(syntaxToken, typeSyntax), ParseDesignation(forPattern: false));
	}

	private ExpressionSyntax ParseThrowExpression()
	{
		return _syntaxFactory.ThrowExpression(EatToken(SyntaxKind.ThrowKeyword), ParseSubExpression(Precedence.Coalescing));
	}

	private ExpressionSyntax ParseIsExpression(ExpressionSyntax leftOperand, SyntaxToken opToken)
	{
		CSharpSyntaxNode cSharpSyntaxNode = ParseTypeOrPatternForIsOperator();
		if (!(cSharpSyntaxNode is PatternSyntax pattern))
		{
			if (cSharpSyntaxNode is TypeSyntax right)
			{
				return _syntaxFactory.BinaryExpression(SyntaxKind.IsExpression, leftOperand, opToken, right);
			}
			throw ExceptionUtilities.UnexpectedValue(cSharpSyntaxNode);
		}
		return _syntaxFactory.IsPatternExpression(leftOperand, opToken, pattern);
	}

	private ExpressionSyntax ParsePrimaryExpression(Precedence precedence)
	{
		return parsePostFixExpression(parsePrimaryExpressionWithoutPostfix(precedence));
		ExpressionSyntax parsePostFixExpression(ExpressionSyntax expr)
		{
			while (true)
			{
				switch (base.CurrentToken.Kind)
				{
				case SyntaxKind.OpenParenToken:
					expr = _syntaxFactory.InvocationExpression(expr, ParseParenthesizedArgumentList());
					continue;
				case SyntaxKind.OpenBracketToken:
					expr = _syntaxFactory.ElementAccessExpression(expr, ParseBracketedArgumentList());
					continue;
				case SyntaxKind.MinusMinusToken:
				case SyntaxKind.PlusPlusToken:
					expr = _syntaxFactory.PostfixUnaryExpression(SyntaxFacts.GetPostfixUnaryExpression(base.CurrentToken.Kind), expr, EatToken());
					continue;
				case SyntaxKind.ColonColonToken:
					expr = ((PeekToken(1).Kind != SyntaxKind.IdentifierToken) ? AddTrailingSkippedSyntax(expr, EatTokenEvenWithIncorrectKind(SyntaxKind.DotToken)) : _syntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expr, ConvertToMissingWithTrailingTrivia(AddError(EatToken(), ErrorCode.ERR_UnexpectedAliasedName), SyntaxKind.DotToken), ParseSimpleName(NameOptions.InExpression)));
					continue;
				case SyntaxKind.MinusGreaterThanToken:
					expr = _syntaxFactory.MemberAccessExpression(SyntaxKind.PointerMemberAccessExpression, expr, EatToken(), ParseSimpleName(NameOptions.InExpression));
					continue;
				case SyntaxKind.DotToken:
					if (!IsAtDotDotToken())
					{
						if (base.CurrentToken.TrailingTrivia.Any(8539) && PeekToken(1).Kind == SyntaxKind.IdentifierToken && PeekToken(2).ContextualKind == SyntaxKind.IdentifierToken)
						{
							return _syntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expr, EatToken(), AddError(CreateMissingIdentifierName(), ErrorCode.ERR_IdentifierExpected));
						}
						expr = _syntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expr, EatToken(), ParseSimpleName(NameOptions.InExpression));
						continue;
					}
					break;
				case SyntaxKind.QuestionToken:
				{
					if (TryParseConditionalAccessExpression(expr, out ConditionalAccessExpressionSyntax conditionalAccessExpression))
					{
						expr = conditionalAccessExpression;
						continue;
					}
					return expr;
				}
				case SyntaxKind.ExclamationToken:
					expr = _syntaxFactory.PostfixUnaryExpression(SyntaxKind.SuppressNullableWarningExpression, expr, EatToken());
					continue;
				}
				break;
			}
			return expr;
		}
		ExpressionSyntax parsePrimaryExpressionWithoutPostfix(Precedence precedence2)
		{
			SyntaxKind kind = base.CurrentToken.Kind;
			switch (kind)
			{
			case SyntaxKind.TypeOfKeyword:
				return ParseTypeOfExpression();
			case SyntaxKind.DefaultKeyword:
				return ParseDefaultExpression();
			case SyntaxKind.SizeOfKeyword:
				return ParseSizeOfExpression();
			case SyntaxKind.MakeRefKeyword:
				return ParseMakeRefExpression();
			case SyntaxKind.RefTypeKeyword:
				return ParseRefTypeExpression();
			case SyntaxKind.CheckedKeyword:
			case SyntaxKind.UncheckedKeyword:
				return ParseCheckedOrUncheckedExpression();
			case SyntaxKind.RefValueKeyword:
				return ParseRefValueExpression();
			case SyntaxKind.ColonColonToken:
				return ParseAliasQualifiedName(NameOptions.InExpression);
			case SyntaxKind.EqualsGreaterThanToken:
				return ParseLambdaExpression();
			case SyntaxKind.StaticKeyword:
				if (IsPossibleAnonymousMethodExpression())
				{
					return ParseAnonymousMethodExpression();
				}
				if (IsPossibleLambdaExpression(precedence2))
				{
					return ParseLambdaExpression();
				}
				return AddError(CreateMissingIdentifierName(), ErrorCode.ERR_InvalidExprTerm, base.CurrentToken.Text);
			case SyntaxKind.IdentifierToken:
				if (IsTrueIdentifier())
				{
					if (IsPossibleAnonymousMethodExpression())
					{
						return ParseAnonymousMethodExpression();
					}
					if (IsPossibleLambdaExpression(precedence2))
					{
						LambdaExpressionSyntax lambdaExpressionSyntax = TryParseLambdaExpression();
						if (lambdaExpressionSyntax != null)
						{
							return lambdaExpressionSyntax;
						}
					}
					if (IsPossibleDeconstructionLeft(precedence2))
					{
						return ParseDeclarationExpression(ParseTypeMode.Normal, isScoped: false);
					}
					if (IsCurrentTokenFieldInKeywordContext() && PeekToken(1).Kind != SyntaxKind.ColonColonToken)
					{
						return _syntaxFactory.FieldExpression(EatContextualToken(SyntaxKind.FieldKeyword));
					}
					return ParseAliasQualifiedName(NameOptions.InExpression);
				}
				return AddError(CreateMissingIdentifierName(), ErrorCode.ERR_InvalidExprTerm, base.CurrentToken.Text);
			case SyntaxKind.OpenBracketToken:
				if (!IsPossibleLambdaExpression(precedence2))
				{
					return ParseCollectionExpression();
				}
				return ParseLambdaExpression();
			case SyntaxKind.ThisKeyword:
				return _syntaxFactory.ThisExpression(EatToken());
			case SyntaxKind.BaseKeyword:
				return ParseBaseExpression();
			case SyntaxKind.NullKeyword:
			case SyntaxKind.TrueKeyword:
			case SyntaxKind.FalseKeyword:
			case SyntaxKind.ArgListKeyword:
			case SyntaxKind.NumericLiteralToken:
			case SyntaxKind.CharacterLiteralToken:
			case SyntaxKind.StringLiteralToken:
			case SyntaxKind.Utf8StringLiteralToken:
				return _syntaxFactory.LiteralExpression(SyntaxFacts.GetLiteralExpression(kind), EatToken());
			case SyntaxKind.InterpolatedStringStartToken:
			case SyntaxKind.InterpolatedVerbatimStringStartToken:
			case SyntaxKind.InterpolatedSingleLineRawStringStartToken:
			case SyntaxKind.InterpolatedMultiLineRawStringStartToken:
				throw new NotImplementedException();
			case SyntaxKind.InterpolatedStringToken:
				return ParseInterpolatedStringToken();
			case SyntaxKind.SingleLineRawStringLiteralToken:
			case SyntaxKind.MultiLineRawStringLiteralToken:
			case SyntaxKind.Utf8SingleLineRawStringLiteralToken:
			case SyntaxKind.Utf8MultiLineRawStringLiteralToken:
				return ParseRawStringToken();
			case SyntaxKind.OpenParenToken:
				if (IsPossibleLambdaExpression(precedence2))
				{
					LambdaExpressionSyntax lambdaExpressionSyntax2 = TryParseLambdaExpression();
					if (lambdaExpressionSyntax2 != null)
					{
						return lambdaExpressionSyntax2;
					}
				}
				return ParseCastOrParenExpressionOrTuple();
			case SyntaxKind.NewKeyword:
				return ParseNewExpression();
			case SyntaxKind.StackAllocKeyword:
				return ParseStackAllocExpression();
			case SyntaxKind.DelegateKeyword:
				if (!IsPossibleLambdaExpression(precedence2))
				{
					return ParseAnonymousMethodExpression();
				}
				return ParseLambdaExpression();
			case SyntaxKind.RefKeyword:
			{
				if (IsPossibleLambdaExpression(precedence2))
				{
					return ParseLambdaExpression();
				}
				SyntaxToken refKeyword = EatToken();
				return AddError(_syntaxFactory.RefExpression(refKeyword, ParseExpressionCore()), ErrorCode.ERR_InvalidExprTerm, SyntaxFacts.GetText(kind));
			}
			default:
			{
				if (IsPredefinedType(kind))
				{
					if (IsPossibleLambdaExpression(precedence2))
					{
						return ParseLambdaExpression();
					}
					PredefinedTypeSyntax predefinedTypeSyntax = _syntaxFactory.PredefinedType(EatToken());
					if (base.CurrentToken.Kind != SyntaxKind.DotToken || kind == SyntaxKind.VoidKeyword)
					{
						predefinedTypeSyntax = AddError(predefinedTypeSyntax, ErrorCode.ERR_InvalidExprTerm, SyntaxFacts.GetText(kind));
					}
					return predefinedTypeSyntax;
				}
				IdentifierNameSyntax identifierNameSyntax = CreateMissingIdentifierName();
				if (kind == SyntaxKind.EndOfFileToken)
				{
					return AddError(identifierNameSyntax, ErrorCode.ERR_ExpressionExpected);
				}
				if (SyntaxFacts.IsBinaryExpression(kind) || SyntaxFacts.IsAssignmentExpressionOperatorToken(kind))
				{
					return WithAdditionalDiagnostics(identifierNameSyntax, SyntaxParser.MakeError(base.CurrentToken.GetLeadingTriviaWidth(), base.CurrentToken.Width, ErrorCode.ERR_InvalidExprTerm, SyntaxFacts.GetText(kind)));
				}
				return AddError(identifierNameSyntax, ErrorCode.ERR_InvalidExprTerm, SyntaxFacts.GetText(kind));
			}
			}
		}
	}

	private ExpressionSyntax ParseBaseExpression()
	{
		return _syntaxFactory.BaseExpression(EatToken());
	}

	private bool IsPossibleDeconstructionLeft(Precedence precedence)
	{
		if (precedence != Precedence.Expression || (!base.CurrentToken.IsIdentifierVar() && !IsPredefinedType(base.CurrentToken.Kind)))
		{
			return false;
		}
		using (GetDisposableResetPoint(resetOnDispose: true))
		{
			EatToken();
			return base.CurrentToken.Kind == SyntaxKind.OpenParenToken && ScanDesignator() && base.CurrentToken.Kind == SyntaxKind.EqualsToken;
		}
	}

	private bool ScanDesignator()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			if (kind == SyntaxKind.IdentifierToken && IsTrueIdentifier())
			{
				EatToken();
				return true;
			}
			return false;
		}
		while (true)
		{
			EatToken();
			if (!ScanDesignator())
			{
				break;
			}
			switch (base.CurrentToken.Kind)
			{
			case SyntaxKind.CommaToken:
				break;
			case SyntaxKind.CloseParenToken:
				EatToken();
				return true;
			default:
				return false;
			}
		}
		return false;
	}

	private bool IsPossibleAnonymousMethodExpression()
	{
		int i;
		for (i = 0; PeekToken(i).Kind == SyntaxKind.StaticKeyword || PeekToken(i).ContextualKind == SyntaxKind.AsyncKeyword; i++)
		{
		}
		if (PeekToken(i).Kind == SyntaxKind.DelegateKeyword)
		{
			return PeekToken(i + 1).Kind != SyntaxKind.AsteriskToken;
		}
		return false;
	}

	private bool TryParseConditionalAccessExpression(ExpressionSyntax primaryExpression, [NotNullWhen(true)] out ConditionalAccessExpressionSyntax? conditionalAccessExpression)
	{
		var (syntaxToken, expressionSyntax) = tryEatQuestionAndBindingExpression();
		if (syntaxToken == null || expressionSyntax == null)
		{
			conditionalAccessExpression = null;
			return false;
		}
		conditionalAccessExpression = _syntaxFactory.ConditionalAccessExpression(primaryExpression, syntaxToken, parseWhenNotNull(expressionSyntax));
		return true;
		bool isStartOfElementBindingExpression(SyntaxKind nextTokenKind)
		{
			if (nextTokenKind != SyntaxKind.OpenBracketToken)
			{
				return false;
			}
			if (ForceConditionalAccessExpression)
			{
				return true;
			}
			using (GetDisposableResetPoint(resetOnDispose: true))
			{
				EatToken();
				ParsePossibleRefExpression();
				return base.CurrentToken.Kind != SyntaxKind.ColonToken;
			}
		}
		ExpressionSyntax parseWhenNotNull(ExpressionSyntax expr)
		{
			while (true)
			{
				using DisposableResetPoint disposableResetPoint = GetDisposableResetPoint(resetOnDispose: false);
				ExpressionSyntax result = expr;
				while (base.CurrentToken.Kind == SyntaxKind.ExclamationToken)
				{
					expr = _syntaxFactory.PostfixUnaryExpression(SyntaxKind.SuppressNullableWarningExpression, expr, EatToken());
				}
				ExpressionSyntax expressionSyntax2 = tryParseDependentAccess(expr);
				if (expressionSyntax2 == null)
				{
					if (TryParseConditionalAccessExpression(expr, out ConditionalAccessExpressionSyntax conditionalAccessExpression2))
					{
						return conditionalAccessExpression2;
					}
					var (syntaxKind, operatorExpressionKind) = GetExpressionOperatorTokenKindAndExpressionKind();
					if (IsExpectedAssignmentOperator(syntaxKind))
					{
						return ParseAssignmentExpression(operatorExpressionKind, expr, EatExpressionOperatorToken(syntaxKind));
					}
					disposableResetPoint.Reset();
					return result;
				}
				expr = expressionSyntax2;
			}
		}
		(SyntaxToken? questionToken, ExpressionSyntax? bindingExpression) tryEatQuestionAndBindingExpression()
		{
			if (base.CurrentToken.Kind == SyntaxKind.QuestionToken)
			{
				SyntaxToken syntaxToken2 = PeekToken(1);
				SyntaxKind kind = syntaxToken2.Kind;
				if (kind == SyntaxKind.DotToken && !IsAtDotDotToken(syntaxToken2, PeekToken(2)))
				{
					return (questionToken: EatToken(), bindingExpression: _syntaxFactory.MemberBindingExpression(EatToken(), ParseSimpleName(NameOptions.InExpression)));
				}
				if (isStartOfElementBindingExpression(kind))
				{
					return (questionToken: EatToken(), bindingExpression: _syntaxFactory.ElementBindingExpression(ParseBracketedArgumentList()));
				}
			}
			return default((SyntaxToken, ExpressionSyntax));
		}
		ExpressionSyntax? tryParseDependentAccess(ExpressionSyntax expr)
		{
			return base.CurrentToken.Kind switch
			{
				SyntaxKind.OpenParenToken => _syntaxFactory.InvocationExpression(expr, ParseParenthesizedArgumentList()), 
				SyntaxKind.OpenBracketToken => _syntaxFactory.ElementAccessExpression(expr, ParseBracketedArgumentList()), 
				SyntaxKind.DotToken => _syntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expr, EatToken(), ParseSimpleName(NameOptions.InExpression)), 
				_ => null, 
			};
		}
	}

	internal ArgumentListSyntax ParseParenthesizedArgumentList()
	{
		if (IsIncrementalAndFactoryContextMatches && base.CurrentNodeKind == SyntaxKind.ArgumentList)
		{
			return (ArgumentListSyntax)EatNode();
		}
		ParseArgumentList(out var openToken, out var arguments, out var closeToken, SyntaxKind.OpenParenToken, SyntaxKind.CloseParenToken);
		return _syntaxFactory.ArgumentList(openToken, arguments, closeToken);
	}

	internal BracketedArgumentListSyntax ParseBracketedArgumentList()
	{
		if (IsIncrementalAndFactoryContextMatches && base.CurrentNodeKind == SyntaxKind.BracketedArgumentList)
		{
			return (BracketedArgumentListSyntax)EatNode();
		}
		ParseArgumentList(out var openToken, out var arguments, out var closeToken, SyntaxKind.OpenBracketToken, SyntaxKind.CloseBracketToken);
		return _syntaxFactory.BracketedArgumentList(openToken, arguments, closeToken);
	}

	private void ParseArgumentList(out SyntaxToken openToken, out Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ArgumentSyntax> arguments, out SyntaxToken closeToken, SyntaxKind openKind, SyntaxKind closeKind)
	{
		bool flag = openKind == SyntaxKind.OpenBracketToken;
		SyntaxKind kind = base.CurrentToken.Kind;
		bool flag2 = ((kind == SyntaxKind.OpenParenToken || kind == SyntaxKind.OpenBracketToken) ? true : false);
		openToken = (flag2 ? EatTokenAsKind(openKind) : EatToken(openKind));
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfArgumentList;
		if (base.CurrentToken.Kind != closeKind && base.CurrentToken.Kind != SyntaxKind.SemicolonToken)
		{
			if (flag)
			{
				arguments = ParseCommaSeparatedSyntaxList(ref openToken, SyntaxKind.CloseBracketToken, (LanguageParser @this) => @this.IsPossibleArgumentExpression(), (LanguageParser @this) => @this.ParseArgumentExpression(isIndexer: true), skipBadArgumentListTokens, allowTrailingSeparator: false, requireOneElement: false, allowSemicolonAsSeparator: false);
			}
			else
			{
				arguments = ParseCommaSeparatedSyntaxList(ref openToken, SyntaxKind.CloseParenToken, (LanguageParser @this) => @this.IsPossibleArgumentExpression(), (LanguageParser @this) => @this.ParseArgumentExpression(isIndexer: false), skipBadArgumentListTokens, allowTrailingSeparator: false, requireOneElement: false, allowSemicolonAsSeparator: false);
			}
		}
		else if (flag && base.CurrentToken.Kind == closeKind)
		{
			SeparatedSyntaxListBuilder<ArgumentSyntax> item = _pool.AllocateSeparated<ArgumentSyntax>();
			item.Add(ParseArgumentExpression(flag));
			arguments = _pool.ToListAndFree(in item);
		}
		else
		{
			arguments = default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ArgumentSyntax>);
		}
		_termState = termState;
		kind = base.CurrentToken.Kind;
		flag2 = ((kind == SyntaxKind.CloseParenToken || kind == SyntaxKind.CloseBracketToken) ? true : false);
		closeToken = (flag2 ? EatTokenAsKind(closeKind) : EatToken(closeKind));
		static PostSkipAction skipBadArgumentListTokens(LanguageParser @this, ref SyntaxToken open, SeparatedSyntaxListBuilder<ArgumentSyntax> list, SyntaxKind expectedKind, SyntaxKind closeKind2)
		{
			SyntaxKind kind2 = @this.CurrentToken.Kind;
			if ((kind2 == SyntaxKind.CloseParenToken || kind2 == SyntaxKind.CloseBracketToken || kind2 == SyntaxKind.SemicolonToken) ? true : false)
			{
				return PostSkipAction.Abort;
			}
			return @this.SkipBadSeparatedListTokensWithExpectedKind(ref open, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleArgumentExpression(), (LanguageParser p, SyntaxKind syntaxKind) => p.CurrentToken.Kind == syntaxKind || p.CurrentToken.Kind == SyntaxKind.SemicolonToken, expectedKind, closeKind2);
		}
	}

	private bool IsEndOfArgumentList()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (kind == SyntaxKind.CloseParenToken || kind == SyntaxKind.CloseBracketToken)
		{
			return true;
		}
		return false;
	}

	private bool IsPossibleArgumentExpression()
	{
		if (!IsValidArgumentRefKindKeyword(base.CurrentToken.Kind))
		{
			return IsPossibleExpression();
		}
		return true;
	}

	private static bool IsValidArgumentRefKindKeyword(SyntaxKind kind)
	{
		if (kind - 8360 <= (SyntaxKind)2)
		{
			return true;
		}
		return false;
	}

	private ArgumentSyntax ParseArgumentExpression(bool isIndexer)
	{
		NameColonSyntax nameColon = ((base.CurrentToken.Kind == SyntaxKind.IdentifierToken && PeekToken(1).Kind == SyntaxKind.ColonToken) ? _syntaxFactory.NameColon(ParseIdentifierName(), EatToken(SyntaxKind.ColonToken)) : null);
		SyntaxToken syntaxToken = null;
		if (IsValidArgumentRefKindKeyword(base.CurrentToken.Kind) && (base.CurrentToken.Kind != SyntaxKind.RefKeyword || !IsPossibleLambdaExpression(Precedence.Expression)))
		{
			syntaxToken = EatToken();
		}
		bool flag = isIndexer;
		if (flag)
		{
			SyntaxKind kind = base.CurrentToken.Kind;
			bool flag2 = ((kind == SyntaxKind.CloseBracketToken || kind == SyntaxKind.CommaToken) ? true : false);
			flag = flag2;
		}
		ExpressionSyntax expression = (flag ? ParseIdentifierName(ErrorCode.ERR_ValueExpected) : ((base.CurrentToken.Kind != SyntaxKind.CommaToken) ? ((syntaxToken != null && syntaxToken.Kind == SyntaxKind.OutKeyword) ? ParseExpressionOrDeclaration(ParseTypeMode.Normal, permitTupleDesignation: false) : ParseSubExpression(Precedence.Expression)) : ParseIdentifierName(ErrorCode.ERR_MissingArgument)));
		return _syntaxFactory.Argument(nameColon, syntaxToken, expression);
	}

	private TypeOfExpressionSyntax ParseTypeOfExpression()
	{
		return _syntaxFactory.TypeOfExpression(EatToken(), EatToken(SyntaxKind.OpenParenToken), ParseTypeOrVoid(), EatToken(SyntaxKind.CloseParenToken));
	}

	private ExpressionSyntax ParseDefaultExpression()
	{
		SyntaxToken syntaxToken = EatToken();
		if (base.CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			return _syntaxFactory.DefaultExpression(syntaxToken, EatToken(SyntaxKind.OpenParenToken), ParseType(), EatToken(SyntaxKind.CloseParenToken));
		}
		return _syntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, syntaxToken);
	}

	private SizeOfExpressionSyntax ParseSizeOfExpression()
	{
		return _syntaxFactory.SizeOfExpression(EatToken(), EatToken(SyntaxKind.OpenParenToken), ParseType(), EatToken(SyntaxKind.CloseParenToken));
	}

	private MakeRefExpressionSyntax ParseMakeRefExpression()
	{
		return _syntaxFactory.MakeRefExpression(EatToken(), EatToken(SyntaxKind.OpenParenToken), ParseExpressionForParenthesizedConstruct(), EatToken(SyntaxKind.CloseParenToken));
	}

	private RefTypeExpressionSyntax ParseRefTypeExpression()
	{
		return _syntaxFactory.RefTypeExpression(EatToken(), EatToken(SyntaxKind.OpenParenToken), ParseExpressionForParenthesizedConstruct(), EatToken(SyntaxKind.CloseParenToken));
	}

	private CheckedExpressionSyntax ParseCheckedOrUncheckedExpression()
	{
		SyntaxToken syntaxToken = EatToken();
		SyntaxKind kind = ((syntaxToken.Kind == SyntaxKind.CheckedKeyword) ? SyntaxKind.CheckedExpression : SyntaxKind.UncheckedExpression);
		return _syntaxFactory.CheckedExpression(kind, syntaxToken, EatToken(SyntaxKind.OpenParenToken), ParseExpressionForParenthesizedConstruct(), EatToken(SyntaxKind.CloseParenToken));
	}

	private RefValueExpressionSyntax ParseRefValueExpression()
	{
		return _syntaxFactory.RefValueExpression(EatToken(SyntaxKind.RefValueKeyword), EatToken(SyntaxKind.OpenParenToken), ParseSubExpression(Precedence.Expression), EatToken(SyntaxKind.CommaToken), ParseType(), EatToken(SyntaxKind.CloseParenToken));
	}

	private bool ScanParenthesizedLambda(Precedence precedence)
	{
		if (!ScanImplicitlyTypedLambdaOrSimpleExplicitlyTypedParenthesizedLambda(precedence))
		{
			return ScanExplicitlyTypedLambda(precedence);
		}
		return true;
	}

	private bool ScanImplicitlyTypedLambdaOrSimpleExplicitlyTypedParenthesizedLambda(Precedence precedence)
	{
		if (precedence != Precedence.Expression)
		{
			return false;
		}
		int n = 1;
		SyntaxToken syntaxToken;
		do
		{
			syntaxToken = PeekToken(n++);
		}
		while (IsTrueIdentifier(syntaxToken) || syntaxToken.Kind == SyntaxKind.CommaToken || IsParameterModifierIncludingScoped(syntaxToken));
		if (syntaxToken.Kind == SyntaxKind.CloseParenToken)
		{
			return PeekToken(n).Kind == SyntaxKind.EqualsGreaterThanToken;
		}
		return false;
	}

	private bool ScanExplicitlyTypedLambda(Precedence precedence)
	{
		if (precedence != Precedence.Expression)
		{
			return false;
		}
		using (GetDisposableResetPoint(resetOnDispose: true))
		{
			while (true)
			{
				EatToken();
				ParseAttributeDeclarations(inExpressionContext: true);
				if (IsParameterModifierIncludingScoped(base.CurrentToken))
				{
					SyntaxListBuilder syntaxListBuilder = _pool.Allocate();
					ParseParameterModifiers(syntaxListBuilder, isFunctionPointerParameter: false, isLambdaParameter: true);
					_pool.Free(syntaxListBuilder);
				}
				if (ShouldParseLambdaParameterType() && ScanType() == ScanTypeFlags.NotType)
				{
					return false;
				}
				if (!IsTrueIdentifier())
				{
					CreateMissingIdentifierToken();
				}
				else
				{
					EatToken();
				}
				if (TryEatToken(SyntaxKind.EqualsToken) != null)
				{
					if (base.CurrentToken.Kind == SyntaxKind.OpenBracketToken)
					{
						break;
					}
					ParseExpressionCore();
				}
				switch (base.CurrentToken.Kind)
				{
				case SyntaxKind.CommaToken:
					break;
				case SyntaxKind.CloseParenToken:
					return PeekToken(1).Kind == SyntaxKind.EqualsGreaterThanToken;
				default:
					return false;
				}
			}
			return false;
		}
	}

	private ExpressionSyntax ParseCastOrParenExpressionOrTuple()
	{
		using DisposableResetPoint disposableResetPoint = GetDisposableResetPoint(resetOnDispose: false);
		if (ScanCast() && !IsCurrentTokenQueryKeywordInQuery())
		{
			disposableResetPoint.Reset();
			return _syntaxFactory.CastExpression(EatToken(SyntaxKind.OpenParenToken), ParseType(), EatToken(SyntaxKind.CloseParenToken), ParseSubExpression(Precedence.Cast));
		}
		disposableResetPoint.Reset();
		SyntaxToken syntaxToken = EatToken(SyntaxKind.OpenParenToken);
		ExpressionSyntax expressionSyntax = ParseExpressionOrDeclaration(ParseTypeMode.FirstElementOfPossibleTupleLiteral, permitTupleDesignation: true);
		if (base.CurrentToken.Kind == SyntaxKind.CommaToken)
		{
			return ParseTupleExpressionTail(syntaxToken, _syntaxFactory.Argument(null, null, expressionSyntax));
		}
		if (expressionSyntax.Kind == SyntaxKind.IdentifierName && base.CurrentToken.Kind == SyntaxKind.ColonToken)
		{
			return ParseTupleExpressionTail(syntaxToken, _syntaxFactory.Argument(_syntaxFactory.NameColon((IdentifierNameSyntax)expressionSyntax, EatToken()), null, ParseExpressionOrDeclaration(ParseTypeMode.FirstElementOfPossibleTupleLiteral, permitTupleDesignation: true)));
		}
		return _syntaxFactory.ParenthesizedExpression(syntaxToken, ParseErrantExpressionWhenNoCloseParenToken(expressionSyntax), EatToken(SyntaxKind.CloseParenToken));
	}

	private TupleExpressionSyntax ParseTupleExpressionTail(SyntaxToken openParen, ArgumentSyntax firstArg)
	{
		SeparatedSyntaxListBuilder<ArgumentSyntax> item = _pool.AllocateSeparated<ArgumentSyntax>();
		item.Add(firstArg);
		while (base.CurrentToken.Kind == SyntaxKind.CommaToken)
		{
			item.AddSeparator(EatToken(SyntaxKind.CommaToken));
			ExpressionSyntax expressionSyntax = ParseExpressionOrDeclaration(ParseTypeMode.AfterTupleComma, permitTupleDesignation: true);
			ArgumentSyntax node = ((expressionSyntax.Kind != SyntaxKind.IdentifierName || base.CurrentToken.Kind != SyntaxKind.ColonToken) ? _syntaxFactory.Argument(null, null, expressionSyntax) : _syntaxFactory.Argument(_syntaxFactory.NameColon((IdentifierNameSyntax)expressionSyntax, EatToken()), null, ParseExpressionOrDeclaration(ParseTypeMode.AfterTupleComma, permitTupleDesignation: true)));
			item.Add(node);
		}
		if (item.Count < 2)
		{
			item.AddSeparator(SyntaxFactory.MissingToken(SyntaxKind.CommaToken));
			item.Add(_syntaxFactory.Argument(null, null, AddError(CreateMissingIdentifierName(), ErrorCode.ERR_TupleTooFewElements)));
		}
		return _syntaxFactory.TupleExpression(openParen, _pool.ToListAndFree(in item), EatToken(SyntaxKind.CloseParenToken));
	}

	private bool ScanCast(bool forPattern = false)
	{
		if (base.CurrentToken.Kind != SyntaxKind.OpenParenToken)
		{
			return false;
		}
		EatToken();
		ScanTypeFlags scanTypeFlags = ScanType(forPattern);
		if (scanTypeFlags == ScanTypeFlags.NotType)
		{
			return false;
		}
		if (base.CurrentToken.Kind != SyntaxKind.CloseParenToken)
		{
			return false;
		}
		EatToken();
		if (forPattern && base.CurrentToken.Kind == SyntaxKind.IdentifierToken)
		{
			return !isBinaryPattern();
		}
		switch (scanTypeFlags)
		{
		case ScanTypeFlags.MustBeType:
		case ScanTypeFlags.AliasQualifiedName:
		case ScanTypeFlags.NullableType:
		case ScanTypeFlags.PointerOrMultiplication:
		{
			bool flag = !forPattern;
			if (!flag)
			{
				SyntaxKind kind = base.CurrentToken.Kind;
				bool flag2 = kind - 8198 <= SyntaxKind.List || kind - 8202 <= SyntaxKind.List || (kind == SyntaxKind.DotToken && IsAtDotDotToken()) || CanFollowCast(kind);
				flag = flag2;
			}
			return flag;
		}
		case ScanTypeFlags.GenericTypeOrMethod:
		case ScanTypeFlags.TupleType:
			if (base.CurrentToken.Kind != SyntaxKind.OpenBracketToken)
			{
				return CanFollowCast(base.CurrentToken.Kind);
			}
			return true;
		case ScanTypeFlags.GenericTypeOrExpression:
		case ScanTypeFlags.NonGenericTypeOrExpression:
			if (base.CurrentToken.Kind == SyntaxKind.OpenBracketToken && PeekToken(1).Kind == SyntaxKind.CloseBracketToken)
			{
				return true;
			}
			return CanFollowCast(base.CurrentToken.Kind);
		default:
			throw ExceptionUtilities.UnexpectedValue(scanTypeFlags);
		}
		bool isBinaryPattern()
		{
			if (!isBinaryPatternKeyword())
			{
				return false;
			}
			bool flag3 = true;
			EatToken();
			while (isBinaryPatternKeyword())
			{
				flag3 = !flag3;
				EatToken();
			}
			return flag3 == IsPossibleSubpatternElement();
		}
		bool isBinaryPatternKeyword()
		{
			SyntaxKind contextualKind = base.CurrentToken.ContextualKind;
			if (contextualKind - 8438 <= SyntaxKind.List)
			{
				return true;
			}
			return false;
		}
	}

	private bool IsPossibleLambdaExpression(Precedence precedence)
	{
		if (precedence != Precedence.Expression)
		{
			return false;
		}
		if (PeekToken(1).Kind == SyntaxKind.EqualsGreaterThanToken)
		{
			return true;
		}
		using (GetDisposableResetPoint(resetOnDispose: true))
		{
			if (base.CurrentToken.Kind == SyntaxKind.OpenBracketToken)
			{
				Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> syntaxList = ParseAttributeDeclarations(inExpressionContext: true);
				int count = syntaxList.Count;
				if (count >= 1)
				{
					AttributeListSyntax attributeListSyntax = syntaxList[count - 1];
					if (attributeListSyntax != null)
					{
						SyntaxToken closeBracketToken = attributeListSyntax.CloseBracketToken;
						if (closeBracketToken != null && closeBracketToken.IsMissing)
						{
							return false;
						}
					}
				}
			}
			bool flag;
			if (base.CurrentToken.Kind == SyntaxKind.StaticKeyword)
			{
				EatToken();
				flag = true;
			}
			else if (base.CurrentToken.ContextualKind == SyntaxKind.AsyncKeyword && PeekToken(1).Kind == SyntaxKind.StaticKeyword)
			{
				EatToken();
				EatToken();
				flag = true;
			}
			else
			{
				flag = false;
			}
			if (flag)
			{
				if (base.CurrentToken.Kind == SyntaxKind.EqualsGreaterThanToken)
				{
					return true;
				}
				if (base.CurrentToken.Kind == SyntaxKind.OpenParenToken)
				{
					return true;
				}
			}
			if (base.CurrentToken.Kind == SyntaxKind.IdentifierToken && PeekToken(1).Kind == SyntaxKind.EqualsGreaterThanToken)
			{
				return true;
			}
			if (base.CurrentToken.ContextualKind == SyntaxKind.AsyncKeyword && IsAnonymousFunctionAsyncModifier())
			{
				EatToken();
			}
			using (DisposableResetPoint disposableResetPoint = GetDisposableResetPoint(resetOnDispose: false))
			{
				if (ScanType() == ScanTypeFlags.NotType || base.CurrentToken.Kind != SyntaxKind.OpenParenToken)
				{
					disposableResetPoint.Reset();
				}
			}
			if (base.CurrentToken.Kind == SyntaxKind.IdentifierToken && PeekToken(1).Kind == SyntaxKind.EqualsGreaterThanToken)
			{
				return true;
			}
			if (base.CurrentToken.Kind != SyntaxKind.OpenParenToken)
			{
				return false;
			}
			return ScanParenthesizedLambda(precedence);
		}
	}

	private static bool CanFollowCast(SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.PercentToken:
		case SyntaxKind.CaretToken:
		case SyntaxKind.AmpersandToken:
		case SyntaxKind.AsteriskToken:
		case SyntaxKind.CloseParenToken:
		case SyntaxKind.MinusToken:
		case SyntaxKind.PlusToken:
		case SyntaxKind.EqualsToken:
		case SyntaxKind.OpenBraceToken:
		case SyntaxKind.CloseBraceToken:
		case SyntaxKind.OpenBracketToken:
		case SyntaxKind.CloseBracketToken:
		case SyntaxKind.BarToken:
		case SyntaxKind.ColonToken:
		case SyntaxKind.SemicolonToken:
		case SyntaxKind.LessThanToken:
		case SyntaxKind.CommaToken:
		case SyntaxKind.GreaterThanToken:
		case SyntaxKind.DotToken:
		case SyntaxKind.QuestionToken:
		case SyntaxKind.SlashToken:
		case SyntaxKind.BarBarToken:
		case SyntaxKind.AmpersandAmpersandToken:
		case SyntaxKind.MinusMinusToken:
		case SyntaxKind.PlusPlusToken:
		case SyntaxKind.QuestionQuestionToken:
		case SyntaxKind.MinusGreaterThanToken:
		case SyntaxKind.ExclamationEqualsToken:
		case SyntaxKind.EqualsEqualsToken:
		case SyntaxKind.EqualsGreaterThanToken:
		case SyntaxKind.LessThanEqualsToken:
		case SyntaxKind.LessThanLessThanToken:
		case SyntaxKind.LessThanLessThanEqualsToken:
		case SyntaxKind.GreaterThanEqualsToken:
		case SyntaxKind.GreaterThanGreaterThanToken:
		case SyntaxKind.GreaterThanGreaterThanEqualsToken:
		case SyntaxKind.SlashEqualsToken:
		case SyntaxKind.AsteriskEqualsToken:
		case SyntaxKind.BarEqualsToken:
		case SyntaxKind.AmpersandEqualsToken:
		case SyntaxKind.PlusEqualsToken:
		case SyntaxKind.MinusEqualsToken:
		case SyntaxKind.CaretEqualsToken:
		case SyntaxKind.PercentEqualsToken:
		case SyntaxKind.QuestionQuestionEqualsToken:
		case SyntaxKind.GreaterThanGreaterThanGreaterThanToken:
		case SyntaxKind.GreaterThanGreaterThanGreaterThanEqualsToken:
		case SyntaxKind.SwitchKeyword:
		case SyntaxKind.IsKeyword:
		case SyntaxKind.AsKeyword:
		case SyntaxKind.EndOfFileToken:
			return false;
		default:
			return true;
		}
	}

	private ExpressionSyntax ParseNewExpression()
	{
		if (IsAnonymousType())
		{
			return ParseAnonymousTypeExpression();
		}
		if (IsImplicitlyTypedArray())
		{
			return ParseImplicitlyTypedArrayCreation();
		}
		return ParseArrayOrObjectCreationExpression();
	}

	private CollectionExpressionSyntax ParseCollectionExpression()
	{
		SyntaxToken openToken = EatToken(SyntaxKind.OpenBracketToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<CollectionElementSyntax> elements = ParseCommaSeparatedSyntaxList(ref openToken, SyntaxKind.CloseBracketToken, (LanguageParser @this) => @this.IsPossibleCollectionElement(), (LanguageParser @this) => @this.ParseCollectionElement(), skipBadCollectionElementTokens, allowTrailingSeparator: true, requireOneElement: false, allowSemicolonAsSeparator: false);
		return _syntaxFactory.CollectionExpression(openToken, elements, EatToken(SyntaxKind.CloseBracketToken));
		static PostSkipAction skipBadCollectionElementTokens(LanguageParser @this, ref SyntaxToken openBracket, SeparatedSyntaxListBuilder<CollectionElementSyntax> list, SyntaxKind expectedKind, SyntaxKind closeKind)
		{
			return @this.SkipBadSeparatedListTokensWithExpectedKind(ref openBracket, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleCollectionElement(), (LanguageParser p, SyntaxKind syntaxKind) => p.CurrentToken.Kind == syntaxKind, expectedKind, closeKind);
		}
	}

	private bool IsPossibleCollectionElement()
	{
		return IsPossibleExpression();
	}

	private CollectionElementSyntax ParseCollectionElement()
	{
		if (!IsAtDotDotToken())
		{
			return _syntaxFactory.ExpressionElement(ParseExpressionCore());
		}
		return _syntaxFactory.SpreadElement(EatDotDotToken(), ParseExpressionCore());
	}

	private bool IsAnonymousType()
	{
		if (base.CurrentToken.Kind == SyntaxKind.NewKeyword)
		{
			return PeekToken(1).Kind == SyntaxKind.OpenBraceToken;
		}
		return false;
	}

	private AnonymousObjectCreationExpressionSyntax ParseAnonymousTypeExpression()
	{
		SyntaxToken newKeyword = EatToken(SyntaxKind.NewKeyword);
		SyntaxToken openToken = EatToken(SyntaxKind.OpenBraceToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<AnonymousObjectMemberDeclaratorSyntax> initializers = ParseCommaSeparatedSyntaxList(ref openToken, SyntaxKind.CloseBraceToken, (LanguageParser @this) => @this.IsPossibleExpression(), (LanguageParser @this) => @this.ParseAnonymousTypeMemberInitializer(), SkipBadInitializerListTokens, allowTrailingSeparator: true, requireOneElement: false, allowSemicolonAsSeparator: false);
		return _syntaxFactory.AnonymousObjectCreationExpression(newKeyword, openToken, initializers, EatToken(SyntaxKind.CloseBraceToken));
	}

	private AnonymousObjectMemberDeclaratorSyntax ParseAnonymousTypeMemberInitializer()
	{
		return _syntaxFactory.AnonymousObjectMemberDeclarator(IsNamedAssignment() ? ParseNameEquals() : null, ParseExpressionCore());
	}

	private bool IsInitializerMember()
	{
		if (!IsComplexElementInitializer() && !IsNamedAssignment() && !IsDictionaryInitializer())
		{
			return IsPossibleExpression();
		}
		return true;
	}

	private bool IsComplexElementInitializer()
	{
		return base.CurrentToken.Kind == SyntaxKind.OpenBraceToken;
	}

	private bool IsNamedAssignment()
	{
		if (IsTrueIdentifier())
		{
			return PeekToken(1).Kind == SyntaxKind.EqualsToken;
		}
		return false;
	}

	private bool IsNamedMemberInitializer()
	{
		bool flag = IsTrueIdentifier();
		if (flag)
		{
			SyntaxKind kind = PeekToken(1).Kind;
			bool flag2 = ((kind == SyntaxKind.EqualsToken || kind == SyntaxKind.ColonToken) ? true : false);
			flag = flag2;
		}
		return flag;
	}

	private bool IsDictionaryInitializer()
	{
		return base.CurrentToken.Kind == SyntaxKind.OpenBracketToken;
	}

	private ExpressionSyntax ParseArrayOrObjectCreationExpression()
	{
		SyntaxToken newKeyword = EatToken(SyntaxKind.NewKeyword);
		TypeSyntax typeSyntax = null;
		InitializerExpressionSyntax initializerExpressionSyntax = null;
		if (!IsImplicitObjectCreation())
		{
			typeSyntax = ParseType(ParseTypeMode.NewExpression);
			if (typeSyntax.Kind == SyntaxKind.ArrayType)
			{
				if (base.CurrentToken.Kind == SyntaxKind.OpenBraceToken)
				{
					initializerExpressionSyntax = ParseArrayInitializer();
				}
				return _syntaxFactory.ArrayCreationExpression(newKeyword, (ArrayTypeSyntax)typeSyntax, initializerExpressionSyntax);
			}
		}
		ArgumentListSyntax argumentListSyntax = null;
		if (base.CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			argumentListSyntax = ParseParenthesizedArgumentList();
		}
		if (base.CurrentToken.Kind == SyntaxKind.OpenBraceToken)
		{
			initializerExpressionSyntax = ParseObjectOrCollectionInitializer();
		}
		if (argumentListSyntax == null && initializerExpressionSyntax == null)
		{
			argumentListSyntax = _syntaxFactory.ArgumentList(EatToken(SyntaxKind.OpenParenToken, ErrorCode.ERR_BadNewExpr, typeSyntax != null && !typeSyntax.ContainsDiagnostics), default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ArgumentSyntax>), SyntaxFactory.MissingToken(SyntaxKind.CloseParenToken));
		}
		if (typeSyntax != null)
		{
			return _syntaxFactory.ObjectCreationExpression(newKeyword, typeSyntax, argumentListSyntax, initializerExpressionSyntax);
		}
		return _syntaxFactory.ImplicitObjectCreationExpression(newKeyword, argumentListSyntax, initializerExpressionSyntax);
	}

	private bool IsImplicitObjectCreation()
	{
		if (base.CurrentToken.Kind != SyntaxKind.OpenParenToken)
		{
			return false;
		}
		using (GetDisposableResetPoint(resetOnDispose: true))
		{
			EatToken();
			if (ScanTupleType(out var _) != ScanTypeFlags.NotType)
			{
				SyntaxKind kind = base.CurrentToken.Kind;
				if (kind == SyntaxKind.OpenParenToken || kind == SyntaxKind.OpenBracketToken || kind == SyntaxKind.QuestionToken)
				{
					return false;
				}
			}
			return true;
		}
	}

	private WithExpressionSyntax ParseWithExpression(ExpressionSyntax receiverExpression, SyntaxToken withKeyword)
	{
		SyntaxToken openToken = EatToken(SyntaxKind.OpenBraceToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> expressions = ParseCommaSeparatedSyntaxList(ref openToken, SyntaxKind.CloseBraceToken, (LanguageParser @this) => @this.IsPossibleExpression(), (LanguageParser @this) => @this.ParseExpressionCore(), SkipBadInitializerListTokens, allowTrailingSeparator: true, requireOneElement: false, allowSemicolonAsSeparator: false);
		return _syntaxFactory.WithExpression(receiverExpression, withKeyword, _syntaxFactory.InitializerExpression(SyntaxKind.WithInitializerExpression, openToken, expressions, EatToken(SyntaxKind.CloseBraceToken)));
	}

	private InitializerExpressionSyntax ParseObjectOrCollectionInitializer()
	{
		SyntaxToken openToken = EatToken(SyntaxKind.OpenBraceToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> separatedSyntaxList = ParseCommaSeparatedSyntaxList(ref openToken, SyntaxKind.CloseBraceToken, (LanguageParser @this) => @this.IsInitializerMember(), (LanguageParser @this) => @this.ParseObjectOrCollectionInitializerMember(), SkipBadInitializerListTokens, allowTrailingSeparator: true, requireOneElement: false, allowSemicolonAsSeparator: true);
		SyntaxKind kind = (isObjectInitializer(separatedSyntaxList) ? SyntaxKind.ObjectInitializerExpression : SyntaxKind.CollectionInitializerExpression);
		return _syntaxFactory.InitializerExpression(kind, openToken, separatedSyntaxList, EatToken(SyntaxKind.CloseBraceToken));
		static bool isObjectInitializer(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> initializers)
		{
			if (initializers.Count == 0)
			{
				return true;
			}
			int num = 0;
			int count = initializers.Count;
			while (num < count)
			{
				ExpressionSyntax expressionSyntax = initializers[num];
				bool flag;
				if (expressionSyntax is AssignmentExpressionSyntax assignmentExpressionSyntax && expressionSyntax.Kind == SyntaxKind.SimpleAssignmentExpression)
				{
					ExpressionSyntax left = assignmentExpressionSyntax.Left;
					if (left != null)
					{
						SyntaxKind kind2 = left.Kind;
						if (kind2 == SyntaxKind.IdentifierName || kind2 == SyntaxKind.ImplicitElementAccess)
						{
							flag = true;
							goto IL_0066;
						}
					}
				}
				flag = false;
				goto IL_0066;
				IL_0066:
				if (flag)
				{
					return true;
				}
				num++;
			}
			return false;
		}
	}

	private ExpressionSyntax ParseObjectOrCollectionInitializerMember()
	{
		if (IsComplexElementInitializer())
		{
			return ParseComplexElementInitializer();
		}
		if (IsDictionaryInitializer())
		{
			return ParseDictionaryInitializer();
		}
		if (IsNamedMemberInitializer())
		{
			return ParseObjectInitializerNamedAssignment();
		}
		return ParsePossibleRefExpression();
	}

	private static PostSkipAction SkipBadInitializerListTokens<T>(LanguageParser @this, ref SyntaxToken startToken, SeparatedSyntaxListBuilder<T> list, SyntaxKind expectedKind, SyntaxKind closeKind) where T : CSharpSyntaxNode
	{
		return @this.SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleExpression(), (LanguageParser p, SyntaxKind syntaxKind) => p.CurrentToken.Kind == syntaxKind, expectedKind, closeKind);
	}

	private AssignmentExpressionSyntax ParseObjectInitializerNamedAssignment()
	{
		return _syntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, ParseIdentifierName(), (base.CurrentToken.Kind == SyntaxKind.ColonToken) ? EatTokenAsKind(SyntaxKind.EqualsToken) : EatToken(SyntaxKind.EqualsToken), (base.CurrentToken.Kind == SyntaxKind.OpenBraceToken) ? ParseObjectOrCollectionInitializer() : ParsePossibleRefExpression());
	}

	private AssignmentExpressionSyntax ParseDictionaryInitializer()
	{
		return _syntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, _syntaxFactory.ImplicitElementAccess(ParseBracketedArgumentList()), EatToken(SyntaxKind.EqualsToken), (base.CurrentToken.Kind == SyntaxKind.OpenBraceToken) ? ParseObjectOrCollectionInitializer() : ParsePossibleRefExpression());
	}

	private InitializerExpressionSyntax ParseComplexElementInitializer()
	{
		SyntaxToken openToken = EatToken(SyntaxKind.OpenBraceToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> expressions = ParseCommaSeparatedSyntaxList(ref openToken, SyntaxKind.CloseBraceToken, (LanguageParser @this) => @this.IsPossibleExpression(), (LanguageParser @this) => @this.ParseExpressionCore(), SkipBadInitializerListTokens, allowTrailingSeparator: false, requireOneElement: false, allowSemicolonAsSeparator: false);
		return _syntaxFactory.InitializerExpression(SyntaxKind.ComplexElementInitializerExpression, openToken, expressions, EatToken(SyntaxKind.CloseBraceToken));
	}

	private bool IsImplicitlyTypedArray()
	{
		return PeekToken(1).Kind == SyntaxKind.OpenBracketToken;
	}

	private ImplicitArrayCreationExpressionSyntax ParseImplicitlyTypedArrayCreation()
	{
		SyntaxToken newKeyword = EatToken(SyntaxKind.NewKeyword);
		SyntaxToken syntaxToken = EatToken(SyntaxKind.OpenBracketToken);
		SyntaxListBuilder syntaxListBuilder = _pool.Allocate();
		int lastTokenPosition = -1;
		while (IsMakingProgress(ref lastTokenPosition))
		{
			if (IsPossibleExpression())
			{
				ExpressionSyntax skippedSyntax = AddError(ParseExpressionCore(), ErrorCode.ERR_InvalidArray);
				if (syntaxListBuilder.Count == 0)
				{
					syntaxToken = AddTrailingSkippedSyntax(syntaxToken, skippedSyntax);
				}
				else
				{
					AddTrailingSkippedSyntax(syntaxListBuilder, skippedSyntax);
				}
			}
			if (base.CurrentToken.Kind != SyntaxKind.CommaToken)
			{
				break;
			}
			syntaxListBuilder.Add(EatToken());
		}
		return _syntaxFactory.ImplicitArrayCreationExpression(newKeyword, syntaxToken, _pool.ToTokenListAndFree(syntaxListBuilder), EatToken(SyntaxKind.CloseBracketToken), ParseArrayInitializer());
	}

	private InitializerExpressionSyntax ParseArrayInitializer()
	{
		SyntaxToken openToken = EatToken(SyntaxKind.OpenBraceToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> expressions = ParseCommaSeparatedSyntaxList(ref openToken, SyntaxKind.CloseBraceToken, (LanguageParser @this) => @this.IsPossibleVariableInitializer(), (LanguageParser @this) => @this.ParseVariableInitializer(), skipBadArrayInitializerTokens, allowTrailingSeparator: true, requireOneElement: false, allowSemicolonAsSeparator: false);
		return _syntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression, openToken, expressions, EatToken(SyntaxKind.CloseBraceToken));
		static PostSkipAction skipBadArrayInitializerTokens(LanguageParser @this, ref SyntaxToken openBrace, SeparatedSyntaxListBuilder<ExpressionSyntax> list, SyntaxKind expectedKind, SyntaxKind closeKind)
		{
			return @this.SkipBadSeparatedListTokensWithExpectedKind(ref openBrace, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleVariableInitializer(), (LanguageParser p, SyntaxKind syntaxKind) => p.CurrentToken.Kind == syntaxKind, expectedKind, closeKind);
		}
	}

	private ExpressionSyntax ParseStackAllocExpression()
	{
		if (!IsImplicitlyTypedArray())
		{
			return ParseRegularStackAllocExpression();
		}
		return ParseImplicitlyTypedStackAllocExpression();
	}

	private ExpressionSyntax ParseImplicitlyTypedStackAllocExpression()
	{
		SyntaxToken stackAllocKeyword = EatToken(SyntaxKind.StackAllocKeyword);
		SyntaxToken syntaxToken = EatToken(SyntaxKind.OpenBracketToken);
		int lastTokenPosition = -1;
		while (IsMakingProgress(ref lastTokenPosition))
		{
			if (IsPossibleExpression())
			{
				ExpressionSyntax skippedSyntax = AddError(ParseExpressionCore(), ErrorCode.ERR_InvalidStackAllocArray);
				syntaxToken = AddTrailingSkippedSyntax(syntaxToken, skippedSyntax);
			}
			if (base.CurrentToken.Kind != SyntaxKind.CommaToken)
			{
				break;
			}
			SyntaxToken skippedSyntax2 = AddError(EatToken(), ErrorCode.ERR_InvalidStackAllocArray);
			syntaxToken = AddTrailingSkippedSyntax(syntaxToken, skippedSyntax2);
		}
		return _syntaxFactory.ImplicitStackAllocArrayCreationExpression(stackAllocKeyword, syntaxToken, EatToken(SyntaxKind.CloseBracketToken), ParseArrayInitializer());
	}

	private ExpressionSyntax ParseRegularStackAllocExpression()
	{
		return _syntaxFactory.StackAllocArrayCreationExpression(EatToken(SyntaxKind.StackAllocKeyword), ParseType(), (base.CurrentToken.Kind == SyntaxKind.OpenBraceToken) ? ParseArrayInitializer() : null);
	}

	private AnonymousMethodExpressionSyntax ParseAnonymousMethodExpression()
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = ParseAnonymousFunctionModifiers();
		bool? isInAsyncContext = IsInAsync || modifiers.Any(8435);
		bool? forceConditionalAccessExpression = false;
		using (new ParserSyntaxContextResetter(this, isInAsyncContext, null, null, forceConditionalAccessExpression))
		{
			return parseAnonymousMethodExpressionWorker();
		}
		AnonymousMethodExpressionSyntax parseAnonymousMethodExpressionWorker()
		{
			SyntaxToken delegateKeyword = EatToken(SyntaxKind.DelegateKeyword);
			ParameterListSyntax parameterList = null;
			if (base.CurrentToken.Kind == SyntaxKind.OpenParenToken)
			{
				parameterList = ParseParenthesizedParameterList(forExtension: false);
			}
			if (base.CurrentToken.Kind != SyntaxKind.OpenBraceToken)
			{
				SyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
				return _syntaxFactory.AnonymousMethodExpression(modifiers, delegateKeyword, parameterList, _syntaxFactory.Block(default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>), openBraceToken, default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<StatementSyntax>), SyntaxFactory.MissingToken(SyntaxKind.CloseBraceToken)), null);
			}
			return _syntaxFactory.AnonymousMethodExpression(modifiers, delegateKeyword, parameterList, ParseBlock(default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>)), null);
		}
	}

	private Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> ParseAnonymousFunctionModifiers()
	{
		SyntaxListBuilder syntaxListBuilder = _pool.Allocate();
		while (true)
		{
			if (base.CurrentToken.Kind == SyntaxKind.StaticKeyword)
			{
				syntaxListBuilder.Add(EatToken(SyntaxKind.StaticKeyword));
				continue;
			}
			if (base.CurrentToken.ContextualKind != SyntaxKind.AsyncKeyword || !IsAnonymousFunctionAsyncModifier())
			{
				break;
			}
			syntaxListBuilder.Add(EatContextualToken(SyntaxKind.AsyncKeyword));
		}
		return _pool.ToTokenListAndFree(syntaxListBuilder);
	}

	private bool IsAnonymousFunctionAsyncModifier()
	{
		SyntaxKind kind = PeekToken(1).Kind;
		switch (kind)
		{
		case SyntaxKind.OpenParenToken:
		case SyntaxKind.StaticKeyword:
		case SyntaxKind.RefKeyword:
		case SyntaxKind.DelegateKeyword:
		case SyntaxKind.IdentifierToken:
			return true;
		default:
			return IsPredefinedType(kind);
		}
	}

	private LambdaExpressionSyntax TryParseLambdaExpression()
	{
		using DisposableResetPoint disposableResetPoint = GetDisposableResetPoint(resetOnDispose: false);
		LambdaExpressionSyntax lambdaExpressionSyntax = ParseLambdaExpression();
		if (base.CurrentToken.Kind == SyntaxKind.ColonToken && lambdaExpressionSyntax is ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpressionSyntax && parenthesizedLambdaExpressionSyntax.ReturnType is NullableTypeSyntax)
		{
			disposableResetPoint.Reset();
			return null;
		}
		return lambdaExpressionSyntax;
	}

	private LambdaExpressionSyntax ParseLambdaExpression()
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributes = ParseAttributeDeclarations(inExpressionContext: true);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers = ParseAnonymousFunctionModifiers();
		bool? isInAsyncContext = IsInAsync || modifiers.Any(8435);
		bool? forceConditionalAccessExpression = false;
		using (new ParserSyntaxContextResetter(this, isInAsyncContext, null, null, forceConditionalAccessExpression))
		{
			return parseLambdaExpressionWorker();
		}
		LambdaExpressionSyntax parseLambdaExpressionWorker()
		{
			TypeSyntax returnType;
			using (DisposableResetPoint disposableResetPoint = GetDisposableResetPoint(resetOnDispose: false))
			{
				returnType = ParseReturnType();
				if (base.CurrentToken.Kind != SyntaxKind.OpenParenToken)
				{
					disposableResetPoint.Reset();
					returnType = null;
				}
			}
			if (base.CurrentToken.Kind == SyntaxKind.OpenParenToken)
			{
				ParameterListSyntax parameterList = ParseLambdaParameterList();
				SyntaxToken arrowToken = EatToken(SyntaxKind.EqualsGreaterThanToken);
				var (block, expressionBody) = ParseLambdaBody();
				return _syntaxFactory.ParenthesizedLambdaExpression(attributes, modifiers, returnType, parameterList, arrowToken, block, expressionBody);
			}
			SyntaxToken identifier = ((base.CurrentToken.Kind != SyntaxKind.IdentifierToken && PeekToken(1).Kind == SyntaxKind.EqualsGreaterThanToken) ? EatTokenAsKind(SyntaxKind.IdentifierToken) : ParseIdentifierToken());
			SyntaxToken arrowToken2 = EatToken(SyntaxKind.EqualsGreaterThanToken);
			ParameterSyntax parameter = _syntaxFactory.Parameter(default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>), default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>), null, identifier, null);
			var (block2, expressionBody2) = ParseLambdaBody();
			return _syntaxFactory.SimpleLambdaExpression(attributes, modifiers, parameter, arrowToken2, block2, expressionBody2);
		}
	}

	private (BlockSyntax, ExpressionSyntax) ParseLambdaBody()
	{
		if (base.CurrentToken.Kind != SyntaxKind.OpenBraceToken)
		{
			return (null, ParsePossibleRefExpression());
		}
		return (ParseBlock(default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>)), null);
	}

	private ParameterListSyntax ParseLambdaParameterList()
	{
		SyntaxToken openToken = EatToken(SyntaxKind.OpenParenToken);
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsEndOfParameterList;
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ParameterSyntax> parameters = ParseCommaSeparatedSyntaxList(ref openToken, SyntaxKind.CloseParenToken, (LanguageParser @this) => @this.IsPossibleLambdaParameter(), (LanguageParser @this) => @this.ParseLambdaParameter(), skipBadLambdaParameterListTokens, allowTrailingSeparator: false, requireOneElement: false, allowSemicolonAsSeparator: false);
		_termState = termState;
		return _syntaxFactory.ParameterList(openToken, parameters, EatToken(SyntaxKind.CloseParenToken));
		static PostSkipAction skipBadLambdaParameterListTokens(LanguageParser @this, ref SyntaxToken openParen, SeparatedSyntaxListBuilder<ParameterSyntax> list, SyntaxKind expectedKind, SyntaxKind closeKind)
		{
			return @this.SkipBadSeparatedListTokensWithExpectedKind(ref openParen, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleLambdaParameter(), (LanguageParser p, SyntaxKind syntaxKind) => p.CurrentToken.Kind == syntaxKind, expectedKind, closeKind);
		}
	}

	private bool IsPossibleLambdaParameter()
	{
		switch (base.CurrentToken.Kind)
		{
		case SyntaxKind.OpenParenToken:
		case SyntaxKind.OpenBracketToken:
		case SyntaxKind.ReadOnlyKeyword:
		case SyntaxKind.RefKeyword:
		case SyntaxKind.OutKeyword:
		case SyntaxKind.InKeyword:
		case SyntaxKind.ParamsKeyword:
			return true;
		case SyntaxKind.IdentifierToken:
			return IsTrueIdentifier();
		case SyntaxKind.DelegateKeyword:
			return IsFunctionPointerStart();
		default:
			return IsPredefinedType(base.CurrentToken.Kind);
		}
	}

	private ParameterSyntax ParseLambdaParameter()
	{
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists = ParseAttributeDeclarations(inExpressionContext: false);
		SyntaxListBuilder syntaxListBuilder = _pool.Allocate();
		if (IsParameterModifierIncludingScoped(base.CurrentToken))
		{
			ParseParameterModifiers(syntaxListBuilder, isFunctionPointerParameter: false, isLambdaParameter: true);
		}
		TypeSyntax type = (ShouldParseLambdaParameterType() ? ParseType(ParseTypeMode.Parameter) : null);
		SyntaxToken identifier = ParseIdentifierToken();
		SyntaxToken syntaxToken = TryEatToken(SyntaxKind.EqualsToken);
		return _syntaxFactory.Parameter(attributeLists, _pool.ToTokenListAndFree(syntaxListBuilder), type, identifier, (syntaxToken != null) ? _syntaxFactory.EqualsValueClause(syntaxToken, ParseExpressionCore()) : null);
	}

	private bool ShouldParseLambdaParameterType()
	{
		if (IsPredefinedType(base.CurrentToken.Kind))
		{
			return true;
		}
		if (base.CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			return true;
		}
		if (IsFunctionPointerStart())
		{
			return true;
		}
		if (IsTrueIdentifier(base.CurrentToken))
		{
			SyntaxKind kind = PeekToken(1).Kind;
			if (kind != SyntaxKind.CommaToken && kind != SyntaxKind.CloseParenToken && kind != SyntaxKind.EqualsGreaterThanToken && kind != SyntaxKind.OpenBraceToken && kind != SyntaxKind.EqualsToken)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsTokenQueryContextualKeyword(SyntaxToken token)
	{
		if (IsTokenStartOfNewQueryClause(token))
		{
			return true;
		}
		SyntaxKind contextualKind = token.ContextualKind;
		if (contextualKind == SyntaxKind.ByKeyword || contextualKind - 8430 <= (SyntaxKind)3)
		{
			return true;
		}
		return false;
	}

	private static bool IsTokenStartOfNewQueryClause(SyntaxToken token)
	{
		SyntaxKind contextualKind = token.ContextualKind;
		if (contextualKind - 8421 <= (SyntaxKind)5 || contextualKind - 8428 <= SyntaxKind.List)
		{
			return true;
		}
		return false;
	}

	private bool IsQueryExpression(bool mayBeVariableDeclaration, bool mayBeMemberDeclaration)
	{
		if (base.CurrentToken.ContextualKind == SyntaxKind.FromKeyword)
		{
			return IsQueryExpressionAfterFrom(mayBeVariableDeclaration, mayBeMemberDeclaration);
		}
		return false;
	}

	private bool IsQueryExpressionAfterFrom(bool mayBeVariableDeclaration, bool mayBeMemberDeclaration)
	{
		SyntaxKind kind = PeekToken(1).Kind;
		if (IsPredefinedType(kind))
		{
			return true;
		}
		if (kind == SyntaxKind.IdentifierToken)
		{
			SyntaxKind kind2 = PeekToken(2).Kind;
			if (kind2 == SyntaxKind.InKeyword)
			{
				return true;
			}
			if (mayBeVariableDeclaration && ((kind2 == SyntaxKind.EqualsToken || kind2 == SyntaxKind.SemicolonToken || kind2 == SyntaxKind.CommaToken) ? true : false))
			{
				return false;
			}
			if (!mayBeMemberDeclaration)
			{
				return true;
			}
			if ((kind2 == SyntaxKind.OpenParenToken || kind2 == SyntaxKind.OpenBraceToken) ? true : false)
			{
				return false;
			}
		}
		using (GetDisposableResetPoint(resetOnDispose: true))
		{
			EatToken();
			bool flag = ScanType() != ScanTypeFlags.NotType;
			if (flag)
			{
				SyntaxKind kind3 = base.CurrentToken.Kind;
				bool flag2 = ((kind3 == SyntaxKind.InKeyword || kind3 == SyntaxKind.IdentifierToken) ? true : false);
				flag = flag2;
			}
			return flag;
		}
	}

	private QueryExpressionSyntax ParseQueryExpression(Precedence precedence)
	{
		bool? isInQueryContext = true;
		using (new ParserSyntaxContextResetter(this, null, isInQueryContext))
		{
			FromClauseSyntax fromClauseSyntax = ParseFromClause();
			return _syntaxFactory.QueryExpression((precedence == Precedence.Expression) ? fromClauseSyntax : AddError(fromClauseSyntax, ErrorCode.WRN_PrecedenceInversion, SyntaxFacts.GetText(SyntaxKind.FromKeyword)), ParseQueryBody());
		}
	}

	private QueryBodySyntax ParseQueryBody()
	{
		SyntaxListBuilder<QueryClauseSyntax> item = _pool.Allocate<QueryClauseSyntax>();
		while (true)
		{
			switch (base.CurrentToken.ContextualKind)
			{
			case SyntaxKind.FromKeyword:
				item.Add(ParseFromClause());
				continue;
			case SyntaxKind.JoinKeyword:
				item.Add(ParseJoinClause());
				continue;
			case SyntaxKind.LetKeyword:
				item.Add(ParseLetClause());
				continue;
			case SyntaxKind.WhereKeyword:
				item.Add(ParseWhereClause());
				continue;
			case SyntaxKind.OrderByKeyword:
				item.Add(ParseOrderByClause());
				continue;
			}
			SelectOrGroupClauseSyntax selectOrGroup = base.CurrentToken.ContextualKind switch
			{
				SyntaxKind.SelectKeyword => ParseSelectClause(), 
				SyntaxKind.GroupKeyword => ParseGroupClause(), 
				_ => _syntaxFactory.SelectClause(EatToken(SyntaxKind.SelectKeyword, ErrorCode.ERR_ExpectedSelectOrGroup), CreateMissingIdentifierName()), 
			};
			return _syntaxFactory.QueryBody(_pool.ToListAndFree(item), selectOrGroup, (base.CurrentToken.ContextualKind == SyntaxKind.IntoKeyword) ? ParseQueryContinuation() : null);
		}
	}

	private FromClauseSyntax ParseFromClause()
	{
		SyntaxToken fromKeyword = EatContextualToken(SyntaxKind.FromKeyword);
		TypeSyntax type = ((PeekToken(1).Kind != SyntaxKind.InKeyword) ? ParseType() : null);
		SyntaxToken syntaxToken;
		if (PeekToken(1).ContextualKind == SyntaxKind.InKeyword && (base.CurrentToken.Kind != SyntaxKind.IdentifierToken || SyntaxFacts.IsQueryContextualKeyword(base.CurrentToken.ContextualKind)))
		{
			syntaxToken = EatToken();
			syntaxToken = WithAdditionalDiagnostics(syntaxToken, GetExpectedTokenError(SyntaxKind.IdentifierToken, syntaxToken.ContextualKind, syntaxToken.GetLeadingTriviaWidth(), syntaxToken.Width));
			syntaxToken = ConvertToMissingWithTrailingTrivia(syntaxToken, SyntaxKind.IdentifierToken);
		}
		else
		{
			syntaxToken = ParseIdentifierToken();
		}
		return _syntaxFactory.FromClause(fromKeyword, type, syntaxToken, EatToken(SyntaxKind.InKeyword), ParseExpressionCore());
	}

	private JoinClauseSyntax ParseJoinClause()
	{
		return _syntaxFactory.JoinClause(EatContextualToken(SyntaxKind.JoinKeyword), (PeekToken(1).Kind != SyntaxKind.InKeyword) ? ParseType() : null, ParseIdentifierToken(), EatToken(SyntaxKind.InKeyword), ParseExpressionCore(), EatContextualToken(SyntaxKind.OnKeyword, ErrorCode.ERR_ExpectedContextualKeywordOn), ParseExpressionCore(), EatContextualToken(SyntaxKind.EqualsKeyword, ErrorCode.ERR_ExpectedContextualKeywordEquals), ParseExpressionCore(), (base.CurrentToken.ContextualKind == SyntaxKind.IntoKeyword) ? _syntaxFactory.JoinIntoClause(SyntaxParser.ConvertToKeyword(EatToken()), ParseIdentifierToken()) : null);
	}

	private LetClauseSyntax ParseLetClause()
	{
		return _syntaxFactory.LetClause(EatContextualToken(SyntaxKind.LetKeyword), (SyntaxFacts.IsReservedKeyword(base.CurrentToken.Kind) && PeekToken(1).Kind == SyntaxKind.EqualsToken) ? EatTokenAsKind(SyntaxKind.IdentifierToken) : ParseIdentifierToken(), EatToken(SyntaxKind.EqualsToken), ParseExpressionCore());
	}

	private WhereClauseSyntax ParseWhereClause()
	{
		return _syntaxFactory.WhereClause(EatContextualToken(SyntaxKind.WhereKeyword), ParseExpressionCore());
	}

	private OrderByClauseSyntax ParseOrderByClause()
	{
		SyntaxToken orderByKeyword = EatContextualToken(SyntaxKind.OrderByKeyword);
		SeparatedSyntaxListBuilder<OrderingSyntax> item = _pool.AllocateSeparated<OrderingSyntax>();
		item.Add(ParseOrdering());
		while (base.CurrentToken.Kind == SyntaxKind.CommaToken)
		{
			SyntaxKind kind = base.CurrentToken.Kind;
			if ((kind == SyntaxKind.CloseParenToken || kind == SyntaxKind.SemicolonToken) ? true : false)
			{
				break;
			}
			if (base.CurrentToken.Kind == SyntaxKind.CommaToken)
			{
				item.AddSeparator(EatToken(SyntaxKind.CommaToken));
				item.Add(ParseOrdering());
			}
			else if (skipBadOrderingListTokens(item, SyntaxKind.CommaToken) == PostSkipAction.Abort)
			{
				break;
			}
		}
		return _syntaxFactory.OrderByClause(orderByKeyword, _pool.ToListAndFree(in item));
		PostSkipAction skipBadOrderingListTokens(SeparatedSyntaxListBuilder<OrderingSyntax> list, SyntaxKind expected)
		{
			CSharpSyntaxNode startToken = null;
			return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken, (LanguageParser p, SyntaxKind _) => p.CurrentToken.Kind == SyntaxKind.CloseParenToken || p.CurrentToken.Kind == SyntaxKind.SemicolonToken || p.IsCurrentTokenQueryContextualKeyword, expected);
		}
	}

	private OrderingSyntax ParseOrdering()
	{
		ExpressionSyntax expression = ParseExpressionCore();
		SyntaxToken syntaxToken = null;
		SyntaxKind kind = SyntaxKind.AscendingOrdering;
		SyntaxKind contextualKind = base.CurrentToken.ContextualKind;
		if (contextualKind - 8432 <= SyntaxKind.List)
		{
			syntaxToken = SyntaxParser.ConvertToKeyword(EatToken());
			if (syntaxToken.Kind == SyntaxKind.DescendingKeyword)
			{
				kind = SyntaxKind.DescendingOrdering;
			}
		}
		return _syntaxFactory.Ordering(kind, expression, syntaxToken);
	}

	private SelectClauseSyntax ParseSelectClause()
	{
		return _syntaxFactory.SelectClause(EatContextualToken(SyntaxKind.SelectKeyword), ParseExpressionCore());
	}

	private GroupClauseSyntax ParseGroupClause()
	{
		return _syntaxFactory.GroupClause(EatContextualToken(SyntaxKind.GroupKeyword), ParseExpressionCore(), EatContextualToken(SyntaxKind.ByKeyword, ErrorCode.ERR_ExpectedContextualKeywordBy), ParseExpressionCore());
	}

	private QueryContinuationSyntax ParseQueryContinuation()
	{
		return _syntaxFactory.QueryContinuation(EatContextualToken(SyntaxKind.IntoKeyword), ParseIdentifierToken(), ParseQueryBody());
	}

	internal static bool MatchesFactoryContext(GreenNode green, SyntaxFactoryContext context)
	{
		if (context.IsInAsync == green.ParsedInAsync && context.IsInQuery == green.ParsedInQuery)
		{
			return context.IsInFieldKeywordContext == green.ParsedInFieldKeywordContext;
		}
		return false;
	}

	private Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TNode> ParseCommaSeparatedSyntaxList<TNode>(ref SyntaxToken openToken, SyntaxKind closeTokenKind, Func<LanguageParser, bool> isPossibleElement, Func<LanguageParser, TNode> parseElement, SkipBadTokens<TNode> skipBadTokens, bool allowTrailingSeparator, bool requireOneElement, bool allowSemicolonAsSeparator) where TNode : GreenNode
	{
		return ParseCommaSeparatedSyntaxList(ref openToken, closeTokenKind, isPossibleElement, parseElement, null, skipBadTokens, allowTrailingSeparator, requireOneElement, allowSemicolonAsSeparator);
	}

	private Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TNode> ParseCommaSeparatedSyntaxList<TNode>(ref SyntaxToken openToken, SyntaxKind closeTokenKind, Func<LanguageParser, bool> isPossibleElement, Func<LanguageParser, TNode> parseElement, Func<TNode, bool>? immediatelyAbort, SkipBadTokens<TNode> skipBadTokens, bool allowTrailingSeparator, bool requireOneElement, bool allowSemicolonAsSeparator) where TNode : GreenNode
	{
		SyntaxKind separatorTokenKind = SyntaxKind.CommaToken;
		SeparatedSyntaxListBuilder<TNode> item = _pool.AllocateSeparated<TNode>();
		while (requireOneElement || base.CurrentToken.Kind != closeTokenKind)
		{
			if (requireOneElement || shouldParseSeparatorOrElement())
			{
				TNode val = parseElement(this);
				item.Add(val);
				requireOneElement = false;
				int lastTokenPosition = -1;
				while ((immediatelyAbort == null || !immediatelyAbort(val)) && IsMakingProgress(ref lastTokenPosition) && base.CurrentToken.Kind != closeTokenKind)
				{
					if (shouldParseSeparatorOrElement())
					{
						item.AddSeparator((base.CurrentToken.Kind == SyntaxKind.SemicolonToken) ? EatTokenEvenWithIncorrectKind(separatorTokenKind) : EatToken(separatorTokenKind));
						if (allowTrailingSeparator)
						{
							if (base.CurrentToken.Kind == closeTokenKind)
							{
								break;
							}
							if (!isPossibleElement(this))
							{
								goto IL_0031;
							}
						}
						val = parseElement(this);
						item.Add(val);
					}
					else if (skipBadTokens(this, ref openToken, item, separatorTokenKind, closeTokenKind) == PostSkipAction.Abort)
					{
						break;
					}
				}
				break;
			}
			if (skipBadTokens(this, ref openToken, item, SyntaxKind.IdentifierToken, closeTokenKind) != PostSkipAction.Continue)
			{
				break;
			}
			IL_0031:;
		}
		return _pool.ToListAndFree(in item);
		bool shouldParseSeparatorOrElement()
		{
			if (base.CurrentToken.Kind == separatorTokenKind)
			{
				return true;
			}
			if (allowSemicolonAsSeparator && base.CurrentToken.Kind == SyntaxKind.SemicolonToken)
			{
				return true;
			}
			if (isPossibleElement(this))
			{
				return true;
			}
			return false;
		}
	}

	private DisposableResetPoint GetDisposableResetPoint(bool resetOnDispose)
	{
		return new DisposableResetPoint(this, resetOnDispose, GetResetPoint());
	}

	private new ResetPoint GetResetPoint()
	{
		return new ResetPoint(base.GetResetPoint(), _termState, IsInAsync, IsInQuery, IsInFieldKeywordContext);
	}

	private void Reset(ref ResetPoint state)
	{
		_termState = state.TerminatorState;
		IsInAsync = state.IsInAsync;
		IsInQuery = state.IsInQuery;
		IsInFieldKeywordContext = state.IsInFieldKeywordContext;
		Reset(ref state.BaseResetPoint);
	}

	private void Release(ref ResetPoint state)
	{
		Release(ref state.BaseResetPoint);
	}

	internal TNode ConsumeUnexpectedTokens<TNode>(TNode node) where TNode : CSharpSyntaxNode
	{
		if (base.CurrentToken.Kind == SyntaxKind.EndOfFileToken)
		{
			return node;
		}
		SyntaxListBuilder<SyntaxToken> syntaxListBuilder = _pool.Allocate<SyntaxToken>();
		while (base.CurrentToken.Kind != SyntaxKind.EndOfFileToken)
		{
			syntaxListBuilder.Add(EatToken());
		}
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> syntaxList = syntaxListBuilder.ToList();
		_pool.Free(syntaxListBuilder);
		node = AddError(node, ErrorCode.ERR_UnexpectedToken, syntaxList[0].ToString());
		node = AddTrailingSkippedSyntax(node, syntaxList.Node);
		return node;
	}

	private static bool ContainsErrorDiagnostic(GreenNode node)
	{
		if (node.ContainsDiagnostics)
		{
			ArrayBuilder<GreenNode> instance = ArrayBuilder<GreenNode>.GetInstance();
			try
			{
				instance.Push(node);
				while (instance.Count > 0)
				{
					GreenNode greenNode = instance.Pop();
					if (!greenNode.ContainsDiagnostics)
					{
						continue;
					}
					DiagnosticInfo[] diagnostics = greenNode.GetDiagnostics();
					for (int i = 0; i < diagnostics.Length; i++)
					{
						if (diagnostics[i].Severity == DiagnosticSeverity.Error)
						{
							return true;
						}
					}
					foreach (GreenNode item in greenNode.ChildNodesAndTokens())
					{
						instance.Push(item);
					}
				}
			}
			finally
			{
				instance.Free();
			}
		}
		return false;
	}

	private LiteralExpressionSyntax ParseRawStringToken()
	{
		SyntaxToken originalToken = EatToken();
		SyntaxKind literalExpression = SyntaxFacts.GetLiteralExpression(originalToken.Kind);
		InterpolatedStringExpressionSyntax interpolatedString = ParseInterpolatedOrRawStringToken(originalToken, isInterpolatedString: false);
		InterpolatedStringTextSyntax interpolatedText = (InterpolatedStringTextSyntax)interpolatedString.Contents[0];
		DiagnosticInfo[] diagnostics = getDiagnostics();
		SyntaxToken token = SyntaxFactory.Literal(originalToken.GetLeadingTrivia(), originalToken.Text, originalToken.Kind, getTokenValue(), originalToken.GetTrailingTrivia()).WithDiagnosticsGreen(diagnostics);
		return _syntaxFactory.LiteralExpression(literalExpression, token);
		DiagnosticInfo[] getDiagnostics()
		{
			ArrayBuilder<DiagnosticInfo> instance = ArrayBuilder<DiagnosticInfo>.GetInstance();
			instance.AddRange(interpolatedString.GetDiagnostics());
			DiagnosticInfo[] array = MoveDiagnostics(interpolatedText.GetDiagnostics(), interpolatedString.StringStartToken.Width);
			if (array != null)
			{
				instance.AddRange(array);
			}
			_ = originalToken.ContainsDiagnostics;
			return instance.ToArrayAndFree();
		}
		string getTokenValue()
		{
			if (diagnostics.Length == 0)
			{
				return interpolatedText.TextToken.GetValueText();
			}
			int i = 0;
			string text;
			for (text = originalToken.Text; i < text.Length && text[i] == '"'; i++)
			{
			}
			string text2 = text;
			int num = i;
			return text2.Substring(num, text2.Length - num);
		}
	}

	private InterpolatedStringExpressionSyntax ParseInterpolatedStringToken()
	{
		SyntaxToken originalToken = EatToken();
		return ParseInterpolatedOrRawStringToken(originalToken, isInterpolatedString: true);
	}

	private InterpolatedStringExpressionSyntax ParseInterpolatedOrRawStringToken(SyntaxToken originalToken, bool isInterpolatedString)
	{
		string originalText = originalToken.Text;
		ReadOnlySpan<char> originalTextSpan = System.MemoryExtensions.AsSpan(originalText);
		ArrayBuilder<Lexer.Interpolation> interpolations = ArrayBuilder<Lexer.Interpolation>.GetInstance();
		rescanInterpolation(out var kind, out var error, out var openQuoteRange, interpolations, out var closeQuoteRange);
		bool needsDedentation = kind == Lexer.InterpolatedStringKind.MultiLineRaw && error == null;
		InterpolatedStringExpressionSyntax interpolatedStringExpressionSyntax = SyntaxFactory.InterpolatedStringExpression(getOpenQuote(), getContent(originalTextSpan), getCloseQuote());
		if (error != null)
		{
			interpolatedStringExpressionSyntax = interpolatedStringExpressionSyntax.WithDiagnosticsGreen(new DiagnosticInfo[1] { error });
		}
		interpolations.Free();
		return interpolatedStringExpressionSyntax;
		SyntaxToken getCloseQuote()
		{
			int kind2 = kind switch
			{
				Lexer.InterpolatedStringKind.Normal => 8483, 
				Lexer.InterpolatedStringKind.Verbatim => 8483, 
				Lexer.InterpolatedStringKind.SingleLineRaw => 9074, 
				Lexer.InterpolatedStringKind.MultiLineRaw => 9074, 
				_ => throw ExceptionUtilities.UnexpectedValue(kind), 
			};
			string text = originalText;
			Range range = closeQuoteRange;
			return TokenOrMissingToken(null, (SyntaxKind)kind2, text[(Index)range.Start..(Index)range.End], originalToken.GetTrailingTrivia());
		}
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<InterpolatedStringContentSyntax> getContent(ReadOnlySpan<char> originalTextSpan2)
		{
			PooledStringBuilder instance = PooledStringBuilder.GetInstance();
			SyntaxListBuilder<InterpolatedStringContentSyntax> syntaxListBuilder = _pool.Allocate<InterpolatedStringContentSyntax>();
			ReadOnlySpan<char> indentationWhitespace = (needsDedentation ? getIndentationWhitespace(originalTextSpan2) : default(ReadOnlySpan<char>));
			Index end = openQuoteRange.End;
			Index start;
			Index index;
			int offset;
			int length;
			for (int i = 0; i < interpolations.Count; i++)
			{
				Lexer.Interpolation interpolation = interpolations[i];
				StringBuilder content = instance;
				bool isFirst = i == 0;
				index = end;
				start = interpolation.OpenBraceRange.Start;
				length = originalTextSpan2.Length;
				offset = index.GetOffset(length);
				syntaxListBuilder.Add(makeContent(indentationWhitespace, content, isFirst, isLast: false, originalTextSpan2.Slice(offset, start.GetOffset(length) - offset)));
				InterpolationSyntax node = ParseInterpolation(base.Options, originalText, interpolation, kind, IsInAsync, IsInFieldKeywordContext);
				SyntaxDiagnosticInfo syntaxDiagnosticInfo = getInterpolationIndentationError(indentationWhitespace, interpolation);
				if (syntaxDiagnosticInfo != null)
				{
					node = node.WithDiagnosticsGreen(new DiagnosticInfo[1] { syntaxDiagnosticInfo });
				}
				syntaxListBuilder.Add(node);
				end = interpolation.CloseBraceRange.End;
			}
			StringBuilder content2 = instance;
			bool isFirst2 = interpolations.Count == 0;
			start = end;
			index = closeQuoteRange.Start;
			offset = originalTextSpan2.Length;
			length = start.GetOffset(offset);
			syntaxListBuilder.Add(makeContent(indentationWhitespace, content2, isFirst2, isLast: true, originalTextSpan2.Slice(length, index.GetOffset(offset) - length)));
			Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<InterpolatedStringContentSyntax> result = syntaxListBuilder;
			_pool.Free(syntaxListBuilder);
			instance.Free();
			return result;
		}
		ReadOnlySpan<char> getIndentationWhitespace(ReadOnlySpan<char> readOnlySpan)
		{
			Range range = closeQuoteRange;
			ReadOnlySpan<char> text = readOnlySpan[(Index)range.Start..(Index)range.End];
			int newLineWidth = SlidingTextWindow.GetNewLineWidth(text[0], text[1]);
			int num = SkipWhitespace(text, newLineWidth);
			int num2 = newLineWidth;
			return text.Slice(num2, num - num2);
		}
		SyntaxDiagnosticInfo? getInterpolationIndentationError(ReadOnlySpan<char> indentationWhitespace, Lexer.Interpolation interpolation)
		{
			if (needsDedentation && !indentationWhitespace.IsEmpty)
			{
				int value = interpolation.OpenBraceRange.Start.Value;
				if (value > 0 && SyntaxFacts.IsNewLine(originalText[value - 1]))
				{
					return SyntaxParser.MakeError(0, 1, ErrorCode.ERR_LineDoesNotStartWithSameWhitespace);
				}
			}
			return null;
		}
		SyntaxToken getOpenQuote()
		{
			GreenNode leadingTrivia = originalToken.GetLeadingTrivia();
			int kind2 = kind switch
			{
				Lexer.InterpolatedStringKind.Normal => 8482, 
				Lexer.InterpolatedStringKind.Verbatim => 8484, 
				Lexer.InterpolatedStringKind.SingleLineRaw => 9072, 
				Lexer.InterpolatedStringKind.MultiLineRaw => 9073, 
				_ => throw ExceptionUtilities.UnexpectedValue(kind), 
			};
			string text = originalText;
			Range range = openQuoteRange;
			return SyntaxFactory.Token(leadingTrivia, (SyntaxKind)kind2, text[(Index)range.Start..(Index)range.End], null);
		}
		InterpolatedStringContentSyntax? makeContent(ReadOnlySpan<char> indentationWhitespace, StringBuilder content, bool isFirst, bool isLast, ReadOnlySpan<char> text)
		{
			if (text.IsEmpty)
			{
				if (!isInterpolatedString)
				{
					return SyntaxFactory.InterpolatedStringText(SyntaxFactory.Literal(null, "", SyntaxKind.InterpolatedStringTextToken, "", null));
				}
				return null;
			}
			if (!needsDedentation || indentationWhitespace.IsEmpty)
			{
				return SyntaxFactory.InterpolatedStringText(MakeInterpolatedStringTextToken(kind, text.ToString()));
			}
			content.Clear();
			int num = 0;
			if (!isFirst)
			{
				num = ConsumeRemainingContentThroughNewLine(content, text, num);
			}
			SyntaxDiagnosticInfo syntaxDiagnosticInfo = null;
			while (num < text.Length)
			{
				int num2 = num;
				if (syntaxDiagnosticInfo == null)
				{
					num = SkipWhitespace(text, num);
					int num3 = num2;
					ReadOnlySpan<char> readOnlySpan = text.Slice(num3, num - num3);
					if (!readOnlySpan.StartsWith(indentationWhitespace) && ((!((num == text.Length) & isLast) && (num >= text.Length || !SyntaxFacts.IsNewLine(text[num]))) || !indentationWhitespace.StartsWith(readOnlySpan)))
					{
						if (CheckForSpaceDifference(readOnlySpan, indentationWhitespace, out string currentLineMessage, out string indentationLineMessage))
						{
							if (syntaxDiagnosticInfo == null)
							{
								syntaxDiagnosticInfo = SyntaxParser.MakeError(num2, num - num2, ErrorCode.ERR_LineContainsDifferentWhitespace, currentLineMessage, indentationLineMessage);
							}
						}
						else if (syntaxDiagnosticInfo == null)
						{
							syntaxDiagnosticInfo = SyntaxParser.MakeError(num2, num - num2, ErrorCode.ERR_LineDoesNotStartWithSameWhitespace);
						}
					}
				}
				num = Math.Min(num, num2 + indentationWhitespace.Length);
				num = ConsumeRemainingContentThroughNewLine(content, text, num);
			}
			string text2 = text.ToString();
			string value = ((syntaxDiagnosticInfo != null) ? text2 : content.ToString());
			InterpolatedStringTextSyntax interpolatedStringTextSyntax = SyntaxFactory.InterpolatedStringText(SyntaxFactory.Literal(null, text2, SyntaxKind.InterpolatedStringTextToken, value, null));
			if (syntaxDiagnosticInfo == null)
			{
				return interpolatedStringTextSyntax;
			}
			return interpolatedStringTextSyntax.WithDiagnosticsGreen(new DiagnosticInfo[1] { syntaxDiagnosticInfo });
		}
		void rescanInterpolation(out Lexer.InterpolatedStringKind kind2, out SyntaxDiagnosticInfo? error2, out Range openQuoteRange2, ArrayBuilder<Lexer.Interpolation> interpolations2, out Range closeQuoteRange2)
		{
			using Lexer lexer = new Lexer(SourceText.From(originalText), base.Options, allowPreprocessorDirectives: false);
			Lexer.TokenInfo info = default(Lexer.TokenInfo);
			lexer.ScanInterpolatedOrRawStringLiteralTop(ref info, isInterpolatedString, out error2, out kind2, out openQuoteRange2, interpolations2, out closeQuoteRange2);
		}
	}

	private static string CharToString(char ch)
	{
		return ch switch
		{
			'\t' => "\\t", 
			'\v' => "\\v", 
			'\f' => "\\f", 
			_ => $"\\u{(int)ch:x4}", 
		};
	}

	private static bool CheckForSpaceDifference(ReadOnlySpan<char> currentLineWhitespace, ReadOnlySpan<char> indentationLineWhitespace, [NotNullWhen(true)] out string? currentLineMessage, [NotNullWhen(true)] out string? indentationLineMessage)
	{
		int i = 0;
		for (int num = Math.Min(currentLineWhitespace.Length, indentationLineWhitespace.Length); i < num; i++)
		{
			char c = currentLineWhitespace[i];
			char c2 = indentationLineWhitespace[i];
			if (c != c2 && SyntaxFacts.IsWhitespace(c) && SyntaxFacts.IsWhitespace(c2))
			{
				currentLineMessage = CharToString(c);
				indentationLineMessage = CharToString(c2);
				return true;
			}
		}
		currentLineMessage = null;
		indentationLineMessage = null;
		return false;
	}

	private static SyntaxToken TokenOrMissingToken(GreenNode? leading, SyntaxKind kind, string text, GreenNode? trailing)
	{
		if (!(text == ""))
		{
			return SyntaxFactory.Token(leading, kind, text, trailing);
		}
		return SyntaxFactory.MissingToken(leading, kind, trailing);
	}

	private static int SkipWhitespace(ReadOnlySpan<char> text, int currentIndex)
	{
		while (currentIndex < text.Length && SyntaxFacts.IsWhitespace(text[currentIndex]))
		{
			currentIndex++;
		}
		return currentIndex;
	}

	private unsafe static int ConsumeRemainingContentThroughNewLine(StringBuilder content, ReadOnlySpan<char> text, int currentIndex)
	{
		int num = currentIndex;
		while (currentIndex < text.Length)
		{
			char c = text[currentIndex];
			if (!SyntaxFacts.IsNewLine(c))
			{
				currentIndex++;
				continue;
			}
			currentIndex += SlidingTextWindow.GetNewLineWidth(c, (currentIndex + 1 < text.Length) ? text[currentIndex + 1] : '\0');
			break;
		}
		int num2 = num;
		ReadOnlySpan<char> readOnlySpan = text.Slice(num2, currentIndex - num2);
		fixed (char* value = readOnlySpan)
		{
			content.Append(value, readOnlySpan.Length);
		}
		return currentIndex;
	}

	private static InterpolationSyntax ParseInterpolation(CSharpParseOptions options, string text, Lexer.Interpolation interpolation, Lexer.InterpolatedStringKind kind, bool isInAsyncContext, bool isInFieldKeywordContext)
	{
		Range range = (interpolation.HasColon ? interpolation.ColonRange : interpolation.CloseBraceRange);
		Index end = interpolation.OpenBraceRange.End;
		Index start = range.Start;
		int length = text.Length;
		int offset = end.GetOffset(length);
		using Lexer lexer = new Lexer(SourceText.From(text.Substring(offset, start.GetOffset(length) - offset)), options, allowPreprocessorDirectives: false, interpolation.HasColon);
		GreenNode node = lexer.LexSyntaxTrailingTrivia().Node;
		using LanguageParser languageParser = new LanguageParser(lexer, null, null);
		bool? isInAsyncContext2 = isInAsyncContext;
		bool? isInFieldKeywordContext2 = isInFieldKeywordContext;
		using (new ParserSyntaxContextResetter(languageParser, isInAsyncContext2, null, isInFieldKeywordContext2))
		{
			Lexer.Interpolation interpolation2 = interpolation;
			Range openBraceRange = interpolation.OpenBraceRange;
			return languageParser.ParseInterpolation(text, interpolation2, kind, SyntaxFactory.Token(null, SyntaxKind.OpenBraceToken, text[(Index)openBraceRange.Start..(Index)openBraceRange.End], node));
		}
	}

	private InterpolationSyntax ParseInterpolation(string text, Lexer.Interpolation interpolation, Lexer.InterpolatedStringKind kind, SyntaxToken openBraceToken)
	{
		var (expression, alignmentClause) = getExpressionAndAlignment();
		var (formatClause, closeBraceToken) = getFormatAndCloseBrace();
		return SyntaxFactory.Interpolation(openBraceToken, expression, alignmentClause, formatClause, closeBraceToken);
		(ExpressionSyntax expression, InterpolationAlignmentClauseSyntax? alignment) getExpressionAndAlignment()
		{
			ExpressionSyntax expressionSyntax = ParseExpressionCore();
			if (base.CurrentToken.Kind != SyntaxKind.CommaToken)
			{
				return (expression: ConsumeUnexpectedTokens(expressionSyntax), alignment: null);
			}
			InterpolationAlignmentClauseSyntax item = SyntaxFactory.InterpolationAlignmentClause(EatToken(SyntaxKind.CommaToken), ConsumeUnexpectedTokens(ParseExpressionCore()));
			return (expression: expressionSyntax, alignment: item);
		}
		(InterpolationFormatClauseSyntax? format, SyntaxToken closeBraceToken) getFormatAndCloseBrace()
		{
			GreenNode leadingTrivia = base.CurrentToken.GetLeadingTrivia();
			if (interpolation.HasColon)
			{
				string text2 = text;
				Range colonRange = interpolation.ColonRange;
				SyntaxToken colonToken = SyntaxFactory.Token(leadingTrivia, SyntaxKind.ColonToken, text2[(Index)colonRange.Start..(Index)colonRange.End], null);
				Lexer.InterpolatedStringKind kind2 = kind;
				string text3 = text;
				Index end = interpolation.ColonRange.End;
				Index start = interpolation.CloseBraceRange.Start;
				int length = text3.Length;
				int offset = end.GetOffset(length);
				return (format: SyntaxFactory.InterpolationFormatClause(colonToken, MakeInterpolatedStringTextToken(kind2, text3.Substring(offset, start.GetOffset(length) - offset))), closeBraceToken: getInterpolationCloseToken(null));
			}
			return (format: null, closeBraceToken: getInterpolationCloseToken(leadingTrivia));
		}
		SyntaxToken getInterpolationCloseToken(GreenNode? leading)
		{
			string text2 = text;
			Range closeBraceRange = interpolation.CloseBraceRange;
			return TokenOrMissingToken(leading, SyntaxKind.CloseBraceToken, text2[(Index)closeBraceRange.Start..(Index)closeBraceRange.End], null);
		}
	}

	private SyntaxToken MakeInterpolatedStringTextToken(Lexer.InterpolatedStringKind kind, string text)
	{
		if ((uint)(kind - 2) <= 1u)
		{
			return SyntaxFactory.Literal(null, text, SyntaxKind.InterpolatedStringTextToken, text, null);
		}
		string text2 = ((kind == Lexer.InterpolatedStringKind.Verbatim) ? "@\"" : "\"");
		using Lexer lexer = new Lexer(SourceText.From(text2 + text + "\""), base.Options, allowPreprocessorDirectives: false);
		LexerMode mode = LexerMode.Syntax;
		SyntaxToken syntaxToken = lexer.Lex(ref mode);
		SyntaxToken syntaxToken2 = SyntaxFactory.Literal(null, text, SyntaxKind.InterpolatedStringTextToken, syntaxToken.ValueText, null);
		if (syntaxToken.ContainsDiagnostics)
		{
			syntaxToken2 = syntaxToken2.WithDiagnosticsGreen(MoveDiagnostics(syntaxToken.GetDiagnostics(), -text2.Length));
		}
		return syntaxToken2;
	}

	private static DiagnosticInfo[]? MoveDiagnostics(DiagnosticInfo[]? infos, int offset)
	{
		if ((infos == null || infos.Length == 0) ? true : false)
		{
			return null;
		}
		ArrayBuilder<DiagnosticInfo> instance = ArrayBuilder<DiagnosticInfo>.GetInstance(infos.Length);
		for (int i = 0; i < infos.Length; i++)
		{
			SyntaxDiagnosticInfo syntaxDiagnosticInfo = (SyntaxDiagnosticInfo)infos[i];
			instance.Add(syntaxDiagnosticInfo.WithOffset(syntaxDiagnosticInfo.Offset + offset));
		}
		return instance.ToArrayAndFree();
	}

	private CSharpSyntaxNode ParseTypeOrPatternForIsOperator()
	{
		PatternSyntax patternSyntax = ParsePattern(GetPrecedence(SyntaxKind.IsPatternExpression), afterIs: true);
		if (!(patternSyntax is ConstantPatternSyntax constantPatternSyntax))
		{
			if (patternSyntax is TypePatternSyntax typePatternSyntax)
			{
				return typePatternSyntax.Type;
			}
			if (patternSyntax is DiscardPatternSyntax discardPatternSyntax)
			{
				DiscardPatternSyntax discardPatternSyntax2 = discardPatternSyntax;
				return _syntaxFactory.IdentifierName(SyntaxParser.ConvertToIdentifier(discardPatternSyntax2.UnderscoreToken));
			}
		}
		else
		{
			ConstantPatternSyntax constantPatternSyntax2 = constantPatternSyntax;
			if (ConvertExpressionToType(constantPatternSyntax2.Expression, out NameSyntax type))
			{
				return type;
			}
		}
		return patternSyntax;
	}

	private bool ConvertExpressionToType(ExpressionSyntax expression, [NotNullWhen(true)] out NameSyntax? type)
	{
		if (!(expression is SimpleNameSyntax simpleNameSyntax))
		{
			if (expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
			{
				ExpressionSyntax expression2 = memberAccessExpressionSyntax.Expression;
				SyntaxToken operatorToken = memberAccessExpressionSyntax.OperatorToken;
				if (operatorToken != null && operatorToken.Kind == SyntaxKind.DotToken)
				{
					SimpleNameSyntax name = memberAccessExpressionSyntax.Name;
					ExpressionSyntax expression3 = expression2;
					SyntaxToken dotToken = operatorToken;
					SimpleNameSyntax right = name;
					if (ConvertExpressionToType(expression3, out NameSyntax type2))
					{
						type = _syntaxFactory.QualifiedName(type2, dotToken, right);
						return true;
					}
				}
			}
			else if (expression is AliasQualifiedNameSyntax aliasQualifiedNameSyntax)
			{
				AliasQualifiedNameSyntax aliasQualifiedNameSyntax2 = aliasQualifiedNameSyntax;
				type = aliasQualifiedNameSyntax2;
				return true;
			}
			type = null;
			return false;
		}
		SimpleNameSyntax simpleNameSyntax2 = simpleNameSyntax;
		type = simpleNameSyntax2;
		return true;
	}

	private PatternSyntax ParsePattern(Precedence precedence, bool afterIs = false, bool inSwitchArmPattern = false)
	{
		return ParseDisjunctivePattern(precedence, afterIs, inSwitchArmPattern);
	}

	private PatternSyntax ParseDisjunctivePattern(Precedence precedence, bool afterIs, bool inSwitchArmPattern)
	{
		PatternSyntax patternSyntax = ParseConjunctivePattern(precedence, afterIs, inSwitchArmPattern);
		while (base.CurrentToken.ContextualKind == SyntaxKind.OrKeyword)
		{
			patternSyntax = _syntaxFactory.BinaryPattern(SyntaxKind.OrPattern, patternSyntax, SyntaxParser.ConvertToKeyword(EatToken()), ParseConjunctivePattern(precedence, afterIs, inSwitchArmPattern));
		}
		return patternSyntax;
	}

	private bool LooksLikeTypeOfPattern()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		if (SyntaxFacts.IsPredefinedType(kind))
		{
			return true;
		}
		if (kind == SyntaxKind.IdentifierToken && base.CurrentToken.ContextualKind != SyntaxKind.UnderscoreToken && (base.CurrentToken.ContextualKind != SyntaxKind.NameOfKeyword || PeekToken(1).Kind != SyntaxKind.OpenParenToken))
		{
			return true;
		}
		if (LooksLikeTupleArrayType())
		{
			return true;
		}
		if (IsFunctionPointerStart())
		{
			return true;
		}
		return false;
	}

	private PatternSyntax ParseConjunctivePattern(Precedence precedence, bool afterIs, bool inSwitchArmPattern)
	{
		PatternSyntax patternSyntax = ParseNegatedPattern(precedence, afterIs, inSwitchArmPattern);
		while (base.CurrentToken.ContextualKind == SyntaxKind.AndKeyword)
		{
			patternSyntax = _syntaxFactory.BinaryPattern(SyntaxKind.AndPattern, patternSyntax, SyntaxParser.ConvertToKeyword(EatToken()), ParseNegatedPattern(precedence, afterIs, inSwitchArmPattern));
		}
		return patternSyntax;
	}

	private bool ScanDesignation(bool permitTuple)
	{
		switch (base.CurrentToken.Kind)
		{
		default:
			return false;
		case SyntaxKind.IdentifierToken:
		{
			bool result2 = IsTrueIdentifier();
			EatToken();
			return result2;
		}
		case SyntaxKind.OpenParenToken:
		{
			if (!permitTuple)
			{
				return false;
			}
			bool result = false;
			while (true)
			{
				EatToken();
				if (!ScanDesignation(permitTuple: true))
				{
					break;
				}
				switch (base.CurrentToken.Kind)
				{
				case SyntaxKind.CloseParenToken:
					EatToken();
					return result;
				case SyntaxKind.CommaToken:
					break;
				default:
					return false;
				}
				result = true;
			}
			return false;
		}
		}
	}

	private PatternSyntax ParseNegatedPattern(Precedence precedence, bool afterIs, bool inSwitchArmPattern)
	{
		if (base.CurrentToken.ContextualKind == SyntaxKind.NotKeyword)
		{
			return _syntaxFactory.UnaryPattern(SyntaxParser.ConvertToKeyword(EatToken()), ParseNegatedPattern(precedence, afterIs, inSwitchArmPattern));
		}
		if (base.CurrentToken.Kind == SyntaxKind.EqualsEqualsToken)
		{
			return AddLeadingSkippedSyntax<PatternSyntax>(skippedSyntax: AddError(EatToken(), ErrorCode.ERR_EqualityOperatorInPatternNotSupported), node: ParseNegatedPattern(precedence, afterIs, inSwitchArmPattern));
		}
		if (base.CurrentToken.Kind == SyntaxKind.ExclamationEqualsToken)
		{
			return _syntaxFactory.UnaryPattern(AddTrailingSkippedSyntax(SyntaxFactory.MissingToken(SyntaxKind.NotKeyword), AddError(EatToken(), ErrorCode.ERR_InequalityOperatorInPatternNotSupported)), ParseNegatedPattern(precedence, afterIs, inSwitchArmPattern));
		}
		return ParsePrimaryPattern(precedence, afterIs, inSwitchArmPattern);
	}

	private PatternSyntax ParsePrimaryPattern(Precedence precedence, bool afterIs, bool inSwitchArmPattern)
	{
		switch (base.CurrentToken.Kind)
		{
		case SyntaxKind.CloseParenToken:
		case SyntaxKind.CloseBraceToken:
		case SyntaxKind.CloseBracketToken:
		case SyntaxKind.SemicolonToken:
		case SyntaxKind.CommaToken:
		case SyntaxKind.EqualsGreaterThanToken:
			return _syntaxFactory.ConstantPattern(ParseIdentifierName(ErrorCode.ERR_MissingPattern));
		default:
		{
			if (base.CurrentToken.ContextualKind == SyntaxKind.UnderscoreToken)
			{
				return _syntaxFactory.DiscardPattern(EatContextualToken(SyntaxKind.UnderscoreToken));
			}
			switch (base.CurrentToken.Kind)
			{
			case SyntaxKind.OpenBracketToken:
				return ParseListPattern(inSwitchArmPattern);
			case SyntaxKind.DotToken:
				if (IsAtDotDotToken())
				{
					return _syntaxFactory.SlicePattern(EatDotDotToken(), IsPossibleSubpatternElement() ? ParsePattern(precedence, afterIs: false, inSwitchArmPattern) : null);
				}
				break;
			case SyntaxKind.LessThanToken:
			case SyntaxKind.GreaterThanToken:
			case SyntaxKind.ExclamationEqualsToken:
			case SyntaxKind.EqualsEqualsToken:
			case SyntaxKind.LessThanEqualsToken:
			case SyntaxKind.GreaterThanEqualsToken:
				return _syntaxFactory.RelationalPattern(EatToken(), ParseSubExpression(Precedence.Relational));
			}
			using DisposableResetPoint disposableResetPoint = GetDisposableResetPoint(resetOnDispose: false);
			TypeSyntax typeSyntax = null;
			if (LooksLikeTypeOfPattern())
			{
				typeSyntax = ParseType(afterIs ? ParseTypeMode.AfterIs : ParseTypeMode.DefinitePattern);
				if (typeSyntax.IsMissing || !CanTokenFollowTypeInPattern(precedence))
				{
					disposableResetPoint.Reset();
					typeSyntax = null;
				}
			}
			PatternSyntax patternSyntax = ParsePatternContinued(typeSyntax, precedence, inSwitchArmPattern);
			if (patternSyntax != null)
			{
				return patternSyntax;
			}
			disposableResetPoint.Reset();
			ExpressionSyntax expression = ParseSubExpression(precedence);
			return _syntaxFactory.ConstantPattern(expression);
		}
		}
	}

	private bool CanTokenFollowTypeInPattern(Precedence precedence)
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		switch (kind)
		{
		case SyntaxKind.OpenParenToken:
		case SyntaxKind.CloseParenToken:
		case SyntaxKind.OpenBraceToken:
		case SyntaxKind.CloseBraceToken:
		case SyntaxKind.CloseBracketToken:
		case SyntaxKind.SemicolonToken:
		case SyntaxKind.CommaToken:
		case SyntaxKind.IdentifierToken:
			return true;
		case SyntaxKind.DotToken:
			return false;
		case SyntaxKind.ExclamationToken:
		case SyntaxKind.MinusGreaterThanToken:
			return false;
		default:
			if (SyntaxFacts.IsBinaryExpressionOperatorToken(kind))
			{
				return GetPrecedence(SyntaxFacts.GetBinaryExpression(kind)) <= precedence;
			}
			return true;
		}
	}

	private PatternSyntax? ParsePatternContinued(TypeSyntax? type, Precedence precedence, bool inSwitchArmPattern)
	{
		if (type != null && type.Kind == SyntaxKind.IdentifierName)
		{
			SyntaxToken identifier = ((IdentifierNameSyntax)type).Identifier;
			if (identifier.ContextualKind == SyntaxKind.VarKeyword && (base.CurrentToken.Kind == SyntaxKind.OpenParenToken || IsValidPatternDesignation(inSwitchArmPattern)))
			{
				SyntaxToken varKeyword = SyntaxParser.ConvertToKeyword(identifier);
				VariableDesignationSyntax designation = ParseDesignation(forPattern: true);
				return _syntaxFactory.VarPattern(varKeyword, designation);
			}
		}
		if (base.CurrentToken.Kind == SyntaxKind.OpenParenToken && (type != null || !looksLikeCast()))
		{
			SyntaxToken openToken = EatToken(SyntaxKind.OpenParenToken);
			Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<SubpatternSyntax> subpatterns = ParseCommaSeparatedSyntaxList(ref openToken, SyntaxKind.CloseParenToken, (LanguageParser @this) => @this.IsPossibleSubpatternElement(), (LanguageParser @this) => @this.ParseSubpatternElement(), SkipBadPatternListTokens, allowTrailingSeparator: false, requireOneElement: false, allowSemicolonAsSeparator: false);
			SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
			parsePropertyPatternClause(out var propertyPatternClauseResult);
			VariableDesignationSyntax variableDesignationSyntax = TryParseSimpleDesignation(inSwitchArmPattern);
			if (type == null && propertyPatternClauseResult == null && variableDesignationSyntax == null && subpatterns.Count == 1 && subpatterns.SeparatorCount == 0)
			{
				SubpatternSyntax subpatternSyntax = subpatterns[0];
				if (subpatternSyntax.ExpressionColon == null)
				{
					PatternSyntax pattern = subpatternSyntax.Pattern;
					if (pattern is ConstantPatternSyntax constantPatternSyntax)
					{
						ExpressionSyntax unaryOrPrimaryExpression = _syntaxFactory.ParenthesizedExpression(openToken, constantPatternSyntax.Expression, closeParenToken);
						unaryOrPrimaryExpression = ParseExpressionContinued(unaryOrPrimaryExpression, precedence);
						return _syntaxFactory.ConstantPattern(unaryOrPrimaryExpression);
					}
					return _syntaxFactory.ParenthesizedPattern(openToken, pattern, closeParenToken);
				}
			}
			PositionalPatternClauseSyntax positionalPatternClause = _syntaxFactory.PositionalPatternClause(openToken, subpatterns, closeParenToken);
			return _syntaxFactory.RecursivePattern(type, positionalPatternClause, propertyPatternClauseResult, variableDesignationSyntax);
		}
		if (parsePropertyPatternClause(out var propertyPatternClauseResult2))
		{
			return _syntaxFactory.RecursivePattern(type, null, propertyPatternClauseResult2, TryParseSimpleDesignation(inSwitchArmPattern));
		}
		if (type != null)
		{
			VariableDesignationSyntax variableDesignationSyntax2 = TryParseSimpleDesignation(inSwitchArmPattern);
			if (variableDesignationSyntax2 != null)
			{
				return _syntaxFactory.DeclarationPattern(type, variableDesignationSyntax2);
			}
			if (!ConvertTypeToExpression(type, out ExpressionSyntax expr))
			{
				return _syntaxFactory.TypePattern(type);
			}
			return _syntaxFactory.ConstantPattern(ParseExpressionContinued(expr, precedence));
		}
		return null;
		bool looksLikeCast()
		{
			using (GetDisposableResetPoint(resetOnDispose: true))
			{
				return ScanCast(forPattern: true);
			}
		}
		bool parsePropertyPatternClause([NotNullWhen(true)] out PropertyPatternClauseSyntax? reference)
		{
			SyntaxToken skippedSyntax = ((IsTrueIdentifier() && IsValidPatternDesignation(inSwitchArmPattern) && PeekToken(1).Kind == SyntaxKind.OpenBraceToken) ? AddError(EatToken(), ErrorCode.ERR_DesignatorBeforePropertyPattern) : null);
			if (base.CurrentToken.Kind == SyntaxKind.OpenBraceToken)
			{
				reference = AddLeadingSkippedSyntax(ParsePropertyPatternClause(), skippedSyntax);
				return true;
			}
			reference = null;
			return false;
		}
	}

	private VariableDesignationSyntax? TryParseSimpleDesignation(bool whenIsKeyword)
	{
		if (!IsTrueIdentifier() || !IsValidPatternDesignation(whenIsKeyword))
		{
			return null;
		}
		return ParseSimpleDesignation();
	}

	private bool IsValidPatternDesignation(bool whenIsKeyword)
	{
		if (base.CurrentToken.Kind == SyntaxKind.IdentifierToken)
		{
			switch (base.CurrentToken.ContextualKind)
			{
			case SyntaxKind.WhenKeyword:
				return !whenIsKeyword;
			case SyntaxKind.OrKeyword:
			case SyntaxKind.AndKeyword:
			{
				SyntaxKind kind = PeekToken(1).Kind;
				switch (kind)
				{
				case SyntaxKind.CloseParenToken:
				case SyntaxKind.CloseBraceToken:
				case SyntaxKind.CloseBracketToken:
				case SyntaxKind.ColonToken:
				case SyntaxKind.SemicolonToken:
				case SyntaxKind.CommaToken:
				case SyntaxKind.QuestionToken:
					return true;
				case SyntaxKind.OpenParenToken:
				case SyntaxKind.OpenBraceToken:
				case SyntaxKind.OpenBracketToken:
				case SyntaxKind.LessThanToken:
				case SyntaxKind.GreaterThanToken:
				case SyntaxKind.LessThanEqualsToken:
				case SyntaxKind.GreaterThanEqualsToken:
				case SyntaxKind.IdentifierToken:
					return false;
				default:
					if (SyntaxFacts.IsBinaryExpression(kind))
					{
						return true;
					}
					using (GetDisposableResetPoint(resetOnDispose: true))
					{
						EatToken();
						return !CanStartExpression();
					}
				}
			}
			default:
				return true;
			}
		}
		return false;
	}

	private CSharpSyntaxNode ParseExpressionOrPatternForSwitchStatement()
	{
		TerminatorState termState = _termState;
		_termState |= TerminatorState.IsExpressionOrPatternInCaseLabelOfSwitchStatement;
		PatternSyntax pattern = ParsePattern(Precedence.Conditional, afterIs: false, inSwitchArmPattern: true);
		_termState = termState;
		return ConvertPatternToExpressionIfPossible(pattern);
	}

	private CSharpSyntaxNode ConvertPatternToExpressionIfPossible(PatternSyntax pattern, bool permitTypeArguments = false)
	{
		if (!(pattern is ConstantPatternSyntax constantPatternSyntax))
		{
			if (!(pattern is TypePatternSyntax typePatternSyntax))
			{
				if (pattern is DiscardPatternSyntax discardPatternSyntax)
				{
					DiscardPatternSyntax discardPatternSyntax2 = discardPatternSyntax;
					return _syntaxFactory.IdentifierName(SyntaxParser.ConvertToIdentifier(discardPatternSyntax2.UnderscoreToken));
				}
			}
			else
			{
				TypePatternSyntax typePatternSyntax2 = typePatternSyntax;
				if (ConvertTypeToExpression(typePatternSyntax2.Type, out ExpressionSyntax expr, permitTypeArguments))
				{
					return expr;
				}
			}
			return pattern;
		}
		return constantPatternSyntax.Expression;
	}

	private bool ConvertTypeToExpression(TypeSyntax type, [NotNullWhen(true)] out ExpressionSyntax? expr, bool permitTypeArguments = false)
	{
		if (!(type is GenericNameSyntax genericNameSyntax))
		{
			if (!(type is SimpleNameSyntax simpleNameSyntax))
			{
				if (type is QualifiedNameSyntax qualifiedNameSyntax)
				{
					NameSyntax left = qualifiedNameSyntax.Left;
					SyntaxToken dotToken = qualifiedNameSyntax.dotToken;
					SimpleNameSyntax right = qualifiedNameSyntax.Right;
					if (permitTypeArguments || !(right is GenericNameSyntax))
					{
						ExpressionSyntax expression = (ConvertTypeToExpression(left, out ExpressionSyntax expr2, permitTypeArguments: true) ? expr2 : left);
						expr = _syntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, dotToken, right);
						return true;
					}
				}
				expr = null;
				return false;
			}
			expr = simpleNameSyntax;
			return true;
		}
		expr = genericNameSyntax;
		return permitTypeArguments;
	}

	private bool LooksLikeTupleArrayType()
	{
		if (base.CurrentToken.Kind != SyntaxKind.OpenParenToken)
		{
			return false;
		}
		using (GetDisposableResetPoint(resetOnDispose: true))
		{
			return ScanType(forPattern: true) != ScanTypeFlags.NotType;
		}
	}

	private PropertyPatternClauseSyntax ParsePropertyPatternClause()
	{
		SyntaxToken openToken = EatToken(SyntaxKind.OpenBraceToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<SubpatternSyntax> subpatterns = ParseCommaSeparatedSyntaxList(ref openToken, SyntaxKind.CloseBraceToken, (LanguageParser @this) => @this.IsPossibleSubpatternElement(), (LanguageParser @this) => @this.ParseSubpatternElement(), SkipBadPatternListTokens, allowTrailingSeparator: true, requireOneElement: false, allowSemicolonAsSeparator: false);
		return _syntaxFactory.PropertyPatternClause(openToken, subpatterns, EatToken(SyntaxKind.CloseBraceToken));
	}

	private SubpatternSyntax ParseSubpatternElement()
	{
		BaseExpressionColonSyntax expressionColon = null;
		PatternSyntax pattern = ParsePattern(Precedence.Conditional);
		if (base.CurrentToken.Kind == SyntaxKind.ColonToken && ConvertPatternToExpressionIfPossible(pattern, permitTypeArguments: true) is ExpressionSyntax expressionSyntax)
		{
			SyntaxToken colonToken = EatToken();
			expressionColon = ((expressionSyntax is IdentifierNameSyntax name) ? ((BaseExpressionColonSyntax)_syntaxFactory.NameColon(name, colonToken)) : ((BaseExpressionColonSyntax)_syntaxFactory.ExpressionColon(expressionSyntax, colonToken)));
			pattern = ParsePattern(Precedence.Conditional);
		}
		return _syntaxFactory.Subpattern(expressionColon, pattern);
	}

	private bool IsPossibleSubpatternElement()
	{
		bool flag = CanStartExpression();
		if (!flag)
		{
			bool flag2;
			switch (base.CurrentToken.Kind)
			{
			case SyntaxKind.OpenBraceToken:
			case SyntaxKind.OpenBracketToken:
			case SyntaxKind.LessThanToken:
			case SyntaxKind.GreaterThanToken:
			case SyntaxKind.LessThanEqualsToken:
			case SyntaxKind.GreaterThanEqualsToken:
				flag2 = true;
				break;
			default:
				flag2 = false;
				break;
			}
			flag = flag2;
		}
		return flag;
	}

	private static PostSkipAction SkipBadPatternListTokens<T>(LanguageParser @this, ref SyntaxToken open, SeparatedSyntaxListBuilder<T> list, SyntaxKind expectedKind, SyntaxKind closeKind) where T : CSharpSyntaxNode
	{
		bool flag;
		switch (@this.CurrentToken.Kind)
		{
		case SyntaxKind.CloseParenToken:
		case SyntaxKind.CloseBraceToken:
		case SyntaxKind.CloseBracketToken:
		case SyntaxKind.SemicolonToken:
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			return PostSkipAction.Abort;
		}
		if (@this._termState.HasFlag(TerminatorState.IsExpressionOrPatternInCaseLabelOfSwitchStatement) && @this.CurrentToken.Kind == SyntaxKind.ColonToken)
		{
			return PostSkipAction.Abort;
		}
		flag = @this._termState.HasFlag(TerminatorState.IsPatternInSwitchExpressionArm);
		if (flag)
		{
			SyntaxKind kind = @this.CurrentToken.Kind;
			bool flag2 = ((kind == SyntaxKind.ColonToken || kind == SyntaxKind.EqualsGreaterThanToken) ? true : false);
			flag = flag2;
		}
		if (flag)
		{
			return PostSkipAction.Abort;
		}
		return @this.SkipBadSeparatedListTokensWithExpectedKind(ref open, list, (LanguageParser p) => p.CurrentToken.Kind != SyntaxKind.CommaToken && !p.IsPossibleSubpatternElement(), (LanguageParser p, SyntaxKind syntaxKind) => p.CurrentToken.Kind == syntaxKind || p.CurrentToken.Kind == SyntaxKind.SemicolonToken, expectedKind, closeKind);
	}

	private SwitchExpressionSyntax ParseSwitchExpression(ExpressionSyntax governingExpression, SyntaxToken switchKeyword)
	{
		return _syntaxFactory.SwitchExpression(governingExpression, switchKeyword, EatToken(SyntaxKind.OpenBraceToken), parseSwitchExpressionArms(), EatToken(SyntaxKind.CloseBraceToken));
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<SwitchExpressionArmSyntax> parseSwitchExpressionArms()
		{
			SeparatedSyntaxListBuilder<SwitchExpressionArmSyntax> item = _pool.AllocateSeparated<SwitchExpressionArmSyntax>();
			while (base.CurrentToken.Kind != SyntaxKind.CloseBraceToken)
			{
				SyntaxToken syntaxToken = ((base.CurrentToken.Kind == SyntaxKind.CaseKeyword) ? AddError(EatToken(), ErrorCode.ERR_BadCaseInSwitchArm) : null);
				TerminatorState termState = _termState;
				_termState |= TerminatorState.IsPatternInSwitchExpressionArm;
				PatternSyntax pattern = ParsePattern(Precedence.Coalescing, afterIs: false, inSwitchArmPattern: true);
				_termState = termState;
				SwitchExpressionArmSyntax switchExpressionArmSyntax = _syntaxFactory.SwitchExpressionArm(pattern, ParseWhenClause(Precedence.Coalescing), (base.CurrentToken.Kind == SyntaxKind.ColonToken) ? EatTokenAsKind(SyntaxKind.EqualsGreaterThanToken) : EatToken(SyntaxKind.EqualsGreaterThanToken), ParseExpressionCore());
				if (syntaxToken == null && switchExpressionArmSyntax.FullWidth == 0 && base.CurrentToken.Kind != SyntaxKind.CommaToken)
				{
					break;
				}
				if (syntaxToken != null)
				{
					switchExpressionArmSyntax = AddLeadingSkippedSyntax(switchExpressionArmSyntax, syntaxToken);
				}
				item.Add(switchExpressionArmSyntax);
				if (base.CurrentToken.Kind != SyntaxKind.CloseBraceToken)
				{
					SyntaxToken separatorToken = ((base.CurrentToken.Kind == SyntaxKind.SemicolonToken) ? EatTokenAsKind(SyntaxKind.CommaToken) : EatToken(SyntaxKind.CommaToken));
					item.AddSeparator(separatorToken);
				}
			}
			return _pool.ToListAndFree(in item);
		}
	}

	private ListPatternSyntax ParseListPattern(bool inSwitchArmPattern)
	{
		SyntaxToken openToken = EatToken(SyntaxKind.OpenBracketToken);
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<PatternSyntax> patterns = ParseCommaSeparatedSyntaxList(ref openToken, SyntaxKind.CloseBracketToken, (LanguageParser @this) => @this.IsPossibleSubpatternElement(), (LanguageParser @this) => @this.ParsePattern(Precedence.Conditional), SkipBadPatternListTokens, allowTrailingSeparator: true, requireOneElement: false, allowSemicolonAsSeparator: false);
		return _syntaxFactory.ListPattern(openToken, patterns, EatToken(SyntaxKind.CloseBracketToken), TryParseSimpleDesignation(inSwitchArmPattern));
	}
}
