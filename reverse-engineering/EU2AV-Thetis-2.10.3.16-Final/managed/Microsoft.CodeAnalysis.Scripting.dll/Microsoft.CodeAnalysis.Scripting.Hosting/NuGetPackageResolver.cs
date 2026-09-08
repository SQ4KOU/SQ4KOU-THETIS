using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal abstract class NuGetPackageResolver
{
	private const string ReferencePrefix = "nuget:";

	internal static bool TryParsePackageReference(string reference, out string name, out string version)
	{
		if (reference.StartsWith("nuget:", StringComparison.Ordinal))
		{
			int length = "nuget:".Length;
			string[] array = reference.Substring(length, reference.Length - length).Split(new char[1] { '/' });
			name = array[0];
			if (name.Length > 0)
			{
				switch (array.Length)
				{
				case 1:
					version = string.Empty;
					return true;
				case 2:
					version = array[1];
					if (version.Length > 0)
					{
						return true;
					}
					break;
				}
			}
		}
		name = null;
		version = null;
		return false;
	}

	internal abstract ImmutableArray<string> ResolveNuGetPackage(string packageName, string packageVersion);
}
