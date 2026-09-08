namespace ExCSS;

public interface IKeyframesRule : IRule, IStylesheetNode, IStyleFormattable
{
	string Name { get; set; }

	IRuleList Rules { get; }

	void Add(string rule);

	void Remove(string key);

	IKeyframeRule Find(string key);
}
