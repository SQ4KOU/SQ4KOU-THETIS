using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceMemberContainerTypeSymbol : NamedTypeSymbol
{
	private struct Flags
	{
		private int _flags;

		private const int SpecialTypeOffset = 0;

		private const int SpecialTypeSize = 6;

		private const int ManagedKindOffset = 6;

		private const int ManagedKindSize = 2;

		private const int FieldDefinitionsNotedOffset = 8;

		private const int FieldDefinitionsNotedSize = 1;

		private const int FlattenedMembersIsSortedOffset = 9;

		private const int FlattenedMembersIsSortedSize = 1;

		private const int TypeKindOffset = 10;

		private const int TypeKindSize = 4;

		private const int NullableContextOffset = 14;

		private const int NullableContextSize = 3;

		private const int HasDeclaredRequiredMembersOffset = 17;

		private const int HasDeclaredRequiredMembersSize = 2;

		private const int HasPrimaryConstructorOffset = 19;

		private const int SpecialTypeMask = 63;

		private const int ManagedKindMask = 3;

		private const int TypeKindMask = 15;

		private const int NullableContextMask = 7;

		private const int FieldDefinitionsNotedBit = 256;

		private const int FlattenedMembersIsSortedBit = 512;

		private const int HasDeclaredMembersBit = 131072;

		private const int HasDeclaredMembersBitSet = 262144;

		private const int HasPrimaryConstructorBit = 524288;

		public ExtendedSpecialType ExtendedSpecialType => (ExtendedSpecialType)(_flags & 0x3F);

		public ManagedKind ManagedKind => (ManagedKind)((_flags >> 6) & 3);

		public bool FieldDefinitionsNoted => (_flags & 0x100) != 0;

		public bool FlattenedMembersIsSorted => (_flags & 0x200) != 0;

		public TypeKind TypeKind => (TypeKind)((_flags >> 10) & 0xF);

		public readonly bool HasPrimaryConstructor => (_flags & 0x80000) != 0;

		public Flags(ExtendedSpecialType specialType, TypeKind typeKind, bool hasPrimaryConstructor)
		{
			int num = (int)specialType & 0x3F;
			int num2 = (int)((uint)(typeKind & (TypeKind)0xF) << 10);
			int num3 = (hasPrimaryConstructor ? 524288 : 0);
			_flags = num | num2 | num3;
		}

		public void SetFieldDefinitionsNoted()
		{
			ThreadSafeFlagOperations.Set(ref _flags, 256);
		}

		public void SetFlattenedMembersIsSorted()
		{
			ThreadSafeFlagOperations.Set(ref _flags, 512);
		}

		private static bool BitsAreUnsetOrSame(int bits, int mask)
		{
			if ((bits & mask) != 0)
			{
				return (bits & mask) == mask;
			}
			return true;
		}

		public void SetManagedKind(ManagedKind managedKind)
		{
			int toSet = (int)((uint)(managedKind & ManagedKind.Managed) << 6);
			ThreadSafeFlagOperations.Set(ref _flags, toSet);
		}

		public bool TryGetNullableContext(out byte? value)
		{
			return ((NullableContextKind)((_flags >> 14) & 7)).TryGetByte(out value);
		}

		public bool SetNullableContext(byte? value)
		{
			return ThreadSafeFlagOperations.Set(ref _flags, (int)((uint)(value.ToNullableContextFlags() & (NullableContextKind)7) << 14));
		}

		public bool TryGetHasDeclaredRequiredMembers(out bool value)
		{
			if ((_flags & 0x40000) != 0)
			{
				value = (_flags & 0x20000) != 0;
				return true;
			}
			value = false;
			return false;
		}

		public bool SetHasDeclaredRequiredMembers(bool value)
		{
			return ThreadSafeFlagOperations.Set(ref _flags, 0x40000 | (value ? 131072 : 0));
		}
	}

	protected sealed class MembersAndInitializers
	{
		internal readonly SynthesizedPrimaryConstructor? PrimaryConstructor;

		internal readonly ImmutableArray<Symbol> NonTypeMembers;

		internal readonly ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> StaticInitializers;

		internal readonly ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> InstanceInitializers;

		internal readonly bool HaveIndexers;

		internal readonly bool IsNullableEnabledForInstanceConstructorsAndFields;

		internal readonly bool IsNullableEnabledForStaticConstructorsAndFields;

		public MembersAndInitializers(SynthesizedPrimaryConstructor? primaryConstructor, ImmutableArray<Symbol> nonTypeMembers, ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> staticInitializers, ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> instanceInitializers, bool haveIndexers, bool isNullableEnabledForInstanceConstructorsAndFields, bool isNullableEnabledForStaticConstructorsAndFields)
		{
			PrimaryConstructor = primaryConstructor;
			NonTypeMembers = nonTypeMembers;
			StaticInitializers = staticInitializers;
			InstanceInitializers = instanceInitializers;
			HaveIndexers = haveIndexers;
			IsNullableEnabledForInstanceConstructorsAndFields = isNullableEnabledForInstanceConstructorsAndFields;
			IsNullableEnabledForStaticConstructorsAndFields = isNullableEnabledForStaticConstructorsAndFields;
		}
	}

	private sealed class DeclaredMembersAndInitializersBuilder
	{
		public ArrayBuilder<Symbol> NonTypeMembersWithPartialImplementations = ArrayBuilder<Symbol>.GetInstance();

		public readonly ArrayBuilder<ArrayBuilder<FieldOrPropertyInitializer>> StaticInitializers = ArrayBuilder<ArrayBuilder<FieldOrPropertyInitializer>>.GetInstance();

		public readonly ArrayBuilder<ArrayBuilder<FieldOrPropertyInitializer>> InstanceInitializers = ArrayBuilder<ArrayBuilder<FieldOrPropertyInitializer>>.GetInstance();

		public bool HaveIndexers;

		public TypeDeclarationSyntax? DeclarationWithParameters;

		public SynthesizedPrimaryConstructor? PrimaryConstructor;

		public bool IsNullableEnabledForInstanceConstructorsAndFields;

		public bool IsNullableEnabledForStaticConstructorsAndFields;

		public DeclaredMembersAndInitializers ToReadOnlyAndFree(CSharpCompilation compilation)
		{
			return new DeclaredMembersAndInitializers(NonTypeMembersWithPartialImplementations.ToImmutableAndFree(), MembersAndInitializersBuilder.ToReadOnlyAndFree(StaticInitializers), MembersAndInitializersBuilder.ToReadOnlyAndFree(InstanceInitializers), HaveIndexers, DeclarationWithParameters, PrimaryConstructor, IsNullableEnabledForInstanceConstructorsAndFields, IsNullableEnabledForStaticConstructorsAndFields, compilation);
		}

		public void UpdateIsNullableEnabledForConstructorsAndFields(bool useStatic, CSharpCompilation compilation, CSharpSyntaxNode syntax)
		{
			ref bool isNullableEnabledForConstructorsAndFields = ref GetIsNullableEnabledForConstructorsAndFields(useStatic);
			isNullableEnabledForConstructorsAndFields = isNullableEnabledForConstructorsAndFields || compilation.IsNullableAnalysisEnabledIn(syntax);
		}

		public void UpdateIsNullableEnabledForConstructorsAndFields(bool useStatic, bool value)
		{
			GetIsNullableEnabledForConstructorsAndFields(useStatic) |= value;
		}

		private ref bool GetIsNullableEnabledForConstructorsAndFields(bool useStatic)
		{
			if (!useStatic)
			{
				return ref IsNullableEnabledForInstanceConstructorsAndFields;
			}
			return ref IsNullableEnabledForStaticConstructorsAndFields;
		}

		public void Free()
		{
			NonTypeMembersWithPartialImplementations.Free();
			foreach (ArrayBuilder<FieldOrPropertyInitializer> staticInitializer in StaticInitializers)
			{
				staticInitializer.Free();
			}
			StaticInitializers.Free();
			foreach (ArrayBuilder<FieldOrPropertyInitializer> instanceInitializer in InstanceInitializers)
			{
				instanceInitializer.Free();
			}
			InstanceInitializers.Free();
		}
	}

	protected sealed class DeclaredMembersAndInitializers
	{
		public readonly ImmutableArray<Symbol> NonTypeMembersWithPartialImplementations;

		public readonly ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> StaticInitializers;

		public readonly ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> InstanceInitializers;

		public readonly bool HaveIndexers;

		public readonly TypeDeclarationSyntax? DeclarationWithParameters;

		public readonly SynthesizedPrimaryConstructor? PrimaryConstructor;

		public readonly bool IsNullableEnabledForInstanceConstructorsAndFields;

		public readonly bool IsNullableEnabledForStaticConstructorsAndFields;

		private ImmutableArray<Symbol> _lazyNonTypeMembers;

		public static readonly DeclaredMembersAndInitializers UninitializedSentinel = new DeclaredMembersAndInitializers();

		private DeclaredMembersAndInitializers()
		{
		}

		public DeclaredMembersAndInitializers(ImmutableArray<Symbol> nonTypeMembersWithPartialImplementations, ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> staticInitializers, ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> instanceInitializers, bool haveIndexers, TypeDeclarationSyntax? declarationWithParameters, SynthesizedPrimaryConstructor? primaryConstructor, bool isNullableEnabledForInstanceConstructorsAndFields, bool isNullableEnabledForStaticConstructorsAndFields, CSharpCompilation compilation)
		{
			NonTypeMembersWithPartialImplementations = nonTypeMembersWithPartialImplementations;
			StaticInitializers = staticInitializers;
			InstanceInitializers = instanceInitializers;
			HaveIndexers = haveIndexers;
			DeclarationWithParameters = declarationWithParameters;
			PrimaryConstructor = primaryConstructor;
			IsNullableEnabledForInstanceConstructorsAndFields = isNullableEnabledForInstanceConstructorsAndFields;
			IsNullableEnabledForStaticConstructorsAndFields = isNullableEnabledForStaticConstructorsAndFields;
		}

		[Conditional("DEBUG")]
		public static void AssertInitializers(ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> initializers, CSharpCompilation compilation)
		{
			if (!initializers.IsEmpty)
			{
				foreach (ImmutableArray<FieldOrPropertyInitializer> item in initializers)
				{
					_ = item;
				}
				for (int i = 0; i < initializers.Length; i++)
				{
					_ = 0;
					_ = i + 1;
					_ = initializers.Length;
					_ = initializers[i].Length;
					_ = 1;
				}
			}
		}

		public ImmutableArray<Symbol> GetNonTypeMembers(SourceMemberContainerTypeSymbol container)
		{
			if (_lazyNonTypeMembers.IsDefault)
			{
				container.MergePartialMembersAndInitializeNonTypeMembers(NonTypeMembersWithPartialImplementations, ref _lazyNonTypeMembers);
			}
			return _lazyNonTypeMembers;
		}
	}

	private sealed class MembersAndInitializersBuilder
	{
		private ArrayBuilder<Symbol>? NonTypeMembers;

		private ArrayBuilder<FieldOrPropertyInitializer>? InstanceInitializersForPositionalMembers;

		private bool IsNullableEnabledForInstanceConstructorsAndFields;

		private bool IsNullableEnabledForStaticConstructorsAndFields;

		public MembersAndInitializersBuilder(DeclaredMembersAndInitializers declaredMembersAndInitializers)
		{
			IsNullableEnabledForInstanceConstructorsAndFields = declaredMembersAndInitializers.IsNullableEnabledForInstanceConstructorsAndFields;
			IsNullableEnabledForStaticConstructorsAndFields = declaredMembersAndInitializers.IsNullableEnabledForStaticConstructorsAndFields;
		}

		public MembersAndInitializers ToReadOnlyAndFree(SourceMemberContainerTypeSymbol container, DeclaredMembersAndInitializers declaredMembers)
		{
			ImmutableArray<Symbol> nonTypeMembers = NonTypeMembers?.ToImmutableAndFree() ?? declaredMembers.GetNonTypeMembers(container);
			ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> instanceInitializers = ((InstanceInitializersForPositionalMembers == null) ? declaredMembers.InstanceInitializers : mergeInitializers());
			return new MembersAndInitializers(declaredMembers.PrimaryConstructor, nonTypeMembers, declaredMembers.StaticInitializers, instanceInitializers, declaredMembers.HaveIndexers, IsNullableEnabledForInstanceConstructorsAndFields, IsNullableEnabledForStaticConstructorsAndFields);
			ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> mergeInitializers()
			{
				int length = declaredMembers.InstanceInitializers.Length;
				if (length == 0)
				{
					return ImmutableArray.Create(InstanceInitializersForPositionalMembers.ToImmutableAndFree());
				}
				CSharpCompilation declaringCompilation = declaredMembers.PrimaryConstructor.DeclaringCompilation;
				LexicalSortKey xSortKey = new LexicalSortKey(InstanceInitializersForPositionalMembers.First().Syntax, declaringCompilation);
				int i;
				for (i = 0; i < length && LexicalSortKey.Compare(xSortKey, new LexicalSortKey(declaredMembers.InstanceInitializers[i][0].Syntax, declaringCompilation)) >= 0; i++)
				{
				}
				ArrayBuilder<ImmutableArray<FieldOrPropertyInitializer>> instance;
				if (i != length && declaredMembers.DeclarationWithParameters.SyntaxTree == declaredMembers.InstanceInitializers[i][0].Syntax.SyntaxTree && declaredMembers.DeclarationWithParameters.Span.Contains(declaredMembers.InstanceInitializers[i][0].Syntax.Span.Start))
				{
					ImmutableArray<FieldOrPropertyInitializer> items = declaredMembers.InstanceInitializers[i];
					ArrayBuilder<FieldOrPropertyInitializer> instanceInitializersForPositionalMembers = InstanceInitializersForPositionalMembers;
					instanceInitializersForPositionalMembers.AddRange(items);
					instance = ArrayBuilder<ImmutableArray<FieldOrPropertyInitializer>>.GetInstance(length);
					instance.AddRange(declaredMembers.InstanceInitializers, i);
					instance.Add(instanceInitializersForPositionalMembers.ToImmutableAndFree());
					instance.AddRange(declaredMembers.InstanceInitializers, i + 1, length - (i + 1));
				}
				else
				{
					instance = ArrayBuilder<ImmutableArray<FieldOrPropertyInitializer>>.GetInstance(length + 1);
					instance.AddRange(declaredMembers.InstanceInitializers, i);
					instance.Add(InstanceInitializersForPositionalMembers.ToImmutableAndFree());
					instance.AddRange(declaredMembers.InstanceInitializers, i, length - i);
				}
				return instance.ToImmutableAndFree();
			}
		}

		public void AddInstanceInitializerForPositionalMembers(FieldOrPropertyInitializer initializer)
		{
			if (InstanceInitializersForPositionalMembers == null)
			{
				InstanceInitializersForPositionalMembers = ArrayBuilder<FieldOrPropertyInitializer>.GetInstance();
			}
			InstanceInitializersForPositionalMembers.Add(initializer);
		}

		public IReadOnlyCollection<Symbol> GetNonTypeMembers(SourceMemberContainerTypeSymbol container, DeclaredMembersAndInitializers declaredMembers)
		{
			IReadOnlyCollection<Symbol> nonTypeMembers = NonTypeMembers;
			return (IReadOnlyCollection<Symbol>)(nonTypeMembers ?? ((object)declaredMembers.GetNonTypeMembers(container)));
		}

		public void AddNonTypeMember(SourceMemberContainerTypeSymbol container, Symbol member, DeclaredMembersAndInitializers declaredMembers)
		{
			if (NonTypeMembers == null)
			{
				ImmutableArray<Symbol> nonTypeMembers = declaredMembers.GetNonTypeMembers(container);
				NonTypeMembers = ArrayBuilder<Symbol>.GetInstance(nonTypeMembers.Length + 1);
				NonTypeMembers.AddRange(nonTypeMembers);
			}
			NonTypeMembers.Add(member);
		}

		public void SetNonTypeMembers(ArrayBuilder<Symbol> members)
		{
			NonTypeMembers?.Free();
			NonTypeMembers = members;
		}

		public void UpdateIsNullableEnabledForConstructorsAndFields(bool useStatic, CSharpCompilation compilation, CSharpSyntaxNode syntax)
		{
			ref bool isNullableEnabledForConstructorsAndFields = ref GetIsNullableEnabledForConstructorsAndFields(useStatic);
			isNullableEnabledForConstructorsAndFields = isNullableEnabledForConstructorsAndFields || compilation.IsNullableAnalysisEnabledIn(syntax);
		}

		private ref bool GetIsNullableEnabledForConstructorsAndFields(bool useStatic)
		{
			if (!useStatic)
			{
				return ref IsNullableEnabledForInstanceConstructorsAndFields;
			}
			return ref IsNullableEnabledForStaticConstructorsAndFields;
		}

		internal static ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> ToReadOnlyAndFree(ArrayBuilder<ArrayBuilder<FieldOrPropertyInitializer>> initializers)
		{
			if (initializers.Count == 0)
			{
				initializers.Free();
				return ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>>.Empty;
			}
			ArrayBuilder<ImmutableArray<FieldOrPropertyInitializer>> instance = ArrayBuilder<ImmutableArray<FieldOrPropertyInitializer>>.GetInstance(initializers.Count);
			foreach (ArrayBuilder<FieldOrPropertyInitializer> initializer in initializers)
			{
				instance.Add(initializer.ToImmutableAndFree());
			}
			initializers.Free();
			return instance.ToImmutableAndFree();
		}

		public void Free()
		{
			NonTypeMembers?.Free();
			InstanceInitializersForPositionalMembers?.Free();
		}
	}

	internal class SynthesizedExplicitImplementations
	{
		public static readonly SynthesizedExplicitImplementations Empty = new SynthesizedExplicitImplementations(ImmutableArray<SynthesizedExplicitImplementationForwardingMethod>.Empty, ImmutableArray<(MethodSymbol, MethodSymbol)>.Empty);

		public readonly ImmutableArray<SynthesizedExplicitImplementationForwardingMethod> ForwardingMethods;

		public readonly ImmutableArray<(MethodSymbol Body, MethodSymbol Implemented)> MethodImpls;

		private SynthesizedExplicitImplementations(ImmutableArray<SynthesizedExplicitImplementationForwardingMethod> forwardingMethods, ImmutableArray<(MethodSymbol Body, MethodSymbol Implemented)> methodImpls)
		{
			ForwardingMethods = forwardingMethods.NullToEmpty();
			MethodImpls = methodImpls.NullToEmpty();
		}

		internal static SynthesizedExplicitImplementations Create(ImmutableArray<SynthesizedExplicitImplementationForwardingMethod> forwardingMethods, ImmutableArray<(MethodSymbol Body, MethodSymbol Implemented)> methodImpls)
		{
			if (forwardingMethods.IsDefaultOrEmpty && methodImpls.IsDefaultOrEmpty)
			{
				return Empty;
			}
			return new SynthesizedExplicitImplementations(forwardingMethods, methodImpls);
		}
	}

	private enum HasBaseTypeDeclaringInterfaceResult
	{
		NoMatch,
		IgnoringNullableMatch,
		ExactMatch
	}

	private static readonly ObjectPool<PooledDictionary<Symbol, Symbol>> s_duplicateRecordMemberSignatureDictionary = PooledDictionary<Symbol, Symbol>.CreatePool(MemberSignatureComparer.RecordAPISignatureComparer);

	protected SymbolCompletionState state;

	private Flags _flags;

	private ImmutableArray<DiagnosticInfo> _managedKindUseSiteDiagnostics;

	private ImmutableArray<AssemblySymbol> _managedKindUseSiteDependencies;

	private readonly DeclarationModifiers _declModifiers;

	private readonly NamespaceOrTypeSymbol _containingSymbol;

	protected readonly MergedTypeDeclaration declaration;

	private ImmutableArray<SynthesizedSimpleProgramEntryPointSymbol> _lazySimpleProgramEntryPoints;

	private DeclaredMembersAndInitializers? _lazyDeclaredMembersAndInitializers = DeclaredMembersAndInitializers.UninitializedSentinel;

	private MembersAndInitializers? _lazyMembersAndInitializers;

	private Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>>? _lazyMembersDictionary;

	private Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>>? _lazyEarlyAttributeDecodingMembersDictionary;

	private static readonly Dictionary<ReadOnlyMemory<char>, ImmutableArray<NamedTypeSymbol>> s_emptyTypeMembers = new Dictionary<ReadOnlyMemory<char>, ImmutableArray<NamedTypeSymbol>>(EmptyReadOnlyMemoryOfCharComparer.Instance);

	private Dictionary<ReadOnlyMemory<char>, ImmutableArray<NamedTypeSymbol>>? _lazyTypeMembers;

	private ImmutableArray<Symbol> _lazyMembersFlattened;

	private SynthesizedExplicitImplementations? _lazySynthesizedExplicitImplementations;

	private int _lazyKnownCircularStruct;

	private LexicalSortKey _lazyLexicalSortKey = LexicalSortKey.NotInitialized;

	private ThreeState _lazyContainsExtensionMethods;

	private ThreeState _lazyAnyMemberHasAttributes;

	private ExtensionGroupingInfo? _lazyExtensionGroupingInfo;

	private static readonly ReportMismatchInReturnType<Location> ReportBadReturn = delegate(BindingDiagnosticBag diagnostics, MethodSymbol overriddenMethod, MethodSymbol overridingMethod, bool topLevel, Location location)
	{
		diagnostics.Add(topLevel ? ErrorCode.WRN_TopLevelNullabilityMismatchInReturnTypeOnOverride : ErrorCode.WRN_NullabilityMismatchInReturnTypeOnOverride, location);
	};

	private static readonly ReportMismatchInParameterType<Location> ReportBadParameter = delegate(BindingDiagnosticBag diagnostics, MethodSymbol overriddenMethod, MethodSymbol overridingMethod, ParameterSymbol overridingParameter, bool topLevel, Location location)
	{
		diagnostics.Add(topLevel ? ErrorCode.WRN_TopLevelNullabilityMismatchInParameterTypeOnOverride : ErrorCode.WRN_NullabilityMismatchInParameterTypeOnOverride, location, new FormattedSymbol(overridingParameter, SymbolDisplayFormat.ShortFormat));
	};

	internal sealed override bool RequiresCompletion => true;

	public sealed override NamedTypeSymbol? ContainingType => _containingSymbol as NamedTypeSymbol;

	public sealed override Symbol ContainingSymbol => _containingSymbol;

	public override ExtendedSpecialType ExtendedSpecialType => _flags.ExtendedSpecialType;

	public override TypeKind TypeKind => _flags.TypeKind;

	internal MergedTypeDeclaration MergedDeclaration => declaration;

	internal sealed override bool IsInterface => TypeKind == TypeKind.Interface;

	public override bool IsStatic => HasFlag(DeclarationModifiers.Static);

	public sealed override bool IsRefLikeType => HasFlag(DeclarationModifiers.Ref);

	public override bool IsReadOnly => HasFlag(DeclarationModifiers.ReadOnly);

	public override bool IsSealed => HasFlag(DeclarationModifiers.Sealed);

	public override bool IsAbstract => HasFlag(DeclarationModifiers.Abstract);

	internal bool IsPartial => HasFlag(DeclarationModifiers.Partial);

	internal bool IsNew => HasFlag(DeclarationModifiers.New);

	internal sealed override bool IsFileLocal => HasFlag(DeclarationModifiers.File);

	internal bool IsUnsafe => HasFlag(DeclarationModifiers.Unsafe);

	private SyntaxTree? AssociatedSyntaxTree
	{
		get
		{
			if (!IsFileLocal)
			{
				return null;
			}
			return declaration.Declarations[0].Location.SourceTree;
		}
	}

	internal sealed override FileIdentifier? AssociatedFileIdentifier
	{
		get
		{
			SyntaxTree associatedSyntaxTree = AssociatedSyntaxTree;
			if (associatedSyntaxTree == null)
			{
				return null;
			}
			return FileIdentifier.Create(associatedSyntaxTree, DeclaringCompilation?.Options?.SourceReferenceResolver);
		}
	}

	public override Accessibility DeclaredAccessibility => ModifierUtils.EffectiveAccessibility(_declModifiers);

	public override bool IsScriptClass
	{
		get
		{
			DeclarationKind kind = declaration.Declarations[0].Kind;
			if (kind != DeclarationKind.Script)
			{
				return kind == DeclarationKind.Submission;
			}
			return true;
		}
	}

	public override bool IsImplicitClass => declaration.Declarations[0].Kind == DeclarationKind.ImplicitClass;

	internal override bool IsRecord => declaration.Declarations[0].Kind == DeclarationKind.Record;

	internal override bool IsRecordStruct => declaration.Declarations[0].Kind == DeclarationKind.RecordStruct;

	public override bool IsImplicitlyDeclared
	{
		get
		{
			if (!IsImplicitClass)
			{
				return IsScriptClass;
			}
			return true;
		}
	}

	public override int Arity => declaration.Arity;

	public override string Name => declaration.Name;

	internal override bool MangleName => Arity > 0;

	public sealed override ImmutableArray<Location> Locations => ImmutableArray<Location>.CastUp(declaration.NameLocations.ToImmutable());

	public ImmutableArray<SyntaxReference> SyntaxReferences => declaration.SyntaxReferences;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => SyntaxReferences;

	internal ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> StaticInitializers => GetMembersAndInitializers().StaticInitializers;

	internal ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> InstanceInitializers => GetMembersAndInitializers().InstanceInitializers;

	public override IEnumerable<string> MemberNames
	{
		get
		{
			if (!IsTupleType && !IsRecord && !IsRecordStruct && !declaration.ContainsExtensionDeclarations)
			{
				return declaration.MemberNames;
			}
			return from m in GetMembers()
				select m.Name;
		}
	}

	internal override bool HasDeclaredRequiredMembers
	{
		get
		{
			if (_flags.TryGetHasDeclaredRequiredMembers(out var value))
			{
				return value;
			}
			value = declaration.Declarations.Any((SingleTypeDeclaration decl) => decl.HasRequiredMembers);
			_flags.SetHasDeclaredRequiredMembers(value);
			return value;
		}
	}

	internal bool AreMembersComplete => state.HasComplete(CompletionPart.Members);

	internal override bool KnownCircularStruct
	{
		get
		{
			if (_lazyKnownCircularStruct == 0)
			{
				if (TypeKind != TypeKind.Struct)
				{
					Interlocked.CompareExchange(ref _lazyKnownCircularStruct, 1, 0);
				}
				else
				{
					BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
					int value = (int)CheckStructCircularity(instance).ToThreeState();
					if (Interlocked.CompareExchange(ref _lazyKnownCircularStruct, value, 0) == 0)
					{
						AddDeclarationDiagnostics(instance);
					}
					instance.Free();
				}
			}
			return _lazyKnownCircularStruct == 2;
		}
	}

	internal bool HasPrimaryConstructor => _flags.HasPrimaryConstructor;

	internal SynthesizedPrimaryConstructor? PrimaryConstructor
	{
		get
		{
			if (!HasPrimaryConstructor)
			{
				return null;
			}
			DeclaredMembersAndInitializers declaredMembersAndInitializers = Volatile.Read(in _lazyDeclaredMembersAndInitializers);
			if (declaredMembersAndInitializers != null && declaredMembersAndInitializers != DeclaredMembersAndInitializers.UninitializedSentinel)
			{
				return declaredMembersAndInitializers.PrimaryConstructor;
			}
			return GetMembersAndInitializers().PrimaryConstructor;
		}
	}

	internal bool ContainsExtensionMethods
	{
		get
		{
			if (!_lazyContainsExtensionMethods.HasValue())
			{
				bool value = ((IsStatic && !base.IsGenericType) || IsScriptClass) && (declaration.ContainsExtensionMethods || declaration.ContainsExtensionDeclarations);
				_lazyContainsExtensionMethods = value.ToThreeState();
			}
			return _lazyContainsExtensionMethods.Value();
		}
	}

	internal bool AnyMemberHasAttributes
	{
		get
		{
			if (!_lazyAnyMemberHasAttributes.HasValue())
			{
				bool anyMemberHasAttributes = declaration.AnyMemberHasAttributes;
				_lazyAnyMemberHasAttributes = anyMemberHasAttributes.ToThreeState();
			}
			return _lazyAnyMemberHasAttributes.Value();
		}
	}

	public override bool MightContainExtensionMethods => ContainsExtensionMethods;

	public sealed override NamedTypeSymbol ConstructedFrom => this;

	internal SourceMemberContainerTypeSymbol(NamespaceOrTypeSymbol containingSymbol, MergedTypeDeclaration declaration, BindingDiagnosticBag diagnostics, TupleExtraData? tupleData = null)
		: base(tupleData)
	{
		_containingSymbol = containingSymbol;
		this.declaration = declaration;
		TypeKind typeKind = declaration.Kind.ToTypeKind();
		DeclarationModifiers declarationModifiers = MakeModifiers(typeKind, diagnostics);
		foreach (SingleTypeDeclaration declaration2 in declaration.Declarations)
		{
			diagnostics.AddRange(declaration2.Diagnostics);
		}
		int num = (int)(declarationModifiers & DeclarationModifiers.AccessibilityMask);
		if ((num & (num - 1)) != 0)
		{
			if ((declarationModifiers & DeclarationModifiers.Partial) != DeclarationModifiers.None)
			{
				diagnostics.Add(ErrorCode.ERR_PartialModifierConflict, GetFirstLocation(), this);
			}
			num &= ~(num - 1);
			declarationModifiers = (DeclarationModifiers)((uint)declarationModifiers & 0xFFFFFC0Fu);
			declarationModifiers = (DeclarationModifiers)((uint)declarationModifiers | (uint)num);
		}
		_declModifiers = declarationModifiers;
		_flags = new Flags((num == 16) ? MakeExtendedSpecialType() : default(ExtendedSpecialType), typeKind, declaration.HasPrimaryConstructor);
		NamedTypeSymbol? containingType = ContainingType;
		if ((object)containingType != null && containingType.IsSealed && DeclaredAccessibility.HasProtected())
		{
			diagnostics.Add(AccessCheck.GetProtectedMemberInSealedTypeError(ContainingType), GetFirstLocation(), this);
		}
		state.NotePartComplete(CompletionPart.TypeArguments);
	}

	private ExtendedSpecialType MakeExtendedSpecialType()
	{
		if (ContainingSymbol.Kind == SymbolKind.Namespace && ContainingSymbol.ContainingAssembly.KeepLookingForDeclaredSpecialTypes)
		{
			return SpecialTypes.GetTypeFromMetadataName(MetadataHelpers.BuildQualifiedName(ContainingSymbol.ToDisplayString(SymbolDisplayFormat.QualifiedNameOnlyFormat), MetadataName));
		}
		return default(ExtendedSpecialType);
	}

	private DeclarationModifiers MakeModifiers(TypeKind typeKind, BindingDiagnosticBag diagnostics)
	{
		Symbol containingSymbol = ContainingSymbol;
		DeclarationModifiers declarationModifiers;
		DeclarationModifiers defaultAccess;
		if (typeKind == TypeKind.Extension)
		{
			declarationModifiers = DeclarationModifiers.None;
			defaultAccess = DeclarationModifiers.Public;
		}
		else
		{
			declarationModifiers = DeclarationModifiers.AccessibilityMask | DeclarationModifiers.File;
			if (containingSymbol.Kind == SymbolKind.Namespace)
			{
				defaultAccess = DeclarationModifiers.Internal;
			}
			else
			{
				declarationModifiers |= DeclarationModifiers.New;
				defaultAccess = ((!((NamedTypeSymbol)containingSymbol).IsInterface) ? DeclarationModifiers.Private : DeclarationModifiers.Public);
			}
			switch (typeKind)
			{
			case TypeKind.Class:
			case TypeKind.Submission:
				declarationModifiers |= DeclarationModifiers.Abstract | DeclarationModifiers.Sealed | DeclarationModifiers.Partial | DeclarationModifiers.Unsafe;
				if (!IsRecord)
				{
					declarationModifiers |= DeclarationModifiers.Static;
				}
				break;
			case TypeKind.Struct:
				declarationModifiers |= DeclarationModifiers.ReadOnly | DeclarationModifiers.Partial | DeclarationModifiers.Unsafe;
				if (!IsRecordStruct)
				{
					declarationModifiers |= DeclarationModifiers.Ref;
				}
				break;
			case TypeKind.Interface:
				declarationModifiers |= DeclarationModifiers.Partial | DeclarationModifiers.Unsafe;
				break;
			case TypeKind.Delegate:
				declarationModifiers |= DeclarationModifiers.Unsafe;
				break;
			}
		}
		DeclarationModifiers declarationModifiers2 = MakeAndCheckTypeModifiers(defaultAccess, declarationModifiers, diagnostics, out var modifierErrors);
		this.CheckUnsafeModifier(declarationModifiers2, diagnostics);
		if (!modifierErrors && (declarationModifiers2 & DeclarationModifiers.Abstract) != DeclarationModifiers.None && (declarationModifiers2 & (DeclarationModifiers.Sealed | DeclarationModifiers.Static)) != DeclarationModifiers.None)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractSealedStatic, GetFirstLocation(), this);
		}
		if (!modifierErrors && (declarationModifiers2 & (DeclarationModifiers.Sealed | DeclarationModifiers.Static)) == (DeclarationModifiers.Sealed | DeclarationModifiers.Static))
		{
			diagnostics.Add(ErrorCode.ERR_SealedStaticClass, GetFirstLocation(), this);
		}
		switch (typeKind)
		{
		case TypeKind.Interface:
			declarationModifiers2 |= DeclarationModifiers.Abstract;
			break;
		case TypeKind.Delegate:
		case TypeKind.Enum:
		case TypeKind.Struct:
		case TypeKind.Extension:
			declarationModifiers2 |= DeclarationModifiers.Sealed;
			break;
		}
		return declarationModifiers2;
	}

	private DeclarationModifiers MakeAndCheckTypeModifiers(DeclarationModifiers defaultAccess, DeclarationModifiers allowedModifiers, BindingDiagnosticBag diagnostics, out bool modifierErrors)
	{
		modifierErrors = false;
		DeclarationModifiers declarationModifiers = DeclarationModifiers.Unset;
		int length = declaration.Declarations.Length;
		bool flag = false;
		for (int i = 0; i < length; i++)
		{
			DeclarationModifiers declarationModifiers2 = declaration.Declarations[i].Modifiers;
			if (length > 1 && (declarationModifiers2 & DeclarationModifiers.Partial) == 0)
			{
				flag = true;
			}
			if (!modifierErrors)
			{
				declarationModifiers2 = ModifierUtils.CheckModifiers(isForTypeDeclaration: true, isForInterfaceMember: false, declarationModifiers2, allowedModifiers, declaration.Declarations[i].NameLocation, diagnostics, null, out modifierErrors);
				if (!modifierErrors)
				{
					modifierErrors = ModifierUtils.CheckAccessibility(declarationModifiers2, this, isExplicitInterfaceImplementation: false, diagnostics, GetFirstLocation());
				}
			}
			declarationModifiers = ((declarationModifiers != DeclarationModifiers.Unset) ? (declarationModifiers | declarationModifiers2) : declarationModifiers2);
		}
		if ((declarationModifiers & DeclarationModifiers.AccessibilityMask) == 0)
		{
			declarationModifiers |= defaultAccess;
		}
		else if ((declarationModifiers & DeclarationModifiers.File) != DeclarationModifiers.None)
		{
			diagnostics.Add(ErrorCode.ERR_FileTypeNoExplicitAccessibility, GetFirstLocation(), this);
		}
		if (flag)
		{
			if ((declarationModifiers & DeclarationModifiers.Partial) == 0)
			{
				switch (ContainingSymbol.Kind)
				{
				case SymbolKind.Namespace:
				{
					for (int k = 1; k < length; k++)
					{
						diagnostics.Add(((declarationModifiers & DeclarationModifiers.File) != DeclarationModifiers.None) ? ErrorCode.ERR_FileLocalDuplicateNameInNS : ErrorCode.ERR_DuplicateNameInNS, declaration.Declarations[k].NameLocation, Name, ContainingSymbol);
						modifierErrors = true;
					}
					break;
				}
				case SymbolKind.NamedType:
				{
					for (int j = 1; j < length; j++)
					{
						if (ContainingType.Locations.Length == 1 || ContainingType.IsPartial())
						{
							diagnostics.Add(ErrorCode.ERR_DuplicateNameInClass, declaration.Declarations[j].NameLocation, ContainingSymbol, Name);
						}
						modifierErrors = true;
					}
					break;
				}
				}
			}
			else
			{
				for (int l = 0; l < length; l++)
				{
					SingleTypeDeclaration singleTypeDeclaration = declaration.Declarations[l];
					if ((singleTypeDeclaration.Modifiers & DeclarationModifiers.Partial) == 0)
					{
						diagnostics.Add(ErrorCode.ERR_MissingPartial, singleTypeDeclaration.NameLocation, Name);
						modifierErrors = true;
					}
				}
			}
		}
		return declarationModifiers;
	}

	internal static bool IsReservedTypeName(string? name)
	{
		if (name != null && name.Length > 0)
		{
			return Enumerable.All(name, (char c) => c >= 'a' && c <= 'z');
		}
		return false;
	}

	internal static void ReportReservedTypeName(string? name, CSharpCompilation compilation, DiagnosticBag? diagnostics, Location location)
	{
		if (diagnostics != null && !reportIfContextual(SyntaxKind.RecordKeyword, MessageID.IDS_FeatureRecords, ErrorCode.WRN_RecordNamedDisallowed) && !reportIfContextual(SyntaxKind.RequiredKeyword, MessageID.IDS_FeatureRequiredMembers, ErrorCode.ERR_RequiredNameDisallowed) && !reportIfContextual(SyntaxKind.FileKeyword, MessageID.IDS_FeatureFileTypes, ErrorCode.ERR_FileTypeNameDisallowed) && !reportIfContextual(SyntaxKind.ScopedKeyword, MessageID.IDS_FeatureRefFields, ErrorCode.ERR_ScopedTypeNameDisallowed) && !reportIfContextual(SyntaxKind.ExtensionKeyword, MessageID.IDS_FeatureExtensions, ErrorCode.ERR_ExtensionTypeNameDisallowed) && IsReservedTypeName(name))
		{
			diagnostics.Add(ErrorCode.WRN_LowerCaseTypeName, location, name);
		}
		bool reportIfContextual(SyntaxKind contextualKind, MessageID featureId, ErrorCode error)
		{
			if (name == SyntaxFacts.GetText(contextualKind) && compilation.LanguageVersion >= featureId.RequiredVersion())
			{
				diagnostics.Add(error, location);
				return true;
			}
			return false;
		}
	}

	internal sealed override bool HasComplete(CompletionPart part)
	{
		return state.HasComplete(part);
	}

	protected abstract void CheckBase(BindingDiagnosticBag diagnostics);

	protected abstract void CheckInterfaces(BindingDiagnosticBag diagnostics);

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
			case CompletionPart.StartBaseType:
			case CompletionPart.FinishBaseType:
				if (state.NotePartComplete(CompletionPart.StartBaseType))
				{
					BindingDiagnosticBag instance3 = BindingDiagnosticBag.GetInstance();
					CheckBase(instance3);
					AddDeclarationDiagnostics(instance3);
					state.NotePartComplete(CompletionPart.FinishBaseType);
					instance3.Free();
				}
				break;
			case CompletionPart.StartInterfaces:
			case CompletionPart.FinishInterfaces:
				if (state.NotePartComplete(CompletionPart.StartInterfaces))
				{
					BindingDiagnosticBag instance4 = BindingDiagnosticBag.GetInstance();
					CheckInterfaces(instance4);
					AddDeclarationDiagnostics(instance4);
					state.NotePartComplete(CompletionPart.FinishInterfaces);
					instance4.Free();
				}
				break;
			case CompletionPart.EnumUnderlyingType:
				_ = EnumUnderlyingType;
				break;
			case CompletionPart.TypeArguments:
				_ = TypeArgumentsWithAnnotationsNoUseSiteDiagnostics;
				break;
			case CompletionPart.TypeParameters:
				foreach (TypeParameterSymbol typeParameter in TypeParameters)
				{
					typeParameter.ForceComplete(locationOpt, null, cancellationToken);
				}
				state.NotePartComplete(CompletionPart.TypeParameters);
				break;
			case CompletionPart.Members:
				GetMembersByName();
				if (IsExtension)
				{
					((SourceNamedTypeSymbol)this).TryGetOrCreateExtensionMarker();
				}
				break;
			case CompletionPart.TypeMembers:
				GetTypeMembersUnordered();
				break;
			case CompletionPart.SynthesizedExplicitImplementations:
				GetSynthesizedExplicitImplementations(cancellationToken);
				break;
			case CompletionPart.StartMemberChecks:
			case CompletionPart.FinishMemberChecks:
				if (state.NotePartComplete(CompletionPart.StartMemberChecks))
				{
					BindingDiagnosticBag instance2 = BindingDiagnosticBag.GetInstance();
					AfterMembersChecks(instance2);
					AddDeclarationDiagnostics(instance2);
					DeclaringCompilation.SymbolDeclaredEvent(this);
					state.NotePartComplete(CompletionPart.FinishMemberChecks);
					instance2.Free();
				}
				break;
			case CompletionPart.MembersCompletedChecksStarted:
			case CompletionPart.MembersCompleted:
			{
				if (IsExtension)
				{
					((SourceNamedTypeSymbol)this).TryGetOrCreateExtensionMarker()?.ForceComplete(locationOpt, null, cancellationToken);
				}
				ImmutableArray<Symbol> membersUnordered = GetMembersUnordered();
				bool flag = true;
				if (locationOpt == null && filter == null)
				{
					foreach (Symbol item in membersUnordered)
					{
						cancellationToken.ThrowIfCancellationRequested();
						item.ForceComplete(locationOpt, null, cancellationToken);
					}
				}
				else
				{
					foreach (Symbol item2 in membersUnordered)
					{
						Symbol.ForceCompleteMemberConditionally(locationOpt, filter, item2, cancellationToken);
						flag = flag && item2.HasComplete(CompletionPart.All);
					}
				}
				if (!flag)
				{
					CompletionPart part = CompletionPart.NamedTypeSymbolWithLocationAll;
					state.SpinWaitComplete(part, cancellationToken);
					return;
				}
				EnsureFieldDefinitionsNoted();
				cancellationToken.ThrowIfCancellationRequested();
				if (state.NotePartComplete(CompletionPart.MembersCompletedChecksStarted))
				{
					BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
					AfterMembersCompletedChecks(instance);
					AddDeclarationDiagnostics(instance);
					state.NotePartComplete(CompletionPart.MembersCompleted);
					instance.Free();
				}
				break;
			}
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete(CompletionPart.ReturnTypeAttributes | CompletionPart.Parameters | CompletionPart.Type);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	internal void EnsureFieldDefinitionsNoted()
	{
		if (!_flags.FieldDefinitionsNoted)
		{
			NoteFieldDefinitions();
		}
	}

	private void NoteFieldDefinitions()
	{
		MembersAndInitializers membersAndInitializers = GetMembersAndInitializers();
		lock (membersAndInitializers)
		{
			if (_flags.FieldDefinitionsNoted)
			{
				return;
			}
			SourceAssemblySymbol sourceAssemblySymbol = (SourceAssemblySymbol)ContainingAssembly;
			Accessibility accessibility = EffectiveAccessibility();
			foreach (Symbol nonTypeMember in membersAndInitializers.NonTypeMembers)
			{
				if (nonTypeMember.IsFieldOrFieldLikeEvent(out var field) && !field.IsConst && !field.IsFixedSizeBuffer)
				{
					Accessibility declaredAccessibility = field.DeclaredAccessibility;
					if (declaredAccessibility == Accessibility.Private)
					{
						sourceAssemblySymbol.NoteFieldDefinition(field, isInternal: false, isUnread: true);
					}
					else if (accessibility == Accessibility.Private)
					{
						sourceAssemblySymbol.NoteFieldDefinition(field, isInternal: false, isUnread: false);
					}
					else if (declaredAccessibility == Accessibility.Internal || accessibility == Accessibility.Internal)
					{
						sourceAssemblySymbol.NoteFieldDefinition(field, isInternal: true, isUnread: false);
					}
				}
			}
			_flags.SetFieldDefinitionsNoted();
		}
	}

	internal override ManagedKind GetManagedKind(ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ManagedKind managedKind = _flags.ManagedKind;
		if (managedKind == ManagedKind.Unknown)
		{
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2 = new CompoundUseSiteInfo<AssemblySymbol>(ContainingAssembly);
			managedKind = base.GetManagedKind(ref useSiteInfo2);
			ImmutableInterlocked.InterlockedInitialize(ref _managedKindUseSiteDiagnostics, useSiteInfo2.Diagnostics?.ToImmutableArray() ?? ImmutableArray<DiagnosticInfo>.Empty);
			ImmutableInterlocked.InterlockedInitialize(ref _managedKindUseSiteDependencies, useSiteInfo2.Dependencies?.ToImmutableArray() ?? ImmutableArray<AssemblySymbol>.Empty);
			_flags.SetManagedKind(managedKind);
		}
		if (useSiteInfo.AccumulatesDiagnostics)
		{
			ImmutableArray<DiagnosticInfo> managedKindUseSiteDiagnostics = _managedKindUseSiteDiagnostics;
			managedKindUseSiteDiagnostics = ImmutableInterlocked.InterlockedCompareExchange(ref _managedKindUseSiteDiagnostics, managedKindUseSiteDiagnostics, managedKindUseSiteDiagnostics);
			useSiteInfo.AddDiagnostics(managedKindUseSiteDiagnostics);
		}
		if (useSiteInfo.AccumulatesDependencies)
		{
			ImmutableArray<AssemblySymbol> managedKindUseSiteDependencies = _managedKindUseSiteDependencies;
			managedKindUseSiteDependencies = ImmutableInterlocked.InterlockedCompareExchange(ref _managedKindUseSiteDependencies, managedKindUseSiteDependencies, managedKindUseSiteDependencies);
			useSiteInfo.AddDependencies(managedKindUseSiteDependencies);
		}
		return managedKind;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool HasFlag(DeclarationModifiers flag)
	{
		return (_declModifiers & flag) != 0;
	}

	private Accessibility EffectiveAccessibility()
	{
		Accessibility accessibility = DeclaredAccessibility;
		if (accessibility == Accessibility.Private)
		{
			return Accessibility.Private;
		}
		Symbol containingType = ContainingType;
		while ((object)containingType != null)
		{
			switch (containingType.DeclaredAccessibility)
			{
			case Accessibility.Private:
				return Accessibility.Private;
			case Accessibility.Internal:
				accessibility = Accessibility.Internal;
				break;
			}
			containingType = containingType.ContainingType;
		}
		return accessibility;
	}

	internal override LexicalSortKey GetLexicalSortKey()
	{
		if (!_lazyLexicalSortKey.IsInitialized)
		{
			_lazyLexicalSortKey.SetFrom(declaration.GetLexicalSortKey(DeclaringCompilation));
		}
		return _lazyLexicalSortKey;
	}

	public override Location TryGetFirstLocation()
	{
		return declaration.Declarations[0].NameLocation;
	}

	public override bool IsDefinedInSourceTree(SyntaxTree tree, TextSpan? definedWithinSpan, CancellationToken cancellationToken)
	{
		ImmutableArray<SingleTypeDeclaration> declarations = declaration.Declarations;
		if (IsImplicitlyDeclared && declarations.IsEmpty)
		{
			return ContainingSymbol.IsDefinedInSourceTree(tree, definedWithinSpan, cancellationToken);
		}
		foreach (SingleTypeDeclaration item in declarations)
		{
			cancellationToken.ThrowIfCancellationRequested();
			SyntaxReference syntaxReference = item.SyntaxReference;
			if (syntaxReference.SyntaxTree == tree && (!definedWithinSpan.HasValue || syntaxReference.Span.IntersectsWith(definedWithinSpan.Value)))
			{
				return true;
			}
		}
		return false;
	}

	internal int CalculateSyntaxOffsetInSynthesizedConstructor(int position, SyntaxTree tree, bool isStatic)
	{
		if (IsScriptClass && !isStatic)
		{
			int num = 0;
			foreach (SingleTypeDeclaration declaration in declaration.Declarations)
			{
				SyntaxReference syntaxReference = declaration.SyntaxReference;
				if (tree == syntaxReference.SyntaxTree)
				{
					return num + position;
				}
				num += syntaxReference.Span.Length;
			}
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/SourceMemberContainerSymbol.cs", 1131);
		}
		if (TryCalculateSyntaxOffsetOfPositionInInitializer(position, tree, isStatic, 0, out var syntaxOffset))
		{
			return syntaxOffset;
		}
		if (this.declaration.Declarations.Length >= 1 && position == this.declaration.Declarations[0].Location.SourceSpan.Start)
		{
			return 0;
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/SourceMemberContainerSymbol.cs", 1151);
	}

	internal bool TryCalculateSyntaxOffsetOfPositionInInitializer(int position, SyntaxTree tree, bool isStatic, int ctorInitializerLength, out int syntaxOffset)
	{
		MembersAndInitializers membersAndInitializers = GetMembersAndInitializers();
		ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> initializers = (isStatic ? membersAndInitializers.StaticInitializers : membersAndInitializers.InstanceInitializers);
		if (!findInitializer(initializers, position, tree, out var found, out var precedingLength))
		{
			syntaxOffset = 0;
			return false;
		}
		int num = getInitializersLength(initializers);
		int num2 = position - found.Syntax.Span.Start;
		int num3 = num + ctorInitializerLength - (precedingLength + num2);
		syntaxOffset = -num3;
		return true;
		static bool findInitializer(ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> immutableArray, int num4, SyntaxTree syntaxTree, out FieldOrPropertyInitializer reference2, out int reference)
		{
			reference = 0;
			foreach (ImmutableArray<FieldOrPropertyInitializer> item in immutableArray)
			{
				if (!item.IsEmpty && item[0].Syntax.SyntaxTree == syntaxTree && num4 < item.Last().Syntax.Span.End)
				{
					int num5 = IndexOfInitializerContainingPosition(item, num4);
					if (num5 < 0)
					{
						break;
					}
					reference += getPrecedingInitializersLength(item, num5);
					reference2 = item[num5];
					return true;
				}
				reference += getGroupLength(item);
			}
			reference2 = default(FieldOrPropertyInitializer);
			return false;
		}
		static int getGroupLength(ImmutableArray<FieldOrPropertyInitializer> immutableArray)
		{
			int num4 = 0;
			foreach (FieldOrPropertyInitializer item2 in immutableArray)
			{
				num4 += getInitializerLength(item2);
			}
			return num4;
		}
		static int getInitializerLength(FieldOrPropertyInitializer initializer)
		{
			if (initializer.FieldOpt == null || !initializer.FieldOpt.IsMetadataConstant)
			{
				return initializer.Syntax.Span.Length;
			}
			return 0;
		}
		static int getInitializersLength(ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> immutableArray)
		{
			int num4 = 0;
			foreach (ImmutableArray<FieldOrPropertyInitializer> item3 in immutableArray)
			{
				num4 += getGroupLength(item3);
			}
			return num4;
		}
		static int getPrecedingInitializersLength(ImmutableArray<FieldOrPropertyInitializer> immutableArray, int index)
		{
			int num4 = 0;
			for (int i = 0; i < index; i++)
			{
				num4 += getInitializerLength(immutableArray[i]);
			}
			return num4;
		}
	}

	private static int IndexOfInitializerContainingPosition(ImmutableArray<FieldOrPropertyInitializer> initializers, int position)
	{
		int num = initializers.BinarySearch(position, (FieldOrPropertyInitializer initializer, int pos) => initializer.Syntax.Span.Start.CompareTo(pos));
		if (num >= 0)
		{
			return num;
		}
		int num2 = ~num - 1;
		if (num2 >= 0 && initializers[num2].Syntax.Span.Contains(position))
		{
			return num2;
		}
		return -1;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetTypeMembersUnordered()
	{
		return GetTypeMembersDictionary().Flatten();
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		return GetTypeMembersDictionary().Flatten(LexicalOrderSymbolComparer.Instance);
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name)
	{
		if (GetTypeMembersDictionary().TryGetValue(name, out ImmutableArray<NamedTypeSymbol> value))
		{
			return value;
		}
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name, int arity)
	{
		return GetTypeMembers(name).WhereAsArray((NamedTypeSymbol t, int num) => t.Arity == num, arity);
	}

	private Dictionary<ReadOnlyMemory<char>, ImmutableArray<NamedTypeSymbol>> GetTypeMembersDictionary()
	{
		if (_lazyTypeMembers == null)
		{
			BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
			if (Interlocked.CompareExchange(ref _lazyTypeMembers, MakeTypeMembers(instance), null) == null)
			{
				AddDeclarationDiagnostics(instance);
				state.NotePartComplete(CompletionPart.TypeMembers);
			}
			instance.Free();
		}
		return _lazyTypeMembers;
	}

	private Dictionary<ReadOnlyMemory<char>, ImmutableArray<NamedTypeSymbol>> MakeTypeMembers(BindingDiagnosticBag diagnostics)
	{
		ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		Dictionary<(string, int, SyntaxTree), SourceNamedTypeSymbol> dictionary = new Dictionary<(string, int, SyntaxTree), SourceNamedTypeSymbol>();
		try
		{
			foreach (MergedTypeDeclaration child in declaration.Children)
			{
				SourceNamedTypeSymbol sourceNamedTypeSymbol = new SourceNamedTypeSymbol(this, child, diagnostics);
				if (!sourceNamedTypeSymbol.IsExtension)
				{
					CheckMemberNameDistinctFromType(sourceNamedTypeSymbol, diagnostics);
					(string, int, SyntaxTree) key = (sourceNamedTypeSymbol.Name, sourceNamedTypeSymbol.Arity, sourceNamedTypeSymbol.AssociatedSyntaxTree);
					if (dictionary.TryGetValue(key, out var value))
					{
						if (Locations.Length == 1 || IsPartial)
						{
							if (sourceNamedTypeSymbol.IsPartial && value.IsPartial)
							{
								diagnostics.Add(ErrorCode.ERR_PartialTypeKindConflict, sourceNamedTypeSymbol.GetFirstLocation(), sourceNamedTypeSymbol);
							}
							else
							{
								diagnostics.Add(ErrorCode.ERR_DuplicateNameInClass, sourceNamedTypeSymbol.GetFirstLocation(), this, sourceNamedTypeSymbol.Name);
							}
						}
					}
					else
					{
						dictionary.Add(key, sourceNamedTypeSymbol);
					}
				}
				instance.Add(sourceNamedTypeSymbol);
			}
			if (IsInterface)
			{
				foreach (NamedTypeSymbol item in instance)
				{
					Binder.CheckFeatureAvailability(item.DeclaringSyntaxReferences[0].GetSyntax(), MessageID.IDS_DefaultInterfaceImplementation, diagnostics, item.GetFirstLocation());
				}
			}
			return (instance.Count > 0) ? instance.ToDictionary((NamedTypeSymbol s) => System.MemoryExtensions.AsMemory(s.Name), ReadOnlyMemoryOfCharComparer.Instance) : s_emptyTypeMembers;
		}
		finally
		{
			instance.Free();
		}
	}

	private void CheckMemberNameDistinctFromType(Symbol member, BindingDiagnosticBag diagnostics)
	{
		switch (TypeKind)
		{
		case TypeKind.Class:
		case TypeKind.Struct:
			checkContainingTypeName(member, Name, diagnostics);
			break;
		case TypeKind.Interface:
			if (member.IsStatic)
			{
				checkContainingTypeName(member, Name, diagnostics);
			}
			break;
		case TypeKind.Extension:
		{
			if (member.Kind != SymbolKind.Method)
			{
				NamedTypeSymbol containingType = ContainingType;
				if ((object)containingType != null)
				{
					checkContainingTypeName(member, containingType.Name, diagnostics);
				}
			}
			ParameterSymbol extensionParameter = ExtensionParameter;
			if ((object)extensionParameter != null && extensionParameter.Type is NamedTypeSymbol { Name: var name })
			{
				checkExtendedTypeName(member, name, diagnostics);
			}
			break;
		}
		}
		static void checkContainingTypeName(Symbol symbol, string typeName, BindingDiagnosticBag bindingDiagnosticBag)
		{
			if (symbol.Name == typeName)
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_MemberNameSameAsType, symbol.GetFirstLocation(), typeName);
			}
		}
		static void checkExtendedTypeName(Symbol symbol, string typeName, BindingDiagnosticBag bindingDiagnosticBag)
		{
			if (symbol.Name == typeName)
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_MemberNameSameAsExtendedType, symbol.GetFirstLocation(), typeName);
			}
		}
	}

	internal override bool HasAsyncMethodBuilderAttribute(out TypeSymbol? builderArgument)
	{
		return HasAsyncMethodBuilderAttribute(this, out builderArgument);
	}

	internal static bool HasAsyncMethodBuilderAttribute(Symbol symbol, [NotNullWhen(true)] out TypeSymbol? builderArgument)
	{
		foreach (CSharpAttributeData attribute in symbol.GetAttributes())
		{
			if (attribute.IsTargetAttribute(AttributeDescription.AsyncMethodBuilderAttribute) && attribute.CommonConstructorArguments.Length == 1 && attribute.CommonConstructorArguments[0].Kind == TypedConstantKind.Type)
			{
				builderArgument = (TypeSymbol)attribute.CommonConstructorArguments[0].ValueInternal;
				return true;
			}
		}
		builderArgument = null;
		return false;
	}

	internal override ImmutableArray<Symbol> GetMembersUnordered()
	{
		ImmutableArray<Symbol> lazyMembersFlattened = _lazyMembersFlattened;
		if (lazyMembersFlattened.IsDefault)
		{
			lazyMembersFlattened = GetMembersByName().Flatten();
			ImmutableInterlocked.InterlockedInitialize(ref _lazyMembersFlattened, lazyMembersFlattened);
			lazyMembersFlattened = _lazyMembersFlattened;
		}
		return lazyMembersFlattened.ConditionallyDeOrder();
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (_flags.FlattenedMembersIsSorted)
		{
			return _lazyMembersFlattened;
		}
		ImmutableArray<Symbol> immutableArray = GetMembersUnordered();
		if (immutableArray.Length > 1)
		{
			immutableArray = immutableArray.Sort(LexicalOrderSymbolComparer.Instance);
			ImmutableInterlocked.InterlockedExchange(ref _lazyMembersFlattened, immutableArray);
		}
		_flags.SetFlattenedMembersIsSorted();
		return immutableArray;
	}

	public sealed override ImmutableArray<Symbol> GetMembers(string name)
	{
		if (GetMembersByName().TryGetValue(System.MemoryExtensions.AsMemory(name), out ImmutableArray<Symbol> value))
		{
			return value;
		}
		return ImmutableArray<Symbol>.Empty;
	}

	internal override bool HasPossibleWellKnownCloneMethod()
	{
		return IsRecord;
	}

	internal override ImmutableArray<Symbol> GetSimpleNonTypeMembers(string name)
	{
		bool flag = _lazyMembersDictionary != null || declaration.ContainsExtensionDeclarations || declaration.MemberNames.Contains(name);
		if (!flag)
		{
			DeclarationKind kind = declaration.Kind;
			bool flag2 = kind - 9 <= DeclarationKind.Class;
			flag = flag2;
		}
		if (flag)
		{
			return GetMembers(name);
		}
		return ImmutableArray<Symbol>.Empty;
	}

	internal override IEnumerable<FieldSymbol> GetFieldsToEmit()
	{
		if (TypeKind == TypeKind.Enum)
		{
			yield return ((SourceNamedTypeSymbol)this).EnumValueField;
		}
		foreach (Symbol member in GetMembers())
		{
			switch (member.Kind)
			{
			case SymbolKind.Field:
				if (!(member is TupleErrorFieldSymbol))
				{
					yield return (FieldSymbol)member;
				}
				break;
			case SymbolKind.Event:
			{
				FieldSymbol associatedField = ((EventSymbol)member).AssociatedField;
				if ((object)associatedField != null)
				{
					yield return associatedField;
				}
				break;
			}
			}
		}
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers()
	{
		return GetEarlyAttributeDecodingMembersDictionary().Flatten();
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers(string name)
	{
		if (!GetEarlyAttributeDecodingMembersDictionary().TryGetValue(System.MemoryExtensions.AsMemory(name), out ImmutableArray<Symbol> value))
		{
			return ImmutableArray<Symbol>.Empty;
		}
		return value;
	}

	private Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> GetEarlyAttributeDecodingMembersDictionary()
	{
		if (_lazyEarlyAttributeDecodingMembersDictionary == null)
		{
			Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> dictionary = Volatile.Read(in _lazyMembersDictionary);
			if (dictionary != null)
			{
				return dictionary;
			}
			MembersAndInitializers membersAndInitializers = GetMembersAndInitializers();
			Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> dictionary2 = (membersAndInitializers.HaveIndexers ? ToNameKeyedDictionary(membersAndInitializers.NonTypeMembers.WhereAsArray(delegate(Symbol s)
			{
				if (!s.IsIndexer())
				{
					if (s.IsAccessor())
					{
						Symbol associatedSymbol = ((MethodSymbol)s).AssociatedSymbol;
						if ((object)associatedSymbol == null)
						{
							return true;
						}
						return !associatedSymbol.IsIndexer();
					}
					return true;
				}
				return false;
			})) : ToNameKeyedDictionary(membersAndInitializers.NonTypeMembers));
			AddNestedTypesToDictionary(dictionary2, GetTypeMembersDictionary());
			Interlocked.CompareExchange(ref _lazyEarlyAttributeDecodingMembersDictionary, dictionary2, null);
		}
		return _lazyEarlyAttributeDecodingMembersDictionary;
	}

	private static Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> ToNameKeyedDictionary(ImmutableArray<Symbol> symbols)
	{
		if (symbols.Length == 1)
		{
			Symbol symbol = symbols[0];
			return new Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>>(1, ReadOnlyMemoryOfCharComparer.Instance) { 
			{
				System.MemoryExtensions.AsMemory(symbol.Name),
				ImmutableArray.Create(symbol)
			} };
		}
		if (symbols.Length == 0)
		{
			return new Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>>(ReadOnlyMemoryOfCharComparer.Instance);
		}
		PooledDictionary<ReadOnlyMemory<char>, object> pooledDictionary = NamespaceOrTypeSymbol.s_nameToObjectPool.Allocate();
		foreach (Symbol item in symbols)
		{
			ImmutableArrayExtensions.AddToMultiValueDictionaryBuilder(pooledDictionary, System.MemoryExtensions.AsMemory(item.Name), item);
		}
		Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> dictionary = new Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>>(pooledDictionary.Count, ReadOnlyMemoryOfCharComparer.Instance);
		foreach (KeyValuePair<ReadOnlyMemory<char>, object> item2 in pooledDictionary)
		{
			dictionary.Add(item2.Key, (item2.Value is ArrayBuilder<Symbol> arrayBuilder) ? arrayBuilder.ToImmutableAndFree() : ImmutableArray.Create((Symbol)item2.Value));
		}
		pooledDictionary.Free();
		return dictionary;
	}

	protected MembersAndInitializers GetMembersAndInitializers()
	{
		MembersAndInitializers lazyMembersAndInitializers = _lazyMembersAndInitializers;
		if (lazyMembersAndInitializers != null)
		{
			return lazyMembersAndInitializers;
		}
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
		lazyMembersAndInitializers = BuildMembersAndInitializers(instance);
		MembersAndInitializers membersAndInitializers = Interlocked.CompareExchange(ref _lazyMembersAndInitializers, lazyMembersAndInitializers, null);
		if (membersAndInitializers != null)
		{
			instance.Free();
			return membersAndInitializers;
		}
		AddDeclarationDiagnostics(instance);
		instance.Free();
		_lazyDeclaredMembersAndInitializers = null;
		return lazyMembersAndInitializers;
	}

	[Conditional("DEBUG")]
	internal void AssertMemberExposure(Symbol member, bool forDiagnostics = false)
	{
		if (member is NamedTypeSymbol || member is TypeParameterSymbol || member is SynthesizedMethodBaseSymbol)
		{
			return;
		}
		if (member is FieldSymbol { AssociatedSymbol: EventSymbol associatedSymbol })
		{
			member = associatedSymbol;
		}
		else
		{
			if (member is FieldSymbol { AssociatedSymbol: SourcePropertySymbolBase associatedSymbol2 })
			{
				PropertySymbol partialDefinitionPart = associatedSymbol2.PartialDefinitionPart;
				if ((object)partialDefinitionPart != null && (object)partialDefinitionPart.PartialImplementationPart == associatedSymbol2 && (object)associatedSymbol2.BackingField != member)
				{
					member = associatedSymbol2;
					goto IL_00bf;
				}
			}
			if (member is SynthesizedExtensionMarker)
			{
				return;
			}
		}
		goto IL_00bf;
		IL_00bf:
		MembersAndInitializers membersAndInitializers = Volatile.Read(in _lazyMembersAndInitializers);
		if (isMemberInCompleteMemberList(membersAndInitializers, member) || membersAndInitializers != null || member is SynthesizedSimpleProgramEntryPointSymbol)
		{
			return;
		}
		DeclaredMembersAndInitializers declaredMembersAndInitializers = Volatile.Read(in _lazyDeclaredMembersAndInitializers);
		if (declaredMembersAndInitializers != null)
		{
			if (!declaredMembersAndInitializers.NonTypeMembersWithPartialImplementations.Contains((Symbol m) => (object)m == member))
			{
				_ = declaredMembersAndInitializers.PrimaryConstructor;
				_ = member;
			}
		}
		else
		{
			membersAndInitializers = Volatile.Read(in _lazyMembersAndInitializers);
			isMemberInCompleteMemberList(membersAndInitializers, member);
		}
		static bool isMemberInCompleteMemberList(MembersAndInitializers? membersAndInitializers2, Symbol symbol)
		{
			symbol = symbol.GetPartialDefinitionPart() ?? symbol;
			return membersAndInitializers2?.NonTypeMembers.Contains((Symbol m) => (object)m == symbol) ?? false;
		}
	}

	protected Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> GetMembersByName()
	{
		if (state.HasComplete(CompletionPart.Members))
		{
			return _lazyMembersDictionary;
		}
		return GetMembersByNameSlow();
	}

	private Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> GetMembersByNameSlow()
	{
		if (_lazyMembersDictionary == null)
		{
			Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> value = MakeAllMembers();
			if (Interlocked.CompareExchange(ref _lazyMembersDictionary, value, null) == null)
			{
				state.NotePartComplete(CompletionPart.Members);
			}
		}
		state.SpinWaitComplete(CompletionPart.Members, default(CancellationToken));
		return _lazyMembersDictionary;
	}

	internal override IEnumerable<Symbol> GetInstanceFieldsAndEvents()
	{
		return GetMembersAndInitializers().NonTypeMembers.Where(NamedTypeSymbol.IsInstanceFieldOrEvent);
	}

	protected void AfterMembersChecks(BindingDiagnosticBag diagnostics)
	{
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		Location firstLocation = GetFirstLocation();
		if (IsInterface)
		{
			CheckInterfaceMembers(GetMembersAndInitializers().NonTypeMembers, diagnostics);
		}
		else if (IsExtension)
		{
			CheckExtensionMembers(GetMembers(), diagnostics);
			MessageID.IDS_FeatureExtensions.CheckFeatureAvailability(diagnostics, declaringCompilation, firstLocation);
		}
		CheckMemberNamesDistinctFromType(diagnostics);
		CheckMemberNameConflictsAndUnmatchedOperators(diagnostics);
		CheckRecordMemberNames(diagnostics);
		CheckSpecialMemberErrors(diagnostics);
		CheckTypeParameterNameConflicts(diagnostics);
		_ = KnownCircularStruct;
		CheckSequentialOnPartialType(diagnostics);
		CheckForProtectedInStaticClass(diagnostics);
		CheckForRequiredMemberAttribute(diagnostics);
		if (IsScriptClass || base.IsSubmissionClass)
		{
			ReportRequiredMembers(diagnostics);
		}
		if (IsRefLikeType)
		{
			declaringCompilation.EnsureIsByRefLikeAttributeExists(diagnostics, firstLocation, modifyCompilation: true);
		}
		if (IsReadOnly)
		{
			declaringCompilation.EnsureIsReadOnlyAttributeExists(diagnostics, firstLocation, modifyCompilation: true);
		}
		NamedTypeSymbol baseType = BaseTypeNoUseSiteDiagnostics;
		ImmutableArray<NamedTypeSymbol> interfaces = GetInterfacesToEmit();
		if (declaringCompilation.ShouldEmitNativeIntegerAttributes() && hasBaseTypeOrInterface((NamedTypeSymbol t) => t.ContainsNativeIntegerWrapperType()))
		{
			declaringCompilation.EnsureNativeIntegerAttributeExists(diagnostics, firstLocation, modifyCompilation: true);
		}
		if (declaringCompilation.ShouldEmitNullableAttributes(this))
		{
			if (ShouldEmitNullableContextValue(out var _))
			{
				declaringCompilation.EnsureNullableContextAttributeExists(diagnostics, firstLocation, modifyCompilation: true);
			}
			if (hasBaseTypeOrInterface((NamedTypeSymbol t) => t.NeedsNullableAttribute()))
			{
				declaringCompilation.EnsureNullableAttributeExists(diagnostics, firstLocation, modifyCompilation: true);
			}
		}
		if (interfaces.Any(needsTupleElementNamesAttribute))
		{
			Binder.ReportMissingTupleElementNamesAttributesIfNeeded(declaringCompilation, firstLocation, diagnostics);
		}
		if (IsReservedTypeName(Name))
		{
			foreach (SyntaxReference syntaxReference in SyntaxReferences)
			{
				SyntaxNode syntax = syntaxReference.GetSyntax();
				SyntaxToken? syntaxToken = ((syntax is BaseTypeDeclarationSyntax baseTypeDeclarationSyntax) ? new SyntaxToken?(baseTypeDeclarationSyntax.Identifier) : ((!(syntax is DelegateDeclarationSyntax delegateDeclarationSyntax)) ? ((SyntaxToken?)null) : new SyntaxToken?(delegateDeclarationSyntax.Identifier)));
				SyntaxToken? syntaxToken2 = syntaxToken;
				ReportReservedTypeName(syntaxToken2?.Text, DeclaringCompilation, diagnostics.DiagnosticBag, syntaxToken2?.GetLocation() ?? Location.None);
			}
		}
		FileIdentifier associatedFileIdentifier = AssociatedFileIdentifier;
		if (associatedFileIdentifier != null)
		{
			_ = declaration.Declarations[0].SyntaxReference.SyntaxTree;
			string encoderFallbackErrorMessage = associatedFileIdentifier.EncoderFallbackErrorMessage;
			if (encoderFallbackErrorMessage != null)
			{
				diagnostics.Add(ErrorCode.ERR_FilePathCannotBeConvertedToUtf8, firstLocation, this, encoderFallbackErrorMessage);
			}
			if ((object)ContainingType != null)
			{
				diagnostics.Add(ErrorCode.ERR_FileTypeNested, firstLocation, this);
			}
		}
		if (IsExtension)
		{
			SourceOrdinaryMethodSymbol.CheckExtensionAttributeAvailability(DeclaringCompilation, firstLocation, diagnostics);
		}
		bool hasBaseTypeOrInterface(Func<NamedTypeSymbol, bool> predicate)
		{
			if ((object)baseType == null || !predicate(baseType))
			{
				return interfaces.Any(predicate);
			}
			return true;
		}
		static bool needsTupleElementNamesAttribute(TypeSymbol type)
		{
			if ((object)type == null)
			{
				return false;
			}
			return (object)type.VisitType((TypeSymbol t, object a, bool b) => !t.TupleElementNames.IsDefaultOrEmpty && !t.IsErrorType(), null) != null;
		}
	}

	protected virtual void AfterMembersCompletedChecks(BindingDiagnosticBag diagnostics)
	{
	}

	private void CheckMemberNamesDistinctFromType(BindingDiagnosticBag diagnostics)
	{
		foreach (Symbol nonTypeMember in GetMembersAndInitializers().NonTypeMembers)
		{
			CheckMemberNameDistinctFromType(nonTypeMember, diagnostics);
		}
	}

	private void CheckRecordMemberNames(BindingDiagnosticBag diagnostics)
	{
		if (declaration.Kind == DeclarationKind.Record || declaration.Kind == DeclarationKind.RecordStruct)
		{
			foreach (Symbol member in GetMembers("Clone"))
			{
				diagnostics.Add(ErrorCode.ERR_CloneDisallowedInRecord, member.GetFirstLocation());
			}
		}
	}

	private static void CheckMemberNameConflicts(SourceMemberContainerTypeSymbol containerForDiagnostics, bool mightHaveMembersFromDistinctNonPartialDeclarations, Dictionary<ReadOnlyMemory<char>, ImmutableArray<NamedTypeSymbol>>? typesByName, Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> membersByName, BindingDiagnosticBag diagnostics)
	{
		CheckIndexerNameConflicts(containerForDiagnostics, mightHaveMembersFromDistinctNonPartialDeclarations, diagnostics, membersByName);
		Dictionary<MethodSymbol, MethodSymbol> dictionary = new Dictionary<MethodSymbol, MethodSymbol>(MemberSignatureComparer.DuplicateSourceComparer);
		Dictionary<MethodSymbol, MethodSymbol> dictionary2 = new Dictionary<MethodSymbol, MethodSymbol>(MemberSignatureComparer.DuplicateSourceComparer);
		HashSet<MethodSymbol> hashSet = new HashSet<MethodSymbol>(ConversionSignatureComparer.Comparer);
		foreach (KeyValuePair<ReadOnlyMemory<char>, ImmutableArray<Symbol>> item in membersByName)
		{
			ReadOnlyMemory<char> key = item.Key;
			Symbol symbol = ((typesByName != null && typesByName.TryGetValue(key, out ImmutableArray<NamedTypeSymbol> value)) ? value.FirstOrDefault() : null);
			dictionary.Clear();
			foreach (Symbol item2 in item.Value)
			{
				if (item2.Kind == SymbolKind.NamedType || item2.IsAccessor() || item2.IsIndexer())
				{
					continue;
				}
				if ((object)symbol != null)
				{
					if (item2.Kind != SymbolKind.Method || symbol.Kind != SymbolKind.Method)
					{
						if ((item2.Kind != SymbolKind.Field || !item2.IsImplicitlyDeclared) && !mightHaveMembersFromDistinctNonPartialDeclarations)
						{
							diagnostics.Add(ErrorCode.ERR_DuplicateNameInClass, item2.GetFirstLocation(), containerForDiagnostics, item2.Name);
						}
						if (symbol.Kind == SymbolKind.Method)
						{
							symbol = item2;
						}
					}
				}
				else
				{
					symbol = item2;
				}
				if (item2 is MethodSymbol { MethodKind: MethodKind.Conversion } methodSymbol)
				{
					if (!hashSet.Add(methodSymbol))
					{
						diagnostics.Add(ErrorCode.ERR_DuplicateConversionInClass, methodSymbol.GetFirstLocation(), containerForDiagnostics);
					}
					else if (!dictionary2.ContainsKey(methodSymbol))
					{
						dictionary2.Add(methodSymbol, methodSymbol);
					}
					if (dictionary.TryGetValue(methodSymbol, out var value2))
					{
						ReportMethodSignatureCollision(containerForDiagnostics, diagnostics, methodSymbol, value2);
					}
				}
				else if (item2 is MethodSymbol methodSymbol2)
				{
					if (dictionary2.TryGetValue(methodSymbol2, out var value3))
					{
						ReportMethodSignatureCollision(containerForDiagnostics, diagnostics, methodSymbol2, value3);
					}
					if (dictionary.TryGetValue(methodSymbol2, out var value4))
					{
						ReportMethodSignatureCollision(containerForDiagnostics, diagnostics, methodSymbol2, value4);
					}
					else
					{
						dictionary.Add(methodSymbol2, methodSymbol2);
					}
				}
			}
		}
	}

	private static void ReportMethodSignatureCollision(SourceMemberContainerTypeSymbol containerForDiagnostics, BindingDiagnosticBag diagnostics, MethodSymbol method1, MethodSymbol method2)
	{
		SourceOrdinaryMethodSymbol sourceOrdinaryMethodSymbol2;
		if (method1 is SourceOrdinaryMethodSymbol sourceOrdinaryMethodSymbol)
		{
			if (sourceOrdinaryMethodSymbol.IsPartialDefinition)
			{
				sourceOrdinaryMethodSymbol2 = method2 as SourceOrdinaryMethodSymbol;
				if ((object)sourceOrdinaryMethodSymbol2 != null)
				{
					if (sourceOrdinaryMethodSymbol2.IsPartialImplementation)
					{
						return;
					}
					if (sourceOrdinaryMethodSymbol.IsPartialImplementation)
					{
						goto IL_004a;
					}
				}
			}
			else if (sourceOrdinaryMethodSymbol.IsPartialImplementation)
			{
				sourceOrdinaryMethodSymbol2 = method2 as SourceOrdinaryMethodSymbol;
				if ((object)sourceOrdinaryMethodSymbol2 != null)
				{
					goto IL_004a;
				}
			}
		}
		else if (method1 is SynthesizedSimpleProgramEntryPointSymbol && method2 is SynthesizedSimpleProgramEntryPointSymbol)
		{
			return;
		}
		goto IL_0069;
		IL_004a:
		if (!sourceOrdinaryMethodSymbol2.IsPartialDefinition)
		{
			goto IL_0069;
		}
		return;
		IL_0069:
		if (method1.OriginalDefinition is SourceMemberMethodSymbol { MethodKind: MethodKind.Constructor } sourceMemberMethodSymbol && ((ConstructorDeclarationSyntax)sourceMemberMethodSymbol.SyntaxRef.GetSyntax()).Identifier.ValueText != method1.ContainingType.Name)
		{
			return;
		}
		if (method1 is SourceExtensionImplementationMethodSymbol { UnderlyingMethod: var underlyingMethod } && method2 is SourceExtensionImplementationMethodSymbol { UnderlyingMethod: var underlyingMethod2 } && underlyingMethod.IsStatic == underlyingMethod2.IsStatic && ((object)underlyingMethod.ContainingType == underlyingMethod2.ContainingType || ((SourceNamedTypeSymbol)underlyingMethod.ContainingType).ExtensionGroupingName == ((SourceNamedTypeSymbol)underlyingMethod2.ContainingType).ExtensionGroupingName))
		{
			DiagnosticBag? diagnosticBag = diagnostics.DiagnosticBag;
			if (diagnosticBag != null && diagnosticBag.AsEnumerableWithoutResolution().Any(delegate(Diagnostic d, (MethodSymbol method1, MethodSymbol underlying1, MethodSymbol method2, MethodSymbol underlying2) arg)
			{
				bool flag;
				switch (d.Code)
				{
				case 82:
				case 102:
				case 111:
				case 663:
					flag = true;
					break;
				default:
					flag = false;
					break;
				}
				return flag && (d.Location == arg.method1.GetFirstLocation() || d.Location == arg.underlying1.AssociatedSymbol?.TryGetFirstLocation() || d.Location == arg.method2.GetFirstLocation() || d.Location == arg.underlying2.AssociatedSymbol?.TryGetFirstLocation());
			}, (method1, underlyingMethod, method2, underlyingMethod2)))
			{
				return;
			}
		}
		for (int num = 0; num < method1.ParameterCount; num++)
		{
			RefKind refKind = method1.Parameters[num].RefKind;
			RefKind refKind2 = method2.Parameters[num].RefKind;
			if (refKind != refKind2)
			{
				MessageID id = ((method1.MethodKind == MethodKind.Constructor) ? MessageID.IDS_SK_CONSTRUCTOR : MessageID.IDS_SK_METHOD);
				diagnostics.Add(ErrorCode.ERR_OverloadRefKind, method1.GetFirstLocation(), containerForDiagnostics, id.Localize(), refKind.ToParameterDisplayString(), refKind2.ToParameterDisplayString());
				return;
			}
		}
		if (method1 is SourceExtensionImplementationMethodSymbol sourceExtensionImplementationMethodSymbol3)
		{
			method1 = sourceExtensionImplementationMethodSymbol3.UnderlyingMethod;
		}
		string text = ((method1.MethodKind == MethodKind.Destructor && method2.MethodKind == MethodKind.Destructor) ? ("~" + method1.ContainingType.Name) : (method1.IsConstructor() ? method1.ContainingType.Name : method1.Name));
		diagnostics.Add(ErrorCode.ERR_MemberAlreadyExists, method1.GetFirstLocation(), text, containerForDiagnostics);
	}

	private static void CheckIndexerNameConflicts(SourceMemberContainerTypeSymbol containerForDiagnostics, bool mightHaveMembersFromDistinctNonPartialDeclarations, BindingDiagnosticBag diagnostics, Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> membersByName)
	{
		PooledHashSet<string> pooledHashSet = null;
		bool flag = true;
		Dictionary<PropertySymbol, PropertySymbol> dictionary = new Dictionary<PropertySymbol, PropertySymbol>(MemberSignatureComparer.DuplicateSourceComparer);
		foreach (ImmutableArray<Symbol> value in membersByName.Values)
		{
			string lastIndexerName = null;
			dictionary.Clear();
			foreach (Symbol item in value)
			{
				if (!item.IsIndexer())
				{
					continue;
				}
				PropertySymbol propertySymbol = (PropertySymbol)item;
				CheckIndexerSignatureCollisions(containerForDiagnostics, mightHaveMembersFromDistinctNonPartialDeclarations, propertySymbol, diagnostics, membersByName, dictionary, ref lastIndexerName);
				if (flag && pooledHashSet == null)
				{
					if (!propertySymbol.IsExtensionBlockMember() && propertySymbol.ContainingType.Arity > 0)
					{
						pooledHashSet = PooledHashSet<string>.GetInstance();
						foreach (TypeParameterSymbol typeParameter in propertySymbol.ContainingType.TypeParameters)
						{
							pooledHashSet.Add(typeParameter.Name);
						}
					}
					else
					{
						flag = false;
					}
				}
				if (pooledHashSet != null)
				{
					string metadataName = propertySymbol.MetadataName;
					if (pooledHashSet.Contains(metadataName))
					{
						diagnostics.Add(ErrorCode.ERR_DuplicateNameInClass, propertySymbol.GetFirstLocation(), containerForDiagnostics, metadataName);
					}
				}
			}
		}
		pooledHashSet?.Free();
	}

	private static void CheckIndexerSignatureCollisions(SourceMemberContainerTypeSymbol containerForDiagnostics, bool mightHaveMembersFromDistinctNonPartialDeclarations, PropertySymbol indexer, BindingDiagnosticBag diagnostics, Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> membersByName, Dictionary<PropertySymbol, PropertySymbol> indexersBySignature, ref string? lastIndexerName)
	{
		if (!indexer.IsExplicitInterfaceImplementation)
		{
			string metadataName = indexer.MetadataName;
			if (lastIndexerName != null && lastIndexerName != metadataName)
			{
				diagnostics.Add(ErrorCode.ERR_InconsistentIndexerNames, indexer.GetFirstLocation());
			}
			lastIndexerName = metadataName;
			if (!mightHaveMembersFromDistinctNonPartialDeclarations && membersByName.ContainsKey(System.MemoryExtensions.AsMemory(metadataName)))
			{
				diagnostics.Add(ErrorCode.ERR_DuplicateNameInClass, indexer.GetFirstLocation(), containerForDiagnostics, metadataName);
			}
		}
		if (indexersBySignature.TryGetValue(indexer, out PropertySymbol _))
		{
			diagnostics.Add(ErrorCode.ERR_MemberAlreadyExists, indexer.GetFirstLocation(), SyntaxFacts.GetText(SyntaxKind.ThisKeyword), containerForDiagnostics);
		}
		else
		{
			indexersBySignature[indexer] = indexer;
		}
	}

	private void CheckMemberNameConflictsAndUnmatchedOperators(BindingDiagnosticBag diagnostics)
	{
		if (!IsExtension)
		{
			if (declaration.ContainsExtensionDeclarations)
			{
				checkMemberNameConflictsInExtensions(diagnostics);
				GetExtensionGroupingInfo().CheckSignatureCollisions(diagnostics);
			}
			checkMemberNameConflicts(GetMembersByName(), GetTypeMembersDictionary(), GetMembersUnordered(), diagnostics);
			CheckForEqualityAndGetHashCode(diagnostics);
		}
		void checkMemberNameConflicts(Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> membersByName, Dictionary<ReadOnlyMemory<char>, ImmutableArray<NamedTypeSymbol>>? typesByName, ImmutableArray<Symbol> membersUnordered, BindingDiagnosticBag diagnostics2)
		{
			bool mightHaveMembersFromDistinctNonPartialDeclarations = Locations.Length != 1 && !IsPartial;
			CheckMemberNameConflicts(this, mightHaveMembersFromDistinctNonPartialDeclarations, typesByName, membersByName, diagnostics2);
			CheckAccessorNameConflicts(this, mightHaveMembersFromDistinctNonPartialDeclarations, membersByName, membersUnordered, diagnostics2);
			CheckForUnmatchedOperators(membersByName, diagnostics2);
		}
		void checkMemberNameConflictsInExtensions(BindingDiagnosticBag diagnostics2)
		{
			foreach (IGrouping<string, NamedTypeSymbol> item2 in from t in GetTypeMembers("")
				where t.IsExtension
				group t by ((SourceNamedTypeSymbol)t).ExtensionGroupingName)
			{
				var (dictionary, membersUnordered) = mergeMembersInGroup(item2);
				if (dictionary != null)
				{
					checkMemberNameConflicts(dictionary, null, membersUnordered, diagnostics2);
				}
			}
		}
		static ImmutableArray<Symbol> concatMembers(ImmutableArray<Symbol> existingMembers, NamedTypeSymbol extension, ImmutableArray<Symbol> newMembers, ref ImmutableArray<Symbol> membersUnordered)
		{
			if (extension.IsDefinition)
			{
				return existingMembers.Concat(newMembers);
			}
			ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(existingMembers.Length + newMembers.Length);
			ArrayBuilder<Symbol> instance2 = ArrayBuilder<Symbol>.GetInstance(membersUnordered.Length + newMembers.Length);
			instance.AddRange(existingMembers);
			instance2.AddRange(membersUnordered);
			foreach (Symbol item3 in newMembers)
			{
				Symbol item = item3.SymbolAsMember(extension);
				instance.Add(item);
				instance2.Add(item);
			}
			membersUnordered = instance2.ToImmutableAndFree();
			return instance.ToImmutableAndFree();
		}
		static (Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>>? membersByName, ImmutableArray<Symbol> membersUnordered) mergeMembersInGroup(IGrouping<string, NamedTypeSymbol> grouping)
		{
			Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> dictionary = null;
			ImmutableArray<Symbol> membersUnordered = ImmutableArray<Symbol>.Empty;
			NamedTypeSymbol namedTypeSymbol = null;
			bool flag = true;
			foreach (NamedTypeSymbol item4 in grouping)
			{
				NamedTypeSymbol namedTypeSymbol2 = item4;
				Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> membersByName = ((SourceMemberContainerTypeSymbol)namedTypeSymbol2).GetMembersByName();
				if (membersByName.Count != 0)
				{
					if (dictionary == null)
					{
						dictionary = membersByName;
						membersUnordered = namedTypeSymbol2.GetMembersUnordered();
						namedTypeSymbol = namedTypeSymbol2;
					}
					else
					{
						if (flag)
						{
							dictionary = new Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>>(dictionary, ReadOnlyMemoryOfCharComparer.Instance);
							flag = false;
						}
						if (namedTypeSymbol2.Arity != 0)
						{
							namedTypeSymbol2 = namedTypeSymbol2.Construct(namedTypeSymbol.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics);
						}
						else
						{
							membersUnordered = membersUnordered.Concat(namedTypeSymbol2.GetMembersUnordered());
						}
						foreach (KeyValuePair<ReadOnlyMemory<char>, ImmutableArray<Symbol>> item5 in membersByName)
						{
							if (dictionary.TryGetValue(item5.Key, out var value))
							{
								dictionary[item5.Key] = concatMembers(value, namedTypeSymbol2, item5.Value, ref membersUnordered);
							}
							else
							{
								dictionary.Add(item5.Key, concatMembers(ImmutableArray<Symbol>.Empty, namedTypeSymbol2, item5.Value, ref membersUnordered));
							}
						}
					}
				}
			}
			return (membersByName: dictionary, membersUnordered: membersUnordered);
		}
	}

	private void CheckSpecialMemberErrors(BindingDiagnosticBag diagnostics)
	{
		TypeConversions typeConversions = ContainingAssembly.CorLibrary.TypeConversions;
		if (IsExtension)
		{
			((SourceNamedTypeSymbol)this).TryGetOrCreateExtensionMarker()?.AfterAddingTypeMembersChecks(typeConversions, diagnostics);
		}
		foreach (Symbol item in GetMembersUnordered())
		{
			item.AfterAddingTypeMembersChecks(typeConversions, diagnostics);
		}
	}

	private void CheckTypeParameterNameConflicts(BindingDiagnosticBag diagnostics)
	{
		TypeKind typeKind = TypeKind;
		bool flag = ((typeKind == TypeKind.Delegate || typeKind == TypeKind.Extension) ? true : false);
		if (flag || (Locations.Length != 1 && !IsPartial))
		{
			return;
		}
		foreach (TypeParameterSymbol typeParameter in TypeParameters)
		{
			foreach (Symbol member in GetMembers(typeParameter.Name))
			{
				diagnostics.Add(ErrorCode.ERR_DuplicateNameInClass, member.GetFirstLocation(), this, typeParameter.Name);
			}
		}
	}

	private static void CheckAccessorNameConflicts(SourceMemberContainerTypeSymbol containerForDiagnostics, bool mightHaveMembersFromDistinctNonPartialDeclarations, Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> membersByName, ImmutableArray<Symbol> membersUnordered, BindingDiagnosticBag diagnostics)
	{
		foreach (Symbol item in membersUnordered)
		{
			if (!item.IsExplicitInterfaceImplementation())
			{
				switch (item.Kind)
				{
				case SymbolKind.Property:
				{
					PropertySymbol propertySymbol = (PropertySymbol)item;
					CheckForMemberConflictWithPropertyAccessor(containerForDiagnostics, mightHaveMembersFromDistinctNonPartialDeclarations, membersByName, propertySymbol, getNotSet: true, diagnostics);
					CheckForMemberConflictWithPropertyAccessor(containerForDiagnostics, mightHaveMembersFromDistinctNonPartialDeclarations, membersByName, propertySymbol, getNotSet: false, diagnostics);
					break;
				}
				case SymbolKind.Event:
				{
					EventSymbol eventSymbol = (EventSymbol)item;
					CheckForMemberConflictWithEventAccessor(containerForDiagnostics, mightHaveMembersFromDistinctNonPartialDeclarations, membersByName, eventSymbol, isAdder: true, diagnostics);
					CheckForMemberConflictWithEventAccessor(containerForDiagnostics, mightHaveMembersFromDistinctNonPartialDeclarations, membersByName, eventSymbol, isAdder: false, diagnostics);
					break;
				}
				}
			}
		}
	}

	private bool CheckStructCircularity(BindingDiagnosticBag diagnostics)
	{
		CheckFiniteFlatteningGraph(diagnostics);
		return HasStructCircularity(diagnostics);
	}

	private bool HasStructCircularity(BindingDiagnosticBag diagnostics)
	{
		foreach (ImmutableArray<Symbol> value in GetMembersByName().Values)
		{
			foreach (Symbol item in value)
			{
				SymbolKind kind = item.Kind;
				FieldSymbol fieldSymbol;
				if (kind != SymbolKind.Event)
				{
					if (kind != SymbolKind.Field)
					{
						continue;
					}
					fieldSymbol = (FieldSymbol)item;
				}
				else
				{
					fieldSymbol = ((EventSymbol)item).AssociatedField;
				}
				if ((object)fieldSymbol == null || fieldSymbol.IsStatic)
				{
					continue;
				}
				TypeSymbol typeSymbol = fieldSymbol.NonPointerType();
				if ((object)typeSymbol != null && typeSymbol.TypeKind == TypeKind.Struct && BaseTypeAnalysis.StructDependsOn((NamedTypeSymbol)typeSymbol, this) && !typeSymbol.IsPrimitiveRecursiveStruct())
				{
					if (fieldSymbol is SynthesizedPrimaryConstructorParameterBackingFieldSymbol synthesizedPrimaryConstructorParameterBackingFieldSymbol)
					{
						ParameterSymbol parameterSymbol = synthesizedPrimaryConstructorParameterBackingFieldSymbol.ParameterSymbol;
						diagnostics.Add(ErrorCode.ERR_StructLayoutCyclePrimaryConstructorParameter, parameterSymbol.GetFirstLocation(), parameterSymbol, typeSymbol);
					}
					else
					{
						Symbol symbol = fieldSymbol.AssociatedSymbol ?? fieldSymbol;
						diagnostics.Add(ErrorCode.ERR_StructLayoutCycle, symbol.GetFirstLocation(), symbol, typeSymbol);
					}
					return true;
				}
			}
		}
		return false;
	}

	private void CheckForProtectedInStaticClass(BindingDiagnosticBag diagnostics)
	{
		if (!IsStatic)
		{
			return;
		}
		foreach (ImmutableArray<Symbol> value in GetMembersByName().Values)
		{
			foreach (Symbol item in value)
			{
				if (!(item is TypeSymbol) && item.DeclaredAccessibility.HasProtected() && !(item is SourceExtensionImplementationMethodSymbol) && (item.Kind != SymbolKind.Method || ((MethodSymbol)item).MethodKind != MethodKind.Destructor))
				{
					diagnostics.Add(ErrorCode.ERR_ProtectedInStatic, item.GetFirstLocation(), item);
				}
			}
		}
	}

	private static void CheckForUnmatchedOperators(Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> membersByName, BindingDiagnosticBag diagnostics)
	{
		CheckForUnmatchedOperator(membersByName, diagnostics, "op_True", "op_False");
		CheckForUnmatchedOperator(membersByName, diagnostics, "op_Equality", "op_Inequality");
		CheckForUnmatchedOperator(membersByName, diagnostics, "op_LessThan", "op_GreaterThan");
		CheckForUnmatchedOperator(membersByName, diagnostics, "op_LessThanOrEqual", "op_GreaterThanOrEqual");
		CheckForUnmatchedOperator(membersByName, diagnostics, "op_CheckedDecrement", "op_Decrement", symmetricCheck: false);
		CheckForUnmatchedOperator(membersByName, diagnostics, "op_CheckedIncrement", "op_Increment", symmetricCheck: false);
		CheckForUnmatchedOperator(membersByName, diagnostics, "op_CheckedUnaryNegation", "op_UnaryNegation", symmetricCheck: false);
		CheckForUnmatchedOperator(membersByName, diagnostics, "op_CheckedAddition", "op_Addition", symmetricCheck: false);
		CheckForUnmatchedOperator(membersByName, diagnostics, "op_CheckedDivision", "op_Division", symmetricCheck: false);
		CheckForUnmatchedOperator(membersByName, diagnostics, "op_CheckedMultiply", "op_Multiply", symmetricCheck: false);
		CheckForUnmatchedOperator(membersByName, diagnostics, "op_CheckedSubtraction", "op_Subtraction", symmetricCheck: false);
		CheckForUnmatchedOperator(membersByName, diagnostics, "op_CheckedExplicit", "op_Explicit", symmetricCheck: false);
		CheckForUnmatchedOperator(membersByName, diagnostics, "op_CheckedAdditionAssignment", "op_AdditionAssignment", symmetricCheck: false);
		CheckForUnmatchedOperator(membersByName, diagnostics, "op_CheckedDivisionAssignment", "op_DivisionAssignment", symmetricCheck: false);
		CheckForUnmatchedOperator(membersByName, diagnostics, "op_CheckedMultiplicationAssignment", "op_MultiplicationAssignment", symmetricCheck: false);
		CheckForUnmatchedOperator(membersByName, diagnostics, "op_CheckedSubtractionAssignment", "op_SubtractionAssignment", symmetricCheck: false);
		CheckForUnmatchedOperator(membersByName, diagnostics, "op_CheckedDecrementAssignment", "op_DecrementAssignment", symmetricCheck: false);
		CheckForUnmatchedOperator(membersByName, diagnostics, "op_CheckedIncrementAssignment", "op_IncrementAssignment", symmetricCheck: false);
	}

	private static void CheckForUnmatchedOperator(Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> membersByName, BindingDiagnosticBag diagnostics, string operatorName1, string operatorName2, bool symmetricCheck = true)
	{
		ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
		addOperators(membersByName, operatorName1, instance);
		if (symmetricCheck)
		{
			ArrayBuilder<MethodSymbol> instance2 = ArrayBuilder<MethodSymbol>.GetInstance();
			addOperators(membersByName, operatorName2, instance2);
			CheckForUnmatchedOperator(diagnostics, instance, instance2, operatorName2, reportOperatorNeedsMatch);
			CheckForUnmatchedOperator(diagnostics, instance2, instance, operatorName1, reportOperatorNeedsMatch);
			instance2.Free();
		}
		else if (!instance.IsEmpty)
		{
			ArrayBuilder<MethodSymbol> instance3 = ArrayBuilder<MethodSymbol>.GetInstance();
			addOperators(membersByName, operatorName2, instance3);
			CheckForUnmatchedOperator(diagnostics, instance, instance3, operatorName2, reportCheckedOperatorNeedsMatch);
			instance3.Free();
		}
		instance.Free();
		static void addOperators(Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> dictionary, string text, ArrayBuilder<MethodSymbol> ops1)
		{
			if (dictionary.TryGetValue(System.MemoryExtensions.AsMemory(text), out ImmutableArray<Symbol> value))
			{
				NamedTypeSymbol.AddOperators(ops1, value);
			}
		}
		static void reportCheckedOperatorNeedsMatch(BindingDiagnosticBag bindingDiagnosticBag, string text, MethodSymbol op1)
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_CheckedOperatorNeedsMatch, op1.GetFirstLocation(), op1);
		}
		static void reportOperatorNeedsMatch(BindingDiagnosticBag bindingDiagnosticBag, string operatorMetadataName, MethodSymbol op1)
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_OperatorNeedsMatch, op1.GetFirstLocation(), op1, SyntaxFacts.GetText(SyntaxFacts.GetOperatorKind(operatorMetadataName)));
		}
	}

	private static void CheckForUnmatchedOperator(BindingDiagnosticBag diagnostics, ArrayBuilder<MethodSymbol> ops1, ArrayBuilder<MethodSymbol> ops2, string operatorName2, Action<BindingDiagnosticBag, string, MethodSymbol> reportMatchNotFoundError)
	{
		foreach (MethodSymbol item in ops1)
		{
			if (item.IsOverride)
			{
				continue;
			}
			bool flag = false;
			foreach (MethodSymbol item2 in ops2)
			{
				flag = DoOperatorsPair(item, item2);
				if (flag)
				{
					break;
				}
			}
			if (!flag)
			{
				reportMatchNotFoundError(diagnostics, operatorName2, item);
			}
		}
	}

	internal static bool DoOperatorsPair(MethodSymbol op1, MethodSymbol op2)
	{
		if (op1.ParameterCount != op2.ParameterCount)
		{
			return false;
		}
		for (int i = 0; i < op1.ParameterCount; i++)
		{
			if (!op1.ParameterTypesWithAnnotations[i].Equals(op2.ParameterTypesWithAnnotations[i], TypeCompareKind.AllIgnoreOptions))
			{
				return false;
			}
		}
		if (!op1.ReturnType.Equals(op2.ReturnType, TypeCompareKind.AllIgnoreOptions))
		{
			return false;
		}
		return true;
	}

	private void CheckForEqualityAndGetHashCode(BindingDiagnosticBag diagnostics)
	{
		if (this.IsInterfaceType() || IsRecord || IsRecordStruct)
		{
			return;
		}
		ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
		AddOperators("op_Equality", instance);
		AddOperators("op_Inequality", instance);
		bool flag = instance.Any();
		bool flag2 = TypeOverridesObjectMethod("Equals");
		if (flag | flag2)
		{
			bool flag3 = TypeOverridesObjectMethod("GetHashCode");
			if (flag2 && !flag3)
			{
				diagnostics.Add(ErrorCode.WRN_EqualsWithoutGetHashCode, GetFirstLocation(), this);
			}
			if (flag && !flag2)
			{
				diagnostics.Add(ErrorCode.WRN_EqualityOpWithoutEquals, GetFirstLocation(), this);
			}
			if (flag && !flag3)
			{
				diagnostics.Add(ErrorCode.WRN_EqualityOpWithoutGetHashCode, GetFirstLocation(), this);
			}
		}
		instance.Free();
	}

	private void CheckForRequiredMemberAttribute(BindingDiagnosticBag diagnostics)
	{
		if (HasDeclaredRequiredMembers)
		{
			Binder.GetWellKnownTypeMember(DeclaringCompilation, WellKnownMember.System_Runtime_CompilerServices_RequiredMemberAttribute__ctor, diagnostics, GetFirstLocation());
		}
		if (base.HasAnyRequiredMembers)
		{
			Binder.GetWellKnownTypeMember(DeclaringCompilation, WellKnownMember.System_Runtime_CompilerServices_CompilerFeatureRequiredAttribute__ctor, diagnostics, GetFirstLocation());
			if (IsRecord)
			{
				Binder.GetWellKnownTypeMember(DeclaringCompilation, WellKnownMember.System_Diagnostics_CodeAnalysis_SetsRequiredMembersAttribute__ctor, diagnostics, GetFirstLocation());
			}
		}
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
		if (baseTypeNoUseSiteDiagnostics is SourceMemberContainerTypeSymbol || (object)baseTypeNoUseSiteDiagnostics == null || !baseTypeNoUseSiteDiagnostics.HasRequiredMembersError)
		{
			return;
		}
		foreach (Symbol item in GetMembersUnordered())
		{
			if (item is MethodSymbol methodSymbol && methodSymbol.ShouldCheckRequiredMembers())
			{
				diagnostics.Add(ErrorCode.ERR_RequiredMembersBaseTypeInvalid, methodSymbol.GetFirstLocation(), BaseTypeNoUseSiteDiagnostics);
			}
		}
	}

	private void ReportRequiredMembers(BindingDiagnosticBag diagnostics)
	{
		foreach (Symbol item in GetMembersUnordered())
		{
			if (item.IsRequired())
			{
				diagnostics.Add(ErrorCode.ERR_ScriptsAndSubmissionsCannotHaveRequiredMembers, item.GetFirstLocation());
			}
		}
	}

	private bool TypeOverridesObjectMethod(string name)
	{
		foreach (MethodSymbol item in GetMembers(name).OfType<MethodSymbol>())
		{
			if (item.IsOverride && item.GetConstructedLeastOverriddenMethod(this, requireSameReturnType: false).ContainingType.SpecialType == SpecialType.System_Object)
			{
				return true;
			}
		}
		return false;
	}

	private void CheckFiniteFlatteningGraph(BindingDiagnosticBag diagnostics)
	{
		if (AllTypeArgumentCount() == 0)
		{
			return;
		}
		Dictionary<NamedTypeSymbol, NamedTypeSymbol> dictionary = new Dictionary<NamedTypeSymbol, NamedTypeSymbol>(ReferenceEqualityComparer.Instance);
		dictionary.Add(this, this);
		foreach (Symbol item in GetMembersUnordered())
		{
			SymbolKind kind = item.Kind;
			FieldSymbol fieldSymbol;
			if (kind != SymbolKind.Event)
			{
				if (kind != SymbolKind.Field)
				{
					continue;
				}
				fieldSymbol = (FieldSymbol)item;
			}
			else
			{
				fieldSymbol = ((EventSymbol)item).AssociatedField;
			}
			if ((object)fieldSymbol != null && fieldSymbol.IsStatic && fieldSymbol.Type.TypeKind == TypeKind.Struct)
			{
				NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)fieldSymbol.Type;
				if (InfiniteFlatteningGraph(this, namedTypeSymbol, dictionary))
				{
					diagnostics.Add(ErrorCode.ERR_StructLayoutCycle, fieldSymbol.GetFirstLocation(), fieldSymbol, namedTypeSymbol);
					break;
				}
			}
		}
	}

	private static bool InfiniteFlatteningGraph(SourceMemberContainerTypeSymbol top, NamedTypeSymbol t, Dictionary<NamedTypeSymbol, NamedTypeSymbol> instanceMap)
	{
		if (!t.ContainsTypeParameter())
		{
			return false;
		}
		NamedTypeSymbol originalDefinition = t.OriginalDefinition;
		if (instanceMap.TryGetValue(originalDefinition, out NamedTypeSymbol value))
		{
			if (!TypeSymbol.Equals(value, t, TypeCompareKind.AllNullableIgnoreOptions))
			{
				return (object)originalDefinition == top;
			}
			return false;
		}
		instanceMap.Add(originalDefinition, t);
		try
		{
			foreach (Symbol item in t.GetMembersUnordered())
			{
				if (item is FieldSymbol { IsStatic: not false } fieldSymbol && fieldSymbol.Type.TypeKind == TypeKind.Struct)
				{
					NamedTypeSymbol t2 = (NamedTypeSymbol)fieldSymbol.Type;
					if (InfiniteFlatteningGraph(top, t2, instanceMap))
					{
						return true;
					}
				}
			}
			return false;
		}
		finally
		{
			instanceMap.Remove(originalDefinition);
		}
	}

	private void CheckSequentialOnPartialType(BindingDiagnosticBag diagnostics)
	{
		if (!IsPartial || Layout.Kind != LayoutKind.Sequential)
		{
			return;
		}
		SyntaxReference syntaxReference = null;
		if (SyntaxReferences.Length <= 1)
		{
			return;
		}
		foreach (SyntaxReference syntaxReference2 in SyntaxReferences)
		{
			if (!(syntaxReference2.GetSyntax() is TypeDeclarationSyntax typeDeclarationSyntax))
			{
				continue;
			}
			foreach (MemberDeclarationSyntax member in typeDeclarationSyntax.Members)
			{
				if (hasInstanceData(member))
				{
					if (syntaxReference != null && syntaxReference != syntaxReference2)
					{
						diagnostics.Add(ErrorCode.WRN_SequentialOnPartialClass, GetFirstLocation(), this);
						return;
					}
					syntaxReference = syntaxReference2;
				}
			}
		}
		if (syntaxReference != null)
		{
			SynthesizedPrimaryConstructor primaryConstructor = PrimaryConstructor;
			if ((object)primaryConstructor != null && primaryConstructor.GetCapturedParameters().Any() && (primaryConstructor.SyntaxRef.SyntaxTree != syntaxReference.SyntaxTree || primaryConstructor.SyntaxRef.Span != syntaxReference.Span))
			{
				diagnostics.Add(ErrorCode.WRN_SequentialOnPartialClass, GetFirstLocation(), this);
			}
		}
		static bool hasInstanceData(MemberDeclarationSyntax m)
		{
			switch (m.Kind())
			{
			case SyntaxKind.FieldDeclaration:
			{
				FieldDeclarationSyntax fieldDeclarationSyntax = (FieldDeclarationSyntax)m;
				if (!ContainsModifier(fieldDeclarationSyntax.Modifiers, SyntaxKind.StaticKeyword))
				{
					return !ContainsModifier(fieldDeclarationSyntax.Modifiers, SyntaxKind.ConstKeyword);
				}
				return false;
			}
			case SyntaxKind.PropertyDeclaration:
			{
				PropertyDeclarationSyntax propertyDeclarationSyntax = (PropertyDeclarationSyntax)m;
				if (!ContainsModifier(propertyDeclarationSyntax.Modifiers, SyntaxKind.StaticKeyword) && !ContainsModifier(propertyDeclarationSyntax.Modifiers, SyntaxKind.AbstractKeyword) && !ContainsModifier(propertyDeclarationSyntax.Modifiers, SyntaxKind.ExternKeyword) && !ContainsModifier(propertyDeclarationSyntax.Modifiers, SyntaxKind.PartialKeyword) && propertyDeclarationSyntax.AccessorList != null)
				{
					return All(propertyDeclarationSyntax.AccessorList.Accessors, (AccessorDeclarationSyntax a) => a.Body == null && a.ExpressionBody == null);
				}
				return false;
			}
			case SyntaxKind.EventFieldDeclaration:
			{
				EventFieldDeclarationSyntax eventFieldDeclarationSyntax = (EventFieldDeclarationSyntax)m;
				if (!ContainsModifier(eventFieldDeclarationSyntax.Modifiers, SyntaxKind.StaticKeyword) && !ContainsModifier(eventFieldDeclarationSyntax.Modifiers, SyntaxKind.AbstractKeyword))
				{
					return !ContainsModifier(eventFieldDeclarationSyntax.Modifiers, SyntaxKind.ExternKeyword);
				}
				return false;
			}
			default:
				return false;
			}
		}
	}

	private static bool All<T>(SyntaxList<T> list, Func<T, bool> predicate) where T : CSharpSyntaxNode
	{
		foreach (T item in list)
		{
			if (predicate(item))
			{
				return true;
			}
		}
		return false;
	}

	private static bool ContainsModifier(SyntaxTokenList modifiers, SyntaxKind modifier)
	{
		foreach (SyntaxToken item in modifiers)
		{
			if (item.IsKind(modifier))
			{
				return true;
			}
		}
		return false;
	}

	private Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> MakeAllMembers()
	{
		MembersAndInitializers membersAndInitializers = GetMembersAndInitializers();
		Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> dictionary;
		if (!membersAndInitializers.HaveIndexers && !IsTupleType && _lazyEarlyAttributeDecodingMembersDictionary != null)
		{
			dictionary = _lazyEarlyAttributeDecodingMembersDictionary;
		}
		else
		{
			dictionary = ToNameKeyedDictionary(membersAndInitializers.NonTypeMembers);
			AddNestedTypesToDictionary(dictionary, GetTypeMembersDictionary());
		}
		return dictionary;
	}

	private static void AddNestedTypesToDictionary(Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> membersByName, Dictionary<ReadOnlyMemory<char>, ImmutableArray<NamedTypeSymbol>> typesByName)
	{
		foreach (var (key, immutableArray2) in typesByName)
		{
			ImmutableArray<Symbol> immutableArray3 = StaticCast<Symbol>.From(immutableArray2);
			if (membersByName.TryGetValue(key, out ImmutableArray<Symbol> value))
			{
				membersByName[key] = value.Concat(immutableArray3);
			}
			else
			{
				membersByName.Add(key, immutableArray3);
			}
		}
	}

	private MembersAndInitializers? BuildMembersAndInitializers(BindingDiagnosticBag diagnostics)
	{
		DeclaredMembersAndInitializers declaredMembersAndInitializers = getDeclaredMembersAndInitializers();
		if (declaredMembersAndInitializers == null)
		{
			return null;
		}
		MembersAndInitializersBuilder membersAndInitializersBuilder = new MembersAndInitializersBuilder(declaredMembersAndInitializers);
		AddSynthesizedMembers(membersAndInitializersBuilder, declaredMembersAndInitializers, diagnostics);
		if (Volatile.Read(in _lazyMembersAndInitializers) != null)
		{
			membersAndInitializersBuilder.Free();
			return null;
		}
		return membersAndInitializersBuilder.ToReadOnlyAndFree(this, declaredMembersAndInitializers);
		DeclaredMembersAndInitializers? buildDeclaredMembersAndInitializers(BindingDiagnosticBag diagnostics2)
		{
			DeclaredMembersAndInitializersBuilder declaredMembersAndInitializersBuilder = new DeclaredMembersAndInitializersBuilder();
			AddDeclaredNontypeMembers(declaredMembersAndInitializersBuilder, diagnostics2);
			switch (TypeKind)
			{
			case TypeKind.Struct:
				CheckForStructBadInitializers(declaredMembersAndInitializersBuilder, diagnostics2);
				CheckForStructDefaultConstructors(declaredMembersAndInitializersBuilder.NonTypeMembersWithPartialImplementations, isEnum: false, diagnostics2);
				break;
			case TypeKind.Enum:
				CheckForStructDefaultConstructors(declaredMembersAndInitializersBuilder.NonTypeMembersWithPartialImplementations, isEnum: true, diagnostics2);
				break;
			}
			if (Volatile.Read(in _lazyDeclaredMembersAndInitializers) != DeclaredMembersAndInitializers.UninitializedSentinel)
			{
				declaredMembersAndInitializersBuilder.Free();
				return null;
			}
			return declaredMembersAndInitializersBuilder.ToReadOnlyAndFree(DeclaringCompilation);
		}
		DeclaredMembersAndInitializers? getDeclaredMembersAndInitializers()
		{
			DeclaredMembersAndInitializers lazyDeclaredMembersAndInitializers = _lazyDeclaredMembersAndInitializers;
			if (lazyDeclaredMembersAndInitializers != DeclaredMembersAndInitializers.UninitializedSentinel)
			{
				return lazyDeclaredMembersAndInitializers;
			}
			if (Volatile.Read(in _lazyMembersAndInitializers) != null)
			{
				return null;
			}
			BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
			lazyDeclaredMembersAndInitializers = buildDeclaredMembersAndInitializers(instance);
			DeclaredMembersAndInitializers declaredMembersAndInitializers2 = Interlocked.CompareExchange(ref _lazyDeclaredMembersAndInitializers, lazyDeclaredMembersAndInitializers, DeclaredMembersAndInitializers.UninitializedSentinel);
			if (declaredMembersAndInitializers2 != DeclaredMembersAndInitializers.UninitializedSentinel)
			{
				instance.Free();
				return declaredMembersAndInitializers2;
			}
			AddDeclarationDiagnostics(instance);
			instance.Free();
			return lazyDeclaredMembersAndInitializers;
		}
	}

	private void MergePartialMembersAndInitializeNonTypeMembers(ImmutableArray<Symbol> nonTypeMembersWithPartialImplementations, ref ImmutableArray<Symbol> nonTypeMembers)
	{
		PooledDictionary<ReadOnlyMemory<char>, object> pooledDictionary = null;
		foreach (Symbol item in nonTypeMembersWithPartialImplementations)
		{
			if (item.IsPartialMember())
			{
				ImmutableArrayExtensions.AddToMultiValueDictionaryBuilder(pooledDictionary ?? (pooledDictionary = NamespaceOrTypeSymbol.s_nameToObjectPool.Allocate()), System.MemoryExtensions.AsMemory(item.IsIndexer() ? "this[]" : item.Name), item);
			}
		}
		if (pooledDictionary == null)
		{
			ImmutableInterlocked.InterlockedInitialize(ref nonTypeMembers, nonTypeMembersWithPartialImplementations);
			return;
		}
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
		ArrayBuilder<Symbol> instance2 = ArrayBuilder<Symbol>.GetInstance(nonTypeMembersWithPartialImplementations.Length);
		instance2.AddRange(nonTypeMembersWithPartialImplementations);
		MergePartialMembers(pooledDictionary, instance2, instance);
		pooledDictionary.Free();
		if (ImmutableInterlocked.InterlockedInitialize(ref nonTypeMembers, instance2.ToImmutableAndFree()))
		{
			AddDeclarationDiagnostics(instance);
		}
		instance.Free();
	}

	internal ImmutableArray<SynthesizedSimpleProgramEntryPointSymbol> GetSimpleProgramEntryPoints()
	{
		if (_lazySimpleProgramEntryPoints.IsDefault)
		{
			BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
			ImmutableArray<SynthesizedSimpleProgramEntryPointSymbol> value = buildSimpleProgramEntryPoint(instance);
			if (ImmutableInterlocked.InterlockedInitialize(ref _lazySimpleProgramEntryPoints, value))
			{
				AddDeclarationDiagnostics(instance);
			}
			instance.Free();
		}
		return _lazySimpleProgramEntryPoints;
		ImmutableArray<SynthesizedSimpleProgramEntryPointSymbol> buildSimpleProgramEntryPoint(BindingDiagnosticBag diagnostics)
		{
			if (!(ContainingSymbol is NamespaceSymbol { IsGlobalNamespace: not false }) || Name != "Program")
			{
				return ImmutableArray<SynthesizedSimpleProgramEntryPointSymbol>.Empty;
			}
			ArrayBuilder<SynthesizedSimpleProgramEntryPointSymbol> arrayBuilder = null;
			foreach (SingleTypeDeclaration declaration in declaration.Declarations)
			{
				if (declaration.IsSimpleProgram)
				{
					if (arrayBuilder == null)
					{
						arrayBuilder = ArrayBuilder<SynthesizedSimpleProgramEntryPointSymbol>.GetInstance();
					}
					else
					{
						Binder.Error(diagnostics, ErrorCode.ERR_SimpleProgramMultipleUnitsWithTopLevelStatements, declaration.NameLocation);
					}
					arrayBuilder.Add(new SynthesizedSimpleProgramEntryPointSymbol(this, declaration, diagnostics));
				}
			}
			return arrayBuilder?.ToImmutableAndFree() ?? ImmutableArray<SynthesizedSimpleProgramEntryPointSymbol>.Empty;
		}
	}

	internal IEnumerable<SourceMemberMethodSymbol> GetMethodsPossiblyCapturingPrimaryConstructorParameters()
	{
		DeclaredMembersAndInitializers declaredMembersAndInitializers = Volatile.Read(in _lazyDeclaredMembersAndInitializers);
		ImmutableArray<Symbol> nonTypeMembers;
		SynthesizedPrimaryConstructor primaryConstructor;
		if (declaredMembersAndInitializers != null && declaredMembersAndInitializers != DeclaredMembersAndInitializers.UninitializedSentinel)
		{
			nonTypeMembers = declaredMembersAndInitializers.GetNonTypeMembers(this);
			primaryConstructor = declaredMembersAndInitializers.PrimaryConstructor;
		}
		else
		{
			MembersAndInitializers membersAndInitializers = GetMembersAndInitializers();
			nonTypeMembers = membersAndInitializers.NonTypeMembers;
			primaryConstructor = membersAndInitializers.PrimaryConstructor;
		}
		foreach (Symbol item in nonTypeMembers)
		{
			if ((object)item != primaryConstructor && !item.IsStatic && item is MethodSymbol method && MethodCompiler.GetMethodToCompile(method) is SourceMemberMethodSymbol { IsExtern: false, IsAbstract: false, SynthesizesLoweredBoundBody: false } sourceMemberMethodSymbol)
			{
				yield return sourceMemberMethodSymbol;
			}
		}
	}

	internal ImmutableArray<Symbol> GetMembersToMatchAgainstDeclarationSpan()
	{
		DeclaredMembersAndInitializers declaredMembersAndInitializers = Volatile.Read(in _lazyDeclaredMembersAndInitializers);
		if (declaredMembersAndInitializers != null && declaredMembersAndInitializers != DeclaredMembersAndInitializers.UninitializedSentinel)
		{
			return declaredMembersAndInitializers.GetNonTypeMembers(this);
		}
		return GetMembersAndInitializers().NonTypeMembers;
	}

	internal ImmutableArray<Symbol> GetCandidateMembersForLookup(string name)
	{
		bool flag = (((object)this != null && (IsRecord || IsRecordStruct)) ? true : false);
		if (flag || state.HasComplete(CompletionPart.Members))
		{
			return GetMembers(name);
		}
		DeclaredMembersAndInitializers declaredMembersAndInitializers = Volatile.Read(in _lazyDeclaredMembersAndInitializers);
		ImmutableArray<Symbol> nonTypeMembers;
		SynthesizedPrimaryConstructor primaryConstructor;
		if (declaredMembersAndInitializers != null && declaredMembersAndInitializers != DeclaredMembersAndInitializers.UninitializedSentinel)
		{
			nonTypeMembers = declaredMembersAndInitializers.GetNonTypeMembers(this);
			primaryConstructor = declaredMembersAndInitializers.PrimaryConstructor;
		}
		else
		{
			MembersAndInitializers membersAndInitializers = GetMembersAndInitializers();
			nonTypeMembers = membersAndInitializers.NonTypeMembers;
			primaryConstructor = membersAndInitializers.PrimaryConstructor;
		}
		if (primaryConstructor.ParameterCount == 0)
		{
			return GetMembers(name);
		}
		ImmutableArray<Symbol> immutableArray = GetTypeMembers(name).Cast<NamedTypeSymbol, Symbol>();
		ArrayBuilder<Symbol> arrayBuilder = null;
		foreach (Symbol item in nonTypeMembers)
		{
			if (!item.IsAccessor() && item.Name == name)
			{
				if (arrayBuilder == null)
				{
					arrayBuilder = ArrayBuilder<Symbol>.GetInstance(immutableArray.Length + 1);
				}
				arrayBuilder.Add(item);
			}
		}
		if (arrayBuilder == null)
		{
			return immutableArray;
		}
		arrayBuilder.AddRange(immutableArray);
		return arrayBuilder.ToImmutableAndFree();
	}

	private void AddSynthesizedMembers(MembersAndInitializersBuilder builder, DeclaredMembersAndInitializers declaredMembersAndInitializers, BindingDiagnosticBag diagnostics)
	{
		if (TypeKind == TypeKind.Class)
		{
			AddSynthesizedSimpleProgramEntryPointIfNecessary(builder, declaredMembersAndInitializers);
		}
		switch (TypeKind)
		{
		case TypeKind.Class:
		case TypeKind.Enum:
		case TypeKind.Interface:
		case TypeKind.Struct:
		case TypeKind.Submission:
			AddSynthesizedTypeMembersIfNecessary(builder, declaredMembersAndInitializers, diagnostics);
			AddSynthesizedConstructorsIfNecessary(builder, declaredMembersAndInitializers, diagnostics);
			if (TypeKind == TypeKind.Class)
			{
				AddSynthesizedExtensionImplementationsIfNecessary(builder, declaredMembersAndInitializers);
			}
			break;
		}
		AddSynthesizedTupleMembersIfNecessary(builder, declaredMembersAndInitializers);
	}

	private void AddSynthesizedExtensionImplementationsIfNecessary(MembersAndInitializersBuilder builder, DeclaredMembersAndInitializers declaredMembersAndInitializers)
	{
		foreach (NamedTypeSymbol typeMember in GetTypeMembers(""))
		{
			if (typeMember.TypeKind != TypeKind.Extension)
			{
				continue;
			}
			foreach (Symbol member in typeMember.GetMembers())
			{
				if (member is MethodSymbol methodSymbol && !member.IsImplicitlyDeclared)
				{
					MethodKind methodKind = methodSymbol.MethodKind;
					if (methodKind != MethodKind.Constructor && methodKind != MethodKind.StaticConstructor && methodKind != MethodKind.Destructor && methodKind != MethodKind.ExplicitInterfaceImplementation && (methodSymbol.IsStatic || (object)typeMember.ExtensionParameter != null))
					{
						builder.AddNonTypeMember(this, new SourceExtensionImplementationMethodSymbol(methodSymbol), declaredMembersAndInitializers);
					}
				}
			}
		}
	}

	private void AddDeclaredNontypeMembers(DeclaredMembersAndInitializersBuilder builder, BindingDiagnosticBag diagnostics)
	{
		foreach (SingleTypeDeclaration declaration in declaration.Declarations)
		{
			if (declaration.HasAnyNontypeMembers)
			{
				if (_lazyMembersAndInitializers != null)
				{
					break;
				}
				SyntaxNode syntax = declaration.SyntaxReference.GetSyntax();
				switch (syntax.Kind())
				{
				case SyntaxKind.EnumDeclaration:
					AddEnumMembers(builder, (EnumDeclarationSyntax)syntax, diagnostics);
					break;
				case SyntaxKind.DelegateDeclaration:
					SourceDelegateMethodSymbol.AddDelegateMembers(this, builder.NonTypeMembersWithPartialImplementations, (DelegateDeclarationSyntax)syntax, diagnostics);
					break;
				case SyntaxKind.NamespaceDeclaration:
				case SyntaxKind.FileScopedNamespaceDeclaration:
					AddNonTypeMembers(builder, ((BaseNamespaceDeclarationSyntax)syntax).Members, diagnostics);
					break;
				case SyntaxKind.CompilationUnit:
					AddNonTypeMembers(builder, ((CompilationUnitSyntax)syntax).Members, diagnostics);
					break;
				case SyntaxKind.InterfaceDeclaration:
					AddNonTypeMembers(builder, ((InterfaceDeclarationSyntax)syntax).Members, diagnostics);
					break;
				case SyntaxKind.ClassDeclaration:
				case SyntaxKind.StructDeclaration:
				case SyntaxKind.RecordDeclaration:
				case SyntaxKind.RecordStructDeclaration:
				case SyntaxKind.ExtensionBlockDeclaration:
				{
					TypeDeclarationSyntax typeDeclarationSyntax = (TypeDeclarationSyntax)syntax;
					noteTypeParameters(typeDeclarationSyntax, builder, diagnostics);
					AddNonTypeMembers(builder, typeDeclarationSyntax.Members, diagnostics);
					break;
				}
				default:
					throw ExceptionUtilities.UnexpectedValue(syntax.Kind());
				}
			}
		}
		void noteTypeParameters(TypeDeclarationSyntax typeDeclarationSyntax2, DeclaredMembersAndInitializersBuilder declaredMembersAndInitializersBuilder, BindingDiagnosticBag bindingDiagnosticBag)
		{
			ParameterListSyntax parameterList = typeDeclarationSyntax2.ParameterList;
			if (parameterList != null && !IsExtension)
			{
				if (declaredMembersAndInitializersBuilder.DeclarationWithParameters == null)
				{
					declaredMembersAndInitializersBuilder.DeclarationWithParameters = typeDeclarationSyntax2;
					SynthesizedPrimaryConstructor synthesizedPrimaryConstructor = new SynthesizedPrimaryConstructor(this, typeDeclarationSyntax2);
					if (IsStatic)
					{
						bindingDiagnosticBag.Add(ErrorCode.ERR_ConstructorInStaticClass, typeDeclarationSyntax2.Identifier.GetLocation());
					}
					declaredMembersAndInitializersBuilder.PrimaryConstructor = synthesizedPrimaryConstructor;
					CSharpCompilation declaringCompilation = DeclaringCompilation;
					declaredMembersAndInitializersBuilder.UpdateIsNullableEnabledForConstructorsAndFields(synthesizedPrimaryConstructor.IsStatic, declaringCompilation, parameterList);
					if (typeDeclarationSyntax2 != null)
					{
						PrimaryConstructorBaseTypeSyntax primaryConstructorBaseTypeIfClass = typeDeclarationSyntax2.PrimaryConstructorBaseTypeIfClass;
						if (primaryConstructorBaseTypeIfClass != null)
						{
							ArgumentListSyntax argumentList = primaryConstructorBaseTypeIfClass.ArgumentList;
							if (argumentList != null)
							{
								declaredMembersAndInitializersBuilder.UpdateIsNullableEnabledForConstructorsAndFields(synthesizedPrimaryConstructor.IsStatic, declaringCompilation, argumentList);
							}
						}
					}
				}
				else
				{
					bindingDiagnosticBag.Add(ErrorCode.ERR_MultipleRecordParameterLists, parameterList.Location);
				}
			}
		}
	}

	internal Binder GetBinder(CSharpSyntaxNode syntaxNode)
	{
		return DeclaringCompilation.GetBinder(syntaxNode);
	}

	private static void MergePartialMembers(Dictionary<ReadOnlyMemory<char>, object> membersByName, ArrayBuilder<Symbol> nonTypeMembers, BindingDiagnosticBag diagnostics)
	{
		Dictionary<Symbol, Symbol> dictionary = new Dictionary<Symbol, Symbol>(MemberSignatureComparer.PartialMethodsComparer);
		foreach (KeyValuePair<ReadOnlyMemory<char>, object> item in membersByName)
		{
			dictionary.Clear();
			if (item.Value is ArrayBuilder<Symbol> arrayBuilder)
			{
				foreach (Symbol item2 in arrayBuilder)
				{
					if ((item2 is SourcePropertyAccessorSymbol || item2 is SourceEventAccessorSymbol) ? true : false)
					{
						continue;
					}
					if (!dictionary.TryGetValue(item2, out var value))
					{
						dictionary.Add(item2, item2);
					}
					else if (item2 is SourceOrdinaryMethodSymbol currentMethod)
					{
						if (value is SourceOrdinaryMethodSymbol prevMethod)
						{
							mergePartialMethods(nonTypeMembers, currentMethod, prevMethod, diagnostics);
						}
					}
					else if (item2 is SourcePropertySymbol currentProperty)
					{
						if (value is SourcePropertySymbol prevProperty)
						{
							mergePartialProperties(nonTypeMembers, currentProperty, prevProperty, diagnostics);
						}
					}
					else if (item2 is SourceConstructorSymbol sourceConstructorSymbol)
					{
						if (!sourceConstructorSymbol.IsStatic && value is SourceConstructorSymbol { IsStatic: false } sourceConstructorSymbol2)
						{
							mergePartialConstructors(nonTypeMembers, sourceConstructorSymbol, sourceConstructorSymbol2, diagnostics);
						}
					}
					else if (item2 is SourceEventSymbol currentEvent && value is SourceEventSymbol prevEvent)
					{
						mergePartialEvents(nonTypeMembers, currentEvent, prevEvent, diagnostics);
					}
				}
			}
			else
			{
				Symbol symbol = (Symbol)item.Value;
				if ((symbol is SourcePropertyAccessorSymbol || symbol is SourceEventAccessorSymbol) ? true : false)
				{
					continue;
				}
				dictionary.Add(symbol, symbol);
			}
			foreach (Symbol value2 in dictionary.Values)
			{
				if (!(value2 is SourceOrdinaryMethodSymbol sourceOrdinaryMethodSymbol))
				{
					if (!(value2 is SourcePropertySymbol sourcePropertySymbol))
					{
						if (!(value2 is SourceConstructorSymbol sourceConstructorSymbol3))
						{
							if (!(value2 is SourceEventSymbol sourceEventSymbol))
							{
								throw ExceptionUtilities.UnexpectedValue(value2);
							}
							if ((object)sourceEventSymbol.OtherPartOfPartial == null)
							{
								diagnostics.Add(sourceEventSymbol.IsPartialDefinition ? ErrorCode.ERR_PartialMemberMissingImplementation : ErrorCode.ERR_PartialMemberMissingDefinition, sourceEventSymbol.GetFirstLocation(), sourceEventSymbol);
							}
						}
						else if ((object)sourceConstructorSymbol3.OtherPartOfPartial == null)
						{
							diagnostics.Add(sourceConstructorSymbol3.IsPartialDefinition ? ErrorCode.ERR_PartialMemberMissingImplementation : ErrorCode.ERR_PartialMemberMissingDefinition, sourceConstructorSymbol3.GetFirstLocation(), sourceConstructorSymbol3);
						}
					}
					else if ((object)sourcePropertySymbol.OtherPartOfPartial == null)
					{
						diagnostics.Add(sourcePropertySymbol.IsPartialDefinition ? ErrorCode.ERR_PartialPropertyMissingImplementation : ErrorCode.ERR_PartialPropertyMissingDefinition, sourcePropertySymbol.GetFirstLocation(), sourcePropertySymbol);
					}
				}
				else if (sourceOrdinaryMethodSymbol.IsPartialImplementation && (object)sourceOrdinaryMethodSymbol.OtherPartOfPartial == null)
				{
					diagnostics.Add(ErrorCode.ERR_PartialMethodMustHaveLatent, sourceOrdinaryMethodSymbol.GetFirstLocation(), sourceOrdinaryMethodSymbol);
				}
				else if ((object)sourceOrdinaryMethodSymbol != null && sourceOrdinaryMethodSymbol.IsPartialDefinition && (object)sourceOrdinaryMethodSymbol.OtherPartOfPartial == null && sourceOrdinaryMethodSymbol.HasExplicitAccessModifier)
				{
					diagnostics.Add(ErrorCode.ERR_PartialMethodWithAccessibilityModsMustHaveImplementation, sourceOrdinaryMethodSymbol.GetFirstLocation(), sourceOrdinaryMethodSymbol);
				}
			}
		}
		foreach (KeyValuePair<ReadOnlyMemory<char>, object> item3 in membersByName)
		{
			if (item3.Value is ArrayBuilder<Symbol> arrayBuilder2)
			{
				foreach (Symbol item4 in arrayBuilder2)
				{
					fixupNotMergedPartialProperty(item4);
				}
			}
			else
			{
				fixupNotMergedPartialProperty((Symbol)item3.Value);
			}
		}
		static void fixupNotMergedPartialProperty(Symbol symbol2)
		{
			if (symbol2 is SourcePropertySymbol { OtherPartOfPartial: null } sourcePropertySymbol2)
			{
				sourcePropertySymbol2.SetMergedBackingField(sourcePropertySymbol2.DeclaredBackingField);
			}
		}
		static bool hasInitializer(SourcePropertySymbol property)
		{
			return property.DeclaredBackingField?.HasInitializer ?? false;
		}
		static void mergeAccessors(ArrayBuilder<Symbol> symbols, SourceEventAccessorSymbol? currentAccessor, SourceEventAccessorSymbol? prevAccessor)
		{
			if ((object)currentAccessor != null && currentAccessor.IsPartialImplementation)
			{
				Remove(symbols, currentAccessor);
			}
			else if ((object)prevAccessor != null && prevAccessor.IsPartialImplementation)
			{
				Remove(symbols, prevAccessor);
			}
		}
		static void mergePartialConstructors(ArrayBuilder<Symbol> nonTypeMembers2, SourceConstructorSymbol currentConstructor, SourceConstructorSymbol prevConstructor, BindingDiagnosticBag bindingDiagnosticBag)
		{
			if (currentConstructor.IsPartialImplementation)
			{
				if (!prevConstructor.IsPartialImplementation)
				{
					SourceConstructorSymbol otherPartOfPartial = prevConstructor.OtherPartOfPartial;
					if ((object)otherPartOfPartial == null || (object)otherPartOfPartial == currentConstructor)
					{
						goto IL_003b;
					}
				}
				bindingDiagnosticBag.Add(ErrorCode.ERR_PartialMemberDuplicateImplementation, currentConstructor.GetFirstLocation(), currentConstructor);
				return;
			}
			goto IL_003b;
			IL_0076:
			FixPartialConstructor(nonTypeMembers2, prevConstructor, currentConstructor);
			return;
			IL_003b:
			if (currentConstructor.IsPartialDefinition)
			{
				if (!prevConstructor.IsPartialDefinition)
				{
					SourceConstructorSymbol otherPartOfPartial2 = prevConstructor.OtherPartOfPartial;
					if ((object)otherPartOfPartial2 == null || (object)otherPartOfPartial2 == currentConstructor)
					{
						goto IL_0076;
					}
				}
				bindingDiagnosticBag.Add(ErrorCode.ERR_PartialMemberDuplicateDefinition, currentConstructor.GetFirstLocation(), currentConstructor);
				return;
			}
			goto IL_0076;
		}
		static void mergePartialEvents(ArrayBuilder<Symbol> nonTypeMembers2, SourceEventSymbol sourceEventSymbol2, SourceEventSymbol sourceEventSymbol3, BindingDiagnosticBag bindingDiagnosticBag)
		{
			if (sourceEventSymbol2.IsPartialImplementation)
			{
				if (!sourceEventSymbol3.IsPartialImplementation)
				{
					SourceEventSymbol otherPartOfPartial = sourceEventSymbol3.OtherPartOfPartial;
					if ((object)otherPartOfPartial == null || (object)otherPartOfPartial == sourceEventSymbol2)
					{
						goto IL_003b;
					}
				}
				bindingDiagnosticBag.Add(ErrorCode.ERR_PartialMemberDuplicateImplementation, sourceEventSymbol2.GetFirstLocation(), sourceEventSymbol2);
				return;
			}
			goto IL_003b;
			IL_0076:
			mergeAccessors(nonTypeMembers2, (SourceEventAccessorSymbol)sourceEventSymbol2.AddMethod, (SourceEventAccessorSymbol)sourceEventSymbol3.AddMethod);
			mergeAccessors(nonTypeMembers2, (SourceEventAccessorSymbol)sourceEventSymbol2.RemoveMethod, (SourceEventAccessorSymbol)sourceEventSymbol3.RemoveMethod);
			FixPartialEvent(nonTypeMembers2, sourceEventSymbol3, sourceEventSymbol2);
			return;
			IL_003b:
			if (sourceEventSymbol2.IsPartialDefinition)
			{
				if (!sourceEventSymbol3.IsPartialDefinition)
				{
					SourceEventSymbol otherPartOfPartial2 = sourceEventSymbol3.OtherPartOfPartial;
					if ((object)otherPartOfPartial2 == null || (object)otherPartOfPartial2 == sourceEventSymbol2)
					{
						goto IL_0076;
					}
				}
				bindingDiagnosticBag.Add(ErrorCode.ERR_PartialMemberDuplicateDefinition, sourceEventSymbol2.GetFirstLocation(), sourceEventSymbol2);
				return;
			}
			goto IL_0076;
		}
		static void mergePartialMethods(ArrayBuilder<Symbol> nonTypeMembers2, SourceOrdinaryMethodSymbol sourceOrdinaryMethodSymbol2, SourceOrdinaryMethodSymbol sourceOrdinaryMethodSymbol3, BindingDiagnosticBag bindingDiagnosticBag)
		{
			if (sourceOrdinaryMethodSymbol2.IsPartialImplementation)
			{
				if (!sourceOrdinaryMethodSymbol3.IsPartialImplementation)
				{
					MethodSymbol otherPartOfPartial = sourceOrdinaryMethodSymbol3.OtherPartOfPartial;
					if ((object)otherPartOfPartial == null || (object)otherPartOfPartial == sourceOrdinaryMethodSymbol2)
					{
						goto IL_0031;
					}
				}
				bindingDiagnosticBag.Add(ErrorCode.ERR_PartialMethodOnlyOneActual, sourceOrdinaryMethodSymbol2.GetFirstLocation());
				return;
			}
			goto IL_0031;
			IL_0062:
			FixPartialMethod(nonTypeMembers2, sourceOrdinaryMethodSymbol3, sourceOrdinaryMethodSymbol2);
			return;
			IL_0031:
			if (sourceOrdinaryMethodSymbol2.IsPartialDefinition)
			{
				if (!sourceOrdinaryMethodSymbol3.IsPartialDefinition)
				{
					MethodSymbol otherPartOfPartial2 = sourceOrdinaryMethodSymbol3.OtherPartOfPartial;
					if ((object)otherPartOfPartial2 == null || (object)otherPartOfPartial2 == sourceOrdinaryMethodSymbol2)
					{
						goto IL_0062;
					}
				}
				bindingDiagnosticBag.Add(ErrorCode.ERR_PartialMethodOnlyOneLatent, sourceOrdinaryMethodSymbol2.GetFirstLocation());
				return;
			}
			goto IL_0062;
		}
		static void mergePartialProperties(ArrayBuilder<Symbol> nonTypeMembers2, SourcePropertySymbol sourcePropertySymbol2, SourcePropertySymbol sourcePropertySymbol3, BindingDiagnosticBag bindingDiagnosticBag)
		{
			SourcePropertySymbol currentProperty2 = sourcePropertySymbol2;
			SourcePropertySymbol prevProperty2 = sourcePropertySymbol3;
			BindingDiagnosticBag diagnostics2 = bindingDiagnosticBag;
			if (currentProperty2.IsPartialImplementation)
			{
				if (!prevProperty2.IsPartialImplementation)
				{
					SourcePropertySymbol otherPartOfPartial = prevProperty2.OtherPartOfPartial;
					if ((object)otherPartOfPartial == null || (object)otherPartOfPartial == currentProperty2)
					{
						goto IL_0067;
					}
				}
				diagnostics2.Add(ErrorCode.ERR_PartialPropertyDuplicateImplementation, currentProperty2.GetFirstLocation());
				return;
			}
			goto IL_0067;
			IL_00b6:
			if (hasInitializer(prevProperty2) && hasInitializer(currentProperty2))
			{
				diagnostics2.Add(ErrorCode.ERR_PartialPropertyDuplicateInitializer, currentProperty2.GetFirstLocation());
			}
			mergeAccessors2(nonTypeMembers2, (SourcePropertyAccessorSymbol)currentProperty2.GetMethod, (SourcePropertyAccessorSymbol)prevProperty2.GetMethod);
			mergeAccessors2(nonTypeMembers2, (SourcePropertyAccessorSymbol)currentProperty2.SetMethod, (SourcePropertyAccessorSymbol)prevProperty2.SetMethod);
			FixPartialProperty(nonTypeMembers2, prevProperty2, currentProperty2);
			return;
			IL_0067:
			if (currentProperty2.IsPartialDefinition)
			{
				if (!prevProperty2.IsPartialDefinition)
				{
					SourcePropertySymbol otherPartOfPartial2 = prevProperty2.OtherPartOfPartial;
					if ((object)otherPartOfPartial2 == null || (object)otherPartOfPartial2 == currentProperty2)
					{
						goto IL_00b6;
					}
				}
				diagnostics2.Add(ErrorCode.ERR_PartialPropertyDuplicateDefinition, currentProperty2.GetFirstLocation());
				return;
			}
			goto IL_00b6;
			void mergeAccessors2(ArrayBuilder<Symbol> symbols, SourcePropertyAccessorSymbol? currentAccessor, SourcePropertyAccessorSymbol? prevAccessor)
			{
				if ((object)currentAccessor != null && (object)prevAccessor != null)
				{
					SourcePropertyAccessorSymbol symbol2 = (currentProperty2.IsPartialDefinition ? prevAccessor : currentAccessor);
					Remove(symbols, symbol2);
				}
				else if ((object)currentAccessor != null || (object)prevAccessor != null)
				{
					SourcePropertySymbol sourcePropertySymbol6;
					SourcePropertySymbol sourcePropertySymbol7;
					SourcePropertyAccessorSymbol sourcePropertyAccessorSymbol;
					if ((object)prevAccessor == null)
					{
						SourcePropertySymbol sourcePropertySymbol4 = currentProperty2;
						SourcePropertySymbol sourcePropertySymbol5 = prevProperty2;
						sourcePropertySymbol6 = sourcePropertySymbol5;
						sourcePropertySymbol7 = sourcePropertySymbol4;
						sourcePropertyAccessorSymbol = currentAccessor;
					}
					else
					{
						SourcePropertySymbol sourcePropertySymbol5 = prevProperty2;
						SourcePropertySymbol sourcePropertySymbol4 = currentProperty2;
						sourcePropertySymbol6 = sourcePropertySymbol4;
						sourcePropertySymbol7 = sourcePropertySymbol5;
						sourcePropertyAccessorSymbol = prevAccessor;
					}
					SourcePropertySymbol sourcePropertySymbol8;
					ErrorCode code;
					if (!sourcePropertyAccessorSymbol.IsPartialDefinition)
					{
						SourcePropertySymbol sourcePropertySymbol4 = sourcePropertySymbol7;
						sourcePropertySymbol8 = sourcePropertySymbol4;
						code = ErrorCode.ERR_PartialPropertyUnexpectedAccessor;
					}
					else
					{
						SourcePropertySymbol sourcePropertySymbol4 = sourcePropertySymbol6;
						sourcePropertySymbol8 = sourcePropertySymbol4;
						code = ErrorCode.ERR_PartialPropertyMissingAccessor;
					}
					diagnostics2.Add(code, sourcePropertySymbol8.GetFirstLocation(), sourcePropertyAccessorSymbol);
				}
			}
		}
	}

	private static void FixPartialMethod(ArrayBuilder<Symbol> nonTypeMembers, SourceOrdinaryMethodSymbol part1, SourceOrdinaryMethodSymbol part2)
	{
		SourceOrdinaryMethodSymbol definition;
		SourceOrdinaryMethodSymbol sourceOrdinaryMethodSymbol;
		if (part1.IsPartialDefinition)
		{
			definition = part1;
			sourceOrdinaryMethodSymbol = part2;
		}
		else
		{
			definition = part2;
			sourceOrdinaryMethodSymbol = part1;
		}
		SourceOrdinaryMethodSymbol.InitializePartialMethodParts(definition, sourceOrdinaryMethodSymbol);
		Remove(nonTypeMembers, sourceOrdinaryMethodSymbol);
	}

	private static void FixPartialProperty(ArrayBuilder<Symbol> nonTypeMembers, SourcePropertySymbol part1, SourcePropertySymbol part2)
	{
		SourcePropertySymbol sourcePropertySymbol;
		SourcePropertySymbol sourcePropertySymbol2;
		if (part1.IsPartialDefinition)
		{
			sourcePropertySymbol = part1;
			sourcePropertySymbol2 = part2;
		}
		else
		{
			sourcePropertySymbol = part2;
			sourcePropertySymbol2 = part1;
		}
		SynthesizedBackingFieldSymbol declaredBackingField = sourcePropertySymbol2.DeclaredBackingField;
		if ((object)declaredBackingField != null && (object)sourcePropertySymbol.DeclaredBackingField != null)
		{
			Remove(nonTypeMembers, declaredBackingField);
		}
		SourcePropertySymbol.InitializePartialPropertyParts(sourcePropertySymbol, sourcePropertySymbol2);
		Remove(nonTypeMembers, sourcePropertySymbol2);
	}

	private static void FixPartialConstructor(ArrayBuilder<Symbol> nonTypeMembers, SourceConstructorSymbol part1, SourceConstructorSymbol part2)
	{
		SourceConstructorSymbol definition;
		SourceConstructorSymbol sourceConstructorSymbol;
		if (part1.IsPartialDefinition)
		{
			definition = part1;
			sourceConstructorSymbol = part2;
		}
		else
		{
			definition = part2;
			sourceConstructorSymbol = part1;
		}
		SourceConstructorSymbol.InitializePartialConstructorParts(definition, sourceConstructorSymbol);
		Remove(nonTypeMembers, sourceConstructorSymbol);
	}

	private static void FixPartialEvent(ArrayBuilder<Symbol> nonTypeMembers, SourceEventSymbol part1, SourceEventSymbol part2)
	{
		SourceEventSymbol definition;
		SourceEventSymbol sourceEventSymbol;
		if (part1.IsPartialDefinition)
		{
			definition = part1;
			sourceEventSymbol = part2;
		}
		else
		{
			definition = part2;
			sourceEventSymbol = part1;
		}
		SourceEventSymbol.InitializePartialEventParts(definition, sourceEventSymbol);
		Remove(nonTypeMembers, sourceEventSymbol);
	}

	private static void Remove(ArrayBuilder<Symbol> symbols, Symbol symbol)
	{
		for (int i = 0; i < symbols.Count; i++)
		{
			if ((object)symbols[i] == symbol)
			{
				symbols.RemoveAt(i);
				return;
			}
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/SourceMemberContainerSymbol.cs", 4346);
	}

	private static void CheckForMemberConflictWithPropertyAccessor(SourceMemberContainerTypeSymbol containerForDiagnostics, bool mightHaveMembersFromDistinctNonPartialDeclarations, Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> membersByName, PropertySymbol propertySymbol, bool getNotSet, BindingDiagnosticBag diagnostics)
	{
		MethodSymbol methodSymbol = (getNotSet ? propertySymbol.GetMethod : propertySymbol.SetMethod);
		string text = (((object)methodSymbol == null) ? SourcePropertyAccessorSymbol.GetAccessorName(propertySymbol.IsIndexer ? propertySymbol.MetadataName : propertySymbol.Name, getNotSet, propertySymbol.IsCompilationOutputWinMdObj()) : methodSymbol.Name);
		ImmutableArray<Symbol> value;
		foreach (Symbol item in membersByName.TryGetValue(System.MemoryExtensions.AsMemory(text), out value) ? value : ImmutableArray<Symbol>.Empty)
		{
			if (item.Kind != SymbolKind.Method)
			{
				if (!mightHaveMembersFromDistinctNonPartialDeclarations)
				{
					diagnostics.Add(ErrorCode.ERR_DuplicateNameInClass, GetAccessorOrPropertyLocation(propertySymbol, getNotSet), containerForDiagnostics, text);
				}
				break;
			}
			MethodSymbol methodSymbol2 = (MethodSymbol)item;
			if (methodSymbol2.MethodKind == MethodKind.Ordinary && ParametersMatchPropertyAccessor(propertySymbol, getNotSet, methodSymbol2.Parameters))
			{
				diagnostics.Add(ErrorCode.ERR_MemberReserved, GetAccessorOrPropertyLocation(propertySymbol, getNotSet), text, containerForDiagnostics);
				break;
			}
		}
	}

	private static void CheckForMemberConflictWithEventAccessor(SourceMemberContainerTypeSymbol containerForDiagnostics, bool mightHaveMembersFromDistinctNonPartialDeclarations, Dictionary<ReadOnlyMemory<char>, ImmutableArray<Symbol>> membersByName, EventSymbol eventSymbol, bool isAdder, BindingDiagnosticBag diagnostics)
	{
		string accessorName = SourceEventSymbol.GetAccessorName(eventSymbol.Name, isAdder);
		ImmutableArray<Symbol> value;
		foreach (Symbol item in membersByName.TryGetValue(System.MemoryExtensions.AsMemory(accessorName), out value) ? value : ImmutableArray<Symbol>.Empty)
		{
			if (item.Kind != SymbolKind.Method)
			{
				if (!mightHaveMembersFromDistinctNonPartialDeclarations)
				{
					diagnostics.Add(ErrorCode.ERR_DuplicateNameInClass, GetAccessorOrEventLocation(eventSymbol, isAdder), containerForDiagnostics, accessorName);
				}
				break;
			}
			MethodSymbol methodSymbol = (MethodSymbol)item;
			if (methodSymbol.MethodKind == MethodKind.Ordinary && ParametersMatchEventAccessor(eventSymbol, methodSymbol.Parameters))
			{
				diagnostics.Add(ErrorCode.ERR_MemberReserved, GetAccessorOrEventLocation(eventSymbol, isAdder), accessorName, containerForDiagnostics);
				break;
			}
		}
	}

	private static Location GetAccessorOrPropertyLocation(PropertySymbol propertySymbol, bool getNotSet)
	{
		return ((Symbol)(((object)(getNotSet ? propertySymbol.GetMethod : propertySymbol.SetMethod)) ?? ((object)propertySymbol))).GetFirstLocation();
	}

	private static Location GetAccessorOrEventLocation(EventSymbol propertySymbol, bool isAdder)
	{
		return ((Symbol)(((object)(isAdder ? propertySymbol.AddMethod : propertySymbol.RemoveMethod)) ?? ((object)propertySymbol))).GetFirstLocation();
	}

	private static bool ParametersMatchPropertyAccessor(PropertySymbol propertySymbol, bool getNotSet, ImmutableArray<ParameterSymbol> methodParams)
	{
		ImmutableArray<ParameterSymbol> parameters = propertySymbol.Parameters;
		int num = parameters.Length + ((!getNotSet) ? 1 : 0);
		if (num != methodParams.Length)
		{
			return false;
		}
		for (int i = 0; i < num; i++)
		{
			ParameterSymbol parameterSymbol = methodParams[i];
			if (parameterSymbol.RefKind != RefKind.None)
			{
				return false;
			}
			if (!((i == num - 1 && !getNotSet) ? propertySymbol.TypeWithAnnotations : parameters[i].TypeWithAnnotations).Type.Equals(parameterSymbol.Type, TypeCompareKind.AllIgnoreOptions))
			{
				return false;
			}
		}
		return true;
	}

	private static bool ParametersMatchEventAccessor(EventSymbol eventSymbol, ImmutableArray<ParameterSymbol> methodParams)
	{
		if (methodParams.Length == 1 && methodParams[0].RefKind == RefKind.None)
		{
			return eventSymbol.Type.Equals(methodParams[0].Type, TypeCompareKind.AllIgnoreOptions);
		}
		return false;
	}

	private void AddEnumMembers(DeclaredMembersAndInitializersBuilder result, EnumDeclarationSyntax syntax, BindingDiagnosticBag diagnostics)
	{
		SourceEnumConstantSymbol sourceEnumConstantSymbol = null;
		int num = 0;
		foreach (EnumMemberDeclarationSyntax member in syntax.Members)
		{
			EqualsValueClauseSyntax? equalsValue = member.EqualsValue;
			SourceEnumConstantSymbol sourceEnumConstantSymbol2 = ((equalsValue == null) ? SourceEnumConstantSymbol.CreateImplicitValuedConstant(this, member, sourceEnumConstantSymbol, num, diagnostics) : SourceEnumConstantSymbol.CreateExplicitValuedConstant(this, member, diagnostics));
			result.NonTypeMembersWithPartialImplementations.Add(sourceEnumConstantSymbol2);
			if (equalsValue != null || (object)sourceEnumConstantSymbol == null)
			{
				sourceEnumConstantSymbol = sourceEnumConstantSymbol2;
				num = 1;
			}
			else
			{
				num++;
			}
		}
	}

	private static void AddInitializer(ref ArrayBuilder<FieldOrPropertyInitializer>? initializers, FieldSymbol? fieldOpt, CSharpSyntaxNode node)
	{
		if (initializers == null)
		{
			initializers = ArrayBuilder<FieldOrPropertyInitializer>.GetInstance();
		}
		else
		{
			_ = initializers.Count;
		}
		initializers.Add(new FieldOrPropertyInitializer(fieldOpt, node));
	}

	private static void AddInitializers(ArrayBuilder<ArrayBuilder<FieldOrPropertyInitializer>> allInitializers, ArrayBuilder<FieldOrPropertyInitializer>? siblingsOpt)
	{
		if (siblingsOpt != null)
		{
			allInitializers.Add(siblingsOpt);
		}
	}

	private static void CheckInterfaceMembers(ImmutableArray<Symbol> nonTypeMembers, BindingDiagnosticBag diagnostics)
	{
		foreach (Symbol item in nonTypeMembers)
		{
			CheckInterfaceMember(item, diagnostics);
		}
	}

	private static void CheckInterfaceMember(Symbol member, BindingDiagnosticBag diagnostics)
	{
		switch (member.Kind)
		{
		case SymbolKind.Method:
		{
			MethodSymbol methodSymbol = (MethodSymbol)member;
			switch (methodSymbol.MethodKind)
			{
			case MethodKind.Constructor:
				diagnostics.Add(ErrorCode.ERR_InterfacesCantContainConstructors, member.GetFirstLocation());
				break;
			case MethodKind.Destructor:
				diagnostics.Add(ErrorCode.ERR_OnlyClassesCanContainDestructors, member.GetFirstLocation());
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(methodSymbol.MethodKind);
			case MethodKind.Conversion:
			case MethodKind.EventAdd:
			case MethodKind.EventRemove:
			case MethodKind.ExplicitInterfaceImplementation:
			case MethodKind.UserDefinedOperator:
			case MethodKind.Ordinary:
			case MethodKind.PropertyGet:
			case MethodKind.PropertySet:
			case MethodKind.StaticConstructor:
			case MethodKind.LocalFunction:
				break;
			}
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(member.Kind);
		case SymbolKind.Event:
		case SymbolKind.Field:
		case SymbolKind.Property:
			break;
		}
	}

	private static void CheckExtensionMembers(ImmutableArray<Symbol> members, BindingDiagnosticBag diagnostics)
	{
		foreach (Symbol item in members)
		{
			checkExtensionMember(item, diagnostics);
		}
		static void checkExtensionMember(Symbol member, BindingDiagnosticBag bindingDiagnosticBag)
		{
			if (!IsAllowedExtensionMember(member))
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_ExtensionDisallowsMember, member.GetFirstLocation());
			}
		}
	}

	internal static bool IsAllowedExtensionMember(Symbol member)
	{
		switch (member.Kind)
		{
		case SymbolKind.Method:
		{
			MethodSymbol methodSymbol = (MethodSymbol)member;
			switch (methodSymbol.MethodKind)
			{
			case MethodKind.UserDefinedOperator:
			case MethodKind.Ordinary:
			case MethodKind.PropertyGet:
			case MethodKind.PropertySet:
				return true;
			default:
				throw ExceptionUtilities.UnexpectedValue(methodSymbol.MethodKind);
			case MethodKind.Constructor:
			case MethodKind.Conversion:
			case MethodKind.Destructor:
			case MethodKind.EventAdd:
			case MethodKind.EventRemove:
			case MethodKind.ExplicitInterfaceImplementation:
			case MethodKind.StaticConstructor:
				break;
			}
			break;
		}
		case SymbolKind.Property:
			if (!((PropertySymbol)member).IsIndexer)
			{
				return true;
			}
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(member.Kind);
		case SymbolKind.Event:
		case SymbolKind.Field:
		case SymbolKind.NamedType:
			break;
		}
		return false;
	}

	private static void CheckForStructDefaultConstructors(ArrayBuilder<Symbol> members, bool isEnum, BindingDiagnosticBag diagnostics)
	{
		foreach (Symbol member in members)
		{
			if (!(member is MethodSymbol { MethodKind: MethodKind.Constructor, ParameterCount: 0 } methodSymbol))
			{
				continue;
			}
			Location firstLocation = methodSymbol.GetFirstLocation();
			if (isEnum)
			{
				diagnostics.Add(ErrorCode.ERR_EnumsCantContainDefaultConstructor, firstLocation);
				continue;
			}
			MessageID.IDS_FeatureParameterlessStructConstructors.CheckFeatureAvailability(diagnostics, methodSymbol.DeclaringCompilation, firstLocation);
			if (methodSymbol.DeclaredAccessibility != Accessibility.Public)
			{
				diagnostics.Add(ErrorCode.ERR_NonPublicParameterlessStructConstructor, firstLocation);
			}
		}
	}

	private void CheckForStructBadInitializers(DeclaredMembersAndInitializersBuilder builder, BindingDiagnosticBag diagnostics)
	{
		if (builder.DeclarationWithParameters != null)
		{
			return;
		}
		bool flag = false;
		foreach (ArrayBuilder<FieldOrPropertyInitializer> instanceInitializer in builder.InstanceInitializers)
		{
			foreach (FieldOrPropertyInitializer item in instanceInitializer)
			{
				flag = true;
				Symbol symbol = item.FieldOpt.AssociatedSymbol ?? item.FieldOpt;
				MessageID.IDS_FeatureStructFieldInitializers.CheckFeatureAvailability(diagnostics, symbol.DeclaringCompilation, symbol.GetFirstLocation());
			}
		}
		if (flag && !builder.NonTypeMembersWithPartialImplementations.Any((Symbol member) => member is MethodSymbol methodSymbol && methodSymbol.MethodKind == MethodKind.Constructor))
		{
			diagnostics.Add(ErrorCode.ERR_StructHasInitializersAndNoDeclaredConstructor, GetFirstLocation());
		}
	}

	private void AddSynthesizedSimpleProgramEntryPointIfNecessary(MembersAndInitializersBuilder builder, DeclaredMembersAndInitializers declaredMembersAndInitializers)
	{
		foreach (SynthesizedSimpleProgramEntryPointSymbol simpleProgramEntryPoint in GetSimpleProgramEntryPoints())
		{
			builder.AddNonTypeMember(this, simpleProgramEntryPoint, declaredMembersAndInitializers);
		}
	}

	private void AddSynthesizedTypeMembersIfNecessary(MembersAndInitializersBuilder builder, DeclaredMembersAndInitializers declaredMembersAndInitializers, BindingDiagnosticBag diagnostics)
	{
		DeclarationKind kind = declaration.Kind;
		bool flag = kind - 9 <= DeclarationKind.Class;
		if (!flag && (object)declaredMembersAndInitializers.PrimaryConstructor == null)
		{
			return;
		}
		IReadOnlyCollection<Symbol> nonTypeMembers = builder.GetNonTypeMembers(this, declaredMembersAndInitializers);
		ArrayBuilder<Symbol> members = ArrayBuilder<Symbol>.GetInstance(nonTypeMembers.Count + 1);
		kind = declaration.Kind;
		if (kind - 9 > DeclarationKind.Class)
		{
			SynthesizedPrimaryConstructor primaryConstructor = declaredMembersAndInitializers.PrimaryConstructor;
			members.Add(primaryConstructor);
			members.AddRange(primaryConstructor.GetBackingFields());
			members.AddRange(nonTypeMembers);
			builder.SetNonTypeMembers(members);
			return;
		}
		ParameterListSyntax paramList = declaredMembersAndInitializers.DeclarationWithParameters?.ParameterList;
		PooledDictionary<Symbol, Symbol> memberSignatures = s_duplicateRecordMemberSignatureDictionary.Allocate();
		PooledDictionary<string, Symbol> fieldsByName = PooledDictionary<string, Symbol>.GetInstance();
		PooledHashSet<string> memberNames = PooledHashSet<string>.GetInstance();
		foreach (Symbol item2 in nonTypeMembers)
		{
			memberNames.Add(item2.Name);
			if (item2 is EventSymbol)
			{
				continue;
			}
			if (item2 is MethodSymbol { MethodKind: var methodKind })
			{
				if (methodKind != MethodKind.Constructor && methodKind != MethodKind.Ordinary)
				{
					continue;
				}
			}
			else if (item2 is FieldSymbol)
			{
				string name = item2.Name;
				if (!fieldsByName.ContainsKey(name))
				{
					fieldsByName.Add(name, item2);
				}
				continue;
			}
			if (!memberSignatures.ContainsKey(item2))
			{
				memberSignatures.Add(item2, item2);
			}
		}
		CSharpCompilation compilation = DeclaringCompilation;
		bool isRecordClass = declaration.Kind == DeclarationKind.Record;
		bool primaryAndCopyCtorAmbiguity = false;
		if (paramList != null)
		{
			SynthesizedPrimaryConstructor primaryConstructor2 = declaredMembersAndInitializers.PrimaryConstructor;
			members.Add(primaryConstructor2);
			if (!memberSignatures.ContainsKey(primaryConstructor2))
			{
				memberSignatures.Add(primaryConstructor2, primaryConstructor2);
			}
			if (primaryConstructor2.ParameterCount != 0)
			{
				ImmutableArray<Symbol> positionalMembers = addProperties(primaryConstructor2.Parameters);
				addDeconstruct(primaryConstructor2, positionalMembers);
			}
			if (isRecordClass)
			{
				primaryAndCopyCtorAmbiguity = primaryConstructor2.ParameterCount == 1 && primaryConstructor2.Parameters[0].Type.Equals(this, TypeCompareKind.AllIgnoreOptions);
			}
		}
		if (isRecordClass)
		{
			addCopyCtor(primaryAndCopyCtorAmbiguity, declaredMembersAndInitializers);
			addCloneMethod();
		}
		PropertySymbol equalityContract = (isRecordClass ? addEqualityContract() : null);
		MethodSymbol methodSymbol2 = addThisEquals(equalityContract);
		if (isRecordClass)
		{
			addBaseEquals();
		}
		addObjectEquals(methodSymbol2);
		MethodSymbol methodSymbol3 = addGetHashCode(equalityContract);
		addEqualityOperators();
		if (!(methodSymbol2 is SynthesizedRecordEquals) && methodSymbol3 is SynthesizedRecordGetHashCode)
		{
			diagnostics.Add(ErrorCode.WRN_RecordEqualsWithoutGetHashCode, methodSymbol2.GetFirstLocation(), declaration.Name);
		}
		MethodSymbol printMethod = addPrintMembersMethod(nonTypeMembers);
		addToStringMethod(printMethod);
		memberSignatures.Free();
		fieldsByName.Free();
		memberNames.Free();
		members.AddRange(nonTypeMembers);
		builder.SetNonTypeMembers(members);
		void addBaseEquals()
		{
			if (!BaseTypeNoUseSiteDiagnostics.IsObjectType())
			{
				members.Add(new SynthesizedRecordBaseEquals(this, members.Count));
			}
		}
		void addCloneMethod()
		{
			members.Add(new SynthesizedRecordClone(this, members.Count));
		}
		void addCopyCtor(bool flag2, DeclaredMembersAndInitializers declaredMembersAndInitializers2)
		{
			SignatureOnlyMethodSymbol key = new SignatureOnlyMethodSymbol(".ctor", this, MethodKind.Constructor, Microsoft.Cci.CallingConvention.HasThis, ImmutableArray<TypeParameterSymbol>.Empty, ImmutableArray.Create((ParameterSymbol)new SignatureOnlyParameterSymbol(TypeWithAnnotations.Create(this), ImmutableArray<CustomModifier>.Empty, isParamsArray: false, isParamsCollection: false, RefKind.None)), RefKind.None, isInitOnly: false, isStatic: false, TypeWithAnnotations.Create(compilation.GetSpecialType(SpecialType.System_Void)), ImmutableArray<CustomModifier>.Empty, ImmutableArray<MethodSymbol>.Empty);
			if (!memberSignatures.TryGetValue(key, out Symbol value))
			{
				SynthesizedRecordCopyCtor synthesizedRecordCopyCtor = new SynthesizedRecordCopyCtor(this, members.Count);
				members.Add(synthesizedRecordCopyCtor);
				if (flag2)
				{
					diagnostics.Add(ErrorCode.ERR_RecordAmbigCtor, synthesizedRecordCopyCtor.GetFirstLocation());
				}
			}
			else
			{
				MethodSymbol methodSymbol4 = (MethodSymbol)value;
				if (((object)methodSymbol4 == declaredMembersAndInitializers2.PrimaryConstructor) & flag2)
				{
					diagnostics.Add(ErrorCode.ERR_RecordAmbigCtor, GetFirstLocation());
				}
				else if (!IsSealed && methodSymbol4.DeclaredAccessibility != Accessibility.Public && methodSymbol4.DeclaredAccessibility != Accessibility.Protected)
				{
					diagnostics.Add(ErrorCode.ERR_CopyConstructorWrongAccessibility, methodSymbol4.GetFirstLocation(), methodSymbol4);
				}
			}
		}
		void addDeconstruct(SynthesizedPrimaryConstructor ctor, ImmutableArray<Symbol> positionalMembers2)
		{
			SignatureOnlyMethodSymbol signatureOnlyMethodSymbol = new SignatureOnlyMethodSymbol("Deconstruct", this, MethodKind.Ordinary, Microsoft.Cci.CallingConvention.HasThis, ImmutableArray<TypeParameterSymbol>.Empty, ctor.Parameters.SelectAsArray((Func<ParameterSymbol, ParameterSymbol>)((ParameterSymbol param) => new SignatureOnlyParameterSymbol(param.TypeWithAnnotations, ImmutableArray<CustomModifier>.Empty, isParamsArray: false, isParamsCollection: false, RefKind.Out))), RefKind.None, isInitOnly: false, isStatic: false, TypeWithAnnotations.Create(compilation.GetSpecialType(SpecialType.System_Void)), ImmutableArray<CustomModifier>.Empty, ImmutableArray<MethodSymbol>.Empty);
			if (!memberSignatures.TryGetValue(signatureOnlyMethodSymbol, out Symbol value))
			{
				members.Add(new SynthesizedRecordDeconstruct(this, ctor, positionalMembers2, members.Count));
			}
			else
			{
				MethodSymbol methodSymbol4 = (MethodSymbol)value;
				if (methodSymbol4.DeclaredAccessibility != Accessibility.Public)
				{
					diagnostics.Add(ErrorCode.ERR_NonPublicAPIInRecord, methodSymbol4.GetFirstLocation(), methodSymbol4);
				}
				if (methodSymbol4.ReturnType.SpecialType != SpecialType.System_Void && !methodSymbol4.ReturnType.IsErrorType())
				{
					diagnostics.Add(ErrorCode.ERR_SignatureMismatchInRecord, methodSymbol4.GetFirstLocation(), methodSymbol4, signatureOnlyMethodSymbol.ReturnType);
				}
				if (methodSymbol4.IsStatic)
				{
					diagnostics.Add(ErrorCode.ERR_StaticAPIInRecord, methodSymbol4.GetFirstLocation(), methodSymbol4);
				}
			}
		}
		PropertySymbol addEqualityContract()
		{
			SignatureOnlyPropertySymbol signatureOnlyPropertySymbol = new SignatureOnlyPropertySymbol("EqualityContract", this, ImmutableArray<ParameterSymbol>.Empty, RefKind.None, TypeWithAnnotations.Create(compilation.GetWellKnownType(WellKnownType.System_Type)), ImmutableArray<CustomModifier>.Empty, isStatic: false, ImmutableArray<PropertySymbol>.Empty);
			PropertySymbol propertySymbol;
			if (!memberSignatures.TryGetValue(signatureOnlyPropertySymbol, out Symbol value))
			{
				propertySymbol = new SynthesizedRecordEqualityContractProperty(this, diagnostics);
				members.Add(propertySymbol);
				members.Add(propertySymbol.GetMethod);
			}
			else
			{
				propertySymbol = (PropertySymbol)value;
				if (IsSealed && BaseTypeNoUseSiteDiagnostics.IsObjectType())
				{
					if (propertySymbol.DeclaredAccessibility != Accessibility.Private)
					{
						diagnostics.Add(ErrorCode.ERR_NonPrivateAPIInRecord, propertySymbol.GetFirstLocation(), propertySymbol);
					}
				}
				else if (propertySymbol.DeclaredAccessibility != Accessibility.Protected)
				{
					diagnostics.Add(ErrorCode.ERR_NonProtectedAPIInRecord, propertySymbol.GetFirstLocation(), propertySymbol);
				}
				if (!propertySymbol.Type.Equals(signatureOnlyPropertySymbol.Type, TypeCompareKind.AllIgnoreOptions))
				{
					if (!propertySymbol.Type.IsErrorType())
					{
						diagnostics.Add(ErrorCode.ERR_SignatureMismatchInRecord, propertySymbol.GetFirstLocation(), propertySymbol, signatureOnlyPropertySymbol.Type);
					}
				}
				else
				{
					SynthesizedRecordEqualityContractProperty.VerifyOverridesEqualityContractFromBase(propertySymbol, diagnostics);
				}
				if ((object)propertySymbol.GetMethod == null)
				{
					diagnostics.Add(ErrorCode.ERR_EqualityContractRequiresGetter, propertySymbol.GetFirstLocation(), propertySymbol);
				}
				reportStaticOrNotOverridableAPIInRecord(propertySymbol, diagnostics);
			}
			return propertySymbol;
		}
		void addEqualityOperators()
		{
			members.Add(new SynthesizedRecordEqualityOperator(this, members.Count, diagnostics));
			members.Add(new SynthesizedRecordInequalityOperator(this, members.Count, diagnostics));
		}
		MethodSymbol addGetHashCode(PropertySymbol? equalityContract2)
		{
			SignatureOnlyMethodSymbol key = new SignatureOnlyMethodSymbol("GetHashCode", this, MethodKind.Ordinary, Microsoft.Cci.CallingConvention.HasThis, ImmutableArray<TypeParameterSymbol>.Empty, ImmutableArray<ParameterSymbol>.Empty, RefKind.None, isInitOnly: false, isStatic: false, TypeWithAnnotations.Create(compilation.GetSpecialType(SpecialType.System_Int32)), ImmutableArray<CustomModifier>.Empty, ImmutableArray<MethodSymbol>.Empty);
			MethodSymbol methodSymbol4;
			if (!memberSignatures.TryGetValue(key, out Symbol value))
			{
				methodSymbol4 = new SynthesizedRecordGetHashCode(this, equalityContract2, members.Count);
				members.Add(methodSymbol4);
			}
			else
			{
				methodSymbol4 = (MethodSymbol)value;
				if (!SynthesizedRecordObjectMethod.VerifyOverridesMethodFromObject(methodSymbol4, SpecialMember.System_Object__GetHashCode, diagnostics) && methodSymbol4.IsSealed && !IsSealed)
				{
					diagnostics.Add(ErrorCode.ERR_SealedAPIInRecord, methodSymbol4.GetFirstLocation(), methodSymbol4);
				}
			}
			return methodSymbol4;
		}
		void addObjectEquals(MethodSymbol thisEquals)
		{
			members.Add(new SynthesizedRecordObjEquals(this, thisEquals, members.Count));
		}
		MethodSymbol addPrintMembersMethod(IEnumerable<Symbol> userDefinedMembers)
		{
			SignatureOnlyMethodSymbol signatureOnlyMethodSymbol = new SignatureOnlyMethodSymbol("PrintMembers", this, MethodKind.Ordinary, Microsoft.Cci.CallingConvention.HasThis, ImmutableArray<TypeParameterSymbol>.Empty, ImmutableArray.Create((ParameterSymbol)new SignatureOnlyParameterSymbol(TypeWithAnnotations.Create(compilation.GetWellKnownType(WellKnownType.System_Text_StringBuilder)), ImmutableArray<CustomModifier>.Empty, isParamsArray: false, isParamsCollection: false, RefKind.None)), RefKind.None, isInitOnly: false, isStatic: false, TypeWithAnnotations.Create(compilation.GetSpecialType(SpecialType.System_Boolean)), ImmutableArray<CustomModifier>.Empty, ImmutableArray<MethodSymbol>.Empty);
			MethodSymbol methodSymbol4;
			if (!memberSignatures.TryGetValue(signatureOnlyMethodSymbol, out Symbol value))
			{
				methodSymbol4 = new SynthesizedRecordPrintMembers(this, userDefinedMembers, members.Count);
				members.Add(methodSymbol4);
			}
			else
			{
				methodSymbol4 = (MethodSymbol)value;
				if (!isRecordClass || (IsSealed && BaseTypeNoUseSiteDiagnostics.IsObjectType()))
				{
					if (methodSymbol4.DeclaredAccessibility != Accessibility.Private)
					{
						diagnostics.Add(ErrorCode.ERR_NonPrivateAPIInRecord, methodSymbol4.GetFirstLocation(), methodSymbol4);
					}
				}
				else if (methodSymbol4.DeclaredAccessibility != Accessibility.Protected)
				{
					diagnostics.Add(ErrorCode.ERR_NonProtectedAPIInRecord, methodSymbol4.GetFirstLocation(), methodSymbol4);
				}
				if (!methodSymbol4.ReturnType.Equals(signatureOnlyMethodSymbol.ReturnType, TypeCompareKind.AllIgnoreOptions))
				{
					if (!methodSymbol4.ReturnType.IsErrorType())
					{
						diagnostics.Add(ErrorCode.ERR_SignatureMismatchInRecord, methodSymbol4.GetFirstLocation(), methodSymbol4, signatureOnlyMethodSymbol.ReturnType);
					}
				}
				else if (isRecordClass)
				{
					SynthesizedRecordPrintMembers.VerifyOverridesPrintMembersFromBase(methodSymbol4, diagnostics);
				}
				reportStaticOrNotOverridableAPIInRecord(methodSymbol4, diagnostics);
			}
			return methodSymbol4;
		}
		ImmutableArray<Symbol> addProperties(ImmutableArray<ParameterSymbol> recordParameters)
		{
			ArrayBuilder<Symbol> existingOrAddedMembers = ArrayBuilder<Symbol>.GetInstance(recordParameters.Length);
			int addedCount = 0;
			foreach (ParameterSymbol item3 in recordParameters)
			{
				ParameterSymbol param = item3;
				bool flag2 = false;
				CSharpSyntaxNode nonNullSyntaxNode = param.GetNonNullSyntaxNode();
				SignatureOnlyPropertySymbol signatureOnlyPropertySymbol = new SignatureOnlyPropertySymbol(param.Name, this, ImmutableArray<ParameterSymbol>.Empty, RefKind.None, param.TypeWithAnnotations, ImmutableArray<CustomModifier>.Empty, isStatic: false, ImmutableArray<PropertySymbol>.Empty);
				if (!memberSignatures.TryGetValue(signatureOnlyPropertySymbol, out Symbol value) && !fieldsByName.TryGetValue(param.Name, out value))
				{
					value = OverriddenOrHiddenMembersHelpers.FindFirstHiddenMemberIfAny(signatureOnlyPropertySymbol, memberIsFromSomeCompilation: true);
					flag2 = true;
				}
				if ((object)value == null)
				{
					addProperty(new SynthesizedRecordPropertySymbol(this, nonNullSyntaxNode, param, isOverride: false, diagnostics));
				}
				else if (value is FieldSymbol fieldSymbol && !value.IsStatic && fieldSymbol.TypeWithAnnotations.Equals(param.TypeWithAnnotations, TypeCompareKind.AllIgnoreOptions))
				{
					Binder.CheckFeatureAvailability(nonNullSyntaxNode, MessageID.IDS_FeaturePositionalFieldsInRecords, diagnostics);
					if (!flag2 || checkMemberNotHidden(fieldSymbol, param))
					{
						existingOrAddedMembers.Add(fieldSymbol);
					}
				}
				else if (value is PropertySymbol propertySymbol && !value.IsStatic && (object)propertySymbol.GetMethod != null && propertySymbol.TypeWithAnnotations.Equals(param.TypeWithAnnotations, TypeCompareKind.AllIgnoreOptions))
				{
					if (flag2 && propertySymbol.IsAbstract)
					{
						addProperty(new SynthesizedRecordPropertySymbol(this, nonNullSyntaxNode, param, isOverride: true, diagnostics));
					}
					else if (!flag2 || checkMemberNotHidden(propertySymbol, param))
					{
						existingOrAddedMembers.Add(propertySymbol);
					}
				}
				else
				{
					diagnostics.Add(ErrorCode.ERR_BadRecordMemberForPositionalParameter, param.GetFirstLocation(), new FormattedSymbol(value, SymbolDisplayFormat.CSharpErrorMessageFormat.WithMemberOptions(SymbolDisplayMemberOptions.IncludeContainingType)), param.TypeWithAnnotations, param.Name);
				}
				void addProperty(SynthesizedRecordPropertySymbol property)
				{
					existingOrAddedMembers.Add(property);
					members.Add(property);
					members.Add(property.GetMethod);
					members.Add(property.SetMethod);
					SynthesizedBackingFieldSymbol declaredBackingField = property.DeclaredBackingField;
					members.Add(declaredBackingField);
					builder.AddInstanceInitializerForPositionalMembers(new FieldOrPropertyInitializer(property.BackingField, paramList.Parameters[param.Ordinal]));
					addedCount++;
				}
			}
			return existingOrAddedMembers.ToImmutableAndFree();
		}
		MethodSymbol addThisEquals(PropertySymbol? equalityContract2)
		{
			SignatureOnlyMethodSymbol signatureOnlyMethodSymbol = new SignatureOnlyMethodSymbol("Equals", this, MethodKind.Ordinary, Microsoft.Cci.CallingConvention.HasThis, ImmutableArray<TypeParameterSymbol>.Empty, ImmutableArray.Create((ParameterSymbol)new SignatureOnlyParameterSymbol(TypeWithAnnotations.Create(this), ImmutableArray<CustomModifier>.Empty, isParamsArray: false, isParamsCollection: false, RefKind.None)), RefKind.None, isInitOnly: false, isStatic: false, TypeWithAnnotations.Create(compilation.GetSpecialType(SpecialType.System_Boolean)), ImmutableArray<CustomModifier>.Empty, ImmutableArray<MethodSymbol>.Empty);
			MethodSymbol methodSymbol4;
			if (!memberSignatures.TryGetValue(signatureOnlyMethodSymbol, out Symbol value))
			{
				methodSymbol4 = new SynthesizedRecordEquals(this, equalityContract2, members.Count);
				members.Add(methodSymbol4);
			}
			else
			{
				methodSymbol4 = (MethodSymbol)value;
				if (methodSymbol4.DeclaredAccessibility != Accessibility.Public)
				{
					diagnostics.Add(ErrorCode.ERR_NonPublicAPIInRecord, methodSymbol4.GetFirstLocation(), methodSymbol4);
				}
				if (methodSymbol4.ReturnType.SpecialType != SpecialType.System_Boolean && !methodSymbol4.ReturnType.IsErrorType())
				{
					diagnostics.Add(ErrorCode.ERR_SignatureMismatchInRecord, methodSymbol4.GetFirstLocation(), methodSymbol4, signatureOnlyMethodSymbol.ReturnType);
				}
				reportStaticOrNotOverridableAPIInRecord(methodSymbol4, diagnostics);
			}
			return methodSymbol4;
		}
		void addToStringMethod(MethodSymbol printMethod2)
		{
			SignatureOnlyMethodSymbol key = new SignatureOnlyMethodSymbol("ToString", this, MethodKind.Ordinary, Microsoft.Cci.CallingConvention.HasThis, ImmutableArray<TypeParameterSymbol>.Empty, ImmutableArray<ParameterSymbol>.Empty, RefKind.None, isInitOnly: false, isStatic: false, TypeWithAnnotations.Create(compilation.GetSpecialType(SpecialType.System_String)), ImmutableArray<CustomModifier>.Empty, ImmutableArray<MethodSymbol>.Empty);
			MethodSymbol methodSymbol4 = getBaseToStringMethod();
			Symbol value;
			if ((object)methodSymbol4 != null && methodSymbol4.IsSealed)
			{
				if (methodSymbol4.ContainingModule != ContainingModule && !DeclaringCompilation.IsFeatureEnabled(MessageID.IDS_FeatureSealedToStringInRecord))
				{
					LanguageVersion languageVersion = ((CSharpParseOptions)GetFirstLocation().SourceTree.Options).LanguageVersion;
					LanguageVersion version = MessageID.IDS_FeatureSealedToStringInRecord.RequiredVersion();
					diagnostics.Add(ErrorCode.ERR_InheritingFromRecordWithSealedToString, GetFirstLocation(), languageVersion.ToDisplayString(), new CSharpRequiredLanguageVersion(version));
				}
			}
			else if (!memberSignatures.TryGetValue(key, out value))
			{
				SynthesizedRecordToString item = new SynthesizedRecordToString(this, printMethod2, members.Count);
				members.Add(item);
			}
			else
			{
				MethodSymbol methodSymbol5 = (MethodSymbol)value;
				if (!SynthesizedRecordObjectMethod.VerifyOverridesMethodFromObject(methodSymbol5, SpecialMember.System_Object__ToString, diagnostics) && methodSymbol5.IsSealed && !IsSealed)
				{
					MessageID.IDS_FeatureSealedToStringInRecord.CheckFeatureAvailability(diagnostics, DeclaringCompilation, methodSymbol5.GetFirstLocation());
				}
			}
		}
		bool checkMemberNotHidden(Symbol symbol, ParameterSymbol param)
		{
			if (memberNames.Contains(symbol.Name) || GetTypeMembersDictionary().ContainsKey(System.MemoryExtensions.AsMemory(symbol.Name)))
			{
				diagnostics.Add(ErrorCode.ERR_HiddenPositionalMember, param.GetFirstLocation(), symbol);
				return false;
			}
			return true;
		}
		MethodSymbol? getBaseToStringMethod()
		{
			Symbol specialTypeMember = DeclaringCompilation.GetSpecialTypeMember(SpecialMember.System_Object__ToString);
			NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
			while ((object)baseTypeNoUseSiteDiagnostics != null)
			{
				foreach (Symbol simpleNonTypeMember in baseTypeNoUseSiteDiagnostics.GetSimpleNonTypeMembers("ToString"))
				{
					if (simpleNonTypeMember is MethodSymbol methodSymbol4 && methodSymbol4.GetLeastOverriddenMethod(null) == specialTypeMember)
					{
						return methodSymbol4;
					}
				}
				baseTypeNoUseSiteDiagnostics = baseTypeNoUseSiteDiagnostics.BaseTypeNoUseSiteDiagnostics;
			}
			return null;
		}
		void reportStaticOrNotOverridableAPIInRecord(Symbol symbol, BindingDiagnosticBag bindingDiagnosticBag)
		{
			if (isRecordClass && !IsSealed && ((!symbol.IsAbstract && !symbol.IsVirtual && !symbol.IsOverride) || symbol.IsSealed))
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_NotOverridableAPIInRecord, symbol.GetFirstLocation(), symbol);
			}
			else if (symbol.IsStatic)
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_StaticAPIInRecord, symbol.GetFirstLocation(), symbol);
			}
		}
	}

	private void AddSynthesizedConstructorsIfNecessary(MembersAndInitializersBuilder builder, DeclaredMembersAndInitializers declaredMembersAndInitializers, BindingDiagnosticBag diagnostics)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		foreach (Symbol nonTypeMember in builder.GetNonTypeMembers(this, declaredMembersAndInitializers))
		{
			if (nonTypeMember.Kind == SymbolKind.Method)
			{
				MethodSymbol methodSymbol = (MethodSymbol)nonTypeMember;
				switch (methodSymbol.MethodKind)
				{
				case MethodKind.Constructor:
					if (!IsRecord || !SynthesizedRecordCopyCtor.HasCopyConstructorSignature(methodSymbol) || methodSymbol is SynthesizedPrimaryConstructor)
					{
						flag = true;
						flag2 = flag2 || methodSymbol.ParameterCount == 0;
					}
					break;
				case MethodKind.StaticConstructor:
					flag3 = true;
					break;
				}
			}
			if (flag & flag2 & flag3)
			{
				break;
			}
		}
		if ((!flag2 && this.IsStructType()) || (!flag && !IsStatic && !IsInterface))
		{
			builder.AddNonTypeMember(this, (TypeKind == TypeKind.Submission) ? new SynthesizedSubmissionConstructor(this, diagnostics) : new SynthesizedInstanceConstructor(this), declaredMembersAndInitializers);
		}
		if (!flag3 && hasNonConstantInitializer(declaredMembersAndInitializers.StaticInitializers))
		{
			builder.AddNonTypeMember(this, new SynthesizedStaticConstructor(this), declaredMembersAndInitializers);
		}
		if (IsScriptClass)
		{
			SynthesizedInteractiveInitializerMethod synthesizedInteractiveInitializerMethod = new SynthesizedInteractiveInitializerMethod(this, diagnostics);
			builder.AddNonTypeMember(this, synthesizedInteractiveInitializerMethod, declaredMembersAndInitializers);
			SynthesizedEntryPointSymbol member = SynthesizedEntryPointSymbol.Create(synthesizedInteractiveInitializerMethod, diagnostics);
			builder.AddNonTypeMember(this, member, declaredMembersAndInitializers);
		}
		static bool hasNonConstantInitializer(ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> initializers)
		{
			return initializers.Any((ImmutableArray<FieldOrPropertyInitializer> siblings) => siblings.Any((FieldOrPropertyInitializer initializer) => !initializer.FieldOpt.IsConst));
		}
	}

	private void AddSynthesizedTupleMembersIfNecessary(MembersAndInitializersBuilder builder, DeclaredMembersAndInitializers declaredMembersAndInitializers)
	{
		if (!IsTupleType)
		{
			return;
		}
		ArrayBuilder<Symbol> arrayBuilder = MakeSynthesizedTupleMembers(declaredMembersAndInitializers.GetNonTypeMembers(this));
		if (arrayBuilder != null)
		{
			foreach (Symbol item in arrayBuilder)
			{
				builder.AddNonTypeMember(this, item, declaredMembersAndInitializers);
			}
			arrayBuilder.Free();
		}
	}

	private void AddNonTypeMembers(DeclaredMembersAndInitializersBuilder builder, SyntaxList<MemberDeclarationSyntax> members, BindingDiagnosticBag diagnostics)
	{
		if (members.Count == 0)
		{
			return;
		}
		MemberDeclarationSyntax syntaxNode = members[0];
		Binder binder = GetBinder(syntaxNode);
		ArrayBuilder<FieldOrPropertyInitializer> initializers = null;
		ArrayBuilder<FieldOrPropertyInitializer> initializers2 = null;
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		foreach (MemberDeclarationSyntax item5 in members)
		{
			if (_lazyMembersAndInitializers != null)
			{
				return;
			}
			bool flag = !item5.HasErrors;
			switch (item5.Kind())
			{
			case SyntaxKind.FieldDeclaration:
			{
				FieldDeclarationSyntax fieldDeclarationSyntax = (FieldDeclarationSyntax)item5;
				fieldDeclarationSyntax.Declaration.Type.SkipScoped(out var _).SkipRefInField(out var refKind);
				reportMisplacedMemberInNamespace(fieldDeclarationSyntax, fieldDeclarationSyntax.Declaration.Variables.First().Identifier, flag);
				DeclarationModifiers declarationModifiers = SourceMemberFieldSymbol.MakeModifiers(this, fieldDeclarationSyntax.Declaration.Variables[0].Identifier, fieldDeclarationSyntax.Modifiers, refKind != RefKind.None, diagnostics, out var modifierErrors);
				foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables)
				{
					SourceMemberFieldSymbolFromDeclarator sourceMemberFieldSymbolFromDeclarator = (((declarationModifiers & DeclarationModifiers.Fixed) == 0) ? new SourceMemberFieldSymbolFromDeclarator(this, variable, declarationModifiers, modifierErrors, diagnostics) : new SourceFixedFieldSymbol(this, variable, declarationModifiers, modifierErrors, diagnostics));
					builder.NonTypeMembersWithPartialImplementations.Add(sourceMemberFieldSymbolFromDeclarator);
					builder.UpdateIsNullableEnabledForConstructorsAndFields(sourceMemberFieldSymbolFromDeclarator.IsStatic, declaringCompilation, variable);
					if (IsScriptClass)
					{
						ExpressionFieldFinder.FindExpressionVariables(builder.NonTypeMembersWithPartialImplementations, variable, this, DeclarationModifiers.Private | (declarationModifiers & DeclarationModifiers.Static), sourceMemberFieldSymbolFromDeclarator);
					}
					if (variable.Initializer != null)
					{
						if (sourceMemberFieldSymbolFromDeclarator.IsStatic)
						{
							AddInitializer(ref initializers, sourceMemberFieldSymbolFromDeclarator, variable.Initializer);
						}
						else
						{
							AddInitializer(ref initializers2, sourceMemberFieldSymbolFromDeclarator, variable.Initializer);
						}
					}
				}
				break;
			}
			case SyntaxKind.MethodDeclaration:
			{
				MethodDeclarationSyntax methodDeclarationSyntax = (MethodDeclarationSyntax)item5;
				reportMisplacedMemberInNamespace(methodDeclarationSyntax, methodDeclarationSyntax.Identifier, flag);
				SourceOrdinaryMethodSymbol item2 = SourceOrdinaryMethodSymbol.CreateMethodSymbol(this, binder, methodDeclarationSyntax, declaringCompilation.IsNullableAnalysisEnabledIn(methodDeclarationSyntax), diagnostics);
				builder.NonTypeMembersWithPartialImplementations.Add(item2);
				break;
			}
			case SyntaxKind.ConstructorDeclaration:
			{
				ConstructorDeclarationSyntax constructorDeclarationSyntax = (ConstructorDeclarationSyntax)item5;
				reportMisplacedMemberInNamespace(constructorDeclarationSyntax, constructorDeclarationSyntax.Identifier, flag);
				bool flag2 = declaringCompilation.IsNullableAnalysisEnabledIn(constructorDeclarationSyntax);
				SourceConstructorSymbol sourceConstructorSymbol = SourceConstructorSymbol.CreateConstructorSymbol(this, constructorDeclarationSyntax, flag2, diagnostics);
				builder.NonTypeMembersWithPartialImplementations.Add(sourceConstructorSymbol);
				ConstructorInitializerSyntax? initializer2 = constructorDeclarationSyntax.Initializer;
				if (initializer2 == null || initializer2.Kind() != SyntaxKind.ThisConstructorInitializer)
				{
					builder.UpdateIsNullableEnabledForConstructorsAndFields(sourceConstructorSymbol.IsStatic, flag2);
				}
				break;
			}
			case SyntaxKind.DestructorDeclaration:
			{
				DestructorDeclarationSyntax destructorDeclarationSyntax = (DestructorDeclarationSyntax)item5;
				reportMisplacedMemberInNamespace(destructorDeclarationSyntax, destructorDeclarationSyntax.Identifier, flag);
				SourceDestructorSymbol item = new SourceDestructorSymbol(this, destructorDeclarationSyntax, declaringCompilation.IsNullableAnalysisEnabledIn(destructorDeclarationSyntax), diagnostics);
				builder.NonTypeMembersWithPartialImplementations.Add(item);
				break;
			}
			case SyntaxKind.PropertyDeclaration:
			{
				PropertyDeclarationSyntax propertyDeclarationSyntax = (PropertyDeclarationSyntax)item5;
				reportMisplacedMemberInNamespace(propertyDeclarationSyntax, propertyDeclarationSyntax.Identifier, flag);
				SourcePropertySymbol sourcePropertySymbol = SourcePropertySymbol.Create(this, binder, propertyDeclarationSyntax, diagnostics);
				builder.NonTypeMembersWithPartialImplementations.Add(sourcePropertySymbol);
				AddAccessorIfAvailable(builder.NonTypeMembersWithPartialImplementations, sourcePropertySymbol.GetMethod);
				AddAccessorIfAvailable(builder.NonTypeMembersWithPartialImplementations, sourcePropertySymbol.SetMethod);
				FieldSymbol declaredBackingField = sourcePropertySymbol.DeclaredBackingField;
				if ((object)declaredBackingField == null)
				{
					break;
				}
				builder.NonTypeMembersWithPartialImplementations.Add(declaredBackingField);
				builder.UpdateIsNullableEnabledForConstructorsAndFields(declaredBackingField.IsStatic, declaringCompilation, propertyDeclarationSyntax);
				EqualsValueClauseSyntax initializer = propertyDeclarationSyntax.Initializer;
				if (initializer != null)
				{
					if (IsScriptClass)
					{
						ExpressionFieldFinder.FindExpressionVariables(builder.NonTypeMembersWithPartialImplementations, initializer, this, (DeclarationModifiers)(0x100 | (sourcePropertySymbol.IsStatic ? 4 : 0)), declaredBackingField);
					}
					if (sourcePropertySymbol.IsStatic)
					{
						AddInitializer(ref initializers, declaredBackingField, initializer);
					}
					else
					{
						AddInitializer(ref initializers2, declaredBackingField, initializer);
					}
				}
				break;
			}
			case SyntaxKind.EventFieldDeclaration:
			{
				EventFieldDeclarationSyntax eventFieldDeclarationSyntax = (EventFieldDeclarationSyntax)item5;
				reportMisplacedMemberInNamespace(eventFieldDeclarationSyntax, eventFieldDeclarationSyntax.Declaration.Variables.First().Identifier, flag);
				foreach (VariableDeclaratorSyntax variable2 in eventFieldDeclarationSyntax.Declaration.Variables)
				{
					SourceFieldLikeEventSymbol sourceFieldLikeEventSymbol = new SourceFieldLikeEventSymbol(this, binder, eventFieldDeclarationSyntax.Modifiers, variable2, diagnostics);
					builder.NonTypeMembersWithPartialImplementations.Add(sourceFieldLikeEventSymbol);
					FieldSymbol associatedField = sourceFieldLikeEventSymbol.AssociatedField;
					if (IsScriptClass)
					{
						ExpressionFieldFinder.FindExpressionVariables(builder.NonTypeMembersWithPartialImplementations, variable2, this, (DeclarationModifiers)(0x100 | (sourceFieldLikeEventSymbol.IsStatic ? 4 : 0)), associatedField);
					}
					if ((object)associatedField != null)
					{
						builder.UpdateIsNullableEnabledForConstructorsAndFields(associatedField.IsStatic, declaringCompilation, variable2);
						if (variable2.Initializer != null)
						{
							if (associatedField.IsStatic)
							{
								AddInitializer(ref initializers, associatedField, variable2.Initializer);
							}
							else
							{
								AddInitializer(ref initializers2, associatedField, variable2.Initializer);
							}
						}
					}
					AddAccessorIfAvailable(builder.NonTypeMembersWithPartialImplementations, sourceFieldLikeEventSymbol.AddMethod);
					AddAccessorIfAvailable(builder.NonTypeMembersWithPartialImplementations, sourceFieldLikeEventSymbol.RemoveMethod);
				}
				break;
			}
			case SyntaxKind.EventDeclaration:
			{
				EventDeclarationSyntax eventDeclarationSyntax = (EventDeclarationSyntax)item5;
				reportMisplacedMemberInNamespace(eventDeclarationSyntax, eventDeclarationSyntax.Identifier, flag);
				SourceCustomEventSymbol sourceCustomEventSymbol = new SourceCustomEventSymbol(this, binder, eventDeclarationSyntax, diagnostics);
				builder.NonTypeMembersWithPartialImplementations.Add(sourceCustomEventSymbol);
				AddAccessorIfAvailable(builder.NonTypeMembersWithPartialImplementations, sourceCustomEventSymbol.AddMethod);
				AddAccessorIfAvailable(builder.NonTypeMembersWithPartialImplementations, sourceCustomEventSymbol.RemoveMethod);
				break;
			}
			case SyntaxKind.IndexerDeclaration:
			{
				IndexerDeclarationSyntax indexerDeclarationSyntax = (IndexerDeclarationSyntax)item5;
				reportMisplacedMemberInNamespace(indexerDeclarationSyntax, indexerDeclarationSyntax.ThisKeyword, flag);
				SourcePropertySymbol sourcePropertySymbol2 = SourcePropertySymbol.Create(this, binder, indexerDeclarationSyntax, diagnostics);
				builder.HaveIndexers = true;
				builder.NonTypeMembersWithPartialImplementations.Add(sourcePropertySymbol2);
				AddAccessorIfAvailable(builder.NonTypeMembersWithPartialImplementations, sourcePropertySymbol2.GetMethod);
				AddAccessorIfAvailable(builder.NonTypeMembersWithPartialImplementations, sourcePropertySymbol2.SetMethod);
				break;
			}
			case SyntaxKind.ConversionOperatorDeclaration:
			{
				ConversionOperatorDeclarationSyntax conversionOperatorDeclarationSyntax = (ConversionOperatorDeclarationSyntax)item5;
				reportMisplacedMemberInNamespace(conversionOperatorDeclarationSyntax, conversionOperatorDeclarationSyntax.OperatorKeyword, flag);
				SourceUserDefinedConversionSymbol item4 = SourceUserDefinedConversionSymbol.CreateUserDefinedConversionSymbol(this, binder, conversionOperatorDeclarationSyntax, declaringCompilation.IsNullableAnalysisEnabledIn(conversionOperatorDeclarationSyntax), diagnostics);
				builder.NonTypeMembersWithPartialImplementations.Add(item4);
				break;
			}
			case SyntaxKind.OperatorDeclaration:
			{
				OperatorDeclarationSyntax operatorDeclarationSyntax = (OperatorDeclarationSyntax)item5;
				reportMisplacedMemberInNamespace(operatorDeclarationSyntax, operatorDeclarationSyntax.OperatorKeyword, flag);
				SourceUserDefinedOperatorSymbol item3 = SourceUserDefinedOperatorSymbol.CreateUserDefinedOperatorSymbol(this, binder, operatorDeclarationSyntax, declaringCompilation.IsNullableAnalysisEnabledIn(operatorDeclarationSyntax), diagnostics);
				builder.NonTypeMembersWithPartialImplementations.Add(item3);
				break;
			}
			case SyntaxKind.GlobalStatement:
			{
				StatementSyntax statement = ((GlobalStatementSyntax)item5).Statement;
				if (IsScriptClass)
				{
					StatementSyntax statementSyntax = statement;
					while (statementSyntax.Kind() == SyntaxKind.LabeledStatement)
					{
						statementSyntax = ((LabeledStatementSyntax)statementSyntax).Statement;
					}
					switch (statementSyntax.Kind())
					{
					case SyntaxKind.LocalDeclarationStatement:
						foreach (VariableDeclaratorSyntax variable3 in ((LocalDeclarationStatementSyntax)statementSyntax).Declaration.Variables)
						{
							ExpressionFieldFinder.FindExpressionVariables(builder.NonTypeMembersWithPartialImplementations, variable3, this, DeclarationModifiers.Private, null);
						}
						break;
					case SyntaxKind.ExpressionStatement:
					case SyntaxKind.ReturnStatement:
					case SyntaxKind.YieldReturnStatement:
					case SyntaxKind.ThrowStatement:
					case SyntaxKind.LockStatement:
					case SyntaxKind.IfStatement:
					case SyntaxKind.SwitchStatement:
						ExpressionFieldFinder.FindExpressionVariables(builder.NonTypeMembersWithPartialImplementations, statementSyntax, this, DeclarationModifiers.Private, null);
						break;
					}
					AddInitializer(ref initializers2, null, statement);
				}
				else if (flag && !SyntaxFacts.IsSimpleProgramTopLevelStatement((GlobalStatementSyntax)item5))
				{
					diagnostics.Add(ErrorCode.ERR_GlobalStatement, new SourceLocation(statement));
				}
				break;
			}
			}
		}
		AddInitializers(builder.InstanceInitializers, initializers2);
		AddInitializers(builder.StaticInitializers, initializers);
		void reportMisplacedMemberInNamespace(SyntaxNode member, SyntaxToken locationSyntax, bool reportMisplacedGlobalCode)
		{
			if (IsImplicitClass & reportMisplacedGlobalCode)
			{
				ErrorCode code = (member.Parent.IsKind(SyntaxKind.CompilationUnit) ? ErrorCode.ERR_CompilationUnitUnexpected : ErrorCode.ERR_NamespaceUnexpected);
				diagnostics.Add(code, new SourceLocation(in locationSyntax));
			}
		}
	}

	private void AddAccessorIfAvailable(ArrayBuilder<Symbol> symbols, MethodSymbol? accessorOpt)
	{
		if ((object)accessorOpt != null)
		{
			symbols.Add(accessorOpt);
		}
	}

	internal override byte? GetLocalNullableContextValue()
	{
		if (!_flags.TryGetNullableContext(out var value))
		{
			value = ComputeNullableContextValue();
			_flags.SetNullableContext(value);
		}
		return value;
	}

	private byte? ComputeNullableContextValue()
	{
		if (IsExtension)
		{
			return null;
		}
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		if (!declaringCompilation.ShouldEmitNullableAttributes(this))
		{
			return null;
		}
		MostCommonNullableValueBuilder builder = default(MostCommonNullableValueBuilder);
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
		if ((object)baseTypeNoUseSiteDiagnostics != null)
		{
			builder.AddValue(TypeWithAnnotations.Create(baseTypeNoUseSiteDiagnostics));
		}
		foreach (NamedTypeSymbol item in GetInterfacesToEmit())
		{
			builder.AddValue(TypeWithAnnotations.Create(item));
		}
		foreach (TypeParameterSymbol typeParameter in TypeParameters)
		{
			typeParameter.GetCommonNullableValues(declaringCompilation, ref builder);
		}
		foreach (Symbol item2 in GetMembersUnordered())
		{
			item2.GetCommonNullableValues(declaringCompilation, ref builder);
		}
		return builder.MostCommonValue;
	}

	internal bool IsNullableEnabledForConstructorsAndInitializers(bool useStatic)
	{
		MembersAndInitializers membersAndInitializers = GetMembersAndInitializers();
		if (!useStatic)
		{
			return membersAndInitializers.IsNullableEnabledForInstanceConstructorsAndFields;
		}
		return membersAndInitializers.IsNullableEnabledForStaticConstructorsAndFields;
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
		if ((object)baseTypeNoUseSiteDiagnostics != null)
		{
			if (baseTypeNoUseSiteDiagnostics.ContainsDynamic())
			{
				Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeDynamicAttribute(baseTypeNoUseSiteDiagnostics, 0));
			}
			if (declaringCompilation.ShouldEmitNativeIntegerAttributes(baseTypeNoUseSiteDiagnostics))
			{
				Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeNativeIntegerAttribute(this, baseTypeNoUseSiteDiagnostics));
			}
			if (baseTypeNoUseSiteDiagnostics.ContainsTupleNames())
			{
				Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeTupleNamesAttribute(baseTypeNoUseSiteDiagnostics));
			}
		}
		if (declaringCompilation.ShouldEmitNullableAttributes(this))
		{
			if (ShouldEmitNullableContextValue(out var value))
			{
				Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeNullableContextAttribute(this, value));
			}
			if ((object)baseTypeNoUseSiteDiagnostics != null)
			{
				Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeNullableAttributeIfNecessary(this, value, TypeWithAnnotations.Create(baseTypeNoUseSiteDiagnostics)));
			}
		}
	}

	internal void DiscoverInterceptors(ArrayBuilder<NamespaceOrTypeSymbol> toSearch)
	{
		foreach (NamedTypeSymbol typeMember in GetTypeMembers())
		{
			toSearch.Add(typeMember);
		}
		if (!declaration.AnyMemberHasAttributes)
		{
			return;
		}
		foreach (Symbol item in GetMembersUnordered())
		{
			if (item is MethodSymbol { MethodKind: MethodKind.Ordinary })
			{
				item.GetAttributes();
			}
		}
	}

	internal ExtensionGroupingInfo GetExtensionGroupingInfo()
	{
		if (_lazyExtensionGroupingInfo == null)
		{
			Interlocked.CompareExchange(ref _lazyExtensionGroupingInfo, new ExtensionGroupingInfo(this), null);
		}
		return _lazyExtensionGroupingInfo;
	}

	internal SynthesizedExplicitImplementations GetSynthesizedExplicitImplementations(CancellationToken cancellationToken)
	{
		if (_lazySynthesizedExplicitImplementations == null)
		{
			BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				CheckMembersAgainstBaseType(instance, cancellationToken);
				cancellationToken.ThrowIfCancellationRequested();
				CheckAbstractClassImplementations(instance);
				cancellationToken.ThrowIfCancellationRequested();
				CheckInterfaceUnification(instance);
				if (IsInterface)
				{
					cancellationToken.ThrowIfCancellationRequested();
					this.CheckInterfaceVarianceSafety(instance);
				}
				if (Interlocked.CompareExchange(ref _lazySynthesizedExplicitImplementations, ComputeInterfaceImplementations(instance, cancellationToken), null) == null)
				{
					AddDeclarationDiagnostics(instance);
					state.NotePartComplete(CompletionPart.SynthesizedExplicitImplementations);
				}
			}
			finally
			{
				instance.Free();
			}
		}
		return _lazySynthesizedExplicitImplementations;
	}

	internal sealed override IEnumerable<(MethodSymbol Body, MethodSymbol Implemented)> SynthesizedInterfaceMethodImpls()
	{
		SynthesizedExplicitImplementations synthesizedImplementations = GetSynthesizedExplicitImplementations(default(CancellationToken));
		foreach (var methodImpl in synthesizedImplementations.MethodImpls)
		{
			yield return methodImpl;
		}
		foreach (SynthesizedExplicitImplementationForwardingMethod forwardingMethod in synthesizedImplementations.ForwardingMethods)
		{
			yield return (Body: forwardingMethod.ImplementingMethod, Implemented: forwardingMethod.ExplicitInterfaceImplementations.Single());
		}
	}

	private void CheckAbstractClassImplementations(BindingDiagnosticBag diagnostics)
	{
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
		if (IsAbstract || (object)baseTypeNoUseSiteDiagnostics == null || !baseTypeNoUseSiteDiagnostics.IsAbstract)
		{
			return;
		}
		foreach (Symbol abstractMember in base.AbstractMembers)
		{
			if (abstractMember.Kind == SymbolKind.Method && !(abstractMember is SynthesizedRecordOrdinaryMethod))
			{
				diagnostics.Add(ErrorCode.ERR_UnimplementedAbstractMethod, GetFirstLocation(), this, abstractMember);
			}
		}
	}

	private SynthesizedExplicitImplementations ComputeInterfaceImplementations(BindingDiagnosticBag diagnostics, CancellationToken cancellationToken)
	{
		ArrayBuilder<SynthesizedExplicitImplementationForwardingMethod> instance = ArrayBuilder<SynthesizedExplicitImplementationForwardingMethod>.GetInstance();
		ArrayBuilder<(MethodSymbol, MethodSymbol)> instance2 = ArrayBuilder<(MethodSymbol, MethodSymbol)>.GetInstance();
		MultiDictionary<NamedTypeSymbol, NamedTypeSymbol> interfacesAndTheirBaseInterfacesNoUseSiteDiagnostics = base.InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics;
		foreach (NamedTypeSymbol allInterfacesNoUseSiteDiagnostic in base.AllInterfacesNoUseSiteDiagnostics)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (!interfacesAndTheirBaseInterfacesNoUseSiteDiagnostics[allInterfacesNoUseSiteDiagnostic].Contains(allInterfacesNoUseSiteDiagnostic))
			{
				continue;
			}
			HasBaseTypeDeclaringInterfaceResult? hasBaseTypeDeclaringInterfaceResult = null;
			foreach (Symbol member in allInterfacesNoUseSiteDiagnostic.GetMembers())
			{
				cancellationToken.ThrowIfCancellationRequested();
				SymbolKind kind = member.Kind;
				if ((kind != SymbolKind.Event && kind != SymbolKind.Method && kind != SymbolKind.Property) || !member.IsImplementableInterfaceMember())
				{
					continue;
				}
				SymbolAndDiagnostics symbolAndDiagnostics;
				if (IsInterface)
				{
					MultiDictionary<Symbol, Symbol>.ValueSet explicitImplementationForInterfaceMember = GetExplicitImplementationForInterfaceMember(member);
					int count = explicitImplementationForInterfaceMember.Count;
					if (count == 0)
					{
						continue;
					}
					if (count == 1)
					{
						symbolAndDiagnostics = new SymbolAndDiagnostics(explicitImplementationForInterfaceMember.Single(), ReadOnlyBindingDiagnostic<AssemblySymbol>.Empty);
					}
					else
					{
						Diagnostic item = new CSDiagnostic(new CSDiagnosticInfo(ErrorCode.ERR_DuplicateExplicitImpl, member), GetFirstLocation());
						symbolAndDiagnostics = new SymbolAndDiagnostics(null, new ReadOnlyBindingDiagnostic<AssemblySymbol>(ImmutableArray.Create(item), default(ImmutableArray<AssemblySymbol>)));
					}
				}
				else
				{
					symbolAndDiagnostics = FindImplementationForInterfaceMemberInNonInterfaceWithDiagnostics(member);
				}
				Symbol symbol = symbolAndDiagnostics.Symbol;
				(SynthesizedExplicitImplementationForwardingMethod, (MethodSymbol, MethodSymbol)?) tuple = SynthesizeInterfaceMemberImplementation(symbolAndDiagnostics, member);
				bool flag = (object)symbol != null;
				var (synthesizedExplicitImplementationForwardingMethod, _) = tuple;
				if ((object)synthesizedExplicitImplementationForwardingMethod != null)
				{
					if (synthesizedExplicitImplementationForwardingMethod.IsVararg)
					{
						diagnostics.Add(ErrorCode.ERR_InterfaceImplementedImplicitlyByVariadic, TypeSymbol.GetImplicitImplementationDiagnosticLocation(member, this, symbol), symbol, member, this);
					}
					else
					{
						instance.Add(synthesizedExplicitImplementationForwardingMethod);
					}
				}
				(MethodSymbol, MethodSymbol)? item2 = tuple.Item2;
				if (item2.HasValue)
				{
					(MethodSymbol, MethodSymbol) valueOrDefault = item2.GetValueOrDefault();
					instance2.Add(valueOrDefault);
				}
				if (flag && kind == SymbolKind.Event)
				{
					EventSymbol eventSymbol = (EventSymbol)member;
					EventSymbol eventSymbol2 = (EventSymbol)symbol;
					EventSymbol eventSymbol3;
					EventSymbol eventSymbol4;
					if (eventSymbol.IsWindowsRuntimeEvent)
					{
						eventSymbol3 = eventSymbol;
						eventSymbol4 = eventSymbol2;
					}
					else
					{
						eventSymbol3 = eventSymbol2;
						eventSymbol4 = eventSymbol;
					}
					if (eventSymbol.IsWindowsRuntimeEvent != eventSymbol2.IsWindowsRuntimeEvent)
					{
						object[] args = new object[4] { eventSymbol2, eventSymbol, eventSymbol3, eventSymbol4 };
						CSDiagnosticInfo info = new CSDiagnosticInfo(ErrorCode.ERR_MixingWinRTEventWithRegular, args, ImmutableArray<Symbol>.Empty, ImmutableArray.Create(GetFirstLocation()));
						diagnostics.Add(info, eventSymbol2.GetFirstLocation());
					}
				}
				Symbol symbol2 = ((kind == SymbolKind.Method) ? ((MethodSymbol)member).AssociatedSymbol : null);
				if ((object)symbol2 != null && !ReportAccessorOfInterfacePropertyOrEvent(symbol2) && (!flag || symbol.IsAccessor()))
				{
					continue;
				}
				bool flag2 = false;
				if (symbolAndDiagnostics.Diagnostics.Diagnostics.Any())
				{
					diagnostics.AddRange(symbolAndDiagnostics.Diagnostics);
					flag2 = symbolAndDiagnostics.Diagnostics.Diagnostics.Any((Diagnostic d) => d.Severity == DiagnosticSeverity.Error);
				}
				if (flag2)
				{
					continue;
				}
				if (!flag || (!symbol.ContainingType.Equals(this, TypeCompareKind.ConsiderEverything) && symbol.GetExplicitInterfaceImplementations().Contains(member, ExplicitInterfaceImplementationTargetMemberEqualityComparer.Instance)))
				{
					hasBaseTypeDeclaringInterfaceResult = hasBaseTypeDeclaringInterfaceResult ?? HasBaseClassDeclaringInterface(allInterfacesNoUseSiteDiagnostic);
					HasBaseTypeDeclaringInterfaceResult matchResult = hasBaseTypeDeclaringInterfaceResult.GetValueOrDefault();
					if (((matchResult != HasBaseTypeDeclaringInterfaceResult.ExactMatch) & flag) && symbol.ContainingType.IsInterface)
					{
						HasBaseInterfaceDeclaringInterface(symbol.ContainingType, allInterfacesNoUseSiteDiagnostic, ref matchResult);
					}
					switch (matchResult)
					{
					case HasBaseTypeDeclaringInterfaceResult.NoMatch:
						if (!member.MustCallMethodsDirectly() && !member.IsIndexedProperty())
						{
							DiagnosticInfo diagnosticInfo = member.GetUseSiteInfo().DiagnosticInfo;
							if (diagnosticInfo != null && diagnosticInfo.DefaultSeverity == DiagnosticSeverity.Error)
							{
								diagnostics.Add(diagnosticInfo, GetImplementsLocationOrFallback(allInterfacesNoUseSiteDiagnostic));
								break;
							}
							diagnostics.Add(ErrorCode.ERR_UnimplementedInterfaceMember, GetImplementsLocationOrFallback(allInterfacesNoUseSiteDiagnostic), this, member);
						}
						break;
					case HasBaseTypeDeclaringInterfaceResult.IgnoringNullableMatch:
						diagnostics.Add(ErrorCode.WRN_NullabilityMismatchInInterfaceImplementedByBase, GetImplementsLocationOrFallback(allInterfacesNoUseSiteDiagnostic), this, member);
						break;
					default:
						throw ExceptionUtilities.UnexpectedValue(matchResult);
					case HasBaseTypeDeclaringInterfaceResult.ExactMatch:
						break;
					}
				}
				if (flag && kind == SymbolKind.Method && ((object)tuple.Item1 != null || TypeSymbol.Equals(symbol.ContainingType, this, TypeCompareKind.ConsiderEverything)))
				{
					UseSiteInfo<AssemblySymbol> useSiteInfo = member.GetUseSiteInfo();
					Location location = (symbol.IsFromCompilation(DeclaringCompilation) ? symbol.GetFirstLocation() : GetFirstLocation());
					diagnostics.Add(useSiteInfo, location);
				}
			}
		}
		return SynthesizedExplicitImplementations.Create(instance.ToImmutableAndFree(), instance2.ToImmutableAndFree());
	}

	protected abstract Location GetCorrespondingBaseListLocation(NamedTypeSymbol @base);

	private Location GetImplementsLocationOrFallback(NamedTypeSymbol implementedInterface)
	{
		return GetImplementsLocation(implementedInterface) ?? GetFirstLocation();
	}

	internal Location? GetImplementsLocation(NamedTypeSymbol implementedInterface)
	{
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
		NamedTypeSymbol namedTypeSymbol = null;
		foreach (NamedTypeSymbol item in InterfacesNoUseSiteDiagnostics())
		{
			if (TypeSymbol.Equals(item, implementedInterface, TypeCompareKind.ConsiderEverything))
			{
				namedTypeSymbol = item;
				break;
			}
			if ((object)namedTypeSymbol == null && item.ImplementsInterface(implementedInterface, ref useSiteInfo))
			{
				namedTypeSymbol = item;
			}
		}
		return GetCorrespondingBaseListLocation(namedTypeSymbol);
	}

	private bool ReportAccessorOfInterfacePropertyOrEvent(Symbol interfacePropertyOrEvent)
	{
		if (interfacePropertyOrEvent.IsIndexedProperty())
		{
			return true;
		}
		Symbol symbol;
		if (IsInterface)
		{
			MultiDictionary<Symbol, Symbol>.ValueSet explicitImplementationForInterfaceMember = GetExplicitImplementationForInterfaceMember(interfacePropertyOrEvent);
			switch (explicitImplementationForInterfaceMember.Count)
			{
			case 0:
				return true;
			case 1:
				symbol = explicitImplementationForInterfaceMember.Single();
				break;
			default:
				symbol = null;
				break;
			}
		}
		else
		{
			symbol = FindImplementationForInterfaceMemberInNonInterface(interfacePropertyOrEvent);
		}
		if ((object)symbol == null)
		{
			return false;
		}
		if (interfacePropertyOrEvent.Kind == SymbolKind.Event && symbol.Kind == SymbolKind.Event && ((EventSymbol)interfacePropertyOrEvent).IsWindowsRuntimeEvent != ((EventSymbol)symbol).IsWindowsRuntimeEvent)
		{
			return false;
		}
		return true;
	}

	private HasBaseTypeDeclaringInterfaceResult HasBaseClassDeclaringInterface(NamedTypeSymbol @interface)
	{
		HasBaseTypeDeclaringInterfaceResult result = HasBaseTypeDeclaringInterfaceResult.NoMatch;
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
		while ((object)baseTypeNoUseSiteDiagnostics != null && !DeclaresBaseInterface(baseTypeNoUseSiteDiagnostics, @interface, ref result))
		{
			baseTypeNoUseSiteDiagnostics = baseTypeNoUseSiteDiagnostics.BaseTypeNoUseSiteDiagnostics;
		}
		return result;
	}

	private static bool DeclaresBaseInterface(NamedTypeSymbol currType, NamedTypeSymbol @interface, ref HasBaseTypeDeclaringInterfaceResult result)
	{
		MultiDictionary<NamedTypeSymbol, NamedTypeSymbol>.ValueSet valueSet = currType.InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics[@interface];
		if (valueSet.Count != 0)
		{
			if (valueSet.Contains(@interface))
			{
				result = HasBaseTypeDeclaringInterfaceResult.ExactMatch;
				return true;
			}
			if (result == HasBaseTypeDeclaringInterfaceResult.NoMatch && valueSet.Contains(@interface, SymbolEqualityComparer.IgnoringNullable))
			{
				result = HasBaseTypeDeclaringInterfaceResult.IgnoringNullableMatch;
			}
		}
		return false;
	}

	private void HasBaseInterfaceDeclaringInterface(NamedTypeSymbol baseInterface, NamedTypeSymbol @interface, ref HasBaseTypeDeclaringInterfaceResult matchResult)
	{
		if (DeclaresBaseInterface(baseInterface, @interface, ref matchResult))
		{
			return;
		}
		foreach (NamedTypeSymbol allInterfacesNoUseSiteDiagnostic in base.AllInterfacesNoUseSiteDiagnostics)
		{
			if ((object)allInterfacesNoUseSiteDiagnostic != baseInterface && allInterfacesNoUseSiteDiagnostic.Equals(baseInterface, TypeCompareKind.CLRSignatureCompareOptions) && DeclaresBaseInterface(allInterfacesNoUseSiteDiagnostic, @interface, ref matchResult))
			{
				break;
			}
		}
	}

	private void CheckMembersAgainstBaseType(BindingDiagnosticBag diagnostics, CancellationToken cancellationToken)
	{
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
		if ((object)baseTypeNoUseSiteDiagnostics != null && baseTypeNoUseSiteDiagnostics.IsErrorType())
		{
			return;
		}
		bool flag = IsRecord && !baseTypeNoUseSiteDiagnostics.IsObjectType() && !baseTypeNoUseSiteDiagnostics.IsRecord;
		switch (TypeKind)
		{
		case TypeKind.Delegate:
		case TypeKind.Enum:
		case TypeKind.Extension:
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(TypeKind);
		case TypeKind.Class:
		case TypeKind.Interface:
		case TypeKind.Struct:
		case TypeKind.Submission:
			foreach (Symbol item in GetMembersUnordered())
			{
				cancellationToken.ThrowIfCancellationRequested();
				bool wasAmbiguous = flag;
				if (wasAmbiguous)
				{
					bool flag2 = ((item is SynthesizedRecordBaseEquals || item is SynthesizedRecordEqualityContractProperty || item is SynthesizedRecordPrintMembers) ? true : false);
					wasAmbiguous = flag2;
				}
				if (wasAmbiguous)
				{
					continue;
				}
				bool suppressAccessors;
				switch (item.Kind)
				{
				case SymbolKind.Method:
				{
					MethodSymbol methodSymbol = (MethodSymbol)item;
					if (MethodSymbol.CanOverrideOrHide(methodSymbol.MethodKind) && !methodSymbol.IsAccessor())
					{
						if (item.IsOverride)
						{
							CheckOverrideMember(methodSymbol, methodSymbol.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
						}
						else if (methodSymbol is SourceMemberMethodSymbol { IsNew: var isNew3 })
						{
							CheckNonOverrideMember(methodSymbol, isNew3, methodSymbol.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
						}
					}
					else if (methodSymbol.MethodKind == MethodKind.Destructor)
					{
						MethodSymbol firstRuntimeOverriddenMethodIgnoringNewSlot = methodSymbol.GetFirstRuntimeOverriddenMethodIgnoringNewSlot(out wasAmbiguous);
						if ((object)firstRuntimeOverriddenMethodIgnoringNewSlot != null && firstRuntimeOverriddenMethodIgnoringNewSlot.IsMetadataFinal)
						{
							diagnostics.Add(ErrorCode.ERR_CantOverrideSealed, methodSymbol.GetFirstLocation(), methodSymbol, firstRuntimeOverriddenMethodIgnoringNewSlot);
						}
					}
					break;
				}
				case SymbolKind.Property:
				{
					PropertySymbol propertySymbol = (PropertySymbol)item;
					MethodSymbol getMethod = propertySymbol.GetMethod;
					MethodSymbol setMethod = propertySymbol.SetMethod;
					if (item.IsOverride)
					{
						CheckOverrideMember(propertySymbol, propertySymbol.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
						if (!suppressAccessors)
						{
							if ((object)getMethod != null)
							{
								CheckOverrideMember(getMethod, getMethod.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
							}
							if ((object)setMethod != null)
							{
								CheckOverrideMember(setMethod, setMethod.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
							}
						}
					}
					else
					{
						if (!(propertySymbol is SourcePropertySymbolBase { IsNew: var isNew2 }))
						{
							break;
						}
						CheckNonOverrideMember(propertySymbol, isNew2, propertySymbol.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
						if (!suppressAccessors)
						{
							if ((object)getMethod != null)
							{
								CheckNonOverrideMember(getMethod, isNew2, getMethod.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
							}
							if ((object)setMethod != null)
							{
								CheckNonOverrideMember(setMethod, isNew2, setMethod.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
							}
						}
					}
					break;
				}
				case SymbolKind.Event:
				{
					EventSymbol eventSymbol = (EventSymbol)item;
					MethodSymbol addMethod = eventSymbol.AddMethod;
					MethodSymbol removeMethod = eventSymbol.RemoveMethod;
					if (item.IsOverride)
					{
						CheckOverrideMember(eventSymbol, eventSymbol.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
						if (!suppressAccessors)
						{
							if ((object)addMethod != null)
							{
								CheckOverrideMember(addMethod, addMethod.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
							}
							if ((object)removeMethod != null)
							{
								CheckOverrideMember(removeMethod, removeMethod.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
							}
						}
						break;
					}
					bool isNew4 = ((SourceEventSymbol)eventSymbol).IsNew;
					CheckNonOverrideMember(eventSymbol, isNew4, eventSymbol.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
					if (!suppressAccessors)
					{
						if ((object)addMethod != null)
						{
							CheckNonOverrideMember(addMethod, isNew4, addMethod.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
						}
						if ((object)removeMethod != null)
						{
							CheckNonOverrideMember(removeMethod, isNew4, removeMethod.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
						}
					}
					break;
				}
				case SymbolKind.Field:
				{
					bool isNew = item is SourceFieldSymbol sourceFieldSymbol && sourceFieldSymbol.IsNew;
					CheckNewModifier(item, isNew, diagnostics);
					break;
				}
				case SymbolKind.NamedType:
					CheckNewModifier(item, ((SourceMemberContainerTypeSymbol)item).IsNew, diagnostics);
					break;
				}
			}
			break;
		}
	}

	private void CheckNewModifier(Symbol symbol, bool isNew, BindingDiagnosticBag diagnostics)
	{
		if (symbol.IsImplicitlyDeclared)
		{
			return;
		}
		if (symbol.ContainingType.IsInterface)
		{
			CheckNonOverrideMember(symbol, isNew, OverriddenOrHiddenMembersHelpers.MakeInterfaceOverriddenOrHiddenMembers(symbol, memberIsFromSomeCompilation: true), diagnostics, out var _);
		}
		else
		{
			if ((object)BaseTypeNoUseSiteDiagnostics == null)
			{
				return;
			}
			int memberArity = symbol.GetMemberArity();
			Location location = symbol.TryGetFirstLocation();
			bool suppressAccessors2 = false;
			NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
			while ((object)baseTypeNoUseSiteDiagnostics != null)
			{
				foreach (Symbol member in baseTypeNoUseSiteDiagnostics.GetMembers(symbol.Name))
				{
					if (member.Kind == SymbolKind.Method && !((MethodSymbol)member).CanBeHiddenByMember(symbol))
					{
						continue;
					}
					CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, ContainingAssembly);
					bool num = AccessCheck.IsSymbolAccessible(member, this, ref useSiteInfo);
					diagnostics.Add(location, useSiteInfo);
					if (num && member.GetMemberArity() == memberArity)
					{
						if (!isNew)
						{
							diagnostics.Add(ErrorCode.WRN_NewRequired, location, symbol, member);
						}
						AddHidingAbstractDiagnostic(symbol, location, member, diagnostics, ref suppressAccessors2);
						if (member.IsRequired())
						{
							diagnostics.Add(ErrorCode.ERR_RequiredMemberCannotBeHidden, location, member, symbol);
						}
						return;
					}
				}
				baseTypeNoUseSiteDiagnostics = baseTypeNoUseSiteDiagnostics.BaseTypeNoUseSiteDiagnostics;
			}
			if (isNew)
			{
				diagnostics.Add(ErrorCode.WRN_NewNotRequired, location, symbol);
			}
		}
	}

	private void CheckOverrideMember(Symbol overridingMember, OverriddenOrHiddenMembersResult overriddenOrHiddenMembers, BindingDiagnosticBag diagnostics, out bool suppressAccessors)
	{
		suppressAccessors = false;
		bool flag = overridingMember.Kind == SymbolKind.Method;
		bool flag2 = overridingMember.Kind == SymbolKind.Property;
		_ = overridingMember.Kind;
		Location firstLocation = overridingMember.GetFirstLocation();
		ImmutableArray<Symbol> overriddenMembers = overriddenOrHiddenMembers.OverriddenMembers;
		if (overriddenMembers.Length == 0)
		{
			ImmutableArray<Symbol> hiddenMembers = overriddenOrHiddenMembers.HiddenMembers;
			if (hiddenMembers.Any())
			{
				ErrorCode code = (flag ? ErrorCode.ERR_CantOverrideNonFunction : (flag2 ? ErrorCode.ERR_CantOverrideNonProperty : ErrorCode.ERR_CantOverrideNonEvent));
				diagnostics.Add(code, firstLocation, overridingMember, hiddenMembers[0]);
			}
			else
			{
				Symbol symbol = null;
				if (flag)
				{
					symbol = ((MethodSymbol)overridingMember).AssociatedSymbol;
				}
				if ((object)symbol == null)
				{
					bool flag3 = false;
					if (flag || overridingMember.IsIndexer())
					{
						foreach (TypeWithAnnotations item in flag ? ((MethodSymbol)overridingMember).ParameterTypesWithAnnotations : ((PropertySymbol)overridingMember).ParameterTypesWithAnnotations)
						{
							if (IsOrContainsErrorType(item.Type))
							{
								flag3 = true;
								break;
							}
						}
					}
					if (!flag3)
					{
						diagnostics.Add(ErrorCode.ERR_OverrideNotExpected, firstLocation, overridingMember);
					}
				}
				else if (symbol.Kind == SymbolKind.Property)
				{
					PropertySymbol propertySymbol = (PropertySymbol)symbol;
					PropertySymbol overriddenProperty = propertySymbol.OverriddenProperty;
					if ((object)overriddenProperty != null)
					{
						if (propertySymbol.GetMethod == overridingMember && (object)overriddenProperty.GetMethod == null)
						{
							diagnostics.Add(ErrorCode.ERR_NoGetToOverride, firstLocation, overridingMember, overriddenProperty);
						}
						else if (propertySymbol.SetMethod == overridingMember && (object)overriddenProperty.SetMethod == null)
						{
							diagnostics.Add(ErrorCode.ERR_NoSetToOverride, firstLocation, overridingMember, overriddenProperty);
						}
						else
						{
							diagnostics.Add(ErrorCode.ERR_OverrideNotExpected, firstLocation, overridingMember);
						}
					}
				}
			}
		}
		else
		{
			NamedTypeSymbol containingType = overridingMember.ContainingType;
			if (overriddenMembers.Length > 1)
			{
				diagnostics.Add(ErrorCode.ERR_AmbigOverride, firstLocation, overriddenMembers[0].OriginalDefinition, overriddenMembers[1].OriginalDefinition, containingType);
				suppressAccessors = true;
			}
			else
			{
				checkSingleOverriddenMember(overridingMember, overriddenMembers[0], diagnostics, ref suppressAccessors);
			}
		}
		if (!ContainingAssembly.RuntimeSupportsCovariantReturnsOfClasses && overridingMember is MethodSymbol methodSymbol)
		{
			methodSymbol.RequiresExplicitOverride(out var warnAmbiguous);
			if (warnAmbiguous)
			{
				MethodSymbol overriddenMethod = methodSymbol.OverriddenMethod;
				diagnostics.Add(ErrorCode.WRN_MultipleRuntimeOverrideMatches, overriddenMethod.GetFirstLocation(), overriddenMethod, overridingMember);
				suppressAccessors = true;
			}
		}
		void checkSingleOverriddenMember(Symbol symbol2, Symbol overriddenMember, BindingDiagnosticBag bindingDiagnosticBag, ref bool reference)
		{
			Location firstLocation2 = symbol2.GetFirstLocation();
			bool flag4 = symbol2.Kind == SymbolKind.Method;
			bool flag5 = symbol2.Kind == SymbolKind.Property;
			bool flag6 = symbol2.Kind == SymbolKind.Event;
			_ = symbol2.ContainingType;
			if (overriddenMember.MustCallMethodsDirectly())
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_CantOverrideBogusMethod, firstLocation2, symbol2, overriddenMember);
				reference = true;
			}
			else if (!overriddenMember.IsVirtual && !overriddenMember.IsAbstract && !overriddenMember.IsOverride && (!flag4 || ((MethodSymbol)overriddenMember).MethodKind != MethodKind.Destructor))
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_CantOverrideNonVirtual, firstLocation2, symbol2, overriddenMember);
				reference = true;
			}
			else if (overriddenMember.IsSealed)
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_CantOverrideSealed, firstLocation2, symbol2, overriddenMember);
				reference = true;
			}
			else if (!OverrideHasCorrectAccessibility(overriddenMember, symbol2))
			{
				string text = SyntaxFacts.GetText(overriddenMember.DeclaredAccessibility);
				bindingDiagnosticBag.Add(ErrorCode.ERR_CantChangeAccessOnOverride, firstLocation2, symbol2, text, overriddenMember);
				reference = true;
			}
			else if (symbol2.ContainsTupleNames() && MemberSignatureComparer.ConsideringTupleNamesCreatesDifference(symbol2, overriddenMember))
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_CantChangeTupleNamesOnOverride, firstLocation2, symbol2, overriddenMember);
			}
			else if (overriddenMember is PropertySymbol { IsRequired: not false } && symbol2 is PropertySymbol { IsRequired: false })
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_OverrideMustHaveRequired, firstLocation2, symbol2, overriddenMember);
			}
			else if (overriddenMember is MethodSymbol methodSymbol2 && methodSymbol2.IsOperator() != ((MethodSymbol)symbol2).IsOperator())
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_OperatorMismatchOnOverride, firstLocation2, symbol2, overriddenMember);
			}
			else
			{
				Symbol leastOverriddenMember = overriddenMember.GetLeastOverriddenMember(overriddenMember.ContainingType);
				symbol2.ForceCompleteObsoleteAttribute();
				leastOverriddenMember.ForceCompleteObsoleteAttribute();
				bool flag7 = symbol2.ObsoleteState == ThreeState.True;
				bool flag8 = leastOverriddenMember.ObsoleteState == ThreeState.True;
				if (flag7 != flag8)
				{
					ErrorCode code2 = (flag7 ? ErrorCode.WRN_ObsoleteOverridingNonObsolete : ErrorCode.WRN_NonObsoleteOverridingObsolete);
					bindingDiagnosticBag.Add(code2, firstLocation2, symbol2, leastOverriddenMember);
				}
				if (flag5)
				{
					checkOverriddenProperty((PropertySymbol)symbol2, (PropertySymbol)overriddenMember, bindingDiagnosticBag, ref reference);
				}
				else if (flag6)
				{
					EventSymbol eventSymbol = (EventSymbol)symbol2;
					EventSymbol eventSymbol2 = (EventSymbol)overriddenMember;
					TypeWithAnnotations typeWithAnnotations = eventSymbol.TypeWithAnnotations;
					TypeWithAnnotations typeWithAnnotations2 = eventSymbol2.TypeWithAnnotations;
					if (!typeWithAnnotations.Equals(typeWithAnnotations2, TypeCompareKind.AllIgnoreOptions))
					{
						if (!IsOrContainsErrorType(typeWithAnnotations.Type))
						{
							bindingDiagnosticBag.Add(ErrorCode.ERR_CantChangeTypeOnOverride, firstLocation2, symbol2, overriddenMember, typeWithAnnotations2.Type);
						}
						reference = true;
					}
					else
					{
						CheckValidNullableEventOverride(eventSymbol.DeclaringCompilation, eventSymbol2, eventSymbol, bindingDiagnosticBag, delegate(BindingDiagnosticBag bindingDiagnosticBag2, EventSymbol overriddenEvent, EventSymbol overridingEvent, Location location)
						{
							bindingDiagnosticBag2.Add(ErrorCode.WRN_NullabilityMismatchInTypeOnOverride, location);
						}, firstLocation2);
					}
				}
				else
				{
					MethodSymbol methodSymbol3 = (MethodSymbol)symbol2;
					MethodSymbol methodSymbol4 = (MethodSymbol)overriddenMember;
					if (methodSymbol3.IsGenericMethod)
					{
						methodSymbol4 = methodSymbol4.Construct(TypeMap.TypeParametersAsTypeSymbolsWithIgnoredAnnotations(methodSymbol3.TypeParameters));
					}
					if (methodSymbol3.RefKind != methodSymbol4.RefKind)
					{
						bindingDiagnosticBag.Add(ErrorCode.ERR_CantChangeRefReturnOnOverride, firstLocation2, symbol2, overriddenMember);
					}
					else if (!IsValidOverrideReturnType(methodSymbol3, methodSymbol3.ReturnTypeWithAnnotations, methodSymbol4.ReturnTypeWithAnnotations, bindingDiagnosticBag))
					{
						if (!IsOrContainsErrorType(methodSymbol3.ReturnType))
						{
							CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
							if (DeclaringCompilation.Conversions.HasIdentityOrImplicitReferenceConversion(methodSymbol3.ReturnTypeWithAnnotations.Type, methodSymbol4.ReturnTypeWithAnnotations.Type, ref useSiteInfo))
							{
								if (!methodSymbol3.ContainingAssembly.RuntimeSupportsCovariantReturnsOfClasses)
								{
									bindingDiagnosticBag.Add(ErrorCode.ERR_RuntimeDoesNotSupportCovariantReturnsOfClasses, firstLocation2, symbol2, overriddenMember, methodSymbol4.ReturnType);
								}
								else
								{
									CSDiagnosticInfo featureAvailabilityDiagnosticInfo = MessageID.IDS_FeatureCovariantReturnsForOverrides.GetFeatureAvailabilityDiagnosticInfo(DeclaringCompilation);
									if (featureAvailabilityDiagnosticInfo == null)
									{
										throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/SourceMemberContainerSymbol_ImplementationChecks.cs", 1030);
									}
									bindingDiagnosticBag.Add(featureAvailabilityDiagnosticInfo, firstLocation2);
								}
							}
							else
							{
								bindingDiagnosticBag.Add(ErrorCode.ERR_CantChangeReturnTypeOnOverride, firstLocation2, symbol2, overriddenMember, methodSymbol4.ReturnType);
							}
						}
					}
					else if (methodSymbol4.IsRuntimeFinalizer())
					{
						bindingDiagnosticBag.Add(ErrorCode.ERR_OverrideFinalizeDeprecated, firstLocation2);
					}
					else if (!methodSymbol3.IsAccessor())
					{
						checkValidMethodOverride(firstLocation2, methodSymbol4, methodSymbol3, bindingDiagnosticBag);
					}
				}
				if (Binder.ReportUseSite(overriddenMember, bindingDiagnosticBag, symbol2.GetFirstLocation()))
				{
					reference = true;
				}
			}
			void checkOverriddenProperty(PropertySymbol overridingProperty, PropertySymbol propertySymbol4, BindingDiagnosticBag bindingDiagnosticBag2, ref bool reference2)
			{
				Location firstLocation3 = overridingProperty.GetFirstLocation();
				NamedTypeSymbol containingType2 = overridingProperty.ContainingType;
				TypeWithAnnotations typeWithAnnotations3 = overridingProperty.TypeWithAnnotations;
				TypeWithAnnotations typeWithAnnotations4 = propertySymbol4.TypeWithAnnotations;
				if (overridingProperty.RefKind != propertySymbol4.RefKind)
				{
					bindingDiagnosticBag2.Add(ErrorCode.ERR_CantChangeRefReturnOnOverride, firstLocation3, overridingProperty, propertySymbol4);
					reference2 = true;
				}
				else if (((object)overridingProperty.SetMethod == null) ? (!IsValidOverrideReturnType(overridingProperty, typeWithAnnotations3, typeWithAnnotations4, bindingDiagnosticBag2)) : (!typeWithAnnotations3.Equals(typeWithAnnotations4, TypeCompareKind.AllIgnoreOptions)))
				{
					if (!IsOrContainsErrorType(typeWithAnnotations3.Type))
					{
						CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2 = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
						if ((object)overridingProperty.SetMethod == null && DeclaringCompilation.Conversions.HasIdentityOrImplicitReferenceConversion(typeWithAnnotations3.Type, typeWithAnnotations4.Type, ref useSiteInfo2))
						{
							if (!overridingProperty.ContainingAssembly.RuntimeSupportsCovariantReturnsOfClasses)
							{
								bindingDiagnosticBag2.Add(ErrorCode.ERR_RuntimeDoesNotSupportCovariantPropertiesOfClasses, firstLocation3, symbol2, overriddenMember, typeWithAnnotations4.Type);
							}
							else
							{
								CSDiagnosticInfo featureAvailabilityDiagnosticInfo2 = MessageID.IDS_FeatureCovariantReturnsForOverrides.GetFeatureAvailabilityDiagnosticInfo(DeclaringCompilation);
								bindingDiagnosticBag2.Add(featureAvailabilityDiagnosticInfo2, firstLocation3);
							}
						}
						else
						{
							bindingDiagnosticBag2.Add(ErrorCode.ERR_CantChangeTypeOnOverride, firstLocation3, symbol2, overriddenMember, typeWithAnnotations4.Type);
						}
					}
					reference2 = true;
				}
				else
				{
					if ((object)overridingProperty.GetMethod != null)
					{
						MethodSymbol ownOrInheritedGetMethod = propertySymbol4.GetOwnOrInheritedGetMethod();
						checkValidMethodOverride(overridingProperty.GetMethod.GetFirstLocation(), ownOrInheritedGetMethod, overridingProperty.GetMethod, bindingDiagnosticBag2);
					}
					if ((object)overridingProperty.SetMethod != null)
					{
						MethodSymbol ownOrInheritedSetMethod = propertySymbol4.GetOwnOrInheritedSetMethod();
						checkValidMethodOverride(overridingProperty.SetMethod.GetFirstLocation(), ownOrInheritedSetMethod, overridingProperty.SetMethod, bindingDiagnosticBag2);
						if ((object)ownOrInheritedSetMethod != null && overridingProperty.SetMethod.IsInitOnly != ownOrInheritedSetMethod.IsInitOnly)
						{
							bindingDiagnosticBag2.Add(ErrorCode.ERR_CantChangeInitOnlyOnOverride, firstLocation3, overridingProperty, propertySymbol4);
						}
					}
				}
				if (overridingProperty.IsSealed)
				{
					MethodSymbol ownOrInheritedGetMethod2 = overridingProperty.GetOwnOrInheritedGetMethod();
					CompoundUseSiteInfo<AssemblySymbol> useSiteInfo3 = new CompoundUseSiteInfo<AssemblySymbol>(bindingDiagnosticBag2, overridingProperty.ContainingAssembly);
					if (overridingProperty.GetMethod != ownOrInheritedGetMethod2 && !AccessCheck.IsSymbolAccessible(ownOrInheritedGetMethod2, containingType2, ref useSiteInfo3))
					{
						bindingDiagnosticBag2.Add(ErrorCode.ERR_NoGetToOverride, firstLocation3, overridingProperty, propertySymbol4);
					}
					MethodSymbol ownOrInheritedSetMethod2 = overridingProperty.GetOwnOrInheritedSetMethod();
					if (overridingProperty.SetMethod != ownOrInheritedSetMethod2 && !AccessCheck.IsSymbolAccessible(ownOrInheritedSetMethod2, containingType2, ref useSiteInfo3))
					{
						bindingDiagnosticBag2.Add(ErrorCode.ERR_NoSetToOverride, firstLocation3, overridingProperty, propertySymbol4);
					}
					bindingDiagnosticBag2.Add(firstLocation3, useSiteInfo3);
				}
			}
		}
		static void checkValidMethodOverride(Location overridingMemberLocation, MethodSymbol methodSymbol2, MethodSymbol overridingMethod, BindingDiagnosticBag diagnostics2)
		{
			if (RequiresValidScopedOverrideForRefSafety(methodSymbol2, overridingMethod.TryGetThisParameter(out ParameterSymbol thisParameter) ? thisParameter : null))
			{
				CheckValidScopedOverride(methodSymbol2, overridingMethod, diagnostics2, delegate(BindingDiagnosticBag bindingDiagnosticBag, MethodSymbol baseMethod, MethodSymbol overrideMethod, ParameterSymbol overridingParameter, bool _, Location location)
				{
					bindingDiagnosticBag.Add(ReportInvalidScopedOverrideAsError(baseMethod, overrideMethod) ? ErrorCode.ERR_ScopedMismatchInParameterOfOverrideOrImplementation : ErrorCode.WRN_ScopedMismatchInParameterOfOverrideOrImplementation, location, new FormattedSymbol(overridingParameter, SymbolDisplayFormat.ShortFormat));
				}, overridingMemberLocation, allowVariance: true, invokedAsExtensionMethod: false);
			}
			CheckValidNullableMethodOverride(overridingMethod.DeclaringCompilation, methodSymbol2, overridingMethod, diagnostics2, ReportBadReturn, ReportBadParameter, overridingMemberLocation);
			CheckRefReadonlyInMismatch(methodSymbol2, overridingMethod, diagnostics2, delegate(BindingDiagnosticBag bindingDiagnosticBag, MethodSymbol _, MethodSymbol _, ParameterSymbol overridingParameter, bool _, (ParameterSymbol BaseParameter, Location Arg) arg)
			{
				var (parameterSymbol, location) = arg;
				bindingDiagnosticBag.Add(ErrorCode.WRN_OverridingDifferentRefness, location, overridingParameter, parameterSymbol);
			}, overridingMemberLocation, invokedAsExtensionMethod: false);
		}
	}

	internal static bool IsOrContainsErrorType(TypeSymbol typeSymbol)
	{
		return (object)typeSymbol.VisitType((TypeSymbol currentTypeSymbol, object unused1, bool unused2) => currentTypeSymbol.IsErrorType(), null) != null;
	}

	private bool IsValidOverrideReturnType(Symbol overridingSymbol, TypeWithAnnotations overridingReturnType, TypeWithAnnotations overriddenReturnType, BindingDiagnosticBag diagnostics)
	{
		if (overridingSymbol.ContainingAssembly.RuntimeSupportsCovariantReturnsOfClasses && DeclaringCompilation.LanguageVersion >= MessageID.IDS_FeatureCovariantReturnsForOverrides.RequiredVersion())
		{
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, ContainingAssembly);
			bool result = DeclaringCompilation.Conversions.HasIdentityOrImplicitReferenceConversion(overridingReturnType.Type, overriddenReturnType.Type, ref useSiteInfo);
			Location location = overridingSymbol.TryGetFirstLocation();
			diagnostics.Add(location, useSiteInfo);
			return result;
		}
		return overridingReturnType.Equals(overriddenReturnType, TypeCompareKind.AllIgnoreOptions);
	}

	internal static bool CheckValidNullableMethodOverride<TArg>(CSharpCompilation compilation, MethodSymbol baseMethod, MethodSymbol overrideMethod, BindingDiagnosticBag diagnostics, ReportMismatchInReturnType<TArg> reportMismatchInReturnType, ReportMismatchInParameterType<TArg> reportMismatchInParameterType, TArg extraArgument, bool invokedAsExtensionMethod = false)
	{
		if (!PerformValidNullableOverrideCheck(compilation, baseMethod, overrideMethod))
		{
			return false;
		}
		bool result = false;
		if ((baseMethod.FlowAnalysisAnnotations & FlowAnalysisAnnotations.DoesNotReturn) == FlowAnalysisAnnotations.DoesNotReturn && (overrideMethod.FlowAnalysisAnnotations & FlowAnalysisAnnotations.DoesNotReturn) != FlowAnalysisAnnotations.DoesNotReturn)
		{
			diagnostics.Add(ErrorCode.WRN_DoesNotReturnMismatch, overrideMethod.GetFirstLocation(), new FormattedSymbol(overrideMethod, SymbolDisplayFormat.MinimallyQualifiedFormat));
			result = true;
		}
		Conversions conversions = compilation.Conversions.WithNullability(includeNullability: true);
		ImmutableArray<ParameterSymbol> baseParameters = baseMethod.Parameters;
		ImmutableArray<ParameterSymbol> overrideParameters = overrideMethod.Parameters;
		int overrideParameterOffset = (invokedAsExtensionMethod ? 1 : 0);
		if (reportMismatchInReturnType != null)
		{
			TypeWithAnnotations overridingType = getNotNullIfNotNullOutputType(overrideMethod.ReturnTypeWithAnnotations, overrideMethod.ReturnNotNullIfParameterNotNull);
			if (!isValidNullableConversion(conversions, overrideMethod.RefKind, overridingType.Type, baseMethod.ReturnTypeWithAnnotations.Type))
			{
				reportMismatchInReturnType(diagnostics, baseMethod, overrideMethod, topLevel: false, extraArgument);
				return true;
			}
			if (!NullableWalker.AreParameterAnnotationsCompatible((overrideMethod.RefKind == RefKind.Ref) ? RefKind.Ref : RefKind.Out, baseMethod.ReturnTypeWithAnnotations, baseMethod.ReturnTypeFlowAnalysisAnnotations, overridingType, overrideMethod.ReturnTypeFlowAnalysisAnnotations))
			{
				reportMismatchInReturnType(diagnostics, baseMethod, overrideMethod, topLevel: true, extraArgument);
				return true;
			}
		}
		if (reportMismatchInParameterType != null)
		{
			for (int i = 0; i < baseParameters.Length; i++)
			{
				ParameterSymbol parameterSymbol = baseParameters[i];
				TypeWithAnnotations typeWithAnnotations = parameterSymbol.TypeWithAnnotations;
				int index = i + overrideParameterOffset;
				ParameterSymbol parameterSymbol2 = overrideParameters[index];
				TypeWithAnnotations overridingType2 = getNotNullIfNotNullOutputType(parameterSymbol2.TypeWithAnnotations, parameterSymbol2.NotNullIfParameterNotNull);
				if (!isValidNullableConversion(conversions, parameterSymbol2.RefKind, typeWithAnnotations.Type, overridingType2.Type))
				{
					reportMismatchInParameterType(diagnostics, baseMethod, overrideMethod, parameterSymbol2, topLevel: false, extraArgument);
					result = true;
				}
				else if (!NullableWalker.AreParameterAnnotationsCompatible(parameterSymbol2.RefKind, typeWithAnnotations, parameterSymbol.FlowAnalysisAnnotations, overridingType2, parameterSymbol2.FlowAnalysisAnnotations))
				{
					reportMismatchInParameterType(diagnostics, baseMethod, overrideMethod, parameterSymbol2, topLevel: true, extraArgument);
					result = true;
				}
			}
		}
		return result;
		TypeWithAnnotations getNotNullIfNotNullOutputType(TypeWithAnnotations outputType, ImmutableHashSet<string> notNullIfParameterNotNull)
		{
			if (!notNullIfParameterNotNull.IsEmpty)
			{
				for (int j = 0; j < baseParameters.Length; j++)
				{
					ParameterSymbol parameterSymbol3 = overrideParameters[j + overrideParameterOffset];
					ParameterSymbol parameterSymbol4 = baseParameters[j];
					if (notNullIfParameterNotNull.Contains(parameterSymbol3.Name) && NullableWalker.GetParameterState(parameterSymbol4.TypeWithAnnotations, parameterSymbol4.FlowAnalysisAnnotations).IsNotNull)
					{
						return outputType.AsNotAnnotated();
					}
				}
			}
			return outputType;
		}
		static bool isValidNullableConversion(ConversionsBase conversionsBase, RefKind refKind, TypeSymbol sourceType, TypeSymbol targetType)
		{
			switch (refKind)
			{
			case RefKind.Ref:
				return sourceType.Equals(targetType, TypeCompareKind.AllIgnoreOptionsPlusNullableWithObliviousMatchesAny);
			case RefKind.Out:
			{
				TypeSymbol typeSymbol = targetType;
				targetType = sourceType;
				sourceType = typeSymbol;
				break;
			}
			}
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
			return conversionsBase.ClassifyImplicitConversionFromType(sourceType, targetType, ref useSiteInfo).Kind != ConversionKind.NoConversion;
		}
	}

	internal static bool RequiresValidScopedOverrideForRefSafety(MethodSymbol? method, ParameterSymbol? overrideThisParameter)
	{
		if ((object)method == null)
		{
			return false;
		}
		ImmutableArray<ParameterSymbol> parameters = method.Parameters;
		if (parameters.Any(delegate(ParameterSymbol p)
		{
			if ((object)p != null)
			{
				ScopedKind effectiveScope = p.EffectiveScope;
				if (effectiveScope != ScopedKind.None)
				{
					if (effectiveScope == ScopedKind.ScopedRef && p.RefKind == RefKind.Out)
					{
						goto IL_0027;
					}
				}
				else if (p.RefKind == RefKind.Ref)
				{
					goto IL_0027;
				}
			}
			bool flag4 = false;
			goto IL_002d;
			IL_002d:
			if (flag4)
			{
				return p.Type.IsRefLikeOrAllowsRefLikeType();
			}
			return false;
			IL_0027:
			flag4 = true;
			goto IL_002d;
		}))
		{
			return true;
		}
		bool flag;
		if ((object)overrideThisParameter != null)
		{
			RefKind refKind = overrideThisParameter.RefKind;
			if (refKind - 1 <= RefKind.Ref)
			{
				flag = true;
				goto IL_004b;
			}
		}
		flag = false;
		goto IL_004b;
		IL_004b:
		bool flag2 = (flag && overrideThisParameter.Type.IsRefLikeOrAllowsRefLikeType()) || method.ReturnType.IsRefLikeOrAllowsRefLikeType();
		if (!flag2)
		{
			RefKind refKind2 = method.RefKind;
			bool flag3 = ((refKind2 == RefKind.Ref || refKind2 == RefKind.In) ? true : false);
			flag2 = flag3;
		}
		int num;
		if (flag2)
		{
			num = 1;
		}
		else
		{
			if (!parameters.Any(delegate(ParameterSymbol p)
			{
				RefKind refKind3 = p.RefKind;
				return refKind3 - 1 <= RefKind.Ref && p.Type.IsRefLikeOrAllowsRefLikeType();
			}))
			{
				return false;
			}
			num = 2;
		}
		if (parameters.Count(delegate(ParameterSymbol p)
		{
			RefKind refKind3 = p.RefKind;
			return refKind3 - 1 <= RefKind.In;
		}) >= num)
		{
			return true;
		}
		if (parameters.Any((ParameterSymbol p) => p.RefKind == RefKind.None && p.Type.IsRefLikeOrAllowsRefLikeType()))
		{
			return true;
		}
		return false;
	}

	internal static bool ReportInvalidScopedOverrideAsError(MethodSymbol baseMethod, MethodSymbol overrideMethod)
	{
		if (baseMethod.UseUpdatedEscapeRules)
		{
			return overrideMethod.UseUpdatedEscapeRules;
		}
		return false;
	}

	internal static bool CheckValidScopedOverride<TArg>(MethodSymbol? baseMethod, MethodSymbol? overrideMethod, BindingDiagnosticBag diagnostics, ReportMismatchInParameterType<TArg> reportMismatchInParameterType, TArg extraArgument, bool allowVariance, bool invokedAsExtensionMethod)
	{
		if ((object)baseMethod == null || (object)overrideMethod == null)
		{
			return false;
		}
		bool result = false;
		ImmutableArray<ParameterSymbol> parameters = baseMethod.Parameters;
		ImmutableArray<ParameterSymbol> parameters2 = overrideMethod.Parameters;
		int num = (invokedAsExtensionMethod ? 1 : 0);
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterSymbol parameterSymbol = parameters[i];
			ParameterSymbol parameterSymbol2 = parameters2[i + num];
			if (!isValidScopedConversion(allowVariance, parameterSymbol.EffectiveScope, parameterSymbol.HasUnscopedRefAttribute, parameterSymbol2.EffectiveScope, parameterSymbol2.HasUnscopedRefAttribute))
			{
				reportMismatchInParameterType(diagnostics, baseMethod, overrideMethod, parameterSymbol2, topLevel: true, extraArgument);
				result = true;
			}
		}
		return result;
		static bool isValidScopedConversion(bool flag, ScopedKind baseScope, bool baseHasUnscopedRefAttribute, ScopedKind overrideScope, bool overrideHasUnscopedRefAttribute)
		{
			if (baseScope == overrideScope)
			{
				if (baseHasUnscopedRefAttribute == overrideHasUnscopedRefAttribute)
				{
					return true;
				}
				if (flag)
				{
					return !overrideHasUnscopedRefAttribute;
				}
				return false;
			}
			if (flag)
			{
				return baseScope == ScopedKind.None;
			}
			return false;
		}
	}

	internal static void CheckRefReadonlyInMismatch<TArg>(MethodSymbol? baseMethod, MethodSymbol? overrideMethod, BindingDiagnosticBag diagnostics, ReportMismatchInParameterType<(ParameterSymbol BaseParameter, TArg Arg)> reportMismatchInParameterType, TArg extraArgument, bool invokedAsExtensionMethod)
	{
		if ((object)baseMethod == null || (object)overrideMethod == null)
		{
			return;
		}
		ImmutableArray<ParameterSymbol> parameters = baseMethod.Parameters;
		ImmutableArray<ParameterSymbol> parameters2 = overrideMethod.Parameters;
		int num = (invokedAsExtensionMethod ? 1 : 0);
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterSymbol parameterSymbol = parameters[i];
			ParameterSymbol parameterSymbol2 = parameters2[i + num];
			if (parameterSymbol.RefKind != parameterSymbol2.RefKind)
			{
				reportMismatchInParameterType(diagnostics, baseMethod, overrideMethod, parameterSymbol2, topLevel: true, (parameterSymbol, extraArgument));
			}
		}
	}

	private static bool PerformValidNullableOverrideCheck(CSharpCompilation compilation, Symbol overriddenMember, Symbol overridingMember)
	{
		if ((object)overriddenMember != null && (object)overridingMember != null && compilation != null)
		{
			return compilation.IsFeatureEnabled(MessageID.IDS_FeatureNullableReferenceTypes);
		}
		return false;
	}

	internal static void CheckValidNullableEventOverride<TArg>(CSharpCompilation compilation, EventSymbol overriddenEvent, EventSymbol overridingEvent, BindingDiagnosticBag diagnostics, Action<BindingDiagnosticBag, EventSymbol, EventSymbol, TArg> reportMismatch, TArg extraArgument)
	{
		if (PerformValidNullableOverrideCheck(compilation, overriddenEvent, overridingEvent) && !compilation.Conversions.WithNullability(includeNullability: true).HasAnyNullabilityImplicitConversion(overriddenEvent.TypeWithAnnotations, overridingEvent.TypeWithAnnotations))
		{
			reportMismatch(diagnostics, overriddenEvent, overridingEvent, extraArgument);
		}
	}

	private static void CheckNonOverrideMember(Symbol hidingMember, bool hidingMemberIsNew, OverriddenOrHiddenMembersResult overriddenOrHiddenMembers, BindingDiagnosticBag diagnostics, out bool suppressAccessors)
	{
		suppressAccessors = false;
		Location firstLocation = hidingMember.GetFirstLocation();
		ImmutableArray<Symbol> hiddenMembers = overriddenOrHiddenMembers.HiddenMembers;
		if (hiddenMembers.Length == 0)
		{
			if (hidingMemberIsNew && !hidingMember.IsAccessor())
			{
				diagnostics.Add(ErrorCode.WRN_NewNotRequired, firstLocation, hidingMember);
			}
			return;
		}
		bool flag = false;
		if (!hidingMember.ContainingType.IsInterface)
		{
			foreach (Symbol item in hiddenMembers)
			{
				flag |= AddHidingAbstractDiagnostic(hidingMember, firstLocation, item, diagnostics, ref suppressAccessors);
				if (!hidingMemberIsNew && item.Kind == hidingMember.Kind && !hidingMember.IsAccessor() && (item.IsAbstract || item.IsVirtual || item.IsOverride) && !IsShadowingSynthesizedRecordMember(hidingMember))
				{
					diagnostics.Add(ErrorCode.WRN_NewOrOverrideExpected, firstLocation, hidingMember, item);
					flag = true;
				}
				if (item.IsRequired())
				{
					diagnostics.Add(ErrorCode.ERR_RequiredMemberCannotBeHidden, firstLocation, item, hidingMember);
					flag = true;
				}
				if (flag)
				{
					break;
				}
			}
		}
		if (!hidingMemberIsNew && !IsShadowingSynthesizedRecordMember(hidingMember) && !flag && !hidingMember.IsAccessor() && (!hidingMember.IsOperator() || hiddenMembers[0].IsOperator()))
		{
			diagnostics.Add(ErrorCode.WRN_NewRequired, firstLocation, hidingMember, hiddenMembers[0]);
		}
		if (hidingMember is MethodSymbol overrideMethod && hiddenMembers[0] is MethodSymbol baseMethod)
		{
			CheckRefReadonlyInMismatch(baseMethod, overrideMethod, diagnostics, delegate(BindingDiagnosticBag bindingDiagnosticBag, MethodSymbol _, MethodSymbol _, ParameterSymbol hidingParameter, bool _, (ParameterSymbol BaseParameter, Location Arg) arg)
			{
				var (parameterSymbol, location) = arg;
				bindingDiagnosticBag.Add(ErrorCode.WRN_HidingDifferentRefness, location, hidingParameter, parameterSymbol);
			}, firstLocation, invokedAsExtensionMethod: false);
		}
	}

	private static bool IsShadowingSynthesizedRecordMember(Symbol hidingMember)
	{
		if (!(hidingMember is SynthesizedRecordEquals) && !(hidingMember is SynthesizedRecordDeconstruct))
		{
			return hidingMember is SynthesizedRecordClone;
		}
		return true;
	}

	private static bool AddHidingAbstractDiagnostic(Symbol hidingMember, Location hidingMemberLocation, Symbol hiddenMember, BindingDiagnosticBag diagnostics, ref bool suppressAccessors)
	{
		SymbolKind kind = hiddenMember.Kind;
		if (kind != SymbolKind.Event && kind != SymbolKind.Method && kind != SymbolKind.Property)
		{
			return false;
		}
		if (!hiddenMember.IsAbstract || !hidingMember.ContainingType.IsAbstract)
		{
			return false;
		}
		switch (hidingMember.DeclaredAccessibility)
		{
		case Accessibility.Protected:
		case Accessibility.ProtectedOrInternal:
		case Accessibility.Public:
			kind = hidingMember.Kind;
			if (kind == SymbolKind.Event)
			{
				goto IL_009f;
			}
			if (kind != SymbolKind.Method)
			{
				if (kind == SymbolKind.Property)
				{
					goto IL_009f;
				}
			}
			else
			{
				Symbol associatedSymbol = ((MethodSymbol)hidingMember).AssociatedSymbol;
				if ((object)associatedSymbol != null)
				{
					diagnostics.Add(ErrorCode.ERR_HidingAbstractMethod, associatedSymbol.GetFirstLocation(), associatedSymbol, hiddenMember);
					goto IL_00be;
				}
			}
			goto IL_00a3;
		default:
			throw ExceptionUtilities.UnexpectedValue(hidingMember.DeclaredAccessibility);
		case Accessibility.Private:
		case Accessibility.ProtectedAndInternal:
		case Accessibility.Internal:
			{
				return false;
			}
			IL_00be:
			return true;
			IL_009f:
			suppressAccessors = true;
			goto IL_00a3;
			IL_00a3:
			diagnostics.Add(ErrorCode.ERR_HidingAbstractMethod, hidingMemberLocation, hidingMember, hiddenMember);
			goto IL_00be;
		}
	}

	private static bool OverrideHasCorrectAccessibility(Symbol overridden, Symbol overriding)
	{
		if (!overriding.ContainingAssembly.HasInternalAccessTo(overridden.ContainingAssembly) && overridden.DeclaredAccessibility == Accessibility.ProtectedOrInternal)
		{
			return overriding.DeclaredAccessibility == Accessibility.Protected;
		}
		return overridden.DeclaredAccessibility == overriding.DeclaredAccessibility;
	}

	private void CheckInterfaceUnification(BindingDiagnosticBag diagnostics)
	{
		if (!base.IsGenericType)
		{
			return;
		}
		int count = base.InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics.Count;
		if (count < 2)
		{
			return;
		}
		NamedTypeSymbol[] array = base.InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics.Keys.ToArray();
		for (int i = 0; i < count; i++)
		{
			for (int j = i + 1; j < count; j++)
			{
				NamedTypeSymbol namedTypeSymbol = array[i];
				NamedTypeSymbol namedTypeSymbol2 = array[j];
				if (namedTypeSymbol.IsGenericType && namedTypeSymbol2.IsGenericType && TypeSymbol.Equals(namedTypeSymbol.OriginalDefinition, namedTypeSymbol2.OriginalDefinition, TypeCompareKind.ConsiderEverything) && namedTypeSymbol.CanUnifyWith(namedTypeSymbol2))
				{
					if (GetImplementsLocationOrFallback(namedTypeSymbol).SourceSpan.Start > GetImplementsLocationOrFallback(namedTypeSymbol2).SourceSpan.Start)
					{
						NamedTypeSymbol namedTypeSymbol3 = namedTypeSymbol;
						namedTypeSymbol = namedTypeSymbol2;
						namedTypeSymbol2 = namedTypeSymbol3;
					}
					diagnostics.Add(ErrorCode.ERR_UnifyingInterfaceInstantiations, GetFirstLocation(), this, namedTypeSymbol, namedTypeSymbol2);
				}
			}
		}
	}

	private (SynthesizedExplicitImplementationForwardingMethod? ForwardingMethod, (MethodSymbol Body, MethodSymbol Implemented)? MethodImpl) SynthesizeInterfaceMemberImplementation(SymbolAndDiagnostics implementingMemberAndDiagnostics, Symbol interfaceMember)
	{
		foreach (Diagnostic diagnostic in implementingMemberAndDiagnostics.Diagnostics.Diagnostics)
		{
			bool flag = diagnostic.Severity == DiagnosticSeverity.Error;
			if (flag)
			{
				int code = diagnostic.Code;
				bool flag2 = ((code == 8704 || code == 9044) ? true : false);
				flag = !flag2;
			}
			if (flag)
			{
				return default((SynthesizedExplicitImplementationForwardingMethod, (MethodSymbol, MethodSymbol)?));
			}
		}
		Symbol symbol = implementingMemberAndDiagnostics.Symbol;
		if ((object)symbol == null || symbol.Kind != SymbolKind.Method)
		{
			return default((SynthesizedExplicitImplementationForwardingMethod, (MethodSymbol, MethodSymbol)?));
		}
		MethodSymbol methodSymbol = (MethodSymbol)interfaceMember;
		MethodSymbol methodSymbol2 = (MethodSymbol)symbol;
		if (methodSymbol2.ExplicitInterfaceImplementations.Contains(methodSymbol, ExplicitInterfaceImplementationTargetMemberEqualityComparer.Instance))
		{
			return default((SynthesizedExplicitImplementationForwardingMethod, (MethodSymbol, MethodSymbol)?));
		}
		if (!methodSymbol.IsStatic)
		{
			MethodSymbol originalDefinition = methodSymbol2.OriginalDefinition;
			bool flag3 = true;
			if (MemberSignatureComparer.RuntimeImplicitImplementationComparer.Equals(methodSymbol2, methodSymbol) && IsOverrideOfPossibleImplementationUnderRuntimeRules(methodSymbol2, methodSymbol.ContainingType))
			{
				if ((object)ContainingModule == originalDefinition.ContainingModule)
				{
					if (originalDefinition is SourceMemberMethodSymbol sourceMemberMethodSymbol)
					{
						sourceMemberMethodSymbol.EnsureMetadataVirtual();
						flag3 = false;
					}
				}
				else if (methodSymbol2.IsMetadataVirtual(MethodSymbol.IsMetadataVirtualOption.IgnoreInterfaceImplementationChanges))
				{
					flag3 = false;
				}
			}
			if (!flag3)
			{
				return default((SynthesizedExplicitImplementationForwardingMethod, (MethodSymbol, MethodSymbol)?));
			}
		}
		else if ((object)methodSymbol2.ContainingType != this)
		{
			if (methodSymbol2.ContainingType.IsInterface || methodSymbol2.Equals(BaseTypeNoUseSiteDiagnostics?.FindImplementationForInterfaceMemberInNonInterfaceWithDiagnostics(methodSymbol).Symbol, TypeCompareKind.CLRSignatureCompareOptions))
			{
				return default((SynthesizedExplicitImplementationForwardingMethod, (MethodSymbol, MethodSymbol)?));
			}
		}
		else if (MemberSignatureComparer.RuntimeExplicitImplementationSignatureComparer.Equals(methodSymbol2, methodSymbol))
		{
			return (ForwardingMethod: null, MethodImpl: (methodSymbol2, methodSymbol));
		}
		return (ForwardingMethod: new SynthesizedExplicitImplementationForwardingMethod(methodSymbol, methodSymbol2, this), MethodImpl: null);
	}

	private static bool IsPossibleImplementationUnderRuntimeRules(MethodSymbol implementingMethod, NamedTypeSymbol @interface)
	{
		NamedTypeSymbol containingType = implementingMethod.ContainingType;
		if (containingType.InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics.ContainsKey(@interface))
		{
			return true;
		}
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = containingType.BaseTypeNoUseSiteDiagnostics;
		if ((object)baseTypeNoUseSiteDiagnostics != null)
		{
			return !baseTypeNoUseSiteDiagnostics.AllInterfacesNoUseSiteDiagnostics.Contains(@interface);
		}
		return true;
	}

	private static bool IsOverrideOfPossibleImplementationUnderRuntimeRules(MethodSymbol implementingMethod, NamedTypeSymbol @interface)
	{
		MethodSymbol methodSymbol = implementingMethod;
		while ((object)methodSymbol != null)
		{
			if (IsPossibleImplementationUnderRuntimeRules(methodSymbol, @interface))
			{
				return true;
			}
			methodSymbol = methodSymbol.OverriddenMethod;
		}
		return false;
	}

	internal sealed override ImmutableArray<NamedTypeSymbol> GetInterfacesToEmit()
	{
		return CalculateInterfacesToEmit();
	}
}
