using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class TypeSubstitutedLocalSymbol : LocalSymbol
{
	private readonly LocalSymbol _originalVariable;

	private readonly TypeWithAnnotations _type;

	private readonly Symbol _containingSymbol;

	internal override bool IsImportedFromMetadata => _originalVariable.IsImportedFromMetadata;

	internal override LocalDeclarationKind DeclarationKind => _originalVariable.DeclarationKind;

	internal override SynthesizedLocalKind SynthesizedKind => _originalVariable.SynthesizedKind;

	internal override SyntaxNode ScopeDesignatorOpt => _originalVariable.ScopeDesignatorOpt;

	public override string Name => _originalVariable.Name;

	public override Symbol ContainingSymbol => _containingSymbol;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => _originalVariable.DeclaringSyntaxReferences;

	internal override bool HasSourceLocation => _originalVariable.HasSourceLocation;

	public override ImmutableArray<Location> Locations => _originalVariable.Locations;

	public override TypeWithAnnotations TypeWithAnnotations => _type;

	internal override SyntaxToken IdentifierToken => _originalVariable.IdentifierToken;

	internal override bool IsCompilerGenerated => _originalVariable.IsCompilerGenerated;

	internal override bool IsPinned => _originalVariable.IsPinned;

	internal override bool IsKnownToReferToTempIfReferenceType => _originalVariable.IsKnownToReferToTempIfReferenceType;

	public override RefKind RefKind => _originalVariable.RefKind;

	internal override ScopedKind Scope
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	internal LocalSymbol UnderlyingLocalSymbol => _originalVariable;

	public TypeSubstitutedLocalSymbol(LocalSymbol originalVariable, TypeWithAnnotations type, Symbol containingSymbol)
	{
		_originalVariable = ((!(originalVariable is TypeSubstitutedLocalSymbol typeSubstitutedLocalSymbol)) ? originalVariable : typeSubstitutedLocalSymbol._originalVariable);
		_type = type;
		_containingSymbol = containingSymbol;
	}

	internal override SyntaxNode GetDeclaratorSyntax()
	{
		return _originalVariable.GetDeclaratorSyntax();
	}

	internal override ConstantValue GetConstantValue(SyntaxNode node, LocalSymbol inProgress, BindingDiagnosticBag diagnostics)
	{
		return _originalVariable.GetConstantValue(node, inProgress, diagnostics);
	}

	internal override ReadOnlyBindingDiagnostic<AssemblySymbol> GetConstantValueDiagnostics(BoundExpression boundInitValue)
	{
		return _originalVariable.GetConstantValueDiagnostics(boundInitValue);
	}

	internal override LocalSymbol WithSynthesizedLocalKindAndSyntax(SynthesizedLocalKind kind, SyntaxNode syntax)
	{
		return new TypeSubstitutedLocalSymbol(((SynthesizedLocal)_originalVariable).WithSynthesizedLocalKindAndSyntax(kind, syntax), _type, _containingSymbol);
	}
}
