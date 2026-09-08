using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Scripting;

internal sealed class CSharpScriptCompiler : ScriptCompiler
{
	public static readonly ScriptCompiler Instance = new CSharpScriptCompiler();

	internal static readonly CSharpParseOptions DefaultParseOptions = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Parse, SourceCodeKind.Script);

	public override DiagnosticFormatter DiagnosticFormatter => CSharpDiagnosticFormatter.Instance;

	public override StringComparer IdentifierComparer => StringComparer.Ordinal;

	private CSharpScriptCompiler()
	{
	}

	public override bool IsCompleteSubmission(SyntaxTree tree)
	{
		return SyntaxFactory.IsCompleteSubmission(tree);
	}

	public override SyntaxTree ParseSubmission(SourceText text, ParseOptions parseOptions, CancellationToken cancellationToken)
	{
		return SyntaxFactory.ParseSyntaxTree(text, parseOptions ?? DefaultParseOptions, "", cancellationToken);
	}

	public override Compilation CreateSubmission(Script script)
	{
		CSharpCompilation previousScriptCompilation = null;
		if (script.Previous != null)
		{
			previousScriptCompilation = (CSharpCompilation)script.Previous.GetCompilation();
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		ImmutableArray<MetadataReference> referencesForCompilation = script.GetReferencesForCompilation(MessageProvider.Instance, instance);
		instance.Free();
		SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(script.SourceText, script.Options.ParseOptions ?? DefaultParseOptions, script.Options.FilePath);
		script.Builder.GenerateSubmissionId(out var assemblyName, out var typeName);
		string assemblyName2 = assemblyName;
		object references = referencesForCompilation;
		string scriptClassName = typeName;
		object usings = script.Options.Imports;
		OptimizationLevel optimizationLevel = script.Options.OptimizationLevel;
		bool checkOverflow = script.Options.CheckOverflow;
		bool allowUnsafe = script.Options.AllowUnsafe;
		int warningLevel = script.Options.WarningLevel;
		SourceReferenceResolver sourceResolver = script.Options.SourceResolver;
		MetadataReferenceResolver metadataResolver = script.Options.MetadataResolver;
		AssemblyIdentityComparer assemblyIdentityComparer = DesktopAssemblyIdentityComparer.Default;
		return CSharpCompilation.CreateScriptCompilation(assemblyName2, syntaxTree, (IEnumerable<MetadataReference>?)references, WithTopLevelBinderFlags(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, reportSuppressedDiagnostics: false, null, null, scriptClassName, (IEnumerable<string>?)usings, optimizationLevel, checkOverflow, allowUnsafe, null, null, default(ImmutableArray<byte>), null, Platform.AnyCpu, ReportDiagnostic.Default, warningLevel, null, concurrentBuild: true, deterministic: false, null, sourceResolver, metadataResolver, assemblyIdentityComparer)), previousScriptCompilation, script.ReturnType, script.GlobalsType);
	}

	internal static CSharpCompilationOptions WithTopLevelBinderFlags(CSharpCompilationOptions options)
	{
		return options.WithTopLevelBinderFlags(BinderFlags.IgnoreCorLibraryDuplicatedTypes);
	}
}
