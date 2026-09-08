using System;
using System.IO;
using System.Reflection;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal sealed class DesktopAssemblyLoaderImpl : AssemblyLoaderImpl
{
	public DesktopAssemblyLoaderImpl(InteractiveAssemblyLoader loader)
		: base(loader)
	{
		AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
	}

	public override void Dispose()
	{
		AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
	}

	private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
	{
		return Loader.ResolveAssembly(args.Name, args.RequestingAssembly);
	}

	public override Assembly LoadFromStream(Stream peStream, Stream pdbStream)
	{
		byte[] array = new byte[peStream.Length];
		peStream.TryReadAll(array, 0, array.Length);
		if (pdbStream != null)
		{
			byte[] array2 = new byte[pdbStream.Length];
			pdbStream.TryReadAll(array2, 0, array2.Length);
			return Assembly.Load(array, array2);
		}
		return Assembly.Load(array);
	}

	public override AssemblyAndLocation LoadFromPath(string path)
	{
		Assembly assembly = Assembly.LoadFile(path);
		return new AssemblyAndLocation(assembly, assembly.Location, assembly.GlobalAssemblyCache);
	}
}
