using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class TypeParameterBuilder
{
	private readonly SyntaxReference _syntaxRef;

	private readonly SourceNamedTypeSymbol _owner;

	private readonly Location _location;

	internal TypeParameterBuilder(SyntaxReference syntaxRef, SourceNamedTypeSymbol owner, Location location)
	{
		_syntaxRef = syntaxRef;
		_owner = owner;
		_location = location;
	}

	internal TypeParameterSymbol MakeSymbol(int ordinal, IList<TypeParameterBuilder> builders, BindingDiagnosticBag diagnostics)
	{
		TypeParameterSyntax typeParameterSyntax = (TypeParameterSyntax)_syntaxRef.GetSyntax();
		SourceTypeTypeParameterSymbol sourceTypeTypeParameterSymbol = new SourceTypeTypeParameterSymbol(_owner, typeParameterSyntax.Identifier.ValueText, ordinal, typeParameterSyntax.VarianceKeyword.VarianceKindFromToken(), ToLocations(builders), ToSyntaxRefs(builders));
		if (sourceTypeTypeParameterSymbol.Name == sourceTypeTypeParameterSymbol.ContainingSymbol.Name)
		{
			diagnostics.Add(ErrorCode.ERR_TypeVariableSameAsParent, sourceTypeTypeParameterSymbol.GetFirstLocation(), sourceTypeTypeParameterSymbol.Name);
		}
		return sourceTypeTypeParameterSymbol;
	}

	private static ImmutableArray<Location> ToLocations(IList<TypeParameterBuilder> builders)
	{
		ArrayBuilder<Location> instance = ArrayBuilder<Location>.GetInstance(builders.Count);
		foreach (TypeParameterBuilder builder in builders)
		{
			instance.Add(builder._location);
		}
		return instance.ToImmutableAndFree();
	}

	private static ImmutableArray<SyntaxReference> ToSyntaxRefs(IList<TypeParameterBuilder> builders)
	{
		ArrayBuilder<SyntaxReference> instance = ArrayBuilder<SyntaxReference>.GetInstance(builders.Count);
		foreach (TypeParameterBuilder builder in builders)
		{
			instance.Add(builder._syntaxRef);
		}
		return instance.ToImmutableAndFree();
	}
}
