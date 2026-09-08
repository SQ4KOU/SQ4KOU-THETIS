namespace ExCSS;

public interface IConditionFunction : IStylesheetNode, IStyleFormattable
{
	bool Check();
}
