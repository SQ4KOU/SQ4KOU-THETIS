using System;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal sealed class InteractiveAssemblyLoaderException : NotSupportedException
{
	internal InteractiveAssemblyLoaderException(string message)
		: base(message)
	{
	}
}
