using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDeclarationPattern : BoundObjectPattern
{
	public BoundTypeExpression DeclaredType { get; }

	public bool IsVar { get; }

	public BoundDeclarationPattern(SyntaxNode syntax, BoundTypeExpression declaredType, bool isVar, Symbol? variable, BoundExpression? variableAccess, TypeSymbol inputType, TypeSymbol narrowedType, bool hasErrors = false)
		: base(BoundKind.DeclarationPattern, syntax, variable, variableAccess, inputType, narrowedType, hasErrors || declaredType.HasErrors() || variableAccess.HasErrors())
	{
		DeclaredType = declaredType;
		IsVar = isVar;
	}

	[Conditional("DEBUG")]
	private void Validate()
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDeclarationPattern(this);
	}

	public BoundDeclarationPattern Update(BoundTypeExpression declaredType, bool isVar, Symbol? variable, BoundExpression? variableAccess, TypeSymbol inputType, TypeSymbol narrowedType)
	{
		if (declaredType != DeclaredType || isVar != IsVar || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(variable, base.Variable) || variableAccess != base.VariableAccess || !TypeSymbol.Equals(inputType, base.InputType, TypeCompareKind.ConsiderEverything) || !TypeSymbol.Equals(narrowedType, base.NarrowedType, TypeCompareKind.ConsiderEverything))
		{
			BoundDeclarationPattern boundDeclarationPattern = new BoundDeclarationPattern(Syntax, declaredType, isVar, variable, variableAccess, inputType, narrowedType, base.HasErrors);
			boundDeclarationPattern.CopyAttributes(this);
			return boundDeclarationPattern;
		}
		return this;
	}
}
