using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class SpeculativeSemanticModelWithMemberModel : PublicSemanticModel
{
	private readonly SyntaxTreeSemanticModel _parentSemanticModel;

	private readonly int _position;

	private readonly NullableWalker.SnapshotManager? _parentSnapshotManagerOpt;

	private readonly MemberSemanticModel _memberModel;

	private ImmutableDictionary<CSharpSyntaxNode, MemberSemanticModel> _childMemberModels = ImmutableDictionary<CSharpSyntaxNode, MemberSemanticModel>.Empty;

	internal NullableWalker.SnapshotManager? ParentSnapshotManagerOpt => _parentSnapshotManagerOpt;

	public override bool IsSpeculativeSemanticModel => true;

	public override int OriginalPositionForSpeculation => _position;

	public override CSharpSemanticModel ParentModel => _parentSemanticModel;

	public override CSharpCompilation Compilation => _parentSemanticModel.Compilation;

	internal override CSharpSyntaxNode Root => _memberModel.Root;

	public override SyntaxTree SyntaxTree => _memberModel.SyntaxTree;

	[Experimental("RSEXPERIMENTAL001", UrlFormat = "https://github.com/dotnet/roslyn/issues/70609")]
	public override bool NullableAnalysisIsDisabled => _parentSemanticModel.NullableAnalysisIsDisabled;

	public override bool IgnoresAccessibility => _parentSemanticModel.IgnoresAccessibility;

	private SpeculativeSemanticModelWithMemberModel(SyntaxTreeSemanticModel parentSemanticModel, int position, NullableWalker.SnapshotManager? snapshotManagerOpt)
	{
		_parentSemanticModel = parentSemanticModel;
		_position = position;
		_parentSnapshotManagerOpt = snapshotManagerOpt;
		_memberModel = null;
	}

	public SpeculativeSemanticModelWithMemberModel(SyntaxTreeSemanticModel parentSemanticModel, int position, AttributeSyntax syntax, NamedTypeSymbol attributeType, AliasSymbol aliasOpt, Binder rootBinder, ImmutableDictionary<Symbol, Symbol>? parentRemappedSymbolsOpt)
		: this(parentSemanticModel, position, null)
	{
		_memberModel = new AttributeSemanticModel(syntax, attributeType, getAttributeTargetFromPosition(position, parentSemanticModel), aliasOpt, rootBinder, this, parentRemappedSymbolsOpt);
		static Symbol? getAttributeTargetFromPosition(int position2, SyntaxTreeSemanticModel model)
		{
			SyntaxNode syntaxNode = model.SyntaxTree.GetRoot().FindToken(position2).Parent?.FirstAncestorOrSelf<AttributeListSyntax>()?.Parent;
			if (syntaxNode != null)
			{
				return model.GetDeclaredSymbolForNode(syntaxNode).GetSymbol();
			}
			return null;
		}
	}

	public SpeculativeSemanticModelWithMemberModel(SyntaxTreeSemanticModel parentSemanticModel, int position, Symbol owner, EqualsValueClauseSyntax syntax, Binder rootBinder, ImmutableDictionary<Symbol, Symbol>? parentRemappedSymbolsOpt)
		: this(parentSemanticModel, position, null)
	{
		_memberModel = new InitializerSemanticModel(syntax, owner, rootBinder, this, parentRemappedSymbolsOpt);
	}

	public SpeculativeSemanticModelWithMemberModel(SyntaxTreeSemanticModel parentModel, int position, Symbol owner, TypeSyntax type, Binder rootBinder, ImmutableDictionary<Symbol, Symbol>? parentRemappedSymbolsOpt, NullableWalker.SnapshotManager? snapshotManagerOpt)
		: this(parentModel, position, snapshotManagerOpt)
	{
		_memberModel = new MemberSemanticModel.SpeculativeMemberSemanticModel(this, owner, type, rootBinder, parentRemappedSymbolsOpt);
	}

	public SpeculativeSemanticModelWithMemberModel(SyntaxTreeSemanticModel parentSemanticModel, int position, MethodSymbol owner, CSharpSyntaxNode syntax, Binder rootBinder, ImmutableDictionary<Symbol, Symbol>? parentRemappedSymbolsOpt, NullableWalker.SnapshotManager? snapshotManagerOpt)
		: this(parentSemanticModel, position, snapshotManagerOpt)
	{
		_memberModel = new MethodBodySemanticModel(owner, rootBinder, syntax, this, parentRemappedSymbolsOpt);
	}

	private MemberSemanticModel GetEnclosingMemberModel(int position)
	{
		SyntaxNode parent = Root.FindTokenIncludingCrefAndNameAttributes(position).Parent;
		if (parent != null)
		{
			return GetEnclosingMemberModel(parent);
		}
		return _memberModel;
	}

	private MemberSemanticModel GetEnclosingMemberModel(SyntaxNode node)
	{
		if (node.SyntaxTree != SyntaxTree)
		{
			return _memberModel;
		}
		SyntaxNode syntaxNode = node.FirstAncestorOrSelf(delegate(SyntaxNode n)
		{
			SyntaxKind syntaxKind = n.Kind();
			return (syntaxKind == SyntaxKind.Attribute || syntaxKind == SyntaxKind.Parameter) ? true : false;
		});
		if (syntaxNode == null || syntaxNode == Root || syntaxNode.Parent == null || !Root.Span.Contains(syntaxNode.Span))
		{
			return _memberModel;
		}
		MemberSemanticModel enclosingMemberModel = GetEnclosingMemberModel(syntaxNode.Parent);
		if (!(syntaxNode is AttributeSyntax attribute))
		{
			if (syntaxNode is ParameterSyntax paramDecl)
			{
				return GetOrAddModelForParameter(node, enclosingMemberModel, paramDecl);
			}
			ExceptionUtilities.UnexpectedValue(syntaxNode);
			return enclosingMemberModel;
		}
		return GetOrAddModelForAttribute(enclosingMemberModel, attribute);
	}

	private MemberSemanticModel GetOrAddModelForAttribute(MemberSemanticModel containing, AttributeSyntax attribute)
	{
		return ImmutableInterlocked.GetOrAdd(ref _childMemberModels, attribute, (CSharpSyntaxNode node, (Binder binder, MemberSemanticModel model) binderAndModel) => CreateModelForAttribute(binderAndModel.binder, (AttributeSyntax)node, binderAndModel.model), (containing.GetEnclosingBinder(attribute.SpanStart), containing));
	}

	private MemberSemanticModel GetOrAddModelForParameter(SyntaxNode node, MemberSemanticModel containing, ParameterSyntax paramDecl)
	{
		EqualsValueClauseSyntax equalsValueClauseSyntax = paramDecl.Default;
		if (equalsValueClauseSyntax != null && equalsValueClauseSyntax.FullSpan.Contains(node.Span))
		{
			ParameterSymbol symbol = containing.GetDeclaredSymbol(paramDecl).GetSymbol<ParameterSymbol>();
			if ((object)symbol != null)
			{
				return ImmutableInterlocked.GetOrAdd(ref _childMemberModels, equalsValueClauseSyntax, (CSharpSyntaxNode equalsValue, (CSharpCompilation compilation, ParameterSyntax paramDecl, ParameterSymbol parameterSymbol, MemberSemanticModel containing) tuple) => InitializerSemanticModel.Create(this, tuple.paramDecl, tuple.parameterSymbol, tuple.containing.GetEnclosingBinder(tuple.paramDecl.SpanStart).CreateBinderForParameterDefaultValue(tuple.parameterSymbol, (EqualsValueClauseSyntax)equalsValue), tuple.containing.GetRemappedSymbols()), (Compilation, paramDecl, symbol, containing));
			}
		}
		return containing;
	}

	internal override MemberSemanticModel GetMemberModel(SyntaxNode node)
	{
		return GetEnclosingMemberModel(node).GetMemberModel(node);
	}

	public override Conversion ClassifyConversion(ExpressionSyntax expression, ITypeSymbol destination, bool isExplicitInSource = false)
	{
		return GetEnclosingMemberModel(expression).ClassifyConversion(expression, destination, isExplicitInSource);
	}

	internal override Conversion ClassifyConversionForCast(ExpressionSyntax expression, TypeSymbol destination)
	{
		return GetEnclosingMemberModel(expression).ClassifyConversionForCast(expression, destination);
	}

	public override ImmutableArray<Diagnostic> GetSyntaxDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotSupportedException();
	}

	public override ImmutableArray<Diagnostic> GetDeclarationDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotSupportedException();
	}

	public override ImmutableArray<Diagnostic> GetMethodBodyDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotSupportedException();
	}

	public override ImmutableArray<Diagnostic> GetDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotSupportedException();
	}

	public override INamespaceSymbol GetDeclaredSymbol(NamespaceDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override INamespaceSymbol GetDeclaredSymbol(FileScopedNamespaceDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override INamedTypeSymbol GetDeclaredSymbol(BaseTypeDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override INamedTypeSymbol GetDeclaredSymbol(DelegateDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override IFieldSymbol GetDeclaredSymbol(EnumMemberDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override IMethodSymbol GetDeclaredSymbol(LocalFunctionStatementSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override ISymbol GetDeclaredSymbol(MemberDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override IMethodSymbol GetDeclaredSymbol(CompilationUnitSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override IMethodSymbol GetDeclaredSymbol(BaseMethodDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override ISymbol GetDeclaredSymbol(BasePropertyDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override IPropertySymbol GetDeclaredSymbol(PropertyDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override IPropertySymbol GetDeclaredSymbol(IndexerDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override IEventSymbol GetDeclaredSymbol(EventDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override IMethodSymbol GetDeclaredSymbol(AccessorDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override IMethodSymbol GetDeclaredSymbol(ArrowExpressionClauseSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override ISymbol GetDeclaredSymbol(VariableDeclaratorSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override ISymbol GetDeclaredSymbol(SingleVariableDesignationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	internal override LocalSymbol GetAdjustedLocalSymbol(SourceLocalSymbol local)
	{
		return _memberModel.GetAdjustedLocalSymbol(local);
	}

	public override ILabelSymbol GetDeclaredSymbol(LabeledStatementSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override ILabelSymbol GetDeclaredSymbol(SwitchLabelSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override IAliasSymbol GetDeclaredSymbol(UsingDirectiveSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override IAliasSymbol GetDeclaredSymbol(ExternAliasDirectiveSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override IParameterSymbol GetDeclaredSymbol(ParameterSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	internal override ImmutableArray<ISymbol> GetDeclaredSymbols(BaseFieldDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declarationSyntax).GetDeclaredSymbols(declarationSyntax, cancellationToken);
	}

	public override ITypeParameterSymbol GetDeclaredSymbol(TypeParameterSyntax typeParameter, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(typeParameter).GetDeclaredSymbol(typeParameter, cancellationToken);
	}

	public override IRangeVariableSymbol GetDeclaredSymbol(JoinIntoClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(node).GetDeclaredSymbol(node, cancellationToken);
	}

	public override IRangeVariableSymbol GetDeclaredSymbol(QueryClauseSyntax queryClause, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(queryClause).GetDeclaredSymbol(queryClause, cancellationToken);
	}

	public override IRangeVariableSymbol GetDeclaredSymbol(QueryContinuationSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(node).GetDeclaredSymbol(node, cancellationToken);
	}

	public override AwaitExpressionInfo GetAwaitExpressionInfo(AwaitExpressionSyntax node)
	{
		return GetEnclosingMemberModel(node).GetAwaitExpressionInfo(node);
	}

	public override AwaitExpressionInfo GetAwaitExpressionInfo(LocalDeclarationStatementSyntax node)
	{
		return GetEnclosingMemberModel(node).GetAwaitExpressionInfo(node);
	}

	public override AwaitExpressionInfo GetAwaitExpressionInfo(UsingStatementSyntax node)
	{
		return GetEnclosingMemberModel(node).GetAwaitExpressionInfo(node);
	}

	public override ForEachStatementInfo GetForEachStatementInfo(ForEachStatementSyntax node)
	{
		return GetEnclosingMemberModel(node).GetForEachStatementInfo(node);
	}

	public override ForEachStatementInfo GetForEachStatementInfo(CommonForEachStatementSyntax node)
	{
		return GetEnclosingMemberModel(node).GetForEachStatementInfo(node);
	}

	public override DeconstructionInfo GetDeconstructionInfo(AssignmentExpressionSyntax node)
	{
		return GetEnclosingMemberModel(node).GetDeconstructionInfo(node);
	}

	public override DeconstructionInfo GetDeconstructionInfo(ForEachVariableStatementSyntax node)
	{
		return GetEnclosingMemberModel(node).GetDeconstructionInfo(node);
	}

	public override QueryClauseInfo GetQueryClauseInfo(QueryClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(node).GetQueryClauseInfo(node, cancellationToken);
	}

	public override IPropertySymbol GetDeclaredSymbol(AnonymousObjectMemberDeclaratorSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declaratorSyntax).GetDeclaredSymbol(declaratorSyntax, cancellationToken);
	}

	public override INamedTypeSymbol GetDeclaredSymbol(AnonymousObjectCreationExpressionSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declaratorSyntax).GetDeclaredSymbol(declaratorSyntax, cancellationToken);
	}

	public override INamedTypeSymbol GetDeclaredSymbol(TupleExpressionSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declaratorSyntax).GetDeclaredSymbol(declaratorSyntax, cancellationToken);
	}

	public override ISymbol GetDeclaredSymbol(ArgumentSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(declaratorSyntax).GetDeclaredSymbol(declaratorSyntax, cancellationToken);
	}

	internal override IOperation? GetOperationWorker(CSharpSyntaxNode node, CancellationToken cancellationToken)
	{
		return GetEnclosingMemberModel(node).GetOperationWorker(node, cancellationToken);
	}

	internal override SymbolInfo GetSymbolInfoWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(node).GetSymbolInfoWorker(node, options, cancellationToken);
	}

	internal override CSharpTypeInfo GetTypeInfoWorker(CSharpSyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(node).GetTypeInfoWorker(node, cancellationToken);
	}

	internal override ImmutableArray<Symbol> GetMemberGroupWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(node).GetMemberGroupWorker(node, options, cancellationToken);
	}

	internal override ImmutableArray<IPropertySymbol> GetIndexerGroupWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(node).GetIndexerGroupWorker(node, options, cancellationToken);
	}

	internal override Optional<object> GetConstantValueWorker(CSharpSyntaxNode node, CancellationToken cancellationToken)
	{
		return GetEnclosingMemberModel(node).GetConstantValueWorker(node, cancellationToken);
	}

	internal override SymbolInfo GetCollectionInitializerSymbolInfoWorker(InitializerExpressionSyntax collectionInitializer, ExpressionSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(collectionInitializer).GetCollectionInitializerSymbolInfoWorker(collectionInitializer, node, cancellationToken);
	}

	public override SymbolInfo GetSymbolInfo(OrderingSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(node).GetSymbolInfo(node, cancellationToken);
	}

	public override SymbolInfo GetSymbolInfo(SelectOrGroupClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(node).GetSymbolInfo(node, cancellationToken);
	}

	public override TypeInfo GetTypeInfo(SelectOrGroupClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingMemberModel(node).GetTypeInfo(node, cancellationToken);
	}

	internal override Binder GetEnclosingBinderInternal(int position)
	{
		return GetEnclosingMemberModel(position).GetEnclosingBinderInternal(position);
	}

	internal override Symbol RemapSymbolIfNecessaryCore(Symbol symbol)
	{
		return _memberModel.RemapSymbolIfNecessaryCore(symbol);
	}

	internal sealed override Func<SyntaxNode, bool> GetSyntaxNodesToAnalyzeFilter(SyntaxNode declaredNode, ISymbol declaredSymbol)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Compilation/SpeculativeSemanticModelWithMemberModel.cs", 517);
	}

	internal override bool ShouldSkipSyntaxNodeAnalysis(SyntaxNode node, ISymbol containingSymbol)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Compilation/SpeculativeSemanticModelWithMemberModel.cs", 522);
	}

	internal override BoundNode Bind(Binder binder, CSharpSyntaxNode node, BindingDiagnosticBag diagnostics)
	{
		return GetEnclosingMemberModel(node).Bind(binder, node, diagnostics);
	}

	internal override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, ConstructorInitializerSyntax constructorInitializer, out PublicSemanticModel? speculativeModel)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Compilation/SpeculativeSemanticModelWithMemberModel.cs", 532);
	}

	internal override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, PrimaryConstructorBaseTypeSyntax constructorInitializer, out PublicSemanticModel? speculativeModel)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Compilation/SpeculativeSemanticModelWithMemberModel.cs", 537);
	}

	internal override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, EqualsValueClauseSyntax initializer, out PublicSemanticModel? speculativeModel)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Compilation/SpeculativeSemanticModelWithMemberModel.cs", 542);
	}

	internal override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, ArrowExpressionClauseSyntax expressionBody, out PublicSemanticModel? speculativeModel)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Compilation/SpeculativeSemanticModelWithMemberModel.cs", 547);
	}

	internal override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, StatementSyntax statement, out PublicSemanticModel? speculativeModel)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Compilation/SpeculativeSemanticModelWithMemberModel.cs", 552);
	}

	internal override bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, BaseMethodDeclarationSyntax method, out PublicSemanticModel? speculativeModel)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Compilation/SpeculativeSemanticModelWithMemberModel.cs", 557);
	}

	internal override bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, AccessorDeclarationSyntax accessor, out PublicSemanticModel? speculativeModel)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Compilation/SpeculativeSemanticModelWithMemberModel.cs", 562);
	}

	internal sealed override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, TypeSyntax type, SpeculativeBindingOption bindingOption, out PublicSemanticModel speculativeModel)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Compilation/SpeculativeSemanticModelWithMemberModel.cs", 567);
	}

	internal sealed override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, CrefSyntax crefSyntax, out PublicSemanticModel speculativeModel)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Compilation/SpeculativeSemanticModelWithMemberModel.cs", 572);
	}

	internal override BoundExpression GetSpeculativelyBoundExpression(int position, ExpressionSyntax expression, SpeculativeBindingOption bindingOption, out Binder binder, out ImmutableArray<Symbol> crefSymbols)
	{
		return GetEnclosingMemberModel(CheckAndAdjustPosition(position)).GetSpeculativelyBoundExpression(position, expression, bindingOption, out binder, out crefSymbols);
	}
}
