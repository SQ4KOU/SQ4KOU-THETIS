using System;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

public abstract class ObjectFormatter
{
	public string FormatObject(object obj)
	{
		return FormatObject(obj, new PrintOptions());
	}

	public abstract string FormatObject(object obj, PrintOptions options);

	public abstract string FormatException(Exception e);
}
