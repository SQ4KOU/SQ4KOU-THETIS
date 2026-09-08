namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal readonly struct AssemblyLoadResult
{
	public bool IsSuccessful { get; }

	public string Path { get; }

	public string OriginalPath { get; }

	internal static AssemblyLoadResult CreateSuccessful(string path, string originalPath)
	{
		return new AssemblyLoadResult(path, originalPath, isSuccessful: true);
	}

	internal static AssemblyLoadResult CreateAlreadyLoaded(string path, string originalPath)
	{
		return new AssemblyLoadResult(path, originalPath, isSuccessful: false);
	}

	public AssemblyLoadResult(string path, string originalPath, bool isSuccessful)
	{
		Path = path;
		OriginalPath = originalPath;
		IsSuccessful = isSuccessful;
	}
}
