using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.DiaSymReader;

namespace Microsoft.Cci;

internal static class PeWriter
{
	internal struct EmitBuilders
	{
		internal BlobBuilder IlBlobBuilder = new BlobBuilder(32768);

		internal PooledBlobBuilder? MappedFieldDataBlobBuilder = null;

		internal PooledBlobBuilder? ManagedResourceBlobBuilder = null;

		internal PooledBlobBuilder? PortableExecutableBlobBuilder = null;

		internal PooledBlobBuilder? PortablePdbBlobBuilder = null;

		public EmitBuilders()
		{
		}

		internal void Free()
		{
			if (PortableExecutableBlobBuilder == null)
			{
				MappedFieldDataBlobBuilder?.Free();
				ManagedResourceBlobBuilder?.Free();
			}
			else
			{
				PortableExecutableBlobBuilder.Free();
			}
			PortablePdbBlobBuilder?.Free();
		}
	}

	private sealed class ResourceSectionBuilderFromObj : ResourceSectionBuilder
	{
		private readonly ResourceSection _resourceSection;

		public ResourceSectionBuilderFromObj(ResourceSection resourceSection)
		{
			_resourceSection = resourceSection;
		}

		protected override void Serialize(BlobBuilder builder, SectionLocation location)
		{
			NativeResourceWriter.SerializeWin32Resources(builder, _resourceSection, location.RelativeVirtualAddress);
		}
	}

	private sealed class ResourceSectionBuilderFromResources : ResourceSectionBuilder
	{
		private readonly IEnumerable<IWin32Resource> _resources;

		public ResourceSectionBuilderFromResources(IEnumerable<IWin32Resource> resources)
		{
			_resources = resources;
		}

		protected override void Serialize(BlobBuilder builder, SectionLocation location)
		{
			NativeResourceWriter.SerializeWin32Resources(builder, _resources, location.RelativeVirtualAddress);
		}
	}

	private sealed class ResourceSectionBuilderFromRaw : ResourceSectionBuilder
	{
		private readonly Stream _resources;

		public ResourceSectionBuilderFromRaw(Stream resources)
		{
			_resources = resources;
		}

		protected override void Serialize(BlobBuilder builder, SectionLocation location)
		{
			int num;
			while ((num = _resources.ReadByte()) >= 0)
			{
				builder.WriteByte((byte)num);
			}
		}
	}

	private static MethodInfo? s_calculateChecksumMethod;

	internal static bool WritePeToStream(EmitContext context, CommonMessageProvider messageProvider, Func<Stream?> getPeStream, Func<Stream?>? getPortablePdbStreamOpt, PdbWriter? nativePdbWriterOpt, string? pdbPathOpt, bool metadataOnly, bool isDeterministic, bool emitTestCoverageData, RSAParameters? privateKeyOpt, CancellationToken cancellationToken)
	{
		MetadataWriter metadataWriter = FullMetadataWriter.Create(context, messageProvider, metadataOnly, isDeterministic, emitTestCoverageData, getPortablePdbStreamOpt != null, cancellationToken);
		ModulePropertiesForSerialization serializationProperties = context.Module.SerializationProperties;
		context.Module.TestData?.SetMetadataWriter(metadataWriter);
		nativePdbWriterOpt?.SetMetadataEmitter(metadataWriter);
		EmitBuilders emitBuilders = new EmitBuilders();
		metadataWriter.BuildMetadataAndIL(nativePdbWriterOpt, emitBuilders.IlBlobBuilder, out emitBuilders.MappedFieldDataBlobBuilder, out emitBuilders.ManagedResourceBlobBuilder, out var mvidFixup, out var mvidStringFixup);
		metadataWriter.GetEntryPoints(out var entryPointHandle, out var debugEntryPointHandle);
		if (!debugEntryPointHandle.IsNil)
		{
			nativePdbWriterOpt?.SetEntryPoint(MetadataTokens.GetToken(debugEntryPointHandle));
		}
		if (nativePdbWriterOpt != null)
		{
			if (context.Module.SourceLinkStreamOpt != null)
			{
				nativePdbWriterOpt.EmbedSourceLink(context.Module.SourceLinkStreamOpt);
			}
			if (metadataWriter.Module.OutputKind == OutputKind.WindowsRuntimeMetadata)
			{
				nativePdbWriterOpt.WriteDefinitionLocations(metadataWriter.Module.GetSymbolToLocationMap());
			}
			nativePdbWriterOpt.WriteRemainingDebugDocuments(metadataWriter.Module.DebugDocumentsBuilder.DebugDocuments);
			nativePdbWriterOpt.WriteCompilerVersion(context.Module.CommonCompilation.Language);
		}
		Stream stream = getPeStream();
		if (stream == null)
		{
			emitBuilders.Free();
			return false;
		}
		BlobContentId pdbContentId = nativePdbWriterOpt?.GetContentId() ?? default(BlobContentId);
		nativePdbWriterOpt = null;
		ushort portablePdbVersion = 0;
		MetadataRootBuilder rootBuilder = metadataWriter.GetRootBuilder();
		PEHeaderBuilder header = new PEHeaderBuilder(serializationProperties.Machine, serializationProperties.SectionAlignment, serializationProperties.FileAlignment, serializationProperties.BaseAddress, serializationProperties.LinkerMajorVersion, serializationProperties.LinkerMinorVersion, 4, 0, 0, 0, serializationProperties.MajorSubsystemVersion, serializationProperties.MinorSubsystemVersion, serializationProperties.Subsystem, serializationProperties.DllCharacteristics, serializationProperties.ImageCharacteristics, serializationProperties.SizeOfStackReserve, serializationProperties.SizeOfStackCommit, serializationProperties.SizeOfHeapReserve, serializationProperties.SizeOfHeapCommit);
		Func<IEnumerable<Blob>, BlobContentId> deterministicIdProvider = (isDeterministic ? ((Func<IEnumerable<Blob>, BlobContentId>)((IEnumerable<Blob> content) => BlobContentId.FromHash(CryptographicHashProvider.ComputeSourceHash(content)))) : null);
		ImmutableArray<byte> portablePdbContentHash = default(ImmutableArray<byte>);
		PooledBlobBuilder pooledBlobBuilder = null;
		if (metadataWriter.EmitPortableDebugMetadata)
		{
			metadataWriter.AddRemainingDebugDocuments(metadataWriter.Module.DebugDocumentsBuilder.DebugDocuments);
			Func<IEnumerable<Blob>, BlobContentId> deterministicIdProviderOpt = ((context.Module.PdbChecksumAlgorithm.Name != null) ? ((Func<IEnumerable<Blob>, BlobContentId>)((IEnumerable<Blob> content) => BlobContentId.FromHash(portablePdbContentHash = CryptographicHashProvider.ComputeHash(context.Module.PdbChecksumAlgorithm, content)))) : null);
			emitBuilders.PortablePdbBlobBuilder = PooledBlobBuilder.GetInstance(zero: true);
			PortablePdbBuilder portablePdbBuilder = metadataWriter.GetPortablePdbBuilder(rootBuilder.Sizes.RowCounts, debugEntryPointHandle, deterministicIdProviderOpt);
			pdbContentId = portablePdbBuilder.Serialize(emitBuilders.PortablePdbBlobBuilder);
			portablePdbVersion = portablePdbBuilder.FormatVersion;
			if (getPortablePdbStreamOpt == null)
			{
				pooledBlobBuilder = emitBuilders.PortablePdbBlobBuilder;
			}
			else
			{
				Stream stream2 = getPortablePdbStreamOpt();
				if (stream2 != null)
				{
					try
					{
						emitBuilders.PortablePdbBlobBuilder.WriteContentTo(stream2);
					}
					catch (Exception ex) when (!(ex is OperationCanceledException))
					{
						throw new SymUnmanagedWriterException(ex.Message, ex);
					}
				}
			}
		}
		DebugDirectoryBuilder debugDirectoryBuilder;
		if (((pdbPathOpt != null) | isDeterministic) || pooledBlobBuilder != null)
		{
			debugDirectoryBuilder = new DebugDirectoryBuilder();
			if (pdbPathOpt != null)
			{
				string pdbPath = (isDeterministic ? pdbPathOpt : PadPdbPath(pdbPathOpt));
				debugDirectoryBuilder.AddCodeViewEntry(pdbPath, pdbContentId, portablePdbVersion);
				if (!portablePdbContentHash.IsDefault)
				{
					debugDirectoryBuilder.AddPdbChecksumEntry(context.Module.PdbChecksumAlgorithm.Name, portablePdbContentHash);
				}
			}
			if (isDeterministic)
			{
				debugDirectoryBuilder.AddReproducibleEntry();
			}
			if (pooledBlobBuilder != null)
			{
				debugDirectoryBuilder.AddEmbeddedPortablePdbEntry(pooledBlobBuilder, portablePdbVersion);
			}
		}
		else
		{
			debugDirectoryBuilder = null;
		}
		StrongNameProvider strongNameProvider = context.Module.CommonCompilation.Options.StrongNameProvider;
		CorFlags corFlags = serializationProperties.CorFlags;
		ExtendedPEBuilder extendedPEBuilder = new ExtendedPEBuilder(header, rootBuilder, emitBuilders.IlBlobBuilder, emitBuilders.MappedFieldDataBlobBuilder, emitBuilders.ManagedResourceBlobBuilder, CreateNativeResourceSectionSerializer(context.Module), debugDirectoryBuilder, SigningUtilities.CalculateStrongNameSignatureSize(context.Module, privateKeyOpt), entryPointHandle, corFlags, deterministicIdProvider, metadataOnly && !context.IncludePrivateMembers);
		emitBuilders.PortableExecutableBlobBuilder = PooledBlobBuilder.GetInstance(zero: true);
		BlobContentId blobContentId = extendedPEBuilder.Serialize(emitBuilders.PortableExecutableBlobBuilder, out var mvidSectionFixup);
		PatchModuleVersionIds(mvidFixup, mvidSectionFixup, mvidStringFixup, blobContentId.Guid);
		if (privateKeyOpt.HasValue && corFlags.HasFlag(CorFlags.StrongNameSigned))
		{
			strongNameProvider.SignBuilder(extendedPEBuilder, emitBuilders.PortableExecutableBlobBuilder, privateKeyOpt.Value);
		}
		try
		{
			emitBuilders.PortableExecutableBlobBuilder.WriteContentTo(stream);
		}
		catch (Exception ex2) when (!(ex2 is OperationCanceledException))
		{
			throw new PeWritingException(ex2);
		}
		emitBuilders.Free();
		return true;
	}

	internal static uint CalculateChecksum(BlobBuilder peBlob, Blob checksumBlob)
	{
		if (s_calculateChecksumMethod == null)
		{
			s_calculateChecksumMethod = (from m in typeof(PEBuilder).GetRuntimeMethods()
				where m.Name == "CalculateChecksum" && m.GetParameters().Length == 2
				select m).Single();
		}
		return (uint)s_calculateChecksumMethod.Invoke(null, new object[2] { peBlob, checksumBlob });
	}

	private static void PatchModuleVersionIds(Blob guidFixup, Blob guidSectionFixup, Blob stringFixup, Guid mvid)
	{
		if (!guidFixup.IsDefault)
		{
			new BlobWriter(guidFixup).WriteGuid(mvid);
		}
		if (!guidSectionFixup.IsDefault)
		{
			new BlobWriter(guidSectionFixup).WriteGuid(mvid);
		}
		if (!stringFixup.IsDefault)
		{
			new BlobWriter(stringFixup).WriteUserString(mvid.ToString());
		}
	}

	private static string PadPdbPath(string path)
	{
		return path + new string('\0', Math.Max(0, 260 - Encoding.UTF8.GetByteCount(path) - 1));
	}

	private static ResourceSectionBuilder? CreateNativeResourceSectionSerializer(CommonPEModuleBuilder module)
	{
		ResourceSection win32ResourceSection = module.Win32ResourceSection;
		if (win32ResourceSection != null)
		{
			return new ResourceSectionBuilderFromObj(win32ResourceSection);
		}
		IEnumerable<IWin32Resource> win32Resources = module.Win32Resources;
		if (win32Resources != null && win32Resources.Any())
		{
			return new ResourceSectionBuilderFromResources(win32Resources);
		}
		Stream rawWin32Resources = module.RawWin32Resources;
		if (rawWin32Resources != null)
		{
			return new ResourceSectionBuilderFromRaw(rawWin32Resources);
		}
		return null;
	}
}
