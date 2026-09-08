using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Nrbf;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Resources.Extensions.BinaryFormat.Deserializer;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;

namespace System.Resources.Extensions.BinaryFormat;

internal sealed class BinaryFormattedObject
{
	internal interface IParseState
	{
		BinaryReader Reader { get; }

		IReadOnlyDictionary<SerializationRecordId, SerializationRecord> RecordMap { get; }

		Options Options { get; }

		ITypeResolver TypeResolver { get; }
	}

	internal interface ITypeResolver
	{
		[RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetType(String)")]
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
		Type GetType(TypeName typeName);
	}

	internal sealed class Options
	{
		public FormatterAssemblyStyle AssemblyMatching { get; set; }

		public SerializationBinder Binder { get; set; }

		public ISurrogateSelector SurrogateSelector { get; set; }

		public StreamingContext StreamingContext { get; set; } = new StreamingContext(StreamingContextStates.All);
	}

	internal sealed class ParseState : IParseState
	{
		private readonly BinaryFormattedObject _format;

		public BinaryReader Reader { get; }

		public IReadOnlyDictionary<SerializationRecordId, SerializationRecord> RecordMap => _format.RecordMap;

		public Options Options => _format._options;

		public ITypeResolver TypeResolver => _format.TypeResolver;

		public ParseState(BinaryReader reader, BinaryFormattedObject format)
		{
			Reader = reader;
			_format = format;
		}
	}

	internal sealed class DefaultTypeResolver : ITypeResolver
	{
		private sealed class TopLevelAssemblyTypeResolver
		{
			private readonly Assembly _topLevelAssembly;

			public TopLevelAssemblyTypeResolver(Assembly topLevelAssembly)
			{
				_topLevelAssembly = topLevelAssembly;
			}

			[RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetType(String, Boolean, Boolean)")]
			public Type ResolveType(Assembly assembly, string simpleTypeName, bool ignoreCase)
			{
				if ((object)assembly == null)
				{
					assembly = _topLevelAssembly;
				}
				return assembly.GetType(simpleTypeName, throwOnError: false, ignoreCase);
			}
		}

		private readonly FormatterAssemblyStyle _assemblyMatching;

		private readonly SerializationBinder _binder;

		private readonly Dictionary<string, Assembly> _assemblies = new Dictionary<string, Assembly>();

		private readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();

		internal DefaultTypeResolver(Options options)
		{
			_assemblyMatching = options.AssemblyMatching;
			_binder = options.Binder;
		}

		[RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetType(String)")]
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
		Type ITypeResolver.GetType(TypeName typeName)
		{
			if (_types.TryGetValue(typeName.AssemblyQualifiedName, out var value))
			{
				return value;
			}
			Type type = _binder?.BindToType(typeName.AssemblyName.FullName, typeName.FullName);
			if ((object)type != null)
			{
				_types[typeName.AssemblyQualifiedName] = type;
				return type;
			}
			if (!_assemblies.TryGetValue(typeName.AssemblyName.FullName, out var value2))
			{
				AssemblyName assemblyName = typeName.AssemblyName.ToAssemblyName();
				try
				{
					value2 = Assembly.Load(assemblyName);
				}
				catch
				{
					if (_assemblyMatching != FormatterAssemblyStyle.Simple)
					{
						throw;
					}
					value2 = Assembly.Load(assemblyName.Name);
				}
				_assemblies.Add(typeName.AssemblyName.FullName, value2);
			}
			Type type2 = ((_assemblyMatching != FormatterAssemblyStyle.Simple) ? value2.GetType(typeName.FullName) : GetSimplyNamedTypeFromAssembly(value2, typeName));
			_types[typeName.AssemblyQualifiedName] = type2 ?? throw new SerializationException(System.SR.Format(System.SR.Serialization_MissingType, typeName.AssemblyQualifiedName));
			return type2;
		}

		[RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetType(String, Boolean, Boolean)")]
		private static Type GetSimplyNamedTypeFromAssembly(Assembly assembly, TypeName typeName)
		{
			try
			{
				return assembly.GetType(typeName.FullName, throwOnError: false, ignoreCase: false);
			}
			catch (TypeLoadException)
			{
			}
			catch (FileNotFoundException)
			{
			}
			catch (FileLoadException)
			{
			}
			catch (BadImageFormatException)
			{
			}
			return Type.GetType(typeName.FullName, ResolveSimpleAssemblyName, new TopLevelAssemblyTypeResolver(assembly).ResolveType, throwOnError: false);
			static Assembly ResolveSimpleAssemblyName(AssemblyName assemblyName)
			{
				try
				{
					return Assembly.Load(assemblyName);
				}
				catch
				{
				}
				try
				{
					return Assembly.Load(assemblyName.Name);
				}
				catch
				{
				}
				return null;
			}
		}
	}

	private static readonly Options s_defaultOptions = new Options();

	private static readonly PayloadOptions s_payloadOptions = new PayloadOptions
	{
		UndoTruncatedTypeNames = true
	};

	private readonly Options _options;

	[CompilerGenerated]
	private ITypeResolver _003CTypeResolver_003Ek__BackingField;

	internal static FormatterConverter DefaultConverter { get; } = new FormatterConverter();

	private ITypeResolver TypeResolver => _003CTypeResolver_003Ek__BackingField ?? (_003CTypeResolver_003Ek__BackingField = new DefaultTypeResolver(_options));

	public SerializationRecord RootRecord { get; }

	public SerializationRecord this[SerializationRecordId id] => RecordMap[id];

	public IReadOnlyDictionary<SerializationRecordId, SerializationRecord> RecordMap { get; }

	public BinaryFormattedObject(Stream stream, Options options = null)
	{
		_options = options ?? s_defaultOptions;
		try
		{
			RootRecord = NrbfDecoder.Decode(stream, out IReadOnlyDictionary<SerializationRecordId, SerializationRecord> recordMap, s_payloadOptions, leaveOpen: true);
			RecordMap = recordMap;
		}
		catch (Exception ex) when (((ex is ArgumentException || ex is InvalidCastException || ex is ArithmeticException || ex is IOException) ? 1 : 0) != 0)
		{
			throw ex.ConvertToSerializationException();
		}
		catch (TargetInvocationException ex2)
		{
			throw ExceptionDispatchInfo.Capture(ex2.InnerException).SourceException.ConvertToSerializationException();
		}
	}

	[RequiresUnreferencedCode("Ultimately calls Assembly.GetType for type names in the data.")]
	public object Deserialize()
	{
		try
		{
			return System.Resources.Extensions.BinaryFormat.Deserializer.Deserializer.Deserialize(RootRecord.Id, RecordMap, TypeResolver, _options);
		}
		catch (Exception ex) when (((ex is ArgumentException || ex is InvalidCastException || ex is ArithmeticException || ex is IOException) ? 1 : 0) != 0)
		{
			throw ex.ConvertToSerializationException();
		}
		catch (TargetInvocationException ex2)
		{
			throw ExceptionDispatchInfo.Capture(ex2.InnerException).SourceException.ConvertToSerializationException();
		}
	}
}
