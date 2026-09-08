using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class LockBinder : LockOrUsingBinder
{
	private readonly LockStatementSyntax _syntax;

	protected override ExpressionSyntax TargetExpressionSyntax => _syntax.Expression;

	public LockBinder(Binder enclosing, LockStatementSyntax syntax)
		: base(enclosing)
	{
		_syntax = syntax;
	}

	internal override BoundStatement BindLockStatementParts(BindingDiagnosticBag diagnostics, Binder originalBinder)
	{
		ExpressionSyntax targetExpressionSyntax = TargetExpressionSyntax;
		BoundExpression boundExpression = BindTargetExpression(diagnostics, originalBinder);
		TypeSymbol type = boundExpression.Type;
		bool hasErrors = false;
		if ((object)type == null)
		{
			if (boundExpression.ConstantValueOpt != ConstantValue.Null || base.Compilation.FeatureStrictEnabled)
			{
				Binder.Error(diagnostics, ErrorCode.ERR_LockNeedsReference, targetExpressionSyntax, boundExpression.Display);
				hasErrors = true;
			}
		}
		else if (!type.IsReferenceType && (type.IsValueType || base.Compilation.FeatureStrictEnabled))
		{
			Binder.Error(diagnostics, ErrorCode.ERR_LockNeedsReference, targetExpressionSyntax, type);
			hasErrors = true;
		}
		if ((object)type != null && type.IsWellKnownTypeLock())
		{
			(MethodSymbol, TypeSymbol, MethodSymbol)? tuple = TryFindLockTypeInfo(type, diagnostics, targetExpressionSyntax);
			if (tuple.HasValue)
			{
				(MethodSymbol, TypeSymbol, MethodSymbol) valueOrDefault = tuple.GetValueOrDefault();
				Binder.CheckFeatureAvailability(targetExpressionSyntax, MessageID.IDS_FeatureLockObject, diagnostics);
				if (diagnostics.ReportUseSite(valueOrDefault.Item1, targetExpressionSyntax) || diagnostics.ReportUseSite(valueOrDefault.Item2, targetExpressionSyntax))
				{
					_ = 1;
				}
				else
					diagnostics.ReportUseSite(valueOrDefault.Item3, targetExpressionSyntax);
			}
		}
		BoundStatement body = originalBinder.BindPossibleEmbeddedStatement(_syntax.Statement, diagnostics);
		return new BoundLockStatement(_syntax, boundExpression, body, hasErrors);
	}

	internal static (MethodSymbol EnterScopeMethod, TypeSymbol ScopeType, MethodSymbol ScopeDisposeMethod)? TryFindLockTypeInfo(TypeSymbol lockType, BindingDiagnosticBag diagnostics, SyntaxNode syntax)
	{
		MethodSymbol methodSymbol = TryFindPublicVoidParameterlessMethod(lockType, "EnterScope");
		if ((object)methodSymbol == null || methodSymbol.ReturnsVoid || methodSymbol.RefKind != RefKind.None)
		{
			Binder.Error(diagnostics, ErrorCode.ERR_MissingPredefinedMember, syntax, "System.Threading.Lock", "EnterScope");
			return null;
		}
		TypeSymbol returnType = methodSymbol.ReturnType;
		if (!(returnType is NamedTypeSymbol { Name: "Scope", Arity: 0, IsValueType: not false } namedTypeSymbol) || !returnType.IsRefLikeType || namedTypeSymbol.DeclaredAccessibility != Accessibility.Public || !TypeSymbol.Equals(returnType.ContainingType, lockType, TypeCompareKind.ConsiderEverything))
		{
			Binder.Error(diagnostics, ErrorCode.ERR_MissingPredefinedMember, syntax, "System.Threading.Lock", "EnterScope");
			return null;
		}
		MethodSymbol methodSymbol2 = TryFindPublicVoidParameterlessMethod(returnType, "Dispose");
		if ((object)methodSymbol2 == null || !methodSymbol2.ReturnsVoid)
		{
			Binder.Error(diagnostics, ErrorCode.ERR_MissingPredefinedMember, syntax, "System.Threading.Lock+Scope", "Dispose");
			return null;
		}
		return new(MethodSymbol, TypeSymbol, MethodSymbol)
		{
			Item1 = methodSymbol,
			Item2 = returnType,
			Item3 = methodSymbol2
		};
	}

	private static MethodSymbol? TryFindPublicVoidParameterlessMethod(TypeSymbol type, string name)
	{
		ImmutableArray<Symbol> members = type.GetMembers(name);
		MethodSymbol methodSymbol = null;
		foreach (Symbol item in members)
		{
			if (item is MethodSymbol { ParameterCount: 0, Arity: 0 } methodSymbol2 && !item.IsStatic && item.DeclaredAccessibility == Accessibility.Public && methodSymbol2.MethodKind == MethodKind.Ordinary)
			{
				if ((object)methodSymbol != null)
				{
					return null;
				}
				methodSymbol = methodSymbol2;
			}
		}
		return methodSymbol;
	}
}
