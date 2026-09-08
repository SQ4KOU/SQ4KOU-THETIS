namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class LineDirectivePositionSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken openParenToken;

	internal readonly SyntaxToken line;

	internal readonly SyntaxToken commaToken;

	internal readonly SyntaxToken character;

	internal readonly SyntaxToken closeParenToken;

	public SyntaxToken OpenParenToken => openParenToken;

	public SyntaxToken Line => line;

	public SyntaxToken CommaToken => commaToken;

	public SyntaxToken Character => character;

	public SyntaxToken CloseParenToken => closeParenToken;

	internal LineDirectivePositionSyntax(SyntaxKind kind, SyntaxToken openParenToken, SyntaxToken line, SyntaxToken commaToken, SyntaxToken character, SyntaxToken closeParenToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 5;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(line);
		this.line = line;
		AdjustFlagsAndWidth(commaToken);
		this.commaToken = commaToken;
		AdjustFlagsAndWidth(character);
		this.character = character;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal LineDirectivePositionSyntax(SyntaxKind kind, SyntaxToken openParenToken, SyntaxToken line, SyntaxToken commaToken, SyntaxToken character, SyntaxToken closeParenToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 5;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(line);
		this.line = line;
		AdjustFlagsAndWidth(commaToken);
		this.commaToken = commaToken;
		AdjustFlagsAndWidth(character);
		this.character = character;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal LineDirectivePositionSyntax(SyntaxKind kind, SyntaxToken openParenToken, SyntaxToken line, SyntaxToken commaToken, SyntaxToken character, SyntaxToken closeParenToken)
		: base(kind)
	{
		base.SlotCount = 5;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(line);
		this.line = line;
		AdjustFlagsAndWidth(commaToken);
		this.commaToken = commaToken;
		AdjustFlagsAndWidth(character);
		this.character = character;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => openParenToken, 
			1 => line, 
			2 => commaToken, 
			3 => character, 
			4 => closeParenToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.LineDirectivePositionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLineDirectivePosition(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLineDirectivePosition(this);
	}

	public LineDirectivePositionSyntax Update(SyntaxToken openParenToken, SyntaxToken line, SyntaxToken commaToken, SyntaxToken character, SyntaxToken closeParenToken)
	{
		if (openParenToken != OpenParenToken || line != Line || commaToken != CommaToken || character != Character || closeParenToken != CloseParenToken)
		{
			LineDirectivePositionSyntax lineDirectivePositionSyntax = SyntaxFactory.LineDirectivePosition(openParenToken, line, commaToken, character, closeParenToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				lineDirectivePositionSyntax = lineDirectivePositionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				lineDirectivePositionSyntax = lineDirectivePositionSyntax.WithAnnotationsGreen(annotations);
			}
			return lineDirectivePositionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new LineDirectivePositionSyntax(base.Kind, openParenToken, line, commaToken, character, closeParenToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new LineDirectivePositionSyntax(base.Kind, openParenToken, line, commaToken, character, closeParenToken, GetDiagnostics(), annotations);
	}
}
