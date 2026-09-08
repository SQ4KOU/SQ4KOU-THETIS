namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ObjectCreationExpressionSyntax : BaseObjectCreationExpressionSyntax
{
	internal readonly SyntaxToken newKeyword;

	internal readonly TypeSyntax type;

	internal readonly ArgumentListSyntax? argumentList;

	internal readonly InitializerExpressionSyntax? initializer;

	public override SyntaxToken NewKeyword => newKeyword;

	public TypeSyntax Type => type;

	public override ArgumentListSyntax? ArgumentList => argumentList;

	public override InitializerExpressionSyntax? Initializer => initializer;

	internal ObjectCreationExpressionSyntax(SyntaxKind kind, SyntaxToken newKeyword, TypeSyntax type, ArgumentListSyntax? argumentList, InitializerExpressionSyntax? initializer, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (argumentList != null)
		{
			AdjustFlagsAndWidth(argumentList);
			this.argumentList = argumentList;
		}
		if (initializer != null)
		{
			AdjustFlagsAndWidth(initializer);
			this.initializer = initializer;
		}
	}

	internal ObjectCreationExpressionSyntax(SyntaxKind kind, SyntaxToken newKeyword, TypeSyntax type, ArgumentListSyntax? argumentList, InitializerExpressionSyntax? initializer, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (argumentList != null)
		{
			AdjustFlagsAndWidth(argumentList);
			this.argumentList = argumentList;
		}
		if (initializer != null)
		{
			AdjustFlagsAndWidth(initializer);
			this.initializer = initializer;
		}
	}

	internal ObjectCreationExpressionSyntax(SyntaxKind kind, SyntaxToken newKeyword, TypeSyntax type, ArgumentListSyntax? argumentList, InitializerExpressionSyntax? initializer)
		: base(kind)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (argumentList != null)
		{
			AdjustFlagsAndWidth(argumentList);
			this.argumentList = argumentList;
		}
		if (initializer != null)
		{
			AdjustFlagsAndWidth(initializer);
			this.initializer = initializer;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => newKeyword, 
			1 => type, 
			2 => argumentList, 
			3 => initializer, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitObjectCreationExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitObjectCreationExpression(this);
	}

	public ObjectCreationExpressionSyntax Update(SyntaxToken newKeyword, TypeSyntax type, ArgumentListSyntax argumentList, InitializerExpressionSyntax initializer)
	{
		if (newKeyword != NewKeyword || type != Type || argumentList != ArgumentList || initializer != Initializer)
		{
			ObjectCreationExpressionSyntax objectCreationExpressionSyntax = SyntaxFactory.ObjectCreationExpression(newKeyword, type, argumentList, initializer);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				objectCreationExpressionSyntax = objectCreationExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				objectCreationExpressionSyntax = objectCreationExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return objectCreationExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ObjectCreationExpressionSyntax(base.Kind, newKeyword, type, argumentList, initializer, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ObjectCreationExpressionSyntax(base.Kind, newKeyword, type, argumentList, initializer, GetDiagnostics(), annotations);
	}
}
