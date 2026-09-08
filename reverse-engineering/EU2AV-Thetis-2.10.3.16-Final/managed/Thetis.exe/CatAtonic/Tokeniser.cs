namespace CatAtonic;

public class Tokeniser
{
	private readonly string input;

	private int index;

	private readonly int length;

	public Tokeniser(string input)
	{
		this.input = input ?? string.Empty;
		index = 0;
		length = this.input.Length;
	}

	private void skip_ws_and_comments()
	{
		while (true)
		{
			if (index < length && char.IsWhiteSpace(input[index]))
			{
				index++;
				continue;
			}
			if (index < length && input[index] == '#')
			{
				while (index < length && input[index] != '\n')
				{
					index++;
				}
				if (index < length && input[index] == '\n')
				{
					index++;
				}
				continue;
			}
			break;
		}
	}

	public Token next()
	{
		skip_ws_and_comments();
		if (index >= length)
		{
			return new Token(TokenType.Eof, string.Empty);
		}
		if (input[index] == '[')
		{
			int num = index + 1;
			for (int i = num; i < length; i++)
			{
				switch (input[i])
				{
				case ']':
				{
					string text2 = input.Substring(num, i - num).Trim();
					index = i + 1;
					return new Token(TokenType.Bracket, text2);
				}
				case '\n':
				case '\r':
				case ';':
				case '[':
				{
					string text = input.Substring(num, i - num).Trim();
					if (text.Length == 0)
					{
						text = "?";
					}
					return new Token(TokenType.Error, "non completed [] at " + text);
				}
				}
			}
			return new Token(TokenType.Error, "non completed []");
		}
		for (int j = index; j < length; j++)
		{
			switch (input[j])
			{
			case ';':
			{
				string text3 = input.Substring(index, j - index + 1).Trim();
				index = j + 1;
				return new Token(TokenType.Cat, text3);
			}
			case '\n':
			case '\r':
			case '#':
			case '[':
			case ']':
				return new Token(TokenType.Error, "non terminated cat message in a ;");
			}
		}
		return new Token(TokenType.Error, "non terminated cat message in a ;");
	}
}
