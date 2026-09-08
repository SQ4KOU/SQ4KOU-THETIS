using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class VariablePendingInference : BoundExpression
{
	protected abstract ErrorCode InferenceFailedError { get; }

	public new TypeSymbol? Type => base.Type;

	public Symbol VariableSymbol { get; }

	public BoundExpression? ReceiverOpt { get; }

	internal BoundExpression SetInferredTypeWithAnnotations(TypeWithAnnotations type, BindingDiagnosticBag? diagnosticsOpt)
	{
		return SetInferredTypeWithAnnotations(type, null, diagnosticsOpt);
	}

	internal BoundExpression SetInferredTypeWithAnnotations(TypeWithAnnotations type, Binder? binderOpt, BindingDiagnosticBag? diagnosticsOpt)
	{
		bool flag = !type.HasType;
		if (flag)
		{
			type = TypeWithAnnotations.Create(binderOpt.Compilation.ImplicitlyTypedVariableInferenceFailedType);
		}
		switch (VariableSymbol.Kind)
		{
		case SymbolKind.Local:
		{
			SourceLocalSymbol sourceLocalSymbol = (SourceLocalSymbol)VariableSymbol;
			if (diagnosticsOpt?.DiagnosticBag != null)
			{
				if (flag)
				{
					ReportInferenceFailure(diagnosticsOpt);
				}
				else
				{
					SyntaxNode syntaxNode = ((Syntax.Kind() == SyntaxKind.DeclarationExpression) ? ((DeclarationExpressionSyntax)Syntax).Type : Syntax);
					Binder.CheckRestrictedTypeInAsyncMethod(sourceLocalSymbol.ContainingSymbol, type.Type, diagnosticsOpt, syntaxNode);
					if (sourceLocalSymbol.Scope == ScopedKind.ScopedValue && !type.Type.IsErrorOrRefLikeOrAllowsRefLikeType())
					{
						diagnosticsOpt.Add(ErrorCode.ERR_ScopedRefAndRefStructOnly, ((syntaxNode is TypeSyntax syntax) ? syntax.SkipScoped(out var _).SkipRef() : syntaxNode).Location);
					}
				}
			}
			sourceLocalSymbol.SetTypeWithAnnotations(type);
			return new BoundLocal(Syntax, sourceLocalSymbol, BoundLocalDeclarationKind.WithInferredType, null, isNullableUnknown: false, type.Type, base.HasErrors | flag).WithWasConverted();
		}
		case SymbolKind.Field:
		{
			GlobalExpressionVariable globalExpressionVariable = (GlobalExpressionVariable)VariableSymbol;
			BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: true, withDependencies: false);
			if (flag)
			{
				ReportInferenceFailure(instance);
			}
			type = globalExpressionVariable.SetTypeWithAnnotations(type, instance);
			instance.Free();
			return new BoundFieldAccess(Syntax, ReceiverOpt, globalExpressionVariable, null, LookupResultKind.Viable, isDeclaration: true, type.Type, base.HasErrors | flag);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(VariableSymbol.Kind);
		}
	}

	internal BoundExpression FailInference(Binder binder, BindingDiagnosticBag? diagnosticsOpt)
	{
		return SetInferredTypeWithAnnotations(default(TypeWithAnnotations), binder, diagnosticsOpt);
	}

	private void ReportInferenceFailure(BindingDiagnosticBag diagnostics)
	{
		SingleVariableDesignationSyntax singleVariableDesignationSyntax = Syntax.Kind() switch
		{
			SyntaxKind.DeclarationExpression => (SingleVariableDesignationSyntax)((DeclarationExpressionSyntax)Syntax).Designation, 
			SyntaxKind.SingleVariableDesignation => (SingleVariableDesignationSyntax)Syntax, 
			_ => throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/BoundTree/VariablePendingInference.cs", 131), 
		};
		Binder.Error(diagnostics, InferenceFailedError, singleVariableDesignationSyntax.Identifier, singleVariableDesignationSyntax.Identifier.ValueText);
	}

	protected VariablePendingInference(BoundKind kind, SyntaxNode syntax, Symbol variableSymbol, BoundExpression? receiverOpt, bool hasErrors = false)
		: base(kind, syntax, null, hasErrors)
	{
		VariableSymbol = variableSymbol;
		ReceiverOpt = receiverOpt;
	}
}
