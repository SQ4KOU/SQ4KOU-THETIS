using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class BaseParameterSyntax : CSharpSyntaxNode
{
	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists { get; }

	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers { get; }

	public abstract TypeSyntax? Type { get; }

	internal BaseParameterSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal BaseParameterSyntax(SyntaxKind kind)
		: base(kind)
	{
	}
}
