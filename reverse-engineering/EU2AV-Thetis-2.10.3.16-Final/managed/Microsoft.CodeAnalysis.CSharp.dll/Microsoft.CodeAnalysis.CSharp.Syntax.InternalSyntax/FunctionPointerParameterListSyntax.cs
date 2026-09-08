using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class FunctionPointerParameterListSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken lessThanToken;

	internal readonly GreenNode? parameters;

	internal readonly SyntaxToken greaterThanToken;

	public SyntaxToken LessThanToken => lessThanToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<FunctionPointerParameterSyntax> Parameters => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<FunctionPointerParameterSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(parameters));

	public SyntaxToken GreaterThanToken => greaterThanToken;

	internal FunctionPointerParameterListSyntax(SyntaxKind kind, SyntaxToken lessThanToken, GreenNode? parameters, SyntaxToken greaterThanToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(lessThanToken);
		this.lessThanToken = lessThanToken;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
		AdjustFlagsAndWidth(greaterThanToken);
		this.greaterThanToken = greaterThanToken;
	}

	internal FunctionPointerParameterListSyntax(SyntaxKind kind, SyntaxToken lessThanToken, GreenNode? parameters, SyntaxToken greaterThanToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(lessThanToken);
		this.lessThanToken = lessThanToken;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
		AdjustFlagsAndWidth(greaterThanToken);
		this.greaterThanToken = greaterThanToken;
	}

	internal FunctionPointerParameterListSyntax(SyntaxKind kind, SyntaxToken lessThanToken, GreenNode? parameters, SyntaxToken greaterThanToken)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(lessThanToken);
		this.lessThanToken = lessThanToken;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
		AdjustFlagsAndWidth(greaterThanToken);
		this.greaterThanToken = greaterThanToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => lessThanToken, 
			1 => parameters, 
			2 => greaterThanToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.FunctionPointerParameterListSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitFunctionPointerParameterList(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFunctionPointerParameterList(this);
	}

	public FunctionPointerParameterListSyntax Update(SyntaxToken lessThanToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<FunctionPointerParameterSyntax> parameters, SyntaxToken greaterThanToken)
	{
		if (lessThanToken != LessThanToken || parameters != Parameters || greaterThanToken != GreaterThanToken)
		{
			FunctionPointerParameterListSyntax functionPointerParameterListSyntax = SyntaxFactory.FunctionPointerParameterList(lessThanToken, parameters, greaterThanToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				functionPointerParameterListSyntax = functionPointerParameterListSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				functionPointerParameterListSyntax = functionPointerParameterListSyntax.WithAnnotationsGreen(annotations);
			}
			return functionPointerParameterListSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new FunctionPointerParameterListSyntax(base.Kind, lessThanToken, parameters, greaterThanToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new FunctionPointerParameterListSyntax(base.Kind, lessThanToken, parameters, greaterThanToken, GetDiagnostics(), annotations);
	}
}
