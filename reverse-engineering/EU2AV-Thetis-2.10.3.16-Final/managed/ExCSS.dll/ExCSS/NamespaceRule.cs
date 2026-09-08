using System.IO;
using System.Linq;

namespace ExCSS;

internal sealed class NamespaceRule : Rule, INamespaceRule, IRule, IStylesheetNode, IStyleFormattable
{
	private string _namespaceUri;

	private string _prefix;

	public string NamespaceUri
	{
		get
		{
			return _namespaceUri;
		}
		set
		{
			CheckValidity();
			_namespaceUri = value ?? string.Empty;
		}
	}

	public string Prefix
	{
		get
		{
			return _prefix;
		}
		set
		{
			CheckValidity();
			_prefix = value ?? string.Empty;
		}
	}

	internal NamespaceRule(StylesheetParser parser)
		: base(RuleType.Namespace, parser)
	{
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		string text = (string.IsNullOrEmpty(_prefix) ? string.Empty : " ");
		string value = _prefix + text + _namespaceUri.StylesheetUrl();
		writer.Write(formatter.Rule("@namespace", value));
	}

	protected override void ReplaceWith(IRule rule)
	{
		NamespaceRule namespaceRule = rule as NamespaceRule;
		_namespaceUri = namespaceRule?._namespaceUri;
		_prefix = namespaceRule?._prefix;
		base.ReplaceWith(rule);
	}

	private static bool IsNotSupported(RuleType type)
	{
		if (type != RuleType.Charset && type != RuleType.Import)
		{
			return type != RuleType.Namespace;
		}
		return false;
	}

	private void CheckValidity()
	{
		RuleList ruleList = base.Owner?.Rules;
		if (ruleList == null || !ruleList.Any((IRule entry) => IsNotSupported(entry.Type)))
		{
			return;
		}
		throw new ParseException("Rule is not supported");
	}
}
