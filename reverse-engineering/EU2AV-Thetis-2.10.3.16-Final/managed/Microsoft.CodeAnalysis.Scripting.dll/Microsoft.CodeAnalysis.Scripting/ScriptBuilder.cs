using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Scripting;

internal sealed class ScriptBuilder
{
	private static readonly string s_globalAssemblyNamePrefix;

	private static int s_engineIdDispenser;

	private int _submissionIdDispenser = -1;

	private readonly string _assemblyNamePrefix;

	private readonly InteractiveAssemblyLoader _assemblyLoader;

	private static readonly EmitOptions s_EmitOptionsWithDebuggingInformation;

	static ScriptBuilder()
	{
		DebugInformationFormat platformSpecificDebugInformationFormat = PdbHelpers.GetPlatformSpecificDebugInformationFormat();
		long baseAddress = 0L;
		HashAlgorithmName? pdbChecksumAlgorithm = default(HashAlgorithmName);
		s_EmitOptionsWithDebuggingInformation = new EmitOptions(false, platformSpecificDebugInformationFormat, null, null, 0, (ulong)baseAddress, false, default(SubsystemVersion), null, false, true, default(ImmutableArray<InstrumentationKind>), pdbChecksumAlgorithm, null, null);
		s_globalAssemblyNamePrefix = "ℛ*" + Guid.NewGuid();
	}

	public ScriptBuilder(InteractiveAssemblyLoader assemblyLoader)
	{
		_assemblyNamePrefix = s_globalAssemblyNamePrefix + "#" + Interlocked.Increment(ref s_engineIdDispenser);
		_assemblyLoader = assemblyLoader;
	}

	public int GenerateSubmissionId(out string assemblyName, out string typeName)
	{
		int result = Interlocked.Increment(ref _submissionIdDispenser);
		string text = result.ToString();
		assemblyName = _assemblyNamePrefix + "-" + text;
		typeName = "Submission#" + text;
		return result;
	}

	internal Func<object[], Task<T>> CreateExecutor<T>(ScriptCompiler compiler, Compilation compilation, bool emitDebugInformation, CancellationToken cancellationToken)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			instance.AddRange(compilation.GetParseDiagnostics(cancellationToken));
			ThrowIfAnyCompilationErrors(instance, compiler.DiagnosticFormatter);
			instance.Clear();
			Func<object[], Task<T>> func = Build<T>(compilation, instance, emitDebugInformation, cancellationToken);
			ThrowIfAnyCompilationErrors(instance, compiler.DiagnosticFormatter);
			if (func == null)
			{
				func = (object[] s) => Task.FromResult(default(T));
			}
			return func;
		}
		finally
		{
			instance.Free();
		}
	}

	private static void ThrowIfAnyCompilationErrors(DiagnosticBag diagnostics, DiagnosticFormatter formatter)
	{
		if (!diagnostics.IsEmptyWithoutResolution)
		{
			ImmutableArray<Diagnostic> diagnostics2 = (from d in diagnostics.AsEnumerable()
				where d.Severity == DiagnosticSeverity.Error
				select d).AsImmutable();
			if (!diagnostics2.IsEmpty)
			{
				throw new CompilationErrorException(formatter.Format(diagnostics2[0], CultureInfo.CurrentCulture), diagnostics2);
			}
		}
	}

	private Func<object[], Task<T>> Build<T>(Compilation compilation, DiagnosticBag diagnostics, bool emitDebugInformation, CancellationToken cancellationToken)
	{
		IMethodSymbol entryPoint = compilation.GetEntryPoint(cancellationToken);
		using MemoryStream memoryStream = new MemoryStream();
		using MemoryStream memoryStream2 = (emitDebugInformation ? new MemoryStream() : null);
		EmitResult emitResult = Emit(memoryStream, memoryStream2, compilation, GetEmitOptions(emitDebugInformation), cancellationToken);
		diagnostics.AddRange(emitResult.Diagnostics);
		if (!emitResult.Success)
		{
			return null;
		}
		foreach (KeyValuePair<MetadataReference, IAssemblySymbolInternal> referencedAssembly in compilation.GetBoundReferenceManager().GetReferencedAssemblies())
		{
			string text = (referencedAssembly.Key as PortableExecutableReference)?.FilePath;
			if (text != null)
			{
				_assemblyLoader.RegisterDependency(referencedAssembly.Value.Identity, text);
			}
		}
		memoryStream.Position = 0L;
		if (memoryStream2 != null)
		{
			memoryStream2.Position = 0L;
		}
		Assembly assembly = _assemblyLoader.LoadAssemblyFromStream(memoryStream, memoryStream2);
		return ReflectionUtilities.CreateDelegate<Func<object[], Task<T>>>(GetEntryPointRuntimeMethod(entryPoint, assembly));
	}

	internal static EmitOptions GetEmitOptions(bool emitDebugInformation)
	{
		if (!emitDebugInformation)
		{
			return EmitOptions.Default;
		}
		return s_EmitOptionsWithDebuggingInformation;
	}

	internal static EmitResult Emit(Stream peStream, Stream pdbStreamOpt, Compilation compilation, EmitOptions options, CancellationToken cancellationToken)
	{
		return compilation.Emit(peStream, pdbStreamOpt, null, null, null, options, cancellationToken);
	}

	internal static MethodInfo GetEntryPointRuntimeMethod(IMethodSymbol entryPoint, Assembly assembly)
	{
		string name = MetadataHelpers.BuildQualifiedName(entryPoint.ContainingNamespace.MetadataName, entryPoint.ContainingType.MetadataName);
		string metadataName = entryPoint.MetadataName;
		return assembly.GetType(name, throwOnError: true, ignoreCase: false).GetTypeInfo().GetDeclaredMethod(metadataName);
	}
}
