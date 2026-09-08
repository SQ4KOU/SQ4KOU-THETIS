using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundITuplePattern : BoundPattern
{
	public MethodSymbol GetLengthMethod { get; }

	public MethodSymbol GetItemMethod { get; }

	public ImmutableArray<BoundPositionalSubpattern> Subpatterns { get; }

	public BoundITuplePattern(SyntaxNode syntax, MethodSymbol getLengthMethod, MethodSymbol getItemMethod, ImmutableArray<BoundPositionalSubpattern> subpatterns, TypeSymbol inputType, TypeSymbol narrowedType, bool hasErrors = false)
		: base(BoundKind.ITuplePattern, syntax, inputType, narrowedType, hasErrors || subpatterns.HasErrors())
	{
		GetLengthMethod = getLengthMethod;
		GetItemMethod = getItemMethod;
		Subpatterns = subpatterns;
	}

	[Conditional("DEBUG")]
	private void Validate()
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitITuplePattern(this);
	}

	public BoundITuplePattern Update(MethodSymbol getLengthMethod, MethodSymbol getItemMethod, ImmutableArray<BoundPositionalSubpattern> subpatterns, TypeSymbol inputType, TypeSymbol narrowedType)
	{
		if (!Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(getLengthMethod, GetLengthMethod) || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(getItemMethod, GetItemMethod) || subpatterns != Subpatterns || !TypeSymbol.Equals(inputType, base.InputType, TypeCompareKind.ConsiderEverything) || !TypeSymbol.Equals(narrowedType, base.NarrowedType, TypeCompareKind.ConsiderEverything))
		{
			BoundITuplePattern boundITuplePattern = new BoundITuplePattern(Syntax, getLengthMethod, getItemMethod, subpatterns, inputType, narrowedType, base.HasErrors);
			boundITuplePattern.CopyAttributes(this);
			return boundITuplePattern;
		}
		return this;
	}
}
