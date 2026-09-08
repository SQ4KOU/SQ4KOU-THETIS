using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class InterpolatedStringExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken stringStartToken;

	internal readonly GreenNode? contents;

	internal readonly SyntaxToken stringEndToken;

	public SyntaxToken StringStartToken => stringStartToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<InterpolatedStringContentSyntax> Contents => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<InterpolatedStringContentSyntax>(contents);

	public SyntaxToken StringEndToken => stringEndToken;

	internal InterpolatedStringExpressionSyntax(SyntaxKind kind, SyntaxToken stringStartToken, GreenNode? contents, SyntaxToken stringEndToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(stringStartToken);
		this.stringStartToken = stringStartToken;
		if (contents != null)
		{
			AdjustFlagsAndWidth(contents);
			this.contents = contents;
		}
		AdjustFlagsAndWidth(stringEndToken);
		this.stringEndToken = stringEndToken;
	}

	internal InterpolatedStringExpressionSyntax(SyntaxKind kind, SyntaxToken stringStartToken, GreenNode? contents, SyntaxToken stringEndToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(stringStartToken);
		this.stringStartToken = stringStartToken;
		if (contents != null)
		{
			AdjustFlagsAndWidth(contents);
			this.contents = contents;
		}
		AdjustFlagsAndWidth(stringEndToken);
		this.stringEndToken = stringEndToken;
	}

	internal InterpolatedStringExpressionSyntax(SyntaxKind kind, SyntaxToken stringStartToken, GreenNode? contents, SyntaxToken stringEndToken)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(stringStartToken);
		this.stringStartToken = stringStartToken;
		if (contents != null)
		{
			AdjustFlagsAndWidth(contents);
			this.contents = contents;
		}
		AdjustFlagsAndWidth(stringEndToken);
		this.stringEndToken = stringEndToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => stringStartToken, 
			1 => contents, 
			2 => stringEndToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.InterpolatedStringExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitInterpolatedStringExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInterpolatedStringExpression(this);
	}

	public InterpolatedStringExpressionSyntax Update(SyntaxToken stringStartToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<InterpolatedStringContentSyntax> contents, SyntaxToken stringEndToken)
	{
		if (stringStartToken != StringStartToken || contents != Contents || stringEndToken != StringEndToken)
		{
			InterpolatedStringExpressionSyntax interpolatedStringExpressionSyntax = SyntaxFactory.InterpolatedStringExpression(stringStartToken, contents, stringEndToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				interpolatedStringExpressionSyntax = interpolatedStringExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				interpolatedStringExpressionSyntax = interpolatedStringExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return interpolatedStringExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new InterpolatedStringExpressionSyntax(base.Kind, stringStartToken, contents, stringEndToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new InterpolatedStringExpressionSyntax(base.Kind, stringStartToken, contents, stringEndToken, GetDiagnostics(), annotations);
	}
}
