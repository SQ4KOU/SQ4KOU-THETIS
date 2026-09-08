using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis;

internal sealed class ShadowCopyAnalyzerPathResolver : IAnalyzerPathResolver
{
	private enum DirectoryCleanupState
	{
		InProgress,
		Completed
	}

	private static readonly ConcurrentDictionary<string, DirectoryCleanupState> s_directoryCleanupStates = new ConcurrentDictionary<string, DirectoryCleanupState>(AnalyzerAssemblyLoader.OriginalPathComparer);

	private int _directoryCount;

	internal string BaseDirectory { get; }

	internal string ShadowDirectory { get; }

	private Mutex Mutex { get; }

	internal Task DeleteLeftoverDirectoriesTask { get; }

	private ConcurrentDictionary<string, int> OriginalDirectoryMap { get; } = new ConcurrentDictionary<string, int>(AnalyzerAssemblyLoader.OriginalPathComparer);

	private ConcurrentDictionary<string, Task<string>> CopyMap { get; } = new ConcurrentDictionary<string, Task<string>>(AnalyzerAssemblyLoader.OriginalPathComparer);

	internal int CopyCount => CopyMap.Count;

	public ShadowCopyAnalyzerPathResolver(string baseDirectory)
	{
		if (baseDirectory == null)
		{
			throw new ArgumentNullException("baseDirectory");
		}
		if (!Path.IsPathRooted(baseDirectory))
		{
			throw new ArgumentException("Must be a full path: " + baseDirectory, "baseDirectory");
		}
		BaseDirectory = baseDirectory;
		string text = Guid.NewGuid().ToString("N").ToLowerInvariant();
		ShadowDirectory = Path.Combine(BaseDirectory, text);
		Mutex = new Mutex(initiallyOwned: false, text);
		DeleteLeftoverDirectoriesTask = Task.Run((Action)DeleteLeftoverDirectories);
	}

	private void DeleteLeftoverDirectories()
	{
		if (!s_directoryCleanupStates.TryAdd(BaseDirectory, DirectoryCleanupState.InProgress))
		{
			SpinWait.SpinUntil(() => s_directoryCleanupStates[BaseDirectory] == DirectoryCleanupState.Completed, -1);
			return;
		}
		try
		{
			if (!Directory.Exists(BaseDirectory))
			{
				return;
			}
			IEnumerable<string> enumerable;
			try
			{
				enumerable = Directory.EnumerateDirectories(BaseDirectory);
			}
			catch (DirectoryNotFoundException)
			{
				return;
			}
			foreach (string item in enumerable)
			{
				string name = Path.GetFileName(item).ToLowerInvariant();
				Mutex result = null;
				try
				{
					if (System.Threading.Mutex.TryOpenExisting(name, out result))
					{
						continue;
					}
					try
					{
						if (Directory.Exists(item))
						{
							try
							{
								Directory.Delete(item, recursive: true);
							}
							catch (DirectoryNotFoundException)
							{
							}
						}
					}
					catch (IOException)
					{
						ClearReadOnlyFlagOnFiles(item);
						Directory.Delete(item, recursive: true);
					}
				}
				catch
				{
				}
				finally
				{
					result?.Dispose();
				}
			}
		}
		finally
		{
			s_directoryCleanupStates[BaseDirectory] = DirectoryCleanupState.Completed;
		}
	}

	public bool IsAnalyzerPathHandled(string analyzerFilePath)
	{
		return true;
	}

	public string GetResolvedAnalyzerPath(string originalAnalyzerPath)
	{
		string text = Path.Combine(GetAnalyzerShadowDirectory(originalAnalyzerPath), Path.GetFileName(originalAnalyzerPath));
		ShadowCopyFile(originalAnalyzerPath, text);
		return text;
	}

	public string? GetResolvedSatellitePath(string originalAnalyzerPath, CultureInfo cultureInfo)
	{
		string satelliteAssemblyPath = AnalyzerAssemblyLoader.GetSatelliteAssemblyPath(originalAnalyzerPath, cultureInfo);
		if (satelliteAssemblyPath == null)
		{
			return null;
		}
		string analyzerShadowDirectory = GetAnalyzerShadowDirectory(originalAnalyzerPath);
		string fileName = Path.GetFileName(satelliteAssemblyPath);
		string fileName2 = Path.GetFileName(Path.GetDirectoryName(satelliteAssemblyPath));
		string text = Path.Combine(analyzerShadowDirectory, fileName2, fileName);
		ShadowCopyFile(satelliteAssemblyPath, text);
		return text;
	}

	private string GetAnalyzerShadowDirectory(string analyzerFilePath)
	{
		string directoryName = Path.GetDirectoryName(analyzerFilePath);
		string path = OriginalDirectoryMap.GetOrAdd(directoryName, (string _) => Interlocked.Increment(ref _directoryCount)).ToString(CultureInfo.InvariantCulture);
		return Path.Combine(ShadowDirectory, path);
	}

	private void ShadowCopyFile(string originalFilePath, string shadowCopyPath)
	{
		if (CopyMap.TryGetValue(originalFilePath, out Task<string> value))
		{
			value.Wait();
			return;
		}
		TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>();
		Task<string> orAdd = CopyMap.GetOrAdd(originalFilePath, taskCompletionSource.Task);
		if (orAdd == taskCompletionSource.Task)
		{
			try
			{
				copyFile(originalFilePath, shadowCopyPath);
				taskCompletionSource.SetResult(shadowCopyPath);
				return;
			}
			catch (Exception exception)
			{
				taskCompletionSource.SetException(exception);
				throw;
			}
		}
		orAdd.Wait();
		static void copyFile(string originalPath, string text)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(text) ?? throw new ArgumentException("Shadow copy path '" + text + "' must not be the root directory"));
			if (File.Exists(originalPath))
			{
				File.Copy(originalPath, text);
				ClearReadOnlyFlagOnFile(new FileInfo(text));
			}
		}
	}

	private static void ClearReadOnlyFlagOnFiles(string directoryPath)
	{
		foreach (FileInfo item in new DirectoryInfo(directoryPath).EnumerateFiles("*", SearchOption.AllDirectories))
		{
			ClearReadOnlyFlagOnFile(item);
		}
	}

	private static void ClearReadOnlyFlagOnFile(FileInfo fileInfo)
	{
		try
		{
			if (fileInfo.IsReadOnly)
			{
				fileInfo.IsReadOnly = false;
			}
		}
		catch
		{
		}
	}
}
