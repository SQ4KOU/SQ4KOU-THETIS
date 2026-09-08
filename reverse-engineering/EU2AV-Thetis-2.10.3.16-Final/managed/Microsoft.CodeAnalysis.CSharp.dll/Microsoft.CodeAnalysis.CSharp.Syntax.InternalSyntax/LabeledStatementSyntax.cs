using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class LabeledStatementSyntax : StatementSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly SyntaxToken identifier;

	internal readonly SyntaxToken colonToken;

	internal readonly StatementSyntax statement;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public SyntaxToken Identifier => identifier;

	public SyntaxToken ColonToken => colonToken;

	public StatementSyntax Statement => statement;

	internal LabeledStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken identifier, SyntaxToken colonToken, StatementSyntax statement, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal LabeledStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken identifier, SyntaxToken colonToken, StatementSyntax statement, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal LabeledStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken identifier, SyntaxToken colonToken, StatementSyntax statement)
		: base(kind)
	{
		base.SlotCount = 4;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			1 => identifier, 
			2 => colonToken, 
			3 => statement, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.LabeledStatementSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLabeledStatement(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLabeledStatement(this);
	}

	public LabeledStatementSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken identifier, SyntaxToken colonToken, StatementSyntax statement)
	{
		if (attributeLists != AttributeLists || identifier != Identifier || colonToken != ColonToken || statement != Statement)
		{
			LabeledStatementSyntax labeledStatementSyntax = SyntaxFactory.LabeledStatement(attributeLists, identifier, colonToken, statement);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				labeledStatementSyntax = labeledStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				labeledStatementSyntax = labeledStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return labeledStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new LabeledStatementSyntax(base.Kind, attributeLists, identifier, colonToken, statement, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new LabeledStatementSyntax(base.Kind, attributeLists, identifier, colonToken, statement, GetDiagnostics(), annotations);
	}
}
