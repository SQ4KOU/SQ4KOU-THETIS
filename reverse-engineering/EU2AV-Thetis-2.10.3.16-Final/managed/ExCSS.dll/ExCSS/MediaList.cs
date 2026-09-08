using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExCSS;

public sealed class MediaList : StylesheetNode
{
	private readonly StylesheetParser _parser;

	public string this[int index] => Media.GetItemByIndex(index).ToCss();

	public IEnumerable<Medium> Media => base.Children.OfType<Medium>();

	public int Length => Media.Count();

	public string MediaText
	{
		get
		{
			return this.ToCss();
		}
		set
		{
			Clear();
			foreach (Medium item in _parser.ParseMediaList(value))
			{
				if (item == null)
				{
					throw new ParseException("Unable to parse media list element");
				}
				AppendChild(item);
			}
		}
	}

	internal MediaList(StylesheetParser parser)
	{
		_parser = parser;
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		Medium[] array = Media.ToArray();
		if (array.Length != 0)
		{
			array[0].ToCss(writer, formatter);
			for (int i = 1; i < array.Length; i++)
			{
				writer.Write(", ");
				array[i].ToCss(writer, formatter);
			}
		}
	}

	public void Add(string newMedium)
	{
		Medium medium = _parser.ParseMedium(newMedium);
		if (medium == null)
		{
			throw new ParseException("Unable to parse medium");
		}
		AppendChild(medium);
	}

	public void Remove(string oldMedium)
	{
		Medium medium = _parser.ParseMedium(oldMedium);
		if (medium == null)
		{
			throw new ParseException("Unable to parse medium");
		}
		foreach (Medium medium2 in Media)
		{
			if (medium2.Equals(medium))
			{
				RemoveChild(medium2);
				return;
			}
		}
		throw new ParseException("Media list element not found");
	}

	public IEnumerator<Medium> GetEnumerator()
	{
		return Media.GetEnumerator();
	}
}
