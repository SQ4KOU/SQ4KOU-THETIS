using System;
using System.IO;
using System.Reflection.PortableExecutable;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;

namespace Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;

internal sealed class CSharpInteractiveCompiler : CSharpCompiler
{
	private readonly Func<string, PEStreamOptions, MetadataReferenceProperties, MetadataImageReference> _createFromFileFunc;

	internal override Type Type => typeof(CSharpInteractiveCompiler);

	internal CSharpInteractiveCompiler(string? responseFile, BuildPaths buildPaths, string[] args, IAnalyzerAssemblyLoader analyzerLoader, Func<string, PEStreamOptions, MetadataReferenceProperties, MetadataImageReference>? createFromFileFunc = null)
		: base(CSharpCommandLineParser.Script, responseFile, args, buildPaths, null, analyzerLoader)
	{
		_createFromFileFunc = createFromFileFunc ?? new Func<string, PEStreamOptions, MetadataReferenceProperties, MetadataImageReference>(Script.CreateFromFile);
	}

	internal override MetadataReferenceResolver GetCommandLineMetadataReferenceResolver(TouchedFileLogger? loggerOpt)
	{
		return CommandLineRunner.GetMetadataReferenceResolver(base.Arguments, loggerOpt, _createFromFileFunc);
	}

	public override void PrintLogo(TextWriter consoleOutput)
	{
		consoleOutput.WriteLine(CSharpScriptingResources.LogoLine1, GetCompilerVersion());
		consoleOutput.WriteLine(CSharpScriptingResources.LogoLine2);
		consoleOutput.WriteLine();
	}

	public override void PrintHelp(TextWriter consoleOutput)
	{
		consoleOutput.Write(CSharpScriptingResources.InteractiveHelp);
	}
}
