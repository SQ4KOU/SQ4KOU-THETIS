using System;
using Microsoft.CodeAnalysis.Scripting.Hosting;

namespace Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;

public sealed class CSharpObjectFormatter : ObjectFormatter
{
	private static readonly ObjectFormatter s_impl = new CSharpObjectFormatterImpl();

	public static CSharpObjectFormatter Instance { get; } = new CSharpObjectFormatter();

	private CSharpObjectFormatter()
	{
	}

	public override string FormatObject(object obj, PrintOptions options)
	{
		return s_impl.FormatObject(obj, options);
	}

	public override string FormatException(Exception e)
	{
		return s_impl.FormatException(e);
	}
}
