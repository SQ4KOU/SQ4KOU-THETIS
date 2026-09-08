using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public sealed class ModuleMetadata : Metadata
{
	private readonly PEModule _module;

	private Action? _onDispose;

	private bool _isDisposed;

	public bool IsDisposed
	{
		get
		{
			if (!_isDisposed)
			{
				return _module.IsDisposed;
			}
			return true;
		}
	}

	internal PEModule Module
	{
		get
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("ModuleMetadata");
			}
			return _module;
		}
	}

	public string Name => Module.Name;

	public override MetadataImageKind Kind => MetadataImageKind.Module;

	internal MetadataReader MetadataReader => Module.MetadataReader;

	private ModuleMetadata(PEReader peReader, Action? onDispose)
		: base(isImageOwner: true, MetadataId.CreateNewId())
	{
		_module = new PEModule(this, peReader, IntPtr.Zero, 0, includeEmbeddedInteropTypes: false, ignoreAssemblyRefs: false);
		_onDispose = onDispose;
	}

	private ModuleMetadata(IntPtr metadata, int size, Action? onDispose, bool includeEmbeddedInteropTypes, bool ignoreAssemblyRefs)
		: base(isImageOwner: true, MetadataId.CreateNewId())
	{
		_module = new PEModule(this, null, metadata, size, includeEmbeddedInteropTypes, ignoreAssemblyRefs);
		_onDispose = onDispose;
	}

	private ModuleMetadata(ModuleMetadata metadata)
		: base(isImageOwner: false, metadata.Id)
	{
		_module = metadata.Module;
	}

	public static ModuleMetadata CreateFromMetadata(nint metadata, int size)
	{
		return CreateFromMetadataWorker(metadata, size, null);
	}

	public static ModuleMetadata CreateFromMetadata(nint metadata, int size, Action onDispose)
	{
		if (onDispose == null)
		{
			throw new ArgumentNullException("onDispose");
		}
		return CreateFromMetadataWorker(metadata, size, onDispose);
	}

	private static ModuleMetadata CreateFromMetadataWorker(nint metadata, int size, Action? onDispose)
	{
		if (metadata == 0)
		{
			throw new ArgumentNullException("metadata");
		}
		if (size <= 0)
		{
			throw new ArgumentOutOfRangeException(CodeAnalysisResources.SizeHasToBePositive, "size");
		}
		return new ModuleMetadata(metadata, size, onDispose, includeEmbeddedInteropTypes: false, ignoreAssemblyRefs: false);
	}

	internal static ModuleMetadata CreateFromMetadata(IntPtr metadata, int size, bool includeEmbeddedInteropTypes, bool ignoreAssemblyRefs = false)
	{
		return new ModuleMetadata(metadata, size, null, includeEmbeddedInteropTypes, ignoreAssemblyRefs);
	}

	public unsafe static ModuleMetadata CreateFromImage(nint peImage, int size)
	{
		return CreateFromImage((byte*)peImage, size, null);
	}

	private unsafe static ModuleMetadata CreateFromImage(byte* peImage, int size, Action? onDispose)
	{
		if (peImage == null)
		{
			throw new ArgumentNullException("peImage");
		}
		if (size <= 0)
		{
			throw new ArgumentOutOfRangeException(CodeAnalysisResources.SizeHasToBePositive, "size");
		}
		return new ModuleMetadata(new PEReader(peImage, size), onDispose);
	}

	public static ModuleMetadata CreateFromImage(IEnumerable<byte> peImage)
	{
		if (peImage == null)
		{
			throw new ArgumentNullException("peImage");
		}
		return CreateFromImage(ImmutableArray.CreateRange(peImage));
	}

	public static ModuleMetadata CreateFromImage(ImmutableArray<byte> peImage)
	{
		if (peImage.IsDefault)
		{
			throw new ArgumentNullException("peImage");
		}
		return new ModuleMetadata(new PEReader(peImage), null);
	}

	public static ModuleMetadata CreateFromStream(Stream peStream, bool leaveOpen = false)
	{
		return CreateFromStream(peStream, leaveOpen ? PEStreamOptions.LeaveOpen : PEStreamOptions.Default);
	}

	public unsafe static ModuleMetadata CreateFromStream(Stream peStream, PEStreamOptions options)
	{
		if (peStream == null)
		{
			throw new ArgumentNullException("peStream");
		}
		if (!peStream.CanRead || !peStream.CanSeek)
		{
			throw new ArgumentException(CodeAnalysisResources.StreamMustSupportReadAndSeek, "peStream");
		}
		if ((options & (PEStreamOptions.PrefetchMetadata | PEStreamOptions.PrefetchEntireImage)) == 0 && peStream is UnmanagedMemoryStream unmanagedMemoryStream)
		{
			Action onDispose = (options.HasFlag(PEStreamOptions.LeaveOpen) ? null : new Action(unmanagedMemoryStream.Dispose));
			return CreateFromImage(unmanagedMemoryStream.PositionPointer, (int)Math.Min(unmanagedMemoryStream.Length, 2147483647L), onDispose);
		}
		return new ModuleMetadata(new PEReader(peStream, options), null);
	}

	public static ModuleMetadata CreateFromFile(string path)
	{
		return CreateFromStream(StandardFileSystem.Instance.OpenFileWithNormalizedException(path, FileMode.Open, FileAccess.Read, FileShare.Read));
	}

	internal new ModuleMetadata Copy()
	{
		return new ModuleMetadata(this);
	}

	protected override Metadata CommonCopy()
	{
		return Copy();
	}

	public override void Dispose()
	{
		_isDisposed = true;
		if (IsImageOwner)
		{
			_module.Dispose();
			Interlocked.Exchange(ref _onDispose, null)?.Invoke();
		}
	}

	public Guid GetModuleVersionId()
	{
		return Module.GetModuleVersionIdOrThrow();
	}

	public ImmutableArray<string> GetModuleNames()
	{
		return Module.GetMetadataModuleNamesOrThrow();
	}

	public MetadataReader GetMetadataReader()
	{
		return MetadataReader;
	}

	public PortableExecutableReference GetReference(DocumentationProvider? documentation = null, string? filePath = null, string? display = null)
	{
		return new MetadataImageReference(this, MetadataReferenceProperties.Module, documentation, filePath, display);
	}
}
