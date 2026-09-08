using System;
using System.Reflection;

namespace Microsoft.CodeAnalysis;

internal interface IAnalyzerAssemblyLoaderInternal : IAnalyzerAssemblyLoader, IDisposable
{
	bool IsHostAssembly(Assembly assembly);

	string? GetOriginalDependencyLocation(AssemblyName assembly);
}
