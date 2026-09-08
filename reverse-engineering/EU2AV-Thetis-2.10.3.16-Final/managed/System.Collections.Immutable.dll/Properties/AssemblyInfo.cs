using System;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;

[assembly: AssemblyMetadata("Serviceable", "True")]
[assembly: AssemblyMetadata("PreferInbox", "True")]
[assembly: AssemblyDefaultAlias("System.Collections.Immutable")]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: CLSCompliant(true)]
[assembly: AssemblyMetadata("IsTrimmable", "True")]
[assembly: AssemblyMetadata("IsAotCompatible", "True")]
[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32 | DllImportSearchPath.AssemblyDirectory)]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyCopyright("© Microsoft Corporation. All rights reserved.")]
[assembly: AssemblyDescription("This package provides collections that are thread safe and guaranteed to never change their contents, also known as immutable collections. Like strings, any methods that perform modifications will not change the existing instance but instead return a new instance. For efficiency reasons, the implementation uses a sharing mechanism to ensure that newly created instances share as much data as possible with the previous instance while ensuring that operations have a predictable time complexity.\r\n\r\nThe System.Collections.Immutable library is built-in as part of the shared framework in .NET Runtime. The package can be installed when you need to use it in other target frameworks.")]
[assembly: AssemblyFileVersion("10.0.726.21808")]
[assembly: AssemblyInformationalVersion("10.0.7+b16286c2284fecf303dbc12a0bb152476d662e44")]
[assembly: AssemblyProduct("Microsoft® .NET")]
[assembly: AssemblyTitle("System.Collections.Immutable")]
[assembly: AssemblyMetadata("RepositoryUrl", "https://github.com/dotnet/dotnet")]
[assembly: AssemblyVersion("10.0.0.7")]
[module: NullablePublicOnly(false)]
