using System.IO;

namespace ExCSS;

public abstract class MediaFeature : StylesheetNode, IMediaFeature, IStylesheetNode, IStyleFormattable
{
	private TokenValue _tokenValue;

	private TokenType _constraintDelimiter;

	internal abstract IValueConverter Converter { get; }

	public bool IsMinimum { get; }

	public bool IsMaximum { get; }

	public string Name { get; }

	public string Value
	{
		get
		{
			if (!HasValue)
			{
				return string.Empty;
			}
			return _tokenValue.Text;
		}
	}

	public bool HasValue
	{
		get
		{
			TokenValue tokenValue = _tokenValue;
			if (tokenValue != null)
			{
				return tokenValue.Count > 0;
			}
			return false;
		}
	}

	internal MediaFeature(string name)
	{
		Name = name;
		IsMinimum = name.StartsWith("min-");
		IsMaximum = name.StartsWith("max-");
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		GetConstraintDelimiter();
		string value = (HasValue ? Value : null);
		writer.Write(formatter.Constraint(Name, value, GetConstraintDelimiter()));
	}

	private string GetConstraintDelimiter()
	{
		if (_constraintDelimiter == TokenType.Colon)
		{
			return ": ";
		}
		if (_constraintDelimiter == TokenType.GreaterThan)
		{
			return " > ";
		}
		if (_constraintDelimiter == TokenType.LessThan)
		{
			return " < ";
		}
		if (_constraintDelimiter == TokenType.Equal)
		{
			return " = ";
		}
		if (_constraintDelimiter == TokenType.GreaterThanOrEqual)
		{
			return " >= ";
		}
		if (_constraintDelimiter == TokenType.LessThanOrEqual)
		{
			return " <= ";
		}
		return ": ";
	}

	internal bool TrySetValue(TokenValue tokenValue, TokenType constraintDelimiter)
	{
		bool flag = ((tokenValue != null) ? (Converter.Convert(tokenValue) != null) : (!IsMinimum && !IsMaximum && Converter.ConvertDefault() != null));
		if (flag)
		{
			_tokenValue = tokenValue;
		}
		_constraintDelimiter = constraintDelimiter;
		return flag;
	}
}
