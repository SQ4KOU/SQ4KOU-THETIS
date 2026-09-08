using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class CatchClauseBinder : LocalScopeBinder
{
	private readonly CatchClauseSyntax _syntax;

	internal override SyntaxNode ScopeDesignator => _syntax;

	public CatchClauseBinder(Binder enclosing, CatchClauseSyntax syntax)
		: base(enclosing, (BinderFlags)((uint)(enclosing.Flags | BinderFlags.InCatchBlock) & 0xFFDFFFFFu))
	{
		_syntax = syntax;
	}

	protected override ImmutableArray<LocalSymbol> BuildLocals()
	{
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		CatchDeclarationSyntax declaration = _syntax.Declaration;
		if (declaration != null && declaration.Identifier.Kind() != SyntaxKind.None)
		{
			instance.Add(SourceLocalSymbol.MakeLocal(ContainingMemberOrLambda, this, allowRefKind: false, allowScoped: false, declaration.Type, declaration.Identifier, LocalDeclarationKind.CatchVariable, null));
		}
		if (_syntax.Filter != null)
		{
			ExpressionVariableFinder.FindExpressionVariables(this, instance, _syntax.Filter.FilterExpression);
		}
		return instance.ToImmutableAndFree();
	}

	internal override ImmutableArray<LocalSymbol> GetDeclaredLocalsForScope(SyntaxNode scopeDesignator)
	{
		if (_syntax == scopeDesignator)
		{
			return Locals;
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/CatchClauseBinder.cs", 52);
	}

	internal override ImmutableArray<LocalFunctionSymbol> GetDeclaredLocalFunctionsForScope(CSharpSyntaxNode scopeDesignator)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/CatchClauseBinder.cs", 57);
	}
}
