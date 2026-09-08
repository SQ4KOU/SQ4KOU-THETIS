namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class RecursivePatternSyntax : PatternSyntax
{
	internal readonly TypeSyntax? type;

	internal readonly PositionalPatternClauseSyntax? positionalPatternClause;

	internal readonly PropertyPatternClauseSyntax? propertyPatternClause;

	internal readonly VariableDesignationSyntax? designation;

	public TypeSyntax? Type => type;

	public PositionalPatternClauseSyntax? PositionalPatternClause => positionalPatternClause;

	public PropertyPatternClauseSyntax? PropertyPatternClause => propertyPatternClause;

	public VariableDesignationSyntax? Designation => designation;

	internal RecursivePatternSyntax(SyntaxKind kind, TypeSyntax? type, PositionalPatternClauseSyntax? positionalPatternClause, PropertyPatternClauseSyntax? propertyPatternClause, VariableDesignationSyntax? designation, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		if (type != null)
		{
			AdjustFlagsAndWidth(type);
			this.type = type;
		}
		if (positionalPatternClause != null)
		{
			AdjustFlagsAndWidth(positionalPatternClause);
			this.positionalPatternClause = positionalPatternClause;
		}
		if (propertyPatternClause != null)
		{
			AdjustFlagsAndWidth(propertyPatternClause);
			this.propertyPatternClause = propertyPatternClause;
		}
		if (designation != null)
		{
			AdjustFlagsAndWidth(designation);
			this.designation = designation;
		}
	}

	internal RecursivePatternSyntax(SyntaxKind kind, TypeSyntax? type, PositionalPatternClauseSyntax? positionalPatternClause, PropertyPatternClauseSyntax? propertyPatternClause, VariableDesignationSyntax? designation, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		if (type != null)
		{
			AdjustFlagsAndWidth(type);
			this.type = type;
		}
		if (positionalPatternClause != null)
		{
			AdjustFlagsAndWidth(positionalPatternClause);
			this.positionalPatternClause = positionalPatternClause;
		}
		if (propertyPatternClause != null)
		{
			AdjustFlagsAndWidth(propertyPatternClause);
			this.propertyPatternClause = propertyPatternClause;
		}
		if (designation != null)
		{
			AdjustFlagsAndWidth(designation);
			this.designation = designation;
		}
	}

	internal RecursivePatternSyntax(SyntaxKind kind, TypeSyntax? type, PositionalPatternClauseSyntax? positionalPatternClause, PropertyPatternClauseSyntax? propertyPatternClause, VariableDesignationSyntax? designation)
		: base(kind)
	{
		base.SlotCount = 4;
		if (type != null)
		{
			AdjustFlagsAndWidth(type);
			this.type = type;
		}
		if (positionalPatternClause != null)
		{
			AdjustFlagsAndWidth(positionalPatternClause);
			this.positionalPatternClause = positionalPatternClause;
		}
		if (propertyPatternClause != null)
		{
			AdjustFlagsAndWidth(propertyPatternClause);
			this.propertyPatternClause = propertyPatternClause;
		}
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
			0 => type, 
			1 => positionalPatternClause, 
			2 => propertyPatternClause, 
			3 => designation, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.RecursivePatternSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitRecursivePattern(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitRecursivePattern(this);
	}

	public RecursivePatternSyntax Update(TypeSyntax type, PositionalPatternClauseSyntax positionalPatternClause, PropertyPatternClauseSyntax propertyPatternClause, VariableDesignationSyntax designation)
	{
		if (type != Type || positionalPatternClause != PositionalPatternClause || propertyPatternClause != PropertyPatternClause || designation != Designation)
		{
			RecursivePatternSyntax recursivePatternSyntax = SyntaxFactory.RecursivePattern(type, positionalPatternClause, propertyPatternClause, designation);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				recursivePatternSyntax = recursivePatternSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				recursivePatternSyntax = recursivePatternSyntax.WithAnnotationsGreen(annotations);
			}
			return recursivePatternSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new RecursivePatternSyntax(base.Kind, type, positionalPatternClause, propertyPatternClause, designation, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new RecursivePatternSyntax(base.Kind, type, positionalPatternClause, propertyPatternClause, designation, GetDiagnostics(), annotations);
	}
}
