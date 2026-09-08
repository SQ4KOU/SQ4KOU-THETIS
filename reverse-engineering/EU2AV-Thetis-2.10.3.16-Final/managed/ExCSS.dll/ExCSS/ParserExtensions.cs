using System;
using System.Collections.Generic;
using ExCSS.Conditions;

namespace ExCSS;

internal static class ParserExtensions
{
	private static readonly Dictionary<string, Func<string, DocumentFunction>> FunctionTypes = new Dictionary<string, Func<string, DocumentFunction>>(StringComparer.OrdinalIgnoreCase)
	{
		{
			FunctionNames.Url,
			(string url) => new UrlFunction(url)
		},
		{
			FunctionNames.Domain,
			(string url) => new DomainFunction(url)
		},
		{
			FunctionNames.UrlPrefix,
			(string url) => new UrlPrefixFunction(url)
		}
	};

	private static readonly Dictionary<string, Func<IEnumerable<IConditionFunction>, IConditionFunction>> GroupCreators = new Dictionary<string, Func<IEnumerable<IConditionFunction>, IConditionFunction>>(StringComparer.OrdinalIgnoreCase)
	{
		{
			Keywords.And,
			CreateAndCondition
		},
		{
			Keywords.Or,
			CreateOrCondition
		}
	};

	private static IConditionFunction CreateAndCondition(IEnumerable<IConditionFunction> conditions)
	{
		AndCondition andCondition = new AndCondition();
		foreach (IConditionFunction condition in conditions)
		{
			andCondition.AppendChild(condition);
		}
		return andCondition;
	}

	private static IConditionFunction CreateOrCondition(IEnumerable<IConditionFunction> conditions)
	{
		OrCondition orCondition = new OrCondition();
		foreach (IConditionFunction condition in conditions)
		{
			orCondition.AppendChild(condition);
		}
		return orCondition;
	}

	public static TokenType GetTypeFromName(this string functionName)
	{
		if (!FunctionTypes.TryGetValue(functionName, out var _))
		{
			return TokenType.Function;
		}
		return TokenType.Url;
	}

	public static Func<IEnumerable<IConditionFunction>, IConditionFunction> GetCreator(this string conjunction)
	{
		GroupCreators.TryGetValue(conjunction, out var value);
		return value;
	}

	public static int GetCode(this ParseError code)
	{
		return (int)code;
	}

	public static bool Is(this Token token, TokenType a)
	{
		return token.Type == a;
	}

	public static bool Is(this Token token, TokenType a, TokenType b)
	{
		TokenType type = token.Type;
		if (type != a)
		{
			return type == b;
		}
		return true;
	}

	public static bool IsNot(this Token token, TokenType a, TokenType b)
	{
		TokenType type = token.Type;
		if (type != a)
		{
			return type != b;
		}
		return false;
	}

	public static bool IsNot(this Token token, TokenType a, TokenType b, TokenType c)
	{
		TokenType type = token.Type;
		if (type != a && type != b)
		{
			return type != c;
		}
		return false;
	}

	public static bool IsDeclarationName(this Token token)
	{
		if (token.Type != TokenType.EndOfFile && token.Type != TokenType.Colon && token.Type != TokenType.Whitespace && token.Type != TokenType.Comment && token.Type != TokenType.CurlyBracketOpen)
		{
			return token.Type != TokenType.Semicolon;
		}
		return false;
	}

	public static DocumentFunction ToDocumentFunction(this Token token)
	{
		switch (token.Type)
		{
		case TokenType.Url:
		{
			string functionName = ((UrlToken)token).FunctionName;
			FunctionTypes.TryGetValue(functionName, out var value);
			return value(token.Data);
		}
		case TokenType.Function:
			if (token.Data.Isi(FunctionNames.Regexp))
			{
				string text = ((FunctionToken)token).ArgumentTokens.ToCssString();
				if (text != null)
				{
					return new RegexpFunction(text);
				}
			}
			break;
		}
		return null;
	}

	public static Rule CreateRule(this StylesheetParser parser, RuleType type)
	{
		return type switch
		{
			RuleType.Charset => new CharsetRule(parser), 
			RuleType.Document => new DocumentRule(parser), 
			RuleType.FontFace => new FontFaceRule(parser), 
			RuleType.Import => new ImportRule(parser), 
			RuleType.Keyframe => new KeyframeRule(parser), 
			RuleType.Keyframes => new KeyframesRule(parser), 
			RuleType.Media => new MediaRule(parser), 
			RuleType.Container => new ContainerRule(parser), 
			RuleType.Namespace => new NamespaceRule(parser), 
			RuleType.Page => new PageRule(parser), 
			RuleType.Style => new StyleRule(parser), 
			RuleType.Supports => new SupportsRule(parser), 
			RuleType.Viewport => new ViewportRule(parser), 
			_ => null, 
		};
	}
}
