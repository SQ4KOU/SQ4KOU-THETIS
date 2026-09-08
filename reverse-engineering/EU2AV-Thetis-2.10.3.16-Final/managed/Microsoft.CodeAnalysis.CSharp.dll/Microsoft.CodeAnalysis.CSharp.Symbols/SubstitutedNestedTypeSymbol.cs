using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SubstitutedNestedTypeSymbol : SubstitutedNamedTypeSymbol
{
	internal override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotationsNoUseSiteDiagnostics => GetTypeParametersAsTypeArguments();

	public override NamedTypeSymbol ConstructedFrom => this;

	public sealed override bool AreLocalsZeroed
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/ConstructedNamedTypeSymbol.cs", 45);
		}
	}

	internal SubstitutedNestedTypeSymbol(SubstitutedNamedTypeSymbol newContainer, NamedTypeSymbol originalDefinition)
		: base(newContainer, newContainer.TypeSubstitution, originalDefinition, null, newContainer.IsUnboundGenericType && originalDefinition.Arity == 0)
	{
	}

	internal override bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, Symbol owner, ref HashSet<TypeSymbol> checkedTypes)
	{
		return OriginalDefinition.GetUnificationUseSiteDiagnosticRecursive(ref result, owner, ref checkedTypes);
	}

	protected override NamedTypeSymbol WithTupleDataCore(TupleExtraData newData)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/ConstructedNamedTypeSymbol.cs", 50);
	}
}
