using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Scripting.Hosting;

namespace Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;

internal class CSharpMemberFilter : CommonMemberFilter
{
	protected override bool IsGeneratedMemberName(string name)
	{
		return GeneratedNames.IsGeneratedMemberName(name);
	}
}
