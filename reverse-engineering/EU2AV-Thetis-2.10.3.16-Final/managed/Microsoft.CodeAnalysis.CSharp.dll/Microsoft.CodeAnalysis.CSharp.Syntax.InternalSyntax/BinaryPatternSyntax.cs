namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class BinaryPatternSyntax : PatternSyntax
{
	internal readonly PatternSyntax left;

	internal readonly SyntaxToken operatorToken;

	internal readonly PatternSyntax right;

	public PatternSyntax Left => left;

	public SyntaxToken OperatorToken => operatorToken;

	public PatternSyntax Right => right;

	internal BinaryPatternSyntax(SyntaxKind kind, PatternSyntax left, SyntaxToken operatorToken, PatternSyntax right, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(left);
		this.left = left;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(right);
		this.right = right;
	}

	internal BinaryPatternSyntax(SyntaxKind kind, PatternSyntax left, SyntaxToken operatorToken, PatternSyntax right, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(left);
		this.left = left;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(right);
		this.right = right;
	}

	internal BinaryPatternSyntax(SyntaxKind kind, PatternSyntax left, SyntaxToken operatorToken, PatternSyntax right)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(left);
		this.left = left;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(right);
		this.right = right;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => left, 
			1 => operatorToken, 
			2 => right, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.BinaryPatternSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitBinaryPattern(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBinaryPattern(this);
	}

	public BinaryPatternSyntax Update(PatternSyntax left, SyntaxToken operatorToken, PatternSyntax right)
	{
		if (left != Left || operatorToken != OperatorToken || right != Right)
		{
			BinaryPatternSyntax binaryPatternSyntax = SyntaxFactory.BinaryPattern(base.Kind, left, operatorToken, right);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				binaryPatternSyntax = binaryPatternSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				binaryPatternSyntax = binaryPatternSyntax.WithAnnotationsGreen(annotations);
			}
			return binaryPatternSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new BinaryPatternSyntax(base.Kind, left, operatorToken, right, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new BinaryPatternSyntax(base.Kind, left, operatorToken, right, GetDiagnostics(), annotations);
	}
}
