using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.SymbolDisplay;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class SymbolDisplayVisitor : AbstractSymbolDisplayVisitor
{
	private static readonly ObjectPool<SymbolDisplayVisitor> s_visitorPool = new ObjectPool<SymbolDisplayVisitor>((ObjectPool<SymbolDisplayVisitor> pool) => new SymbolDisplayVisitor(pool), 128);

	private readonly ObjectPool<SymbolDisplayVisitor> _pool;

	private bool _escapeKeywordIdentifiers;

	private IDictionary<INamespaceOrTypeSymbol, IAliasSymbol>? _lazyAliasMap;

	private const string IL_KEYWORD_MODOPT = "modopt";

	private const string IL_KEYWORD_MODREQ = "modreq";

	private IDictionary<INamespaceOrTypeSymbol, IAliasSymbol> AliasMap
	{
		get
		{
			IDictionary<INamespaceOrTypeSymbol, IAliasSymbol> lazyAliasMap = _lazyAliasMap;
			if (lazyAliasMap != null)
			{
				return lazyAliasMap;
			}
			lazyAliasMap = CreateAliasMap();
			return Interlocked.CompareExchange(ref _lazyAliasMap, lazyAliasMap, null) ?? lazyAliasMap;
		}
	}

	private SymbolDisplayVisitor(ObjectPool<SymbolDisplayVisitor> pool)
	{
		_pool = pool;
	}

	public static SymbolDisplayVisitor GetInstance(ArrayBuilder<SymbolDisplayPart> builder, SymbolDisplayFormat format, SemanticModel? semanticModelOpt, int positionOpt)
	{
		SymbolDisplayVisitor symbolDisplayVisitor = s_visitorPool.Allocate();
		symbolDisplayVisitor.Initialize(builder, format, isFirstSymbolVisited: true, semanticModelOpt, positionOpt, inNamespaceOrType: false);
		return symbolDisplayVisitor;
	}

	private static SymbolDisplayVisitor GetInstance(ArrayBuilder<SymbolDisplayPart> builder, SymbolDisplayFormat format, SemanticModel? semanticModelOpt, int positionOpt, bool escapeKeywordIdentifiers, IDictionary<INamespaceOrTypeSymbol, IAliasSymbol>? aliasMap, bool isFirstSymbolVisited, bool inNamespaceOrType = false)
	{
		SymbolDisplayVisitor symbolDisplayVisitor = s_visitorPool.Allocate();
		symbolDisplayVisitor.Initialize(builder, format, isFirstSymbolVisited, semanticModelOpt, positionOpt, inNamespaceOrType);
		symbolDisplayVisitor._escapeKeywordIdentifiers = escapeKeywordIdentifiers;
		symbolDisplayVisitor._lazyAliasMap = aliasMap;
		return symbolDisplayVisitor;
	}

	protected new void Initialize(ArrayBuilder<SymbolDisplayPart> builder, SymbolDisplayFormat format, bool isFirstSymbolVisited, SemanticModel? semanticModelOpt, int positionOpt, bool inNamespaceOrType)
	{
		base.Initialize(builder, format, isFirstSymbolVisited, semanticModelOpt, positionOpt, inNamespaceOrType);
		_escapeKeywordIdentifiers = format.MiscellaneousOptions.IncludesOption(SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);
	}

	public override void Free()
	{
		base.Free();
		_escapeKeywordIdentifiers = false;
		_lazyAliasMap = null;
		_pool.Free(this);
	}

	protected override AbstractSymbolDisplayVisitor MakeNotFirstVisitor(bool inNamespaceOrType = false)
	{
		return GetInstance(base.Builder, base.Format, base.SemanticModelOpt, base.PositionOpt, _escapeKeywordIdentifiers, _lazyAliasMap, isFirstSymbolVisited: false, inNamespaceOrType);
	}

	protected override void FreeNotFirstVisitor(AbstractSymbolDisplayVisitor visitor)
	{
		visitor.Free();
	}

	internal SymbolDisplayPart CreatePart(SymbolDisplayPartKind kind, ISymbol? symbol, string text)
	{
		if (text == null)
		{
			return new SymbolDisplayPart(kind, symbol, "?");
		}
		if (!_escapeKeywordIdentifiers)
		{
			return new SymbolDisplayPart(kind, symbol, text);
		}
		if (IsEscapable(kind))
		{
			string identifier = text;
			bool isNamedTypeOrAliasName;
			switch (symbol?.Kind)
			{
			case SymbolKind.Alias:
			case SymbolKind.NamedType:
				isNamedTypeOrAliasName = true;
				break;
			default:
				isNamedTypeOrAliasName = false;
				break;
			}
			text = EscapeIdentifier(identifier, isNamedTypeOrAliasName);
		}
		return new SymbolDisplayPart(kind, symbol, text);
	}

	private static bool IsEscapable(SymbolDisplayPartKind kind)
	{
		switch (kind)
		{
		case SymbolDisplayPartKind.AliasName:
		case SymbolDisplayPartKind.ClassName:
		case SymbolDisplayPartKind.DelegateName:
		case SymbolDisplayPartKind.EnumName:
		case SymbolDisplayPartKind.FieldName:
		case SymbolDisplayPartKind.InterfaceName:
		case SymbolDisplayPartKind.LocalName:
		case SymbolDisplayPartKind.MethodName:
		case SymbolDisplayPartKind.NamespaceName:
		case SymbolDisplayPartKind.ParameterName:
		case SymbolDisplayPartKind.PropertyName:
		case SymbolDisplayPartKind.StructName:
		case SymbolDisplayPartKind.TypeParameterName:
		case SymbolDisplayPartKind.RecordClassName:
		case SymbolDisplayPartKind.RecordStructName:
			return true;
		default:
			return false;
		}
	}

	private static string EscapeIdentifier(string identifier, bool isNamedTypeOrAliasName)
	{
		SyntaxKind syntaxKind = SyntaxFacts.GetKeywordKind(identifier);
		if (((syntaxKind == SyntaxKind.None) & isNamedTypeOrAliasName) && StringComparer.Ordinal.Equals(identifier, "record"))
		{
			syntaxKind = SyntaxKind.RecordKeyword;
		}
		if (syntaxKind != SyntaxKind.None)
		{
			return "@" + identifier;
		}
		return identifier;
	}

	public override void VisitAssembly(IAssemblySymbol symbol)
	{
		string text = ((base.Format.TypeQualificationStyle == SymbolDisplayTypeQualificationStyle.NameOnly) ? symbol.Identity.Name : symbol.Identity.GetDisplayName());
		base.Builder.Add(CreatePart(SymbolDisplayPartKind.AssemblyName, symbol, text));
	}

	public override void VisitModule(IModuleSymbol symbol)
	{
		base.Builder.Add(CreatePart(SymbolDisplayPartKind.ModuleName, symbol, symbol.Name));
	}

	public override void VisitNamespace(INamespaceSymbol symbol)
	{
		if (base.IsMinimizing)
		{
			if (!TryAddAlias(symbol, base.Builder))
			{
				MinimallyQualify(symbol);
			}
			return;
		}
		if (base.IsFirstSymbolVisited && base.Format.KindOptions.IncludesOption(SymbolDisplayKindOptions.IncludeNamespaceKeyword))
		{
			AddKeyword(SyntaxKind.NamespaceKeyword);
			AddSpace();
		}
		if (base.Format.TypeQualificationStyle == SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces)
		{
			INamespaceSymbol containingNamespace = symbol.ContainingNamespace;
			if (ShouldVisitNamespace(containingNamespace))
			{
				containingNamespace.Accept(base.NotFirstVisitor);
				AddPunctuation(containingNamespace.IsGlobalNamespace ? SyntaxKind.ColonColonToken : SyntaxKind.DotToken);
			}
		}
		if (symbol.IsGlobalNamespace)
		{
			AddGlobalNamespace(symbol);
		}
		else
		{
			base.Builder.Add(CreatePart(SymbolDisplayPartKind.NamespaceName, symbol, symbol.Name));
		}
	}

	private void AddGlobalNamespace(INamespaceSymbol globalNamespace)
	{
		switch (base.Format.GlobalNamespaceStyle)
		{
		case SymbolDisplayGlobalNamespaceStyle.Included:
			if (base.IsFirstSymbolVisited)
			{
				base.Builder.Add(CreatePart(SymbolDisplayPartKind.Text, globalNamespace, "<global namespace>"));
			}
			else
			{
				base.Builder.Add(CreatePart(SymbolDisplayPartKind.Keyword, globalNamespace, SyntaxFacts.GetText(SyntaxKind.GlobalKeyword)));
			}
			break;
		case SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining:
			base.Builder.Add(CreatePart(SymbolDisplayPartKind.Text, globalNamespace, "<global namespace>"));
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(base.Format.GlobalNamespaceStyle);
		case SymbolDisplayGlobalNamespaceStyle.Omitted:
			break;
		}
	}

	public override void VisitLocal(ILocalSymbol symbol)
	{
		if (base.Format.LocalOptions.IncludesOption(SymbolDisplayLocalOptions.IncludeRef))
		{
			if (symbol.IsRef)
			{
				if (symbol.ScopedKind == ScopedKind.ScopedRef)
				{
					AddKeyword(SyntaxKind.ScopedKeyword);
					AddSpace();
				}
				AddKeyword(SyntaxKind.RefKeyword);
				AddSpace();
				if (symbol.RefKind == RefKind.In)
				{
					AddKeyword(SyntaxKind.ReadOnlyKeyword);
					AddSpace();
				}
			}
			else if (symbol.ScopedKind == ScopedKind.ScopedValue)
			{
				AddKeyword(SyntaxKind.ScopedKeyword);
				AddSpace();
			}
		}
		if (base.Format.LocalOptions.IncludesOption(SymbolDisplayLocalOptions.IncludeType))
		{
			symbol.Type.Accept(base.NotFirstVisitor);
			AddSpace();
		}
		if (symbol.IsConst)
		{
			base.Builder.Add(CreatePart(SymbolDisplayPartKind.ConstantName, symbol, symbol.Name));
		}
		else
		{
			base.Builder.Add(CreatePart(SymbolDisplayPartKind.LocalName, symbol, symbol.Name));
		}
		if (base.Format.LocalOptions.IncludesOption(SymbolDisplayLocalOptions.IncludeConstantValue) && symbol.IsConst && symbol.HasConstantValue && CanAddConstant(symbol.Type, symbol.ConstantValue))
		{
			AddSpace();
			AddPunctuation(SyntaxKind.EqualsToken);
			AddSpace();
			AddConstantValue(symbol.Type, symbol.ConstantValue);
		}
	}

	public override void VisitDiscard(IDiscardSymbol symbol)
	{
		if (base.Format.LocalOptions.IncludesOption(SymbolDisplayLocalOptions.IncludeType))
		{
			symbol.Type.Accept(base.NotFirstVisitor);
			AddSpace();
		}
		base.Builder.Add(CreatePart(SymbolDisplayPartKind.Punctuation, symbol, "_"));
	}

	public override void VisitRangeVariable(IRangeVariableSymbol symbol)
	{
		if (base.Format.LocalOptions.IncludesOption(SymbolDisplayLocalOptions.IncludeType))
		{
			ITypeSymbol rangeVariableType = GetRangeVariableType(symbol);
			if (rangeVariableType != null && rangeVariableType.TypeKind != TypeKind.Error)
			{
				rangeVariableType.Accept(this);
			}
			else
			{
				base.Builder.Add(CreatePart(SymbolDisplayPartKind.ErrorTypeName, rangeVariableType, "?"));
			}
			AddSpace();
		}
		base.Builder.Add(CreatePart(SymbolDisplayPartKind.RangeVariableName, symbol, symbol.Name));
	}

	public override void VisitLabel(ILabelSymbol symbol)
	{
		base.Builder.Add(CreatePart(SymbolDisplayPartKind.LabelName, symbol, symbol.Name));
	}

	public override void VisitAlias(IAliasSymbol symbol)
	{
		base.Builder.Add(CreatePart(SymbolDisplayPartKind.AliasName, symbol, symbol.Name));
		if (base.Format.LocalOptions.IncludesOption(SymbolDisplayLocalOptions.IncludeType))
		{
			AddPunctuation(SyntaxKind.EqualsToken);
			symbol.Target.Accept(this);
		}
	}

	internal override void VisitPreprocessing(IPreprocessingSymbol symbol)
	{
		SymbolDisplayPart item = new SymbolDisplayPart(SymbolDisplayPartKind.Text, symbol, symbol.Name);
		base.Builder.Add(item);
	}

	protected override void AddSpace()
	{
		base.Builder.Add(CreatePart(SymbolDisplayPartKind.Space, null, " "));
	}

	private void AddPunctuation(SyntaxKind punctuationKind)
	{
		base.Builder.Add(CreatePart(SymbolDisplayPartKind.Punctuation, null, SyntaxFacts.GetText(punctuationKind)));
	}

	private void AddKeyword(SyntaxKind keywordKind)
	{
		base.Builder.Add(CreatePart(SymbolDisplayPartKind.Keyword, null, SyntaxFacts.GetText(keywordKind)));
	}

	private void AddAccessibilityIfNeeded(ISymbol symbol)
	{
		INamedTypeSymbol containingType = symbol.ContainingType;
		if (base.Format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeAccessibility) && (containingType == null || (containingType.TypeKind != TypeKind.Interface && (!IsEnumMember(symbol) & !IsLocalFunction(symbol)))))
		{
			AddAccessibility(symbol);
		}
	}

	private static bool IsLocalFunction(ISymbol symbol)
	{
		if (symbol.Kind != SymbolKind.Method)
		{
			return false;
		}
		return ((IMethodSymbol)symbol).MethodKind == MethodKind.LocalFunction;
	}

	private void AddAccessibility(ISymbol symbol)
	{
		switch (symbol.DeclaredAccessibility)
		{
		case Accessibility.Private:
			AddKeyword(SyntaxKind.PrivateKeyword);
			break;
		case Accessibility.Internal:
			AddKeyword(SyntaxKind.InternalKeyword);
			break;
		case Accessibility.ProtectedAndInternal:
			AddKeyword(SyntaxKind.PrivateKeyword);
			AddSpace();
			AddKeyword(SyntaxKind.ProtectedKeyword);
			break;
		case Accessibility.Protected:
			AddKeyword(SyntaxKind.ProtectedKeyword);
			break;
		case Accessibility.ProtectedOrInternal:
			AddKeyword(SyntaxKind.ProtectedKeyword);
			AddSpace();
			AddKeyword(SyntaxKind.InternalKeyword);
			break;
		case Accessibility.Public:
			AddKeyword(SyntaxKind.PublicKeyword);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.DeclaredAccessibility);
		}
		AddSpace();
	}

	private bool ShouldVisitNamespace(ISymbol containingSymbol)
	{
		if (!(containingSymbol is INamespaceSymbol namespaceSymbol))
		{
			return false;
		}
		if (base.Format.TypeQualificationStyle != SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces)
		{
			return false;
		}
		if (namespaceSymbol.IsGlobalNamespace)
		{
			return base.Format.GlobalNamespaceStyle == SymbolDisplayGlobalNamespaceStyle.Included;
		}
		return true;
	}

	private bool IncludeNamedType([NotNullWhen(true)] INamedTypeSymbol? namedType)
	{
		if (namedType == null)
		{
			return false;
		}
		if (namedType.IsScriptClass && !base.Format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.IncludeScriptType))
		{
			return false;
		}
		if (namedType == base.SemanticModelOpt?.Compilation.ScriptGlobalsType)
		{
			return false;
		}
		return true;
	}

	private static bool IsEnumMember(ISymbol symbol)
	{
		if (symbol != null && symbol.Kind == SymbolKind.Field && symbol.ContainingType != null && symbol.ContainingType.TypeKind == TypeKind.Enum)
		{
			return symbol.Name != "value__";
		}
		return false;
	}

	private void VisitFieldType(IFieldSymbol symbol)
	{
		symbol.Type.Accept(base.NotFirstVisitor);
	}

	public override void VisitField(IFieldSymbol symbol)
	{
		AddAccessibilityIfNeeded(symbol);
		AddMemberModifiersIfNeeded(symbol);
		AddFieldModifiersIfNeeded(symbol);
		if (base.Format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeType) && base.IsFirstSymbolVisited && !IsEnumMember(symbol))
		{
			switch (symbol.RefKind)
			{
			case RefKind.Ref:
				AddRefIfNeeded();
				break;
			case RefKind.In:
				AddRefReadonlyIfNeeded();
				break;
			}
			AddCustomModifiersIfNeeded(symbol.RefCustomModifiers);
			VisitFieldType(symbol);
			AddSpace();
			AddCustomModifiersIfNeeded(symbol.CustomModifiers);
		}
		if (base.Format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeContainingType) && IncludeNamedType(symbol.ContainingType))
		{
			symbol.ContainingType.Accept(base.NotFirstVisitor);
			AddPunctuation(SyntaxKind.DotToken);
		}
		if (!base.Format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.UseMetadataMemberNames) && symbol is Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.FieldSymbol && symbol.AssociatedSymbol is IPropertySymbol symbol2)
		{
			AddPropertyNameAndParameters(symbol2);
			AddPunctuation(SyntaxKind.DotToken);
			AddKeyword(SyntaxKind.FieldKeyword);
		}
		else if (symbol.ContainingType.TypeKind == TypeKind.Enum)
		{
			base.Builder.Add(CreatePart(SymbolDisplayPartKind.EnumMemberName, symbol, symbol.Name));
		}
		else if (symbol.IsConst)
		{
			base.Builder.Add(CreatePart(SymbolDisplayPartKind.ConstantName, symbol, symbol.Name));
		}
		else
		{
			base.Builder.Add(CreatePart(SymbolDisplayPartKind.FieldName, symbol, symbol.Name));
		}
		if (base.IsFirstSymbolVisited && base.Format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeConstantValue) && symbol.IsConst && symbol.HasConstantValue && CanAddConstant(symbol.Type, symbol.ConstantValue))
		{
			AddSpace();
			AddPunctuation(SyntaxKind.EqualsToken);
			AddSpace();
			AddConstantValue(symbol.Type, symbol.ConstantValue, IsEnumMember(symbol));
		}
	}

	private static bool ShouldPropertyDisplayReadOnly(IPropertySymbol property)
	{
		INamedTypeSymbol containingType = property.ContainingType;
		if (containingType != null && containingType.IsReadOnly)
		{
			return false;
		}
		IMethodSymbol getMethod = property.GetMethod;
		if (getMethod != null && !ShouldMethodDisplayReadOnly(getMethod, property))
		{
			return false;
		}
		IMethodSymbol setMethod = property.SetMethod;
		if (setMethod != null && !ShouldMethodDisplayReadOnly(setMethod, property))
		{
			return false;
		}
		if (getMethod == null)
		{
			return setMethod != null;
		}
		return true;
	}

	private static bool ShouldMethodDisplayReadOnly(IMethodSymbol method, IPropertySymbol? propertyOpt = null)
	{
		INamedTypeSymbol containingType = method.ContainingType;
		if (containingType != null && containingType.IsReadOnly)
		{
			return false;
		}
		if ((method as Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.MethodSymbol)?.UnderlyingMethodSymbol is SourcePropertyAccessorSymbol sourcePropertyAccessorSymbol && (propertyOpt as Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.PropertySymbol)?.UnderlyingSymbol is SourcePropertySymbolBase sourcePropertySymbolBase)
		{
			if (!sourcePropertyAccessorSymbol.LocalDeclaredReadOnly)
			{
				return sourcePropertySymbolBase.HasReadOnlyModifier;
			}
			return true;
		}
		if (method is Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.MethodSymbol methodSymbol)
		{
			return methodSymbol.UnderlyingMethodSymbol.IsDeclaredReadOnly;
		}
		return false;
	}

	public override void VisitProperty(IPropertySymbol symbol)
	{
		AddAccessibilityIfNeeded(symbol);
		AddMemberModifiersIfNeeded(symbol);
		if (ShouldPropertyDisplayReadOnly(symbol))
		{
			AddReadOnlyIfNeeded();
		}
		if (base.Format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeType))
		{
			if (symbol.ReturnsByRef)
			{
				AddRefIfNeeded();
			}
			else if (symbol.ReturnsByRefReadonly)
			{
				AddRefReadonlyIfNeeded();
			}
			AddCustomModifiersIfNeeded(symbol.RefCustomModifiers);
			symbol.Type.Accept(base.NotFirstVisitor);
			AddSpace();
			AddCustomModifiersIfNeeded(symbol.TypeCustomModifiers);
		}
		if (base.Format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeContainingType) && IncludeNamedType(symbol.ContainingType))
		{
			symbol.ContainingType.Accept(base.NotFirstVisitor);
			AddPunctuation(SyntaxKind.DotToken);
		}
		AddPropertyNameAndParameters(symbol);
		if (base.Format.PropertyStyle == SymbolDisplayPropertyStyle.ShowReadWriteDescriptor)
		{
			AddSpace();
			AddPunctuation(SyntaxKind.OpenBraceToken);
			AddAccessor(symbol, symbol.GetMethod, SyntaxKind.GetKeyword);
			SyntaxKind keyword = (IsInitOnly(symbol.SetMethod) ? SyntaxKind.InitKeyword : SyntaxKind.SetKeyword);
			AddAccessor(symbol, symbol.SetMethod, keyword);
			AddSpace();
			AddPunctuation(SyntaxKind.CloseBraceToken);
		}
	}

	private static bool IsInitOnly([NotNullWhen(true)] IMethodSymbol? symbol)
	{
		return symbol?.IsInitOnly ?? false;
	}

	private void AddPropertyNameAndParameters(IPropertySymbol symbol)
	{
		bool flag = symbol.Name.LastIndexOf('.') > 0;
		if (flag)
		{
			AddExplicitInterfaceIfNeeded(symbol.ExplicitInterfaceImplementations);
		}
		if (symbol.IsIndexer)
		{
			AddKeyword(SyntaxKind.ThisKeyword);
		}
		else if (flag)
		{
			base.Builder.Add(CreatePart(SymbolDisplayPartKind.PropertyName, symbol, ExplicitInterfaceHelpers.GetMemberNameWithoutInterfaceName(symbol.Name)));
		}
		else
		{
			base.Builder.Add(CreatePart(SymbolDisplayPartKind.PropertyName, symbol, symbol.Name));
		}
		if (base.Format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeParameters) && symbol.Parameters.Any())
		{
			AddPunctuation(SyntaxKind.OpenBracketToken);
			AddParametersIfNeeded(hasThisParameter: false, isVarargs: false, symbol.Parameters);
			AddPunctuation(SyntaxKind.CloseBracketToken);
		}
	}

	public override void VisitEvent(IEventSymbol symbol)
	{
		AddAccessibilityIfNeeded(symbol);
		AddMemberModifiersIfNeeded(symbol);
		IMethodSymbol methodSymbol = symbol.AddMethod ?? symbol.RemoveMethod;
		if (methodSymbol != null && ShouldMethodDisplayReadOnly(methodSymbol))
		{
			AddReadOnlyIfNeeded();
		}
		if (base.Format.KindOptions.IncludesOption(SymbolDisplayKindOptions.IncludeMemberKeyword))
		{
			AddKeyword(SyntaxKind.EventKeyword);
			AddSpace();
		}
		if (base.Format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeType))
		{
			symbol.Type.Accept(base.NotFirstVisitor);
			AddSpace();
		}
		if (base.Format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeContainingType) && IncludeNamedType(symbol.ContainingType))
		{
			symbol.ContainingType.Accept(base.NotFirstVisitor);
			AddPunctuation(SyntaxKind.DotToken);
		}
		AddEventName(symbol);
	}

	private void AddEventName(IEventSymbol symbol)
	{
		if (symbol.Name.LastIndexOf('.') > 0)
		{
			AddExplicitInterfaceIfNeeded(symbol.ExplicitInterfaceImplementations);
			base.Builder.Add(CreatePart(SymbolDisplayPartKind.EventName, symbol, ExplicitInterfaceHelpers.GetMemberNameWithoutInterfaceName(symbol.Name)));
		}
		else
		{
			base.Builder.Add(CreatePart(SymbolDisplayPartKind.EventName, symbol, symbol.Name));
		}
	}

	public override void VisitMethod(IMethodSymbol symbol)
	{
		if (symbol.MethodKind == MethodKind.AnonymousFunction)
		{
			base.Builder.Add(CreatePart(SymbolDisplayPartKind.NumericLiteral, symbol, "lambda expression"));
			return;
		}
		if (symbol.MethodKind == MethodKind.FunctionPointerSignature)
		{
			visitFunctionPointerSignature(symbol);
			return;
		}
		if (symbol.IsExtensionMethod && base.Format.ExtensionMethodStyle != SymbolDisplayExtensionMethodStyle.Default)
		{
			if (symbol.MethodKind == MethodKind.ReducedExtension && base.Format.ExtensionMethodStyle == SymbolDisplayExtensionMethodStyle.StaticMethod)
			{
				symbol = symbol.GetConstructedReducedFrom();
			}
			else if (symbol.MethodKind != MethodKind.ReducedExtension && base.Format.ExtensionMethodStyle == SymbolDisplayExtensionMethodStyle.InstanceMethod)
			{
				symbol = symbol.ReduceExtensionMethod(symbol.Parameters.First().Type) ?? symbol;
			}
		}
		if (symbol.ContainingType != null || symbol.ContainingSymbol is ITypeSymbol)
		{
			AddAccessibilityIfNeeded(symbol);
			AddMemberModifiersIfNeeded(symbol);
			if (ShouldMethodDisplayReadOnly(symbol))
			{
				AddReadOnlyIfNeeded();
			}
			if (base.Format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeType))
			{
				switch (symbol.MethodKind)
				{
				case MethodKind.Destructor:
					if (!base.Format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.UseMetadataMemberNames))
					{
						break;
					}
					goto default;
				case MethodKind.Conversion:
					if (!base.Format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.UseMetadataMemberNames) && tryGetUserDefinedOperatorTokenKind(symbol.MetadataName) != SyntaxKind.None)
					{
						break;
					}
					goto default;
				default:
					if (symbol.ReturnsByRef)
					{
						AddRefIfNeeded();
					}
					else if (symbol.ReturnsByRefReadonly)
					{
						AddRefReadonlyIfNeeded();
					}
					AddCustomModifiersIfNeeded(symbol.RefCustomModifiers);
					if (symbol.ReturnsVoid)
					{
						AddKeyword(SyntaxKind.VoidKeyword);
					}
					else if (symbol.ReturnType != null)
					{
						AddReturnType(symbol);
					}
					AddSpace();
					AddCustomModifiersIfNeeded(symbol.ReturnTypeCustomModifiers);
					break;
				case MethodKind.Constructor:
				case MethodKind.StaticConstructor:
					break;
				}
			}
			if (base.Format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeContainingType))
			{
				bool flag;
				ITypeSymbol typeSymbol;
				if (symbol.MethodKind == MethodKind.LocalFunction)
				{
					flag = false;
					typeSymbol = null;
				}
				else if (symbol.MethodKind == MethodKind.ReducedExtension)
				{
					typeSymbol = symbol.ReceiverType;
					flag = true;
				}
				else
				{
					typeSymbol = symbol.ContainingType;
					if (typeSymbol != null)
					{
						flag = IncludeNamedType(symbol.ContainingType);
					}
					else
					{
						typeSymbol = (ITypeSymbol)symbol.ContainingSymbol;
						flag = true;
					}
				}
				if (flag)
				{
					typeSymbol.Accept(base.NotFirstVisitor);
					AddPunctuation(SyntaxKind.DotToken);
				}
			}
		}
		bool flag2 = false;
		switch (symbol.MethodKind)
		{
		case MethodKind.DelegateInvoke:
		case MethodKind.Ordinary:
		case MethodKind.LocalFunction:
			base.Builder.Add(CreatePart(SymbolDisplayPartKind.MethodName, symbol, symbol.Name));
			break;
		case MethodKind.ReducedExtension:
			base.Builder.Add(CreatePart(SymbolDisplayPartKind.ExtensionMethodName, symbol, symbol.Name));
			break;
		case MethodKind.PropertyGet:
		case MethodKind.PropertySet:
		{
			flag2 = true;
			IPropertySymbol propertySymbol = (IPropertySymbol)symbol.AssociatedSymbol;
			if (propertySymbol != null)
			{
				AddPropertyNameAndParameters(propertySymbol);
				AddPunctuation(SyntaxKind.DotToken);
				AddKeyword((symbol.MethodKind == MethodKind.PropertyGet) ? SyntaxKind.GetKeyword : (IsInitOnly(symbol) ? SyntaxKind.InitKeyword : SyntaxKind.SetKeyword));
				break;
			}
			goto case MethodKind.DelegateInvoke;
		}
		case MethodKind.EventAdd:
		case MethodKind.EventRemove:
		{
			flag2 = true;
			IEventSymbol eventSymbol = (IEventSymbol)symbol.AssociatedSymbol;
			if (eventSymbol != null)
			{
				AddEventName(eventSymbol);
				AddPunctuation(SyntaxKind.DotToken);
				AddKeyword((symbol.MethodKind == MethodKind.EventAdd) ? SyntaxKind.AddKeyword : SyntaxKind.RemoveKeyword);
				break;
			}
			goto case MethodKind.DelegateInvoke;
		}
		case MethodKind.Constructor:
		case MethodKind.StaticConstructor:
		{
			string text2 = ((base.Format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.UseMetadataMemberNames) || symbol.ContainingType == null || symbol.ContainingType.IsAnonymousType || symbol.ContainingType.IsExtension) ? symbol.Name : symbol.ContainingType.Name);
			SymbolDisplayPartKind partKindForConstructorOrDestructor = GetPartKindForConstructorOrDestructor(symbol);
			base.Builder.Add(CreatePart(partKindForConstructorOrDestructor, symbol, text2));
			break;
		}
		case MethodKind.Destructor:
		{
			SymbolDisplayPartKind partKindForConstructorOrDestructor2 = GetPartKindForConstructorOrDestructor(symbol);
			if (base.Format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.UseMetadataMemberNames) || symbol.ContainingType == null)
			{
				base.Builder.Add(CreatePart(partKindForConstructorOrDestructor2, symbol, symbol.Name));
				break;
			}
			AddPunctuation(SyntaxKind.TildeToken);
			base.Builder.Add(CreatePart(partKindForConstructorOrDestructor2, symbol, symbol.ContainingType.Name));
			break;
		}
		case MethodKind.ExplicitInterfaceImplementation:
			AddExplicitInterfaceIfNeeded(symbol.ExplicitInterfaceImplementations);
			if (!base.Format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.UseMetadataMemberNames) && symbol.GetSymbol()?.OriginalDefinition is SourceUserDefinedOperatorSymbolBase sourceUserDefinedOperatorSymbolBase)
			{
				string text = symbol.MetadataName;
				int num = text.LastIndexOf('.');
				if (num >= 0)
				{
					text = text.Substring(num + 1);
				}
				if (sourceUserDefinedOperatorSymbolBase is SourceUserDefinedConversionSymbol)
				{
					addUserDefinedConversionName(symbol, tryGetUserDefinedConversionTokenKind(text), text);
				}
				else
				{
					addUserDefinedOperatorName(symbol, tryGetUserDefinedOperatorTokenKind(text), text);
				}
			}
			else
			{
				base.Builder.Add(CreatePart(SymbolDisplayPartKind.MethodName, symbol, ExplicitInterfaceHelpers.GetMemberNameWithoutInterfaceName(symbol.Name)));
			}
			break;
		case MethodKind.UserDefinedOperator:
		case MethodKind.BuiltinOperator:
		{
			if (base.Format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.UseMetadataMemberNames))
			{
				base.Builder.Add(CreatePart(SymbolDisplayPartKind.MethodName, symbol, symbol.MetadataName));
				break;
			}
			SyntaxKind syntaxKind2 = tryGetUserDefinedOperatorTokenKind(symbol.MetadataName);
			if (syntaxKind2 == SyntaxKind.None)
			{
				base.Builder.Add(CreatePart(SymbolDisplayPartKind.MethodName, symbol, symbol.Name));
			}
			else
			{
				addUserDefinedOperatorName(symbol, syntaxKind2, symbol.MetadataName);
			}
			break;
		}
		case MethodKind.Conversion:
		{
			if (base.Format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.UseMetadataMemberNames))
			{
				base.Builder.Add(CreatePart(SymbolDisplayPartKind.MethodName, symbol, symbol.MetadataName));
				break;
			}
			SyntaxKind syntaxKind = tryGetUserDefinedConversionTokenKind(symbol.MetadataName);
			if (syntaxKind == SyntaxKind.None)
			{
				base.Builder.Add(CreatePart(SymbolDisplayPartKind.MethodName, symbol, symbol.Name));
			}
			else
			{
				addUserDefinedConversionName(symbol, syntaxKind, symbol.MetadataName);
			}
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.MethodKind);
		}
		if (!flag2)
		{
			AddTypeArguments(symbol, default(ImmutableArray<ImmutableArray<CustomModifier>>));
			AddParameters(symbol);
			AddTypeParameterConstraints(symbol);
		}
		void addUserDefinedConversionName(IMethodSymbol symbol2, SyntaxKind conversionKind, string operatorName)
		{
			AddKeyword(conversionKind);
			AddSpace();
			AddKeyword(SyntaxKind.OperatorKeyword);
			AddSpace();
			if (operatorName == "op_CheckedExplicit")
			{
				AddKeyword(SyntaxKind.CheckedKeyword);
				AddSpace();
			}
			AddReturnType(symbol2);
		}
		void addUserDefinedOperatorName(IMethodSymbol symbol2, SyntaxKind operatorKind, string operatorName)
		{
			AddKeyword(SyntaxKind.OperatorKeyword);
			AddSpace();
			switch (operatorKind)
			{
			case SyntaxKind.TrueKeyword:
				AddKeyword(SyntaxKind.TrueKeyword);
				break;
			case SyntaxKind.FalseKeyword:
				AddKeyword(SyntaxKind.FalseKeyword);
				break;
			default:
				if (SyntaxFacts.IsCheckedOperator(operatorName))
				{
					AddKeyword(SyntaxKind.CheckedKeyword);
					AddSpace();
				}
				base.Builder.Add(CreatePart(SymbolDisplayPartKind.Operator, symbol2, SyntaxFacts.GetText(operatorKind)));
				break;
			}
		}
		static SyntaxKind tryGetUserDefinedConversionTokenKind(string operatorName)
		{
			if ((operatorName == "op_Explicit" || operatorName == "op_CheckedExplicit") ? true : false)
			{
				return SyntaxKind.ExplicitKeyword;
			}
			if (operatorName == "op_Implicit")
			{
				return SyntaxKind.ImplicitKeyword;
			}
			return SyntaxKind.None;
		}
		static SyntaxKind tryGetUserDefinedOperatorTokenKind(string operatorName)
		{
			if (operatorName == "op_True")
			{
				return SyntaxKind.TrueKeyword;
			}
			if (operatorName == "op_False")
			{
				return SyntaxKind.FalseKeyword;
			}
			return SyntaxFacts.GetOperatorKind(operatorName);
		}
		void visitFunctionPointerSignature(IMethodSymbol methodSymbol)
		{
			AddKeyword(SyntaxKind.DelegateKeyword);
			AddPunctuation(SyntaxKind.AsteriskToken);
			if (methodSymbol.CallingConvention != SignatureCallingConvention.Default)
			{
				AddSpace();
				AddKeyword(SyntaxKind.UnmanagedKeyword);
				ImmutableArray<INamedTypeSymbol> unmanagedCallingConventionTypes = methodSymbol.UnmanagedCallingConventionTypes;
				if (methodSymbol.CallingConvention != SignatureCallingConvention.Unmanaged || !unmanagedCallingConventionTypes.IsEmpty)
				{
					AddPunctuation(SyntaxKind.OpenBracketToken);
					switch (methodSymbol.CallingConvention)
					{
					case SignatureCallingConvention.CDecl:
						base.Builder.Add(CreatePart(SymbolDisplayPartKind.ClassName, methodSymbol, "Cdecl"));
						break;
					case SignatureCallingConvention.StdCall:
						base.Builder.Add(CreatePart(SymbolDisplayPartKind.ClassName, methodSymbol, "Stdcall"));
						break;
					case SignatureCallingConvention.ThisCall:
						base.Builder.Add(CreatePart(SymbolDisplayPartKind.ClassName, methodSymbol, "Thiscall"));
						break;
					case SignatureCallingConvention.FastCall:
						base.Builder.Add(CreatePart(SymbolDisplayPartKind.ClassName, methodSymbol, "Fastcall"));
						break;
					case SignatureCallingConvention.Unmanaged:
					{
						bool flag3 = true;
						foreach (INamedTypeSymbol item in unmanagedCallingConventionTypes)
						{
							if (!flag3)
							{
								AddPunctuation(SyntaxKind.CommaToken);
								AddSpace();
							}
							flag3 = false;
							ArrayBuilder<SymbolDisplayPart> builder = base.Builder;
							string name = item.Name;
							builder.Add(CreatePart(SymbolDisplayPartKind.ClassName, item, name.Substring(8, name.Length - 8)));
						}
						break;
					}
					}
					AddPunctuation(SyntaxKind.CloseBracketToken);
				}
			}
			AddPunctuation(SyntaxKind.LessThanToken);
			foreach (IParameterSymbol parameter in methodSymbol.Parameters)
			{
				AddParameterRefKind(parameter.RefKind);
				AddCustomModifiersIfNeeded(parameter.RefCustomModifiers);
				parameter.Type.Accept(base.NotFirstVisitor);
				AddCustomModifiersIfNeeded(parameter.CustomModifiers, leadingSpace: true, trailingSpace: false);
				AddPunctuation(SyntaxKind.CommaToken);
				AddSpace();
			}
			if (methodSymbol.ReturnsByRef)
			{
				AddRef();
			}
			else if (methodSymbol.ReturnsByRefReadonly)
			{
				AddRefReadonly();
			}
			AddCustomModifiersIfNeeded(methodSymbol.RefCustomModifiers);
			methodSymbol.ReturnType.Accept(base.NotFirstVisitor);
			AddCustomModifiersIfNeeded(methodSymbol.ReturnTypeCustomModifiers, leadingSpace: true, trailingSpace: false);
			AddPunctuation(SyntaxKind.GreaterThanToken);
		}
	}

	private static SymbolDisplayPartKind GetPartKindForConstructorOrDestructor(IMethodSymbol symbol)
	{
		if (symbol.ContainingType == null || symbol.ContainingType.IsExtension)
		{
			return SymbolDisplayPartKind.MethodName;
		}
		return GetPartKind(symbol.ContainingType);
	}

	private void AddReturnType(IMethodSymbol symbol)
	{
		symbol.ReturnType.Accept(base.NotFirstVisitor);
	}

	private void AddTypeParameterConstraints(IMethodSymbol symbol)
	{
		if (base.Format.GenericsOptions.IncludesOption(SymbolDisplayGenericsOptions.IncludeTypeConstraints))
		{
			AddTypeParameterConstraints(symbol.TypeArguments);
		}
	}

	private void AddParameters(IMethodSymbol symbol)
	{
		if (base.Format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeParameters))
		{
			AddPunctuation(SyntaxKind.OpenParenToken);
			AddParametersIfNeeded(symbol.IsExtensionMethod && symbol.MethodKind != MethodKind.ReducedExtension, symbol.IsVararg, symbol.Parameters);
			AddPunctuation(SyntaxKind.CloseParenToken);
		}
	}

	public override void VisitParameter(IParameterSymbol symbol)
	{
		bool flag = base.Format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeType);
		bool flag2 = symbol.Name.Length != 0 && (base.Format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeName) || (!base.Format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.ExcludeParameterNameIfStandalone) && base.Builder.Count == 0));
		bool num = base.Format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeOptionalBrackets);
		bool flag3 = base.Format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeDefaultValue) && base.Format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeName) && symbol.HasExplicitDefaultValue && CanAddConstant(symbol.Type, symbol.ExplicitDefaultValue);
		if (num && symbol.IsOptional)
		{
			AddPunctuation(SyntaxKind.OpenBracketToken);
		}
		if (flag)
		{
			AddParameterModifiersAndType(symbol);
		}
		if (flag2)
		{
			if (flag)
			{
				AddSpace();
			}
			SymbolDisplayPartKind kind = (symbol.IsThis ? SymbolDisplayPartKind.Keyword : SymbolDisplayPartKind.ParameterName);
			base.Builder.Add(CreatePart(kind, symbol, symbol.Name));
		}
		if (flag3)
		{
			if (flag2 | flag)
			{
				AddSpace();
			}
			AddPunctuation(SyntaxKind.EqualsToken);
			AddSpace();
			AddConstantValue(symbol.Type, symbol.ExplicitDefaultValue);
		}
		if (num && symbol.IsOptional)
		{
			AddPunctuation(SyntaxKind.CloseBracketToken);
		}
	}

	private void AddParameterModifiersAndType(IParameterSymbol symbol)
	{
		if (base.Format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeParamsRefOut))
		{
			if (symbol.ScopedKind == ScopedKind.ScopedRef && symbol.RefKind != RefKind.Out && !symbol.IsThis)
			{
				AddKeyword(SyntaxKind.ScopedKeyword);
				AddSpace();
			}
			AddParameterRefKind(symbol.RefKind);
		}
		AddCustomModifiersIfNeeded(symbol.RefCustomModifiers);
		if (base.Format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeParamsRefOut))
		{
			if (symbol.IsParams)
			{
				AddKeyword(SyntaxKind.ParamsKeyword);
				AddSpace();
			}
			bool flag = symbol.ScopedKind == ScopedKind.ScopedValue && symbol.RefKind == RefKind.None;
			if (flag)
			{
				bool flag2 = symbol.IsParams;
				if (flag2)
				{
					ITypeSymbol type = symbol.Type;
					bool flag3 = ((type != null && (type.IsRefLikeType || type is ITypeParameterSymbol { AllowsRefLikeType: not false })) ? true : false);
					flag2 = flag3;
				}
				flag = !flag2;
			}
			if (flag)
			{
				AddKeyword(SyntaxKind.ScopedKeyword);
				AddSpace();
			}
		}
		symbol.Type.Accept(base.NotFirstVisitor);
		AddCustomModifiersIfNeeded(symbol.CustomModifiers, leadingSpace: true, trailingSpace: false);
	}

	private static bool CanAddConstant(ITypeSymbol type, object? value)
	{
		if (type.TypeKind == TypeKind.Enum)
		{
			return true;
		}
		if (value == null)
		{
			return true;
		}
		if (!value.GetType().GetTypeInfo().IsPrimitive && !(value is string))
		{
			return value is decimal;
		}
		return true;
	}

	private void AddFieldModifiersIfNeeded(IFieldSymbol symbol)
	{
		if (base.Format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeModifiers) && !IsEnumMember(symbol))
		{
			if (symbol.IsConst)
			{
				AddKeyword(SyntaxKind.ConstKeyword);
				AddSpace();
			}
			if (symbol.IsReadOnly)
			{
				AddKeyword(SyntaxKind.ReadOnlyKeyword);
				AddSpace();
			}
			if (symbol.IsVolatile)
			{
				AddKeyword(SyntaxKind.VolatileKeyword);
				AddSpace();
			}
		}
	}

	private void AddMemberModifiersIfNeeded(ISymbol symbol)
	{
		INamedTypeSymbol containingType = symbol.ContainingType;
		if (base.Format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeModifiers) && (containingType == null || (containingType.TypeKind != TypeKind.Interface && !IsEnumMember(symbol) && !IsLocalFunction(symbol))))
		{
			bool flag = symbol is IFieldSymbol fieldSymbol && fieldSymbol.IsConst;
			bool flag2 = ((symbol is IFieldSymbol { IsRequired: not false } || symbol is IPropertySymbol { IsRequired: not false }) ? true : false);
			if (symbol.IsStatic && !flag)
			{
				AddKeyword(SyntaxKind.StaticKeyword);
				AddSpace();
			}
			if (symbol.IsOverride)
			{
				AddKeyword(SyntaxKind.OverrideKeyword);
				AddSpace();
			}
			if (symbol.IsAbstract)
			{
				AddKeyword(SyntaxKind.AbstractKeyword);
				AddSpace();
			}
			if (symbol.IsSealed)
			{
				AddKeyword(SyntaxKind.SealedKeyword);
				AddSpace();
			}
			if (symbol.IsExtern)
			{
				AddKeyword(SyntaxKind.ExternKeyword);
				AddSpace();
			}
			if (symbol.IsVirtual)
			{
				AddKeyword(SyntaxKind.VirtualKeyword);
				AddSpace();
			}
			if (flag2)
			{
				AddKeyword(SyntaxKind.RequiredKeyword);
				AddSpace();
			}
		}
	}

	private void AddParametersIfNeeded(bool hasThisParameter, bool isVarargs, ImmutableArray<IParameterSymbol> parameters)
	{
		if (base.Format.ParameterOptions == SymbolDisplayParameterOptions.None)
		{
			return;
		}
		bool flag = true;
		if (!parameters.IsDefault)
		{
			foreach (IParameterSymbol item in parameters)
			{
				if (!flag)
				{
					AddPunctuation(SyntaxKind.CommaToken);
					AddSpace();
				}
				else if (hasThisParameter && base.Format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeExtensionThis))
				{
					AddKeyword(SyntaxKind.ThisKeyword);
					AddSpace();
				}
				flag = false;
				item.Accept(base.NotFirstVisitor);
			}
		}
		if (isVarargs)
		{
			if (!flag)
			{
				AddPunctuation(SyntaxKind.CommaToken);
				AddSpace();
			}
			AddKeyword(SyntaxKind.ArgListKeyword);
		}
	}

	private void AddAccessor(IPropertySymbol property, IMethodSymbol? method, SyntaxKind keyword)
	{
		if (method != null)
		{
			AddSpace();
			if (method.DeclaredAccessibility != property.DeclaredAccessibility)
			{
				AddAccessibility(method);
			}
			if (!ShouldPropertyDisplayReadOnly(property) && ShouldMethodDisplayReadOnly(method, property))
			{
				AddReadOnlyIfNeeded();
			}
			AddKeyword(keyword);
			AddPunctuation(SyntaxKind.SemicolonToken);
		}
	}

	private void AddExplicitInterfaceIfNeeded<T>(ImmutableArray<T> implementedMembers) where T : ISymbol
	{
		if (base.Format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeExplicitInterface) && !implementedMembers.IsEmpty)
		{
			INamedTypeSymbol containingType = implementedMembers[0].ContainingType;
			if (containingType != null)
			{
				containingType.Accept(base.NotFirstVisitor);
				AddPunctuation(SyntaxKind.DotToken);
			}
		}
	}

	private void AddCustomModifiersIfNeeded(ImmutableArray<CustomModifier> customModifiers, bool leadingSpace = false, bool trailingSpace = true)
	{
		if (!base.Format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.IncludeCustomModifiers) || customModifiers.IsEmpty)
		{
			return;
		}
		bool flag = true;
		foreach (CustomModifier item in customModifiers)
		{
			if (!flag | leadingSpace)
			{
				AddSpace();
			}
			flag = false;
			base.Builder.Add(CreatePart((SymbolDisplayPartKind)34, null, item.IsOptional ? "modopt" : "modreq"));
			AddPunctuation(SyntaxKind.OpenParenToken);
			item.Modifier.Accept(base.NotFirstVisitor);
			AddPunctuation(SyntaxKind.CloseParenToken);
		}
		if (trailingSpace)
		{
			AddSpace();
		}
	}

	private void AddRefIfNeeded()
	{
		if (base.Format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeRef))
		{
			AddRef();
		}
	}

	private void AddRef()
	{
		AddKeyword(SyntaxKind.RefKeyword);
		AddSpace();
	}

	private void AddRefReadonlyIfNeeded()
	{
		if (base.Format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeRef))
		{
			AddRefReadonly();
		}
	}

	private void AddRefReadonly()
	{
		AddKeyword(SyntaxKind.RefKeyword);
		AddSpace();
		AddKeyword(SyntaxKind.ReadOnlyKeyword);
		AddSpace();
	}

	private void AddReadOnlyIfNeeded()
	{
		if (base.Format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeRef))
		{
			AddKeyword(SyntaxKind.ReadOnlyKeyword);
			AddSpace();
		}
	}

	private void AddParameterRefKind(RefKind refKind)
	{
		switch (refKind)
		{
		case RefKind.Out:
			AddKeyword(SyntaxKind.OutKeyword);
			AddSpace();
			break;
		case RefKind.Ref:
			AddKeyword(SyntaxKind.RefKeyword);
			AddSpace();
			break;
		case RefKind.In:
			AddKeyword(SyntaxKind.InKeyword);
			AddSpace();
			break;
		case RefKind.RefReadOnlyParameter:
			AddKeyword(SyntaxKind.RefKeyword);
			AddSpace();
			AddKeyword(SyntaxKind.ReadOnlyKeyword);
			AddSpace();
			break;
		}
	}

	public override void VisitArrayType(IArrayTypeSymbol symbol)
	{
		VisitArrayTypeWithoutNullability(symbol);
		AddNullableAnnotations(symbol);
	}

	private void VisitArrayTypeWithoutNullability(IArrayTypeSymbol symbol)
	{
		if (TryAddAlias(symbol, base.Builder))
		{
			return;
		}
		if (base.Format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.ReverseArrayRankSpecifiers))
		{
			symbol.ElementType.Accept(this);
			AddArrayRank(symbol);
			return;
		}
		ITypeSymbol typeSymbol = symbol;
		do
		{
			typeSymbol = ((IArrayTypeSymbol)typeSymbol).ElementType;
		}
		while (typeSymbol.Kind == SymbolKind.ArrayType && !ShouldAddNullableAnnotation(typeSymbol));
		typeSymbol.Accept(base.NotFirstVisitor);
		IArrayTypeSymbol arrayTypeSymbol = symbol;
		while (arrayTypeSymbol != null && arrayTypeSymbol != typeSymbol)
		{
			if (!base.IsFirstSymbolVisited)
			{
				AddCustomModifiersIfNeeded(arrayTypeSymbol.CustomModifiers, leadingSpace: true);
			}
			AddArrayRank(arrayTypeSymbol);
			arrayTypeSymbol = arrayTypeSymbol.ElementType as IArrayTypeSymbol;
		}
	}

	private void AddNullableAnnotations(ITypeSymbol type)
	{
		if (ShouldAddNullableAnnotation(type))
		{
			AddPunctuation((type.NullableAnnotation == Microsoft.CodeAnalysis.NullableAnnotation.Annotated) ? SyntaxKind.QuestionToken : SyntaxKind.ExclamationToken);
		}
	}

	private bool ShouldAddNullableAnnotation(ITypeSymbol type)
	{
		switch (type.NullableAnnotation)
		{
		case Microsoft.CodeAnalysis.NullableAnnotation.Annotated:
			if (base.Format.MiscellaneousOptions.IncludesOption(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier) && !ITypeSymbolHelpers.IsNullableType(type) && !type.IsValueType)
			{
				return true;
			}
			break;
		case Microsoft.CodeAnalysis.NullableAnnotation.NotAnnotated:
			if (base.Format.MiscellaneousOptions.IncludesOption(SymbolDisplayMiscellaneousOptions.IncludeNotNullableReferenceTypeModifier) && !type.IsValueType)
			{
				Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.TypeSymbol obj = type as Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.TypeSymbol;
				if (obj == null || !obj.UnderlyingTypeSymbol.IsTypeParameterDisallowingAnnotationInCSharp8())
				{
					return true;
				}
			}
			break;
		}
		return false;
	}

	private void AddArrayRank(IArrayTypeSymbol symbol)
	{
		bool flag = base.Format.MiscellaneousOptions.IncludesOption(SymbolDisplayMiscellaneousOptions.UseAsterisksInMultiDimensionalArrays);
		AddPunctuation(SyntaxKind.OpenBracketToken);
		if (symbol.Rank > 1)
		{
			if (flag)
			{
				AddPunctuation(SyntaxKind.AsteriskToken);
			}
		}
		else if (!symbol.IsSZArray)
		{
			AddPunctuation(SyntaxKind.AsteriskToken);
		}
		for (int i = 0; i < symbol.Rank - 1; i++)
		{
			AddPunctuation(SyntaxKind.CommaToken);
			if (flag)
			{
				AddPunctuation(SyntaxKind.AsteriskToken);
			}
		}
		AddPunctuation(SyntaxKind.CloseBracketToken);
	}

	public override void VisitPointerType(IPointerTypeSymbol symbol)
	{
		symbol.PointedAtType.Accept(base.NotFirstVisitor);
		AddNullableAnnotations(symbol);
		if (!base.IsFirstSymbolVisited)
		{
			AddCustomModifiersIfNeeded(symbol.CustomModifiers, leadingSpace: true);
		}
		AddPunctuation(SyntaxKind.AsteriskToken);
	}

	public override void VisitFunctionPointerType(IFunctionPointerTypeSymbol symbol)
	{
		VisitMethod(symbol.Signature);
	}

	public override void VisitTypeParameter(ITypeParameterSymbol symbol)
	{
		if (base.IsFirstSymbolVisited)
		{
			AddTypeParameterVarianceIfNeeded(symbol);
		}
		base.Builder.Add(CreatePart(SymbolDisplayPartKind.TypeParameterName, symbol, symbol.Name));
		AddNullableAnnotations(symbol);
	}

	public override void VisitDynamicType(IDynamicTypeSymbol symbol)
	{
		base.Builder.Add(CreatePart(SymbolDisplayPartKind.Keyword, symbol, symbol.Name));
		AddNullableAnnotations(symbol);
	}

	public override void VisitNamedType(INamedTypeSymbol symbol)
	{
		if ((base.Format.CompilerInternalOptions & SymbolDisplayCompilerInternalOptions.IncludeFileLocalTypesPrefix) != SymbolDisplayCompilerInternalOptions.None && symbol is Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.Symbol { UnderlyingSymbol: Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol underlyingSymbol })
		{
			string fileLocalTypeMetadataNamePrefix = underlyingSymbol.GetFileLocalTypeMetadataNamePrefix();
			if (fileLocalTypeMetadataNamePrefix != null)
			{
				base.Builder.Add(CreatePart(SymbolDisplayPartKind.ModuleName, symbol, fileLocalTypeMetadataNamePrefix));
			}
		}
		VisitNamedTypeWithoutNullability(symbol);
		AddNullableAnnotations(symbol);
		if ((base.Format.CompilerInternalOptions & SymbolDisplayCompilerInternalOptions.IncludeContainingFileForFileTypes) == 0 || !(symbol is Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.Symbol { UnderlyingSymbol: Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol underlyingSymbol2 }))
		{
			return;
		}
		FileIdentifier associatedFileIdentifier = underlyingSymbol2.AssociatedFileIdentifier;
		if (associatedFileIdentifier != null)
		{
			string displayFilePath = associatedFileIdentifier.DisplayFilePath;
			object obj;
			if (displayFilePath == null || displayFilePath.Length == 0)
			{
				SyntaxTree sourceTree = underlyingSymbol2.GetFirstLocationOrNone().SourceTree;
				obj = ((sourceTree != null) ? $"<tree {underlyingSymbol2.DeclaringCompilation.GetSyntaxTreeOrdinal(sourceTree)}>" : "<unknown>");
			}
			else
			{
				obj = displayFilePath;
			}
			string text = (string)obj;
			base.Builder.Add(CreatePart(SymbolDisplayPartKind.Punctuation, symbol, "@"));
			base.Builder.Add(CreatePart(SymbolDisplayPartKind.ModuleName, symbol, text));
		}
	}

	private void VisitNamedTypeWithoutNullability(INamedTypeSymbol symbol)
	{
		if (base.IsMinimizing && TryAddAlias(symbol, base.Builder))
		{
			return;
		}
		if (symbol.IsNativeIntegerType)
		{
			if (!base.Format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.UseNativeIntegerUnderlyingType) && AddSpecialTypeKeyword(symbol))
			{
				return;
			}
		}
		else if (base.Format.MiscellaneousOptions.IncludesOption(SymbolDisplayMiscellaneousOptions.UseSpecialTypes) && AddSpecialTypeKeyword(symbol))
		{
			return;
		}
		if (!base.Format.MiscellaneousOptions.IncludesOption(SymbolDisplayMiscellaneousOptions.ExpandNullable) && ITypeSymbolHelpers.IsNullableType(symbol) && !symbol.IsDefinition)
		{
			ITypeSymbol typeSymbol = symbol.TypeArguments[0];
			if (typeSymbol.TypeKind != TypeKind.Pointer)
			{
				typeSymbol.Accept(base.NotFirstVisitor);
				AddCustomModifiersIfNeeded(symbol.GetTypeArgumentCustomModifiers(0), leadingSpace: true, trailingSpace: false);
				AddPunctuation(SyntaxKind.QuestionToken);
				return;
			}
		}
		if (base.IsMinimizing || (symbol.IsTupleType && !ShouldDisplayAsValueTuple(symbol)))
		{
			MinimallyQualify(symbol);
			return;
		}
		AddTypeKind(symbol);
		if (CanShowDelegateSignature(symbol) && base.Format.DelegateStyle == SymbolDisplayDelegateStyle.NameAndSignature)
		{
			IMethodSymbol delegateInvokeMethod = symbol.DelegateInvokeMethod;
			if (delegateInvokeMethod.ReturnsByRef)
			{
				AddRefIfNeeded();
			}
			else if (delegateInvokeMethod.ReturnsByRefReadonly)
			{
				AddRefReadonlyIfNeeded();
			}
			if (delegateInvokeMethod.ReturnsVoid)
			{
				AddKeyword(SyntaxKind.VoidKeyword);
			}
			else
			{
				AddReturnType(symbol.DelegateInvokeMethod);
			}
			AddSpace();
		}
		ISymbol containingSymbol = symbol.ContainingSymbol;
		if (ShouldVisitNamespace(containingSymbol))
		{
			INamespaceSymbol namespaceSymbol = (INamespaceSymbol)containingSymbol;
			if (!namespaceSymbol.IsGlobalNamespace || symbol.TypeKind != TypeKind.Error)
			{
				namespaceSymbol.Accept(base.NotFirstVisitor);
				AddPunctuation(namespaceSymbol.IsGlobalNamespace ? SyntaxKind.ColonColonToken : SyntaxKind.DotToken);
			}
		}
		if ((base.Format.TypeQualificationStyle == SymbolDisplayTypeQualificationStyle.NameAndContainingTypes || base.Format.TypeQualificationStyle == SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces || symbol.IsExtension) && IncludeNamedType(symbol.ContainingType))
		{
			symbol.ContainingType.Accept(base.NotFirstVisitor);
			AddNestedTypeSeparator();
		}
		AddNameAndTypeArgumentsOrParameters(symbol);
	}

	private void AddNestedTypeSeparator()
	{
		AddPunctuation(base.Format.CompilerInternalOptions.HasFlag(SymbolDisplayCompilerInternalOptions.UsePlusForNestedTypes) ? SyntaxKind.PlusToken : SyntaxKind.DotToken);
	}

	private bool ShouldDisplayAsValueTuple(INamedTypeSymbol symbol)
	{
		if (base.Format.MiscellaneousOptions.IncludesOption(SymbolDisplayMiscellaneousOptions.ExpandValueTuple))
		{
			return true;
		}
		return !CanUseTupleSyntax(symbol);
	}

	private void AddNameAndTypeArgumentsOrParameters(INamedTypeSymbol symbol)
	{
		if (symbol.IsAnonymousType && symbol.TypeKind != TypeKind.Delegate)
		{
			AddAnonymousTypeName(symbol);
			return;
		}
		if (symbol.IsTupleType && !ShouldDisplayAsValueTuple(symbol))
		{
			AddTupleTypeName(symbol);
			return;
		}
		Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol namedTypeSymbol = (symbol as Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.NamedTypeSymbol)?.UnderlyingNamedTypeSymbol;
		if (symbol.IsExtension)
		{
			if (base.Format.CompilerInternalOptions.HasFlag(SymbolDisplayCompilerInternalOptions.UseMetadataMemberNames))
			{
				base.Builder.Add(CreatePart(SymbolDisplayPartKind.ClassName, symbol, symbol.ExtensionGroupingName));
			}
			else
			{
				AddKeyword(SyntaxKind.ExtensionKeyword);
			}
		}
		else
		{
			string text = null;
			if (namedTypeSymbol is NoPiaIllegalGenericInstantiationSymbol noPiaIllegalGenericInstantiationSymbol)
			{
				symbol = noPiaIllegalGenericInstantiationSymbol.UnderlyingSymbol.GetPublicSymbol();
			}
			else if (namedTypeSymbol is NoPiaAmbiguousCanonicalTypeSymbol noPiaAmbiguousCanonicalTypeSymbol)
			{
				symbol = noPiaAmbiguousCanonicalTypeSymbol.FirstCandidate.GetPublicSymbol();
			}
			else if (namedTypeSymbol is NoPiaMissingCanonicalTypeSymbol noPiaMissingCanonicalTypeSymbol)
			{
				text = noPiaMissingCanonicalTypeSymbol.FullTypeName;
			}
			if (text == null && symbol.IsAnonymousType && symbol.TypeKind == TypeKind.Delegate)
			{
				text = "<anonymous delegate>";
			}
			SymbolDisplayPartKind partKind = GetPartKind(symbol);
			if (text == null)
			{
				text = symbol.Name;
			}
			if (base.Format.MiscellaneousOptions.IncludesOption(SymbolDisplayMiscellaneousOptions.UseErrorTypeSymbolName) && partKind == SymbolDisplayPartKind.ErrorTypeName && string.IsNullOrEmpty(text))
			{
				base.Builder.Add(CreatePart(partKind, symbol, "?"));
			}
			else
			{
				text = RemoveAttributeSuffixIfNecessary(symbol, text);
				base.Builder.Add(CreatePart(partKind, symbol, text));
			}
		}
		if (base.Format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.UseArityForGenericTypes))
		{
			if (symbol.Arity > 0)
			{
				string aritySuffix = MetadataHelpers.GetAritySuffix(symbol.Arity);
				if (namedTypeSymbol?.MangleName ?? (symbol.MetadataName == symbol.Name + aritySuffix))
				{
					base.Builder.Add(CreatePart((SymbolDisplayPartKind)33, null, aritySuffix));
				}
			}
		}
		else if (symbol.Arity > 0 && base.Format.GenericsOptions.IncludesOption(SymbolDisplayGenericsOptions.IncludeTypeParameters))
		{
			if (namedTypeSymbol is UnsupportedMetadataTypeSymbol || namedTypeSymbol is MissingMetadataTypeSymbol || symbol.IsUnboundGenericType)
			{
				AddPunctuation(SyntaxKind.LessThanToken);
				for (int i = 0; i < symbol.Arity - 1; i++)
				{
					AddPunctuation(SyntaxKind.CommaToken);
				}
				AddPunctuation(SyntaxKind.GreaterThanToken);
			}
			else
			{
				AddTypeArguments(symbol, GetTypeArgumentsModifiers(namedTypeSymbol));
				AddDelegateParameters(symbol);
				if (symbol.IsExtension)
				{
					addExtensionParameter(symbol);
				}
				AddTypeParameterConstraints(symbol.TypeArguments);
			}
		}
		else if (symbol.IsExtension)
		{
			addExtensionParameter(symbol);
		}
		else
		{
			AddDelegateParameters(symbol);
		}
		if (namedTypeSymbol?.OriginalDefinition is MissingMetadataTypeSymbol && base.Format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.FlagMissingMetadataTypes))
		{
			AddPunctuation(SyntaxKind.OpenBracketToken);
			base.Builder.Add(CreatePart((SymbolDisplayPartKind)34, symbol, "missing"));
			AddPunctuation(SyntaxKind.CloseBracketToken);
		}
		void addExtensionParameter(INamedTypeSymbol namedTypeSymbol2)
		{
			if (!base.Format.CompilerInternalOptions.HasFlag(SymbolDisplayCompilerInternalOptions.UseMetadataMemberNames))
			{
				IParameterSymbol extensionParameter = namedTypeSymbol2.ExtensionParameter;
				if (extensionParameter != null)
				{
					AddPunctuation(SyntaxKind.OpenParenToken);
					AddParameterModifiersAndType(extensionParameter);
					AddPunctuation(SyntaxKind.CloseParenToken);
				}
			}
		}
	}

	private ImmutableArray<ImmutableArray<CustomModifier>> GetTypeArgumentsModifiers(Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol? underlyingTypeSymbol)
	{
		if (base.Format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.IncludeCustomModifiers) && (object)underlyingTypeSymbol != null)
		{
			return underlyingTypeSymbol.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics.SelectAsArray((TypeWithAnnotations a) => a.CustomModifiers);
		}
		return default(ImmutableArray<ImmutableArray<CustomModifier>>);
	}

	private void AddDelegateParameters(INamedTypeSymbol symbol)
	{
		if (CanShowDelegateSignature(symbol) && (base.Format.DelegateStyle == SymbolDisplayDelegateStyle.NameAndParameters || base.Format.DelegateStyle == SymbolDisplayDelegateStyle.NameAndSignature))
		{
			IMethodSymbol delegateInvokeMethod = symbol.DelegateInvokeMethod;
			AddPunctuation(SyntaxKind.OpenParenToken);
			AddParametersIfNeeded(hasThisParameter: false, delegateInvokeMethod.IsVararg, delegateInvokeMethod.Parameters);
			AddPunctuation(SyntaxKind.CloseParenToken);
		}
	}

	private void AddAnonymousTypeName(INamedTypeSymbol symbol)
	{
		string text = string.Join(", ", symbol.GetMembers().OfType<IPropertySymbol>().Select(CreateAnonymousTypeMember));
		if (text.Length == 0)
		{
			base.Builder.Add(new SymbolDisplayPart(SymbolDisplayPartKind.ClassName, symbol, "<empty anonymous type>"));
			return;
		}
		string text2 = "<anonymous type: " + text + ">";
		base.Builder.Add(new SymbolDisplayPart(SymbolDisplayPartKind.ClassName, symbol, text2));
	}

	private bool CanUseTupleSyntax(INamedTypeSymbol tupleSymbol)
	{
		if (containsModopt(tupleSymbol))
		{
			return false;
		}
		INamedTypeSymbol tupleUnderlyingTypeOrSelf = GetTupleUnderlyingTypeOrSelf(tupleSymbol);
		if (tupleUnderlyingTypeOrSelf.Arity <= 1)
		{
			return false;
		}
		while (tupleUnderlyingTypeOrSelf.Arity == 8)
		{
			tupleSymbol = (INamedTypeSymbol)tupleUnderlyingTypeOrSelf.TypeArguments[7];
			if (tupleSymbol.TypeKind == TypeKind.Error || HasNonDefaultTupleElements(tupleSymbol) || containsModopt(tupleSymbol))
			{
				return false;
			}
			tupleUnderlyingTypeOrSelf = GetTupleUnderlyingTypeOrSelf(tupleSymbol);
		}
		return true;
		bool containsModopt(INamedTypeSymbol symbol)
		{
			Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol underlyingTypeSymbol = (symbol as Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.NamedTypeSymbol)?.UnderlyingNamedTypeSymbol;
			ImmutableArray<ImmutableArray<CustomModifier>> typeArgumentsModifiers = GetTypeArgumentsModifiers(underlyingTypeSymbol);
			if (typeArgumentsModifiers.IsDefault)
			{
				return false;
			}
			return typeArgumentsModifiers.Any((ImmutableArray<CustomModifier> m) => !m.IsEmpty);
		}
	}

	private static INamedTypeSymbol GetTupleUnderlyingTypeOrSelf(INamedTypeSymbol type)
	{
		return type.TupleUnderlyingType ?? type;
	}

	private static bool HasNonDefaultTupleElements(INamedTypeSymbol tupleSymbol)
	{
		return tupleSymbol.TupleElements.Any((IFieldSymbol e) => !e.IsDefaultTupleElement());
	}

	private void AddTupleTypeName(INamedTypeSymbol symbol)
	{
		if (base.Format.MiscellaneousOptions.IncludesOption(SymbolDisplayMiscellaneousOptions.CollapseTupleTypes))
		{
			base.Builder.Add(CreatePart(SymbolDisplayPartKind.StructName, symbol, "<tuple>"));
			return;
		}
		ImmutableArray<IFieldSymbol> tupleElements = symbol.TupleElements;
		AddPunctuation(SyntaxKind.OpenParenToken);
		for (int i = 0; i < tupleElements.Length; i++)
		{
			IFieldSymbol fieldSymbol = tupleElements[i];
			if (i != 0)
			{
				AddPunctuation(SyntaxKind.CommaToken);
				AddSpace();
			}
			VisitFieldType(fieldSymbol);
			if (fieldSymbol.IsExplicitlyNamedTupleElement)
			{
				AddSpace();
				base.Builder.Add(CreatePart(SymbolDisplayPartKind.FieldName, fieldSymbol, fieldSymbol.Name));
			}
		}
		AddPunctuation(SyntaxKind.CloseParenToken);
		if (symbol.TypeKind == TypeKind.Error && base.Format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.FlagMissingMetadataTypes))
		{
			AddPunctuation(SyntaxKind.OpenBracketToken);
			base.Builder.Add(CreatePart((SymbolDisplayPartKind)34, symbol, "missing"));
			AddPunctuation(SyntaxKind.CloseBracketToken);
		}
	}

	private string CreateAnonymousTypeMember(IPropertySymbol property)
	{
		return property.Type.ToDisplayString(base.Format) + " " + property.Name;
	}

	private bool CanShowDelegateSignature(INamedTypeSymbol symbol)
	{
		if (base.IsFirstSymbolVisited && symbol.TypeKind == TypeKind.Delegate && base.Format.DelegateStyle != SymbolDisplayDelegateStyle.NameOnly)
		{
			return symbol.DelegateInvokeMethod != null;
		}
		return false;
	}

	private static SymbolDisplayPartKind GetPartKind(INamedTypeSymbol symbol)
	{
		switch (symbol.TypeKind)
		{
		case TypeKind.Class:
			if (symbol.IsRecord)
			{
				return SymbolDisplayPartKind.RecordClassName;
			}
			goto case TypeKind.Module;
		case TypeKind.Struct:
			if (symbol.IsRecord)
			{
				return SymbolDisplayPartKind.RecordStructName;
			}
			return SymbolDisplayPartKind.StructName;
		case TypeKind.Module:
		case TypeKind.Submission:
			return SymbolDisplayPartKind.ClassName;
		case TypeKind.Delegate:
			return SymbolDisplayPartKind.DelegateName;
		case TypeKind.Enum:
			return SymbolDisplayPartKind.EnumName;
		case TypeKind.Error:
			return SymbolDisplayPartKind.ErrorTypeName;
		case TypeKind.Interface:
			return SymbolDisplayPartKind.InterfaceName;
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.TypeKind);
		}
	}

	private bool AddSpecialTypeKeyword(INamedTypeSymbol symbol)
	{
		string specialTypeName = GetSpecialTypeName(symbol);
		if (specialTypeName == null)
		{
			return false;
		}
		base.Builder.Add(CreatePart(SymbolDisplayPartKind.Keyword, symbol, specialTypeName));
		return true;
	}

	private static string? GetSpecialTypeName(INamedTypeSymbol symbol)
	{
		switch (symbol.SpecialType)
		{
		case SpecialType.System_Void:
			return "void";
		case SpecialType.System_SByte:
			return "sbyte";
		case SpecialType.System_Int16:
			return "short";
		case SpecialType.System_Int32:
			return "int";
		case SpecialType.System_Int64:
			return "long";
		case SpecialType.System_IntPtr:
			if (symbol.IsNativeIntegerType)
			{
				return "nint";
			}
			break;
		case SpecialType.System_UIntPtr:
			if (symbol.IsNativeIntegerType)
			{
				return "nuint";
			}
			break;
		case SpecialType.System_Byte:
			return "byte";
		case SpecialType.System_UInt16:
			return "ushort";
		case SpecialType.System_UInt32:
			return "uint";
		case SpecialType.System_UInt64:
			return "ulong";
		case SpecialType.System_Single:
			return "float";
		case SpecialType.System_Double:
			return "double";
		case SpecialType.System_Decimal:
			return "decimal";
		case SpecialType.System_Char:
			return "char";
		case SpecialType.System_Boolean:
			return "bool";
		case SpecialType.System_String:
			return "string";
		case SpecialType.System_Object:
			return "object";
		}
		return null;
	}

	private void AddTypeKind(INamedTypeSymbol symbol)
	{
		if (!base.IsFirstSymbolVisited || !base.Format.KindOptions.IncludesOption(SymbolDisplayKindOptions.IncludeTypeKeyword))
		{
			return;
		}
		if (symbol.IsAnonymousType && symbol.TypeKind != TypeKind.Delegate)
		{
			base.Builder.Add(new SymbolDisplayPart(SymbolDisplayPartKind.AnonymousTypeIndicator, null, "AnonymousType"));
			AddSpace();
			return;
		}
		if (symbol.IsTupleType && !ShouldDisplayAsValueTuple(symbol))
		{
			base.Builder.Add(new SymbolDisplayPart(SymbolDisplayPartKind.AnonymousTypeIndicator, null, "Tuple"));
			AddSpace();
			return;
		}
		switch (symbol.TypeKind)
		{
		case TypeKind.Class:
			if (symbol.IsRecord)
			{
				AddKeyword(SyntaxKind.RecordKeyword);
				AddSpace();
				break;
			}
			goto case TypeKind.Module;
		case TypeKind.Struct:
			if (symbol.IsRecord)
			{
				if (symbol.IsReadOnly)
				{
					AddKeyword(SyntaxKind.ReadOnlyKeyword);
					AddSpace();
				}
				AddKeyword(SyntaxKind.RecordKeyword);
				AddSpace();
				AddKeyword(SyntaxKind.StructKeyword);
				AddSpace();
				break;
			}
			if (symbol.IsReadOnly)
			{
				AddKeyword(SyntaxKind.ReadOnlyKeyword);
				AddSpace();
			}
			if (symbol.IsRefLikeType)
			{
				AddKeyword(SyntaxKind.RefKeyword);
				AddSpace();
			}
			AddKeyword(SyntaxKind.StructKeyword);
			AddSpace();
			break;
		case TypeKind.Module:
			AddKeyword(SyntaxKind.ClassKeyword);
			AddSpace();
			break;
		case TypeKind.Enum:
			AddKeyword(SyntaxKind.EnumKeyword);
			AddSpace();
			break;
		case TypeKind.Delegate:
			AddKeyword(SyntaxKind.DelegateKeyword);
			AddSpace();
			break;
		case TypeKind.Interface:
			AddKeyword(SyntaxKind.InterfaceKeyword);
			AddSpace();
			break;
		case TypeKind.Dynamic:
		case TypeKind.Error:
		case TypeKind.Pointer:
			break;
		}
	}

	private void AddTypeParameterVarianceIfNeeded(ITypeParameterSymbol symbol)
	{
		if (base.Format.GenericsOptions.IncludesOption(SymbolDisplayGenericsOptions.IncludeVariance))
		{
			switch (symbol.Variance)
			{
			case VarianceKind.In:
				AddKeyword(SyntaxKind.InKeyword);
				AddSpace();
				break;
			case VarianceKind.Out:
				AddKeyword(SyntaxKind.OutKeyword);
				AddSpace();
				break;
			}
		}
	}

	private void AddTypeArguments(ISymbol owner, ImmutableArray<ImmutableArray<CustomModifier>> modifiers)
	{
		ImmutableArray<ITypeSymbol> immutableArray = ((owner.Kind != SymbolKind.Method) ? ((INamedTypeSymbol)owner).TypeArguments : ((IMethodSymbol)owner).TypeArguments);
		if (immutableArray.Length <= 0 || !base.Format.GenericsOptions.IncludesOption(SymbolDisplayGenericsOptions.IncludeTypeParameters))
		{
			return;
		}
		AddPunctuation(SyntaxKind.LessThanToken);
		bool flag = true;
		for (int i = 0; i < immutableArray.Length; i++)
		{
			ITypeSymbol typeSymbol = immutableArray[i];
			if (!flag)
			{
				AddPunctuation(SyntaxKind.CommaToken);
				AddSpace();
			}
			flag = false;
			AbstractSymbolDisplayVisitor visitor;
			if (typeSymbol.Kind == SymbolKind.TypeParameter)
			{
				ITypeParameterSymbol symbol = (ITypeParameterSymbol)typeSymbol;
				AddTypeParameterVarianceIfNeeded(symbol);
				visitor = base.NotFirstVisitor;
			}
			else
			{
				visitor = base.NotFirstVisitorNamespaceOrType;
			}
			typeSymbol.Accept(visitor);
			if (!modifiers.IsDefault)
			{
				AddCustomModifiersIfNeeded(modifiers[i], leadingSpace: true, trailingSpace: false);
			}
		}
		AddPunctuation(SyntaxKind.GreaterThanToken);
	}

	private static bool TypeParameterHasConstraints(ITypeParameterSymbol typeParam)
	{
		if (typeParam.ConstraintTypes.IsEmpty && !typeParam.HasConstructorConstraint && !typeParam.HasReferenceTypeConstraint && !typeParam.HasValueTypeConstraint && !typeParam.HasNotNullConstraint)
		{
			return typeParam.AllowsRefLikeType;
		}
		return true;
	}

	private void AddTypeParameterConstraints(ImmutableArray<ITypeSymbol> typeArguments)
	{
		if (!base.IsFirstSymbolVisited || !base.Format.GenericsOptions.IncludesOption(SymbolDisplayGenericsOptions.IncludeTypeConstraints))
		{
			return;
		}
		foreach (ITypeSymbol item in typeArguments)
		{
			if (item.Kind != SymbolKind.TypeParameter)
			{
				continue;
			}
			ITypeParameterSymbol typeParameterSymbol = (ITypeParameterSymbol)item;
			if (!TypeParameterHasConstraints(typeParameterSymbol))
			{
				continue;
			}
			AddSpace();
			AddKeyword(SyntaxKind.WhereKeyword);
			AddSpace();
			typeParameterSymbol.Accept(base.NotFirstVisitor);
			AddSpace();
			AddPunctuation(SyntaxKind.ColonToken);
			AddSpace();
			bool flag = false;
			if (typeParameterSymbol.HasReferenceTypeConstraint)
			{
				AddKeyword(SyntaxKind.ClassKeyword);
				switch (typeParameterSymbol.ReferenceTypeConstraintNullableAnnotation)
				{
				case Microsoft.CodeAnalysis.NullableAnnotation.Annotated:
					if (base.Format.MiscellaneousOptions.IncludesOption(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier))
					{
						AddPunctuation(SyntaxKind.QuestionToken);
					}
					break;
				case Microsoft.CodeAnalysis.NullableAnnotation.NotAnnotated:
					if (base.Format.MiscellaneousOptions.IncludesOption(SymbolDisplayMiscellaneousOptions.IncludeNotNullableReferenceTypeModifier))
					{
						AddPunctuation(SyntaxKind.ExclamationToken);
					}
					break;
				}
				flag = true;
			}
			else if (typeParameterSymbol.HasUnmanagedTypeConstraint)
			{
				base.Builder.Add(new SymbolDisplayPart(SymbolDisplayPartKind.Keyword, null, "unmanaged"));
				flag = true;
			}
			else if (typeParameterSymbol.HasValueTypeConstraint)
			{
				AddKeyword(SyntaxKind.StructKeyword);
				flag = true;
			}
			else if (typeParameterSymbol.HasNotNullConstraint)
			{
				base.Builder.Add(new SymbolDisplayPart(SymbolDisplayPartKind.Keyword, null, "notnull"));
				flag = true;
			}
			for (int i = 0; i < typeParameterSymbol.ConstraintTypes.Length; i++)
			{
				ITypeSymbol typeSymbol = typeParameterSymbol.ConstraintTypes[i];
				if (flag)
				{
					AddPunctuation(SyntaxKind.CommaToken);
					AddSpace();
				}
				typeSymbol.Accept(base.NotFirstVisitor);
				flag = true;
			}
			if (typeParameterSymbol.HasConstructorConstraint)
			{
				if (flag)
				{
					AddPunctuation(SyntaxKind.CommaToken);
					AddSpace();
				}
				AddKeyword(SyntaxKind.NewKeyword);
				AddPunctuation(SyntaxKind.OpenParenToken);
				AddPunctuation(SyntaxKind.CloseParenToken);
				flag = true;
			}
			if (typeParameterSymbol.AllowsRefLikeType)
			{
				if (flag)
				{
					AddPunctuation(SyntaxKind.CommaToken);
					AddSpace();
				}
				AddKeyword(SyntaxKind.AllowsKeyword);
				AddSpace();
				AddKeyword(SyntaxKind.RefKeyword);
				AddSpace();
				AddKeyword(SyntaxKind.StructKeyword);
			}
		}
	}

	internal void AddExtensionMarkerName(INamedTypeSymbol extension)
	{
		AddNestedTypeSeparator();
		base.Builder.Add(CreatePart(SymbolDisplayPartKind.ClassName, extension, extension.ExtensionMarkerName));
	}

	private void AddConstantValue(ITypeSymbol type, object? constantValue, bool preferNumericValueOrExpandedFlagsForEnum = false)
	{
		if (constantValue != null)
		{
			AddNonNullConstantValue(type, constantValue, preferNumericValueOrExpandedFlagsForEnum);
			return;
		}
		if (type.IsReferenceType || type.TypeKind == TypeKind.Pointer || ITypeSymbolHelpers.IsNullableType(type))
		{
			AddKeyword(SyntaxKind.NullKeyword);
			return;
		}
		AddKeyword(SyntaxKind.DefaultKeyword);
		if (!base.Format.MiscellaneousOptions.IncludesOption(SymbolDisplayMiscellaneousOptions.AllowDefaultLiteral))
		{
			AddPunctuation(SyntaxKind.OpenParenToken);
			type.Accept(base.NotFirstVisitor);
			AddPunctuation(SyntaxKind.CloseParenToken);
		}
	}

	protected override void AddExplicitlyCastedLiteralValue(INamedTypeSymbol namedType, SpecialType type, object value)
	{
		AddPunctuation(SyntaxKind.OpenParenToken);
		namedType.Accept(base.NotFirstVisitor);
		AddPunctuation(SyntaxKind.CloseParenToken);
		AddLiteralValue(type, value);
	}

	protected override void AddLiteralValue(SpecialType type, object value)
	{
		string text = SymbolDisplay.FormatPrimitive(value, quoteStrings: true, useHexadecimalNumbers: false);
		SymbolDisplayPartKind kind = SymbolDisplayPartKind.NumericLiteral;
		switch (type)
		{
		case SpecialType.System_Boolean:
			kind = SymbolDisplayPartKind.Keyword;
			break;
		case SpecialType.System_Char:
		case SpecialType.System_String:
			kind = SymbolDisplayPartKind.StringLiteral;
			break;
		}
		base.Builder.Add(CreatePart(kind, null, text));
	}

	protected override void AddBitwiseOr()
	{
		AddPunctuation(SyntaxKind.BarToken);
	}

	private bool TryAddAlias(INamespaceOrTypeSymbol symbol, ArrayBuilder<SymbolDisplayPart> builder)
	{
		IAliasSymbol aliasSymbol = GetAliasSymbol(symbol);
		if (aliasSymbol != null)
		{
			string name = aliasSymbol.Name;
			ImmutableArray<ISymbol> immutableArray = base.SemanticModelOpt.LookupNamespacesAndTypes(base.PositionOpt, null, name);
			if (immutableArray.Length == 1 && immutableArray[0] is IAliasSymbol && aliasSymbol.Target.Equals(symbol))
			{
				builder.Add(CreatePart(SymbolDisplayPartKind.AliasName, aliasSymbol, name));
				return true;
			}
		}
		return false;
	}

	protected override bool ShouldRestrictMinimallyQualifyLookupToNamespacesAndTypes()
	{
		SyntaxToken token = base.SemanticModelOpt.SyntaxTree.GetRoot().FindToken(base.PositionOpt);
		if (!SyntaxFacts.IsInNamespaceOrTypeContext(token.Parent as ExpressionSyntax) && !token.IsKind(SyntaxKind.NewKeyword))
		{
			return base.InNamespaceOrType;
		}
		return true;
	}

	private void MinimallyQualify(INamespaceSymbol symbol)
	{
		if (symbol.IsGlobalNamespace)
		{
			return;
		}
		ImmutableArray<ISymbol> immutableArray = (ShouldRestrictMinimallyQualifyLookupToNamespacesAndTypes() ? base.SemanticModelOpt.LookupNamespacesAndTypes(base.PositionOpt, null, symbol.Name) : base.SemanticModelOpt.LookupSymbols(base.PositionOpt, null, symbol.Name));
		ISymbol symbol2 = immutableArray.OfType<ISymbol>().FirstOrDefault();
		if (immutableArray.Length != 1 || symbol2 == null || !symbol2.Equals(symbol))
		{
			INamespaceSymbol namespaceSymbol = ((symbol.ContainingNamespace == null) ? null : base.SemanticModelOpt.Compilation.GetCompilationNamespace(symbol.ContainingNamespace));
			if (namespaceSymbol != null)
			{
				if (namespaceSymbol.IsGlobalNamespace)
				{
					if (base.Format.GlobalNamespaceStyle == SymbolDisplayGlobalNamespaceStyle.Included)
					{
						AddGlobalNamespace(namespaceSymbol);
						AddPunctuation(SyntaxKind.ColonColonToken);
					}
				}
				else
				{
					namespaceSymbol.Accept(base.NotFirstVisitor);
					AddPunctuation(SyntaxKind.DotToken);
				}
			}
		}
		base.Builder.Add(CreatePart(SymbolDisplayPartKind.NamespaceName, symbol, symbol.Name));
	}

	private void MinimallyQualify(INamedTypeSymbol symbol)
	{
		if (!symbol.IsAnonymousType && !symbol.IsTupleType && !NameBoundSuccessfullyToSameSymbol(symbol))
		{
			if (IncludeNamedType(symbol.ContainingType))
			{
				symbol.ContainingType.Accept(base.NotFirstVisitor);
				AddPunctuation(SyntaxKind.DotToken);
			}
			else
			{
				INamespaceSymbol namespaceSymbol = ((symbol.ContainingNamespace == null) ? null : base.SemanticModelOpt.Compilation.GetCompilationNamespace(symbol.ContainingNamespace));
				if (namespaceSymbol != null)
				{
					if (namespaceSymbol.IsGlobalNamespace)
					{
						if (symbol.TypeKind != TypeKind.Error)
						{
							AddKeyword(SyntaxKind.GlobalKeyword);
							AddPunctuation(SyntaxKind.ColonColonToken);
						}
					}
					else
					{
						namespaceSymbol.Accept(base.NotFirstVisitor);
						AddPunctuation(SyntaxKind.DotToken);
					}
				}
			}
		}
		AddNameAndTypeArgumentsOrParameters(symbol);
	}

	private IDictionary<INamespaceOrTypeSymbol, IAliasSymbol> CreateAliasMap()
	{
		if (!base.IsMinimizing)
		{
			return SpecializedCollections.EmptyDictionary<INamespaceOrTypeSymbol, IAliasSymbol>();
		}
		SemanticModel semanticModel;
		int position;
		if (base.SemanticModelOpt.IsSpeculativeSemanticModel)
		{
			semanticModel = base.SemanticModelOpt.ParentModel;
			position = base.SemanticModelOpt.OriginalPositionForSpeculation;
		}
		else
		{
			semanticModel = base.SemanticModelOpt;
			position = base.PositionOpt;
		}
		SyntaxToken syntaxToken = semanticModel.SyntaxTree.GetRoot().FindToken(position);
		SyntaxNode parent;
		for (parent = syntaxToken.Parent; parent != null; parent = parent.Parent)
		{
			if (parent is UsingDirectiveSyntax)
			{
				parent = parent.Parent.Parent;
				break;
			}
		}
		if (parent == null)
		{
			parent = syntaxToken.Parent;
		}
		ImmutableDictionary<INamespaceOrTypeSymbol, IAliasSymbol>.Builder builder = ImmutableDictionary.CreateBuilder<INamespaceOrTypeSymbol, IAliasSymbol>();
		while (parent != null)
		{
			SyntaxList<UsingDirectiveSyntax>? syntaxList = (parent as BaseNamespaceDeclarationSyntax)?.Usings;
			SyntaxList<UsingDirectiveSyntax>? syntaxList2 = syntaxList;
			if (!syntaxList2.HasValue)
			{
				syntaxList = (parent as CompilationUnitSyntax)?.Usings;
			}
			if (syntaxList.HasValue)
			{
				foreach (UsingDirectiveSyntax item in syntaxList.Value)
				{
					if (item.Alias != null)
					{
						IAliasSymbol declaredSymbol = semanticModel.GetDeclaredSymbol(item);
						if (declaredSymbol != null && !builder.ContainsKey(declaredSymbol.Target))
						{
							builder.Add(declaredSymbol.Target, declaredSymbol);
						}
					}
				}
			}
			parent = parent.Parent;
		}
		return builder.ToImmutable();
	}

	private ITypeSymbol? GetRangeVariableType(IRangeVariableSymbol symbol)
	{
		ITypeSymbol result = null;
		if (base.IsMinimizing && !symbol.Locations.IsEmpty)
		{
			Location location = symbol.Locations.First();
			if (location.IsInSource && location.SourceTree == base.SemanticModelOpt.SyntaxTree)
			{
				SyntaxToken token = location.SourceTree.GetRoot().FindToken(base.PositionOpt);
				QueryBodySyntax queryBody = GetQueryBody(token);
				if (queryBody != null)
				{
					IdentifierNameSyntax expression = SyntaxFactory.IdentifierName(symbol.Name);
					result = base.SemanticModelOpt.GetSpeculativeTypeInfo(queryBody.SelectOrGroup.Span.End - 1, expression, SpeculativeBindingOption.BindAsExpression).Type;
				}
				if (token.Parent is IdentifierNameSyntax node)
				{
					result = base.SemanticModelOpt.GetTypeInfo(node).Type;
				}
			}
		}
		return result;
	}

	private static QueryBodySyntax? GetQueryBody(SyntaxToken token)
	{
		SyntaxNode parent = token.Parent;
		if (!(parent is FromClauseSyntax fromClauseSyntax))
		{
			if (!(parent is LetClauseSyntax letClauseSyntax))
			{
				if (!(parent is JoinClauseSyntax joinClauseSyntax))
				{
					if (parent is QueryContinuationSyntax queryContinuationSyntax && queryContinuationSyntax.Identifier == token)
					{
						return queryContinuationSyntax.Body;
					}
				}
				else if (joinClauseSyntax.Identifier == token)
				{
					return joinClauseSyntax.Parent as QueryBodySyntax;
				}
			}
			else if (letClauseSyntax.Identifier == token)
			{
				return letClauseSyntax.Parent as QueryBodySyntax;
			}
		}
		else if (fromClauseSyntax.Identifier == token)
		{
			return (fromClauseSyntax.Parent as QueryBodySyntax) ?? ((QueryExpressionSyntax)fromClauseSyntax.Parent).Body;
		}
		return null;
	}

	private string RemoveAttributeSuffixIfNecessary(INamedTypeSymbol symbol, string symbolName)
	{
		if (base.IsMinimizing && base.Format.MiscellaneousOptions.IncludesOption(SymbolDisplayMiscellaneousOptions.RemoveAttributeSuffix) && base.SemanticModelOpt.Compilation.IsAttributeType(symbol) && symbolName.TryGetWithoutAttributeSuffix(out string result) && SyntaxFactory.ParseToken(result).IsKind(SyntaxKind.IdentifierToken))
		{
			symbolName = result;
		}
		return symbolName;
	}

	private IAliasSymbol? GetAliasSymbol(INamespaceOrTypeSymbol symbol)
	{
		if (!AliasMap.TryGetValue(symbol, out IAliasSymbol value))
		{
			return null;
		}
		return value;
	}
}
