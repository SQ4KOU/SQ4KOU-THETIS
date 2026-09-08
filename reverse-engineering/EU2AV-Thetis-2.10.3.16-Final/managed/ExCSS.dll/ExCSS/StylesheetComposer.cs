using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExCSS;

internal sealed class StylesheetComposer
{
	private readonly Lexer _lexer;

	private readonly StylesheetParser _parser;

	private readonly Stack<StylesheetNode> _nodes;

	public StylesheetComposer(Lexer lexer, StylesheetParser parser)
	{
		_lexer = lexer;
		_parser = parser;
		_nodes = new Stack<StylesheetNode>();
	}

	public Rule CreateAtRule(Token token)
	{
		if (token.Data.Is(RuleNames.Media))
		{
			return CreateMedia(token);
		}
		if (token.Data.Is(RuleNames.FontFace))
		{
			return CreateFontFace(token);
		}
		if (token.Data.Is(RuleNames.Keyframes))
		{
			return CreateKeyframes(token);
		}
		if (token.Data.Is(RuleNames.Import))
		{
			return CreateImport(token);
		}
		if (token.Data.Is(RuleNames.Charset))
		{
			return CreateCharset(token);
		}
		if (token.Data.Is(RuleNames.Namespace))
		{
			return CreateNamespace(token);
		}
		if (token.Data.Is(RuleNames.Page))
		{
			return CreatePage(token);
		}
		if (token.Data.Is(RuleNames.Supports))
		{
			return CreateSupports(token);
		}
		if (token.Data.Is(RuleNames.ViewPort))
		{
			return CreateViewport(token);
		}
		if (token.Data.Is(RuleNames.Container))
		{
			return CreateContainer(token);
		}
		if (!token.Data.Is(RuleNames.Document))
		{
			return CreateUnknown(token);
		}
		return CreateDocument(token);
	}

	public Rule CreateRule(Token token)
	{
		switch (token.Type)
		{
		case TokenType.AtKeyword:
			return CreateAtRule(token);
		case TokenType.CurlyBracketOpen:
			RaiseErrorOccurred(ParseError.InvalidBlockStart, token.Position);
			MoveToRuleEnd(ref token);
			return null;
		case TokenType.String:
		case TokenType.Url:
		case TokenType.RoundBracketClose:
		case TokenType.CurlyBracketClose:
		case TokenType.SquareBracketClose:
			RaiseErrorOccurred(ParseError.InvalidToken, token.Position);
			MoveToRuleEnd(ref token);
			return null;
		default:
			return CreateStyle(token);
		}
	}

	public Rule CreateCharset(Token current)
	{
		CharsetRule charsetRule = new CharsetRule(_parser);
		TextPosition position = current.Position;
		Token token = NextToken();
		_nodes.Push(charsetRule);
		ParseComments(ref token);
		if (token.Type == TokenType.String)
		{
			charsetRule.CharacterSet = token.Data;
		}
		JumpToEnd(ref token);
		charsetRule.StylesheetText = CreateView(position, token.Position);
		_nodes.Pop();
		return charsetRule;
	}

	public Rule CreateDocument(Token current)
	{
		DocumentRule rule = new DocumentRule(_parser);
		TextPosition position = current.Position;
		Token token = NextToken();
		_nodes.Push(rule);
		ParseComments(ref token);
		FillFunctions(delegate(DocumentFunction function)
		{
			rule.AppendChild(function);
		}, ref token);
		ParseComments(ref token);
		if (token.Type == TokenType.CurlyBracketOpen)
		{
			TextPosition end = FillRules(rule);
			rule.StylesheetText = CreateView(position, end);
			_nodes.Pop();
			return rule;
		}
		_nodes.Pop();
		return SkipDeclarations(token);
	}

	public Rule CreateViewport(Token current)
	{
		ViewportRule viewportRule = new ViewportRule(_parser);
		TextPosition position = current.Position;
		Token token = NextToken();
		_nodes.Push(viewportRule);
		ParseComments(ref token);
		if (token.Type == TokenType.CurlyBracketOpen)
		{
			TextPosition end = FillDeclarations(viewportRule, PropertyFactory.Instance.CreateViewport);
			viewportRule.StylesheetText = CreateView(position, end);
			_nodes.Pop();
			return viewportRule;
		}
		_nodes.Pop();
		return SkipDeclarations(token);
	}

	public Rule CreateFontFace(Token current)
	{
		FontFaceRule fontFaceRule = new FontFaceRule(_parser);
		TextPosition position = current.Position;
		Token token = NextToken();
		_nodes.Push(fontFaceRule);
		ParseComments(ref token);
		if (token.Type == TokenType.CurlyBracketOpen)
		{
			TextPosition end = FillDeclarations(fontFaceRule, PropertyFactory.Instance.CreateFont);
			fontFaceRule.StylesheetText = CreateView(position, end);
			_nodes.Pop();
			return fontFaceRule;
		}
		_nodes.Pop();
		return SkipDeclarations(token);
	}

	public Rule CreateImport(Token current)
	{
		ImportRule importRule = new ImportRule(_parser);
		TextPosition position = current.Position;
		Token token = NextToken();
		_nodes.Push(importRule);
		ParseComments(ref token);
		if (token.Is(TokenType.String, TokenType.Url))
		{
			importRule.Href = token.Data;
			token = NextToken();
			ParseComments(ref token);
			FillMediaList(importRule.Media, TokenType.Semicolon, ref token);
		}
		ParseComments(ref token);
		JumpToEnd(ref token);
		importRule.StylesheetText = CreateView(position, token.Position);
		_nodes.Pop();
		return importRule;
	}

	public Rule CreateKeyframes(Token current)
	{
		KeyframesRule keyframesRule = new KeyframesRule(_parser);
		TextPosition position = current.Position;
		Token token = NextToken();
		_nodes.Push(keyframesRule);
		ParseComments(ref token);
		keyframesRule.Name = GetRuleName(ref token);
		ParseComments(ref token);
		if (token.Type == TokenType.CurlyBracketOpen)
		{
			TextPosition end = FillKeyframeRules(keyframesRule);
			keyframesRule.StylesheetText = CreateView(position, end);
			_nodes.Pop();
			return keyframesRule;
		}
		_nodes.Pop();
		return SkipDeclarations(token);
	}

	public Rule CreateMedia(Token current)
	{
		MediaRule mediaRule = new MediaRule(_parser);
		TextPosition position = current.Position;
		Token token = NextToken();
		_nodes.Push(mediaRule);
		ParseComments(ref token);
		FillMediaList(mediaRule.Media, TokenType.CurlyBracketOpen, ref token);
		ParseComments(ref token);
		if (token.Type != TokenType.CurlyBracketOpen)
		{
			while (token.Type != TokenType.EndOfFile)
			{
				if (token.Type == TokenType.Semicolon)
				{
					_nodes.Pop();
					return null;
				}
				if (token.Type == TokenType.CurlyBracketOpen)
				{
					break;
				}
				token = NextToken();
			}
		}
		TextPosition end = FillRules(mediaRule);
		mediaRule.StylesheetText = CreateView(position, end);
		_nodes.Pop();
		return mediaRule;
	}

	public Rule CreateContainer(Token current)
	{
		ContainerRule containerRule = new ContainerRule(_parser);
		TextPosition position = current.Position;
		Token token = NextToken();
		_nodes.Push(containerRule);
		ParseComments(ref token);
		containerRule.Name = GetRuleName(ref token);
		ParseComments(ref token);
		FillMediaList(containerRule.Media, TokenType.CurlyBracketOpen, ref token);
		ParseComments(ref token);
		if (token.Type != TokenType.CurlyBracketOpen)
		{
			while (token.Type != TokenType.EndOfFile)
			{
				if (token.Type == TokenType.Semicolon)
				{
					_nodes.Pop();
					return null;
				}
				if (token.Type == TokenType.CurlyBracketOpen)
				{
					break;
				}
				token = NextToken();
			}
		}
		TextPosition end = FillRules(containerRule);
		containerRule.StylesheetText = CreateView(position, end);
		_nodes.Pop();
		return containerRule;
	}

	public Rule CreateNamespace(Token current)
	{
		NamespaceRule namespaceRule = new NamespaceRule(_parser);
		TextPosition position = current.Position;
		Token token = NextToken();
		_nodes.Push(namespaceRule);
		ParseComments(ref token);
		namespaceRule.Prefix = GetRuleName(ref token);
		ParseComments(ref token);
		if (token.Type == TokenType.Url)
		{
			namespaceRule.NamespaceUri = token.Data;
		}
		JumpToEnd(ref token);
		namespaceRule.StylesheetText = CreateView(position, token.Position);
		_nodes.Pop();
		return namespaceRule;
	}

	public Rule CreatePage(Token current)
	{
		PageRule pageRule = new PageRule(_parser);
		TextPosition position = current.Position;
		Token token = NextToken();
		_nodes.Push(pageRule);
		ParseComments(ref token);
		if (token.Type != TokenType.CurlyBracketOpen)
		{
			pageRule.Selector = CreatePageSelector(ref token);
			ParseComments(ref token);
		}
		if (token.Type == TokenType.CurlyBracketOpen)
		{
			TextPosition end = FillDeclarations(pageRule.Style);
			pageRule.StylesheetText = CreateView(position, end);
			_nodes.Pop();
			return pageRule;
		}
		_nodes.Pop();
		return SkipDeclarations(token);
	}

	public Rule CreateSupports(Token current)
	{
		SupportsRule supportsRule = new SupportsRule(_parser);
		TextPosition position = current.Position;
		Token token = NextToken();
		_nodes.Push(supportsRule);
		ParseComments(ref token);
		supportsRule.Condition = AggregateCondition(ref token);
		ParseComments(ref token);
		if (token.Type == TokenType.CurlyBracketOpen)
		{
			TextPosition end = FillRules(supportsRule);
			supportsRule.StylesheetText = CreateView(position, end);
			_nodes.Pop();
			return supportsRule;
		}
		_nodes.Pop();
		return SkipDeclarations(token);
	}

	public Rule CreateStyle(Token current)
	{
		StyleRule styleRule = new StyleRule(_parser);
		TextPosition position = current.Position;
		_nodes.Push(styleRule);
		ParseComments(ref current);
		styleRule.Selector = CreateSelector(ref current);
		TextPosition end = FillDeclarations(styleRule.Style);
		styleRule.StylesheetText = CreateView(position, end);
		_nodes.Pop();
		if (styleRule.Selector == null)
		{
			return null;
		}
		return styleRule;
	}

	public Rule CreateMarginStyle(ref Token current)
	{
		MarginStyleRule marginStyleRule = new MarginStyleRule(_parser);
		TextPosition position = current.Position;
		_nodes.Push(marginStyleRule);
		ParseComments(ref current);
		marginStyleRule.Selector = CreateMarginSelector(ref current);
		TextPosition end = FillDeclarations(marginStyleRule.Style);
		marginStyleRule.StylesheetText = CreateView(position, end);
		_nodes.Pop();
		if (marginStyleRule.Selector == null)
		{
			return null;
		}
		return marginStyleRule;
	}

	public KeyframeRule CreateKeyframeRule(Token current)
	{
		KeyframeRule keyframeRule = new KeyframeRule(_parser);
		TextPosition position = current.Position;
		_nodes.Push(keyframeRule);
		ParseComments(ref current);
		keyframeRule.Key = CreateKeyframeSelector(ref current);
		TextPosition end = FillDeclarations(keyframeRule.Style);
		keyframeRule.StylesheetText = CreateView(position, end);
		_nodes.Pop();
		if (keyframeRule.Key == null)
		{
			return null;
		}
		return keyframeRule;
	}

	public Rule CreateUnknown(Token current)
	{
		TextPosition position = current.Position;
		if (_parser.Options.IncludeUnknownRules)
		{
			Token token = NextToken();
			UnknownRule unknownRule = new UnknownRule(current.Data, _parser);
			_nodes.Push(unknownRule);
			while (token.IsNot(TokenType.CurlyBracketOpen, TokenType.Semicolon, TokenType.EndOfFile))
			{
				token = NextToken();
			}
			if (token.Type == TokenType.CurlyBracketOpen)
			{
				int num = 1;
				do
				{
					token = NextToken();
					switch (token.Type)
					{
					case TokenType.CurlyBracketOpen:
						num++;
						break;
					case TokenType.CurlyBracketClose:
						num--;
						break;
					case TokenType.EndOfFile:
						num = 0;
						break;
					}
				}
				while (num != 0);
			}
			unknownRule.StylesheetText = CreateView(position, token.Position);
			_nodes.Pop();
			return unknownRule;
		}
		RaiseErrorOccurred(ParseError.UnknownAtRule, position);
		MoveToRuleEnd(ref current);
		return null;
	}

	public TokenValue CreateValue(ref Token token)
	{
		bool important;
		return CreateValue(TokenType.CurlyBracketClose, ref token, out important);
	}

	public List<Medium> CreateMedia(ref Token token)
	{
		List<Medium> list = new List<Medium>();
		ParseComments(ref token);
		while (token.Type != TokenType.EndOfFile)
		{
			Medium medium = CreateMedium(ref token);
			if (medium == null || token.IsNot(TokenType.Comma, TokenType.EndOfFile))
			{
				throw new ParseException("Unable to create medium or end of file reached unexpectedly");
			}
			token = NextToken();
			ParseComments(ref token);
			list.Add(medium);
		}
		return list;
	}

	public TextPosition CreateRules(Stylesheet sheet)
	{
		Token token = NextToken();
		_nodes.Push(sheet);
		ParseComments(ref token);
		while (token.Type != TokenType.EndOfFile)
		{
			Rule rule = CreateRule(token);
			token = NextToken();
			ParseComments(ref token);
			sheet.Rules.Add(rule);
		}
		_nodes.Pop();
		return token.Position;
	}

	public IConditionFunction CreateCondition(ref Token token)
	{
		ParseComments(ref token);
		return AggregateCondition(ref token);
	}

	public KeyframeSelector CreateKeyframeSelector(ref Token token)
	{
		List<Percent> list = new List<Percent>();
		bool flag = true;
		TextPosition position = token.Position;
		ParseComments(ref token);
		for (; token.Type != TokenType.EndOfFile; token = NextToken(), ParseComments(ref token))
		{
			if (list.Count > 0)
			{
				if (token.Type == TokenType.CurlyBracketOpen)
				{
					break;
				}
				if (token.Type != TokenType.Comma)
				{
					flag = false;
				}
				else
				{
					token = NextToken();
				}
				ParseComments(ref token);
			}
			switch (token.Type)
			{
			case TokenType.Percentage:
				list.Add(new Percent(((UnitToken)token).Value));
				continue;
			case TokenType.Ident:
				if (token.Data.Is(Keywords.From))
				{
					list.Add(Percent.Zero);
					continue;
				}
				if (token.Data.Is(Keywords.To))
				{
					list.Add(Percent.Hundred);
					continue;
				}
				break;
			}
			flag = false;
		}
		if (!flag)
		{
			RaiseErrorOccurred(ParseError.InvalidSelector, position);
		}
		return new KeyframeSelector(list);
	}

	private PageSelector CreatePageSelector(ref Token token)
	{
		PageSelector result;
		if (token.Type == TokenType.Colon)
		{
			token = NextToken();
			result = ((token.Type == TokenType.Ident) ? new PageSelector(token.Data) : new PageSelector());
			token = NextToken();
		}
		else
		{
			result = new PageSelector();
		}
		return result;
	}

	public List<DocumentFunction> CreateFunctions(ref Token token)
	{
		List<DocumentFunction> functions = new List<DocumentFunction>();
		ParseComments(ref token);
		FillFunctions(delegate(DocumentFunction function)
		{
			functions.Add(function);
		}, ref token);
		return functions;
	}

	public TextPosition FillDeclarations(StyleDeclaration style)
	{
		Dictionary<string, IProperty> dictionary = new Dictionary<string, IProperty>(StringComparer.OrdinalIgnoreCase);
		Token token = NextToken();
		_nodes.Push(style);
		ParseComments(ref token);
		while (token.IsNot(TokenType.EndOfFile, TokenType.CurlyBracketClose))
		{
			if (token.Is(TokenType.AtKeyword))
			{
				StylesheetNode stylesheetNode = _nodes.FirstOrDefault((StylesheetNode parent) => parent is PageRule);
				if (stylesheetNode != null)
				{
					Token current = new Token(TokenType.Ident, token.Data, token.Position);
					Rule child = CreateMarginStyle(ref current);
					stylesheetNode.AppendChild(child);
					token = current;
				}
				else
				{
					token = _lexer.Get();
				}
			}
			else
			{
				Property property = CreateDeclarationWith(PropertyFactory.Instance.Create, ref token);
				Property[] array = new Property[1] { property };
				if (property != null && property.HasValue)
				{
					if (property is ShorthandProperty shorthandProperty)
					{
						array = PropertyFactory.Instance.CreateLonghandsFor(shorthandProperty.Name);
						shorthandProperty.Export(array);
					}
					Property[] array2 = array;
					foreach (Property property2 in array2)
					{
						if (!dictionary.TryGetValue(property2.Name, out var value) || !value.IsImportant || property2.IsImportant)
						{
							style.SetProperty(property2);
							dictionary[property2.Name] = property2;
						}
					}
				}
			}
			ParseComments(ref token);
		}
		_nodes.Pop();
		return token.Position;
	}

	public Property CreateDeclarationWith(Func<string, Property> createProperty, ref Token token)
	{
		Property property = null;
		StringBuilder stringBuilder = Pool.NewStringBuilder();
		TextPosition position = token.Position;
		while (token.IsDeclarationName())
		{
			stringBuilder.Append(token.ToValue());
			token = NextToken();
		}
		string text = stringBuilder.ToPool();
		if (text.Length > 0)
		{
			property = createProperty(text);
			if (property == null && _parser.Options.IncludeUnknownDeclarations)
			{
				property = new UnknownProperty(text);
			}
			if (property == null)
			{
				RaiseErrorOccurred(ParseError.UnknownDeclarationName, position);
			}
			else
			{
				_nodes.Push(property);
			}
			ParseComments(ref token);
			if (token.Type == TokenType.Colon)
			{
				TokenValue tokenValue = CreateValue(TokenType.CurlyBracketClose, ref token, out var important);
				if (tokenValue == null)
				{
					RaiseErrorOccurred(ParseError.ValueMissing, token.Position);
				}
				else if (property != null)
				{
					if (property.TrySetValue(tokenValue))
					{
						property.IsImportant = important;
					}
					else if (_parser.Options.AllowInvalidValues)
					{
						_nodes.Pop();
						property = new UnknownProperty(text);
						property.TrySetValue(tokenValue);
						property.IsImportant = important;
						_nodes.Push(property);
					}
				}
				ParseComments(ref token);
			}
			else
			{
				RaiseErrorOccurred(ParseError.ColonMissing, token.Position);
			}
			JumpToDeclEnd(ref token);
			if (property != null)
			{
				_nodes.Pop();
			}
		}
		else if (token.Type != TokenType.EndOfFile)
		{
			RaiseErrorOccurred(ParseError.IdentExpected, position);
			JumpToDeclEnd(ref token);
		}
		if (token.Type == TokenType.Semicolon)
		{
			token = NextToken();
		}
		return property;
	}

	public Property CreateDeclaration(ref Token token)
	{
		ParseComments(ref token);
		return CreateDeclarationWith(PropertyFactory.Instance.Create, ref token);
	}

	public Medium CreateMedium(ref Token token)
	{
		Medium medium = new Medium();
		ParseComments(ref token);
		if (token.Type == TokenType.Ident)
		{
			string data = token.Data;
			if (data.Isi(Keywords.Not))
			{
				medium.IsInverse = true;
				token = NextToken();
				ParseComments(ref token);
			}
			else if (data.Isi(Keywords.Only))
			{
				medium.IsExclusive = true;
				token = NextToken();
				ParseComments(ref token);
			}
		}
		if (token.Type == TokenType.Ident)
		{
			medium.Type = token.Data;
			token = NextToken();
			ParseComments(ref token);
			if (token.Type != TokenType.Ident || !token.Data.Isi(Keywords.And))
			{
				return medium;
			}
			token = NextToken();
			ParseComments(ref token);
		}
		do
		{
			if (token.Type != TokenType.RoundBracketOpen)
			{
				return null;
			}
			token = NextToken();
			ParseComments(ref token);
			MediaFeature mediaFeature = CreateFeature(ref token);
			if (mediaFeature != null)
			{
				medium.AppendChild(mediaFeature);
			}
			if (token.Type != TokenType.RoundBracketClose)
			{
				return null;
			}
			token = NextToken();
			ParseComments(ref token);
			if (mediaFeature == null)
			{
				return null;
			}
			if (token.Type != TokenType.Ident || !token.Data.Isi(Keywords.And))
			{
				break;
			}
			token = NextToken();
			ParseComments(ref token);
		}
		while (token.Type != TokenType.EndOfFile);
		return medium;
	}

	private void JumpToEnd(ref Token current)
	{
		while (current.IsNot(TokenType.EndOfFile, TokenType.Semicolon))
		{
			current = NextToken();
		}
	}

	private void MoveToRuleEnd(ref Token current)
	{
		int num = 0;
		while (current.Type != TokenType.EndOfFile)
		{
			if (current.Type == TokenType.CurlyBracketOpen)
			{
				num++;
			}
			else if (current.Type == TokenType.CurlyBracketClose)
			{
				num--;
			}
			if (num > 0 || !current.Is(TokenType.CurlyBracketClose, TokenType.Semicolon))
			{
				current = NextToken();
				continue;
			}
			break;
		}
	}

	private void JumpToArgEnd(ref Token current)
	{
		int num = 0;
		while (current.Type != TokenType.EndOfFile)
		{
			if (current.Type == TokenType.RoundBracketOpen)
			{
				num++;
			}
			else
			{
				if (num <= 0 && current.Type == TokenType.RoundBracketClose)
				{
					break;
				}
				if (current.Type == TokenType.RoundBracketClose)
				{
					num--;
				}
			}
			current = NextToken();
		}
	}

	private void JumpToDeclEnd(ref Token current)
	{
		int num = 0;
		while (current.Type != TokenType.EndOfFile)
		{
			if (current.Type == TokenType.CurlyBracketOpen)
			{
				num++;
			}
			else
			{
				if (num <= 0 && current.Is(TokenType.CurlyBracketClose, TokenType.Semicolon))
				{
					break;
				}
				if (current.Type == TokenType.CurlyBracketClose)
				{
					num--;
				}
			}
			current = NextToken();
		}
	}

	private Token NextToken()
	{
		return _lexer.Get();
	}

	private StylesheetText CreateView(TextPosition start, TextPosition end)
	{
		return new StylesheetText(new TextRange(start, end), _lexer.Source);
	}

	private void ParseComments(ref Token token)
	{
		bool preserveComments = _parser.Options.PreserveComments;
		while (token.Type == TokenType.Whitespace || token.Type == TokenType.Comment || token.Type == TokenType.Cdc || token.Type == TokenType.Cdo)
		{
			if (preserveComments && token.Type == TokenType.Comment)
			{
				StylesheetNode stylesheetNode = _nodes.Peek();
				Comment comment = new Comment(token.Data);
				TextPosition position = token.Position;
				comment.StylesheetText = CreateView(end: position.After(token.ToValue()), start: position);
				stylesheetNode.AppendChild(comment);
			}
			token = _lexer.Get();
		}
	}

	private Rule SkipDeclarations(Token token)
	{
		RaiseErrorOccurred(ParseError.InvalidToken, token.Position);
		MoveToRuleEnd(ref token);
		return null;
	}

	private void RaiseErrorOccurred(ParseError code, TextPosition position)
	{
		_lexer.RaiseErrorOccurred(code, position);
	}

	private IConditionFunction AggregateCondition(ref Token token)
	{
		IConditionFunction conditionFunction = ExtractCondition(ref token);
		if (conditionFunction == null)
		{
			return null;
		}
		ParseComments(ref token);
		string data = token.Data;
		Func<IEnumerable<IConditionFunction>, IConditionFunction> creator = data.GetCreator();
		if (creator != null)
		{
			token = NextToken();
			ParseComments(ref token);
			List<IConditionFunction> arg = MultipleConditions(conditionFunction, data, ref token);
			conditionFunction = creator(arg);
		}
		return conditionFunction;
	}

	private IConditionFunction ExtractCondition(ref Token token)
	{
		if (token.Type == TokenType.RoundBracketOpen)
		{
			token = NextToken();
			ParseComments(ref token);
			IConditionFunction conditionFunction = AggregateCondition(ref token);
			if (conditionFunction != null)
			{
				conditionFunction = new GroupCondition
				{
					Content = conditionFunction
				};
			}
			else if (token.Type == TokenType.Ident)
			{
				conditionFunction = DeclarationCondition(ref token);
			}
			if (token.Type != TokenType.RoundBracketClose)
			{
				return conditionFunction;
			}
			token = NextToken();
			ParseComments(ref token);
			return conditionFunction;
		}
		if (token.Data.Isi(Keywords.Not))
		{
			NotCondition notCondition = new NotCondition();
			token = NextToken();
			ParseComments(ref token);
			notCondition.Content = ExtractCondition(ref token);
			return notCondition;
		}
		return null;
	}

	private IConditionFunction DeclarationCondition(ref Token token)
	{
		Property property = PropertyFactory.Instance.Create(token.Data) ?? new UnknownProperty(token.Data);
		DeclarationCondition result = null;
		token = NextToken();
		ParseComments(ref token);
		if (token.Type != TokenType.Colon)
		{
			return null;
		}
		TokenValue tokenValue = CreateValue(TokenType.RoundBracketClose, ref token, out var important);
		property.IsImportant = important;
		if (tokenValue != null)
		{
			result = new DeclarationCondition(property, tokenValue);
		}
		return result;
	}

	private List<IConditionFunction> MultipleConditions(IConditionFunction condition, string connector, ref Token token)
	{
		List<IConditionFunction> list = new List<IConditionFunction>();
		ParseComments(ref token);
		list.Add(condition);
		while (token.Type != TokenType.EndOfFile)
		{
			condition = ExtractCondition(ref token);
			if (condition == null)
			{
				break;
			}
			list.Add(condition);
			if (!token.Data.Isi(connector))
			{
				break;
			}
			token = NextToken();
			ParseComments(ref token);
		}
		return list;
	}

	private void FillFunctions(Action<DocumentFunction> add, ref Token token)
	{
		do
		{
			DocumentFunction documentFunction = token.ToDocumentFunction();
			if (documentFunction != null)
			{
				token = NextToken();
				ParseComments(ref token);
				add(documentFunction);
				if (token.Type == TokenType.Comma)
				{
					token = NextToken();
					ParseComments(ref token);
					continue;
				}
				break;
			}
			break;
		}
		while (token.Type != TokenType.EndOfFile);
	}

	private TextPosition FillKeyframeRules(KeyframesRule parentRule)
	{
		Token token = NextToken();
		ParseComments(ref token);
		while (token.IsNot(TokenType.EndOfFile, TokenType.CurlyBracketClose))
		{
			KeyframeRule rule = CreateKeyframeRule(token);
			token = NextToken();
			ParseComments(ref token);
			parentRule.Rules.Add(rule);
		}
		return token.Position;
	}

	private TextPosition FillDeclarations(DeclarationRule rule, Func<string, Property> createProperty)
	{
		Token token = NextToken();
		ParseComments(ref token);
		while (token.IsNot(TokenType.EndOfFile, TokenType.CurlyBracketClose))
		{
			Property property = CreateDeclarationWith(createProperty, ref token);
			if (property != null && property.HasValue)
			{
				rule.SetProperty(property);
			}
			ParseComments(ref token);
		}
		return token.Position;
	}

	private TextPosition FillRules(GroupingRule group)
	{
		Token token = NextToken();
		ParseComments(ref token);
		while (token.IsNot(TokenType.EndOfFile, TokenType.CurlyBracketClose))
		{
			Rule rule = CreateRule(token);
			token = NextToken();
			ParseComments(ref token);
			group.Rules.Add(rule);
		}
		return token.Position;
	}

	private void FillMediaList(MediaList list, TokenType end, ref Token token)
	{
		_nodes.Push(list);
		if (token.Type != end)
		{
			while (token.Type != TokenType.EndOfFile)
			{
				Medium medium = CreateMedium(ref token);
				if (medium != null)
				{
					list.AppendChild(medium);
				}
				if (token.Type != TokenType.Comma)
				{
					break;
				}
				token = NextToken();
				ParseComments(ref token);
			}
			if (token.Type != end || list.Length == 0)
			{
				list.Clear();
				list.AppendChild(new Medium
				{
					IsInverse = true,
					Type = Keywords.All
				});
			}
		}
		_nodes.Pop();
	}

	private ISelector CreateSelector(ref Token token)
	{
		SelectorConstructor selectorCreator = _parser.GetSelectorCreator();
		TextPosition position = token.Position;
		while (token.IsNot(TokenType.EndOfFile, TokenType.CurlyBracketOpen, TokenType.CurlyBracketClose))
		{
			selectorCreator.Apply(token);
			token = NextToken();
		}
		bool isValid = selectorCreator.IsValid;
		ISelector selector = selectorCreator.ToPool();
		if (selector is StylesheetNode stylesheetNode)
		{
			TextPosition end = token.Position.Shift(-1);
			stylesheetNode.StylesheetText = CreateView(position, end);
		}
		if (!isValid && !_parser.Options.AllowInvalidValues)
		{
			RaiseErrorOccurred(ParseError.InvalidSelector, position);
			selector = null;
		}
		return selector;
	}

	private ISelector CreateMarginSelector(ref Token token)
	{
		SelectorConstructor selectorCreator = _parser.GetSelectorCreator();
		TextPosition position = token.Position;
		while (token.IsNot(TokenType.EndOfFile, TokenType.CurlyBracketOpen, TokenType.CurlyBracketClose))
		{
			selectorCreator.Apply(token);
			token = NextToken();
		}
		bool isValid = selectorCreator.IsValid;
		ISelector selector = selectorCreator.ToPool();
		if (selector is StylesheetNode stylesheetNode)
		{
			TextPosition end = token.Position.Shift(-1);
			stylesheetNode.StylesheetText = CreateView(position, end);
		}
		if (!isValid && !_parser.Options.AllowInvalidValues)
		{
			RaiseErrorOccurred(ParseError.InvalidSelector, position);
			selector = null;
		}
		return selector;
	}

	private TokenValue CreateValue(TokenType closing, ref Token token, out bool important)
	{
		ValueBuilder valueBuilder = Pool.NewValueBuilder();
		_lexer.IsInValue = true;
		token = NextToken();
		TextPosition position = token.Position;
		while (token.IsNot(TokenType.EndOfFile, TokenType.Semicolon, closing))
		{
			valueBuilder.Apply(token);
			token = NextToken();
		}
		important = valueBuilder.IsImportant;
		_lexer.IsInValue = false;
		bool isValid = valueBuilder.IsValid;
		TokenValue tokenValue = valueBuilder.ToPool();
		StylesheetNode stylesheetNode = tokenValue;
		if (stylesheetNode != null)
		{
			TextPosition end = token.Position.Shift(-1);
			stylesheetNode.StylesheetText = CreateView(position, end);
		}
		if (!isValid && !_parser.Options.AllowInvalidValues)
		{
			RaiseErrorOccurred(ParseError.InvalidValue, position);
			tokenValue = null;
		}
		return tokenValue;
	}

	private string GetRuleName(ref Token token)
	{
		string result = string.Empty;
		if (token.Type == TokenType.Ident)
		{
			result = token.Data;
			token = NextToken();
		}
		return result;
	}

	private MediaFeature CreateFeature(ref Token token)
	{
		if (token.Type == TokenType.Ident)
		{
			TextPosition position = token.Position;
			TokenValue tokenValue = TokenValue.Empty;
			MediaFeature mediaFeature = (_parser.Options.AllowInvalidConstraints ? new UnknownMediaFeature(token.Data) : MediaFeatureFactory.Instance.Create(token.Data));
			token = NextToken();
			ParseComments(ref token);
			TokenType constraintDelimiter = TokenType.Colon;
			if (token.Type == TokenType.Colon || token.Type == TokenType.GreaterThan || token.Type == TokenType.LessThan || token.Type == TokenType.GreaterThanOrEqual || token.Type == TokenType.LessThanOrEqual)
			{
				constraintDelimiter = token.Type;
				ValueBuilder valueBuilder = Pool.NewValueBuilder();
				token = NextToken();
				while (token.IsNot(TokenType.RoundBracketClose, TokenType.EndOfFile) || !valueBuilder.IsReady)
				{
					valueBuilder.Apply(token);
					token = NextToken();
				}
				tokenValue = valueBuilder.ToPool();
			}
			else if (token.Type == TokenType.EndOfFile)
			{
				return null;
			}
			if (mediaFeature != null && mediaFeature.TrySetValue(tokenValue, constraintDelimiter))
			{
				StylesheetNode stylesheetNode = mediaFeature;
				if (stylesheetNode != null)
				{
					TextPosition end = token.Position.Shift(-1);
					stylesheetNode.StylesheetText = CreateView(position, end);
				}
				return mediaFeature;
			}
		}
		else
		{
			JumpToArgEnd(ref token);
		}
		return null;
	}
}
