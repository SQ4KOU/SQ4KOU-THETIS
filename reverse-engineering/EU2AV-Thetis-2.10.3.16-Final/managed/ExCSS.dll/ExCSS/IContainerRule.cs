namespace ExCSS;

public interface IContainerRule : IConditionRule, IGroupingRule, IRule, IStylesheetNode, IStyleFormattable, IRuleCreator
{
	string Name { get; set; }

	MediaList Media { get; }
}
