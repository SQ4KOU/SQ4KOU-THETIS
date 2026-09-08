using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class BuckStopsHereBinder : Binder
{
	internal readonly FileIdentifier? AssociatedFileIdentifier;

	internal override ImportChain? ImportChain => null;

	internal override QuickAttributeChecker QuickAttributeChecker => Microsoft.CodeAnalysis.CSharp.Symbols.QuickAttributeChecker.Predefined;

	protected override bool InExecutableBinder => false;

	protected override SyntaxNode? EnclosingNameofArgument => null;

	internal override bool IsInsideNameof => false;

	internal override ConstantFieldsInProgress ConstantFieldsInProgress => Microsoft.CodeAnalysis.CSharp.ConstantFieldsInProgress.Empty;

	internal override ConsList<FieldSymbol> FieldsBeingBound => ConsList<FieldSymbol>.Empty;

	internal override LocalSymbol? LocalInProgress => null;

	internal override bool IsInMethodBody => false;

	internal override bool IsDirectlyInIterator => false;

	internal override bool IsIndirectlyInIterator => false;

	internal override GeneratedLabelSymbol? BreakLabel => null;

	internal override GeneratedLabelSymbol? ContinueLabel => null;

	internal override BoundExpression? ConditionalReceiverExpression => null;

	internal override Symbol? ContainingMemberOrLambda => null;

	internal override ImmutableHashSet<Symbol> LockedOrDisposedVariables => ImmutableHashSet.Create<Symbol>();

	internal BuckStopsHereBinder(CSharpCompilation compilation, FileIdentifier? associatedFileIdentifier)
		: base(compilation)
	{
		AssociatedFileIdentifier = associatedFileIdentifier;
	}

	protected override SourceLocalSymbol? LookupLocal(SyntaxToken nameToken)
	{
		return null;
	}

	protected override LocalFunctionSymbol? LookupLocalFunction(SyntaxToken nameToken)
	{
		return null;
	}

	internal override bool IsAccessibleHelper(Symbol symbol, TypeSymbol accessThroughType, out bool failedThroughTypeCheck, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, ConsList<TypeSymbol> basesBeingResolved)
	{
		failedThroughTypeCheck = false;
		return Binder.IsSymbolAccessibleConditional(symbol, base.Compilation.Assembly, ref useSiteInfo);
	}

	protected override bool IsUnboundTypeAllowed(GenericNameSyntax syntax)
	{
		return false;
	}

	internal override TypeWithAnnotations GetIteratorElementType()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/BuckStopsHereBinder.cs", 155);
	}

	internal override bool AreNullableAnnotationsGloballyEnabled()
	{
		return GetGlobalAnnotationState();
	}

	internal override Binder? GetBinder(SyntaxNode node)
	{
		return null;
	}

	internal override ImmutableArray<LocalSymbol> GetDeclaredLocalsForScope(SyntaxNode scopeDesignator)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/BuckStopsHereBinder.cs", 178);
	}

	internal override ImmutableArray<LocalFunctionSymbol> GetDeclaredLocalFunctionsForScope(CSharpSyntaxNode scopeDesignator)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/BuckStopsHereBinder.cs", 183);
	}

	internal override BoundStatement BindSwitchStatementCore(SwitchStatementSyntax node, Binder originalBinder, BindingDiagnosticBag diagnostics)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/BuckStopsHereBinder.cs", 189);
	}

	internal override BoundExpression BindSwitchExpressionCore(SwitchExpressionSyntax node, Binder originalBinder, BindingDiagnosticBag diagnostics)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/BuckStopsHereBinder.cs", 195);
	}

	internal override void BindPatternSwitchLabelForInference(CasePatternSwitchLabelSyntax node, BindingDiagnosticBag diagnostics)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/BuckStopsHereBinder.cs", 201);
	}

	internal override BoundSwitchExpressionArm BindSwitchExpressionArm(SwitchExpressionArmSyntax node, TypeSymbol switchGoverningType, BindingDiagnosticBag diagnostics)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/BuckStopsHereBinder.cs", 207);
	}

	internal override BoundForStatement BindForParts(BindingDiagnosticBag diagnostics, Binder originalBinder)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/BuckStopsHereBinder.cs", 213);
	}

	internal override BoundStatement BindForEachParts(BindingDiagnosticBag diagnostics, Binder originalBinder)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/BuckStopsHereBinder.cs", 219);
	}

	internal override BoundStatement BindForEachDeconstruction(BindingDiagnosticBag diagnostics, Binder originalBinder)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/BuckStopsHereBinder.cs", 225);
	}

	internal override BoundWhileStatement BindWhileParts(BindingDiagnosticBag diagnostics, Binder originalBinder)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/BuckStopsHereBinder.cs", 231);
	}

	internal override BoundDoStatement BindDoParts(BindingDiagnosticBag diagnostics, Binder originalBinder)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/BuckStopsHereBinder.cs", 237);
	}

	internal override BoundStatement BindUsingStatementParts(BindingDiagnosticBag diagnostics, Binder originalBinder)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/BuckStopsHereBinder.cs", 243);
	}

	internal override BoundStatement BindLockStatementParts(BindingDiagnosticBag diagnostics, Binder originalBinder)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/BuckStopsHereBinder.cs", 249);
	}
}
