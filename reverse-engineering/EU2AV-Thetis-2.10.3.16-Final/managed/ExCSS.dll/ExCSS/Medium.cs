using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExCSS;

public sealed class Medium : StylesheetNode
{
	public IEnumerable<MediaFeature> Features => base.Children.OfType<MediaFeature>();

	public string Type { get; internal set; }

	public bool IsExclusive { get; internal set; }

	public bool IsInverse { get; internal set; }

	public string Constraints
	{
		get
		{
			IEnumerable<string> values = Features.Select((MediaFeature m) => m.ToCss());
			return string.Join(" and ", values);
		}
	}

	public override bool Equals(object obj)
	{
		if (obj is Medium medium && medium.IsExclusive == IsExclusive && medium.IsInverse == IsInverse && medium.Type.Is(Type) && medium.Features.Count() == Features.Count())
		{
			return medium.Features.Select((MediaFeature feature) => Features.Any((MediaFeature m) => m.Name.Is(feature.Name) && m.Value.Is(feature.Value))).All((bool isShared) => isShared);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		writer.Write(formatter.Medium(IsExclusive, IsInverse, Type, Features));
	}
}
