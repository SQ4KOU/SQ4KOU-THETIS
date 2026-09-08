using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal class SwitchBinder : LocalScopeBinder
{
	protected readonly SwitchStatementSyntax SwitchSyntax;

	private readonly GeneratedLabelSymbol _breakLabel;

	private BoundExpression _switchGoverningExpression;

	private ImmutableArray<Diagnostic> _switchGoverningDiagnostics;

	private ImmutableArray<AssemblySymbol> _switchGoverningDependencies;

	private Dictionary<object, SourceLabelSymbol> _lazySwitchLabelsMap;

	private static readonly object s_defaultKey = new object();

	private static readonly object s_nullKey = new object();

	private Dictionary<SyntaxNode, LabelSymbol> _labelsByNode;

	protected bool PatternsEnabled => ((CSharpParseOptions)SwitchSyntax.SyntaxTree.Options)?.IsFeatureEnabled(MessageID.IDS_FeaturePatternMatching) ?? true;

	protected BoundExpression SwitchGoverningExpression
	{
		get
		{
			EnsureSwitchGoverningExpressionAndDiagnosticsBound();
			return _switchGoverningExpression;
		}
	}

	protected TypeSymbol SwitchGoverningType => SwitchGoverningExpression.Type;

	protected ReadOnlyBindingDiagnostic<AssemblySymbol> SwitchGoverningDiagnostics
	{
		get
		{
			EnsureSwitchGoverningExpressionAndDiagnosticsBound();
			return new ReadOnlyBindingDiagnostic<AssemblySymbol>(_switchGoverningDiagnostics, _switchGoverningDependencies);
		}
	}

	private Dictionary<object, SourceLabelSymbol> LabelsByValue
	{
		get
		{
			if (_lazySwitchLabelsMap == null && Labels.Length > 0)
			{
				_lazySwitchLabelsMap = BuildLabelsByValue(Labels);
			}
			return _lazySwitchLabelsMap;
		}
	}

	internal override bool IsLocalFunctionsScopeBinder => true;

	internal override GeneratedLabelSymbol BreakLabel => _breakLabel;

	internal override bool IsLabelsScopeBinder => true;

	internal override SyntaxNode ScopeDesignator => SwitchSyntax;

	protected Dictionary<SyntaxNode, LabelSymbol> LabelsByNode
	{
		get
		{
			if (_labelsByNode == null)
			{
				Dictionary<SyntaxNode, LabelSymbol> dictionary = new Dictionary<SyntaxNode, LabelSymbol>();
				foreach (LabelSymbol label in Labels)
				{
					SyntaxNode syntaxNode = ((SourceLabelSymbol)label).IdentifierNodeOrToken.AsNode();
					if (syntaxNode != null)
					{
						dictionary.Add(syntaxNode, label);
					}
				}
				_labelsByNode = dictionary;
			}
			return _labelsByNode;
		}
	}

	private SwitchBinder(Binder next, SwitchStatementSyntax switchSyntax)
		: base(next)
	{
		SwitchSyntax = switchSyntax;
		_breakLabel = new GeneratedLabelSymbol("break");
	}

	private void EnsureSwitchGoverningExpressionAndDiagnosticsBound()
	{
		if (_switchGoverningExpression == null)
		{
			BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
			BoundExpression value = BindSwitchGoverningExpression(instance);
			ReadOnlyBindingDiagnostic<AssemblySymbol> readOnlyBindingDiagnostic = instance.ToReadOnlyAndFree();
			ImmutableInterlocked.InterlockedInitialize(ref _switchGoverningDiagnostics, readOnlyBindingDiagnostic.Diagnostics);
			ImmutableInterlocked.InterlockedInitialize(ref _switchGoverningDependencies, readOnlyBindingDiagnostic.Dependencies);
			Interlocked.CompareExchange(ref _switchGoverningExpression, value, null);
		}
	}

	private static Dictionary<object, SourceLabelSymbol> BuildLabelsByValue(ImmutableArray<LabelSymbol> labels)
	{
		Dictionary<object, SourceLabelSymbol> dictionary = new Dictionary<object, SourceLabelSymbol>(labels.Length, new SwitchConstantValueHelper.SwitchLabelsComparer());
		foreach (SourceLabelSymbol item in labels)
		{
			SyntaxKind syntaxKind = item.IdentifierNodeOrToken.Kind();
			if (syntaxKind != SyntaxKind.IdentifierToken)
			{
				ConstantValue switchCaseLabelConstant = item.SwitchCaseLabelConstant;
				object key = (((object)switchCaseLabelConstant != null && !switchCaseLabelConstant.IsBad) ? KeyForConstant(switchCaseLabelConstant) : ((syntaxKind != SyntaxKind.DefaultSwitchLabel) ? item.IdentifierNodeOrToken.AsNode() : s_defaultKey));
				if (!dictionary.ContainsKey(key))
				{
					dictionary.Add(key, item);
				}
			}
		}
		return dictionary;
	}

	protected override ImmutableArray<LocalSymbol> BuildLocals()
	{
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		foreach (SwitchSectionSyntax section in SwitchSyntax.Sections)
		{
			instance.AddRange(BuildLocals(section.Statements, GetBinder(section)));
		}
		return instance.ToImmutableAndFree();
	}

	protected override ImmutableArray<LocalFunctionSymbol> BuildLocalFunctions()
	{
		ArrayBuilder<LocalFunctionSymbol> instance = ArrayBuilder<LocalFunctionSymbol>.GetInstance();
		foreach (SwitchSectionSyntax section in SwitchSyntax.Sections)
		{
			instance.AddRange(BuildLocalFunctions(section.Statements));
		}
		return instance.ToImmutableAndFree();
	}

	protected override ImmutableArray<LabelSymbol> BuildLabels()
	{
		ArrayBuilder<LabelSymbol> labels = ArrayBuilder<LabelSymbol>.GetInstance();
		foreach (SwitchSectionSyntax section in SwitchSyntax.Sections)
		{
			BuildSwitchLabels(section.Labels, GetBinder(section), labels, BindingDiagnosticBag.Discarded);
			BuildLabels(section.Statements, ref labels);
		}
		return labels.ToImmutableAndFree();
	}

	private void BuildSwitchLabels(SyntaxList<SwitchLabelSyntax> labelsSyntax, Binder sectionBinder, ArrayBuilder<LabelSymbol> labels, BindingDiagnosticBag tempDiagnosticBag)
	{
		foreach (SwitchLabelSyntax item in labelsSyntax)
		{
			ConstantValue constantValueOpt = null;
			switch (item.Kind())
			{
			case SyntaxKind.CaseSwitchLabel:
			{
				CaseSwitchLabelSyntax caseSwitchLabelSyntax = (CaseSwitchLabelSyntax)item;
				BoundExpression boundExpression = sectionBinder.BindTypeOrRValue(caseSwitchLabelSyntax.Value, tempDiagnosticBag);
				if (!(boundExpression is BoundTypeExpression))
				{
					ConvertCaseExpression(item, boundExpression, out constantValueOpt, tempDiagnosticBag);
				}
				break;
			}
			case SyntaxKind.CasePatternSwitchLabel:
			{
				CasePatternSwitchLabelSyntax casePatternSwitchLabelSyntax = (CasePatternSwitchLabelSyntax)item;
				sectionBinder.BindPattern(casePatternSwitchLabelSyntax.Pattern, SwitchGoverningType, permitDesignations: true, item.HasErrors, tempDiagnosticBag);
				break;
			}
			}
			labels.Add(new SourceLabelSymbol((MethodSymbol)ContainingMemberOrLambda, item, constantValueOpt));
		}
	}

	protected BoundExpression ConvertCaseExpression(CSharpSyntaxNode node, BoundExpression caseExpression, out ConstantValue constantValueOpt, BindingDiagnosticBag diagnostics, bool isGotoCaseExpr = false)
	{
		bool hasErrors = false;
		if (isGotoCaseExpr)
		{
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = GetNewCompoundUseSiteInfo(diagnostics);
			Conversion conversion = base.Conversions.ClassifyConversionFromExpression(caseExpression, SwitchGoverningType, base.CheckOverflowAtRuntime, ref useSiteInfo);
			diagnostics.Add(node, useSiteInfo);
			if (!conversion.IsValid)
			{
				GenerateImplicitConversionError(diagnostics, node, conversion, caseExpression, SwitchGoverningType);
				hasErrors = true;
			}
			else if (!conversion.IsImplicit)
			{
				diagnostics.Add(ErrorCode.WRN_GotoCaseShouldConvert, node.Location, SwitchGoverningType);
				hasErrors = true;
			}
			caseExpression = CreateConversion(caseExpression, conversion, SwitchGoverningType, diagnostics);
		}
		Conversion patternExpressionConversion;
		return ConvertPatternExpression(SwitchGoverningType, node, caseExpression, out constantValueOpt, hasErrors, diagnostics, out patternExpressionConversion);
	}

	protected static object KeyForConstant(ConstantValue constantValue)
	{
		if (!constantValue.IsNull)
		{
			return constantValue.Value;
		}
		return s_nullKey;
	}

	protected SourceLabelSymbol FindMatchingSwitchCaseLabel(ConstantValue constantValue, CSharpSyntaxNode labelSyntax)
	{
		object key = (((object)constantValue == null || constantValue.IsBad) ? labelSyntax : KeyForConstant(constantValue));
		return FindMatchingSwitchLabel(key);
	}

	private SourceLabelSymbol GetDefaultLabel()
	{
		return FindMatchingSwitchLabel(s_defaultKey);
	}

	private SourceLabelSymbol FindMatchingSwitchLabel(object key)
	{
		Dictionary<object, SourceLabelSymbol> labelsByValue = LabelsByValue;
		if (labelsByValue != null && labelsByValue.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	internal override ImmutableArray<LocalSymbol> GetDeclaredLocalsForScope(SyntaxNode scopeDesignator)
	{
		if (SwitchSyntax == scopeDesignator)
		{
			return Locals;
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/SwitchBinder.cs", 335);
	}

	internal override ImmutableArray<LocalFunctionSymbol> GetDeclaredLocalFunctionsForScope(CSharpSyntaxNode scopeDesignator)
	{
		if (SwitchSyntax == scopeDesignator)
		{
			return LocalFunctions;
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/SwitchBinder.cs", 345);
	}

	private BoundExpression BindSwitchGoverningExpression(BindingDiagnosticBag diagnostics)
	{
		ExpressionSyntax expression = SwitchSyntax.Expression;
		Binder binder = GetBinder(expression);
		BoundExpression boundExpression = binder.BindRValueWithoutTargetType(expression, diagnostics);
		TypeSymbol typeSymbol = boundExpression.Type;
		if ((object)typeSymbol != null && !typeSymbol.IsErrorType())
		{
			if (typeSymbol.IsValidV6SwitchGoverningType())
			{
				if (typeSymbol.SpecialType == SpecialType.System_Boolean)
				{
					Binder.CheckFeatureAvailability(expression, MessageID.IDS_FeatureSwitchOnBool, diagnostics);
				}
				return boundExpression;
			}
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = GetNewCompoundUseSiteInfo(diagnostics);
			Conversion conversion = binder.Conversions.ClassifyImplicitUserDefinedConversionForV6SwitchGoverningType(typeSymbol, out var switchGoverningType, ref useSiteInfo);
			diagnostics.Add(expression, useSiteInfo);
			if (conversion.IsValid)
			{
				return binder.CreateConversion(expression, boundExpression, conversion, isCast: false, null, switchGoverningType, diagnostics);
			}
			if (!typeSymbol.IsVoidType())
			{
				if (!PatternsEnabled)
				{
					diagnostics.Add(ErrorCode.ERR_V6SwitchGoverningTypeValueExpected, expression.Location);
				}
				return boundExpression;
			}
			typeSymbol = CreateErrorType(typeSymbol.Name);
		}
		if (!boundExpression.HasAnyErrors)
		{
			diagnostics.Add(ErrorCode.ERR_SwitchExpressionValueExpected, expression.Location, boundExpression.Display);
		}
		return new BoundBadExpression(expression, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray.Create(boundExpression), typeSymbol ?? CreateErrorType());
	}

	internal BoundStatement BindGotoCaseOrDefault(GotoStatementSyntax node, Binder gotoBinder, BindingDiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = null;
		if (!node.HasErrors)
		{
			ConstantValue constantValueOpt = null;
			bool flag = false;
			SourceLabelSymbol sourceLabelSymbol;
			if (node.Expression != null)
			{
				boundExpression = gotoBinder.BindValue(node.Expression, diagnostics, BindValueKind.RValue);
				boundExpression = ConvertCaseExpression(node, boundExpression, out constantValueOpt, diagnostics, isGotoCaseExpr: true);
				flag = flag || boundExpression.HasAnyErrors;
				if (!flag && constantValueOpt == null)
				{
					diagnostics.Add(ErrorCode.ERR_ConstantExpected, node.Location);
					flag = true;
				}
				ConstantValueUtils.CheckLangVersionForConstantValue(boundExpression, diagnostics);
				sourceLabelSymbol = FindMatchingSwitchCaseLabel(constantValueOpt, node);
			}
			else
			{
				sourceLabelSymbol = GetDefaultLabel();
			}
			if ((object)sourceLabelSymbol != null)
			{
				return new BoundGotoStatement(node, sourceLabelSymbol, boundExpression, null, flag);
			}
			if (!flag)
			{
				string text = SyntaxFacts.GetText(node.CaseOrDefaultKeyword.Kind());
				if (node.Kind() == SyntaxKind.GotoCaseStatement)
				{
					text = text + " " + constantValueOpt.Value;
				}
				text += ":";
				diagnostics.Add(ErrorCode.ERR_LabelNotFound, node.Location, text);
				flag = true;
			}
		}
		return new BoundBadStatement(node, (boundExpression != null) ? ImmutableArray.Create((BoundNode)boundExpression) : ImmutableArray<BoundNode>.Empty, hasErrors: true);
	}

	internal static SwitchBinder Create(Binder next, SwitchStatementSyntax switchSyntax)
	{
		return new SwitchBinder(next, switchSyntax);
	}

	internal override BoundStatement BindSwitchStatementCore(SwitchStatementSyntax node, Binder originalBinder, BindingDiagnosticBag diagnostics)
	{
		if (node.Sections.Count == 0)
		{
			diagnostics.Add(ErrorCode.WRN_EmptySwitch, node.OpenBraceToken.GetLocation());
		}
		BoundExpression switchGoverningExpression = SwitchGoverningExpression;
		diagnostics.AddRange(SwitchGoverningDiagnostics, allowMismatchInDependencyAccumulation: true);
		ImmutableArray<BoundSwitchSection> switchSections = BindSwitchSections(originalBinder, diagnostics, out var defaultLabel);
		ImmutableArray<LocalSymbol> declaredLocalsForScope = GetDeclaredLocalsForScope(node);
		ImmutableArray<LocalFunctionSymbol> declaredLocalFunctionsForScope = GetDeclaredLocalFunctionsForScope(node);
		BoundDecisionDag boundDecisionDag = DecisionDagBuilder.CreateDecisionDagForSwitchStatement(base.Compilation, node, switchGoverningExpression, switchSections, defaultLabel?.Label ?? BreakLabel, diagnostics);
		CheckSwitchErrors(ref switchSections, boundDecisionDag, out var wasReported, diagnostics);
		boundDecisionDag = boundDecisionDag.SimplifyDecisionDagIfConstantInput(switchGoverningExpression);
		if (!wasReported && diagnostics.AccumulatesDiagnostics && DecisionDagBuilder.EnableRedundantPatternsCheck(base.Compilation))
		{
			DecisionDagBuilder.CheckRedundantPatternsForSwitchStatement(base.Compilation, node, switchGoverningExpression, switchSections, diagnostics);
		}
		ImmutableArray<MethodSymbol> innerLocalFunctions = ImmutableArray<MethodSymbol>.CastUp(declaredLocalFunctionsForScope);
		ImmutableArray<BoundSwitchSection> switchSections2 = switchSections;
		BoundSwitchLabel defaultLabel2 = defaultLabel;
		LabelSymbol breakLabel = BreakLabel;
		return new BoundSwitchStatement(node, switchGoverningExpression, declaredLocalsForScope, innerLocalFunctions, switchSections2, boundDecisionDag, defaultLabel2, breakLabel);
	}

	private void CheckSwitchErrors(ref ImmutableArray<BoundSwitchSection> switchSections, BoundDecisionDag decisionDag, out bool wasReported, BindingDiagnosticBag diagnostics)
	{
		wasReported = false;
		ImmutableHashSet<LabelSymbol> reachableLabels = decisionDag.ReachableLabels;
		if (!switchSections.Any((BoundSwitchSection s, ImmutableHashSet<LabelSymbol> arg) => s.SwitchLabels.Any(isSubsumed, arg), reachableLabels))
		{
			return;
		}
		ArrayBuilder<BoundSwitchSection> instance = ArrayBuilder<BoundSwitchSection>.GetInstance(switchSections.Length);
		bool flag = false;
		foreach (BoundSwitchSection switchSection in switchSections)
		{
			ArrayBuilder<BoundSwitchLabel> instance2 = ArrayBuilder<BoundSwitchLabel>.GetInstance(switchSection.SwitchLabels.Length);
			foreach (BoundSwitchLabel switchLabel in switchSection.SwitchLabels)
			{
				BoundSwitchLabel item = switchLabel;
				if (!switchLabel.HasErrors && isSubsumed(switchLabel, reachableLabels) && switchLabel.Syntax.Kind() != SyntaxKind.DefaultSwitchLabel)
				{
					SyntaxNode syntax = switchLabel.Syntax;
					if (!(syntax is CasePatternSwitchLabelSyntax casePatternSwitchLabelSyntax))
					{
						if (!(syntax is CaseSwitchLabelSyntax caseSwitchLabelSyntax))
						{
							throw ExceptionUtilities.UnexpectedValue(syntax.Kind());
						}
						if (switchLabel.Pattern is BoundConstantPattern boundConstantPattern && !boundConstantPattern.ConstantValue.IsBad && FindMatchingSwitchCaseLabel(boundConstantPattern.ConstantValue, caseSwitchLabelSyntax) != switchLabel.Label)
						{
							diagnostics.Add(ErrorCode.ERR_DuplicateCaseLabel, syntax.Location, boundConstantPattern.ConstantValue.GetValueToDisplay());
							wasReported = true;
						}
						else if (!switchLabel.Pattern.HasErrors && !flag)
						{
							diagnostics.Add(ErrorCode.ERR_SwitchCaseSubsumed, caseSwitchLabelSyntax.Value.Location);
							wasReported = true;
						}
					}
					else if (!casePatternSwitchLabelSyntax.Pattern.HasErrors && !flag)
					{
						diagnostics.Add(ErrorCode.ERR_SwitchCaseSubsumed, casePatternSwitchLabelSyntax.Pattern.Location);
						wasReported = true;
					}
					item = new BoundSwitchLabel(switchLabel.Syntax, switchLabel.Label, switchLabel.Pattern, switchLabel.WhenClause, hasErrors: true);
				}
				flag |= switchLabel.HasErrors;
				instance2.Add(item);
			}
			instance.Add(switchSection.Update(switchSection.Locals, instance2.ToImmutableAndFree(), switchSection.Statements));
		}
		switchSections = instance.ToImmutableAndFree();
		static bool isSubsumed(BoundSwitchLabel switchLabel, ImmutableHashSet<LabelSymbol> immutableHashSet)
		{
			return !immutableHashSet.Contains(switchLabel.Label);
		}
	}

	internal override void BindPatternSwitchLabelForInference(CasePatternSwitchLabelSyntax node, BindingDiagnosticBag diagnostics)
	{
		BoundSwitchLabel defaultLabel = null;
		BindSwitchSectionLabel(GetBinder(node.Parent), node, LabelsByNode[node], ref defaultLabel, diagnostics);
	}

	private ImmutableArray<BoundSwitchSection> BindSwitchSections(Binder originalBinder, BindingDiagnosticBag diagnostics, out BoundSwitchLabel defaultLabel)
	{
		ArrayBuilder<BoundSwitchSection> instance = ArrayBuilder<BoundSwitchSection>.GetInstance(SwitchSyntax.Sections.Count);
		defaultLabel = null;
		foreach (SwitchSectionSyntax section in SwitchSyntax.Sections)
		{
			BoundSwitchSection item = BindSwitchSection(section, originalBinder, ref defaultLabel, diagnostics);
			instance.Add(item);
		}
		return instance.ToImmutableAndFree();
	}

	private BoundSwitchSection BindSwitchSection(SwitchSectionSyntax node, Binder originalBinder, ref BoundSwitchLabel defaultLabel, BindingDiagnosticBag diagnostics)
	{
		ArrayBuilder<BoundSwitchLabel> instance = ArrayBuilder<BoundSwitchLabel>.GetInstance(node.Labels.Count);
		Binder binder = originalBinder.GetBinder(node);
		Dictionary<SyntaxNode, LabelSymbol> labelsByNode = LabelsByNode;
		foreach (SwitchLabelSyntax label2 in node.Labels)
		{
			LabelSymbol label = labelsByNode[label2];
			BoundSwitchLabel item = BindSwitchSectionLabel(binder, label2, label, ref defaultLabel, diagnostics);
			instance.Add(item);
		}
		ArrayBuilder<BoundStatement> instance2 = ArrayBuilder<BoundStatement>.GetInstance(node.Statements.Count);
		foreach (StatementSyntax statement in node.Statements)
		{
			BoundStatement boundStatement = binder.BindStatement(statement, diagnostics);
			if (ContainsUsingVariable(boundStatement))
			{
				diagnostics.Add(ErrorCode.ERR_UsingVarInSwitchCase, statement.Location);
			}
			instance2.Add(boundStatement);
		}
		return new BoundSwitchSection(node, binder.GetDeclaredLocalsForScope(node), instance.ToImmutableAndFree(), instance2.ToImmutableAndFree());
	}

	internal static bool ContainsUsingVariable(BoundStatement boundStatement)
	{
		if (boundStatement is BoundLocalDeclaration boundLocalDeclaration)
		{
			return boundLocalDeclaration.LocalSymbol.IsUsing;
		}
		if (boundStatement is BoundMultipleLocalDeclarationsBase boundMultipleLocalDeclarationsBase && !boundMultipleLocalDeclarationsBase.LocalDeclarations.IsDefaultOrEmpty)
		{
			return boundMultipleLocalDeclarationsBase.LocalDeclarations[0].LocalSymbol.IsUsing;
		}
		return false;
	}

	private BoundSwitchLabel BindSwitchSectionLabel(Binder sectionBinder, SwitchLabelSyntax node, LabelSymbol label, ref BoundSwitchLabel defaultLabel, BindingDiagnosticBag diagnostics)
	{
		switch (node.Kind())
		{
		case SyntaxKind.CaseSwitchLabel:
		{
			CaseSwitchLabelSyntax caseSwitchLabelSyntax = (CaseSwitchLabelSyntax)node;
			bool hasErrors = node.HasErrors;
			BoundPattern boundPattern = sectionBinder.BindConstantPatternWithFallbackToTypePattern(caseSwitchLabelSyntax.Value, caseSwitchLabelSyntax.Value, SwitchGoverningType, hasErrors, diagnostics);
			boundPattern.WasCompilerGenerated = true;
			reportIfConstantNamedUnderscore(boundPattern, caseSwitchLabelSyntax.Value);
			return new BoundSwitchLabel(node, label, boundPattern, null, boundPattern.HasErrors);
		}
		case SyntaxKind.DefaultSwitchLabel:
		{
			BoundDiscardPattern boundDiscardPattern = new BoundDiscardPattern(node, SwitchGoverningType, SwitchGoverningType);
			bool hasErrors2 = boundDiscardPattern.HasErrors;
			if (defaultLabel != null)
			{
				diagnostics.Add(ErrorCode.ERR_DuplicateCaseLabel, node.Location, label.Name);
				hasErrors2 = true;
				return new BoundSwitchLabel(node, label, boundDiscardPattern, null, hasErrors2);
			}
			return defaultLabel = new BoundSwitchLabel(node, label, boundDiscardPattern, null, hasErrors2);
		}
		case SyntaxKind.CasePatternSwitchLabel:
		{
			CasePatternSwitchLabelSyntax casePatternSwitchLabelSyntax = (CasePatternSwitchLabelSyntax)node;
			MessageID.IDS_FeaturePatternMatching.CheckFeatureAvailability(diagnostics, node.Keyword);
			BoundPattern pattern = sectionBinder.BindPattern(casePatternSwitchLabelSyntax.Pattern, SwitchGoverningType, permitDesignations: true, node.HasErrors, diagnostics);
			if (casePatternSwitchLabelSyntax.Pattern is ConstantPatternSyntax constantPatternSyntax)
			{
				reportIfConstantNamedUnderscore(pattern, constantPatternSyntax.Expression);
			}
			return new BoundSwitchLabel(node, label, pattern, (casePatternSwitchLabelSyntax.WhenClause != null) ? sectionBinder.BindBooleanExpression(casePatternSwitchLabelSyntax.WhenClause.Condition, diagnostics) : null, node.HasErrors);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(node);
		}
		void reportIfConstantNamedUnderscore(BoundPattern boundPattern2, ExpressionSyntax expression)
		{
			if (boundPattern2 is BoundConstantPattern && !boundPattern2.HasErrors && Binder.IsUnderscore(expression))
			{
				diagnostics.Add(ErrorCode.WRN_CaseConstantNamedUnderscore, expression.Location);
			}
		}
	}
}
