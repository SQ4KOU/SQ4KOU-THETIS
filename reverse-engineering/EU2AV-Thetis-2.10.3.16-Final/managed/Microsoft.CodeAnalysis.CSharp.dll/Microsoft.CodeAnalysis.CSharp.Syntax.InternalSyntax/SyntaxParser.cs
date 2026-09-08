using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class SyntaxParser : IDisposable
{
	protected readonly struct ResetPoint
	{
		internal readonly int ResetCount;

		internal readonly LexerMode Mode;

		internal readonly int Position;

		internal readonly GreenNode PrevTokenTrailingTrivia;

		internal ResetPoint(int resetCount, LexerMode mode, int position, GreenNode prevTokenTrailingTrivia)
		{
			ResetCount = resetCount;
			Mode = mode;
			Position = position;
			PrevTokenTrailingTrivia = prevTokenTrailingTrivia;
		}
	}

	protected readonly Lexer lexer;

	private readonly bool _isIncremental;

	private readonly bool _allowModeReset;

	protected readonly CancellationToken cancellationToken;

	private LexerMode _mode;

	private Blender _firstBlender;

	private BlendedNode _currentNode;

	private SyntaxToken _currentToken;

	private ArrayElement<SyntaxToken>[] _lexedTokens;

	private GreenNode _prevTokenTrailingTrivia;

	private int _firstToken;

	private int _tokenOffset;

	private int _tokenCount;

	private int _resetCount;

	private int _resetStart;

	private static readonly ObjectPool<BlendedNode[]> s_blendedNodesPool = new ObjectPool<BlendedNode[]>(() => new BlendedNode[32]);

	private static readonly ObjectPool<ArrayElement<SyntaxToken>[]> s_lexedTokensPool = new ObjectPool<ArrayElement<SyntaxToken>[]>(() => new ArrayElement<SyntaxToken>[4096]);

	private const int CachedTokenArraySize = 4096;

	private int _maxWrittenLexedTokenIndex = -1;

	private BlendedNode[] _blendedTokens;

	protected bool IsIncremental => _isIncremental;

	public CSharpParseOptions Options => lexer.Options;

	public bool IsScript => Options.Kind == SourceCodeKind.Script;

	protected LexerMode Mode
	{
		get
		{
			return _mode;
		}
		set
		{
			if (_mode != value)
			{
				_mode = value;
				_currentToken = null;
				_currentNode = default(BlendedNode);
				_tokenCount = _tokenOffset;
			}
		}
	}

	protected Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode CurrentNode
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode node = _currentNode.Node;
			if (node != null)
			{
				return node;
			}
			ReadCurrentNode();
			return _currentNode.Node;
		}
	}

	protected SyntaxKind CurrentNodeKind => CurrentNode?.Kind() ?? SyntaxKind.None;

	protected SyntaxToken CurrentToken => _currentToken ?? (_currentToken = FetchCurrentToken());

	internal DirectiveStack Directives => lexer.Directives;

	private int CurrentTokenPosition => _firstToken + _tokenOffset;

	protected SyntaxParser(Lexer lexer, LexerMode mode, Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode oldTree, IEnumerable<TextChangeRange> changes, bool allowModeReset, bool preLexIfNotIncremental = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		this.lexer = lexer;
		_mode = mode;
		_allowModeReset = allowModeReset;
		this.cancellationToken = cancellationToken;
		_currentNode = default(BlendedNode);
		_isIncremental = oldTree != null;
		if (IsIncremental | allowModeReset)
		{
			_firstBlender = new Blender(lexer, oldTree, changes);
			_blendedTokens = s_blendedNodesPool.Allocate();
		}
		else
		{
			_firstBlender = default(Blender);
			_lexedTokens = s_lexedTokensPool.Allocate();
		}
		if (preLexIfNotIncremental && !IsIncremental && !cancellationToken.CanBeCanceled)
		{
			PreLex();
		}
	}

	public void Dispose()
	{
		BlendedNode[] blendedTokens = _blendedTokens;
		if (blendedTokens != null)
		{
			_blendedTokens = null;
			if (blendedTokens.Length < 4096)
			{
				Array.Clear(blendedTokens, 0, blendedTokens.Length);
				s_blendedNodesPool.Free(blendedTokens);
			}
		}
		ArrayElement<SyntaxToken>[] lexedTokens = _lexedTokens;
		if (lexedTokens != null)
		{
			_lexedTokens = null;
			ReturnLexedTokensToPool(lexedTokens);
		}
	}

	protected void ReInitialize()
	{
		_firstToken = 0;
		_tokenOffset = 0;
		_tokenCount = 0;
		_resetCount = 0;
		_resetStart = 0;
		_currentToken = null;
		_prevTokenTrailingTrivia = null;
		if (IsIncremental || _allowModeReset)
		{
			_firstBlender = new Blender(lexer, null, null);
		}
	}

	private void PreLex()
	{
		int num = Math.Min(4096, this.lexer.TextWindow.Text.Length / 2);
		Lexer lexer = this.lexer;
		LexerMode mode = _mode;
		if (_lexedTokens == null)
		{
			_lexedTokens = s_lexedTokensPool.Allocate();
		}
		for (int i = 0; i < num; i++)
		{
			SyntaxToken syntaxToken = lexer.Lex(mode);
			AddLexedToken(syntaxToken);
			if (syntaxToken.Kind == SyntaxKind.EndOfFileToken)
			{
				break;
			}
		}
	}

	protected ResetPoint GetResetPoint()
	{
		int currentTokenPosition = CurrentTokenPosition;
		if (_resetCount == 0)
		{
			_resetStart = currentTokenPosition;
		}
		_resetCount++;
		return new ResetPoint(_resetCount, _mode, currentTokenPosition, _prevTokenTrailingTrivia);
	}

	protected void Reset(ref ResetPoint point)
	{
		int num = point.Position - _firstToken;
		if (num >= _tokenCount)
		{
			PeekToken(num - _tokenOffset);
			num = point.Position - _firstToken;
		}
		_mode = point.Mode;
		_tokenOffset = num;
		_currentToken = null;
		_currentNode = default(BlendedNode);
		_prevTokenTrailingTrivia = point.PrevTokenTrailingTrivia;
		if (_blendedTokens == null)
		{
			return;
		}
		for (int i = _tokenOffset; i < _tokenCount; i++)
		{
			if (_blendedTokens[i].Token == null)
			{
				_tokenCount = i;
				if (_tokenCount == _tokenOffset)
				{
					FetchCurrentToken();
				}
				break;
			}
		}
	}

	protected void Release(ref ResetPoint point)
	{
		_resetCount--;
		if (_resetCount == 0)
		{
			_resetStart = -1;
		}
	}

	private void ReadCurrentNode()
	{
		if (_tokenOffset == 0)
		{
			_currentNode = _firstBlender.ReadNode(_mode);
		}
		else
		{
			_currentNode = _blendedTokens[_tokenOffset - 1].Blender.ReadNode(_mode);
		}
	}

	protected GreenNode EatNode()
	{
		GreenNode green = CurrentNode.Green;
		if (_tokenOffset >= _blendedTokens.Length)
		{
			AddTokenSlot();
		}
		_blendedTokens[_tokenOffset++] = _currentNode;
		_tokenCount = _tokenOffset;
		_currentNode = default(BlendedNode);
		_currentToken = null;
		return green;
	}

	private SyntaxToken FetchCurrentToken()
	{
		if (_tokenOffset >= _tokenCount)
		{
			AddNewToken();
		}
		if (_blendedTokens != null)
		{
			return _blendedTokens[_tokenOffset].Token;
		}
		return _lexedTokens[_tokenOffset];
	}

	private void AddNewToken()
	{
		if (_blendedTokens != null)
		{
			if (_tokenCount > 0)
			{
				AddToken(_blendedTokens[_tokenCount - 1].Blender.ReadToken(_mode));
			}
			else if (_currentNode.Token != null)
			{
				AddToken(in _currentNode);
			}
			else
			{
				AddToken(_firstBlender.ReadToken(_mode));
			}
		}
		else
		{
			AddLexedToken(lexer.Lex(_mode));
		}
	}

	private void AddToken(in BlendedNode tokenResult)
	{
		if (_tokenCount >= _blendedTokens.Length)
		{
			AddTokenSlot();
		}
		_blendedTokens[_tokenCount] = tokenResult;
		_tokenCount++;
	}

	private void AddLexedToken(SyntaxToken token)
	{
		if (_tokenCount >= _lexedTokens.Length)
		{
			AddLexedTokenSlot();
		}
		if (_tokenCount > _maxWrittenLexedTokenIndex)
		{
			_maxWrittenLexedTokenIndex = _tokenCount;
		}
		_lexedTokens[_tokenCount].Value = token;
		_tokenCount++;
	}

	private void AddTokenSlot()
	{
		if (_tokenOffset > _blendedTokens.Length >> 1 && (_resetStart == -1 || _resetStart > _firstToken))
		{
			int num = ((_resetStart == -1) ? _tokenOffset : (_resetStart - _firstToken));
			int num2 = _tokenCount - num;
			_firstBlender = _blendedTokens[num - 1].Blender;
			if (num2 > 0)
			{
				Array.Copy(_blendedTokens, num, _blendedTokens, 0, num2);
			}
			_firstToken += num;
			_tokenCount -= num;
			_tokenOffset -= num;
		}
		else
		{
			_ = _blendedTokens;
			Array.Resize(ref _blendedTokens, _blendedTokens.Length * 2);
		}
	}

	private void AddLexedTokenSlot()
	{
		if (_tokenOffset > _lexedTokens.Length >> 1 && (_resetStart == -1 || _resetStart > _firstToken))
		{
			int num = ((_resetStart == -1) ? _tokenOffset : (_resetStart - _firstToken));
			int num2 = _tokenCount - num;
			if (num2 > 0)
			{
				Array.Copy(_lexedTokens, num, _lexedTokens, 0, num2);
			}
			_firstToken += num;
			_tokenCount -= num;
			_tokenOffset -= num;
		}
		else
		{
			ArrayElement<SyntaxToken>[] lexedTokens = _lexedTokens;
			Array.Resize(ref _lexedTokens, _lexedTokens.Length * 2);
			ReturnLexedTokensToPool(lexedTokens);
		}
	}

	private void ReturnLexedTokensToPool(ArrayElement<SyntaxToken>[] lexedTokens)
	{
		if (lexedTokens.Length == 4096)
		{
			Array.Clear(lexedTokens, 0, _maxWrittenLexedTokenIndex + 1);
			s_lexedTokensPool.Free(lexedTokens);
		}
	}

	protected SyntaxToken PeekToken(int n)
	{
		while (_tokenOffset + n >= _tokenCount)
		{
			AddNewToken();
		}
		if (_blendedTokens != null)
		{
			return _blendedTokens[_tokenOffset + n].Token;
		}
		return _lexedTokens[_tokenOffset + n];
	}

	protected SyntaxToken EatToken()
	{
		SyntaxToken currentToken = CurrentToken;
		MoveToNextToken();
		return currentToken;
	}

	protected SyntaxToken TryEatToken(SyntaxKind kind)
	{
		if (CurrentToken.Kind != kind)
		{
			return null;
		}
		return EatToken();
	}

	private void MoveToNextToken()
	{
		_prevTokenTrailingTrivia = _currentToken.GetTrailingTrivia();
		_currentToken = null;
		if (_blendedTokens != null)
		{
			_currentNode = default(BlendedNode);
		}
		_tokenOffset++;
	}

	protected void ForceEndOfFile()
	{
		_currentToken = SyntaxFactory.Token(SyntaxKind.EndOfFileToken);
	}

	protected SyntaxToken EatToken(SyntaxKind kind)
	{
		SyntaxToken currentToken = CurrentToken;
		if (currentToken.Kind == kind)
		{
			MoveToNextToken();
			return currentToken;
		}
		return CreateMissingToken(kind, CurrentToken.Kind);
	}

	protected SyntaxToken EatTokenAsKind(SyntaxKind expected)
	{
		SyntaxToken currentToken = CurrentToken;
		if (currentToken.Kind == expected)
		{
			MoveToNextToken();
			return currentToken;
		}
		SyntaxToken node = CreateMissingToken(expected, CurrentToken.Kind);
		return AddTrailingSkippedSyntax(node, EatToken());
	}

	protected SyntaxToken CreateMissingToken(SyntaxKind expected, SyntaxKind actual)
	{
		SyntaxToken syntaxToken = SyntaxFactory.MissingToken(expected);
		return WithAdditionalDiagnostics(syntaxToken, GetExpectedMissingNodeOrTokenError(syntaxToken, expected, actual));
	}

	private SyntaxToken CreateMissingToken(SyntaxKind expected, ErrorCode code, bool reportError)
	{
		SyntaxToken syntaxToken = SyntaxFactory.MissingToken(expected);
		if (reportError)
		{
			syntaxToken = AddError(syntaxToken, code);
		}
		return syntaxToken;
	}

	protected SyntaxToken EatToken(SyntaxKind kind, bool reportError)
	{
		if (reportError)
		{
			return EatToken(kind);
		}
		if (CurrentToken.Kind != kind)
		{
			return SyntaxFactory.MissingToken(kind);
		}
		return EatToken();
	}

	protected SyntaxToken EatToken(SyntaxKind kind, ErrorCode code, bool reportError = true)
	{
		if (CurrentToken.Kind != kind)
		{
			return CreateMissingToken(kind, code, reportError);
		}
		return EatToken();
	}

	protected SyntaxToken EatTokenEvenWithIncorrectKind(SyntaxKind kind)
	{
		SyntaxToken token = CurrentToken;
		if (token.Kind != kind)
		{
			(int offset, int width) tuple = getDiagnosticSpan();
			int item = tuple.offset;
			int item2 = tuple.width;
			token = WithAdditionalDiagnostics(token, GetExpectedTokenError(kind, token.Kind, item, item2));
		}
		MoveToNextToken();
		return token;
		(int offset, int width) getDiagnosticSpan()
		{
			GreenNode prevTokenTrailingTrivia = _prevTokenTrailingTrivia;
			if (new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(prevTokenTrailingTrivia).Any(8539))
			{
				return (offset: -(prevTokenTrailingTrivia.FullWidth + token.GetLeadingTriviaWidth()), width: 0);
			}
			return (offset: 0, width: token.Width);
		}
	}

	protected SyntaxToken EatTokenWithPrejudice(ErrorCode errorCode, params object[] args)
	{
		SyntaxToken syntaxToken = EatToken();
		return WithAdditionalDiagnostics(syntaxToken, MakeError(0, syntaxToken.Width, errorCode, args));
	}

	protected SyntaxToken EatContextualToken(SyntaxKind kind, ErrorCode code)
	{
		if (CurrentToken.ContextualKind != kind)
		{
			return CreateMissingToken(kind, code, reportError: true);
		}
		return ConvertToKeyword(EatToken());
	}

	protected SyntaxToken EatContextualToken(SyntaxKind kind)
	{
		SyntaxKind contextualKind = CurrentToken.ContextualKind;
		if (contextualKind != kind)
		{
			return CreateMissingToken(kind, contextualKind);
		}
		return ConvertToKeyword(EatToken());
	}

	protected virtual SyntaxDiagnosticInfo GetExpectedTokenError(SyntaxKind expected, SyntaxKind actual, int offset, int width)
	{
		ErrorCode expectedTokenErrorCode = GetExpectedTokenErrorCode(expected, actual);
		return expectedTokenErrorCode switch
		{
			ErrorCode.ERR_SyntaxError => new SyntaxDiagnosticInfo(offset, width, expectedTokenErrorCode, SyntaxFacts.GetText(expected)), 
			ErrorCode.ERR_IdentifierExpectedKW => new SyntaxDiagnosticInfo(offset, width, expectedTokenErrorCode, string.Empty, SyntaxFacts.GetText(actual)), 
			_ => new SyntaxDiagnosticInfo(offset, width, expectedTokenErrorCode), 
		};
	}

	protected virtual SyntaxDiagnosticInfo GetExpectedMissingNodeOrTokenError(GreenNode missingNodeOrToken, SyntaxKind expected, SyntaxKind actual)
	{
		var (offset, width) = GetDiagnosticSpanForMissingNodeOrToken(missingNodeOrToken);
		return GetExpectedTokenError(expected, actual, offset, width);
	}

	private static ErrorCode GetExpectedTokenErrorCode(SyntaxKind expected, SyntaxKind actual)
	{
		switch (expected)
		{
		case SyntaxKind.IdentifierToken:
			if (SyntaxFacts.IsReservedKeyword(actual))
			{
				return ErrorCode.ERR_IdentifierExpectedKW;
			}
			return ErrorCode.ERR_IdentifierExpected;
		case SyntaxKind.SemicolonToken:
			return ErrorCode.ERR_SemicolonExpected;
		case SyntaxKind.CloseParenToken:
			return ErrorCode.ERR_CloseParenExpected;
		case SyntaxKind.OpenBraceToken:
			return ErrorCode.ERR_LbraceExpected;
		case SyntaxKind.CloseBraceToken:
			return ErrorCode.ERR_RbraceExpected;
		default:
			return ErrorCode.ERR_SyntaxError;
		}
	}

	protected virtual TNode WithAdditionalDiagnostics<TNode>(TNode node, params DiagnosticInfo[] diagnostics) where TNode : GreenNode
	{
		DiagnosticInfo[] diagnostics2 = node.GetDiagnostics();
		int num = diagnostics2.Length;
		if (num == 0)
		{
			return node.WithDiagnosticsGreen(diagnostics);
		}
		DiagnosticInfo[] array = new DiagnosticInfo[diagnostics2.Length + diagnostics.Length];
		diagnostics2.CopyTo(array, 0);
		diagnostics.CopyTo(array, num);
		return node.WithDiagnosticsGreen(array);
	}

	protected TNode AddError<TNode>(TNode node, ErrorCode code) where TNode : GreenNode
	{
		return AddError(node, code, Array.Empty<object>());
	}

	protected TNode AddErrorAsWarning<TNode>(TNode node, ErrorCode code, params object[] args) where TNode : GreenNode
	{
		return AddError(node, ErrorCode.WRN_ErrorOverride, MakeError(node, code, args), (int)code);
	}

	protected TNode AddError<TNode>(TNode nodeOrToken, ErrorCode code, params object[] args) where TNode : GreenNode
	{
		if (!nodeOrToken.IsMissing)
		{
			return WithAdditionalDiagnostics(nodeOrToken, MakeError(nodeOrToken, code, args));
		}
		var (offset, width) = GetDiagnosticSpanForMissingNodeOrToken(nodeOrToken);
		return WithAdditionalDiagnostics(nodeOrToken, MakeError(offset, width, code, args));
	}

	protected (int offset, int width) GetDiagnosticSpanForMissingNodeOrToken(GreenNode missingNodeOrToken)
	{
		if (!missingNodeOrToken.ContainsSkippedText)
		{
			return getOffsetAndWidthBasedOnPriorAndNextTokens();
		}
		return getOffsetAndWidthOfSkippedToken();
		(int offset, int width) getOffsetAndWidthBasedOnPriorAndNextTokens()
		{
			GreenNode prevTokenTrailingTrivia = _prevTokenTrailingTrivia;
			if (new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(prevTokenTrailingTrivia).Any(8539))
			{
				return (offset: -missingNodeOrToken.GetLeadingTriviaWidth() - prevTokenTrailingTrivia.FullWidth, width: 0);
			}
			SyntaxToken currentToken = CurrentToken;
			return (offset: missingNodeOrToken.Width + missingNodeOrToken.GetTrailingTriviaWidth() + currentToken.GetLeadingTriviaWidth(), width: currentToken.Width);
		}
		(int offset, int width) getOffsetAndWidthOfSkippedToken()
		{
			int num = 0;
			foreach (GreenNode item in missingNodeOrToken.EnumerateNodes())
			{
				if (item.IsToken)
				{
					SyntaxToken syntaxToken = (SyntaxToken)item;
					if (item.ContainsSkippedText)
					{
						foreach (GreenNode item2 in new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<GreenNode>(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.Concat(syntaxToken.GetLeadingTrivia(), syntaxToken.GetTrailingTrivia())))
						{
							if (item2.IsSkippedTokensTrivia)
							{
								return (offset: num, width: item2.Width);
							}
							num += item2.FullWidth;
						}
						return default((int, int));
					}
					num += item.FullWidth;
				}
			}
			return default((int, int));
		}
	}

	protected TNode AddError<TNode>(TNode node, int offset, int length, ErrorCode code, params object[] args) where TNode : CSharpSyntaxNode
	{
		return WithAdditionalDiagnostics(node, MakeError(offset, length, code, args));
	}

	protected TNode AddErrorToFirstToken<TNode>(TNode node, ErrorCode code) where TNode : CSharpSyntaxNode
	{
		SyntaxToken firstToken = node.GetFirstToken();
		return WithAdditionalDiagnostics(node, MakeError(0, firstToken.Width, code));
	}

	protected TNode AddErrorToFirstToken<TNode>(TNode node, ErrorCode code, params object[] args) where TNode : CSharpSyntaxNode
	{
		SyntaxToken firstToken = node.GetFirstToken();
		return WithAdditionalDiagnostics(node, MakeError(0, firstToken.Width, code, args));
	}

	protected TNode AddErrorToLastToken<TNode>(TNode node, ErrorCode code) where TNode : CSharpSyntaxNode
	{
		GetOffsetAndWidthForLastToken(node, out var offset, out var width);
		return WithAdditionalDiagnostics(node, MakeError(offset, width, code));
	}

	private static void GetOffsetAndWidthForLastToken<TNode>(TNode node, out int offset, out int width) where TNode : CSharpSyntaxNode
	{
		SyntaxToken lastNonmissingToken = node.GetLastNonmissingToken();
		offset = node.Width + node.GetTrailingTriviaWidth();
		width = 0;
		if (lastNonmissingToken != null)
		{
			offset -= lastNonmissingToken.FullWidth;
			offset += lastNonmissingToken.GetLeadingTriviaWidth();
			width = lastNonmissingToken.Width;
		}
	}

	protected static SyntaxDiagnosticInfo MakeError(int offset, int width, ErrorCode code)
	{
		return new SyntaxDiagnosticInfo(offset, width, code);
	}

	protected static SyntaxDiagnosticInfo MakeError(int offset, int width, ErrorCode code, params object[] args)
	{
		return new SyntaxDiagnosticInfo(offset, width, code, args);
	}

	protected static SyntaxDiagnosticInfo MakeError(GreenNode node, ErrorCode code, params object[] args)
	{
		return new SyntaxDiagnosticInfo(0, node.Width, code, args);
	}

	protected static SyntaxDiagnosticInfo MakeError(ErrorCode code, params object[] args)
	{
		return new SyntaxDiagnosticInfo(code, args);
	}

	protected TNode AddLeadingSkippedSyntax<TNode>(TNode node, GreenNode? skippedSyntax) where TNode : CSharpSyntaxNode
	{
		if (skippedSyntax == null)
		{
			return node;
		}
		SyntaxToken syntaxToken = (node as SyntaxToken) ?? node.GetFirstToken();
		SyntaxToken newToken = AddSkippedSyntax(syntaxToken, skippedSyntax, trailing: false);
		return SyntaxFirstTokenReplacer.Replace(node, syntaxToken, newToken, skippedSyntax.FullWidth);
	}

	protected void AddTrailingSkippedSyntax(SyntaxListBuilder list, GreenNode skippedSyntax)
	{
		list[list.Count - 1] = AddTrailingSkippedSyntax((CSharpSyntaxNode)list[list.Count - 1], skippedSyntax);
	}

	protected void AddTrailingSkippedSyntax<TNode>(SyntaxListBuilder<TNode> list, GreenNode skippedSyntax) where TNode : CSharpSyntaxNode
	{
		list[list.Count - 1] = AddTrailingSkippedSyntax(list[list.Count - 1], skippedSyntax);
	}

	protected TNode AddTrailingSkippedSyntax<TNode>(TNode node, GreenNode skippedSyntax) where TNode : CSharpSyntaxNode
	{
		if (node is SyntaxToken target)
		{
			return (TNode)(CSharpSyntaxNode)AddSkippedSyntax(target, skippedSyntax, trailing: true);
		}
		SyntaxToken lastToken = node.GetLastToken();
		SyntaxToken newToken = AddSkippedSyntax(lastToken, skippedSyntax, trailing: true);
		return SyntaxLastTokenReplacer.Replace(node, newToken);
	}

	internal SyntaxToken AddSkippedSyntax(SyntaxToken target, GreenNode skippedSyntax, bool trailing)
	{
		SyntaxListBuilder syntaxListBuilder = new SyntaxListBuilder(4);
		int num;
		if (trailing)
		{
			num = target.Width + target.GetTrailingTriviaWidth();
			syntaxListBuilder.Add(target.GetTrailingTrivia());
		}
		else
		{
			num = -target.GetLeadingTriviaWidth() - skippedSyntax.FullWidth;
		}
		SyntaxDiagnosticInfo syntaxDiagnosticInfo = null;
		int offset = 0;
		foreach (GreenNode item in skippedSyntax.EnumerateNodes())
		{
			if (item is SyntaxToken syntaxToken)
			{
				syntaxListBuilder.Add(syntaxToken.GetLeadingTrivia());
				if (syntaxToken.Width > 0)
				{
					syntaxListBuilder.Add(SyntaxFactory.SkippedTokensTrivia(syntaxToken.TokenWithLeadingTrivia(null).TokenWithTrailingTrivia(null)));
				}
				else
				{
					SyntaxDiagnosticInfo syntaxDiagnosticInfo2 = (SyntaxDiagnosticInfo)syntaxToken.GetDiagnostics().FirstOrDefault();
					if (syntaxDiagnosticInfo2 != null)
					{
						syntaxDiagnosticInfo = syntaxDiagnosticInfo2;
						offset = num + syntaxToken.GetLeadingTriviaWidth() + syntaxDiagnosticInfo2.Offset;
					}
				}
				syntaxListBuilder.Add(syntaxToken.GetTrailingTrivia());
				num += syntaxToken.FullWidth;
			}
			else if (item.ContainsDiagnostics && syntaxDiagnosticInfo == null)
			{
				SyntaxDiagnosticInfo syntaxDiagnosticInfo3 = (SyntaxDiagnosticInfo)item.GetDiagnostics().FirstOrDefault();
				if (syntaxDiagnosticInfo3 != null)
				{
					syntaxDiagnosticInfo = syntaxDiagnosticInfo3;
					offset = num + item.GetLeadingTriviaWidth() + syntaxDiagnosticInfo3.Offset;
				}
			}
		}
		if (syntaxDiagnosticInfo != null)
		{
			target = WithAdditionalDiagnostics(target, new SyntaxDiagnosticInfo(offset, syntaxDiagnosticInfo.Width, (ErrorCode)syntaxDiagnosticInfo.Code, syntaxDiagnosticInfo.Arguments));
		}
		if (!trailing)
		{
			return target.TokenWithLeadingTrivia(syntaxListBuilder.AddRange(target.GetLeadingTrivia()).ToListNode());
		}
		return target.TokenWithTrailingTrivia(syntaxListBuilder.ToListNode());
	}

	protected static SyntaxToken ConvertToKeyword(SyntaxToken token)
	{
		if (token.Kind != token.ContextualKind)
		{
			SyntaxToken syntaxToken = (token.IsMissing ? SyntaxFactory.MissingToken(token.LeadingTrivia.Node, token.ContextualKind, token.TrailingTrivia.Node) : SyntaxFactory.Token(token.LeadingTrivia.Node, token.ContextualKind, token.TrailingTrivia.Node));
			DiagnosticInfo[] diagnostics = token.GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				syntaxToken = syntaxToken.WithDiagnosticsGreen(diagnostics);
			}
			return syntaxToken;
		}
		return token;
	}

	protected static SyntaxToken ConvertToIdentifier(SyntaxToken token)
	{
		SyntaxToken syntaxToken = SyntaxToken.Identifier(token.Kind, token.LeadingTrivia.Node, token.Text, token.ValueText, token.TrailingTrivia.Node);
		if (token.ContainsDiagnostics)
		{
			syntaxToken = syntaxToken.WithDiagnosticsGreen(token.GetDiagnostics());
		}
		return syntaxToken;
	}

	protected TNode CheckFeatureAvailability<TNode>(TNode node, MessageID feature, bool forceWarning = false) where TNode : GreenNode
	{
		CSDiagnosticInfo featureAvailabilityDiagnosticInfo = feature.GetFeatureAvailabilityDiagnosticInfo(Options);
		if (featureAvailabilityDiagnosticInfo != null)
		{
			if (forceWarning)
			{
				return AddError(node, ErrorCode.WRN_ErrorOverride, featureAvailabilityDiagnosticInfo, (int)featureAvailabilityDiagnosticInfo.Code);
			}
			return AddError(node, featureAvailabilityDiagnosticInfo.Code, featureAvailabilityDiagnosticInfo.Arguments);
		}
		return node;
	}

	protected bool IsFeatureEnabled(MessageID feature)
	{
		return Options.IsFeatureEnabled(feature);
	}

	protected bool IsMakingProgress(ref int lastTokenPosition, bool assertIfFalse = true)
	{
		int currentTokenPosition = CurrentTokenPosition;
		if (currentTokenPosition > lastTokenPosition)
		{
			lastTokenPosition = currentTokenPosition;
			return true;
		}
		return false;
	}
}
