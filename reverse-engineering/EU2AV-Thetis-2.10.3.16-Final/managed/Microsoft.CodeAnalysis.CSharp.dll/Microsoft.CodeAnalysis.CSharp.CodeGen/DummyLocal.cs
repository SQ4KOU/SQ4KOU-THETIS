using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp.CodeGen;

internal sealed class DummyLocal : LocalSymbol
{
	internal override bool IsImportedFromMetadata => false;

	internal override LocalDeclarationKind DeclarationKind => LocalDeclarationKind.None;

	internal override SynthesizedLocalKind SynthesizedKind => SynthesizedLocalKind.OptimizerTemp;

	internal override SyntaxNode ScopeDesignatorOpt => null;

	internal override SyntaxToken IdentifierToken => default(SyntaxToken);

	internal override bool IsPinned => false;

	internal override bool IsKnownToReferToTempIfReferenceType => false;

	public override Symbol ContainingSymbol
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override TypeWithAnnotations TypeWithAnnotations
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override ImmutableArray<Location> Locations
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	internal override bool IsCompilerGenerated => true;

	internal override bool HasSourceLocation => false;

	public override RefKind RefKind => RefKind.None;

	internal override ScopedKind Scope => ScopedKind.None;

	internal override LocalSymbol WithSynthesizedLocalKindAndSyntax(SynthesizedLocalKind kind, SyntaxNode syntax)
	{
		throw new NotImplementedException();
	}

	internal override ConstantValue GetConstantValue(SyntaxNode node, LocalSymbol inProgress, BindingDiagnosticBag diagnostics)
	{
		throw new NotImplementedException();
	}

	internal override ReadOnlyBindingDiagnostic<AssemblySymbol> GetConstantValueDiagnostics(BoundExpression boundInitValue)
	{
		throw new NotImplementedException();
	}

	internal override SyntaxNode GetDeclaratorSyntax()
	{
		throw new NotImplementedException();
	}
}
