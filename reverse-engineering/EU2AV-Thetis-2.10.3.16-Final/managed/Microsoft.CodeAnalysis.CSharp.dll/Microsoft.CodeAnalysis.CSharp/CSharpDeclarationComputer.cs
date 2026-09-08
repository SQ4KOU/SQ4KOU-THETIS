using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp;

internal class CSharpDeclarationComputer : DeclarationComputer
{
	public static void ComputeDeclarationsInSpan(SemanticModel model, TextSpan span, bool getSymbol, ArrayBuilder<DeclarationInfo> builder, CancellationToken cancellationToken)
	{
		ComputeDeclarations(model, null, model.SyntaxTree.GetRoot(cancellationToken), (SyntaxNode node, int? level) => !node.Span.OverlapsWith(span) || InvalidLevel(level), getSymbol, builder, null, cancellationToken);
	}

	public static void ComputeDeclarationsInNode(SemanticModel model, ISymbol associatedSymbol, SyntaxNode node, bool getSymbol, ArrayBuilder<DeclarationInfo> builder, CancellationToken cancellationToken, int? levelsToCompute = null)
	{
		ComputeDeclarations(model, associatedSymbol, node, (SyntaxNode n, int? level) => InvalidLevel(level), getSymbol, builder, levelsToCompute, cancellationToken);
	}

	private static bool InvalidLevel(int? level)
	{
		if (level.HasValue)
		{
			return level.Value <= 0;
		}
		return false;
	}

	private static int? DecrementLevel(int? level)
	{
		if (!level.HasValue)
		{
			return level;
		}
		return level - 1;
	}

	private static void ComputeDeclarations(SemanticModel model, ISymbol associatedSymbol, SyntaxNode node, Func<SyntaxNode, int?, bool> shouldSkip, bool getSymbol, ArrayBuilder<DeclarationInfo> builder, int? levelsToCompute, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (shouldSkip(node, levelsToCompute))
		{
			return;
		}
		int? levelsToCompute2 = DecrementLevel(levelsToCompute);
		switch (node.Kind())
		{
		case SyntaxKind.NamespaceDeclaration:
		case SyntaxKind.FileScopedNamespaceDeclaration:
		{
			BaseNamespaceDeclarationSyntax baseNamespaceDeclarationSyntax = (BaseNamespaceDeclarationSyntax)node;
			foreach (MemberDeclarationSyntax member in baseNamespaceDeclarationSyntax.Members)
			{
				ComputeDeclarations(model, null, member, shouldSkip, getSymbol, builder, levelsToCompute2, cancellationToken);
			}
			DeclarationInfo declarationInfo = DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, cancellationToken);
			builder.Add(declarationInfo);
			NameSyntax nameSyntax = baseNamespaceDeclarationSyntax.Name;
			INamespaceSymbol namespaceSymbol = declarationInfo.DeclaredSymbol as INamespaceSymbol;
			while (nameSyntax.Kind() == SyntaxKind.QualifiedName)
			{
				nameSyntax = ((QualifiedNameSyntax)nameSyntax).Left;
				INamespaceSymbol namespaceSymbol2 = (getSymbol ? namespaceSymbol?.ContainingNamespace : null);
				builder.Add(new DeclarationInfo(nameSyntax, ImmutableArray<SyntaxNode>.Empty, namespaceSymbol2));
				namespaceSymbol = namespaceSymbol2;
			}
			break;
		}
		case SyntaxKind.ClassDeclaration:
		case SyntaxKind.StructDeclaration:
		case SyntaxKind.RecordDeclaration:
		case SyntaxKind.RecordStructDeclaration:
			if (associatedSymbol is IMethodSymbol)
			{
				TypeDeclarationSyntax obj3 = (TypeDeclarationSyntax)node;
				ArrayBuilder<SyntaxNode> instance10 = ArrayBuilder<SyntaxNode>.GetInstance();
				AddParameterListInitializersAndAttributes(obj3.ParameterList, instance10);
				if (obj3.BaseList?.Types.FirstOrDefault() is PrimaryConstructorBaseTypeSyntax item)
				{
					instance10.Add(item);
				}
				builder.Add(DeclarationComputer.GetDeclarationInfo(node, associatedSymbol, instance10));
				instance10.Free();
				break;
			}
			goto case SyntaxKind.InterfaceDeclaration;
		case SyntaxKind.InterfaceDeclaration:
		case SyntaxKind.ExtensionBlockDeclaration:
		{
			TypeDeclarationSyntax typeDeclarationSyntax = (TypeDeclarationSyntax)node;
			foreach (MemberDeclarationSyntax member2 in typeDeclarationSyntax.Members)
			{
				ComputeDeclarations(model, null, member2, shouldSkip, getSymbol, builder, levelsToCompute2, cancellationToken);
			}
			ArrayBuilder<SyntaxNode> instance14 = ArrayBuilder<SyntaxNode>.GetInstance();
			AddAttributes(typeDeclarationSyntax.AttributeLists, instance14);
			AddTypeParameterListAttributes(typeDeclarationSyntax.TypeParameterList, instance14);
			builder.Add(DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, instance14, cancellationToken));
			instance14.Free();
			break;
		}
		case SyntaxKind.EnumDeclaration:
		{
			EnumDeclarationSyntax enumDeclarationSyntax = (EnumDeclarationSyntax)node;
			foreach (EnumMemberDeclarationSyntax member3 in enumDeclarationSyntax.Members)
			{
				ComputeDeclarations(model, null, member3, shouldSkip, getSymbol, builder, levelsToCompute2, cancellationToken);
			}
			ArrayBuilder<SyntaxNode> instance13 = ArrayBuilder<SyntaxNode>.GetInstance();
			AddAttributes(enumDeclarationSyntax.AttributeLists, instance13);
			builder.Add(DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, instance13, cancellationToken));
			instance13.Free();
			break;
		}
		case SyntaxKind.EnumMemberDeclaration:
		{
			EnumMemberDeclarationSyntax enumMemberDeclarationSyntax = (EnumMemberDeclarationSyntax)node;
			ArrayBuilder<SyntaxNode> instance7 = ArrayBuilder<SyntaxNode>.GetInstance();
			instance7.Add(enumMemberDeclarationSyntax.EqualsValue);
			AddAttributes(enumMemberDeclarationSyntax.AttributeLists, instance7);
			builder.Add(DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, instance7, cancellationToken));
			instance7.Free();
			break;
		}
		case SyntaxKind.DelegateDeclaration:
		{
			DelegateDeclarationSyntax obj = (DelegateDeclarationSyntax)node;
			ArrayBuilder<SyntaxNode> instance5 = ArrayBuilder<SyntaxNode>.GetInstance();
			AddAttributes(obj.AttributeLists, instance5);
			AddParameterListInitializersAndAttributes(obj.ParameterList, instance5);
			AddTypeParameterListAttributes(obj.TypeParameterList, instance5);
			builder.Add(DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, instance5, cancellationToken));
			instance5.Free();
			break;
		}
		case SyntaxKind.EventDeclaration:
		{
			EventDeclarationSyntax eventDeclarationSyntax = (EventDeclarationSyntax)node;
			if (eventDeclarationSyntax.AccessorList != null)
			{
				foreach (AccessorDeclarationSyntax accessor in eventDeclarationSyntax.AccessorList.Accessors)
				{
					ComputeDeclarations(model, null, accessor, shouldSkip, getSymbol, builder, levelsToCompute2, cancellationToken);
				}
			}
			ArrayBuilder<SyntaxNode> instance11 = ArrayBuilder<SyntaxNode>.GetInstance();
			AddAttributes(eventDeclarationSyntax.AttributeLists, instance11);
			builder.Add(DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, instance11, cancellationToken));
			instance11.Free();
			break;
		}
		case SyntaxKind.FieldDeclaration:
		case SyntaxKind.EventFieldDeclaration:
		{
			BaseFieldDeclarationSyntax obj2 = (BaseFieldDeclarationSyntax)node;
			ArrayBuilder<SyntaxNode> instance8 = ArrayBuilder<SyntaxNode>.GetInstance();
			AddAttributes(obj2.AttributeLists, instance8);
			foreach (VariableDeclaratorSyntax variable in obj2.Declaration.Variables)
			{
				ArrayBuilder<SyntaxNode> instance9 = ArrayBuilder<SyntaxNode>.GetInstance();
				instance9.Add(variable.Initializer);
				instance9.AddRange(instance8);
				builder.Add(DeclarationComputer.GetDeclarationInfo(model, variable, getSymbol, instance9, cancellationToken));
				instance9.Free();
			}
			instance8.Free();
			break;
		}
		case SyntaxKind.ArrowExpressionClause:
			if (node.Parent is BasePropertyDeclarationSyntax declarationWithExpressionBody)
			{
				builder.Add(GetExpressionBodyDeclarationInfo(declarationWithExpressionBody, (ArrowExpressionClauseSyntax)node, model, getSymbol, cancellationToken));
			}
			break;
		case SyntaxKind.PropertyDeclaration:
		{
			PropertyDeclarationSyntax propertyDeclarationSyntax = (PropertyDeclarationSyntax)node;
			if (propertyDeclarationSyntax.AccessorList != null)
			{
				foreach (AccessorDeclarationSyntax accessor2 in propertyDeclarationSyntax.AccessorList.Accessors)
				{
					ComputeDeclarations(model, null, accessor2, shouldSkip, getSymbol, builder, levelsToCompute2, cancellationToken);
				}
			}
			if (propertyDeclarationSyntax.ExpressionBody != null)
			{
				ComputeDeclarations(model, null, propertyDeclarationSyntax.ExpressionBody, shouldSkip, getSymbol, builder, levelsToCompute, cancellationToken);
			}
			ArrayBuilder<SyntaxNode> instance12 = ArrayBuilder<SyntaxNode>.GetInstance();
			instance12.Add(propertyDeclarationSyntax.Initializer);
			AddAttributes(propertyDeclarationSyntax.AttributeLists, instance12);
			builder.Add(DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, instance12, cancellationToken));
			instance12.Free();
			break;
		}
		case SyntaxKind.IndexerDeclaration:
		{
			IndexerDeclarationSyntax indexerDeclarationSyntax = (IndexerDeclarationSyntax)node;
			if (indexerDeclarationSyntax.AccessorList != null)
			{
				foreach (AccessorDeclarationSyntax accessor3 in indexerDeclarationSyntax.AccessorList.Accessors)
				{
					ComputeDeclarations(model, null, accessor3, shouldSkip, getSymbol, builder, levelsToCompute2, cancellationToken);
				}
			}
			if (indexerDeclarationSyntax.ExpressionBody != null)
			{
				ComputeDeclarations(model, null, indexerDeclarationSyntax.ExpressionBody, shouldSkip, getSymbol, builder, levelsToCompute, cancellationToken);
			}
			ArrayBuilder<SyntaxNode> instance4 = ArrayBuilder<SyntaxNode>.GetInstance();
			AddParameterListInitializersAndAttributes(indexerDeclarationSyntax.ParameterList, instance4);
			AddAttributes(indexerDeclarationSyntax.AttributeLists, instance4);
			builder.Add(DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, instance4, cancellationToken));
			instance4.Free();
			break;
		}
		case SyntaxKind.GetAccessorDeclaration:
		case SyntaxKind.SetAccessorDeclaration:
		case SyntaxKind.AddAccessorDeclaration:
		case SyntaxKind.RemoveAccessorDeclaration:
		case SyntaxKind.InitAccessorDeclaration:
		{
			AccessorDeclarationSyntax accessorDeclarationSyntax = (AccessorDeclarationSyntax)node;
			ArrayBuilder<SyntaxNode> instance3 = ArrayBuilder<SyntaxNode>.GetInstance();
			instance3.AddIfNotNull(accessorDeclarationSyntax.Body);
			instance3.AddIfNotNull(accessorDeclarationSyntax.ExpressionBody);
			AddAttributes(accessorDeclarationSyntax.AttributeLists, instance3);
			builder.Add(DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, instance3, cancellationToken));
			instance3.Free();
			break;
		}
		case SyntaxKind.MethodDeclaration:
		case SyntaxKind.OperatorDeclaration:
		case SyntaxKind.ConversionOperatorDeclaration:
		case SyntaxKind.ConstructorDeclaration:
		case SyntaxKind.DestructorDeclaration:
		{
			BaseMethodDeclarationSyntax baseMethodDeclarationSyntax = (BaseMethodDeclarationSyntax)node;
			ArrayBuilder<SyntaxNode> instance6 = ArrayBuilder<SyntaxNode>.GetInstance();
			AddParameterListInitializersAndAttributes(baseMethodDeclarationSyntax.ParameterList, instance6);
			instance6.Add(baseMethodDeclarationSyntax.Body);
			if (baseMethodDeclarationSyntax is ConstructorDeclarationSyntax { Initializer: not null } constructorDeclarationSyntax)
			{
				instance6.Add(constructorDeclarationSyntax.Initializer);
			}
			ArrowExpressionClauseSyntax expressionBodySyntax = GetExpressionBodySyntax(baseMethodDeclarationSyntax);
			if (expressionBodySyntax != null)
			{
				instance6.Add(expressionBodySyntax);
			}
			AddAttributes(baseMethodDeclarationSyntax.AttributeLists, instance6);
			if (node is MethodDeclarationSyntax { TypeParameterList: not null } methodDeclarationSyntax)
			{
				AddTypeParameterListAttributes(methodDeclarationSyntax.TypeParameterList, instance6);
			}
			builder.Add(DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, instance6, cancellationToken));
			instance6.Free();
			break;
		}
		case SyntaxKind.CompilationUnit:
		{
			CompilationUnitSyntax compilationUnitSyntax = (CompilationUnitSyntax)node;
			if (associatedSymbol is IMethodSymbol)
			{
				ArrayBuilder<SyntaxNode> instance = ArrayBuilder<SyntaxNode>.GetInstance();
				instance.Add(compilationUnitSyntax);
				builder.Add(DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, instance, cancellationToken));
				instance.Free();
				break;
			}
			foreach (MemberDeclarationSyntax member4 in compilationUnitSyntax.Members)
			{
				ComputeDeclarations(model, null, member4, shouldSkip, getSymbol, builder, levelsToCompute2, cancellationToken);
			}
			if (compilationUnitSyntax.AttributeLists.Any())
			{
				ArrayBuilder<SyntaxNode> instance2 = ArrayBuilder<SyntaxNode>.GetInstance();
				AddAttributes(compilationUnitSyntax.AttributeLists, instance2);
				builder.Add(DeclarationComputer.GetDeclarationInfo(model, node, getSymbol: false, instance2, cancellationToken));
				instance2.Free();
			}
			break;
		}
		}
	}

	private static void AddAttributes(SyntaxList<AttributeListSyntax> attributeLists, ArrayBuilder<SyntaxNode> builder)
	{
		foreach (AttributeListSyntax item in attributeLists)
		{
			foreach (AttributeSyntax attribute in item.Attributes)
			{
				builder.Add(attribute);
			}
		}
	}

	private static void AddParameterListInitializersAndAttributes(BaseParameterListSyntax parameterList, ArrayBuilder<SyntaxNode> builder)
	{
		if (parameterList != null)
		{
			foreach (ParameterSyntax parameter in parameterList.Parameters)
			{
				AddParameterInitializersAndAttributes(parameter, builder);
			}
		}
	}

	private static void AddParameterInitializersAndAttributes(ParameterSyntax parameter, ArrayBuilder<SyntaxNode> builder)
	{
		builder.Add(parameter.Default);
		AddAttributes(parameter.AttributeLists, builder);
	}

	private static void AddTypeParameterListAttributes(TypeParameterListSyntax typeParameterList, ArrayBuilder<SyntaxNode> builder)
	{
		if (typeParameterList != null)
		{
			foreach (TypeParameterSyntax parameter in typeParameterList.Parameters)
			{
				AddAttributes(parameter.AttributeLists, builder);
			}
		}
	}

	private static DeclarationInfo GetExpressionBodyDeclarationInfo(BasePropertyDeclarationSyntax declarationWithExpressionBody, ArrowExpressionClauseSyntax expressionBody, SemanticModel model, bool getSymbol, CancellationToken cancellationToken)
	{
		IMethodSymbol declaredSymbol = ((!getSymbol) ? null : (model.GetDeclaredSymbol(declarationWithExpressionBody, cancellationToken) as IPropertySymbol)?.GetMethod);
		return new DeclarationInfo(expressionBody, ImmutableArray.Create((SyntaxNode)expressionBody), declaredSymbol);
	}

	internal static ArrowExpressionClauseSyntax GetExpressionBodySyntax(CSharpSyntaxNode node)
	{
		ArrowExpressionClauseSyntax result = null;
		switch (node.Kind())
		{
		case SyntaxKind.ArrowExpressionClause:
			result = (ArrowExpressionClauseSyntax)node;
			break;
		case SyntaxKind.MethodDeclaration:
			result = ((MethodDeclarationSyntax)node).ExpressionBody;
			break;
		case SyntaxKind.OperatorDeclaration:
			result = ((OperatorDeclarationSyntax)node).ExpressionBody;
			break;
		case SyntaxKind.ConversionOperatorDeclaration:
			result = ((ConversionOperatorDeclarationSyntax)node).ExpressionBody;
			break;
		case SyntaxKind.PropertyDeclaration:
			result = ((PropertyDeclarationSyntax)node).ExpressionBody;
			break;
		case SyntaxKind.IndexerDeclaration:
			result = ((IndexerDeclarationSyntax)node).ExpressionBody;
			break;
		case SyntaxKind.ConstructorDeclaration:
			result = ((ConstructorDeclarationSyntax)node).ExpressionBody;
			break;
		case SyntaxKind.DestructorDeclaration:
			result = ((DestructorDeclarationSyntax)node).ExpressionBody;
			break;
		default:
			ExceptionUtilities.UnexpectedValue(node.Kind());
			break;
		}
		return result;
	}
}
