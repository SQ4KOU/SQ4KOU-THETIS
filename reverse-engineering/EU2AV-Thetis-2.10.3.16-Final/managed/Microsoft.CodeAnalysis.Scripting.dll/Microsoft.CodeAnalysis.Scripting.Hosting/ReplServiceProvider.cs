using System;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal abstract class ReplServiceProvider
{
	public abstract ObjectFormatter ObjectFormatter { get; }

	public abstract CommandLineParser CommandLineParser { get; }

	public abstract DiagnosticFormatter DiagnosticFormatter { get; }

	public abstract string Logo { get; }

	public abstract Script<T> CreateScript<T>(string code, ScriptOptions options, Type globalsTypeOpt, InteractiveAssemblyLoader assemblyLoader);
}
