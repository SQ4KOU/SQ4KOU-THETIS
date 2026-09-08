using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExCSS;

internal abstract class DeclarationRule : Rule, IProperties, IEnumerable<IProperty>, IEnumerable
{
	private struct FormatTransporter(IEnumerable<Property> properties) : IStyleFormattable
	{
		private readonly IEnumerable<Property> _properties = properties.Where((Property m) => m.HasValue);

		public void ToCss(TextWriter writer, IStyleFormatter formatter)
		{
			IEnumerable<string> declarations = _properties.Select((Property m) => m.ToCss(formatter));
			string value = formatter.Declarations(declarations);
			writer.Write(value);
		}
	}

	private readonly string _name;

	public string this[string propertyName] => GetValue(propertyName);

	public IEnumerable<Property> Declarations => base.Children.OfType<Property>();

	public int Length => Declarations.Count();

	internal DeclarationRule(RuleType type, string name, StylesheetParser parser)
		: base(type, parser)
	{
		_name = name;
	}

	internal void SetProperty(Property property)
	{
		foreach (Property declaration in Declarations)
		{
			if (declaration.Name.Is(property.Name))
			{
				ReplaceChild(declaration, property);
				return;
			}
		}
		AppendChild(property);
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		string value = formatter.Style(rules: new FormatTransporter(Declarations), selector: "@" + _name);
		writer.Write(value);
	}

	public string GetPropertyValue(string propertyName)
	{
		return GetValue(propertyName);
	}

	public string GetPropertyPriority(string propertyName)
	{
		return null;
	}

	public void SetProperty(string propertyName, string propertyValue, string priority = null)
	{
		SetValue(propertyName, propertyValue);
	}

	public string RemoveProperty(string propertyName)
	{
		foreach (Property declaration in Declarations)
		{
			if (declaration.HasValue && declaration.Name.Is(propertyName))
			{
				string value = declaration.Value;
				RemoveChild(declaration);
				return value;
			}
		}
		return null;
	}

	public IEnumerator<IProperty> GetEnumerator()
	{
		return Declarations.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	protected abstract Property CreateNewProperty(string name);

	protected string GetValue(string propertyName)
	{
		foreach (Property declaration in Declarations)
		{
			if (declaration.HasValue && declaration.Name.Is(propertyName))
			{
				return declaration.Value;
			}
		}
		return string.Empty;
	}

	protected void SetValue(string propertyName, string valueText)
	{
		foreach (Property declaration in Declarations)
		{
			if (declaration.Name.Is(propertyName))
			{
				TokenValue newTokenValue = base.Parser.ParseValue(valueText);
				declaration.TrySetValue(newTokenValue);
				return;
			}
		}
		Property property = CreateNewProperty(propertyName);
		if (property != null)
		{
			TokenValue newTokenValue2 = base.Parser.ParseValue(valueText);
			property.TrySetValue(newTokenValue2);
			AppendChild(property);
		}
	}
}
