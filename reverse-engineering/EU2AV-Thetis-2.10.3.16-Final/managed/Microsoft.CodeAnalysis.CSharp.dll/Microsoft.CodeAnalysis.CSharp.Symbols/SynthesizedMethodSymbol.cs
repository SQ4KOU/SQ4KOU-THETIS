using System.Collections.Immutable;
using System.Threading;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SynthesizedMethodSymbol : MethodSymbol
{
	private ParameterSymbol _lazyThisParameter;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public sealed override bool IsImplicitlyDeclared => true;

	public sealed override bool AreLocalsZeroed => ContainingType.AreLocalsZeroed;

	public abstract override bool IsStatic { get; }

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	internal sealed override bool HasSpecialNameAttribute
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedMethodSymbol.cs", 81);
		}
	}

	internal override bool IsDeclaredReadOnly => false;

	internal override bool IsInitOnly => false;

	public sealed override FlowAnalysisAnnotations FlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

	internal sealed override bool HasUnscopedRefAttribute => false;

	internal sealed override bool UseUpdatedEscapeRules => ContainingModule.UseUpdatedEscapeRules;

	internal override bool TryGetThisParameter(out ParameterSymbol? thisParameter)
	{
		if (IsStatic)
		{
			thisParameter = null;
			return true;
		}
		if ((object)_lazyThisParameter == null)
		{
			Interlocked.CompareExchange(ref _lazyThisParameter, new ThisParameterSymbol(this), null);
		}
		thisParameter = _lazyThisParameter;
		return true;
	}

	internal sealed override UnmanagedCallersOnlyAttributeData GetUnmanagedCallersOnlyAttributeData(bool forceComplete)
	{
		return null;
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedMethodSymbol.cs", 85);
	}

	internal override bool IsNullableAnalysisEnabled()
	{
		return false;
	}

	internal sealed override bool HasAsyncMethodBuilderAttribute(out TypeSymbol builderArgument)
	{
		builderArgument = null;
		return false;
	}

	internal sealed override int TryGetOverloadResolutionPriority()
	{
		return 0;
	}
}
