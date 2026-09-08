using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class DeconstructionVariablePendingInference : VariablePendingInference
{
	public override object Display
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/BoundTree/Formatting.cs", 140);
		}
	}

	protected override ErrorCode InferenceFailedError => ErrorCode.ERR_TypeInferenceFailedForImplicitlyTypedDeconstructionVariable;

	public DeconstructionVariablePendingInference(SyntaxNode syntax, Symbol variableSymbol, BoundExpression? receiverOpt, bool hasErrors = false)
		: base(BoundKind.DeconstructionVariablePendingInference, syntax, variableSymbol, receiverOpt, hasErrors || receiverOpt.HasErrors())
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDeconstructionVariablePendingInference(this);
	}

	public DeconstructionVariablePendingInference Update(Symbol variableSymbol, BoundExpression? receiverOpt)
	{
		if (!Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(variableSymbol, base.VariableSymbol) || receiverOpt != base.ReceiverOpt)
		{
			DeconstructionVariablePendingInference deconstructionVariablePendingInference = new DeconstructionVariablePendingInference(Syntax, variableSymbol, receiverOpt, base.HasErrors);
			deconstructionVariablePendingInference.CopyAttributes(this);
			return deconstructionVariablePendingInference;
		}
		return this;
	}
}
