using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExCSS.Conditions;

internal sealed class AndCondition : StylesheetNode, IConditionFunction, IStylesheetNode, IStyleFormattable
{
	public bool Check()
	{
		return base.Children.OfType<IConditionFunction>().All((IConditionFunction condition) => condition.Check());
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
				writer.Write(" and ");
			}
			item.ToCss(writer, formatter);
		}
	}
}
