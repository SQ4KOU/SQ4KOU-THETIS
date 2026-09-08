using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class FunctionPointerUnmanagedCallingConventionListSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken openBracketToken;

	internal readonly GreenNode? callingConventions;

	internal readonly SyntaxToken closeBracketToken;

	public SyntaxToken OpenBracketToken => openBracketToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<FunctionPointerUnmanagedCallingConventionSyntax> CallingConventions => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<FunctionPointerUnmanagedCallingConventionSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(callingConventions));

	public SyntaxToken CloseBracketToken => closeBracketToken;

	internal FunctionPointerUnmanagedCallingConventionListSyntax(SyntaxKind kind, SyntaxToken openBracketToken, GreenNode? callingConventions, SyntaxToken closeBracketToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (callingConventions != null)
		{
			AdjustFlagsAndWidth(callingConventions);
			this.callingConventions = callingConventions;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
	}

	internal FunctionPointerUnmanagedCallingConventionListSyntax(SyntaxKind kind, SyntaxToken openBracketToken, GreenNode? callingConventions, SyntaxToken closeBracketToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (callingConventions != null)
		{
			AdjustFlagsAndWidth(callingConventions);
			this.callingConventions = callingConventions;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
	}

	internal FunctionPointerUnmanagedCallingConventionListSyntax(SyntaxKind kind, SyntaxToken openBracketToken, GreenNode? callingConventions, SyntaxToken closeBracketToken)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (callingConventions != null)
		{
			AdjustFlagsAndWidth(callingConventions);
			this.callingConventions = callingConventions;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => openBracketToken, 
			1 => callingConventions, 
			2 => closeBracketToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.FunctionPointerUnmanagedCallingConventionListSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitFunctionPointerUnmanagedCallingConventionList(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFunctionPointerUnmanagedCallingConventionList(this);
	}

	public FunctionPointerUnmanagedCallingConventionListSyntax Update(SyntaxToken openBracketToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<FunctionPointerUnmanagedCallingConventionSyntax> callingConventions, SyntaxToken closeBracketToken)
	{
		if (openBracketToken != OpenBracketToken || callingConventions != CallingConventions || closeBracketToken != CloseBracketToken)
		{
			FunctionPointerUnmanagedCallingConventionListSyntax functionPointerUnmanagedCallingConventionListSyntax = SyntaxFactory.FunctionPointerUnmanagedCallingConventionList(openBracketToken, callingConventions, closeBracketToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				functionPointerUnmanagedCallingConventionListSyntax = functionPointerUnmanagedCallingConventionListSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				functionPointerUnmanagedCallingConventionListSyntax = functionPointerUnmanagedCallingConventionListSyntax.WithAnnotationsGreen(annotations);
			}
			return functionPointerUnmanagedCallingConventionListSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new FunctionPointerUnmanagedCallingConventionListSyntax(base.Kind, openBracketToken, callingConventions, closeBracketToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new FunctionPointerUnmanagedCallingConventionListSyntax(base.Kind, openBracketToken, callingConventions, closeBracketToken, GetDiagnostics(), annotations);
	}
}
