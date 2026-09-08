namespace ExCSS;

public interface IMediaRule : IConditionRule, IGroupingRule, IRule, IStylesheetNode, IStyleFormattable, IRuleCreator
{
	MediaList Media { get; }
}
