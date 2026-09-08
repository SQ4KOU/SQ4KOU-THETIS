using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class RecordDeclarationSyntax : TypeDeclarationSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly GreenNode? modifiers;

	internal readonly SyntaxToken keyword;

	internal readonly SyntaxToken? classOrStructKeyword;

	internal readonly SyntaxToken identifier;

	internal readonly TypeParameterListSyntax? typeParameterList;

	internal readonly ParameterListSyntax? parameterList;

	internal readonly BaseListSyntax? baseList;

	internal readonly GreenNode? constraintClauses;

	internal readonly SyntaxToken? openBraceToken;

	internal readonly GreenNode? members;

	internal readonly SyntaxToken? closeBraceToken;

	internal readonly SyntaxToken? semicolonToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(modifiers);

	public override SyntaxToken Keyword => keyword;

	public SyntaxToken? ClassOrStructKeyword => classOrStructKeyword;

	public override SyntaxToken Identifier => identifier;

	public override TypeParameterListSyntax? TypeParameterList => typeParameterList;

	public override ParameterListSyntax? ParameterList => parameterList;

	public override BaseListSyntax? BaseList => baseList;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> ConstraintClauses => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax>(constraintClauses);

	public override SyntaxToken? OpenBraceToken => openBraceToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> Members => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax>(members);

	public override SyntaxToken? CloseBraceToken => closeBraceToken;

	public override SyntaxToken? SemicolonToken => semicolonToken;

	public override TypeDeclarationSyntax UpdateCore(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken keyword, SyntaxToken identifier, TypeParameterListSyntax typeParameterList, ParameterListSyntax parameterList, BaseListSyntax baseList, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		return Update(attributeLists, modifiers, keyword, ClassOrStructKeyword, identifier, typeParameterList, parameterList, baseList, constraintClauses, openBraceToken, members, closeBraceToken, semicolonToken);
	}

	internal RecordDeclarationSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, SyntaxToken keyword, SyntaxToken? classOrStructKeyword, SyntaxToken identifier, TypeParameterListSyntax? typeParameterList, ParameterListSyntax? parameterList, BaseListSyntax? baseList, GreenNode? constraintClauses, SyntaxToken? openBraceToken, GreenNode? members, SyntaxToken? closeBraceToken, SyntaxToken? semicolonToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 13;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		if (classOrStructKeyword != null)
		{
			AdjustFlagsAndWidth(classOrStructKeyword);
			this.classOrStructKeyword = classOrStructKeyword;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		if (typeParameterList != null)
		{
			AdjustFlagsAndWidth(typeParameterList);
			this.typeParameterList = typeParameterList;
		}
		if (parameterList != null)
		{
			AdjustFlagsAndWidth(parameterList);
			this.parameterList = parameterList;
		}
		if (baseList != null)
		{
			AdjustFlagsAndWidth(baseList);
			this.baseList = baseList;
		}
		if (constraintClauses != null)
		{
			AdjustFlagsAndWidth(constraintClauses);
			this.constraintClauses = constraintClauses;
		}
		if (openBraceToken != null)
		{
			AdjustFlagsAndWidth(openBraceToken);
			this.openBraceToken = openBraceToken;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			this.members = members;
		}
		if (closeBraceToken != null)
		{
			AdjustFlagsAndWidth(closeBraceToken);
			this.closeBraceToken = closeBraceToken;
		}
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			this.semicolonToken = semicolonToken;
		}
	}

	internal RecordDeclarationSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, SyntaxToken keyword, SyntaxToken? classOrStructKeyword, SyntaxToken identifier, TypeParameterListSyntax? typeParameterList, ParameterListSyntax? parameterList, BaseListSyntax? baseList, GreenNode? constraintClauses, SyntaxToken? openBraceToken, GreenNode? members, SyntaxToken? closeBraceToken, SyntaxToken? semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 13;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		if (classOrStructKeyword != null)
		{
			AdjustFlagsAndWidth(classOrStructKeyword);
			this.classOrStructKeyword = classOrStructKeyword;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		if (typeParameterList != null)
		{
			AdjustFlagsAndWidth(typeParameterList);
			this.typeParameterList = typeParameterList;
		}
		if (parameterList != null)
		{
			AdjustFlagsAndWidth(parameterList);
			this.parameterList = parameterList;
		}
		if (baseList != null)
		{
			AdjustFlagsAndWidth(baseList);
			this.baseList = baseList;
		}
		if (constraintClauses != null)
		{
			AdjustFlagsAndWidth(constraintClauses);
			this.constraintClauses = constraintClauses;
		}
		if (openBraceToken != null)
		{
			AdjustFlagsAndWidth(openBraceToken);
			this.openBraceToken = openBraceToken;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			this.members = members;
		}
		if (closeBraceToken != null)
		{
			AdjustFlagsAndWidth(closeBraceToken);
			this.closeBraceToken = closeBraceToken;
		}
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			this.semicolonToken = semicolonToken;
		}
	}

	internal RecordDeclarationSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, SyntaxToken keyword, SyntaxToken? classOrStructKeyword, SyntaxToken identifier, TypeParameterListSyntax? typeParameterList, ParameterListSyntax? parameterList, BaseListSyntax? baseList, GreenNode? constraintClauses, SyntaxToken? openBraceToken, GreenNode? members, SyntaxToken? closeBraceToken, SyntaxToken? semicolonToken)
		: base(kind)
	{
		base.SlotCount = 13;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		if (classOrStructKeyword != null)
		{
			AdjustFlagsAndWidth(classOrStructKeyword);
			this.classOrStructKeyword = classOrStructKeyword;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		if (typeParameterList != null)
		{
			AdjustFlagsAndWidth(typeParameterList);
			this.typeParameterList = typeParameterList;
		}
		if (parameterList != null)
		{
			AdjustFlagsAndWidth(parameterList);
			this.parameterList = parameterList;
		}
		if (baseList != null)
		{
			AdjustFlagsAndWidth(baseList);
			this.baseList = baseList;
		}
		if (constraintClauses != null)
		{
			AdjustFlagsAndWidth(constraintClauses);
			this.constraintClauses = constraintClauses;
		}
		if (openBraceToken != null)
		{
			AdjustFlagsAndWidth(openBraceToken);
			this.openBraceToken = openBraceToken;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			this.members = members;
		}
		if (closeBraceToken != null)
		{
			AdjustFlagsAndWidth(closeBraceToken);
			this.closeBraceToken = closeBraceToken;
		}
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			this.semicolonToken = semicolonToken;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			1 => modifiers, 
			2 => keyword, 
			3 => classOrStructKeyword, 
			4 => identifier, 
			5 => typeParameterList, 
			6 => parameterList, 
			7 => baseList, 
			8 => constraintClauses, 
			9 => openBraceToken, 
			10 => members, 
			11 => closeBraceToken, 
			12 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.RecordDeclarationSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitRecordDeclaration(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitRecordDeclaration(this);
	}

	public RecordDeclarationSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken keyword, SyntaxToken classOrStructKeyword, SyntaxToken identifier, TypeParameterListSyntax typeParameterList, ParameterListSyntax parameterList, BaseListSyntax baseList, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || keyword != Keyword || classOrStructKeyword != ClassOrStructKeyword || identifier != Identifier || typeParameterList != TypeParameterList || parameterList != ParameterList || baseList != BaseList || constraintClauses != ConstraintClauses || openBraceToken != OpenBraceToken || members != Members || closeBraceToken != CloseBraceToken || semicolonToken != SemicolonToken)
		{
			RecordDeclarationSyntax recordDeclarationSyntax = SyntaxFactory.RecordDeclaration(base.Kind, attributeLists, modifiers, keyword, classOrStructKeyword, identifier, typeParameterList, parameterList, baseList, constraintClauses, openBraceToken, members, closeBraceToken, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				recordDeclarationSyntax = recordDeclarationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				recordDeclarationSyntax = recordDeclarationSyntax.WithAnnotationsGreen(annotations);
			}
			return recordDeclarationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new RecordDeclarationSyntax(base.Kind, attributeLists, modifiers, keyword, classOrStructKeyword, identifier, typeParameterList, parameterList, baseList, constraintClauses, openBraceToken, members, closeBraceToken, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new RecordDeclarationSyntax(base.Kind, attributeLists, modifiers, keyword, classOrStructKeyword, identifier, typeParameterList, parameterList, baseList, constraintClauses, openBraceToken, members, closeBraceToken, semicolonToken, GetDiagnostics(), annotations);
	}
}
