using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class CSharpSemanticModel : SemanticModel
{
	[Flags]
	internal enum SymbolInfoOptions
	{
		PreferTypeToConstructors = 1,
		PreferConstructorsToType = 2,
		ResolveAliases = 4,
		PreserveAliases = 8,
		DefaultOptions = PreferConstructorsToType | ResolveAliases
	}

	public new abstract CSharpCompilation Compilation { get; }

	internal new abstract CSharpSyntaxNode Root { get; }

	public new abstract CSharpSemanticModel ParentModel { get; }

	public new abstract SyntaxTree SyntaxTree { get; }

	public sealed override string Language => "C#";

	protected sealed override Compilation CompilationCore => Compilation;

	protected sealed override SemanticModel ParentModelCore => ParentModel;

	protected sealed override SyntaxTree SyntaxTreeCore => SyntaxTree;

	protected sealed override SyntaxNode RootCore => Root;

	internal static bool CanGetSemanticInfo(CSharpSyntaxNode node, bool allowNamedArgumentName = false, bool isSpeculative = false)
	{
		if (!isSpeculative && IsInStructuredTriviaOtherThanCrefOrNameAttribute(node))
		{
			return false;
		}
		switch (node.Kind())
		{
		case SyntaxKind.ObjectInitializerExpression:
		case SyntaxKind.CollectionInitializerExpression:
			return false;
		case SyntaxKind.ComplexElementInitializerExpression:
			return false;
		case SyntaxKind.IdentifierName:
			if (!isSpeculative && node.Parent != null && node.Parent.Kind() == SyntaxKind.NameEquals && node.Parent.Parent.Kind() == SyntaxKind.UsingDirective)
			{
				return false;
			}
			break;
		case SyntaxKind.OmittedTypeArgument:
		case SyntaxKind.RefExpression:
		case SyntaxKind.RefType:
		case SyntaxKind.ScopedType:
			return false;
		}
		if (node.IsMissing)
		{
			return false;
		}
		if ((!(node is ExpressionSyntax) || (!(isSpeculative | allowNamedArgumentName) && SyntaxFacts.IsNamedArgumentName(node))) && !(node is ConstructorInitializerSyntax) && !(node is PrimaryConstructorBaseTypeSyntax) && !(node is AttributeSyntax))
		{
			return node is CrefSyntax;
		}
		return true;
	}

	internal abstract SymbolInfo GetSymbolInfoWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default(CancellationToken));

	internal abstract SymbolInfo GetCollectionInitializerSymbolInfoWorker(InitializerExpressionSyntax collectionInitializer, ExpressionSyntax node, CancellationToken cancellationToken = default(CancellationToken));

	internal abstract CSharpTypeInfo GetTypeInfoWorker(CSharpSyntaxNode node, CancellationToken cancellationToken = default(CancellationToken));

	internal abstract BoundExpression GetSpeculativelyBoundExpression(int position, ExpressionSyntax expression, SpeculativeBindingOption bindingOption, out Binder binder, out ImmutableArray<Symbol> crefSymbols);

	internal abstract ImmutableArray<Symbol> GetMemberGroupWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default(CancellationToken));

	internal abstract ImmutableArray<IPropertySymbol> GetIndexerGroupWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default(CancellationToken));

	internal abstract Optional<object> GetConstantValueWorker(CSharpSyntaxNode node, CancellationToken cancellationToken = default(CancellationToken));

	internal Binder GetSpeculativeBinder(int position, ExpressionSyntax expression, SpeculativeBindingOption bindingOption)
	{
		position = CheckAndAdjustPosition(position);
		if (bindingOption == SpeculativeBindingOption.BindAsTypeOrNamespace && !(expression is TypeSyntax))
		{
			return null;
		}
		Binder binder = GetEnclosingBinder(position);
		if (binder == null)
		{
			return null;
		}
		if (bindingOption == SpeculativeBindingOption.BindAsTypeOrNamespace && IsInTypeofExpression(position))
		{
			binder = new TypeofBinder(expression, binder);
		}
		binder = new WithNullableContextBinder(SyntaxTree, position, binder);
		return new ExecutableCodeBinder(expression, binder.ContainingMemberOrLambda, binder).GetBinder(expression);
	}

	private Binder GetSpeculativeBinderForAttribute(int position, AttributeSyntax attribute)
	{
		position = CheckAndAdjustPositionForSpeculativeAttribute(position);
		Binder enclosingBinder = GetEnclosingBinder(position);
		if (enclosingBinder == null)
		{
			return null;
		}
		return new ExecutableCodeBinder(attribute, enclosingBinder.ContainingMemberOrLambda, enclosingBinder).GetBinder(attribute);
	}

	private static BoundExpression GetSpeculativelyBoundExpressionHelper(Binder binder, ExpressionSyntax expression, SpeculativeBindingOption bindingOption)
	{
		if (bindingOption == SpeculativeBindingOption.BindAsTypeOrNamespace || binder.Flags.Includes(BinderFlags.CrefParameterOrReturnType))
		{
			return binder.BindNamespaceOrType(expression, BindingDiagnosticBag.Discarded);
		}
		return binder.BindExpression(expression, BindingDiagnosticBag.Discarded);
	}

	protected BoundExpression GetSpeculativelyBoundExpressionWithoutNullability(int position, ExpressionSyntax expression, SpeculativeBindingOption bindingOption, out Binder binder, out ImmutableArray<Symbol> crefSymbols)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		crefSymbols = default(ImmutableArray<Symbol>);
		expression = SyntaxFactory.GetStandaloneExpression(expression);
		binder = GetSpeculativeBinder(position, expression, bindingOption);
		if (binder == null)
		{
			return null;
		}
		if (binder.Flags.Includes(BinderFlags.CrefParameterOrReturnType))
		{
			crefSymbols = ImmutableArray.Create((Symbol)binder.BindType(expression, BindingDiagnosticBag.Discarded).Type);
			return null;
		}
		if (binder.InCref)
		{
			if (expression.IsKind(SyntaxKind.QualifiedName))
			{
				QualifiedNameSyntax qualifiedNameSyntax = (QualifiedNameSyntax)expression;
				QualifiedCrefSyntax crefSyntax = SyntaxFactory.QualifiedCref(qualifiedNameSyntax.Left, SyntaxFactory.NameMemberCref(qualifiedNameSyntax.Right));
				crefSymbols = BindCref(crefSyntax, binder);
			}
			else if (expression is TypeSyntax typeSyntax)
			{
				CrefSyntax crefSyntax2 = ((typeSyntax is PredefinedTypeSyntax) ? ((CrefSyntax)SyntaxFactory.TypeCref(typeSyntax)) : ((CrefSyntax)SyntaxFactory.NameMemberCref(typeSyntax)));
				crefSymbols = BindCref(crefSyntax2, binder);
			}
			return null;
		}
		return GetSpeculativelyBoundExpressionHelper(binder, expression, bindingOption);
	}

	internal static ImmutableArray<Symbol> BindCref(CrefSyntax crefSyntax, Binder binder)
	{
		Symbol ambiguityWinner;
		return binder.BindCref(crefSyntax, out ambiguityWinner, BindingDiagnosticBag.Discarded);
	}

	internal SymbolInfo GetCrefSymbolInfo(int position, CrefSyntax crefSyntax, SymbolInfoOptions options, bool hasParameterList)
	{
		Binder enclosingBinder = GetEnclosingBinder(position);
		if (enclosingBinder != null && enclosingBinder.InCref)
		{
			return GetCrefSymbolInfo(OneOrMany.Create(BindCref(crefSyntax, enclosingBinder)), options, hasParameterList);
		}
		return SymbolInfo.None;
	}

	internal static bool HasParameterList(CrefSyntax crefSyntax)
	{
		while (crefSyntax.Kind() == SyntaxKind.QualifiedCref)
		{
			crefSyntax = ((QualifiedCrefSyntax)crefSyntax).Member;
		}
		return crefSyntax.Kind() switch
		{
			SyntaxKind.NameMemberCref => ((NameMemberCrefSyntax)crefSyntax).Parameters != null, 
			SyntaxKind.IndexerMemberCref => ((IndexerMemberCrefSyntax)crefSyntax).Parameters != null, 
			SyntaxKind.OperatorMemberCref => ((OperatorMemberCrefSyntax)crefSyntax).Parameters != null, 
			SyntaxKind.ConversionOperatorMemberCref => ((ConversionOperatorMemberCrefSyntax)crefSyntax).Parameters != null, 
			SyntaxKind.ExtensionMemberCref => HasParameterList(((ExtensionMemberCrefSyntax)crefSyntax).Member), 
			_ => false, 
		};
	}

	private static SymbolInfo GetCrefSymbolInfo(OneOrMany<Symbol> symbols, SymbolInfoOptions options, bool hasParameterList)
	{
		switch (symbols.Count)
		{
		case 0:
			return SymbolInfo.None;
		case 1:
			return GetSymbolInfoForSymbol(symbols[0], options);
		default:
		{
			if ((options & SymbolInfoOptions.ResolveAliases) == SymbolInfoOptions.ResolveAliases)
			{
				symbols = UnwrapAliases(symbols);
			}
			LookupResultKind resultKind = LookupResultKind.Ambiguous;
			SymbolKind firstCandidateKind = symbols[0].Kind;
			if (hasParameterList && symbols.All((Symbol s) => s.Kind == firstCandidateKind))
			{
				resultKind = LookupResultKind.OverloadResolutionFailure;
			}
			return SymbolInfoFactory.Create(symbols, resultKind, isDynamic: false);
		}
		}
	}

	private BoundAttribute GetSpeculativelyBoundAttribute(int position, AttributeSyntax attribute, out Binder binder)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute");
		}
		binder = GetSpeculativeBinderForAttribute(position, attribute);
		if (binder == null)
		{
			return null;
		}
		Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol attributeType = (Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol)binder.BindType(attribute.Name, BindingDiagnosticBag.Discarded, out var _).Type;
		return new ExecutableCodeBinder(attribute, binder.ContainingMemberOrLambda, binder).BindAttribute(attribute, attributeType, null, BindingDiagnosticBag.Discarded);
	}

	private int CheckAndAdjustPositionForSpeculativeAttribute(int position)
	{
		position = CheckAndAdjustPosition(position);
		SyntaxToken syntaxToken = Root.FindToken(position);
		if (position == 0 && position != syntaxToken.SpanStart)
		{
			return position;
		}
		CSharpSyntaxNode cSharpSyntaxNode = (CSharpSyntaxNode)syntaxToken.Parent;
		if (position == cSharpSyntaxNode.SpanStart)
		{
			if (cSharpSyntaxNode is BaseTypeDeclarationSyntax { OpenBraceToken: var openBraceToken })
			{
				position = openBraceToken.SpanStart;
			}
			MethodDeclarationSyntax methodDeclarationSyntax = cSharpSyntaxNode.FirstAncestorOrSelf<MethodDeclarationSyntax>();
			if (methodDeclarationSyntax != null && methodDeclarationSyntax.SpanStart == position)
			{
				position = methodDeclarationSyntax.Identifier.SpanStart;
			}
		}
		return position;
	}

	protected override IOperation GetOperationCore(SyntaxNode node, CancellationToken cancellationToken)
	{
		CSharpSyntaxNode cSharpSyntaxNode = (CSharpSyntaxNode)node;
		CheckSyntaxNode(cSharpSyntaxNode);
		return GetOperationWorker(cSharpSyntaxNode, cancellationToken);
	}

	internal virtual IOperation GetOperationWorker(CSharpSyntaxNode node, CancellationToken cancellationToken)
	{
		return null;
	}

	public abstract SymbolInfo GetSymbolInfo(OrderingSyntax node, CancellationToken cancellationToken = default(CancellationToken));

	public abstract SymbolInfo GetSymbolInfo(SelectOrGroupClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken));

	public SymbolInfo GetSymbolInfo(PositionalPatternClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(node);
		return GetSymbolInfoWorker(node, SymbolInfoOptions.DefaultOptions, cancellationToken);
	}

	public SymbolInfo GetSymbolInfo(ExpressionSyntax expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(expression);
		if (!CanGetSemanticInfo(expression, allowNamedArgumentName: true))
		{
			return SymbolInfo.None;
		}
		if (SyntaxFacts.IsNamedArgumentName(expression))
		{
			return GetNamedArgumentSymbolInfo((IdentifierNameSyntax)expression, cancellationToken);
		}
		if (SyntaxFacts.IsDeclarationExpressionType(expression, out DeclarationExpressionSyntax parent))
		{
			switch (parent.Designation.Kind())
			{
			case SyntaxKind.SingleVariableDesignation:
				return GetSymbolInfoFromSymbolOrNone(TypeFromVariable((SingleVariableDesignationSyntax)parent.Designation, cancellationToken).Type);
			case SyntaxKind.DiscardDesignation:
				return GetSymbolInfoFromSymbolOrNone(GetTypeInfoWorker(parent, cancellationToken).Type.GetPublicSymbol());
			case SyntaxKind.ParenthesizedVariableDesignation:
				if (((TypeSyntax)expression).IsVar)
				{
					CSharpTypeInfo typeInfoWorker = GetTypeInfoWorker(expression, cancellationToken);
					Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type = typeInfoWorker.Type;
					if ((object)type != null && type.TypeKind != TypeKind.Error)
					{
						return GetSymbolInfoFromSymbolOrNone(typeInfoWorker.Type.GetPublicSymbol());
					}
					return GetSymbolInfoFromSymbolOrNone(GetTypeInfoWorker(parent, cancellationToken).Type.GetPublicSymbol());
				}
				break;
			}
		}
		else if (expression is DeclarationExpressionSyntax declarationExpressionSyntax)
		{
			if (declarationExpressionSyntax.Designation.Kind() != SyntaxKind.SingleVariableDesignation)
			{
				return SymbolInfo.None;
			}
			ISymbol declaredSymbol = GetDeclaredSymbol((SingleVariableDesignationSyntax)declarationExpressionSyntax.Designation, cancellationToken);
			if (declaredSymbol == null)
			{
				return SymbolInfo.None;
			}
			return new SymbolInfo(declaredSymbol);
		}
		return GetSymbolInfoWorker(expression, SymbolInfoOptions.DefaultOptions, cancellationToken);
	}

	private static SymbolInfo GetSymbolInfoFromSymbolOrNone(ITypeSymbol type)
	{
		if (type == null || type.Kind != SymbolKind.ErrorType)
		{
			return new SymbolInfo(type);
		}
		return SymbolInfo.None;
	}

	private (ITypeSymbol Type, Microsoft.CodeAnalysis.NullableAnnotation Annotation) TypeFromVariable(SingleVariableDesignationSyntax variableDesignation, CancellationToken cancellationToken)
	{
		ISymbol declaredSymbol = GetDeclaredSymbol(variableDesignation, cancellationToken);
		if (!(declaredSymbol is ILocalSymbol localSymbol))
		{
			if (declaredSymbol is IFieldSymbol fieldSymbol)
			{
				return (Type: fieldSymbol.Type, Annotation: fieldSymbol.NullableAnnotation);
			}
			return default((ITypeSymbol, Microsoft.CodeAnalysis.NullableAnnotation));
		}
		return (Type: localSymbol.Type, Annotation: localSymbol.NullableAnnotation);
	}

	public SymbolInfo GetCollectionInitializerSymbolInfo(ExpressionSyntax expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(expression);
		if (expression.Parent != null && expression.Parent.Kind() == SyntaxKind.CollectionInitializerExpression)
		{
			InitializerExpressionSyntax initializerExpressionSyntax = (InitializerExpressionSyntax)expression.Parent;
			while (initializerExpressionSyntax.Parent != null && initializerExpressionSyntax.Parent.Kind() == SyntaxKind.SimpleAssignmentExpression && ((AssignmentExpressionSyntax)initializerExpressionSyntax.Parent).Right == initializerExpressionSyntax && initializerExpressionSyntax.Parent.Parent != null && initializerExpressionSyntax.Parent.Parent.Kind() == SyntaxKind.ObjectInitializerExpression)
			{
				initializerExpressionSyntax = (InitializerExpressionSyntax)initializerExpressionSyntax.Parent.Parent;
			}
			if (initializerExpressionSyntax.Parent is BaseObjectCreationExpressionSyntax baseObjectCreationExpressionSyntax && baseObjectCreationExpressionSyntax.Initializer == initializerExpressionSyntax && CanGetSemanticInfo(baseObjectCreationExpressionSyntax))
			{
				return GetCollectionInitializerSymbolInfoWorker((InitializerExpressionSyntax)expression.Parent, expression, cancellationToken);
			}
		}
		return SymbolInfo.None;
	}

	public SymbolInfo GetSymbolInfo(ConstructorInitializerSyntax constructorInitializer, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(constructorInitializer);
		if (!CanGetSemanticInfo(constructorInitializer))
		{
			return SymbolInfo.None;
		}
		return GetSymbolInfoWorker(constructorInitializer, SymbolInfoOptions.DefaultOptions, cancellationToken);
	}

	public SymbolInfo GetSymbolInfo(PrimaryConstructorBaseTypeSyntax constructorInitializer, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(constructorInitializer);
		if (!CanGetSemanticInfo(constructorInitializer))
		{
			return SymbolInfo.None;
		}
		return GetSymbolInfoWorker(constructorInitializer, SymbolInfoOptions.DefaultOptions, cancellationToken);
	}

	public SymbolInfo GetSymbolInfo(AttributeSyntax attributeSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(attributeSyntax);
		if (!CanGetSemanticInfo(attributeSyntax))
		{
			return SymbolInfo.None;
		}
		return GetSymbolInfoWorker(attributeSyntax, SymbolInfoOptions.DefaultOptions, cancellationToken);
	}

	public SymbolInfo GetSymbolInfo(CrefSyntax crefSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(crefSyntax);
		if (!CanGetSemanticInfo(crefSyntax))
		{
			return SymbolInfo.None;
		}
		return GetSymbolInfoWorker(crefSyntax, SymbolInfoOptions.DefaultOptions, cancellationToken);
	}

	public SymbolInfo GetSpeculativeSymbolInfo(int position, ExpressionSyntax expression, SpeculativeBindingOption bindingOption)
	{
		if (!CanGetSemanticInfo(expression, allowNamedArgumentName: false, isSpeculative: true))
		{
			return SymbolInfo.None;
		}
		BoundNode speculativelyBoundExpression = GetSpeculativelyBoundExpression(position, expression, bindingOption, out var binder, out var crefSymbols);
		if (speculativelyBoundExpression == null)
		{
			if (!crefSymbols.IsDefault)
			{
				return GetCrefSymbolInfo(OneOrMany.Create(crefSymbols), SymbolInfoOptions.DefaultOptions, hasParameterList: false);
			}
			return SymbolInfo.None;
		}
		return GetSymbolInfoForNode(SymbolInfoOptions.DefaultOptions, speculativelyBoundExpression, speculativelyBoundExpression, null, binder);
	}

	public SymbolInfo GetSpeculativeSymbolInfo(int position, AttributeSyntax attribute)
	{
		BoundNode speculativelyBoundAttribute = GetSpeculativelyBoundAttribute(position, attribute, out var binder);
		if (speculativelyBoundAttribute == null)
		{
			return SymbolInfo.None;
		}
		return GetSymbolInfoForNode(SymbolInfoOptions.DefaultOptions, speculativelyBoundAttribute, speculativelyBoundAttribute, null, binder);
	}

	public SymbolInfo GetSpeculativeSymbolInfo(int position, ConstructorInitializerSyntax constructorInitializer)
	{
		position = CheckAndAdjustPosition(position);
		if (constructorInitializer == null)
		{
			throw new ArgumentNullException("constructorInitializer");
		}
		ConstructorInitializerSyntax constructorInitializerSyntax = Root.FindToken(position).Parent.AncestorsAndSelf().OfType<ConstructorInitializerSyntax>().FirstOrDefault();
		if (constructorInitializerSyntax == null)
		{
			return SymbolInfo.None;
		}
		MemberSemanticModel memberModel = GetMemberModel(constructorInitializerSyntax);
		if (memberModel == null)
		{
			return SymbolInfo.None;
		}
		Binder enclosingBinder = memberModel.GetEnclosingBinder(position);
		if (enclosingBinder != null)
		{
			enclosingBinder = new ExecutableCodeBinder(constructorInitializer, enclosingBinder.ContainingMemberOrLambda, enclosingBinder);
			BoundExpressionStatement bnode = enclosingBinder.BindConstructorInitializer(constructorInitializer, BindingDiagnosticBag.Discarded);
			return GetSymbolInfoFromBoundConstructorInitializer(memberModel, enclosingBinder, bnode);
		}
		return SymbolInfo.None;
	}

	private static SymbolInfo GetSymbolInfoFromBoundConstructorInitializer(MemberSemanticModel memberModel, Binder binder, BoundExpressionStatement bnode)
	{
		BoundExpression boundExpression;
		for (boundExpression = bnode.Expression; boundExpression is BoundSequence boundSequence; boundExpression = boundSequence.Value)
		{
		}
		return memberModel.GetSymbolInfoForNode(SymbolInfoOptions.DefaultOptions, boundExpression, boundExpression, null, binder);
	}

	public SymbolInfo GetSpeculativeSymbolInfo(int position, PrimaryConstructorBaseTypeSyntax constructorInitializer)
	{
		position = CheckAndAdjustPosition(position);
		if (constructorInitializer == null)
		{
			throw new ArgumentNullException("constructorInitializer");
		}
		PrimaryConstructorBaseTypeSyntax primaryConstructorBaseTypeSyntax = Root.FindToken(position).Parent.AncestorsAndSelf().OfType<PrimaryConstructorBaseTypeSyntax>().FirstOrDefault();
		if (primaryConstructorBaseTypeSyntax == null)
		{
			return SymbolInfo.None;
		}
		MemberSemanticModel memberModel = GetMemberModel(primaryConstructorBaseTypeSyntax);
		if (memberModel == null)
		{
			return SymbolInfo.None;
		}
		ArgumentListSyntax argumentList = primaryConstructorBaseTypeSyntax.ArgumentList;
		Binder enclosingBinder = memberModel.GetEnclosingBinder(LookupPosition.IsBetweenTokens(position, argumentList.OpenParenToken, argumentList.CloseParenToken) ? position : argumentList.OpenParenToken.SpanStart);
		if (enclosingBinder != null)
		{
			enclosingBinder = new ExecutableCodeBinder(constructorInitializer, enclosingBinder.ContainingMemberOrLambda, enclosingBinder);
			BoundExpressionStatement bnode = enclosingBinder.BindConstructorInitializer(constructorInitializer, BindingDiagnosticBag.Discarded);
			return GetSymbolInfoFromBoundConstructorInitializer(memberModel, enclosingBinder, bnode);
		}
		return SymbolInfo.None;
	}

	public SymbolInfo GetSpeculativeSymbolInfo(int position, CrefSyntax cref, SymbolInfoOptions options = SymbolInfoOptions.DefaultOptions)
	{
		position = CheckAndAdjustPosition(position);
		return GetCrefSymbolInfo(position, cref, options, HasParameterList(cref));
	}

	public TypeInfo GetTypeInfo(ConstructorInitializerSyntax constructorInitializer, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(constructorInitializer);
		return CanGetSemanticInfo(constructorInitializer) ? GetTypeInfoWorker(constructorInitializer, cancellationToken) : CSharpTypeInfo.None;
	}

	public abstract TypeInfo GetTypeInfo(SelectOrGroupClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken));

	public TypeInfo GetTypeInfo(PatternSyntax pattern, CancellationToken cancellationToken = default(CancellationToken))
	{
		while (pattern is ParenthesizedPatternSyntax parenthesizedPatternSyntax)
		{
			pattern = parenthesizedPatternSyntax.Pattern;
		}
		CheckSyntaxNode(pattern);
		return GetTypeInfoWorker(pattern, cancellationToken);
	}

	public TypeInfo GetTypeInfo(ExpressionSyntax expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(expression);
		if (!CanGetSemanticInfo(expression))
		{
			return CSharpTypeInfo.None;
		}
		if (SyntaxFacts.IsDeclarationExpressionType(expression, out DeclarationExpressionSyntax parent))
		{
			switch (parent.Designation.Kind())
			{
			case SyntaxKind.SingleVariableDesignation:
			{
				(ITypeSymbol Type, Microsoft.CodeAnalysis.NullableAnnotation Annotation) tuple = TypeFromVariable((SingleVariableDesignationSyntax)parent.Designation, cancellationToken);
				ITypeSymbol item = tuple.Type;
				Microsoft.CodeAnalysis.NullableAnnotation item2 = tuple.Annotation;
				Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol symbol = item.GetSymbol();
				NullabilityInfo nullabilityInfo = item2.ToNullabilityInfo(symbol);
				return new CSharpTypeInfo(symbol, symbol, nullabilityInfo, nullabilityInfo, Conversion.Identity);
			}
			case SyntaxKind.DiscardDesignation:
			{
				CSharpTypeInfo typeInfoWorker2 = GetTypeInfoWorker(parent, cancellationToken);
				return new CSharpTypeInfo(typeInfoWorker2.Type, typeInfoWorker2.Type, typeInfoWorker2.Nullability, typeInfoWorker2.Nullability, Conversion.Identity);
			}
			case SyntaxKind.ParenthesizedVariableDesignation:
				if (((TypeSyntax)expression).IsVar)
				{
					CSharpTypeInfo typeInfoWorker = GetTypeInfoWorker(expression, cancellationToken);
					Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type = typeInfoWorker.Type;
					if ((object)type != null && type.TypeKind != TypeKind.Error)
					{
						return typeInfoWorker;
					}
					return GetTypeInfoWorker(parent, cancellationToken);
				}
				break;
			}
		}
		return GetTypeInfoWorker(expression, cancellationToken);
	}

	public TypeInfo GetTypeInfo(AttributeSyntax attributeSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(attributeSyntax);
		return CanGetSemanticInfo(attributeSyntax) ? GetTypeInfoWorker(attributeSyntax, cancellationToken) : CSharpTypeInfo.None;
	}

	public Conversion GetConversion(SyntaxNode expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		CSharpSyntaxNode cSharpSyntaxNode = (CSharpSyntaxNode)expression;
		CheckSyntaxNode(cSharpSyntaxNode);
		CSharpTypeInfo obj = (CanGetSemanticInfo(cSharpSyntaxNode) ? GetTypeInfoWorker(cSharpSyntaxNode, cancellationToken) : CSharpTypeInfo.None);
		return obj.ImplicitConversion;
	}

	public TypeInfo GetSpeculativeTypeInfo(int position, ExpressionSyntax expression, SpeculativeBindingOption bindingOption)
	{
		return GetSpeculativeTypeInfoWorker(position, expression, bindingOption);
	}

	internal CSharpTypeInfo GetSpeculativeTypeInfoWorker(int position, ExpressionSyntax expression, SpeculativeBindingOption bindingOption)
	{
		if (!CanGetSemanticInfo(expression, allowNamedArgumentName: false, isSpeculative: true))
		{
			return CSharpTypeInfo.None;
		}
		BoundNode speculativelyBoundExpression = GetSpeculativelyBoundExpression(position, expression, bindingOption, out var _, out var crefSymbols);
		if (speculativelyBoundExpression == null)
		{
			if (crefSymbols.IsDefault || crefSymbols.Length != 1)
			{
				return CSharpTypeInfo.None;
			}
			return GetTypeInfoForSymbol(crefSymbols[0]);
		}
		return GetTypeInfoForNode(speculativelyBoundExpression, speculativelyBoundExpression, null);
	}

	public Conversion GetSpeculativeConversion(int position, ExpressionSyntax expression, SpeculativeBindingOption bindingOption)
	{
		return GetSpeculativeTypeInfoWorker(position, expression, bindingOption).ImplicitConversion;
	}

	public ImmutableArray<ISymbol> GetMemberGroup(ExpressionSyntax expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(expression);
		if (!CanGetSemanticInfo(expression))
		{
			return ImmutableArray<ISymbol>.Empty;
		}
		return GetMemberGroupWorker(expression, SymbolInfoOptions.DefaultOptions, cancellationToken).GetPublicSymbols();
	}

	public ImmutableArray<ISymbol> GetMemberGroup(AttributeSyntax attribute, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(attribute);
		if (!CanGetSemanticInfo(attribute))
		{
			return ImmutableArray<ISymbol>.Empty;
		}
		return GetMemberGroupWorker(attribute, SymbolInfoOptions.DefaultOptions, cancellationToken).GetPublicSymbols();
	}

	public ImmutableArray<ISymbol> GetMemberGroup(ConstructorInitializerSyntax initializer, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(initializer);
		if (!CanGetSemanticInfo(initializer))
		{
			return ImmutableArray<ISymbol>.Empty;
		}
		return GetMemberGroupWorker(initializer, SymbolInfoOptions.DefaultOptions, cancellationToken).GetPublicSymbols();
	}

	public ImmutableArray<IPropertySymbol> GetIndexerGroup(ExpressionSyntax expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(expression);
		if (!CanGetSemanticInfo(expression))
		{
			return ImmutableArray<IPropertySymbol>.Empty;
		}
		return GetIndexerGroupWorker(expression, SymbolInfoOptions.DefaultOptions, cancellationToken);
	}

	public Optional<object> GetConstantValue(ExpressionSyntax expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(expression);
		if (!CanGetSemanticInfo(expression))
		{
			return default(Optional<object>);
		}
		return GetConstantValueWorker(expression, cancellationToken);
	}

	public abstract QueryClauseInfo GetQueryClauseInfo(QueryClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken));

	public IAliasSymbol GetAliasInfo(IdentifierNameSyntax nameSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(nameSyntax);
		if (!CanGetSemanticInfo(nameSyntax))
		{
			return null;
		}
		return GetSymbolInfoWorker(nameSyntax, SymbolInfoOptions.PreferTypeToConstructors | SymbolInfoOptions.PreserveAliases, cancellationToken).Symbol as IAliasSymbol;
	}

	public IAliasSymbol GetSpeculativeAliasInfo(int position, IdentifierNameSyntax nameSyntax, SpeculativeBindingOption bindingOption)
	{
		BoundNode speculativelyBoundExpression = GetSpeculativelyBoundExpression(position, nameSyntax, bindingOption, out var binder, out var crefSymbols);
		if (speculativelyBoundExpression == null)
		{
			if (crefSymbols.IsDefault || crefSymbols.Length != 1)
			{
				return null;
			}
			return (crefSymbols[0] as Microsoft.CodeAnalysis.CSharp.Symbols.AliasSymbol).GetPublicSymbol();
		}
		return GetSymbolInfoForNode(SymbolInfoOptions.PreferTypeToConstructors | SymbolInfoOptions.PreserveAliases, speculativelyBoundExpression, speculativelyBoundExpression, null, binder).Symbol as IAliasSymbol;
	}

	internal Binder GetEnclosingBinder(int position)
	{
		return GetEnclosingBinderInternal(position);
	}

	internal abstract Binder GetEnclosingBinderInternal(int position);

	internal abstract MemberSemanticModel GetMemberModel(SyntaxNode node);

	internal bool IsInTree(SyntaxNode node)
	{
		return node.SyntaxTree == SyntaxTree;
	}

	private static bool IsInStructuredTriviaOtherThanCrefOrNameAttribute(CSharpSyntaxNode node)
	{
		while (node != null)
		{
			if (node.Kind() == SyntaxKind.XmlCrefAttribute || node.Kind() == SyntaxKind.XmlNameAttribute)
			{
				return false;
			}
			if (node.IsStructuredTrivia)
			{
				return true;
			}
			node = node.ParentOrStructuredTriviaParent;
		}
		return false;
	}

	protected int CheckAndAdjustPosition(int position)
	{
		SyntaxToken token;
		return CheckAndAdjustPosition(position, out token);
	}

	protected int CheckAndAdjustPosition(int position, out SyntaxToken token)
	{
		int position2 = Root.Position;
		int end = Root.FullSpan.End;
		bool flag = position == end && position == SyntaxTree.GetRoot().FullSpan.End;
		if ((position2 <= position && position < end) | flag)
		{
			token = (flag ? ((CSharpSyntaxNode)SyntaxTree.GetRoot()) : Root).FindTokenIncludingCrefAndNameAttributes(position);
			if (position < token.SpanStart)
			{
				token = token.GetPreviousToken();
			}
			return Math.Max(token.SpanStart, position2);
		}
		if (position2 == end && position == end)
		{
			token = default(SyntaxToken);
			return position2;
		}
		throw new ArgumentOutOfRangeException("position", position, string.Format(CSharpResources.PositionIsNotWithinSyntax, Root.FullSpan));
	}

	protected int GetAdjustedNodePosition(SyntaxNode node)
	{
		TextSpan fullSpan = Root.FullSpan;
		int num = node.SpanStart;
		SyntaxToken firstToken = node.GetFirstToken();
		if (firstToken.Node != null)
		{
			int spanStart = firstToken.SpanStart;
			if (spanStart < node.Span.End)
			{
				num = spanStart;
			}
		}
		if (fullSpan.IsEmpty)
		{
			return num;
		}
		if (num == fullSpan.End)
		{
			return CheckAndAdjustPosition(num - 1);
		}
		if (node.IsMissing || node.HasErrors || node.Width == 0 || node.IsPartOfStructuredTrivia())
		{
			return CheckAndAdjustPosition(num);
		}
		return num;
	}

	[Conditional("DEBUG")]
	protected void AssertPositionAdjusted(int position)
	{
	}

	protected void CheckSyntaxNode(CSharpSyntaxNode syntax)
	{
		if (syntax == null)
		{
			throw new ArgumentNullException("syntax");
		}
		if (!IsInTree(syntax))
		{
			throw new ArgumentException(CSharpResources.SyntaxNodeIsNotWithinSynt);
		}
	}

	private void CheckModelAndSyntaxNodeToSpeculate(CSharpSyntaxNode syntax)
	{
		if (syntax == null)
		{
			throw new ArgumentNullException("syntax");
		}
		if (IsSpeculativeSemanticModel)
		{
			throw new InvalidOperationException(CSharpResources.ChainingSpeculativeModelIsNotSupported);
		}
		if (Compilation.ContainsSyntaxTree(syntax.SyntaxTree))
		{
			throw new ArgumentException(CSharpResources.SpeculatedSyntaxNodeCannotBelongToCurrentCompilation);
		}
	}

	public ImmutableArray<ISymbol> LookupSymbols(int position, Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol container = null, string name = null, bool includeExtensions = false)
	{
		return LookupSymbolsInternal(position, container, name, LookupOptions.Default, includeExtensions, useBaseReferenceAccessibility: false);
	}

	public new ImmutableArray<ISymbol> LookupBaseMembers(int position, string name = null)
	{
		return LookupSymbolsInternal(position, null, name, LookupOptions.Default, includeExtensionMembers: false, useBaseReferenceAccessibility: true);
	}

	public ImmutableArray<ISymbol> LookupStaticMembers(int position, Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol container = null, string name = null)
	{
		return LookupSymbolsInternal(position, container, name, LookupOptions.MustNotBeInstance, includeExtensionMembers: false, useBaseReferenceAccessibility: false);
	}

	public ImmutableArray<ISymbol> LookupNamespacesAndTypes(int position, Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol container = null, string name = null)
	{
		return LookupSymbolsInternal(position, container, name, LookupOptions.NamespacesOrTypesOnly, includeExtensionMembers: false, useBaseReferenceAccessibility: false);
	}

	public new ImmutableArray<ISymbol> LookupLabels(int position, string name = null)
	{
		return LookupSymbolsInternal(position, null, name, LookupOptions.LabelsOnly, includeExtensionMembers: false, useBaseReferenceAccessibility: false);
	}

	private ImmutableArray<ISymbol> LookupSymbolsInternal(int position, Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol container, string name, LookupOptions options, bool includeExtensionMembers, bool useBaseReferenceAccessibility)
	{
		if (useBaseReferenceAccessibility)
		{
			options |= LookupOptions.UseBaseReferenceAccessibility;
		}
		options.ThrowIfInvalid();
		position = CheckAndAdjustPosition(position, out var token);
		if ((object)container == null || container.Kind == SymbolKind.Namespace)
		{
			includeExtensionMembers = false;
		}
		Binder enclosingBinder = GetEnclosingBinder(position);
		if (enclosingBinder == null)
		{
			return ImmutableArray<ISymbol>.Empty;
		}
		if (useBaseReferenceAccessibility)
		{
			Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol containingType = enclosingBinder.ContainingType;
			Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol typeSymbol = null;
			if ((object)containingType != null && containingType.Kind == SymbolKind.NamedType && ((Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol)containingType).IsScriptClass)
			{
				return ImmutableArray<ISymbol>.Empty;
			}
			if ((object)containingType == null || (object)(typeSymbol = containingType.BaseTypeNoUseSiteDiagnostics) == null)
			{
				throw new ArgumentException("Not a valid position for a call to LookupBaseMembers (must be in a type with a base type)", "position");
			}
			container = typeSymbol;
		}
		if (!enclosingBinder.IsInMethodBody && (options & (LookupOptions.NamespaceAliasesOnly | LookupOptions.NamespacesOrTypesOnly | LookupOptions.LabelsOnly)) == 0 && token.Parent is ExpressionSyntax expressionSyntax && !(expressionSyntax.Parent is XmlNameAttributeSyntax) && !SyntaxFacts.IsInTypeOnlyContext(expressionSyntax) && !enclosingBinder.IsInsideNameof)
		{
			options |= LookupOptions.MustNotBeMethodTypeParameter;
		}
		LookupSymbolsInfo instance = LookupSymbolsInfo.GetInstance();
		instance.FilterName = name;
		if ((object)container == null)
		{
			enclosingBinder.AddLookupSymbolsInfo(instance, options);
		}
		else
		{
			enclosingBinder.AddMemberLookupSymbolsInfo(instance, container, options, enclosingBinder);
		}
		ArrayBuilder<ISymbol> instance2 = ArrayBuilder<ISymbol>.GetInstance(instance.Count);
		if (name == null)
		{
			foreach (string name2 in instance.Names)
			{
				AppendSymbolsWithName(instance2, name2, enclosingBinder, container, options, instance);
			}
		}
		else
		{
			AppendSymbolsWithName(instance2, name, enclosingBinder, container, options, instance);
		}
		instance.Free();
		if (includeExtensionMembers && container is Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol receiverType)
		{
			LookupResult instance3 = LookupResult.GetInstance();
			options |= LookupOptions.AllMethodsOnArityZero;
			options &= ~LookupOptions.MustBeInstance;
			enclosingBinder.LookupAllExtensions(instance3, name, options);
			if (instance3.IsMultiViable)
			{
				foreach (Symbol symbol2 in instance3.Symbols)
				{
					if (symbol2 is Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol { IsExtensionMethod: not false } methodSymbol)
					{
						Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol methodSymbol2 = methodSymbol.ReduceExtensionMethod(receiverType, Compilation);
						if ((object)methodSymbol2 != null)
						{
							instance2.Add(methodSymbol2.GetPublicSymbol());
						}
					}
					else
					{
						Symbol symbol = SourceNamedTypeSymbol.ReduceExtensionMember(enclosingBinder.Compilation, symbol2, receiverType, out var _);
						if ((object)symbol != null)
						{
							instance2.Add(symbol.GetPublicSymbol());
						}
					}
				}
			}
			instance3.Free();
		}
		if (name == null)
		{
			instance2.RemoveAll((ISymbol symbol2, int _) => !symbol2.CanBeReferencedByName, 0);
		}
		return instance2.ToImmutableAndFree();
	}

	private void AppendSymbolsWithName(ArrayBuilder<ISymbol> results, string name, Binder binder, Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol container, LookupOptions options, LookupSymbolsInfo info)
	{
		if (!info.TryGetAritiesAndUniqueSymbol(name, out AbstractLookupSymbolsInfo<Symbol>.IArityEnumerable arities, out Symbol uniqueSymbol))
		{
			return;
		}
		if ((object)uniqueSymbol != null)
		{
			results.Add(RemapSymbolIfNecessary(uniqueSymbol).GetPublicSymbol());
			return;
		}
		if (arities != null)
		{
			foreach (int item in arities)
			{
				AppendSymbolsWithNameAndArity(results, name, item, binder, container, options);
			}
			return;
		}
		AppendSymbolsWithNameAndArity(results, name, 0, binder, container, options);
	}

	private void AppendSymbolsWithNameAndArity(ArrayBuilder<ISymbol> results, string name, int arity, Binder binder, Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol container, LookupOptions options)
	{
		LookupResult instance = LookupResult.GetInstance();
		CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.Discarded;
		binder.LookupSymbolsSimpleName(instance, container, name, arity, null, options, diagnose: false, ref useSiteInfo);
		if (instance.IsMultiViable)
		{
			if (instance.Symbols.Any((Symbol t) => t.Kind == SymbolKind.NamedType || t.Kind == SymbolKind.Namespace || t.Kind == SymbolKind.ErrorType))
			{
				Symbol symbol = binder.ResultSymbol(instance, name, arity, Root, BindingDiagnosticBag.Discarded, suppressUseSiteDiagnostics: true, out var wasError, container, options);
				if (!wasError)
				{
					results.Add(RemapSymbolIfNecessary(symbol).GetPublicSymbol());
				}
				else
				{
					foreach (Symbol symbol2 in instance.Symbols)
					{
						results.Add(RemapSymbolIfNecessary(symbol2).GetPublicSymbol());
					}
				}
			}
			else
			{
				foreach (Symbol symbol3 in instance.Symbols)
				{
					results.Add(RemapSymbolIfNecessary(symbol3).GetPublicSymbol());
				}
			}
		}
		instance.Free();
	}

	private Symbol RemapSymbolIfNecessary(Symbol symbol)
	{
		if (symbol is Microsoft.CodeAnalysis.CSharp.Symbols.LocalSymbol || symbol is Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol || symbol is Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol { MethodKind: MethodKind.AnonymousFunction })
		{
			return RemapSymbolIfNecessaryCore(symbol);
		}
		return symbol;
	}

	internal abstract Symbol RemapSymbolIfNecessaryCore(Symbol symbol);

	public bool IsAccessible(int position, Symbol symbol)
	{
		position = CheckAndAdjustPosition(position);
		if ((object)symbol == null)
		{
			throw new ArgumentNullException("symbol");
		}
		Binder enclosingBinder = GetEnclosingBinder(position);
		if (enclosingBinder != null)
		{
			CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.Discarded;
			return enclosingBinder.IsAccessible(symbol, ref useSiteInfo);
		}
		return false;
	}

	public bool IsEventUsableAsField(int position, Microsoft.CodeAnalysis.CSharp.Symbols.EventSymbol symbol)
	{
		if ((object)symbol != null && symbol.HasAssociatedField)
		{
			return IsAccessible(position, symbol.AssociatedField);
		}
		return false;
	}

	private bool IsInTypeofExpression(int position)
	{
		for (SyntaxNode syntaxNode = Root.FindToken(position).Parent; syntaxNode != Root; syntaxNode = syntaxNode.ParentOrStructuredTriviaParent)
		{
			if (syntaxNode.IsKind(SyntaxKind.TypeOfExpression))
			{
				return true;
			}
		}
		return false;
	}

	internal SymbolInfo GetSymbolInfoForNode(SymbolInfoOptions options, BoundNode lowestBoundNode, BoundNode highestBoundNode, BoundNode boundNodeForSyntacticParent, Binder binderOpt)
	{
		if (highestBoundNode is BoundRecursivePattern pat)
		{
			return GetSymbolInfoForDeconstruction(pat);
		}
		if (!(lowestBoundNode is BoundPositionalSubpattern boundPositionalSubpattern))
		{
			if (!(lowestBoundNode is BoundPropertySubpattern boundPropertySubpattern))
			{
				if (!(lowestBoundNode is BoundPropertySubpatternMember boundPropertySubpatternMember))
				{
					if (lowestBoundNode is BoundExpression boundExpression)
					{
						BoundExpression boundExpression2 = boundExpression;
						OneOrMany<Symbol> oneOrMany = GetSemanticSymbols(boundExpression2, boundNodeForSyntacticParent, binderOpt, options, out var isDynamic, out var resultKind, out var _);
						if (highestBoundNode is BoundBadExpression boundNode)
						{
							GetSemanticSymbols(boundNode, boundNodeForSyntacticParent, binderOpt, options, out var isDynamic2, out var resultKind2, out var _);
							if (resultKind2 != LookupResultKind.Empty && (int)resultKind2 < (int)resultKind)
							{
								resultKind = resultKind2;
								isDynamic = isDynamic2;
							}
						}
						else if (boundExpression2 is BoundMethodGroup && resultKind == LookupResultKind.OverloadResolutionFailure && highestBoundNode is BoundConversion { ConversionKind: ConversionKind.MethodGroup } boundConversion)
						{
							OneOrMany<Symbol> semanticSymbols = GetSemanticSymbols(boundConversion, boundNodeForSyntacticParent, binderOpt, options, out var isDynamic3, out var resultKind3, out var _);
							if (semanticSymbols.Count > 0)
							{
								oneOrMany = semanticSymbols;
								resultKind = resultKind3;
								isDynamic = isDynamic3;
							}
						}
						if (resultKind == LookupResultKind.Empty)
						{
							return SymbolInfoFactory.Create(ImmutableArray<Symbol>.Empty, LookupResultKind.Empty, isDynamic);
						}
						ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(oneOrMany.Count);
						foreach (Symbol item in oneOrMany)
						{
							AddUnwrappingErrorTypes(instance, item);
						}
						oneOrMany = instance.ToOneOrManyAndFree();
						if ((options & SymbolInfoOptions.ResolveAliases) != 0)
						{
							oneOrMany = UnwrapAliases(oneOrMany);
						}
						if (resultKind == LookupResultKind.Viable && oneOrMany.Count > 1)
						{
							resultKind = LookupResultKind.OverloadResolutionFailure;
						}
						return SymbolInfoFactory.Create(oneOrMany, resultKind, isDynamic);
					}
					return SymbolInfo.None;
				}
				return GetSymbolInfoForSubpattern(boundPropertySubpatternMember.Symbol);
			}
			return GetSymbolInfoForSubpattern(boundPropertySubpattern.Member?.Symbol);
		}
		return GetSymbolInfoForSubpattern(boundPositionalSubpattern.Symbol);
	}

	private static SymbolInfo GetSymbolInfoForSubpattern(Symbol subpatternSymbol)
	{
		if (subpatternSymbol?.OriginalDefinition is Microsoft.CodeAnalysis.CSharp.Symbols.ErrorTypeSymbol errorTypeSymbol)
		{
			return new SymbolInfo(errorTypeSymbol.CandidateSymbols.GetPublicSymbols(), errorTypeSymbol.ResultKind.ToCandidateReason());
		}
		return new SymbolInfo(subpatternSymbol.GetPublicSymbol());
	}

	private SymbolInfo GetSymbolInfoForDeconstruction(BoundRecursivePattern pat)
	{
		return new SymbolInfo(pat.DeconstructMethod.GetPublicSymbol());
	}

	private static void AddUnwrappingErrorTypes(ArrayBuilder<Symbol> builder, Symbol s)
	{
		if (s.OriginalDefinition is Microsoft.CodeAnalysis.CSharp.Symbols.ErrorTypeSymbol errorTypeSymbol)
		{
			builder.AddRange(errorTypeSymbol.CandidateSymbols);
		}
		else
		{
			builder.Add(s);
		}
	}

	internal CSharpTypeInfo GetTypeInfoForNode(BoundNode lowestBoundNode, BoundNode highestBoundNode, BoundNode boundNodeForSyntacticParent)
	{
		BoundPattern boundPattern = (lowestBoundNode as BoundPattern) ?? (highestBoundNode as BoundPattern) ?? ((highestBoundNode is BoundSubpattern boundSubpattern) ? boundSubpattern.Pattern : null);
		if (boundPattern != null)
		{
			CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.Discarded;
			return new CSharpTypeInfo(boundPattern.InputType, boundPattern.NarrowedType, default(NullabilityInfo), default(NullabilityInfo), Compilation.Conversions.ClassifyBuiltInConversion(boundPattern.InputType, boundPattern.NarrowedType, isChecked: false, ref useSiteInfo));
		}
		if (lowestBoundNode is BoundPropertySubpatternMember boundPropertySubpatternMember)
		{
			return new CSharpTypeInfo(boundPropertySubpatternMember.Type, boundPropertySubpatternMember.Type, default(NullabilityInfo), default(NullabilityInfo), Conversion.Identity);
		}
		BoundExpression boundExpression = lowestBoundNode as BoundExpression;
		BoundExpression boundExpression2 = highestBoundNode as BoundExpression;
		Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol typeSymbol;
		NullabilityInfo nullabilityInfo;
		Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol typeSymbol2;
		NullabilityInfo convertedNullability;
		Conversion implicitConversion;
		if (boundExpression != null)
		{
			typeSymbol = null;
			nullabilityInfo = boundExpression.TopLevelNullability;
			if (boundExpression.HasExpressionType())
			{
				typeSymbol = boundExpression.Type;
				if (!(boundExpression is BoundLocal boundLocal))
				{
					if (boundExpression is BoundConvertedTupleLiteral boundConvertedTupleLiteral)
					{
						BoundTupleLiteral sourceTuple = boundConvertedTupleLiteral.SourceTuple;
						if (sourceTuple != null)
						{
							typeSymbol = sourceTuple.Type;
						}
					}
				}
				else if (typeSymbol is ExtendedErrorTypeSymbol { VariableUsedBeforeDeclaration: not false })
				{
					typeSymbol = boundLocal.LocalSymbol.Type;
					nullabilityInfo = boundLocal.LocalSymbol.TypeWithAnnotations.NullableAnnotation.ToNullabilityInfo(typeSymbol);
				}
			}
			BoundKind boundKind = boundExpression2?.Kind ?? BoundKind.NoOpStatement;
			if (boundKind == BoundKind.Lambda)
			{
				BoundLambda boundLambda = (BoundLambda)boundExpression2;
				typeSymbol2 = boundLambda.Type;
				typeSymbol = null;
				nullabilityInfo = default(NullabilityInfo);
				convertedNullability = new NullabilityInfo(Microsoft.CodeAnalysis.NullableAnnotation.NotAnnotated, Microsoft.CodeAnalysis.NullableFlowState.NotNull);
				implicitConversion = new Conversion(ConversionKind.AnonymousFunction, boundLambda.Symbol, isExtensionMethod: false);
			}
			else
			{
				BoundConversion obj = boundExpression2 as BoundConversion;
				if (obj != null && obj.Conversion.IsTupleLiteralConversion)
				{
					BoundConversion boundConversion = (BoundConversion)boundExpression2;
					if (boundConversion.Operand.Kind != BoundKind.ConvertedTupleLiteral)
					{
						(typeSymbol, nullabilityInfo) = getTypeAndNullability(boundConversion.Operand);
					}
					else
					{
						BoundConvertedTupleLiteral obj2 = (BoundConvertedTupleLiteral)boundConversion.Operand;
						typeSymbol = obj2.SourceTuple.Type;
						nullabilityInfo = obj2.TopLevelNullability;
					}
					(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol, NullabilityInfo) tuple2 = getTypeAndNullability(boundConversion);
					typeSymbol2 = tuple2.Item1;
					convertedNullability = tuple2.Item2;
					implicitConversion = boundConversion.Conversion;
				}
				else if (boundKind == BoundKind.FixedLocalCollectionInitializer)
				{
					BoundFixedLocalCollectionInitializer boundFixedLocalCollectionInitializer = (BoundFixedLocalCollectionInitializer)boundExpression2;
					(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol, NullabilityInfo) tuple3 = getTypeAndNullability(boundFixedLocalCollectionInitializer);
					typeSymbol2 = tuple3.Item1;
					convertedNullability = tuple3.Item2;
					(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol, NullabilityInfo) tuple4 = getTypeAndNullability(boundFixedLocalCollectionInitializer.Expression);
					typeSymbol = tuple4.Item1;
					nullabilityInfo = tuple4.Item2;
					implicitConversion = BoundNode.GetConversion(boundFixedLocalCollectionInitializer.ElementPointerConversion, boundFixedLocalCollectionInitializer.ElementPointerPlaceholder);
				}
				else if (boundExpression is BoundConvertedSwitchExpression { WasTargetTyped: not false } boundConvertedSwitchExpression)
				{
					if (boundExpression2 is BoundConversion { ConversionKind: ConversionKind.SwitchExpression, Conversion: var conversion })
					{
						typeSymbol = boundConvertedSwitchExpression.NaturalTypeOpt;
						typeSymbol2 = boundConvertedSwitchExpression.Type;
						convertedNullability = boundConvertedSwitchExpression.TopLevelNullability;
						implicitConversion = (conversion.IsValid ? conversion : Conversion.NoConversion);
					}
					else
					{
						typeSymbol = boundConvertedSwitchExpression.NaturalTypeOpt;
						Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol typeSymbol3 = typeSymbol;
						convertedNullability = nullabilityInfo;
						typeSymbol2 = typeSymbol3;
						implicitConversion = Conversion.Identity;
					}
				}
				else if (boundExpression is BoundConditionalOperator { WasTargetTyped: not false } boundConditionalOperator)
				{
					if (boundExpression2 is BoundConversion { ConversionKind: ConversionKind.ConditionalExpression })
					{
						typeSymbol = boundConditionalOperator.NaturalTypeOpt;
						typeSymbol2 = boundConditionalOperator.Type;
						convertedNullability = nullabilityInfo;
						implicitConversion = Conversion.MakeConditionalExpression(ImmutableArray<Conversion>.Empty);
					}
					else
					{
						typeSymbol = boundConditionalOperator.NaturalTypeOpt;
						Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol typeSymbol4 = typeSymbol;
						convertedNullability = nullabilityInfo;
						typeSymbol2 = typeSymbol4;
						implicitConversion = Conversion.Identity;
					}
				}
				else if (boundExpression is BoundCollectionExpression boundCollectionExpression)
				{
					typeSymbol = null;
					Conversion conversion2 = default(Conversion);
					bool flag;
					if (boundExpression2 is BoundConversion { ConversionKind: var conversionKind } boundConversion4 && (conversionKind == ConversionKind.NoConversion || conversionKind == ConversionKind.CollectionExpression))
					{
						conversion2 = boundConversion4.Conversion;
						flag = true;
					}
					else
					{
						flag = false;
					}
					if (flag)
					{
						typeSymbol2 = boundExpression2.Type;
						convertedNullability = boundCollectionExpression.TopLevelNullability;
						implicitConversion = conversion2;
					}
					else if (boundExpression2 is BoundConversion { ConversionKind: ConversionKind.ImplicitNullable, Conversion: { UnderlyingConversions: { Length: 1 } underlyingConversions } } boundConversion5 && underlyingConversions[0].Kind == ConversionKind.CollectionExpression)
					{
						typeSymbol2 = boundExpression2.Type;
						convertedNullability = boundCollectionExpression.TopLevelNullability;
						implicitConversion = boundConversion5.Conversion;
					}
					else
					{
						convertedNullability = nullabilityInfo;
						typeSymbol2 = null;
						implicitConversion = Conversion.Identity;
					}
				}
				else if (boundExpression2 != null && boundExpression2 != boundExpression && boundExpression2.HasExpressionType())
				{
					(typeSymbol2, convertedNullability) = getTypeAndNullability(boundExpression2);
					if (boundKind != BoundKind.Conversion)
					{
						implicitConversion = Conversion.Identity;
					}
					else if (((BoundConversion)boundExpression2).Operand.Kind != BoundKind.Conversion)
					{
						implicitConversion = boundExpression2.GetConversion();
						if (implicitConversion.Kind == ConversionKind.AnonymousFunction)
						{
							typeSymbol = null;
							nullabilityInfo = default(NullabilityInfo);
						}
					}
					else
					{
						Binder enclosingBinder = GetEnclosingBinder(boundExpression.Syntax.Span.Start);
						CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> useSiteInfo2 = CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.Discarded;
						implicitConversion = enclosingBinder.Conversions.ClassifyConversionFromExpression(boundExpression, typeSymbol2, ((BoundConversion)boundExpression2).Checked, ref useSiteInfo2);
					}
				}
				else if (boundNodeForSyntacticParent != null && boundNodeForSyntacticParent.Kind == BoundKind.DelegateCreationExpression)
				{
					BoundDelegateCreationExpression boundDelegateCreationExpression = (BoundDelegateCreationExpression)boundNodeForSyntacticParent;
					(typeSymbol2, convertedNullability) = getTypeAndNullability(boundDelegateCreationExpression);
					switch (boundExpression.Kind)
					{
					case BoundKind.MethodGroup:
						implicitConversion = new Conversion(ConversionKind.MethodGroup, boundDelegateCreationExpression.MethodOpt, boundDelegateCreationExpression.IsExtensionMethod);
						break;
					case BoundKind.Lambda:
					{
						BoundLambda boundLambda3 = (BoundLambda)boundExpression;
						implicitConversion = new Conversion(ConversionKind.AnonymousFunction, boundLambda3.Symbol, boundDelegateCreationExpression.IsExtensionMethod);
						break;
					}
					case BoundKind.UnboundLambda:
					{
						BoundLambda boundLambda2 = ((UnboundLambda)boundExpression).BindForErrorRecovery();
						implicitConversion = new Conversion(ConversionKind.AnonymousFunction, boundLambda2.Symbol, boundDelegateCreationExpression.IsExtensionMethod);
						break;
					}
					default:
						implicitConversion = Conversion.Identity;
						break;
					}
				}
				else
				{
					if (boundExpression is BoundConversion { ConversionKind: ConversionKind.MethodGroup, Conversion: var conversion4 } boundConversion6)
					{
						Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type = boundConversion6.Type;
						if ((object)type != null && type.TypeKind == TypeKind.FunctionPointer)
						{
							_ = boundConversion6.SymbolOpt;
							typeSymbol2 = typeSymbol;
							convertedNullability = nullabilityInfo;
							implicitConversion = conversion4;
							typeSymbol = null;
							nullabilityInfo = new NullabilityInfo(Microsoft.CodeAnalysis.NullableAnnotation.NotAnnotated, Microsoft.CodeAnalysis.NullableFlowState.NotNull);
							goto IL_0666;
						}
					}
					typeSymbol2 = typeSymbol;
					convertedNullability = nullabilityInfo;
					implicitConversion = Conversion.Identity;
				}
			}
			goto IL_0666;
		}
		return CSharpTypeInfo.None;
		IL_0666:
		return new CSharpTypeInfo(typeSymbol, typeSymbol2, nullabilityInfo, convertedNullability, implicitConversion);
		static (Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol, NullabilityInfo) getTypeAndNullability(BoundExpression expr)
		{
			return (expr.Type, expr.TopLevelNullability);
		}
	}

	internal ImmutableArray<Symbol> GetMemberGroupForNode(SymbolInfoOptions options, BoundNode lowestBoundNode, BoundNode boundNodeForSyntacticParent, Binder binderOpt)
	{
		if (lowestBoundNode is BoundExpression boundNode)
		{
			GetSemanticSymbols(boundNode, boundNodeForSyntacticParent, binderOpt, options, out var _, out var _, out var memberGroup);
			return memberGroup;
		}
		return ImmutableArray<Symbol>.Empty;
	}

	internal ImmutableArray<IPropertySymbol> GetIndexerGroupForNode(BoundNode lowestBoundNode, Binder binderOpt)
	{
		if (lowestBoundNode is BoundExpression { Kind: not BoundKind.TypeExpression } boundExpression)
		{
			return GetIndexerGroupSemanticSymbols(boundExpression, binderOpt);
		}
		return ImmutableArray<IPropertySymbol>.Empty;
	}

	internal static SymbolInfo GetSymbolInfoForSymbol(Symbol symbol, SymbolInfoOptions options)
	{
		Symbol symbol2 = UnwrapAlias(symbol);
		Microsoft.CodeAnalysis.CSharp.Symbols.ErrorTypeSymbol errorTypeSymbol = ((symbol2 is Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol typeSymbol) ? (typeSymbol.OriginalDefinition as Microsoft.CodeAnalysis.CSharp.Symbols.ErrorTypeSymbol) : null);
		if ((object)errorTypeSymbol != null)
		{
			OneOrMany<Symbol> symbols = OneOrMany<Symbol>.Empty;
			LookupResultKind resultKind = errorTypeSymbol.ResultKind;
			if (resultKind != LookupResultKind.Empty)
			{
				symbols = OneOrMany.Create(errorTypeSymbol.CandidateSymbols);
			}
			if ((options & SymbolInfoOptions.ResolveAliases) != 0)
			{
				symbols = UnwrapAliases(symbols);
			}
			return SymbolInfoFactory.Create(symbols, resultKind, isDynamic: false);
		}
		return new SymbolInfo((((options & SymbolInfoOptions.ResolveAliases) != 0) ? symbol2 : symbol).GetPublicSymbol());
	}

	internal static CSharpTypeInfo GetTypeInfoForSymbol(Symbol symbol)
	{
		Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol obj = UnwrapAlias(symbol) as Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol;
		return new CSharpTypeInfo(obj, obj, default(NullabilityInfo), default(NullabilityInfo), Conversion.Identity);
	}

	protected static Symbol UnwrapAlias(Symbol symbol)
	{
		if (!(symbol is Microsoft.CodeAnalysis.CSharp.Symbols.AliasSymbol aliasSymbol))
		{
			return symbol;
		}
		return aliasSymbol.Target;
	}

	protected static OneOrMany<Symbol> UnwrapAliases(OneOrMany<Symbol> symbols)
	{
		bool flag = false;
		foreach (Symbol item in symbols)
		{
			if (item.Kind == SymbolKind.Alias)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return symbols;
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		foreach (Symbol item2 in symbols)
		{
			AddUnwrappingErrorTypes(instance, UnwrapAlias(item2));
		}
		return instance.ToOneOrManyAndFree();
	}

	internal virtual BoundNode Bind(Binder binder, CSharpSyntaxNode node, BindingDiagnosticBag diagnostics)
	{
		if (Compilation.TestOnlyCompilationData is MemberSemanticModel.MemberSemanticBindingCounter memberSemanticBindingCounter)
		{
			memberSemanticBindingCounter.BindCount++;
		}
		if (!(node is ExpressionSyntax expressionSyntax))
		{
			if (!(node is StatementSyntax node2))
			{
				if (node is GlobalStatementSyntax globalStatementSyntax)
				{
					BoundStatement statement = binder.BindStatement(globalStatementSyntax.Statement, diagnostics);
					return new BoundGlobalStatementInitializer(node, statement);
				}
				return null;
			}
			return binder.BindStatement(node2, diagnostics);
		}
		if (!expressionSyntax.Parent.IsKind(SyntaxKind.GotoStatement))
		{
			return binder.BindNamespaceOrTypeOrExpression(expressionSyntax, diagnostics);
		}
		return binder.BindLabel(expressionSyntax, diagnostics);
	}

	public virtual ControlFlowAnalysis AnalyzeControlFlow(StatementSyntax firstStatement, StatementSyntax lastStatement)
	{
		throw new NotSupportedException();
	}

	public virtual ControlFlowAnalysis AnalyzeControlFlow(StatementSyntax statement)
	{
		return AnalyzeControlFlow(statement, statement);
	}

	public virtual DataFlowAnalysis AnalyzeDataFlow(ConstructorInitializerSyntax constructorInitializer)
	{
		throw new NotSupportedException();
	}

	public virtual DataFlowAnalysis AnalyzeDataFlow(PrimaryConstructorBaseTypeSyntax primaryConstructorBaseType)
	{
		throw new NotSupportedException();
	}

	public virtual DataFlowAnalysis AnalyzeDataFlow(ExpressionSyntax expression)
	{
		throw new NotSupportedException();
	}

	public virtual DataFlowAnalysis AnalyzeDataFlow(StatementSyntax firstStatement, StatementSyntax lastStatement)
	{
		throw new NotSupportedException();
	}

	public virtual DataFlowAnalysis AnalyzeDataFlow(StatementSyntax statement)
	{
		return AnalyzeDataFlow(statement, statement);
	}

	public bool TryGetSpeculativeSemanticModelForMethodBody(int position, BaseMethodDeclarationSyntax method, out SemanticModel speculativeModel)
	{
		CheckModelAndSyntaxNodeToSpeculate(method);
		bool result = TryGetSpeculativeSemanticModelForMethodBodyCore((SyntaxTreeSemanticModel)this, position, method, out var speculativeModel2);
		speculativeModel = speculativeModel2;
		return result;
	}

	internal abstract bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, BaseMethodDeclarationSyntax method, out PublicSemanticModel speculativeModel);

	public bool TryGetSpeculativeSemanticModelForMethodBody(int position, AccessorDeclarationSyntax accessor, out SemanticModel speculativeModel)
	{
		CheckModelAndSyntaxNodeToSpeculate(accessor);
		bool result = TryGetSpeculativeSemanticModelForMethodBodyCore((SyntaxTreeSemanticModel)this, position, accessor, out var speculativeModel2);
		speculativeModel = speculativeModel2;
		return result;
	}

	internal abstract bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, AccessorDeclarationSyntax accessor, out PublicSemanticModel speculativeModel);

	public bool TryGetSpeculativeSemanticModel(int position, TypeSyntax type, out SemanticModel speculativeModel, SpeculativeBindingOption bindingOption = SpeculativeBindingOption.BindAsExpression)
	{
		CheckModelAndSyntaxNodeToSpeculate(type);
		bool result = TryGetSpeculativeSemanticModelCore((SyntaxTreeSemanticModel)this, position, type, bindingOption, out var speculativeModel2);
		speculativeModel = speculativeModel2;
		return result;
	}

	internal abstract bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, TypeSyntax type, SpeculativeBindingOption bindingOption, out PublicSemanticModel speculativeModel);

	public bool TryGetSpeculativeSemanticModel(int position, StatementSyntax statement, out SemanticModel speculativeModel)
	{
		CheckModelAndSyntaxNodeToSpeculate(statement);
		bool result = TryGetSpeculativeSemanticModelCore((SyntaxTreeSemanticModel)this, position, statement, out var speculativeModel2);
		speculativeModel = speculativeModel2;
		return result;
	}

	internal abstract bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, StatementSyntax statement, out PublicSemanticModel speculativeModel);

	public bool TryGetSpeculativeSemanticModel(int position, EqualsValueClauseSyntax initializer, out SemanticModel speculativeModel)
	{
		CheckModelAndSyntaxNodeToSpeculate(initializer);
		bool result = TryGetSpeculativeSemanticModelCore((SyntaxTreeSemanticModel)this, position, initializer, out var speculativeModel2);
		speculativeModel = speculativeModel2;
		return result;
	}

	internal abstract bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, EqualsValueClauseSyntax initializer, out PublicSemanticModel speculativeModel);

	public bool TryGetSpeculativeSemanticModel(int position, ArrowExpressionClauseSyntax expressionBody, out SemanticModel speculativeModel)
	{
		CheckModelAndSyntaxNodeToSpeculate(expressionBody);
		bool result = TryGetSpeculativeSemanticModelCore((SyntaxTreeSemanticModel)this, position, expressionBody, out var speculativeModel2);
		speculativeModel = speculativeModel2;
		return result;
	}

	internal abstract bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, ArrowExpressionClauseSyntax expressionBody, out PublicSemanticModel speculativeModel);

	public bool TryGetSpeculativeSemanticModel(int position, ConstructorInitializerSyntax constructorInitializer, out SemanticModel speculativeModel)
	{
		CheckModelAndSyntaxNodeToSpeculate(constructorInitializer);
		bool result = TryGetSpeculativeSemanticModelCore((SyntaxTreeSemanticModel)this, position, constructorInitializer, out var speculativeModel2);
		speculativeModel = speculativeModel2;
		return result;
	}

	internal abstract bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, ConstructorInitializerSyntax constructorInitializer, out PublicSemanticModel speculativeModel);

	public bool TryGetSpeculativeSemanticModel(int position, PrimaryConstructorBaseTypeSyntax constructorInitializer, out SemanticModel speculativeModel)
	{
		CheckModelAndSyntaxNodeToSpeculate(constructorInitializer);
		bool result = TryGetSpeculativeSemanticModelCore((SyntaxTreeSemanticModel)this, position, constructorInitializer, out var speculativeModel2);
		speculativeModel = speculativeModel2;
		return result;
	}

	internal abstract bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, PrimaryConstructorBaseTypeSyntax constructorInitializer, out PublicSemanticModel speculativeModel);

	public bool TryGetSpeculativeSemanticModel(int position, CrefSyntax crefSyntax, out SemanticModel speculativeModel)
	{
		CheckModelAndSyntaxNodeToSpeculate(crefSyntax);
		bool result = TryGetSpeculativeSemanticModelCore((SyntaxTreeSemanticModel)this, position, crefSyntax, out var speculativeModel2);
		speculativeModel = speculativeModel2;
		return result;
	}

	internal abstract bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, CrefSyntax crefSyntax, out PublicSemanticModel speculativeModel);

	public bool TryGetSpeculativeSemanticModel(int position, AttributeSyntax attribute, out SemanticModel speculativeModel)
	{
		CheckModelAndSyntaxNodeToSpeculate(attribute);
		Binder speculativeBinderForAttribute = GetSpeculativeBinderForAttribute(position, attribute);
		if (speculativeBinderForAttribute == null)
		{
			speculativeModel = null;
			return false;
		}
		Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol attributeType = (Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol)speculativeBinderForAttribute.BindType(attribute.Name, BindingDiagnosticBag.Discarded, out var alias).Type;
		speculativeModel = ((SyntaxTreeSemanticModel)this).CreateSpeculativeAttributeSemanticModel(position, attribute, speculativeBinderForAttribute, alias, attributeType);
		return true;
	}

	public abstract Conversion ClassifyConversion(ExpressionSyntax expression, ITypeSymbol destination, bool isExplicitInSource = false);

	public Conversion ClassifyConversion(int position, ExpressionSyntax expression, ITypeSymbol destination, bool isExplicitInSource = false)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol typeSymbol = destination.EnsureCSharpSymbolOrNull("destination");
		if (expression.Kind() == SyntaxKind.DeclarationExpression)
		{
			return Conversion.NoConversion;
		}
		if (isExplicitInSource)
		{
			return ClassifyConversionForCast(position, expression, typeSymbol);
		}
		position = CheckAndAdjustPosition(position);
		Binder enclosingBinder = GetEnclosingBinder(position);
		if (enclosingBinder != null)
		{
			BoundExpression boundExpression = enclosingBinder.BindExpression(expression, BindingDiagnosticBag.Discarded);
			if (boundExpression != null && !typeSymbol.IsErrorType())
			{
				CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.Discarded;
				return enclosingBinder.Conversions.ClassifyConversionFromExpression(boundExpression, typeSymbol, enclosingBinder.CheckOverflowAtRuntime, ref useSiteInfo);
			}
		}
		return Conversion.NoConversion;
	}

	internal abstract Conversion ClassifyConversionForCast(ExpressionSyntax expression, Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol destination);

	internal Conversion ClassifyConversionForCast(int position, ExpressionSyntax expression, Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol destination)
	{
		if ((object)destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		position = CheckAndAdjustPosition(position);
		Binder enclosingBinder = GetEnclosingBinder(position);
		if (enclosingBinder != null)
		{
			BoundExpression boundExpression = enclosingBinder.BindExpression(expression, BindingDiagnosticBag.Discarded);
			if (boundExpression != null && !destination.IsErrorType())
			{
				CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.Discarded;
				return enclosingBinder.Conversions.ClassifyConversionFromExpression(boundExpression, destination, enclosingBinder.CheckOverflowAtRuntime, ref useSiteInfo, forCast: true);
			}
		}
		return Conversion.NoConversion;
	}

	public abstract ISymbol GetDeclaredSymbol(MemberDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IMethodSymbol GetDeclaredSymbol(LocalFunctionStatementSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IMethodSymbol GetDeclaredSymbol(CompilationUnitSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract INamespaceSymbol GetDeclaredSymbol(NamespaceDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract INamespaceSymbol GetDeclaredSymbol(FileScopedNamespaceDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract INamedTypeSymbol GetDeclaredSymbol(BaseTypeDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract INamedTypeSymbol GetDeclaredSymbol(DelegateDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IFieldSymbol GetDeclaredSymbol(EnumMemberDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IMethodSymbol GetDeclaredSymbol(BaseMethodDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract ISymbol GetDeclaredSymbol(BasePropertyDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IPropertySymbol GetDeclaredSymbol(PropertyDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IPropertySymbol GetDeclaredSymbol(IndexerDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IEventSymbol GetDeclaredSymbol(EventDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IPropertySymbol GetDeclaredSymbol(AnonymousObjectMemberDeclaratorSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract INamedTypeSymbol GetDeclaredSymbol(AnonymousObjectCreationExpressionSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract INamedTypeSymbol GetDeclaredSymbol(TupleExpressionSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract ISymbol GetDeclaredSymbol(ArgumentSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IMethodSymbol GetDeclaredSymbol(AccessorDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IMethodSymbol GetDeclaredSymbol(ArrowExpressionClauseSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract ISymbol GetDeclaredSymbol(VariableDeclaratorSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract ISymbol GetDeclaredSymbol(SingleVariableDesignationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract ILabelSymbol GetDeclaredSymbol(LabeledStatementSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract ILabelSymbol GetDeclaredSymbol(SwitchLabelSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IAliasSymbol GetDeclaredSymbol(UsingDirectiveSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IAliasSymbol GetDeclaredSymbol(ExternAliasDirectiveSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IParameterSymbol GetDeclaredSymbol(ParameterSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	internal abstract ImmutableArray<ISymbol> GetDeclaredSymbols(BaseFieldDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	protected Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol GetParameterSymbol(ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol> parameters, ParameterSyntax parameter, CancellationToken cancellationToken = default(CancellationToken))
	{
		foreach (Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol item in parameters)
		{
			cancellationToken.ThrowIfCancellationRequested();
			foreach (Location location in item.Locations)
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (location.SourceTree == SyntaxTree && parameter.Span.Contains(location.SourceSpan))
				{
					return item;
				}
			}
		}
		return null;
	}

	public abstract ITypeParameterSymbol GetDeclaredSymbol(TypeParameterSyntax typeParameter, CancellationToken cancellationToken = default(CancellationToken));

	internal BinderFlags GetSemanticModelBinderFlags()
	{
		if (!IgnoresAccessibility)
		{
			return BinderFlags.SemanticModel;
		}
		return BinderFlags.SemanticModel | BinderFlags.IgnoreAccessibility;
	}

	public ILocalSymbol GetDeclaredSymbol(ForEachStatementSyntax forEachStatement)
	{
		Binder enclosingBinder = GetEnclosingBinder(GetAdjustedNodePosition(forEachStatement));
		if (enclosingBinder == null)
		{
			return null;
		}
		Binder binder = enclosingBinder.GetBinder(forEachStatement);
		if (binder == null)
		{
			return null;
		}
		Microsoft.CodeAnalysis.CSharp.Symbols.LocalSymbol localSymbol = binder.GetDeclaredLocalsForScope(forEachStatement).FirstOrDefault();
		return ((localSymbol is SourceLocalSymbol { DeclarationKind: LocalDeclarationKind.ForEachIterationVariable } sourceLocalSymbol) ? GetAdjustedLocalSymbol(sourceLocalSymbol) : localSymbol).GetPublicSymbol();
	}

	internal abstract Microsoft.CodeAnalysis.CSharp.Symbols.LocalSymbol GetAdjustedLocalSymbol(SourceLocalSymbol originalSymbol);

	public ILocalSymbol GetDeclaredSymbol(CatchDeclarationSyntax catchDeclaration)
	{
		CSharpSyntaxNode parent = catchDeclaration.Parent;
		Binder enclosingBinder = GetEnclosingBinder(GetAdjustedNodePosition(parent));
		if (enclosingBinder == null)
		{
			return null;
		}
		if (enclosingBinder.GetBinder(parent) == null)
		{
			return null;
		}
		Microsoft.CodeAnalysis.CSharp.Symbols.LocalSymbol localSymbol = enclosingBinder.GetBinder(parent).GetDeclaredLocalsForScope(parent).FirstOrDefault();
		if ((object)localSymbol == null || localSymbol.DeclarationKind != LocalDeclarationKind.CatchVariable)
		{
			return null;
		}
		return localSymbol.GetPublicSymbol();
	}

	public abstract IRangeVariableSymbol GetDeclaredSymbol(QueryClauseSyntax queryClause, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IRangeVariableSymbol GetDeclaredSymbol(JoinIntoClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IRangeVariableSymbol GetDeclaredSymbol(QueryContinuationSyntax node, CancellationToken cancellationToken = default(CancellationToken));

	private OneOrMany<Symbol> GetSemanticSymbols(BoundExpression boundNode, BoundNode boundNodeForSyntacticParent, Binder binderOpt, SymbolInfoOptions options, out bool isDynamic, out LookupResultKind resultKind, out ImmutableArray<Symbol> memberGroup)
	{
		memberGroup = ImmutableArray<Symbol>.Empty;
		OneOrMany<Symbol> symbols = OneOrMany<Symbol>.Empty;
		resultKind = LookupResultKind.Viable;
		isDynamic = false;
		switch (boundNode.Kind)
		{
		case BoundKind.MethodGroup:
			symbols = GetMethodGroupSemanticSymbols((BoundMethodGroup)boundNode, boundNodeForSyntacticParent, binderOpt, out resultKind, out isDynamic, out memberGroup);
			break;
		case BoundKind.PropertyGroup:
			symbols = GetPropertyGroupSemanticSymbols((BoundPropertyGroup)boundNode, boundNodeForSyntacticParent, binderOpt, out resultKind, out memberGroup);
			break;
		case BoundKind.BadExpression:
		{
			BoundBadExpression boundBadExpression = (BoundBadExpression)boundNode;
			resultKind = boundBadExpression.ResultKind;
			SyntaxKind syntaxKind = boundBadExpression.Syntax.Kind();
			if ((syntaxKind == SyntaxKind.ObjectCreationExpression || syntaxKind == SyntaxKind.ImplicitObjectCreationExpression) ? true : false)
			{
				if (resultKind == LookupResultKind.NotCreatable)
				{
					return OneOrMany.Create(boundBadExpression.Symbols);
				}
				if (boundBadExpression.Type.IsDelegateType())
				{
					resultKind = LookupResultKind.Empty;
					return symbols;
				}
				memberGroup = boundBadExpression.Symbols;
			}
			return OneOrMany.Create(boundBadExpression.Symbols);
		}
		case BoundKind.TypeExpression:
		{
			BoundTypeExpression boundTypeExpression = (BoundTypeExpression)boundNode;
			if (boundNodeForSyntacticParent != null && boundNodeForSyntacticParent.Syntax.Kind() == SyntaxKind.ObjectCreationExpression && ((ObjectCreationExpressionSyntax)boundNodeForSyntacticParent.Syntax).Type == boundTypeExpression.Syntax && boundNodeForSyntacticParent.Kind == BoundKind.BadExpression && ((BoundBadExpression)boundNodeForSyntacticParent).ResultKind == LookupResultKind.NotCreatable)
			{
				resultKind = LookupResultKind.NotCreatable;
			}
			Symbol symbol = (Symbol)(((object)boundTypeExpression.AliasOpt) ?? ((object)boundTypeExpression.Type));
			if (symbol.OriginalDefinition is Microsoft.CodeAnalysis.CSharp.Symbols.ErrorTypeSymbol errorTypeSymbol)
			{
				resultKind = errorTypeSymbol.ResultKind;
				symbols = OneOrMany.Create(errorTypeSymbol.CandidateSymbols);
			}
			else
			{
				symbols = OneOrMany.Create(symbol);
			}
			break;
		}
		case BoundKind.TypeOrValueExpression:
			symbols = OneOrMany.Create(((BoundTypeOrValueExpression)boundNode).ValueSymbol);
			break;
		case BoundKind.Call:
		{
			BoundCall boundCall = (BoundCall)boundNode;
			if (boundCall.OriginalMethodsOpt.IsDefault)
			{
				if ((object)boundCall.Method != null)
				{
					symbols = CreateReducedExtensionMethodIfPossible(boundCall);
					resultKind = boundCall.ResultKind;
				}
			}
			else
			{
				symbols = CreateReducedAndFilteredSymbolsFromOriginals(boundCall, Compilation);
				resultKind = boundCall.ResultKind;
			}
			break;
		}
		case BoundKind.FunctionPointerInvocation:
		{
			BoundFunctionPointerInvocation boundFunctionPointerInvocation = (BoundFunctionPointerInvocation)boundNode;
			symbols = OneOrMany.Create((Symbol)boundFunctionPointerInvocation.FunctionPointer);
			resultKind = boundFunctionPointerInvocation.ResultKind;
			break;
		}
		case BoundKind.UnconvertedAddressOfOperator:
		{
			symbols = GetMethodGroupSemanticSymbols(((BoundUnconvertedAddressOfOperator)boundNode).Operand, boundNodeForSyntacticParent, binderOpt, out resultKind, out isDynamic, out var _);
			break;
		}
		case BoundKind.IndexerAccess:
		{
			BoundIndexerAccess boundIndexerAccess = (BoundIndexerAccess)boundNode;
			resultKind = boundIndexerAccess.ResultKind;
			ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.PropertySymbol> originalIndexersOpt = boundIndexerAccess.OriginalIndexersOpt;
			symbols = (originalIndexersOpt.IsDefault ? OneOrMany.Create((Symbol)boundIndexerAccess.Indexer) : StaticCast<Symbol>.From(OneOrMany.Create(originalIndexersOpt)));
			break;
		}
		case BoundKind.ImplicitIndexerAccess:
			return GetSemanticSymbols(((BoundImplicitIndexerAccess)boundNode).IndexerOrSliceAccess, boundNodeForSyntacticParent, binderOpt, options, out isDynamic, out resultKind, out memberGroup);
		case BoundKind.EventAssignmentOperator:
		{
			BoundEventAssignmentOperator boundEventAssignmentOperator = (BoundEventAssignmentOperator)boundNode;
			isDynamic = boundEventAssignmentOperator.IsDynamic;
			Microsoft.CodeAnalysis.CSharp.Symbols.EventSymbol eventSymbol = boundEventAssignmentOperator.Event;
			Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol methodSymbol = (boundEventAssignmentOperator.IsAddition ? eventSymbol.AddMethod : eventSymbol.RemoveMethod);
			if ((object)methodSymbol == null)
			{
				symbols = OneOrMany<Symbol>.Empty;
				resultKind = LookupResultKind.Empty;
			}
			else
			{
				symbols = OneOrMany.Create((Symbol)methodSymbol);
				resultKind = boundEventAssignmentOperator.ResultKind;
			}
			break;
		}
		case BoundKind.EventAccess:
			if (boundNodeForSyntacticParent is BoundEventAssignmentOperator { ResultKind: LookupResultKind.Viable } boundEventAssignmentOperator2)
			{
				Symbol expressionSymbol = boundNode.ExpressionSymbol;
				if ((object)expressionSymbol != null && boundNode != boundEventAssignmentOperator2.Argument && boundEventAssignmentOperator2.Event.Equals(expressionSymbol, TypeCompareKind.AllNullableIgnoreOptions))
				{
					symbols = OneOrMany.Create((Symbol)boundEventAssignmentOperator2.Event);
					resultKind = boundEventAssignmentOperator2.ResultKind;
					break;
				}
			}
			goto default;
		case BoundKind.Conversion:
		{
			BoundConversion boundConversion = (BoundConversion)boundNode;
			isDynamic = boundConversion.ConversionKind.IsDynamic();
			if (isDynamic)
			{
				break;
			}
			if (boundConversion.ConversionKind == ConversionKind.MethodGroup && boundConversion.IsExtensionMethod)
			{
				symbols = OneOrMany.Create((Symbol)ReducedExtensionMethodSymbol.Create(boundConversion.SymbolOpt));
				resultKind = boundConversion.ResultKind;
				break;
			}
			if (boundConversion.ConversionKind.IsUserDefinedConversion())
			{
				GetSymbolsAndResultKind(boundConversion, boundConversion.SymbolOpt, boundConversion.Conversion.OriginalUserDefinedConversions, out symbols, out resultKind);
				break;
			}
			goto default;
		}
		case BoundKind.BinaryOperator:
			GetSymbolsAndResultKind((BoundBinaryOperator)boundNode, out isDynamic, ref resultKind, ref symbols);
			break;
		case BoundKind.UnaryOperator:
			GetSymbolsAndResultKind((BoundUnaryOperator)boundNode, out isDynamic, ref resultKind, ref symbols);
			break;
		case BoundKind.UserDefinedConditionalLogicalOperator:
		{
			BoundUserDefinedConditionalLogicalOperator boundUserDefinedConditionalLogicalOperator = (BoundUserDefinedConditionalLogicalOperator)boundNode;
			isDynamic = false;
			GetSymbolsAndResultKind(boundUserDefinedConditionalLogicalOperator, boundUserDefinedConditionalLogicalOperator.LogicalOperator, boundUserDefinedConditionalLogicalOperator.OriginalUserDefinedOperatorsOpt, out symbols, out resultKind);
			break;
		}
		case BoundKind.CompoundAssignmentOperator:
			GetSymbolsAndResultKind((BoundCompoundAssignmentOperator)boundNode, out isDynamic, ref resultKind, ref symbols);
			break;
		case BoundKind.IncrementOperator:
			GetSymbolsAndResultKind((BoundIncrementOperator)boundNode, out isDynamic, ref resultKind, ref symbols);
			break;
		case BoundKind.AwaitExpression:
		{
			BoundAwaitExpression boundAwaitExpression = (BoundAwaitExpression)boundNode;
			isDynamic = boundAwaitExpression.AwaitableInfo.IsDynamic;
			goto default;
		}
		case BoundKind.ConditionalOperator:
		{
			BoundConditionalOperator boundConditionalOperator = (BoundConditionalOperator)boundNode;
			isDynamic = boundConditionalOperator.IsDynamic;
			goto default;
		}
		case BoundKind.Attribute:
		{
			BoundAttribute boundAttribute = (BoundAttribute)boundNode;
			resultKind = boundAttribute.ResultKind;
			Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol namedTypeSymbol = (Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol)boundAttribute.Type;
			if (namedTypeSymbol.IsErrorType())
			{
				ImmutableArray<Symbol> candidateSymbols = ((Microsoft.CodeAnalysis.CSharp.Symbols.ErrorTypeSymbol)namedTypeSymbol).CandidateSymbols;
				if (candidateSymbols.Length != 1 || !(candidateSymbols[0] is Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol))
				{
					symbols = OneOrMany.Create(candidateSymbols);
					break;
				}
				namedTypeSymbol = (Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol)candidateSymbols[0];
			}
			AdjustSymbolsForObjectCreation(boundAttribute, namedTypeSymbol, boundAttribute.Constructor, binderOpt, ref resultKind, ref symbols, ref memberGroup);
			break;
		}
		case BoundKind.QueryClause:
		{
			BoundQueryClause boundQueryClause = (BoundQueryClause)boundNode;
			ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
			if (boundQueryClause.Operation != null && (object)boundQueryClause.Operation.ExpressionSymbol != null)
			{
				instance.Add(boundQueryClause.Operation.ExpressionSymbol);
			}
			if ((object)boundQueryClause.DefinedSymbol != null)
			{
				instance.Add(boundQueryClause.DefinedSymbol);
			}
			if (boundQueryClause.Cast != null && (object)boundQueryClause.Cast.ExpressionSymbol != null)
			{
				instance.Add(boundQueryClause.Cast.ExpressionSymbol);
			}
			symbols = instance.ToOneOrManyAndFree();
			break;
		}
		case BoundKind.DynamicInvocation:
		{
			BoundDynamicInvocation boundDynamicInvocation = (BoundDynamicInvocation)boundNode;
			memberGroup = boundDynamicInvocation.ApplicableMethods.Cast<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol, Symbol>();
			symbols = OneOrMany.Create(memberGroup);
			isDynamic = true;
			break;
		}
		case BoundKind.DynamicCollectionElementInitializer:
		{
			BoundDynamicCollectionElementInitializer boundDynamicCollectionElementInitializer = (BoundDynamicCollectionElementInitializer)boundNode;
			memberGroup = boundDynamicCollectionElementInitializer.ApplicableMethods.Cast<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol, Symbol>();
			symbols = OneOrMany.Create(memberGroup);
			isDynamic = true;
			break;
		}
		case BoundKind.DynamicIndexerAccess:
		{
			BoundDynamicIndexerAccess boundDynamicIndexerAccess = (BoundDynamicIndexerAccess)boundNode;
			memberGroup = boundDynamicIndexerAccess.ApplicableIndexers.Cast<Microsoft.CodeAnalysis.CSharp.Symbols.PropertySymbol, Symbol>();
			symbols = OneOrMany.Create(memberGroup);
			isDynamic = true;
			break;
		}
		case BoundKind.DynamicMemberAccess:
			isDynamic = true;
			break;
		case BoundKind.DynamicObjectCreationExpression:
		{
			BoundDynamicObjectCreationExpression boundDynamicObjectCreationExpression = (BoundDynamicObjectCreationExpression)boundNode;
			memberGroup = boundDynamicObjectCreationExpression.ApplicableMethods.Cast<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol, Symbol>();
			symbols = OneOrMany.Create(memberGroup);
			isDynamic = true;
			break;
		}
		case BoundKind.ObjectCreationExpression:
		{
			BoundObjectCreationExpression boundObjectCreationExpression = (BoundObjectCreationExpression)boundNode;
			if ((object)boundObjectCreationExpression.Constructor != null)
			{
				symbols = OneOrMany.Create((Symbol)boundObjectCreationExpression.Constructor);
			}
			else if (boundObjectCreationExpression.ConstructorsGroup.Length > 0)
			{
				symbols = StaticCast<Symbol>.From(OneOrMany.Create(boundObjectCreationExpression.ConstructorsGroup));
				resultKind = resultKind.WorseResultKind(LookupResultKind.OverloadResolutionFailure);
			}
			memberGroup = boundObjectCreationExpression.ConstructorsGroup.Cast<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol, Symbol>();
			break;
		}
		case BoundKind.ThisReference:
		case BoundKind.BaseReference:
		{
			Binder obj = binderOpt ?? GetEnclosingBinder(GetAdjustedNodePosition(boundNode.Syntax));
			Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol containingType = obj.ContainingType;
			Symbol containingMember = obj.ContainingMember();
			Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol thisParameter = GetThisParameter(boundNode.Type, containingType, containingMember, out resultKind);
			symbols = ((thisParameter != null) ? OneOrMany.Create((Symbol)thisParameter) : OneOrMany<Symbol>.Empty);
			break;
		}
		case BoundKind.FromEndIndexExpression:
		{
			BoundFromEndIndexExpression boundFromEndIndexExpression = (BoundFromEndIndexExpression)boundNode;
			if ((object)boundFromEndIndexExpression.MethodOpt != null)
			{
				symbols = OneOrMany.Create((Symbol)boundFromEndIndexExpression.MethodOpt);
			}
			break;
		}
		case BoundKind.RangeExpression:
		{
			BoundRangeExpression boundRangeExpression = (BoundRangeExpression)boundNode;
			if ((object)boundRangeExpression.MethodOpt != null)
			{
				symbols = OneOrMany.Create((Symbol)boundRangeExpression.MethodOpt);
			}
			break;
		}
		default:
		{
			Symbol expressionSymbol2 = boundNode.ExpressionSymbol;
			if ((object)expressionSymbol2 != null)
			{
				symbols = OneOrMany.Create(expressionSymbol2);
				resultKind = boundNode.ResultKind;
			}
			break;
		}
		case BoundKind.DelegateCreationExpression:
			break;
		}
		if (boundNodeForSyntacticParent != null && (options & SymbolInfoOptions.PreferConstructorsToType) != 0)
		{
			AdjustSymbolsForObjectCreation(boundNode, boundNodeForSyntacticParent, binderOpt, ref resultKind, ref symbols, ref memberGroup);
		}
		return symbols;
	}

	private static Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol GetThisParameter(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol typeOfThis, Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol containingType, Symbol containingMember, out LookupResultKind resultKind)
	{
		if ((object)containingMember == null || (object)containingType == null)
		{
			resultKind = LookupResultKind.NotReferencable;
			return new ThisParameterSymbol(containingMember as Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol, typeOfThis);
		}
		SymbolKind kind = containingMember.Kind;
		Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol result;
		if (kind == SymbolKind.Field || kind == SymbolKind.Method || kind == SymbolKind.Property)
		{
			if (containingMember.IsExtensionBlockMember())
			{
				resultKind = LookupResultKind.NotReferencable;
				result = new ThisParameterSymbol(null, containingType);
			}
			else if (containingMember.IsStatic)
			{
				resultKind = LookupResultKind.StaticInstanceMismatch;
				result = new ThisParameterSymbol(containingMember as Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol, containingType);
			}
			else if ((object)typeOfThis == Microsoft.CodeAnalysis.CSharp.Symbols.ErrorTypeSymbol.UnknownResultType)
			{
				result = new ThisParameterSymbol(containingMember as Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol, containingType);
				resultKind = LookupResultKind.NotReferencable;
			}
			else
			{
				switch (containingMember.Kind)
				{
				case SymbolKind.Method:
					resultKind = LookupResultKind.Viable;
					result = containingMember.EnclosingThisSymbol();
					break;
				case SymbolKind.Field:
				case SymbolKind.Property:
					resultKind = LookupResultKind.NotReferencable;
					result = containingMember.EnclosingThisSymbol() ?? new ThisParameterSymbol(null, containingType);
					break;
				default:
					throw ExceptionUtilities.UnexpectedValue(containingMember.Kind);
				}
			}
		}
		else
		{
			result = new ThisParameterSymbol(containingMember as Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol, typeOfThis);
			resultKind = LookupResultKind.NotReferencable;
		}
		return result;
	}

	private static void GetSymbolsAndResultKind(BoundUnaryOperator unaryOperator, out bool isDynamic, ref LookupResultKind resultKind, ref OneOrMany<Symbol> symbols)
	{
		UnaryOperatorKind unaryOperatorKind = unaryOperator.OperatorKind.OperandTypes();
		isDynamic = unaryOperator.OperatorKind.IsDynamic();
		if (unaryOperatorKind == UnaryOperatorKind.Error || unaryOperatorKind == UnaryOperatorKind.UserDefined || unaryOperator.ResultKind != LookupResultKind.Viable)
		{
			if (!isDynamic)
			{
				GetSymbolsAndResultKind(unaryOperator, unaryOperator.MethodOpt, unaryOperator.OriginalUserDefinedOperatorsOpt, out symbols, out resultKind);
			}
		}
		else
		{
			UnaryOperatorKind kind = unaryOperator.OperatorKind.Operator();
			symbols = OneOrMany.Create((Symbol)new SynthesizedIntrinsicOperatorSymbol(unaryOperator.Operand.Type.StrippedType(), OperatorFacts.UnaryOperatorNameFromOperatorKind(kind, unaryOperator.OperatorKind.IsChecked()), unaryOperator.Type.StrippedType()));
			resultKind = unaryOperator.ResultKind;
		}
	}

	private static void GetSymbolsAndResultKind(BoundIncrementOperator increment, out bool isDynamic, ref LookupResultKind resultKind, ref OneOrMany<Symbol> symbols)
	{
		UnaryOperatorKind unaryOperatorKind = increment.OperatorKind.OperandTypes();
		isDynamic = increment.OperatorKind.IsDynamic();
		if (unaryOperatorKind == UnaryOperatorKind.Error || unaryOperatorKind == UnaryOperatorKind.UserDefined || increment.ResultKind != LookupResultKind.Viable)
		{
			if (!isDynamic)
			{
				GetSymbolsAndResultKind(increment, increment.MethodOpt, increment.OriginalUserDefinedOperatorsOpt, out symbols, out resultKind);
			}
		}
		else
		{
			UnaryOperatorKind kind = increment.OperatorKind.Operator();
			Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol typeSymbol = increment.Operand.Type.StrippedType();
			symbols = OneOrMany.Create((Symbol)new SynthesizedIntrinsicOperatorSymbol(typeSymbol, OperatorFacts.UnaryOperatorNameFromOperatorKind(kind, increment.OperatorKind.IsChecked()), typeSymbol));
			resultKind = increment.ResultKind;
		}
	}

	private static void GetSymbolsAndResultKind(BoundBinaryOperator binaryOperator, out bool isDynamic, ref LookupResultKind resultKind, ref OneOrMany<Symbol> symbols)
	{
		BinaryOperatorKind binaryOperatorKind = binaryOperator.OperatorKind.OperandTypes();
		BinaryOperatorKind binaryOperatorKind2 = binaryOperator.OperatorKind.Operator();
		isDynamic = binaryOperator.OperatorKind.IsDynamic();
		if (binaryOperatorKind == BinaryOperatorKind.Error || binaryOperatorKind == BinaryOperatorKind.UserDefined || binaryOperator.ResultKind != LookupResultKind.Viable || binaryOperator.OperatorKind.IsLogical())
		{
			if (!isDynamic)
			{
				GetSymbolsAndResultKind(binaryOperator, binaryOperator.BinaryOperatorMethod, binaryOperator.OriginalUserDefinedOperatorsOpt, out symbols, out resultKind);
			}
			return;
		}
		if (!isDynamic && (binaryOperatorKind2 == BinaryOperatorKind.Equal || binaryOperatorKind2 == BinaryOperatorKind.NotEqual) && ((binaryOperator.Left.IsLiteralNull() && binaryOperator.Right.Type.IsNullableType()) || (binaryOperator.Right.IsLiteralNull() && binaryOperator.Left.Type.IsNullableType())) && binaryOperator.Type.SpecialType == SpecialType.System_Boolean)
		{
			Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol specialType = binaryOperator.Type.ContainingAssembly.GetSpecialType(SpecialType.System_Object);
			symbols = OneOrMany.Create((Symbol)new SynthesizedIntrinsicOperatorSymbol(specialType, OperatorFacts.BinaryOperatorNameFromOperatorKind(binaryOperatorKind2, binaryOperator.OperatorKind.IsChecked()), specialType, binaryOperator.Type));
		}
		else
		{
			symbols = OneOrMany.Create(GetIntrinsicOperatorSymbol(binaryOperatorKind2, isDynamic, binaryOperator.Left.Type, binaryOperator.Right.Type, binaryOperator.Type, binaryOperator.OperatorKind.IsChecked()));
		}
		resultKind = binaryOperator.ResultKind;
	}

	private static Symbol GetIntrinsicOperatorSymbol(BinaryOperatorKind op, bool isDynamic, Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol leftType, Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol rightType, Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol returnType, bool isChecked)
	{
		if (!isDynamic)
		{
			leftType = leftType.StrippedType();
			rightType = rightType.StrippedType();
			returnType = returnType.StrippedType();
		}
		else if ((object)leftType == null)
		{
			leftType = rightType;
		}
		else if ((object)rightType == null)
		{
			rightType = leftType;
		}
		return new SynthesizedIntrinsicOperatorSymbol(leftType, OperatorFacts.BinaryOperatorNameFromOperatorKind(op, isChecked), rightType, returnType);
	}

	private static void GetSymbolsAndResultKind(BoundCompoundAssignmentOperator compoundAssignment, out bool isDynamic, ref LookupResultKind resultKind, ref OneOrMany<Symbol> symbols)
	{
		BinaryOperatorKind binaryOperatorKind = compoundAssignment.Operator.Kind.OperandTypes();
		BinaryOperatorKind op = compoundAssignment.Operator.Kind.Operator();
		isDynamic = compoundAssignment.Operator.Kind.IsDynamic();
		if (binaryOperatorKind == BinaryOperatorKind.Error || binaryOperatorKind == BinaryOperatorKind.UserDefined || compoundAssignment.ResultKind != LookupResultKind.Viable)
		{
			if (!isDynamic)
			{
				GetSymbolsAndResultKind(compoundAssignment, compoundAssignment.Operator.Method, compoundAssignment.OriginalUserDefinedOperatorsOpt, out symbols, out resultKind);
			}
		}
		else
		{
			symbols = OneOrMany.Create(GetIntrinsicOperatorSymbol(op, isDynamic, compoundAssignment.Operator.LeftType, compoundAssignment.Operator.RightType, compoundAssignment.Operator.ReturnType, compoundAssignment.Operator.Kind.IsChecked()));
			resultKind = compoundAssignment.ResultKind;
		}
	}

	private static void GetSymbolsAndResultKind(BoundExpression node, Symbol symbolOpt, ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol> originalCandidates, out OneOrMany<Symbol> symbols, out LookupResultKind resultKind)
	{
		if ((object)symbolOpt != null)
		{
			symbols = OneOrMany.Create(symbolOpt);
			resultKind = node.ResultKind;
		}
		else if (!originalCandidates.IsDefault)
		{
			symbols = StaticCast<Symbol>.From(OneOrMany.Create(originalCandidates));
			resultKind = node.ResultKind;
		}
		else
		{
			symbols = OneOrMany<Symbol>.Empty;
			resultKind = LookupResultKind.Empty;
		}
	}

	private void AdjustSymbolsForObjectCreation(BoundExpression boundNode, BoundNode boundNodeForSyntacticParent, Binder binderOpt, ref LookupResultKind resultKind, ref OneOrMany<Symbol> symbols, ref ImmutableArray<Symbol> memberGroup)
	{
		Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol typeSymbolOpt = null;
		Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol constructorOpt = null;
		SyntaxNode syntax = boundNodeForSyntacticParent.Syntax;
		if (syntax == null || syntax != boundNode.Syntax.Parent || syntax.Kind() != SyntaxKind.Attribute || ((AttributeSyntax)syntax).Name != boundNode.Syntax)
		{
			return;
		}
		OneOrMany<Symbol> oneOrMany = UnwrapAliases(symbols);
		switch (boundNodeForSyntacticParent.Kind)
		{
		case BoundKind.Attribute:
		{
			BoundAttribute boundAttribute = (BoundAttribute)boundNodeForSyntacticParent;
			if (oneOrMany.Count == 1 && oneOrMany[0].Kind == SymbolKind.NamedType)
			{
				typeSymbolOpt = (Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol)oneOrMany[0];
				constructorOpt = boundAttribute.Constructor;
				resultKind = resultKind.WorseResultKind(boundAttribute.ResultKind);
			}
			break;
		}
		case BoundKind.BadExpression:
		{
			BoundBadExpression boundBadExpression = (BoundBadExpression)boundNodeForSyntacticParent;
			if (oneOrMany.Count == 1)
			{
				resultKind = resultKind.WorseResultKind(boundBadExpression.ResultKind);
				typeSymbolOpt = oneOrMany[0] as Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol;
			}
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(boundNodeForSyntacticParent.Kind);
		}
		AdjustSymbolsForObjectCreation(boundNode, typeSymbolOpt, constructorOpt, binderOpt, ref resultKind, ref symbols, ref memberGroup);
	}

	private void AdjustSymbolsForObjectCreation(BoundNode lowestBoundNode, Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol typeSymbolOpt, Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol constructorOpt, Binder binderOpt, ref LookupResultKind resultKind, ref OneOrMany<Symbol> symbols, ref ImmutableArray<Symbol> memberGroup)
	{
		if ((object)typeSymbolOpt == null)
		{
			return;
		}
		Binder binder = binderOpt ?? GetEnclosingBinder(GetAdjustedNodePosition(lowestBoundNode.Syntax));
		ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol> immutableArray2;
		if (binder != null)
		{
			ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol> immutableArray = ((typeSymbolOpt.IsInterfaceType() && (object)typeSymbolOpt.ComImportCoClass != null) ? typeSymbolOpt.ComImportCoClass.InstanceConstructors : typeSymbolOpt.InstanceConstructors);
			CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.Discarded;
			immutableArray2 = binder.FilterInaccessibleConstructors(immutableArray, allowProtectedConstructorsOfBaseType: false, ref useSiteInfo);
			if (((object)constructorOpt == null) ? (!immutableArray2.Any()) : (!immutableArray2.Contains(constructorOpt)))
			{
				immutableArray2 = immutableArray;
			}
		}
		else
		{
			immutableArray2 = ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol>.Empty;
		}
		if ((object)constructorOpt != null)
		{
			symbols = OneOrMany.Create((Symbol)constructorOpt);
		}
		else if (immutableArray2.Length > 0)
		{
			symbols = StaticCast<Symbol>.From(OneOrMany.Create(immutableArray2));
			resultKind = resultKind.WorseResultKind(LookupResultKind.OverloadResolutionFailure);
		}
		memberGroup = immutableArray2.Cast<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol, Symbol>();
	}

	private ImmutableArray<IPropertySymbol> GetIndexerGroupSemanticSymbols(BoundExpression boundNode, Binder binderOpt)
	{
		Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type = boundNode.Type;
		if ((object)type == null || type.IsStatic)
		{
			return ImmutableArray<IPropertySymbol>.Empty;
		}
		Binder binder = binderOpt ?? GetEnclosingBinder(GetAdjustedNodePosition(boundNode.Syntax));
		ArrayBuilder<ISymbol> instance = ArrayBuilder<ISymbol>.GetInstance();
		AppendSymbolsWithNameAndArity(instance, "this[]", 0, binder, type, LookupOptions.MustBeInstance);
		if (instance.Count == 0)
		{
			instance.Free();
			return ImmutableArray<IPropertySymbol>.Empty;
		}
		ImmutableArray<IPropertySymbol> result = FilterOverriddenOrHiddenIndexers(instance);
		instance.Free();
		return result;
	}

	private static ImmutableArray<IPropertySymbol> FilterOverriddenOrHiddenIndexers(ArrayBuilder<ISymbol> symbols)
	{
		PooledHashSet<Symbol> pooledHashSet = null;
		foreach (ISymbol symbol in symbols)
		{
			OverriddenOrHiddenMembersResult overriddenOrHiddenMembers = ((Microsoft.CodeAnalysis.CSharp.Symbols.PropertySymbol)symbol.GetSymbol()).OverriddenOrHiddenMembers;
			foreach (Symbol overriddenMember in overriddenOrHiddenMembers.OverriddenMembers)
			{
				if (pooledHashSet == null)
				{
					pooledHashSet = PooledHashSet<Symbol>.GetInstance();
				}
				pooledHashSet.Add(overriddenMember);
			}
			foreach (Symbol hiddenMember in overriddenOrHiddenMembers.HiddenMembers)
			{
				if (pooledHashSet == null)
				{
					pooledHashSet = PooledHashSet<Symbol>.GetInstance();
				}
				pooledHashSet.Add(hiddenMember);
			}
		}
		ArrayBuilder<IPropertySymbol> instance = ArrayBuilder<IPropertySymbol>.GetInstance();
		foreach (IPropertySymbol symbol2 in symbols)
		{
			if (pooledHashSet == null || !pooledHashSet.Contains(symbol2.GetSymbol()))
			{
				instance.Add(symbol2);
			}
		}
		pooledHashSet?.Free();
		return instance.ToImmutableAndFree();
	}

	private static ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol> FilterOverriddenOrHiddenMethods(ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol> methods)
	{
		if (methods.Length <= 1)
		{
			return methods;
		}
		HashSet<Symbol> hashSet = new HashSet<Symbol>();
		foreach (Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol item in methods)
		{
			OverriddenOrHiddenMembersResult overriddenOrHiddenMembers = item.OverriddenOrHiddenMembers;
			foreach (Symbol overriddenMember in overriddenOrHiddenMembers.OverriddenMembers)
			{
				hashSet.Add(overriddenMember);
			}
			foreach (Symbol hiddenMember in overriddenOrHiddenMembers.HiddenMembers)
			{
				hashSet.Add(hiddenMember);
			}
		}
		return methods.WhereAsArray((Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol m, HashSet<Symbol> hiddenSymbols) => !hiddenSymbols.Contains(m), hashSet);
	}

	private OneOrMany<Symbol> GetMethodGroupSemanticSymbols(BoundMethodGroup boundNode, BoundNode boundNodeForSyntacticParent, Binder binderOpt, out LookupResultKind resultKind, out bool isDynamic, out ImmutableArray<Symbol> memberGroup)
	{
		OneOrMany<Symbol> result = OneOrMany<Symbol>.Empty;
		resultKind = boundNode.ResultKind;
		if (resultKind == LookupResultKind.Empty)
		{
			resultKind = LookupResultKind.Viable;
		}
		isDynamic = false;
		Binder binder = binderOpt ?? GetEnclosingBinder(GetAdjustedNodePosition(boundNode.Syntax));
		memberGroup = GetReducedAndFilteredMethodGroupSymbols(binder, boundNode);
		if (boundNodeForSyntacticParent != null)
		{
			BoundKind kind = boundNodeForSyntacticParent.Kind;
			if (kind <= BoundKind.DynamicInvocation)
			{
				if (kind != BoundKind.BadExpression)
				{
					if (kind != BoundKind.Conversion)
					{
						if (kind != BoundKind.DynamicInvocation)
						{
							goto IL_026f;
						}
						result = OneOrMany.Create(((BoundDynamicInvocation)boundNodeForSyntacticParent).ApplicableMethods.Cast<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol, Symbol>());
						isDynamic = true;
					}
					else
					{
						BoundConversion boundConversion = (BoundConversion)boundNodeForSyntacticParent;
						Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol methodSymbol = null;
						if (boundConversion.ConversionKind == ConversionKind.MethodGroup)
						{
							methodSymbol = boundConversion.SymbolOpt;
						}
						else if (boundConversion.Operand is BoundConversion { ConversionKind: ConversionKind.MethodGroup } boundConversion2)
						{
							methodSymbol = boundConversion2.SymbolOpt;
						}
						if ((object)methodSymbol == null)
						{
							goto IL_026f;
						}
						if (boundConversion.IsExtensionMethod)
						{
							methodSymbol = ReducedExtensionMethodSymbol.Create(methodSymbol);
						}
						result = OneOrMany.Create((Symbol)methodSymbol);
						resultKind = boundConversion.ResultKind;
					}
				}
				else
				{
					ImmutableArray<Symbol> arg = memberGroup;
					result = OneOrMany.Create(((BoundBadExpression)boundNodeForSyntacticParent).Symbols.WhereAsArray<Symbol, ImmutableArray<Symbol>>((Symbol sym, ImmutableArray<Symbol> myMethodGroup) => myMethodGroup.Contains(sym), arg));
					if (result.Any())
					{
						resultKind = ((BoundBadExpression)boundNodeForSyntacticParent).ResultKind;
					}
				}
			}
			else if (kind != BoundKind.Call)
			{
				if (kind != BoundKind.DelegateCreationExpression)
				{
					if (kind != BoundKind.NameOfOperator)
					{
						goto IL_026f;
					}
					result = OneOrMany.Create(memberGroup);
					resultKind = resultKind.WorseResultKind(LookupResultKind.MemberGroup);
				}
				else
				{
					BoundDelegateCreationExpression boundDelegateCreationExpression = (BoundDelegateCreationExpression)boundNodeForSyntacticParent;
					if (boundDelegateCreationExpression.Argument == boundNode && (object)boundDelegateCreationExpression.MethodOpt != null)
					{
						result = CreateReducedExtensionMethodIfPossible(boundDelegateCreationExpression, boundNode.ReceiverOpt);
					}
				}
			}
			else
			{
				BoundCall boundCall = (BoundCall)boundNodeForSyntacticParent;
				if (boundCall.Syntax is InvocationExpressionSyntax invocationExpressionSyntax && invocationExpressionSyntax.Expression.SkipParens() == ((ExpressionSyntax)boundNode.Syntax).SkipParens() && (object)boundCall.Method != null)
				{
					if (boundCall.OriginalMethodsOpt.IsDefault)
					{
						result = CreateReducedExtensionMethodIfPossible(boundCall);
						resultKind = LookupResultKind.Viable;
					}
					else
					{
						resultKind = boundCall.ResultKind.WorseResultKind(LookupResultKind.OverloadResolutionFailure);
						result = CreateReducedAndFilteredSymbolsFromOriginals(boundCall, Compilation);
					}
				}
			}
		}
		else if (memberGroup.Length == 1 && !boundNode.HasAnyErrors)
		{
			result = OneOrMany.Create(memberGroup);
			if (result.Count > 0)
			{
				resultKind = resultKind.WorseResultKind(LookupResultKind.OverloadResolutionFailure);
			}
		}
		goto IL_02cb;
		IL_02cb:
		if (!result.Any())
		{
			result = OneOrMany.Create(memberGroup);
			if (!isDynamic && (int)resultKind > 12)
			{
				resultKind = LookupResultKind.OverloadResolutionFailure;
			}
		}
		return result;
		IL_026f:
		result = OneOrMany.Create(memberGroup);
		if (result.Count > 0)
		{
			resultKind = resultKind.WorseResultKind(LookupResultKind.OverloadResolutionFailure);
		}
		goto IL_02cb;
	}

	private OneOrMany<Symbol> GetPropertyGroupSemanticSymbols(BoundPropertyGroup boundNode, BoundNode boundNodeForSyntacticParent, Binder binderOpt, out LookupResultKind resultKind, out ImmutableArray<Symbol> propertyGroup)
	{
		OneOrMany<Symbol> result = OneOrMany<Symbol>.Empty;
		resultKind = boundNode.ResultKind;
		if (resultKind == LookupResultKind.Empty)
		{
			resultKind = LookupResultKind.Viable;
		}
		propertyGroup = boundNode.Properties.Cast<Microsoft.CodeAnalysis.CSharp.Symbols.PropertySymbol, Symbol>();
		if (boundNodeForSyntacticParent != null)
		{
			switch (boundNodeForSyntacticParent.Kind)
			{
			case BoundKind.IndexerAccess:
			{
				BoundIndexerAccess boundIndexerAccess = (BoundIndexerAccess)boundNodeForSyntacticParent;
				if (boundIndexerAccess.Syntax is ElementAccessExpressionSyntax elementAccessExpressionSyntax && elementAccessExpressionSyntax.Expression == boundNode.Syntax && (object)boundIndexerAccess.Indexer != null)
				{
					if (boundIndexerAccess.OriginalIndexersOpt.IsDefault)
					{
						result = OneOrMany.Create((Symbol)boundIndexerAccess.Indexer);
						resultKind = LookupResultKind.Viable;
					}
					else
					{
						resultKind = boundIndexerAccess.ResultKind.WorseResultKind(LookupResultKind.OverloadResolutionFailure);
						result = StaticCast<Symbol>.From(OneOrMany.Create(boundIndexerAccess.OriginalIndexersOpt));
					}
				}
				break;
			}
			case BoundKind.BadExpression:
			{
				ImmutableArray<Symbol> arg = propertyGroup;
				result = OneOrMany.Create(((BoundBadExpression)boundNodeForSyntacticParent).Symbols.WhereAsArray<Symbol, ImmutableArray<Symbol>>((Symbol sym, ImmutableArray<Symbol> myPropertyGroup) => myPropertyGroup.Contains(sym), arg));
				if (result.Any())
				{
					resultKind = ((BoundBadExpression)boundNodeForSyntacticParent).ResultKind;
				}
				break;
			}
			}
		}
		else if (propertyGroup.Length == 1 && !boundNode.HasAnyErrors)
		{
			result = OneOrMany.Create(propertyGroup);
		}
		if (!result.Any())
		{
			result = OneOrMany.Create(propertyGroup);
			if ((int)resultKind > 12)
			{
				resultKind = LookupResultKind.OverloadResolutionFailure;
			}
		}
		return result;
	}

	private SymbolInfo GetNamedArgumentSymbolInfo(IdentifierNameSyntax identifierNameSyntax, CancellationToken cancellationToken)
	{
		string valueText = identifierNameSyntax.Identifier.ValueText;
		if (valueText.Length == 0)
		{
			return SymbolInfo.None;
		}
		CSharpSyntaxNode parent = identifierNameSyntax.Parent.Parent.Parent;
		if (parent.IsKind(SyntaxKind.TupleExpression))
		{
			ArgumentSyntax declaratorSyntax = (ArgumentSyntax)identifierNameSyntax.Parent.Parent;
			ISymbol declaredSymbol = GetDeclaredSymbol(declaratorSyntax, cancellationToken);
			if (declaredSymbol != null)
			{
				return new SymbolInfo(declaredSymbol);
			}
			return SymbolInfo.None;
		}
		if (parent.IsKind(SyntaxKind.PropertyPatternClause) || parent.IsKind(SyntaxKind.PositionalPatternClause))
		{
			return GetSymbolInfoWorker(identifierNameSyntax, SymbolInfoOptions.DefaultOptions, cancellationToken);
		}
		CSharpSyntaxNode parent2 = parent.Parent;
		SymbolInfo symbolInfoWorker = GetSymbolInfoWorker(parent2, SymbolInfoOptions.DefaultOptions, cancellationToken);
		if (symbolInfoWorker.Symbol != null)
		{
			Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol parameterSymbol = FindNamedParameter(symbolInfoWorker.Symbol.GetSymbol().GetParameters(), valueText);
			if ((object)parameterSymbol != null)
			{
				return new SymbolInfo(parameterSymbol.GetPublicSymbol());
			}
			return SymbolInfo.None;
		}
		ArrayBuilder<ISymbol> instance = ArrayBuilder<ISymbol>.GetInstance();
		foreach (ISymbol candidateSymbol in symbolInfoWorker.CandidateSymbols)
		{
			SymbolKind kind = candidateSymbol.Kind;
			if (kind == SymbolKind.Method || kind == SymbolKind.Property)
			{
				Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol parameterSymbol2 = FindNamedParameter(candidateSymbol.GetSymbol().GetParameters(), valueText);
				if ((object)parameterSymbol2 != null)
				{
					instance.Add(parameterSymbol2.GetPublicSymbol());
				}
			}
		}
		if (instance.Count == 0)
		{
			instance.Free();
			return SymbolInfo.None;
		}
		return new SymbolInfo(instance.ToImmutableAndFree(), symbolInfoWorker.CandidateReason);
	}

	private static Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol FindNamedParameter(ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol> parameters, string argumentName)
	{
		foreach (Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol item in parameters)
		{
			if (item.Name == argumentName)
			{
				return item;
			}
		}
		return null;
	}

	internal static ImmutableArray<Symbol> GetReducedAndFilteredMethodGroupSymbols(Binder binder, BoundMethodGroup node)
	{
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		ArrayBuilder<Symbol> instance2 = ArrayBuilder<Symbol>.GetInstance();
		LookupResultKind resultKind = LookupResultKind.Empty;
		ImmutableArray<TypeWithAnnotations> typeArgumentsOpt = node.TypeArgumentsOpt;
		if (node.Methods.Any())
		{
			foreach (Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol item in FilterOverriddenOrHiddenMethods(node.Methods))
			{
				MergeReducedAndFilteredSymbol(instance, instance2, new SingleLookupResult(node.ResultKind, item, node.LookupError), typeArgumentsOpt, null, ref resultKind, binder.Compilation);
			}
		}
		else
		{
			Symbol lookupSymbolOpt = node.LookupSymbolOpt;
			if ((object)lookupSymbolOpt != null && lookupSymbolOpt.Kind == SymbolKind.Method)
			{
				MergeReducedAndFilteredSymbol(instance, instance2, new SingleLookupResult(node.ResultKind, lookupSymbolOpt, node.LookupError), typeArgumentsOpt, null, ref resultKind, binder.Compilation);
			}
		}
		BoundExpression receiverOpt = node.ReceiverOpt;
		string name = node.Name;
		if (node.SearchExtensions)
		{
			Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type = receiverOpt.Type;
			if ((object)type != null)
			{
				int arity;
				LookupOptions options;
				if (typeArgumentsOpt.IsDefault)
				{
					arity = 0;
					options = LookupOptions.AllMethodsOnArityZero;
				}
				else
				{
					arity = typeArgumentsOpt.Length;
					options = LookupOptions.Default;
				}
				binder = binder.WithAdditionalFlags(BinderFlags.SemanticModel);
				CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.Discarded;
				ArrayBuilder<SingleLookupResult> instance3 = ArrayBuilder<SingleLookupResult>.GetInstance();
				foreach (ExtensionScope item2 in new ExtensionScopes(binder))
				{
					instance3.Clear();
					item2.Binder.EnumerateAllExtensionMembersInSingleBinder(instance3, name, arity, options, binder, ref useSiteInfo, ref useSiteInfo);
					foreach (SingleLookupResult item3 in instance3)
					{
						Symbol symbol = item3.Symbol;
						if ((symbol is Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol || symbol is Microsoft.CodeAnalysis.CSharp.Symbols.PropertySymbol) ? true : false)
						{
							MergeReducedAndFilteredSymbol(instance, instance2, item3, typeArgumentsOpt, type, ref resultKind, binder.Compilation);
						}
					}
				}
				instance3.Free();
			}
		}
		instance.Free();
		return instance2.ToImmutableAndFree();
	}

	private static bool AddReducedAndFilteredSymbol(ArrayBuilder<Symbol> members, ArrayBuilder<Symbol> filteredMembers, Symbol member, ImmutableArray<TypeWithAnnotations> typeArguments, Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol receiverType, CSharpCompilation compilation)
	{
		Symbol reducedAndFilteredSymbol = member.GetReducedAndFilteredSymbol(typeArguments, receiverType, compilation, checkFullyInferred: false);
		if ((object)reducedAndFilteredSymbol == null)
		{
			return false;
		}
		if (filteredMembers.Contains(reducedAndFilteredSymbol))
		{
			return false;
		}
		members.Add(member);
		filteredMembers.Add(reducedAndFilteredSymbol);
		return true;
	}

	private static void MergeReducedAndFilteredSymbol(ArrayBuilder<Symbol> members, ArrayBuilder<Symbol> filteredMembers, SingleLookupResult singleResult, ImmutableArray<TypeWithAnnotations> typeArguments, Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol receiverType, ref LookupResultKind resultKind, CSharpCompilation compilation)
	{
		if ((object)singleResult.Symbol == null)
		{
			return;
		}
		Symbol symbol = singleResult.Symbol;
		LookupResultKind kind = singleResult.Kind;
		if ((int)resultKind <= (int)kind)
		{
			if ((int)resultKind < (int)kind)
			{
				members.Clear();
				filteredMembers.Clear();
				resultKind = LookupResultKind.Empty;
			}
			if (AddReducedAndFilteredSymbol(members, filteredMembers, symbol, typeArguments, receiverType, compilation) && (int)resultKind < (int)kind)
			{
				resultKind = kind;
			}
		}
	}

	private static OneOrMany<Symbol> CreateReducedAndFilteredSymbolsFromOriginals(BoundCall call, CSharpCompilation compilation)
	{
		ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol> originalMethodsOpt = call.OriginalMethodsOpt;
		Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol receiverType = null;
		if (call.InvokedAsExtensionMethod)
		{
			receiverType = ((call.ReceiverOpt == null) ? call.Arguments[0].Type : call.ReceiverOpt.Type);
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		ArrayBuilder<Symbol> instance2 = ArrayBuilder<Symbol>.GetInstance();
		foreach (Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol item in FilterOverriddenOrHiddenMethods(originalMethodsOpt))
		{
			AddReducedAndFilteredSymbol(instance, instance2, item, default(ImmutableArray<TypeWithAnnotations>), receiverType, compilation);
		}
		instance.Free();
		return instance2.ToOneOrManyAndFree();
	}

	private OneOrMany<Symbol> CreateReducedExtensionMethodIfPossible(BoundCall call)
	{
		Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol methodSymbol = call.Method;
		if (call.InvokedAsExtensionMethod && methodSymbol.IsExtensionMethod && methodSymbol.MethodKind != MethodKind.ReducedExtension)
		{
			BoundExpression boundExpression = call.Arguments[0];
			methodSymbol = methodSymbol.ReduceExtensionMethod(boundExpression.Type, Compilation) ?? methodSymbol;
		}
		return OneOrMany.Create((Symbol)methodSymbol);
	}

	private OneOrMany<Symbol> CreateReducedExtensionMethodIfPossible(BoundDelegateCreationExpression delegateCreation, BoundExpression receiverOpt)
	{
		Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol methodSymbol = delegateCreation.MethodOpt;
		if (delegateCreation.IsExtensionMethod && methodSymbol.IsExtensionMethod && receiverOpt != null)
		{
			methodSymbol = methodSymbol.ReduceExtensionMethod(receiverOpt.Type, Compilation) ?? methodSymbol;
		}
		return OneOrMany.Create((Symbol)methodSymbol);
	}

	public abstract ForEachStatementInfo GetForEachStatementInfo(ForEachStatementSyntax node);

	public abstract ForEachStatementInfo GetForEachStatementInfo(CommonForEachStatementSyntax node);

	public abstract DeconstructionInfo GetDeconstructionInfo(AssignmentExpressionSyntax node);

	public abstract DeconstructionInfo GetDeconstructionInfo(ForEachVariableStatementSyntax node);

	public abstract AwaitExpressionInfo GetAwaitExpressionInfo(AwaitExpressionSyntax node);

	public abstract AwaitExpressionInfo GetAwaitExpressionInfo(LocalDeclarationStatementSyntax node);

	public abstract AwaitExpressionInfo GetAwaitExpressionInfo(UsingStatementSyntax node);

	public PreprocessingSymbolInfo GetPreprocessingSymbolInfo(IdentifierNameSyntax node)
	{
		CheckSyntaxNode(node);
		if (isPossiblePreprocessingSymbolReference(node))
		{
			bool isDefined = SyntaxTree.IsPreprocessorSymbolDefined(node.Identifier.ValueText, node.Identifier.SpanStart);
			return new PreprocessingSymbolInfo(new PreprocessingSymbol(node.Identifier.ValueText), isDefined);
		}
		return PreprocessingSymbolInfo.None;
		static bool isPossiblePreprocessingSymbolReference(IdentifierNameSyntax identifierNameSyntax)
		{
			CSharpSyntaxNode parent = identifierNameSyntax.Parent;
			while (parent != null)
			{
				SyntaxKind syntaxKind = parent.Kind();
				switch (syntaxKind)
				{
				case SyntaxKind.IfDirectiveTrivia:
					return ((IfDirectiveTriviaSyntax)parent).Condition.FullSpan.Contains(identifierNameSyntax.FullSpan);
				case SyntaxKind.ElifDirectiveTrivia:
					return ((ElifDirectiveTriviaSyntax)parent).Condition.FullSpan.Contains(identifierNameSyntax.FullSpan);
				default:
					if (SyntaxFacts.IsPreprocessorDirective(syntaxKind))
					{
						return false;
					}
					parent = parent.Parent;
					break;
				}
			}
			return false;
		}
	}

	internal static void ValidateSymbolInfoOptions(SymbolInfoOptions options)
	{
	}

	public ISymbol GetEnclosingSymbol(int position)
	{
		position = CheckAndAdjustPosition(position);
		return GetEnclosingBinder(position)?.ContainingMemberOrLambda.GetPublicSymbol();
	}

	private SymbolInfo GetSymbolInfoFromNode(SyntaxNode node, CancellationToken cancellationToken)
	{
		if (node != null)
		{
			if (!(node is ExpressionSyntax expression))
			{
				if (!(node is ConstructorInitializerSyntax constructorInitializer))
				{
					if (!(node is PrimaryConstructorBaseTypeSyntax constructorInitializer2))
					{
						if (!(node is AttributeSyntax attributeSyntax))
						{
							if (!(node is CrefSyntax crefSyntax))
							{
								if (!(node is SelectOrGroupClauseSyntax node2))
								{
									if (!(node is OrderingSyntax node3))
									{
										if (node is PositionalPatternClauseSyntax node4)
										{
											return GetSymbolInfo(node4, cancellationToken);
										}
										return SymbolInfo.None;
									}
									return GetSymbolInfo(node3, cancellationToken);
								}
								return GetSymbolInfo(node2, cancellationToken);
							}
							return GetSymbolInfo(crefSyntax, cancellationToken);
						}
						return GetSymbolInfo(attributeSyntax, cancellationToken);
					}
					return GetSymbolInfo(constructorInitializer2, cancellationToken);
				}
				return GetSymbolInfo(constructorInitializer, cancellationToken);
			}
			return GetSymbolInfo(expression, cancellationToken);
		}
		throw new ArgumentNullException("node");
	}

	private TypeInfo GetTypeInfoFromNode(SyntaxNode node, CancellationToken cancellationToken)
	{
		if (node != null)
		{
			if (!(node is ExpressionSyntax expression))
			{
				if (!(node is ConstructorInitializerSyntax constructorInitializer))
				{
					if (!(node is AttributeSyntax attributeSyntax))
					{
						if (!(node is SelectOrGroupClauseSyntax node2))
						{
							if (node is PatternSyntax pattern)
							{
								return GetTypeInfo(pattern, cancellationToken);
							}
							return CSharpTypeInfo.None;
						}
						return GetTypeInfo(node2, cancellationToken);
					}
					return GetTypeInfo(attributeSyntax, cancellationToken);
				}
				return GetTypeInfo(constructorInitializer, cancellationToken);
			}
			return GetTypeInfo(expression, cancellationToken);
		}
		throw new ArgumentNullException("node");
	}

	private ImmutableArray<ISymbol> GetMemberGroupFromNode(SyntaxNode node, CancellationToken cancellationToken)
	{
		if (node != null)
		{
			if (!(node is ExpressionSyntax expression))
			{
				if (!(node is ConstructorInitializerSyntax initializer))
				{
					if (node is AttributeSyntax attribute)
					{
						return GetMemberGroup(attribute, cancellationToken);
					}
					return ImmutableArray<ISymbol>.Empty;
				}
				return GetMemberGroup(initializer, cancellationToken);
			}
			return GetMemberGroup(expression, cancellationToken);
		}
		throw new ArgumentNullException("node");
	}

	protected sealed override ImmutableArray<ISymbol> GetMemberGroupCore(SyntaxNode node, CancellationToken cancellationToken)
	{
		return StaticCast<ISymbol>.From(GetMemberGroupFromNode(node, cancellationToken));
	}

	protected sealed override SymbolInfo GetSpeculativeSymbolInfoCore(int position, SyntaxNode node, SpeculativeBindingOption bindingOption)
	{
		if (!(node is ExpressionSyntax expression))
		{
			if (!(node is ConstructorInitializerSyntax constructorInitializer))
			{
				if (!(node is PrimaryConstructorBaseTypeSyntax constructorInitializer2))
				{
					if (!(node is AttributeSyntax attribute))
					{
						if (node is CrefSyntax cref)
						{
							return GetSpeculativeSymbolInfo(position, cref);
						}
						return SymbolInfo.None;
					}
					return GetSpeculativeSymbolInfo(position, attribute);
				}
				return GetSpeculativeSymbolInfo(position, constructorInitializer2);
			}
			return GetSpeculativeSymbolInfo(position, constructorInitializer);
		}
		return GetSpeculativeSymbolInfo(position, expression, bindingOption);
	}

	protected sealed override TypeInfo GetSpeculativeTypeInfoCore(int position, SyntaxNode node, SpeculativeBindingOption bindingOption)
	{
		if (!(node is ExpressionSyntax expression))
		{
			return CSharpTypeInfo.None;
		}
		return GetSpeculativeTypeInfo(position, expression, bindingOption);
	}

	protected sealed override IAliasSymbol GetSpeculativeAliasInfoCore(int position, SyntaxNode nameSyntax, SpeculativeBindingOption bindingOption)
	{
		if (!(nameSyntax is IdentifierNameSyntax nameSyntax2))
		{
			return null;
		}
		return GetSpeculativeAliasInfo(position, nameSyntax2, bindingOption);
	}

	protected sealed override SymbolInfo GetSymbolInfoCore(SyntaxNode node, CancellationToken cancellationToken)
	{
		return GetSymbolInfoFromNode(node, cancellationToken);
	}

	protected sealed override TypeInfo GetTypeInfoCore(SyntaxNode node, CancellationToken cancellationToken)
	{
		return GetTypeInfoFromNode(node, cancellationToken);
	}

	protected sealed override IAliasSymbol GetAliasInfoCore(SyntaxNode node, CancellationToken cancellationToken)
	{
		if (!(node is IdentifierNameSyntax nameSyntax))
		{
			return null;
		}
		return GetAliasInfo(nameSyntax, cancellationToken);
	}

	protected sealed override PreprocessingSymbolInfo GetPreprocessingSymbolInfoCore(SyntaxNode node)
	{
		if (!(node is IdentifierNameSyntax node2))
		{
			return PreprocessingSymbolInfo.None;
		}
		return GetPreprocessingSymbolInfo(node2);
	}

	protected sealed override ISymbol GetDeclaredSymbolCore(SyntaxNode node, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (!(node is AccessorDeclarationSyntax declarationSyntax))
		{
			if (!(node is BaseTypeDeclarationSyntax declarationSyntax2))
			{
				if (!(node is QueryClauseSyntax queryClause))
				{
					if (node is MemberDeclarationSyntax declarationSyntax3)
					{
						return GetDeclaredSymbol(declarationSyntax3, cancellationToken);
					}
					switch (node.Kind())
					{
					case SyntaxKind.LocalFunctionStatement:
						return GetDeclaredSymbol((LocalFunctionStatementSyntax)node, cancellationToken);
					case SyntaxKind.LabeledStatement:
						return GetDeclaredSymbol((LabeledStatementSyntax)node, cancellationToken);
					case SyntaxKind.CaseSwitchLabel:
					case SyntaxKind.DefaultSwitchLabel:
						return GetDeclaredSymbol((SwitchLabelSyntax)node, cancellationToken);
					case SyntaxKind.AnonymousObjectCreationExpression:
						return GetDeclaredSymbol((AnonymousObjectCreationExpressionSyntax)node, cancellationToken);
					case SyntaxKind.AnonymousObjectMemberDeclarator:
						return GetDeclaredSymbol((AnonymousObjectMemberDeclaratorSyntax)node, cancellationToken);
					case SyntaxKind.TupleExpression:
						return GetDeclaredSymbol((TupleExpressionSyntax)node, cancellationToken);
					case SyntaxKind.Argument:
						return GetDeclaredSymbol((ArgumentSyntax)node, cancellationToken);
					case SyntaxKind.VariableDeclarator:
						return GetDeclaredSymbol((VariableDeclaratorSyntax)node, cancellationToken);
					case SyntaxKind.SingleVariableDesignation:
						return GetDeclaredSymbol((SingleVariableDesignationSyntax)node, cancellationToken);
					case SyntaxKind.TupleElement:
						return GetDeclaredSymbol((TupleElementSyntax)node, cancellationToken);
					case SyntaxKind.NamespaceDeclaration:
						return GetDeclaredSymbol((NamespaceDeclarationSyntax)node, cancellationToken);
					case SyntaxKind.FileScopedNamespaceDeclaration:
						return GetDeclaredSymbol((FileScopedNamespaceDeclarationSyntax)node, cancellationToken);
					case SyntaxKind.Parameter:
						return GetDeclaredSymbol((ParameterSyntax)node, cancellationToken);
					case SyntaxKind.TypeParameter:
						return GetDeclaredSymbol((TypeParameterSyntax)node, cancellationToken);
					case SyntaxKind.UsingDirective:
					{
						UsingDirectiveSyntax usingDirectiveSyntax = (UsingDirectiveSyntax)node;
						if (usingDirectiveSyntax.Alias != null)
						{
							return GetDeclaredSymbol(usingDirectiveSyntax, cancellationToken);
						}
						break;
					}
					case SyntaxKind.ForEachStatement:
						return GetDeclaredSymbol((ForEachStatementSyntax)node);
					case SyntaxKind.CatchDeclaration:
						return GetDeclaredSymbol((CatchDeclarationSyntax)node);
					case SyntaxKind.JoinIntoClause:
						return GetDeclaredSymbol((JoinIntoClauseSyntax)node, cancellationToken);
					case SyntaxKind.QueryContinuation:
						return GetDeclaredSymbol((QueryContinuationSyntax)node, cancellationToken);
					case SyntaxKind.CompilationUnit:
						return GetDeclaredSymbol((CompilationUnitSyntax)node, cancellationToken);
					}
					return null;
				}
				return GetDeclaredSymbol(queryClause, cancellationToken);
			}
			return GetDeclaredSymbol(declarationSyntax2, cancellationToken);
		}
		return GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public ISymbol GetDeclaredSymbol(TupleElementSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		if (declarationSyntax.Parent is TupleTypeSyntax tupleTypeSyntax)
		{
			return (GetSymbolInfo(tupleTypeSyntax, cancellationToken).Symbol.GetSymbol() as Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol)?.TupleElements.ElementAtOrDefault(tupleTypeSyntax.Elements.IndexOf(declarationSyntax)).GetPublicSymbol();
		}
		return null;
	}

	protected sealed override ImmutableArray<ISymbol> GetDeclaredSymbolsCore(SyntaxNode declaration, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (declaration is BaseFieldDeclarationSyntax declarationSyntax)
		{
			return GetDeclaredSymbols(declarationSyntax, cancellationToken);
		}
		if (declaration is TypeDeclarationSyntax typeDeclarationSyntax)
		{
			INamedTypeSymbol declaredSymbol = GetDeclaredSymbol(typeDeclarationSyntax, cancellationToken);
			SynthesizedPrimaryConstructor synthesizedPrimaryConstructor = TryGetSynthesizedPrimaryConstructor(typeDeclarationSyntax, declaredSymbol.GetSymbol<Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol>());
			if ((object)synthesizedPrimaryConstructor != null)
			{
				return ImmutableArray.Create((ISymbol)declaredSymbol, (ISymbol)synthesizedPrimaryConstructor.GetPublicSymbol());
			}
			return ImmutableArray.Create((ISymbol)declaredSymbol);
		}
		ISymbol declaredSymbolCore = GetDeclaredSymbolCore(declaration, cancellationToken);
		if (declaredSymbolCore == null)
		{
			return ImmutableArray<ISymbol>.Empty;
		}
		return ImmutableArray.Create(declaredSymbolCore);
	}

	public IMethodSymbol? GetInterceptorMethod(InvocationExpressionSyntax node, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		CheckSyntaxNode(node);
		SimpleNameSyntax interceptableNameSyntax = node.GetInterceptableNameSyntax();
		if (interceptableNameSyntax != null)
		{
			(Location, Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol)? tuple = Compilation.TryGetInterceptor(interceptableNameSyntax);
			if (tuple.HasValue)
			{
				Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol item = tuple.GetValueOrDefault().Item2;
				if ((object)item != null)
				{
					return item.GetPublicSymbol();
				}
			}
		}
		return null;
	}

	public InterceptableLocation? GetInterceptableLocation(InvocationExpressionSyntax node, CancellationToken cancellationToken)
	{
		CheckSyntaxNode(node);
		SimpleNameSyntax interceptableNameSyntax = node.GetInterceptableNameSyntax();
		if (interceptableNameSyntax == null)
		{
			return null;
		}
		return GetInterceptableLocationInternal(interceptableNameSyntax, cancellationToken);
	}

	internal InterceptableLocation GetInterceptableLocationInternal(SyntaxNode nameSyntax, CancellationToken cancellationToken)
	{
		SyntaxTree syntaxTree = nameSyntax.SyntaxTree;
		SourceText text = syntaxTree.GetText(cancellationToken);
		string filePath = syntaxTree.FilePath;
		ImmutableArray<byte> contentHash = text.GetContentHash();
		LinePosition start = nameSyntax.Location.GetLineSpan().Span.Start;
		return new InterceptableLocation1(lineNumberOneIndexed: start.Line + 1, characterNumberOneIndexed: start.Character + 1, checksum: contentHash, path: filePath, resolver: Compilation.Options.SourceReferenceResolver, position: nameSyntax.Position);
	}

	protected static SynthesizedPrimaryConstructor TryGetSynthesizedPrimaryConstructor(TypeDeclarationSyntax node, Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol type)
	{
		if (type is SourceMemberContainerTypeSymbol sourceMemberContainerTypeSymbol)
		{
			SynthesizedPrimaryConstructor primaryConstructor = sourceMemberContainerTypeSymbol.PrimaryConstructor;
			if ((object)primaryConstructor != null && primaryConstructor.SyntaxRef.SyntaxTree == node.SyntaxTree && primaryConstructor.GetSyntax() == node)
			{
				return primaryConstructor;
			}
		}
		return null;
	}

	internal override void ComputeDeclarationsInSpan(TextSpan span, bool getSymbol, ArrayBuilder<DeclarationInfo> builder, CancellationToken cancellationToken)
	{
		CSharpDeclarationComputer.ComputeDeclarationsInSpan(this, span, getSymbol, builder, cancellationToken);
	}

	internal override void ComputeDeclarationsInNode(SyntaxNode node, ISymbol associatedSymbol, bool getSymbol, ArrayBuilder<DeclarationInfo> builder, CancellationToken cancellationToken, int? levelsToCompute = null)
	{
		CSharpDeclarationComputer.ComputeDeclarationsInNode(this, associatedSymbol, node, getSymbol, builder, cancellationToken, levelsToCompute);
	}

	internal abstract override Func<SyntaxNode, bool> GetSyntaxNodesToAnalyzeFilter(SyntaxNode declaredNode, ISymbol declaredSymbol);

	internal abstract override bool ShouldSkipSyntaxNodeAnalysis(SyntaxNode node, ISymbol containingSymbol);

	protected internal override SyntaxNode GetTopmostNodeForDiagnosticAnalysis(ISymbol symbol, SyntaxNode declaringSyntax)
	{
		SymbolKind kind = symbol.Kind;
		if ((uint)(kind - 5) <= 1u)
		{
			BaseFieldDeclarationSyntax baseFieldDeclarationSyntax = declaringSyntax.FirstAncestorOrSelf<BaseFieldDeclarationSyntax>();
			if (baseFieldDeclarationSyntax != null)
			{
				return baseFieldDeclarationSyntax;
			}
		}
		return declaringSyntax;
	}

	protected sealed override ImmutableArray<ISymbol> LookupSymbolsCore(int position, INamespaceOrTypeSymbol container, string name, bool includeReducedExtensionMethods)
	{
		return LookupSymbols(position, container.EnsureCSharpSymbolOrNull("container"), name, includeReducedExtensionMethods);
	}

	protected sealed override ImmutableArray<ISymbol> LookupBaseMembersCore(int position, string name)
	{
		return LookupBaseMembers(position, name);
	}

	protected sealed override ImmutableArray<ISymbol> LookupStaticMembersCore(int position, INamespaceOrTypeSymbol container, string name)
	{
		return LookupStaticMembers(position, container.EnsureCSharpSymbolOrNull("container"), name);
	}

	protected sealed override ImmutableArray<ISymbol> LookupNamespacesAndTypesCore(int position, INamespaceOrTypeSymbol container, string name)
	{
		return LookupNamespacesAndTypes(position, container.EnsureCSharpSymbolOrNull("container"), name);
	}

	protected sealed override ImmutableArray<ISymbol> LookupLabelsCore(int position, string name)
	{
		return LookupLabels(position, name);
	}

	protected sealed override ControlFlowAnalysis AnalyzeControlFlowCore(SyntaxNode firstStatement, SyntaxNode lastStatement)
	{
		if (firstStatement == null)
		{
			throw new ArgumentNullException("firstStatement");
		}
		if (lastStatement == null)
		{
			throw new ArgumentNullException("lastStatement");
		}
		if (!(firstStatement is StatementSyntax firstStatement2))
		{
			throw new ArgumentException("firstStatement is not a StatementSyntax.");
		}
		if (!(lastStatement is StatementSyntax lastStatement2))
		{
			throw new ArgumentException("firstStatement is a StatementSyntax but lastStatement isn't.");
		}
		return AnalyzeControlFlow(firstStatement2, lastStatement2);
	}

	protected sealed override ControlFlowAnalysis AnalyzeControlFlowCore(SyntaxNode statement)
	{
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		if (!(statement is StatementSyntax statement2))
		{
			throw new ArgumentException("statement is not a StatementSyntax.");
		}
		return AnalyzeControlFlow(statement2);
	}

	protected sealed override DataFlowAnalysis AnalyzeDataFlowCore(SyntaxNode firstStatement, SyntaxNode lastStatement)
	{
		if (firstStatement == null)
		{
			throw new ArgumentNullException("firstStatement");
		}
		if (lastStatement == null)
		{
			throw new ArgumentNullException("lastStatement");
		}
		if (!(firstStatement is StatementSyntax firstStatement2))
		{
			throw new ArgumentException("firstStatement is not a StatementSyntax.");
		}
		if (!(lastStatement is StatementSyntax lastStatement2))
		{
			throw new ArgumentException("lastStatement is not a StatementSyntax.");
		}
		return AnalyzeDataFlow(firstStatement2, lastStatement2);
	}

	protected sealed override DataFlowAnalysis AnalyzeDataFlowCore(SyntaxNode statementOrExpression)
	{
		if (statementOrExpression != null)
		{
			if (!(statementOrExpression is StatementSyntax statement))
			{
				if (!(statementOrExpression is ExpressionSyntax expression))
				{
					if (!(statementOrExpression is ConstructorInitializerSyntax constructorInitializer))
					{
						if (statementOrExpression is PrimaryConstructorBaseTypeSyntax primaryConstructorBaseType)
						{
							return AnalyzeDataFlow(primaryConstructorBaseType);
						}
						throw new ArgumentException("statementOrExpression is not a StatementSyntax or an ExpressionSyntax or a ConstructorInitializerSyntax or a PrimaryConstructorBaseTypeSyntax.");
					}
					return AnalyzeDataFlow(constructorInitializer);
				}
				return AnalyzeDataFlow(expression);
			}
			return AnalyzeDataFlow(statement);
		}
		throw new ArgumentNullException("statementOrExpression");
	}

	protected sealed override Optional<object> GetConstantValueCore(SyntaxNode node, CancellationToken cancellationToken)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (!(node is ExpressionSyntax expression))
		{
			return default(Optional<object>);
		}
		return GetConstantValue(expression, cancellationToken);
	}

	protected sealed override ISymbol GetEnclosingSymbolCore(int position, CancellationToken cancellationToken)
	{
		return GetEnclosingSymbol(position);
	}

	private protected sealed override ImmutableArray<IImportScope> GetImportScopesCore(int position, CancellationToken cancellationToken)
	{
		position = CheckAndAdjustPosition(position);
		Binder enclosingBinder = GetEnclosingBinder(position);
		ArrayBuilder<IImportScope> instance = ArrayBuilder<IImportScope>.GetInstance();
		for (ImportChain importChain = enclosingBinder?.ImportChain; importChain != null; importChain = importChain.ParentOpt)
		{
			Imports imports = importChain.Imports;
			if (!imports.IsEmpty)
			{
				instance.Add(new SimpleImportScope(imports.UsingAliases.SelectAsArray((KeyValuePair<string, AliasAndUsingDirective> kvp) => kvp.Value.Alias.GetPublicSymbol()), imports.ExternAliases.SelectAsArray((AliasAndExternAliasDirective e) => e.Alias.GetPublicSymbol()), imports.Usings.SelectAsArray((NamespaceOrTypeAndUsingDirective n) => new ImportedNamespaceOrType(n.NamespaceOrType.GetPublicSymbol(), n.UsingDirectiveReference)), ImmutableArray<ImportedXmlNamespace>.Empty));
			}
		}
		return instance.ToImmutableAndFree();
	}

	protected sealed override bool IsAccessibleCore(int position, ISymbol symbol)
	{
		return IsAccessible(position, symbol.EnsureCSharpSymbolOrNull("symbol"));
	}

	protected sealed override bool IsEventUsableAsFieldCore(int position, IEventSymbol symbol)
	{
		return IsEventUsableAsField(position, symbol.EnsureCSharpSymbolOrNull("symbol"));
	}

	public sealed override NullableContext GetNullableContext(int position)
	{
		CSharpSyntaxTree syntaxTree = (CSharpSyntaxTree)Root.SyntaxTree;
		NullableContextOptions? lazyDefaultState = null;
		NullableContextState nullableContextState = syntaxTree.GetNullableContextState(position);
		return (NullableContext)((nullableContextState.AnnotationsState switch
		{
			NullableContextState.State.Enabled => 2, 
			NullableContextState.State.Disabled => 0, 
			_ => (!getDefaultState().AnnotationsEnabled()) ? 8 : 10, 
		}) | (nullableContextState.WarningsState switch
		{
			NullableContextState.State.Enabled => 1, 
			NullableContextState.State.Disabled => 0, 
			_ => (!getDefaultState().WarningsEnabled()) ? 4 : 5, 
		}));
		NullableContextOptions getDefaultState()
		{
			NullableContextOptions valueOrDefault = lazyDefaultState.GetValueOrDefault();
			if (!lazyDefaultState.HasValue)
			{
				valueOrDefault = ((!syntaxTree.IsGeneratedCode(Compilation.Options.SyntaxTreeOptionsProvider, CancellationToken.None)) ? Compilation.Options.NullableContextOptions : NullableContextOptions.Disable);
				lazyDefaultState = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}
}
