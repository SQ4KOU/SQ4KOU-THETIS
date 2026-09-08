namespace Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class SyntaxBindingUtilities
{
	public static bool BindsToResumableStateMachineState(SyntaxNode node)
	{
		bool flag = node.IsKind(SyntaxKind.YieldReturnStatement) || node.IsKind(SyntaxKind.AwaitExpression);
		bool flag2;
		if (!flag)
		{
			if (node is CommonForEachStatementSyntax { AwaitKeyword: var awaitKeyword })
			{
				if (awaitKeyword.RawKind != 0)
				{
					goto IL_00cc;
				}
			}
			else if (node is VariableDeclaratorSyntax variableDeclaratorSyntax)
			{
				CSharpSyntaxNode parent = variableDeclaratorSyntax.Parent;
				if (parent != null)
				{
					CSharpSyntaxNode parent2 = parent.Parent;
					if (parent2 is UsingStatementSyntax { AwaitKeyword: var awaitKeyword2 })
					{
						if (awaitKeyword2.RawKind != 0)
						{
							goto IL_00cc;
						}
					}
					else if (parent2 is LocalDeclarationStatementSyntax { AwaitKeyword: { RawKind: not 0 } })
					{
						goto IL_00cc;
					}
				}
			}
			else if (node is UsingStatementSyntax { Expression: not null, AwaitKeyword: { RawKind: not 0 } })
			{
				goto IL_00cc;
			}
			flag2 = false;
			goto IL_00d4;
		}
		goto IL_00d7;
		IL_00cc:
		flag2 = true;
		goto IL_00d4;
		IL_00d7:
		return flag;
		IL_00d4:
		flag = flag2;
		goto IL_00d7;
	}

	public static bool BindsToTryStatement(SyntaxNode node)
	{
		if (node is VariableDeclaratorSyntax variableDeclaratorSyntax)
		{
			CSharpSyntaxNode parent = variableDeclaratorSyntax.Parent;
			if (parent != null)
			{
				CSharpSyntaxNode parent2 = parent.Parent;
				if (parent2 is UsingStatementSyntax || parent2 is LocalDeclarationStatementSyntax { UsingKeyword: { RawKind: not 0 } })
				{
					goto IL_006f;
				}
			}
		}
		else if (node is UsingStatementSyntax usingStatementSyntax)
		{
			if (usingStatementSyntax.Expression != null)
			{
				goto IL_006f;
			}
		}
		else if (node is CommonForEachStatementSyntax || node is TryStatementSyntax || node is LockStatementSyntax)
		{
			goto IL_006f;
		}
		return false;
		IL_006f:
		return true;
	}
}
