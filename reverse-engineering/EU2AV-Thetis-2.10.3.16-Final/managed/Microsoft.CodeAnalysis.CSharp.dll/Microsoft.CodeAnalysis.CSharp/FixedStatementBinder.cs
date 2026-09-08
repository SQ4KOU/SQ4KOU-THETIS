using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class FixedStatementBinder : LocalScopeBinder
{
	private readonly FixedStatementSyntax _syntax;

	internal override SyntaxNode ScopeDesignator => _syntax;

	public FixedStatementBinder(Binder enclosing, FixedStatementSyntax syntax)
		: base(enclosing)
	{
		_syntax = syntax;
	}

	protected override ImmutableArray<LocalSymbol> BuildLocals()
	{
		if (_syntax.Declaration != null)
		{
			ArrayBuilder<LocalSymbol> arrayBuilder = new ArrayBuilder<LocalSymbol>(_syntax.Declaration.Variables.Count);
			_syntax.Declaration.Type.VisitRankSpecifiers(delegate(ArrayRankSpecifierSyntax rankSpecifier, (FixedStatementBinder binder, ArrayBuilder<LocalSymbol> locals) args)
			{
				foreach (ExpressionSyntax size in rankSpecifier.Sizes)
				{
					if (size.Kind() != SyntaxKind.OmittedArraySizeExpression)
					{
						ExpressionVariableFinder.FindExpressionVariables(args.binder, args.locals, size);
					}
				}
			}, (this, arrayBuilder));
			foreach (VariableDeclaratorSyntax variable in _syntax.Declaration.Variables)
			{
				arrayBuilder.Add(MakeLocal(_syntax.Declaration, variable, LocalDeclarationKind.FixedVariable, allowScoped: false));
				ExpressionVariableFinder.FindExpressionVariables(this, arrayBuilder, variable);
			}
			return arrayBuilder.ToImmutable();
		}
		return ImmutableArray<LocalSymbol>.Empty;
	}

	internal override ImmutableArray<LocalSymbol> GetDeclaredLocalsForScope(SyntaxNode scopeDesignator)
	{
		if (_syntax == scopeDesignator)
		{
			return Locals;
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/FixedStatementBinder.cs", 67);
	}

	internal override ImmutableArray<LocalFunctionSymbol> GetDeclaredLocalFunctionsForScope(CSharpSyntaxNode scopeDesignator)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/FixedStatementBinder.cs", 72);
	}
}
