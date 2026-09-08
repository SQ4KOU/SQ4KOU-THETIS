using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class DocumentationCommentTriviaSyntax : StructuredTriviaSyntax
{
	internal readonly GreenNode? content;

	internal readonly SyntaxToken endOfComment;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlNodeSyntax> Content => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlNodeSyntax>(content);

	public SyntaxToken EndOfComment => endOfComment;

	internal DocumentationCommentTriviaSyntax(SyntaxKind kind, GreenNode? content, SyntaxToken endOfComment, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		if (content != null)
		{
			AdjustFlagsAndWidth(content);
			this.content = content;
		}
		AdjustFlagsAndWidth(endOfComment);
		this.endOfComment = endOfComment;
	}

	internal DocumentationCommentTriviaSyntax(SyntaxKind kind, GreenNode? content, SyntaxToken endOfComment, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		if (content != null)
		{
			AdjustFlagsAndWidth(content);
			this.content = content;
		}
		AdjustFlagsAndWidth(endOfComment);
		this.endOfComment = endOfComment;
	}

	internal DocumentationCommentTriviaSyntax(SyntaxKind kind, GreenNode? content, SyntaxToken endOfComment)
		: base(kind)
	{
		base.SlotCount = 2;
		if (content != null)
		{
			AdjustFlagsAndWidth(content);
			this.content = content;
		}
		AdjustFlagsAndWidth(endOfComment);
		this.endOfComment = endOfComment;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => content, 
			1 => endOfComment, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.DocumentationCommentTriviaSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitDocumentationCommentTrivia(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDocumentationCommentTrivia(this);
	}

	public DocumentationCommentTriviaSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlNodeSyntax> content, SyntaxToken endOfComment)
	{
		if (content != Content || endOfComment != EndOfComment)
		{
			DocumentationCommentTriviaSyntax documentationCommentTriviaSyntax = SyntaxFactory.DocumentationCommentTrivia(base.Kind, content, endOfComment);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				documentationCommentTriviaSyntax = documentationCommentTriviaSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				documentationCommentTriviaSyntax = documentationCommentTriviaSyntax.WithAnnotationsGreen(annotations);
			}
			return documentationCommentTriviaSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new DocumentationCommentTriviaSyntax(base.Kind, content, endOfComment, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new DocumentationCommentTriviaSyntax(base.Kind, content, endOfComment, GetDiagnostics(), annotations);
	}
}
