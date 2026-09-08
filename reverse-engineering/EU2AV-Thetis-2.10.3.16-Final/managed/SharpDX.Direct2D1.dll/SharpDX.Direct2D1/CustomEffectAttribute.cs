using System;

namespace SharpDX.Direct2D1;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class CustomEffectAttribute : Attribute
{
	private string description;

	private string category;

	private string author;

	public string DisplayName { get; set; }

	public string Description => description;

	public string Category => category;

	public string Author => author;

	public CustomEffectAttribute(string description, string category, string author)
	{
		this.description = description;
		this.category = category;
		this.author = author;
	}
}
