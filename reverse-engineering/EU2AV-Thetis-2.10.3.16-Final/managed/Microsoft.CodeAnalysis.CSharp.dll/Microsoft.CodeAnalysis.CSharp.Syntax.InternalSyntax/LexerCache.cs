using System;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal class LexerCache
{
	private static readonly ObjectPool<LexerCache> s_lexerCachePool = new ObjectPool<LexerCache>(() => new LexerCache());

	private static readonly ObjectPool<CachingIdentityFactory<string, SyntaxKind>> s_keywordKindPool = CachingIdentityFactory<string, SyntaxKind>.CreatePool(512, delegate(string key)
	{
		SyntaxKind syntaxKind = SyntaxFacts.GetKeywordKind(key);
		if (syntaxKind == SyntaxKind.None)
		{
			syntaxKind = SyntaxFacts.GetContextualKeywordKind(key);
		}
		return syntaxKind;
	});

	private TextKeyedCache<SyntaxTrivia>? _triviaMap;

	private TextKeyedCache<SyntaxToken>? _tokenMap;

	private CachingIdentityFactory<string, SyntaxKind>? _keywordKindMap;

	internal const int MaxKeywordLength = 10;

	private PooledStringBuilder? _stringBuilder;

	private readonly char[] _identBuffer;

	private SyntaxListBuilder? _leadingTriviaCache;

	private SyntaxListBuilder? _trailingTriviaCache;

	private const int LeadingTriviaCacheInitialCapacity = 128;

	private const int TrailingTriviaCacheInitialCapacity = 16;

	internal char[] IdentBuffer => _identBuffer;

	private TextKeyedCache<SyntaxTrivia> TriviaMap
	{
		get
		{
			if (_triviaMap == null)
			{
				_triviaMap = TextKeyedCache<SyntaxTrivia>.GetInstance();
			}
			return _triviaMap;
		}
	}

	private TextKeyedCache<SyntaxToken> TokenMap
	{
		get
		{
			if (_tokenMap == null)
			{
				_tokenMap = TextKeyedCache<SyntaxToken>.GetInstance();
			}
			return _tokenMap;
		}
	}

	private CachingIdentityFactory<string, SyntaxKind> KeywordKindMap
	{
		get
		{
			if (_keywordKindMap == null)
			{
				_keywordKindMap = s_keywordKindPool.Allocate();
			}
			return _keywordKindMap;
		}
	}

	internal PooledStringBuilder StringBuilder
	{
		get
		{
			if (_stringBuilder == null)
			{
				_stringBuilder = PooledStringBuilder.GetInstance();
			}
			return _stringBuilder;
		}
	}

	internal SyntaxListBuilder LeadingTriviaCache
	{
		get
		{
			if (_leadingTriviaCache == null)
			{
				_leadingTriviaCache = new SyntaxListBuilder(128);
			}
			return _leadingTriviaCache;
		}
	}

	internal SyntaxListBuilder TrailingTriviaCache
	{
		get
		{
			if (_trailingTriviaCache == null)
			{
				_trailingTriviaCache = new SyntaxListBuilder(16);
			}
			return _trailingTriviaCache;
		}
	}

	private LexerCache()
	{
		_identBuffer = new char[32];
	}

	public static LexerCache GetInstance()
	{
		return s_lexerCachePool.Allocate();
	}

	public void Free()
	{
		if (_keywordKindMap != null)
		{
			_keywordKindMap.Free();
			_keywordKindMap = null;
		}
		if (_triviaMap != null)
		{
			_triviaMap.Free();
			_triviaMap = null;
		}
		if (_tokenMap != null)
		{
			_tokenMap.Free();
			_tokenMap = null;
		}
		if (_stringBuilder != null)
		{
			_stringBuilder.Free();
			_stringBuilder = null;
		}
		if (_leadingTriviaCache != null)
		{
			if (_leadingTriviaCache.Capacity > 256)
			{
				_leadingTriviaCache = null;
			}
			else
			{
				_leadingTriviaCache.Clear();
			}
		}
		if (_trailingTriviaCache != null)
		{
			if (_trailingTriviaCache.Capacity > 32)
			{
				_trailingTriviaCache = null;
			}
			else
			{
				_trailingTriviaCache.Clear();
			}
		}
		s_lexerCachePool.Free(this);
	}

	internal bool TryGetKeywordKind(string key, out SyntaxKind kind)
	{
		if (key.Length > 10)
		{
			kind = SyntaxKind.None;
			return false;
		}
		kind = KeywordKindMap.GetOrMakeValue(key);
		return kind != SyntaxKind.None;
	}

	internal SyntaxTrivia LookupWhitespaceTrivia(in SlidingTextWindow textWindow, int lexemeStartPosition, int hashCode)
	{
		TextSpan span = TextSpan.FromBounds(lexemeStartPosition, textWindow.Position);
		if (textWindow.TryGetTextIfWithinWindow(span, out var textSpan))
		{
			SyntaxTrivia syntaxTrivia = TriviaMap.FindItem(textSpan, hashCode);
			if (syntaxTrivia == null)
			{
				syntaxTrivia = SyntaxFactory.Whitespace(textWindow.GetText(lexemeStartPosition, intern: true));
				TriviaMap.AddItem(textSpan, hashCode, syntaxTrivia);
			}
			return syntaxTrivia;
		}
		return SyntaxFactory.Whitespace(textWindow.GetText(lexemeStartPosition, intern: true));
	}

	internal SyntaxToken LookupToken<TArg>(ReadOnlySpan<char> textBuffer, int hashCode, Func<TArg, SyntaxToken> createTokenFunction, TArg data)
	{
		SyntaxToken syntaxToken = TokenMap.FindItem(textBuffer, hashCode);
		if (syntaxToken == null)
		{
			syntaxToken = createTokenFunction(data);
			TokenMap.AddItem(textBuffer, hashCode, syntaxToken);
		}
		return syntaxToken;
	}
}
