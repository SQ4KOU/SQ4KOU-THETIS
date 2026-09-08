using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class TypeDeclarationSyntax : BaseTypeDeclarationSyntax
{
	public abstract SyntaxToken Keyword { get; }

	public abstract TypeParameterListSyntax? TypeParameterList { get; }

	public abstract ParameterListSyntax? ParameterList { get; }

	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> ConstraintClauses { get; }

	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> Members { get; }

	public abstract TypeDeclarationSyntax UpdateCore(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken keyword, SyntaxToken identifier, TypeParameterListSyntax typeParameterList, ParameterListSyntax parameterList, BaseListSyntax baseList, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken);

	internal TypeDeclarationSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal TypeDeclarationSyntax(SyntaxKind kind)
		: base(kind)
	{
	}
}
