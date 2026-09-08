using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExCSS;

internal sealed class OrCondition : StylesheetNode, IConditionFunction, IStylesheetNode, IStyleFormattable
{
	public bool Check()
	{
		return base.Children.OfType<IConditionFunction>().Any((IConditionFunction condition) => condition.Check());
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		IEnumerable<IConditionFunction> enumerable = base.Children.OfType<IConditionFunction>();
		bool flag = true;
		foreach (IConditionFunction item in enumerable)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				writer.Write(" or ");
			}
			item.ToCss(writer, formatter);
		}
	}
}
