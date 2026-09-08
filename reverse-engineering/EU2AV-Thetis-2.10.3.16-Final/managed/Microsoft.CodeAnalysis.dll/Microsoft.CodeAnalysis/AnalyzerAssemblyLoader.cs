using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class AnalyzerAssemblyLoader : IAnalyzerAssemblyLoaderInternal, IAnalyzerAssemblyLoader, IDisposable
{
	private readonly object _guard = new object();

	internal static readonly StringComparer OriginalPathComparer = StringComparer.Ordinal;

	internal static readonly StringComparer GeneratedPathComparer = StringComparer.Ordinal;

	internal static readonly (StringComparer Comparer, StringComparison Comparison) SimpleNameComparer = (Comparer: StringComparer.OrdinalIgnoreCase, Comparison: StringComparison.OrdinalIgnoreCase);

	private readonly Dictionary<string, (IAnalyzerPathResolver? Resolver, string ResolvedPath, AssemblyName? AssemblyName)> _originalPathInfoMap = new Dictionary<string, (IAnalyzerPathResolver, string, AssemblyName)>(OriginalPathComparer);

	private readonly Dictionary<string, HashSet<string>> _assemblySimpleNameToOriginalPathListMap = new Dictionary<string, HashSet<string>>(SimpleNameComparer.Comparer);

	private readonly Dictionary<string, string> _resolvedToOriginalPathMap = new Dictionary<string, string>(GeneratedPathComparer);

	private bool _isDisposed;

	private bool _hookedAssemblyResolve;

	public ImmutableArray<IAnalyzerPathResolver> AnalyzerPathResolvers { get; }

	private Assembly Load(AssemblyName assemblyName, string resolvedPath)
	{
		EnsureResolvedHooked();
		return AppDomain.CurrentDomain.Load(assemblyName);
	}

	private bool IsMatch(AssemblyName requestedName, AssemblyName candidateName)
	{
		if (candidateName.Name == requestedName.Name && candidateName.Version >= requestedName.Version)
		{
			return System.MemoryExtensions.AsSpan(candidateName.GetPublicKeyToken()).SequenceEqual(System.MemoryExtensions.AsSpan(requestedName.GetPublicKeyToken()));
		}
		return false;
	}

	private void CheckIfDisposed()
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
	}

	public void Dispose()
	{
		if (!_isDisposed)
		{
			_isDisposed = true;
			DisposeWorker();
		}
	}

	private void DisposeWorker()
	{
		EnsureResolvedUnhooked();
	}

	public void AddDependencyLocation(string originalPath)
	{
		CheckIfDisposed();
		CompilerPathUtilities.RequireAbsolutePath(originalPath, "originalPath");
		lock (_guard)
		{
			if (_originalPathInfoMap.ContainsKey(originalPath))
			{
				return;
			}
		}
		string fileName = PathUtilities.GetFileName(originalPath, includeExtension: false);
		string text = originalPath;
		IAnalyzerPathResolver analyzerPathResolver = null;
		foreach (IAnalyzerPathResolver analyzerPathResolver2 in AnalyzerPathResolvers)
		{
			if (analyzerPathResolver2.IsAnalyzerPathHandled(originalPath))
			{
				analyzerPathResolver = analyzerPathResolver2;
				text = analyzerPathResolver.GetResolvedAnalyzerPath(originalPath);
				break;
			}
		}
		AssemblyName item = readAssemblyName(text);
		lock (_guard)
		{
			if (DictionaryExtensions.TryAdd(_originalPathInfoMap, originalPath, (analyzerPathResolver, text, item)))
			{
				DictionaryExtensions.TryAdd(_resolvedToOriginalPathMap, text, originalPath);
				if (!_assemblySimpleNameToOriginalPathListMap.TryGetValue(fileName, out HashSet<string> value))
				{
					value = new HashSet<string>(OriginalPathComparer);
					_assemblySimpleNameToOriginalPathListMap[fileName] = value;
				}
				value.Add(originalPath);
			}
		}
		static AssemblyName? readAssemblyName(string filePath)
		{
			try
			{
				return AssemblyName.GetAssemblyName(filePath);
			}
			catch
			{
				return null;
			}
		}
	}

	public Assembly LoadFromPath(string originalPath)
	{
		CheckIfDisposed();
		CompilerPathUtilities.RequireAbsolutePath(originalPath, "originalPath");
		var (resolvedPath, assemblyName) = GetResolvedAnalyzerPathAndName(originalPath);
		if (assemblyName == null)
		{
			throw new ArgumentException("Not a valid assembly: " + originalPath);
		}
		try
		{
			return Load(assemblyName, resolvedPath);
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException("Unable to load " + assemblyName.Name + ": " + ex.Message, ex);
		}
	}

	private (string ResolvedPath, AssemblyName? AssemblyName) GetResolvedAnalyzerPathAndName(string originalPath)
	{
		CheckIfDisposed();
		lock (_guard)
		{
			if (!_originalPathInfoMap.TryGetValue(originalPath, out (IAnalyzerPathResolver, string, AssemblyName) value))
			{
				throw new ArgumentException("Path not registered: " + originalPath, "originalPath");
			}
			return (ResolvedPath: value.Item2, AssemblyName: value.Item3);
		}
	}

	public string GetResolvedAnalyzerPath(string originalPath)
	{
		return GetResolvedAnalyzerPathAndName(originalPath).ResolvedPath;
	}

	public string? GetResolvedSatellitePath(string originalPath, CultureInfo cultureInfo)
	{
		CheckIfDisposed();
		IAnalyzerPathResolver analyzerPathResolver;
		lock (_guard)
		{
			if (!_originalPathInfoMap.TryGetValue(originalPath, out (IAnalyzerPathResolver, string, AssemblyName) value))
			{
				throw new ArgumentException("Path not registered: " + originalPath, "originalPath");
			}
			(analyzerPathResolver, _, _) = value;
		}
		if (analyzerPathResolver != null)
		{
			return analyzerPathResolver.GetResolvedSatellitePath(originalPath, cultureInfo);
		}
		return GetSatelliteAssemblyPath(originalPath, cultureInfo);
	}

	private string? GetSatelliteLoadPath(string resolvedPath, CultureInfo cultureInfo)
	{
		string value;
		lock (_guard)
		{
			if (!_resolvedToOriginalPathMap.TryGetValue(resolvedPath, out value))
			{
				return null;
			}
		}
		return GetResolvedSatellitePath(value, cultureInfo);
	}

	internal static string? GetSatelliteAssemblyPath(string assemblyFilePath, CultureInfo cultureInfo)
	{
		string path = Path.ChangeExtension(Path.GetFileName(assemblyFilePath), ".resources.dll");
		string directoryName = Path.GetDirectoryName(assemblyFilePath);
		if (directoryName == null)
		{
			return null;
		}
		while (cultureInfo != CultureInfo.InvariantCulture)
		{
			string text = Path.Combine(directoryName, cultureInfo.Name, path);
			if (File.Exists(text))
			{
				return text;
			}
			cultureInfo = cultureInfo.Parent;
		}
		return null;
	}

	public string? GetOriginalDependencyLocation(AssemblyName assemblyName)
	{
		CheckIfDisposed();
		return GetBestResolvedPath(assemblyName).BestOriginalPath;
	}

	private (string? BestOriginalPath, string? BestResolvedPath) GetBestResolvedPath(AssemblyName requestedName)
	{
		CheckIfDisposed();
		if (requestedName.Name == null)
		{
			return (BestOriginalPath: null, BestResolvedPath: null);
		}
		List<string> list;
		lock (_guard)
		{
			if (!_assemblySimpleNameToOriginalPathListMap.TryGetValue(requestedName.Name, out HashSet<string> value))
			{
				return (BestOriginalPath: null, BestResolvedPath: null);
			}
			list = value.OrderBy((string x) => x).ToList();
		}
		string item = null;
		string item2 = null;
		AssemblyName assemblyName = null;
		foreach (string item3 in list)
		{
			var (text, assemblyName2) = GetResolvedAnalyzerPathAndName(item3);
			if (assemblyName2 != null && IsMatch(requestedName, assemblyName2))
			{
				if (assemblyName2.Version == requestedName.Version)
				{
					return (BestOriginalPath: item3, BestResolvedPath: text);
				}
				if (assemblyName == null || assemblyName2.Version > assemblyName.Version)
				{
					item2 = item3;
					item = text;
					assemblyName = assemblyName2;
				}
			}
		}
		return (BestOriginalPath: item2, BestResolvedPath: item);
	}

	internal ImmutableArray<(string OriginalAssemblyPath, string ResolvedAssemblyPath)> GetPathMapSnapshot()
	{
		CheckIfDisposed();
		lock (_guard)
		{
			return _resolvedToOriginalPathMap.Select<KeyValuePair<string, string>, (string, string)>((KeyValuePair<string, string> x) => (Value: x.Value, Key: x.Key)).ToImmutableArray();
		}
	}

	internal static IAnalyzerAssemblyLoaderInternal CreateNonLockingLoader(string windowsShadowPath, ImmutableArray<IAnalyzerPathResolver> pathResolvers = default(ImmutableArray<IAnalyzerPathResolver>))
	{
		CodeAnalysisEventSource.Log.CreateNonLockingLoader(windowsShadowPath);
		pathResolvers = pathResolvers.NullToEmpty();
		ImmutableArray<IAnalyzerPathResolver> immutableArray = pathResolvers;
		int num = 0;
		IAnalyzerPathResolver[] array = new IAnalyzerPathResolver[2 + immutableArray.Length];
		ReadOnlySpan<IAnalyzerPathResolver> readOnlySpan = immutableArray.AsSpan();
		readOnlySpan.CopyTo(new Span<IAnalyzerPathResolver>(array).Slice(num, readOnlySpan.Length));
		num += readOnlySpan.Length;
		array[num] = ProgramFilesAnalyzerPathResolver.Instance;
		num++;
		array[num] = new ShadowCopyAnalyzerPathResolver(windowsShadowPath);
		return new AnalyzerAssemblyLoader(ImmutableCollectionsMarshal.AsImmutableArray(array));
	}

	internal AnalyzerAssemblyLoader()
		: this(ImmutableArray<IAnalyzerPathResolver>.Empty)
	{
	}

	internal AnalyzerAssemblyLoader(ImmutableArray<IAnalyzerPathResolver> analyzerPathResolvers)
	{
		AnalyzerPathResolvers = analyzerPathResolvers;
	}

	public bool IsHostAssembly(Assembly assembly)
	{
		CheckIfDisposed();
		if (assembly.GlobalAssemblyCache)
		{
			return true;
		}
		string directoryName = Path.GetDirectoryName(typeof(AnalyzerAssemblyLoader).Assembly.Location);
		if (PathUtilities.Comparer.Equals(directoryName, Path.GetDirectoryName(assembly.Location)))
		{
			return true;
		}
		return false;
	}

	internal bool EnsureResolvedHooked()
	{
		CheckIfDisposed();
		lock (_guard)
		{
			if (!_hookedAssemblyResolve)
			{
				AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
				_hookedAssemblyResolve = true;
				return true;
			}
		}
		return false;
	}

	internal bool EnsureResolvedUnhooked()
	{
		lock (_guard)
		{
			if (_hookedAssemblyResolve)
			{
				AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve;
				_hookedAssemblyResolve = false;
				return true;
			}
		}
		return false;
	}

	private Assembly? AssemblyResolve(object sender, ResolveEventArgs args)
	{
		try
		{
			AssemblyName assemblyName = new AssemblyName(args.Name);
			string name = assemblyName.Name;
			string text2;
			if (assemblyName.CultureInfo != null && name.EndsWith(".resources", SimpleNameComparer.Comparison))
			{
				string text = name;
				int length = ".resources".Length;
				assemblyName.Name = text.Substring(0, text.Length - length);
				string item = GetBestResolvedPath(assemblyName).BestResolvedPath;
				text2 = ((item != null) ? GetSatelliteLoadPath(item, assemblyName.CultureInfo) : null);
			}
			else
			{
				text2 = GetBestResolvedPath(assemblyName).BestResolvedPath;
			}
			if (text2 != null)
			{
				return Assembly.LoadFrom(text2);
			}
			return null;
		}
		catch
		{
			return null;
		}
	}
}
