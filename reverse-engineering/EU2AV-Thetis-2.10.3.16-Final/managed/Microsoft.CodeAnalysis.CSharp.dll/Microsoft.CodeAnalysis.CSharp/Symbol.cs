using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract class Symbol : IReference, ISymbolInternal, IFormattable
{
	[Flags]
	internal enum AllowedRequiredModifierType
	{
		None = 0,
		System_Runtime_CompilerServices_Volatile = 1,
		System_Runtime_InteropServices_InAttribute = 2,
		System_Runtime_CompilerServices_IsExternalInit = 4,
		System_Runtime_InteropServices_OutAttribute = 8
	}

	[Flags]
	internal enum ReservedAttributes
	{
		DynamicAttribute = 2,
		IsReadOnlyAttribute = 4,
		IsUnmanagedAttribute = 8,
		IsByRefLikeAttribute = 0x10,
		TupleElementNamesAttribute = 0x20,
		NullableAttribute = 0x40,
		NullableContextAttribute = 0x80,
		NullablePublicOnlyAttribute = 0x100,
		NativeIntegerAttribute = 0x200,
		CaseSensitiveExtensionAttribute = 0x400,
		RequiredMemberAttribute = 0x800,
		ScopedRefAttribute = 0x1000,
		RefSafetyRulesAttribute = 0x2000,
		RequiresLocationAttribute = 0x4000,
		ExtensionMarkerAttribute = 0x8000
	}

	private ISymbol _lazyISymbol;

	private static readonly SymbolDisplayFormat s_debuggerDisplayFormat = SymbolDisplayFormat.TestFormat.AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier | SymbolDisplayMiscellaneousOptions.IncludeNotNullableReferenceTypeModifier).WithCompilerInternalOptions(SymbolDisplayCompilerInternalOptions.IncludeContainingFileForFileTypes);

	internal Symbol AdaptedSymbol => this;

	internal virtual bool RequiresCompletion => false;

	public virtual string Name => string.Empty;

	public virtual string MetadataName => Name;

	public virtual int MetadataToken => 0;

	public abstract SymbolKind Kind { get; }

	public abstract Symbol ContainingSymbol { get; }

	public virtual NamedTypeSymbol ContainingType
	{
		get
		{
			Symbol containingSymbol = ContainingSymbol;
			NamedTypeSymbol namedTypeSymbol = containingSymbol as NamedTypeSymbol;
			if ((object)namedTypeSymbol == containingSymbol)
			{
				return namedTypeSymbol;
			}
			return containingSymbol.ContainingType;
		}
	}

	public virtual NamespaceSymbol ContainingNamespace
	{
		get
		{
			Symbol containingSymbol = ContainingSymbol;
			while ((object)containingSymbol != null)
			{
				if (containingSymbol is NamespaceSymbol result)
				{
					return result;
				}
				containingSymbol = containingSymbol.ContainingSymbol;
			}
			return null;
		}
	}

	public virtual AssemblySymbol ContainingAssembly => ContainingSymbol?.ContainingAssembly;

	internal virtual CSharpCompilation DeclaringCompilation
	{
		get
		{
			if (!IsDefinition)
			{
				return OriginalDefinition.DeclaringCompilation;
			}
			switch (Kind)
			{
			case SymbolKind.ErrorType:
				return null;
			case SymbolKind.Assembly:
				return null;
			case SymbolKind.NetModule:
				return null;
			default:
			{
				ModuleSymbol containingModule = ContainingModule;
				if (!(containingModule is SourceModuleSymbol sourceModuleSymbol))
				{
					if (containingModule is PEModuleSymbol)
					{
						return ContainingSymbol?.DeclaringCompilation;
					}
					return null;
				}
				return sourceModuleSymbol.DeclaringCompilation;
			}
			}
		}
	}

	Compilation ISymbolInternal.DeclaringCompilation => DeclaringCompilation;

	string ISymbolInternal.Name => Name;

	string ISymbolInternal.MetadataName => MetadataName;

	public virtual TypeMemberVisibility MetadataVisibility
	{
		get
		{
			switch (DeclaredAccessibility)
			{
			case Accessibility.Public:
				return TypeMemberVisibility.Public;
			case Accessibility.Private:
			{
				NamedTypeSymbol containingType = ContainingType;
				if ((object)containingType != null && containingType.TypeKind == TypeKind.Submission)
				{
					return TypeMemberVisibility.Public;
				}
				return TypeMemberVisibility.Private;
			}
			case Accessibility.Internal:
				if (ContainingAssembly.IsInteractive)
				{
					return TypeMemberVisibility.Public;
				}
				return TypeMemberVisibility.Assembly;
			case Accessibility.Protected:
				if (ContainingType.TypeKind == TypeKind.Submission)
				{
					return TypeMemberVisibility.Public;
				}
				return TypeMemberVisibility.Family;
			case Accessibility.ProtectedAndInternal:
				return TypeMemberVisibility.FamilyAndAssembly;
			case Accessibility.ProtectedOrInternal:
				if (ContainingAssembly.IsInteractive)
				{
					return TypeMemberVisibility.Public;
				}
				return TypeMemberVisibility.FamilyOrAssembly;
			default:
				throw ExceptionUtilities.UnexpectedValue(DeclaredAccessibility);
			}
		}
	}

	ISymbolInternal ISymbolInternal.ContainingSymbol => ContainingSymbol;

	IModuleSymbolInternal ISymbolInternal.ContainingModule => ContainingModule;

	IAssemblySymbolInternal ISymbolInternal.ContainingAssembly => ContainingAssembly;

	ImmutableArray<Location> ISymbolInternal.Locations => Locations;

	INamespaceSymbolInternal ISymbolInternal.ContainingNamespace => ContainingNamespace;

	bool ISymbolInternal.IsImplicitlyDeclared => IsImplicitlyDeclared;

	INamedTypeSymbolInternal ISymbolInternal.ContainingType => ContainingType;

	internal virtual ModuleSymbol ContainingModule => ContainingSymbol?.ContainingModule;

	internal virtual int? MemberIndexOpt => null;

	public Symbol OriginalDefinition => OriginalSymbolDefinition;

	protected virtual Symbol OriginalSymbolDefinition => this;

	public bool IsDefinition => (object)this == OriginalDefinition;

	public abstract ImmutableArray<Location> Locations { get; }

	public abstract ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

	public abstract Accessibility DeclaredAccessibility { get; }

	public abstract bool IsStatic { get; }

	public abstract bool IsVirtual { get; }

	public abstract bool IsOverride { get; }

	public abstract bool IsAbstract { get; }

	public abstract bool IsSealed { get; }

	public abstract bool IsExtern { get; }

	public virtual bool IsImplicitlyDeclared => false;

	public bool CanBeReferencedByName
	{
		get
		{
			switch (Kind)
			{
			case SymbolKind.Alias:
			case SymbolKind.Label:
			case SymbolKind.Local:
			case SymbolKind.RangeVariable:
				return true;
			case SymbolKind.NamedType:
			{
				NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)this;
				if (namedTypeSymbol.IsSubmissionClass || namedTypeSymbol.IsExtension)
				{
					return false;
				}
				break;
			}
			case SymbolKind.Property:
			{
				PropertySymbol propertySymbol = (PropertySymbol)this;
				if (propertySymbol.IsIndexer || propertySymbol.MustCallMethodsDirectly)
				{
					return false;
				}
				break;
			}
			case SymbolKind.Method:
			{
				MethodSymbol methodSymbol = (MethodSymbol)this;
				switch (methodSymbol.MethodKind)
				{
				case MethodKind.Destructor:
					return true;
				case MethodKind.DelegateInvoke:
					return true;
				case MethodKind.PropertyGet:
				case MethodKind.PropertySet:
					if (!((PropertySymbol)methodSymbol.AssociatedSymbol).CanCallMethodsDirectly())
					{
						return false;
					}
					break;
				default:
					return false;
				case MethodKind.Ordinary:
				case MethodKind.ReducedExtension:
				case MethodKind.LocalFunction:
					break;
				}
				break;
			}
			case SymbolKind.ArrayType:
			case SymbolKind.Assembly:
			case SymbolKind.DynamicType:
			case SymbolKind.NetModule:
			case SymbolKind.PointerType:
			case SymbolKind.Discard:
			case SymbolKind.FunctionPointerType:
				return false;
			default:
				throw ExceptionUtilities.UnexpectedValue(Kind);
			case SymbolKind.ErrorType:
			case SymbolKind.Event:
			case SymbolKind.Field:
			case SymbolKind.Namespace:
			case SymbolKind.Parameter:
			case SymbolKind.TypeParameter:
				break;
			}
			if (SyntaxFacts.IsValidIdentifier(Name))
			{
				return !SyntaxFacts.ContainsDroppedIdentifierCharacters(Name);
			}
			return false;
		}
	}

	internal bool CanBeReferencedByNameIgnoringIllegalCharacters
	{
		get
		{
			if (Kind == SymbolKind.Method)
			{
				MethodSymbol methodSymbol = (MethodSymbol)this;
				switch (methodSymbol.MethodKind)
				{
				case MethodKind.DelegateInvoke:
				case MethodKind.Destructor:
				case MethodKind.Ordinary:
				case MethodKind.LocalFunction:
					return true;
				case MethodKind.PropertyGet:
				case MethodKind.PropertySet:
					return ((PropertySymbol)methodSymbol.AssociatedSymbol).CanCallMethodsDirectly();
				default:
					return false;
				}
			}
			return true;
		}
	}

	internal bool Dangerous_IsFromSomeCompilation => DeclaringCompilation != null;

	internal bool HasUseSiteError
	{
		get
		{
			DiagnosticInfo? diagnosticInfo = GetUseSiteInfo().DiagnosticInfo;
			if (diagnosticInfo == null)
			{
				return false;
			}
			return diagnosticInfo.Severity == DiagnosticSeverity.Error;
		}
	}

	protected AssemblySymbol PrimaryDependency
	{
		get
		{
			AssemblySymbol containingAssembly = ContainingAssembly;
			if ((object)containingAssembly != null && containingAssembly.CorLibrary == containingAssembly)
			{
				return null;
			}
			return containingAssembly;
		}
	}

	public virtual bool HasUnsupportedMetadata => false;

	internal ThreeState ObsoleteState
	{
		get
		{
			switch (ObsoleteKind)
			{
			case ObsoleteAttributeKind.None:
			case ObsoleteAttributeKind.WindowsExperimental:
			case ObsoleteAttributeKind.Experimental:
				return ThreeState.False;
			case ObsoleteAttributeKind.Uninitialized:
				return ThreeState.Unknown;
			default:
				return ThreeState.True;
			}
		}
	}

	internal ThreeState ExperimentalState => ObsoleteKind switch
	{
		ObsoleteAttributeKind.Experimental => ThreeState.True, 
		ObsoleteAttributeKind.Uninitialized => ThreeState.Unknown, 
		_ => ThreeState.False, 
	};

	internal ObsoleteAttributeKind ObsoleteKind => ObsoleteAttributeData?.Kind ?? ObsoleteAttributeKind.None;

	internal abstract ObsoleteAttributeData? ObsoleteAttributeData { get; }

	bool ISymbolInternal.IsStatic => IsStatic;

	bool ISymbolInternal.IsVirtual => IsVirtual;

	bool ISymbolInternal.IsOverride => IsOverride;

	bool ISymbolInternal.IsAbstract => IsAbstract;

	bool ISymbolInternal.IsExtern => IsExtern;

	Accessibility ISymbolInternal.DeclaredAccessibility => DeclaredAccessibility;

	internal ISymbol ISymbol
	{
		get
		{
			if (_lazyISymbol == null)
			{
				Interlocked.CompareExchange(ref _lazyISymbol, CreateISymbol(), null);
			}
			return _lazyISymbol;
		}
	}

	public static bool IsSymbolAccessible(Symbol symbol, NamedTypeSymbol within, NamedTypeSymbol throughTypeOpt = null)
	{
		if ((object)symbol == null)
		{
			throw new ArgumentNullException("symbol");
		}
		if ((object)within == null)
		{
			throw new ArgumentNullException("within");
		}
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
		return AccessCheck.IsSymbolAccessible(symbol, within, ref useSiteInfo, throughTypeOpt);
	}

	public static bool IsSymbolAccessible(Symbol symbol, AssemblySymbol within)
	{
		if ((object)symbol == null)
		{
			throw new ArgumentNullException("symbol");
		}
		if ((object)within == null)
		{
			throw new ArgumentNullException("within");
		}
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
		return AccessCheck.IsSymbolAccessible(symbol, within, ref useSiteInfo);
	}

	IDefinition IReference.AsDefinition(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Emitter/Model/SymbolAdapter.cs", 32);
	}

	ISymbolInternal IReference.GetInternalSymbol()
	{
		return AdaptedSymbol;
	}

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Emitter/Model/SymbolAdapter.cs", 39);
	}

	IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context)
	{
		return AdaptedSymbol.GetCustomAttributesToEmit((PEModuleBuilder)context.Module);
	}

	internal Symbol GetCciAdapter()
	{
		return this;
	}

	[Conditional("DEBUG")]
	protected internal void CheckDefinitionInvariant()
	{
	}

	IReference ISymbolInternal.GetCciAdapter()
	{
		return GetCciAdapter();
	}

	internal bool IsDefinitionOrDistinct()
	{
		if (!IsDefinition)
		{
			return !Equals(OriginalDefinition, SymbolEqualityComparer.ConsiderEverything.CompareKind);
		}
		return true;
	}

	internal virtual IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		return GetCustomAttributesToEmit(moduleBuilder, emittingAssemblyAttributesInNetModule: false);
	}

	internal IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder, bool emittingAssemblyAttributesInNetModule)
	{
		ArrayBuilder<CSharpAttributeData> attributes = null;
		ImmutableArray<CSharpAttributeData> attributes2 = GetAttributes();
		AddSynthesizedAttributes(moduleBuilder, ref attributes);
		return GetCustomAttributesToEmit(attributes2, attributes, isReturnType: false, emittingAssemblyAttributesInNetModule);
	}

	internal IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(ImmutableArray<CSharpAttributeData> userDefined, ArrayBuilder<CSharpAttributeData> synthesized, bool isReturnType, bool emittingAssemblyAttributesInNetModule)
	{
		if (userDefined.IsEmpty && synthesized == null)
		{
			return SpecializedCollections.EmptyEnumerable<CSharpAttributeData>();
		}
		return GetCustomAttributesToEmitIterator(userDefined, synthesized, isReturnType, emittingAssemblyAttributesInNetModule);
	}

	private IEnumerable<CSharpAttributeData> GetCustomAttributesToEmitIterator(ImmutableArray<CSharpAttributeData> userDefined, ArrayBuilder<CSharpAttributeData> synthesized, bool isReturnType, bool emittingAssemblyAttributesInNetModule)
	{
		if (synthesized != null)
		{
			foreach (CSharpAttributeData item in synthesized)
			{
				yield return item;
			}
			synthesized.Free();
		}
		for (int i = 0; i < userDefined.Length; i++)
		{
			CSharpAttributeData cSharpAttributeData = userDefined[i];
			if ((Kind != SymbolKind.Assembly || !((SourceAssemblySymbol)this).IsIndexOfOmittedAssemblyAttribute(i)) && cSharpAttributeData.ShouldEmitAttribute(this, isReturnType, emittingAssemblyAttributesInNetModule))
			{
				yield return cSharpAttributeData;
			}
		}
	}

	internal virtual void ForceComplete(SourceLocation? locationOpt, Predicate<Symbol>? filter, CancellationToken cancellationToken)
	{
	}

	internal virtual bool HasComplete(CompletionPart part)
	{
		return true;
	}

	ISymbol ISymbolInternal.GetISymbol()
	{
		return ISymbol;
	}

	internal virtual LexicalSortKey GetLexicalSortKey()
	{
		Location location = TryGetFirstLocation();
		if ((object)location == null)
		{
			return LexicalSortKey.NotInSource;
		}
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		return new LexicalSortKey(location, declaringCompilation);
	}

	public virtual Location? TryGetFirstLocation()
	{
		ImmutableArray<Location> locations = Locations;
		if (!locations.IsEmpty)
		{
			return locations[0];
		}
		return null;
	}

	public Location GetFirstLocation()
	{
		return TryGetFirstLocation() ?? throw new InvalidOperationException("Symbol has no locations");
	}

	public Location GetFirstLocationOrNone()
	{
		return TryGetFirstLocation() ?? Location.None;
	}

	public virtual bool HasLocationContainedWithin(SyntaxTree tree, TextSpan declarationSpan, out bool wasZeroWidthMatch)
	{
		foreach (Location location in Locations)
		{
			if (IsLocationContainedWithin(location, tree, declarationSpan, out wasZeroWidthMatch))
			{
				return true;
			}
		}
		wasZeroWidthMatch = false;
		return false;
	}

	protected static bool IsLocationContainedWithin(Location loc, SyntaxTree tree, TextSpan declarationSpan, out bool wasZeroWidthMatch)
	{
		if (loc.IsInSource && loc.SourceTree == tree && declarationSpan.Contains(loc.SourceSpan))
		{
			wasZeroWidthMatch = loc.SourceSpan.IsEmpty && loc.SourceSpan.End == declarationSpan.Start;
			return true;
		}
		wasZeroWidthMatch = false;
		return false;
	}

	internal static ImmutableArray<SyntaxReference> GetDeclaringSyntaxReferenceHelper<TNode>(ImmutableArray<Location> locations) where TNode : CSharpSyntaxNode
	{
		if (locations.IsEmpty)
		{
			return ImmutableArray<SyntaxReference>.Empty;
		}
		ArrayBuilder<SyntaxReference> instance = ArrayBuilder<SyntaxReference>.GetInstance();
		foreach (Location location in locations)
		{
			if (location == null || !location.IsInSource)
			{
				continue;
			}
			if (location.SourceSpan.Length != 0)
			{
				SyntaxToken token = location.SourceTree.GetRoot().FindToken(location.SourceSpan.Start);
				if (token.Kind() != SyntaxKind.None)
				{
					CSharpSyntaxNode cSharpSyntaxNode = token.Parent.FirstAncestorOrSelf<TNode>();
					if (cSharpSyntaxNode != null)
					{
						instance.Add(cSharpSyntaxNode.GetReference());
					}
				}
				continue;
			}
			SyntaxNode root = location.SourceTree.GetRoot();
			SyntaxNode syntaxNode = null;
			foreach (SyntaxNode item in root.DescendantNodesAndSelf((SyntaxNode c) => c.Location.SourceSpan.Contains(location.SourceSpan)))
			{
				if (item is TNode && item.Location.SourceSpan.Contains(location.SourceSpan))
				{
					syntaxNode = item;
				}
			}
			if (syntaxNode != null)
			{
				instance.Add(syntaxNode.GetReference());
			}
		}
		return instance.ToImmutableAndFree();
	}

	internal virtual void AfterAddingTypeMembersChecks(ConversionsBase conversions, BindingDiagnosticBag diagnostics)
	{
	}

	public static bool operator ==(Symbol left, Symbol right)
	{
		if ((object)right == null)
		{
			return (object)left == null;
		}
		if ((object)left != right)
		{
			return right.Equals(left);
		}
		return true;
	}

	public static bool operator !=(Symbol left, Symbol right)
	{
		if ((object)right == null)
		{
			return (object)left != null;
		}
		if ((object)left != right)
		{
			return !right.Equals(left);
		}
		return false;
	}

	public sealed override bool Equals(object obj)
	{
		return Equals(obj as Symbol, SymbolEqualityComparer.Default.CompareKind);
	}

	public bool Equals(Symbol other)
	{
		return Equals(other, SymbolEqualityComparer.Default.CompareKind);
	}

	bool ISymbolInternal.Equals(ISymbolInternal other, TypeCompareKind compareKind)
	{
		return Equals(other as Symbol, compareKind);
	}

	public virtual bool Equals(Symbol other, TypeCompareKind compareKind)
	{
		return (object)this == other;
	}

	public override int GetHashCode()
	{
		return RuntimeHelpers.GetHashCode(this);
	}

	public static bool Equals(Symbol first, Symbol second, TypeCompareKind compareKind)
	{
		return first?.Equals(second, compareKind) ?? ((object)second == null);
	}

	public sealed override string ToString()
	{
		return ToDisplayString();
	}

	internal abstract TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument a);

	internal Symbol()
	{
	}

	internal virtual void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
	}

	internal static void AddSynthesizedAttribute(ref ArrayBuilder<CSharpAttributeData> attributes, CSharpAttributeData attribute)
	{
		if (attribute != null)
		{
			if (attributes == null)
			{
				attributes = new ArrayBuilder<CSharpAttributeData>(1);
			}
			attributes.Add(attribute);
		}
	}

	internal CharSet? GetEffectiveDefaultMarshallingCharSet()
	{
		return ContainingModule.DefaultMarshallingCharSet;
	}

	internal bool IsFromCompilation(CSharpCompilation compilation)
	{
		return compilation == DeclaringCompilation;
	}

	public virtual bool IsDefinedInSourceTree(SyntaxTree tree, TextSpan? definedWithinSpan, CancellationToken cancellationToken = default(CancellationToken))
	{
		ImmutableArray<SyntaxReference> declaringSyntaxReferences = DeclaringSyntaxReferences;
		if (IsImplicitlyDeclared && declaringSyntaxReferences.Length == 0)
		{
			return ContainingSymbol.IsDefinedInSourceTree(tree, definedWithinSpan, cancellationToken);
		}
		foreach (SyntaxReference item in declaringSyntaxReferences)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (IsDefinedInSourceTree(item, tree, definedWithinSpan))
			{
				return true;
			}
		}
		return false;
	}

	protected static bool IsDefinedInSourceTree(SyntaxReference syntaxRef, SyntaxTree tree, TextSpan? definedWithinSpan)
	{
		if (syntaxRef.SyntaxTree == tree)
		{
			if (definedWithinSpan.HasValue)
			{
				return syntaxRef.Span.IntersectsWith(definedWithinSpan.Value);
			}
			return true;
		}
		return false;
	}

	internal static void ForceCompleteMemberConditionally(SourceLocation? locationOpt, Predicate<Symbol>? filter, Symbol member, CancellationToken cancellationToken)
	{
		if ((locationOpt == null || member.IsDefinedInSourceTree(locationOpt.SourceTree, locationOpt.SourceSpan, cancellationToken)) && (filter == null || filter(member)))
		{
			cancellationToken.ThrowIfCancellationRequested();
			member.ForceComplete(locationOpt, filter, cancellationToken);
		}
	}

	public virtual string? GetDocumentationCommentId()
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		try
		{
			StringBuilder builder = instance.Builder;
			DocumentationCommentIDVisitor.Instance.Visit(this, builder);
			return (builder.Length == 0) ? null : builder.ToString();
		}
		finally
		{
			instance.Free();
		}
	}

	public string? GetEscapedDocumentationCommentId()
	{
		string documentationCommentId = GetDocumentationCommentId();
		if (documentationCommentId != null)
		{
			return escape(documentationCommentId);
		}
		return null;
		static string escape(string s)
		{
			return s.Replace("<", "&lt;").Replace(">", "&gt;");
		}
	}

	public virtual string GetDocumentationCommentXml(CultureInfo? preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return "";
	}

	internal virtual string GetDebuggerDisplay()
	{
		return $"{Kind} {ToDisplayString(s_debuggerDisplayFormat)}";
	}

	internal virtual void AddDeclarationDiagnostics(BindingDiagnosticBag diagnostics)
	{
		DiagnosticBag? diagnosticBag = diagnostics.DiagnosticBag;
		if (diagnosticBag == null || diagnosticBag.IsEmptyWithoutResolution)
		{
			ICollection<AssemblySymbol>? dependenciesBag = diagnostics.DependenciesBag;
			if (dependenciesBag == null || dependenciesBag.Count <= 0)
			{
				return;
			}
		}
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		declaringCompilation.AddUsedAssemblies(diagnostics.DependenciesBag);
		DiagnosticBag? diagnosticBag2 = diagnostics.DiagnosticBag;
		if (diagnosticBag2 != null && !diagnosticBag2.IsEmptyWithoutResolution)
		{
			declaringCompilation.DeclarationDiagnostics.AddRange(diagnostics.DiagnosticBag);
		}
	}

	internal virtual UseSiteInfo<AssemblySymbol> GetUseSiteInfo()
	{
		return default(UseSiteInfo<AssemblySymbol>);
	}

	protected virtual bool IsHighestPriorityUseSiteErrorCode(int code)
	{
		return true;
	}

	internal bool MergeUseSiteDiagnostics(ref DiagnosticInfo result, DiagnosticInfo info)
	{
		if (info == null)
		{
			return false;
		}
		if (info.Severity == DiagnosticSeverity.Error && IsHighestPriorityUseSiteErrorCode(info.Code))
		{
			result = info;
			return true;
		}
		if (result == null || (result.Severity == DiagnosticSeverity.Warning && info.Severity == DiagnosticSeverity.Error))
		{
			result = info;
			return false;
		}
		return false;
	}

	internal bool MergeUseSiteInfo(ref UseSiteInfo<AssemblySymbol> result, UseSiteInfo<AssemblySymbol> info)
	{
		DiagnosticInfo result2 = result.DiagnosticInfo;
		bool result3 = MergeUseSiteDiagnostics(ref result2, info.DiagnosticInfo);
		if (result2 != null && result2.Severity == DiagnosticSeverity.Error)
		{
			result = new UseSiteInfo<AssemblySymbol>(result2);
			return result3;
		}
		ImmutableHashSet<AssemblySymbol> secondaryDependencies = result.SecondaryDependencies;
		AssemblySymbol primaryDependency = result.PrimaryDependency;
		info.MergeDependencies(ref primaryDependency, ref secondaryDependencies);
		result = new UseSiteInfo<AssemblySymbol>(result2, primaryDependency, secondaryDependencies);
		return result3;
	}

	internal static bool ReportUseSiteDiagnostic(DiagnosticInfo info, DiagnosticBag diagnostics, Location location)
	{
		if (info.Code == 1702 || info.Code == 1701 || info.Code == 1705)
		{
			location = NoLocation.Singleton;
		}
		diagnostics.Add(info, location);
		return info.Severity == DiagnosticSeverity.Error;
	}

	internal static bool ReportUseSiteDiagnostic(DiagnosticInfo info, BindingDiagnosticBag diagnostics, Location location)
	{
		return diagnostics.ReportUseSiteDiagnostic(info, location);
	}

	internal bool DeriveUseSiteInfoFromType(ref UseSiteInfo<AssemblySymbol> result, TypeSymbol type)
	{
		UseSiteInfo<AssemblySymbol> info = type.GetUseSiteInfo();
		DiagnosticInfo? diagnosticInfo = info.DiagnosticInfo;
		if (diagnosticInfo != null && diagnosticInfo.Code == 648)
		{
			GetSymbolSpecificUnsupportedMetadataUseSiteErrorInfo(ref info);
		}
		return MergeUseSiteInfo(ref result, info);
	}

	private void GetSymbolSpecificUnsupportedMetadataUseSiteErrorInfo(ref UseSiteInfo<AssemblySymbol> info)
	{
		SymbolKind kind = Kind;
		if ((uint)(kind - 5) <= 1u || kind == SymbolKind.Method || kind == SymbolKind.Property)
		{
			info = info.AdjustDiagnosticInfo(new CSDiagnosticInfo(ErrorCode.ERR_BindToBogus, this));
		}
	}

	private UseSiteInfo<AssemblySymbol> GetSymbolSpecificUnsupportedMetadataUseSiteErrorInfo()
	{
		UseSiteInfo<AssemblySymbol> info = new UseSiteInfo<AssemblySymbol>(new CSDiagnosticInfo(ErrorCode.ERR_BogusType, string.Empty));
		GetSymbolSpecificUnsupportedMetadataUseSiteErrorInfo(ref info);
		return info;
	}

	internal bool DeriveUseSiteInfoFromType(ref UseSiteInfo<AssemblySymbol> result, TypeWithAnnotations type, AllowedRequiredModifierType allowedRequiredModifierType)
	{
		if (!DeriveUseSiteInfoFromType(ref result, type.Type))
		{
			return DeriveUseSiteInfoFromCustomModifiers(ref result, type.CustomModifiers, allowedRequiredModifierType);
		}
		return true;
	}

	internal bool DeriveUseSiteInfoFromParameter(ref UseSiteInfo<AssemblySymbol> result, ParameterSymbol param)
	{
		if (!DeriveUseSiteInfoFromType(ref result, param.TypeWithAnnotations, AllowedRequiredModifierType.None))
		{
			return DeriveUseSiteInfoFromCustomModifiers(ref result, param.RefCustomModifiers, (this is MethodSymbol { MethodKind: MethodKind.FunctionPointerSignature }) ? (AllowedRequiredModifierType.System_Runtime_InteropServices_InAttribute | AllowedRequiredModifierType.System_Runtime_InteropServices_OutAttribute) : AllowedRequiredModifierType.System_Runtime_InteropServices_InAttribute);
		}
		return true;
	}

	internal bool DeriveUseSiteInfoFromParameters(ref UseSiteInfo<AssemblySymbol> result, ImmutableArray<ParameterSymbol> parameters)
	{
		foreach (ParameterSymbol item in parameters)
		{
			if (DeriveUseSiteInfoFromParameter(ref result, item))
			{
				return true;
			}
		}
		return false;
	}

	internal bool DeriveUseSiteInfoFromCustomModifiers(ref UseSiteInfo<AssemblySymbol> result, ImmutableArray<CustomModifier> customModifiers, AllowedRequiredModifierType allowedRequiredModifierType)
	{
		AllowedRequiredModifierType allowedRequiredModifierType2 = AllowedRequiredModifierType.None;
		bool flag = true;
		foreach (CustomModifier item in customModifiers)
		{
			NamedTypeSymbol namedTypeSymbol = ((CSharpCustomModifier)item).ModifierSymbol;
			if (flag && !item.IsOptional)
			{
				AllowedRequiredModifierType allowedRequiredModifierType3 = AllowedRequiredModifierType.None;
				if ((allowedRequiredModifierType & AllowedRequiredModifierType.System_Runtime_InteropServices_InAttribute) != AllowedRequiredModifierType.None && namedTypeSymbol.IsWellKnownTypeInAttribute())
				{
					allowedRequiredModifierType3 = AllowedRequiredModifierType.System_Runtime_InteropServices_InAttribute;
				}
				else if ((allowedRequiredModifierType & AllowedRequiredModifierType.System_Runtime_CompilerServices_Volatile) != AllowedRequiredModifierType.None && namedTypeSymbol.SpecialType == SpecialType.System_Runtime_CompilerServices_IsVolatile)
				{
					allowedRequiredModifierType3 = AllowedRequiredModifierType.System_Runtime_CompilerServices_Volatile;
				}
				else if ((allowedRequiredModifierType & AllowedRequiredModifierType.System_Runtime_CompilerServices_IsExternalInit) != AllowedRequiredModifierType.None && namedTypeSymbol.IsWellKnownTypeIsExternalInit())
				{
					allowedRequiredModifierType3 = AllowedRequiredModifierType.System_Runtime_CompilerServices_IsExternalInit;
				}
				else if ((allowedRequiredModifierType & AllowedRequiredModifierType.System_Runtime_InteropServices_OutAttribute) != AllowedRequiredModifierType.None && namedTypeSymbol.IsWellKnownTypeOutAttribute())
				{
					allowedRequiredModifierType3 = AllowedRequiredModifierType.System_Runtime_InteropServices_OutAttribute;
				}
				if (allowedRequiredModifierType3 == AllowedRequiredModifierType.None || (allowedRequiredModifierType3 != allowedRequiredModifierType2 && allowedRequiredModifierType2 != AllowedRequiredModifierType.None))
				{
					if (MergeUseSiteInfo(ref result, GetSymbolSpecificUnsupportedMetadataUseSiteErrorInfo()))
					{
						return true;
					}
					flag = false;
				}
				allowedRequiredModifierType2 |= allowedRequiredModifierType3;
			}
			if (namedTypeSymbol.IsUnboundGenericType)
			{
				namedTypeSymbol = namedTypeSymbol.OriginalDefinition;
			}
			if (DeriveUseSiteInfoFromType(ref result, namedTypeSymbol))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool GetUnificationUseSiteDiagnosticRecursive<T>(ref DiagnosticInfo result, ImmutableArray<T> types, Symbol owner, ref HashSet<TypeSymbol> checkedTypes) where T : TypeSymbol
	{
		foreach (T item in types)
		{
			if (item.GetUnificationUseSiteDiagnosticRecursive(ref result, owner, ref checkedTypes))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, ImmutableArray<TypeWithAnnotations> types, Symbol owner, ref HashSet<TypeSymbol> checkedTypes)
	{
		foreach (TypeWithAnnotations item in types)
		{
			if (item.GetUnificationUseSiteDiagnosticRecursive(ref result, owner, ref checkedTypes))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, ImmutableArray<CustomModifier> modifiers, Symbol owner, ref HashSet<TypeSymbol> checkedTypes)
	{
		foreach (CSharpCustomModifier item in modifiers)
		{
			if (item.ModifierSymbol.GetUnificationUseSiteDiagnosticRecursive(ref result, owner, ref checkedTypes))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, ImmutableArray<ParameterSymbol> parameters, Symbol owner, ref HashSet<TypeSymbol> checkedTypes)
	{
		foreach (ParameterSymbol item in parameters)
		{
			if (item.TypeWithAnnotations.GetUnificationUseSiteDiagnosticRecursive(ref result, owner, ref checkedTypes) || GetUnificationUseSiteDiagnosticRecursive(ref result, item.RefCustomModifiers, owner, ref checkedTypes))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, ImmutableArray<TypeParameterSymbol> typeParameters, Symbol owner, ref HashSet<TypeSymbol> checkedTypes)
	{
		foreach (TypeParameterSymbol item in typeParameters)
		{
			if (GetUnificationUseSiteDiagnosticRecursive(ref result, item.ConstraintTypesNoUseSiteDiagnostics, owner, ref checkedTypes))
			{
				return true;
			}
		}
		return false;
	}

	public string ToDisplayString(SymbolDisplayFormat format = null)
	{
		return SymbolDisplay.ToDisplayString(ISymbol, format);
	}

	public ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat format = null)
	{
		return SymbolDisplay.ToDisplayParts(ISymbol, format);
	}

	public string ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
	{
		return SymbolDisplay.ToMinimalDisplayString(ISymbol, semanticModel, position, format);
	}

	public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
	{
		return SymbolDisplay.ToMinimalDisplayParts(ISymbol, semanticModel, position, format);
	}

	internal static void ReportErrorIfHasConstraints(SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, DiagnosticBag diagnostics)
	{
		if (constraintClauses.Count > 0)
		{
			diagnostics.Add(ErrorCode.ERR_ConstraintOnlyAllowedOnGenericDecl, constraintClauses[0].WhereKeyword.GetLocation());
		}
	}

	internal static void CheckForBlockAndExpressionBody(CSharpSyntaxNode block, CSharpSyntaxNode expression, CSharpSyntaxNode syntax, BindingDiagnosticBag diagnostics)
	{
		if (block != null && expression != null)
		{
			diagnostics.Add(ErrorCode.ERR_BlockBodyAndExpressionBody, syntax.GetLocation());
		}
	}

	internal bool ReportExplicitUseOfReservedAttributes(in DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments, ReservedAttributes reserved)
	{
		CSharpAttributeData attribute = arguments.Attribute;
		BindingDiagnosticBag diagnostics = (BindingDiagnosticBag)arguments.Diagnostics;
		if ((reserved & ReservedAttributes.DynamicAttribute) != 0 && attribute.IsTargetAttribute(AttributeDescription.DynamicAttribute))
		{
			diagnostics.Add(ErrorCode.ERR_ExplicitDynamicAttr, arguments.AttributeSyntaxOpt.Location);
		}
		else if (((reserved & ReservedAttributes.IsReadOnlyAttribute) == 0 || !reportExplicitUseOfReservedAttribute(attribute, in arguments, in AttributeDescription.IsReadOnlyAttribute)) && ((reserved & ReservedAttributes.RequiresLocationAttribute) == 0 || !reportExplicitUseOfReservedAttribute(attribute, in arguments, in AttributeDescription.RequiresLocationAttribute)) && ((reserved & ReservedAttributes.IsUnmanagedAttribute) == 0 || !reportExplicitUseOfReservedAttribute(attribute, in arguments, in AttributeDescription.IsUnmanagedAttribute)) && ((reserved & ReservedAttributes.IsByRefLikeAttribute) == 0 || !reportExplicitUseOfReservedAttribute(attribute, in arguments, in AttributeDescription.IsByRefLikeAttribute)))
		{
			if ((reserved & ReservedAttributes.TupleElementNamesAttribute) != 0 && attribute.IsTargetAttribute(AttributeDescription.TupleElementNamesAttribute))
			{
				diagnostics.Add(ErrorCode.ERR_ExplicitTupleElementNamesAttribute, arguments.AttributeSyntaxOpt.Location);
			}
			else if ((reserved & ReservedAttributes.NullableAttribute) != 0 && attribute.IsTargetAttribute(AttributeDescription.NullableAttribute))
			{
				diagnostics.Add(ErrorCode.ERR_ExplicitNullableAttribute, arguments.AttributeSyntaxOpt.Location);
			}
			else if (((reserved & ReservedAttributes.NullableContextAttribute) == 0 || !reportExplicitUseOfReservedAttribute(attribute, in arguments, in AttributeDescription.NullableContextAttribute)) && ((reserved & ReservedAttributes.NullablePublicOnlyAttribute) == 0 || !reportExplicitUseOfReservedAttribute(attribute, in arguments, in AttributeDescription.NullablePublicOnlyAttribute)) && ((reserved & ReservedAttributes.NativeIntegerAttribute) == 0 || !reportExplicitUseOfReservedAttribute(attribute, in arguments, in AttributeDescription.NativeIntegerAttribute)))
			{
				if ((reserved & ReservedAttributes.CaseSensitiveExtensionAttribute) != 0 && attribute.IsTargetAttribute(AttributeDescription.CaseSensitiveExtensionAttribute))
				{
					diagnostics.Add(ErrorCode.ERR_ExplicitExtension, arguments.AttributeSyntaxOpt.Location);
				}
				else if ((reserved & ReservedAttributes.RequiredMemberAttribute) != 0 && attribute.IsTargetAttribute(AttributeDescription.RequiredMemberAttribute))
				{
					diagnostics.Add(ErrorCode.ERR_ExplicitRequiredMember, arguments.AttributeSyntaxOpt.Location);
				}
				else if ((reserved & ReservedAttributes.ScopedRefAttribute) != 0 && attribute.IsTargetAttribute(AttributeDescription.ScopedRefAttribute))
				{
					diagnostics.Add(ErrorCode.ERR_ExplicitScopedRef, arguments.AttributeSyntaxOpt.Location);
				}
				else if (((reserved & ReservedAttributes.RefSafetyRulesAttribute) == 0 || !reportExplicitUseOfReservedAttribute(attribute, in arguments, in AttributeDescription.RefSafetyRulesAttribute)) && ((reserved & ReservedAttributes.ExtensionMarkerAttribute) == 0 || !reportExplicitUseOfReservedAttribute(attribute, in arguments, in AttributeDescription.ExtensionMarkerAttribute)))
				{
					return false;
				}
			}
		}
		return true;
		bool reportExplicitUseOfReservedAttribute(CSharpAttributeData cSharpAttributeData, in DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> reference, in AttributeDescription attributeDescription)
		{
			if (cSharpAttributeData.IsTargetAttribute(attributeDescription))
			{
				diagnostics.Add(ErrorCode.ERR_ExplicitReservedAttr, reference.AttributeSyntaxOpt.Location, attributeDescription.FullName);
				return true;
			}
			return false;
		}
	}

	internal virtual byte? GetNullableContextValue()
	{
		return GetLocalNullableContextValue() ?? ContainingSymbol?.GetNullableContextValue();
	}

	internal virtual byte? GetLocalNullableContextValue()
	{
		return null;
	}

	internal void GetCommonNullableValues(CSharpCompilation compilation, ref MostCommonNullableValueBuilder builder)
	{
		switch (Kind)
		{
		case SymbolKind.NamedType:
			if (compilation.ShouldEmitNullableAttributes(this))
			{
				builder.AddValue(GetLocalNullableContextValue());
			}
			break;
		case SymbolKind.Event:
			if (compilation.ShouldEmitNullableAttributes(this))
			{
				builder.AddValue(((EventSymbol)this).TypeWithAnnotations);
			}
			break;
		case SymbolKind.Field:
		{
			FieldSymbol fieldSymbol = (FieldSymbol)this;
			if (fieldSymbol is TupleElementFieldSymbol tupleElementFieldSymbol)
			{
				fieldSymbol = tupleElementFieldSymbol.TupleUnderlyingField;
			}
			if (compilation.ShouldEmitNullableAttributes(fieldSymbol))
			{
				builder.AddValue(fieldSymbol.TypeWithAnnotations);
			}
			break;
		}
		case SymbolKind.Method:
			if (compilation.ShouldEmitNullableAttributes(this))
			{
				builder.AddValue(GetLocalNullableContextValue());
			}
			break;
		case SymbolKind.Property:
			if (compilation.ShouldEmitNullableAttributes(this))
			{
				builder.AddValue(((PropertySymbol)this).TypeWithAnnotations);
			}
			break;
		case SymbolKind.Parameter:
			builder.AddValue(((ParameterSymbol)this).TypeWithAnnotations);
			break;
		case SymbolKind.TypeParameter:
			if (this is SourceTypeParameterSymbol sourceTypeParameterSymbol)
			{
				builder.AddValue(sourceTypeParameterSymbol.GetSynthesizedNullableAttributeValue());
				foreach (TypeWithAnnotations constraintTypesNoUseSiteDiagnostic in sourceTypeParameterSymbol.ConstraintTypesNoUseSiteDiagnostics)
				{
					builder.AddValue(constraintTypesNoUseSiteDiagnostic);
				}
			}
			break;
		case SymbolKind.Label:
		case SymbolKind.Local:
		case SymbolKind.NetModule:
		case SymbolKind.Namespace:
		case SymbolKind.PointerType:
		case SymbolKind.RangeVariable:
			break;
		}
	}

	internal bool ShouldEmitNullableContextValue(out byte value)
	{
		byte? localNullableContextValue = GetLocalNullableContextValue();
		if (!localNullableContextValue.HasValue)
		{
			value = 0;
			return false;
		}
		value = localNullableContextValue.GetValueOrDefault();
		byte valueOrDefault = (ContainingSymbol?.GetNullableContextValue()).GetValueOrDefault();
		return value != valueOrDefault;
	}

	internal static bool IsCaptured(Symbol variable, SourceMethodSymbol containingSymbol)
	{
		switch (variable.Kind)
		{
		case SymbolKind.Event:
		case SymbolKind.Field:
		case SymbolKind.Property:
		case SymbolKind.RangeVariable:
			return false;
		case SymbolKind.Local:
			if (((LocalSymbol)variable).IsConst)
			{
				return false;
			}
			break;
		case SymbolKind.Method:
			if (variable is LocalFunctionSymbol localFunctionSymbol)
			{
				if (localFunctionSymbol.IsStatic)
				{
					return false;
				}
				break;
			}
			throw ExceptionUtilities.UnexpectedValue(variable);
		default:
			throw ExceptionUtilities.UnexpectedValue(variable.Kind);
		case SymbolKind.Parameter:
			break;
		}
		if (containingSymbol.IsExtensionBlockMember() && variable is ParameterSymbol && variable.ContainingSymbol is NamedTypeSymbol { IsExtension: not false })
		{
			return false;
		}
		Symbol containingSymbol2 = variable.ContainingSymbol;
		while ((object)containingSymbol2 != null)
		{
			if ((object)containingSymbol2 == containingSymbol)
			{
				return false;
			}
			containingSymbol2 = containingSymbol2.ContainingSymbol;
		}
		return true;
	}

	public abstract void Accept(CSharpSymbolVisitor visitor);

	public abstract TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor);

	string IFormattable.ToString(string format, IFormatProvider formatProvider)
	{
		return ToString();
	}

	protected abstract ISymbol CreateISymbol();

	public virtual ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return ImmutableArray<CSharpAttributeData>.Empty;
	}

	internal virtual AttributeTargets GetAttributeTarget()
	{
		switch (Kind)
		{
		case SymbolKind.Assembly:
			return AttributeTargets.Assembly;
		case SymbolKind.Field:
			return AttributeTargets.Field;
		case SymbolKind.Method:
		{
			MethodKind methodKind = ((MethodSymbol)this).MethodKind;
			if (methodKind == MethodKind.Constructor || methodKind == MethodKind.StaticConstructor)
			{
				return AttributeTargets.Constructor;
			}
			return AttributeTargets.Method;
		}
		case SymbolKind.NamedType:
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)this;
			switch (namedTypeSymbol.TypeKind)
			{
			case TypeKind.Class:
				return AttributeTargets.Class;
			case TypeKind.Delegate:
				return AttributeTargets.Delegate;
			case TypeKind.Enum:
				return AttributeTargets.Enum;
			case TypeKind.Interface:
				return AttributeTargets.Interface;
			case TypeKind.Struct:
				return AttributeTargets.Struct;
			case TypeKind.TypeParameter:
				return AttributeTargets.GenericParameter;
			case TypeKind.Submission:
				throw ExceptionUtilities.UnexpectedValue(namedTypeSymbol.TypeKind);
			}
			break;
		}
		case SymbolKind.NetModule:
			return AttributeTargets.Module;
		case SymbolKind.Parameter:
			return AttributeTargets.Parameter;
		case SymbolKind.Property:
			return AttributeTargets.Property;
		case SymbolKind.Event:
			return AttributeTargets.Event;
		case SymbolKind.TypeParameter:
			return AttributeTargets.GenericParameter;
		}
		return (AttributeTargets)0;
	}

	internal virtual void EarlyDecodeWellKnownAttributeType(NamedTypeSymbol attributeType, AttributeSyntax attributeSyntax)
	{
	}

	internal virtual void PostEarlyDecodeWellKnownAttributeTypes()
	{
	}

	internal virtual (CSharpAttributeData?, BoundAttribute?) EarlyDecodeWellKnownAttribute(ref EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation> arguments)
	{
		return (null, null);
	}

	internal static bool EarlyDecodeDeprecatedOrExperimentalOrObsoleteAttribute(ref EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation> arguments, out CSharpAttributeData? attributeData, out BoundAttribute? boundAttribute, out ObsoleteAttributeData? obsoleteData)
	{
		NamedTypeSymbol attributeType = arguments.AttributeType;
		AttributeSyntax attributeSyntax = arguments.AttributeSyntax;
		ObsoleteAttributeKind kind;
		if (CSharpAttributeData.IsTargetEarlyAttribute(attributeType, attributeSyntax, AttributeDescription.ObsoleteAttribute))
		{
			kind = ObsoleteAttributeKind.Obsolete;
		}
		else if (CSharpAttributeData.IsTargetEarlyAttribute(attributeType, attributeSyntax, AttributeDescription.DeprecatedAttribute))
		{
			kind = ObsoleteAttributeKind.Deprecated;
		}
		else if (CSharpAttributeData.IsTargetEarlyAttribute(attributeType, attributeSyntax, AttributeDescription.WindowsExperimentalAttribute))
		{
			kind = ObsoleteAttributeKind.WindowsExperimental;
		}
		else
		{
			if (!CSharpAttributeData.IsTargetEarlyAttribute(attributeType, attributeSyntax, AttributeDescription.ExperimentalAttribute))
			{
				obsoleteData = null;
				attributeData = null;
				boundAttribute = null;
				return false;
			}
			kind = ObsoleteAttributeKind.Experimental;
		}
		(attributeData, boundAttribute) = arguments.Binder.GetAttribute(attributeSyntax, attributeType, null, null, out var generatedDiagnostics);
		if (!attributeData.HasErrors)
		{
			obsoleteData = attributeData.DecodeObsoleteAttribute(kind);
			if (generatedDiagnostics)
			{
				attributeData = null;
				boundAttribute = null;
			}
		}
		else
		{
			obsoleteData = null;
			attributeData = null;
			boundAttribute = null;
		}
		return true;
	}

	protected void DecodeWellKnownAttribute(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		if (arguments.Attribute.IsTargetAttribute(AttributeDescription.CompilerFeatureRequiredAttribute))
		{
			arguments.Diagnostics.DiagnosticBag.Add(ErrorCode.ERR_ExplicitReservedAttr, arguments.AttributeSyntaxOpt.Location, AttributeDescription.CompilerFeatureRequiredAttribute.FullName);
		}
		else if (arguments.Attribute.IsTargetAttribute(AttributeDescription.ExperimentalAttribute))
		{
			if (!SyntaxFacts.IsValidIdentifier((string)arguments.Attribute.CommonConstructorArguments[0].ValueInternal))
			{
				Location attributeArgumentLocation = arguments.Attribute.GetAttributeArgumentLocation(0);
				arguments.Diagnostics.DiagnosticBag.Add(ErrorCode.ERR_InvalidExperimentalDiagID, attributeArgumentLocation);
			}
		}
		else if (arguments.Attribute.IsTargetAttribute(AttributeDescription.MetadataUpdateDeletedAttribute))
		{
			arguments.Diagnostics.DiagnosticBag.Add(ErrorCode.ERR_AttributeCannotBeAppliedManually, arguments.AttributeSyntaxOpt.Location, AttributeDescription.MetadataUpdateDeletedAttribute.FullName);
		}
		DecodeWellKnownAttributeImpl(ref arguments);
	}

	protected virtual void DecodeWellKnownAttributeImpl(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
	}

	internal virtual void PostDecodeWellKnownAttributes(ImmutableArray<CSharpAttributeData> boundAttributes, ImmutableArray<AttributeSyntax> allAttributeSyntaxNodes, BindingDiagnosticBag diagnostics, AttributeLocation symbolPart, WellKnownAttributeData decodedData)
	{
	}

	internal bool LoadAndValidateAttributes(OneOrMany<SyntaxList<AttributeListSyntax>> attributesSyntaxLists, ref CustomAttributesBag<CSharpAttributeData>? lazyCustomAttributesBag, AttributeLocation symbolPart = AttributeLocation.None, bool earlyDecodingOnly = false, Binder? binderOpt = null, Func<AttributeSyntax, Binder?, bool>? attributeMatchesOpt = null, Action<AttributeSyntax>? beforeAttributePartBound = null, Action<AttributeSyntax>? afterAttributePartBound = null)
	{
		BindingDiagnosticBag diagnostics = BindingDiagnosticBag.GetInstance();
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		ImmutableArray<AttributeSyntax> attributesToBind = GetAttributesToBind(attributesSyntaxLists, symbolPart, diagnostics, declaringCompilation, attributeMatchesOpt, binderOpt, out var binders);
		int length = attributesToBind.Length;
		BoundAttribute[] array3;
		ImmutableArray<CSharpAttributeData> immutableArray2;
		WellKnownAttributeData wellKnownAttributeData;
		if (length != 0)
		{
			if (lazyCustomAttributesBag == null)
			{
				Interlocked.CompareExchange(ref lazyCustomAttributesBag, new CustomAttributesBag<CSharpAttributeData>(), null);
			}
			NamedTypeSymbol[] array = new NamedTypeSymbol[length];
			Binder.BindAttributeTypes(binders, attributesToBind, this, array, beforeAttributePartBound, afterAttributePartBound, diagnostics);
			bool flag = !earlyDecodingOnly && attributeMatchesOpt == null;
			if (flag)
			{
				for (int i = 0; i < length; i++)
				{
					if (array[i].IsGenericType)
					{
						MessageID.IDS_FeatureGenericAttributes.CheckFeatureAvailability(diagnostics, attributesToBind[i]);
					}
				}
			}
			ImmutableArray<NamedTypeSymbol> immutableArray = array.AsImmutableOrNull();
			EarlyDecodeWellKnownAttributeTypes(immutableArray, attributesToBind);
			PostEarlyDecodeWellKnownAttributeTypes();
			CSharpAttributeData[] array2 = new CSharpAttributeData[length];
			array3 = (flag ? new BoundAttribute[length] : null);
			EarlyWellKnownAttributeData earlyDecodedWellKnownAttributeData = EarlyDecodeWellKnownAttributes(binders, immutableArray, attributesToBind, symbolPart, array2, array3);
			lazyCustomAttributesBag.SetEarlyDecodedWellKnownAttributeData(earlyDecodedWellKnownAttributeData);
			if (earlyDecodingOnly)
			{
				diagnostics.Free();
				return false;
			}
			Binder.GetAttributes(binders, attributesToBind, immutableArray, array2, array3, beforeAttributePartBound, afterAttributePartBound, diagnostics);
			immutableArray2 = array2.AsImmutableOrNull();
			wellKnownAttributeData = ValidateAttributeUsageAndDecodeWellKnownAttributes(binders, attributesToBind, immutableArray2, diagnostics, symbolPart);
			lazyCustomAttributesBag.SetDecodedWellKnownAttributeData(wellKnownAttributeData);
		}
		else
		{
			if (earlyDecodingOnly)
			{
				diagnostics.Free();
				return false;
			}
			immutableArray2 = ImmutableArray<CSharpAttributeData>.Empty;
			array3 = null;
			wellKnownAttributeData = null;
			Interlocked.CompareExchange(ref lazyCustomAttributesBag, CustomAttributesBag<CSharpAttributeData>.WithEmptyData(), null);
			PostEarlyDecodeWellKnownAttributeTypes();
		}
		bool result = false;
		if (lazyCustomAttributesBag.SetAttributes(immutableArray2))
		{
			if (attributeMatchesOpt == null)
			{
				PostDecodeWellKnownAttributes(immutableArray2, attributesToBind, diagnostics, symbolPart, wellKnownAttributeData);
				removeObsoleteDiagnosticsForForwardedTypes(immutableArray2, attributesToBind, ref diagnostics);
				RecordPresenceOfBadAttributes(immutableArray2);
				if (length != 0)
				{
					for (int j = 0; j < length; j++)
					{
						BoundAttribute boundAttribute = array3[j];
						Binder binder = binders[j];
						MethodSymbol constructor = boundAttribute.Constructor;
						if ((object)constructor != null)
						{
							Binder.CheckRequiredMembersInObjectInitializer(constructor, ImmutableArray<BoundExpression>.CastUp(boundAttribute.NamedArguments), boundAttribute.Syntax, diagnostics);
							binder.ReportDiagnosticsIfObsolete(diagnostics, constructor, boundAttribute.Syntax, hasBaseReceiver: false);
						}
						NullableWalker.AnalyzeIfNeeded(binder, boundAttribute, boundAttribute.Syntax, diagnostics.DiagnosticBag);
					}
				}
				AddDeclarationDiagnostics(diagnostics);
			}
			result = true;
			if (lazyCustomAttributesBag.IsEmpty)
			{
				lazyCustomAttributesBag = CustomAttributesBag<CSharpAttributeData>.Empty;
			}
		}
		diagnostics.Free();
		return result;
		static bool isObsoleteDiagnostic(DiagnosticWithInfo d)
		{
			if (!d.HasLazyInfo)
			{
				return d.Info.IsObsoleteDiagnostic();
			}
			return d.LazyInfo is LazyObsoleteDiagnosticInfo;
		}
		void removeObsoleteDiagnosticsForForwardedTypes(ImmutableArray<CSharpAttributeData> boundAttributes, ImmutableArray<AttributeSyntax> immutableArray3, ref BindingDiagnosticBag reference)
		{
			if (!boundAttributes.IsDefaultOrEmpty && this is SourceAssemblySymbol && !reference.DiagnosticBag.IsEmptyWithoutResolution && reference.DiagnosticBag.AsEnumerableWithoutResolution().OfType<DiagnosticWithInfo>().Where(isObsoleteDiagnostic)
				.Any())
			{
				ArrayBuilder<Location> instance = ArrayBuilder<Location>.GetInstance();
				int length2 = immutableArray3.Length;
				for (int k = 0; k < length2; k++)
				{
					CSharpAttributeData cSharpAttributeData = boundAttributes[k];
					if (!cSharpAttributeData.HasErrors && cSharpAttributeData.IsTargetAttribute(AttributeDescription.TypeForwardedToAttribute) && cSharpAttributeData.CommonConstructorArguments[0].ValueInternal is TypeSymbol)
					{
						Location location = immutableArray3[k].ArgumentList?.Arguments[0].Expression.Location;
						if ((object)location != null)
						{
							instance.Add(location);
						}
					}
				}
				if (instance.Count != 0)
				{
					HashSet<Diagnostic> hashSet = new HashSet<Diagnostic>(ReferenceEqualityComparer.Instance);
					foreach (Diagnostic item in reference.DiagnosticBag.AsEnumerableWithoutResolution())
					{
						if (item is DiagnosticWithInfo diagnosticWithInfo && isObsoleteDiagnostic(diagnosticWithInfo))
						{
							Location location2 = diagnosticWithInfo.Location;
							foreach (Location item2 in instance)
							{
								if (location2.SourceTree == item2.SourceTree && item2.SourceSpan.Contains(location2.SourceSpan))
								{
									hashSet.Add(diagnosticWithInfo);
									break;
								}
							}
						}
					}
					if (hashSet.Count != 0)
					{
						BindingDiagnosticBag instance2 = BindingDiagnosticBag.GetInstance();
						instance2.AddDependencies(reference);
						foreach (Diagnostic item3 in reference.DiagnosticBag.AsEnumerableWithoutResolution())
						{
							if (!hashSet.Contains(item3))
							{
								instance2.Add(item3);
							}
						}
						reference.Free();
						reference = instance2;
					}
				}
				instance.Free();
			}
		}
	}

	protected ImmutableArray<(CSharpAttributeData, BoundAttribute)> BindAttributes(OneOrMany<SyntaxList<AttributeListSyntax>> attributeDeclarations, Binder? rootBinder)
	{
		ArrayBuilder<(CSharpAttributeData, BoundAttribute)> instance = ArrayBuilder<(CSharpAttributeData, BoundAttribute)>.GetInstance();
		foreach (SyntaxList<AttributeListSyntax> item in attributeDeclarations)
		{
			Binder attributeBinder = GetAttributeBinder(item, DeclaringCompilation, rootBinder);
			foreach (AttributeListSyntax item2 in item)
			{
				foreach (AttributeSyntax attribute2 in item2.Attributes)
				{
					NamedTypeSymbol boundAttributeType = (NamedTypeSymbol)attributeBinder.BindType(attribute2.Name, BindingDiagnosticBag.Discarded).Type;
					(CSharpAttributeData, BoundAttribute) attribute = attributeBinder.GetAttribute(attribute2, boundAttributeType, null, null, BindingDiagnosticBag.Discarded);
					instance.Add(attribute);
				}
			}
		}
		return instance.ToImmutableAndFree();
	}

	private void RecordPresenceOfBadAttributes(ImmutableArray<CSharpAttributeData> boundAttributes)
	{
		foreach (CSharpAttributeData item in boundAttributes)
		{
			if (item.HasErrors)
			{
				((SourceModuleSymbol)DeclaringCompilation.SourceModule).RecordPresenceOfBadAttributes();
				break;
			}
		}
	}

	private ImmutableArray<AttributeSyntax> GetAttributesToBind(OneOrMany<SyntaxList<AttributeListSyntax>> attributeDeclarationSyntaxLists, AttributeLocation symbolPart, BindingDiagnosticBag diagnostics, CSharpCompilation compilation, Func<AttributeSyntax, Binder, bool> attributeMatchesOpt, Binder rootBinderOpt, out ImmutableArray<Binder> binders)
	{
		IAttributeTargetSymbol attributeTarget = (IAttributeTargetSymbol)this;
		ArrayBuilder<AttributeSyntax> arrayBuilder = null;
		ArrayBuilder<Binder> arrayBuilder2 = null;
		int num = 0;
		for (int i = 0; i < attributeDeclarationSyntaxLists.Count; i++)
		{
			SyntaxList<AttributeListSyntax> attributeDeclarationSyntaxList = attributeDeclarationSyntaxLists[i];
			if (!attributeDeclarationSyntaxList.Any())
			{
				continue;
			}
			int num2 = num;
			foreach (AttributeListSyntax item in attributeDeclarationSyntaxList)
			{
				if (!MatchAttributeTarget(attributeTarget, symbolPart, item.Target, diagnostics) || !ShouldBindAttributes(item, diagnostics))
				{
					continue;
				}
				if (arrayBuilder == null)
				{
					arrayBuilder = new ArrayBuilder<AttributeSyntax>();
					arrayBuilder2 = new ArrayBuilder<Binder>();
				}
				SeparatedSyntaxList<AttributeSyntax> attributes = item.Attributes;
				if (attributeMatchesOpt == null)
				{
					arrayBuilder.AddRange(attributes);
					num += attributes.Count;
					continue;
				}
				foreach (AttributeSyntax item2 in attributes)
				{
					if (attributeMatchesOpt(item2, rootBinderOpt))
					{
						arrayBuilder.Add(item2);
						num++;
					}
				}
			}
			if (num != num2)
			{
				Binder attributeBinder = GetAttributeBinder(attributeDeclarationSyntaxList, compilation, rootBinderOpt);
				for (int j = 0; j < num - num2; j++)
				{
					arrayBuilder2.Add(attributeBinder);
				}
			}
		}
		if (arrayBuilder != null)
		{
			binders = arrayBuilder2.ToImmutableAndFree();
			return arrayBuilder.ToImmutableAndFree();
		}
		binders = ImmutableArray<Binder>.Empty;
		return ImmutableArray<AttributeSyntax>.Empty;
	}

	protected virtual bool ShouldBindAttributes(AttributeListSyntax attributeDeclarationSyntax, BindingDiagnosticBag diagnostics)
	{
		return true;
	}

	protected Binder GetAttributeBinder(SyntaxList<AttributeListSyntax> attributeDeclarationSyntaxList, CSharpCompilation compilation, Binder? rootBinder = null)
	{
		return new ContextualAttributeBinder(rootBinder ?? compilation.GetBinderFactory(attributeDeclarationSyntaxList.Node.SyntaxTree).GetBinder(attributeDeclarationSyntaxList.Node), this);
	}

	private static bool MatchAttributeTarget(IAttributeTargetSymbol attributeTarget, AttributeLocation symbolPart, AttributeTargetSpecifierSyntax targetOpt, BindingDiagnosticBag diagnostics)
	{
		IAttributeTargetSymbol attributesOwner = attributeTarget.AttributesOwner;
		bool flag = symbolPart == AttributeLocation.None && attributesOwner == attributeTarget;
		if (targetOpt == null)
		{
			return flag;
		}
		if (flag && targetOpt.Identifier.ToAttributeLocation() == AttributeLocation.Module && ((CSharpParseOptions)targetOpt.SyntaxTree.Options).LanguageVersion == LanguageVersion.CSharp1)
		{
			diagnostics.Add(ErrorCode.WRN_NonECMAFeature, targetOpt.GetLocation(), MessageID.IDS_FeatureModuleAttrLoc);
		}
		AttributeLocation allowedAttributeLocations = attributesOwner.AllowedAttributeLocations;
		AttributeLocation attributeLocation = targetOpt.GetAttributeLocation();
		if (attributeLocation == AttributeLocation.None)
		{
			if (flag)
			{
				diagnostics.Add(ErrorCode.WRN_InvalidAttributeLocation, targetOpt.Identifier.GetLocation(), targetOpt.Identifier.ValueText, allowedAttributeLocations.ToDisplayString());
			}
			return false;
		}
		if ((attributeLocation & allowedAttributeLocations) == 0)
		{
			if (flag)
			{
				if (allowedAttributeLocations == AttributeLocation.None)
				{
					AttributeLocation defaultAttributeLocation = attributeTarget.DefaultAttributeLocation;
					if ((uint)(defaultAttributeLocation - 1) > 1u)
					{
						throw ExceptionUtilities.UnexpectedValue(attributeTarget.DefaultAttributeLocation);
					}
					diagnostics.Add(ErrorCode.ERR_GlobalAttributesNotAllowed, targetOpt.Identifier.GetLocation());
				}
				else
				{
					diagnostics.Add(ErrorCode.WRN_AttributeLocationOnBadDeclaration, targetOpt.Identifier.GetLocation(), targetOpt.Identifier.ToString(), allowedAttributeLocations.ToDisplayString());
				}
			}
			return false;
		}
		if (symbolPart == AttributeLocation.None)
		{
			return attributeLocation == attributeTarget.DefaultAttributeLocation;
		}
		return attributeLocation == symbolPart;
	}

	internal EarlyWellKnownAttributeData? EarlyDecodeWellKnownAttributes(ImmutableArray<Binder> binders, ImmutableArray<NamedTypeSymbol> boundAttributeTypes, ImmutableArray<AttributeSyntax> attributesToBind, AttributeLocation symbolPart, CSharpAttributeData?[] attributeDataArray, BoundAttribute?[]? boundAttributeArray)
	{
		EarlyWellKnownAttributeBinder earlyWellKnownAttributeBinder = new EarlyWellKnownAttributeBinder(binders[0]);
		EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation> arguments = new EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation>
		{
			SymbolPart = symbolPart
		};
		for (int i = 0; i < boundAttributeTypes.Length; i++)
		{
			NamedTypeSymbol namedTypeSymbol = boundAttributeTypes[i];
			if (!namedTypeSymbol.IsErrorType())
			{
				if (binders[i] != earlyWellKnownAttributeBinder.Next)
				{
					earlyWellKnownAttributeBinder = new EarlyWellKnownAttributeBinder(binders[i]);
				}
				arguments.Binder = earlyWellKnownAttributeBinder;
				arguments.AttributeType = namedTypeSymbol;
				arguments.AttributeSyntax = attributesToBind[i];
				var (cSharpAttributeData, boundAttribute) = EarlyDecodeWellKnownAttribute(ref arguments);
				attributeDataArray[i] = cSharpAttributeData;
				if (boundAttributeArray != null)
				{
					boundAttributeArray[i] = boundAttribute;
				}
			}
		}
		if (!arguments.HasDecodedData)
		{
			return null;
		}
		return arguments.DecodedData;
	}

	private void EarlyDecodeWellKnownAttributeTypes(ImmutableArray<NamedTypeSymbol> attributeTypes, ImmutableArray<AttributeSyntax> attributeSyntaxList)
	{
		for (int i = 0; i < attributeTypes.Length; i++)
		{
			NamedTypeSymbol namedTypeSymbol = attributeTypes[i];
			if (!namedTypeSymbol.IsErrorType())
			{
				EarlyDecodeWellKnownAttributeType(namedTypeSymbol, attributeSyntaxList[i]);
			}
		}
	}

	private WellKnownAttributeData ValidateAttributeUsageAndDecodeWellKnownAttributes(ImmutableArray<Binder> binders, ImmutableArray<AttributeSyntax> attributeSyntaxList, ImmutableArray<CSharpAttributeData> boundAttributes, BindingDiagnosticBag diagnostics, AttributeLocation symbolPart)
	{
		int length = boundAttributes.Length;
		HashSet<NamedTypeSymbol> uniqueAttributeTypes = new HashSet<NamedTypeSymbol>();
		DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments = new DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation>
		{
			Diagnostics = diagnostics,
			AttributesCount = length,
			SymbolPart = symbolPart
		};
		for (int i = 0; i < length; i++)
		{
			CSharpAttributeData cSharpAttributeData = boundAttributes[i];
			AttributeSyntax attributeSyntax = attributeSyntaxList[i];
			Binder binder = binders[i];
			if (!cSharpAttributeData.HasErrors && ValidateAttributeUsage(cSharpAttributeData, attributeSyntax, binder.Compilation, symbolPart, diagnostics, uniqueAttributeTypes))
			{
				arguments.Attribute = cSharpAttributeData;
				arguments.AttributeSyntaxOpt = attributeSyntax;
				arguments.Index = i;
				DecodeWellKnownAttribute(ref arguments);
			}
		}
		if (!arguments.HasDecodedData)
		{
			return null;
		}
		return arguments.DecodedData;
	}

	private bool ValidateAttributeUsage(CSharpAttributeData attribute, AttributeSyntax node, CSharpCompilation compilation, AttributeLocation symbolPart, BindingDiagnosticBag diagnostics, HashSet<NamedTypeSymbol> uniqueAttributeTypes)
	{
		NamedTypeSymbol attributeClass = attribute.AttributeClass;
		AttributeUsageInfo attributeUsageInfo = attributeClass.GetAttributeUsageInfo();
		if (!uniqueAttributeTypes.Add(attributeClass.OriginalDefinition) && !attributeUsageInfo.AllowMultiple)
		{
			diagnostics.Add(ErrorCode.ERR_DuplicateAttribute, node.Name.Location, node.GetErrorDisplayName());
			return false;
		}
		AttributeTargets attributeTargets = ((symbolPart != AttributeLocation.Return) ? GetAttributeTarget() : AttributeTargets.ReturnValue);
		if ((attributeTargets & attributeUsageInfo.ValidTargets) == 0)
		{
			diagnostics.Add(ErrorCode.ERR_AttributeOnBadSymbolType, node.Name.Location, node.GetErrorDisplayName(), attributeUsageInfo.GetValidTargetsErrorArgument());
			return false;
		}
		if (attribute.IsSecurityAttribute(compilation))
		{
			SymbolKind kind = Kind;
			if (kind != SymbolKind.Assembly && kind != SymbolKind.Method && kind != SymbolKind.NamedType)
			{
				diagnostics.Add(ErrorCode.ERR_SecurityAttributeInvalidTarget, node.Name.Location, node.GetErrorDisplayName());
				return false;
			}
		}
		return true;
	}

	internal void ForceCompleteObsoleteAttribute()
	{
		if (ObsoleteKind == ObsoleteAttributeKind.Uninitialized)
		{
			GetAttributes();
		}
		ContainingSymbol?.ForceCompleteObsoleteAttribute();
	}
}
