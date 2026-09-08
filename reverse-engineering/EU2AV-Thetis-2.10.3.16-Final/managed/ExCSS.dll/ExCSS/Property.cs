using System.IO;

namespace ExCSS;

public abstract class Property : StylesheetNode, IProperty, IStylesheetNode, IStyleFormattable
{
	private readonly PropertyFlags _flags;

	public string Value
	{
		get
		{
			if (DeclaredValue == null)
			{
				return Keywords.Initial;
			}
			return DeclaredValue.CssText;
		}
	}

	public string Original
	{
		get
		{
			if (DeclaredValue == null)
			{
				return Keywords.Initial;
			}
			return DeclaredValue.Original.Text;
		}
	}

	public bool IsInherited
	{
		get
		{
			if ((_flags & PropertyFlags.Inherited) != PropertyFlags.Inherited || !IsInitial)
			{
				if (DeclaredValue != null)
				{
					return DeclaredValue.CssText.Is(Keywords.Inherit);
				}
				return false;
			}
			return true;
		}
	}

	public bool IsAnimatable => (_flags & PropertyFlags.Animatable) == PropertyFlags.Animatable;

	public bool IsInitial
	{
		get
		{
			if (DeclaredValue != null)
			{
				return DeclaredValue.CssText.Is(Keywords.Initial);
			}
			return true;
		}
	}

	internal bool HasValue => DeclaredValue != null;

	internal bool CanBeHashless => (_flags & PropertyFlags.Hashless) == PropertyFlags.Hashless;

	internal bool CanBeUnitless => (_flags & PropertyFlags.Unitless) == PropertyFlags.Unitless;

	public bool CanBeInherited => (_flags & PropertyFlags.Inherited) == PropertyFlags.Inherited;

	internal bool IsShorthand => (_flags & PropertyFlags.Shorthand) == PropertyFlags.Shorthand;

	public string Name { get; }

	public bool IsImportant { get; set; }

	public string CssText => this.ToCss();

	internal abstract IValueConverter Converter { get; }

	internal IPropertyValue DeclaredValue { get; set; }

	internal Property(string name, PropertyFlags flags = PropertyFlags.None)
	{
		Name = name;
		_flags = flags;
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		writer.Write(formatter.Declaration(Name, Value, IsImportant));
	}

	internal bool TrySetValue(TokenValue newTokenValue)
	{
		IPropertyValue propertyValue = Converter.Convert(newTokenValue ?? TokenValue.Initial);
		if (propertyValue == null)
		{
			return false;
		}
		DeclaredValue = propertyValue;
		return true;
	}
}
