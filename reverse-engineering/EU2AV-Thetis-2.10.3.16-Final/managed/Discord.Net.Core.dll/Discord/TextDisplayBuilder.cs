using System;

namespace Discord;

public class TextDisplayBuilder : IMessageComponentBuilder
{
	public const int MaxContentLength = 4000;

	public ComponentType Type => ComponentType.TextDisplay;

	public int? Id { get; set; }

	public string Content { get; set; }

	public TextDisplayBuilder()
	{
	}

	public TextDisplayBuilder(string content, int? id = null)
	{
		Content = content;
		Id = id;
	}

	public TextDisplayBuilder(TextDisplayComponent textDisplay)
	{
		Content = textDisplay.Content;
		Id = textDisplay.Id;
	}

	public TextDisplayBuilder WithContent(string content)
	{
		Content = content;
		return this;
	}

	public TextDisplayComponent Build()
	{
		if (Content.Length > 4000)
		{
			throw new ArgumentException($"Content length must be less than or equal to {4000}.", "Content");
		}
		return new TextDisplayComponent(Content, Id);
	}

	IMessageComponent IMessageComponentBuilder.Build()
	{
		return Build();
	}
}
