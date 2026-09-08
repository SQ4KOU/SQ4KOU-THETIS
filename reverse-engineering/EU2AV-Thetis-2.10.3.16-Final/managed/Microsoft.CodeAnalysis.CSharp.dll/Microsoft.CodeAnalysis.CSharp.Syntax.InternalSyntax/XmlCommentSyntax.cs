using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class XmlCommentSyntax : XmlNodeSyntax
{
	internal readonly SyntaxToken lessThanExclamationMinusMinusToken;

	internal readonly GreenNode? textTokens;

	internal readonly SyntaxToken minusMinusGreaterThanToken;

	public SyntaxToken LessThanExclamationMinusMinusToken => lessThanExclamationMinusMinusToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> TextTokens => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(textTokens);

	public SyntaxToken MinusMinusGreaterThanToken => minusMinusGreaterThanToken;

	internal XmlCommentSyntax(SyntaxKind kind, SyntaxToken lessThanExclamationMinusMinusToken, GreenNode? textTokens, SyntaxToken minusMinusGreaterThanToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(lessThanExclamationMinusMinusToken);
		this.lessThanExclamationMinusMinusToken = lessThanExclamationMinusMinusToken;
		if (textTokens != null)
		{
			AdjustFlagsAndWidth(textTokens);
			this.textTokens = textTokens;
		}
		AdjustFlagsAndWidth(minusMinusGreaterThanToken);
		this.minusMinusGreaterThanToken = minusMinusGreaterThanToken;
	}

	internal XmlCommentSyntax(SyntaxKind kind, SyntaxToken lessThanExclamationMinusMinusToken, GreenNode? textTokens, SyntaxToken minusMinusGreaterThanToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(lessThanExclamationMinusMinusToken);
		this.lessThanExclamationMinusMinusToken = lessThanExclamationMinusMinusToken;
		if (textTokens != null)
		{
			AdjustFlagsAndWidth(textTokens);
			this.textTokens = textTokens;
		}
		AdjustFlagsAndWidth(minusMinusGreaterThanToken);
		this.minusMinusGreaterThanToken = minusMinusGreaterThanToken;
	}

	internal XmlCommentSyntax(SyntaxKind kind, SyntaxToken lessThanExclamationMinusMinusToken, GreenNode? textTokens, SyntaxToken minusMinusGreaterThanToken)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(lessThanExclamationMinusMinusToken);
		this.lessThanExclamationMinusMinusToken = lessThanExclamationMinusMinusToken;
		if (textTokens != null)
		{
			AdjustFlagsAndWidth(textTokens);
			this.textTokens = textTokens;
		}
		AdjustFlagsAndWidth(minusMinusGreaterThanToken);
		this.minusMinusGreaterThanToken = minusMinusGreaterThanToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => lessThanExclamationMinusMinusToken, 
			1 => textTokens, 
			2 => minusMinusGreaterThanToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.XmlCommentSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlComment(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlComment(this);
	}

	public XmlCommentSyntax Update(SyntaxToken lessThanExclamationMinusMinusToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens, SyntaxToken minusMinusGreaterThanToken)
	{
		if (lessThanExclamationMinusMinusToken != LessThanExclamationMinusMinusToken || textTokens != TextTokens || minusMinusGreaterThanToken != MinusMinusGreaterThanToken)
		{
			XmlCommentSyntax xmlCommentSyntax = SyntaxFactory.XmlComment(lessThanExclamationMinusMinusToken, textTokens, minusMinusGreaterThanToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				xmlCommentSyntax = xmlCommentSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				xmlCommentSyntax = xmlCommentSyntax.WithAnnotationsGreen(annotations);
			}
			return xmlCommentSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new XmlCommentSyntax(base.Kind, lessThanExclamationMinusMinusToken, textTokens, minusMinusGreaterThanToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new XmlCommentSyntax(base.Kind, lessThanExclamationMinusMinusToken, textTokens, minusMinusGreaterThanToken, GetDiagnostics(), annotations);
	}
}
