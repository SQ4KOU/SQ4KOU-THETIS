using System;
using System.IO;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal abstract class AssemblyLoaderImpl(InteractiveAssemblyLoader loader) : IDisposable
{
	internal readonly InteractiveAssemblyLoader Loader = loader;

	public static AssemblyLoaderImpl Create(InteractiveAssemblyLoader loader)
	{
		return new DesktopAssemblyLoaderImpl(loader);
	}

	public abstract Assembly LoadFromStream(Stream peStream, Stream pdbStream);

	public abstract AssemblyAndLocation LoadFromPath(string path);

	public abstract void Dispose();
}
