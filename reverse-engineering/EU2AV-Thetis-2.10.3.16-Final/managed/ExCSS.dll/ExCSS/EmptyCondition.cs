using System.IO;

namespace ExCSS;

internal sealed class EmptyCondition : StylesheetNode, IConditionFunction, IStylesheetNode, IStyleFormattable
{
	public bool Check()
	{
		return true;
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
	}
}
