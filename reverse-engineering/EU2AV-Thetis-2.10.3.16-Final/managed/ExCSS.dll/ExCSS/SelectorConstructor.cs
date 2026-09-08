using System;
using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

internal sealed class SelectorConstructor
{
	private enum State : byte
	{
		Data,
		Attribute,
		AttributeOperator,
		AttributeValue,
		AttributeEnd,
		Class,
		PseudoClass,
		PseudoElement
	}

	private abstract class FunctionState : IDisposable
	{
		public virtual void Dispose()
		{
		}

		public bool Finished(Token token)
		{
			return OnToken(token);
		}

		public abstract ISelector Produce();

		protected abstract bool OnToken(Token token);
	}

	private sealed class NotFunctionState : FunctionState
	{
		private readonly SelectorConstructor _selector;

		public NotFunctionState(SelectorConstructor parent)
		{
			_selector = parent.CreateChild();
			_selector.IsNested = true;
		}

		protected override bool OnToken(Token token)
		{
			if (token.Type == TokenType.RoundBracketClose && _selector._state == State.Data)
			{
				return true;
			}
			_selector.Apply(token);
			return false;
		}

		public override ISelector Produce()
		{
			bool isValid = _selector.IsValid;
			ISelector result = _selector.GetResult();
			if (isValid)
			{
				return PseudoClassSelector.Create(PseudoClassNames.Not.StylesheetFunction(result.Text));
			}
			return null;
		}

		public override void Dispose()
		{
			base.Dispose();
			_selector.ToPool();
		}
	}

	private sealed class HasFunctionState : FunctionState
	{
		private readonly SelectorConstructor _nested;

		public HasFunctionState(SelectorConstructor parent)
		{
			_nested = parent.CreateChild();
		}

		protected override bool OnToken(Token token)
		{
			if (token.Type == TokenType.RoundBracketClose && _nested._state == State.Data)
			{
				return true;
			}
			_nested.Apply(token);
			return false;
		}

		public override ISelector Produce()
		{
			bool isValid = _nested.IsValid;
			ISelector result = _nested.GetResult();
			if (!isValid)
			{
				return null;
			}
			return PseudoClassSelector.Create(PseudoClassNames.Has.StylesheetFunction(result.Text));
		}

		public override void Dispose()
		{
			base.Dispose();
			_nested.ToPool();
		}
	}

	private sealed class MatchesFunctionState : FunctionState
	{
		private readonly SelectorConstructor _selector;

		public MatchesFunctionState(SelectorConstructor parent)
		{
			_selector = parent.CreateChild();
		}

		protected override bool OnToken(Token token)
		{
			if (token.Type == TokenType.RoundBracketClose && _selector._state == State.Data)
			{
				return true;
			}
			_selector.Apply(token);
			return false;
		}

		public override ISelector Produce()
		{
			bool isValid = _selector.IsValid;
			ISelector result = _selector.GetResult();
			if (!isValid)
			{
				return null;
			}
			return PseudoClassSelector.Create(PseudoClassNames.Matches.StylesheetFunction(result.Text));
		}

		public override void Dispose()
		{
			base.Dispose();
			_selector.ToPool();
		}
	}

	private sealed class DirFunctionState : FunctionState
	{
		private bool _valid = true;

		private string _value;

		protected override bool OnToken(Token token)
		{
			if (token.Type == TokenType.Ident)
			{
				_value = token.Data;
			}
			else
			{
				if (token.Type == TokenType.RoundBracketClose)
				{
					return true;
				}
				if (token.Type != TokenType.Whitespace)
				{
					_valid = false;
				}
			}
			return false;
		}

		public override ISelector Produce()
		{
			if (!_valid || _value == null)
			{
				return null;
			}
			return PseudoClassSelector.Create(PseudoClassNames.Dir.StylesheetFunction(_value));
		}
	}

	private sealed class LangFunctionState : FunctionState
	{
		private bool _valid = true;

		private string _value;

		protected override bool OnToken(Token token)
		{
			if (token.Type == TokenType.Ident)
			{
				_value = token.Data;
			}
			else
			{
				if (token.Type == TokenType.RoundBracketClose)
				{
					return true;
				}
				if (token.Type != TokenType.Whitespace)
				{
					_valid = false;
				}
			}
			return false;
		}

		public override ISelector Produce()
		{
			if (!_valid || _value == null)
			{
				return null;
			}
			return PseudoClassSelector.Create(PseudoClassNames.Lang.StylesheetFunction(_value));
		}
	}

	private sealed class ContainsFunctionState : FunctionState
	{
		private bool _valid = true;

		private string _value;

		protected override bool OnToken(Token token)
		{
			TokenType type = token.Type;
			if ((type == TokenType.String || type == TokenType.Ident) ? true : false)
			{
				_value = token.Data;
			}
			else
			{
				if (token.Type == TokenType.RoundBracketClose)
				{
					return true;
				}
				if (token.Type != TokenType.Whitespace)
				{
					_valid = false;
				}
			}
			return false;
		}

		public override ISelector Produce()
		{
			if (!_valid || _value == null)
			{
				return null;
			}
			return PseudoClassSelector.Create(PseudoClassNames.Contains.StylesheetFunction(_value));
		}
	}

	private sealed class HostContextFunctionState : FunctionState
	{
		private readonly SelectorConstructor _selector;

		public HostContextFunctionState(SelectorConstructor parent)
		{
			_selector = parent.CreateChild();
		}

		protected override bool OnToken(Token token)
		{
			if (token.Type == TokenType.RoundBracketClose && _selector._state == State.Data)
			{
				return true;
			}
			_selector.Apply(token);
			return false;
		}

		public override ISelector Produce()
		{
			bool isValid = _selector.IsValid;
			ISelector result = _selector.GetResult();
			if (!isValid)
			{
				return null;
			}
			return PseudoClassSelector.Create(PseudoClassNames.HostContext.StylesheetFunction(result.Text));
		}

		public override void Dispose()
		{
			base.Dispose();
			_selector.ToPool();
		}
	}

	private sealed class ChildFunctionState<T> : FunctionState where T : ChildSelector, ISelector, new()
	{
		private enum ParseState : byte
		{
			Initial,
			AfterInitialSign,
			Offset,
			BeforeOf,
			AfterOf
		}

		private readonly SelectorConstructor _parent;

		private readonly bool _allowOf;

		private SelectorConstructor _nested;

		private int _offset;

		private int _sign;

		private ParseState _state;

		private int _step;

		private bool _valid;

		public ChildFunctionState(SelectorConstructor parent, bool withOptionalSelector = true)
		{
			_parent = parent;
			_allowOf = withOptionalSelector;
			_valid = true;
			_sign = 1;
			_state = ParseState.Initial;
		}

		public override ISelector Produce()
		{
			int num;
			if (_valid)
			{
				SelectorConstructor nested = _nested;
				num = ((nested != null && !nested.IsValid) ? 1 : 0);
			}
			else
			{
				num = 1;
			}
			ISelector kind = _nested?.ToPool() ?? AllSelector.Create();
			if (num != 0)
			{
				return null;
			}
			return new T().With(_step, _offset, kind);
		}

		protected override bool OnToken(Token token)
		{
			return _state switch
			{
				ParseState.Initial => OnInitial(token), 
				ParseState.AfterInitialSign => OnAfterInitialSign(token), 
				ParseState.Offset => OnOffset(token), 
				ParseState.BeforeOf => OnBeforeOf(token), 
				_ => OnAfter(token), 
			};
		}

		private bool OnAfterInitialSign(Token token)
		{
			switch (token.Type)
			{
			case TokenType.Number:
				return OnOffset(token);
			case TokenType.Dimension:
			{
				UnitToken unitToken = (UnitToken)token;
				_valid = _valid && unitToken.Unit.Isi("n") && int.TryParse(token.Data, out _step);
				_step *= _sign;
				_sign = 1;
				_state = ParseState.Offset;
				return false;
			}
			case TokenType.Ident:
				if (token.Data.Isi("n"))
				{
					_step = _sign;
					_sign = 1;
					_state = ParseState.Offset;
					return false;
				}
				break;
			}
			if (_state == ParseState.Initial && token.Type == TokenType.Ident && token.Data.Isi("-n"))
			{
				_step = -1;
				_state = ParseState.Offset;
				return false;
			}
			_valid = false;
			return token.Type == TokenType.RoundBracketClose;
		}

		private bool OnAfter(Token token)
		{
			if (token.Type == TokenType.RoundBracketClose && _nested._state == State.Data)
			{
				return true;
			}
			_nested.Apply(token);
			return false;
		}

		private bool OnBeforeOf(Token token)
		{
			if (token.Type == TokenType.Whitespace)
			{
				return false;
			}
			if (token.Data.Isi(Keywords.Of))
			{
				_valid = _allowOf;
				_state = ParseState.AfterOf;
				_nested = _parent.CreateChild();
				return false;
			}
			if (token.Type == TokenType.RoundBracketClose)
			{
				return true;
			}
			_valid = false;
			return false;
		}

		private bool OnOffset(Token token)
		{
			switch (token.Type)
			{
			case TokenType.Whitespace:
				return false;
			case TokenType.Number:
				_valid = _valid && ((NumberToken)token).IsInteger && int.TryParse(token.Data, out _offset);
				_offset *= _sign;
				_state = ParseState.BeforeOf;
				return false;
			default:
				return OnBeforeOf(token);
			}
		}

		private bool OnInitial(Token token)
		{
			if (token.Type == TokenType.Whitespace)
			{
				return false;
			}
			if (token.Data.Isi(Keywords.Odd))
			{
				_state = ParseState.BeforeOf;
				_step = 2;
				_offset = 1;
				return false;
			}
			if (token.Data.Isi(Keywords.Even))
			{
				_state = ParseState.BeforeOf;
				_step = 2;
				_offset = 0;
				return false;
			}
			if (token.Type == TokenType.Delim && token.Data.IsOneOf("+", "-"))
			{
				_sign = ((!(token.Data == "-")) ? 1 : (-1));
				_state = ParseState.AfterInitialSign;
				return false;
			}
			return OnAfterInitialSign(token);
		}
	}

	private readonly Stack<Combinator> _combinators;

	private State _state;

	private ISelector _temp;

	private ListSelector _group;

	private ComplexSelector _complex;

	private string _attrName;

	private string _attrValue;

	private string _attrOp;

	private string _attrNs;

	private bool _valid;

	private bool _ready;

	private AttributeSelectorFactory _attributeSelector;

	private PseudoElementSelectorFactory _pseudoElementSelector;

	private PseudoClassSelectorFactory _pseudoClassSelector;

	private static readonly Dictionary<string, Func<SelectorConstructor, FunctionState>> PseudoClassFunctions = new Dictionary<string, Func<SelectorConstructor, FunctionState>>(StringComparer.OrdinalIgnoreCase)
	{
		{
			PseudoClassNames.NthChild,
			(SelectorConstructor ctx) => new ChildFunctionState<FirstChildSelector>(ctx)
		},
		{
			PseudoClassNames.NthLastChild,
			(SelectorConstructor ctx) => new ChildFunctionState<LastChildSelector>(ctx)
		},
		{
			PseudoClassNames.NthOfType,
			(SelectorConstructor ctx) => new ChildFunctionState<FirstTypeSelector>(ctx, withOptionalSelector: false)
		},
		{
			PseudoClassNames.NthLastOfType,
			(SelectorConstructor ctx) => new ChildFunctionState<LastTypeSelector>(ctx, withOptionalSelector: false)
		},
		{
			PseudoClassNames.NthColumn,
			(SelectorConstructor ctx) => new ChildFunctionState<FirstColumnSelector>(ctx, withOptionalSelector: false)
		},
		{
			PseudoClassNames.NthLastColumn,
			(SelectorConstructor ctx) => new ChildFunctionState<LastColumnSelector>(ctx, withOptionalSelector: false)
		},
		{
			PseudoClassNames.Not,
			(SelectorConstructor ctx) => new NotFunctionState(ctx)
		},
		{
			PseudoClassNames.Dir,
			(SelectorConstructor _) => new DirFunctionState()
		},
		{
			PseudoClassNames.Lang,
			(SelectorConstructor _) => new LangFunctionState()
		},
		{
			PseudoClassNames.Contains,
			(SelectorConstructor _) => new ContainsFunctionState()
		},
		{
			PseudoClassNames.Has,
			(SelectorConstructor ctx) => new HasFunctionState(ctx)
		},
		{
			PseudoClassNames.Matches,
			(SelectorConstructor ctx) => new MatchesFunctionState(ctx)
		},
		{
			PseudoClassNames.HostContext,
			(SelectorConstructor ctx) => new HostContextFunctionState(ctx)
		}
	};

	public bool IsValid
	{
		get
		{
			if (_valid)
			{
				return _ready;
			}
			return false;
		}
	}

	public bool IsNested { get; private set; }

	public SelectorConstructor(AttributeSelectorFactory attributeSelector, PseudoClassSelectorFactory pseudoClassSelector, PseudoElementSelectorFactory pseudoElementSelector)
	{
		_combinators = new Stack<Combinator>();
		Reset(attributeSelector, pseudoClassSelector, pseudoElementSelector);
	}

	public ISelector GetResult()
	{
		if (!IsValid)
		{
			return new UnknownSelector();
		}
		if (_complex != null)
		{
			_complex.ConcludeSelector(_temp);
			_temp = _complex;
			_complex = null;
		}
		if (_group == null || _group.Length == 0)
		{
			return _temp ?? AllSelector.Create();
		}
		if (_temp == null && _group.Length == 1)
		{
			return _group[0];
		}
		if (_temp != null)
		{
			_group.Add(_temp);
			_temp = null;
		}
		return _group;
	}

	public void Apply(Token token)
	{
		if (token.Type != TokenType.Comment)
		{
			switch (_state)
			{
			case State.Data:
				OnData(token);
				break;
			case State.Class:
				OnClass(token);
				break;
			case State.Attribute:
				OnAttribute(token);
				break;
			case State.AttributeOperator:
				OnAttributeOperator(token);
				break;
			case State.AttributeValue:
				OnAttributeValue(token);
				break;
			case State.AttributeEnd:
				OnAttributeEnd(token);
				break;
			case State.PseudoClass:
				OnPseudoClass(token);
				break;
			case State.PseudoElement:
				OnPseudoElement(token);
				break;
			default:
				_valid = false;
				break;
			}
		}
	}

	public SelectorConstructor Reset(AttributeSelectorFactory attributeSelector, PseudoClassSelectorFactory pseudoClassSelector, PseudoElementSelectorFactory pseudoElementSelector)
	{
		_attrName = null;
		_attrValue = null;
		_attrNs = null;
		_attrOp = string.Empty;
		_state = State.Data;
		_combinators.Clear();
		_temp = null;
		_group = null;
		_complex = null;
		_valid = true;
		IsNested = false;
		_ready = true;
		_attributeSelector = attributeSelector;
		_pseudoClassSelector = pseudoClassSelector;
		_pseudoElementSelector = pseudoElementSelector;
		return this;
	}

	private void OnData(Token token)
	{
		switch (token.Type)
		{
		case TokenType.SquareBracketOpen:
			_attrName = null;
			_attrValue = null;
			_attrOp = string.Empty;
			_attrNs = null;
			_state = State.Attribute;
			_ready = false;
			break;
		case TokenType.Colon:
			_state = State.PseudoClass;
			_ready = false;
			break;
		case TokenType.Hash:
			Insert(IdSelector.Create(token.Data));
			_ready = true;
			break;
		case TokenType.Ident:
			Insert(TypeSelector.Create(token.Data));
			_ready = true;
			break;
		case TokenType.Whitespace:
			Insert(Combinator.Descendent);
			break;
		case TokenType.Delim:
			OnDelim(token);
			break;
		case TokenType.GreaterThan:
			OnDelim(token);
			break;
		case TokenType.Comma:
			InsertOr();
			_ready = false;
			break;
		default:
			_valid = false;
			break;
		}
	}

	private void OnAttribute(Token token)
	{
		if (token.Type != TokenType.Whitespace)
		{
			if (token.Type == TokenType.Ident || token.Type == TokenType.String)
			{
				_state = State.AttributeOperator;
				_attrName = token.Data;
			}
			else if (token.Type == TokenType.Delim && token.Data.Is(Combinators.Pipe))
			{
				_state = State.Attribute;
				_attrNs = string.Empty;
			}
			else if (token.Type == TokenType.Delim && token.Data.Is(Keywords.Asterisk))
			{
				_state = State.AttributeOperator;
				_attrName = token.ToValue();
			}
			else
			{
				_state = State.Data;
				_valid = false;
			}
		}
	}

	private void OnAttributeOperator(Token token)
	{
		if (token.Type == TokenType.Whitespace)
		{
			return;
		}
		if (token.Type == TokenType.SquareBracketClose)
		{
			_state = State.AttributeValue;
			OnAttributeEnd(token);
		}
		else if (token.Type == TokenType.Match || token.Type == TokenType.Delim)
		{
			_state = State.AttributeValue;
			_attrOp = token.ToValue();
			if (!(_attrOp != Combinators.Pipe))
			{
				_attrNs = _attrName;
				_attrName = null;
				_attrOp = string.Empty;
				_state = State.Attribute;
			}
		}
		else
		{
			_state = State.AttributeEnd;
			_valid = false;
		}
	}

	private void OnAttributeValue(Token token)
	{
		if (token.Type != TokenType.Whitespace)
		{
			if (token.Type == TokenType.Ident || token.Type == TokenType.String || token.Type == TokenType.Number)
			{
				_state = State.AttributeEnd;
				_attrValue = token.Data;
			}
			else
			{
				_state = State.Data;
				_valid = false;
			}
		}
	}

	private void OnAttributeEnd(Token token)
	{
		if (token.Type != TokenType.Whitespace)
		{
			_state = State.Data;
			_ready = true;
			if (token.Type == TokenType.SquareBracketClose)
			{
				IAttrSelector selector = _attributeSelector.Create(_attrOp, _attrName, _attrValue, _attrNs);
				Insert(selector);
			}
			else
			{
				_valid = false;
			}
		}
	}

	private void OnPseudoClass(Token token)
	{
		_state = State.Data;
		_ready = true;
		switch (token.Type)
		{
		case TokenType.Colon:
			_state = State.PseudoElement;
			return;
		case TokenType.Function:
		{
			ISelector pseudoFunction = GetPseudoFunction(token as FunctionToken);
			if (pseudoFunction != null)
			{
				Insert(pseudoFunction);
				return;
			}
			break;
		}
		case TokenType.Ident:
		{
			ISelector selector = _pseudoClassSelector.Create(token.Data);
			if (selector != null)
			{
				Insert(selector);
				return;
			}
			break;
		}
		}
		_valid = false;
	}

	private void OnPseudoElement(Token token)
	{
		_state = State.Data;
		_ready = true;
		if (token.Type == TokenType.Ident)
		{
			ISelector selector = _pseudoElementSelector.Create(token.Data);
			if (selector != null)
			{
				_valid = _valid && !IsNested;
				Insert(selector);
				return;
			}
		}
		_valid = false;
	}

	private void OnClass(Token token)
	{
		_state = State.Data;
		_ready = true;
		if (token.Type == TokenType.Ident)
		{
			Insert(ClassSelector.Create(token.Data));
		}
		else
		{
			_valid = false;
		}
	}

	private void InsertOr()
	{
		if (_temp != null)
		{
			if (_group == null)
			{
				_group = new ListSelector();
			}
			if (_complex != null)
			{
				_complex.ConcludeSelector(_temp);
				_group.Add(_complex);
				_complex = null;
			}
			else
			{
				_group.Add(_temp);
			}
			_temp = null;
		}
	}

	private void Insert(ISelector selector)
	{
		if (_temp != null)
		{
			if (_combinators.Count == 0)
			{
				CompoundSelector compoundSelector = (_temp as CompoundSelector) ?? new CompoundSelector { _temp };
				compoundSelector.Add(selector);
				_temp = compoundSelector;
				return;
			}
			if (_complex == null)
			{
				_complex = new ComplexSelector();
			}
			Combinator combinator = GetCombinator();
			_complex.AppendSelector(_temp, combinator);
			_temp = selector;
		}
		else
		{
			_combinators.Clear();
			_temp = selector;
		}
	}

	private Combinator GetCombinator()
	{
		while (_combinators.Count > 1 && _combinators.Peek() == Combinator.Descendent)
		{
			_combinators.Pop();
		}
		if (_combinators.Count <= 1)
		{
			return _combinators.Pop();
		}
		Combinator combinator = _combinators.Pop();
		Combinator combinator2 = _combinators.Pop();
		if (combinator == Combinator.Child && combinator2 == Combinator.Child)
		{
			if (_combinators.Count == 0 || _combinators.Peek() != Combinator.Child)
			{
				combinator = Combinator.Descendent;
			}
			else if (_combinators.Pop() == Combinator.Child)
			{
				combinator = Combinator.Deep;
			}
		}
		else if (combinator == Combinator.Namespace && combinator2 == Combinator.Namespace)
		{
			combinator = Combinator.Column;
		}
		else
		{
			_combinators.Push(combinator2);
		}
		while (_combinators.Count > 0)
		{
			_valid = _combinators.Pop() == Combinator.Descendent && _valid;
		}
		return combinator;
	}

	private void Insert(Combinator combinator)
	{
		_combinators.Push(combinator);
	}

	private void OnDelim(Token token)
	{
		switch (token.Data[0])
		{
		case ',':
			InsertOr();
			_ready = false;
			break;
		case '>':
			Insert(Combinator.Child);
			_ready = false;
			break;
		case '+':
			Insert(Combinator.AdjacentSibling);
			_ready = false;
			break;
		case '~':
			Insert(Combinator.Sibling);
			_ready = false;
			break;
		case '*':
			Insert(AllSelector.Create());
			_ready = true;
			break;
		case '.':
			_state = State.Class;
			_ready = false;
			break;
		case '|':
			if (_combinators.Count > 0 && _combinators.Peek() == Combinator.Descendent)
			{
				Insert(TypeSelector.Create(string.Empty));
			}
			Insert(Combinator.Namespace);
			_ready = false;
			break;
		default:
			_valid = false;
			break;
		}
	}

	private ISelector GetPseudoFunction(FunctionToken arguments)
	{
		if (!PseudoClassFunctions.TryGetValue(arguments.Data, out var value))
		{
			return null;
		}
		FunctionState function = value(this);
		try
		{
			_ready = false;
			if (arguments.Any((Token token) => function.Finished(token)))
			{
				ISelector result = function.Produce();
				if (IsNested && function is NotFunctionState)
				{
					result = null;
				}
				_ready = true;
				return result;
			}
			return null;
		}
		finally
		{
			if (function != null)
			{
				((IDisposable)function).Dispose();
			}
		}
	}

	private SelectorConstructor CreateChild()
	{
		return Pool.NewSelectorConstructor(_attributeSelector, _pseudoClassSelector, _pseudoElementSelector);
	}
}
