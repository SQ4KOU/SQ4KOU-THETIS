using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Scripting;

public static class CSharpScript
{
	public static Script<T> Create<T>(string code, ScriptOptions options = null, Type globalsType = null, InteractiveAssemblyLoader assemblyLoader = null)
	{
		if (code == null)
		{
			throw new ArgumentNullException("code");
		}
		return Script.CreateInitialScript<T>(CSharpScriptCompiler.Instance, SourceText.From(code, options?.FileEncoding, SourceHashAlgorithm.Sha256), options, globalsType, assemblyLoader);
	}

	public static Script<T> Create<T>(Stream code, ScriptOptions options = null, Type globalsType = null, InteractiveAssemblyLoader assemblyLoader = null)
	{
		if (code == null)
		{
			throw new ArgumentNullException("code");
		}
		return Script.CreateInitialScript<T>(CSharpScriptCompiler.Instance, SourceText.From(code, options?.FileEncoding), options, globalsType, assemblyLoader);
	}

	public static Script<object> Create(string code, ScriptOptions options = null, Type globalsType = null, InteractiveAssemblyLoader assemblyLoader = null)
	{
		if (code == null)
		{
			throw new ArgumentNullException("code");
		}
		return Create<object>(code, options, globalsType, assemblyLoader);
	}

	public static Script<object> Create(Stream code, ScriptOptions options = null, Type globalsType = null, InteractiveAssemblyLoader assemblyLoader = null)
	{
		if (code == null)
		{
			throw new ArgumentNullException("code");
		}
		return Create<object>(code, options, globalsType, assemblyLoader);
	}

	public static Task<ScriptState<T>> RunAsync<T>(string code, ScriptOptions options = null, object globals = null, Type globalsType = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return Create<T>(code, options, globalsType ?? globals?.GetType()).RunAsync(globals, cancellationToken);
	}

	public static Task<ScriptState<object>> RunAsync(string code, ScriptOptions options = null, object globals = null, Type globalsType = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return RunAsync<object>(code, options, globals, globalsType, cancellationToken);
	}

	public static Task<T> EvaluateAsync<T>(string code, ScriptOptions options = null, object globals = null, Type globalsType = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return RunAsync<T>(code, options, globals, globalsType, cancellationToken).GetEvaluationResultAsync();
	}

	public static Task<object> EvaluateAsync(string code, ScriptOptions options = null, object globals = null, Type globalsType = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return EvaluateAsync<object>(code, options, globals, globalsType, cancellationToken);
	}
}
