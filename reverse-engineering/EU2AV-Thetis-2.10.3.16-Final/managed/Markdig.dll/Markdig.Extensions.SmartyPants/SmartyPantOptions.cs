using System.Collections.Generic;

namespace Markdig.Extensions.SmartyPants;

public class SmartyPantOptions
{
	public Dictionary<SmartyPantType, string> Mapping { get; }

	public SmartyPantOptions()
	{
		Mapping = new Dictionary<SmartyPantType, string>
		{
			{
				SmartyPantType.Quote,
				"'"
			},
			{
				SmartyPantType.DoubleQuote,
				"\""
			},
			{
				SmartyPantType.LeftQuote,
				"&lsquo;"
			},
			{
				SmartyPantType.RightQuote,
				"&rsquo;"
			},
			{
				SmartyPantType.LeftDoubleQuote,
				"&ldquo;"
			},
			{
				SmartyPantType.RightDoubleQuote,
				"&rdquo;"
			},
			{
				SmartyPantType.LeftAngleQuote,
				"&laquo;"
			},
			{
				SmartyPantType.RightAngleQuote,
				"&raquo;"
			},
			{
				SmartyPantType.Ellipsis,
				"&hellip;"
			},
			{
				SmartyPantType.Dash2,
				"&ndash;"
			},
			{
				SmartyPantType.Dash3,
				"&mdash;"
			}
		};
	}
}
