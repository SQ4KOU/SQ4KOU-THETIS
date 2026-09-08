using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BinderFactory
{
	internal sealed class BinderFactoryVisitor : CSharpSyntaxVisitor<Binder>
	{
		private int _position;

		private CSharpSyntaxNode _memberDeclarationOpt;

		private Symbol _memberOpt;

		private BinderFactory _factory;

		private CSharpCompilation compilation => _factory._compilation;

		private SyntaxTree syntaxTree => _factory._syntaxTree;

		private BuckStopsHereBinder buckStopsHereBinder => _factory._buckStopsHereBinder;

		private ConcurrentCache<BinderCacheKey, Binder> binderCache => _factory._binderCache;

		private bool InScript => _factory.InScript;

		internal void Initialize(BinderFactory factory, int position, CSharpSyntaxNode memberDeclarationOpt, Symbol memberOpt)
		{
			_factory = factory;
			_position = position;
			_memberDeclarationOpt = memberDeclarationOpt;
			_memberOpt = memberOpt;
		}

		internal void Clear()
		{
			_factory = null;
			_position = 0;
			_memberDeclarationOpt = null;
			_memberOpt = null;
		}

		public override Binder DefaultVisit(SyntaxNode parent)
		{
			return VisitCore(parent.Parent);
		}

		public override Binder Visit(SyntaxNode node)
		{
			return VisitCore(node);
		}

		private Binder VisitCore(SyntaxNode node)
		{
			return ((CSharpSyntaxNode)node).Accept(this);
		}

		public override Binder VisitGlobalStatement(GlobalStatementSyntax node)
		{
			if (SyntaxFacts.IsSimpleProgramTopLevelStatement(node))
			{
				CompilationUnitSyntax compilationUnitSyntax = (CompilationUnitSyntax)node.Parent;
				if (compilationUnitSyntax != syntaxTree.GetRoot())
				{
					throw new ArgumentOutOfRangeException("node", "node not part of tree");
				}
				BinderCacheKey key = CreateBinderCacheKey(compilationUnitSyntax, NodeUsage.MethodBody);
				if (!binderCache.TryGetValue(key, out var value))
				{
					value = SynthesizedSimpleProgramEntryPointSymbol.GetSimpleProgramEntryPoint(compilation, (CompilationUnitSyntax)node.Parent, fallbackToMainEntryPoint: false).GetBodyBinder(_factory._ignoreAccessibility).GetBinder(compilationUnitSyntax);
					binderCache.TryAdd(key, value);
				}
				return value;
			}
			return base.VisitGlobalStatement(node);
		}

		public override Binder VisitMethodDeclaration(MethodDeclarationSyntax methodDecl)
		{
			if (!LookupPosition.IsInMethodDeclaration(_position, methodDecl))
			{
				return VisitCore(methodDecl.Parent);
			}
			NodeUsage nodeUsage = (LookupPosition.IsInBody(_position, methodDecl) ? NodeUsage.MethodBody : (LookupPosition.IsInMethodTypeParameterScope(_position, methodDecl) ? NodeUsage.MethodTypeParameters : NodeUsage.Normal));
			BinderCacheKey key = CreateBinderCacheKey(methodDecl, nodeUsage);
			if (!binderCache.TryGetValue(key, out var value))
			{
				value = ((!(methodDecl.Parent is TypeDeclarationSyntax parent)) ? VisitCore(methodDecl.Parent) : VisitTypeDeclarationCore(parent, NodeUsage.MethodBody));
				SourceMemberMethodSymbol sourceMemberMethodSymbol = null;
				bool isIteratorBody = false;
				if (nodeUsage != NodeUsage.Normal && methodDecl.TypeParameterList != null)
				{
					sourceMemberMethodSymbol = GetMethodSymbol(methodDecl, value);
					value = new WithMethodTypeParametersBinder(sourceMemberMethodSymbol, value);
				}
				if (nodeUsage == NodeUsage.MethodBody)
				{
					sourceMemberMethodSymbol = sourceMemberMethodSymbol ?? GetMethodSymbol(methodDecl, value);
					isIteratorBody = sourceMemberMethodSymbol.IsIterator;
					value = new InMethodBinder(sourceMemberMethodSymbol, value);
				}
				value = value.SetOrClearUnsafeRegionIfNecessary(methodDecl.Modifiers, isIteratorBody);
				binderCache.TryAdd(key, value);
			}
			return value;
		}

		public override Binder VisitConstructorDeclaration(ConstructorDeclarationSyntax parent)
		{
			if (!LookupPosition.IsInMethodDeclaration(_position, parent))
			{
				return VisitCore(parent.Parent);
			}
			bool flag = LookupPosition.IsInConstructorParameterScope(_position, parent);
			NodeUsage usage = (flag ? NodeUsage.MethodTypeParameters : NodeUsage.Normal);
			BinderCacheKey key = CreateBinderCacheKey(parent, usage);
			if (!binderCache.TryGetValue(key, out var value))
			{
				value = VisitCore(parent.Parent);
				if (flag)
				{
					SourceMemberMethodSymbol methodSymbol = GetMethodSymbol(parent, value);
					if ((object)methodSymbol != null)
					{
						value = new InMethodBinder(methodSymbol, value);
					}
				}
				value = value.SetOrClearUnsafeRegionIfNecessary(parent.Modifiers);
				binderCache.TryAdd(key, value);
			}
			return value;
		}

		public override Binder VisitDestructorDeclaration(DestructorDeclarationSyntax parent)
		{
			if (!LookupPosition.IsInBody(_position, parent))
			{
				return VisitCore(parent.Parent);
			}
			BinderCacheKey key = CreateBinderCacheKey(parent, NodeUsage.Normal);
			if (!binderCache.TryGetValue(key, out var value))
			{
				value = VisitCore(parent.Parent);
				value = new InMethodBinder(GetMethodSymbol(parent, value), value);
				value = value.SetOrClearUnsafeRegionIfNecessary(parent.Modifiers);
				binderCache.TryAdd(key, value);
			}
			return value;
		}

		public override Binder VisitAccessorDeclaration(AccessorDeclarationSyntax parent)
		{
			if (!LookupPosition.IsInMethodDeclaration(_position, parent))
			{
				return VisitCore(parent.Parent);
			}
			bool flag = LookupPosition.IsInBody(_position, parent);
			NodeUsage usage = (flag ? NodeUsage.MethodTypeParameters : NodeUsage.Normal);
			BinderCacheKey key = CreateBinderCacheKey(parent, usage);
			if (!binderCache.TryGetValue(key, out var value))
			{
				value = VisitCore(parent.Parent);
				if (flag)
				{
					CSharpSyntaxNode parent2 = parent.Parent.Parent;
					MethodSymbol methodSymbol = null;
					switch (parent2.Kind())
					{
					case SyntaxKind.PropertyDeclaration:
					case SyntaxKind.IndexerDeclaration:
					{
						SourcePropertySymbol propertySymbol = GetPropertySymbol((BasePropertyDeclarationSyntax)parent2, value);
						if ((object)propertySymbol != null)
						{
							methodSymbol = ((parent.Kind() == SyntaxKind.GetAccessorDeclaration) ? propertySymbol.GetMethod : propertySymbol.SetMethod);
						}
						break;
					}
					case SyntaxKind.EventFieldDeclaration:
					case SyntaxKind.EventDeclaration:
					{
						SourceEventSymbol eventSymbol = GetEventSymbol((EventDeclarationSyntax)parent2, value);
						if ((object)eventSymbol != null)
						{
							methodSymbol = ((parent.Kind() == SyntaxKind.AddAccessorDeclaration) ? eventSymbol.AddMethod : eventSymbol.RemoveMethod);
						}
						break;
					}
					default:
						throw ExceptionUtilities.UnexpectedValue(parent2.Kind());
					}
					if ((object)methodSymbol != null)
					{
						value = new InMethodBinder(methodSymbol, value);
						value = value.SetOrClearUnsafeRegionIfNecessary(default(SyntaxTokenList), methodSymbol.IsIterator);
					}
				}
				binderCache.TryAdd(key, value);
			}
			return value;
		}

		private Binder VisitOperatorOrConversionDeclaration(BaseMethodDeclarationSyntax parent)
		{
			if (!LookupPosition.IsInMethodDeclaration(_position, parent))
			{
				return VisitCore(parent.Parent);
			}
			bool flag = LookupPosition.IsInBody(_position, parent);
			NodeUsage usage = (flag ? NodeUsage.MethodTypeParameters : NodeUsage.Normal);
			BinderCacheKey key = CreateBinderCacheKey(parent, usage);
			if (!binderCache.TryGetValue(key, out var value))
			{
				value = VisitCore(parent.Parent);
				MethodSymbol methodSymbol = GetMethodSymbol(parent, value);
				bool isIteratorBody = false;
				if (((object)methodSymbol != null) & flag)
				{
					isIteratorBody = methodSymbol.IsIterator;
					value = new InMethodBinder(methodSymbol, value);
				}
				value = value.SetOrClearUnsafeRegionIfNecessary(parent.Modifiers, isIteratorBody);
				binderCache.TryAdd(key, value);
			}
			return value;
		}

		public override Binder VisitOperatorDeclaration(OperatorDeclarationSyntax parent)
		{
			return VisitOperatorOrConversionDeclaration(parent);
		}

		public override Binder VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax parent)
		{
			return VisitOperatorOrConversionDeclaration(parent);
		}

		public override Binder VisitFieldDeclaration(FieldDeclarationSyntax parent)
		{
			return VisitCore(parent.Parent).SetOrClearUnsafeRegionIfNecessary(parent.Modifiers);
		}

		public override Binder VisitEventDeclaration(EventDeclarationSyntax parent)
		{
			return VisitCore(parent.Parent).SetOrClearUnsafeRegionIfNecessary(parent.Modifiers);
		}

		public override Binder VisitEventFieldDeclaration(EventFieldDeclarationSyntax parent)
		{
			return VisitCore(parent.Parent).SetOrClearUnsafeRegionIfNecessary(parent.Modifiers);
		}

		public override Binder VisitPropertyDeclaration(PropertyDeclarationSyntax parent)
		{
			if (!LookupPosition.IsInBody(_position, parent))
			{
				return VisitCore(parent.Parent).SetOrClearUnsafeRegionIfNecessary(parent.Modifiers);
			}
			return VisitPropertyOrIndexerExpressionBody(parent);
		}

		public override Binder VisitIndexerDeclaration(IndexerDeclarationSyntax parent)
		{
			if (!LookupPosition.IsInBody(_position, parent))
			{
				return VisitCore(parent.Parent).SetOrClearUnsafeRegionIfNecessary(parent.Modifiers);
			}
			return VisitPropertyOrIndexerExpressionBody(parent);
		}

		private Binder VisitPropertyOrIndexerExpressionBody(BasePropertyDeclarationSyntax parent)
		{
			BinderCacheKey key = CreateBinderCacheKey(parent, NodeUsage.MethodTypeParameters);
			if (!binderCache.TryGetValue(key, out var value))
			{
				value = VisitCore(parent.Parent).SetOrClearUnsafeRegionIfNecessary(parent.Modifiers);
				MethodSymbol getMethod = GetPropertySymbol(parent, value).GetMethod;
				if ((object)getMethod != null)
				{
					value = new InMethodBinder(getMethod, value);
				}
				binderCache.TryAdd(key, value);
			}
			return value;
		}

		private NamedTypeSymbol GetContainerType(Binder binder, CSharpSyntaxNode node)
		{
			Symbol containingMemberOrLambda = binder.ContainingMemberOrLambda;
			NamedTypeSymbol namedTypeSymbol = containingMemberOrLambda as NamedTypeSymbol;
			if ((object)namedTypeSymbol == null)
			{
				namedTypeSymbol = ((node.Parent.Kind() != SyntaxKind.CompilationUnit || syntaxTree.Options.Kind == SourceCodeKind.Regular) ? ((NamespaceSymbol)containingMemberOrLambda).ImplicitType : compilation.ScriptClass);
			}
			return namedTypeSymbol;
		}

		private static string GetMethodName(BaseMethodDeclarationSyntax baseMethodDeclarationSyntax, Binder outerBinder)
		{
			switch (baseMethodDeclarationSyntax.Kind())
			{
			case SyntaxKind.ConstructorDeclaration:
				if (!baseMethodDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword))
				{
					return ".ctor";
				}
				return ".cctor";
			case SyntaxKind.DestructorDeclaration:
				return "Finalize";
			case SyntaxKind.OperatorDeclaration:
			{
				OperatorDeclarationSyntax operatorDeclarationSyntax = (OperatorDeclarationSyntax)baseMethodDeclarationSyntax;
				return ExplicitInterfaceHelpers.GetMemberName(outerBinder, baseMethodDeclarationSyntax.Modifiers, operatorDeclarationSyntax.ExplicitInterfaceSpecifier, OperatorFacts.OperatorNameFromDeclaration(operatorDeclarationSyntax));
			}
			case SyntaxKind.ConversionOperatorDeclaration:
			{
				ConversionOperatorDeclarationSyntax conversionOperatorDeclarationSyntax = (ConversionOperatorDeclarationSyntax)baseMethodDeclarationSyntax;
				return ExplicitInterfaceHelpers.GetMemberName(outerBinder, baseMethodDeclarationSyntax.Modifiers, conversionOperatorDeclarationSyntax.ExplicitInterfaceSpecifier, OperatorFacts.OperatorNameFromDeclaration(conversionOperatorDeclarationSyntax));
			}
			case SyntaxKind.MethodDeclaration:
			{
				MethodDeclarationSyntax methodDeclarationSyntax = (MethodDeclarationSyntax)baseMethodDeclarationSyntax;
				return ExplicitInterfaceHelpers.GetMemberName(outerBinder, baseMethodDeclarationSyntax.Modifiers, methodDeclarationSyntax.ExplicitInterfaceSpecifier, methodDeclarationSyntax.Identifier.ValueText);
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(baseMethodDeclarationSyntax.Kind());
			}
		}

		private static string GetPropertyOrEventName(BasePropertyDeclarationSyntax basePropertyDeclarationSyntax, Binder outerBinder)
		{
			ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier = basePropertyDeclarationSyntax.ExplicitInterfaceSpecifier;
			switch (basePropertyDeclarationSyntax.Kind())
			{
			case SyntaxKind.PropertyDeclaration:
			{
				PropertyDeclarationSyntax propertyDeclarationSyntax = (PropertyDeclarationSyntax)basePropertyDeclarationSyntax;
				return ExplicitInterfaceHelpers.GetMemberName(outerBinder, basePropertyDeclarationSyntax.Modifiers, explicitInterfaceSpecifier, propertyDeclarationSyntax.Identifier.ValueText);
			}
			case SyntaxKind.IndexerDeclaration:
				return ExplicitInterfaceHelpers.GetMemberName(outerBinder, basePropertyDeclarationSyntax.Modifiers, explicitInterfaceSpecifier, "this[]");
			case SyntaxKind.EventFieldDeclaration:
			case SyntaxKind.EventDeclaration:
			{
				EventDeclarationSyntax eventDeclarationSyntax = (EventDeclarationSyntax)basePropertyDeclarationSyntax;
				return ExplicitInterfaceHelpers.GetMemberName(outerBinder, eventDeclarationSyntax.Modifiers, explicitInterfaceSpecifier, eventDeclarationSyntax.Identifier.ValueText);
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(basePropertyDeclarationSyntax.Kind());
			}
		}

		private SourceMemberMethodSymbol GetMethodSymbol(BaseMethodDeclarationSyntax baseMethodDeclarationSyntax, Binder outerBinder)
		{
			if (baseMethodDeclarationSyntax == _memberDeclarationOpt)
			{
				return (SourceMemberMethodSymbol)_memberOpt;
			}
			NamedTypeSymbol containerType = GetContainerType(outerBinder, baseMethodDeclarationSyntax);
			if ((object)containerType == null)
			{
				return null;
			}
			string methodName = GetMethodName(baseMethodDeclarationSyntax, outerBinder);
			return (SourceMemberMethodSymbol)GetMemberSymbol(methodName, baseMethodDeclarationSyntax.FullSpan, containerType, SymbolKind.Method);
		}

		private SourcePropertySymbol GetPropertySymbol(BasePropertyDeclarationSyntax basePropertyDeclarationSyntax, Binder outerBinder)
		{
			if (basePropertyDeclarationSyntax == _memberDeclarationOpt)
			{
				return (SourcePropertySymbol)_memberOpt;
			}
			NamedTypeSymbol containerType = GetContainerType(outerBinder, basePropertyDeclarationSyntax);
			if ((object)containerType == null)
			{
				return null;
			}
			string propertyOrEventName = GetPropertyOrEventName(basePropertyDeclarationSyntax, outerBinder);
			return (SourcePropertySymbol)GetMemberSymbol(propertyOrEventName, basePropertyDeclarationSyntax.Span, containerType, SymbolKind.Property);
		}

		private SourceEventSymbol GetEventSymbol(EventDeclarationSyntax eventDeclarationSyntax, Binder outerBinder)
		{
			if (eventDeclarationSyntax == _memberDeclarationOpt)
			{
				return (SourceEventSymbol)_memberOpt;
			}
			NamedTypeSymbol containerType = GetContainerType(outerBinder, eventDeclarationSyntax);
			if ((object)containerType == null)
			{
				return null;
			}
			string propertyOrEventName = GetPropertyOrEventName(eventDeclarationSyntax, outerBinder);
			return (SourceEventSymbol)GetMemberSymbol(propertyOrEventName, eventDeclarationSyntax.Span, containerType, SymbolKind.Event);
		}

		private Symbol GetMemberSymbol(string memberName, TextSpan memberSpan, NamedTypeSymbol container, SymbolKind kind)
		{
			if (container is SourceMemberContainerTypeSymbol { HasPrimaryConstructor: not false } sourceMemberContainerTypeSymbol)
			{
				foreach (Symbol item in sourceMemberContainerTypeSymbol.GetMembersToMatchAgainstDeclarationSpan())
				{
					if (!item.IsAccessor() && item.Name == memberName && checkSymbol(item, memberSpan, kind, out var result))
					{
						return result;
					}
				}
			}
			else
			{
				foreach (Symbol member in container.GetMembers(memberName))
				{
					if (checkSymbol(member, memberSpan, kind, out var result2))
					{
						return result2;
					}
				}
			}
			return null;
			bool checkSymbol(Symbol sym, TextSpan span, SymbolKind symbolKind, out Symbol reference)
			{
				reference = sym;
				if (sym.Kind != symbolKind)
				{
					return false;
				}
				if ((symbolKind == SymbolKind.Event || symbolKind == SymbolKind.Method || symbolKind == SymbolKind.Property) ? true : false)
				{
					if (InSpan(sym.GetFirstLocation(), syntaxTree, span))
					{
						return true;
					}
					Symbol partialImplementationPart = sym.GetPartialImplementationPart();
					if ((object)partialImplementationPart != null && InSpan(partialImplementationPart.GetFirstLocation(), syntaxTree, span))
					{
						reference = partialImplementationPart;
						return true;
					}
				}
				else if (InSpan(sym.Locations, syntaxTree, span))
				{
					return true;
				}
				return false;
			}
		}

		private static bool InSpan(Location location, SyntaxTree syntaxTree, TextSpan span)
		{
			if (location.SourceTree == syntaxTree)
			{
				return span.Contains(location.SourceSpan);
			}
			return false;
		}

		private static bool InSpan(ImmutableArray<Location> locations, SyntaxTree syntaxTree, TextSpan span)
		{
			foreach (Location item in locations)
			{
				if (InSpan(item, syntaxTree, span))
				{
					return true;
				}
			}
			return false;
		}

		public override Binder VisitDelegateDeclaration(DelegateDeclarationSyntax parent)
		{
			if (!LookupPosition.IsInDelegateDeclaration(_position, parent))
			{
				return VisitCore(parent.Parent);
			}
			BinderCacheKey key = CreateBinderCacheKey(parent, NodeUsage.Normal);
			if (!binderCache.TryGetValue(key, out var value))
			{
				Binder binder = VisitCore(parent.Parent);
				SourceNamedTypeSymbol sourceTypeMember = ((NamespaceOrTypeSymbol)binder.ContainingMemberOrLambda).GetSourceTypeMember(parent);
				value = new InContainerBinder(sourceTypeMember, binder);
				if (parent.TypeParameterList != null)
				{
					value = new WithClassTypeParametersBinder(sourceTypeMember, value);
				}
				value = value.SetOrClearUnsafeRegionIfNecessary(parent.Modifiers);
				binderCache.TryAdd(key, value);
			}
			return value;
		}

		public override Binder VisitEnumDeclaration(EnumDeclarationSyntax parent)
		{
			if (!LookupPosition.IsBetweenTokens(_position, parent.OpenBraceToken, parent.CloseBraceToken) && !LookupPosition.IsInAttributeSpecification(_position, parent.AttributeLists))
			{
				return VisitCore(parent.Parent);
			}
			BinderCacheKey key = CreateBinderCacheKey(parent, NodeUsage.Normal);
			if (!binderCache.TryGetValue(key, out var value))
			{
				Binder binder = VisitCore(parent.Parent);
				value = new InContainerBinder(((NamespaceOrTypeSymbol)binder.ContainingMemberOrLambda).GetSourceTypeMember(parent.Identifier.ValueText, 0, SyntaxKind.EnumDeclaration, parent), binder);
				value = value.SetOrClearUnsafeRegionIfNecessary(parent.Modifiers);
				binderCache.TryAdd(key, value);
			}
			return value;
		}

		private Binder VisitTypeDeclarationCore(TypeDeclarationSyntax parent)
		{
			if (!LookupPosition.IsInTypeDeclaration(_position, parent))
			{
				return VisitCore(parent.Parent);
			}
			NodeUsage extraInfo = NodeUsage.Normal;
			if (parent.OpenBraceToken != default(SyntaxToken) && parent.CloseBraceToken != default(SyntaxToken) && LookupPosition.IsBetweenTokens(_position, parent.OpenBraceToken, parent.CloseBraceToken))
			{
				extraInfo = NodeUsage.MethodBody;
			}
			else if (LookupPosition.IsInAttributeSpecification(_position, parent.AttributeLists))
			{
				extraInfo = NodeUsage.MethodBody;
			}
			else if (LookupPosition.IsInTypeParameterList(_position, parent))
			{
				extraInfo = NodeUsage.MethodBody;
			}
			else if (LookupPosition.IsBetweenTokens(_position, parent.Keyword, parent.OpenBraceToken))
			{
				extraInfo = NodeUsage.NamedTypeBaseListOrParameterList;
			}
			return VisitTypeDeclarationCore(parent, extraInfo);
		}

		internal Binder VisitTypeDeclarationCore(TypeDeclarationSyntax parent, NodeUsage extraInfo)
		{
			BinderCacheKey key = CreateBinderCacheKey(parent, extraInfo);
			if (!binderCache.TryGetValue(key, out var value))
			{
				value = VisitCore(parent.Parent);
				if (extraInfo != NodeUsage.Normal)
				{
					SourceNamedTypeSymbol sourceTypeMember = ((NamespaceOrTypeSymbol)value.ContainingMemberOrLambda).GetSourceTypeMember(parent);
					if (extraInfo == NodeUsage.NamedTypeBaseListOrParameterList)
					{
						value = new WithClassTypeParametersBinder(sourceTypeMember, value);
					}
					else
					{
						value = new WithPrimaryConstructorParametersBinder(sourceTypeMember, value);
						value = new InContainerBinder(sourceTypeMember, value);
						if (parent.TypeParameterList != null)
						{
							value = new WithClassTypeParametersBinder(sourceTypeMember, value);
						}
						if (sourceTypeMember.IsExtension)
						{
							value = new WithExtensionParameterBinder(sourceTypeMember, value);
						}
					}
				}
				value = value.SetOrClearUnsafeRegionIfNecessary(parent.Modifiers);
				binderCache.TryAdd(key, value);
			}
			return value;
		}

		public override Binder VisitClassDeclaration(ClassDeclarationSyntax node)
		{
			return VisitTypeDeclarationCore(node);
		}

		public override Binder VisitStructDeclaration(StructDeclarationSyntax node)
		{
			return VisitTypeDeclarationCore(node);
		}

		public override Binder VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
		{
			return VisitTypeDeclarationCore(node);
		}

		public override Binder VisitRecordDeclaration(RecordDeclarationSyntax node)
		{
			return VisitTypeDeclarationCore(node);
		}

		public override Binder VisitExtensionBlockDeclaration(ExtensionBlockDeclarationSyntax node)
		{
			return VisitTypeDeclarationCore(node);
		}

		public sealed override Binder VisitNamespaceDeclaration(NamespaceDeclarationSyntax parent)
		{
			if (!LookupPosition.IsInNamespaceDeclaration(_position, parent))
			{
				return VisitCore(parent.Parent);
			}
			bool inBody = LookupPosition.IsBetweenTokens(_position, parent.OpenBraceToken, parent.CloseBraceToken);
			bool inUsing = IsInUsing(parent);
			return VisitNamespaceDeclaration(parent, _position, inBody, inUsing);
		}

		public override Binder VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax parent)
		{
			if (!LookupPosition.IsInNamespaceDeclaration(_position, parent))
			{
				return VisitCore(parent.Parent);
			}
			bool inBody = _position >= parent.SemicolonToken.EndPosition;
			bool inUsing = IsInUsing(parent);
			return VisitNamespaceDeclaration(parent, _position, inBody, inUsing);
		}

		internal Binder VisitNamespaceDeclaration(BaseNamespaceDeclarationSyntax parent, int position, bool inBody, bool inUsing)
		{
			NodeUsage usage = (inUsing ? NodeUsage.MethodBody : (inBody ? NodeUsage.MethodTypeParameters : NodeUsage.Normal));
			BinderCacheKey key = CreateBinderCacheKey(parent, usage);
			if (!binderCache.TryGetValue(key, out var value))
			{
				CSharpSyntaxNode parent2 = parent.Parent;
				Binder binder = ((!InScript || parent2.Kind() != SyntaxKind.CompilationUnit) ? _factory.GetBinder(parent.Parent, position) : VisitCompilationUnit((CompilationUnitSyntax)parent2, inUsing: false, inScript: false));
				value = (inBody ? MakeNamespaceBinder(parent, parent.Name, binder, inUsing) : binder);
				binderCache.TryAdd(key, value);
			}
			return value;
		}

		private static Binder MakeNamespaceBinder(CSharpSyntaxNode node, NameSyntax name, Binder outer, bool inUsing)
		{
			if (name is QualifiedNameSyntax qualifiedNameSyntax)
			{
				outer = MakeNamespaceBinder(qualifiedNameSyntax.Left, qualifiedNameSyntax.Left, outer, inUsing: false);
				name = qualifiedNameSyntax.Right;
			}
			NamespaceOrTypeSymbol namespaceOrTypeSymbol = ((!(outer is InContainerBinder inContainerBinder)) ? outer.Compilation.GlobalNamespace : inContainerBinder.Container);
			NamespaceSymbol nestedNamespace = ((NamespaceSymbol)namespaceOrTypeSymbol).GetNestedNamespace(name);
			if ((object)nestedNamespace == null)
			{
				return outer;
			}
			if (node is BaseNamespaceDeclarationSyntax declarationSyntax)
			{
				outer = AddInImportsBinders((SourceNamespaceSymbol)outer.Compilation.SourceModule.GetModuleNamespace(nestedNamespace), declarationSyntax, outer, inUsing);
			}
			return new InContainerBinder(nestedNamespace, outer);
		}

		public override Binder VisitCompilationUnit(CompilationUnitSyntax parent)
		{
			return VisitCompilationUnit(parent, IsInUsing(parent), InScript);
		}

		internal Binder VisitCompilationUnit(CompilationUnitSyntax compilationUnit, bool inUsing, bool inScript)
		{
			if (compilationUnit != syntaxTree.GetRoot())
			{
				throw new ArgumentOutOfRangeException("compilationUnit", "node not part of tree");
			}
			NodeUsage usage = ((!inUsing) ? (inScript ? NodeUsage.MethodBody : NodeUsage.Normal) : ((!inScript) ? NodeUsage.MethodTypeParameters : NodeUsage.NamedTypeBaseListOrParameterList));
			BinderCacheKey key = CreateBinderCacheKey(compilationUnit, usage);
			if (!binderCache.TryGetValue(key, out var value))
			{
				value = buckStopsHereBinder;
				if (inScript)
				{
					bool flag = compilation.IsSubmissionSyntaxTree(compilationUnit.SyntaxTree);
					NamedTypeSymbol scriptClass = compilation.ScriptClass;
					bool isSubmissionClass = scriptClass.IsSubmissionClass;
					if (!inUsing)
					{
						value = WithUsingNamespacesAndTypesBinder.Create(compilation.GlobalImports, value, withImportChainEntry: true);
						if (isSubmissionClass)
						{
							value = WithUsingNamespacesAndTypesBinder.Create((SourceNamespaceSymbol)compilation.SourceModule.GlobalNamespace, compilationUnit, value, (compilation.PreviousSubmission != null) & flag, withImportChainEntry: true);
						}
					}
					value = new InContainerBinder(compilation.GlobalNamespace, value);
					if (compilation.HostObjectType != null)
					{
						value = new HostObjectModelBinder(value);
					}
					if (isSubmissionClass)
					{
						value = new InSubmissionClassBinder(scriptClass, value, compilationUnit, inUsing);
					}
					else
					{
						value = AddInImportsBinders((SourceNamespaceSymbol)compilation.SourceModule.GlobalNamespace, compilationUnit, value, inUsing);
						value = new InContainerBinder(scriptClass, value);
					}
				}
				else
				{
					NamespaceSymbol globalNamespace = compilation.GlobalNamespace;
					value = AddInImportsBinders((SourceNamespaceSymbol)compilation.SourceModule.GlobalNamespace, compilationUnit, value, inUsing);
					value = new InContainerBinder(globalNamespace, value);
					if (!inUsing)
					{
						SynthesizedSimpleProgramEntryPointSymbol simpleProgramEntryPoint = SynthesizedSimpleProgramEntryPointSymbol.GetSimpleProgramEntryPoint(compilation, compilationUnit, fallbackToMainEntryPoint: true);
						if ((object)simpleProgramEntryPoint != null)
						{
							ExecutableCodeBinder bodyBinder = simpleProgramEntryPoint.GetBodyBinder(_factory._ignoreAccessibility);
							value = new SimpleProgramUnitBinder(value, (SimpleProgramBinder)bodyBinder.GetBinder(simpleProgramEntryPoint.SyntaxNode));
						}
					}
				}
				binderCache.TryAdd(key, value);
			}
			return value;
		}

		private static Binder AddInImportsBinders(SourceNamespaceSymbol declaringSymbol, CSharpSyntaxNode declarationSyntax, Binder next, bool inUsing)
		{
			if (inUsing)
			{
				return WithExternAliasesBinder.Create(declaringSymbol, declarationSyntax, next);
			}
			return WithExternAndUsingAliasesBinder.Create(declaringSymbol, declarationSyntax, WithUsingNamespacesAndTypesBinder.Create(declaringSymbol, declarationSyntax, next));
		}

		internal static BinderCacheKey CreateBinderCacheKey(CSharpSyntaxNode node, NodeUsage usage)
		{
			return new BinderCacheKey(node, usage);
		}

		private bool IsInUsing(CSharpSyntaxNode containingNode)
		{
			TextSpan span = containingNode.Span;
			SyntaxToken syntaxToken;
			if (containingNode.Kind() != SyntaxKind.CompilationUnit && _position == span.End)
			{
				syntaxToken = containingNode.GetLastToken();
			}
			else
			{
				if (_position < span.Start || _position > span.End)
				{
					return false;
				}
				syntaxToken = containingNode.FindToken(_position);
			}
			SyntaxNode parent = syntaxToken.Parent;
			while (parent != null && parent != containingNode)
			{
				if (parent.IsKind(SyntaxKind.UsingDirective) && parent.Parent == containingNode)
				{
					return true;
				}
				parent = parent.Parent;
			}
			return false;
		}

		public override Binder VisitDocumentationCommentTrivia(DocumentationCommentTriviaSyntax parent)
		{
			return VisitCore(parent.ParentTrivia.Token.Parent);
		}

		public override Binder VisitCrefParameter(CrefParameterSyntax parent)
		{
			XmlCrefAttributeSyntax parent2 = parent.FirstAncestorOrSelf<XmlCrefAttributeSyntax>(null, ascendOutOfTrivia: false);
			return VisitXmlCrefAttributeInternal(parent2, NodeUsage.MethodTypeParameters);
		}

		public override Binder VisitConversionOperatorMemberCref(ConversionOperatorMemberCrefSyntax parent)
		{
			if (parent.Type.Span.Contains(_position))
			{
				XmlCrefAttributeSyntax parent2 = parent.FirstAncestorOrSelf<XmlCrefAttributeSyntax>(null, ascendOutOfTrivia: false);
				return VisitXmlCrefAttributeInternal(parent2, NodeUsage.MethodTypeParameters);
			}
			return base.VisitConversionOperatorMemberCref(parent);
		}

		public override Binder VisitXmlCrefAttribute(XmlCrefAttributeSyntax parent)
		{
			if (!LookupPosition.IsInXmlAttributeValue(_position, parent))
			{
				return VisitCore(parent.Parent);
			}
			NodeUsage extraInfo = NodeUsage.Normal;
			return VisitXmlCrefAttributeInternal(parent, extraInfo);
		}

		private Binder VisitXmlCrefAttributeInternal(XmlCrefAttributeSyntax parent, NodeUsage extraInfo)
		{
			BinderCacheKey key = CreateBinderCacheKey(parent, extraInfo);
			if (!binderCache.TryGetValue(key, out var value))
			{
				CrefSyntax cref = parent.Cref;
				MemberDeclarationSyntax associatedMemberForXmlSyntax = GetAssociatedMemberForXmlSyntax(parent);
				bool inParameterOrReturnType = extraInfo == NodeUsage.MethodTypeParameters;
				value = ((associatedMemberForXmlSyntax == null) ? MakeCrefBinderInternal(cref, VisitCore(parent.Parent), inParameterOrReturnType) : MakeCrefBinder(cref, associatedMemberForXmlSyntax, _factory, inParameterOrReturnType));
				binderCache.TryAdd(key, value);
			}
			return value;
		}

		public override Binder VisitXmlNameAttribute(XmlNameAttributeSyntax parent)
		{
			if (!LookupPosition.IsInXmlAttributeValue(_position, parent))
			{
				return VisitCore(parent.Parent);
			}
			XmlNameAttributeElementKind elementKind = parent.GetElementKind();
			BinderCacheKey key = CreateBinderCacheKey(usage: elementKind switch
			{
				XmlNameAttributeElementKind.Parameter => NodeUsage.MethodTypeParameters, 
				XmlNameAttributeElementKind.ParameterReference => NodeUsage.MethodBody, 
				XmlNameAttributeElementKind.TypeParameter => NodeUsage.NamedTypeBaseListOrParameterList, 
				XmlNameAttributeElementKind.TypeParameterReference => NodeUsage.DocumentationCommentTypeParameterReference, 
				_ => throw ExceptionUtilities.UnexpectedValue(elementKind), 
			}, node: GetEnclosingDocumentationComment(parent));
			if (!binderCache.TryGetValue(key, out var value))
			{
				value = buckStopsHereBinder;
				Binder binder = VisitCore(GetEnclosingDocumentationComment(parent));
				if (binder != null)
				{
					value = value.WithContainingMemberOrLambda(binder.ContainingMemberOrLambda);
				}
				MemberDeclarationSyntax associatedMemberForXmlSyntax = GetAssociatedMemberForXmlSyntax(parent);
				if (associatedMemberForXmlSyntax != null)
				{
					switch (elementKind)
					{
					case XmlNameAttributeElementKind.Parameter:
					case XmlNameAttributeElementKind.ParameterReference:
						value = GetParameterNameAttributeValueBinder(associatedMemberForXmlSyntax, elementKind == XmlNameAttributeElementKind.ParameterReference, value);
						break;
					case XmlNameAttributeElementKind.TypeParameter:
						value = GetTypeParameterNameAttributeValueBinder(associatedMemberForXmlSyntax, includeContainingSymbols: false, value);
						break;
					case XmlNameAttributeElementKind.TypeParameterReference:
						value = GetTypeParameterNameAttributeValueBinder(associatedMemberForXmlSyntax, includeContainingSymbols: true, value);
						break;
					}
				}
				binderCache.TryAdd(key, value);
			}
			return value;
		}

		private Binder GetParameterNameAttributeValueBinder(MemberDeclarationSyntax memberSyntax, bool isParamRef, Binder nextBinder)
		{
			ParameterSymbol extensionParameter;
			if (memberSyntax is BaseMethodDeclarationSyntax baseMethodDeclarationSyntax)
			{
				Binder outerBinder = VisitCore(memberSyntax.Parent);
				MethodSymbol methodSymbol = GetMethodSymbol(baseMethodDeclarationSyntax, outerBinder);
				if (isParamRef && methodSymbol.TryGetInstanceExtensionParameter(out extensionParameter))
				{
					nextBinder = new WithExtensionParameterBinder(methodSymbol.ContainingType, nextBinder);
				}
				if (methodSymbol.ParameterCount > 0)
				{
					return new WithParametersBinder(methodSymbol.Parameters, nextBinder);
				}
				return nextBinder;
			}
			if (memberSyntax is ExtensionBlockDeclarationSyntax)
			{
				return new WithExtensionParameterBinder(((NamespaceOrTypeSymbol)VisitCore(memberSyntax).ContainingMemberOrLambda).GetSourceTypeMember((TypeDeclarationSyntax)memberSyntax), nextBinder);
			}
			if (memberSyntax is TypeDeclarationSyntax typeDeclarationSyntax)
			{
				ParameterListSyntax parameterList = typeDeclarationSyntax.ParameterList;
				if (parameterList != null && parameterList.ParameterCount > 0)
				{
					_ = typeDeclarationSyntax.ParameterList;
					SynthesizedPrimaryConstructor primaryConstructor = ((NamespaceOrTypeSymbol)VisitCore(memberSyntax).ContainingMemberOrLambda).GetSourceTypeMember((TypeDeclarationSyntax)memberSyntax).PrimaryConstructor;
					if (primaryConstructor.SyntaxRef.SyntaxTree == memberSyntax.SyntaxTree && primaryConstructor.GetSyntax() == memberSyntax)
					{
						return new WithParametersBinder(primaryConstructor.Parameters, nextBinder);
					}
				}
			}
			switch (memberSyntax.Kind())
			{
			case SyntaxKind.PropertyDeclaration:
			case SyntaxKind.IndexerDeclaration:
			{
				Binder outerBinder2 = VisitCore(memberSyntax.Parent);
				BasePropertyDeclarationSyntax basePropertyDeclarationSyntax = (BasePropertyDeclarationSyntax)memberSyntax;
				PropertySymbol propertySymbol = GetPropertySymbol(basePropertyDeclarationSyntax, outerBinder2);
				ImmutableArray<ParameterSymbol> immutableArray = propertySymbol.Parameters;
				if (isParamRef && propertySymbol.TryGetInstanceExtensionParameter(out extensionParameter))
				{
					nextBinder = new WithExtensionParameterBinder(propertySymbol.ContainingType, nextBinder);
				}
				if ((object)propertySymbol.SetMethod != null)
				{
					immutableArray = immutableArray.Add(propertySymbol.SetMethod.Parameters.Last());
				}
				if (immutableArray.Any())
				{
					nextBinder = new WithParametersBinder(immutableArray, nextBinder);
				}
				return nextBinder;
			}
			case SyntaxKind.DelegateDeclaration:
			{
				ImmutableArray<ParameterSymbol> parameters = ((NamespaceOrTypeSymbol)VisitCore(memberSyntax.Parent).ContainingMemberOrLambda).GetSourceTypeMember((DelegateDeclarationSyntax)memberSyntax).DelegateInvokeMethod.Parameters;
				if (parameters.Any())
				{
					return new WithParametersBinder(parameters, nextBinder);
				}
				break;
			}
			}
			return nextBinder;
		}

		private Binder GetTypeParameterNameAttributeValueBinder(MemberDeclarationSyntax memberSyntax, bool includeContainingSymbols, Binder nextBinder)
		{
			if (includeContainingSymbols)
			{
				NamedTypeSymbol containingType = VisitCore(memberSyntax.Parent).ContainingType;
				while ((object)containingType != null)
				{
					if (containingType.Arity > 0)
					{
						nextBinder = new WithClassTypeParametersBinder(containingType, nextBinder);
					}
					containingType = containingType.ContainingType;
				}
			}
			if (memberSyntax is TypeDeclarationSyntax { Arity: >0 } typeDeclarationSyntax)
			{
				return new WithClassTypeParametersBinder(((NamespaceOrTypeSymbol)VisitCore(memberSyntax.Parent).ContainingMemberOrLambda).GetSourceTypeMember(typeDeclarationSyntax), nextBinder);
			}
			if (memberSyntax.Kind() == SyntaxKind.MethodDeclaration)
			{
				MethodDeclarationSyntax methodDeclarationSyntax = (MethodDeclarationSyntax)memberSyntax;
				if (methodDeclarationSyntax.Arity > 0)
				{
					Binder outerBinder = VisitCore(memberSyntax.Parent);
					return new WithMethodTypeParametersBinder(GetMethodSymbol(methodDeclarationSyntax, outerBinder), nextBinder);
				}
			}
			else if (memberSyntax.Kind() == SyntaxKind.DelegateDeclaration)
			{
				SourceNamedTypeSymbol sourceTypeMember = ((NamespaceOrTypeSymbol)VisitCore(memberSyntax.Parent).ContainingMemberOrLambda).GetSourceTypeMember((DelegateDeclarationSyntax)memberSyntax);
				if (sourceTypeMember.TypeParameters.Any())
				{
					return new WithClassTypeParametersBinder(sourceTypeMember, nextBinder);
				}
			}
			return nextBinder;
		}
	}

	internal readonly struct BinderCacheKey(CSharpSyntaxNode syntaxNode, NodeUsage usage) : IEquatable<BinderCacheKey>
	{
		public readonly CSharpSyntaxNode syntaxNode = syntaxNode;

		public readonly NodeUsage usage = usage;

		bool IEquatable<BinderCacheKey>.Equals(BinderCacheKey other)
		{
			if (syntaxNode == other.syntaxNode)
			{
				return usage == other.usage;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(syntaxNode.GetHashCode(), (int)usage);
		}

		public override bool Equals(object obj)
		{
			throw new NotSupportedException();
		}
	}

	internal enum NodeUsage : byte
	{
		Normal = 0,
		MethodTypeParameters = 1,
		MethodBody = 2,
		ConstructorBodyOrInitializer = MethodTypeParameters,
		AccessorBody = MethodTypeParameters,
		OperatorBody = MethodTypeParameters,
		NamedTypeBodyOrTypeParameters = MethodBody,
		NamedTypeBaseListOrParameterList = 4,
		NamespaceBody = MethodTypeParameters,
		NamespaceUsings = MethodBody,
		CompilationUnitUsings = MethodTypeParameters,
		CompilationUnitScript = MethodBody,
		CompilationUnitScriptUsings = NamedTypeBaseListOrParameterList,
		DocumentationCommentParameter = MethodTypeParameters,
		DocumentationCommentParameterReference = MethodBody,
		DocumentationCommentTypeParameter = NamedTypeBaseListOrParameterList,
		DocumentationCommentTypeParameterReference = 8,
		CrefParameterOrReturnType = MethodTypeParameters
	}

	private readonly ConcurrentCache<BinderCacheKey, Binder> _binderCache;

	private readonly CSharpCompilation _compilation;

	private readonly SyntaxTree _syntaxTree;

	private readonly BuckStopsHereBinder _buckStopsHereBinder;

	private readonly bool _ignoreAccessibility;

	private static readonly ObjectPool<BinderFactoryVisitor> s_binderFactoryVisitorPool = new ObjectPool<BinderFactoryVisitor>(() => new BinderFactoryVisitor(), 64);

	private readonly ObjectPool<BinderFactoryVisitor> _binderFactoryVisitorPool;

	internal SyntaxTree SyntaxTree => _syntaxTree;

	private bool InScript => _syntaxTree.Options.Kind == SourceCodeKind.Script;

	internal static Binder MakeCrefBinder(CrefSyntax crefSyntax, MemberDeclarationSyntax memberSyntax, BinderFactory factory, bool inParameterOrReturnType = false)
	{
		Binder binder = ((memberSyntax is BaseTypeDeclarationSyntax baseTypeDeclaration) ? getBinder(baseTypeDeclaration) : factory.GetBinder(memberSyntax));
		return MakeCrefBinderInternal(crefSyntax, binder, inParameterOrReturnType);
		Binder getBinder(BaseTypeDeclarationSyntax baseTypeDeclarationSyntax)
		{
			if (baseTypeDeclarationSyntax is TypeDeclarationSyntax typeDecl && baseTypeDeclarationSyntax.SemicolonToken.RawKind == 8212 && baseTypeDeclarationSyntax.OpenBraceToken.RawKind == 0)
			{
				return factory.GetInTypeBodyBinder(typeDecl);
			}
			return factory.GetBinder(baseTypeDeclarationSyntax, baseTypeDeclarationSyntax.OpenBraceToken.SpanStart);
		}
	}

	private static Binder MakeCrefBinderInternal(CrefSyntax crefSyntax, Binder binder, bool inParameterOrReturnType)
	{
		BinderFlags binderFlags = BinderFlags.SuppressConstraintChecks | BinderFlags.Cref | BinderFlags.UnsafeRegion;
		if (inParameterOrReturnType)
		{
			binderFlags |= BinderFlags.CrefParameterOrReturnType;
		}
		binder = binder.WithAdditionalFlags(binderFlags);
		binder = new WithCrefTypeParametersBinder(crefSyntax, binder);
		return binder;
	}

	internal static MemberDeclarationSyntax GetAssociatedMemberForXmlSyntax(CSharpSyntaxNode xmlSyntax)
	{
		SyntaxTrivia parentTrivia = GetEnclosingDocumentationComment(xmlSyntax).ParentTrivia;
		for (CSharpSyntaxNode cSharpSyntaxNode = (CSharpSyntaxNode)parentTrivia.Token.Parent; cSharpSyntaxNode != null; cSharpSyntaxNode = cSharpSyntaxNode.Parent)
		{
			if (cSharpSyntaxNode is MemberDeclarationSyntax memberDeclarationSyntax)
			{
				if (!memberDeclarationSyntax.GetLeadingTrivia().Contains(parentTrivia))
				{
					return null;
				}
				return memberDeclarationSyntax;
			}
		}
		return null;
	}

	private static DocumentationCommentTriviaSyntax GetEnclosingDocumentationComment(CSharpSyntaxNode xmlSyntax)
	{
		CSharpSyntaxNode cSharpSyntaxNode = xmlSyntax;
		while (!SyntaxFacts.IsDocumentationCommentTrivia(cSharpSyntaxNode.Kind()))
		{
			cSharpSyntaxNode = cSharpSyntaxNode.Parent;
		}
		return (DocumentationCommentTriviaSyntax)cSharpSyntaxNode;
	}

	internal BinderFactory(CSharpCompilation compilation, SyntaxTree syntaxTree, bool ignoreAccessibility, ObjectPool<BinderFactoryVisitor> binderFactoryVisitorPoolOpt = null)
	{
		_compilation = compilation;
		_syntaxTree = syntaxTree;
		_ignoreAccessibility = ignoreAccessibility;
		_binderFactoryVisitorPool = binderFactoryVisitorPoolOpt ?? s_binderFactoryVisitorPool;
		_binderCache = new ConcurrentCache<BinderCacheKey, Binder>(50);
		_buckStopsHereBinder = new BuckStopsHereBinder(compilation, FileIdentifier.Create(syntaxTree, compilation.Options.SourceReferenceResolver));
	}

	internal Binder GetBinder(SyntaxNode node, CSharpSyntaxNode memberDeclarationOpt = null, Symbol memberOpt = null)
	{
		int spanStart = node.SpanStart;
		if ((!InScript || node.Kind() != SyntaxKind.CompilationUnit) && node.Parent != null)
		{
			node = node.Parent;
		}
		return GetBinder(node, spanStart, memberDeclarationOpt, memberOpt);
	}

	internal Binder GetBinder(SyntaxNode node, int position, CSharpSyntaxNode memberDeclarationOpt = null, Symbol memberOpt = null)
	{
		BinderFactoryVisitor binderFactoryVisitor = GetBinderFactoryVisitor(position, memberDeclarationOpt, memberOpt);
		Binder result = binderFactoryVisitor.Visit(node);
		ClearBinderFactoryVisitor(binderFactoryVisitor);
		return result;
	}

	private BinderFactoryVisitor GetBinderFactoryVisitor(int position, CSharpSyntaxNode memberDeclarationOpt, Symbol memberOpt)
	{
		BinderFactoryVisitor binderFactoryVisitor = _binderFactoryVisitorPool.Allocate();
		binderFactoryVisitor.Initialize(this, position, memberDeclarationOpt, memberOpt);
		return binderFactoryVisitor;
	}

	private void ClearBinderFactoryVisitor(BinderFactoryVisitor visitor)
	{
		visitor.Clear();
		_binderFactoryVisitorPool.Free(visitor);
	}

	internal InMethodBinder GetPrimaryConstructorInMethodBinder(SynthesizedPrimaryConstructor constructor)
	{
		TypeDeclarationSyntax syntax = constructor.GetSyntax();
		NodeUsage usage = NodeUsage.MethodTypeParameters;
		BinderCacheKey key = BinderFactoryVisitor.CreateBinderCacheKey(syntax, usage);
		if (!_binderCache.TryGetValue(key, out var value))
		{
			value = new InMethodBinder(constructor, GetInTypeBodyBinder(syntax));
			_binderCache.TryAdd(key, value);
		}
		return (InMethodBinder)value;
	}

	internal Binder GetInTypeBodyBinder(TypeDeclarationSyntax typeDecl)
	{
		BinderFactoryVisitor binderFactoryVisitor = GetBinderFactoryVisitor(typeDecl.SpanStart, null, null);
		Binder result = binderFactoryVisitor.VisitTypeDeclarationCore(typeDecl, NodeUsage.MethodBody);
		ClearBinderFactoryVisitor(binderFactoryVisitor);
		return result;
	}

	internal Binder GetInNamespaceBinder(CSharpSyntaxNode unit)
	{
		switch (unit.Kind())
		{
		case SyntaxKind.NamespaceDeclaration:
		case SyntaxKind.FileScopedNamespaceDeclaration:
		{
			BinderFactoryVisitor binderFactoryVisitor2 = GetBinderFactoryVisitor(0, null, null);
			Binder result2 = binderFactoryVisitor2.VisitNamespaceDeclaration((BaseNamespaceDeclarationSyntax)unit, unit.SpanStart, inBody: true, inUsing: false);
			ClearBinderFactoryVisitor(binderFactoryVisitor2);
			return result2;
		}
		case SyntaxKind.CompilationUnit:
		{
			BinderFactoryVisitor binderFactoryVisitor = GetBinderFactoryVisitor(0, null, null);
			Binder result = binderFactoryVisitor.VisitCompilationUnit((CompilationUnitSyntax)unit, inUsing: false, InScript);
			ClearBinderFactoryVisitor(binderFactoryVisitor);
			return result;
		}
		default:
			return null;
		}
	}
}
