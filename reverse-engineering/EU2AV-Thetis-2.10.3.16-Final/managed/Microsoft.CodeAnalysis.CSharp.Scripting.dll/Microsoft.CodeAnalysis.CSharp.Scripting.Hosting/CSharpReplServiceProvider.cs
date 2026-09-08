using System;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;

namespace Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;

internal sealed class CSharpReplServiceProvider : ReplServiceProvider
{
	public override ObjectFormatter ObjectFormatter { get; } = CSharpObjectFormatter.Instance;

	public override CommandLineParser CommandLineParser => CSharpCommandLineParser.Script;

	public override DiagnosticFormatter DiagnosticFormatter => CSharpDiagnosticFormatter.Instance;

	public override string Logo => string.Format(CSharpScriptingResources.LogoLine1, CommonCompiler.GetProductVersion(typeof(CSharpReplServiceProvider)));

	public override Script<T> CreateScript<T>(string code, ScriptOptions options, Type globalsTypeOpt, InteractiveAssemblyLoader assemblyLoader)
	{
		return CSharpScript.Create<T>(code, options, globalsTypeOpt, assemblyLoader);
	}
}
