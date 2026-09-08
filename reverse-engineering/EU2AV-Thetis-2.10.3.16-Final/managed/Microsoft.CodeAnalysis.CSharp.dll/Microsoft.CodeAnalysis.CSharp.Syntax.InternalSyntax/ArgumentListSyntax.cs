using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ArgumentListSyntax : BaseArgumentListSyntax
{
	internal readonly SyntaxToken openParenToken;

	internal readonly GreenNode? arguments;

	internal readonly SyntaxToken closeParenToken;

	public SyntaxToken OpenParenToken => openParenToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ArgumentSyntax> Arguments => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ArgumentSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(arguments));

	public SyntaxToken CloseParenToken => closeParenToken;

	internal ArgumentListSyntax(SyntaxKind kind, SyntaxToken openParenToken, GreenNode? arguments, SyntaxToken closeParenToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		if (arguments != null)
		{
			AdjustFlagsAndWidth(arguments);
			this.arguments = arguments;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal ArgumentListSyntax(SyntaxKind kind, SyntaxToken openParenToken, GreenNode? arguments, SyntaxToken closeParenToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		if (arguments != null)
		{
			AdjustFlagsAndWidth(arguments);
			this.arguments = arguments;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal ArgumentListSyntax(SyntaxKind kind, SyntaxToken openParenToken, GreenNode? arguments, SyntaxToken closeParenToken)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		if (arguments != null)
		{
			AdjustFlagsAndWidth(arguments);
			this.arguments = arguments;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => openParenToken, 
			1 => arguments, 
			2 => closeParenToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentListSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitArgumentList(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitArgumentList(this);
	}

	public ArgumentListSyntax Update(SyntaxToken openParenToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ArgumentSyntax> arguments, SyntaxToken closeParenToken)
	{
		if (openParenToken != OpenParenToken || arguments != Arguments || closeParenToken != CloseParenToken)
		{
			ArgumentListSyntax argumentListSyntax = SyntaxFactory.ArgumentList(openParenToken, arguments, closeParenToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				argumentListSyntax = argumentListSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				argumentListSyntax = argumentListSyntax.WithAnnotationsGreen(annotations);
			}
			return argumentListSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ArgumentListSyntax(base.Kind, openParenToken, arguments, closeParenToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ArgumentListSyntax(base.Kind, openParenToken, arguments, closeParenToken, GetDiagnostics(), annotations);
	}
}
