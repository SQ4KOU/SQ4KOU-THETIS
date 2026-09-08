using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class RewrittenLambdaOrLocalFunctionSymbol : RewrittenMethodSymbol
{
	private readonly RewrittenMethodSymbol _containingMethod;

	public override Symbol? AssociatedSymbol => null;

	public override Symbol ContainingSymbol => _containingMethod;

	public RewrittenLambdaOrLocalFunctionSymbol(MethodSymbol lambdaOrLocalFunctionSymbol, RewrittenMethodSymbol containingMethod)
		: base(lambdaOrLocalFunctionSymbol, containingMethod.TypeMap, lambdaOrLocalFunctionSymbol.TypeParameters)
	{
		_containingMethod = containingMethod;
	}

	internal override bool TryGetThisParameter(out ParameterSymbol? thisParameter)
	{
		thisParameter = null;
		return true;
	}

	internal override int TryGetOverloadResolutionPriority()
	{
		return _originalMethod.TryGetOverloadResolutionPriority();
	}

	protected override ImmutableArray<ParameterSymbol> MakeParameters()
	{
		return ImmutableArray<ParameterSymbol>.CastUp(_originalMethod.Parameters.SelectAsArray((ParameterSymbol p, RewrittenLambdaOrLocalFunctionSymbol @this) => new RewrittenMethodParameterSymbol(@this, p), this));
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Extensions/RewrittenLambdaOrLocalFunctionSymbol.cs", 43);
	}
}
