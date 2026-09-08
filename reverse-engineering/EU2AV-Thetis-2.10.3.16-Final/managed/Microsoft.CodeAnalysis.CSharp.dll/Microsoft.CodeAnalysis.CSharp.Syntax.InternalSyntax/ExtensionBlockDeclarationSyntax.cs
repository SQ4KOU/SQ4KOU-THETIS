using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ExtensionBlockDeclarationSyntax : TypeDeclarationSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly GreenNode? modifiers;

	internal readonly SyntaxToken keyword;

	internal readonly TypeParameterListSyntax? typeParameterList;

	internal readonly ParameterListSyntax? parameterList;

	internal readonly GreenNode? constraintClauses;

	internal readonly SyntaxToken? openBraceToken;

	internal readonly GreenNode? members;

	internal readonly SyntaxToken? closeBraceToken;

	internal readonly SyntaxToken? semicolonToken;

	public override SyntaxToken Identifier => null;

	public override BaseListSyntax BaseList => null;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(modifiers);

	public override SyntaxToken Keyword => keyword;

	public override TypeParameterListSyntax? TypeParameterList => typeParameterList;

	public override ParameterListSyntax? ParameterList => parameterList;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> ConstraintClauses => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax>(constraintClauses);

	public override SyntaxToken? OpenBraceToken => openBraceToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> Members => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax>(members);

	public override SyntaxToken? CloseBraceToken => closeBraceToken;

	public override SyntaxToken? SemicolonToken => semicolonToken;

	public override TypeDeclarationSyntax UpdateCore(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken keyword, SyntaxToken identifier, TypeParameterListSyntax typeParameterList, ParameterListSyntax parameterList, BaseListSyntax baseList, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		if (identifier != null)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Syntax/InternalSyntax/TypeDeclarationSyntax.cs", 176);
		}
		if (baseList != null)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Syntax/InternalSyntax/TypeDeclarationSyntax.cs", 181);
		}
		return Update(attributeLists, modifiers, keyword, typeParameterList, parameterList, constraintClauses, openBraceToken, members, closeBraceToken, semicolonToken);
	}

	internal ExtensionBlockDeclarationSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, SyntaxToken keyword, TypeParameterListSyntax? typeParameterList, ParameterListSyntax? parameterList, GreenNode? constraintClauses, SyntaxToken? openBraceToken, GreenNode? members, SyntaxToken? closeBraceToken, SyntaxToken? semicolonToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 10;
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

	internal ExtensionBlockDeclarationSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, SyntaxToken keyword, TypeParameterListSyntax? typeParameterList, ParameterListSyntax? parameterList, GreenNode? constraintClauses, SyntaxToken? openBraceToken, GreenNode? members, SyntaxToken? closeBraceToken, SyntaxToken? semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 10;
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

	internal ExtensionBlockDeclarationSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, SyntaxToken keyword, TypeParameterListSyntax? typeParameterList, ParameterListSyntax? parameterList, GreenNode? constraintClauses, SyntaxToken? openBraceToken, GreenNode? members, SyntaxToken? closeBraceToken, SyntaxToken? semicolonToken)
		: base(kind)
	{
		base.SlotCount = 10;
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
			3 => typeParameterList, 
			4 => parameterList, 
			5 => constraintClauses, 
			6 => openBraceToken, 
			7 => members, 
			8 => closeBraceToken, 
			9 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ExtensionBlockDeclarationSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitExtensionBlockDeclaration(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitExtensionBlockDeclaration(this);
	}

	public ExtensionBlockDeclarationSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken keyword, TypeParameterListSyntax typeParameterList, ParameterListSyntax parameterList, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || keyword != Keyword || typeParameterList != TypeParameterList || parameterList != ParameterList || constraintClauses != ConstraintClauses || openBraceToken != OpenBraceToken || members != Members || closeBraceToken != CloseBraceToken || semicolonToken != SemicolonToken)
		{
			ExtensionBlockDeclarationSyntax extensionBlockDeclarationSyntax = SyntaxFactory.ExtensionBlockDeclaration(attributeLists, modifiers, keyword, typeParameterList, parameterList, constraintClauses, openBraceToken, members, closeBraceToken, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				extensionBlockDeclarationSyntax = extensionBlockDeclarationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				extensionBlockDeclarationSyntax = extensionBlockDeclarationSyntax.WithAnnotationsGreen(annotations);
			}
			return extensionBlockDeclarationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ExtensionBlockDeclarationSyntax(base.Kind, attributeLists, modifiers, keyword, typeParameterList, parameterList, constraintClauses, openBraceToken, members, closeBraceToken, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ExtensionBlockDeclarationSyntax(base.Kind, attributeLists, modifiers, keyword, typeParameterList, parameterList, constraintClauses, openBraceToken, members, closeBraceToken, semicolonToken, GetDiagnostics(), annotations);
	}
}
