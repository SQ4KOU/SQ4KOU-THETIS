using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class WrappedParameterSymbol : ParameterSymbol
{
	protected readonly ParameterSymbol _underlyingParameter;

	public ParameterSymbol UnderlyingParameter => _underlyingParameter;

	public sealed override bool IsDiscard => _underlyingParameter.IsDiscard;

	public override TypeWithAnnotations TypeWithAnnotations => _underlyingParameter.TypeWithAnnotations;

	public sealed override RefKind RefKind => _underlyingParameter.RefKind;

	internal sealed override bool IsMetadataIn => _underlyingParameter.IsMetadataIn;

	internal sealed override bool IsMetadataOut => _underlyingParameter.IsMetadataOut;

	public sealed override ImmutableArray<Location> Locations => _underlyingParameter.Locations;

	public sealed override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => _underlyingParameter.DeclaringSyntaxReferences;

	internal sealed override ConstantValue? ExplicitDefaultConstantValue => _underlyingParameter.ExplicitDefaultConstantValue;

	internal sealed override ConstantValue? DefaultValueFromAttributes => _underlyingParameter.DefaultValueFromAttributes;

	public override int Ordinal => _underlyingParameter.Ordinal;

	public override bool IsParamsArray => _underlyingParameter.IsParamsArray;

	public override bool IsParamsCollection => _underlyingParameter.IsParamsCollection;

	internal override bool IsMetadataOptional => _underlyingParameter.IsMetadataOptional;

	public override bool IsImplicitlyDeclared => _underlyingParameter.IsImplicitlyDeclared;

	public sealed override string Name => _underlyingParameter.Name;

	public sealed override string MetadataName => _underlyingParameter.MetadataName;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => _underlyingParameter.RefCustomModifiers;

	internal override MarshalPseudoCustomAttributeData? MarshallingInformation => _underlyingParameter.MarshallingInformation;

	internal override UnmanagedType MarshallingType => _underlyingParameter.MarshallingType;

	internal override bool IsIDispatchConstant => _underlyingParameter.IsIDispatchConstant;

	internal override bool IsIUnknownConstant => _underlyingParameter.IsIUnknownConstant;

	internal override FlowAnalysisAnnotations FlowAnalysisAnnotations => _underlyingParameter.FlowAnalysisAnnotations;

	internal override ImmutableHashSet<string> NotNullIfParameterNotNull => _underlyingParameter.NotNullIfParameterNotNull;

	internal sealed override ScopedKind DeclaredScope => _underlyingParameter.DeclaredScope;

	internal sealed override ScopedKind EffectiveScope => _underlyingParameter.EffectiveScope;

	internal sealed override bool HasUnscopedRefAttribute => _underlyingParameter.HasUnscopedRefAttribute;

	internal sealed override bool UseUpdatedEscapeRules => _underlyingParameter.UseUpdatedEscapeRules;

	protected WrappedParameterSymbol(ParameterSymbol underlyingParameter)
	{
		_underlyingParameter = underlyingParameter;
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return _underlyingParameter.GetAttributes();
	}

	internal abstract override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes);

	public override string GetDocumentationCommentXml(CultureInfo? preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return _underlyingParameter.GetDocumentationCommentXml(preferredCulture, expandIncludes, cancellationToken);
	}
}
