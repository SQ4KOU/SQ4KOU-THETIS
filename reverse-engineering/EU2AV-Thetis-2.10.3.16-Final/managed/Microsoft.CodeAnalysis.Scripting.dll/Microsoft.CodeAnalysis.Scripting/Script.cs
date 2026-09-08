using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Scripting;

public abstract class Script
{
	internal readonly ScriptCompiler Compiler;

	internal readonly ScriptBuilder Builder;

	private Compilation _lazyCompilation;

	internal static readonly MetadataReferenceProperties HostAssemblyReferenceProperties = MetadataReferenceProperties.Assembly.WithAliases(ImmutableArray.Create("<host>")).WithRecursiveAliases(value: true);

	public Script Previous { get; }

	public ScriptOptions Options { get; }

	public string Code => SourceText.ToString();

	internal SourceText SourceText { get; }

	public Type GlobalsType { get; }

	public abstract Type ReturnType { get; }

	internal Script(ScriptCompiler compiler, ScriptBuilder builder, SourceText sourceText, ScriptOptions options, Type globalsTypeOpt, Script previousOpt)
	{
		Compiler = compiler;
		Builder = builder;
		Previous = previousOpt;
		SourceText = sourceText;
		Options = options;
		GlobalsType = globalsTypeOpt;
	}

	internal static Script<T> CreateInitialScript<T>(ScriptCompiler compiler, SourceText sourceText, ScriptOptions optionsOpt, Type globalsTypeOpt, InteractiveAssemblyLoader assemblyLoaderOpt)
	{
		return new Script<T>(compiler, new ScriptBuilder(assemblyLoaderOpt ?? new InteractiveAssemblyLoader()), sourceText, optionsOpt ?? ScriptOptions.Default, globalsTypeOpt, null);
	}

	public Script WithOptions(ScriptOptions options)
	{
		return WithOptionsInternal(options);
	}

	internal abstract Script WithOptionsInternal(ScriptOptions options);

	public Script<object> ContinueWith(string code, ScriptOptions options = null)
	{
		return ContinueWith<object>(code, options);
	}

	public Script<object> ContinueWith(Stream code, ScriptOptions options = null)
	{
		return ContinueWith<object>(code, options);
	}

	public Script<TResult> ContinueWith<TResult>(string code, ScriptOptions options = null)
	{
		options = options ?? InheritOptions(Options);
		return new Script<TResult>(Compiler, Builder, SourceText.From(code ?? "", options.FileEncoding), options, GlobalsType, this);
	}

	public Script<TResult> ContinueWith<TResult>(Stream code, ScriptOptions options = null)
	{
		if (code == null)
		{
			throw new ArgumentNullException("code");
		}
		options = options ?? InheritOptions(Options);
		return new Script<TResult>(Compiler, Builder, SourceText.From(code, options.FileEncoding), options, GlobalsType, this);
	}

	private static ScriptOptions InheritOptions(ScriptOptions previous)
	{
		return previous.WithReferences(ImmutableArray<MetadataReference>.Empty).WithImports(ImmutableArray<string>.Empty);
	}

	public Compilation GetCompilation()
	{
		if (_lazyCompilation == null)
		{
			Compilation value = Compiler.CreateSubmission(this);
			Interlocked.CompareExchange(ref _lazyCompilation, value, null);
		}
		return _lazyCompilation;
	}

	internal Task<object> EvaluateAsync(object globals = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return CommonEvaluateAsync(globals, cancellationToken);
	}

	internal abstract Task<object> CommonEvaluateAsync(object globals, CancellationToken cancellationToken);

	public Task<ScriptState> RunAsync(object globals, CancellationToken cancellationToken)
	{
		return CommonRunAsync(globals, null, cancellationToken);
	}

	public Task<ScriptState> RunAsync(object globals = null, Func<Exception, bool> catchException = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return CommonRunAsync(globals, catchException, cancellationToken);
	}

	internal abstract Task<ScriptState> CommonRunAsync(object globals, Func<Exception, bool> catchException, CancellationToken cancellationToken);

	public Task<ScriptState> RunFromAsync(ScriptState previousState, CancellationToken cancellationToken)
	{
		return CommonRunFromAsync(previousState, null, cancellationToken);
	}

	public Task<ScriptState> RunFromAsync(ScriptState previousState, Func<Exception, bool> catchException = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return CommonRunFromAsync(previousState, catchException, cancellationToken);
	}

	internal abstract Task<ScriptState> CommonRunFromAsync(ScriptState previousState, Func<Exception, bool> catchException, CancellationToken cancellationToken);

	public ImmutableArray<Diagnostic> Compile(CancellationToken cancellationToken = default(CancellationToken))
	{
		return CommonCompile(cancellationToken);
	}

	internal abstract ImmutableArray<Diagnostic> CommonCompile(CancellationToken cancellationToken);

	internal abstract Func<object[], Task> CommonGetExecutor(CancellationToken cancellationToken);

	internal ImmutableArray<MetadataReference> GetReferencesForCompilation(CommonMessageProvider messageProvider, DiagnosticBag diagnostics, MetadataReference languageRuntimeReferenceOpt = null)
	{
		MetadataReferenceResolver metadataResolver = Options.MetadataResolver;
		ArrayBuilder<MetadataReference> instance = ArrayBuilder<MetadataReference>.GetInstance();
		try
		{
			if (Previous == null)
			{
				MetadataImageReference item = CreateFromAssembly(typeof(object).GetTypeInfo().Assembly, default(MetadataReferenceProperties), Options.CreateFromFileFunc);
				instance.Add(item);
				if (GlobalsType != null)
				{
					Assembly assembly = GlobalsType.GetTypeInfo().Assembly;
					if (MetadataReference.HasMetadata(assembly))
					{
						instance.Add(CreateFromAssembly(assembly, HostAssemblyReferenceProperties, Options.CreateFromFileFunc));
					}
				}
				if (languageRuntimeReferenceOpt != null)
				{
					instance.Add(languageRuntimeReferenceOpt);
				}
			}
			foreach (MetadataReference metadataReference in Options.MetadataReferences)
			{
				if (metadataReference is UnresolvedMetadataReference unresolvedMetadataReference)
				{
					ImmutableArray<PortableExecutableReference> items = metadataResolver.ResolveReference(unresolvedMetadataReference.Reference, null, unresolvedMetadataReference.Properties);
					if (items.IsDefault)
					{
						diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_MetadataFileNotFound, Location.None, unresolvedMetadataReference.Reference));
					}
					else
					{
						instance.AddRange(items);
					}
				}
				else
				{
					instance.Add(metadataReference);
				}
			}
			return instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
	}

	internal bool HasReturnValue()
	{
		return GetCompilation().HasSubmissionResult();
	}

	internal static MetadataImageReference CreateFromAssembly(Assembly assembly, MetadataReferenceProperties properties, Func<string, PEStreamOptions, MetadataReferenceProperties, MetadataImageReference> createFromFileFunc)
	{
		string assemblyFilePath = MetadataReference.GetAssemblyFilePath(assembly, properties);
		return createFromFileFunc(assemblyFilePath, PEStreamOptions.Default, properties);
	}

	internal static MetadataImageReference CreateFromFile(string filePath, PEStreamOptions options, MetadataReferenceProperties properties)
	{
		return MetadataReference.CreateFromFile(filePath, options, properties);
	}
}
public sealed class Script<T> : Script
{
	private ImmutableArray<Func<object[], Task>> _lazyPrecedingExecutors;

	private Func<object[], Task<T>> _lazyExecutor;

	public override Type ReturnType => typeof(T);

	internal Script(ScriptCompiler compiler, ScriptBuilder builder, SourceText sourceText, ScriptOptions options, Type globalsTypeOpt, Script previousOpt)
		: base(compiler, builder, sourceText, options, globalsTypeOpt, previousOpt)
	{
	}

	public new Script<T> WithOptions(ScriptOptions options)
	{
		if (options != base.Options)
		{
			return new Script<T>(Compiler, Builder, base.SourceText, options, base.GlobalsType, base.Previous);
		}
		return this;
	}

	internal override Script WithOptionsInternal(ScriptOptions options)
	{
		return WithOptions(options);
	}

	internal override ImmutableArray<Diagnostic> CommonCompile(CancellationToken cancellationToken)
	{
		try
		{
			GetPrecedingExecutors(cancellationToken);
			GetExecutor(cancellationToken);
			return ImmutableArray.CreateRange(from d in GetCompilation().GetDiagnostics(cancellationToken)
				where d.Severity == DiagnosticSeverity.Warning
				select d);
		}
		catch (CompilationErrorException ex)
		{
			return ImmutableArray.CreateRange(ex.Diagnostics.Where(delegate(Diagnostic d)
			{
				DiagnosticSeverity severity = d.Severity;
				return (uint)(severity - 2) <= 1u;
			}));
		}
	}

	internal override Func<object[], Task> CommonGetExecutor(CancellationToken cancellationToken)
	{
		return GetExecutor(cancellationToken);
	}

	internal override Task<object> CommonEvaluateAsync(object globals, CancellationToken cancellationToken)
	{
		return EvaluateAsync(globals, cancellationToken).CastAsync<T, object>();
	}

	internal override Task<ScriptState> CommonRunAsync(object globals, Func<Exception, bool> catchException, CancellationToken cancellationToken)
	{
		return RunAsync(globals, catchException, cancellationToken).CastAsync<ScriptState<T>, ScriptState>();
	}

	internal override Task<ScriptState> CommonRunFromAsync(ScriptState previousState, Func<Exception, bool> catchException, CancellationToken cancellationToken)
	{
		return RunFromAsync(previousState, catchException, cancellationToken).CastAsync<ScriptState<T>, ScriptState>();
	}

	private Func<object[], Task<T>> GetExecutor(CancellationToken cancellationToken)
	{
		if (_lazyExecutor == null)
		{
			Interlocked.CompareExchange(ref _lazyExecutor, Builder.CreateExecutor<T>(Compiler, GetCompilation(), base.Options.EmitDebugInformation, cancellationToken), null);
		}
		return _lazyExecutor;
	}

	private ImmutableArray<Func<object[], Task>> GetPrecedingExecutors(CancellationToken cancellationToken)
	{
		if (_lazyPrecedingExecutors.IsDefault)
		{
			ImmutableArray<Func<object[], Task>> initializedValue = TryGetPrecedingExecutors(null, cancellationToken);
			InterlockedOperations.Initialize(ref _lazyPrecedingExecutors, initializedValue);
		}
		return _lazyPrecedingExecutors;
	}

	private ImmutableArray<Func<object[], Task>> TryGetPrecedingExecutors(Script lastExecutedScriptInChainOpt, CancellationToken cancellationToken)
	{
		Script previous = base.Previous;
		if (previous == lastExecutedScriptInChainOpt)
		{
			return ImmutableArray<Func<object[], Task>>.Empty;
		}
		ArrayBuilder<Script> instance = ArrayBuilder<Script>.GetInstance();
		while (previous != null && previous != lastExecutedScriptInChainOpt)
		{
			instance.Add(previous);
			previous = previous.Previous;
		}
		if (lastExecutedScriptInChainOpt != null && previous != lastExecutedScriptInChainOpt)
		{
			instance.Free();
			return default(ImmutableArray<Func<object[], Task>>);
		}
		ArrayBuilder<Func<object[], Task>> instance2 = ArrayBuilder<Func<object[], Task>>.GetInstance(instance.Count);
		for (int num = instance.Count - 1; num >= 0; num--)
		{
			instance2.Add(instance[num].CommonGetExecutor(cancellationToken));
		}
		return instance2.ToImmutableAndFree();
	}

	internal new Task<T> EvaluateAsync(object globals = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return RunAsync(globals, cancellationToken).GetEvaluationResultAsync();
	}

	public new Task<ScriptState<T>> RunAsync(object globals, CancellationToken cancellationToken)
	{
		return RunAsync(globals, null, cancellationToken);
	}

	public new Task<ScriptState<T>> RunAsync(object globals = null, Func<Exception, bool> catchException = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		ValidateGlobals(globals, base.GlobalsType);
		ScriptExecutionState executionState = ScriptExecutionState.Create(globals);
		ImmutableArray<Func<object[], Task>> precedingExecutors = GetPrecedingExecutors(cancellationToken);
		Func<object[], Task<T>> executor = GetExecutor(cancellationToken);
		return RunSubmissionsAsync(executionState, precedingExecutors, executor, catchException, cancellationToken);
	}

	public ScriptRunner<T> CreateDelegate(CancellationToken cancellationToken = default(CancellationToken))
	{
		ImmutableArray<Func<object[], Task>> precedingExecutors = GetPrecedingExecutors(cancellationToken);
		Func<object[], Task<T>> currentExecutor = GetExecutor(cancellationToken);
		Type globalsType = base.GlobalsType;
		return delegate(object globals, CancellationToken token)
		{
			ValidateGlobals(globals, globalsType);
			return ScriptExecutionState.Create(globals).RunSubmissionsAsync<T>(precedingExecutors, currentExecutor, null, null, token);
		};
	}

	public new Task<ScriptState<T>> RunFromAsync(ScriptState previousState, CancellationToken cancellationToken)
	{
		return RunFromAsync(previousState, null, cancellationToken);
	}

	public new Task<ScriptState<T>> RunFromAsync(ScriptState previousState, Func<Exception, bool> catchException = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (previousState == null)
		{
			throw new ArgumentNullException("previousState");
		}
		if (previousState.Script == this)
		{
			return Task.FromResult((ScriptState<T>)previousState);
		}
		ImmutableArray<Func<object[], Task>> precedingExecutors = TryGetPrecedingExecutors(previousState.Script, cancellationToken);
		if (precedingExecutors.IsDefault)
		{
			throw new ArgumentException(ScriptingResources.StartingStateIncompatible, "previousState");
		}
		Func<object[], Task<T>> executor = GetExecutor(cancellationToken);
		ScriptExecutionState executionState = previousState.ExecutionState.FreezeAndClone();
		return RunSubmissionsAsync(executionState, precedingExecutors, executor, catchException, cancellationToken);
	}

	private async Task<ScriptState<T>> RunSubmissionsAsync(ScriptExecutionState executionState, ImmutableArray<Func<object[], Task>> precedingExecutors, Func<object[], Task> currentExecutor, Func<Exception, bool> catchExceptionOpt, CancellationToken cancellationToken)
	{
		StrongBox<Exception> exceptionOpt = ((catchExceptionOpt != null) ? new StrongBox<Exception>() : null);
		return new ScriptState<T>(executionState, this, await executionState.RunSubmissionsAsync<T>(precedingExecutors, currentExecutor, exceptionOpt, catchExceptionOpt, cancellationToken).ConfigureAwait(continueOnCapturedContext: false), exceptionOpt?.Value);
	}

	private static void ValidateGlobals(object globals, Type globalsType)
	{
		if (globalsType != null)
		{
			if (globals == null)
			{
				throw new ArgumentException(ScriptingResources.ScriptRequiresGlobalVariables, "globals");
			}
			System.Reflection.TypeInfo typeInfo = globals.GetType().GetTypeInfo();
			System.Reflection.TypeInfo typeInfo2 = globalsType.GetTypeInfo();
			if (!typeInfo2.IsAssignableFrom(typeInfo))
			{
				throw new ArgumentException(string.Format(ScriptingResources.GlobalsNotAssignable, typeInfo, typeInfo2), "globals");
			}
		}
		else if (globals != null)
		{
			throw new ArgumentException(ScriptingResources.GlobalVariablesWithoutGlobalType, "globals");
		}
	}
}
