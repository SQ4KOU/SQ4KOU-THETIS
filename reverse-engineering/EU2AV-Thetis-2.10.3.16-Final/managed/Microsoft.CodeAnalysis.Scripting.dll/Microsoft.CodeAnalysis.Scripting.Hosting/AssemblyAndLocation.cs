using System.Reflection;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal readonly record struct AssemblyAndLocation(Assembly Assembly, string Location, bool GlobalAssemblyCache)
{
	public bool IsDefault => Assembly == null;

	public override string ToString()
	{
		return Assembly?.ToString() + " @ " + (GlobalAssemblyCache ? "<GAC>" : Location);
	}
}
