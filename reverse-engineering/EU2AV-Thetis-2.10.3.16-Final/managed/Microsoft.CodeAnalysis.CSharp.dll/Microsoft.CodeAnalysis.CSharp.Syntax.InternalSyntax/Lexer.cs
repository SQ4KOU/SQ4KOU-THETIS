using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class Lexer : AbstractLexer
{
	internal struct TokenInfo
	{
		internal SyntaxKind Kind;

		internal SyntaxKind ContextualKind;

		internal string? Text;

		internal SpecialType ValueKind;

		internal bool RequiresTextForXmlEntity;

		internal bool HasIdentifierEscapeSequence;

		internal string? StringValue;

		internal char CharValue;

		internal int IntValue;

		internal uint UintValue;

		internal long LongValue;

		internal ulong UlongValue;

		internal float FloatValue;

		internal double DoubleValue;

		internal decimal DecimalValue;

		internal bool IsVerbatim;
	}

	internal readonly struct Interpolation
	{
		public readonly Range OpenBraceRange;

		public readonly Range ColonRange;

		public readonly Range CloseBraceRange;

		public bool HasColon => ColonRange.Start.Value != ColonRange.End.Value;

		public Interpolation(Range openBraceRange, Range colonRange, Range closeBraceRange)
		{
			OpenBraceRange = openBraceRange;
			ColonRange = colonRange;
			CloseBraceRange = closeBraceRange;
		}
	}

	internal enum InterpolatedStringKind
	{
		Normal,
		Verbatim,
		SingleLineRaw,
		MultiLineRaw
	}

	[NonCopyable]
	private ref struct InterpolatedOrRawStringScanner(Lexer lexer, bool isInterpolatedString)
	{
		private readonly Lexer _lexer = lexer;

		private readonly bool _isInterpolatedString = isInterpolatedString;

		public SyntaxDiagnosticInfo? Error = null;

		private bool IsAtEnd(InterpolatedStringKind kind)
		{
			bool allowNewline = ((kind == InterpolatedStringKind.Verbatim || kind == InterpolatedStringKind.MultiLineRaw) ? true : false);
			return IsAtEnd(allowNewline);
		}

		private bool IsAtEnd(bool allowNewline)
		{
			char c = _lexer.TextWindow.PeekChar();
			if (allowNewline || !SyntaxFacts.IsNewLine(c))
			{
				if (c == '\uffff')
				{
					return _lexer.TextWindow.IsReallyAtEnd();
				}
				return false;
			}
			return true;
		}

		private void TrySetError(SyntaxDiagnosticInfo error)
		{
			if (Error == null)
			{
				Error = error;
			}
		}

		internal void ScanStringLiteralTop(out InterpolatedStringKind kind, out Range openQuoteRange, ArrayBuilder<Interpolation>? interpolations, out Range closeQuoteRange)
		{
			int position = _lexer.TextWindow.Position;
			bool num = ScanOpenQuote(out kind, out var startingDollarSignCount, out var startingQuoteCount);
			openQuoteRange = position.._lexer.TextWindow.Position;
			if (!num)
			{
				closeQuoteRange = _lexer.TextWindow.Position.._lexer.TextWindow.Position;
				return;
			}
			ScanInterpolatedStringLiteralContents(kind, startingDollarSignCount, startingQuoteCount, interpolations);
			ScanInterpolatedStringLiteralEnd(kind, startingQuoteCount, out closeQuoteRange);
		}

		private bool ScanOpenQuote(out InterpolatedStringKind kind, out int startingDollarSignCount, out int startingQuoteCount)
		{
			ref SlidingTextWindow textWindow = ref _lexer.TextWindow;
			int position = textWindow.Position;
			char c = textWindow.PeekChar(0);
			char c2 = textWindow.PeekChar(1);
			char c3 = textWindow.PeekChar(2);
			if (c != '$')
			{
				if (c == '@' && c2 == '$')
				{
					goto IL_004a;
				}
			}
			else if (c2 == '@')
			{
				goto IL_004a;
			}
			goto IL_0055;
			IL_0058:
			bool flag;
			if (flag)
			{
				kind = InterpolatedStringKind.Verbatim;
				startingDollarSignCount = 1;
				startingQuoteCount = 1;
				textWindow.AdvanceChar(3);
				return true;
			}
			char num = textWindow.PeekChar(0);
			c3 = textWindow.PeekChar(1);
			c2 = textWindow.PeekChar(2);
			c = textWindow.PeekChar(3);
			if ((num == '$' && c3 == '"' && (c2 != '"' || c != '"')) ? true : false)
			{
				kind = InterpolatedStringKind.Normal;
				startingDollarSignCount = 1;
				startingQuoteCount = 1;
				textWindow.AdvanceChar(2);
				return true;
			}
			int num2 = _lexer.ConsumeAtSignSequence();
			startingDollarSignCount = _lexer.ConsumeDollarSignSequence();
			int num3 = _lexer.ConsumeAtSignSequence();
			startingQuoteCount = _lexer.ConsumeQuoteSequence();
			int num4 = num2 + num3;
			_ = _isInterpolatedString;
			if (startingQuoteCount == 0)
			{
				TrySetError(_lexer.MakeError(position, textWindow.Position - position, ErrorCode.ERR_StringMustStartWithQuoteCharacter));
				kind = ((num4 == 1 && startingDollarSignCount == 1) ? InterpolatedStringKind.Verbatim : InterpolatedStringKind.SingleLineRaw);
				return false;
			}
			if (num4 > 0)
			{
				TrySetError(_lexer.MakeError(position, textWindow.Position - position, ErrorCode.ERR_IllegalAtSequence));
			}
			if (startingQuoteCount < 3)
			{
				TrySetError(_lexer.MakeError(textWindow.Position - startingQuoteCount, startingQuoteCount, ErrorCode.ERR_NotEnoughQuotesForRawString));
			}
			int position2 = textWindow.Position;
			_lexer.ConsumeWhitespace();
			if (SyntaxFacts.IsNewLine(textWindow.PeekChar()))
			{
				textWindow.AdvancePastNewLine();
				kind = InterpolatedStringKind.MultiLineRaw;
			}
			else
			{
				textWindow.Reset(position2);
				kind = InterpolatedStringKind.SingleLineRaw;
			}
			return true;
			IL_004a:
			if (c3 != '"')
			{
				goto IL_0055;
			}
			flag = true;
			goto IL_0058;
			IL_0055:
			flag = false;
			goto IL_0058;
		}

		private void ScanInterpolatedStringLiteralEnd(InterpolatedStringKind kind, int startingQuoteCount, out Range closeQuoteRange)
		{
			int position = _lexer.TextWindow.Position;
			if ((uint)kind <= 1u)
			{
				ScanNormalOrVerbatimInterpolatedStringLiteralEnd(kind);
			}
			else
			{
				ScanRawInterpolatedStringLiteralEnd(kind, startingQuoteCount);
				if (!_isInterpolatedString)
				{
					_lexer.ScanUtf8Suffix();
				}
			}
			closeQuoteRange = position.._lexer.TextWindow.Position;
		}

		private void ScanNormalOrVerbatimInterpolatedStringLiteralEnd(InterpolatedStringKind kind)
		{
			if (_lexer.TextWindow.PeekChar() != '"')
			{
				TrySetError(_lexer.MakeError(IsAtEnd(allowNewline: true) ? (_lexer.TextWindow.Position - 1) : _lexer.TextWindow.Position, 1, ErrorCode.ERR_UnterminatedStringLit));
			}
			else
			{
				_lexer.TextWindow.AdvanceChar();
			}
		}

		private void ScanRawInterpolatedStringLiteralEnd(InterpolatedStringKind kind, int startingQuoteCount)
		{
			if (kind == InterpolatedStringKind.SingleLineRaw)
			{
				if (_lexer.TextWindow.PeekChar() != '"')
				{
					TrySetError(_lexer.MakeError(_lexer.TextWindow.Position, SyntaxFacts.IsNewLine(_lexer.TextWindow.PeekChar()) ? 1 : 0, ErrorCode.ERR_UnterminatedRawString));
					return;
				}
				int num = _lexer.ConsumeQuoteSequence();
				if (num > startingQuoteCount)
				{
					int num2 = num - startingQuoteCount;
					TrySetError(_lexer.MakeError(_lexer.TextWindow.Position - num2, num2, ErrorCode.ERR_TooManyQuotesForRawString));
				}
			}
			else if (IsAtEnd(kind))
			{
				TrySetError(_lexer.MakeError(_lexer.TextWindow.Position, 0, ErrorCode.ERR_UnterminatedRawString));
			}
			else if (_lexer.TextWindow.PeekChar() == '"')
			{
				int num3 = _lexer.ConsumeQuoteSequence();
				TrySetError(_lexer.MakeError(_lexer.TextWindow.Position - num3, num3, ErrorCode.ERR_RawStringDelimiterOnOwnLine));
			}
			else
			{
				_lexer.TextWindow.AdvancePastNewLine();
				_lexer.ConsumeWhitespace();
				int num4 = _lexer.ConsumeQuoteSequence();
				if (num4 > startingQuoteCount)
				{
					int num5 = num4 - startingQuoteCount;
					TrySetError(_lexer.MakeError(_lexer.TextWindow.Position - num5, num5, ErrorCode.ERR_TooManyQuotesForRawString));
				}
			}
		}

		private void ScanInterpolatedStringLiteralContents(InterpolatedStringKind kind, int startingDollarSignCount, int startingQuoteCount, ArrayBuilder<Interpolation>? interpolations)
		{
			if (CheckForIllegalEmptyMultiLineRawStringLiteral(kind, startingQuoteCount))
			{
				return;
			}
			while (!IsAtEnd(kind) && !IsAtEndOfMultiLineRawLiteral(kind, startingQuoteCount))
			{
				switch (_lexer.TextWindow.PeekChar())
				{
				case '"':
					if (!IsEndDelimiterOtherwiseConsume(kind, startingQuoteCount))
					{
						continue;
					}
					return;
				case '}':
					if (_isInterpolatedString)
					{
						HandleCloseBraceInContent(kind, startingDollarSignCount);
						continue;
					}
					break;
				case '{':
					if (_isInterpolatedString)
					{
						HandleOpenBraceInContent(kind, startingDollarSignCount, interpolations);
						continue;
					}
					break;
				case '\\':
					if (kind == InterpolatedStringKind.Normal)
					{
						int position = _lexer.TextWindow.Position;
						char c = _lexer.ScanEscapeSequence(out var _);
						if ((c == '{' || c == '}') ? true : false)
						{
							TrySetError(_lexer.MakeError(position, _lexer.TextWindow.Position - position, ErrorCode.ERR_EscapedCurly, c));
						}
					}
					else
					{
						_lexer.TextWindow.AdvanceChar();
					}
					continue;
				}
				_lexer.TextWindow.AdvanceChar();
			}
		}

		private bool CheckForIllegalEmptyMultiLineRawStringLiteral(InterpolatedStringKind kind, int startingQuoteCount)
		{
			if (kind == InterpolatedStringKind.MultiLineRaw)
			{
				_lexer.ConsumeWhitespace();
				int position = _lexer.TextWindow.Position;
				int num = _lexer.ConsumeQuoteSequence();
				if (num >= startingQuoteCount)
				{
					TrySetError(_lexer.MakeError(_lexer.TextWindow.Position - num, num, ErrorCode.ERR_RawStringMustContainContent));
					_lexer.TextWindow.Reset(position);
					return true;
				}
			}
			return false;
		}

		private bool IsAtEndOfMultiLineRawLiteral(InterpolatedStringKind kind, int startingQuoteCount)
		{
			if (kind == InterpolatedStringKind.MultiLineRaw)
			{
				int position = _lexer.TextWindow.Position;
				if (SyntaxFacts.IsNewLine(_lexer.TextWindow.PeekChar()))
				{
					_lexer.TextWindow.AdvancePastNewLine();
					_lexer.ConsumeWhitespace();
					int num = _lexer.ConsumeQuoteSequence();
					_lexer.TextWindow.Reset(position);
					if (num >= startingQuoteCount)
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool IsEndDelimiterOtherwiseConsume(InterpolatedStringKind kind, int startingQuoteCount)
		{
			if ((uint)kind <= 1u)
			{
				if (RecoveringFromRunawayLexing())
				{
					return true;
				}
				if (kind == InterpolatedStringKind.Normal)
				{
					return true;
				}
				if (_lexer.TextWindow.PeekChar(1) != '"')
				{
					return true;
				}
				_lexer.TextWindow.AdvanceChar(2);
			}
			else
			{
				int position = _lexer.TextWindow.Position;
				if (_lexer.ConsumeQuoteSequence() >= startingQuoteCount)
				{
					_lexer.TextWindow.Reset(position);
					return true;
				}
			}
			return false;
		}

		private void HandleCloseBraceInContent(InterpolatedStringKind kind, int startingDollarSignCount)
		{
			if ((uint)kind <= 1u)
			{
				int position = _lexer.TextWindow.Position;
				_lexer.TextWindow.AdvanceChar();
				if (_lexer.TextWindow.PeekChar() == '}')
				{
					_lexer.TextWindow.AdvanceChar();
					return;
				}
				TrySetError(_lexer.MakeError(position, 1, ErrorCode.ERR_UnescapedCurly, "}"));
			}
			else
			{
				int num = _lexer.ConsumeCloseBraceSequence();
				if (num >= startingDollarSignCount)
				{
					TrySetError(_lexer.MakeError(_lexer.TextWindow.Position - num, num, ErrorCode.ERR_TooManyCloseBracesForRawString));
				}
			}
		}

		private void HandleOpenBraceInContent(InterpolatedStringKind kind, int startingDollarSignCount, ArrayBuilder<Interpolation>? interpolations)
		{
			if ((uint)kind <= 1u)
			{
				HandleOpenBraceInNormalOrVerbatimContent(kind, interpolations);
			}
			else
			{
				HandleOpenBraceInRawContent(kind, startingDollarSignCount, interpolations);
			}
		}

		private void HandleOpenBraceInNormalOrVerbatimContent(InterpolatedStringKind kind, ArrayBuilder<Interpolation>? interpolations)
		{
			if (_lexer.TextWindow.PeekChar(1) == '{')
			{
				_lexer.TextWindow.AdvanceChar(2);
				return;
			}
			int position = _lexer.TextWindow.Position;
			_lexer.TextWindow.AdvanceChar();
			ScanInterpolatedStringLiteralHoleBalancedText(kind, '}', isHole: true, out var colonRange);
			int position2 = _lexer.TextWindow.Position;
			if (_lexer.TextWindow.PeekChar() == '}')
			{
				_lexer.TextWindow.AdvanceChar();
			}
			else
			{
				TrySetError(_lexer.MakeError(position - 1, 2, ErrorCode.ERR_UnclosedExpressionHole));
			}
			interpolations?.Add(new Interpolation(position..(position + 1), colonRange, position2.._lexer.TextWindow.Position));
		}

		private void HandleOpenBraceInRawContent(InterpolatedStringKind kind, int startingDollarSignCount, ArrayBuilder<Interpolation>? interpolations)
		{
			int position = _lexer.TextWindow.Position;
			int num = _lexer.ConsumeOpenBraceSequence();
			if (num >= startingDollarSignCount)
			{
				int position2 = _lexer.TextWindow.Position;
				if (num >= 2 * startingDollarSignCount)
				{
					TrySetError(_lexer.MakeError(position, num - startingDollarSignCount, ErrorCode.ERR_TooManyOpenBracesForRawString));
				}
				ScanInterpolatedStringLiteralHoleBalancedText(kind, '}', isHole: true, out var colonRange);
				int position3 = _lexer.TextWindow.Position;
				int num2 = _lexer.ConsumeCloseBraceSequence();
				if (num2 == 0)
				{
					TrySetError(_lexer.MakeError(position2 - startingDollarSignCount, startingDollarSignCount, ErrorCode.ERR_UnclosedExpressionHole));
				}
				else if (num2 < startingDollarSignCount)
				{
					TrySetError(_lexer.MakeError(position, num - startingDollarSignCount, ErrorCode.ERR_NotEnoughCloseBracesForRawString));
				}
				else
				{
					_lexer.TextWindow.Reset(position3 + startingDollarSignCount);
				}
				interpolations?.Add(new Interpolation((position2 - startingDollarSignCount)..position2, colonRange, position3.._lexer.TextWindow.Position));
			}
		}

		private void ScanFormatSpecifier(InterpolatedStringKind kind)
		{
			_lexer.TextWindow.AdvanceChar();
			while (true)
			{
				char c = _lexer.TextWindow.PeekChar();
				if (c == '\\' && kind == InterpolatedStringKind.Normal)
				{
					int position = _lexer.TextWindow.Position;
					c = _lexer.ScanEscapeSequence(out var _);
					if ((c == '{' || c == '}') ? true : false)
					{
						TrySetError(_lexer.MakeError(position, 1, ErrorCode.ERR_EscapedCurly, c));
					}
					continue;
				}
				switch (c)
				{
				case '"':
					if (kind == InterpolatedStringKind.Verbatim && _lexer.TextWindow.PeekChar(1) == '"')
					{
						_lexer.TextWindow.AdvanceChar(2);
						break;
					}
					return;
				case '{':
					TrySetError(_lexer.MakeError(_lexer.TextWindow.Position, 1, ErrorCode.ERR_UnexpectedCharacter, c));
					_lexer.TextWindow.AdvanceChar();
					break;
				case '}':
					return;
				default:
					if (IsAtEnd(allowNewline: true))
					{
						return;
					}
					_lexer.TextWindow.AdvanceChar();
					break;
				}
			}
		}

		private void ScanInterpolatedStringLiteralHoleBalancedText(InterpolatedStringKind kind, char endingChar, bool isHole, out Range colonRange)
		{
			colonRange = default(Range);
			while (true)
			{
				char c = _lexer.TextWindow.PeekChar();
				if (IsAtEnd(allowNewline: true))
				{
					break;
				}
				bool isTerminated;
				switch (c)
				{
				case '#':
					TrySetError(_lexer.MakeError(_lexer.TextWindow.Position, 1, ErrorCode.ERR_SyntaxError, endingChar.ToString()));
					_lexer.TextWindow.AdvanceChar();
					continue;
				case '$':
				{
					TokenInfo info2 = default(TokenInfo);
					if (_lexer.TryScanInterpolatedString(ref info2))
					{
						continue;
					}
					break;
				}
				case ':':
					if (isHole)
					{
						colonRange = _lexer.TextWindow.Position..(_lexer.TextWindow.Position + 1);
						ScanFormatSpecifier(kind);
						return;
					}
					break;
				case ')':
				case ']':
				case '}':
					if (c == endingChar)
					{
						return;
					}
					TrySetError(_lexer.MakeError(_lexer.TextWindow.Position, 1, ErrorCode.ERR_SyntaxError, endingChar.ToString()));
					break;
				case '"':
					if (RecoveringFromRunawayLexing())
					{
						return;
					}
					ScanInterpolatedStringLiteralNestedString();
					continue;
				case '\'':
					ScanInterpolatedStringLiteralNestedString();
					continue;
				case '@':
				{
					TokenInfo info = default(TokenInfo);
					if (_lexer.TryScanAtStringToken(ref info))
					{
						continue;
					}
					if (_lexer.TextWindow.PeekChar(1) == '*')
					{
						_lexer.ScanMultiLineComment(out isTerminated, '@');
						continue;
					}
					break;
				}
				case '/':
					switch (_lexer.TextWindow.PeekChar(1))
					{
					case '/':
						_lexer.ScanToEndOfLine();
						break;
					case '*':
						_lexer.ScanMultiLineComment(out isTerminated, '/');
						break;
					default:
						_lexer.TextWindow.AdvanceChar();
						break;
					}
					continue;
				case '{':
					ScanInterpolatedStringLiteralHoleBracketed(kind, '{', '}');
					continue;
				case '(':
					ScanInterpolatedStringLiteralHoleBracketed(kind, '(', ')');
					continue;
				case '[':
					ScanInterpolatedStringLiteralHoleBracketed(kind, '[', ']');
					continue;
				}
				_lexer.TextWindow.AdvanceChar();
			}
		}

		private bool RecoveringFromRunawayLexing()
		{
			return Error != null;
		}

		private void ScanInterpolatedStringLiteralNestedString()
		{
			TokenInfo info = default(TokenInfo);
			_lexer.ScanStringLiteral(ref info, inDirective: false);
		}

		private void ScanInterpolatedStringLiteralHoleBracketed(InterpolatedStringKind kind, char start, char end)
		{
			_lexer.TextWindow.AdvanceChar();
			ScanInterpolatedStringLiteralHoleBalancedText(kind, end, isHole: false, out var _);
			if (_lexer.TextWindow.PeekChar() == end)
			{
				_lexer.TextWindow.AdvanceChar();
			}
		}
	}

	private enum QuickScanState : byte
	{
		Initial,
		FollowingWhite,
		FollowingCR,
		Ident,
		Number,
		Punctuation,
		Dot,
		CompoundPunctStart,
		DoneAfterNext,
		Done,
		Bad
	}

	private enum CharFlags : byte
	{
		White,
		CR,
		LF,
		Letter,
		Digit,
		Punct,
		Dot,
		CompoundPunctStart,
		Slash,
		Complex,
		EndOfFile
	}

	private const int TriviaListInitialCapacity = 8;

	private readonly CSharpParseOptions _options;

	private LexerMode _mode;

	private readonly StringBuilder _builder;

	private char[] _identBuffer;

	private int _identLen;

	private DirectiveStack _directives;

	private readonly LexerCache _cache;

	private readonly bool _allowPreprocessorDirectives;

	private readonly bool _interpolationFollowedByColon;

	private int _badTokenCount;

	private DocumentationCommentParser? _xmlParser;

	private DirectiveParser? _directiveParser;

	private SyntaxListBuilder _leadingTriviaCache;

	private SyntaxListBuilder _trailingTriviaCache;

	private SyntaxListBuilder? _directiveTriviaCache;

	private static readonly int s_conflictMarkerLength = "<<<<<<<".Length;

	internal const int MaxCachedTokenSize = 42;

	private static readonly byte[,] s_stateTransitions = new byte[9, 11]
	{
		{
			0, 0, 0, 3, 4, 5, 6, 7, 10, 10,
			10
		},
		{
			1, 2, 8, 9, 9, 9, 9, 9, 10, 10,
			9
		},
		{
			9, 9, 8, 9, 9, 9, 9, 9, 9, 9,
			9
		},
		{
			1, 2, 8, 3, 3, 9, 9, 9, 10, 10,
			9
		},
		{
			1, 2, 8, 10, 4, 9, 10, 9, 10, 10,
			9
		},
		{
			1, 2, 8, 9, 9, 9, 9, 9, 10, 10,
			9
		},
		{
			1, 2, 8, 9, 10, 9, 10, 9, 10, 10,
			9
		},
		{
			1, 2, 8, 9, 9, 10, 9, 10, 10, 10,
			9
		},
		{
			9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
			9
		}
	};

	public bool SuppressDocumentationCommentParse => (int)_options.DocumentationMode < 1;

	public CSharpParseOptions Options => _options;

	public DirectiveStack Directives => _directives;

	public bool InterpolationFollowedByColon => _interpolationFollowedByColon;

	private bool InDocumentationComment
	{
		get
		{
			switch (ModeOf(_mode))
			{
			case LexerMode.XmlDocComment:
			case LexerMode.XmlElementTag:
			case LexerMode.XmlAttributeTextQuote:
			case LexerMode.XmlAttributeTextDoubleQuote:
			case LexerMode.XmlCrefQuote:
			case LexerMode.XmlCrefDoubleQuote:
			case LexerMode.XmlNameQuote:
			case LexerMode.XmlNameDoubleQuote:
			case LexerMode.XmlCDataSectionText:
			case LexerMode.XmlCommentText:
			case LexerMode.XmlProcessingInstructionText:
			case LexerMode.XmlCharacter:
				return true;
			default:
				return false;
			}
		}
	}

	private bool InXmlCrefOrNameAttributeValue
	{
		get
		{
			switch (_mode & LexerMode.MaskLexMode)
			{
			case LexerMode.XmlCrefQuote:
			case LexerMode.XmlCrefDoubleQuote:
			case LexerMode.XmlNameQuote:
			case LexerMode.XmlNameDoubleQuote:
				return true;
			default:
				return false;
			}
		}
	}

	private bool InXmlNameAttributeValue
	{
		get
		{
			LexerMode lexerMode = _mode & LexerMode.MaskLexMode;
			if (lexerMode == LexerMode.XmlNameQuote || lexerMode == LexerMode.XmlNameDoubleQuote)
			{
				return true;
			}
			return false;
		}
	}

	private static ReadOnlySpan<byte> CharProperties => new byte[384]
	{
		9, 9, 9, 9, 9, 9, 9, 9, 9, 0,
		2, 0, 0, 1, 9, 9, 9, 9, 9, 9,
		9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		9, 9, 0, 7, 9, 9, 9, 7, 7, 9,
		5, 5, 7, 7, 5, 7, 6, 8, 4, 4,
		4, 4, 4, 4, 4, 4, 4, 4, 7, 5,
		7, 7, 7, 7, 9, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 5, 9, 5, 7, 3, 9, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 5, 7, 5, 7, 9, 9, 9,
		9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		3, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		9, 3, 9, 9, 9, 9, 3, 9, 9, 9,
		9, 9, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 9, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 9, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3
	};

	public Lexer(SourceText text, CSharpParseOptions options, bool allowPreprocessorDirectives = true, bool interpolationFollowedByColon = false)
		: base(text)
	{
		_options = options;
		_allowPreprocessorDirectives = allowPreprocessorDirectives;
		_interpolationFollowedByColon = interpolationFollowedByColon;
		_cache = LexerCache.GetInstance();
		_builder = _cache.StringBuilder;
		_identBuffer = _cache.IdentBuffer;
		_leadingTriviaCache = _cache.LeadingTriviaCache;
		_trailingTriviaCache = _cache.TrailingTriviaCache;
	}

	public override void Dispose()
	{
		_cache.Free();
		_xmlParser?.Dispose();
		_directiveParser?.Dispose();
		base.Dispose();
	}

	public void Reset(int position, DirectiveStack directives)
	{
		TextWindow.Reset(position);
		_directives = directives;
	}

	private static LexerMode ModeOf(LexerMode mode)
	{
		return mode & LexerMode.MaskLexMode;
	}

	private bool ModeIs(LexerMode mode)
	{
		return ModeOf(_mode) == mode;
	}

	private static XmlDocCommentLocation LocationOf(LexerMode mode)
	{
		return (XmlDocCommentLocation)((int)(mode & LexerMode.MaskXmlDocCommentLocation) >> 16);
	}

	private bool LocationIs(XmlDocCommentLocation location)
	{
		return LocationOf(_mode) == location;
	}

	private void MutateLocation(XmlDocCommentLocation location)
	{
		_mode &= ~LexerMode.MaskXmlDocCommentLocation;
		_mode |= (LexerMode)((int)location << 16);
	}

	private static XmlDocCommentStyle StyleOf(LexerMode mode)
	{
		return (XmlDocCommentStyle)((int)(mode & LexerMode.MaskXmlDocCommentStyle) >> 20);
	}

	private bool StyleIs(XmlDocCommentStyle style)
	{
		return StyleOf(_mode) == style;
	}

	public SyntaxToken Lex(ref LexerMode mode)
	{
		SyntaxToken result = Lex(mode);
		mode = _mode;
		return result;
	}

	public SyntaxToken Lex(LexerMode mode)
	{
		_mode = mode;
		switch (_mode)
		{
		case LexerMode.Syntax:
		case LexerMode.DebuggerSyntax:
			return QuickScanSyntaxToken() ?? LexSyntaxToken();
		case LexerMode.Directive:
			return LexDirectiveToken();
		default:
			switch (ModeOf(_mode))
			{
			case LexerMode.XmlDocComment:
				return LexXmlToken();
			case LexerMode.XmlElementTag:
				return LexXmlElementTagToken();
			case LexerMode.XmlAttributeTextQuote:
			case LexerMode.XmlAttributeTextDoubleQuote:
				return LexXmlAttributeTextToken();
			case LexerMode.XmlCDataSectionText:
				return LexXmlCDataSectionTextToken();
			case LexerMode.XmlCommentText:
				return LexXmlCommentTextToken();
			case LexerMode.XmlProcessingInstructionText:
				return LexXmlProcessingInstructionTextToken();
			case LexerMode.XmlCrefQuote:
			case LexerMode.XmlCrefDoubleQuote:
				return LexXmlCrefOrNameToken();
			case LexerMode.XmlNameQuote:
			case LexerMode.XmlNameDoubleQuote:
				return LexXmlCrefOrNameToken();
			case LexerMode.XmlCharacter:
				return LexXmlCharacter();
			default:
				throw ExceptionUtilities.UnexpectedValue(ModeOf(_mode));
			}
		}
	}

	private SyntaxToken LexSyntaxToken()
	{
		_leadingTriviaCache.Clear();
		LexSyntaxTrivia(TextWindow.Position > 0, isTrailing: false, ref _leadingTriviaCache);
		SyntaxListBuilder leadingTriviaCache = _leadingTriviaCache;
		TokenInfo info = default(TokenInfo);
		Start();
		ScanSyntaxToken(ref info);
		SyntaxDiagnosticInfo[] errors = GetErrors();
		_trailingTriviaCache.Clear();
		LexSyntaxTrivia(isFollowingToken: true, isTrailing: true, ref _trailingTriviaCache);
		SyntaxListBuilder trailingTriviaCache = _trailingTriviaCache;
		return Create(in info, leadingTriviaCache, trailingTriviaCache, errors);
	}

	internal SyntaxTriviaList LexSyntaxLeadingTrivia()
	{
		_leadingTriviaCache.Clear();
		LexSyntaxTrivia(TextWindow.Position > 0, isTrailing: false, ref _leadingTriviaCache);
		return new SyntaxTriviaList(default(Microsoft.CodeAnalysis.SyntaxToken), _leadingTriviaCache.ToListNode(), 0);
	}

	internal SyntaxTriviaList LexSyntaxTrailingTrivia()
	{
		_trailingTriviaCache.Clear();
		LexSyntaxTrivia(isFollowingToken: true, isTrailing: true, ref _trailingTriviaCache);
		return new SyntaxTriviaList(default(Microsoft.CodeAnalysis.SyntaxToken), _trailingTriviaCache.ToListNode(), 0);
	}

	private SyntaxToken Create(in TokenInfo info, SyntaxListBuilder? leading, SyntaxListBuilder? trailing, SyntaxDiagnosticInfo[]? errors)
	{
		GreenNode leading2 = leading?.ToListNode();
		GreenNode trailing2 = trailing?.ToListNode();
		SyntaxToken syntaxToken;
		if (info.RequiresTextForXmlEntity)
		{
			syntaxToken = SyntaxFactory.Token(leading2, info.Kind, info.Text, info.StringValue, trailing2);
		}
		else
		{
			switch (info.Kind)
			{
			case SyntaxKind.IdentifierToken:
				syntaxToken = SyntaxFactory.Identifier(info.ContextualKind, leading2, info.Text, info.StringValue, trailing2);
				break;
			case SyntaxKind.NumericLiteralToken:
				syntaxToken = info.ValueKind switch
				{
					SpecialType.System_Int32 => SyntaxFactory.Literal(leading2, info.Text, info.IntValue, trailing2), 
					SpecialType.System_UInt32 => SyntaxFactory.Literal(leading2, info.Text, info.UintValue, trailing2), 
					SpecialType.System_Int64 => SyntaxFactory.Literal(leading2, info.Text, info.LongValue, trailing2), 
					SpecialType.System_UInt64 => SyntaxFactory.Literal(leading2, info.Text, info.UlongValue, trailing2), 
					SpecialType.System_Single => SyntaxFactory.Literal(leading2, info.Text, info.FloatValue, trailing2), 
					SpecialType.System_Double => SyntaxFactory.Literal(leading2, info.Text, info.DoubleValue, trailing2), 
					SpecialType.System_Decimal => SyntaxFactory.Literal(leading2, info.Text, info.DecimalValue, trailing2), 
					_ => throw ExceptionUtilities.UnexpectedValue(info.ValueKind), 
				};
				break;
			case SyntaxKind.InterpolatedStringToken:
				syntaxToken = SyntaxFactory.Literal(leading2, info.Text, info.Kind, info.Text, trailing2);
				break;
			case SyntaxKind.StringLiteralToken:
			case SyntaxKind.SingleLineRawStringLiteralToken:
			case SyntaxKind.MultiLineRawStringLiteralToken:
			case SyntaxKind.Utf8StringLiteralToken:
			case SyntaxKind.Utf8SingleLineRawStringLiteralToken:
			case SyntaxKind.Utf8MultiLineRawStringLiteralToken:
				syntaxToken = SyntaxFactory.Literal(leading2, info.Text, info.Kind, info.StringValue, trailing2);
				break;
			case SyntaxKind.CharacterLiteralToken:
				syntaxToken = SyntaxFactory.Literal(leading2, info.Text, info.CharValue, trailing2);
				break;
			case SyntaxKind.XmlTextLiteralNewLineToken:
				syntaxToken = SyntaxFactory.XmlTextNewLine(leading2, info.Text, info.StringValue, trailing2);
				break;
			case SyntaxKind.XmlTextLiteralToken:
				syntaxToken = SyntaxFactory.XmlTextLiteral(leading2, info.Text, info.StringValue, trailing2);
				break;
			case SyntaxKind.XmlEntityLiteralToken:
				syntaxToken = SyntaxFactory.XmlEntity(leading2, info.Text, info.StringValue, trailing2);
				break;
			case SyntaxKind.EndOfDocumentationCommentToken:
			case SyntaxKind.EndOfFileToken:
				syntaxToken = SyntaxFactory.Token(leading2, info.Kind, trailing2);
				break;
			case SyntaxKind.RazorContentToken:
				syntaxToken = SyntaxFactory.Token(leading2, info.Kind, info.Text, trailing2);
				break;
			case SyntaxKind.None:
				syntaxToken = SyntaxFactory.BadToken(leading2, info.Text, trailing2);
				break;
			default:
				syntaxToken = SyntaxFactory.Token(leading2, info.Kind, trailing2);
				break;
			}
		}
		if (errors != null && ((int)_options.DocumentationMode >= 2 || !InDocumentationComment))
		{
			syntaxToken = syntaxToken.WithDiagnosticsGreen(errors);
		}
		return syntaxToken;
	}

	private void ScanSyntaxToken(ref TokenInfo info)
	{
		info.Kind = SyntaxKind.None;
		info.ContextualKind = SyntaxKind.None;
		info.Text = null;
		bool flag = false;
		int position = TextWindow.Position;
		char c = TextWindow.PeekChar();
		char surrogateCharacter;
		switch (c)
		{
		case '"':
		case '\'':
			ScanStringLiteral(ref info, inDirective: false);
			break;
		case '/':
			TextWindow.AdvanceChar();
			info.Kind = (TextWindow.TryAdvance('=') ? SyntaxKind.SlashEqualsToken : SyntaxKind.SlashToken);
			break;
		case '.':
			surrogateCharacter = TextWindow.PeekChar(1);
			if (surrogateCharacter >= '0' && surrogateCharacter <= '9')
			{
				int position2 = TextWindow.Position;
				if (position2 >= 1 && position2 == LexemeStartPosition && TextWindow.PreviousChar() == '.')
				{
					TextWindow.AdvanceChar();
					info.Kind = SyntaxKind.DotToken;
					break;
				}
			}
			if (!ScanNumericLiteral(ref info))
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.DotToken;
			}
			break;
		case ',':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.CommaToken;
			break;
		case ':':
			TextWindow.AdvanceChar();
			info.Kind = (TextWindow.TryAdvance(':') ? SyntaxKind.ColonColonToken : SyntaxKind.ColonToken);
			break;
		case ';':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.SemicolonToken;
			break;
		case '~':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.TildeToken;
			break;
		case '!':
			TextWindow.AdvanceChar();
			info.Kind = (TextWindow.TryAdvance('=') ? SyntaxKind.ExclamationEqualsToken : SyntaxKind.ExclamationToken);
			break;
		case '=':
			TextWindow.AdvanceChar();
			info.Kind = (TextWindow.TryAdvance('=') ? SyntaxKind.EqualsEqualsToken : (TextWindow.TryAdvance('>') ? SyntaxKind.EqualsGreaterThanToken : SyntaxKind.EqualsToken));
			break;
		case '*':
			TextWindow.AdvanceChar();
			info.Kind = (TextWindow.TryAdvance('=') ? SyntaxKind.AsteriskEqualsToken : SyntaxKind.AsteriskToken);
			break;
		case '(':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.OpenParenToken;
			break;
		case ')':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.CloseParenToken;
			break;
		case '{':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.OpenBraceToken;
			break;
		case '}':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.CloseBraceToken;
			break;
		case '[':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.OpenBracketToken;
			break;
		case ']':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.CloseBracketToken;
			break;
		case '?':
			TextWindow.AdvanceChar();
			info.Kind = ((!TextWindow.TryAdvance('?')) ? SyntaxKind.QuestionToken : (TextWindow.TryAdvance('=') ? SyntaxKind.QuestionQuestionEqualsToken : SyntaxKind.QuestionQuestionToken));
			break;
		case '+':
			TextWindow.AdvanceChar();
			info.Kind = (TextWindow.TryAdvance('=') ? SyntaxKind.PlusEqualsToken : (TextWindow.TryAdvance('+') ? SyntaxKind.PlusPlusToken : SyntaxKind.PlusToken));
			break;
		case '-':
			TextWindow.AdvanceChar();
			info.Kind = (TextWindow.TryAdvance('=') ? SyntaxKind.MinusEqualsToken : (TextWindow.TryAdvance('-') ? SyntaxKind.MinusMinusToken : (TextWindow.TryAdvance('>') ? SyntaxKind.MinusGreaterThanToken : SyntaxKind.MinusToken)));
			break;
		case '%':
			TextWindow.AdvanceChar();
			info.Kind = (TextWindow.TryAdvance('=') ? SyntaxKind.PercentEqualsToken : SyntaxKind.PercentToken);
			break;
		case '&':
			TextWindow.AdvanceChar();
			info.Kind = (TextWindow.TryAdvance('=') ? SyntaxKind.AmpersandEqualsToken : (TextWindow.TryAdvance('&') ? SyntaxKind.AmpersandAmpersandToken : SyntaxKind.AmpersandToken));
			break;
		case '^':
			TextWindow.AdvanceChar();
			info.Kind = (TextWindow.TryAdvance('=') ? SyntaxKind.CaretEqualsToken : SyntaxKind.CaretToken);
			break;
		case '|':
			TextWindow.AdvanceChar();
			info.Kind = (TextWindow.TryAdvance('=') ? SyntaxKind.BarEqualsToken : (TextWindow.TryAdvance('|') ? SyntaxKind.BarBarToken : SyntaxKind.BarToken));
			break;
		case '<':
			TextWindow.AdvanceChar();
			info.Kind = (TextWindow.TryAdvance('=') ? SyntaxKind.LessThanEqualsToken : ((!TextWindow.TryAdvance('<')) ? SyntaxKind.LessThanToken : (TextWindow.TryAdvance('=') ? SyntaxKind.LessThanLessThanEqualsToken : SyntaxKind.LessThanLessThanToken)));
			break;
		case '>':
			TextWindow.AdvanceChar();
			info.Kind = (TextWindow.TryAdvance('=') ? SyntaxKind.GreaterThanEqualsToken : SyntaxKind.GreaterThanToken);
			break;
		case '@':
			if (!TryScanAtStringToken(ref info) && !ScanIdentifierOrKeyword(ref info))
			{
				if (TextWindow.PeekChar(1) == ':')
				{
					info.Kind = SyntaxKind.RazorContentToken;
					AddError(TextWindow.Position + 1, 1, ErrorCode.ERR_ExpectedVerbatimLiteral);
					ScanToEndOfLine();
					info.Text = GetNonInternedLexemeText();
				}
				else
				{
					ConsumeAtSignSequence();
					info.Text = GetInternedLexemeText();
					AddError(ErrorCode.ERR_ExpectedVerbatimLiteral);
				}
			}
			break;
		case '$':
			if (!TryScanInterpolatedString(ref info))
			{
				if (ModeIs(LexerMode.DebuggerSyntax))
				{
					goto case 'A';
				}
				goto default;
			}
			break;
		case 'A':
		case 'B':
		case 'C':
		case 'D':
		case 'E':
		case 'F':
		case 'G':
		case 'H':
		case 'I':
		case 'J':
		case 'K':
		case 'L':
		case 'M':
		case 'N':
		case 'O':
		case 'P':
		case 'Q':
		case 'R':
		case 'S':
		case 'T':
		case 'U':
		case 'V':
		case 'W':
		case 'X':
		case 'Y':
		case 'Z':
		case '_':
		case 'a':
		case 'b':
		case 'c':
		case 'd':
		case 'e':
		case 'f':
		case 'g':
		case 'h':
		case 'i':
		case 'j':
		case 'k':
		case 'l':
		case 'm':
		case 'n':
		case 'o':
		case 'p':
		case 'q':
		case 'r':
		case 's':
		case 't':
		case 'u':
		case 'v':
		case 'w':
		case 'x':
		case 'y':
		case 'z':
			ScanIdentifierOrKeyword(ref info);
			break;
		case '0':
		case '1':
		case '2':
		case '3':
		case '4':
		case '5':
		case '6':
		case '7':
		case '8':
		case '9':
			ScanNumericLiteral(ref info);
			break;
		case '\\':
			flag = true;
			c = PeekCharOrUnicodeEscape(out surrogateCharacter);
			if (SyntaxFacts.IsIdentifierStartCharacter(c))
			{
				goto case 'A';
			}
			goto default;
		case '\uffff':
			if (TextWindow.IsReallyAtEnd())
			{
				if (_directives.HasUnfinishedIf())
				{
					AddError(ErrorCode.ERR_EndifDirectiveExpected);
				}
				if (_directives.HasUnfinishedRegion())
				{
					AddError(ErrorCode.ERR_EndRegionDirectiveExpected);
				}
				info.Kind = SyntaxKind.EndOfFileToken;
				break;
			}
			goto default;
		default:
			if (!SyntaxFacts.IsIdentifierStartCharacter(c))
			{
				if (flag)
				{
					NextCharOrUnicodeEscape(out surrogateCharacter, out SyntaxDiagnosticInfo info2);
					AddError(info2);
				}
				else
				{
					TextWindow.AdvanceChar();
					if (char.IsHighSurrogate(c) && char.IsLowSurrogate(TextWindow.PeekChar()))
					{
						TextWindow.AdvanceChar();
					}
				}
				if (_badTokenCount++ <= 200)
				{
					info.Text = GetInternedLexemeText();
				}
				else
				{
					int length = TextWindow.Text.Length;
					info.Text = TextWindow.Text.ToString(TextSpan.FromBounds(position, length));
					TextWindow.Reset(length);
				}
				string text = (flag ? info.Text : ObjectDisplay.FormatLiteral(info.Text, ObjectDisplayOptions.EscapeNonPrintableCharacters));
				AddError(ErrorCode.ERR_UnexpectedCharacter, text);
				break;
			}
			goto case 'A';
		}
	}

	private bool TryScanAtStringToken(ref TokenInfo info)
	{
		int i;
		for (i = 0; TextWindow.PeekChar(i) == '@'; i++)
		{
		}
		if (TextWindow.PeekChar(i) == '"')
		{
			ScanVerbatimStringLiteral(ref info);
			return true;
		}
		if (TextWindow.PeekChar(i) == '$')
		{
			ScanInterpolatedStringLiteral(ref info);
			return true;
		}
		return false;
	}

	private bool TryScanInterpolatedString(ref TokenInfo info)
	{
		char c = TextWindow.PeekChar(1);
		if ((c == '"' || c == '$' || c == '@') ? true : false)
		{
			ScanInterpolatedStringLiteral(ref info);
			return true;
		}
		return false;
	}

	private void CheckFeatureAvailability(MessageID feature)
	{
		CSDiagnosticInfo featureAvailabilityDiagnosticInfo = feature.GetFeatureAvailabilityDiagnosticInfo(Options);
		if (featureAvailabilityDiagnosticInfo != null)
		{
			AddError(featureAvailabilityDiagnosticInfo.Code, featureAvailabilityDiagnosticInfo.Arguments);
		}
	}

	private bool ScanInteger()
	{
		int position = TextWindow.Position;
		while (true)
		{
			char c = TextWindow.PeekChar();
			if (c < '0' || c > '9')
			{
				break;
			}
			TextWindow.AdvanceChar();
		}
		return position < TextWindow.Position;
	}

	private void ScanNumericLiteralSingleInteger(ref bool underscoreInWrongPlace, ref bool usedUnderscore, ref bool firstCharWasUnderscore, bool isHex, bool isBinary)
	{
		if (TextWindow.PeekChar() == '_')
		{
			if (isHex | isBinary)
			{
				firstCharWasUnderscore = true;
			}
			else
			{
				underscoreInWrongPlace = true;
			}
		}
		bool flag = false;
		while (true)
		{
			char c = TextWindow.PeekChar();
			if (c == '_')
			{
				usedUnderscore = true;
				flag = true;
			}
			else
			{
				if (!(isHex ? SyntaxFacts.IsHexDigit(c) : (isBinary ? SyntaxFacts.IsBinaryDigit(c) : SyntaxFacts.IsDecDigit(c))))
				{
					break;
				}
				_builder.Append(c);
				flag = false;
			}
			TextWindow.AdvanceChar();
		}
		if (flag)
		{
			underscoreInWrongPlace = true;
		}
	}

	private bool ScanNumericLiteral(ref TokenInfo info)
	{
		int position = TextWindow.Position;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		info.Text = null;
		info.ValueKind = SpecialType.None;
		_builder.Clear();
		bool flag5 = false;
		bool flag6 = false;
		bool underscoreInWrongPlace = false;
		bool usedUnderscore = false;
		bool firstCharWasUnderscore = false;
		char c = TextWindow.PeekChar();
		if (c == '0')
		{
			switch (TextWindow.PeekChar(1))
			{
			case 'X':
			case 'x':
				TextWindow.AdvanceChar(2);
				flag = true;
				break;
			case 'B':
			case 'b':
				CheckFeatureAvailability(MessageID.IDS_FeatureBinaryLiteral);
				TextWindow.AdvanceChar(2);
				flag2 = true;
				break;
			}
		}
		if (flag | flag2)
		{
			ScanNumericLiteralSingleInteger(ref underscoreInWrongPlace, ref usedUnderscore, ref firstCharWasUnderscore, flag, flag2);
			char c2 = TextWindow.PeekChar();
			if ((c2 == 'L' || c2 == 'l') ? true : false)
			{
				TextWindow.AdvanceChar();
				flag6 = true;
				c2 = TextWindow.PeekChar();
				if ((c2 == 'U' || c2 == 'u') ? true : false)
				{
					TextWindow.AdvanceChar();
					flag5 = true;
				}
			}
			else
			{
				c2 = TextWindow.PeekChar();
				if ((c2 == 'U' || c2 == 'u') ? true : false)
				{
					TextWindow.AdvanceChar();
					flag5 = true;
					c2 = TextWindow.PeekChar();
					if ((c2 == 'L' || c2 == 'l') ? true : false)
					{
						TextWindow.AdvanceChar();
						flag6 = true;
					}
				}
			}
		}
		else
		{
			ScanNumericLiteralSingleInteger(ref underscoreInWrongPlace, ref usedUnderscore, ref firstCharWasUnderscore, isHex: false, isBinary: false);
			if (ModeIs(LexerMode.DebuggerSyntax) && TextWindow.PeekChar() == '#')
			{
				TextWindow.AdvanceChar();
				info.StringValue = (info.Text = GetInternedLexemeText());
				info.Kind = SyntaxKind.IdentifierToken;
				AddError(AbstractLexer.MakeError(ErrorCode.ERR_LegacyObjectIdSyntax));
				return true;
			}
			if ((c = TextWindow.PeekChar()) == '.')
			{
				char c3 = TextWindow.PeekChar(1);
				if (c3 >= '0' && c3 <= '9')
				{
					flag3 = true;
					_builder.Append(c);
					TextWindow.AdvanceChar();
					ScanNumericLiteralSingleInteger(ref underscoreInWrongPlace, ref usedUnderscore, ref firstCharWasUnderscore, isHex: false, isBinary: false);
				}
				else if (_builder.Length == 0)
				{
					TextWindow.Reset(position);
					return false;
				}
			}
			char c2 = (c = TextWindow.PeekChar());
			if ((c2 == 'E' || c2 == 'e') ? true : false)
			{
				_builder.Append(c);
				TextWindow.AdvanceChar();
				flag4 = true;
				c2 = (c = TextWindow.PeekChar());
				if ((c2 == '+' || c2 == '-') ? true : false)
				{
					_builder.Append(c);
					TextWindow.AdvanceChar();
				}
				if (((c = TextWindow.PeekChar()) < '0' || c > '9') && c != '_')
				{
					AddError(AbstractLexer.MakeError(ErrorCode.ERR_InvalidReal));
					_builder.Append('0');
				}
				else
				{
					ScanNumericLiteralSingleInteger(ref underscoreInWrongPlace, ref usedUnderscore, ref firstCharWasUnderscore, isHex: false, isBinary: false);
				}
			}
			c = TextWindow.PeekChar();
			if (flag4 | flag3)
			{
				if ((c == 'F' || c == 'f') ? true : false)
				{
					TextWindow.AdvanceChar();
					info.ValueKind = SpecialType.System_Single;
				}
				else if ((c == 'D' || c == 'd') ? true : false)
				{
					TextWindow.AdvanceChar();
					info.ValueKind = SpecialType.System_Double;
				}
				else if ((c == 'M' || c == 'm') ? true : false)
				{
					TextWindow.AdvanceChar();
					info.ValueKind = SpecialType.System_Decimal;
				}
				else
				{
					info.ValueKind = SpecialType.System_Double;
				}
			}
			else if ((c == 'F' || c == 'f') ? true : false)
			{
				TextWindow.AdvanceChar();
				info.ValueKind = SpecialType.System_Single;
			}
			else if ((c == 'D' || c == 'd') ? true : false)
			{
				TextWindow.AdvanceChar();
				info.ValueKind = SpecialType.System_Double;
			}
			else if ((c == 'M' || c == 'm') ? true : false)
			{
				TextWindow.AdvanceChar();
				info.ValueKind = SpecialType.System_Decimal;
			}
			else if ((c == 'L' || c == 'l') ? true : false)
			{
				TextWindow.AdvanceChar();
				flag6 = true;
				c2 = TextWindow.PeekChar();
				if ((c2 == 'U' || c2 == 'u') ? true : false)
				{
					TextWindow.AdvanceChar();
					flag5 = true;
				}
			}
			else if (c == 'u' || c == 'U')
			{
				flag5 = true;
				TextWindow.AdvanceChar();
				c2 = TextWindow.PeekChar();
				if ((c2 == 'L' || c2 == 'l') ? true : false)
				{
					TextWindow.AdvanceChar();
					flag6 = true;
				}
			}
		}
		if (underscoreInWrongPlace)
		{
			AddError(MakeError(position, TextWindow.Position - position, ErrorCode.ERR_InvalidNumber));
		}
		else if (firstCharWasUnderscore)
		{
			CheckFeatureAvailability(MessageID.IDS_FeatureLeadingDigitSeparator);
		}
		else if (usedUnderscore)
		{
			CheckFeatureAvailability(MessageID.IDS_FeatureDigitSeparator);
		}
		info.Kind = SyntaxKind.NumericLiteralToken;
		info.Text = GetInternedLexemeText();
		string text = TextWindow.Intern(_builder);
		switch (info.ValueKind)
		{
		case SpecialType.System_Single:
			info.FloatValue = GetValueSingle(text);
			break;
		case SpecialType.System_Double:
			info.DoubleValue = GetValueDouble(text);
			break;
		case SpecialType.System_Decimal:
			info.DecimalValue = GetValueDecimal(text, position, TextWindow.Position);
			break;
		default:
		{
			ulong num;
			if (string.IsNullOrEmpty(text))
			{
				if (!underscoreInWrongPlace)
				{
					AddError(AbstractLexer.MakeError(ErrorCode.ERR_InvalidNumber));
				}
				num = 0uL;
			}
			else
			{
				num = GetValueUInt64(text, flag, flag2);
			}
			if (!flag5 && !flag6)
			{
				if (num <= int.MaxValue)
				{
					info.ValueKind = SpecialType.System_Int32;
					info.IntValue = (int)num;
				}
				else if (num <= uint.MaxValue)
				{
					info.ValueKind = SpecialType.System_UInt32;
					info.UintValue = (uint)num;
				}
				else if (num <= long.MaxValue)
				{
					info.ValueKind = SpecialType.System_Int64;
					info.LongValue = (long)num;
				}
				else
				{
					info.ValueKind = SpecialType.System_UInt64;
					info.UlongValue = num;
				}
			}
			else if (flag5 && !flag6)
			{
				if (num <= uint.MaxValue)
				{
					info.ValueKind = SpecialType.System_UInt32;
					info.UintValue = (uint)num;
				}
				else
				{
					info.ValueKind = SpecialType.System_UInt64;
					info.UlongValue = num;
				}
			}
			else if (!flag5 & flag6)
			{
				if (num <= long.MaxValue)
				{
					info.ValueKind = SpecialType.System_Int64;
					info.LongValue = (long)num;
				}
				else
				{
					info.ValueKind = SpecialType.System_UInt64;
					info.UlongValue = num;
				}
			}
			else
			{
				info.ValueKind = SpecialType.System_UInt64;
				info.UlongValue = num;
			}
			break;
		}
		}
		return true;
	}

	private static bool TryParseBinaryUInt64(string text, out ulong value)
	{
		value = 0uL;
		foreach (char c in text)
		{
			if ((value & 0x8000000000000000uL) != 0L)
			{
				return false;
			}
			ulong num = (ulong)SyntaxFacts.BinaryValue(c);
			value = (value << 1) | num;
		}
		return true;
	}

	private int GetValueInt32(string text, bool isHex)
	{
		if (!int.TryParse(text, isHex ? NumberStyles.AllowHexSpecifier : NumberStyles.None, CultureInfo.InvariantCulture, out var result))
		{
			AddError(AbstractLexer.MakeError(ErrorCode.ERR_IntOverflow));
		}
		return result;
	}

	private ulong GetValueUInt64(string text, bool isHex, bool isBinary)
	{
		ulong result;
		if (isBinary)
		{
			if (!TryParseBinaryUInt64(text, out result))
			{
				AddError(AbstractLexer.MakeError(ErrorCode.ERR_IntOverflow));
			}
		}
		else if (!ulong.TryParse(text, isHex ? NumberStyles.AllowHexSpecifier : NumberStyles.None, CultureInfo.InvariantCulture, out result))
		{
			AddError(AbstractLexer.MakeError(ErrorCode.ERR_IntOverflow));
		}
		return result;
	}

	private double GetValueDouble(string text)
	{
		if (!RealParser.TryParseDouble(text, out var d))
		{
			AddError(AbstractLexer.MakeError(ErrorCode.ERR_FloatOverflow, "double"));
		}
		return d;
	}

	private float GetValueSingle(string text)
	{
		if (!RealParser.TryParseFloat(text, out var f))
		{
			AddError(AbstractLexer.MakeError(ErrorCode.ERR_FloatOverflow, "float"));
		}
		return f;
	}

	private decimal GetValueDecimal(string text, int start, int end)
	{
		if (!decimal.TryParse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out var result))
		{
			AddError(MakeError(start, end - start, ErrorCode.ERR_FloatOverflow, "decimal"));
		}
		return result;
	}

	private void ResetIdentBuffer()
	{
		_identLen = 0;
	}

	private void AddIdentChar(char ch)
	{
		if (_identLen >= _identBuffer.Length)
		{
			GrowIdentBuffer();
		}
		_identBuffer[_identLen++] = ch;
	}

	private void GrowIdentBuffer()
	{
		char[] array = new char[_identBuffer.Length * 2];
		Array.Copy(_identBuffer, array, _identBuffer.Length);
		_identBuffer = array;
	}

	private bool ScanIdentifier(ref TokenInfo info)
	{
		if (!ScanIdentifier_FastPath(ref info))
		{
			if (!InXmlCrefOrNameAttributeValue)
			{
				return ScanIdentifier_SlowPath(ref info);
			}
			return ScanIdentifier_CrefSlowPath(ref info);
		}
		return true;
	}

	private bool ScanIdentifier_FastPath(ref TokenInfo info)
	{
		if ((_mode & LexerMode.MaskLexMode) == LexerMode.DebuggerSyntax)
		{
			return false;
		}
		ReadOnlySpan<char> currentWindowSpan = TextWindow.CurrentWindowSpan;
		for (int i = 0; i != currentWindowSpan.Length; i++)
		{
			switch (currentWindowSpan[i])
			{
			case '&':
				if (InXmlCrefOrNameAttributeValue)
				{
					return false;
				}
				goto case '\0';
			case '\0':
			case '\t':
			case '\n':
			case '\r':
			case ' ':
			case '!':
			case '"':
			case '%':
			case '\'':
			case '(':
			case ')':
			case '*':
			case '+':
			case ',':
			case '-':
			case '.':
			case '/':
			case ':':
			case ';':
			case '<':
			case '=':
			case '>':
			case '?':
			case '[':
			case ']':
			case '^':
			case '{':
			case '|':
			case '}':
			case '~':
			{
				int num = i;
				TextWindow.AdvanceChar(num);
				info.Text = (info.StringValue = TextWindow.Intern(currentWindowSpan.Slice(0, num)));
				info.IsVerbatim = false;
				return true;
			}
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				if (i == 0)
				{
					return false;
				}
				break;
			case 'A':
			case 'B':
			case 'C':
			case 'D':
			case 'E':
			case 'F':
			case 'G':
			case 'H':
			case 'I':
			case 'J':
			case 'K':
			case 'L':
			case 'M':
			case 'N':
			case 'O':
			case 'P':
			case 'Q':
			case 'R':
			case 'S':
			case 'T':
			case 'U':
			case 'V':
			case 'W':
			case 'X':
			case 'Y':
			case 'Z':
			case '_':
			case 'a':
			case 'b':
			case 'c':
			case 'd':
			case 'e':
			case 'f':
			case 'g':
			case 'h':
			case 'i':
			case 'j':
			case 'k':
			case 'l':
			case 'm':
			case 'n':
			case 'o':
			case 'p':
			case 'q':
			case 'r':
			case 's':
			case 't':
			case 'u':
			case 'v':
			case 'w':
			case 'x':
			case 'y':
			case 'z':
				break;
			default:
				return false;
			}
		}
		return false;
	}

	private bool ScanIdentifier_SlowPath(ref TokenInfo info)
	{
		int position = TextWindow.Position;
		ResetIdentBuffer();
		while (TextWindow.PeekChar() == '@')
		{
			TextWindow.AdvanceChar();
		}
		int num = TextWindow.Position - position;
		info.IsVerbatim = num > 0;
		bool flag = false;
		while (true)
		{
			char surrogateCharacter = '\uffff';
			bool flag2 = false;
			char c = TextWindow.PeekChar();
			while (true)
			{
				switch (c)
				{
				case '\\':
					if (!flag2 && IsUnicodeEscape())
					{
						goto IL_0133;
					}
					goto default;
				case '$':
					if (ModeIs(LexerMode.DebuggerSyntax) && _identLen <= 0)
					{
						goto case 'A';
					}
					goto case '\t';
				case '\uffff':
					if (!TextWindow.IsReallyAtEnd())
					{
						goto default;
					}
					goto case '\t';
				case '0':
					if (_identLen == 0)
					{
						if (!info.IsVerbatim || !ModeIs(LexerMode.DebuggerSyntax) || char.ToLower(TextWindow.PeekChar(1)) != 'x')
						{
							goto case '\t';
						}
						flag = true;
					}
					goto case 'A';
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					if (_identLen != 0)
					{
						goto case 'A';
					}
					goto case '\t';
				case '<':
					if (_identLen == 0 && ModeIs(LexerMode.DebuggerSyntax) && TextWindow.PeekChar(1) == '>')
					{
						TextWindow.AdvanceChar(2);
						AddIdentChar('<');
						AddIdentChar('>');
						break;
					}
					goto case '\t';
				default:
					if (_identLen != 0 || c <= '\u007f' || !SyntaxFacts.IsIdentifierStartCharacter(c))
					{
						if (_identLen <= 0 || c <= '\u007f' || !SyntaxFacts.IsIdentifierPartCharacter(c))
						{
							goto case '\t';
						}
						if (UnicodeCharacterUtilities.IsFormattingChar(c))
						{
							if (flag2)
							{
								NextCharOrUnicodeEscape(out surrogateCharacter, out SyntaxDiagnosticInfo info3);
								AddError(info3);
							}
							else
							{
								TextWindow.AdvanceChar();
							}
							break;
						}
					}
					goto case 'A';
				case 'A':
				case 'B':
				case 'C':
				case 'D':
				case 'E':
				case 'F':
				case 'G':
				case 'H':
				case 'I':
				case 'J':
				case 'K':
				case 'L':
				case 'M':
				case 'N':
				case 'O':
				case 'P':
				case 'Q':
				case 'R':
				case 'S':
				case 'T':
				case 'U':
				case 'V':
				case 'W':
				case 'X':
				case 'Y':
				case 'Z':
				case '_':
				case 'a':
				case 'b':
				case 'c':
				case 'd':
				case 'e':
				case 'f':
				case 'g':
				case 'h':
				case 'i':
				case 'j':
				case 'k':
				case 'l':
				case 'm':
				case 'n':
				case 'o':
				case 'p':
				case 'q':
				case 'r':
				case 's':
				case 't':
				case 'u':
				case 'v':
				case 'w':
				case 'x':
				case 'y':
				case 'z':
					if (flag2)
					{
						NextCharOrUnicodeEscape(out surrogateCharacter, out SyntaxDiagnosticInfo info2);
						AddError(info2);
					}
					else
					{
						TextWindow.AdvanceChar();
					}
					AddIdentChar(c);
					if (surrogateCharacter != '\uffff')
					{
						AddIdentChar(surrogateCharacter);
					}
					break;
				case '\t':
				case ' ':
				case '(':
				case ')':
				case ',':
				case '.':
				case ';':
					{
						int currentLexemeWidth = base.CurrentLexemeWidth;
						if (_identLen > 0)
						{
							info.Text = GetInternedLexemeText();
							if (_identLen == currentLexemeWidth)
							{
								info.StringValue = info.Text;
							}
							else
							{
								info.StringValue = TextWindow.Intern(_identBuffer, 0, _identLen);
							}
							if (flag)
							{
								string text = TextWindow.Intern(_identBuffer, 2, _identLen - 2);
								if (text.Length == 0 || !text.All(SyntaxFacts.IsHexDigit))
								{
									goto IL_0387;
								}
								GetValueUInt64(text, isHex: true, isBinary: false);
							}
							if (num >= 2)
							{
								AddError(position, num, ErrorCode.ERR_IllegalAtSequence);
							}
							return true;
						}
						goto IL_0387;
					}
					IL_0387:
					info.Text = null;
					info.StringValue = null;
					TextWindow.Reset(position);
					return false;
				}
				break;
				IL_0133:
				info.HasIdentifierEscapeSequence = true;
				flag2 = true;
				c = PeekUnicodeEscape(out surrogateCharacter);
			}
		}
	}

	private bool ScanIdentifier_CrefSlowPath(ref TokenInfo info)
	{
		int position = TextWindow.Position;
		ResetIdentBuffer();
		if (AdvanceIfMatches('@'))
		{
			if (InXmlNameAttributeValue)
			{
				AddIdentChar('@');
			}
			else
			{
				info.IsVerbatim = true;
			}
		}
		while (true)
		{
			int position2 = TextWindow.Position;
			char ch;
			char surrogate;
			if (TextWindow.PeekChar() == '&')
			{
				if (!TryScanXmlEntity(out ch, out surrogate))
				{
					TextWindow.Reset(position2);
					break;
				}
			}
			else
			{
				ch = TextWindow.NextChar();
				surrogate = '\uffff';
			}
			bool flag = false;
			while (true)
			{
				switch (ch)
				{
				case '\\':
				{
					bool flag2 = !flag && TextWindow.Position == position2 + 1;
					if (flag2)
					{
						char c = TextWindow.PeekChar();
						bool flag3 = ((c == 'U' || c == 'u') ? true : false);
						flag2 = flag3;
					}
					if (flag2)
					{
						info.HasIdentifierEscapeSequence = true;
						TextWindow.Reset(position2);
						flag = true;
						ch = NextUnicodeEscape(out surrogate, out SyntaxDiagnosticInfo info2);
						AddCrefError(info2);
						continue;
					}
					goto IL_01e5;
				}
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					break;
				case '\t':
				case ' ':
				case '$':
				case '(':
				case ')':
				case ',':
				case '.':
				case ';':
				case '<':
					goto IL_01bc;
				case '\uffff':
					goto IL_01ca;
				default:
					goto IL_01e5;
				case 'A':
				case 'B':
				case 'C':
				case 'D':
				case 'E':
				case 'F':
				case 'G':
				case 'H':
				case 'I':
				case 'J':
				case 'K':
				case 'L':
				case 'M':
				case 'N':
				case 'O':
				case 'P':
				case 'Q':
				case 'R':
				case 'S':
				case 'T':
				case 'U':
				case 'V':
				case 'W':
				case 'X':
				case 'Y':
				case 'Z':
				case '_':
				case 'a':
				case 'b':
				case 'c':
				case 'd':
				case 'e':
				case 'f':
				case 'g':
				case 'h':
				case 'i':
				case 'j':
				case 'k':
				case 'l':
				case 'm':
				case 'n':
				case 'o':
				case 'p':
				case 'q':
				case 'r':
				case 's':
				case 't':
				case 'u':
				case 'v':
				case 'w':
				case 'x':
				case 'y':
				case 'z':
					goto IL_022b;
				}
				break;
			}
			if (_identLen == 0)
			{
				TextWindow.Reset(position2);
				break;
			}
			goto IL_022b;
			IL_01ca:
			if (TextWindow.IsReallyAtEnd())
			{
				TextWindow.Reset(position2);
				break;
			}
			goto IL_01e5;
			IL_01bc:
			TextWindow.Reset(position2);
			break;
			IL_022b:
			AddIdentChar(ch);
			if (surrogate != '\uffff')
			{
				AddIdentChar(surrogate);
			}
			continue;
			IL_01e5:
			if (_identLen != 0 || ch <= '\u007f' || !SyntaxFacts.IsIdentifierStartCharacter(ch))
			{
				if (_identLen <= 0 || ch <= '\u007f' || !SyntaxFacts.IsIdentifierPartCharacter(ch))
				{
					TextWindow.Reset(position2);
					break;
				}
				if (UnicodeCharacterUtilities.IsFormattingChar(ch))
				{
					continue;
				}
			}
			goto IL_022b;
		}
		if (_identLen > 0)
		{
			int currentLexemeWidth = base.CurrentLexemeWidth;
			if (_identLen == currentLexemeWidth)
			{
				info.StringValue = GetInternedLexemeText();
				info.Text = info.StringValue;
			}
			else
			{
				info.StringValue = TextWindow.Intern(_identBuffer, 0, _identLen);
				info.Text = GetNonInternedLexemeText();
			}
			return true;
		}
		info.Text = null;
		info.StringValue = null;
		TextWindow.Reset(position);
		return false;
	}

	private bool ScanIdentifierOrKeyword(ref TokenInfo info)
	{
		info.ContextualKind = SyntaxKind.None;
		if (ScanIdentifier(ref info))
		{
			if (!info.IsVerbatim && !info.HasIdentifierEscapeSequence)
			{
				if (ModeIs(LexerMode.Directive))
				{
					SyntaxKind preprocessorKeywordKind = SyntaxFacts.GetPreprocessorKeywordKind(info.Text);
					if (SyntaxFacts.IsPreprocessorContextualKeyword(preprocessorKeywordKind))
					{
						info.Kind = SyntaxKind.IdentifierToken;
						info.ContextualKind = preprocessorKeywordKind;
					}
					else
					{
						info.Kind = preprocessorKeywordKind;
					}
				}
				else if (!_cache.TryGetKeywordKind(info.Text, out info.Kind))
				{
					info.ContextualKind = (info.Kind = SyntaxKind.IdentifierToken);
				}
				else if (SyntaxFacts.IsContextualKeyword(info.Kind))
				{
					info.ContextualKind = info.Kind;
					info.Kind = SyntaxKind.IdentifierToken;
				}
				if (info.Kind == SyntaxKind.None)
				{
					info.Kind = SyntaxKind.IdentifierToken;
				}
			}
			else
			{
				info.ContextualKind = (info.Kind = SyntaxKind.IdentifierToken);
			}
			return true;
		}
		info.Kind = SyntaxKind.None;
		return false;
	}

	private void LexSyntaxTrivia(bool isFollowingToken, bool isTrailing, ref SyntaxListBuilder triviaList)
	{
		bool flag = !isTrailing;
		while (true)
		{
			Start();
			char c = TextWindow.PeekChar();
			if (c == ' ')
			{
				AddTrivia(ScanWhitespace(), ref triviaList);
				continue;
			}
			if (c > '\u007f')
			{
				if (SyntaxFacts.IsWhitespace(c))
				{
					c = ' ';
				}
				else if (SyntaxFacts.IsNewLine(c))
				{
					c = '\n';
				}
			}
			switch (c)
			{
			default:
				return;
			case '\t':
			case '\v':
			case '\f':
			case '\u001a':
			case ' ':
				AddTrivia(ScanWhitespace(), ref triviaList);
				break;
			case '/':
				if ((c = TextWindow.PeekChar(1)) == '/')
				{
					if (!SuppressDocumentationCommentParse && TextWindow.PeekChar(2) == '/' && TextWindow.PeekChar(3) != '/')
					{
						if (isTrailing)
						{
							return;
						}
						AddTrivia(LexXmlDocComment(XmlDocCommentStyle.SingleLine), ref triviaList);
					}
					else
					{
						lexSingleLineComment(ref triviaList);
						flag = false;
					}
					break;
				}
				if (c != '*')
				{
					return;
				}
				if (!SuppressDocumentationCommentParse && TextWindow.PeekChar(2) == '*' && TextWindow.PeekChar(3) != '*' && TextWindow.PeekChar(3) != '/')
				{
					if (isTrailing)
					{
						return;
					}
					AddTrivia(LexXmlDocComment(XmlDocCommentStyle.Delimited), ref triviaList);
				}
				else
				{
					lexMultiLineComment(ref triviaList, '/');
					flag = false;
				}
				break;
			case '@':
				if ((c = TextWindow.PeekChar(1)) == '*')
				{
					AddError(TextWindow.Position, 1, ErrorCode.ERR_UnexpectedCharacter, '@');
					lexMultiLineComment(ref triviaList, '@');
					flag = false;
					break;
				}
				return;
			case '\n':
			case '\r':
			{
				CSharpSyntaxNode trivia = ScanEndOfLine();
				AddTrivia(trivia, ref triviaList);
				if (isTrailing)
				{
					return;
				}
				flag = true;
				break;
			}
			case '#':
				if (_allowPreprocessorDirectives)
				{
					if (isTrailing || !flag)
					{
						int position = TextWindow.Position;
						ParseDirective(isActive: false, endIsActive: false, isFollowingToken: false);
						SourceText subText = TextWindow.Text.GetSubText(TextSpan.FromBounds(position, TextWindow.Position));
						SyntaxDiagnosticInfo syntaxDiagnosticInfo = new SyntaxDiagnosticInfo(0, 1, ErrorCode.ERR_BadDirectivePlacement);
						SyntaxToken syntaxToken = SyntaxFactory.BadToken(null, subText.ToString(), null).WithDiagnosticsGreen(new DiagnosticInfo[1] { syntaxDiagnosticInfo });
						AddTrivia(SyntaxFactory.SkippedTokensTrivia(syntaxToken), ref triviaList);
					}
					else
					{
						LexDirectiveAndExcludedTrivia(isFollowingToken, ref triviaList);
					}
					flag = true;
					break;
				}
				return;
			case '<':
			case '=':
			case '|':
				if (!isTrailing && IsConflictMarkerTrivia())
				{
					LexConflictMarkerTrivia(ref triviaList);
					break;
				}
				return;
			}
		}
		void lexMultiLineComment(ref SyntaxListBuilder list, char delimiter)
		{
			ScanMultiLineComment(out var isTerminated, delimiter);
			if (!isTerminated)
			{
				AddError(ErrorCode.ERR_OpenEndedComment);
			}
			string nonInternedLexemeText = GetNonInternedLexemeText();
			AddTrivia(SyntaxFactory.Comment(nonInternedLexemeText), ref list);
		}
		void lexSingleLineComment(ref SyntaxListBuilder list)
		{
			ScanToEndOfLine();
			string nonInternedLexemeText = GetNonInternedLexemeText();
			AddTrivia(SyntaxFactory.Comment(nonInternedLexemeText), ref list);
		}
	}

	private bool IsConflictMarkerTrivia()
	{
		int position = TextWindow.Position;
		SourceText text = TextWindow.Text;
		if (position == 0 || SyntaxFacts.IsNewLine(text[position - 1]))
		{
			char c = text[position];
			if (position + s_conflictMarkerLength <= text.Length)
			{
				int i = 0;
				for (int num = s_conflictMarkerLength; i < num; i++)
				{
					if (text[position + i] != c)
					{
						return false;
					}
				}
				if ((c == '=' || c == '|') ? true : false)
				{
					return true;
				}
				if (position + s_conflictMarkerLength < text.Length)
				{
					return text[position + s_conflictMarkerLength] == ' ';
				}
				return false;
			}
		}
		return false;
	}

	private void LexConflictMarkerTrivia(ref SyntaxListBuilder triviaList)
	{
		Start();
		AddError(TextWindow.Position, s_conflictMarkerLength, ErrorCode.ERR_Merge_conflict_marker_encountered);
		char c = TextWindow.PeekChar();
		LexConflictMarkerHeader(ref triviaList);
		LexConflictMarkerEndOfLine(ref triviaList);
		if ((c == '=' || c == '|') ? true : false)
		{
			LexConflictMarkerDisabledText(c == '=', ref triviaList);
		}
	}

	private SyntaxListBuilder LexConflictMarkerDisabledText(bool atSecondMiddleMarker, ref SyntaxListBuilder triviaList)
	{
		Start();
		bool flag = false;
		while (true)
		{
			char c = TextWindow.PeekChar();
			if (c == '\uffff')
			{
				break;
			}
			if (!atSecondMiddleMarker && c == '=' && IsConflictMarkerTrivia())
			{
				flag = true;
				break;
			}
			if (c == '>' && IsConflictMarkerTrivia())
			{
				flag = true;
				break;
			}
			TextWindow.AdvanceChar();
		}
		if (base.CurrentLexemeWidth > 0)
		{
			AddTrivia(SyntaxFactory.DisabledText(GetNonInternedLexemeText()), ref triviaList);
		}
		if (flag)
		{
			LexConflictMarkerTrivia(ref triviaList);
		}
		return triviaList;
	}

	private void LexConflictMarkerEndOfLine(ref SyntaxListBuilder triviaList)
	{
		Start();
		while (SyntaxFacts.IsNewLine(TextWindow.PeekChar()))
		{
			TextWindow.AdvanceChar();
		}
		if (base.CurrentLexemeWidth > 0)
		{
			AddTrivia(SyntaxFactory.EndOfLine(GetNonInternedLexemeText()), ref triviaList);
		}
	}

	private void LexConflictMarkerHeader(ref SyntaxListBuilder triviaList)
	{
		while (true)
		{
			char c = TextWindow.PeekChar();
			if (c == '\uffff' || SyntaxFacts.IsNewLine(c))
			{
				break;
			}
			TextWindow.AdvanceChar();
		}
		AddTrivia(SyntaxFactory.ConflictMarker(GetNonInternedLexemeText()), ref triviaList);
	}

	private void AddTrivia(CSharpSyntaxNode trivia, [NotNull] ref SyntaxListBuilder? list)
	{
		if (base.HasErrors)
		{
			CSharpSyntaxNode node = trivia;
			DiagnosticInfo[] errors = GetErrors();
			trivia = node.WithDiagnosticsGreen(errors);
		}
		if (list == null)
		{
			list = new SyntaxListBuilder(8);
		}
		list.Add(trivia);
	}

	private void ScanMultiLineComment(out bool isTerminated, char delimiter)
	{
		TextWindow.AdvanceChar(2);
		while (true)
		{
			char c;
			if ((c = TextWindow.PeekChar()) == '\uffff' && TextWindow.IsReallyAtEnd())
			{
				isTerminated = false;
				return;
			}
			if (c == '*' && TextWindow.PeekChar(1) == delimiter)
			{
				break;
			}
			TextWindow.AdvanceChar();
		}
		TextWindow.AdvanceChar(2);
		isTerminated = true;
	}

	private void ScanToEndOfLine()
	{
		char c;
		while (!SyntaxFacts.IsNewLine(c = TextWindow.PeekChar()) && (c != '\uffff' || !TextWindow.IsReallyAtEnd()))
		{
			TextWindow.AdvanceChar();
		}
	}

	private CSharpSyntaxNode? ScanEndOfLine()
	{
		char ch;
		switch (ch = TextWindow.PeekChar())
		{
		case '\r':
			TextWindow.AdvanceChar();
			if (!TextWindow.TryAdvance('\n'))
			{
				return SyntaxFactory.CarriageReturn;
			}
			return SyntaxFactory.CarriageReturnLineFeed;
		case '\n':
			TextWindow.AdvanceChar();
			return SyntaxFactory.LineFeed;
		default:
			if (SyntaxFacts.IsNewLine(ch))
			{
				TextWindow.AdvanceChar();
				return SyntaxFactory.EndOfLine(ch.ToString());
			}
			return null;
		}
	}

	private SyntaxTrivia ScanWhitespace()
	{
		int hashCode = -2128831035;
		bool flag = true;
		while (true)
		{
			char c = TextWindow.PeekChar();
			switch (c)
			{
			default:
				if (c != '\u001a')
				{
					if (c == ' ')
					{
						goto IL_003f;
					}
					if (c <= '\u007f' || !SyntaxFacts.IsWhitespace(c))
					{
						break;
					}
				}
				goto case '\t';
			case '\t':
			case '\v':
			case '\f':
				flag = false;
				goto IL_003f;
			case '\n':
			case '\r':
				break;
			}
			break;
			IL_003f:
			TextWindow.AdvanceChar();
			hashCode = Hash.CombineFNVHash(hashCode, c);
		}
		if ((base.CurrentLexemeWidth == 1) & flag)
		{
			return SyntaxFactory.Space;
		}
		if (base.CurrentLexemeWidth < 42)
		{
			return _cache.LookupWhitespaceTrivia(in TextWindow, LexemeStartPosition, hashCode);
		}
		return SyntaxFactory.Whitespace(GetInternedLexemeText());
	}

	private void LexDirectiveAndExcludedTrivia(bool isFollowingToken, ref SyntaxListBuilder triviaList)
	{
		if (LexSingleDirective(isActive: true, endIsActive: true, isFollowingToken, ref triviaList) is BranchingDirectiveTriviaSyntax { BranchTaken: false })
		{
			LexExcludedDirectivesAndTrivia(endIsActive: true, ref triviaList);
		}
	}

	private void LexExcludedDirectivesAndTrivia(bool endIsActive, ref SyntaxListBuilder triviaList)
	{
		while (true)
		{
			CSharpSyntaxNode cSharpSyntaxNode = LexDisabledText(out var followedByDirective);
			if (cSharpSyntaxNode != null)
			{
				AddTrivia(cSharpSyntaxNode, ref triviaList);
			}
			if (followedByDirective)
			{
				CSharpSyntaxNode cSharpSyntaxNode2 = LexSingleDirective(isActive: false, endIsActive, isFollowingToken: false, ref triviaList);
				BranchingDirectiveTriviaSyntax branchingDirectiveTriviaSyntax = cSharpSyntaxNode2 as BranchingDirectiveTriviaSyntax;
				if (cSharpSyntaxNode2.Kind != SyntaxKind.EndIfDirectiveTrivia && (branchingDirectiveTriviaSyntax == null || !branchingDirectiveTriviaSyntax.BranchTaken))
				{
					if (cSharpSyntaxNode2.Kind == SyntaxKind.IfDirectiveTrivia)
					{
						LexExcludedDirectivesAndTrivia(endIsActive: false, ref triviaList);
					}
					continue;
				}
				break;
			}
			break;
		}
	}

	private CSharpSyntaxNode LexSingleDirective(bool isActive, bool endIsActive, bool isFollowingToken, ref SyntaxListBuilder triviaList)
	{
		if (SyntaxFacts.IsWhitespace(TextWindow.PeekChar()))
		{
			Start();
			AddTrivia(ScanWhitespace(), ref triviaList);
		}
		CSharpSyntaxNode cSharpSyntaxNode = ParseDirective(isActive, endIsActive, isFollowingToken);
		AddTrivia(cSharpSyntaxNode, ref triviaList);
		_directives = cSharpSyntaxNode.ApplyDirectives(_directives);
		return cSharpSyntaxNode;
	}

	private CSharpSyntaxNode ParseDirective(bool isActive, bool endIsActive, bool isFollowingToken)
	{
		LexerMode mode = _mode;
		if (_directiveParser == null)
		{
			_directiveParser = new DirectiveParser(this);
		}
		_directiveParser.ReInitialize(_directives);
		CSharpSyntaxNode result = _directiveParser.ParseDirective(isActive, endIsActive, isFollowingToken);
		_mode = mode;
		return result;
	}

	private CSharpSyntaxNode? LexDisabledText(out bool followedByDirective)
	{
		Start();
		int position = TextWindow.Position;
		int num = 0;
		bool flag = true;
		while (true)
		{
			char c = TextWindow.PeekChar();
			if ((uint)c <= 13u)
			{
				if (c == '\n' || c == '\r')
				{
					goto IL_00af;
				}
			}
			else
			{
				switch (c)
				{
				case '\uffff':
					if (TextWindow.IsReallyAtEnd())
					{
						followedByDirective = false;
						if (base.CurrentLexemeWidth <= 0)
						{
							return null;
						}
						return SyntaxFactory.DisabledText(GetNonInternedLexemeText());
					}
					break;
				case '#':
					if (!_allowPreprocessorDirectives)
					{
						break;
					}
					followedByDirective = true;
					if (position >= TextWindow.Position || flag)
					{
						TextWindow.Reset(position);
						if (base.CurrentLexemeWidth <= 0)
						{
							return null;
						}
						return SyntaxFactory.DisabledText(GetNonInternedLexemeText());
					}
					break;
				}
			}
			if (!SyntaxFacts.IsNewLine(c))
			{
				flag = flag && SyntaxFacts.IsWhitespace(c);
				TextWindow.AdvanceChar();
				continue;
			}
			goto IL_00af;
			IL_00af:
			ScanEndOfLine();
			position = TextWindow.Position;
			flag = true;
			num++;
		}
	}

	private SyntaxToken LexDirectiveToken()
	{
		Start();
		TokenInfo info = default(TokenInfo);
		ScanDirectiveToken(ref info);
		SyntaxDiagnosticInfo[] errors = GetErrors();
		SyntaxListBuilder trivia = _directiveTriviaCache;
		trivia?.Clear();
		_directiveTriviaCache = null;
		LexDirectiveTrailingTrivia(info.Kind == SyntaxKind.EndOfDirectiveToken, ref trivia);
		SyntaxToken result = Create(in info, null, trivia, errors);
		_directiveTriviaCache = trivia;
		return result;
	}

	private string? LexOptionalPreprocessingMessage()
	{
		PooledStringBuilder pooledStringBuilder = null;
		while (true)
		{
			char c = TextWindow.PeekChar();
			if (SyntaxFacts.IsNewLine(c) || (c == '\uffff' && TextWindow.IsReallyAtEnd()))
			{
				break;
			}
			if (pooledStringBuilder == null)
			{
				pooledStringBuilder = PooledStringBuilder.GetInstance();
			}
			pooledStringBuilder.Builder.Append(c);
			TextWindow.AdvanceChar();
		}
		return pooledStringBuilder?.ToStringAndFree();
	}

	private SyntaxToken LexEndOfDirectiveAfterOptionalPreprocessingMessage(SyntaxTrivia? leading)
	{
		SyntaxListBuilder trivia = _directiveTriviaCache;
		trivia?.Clear();
		_directiveTriviaCache = null;
		LexDirectiveTrailingTrivia(includeEndOfLine: true, ref trivia);
		GreenNode trailing = trivia?.ToListNode();
		_directiveTriviaCache = trivia;
		return SyntaxFactory.Token(leading, SyntaxKind.EndOfDirectiveToken, trailing);
	}

	public SyntaxToken LexEndOfDirectiveWithOptionalPreprocessingMessage()
	{
		string text = LexOptionalPreprocessingMessage();
		SyntaxTrivia leading = ((text != null) ? SyntaxFactory.PreprocessingMessage(text) : null);
		return LexEndOfDirectiveAfterOptionalPreprocessingMessage(leading);
	}

	public SyntaxToken LexEndOfDirectiveWithOptionalContent(out SyntaxToken? content)
	{
		string text = LexOptionalPreprocessingMessage();
		content = ((text != null) ? SyntaxToken.StringLiteral(text) : null);
		return LexEndOfDirectiveAfterOptionalPreprocessingMessage(null);
	}

	private bool ScanDirectiveToken(ref TokenInfo info)
	{
		bool flag = false;
		char ch;
		char surrogateCharacter;
		switch (ch = TextWindow.PeekChar())
		{
		case '\uffff':
			if (TextWindow.IsReallyAtEnd())
			{
				info.Kind = SyntaxKind.EndOfDirectiveToken;
				break;
			}
			goto default;
		case '\n':
		case '\r':
			info.Kind = SyntaxKind.EndOfDirectiveToken;
			break;
		case '#':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.HashToken;
			break;
		case '(':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.OpenParenToken;
			break;
		case ')':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.CloseParenToken;
			break;
		case ',':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.CommaToken;
			break;
		case '-':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.MinusToken;
			break;
		case ':':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.ColonToken;
			break;
		case '!':
			TextWindow.AdvanceChar();
			if (TextWindow.PeekChar() == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.ExclamationEqualsToken;
			}
			else
			{
				info.Kind = SyntaxKind.ExclamationToken;
			}
			break;
		case '=':
			TextWindow.AdvanceChar();
			if (TextWindow.PeekChar() == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.EqualsEqualsToken;
			}
			else
			{
				info.Kind = SyntaxKind.EqualsToken;
			}
			break;
		case '&':
			if (TextWindow.PeekChar(1) == '&')
			{
				TextWindow.AdvanceChar(2);
				info.Kind = SyntaxKind.AmpersandAmpersandToken;
				break;
			}
			goto default;
		case '|':
			if (TextWindow.PeekChar(1) == '|')
			{
				TextWindow.AdvanceChar(2);
				info.Kind = SyntaxKind.BarBarToken;
				break;
			}
			goto default;
		case '0':
		case '1':
		case '2':
		case '3':
		case '4':
		case '5':
		case '6':
		case '7':
		case '8':
		case '9':
			ScanInteger();
			info.Kind = SyntaxKind.NumericLiteralToken;
			info.Text = GetInternedLexemeText();
			info.ValueKind = SpecialType.System_Int32;
			info.IntValue = GetValueInt32(info.Text, isHex: false);
			break;
		case '"':
			ScanStringLiteral(ref info, inDirective: true);
			break;
		case '\\':
			ch = PeekCharOrUnicodeEscape(out surrogateCharacter);
			flag = true;
			if (SyntaxFacts.IsIdentifierStartCharacter(ch))
			{
				ScanIdentifierOrKeyword(ref info);
				break;
			}
			goto default;
		default:
			if (flag || !SyntaxFacts.IsNewLine(ch))
			{
				if (SyntaxFacts.IsIdentifierStartCharacter(ch))
				{
					ScanIdentifierOrKeyword(ref info);
					break;
				}
				if (flag)
				{
					NextCharOrUnicodeEscape(out surrogateCharacter, out SyntaxDiagnosticInfo info2);
					AddError(info2);
				}
				else
				{
					TextWindow.AdvanceChar();
				}
				info.Kind = SyntaxKind.None;
				info.Text = GetInternedLexemeText();
				break;
			}
			goto case '\n';
		}
		return info.Kind != SyntaxKind.None;
	}

	private void LexDirectiveTrailingTrivia(bool includeEndOfLine, ref SyntaxListBuilder? trivia)
	{
		while (true)
		{
			int position = TextWindow.Position;
			CSharpSyntaxNode cSharpSyntaxNode = LexDirectiveTrivia();
			if (cSharpSyntaxNode == null)
			{
				break;
			}
			if (cSharpSyntaxNode.Kind == SyntaxKind.EndOfLineTrivia)
			{
				if (includeEndOfLine)
				{
					AddTrivia(cSharpSyntaxNode, ref trivia);
				}
				else
				{
					TextWindow.Reset(position);
				}
				break;
			}
			AddTrivia(cSharpSyntaxNode, ref trivia);
		}
	}

	private CSharpSyntaxNode? LexDirectiveTrivia()
	{
		CSharpSyntaxNode result = null;
		Start();
		char c = TextWindow.PeekChar();
		switch (c)
		{
		case '/':
			if (TextWindow.PeekChar(1) == '/')
			{
				ScanToEndOfLine();
				result = SyntaxFactory.Comment(GetNonInternedLexemeText());
			}
			break;
		case '\n':
		case '\r':
			result = ScanEndOfLine();
			break;
		case '\t':
		case '\v':
		case '\f':
		case ' ':
			result = ScanWhitespace();
			break;
		default:
			if (!SyntaxFacts.IsWhitespace(c))
			{
				if (!SyntaxFacts.IsNewLine(c))
				{
					break;
				}
				goto case '\n';
			}
			goto case '\t';
		}
		return result;
	}

	private CSharpSyntaxNode LexXmlDocComment(XmlDocCommentStyle style)
	{
		LexerMode mode = _mode;
		if (_xmlParser == null)
		{
			_xmlParser = new DocumentationCommentParser(this);
		}
		_xmlParser.ReInitialize((style != XmlDocCommentStyle.SingleLine) ? LexerMode.XmlDocCommentStyleDelimited : LexerMode.XmlDocCommentLocationStart);
		DocumentationCommentTriviaSyntax result = _xmlParser.ParseDocumentationComment(out var isTerminated);
		_mode = mode;
		if (!isTerminated)
		{
			AddError(LexemeStartPosition, base.CurrentLexemeWidth, ErrorCode.ERR_OpenEndedComment);
		}
		return result;
	}

	private SyntaxToken LexXmlToken()
	{
		TokenInfo info = default(TokenInfo);
		SyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTrivia(ref trivia);
		Start();
		ScanXmlToken(ref info);
		SyntaxDiagnosticInfo[] errors = GetErrors();
		return Create(in info, trivia, null, errors);
	}

	private bool ScanXmlToken(ref TokenInfo info)
	{
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		char ch;
		char c = (ch = TextWindow.PeekChar());
		if ((uint)c <= 13u)
		{
			if (c == '\n' || c == '\r')
			{
				goto IL_0066;
			}
			goto IL_0089;
		}
		if (c != '&')
		{
			if (c != '<')
			{
				if (c != '\uffff' || !TextWindow.IsReallyAtEnd())
				{
					goto IL_0089;
				}
				info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			}
			else
			{
				ScanXmlTagStart(ref info);
			}
		}
		else
		{
			ScanXmlEntity(ref info);
			info.Kind = SyntaxKind.XmlEntityLiteralToken;
		}
		goto IL_00a3;
		IL_0066:
		ScanXmlTextLiteralNewLineToken(ref info);
		goto IL_00a3;
		IL_0089:
		if (SyntaxFacts.IsNewLine(ch))
		{
			goto IL_0066;
		}
		ScanXmlText(ref info);
		info.Kind = SyntaxKind.XmlTextLiteralToken;
		goto IL_00a3;
		IL_00a3:
		return info.Kind != SyntaxKind.None;
	}

	private void ScanXmlTextLiteralNewLineToken(ref TokenInfo info)
	{
		ScanEndOfLine();
		info.StringValue = (info.Text = GetNonInternedLexemeText());
		info.Kind = SyntaxKind.XmlTextLiteralNewLineToken;
		MutateLocation(XmlDocCommentLocation.Exterior);
	}

	private void ScanXmlTagStart(ref TokenInfo info)
	{
		if (TextWindow.PeekChar(1) == '!')
		{
			if (TextWindow.PeekChar(2) == '-' && TextWindow.PeekChar(3) == '-')
			{
				TextWindow.AdvanceChar(4);
				info.Kind = SyntaxKind.XmlCommentStartToken;
			}
			else if (TextWindow.PeekChar(2) == '[' && TextWindow.PeekChar(3) == 'C' && TextWindow.PeekChar(4) == 'D' && TextWindow.PeekChar(5) == 'A' && TextWindow.PeekChar(6) == 'T' && TextWindow.PeekChar(7) == 'A' && TextWindow.PeekChar(8) == '[')
			{
				TextWindow.AdvanceChar(9);
				info.Kind = SyntaxKind.XmlCDataStartToken;
			}
			else
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.LessThanToken;
			}
		}
		else if (TextWindow.PeekChar(1) == '/')
		{
			TextWindow.AdvanceChar(2);
			info.Kind = SyntaxKind.LessThanSlashToken;
		}
		else if (TextWindow.PeekChar(1) == '?')
		{
			TextWindow.AdvanceChar(2);
			info.Kind = SyntaxKind.XmlProcessingInstructionStartToken;
		}
		else
		{
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.LessThanToken;
		}
	}

	private void ScanXmlEntity(ref TokenInfo info)
	{
		info.StringValue = null;
		TextWindow.AdvanceChar();
		_builder.Clear();
		XmlParseErrorCode? xmlParseErrorCode = null;
		object[] array = null;
		char c;
		if (IsXmlNameStartChar(c = TextWindow.PeekChar()))
		{
			while (IsXmlNameChar(c = TextWindow.PeekChar()))
			{
				TextWindow.AdvanceChar();
				_builder.Append(c);
			}
			switch (_builder.ToString())
			{
			case "lt":
				info.StringValue = "<";
				break;
			case "gt":
				info.StringValue = ">";
				break;
			case "amp":
				info.StringValue = "&";
				break;
			case "apos":
				info.StringValue = "'";
				break;
			case "quot":
				info.StringValue = "\"";
				break;
			default:
			{
				xmlParseErrorCode = XmlParseErrorCode.XML_RefUndefinedEntity_1;
				object[] array2 = new string[1] { _builder.ToString() };
				array = array2;
				break;
			}
			}
		}
		else if (c == '#')
		{
			TextWindow.AdvanceChar();
			bool num = TextWindow.PeekChar() == 'x';
			uint num2 = 0u;
			if (num)
			{
				TextWindow.AdvanceChar();
				while (SyntaxFacts.IsHexDigit(c = TextWindow.PeekChar()))
				{
					TextWindow.AdvanceChar();
					if (num2 <= 134217727)
					{
						num2 = (num2 << 4) + (uint)SyntaxFacts.HexValue(c);
					}
				}
			}
			else
			{
				while (SyntaxFacts.IsDecDigit(c = TextWindow.PeekChar()))
				{
					TextWindow.AdvanceChar();
					if (num2 <= 134217727)
					{
						num2 = (num2 << 3) + (num2 << 1) + (uint)SyntaxFacts.DecValue(c);
					}
				}
			}
			if (TextWindow.PeekChar() != ';')
			{
				xmlParseErrorCode = XmlParseErrorCode.XML_InvalidCharEntity;
			}
			if (MatchesProductionForXmlChar(num2))
			{
				char charsFromUtf = GetCharsFromUtf32(num2, out var lowSurrogate);
				_builder.Append(charsFromUtf);
				if (lowSurrogate != '\uffff')
				{
					_builder.Append(lowSurrogate);
				}
				info.StringValue = _builder.ToString();
			}
			else if (!xmlParseErrorCode.HasValue)
			{
				xmlParseErrorCode = XmlParseErrorCode.XML_InvalidUnicodeChar;
			}
		}
		else if (SyntaxFacts.IsWhitespace(c) || SyntaxFacts.IsNewLine(c))
		{
			if (!xmlParseErrorCode.HasValue)
			{
				xmlParseErrorCode = XmlParseErrorCode.XML_InvalidWhitespace;
			}
		}
		else if (!xmlParseErrorCode.HasValue)
		{
			xmlParseErrorCode = XmlParseErrorCode.XML_InvalidToken;
			object[] array2 = new string[1] { c.ToString() };
			array = array2;
		}
		c = TextWindow.PeekChar();
		if (c == ';')
		{
			TextWindow.AdvanceChar();
		}
		else if (!xmlParseErrorCode.HasValue)
		{
			xmlParseErrorCode = XmlParseErrorCode.XML_InvalidToken;
			object[] array2 = new string[1] { c.ToString() };
			array = array2;
		}
		info.Text = GetInternedLexemeText();
		if (info.StringValue == null)
		{
			info.StringValue = info.Text;
		}
		if (xmlParseErrorCode.HasValue)
		{
			AddError(xmlParseErrorCode.Value, array ?? Array.Empty<object>());
		}
	}

	private static bool MatchesProductionForXmlChar(uint charValue)
	{
		if (charValue != 9 && charValue != 10 && charValue != 13 && (charValue < 32 || charValue > 55295) && (charValue < 57344 || charValue > 65533))
		{
			if (charValue >= 65536)
			{
				return charValue <= 1114111;
			}
			return false;
		}
		return true;
	}

	private void ScanXmlText(ref TokenInfo info)
	{
		if (TextWindow.PeekChar() == ']' && TextWindow.PeekChar(1) == ']' && TextWindow.PeekChar(2) == '>')
		{
			TextWindow.AdvanceChar(3);
			info.StringValue = (info.Text = GetNonInternedLexemeText());
			AddError(XmlParseErrorCode.XML_CDataEndTagNotAllowed);
			return;
		}
		while (true)
		{
			char c = TextWindow.PeekChar();
			if ((uint)c <= 38u)
			{
				if (c == '\n' || c == '\r' || c == '&')
				{
					goto IL_00c8;
				}
			}
			else if ((uint)c <= 60u)
			{
				if (c != '*')
				{
					if (c == '<')
					{
						goto IL_00c8;
					}
				}
				else if (StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar(1) == '/')
				{
					break;
				}
			}
			else
			{
				switch (c)
				{
				case '\uffff':
					if (TextWindow.IsReallyAtEnd())
					{
						info.StringValue = (info.Text = GetNonInternedLexemeText());
						return;
					}
					break;
				case ']':
					if (TextWindow.PeekChar(1) == ']' && TextWindow.PeekChar(2) == '>')
					{
						info.StringValue = (info.Text = GetNonInternedLexemeText());
						return;
					}
					break;
				}
			}
			if (!SyntaxFacts.IsNewLine(c))
			{
				TextWindow.AdvanceChar();
				continue;
			}
			goto IL_00c8;
			IL_00c8:
			info.StringValue = (info.Text = GetNonInternedLexemeText());
			return;
		}
		info.StringValue = (info.Text = GetNonInternedLexemeText());
	}

	private SyntaxToken LexXmlElementTagToken()
	{
		TokenInfo info = default(TokenInfo);
		SyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTriviaWithWhitespace(ref trivia);
		Start();
		ScanXmlElementTagToken(ref info);
		SyntaxDiagnosticInfo[] errors = GetErrors();
		if (errors == null && info.ContextualKind == SyntaxKind.None && info.Kind == SyntaxKind.IdentifierToken)
		{
			SyntaxToken syntaxToken = DocumentationCommentXmlTokens.LookupToken(info.Text, trivia);
			if (syntaxToken != null)
			{
				return syntaxToken;
			}
		}
		return Create(in info, trivia, null, errors);
	}

	private bool ScanXmlElementTagToken(ref TokenInfo info)
	{
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		char ch;
		switch (ch = TextWindow.PeekChar())
		{
		case '<':
			ScanXmlTagStart(ref info);
			break;
		case '>':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.GreaterThanToken;
			break;
		case '/':
			if (TextWindow.PeekChar(1) == '>')
			{
				TextWindow.AdvanceChar(2);
				info.Kind = SyntaxKind.SlashGreaterThanToken;
				break;
			}
			goto default;
		case '"':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.DoubleQuoteToken;
			break;
		case '\'':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.SingleQuoteToken;
			break;
		case '=':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.EqualsToken;
			break;
		case ':':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.ColonToken;
			break;
		case '\uffff':
			if (TextWindow.IsReallyAtEnd())
			{
				info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
				break;
			}
			goto default;
		case '*':
			if (StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar(1) == '/')
			{
				break;
			}
			goto default;
		default:
			if (IsXmlNameStartChar(ch))
			{
				ScanXmlName(ref info);
				info.StringValue = info.Text;
				info.Kind = SyntaxKind.IdentifierToken;
			}
			else if (!SyntaxFacts.IsWhitespace(ch) && !SyntaxFacts.IsNewLine(ch))
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.None;
				info.StringValue = (info.Text = GetNonInternedLexemeText());
			}
			break;
		case '\n':
		case '\r':
			break;
		}
		return info.Kind != SyntaxKind.None;
	}

	private void ScanXmlName(ref TokenInfo info)
	{
		int position = TextWindow.Position;
		while (true)
		{
			char c = TextWindow.PeekChar();
			if (c == ':' || !IsXmlNameChar(c))
			{
				break;
			}
			TextWindow.AdvanceChar();
		}
		info.Text = TextWindow.GetText(position, TextWindow.Position - position, intern: true);
	}

	private static bool IsXmlNameStartChar(char ch)
	{
		return XmlCharType.IsStartNCNameCharXml4e(ch);
	}

	private static bool IsXmlNameChar(char ch)
	{
		return XmlCharType.IsNCNameCharXml4e(ch);
	}

	private SyntaxToken LexXmlAttributeTextToken()
	{
		TokenInfo info = default(TokenInfo);
		SyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTrivia(ref trivia);
		Start();
		ScanXmlAttributeTextToken(ref info);
		SyntaxDiagnosticInfo[] errors = GetErrors();
		return Create(in info, trivia, null, errors);
	}

	private bool ScanXmlAttributeTextToken(ref TokenInfo info)
	{
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		char ch;
		char c = (ch = TextWindow.PeekChar());
		if ((uint)c <= 34u)
		{
			if (c == '\n' || c == '\r')
			{
				goto IL_00e2;
			}
			if (c != '"' || !ModeIs(LexerMode.XmlAttributeTextDoubleQuote))
			{
				goto IL_0105;
			}
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.DoubleQuoteToken;
		}
		else if ((uint)c <= 39u)
		{
			if (c != '&')
			{
				if (c != '\'' || !ModeIs(LexerMode.XmlAttributeTextQuote))
				{
					goto IL_0105;
				}
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.SingleQuoteToken;
			}
			else
			{
				ScanXmlEntity(ref info);
				info.Kind = SyntaxKind.XmlEntityLiteralToken;
			}
		}
		else if (c != '<')
		{
			if (c != '\uffff' || !TextWindow.IsReallyAtEnd())
			{
				goto IL_0105;
			}
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
		}
		else
		{
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.LessThanToken;
		}
		goto IL_011f;
		IL_0105:
		if (SyntaxFacts.IsNewLine(ch))
		{
			goto IL_00e2;
		}
		ScanXmlAttributeText(ref info);
		info.Kind = SyntaxKind.XmlTextLiteralToken;
		goto IL_011f;
		IL_011f:
		return info.Kind != SyntaxKind.None;
		IL_00e2:
		ScanXmlTextLiteralNewLineToken(ref info);
		goto IL_011f;
	}

	private void ScanXmlAttributeText(ref TokenInfo info)
	{
		while (true)
		{
			char c = TextWindow.PeekChar();
			switch (c)
			{
			case '"':
				if (ModeIs(LexerMode.XmlAttributeTextDoubleQuote))
				{
					info.StringValue = (info.Text = GetNonInternedLexemeText());
					return;
				}
				goto default;
			case '\'':
				if (ModeIs(LexerMode.XmlAttributeTextQuote))
				{
					info.StringValue = (info.Text = GetNonInternedLexemeText());
					return;
				}
				goto default;
			case '\n':
			case '\r':
			case '&':
			case '<':
				info.StringValue = (info.Text = GetNonInternedLexemeText());
				return;
			case '\uffff':
				if (TextWindow.IsReallyAtEnd())
				{
					info.StringValue = (info.Text = GetNonInternedLexemeText());
					return;
				}
				goto default;
			case '*':
				if (StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar(1) == '/')
				{
					info.StringValue = (info.Text = GetNonInternedLexemeText());
					return;
				}
				goto default;
			default:
				if (!SyntaxFacts.IsNewLine(c))
				{
					break;
				}
				goto case '\n';
			}
			TextWindow.AdvanceChar();
		}
	}

	private SyntaxToken LexXmlCharacter()
	{
		TokenInfo info = default(TokenInfo);
		SyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTriviaWithWhitespace(ref trivia);
		Start();
		ScanXmlCharacter(ref info);
		SyntaxDiagnosticInfo[] errors = GetErrors();
		return Create(in info, trivia, null, errors);
	}

	private bool ScanXmlCharacter(ref TokenInfo info)
	{
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		char c = TextWindow.PeekChar();
		if (c != '&')
		{
			if (c == '\uffff' && TextWindow.IsReallyAtEnd())
			{
				info.Kind = SyntaxKind.EndOfFileToken;
			}
			else
			{
				info.Kind = SyntaxKind.XmlTextLiteralToken;
				info.Text = (info.StringValue = TextWindow.NextChar().ToString());
			}
		}
		else
		{
			ScanXmlEntity(ref info);
			info.Kind = SyntaxKind.XmlEntityLiteralToken;
		}
		return true;
	}

	private SyntaxToken LexXmlCrefOrNameToken()
	{
		TokenInfo info = default(TokenInfo);
		SyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTriviaWithWhitespace(ref trivia);
		Start();
		ScanXmlCrefToken(ref info);
		SyntaxDiagnosticInfo[] errors = GetErrors();
		return Create(in info, trivia, null, errors);
	}

	private bool ScanXmlCrefToken(ref TokenInfo info)
	{
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		int position = TextWindow.Position;
		char ch = TextWindow.NextChar();
		char surrogate = '\uffff';
		if ((uint)ch <= 38u)
		{
			if ((uint)ch <= 13u)
			{
				if (ch == '\n' || ch == '\r')
				{
					goto IL_0128;
				}
				goto IL_017f;
			}
			if (ch != '"')
			{
				if (ch != '&')
				{
					goto IL_017f;
				}
				TextWindow.Reset(position);
				if (!TryScanXmlEntity(out ch, out surrogate))
				{
					TextWindow.Reset(position);
					ScanXmlEntity(ref info);
					info.Kind = SyntaxKind.XmlEntityLiteralToken;
					return true;
				}
			}
			else if (ModeIs(LexerMode.XmlCrefDoubleQuote) || ModeIs(LexerMode.XmlNameDoubleQuote))
			{
				info.Kind = SyntaxKind.DoubleQuoteToken;
				return true;
			}
		}
		else if ((uint)ch <= 60u)
		{
			if (ch != '\'')
			{
				if (ch != '<')
				{
					goto IL_017f;
				}
				info.Text = GetNonInternedLexemeText();
				AddError(XmlParseErrorCode.XML_LessThanInAttributeValue, info.Text);
				return true;
			}
			if (ModeIs(LexerMode.XmlCrefQuote) || ModeIs(LexerMode.XmlNameQuote))
			{
				info.Kind = SyntaxKind.SingleQuoteToken;
				return true;
			}
		}
		else if (ch != '{')
		{
			if (ch != '}')
			{
				if (ch != '\uffff' || !TextWindow.IsReallyAtEnd())
				{
					goto IL_017f;
				}
				info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
				return true;
			}
			ch = '>';
		}
		else
		{
			ch = '<';
		}
		goto IL_0187;
		IL_0187:
		switch (ch)
		{
		case '(':
			info.Kind = SyntaxKind.OpenParenToken;
			break;
		case ')':
			info.Kind = SyntaxKind.CloseParenToken;
			break;
		case '[':
			info.Kind = SyntaxKind.OpenBracketToken;
			break;
		case ']':
			info.Kind = SyntaxKind.CloseBracketToken;
			break;
		case ',':
			info.Kind = SyntaxKind.CommaToken;
			break;
		case '.':
			if (AdvanceIfMatches('.'))
			{
				if (TextWindow.PeekChar() == '.')
				{
					AddCrefError(ErrorCode.ERR_UnexpectedCharacter, ".");
				}
				info.Kind = SyntaxKind.DotDotToken;
			}
			else
			{
				info.Kind = SyntaxKind.DotToken;
			}
			break;
		case '?':
			info.Kind = SyntaxKind.QuestionToken;
			break;
		case '&':
			info.Kind = SyntaxKind.AmpersandToken;
			break;
		case '*':
			info.Kind = SyntaxKind.AsteriskToken;
			break;
		case '|':
			info.Kind = SyntaxKind.BarToken;
			break;
		case '^':
			info.Kind = SyntaxKind.CaretToken;
			break;
		case '%':
			info.Kind = SyntaxKind.PercentToken;
			break;
		case '/':
			info.Kind = SyntaxKind.SlashToken;
			break;
		case '~':
			info.Kind = SyntaxKind.TildeToken;
			break;
		case '{':
			info.Kind = SyntaxKind.LessThanToken;
			break;
		case '}':
			info.Kind = SyntaxKind.GreaterThanToken;
			break;
		case ':':
			if (AdvanceIfMatches(':'))
			{
				info.Kind = SyntaxKind.ColonColonToken;
			}
			else
			{
				info.Kind = SyntaxKind.ColonToken;
			}
			break;
		case '=':
			if (AdvanceIfMatches('='))
			{
				info.Kind = SyntaxKind.EqualsEqualsToken;
			}
			else
			{
				info.Kind = SyntaxKind.EqualsToken;
			}
			break;
		case '!':
			if (AdvanceIfMatches('='))
			{
				info.Kind = SyntaxKind.ExclamationEqualsToken;
			}
			else
			{
				info.Kind = SyntaxKind.ExclamationToken;
			}
			break;
		case '>':
			if (AdvanceIfMatches('='))
			{
				info.Kind = SyntaxKind.GreaterThanEqualsToken;
			}
			else
			{
				info.Kind = SyntaxKind.GreaterThanToken;
			}
			break;
		case '<':
			if (AdvanceIfMatches('='))
			{
				info.Kind = SyntaxKind.LessThanEqualsToken;
			}
			else if (AdvanceIfMatches('<'))
			{
				info.Kind = SyntaxKind.LessThanLessThanToken;
			}
			else
			{
				info.Kind = SyntaxKind.LessThanToken;
			}
			break;
		case '+':
			if (AdvanceIfMatches('+'))
			{
				info.Kind = SyntaxKind.PlusPlusToken;
			}
			else
			{
				info.Kind = SyntaxKind.PlusToken;
			}
			break;
		case '-':
			if (AdvanceIfMatches('-'))
			{
				info.Kind = SyntaxKind.MinusMinusToken;
			}
			else
			{
				info.Kind = SyntaxKind.MinusToken;
			}
			break;
		}
		if (info.Kind != SyntaxKind.None)
		{
			string text = SyntaxFacts.GetText(info.Kind);
			string nonInternedLexemeText = GetNonInternedLexemeText();
			if (!string.IsNullOrEmpty(text) && nonInternedLexemeText != text)
			{
				info.RequiresTextForXmlEntity = true;
				info.Text = nonInternedLexemeText;
				info.StringValue = text;
			}
		}
		else
		{
			TextWindow.Reset(position);
			if (ScanIdentifier(ref info) && info.Text.Length > 0)
			{
				if (!InXmlNameAttributeValue && !info.IsVerbatim && !info.HasIdentifierEscapeSequence && _cache.TryGetKeywordKind(info.StringValue, out var kind))
				{
					if (SyntaxFacts.IsContextualKeyword(kind))
					{
						info.Kind = SyntaxKind.IdentifierToken;
						info.ContextualKind = kind;
					}
					else
					{
						info.Kind = kind;
						info.RequiresTextForXmlEntity = info.Text != info.StringValue;
					}
				}
				else
				{
					info.ContextualKind = (info.Kind = SyntaxKind.IdentifierToken);
				}
			}
			else if (ch == '@')
			{
				if (TextWindow.PeekChar() == '@')
				{
					TextWindow.NextChar();
					info.Text = GetInternedLexemeText();
					info.StringValue = "";
				}
				else
				{
					ScanXmlEntity(ref info);
				}
				info.Kind = SyntaxKind.IdentifierToken;
				AddError(ErrorCode.ERR_ExpectedVerbatimLiteral);
			}
			else if (TextWindow.PeekChar() == '&')
			{
				ScanXmlEntity(ref info);
				info.Kind = SyntaxKind.XmlEntityLiteralToken;
				AddCrefError(ErrorCode.ERR_UnexpectedCharacter, info.Text);
			}
			else
			{
				char charValue = TextWindow.NextChar();
				info.Text = GetNonInternedLexemeText();
				if (MatchesProductionForXmlChar(charValue))
				{
					AddCrefError(ErrorCode.ERR_UnexpectedCharacter, info.Text);
				}
				else
				{
					AddError(XmlParseErrorCode.XML_InvalidUnicodeChar);
				}
			}
		}
		return info.Kind != SyntaxKind.None;
		IL_017f:
		if (SyntaxFacts.IsNewLine(ch))
		{
			goto IL_0128;
		}
		goto IL_0187;
		IL_0128:
		TextWindow.Reset(position);
		ScanXmlTextLiteralNewLineToken(ref info);
		goto IL_0187;
	}

	private bool AdvanceIfMatches(char ch)
	{
		char c = TextWindow.PeekChar();
		if (c == ch || (c == '{' && ch == '<') || (c == '}' && ch == '>'))
		{
			TextWindow.AdvanceChar();
			return true;
		}
		if (c == '&')
		{
			int position = TextWindow.Position;
			if (TryScanXmlEntity(out var ch2, out var surrogate) && ch2 == ch && surrogate == '\uffff')
			{
				return true;
			}
			TextWindow.Reset(position);
		}
		return false;
	}

	private void AddCrefError(ErrorCode code, params object[] args)
	{
		AddCrefError(AbstractLexer.MakeError(code, args));
	}

	private void AddCrefError(DiagnosticInfo? info)
	{
		if (info != null)
		{
			AddError(ErrorCode.WRN_ErrorOverride, info, info.Code);
		}
	}

	private SyntaxToken LexXmlCDataSectionTextToken()
	{
		TokenInfo info = default(TokenInfo);
		SyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTrivia(ref trivia);
		Start();
		ScanXmlCDataSectionTextToken(ref info);
		SyntaxDiagnosticInfo[] errors = GetErrors();
		return Create(in info, trivia, null, errors);
	}

	private bool ScanXmlCDataSectionTextToken(ref TokenInfo info)
	{
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		char ch;
		char c = (ch = TextWindow.PeekChar());
		if ((uint)c <= 13u)
		{
			if (c == '\n' || c == '\r')
			{
				goto IL_007d;
			}
			goto IL_00a0;
		}
		if (c != ']')
		{
			if (c != '\uffff' || !TextWindow.IsReallyAtEnd())
			{
				goto IL_00a0;
			}
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
		}
		else
		{
			if (TextWindow.PeekChar(1) != ']' || TextWindow.PeekChar(2) != '>')
			{
				goto IL_00a0;
			}
			TextWindow.AdvanceChar(3);
			info.Kind = SyntaxKind.XmlCDataEndToken;
		}
		goto IL_00ba;
		IL_00ba:
		return true;
		IL_00a0:
		if (SyntaxFacts.IsNewLine(ch))
		{
			goto IL_007d;
		}
		ScanXmlCDataSectionText(ref info);
		info.Kind = SyntaxKind.XmlTextLiteralToken;
		goto IL_00ba;
		IL_007d:
		ScanXmlTextLiteralNewLineToken(ref info);
		goto IL_00ba;
	}

	private void ScanXmlCDataSectionText(ref TokenInfo info)
	{
		while (true)
		{
			char c = TextWindow.PeekChar();
			if ((uint)c <= 13u)
			{
				if (c == '\n' || c == '\r')
				{
					break;
				}
			}
			else
			{
				switch (c)
				{
				case ']':
					if (TextWindow.PeekChar(1) == ']' && TextWindow.PeekChar(2) == '>')
					{
						info.StringValue = (info.Text = GetNonInternedLexemeText());
						return;
					}
					break;
				case '\uffff':
					if (TextWindow.IsReallyAtEnd())
					{
						info.StringValue = (info.Text = GetNonInternedLexemeText());
						return;
					}
					break;
				case '*':
					if (StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar(1) == '/')
					{
						info.StringValue = (info.Text = GetNonInternedLexemeText());
						return;
					}
					break;
				}
			}
			if (SyntaxFacts.IsNewLine(c))
			{
				break;
			}
			TextWindow.AdvanceChar();
		}
		info.StringValue = (info.Text = GetNonInternedLexemeText());
	}

	private SyntaxToken LexXmlCommentTextToken()
	{
		TokenInfo info = default(TokenInfo);
		SyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTrivia(ref trivia);
		Start();
		ScanXmlCommentTextToken(ref info);
		SyntaxDiagnosticInfo[] errors = GetErrors();
		return Create(in info, trivia, null, errors);
	}

	private bool ScanXmlCommentTextToken(ref TokenInfo info)
	{
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		char ch;
		char c = (ch = TextWindow.PeekChar());
		if ((uint)c <= 13u)
		{
			if (c == '\n' || c == '\r')
			{
				goto IL_0099;
			}
			goto IL_00bc;
		}
		if (c != '-')
		{
			if (c != '\uffff' || !TextWindow.IsReallyAtEnd())
			{
				goto IL_00bc;
			}
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
		}
		else
		{
			if (TextWindow.PeekChar(1) != '-')
			{
				goto IL_00bc;
			}
			if (TextWindow.PeekChar(2) == '>')
			{
				TextWindow.AdvanceChar(3);
				info.Kind = SyntaxKind.XmlCommentEndToken;
			}
			else
			{
				TextWindow.AdvanceChar(2);
				info.Kind = SyntaxKind.MinusMinusToken;
			}
		}
		goto IL_00d6;
		IL_00d6:
		return true;
		IL_00bc:
		if (SyntaxFacts.IsNewLine(ch))
		{
			goto IL_0099;
		}
		ScanXmlCommentText(ref info);
		info.Kind = SyntaxKind.XmlTextLiteralToken;
		goto IL_00d6;
		IL_0099:
		ScanXmlTextLiteralNewLineToken(ref info);
		goto IL_00d6;
	}

	private void ScanXmlCommentText(ref TokenInfo info)
	{
		while (true)
		{
			char c = TextWindow.PeekChar();
			if ((uint)c <= 13u)
			{
				if (c == '\n' || c == '\r')
				{
					break;
				}
			}
			else
			{
				switch (c)
				{
				case '-':
					if (TextWindow.PeekChar(1) == '-')
					{
						info.StringValue = (info.Text = GetNonInternedLexemeText());
						return;
					}
					break;
				case '\uffff':
					if (TextWindow.IsReallyAtEnd())
					{
						info.StringValue = (info.Text = GetNonInternedLexemeText());
						return;
					}
					break;
				case '*':
					if (StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar(1) == '/')
					{
						info.StringValue = (info.Text = GetNonInternedLexemeText());
						return;
					}
					break;
				}
			}
			if (SyntaxFacts.IsNewLine(c))
			{
				break;
			}
			TextWindow.AdvanceChar();
		}
		info.StringValue = (info.Text = GetNonInternedLexemeText());
	}

	private SyntaxToken LexXmlProcessingInstructionTextToken()
	{
		TokenInfo info = default(TokenInfo);
		SyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTrivia(ref trivia);
		Start();
		ScanXmlProcessingInstructionTextToken(ref info);
		SyntaxDiagnosticInfo[] errors = GetErrors();
		return Create(in info, trivia, null, errors);
	}

	private bool ScanXmlProcessingInstructionTextToken(ref TokenInfo info)
	{
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		char ch;
		char c = (ch = TextWindow.PeekChar());
		if ((uint)c <= 13u)
		{
			if (c == '\n' || c == '\r')
			{
				goto IL_006d;
			}
			goto IL_0090;
		}
		if (c != '?')
		{
			if (c != '\uffff' || !TextWindow.IsReallyAtEnd())
			{
				goto IL_0090;
			}
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
		}
		else
		{
			if (TextWindow.PeekChar(1) != '>')
			{
				goto IL_0090;
			}
			TextWindow.AdvanceChar(2);
			info.Kind = SyntaxKind.XmlProcessingInstructionEndToken;
		}
		goto IL_00aa;
		IL_006d:
		ScanXmlTextLiteralNewLineToken(ref info);
		goto IL_00aa;
		IL_00aa:
		return true;
		IL_0090:
		if (SyntaxFacts.IsNewLine(ch))
		{
			goto IL_006d;
		}
		ScanXmlProcessingInstructionText(ref info);
		info.Kind = SyntaxKind.XmlTextLiteralToken;
		goto IL_00aa;
	}

	private void ScanXmlProcessingInstructionText(ref TokenInfo info)
	{
		while (true)
		{
			char c = TextWindow.PeekChar();
			if ((uint)c <= 13u)
			{
				if (c == '\n' || c == '\r')
				{
					break;
				}
			}
			else
			{
				switch (c)
				{
				case '?':
					if (TextWindow.PeekChar(1) == '>')
					{
						info.StringValue = (info.Text = GetNonInternedLexemeText());
						return;
					}
					break;
				case '\uffff':
					if (TextWindow.IsReallyAtEnd())
					{
						info.StringValue = (info.Text = GetNonInternedLexemeText());
						return;
					}
					break;
				case '*':
					if (StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar(1) == '/')
					{
						info.StringValue = (info.Text = GetNonInternedLexemeText());
						return;
					}
					break;
				}
			}
			if (SyntaxFacts.IsNewLine(c))
			{
				break;
			}
			TextWindow.AdvanceChar();
		}
		info.StringValue = (info.Text = GetNonInternedLexemeText());
	}

	private void LexXmlDocCommentLeadingTrivia(ref SyntaxListBuilder? trivia)
	{
		int position = TextWindow.Position;
		Start();
		if (LocationIs(XmlDocCommentLocation.Start) && StyleIs(XmlDocCommentStyle.Delimited))
		{
			if (TextWindow.PeekChar() == '/' && TextWindow.PeekChar(1) == '*' && TextWindow.PeekChar(2) == '*' && TextWindow.PeekChar(3) != '*')
			{
				TextWindow.AdvanceChar(3);
				string internedLexemeText = GetInternedLexemeText();
				AddTrivia(SyntaxFactory.DocumentationCommentExteriorTrivia(internedLexemeText), ref trivia);
				MutateLocation(XmlDocCommentLocation.Interior);
			}
		}
		else if (LocationIs(XmlDocCommentLocation.Start) || LocationIs(XmlDocCommentLocation.Exterior))
		{
			while (true)
			{
				char c = TextWindow.PeekChar();
				switch (c)
				{
				case '\t':
				case '\v':
				case '\f':
				case ' ':
					goto IL_00f2;
				case '/':
					if (StyleIs(XmlDocCommentStyle.SingleLine) && TextWindow.PeekChar(1) == '/' && TextWindow.PeekChar(2) == '/' && TextWindow.PeekChar(3) != '/')
					{
						TextWindow.AdvanceChar(3);
						string internedLexemeText3 = GetInternedLexemeText();
						AddTrivia(SyntaxFactory.DocumentationCommentExteriorTrivia(internedLexemeText3), ref trivia);
						MutateLocation(XmlDocCommentLocation.Interior);
						return;
					}
					break;
				case '*':
					if (StyleIs(XmlDocCommentStyle.Delimited))
					{
						while (TextWindow.PeekChar() == '*' && TextWindow.PeekChar(1) != '/')
						{
							TextWindow.AdvanceChar();
						}
						string internedLexemeText2 = GetInternedLexemeText();
						if (!string.IsNullOrEmpty(internedLexemeText2))
						{
							AddTrivia(SyntaxFactory.DocumentationCommentExteriorTrivia(internedLexemeText2), ref trivia);
						}
						if (TextWindow.PeekChar() == '*' && TextWindow.PeekChar(1) == '/')
						{
							TextWindow.AdvanceChar(2);
							AddTrivia(SyntaxFactory.DocumentationCommentExteriorTrivia("*/"), ref trivia);
							MutateLocation(XmlDocCommentLocation.End);
						}
						else
						{
							MutateLocation(XmlDocCommentLocation.Interior);
						}
						return;
					}
					break;
				}
				if (!SyntaxFacts.IsWhitespace(c))
				{
					break;
				}
				goto IL_00f2;
				IL_00f2:
				TextWindow.AdvanceChar();
			}
			if (StyleIs(XmlDocCommentStyle.SingleLine))
			{
				TextWindow.Reset(position);
				MutateLocation(XmlDocCommentLocation.End);
				return;
			}
			string internedLexemeText4 = GetInternedLexemeText();
			if (!string.IsNullOrEmpty(internedLexemeText4))
			{
				AddTrivia(SyntaxFactory.DocumentationCommentExteriorTrivia(internedLexemeText4), ref trivia);
			}
			MutateLocation(XmlDocCommentLocation.Interior);
		}
		else if (!LocationIs(XmlDocCommentLocation.End) && StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar() == '*' && TextWindow.PeekChar(1) == '/')
		{
			TextWindow.AdvanceChar(2);
			string internedLexemeText5 = GetInternedLexemeText();
			AddTrivia(SyntaxFactory.DocumentationCommentExteriorTrivia(internedLexemeText5), ref trivia);
			MutateLocation(XmlDocCommentLocation.End);
		}
	}

	private void LexXmlDocCommentLeadingTriviaWithWhitespace(ref SyntaxListBuilder? trivia)
	{
		while (true)
		{
			LexXmlDocCommentLeadingTrivia(ref trivia);
			char ch = TextWindow.PeekChar();
			if (LocationIs(XmlDocCommentLocation.Interior) && (SyntaxFacts.IsWhitespace(ch) || SyntaxFacts.IsNewLine(ch)))
			{
				LexXmlWhitespaceAndNewLineTrivia(ref trivia);
				continue;
			}
			break;
		}
	}

	private void LexXmlWhitespaceAndNewLineTrivia(ref SyntaxListBuilder? trivia)
	{
		Start();
		if (!LocationIs(XmlDocCommentLocation.Interior))
		{
			return;
		}
		char c = TextWindow.PeekChar();
		switch (c)
		{
		case '\t':
		case '\v':
		case '\f':
		case ' ':
			AddTrivia(ScanWhitespace(), ref trivia);
			break;
		case '\n':
		case '\r':
		{
			CSharpSyntaxNode trivia2 = ScanEndOfLine();
			AddTrivia(trivia2, ref trivia);
			MutateLocation(XmlDocCommentLocation.Exterior);
			break;
		}
		case '*':
			if (StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar(1) == '/')
			{
				break;
			}
			goto default;
		default:
			if (SyntaxFacts.IsWhitespace(c))
			{
				goto case '\t';
			}
			if (!SyntaxFacts.IsNewLine(c))
			{
				break;
			}
			goto case '\n';
		}
	}

	private bool IsUnicodeEscape()
	{
		if (TextWindow.PeekChar() == '\\')
		{
			char c = TextWindow.PeekChar(1);
			if (c == 'U' || c == 'u')
			{
				return true;
			}
		}
		return false;
	}

	private char PeekCharOrUnicodeEscape(out char surrogateCharacter)
	{
		if (IsUnicodeEscape())
		{
			return PeekUnicodeEscape(out surrogateCharacter);
		}
		surrogateCharacter = '\uffff';
		return TextWindow.PeekChar();
	}

	private char PeekUnicodeEscape(out char surrogateCharacter)
	{
		int position = TextWindow.Position;
		char result = ScanUnicodeEscape(peek: true, out surrogateCharacter, out SyntaxDiagnosticInfo _);
		TextWindow.Reset(position);
		return result;
	}

	private char NextCharOrUnicodeEscape(out char surrogateCharacter, out SyntaxDiagnosticInfo? info)
	{
		char c = TextWindow.PeekChar();
		if (c == '\\')
		{
			char c2 = TextWindow.PeekChar(1);
			if (c2 == 'U' || c2 == 'u')
			{
				return ScanUnicodeEscape(peek: false, out surrogateCharacter, out info);
			}
		}
		surrogateCharacter = '\uffff';
		info = null;
		TextWindow.AdvanceChar();
		return c;
	}

	private char NextUnicodeEscape(out char surrogateCharacter, out SyntaxDiagnosticInfo? info)
	{
		return ScanUnicodeEscape(peek: false, out surrogateCharacter, out info);
	}

	private char ScanUnicodeEscape(bool peek, out char surrogateCharacter, out SyntaxDiagnosticInfo? info)
	{
		surrogateCharacter = '\uffff';
		info = null;
		int position = TextWindow.Position;
		char c = TextWindow.PeekChar();
		TextWindow.AdvanceChar();
		c = TextWindow.PeekChar();
		if (c == 'U')
		{
			uint num = 0u;
			TextWindow.AdvanceChar();
			if (!SyntaxFacts.IsHexDigit(TextWindow.PeekChar()))
			{
				if (!peek)
				{
					info = CreateIllegalEscapeDiagnostic(position);
				}
			}
			else
			{
				for (int i = 0; i < 8; i++)
				{
					c = TextWindow.PeekChar();
					if (!SyntaxFacts.IsHexDigit(c))
					{
						if (!peek)
						{
							info = CreateIllegalEscapeDiagnostic(position);
						}
						break;
					}
					num = (uint)((num << 4) + SyntaxFacts.HexValue(c));
					TextWindow.AdvanceChar();
				}
				if (num > 1114111)
				{
					if (!peek)
					{
						info = CreateIllegalEscapeDiagnostic(position);
					}
				}
				else
				{
					c = GetCharsFromUtf32(num, out surrogateCharacter);
				}
			}
		}
		else
		{
			int num2 = 0;
			TextWindow.AdvanceChar();
			if (!SyntaxFacts.IsHexDigit(TextWindow.PeekChar()))
			{
				if (!peek)
				{
					info = CreateIllegalEscapeDiagnostic(position);
				}
			}
			else
			{
				for (int j = 0; j < 4; j++)
				{
					char c2 = TextWindow.PeekChar();
					if (!SyntaxFacts.IsHexDigit(c2))
					{
						if (c == 'u' && !peek)
						{
							info = CreateIllegalEscapeDiagnostic(position);
						}
						break;
					}
					num2 = (num2 << 4) + SyntaxFacts.HexValue(c2);
					TextWindow.AdvanceChar();
				}
				c = (char)num2;
			}
		}
		return c;
	}

	public bool TryScanXmlEntity(out char ch, out char surrogate)
	{
		ch = '&';
		TextWindow.AdvanceChar();
		surrogate = '\uffff';
		switch (TextWindow.PeekChar())
		{
		case 'l':
			if (TextWindow.AdvanceIfMatches("lt;"))
			{
				ch = '<';
				return true;
			}
			break;
		case 'g':
			if (TextWindow.AdvanceIfMatches("gt;"))
			{
				ch = '>';
				return true;
			}
			break;
		case 'a':
			if (TextWindow.AdvanceIfMatches("amp;"))
			{
				ch = '&';
				return true;
			}
			if (TextWindow.AdvanceIfMatches("apos;"))
			{
				ch = '\'';
				return true;
			}
			break;
		case 'q':
			if (TextWindow.AdvanceIfMatches("quot;"))
			{
				ch = '"';
				return true;
			}
			break;
		case '#':
		{
			TextWindow.AdvanceChar();
			uint num = 0u;
			if (TextWindow.AdvanceIfMatches("x"))
			{
				char c;
				while (SyntaxFacts.IsHexDigit(c = TextWindow.PeekChar()))
				{
					TextWindow.AdvanceChar();
					if (num <= 134217727)
					{
						num = (num << 4) + (uint)SyntaxFacts.HexValue(c);
						continue;
					}
					return false;
				}
			}
			else
			{
				char c2;
				while (SyntaxFacts.IsDecDigit(c2 = TextWindow.PeekChar()))
				{
					TextWindow.AdvanceChar();
					if (num <= 134217727)
					{
						num = (num << 3) + (num << 1) + (uint)SyntaxFacts.DecValue(c2);
						continue;
					}
					return false;
				}
			}
			if (TextWindow.AdvanceIfMatches(";"))
			{
				ch = GetCharsFromUtf32(num, out surrogate);
				return true;
			}
			break;
		}
		}
		return false;
	}

	private SyntaxDiagnosticInfo CreateIllegalEscapeDiagnostic(int start)
	{
		return new SyntaxDiagnosticInfo(start - LexemeStartPosition, TextWindow.Position - start, ErrorCode.ERR_IllegalEscape);
	}

	private static char GetCharsFromUtf32(uint codepoint, out char lowSurrogate)
	{
		if (codepoint < 65536)
		{
			lowSurrogate = '\uffff';
			return (char)codepoint;
		}
		lowSurrogate = (char)((codepoint - 65536) % 1024 + 56320);
		return (char)((codepoint - 65536) / 1024 + 55296);
	}

	private int ConsumeCharSequence(char ch)
	{
		int position = TextWindow.Position;
		while (TextWindow.PeekChar() == ch)
		{
			TextWindow.AdvanceChar();
		}
		return TextWindow.Position - position;
	}

	private int ConsumeQuoteSequence()
	{
		return ConsumeCharSequence('"');
	}

	private int ConsumeDollarSignSequence()
	{
		return ConsumeCharSequence('$');
	}

	private int ConsumeAtSignSequence()
	{
		return ConsumeCharSequence('@');
	}

	private int ConsumeOpenBraceSequence()
	{
		return ConsumeCharSequence('{');
	}

	private int ConsumeCloseBraceSequence()
	{
		return ConsumeCharSequence('}');
	}

	private void ConsumeWhitespace()
	{
		while (SyntaxFacts.IsWhitespace(TextWindow.PeekChar()))
		{
			TextWindow.AdvanceChar();
		}
	}

	private bool IsAtEndOfText(char currentChar)
	{
		if (currentChar == '\uffff')
		{
			return TextWindow.IsReallyAtEnd();
		}
		return false;
	}

	private void ScanRawStringLiteral(ref TokenInfo info, bool inDirective)
	{
		_builder.Length = 0;
		int startingQuoteCount = ConsumeQuoteSequence();
		ConsumeWhitespace();
		if (SyntaxFacts.IsNewLine(TextWindow.PeekChar()))
		{
			ScanMultiLineRawStringLiteral(ref info, startingQuoteCount);
		}
		else
		{
			ScanSingleLineRawStringLiteral(ref info, startingQuoteCount);
		}
		if (!inDirective && ScanUtf8Suffix())
		{
			switch (info.Kind)
			{
			case SyntaxKind.SingleLineRawStringLiteralToken:
				info.Kind = SyntaxKind.Utf8SingleLineRawStringLiteralToken;
				break;
			case SyntaxKind.MultiLineRawStringLiteralToken:
				info.Kind = SyntaxKind.Utf8MultiLineRawStringLiteralToken;
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(info.Kind);
			}
		}
		info.Text = GetInternedLexemeText();
	}

	private void ScanSingleLineRawStringLiteral(ref TokenInfo info, int startingQuoteCount)
	{
		info.Kind = SyntaxKind.SingleLineRawStringLiteralToken;
		while (true)
		{
			char c = TextWindow.PeekChar();
			if (SyntaxFacts.IsNewLine(c) || IsAtEndOfText(c))
			{
				break;
			}
			if (c != '"')
			{
				TextWindow.AdvanceChar();
			}
			else if (ConsumeQuoteSequence() >= startingQuoteCount)
			{
				break;
			}
		}
	}

	private void ScanMultiLineRawStringLiteral(ref TokenInfo info, int startingQuoteCount)
	{
		info.Kind = SyntaxKind.MultiLineRawStringLiteralToken;
		while (scanMultiLineRawStringLiteralLine(startingQuoteCount))
		{
		}
		bool scanMultiLineRawStringLiteralLine(int num)
		{
			TextWindow.AdvancePastNewLine();
			ConsumeWhitespace();
			if (ConsumeQuoteSequence() >= num)
			{
				return false;
			}
			while (true)
			{
				char c = TextWindow.PeekChar();
				if (IsAtEndOfText(c))
				{
					return false;
				}
				if (SyntaxFacts.IsNewLine(c))
				{
					return true;
				}
				if (c == '"')
				{
					if (ConsumeQuoteSequence() >= num)
					{
						break;
					}
				}
				else
				{
					TextWindow.AdvanceChar();
				}
			}
			return false;
		}
	}

	private void ScanStringLiteral(ref TokenInfo info, bool inDirective)
	{
		char c = TextWindow.PeekChar();
		if (TextWindow.PeekChar() == '"' && TextWindow.PeekChar(1) == '"' && TextWindow.PeekChar(2) == '"')
		{
			ScanRawStringLiteral(ref info, inDirective);
			if (inDirective)
			{
				info.Kind = SyntaxKind.StringLiteralToken;
				info.StringValue = "";
				AddError(ErrorCode.ERR_RawStringNotInDirectives);
			}
			return;
		}
		TextWindow.AdvanceChar();
		_builder.Length = 0;
		while (true)
		{
			char c2 = TextWindow.PeekChar();
			if (c2 == '\\' && !inDirective)
			{
				c2 = ScanEscapeSequence(out var surrogateCharacter);
				_builder.Append(c2);
				if (surrogateCharacter != '\uffff')
				{
					_builder.Append(surrogateCharacter);
				}
				continue;
			}
			if (c2 == c)
			{
				TextWindow.AdvanceChar();
				break;
			}
			if (SyntaxFacts.IsNewLine(c2) || (c2 == '\uffff' && TextWindow.IsReallyAtEnd()))
			{
				AddError(ErrorCode.ERR_NewlineInConst);
				break;
			}
			TextWindow.AdvanceChar();
			_builder.Append(c2);
		}
		if (c == '\'')
		{
			info.Text = GetInternedLexemeText();
			info.Kind = SyntaxKind.CharacterLiteralToken;
			if (_builder.Length != 1)
			{
				AddError((_builder.Length != 0) ? ErrorCode.ERR_TooManyCharsInConst : ErrorCode.ERR_EmptyCharConst);
			}
			if (_builder.Length > 0)
			{
				info.StringValue = TextWindow.Intern(_builder);
				info.CharValue = info.StringValue[0];
			}
			else
			{
				info.StringValue = string.Empty;
				info.CharValue = '\uffff';
			}
		}
		else
		{
			if (!inDirective && ScanUtf8Suffix())
			{
				info.Kind = SyntaxKind.Utf8StringLiteralToken;
			}
			else
			{
				info.Kind = SyntaxKind.StringLiteralToken;
			}
			info.Text = GetInternedLexemeText();
			if (_builder.Length > 0)
			{
				info.StringValue = TextWindow.Intern(_builder);
			}
			else
			{
				info.StringValue = string.Empty;
			}
		}
	}

	private bool ScanUtf8Suffix()
	{
		char c = TextWindow.PeekChar();
		bool flag = ((c == 'U' || c == 'u') ? true : false);
		if (flag && TextWindow.PeekChar(1) == '8')
		{
			TextWindow.AdvanceChar(2);
			return true;
		}
		return false;
	}

	private char ScanEscapeSequence(out char surrogateCharacter)
	{
		int position = TextWindow.Position;
		surrogateCharacter = '\uffff';
		char c = TextWindow.NextChar();
		c = TextWindow.NextChar();
		switch (c)
		{
		case '0':
			c = '\0';
			break;
		case 'a':
			c = '\a';
			break;
		case 'b':
			c = '\b';
			break;
		case 'e':
		{
			CSDiagnosticInfo featureAvailabilityDiagnosticInfo = MessageID.IDS_FeatureStringEscapeCharacter.GetFeatureAvailabilityDiagnosticInfo(Options);
			if (featureAvailabilityDiagnosticInfo != null)
			{
				AddError(position, TextWindow.Position - position, featureAvailabilityDiagnosticInfo.Code, featureAvailabilityDiagnosticInfo.Arguments);
			}
			c = '\u001b';
			break;
		}
		case 'f':
			c = '\f';
			break;
		case 'n':
			c = '\n';
			break;
		case 'r':
			c = '\r';
			break;
		case 't':
			c = '\t';
			break;
		case 'v':
			c = '\v';
			break;
		case 'U':
		case 'u':
		case 'x':
		{
			TextWindow.Reset(position);
			c = NextUnicodeEscape(out surrogateCharacter, out SyntaxDiagnosticInfo info);
			AddError(info);
			break;
		}
		default:
			AddError(position, TextWindow.Position - position, ErrorCode.ERR_IllegalEscape);
			break;
		case '"':
		case '\'':
		case '\\':
			break;
		}
		return c;
	}

	private void ScanVerbatimStringLiteral(ref TokenInfo info)
	{
		_builder.Length = 0;
		int position = TextWindow.Position;
		while (TextWindow.PeekChar() == '@')
		{
			TextWindow.AdvanceChar();
		}
		if (TextWindow.Position - position >= 2)
		{
			AddError(position, TextWindow.Position - position, ErrorCode.ERR_IllegalAtSequence);
		}
		TextWindow.AdvanceChar();
		while (true)
		{
			char c = TextWindow.PeekChar();
			if (c == '"')
			{
				TextWindow.AdvanceChar();
				if (TextWindow.PeekChar() != '"')
				{
					break;
				}
				TextWindow.AdvanceChar();
				_builder.Append(c);
			}
			else
			{
				if (c == '\uffff' && TextWindow.IsReallyAtEnd())
				{
					AddError(ErrorCode.ERR_UnterminatedStringLit);
					break;
				}
				TextWindow.AdvanceChar();
				_builder.Append(c);
			}
		}
		if (ScanUtf8Suffix())
		{
			info.Kind = SyntaxKind.Utf8StringLiteralToken;
		}
		else
		{
			info.Kind = SyntaxKind.StringLiteralToken;
		}
		info.Text = GetNonInternedLexemeText();
		info.StringValue = _builder.ToString();
	}

	private void ScanInterpolatedStringLiteral(ref TokenInfo info)
	{
		ScanInterpolatedOrRawStringLiteralTop(ref info, isInterpolatedString: true, out SyntaxDiagnosticInfo error, out InterpolatedStringKind _, out Range _, null, out Range _);
		AddError(error);
	}

	internal void ScanInterpolatedOrRawStringLiteralTop(ref TokenInfo info, bool isInterpolatedString, out SyntaxDiagnosticInfo? error, out InterpolatedStringKind kind, out Range openQuoteRange, ArrayBuilder<Interpolation>? interpolations, out Range closeQuoteRange)
	{
		InterpolatedOrRawStringScanner interpolatedOrRawStringScanner = new InterpolatedOrRawStringScanner(this, isInterpolatedString);
		interpolatedOrRawStringScanner.ScanStringLiteralTop(out kind, out openQuoteRange, interpolations, out closeQuoteRange);
		error = interpolatedOrRawStringScanner.Error;
		info.Kind = SyntaxKind.InterpolatedStringToken;
		info.Text = GetNonInternedLexemeText();
	}

	internal static SyntaxToken RescanInterpolatedString(InterpolatedStringExpressionSyntax interpolatedString)
	{
		string text = interpolatedString.ToString();
		SyntaxKind kind = SyntaxKind.InterpolatedStringToken;
		return SyntaxFactory.Literal(interpolatedString.GetFirstToken().GetLeadingTrivia(), text, kind, text, interpolatedString.GetLastToken().GetTrailingTrivia());
	}

	private SyntaxToken? QuickScanSyntaxToken()
	{
		Start();
		QuickScanState quickScanState = QuickScanState.Initial;
		ReadOnlySpan<char> readOnlySpan = TextWindow.CurrentWindowSpan;
		readOnlySpan = readOnlySpan.Slice(0, Math.Min(42, readOnlySpan.Length));
		int num = -2128831035;
		int length = CharProperties.Length;
		int num2 = 0;
		while (true)
		{
			if (num2 < readOnlySpan.Length)
			{
				int num3 = readOnlySpan[num2];
				CharFlags charFlags = (CharFlags)((num3 < length) ? CharProperties[num3] : 9);
				quickScanState = (QuickScanState)s_stateTransitions[(uint)quickScanState, (uint)charFlags];
				if ((int)quickScanState >= 9)
				{
					break;
				}
				num = (num ^ num3) * 16777619;
				num2++;
				continue;
			}
			quickScanState = QuickScanState.Bad;
			break;
		}
		if (quickScanState == QuickScanState.Done)
		{
			int num4 = num2;
			TextWindow.AdvanceChar(num4);
			return _cache.LookupToken(readOnlySpan.Slice(0, num4), num, CreateQuickToken, this);
		}
		return null;
	}

	private static SyntaxToken CreateQuickToken(Lexer lexer)
	{
		lexer.TextWindow.Reset(lexer.LexemeStartPosition);
		return lexer.LexSyntaxToken();
	}
}
