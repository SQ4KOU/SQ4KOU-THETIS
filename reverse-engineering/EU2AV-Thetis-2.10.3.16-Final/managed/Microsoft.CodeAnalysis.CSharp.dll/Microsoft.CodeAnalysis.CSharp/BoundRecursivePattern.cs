using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundRecursivePattern : BoundObjectPattern
{
	public BoundTypeExpression? DeclaredType { get; }

	public MethodSymbol? DeconstructMethod { get; }

	public ImmutableArray<BoundPositionalSubpattern> Deconstruction { get; }

	public ImmutableArray<BoundPropertySubpattern> Properties { get; }

	public bool IsExplicitNotNullTest { get; }

	public BoundRecursivePattern(SyntaxNode syntax, BoundTypeExpression? declaredType, MethodSymbol? deconstructMethod, ImmutableArray<BoundPositionalSubpattern> deconstruction, ImmutableArray<BoundPropertySubpattern> properties, bool isExplicitNotNullTest, Symbol? variable, BoundExpression? variableAccess, TypeSymbol inputType, TypeSymbol narrowedType, bool hasErrors = false)
		: base(BoundKind.RecursivePattern, syntax, variable, variableAccess, inputType, narrowedType, hasErrors || declaredType.HasErrors() || deconstruction.HasErrors() || properties.HasErrors() || variableAccess.HasErrors())
	{
		DeclaredType = declaredType;
		DeconstructMethod = deconstructMethod;
		Deconstruction = deconstruction;
		Properties = properties;
		IsExplicitNotNullTest = isExplicitNotNullTest;
	}

	[Conditional("DEBUG")]
	private void Validate()
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitRecursivePattern(this);
	}

	public BoundRecursivePattern Update(BoundTypeExpression? declaredType, MethodSymbol? deconstructMethod, ImmutableArray<BoundPositionalSubpattern> deconstruction, ImmutableArray<BoundPropertySubpattern> properties, bool isExplicitNotNullTest, Symbol? variable, BoundExpression? variableAccess, TypeSymbol inputType, TypeSymbol narrowedType)
	{
		if (declaredType != DeclaredType || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(deconstructMethod, DeconstructMethod) || deconstruction != Deconstruction || properties != Properties || isExplicitNotNullTest != IsExplicitNotNullTest || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(variable, base.Variable) || variableAccess != base.VariableAccess || !TypeSymbol.Equals(inputType, base.InputType, TypeCompareKind.ConsiderEverything) || !TypeSymbol.Equals(narrowedType, base.NarrowedType, TypeCompareKind.ConsiderEverything))
		{
			BoundRecursivePattern boundRecursivePattern = new BoundRecursivePattern(Syntax, declaredType, deconstructMethod, deconstruction, properties, isExplicitNotNullTest, variable, variableAccess, inputType, narrowedType, base.HasErrors);
			boundRecursivePattern.CopyAttributes(this);
			return boundRecursivePattern;
		}
		return this;
	}
}
