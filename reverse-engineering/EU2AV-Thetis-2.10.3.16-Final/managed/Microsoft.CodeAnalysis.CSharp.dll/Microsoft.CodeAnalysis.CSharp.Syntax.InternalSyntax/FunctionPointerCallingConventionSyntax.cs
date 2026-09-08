namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class FunctionPointerCallingConventionSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken managedOrUnmanagedKeyword;

	internal readonly FunctionPointerUnmanagedCallingConventionListSyntax? unmanagedCallingConventionList;

	public SyntaxToken ManagedOrUnmanagedKeyword => managedOrUnmanagedKeyword;

	public FunctionPointerUnmanagedCallingConventionListSyntax? UnmanagedCallingConventionList => unmanagedCallingConventionList;

	internal FunctionPointerCallingConventionSyntax(SyntaxKind kind, SyntaxToken managedOrUnmanagedKeyword, FunctionPointerUnmanagedCallingConventionListSyntax? unmanagedCallingConventionList, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(managedOrUnmanagedKeyword);
		this.managedOrUnmanagedKeyword = managedOrUnmanagedKeyword;
		if (unmanagedCallingConventionList != null)
		{
			AdjustFlagsAndWidth(unmanagedCallingConventionList);
			this.unmanagedCallingConventionList = unmanagedCallingConventionList;
		}
	}

	internal FunctionPointerCallingConventionSyntax(SyntaxKind kind, SyntaxToken managedOrUnmanagedKeyword, FunctionPointerUnmanagedCallingConventionListSyntax? unmanagedCallingConventionList, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(managedOrUnmanagedKeyword);
		this.managedOrUnmanagedKeyword = managedOrUnmanagedKeyword;
		if (unmanagedCallingConventionList != null)
		{
			AdjustFlagsAndWidth(unmanagedCallingConventionList);
			this.unmanagedCallingConventionList = unmanagedCallingConventionList;
		}
	}

	internal FunctionPointerCallingConventionSyntax(SyntaxKind kind, SyntaxToken managedOrUnmanagedKeyword, FunctionPointerUnmanagedCallingConventionListSyntax? unmanagedCallingConventionList)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(managedOrUnmanagedKeyword);
		this.managedOrUnmanagedKeyword = managedOrUnmanagedKeyword;
		if (unmanagedCallingConventionList != null)
		{
			AdjustFlagsAndWidth(unmanagedCallingConventionList);
			this.unmanagedCallingConventionList = unmanagedCallingConventionList;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => managedOrUnmanagedKeyword, 
			1 => unmanagedCallingConventionList, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.FunctionPointerCallingConventionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitFunctionPointerCallingConvention(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFunctionPointerCallingConvention(this);
	}

	public FunctionPointerCallingConventionSyntax Update(SyntaxToken managedOrUnmanagedKeyword, FunctionPointerUnmanagedCallingConventionListSyntax unmanagedCallingConventionList)
	{
		if (managedOrUnmanagedKeyword != ManagedOrUnmanagedKeyword || unmanagedCallingConventionList != UnmanagedCallingConventionList)
		{
			FunctionPointerCallingConventionSyntax functionPointerCallingConventionSyntax = SyntaxFactory.FunctionPointerCallingConvention(managedOrUnmanagedKeyword, unmanagedCallingConventionList);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				functionPointerCallingConventionSyntax = functionPointerCallingConventionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				functionPointerCallingConventionSyntax = functionPointerCallingConventionSyntax.WithAnnotationsGreen(annotations);
			}
			return functionPointerCallingConventionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new FunctionPointerCallingConventionSyntax(base.Kind, managedOrUnmanagedKeyword, unmanagedCallingConventionList, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new FunctionPointerCallingConventionSyntax(base.Kind, managedOrUnmanagedKeyword, unmanagedCallingConventionList, GetDiagnostics(), annotations);
	}
}
