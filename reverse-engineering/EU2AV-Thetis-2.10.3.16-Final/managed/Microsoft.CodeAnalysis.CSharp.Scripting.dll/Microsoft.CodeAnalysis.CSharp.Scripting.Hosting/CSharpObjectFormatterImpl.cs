using System.Reflection;
using Microsoft.CodeAnalysis.Scripting.Hosting;

namespace Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;

internal class CSharpObjectFormatterImpl : CommonObjectFormatter
{
	protected override CommonTypeNameFormatter TypeNameFormatter { get; }

	protected override CommonPrimitiveFormatter PrimitiveFormatter { get; }

	protected override Microsoft.CodeAnalysis.Scripting.Hosting.MemberFilter Filter { get; }

	internal CSharpObjectFormatterImpl()
	{
		PrimitiveFormatter = new CSharpPrimitiveFormatter();
		TypeNameFormatter = new CSharpTypeNameFormatter(PrimitiveFormatter);
		Filter = new CSharpMemberFilter();
	}

	protected override string FormatRefKind(ParameterInfo parameter)
	{
		if (!parameter.IsOut)
		{
			return "ref";
		}
		return "out";
	}
}
