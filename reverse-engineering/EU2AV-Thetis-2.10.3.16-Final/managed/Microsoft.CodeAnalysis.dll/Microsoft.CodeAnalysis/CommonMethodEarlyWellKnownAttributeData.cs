using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis;

internal class CommonMethodEarlyWellKnownAttributeData : EarlyWellKnownAttributeData
{
	private ImmutableArray<string?> _lazyConditionalSymbols = ImmutableArray<string>.Empty;

	private ObsoleteAttributeData _obsoleteAttributeData = Microsoft.CodeAnalysis.ObsoleteAttributeData.Uninitialized;

	private bool _hasSetsRequiredMembers;

	private int _overloadResolutionPriority;

	public ImmutableArray<string?> ConditionalSymbols => _lazyConditionalSymbols;

	public ObsoleteAttributeData? ObsoleteAttributeData
	{
		get
		{
			if (!_obsoleteAttributeData.IsUninitialized)
			{
				return _obsoleteAttributeData;
			}
			return null;
		}
		set
		{
			if (!PEModule.IsMoreImportantObsoleteKind(_obsoleteAttributeData.Kind, value.Kind))
			{
				_obsoleteAttributeData = value;
			}
		}
	}

	public bool HasSetsRequiredMembersAttribute
	{
		get
		{
			return _hasSetsRequiredMembers;
		}
		set
		{
			_hasSetsRequiredMembers = value;
		}
	}

	public int OverloadResolutionPriority
	{
		get
		{
			return _overloadResolutionPriority;
		}
		[param: DisallowNull]
		set
		{
			_overloadResolutionPriority = value;
		}
	}

	public void AddConditionalSymbol(string? name)
	{
		_lazyConditionalSymbols = _lazyConditionalSymbols.Add(name);
	}
}
