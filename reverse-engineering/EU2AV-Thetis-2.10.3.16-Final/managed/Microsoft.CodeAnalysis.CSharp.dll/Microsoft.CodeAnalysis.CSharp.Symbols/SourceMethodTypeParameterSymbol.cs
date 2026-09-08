using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceMethodTypeParameterSymbol : SourceTypeParameterSymbol
{
	public abstract SourceMethodSymbol Owner { get; }

	public sealed override TypeParameterKind TypeParameterKind => TypeParameterKind.Method;

	public sealed override Symbol ContainingSymbol => Owner;

	public abstract override bool HasConstructorConstraint { get; }

	public abstract override bool HasValueTypeConstraint { get; }

	public abstract override bool AllowsRefLikeType { get; }

	public abstract override bool IsValueTypeFromConstraintTypes { get; }

	public abstract override bool HasReferenceTypeConstraint { get; }

	public abstract override bool IsReferenceTypeFromConstraintTypes { get; }

	public abstract override bool HasNotNullConstraint { get; }

	internal abstract override bool? ReferenceTypeConstraintIsNullable { get; }

	internal abstract override bool? IsNotNullable { get; }

	public abstract override bool HasUnmanagedTypeConstraint { get; }

	protected sealed override ImmutableArray<TypeParameterSymbol> ContainerTypeParameters => Owner.TypeParameters;

	protected SourceMethodTypeParameterSymbol(string name, int ordinal, ImmutableArray<Location> locations, ImmutableArray<SyntaxReference> syntaxRefs)
		: base(name, ordinal, locations, syntaxRefs)
	{
	}

	internal sealed override void AddDeclarationDiagnostics(BindingDiagnosticBag diagnostics)
	{
		Owner.AddDeclarationDiagnostics(diagnostics);
	}

	protected abstract override TypeParameterBounds ResolveBounds(ConsList<TypeParameterSymbol> inProgress, BindingDiagnosticBag diagnostics);
}
