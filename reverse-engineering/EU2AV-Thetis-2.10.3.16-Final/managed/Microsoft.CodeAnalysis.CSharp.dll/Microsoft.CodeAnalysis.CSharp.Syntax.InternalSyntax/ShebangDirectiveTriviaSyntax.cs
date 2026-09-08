using System;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ShebangDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	internal readonly SyntaxToken hashToken;

	internal readonly SyntaxToken exclamationToken;

	internal readonly SyntaxToken endOfDirectiveToken;

	internal readonly bool isActive;

	public override SyntaxToken HashToken => hashToken;

	public SyntaxToken ExclamationToken => exclamationToken;

	public override SyntaxToken EndOfDirectiveToken => endOfDirectiveToken;

	public override bool IsActive => isActive;

	public ShebangDirectiveTriviaSyntax WithContent(SyntaxToken content)
	{
		SyntaxToken syntaxToken = EndOfDirectiveToken;
		if (content.Kind == SyntaxKind.StringLiteralToken)
		{
			syntaxToken = syntaxToken.TokenWithLeadingTrivia(SyntaxFactory.PreprocessingMessage(content.ToString()));
		}
		else if (content.Kind != SyntaxKind.None)
		{
			throw new ArgumentException("content");
		}
		return Update(HashToken, ExclamationToken, syntaxToken, IsActive);
	}

	internal ShebangDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken exclamationToken, SyntaxToken endOfDirectiveToken, bool isActive, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(exclamationToken);
		this.exclamationToken = exclamationToken;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal ShebangDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken exclamationToken, SyntaxToken endOfDirectiveToken, bool isActive, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(exclamationToken);
		this.exclamationToken = exclamationToken;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal ShebangDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken exclamationToken, SyntaxToken endOfDirectiveToken, bool isActive)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(exclamationToken);
		this.exclamationToken = exclamationToken;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => hashToken, 
			1 => exclamationToken, 
			2 => endOfDirectiveToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ShebangDirectiveTriviaSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitShebangDirectiveTrivia(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitShebangDirectiveTrivia(this);
	}

	public ShebangDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken exclamationToken, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || exclamationToken != ExclamationToken || endOfDirectiveToken != EndOfDirectiveToken)
		{
			ShebangDirectiveTriviaSyntax shebangDirectiveTriviaSyntax = SyntaxFactory.ShebangDirectiveTrivia(hashToken, exclamationToken, endOfDirectiveToken, isActive);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				shebangDirectiveTriviaSyntax = shebangDirectiveTriviaSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				shebangDirectiveTriviaSyntax = shebangDirectiveTriviaSyntax.WithAnnotationsGreen(annotations);
			}
			return shebangDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ShebangDirectiveTriviaSyntax(base.Kind, hashToken, exclamationToken, endOfDirectiveToken, isActive, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ShebangDirectiveTriviaSyntax(base.Kind, hashToken, exclamationToken, endOfDirectiveToken, isActive, GetDiagnostics(), annotations);
	}
}
