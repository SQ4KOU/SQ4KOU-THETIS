using System;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.Scripting;

internal static class PdbHelpers
{
	public static DebugInformationFormat GetPlatformSpecificDebugInformationFormat()
	{
		if (Type.GetType("Mono.Runtime") != null)
		{
			return DebugInformationFormat.PortablePdb;
		}
		return DebugInformationFormat.Pdb;
	}
}
