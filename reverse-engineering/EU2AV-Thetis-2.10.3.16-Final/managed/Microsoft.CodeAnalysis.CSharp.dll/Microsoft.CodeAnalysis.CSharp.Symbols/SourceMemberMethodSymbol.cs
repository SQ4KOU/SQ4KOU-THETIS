using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceMemberMethodSymbol : LocalFunctionOrSourceMemberMethodSymbol, IAttributeTargetSymbol
{
	protected struct Flags
	{
		private int _flags;

		private const int MethodKindOffset = 0;

		private const int MethodKindSize = 5;

		private const int MethodKindMask = 31;

		private const int RefKindOffset = 5;

		private const int RefKindSize = 3;

		private const int RefKindMask = 7;

		private const int IsExtensionMethodOffset = 8;

		private const int IsExtensionMethodSize = 1;

		private const int IsMetadataVirtualIgnoringInterfaceChangesOffset = 9;

		private const int IsMetadataVirtualIgnoringInterfaceChangesSize = 1;

		private const int IsMetadataVirtualOffset = 10;

		private const int IsMetadataVirtualSize = 1;

		private const int IsMetadataVirtualLockedOffset = 11;

		private const int IsMetadataVirtualLockedSize = 1;

		private const int ReturnsVoidOffset = 12;

		private const int ReturnsVoidSize = 2;

		private const int NullableContextOffset = 14;

		private const int NullableContextSize = 3;

		private const int NullableContextMask = 7;

		private const int IsNullableAnalysisEnabledOffset = 17;

		private const int IsNullableAnalysisEnabledSize = 1;

		private const int IsExpressionBodiedOffset = 18;

		private const int IsExpressionBodiedSize = 1;

		private const int HasAnyBodyOffset = 19;

		private const int HasAnyBodySize = 1;

		private const int IsVarargOffset = 20;

		private const int IsVarargSize = 1;

		private const int HasThisInitializerOffset = 21;

		private const int HasThisInitializerSize = 1;

		private const int HasExplicitAccessModifierOffset = 22;

		private const int HasExplicitAccessModifierSize = 1;

		private const int HasAnyBodyBit = 524288;

		private const int IsExpressionBodiedBit = 262144;

		private const int IsExtensionMethodBit = 256;

		private const int IsMetadataVirtualIgnoringInterfaceChangesBit = 512;

		private const int IsMetadataVirtualBit = 512;

		private const int IsMetadataVirtualLockedBit = 2048;

		private const int IsVarargBit = 1048576;

		private const int HasThisInitializerBit = 2097152;

		private const int HasExplicitAccessModifierBit = 4194304;

		private const int ReturnsVoidBit = 4096;

		private const int ReturnsVoidIsSetBit = 8192;

		private const int IsNullableAnalysisEnabledBit = 131072;

		public bool ReturnsVoid => (_flags & 0x1000) != 0;

		public MethodKind MethodKind => (MethodKind)(_flags & 0x1F);

		public RefKind RefKind => (RefKind)((_flags >> 5) & 7);

		public bool HasAnyBody => (_flags & 0x80000) != 0;

		public bool IsExpressionBodied => (_flags & 0x40000) != 0;

		public bool IsExtensionMethod => (_flags & 0x100) != 0;

		public bool IsNullableAnalysisEnabled => (_flags & 0x20000) != 0;

		public bool IsMetadataVirtualLocked => (_flags & 0x800) != 0;

		public bool IsVararg => (_flags & 0x100000) != 0;

		public readonly bool HasThisInitializer => (_flags & 0x200000) != 0;

		public readonly bool HasExplicitAccessModifier => (_flags & 0x400000) != 0;

		public void SetReturnsVoid(bool value)
		{
			ThreadSafeFlagOperations.Set(ref _flags, 0x2000 | (value ? 4096 : 0));
		}

		private static bool ModifiersRequireMetadataVirtual(DeclarationModifiers modifiers)
		{
			return (modifiers & (DeclarationModifiers.Abstract | DeclarationModifiers.Virtual | DeclarationModifiers.Override)) != 0;
		}

		public Flags(MethodKind methodKind, RefKind refKind, DeclarationModifiers declarationModifiers, bool returnsVoid, bool returnsVoidIsSet, bool hasAnyBody, bool isExpressionBodied, bool isExtensionMethod, bool isNullableAnalysisEnabled, bool isVararg, bool isExplicitInterfaceImplementation, bool hasThisInitializer, bool hasExplicitAccessModifier)
		{
			bool num = (isExplicitInterfaceImplementation && (declarationModifiers & DeclarationModifiers.Static) == 0) || ModifiersRequireMetadataVirtual(declarationModifiers);
			int num2 = (int)(methodKind & (MethodKind)0x1F);
			int num3 = (int)((uint)(refKind & (RefKind)7) << 5);
			int num4 = (hasAnyBody ? 524288 : 0);
			int num5 = (isExpressionBodied ? 262144 : 0);
			int num6 = (isExtensionMethod ? 256 : 0);
			int num7 = (isNullableAnalysisEnabled ? 131072 : 0);
			int num8 = (isVararg ? 1048576 : 0);
			int num9 = (num ? 512 : 0);
			int num10 = (num ? 512 : 0);
			int num11 = (hasThisInitializer ? 2097152 : 0);
			int num12 = (hasExplicitAccessModifier ? 4194304 : 0);
			_flags = num2 | num3 | num4 | num5 | num6 | num7 | num8 | num9 | num10 | num11 | num12 | (returnsVoid ? 4096 : 0) | (returnsVoidIsSet ? 8192 : 0);
		}

		public Flags(MethodKind methodKind, RefKind refKind, DeclarationModifiers declarationModifiers, bool returnsVoid, bool returnsVoidIsSet, bool isExpressionBodied, bool isExtensionMethod, bool isNullableAnalysisEnabled, bool isVararg, bool isExplicitInterfaceImplementation, bool hasThisInitializer)
			: this(methodKind, refKind, declarationModifiers, returnsVoid, returnsVoidIsSet, hasAnyBody: false, isExpressionBodied, isExtensionMethod, isNullableAnalysisEnabled, isVararg, isExplicitInterfaceImplementation, hasThisInitializer, hasExplicitAccessModifier: false)
		{
		}

		public bool IsMetadataVirtual(bool ignoreInterfaceImplementationChanges = false)
		{
			if (ignoreInterfaceImplementationChanges)
			{
				return (_flags & 0x200) != 0;
			}
			if (!IsMetadataVirtualLocked)
			{
				ThreadSafeFlagOperations.Set(ref _flags, 2048);
			}
			return (_flags & 0x200) != 0;
		}

		public void EnsureMetadataVirtual()
		{
			if ((_flags & 0x200) == 0)
			{
				ThreadSafeFlagOperations.Set(ref _flags, 512);
			}
		}

		public bool TryGetNullableContext(out byte? value)
		{
			return ((NullableContextKind)((_flags >> 14) & 7)).TryGetByte(out value);
		}

		public bool SetNullableContext(byte? value)
		{
			return ThreadSafeFlagOperations.Set(ref _flags, (int)((uint)(value.ToNullableContextFlags() & (NullableContextKind)7) << 14));
		}
	}

	protected SymbolCompletionState state;

	protected readonly DeclarationModifiers DeclarationModifiers;

	protected Flags flags;

	private readonly NamedTypeSymbol _containingType;

	private ParameterSymbol _lazyThisParameter;

	private OverriddenOrHiddenMembersResult _lazyOverriddenOrHiddenMembers;

	protected readonly Location _location;

	protected string lazyDocComment;

	protected string lazyExpandedDocComment;

	private ImmutableArray<Diagnostic> _cachedDiagnostics;

	internal ImmutableArray<Diagnostic> Diagnostics => _cachedDiagnostics;

	protected virtual object MethodChecksLockObject => syntaxReferenceOpt;

	public sealed override Symbol ContainingSymbol => _containingType;

	public override NamedTypeSymbol ContainingType => _containingType;

	public override Symbol AssociatedSymbol => null;

	public override bool ReturnsVoid => flags.ReturnsVoid;

	public sealed override MethodKind MethodKind => flags.MethodKind;

	public sealed override bool IsExtensionMethod => flags.IsExtensionMethod;

	public override Accessibility DeclaredAccessibility => ModifierUtils.EffectiveAccessibility(DeclarationModifiers);

	internal bool HasExternModifier => (DeclarationModifiers & DeclarationModifiers.Extern) != 0;

	public override bool IsExtern => HasExternModifier;

	public sealed override bool IsSealed => (DeclarationModifiers & DeclarationModifiers.Sealed) != 0;

	public sealed override bool IsAbstract => (DeclarationModifiers & DeclarationModifiers.Abstract) != 0;

	public sealed override bool IsOverride => (DeclarationModifiers & DeclarationModifiers.Override) != 0;

	internal bool IsPartial => (DeclarationModifiers & DeclarationModifiers.Partial) != 0;

	public sealed override bool IsVirtual => (DeclarationModifiers & DeclarationModifiers.Virtual) != 0;

	internal bool IsNew => (DeclarationModifiers & DeclarationModifiers.New) != 0;

	public sealed override bool IsStatic => (DeclarationModifiers & DeclarationModifiers.Static) != 0;

	internal bool IsUnsafe => (DeclarationModifiers & DeclarationModifiers.Unsafe) != 0;

	public sealed override bool IsAsync => (DeclarationModifiers & DeclarationModifiers.Async) != 0;

	internal override bool IsDeclaredReadOnly => (DeclarationModifiers & DeclarationModifiers.ReadOnly) != 0;

	internal override bool IsInitOnly => false;

	internal sealed override CallingConvention CallingConvention
	{
		get
		{
			CallingConvention callingConvention = (IsVararg ? CallingConvention.ExtraArguments : CallingConvention.Default);
			if (IsGenericMethod)
			{
				callingConvention |= CallingConvention.Generic;
			}
			if (!IsStatic)
			{
				callingConvention |= CallingConvention.HasThis;
			}
			return callingConvention;
		}
	}

	internal (BlockSyntax blockBody, ArrowExpressionClauseSyntax arrowBody) Bodies
	{
		get
		{
			CSharpSyntaxNode syntaxNode = base.SyntaxNode;
			if (!(syntaxNode is BaseMethodDeclarationSyntax baseMethodDeclarationSyntax))
			{
				if (!(syntaxNode is AccessorDeclarationSyntax accessorDeclarationSyntax))
				{
					if (!(syntaxNode is ArrowExpressionClauseSyntax item))
					{
						if (syntaxNode is BlockSyntax item2)
						{
							return (blockBody: item2, arrowBody: null);
						}
						return (blockBody: null, arrowBody: null);
					}
					return (blockBody: null, arrowBody: item);
				}
				return (blockBody: accessorDeclarationSyntax.Body, arrowBody: accessorDeclarationSyntax.ExpressionBody);
			}
			return (blockBody: baseMethodDeclarationSyntax.Body, arrowBody: baseMethodDeclarationSyntax.ExpressionBody);
		}
	}

	public override ImmutableArray<Location> Locations => ImmutableArray.Create(_location);

	public sealed override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotations => GetTypeParametersAsTypeArguments();

	public sealed override int Arity => TypeParameters.Length;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

	internal sealed override OverriddenOrHiddenMembersResult OverriddenOrHiddenMembers
	{
		get
		{
			LazyMethodChecks();
			if (_lazyOverriddenOrHiddenMembers == null)
			{
				Interlocked.CompareExchange(ref _lazyOverriddenOrHiddenMembers, this.MakeOverriddenOrHiddenMembers(), null);
			}
			return _lazyOverriddenOrHiddenMembers;
		}
	}

	internal sealed override bool RequiresCompletion => true;

	internal bool IsExpressionBodied => flags.IsExpressionBodied;

	public sealed override RefKind RefKind => flags.RefKind;

	public sealed override bool IsVararg => flags.IsVararg;

	internal ImmutableArray<Diagnostic> SetDiagnostics(ImmutableArray<Diagnostic> newSet, out bool diagsWritten)
	{
		diagsWritten = ImmutableInterlocked.InterlockedInitialize(ref _cachedDiagnostics, newSet);
		return _cachedDiagnostics;
	}

	protected SourceMemberMethodSymbol(NamedTypeSymbol containingType, SyntaxReference syntaxReferenceOpt, Location location, bool isIterator, (DeclarationModifiers declarationModifiers, Flags flags) modifiersAndFlags)
		: base(syntaxReferenceOpt, isIterator)
	{
		_containingType = containingType;
		_location = location;
		(DeclarationModifiers, flags) = modifiersAndFlags;
	}

	protected void CheckEffectiveAccessibility(TypeWithAnnotations returnType, ImmutableArray<ParameterSymbol> parameters, BindingDiagnosticBag diagnostics)
	{
		if (DeclaredAccessibility <= Accessibility.Private || MethodKind == MethodKind.ExplicitInterfaceImplementation)
		{
			return;
		}
		ErrorCode code = ((MethodKind == MethodKind.Conversion || MethodKind == MethodKind.UserDefinedOperator) ? ErrorCode.ERR_BadVisOpReturn : ErrorCode.ERR_BadVisReturnType);
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, ContainingAssembly);
		if (!this.IsNoMoreVisibleThan(returnType, ref useSiteInfo))
		{
			diagnostics.Add(code, GetFirstLocation(), this, returnType.Type);
		}
		code = ((MethodKind == MethodKind.Conversion || MethodKind == MethodKind.UserDefinedOperator) ? ErrorCode.ERR_BadVisOpParam : ErrorCode.ERR_BadVisParamType);
		foreach (ParameterSymbol item in parameters)
		{
			if (!item.TypeWithAnnotations.IsAtLeastAsVisibleAs(this, ref useSiteInfo))
			{
				diagnostics.Add(code, GetFirstLocation(), this, item.Type);
			}
		}
		if (this.IsExtensionBlockMember())
		{
			ParameterSymbol extensionParameter = ContainingType.ExtensionParameter;
			if ((object)extensionParameter != null && !extensionParameter.TypeWithAnnotations.IsAtLeastAsVisibleAs(this, ref useSiteInfo))
			{
				diagnostics.Add(code, GetFirstLocation(), this, extensionParameter.Type);
			}
		}
		diagnostics.Add(GetFirstLocation(), useSiteInfo);
	}

	protected void CheckFileTypeUsage(TypeWithAnnotations returnType, ImmutableArray<ParameterSymbol> parameters, BindingDiagnosticBag diagnostics)
	{
		NamedTypeSymbol namedTypeSymbol = ContainingType;
		if ((object)namedTypeSymbol != null && namedTypeSymbol.IsExtension)
		{
			NamedTypeSymbol containingType = namedTypeSymbol.ContainingType;
			if ((object)containingType != null)
			{
				namedTypeSymbol = containingType;
			}
		}
		if (namedTypeSymbol.HasFileLocalTypes())
		{
			return;
		}
		if (returnType.Type.HasFileLocalTypes())
		{
			diagnostics.Add(ErrorCode.ERR_FileTypeDisallowedInSignature, GetFirstLocation(), returnType.Type, namedTypeSymbol);
		}
		foreach (ParameterSymbol item in parameters)
		{
			if (item.Type.HasFileLocalTypes())
			{
				diagnostics.Add(ErrorCode.ERR_FileTypeDisallowedInSignature, GetFirstLocation(), item.Type, namedTypeSymbol);
			}
		}
	}

	protected static Flags MakeFlags(MethodKind methodKind, RefKind refKind, DeclarationModifiers declarationModifiers, bool returnsVoid, bool returnsVoidIsSet, bool isExpressionBodied, bool isExtensionMethod, bool isNullableAnalysisEnabled, bool isVarArg, bool isExplicitInterfaceImplementation, bool hasThisInitializer)
	{
		return new Flags(methodKind, refKind, declarationModifiers, returnsVoid, returnsVoidIsSet, isExpressionBodied, isExtensionMethod, isNullableAnalysisEnabled, isVarArg, isExplicitInterfaceImplementation, hasThisInitializer);
	}

	protected void SetReturnsVoid(bool returnsVoid)
	{
		flags.SetReturnsVoid(returnsVoid);
	}

	protected abstract void MethodChecks(BindingDiagnosticBag diagnostics);

	protected void LazyMethodChecks()
	{
		if (state.HasComplete(CompletionPart.StartMemberChecks))
		{
			return;
		}
		lock (MethodChecksLockObject)
		{
			if (state.NotePartComplete(CompletionPart.SynthesizedExplicitImplementations))
			{
				BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
				try
				{
					MethodChecks(instance);
					AddDeclarationDiagnostics(instance);
					return;
				}
				finally
				{
					state.NotePartComplete(CompletionPart.StartMemberChecks);
					instance.Free();
				}
			}
		}
	}

	protected virtual void LazyAsyncMethodChecks(CancellationToken cancellationToken)
	{
		state.NotePartComplete(CompletionPart.Members);
		state.NotePartComplete(CompletionPart.TypeMembers);
	}

	internal override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		if (IsExplicitInterfaceImplementation && _containingType.IsInterface)
		{
			return false;
		}
		if (!IsOverride)
		{
			if (!IsStatic)
			{
				return IsMetadataVirtual(ignoreInterfaceImplementationChanges ? IsMetadataVirtualOption.IgnoreInterfaceImplementationChanges : IsMetadataVirtualOption.None);
			}
			return false;
		}
		bool warnAmbiguous;
		return this.RequiresExplicitOverride(out warnAmbiguous);
	}

	internal override bool IsMetadataVirtual(IsMetadataVirtualOption option = IsMetadataVirtualOption.None)
	{
		return flags.IsMetadataVirtual(option == IsMetadataVirtualOption.IgnoreInterfaceImplementationChanges);
	}

	internal void EnsureMetadataVirtual()
	{
		flags.EnsureMetadataVirtual();
	}

	private Binder TryGetInMethodBinder(BinderFactory binderFactoryOpt = null)
	{
		CSharpSyntaxNode inMethodSyntaxNode = GetInMethodSyntaxNode();
		if (inMethodSyntaxNode == null)
		{
			return null;
		}
		return (binderFactoryOpt ?? DeclaringCompilation.GetBinderFactory(inMethodSyntaxNode.SyntaxTree)).GetBinder(inMethodSyntaxNode);
	}

	internal abstract ExecutableCodeBinder TryGetBodyBinder(BinderFactory binderFactoryOpt = null, bool ignoreAccessibility = false);

	protected ExecutableCodeBinder TryGetBodyBinderFromSyntax(BinderFactory binderFactoryOpt = null, bool ignoreAccessibility = false)
	{
		Binder binder = TryGetInMethodBinder(binderFactoryOpt);
		if (binder != null)
		{
			return new ExecutableCodeBinder(base.SyntaxNode, this, binder.WithAdditionalFlags(ignoreAccessibility ? BinderFlags.IgnoreAccessibility : BinderFlags.None));
		}
		return null;
	}

	public override Location TryGetFirstLocation()
	{
		return _location;
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return SourceDocumentationCommentUtils.GetAndCacheDocumentationComment(this, expandIncludes, ref expandIncludes ? ref lazyExpandedDocComment : ref lazyDocComment);
	}

	internal sealed override bool TryGetThisParameter(out ParameterSymbol? thisParameter)
	{
		thisParameter = _lazyThisParameter;
		if ((object)thisParameter != null || IsStatic || this.IsExtensionBlockMember())
		{
			return true;
		}
		Interlocked.CompareExchange(ref _lazyThisParameter, new ThisParameterSymbol(this), null);
		thisParameter = _lazyThisParameter;
		return true;
	}

	internal sealed override bool HasComplete(CompletionPart part)
	{
		return state.HasComplete(part);
	}

	internal override void ForceComplete(SourceLocation? locationOpt, Predicate<Symbol>? filter, CancellationToken cancellationToken)
	{
		if (filter != null && !filter(this))
		{
			return;
		}
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			CompletionPart nextIncompletePart = state.NextIncompletePart;
			switch (nextIncompletePart)
			{
			case CompletionPart.Attributes:
				GetAttributes();
				break;
			case CompletionPart.ReturnTypeAttributes:
				GetReturnTypeAttributes();
				break;
			case CompletionPart.Type:
				_ = ReturnTypeWithAnnotations;
				state.NotePartComplete(CompletionPart.Type);
				break;
			case CompletionPart.Parameters:
				foreach (ParameterSymbol parameter in Parameters)
				{
					parameter.ForceComplete(locationOpt, null, cancellationToken);
				}
				if (this is SynthesizedPrimaryConstructor synthesizedPrimaryConstructor)
				{
					foreach (FieldSymbol backingField in synthesizedPrimaryConstructor.GetBackingFields())
					{
						backingField.GetAttributes();
					}
				}
				state.NotePartComplete(CompletionPart.Parameters);
				break;
			case CompletionPart.TypeParameters:
				foreach (TypeParameterSymbol typeParameter in TypeParameters)
				{
					typeParameter.ForceComplete(locationOpt, null, cancellationToken);
				}
				state.NotePartComplete(CompletionPart.TypeParameters);
				break;
			case CompletionPart.Members:
			case CompletionPart.TypeMembers:
				LazyAsyncMethodChecks(cancellationToken);
				break;
			case CompletionPart.SynthesizedExplicitImplementations:
			case CompletionPart.StartMemberChecks:
			{
				LazyMethodChecks();
				CompletionPart part = CompletionPart.MethodSymbolAll;
				state.SpinWaitComplete(part, cancellationToken);
				return;
			}
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete(CompletionPart.ImportsAll | CompletionPart.StartInterfaces | CompletionPart.FinishInterfaces | CompletionPart.EnumUnderlyingType | CompletionPart.TypeArguments | CompletionPart.FinishMemberChecks | CompletionPart.MembersCompletedChecksStarted | CompletionPart.MembersCompleted);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	protected sealed override void NoteAttributesComplete(bool forReturnType)
	{
		CompletionPart part = ((!forReturnType) ? CompletionPart.Attributes : CompletionPart.ReturnTypeAttributes);
		state.NotePartComplete(part);
	}

	internal override void AfterAddingTypeMembersChecks(ConversionsBase conversions, BindingDiagnosticBag diagnostics)
	{
		base.AfterAddingTypeMembersChecks(conversions, diagnostics);
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		if (IsDeclaredReadOnly && !ContainingType.IsReadOnly)
		{
			declaringCompilation.EnsureIsReadOnlyAttributeExists(diagnostics, _location, modifyCompilation: true);
		}
		if (declaringCompilation.ShouldEmitNullableAttributes(this) && ShouldEmitNullableContextValue(out var _))
		{
			declaringCompilation.EnsureNullableContextAttributeExists(diagnostics, _location, modifyCompilation: true);
		}
	}

	internal override byte? GetLocalNullableContextValue()
	{
		if (!flags.TryGetNullableContext(out var value))
		{
			value = ComputeNullableContextValue(this);
			flags.SetNullableContext(value);
		}
		return value;
	}

	internal static byte? ComputeNullableContextValue(MethodSymbol method)
	{
		CSharpCompilation declaringCompilation = method.DeclaringCompilation;
		if (!declaringCompilation.ShouldEmitNullableAttributes(method))
		{
			return null;
		}
		MostCommonNullableValueBuilder builder = default(MostCommonNullableValueBuilder);
		foreach (TypeParameterSymbol typeParameter in method.TypeParameters)
		{
			typeParameter.GetCommonNullableValues(declaringCompilation, ref builder);
		}
		builder.AddValue(method.ReturnTypeWithAnnotations);
		foreach (ParameterSymbol parameter in method.Parameters)
		{
			parameter.GetCommonNullableValues(declaringCompilation, ref builder);
		}
		return builder.MostCommonValue;
	}

	internal override bool IsNullableAnalysisEnabled()
	{
		return flags.IsNullableAnalysisEnabled;
	}

	protected void CheckModifiersForBody(Location location, BindingDiagnosticBag diagnostics)
	{
		if (IsExtern && !IsAbstract)
		{
			diagnostics.Add(ErrorCode.ERR_ExternHasBody, location, this);
		}
		else if (IsAbstract && !IsExtern)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractHasBody, location, this);
		}
	}

	protected void CheckFeatureAvailabilityAndRuntimeSupport(SyntaxNode declarationSyntax, Location location, bool hasBody, BindingDiagnosticBag diagnostics)
	{
		if (_containingType.IsInterface)
		{
			if ((!IsStatic || MethodKind == MethodKind.StaticConstructor) && (hasBody || IsExplicitInterfaceImplementation))
			{
				Binder.CheckFeatureAvailability(declarationSyntax, MessageID.IDS_DefaultInterfaceImplementation, diagnostics, location);
			}
			if ((((hasBody || IsExtern) && (!IsStatic || !IsVirtual)) || IsExplicitInterfaceImplementation) && !ContainingAssembly.RuntimeSupportsDefaultInterfaceImplementation)
			{
				diagnostics.Add(ErrorCode.ERR_RuntimeDoesNotSupportDefaultInterfaceImplementation, location);
			}
			if (((!hasBody && IsAbstract) || IsVirtual) && !IsExplicitInterfaceImplementation && IsStatic && !ContainingAssembly.RuntimeSupportsStaticAbstractMembersInInterfaces)
			{
				diagnostics.Add(ErrorCode.ERR_RuntimeDoesNotSupportStaticAbstractMembersInInterfaces, location);
			}
		}
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		(BlockSyntax blockBody, ArrowExpressionClauseSyntax arrowBody) bodies = Bodies;
		BlockSyntax item = bodies.blockBody;
		ArrowExpressionClauseSyntax item2 = bodies.arrowBody;
		CSharpSyntaxNode cSharpSyntaxNode = null;
		if (item != null && item.Span.Contains(localPosition))
		{
			cSharpSyntaxNode = item;
		}
		else
		{
			if (item2 == null || !item2.Span.Contains(localPosition))
			{
				return -1;
			}
			cSharpSyntaxNode = item2;
		}
		return localPosition - cSharpSyntaxNode.SpanStart;
	}
}
