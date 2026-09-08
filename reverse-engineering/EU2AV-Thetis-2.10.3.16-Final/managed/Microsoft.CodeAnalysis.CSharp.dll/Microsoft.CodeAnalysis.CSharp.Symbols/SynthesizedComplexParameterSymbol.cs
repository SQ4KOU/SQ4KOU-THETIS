using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedComplexParameterSymbol : SynthesizedParameterSymbolBase
{
	private readonly ImmutableArray<CustomModifier> _refCustomModifiers;

	private readonly ParameterSymbol? _baseParameterForAttributes;

	private readonly ConstantValue? _defaultValue;

	private readonly bool _isParams;

	private readonly bool _hasUnscopedRefAttribute;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => _refCustomModifiers;

	internal override bool HasEnumeratorCancellationAttribute => _baseParameterForAttributes?.HasEnumeratorCancellationAttribute ?? false;

	internal override MarshalPseudoCustomAttributeData? MarshallingInformation => _baseParameterForAttributes?.MarshallingInformation;

	public override bool IsParamsArray
	{
		get
		{
			if (_isParams)
			{
				return base.Type.IsSZArray();
			}
			return false;
		}
	}

	public override bool IsParamsCollection
	{
		get
		{
			if (_isParams)
			{
				return !base.Type.IsSZArray();
			}
			return false;
		}
	}

	internal override bool HasUnscopedRefAttribute => _hasUnscopedRefAttribute;

	internal override bool IsMetadataOptional => _baseParameterForAttributes?.IsMetadataOptional ?? base.IsMetadataOptional;

	internal override bool IsCallerLineNumber => _baseParameterForAttributes?.IsCallerLineNumber ?? false;

	internal override bool IsCallerFilePath => _baseParameterForAttributes?.IsCallerFilePath ?? false;

	internal override bool IsCallerMemberName => _baseParameterForAttributes?.IsCallerMemberName ?? false;

	internal override bool IsMetadataIn
	{
		get
		{
			RefKind refKind = RefKind;
			if (refKind - 3 > RefKind.Ref)
			{
				return _baseParameterForAttributes?.IsMetadataIn ?? false;
			}
			return true;
		}
	}

	internal override bool IsMetadataOut
	{
		get
		{
			if (RefKind != RefKind.Out)
			{
				return _baseParameterForAttributes?.IsMetadataOut ?? false;
			}
			return true;
		}
	}

	internal override ConstantValue? ExplicitDefaultConstantValue => _defaultValue;

	internal override ConstantValue? DefaultValueFromAttributes => _baseParameterForAttributes?.DefaultValueFromAttributes ?? null;

	internal override FlowAnalysisAnnotations FlowAnalysisAnnotations => base.FlowAnalysisAnnotations;

	internal override ImmutableHashSet<string> NotNullIfParameterNotNull => base.NotNullIfParameterNotNull;

	public SynthesizedComplexParameterSymbol(Symbol? container, TypeWithAnnotations type, int ordinal, RefKind refKind, ScopedKind scope, ConstantValue? defaultValue, string name, ImmutableArray<CustomModifier> refCustomModifiers, ParameterSymbol? baseParameterForAttributes, bool isParams, bool hasUnscopedRefAttribute)
		: base(container, type, ordinal, refKind, scope, name)
	{
		_refCustomModifiers = refCustomModifiers;
		_baseParameterForAttributes = baseParameterForAttributes;
		_defaultValue = defaultValue;
		_isParams = isParams;
		_hasUnscopedRefAttribute = hasUnscopedRefAttribute;
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return _baseParameterForAttributes?.GetAttributes() ?? ImmutableArray<CSharpAttributeData>.Empty;
	}
}
