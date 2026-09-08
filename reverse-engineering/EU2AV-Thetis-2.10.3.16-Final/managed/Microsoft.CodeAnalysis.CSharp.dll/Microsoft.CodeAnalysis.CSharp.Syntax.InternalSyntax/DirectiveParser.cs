using System;
using System.Globalization;
using System.IO;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class DirectiveParser : SyntaxParser
{
	private const int MAX_DIRECTIVE_IDENTIFIER_WIDTH = 128;

	private DirectiveStack _context;

	private const int MaxLineValue = 16707565;

	private const int MaxCharacterValue = 65536;

	internal DirectiveParser(Lexer lexer)
		: base(lexer, LexerMode.Directive, null, null, allowModeReset: false)
	{
	}

	public void ReInitialize(DirectiveStack context)
	{
		ReInitialize();
		_context = context;
	}

	public CSharpSyntaxNode ParseDirective(bool isActive, bool endIsActive, bool isFollowingToken)
	{
		int position = lexer.TextWindow.Position;
		SyntaxToken syntaxToken = EatToken(SyntaxKind.HashToken, reportError: false);
		SyntaxKind contextualKind = base.CurrentToken.ContextualKind;
		switch (contextualKind)
		{
		case SyntaxKind.IfKeyword:
			return ParseIfDirective(syntaxToken, EatContextualToken(contextualKind), isActive);
		case SyntaxKind.ElifKeyword:
			return ParseElifDirective(syntaxToken, EatContextualToken(contextualKind), isActive, endIsActive);
		case SyntaxKind.ElseKeyword:
			return ParseElseDirective(syntaxToken, EatContextualToken(contextualKind), isActive, endIsActive);
		case SyntaxKind.EndIfKeyword:
			return ParseEndIfDirective(syntaxToken, EatContextualToken(contextualKind), isActive, endIsActive);
		case SyntaxKind.RegionKeyword:
			return ParseRegionDirective(syntaxToken, EatContextualToken(contextualKind), isActive);
		case SyntaxKind.EndRegionKeyword:
			return ParseEndRegionDirective(syntaxToken, EatContextualToken(contextualKind), isActive);
		case SyntaxKind.DefineKeyword:
		case SyntaxKind.UndefKeyword:
			return ParseDefineOrUndefDirective(syntaxToken, EatContextualToken(contextualKind), isActive, isFollowingToken);
		case SyntaxKind.WarningKeyword:
		case SyntaxKind.ErrorKeyword:
			return ParseErrorOrWarningDirective(syntaxToken, EatContextualToken(contextualKind), isActive);
		case SyntaxKind.LineKeyword:
		{
			SyntaxToken syntaxToken2 = EatContextualToken(contextualKind);
			return (base.CurrentToken.Kind == SyntaxKind.OpenParenToken) ? ParseLineSpanDirective(syntaxToken, syntaxToken2, isActive) : ParseLineDirective(syntaxToken, syntaxToken2, isActive);
		}
		case SyntaxKind.PragmaKeyword:
			return ParsePragmaDirective(syntaxToken, EatContextualToken(contextualKind), isActive);
		case SyntaxKind.ReferenceKeyword:
			return ParseReferenceDirective(syntaxToken, EatContextualToken(contextualKind), isActive, isFollowingToken);
		case SyntaxKind.LoadKeyword:
			return ParseLoadDirective(syntaxToken, EatContextualToken(contextualKind), isActive, isFollowingToken);
		case SyntaxKind.NullableKeyword:
			return ParseNullableDirective(syntaxToken, EatContextualToken(contextualKind), isActive);
		case SyntaxKind.ExclamationToken:
			if (position != 0 || syntaxToken.HasTrailingTrivia)
			{
				syntaxToken = AddError(syntaxToken, ErrorCode.ERR_BadDirectivePlacement);
			}
			return ParseShebangDirective(syntaxToken, EatToken(SyntaxKind.ExclamationToken), isActive);
		case SyntaxKind.ColonToken:
			if (!syntaxToken.HasTrailingTrivia)
			{
				return ParseIgnoredDirective(syntaxToken, EatToken(SyntaxKind.ColonToken), isActive, isFollowingToken);
			}
			break;
		}
		SyntaxToken syntaxToken3 = EatToken(SyntaxKind.IdentifierToken, reportError: false);
		SyntaxToken endOfDirectiveToken = ParseEndOfDirective(ignoreErrors: true);
		if (!syntaxToken3.IsMissing)
		{
			syntaxToken3 = AddError(syntaxToken3, ErrorCode.ERR_PPDirectiveExpected);
		}
		else
		{
			syntaxToken = AddError(syntaxToken, ErrorCode.ERR_PPDirectiveExpected);
		}
		return SyntaxFactory.BadDirectiveTrivia(syntaxToken, syntaxToken3, endOfDirectiveToken, isActive);
	}

	private DirectiveTriviaSyntax ParseIfDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive)
	{
		ExpressionSyntax expressionSyntax = ParseExpression();
		SyntaxToken endOfDirectiveToken = ParseEndOfDirective(ignoreErrors: false);
		bool flag = EvaluateBool(expressionSyntax);
		bool branchTaken = isActive & flag;
		return SyntaxFactory.IfDirectiveTrivia(hash, keyword, expressionSyntax, endOfDirectiveToken, isActive, branchTaken, flag);
	}

	private DirectiveTriviaSyntax ParseElifDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive, bool endIsActive)
	{
		ExpressionSyntax expressionSyntax = ParseExpression();
		SyntaxToken syntaxToken = ParseEndOfDirective(ignoreErrors: false);
		if (_context.HasPreviousIfOrElif())
		{
			bool flag = EvaluateBool(expressionSyntax);
			bool branchTaken = (endIsActive & flag) && !_context.PreviousBranchTaken();
			return SyntaxFactory.ElifDirectiveTrivia(hash, keyword, expressionSyntax, syntaxToken, endIsActive, branchTaken, flag);
		}
		syntaxToken = syntaxToken.TokenWithLeadingTrivia(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.Concat(SyntaxFactory.DisabledText(expressionSyntax.ToFullString()), syntaxToken.GetLeadingTrivia()));
		if (_context.HasUnfinishedRegion())
		{
			return AddError(SyntaxFactory.BadDirectiveTrivia(hash, keyword, syntaxToken, isActive), ErrorCode.ERR_EndRegionDirectiveExpected);
		}
		if (_context.HasUnfinishedIf())
		{
			return AddError(SyntaxFactory.BadDirectiveTrivia(hash, keyword, syntaxToken, isActive), ErrorCode.ERR_EndifDirectiveExpected);
		}
		return AddError(SyntaxFactory.BadDirectiveTrivia(hash, keyword, syntaxToken, isActive), ErrorCode.ERR_UnexpectedDirective);
	}

	private DirectiveTriviaSyntax ParseElseDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive, bool endIsActive)
	{
		SyntaxToken endOfDirectiveToken = ParseEndOfDirective(ignoreErrors: false);
		if (_context.HasPreviousIfOrElif())
		{
			bool branchTaken = endIsActive && !_context.PreviousBranchTaken();
			return SyntaxFactory.ElseDirectiveTrivia(hash, keyword, endOfDirectiveToken, endIsActive, branchTaken);
		}
		if (_context.HasUnfinishedRegion())
		{
			return AddError(SyntaxFactory.BadDirectiveTrivia(hash, keyword, endOfDirectiveToken, isActive), ErrorCode.ERR_EndRegionDirectiveExpected);
		}
		if (_context.HasUnfinishedIf())
		{
			return AddError(SyntaxFactory.BadDirectiveTrivia(hash, keyword, endOfDirectiveToken, isActive), ErrorCode.ERR_EndifDirectiveExpected);
		}
		return AddError(SyntaxFactory.BadDirectiveTrivia(hash, keyword, endOfDirectiveToken, isActive), ErrorCode.ERR_UnexpectedDirective);
	}

	private DirectiveTriviaSyntax ParseEndIfDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive, bool endIsActive)
	{
		SyntaxToken endOfDirectiveToken = ParseEndOfDirective(ignoreErrors: false);
		if (_context.HasUnfinishedIf())
		{
			return SyntaxFactory.EndIfDirectiveTrivia(hash, keyword, endOfDirectiveToken, endIsActive);
		}
		if (_context.HasUnfinishedRegion())
		{
			return AddError(SyntaxFactory.BadDirectiveTrivia(hash, keyword, endOfDirectiveToken, isActive), ErrorCode.ERR_EndRegionDirectiveExpected);
		}
		return AddError(SyntaxFactory.BadDirectiveTrivia(hash, keyword, endOfDirectiveToken, isActive), ErrorCode.ERR_UnexpectedDirective);
	}

	private DirectiveTriviaSyntax ParseRegionDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive)
	{
		return SyntaxFactory.RegionDirectiveTrivia(hash, keyword, ParseEndOfDirectiveWithOptionalPreprocessingMessage(), isActive);
	}

	private DirectiveTriviaSyntax ParseEndRegionDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive)
	{
		SyntaxToken endOfDirectiveToken = ParseEndOfDirectiveWithOptionalPreprocessingMessage();
		if (_context.HasUnfinishedRegion())
		{
			return SyntaxFactory.EndRegionDirectiveTrivia(hash, keyword, endOfDirectiveToken, isActive);
		}
		if (_context.HasUnfinishedIf())
		{
			return AddError(SyntaxFactory.BadDirectiveTrivia(hash, keyword, endOfDirectiveToken, isActive), ErrorCode.ERR_EndifDirectiveExpected);
		}
		return AddError(SyntaxFactory.BadDirectiveTrivia(hash, keyword, endOfDirectiveToken, isActive), ErrorCode.ERR_UnexpectedDirective);
	}

	private DirectiveTriviaSyntax ParseDefineOrUndefDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive, bool isFollowingToken)
	{
		if (isFollowingToken)
		{
			keyword = AddError(keyword, ErrorCode.ERR_PPDefFollowsToken);
		}
		SyntaxToken identifier = EatToken(SyntaxKind.IdentifierToken, ErrorCode.ERR_IdentifierExpected);
		identifier = TruncateIdentifier(identifier);
		SyntaxToken endOfDirectiveToken = ParseEndOfDirective(identifier.IsMissing);
		if (keyword.Kind == SyntaxKind.DefineKeyword)
		{
			return SyntaxFactory.DefineDirectiveTrivia(hash, keyword, identifier, endOfDirectiveToken, isActive);
		}
		return SyntaxFactory.UndefDirectiveTrivia(hash, keyword, identifier, endOfDirectiveToken, isActive);
	}

	private DirectiveTriviaSyntax ParseErrorOrWarningDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive)
	{
		SyntaxToken syntaxToken = ParseEndOfDirectiveWithOptionalPreprocessingMessage();
		bool flag = keyword.Kind == SyntaxKind.ErrorKeyword;
		if (isActive)
		{
			StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			int num = 0;
			bool flag2 = true;
			foreach (CSharpSyntaxNode trailingTrivium in keyword.TrailingTrivia)
			{
				if (flag2)
				{
					if (trailingTrivium.Kind == SyntaxKind.WhitespaceTrivia)
					{
						continue;
					}
					flag2 = false;
				}
				trailingTrivium.WriteTo(stringWriter, leading: true, trailing: true);
				num += trailingTrivium.FullWidth;
			}
			foreach (CSharpSyntaxNode leadingTrivium in syntaxToken.LeadingTrivia)
			{
				leadingTrivium.WriteTo(stringWriter, leading: true, trailing: true);
				num += leadingTrivium.FullWidth;
			}
			int offset = -num;
			string text = stringWriter.ToString();
			syntaxToken = AddError(syntaxToken, offset, num, flag ? ErrorCode.ERR_ErrorDirective : ErrorCode.WRN_WarningDirective, text);
			if (flag)
			{
				LanguageVersion result;
				if (text.Equals("version", StringComparison.Ordinal))
				{
					string productVersion = CommonCompiler.GetProductVersion(typeof(CSharpCompiler));
					string assemblyLocation = CommonCompiler.GetAssemblyLocation(typeof(CSharpCompiler));
					LanguageVersion specifiedLanguageVersion = base.Options.SpecifiedLanguageVersion;
					LanguageVersion languageVersion = specifiedLanguageVersion.MapSpecifiedToEffectiveVersion();
					string text2 = ((specifiedLanguageVersion == languageVersion) ? specifiedLanguageVersion.ToDisplayString() : (specifiedLanguageVersion.ToDisplayString() + " (" + languageVersion.ToDisplayString() + ")"));
					syntaxToken = AddError(syntaxToken, offset, num, ErrorCode.ERR_CompilerAndLanguageVersion, productVersion, text2, assemblyLocation);
				}
				else if (base.Options.LanguageVersion != LanguageVersion.Preview && text.StartsWith("version:", StringComparison.Ordinal) && LanguageVersionFacts.TryParse(text.Substring("version:".Length), out result))
				{
					ErrorCode errorCode = base.Options.LanguageVersion.GetErrorCode();
					syntaxToken = AddError(syntaxToken, offset, num, errorCode, "version", new CSharpRequiredLanguageVersion(result));
				}
			}
		}
		if (flag)
		{
			return SyntaxFactory.ErrorDirectiveTrivia(hash, keyword, syntaxToken, isActive);
		}
		return SyntaxFactory.WarningDirectiveTrivia(hash, keyword, syntaxToken, isActive);
	}

	private DirectiveTriviaSyntax ParseLineDirective(SyntaxToken hash, SyntaxToken id, bool isActive)
	{
		SyntaxToken file = null;
		bool afterLineNumber = false;
		SyntaxKind kind = base.CurrentToken.Kind;
		SyntaxToken syntaxToken;
		if (kind == SyntaxKind.DefaultKeyword || kind == SyntaxKind.HiddenKeyword)
		{
			syntaxToken = EatToken();
		}
		else
		{
			syntaxToken = EatToken(SyntaxKind.NumericLiteralToken, ErrorCode.ERR_InvalidLineNumber, isActive);
			afterLineNumber = true;
			if (isActive && !syntaxToken.IsMissing && syntaxToken.Kind == SyntaxKind.NumericLiteralToken)
			{
				if ((int)syntaxToken.Value < 1)
				{
					syntaxToken = AddError(syntaxToken, ErrorCode.ERR_InvalidLineNumber);
				}
				else if ((int)syntaxToken.Value > 16707565)
				{
					syntaxToken = AddError(syntaxToken, ErrorCode.WRN_TooManyLinesForDebugger);
				}
			}
			if (base.CurrentToken.Kind == SyntaxKind.StringLiteralToken && (syntaxToken.IsMissing || syntaxToken.GetTrailingTriviaWidth() > 0 || base.CurrentToken.GetLeadingTriviaWidth() > 0))
			{
				file = EatToken();
				afterLineNumber = false;
			}
		}
		SyntaxToken endOfDirectiveToken = ParseEndOfDirective(syntaxToken.IsMissing || !isActive, afterPragma: false, afterLineNumber);
		return SyntaxFactory.LineDirectiveTrivia(hash, id, syntaxToken, file, endOfDirectiveToken, isActive);
	}

	private LineSpanDirectiveTriviaSyntax ParseLineSpanDirective(SyntaxToken hash, SyntaxToken lineKeyword, bool isActive)
	{
		bool reportError = isActive;
		LineDirectivePositionSyntax lineDirectivePositionSyntax = ParseLineDirectivePosition(ref reportError, out var line, out var character);
		if (noTriviaBetween(lineKeyword, lineDirectivePositionSyntax.GetFirstToken()))
		{
			lineDirectivePositionSyntax = AddError(lineDirectivePositionSyntax, ErrorCode.ERR_LineSpanDirectiveRequiresSpace);
		}
		SyntaxToken syntaxToken = EatToken(SyntaxKind.MinusToken, reportError);
		if (syntaxToken.IsMissing)
		{
			reportError = false;
		}
		LineDirectivePositionSyntax lineDirectivePositionSyntax2 = ParseLineDirectivePosition(ref reportError, out var line2, out var character2);
		if (reportError && (line2 < line || (line2 == line && character2 < character)))
		{
			lineDirectivePositionSyntax2 = AddError(lineDirectivePositionSyntax2, ErrorCode.ERR_LineSpanDirectiveEndLessThanStart);
		}
		SyntaxToken syntaxToken2 = ((base.CurrentToken.Kind == SyntaxKind.NumericLiteralToken) ? ParseLineDirectiveNumericLiteral(ref reportError, 1, 65536, out var _) : null);
		if (noTriviaBetween(lineDirectivePositionSyntax2.GetLastToken(), syntaxToken2))
		{
			syntaxToken2 = AddError(syntaxToken2, ErrorCode.ERR_LineSpanDirectiveRequiresSpace);
		}
		SyntaxToken syntaxToken3 = EatToken(SyntaxKind.StringLiteralToken, ErrorCode.ERR_MissingPPFile, reportError);
		if (syntaxToken3.IsMissing)
		{
			reportError = false;
		}
		if (noTriviaBetween(syntaxToken2 ?? lineDirectivePositionSyntax2.GetLastToken(), syntaxToken3))
		{
			syntaxToken3 = AddError(syntaxToken3, ErrorCode.ERR_LineSpanDirectiveRequiresSpace);
		}
		SyntaxToken endOfDirectiveToken = ParseEndOfDirective(!reportError);
		return SyntaxFactory.LineSpanDirectiveTrivia(hash, lineKeyword, lineDirectivePositionSyntax, syntaxToken, lineDirectivePositionSyntax2, syntaxToken2, syntaxToken3, endOfDirectiveToken, isActive);
		static bool noTriviaBetween(SyntaxToken token1, SyntaxToken token2)
		{
			if (token1 != null && !token1.IsMissing && token2 != null && !token2.IsMissing)
			{
				return LanguageParser.NoTriviaBetween(token1, token2);
			}
			return false;
		}
	}

	private LineDirectivePositionSyntax ParseLineDirectivePosition(ref bool reportError, out int line, out int character)
	{
		SyntaxToken syntaxToken = EatToken(SyntaxKind.OpenParenToken, reportError);
		if (syntaxToken.IsMissing)
		{
			reportError = false;
		}
		SyntaxToken line2 = ParseLineDirectiveNumericLiteral(ref reportError, 1, 16707565, out line);
		SyntaxToken syntaxToken2 = EatToken(SyntaxKind.CommaToken, reportError);
		if (syntaxToken2.IsMissing)
		{
			reportError = false;
		}
		SyntaxToken character2 = ParseLineDirectiveNumericLiteral(ref reportError, 1, 65536, out character);
		SyntaxToken syntaxToken3 = EatToken(SyntaxKind.CloseParenToken, reportError);
		if (syntaxToken3.IsMissing)
		{
			reportError = false;
		}
		return SyntaxFactory.LineDirectivePosition(syntaxToken, line2, syntaxToken2, character2, syntaxToken3);
	}

	private SyntaxToken ParseLineDirectiveNumericLiteral(ref bool reportError, int minValue, int maxValue, out int value)
	{
		SyntaxToken syntaxToken = EatToken(SyntaxKind.NumericLiteralToken, ErrorCode.ERR_LineSpanDirectiveInvalidValue, reportError);
		value = 0;
		if (syntaxToken.IsMissing)
		{
			reportError = false;
		}
		else if (syntaxToken.Kind == SyntaxKind.NumericLiteralToken)
		{
			value = (int)syntaxToken.Value;
			if (value < minValue || value > maxValue)
			{
				syntaxToken = AddError(syntaxToken, ErrorCode.ERR_LineSpanDirectiveInvalidValue);
				reportError = false;
			}
		}
		return syntaxToken;
	}

	private DirectiveTriviaSyntax ParseReferenceDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive, bool isFollowingToken)
	{
		if (isActive)
		{
			if (base.Options.Kind == SourceCodeKind.Regular)
			{
				keyword = AddError(keyword, ErrorCode.ERR_ReferenceDirectiveOnlyAllowedInScripts);
			}
			else if (isFollowingToken)
			{
				keyword = AddError(keyword, ErrorCode.ERR_PPReferenceFollowsToken);
			}
		}
		SyntaxToken syntaxToken = EatToken(SyntaxKind.StringLiteralToken, ErrorCode.ERR_ExpectedPPFile, isActive);
		SyntaxToken endOfDirectiveToken = ParseEndOfDirective(syntaxToken.IsMissing || !isActive);
		return SyntaxFactory.ReferenceDirectiveTrivia(hash, keyword, syntaxToken, endOfDirectiveToken, isActive);
	}

	private DirectiveTriviaSyntax ParseLoadDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive, bool isFollowingToken)
	{
		if (isActive)
		{
			if (base.Options.Kind == SourceCodeKind.Regular)
			{
				keyword = AddError(keyword, ErrorCode.ERR_LoadDirectiveOnlyAllowedInScripts);
			}
			else if (isFollowingToken)
			{
				keyword = AddError(keyword, ErrorCode.ERR_PPLoadFollowsToken);
			}
		}
		SyntaxToken syntaxToken = EatToken(SyntaxKind.StringLiteralToken, ErrorCode.ERR_ExpectedPPFile, isActive);
		SyntaxToken endOfDirectiveToken = ParseEndOfDirective(syntaxToken.IsMissing || !isActive);
		return SyntaxFactory.LoadDirectiveTrivia(hash, keyword, syntaxToken, endOfDirectiveToken, isActive);
	}

	private DirectiveTriviaSyntax ParseNullableDirective(SyntaxToken hash, SyntaxToken token, bool isActive)
	{
		if (isActive)
		{
			token = CheckFeatureAvailability(token, MessageID.IDS_FeatureNullableReferenceTypes);
		}
		SyntaxToken syntaxToken = base.CurrentToken.Kind switch
		{
			SyntaxKind.EnableKeyword => EatToken(), 
			SyntaxKind.DisableKeyword => EatToken(), 
			SyntaxKind.RestoreKeyword => EatToken(), 
			_ => EatToken(SyntaxKind.DisableKeyword, ErrorCode.ERR_NullableDirectiveQualifierExpected, isActive), 
		};
		SyntaxToken syntaxToken2 = base.CurrentToken.Kind switch
		{
			SyntaxKind.WarningsKeyword => EatToken(), 
			SyntaxKind.AnnotationsKeyword => EatToken(), 
			SyntaxKind.EndOfDirectiveToken => null, 
			SyntaxKind.EndOfFileToken => null, 
			_ => EatToken(SyntaxKind.WarningsKeyword, ErrorCode.ERR_NullableDirectiveTargetExpected, !syntaxToken.IsMissing & isActive), 
		};
		SyntaxToken endOfDirectiveToken = ParseEndOfDirective(syntaxToken.IsMissing || (syntaxToken2 != null && syntaxToken2.IsMissing) || !isActive);
		return SyntaxFactory.NullableDirectiveTrivia(hash, token, syntaxToken, syntaxToken2, endOfDirectiveToken, isActive);
	}

	private DirectiveTriviaSyntax ParsePragmaDirective(SyntaxToken hash, SyntaxToken pragma, bool isActive)
	{
		if (isActive)
		{
			pragma = CheckFeatureAvailability(pragma, MessageID.IDS_FeaturePragma);
		}
		bool flag = false;
		if (base.CurrentToken.ContextualKind == SyntaxKind.WarningKeyword)
		{
			SyntaxToken warningKeyword = EatContextualToken(SyntaxKind.WarningKeyword);
			SyntaxToken disableOrRestoreKeyword;
			if (base.CurrentToken.Kind == SyntaxKind.DisableKeyword || base.CurrentToken.Kind == SyntaxKind.RestoreKeyword)
			{
				disableOrRestoreKeyword = EatToken();
				SeparatedSyntaxListBuilder<ExpressionSyntax> separatedSyntaxListBuilder = new SeparatedSyntaxListBuilder<ExpressionSyntax>(10);
				while (base.CurrentToken.Kind != SyntaxKind.EndOfDirectiveToken)
				{
					SyntaxToken syntaxToken;
					ExpressionSyntax node;
					if (base.CurrentToken.Kind == SyntaxKind.NumericLiteralToken)
					{
						syntaxToken = EatToken();
						node = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, syntaxToken);
					}
					else if (base.CurrentToken.Kind == SyntaxKind.IdentifierToken)
					{
						syntaxToken = EatToken();
						node = SyntaxFactory.IdentifierName(syntaxToken);
					}
					else
					{
						syntaxToken = EatToken(SyntaxKind.NumericLiteralToken, ErrorCode.WRN_IdentifierOrNumericLiteralExpected, isActive);
						node = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, syntaxToken);
					}
					flag = flag || syntaxToken.ContainsDiagnostics;
					separatedSyntaxListBuilder.Add(node);
					if (base.CurrentToken.Kind != SyntaxKind.CommaToken)
					{
						break;
					}
					separatedSyntaxListBuilder.AddSeparator(EatToken());
				}
				SyntaxToken endOfDirectiveToken = ParseEndOfDirective(flag || !isActive, afterPragma: true);
				return SyntaxFactory.PragmaWarningDirectiveTrivia(hash, pragma, warningKeyword, disableOrRestoreKeyword, separatedSyntaxListBuilder.ToList(), endOfDirectiveToken, isActive);
			}
			disableOrRestoreKeyword = EatToken(SyntaxKind.DisableKeyword, ErrorCode.WRN_IllegalPPWarning, isActive);
			SyntaxToken endOfDirectiveToken2 = ParseEndOfDirective(ignoreErrors: true, afterPragma: true);
			return SyntaxFactory.PragmaWarningDirectiveTrivia(hash, pragma, warningKeyword, disableOrRestoreKeyword, default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax>), endOfDirectiveToken2, isActive);
		}
		if (base.CurrentToken.Kind == SyntaxKind.ChecksumKeyword)
		{
			SyntaxToken checksumKeyword = EatToken();
			SyntaxToken syntaxToken2 = EatToken(SyntaxKind.StringLiteralToken, ErrorCode.WRN_IllegalPPChecksum, isActive);
			SyntaxToken syntaxToken3 = EatToken(SyntaxKind.StringLiteralToken, ErrorCode.WRN_IllegalPPChecksum, isActive && !syntaxToken2.IsMissing);
			if (isActive && !syntaxToken3.IsMissing && !Guid.TryParse(syntaxToken3.ValueText, out var _))
			{
				syntaxToken3 = AddError(syntaxToken3, ErrorCode.WRN_IllegalPPChecksum);
			}
			SyntaxToken syntaxToken4 = EatToken(SyntaxKind.StringLiteralToken, ErrorCode.WRN_IllegalPPChecksum, isActive && !syntaxToken3.IsMissing);
			if (isActive && !syntaxToken4.IsMissing)
			{
				if (syntaxToken4.ValueText.Length % 2 != 0)
				{
					syntaxToken4 = AddError(syntaxToken4, ErrorCode.WRN_IllegalPPChecksum);
				}
				else
				{
					string valueText = syntaxToken4.ValueText;
					for (int i = 0; i < valueText.Length; i++)
					{
						if (!SyntaxFacts.IsHexDigit(valueText[i]))
						{
							syntaxToken4 = AddError(syntaxToken4, ErrorCode.WRN_IllegalPPChecksum);
							break;
						}
					}
				}
			}
			flag = syntaxToken2.ContainsDiagnostics | syntaxToken3.ContainsDiagnostics | syntaxToken4.ContainsDiagnostics;
			SyntaxToken endOfDirectiveToken3 = ParseEndOfDirective(flag, afterPragma: true);
			return SyntaxFactory.PragmaChecksumDirectiveTrivia(hash, pragma, checksumKeyword, syntaxToken2, syntaxToken3, syntaxToken4, endOfDirectiveToken3, isActive);
		}
		SyntaxToken warningKeyword2 = EatToken(SyntaxKind.WarningKeyword, ErrorCode.WRN_IllegalPragma, isActive);
		SyntaxToken disableOrRestoreKeyword2 = EatToken(SyntaxKind.DisableKeyword, reportError: false);
		SyntaxToken endOfDirectiveToken4 = ParseEndOfDirective(ignoreErrors: true, afterPragma: true);
		return SyntaxFactory.PragmaWarningDirectiveTrivia(hash, pragma, warningKeyword2, disableOrRestoreKeyword2, default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax>), endOfDirectiveToken4, isActive);
	}

	private DirectiveTriviaSyntax ParseShebangDirective(SyntaxToken hash, SyntaxToken exclamation, bool isActive)
	{
		if (lexer.Options.Kind != SourceCodeKind.Script && !lexer.Options.FileBasedProgram)
		{
			exclamation = AddError(exclamation, ErrorCode.ERR_PPShebangInProjectBasedProgram);
		}
		return SyntaxFactory.ShebangDirectiveTrivia(hash, exclamation, ParseEndOfDirectiveWithOptionalPreprocessingMessage(), isActive);
	}

	private DirectiveTriviaSyntax ParseIgnoredDirective(SyntaxToken hash, SyntaxToken colon, bool isActive, bool isFollowingToken)
	{
		if (isActive)
		{
			if (!lexer.Options.FileBasedProgram)
			{
				colon = AddError(colon, ErrorCode.ERR_PPIgnoredNeedsFileBasedProgram);
			}
			if (isFollowingToken)
			{
				colon = AddError(colon, ErrorCode.ERR_PPIgnoredFollowsToken);
			}
			if (_context.SeenAnyIfDirectives)
			{
				colon = AddError(colon, ErrorCode.ERR_PPIgnoredFollowsIf);
			}
		}
		SyntaxToken endOfDirectiveToken = lexer.LexEndOfDirectiveWithOptionalContent(out SyntaxToken content);
		return SyntaxFactory.IgnoredDirectiveTrivia(hash, colon, content, endOfDirectiveToken, isActive);
	}

	private SyntaxToken ParseEndOfDirectiveWithOptionalPreprocessingMessage()
	{
		return lexer.LexEndOfDirectiveWithOptionalPreprocessingMessage();
	}

	private SyntaxToken ParseEndOfDirective(bool ignoreErrors, bool afterPragma = false, bool afterLineNumber = false)
	{
		SyntaxListBuilder<SyntaxToken> syntaxListBuilder = default(SyntaxListBuilder<SyntaxToken>);
		if (base.CurrentToken.Kind != SyntaxKind.EndOfDirectiveToken && base.CurrentToken.Kind != SyntaxKind.EndOfFileToken)
		{
			syntaxListBuilder = new SyntaxListBuilder<SyntaxToken>(10);
			if (!ignoreErrors)
			{
				ErrorCode code = ErrorCode.ERR_EndOfPPLineExpected;
				if (afterPragma)
				{
					code = ErrorCode.WRN_EndOfPPLineExpected;
				}
				else if (afterLineNumber)
				{
					code = ErrorCode.ERR_MissingPPFile;
				}
				syntaxListBuilder.Add(AddError(EatToken().WithoutDiagnosticsGreen(), code));
			}
			while (base.CurrentToken.Kind != SyntaxKind.EndOfDirectiveToken && base.CurrentToken.Kind != SyntaxKind.EndOfFileToken)
			{
				syntaxListBuilder.Add(EatToken().WithoutDiagnosticsGreen());
			}
		}
		SyntaxToken syntaxToken = ((base.CurrentToken.Kind == SyntaxKind.EndOfDirectiveToken) ? EatToken() : SyntaxFactory.Token(SyntaxKind.EndOfDirectiveToken));
		if (!syntaxListBuilder.IsNull)
		{
			syntaxToken = syntaxToken.TokenWithLeadingTrivia(SyntaxFactory.SkippedTokensTrivia(syntaxListBuilder.ToList()));
		}
		return syntaxToken;
	}

	private ExpressionSyntax ParseExpression()
	{
		return ParseLogicalOr();
	}

	private ExpressionSyntax ParseLogicalOr()
	{
		ExpressionSyntax expressionSyntax = ParseLogicalAnd();
		while (base.CurrentToken.Kind == SyntaxKind.BarBarToken)
		{
			SyntaxToken operatorToken = EatToken();
			ExpressionSyntax right = ParseLogicalAnd();
			expressionSyntax = SyntaxFactory.BinaryExpression(SyntaxKind.LogicalOrExpression, expressionSyntax, operatorToken, right);
		}
		return expressionSyntax;
	}

	private ExpressionSyntax ParseLogicalAnd()
	{
		ExpressionSyntax expressionSyntax = ParseEquality();
		while (base.CurrentToken.Kind == SyntaxKind.AmpersandAmpersandToken)
		{
			SyntaxToken operatorToken = EatToken();
			ExpressionSyntax right = ParseEquality();
			expressionSyntax = SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression, expressionSyntax, operatorToken, right);
		}
		return expressionSyntax;
	}

	private ExpressionSyntax ParseEquality()
	{
		ExpressionSyntax expressionSyntax = ParseLogicalNot();
		while (base.CurrentToken.Kind == SyntaxKind.EqualsEqualsToken || base.CurrentToken.Kind == SyntaxKind.ExclamationEqualsToken)
		{
			SyntaxToken syntaxToken = EatToken();
			ExpressionSyntax right = ParseEquality();
			expressionSyntax = SyntaxFactory.BinaryExpression(SyntaxFacts.GetBinaryExpression(syntaxToken.Kind), expressionSyntax, syntaxToken, right);
		}
		return expressionSyntax;
	}

	private ExpressionSyntax ParseLogicalNot()
	{
		if (base.CurrentToken.Kind == SyntaxKind.ExclamationToken)
		{
			SyntaxToken operatorToken = EatToken();
			return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, operatorToken, ParseLogicalNot());
		}
		return ParsePrimary();
	}

	private ExpressionSyntax ParsePrimary()
	{
		SyntaxKind kind = base.CurrentToken.Kind;
		switch (kind)
		{
		case SyntaxKind.OpenParenToken:
		{
			SyntaxToken openParenToken = EatToken();
			ExpressionSyntax expression = ParseExpression();
			SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
			return SyntaxFactory.ParenthesizedExpression(openParenToken, expression, closeParenToken);
		}
		case SyntaxKind.IdentifierToken:
			return SyntaxFactory.IdentifierName(TruncateIdentifier(EatToken()));
		case SyntaxKind.TrueKeyword:
		case SyntaxKind.FalseKeyword:
			return SyntaxFactory.LiteralExpression(SyntaxFacts.GetLiteralExpression(kind), EatToken());
		default:
			return SyntaxFactory.IdentifierName(EatToken(SyntaxKind.IdentifierToken, ErrorCode.ERR_InvalidPreprocExpr));
		}
	}

	private static SyntaxToken TruncateIdentifier(SyntaxToken identifier)
	{
		if (identifier.Width > 128)
		{
			GreenNode leadingTrivia = identifier.GetLeadingTrivia();
			GreenNode trailingTrivia = identifier.GetTrailingTrivia();
			string text = identifier.ToString();
			string valueText = text.Substring(0, 128);
			identifier = SyntaxFactory.Identifier(SyntaxKind.IdentifierToken, leadingTrivia, text, valueText, trailingTrivia);
		}
		return identifier;
	}

	private bool EvaluateBool(ExpressionSyntax expr)
	{
		object obj = Evaluate(expr);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return false;
	}

	private object Evaluate(ExpressionSyntax expr)
	{
		switch (expr.Kind)
		{
		case SyntaxKind.ParenthesizedExpression:
			return Evaluate(((ParenthesizedExpressionSyntax)expr).Expression);
		case SyntaxKind.TrueLiteralExpression:
		case SyntaxKind.FalseLiteralExpression:
			return ((LiteralExpressionSyntax)expr).Token.Value;
		case SyntaxKind.LogicalAndExpression:
		case SyntaxKind.BitwiseAndExpression:
			return EvaluateBool(((BinaryExpressionSyntax)expr).Left) && EvaluateBool(((BinaryExpressionSyntax)expr).Right);
		case SyntaxKind.LogicalOrExpression:
		case SyntaxKind.BitwiseOrExpression:
			return EvaluateBool(((BinaryExpressionSyntax)expr).Left) || EvaluateBool(((BinaryExpressionSyntax)expr).Right);
		case SyntaxKind.EqualsExpression:
			return object.Equals(Evaluate(((BinaryExpressionSyntax)expr).Left), Evaluate(((BinaryExpressionSyntax)expr).Right));
		case SyntaxKind.NotEqualsExpression:
			return !object.Equals(Evaluate(((BinaryExpressionSyntax)expr).Left), Evaluate(((BinaryExpressionSyntax)expr).Right));
		case SyntaxKind.LogicalNotExpression:
			return !EvaluateBool(((PrefixUnaryExpressionSyntax)expr).Operand);
		case SyntaxKind.IdentifierName:
		{
			string valueText = ((IdentifierNameSyntax)expr).Identifier.ValueText;
			if (bool.TryParse(valueText, out var result))
			{
				return result;
			}
			return IsDefined(valueText);
		}
		default:
			return false;
		}
	}

	private bool IsDefined(string id)
	{
		return _context.IsDefined(id) switch
		{
			DefineState.Defined => true, 
			DefineState.Undefined => false, 
			_ => base.Options.PreprocessorSymbols.Contains(id), 
		};
	}
}
