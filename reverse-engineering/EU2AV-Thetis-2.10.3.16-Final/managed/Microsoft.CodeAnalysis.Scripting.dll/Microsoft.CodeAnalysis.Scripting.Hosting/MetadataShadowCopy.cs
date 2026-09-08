namespace Microsoft.CodeAnalysis.Scripting.Hosting;

public sealed class MetadataShadowCopy
{
	public FileShadowCopy PrimaryModule { get; }

	public FileShadowCopy DocumentationFile { get; }

	public Metadata Metadata { get; }

	internal MetadataShadowCopy(FileShadowCopy primaryModule, FileShadowCopy documentationFileOpt, Metadata metadataCopy)
	{
		PrimaryModule = primaryModule;
		DocumentationFile = documentationFileOpt;
		Metadata = metadataCopy;
	}

	internal void DisposeFileHandles()
	{
		PrimaryModule.DisposeFileStream();
		DocumentationFile?.DisposeFileStream();
	}
}
