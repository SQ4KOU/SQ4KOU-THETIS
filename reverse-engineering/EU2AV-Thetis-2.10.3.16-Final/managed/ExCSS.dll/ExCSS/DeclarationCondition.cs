using System.IO;

namespace ExCSS;

internal sealed class DeclarationCondition : StylesheetNode, IConditionFunction, IStylesheetNode, IStyleFormattable
{
	private readonly Property _property;

	private readonly TokenValue _tokenValue;

	public DeclarationCondition(Property property, TokenValue tokenValue)
	{
		_property = property;
		_tokenValue = tokenValue;
	}

	public bool Check()
	{
		if (!(_property is UnknownProperty))
		{
			return _property.TrySetValue(_tokenValue);
		}
		return false;
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		string text = formatter.Declaration(_property.Name, _tokenValue.Text, _property.IsImportant);
		writer.Write("(" + text + ")");
	}
}
