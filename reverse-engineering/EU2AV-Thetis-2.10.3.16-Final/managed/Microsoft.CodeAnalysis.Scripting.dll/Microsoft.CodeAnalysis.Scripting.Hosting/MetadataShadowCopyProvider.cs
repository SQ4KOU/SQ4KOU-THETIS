using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

public sealed class MetadataShadowCopyProvider : IDisposable
{
	private readonly struct CacheEntry<TPublic>(TPublic @public, Metadata @private)
	{
		public readonly TPublic Public = @public;

		public readonly Metadata Private = @private;
	}

	private readonly CultureInfo _documentationCommentsCulture;

	private readonly string _baseDirectory;

	internal string ShadowCopyDirectory;

	private readonly ImmutableArray<string> _noShadowCopyDirectories;

	private readonly Dictionary<FileKey, CacheEntry<MetadataShadowCopy>> _shadowCopies = new Dictionary<FileKey, CacheEntry<MetadataShadowCopy>>();

	private readonly Dictionary<FileKey, CacheEntry<Metadata>> _noShadowCopyCache = new Dictionary<FileKey, CacheEntry<Metadata>>();

	private HashSet<string> _lazySuppressedFiles;

	private object Guard => _shadowCopies;

	internal int CacheSize => _shadowCopies.Count;

	public MetadataShadowCopyProvider(string directory = null, IEnumerable<string> noShadowCopyDirectories = null, CultureInfo documentationCommentsCulture = null)
	{
		if (directory != null)
		{
			RequireAbsolutePath(directory, "directory");
			try
			{
				_baseDirectory = FileUtilities.NormalizeDirectoryPath(directory);
			}
			catch (Exception ex)
			{
				throw new ArgumentException(ex.Message, "directory");
			}
		}
		else
		{
			_baseDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		}
		if (noShadowCopyDirectories != null)
		{
			try
			{
				_noShadowCopyDirectories = ImmutableArray.CreateRange(noShadowCopyDirectories.Select(FileUtilities.NormalizeDirectoryPath));
			}
			catch (Exception ex2)
			{
				throw new ArgumentException(ex2.Message, "noShadowCopyDirectories");
			}
		}
		else
		{
			_noShadowCopyDirectories = ImmutableArray<string>.Empty;
		}
		_documentationCommentsCulture = documentationCommentsCulture;
	}

	private static void RequireAbsolutePath(string path, string argumentName)
	{
		if (path == null)
		{
			throw new ArgumentNullException(argumentName);
		}
		if (!PathUtilities.IsAbsolute(path))
		{
			throw new ArgumentException(ScriptingResources.AbsolutePathExpected, argumentName);
		}
	}

	public bool IsShadowCopy(string fullPath)
	{
		RequireAbsolutePath(fullPath, "fullPath");
		string shadowCopyDirectory = ShadowCopyDirectory;
		if (shadowCopyDirectory == null)
		{
			return false;
		}
		string text;
		try
		{
			text = FileUtilities.NormalizeDirectoryPath(fullPath);
		}
		catch
		{
			return false;
		}
		return text.StartsWith(shadowCopyDirectory, StringComparison.OrdinalIgnoreCase);
	}

	~MetadataShadowCopyProvider()
	{
		DisposeShadowCopies();
		DeleteShadowCopyDirectory();
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		lock (Guard)
		{
			DisposeShadowCopies();
			_shadowCopies.Clear();
		}
		DeleteShadowCopyDirectory();
	}

	private void DisposeShadowCopies()
	{
		foreach (CacheEntry<MetadataShadowCopy> value in _shadowCopies.Values)
		{
			value.Public.DisposeFileHandles();
			value.Private.Dispose();
		}
	}

	private void DeleteShadowCopyDirectory()
	{
		string shadowCopyDirectory = ShadowCopyDirectory;
		if (!Directory.Exists(shadowCopyDirectory))
		{
			return;
		}
		try
		{
			foreach (FileInfo item in new DirectoryInfo(shadowCopyDirectory).EnumerateFiles("*", SearchOption.AllDirectories))
			{
				StripReadOnlyAttributeFromFile(item);
			}
			Directory.Delete(shadowCopyDirectory, recursive: true);
		}
		catch
		{
		}
	}

	private static void StripReadOnlyAttributeFromFile(FileInfo fileInfo)
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

	public Metadata GetMetadata(string fullPath, MetadataImageKind kind)
	{
		if (NeedsShadowCopy(fullPath))
		{
			return GetMetadataShadowCopyNoCheck(fullPath, kind).Metadata;
		}
		FileKey key = FileKey.Create(fullPath);
		lock (Guard)
		{
			if (_noShadowCopyCache.TryGetValue(key, out var value))
			{
				return value.Public;
			}
		}
		Metadata metadata = ((kind != MetadataImageKind.Assembly) ? ((Metadata)ModuleMetadata.CreateFromFile(fullPath)) : ((Metadata)AssemblyMetadata.CreateFromFile(fullPath)));
		key = FileKey.Create(fullPath);
		lock (Guard)
		{
			if (_noShadowCopyCache.TryGetValue(key, out var value2))
			{
				metadata.Dispose();
				return value2.Public;
			}
			Metadata metadata2 = metadata.Copy();
			_noShadowCopyCache.Add(key, new CacheEntry<Metadata>(metadata2, metadata));
			return metadata2;
		}
	}

	public MetadataShadowCopy GetMetadataShadowCopy(string fullPath, MetadataImageKind kind)
	{
		if (!NeedsShadowCopy(fullPath))
		{
			return null;
		}
		return GetMetadataShadowCopyNoCheck(fullPath, kind);
	}

	private MetadataShadowCopy GetMetadataShadowCopyNoCheck(string fullPath, MetadataImageKind kind)
	{
		if ((int)kind > 1)
		{
			throw new ArgumentOutOfRangeException("kind");
		}
		FileKey key = FileKey.Create(fullPath);
		lock (Guard)
		{
			if (CopyExistsOrIsSuppressed(key, out var existing))
			{
				return existing.Public;
			}
		}
		CacheEntry<MetadataShadowCopy> value = CreateMetadataShadowCopy(fullPath, kind);
		bool flag = true;
		try
		{
			key = new FileKey(fullPath, FileUtilities.GetFileTimeStamp(value.Public.PrimaryModule.FullPath));
			flag = false;
		}
		finally
		{
			if (flag)
			{
				value.Private.Dispose();
			}
		}
		lock (Guard)
		{
			if (CopyExistsOrIsSuppressed(key, out var existing2))
			{
				value.Private.Dispose();
				return existing2.Public;
			}
			_shadowCopies.Add(key, value);
		}
		return value.Public;
	}

	private bool CopyExistsOrIsSuppressed(FileKey key, out CacheEntry<MetadataShadowCopy> existing)
	{
		if (_lazySuppressedFiles != null && _lazySuppressedFiles.Contains(key.FullPath))
		{
			existing = default(CacheEntry<MetadataShadowCopy>);
			return true;
		}
		return _shadowCopies.TryGetValue(key, out existing);
	}

	public void SuppressShadowCopy(string originalPath)
	{
		RequireAbsolutePath(originalPath, "originalPath");
		lock (Guard)
		{
			if (_lazySuppressedFiles == null)
			{
				_lazySuppressedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			}
			_lazySuppressedFiles.Add(originalPath);
		}
	}

	public bool NeedsShadowCopy(string fullPath)
	{
		RequireAbsolutePath(fullPath, "fullPath");
		string directoryName = Path.GetDirectoryName(fullPath);
		string shadowCopyDirectory = ShadowCopyDirectory;
		if (shadowCopyDirectory != null && directoryName.StartsWith(shadowCopyDirectory, StringComparison.Ordinal))
		{
			return false;
		}
		return !_noShadowCopyDirectories.Any((string dir, string directory) => directory.StartsWith(dir, StringComparison.Ordinal), directoryName);
	}

	private CacheEntry<MetadataShadowCopy> CreateMetadataShadowCopy(string originalPath, MetadataImageKind kind)
	{
		int num = 10;
		while (true)
		{
			try
			{
				if (ShadowCopyDirectory == null)
				{
					ShadowCopyDirectory = CreateUniqueDirectory(_baseDirectory);
				}
				string text = CreateUniqueDirectory(ShadowCopyDirectory);
				string text2 = Path.Combine(text, Path.GetFileName(originalPath));
				FileShadowCopy documentationFileOpt = TryCopyDocumentationFile(originalPath, text, _documentationCommentsCulture);
				FileStream fileStream = CopyFile(originalPath, text2);
				FileShadowCopy primaryModule = new FileShadowCopy(fileStream, originalPath, text2);
				Metadata metadata = ((kind != MetadataImageKind.Assembly) ? ((Metadata)CreateModuleMetadata(fileStream)) : ((Metadata)CreateAssemblyMetadata(fileStream, originalPath, text2)));
				Metadata metadataCopy = metadata.Copy();
				return new CacheEntry<MetadataShadowCopy>(new MetadataShadowCopy(primaryModule, documentationFileOpt, metadataCopy), metadata);
			}
			catch (DirectoryNotFoundException)
			{
				if (Directory.Exists(ShadowCopyDirectory))
				{
					goto IL_00b1;
				}
				ShadowCopyDirectory = null;
				if (num-- <= 0)
				{
					goto IL_00b1;
				}
				goto end_IL_008f;
				IL_00b1:
				throw;
				end_IL_008f:;
			}
		}
	}

	private AssemblyMetadata CreateAssemblyMetadata(FileStream manifestModuleCopyStream, string originalPath, string shadowCopyPath)
	{
		ImmutableArray<ModuleMetadata>.Builder builder = null;
		bool flag = true;
		ModuleMetadata moduleMetadata = null;
		try
		{
			moduleMetadata = CreateModuleMetadata(manifestModuleCopyStream);
			string path = null;
			string path2 = null;
			foreach (string moduleName in moduleMetadata.GetModuleNames())
			{
				if (builder == null)
				{
					builder = ImmutableArray.CreateBuilder<ModuleMetadata>();
					builder.Add(moduleMetadata);
					path = Path.GetDirectoryName(originalPath);
					path2 = Path.GetDirectoryName(shadowCopyPath);
				}
				FileStream stream = CopyFile(Path.Combine(path, moduleName), Path.Combine(path2, moduleName));
				builder.Add(CreateModuleMetadata(stream));
			}
			ImmutableArray<ModuleMetadata> modules = builder?.ToImmutable() ?? ImmutableArray.Create(moduleMetadata);
			flag = false;
			return AssemblyMetadata.Create(modules);
		}
		finally
		{
			if (flag)
			{
				moduleMetadata?.Dispose();
				if (builder != null)
				{
					for (int i = 1; i < builder.Count; i++)
					{
						builder[i].Dispose();
					}
				}
			}
		}
	}

	private static ModuleMetadata CreateModuleMetadata(FileStream stream)
	{
		return ModuleMetadata.CreateFromStream(stream);
	}

	private string CreateUniqueDirectory(string basePath)
	{
		int num = 10;
		while (true)
		{
			string text = Path.Combine(basePath, Guid.NewGuid().ToString());
			if (File.Exists(text) || Directory.Exists(text))
			{
				continue;
			}
			try
			{
				Directory.CreateDirectory(text);
				return text;
			}
			catch (IOException)
			{
				if (File.Exists(text) || --num != 0)
				{
					continue;
				}
				throw;
			}
		}
	}

	private static FileShadowCopy TryCopyDocumentationFile(string originalAssemblyPath, string assemblyCopyDirectory, CultureInfo docCultureOpt)
	{
		string directoryName = Path.GetDirectoryName(originalAssemblyPath);
		string fileName = Path.GetFileName(originalAssemblyPath);
		if (docCultureOpt == null || !TryFindCollocatedDocumentationFile(directoryName, fileName, docCultureOpt, out var docSubdirectory, out var docFileName))
		{
			return null;
		}
		if (!docSubdirectory.IsEmpty())
		{
			try
			{
				Directory.CreateDirectory(Path.Combine(assemblyCopyDirectory, docSubdirectory));
			}
			catch
			{
				return null;
			}
		}
		string text = Path.Combine(assemblyCopyDirectory, docSubdirectory, docFileName);
		string originalPath = Path.Combine(directoryName, docSubdirectory, docFileName);
		FileStream fileStream = CopyFile(originalPath, text, fileMayNotExist: true);
		if (fileStream == null)
		{
			return null;
		}
		return new FileShadowCopy(fileStream, originalPath, text);
	}

	private static bool TryFindCollocatedDocumentationFile(string assemblyDirectory, string assemblyFileName, CultureInfo culture, out string docSubdirectory, out string docFileName)
	{
		docFileName = Path.ChangeExtension(assemblyFileName, ".xml");
		while (culture != CultureInfo.InvariantCulture)
		{
			docSubdirectory = culture.Name;
			if (File.Exists(Path.Combine(assemblyDirectory, docSubdirectory, docFileName)))
			{
				return true;
			}
			culture = culture.Parent;
		}
		docSubdirectory = string.Empty;
		if (File.Exists(Path.Combine(assemblyDirectory, docFileName)))
		{
			return true;
		}
		docFileName = null;
		return false;
	}

	private static FileStream CopyFile(string originalPath, string shadowCopyPath, bool fileMayNotExist = false)
	{
		try
		{
			File.Copy(originalPath, shadowCopyPath, overwrite: true);
			StripReadOnlyAttributeFromFile(new FileInfo(shadowCopyPath));
			return new FileStream(shadowCopyPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		}
		catch (Exception ex) when (((Func<bool>)delegate
		{
			// Could not convert BlockContainer to single expression
			bool flag = fileMayNotExist;
			if (flag)
			{
				flag = ((ex is FileNotFoundException || ex is DirectoryNotFoundException) ? true : false);
			}
			return flag;
		}).Invoke())
		{
			return null;
		}
	}
}
