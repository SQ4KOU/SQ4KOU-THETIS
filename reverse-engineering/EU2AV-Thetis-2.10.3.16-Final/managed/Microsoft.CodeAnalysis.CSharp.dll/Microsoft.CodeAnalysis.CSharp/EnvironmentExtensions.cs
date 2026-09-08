using System;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class EnvironmentExtensions
{
	extension(Environment)
	{
		public static int ProcessId => Process.GetCurrentProcess().Id;
	}
}
