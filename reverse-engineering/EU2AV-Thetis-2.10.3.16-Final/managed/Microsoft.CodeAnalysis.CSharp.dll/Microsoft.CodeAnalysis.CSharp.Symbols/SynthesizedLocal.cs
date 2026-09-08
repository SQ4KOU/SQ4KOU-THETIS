using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
internal sealed class SynthesizedLocal : LocalSymbol
{
	private readonly MethodSymbol _containingMethodOpt;

	private readonly TypeWithAnnotations _type;

	private readonly SynthesizedLocalKind _kind;

	private readonly SyntaxNode _syntaxOpt;

	private readonly bool _isPinned;

	private bool _isKnownToReferToTempIfReferenceType;

	private readonly RefKind _refKind;

	public SyntaxNode SyntaxOpt => _syntaxOpt;

	public sealed override RefKind RefKind => _refKind;

	internal sealed override bool IsImportedFromMetadata => false;

	internal sealed override LocalDeclarationKind DeclarationKind => LocalDeclarationKind.None;

	internal sealed override SynthesizedLocalKind SynthesizedKind => _kind;

	internal sealed override SyntaxNode ScopeDesignatorOpt => null;

	internal sealed override SyntaxToken IdentifierToken => default(SyntaxToken);

	public sealed override Symbol ContainingSymbol => _containingMethodOpt;

	public sealed override string Name => null;

	public sealed override TypeWithAnnotations TypeWithAnnotations => _type;

	public sealed override ImmutableArray<Location> Locations
	{
		get
		{
			if (_syntaxOpt != null)
			{
				return ImmutableArray.Create(_syntaxOpt.GetLocation());
			}
			return ImmutableArray<Location>.Empty;
		}
	}

	public sealed override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
	{
		get
		{
			if (_syntaxOpt != null)
			{
				return ImmutableArray.Create(_syntaxOpt.GetReference());
			}
			return ImmutableArray<SyntaxReference>.Empty;
		}
	}

	internal override bool HasSourceLocation => _syntaxOpt != null;

	public sealed override bool IsImplicitlyDeclared => true;

	internal sealed override bool IsPinned => _isPinned;

	internal sealed override bool IsKnownToReferToTempIfReferenceType => _isKnownToReferToTempIfReferenceType;

	internal sealed override bool IsCompilerGenerated => true;

	internal sealed override ScopedKind Scope => ScopedKind.None;

	internal SynthesizedLocal(MethodSymbol containingMethodOpt, TypeWithAnnotations type, SynthesizedLocalKind kind, SyntaxNode syntaxOpt = null, bool isPinned = false, bool isKnownToReferToTempIfReferenceType = false, RefKind refKind = RefKind.None)
	{
		_containingMethodOpt = containingMethodOpt;
		_type = type;
		_kind = kind;
		_syntaxOpt = syntaxOpt;
		_isPinned = isPinned;
		_isKnownToReferToTempIfReferenceType = isKnownToReferToTempIfReferenceType;
		_refKind = refKind;
	}

	internal sealed override LocalSymbol WithSynthesizedLocalKindAndSyntax(SynthesizedLocalKind kind, SyntaxNode syntax)
	{
		return new SynthesizedLocal(_containingMethodOpt, _type, kind, syntax, _isPinned, _isKnownToReferToTempIfReferenceType, _refKind);
	}

	internal sealed override SyntaxNode GetDeclaratorSyntax()
	{
		return _syntaxOpt;
	}

	internal void SetIsKnownToReferToTempIfReferenceType()
	{
		_isKnownToReferToTempIfReferenceType = true;
	}

	internal sealed override ConstantValue GetConstantValue(SyntaxNode node, LocalSymbol inProgress, BindingDiagnosticBag diagnostics)
	{
		return null;
	}

	internal sealed override ReadOnlyBindingDiagnostic<AssemblySymbol> GetConstantValueDiagnostics(BoundExpression boundInitValue)
	{
		return ReadOnlyBindingDiagnostic<AssemblySymbol>.Empty;
	}

	internal sealed override string GetDebuggerDisplay()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('<');
		stringBuilder.Append(_kind.ToString());
		stringBuilder.Append('>');
		stringBuilder.Append(' ');
		stringBuilder.Append(_type.ToDisplayString(SymbolDisplayFormat.TestFormat));
		return stringBuilder.ToString();
	}
}
