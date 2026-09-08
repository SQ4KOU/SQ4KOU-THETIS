using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ExCSS;

public class StylesheetParser
{
	internal static readonly StylesheetParser Default = new StylesheetParser();

	internal ParserOptions Options { get; }

	public StylesheetParser(bool includeUnknownRules = false, bool includeUnknownDeclarations = false, bool tolerateInvalidSelectors = false, bool tolerateInvalidValues = false, bool tolerateInvalidConstraints = false, bool preserveComments = false, bool preserveDuplicateProperties = false)
	{
		Options = new ParserOptions
		{
			IncludeUnknownRules = includeUnknownRules,
			IncludeUnknownDeclarations = includeUnknownDeclarations,
			AllowInvalidSelectors = tolerateInvalidSelectors,
			AllowInvalidValues = tolerateInvalidValues,
			AllowInvalidConstraints = tolerateInvalidConstraints,
			PreserveComments = preserveComments,
			PreserveDuplicateProperties = preserveDuplicateProperties
		};
	}

	public Stylesheet Parse(string content)
	{
		TextSource source = new TextSource(content);
		return Parse(source);
	}

	public Stylesheet Parse(Stream content)
	{
		TextSource source = new TextSource(content);
		return Parse(source);
	}

	public Task<Stylesheet> ParseAsync(string content)
	{
		return ParseAsync(content, CancellationToken.None);
	}

	public async Task<Stylesheet> ParseAsync(string content, CancellationToken cancelToken)
	{
		TextSource source = new TextSource(content);
		await source.PrefetchAllAsync(cancelToken).ConfigureAwait(continueOnCapturedContext: false);
		return Parse(source);
	}

	public Task<Stylesheet> ParseAsync(Stream content)
	{
		return ParseAsync(content, CancellationToken.None);
	}

	public async Task<Stylesheet> ParseAsync(Stream content, CancellationToken cancelToken)
	{
		TextSource source = new TextSource(content);
		await source.PrefetchAllAsync(cancelToken).ConfigureAwait(continueOnCapturedContext: false);
		return Parse(source);
	}

	public ISelector ParseSelector(string selectorText)
	{
		Lexer lexer = CreateTokenizer(selectorText);
		Token token = lexer.Get();
		SelectorConstructor selectorCreator = GetSelectorCreator();
		while (token.Type != TokenType.EndOfFile)
		{
			selectorCreator.Apply(token);
			token = lexer.Get();
		}
		bool isValid = selectorCreator.IsValid;
		ISelector result = selectorCreator.ToPool();
		if (!isValid && !Options.AllowInvalidSelectors)
		{
			return null;
		}
		return result;
	}

	internal KeyframeSelector ParseKeyframeSelector(string keyText)
	{
		return Parse(keyText, (StylesheetComposer b, Token t) => Tuple.Create(b.CreateKeyframeSelector(ref t), t));
	}

	internal SelectorConstructor GetSelectorCreator()
	{
		AttributeSelectorFactory instance = AttributeSelectorFactory.Instance;
		PseudoClassSelectorFactory instance2 = PseudoClassSelectorFactory.Instance;
		PseudoElementSelectorFactory pseudoElementSelector = new PseudoElementSelectorFactory(this);
		return Pool.NewSelectorConstructor(instance, instance2, pseudoElementSelector);
	}

	internal Stylesheet Parse(TextSource source)
	{
		Stylesheet stylesheet = new Stylesheet(this);
		Lexer lexer = new Lexer(source);
		TextPosition currentPosition = lexer.GetCurrentPosition();
		TextPosition end = new StylesheetComposer(lexer, this).CreateRules(stylesheet);
		TextRange range = new TextRange(currentPosition, end);
		stylesheet.StylesheetText = new StylesheetText(range, source);
		return stylesheet;
	}

	internal async Task<Stylesheet> ParseAsync(Stylesheet sheet, TextSource source)
	{
		await source.PrefetchAllAsync(CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
		Lexer lexer = new Lexer(source);
		TextPosition currentPosition = lexer.GetCurrentPosition();
		TextPosition end = new StylesheetComposer(lexer, this).CreateRules(sheet);
		TextRange range = new TextRange(currentPosition, end);
		sheet.StylesheetText = new StylesheetText(range, source);
		foreach (IRule rule in sheet.Rules)
		{
			if (rule.Type != RuleType.Charset && rule.Type != RuleType.Import)
			{
				break;
			}
		}
		return sheet;
	}

	internal TokenValue ParseValue(string valueText)
	{
		Lexer lexer = CreateTokenizer(valueText);
		Token token = null;
		TokenValue result = new StylesheetComposer(lexer, this).CreateValue(ref token);
		if (token.Type != TokenType.EndOfFile)
		{
			return null;
		}
		return result;
	}

	internal Rule ParseRule(string ruleText)
	{
		return Parse(ruleText, (StylesheetComposer b, Token t) => b.CreateRule(t));
	}

	internal Property ParseDeclaration(string declarationText)
	{
		return Parse(declarationText, (StylesheetComposer b, Token t) => Tuple.Create(b.CreateDeclaration(ref t), t));
	}

	internal List<Medium> ParseMediaList(string mediaText)
	{
		return Parse(mediaText, (StylesheetComposer b, Token t) => Tuple.Create(b.CreateMedia(ref t), t));
	}

	internal IConditionFunction ParseCondition(string conditionText)
	{
		return Parse(conditionText, (StylesheetComposer b, Token t) => Tuple.Create(b.CreateCondition(ref t), t));
	}

	internal List<DocumentFunction> ParseDocumentRules(string documentText)
	{
		return Parse(documentText, (StylesheetComposer b, Token t) => Tuple.Create(b.CreateFunctions(ref t), t));
	}

	internal Medium ParseMedium(string mediumText)
	{
		return Parse(mediumText, (StylesheetComposer b, Token t) => Tuple.Create(b.CreateMedium(ref t), t));
	}

	internal KeyframeRule ParseKeyframeRule(string ruleText)
	{
		return Parse(ruleText, (StylesheetComposer b, Token t) => b.CreateKeyframeRule(t));
	}

	internal void AppendDeclarations(StyleDeclaration style, string declarations)
	{
		new StylesheetComposer(CreateTokenizer(declarations), this).FillDeclarations(style);
	}

	private T Parse<T>(string source, Func<StylesheetComposer, Token, T> create)
	{
		Lexer lexer = CreateTokenizer(source);
		Token arg = lexer.Get();
		StylesheetComposer arg2 = new StylesheetComposer(lexer, this);
		T result = create(arg2, arg);
		if (lexer.Get().Type != TokenType.EndOfFile)
		{
			return default(T);
		}
		return result;
	}

	private T Parse<T>(string source, Func<StylesheetComposer, Token, Tuple<T, Token>> create)
	{
		Lexer lexer = CreateTokenizer(source);
		Token arg = lexer.Get();
		StylesheetComposer arg2 = new StylesheetComposer(lexer, this);
		Tuple<T, Token> tuple = create(arg2, arg);
		if (tuple.Item2.Type != TokenType.EndOfFile)
		{
			return default(T);
		}
		return tuple.Item1;
	}

	private static Lexer CreateTokenizer(string sourceCode)
	{
		return new Lexer(new TextSource(sourceCode));
	}
}
