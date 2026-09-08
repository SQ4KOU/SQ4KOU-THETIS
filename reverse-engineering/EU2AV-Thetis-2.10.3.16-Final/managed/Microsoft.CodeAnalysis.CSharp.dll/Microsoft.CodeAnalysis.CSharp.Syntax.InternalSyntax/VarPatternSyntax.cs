namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class VarPatternSyntax : PatternSyntax
{
	internal readonly SyntaxToken varKeyword;

	internal readonly VariableDesignationSyntax designation;

	public SyntaxToken VarKeyword => varKeyword;

	public VariableDesignationSyntax Designation => designation;

	internal VarPatternSyntax(SyntaxKind kind, SyntaxToken varKeyword, VariableDesignationSyntax designation, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(varKeyword);
		this.varKeyword = varKeyword;
		AdjustFlagsAndWidth(designation);
		this.designation = designation;
	}

	internal VarPatternSyntax(SyntaxKind kind, SyntaxToken varKeyword, VariableDesignationSyntax designation, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(varKeyword);
		this.varKeyword = varKeyword;
		AdjustFlagsAndWidth(designation);
		this.designation = designation;
	}

	internal VarPatternSyntax(SyntaxKind kind, SyntaxToken varKeyword, VariableDesignationSyntax designation)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(varKeyword);
		this.varKeyword = varKeyword;
		AdjustFlagsAndWidth(designation);
		this.designation = designation;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => varKeyword, 
			1 => designation, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.VarPatternSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitVarPattern(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitVarPattern(this);
	}

	public VarPatternSyntax Update(SyntaxToken varKeyword, VariableDesignationSyntax designation)
	{
		if (varKeyword != VarKeyword || designation != Designation)
		{
			VarPatternSyntax varPatternSyntax = SyntaxFactory.VarPattern(varKeyword, designation);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				varPatternSyntax = varPatternSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				varPatternSyntax = varPatternSyntax.WithAnnotationsGreen(annotations);
			}
			return varPatternSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new VarPatternSyntax(base.Kind, varKeyword, designation, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new VarPatternSyntax(base.Kind, varKeyword, designation, GetDiagnostics(), annotations);
	}
}
