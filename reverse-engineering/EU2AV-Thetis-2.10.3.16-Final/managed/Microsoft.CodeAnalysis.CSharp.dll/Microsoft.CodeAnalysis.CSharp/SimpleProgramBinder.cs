using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class SimpleProgramBinder : LocalScopeBinder
{
	private readonly SynthesizedSimpleProgramEntryPointSymbol _entryPoint;

	internal override bool IsLocalFunctionsScopeBinder => true;

	internal override bool IsLabelsScopeBinder => true;

	internal override SyntaxNode ScopeDesignator => _entryPoint.SyntaxNode;

	public SimpleProgramBinder(Binder enclosing, SynthesizedSimpleProgramEntryPointSymbol entryPoint)
		: base(enclosing, enclosing.Flags)
	{
		_entryPoint = entryPoint;
	}

	protected override ImmutableArray<LocalSymbol> BuildLocals()
	{
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance(16);
		foreach (MemberDeclarationSyntax member in _entryPoint.CompilationUnit.Members)
		{
			if (member is GlobalStatementSyntax globalStatementSyntax)
			{
				BuildLocals(this, globalStatementSyntax.Statement, instance);
			}
		}
		return instance.ToImmutableAndFree();
	}

	protected override ImmutableArray<LocalFunctionSymbol> BuildLocalFunctions()
	{
		ArrayBuilder<LocalFunctionSymbol> locals = null;
		foreach (MemberDeclarationSyntax member in _entryPoint.CompilationUnit.Members)
		{
			if (member is GlobalStatementSyntax globalStatementSyntax)
			{
				BuildLocalFunctions(globalStatementSyntax.Statement, ref locals);
			}
		}
		return locals?.ToImmutableAndFree() ?? ImmutableArray<LocalFunctionSymbol>.Empty;
	}

	protected override ImmutableArray<LabelSymbol> BuildLabels()
	{
		ArrayBuilder<LabelSymbol> labels = null;
		foreach (MemberDeclarationSyntax member in _entryPoint.CompilationUnit.Members)
		{
			if (member is GlobalStatementSyntax globalStatementSyntax)
			{
				LocalScopeBinder.BuildLabels(_entryPoint, globalStatementSyntax.Statement, ref labels);
			}
		}
		return labels?.ToImmutableAndFree() ?? ImmutableArray<LabelSymbol>.Empty;
	}

	internal override ImmutableArray<LocalSymbol> GetDeclaredLocalsForScope(SyntaxNode scopeDesignator)
	{
		if (ScopeDesignator == scopeDesignator)
		{
			return Locals;
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/SimpleProgramBinder.cs", 94);
	}

	internal override ImmutableArray<LocalFunctionSymbol> GetDeclaredLocalFunctionsForScope(CSharpSyntaxNode scopeDesignator)
	{
		if (ScopeDesignator == scopeDesignator)
		{
			return LocalFunctions;
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/SimpleProgramBinder.cs", 112);
	}
}
