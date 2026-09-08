using System;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal sealed class NotImplementedAnalyzerLoader : IAnalyzerAssemblyLoader
{
	public void AddDependencyLocation(string fullPath)
	{
		throw new NotImplementedException();
	}

	public Assembly LoadFromPath(string fullPath)
	{
		throw new NotImplementedException();
	}
}
