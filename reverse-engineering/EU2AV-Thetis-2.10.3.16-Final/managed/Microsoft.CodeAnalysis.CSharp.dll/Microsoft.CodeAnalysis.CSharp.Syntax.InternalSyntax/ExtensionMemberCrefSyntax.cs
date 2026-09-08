namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ExtensionMemberCrefSyntax : MemberCrefSyntax
{
	internal readonly SyntaxToken extensionKeyword;

	internal readonly TypeArgumentListSyntax? typeArgumentList;

	internal readonly CrefParameterListSyntax parameters;

	internal readonly SyntaxToken dotToken;

	internal readonly MemberCrefSyntax member;

	public SyntaxToken ExtensionKeyword => extensionKeyword;

	public TypeArgumentListSyntax? TypeArgumentList => typeArgumentList;

	public CrefParameterListSyntax Parameters => parameters;

	public SyntaxToken DotToken => dotToken;

	public MemberCrefSyntax Member => member;

	internal ExtensionMemberCrefSyntax(SyntaxKind kind, SyntaxToken extensionKeyword, TypeArgumentListSyntax? typeArgumentList, CrefParameterListSyntax parameters, SyntaxToken dotToken, MemberCrefSyntax member, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 5;
		AdjustFlagsAndWidth(extensionKeyword);
		this.extensionKeyword = extensionKeyword;
		if (typeArgumentList != null)
		{
			AdjustFlagsAndWidth(typeArgumentList);
			this.typeArgumentList = typeArgumentList;
		}
		AdjustFlagsAndWidth(parameters);
		this.parameters = parameters;
		AdjustFlagsAndWidth(dotToken);
		this.dotToken = dotToken;
		AdjustFlagsAndWidth(member);
		this.member = member;
	}

	internal ExtensionMemberCrefSyntax(SyntaxKind kind, SyntaxToken extensionKeyword, TypeArgumentListSyntax? typeArgumentList, CrefParameterListSyntax parameters, SyntaxToken dotToken, MemberCrefSyntax member, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 5;
		AdjustFlagsAndWidth(extensionKeyword);
		this.extensionKeyword = extensionKeyword;
		if (typeArgumentList != null)
		{
			AdjustFlagsAndWidth(typeArgumentList);
			this.typeArgumentList = typeArgumentList;
		}
		AdjustFlagsAndWidth(parameters);
		this.parameters = parameters;
		AdjustFlagsAndWidth(dotToken);
		this.dotToken = dotToken;
		AdjustFlagsAndWidth(member);
		this.member = member;
	}

	internal ExtensionMemberCrefSyntax(SyntaxKind kind, SyntaxToken extensionKeyword, TypeArgumentListSyntax? typeArgumentList, CrefParameterListSyntax parameters, SyntaxToken dotToken, MemberCrefSyntax member)
		: base(kind)
	{
		base.SlotCount = 5;
		AdjustFlagsAndWidth(extensionKeyword);
		this.extensionKeyword = extensionKeyword;
		if (typeArgumentList != null)
		{
			AdjustFlagsAndWidth(typeArgumentList);
			this.typeArgumentList = typeArgumentList;
		}
		AdjustFlagsAndWidth(parameters);
		this.parameters = parameters;
		AdjustFlagsAndWidth(dotToken);
		this.dotToken = dotToken;
		AdjustFlagsAndWidth(member);
		this.member = member;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => extensionKeyword, 
			1 => typeArgumentList, 
			2 => parameters, 
			3 => dotToken, 
			4 => member, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ExtensionMemberCrefSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitExtensionMemberCref(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitExtensionMemberCref(this);
	}

	public ExtensionMemberCrefSyntax Update(SyntaxToken extensionKeyword, TypeArgumentListSyntax typeArgumentList, CrefParameterListSyntax parameters, SyntaxToken dotToken, MemberCrefSyntax member)
	{
		if (extensionKeyword != ExtensionKeyword || typeArgumentList != TypeArgumentList || parameters != Parameters || dotToken != DotToken || member != Member)
		{
			ExtensionMemberCrefSyntax extensionMemberCrefSyntax = SyntaxFactory.ExtensionMemberCref(extensionKeyword, typeArgumentList, parameters, dotToken, member);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				extensionMemberCrefSyntax = extensionMemberCrefSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				extensionMemberCrefSyntax = extensionMemberCrefSyntax.WithAnnotationsGreen(annotations);
			}
			return extensionMemberCrefSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ExtensionMemberCrefSyntax(base.Kind, extensionKeyword, typeArgumentList, parameters, dotToken, member, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ExtensionMemberCrefSyntax(base.Kind, extensionKeyword, typeArgumentList, parameters, dotToken, member, GetDiagnostics(), annotations);
	}
}
