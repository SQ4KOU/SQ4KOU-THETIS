using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

internal sealed class PEAssembly
{
	internal readonly ImmutableArray<AssemblyIdentity> AssemblyReferences;

	internal readonly ImmutableArray<int> ModuleReferenceCounts;

	private readonly ImmutableArray<PEModule> _modules;

	private readonly AssemblyIdentity _identity;

	private ThreeState _lazyContainsNoPiaLocalTypes;

	private ThreeState _lazyDeclaresTheObjectClass;

	private readonly AssemblyMetadata _owner;

	private Dictionary<string, List<ImmutableArray<byte>>> _lazyInternalsVisibleToMap;

	internal EntityHandle Handle => EntityHandle.AssemblyDefinition;

	internal PEModule ManifestModule => Modules[0];

	internal ImmutableArray<PEModule> Modules => _modules;

	internal AssemblyIdentity Identity => _identity;

	internal bool DeclaresTheObjectClass
	{
		get
		{
			if (_lazyDeclaresTheObjectClass == ThreeState.Unknown)
			{
				bool value = _modules[0].MetadataReader.DeclaresTheObjectClass();
				_lazyDeclaresTheObjectClass = value.ToThreeState();
			}
			return _lazyDeclaresTheObjectClass == ThreeState.True;
		}
	}

	internal PEAssembly(AssemblyMetadata owner, ImmutableArray<PEModule> modules)
	{
		_identity = modules[0].ReadAssemblyIdentityOrThrow();
		int capacity = modules.Sum((PEModule module) => module.ReferencedAssemblies.Length);
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance(modules.Length);
		ArrayBuilder<AssemblyIdentity> instance2 = ArrayBuilder<AssemblyIdentity>.GetInstance(capacity);
		for (int num = 0; num < modules.Length; num++)
		{
			ImmutableArray<AssemblyIdentity> referencedAssemblies = modules[num].ReferencedAssemblies;
			instance.Add(referencedAssemblies.Length);
			instance2.AddRange(referencedAssemblies);
		}
		_modules = modules;
		AssemblyReferences = instance2.ToImmutableAndFree();
		ModuleReferenceCounts = instance.ToImmutableAndFree();
		_owner = owner;
	}

	internal bool ContainsNoPiaLocalTypes()
	{
		if (_lazyContainsNoPiaLocalTypes == ThreeState.Unknown)
		{
			foreach (PEModule module in Modules)
			{
				if (module.ContainsNoPiaLocalTypes())
				{
					_lazyContainsNoPiaLocalTypes = ThreeState.True;
					return true;
				}
			}
			_lazyContainsNoPiaLocalTypes = ThreeState.False;
		}
		return _lazyContainsNoPiaLocalTypes == ThreeState.True;
	}

	private Dictionary<string, List<ImmutableArray<byte>>> BuildInternalsVisibleToMap()
	{
		Dictionary<string, List<ImmutableArray<byte>>> dictionary = new Dictionary<string, List<ImmutableArray<byte>>>(StringComparer.OrdinalIgnoreCase);
		foreach (string internalsVisibleToAttributeValue in Modules[0].GetInternalsVisibleToAttributeValues(Handle))
		{
			if (AssemblyIdentity.TryParseDisplayName(internalsVisibleToAttributeValue, out AssemblyIdentity identity))
			{
				if (dictionary.TryGetValue(identity.Name, out var value))
				{
					value.Add(identity.PublicKey);
					continue;
				}
				value = new List<ImmutableArray<byte>>();
				value.Add(identity.PublicKey);
				dictionary[identity.Name] = value;
			}
		}
		return dictionary;
	}

	internal IEnumerable<ImmutableArray<byte>> GetInternalsVisibleToPublicKeys(string simpleName)
	{
		EnsureInternalsVisibleToMapInitialized();
		_lazyInternalsVisibleToMap.TryGetValue(simpleName, out var value);
		IEnumerable<ImmutableArray<byte>> enumerable = value;
		return enumerable ?? SpecializedCollections.EmptyEnumerable<ImmutableArray<byte>>();
	}

	internal IEnumerable<string> GetInternalsVisibleToAssemblyNames()
	{
		EnsureInternalsVisibleToMapInitialized();
		return _lazyInternalsVisibleToMap.Keys;
	}

	private void EnsureInternalsVisibleToMapInitialized()
	{
		if (_lazyInternalsVisibleToMap == null)
		{
			Interlocked.CompareExchange(ref _lazyInternalsVisibleToMap, BuildInternalsVisibleToMap(), null);
		}
	}

	public AssemblyMetadata GetNonDisposableMetadata()
	{
		return _owner.Copy();
	}
}
