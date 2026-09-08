using System.Collections;
using System.Collections.Generic;

namespace ExCSS;

public interface IFontFaceRule : IRule, IStylesheetNode, IStyleFormattable, IProperties, IEnumerable<IProperty>, IEnumerable
{
	string Family { get; set; }

	string Source { get; set; }

	string Style { get; set; }

	string Weight { get; set; }

	string Stretch { get; set; }

	string Range { get; set; }

	string Variant { get; set; }

	string Features { get; set; }
}
