using System;
using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

public static class Colors
{
	private static readonly Dictionary<string, Color> NamedColors = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
	{
		{
			"aliceblue",
			new Color(240, 248, byte.MaxValue)
		},
		{
			"antiquewhite",
			new Color(250, 235, 215)
		},
		{
			"aqua",
			new Color(0, byte.MaxValue, byte.MaxValue)
		},
		{
			"aquamarine",
			new Color(127, byte.MaxValue, 212)
		},
		{
			"azure",
			new Color(240, byte.MaxValue, byte.MaxValue)
		},
		{
			"beige",
			new Color(245, 245, 220)
		},
		{
			"bisque",
			new Color(byte.MaxValue, 228, 196)
		},
		{
			"black",
			new Color(0, 0, 0)
		},
		{
			"blanchedalmond",
			new Color(byte.MaxValue, 235, 205)
		},
		{
			"blue",
			new Color(0, 0, byte.MaxValue)
		},
		{
			"blueviolet",
			new Color(138, 43, 226)
		},
		{
			"brown",
			new Color(165, 42, 42)
		},
		{
			"burlywood",
			new Color(222, 184, 135)
		},
		{
			"cadetblue",
			new Color(95, 158, 160)
		},
		{
			"chartreuse",
			new Color(127, byte.MaxValue, 0)
		},
		{
			"chocolate",
			new Color(210, 105, 30)
		},
		{
			"coral",
			new Color(byte.MaxValue, 127, 80)
		},
		{
			"cornflowerblue",
			new Color(100, 149, 237)
		},
		{
			"cornsilk",
			new Color(byte.MaxValue, 248, 220)
		},
		{
			"crimson",
			new Color(220, 20, 60)
		},
		{
			"cyan",
			new Color(0, byte.MaxValue, byte.MaxValue)
		},
		{
			"darkblue",
			new Color(0, 0, 139)
		},
		{
			"darkcyan",
			new Color(0, 139, 139)
		},
		{
			"darkgoldenrod",
			new Color(184, 134, 11)
		},
		{
			"darkgray",
			new Color(169, 169, 169)
		},
		{
			"darkgreen",
			new Color(0, 100, 0)
		},
		{
			"darkgrey",
			new Color(169, 169, 169)
		},
		{
			"darkkhaki",
			new Color(189, 183, 107)
		},
		{
			"darkmagenta",
			new Color(139, 0, 139)
		},
		{
			"darkolivegreen",
			new Color(85, 107, 47)
		},
		{
			"darkorange",
			new Color(byte.MaxValue, 140, 0)
		},
		{
			"darkorchid",
			new Color(153, 50, 204)
		},
		{
			"darkred",
			new Color(139, 0, 0)
		},
		{
			"darksalmon",
			new Color(233, 150, 122)
		},
		{
			"darkseagreen",
			new Color(143, 188, 143)
		},
		{
			"darkslateblue",
			new Color(72, 61, 139)
		},
		{
			"darkslategray",
			new Color(47, 79, 79)
		},
		{
			"darkslategrey",
			new Color(47, 79, 79)
		},
		{
			"darkturquoise",
			new Color(0, 206, 209)
		},
		{
			"darkviolet",
			new Color(148, 0, 211)
		},
		{
			"deeppink",
			new Color(byte.MaxValue, 20, 147)
		},
		{
			"deepskyblue",
			new Color(0, 191, byte.MaxValue)
		},
		{
			"dimgray",
			new Color(105, 105, 105)
		},
		{
			"dimgrey",
			new Color(105, 105, 105)
		},
		{
			"dodgerblue",
			new Color(30, 144, byte.MaxValue)
		},
		{
			"firebrick",
			new Color(178, 34, 34)
		},
		{
			"floralwhite",
			new Color(byte.MaxValue, 250, 240)
		},
		{
			"forestgreen",
			new Color(34, 139, 34)
		},
		{
			"fuchsia",
			new Color(byte.MaxValue, 0, byte.MaxValue)
		},
		{
			"gainsboro",
			new Color(220, 220, 220)
		},
		{
			"ghostwhite",
			new Color(248, 248, byte.MaxValue)
		},
		{
			"gold",
			new Color(byte.MaxValue, 215, 0)
		},
		{
			"goldenrod",
			new Color(218, 165, 32)
		},
		{
			"gray",
			new Color(128, 128, 128)
		},
		{
			"green",
			new Color(0, 128, 0)
		},
		{
			"greenyellow",
			new Color(173, byte.MaxValue, 47)
		},
		{
			"grey",
			new Color(128, 128, 128)
		},
		{
			"honeydew",
			new Color(240, byte.MaxValue, 240)
		},
		{
			"hotpink",
			new Color(byte.MaxValue, 105, 180)
		},
		{
			"indianred",
			new Color(205, 92, 92)
		},
		{
			"indigo",
			new Color(75, 0, 130)
		},
		{
			"ivory",
			new Color(byte.MaxValue, byte.MaxValue, 240)
		},
		{
			"khaki",
			new Color(240, 230, 140)
		},
		{
			"lavender",
			new Color(230, 230, 250)
		},
		{
			"lavenderblush",
			new Color(byte.MaxValue, 240, 245)
		},
		{
			"lawngreen",
			new Color(124, 252, 0)
		},
		{
			"lemonchiffon",
			new Color(byte.MaxValue, 250, 205)
		},
		{
			"lightblue",
			new Color(173, 216, 230)
		},
		{
			"lightcoral",
			new Color(240, 128, 128)
		},
		{
			"lightcyan",
			new Color(224, byte.MaxValue, byte.MaxValue)
		},
		{
			"lightgoldenrodyellow",
			new Color(250, 250, 210)
		},
		{
			"lightgray",
			new Color(211, 211, 211)
		},
		{
			"lightgreen",
			new Color(144, 238, 144)
		},
		{
			"lightgrey",
			new Color(211, 211, 211)
		},
		{
			"lightpink",
			new Color(byte.MaxValue, 182, 193)
		},
		{
			"lightsalmon",
			new Color(byte.MaxValue, 160, 122)
		},
		{
			"lightseagreen",
			new Color(32, 178, 170)
		},
		{
			"lightskyblue",
			new Color(135, 206, 250)
		},
		{
			"lightslategray",
			new Color(119, 136, 153)
		},
		{
			"lightslategrey",
			new Color(119, 136, 153)
		},
		{
			"lightsteelblue",
			new Color(176, 196, 222)
		},
		{
			"lightyellow",
			new Color(byte.MaxValue, byte.MaxValue, 224)
		},
		{
			"lime",
			new Color(0, byte.MaxValue, 0)
		},
		{
			"limegreen",
			new Color(50, 205, 50)
		},
		{
			"linen",
			new Color(250, 240, 230)
		},
		{
			"magenta",
			new Color(byte.MaxValue, 0, byte.MaxValue)
		},
		{
			"maroon",
			new Color(128, 0, 0)
		},
		{
			"mediumaquamarine",
			new Color(102, 205, 170)
		},
		{
			"mediumblue",
			new Color(0, 0, 205)
		},
		{
			"mediumorchid",
			new Color(186, 85, 211)
		},
		{
			"mediumpurple",
			new Color(147, 112, 219)
		},
		{
			"mediumseagreen",
			new Color(60, 179, 113)
		},
		{
			"mediumslateblue",
			new Color(123, 104, 238)
		},
		{
			"mediumspringgreen",
			new Color(0, 250, 154)
		},
		{
			"mediumturquoise",
			new Color(72, 209, 204)
		},
		{
			"mediumvioletred",
			new Color(199, 21, 133)
		},
		{
			"midnightblue",
			new Color(25, 25, 112)
		},
		{
			"mintcream",
			new Color(245, byte.MaxValue, 250)
		},
		{
			"mistyrose",
			new Color(byte.MaxValue, 228, 225)
		},
		{
			"moccasin",
			new Color(byte.MaxValue, 228, 181)
		},
		{
			"navajowhite",
			new Color(byte.MaxValue, 222, 173)
		},
		{
			"navy",
			new Color(0, 0, 128)
		},
		{
			"oldlace",
			new Color(253, 245, 230)
		},
		{
			"olive",
			new Color(128, 128, 0)
		},
		{
			"olivedrab",
			new Color(107, 142, 35)
		},
		{
			"orange",
			new Color(byte.MaxValue, 165, 0)
		},
		{
			"orangered",
			new Color(byte.MaxValue, 69, 0)
		},
		{
			"orchid",
			new Color(218, 112, 214)
		},
		{
			"palegoldenrod",
			new Color(238, 232, 170)
		},
		{
			"palegreen",
			new Color(152, 251, 152)
		},
		{
			"paleturquoise",
			new Color(175, 238, 238)
		},
		{
			"palevioletred",
			new Color(219, 112, 147)
		},
		{
			"papayawhip",
			new Color(byte.MaxValue, 239, 213)
		},
		{
			"peachpuff",
			new Color(byte.MaxValue, 218, 185)
		},
		{
			"peru",
			new Color(205, 133, 63)
		},
		{
			"pink",
			new Color(byte.MaxValue, 192, 203)
		},
		{
			"plum",
			new Color(221, 160, 221)
		},
		{
			"powderblue",
			new Color(176, 224, 230)
		},
		{
			"purple",
			new Color(128, 0, 128)
		},
		{
			"rebeccapurple",
			new Color(102, 51, 153)
		},
		{
			"red",
			new Color(byte.MaxValue, 0, 0)
		},
		{
			"rosybrown",
			new Color(188, 143, 143)
		},
		{
			"royalblue",
			new Color(65, 105, 225)
		},
		{
			"saddlebrown",
			new Color(139, 69, 19)
		},
		{
			"salmon",
			new Color(250, 128, 114)
		},
		{
			"sandybrown",
			new Color(244, 164, 96)
		},
		{
			"seagreen",
			new Color(46, 139, 87)
		},
		{
			"seashell",
			new Color(byte.MaxValue, 245, 238)
		},
		{
			"sienna",
			new Color(160, 82, 45)
		},
		{
			"silver",
			new Color(192, 192, 192)
		},
		{
			"skyblue",
			new Color(135, 206, 235)
		},
		{
			"slateblue",
			new Color(106, 90, 205)
		},
		{
			"slategray",
			new Color(112, 128, 144)
		},
		{
			"slategrey",
			new Color(112, 128, 144)
		},
		{
			"snow",
			new Color(byte.MaxValue, 250, 250)
		},
		{
			"springgreen",
			new Color(0, byte.MaxValue, 127)
		},
		{
			"steelblue",
			new Color(70, 130, 180)
		},
		{
			"tan",
			new Color(210, 180, 140)
		},
		{
			"teal",
			new Color(0, 128, 128)
		},
		{
			"thistle",
			new Color(216, 191, 216)
		},
		{
			"tomato",
			new Color(byte.MaxValue, 99, 71)
		},
		{
			"turquoise",
			new Color(64, 224, 208)
		},
		{
			"violet",
			new Color(238, 130, 238)
		},
		{
			"wheat",
			new Color(245, 222, 179)
		},
		{
			"white",
			new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue)
		},
		{
			"whitesmoke",
			new Color(245, 245, 245)
		},
		{
			"yellow",
			new Color(byte.MaxValue, byte.MaxValue, 0)
		},
		{
			"yellowgreen",
			new Color(154, 205, 50)
		},
		{
			"transparent",
			new Color(0, 0, 0, 0)
		},
		{
			"activeborder",
			new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue)
		},
		{
			"activecaption",
			new Color(204, 204, 204)
		},
		{
			"appworkspace",
			new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue)
		},
		{
			"background",
			new Color(99, 99, 206)
		},
		{
			"buttonface",
			new Color(221, 221, 221)
		},
		{
			"buttonhighlight",
			new Color(221, 221, 221)
		},
		{
			"buttonshadow",
			new Color(136, 136, 136)
		},
		{
			"buttontext",
			new Color(0, 0, 0)
		},
		{
			"captiontext",
			new Color(0, 0, 0)
		},
		{
			"graytext",
			new Color(128, 128, 128)
		},
		{
			"highlight",
			new Color(181, 213, byte.MaxValue)
		},
		{
			"highlighttext",
			new Color(0, 0, 0)
		},
		{
			"inactiveborder",
			new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue)
		},
		{
			"inactivecaption",
			new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue)
		},
		{
			"inactivecaptiontext",
			new Color(127, 127, 127)
		},
		{
			"infobackground",
			new Color(251, 252, 197)
		},
		{
			"infotext",
			new Color(0, 0, 0)
		},
		{
			"menu",
			new Color(247, 247, 247)
		},
		{
			"menutext",
			new Color(0, 0, 0)
		},
		{
			"scrollbar",
			new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue)
		},
		{
			"threeddarkshadow",
			new Color(102, 102, 102)
		},
		{
			"threedface",
			new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue)
		},
		{
			"threedhighlight",
			new Color(221, 221, 221)
		},
		{
			"threedlightshadow",
			new Color(192, 192, 192)
		},
		{
			"threedshadow",
			new Color(136, 136, 136)
		},
		{
			"window",
			new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue)
		},
		{
			"windowframe",
			new Color(204, 204, 204)
		},
		{
			"windowtext",
			new Color(0, 0, 0)
		}
	};

	public static IEnumerable<string> Names => NamedColors.Keys;

	public static Color? GetColor(string name)
	{
		if (NamedColors.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public static string GetName(Color color)
	{
		return NamedColors.Where(delegate(KeyValuePair<string, Color> pair)
		{
			KeyValuePair<string, Color> keyValuePair = pair;
			return keyValuePair.Value.Equals(color);
		}).Select(delegate(KeyValuePair<string, Color> pair)
		{
			KeyValuePair<string, Color> keyValuePair = pair;
			return keyValuePair.Key;
		}).FirstOrDefault();
	}
}
