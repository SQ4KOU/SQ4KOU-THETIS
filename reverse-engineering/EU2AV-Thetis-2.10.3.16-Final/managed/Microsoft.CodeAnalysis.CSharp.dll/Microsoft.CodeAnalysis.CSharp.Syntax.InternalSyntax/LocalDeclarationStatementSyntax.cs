using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class LocalDeclarationStatementSyntax : StatementSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly SyntaxToken? awaitKeyword;

	internal readonly SyntaxToken? usingKeyword;

	internal readonly GreenNode? modifiers;

	internal readonly VariableDeclarationSyntax declaration;

	internal readonly SyntaxToken semicolonToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public SyntaxToken? AwaitKeyword => awaitKeyword;

	public SyntaxToken? UsingKeyword => usingKeyword;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(modifiers);

	public VariableDeclarationSyntax Declaration => declaration;

	public SyntaxToken SemicolonToken => semicolonToken;

	internal LocalDeclarationStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken? awaitKeyword, SyntaxToken? usingKeyword, GreenNode? modifiers, VariableDeclarationSyntax declaration, SyntaxToken semicolonToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 6;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (awaitKeyword != null)
		{
			AdjustFlagsAndWidth(awaitKeyword);
			this.awaitKeyword = awaitKeyword;
		}
		if (usingKeyword != null)
		{
			AdjustFlagsAndWidth(usingKeyword);
			this.usingKeyword = usingKeyword;
		}
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		AdjustFlagsAndWidth(declaration);
		this.declaration = declaration;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal LocalDeclarationStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken? awaitKeyword, SyntaxToken? usingKeyword, GreenNode? modifiers, VariableDeclarationSyntax declaration, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 6;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (awaitKeyword != null)
		{
			AdjustFlagsAndWidth(awaitKeyword);
			this.awaitKeyword = awaitKeyword;
		}
		if (usingKeyword != null)
		{
			AdjustFlagsAndWidth(usingKeyword);
			this.usingKeyword = usingKeyword;
		}
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		AdjustFlagsAndWidth(declaration);
		this.declaration = declaration;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal LocalDeclarationStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken? awaitKeyword, SyntaxToken? usingKeyword, GreenNode? modifiers, VariableDeclarationSyntax declaration, SyntaxToken semicolonToken)
		: base(kind)
	{
		base.SlotCount = 6;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (awaitKeyword != null)
		{
			AdjustFlagsAndWidth(awaitKeyword);
			this.awaitKeyword = awaitKeyword;
		}
		if (usingKeyword != null)
		{
			AdjustFlagsAndWidth(usingKeyword);
			this.usingKeyword = usingKeyword;
		}
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		AdjustFlagsAndWidth(declaration);
		this.declaration = declaration;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			1 => awaitKeyword, 
			2 => usingKeyword, 
			3 => modifiers, 
			4 => declaration, 
			5 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLocalDeclarationStatement(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLocalDeclarationStatement(this);
	}

	public LocalDeclarationStatementSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken awaitKeyword, SyntaxToken usingKeyword, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, VariableDeclarationSyntax declaration, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || awaitKeyword != AwaitKeyword || usingKeyword != UsingKeyword || modifiers != Modifiers || declaration != Declaration || semicolonToken != SemicolonToken)
		{
			LocalDeclarationStatementSyntax localDeclarationStatementSyntax = SyntaxFactory.LocalDeclarationStatement(attributeLists, awaitKeyword, usingKeyword, modifiers, declaration, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				localDeclarationStatementSyntax = localDeclarationStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				localDeclarationStatementSyntax = localDeclarationStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return localDeclarationStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new LocalDeclarationStatementSyntax(base.Kind, attributeLists, awaitKeyword, usingKeyword, modifiers, declaration, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new LocalDeclarationStatementSyntax(base.Kind, attributeLists, awaitKeyword, usingKeyword, modifiers, declaration, semicolonToken, GetDiagnostics(), annotations);
	}
}
