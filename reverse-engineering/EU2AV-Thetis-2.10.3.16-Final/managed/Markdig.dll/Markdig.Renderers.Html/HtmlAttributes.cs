using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Markdig.Helpers;
using Markdig.Syntax;

namespace Markdig.Renderers.Html;

public class HtmlAttributes : MarkdownObject
{
	public string? Id { get; set; }

	public List<string>? Classes { get; set; }

	public List<KeyValuePair<string, string?>>? Properties { get; set; }

	public void AddClass(string name)
	{
		if (name == null)
		{
			ThrowHelper.ArgumentNullException_name();
		}
		if (Classes == null)
		{
			List<string> list = (Classes = new List<string>(2));
		}
		if (!Classes.Contains(name))
		{
			Classes.Add(name);
		}
	}

	public void AddProperty(string name, string value)
	{
		if (name == null)
		{
			ThrowHelper.ArgumentNullException_name();
		}
		if (Properties == null)
		{
			List<KeyValuePair<string, string>> list = (Properties = new List<KeyValuePair<string, string>>(2));
		}
		Properties.Add(new KeyValuePair<string, string>(name, value));
	}

	public void AddPropertyIfNotExist(string name, object? value)
	{
		if (name == null)
		{
			ThrowHelper.ArgumentNullException_name();
		}
		if (Properties == null)
		{
			Properties = new List<KeyValuePair<string, string>>(4);
		}
		else
		{
			for (int i = 0; i < Properties.Count; i++)
			{
				if (Properties[i].Key.Equals(name, StringComparison.Ordinal))
				{
					return;
				}
			}
		}
		Properties.Add(new KeyValuePair<string, string>(name, (value == null) ? null : Convert.ToString(value, CultureInfo.InvariantCulture)));
	}

	public void CopyTo(HtmlAttributes htmlAttributes, bool mergeIdAndProperties = false, bool shared = true)
	{
		if (htmlAttributes == null)
		{
			ThrowHelper.ArgumentNullException("htmlAttributes");
		}
		if (!mergeIdAndProperties || Id != null)
		{
			htmlAttributes.Id = Id;
		}
		if (htmlAttributes.Classes == null)
		{
			htmlAttributes.Classes = (shared ? Classes : ((Classes != null) ? new List<string>(Classes) : null));
		}
		else if (Classes != null)
		{
			htmlAttributes.Classes.AddRange(Classes);
		}
		if (htmlAttributes.Properties == null)
		{
			htmlAttributes.Properties = (List<KeyValuePair<string, string?>>?)(shared ? ((IList)Properties) : ((IList)((Properties != null) ? new List<KeyValuePair<string, string>>(Properties) : null)));
		}
		else
		{
			if (Properties == null)
			{
				return;
			}
			if (mergeIdAndProperties)
			{
				foreach (KeyValuePair<string, string> property in Properties)
				{
					htmlAttributes.AddPropertyIfNotExist(property.Key, property.Value);
				}
				return;
			}
			htmlAttributes.Properties.AddRange(Properties);
		}
	}
}
