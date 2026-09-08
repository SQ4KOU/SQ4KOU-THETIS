using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Retargeting;

internal abstract class RetargetingParameterSymbol : WrappedParameterSymbol
{
	private ImmutableArray<CustomModifier> _lazyRefCustomModifiers;

	private ImmutableArray<CSharpAttributeData> _lazyCustomAttributes;

	private TypeWithAnnotations.Boxed? _lazyTypeWithAnnotations;

	protected abstract RetargetingModuleSymbol RetargetingModule { get; }

	public sealed override TypeWithAnnotations TypeWithAnnotations
	{
		get
		{
			if (_lazyTypeWithAnnotations == null)
			{
				Interlocked.CompareExchange(ref _lazyTypeWithAnnotations, new TypeWithAnnotations.Boxed(RetargetingModule.RetargetingTranslator.Retarget(_underlyingParameter.TypeWithAnnotations, RetargetOptions.RetargetPrimitiveTypesByTypeCode)), null);
			}
			return _lazyTypeWithAnnotations.Value;
		}
	}

	public sealed override ImmutableArray<CustomModifier> RefCustomModifiers => RetargetingModule.RetargetingTranslator.RetargetModifiers(_underlyingParameter.RefCustomModifiers, ref _lazyRefCustomModifiers);

	public abstract override Symbol ContainingSymbol { get; }

	public sealed override AssemblySymbol ContainingAssembly => RetargetingModule.ContainingAssembly;

	internal sealed override ModuleSymbol ContainingModule => RetargetingModule;

	internal sealed override bool HasMetadataConstantValue => _underlyingParameter.HasMetadataConstantValue;

	internal sealed override bool IsMarshalledExplicitly => _underlyingParameter.IsMarshalledExplicitly;

	internal override MarshalPseudoCustomAttributeData MarshallingInformation => RetargetingModule.RetargetingTranslator.Retarget(_underlyingParameter.MarshallingInformation);

	internal override ImmutableArray<byte> MarshallingDescriptor => _underlyingParameter.MarshallingDescriptor;

	internal sealed override CSharpCompilation? DeclaringCompilation => null;

	internal sealed override ImmutableArray<int> InterpolatedStringHandlerArgumentIndexes => _underlyingParameter.InterpolatedStringHandlerArgumentIndexes;

	internal sealed override bool HasInterpolatedStringHandlerArgumentError => _underlyingParameter.HasInterpolatedStringHandlerArgumentError;

	internal sealed override bool HasEnumeratorCancellationAttribute => _underlyingParameter.HasEnumeratorCancellationAttribute;

	internal sealed override bool IsCallerLineNumber => _underlyingParameter.IsCallerLineNumber;

	internal sealed override bool IsCallerFilePath => _underlyingParameter.IsCallerFilePath;

	internal sealed override bool IsCallerMemberName => _underlyingParameter.IsCallerMemberName;

	internal sealed override int CallerArgumentExpressionParameterIndex => _underlyingParameter.CallerArgumentExpressionParameterIndex;

	protected RetargetingParameterSymbol(ParameterSymbol underlyingParameter)
		: base(underlyingParameter)
	{
	}

	public sealed override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return RetargetingModule.RetargetingTranslator.GetRetargetedAttributes(_underlyingParameter.GetAttributes(), ref _lazyCustomAttributes);
	}

	internal sealed override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		return RetargetingModule.RetargetingTranslator.RetargetAttributes(_underlyingParameter.GetCustomAttributesToEmit(moduleBuilder));
	}

	internal sealed override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Retargeting/RetargetingParameterSymbol.cs", 165);
	}
}
