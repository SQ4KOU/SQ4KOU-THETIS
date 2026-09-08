namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ConversionOperatorMemberCrefSyntax : MemberCrefSyntax
{
	internal readonly SyntaxToken implicitOrExplicitKeyword;

	internal readonly SyntaxToken operatorKeyword;

	internal readonly SyntaxToken? checkedKeyword;

	internal readonly TypeSyntax type;

	internal readonly CrefParameterListSyntax? parameters;

	public SyntaxToken ImplicitOrExplicitKeyword => implicitOrExplicitKeyword;

	public SyntaxToken OperatorKeyword => operatorKeyword;

	public SyntaxToken? CheckedKeyword => checkedKeyword;

	public TypeSyntax Type => type;

	public CrefParameterListSyntax? Parameters => parameters;

	internal ConversionOperatorMemberCrefSyntax(SyntaxKind kind, SyntaxToken implicitOrExplicitKeyword, SyntaxToken operatorKeyword, SyntaxToken? checkedKeyword, TypeSyntax type, CrefParameterListSyntax? parameters, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 5;
		AdjustFlagsAndWidth(implicitOrExplicitKeyword);
		this.implicitOrExplicitKeyword = implicitOrExplicitKeyword;
		AdjustFlagsAndWidth(operatorKeyword);
		this.operatorKeyword = operatorKeyword;
		if (checkedKeyword != null)
		{
			AdjustFlagsAndWidth(checkedKeyword);
			this.checkedKeyword = checkedKeyword;
		}
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
	}

	internal ConversionOperatorMemberCrefSyntax(SyntaxKind kind, SyntaxToken implicitOrExplicitKeyword, SyntaxToken operatorKeyword, SyntaxToken? checkedKeyword, TypeSyntax type, CrefParameterListSyntax? parameters, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 5;
		AdjustFlagsAndWidth(implicitOrExplicitKeyword);
		this.implicitOrExplicitKeyword = implicitOrExplicitKeyword;
		AdjustFlagsAndWidth(operatorKeyword);
		this.operatorKeyword = operatorKeyword;
		if (checkedKeyword != null)
		{
			AdjustFlagsAndWidth(checkedKeyword);
			this.checkedKeyword = checkedKeyword;
		}
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
	}

	internal ConversionOperatorMemberCrefSyntax(SyntaxKind kind, SyntaxToken implicitOrExplicitKeyword, SyntaxToken operatorKeyword, SyntaxToken? checkedKeyword, TypeSyntax type, CrefParameterListSyntax? parameters)
		: base(kind)
	{
		base.SlotCount = 5;
		AdjustFlagsAndWidth(implicitOrExplicitKeyword);
		this.implicitOrExplicitKeyword = implicitOrExplicitKeyword;
		AdjustFlagsAndWidth(operatorKeyword);
		this.operatorKeyword = operatorKeyword;
		if (checkedKeyword != null)
		{
			AdjustFlagsAndWidth(checkedKeyword);
			this.checkedKeyword = checkedKeyword;
		}
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => implicitOrExplicitKeyword, 
			1 => operatorKeyword, 
			2 => checkedKeyword, 
			3 => type, 
			4 => parameters, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ConversionOperatorMemberCrefSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitConversionOperatorMemberCref(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitConversionOperatorMemberCref(this);
	}

	public ConversionOperatorMemberCrefSyntax Update(SyntaxToken implicitOrExplicitKeyword, SyntaxToken operatorKeyword, SyntaxToken checkedKeyword, TypeSyntax type, CrefParameterListSyntax parameters)
	{
		if (implicitOrExplicitKeyword != ImplicitOrExplicitKeyword || operatorKeyword != OperatorKeyword || checkedKeyword != CheckedKeyword || type != Type || parameters != Parameters)
		{
			ConversionOperatorMemberCrefSyntax conversionOperatorMemberCrefSyntax = SyntaxFactory.ConversionOperatorMemberCref(implicitOrExplicitKeyword, operatorKeyword, checkedKeyword, type, parameters);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				conversionOperatorMemberCrefSyntax = conversionOperatorMemberCrefSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				conversionOperatorMemberCrefSyntax = conversionOperatorMemberCrefSyntax.WithAnnotationsGreen(annotations);
			}
			return conversionOperatorMemberCrefSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ConversionOperatorMemberCrefSyntax(base.Kind, implicitOrExplicitKeyword, operatorKeyword, checkedKeyword, type, parameters, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ConversionOperatorMemberCrefSyntax(base.Kind, implicitOrExplicitKeyword, operatorKeyword, checkedKeyword, type, parameters, GetDiagnostics(), annotations);
	}
}
