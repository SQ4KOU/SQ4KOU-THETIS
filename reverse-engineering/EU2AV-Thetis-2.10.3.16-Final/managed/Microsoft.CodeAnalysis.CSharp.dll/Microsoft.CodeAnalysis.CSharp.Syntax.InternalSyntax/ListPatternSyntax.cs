using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ListPatternSyntax : PatternSyntax
{
	internal readonly SyntaxToken openBracketToken;

	internal readonly GreenNode? patterns;

	internal readonly SyntaxToken closeBracketToken;

	internal readonly VariableDesignationSyntax? designation;

	public SyntaxToken OpenBracketToken => openBracketToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<PatternSyntax> Patterns => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<PatternSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(patterns));

	public SyntaxToken CloseBracketToken => closeBracketToken;

	public VariableDesignationSyntax? Designation => designation;

	internal ListPatternSyntax(SyntaxKind kind, SyntaxToken openBracketToken, GreenNode? patterns, SyntaxToken closeBracketToken, VariableDesignationSyntax? designation, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (patterns != null)
		{
			AdjustFlagsAndWidth(patterns);
			this.patterns = patterns;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
		if (designation != null)
		{
			AdjustFlagsAndWidth(designation);
			this.designation = designation;
		}
	}

	internal ListPatternSyntax(SyntaxKind kind, SyntaxToken openBracketToken, GreenNode? patterns, SyntaxToken closeBracketToken, VariableDesignationSyntax? designation, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (patterns != null)
		{
			AdjustFlagsAndWidth(patterns);
			this.patterns = patterns;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
		if (designation != null)
		{
			AdjustFlagsAndWidth(designation);
			this.designation = designation;
		}
	}

	internal ListPatternSyntax(SyntaxKind kind, SyntaxToken openBracketToken, GreenNode? patterns, SyntaxToken closeBracketToken, VariableDesignationSyntax? designation)
		: base(kind)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (patterns != null)
		{
			AdjustFlagsAndWidth(patterns);
			this.patterns = patterns;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
		if (designation != null)
		{
			AdjustFlagsAndWidth(designation);
			this.designation = designation;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => openBracketToken, 
			1 => patterns, 
			2 => closeBracketToken, 
			3 => designation, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ListPatternSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitListPattern(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitListPattern(this);
	}

	public ListPatternSyntax Update(SyntaxToken openBracketToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<PatternSyntax> patterns, SyntaxToken closeBracketToken, VariableDesignationSyntax designation)
	{
		if (openBracketToken != OpenBracketToken || patterns != Patterns || closeBracketToken != CloseBracketToken || designation != Designation)
		{
			ListPatternSyntax listPatternSyntax = SyntaxFactory.ListPattern(openBracketToken, patterns, closeBracketToken, designation);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				listPatternSyntax = listPatternSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				listPatternSyntax = listPatternSyntax.WithAnnotationsGreen(annotations);
			}
			return listPatternSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ListPatternSyntax(base.Kind, openBracketToken, patterns, closeBracketToken, designation, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ListPatternSyntax(base.Kind, openBracketToken, patterns, closeBracketToken, designation, GetDiagnostics(), annotations);
	}
}
