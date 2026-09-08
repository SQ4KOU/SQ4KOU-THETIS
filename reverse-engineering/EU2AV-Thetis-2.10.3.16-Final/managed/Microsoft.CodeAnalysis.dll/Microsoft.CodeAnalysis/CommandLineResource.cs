using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Microsoft.CodeAnalysis;

public readonly struct CommandLineResource
{
	public string ResourceName { get; }

	public string FullPath { get; }

	public bool IsPublic { get; }

	public string? LinkedResourceFileName { get; }

	public bool IsEmbedded => LinkedResourceFileName == null;

	[MemberNotNullWhen(true, "LinkedResourceFileName")]
	public bool IsLinked
	{
		[MemberNotNullWhen(true, "LinkedResourceFileName")]
		get
		{
			return LinkedResourceFileName != null;
		}
	}

	internal CommandLineResource(string resourceName, string fullPath, string? linkedResourceFileName, bool isPublic)
	{
		ResourceName = resourceName;
		FullPath = fullPath;
		LinkedResourceFileName = linkedResourceFileName;
		IsPublic = isPublic;
	}

	internal ResourceDescription ToDescription()
	{
		string fullPath = FullPath ?? throw new NullReferenceException();
		Func<Stream> dataProvider = () => new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		return new ResourceDescription(ResourceName, LinkedResourceFileName, dataProvider, IsPublic, IsEmbedded, checkArgs: false);
	}
}
