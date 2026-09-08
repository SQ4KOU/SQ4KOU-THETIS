using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class FileIdentifier
{
	private class FileIdentifierData(string? encoderFallbackErrorMessage, string displayFilePath, ImmutableArray<byte> filePathChecksumOpt)
	{
		public readonly string? EncoderFallbackErrorMessage = encoderFallbackErrorMessage;

		public readonly string DisplayFilePath = displayFilePath;

		public readonly ImmutableArray<byte> FilePathChecksumOpt = filePathChecksumOpt;
	}

	private static readonly Encoding s_encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

	private readonly string _filePath;

	private FileIdentifierData? _data;

	public string DisplayFilePath
	{
		get
		{
			EnsureInitialized();
			return _data.DisplayFilePath;
		}
	}

	public string? EncoderFallbackErrorMessage
	{
		get
		{
			EnsureInitialized();
			return _data.EncoderFallbackErrorMessage;
		}
	}

	public ImmutableArray<byte> FilePathChecksumOpt
	{
		get
		{
			EnsureInitialized();
			return _data.FilePathChecksumOpt;
		}
	}

	private FileIdentifier(string filePath)
	{
		_filePath = filePath;
	}

	private FileIdentifier(ImmutableArray<byte> filePathChecksumOpt, string displayFilePath)
	{
		_data = new FileIdentifierData(null, displayFilePath, filePathChecksumOpt);
		_filePath = string.Empty;
	}

	[MemberNotNull("_data")]
	private void EnsureInitialized()
	{
		if (_data != null)
		{
			return;
		}
		string encoderFallbackErrorMessage = null;
		ImmutableArray<byte> filePathChecksumOpt = default(ImmutableArray<byte>);
		try
		{
			byte[] bytes = s_encoding.GetBytes(_filePath);
			using HashAlgorithm hashAlgorithm = SourceHashAlgorithms.CreateDefaultInstance();
			filePathChecksumOpt = hashAlgorithm.ComputeHash(bytes).ToImmutableArray();
		}
		catch (EncoderFallbackException ex)
		{
			encoderFallbackErrorMessage = ex.Message;
		}
		string displayFilePath = GeneratedNames.GetDisplayFilePath(_filePath);
		_data = new FileIdentifierData(encoderFallbackErrorMessage, displayFilePath, filePathChecksumOpt);
	}

	public static FileIdentifier Create(SyntaxTree syntaxTree, SourceReferenceResolver? resolver)
	{
		return new FileIdentifier(syntaxTree.GetNormalizedPath(resolver));
	}

	public static FileIdentifier Create(string normalizedFilePath)
	{
		return new FileIdentifier(normalizedFilePath);
	}

	public static FileIdentifier Create(ImmutableArray<byte> filePathChecksumOpt, string displayFilePath)
	{
		return new FileIdentifier(filePathChecksumOpt, displayFilePath);
	}
}
