using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceClonedParameterSymbol : SourceParameterSymbolBase
{
	private readonly bool _suppressOptional;

	protected readonly SourceParameterSymbol _originalParam;

	public override bool IsImplicitlyDeclared => true;

	public override bool IsDiscard => _originalParam.IsDiscard;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override bool IsParamsArray
	{
		get
		{
			if (!_suppressOptional)
			{
				return _originalParam.IsParamsArray;
			}
			return false;
		}
	}

	public override bool IsParamsCollection
	{
		get
		{
			if (!_suppressOptional)
			{
				return _originalParam.IsParamsCollection;
			}
			return false;
		}
	}

	internal override bool IsMetadataOptional
	{
		get
		{
			if (!_suppressOptional)
			{
				return _originalParam.IsMetadataOptional;
			}
			return _originalParam.HasOptionalAttribute;
		}
	}

	internal sealed override ScopedKind DeclaredScope => _originalParam.DeclaredScope;

	internal sealed override ScopedKind EffectiveScope => _originalParam.EffectiveScope;

	internal override bool HasUnscopedRefAttribute => _originalParam.HasUnscopedRefAttribute;

	internal sealed override bool UseUpdatedEscapeRules => _originalParam.UseUpdatedEscapeRules;

	internal override ConstantValue? ExplicitDefaultConstantValue
	{
		get
		{
			if (!_suppressOptional)
			{
				return _originalParam.ExplicitDefaultConstantValue;
			}
			return _originalParam.DefaultValueFromAttributes;
		}
	}

	internal override ConstantValue? DefaultValueFromAttributes => _originalParam.DefaultValueFromAttributes;

	public override TypeWithAnnotations TypeWithAnnotations => _originalParam.TypeWithAnnotations;

	public override RefKind RefKind => _originalParam.RefKind;

	internal override bool IsMetadataIn => _originalParam.IsMetadataIn;

	internal override bool IsMetadataOut => _originalParam.IsMetadataOut;

	public override ImmutableArray<Location> Locations => _originalParam.Locations;

	public sealed override string Name => _originalParam.Name;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => _originalParam.RefCustomModifiers;

	internal sealed override bool HasEnumeratorCancellationAttribute => _originalParam.HasEnumeratorCancellationAttribute;

	internal override MarshalPseudoCustomAttributeData MarshallingInformation => _originalParam.MarshallingInformation;

	internal override bool IsIDispatchConstant => _originalParam.IsIDispatchConstant;

	internal override bool IsIUnknownConstant => _originalParam.IsIUnknownConstant;

	internal override FlowAnalysisAnnotations FlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

	internal override ImmutableHashSet<string> NotNullIfParameterNotNull => ImmutableHashSet<string>.Empty;

	internal override ImmutableArray<int> InterpolatedStringHandlerArgumentIndexes
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/SourceClonedParameterSymbol.cs", 162);
		}
	}

	internal override bool HasInterpolatedStringHandlerArgumentError
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/SourceClonedParameterSymbol.cs", 164);
		}
	}

	internal SourceClonedParameterSymbol(SourceParameterSymbol originalParam, Symbol newOwner, int newOrdinal, bool suppressOptional)
		: base(newOwner, newOrdinal)
	{
		_suppressOptional = suppressOptional;
		_originalParam = originalParam;
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return _originalParam.GetAttributes();
	}
}
