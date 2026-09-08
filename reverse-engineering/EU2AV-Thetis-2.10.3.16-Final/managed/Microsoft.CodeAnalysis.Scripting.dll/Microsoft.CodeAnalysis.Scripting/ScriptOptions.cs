using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using Microsoft.CodeAnalysis.Scripting.Hosting;

namespace Microsoft.CodeAnalysis.Scripting;

public sealed class ScriptOptions
{
	private static readonly MetadataReferenceProperties s_assemblyReferenceProperties = MetadataReferenceProperties.Assembly.WithRecursiveAliases(value: true);

	public static ScriptOptions Default { get; } = new ScriptOptions(string.Empty, GetDefaultMetadataReferences(), ImmutableArray<string>.Empty, ScriptMetadataResolver.Default, SourceFileResolver.Default, emitDebugInformation: false, null, OptimizationLevel.Debug, checkOverflow: false, allowUnsafe: true, 4, null);

	public ImmutableArray<MetadataReference> MetadataReferences { get; private set; }

	public MetadataReferenceResolver MetadataResolver { get; private set; }

	public SourceReferenceResolver SourceResolver { get; private set; }

	public ImmutableArray<string> Imports { get; private set; }

	public bool EmitDebugInformation { get; private set; }

	public Encoding? FileEncoding { get; private set; }

	public string FilePath { get; private set; }

	public OptimizationLevel OptimizationLevel { get; private set; }

	public bool CheckOverflow { get; private set; }

	public bool AllowUnsafe { get; private set; }

	public int WarningLevel { get; private set; }

	internal ParseOptions? ParseOptions { get; private set; }

	internal Func<string, PEStreamOptions, MetadataReferenceProperties, MetadataImageReference> CreateFromFileFunc { get; private set; }

	private static ImmutableArray<MetadataReference> GetDefaultMetadataReferences()
	{
		if (GacFileResolver.IsAvailable)
		{
			return ImmutableArray<MetadataReference>.Empty;
		}
		return ImmutableArray.CreateRange(new string[25]
		{
			"System.Collections", "System.Collections.Concurrent", "System.Console", "System.Diagnostics.Debug", "System.Diagnostics.Process", "System.Diagnostics.StackTrace", "System.Globalization", "System.IO", "System.IO.FileSystem", "System.IO.FileSystem.Primitives",
			"System.Reflection", "System.Reflection.Extensions", "System.Reflection.Primitives", "System.Runtime", "System.Runtime.Extensions", "System.Runtime.InteropServices", "System.Text.Encoding", "System.Text.Encoding.CodePages", "System.Text.Encoding.Extensions", "System.Text.RegularExpressions",
			"System.Threading", "System.Threading.Tasks", "System.Threading.Tasks.Parallel", "System.Threading.Thread", "System.ValueTuple"
		}.Select(CreateUnresolvedReference));
	}

	internal ScriptOptions(string filePath, ImmutableArray<MetadataReference> references, ImmutableArray<string> namespaces, MetadataReferenceResolver metadataResolver, SourceReferenceResolver sourceResolver, bool emitDebugInformation, Encoding? fileEncoding, OptimizationLevel optimizationLevel, bool checkOverflow, bool allowUnsafe, int warningLevel, ParseOptions? parseOptions, Func<string, PEStreamOptions, MetadataReferenceProperties, MetadataImageReference>? createFromFileFunc = null)
	{
		FilePath = filePath;
		MetadataReferences = references;
		Imports = namespaces;
		MetadataResolver = metadataResolver;
		SourceResolver = sourceResolver;
		EmitDebugInformation = emitDebugInformation;
		FileEncoding = fileEncoding;
		OptimizationLevel = optimizationLevel;
		CheckOverflow = checkOverflow;
		AllowUnsafe = allowUnsafe;
		WarningLevel = warningLevel;
		ParseOptions = parseOptions;
		CreateFromFileFunc = createFromFileFunc ?? new Func<string, PEStreamOptions, MetadataReferenceProperties, MetadataImageReference>(Script.CreateFromFile);
	}

	private ScriptOptions(ScriptOptions other)
		: this(other.FilePath, other.MetadataReferences, other.Imports, other.MetadataResolver, other.SourceResolver, other.EmitDebugInformation, other.FileEncoding, other.OptimizationLevel, other.CheckOverflow, other.AllowUnsafe, other.WarningLevel, other.ParseOptions, other.CreateFromFileFunc)
	{
	}

	public ScriptOptions WithFilePath(string? filePath)
	{
		if (!(FilePath == filePath))
		{
			return new ScriptOptions(this)
			{
				FilePath = (filePath ?? "")
			};
		}
		return this;
	}

	private static MetadataReference CreateUnresolvedReference(string reference)
	{
		return new UnresolvedMetadataReference(reference, s_assemblyReferenceProperties);
	}

	private ScriptOptions WithReferences(ImmutableArray<MetadataReference> references)
	{
		if (!MetadataReferences.Equals(references))
		{
			return new ScriptOptions(this)
			{
				MetadataReferences = ParameterValidationHelpers.CheckImmutableArray(references, "references")
			};
		}
		return this;
	}

	public ScriptOptions WithReferences(IEnumerable<MetadataReference> references)
	{
		return WithReferences(ParameterValidationHelpers.ToImmutableArrayChecked(references, "references"));
	}

	public ScriptOptions WithReferences(params MetadataReference[] references)
	{
		return WithReferences((IEnumerable<MetadataReference>)references);
	}

	public ScriptOptions AddReferences(IEnumerable<MetadataReference> references)
	{
		return WithReferences(ParameterValidationHelpers.ConcatChecked(MetadataReferences, references, "references"));
	}

	public ScriptOptions AddReferences(params MetadataReference[] references)
	{
		return AddReferences((IEnumerable<MetadataReference>)references);
	}

	public ScriptOptions WithReferences(IEnumerable<Assembly> references)
	{
		return WithReferences(ParameterValidationHelpers.SelectChecked(references, "references", CreateFromAssembly));
	}

	public ScriptOptions WithReferences(params Assembly[] references)
	{
		return WithReferences((IEnumerable<Assembly>)references);
	}

	public ScriptOptions AddReferences(IEnumerable<Assembly> references)
	{
		return AddReferences(ParameterValidationHelpers.SelectChecked(references, "references", CreateFromAssembly));
	}

	private MetadataImageReference CreateFromAssembly(Assembly assembly)
	{
		return Script.CreateFromAssembly(assembly, s_assemblyReferenceProperties, CreateFromFileFunc);
	}

	public ScriptOptions AddReferences(params Assembly[] references)
	{
		return AddReferences((IEnumerable<Assembly>)references);
	}

	public ScriptOptions WithReferences(IEnumerable<string> references)
	{
		return WithReferences(ParameterValidationHelpers.SelectChecked(references, "references", CreateUnresolvedReference));
	}

	public ScriptOptions WithReferences(params string[] references)
	{
		return WithReferences((IEnumerable<string>)references);
	}

	public ScriptOptions AddReferences(IEnumerable<string> references)
	{
		return AddReferences(ParameterValidationHelpers.SelectChecked(references, "references", CreateUnresolvedReference));
	}

	public ScriptOptions AddReferences(params string[] references)
	{
		return AddReferences((IEnumerable<string>)references);
	}

	public ScriptOptions WithMetadataResolver(MetadataReferenceResolver resolver)
	{
		if (MetadataResolver != resolver)
		{
			return new ScriptOptions(this)
			{
				MetadataResolver = resolver
			};
		}
		return this;
	}

	public ScriptOptions WithSourceResolver(SourceReferenceResolver resolver)
	{
		if (SourceResolver != resolver)
		{
			return new ScriptOptions(this)
			{
				SourceResolver = resolver
			};
		}
		return this;
	}

	private ScriptOptions WithImports(ImmutableArray<string> imports)
	{
		if (!Imports.Equals(imports))
		{
			return new ScriptOptions(this)
			{
				Imports = ParameterValidationHelpers.CheckImmutableArray(imports, "imports")
			};
		}
		return this;
	}

	public ScriptOptions WithImports(IEnumerable<string> imports)
	{
		return WithImports(ParameterValidationHelpers.ToImmutableArrayChecked(imports, "imports"));
	}

	public ScriptOptions WithImports(params string[] imports)
	{
		return WithImports((IEnumerable<string>)imports);
	}

	public ScriptOptions AddImports(IEnumerable<string> imports)
	{
		return WithImports(ParameterValidationHelpers.ConcatChecked(Imports, imports, "imports"));
	}

	public ScriptOptions AddImports(params string[] imports)
	{
		return AddImports((IEnumerable<string>)imports);
	}

	public ScriptOptions WithEmitDebugInformation(bool emitDebugInformation)
	{
		if (emitDebugInformation != EmitDebugInformation)
		{
			return new ScriptOptions(this)
			{
				EmitDebugInformation = emitDebugInformation
			};
		}
		return this;
	}

	public ScriptOptions WithFileEncoding(Encoding encoding)
	{
		if (encoding != FileEncoding)
		{
			return new ScriptOptions(this)
			{
				FileEncoding = encoding
			};
		}
		return this;
	}

	public ScriptOptions WithOptimizationLevel(OptimizationLevel optimizationLevel)
	{
		if (optimizationLevel != OptimizationLevel)
		{
			return new ScriptOptions(this)
			{
				OptimizationLevel = optimizationLevel
			};
		}
		return this;
	}

	public ScriptOptions WithAllowUnsafe(bool allowUnsafe)
	{
		if (allowUnsafe != AllowUnsafe)
		{
			return new ScriptOptions(this)
			{
				AllowUnsafe = allowUnsafe
			};
		}
		return this;
	}

	public ScriptOptions WithCheckOverflow(bool checkOverflow)
	{
		if (checkOverflow != CheckOverflow)
		{
			return new ScriptOptions(this)
			{
				CheckOverflow = checkOverflow
			};
		}
		return this;
	}

	public ScriptOptions WithWarningLevel(int warningLevel)
	{
		if (warningLevel != WarningLevel)
		{
			return new ScriptOptions(this)
			{
				WarningLevel = warningLevel
			};
		}
		return this;
	}

	internal ScriptOptions WithParseOptions(ParseOptions parseOptions)
	{
		if (!(parseOptions == ParseOptions))
		{
			return new ScriptOptions(this)
			{
				ParseOptions = parseOptions
			};
		}
		return this;
	}

	internal ScriptOptions WithCreateFromFileFunc(Func<string, PEStreamOptions, MetadataReferenceProperties, MetadataImageReference> createFromFileFunc)
	{
		return new ScriptOptions(this)
		{
			CreateFromFileFunc = createFromFileFunc
		};
	}
}
