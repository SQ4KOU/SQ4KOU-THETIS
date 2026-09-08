using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDiscardExpression : BoundExpression
{
	public override Symbol ExpressionSymbol => new DiscardSymbol(TypeWithAnnotations.Create(Type, base.TopLevelNullability.Annotation.ToInternalAnnotation()));

	public override object Display => ((object)Type) ?? ((object)"_");

	public new TypeSymbol? Type => base.Type;

	public NullableAnnotation NullableAnnotation { get; }

	public bool IsInferred { get; }

	public BoundExpression SetInferredTypeWithAnnotations(TypeWithAnnotations type)
	{
		return Update(type.NullableAnnotation, isInferred: true, type.Type);
	}

	public BoundDiscardExpression FailInference(Binder binder, BindingDiagnosticBag? diagnosticsOpt)
	{
		if (diagnosticsOpt?.DiagnosticBag != null)
		{
			Binder.Error(diagnosticsOpt, ErrorCode.ERR_DiscardTypeInferenceFailed, Syntax);
		}
		return Update(NullableAnnotation.Oblivious, IsInferred, binder.CreateErrorType("var"));
	}

	public BoundDiscardExpression(SyntaxNode syntax, NullableAnnotation nullableAnnotation, bool isInferred, TypeSymbol? type, bool hasErrors)
		: base(BoundKind.DiscardExpression, syntax, type, hasErrors)
	{
		NullableAnnotation = nullableAnnotation;
		IsInferred = isInferred;
	}

	public BoundDiscardExpression(SyntaxNode syntax, NullableAnnotation nullableAnnotation, bool isInferred, TypeSymbol? type)
		: base(BoundKind.DiscardExpression, syntax, type)
	{
		NullableAnnotation = nullableAnnotation;
		IsInferred = isInferred;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDiscardExpression(this);
	}

	public BoundDiscardExpression Update(NullableAnnotation nullableAnnotation, bool isInferred, TypeSymbol? type)
	{
		if (nullableAnnotation != NullableAnnotation || isInferred != IsInferred || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundDiscardExpression boundDiscardExpression = new BoundDiscardExpression(Syntax, nullableAnnotation, isInferred, type, base.HasErrors);
			boundDiscardExpression.CopyAttributes(this);
			return boundDiscardExpression;
		}
		return this;
	}
}
