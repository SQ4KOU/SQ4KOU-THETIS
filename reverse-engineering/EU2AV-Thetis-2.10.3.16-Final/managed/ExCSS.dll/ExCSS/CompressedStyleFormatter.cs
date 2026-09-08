using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ExCSS;

public sealed class CompressedStyleFormatter : IStyleFormatter
{
	public static readonly IStyleFormatter Instance = new CompressedStyleFormatter();

	string IStyleFormatter.Declaration(string name, string value, bool important)
	{
		string text = value + (important ? " !important" : string.Empty);
		return name + ": " + text;
	}

	string IStyleFormatter.Constraint(string name, string value, string constraintDelimiter)
	{
		string text = ((value != null) ? (constraintDelimiter + value) : string.Empty);
		return "(" + name + text + ")";
	}

	string IStyleFormatter.Rule(string name, string value)
	{
		return name + " " + value + ";";
	}

	string IStyleFormatter.Rule(string name, string prelude, string rules)
	{
		string text = (string.IsNullOrEmpty(prelude) ? string.Empty : (prelude + " "));
		return name + " " + text + rules;
	}

	string IStyleFormatter.Style(string selector, IStyleFormattable rules)
	{
		StringBuilder stringBuilder = Pool.NewStringBuilder().Append(selector).Append(" { ");
		int length = stringBuilder.Length;
		using (StringWriter writer = new StringWriter(stringBuilder))
		{
			rules.ToCss(writer, this);
		}
		if (stringBuilder.Length > length)
		{
			stringBuilder.Append(' ');
		}
		return stringBuilder.Append('}').ToPool();
	}

	string IStyleFormatter.Comment(string data)
	{
		return string.Join("/* ", data, " */");
	}

	string IStyleFormatter.Sheet(IEnumerable<IStyleFormattable> rules)
	{
		StringBuilder sb = Pool.NewStringBuilder();
		WriteJoined(sb, rules, Environment.NewLine);
		return sb.ToPool();
	}

	string IStyleFormatter.Block(IEnumerable<IStyleFormattable> rules)
	{
		StringBuilder stringBuilder = Pool.NewStringBuilder().Append('{');
		using (StringWriter stringWriter = new StringWriter(stringBuilder))
		{
			foreach (IStyleFormattable rule in rules)
			{
				stringWriter.Write(' ');
				rule.ToCss(stringWriter, this);
			}
		}
		return stringBuilder.Append(' ').Append('}').ToPool();
	}

	string IStyleFormatter.Declarations(IEnumerable<string> declarations)
	{
		return string.Join("; ", declarations);
	}

	string IStyleFormatter.Medium(bool exclusive, bool inverse, string type, IEnumerable<IStyleFormattable> constraints)
	{
		StringBuilder stringBuilder = Pool.NewStringBuilder();
		bool first = true;
		if (exclusive)
		{
			stringBuilder.Append("only ");
		}
		else if (inverse)
		{
			stringBuilder.Append("not ");
		}
		if (!string.IsNullOrEmpty(type))
		{
			stringBuilder.Append(type);
			first = false;
		}
		WriteJoined(stringBuilder, constraints, " and ", first);
		return stringBuilder.ToPool();
	}

	private void WriteJoined(StringBuilder sb, IEnumerable<IStyleFormattable> elements, string separator, bool first = true)
	{
		using StringWriter stringWriter = new StringWriter(sb);
		foreach (IStyleFormattable element in elements)
		{
			if (first)
			{
				first = false;
			}
			else
			{
				stringWriter.Write(separator);
			}
			element.ToCss(stringWriter, this);
		}
	}
}
