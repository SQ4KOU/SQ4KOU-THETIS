using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class OutDeconstructVarPendingInference : BoundExpression
{
	public BoundDeconstructValuePlaceholder? Placeholder;

	public override object Display => string.Empty;

	public new TypeSymbol? Type => base.Type;

	public Symbol? VariableSymbol { get; }

	public bool IsDiscardExpression { get; }

	public BoundDeconstructValuePlaceholder SetInferredTypeWithAnnotations(TypeWithAnnotations type, bool success)
	{
		Placeholder = new BoundDeconstructValuePlaceholder(Syntax, VariableSymbol, IsDiscardExpression, type.Type, base.HasErrors || !success);
		return Placeholder;
	}

	public BoundDeconstructValuePlaceholder FailInference(Binder binder)
	{
		return SetInferredTypeWithAnnotations(TypeWithAnnotations.Create(binder.CreateErrorType()), success: false);
	}

	public OutDeconstructVarPendingInference(SyntaxNode syntax, Symbol? variableSymbol, bool isDiscardExpression, bool hasErrors)
		: base(BoundKind.OutDeconstructVarPendingInference, syntax, null, hasErrors)
	{
		VariableSymbol = variableSymbol;
		IsDiscardExpression = isDiscardExpression;
	}

	public OutDeconstructVarPendingInference(SyntaxNode syntax, Symbol? variableSymbol, bool isDiscardExpression)
		: base(BoundKind.OutDeconstructVarPendingInference, syntax, null)
	{
		VariableSymbol = variableSymbol;
		IsDiscardExpression = isDiscardExpression;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitOutDeconstructVarPendingInference(this);
	}

	public OutDeconstructVarPendingInference Update(Symbol? variableSymbol, bool isDiscardExpression)
	{
		if (!Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(variableSymbol, VariableSymbol) || isDiscardExpression != IsDiscardExpression)
		{
			OutDeconstructVarPendingInference outDeconstructVarPendingInference = new OutDeconstructVarPendingInference(Syntax, variableSymbol, isDiscardExpression, base.HasErrors);
			outDeconstructVarPendingInference.CopyAttributes(this);
			return outDeconstructVarPendingInference;
		}
		return this;
	}
}
