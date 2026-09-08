using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class SyntaxNodeExtensions
{
	public static TNode WithAnnotations<TNode>(this TNode node, params SyntaxAnnotation[] annotations) where TNode : CSharpSyntaxNode
	{
		return (TNode)node.Green.SetAnnotations(annotations).CreateRed();
	}

	public static bool IsAnonymousFunction(this SyntaxNode syntax)
	{
		SyntaxKind syntaxKind = syntax.Kind();
		if (syntaxKind - 8641 <= (SyntaxKind)2)
		{
			return true;
		}
		return false;
	}

	public static bool IsQuery(this SyntaxNode syntax)
	{
		switch (syntax.Kind())
		{
		case SyntaxKind.QueryExpression:
		case SyntaxKind.FromClause:
		case SyntaxKind.LetClause:
		case SyntaxKind.JoinClause:
		case SyntaxKind.JoinIntoClause:
		case SyntaxKind.WhereClause:
		case SyntaxKind.OrderByClause:
		case SyntaxKind.SelectClause:
		case SyntaxKind.GroupClause:
		case SyntaxKind.QueryContinuation:
			return true;
		default:
			return false;
		}
	}

	internal static bool MayBeNameofOperator(this InvocationExpressionSyntax node)
	{
		if (node.Expression.Kind() == SyntaxKind.IdentifierName && ((IdentifierNameSyntax)node.Expression).Identifier.ContextualKind() == SyntaxKind.NameOfKeyword && node.ArgumentList.Arguments.Count == 1)
		{
			ArgumentSyntax argumentSyntax = node.ArgumentList.Arguments[0];
			if (argumentSyntax.NameColon == null && argumentSyntax.RefOrOutKeyword == default(SyntaxToken))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool CanHaveAssociatedLocalBinder(this SyntaxNode syntax)
	{
		switch (syntax.Kind())
		{
		case SyntaxKind.InvocationExpression:
			if (((InvocationExpressionSyntax)syntax).MayBeNameofOperator())
			{
				return true;
			}
			break;
		case SyntaxKind.ArgumentList:
		case SyntaxKind.AnonymousMethodExpression:
		case SyntaxKind.SimpleLambdaExpression:
		case SyntaxKind.ParenthesizedLambdaExpression:
		case SyntaxKind.CheckedExpression:
		case SyntaxKind.UncheckedExpression:
		case SyntaxKind.EqualsValueClause:
		case SyntaxKind.SwitchSection:
		case SyntaxKind.CatchClause:
		case SyntaxKind.CatchFilterClause:
		case SyntaxKind.Attribute:
		case SyntaxKind.ConstructorDeclaration:
		case SyntaxKind.BaseConstructorInitializer:
		case SyntaxKind.ThisConstructorInitializer:
		case SyntaxKind.ArrowExpressionClause:
		case SyntaxKind.SwitchExpression:
		case SyntaxKind.SwitchExpressionArm:
		case SyntaxKind.PrimaryConstructorBaseType:
			return true;
		case SyntaxKind.RecordStructDeclaration:
			return false;
		}
		if (!(syntax is StatementSyntax))
		{
			return (syntax as ExpressionSyntax).IsValidScopeDesignator();
		}
		return true;
	}

	internal static bool IsValidScopeDesignator(this ExpressionSyntax? expression)
	{
		CSharpSyntaxNode cSharpSyntaxNode = expression?.Parent;
		switch (cSharpSyntaxNode?.Kind())
		{
		case SyntaxKind.SimpleLambdaExpression:
		case SyntaxKind.ParenthesizedLambdaExpression:
			return ((LambdaExpressionSyntax)cSharpSyntaxNode).Body == expression;
		case SyntaxKind.SwitchStatement:
			return ((SwitchStatementSyntax)cSharpSyntaxNode).Expression == expression;
		case SyntaxKind.ForStatement:
		{
			ForStatementSyntax forStatementSyntax = (ForStatementSyntax)cSharpSyntaxNode;
			if (forStatementSyntax.Condition != expression)
			{
				return forStatementSyntax.Incrementors.FirstOrDefault() == expression;
			}
			return true;
		}
		case SyntaxKind.ForEachStatement:
		case SyntaxKind.ForEachVariableStatement:
			return ((CommonForEachStatementSyntax)cSharpSyntaxNode).Expression == expression;
		default:
			return false;
		}
	}

	internal static bool IsLegalCSharp73SpanStackAllocPosition(this SyntaxNode node)
	{
		if (node.Parent.IsKind(SyntaxKind.CastExpression))
		{
			node = node.Parent;
		}
		while (node.Parent.IsKind(SyntaxKind.ConditionalExpression))
		{
			node = node.Parent;
		}
		SyntaxNode parent = node.Parent;
		if (parent == null)
		{
			return false;
		}
		switch (parent.Kind())
		{
		case SyntaxKind.EqualsValueClause:
		{
			SyntaxNode parent2 = parent.Parent;
			if (parent2.IsKind(SyntaxKind.VariableDeclarator))
			{
				return parent2.Parent.IsKind(SyntaxKind.VariableDeclaration);
			}
			return false;
		}
		case SyntaxKind.SimpleAssignmentExpression:
			return parent.Parent.IsKind(SyntaxKind.ExpressionStatement);
		default:
			return false;
		}
	}

	internal static CSharpSyntaxNode AnonymousFunctionBody(this SyntaxNode lambda)
	{
		return ((AnonymousFunctionExpressionSyntax)lambda).Body;
	}

	internal static SyntaxToken ExtractAnonymousTypeMemberName(this ExpressionSyntax input)
	{
		while (true)
		{
			switch (input.Kind())
			{
			case SyntaxKind.IdentifierName:
				return ((IdentifierNameSyntax)input).Identifier;
			case SyntaxKind.SimpleMemberAccessExpression:
				input = ((MemberAccessExpressionSyntax)input).Name;
				break;
			case SyntaxKind.ConditionalAccessExpression:
				input = ((ConditionalAccessExpressionSyntax)input).WhenNotNull;
				if (input.Kind() == SyntaxKind.MemberBindingExpression)
				{
					return ((MemberBindingExpressionSyntax)input).Name.Identifier;
				}
				break;
			default:
				return default(SyntaxToken);
			}
		}
	}

	internal static RefKind GetRefKindInLocalOrReturn(this TypeSyntax syntax, BindingDiagnosticBag diagnostics)
	{
		syntax.SkipRefInLocalOrReturn(diagnostics, out var refKind);
		return refKind;
	}

	internal static TypeSyntax SkipRef(this TypeSyntax syntax)
	{
		RefKind refKind;
		return SkipRefWorker(syntax, null, out refKind);
	}

	internal static TypeSyntax SkipRefInField(this TypeSyntax syntax, out RefKind refKind)
	{
		return SkipRefWorker(syntax, null, out refKind);
	}

	internal static TypeSyntax SkipRefInLocalOrReturn(this TypeSyntax syntax, BindingDiagnosticBag? diagnostics, out RefKind refKind)
	{
		return SkipRefWorker(syntax, diagnostics, out refKind);
	}

	private static TypeSyntax SkipRefWorker(TypeSyntax syntax, BindingDiagnosticBag? diagnostics, out RefKind refKind)
	{
		if (syntax.Kind() == SyntaxKind.RefType)
		{
			RefTypeSyntax refTypeSyntax = (RefTypeSyntax)syntax;
			refKind = ((refTypeSyntax.ReadOnlyKeyword.Kind() != SyntaxKind.ReadOnlyKeyword) ? RefKind.Ref : RefKind.In);
			if (diagnostics != null)
			{
				MessageID.IDS_FeatureRefLocalsReturns.CheckFeatureAvailability(diagnostics, refTypeSyntax.RefKeyword);
				if (refTypeSyntax.ReadOnlyKeyword != default(SyntaxToken))
				{
					MessageID.IDS_FeatureReadOnlyReferences.CheckFeatureAvailability(diagnostics, refTypeSyntax.ReadOnlyKeyword);
				}
			}
			return refTypeSyntax.Type;
		}
		refKind = RefKind.None;
		return syntax;
	}

	internal static TypeSyntax SkipScoped(this TypeSyntax syntax, out bool isScoped)
	{
		if (syntax is ScopedTypeSyntax scopedTypeSyntax)
		{
			isScoped = true;
			return scopedTypeSyntax.Type;
		}
		isScoped = false;
		return syntax;
	}

	internal static SyntaxNode ModifyingScopedOrRefTypeOrSelf(this SyntaxNode syntax)
	{
		SyntaxNode parent = syntax.Parent;
		if (parent is RefTypeSyntax refTypeSyntax && refTypeSyntax.Type == syntax)
		{
			syntax = refTypeSyntax;
			parent = parent.Parent;
		}
		if (parent is ScopedTypeSyntax scopedTypeSyntax && scopedTypeSyntax.Type == syntax)
		{
			return scopedTypeSyntax;
		}
		return syntax;
	}

	internal static ExpressionSyntax? CheckAndUnwrapRefExpression(this ExpressionSyntax? syntax, BindingDiagnosticBag diagnostics, out RefKind refKind)
	{
		if (syntax is RefExpressionSyntax refExpressionSyntax)
		{
			ExpressionSyntax expression = refExpressionSyntax.Expression;
			MessageID.IDS_FeatureRefLocalsReturns.CheckFeatureAvailability(diagnostics, refExpressionSyntax.RefKeyword);
			refKind = RefKind.Ref;
			expression.CheckDeconstructionCompatibleArgument(diagnostics);
			return expression;
		}
		refKind = RefKind.None;
		return syntax;
	}

	internal static void CheckDeconstructionCompatibleArgument(this ExpressionSyntax expression, BindingDiagnosticBag diagnostics)
	{
		if (IsDeconstructionCompatibleArgument(expression))
		{
			diagnostics.Add(ErrorCode.ERR_VarInvocationLvalueReserved, expression.GetLocation());
		}
	}

	private static bool IsDeconstructionCompatibleArgument(ExpressionSyntax expression)
	{
		if (expression.Kind() == SyntaxKind.InvocationExpression)
		{
			ExpressionSyntax expression2 = ((InvocationExpressionSyntax)expression).Expression;
			if (expression2.Kind() == SyntaxKind.IdentifierName)
			{
				return ((IdentifierNameSyntax)expression2).IsVar;
			}
			return false;
		}
		return false;
	}

	internal static SimpleNameSyntax? GetInterceptableNameSyntax(this InvocationExpressionSyntax invocation)
	{
		ExpressionSyntax expression = invocation.Expression;
		if (!(expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax))
		{
			if (!(expression is MemberBindingExpressionSyntax memberBindingExpressionSyntax))
			{
				if (expression is SimpleNameSyntax result)
				{
					return result;
				}
				return null;
			}
			return memberBindingExpressionSyntax.Name;
		}
		return memberAccessExpressionSyntax.Name;
	}
}
