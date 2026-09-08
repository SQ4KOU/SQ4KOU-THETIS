using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class DeclarationTreeBuilder : CSharpSyntaxVisitor<SingleNamespaceOrTypeDeclaration>
{
	private static readonly ConditionalWeakTable<GreenNode, StrongBox<ImmutableSegmentedHashSet<string>>> s_nodeToMemberNames = new ConditionalWeakTable<GreenNode, StrongBox<ImmutableSegmentedHashSet<string>>>();

	private static readonly StrongBox<ImmutableSegmentedHashSet<string>> s_emptyMemberNames = new StrongBox<ImmutableSegmentedHashSet<string>>(ImmutableSegmentedHashSet<string>.Empty);

	private readonly SyntaxTree _syntaxTree;

	private readonly string _scriptClassName;

	private readonly bool _isSubmission;

	private readonly OneOrMany<WeakReference<StrongBox<ImmutableSegmentedHashSet<string>>>> _previousMemberNames;

	private QuickAttributes _nonGlobalAliasedQuickAttributes;

	private int _currentTypeIndex;

	private DeclarationTreeBuilder(SyntaxTree syntaxTree, string scriptClassName, bool isSubmission, OneOrMany<WeakReference<StrongBox<ImmutableSegmentedHashSet<string>>>> previousMemberNames)
	{
		_syntaxTree = syntaxTree;
		_scriptClassName = scriptClassName;
		_isSubmission = isSubmission;
		_previousMemberNames = previousMemberNames;
	}

	public static RootSingleNamespaceDeclaration ForTree(SyntaxTree syntaxTree, string scriptClassName, bool isSubmission, OneOrMany<WeakReference<StrongBox<ImmutableSegmentedHashSet<string>>>>? previousMemberNames = null)
	{
		return (RootSingleNamespaceDeclaration)new DeclarationTreeBuilder(syntaxTree, scriptClassName, isSubmission, previousMemberNames ?? OneOrMany<WeakReference<StrongBox<ImmutableSegmentedHashSet<string>>>>.Empty).Visit(syntaxTree.GetRoot());
	}

	public static bool CachesComputedMemberNames(SingleTypeDeclaration typeDeclaration)
	{
		switch (typeDeclaration.Kind)
		{
		case DeclarationKind.Namespace:
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Declarations/DeclarationTreeBuilder.cs", 104);
		case DeclarationKind.Delegate:
			return false;
		case DeclarationKind.Class:
		case DeclarationKind.Interface:
		case DeclarationKind.Struct:
		case DeclarationKind.Enum:
		case DeclarationKind.Script:
		case DeclarationKind.Submission:
		case DeclarationKind.ImplicitClass:
		case DeclarationKind.Record:
		case DeclarationKind.RecordStruct:
			return true;
		case DeclarationKind.Extension:
			return true;
		default:
			throw ExceptionUtilities.UnexpectedValue(typeDeclaration.Kind);
		}
	}

	private ImmutableArray<SingleNamespaceOrTypeDeclaration> VisitNamespaceChildren(CSharpSyntaxNode node, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax> members, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MemberDeclarationSyntax> internalMembers)
	{
		if (members.Count == 0)
		{
			return ImmutableArray<SingleNamespaceOrTypeDeclaration>.Empty;
		}
		bool flag = false;
		bool flag2 = node.Kind() == SyntaxKind.CompilationUnit && _syntaxTree.Options.Kind == SourceCodeKind.Regular;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		Microsoft.CodeAnalysis.CSharp.Syntax.GlobalStatementSyntax globalStatementSyntax = null;
		bool flag6 = false;
		ArrayBuilder<SingleNamespaceOrTypeDeclaration> instance = ArrayBuilder<SingleNamespaceOrTypeDeclaration>.GetInstance();
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax item in members)
		{
			SingleNamespaceOrTypeDeclaration singleNamespaceOrTypeDeclaration = Visit(item);
			if (singleNamespaceOrTypeDeclaration != null)
			{
				instance.Add(singleNamespaceOrTypeDeclaration);
			}
			else if (flag2 && item.IsKind(SyntaxKind.GlobalStatement))
			{
				Microsoft.CodeAnalysis.CSharp.Syntax.GlobalStatementSyntax globalStatementSyntax2 = (Microsoft.CodeAnalysis.CSharp.Syntax.GlobalStatementSyntax)item;
				if (globalStatementSyntax == null)
				{
					globalStatementSyntax = globalStatementSyntax2;
				}
				Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statement = globalStatementSyntax2.Statement;
				if (!statement.IsKind(SyntaxKind.EmptyStatement))
				{
					flag6 = true;
				}
				if (!flag3)
				{
					flag3 = SyntaxFacts.HasAwaitOperations(statement);
				}
				if (!flag4)
				{
					flag4 = SyntaxFacts.HasYieldOperations(statement);
				}
				if (!flag5)
				{
					flag5 = SyntaxFacts.HasReturnWithExpression(statement);
				}
			}
			else if (!flag && item.Kind() != SyntaxKind.IncompleteMember)
			{
				flag = true;
			}
		}
		if (globalStatementSyntax != null)
		{
			ImmutableArray<Diagnostic> diagnostics = ImmutableArray<Diagnostic>.Empty;
			if (!flag6)
			{
				DiagnosticBag instance2 = DiagnosticBag.GetInstance();
				instance2.Add(ErrorCode.ERR_SimpleProgramIsEmpty, ((Microsoft.CodeAnalysis.CSharp.Syntax.EmptyStatementSyntax)globalStatementSyntax.Statement).SemicolonToken.GetLocation());
				diagnostics = instance2.ToReadOnlyAndFree();
			}
			instance.Add(CreateSimpleProgram(globalStatementSyntax, flag3, flag4, flag5, diagnostics));
		}
		if (flag)
		{
			SingleTypeDeclaration.TypeDeclarationFlags declFlags = SingleTypeDeclaration.TypeDeclarationFlags.None;
			StrongBox<ImmutableSegmentedHashSet<string>> nonTypeMemberNames = GetNonTypeMemberNames(node, internalMembers, ref declFlags, flag2);
			SyntaxReference reference = _syntaxTree.GetReference(node);
			instance.Add(CreateImplicitClass(nonTypeMemberNames, reference, declFlags));
		}
		return instance.ToImmutableAndFree();
	}

	private static SingleNamespaceOrTypeDeclaration CreateImplicitClass(StrongBox<ImmutableSegmentedHashSet<string>> memberNames, SyntaxReference container, SingleTypeDeclaration.TypeDeclarationFlags declFlags)
	{
		return new SingleTypeDeclaration(DeclarationKind.ImplicitClass, "<invalid-global-code>", 0, DeclarationModifiers.Sealed | DeclarationModifiers.Internal | DeclarationModifiers.Partial, declFlags, container, new SourceLocation(container), memberNames, ImmutableArray<SingleTypeDeclaration>.Empty, ImmutableArray<Diagnostic>.Empty, QuickAttributes.None);
	}

	private static SingleNamespaceOrTypeDeclaration CreateSimpleProgram(Microsoft.CodeAnalysis.CSharp.Syntax.GlobalStatementSyntax firstGlobalStatement, bool hasAwaitExpressions, bool isIterator, bool hasReturnWithExpression, ImmutableArray<Diagnostic> diagnostics)
	{
		SourceLocation sourceLocation = new SourceLocation(firstGlobalStatement.GetFirstToken());
		if (sourceLocation.SourceTree == null)
		{
			sourceLocation = new SourceLocation(firstGlobalStatement.GetFirstToken(includeZeroWidth: false, includeSkipped: true));
		}
		return new SingleTypeDeclaration(DeclarationKind.Class, "Program", 0, DeclarationModifiers.Partial, (SingleTypeDeclaration.TypeDeclarationFlags)((hasAwaitExpressions ? 64 : 0) | (isIterator ? 128 : 0) | (hasReturnWithExpression ? 256 : 0) | 0x200), firstGlobalStatement.SyntaxTree.GetReference(firstGlobalStatement.Parent), sourceLocation, s_emptyMemberNames, ImmutableArray<SingleTypeDeclaration>.Empty, diagnostics, QuickAttributes.None);
	}

	private RootSingleNamespaceDeclaration CreateScriptRootDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax compilationUnit)
	{
		SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax> members = compilationUnit.Members;
		ArrayBuilder<SingleNamespaceOrTypeDeclaration> instance = ArrayBuilder<SingleNamespaceOrTypeDeclaration>.GetInstance();
		ArrayBuilder<SingleTypeDeclaration> instance2 = ArrayBuilder<SingleTypeDeclaration>.GetInstance();
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax item in members)
		{
			SingleNamespaceOrTypeDeclaration singleNamespaceOrTypeDeclaration = Visit(item);
			if (singleNamespaceOrTypeDeclaration != null)
			{
				if (singleNamespaceOrTypeDeclaration.Kind == DeclarationKind.Namespace)
				{
					instance.Add(singleNamespaceOrTypeDeclaration);
				}
				else
				{
					instance2.Add((SingleTypeDeclaration)singleNamespaceOrTypeDeclaration);
				}
			}
		}
		SingleTypeDeclaration.TypeDeclarationFlags declFlags = SingleTypeDeclaration.TypeDeclarationFlags.None;
		StrongBox<ImmutableSegmentedHashSet<string>> nonTypeMemberNames = GetNonTypeMemberNames(compilationUnit, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CompilationUnitSyntax)compilationUnit.Green).Members, ref declFlags);
		instance.Add(CreateScriptClass(compilationUnit, instance2.ToImmutableAndFree(), nonTypeMemberNames, declFlags));
		return CreateRootSingleNamespaceDeclaration(compilationUnit, instance.ToImmutableAndFree(), isForScript: true);
	}

	private static ImmutableArray<ReferenceDirective> GetReferenceDirectives(Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax compilationUnit)
	{
		IList<Microsoft.CodeAnalysis.CSharp.Syntax.ReferenceDirectiveTriviaSyntax> referenceDirectives = compilationUnit.GetReferenceDirectives((Microsoft.CodeAnalysis.CSharp.Syntax.ReferenceDirectiveTriviaSyntax d) => !d.File.ContainsDiagnostics && !string.IsNullOrEmpty(d.File.ValueText));
		if (referenceDirectives.Count == 0)
		{
			return ImmutableArray<ReferenceDirective>.Empty;
		}
		ArrayBuilder<ReferenceDirective> instance = ArrayBuilder<ReferenceDirective>.GetInstance(referenceDirectives.Count);
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.ReferenceDirectiveTriviaSyntax item in referenceDirectives)
		{
			instance.Add(new ReferenceDirective(item.File.ValueText, new SourceLocation(item)));
		}
		return instance.ToImmutableAndFree();
	}

	private SingleNamespaceOrTypeDeclaration CreateScriptClass(Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax parent, ImmutableArray<SingleTypeDeclaration> children, StrongBox<ImmutableSegmentedHashSet<string>> memberNames, SingleTypeDeclaration.TypeDeclarationFlags declFlags)
	{
		SyntaxReference reference = _syntaxTree.GetReference(parent);
		string[] array = _scriptClassName.Split(new char[1] { '.' });
		SingleNamespaceOrTypeDeclaration singleNamespaceOrTypeDeclaration = new SingleTypeDeclaration(_isSubmission ? DeclarationKind.Submission : DeclarationKind.Script, array.Last(), 0, DeclarationModifiers.Sealed | DeclarationModifiers.Internal | DeclarationModifiers.Partial, declFlags, reference, new SourceLocation(reference), memberNames, children, ImmutableArray<Diagnostic>.Empty, QuickAttributes.None);
		for (int num = array.Length - 2; num >= 0; num--)
		{
			singleNamespaceOrTypeDeclaration = SingleNamespaceDeclaration.Create(array[num], hasUsings: false, hasExternAliases: false, reference, new SourceLocation(reference), ImmutableArray.Create(singleNamespaceOrTypeDeclaration), ImmutableArray<Diagnostic>.Empty);
		}
		return singleNamespaceOrTypeDeclaration;
	}

	private static QuickAttributes GetQuickAttributes(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax> usings, bool global)
	{
		QuickAttributes quickAttributes = QuickAttributes.None;
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax item in usings)
		{
			if (item.Alias != null && item.GlobalKeyword.Kind() != SyntaxKind.None == global)
			{
				Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax name = item.Name;
				if (name != null)
				{
					quickAttributes |= QuickAttributeHelpers.GetQuickAttributes(name.GetUnqualifiedName().Identifier.ValueText, inAttribute: false);
				}
			}
		}
		return quickAttributes;
	}

	public override SingleNamespaceOrTypeDeclaration VisitCompilationUnit(Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax compilationUnit)
	{
		if (_syntaxTree.Options.Kind != SourceCodeKind.Regular)
		{
			return CreateScriptRootDeclaration(compilationUnit);
		}
		_nonGlobalAliasedQuickAttributes = GetNonGlobalAliasedQuickAttributes(compilationUnit);
		ImmutableArray<SingleNamespaceOrTypeDeclaration> children = VisitNamespaceChildren(compilationUnit, compilationUnit.Members, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CompilationUnitSyntax)compilationUnit.Green).Members);
		return CreateRootSingleNamespaceDeclaration(compilationUnit, children, isForScript: false);
	}

	private static QuickAttributes GetNonGlobalAliasedQuickAttributes(Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax compilationUnit)
	{
		QuickAttributes quickAttributes = GetQuickAttributes(compilationUnit.Usings, global: false);
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax member in compilationUnit.Members)
		{
			if (member is Microsoft.CodeAnalysis.CSharp.Syntax.BaseNamespaceDeclarationSyntax baseNamespaceDeclarationSyntax)
			{
				quickAttributes |= GetNonGlobalAliasedQuickAttributes(baseNamespaceDeclarationSyntax);
			}
		}
		return quickAttributes;
	}

	private static QuickAttributes GetNonGlobalAliasedQuickAttributes(Microsoft.CodeAnalysis.CSharp.Syntax.BaseNamespaceDeclarationSyntax @namespace)
	{
		QuickAttributes quickAttributes = GetQuickAttributes(@namespace.Usings, global: false);
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax member in @namespace.Members)
		{
			if (member is Microsoft.CodeAnalysis.CSharp.Syntax.BaseNamespaceDeclarationSyntax baseNamespaceDeclarationSyntax)
			{
				quickAttributes |= GetNonGlobalAliasedQuickAttributes(baseNamespaceDeclarationSyntax);
			}
		}
		return quickAttributes;
	}

	private RootSingleNamespaceDeclaration CreateRootSingleNamespaceDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax compilationUnit, ImmutableArray<SingleNamespaceOrTypeDeclaration> children, bool isForScript)
	{
		bool flag = false;
		bool hasGlobalUsings = false;
		bool flag2 = false;
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax @using in compilationUnit.Usings)
		{
			if (@using.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
			{
				hasGlobalUsings = true;
				if (flag && !flag2)
				{
					flag2 = true;
					instance.Add(ErrorCode.ERR_GlobalUsingOutOfOrder, @using.GlobalKeyword.GetLocation());
				}
			}
			else
			{
				flag = true;
			}
		}
		QuickAttributes quickAttributes = GetQuickAttributes(compilationUnit.Usings, global: true);
		CheckFeatureAvailabilityForUsings(instance, compilationUnit.Usings);
		CheckFeatureAvailabilityForExterns(instance, compilationUnit.Externs);
		return new RootSingleNamespaceDeclaration(hasGlobalUsings, flag, compilationUnit.Externs.Any(), _syntaxTree.GetReference(compilationUnit), children, isForScript ? GetReferenceDirectives(compilationUnit) : ImmutableArray<ReferenceDirective>.Empty, compilationUnit.AttributeLists.Any(), instance.ToReadOnlyAndFree(), quickAttributes);
	}

	private static void CheckFeatureAvailabilityForUsings(DiagnosticBag diagnostics, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax> usings)
	{
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax item in usings)
		{
			if (item.StaticKeyword != default(SyntaxToken))
			{
				MessageID.IDS_FeatureUsingStatic.CheckFeatureAvailability(diagnostics, item, item.StaticKeyword.GetLocation());
			}
			if (item.GlobalKeyword != default(SyntaxToken))
			{
				MessageID.IDS_FeatureGlobalUsing.CheckFeatureAvailability(diagnostics, item, item.GlobalKeyword.GetLocation());
			}
		}
	}

	private static void CheckFeatureAvailabilityForExterns(DiagnosticBag diagnostics, SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.ExternAliasDirectiveSyntax> externs)
	{
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.ExternAliasDirectiveSyntax item in externs)
		{
			MessageID.IDS_FeatureExternAlias.CheckFeatureAvailability(diagnostics, item, item.ExternKeyword.GetLocation());
		}
	}

	public override SingleNamespaceOrTypeDeclaration VisitFileScopedNamespaceDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.FileScopedNamespaceDeclarationSyntax node)
	{
		return VisitBaseNamespaceDeclaration(node);
	}

	public override SingleNamespaceOrTypeDeclaration VisitNamespaceDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax node)
	{
		return VisitBaseNamespaceDeclaration(node);
	}

	private SingleNamespaceDeclaration VisitBaseNamespaceDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.BaseNamespaceDeclarationSyntax node)
	{
		ImmutableArray<SingleNamespaceOrTypeDeclaration> children = VisitNamespaceChildren(node, node.Members, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BaseNamespaceDeclarationSyntax)node.Green).Members);
		bool hasUsings = node.Usings.Any();
		bool hasExternAliases = node.Externs.Any();
		Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax nameSyntax = node.Name;
		CSharpSyntaxNode node2 = node;
		while (nameSyntax is Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax qualifiedNameSyntax)
		{
			children = ImmutableArray.Create((SingleNamespaceOrTypeDeclaration)SingleNamespaceDeclaration.Create(qualifiedNameSyntax.Right.Identifier.ValueText, hasUsings, hasExternAliases, _syntaxTree.GetReference(node2), new SourceLocation(qualifiedNameSyntax.Right), children, ImmutableArray<Diagnostic>.Empty));
			node2 = (nameSyntax = qualifiedNameSyntax.Left);
			hasUsings = false;
			hasExternAliases = false;
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		if (node is Microsoft.CodeAnalysis.CSharp.Syntax.FileScopedNamespaceDeclarationSyntax)
		{
			MessageID.IDS_FeatureFileScopedNamespace.CheckFeatureAvailability(instance, node, node.NamespaceKeyword.GetLocation());
			if (node.Parent is Microsoft.CodeAnalysis.CSharp.Syntax.FileScopedNamespaceDeclarationSyntax)
			{
				instance.Add(ErrorCode.ERR_MultipleFileScopedNamespace, node.Name.GetLocation());
			}
			else if (node.Parent is Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax)
			{
				instance.Add(ErrorCode.ERR_FileScopedAndNormalNamespace, node.Name.GetLocation());
			}
			else
			{
				Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax compilationUnitSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)node.Parent;
				if (node != compilationUnitSyntax.Members[0])
				{
					instance.Add(ErrorCode.ERR_FileScopedNamespaceNotBeforeAllMembers, node.Name.GetLocation());
				}
			}
		}
		else if (node.Parent is Microsoft.CodeAnalysis.CSharp.Syntax.FileScopedNamespaceDeclarationSyntax)
		{
			instance.Add(ErrorCode.ERR_FileScopedAndNormalNamespace, node.Name.GetLocation());
		}
		if (ContainsGeneric(node.Name))
		{
			instance.Add(ErrorCode.ERR_UnexpectedGenericName, node.Name.GetLocation());
		}
		if (ContainsAlias(node.Name))
		{
			instance.Add(ErrorCode.ERR_UnexpectedAliasedName, node.Name.GetLocation());
		}
		if (node.AttributeLists.Count > 0)
		{
			instance.Add(ErrorCode.ERR_BadModifiersOnNamespace, node.AttributeLists[0].GetLocation());
		}
		if (node.Modifiers.Count > 0)
		{
			instance.Add(ErrorCode.ERR_BadModifiersOnNamespace, node.Modifiers[0].GetLocation());
		}
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax @using in node.Usings)
		{
			if (@using.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
			{
				instance.Add(ErrorCode.ERR_GlobalUsingInNamespace, @using.GlobalKeyword.GetLocation());
				break;
			}
		}
		CheckFeatureAvailabilityForUsings(instance, node.Usings);
		CheckFeatureAvailabilityForExterns(instance, node.Externs);
		return SingleNamespaceDeclaration.Create(nameSyntax.GetUnqualifiedName().Identifier.ValueText, hasUsings, hasExternAliases, _syntaxTree.GetReference(node2), new SourceLocation(nameSyntax), children, instance.ToReadOnlyAndFree());
	}

	private static bool ContainsAlias(Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax name)
	{
		return name.Kind() switch
		{
			SyntaxKind.GenericName => false, 
			SyntaxKind.AliasQualifiedName => true, 
			SyntaxKind.QualifiedName => ContainsAlias(((Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax)name).Left), 
			_ => false, 
		};
	}

	private static bool ContainsGeneric(Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax name)
	{
		switch (name.Kind())
		{
		case SyntaxKind.GenericName:
			return true;
		case SyntaxKind.AliasQualifiedName:
			return ContainsGeneric(((Microsoft.CodeAnalysis.CSharp.Syntax.AliasQualifiedNameSyntax)name).Name);
		case SyntaxKind.QualifiedName:
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax qualifiedNameSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax)name;
			if (!ContainsGeneric(qualifiedNameSyntax.Left))
			{
				return ContainsGeneric(qualifiedNameSyntax.Right);
			}
			return true;
		}
		default:
			return false;
		}
	}

	public override SingleNamespaceOrTypeDeclaration VisitClassDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax node)
	{
		return VisitTypeDeclaration(node, DeclarationKind.Class);
	}

	public override SingleNamespaceOrTypeDeclaration VisitStructDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.StructDeclarationSyntax node)
	{
		return VisitTypeDeclaration(node, DeclarationKind.Struct);
	}

	public override SingleNamespaceOrTypeDeclaration VisitInterfaceDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax node)
	{
		return VisitTypeDeclaration(node, DeclarationKind.Interface);
	}

	public override SingleNamespaceOrTypeDeclaration VisitRecordDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.RecordDeclarationSyntax node)
	{
		return VisitTypeDeclaration(node, node.Kind() switch
		{
			SyntaxKind.RecordDeclaration => DeclarationKind.Record, 
			SyntaxKind.RecordStructDeclaration => DeclarationKind.RecordStruct, 
			_ => throw ExceptionUtilities.UnexpectedValue(node.Kind()), 
		});
	}

	public override SingleNamespaceOrTypeDeclaration VisitExtensionBlockDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.ExtensionBlockDeclarationSyntax node)
	{
		return VisitTypeDeclaration(node, DeclarationKind.Extension);
	}

	private SingleTypeDeclaration VisitTypeDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax node, DeclarationKind kind)
	{
		SingleTypeDeclaration.TypeDeclarationFlags declFlags = (node.AttributeLists.Any() ? SingleTypeDeclaration.TypeDeclarationFlags.HasAnyAttributes : SingleTypeDeclaration.TypeDeclarationFlags.None);
		if (node.BaseList != null)
		{
			declFlags |= SingleTypeDeclaration.TypeDeclarationFlags.HasBaseDeclarations;
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		if (node.Arity == 0)
		{
			Symbol.ReportErrorIfHasConstraints(node.ConstraintClauses, instance);
		}
		bool flag = node.ParameterList != null;
		if (flag)
		{
			bool flag2 = ((node is Microsoft.CodeAnalysis.CSharp.Syntax.RecordDeclarationSyntax || node is Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax || node is Microsoft.CodeAnalysis.CSharp.Syntax.StructDeclarationSyntax) ? true : false);
			flag = flag2;
		}
		bool flag3 = flag;
		if (flag3)
		{
			declFlags |= SingleTypeDeclaration.TypeDeclarationFlags.HasAnyNontypeMembers;
			declFlags |= SingleTypeDeclaration.TypeDeclarationFlags.HasPrimaryConstructor;
			foreach (Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax attributeList in node.AttributeLists)
			{
				Microsoft.CodeAnalysis.CSharp.Syntax.AttributeTargetSpecifierSyntax? target = attributeList.Target;
				if (target != null && target.Identifier.ToAttributeLocation() == AttributeLocation.Method)
				{
					declFlags |= SingleTypeDeclaration.TypeDeclarationFlags.AnyMemberHasAttributes;
					break;
				}
			}
		}
		StrongBox<ImmutableSegmentedHashSet<string>> nonTypeMemberNames = GetNonTypeMemberNames(node, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeDeclarationSyntax)node.Green).Members, ref declFlags, skipGlobalStatements: false, flag3);
		if (node is Microsoft.CodeAnalysis.CSharp.Syntax.RecordDeclarationSyntax recordDeclarationSyntax)
		{
			if (recordDeclarationSyntax.ClassOrStructKeyword.Kind() != SyntaxKind.None)
			{
				MessageID.IDS_FeatureRecordStructs.CheckFeatureAvailability(instance, recordDeclarationSyntax, recordDeclarationSyntax.ClassOrStructKeyword.GetLocation());
			}
		}
		else
		{
			SyntaxKind syntaxKind = node.Kind();
			if (syntaxKind - 8855 <= (SyntaxKind)2)
			{
				if (node.ParameterList != null)
				{
					if (node.Kind() == SyntaxKind.InterfaceDeclaration)
					{
						instance.Add(ErrorCode.ERR_UnexpectedParameterList, node.ParameterList.GetLocation());
					}
					else
					{
						MessageID.IDS_FeaturePrimaryConstructors.CheckFeatureAvailability(instance, node.ParameterList);
					}
				}
				else if (node.OpenBraceToken == default(SyntaxToken) && node.CloseBraceToken == default(SyntaxToken) && node.SemicolonToken != default(SyntaxToken))
				{
					MessageID.IDS_FeaturePrimaryConstructors.CheckFeatureAvailability(instance, node, node.SemicolonToken.GetLocation());
				}
			}
		}
		DeclarationModifiers modifiers = node.Modifiers.ToDeclarationModifiers(isForTypeDeclaration: true, instance);
		QuickAttributes quickAttributes = GetQuickAttributes(node.AttributeLists);
		foreach (SyntaxToken modifier in node.Modifiers)
		{
			if (modifier.IsKind(SyntaxKind.StaticKeyword) && kind == DeclarationKind.Class)
			{
				MessageID.IDS_FeatureStaticClasses.CheckFeatureAvailability(instance, node, modifier.GetLocation());
				continue;
			}
			flag = modifier.IsKind(SyntaxKind.ReadOnlyKeyword);
			if (flag)
			{
				bool flag2 = ((kind == DeclarationKind.Struct || kind == DeclarationKind.RecordStruct) ? true : false);
				flag = flag2;
			}
			if (flag)
			{
				MessageID.IDS_FeatureReadOnlyStructs.CheckFeatureAvailability(instance, node, modifier.GetLocation());
				continue;
			}
			flag = modifier.IsKind(SyntaxKind.RefKeyword);
			if (flag)
			{
				bool flag2 = ((kind == DeclarationKind.Struct || kind == DeclarationKind.RecordStruct) ? true : false);
				flag = flag2;
			}
			if (flag)
			{
				MessageID.IDS_FeatureRefStructs.CheckFeatureAvailability(instance, node, modifier.GetLocation());
			}
		}
		bool flag4 = kind == DeclarationKind.Extension;
		return new SingleTypeDeclaration(kind, flag4 ? "" : node.Identifier.ValueText, node.Arity, modifiers, declFlags, _syntaxTree.GetReference(node), new SourceLocation(flag4 ? node.Keyword : node.Identifier), nonTypeMemberNames, VisitTypeChildren(node), instance.ToReadOnlyAndFree(), _nonGlobalAliasedQuickAttributes | quickAttributes);
	}

	private ImmutableArray<SingleTypeDeclaration> VisitTypeChildren(Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax node)
	{
		if (node.Members.Count == 0)
		{
			return ImmutableArray<SingleTypeDeclaration>.Empty;
		}
		ArrayBuilder<SingleTypeDeclaration> instance = ArrayBuilder<SingleTypeDeclaration>.GetInstance();
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax member in node.Members)
		{
			SingleTypeDeclaration value = Visit(member) as SingleTypeDeclaration;
			instance.AddIfNotNull(value);
		}
		return instance.ToImmutableAndFree();
	}

	public override SingleNamespaceOrTypeDeclaration VisitDelegateDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.DelegateDeclarationSyntax node)
	{
		SingleTypeDeclaration.TypeDeclarationFlags typeDeclarationFlags = (node.AttributeLists.Any() ? SingleTypeDeclaration.TypeDeclarationFlags.HasAnyAttributes : SingleTypeDeclaration.TypeDeclarationFlags.None);
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		if (node.Arity == 0)
		{
			Symbol.ReportErrorIfHasConstraints(node.ConstraintClauses, instance);
		}
		typeDeclarationFlags |= SingleTypeDeclaration.TypeDeclarationFlags.HasAnyNontypeMembers;
		DeclarationModifiers modifiers = node.Modifiers.ToDeclarationModifiers(isForTypeDeclaration: true, instance);
		QuickAttributes quickAttributes = GetQuickAttributes(node.AttributeLists);
		return new SingleTypeDeclaration(DeclarationKind.Delegate, node.Identifier.ValueText, node.Arity, modifiers, typeDeclarationFlags, _syntaxTree.GetReference(node), new SourceLocation(node.Identifier), s_emptyMemberNames, ImmutableArray<SingleTypeDeclaration>.Empty, instance.ToReadOnlyAndFree(), _nonGlobalAliasedQuickAttributes | quickAttributes);
	}

	public override SingleNamespaceOrTypeDeclaration VisitEnumDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.EnumDeclarationSyntax node)
	{
		_ = node.Members;
		SingleTypeDeclaration.TypeDeclarationFlags declFlags = (node.AttributeLists.Any() ? SingleTypeDeclaration.TypeDeclarationFlags.HasAnyAttributes : SingleTypeDeclaration.TypeDeclarationFlags.None);
		if (node.BaseList != null)
		{
			declFlags |= SingleTypeDeclaration.TypeDeclarationFlags.HasBaseDeclarations;
		}
		StrongBox<ImmutableSegmentedHashSet<string>> enumMemberNames = GetEnumMemberNames(node, ref declFlags);
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		DeclarationModifiers modifiers = node.Modifiers.ToDeclarationModifiers(isForTypeDeclaration: true, instance);
		QuickAttributes quickAttributes = GetQuickAttributes(node.AttributeLists);
		if (node.OpenBraceToken == default(SyntaxToken) && node.CloseBraceToken == default(SyntaxToken) && node.SemicolonToken != default(SyntaxToken))
		{
			MessageID.IDS_FeaturePrimaryConstructors.CheckFeatureAvailability(instance, node, node.SemicolonToken.GetLocation());
		}
		return new SingleTypeDeclaration(DeclarationKind.Enum, node.Identifier.ValueText, 0, modifiers, declFlags, _syntaxTree.GetReference(node), new SourceLocation(node.Identifier), enumMemberNames, ImmutableArray<SingleTypeDeclaration>.Empty, instance.ToReadOnlyAndFree(), _nonGlobalAliasedQuickAttributes | quickAttributes);
	}

	private static QuickAttributes GetQuickAttributes(SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax> attributeLists)
	{
		QuickAttributes quickAttributes = QuickAttributes.None;
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax item in attributeLists)
		{
			foreach (Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax attribute in item.Attributes)
			{
				quickAttributes |= QuickAttributeHelpers.GetQuickAttributes(attribute.Name.GetUnqualifiedName().Identifier.ValueText, inAttribute: true);
			}
		}
		return quickAttributes;
	}

	private StrongBox<ImmutableSegmentedHashSet<string>> GetEnumMemberNames(Microsoft.CodeAnalysis.CSharp.Syntax.EnumDeclarationSyntax enumDeclaration, ref SingleTypeDeclaration.TypeDeclarationFlags declFlags)
	{
		SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.EnumMemberDeclarationSyntax> members = enumDeclaration.Members;
		if (members.Count != 0)
		{
			declFlags |= SingleTypeDeclaration.TypeDeclarationFlags.HasAnyNontypeMembers;
		}
		if (members.Any((Microsoft.CodeAnalysis.CSharp.Syntax.EnumMemberDeclarationSyntax m) => m.AttributeLists.Any()))
		{
			declFlags |= SingleTypeDeclaration.TypeDeclarationFlags.AnyMemberHasAttributes;
		}
		return GetOrComputeMemberNames(enumDeclaration, delegate(HashSet<string> memberNamesBuilder, SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.EnumMemberDeclarationSyntax> separatedSyntaxList)
		{
			foreach (Microsoft.CodeAnalysis.CSharp.Syntax.EnumMemberDeclarationSyntax item in separatedSyntaxList)
			{
				memberNamesBuilder.Add(item.Identifier.ValueText);
			}
		}, members);
	}

	private StrongBox<ImmutableSegmentedHashSet<string>> GetNonTypeMemberNames(CSharpSyntaxNode parent, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MemberDeclarationSyntax> members, ref SingleTypeDeclaration.TypeDeclarationFlags declFlags, bool skipGlobalStatements = false, bool hasPrimaryCtor = false)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MemberDeclarationSyntax item in members)
		{
			if (!flag4 && HasAnyNonTypeMemberNames(item, skipGlobalStatements))
			{
				flag4 = true;
			}
			if (!flag && CheckMethodMemberForExtensionSyntax(item))
			{
				flag = true;
			}
			if (!flag2 && item.Kind == SyntaxKind.ExtensionBlockDeclaration)
			{
				flag2 = true;
			}
			if (!flag3 && CheckMemberForAttributes(item))
			{
				flag3 = true;
			}
			if (!flag5 && checkPropertyOrFieldMemberForRequiredModifier(item))
			{
				flag5 = true;
			}
			if (flag4 & flag & flag2 & flag3 & flag5)
			{
				break;
			}
		}
		if (flag)
		{
			declFlags |= SingleTypeDeclaration.TypeDeclarationFlags.AnyMemberHasExtensionMethodSyntax;
		}
		if (flag2)
		{
			declFlags |= SingleTypeDeclaration.TypeDeclarationFlags.AnyExtensionDeclarationSyntax;
		}
		if (flag3)
		{
			declFlags |= SingleTypeDeclaration.TypeDeclarationFlags.AnyMemberHasAttributes;
		}
		if (flag4)
		{
			declFlags |= SingleTypeDeclaration.TypeDeclarationFlags.HasAnyNontypeMembers;
		}
		if (flag5)
		{
			declFlags |= SingleTypeDeclaration.TypeDeclarationFlags.HasRequiredMembers;
		}
		return GetOrComputeMemberNames(parent, delegate(HashSet<string> memberNamesBuilder, (Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MemberDeclarationSyntax> members, bool hasPrimaryCtor) tuple)
		{
			if (tuple.hasPrimaryCtor)
			{
				memberNamesBuilder.Add(".ctor");
			}
			foreach (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MemberDeclarationSyntax item2 in tuple.members)
			{
				AddNonTypeMemberNames(item2, memberNamesBuilder);
			}
		}, (members, hasPrimaryCtor));
		static bool checkPropertyOrFieldMemberForRequiredModifier(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode member)
		{
			Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken> syntaxList = (Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>)((member is Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FieldDeclarationSyntax fieldDeclarationSyntax) ? fieldDeclarationSyntax.Modifiers : ((!(member is Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PropertyDeclarationSyntax propertyDeclarationSyntax)) ? ((ValueType)default(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken>)) : ((ValueType)propertyDeclarationSyntax.Modifiers)));
			Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken> syntaxList2 = syntaxList;
			return syntaxList2.Any(8447);
		}
	}

	private StrongBox<ImmutableSegmentedHashSet<string>> GetOrComputeMemberNames<TData>(SyntaxNode parent, Action<HashSet<string>, TData> addMemberNames, TData data)
	{
		StrongBox<ImmutableSegmentedHashSet<string>> result = getOrComputeMemberNamesWorker();
		_currentTypeIndex++;
		return result;
		StrongBox<ImmutableSegmentedHashSet<string>> getOrComputeMemberNamesWorker()
		{
			GreenNode green = parent.Green;
			if (!s_nodeToMemberNames.TryGetValue(green, out var value))
			{
				PooledHashSet<string> instance = PooledHashSet<string>.GetInstance();
				addMemberNames(instance, data);
				StrongBox<ImmutableSegmentedHashSet<string>> strongBox = ((_currentTypeIndex < _previousMemberNames.Count && _previousMemberNames[_currentTypeIndex].TryGetTarget(out var target)) ? target : s_emptyMemberNames);
				value = ((strongBox.Value.Count == instance.Count && strongBox.Value.SetEquals(instance)) ? strongBox : ((instance.Count == 0) ? s_emptyMemberNames : new StrongBox<ImmutableSegmentedHashSet<string>>(ImmutableSegmentedHashSet.CreateRange(instance))));
				instance.Free();
				if (value.Value.Count > 0)
				{
					ConditionalWeakTable<GreenNode, StrongBox<ImmutableSegmentedHashSet<string>>>.CreateValueCallback boundFunction;
					using (PooledDelegates.GetPooledCreateValueCallback((GreenNode _, StrongBox<ImmutableSegmentedHashSet<string>> memberNames) => memberNames, value, out boundFunction))
					{
						value = s_nodeToMemberNames.GetValue(green, boundFunction);
					}
				}
			}
			return value;
		}
	}

	private static bool CheckMethodMemberForExtensionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode member)
	{
		if (member.Kind == SyntaxKind.MethodDeclaration)
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParameterListSyntax parameterList = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MethodDeclarationSyntax)member).parameterList;
			if (parameterList != null)
			{
				Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParameterSyntax> parameters = parameterList.Parameters;
				if (parameters.Count != 0)
				{
					foreach (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken modifier in parameters[0].Modifiers)
					{
						if (modifier.Kind == SyntaxKind.ThisKeyword)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	private static bool CheckMemberForAttributes(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode member)
	{
		switch (member.Kind)
		{
		case SyntaxKind.CompilationUnit:
			return ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CompilationUnitSyntax)member).AttributeLists.Any();
		case SyntaxKind.ClassDeclaration:
		case SyntaxKind.StructDeclaration:
		case SyntaxKind.InterfaceDeclaration:
		case SyntaxKind.EnumDeclaration:
		case SyntaxKind.RecordDeclaration:
		case SyntaxKind.RecordStructDeclaration:
		case SyntaxKind.ExtensionBlockDeclaration:
			return ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BaseTypeDeclarationSyntax)member).AttributeLists.Any();
		case SyntaxKind.DelegateDeclaration:
			return ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.DelegateDeclarationSyntax)member).AttributeLists.Any();
		case SyntaxKind.FieldDeclaration:
		case SyntaxKind.EventFieldDeclaration:
			return ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BaseFieldDeclarationSyntax)member).AttributeLists.Any();
		case SyntaxKind.MethodDeclaration:
		case SyntaxKind.OperatorDeclaration:
		case SyntaxKind.ConversionOperatorDeclaration:
		case SyntaxKind.ConstructorDeclaration:
		case SyntaxKind.DestructorDeclaration:
			return ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BaseMethodDeclarationSyntax)member).AttributeLists.Any();
		case SyntaxKind.PropertyDeclaration:
		case SyntaxKind.EventDeclaration:
		case SyntaxKind.IndexerDeclaration:
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BasePropertyDeclarationSyntax basePropertyDeclarationSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BasePropertyDeclarationSyntax)member;
			bool flag = basePropertyDeclarationSyntax.AttributeLists.Any();
			if (!flag && basePropertyDeclarationSyntax.AccessorList != null)
			{
				foreach (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AccessorDeclarationSyntax accessor in basePropertyDeclarationSyntax.AccessorList.Accessors)
				{
					flag |= accessor.AttributeLists.Any();
				}
			}
			return flag;
		}
		default:
			return false;
		}
	}

	private static void AddNonTypeMemberNames(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode member, HashSet<string> set)
	{
		switch (member.Kind)
		{
		case SyntaxKind.FieldDeclaration:
		{
			Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.VariableDeclaratorSyntax> variables = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FieldDeclarationSyntax)member).Declaration.Variables;
			int count = variables.Count;
			for (int i = 0; i < count; i++)
			{
				set.Add(variables[i].Identifier.ValueText);
			}
			break;
		}
		case SyntaxKind.EventFieldDeclaration:
		{
			Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.VariableDeclaratorSyntax> variables2 = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EventFieldDeclarationSyntax)member).Declaration.Variables;
			int count2 = variables2.Count;
			for (int j = 0; j < count2; j++)
			{
				set.Add(variables2[j].Identifier.ValueText);
			}
			break;
		}
		case SyntaxKind.MethodDeclaration:
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MethodDeclarationSyntax methodDeclarationSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MethodDeclarationSyntax)member;
			if (methodDeclarationSyntax.ExplicitInterfaceSpecifier == null)
			{
				set.Add(methodDeclarationSyntax.Identifier.ValueText);
			}
			break;
		}
		case SyntaxKind.PropertyDeclaration:
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PropertyDeclarationSyntax propertyDeclarationSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PropertyDeclarationSyntax)member;
			if (propertyDeclarationSyntax.ExplicitInterfaceSpecifier == null)
			{
				set.Add(propertyDeclarationSyntax.Identifier.ValueText);
			}
			break;
		}
		case SyntaxKind.EventDeclaration:
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EventDeclarationSyntax eventDeclarationSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EventDeclarationSyntax)member;
			if (eventDeclarationSyntax.ExplicitInterfaceSpecifier == null)
			{
				set.Add(eventDeclarationSyntax.Identifier.ValueText);
			}
			break;
		}
		case SyntaxKind.ConstructorDeclaration:
			set.Add(((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConstructorDeclarationSyntax)member).Modifiers.Any(8347) ? ".cctor" : ".ctor");
			break;
		case SyntaxKind.DestructorDeclaration:
			set.Add("Finalize");
			break;
		case SyntaxKind.IndexerDeclaration:
			set.Add("this[]");
			break;
		case SyntaxKind.OperatorDeclaration:
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.OperatorDeclarationSyntax operatorDeclarationSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.OperatorDeclarationSyntax)member;
			if (operatorDeclarationSyntax.ExplicitInterfaceSpecifier == null)
			{
				string item2 = OperatorFacts.OperatorNameFromDeclaration(operatorDeclarationSyntax);
				set.Add(item2);
			}
			break;
		}
		case SyntaxKind.ConversionOperatorDeclaration:
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConversionOperatorDeclarationSyntax conversionOperatorDeclarationSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConversionOperatorDeclarationSyntax)member;
			if (conversionOperatorDeclarationSyntax.ExplicitInterfaceSpecifier == null)
			{
				string item = OperatorFacts.OperatorNameFromDeclaration(conversionOperatorDeclarationSyntax);
				set.Add(item);
			}
			break;
		}
		}
	}

	private static bool HasAnyNonTypeMemberNames(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode member, bool skipGlobalStatements)
	{
		switch (member.Kind)
		{
		case SyntaxKind.FieldDeclaration:
		case SyntaxKind.EventFieldDeclaration:
		case SyntaxKind.MethodDeclaration:
		case SyntaxKind.OperatorDeclaration:
		case SyntaxKind.ConversionOperatorDeclaration:
		case SyntaxKind.ConstructorDeclaration:
		case SyntaxKind.DestructorDeclaration:
		case SyntaxKind.PropertyDeclaration:
		case SyntaxKind.EventDeclaration:
		case SyntaxKind.IndexerDeclaration:
			return true;
		case SyntaxKind.GlobalStatement:
			return !skipGlobalStatements;
		default:
			return false;
		}
	}
}
