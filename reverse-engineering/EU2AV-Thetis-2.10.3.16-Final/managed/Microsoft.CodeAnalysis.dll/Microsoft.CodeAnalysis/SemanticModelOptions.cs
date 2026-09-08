using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis;

[Experimental("RSEXPERIMENTAL001", UrlFormat = "https://github.com/dotnet/roslyn/issues/70609")]
[Flags]
public enum SemanticModelOptions
{
	None = 0,
	IgnoreAccessibility = 1,
	DisableNullableAnalysis = 2
}
