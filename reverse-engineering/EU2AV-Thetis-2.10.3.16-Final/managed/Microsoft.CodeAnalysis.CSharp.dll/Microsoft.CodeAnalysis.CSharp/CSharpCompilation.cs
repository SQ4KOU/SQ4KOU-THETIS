using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel;
using Microsoft.CodeAnalysis.CSharp.Symbols.Retargeting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.RuntimeMembers;
using Microsoft.CodeAnalysis.Symbols;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

public sealed class CSharpCompilation : Compilation
{
	internal class EntryPoint
	{
		public readonly Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol? MethodSymbol;

		public readonly ReadOnlyBindingDiagnostic<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> Diagnostics;

		public static readonly EntryPoint None = new EntryPoint(null, ReadOnlyBindingDiagnostic<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.Empty);

		public EntryPoint(Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol? methodSymbol, ReadOnlyBindingDiagnostic<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> diagnostics)
		{
			MethodSymbol = methodSymbol;
			Diagnostics = diagnostics;
		}
	}

	private sealed class InterceptorKeyComparer : IEqualityComparer<(ImmutableArray<byte> ContentHash, int Position)>
	{
		public static readonly InterceptorKeyComparer Instance = new InterceptorKeyComparer();

		private InterceptorKeyComparer()
		{
		}

		public bool Equals((ImmutableArray<byte> ContentHash, int Position) x, (ImmutableArray<byte> ContentHash, int Position) y)
		{
			if (x.ContentHash.SequenceEqual(y.ContentHash))
			{
				return x.Position == y.Position;
			}
			return false;
		}

		public int GetHashCode((ImmutableArray<byte> ContentHash, int Position) obj)
		{
			return Hash.Combine(BinaryPrimitives.ReadInt32LittleEndian(obj.ContentHash.AsSpan()), obj.Position);
		}
	}

	private readonly struct ImportInfo(SyntaxTree tree, SyntaxKind kind, TextSpan span) : IEquatable<ImportInfo>
	{
		public readonly SyntaxTree Tree = tree;

		public readonly SyntaxKind Kind = kind;

		public readonly TextSpan Span = span;

		public override bool Equals(object? obj)
		{
			if (obj is ImportInfo)
			{
				return Equals((ImportInfo)obj);
			}
			return false;
		}

		public bool Equals(ImportInfo other)
		{
			if (other.Kind == Kind && other.Tree == Tree)
			{
				return other.Span == Span;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(Tree, Span.Start);
		}
	}

	private class DuplicateFilePathsVisitor : CSharpSymbolVisitor
	{
		private readonly PooledHashSet<string> _duplicatePaths = PooledHashSet<string>.GetInstance();

		private readonly DiagnosticBag _diagnostics;

		private bool _hasDuplicateFilePaths;

		public DuplicateFilePathsVisitor(DiagnosticBag diagnostics)
		{
			_diagnostics = diagnostics;
		}

		public bool CheckDuplicateFilePathsAndFree(ImmutableArray<SyntaxTree> syntaxTrees, Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol globalNamespace)
		{
			PooledHashSet<string> instance = PooledHashSet<string>.GetInstance();
			foreach (SyntaxTree item in syntaxTrees)
			{
				if (!instance.Add(item.FilePath))
				{
					_duplicatePaths.Add(item.FilePath);
				}
			}
			instance.Free();
			if (_duplicatePaths.Any())
			{
				VisitNamespace(globalNamespace);
			}
			_duplicatePaths.Free();
			return _hasDuplicateFilePaths;
		}

		public override void VisitNamespace(Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol symbol)
		{
			foreach (Symbol member in symbol.GetMembers())
			{
				if (!(member is Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol symbol2))
				{
					if (member is Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol symbol3)
					{
						VisitNamedType(symbol3);
					}
				}
				else
				{
					VisitNamespace(symbol2);
				}
			}
		}

		public override void VisitNamedType(Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol symbol)
		{
			if (symbol.IsFileLocal)
			{
				Location firstLocation = symbol.GetFirstLocation();
				string text = firstLocation.SourceTree?.FilePath;
				if (_duplicatePaths.Contains(text))
				{
					_diagnostics.Add(ErrorCode.ERR_FileTypeNonUniquePath, firstLocation, symbol, text);
					_hasDuplicateFilePaths = true;
				}
			}
		}
	}

	private abstract class AbstractSymbolSearcher
	{
		private readonly PooledDictionary<Declaration, Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol> _cache;

		private readonly CSharpCompilation _compilation;

		private readonly bool _includeNamespace;

		private readonly bool _includeType;

		private readonly bool _includeMember;

		private readonly CancellationToken _cancellationToken;

		protected AbstractSymbolSearcher(CSharpCompilation compilation, SymbolFilter filter, CancellationToken cancellationToken)
		{
			_cache = PooledDictionary<Declaration, Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol>.GetInstance();
			_compilation = compilation;
			_includeNamespace = (filter & SymbolFilter.Namespace) == SymbolFilter.Namespace;
			_includeType = (filter & SymbolFilter.Type) == SymbolFilter.Type;
			_includeMember = (filter & SymbolFilter.Member) == SymbolFilter.Member;
			_cancellationToken = cancellationToken;
		}

		protected abstract bool Matches(string name);

		protected abstract bool ShouldCheckTypeForMembers(MergedTypeDeclaration current);

		public IEnumerable<Symbol> GetSymbolsWithName()
		{
			HashSet<Symbol> hashSet = new HashSet<Symbol>();
			ArrayBuilder<MergedNamespaceOrTypeDeclaration> instance = ArrayBuilder<MergedNamespaceOrTypeDeclaration>.GetInstance();
			AppendSymbolsWithName(instance, _compilation.MergedRootDeclaration, hashSet);
			instance.Free();
			_cache.Free();
			return hashSet;
		}

		private void AppendSymbolsWithName(ArrayBuilder<MergedNamespaceOrTypeDeclaration> spine, MergedNamespaceOrTypeDeclaration current, HashSet<Symbol> set)
		{
			if (current.Kind == DeclarationKind.Namespace)
			{
				if (_includeNamespace && Matches(current.Name))
				{
					Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol spineSymbol = GetSpineSymbol(spine);
					Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol symbol = GetSymbol(spineSymbol, current);
					if (symbol != null)
					{
						set.Add(symbol);
					}
				}
			}
			else
			{
				if (_includeType && Matches(current.Name))
				{
					Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol spineSymbol2 = GetSpineSymbol(spine);
					Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol symbol2 = GetSymbol(spineSymbol2, current);
					if (symbol2 != null)
					{
						set.Add(symbol2);
					}
				}
				if (_includeMember)
				{
					MergedTypeDeclaration current2 = (MergedTypeDeclaration)current;
					if (ShouldCheckTypeForMembers(current2))
					{
						AppendMemberSymbolsWithName(spine, current2, set);
					}
				}
			}
			spine.Add(current);
			foreach (Declaration child in current.Children)
			{
				if (child is MergedNamespaceOrTypeDeclaration current4 && (_includeMember || _includeType || child.Kind == DeclarationKind.Namespace))
				{
					AppendSymbolsWithName(spine, current4, set);
				}
			}
			spine.RemoveAt(spine.Count - 1);
		}

		private void AppendMemberSymbolsWithName(ArrayBuilder<MergedNamespaceOrTypeDeclaration> spine, MergedTypeDeclaration current, HashSet<Symbol> set)
		{
			CancellationToken cancellationToken = _cancellationToken;
			cancellationToken.ThrowIfCancellationRequested();
			spine.Add(current);
			Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol spineSymbol = GetSpineSymbol(spine);
			if (spineSymbol != null)
			{
				foreach (Symbol member in spineSymbol.GetMembers())
				{
					if (!member.IsTypeOrTypeAlias() && (member.CanBeReferencedByName || member.IsExplicitInterfaceImplementation() || member.IsIndexer()) && Matches(member.Name))
					{
						set.Add(member);
					}
				}
			}
			spine.RemoveAt(spine.Count - 1);
		}

		protected Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol? GetSpineSymbol(ArrayBuilder<MergedNamespaceOrTypeDeclaration> spine)
		{
			if (spine.Count == 0)
			{
				return null;
			}
			Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol cachedSymbol = GetCachedSymbol(spine[spine.Count - 1]);
			if (cachedSymbol != null)
			{
				return cachedSymbol;
			}
			Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol namespaceOrTypeSymbol = _compilation.GlobalNamespace;
			for (int i = 1; i < spine.Count; i++)
			{
				namespaceOrTypeSymbol = GetSymbol(namespaceOrTypeSymbol, spine[i]);
			}
			return namespaceOrTypeSymbol;
		}

		private Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol? GetCachedSymbol(MergedNamespaceOrTypeDeclaration declaration)
		{
			if (!_cache.TryGetValue(declaration, out Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol value))
			{
				return null;
			}
			return value;
		}

		private Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol? GetSymbol(Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol? container, MergedNamespaceOrTypeDeclaration declaration)
		{
			if (container == null)
			{
				return _compilation.GlobalNamespace;
			}
			if (declaration.Kind == DeclarationKind.Namespace)
			{
				AddCache(container.GetMembers(declaration.Name).OfType<Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol>());
			}
			else
			{
				AddCache(container.GetTypeMembers(declaration.Name));
			}
			return GetCachedSymbol(declaration);
		}

		private void AddCache(IEnumerable<Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol> symbols)
		{
			foreach (Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol symbol in symbols)
			{
				MergedNamespaceSymbol mergedNamespaceSymbol = symbol as MergedNamespaceSymbol;
				if (mergedNamespaceSymbol != null)
				{
					_cache[mergedNamespaceSymbol.ConstituentNamespaces.OfType<SourceNamespaceSymbol>().First().MergedDeclaration] = symbol;
					continue;
				}
				SourceNamespaceSymbol sourceNamespaceSymbol = symbol as SourceNamespaceSymbol;
				if (sourceNamespaceSymbol != null)
				{
					_cache[sourceNamespaceSymbol.MergedDeclaration] = sourceNamespaceSymbol;
				}
				else if (symbol is SourceMemberContainerTypeSymbol sourceMemberContainerTypeSymbol)
				{
					_cache[sourceMemberContainerTypeSymbol.MergedDeclaration] = sourceMemberContainerTypeSymbol;
				}
			}
		}
	}

	private class PredicateSymbolSearcher : AbstractSymbolSearcher
	{
		private readonly Func<string, bool> _predicate;

		public PredicateSymbolSearcher(CSharpCompilation compilation, SymbolFilter filter, Func<string, bool> predicate, CancellationToken cancellationToken)
			: base(compilation, filter, cancellationToken)
		{
			_predicate = predicate;
		}

		protected override bool ShouldCheckTypeForMembers(MergedTypeDeclaration current)
		{
			return true;
		}

		protected override bool Matches(string name)
		{
			return _predicate(name);
		}
	}

	private class NameSymbolSearcher : AbstractSymbolSearcher
	{
		private readonly string _name;

		public NameSymbolSearcher(CSharpCompilation compilation, SymbolFilter filter, string name, CancellationToken cancellationToken)
			: base(compilation, filter, cancellationToken)
		{
			_name = name;
		}

		protected override bool ShouldCheckTypeForMembers(MergedTypeDeclaration current)
		{
			foreach (SingleTypeDeclaration declaration in current.Declarations)
			{
				if (declaration.MemberNames.Value.Contains(_name))
				{
					return true;
				}
			}
			return false;
		}

		protected override bool Matches(string name)
		{
			return _name == name;
		}
	}

	private class UsingsFromOptionsAndDiagnostics
	{
		public static readonly UsingsFromOptionsAndDiagnostics Empty = new UsingsFromOptionsAndDiagnostics
		{
			UsingNamespacesOrTypes = ImmutableArray<NamespaceOrTypeAndUsingDirective>.Empty,
			Diagnostics = null
		};

		private SymbolCompletionState _state;

		public ImmutableArray<NamespaceOrTypeAndUsingDirective> UsingNamespacesOrTypes { get; init; }

		public DiagnosticBag? Diagnostics { get; init; }

		public static UsingsFromOptionsAndDiagnostics FromOptions(CSharpCompilation compilation)
		{
			ImmutableArray<string> usings = compilation.Options.Usings;
			if (usings.Length == 0)
			{
				return Empty;
			}
			DiagnosticBag diagnosticBag = new DiagnosticBag();
			InContainerBinder inContainerBinder = new InContainerBinder(compilation.GlobalNamespace, new BuckStopsHereBinder(compilation, null));
			ArrayBuilder<NamespaceOrTypeAndUsingDirective> instance = ArrayBuilder<NamespaceOrTypeAndUsingDirective>.GetInstance();
			PooledHashSet<Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol> instance2 = PooledHashSet<Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol>.GetInstance();
			foreach (string item in usings)
			{
				if (item.IsValidClrNamespaceName())
				{
					string[] array = item.Split(new char[1] { '.' });
					NameSyntax nameSyntax = SyntaxFactory.IdentifierName(array[0]);
					for (int i = 1; i < array.Length; i++)
					{
						nameSyntax = SyntaxFactory.QualifiedName(nameSyntax, SyntaxFactory.IdentifierName(array[i]));
					}
					BindingDiagnosticBag instance3 = BindingDiagnosticBag.GetInstance();
					Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol namespaceOrTypeSymbol = inContainerBinder.BindNamespaceOrTypeSymbol(nameSyntax, instance3).NamespaceOrTypeSymbol;
					if (instance2.Add(namespaceOrTypeSymbol))
					{
						instance.Add(new NamespaceOrTypeAndUsingDirective(namespaceOrTypeSymbol, null, instance3.DependenciesBag.ToImmutableArray()));
					}
					diagnosticBag.AddRange(instance3.DiagnosticBag);
					instance3.Free();
				}
			}
			if (diagnosticBag.IsEmptyWithoutResolution)
			{
				diagnosticBag = null;
			}
			instance2.Free();
			if (instance.Count == 0 && diagnosticBag == null)
			{
				instance.Free();
				return Empty;
			}
			return new UsingsFromOptionsAndDiagnostics
			{
				UsingNamespacesOrTypes = instance.ToImmutableAndFree(),
				Diagnostics = diagnosticBag
			};
		}

		internal void Complete(CSharpCompilation compilation, CancellationToken cancellationToken)
		{
			while (true)
			{
				cancellationToken.ThrowIfCancellationRequested();
				CompletionPart nextIncompletePart = _state.NextIncompletePart;
				switch (nextIncompletePart)
				{
				case CompletionPart.StartBaseType:
					if (_state.NotePartComplete(CompletionPart.StartBaseType))
					{
						Validate(compilation);
						_state.NotePartComplete(CompletionPart.FinishBaseType);
					}
					break;
				case CompletionPart.FinishBaseType:
					_state.SpinWaitComplete(CompletionPart.FinishBaseType, cancellationToken);
					break;
				case CompletionPart.None:
					return;
				default:
					_state.NotePartComplete(CompletionPart.MethodSymbolAll | CompletionPart.StartInterfaces | CompletionPart.FinishInterfaces | CompletionPart.EnumUnderlyingType | CompletionPart.TypeArguments | CompletionPart.FinishMemberChecks | CompletionPart.MembersCompletedChecksStarted | CompletionPart.MembersCompleted);
					break;
				}
				_state.SpinWaitComplete(nextIncompletePart, cancellationToken);
			}
		}

		private void Validate(CSharpCompilation compilation)
		{
			if (this == Empty)
			{
				return;
			}
			DiagnosticBag declarationDiagnostics = compilation.DeclarationDiagnostics;
			BindingDiagnosticBag diagnostics = BindingDiagnosticBag.GetInstance();
			TypeConversions typeConversions = compilation.SourceAssembly.CorLibrary.TypeConversions;
			foreach (NamespaceOrTypeAndUsingDirective usingNamespacesOrType in UsingNamespacesOrTypes)
			{
				diagnostics.Clear();
				diagnostics.AddDependencies(usingNamespacesOrType.Dependencies);
				Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol namespaceOrType = usingNamespacesOrType.NamespaceOrType;
				if (namespaceOrType.IsType)
				{
					((Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol)namespaceOrType).CheckAllConstraints(location: NoLocation.Singleton, compilation: compilation, conversions: typeConversions, diagnostics: diagnostics);
				}
				declarationDiagnostics.AddRange(diagnostics.DiagnosticBag);
				recordImportDependencies(namespaceOrType);
			}
			if (Diagnostics != null && !Diagnostics.IsEmptyWithoutResolution)
			{
				declarationDiagnostics.AddRange(Diagnostics.AsEnumerable());
			}
			diagnostics.Free();
			void recordImportDependencies(Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol target)
			{
				if (target.IsNamespace)
				{
					diagnostics.AddAssembliesUsedByNamespaceReference((Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol)target);
				}
				compilation.AddUsedAssemblies(diagnostics.DependenciesBag);
			}
		}
	}

	internal static class TupleNamesEncoder
	{
		public static ImmutableArray<string?> Encode(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type)
		{
			ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance();
			if (!TryGetNames(type, instance))
			{
				instance.Free();
				return default(ImmutableArray<string>);
			}
			return instance.ToImmutableAndFree();
		}

		public static ImmutableArray<TypedConstant> Encode(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type, Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol stringType)
		{
			ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance();
			if (!TryGetNames(type, instance))
			{
				instance.Free();
				return default(ImmutableArray<TypedConstant>);
			}
			ImmutableArray<TypedConstant> result = instance.SelectAsArray((string? name, Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol constantType) => new TypedConstant(constantType, TypedConstantKind.Primitive, name), stringType);
			instance.Free();
			return result;
		}

		internal static bool TryGetNames(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type, ArrayBuilder<string?> namesBuilder)
		{
			type.VisitType((Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol t, ArrayBuilder<string> builder, bool _ignore) => AddNames(t, builder), namesBuilder);
			return namesBuilder.Any((string? name) => name != null);
		}

		private static bool AddNames(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type, ArrayBuilder<string?> namesBuilder)
		{
			if (type.IsTupleType)
			{
				if (type.TupleElementNames.IsDefaultOrEmpty)
				{
					namesBuilder.AddMany(null, type.TupleElementTypesWithAnnotations.Length);
				}
				else
				{
					namesBuilder.AddRange(type.TupleElementNames);
				}
			}
			return false;
		}
	}

	internal static class DynamicTransformsEncoder
	{
		internal static ImmutableArray<TypedConstant> Encode(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type, RefKind refKind, int customModifiersCount, Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol booleanType)
		{
			ArrayBuilder<bool> instance = ArrayBuilder<bool>.GetInstance();
			Encode(type, customModifiersCount, refKind, instance, addCustomModifierFlags: true);
			ImmutableArray<TypedConstant> result = instance.SelectAsArray((bool flag, Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol constantType) => new TypedConstant(constantType, TypedConstantKind.Primitive, flag), booleanType);
			instance.Free();
			return result;
		}

		internal static ImmutableArray<bool> Encode(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type, RefKind refKind, int customModifiersCount)
		{
			ArrayBuilder<bool> instance = ArrayBuilder<bool>.GetInstance();
			Encode(type, customModifiersCount, refKind, instance, addCustomModifierFlags: true);
			return instance.ToImmutableAndFree();
		}

		internal static ImmutableArray<bool> EncodeWithoutCustomModifierFlags(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type, RefKind refKind)
		{
			ArrayBuilder<bool> instance = ArrayBuilder<bool>.GetInstance();
			Encode(type, -1, refKind, instance, addCustomModifierFlags: false);
			return instance.ToImmutableAndFree();
		}

		internal static void Encode(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type, int customModifiersCount, RefKind refKind, ArrayBuilder<bool> transformFlagsBuilder, bool addCustomModifierFlags)
		{
			if (refKind != RefKind.None)
			{
				transformFlagsBuilder.Add(item: false);
			}
			if (addCustomModifierFlags)
			{
				HandleCustomModifiers(customModifiersCount, transformFlagsBuilder);
				type.VisitType((Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol typeSymbol, ArrayBuilder<bool> builder, bool isNested) => AddFlags(typeSymbol, builder, isNested, addCustomModifierFlags: true), transformFlagsBuilder);
			}
			else
			{
				type.VisitType((Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol typeSymbol, ArrayBuilder<bool> builder, bool isNested) => AddFlags(typeSymbol, builder, isNested, addCustomModifierFlags: false), transformFlagsBuilder);
			}
		}

		private static bool AddFlags(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type, ArrayBuilder<bool> transformFlagsBuilder, bool isNestedNamedType, bool addCustomModifierFlags)
		{
			switch (type.TypeKind)
			{
			case TypeKind.Dynamic:
				transformFlagsBuilder.Add(item: true);
				break;
			case TypeKind.Array:
				if (addCustomModifierFlags)
				{
					HandleCustomModifiers(((Microsoft.CodeAnalysis.CSharp.Symbols.ArrayTypeSymbol)type).ElementTypeWithAnnotations.CustomModifiers.Length, transformFlagsBuilder);
				}
				transformFlagsBuilder.Add(item: false);
				break;
			case TypeKind.Pointer:
				if (addCustomModifierFlags)
				{
					HandleCustomModifiers(((Microsoft.CodeAnalysis.CSharp.Symbols.PointerTypeSymbol)type).PointedAtTypeWithAnnotations.CustomModifiers.Length, transformFlagsBuilder);
				}
				transformFlagsBuilder.Add(item: false);
				break;
			case TypeKind.FunctionPointer:
				handleFunctionPointerType((Microsoft.CodeAnalysis.CSharp.Symbols.FunctionPointerTypeSymbol)type, transformFlagsBuilder, addCustomModifierFlags);
				return true;
			default:
				if (!isNestedNamedType)
				{
					transformFlagsBuilder.Add(item: false);
				}
				break;
			}
			return false;
			static void handleFunctionPointerType(Microsoft.CodeAnalysis.CSharp.Symbols.FunctionPointerTypeSymbol funcPtr, ArrayBuilder<bool> arrayBuilder, bool flag)
			{
				Func<Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol, (ArrayBuilder<bool>, bool), bool, bool> visitor = (Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type2, (ArrayBuilder<bool> builder, bool addCustomModifierFlags) param, bool isNestedNamedType2) => AddFlags(type2, param.builder, isNestedNamedType2, param.addCustomModifierFlags);
				arrayBuilder.Add(item: false);
				FunctionPointerMethodSymbol signature = funcPtr.Signature;
				handle(signature.RefKind, signature.RefCustomModifiers, signature.ReturnTypeWithAnnotations);
				foreach (Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol parameter in signature.Parameters)
				{
					handle(parameter.RefKind, parameter.RefCustomModifiers, parameter.TypeWithAnnotations);
				}
				void handle(RefKind refKind, ImmutableArray<CustomModifier> customModifiers, TypeWithAnnotations twa)
				{
					if (flag)
					{
						HandleCustomModifiers(customModifiers.Length, arrayBuilder);
					}
					if (refKind != RefKind.None)
					{
						arrayBuilder.Add(item: false);
					}
					if (flag)
					{
						HandleCustomModifiers(twa.CustomModifiers.Length, arrayBuilder);
					}
					twa.Type.VisitType(visitor, (arrayBuilder, flag));
				}
			}
		}

		private static void HandleCustomModifiers(int customModifiersCount, ArrayBuilder<bool> transformFlagsBuilder)
		{
			transformFlagsBuilder.AddMany(item: false, customModifiersCount);
		}
	}

	internal static class NativeIntegerTransformsEncoder
	{
		internal static void Encode(ArrayBuilder<bool> builder, Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type)
		{
			type.VisitType((Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol typeSymbol, ArrayBuilder<bool> builder2, bool isNested) => AddFlags(typeSymbol, builder2), builder);
		}

		private static bool AddFlags(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type, ArrayBuilder<bool> builder)
		{
			SpecialType specialType = type.SpecialType;
			if ((uint)(specialType - 21) <= 1u)
			{
				builder.Add(type.IsNativeIntegerWrapperType);
			}
			return false;
		}
	}

	internal class SpecialMembersSignatureComparer : SignatureComparer<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol, Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol, Microsoft.CodeAnalysis.CSharp.Symbols.PropertySymbol, Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol, Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol>
	{
		public static readonly SpecialMembersSignatureComparer Instance = new SpecialMembersSignatureComparer();

		protected SpecialMembersSignatureComparer()
		{
		}

		protected override Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol? GetMDArrayElementType(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type)
		{
			if (type.Kind != SymbolKind.ArrayType)
			{
				return null;
			}
			Microsoft.CodeAnalysis.CSharp.Symbols.ArrayTypeSymbol arrayTypeSymbol = (Microsoft.CodeAnalysis.CSharp.Symbols.ArrayTypeSymbol)type;
			if (arrayTypeSymbol.IsSZArray)
			{
				return null;
			}
			return arrayTypeSymbol.ElementType;
		}

		protected override Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol GetFieldType(Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol field)
		{
			return field.Type;
		}

		protected override Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol GetPropertyType(Microsoft.CodeAnalysis.CSharp.Symbols.PropertySymbol property)
		{
			return property.Type;
		}

		protected override Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol? GetGenericTypeArgument(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type, int argumentIndex)
		{
			if (type.Kind != SymbolKind.NamedType)
			{
				return null;
			}
			Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol namedTypeSymbol = (Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol)type;
			if (namedTypeSymbol.Arity <= argumentIndex)
			{
				return null;
			}
			if ((object)namedTypeSymbol.ContainingType != null)
			{
				return null;
			}
			return namedTypeSymbol.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[argumentIndex].Type;
		}

		protected override Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol? GetGenericTypeDefinition(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type)
		{
			if (type.Kind != SymbolKind.NamedType)
			{
				return null;
			}
			Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol namedTypeSymbol = (Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol)type;
			if ((object)namedTypeSymbol.ContainingType != null)
			{
				return null;
			}
			if (namedTypeSymbol.Arity == 0)
			{
				return null;
			}
			return namedTypeSymbol.OriginalDefinition;
		}

		protected override ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol> GetParameters(Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol method)
		{
			return method.Parameters;
		}

		protected override ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol> GetParameters(Microsoft.CodeAnalysis.CSharp.Symbols.PropertySymbol property)
		{
			return property.Parameters;
		}

		protected override Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol GetParamType(Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol parameter)
		{
			return parameter.Type;
		}

		protected override Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol? GetPointedToType(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type)
		{
			if (type.Kind != SymbolKind.PointerType)
			{
				return null;
			}
			return ((Microsoft.CodeAnalysis.CSharp.Symbols.PointerTypeSymbol)type).PointedAtType;
		}

		protected override Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol GetReturnType(Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol method)
		{
			return method.ReturnType;
		}

		protected override Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol? GetSZArrayElementType(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type)
		{
			if (type.Kind != SymbolKind.ArrayType)
			{
				return null;
			}
			Microsoft.CodeAnalysis.CSharp.Symbols.ArrayTypeSymbol arrayTypeSymbol = (Microsoft.CodeAnalysis.CSharp.Symbols.ArrayTypeSymbol)type;
			if (!arrayTypeSymbol.IsSZArray)
			{
				return null;
			}
			return arrayTypeSymbol.ElementType;
		}

		protected override bool IsByRefParam(Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol parameter)
		{
			return parameter.RefKind != RefKind.None;
		}

		protected override bool IsByRefMethod(Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol method)
		{
			return method.RefKind != RefKind.None;
		}

		protected override bool IsByRefProperty(Microsoft.CodeAnalysis.CSharp.Symbols.PropertySymbol property)
		{
			return property.RefKind != RefKind.None;
		}

		protected override bool IsGenericMethodTypeParam(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type, int paramPosition)
		{
			if (type.Kind != SymbolKind.TypeParameter)
			{
				return false;
			}
			Microsoft.CodeAnalysis.CSharp.Symbols.TypeParameterSymbol typeParameterSymbol = (Microsoft.CodeAnalysis.CSharp.Symbols.TypeParameterSymbol)type;
			if (typeParameterSymbol.ContainingSymbol.Kind != SymbolKind.Method)
			{
				return false;
			}
			return typeParameterSymbol.Ordinal == paramPosition;
		}

		protected override bool IsGenericTypeParam(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type, int paramPosition)
		{
			if (type.Kind != SymbolKind.TypeParameter)
			{
				return false;
			}
			Microsoft.CodeAnalysis.CSharp.Symbols.TypeParameterSymbol typeParameterSymbol = (Microsoft.CodeAnalysis.CSharp.Symbols.TypeParameterSymbol)type;
			if (typeParameterSymbol.ContainingSymbol.Kind != SymbolKind.NamedType)
			{
				return false;
			}
			return typeParameterSymbol.Ordinal == paramPosition;
		}

		protected override bool MatchArrayRank(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type, int countOfDimensions)
		{
			if (type.Kind != SymbolKind.ArrayType)
			{
				return false;
			}
			return ((Microsoft.CodeAnalysis.CSharp.Symbols.ArrayTypeSymbol)type).Rank == countOfDimensions;
		}

		protected override bool MatchTypeToTypeId(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type, int typeId)
		{
			if ((int)type.OriginalDefinition.ExtendedSpecialType == typeId)
			{
				if (type.IsDefinition)
				{
					return true;
				}
				return type.Equals(type.OriginalDefinition, TypeCompareKind.IgnoreNullableModifiersForReferenceTypes);
			}
			return false;
		}
	}

	internal sealed class WellKnownMembersSignatureComparer : SpecialMembersSignatureComparer
	{
		private readonly CSharpCompilation _compilation;

		public WellKnownMembersSignatureComparer(CSharpCompilation compilation)
		{
			_compilation = compilation;
		}

		protected override bool MatchTypeToTypeId(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type, int typeId)
		{
			if (((WellKnownType)typeId).IsWellKnownType())
			{
				return type.Equals(_compilation.GetWellKnownType((WellKnownType)typeId), TypeCompareKind.IgnoreNullableModifiersForReferenceTypes);
			}
			return base.MatchTypeToTypeId(type, typeId);
		}
	}

	internal sealed class ReferenceManager : CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>
	{
		private abstract class AssemblyDataForMetadataOrCompilation : AssemblyData
		{
			private ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> _assemblies;

			private readonly AssemblyIdentity _identity;

			private readonly ImmutableArray<AssemblyIdentity> _referencedAssemblies;

			private readonly bool _embedInteropTypes;

			public override AssemblyIdentity Identity => _identity;

			public override ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> AvailableSymbols
			{
				get
				{
					if (_assemblies.IsDefault)
					{
						ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> instance = ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.GetInstance();
						AddAvailableSymbols(instance);
						_assemblies = instance.ToImmutableAndFree();
					}
					return _assemblies;
				}
			}

			public override ImmutableArray<AssemblyIdentity> AssemblyReferences => _referencedAssemblies;

			public sealed override bool IsLinked => _embedInteropTypes;

			protected AssemblyDataForMetadataOrCompilation(AssemblyIdentity identity, ImmutableArray<AssemblyIdentity> referencedAssemblies, bool embedInteropTypes)
			{
				_embedInteropTypes = embedInteropTypes;
				_identity = identity;
				_referencedAssemblies = referencedAssemblies;
			}

			internal abstract Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol CreateAssemblySymbol();

			protected abstract void AddAvailableSymbols(ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> builder);

			public override AssemblyReferenceBinding[] BindAssemblyReferences(MultiDictionary<string, (AssemblyData DefinitionData, int DefinitionIndex)> assemblies, AssemblyIdentityComparer assemblyIdentityComparer)
			{
				return CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.ResolveReferencedAssemblies(_referencedAssemblies, assemblies, resolveAgainstAssemblyBeingBuilt: true, assemblyIdentityComparer);
			}
		}

		private sealed class AssemblyDataForFile : AssemblyDataForMetadataOrCompilation
		{
			public readonly PEAssembly Assembly;

			public readonly WeakList<IAssemblySymbolInternal> CachedSymbols;

			public readonly DocumentationProvider DocumentationProvider;

			private readonly MetadataImportOptions _compilationImportOptions;

			private readonly string _sourceAssemblySimpleName;

			private bool _internalsVisibleComputed;

			private bool _internalsPotentiallyVisibleToCompilation;

			internal bool InternalsMayBeVisibleToCompilation
			{
				get
				{
					if (!_internalsVisibleComputed)
					{
						_internalsPotentiallyVisibleToCompilation = CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.InternalsMayBeVisibleToAssemblyBeingCompiled(_sourceAssemblySimpleName, Assembly);
						_internalsVisibleComputed = true;
					}
					return _internalsPotentiallyVisibleToCompilation;
				}
			}

			internal MetadataImportOptions EffectiveImportOptions
			{
				get
				{
					if (InternalsMayBeVisibleToCompilation && _compilationImportOptions == MetadataImportOptions.Public)
					{
						return MetadataImportOptions.Internal;
					}
					return _compilationImportOptions;
				}
			}

			public override bool ContainsNoPiaLocalTypes => Assembly.ContainsNoPiaLocalTypes();

			public override bool DeclaresTheObjectClass => Assembly.DeclaresTheObjectClass;

			public override Compilation? SourceCompilation => null;

			public AssemblyDataForFile(PEAssembly assembly, WeakList<IAssemblySymbolInternal> cachedSymbols, bool embedInteropTypes, DocumentationProvider documentationProvider, string sourceAssemblySimpleName, MetadataImportOptions compilationImportOptions)
				: base(assembly.Identity, assembly.AssemblyReferences, embedInteropTypes)
			{
				CachedSymbols = cachedSymbols;
				Assembly = assembly;
				DocumentationProvider = documentationProvider;
				_compilationImportOptions = compilationImportOptions;
				_sourceAssemblySimpleName = sourceAssemblySimpleName;
			}

			internal override Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol CreateAssemblySymbol()
			{
				return new PEAssemblySymbol(Assembly, DocumentationProvider, IsLinked, EffectiveImportOptions);
			}

			protected override void AddAvailableSymbols(ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> assemblies)
			{
				lock (CommonReferenceManager.SymbolCacheAndReferenceManagerStateGuard)
				{
					foreach (IAssemblySymbolInternal cachedSymbol in CachedSymbols)
					{
						PEAssemblySymbol pEAssemblySymbol = cachedSymbol as PEAssemblySymbol;
						if (IsMatchingAssembly(pEAssemblySymbol))
						{
							assemblies.Add(pEAssemblySymbol);
						}
					}
				}
			}

			public override bool IsMatchingAssembly(Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol? candidateAssembly)
			{
				return IsMatchingAssembly(candidateAssembly as PEAssemblySymbol);
			}

			private bool IsMatchingAssembly(PEAssemblySymbol? peAssembly)
			{
				if ((object)peAssembly == null)
				{
					return false;
				}
				if (peAssembly.Assembly != Assembly)
				{
					return false;
				}
				if (EffectiveImportOptions != peAssembly.PrimaryModule.ImportOptions)
				{
					return false;
				}
				if (!peAssembly.DocumentationProvider.Equals(DocumentationProvider))
				{
					return false;
				}
				return true;
			}
		}

		private sealed class AssemblyDataForCompilation : AssemblyDataForMetadataOrCompilation
		{
			public readonly CSharpCompilation Compilation;

			public override bool ContainsNoPiaLocalTypes => Compilation.MightContainNoPiaLocalTypes();

			public override bool DeclaresTheObjectClass => Compilation.DeclaresTheObjectClass;

			public override Compilation SourceCompilation => Compilation;

			public AssemblyDataForCompilation(CSharpCompilation compilation, bool embedInteropTypes)
				: base(compilation.Assembly.Identity, GetReferencedAssemblies(compilation), embedInteropTypes)
			{
				Compilation = compilation;
			}

			private static ImmutableArray<AssemblyIdentity> GetReferencedAssemblies(CSharpCompilation compilation)
			{
				ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.ModuleSymbol> modules = compilation.Assembly.Modules;
				ImmutableArray<AssemblyIdentity> referencedAssemblies = modules[0].GetReferencedAssemblies();
				ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> referencedAssemblySymbols = modules[0].GetReferencedAssemblySymbols();
				ArrayBuilder<AssemblyIdentity> instance = ArrayBuilder<AssemblyIdentity>.GetInstance(modules.Sum((Microsoft.CodeAnalysis.CSharp.Symbols.ModuleSymbol module, int index) => (index == 0) ? module.GetReferencedAssemblySymbols().Count((Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol identity) => !identity.IsLinked) : module.GetReferencedAssemblies().Length));
				for (int num = 0; num < referencedAssemblies.Length; num++)
				{
					if (!referencedAssemblySymbols[num].IsLinked)
					{
						instance.Add(referencedAssemblies[num]);
					}
				}
				for (int num2 = 1; num2 < modules.Length; num2++)
				{
					instance.AddRange(modules[num2].GetReferencedAssemblies());
				}
				return instance.ToImmutableAndFree();
			}

			internal override Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol CreateAssemblySymbol()
			{
				return new RetargetingAssemblySymbol(Compilation.SourceAssembly, IsLinked);
			}

			protected override void AddAvailableSymbols(ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> assemblies)
			{
				assemblies.Add(Compilation.Assembly);
				lock (CommonReferenceManager.SymbolCacheAndReferenceManagerStateGuard)
				{
					Compilation.AddRetargetingAssemblySymbolsNoLock(assemblies);
				}
			}

			public override bool IsMatchingAssembly(Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol? candidateAssembly)
			{
				Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol assemblySymbol = ((!(candidateAssembly is RetargetingAssemblySymbol retargetingAssemblySymbol)) ? (candidateAssembly as Microsoft.CodeAnalysis.CSharp.Symbols.SourceAssemblySymbol) : retargetingAssemblySymbol.UnderlyingAssembly);
				return (object)assemblySymbol == Compilation.Assembly;
			}
		}

		protected override CommonMessageProvider MessageProvider => Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance;

		public ReferenceManager(string simpleAssemblyName, AssemblyIdentityComparer identityComparer, Dictionary<MetadataReference, object>? observedMetadata)
			: base(simpleAssemblyName, identityComparer, observedMetadata)
		{
		}

		protected override AssemblyData CreateAssemblyDataForFile(PEAssembly assembly, WeakList<IAssemblySymbolInternal> cachedSymbols, DocumentationProvider documentationProvider, string sourceAssemblySimpleName, MetadataImportOptions importOptions, bool embedInteropTypes)
		{
			return new AssemblyDataForFile(assembly, cachedSymbols, embedInteropTypes, documentationProvider, sourceAssemblySimpleName, importOptions);
		}

		protected override AssemblyData CreateAssemblyDataForCompilation(CompilationReference compilationReference)
		{
			if (!(compilationReference is CSharpCompilationReference cSharpCompilationReference))
			{
				throw new NotSupportedException(string.Format(CSharpResources.CantReferenceCompilationOf, compilationReference.GetType(), "C#"));
			}
			return new AssemblyDataForCompilation(cSharpCompilationReference.Compilation, cSharpCompilationReference.Properties.EmbedInteropTypes);
		}

		protected override bool CheckPropertiesConsistency(MetadataReference primaryReference, MetadataReference duplicateReference, DiagnosticBag diagnostics)
		{
			if (primaryReference.Properties.EmbedInteropTypes != duplicateReference.Properties.EmbedInteropTypes)
			{
				diagnostics.Add(ErrorCode.ERR_AssemblySpecifiedForLinkAndRef, NoLocation.Singleton, duplicateReference.Display, primaryReference.Display);
				return false;
			}
			return true;
		}

		protected override bool WeakIdentityPropertiesEquivalent(AssemblyIdentity identity1, AssemblyIdentity identity2)
		{
			return AssemblyIdentityComparer.CultureComparer.Equals(identity1.CultureName, identity2.CultureName);
		}

		protected override void GetActualBoundReferencesUsedBy(Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol assemblySymbol, List<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol?> referencedAssemblySymbols)
		{
			foreach (Microsoft.CodeAnalysis.CSharp.Symbols.ModuleSymbol module in assemblySymbol.Modules)
			{
				referencedAssemblySymbols.AddRange(module.GetReferencedAssemblySymbols());
			}
			for (int i = 0; i < referencedAssemblySymbols.Count; i++)
			{
				if (referencedAssemblySymbols[i].IsMissing)
				{
					referencedAssemblySymbols[i] = null;
				}
			}
		}

		protected override ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> GetNoPiaResolutionAssemblies(Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol candidateAssembly)
		{
			if (candidateAssembly is Microsoft.CodeAnalysis.CSharp.Symbols.SourceAssemblySymbol)
			{
				return ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.Empty;
			}
			return candidateAssembly.GetNoPiaResolutionAssemblies();
		}

		protected override bool IsLinked(Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol candidateAssembly)
		{
			return candidateAssembly.IsLinked;
		}

		protected override Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol? GetCorLibrary(Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol candidateAssembly)
		{
			Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol corLibrary = candidateAssembly.CorLibrary;
			if (!corLibrary.IsMissing)
			{
				return corLibrary;
			}
			return null;
		}

		public void CreateSourceAssemblyForCompilation(CSharpCompilation compilation)
		{
			if (base.IsBound || !CreateAndSetSourceAssemblyFullBind(compilation))
			{
				if (!base.HasCircularReference)
				{
					CreateAndSetSourceAssemblyReuseData(compilation);
				}
				else
				{
					new ReferenceManager(SimpleAssemblyName, IdentityComparer, ObservedMetadata).CreateAndSetSourceAssemblyFullBind(compilation);
				}
			}
		}

		public PEAssemblySymbol CreatePEAssemblyForAssemblyMetadata(AssemblyMetadata metadata, MetadataImportOptions importOptions, out ImmutableDictionary<AssemblyIdentity, AssemblyIdentity> assemblyReferenceIdentityMap)
		{
			AssemblyIdentityMap<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> assemblyIdentityMap = new AssemblyIdentityMap<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>();
			foreach (Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol referencedAssembly in base.ReferencedAssemblies)
			{
				assemblyIdentityMap.Add(referencedAssembly.Identity, referencedAssembly);
			}
			PEAssembly assembly = metadata.GetAssembly();
			ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> immutableArray = assembly.AssemblyReferences.SelectAsArray(MapAssemblyIdentityToResolvedSymbol, assemblyIdentityMap);
			assemblyReferenceIdentityMap = CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.GetAssemblyReferenceIdentityBaselineMap(immutableArray, assembly.AssemblyReferences);
			PEAssemblySymbol pEAssemblySymbol = new PEAssemblySymbol(assembly, DocumentationProvider.Default, isLinked: false, importOptions);
			ImmutableArray<UnifiedAssembly<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>> unifiedAssemblies = base.UnifiedAssemblies.WhereAsArray<UnifiedAssembly<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>, AssemblyIdentityMap<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>>((UnifiedAssembly<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> unified, AssemblyIdentityMap<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> referencedAssembliesByIdentity) => referencedAssembliesByIdentity.Contains(unified.OriginalReference, allowHigherVersion: false), assemblyIdentityMap);
			InitializeAssemblyReuseData(pEAssemblySymbol, immutableArray, unifiedAssemblies);
			if (assembly.ContainsNoPiaLocalTypes())
			{
				pEAssemblySymbol.SetNoPiaResolutionAssemblies(base.ReferencedAssemblies);
			}
			return pEAssemblySymbol;
		}

		private static Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol MapAssemblyIdentityToResolvedSymbol(AssemblyIdentity identity, AssemblyIdentityMap<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> map)
		{
			if (map.TryGetValue(identity, out Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol value, CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.CompareVersionPartsSpecifiedInSource))
			{
				return value;
			}
			if (map.TryGetValue(identity, out value, (Version v1, Version v2, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol s) => true))
			{
				throw new NotSupportedException(string.Format(CodeAnalysisResources.ChangingVersionOfAssemblyReferenceIsNotAllowedDuringDebugging, identity, value.Identity.Version));
			}
			return new MissingAssemblySymbol(identity);
		}

		private void CreateAndSetSourceAssemblyReuseData(CSharpCompilation compilation)
		{
			string moduleName = compilation.MakeSourceModuleName();
			Microsoft.CodeAnalysis.CSharp.Symbols.SourceAssemblySymbol sourceAssemblySymbol = new Microsoft.CodeAnalysis.CSharp.Symbols.SourceAssemblySymbol(compilation, SimpleAssemblyName, moduleName, base.ReferencedModules);
			InitializeAssemblyReuseData(sourceAssemblySymbol, base.ReferencedAssemblies, base.UnifiedAssemblies);
			if ((object)compilation._lazyAssemblySymbol != null)
			{
				return;
			}
			lock (CommonReferenceManager.SymbolCacheAndReferenceManagerStateGuard)
			{
				if ((object)compilation._lazyAssemblySymbol == null)
				{
					compilation._lazyAssemblySymbol = sourceAssemblySymbol;
				}
			}
		}

		private void InitializeAssemblyReuseData(Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol assemblySymbol, ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> referencedAssemblies, ImmutableArray<UnifiedAssembly<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>> unifiedAssemblies)
		{
			assemblySymbol.SetCorLibrary(base.CorLibraryOpt ?? assemblySymbol);
			ModuleReferences<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> moduleReferences = new ModuleReferences<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>(referencedAssemblies.SelectAsArray((Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol a) => a.Identity), referencedAssemblies, unifiedAssemblies);
			assemblySymbol.Modules[0].SetReferences(moduleReferences);
			ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.ModuleSymbol> modules = assemblySymbol.Modules;
			ImmutableArray<ModuleReferences<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>> referencedModulesReferences = base.ReferencedModulesReferences;
			for (int num = 1; num < modules.Length; num++)
			{
				modules[num].SetReferences(referencedModulesReferences[num - 1]);
			}
		}

		private bool CreateAndSetSourceAssemblyFullBind(CSharpCompilation compilation)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			PooledDictionary<string, List<CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.ReferencedAssemblyIdentity>> instance2 = PooledDictionary<string, List<CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.ReferencedAssemblyIdentity>>.GetInstance();
			bool referencesSupersedeLowerVersions = compilation.Options.ReferencesSupersedeLowerVersions;
			try
			{
				ImmutableArray<CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.ResolvedReference> explicitReferenceMap = ResolveMetadataReferences(compilation, instance2, out ImmutableArray<MetadataReference> references, out IDictionary<(string, string), MetadataReference> boundReferenceDirectiveMap, out ImmutableArray<MetadataReference> boundReferenceDirectives, out ImmutableArray<CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.AssemblyData> assemblies, out ImmutableArray<PEModule> modules, instance);
				CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.AssemblyDataForAssemblyBeingBuilt item = new CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.AssemblyDataForAssemblyBeingBuilt(new AssemblyIdentity(noThrow: true, SimpleAssemblyName), assemblies, modules);
				ImmutableArray<CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.AssemblyData> explicitAssemblies = assemblies.Insert(0, item);
				ImmutableDictionary<AssemblyIdentity, PortableExecutableReference> implicitReferenceResolutions = compilation.ScriptCompilationInfo?.PreviousScriptCompilation?.GetBoundReferenceManager().ImplicitReferenceResolutions ?? ImmutableDictionary<AssemblyIdentity, PortableExecutableReference>.Empty;
				CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.BoundInputAssembly[] array = Bind(explicitAssemblies, modules, references, explicitReferenceMap, compilation.Options.MetadataReferenceResolver, compilation.Options.MetadataImportOptions, referencesSupersedeLowerVersions, instance2, out ImmutableArray<CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.AssemblyData> allAssemblies, out ImmutableArray<MetadataReference> implicitlyResolvedReferences, out ImmutableArray<CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.ResolvedReference> implicitlyResolvedReferenceMap, ref implicitReferenceResolutions, instance, out bool hasCircularReference, out int corLibraryIndex);
				ImmutableArray<MetadataReference> references2 = references.AddRange(implicitlyResolvedReferences);
				explicitReferenceMap = explicitReferenceMap.AddRange(implicitlyResolvedReferenceMap);
				CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.BuildReferencedAssembliesAndModulesMaps(array, references2, explicitReferenceMap, modules.Length, assemblies.Length, instance2, referencesSupersedeLowerVersions, out Dictionary<MetadataReference, int> referencedAssembliesMap, out Dictionary<MetadataReference, int> referencedModulesMap, out ImmutableArray<ImmutableArray<string>> aliasesOfReferencedAssemblies, out Dictionary<MetadataReference, ImmutableArray<MetadataReference>> mergedAssemblyReferencesMapOpt);
				List<int> list = new List<int>();
				for (int i = 1; i < array.Length; i++)
				{
					ref CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.BoundInputAssembly reference = ref array[i];
					if ((object)reference.AssemblySymbol == null)
					{
						reference.AssemblySymbol = ((AssemblyDataForMetadataOrCompilation)allAssemblies[i]).CreateAssemblySymbol();
						list.Add(i);
					}
				}
				Microsoft.CodeAnalysis.CSharp.Symbols.SourceAssemblySymbol sourceAssemblySymbol = new Microsoft.CodeAnalysis.CSharp.Symbols.SourceAssemblySymbol(compilation, SimpleAssemblyName, compilation.MakeSourceModuleName(), modules);
				Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol assemblySymbol = ((corLibraryIndex == 0) ? sourceAssemblySymbol : ((corLibraryIndex <= 0) ? MissingCorLibrarySymbol.Instance : array[corLibraryIndex].AssemblySymbol));
				sourceAssemblySymbol.SetCorLibrary(assemblySymbol);
				Dictionary<AssemblyIdentity, MissingAssemblySymbol> missingAssemblies = null;
				int totalReferencedAssemblyCount = allAssemblies.Length - 1;
				SetupReferencesForSourceAssembly(sourceAssemblySymbol, modules, totalReferencedAssemblyCount, array, ref missingAssemblies, out ImmutableArray<ModuleReferences<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>> moduleReferences);
				if (list.Count > 0)
				{
					if (hasCircularReference)
					{
						array[0].AssemblySymbol = sourceAssemblySymbol;
					}
					InitializeNewSymbols(list, sourceAssemblySymbol, allAssemblies, array, missingAssemblies);
				}
				if ((object)compilation._lazyAssemblySymbol == null)
				{
					lock (CommonReferenceManager.SymbolCacheAndReferenceManagerStateGuard)
					{
						if ((object)compilation._lazyAssemblySymbol == null)
						{
							if (base.IsBound)
							{
								return false;
							}
							UpdateSymbolCacheNoLock(list, allAssemblies, array);
							InitializeNoLock(referencedAssembliesMap, referencedModulesMap, boundReferenceDirectiveMap, boundReferenceDirectives, references, implicitReferenceResolutions, hasCircularReference, instance.ToReadOnly(), ((object)assemblySymbol == sourceAssemblySymbol) ? null : assemblySymbol, modules, moduleReferences, sourceAssemblySymbol.SourceModule.GetReferencedAssemblySymbols(), aliasesOfReferencedAssemblies, sourceAssemblySymbol.SourceModule.GetUnifiedAssemblies(), mergedAssemblyReferencesMapOpt);
							compilation._referenceManager = this;
							compilation._lazyAssemblySymbol = sourceAssemblySymbol;
						}
					}
				}
				return true;
			}
			finally
			{
				instance.Free();
				instance2.Free();
			}
		}

		private static void InitializeNewSymbols(List<int> newSymbols, Microsoft.CodeAnalysis.CSharp.Symbols.SourceAssemblySymbol sourceAssembly, ImmutableArray<AssemblyData> assemblies, BoundInputAssembly[] bindingResult, Dictionary<AssemblyIdentity, MissingAssemblySymbol>? missingAssemblies)
		{
			Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol corLibrary = sourceAssembly.CorLibrary;
			foreach (int newSymbol in newSymbols)
			{
				if (assemblies[newSymbol] is AssemblyDataForCompilation)
				{
					SetupReferencesForRetargetingAssembly(bindingResult, ref bindingResult[newSymbol], ref missingAssemblies, sourceAssembly);
				}
				else
				{
					SetupReferencesForFileAssembly((AssemblyDataForFile)assemblies[newSymbol], bindingResult, ref bindingResult[newSymbol], ref missingAssemblies, sourceAssembly);
				}
			}
			ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> instance = ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.GetInstance();
			ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> referencedAssemblySymbols = sourceAssembly.Modules[0].GetReferencedAssemblySymbols();
			foreach (int newSymbol2 in newSymbols)
			{
				ref CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.BoundInputAssembly reference = ref bindingResult[newSymbol2];
				if (assemblies[newSymbol2].ContainsNoPiaLocalTypes)
				{
					reference.AssemblySymbol.SetNoPiaResolutionAssemblies(referencedAssemblySymbols);
				}
				instance.Clear();
				if (assemblies[newSymbol2].IsLinked)
				{
					instance.Add(reference.AssemblySymbol);
				}
				CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.AssemblyReferenceBinding[] referenceBinding = reference.ReferenceBinding;
				for (int i = 0; i < referenceBinding.Length; i++)
				{
					CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.AssemblyReferenceBinding assemblyReferenceBinding = referenceBinding[i];
					if (assemblyReferenceBinding.IsBound && assemblies[assemblyReferenceBinding.DefinitionIndex].IsLinked)
					{
						Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol assemblySymbol = bindingResult[assemblyReferenceBinding.DefinitionIndex].AssemblySymbol;
						instance.Add(assemblySymbol);
					}
				}
				if (instance.Count > 0)
				{
					instance.RemoveDuplicates();
					reference.AssemblySymbol.SetLinkedReferencedAssemblies(instance.ToImmutable());
				}
				reference.AssemblySymbol.SetCorLibrary(corLibrary);
			}
			instance.Free();
			if (missingAssemblies == null)
			{
				return;
			}
			foreach (MissingAssemblySymbol value in missingAssemblies.Values)
			{
				value.SetCorLibrary(corLibrary);
			}
		}

		private static void UpdateSymbolCacheNoLock(List<int> newSymbols, ImmutableArray<AssemblyData> assemblies, BoundInputAssembly[] bindingResult)
		{
			foreach (int newSymbol in newSymbols)
			{
				ref CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.BoundInputAssembly reference = ref bindingResult[newSymbol];
				if (assemblies[newSymbol] is AssemblyDataForCompilation assemblyDataForCompilation)
				{
					assemblyDataForCompilation.Compilation.CacheRetargetingAssemblySymbolNoLock(reference.AssemblySymbol);
				}
				else
				{
					((AssemblyDataForFile)assemblies[newSymbol]).CachedSymbols.Add((PEAssemblySymbol)reference.AssemblySymbol);
				}
			}
		}

		private static void SetupReferencesForRetargetingAssembly(BoundInputAssembly[] bindingResult, ref BoundInputAssembly currentBindingResult, ref Dictionary<AssemblyIdentity, MissingAssemblySymbol>? missingAssemblies, Microsoft.CodeAnalysis.CSharp.Symbols.SourceAssemblySymbol sourceAssemblyDebugOnly)
		{
			RetargetingAssemblySymbol retargetingAssemblySymbol = (RetargetingAssemblySymbol)currentBindingResult.AssemblySymbol;
			ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.ModuleSymbol> modules = retargetingAssemblySymbol.Modules;
			int length = modules.Length;
			int num = 0;
			for (int i = 0; i < length; i++)
			{
				ImmutableArray<AssemblyIdentity> identities = retargetingAssemblySymbol.UnderlyingAssembly.Modules[i].GetReferencedAssemblies();
				if (i == 0)
				{
					ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> referencedAssemblySymbols = retargetingAssemblySymbol.UnderlyingAssembly.Modules[0].GetReferencedAssemblySymbols();
					int num2 = 0;
					foreach (Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol item in referencedAssemblySymbols)
					{
						if (item.IsLinked)
						{
							num2++;
						}
					}
					if (num2 > 0)
					{
						AssemblyIdentity[] array = new AssemblyIdentity[identities.Length - num2];
						int num3 = 0;
						for (int j = 0; j < referencedAssemblySymbols.Length; j++)
						{
							if (!referencedAssemblySymbols[j].IsLinked)
							{
								array[num3] = identities[j];
								num3++;
							}
						}
						identities = array.AsImmutableOrNull();
					}
				}
				int length2 = identities.Length;
				Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol[] array2 = new Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol[length2];
				ArrayBuilder<UnifiedAssembly<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>> unifiedAssemblies = null;
				for (int k = 0; k < length2; k++)
				{
					CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.AssemblyReferenceBinding referenceBinding = currentBindingResult.ReferenceBinding[num + k];
					if (referenceBinding.IsBound)
					{
						array2[k] = GetAssemblyDefinitionSymbol(bindingResult, referenceBinding, ref unifiedAssemblies);
					}
					else
					{
						array2[k] = GetOrAddMissingAssemblySymbol(identities[k], ref missingAssemblies);
					}
				}
				ModuleReferences<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> moduleReferences = new ModuleReferences<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>(identities, array2.AsImmutableOrNull(), unifiedAssemblies.AsImmutableOrEmpty());
				modules[i].SetReferences(moduleReferences, sourceAssemblyDebugOnly);
				num += length2;
			}
		}

		private static void SetupReferencesForFileAssembly(AssemblyDataForFile fileData, BoundInputAssembly[] bindingResult, ref BoundInputAssembly currentBindingResult, ref Dictionary<AssemblyIdentity, MissingAssemblySymbol>? missingAssemblies, Microsoft.CodeAnalysis.CSharp.Symbols.SourceAssemblySymbol sourceAssemblyDebugOnly)
		{
			ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.ModuleSymbol> modules = ((PEAssemblySymbol)currentBindingResult.AssemblySymbol).Modules;
			int length = modules.Length;
			int num = 0;
			for (int i = 0; i < length; i++)
			{
				int num2 = fileData.Assembly.ModuleReferenceCounts[i];
				AssemblyIdentity[] array = new AssemblyIdentity[num2];
				Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol[] array2 = new Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol[num2];
				fileData.AssemblyReferences.CopyTo(num, array, 0, num2);
				ArrayBuilder<UnifiedAssembly<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>> unifiedAssemblies = null;
				for (int j = 0; j < num2; j++)
				{
					CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.AssemblyReferenceBinding referenceBinding = currentBindingResult.ReferenceBinding[num + j];
					if (referenceBinding.IsBound)
					{
						array2[j] = GetAssemblyDefinitionSymbol(bindingResult, referenceBinding, ref unifiedAssemblies);
					}
					else
					{
						array2[j] = GetOrAddMissingAssemblySymbol(array[j], ref missingAssemblies);
					}
				}
				ModuleReferences<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> moduleReferences = new ModuleReferences<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>(array.AsImmutableOrNull(), array2.AsImmutableOrNull(), unifiedAssemblies.AsImmutableOrEmpty());
				modules[i].SetReferences(moduleReferences, sourceAssemblyDebugOnly);
				num += num2;
			}
		}

		private static void SetupReferencesForSourceAssembly(Microsoft.CodeAnalysis.CSharp.Symbols.SourceAssemblySymbol sourceAssembly, ImmutableArray<PEModule> modules, int totalReferencedAssemblyCount, BoundInputAssembly[] bindingResult, ref Dictionary<AssemblyIdentity, MissingAssemblySymbol>? missingAssemblies, out ImmutableArray<ModuleReferences<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>> moduleReferences)
		{
			ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.ModuleSymbol> modules2 = sourceAssembly.Modules;
			ArrayBuilder<ModuleReferences<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>> arrayBuilder = ((modules2.Length > 1) ? ArrayBuilder<ModuleReferences<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>>.GetInstance() : null);
			int num = 0;
			for (int i = 0; i < modules2.Length; i++)
			{
				int num2 = ((i == 0) ? totalReferencedAssemblyCount : modules[i - 1].ReferencedAssemblies.Length);
				AssemblyIdentity[] array = new AssemblyIdentity[num2];
				Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol[] array2 = new Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol[num2];
				ArrayBuilder<UnifiedAssembly<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>> unifiedAssemblies = null;
				for (int j = 0; j < num2; j++)
				{
					CommonReferenceManager<CSharpCompilation, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.AssemblyReferenceBinding referenceBinding = bindingResult[0].ReferenceBinding[num + j];
					if (referenceBinding.IsBound)
					{
						array2[j] = GetAssemblyDefinitionSymbol(bindingResult, referenceBinding, ref unifiedAssemblies);
					}
					else
					{
						array2[j] = GetOrAddMissingAssemblySymbol(referenceBinding.ReferenceIdentity, ref missingAssemblies);
					}
					array[j] = referenceBinding.ReferenceIdentity;
				}
				ModuleReferences<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> moduleReferences2 = new ModuleReferences<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>(array.AsImmutableOrNull(), array2.AsImmutableOrNull(), unifiedAssemblies.AsImmutableOrEmpty());
				if (i > 0)
				{
					arrayBuilder.Add(moduleReferences2);
				}
				modules2[i].SetReferences(moduleReferences2, sourceAssembly);
				num += num2;
			}
			moduleReferences = arrayBuilder.ToImmutableOrEmptyAndFree();
		}

		private static Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol GetAssemblyDefinitionSymbol(BoundInputAssembly[] bindingResult, AssemblyReferenceBinding referenceBinding, ref ArrayBuilder<UnifiedAssembly<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>>? unifiedAssemblies)
		{
			Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol assemblySymbol = bindingResult[referenceBinding.DefinitionIndex].AssemblySymbol;
			if (referenceBinding.VersionDifference != 0)
			{
				if (unifiedAssemblies == null)
				{
					unifiedAssemblies = new ArrayBuilder<UnifiedAssembly<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>>();
				}
				unifiedAssemblies.Add(new UnifiedAssembly<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>(assemblySymbol, referenceBinding.ReferenceIdentity));
			}
			return assemblySymbol;
		}

		private static MissingAssemblySymbol GetOrAddMissingAssemblySymbol(AssemblyIdentity assemblyIdentity, ref Dictionary<AssemblyIdentity, MissingAssemblySymbol>? missingAssemblies)
		{
			MissingAssemblySymbol value;
			if (missingAssemblies == null)
			{
				missingAssemblies = new Dictionary<AssemblyIdentity, MissingAssemblySymbol>();
			}
			else if (missingAssemblies.TryGetValue(assemblyIdentity, out value))
			{
				return value;
			}
			value = new MissingAssemblySymbol(assemblyIdentity);
			missingAssemblies.Add(assemblyIdentity, value);
			return value;
		}

		internal static bool IsSourceAssemblySymbolCreated(CSharpCompilation compilation)
		{
			return (object)compilation._lazyAssemblySymbol != null;
		}

		internal static bool IsReferenceManagerInitialized(CSharpCompilation compilation)
		{
			return compilation._referenceManager.IsBound;
		}
	}

	private readonly CSharpCompilationOptions _options;

	private UsingsFromOptionsAndDiagnostics? _lazyUsingsFromOptions;

	private ImmutableArray<NamespaceOrTypeAndUsingDirective> _lazyGlobalImports;

	private Imports? _lazyPreviousSubmissionImports;

	private Microsoft.CodeAnalysis.CSharp.Symbols.AliasSymbol? _lazyGlobalNamespaceAlias;

	private Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol? _lazyScriptClass = Microsoft.CodeAnalysis.CSharp.Symbols.ErrorTypeSymbol.UnknownResultType;

	private Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol? _lazyHostObjectTypeSymbol;

	private ConcurrentDictionary<ImportInfo, ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>>? _lazyImportInfos;

	private ImmutableArray<Diagnostic> _lazyClsComplianceDiagnostics;

	private ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> _lazyClsComplianceDependencies;

	private Conversions? _conversions;

	private AnonymousTypeManager? _lazyAnonymousTypeManager;

	private Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol? _lazyGlobalNamespace;

	private BuiltInOperators? _lazyBuiltInOperators;

	private Microsoft.CodeAnalysis.CSharp.Symbols.SourceAssemblySymbol? _lazyAssemblySymbol;

	private ReferenceManager _referenceManager;

	private readonly SyntaxAndDeclarationManager _syntaxAndDeclarations;

	private EntryPoint? _lazyEntryPoint;

	private ThreeState _lazyEmitNullablePublicOnly;

	private HashSet<SyntaxTree>? _lazyCompilationUnitCompletedTrees;

	private ImmutableHashSet<SyntaxTree>? _usageOfUsingsRecordedInTrees = ImmutableHashSet<SyntaxTree>.Empty;

	private ConcurrentCache<Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol, Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol>? _lazyTypeToNullableVersion;

	private ImmutableSegmentedDictionary<string, OneOrMany<SyntaxTree>> _mappedPathToSyntaxTree;

	private ImmutableSegmentedDictionary<string, OneOrMany<SyntaxTree>> _pathToSyntaxTree;

	private ImmutableSegmentedDictionary<ReadOnlyMemory<byte>, OneOrMany<SyntaxTree>> _contentHashToSyntaxTree;

	[CompilerGenerated]
	private ExtendedErrorTypeSymbol _003CImplicitlyTypedVariableUsedInForbiddenZoneType_003Ek__BackingField;

	[CompilerGenerated]
	private ExtendedErrorTypeSymbol _003CImplicitlyTypedVariableInferenceFailedType_003Ek__BackingField;

	private static readonly CSharpCompilationOptions s_defaultOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication);

	private static readonly CSharpCompilationOptions s_defaultSubmissionOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithReferencesSupersedeLowerVersions(value: true);

	private ConcurrentDictionary<string, Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol>? _externAliasTargets;

	private ConcurrentSet<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol>? _moduleInitializerMethods;

	internal bool InterceptorsDiscoveryComplete;

	private ConcurrentDictionary<(ImmutableArray<byte> ContentHash, int Position), OneOrMany<(Location AttributeLocation, Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol Interceptor)>>? _interceptions;

	private WeakReference<BinderFactory>[]? _binderFactories;

	private WeakReference<BinderFactory>[]? _ignoreAccessibilityBinderFactories;

	private DiagnosticBag? _lazyDeclarationDiagnostics;

	private bool _declarationDiagnosticsFrozen;

	private readonly DiagnosticBag _additionalCodegenWarnings = new DiagnosticBag();

	private ConcurrentSet<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>? _lazyUsedAssemblyReferences;

	private bool _usedAssemblyReferencesFrozen;

	private WellKnownMembersSignatureComparer? _lazyWellKnownMemberSignatureComparer;

	private Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol?[]? _lazyWellKnownTypes;

	private Symbol?[]? _lazyWellKnownTypeMembers;

	private bool _usesNullableAttributes;

	private int _needsGeneratedAttributes;

	private bool _needsGeneratedAttributes_IsFrozen;

	internal Conversions Conversions
	{
		get
		{
			if (_conversions == null)
			{
				Interlocked.CompareExchange(ref _conversions, new BuckStopsHereBinder(this, null).Conversions, null);
			}
			return _conversions;
		}
	}

	internal ImmutableHashSet<SyntaxTree>? UsageOfUsingsRecordedInTrees => Volatile.Read(in _usageOfUsingsRecordedInTrees);

	internal ExtendedErrorTypeSymbol ImplicitlyTypedVariableUsedInForbiddenZoneType
	{
		get
		{
			if ((object)_003CImplicitlyTypedVariableUsedInForbiddenZoneType_003Ek__BackingField == null)
			{
				Interlocked.CompareExchange(ref _003CImplicitlyTypedVariableUsedInForbiddenZoneType_003Ek__BackingField, new ExtendedErrorTypeSymbol(this, "var", 0, null, unreported: false, variableUsedBeforeDeclaration: true), null);
			}
			return _003CImplicitlyTypedVariableUsedInForbiddenZoneType_003Ek__BackingField;
		}
	}

	internal ExtendedErrorTypeSymbol ImplicitlyTypedVariableInferenceFailedType
	{
		get
		{
			if ((object)_003CImplicitlyTypedVariableInferenceFailedType_003Ek__BackingField == null)
			{
				Interlocked.CompareExchange(ref _003CImplicitlyTypedVariableInferenceFailedType_003Ek__BackingField, new ExtendedErrorTypeSymbol(this, "var", 0, null), null);
			}
			return _003CImplicitlyTypedVariableInferenceFailedType_003Ek__BackingField;
		}
	}

	public override string Language => "C#";

	public override bool IsCaseSensitive => true;

	public new CSharpCompilationOptions Options => _options;

	internal BuiltInOperators BuiltInOperators => InterlockedOperations.Initialize(ref _lazyBuiltInOperators, (CSharpCompilation self) => new BuiltInOperators(self), this);

	internal AnonymousTypeManager AnonymousTypeManager => InterlockedOperations.Initialize(ref _lazyAnonymousTypeManager, (CSharpCompilation self) => new AnonymousTypeManager(self), this);

	internal override CommonAnonymousTypeManager CommonAnonymousTypeManager => AnonymousTypeManager;

	internal bool FeatureStrictEnabled => HasFeature("strict");

	internal bool IsPeVerifyCompatEnabled
	{
		get
		{
			if (LanguageVersion >= LanguageVersion.CSharp7_2)
			{
				return HasFeature("peverify-compat");
			}
			return true;
		}
	}

	internal bool FeatureDisableLengthBasedSwitch => HasFeature("disable-length-based-switch");

	internal bool IsNullableAnalysisEnabledAlways => GetNullableAnalysisValue() == true;

	public LanguageVersion LanguageVersion { get; }

	public new CSharpScriptCompilationInfo? ScriptCompilationInfo { get; }

	internal override ScriptCompilationInfo? CommonScriptCompilationInfo => ScriptCompilationInfo;

	internal CSharpCompilation? PreviousSubmission => ScriptCompilationInfo?.PreviousScriptCompilation;

	public new ImmutableArray<SyntaxTree> SyntaxTrees => _syntaxAndDeclarations.GetLazyState().SyntaxTrees;

	public override ImmutableArray<MetadataReference> DirectiveReferences => GetBoundReferenceManager().DirectiveReferences;

	internal override IDictionary<(string path, string content), MetadataReference> ReferenceDirectiveMap => GetBoundReferenceManager().ReferenceDirectiveMap;

	internal IEnumerable<string> ExternAliases => GetBoundReferenceManager().ExternAliases;

	public override IEnumerable<AssemblyIdentity> ReferencedAssemblyNames => Assembly.Modules.SelectMany((Microsoft.CodeAnalysis.CSharp.Symbols.ModuleSymbol module) => module.GetReferencedAssemblies());

	internal override IEnumerable<ReferenceDirective> ReferenceDirectives => Declarations.ReferenceDirectives;

	internal Microsoft.CodeAnalysis.CSharp.Symbols.SourceAssemblySymbol SourceAssembly
	{
		get
		{
			GetBoundReferenceManager();
			return _lazyAssemblySymbol;
		}
	}

	internal new Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol Assembly => SourceAssembly;

	internal new Microsoft.CodeAnalysis.CSharp.Symbols.ModuleSymbol SourceModule => Assembly.Modules[0];

	internal new Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol GlobalNamespace
	{
		get
		{
			if ((object)_lazyGlobalNamespace == null)
			{
				ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.ModuleSymbol> instance = ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.ModuleSymbol>.GetInstance();
				GetAllUnaliasedModules(instance);
				Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol value = MergedNamespaceSymbol.Create(new NamespaceExtent(this), null, instance.SelectDistinct((Microsoft.CodeAnalysis.CSharp.Symbols.ModuleSymbol m) => m.GlobalNamespace));
				instance.Free();
				Interlocked.CompareExchange(ref _lazyGlobalNamespace, value, null);
			}
			return _lazyGlobalNamespace;
		}
	}

	internal new Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol? ScriptClass
	{
		get
		{
			if ((object)_lazyScriptClass == Microsoft.CodeAnalysis.CSharp.Symbols.ErrorTypeSymbol.UnknownResultType)
			{
				Interlocked.CompareExchange(ref _lazyScriptClass, BindScriptClass(), Microsoft.CodeAnalysis.CSharp.Symbols.ErrorTypeSymbol.UnknownResultType);
			}
			return _lazyScriptClass;
		}
	}

	internal ImmutableArray<NamespaceOrTypeAndUsingDirective> GlobalImports => InterlockedOperations.Initialize(ref _lazyGlobalImports, (CSharpCompilation self) => self.BindGlobalImports(), this);

	private UsingsFromOptionsAndDiagnostics UsingsFromOptions => InterlockedOperations.Initialize(ref _lazyUsingsFromOptions, (CSharpCompilation self) => self.BindUsingsFromOptions(), this);

	internal Microsoft.CodeAnalysis.CSharp.Symbols.AliasSymbol GlobalNamespaceAlias => InterlockedOperations.Initialize(ref _lazyGlobalNamespaceAlias, (CSharpCompilation self) => self.CreateGlobalNamespaceAlias(), this);

	private ConcurrentCache<Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol, Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol> TypeToNullableVersion => InterlockedOperations.Initialize(ref _lazyTypeToNullableVersion, () => new ConcurrentCache<Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol, Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol>(100));

	protected override ITypeSymbol? CommonScriptGlobalsType => GetHostObjectTypeSymbol()?.GetPublicSymbol();

	internal new Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol DynamicType => Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol.DynamicType;

	internal new Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol ObjectType => Assembly.ObjectType;

	internal bool DeclaresTheObjectClass => SourceAssembly.DeclaresTheObjectClass;

	internal override CommonMessageProvider MessageProvider => _syntaxAndDeclarations.MessageProvider;

	internal DiagnosticBag DeclarationDiagnostics
	{
		get
		{
			if (_lazyDeclarationDiagnostics == null)
			{
				DiagnosticBag value = new DiagnosticBag();
				Interlocked.CompareExchange(ref _lazyDeclarationDiagnostics, value, null);
			}
			return _lazyDeclarationDiagnostics;
		}
	}

	internal DiagnosticBag AdditionalCodegenWarnings => _additionalCodegenWarnings;

	internal DeclarationTable Declarations => _syntaxAndDeclarations.GetLazyState().DeclarationTable;

	internal MergedNamespaceDeclaration MergedRootDeclaration => Declarations.GetMergedRoot(this);

	internal override byte LinkerMajorVersion => 48;

	internal override bool IsDelaySigned => SourceAssembly.IsDelaySigned;

	internal override StrongNameKeys StrongNameKeys => SourceAssembly.StrongNameKeys;

	internal override Guid DebugSourceDocumentLanguageId => DebugSourceDocument.CorSymLanguageTypeCSharp;

	protected override IAssemblySymbol CommonAssembly => Assembly.GetPublicSymbol();

	protected override INamespaceSymbol CommonGlobalNamespace => GlobalNamespace.GetPublicSymbol();

	protected override CompilationOptions CommonOptions => _options;

	protected override ImmutableArray<SyntaxTree> CommonSyntaxTrees
	{
		protected internal get
		{
			return SyntaxTrees;
		}
	}

	protected override IModuleSymbol CommonSourceModule => SourceModule.GetPublicSymbol();

	protected override INamedTypeSymbol? CommonScriptClass => ScriptClass.GetPublicSymbol();

	protected override ITypeSymbol CommonDynamicType => DynamicType.GetPublicSymbol();

	protected override INamedTypeSymbol CommonObjectType => ObjectType.GetPublicSymbol();

	internal bool EmitNullablePublicOnly
	{
		get
		{
			if (!_lazyEmitNullablePublicOnly.HasValue())
			{
				SyntaxTree? syntaxTree = SyntaxTrees.FirstOrDefault();
				bool value = syntaxTree != null && syntaxTree.Options?.HasFeature("nullablePublicOnly") == true;
				_lazyEmitNullablePublicOnly = value.ToThreeState();
			}
			return _lazyEmitNullablePublicOnly.Value();
		}
	}

	internal bool EnableEnumArrayBlockInitialization
	{
		get
		{
			Symbol wellKnownTypeMember = GetWellKnownTypeMember(WellKnownMember.System_Runtime_GCLatencyMode__SustainedLowLatency);
			if (wellKnownTypeMember != null)
			{
				return wellKnownTypeMember.ContainingAssembly == Assembly.CorLibrary;
			}
			return false;
		}
	}

	internal WellKnownMembersSignatureComparer WellKnownMemberSignatureComparer => InterlockedOperations.Initialize(ref _lazyWellKnownMemberSignatureComparer, (CSharpCompilation self) => new WellKnownMembersSignatureComparer(self), this);

	internal bool IsNullableAnalysisEnabledIn(SyntaxNode syntax)
	{
		return IsNullableAnalysisEnabledIn((CSharpSyntaxTree)syntax.SyntaxTree, syntax.Span);
	}

	internal bool IsNullableAnalysisEnabledIn(CSharpSyntaxTree tree, TextSpan span)
	{
		return GetNullableAnalysisValue() ?? tree.IsNullableAnalysisEnabled(span) ?? ((Options.NullableContextOptions & NullableContextOptions.Warnings) != 0);
	}

	internal bool IsNullableAnalysisEnabledIn(Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol method)
	{
		return GetNullableAnalysisValue() ?? method.IsNullableAnalysisEnabled();
	}

	private bool? GetNullableAnalysisValue()
	{
		string text = Feature("run-nullable-analysis");
		if (!(text == "always"))
		{
			if (text == "never")
			{
				return false;
			}
			return null;
		}
		return true;
	}

	internal bool IsRuntimeAsyncEnabledIn(Symbol? symbol)
	{
		if (!Assembly.RuntimeSupportsAsyncMethods)
		{
			return false;
		}
		if (!(symbol is Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol methodSymbol))
		{
			return false;
		}
		InternalSpecialType internalSpecialType = (InternalSpecialType)methodSymbol.ReturnType.OriginalDefinition.ExtendedSpecialType;
		if ((uint)(internalSpecialType - 53) > 3u)
		{
			return false;
		}
		if (symbol is SourceMethodSymbol { IsRuntimeAsyncEnabledInMethod: var isRuntimeAsyncEnabledInMethod })
		{
			switch (isRuntimeAsyncEnabledInMethod)
			{
			case ThreeState.True:
				return true;
			case ThreeState.False:
				return false;
			}
		}
		return Feature("runtime-async") == "on";
	}

	protected override INamedTypeSymbol CommonCreateErrorTypeSymbol(INamespaceOrTypeSymbol? container, string name, int arity)
	{
		return new ExtendedErrorTypeSymbol(container.EnsureCSharpSymbolOrNull("container"), name, arity, null).GetPublicSymbol();
	}

	protected override INamespaceSymbol CommonCreateErrorNamespaceSymbol(INamespaceSymbol container, string name)
	{
		return new MissingNamespaceSymbol(container.EnsureCSharpSymbolOrNull("container"), name).GetPublicSymbol();
	}

	protected override IPreprocessingSymbol CommonCreatePreprocessingSymbol(string name)
	{
		return new PreprocessingSymbol(name);
	}

	public static CSharpCompilation Create(string? assemblyName, IEnumerable<SyntaxTree>? syntaxTrees = null, IEnumerable<MetadataReference>? references = null, CSharpCompilationOptions? options = null)
	{
		return Create(assemblyName, options ?? s_defaultOptions, syntaxTrees, references, null, null, null, isSubmission: false);
	}

	public static CSharpCompilation CreateScriptCompilation(string assemblyName, SyntaxTree? syntaxTree = null, IEnumerable<MetadataReference>? references = null, CSharpCompilationOptions? options = null, CSharpCompilation? previousScriptCompilation = null, Type? returnType = null, Type? globalsType = null)
	{
		Compilation.CheckSubmissionOptions(options);
		Compilation.ValidateScriptCompilationParameters(previousScriptCompilation, returnType, ref globalsType);
		CSharpCompilationOptions options2 = options?.WithReferencesSupersedeLowerVersions(value: true) ?? s_defaultSubmissionOptions;
		IEnumerable<SyntaxTree> syntaxTrees;
		if (syntaxTree == null)
		{
			syntaxTrees = SpecializedCollections.EmptyEnumerable<SyntaxTree>();
		}
		else
		{
			IEnumerable<SyntaxTree> enumerable = new SyntaxTree[1] { syntaxTree };
			syntaxTrees = enumerable;
		}
		return Create(assemblyName, options2, syntaxTrees, references, previousScriptCompilation, returnType, globalsType, isSubmission: true);
	}

	private static CSharpCompilation Create(string? assemblyName, CSharpCompilationOptions options, IEnumerable<SyntaxTree>? syntaxTrees, IEnumerable<MetadataReference>? references, CSharpCompilation? previousSubmission, Type? returnType, Type? hostObjectType, bool isSubmission)
	{
		ImmutableArray<MetadataReference> references2 = Compilation.ValidateReferences<CSharpCompilationReference>(references);
		CSharpCompilation cSharpCompilation = new CSharpCompilation(assemblyName, options, references2, previousSubmission, returnType, hostObjectType, isSubmission, null, reuseReferenceManager: false, new SyntaxAndDeclarationManager(ImmutableArray<SyntaxTree>.Empty, options.ScriptClassName, options.SourceReferenceResolver, Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance, isSubmission, null), null);
		if (syntaxTrees != null)
		{
			cSharpCompilation = cSharpCompilation.AddSyntaxTrees(syntaxTrees);
		}
		return cSharpCompilation;
	}

	private CSharpCompilation(string? assemblyName, CSharpCompilationOptions options, ImmutableArray<MetadataReference> references, CSharpCompilation? previousSubmission, Type? submissionReturnType, Type? hostObjectType, bool isSubmission, ReferenceManager? referenceManager, bool reuseReferenceManager, SyntaxAndDeclarationManager syntaxAndDeclarations, SemanticModelProvider? semanticModelProvider, AsyncQueue<CompilationEvent>? eventQueue = null)
		: this(assemblyName, options, references, previousSubmission, submissionReturnType, hostObjectType, isSubmission, referenceManager, reuseReferenceManager, syntaxAndDeclarations, Compilation.SyntaxTreeCommonFeatures(syntaxAndDeclarations.ExternalSyntaxTrees), semanticModelProvider, eventQueue)
	{
	}

	private CSharpCompilation(string? assemblyName, CSharpCompilationOptions options, ImmutableArray<MetadataReference> references, CSharpCompilation? previousSubmission, Type? submissionReturnType, Type? hostObjectType, bool isSubmission, ReferenceManager? referenceManager, bool reuseReferenceManager, SyntaxAndDeclarationManager syntaxAndDeclarations, IReadOnlyDictionary<string, string> features, SemanticModelProvider? semanticModelProvider, AsyncQueue<CompilationEvent>? eventQueue = null)
		: base(assemblyName, references, features, isSubmission, semanticModelProvider, eventQueue)
	{
		_options = options;
		LanguageVersion = CommonLanguageVersion(syntaxAndDeclarations.ExternalSyntaxTrees);
		if (isSubmission)
		{
			ScriptCompilationInfo = new CSharpScriptCompilationInfo(previousSubmission, submissionReturnType, hostObjectType);
		}
		if (reuseReferenceManager)
		{
			if (referenceManager == null)
			{
				throw new ArgumentNullException("referenceManager");
			}
			_referenceManager = referenceManager;
		}
		else
		{
			_referenceManager = new ReferenceManager(MakeSourceAssemblySimpleName(), Options.AssemblyIdentityComparer, referenceManager?.ObservedMetadata);
		}
		_syntaxAndDeclarations = syntaxAndDeclarations;
		if (base.EventQueue != null)
		{
			base.EventQueue.TryEnqueue(new CompilationStartedEvent(this));
		}
	}

	internal override void ValidateDebugEntryPoint(IMethodSymbol debugEntryPoint, DiagnosticBag diagnostics)
	{
		Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol methodSymbol = (debugEntryPoint as Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.MethodSymbol)?.UnderlyingMethodSymbol;
		if (methodSymbol?.DeclaringCompilation != this || !methodSymbol.IsDefinition)
		{
			diagnostics.Add(ErrorCode.ERR_DebugEntryPointNotSourceMethodDefinition, Location.None);
		}
	}

	private static LanguageVersion CommonLanguageVersion(ImmutableArray<SyntaxTree> syntaxTrees)
	{
		LanguageVersion? languageVersion = null;
		foreach (SyntaxTree item in syntaxTrees)
		{
			LanguageVersion languageVersion2 = ((CSharpParseOptions)item.Options).LanguageVersion;
			if (!languageVersion.HasValue)
			{
				languageVersion = languageVersion2;
			}
			else if (languageVersion != languageVersion2)
			{
				throw new ArgumentException(CodeAnalysisResources.InconsistentLanguageVersions, "syntaxTrees");
			}
		}
		return languageVersion ?? LanguageVersion.Default.MapSpecifiedToEffectiveVersion();
	}

	public new CSharpCompilation Clone()
	{
		return new CSharpCompilation(base.AssemblyName, _options, base.ExternalReferences, PreviousSubmission, base.SubmissionReturnType, base.HostObjectType, base.IsSubmission, _referenceManager, reuseReferenceManager: true, _syntaxAndDeclarations, base.SemanticModelProvider);
	}

	private CSharpCompilation Update(ReferenceManager referenceManager, bool reuseReferenceManager, SyntaxAndDeclarationManager syntaxAndDeclarations)
	{
		return new CSharpCompilation(base.AssemblyName, _options, base.ExternalReferences, PreviousSubmission, base.SubmissionReturnType, base.HostObjectType, base.IsSubmission, referenceManager, reuseReferenceManager, syntaxAndDeclarations, base.SemanticModelProvider);
	}

	public new CSharpCompilation WithAssemblyName(string? assemblyName)
	{
		return new CSharpCompilation(assemblyName, _options, base.ExternalReferences, PreviousSubmission, base.SubmissionReturnType, base.HostObjectType, base.IsSubmission, _referenceManager, assemblyName == base.AssemblyName, _syntaxAndDeclarations, base.SemanticModelProvider);
	}

	public new CSharpCompilation WithReferences(IEnumerable<MetadataReference>? references)
	{
		return new CSharpCompilation(base.AssemblyName, _options, Compilation.ValidateReferences<CSharpCompilationReference>(references), PreviousSubmission, base.SubmissionReturnType, base.HostObjectType, base.IsSubmission, null, reuseReferenceManager: false, _syntaxAndDeclarations, base.SemanticModelProvider);
	}

	public new CSharpCompilation WithReferences(params MetadataReference[] references)
	{
		return WithReferences((IEnumerable<MetadataReference>?)references);
	}

	public CSharpCompilation WithOptions(CSharpCompilationOptions options)
	{
		CSharpCompilationOptions options2 = Options;
		bool reuseReferenceManager = options2.CanReuseCompilationReferenceManager(options);
		bool flag = options2.ScriptClassName == options.ScriptClassName && options2.SourceReferenceResolver == options.SourceReferenceResolver;
		return new CSharpCompilation(base.AssemblyName, options, base.ExternalReferences, PreviousSubmission, base.SubmissionReturnType, base.HostObjectType, base.IsSubmission, _referenceManager, reuseReferenceManager, flag ? _syntaxAndDeclarations : new SyntaxAndDeclarationManager(_syntaxAndDeclarations.ExternalSyntaxTrees, options.ScriptClassName, options.SourceReferenceResolver, _syntaxAndDeclarations.MessageProvider, _syntaxAndDeclarations.IsSubmission, null), base.SemanticModelProvider);
	}

	public CSharpCompilation WithScriptCompilationInfo(CSharpScriptCompilationInfo? info)
	{
		if (info == ScriptCompilationInfo)
		{
			return this;
		}
		bool reuseReferenceManager = ScriptCompilationInfo?.PreviousScriptCompilation == info?.PreviousScriptCompilation;
		return new CSharpCompilation(base.AssemblyName, _options, base.ExternalReferences, info?.PreviousScriptCompilation, info?.ReturnTypeOpt, info?.GlobalsType, info != null, _referenceManager, reuseReferenceManager, _syntaxAndDeclarations, base.SemanticModelProvider);
	}

	internal override Compilation WithSemanticModelProvider(SemanticModelProvider? semanticModelProvider)
	{
		if (base.SemanticModelProvider == semanticModelProvider)
		{
			return this;
		}
		return new CSharpCompilation(base.AssemblyName, _options, base.ExternalReferences, PreviousSubmission, base.SubmissionReturnType, base.HostObjectType, base.IsSubmission, _referenceManager, reuseReferenceManager: true, _syntaxAndDeclarations, semanticModelProvider);
	}

	internal override Compilation WithEventQueue(AsyncQueue<CompilationEvent>? eventQueue)
	{
		return new CSharpCompilation(base.AssemblyName, _options, base.ExternalReferences, PreviousSubmission, base.SubmissionReturnType, base.HostObjectType, base.IsSubmission, _referenceManager, reuseReferenceManager: true, _syntaxAndDeclarations, base.SemanticModelProvider, eventQueue);
	}

	internal override bool HasSubmissionResult()
	{
		SyntaxTree syntaxTree = _syntaxAndDeclarations.ExternalSyntaxTrees.SingleOrDefault();
		if (syntaxTree == null)
		{
			return false;
		}
		CompilationUnitSyntax compilationUnitRoot = syntaxTree.GetCompilationUnitRoot();
		if (compilationUnitRoot.HasErrors)
		{
			return false;
		}
		if (compilationUnitRoot.DescendantNodes((SyntaxNode n) => n is GlobalStatementSyntax || n is StatementSyntax || n is CompilationUnitSyntax).Any((SyntaxNode n) => n.IsKind(SyntaxKind.ReturnStatement)))
		{
			return true;
		}
		GlobalStatementSyntax globalStatementSyntax = (GlobalStatementSyntax)compilationUnitRoot.Members.LastOrDefault((MemberDeclarationSyntax m) => m.IsKind(SyntaxKind.GlobalStatement));
		if (globalStatementSyntax != null)
		{
			StatementSyntax statement = globalStatementSyntax.Statement;
			if (statement.IsKind(SyntaxKind.ExpressionStatement))
			{
				ExpressionStatementSyntax expressionStatementSyntax = (ExpressionStatementSyntax)statement;
				if (expressionStatementSyntax.SemicolonToken.IsMissing)
				{
					SemanticModel semanticModel = GetSemanticModel(syntaxTree);
					ExpressionSyntax expression = expressionStatementSyntax.Expression;
					ITypeSymbol? convertedType = semanticModel.GetTypeInfo(expression).ConvertedType;
					if (convertedType == null)
					{
						return true;
					}
					return convertedType.SpecialType != SpecialType.System_Void;
				}
			}
		}
		return false;
	}

	public new bool ContainsSyntaxTree(SyntaxTree? syntaxTree)
	{
		if (syntaxTree != null)
		{
			return _syntaxAndDeclarations.GetLazyState().RootNamespaces.ContainsKey(syntaxTree);
		}
		return false;
	}

	public new CSharpCompilation AddSyntaxTrees(params SyntaxTree[] trees)
	{
		return AddSyntaxTrees((IEnumerable<SyntaxTree>)trees);
	}

	public new CSharpCompilation AddSyntaxTrees(IEnumerable<SyntaxTree> trees)
	{
		if (trees == null)
		{
			throw new ArgumentNullException("trees");
		}
		if (trees.IsEmpty())
		{
			return this;
		}
		PooledHashSet<SyntaxTree> instance = PooledHashSet<SyntaxTree>.GetInstance();
		SyntaxAndDeclarationManager syntaxAndDeclarations = _syntaxAndDeclarations;
		instance.AddAll(syntaxAndDeclarations.ExternalSyntaxTrees);
		bool flag = true;
		int num = 0;
		foreach (CSharpSyntaxTree item in trees.Cast<CSharpSyntaxTree>())
		{
			if (item == null)
			{
				throw new ArgumentNullException(string.Format("{0}[{1}]", "trees", num));
			}
			if (!item.HasCompilationUnitRoot)
			{
				throw new ArgumentException(CSharpResources.TreeMustHaveARootNodeWith, string.Format("{0}[{1}]", "trees", num));
			}
			if (instance.Contains(item))
			{
				throw new ArgumentException(CSharpResources.SyntaxTreeAlreadyPresent, string.Format("{0}[{1}]", "trees", num));
			}
			if (base.IsSubmission && item.Options.Kind == SourceCodeKind.Regular)
			{
				throw new ArgumentException(CSharpResources.SubmissionCanOnlyInclude, string.Format("{0}[{1}]", "trees", num));
			}
			instance.Add(item);
			flag &= !item.HasReferenceOrLoadDirectives;
			num++;
		}
		instance.Free();
		if (base.IsSubmission && num > 1)
		{
			throw new ArgumentException(CSharpResources.SubmissionCanHaveAtMostOne, "trees");
		}
		syntaxAndDeclarations = syntaxAndDeclarations.AddSyntaxTrees(trees);
		return Update(_referenceManager, flag, syntaxAndDeclarations);
	}

	public new CSharpCompilation RemoveSyntaxTrees(params SyntaxTree[] trees)
	{
		return RemoveSyntaxTrees((IEnumerable<SyntaxTree>)trees);
	}

	public new CSharpCompilation RemoveSyntaxTrees(IEnumerable<SyntaxTree> trees)
	{
		if (trees == null)
		{
			throw new ArgumentNullException("trees");
		}
		if (trees.IsEmpty())
		{
			return this;
		}
		PooledHashSet<SyntaxTree> instance = PooledHashSet<SyntaxTree>.GetInstance();
		PooledHashSet<SyntaxTree> instance2 = PooledHashSet<SyntaxTree>.GetInstance();
		SyntaxAndDeclarationManager syntaxAndDeclarations = _syntaxAndDeclarations;
		instance2.AddAll(syntaxAndDeclarations.ExternalSyntaxTrees);
		bool flag = true;
		int num = 0;
		foreach (CSharpSyntaxTree item in trees.Cast<CSharpSyntaxTree>())
		{
			if (!instance2.Contains(item))
			{
				ImmutableDictionary<string, SyntaxTree> loadedSyntaxTreeMap = syntaxAndDeclarations.GetLazyState().LoadedSyntaxTreeMap;
				if (SyntaxAndDeclarationManager.IsLoadedSyntaxTree(item, loadedSyntaxTreeMap))
				{
					throw new ArgumentException(CSharpResources.SyntaxTreeFromLoadNoRemoveReplace, string.Format("{0}[{1}]", "trees", num));
				}
				throw new ArgumentException(CSharpResources.SyntaxTreeNotFoundToRemove, string.Format("{0}[{1}]", "trees", num));
			}
			instance.Add(item);
			flag &= !item.HasReferenceOrLoadDirectives;
			num++;
		}
		instance2.Free();
		syntaxAndDeclarations = syntaxAndDeclarations.RemoveSyntaxTrees(instance);
		instance.Free();
		return Update(_referenceManager, flag, syntaxAndDeclarations);
	}

	public new CSharpCompilation RemoveAllSyntaxTrees()
	{
		SyntaxAndDeclarationManager syntaxAndDeclarations = _syntaxAndDeclarations;
		return Update(_referenceManager, !syntaxAndDeclarations.MayHaveReferenceDirectives(), syntaxAndDeclarations.WithExternalSyntaxTrees(ImmutableArray<SyntaxTree>.Empty));
	}

	public new CSharpCompilation ReplaceSyntaxTree(SyntaxTree oldTree, SyntaxTree? newTree)
	{
		oldTree = (CSharpSyntaxTree)oldTree;
		newTree = (CSharpSyntaxTree)newTree;
		if (oldTree == null)
		{
			throw new ArgumentNullException("oldTree");
		}
		if (newTree == null)
		{
			return RemoveSyntaxTrees(oldTree);
		}
		if (newTree == oldTree)
		{
			return this;
		}
		if (!newTree.HasCompilationUnitRoot)
		{
			throw new ArgumentException(CSharpResources.TreeMustHaveARootNodeWith, "newTree");
		}
		SyntaxAndDeclarationManager syntaxAndDeclarations = _syntaxAndDeclarations;
		ImmutableArray<SyntaxTree> externalSyntaxTrees = syntaxAndDeclarations.ExternalSyntaxTrees;
		if (!externalSyntaxTrees.Contains(oldTree))
		{
			ImmutableDictionary<string, SyntaxTree> loadedSyntaxTreeMap = syntaxAndDeclarations.GetLazyState().LoadedSyntaxTreeMap;
			if (SyntaxAndDeclarationManager.IsLoadedSyntaxTree(oldTree, loadedSyntaxTreeMap))
			{
				throw new ArgumentException(CSharpResources.SyntaxTreeFromLoadNoRemoveReplace, "oldTree");
			}
			throw new ArgumentException(CSharpResources.SyntaxTreeNotFoundToRemove, "oldTree");
		}
		if (externalSyntaxTrees.Contains(newTree))
		{
			throw new ArgumentException(CSharpResources.SyntaxTreeAlreadyPresent, "newTree");
		}
		bool reuseReferenceManager = !oldTree.HasReferenceOrLoadDirectives() && !newTree.HasReferenceOrLoadDirectives();
		syntaxAndDeclarations = syntaxAndDeclarations.ReplaceSyntaxTree(oldTree, newTree);
		return Update(_referenceManager, reuseReferenceManager, syntaxAndDeclarations);
	}

	internal override int GetSyntaxTreeOrdinal(SyntaxTree tree)
	{
		try
		{
			return _syntaxAndDeclarations.GetLazyState().OrdinalMap[tree];
		}
		catch (KeyNotFoundException)
		{
			throw new KeyNotFoundException("Syntax tree not found with file path: " + tree.FilePath);
		}
	}

	internal OneOrMany<SyntaxTree> GetSyntaxTreesByMappedPath(string mappedPath)
	{
		ImmutableSegmentedDictionary<string, OneOrMany<SyntaxTree>> mappedPathToSyntaxTree = _mappedPathToSyntaxTree;
		if (mappedPathToSyntaxTree.IsDefault)
		{
			RoslynImmutableInterlocked.InterlockedInitialize(ref _mappedPathToSyntaxTree, computeMappedPathToSyntaxTree());
			mappedPathToSyntaxTree = _mappedPathToSyntaxTree;
		}
		if (!mappedPathToSyntaxTree.TryGetValue(mappedPath, out var value))
		{
			return OneOrMany<SyntaxTree>.Empty;
		}
		return value;
		ImmutableSegmentedDictionary<string, OneOrMany<SyntaxTree>> computeMappedPathToSyntaxTree()
		{
			ImmutableSegmentedDictionary<string, OneOrMany<SyntaxTree>>.Builder builder = ImmutableSegmentedDictionary.CreateBuilder<string, OneOrMany<SyntaxTree>>();
			SourceReferenceResolver sourceReferenceResolver = Options.SourceReferenceResolver;
			foreach (SyntaxTree syntaxTree in SyntaxTrees)
			{
				string key = sourceReferenceResolver?.NormalizePath(syntaxTree.FilePath, null) ?? syntaxTree.FilePath;
				builder[key] = (builder.ContainsKey(key) ? builder[key].Add(syntaxTree) : OneOrMany.Create(syntaxTree));
			}
			return builder.ToImmutable();
		}
	}

	internal OneOrMany<SyntaxTree> GetSyntaxTreesByContentHash(ReadOnlyMemory<byte> contentHash)
	{
		ImmutableSegmentedDictionary<ReadOnlyMemory<byte>, OneOrMany<SyntaxTree>> contentHashToSyntaxTree = _contentHashToSyntaxTree;
		if (contentHashToSyntaxTree.IsDefault)
		{
			RoslynImmutableInterlocked.InterlockedInitialize(ref _contentHashToSyntaxTree, computeHashToSyntaxTree());
			contentHashToSyntaxTree = _contentHashToSyntaxTree;
		}
		if (!contentHashToSyntaxTree.TryGetValue(contentHash, out var value))
		{
			return OneOrMany<SyntaxTree>.Empty;
		}
		return value;
		ImmutableSegmentedDictionary<ReadOnlyMemory<byte>, OneOrMany<SyntaxTree>> computeHashToSyntaxTree()
		{
			ImmutableSegmentedDictionary<ReadOnlyMemory<byte>, OneOrMany<SyntaxTree>>.Builder builder = ImmutableSegmentedDictionary.CreateBuilder<ReadOnlyMemory<byte>, OneOrMany<SyntaxTree>>(ContentHashComparer.Instance);
			foreach (SyntaxTree syntaxTree in SyntaxTrees)
			{
				ReadOnlyMemory<byte> key = syntaxTree.GetText().GetContentHash().AsMemory();
				builder[key] = (builder.TryGetValue(key, out var value2) ? value2.Add(syntaxTree) : OneOrMany.Create(syntaxTree));
			}
			return builder.ToImmutable();
		}
	}

	internal OneOrMany<SyntaxTree> GetSyntaxTreesByPath(string path)
	{
		ImmutableSegmentedDictionary<string, OneOrMany<SyntaxTree>> pathToSyntaxTree = _pathToSyntaxTree;
		if (pathToSyntaxTree.IsDefault)
		{
			RoslynImmutableInterlocked.InterlockedInitialize(ref _pathToSyntaxTree, computePathToSyntaxTree());
			pathToSyntaxTree = _pathToSyntaxTree;
		}
		if (!pathToSyntaxTree.TryGetValue(path, out var value))
		{
			return OneOrMany<SyntaxTree>.Empty;
		}
		return value;
		ImmutableSegmentedDictionary<string, OneOrMany<SyntaxTree>> computePathToSyntaxTree()
		{
			ImmutableSegmentedDictionary<string, OneOrMany<SyntaxTree>>.Builder builder = ImmutableSegmentedDictionary.CreateBuilder<string, OneOrMany<SyntaxTree>>();
			foreach (SyntaxTree syntaxTree in SyntaxTrees)
			{
				string normalizedPathOrOriginalPath = FileUtilities.GetNormalizedPathOrOriginalPath(syntaxTree.FilePath, null);
				builder[normalizedPathOrOriginalPath] = (builder.ContainsKey(normalizedPathOrOriginalPath) ? builder[normalizedPathOrOriginalPath].Add(syntaxTree) : OneOrMany.Create(syntaxTree));
			}
			return builder.ToImmutable();
		}
	}

	internal override CommonReferenceManager CommonGetBoundReferenceManager()
	{
		return GetBoundReferenceManager();
	}

	internal new ReferenceManager GetBoundReferenceManager()
	{
		if ((object)_lazyAssemblySymbol == null)
		{
			_referenceManager.CreateSourceAssemblyForCompilation(this);
		}
		return _referenceManager;
	}

	internal bool ReferenceManagerEquals(CSharpCompilation other)
	{
		return _referenceManager == other._referenceManager;
	}

	internal new Symbol? GetAssemblyOrModuleSymbol(MetadataReference reference)
	{
		if (reference == null)
		{
			throw new ArgumentNullException("reference");
		}
		if (reference.Properties.Kind == MetadataImageKind.Assembly)
		{
			return GetBoundReferenceManager().GetReferencedAssemblySymbol(reference);
		}
		int referencedModuleIndex = GetBoundReferenceManager().GetReferencedModuleIndex(reference);
		if (referencedModuleIndex >= 0)
		{
			return Assembly.Modules[referencedModuleIndex];
		}
		return null;
	}

	internal override TSymbol? GetSymbolInternal<TSymbol>(ISymbol? symbol) where TSymbol : class
	{
		return (TSymbol)(object)symbol.GetSymbol<Symbol>();
	}

	public MetadataReference? GetDirectiveReference(ReferenceDirectiveTriviaSyntax directive)
	{
		if (!ReferenceDirectiveMap.TryGetValue((directive.SyntaxTree.FilePath, directive.File.ValueText), out MetadataReference value))
		{
			return null;
		}
		return value;
	}

	public new CSharpCompilation AddReferences(params MetadataReference[] references)
	{
		return (CSharpCompilation)base.AddReferences(references);
	}

	public new CSharpCompilation AddReferences(IEnumerable<MetadataReference> references)
	{
		return (CSharpCompilation)base.AddReferences(references);
	}

	public new CSharpCompilation RemoveReferences(params MetadataReference[] references)
	{
		return (CSharpCompilation)base.RemoveReferences(references);
	}

	public new CSharpCompilation RemoveReferences(IEnumerable<MetadataReference> references)
	{
		return (CSharpCompilation)base.RemoveReferences(references);
	}

	public new CSharpCompilation RemoveAllReferences()
	{
		return (CSharpCompilation)base.RemoveAllReferences();
	}

	public new CSharpCompilation ReplaceReference(MetadataReference oldReference, MetadataReference newReference)
	{
		return (CSharpCompilation)base.ReplaceReference(oldReference, newReference);
	}

	public override CompilationReference ToMetadataReference(ImmutableArray<string> aliases = default(ImmutableArray<string>), bool embedInteropTypes = false)
	{
		return new CSharpCompilationReference(this, aliases, embedInteropTypes);
	}

	private void GetAllUnaliasedModules(ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.ModuleSymbol> modules)
	{
		modules.AddRange(Assembly.Modules);
		ReferenceManager boundReferenceManager = GetBoundReferenceManager();
		for (int i = 0; i < boundReferenceManager.ReferencedAssemblies.Length; i++)
		{
			if (boundReferenceManager.DeclarationsAccessibleWithoutAlias(i))
			{
				modules.AddRange(boundReferenceManager.ReferencedAssemblies[i].Modules);
			}
		}
	}

	internal void GetUnaliasedReferencedAssemblies(ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> assemblies)
	{
		ReferenceManager boundReferenceManager = GetBoundReferenceManager();
		int length = boundReferenceManager.ReferencedAssemblies.Length;
		assemblies.EnsureCapacity(assemblies.Count + length);
		for (int i = 0; i < length; i++)
		{
			if (boundReferenceManager.DeclarationsAccessibleWithoutAlias(i))
			{
				assemblies.Add(boundReferenceManager.ReferencedAssemblies[i]);
			}
		}
	}

	public new MetadataReference? GetMetadataReference(IAssemblySymbol assemblySymbol)
	{
		return base.GetMetadataReference(assemblySymbol);
	}

	private protected override MetadataReference? CommonGetMetadataReference(IAssemblySymbol assemblySymbol)
	{
		if (assemblySymbol is Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.AssemblySymbol { UnderlyingAssemblySymbol: var underlyingAssemblySymbol })
		{
			return GetMetadataReference(underlyingAssemblySymbol);
		}
		return null;
	}

	internal MetadataReference? GetMetadataReference(Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol? assemblySymbol)
	{
		return GetBoundReferenceManager().GetMetadataReference(assemblySymbol);
	}

	internal new Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol? GetCompilationNamespace(INamespaceSymbol namespaceSymbol)
	{
		if (namespaceSymbol is Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.NamespaceSymbol namespaceSymbol2 && namespaceSymbol.NamespaceKind == NamespaceKind.Compilation && namespaceSymbol.ContainingCompilation == this)
		{
			return namespaceSymbol2.UnderlyingNamespaceSymbol;
		}
		INamespaceSymbol containingNamespace = namespaceSymbol.ContainingNamespace;
		if (containingNamespace == null)
		{
			return GlobalNamespace;
		}
		return GetCompilationNamespace(containingNamespace)?.GetNestedNamespace(namespaceSymbol.Name);
	}

	internal Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol? GetCompilationNamespace(Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol namespaceSymbol)
	{
		if (namespaceSymbol.NamespaceKind == NamespaceKind.Compilation && namespaceSymbol.ContainingCompilation == this)
		{
			return namespaceSymbol;
		}
		Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol containingNamespace = namespaceSymbol.ContainingNamespace;
		if (containingNamespace == null)
		{
			return GlobalNamespace;
		}
		return GetCompilationNamespace(containingNamespace)?.GetNestedNamespace(namespaceSymbol.Name);
	}

	internal bool GetExternAliasTarget(string aliasName, out Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol @namespace)
	{
		Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol value;
		if (_externAliasTargets == null)
		{
			Interlocked.CompareExchange(ref _externAliasTargets, new ConcurrentDictionary<string, Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol>(), null);
		}
		else if (_externAliasTargets.TryGetValue(aliasName, out value))
		{
			@namespace = value;
			return !(@namespace is MissingNamespaceSymbol);
		}
		ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol> arrayBuilder = null;
		ReferenceManager boundReferenceManager = GetBoundReferenceManager();
		for (int i = 0; i < boundReferenceManager.ReferencedAssemblies.Length; i++)
		{
			if (boundReferenceManager.AliasesOfReferencedAssemblies[i].Contains(aliasName))
			{
				arrayBuilder = arrayBuilder ?? ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol>.GetInstance();
				arrayBuilder.Add(boundReferenceManager.ReferencedAssemblies[i].GlobalNamespace);
			}
		}
		bool flag = arrayBuilder != null;
		@namespace = (flag ? MergedNamespaceSymbol.Create(new NamespaceExtent(this), null, arrayBuilder.ToImmutableAndFree()) : new MissingNamespaceSymbol(new MissingModuleSymbol(new MissingAssemblySymbol(new AssemblyIdentity(Guid.NewGuid().ToString())), -1)));
		@namespace = _externAliasTargets.GetOrAdd(aliasName, @namespace);
		return flag;
	}

	private ImplicitNamedTypeSymbol? BindScriptClass()
	{
		return (ImplicitNamedTypeSymbol)CommonBindScriptClass().GetSymbol();
	}

	internal bool IsSubmissionSyntaxTree(SyntaxTree tree)
	{
		if (base.IsSubmission)
		{
			return tree == _syntaxAndDeclarations.ExternalSyntaxTrees.SingleOrDefault();
		}
		return false;
	}

	private ImmutableArray<NamespaceOrTypeAndUsingDirective> BindGlobalImports()
	{
		UsingsFromOptionsAndDiagnostics usingsFromOptions = UsingsFromOptions;
		CSharpCompilation previousSubmission = PreviousSubmission;
		ImmutableArray<NamespaceOrTypeAndUsingDirective> result = ((previousSubmission != null) ? Imports.ExpandPreviousSubmissionImports(previousSubmission.GlobalImports, this) : ImmutableArray<NamespaceOrTypeAndUsingDirective>.Empty);
		if (usingsFromOptions.UsingNamespacesOrTypes.IsEmpty)
		{
			return result;
		}
		if (result.IsEmpty)
		{
			return usingsFromOptions.UsingNamespacesOrTypes;
		}
		ArrayBuilder<NamespaceOrTypeAndUsingDirective> instance = ArrayBuilder<NamespaceOrTypeAndUsingDirective>.GetInstance();
		PooledHashSet<Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol> instance2 = PooledHashSet<Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol>.GetInstance();
		instance.AddRange(usingsFromOptions.UsingNamespacesOrTypes);
		instance2.AddAll(usingsFromOptions.UsingNamespacesOrTypes.Select((NamespaceOrTypeAndUsingDirective unt) => unt.NamespaceOrType));
		foreach (NamespaceOrTypeAndUsingDirective item in result)
		{
			if (instance2.Add(item.NamespaceOrType))
			{
				instance.Add(item);
			}
		}
		instance2.Free();
		return instance.ToImmutableAndFree();
	}

	private UsingsFromOptionsAndDiagnostics BindUsingsFromOptions()
	{
		return UsingsFromOptionsAndDiagnostics.FromOptions(this);
	}

	internal Imports GetSubmissionImports()
	{
		SyntaxTree syntaxTree = _syntaxAndDeclarations.ExternalSyntaxTrees.SingleOrDefault();
		if (syntaxTree == null)
		{
			return Imports.Empty;
		}
		return ((SourceNamespaceSymbol)SourceModule.GlobalNamespace).GetImports((CSharpSyntaxNode)syntaxTree.GetRoot(), null);
	}

	internal Imports GetPreviousSubmissionImports()
	{
		return InterlockedOperations.Initialize(ref _lazyPreviousSubmissionImports, (CSharpCompilation self) => self.ExpandPreviousSubmissionImports(), this);
	}

	private Imports ExpandPreviousSubmissionImports()
	{
		CSharpCompilation previousSubmission = PreviousSubmission;
		if (previousSubmission == null)
		{
			return Imports.Empty;
		}
		return Imports.ExpandPreviousSubmissionImports(previousSubmission.GetPreviousSubmissionImports(), this).Concat(Imports.ExpandPreviousSubmissionImports(previousSubmission.GetSubmissionImports(), this));
	}

	internal Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol GetSpecialType(ExtendedSpecialType specialType)
	{
		if ((int)specialType <= 0 || (int)specialType >= 58)
		{
			throw new ArgumentOutOfRangeException("specialType", $"Unexpected SpecialType: '{(int)specialType}'.");
		}
		if (IsTypeMissing(specialType))
		{
			MetadataTypeName fullName = MetadataTypeName.FromFullName(specialType.GetMetadataName(), useCLSCompliantNameArityEncoding: true);
			return new MissingMetadataTypeSymbol.TopLevel(Assembly.CorLibrary.Modules[0], ref fullName, specialType);
		}
		return Assembly.GetSpecialType(specialType);
	}

	internal Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol GetOrCreateNullableType(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol typeArgument)
	{
		ConcurrentCache<Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol, Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol> typeToNullableVersion = TypeToNullableVersion;
		if (!typeToNullableVersion.TryGetValue(typeArgument, out var value))
		{
			value = GetSpecialType(SpecialType.System_Nullable_T).Construct(typeArgument);
			typeToNullableVersion.TryAdd(typeArgument, value);
		}
		return value;
	}

	internal Symbol GetSpecialTypeMember(SpecialMember specialMember)
	{
		return Assembly.GetSpecialTypeMember(specialMember);
	}

	internal override ISymbolInternal CommonGetSpecialTypeMember(SpecialMember specialMember)
	{
		return GetSpecialTypeMember(specialMember);
	}

	internal Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol GetTypeByReflectionType(Type type, BindingDiagnosticBag diagnostics)
	{
		Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol typeSymbol = Assembly.GetTypeByReflectionType(type);
		if ((object)typeSymbol == null)
		{
			ExtendedErrorTypeSymbol extendedErrorTypeSymbol = new ExtendedErrorTypeSymbol(this, type.Name, 0, CreateReflectionTypeNotFoundError(type));
			diagnostics.Add(extendedErrorTypeSymbol.ErrorInfo, NoLocation.Singleton);
			typeSymbol = extendedErrorTypeSymbol;
		}
		return typeSymbol;
	}

	private static CSDiagnosticInfo CreateReflectionTypeNotFoundError(Type type)
	{
		return new CSDiagnosticInfo(ErrorCode.ERR_GlobalSingleTypeNameNotFound, new object[1] { type.AssemblyQualifiedName ?? "" }, ImmutableArray<Symbol>.Empty, ImmutableArray<Location>.Empty);
	}

	internal Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol? GetHostObjectTypeSymbol()
	{
		if (base.HostObjectType != null && (object)_lazyHostObjectTypeSymbol == null)
		{
			Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol typeSymbol = Assembly.GetTypeByReflectionType(base.HostObjectType);
			if ((object)typeSymbol == null)
			{
				MetadataTypeName fullName = MetadataTypeName.FromNamespaceAndTypeName(base.HostObjectType.Namespace ?? string.Empty, base.HostObjectType.Name, useCLSCompliantNameArityEncoding: true);
				typeSymbol = new MissingMetadataTypeSymbol.TopLevel(new MissingAssemblySymbol(AssemblyIdentity.FromAssemblyDefinition(base.HostObjectType.GetTypeInfo().Assembly)).Modules[0], ref fullName, SpecialType.None, CreateReflectionTypeNotFoundError(base.HostObjectType));
			}
			Interlocked.CompareExchange(ref _lazyHostObjectTypeSymbol, typeSymbol, null);
		}
		return _lazyHostObjectTypeSymbol;
	}

	internal SynthesizedInteractiveInitializerMethod? GetSubmissionInitializer()
	{
		if (!base.IsSubmission || (object)ScriptClass == null)
		{
			return null;
		}
		return ScriptClass.GetScriptInitializer();
	}

	internal new Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol? GetTypeByMetadataName(string fullyQualifiedMetadataName)
	{
		(Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol) conflicts;
		return Assembly.GetTypeByMetadataName(fullyQualifiedMetadataName, includeReferences: true, isWellKnownType: false, out conflicts);
	}

	internal new Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol? GetEntryPoint(CancellationToken cancellationToken)
	{
		return GetEntryPointAndDiagnostics(cancellationToken).MethodSymbol;
	}

	internal EntryPoint GetEntryPointAndDiagnostics(CancellationToken cancellationToken)
	{
		if (_lazyEntryPoint == null)
		{
			SynthesizedSimpleProgramEntryPointSymbol simpleProgramEntryPoint = SynthesizedSimpleProgramEntryPointSymbol.GetSimpleProgramEntryPoint(this);
			EntryPoint entryPoint;
			if (!Options.OutputKind.IsApplication() && (object)ScriptClass == null)
			{
				if ((object)simpleProgramEntryPoint != null)
				{
					BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
					instance.Add(ErrorCode.ERR_SimpleProgramNotAnExecutable, simpleProgramEntryPoint.ReturnTypeSyntax.Location);
					entryPoint = new EntryPoint(null, instance.ToReadOnlyAndFree());
				}
				else
				{
					entryPoint = EntryPoint.None;
				}
			}
			else
			{
				entryPoint = null;
				if (Options.MainTypeName != null && !Options.MainTypeName.IsValidClrTypeName())
				{
					entryPoint = EntryPoint.None;
				}
				if (entryPoint == null)
				{
					entryPoint = new EntryPoint(FindEntryPoint(simpleProgramEntryPoint, cancellationToken, out ReadOnlyBindingDiagnostic<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> sealedDiagnostics), sealedDiagnostics);
				}
			}
			Interlocked.CompareExchange(ref _lazyEntryPoint, entryPoint, null);
		}
		return _lazyEntryPoint;
	}

	private Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol? FindEntryPoint(Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol? simpleProgramEntryPointSymbol, CancellationToken cancellationToken, out ReadOnlyBindingDiagnostic<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> sealedDiagnostics)
	{
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
		ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol> instance2 = ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol>.GetInstance();
		try
		{
			string mainTypeName = Options.MainTypeName;
			Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol globalNamespace = SourceModule.GlobalNamespace;
			Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol scriptClass = ScriptClass;
			Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol namedTypeSymbol;
			if (mainTypeName != null)
			{
				if ((object)scriptClass != null)
				{
					instance.Add(ErrorCode.WRN_MainIgnored, NoLocation.Singleton, mainTypeName);
					return scriptClass.GetScriptEntryPoint();
				}
				string[] array = mainTypeName.Split(new char[1] { '.' });
				if (array.Any((string n) => string.IsNullOrWhiteSpace(n)))
				{
					instance.Add(ErrorCode.ERR_BadCompilationOptionValue, NoLocation.Singleton, "MainTypeName", mainTypeName);
					return null;
				}
				Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceOrTypeSymbol namespaceOrTypeSymbol = globalNamespace.GetNamespaceOrTypeByQualifiedName(array).OfMinimalArity();
				if ((object)namespaceOrTypeSymbol == null)
				{
					instance.Add(ErrorCode.ERR_MainClassNotFound, NoLocation.Singleton, mainTypeName);
					return null;
				}
				namedTypeSymbol = namespaceOrTypeSymbol as Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol;
				if ((object)namedTypeSymbol == null || namedTypeSymbol.IsGenericType || (namedTypeSymbol.TypeKind != TypeKind.Class && namedTypeSymbol.TypeKind != TypeKind.Struct && !namedTypeSymbol.IsInterface))
				{
					instance.Add(ErrorCode.ERR_MainClassNotClass, namespaceOrTypeSymbol.GetFirstLocation(), namespaceOrTypeSymbol);
					return null;
				}
				AddEntryPointCandidates(instance2, namedTypeSymbol.GetMembersUnordered());
			}
			else
			{
				namedTypeSymbol = null;
				AddEntryPointCandidates(instance2, GetSymbolsWithNameCore("Main", SymbolFilter.Member, cancellationToken));
				if ((object)scriptClass != null || (object)simpleProgramEntryPointSymbol != null)
				{
					foreach (Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol item in instance2)
					{
						if (!(item is SynthesizedSimpleProgramEntryPointSymbol))
						{
							instance.Add(ErrorCode.WRN_MainIgnored, item.GetFirstLocation(), item);
						}
					}
					if ((object)scriptClass != null)
					{
						return scriptClass.GetScriptEntryPoint();
					}
					instance2.Clear();
					instance2.Add(simpleProgramEntryPointSymbol);
				}
			}
			ArrayBuilder<(bool, Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol, BindingDiagnosticBag)> instance3 = ArrayBuilder<(bool, Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol, BindingDiagnosticBag)>.GetInstance();
			BindingDiagnosticBag noMainFoundDiagnostics = BindingDiagnosticBag.GetInstance(instance);
			ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol> instance4 = ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol>.GetInstance();
			foreach (Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol item2 in instance2)
			{
				BindingDiagnosticBag instance5 = BindingDiagnosticBag.GetInstance(instance);
				(bool IsCandidate, bool IsTaskLike) tuple = HasEntryPointSignature(item2, instance5);
				var (flag, _) = tuple;
				if (tuple.IsTaskLike)
				{
					instance3.Add((flag, item2, instance5));
					continue;
				}
				if (checkValid(item2, flag, instance5))
				{
					if (item2.IsAsync)
					{
						instance.Add(ErrorCode.ERR_NonTaskMainCantBeAsync, item2.GetFirstLocation());
					}
					else
					{
						instance.AddRange(instance5);
						instance4.Add(item2);
					}
				}
				instance5.Free();
			}
			if (instance4.Count == 0)
			{
				foreach (var (isCandidate, methodSymbol, bindingDiagnosticBag) in instance3)
				{
					if (checkValid(methodSymbol, isCandidate, bindingDiagnosticBag) && Binder.CheckFeatureAvailability(methodSymbol.ExtractReturnTypeSyntax(), MessageID.IDS_FeatureAsyncMain, instance))
					{
						instance.AddRange(bindingDiagnosticBag);
						instance4.Add(methodSymbol);
					}
				}
			}
			else if (LanguageVersion >= MessageID.IDS_FeatureAsyncMain.RequiredVersion() && instance3.Count > 0)
			{
				ImmutableArray<Symbol> immutableArray = instance3.SelectAsArray((Func<(bool, Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol, BindingDiagnosticBag), Symbol>)(((bool IsValid, Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol Candidate, BindingDiagnosticBag SpecificDiagnostics) s) => s.Candidate));
				ImmutableArray<Location> additionalLocations = immutableArray.SelectAsArray((Symbol s) => s.GetFirstLocation());
				foreach (Symbol item3 in immutableArray)
				{
					CSDiagnosticInfo info = new CSDiagnosticInfo(ErrorCode.WRN_SyncAndAsyncEntryPoints, new object[2]
					{
						item3,
						instance4[0]
					}, immutableArray, additionalLocations);
					instance.Add(new CSDiagnostic(info, item3.GetFirstLocation()));
				}
			}
			foreach (var item4 in instance3)
			{
				item4.Item3.Free();
			}
			if (instance4.Count == 0)
			{
				instance.AddRange(noMainFoundDiagnostics);
			}
			else if ((object)namedTypeSymbol == null)
			{
				foreach (Diagnostic item5 in noMainFoundDiagnostics.DiagnosticBag.AsEnumerable())
				{
					if (item5.Code == 28 || item5.Code == 402)
					{
						instance.Add(item5);
					}
				}
				instance.AddDependencies(noMainFoundDiagnostics);
			}
			Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol result = null;
			if (instance4.Count == 0)
			{
				if ((object)namedTypeSymbol == null)
				{
					instance.Add(ErrorCode.ERR_NoEntryPoint, NoLocation.Singleton);
				}
				else
				{
					instance.Add(ErrorCode.ERR_NoMainInClass, namedTypeSymbol.GetFirstLocation(), namedTypeSymbol);
				}
			}
			else
			{
				foreach (Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol item6 in instance4)
				{
					if (item6.GetUnmanagedCallersOnlyAttributeData(forceComplete: true) != null)
					{
						instance.Add(ErrorCode.ERR_EntryPointCannotBeUnmanagedCallersOnly, item6.GetFirstLocation());
					}
				}
				if (instance4.Count > 1)
				{
					instance4.Sort(LexicalOrderSymbolComparer.Instance);
					CSDiagnosticInfo info2 = new CSDiagnosticInfo(ErrorCode.ERR_MultipleEntryPoints, Array.Empty<object>(), instance4.OfType<Symbol>().AsImmutable(), instance4.Select((Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol m) => m.GetFirstLocation()).OfType<Location>().AsImmutable());
					instance.Add(new CSDiagnostic(info2, instance4.First().GetFirstLocation()));
				}
				else
				{
					result = instance4[0];
				}
			}
			instance3.Free();
			instance4.Free();
			noMainFoundDiagnostics.Free();
			return result;
			bool checkValid(Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol candidate, bool flag2, BindingDiagnosticBag specificDiagnostics)
			{
				if (!flag2)
				{
					noMainFoundDiagnostics.Add(ErrorCode.WRN_InvalidMainSig, candidate.GetFirstLocation(), candidate);
					noMainFoundDiagnostics.AddRange(specificDiagnostics);
					return false;
				}
				if (candidate.IsGenericMethod || candidate.ContainingType.IsGenericType)
				{
					noMainFoundDiagnostics.Add(ErrorCode.WRN_MainCantBeGeneric, candidate.GetFirstLocation(), candidate);
					return false;
				}
				return true;
			}
		}
		finally
		{
			instance2.Free();
			sealedDiagnostics = instance.ToReadOnlyAndFree();
		}
	}

	private static void AddEntryPointCandidates(ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol> entryPointCandidates, IEnumerable<Symbol> members)
	{
		foreach (Symbol member in members)
		{
			if (member.IsExtensionBlockMember())
			{
				if (member is Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol methodSymbol)
				{
					Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol methodSymbol2 = methodSymbol.TryGetCorrespondingExtensionImplementationMethod();
					if ((object)methodSymbol2 != null)
					{
						addIfCandidate(entryPointCandidates, methodSymbol2);
					}
				}
			}
			else
			{
				addIfCandidate(entryPointCandidates, member);
			}
		}
		static void addIfCandidate(ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol> arrayBuilder, Symbol member)
		{
			if (member is Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol { IsEntryPointCandidate: not false } methodSymbol3)
			{
				arrayBuilder.Add(methodSymbol3);
			}
		}
	}

	internal bool ReturnsAwaitableToVoidOrInt(Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol method, BindingDiagnosticBag diagnostics)
	{
		if (method.ReturnType.IsVoidType() || method.ReturnType.SpecialType == SpecialType.System_Int32)
		{
			return false;
		}
		if (!(method.ReturnType is Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol namedTypeSymbol))
		{
			return false;
		}
		if (!Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol.Equals(namedTypeSymbol.ConstructedFrom, GetWellKnownType(WellKnownType.System_Threading_Tasks_Task), TypeCompareKind.ConsiderEverything) && !Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol.Equals(namedTypeSymbol.ConstructedFrom, GetWellKnownType(WellKnownType.System_Threading_Tasks_Task_T), TypeCompareKind.ConsiderEverything))
		{
			return false;
		}
		CSharpSyntaxNode cSharpSyntaxNode = method.ExtractReturnTypeSyntax();
		BoundLiteral expression = new BoundLiteral(cSharpSyntaxNode, ConstantValue.Null, namedTypeSymbol);
		if (!GetBinder(cSharpSyntaxNode).GetAwaitableExpressionInfo(expression, out BoundExpression getAwaiterGetResultCall, out BoundCall runtimeAsyncAwaitCall, cSharpSyntaxNode, diagnostics))
		{
			return false;
		}
		Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol typeSymbol = getAwaiterGetResultCall?.Type ?? runtimeAsyncAwaitCall.Type;
		if (!typeSymbol.IsVoidType())
		{
			return typeSymbol.SpecialType == SpecialType.System_Int32;
		}
		return true;
	}

	internal (bool IsCandidate, bool IsTaskLike) HasEntryPointSignature(Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol method, BindingDiagnosticBag bag)
	{
		if (method.IsVararg)
		{
			return (IsCandidate: false, IsTaskLike: false);
		}
		Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol returnType = method.ReturnType;
		bool flag = false;
		if (returnType.SpecialType != SpecialType.System_Int32 && !returnType.IsVoidType())
		{
			flag = ReturnsAwaitableToVoidOrInt(method, bag);
			if (!flag)
			{
				return (IsCandidate: false, IsTaskLike: false);
			}
		}
		if (method.RefKind != RefKind.None)
		{
			return (IsCandidate: false, IsTaskLike: flag);
		}
		if (method.Parameters.Length == 0)
		{
			return (IsCandidate: true, IsTaskLike: flag);
		}
		if (method.Parameters.Length > 1)
		{
			return (IsCandidate: false, IsTaskLike: flag);
		}
		if (!method.ParameterRefKinds.IsDefault)
		{
			return (IsCandidate: false, IsTaskLike: flag);
		}
		TypeWithAnnotations typeWithAnnotations = method.Parameters[0].TypeWithAnnotations;
		if (typeWithAnnotations.TypeKind != TypeKind.Array)
		{
			return (IsCandidate: false, IsTaskLike: flag);
		}
		Microsoft.CodeAnalysis.CSharp.Symbols.ArrayTypeSymbol arrayTypeSymbol = (Microsoft.CodeAnalysis.CSharp.Symbols.ArrayTypeSymbol)typeWithAnnotations.Type;
		return (IsCandidate: arrayTypeSymbol.IsSZArray && arrayTypeSymbol.ElementType.SpecialType == SpecialType.System_String, IsTaskLike: flag);
	}

	internal override bool IsUnreferencedAssemblyIdentityDiagnosticCode(int code)
	{
		return code == 12;
	}

	internal bool MightContainNoPiaLocalTypes()
	{
		return SourceAssembly.MightContainNoPiaLocalTypes();
	}

	public Conversion ClassifyConversion(ITypeSymbol source, ITypeSymbol destination)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol source2 = source.EnsureCSharpSymbolOrNull("source");
		Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol destination2 = destination.EnsureCSharpSymbolOrNull("destination");
		CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.Discarded;
		return Conversions.ClassifyConversionFromType(source2, destination2, isChecked: false, ref useSiteInfo);
	}

	public override CommonConversion ClassifyCommonConversion(ITypeSymbol source, ITypeSymbol destination)
	{
		return ClassifyConversion(source, destination).ToCommonConversion();
	}

	internal override IConvertibleConversion ClassifyConvertibleConversion(IOperation source, ITypeSymbol? destination, out ConstantValue? constantValue)
	{
		constantValue = null;
		if (destination == null)
		{
			return Conversion.NoConversion;
		}
		ITypeSymbol type = source.Type;
		ConstantValue constantValue2 = source.GetConstantValue();
		if (type == null)
		{
			if ((object)constantValue2 != null && constantValue2.IsNull && destination.IsReferenceType)
			{
				constantValue = constantValue2;
				return Conversion.NullLiteral;
			}
			return Conversion.NoConversion;
		}
		Conversion conversion = ClassifyConversion(type, destination);
		if (conversion.IsReference && (object)constantValue2 != null && constantValue2.IsNull)
		{
			constantValue = constantValue2;
		}
		return conversion;
	}

	internal Microsoft.CodeAnalysis.CSharp.Symbols.ArrayTypeSymbol CreateArrayTypeSymbol(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol elementType, int rank = 1, NullableAnnotation elementNullableAnnotation = NullableAnnotation.Oblivious)
	{
		if ((object)elementType == null)
		{
			throw new ArgumentNullException("elementType");
		}
		if (rank < 1)
		{
			throw new ArgumentException("rank");
		}
		return Microsoft.CodeAnalysis.CSharp.Symbols.ArrayTypeSymbol.CreateCSharpArray(Assembly, TypeWithAnnotations.Create(elementType, elementNullableAnnotation), rank);
	}

	internal Microsoft.CodeAnalysis.CSharp.Symbols.PointerTypeSymbol CreatePointerTypeSymbol(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol elementType, NullableAnnotation elementNullableAnnotation = NullableAnnotation.Oblivious)
	{
		if ((object)elementType == null)
		{
			throw new ArgumentNullException("elementType");
		}
		return new Microsoft.CodeAnalysis.CSharp.Symbols.PointerTypeSymbol(TypeWithAnnotations.Create(elementType, elementNullableAnnotation));
	}

	private protected override bool IsSymbolAccessibleWithinCore(ISymbol symbol, ISymbol within, ITypeSymbol? throughType)
	{
		Symbol symbol2 = symbol.EnsureCSharpSymbolOrNull("symbol");
		Symbol symbol3 = within.EnsureCSharpSymbolOrNull("within");
		Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol throughTypeOpt = throughType.EnsureCSharpSymbolOrNull("throughType");
		CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.Discarded;
		if (symbol3.Kind != SymbolKind.Assembly)
		{
			return AccessCheck.IsSymbolAccessible(symbol2, (Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol)symbol3, ref useSiteInfo, throughTypeOpt);
		}
		return AccessCheck.IsSymbolAccessible(symbol2, (Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol)symbol3, ref useSiteInfo);
	}

	[Obsolete("Compilation.IsSymbolAccessibleWithin is not designed for use within the compilers", true)]
	internal new bool IsSymbolAccessibleWithin(ISymbol symbol, ISymbol within, ITypeSymbol? throughType = null)
	{
		throw new NotImplementedException();
	}

	internal void AddModuleInitializerMethod(Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol method)
	{
		LazyInitializer.EnsureInitialized(ref _moduleInitializerMethods).Add(method);
	}

	internal void AddInterception(ImmutableArray<byte> contentHash, int position, Location attributeLocation, Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol interceptor)
	{
		LazyInitializer.EnsureInitialized(ref _interceptions, () => new ConcurrentDictionary<(ImmutableArray<byte>, int), OneOrMany<(Location, Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol)>>(InterceptorKeyComparer.Instance)).AddOrUpdate<(ImmutableArray<byte>, int), OneOrMany<(Location, Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol)>, (Location, Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol)>((contentHash, position), ((ImmutableArray<byte>, int) key, (Location AttributeLocation, Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol Interceptor) newValue) => OneOrMany.Create(newValue), delegate((ImmutableArray<byte>, int) key, OneOrMany<(Location AttributeLocation, Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol Interceptor)> existingValues, (Location AttributeLocation, Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol Interceptor) newValue)
		{
			foreach (var (location, methodSymbol) in existingValues)
			{
				if (location == newValue.AttributeLocation && methodSymbol.Equals(newValue.Interceptor, TypeCompareKind.ConsiderEverything))
				{
					return existingValues;
				}
			}
			return existingValues.Add(newValue);
		}, (attributeLocation, interceptor));
	}

	internal (Location AttributeLocation, Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol Interceptor)? TryGetInterceptor(SimpleNameSyntax? node)
	{
		if (node == null)
		{
			return null;
		}
		((SourceModuleSymbol)SourceModule).DiscoverInterceptorsIfNeeded();
		if (_interceptions == null)
		{
			return null;
		}
		(ImmutableArray<byte>, int) key = (node.SyntaxTree.GetText().GetContentHash(), node.Position);
		if (_interceptions.TryGetValue(key, out OneOrMany<(Location, Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol)> value) && value.Count == 1)
		{
			return value[0];
		}
		return null;
	}

	public new SemanticModel GetSemanticModel(SyntaxTree syntaxTree, bool ignoreAccessibility)
	{
		return GetSemanticModel(syntaxTree, ignoreAccessibility ? SemanticModelOptions.IgnoreAccessibility : SemanticModelOptions.None);
	}

	[Experimental("RSEXPERIMENTAL001", UrlFormat = "https://github.com/dotnet/roslyn/issues/70609")]
	public new SemanticModel GetSemanticModel(SyntaxTree syntaxTree, SemanticModelOptions options)
	{
		if (syntaxTree == null)
		{
			throw new ArgumentNullException("syntaxTree");
		}
		if (!_syntaxAndDeclarations.GetLazyState().RootNamespaces.ContainsKey(syntaxTree))
		{
			throw new ArgumentException(CSharpResources.SyntaxTreeNotFound, "syntaxTree");
		}
		SemanticModel semanticModel = null;
		if (base.SemanticModelProvider != null)
		{
			semanticModel = base.SemanticModelProvider.GetSemanticModel(syntaxTree, this, options);
		}
		return semanticModel ?? CreateSemanticModel(syntaxTree, options);
	}

	internal override SemanticModel CreateSemanticModel(SyntaxTree syntaxTree, SemanticModelOptions options)
	{
		return new SyntaxTreeSemanticModel(this, syntaxTree, options);
	}

	internal BinderFactory GetBinderFactory(SyntaxTree syntaxTree, bool ignoreAccessibility = false)
	{
		if (ignoreAccessibility && (object)SynthesizedSimpleProgramEntryPointSymbol.GetSimpleProgramEntryPoint(this) != null)
		{
			return GetBinderFactory(syntaxTree, ignoreAccessibility: true, ref _ignoreAccessibilityBinderFactories);
		}
		return GetBinderFactory(syntaxTree, ignoreAccessibility: false, ref _binderFactories);
	}

	private BinderFactory GetBinderFactory(SyntaxTree syntaxTree, bool ignoreAccessibility, ref WeakReference<BinderFactory>[]? cachedBinderFactories)
	{
		int syntaxTreeOrdinal = GetSyntaxTreeOrdinal(syntaxTree);
		WeakReference<BinderFactory>[] array = cachedBinderFactories;
		if (array == null)
		{
			array = new WeakReference<BinderFactory>[SyntaxTrees.Length];
			array = Interlocked.CompareExchange(ref cachedBinderFactories, array, null) ?? array;
		}
		WeakReference<BinderFactory> weakReference = array[syntaxTreeOrdinal];
		if (weakReference != null && weakReference.TryGetTarget(out var target))
		{
			return target;
		}
		return AddNewFactory(syntaxTree, ignoreAccessibility, ref array[syntaxTreeOrdinal]);
	}

	private BinderFactory AddNewFactory(SyntaxTree syntaxTree, bool ignoreAccessibility, [NotNull] ref WeakReference<BinderFactory>? slot)
	{
		BinderFactory binderFactory = new BinderFactory(this, syntaxTree, ignoreAccessibility);
		WeakReference<BinderFactory> value = new WeakReference<BinderFactory>(binderFactory);
		WeakReference<BinderFactory> weakReference;
		do
		{
			weakReference = slot;
			if (weakReference != null && weakReference.TryGetTarget(out var target))
			{
				return target;
			}
		}
		while (Interlocked.CompareExchange(ref slot, value, weakReference) != weakReference);
		return binderFactory;
	}

	internal Binder GetBinder(CSharpSyntaxNode syntax)
	{
		return GetBinderFactory(syntax.SyntaxTree).GetBinder(syntax);
	}

	private Microsoft.CodeAnalysis.CSharp.Symbols.AliasSymbol CreateGlobalNamespaceAlias()
	{
		return Microsoft.CodeAnalysis.CSharp.Symbols.AliasSymbol.CreateGlobalNamespaceAlias(GlobalNamespace);
	}

	private void CompleteTree(SyntaxTree tree)
	{
		if (_lazyCompilationUnitCompletedTrees == null)
		{
			Interlocked.CompareExchange(ref _lazyCompilationUnitCompletedTrees, new HashSet<SyntaxTree>(), null);
		}
		lock (_lazyCompilationUnitCompletedTrees)
		{
			if (_lazyCompilationUnitCompletedTrees.Add(tree))
			{
				base.EventQueue?.TryEnqueue(new CompilationUnitCompletedEvent(this, tree));
				if (_lazyCompilationUnitCompletedTrees.Count == SyntaxTrees.Length)
				{
					CompleteCompilationEventQueue_NoLock();
				}
			}
		}
	}

	internal override void ReportUnusedImports(DiagnosticBag diagnostics, CancellationToken cancellationToken)
	{
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: true, withDependencies: false);
		ReportUnusedImports(null, instance, cancellationToken);
		diagnostics.AddRange(instance.DiagnosticBag);
		instance.Free();
	}

	private void ReportUnusedImports(SyntaxTree? filterTree, BindingDiagnosticBag diagnostics, CancellationToken cancellationToken)
	{
		if (_lazyImportInfos != null && (filterTree == null || Compilation.ReportUnusedImportsInTree(filterTree)))
		{
			PooledHashSet<Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol> pooledHashSet = null;
			if (diagnostics.DependenciesBag != null)
			{
				pooledHashSet = PooledHashSet<Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol>.GetInstance();
			}
			foreach (KeyValuePair<ImportInfo, ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>> lazyImportInfo in _lazyImportInfos)
			{
				cancellationToken.ThrowIfCancellationRequested();
				ImportInfo key = lazyImportInfo.Key;
				SyntaxTree tree = key.Tree;
				if ((filterTree != null && filterTree != tree) || !Compilation.ReportUnusedImportsInTree(tree))
				{
					continue;
				}
				TextSpan span = key.Span;
				if (!IsImportDirectiveUsed(tree, span.Start))
				{
					ErrorCode code = ((key.Kind == SyntaxKind.ExternAliasDirective) ? ErrorCode.HDN_UnusedExternAlias : ErrorCode.HDN_UnusedUsingDirective);
					diagnostics.Add(code, tree.GetLocation(span));
				}
				else
				{
					if (diagnostics.DependenciesBag == null)
					{
						continue;
					}
					ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> value = lazyImportInfo.Value;
					if (!value.IsDefaultOrEmpty)
					{
						diagnostics.AddDependencies(value);
					}
					else if (key.Kind == SyntaxKind.ExternAliasDirective)
					{
						ExternAliasDirectiveSyntax externAliasDirectiveSyntax = key.Tree.GetRoot(cancellationToken).FindToken(key.Span.Start).Parent.FirstAncestorOrSelf<ExternAliasDirectiveSyntax>();
						if (externAliasDirectiveSyntax != null && GetExternAliasTarget(externAliasDirectiveSyntax.Identifier.ValueText, out Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol @namespace))
						{
							pooledHashSet.Add(@namespace);
						}
					}
				}
			}
			if (pooledHashSet != null)
			{
				BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: false, withDependencies: true);
				foreach (Microsoft.CodeAnalysis.CSharp.Symbols.NamespaceSymbol item in pooledHashSet)
				{
					instance.Clear();
					instance.AddAssembliesUsedByNamespaceReference(item);
					ConcurrentSet<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>? lazyUsedAssemblyReferences = _lazyUsedAssemblyReferences;
					if ((lazyUsedAssemblyReferences != null && !lazyUsedAssemblyReferences.IsEmpty) || diagnostics.DependenciesBag.Count != 0)
					{
						foreach (Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol item2 in instance.DependenciesBag)
						{
							ConcurrentSet<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>? lazyUsedAssemblyReferences2 = _lazyUsedAssemblyReferences;
							if ((lazyUsedAssemblyReferences2 != null && lazyUsedAssemblyReferences2.Contains(item2)) || diagnostics.DependenciesBag.Contains(item2))
							{
								instance.DependenciesBag.Clear();
								break;
							}
						}
					}
					diagnostics.AddDependencies(instance);
				}
				instance.Free();
				pooledHashSet.Free();
			}
		}
		CompleteTrees(filterTree);
	}

	internal override void CompleteTrees(SyntaxTree? filterTree)
	{
		if (base.EventQueue != null)
		{
			if (filterTree != null)
			{
				CompleteTree(filterTree);
			}
			else
			{
				foreach (SyntaxTree syntaxTree in SyntaxTrees)
				{
					CompleteTree(syntaxTree);
				}
			}
		}
		if (filterTree == null)
		{
			_usageOfUsingsRecordedInTrees = null;
		}
	}

	internal void RecordImport(UsingDirectiveSyntax syntax)
	{
		RecordImportInternal(syntax);
	}

	internal void RecordImport(ExternAliasDirectiveSyntax syntax)
	{
		RecordImportInternal(syntax);
	}

	private void RecordImportInternal(CSharpSyntaxNode syntax)
	{
		LazyInitializer.EnsureInitialized(ref _lazyImportInfos).TryAdd(new ImportInfo(syntax.SyntaxTree, syntax.Kind(), syntax.Span), default(ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>));
	}

	internal void RecordImportDependencies(UsingDirectiveSyntax syntax, ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> dependencies)
	{
		_lazyImportInfos.TryUpdate(new ImportInfo(syntax.SyntaxTree, syntax.Kind(), syntax.Span), dependencies, default(ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>));
	}

	public override ImmutableArray<Diagnostic> GetParseDiagnostics(CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDiagnostics(CompilationStage.Parse, includeEarlierStages: false, null, cancellationToken);
	}

	public override ImmutableArray<Diagnostic> GetDeclarationDiagnostics(CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDiagnostics(CompilationStage.Declare, includeEarlierStages: false, null, cancellationToken);
	}

	public override ImmutableArray<Diagnostic> GetMethodBodyDiagnostics(CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDiagnostics(CompilationStage.Compile, includeEarlierStages: false, null, cancellationToken);
	}

	public override ImmutableArray<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDiagnostics(CompilationStage.Compile, includeEarlierStages: true, null, cancellationToken);
	}

	internal ImmutableArray<Diagnostic> GetDiagnostics(CompilationStage stage, bool includeEarlierStages, Predicate<ISymbolInternal>? symbolFilter, CancellationToken cancellationToken)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		GetDiagnostics(stage, includeEarlierStages, instance, symbolFilter, cancellationToken);
		return instance.ToReadOnlyAndFree();
	}

	internal override void GetDiagnostics(CompilationStage stage, bool includeEarlierStages, DiagnosticBag diagnostics, CancellationToken cancellationToken = default(CancellationToken))
	{
		GetDiagnostics(stage, includeEarlierStages, diagnostics, null, cancellationToken);
	}

	internal void GetDiagnostics(CompilationStage stage, bool includeEarlierStages, DiagnosticBag diagnostics, Predicate<ISymbolInternal>? symbolFilter, CancellationToken cancellationToken)
	{
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: true, withDependencies: false);
		GetDiagnosticsWithoutSeverityFiltering(stage, includeEarlierStages, instance, symbolFilter, cancellationToken);
		FilterAndAppendDiagnostics(diagnostics, instance.DiagnosticBag, cancellationToken);
		instance.Free();
	}

	private void GetDiagnosticsWithoutSeverityFiltering(CompilationStage stage, bool includeEarlierStages, BindingDiagnosticBag builder, Predicate<Symbol>? symbolFilter, CancellationToken cancellationToken)
	{
		if (stage == CompilationStage.Parse || ((stage > CompilationStage.Parse) & includeEarlierStages))
		{
			ImmutableArray<SyntaxTree> syntaxTrees = SyntaxTrees;
			if (Options.ConcurrentBuild)
			{
				RoslynParallel.For(0, syntaxTrees.Length, UICultureUtilities.WithCurrentUICulture(delegate(int i)
				{
					SyntaxTree syntaxTree = syntaxTrees[i];
					AppendLoadDirectiveDiagnostics(builder.DiagnosticBag, _syntaxAndDeclarations, syntaxTree);
					builder.AddRange(syntaxTree.GetDiagnostics(cancellationToken));
				}), cancellationToken);
			}
			else
			{
				foreach (SyntaxTree item in syntaxTrees)
				{
					cancellationToken.ThrowIfCancellationRequested();
					AppendLoadDirectiveDiagnostics(builder.DiagnosticBag, _syntaxAndDeclarations, item);
					cancellationToken.ThrowIfCancellationRequested();
					builder.AddRange(item.GetDiagnostics(cancellationToken));
				}
			}
			HashSet<ParseOptions> hashSet = new HashSet<ParseOptions>();
			foreach (SyntaxTree item2 in syntaxTrees)
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (!item2.Options.Errors.IsDefaultOrEmpty && hashSet.Add(item2.Options))
				{
					Location location = item2.GetLocation(TextSpan.FromBounds(0, 0));
					foreach (Diagnostic error in item2.Options.Errors)
					{
						builder.Add(error.WithLocation(location));
					}
				}
			}
		}
		if (stage == CompilationStage.Declare || ((stage > CompilationStage.Declare) & includeEarlierStages))
		{
			CheckAssemblyName(builder.DiagnosticBag);
			builder.AddRange(Options.Errors);
			if (Options.NullableContextOptions != NullableContextOptions.Disable && LanguageVersion < MessageID.IDS_FeatureNullableReferenceTypes.RequiredVersion() && _syntaxAndDeclarations.ExternalSyntaxTrees.Any())
			{
				builder.Add(new CSDiagnostic(new CSDiagnosticInfo(ErrorCode.ERR_NullableOptionNotAvailable, "NullableContextOptions", Options.NullableContextOptions, LanguageVersion.ToDisplayString(), new CSharpRequiredLanguageVersion(MessageID.IDS_FeatureNullableReferenceTypes.RequiredVersion())), Location.None));
			}
			cancellationToken.ThrowIfCancellationRequested();
			builder.AddRange(GetBoundReferenceManager().Diagnostics);
			cancellationToken.ThrowIfCancellationRequested();
			BindingDiagnosticBag bindingDiagnosticBag = builder;
			CancellationToken cancellationToken2 = cancellationToken;
			bindingDiagnosticBag.AddRange(GetSourceDeclarationDiagnostics(null, null, null, symbolFilter, cancellationToken2), allowMismatchInDependencyAccumulation: true);
			if (base.EventQueue != null && SyntaxTrees.Length == 0)
			{
				EnsureCompilationEventQueueCompleted();
			}
		}
		cancellationToken.ThrowIfCancellationRequested();
		if (stage == CompilationStage.Compile || ((stage > CompilationStage.Compile) & includeEarlierStages))
		{
			BindingDiagnosticBag bindingDiagnosticBag2 = (builder.AccumulatesDependencies ? BindingDiagnosticBag.GetConcurrentInstance() : BindingDiagnosticBag.GetInstance(withDiagnostics: true, withDependencies: false));
			GetDiagnosticsForAllMethodBodies(bindingDiagnosticBag2, doLowering: false, cancellationToken);
			builder.AddRangeAndFree(bindingDiagnosticBag2);
		}
	}

	private static void AppendLoadDirectiveDiagnostics(DiagnosticBag builder, SyntaxAndDeclarationManager syntaxAndDeclarations, SyntaxTree syntaxTree, Func<IEnumerable<Diagnostic>, IEnumerable<Diagnostic>>? locationFilterOpt = null)
	{
		if (!syntaxAndDeclarations.GetLazyState().LoadDirectiveMap.TryGetValue(syntaxTree, out var value))
		{
			return;
		}
		foreach (LoadDirective item in value)
		{
			IEnumerable<Diagnostic> enumerable = item.Diagnostics;
			if (locationFilterOpt != null)
			{
				enumerable = locationFilterOpt(enumerable);
			}
			builder.AddRange(enumerable);
		}
	}

	private void GetDiagnosticsForAllMethodBodies(BindingDiagnosticBag diagnostics, bool doLowering, CancellationToken cancellationToken)
	{
		MethodCompiler.CompileMethodBodies(this, doLowering ? ((PEModuleBuilder)CreateModuleBuilder(EmitOptions.Default, null, null, null, null, null, diagnostics.DiagnosticBag, cancellationToken)) : null, emittingPdb: false, hasDeclarationErrors: false, emitMethodBodies: false, diagnostics, null, cancellationToken);
		DocumentationCommentCompiler.WriteDocumentationCommentXml(this, null, null, diagnostics, cancellationToken);
		ReportUnusedImports(null, diagnostics, cancellationToken);
	}

	private static bool IsDefinedOrImplementedInSourceTree(Symbol symbol, SyntaxTree tree, TextSpan? span)
	{
		if (symbol.IsDefinedInSourceTree(tree, span))
		{
			return true;
		}
		if (symbol.Kind == SymbolKind.Method && symbol.IsImplicitlyDeclared && ((Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol)symbol).MethodKind == MethodKind.Constructor)
		{
			return IsDefinedOrImplementedInSourceTree(symbol.ContainingType, tree, span);
		}
		return false;
	}

	private ImmutableArray<Diagnostic> GetDiagnosticsForMethodBodiesInTree(SyntaxTree tree, TextSpan? span, CancellationToken cancellationToken)
	{
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: true, withDependencies: false);
		bool flag = (!span.HasValue || span.Value == tree.GetRoot(cancellationToken).FullSpan) && Compilation.ReportUnusedImportsInTree(tree);
		bool flag2 = false;
		if (flag && UsageOfUsingsRecordedInTrees != null)
		{
			foreach (SingleNamespaceDeclaration declaration in ((SourceNamespaceSymbol)SourceModule.GlobalNamespace).MergedDeclaration.Declarations)
			{
				if (declaration.SyntaxReference.SyntaxTree == tree)
				{
					if (declaration.HasGlobalUsings)
					{
						flag2 = true;
					}
					break;
				}
			}
		}
		if (flag2)
		{
			ImmutableHashSet<SyntaxTree>? usageOfUsingsRecordedInTrees = UsageOfUsingsRecordedInTrees;
			if (usageOfUsingsRecordedInTrees != null && usageOfUsingsRecordedInTrees.IsEmpty)
			{
				compileMethodBodiesAndDocComments(null, null, instance, cancellationToken);
				_usageOfUsingsRecordedInTrees = null;
				goto IL_0158;
			}
		}
		compileMethodBodiesAndDocComments(tree, span, instance, cancellationToken);
		if (flag)
		{
			registeredUsageOfUsingsInTree(tree);
		}
		if (flag2)
		{
			BindingDiagnosticBag instance2 = BindingDiagnosticBag.GetInstance(withDiagnostics: true, withDependencies: false);
			foreach (SyntaxTree syntaxTree in SyntaxTrees)
			{
				ImmutableHashSet<SyntaxTree> usageOfUsingsRecordedInTrees2 = UsageOfUsingsRecordedInTrees;
				if (usageOfUsingsRecordedInTrees2 == null)
				{
					break;
				}
				if (!usageOfUsingsRecordedInTrees2.Contains(syntaxTree))
				{
					compileMethodBodiesAndDocComments(syntaxTree, null, instance2, cancellationToken);
					registeredUsageOfUsingsInTree(syntaxTree);
					instance2.DiagnosticBag.Clear();
				}
			}
			instance2.Free();
		}
		goto IL_0158;
		IL_0158:
		if (flag)
		{
			ReportUnusedImports(tree, instance, cancellationToken);
		}
		return instance.ToReadOnlyAndFree().Diagnostics;
		void compileMethodBodiesAndDocComments(SyntaxTree? filterTree, TextSpan? filterSpan, BindingDiagnosticBag bindingDiagnostics, CancellationToken cancellationToken2)
		{
			MethodCompiler.CompileMethodBodies(this, null, emittingPdb: false, hasDeclarationErrors: false, emitMethodBodies: false, bindingDiagnostics, (filterTree != null) ? ((Predicate<Symbol>)((Symbol s) => IsDefinedOrImplementedInSourceTree(s, filterTree, filterSpan))) : null, cancellationToken2);
			DocumentationCommentCompiler.WriteDocumentationCommentXml(this, null, null, bindingDiagnostics, cancellationToken2, filterTree, filterSpan);
		}
		void registeredUsageOfUsingsInTree(SyntaxTree item)
		{
			ImmutableHashSet<SyntaxTree> immutableHashSet = UsageOfUsingsRecordedInTrees;
			while (immutableHashSet != null)
			{
				ImmutableHashSet<SyntaxTree> immutableHashSet2 = immutableHashSet.Add(item);
				if (immutableHashSet2 == immutableHashSet)
				{
					break;
				}
				if (immutableHashSet2.Count == SyntaxTrees.Length)
				{
					_usageOfUsingsRecordedInTrees = null;
					break;
				}
				ImmutableHashSet<SyntaxTree> immutableHashSet3 = Interlocked.CompareExchange(ref _usageOfUsingsRecordedInTrees, immutableHashSet2, immutableHashSet);
				if (immutableHashSet3 == immutableHashSet)
				{
					break;
				}
				immutableHashSet = immutableHashSet3;
			}
		}
	}

	private ReadOnlyBindingDiagnostic<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> GetSourceDeclarationDiagnostics(SyntaxTree? syntaxTree = null, TextSpan? filterSpanWithinTree = null, Func<IEnumerable<Diagnostic>, SyntaxTree, TextSpan?, IEnumerable<Diagnostic>>? locationFilterOpt = null, Predicate<Symbol>? symbolFilter = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		UsingsFromOptions.Complete(this, cancellationToken);
		SourceLocation locationOpt = null;
		if (syntaxTree != null)
		{
			SyntaxNode root = syntaxTree.GetRoot(cancellationToken);
			locationOpt = (filterSpanWithinTree.HasValue ? new SourceLocation(syntaxTree, filterSpanWithinTree.Value) : new SourceLocation(root));
		}
		Assembly.ForceComplete(locationOpt, symbolFilter, cancellationToken);
		if (syntaxTree == null && symbolFilter == null)
		{
			_declarationDiagnosticsFrozen = true;
			_needsGeneratedAttributes_IsFrozen = true;
		}
		IEnumerable<Diagnostic> enumerable = _lazyDeclarationDiagnostics?.AsEnumerable() ?? Enumerable.Empty<Diagnostic>();
		if (locationFilterOpt != null)
		{
			enumerable = locationFilterOpt(enumerable, syntaxTree, filterSpanWithinTree);
		}
		if (symbolFilter == null)
		{
			ReadOnlyBindingDiagnostic<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> clsComplianceDiagnostics = GetClsComplianceDiagnostics(syntaxTree, filterSpanWithinTree, cancellationToken);
			return new ReadOnlyBindingDiagnostic<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>(enumerable.AsImmutable().Concat(clsComplianceDiagnostics.Diagnostics), clsComplianceDiagnostics.Dependencies);
		}
		return new ReadOnlyBindingDiagnostic<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>(enumerable.AsImmutable(), ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.Empty);
	}

	private ReadOnlyBindingDiagnostic<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> GetClsComplianceDiagnostics(SyntaxTree? syntaxTree, TextSpan? filterSpanWithinTree, CancellationToken cancellationToken)
	{
		if (syntaxTree != null)
		{
			BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: true, withDependencies: false);
			ClsComplianceChecker.CheckCompliance(this, instance, cancellationToken, syntaxTree, filterSpanWithinTree);
			return instance.ToReadOnlyAndFree();
		}
		if (_lazyClsComplianceDiagnostics.IsDefault || _lazyClsComplianceDependencies.IsDefault)
		{
			BindingDiagnosticBag instance2 = BindingDiagnosticBag.GetInstance();
			ClsComplianceChecker.CheckCompliance(this, instance2, cancellationToken);
			ReadOnlyBindingDiagnostic<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> readOnlyBindingDiagnostic = instance2.ToReadOnlyAndFree();
			ImmutableInterlocked.InterlockedInitialize(ref _lazyClsComplianceDependencies, readOnlyBindingDiagnostic.Dependencies);
			ImmutableInterlocked.InterlockedInitialize(ref _lazyClsComplianceDiagnostics, readOnlyBindingDiagnostic.Diagnostics);
		}
		return new ReadOnlyBindingDiagnostic<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>(_lazyClsComplianceDiagnostics, _lazyClsComplianceDependencies);
	}

	private static IEnumerable<Diagnostic> FilterDiagnosticsByLocation(IEnumerable<Diagnostic> diagnostics, SyntaxTree tree, TextSpan? filterSpanWithinTree)
	{
		foreach (Diagnostic diagnostic in diagnostics)
		{
			if (diagnostic.HasIntersectingLocation(tree, filterSpanWithinTree))
			{
				yield return diagnostic;
			}
		}
	}

	internal ImmutableArray<Diagnostic> GetDiagnosticsForSyntaxTree(CompilationStage stage, SyntaxTree syntaxTree, TextSpan? filterSpanWithinTree, bool includeEarlierStages, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		DiagnosticBag incoming = DiagnosticBag.GetInstance();
		if (stage == CompilationStage.Parse || ((stage > CompilationStage.Parse) & includeEarlierStages))
		{
			AppendLoadDirectiveDiagnostics(incoming, _syntaxAndDeclarations, syntaxTree, (IEnumerable<Diagnostic> diagnostics3) => FilterDiagnosticsByLocation(diagnostics3, syntaxTree, filterSpanWithinTree));
			IEnumerable<Diagnostic> diagnostics = syntaxTree.GetDiagnostics(cancellationToken);
			diagnostics = FilterDiagnosticsByLocation(diagnostics, syntaxTree, filterSpanWithinTree);
			incoming.AddRange(diagnostics);
		}
		cancellationToken.ThrowIfCancellationRequested();
		if (stage == CompilationStage.Declare || ((stage > CompilationStage.Declare) & includeEarlierStages))
		{
			ReadOnlyBindingDiagnostic<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> sourceDeclarationDiagnostics = GetSourceDeclarationDiagnostics(syntaxTree, filterSpanWithinTree, FilterDiagnosticsByLocation, null, cancellationToken);
			incoming.AddRange(sourceDeclarationDiagnostics.Diagnostics);
		}
		cancellationToken.ThrowIfCancellationRequested();
		if (stage == CompilationStage.Compile || ((stage > CompilationStage.Compile) & includeEarlierStages))
		{
			IEnumerable<Diagnostic> diagnostics2 = GetDiagnosticsForMethodBodiesInTree(syntaxTree, filterSpanWithinTree, cancellationToken);
			diagnostics2 = FilterDiagnosticsByLocation(diagnostics2, syntaxTree, filterSpanWithinTree);
			incoming.AddRange(diagnostics2);
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		FilterAndAppendAndFreeDiagnostics(instance, ref incoming, cancellationToken);
		return instance.ToReadOnlyAndFree<Diagnostic>();
	}

	protected override void AppendDefaultVersionResource(Stream resourceStream)
	{
		Microsoft.CodeAnalysis.CSharp.Symbols.SourceAssemblySymbol sourceAssembly = SourceAssembly;
		string text = sourceAssembly.FileVersion ?? sourceAssembly.Identity.Version.ToString();
		Win32ResourceConversions.AppendVersionToResourceStream(resourceStream, !Options.OutputKind.IsApplication(), text, SourceModule.Name, SourceModule.Name, sourceAssembly.InformationalVersion ?? text, fileDescription: sourceAssembly.Title ?? " ", assemblyVersion: sourceAssembly.Identity.Version, legalCopyright: sourceAssembly.Copyright ?? " ", legalTrademarks: sourceAssembly.Trademark, productName: sourceAssembly.Product, comments: sourceAssembly.Description, companyName: sourceAssembly.Company);
	}

	internal override CommonPEModuleBuilder? CreateModuleBuilder(EmitOptions emitOptions, IMethodSymbol? debugEntryPoint, Stream? sourceLinkStream, IEnumerable<EmbeddedText>? embeddedTexts, IEnumerable<ResourceDescription>? manifestResources, CompilationTestData? testData, DiagnosticBag diagnostics, CancellationToken cancellationToken)
	{
		string runtimeMetadataVersion = GetRuntimeMetadataVersion(emitOptions, diagnostics);
		if (runtimeMetadataVersion == null)
		{
			return null;
		}
		ModulePropertiesForSerialization serializationProperties = ConstructModuleSerializationProperties(emitOptions, runtimeMetadataVersion);
		if (manifestResources == null)
		{
			manifestResources = SpecializedCollections.EmptyEnumerable<ResourceDescription>();
		}
		PEModuleBuilder pEModuleBuilder;
		if (_options.OutputKind.IsNetModule())
		{
			pEModuleBuilder = new PENetModuleBuilder((SourceModuleSymbol)SourceModule, emitOptions, serializationProperties, manifestResources);
		}
		else
		{
			OutputKind outputKind = (_options.OutputKind.IsValid() ? _options.OutputKind : OutputKind.DynamicallyLinkedLibrary);
			pEModuleBuilder = new PEAssemblyBuilder(SourceAssembly, emitOptions, outputKind, serializationProperties, manifestResources);
		}
		if (debugEntryPoint != null)
		{
			pEModuleBuilder.SetDebugEntryPoint(debugEntryPoint.GetSymbol(), diagnostics);
		}
		pEModuleBuilder.SourceLinkStreamOpt = sourceLinkStream;
		if (embeddedTexts != null)
		{
			pEModuleBuilder.EmbeddedTexts = embeddedTexts;
		}
		if (testData != null)
		{
			pEModuleBuilder.SetTestData(testData);
		}
		return pEModuleBuilder;
	}

	internal override bool CompileMethods(CommonPEModuleBuilder moduleBuilder, bool emittingPdb, DiagnosticBag diagnostics, Predicate<ISymbolInternal>? filterOpt, CancellationToken cancellationToken)
	{
		bool emitMetadataOnly = moduleBuilder.EmitOptions.EmitMetadataOnly;
		PooledHashSet<int> pooledHashSet = null;
		if (emitMetadataOnly)
		{
			pooledHashSet = PooledHashSet<int>.GetInstance();
			pooledHashSet.Add(501);
		}
		bool flag = !FilterAndAppendDiagnostics(diagnostics, GetDiagnostics(CompilationStage.Declare, includeEarlierStages: true, filterOpt, cancellationToken), pooledHashSet, cancellationToken);
		pooledHashSet?.Free();
		PEModuleBuilder pEModuleBuilder = (PEModuleBuilder)moduleBuilder;
		if (emitMetadataOnly)
		{
			if (flag)
			{
				return false;
			}
			if (pEModuleBuilder.SourceModule.HasBadAttributes)
			{
				diagnostics.Add(ErrorCode.ERR_ModuleEmitFailure, NoLocation.Singleton, ((INamedEntity)pEModuleBuilder).Name, new LocalizableResourceString("ModuleHasInvalidAttributes", CodeAnalysisResources.ResourceManager, typeof(CodeAnalysisResources)));
				return false;
			}
			SynthesizedMetadataCompiler.ProcessSynthesizedMembers(this, pEModuleBuilder, cancellationToken);
			if (pEModuleBuilder.OutputKind.IsApplication())
			{
				BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: true, withDependencies: false);
				Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol entryPoint = MethodCompiler.GetEntryPoint(this, pEModuleBuilder, hasDeclarationErrors: false, emitMethodBodies: false, instance, cancellationToken);
				diagnostics.AddRange(instance.DiagnosticBag);
				bool num = entryPoint != null && !instance.HasAnyErrors();
				instance.Free();
				if (!num)
				{
					return false;
				}
				pEModuleBuilder.SetPEEntryPoint(entryPoint, diagnostics);
			}
		}
		else
		{
			if ((emittingPdb || pEModuleBuilder.EmitOptions.InstrumentationKinds.Contains(InstrumentationKind.TestCoverage)) && !CreateDebugDocuments(pEModuleBuilder.DebugDocumentsBuilder, pEModuleBuilder.EmbeddedTexts, diagnostics))
			{
				return false;
			}
			BindingDiagnosticBag instance2 = BindingDiagnosticBag.GetInstance(withDiagnostics: true, withDependencies: false);
			MethodCompiler.CompileMethodBodies(this, pEModuleBuilder, emittingPdb, flag, emitMethodBodies: true, instance2, filterOpt, cancellationToken);
			if (!flag && !CommonCompiler.HasUnsuppressableErrors(instance2.DiagnosticBag) && filterOpt == null)
			{
				GenerateModuleInitializer(pEModuleBuilder, instance2.DiagnosticBag);
			}
			bool flag2 = CheckDuplicateFilePaths(diagnostics);
			bool flag3 = !FilterAndAppendDiagnostics(diagnostics, instance2.DiagnosticBag, cancellationToken);
			instance2.Free();
			if (flag | flag3 | flag2)
			{
				return false;
			}
		}
		return true;
	}

	private protected override SymbolMatcher CreatePreviousToCurrentSourceAssemblyMatcher(EmitBaseline previousGeneration, SynthesizedTypeMaps otherSynthesizedTypes, IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> otherSynthesizedMembers, IReadOnlyDictionary<ISymbolInternal, ImmutableArray<ISymbolInternal>> otherDeletedMembers)
	{
		return new CSharpSymbolMatcher(((CSharpCompilation)previousGeneration.Compilation).SourceAssembly, SourceAssembly, otherSynthesizedTypes, otherSynthesizedMembers, otherDeletedMembers);
	}

	private bool CheckDuplicateFilePaths(DiagnosticBag diagnostics)
	{
		return new DuplicateFilePathsVisitor(diagnostics).CheckDuplicateFilePathsAndFree(SyntaxTrees, GlobalNamespace);
	}

	internal bool CheckDuplicateInterceptions(BindingDiagnosticBag diagnostics)
	{
		if (_interceptions == null)
		{
			return false;
		}
		bool result = false;
		foreach (var (_, oneOrMany2) in _interceptions)
		{
			if (oneOrMany2.Count != 1)
			{
				result = true;
				foreach (var item2 in oneOrMany2)
				{
					Location item = item2.Item1;
					diagnostics.Add(ErrorCode.ERR_DuplicateInterceptor, item);
				}
			}
		}
		return result;
	}

	private void GenerateModuleInitializer(PEModuleBuilder moduleBeingBuilt, DiagnosticBag methodBodyDiagnosticBag)
	{
		if (_moduleInitializerMethods == null)
		{
			return;
		}
		ILBuilder iLBuilder = new ILBuilder(moduleBeingBuilt, new LocalSlotManager(null), methodBodyDiagnosticBag, OptimizationLevel.Release, areLocalsZeroed: false);
		foreach (Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol item in ((IEnumerable<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol>)_moduleInitializerMethods).OrderBy((IComparer<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol>?)LexicalOrderSymbolComparer.Instance))
		{
			iLBuilder.EmitOpCode(ILOpCode.Call, 0);
			iLBuilder.EmitToken((ISignature)moduleBeingBuilt.Translate(item, methodBodyDiagnosticBag, needDeclaration: true), CSharpSyntaxTree.Dummy.GetRoot());
		}
		iLBuilder.EmitRet(isVoid: true);
		iLBuilder.Realize();
		moduleBeingBuilt.RootModuleType.SetStaticConstructorBody(iLBuilder.RealizedIL);
	}

	internal override bool GenerateResources(CommonPEModuleBuilder moduleBuilder, Stream? win32Resources, bool useRawWin32Resources, DiagnosticBag diagnostics, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		DiagnosticBag incoming = DiagnosticBag.GetInstance();
		SetupWin32Resources(moduleBuilder, win32Resources, useRawWin32Resources, incoming);
		ReportManifestResourceDuplicates(moduleBuilder.ManifestResources, from m in SourceAssembly.Modules.Skip(1)
			select m.Name, AddedModulesResourceNames(incoming), incoming);
		return FilterAndAppendAndFreeDiagnostics(diagnostics, ref incoming, cancellationToken);
	}

	internal override bool GenerateDocumentationComments(Stream? xmlDocStream, string? outputNameOverride, DiagnosticBag diagnostics, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: true, withDependencies: false);
		string assemblyName = FileNameUtilities.ChangeExtension(outputNameOverride, null);
		DocumentationCommentCompiler.WriteDocumentationCommentXml(this, assemblyName, xmlDocStream, instance, cancellationToken);
		bool result = FilterAndAppendDiagnostics(diagnostics, instance.DiagnosticBag, cancellationToken);
		instance.Free();
		return result;
	}

	private IEnumerable<string> AddedModulesResourceNames(DiagnosticBag diagnostics)
	{
		ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.ModuleSymbol> modules = SourceAssembly.Modules;
		for (int i = 1; i < modules.Length; i++)
		{
			PEModuleSymbol pEModuleSymbol = (PEModuleSymbol)modules[i];
			ImmutableArray<EmbeddedResource> embeddedResourcesOrThrow;
			try
			{
				embeddedResourcesOrThrow = pEModuleSymbol.Module.GetEmbeddedResourcesOrThrow();
			}
			catch (BadImageFormatException)
			{
				diagnostics.Add(new CSDiagnosticInfo(ErrorCode.ERR_BindToBogus, pEModuleSymbol), NoLocation.Singleton);
				continue;
			}
			foreach (EmbeddedResource item in embeddedResourcesOrThrow)
			{
				yield return item.Name;
			}
		}
	}

	internal override EmitDifferenceResult EmitDifference(EmitBaseline baseline, IEnumerable<SemanticEdit> edits, Func<ISymbol, bool> isAddedSymbol, Stream metadataStream, Stream ilStream, Stream pdbStream, EmitDifferenceOptions options, CompilationTestData? testData, CancellationToken cancellationToken)
	{
		return EmitHelpers.EmitDifference(this, baseline, edits, isAddedSymbol, metadataStream, ilStream, pdbStream, options, testData, cancellationToken);
	}

	internal string? GetRuntimeMetadataVersion(EmitOptions emitOptions, DiagnosticBag diagnostics)
	{
		string runtimeMetadataVersion = GetRuntimeMetadataVersion(emitOptions);
		if (runtimeMetadataVersion != null)
		{
			return runtimeMetadataVersion;
		}
		DiagnosticBag incoming = DiagnosticBag.GetInstance();
		incoming.Add(ErrorCode.WRN_NoRuntimeMetadataVersion, NoLocation.Singleton);
		if (!FilterAndAppendAndFreeDiagnostics(diagnostics, ref incoming, CancellationToken.None))
		{
			return null;
		}
		return string.Empty;
	}

	private string? GetRuntimeMetadataVersion(EmitOptions emitOptions)
	{
		if (Assembly.CorLibrary is PEAssemblySymbol pEAssemblySymbol)
		{
			return pEAssemblySymbol.Assembly.ManifestModule.MetadataVersion;
		}
		return emitOptions.RuntimeMetadataVersion;
	}

	internal override void AddDebugSourceDocumentsForChecksumDirectives(DebugDocumentsBuilder documentsBuilder, SyntaxTree tree, DiagnosticBag diagnostics)
	{
		foreach (PragmaChecksumDirectiveTriviaSyntax directive in tree.GetRoot().GetDirectives((DirectiveTriviaSyntax d) => d.Kind() == SyntaxKind.PragmaChecksumDirectiveTrivia && !d.ContainsDiagnostics))
		{
			string valueText = directive.File.ValueText;
			string valueText2 = directive.Bytes.ValueText;
			string text = documentsBuilder.NormalizeDebugDocumentPath(valueText, tree.FilePath);
			DebugSourceDocument debugSourceDocument = documentsBuilder.TryGetDebugDocumentForNormalizedPath(text);
			if (debugSourceDocument != null)
			{
				if (!debugSourceDocument.IsComputedChecksum)
				{
					DebugSourceInfo sourceInfo = debugSourceDocument.GetSourceInfo();
					if (!ChecksumMatches(valueText2, sourceInfo.Checksum) || !(Guid.Parse(directive.Guid.ValueText) == sourceInfo.ChecksumAlgorithmId))
					{
						diagnostics.Add(ErrorCode.WRN_ConflictingChecksum, new SourceLocation(directive), valueText);
					}
				}
			}
			else
			{
				DebugSourceDocument document = new DebugSourceDocument(text, DebugSourceDocument.CorSymLanguageTypeCSharp, MakeChecksumBytes(valueText2), Guid.Parse(directive.Guid.ValueText));
				documentsBuilder.AddDebugDocument(document);
			}
		}
	}

	private static bool ChecksumMatches(string bytesText, ImmutableArray<byte> bytes)
	{
		if (bytesText.Length != bytes.Length * 2)
		{
			return false;
		}
		int i = 0;
		for (int num = bytesText.Length / 2; i < num; i++)
		{
			if (SyntaxFacts.HexValue(bytesText[i * 2]) * 16 + SyntaxFacts.HexValue(bytesText[i * 2 + 1]) != bytes[i])
			{
				return false;
			}
		}
		return true;
	}

	private static ImmutableArray<byte> MakeChecksumBytes(string bytesText)
	{
		int num = bytesText.Length / 2;
		ArrayBuilder<byte> instance = ArrayBuilder<byte>.GetInstance(num);
		for (int i = 0; i < num; i++)
		{
			int num2 = SyntaxFacts.HexValue(bytesText[i * 2]) * 16 + SyntaxFacts.HexValue(bytesText[i * 2 + 1]);
			instance.Add((byte)num2);
		}
		return instance.ToImmutableAndFree();
	}

	internal override bool HasCodeToEmit()
	{
		foreach (SyntaxTree syntaxTree in SyntaxTrees)
		{
			if (syntaxTree.GetCompilationUnitRoot().Members.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	protected override Compilation CommonWithReferences(IEnumerable<MetadataReference> newReferences)
	{
		return WithReferences(newReferences);
	}

	protected override Compilation CommonWithAssemblyName(string? assemblyName)
	{
		return WithAssemblyName(assemblyName);
	}

	[Experimental("RSEXPERIMENTAL001")]
	protected override SemanticModel CommonGetSemanticModel(SyntaxTree syntaxTree, SemanticModelOptions options)
	{
		return GetSemanticModel(syntaxTree, options);
	}

	protected override Compilation CommonAddSyntaxTrees(IEnumerable<SyntaxTree> trees)
	{
		return AddSyntaxTrees(trees);
	}

	protected override Compilation CommonRemoveSyntaxTrees(IEnumerable<SyntaxTree> trees)
	{
		return RemoveSyntaxTrees(trees);
	}

	protected override Compilation CommonRemoveAllSyntaxTrees()
	{
		return RemoveAllSyntaxTrees();
	}

	protected override Compilation CommonReplaceSyntaxTree(SyntaxTree oldTree, SyntaxTree? newTree)
	{
		return ReplaceSyntaxTree(oldTree, newTree);
	}

	protected override Compilation CommonWithOptions(CompilationOptions options)
	{
		return WithOptions((CSharpCompilationOptions)options);
	}

	protected override Compilation CommonWithScriptCompilationInfo(ScriptCompilationInfo? info)
	{
		return WithScriptCompilationInfo((CSharpScriptCompilationInfo)info);
	}

	protected override bool CommonContainsSyntaxTree(SyntaxTree? syntaxTree)
	{
		return ContainsSyntaxTree(syntaxTree);
	}

	protected override ISymbol? CommonGetAssemblyOrModuleSymbol(MetadataReference reference)
	{
		return GetAssemblyOrModuleSymbol(reference).GetPublicSymbol();
	}

	protected override Compilation CommonClone()
	{
		return Clone();
	}

	private protected override INamedTypeSymbolInternal CommonGetSpecialType(SpecialType specialType)
	{
		return GetSpecialType(specialType);
	}

	protected override INamespaceSymbol? CommonGetCompilationNamespace(INamespaceSymbol namespaceSymbol)
	{
		return GetCompilationNamespace(namespaceSymbol).GetPublicSymbol();
	}

	protected override INamedTypeSymbol? CommonGetTypeByMetadataName(string metadataName)
	{
		return GetTypeByMetadataName(metadataName).GetPublicSymbol();
	}

	protected override IArrayTypeSymbol CommonCreateArrayTypeSymbol(ITypeSymbol elementType, int rank, Microsoft.CodeAnalysis.NullableAnnotation elementNullableAnnotation)
	{
		return CreateArrayTypeSymbol(elementType.EnsureCSharpSymbolOrNull("elementType"), rank, elementNullableAnnotation.ToInternalAnnotation()).GetPublicSymbol();
	}

	protected override IPointerTypeSymbol CommonCreatePointerTypeSymbol(ITypeSymbol elementType)
	{
		return CreatePointerTypeSymbol(elementType.EnsureCSharpSymbolOrNull("elementType"), elementType.NullableAnnotation.ToInternalAnnotation()).GetPublicSymbol();
	}

	protected override IFunctionPointerTypeSymbol CommonCreateFunctionPointerTypeSymbol(ITypeSymbol returnType, RefKind returnRefKind, ImmutableArray<ITypeSymbol> parameterTypes, ImmutableArray<RefKind> parameterRefKinds, SignatureCallingConvention callingConvention, ImmutableArray<INamedTypeSymbol> callingConventionTypes)
	{
		if (returnType == null)
		{
			throw new ArgumentNullException("returnType");
		}
		if (parameterTypes.IsDefault)
		{
			throw new ArgumentNullException("parameterTypes");
		}
		for (int i = 0; i < parameterTypes.Length; i++)
		{
			if (parameterTypes[i] == null)
			{
				throw new ArgumentNullException(string.Format("{0}[{1}]", "parameterTypes", i));
			}
		}
		if (parameterRefKinds.IsDefault)
		{
			throw new ArgumentNullException("parameterRefKinds");
		}
		if (parameterRefKinds.Length != parameterTypes.Length)
		{
			throw new ArgumentException(string.Format(CSharpResources.NotSameNumberParameterTypesAndRefKinds, parameterTypes.Length, parameterRefKinds.Length));
		}
		if (returnRefKind == RefKind.Out)
		{
			throw new ArgumentException(CSharpResources.OutIsNotValidForReturn);
		}
		if (callingConvention != SignatureCallingConvention.Unmanaged && !callingConventionTypes.IsDefaultOrEmpty)
		{
			throw new ArgumentException(string.Format(CSharpResources.CallingConventionTypesRequireUnmanaged, "callingConventionTypes", "callingConvention"));
		}
		if (!callingConvention.IsValid())
		{
			throw new ArgumentOutOfRangeException("callingConvention");
		}
		TypeWithAnnotations returnType2 = TypeWithAnnotations.Create(returnType.EnsureCSharpSymbolOrNull("returnType"), returnType.NullableAnnotation.ToInternalAnnotation());
		ImmutableArray<TypeWithAnnotations> parameterTypes2 = parameterTypes.SelectAsArray((ITypeSymbol type) => TypeWithAnnotations.Create(type.EnsureCSharpSymbolOrNull("parameterTypes"), type.NullableAnnotation.ToInternalAnnotation()));
		CallingConvention num = callingConvention.FromSignatureConvention();
		ImmutableArray<CustomModifier> callingConventionModifiers = ((num == CallingConvention.Unmanaged && !callingConventionTypes.IsDefaultOrEmpty) ? callingConventionTypes.SelectAsArray((INamedTypeSymbol type, int index, CSharpCompilation @this) => getCustomModifierForType(type, @this, index), this) : ImmutableArray<CustomModifier>.Empty);
		return Microsoft.CodeAnalysis.CSharp.Symbols.FunctionPointerTypeSymbol.CreateFromParts(num, callingConventionModifiers, returnType2, returnRefKind, parameterTypes2, parameterRefKinds, this).GetPublicSymbol();
		static CustomModifier getCustomModifierForType(INamedTypeSymbol type, CSharpCompilation @this, int index)
		{
			if (type == null)
			{
				throw new ArgumentNullException(string.Format("{0}[{1}]", "callingConventionTypes", index));
			}
			Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol namedTypeSymbol = type.EnsureCSharpSymbolOrNull(string.Format("{0}[{1}]", "callingConventionTypes", index));
			if (!Microsoft.CodeAnalysis.CSharp.Symbols.FunctionPointerTypeSymbol.IsCallingConventionModifier(namedTypeSymbol) || @this.Assembly.CorLibrary != namedTypeSymbol.ContainingAssembly)
			{
				throw new ArgumentException(string.Format(CSharpResources.CallingConventionTypeIsInvalid, type.ToDisplayString()));
			}
			return CSharpCustomModifier.CreateOptional(namedTypeSymbol);
		}
	}

	protected override INamedTypeSymbol CommonCreateNativeIntegerTypeSymbol(bool signed)
	{
		return CreateNativeIntegerTypeSymbol(signed).GetPublicSymbol();
	}

	internal new Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol CreateNativeIntegerTypeSymbol(bool signed)
	{
		return GetSpecialType(signed ? SpecialType.System_IntPtr : SpecialType.System_UIntPtr).AsNativeInteger();
	}

	protected override INamedTypeSymbol CommonCreateTupleTypeSymbol(ImmutableArray<ITypeSymbol> elementTypes, ImmutableArray<string?> elementNames, ImmutableArray<Location?> elementLocations, ImmutableArray<Microsoft.CodeAnalysis.NullableAnnotation> elementNullableAnnotations)
	{
		ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance(elementTypes.Length);
		for (int i = 0; i < elementTypes.Length; i++)
		{
			ITypeSymbol typeSymbol = elementTypes[i];
			Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol typeSymbol2 = typeSymbol.EnsureCSharpSymbolOrNull(string.Format("{0}[{1}]", "elementTypes", i));
			NullableAnnotation nullableAnnotation = (elementNullableAnnotations.IsDefault ? typeSymbol.NullableAnnotation : elementNullableAnnotations[i]).ToInternalAnnotation();
			instance.Add(TypeWithAnnotations.Create(typeSymbol2, nullableAnnotation));
		}
		return Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol.CreateTuple(null, instance.ToImmutableAndFree(), elementLocations, elementNames, this, shouldCheckConstraints: false, includeNullability: false, default(ImmutableArray<bool>)).GetPublicSymbol();
	}

	protected override INamedTypeSymbol CommonCreateTupleTypeSymbol(INamedTypeSymbol underlyingType, ImmutableArray<string?> elementNames, ImmutableArray<Location?> elementLocations, ImmutableArray<Microsoft.CodeAnalysis.NullableAnnotation> elementNullableAnnotations)
	{
		Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol? namedTypeSymbol = underlyingType.EnsureCSharpSymbolOrNull("underlyingType");
		if (!namedTypeSymbol.IsTupleTypeOfCardinality(out var tupleCardinality))
		{
			throw new ArgumentException(CodeAnalysisResources.TupleUnderlyingTypeMustBeTupleCompatible, "underlyingType");
		}
		elementNames = Compilation.CheckTupleElementNames(tupleCardinality, elementNames);
		Compilation.CheckTupleElementLocations(tupleCardinality, elementLocations);
		Compilation.CheckTupleElementNullableAnnotations(tupleCardinality, elementNullableAnnotations);
		Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol namedTypeSymbol2 = Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol.CreateTuple(namedTypeSymbol, elementNames, default(ImmutableArray<bool>), elementLocations);
		if (!elementNullableAnnotations.IsDefault)
		{
			namedTypeSymbol2 = namedTypeSymbol2.WithElementTypes(namedTypeSymbol2.TupleElementTypesWithAnnotations.ZipAsArray(elementNullableAnnotations, (TypeWithAnnotations t, Microsoft.CodeAnalysis.NullableAnnotation a) => TypeWithAnnotations.Create(t.Type, a.ToInternalAnnotation())));
		}
		return namedTypeSymbol2.GetPublicSymbol();
	}

	protected override INamedTypeSymbol CommonCreateAnonymousTypeSymbol(ImmutableArray<ITypeSymbol> memberTypes, ImmutableArray<string> memberNames, ImmutableArray<Location> memberLocations, ImmutableArray<bool> memberIsReadOnly, ImmutableArray<Microsoft.CodeAnalysis.NullableAnnotation> memberNullableAnnotations)
	{
		int i = 0;
		for (int length = memberTypes.Length; i < length; i++)
		{
			memberTypes[i].EnsureCSharpSymbolOrNull(string.Format("{0}[{1}]", "memberTypes", i));
		}
		if (!memberIsReadOnly.IsDefault && memberIsReadOnly.Any((bool v) => !v))
		{
			throw new ArgumentException("Non-ReadOnly members are not supported in C# anonymous types.");
		}
		ArrayBuilder<AnonymousTypeField> instance = ArrayBuilder<AnonymousTypeField>.GetInstance();
		int num = 0;
		for (int length2 = memberTypes.Length; num < length2; num++)
		{
			Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol symbol = memberTypes[num].GetSymbol();
			string name = memberNames[num];
			Location location = (memberLocations.IsDefault ? Location.None : memberLocations[num]);
			NullableAnnotation nullableAnnotation = (memberNullableAnnotations.IsDefault ? NullableAnnotation.Oblivious : memberNullableAnnotations[num].ToInternalAnnotation());
			instance.Add(new AnonymousTypeField(name, location, TypeWithAnnotations.Create(symbol, nullableAnnotation), RefKind.None, ScopedKind.None));
		}
		AnonymousTypeDescriptor typeDescr = new AnonymousTypeDescriptor(instance.ToImmutableAndFree(), Location.None);
		return AnonymousTypeManager.ConstructAnonymousTypeSymbol(typeDescr, BindingDiagnosticBag.Discarded).GetPublicSymbol();
	}

	protected override IMethodSymbol CommonCreateBuiltinOperator(string name, ITypeSymbol returnType, ITypeSymbol leftType, ITypeSymbol rightType)
	{
		Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol csharpReturnType = returnType.EnsureCSharpSymbolOrNull("returnType");
		Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol csharpLeftType = leftType.EnsureCSharpSymbolOrNull("leftType");
		Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol csharpRightType = rightType.EnsureCSharpSymbolOrNull("rightType");
		SyntaxKind syntaxKind = SyntaxFacts.GetOperatorKind(name);
		if (syntaxKind == SyntaxKind.None)
		{
			throw new ArgumentException(string.Format(CodeAnalysisResources.BadBuiltInOps1, name), "name");
		}
		if (OperatorFacts.BinaryOperatorNameFromSyntaxKindIfAny(syntaxKind, SyntaxFacts.IsCheckedOperator(name)) != name)
		{
			throw new ArgumentException(string.Format(CodeAnalysisResources.BadBuiltInOps3, name), "name");
		}
		validateSignature();
		return new SynthesizedIntrinsicOperatorSymbol(csharpLeftType, name, csharpRightType, csharpReturnType).GetPublicSymbol();
		static bool isAllowedPointerArithmeticIntegralType(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type)
		{
			SpecialType specialType = type.SpecialType;
			if ((uint)(specialType - 13) <= 3u)
			{
				return true;
			}
			return false;
		}
		bool isReadOnlySpanOfByteType(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type)
		{
			if (IsReadOnlySpanType(type))
			{
				return ((Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol)type).TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[0].SpecialType == SpecialType.System_Byte;
			}
			return false;
		}
		void validateSignature()
		{
			if (csharpReturnType.TypeKind == TypeKind.Dynamic || csharpLeftType.TypeKind == TypeKind.Dynamic || csharpRightType.TypeKind == TypeKind.Dynamic)
			{
				return;
			}
			BinaryOperatorKind binaryOperatorKind = Binder.SyntaxKindToBinaryOperatorKind(SyntaxFacts.GetBinaryExpression(syntaxKind));
			if (csharpReturnType.SpecialType != SpecialType.None && csharpLeftType.SpecialType != SpecialType.None && csharpRightType.SpecialType != SpecialType.None)
			{
				BinaryOperatorKind binaryOperatorKind2 = OverloadResolution.BinopEasyOut.OpKind(binaryOperatorKind, csharpLeftType, csharpRightType);
				if (binaryOperatorKind2 != BinaryOperatorKind.Error)
				{
					BinaryOperatorSignature signature = BuiltInOperators.GetSignature(binaryOperatorKind2);
					if (csharpReturnType.SpecialType == signature.ReturnType.SpecialType && csharpLeftType.SpecialType == signature.LeftType.SpecialType && csharpRightType.SpecialType == signature.RightType.SpecialType)
					{
						return;
					}
				}
			}
			bool flag = ((binaryOperatorKind == BinaryOperatorKind.Equal || binaryOperatorKind == BinaryOperatorKind.NotEqual) ? true : false);
			if (flag && csharpReturnType.SpecialType == SpecialType.System_Boolean)
			{
				SpecialType specialType = csharpLeftType.SpecialType;
				SpecialType specialType2 = csharpRightType.SpecialType;
				if (specialType != SpecialType.System_Object)
				{
					if (specialType == SpecialType.System_Delegate && specialType2 == SpecialType.System_Delegate)
					{
						goto IL_012b;
					}
				}
				else if (specialType2 == SpecialType.System_Object)
				{
					goto IL_012b;
				}
				flag = false;
				goto IL_0131;
			}
			goto IL_0135;
			IL_0131:
			if (flag)
			{
				return;
			}
			goto IL_0135;
			IL_012b:
			flag = true;
			goto IL_0131;
			IL_0135:
			if (csharpLeftType.TypeKind == TypeKind.Delegate && Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol.Equals(csharpLeftType, csharpRightType, TypeCompareKind.ConsiderEverything))
			{
				flag = ((binaryOperatorKind == BinaryOperatorKind.Equal || binaryOperatorKind == BinaryOperatorKind.NotEqual) ? true : false);
				if (flag && csharpReturnType.SpecialType == SpecialType.System_Boolean)
				{
					return;
				}
				flag = ((binaryOperatorKind == BinaryOperatorKind.Addition || binaryOperatorKind == BinaryOperatorKind.Subtraction) ? true : false);
				if (flag && Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol.Equals(csharpLeftType, csharpReturnType, TypeCompareKind.ConsiderEverything))
				{
					return;
				}
			}
			if (csharpLeftType.IsEnumType() || csharpRightType.IsEnumType())
			{
				switch (binaryOperatorKind)
				{
				case BinaryOperatorKind.Equal:
				case BinaryOperatorKind.NotEqual:
				case BinaryOperatorKind.GreaterThan:
				case BinaryOperatorKind.LessThan:
				case BinaryOperatorKind.GreaterThanOrEqual:
				case BinaryOperatorKind.LessThanOrEqual:
					flag = true;
					break;
				default:
					flag = false;
					break;
				}
				if (flag && csharpReturnType.SpecialType == SpecialType.System_Boolean && Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol.Equals(csharpLeftType, csharpRightType, TypeCompareKind.ConsiderEverything))
				{
					return;
				}
				flag = ((binaryOperatorKind == BinaryOperatorKind.And || binaryOperatorKind == BinaryOperatorKind.Xor || binaryOperatorKind == BinaryOperatorKind.Or) ? true : false);
				if (flag && Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol.Equals(csharpLeftType, csharpRightType, TypeCompareKind.ConsiderEverything) && Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol.Equals(csharpReturnType, csharpRightType, TypeCompareKind.ConsiderEverything))
				{
					return;
				}
				flag = ((binaryOperatorKind == BinaryOperatorKind.Addition || binaryOperatorKind == BinaryOperatorKind.Subtraction) ? true : false);
				if ((flag && ((csharpLeftType.IsEnumType() && csharpRightType.SpecialType == csharpLeftType.GetEnumUnderlyingType()?.SpecialType && Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol.Equals(csharpLeftType, csharpReturnType, TypeCompareKind.ConsiderEverything)) || (csharpRightType.IsEnumType() && csharpLeftType.SpecialType == csharpRightType.GetEnumUnderlyingType()?.SpecialType && Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol.Equals(csharpRightType, csharpReturnType, TypeCompareKind.ConsiderEverything)))) || (binaryOperatorKind == BinaryOperatorKind.Subtraction && csharpReturnType.SpecialType == csharpLeftType.GetEnumUnderlyingType()?.SpecialType && Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol.Equals(csharpLeftType, csharpRightType, TypeCompareKind.ConsiderEverything)))
				{
					return;
				}
			}
			switch (binaryOperatorKind)
			{
			case BinaryOperatorKind.Equal:
			case BinaryOperatorKind.NotEqual:
			case BinaryOperatorKind.GreaterThan:
			case BinaryOperatorKind.LessThan:
			case BinaryOperatorKind.GreaterThanOrEqual:
			case BinaryOperatorKind.LessThanOrEqual:
				flag = true;
				break;
			default:
				flag = false;
				break;
			}
			if (flag && csharpReturnType.SpecialType == SpecialType.System_Boolean && csharpLeftType is Microsoft.CodeAnalysis.CSharp.Symbols.PointerTypeSymbol pointerTypeSymbol)
			{
				Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol pointedAtType = pointerTypeSymbol.PointedAtType;
				if ((object)pointedAtType != null && pointedAtType.SpecialType == SpecialType.System_Void && csharpRightType is Microsoft.CodeAnalysis.CSharp.Symbols.PointerTypeSymbol pointerTypeSymbol2)
				{
					pointedAtType = pointerTypeSymbol2.PointedAtType;
					if ((object)pointedAtType != null && pointedAtType.SpecialType == SpecialType.System_Void)
					{
						return;
					}
				}
			}
			if ((binaryOperatorKind == BinaryOperatorKind.Addition && csharpLeftType.IsPointerType() && isAllowedPointerArithmeticIntegralType(csharpRightType) && Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol.Equals(csharpLeftType, csharpReturnType, TypeCompareKind.ConsiderEverything)) || (binaryOperatorKind == BinaryOperatorKind.Addition && csharpRightType.IsPointerType() && isAllowedPointerArithmeticIntegralType(csharpLeftType) && Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol.Equals(csharpRightType, csharpReturnType, TypeCompareKind.ConsiderEverything)) || (binaryOperatorKind == BinaryOperatorKind.Subtraction && csharpLeftType.IsPointerType() && isAllowedPointerArithmeticIntegralType(csharpRightType) && Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol.Equals(csharpLeftType, csharpReturnType, TypeCompareKind.ConsiderEverything)) || (binaryOperatorKind == BinaryOperatorKind.Subtraction && csharpLeftType.IsPointerType() && csharpReturnType.SpecialType == SpecialType.System_Int64 && Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol.Equals(csharpLeftType, csharpRightType, TypeCompareKind.ConsiderEverything)) || (binaryOperatorKind == BinaryOperatorKind.Addition && isReadOnlySpanOfByteType(csharpReturnType) && isReadOnlySpanOfByteType(csharpLeftType) && isReadOnlySpanOfByteType(csharpRightType)))
			{
				return;
			}
			throw new ArgumentException(string.Format(CodeAnalysisResources.BadBuiltInOps2, csharpReturnType.ToDisplayString() + " operator " + name + "(" + csharpLeftType.ToDisplayString() + ", " + csharpRightType.ToDisplayString() + ")"));
		}
	}

	protected override IMethodSymbol CommonCreateBuiltinOperator(string name, ITypeSymbol returnType, ITypeSymbol operandType)
	{
		Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol csharpReturnType = returnType.EnsureCSharpSymbolOrNull("returnType");
		Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol csharpOperandType = operandType.EnsureCSharpSymbolOrNull("operandType");
		SyntaxKind syntaxKind = SyntaxFacts.GetOperatorKind(name);
		bool flag = syntaxKind == SyntaxKind.None;
		if (!flag)
		{
			string text = name;
			bool flag2 = ((text == "op_True" || text == "op_False") ? true : false);
			flag = flag2;
		}
		if (flag)
		{
			throw new ArgumentException(string.Format(CodeAnalysisResources.BadBuiltInOps1, name), "name");
		}
		if (OperatorFacts.UnaryOperatorNameFromSyntaxKindIfAny(syntaxKind, SyntaxFacts.IsCheckedOperator(name)) != name)
		{
			throw new ArgumentException(string.Format(CodeAnalysisResources.BadBuiltInOps3, name), "name");
		}
		validateSignature();
		return new SynthesizedIntrinsicOperatorSymbol(csharpOperandType, name, csharpReturnType).GetPublicSymbol();
		void validateSignature()
		{
			if (csharpReturnType.TypeKind != TypeKind.Dynamic && csharpOperandType.TypeKind != TypeKind.Dynamic)
			{
				UnaryOperatorKind unaryOperatorKind = Binder.SyntaxKindToUnaryOperatorKind(SyntaxFacts.GetPrefixUnaryExpression(syntaxKind));
				if (csharpReturnType.SpecialType != SpecialType.None && csharpOperandType.SpecialType != SpecialType.None)
				{
					UnaryOperatorKind unaryOperatorKind2 = OverloadResolution.UnopEasyOut.OpKind(unaryOperatorKind, csharpOperandType);
					if (unaryOperatorKind2 != UnaryOperatorKind.Error)
					{
						UnaryOperatorSignature signature = BuiltInOperators.GetSignature(unaryOperatorKind2);
						if (csharpReturnType.SpecialType == signature.ReturnType.SpecialType && csharpOperandType.SpecialType == signature.OperandType.SpecialType)
						{
							return;
						}
					}
				}
				bool flag3 = csharpOperandType.IsEnumType();
				if (flag3)
				{
					bool flag4 = ((unaryOperatorKind == UnaryOperatorKind.PrefixIncrement || unaryOperatorKind == UnaryOperatorKind.PrefixDecrement || unaryOperatorKind == UnaryOperatorKind.BitwiseComplement) ? true : false);
					flag3 = flag4;
				}
				if (!flag3 || !Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol.Equals(csharpOperandType, csharpReturnType, TypeCompareKind.ConsiderEverything))
				{
					flag3 = csharpOperandType.IsPointerType();
					if (flag3)
					{
						bool flag4 = ((unaryOperatorKind == UnaryOperatorKind.PrefixIncrement || unaryOperatorKind == UnaryOperatorKind.PrefixDecrement) ? true : false);
						flag3 = flag4;
					}
					if (!flag3 || !Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol.Equals(csharpOperandType, csharpReturnType, TypeCompareKind.ConsiderEverything))
					{
						throw new ArgumentException(string.Format(CodeAnalysisResources.BadBuiltInOps2, csharpReturnType.ToDisplayString() + " operator " + name + "(" + csharpOperandType.ToDisplayString() + ")"));
					}
				}
			}
		}
	}

	protected override IMethodSymbol? CommonGetEntryPoint(CancellationToken cancellationToken)
	{
		return GetEntryPoint(cancellationToken).GetPublicSymbol();
	}

	internal override int CompareSourceLocations(Location loc1, Location loc2)
	{
		int num = CompareSyntaxTreeOrdering(loc1.SourceTree, loc2.SourceTree);
		if (num != 0)
		{
			return num;
		}
		return loc1.SourceSpan.Start - loc2.SourceSpan.Start;
	}

	internal override int CompareSourceLocations(SyntaxReference loc1, SyntaxReference loc2)
	{
		int num = CompareSyntaxTreeOrdering(loc1.SyntaxTree, loc2.SyntaxTree);
		if (num != 0)
		{
			return num;
		}
		return loc1.Span.Start - loc2.Span.Start;
	}

	internal override int CompareSourceLocations(SyntaxNode loc1, SyntaxNode loc2)
	{
		int num = CompareSyntaxTreeOrdering(loc1.SyntaxTree, loc2.SyntaxTree);
		if (num != 0)
		{
			return num;
		}
		return loc1.Span.Start - loc2.Span.Start;
	}

	public override bool ContainsSymbolsWithName(Func<string, bool> predicate, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		if (filter == SymbolFilter.None)
		{
			throw new ArgumentException(CSharpResources.NoNoneSearchCriteria, "filter");
		}
		return DeclarationTable.ContainsName(MergedRootDeclaration, predicate, filter, cancellationToken);
	}

	public override IEnumerable<ISymbol> GetSymbolsWithName(Func<string, bool> predicate, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		if (filter == SymbolFilter.None)
		{
			throw new ArgumentException(CSharpResources.NoNoneSearchCriteria, "filter");
		}
		return new PredicateSymbolSearcher(this, filter, predicate, cancellationToken).GetSymbolsWithName().GetPublicSymbols();
	}

	public override bool ContainsSymbolsWithName(string name, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (filter == SymbolFilter.None)
		{
			throw new ArgumentException(CSharpResources.NoNoneSearchCriteria, "filter");
		}
		return DeclarationTable.ContainsName(MergedRootDeclaration, name, filter, cancellationToken);
	}

	public override IEnumerable<ISymbol> GetSymbolsWithName(string name, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetSymbolsWithNameCore(name, filter, cancellationToken).GetPublicSymbols();
	}

	internal IEnumerable<Symbol> GetSymbolsWithNameCore(string name, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (filter == SymbolFilter.None)
		{
			throw new ArgumentException(CSharpResources.NoNoneSearchCriteria, "filter");
		}
		return new NameSymbolSearcher(this, filter, name, cancellationToken).GetSymbolsWithName();
	}

	internal bool HasDynamicEmitAttributes(BindingDiagnosticBag diagnostics, Location location)
	{
		if ((object)Binder.GetWellKnownTypeMember(this, WellKnownMember.System_Runtime_CompilerServices_DynamicAttribute__ctor, diagnostics, location) != null)
		{
			return (object)Binder.GetWellKnownTypeMember(this, WellKnownMember.System_Runtime_CompilerServices_DynamicAttribute__ctorTransformFlags, diagnostics, location) != null;
		}
		return false;
	}

	internal bool HasTupleNamesAttributes(BindingDiagnosticBag diagnostics, Location location)
	{
		return (object)Binder.GetWellKnownTypeMember(this, WellKnownMember.System_Runtime_CompilerServices_TupleElementNamesAttribute__ctorTransformNames, diagnostics, location) != null;
	}

	internal bool CanEmitBoolean()
	{
		return CanEmitSpecialType(SpecialType.System_Boolean);
	}

	internal bool CanEmitSpecialType(SpecialType type)
	{
		DiagnosticInfo diagnosticInfo = GetSpecialType(type).GetUseSiteInfo().DiagnosticInfo;
		if (diagnosticInfo != null)
		{
			return diagnosticInfo.Severity != DiagnosticSeverity.Error;
		}
		return true;
	}

	internal bool ShouldEmitNativeIntegerAttributes()
	{
		return !Assembly.RuntimeSupportsNumericIntPtr;
	}

	internal bool ShouldEmitNullableAttributes(Symbol symbol)
	{
		if (symbol.ContainingModule != SourceModule)
		{
			return false;
		}
		if (!EmitNullablePublicOnly)
		{
			return true;
		}
		symbol = getExplicitAccessibilitySymbol(symbol);
		if (!AccessCheck.IsEffectivelyPublicOrInternal(symbol, out var isInternal))
		{
			return false;
		}
		if (isInternal)
		{
			return SourceAssembly.InternalsAreVisible;
		}
		return true;
		static Symbol getExplicitAccessibilitySymbol(Symbol containingSymbol)
		{
			while (true)
			{
				switch (containingSymbol.Kind)
				{
				case SymbolKind.Event:
				case SymbolKind.Parameter:
				case SymbolKind.Property:
				case SymbolKind.TypeParameter:
					break;
				default:
					return containingSymbol;
				}
				containingSymbol = containingSymbol.ContainingSymbol;
			}
		}
	}

	internal override AnalyzerDriver CreateAnalyzerDriver(ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerManager analyzerManager, SeverityFilter severityFilter)
	{
		Func<SyntaxNode, SyntaxKind> getKind = (SyntaxNode node) => node.Kind();
		Func<SyntaxTrivia, bool> isComment = (SyntaxTrivia trivia) => trivia.Kind() == SyntaxKind.SingleLineCommentTrivia || trivia.Kind() == SyntaxKind.MultiLineCommentTrivia;
		return new AnalyzerDriver<SyntaxKind>(analyzers, getKind, analyzerManager, severityFilter, isComment);
	}

	internal void SymbolDeclaredEvent(Symbol symbol)
	{
		base.EventQueue?.TryEnqueue(new SymbolDeclaredCompilationEvent(this, symbol));
	}

	internal override void SerializePdbEmbeddedCompilationOptions(BlobBuilder builder)
	{
		writeValue("language-version", LanguageVersion.ToDisplayString());
		if (Options.CheckOverflow)
		{
			writeValue("checked", Options.CheckOverflow.ToString());
		}
		if (Options.NullableContextOptions != NullableContextOptions.Disable)
		{
			writeValue("nullable", Options.NullableContextOptions.ToString());
		}
		if (Options.AllowUnsafe)
		{
			writeValue("unsafe", Options.AllowUnsafe.ToString());
		}
		ImmutableArray<string> preprocessorSymbols = GetPreprocessorSymbols();
		if (preprocessorSymbols.Any())
		{
			writeValue("define", string.Join(",", preprocessorSymbols));
		}
		void writeValue(string key, string value)
		{
			builder.WriteUTF8(key);
			builder.WriteByte(0);
			builder.WriteUTF8(value);
			builder.WriteByte(0);
		}
	}

	private ImmutableArray<string> GetPreprocessorSymbols()
	{
		return ((CSharpSyntaxTree)SyntaxTrees.FirstOrDefault())?.Options.PreprocessorSymbolNames.ToImmutableArray() ?? ImmutableArray<string>.Empty;
	}

	private protected override bool SupportsRuntimeCapabilityCore(RuntimeCapability capability)
	{
		return Assembly.SupportsRuntimeCapability(capability);
	}

	public override ImmutableArray<MetadataReference> GetUsedAssemblyReferences(CancellationToken cancellationToken = default(CancellationToken))
	{
		ConcurrentSet<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> completeSetOfUsedAssemblies = GetCompleteSetOfUsedAssemblies(cancellationToken);
		if (completeSetOfUsedAssemblies == null)
		{
			return ImmutableArray<MetadataReference>.Empty;
		}
		HashSet<MetadataReference> hashSet = new HashSet<MetadataReference>(ReferenceEqualityComparer.Instance);
		ImmutableDictionary<MetadataReference, ImmutableArray<MetadataReference>> mergedAssemblyReferencesMap = GetBoundReferenceManager().MergedAssemblyReferencesMap;
		foreach (MetadataReference reference in base.References)
		{
			if (reference.Properties.Kind == MetadataImageKind.Assembly)
			{
				Symbol referencedAssemblySymbol = GetBoundReferenceManager().GetReferencedAssemblySymbol(reference);
				if ((object)referencedAssemblySymbol != null && completeSetOfUsedAssemblies.Contains((Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol)referencedAssemblySymbol) && hashSet.Add(reference) && mergedAssemblyReferencesMap.TryGetValue(reference, out var value))
				{
					hashSet.AddAll(value);
				}
			}
		}
		ArrayBuilder<MetadataReference> instance = ArrayBuilder<MetadataReference>.GetInstance(hashSet.Count);
		foreach (MetadataReference reference2 in base.References)
		{
			if (hashSet.Contains(reference2))
			{
				instance.Add(reference2);
			}
		}
		return instance.ToImmutableAndFree();
	}

	private ConcurrentSet<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>? GetCompleteSetOfUsedAssemblies(CancellationToken cancellationToken)
	{
		if (!_usedAssemblyReferencesFrozen && !Volatile.Read(in _usedAssemblyReferencesFrozen))
		{
			BindingDiagnosticBag concurrentInstance = BindingDiagnosticBag.GetConcurrentInstance();
			GetDiagnosticsWithoutSeverityFiltering(CompilationStage.Declare, includeEarlierStages: true, concurrentInstance, null, cancellationToken);
			bool flag = concurrentInstance.HasAnyErrors();
			if (!flag)
			{
				concurrentInstance.DiagnosticBag.Clear();
				GetDiagnosticsForAllMethodBodies(concurrentInstance, doLowering: true, cancellationToken);
				flag = concurrentInstance.HasAnyErrors();
				if (!flag)
				{
					AddUsedAssemblies(concurrentInstance.DependenciesBag);
				}
			}
			completeTheSetOfUsedAssemblies(flag, cancellationToken);
			concurrentInstance.Free();
		}
		return _lazyUsedAssemblyReferences;
		void addReferencedAssemblies(Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol assembly, bool includeMainModule, ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> stack)
		{
			for (int i = ((!includeMainModule) ? 1 : 0); i < assembly.Modules.Length; i++)
			{
				foreach (Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol referencedAssemblySymbol in assembly.Modules[i].ReferencedAssemblySymbols)
				{
					addUsedAssembly(referencedAssemblySymbol, stack);
				}
			}
		}
		void addUsedAssembly(Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol dependency, ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> stack)
		{
			if (AddUsedAssembly(dependency))
			{
				stack.Push(dependency);
			}
		}
		void completeTheSetOfUsedAssemblies(bool seenErrors, CancellationToken cancellationToken2)
		{
			if (!_usedAssemblyReferencesFrozen && !Volatile.Read(in _usedAssemblyReferencesFrozen))
			{
				if (seenErrors)
				{
					foreach (Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol referencedAssemblySymbol2 in SourceModule.ReferencedAssemblySymbols)
					{
						AddUsedAssembly(referencedAssemblySymbol2);
					}
				}
				else
				{
					for (int i = 1; i < SourceAssembly.Modules.Length; i++)
					{
						foreach (Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol referencedAssemblySymbol3 in SourceAssembly.Modules[i].ReferencedAssemblySymbols)
						{
							AddUsedAssembly(referencedAssemblySymbol3);
						}
					}
					if (_usedAssemblyReferencesFrozen || Volatile.Read(in _usedAssemblyReferencesFrozen))
					{
						return;
					}
					if (_lazyUsedAssemblyReferences != null)
					{
						lock (_lazyUsedAssemblyReferences)
						{
							if (_usedAssemblyReferencesFrozen || Volatile.Read(in _usedAssemblyReferencesFrozen))
							{
								return;
							}
							ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> instance = ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.GetInstance(_lazyUsedAssemblyReferences.Count);
							instance.AddRange(_lazyUsedAssemblyReferences);
							while (instance.Count != 0)
							{
								Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol assemblySymbol = instance.Pop();
								if (!(assemblySymbol is Microsoft.CodeAnalysis.CSharp.Symbols.SourceAssemblySymbol sourceAssemblySymbol))
								{
									if (assemblySymbol is RetargetingAssemblySymbol retargetingAssemblySymbol)
									{
										ConcurrentSet<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> completeSetOfUsedAssemblies = retargetingAssemblySymbol.UnderlyingAssembly.DeclaringCompilation.GetCompleteSetOfUsedAssemblies(cancellationToken2);
										if (completeSetOfUsedAssemblies != null)
										{
											foreach (Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol referencedAssemblySymbol4 in retargetingAssemblySymbol.UnderlyingAssembly.SourceModule.ReferencedAssemblySymbols)
											{
												if (!referencedAssemblySymbol4.IsLinked && completeSetOfUsedAssemblies.Contains(referencedAssemblySymbol4))
												{
													if (!((RetargetingModuleSymbol)retargetingAssemblySymbol.Modules[0]).RetargetingDefinitions(referencedAssemblySymbol4, out var to))
													{
														to = referencedAssemblySymbol4;
													}
													addUsedAssembly(to, instance);
												}
											}
										}
										addReferencedAssemblies(retargetingAssemblySymbol, includeMainModule: false, instance);
									}
									else
									{
										addReferencedAssemblies(assemblySymbol, includeMainModule: true, instance);
									}
								}
								else
								{
									ConcurrentSet<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> completeSetOfUsedAssemblies = sourceAssemblySymbol.DeclaringCompilation.GetCompleteSetOfUsedAssemblies(cancellationToken2);
									if (completeSetOfUsedAssemblies != null)
									{
										foreach (Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol item in completeSetOfUsedAssemblies)
										{
											addUsedAssembly(item, instance);
										}
									}
								}
							}
							instance.Free();
						}
					}
					if ((object)SourceAssembly.CorLibrary != null)
					{
						AddUsedAssembly(SourceAssembly.CorLibrary);
					}
				}
				_usedAssemblyReferencesFrozen = true;
			}
		}
	}

	internal void AddUsedAssemblies(ICollection<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>? assemblies)
	{
		if (assemblies.IsNullOrEmpty())
		{
			return;
		}
		foreach (Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol assembly in assemblies)
		{
			AddUsedAssembly(assembly);
		}
	}

	internal bool AddUsedAssembly(Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol? assembly)
	{
		if ((object)assembly == null || assembly == SourceAssembly || assembly.IsMissing)
		{
			return false;
		}
		if (_lazyUsedAssemblyReferences == null)
		{
			Interlocked.CompareExchange(ref _lazyUsedAssemblyReferences, new ConcurrentSet<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>(), null);
		}
		return _lazyUsedAssemblyReferences.Add(assembly);
	}

	internal EmbeddableAttributes GetNeedsGeneratedAttributes(bool freezeState = true)
	{
		if (freezeState)
		{
			_needsGeneratedAttributes_IsFrozen = true;
		}
		return (EmbeddableAttributes)_needsGeneratedAttributes;
	}

	private void SetNeedsGeneratedAttributes(EmbeddableAttributes attributes)
	{
		ThreadSafeFlagOperations.Set(ref _needsGeneratedAttributes, (int)attributes);
	}

	internal bool GetUsesNullableAttributes()
	{
		_needsGeneratedAttributes_IsFrozen = true;
		return _usesNullableAttributes;
	}

	private void SetUsesNullableAttributes()
	{
		_usesNullableAttributes = true;
	}

	internal Symbol? GetWellKnownTypeMember(WellKnownMember member)
	{
		if (IsMemberMissing(member))
		{
			return null;
		}
		if (_lazyWellKnownTypeMembers == null || (object)_lazyWellKnownTypeMembers[(int)member] == Microsoft.CodeAnalysis.CSharp.Symbols.ErrorTypeSymbol.UnknownResultType)
		{
			if (_lazyWellKnownTypeMembers == null)
			{
				Symbol[] array = new Symbol[606];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = Microsoft.CodeAnalysis.CSharp.Symbols.ErrorTypeSymbol.UnknownResultType;
				}
				Interlocked.CompareExchange(ref _lazyWellKnownTypeMembers, array, null);
			}
			MemberDescriptor descriptor = WellKnownMembers.GetDescriptor(member);
			Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol namedTypeSymbol = (descriptor.IsSpecialTypeMember ? GetSpecialType(descriptor.DeclaringSpecialType) : GetWellKnownType(descriptor.DeclaringWellKnownType));
			Symbol value = null;
			if (!namedTypeSymbol.IsErrorType())
			{
				value = GetRuntimeMember(namedTypeSymbol, in descriptor, WellKnownMemberSignatureComparer, Assembly);
			}
			Interlocked.CompareExchange(ref _lazyWellKnownTypeMembers[(int)member], value, Microsoft.CodeAnalysis.CSharp.Symbols.ErrorTypeSymbol.UnknownResultType);
		}
		return _lazyWellKnownTypeMembers[(int)member];
	}

	internal Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol GetWellKnownType(WellKnownType type)
	{
		bool ignoreCorLibraryDuplicatedTypes = Options.TopLevelBinderFlags.Includes(BinderFlags.IgnoreCorLibraryDuplicatedTypes);
		int num = (int)(type - 58);
		DiagnosticBag instance;
		Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol namedTypeSymbol;
		MetadataTypeName fullName;
		CSDiagnosticInfo errorInfo;
		if (_lazyWellKnownTypes == null || (object)_lazyWellKnownTypes[num] == null)
		{
			if (_lazyWellKnownTypes == null)
			{
				Interlocked.CompareExchange(ref _lazyWellKnownTypes, new Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol[307], null);
			}
			string metadataName = type.GetMetadataName();
			instance = DiagnosticBag.GetInstance();
			(Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol) conflicts = default((Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol));
			if (IsTypeMissing(type))
			{
				namedTypeSymbol = null;
			}
			else
			{
				DiagnosticBag warnings = ((type <= WellKnownType.System_Runtime_GCLatencyMode) ? instance : null);
				namedTypeSymbol = Assembly.GetTypeByMetadataName(metadataName, includeReferences: true, isWellKnownType: true, out conflicts, useCLSCompliantNameArityEncoding: true, warnings, ignoreCorLibraryDuplicatedTypes);
			}
			if ((object)namedTypeSymbol == null)
			{
				fullName = MetadataTypeName.FromFullName(metadataName, useCLSCompliantNameArityEncoding: true);
				var (assemblySymbol, _) = conflicts;
				if ((object)assemblySymbol != null)
				{
					Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol item = conflicts.Item2;
					if ((object)item != null)
					{
						errorInfo = new CSDiagnosticInfo(ErrorCode.ERR_PredefinedTypeAmbiguous, fullName.FullName, assemblySymbol, item);
						goto IL_0112;
					}
				}
				errorInfo = ((!type.IsValueTupleType()) ? null : new CSDiagnosticInfo(ErrorCode.ERR_PredefinedValueTupleTypeNotFound, fullName.FullName));
				goto IL_0112;
			}
			goto IL_0133;
		}
		goto IL_015b;
		IL_015b:
		return _lazyWellKnownTypes[num];
		IL_0133:
		if ((object)Interlocked.CompareExchange(ref _lazyWellKnownTypes[num], namedTypeSymbol, null) == null)
		{
			AdditionalCodegenWarnings.AddRange(instance);
		}
		instance.Free();
		goto IL_015b;
		IL_0112:
		namedTypeSymbol = new MissingMetadataTypeSymbol.TopLevel(Assembly.Modules[0], ref fullName, type, errorInfo);
		goto IL_0133;
	}

	internal bool IsAttributeType(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type)
	{
		CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol>.Discarded;
		return IsEqualOrDerivedFromWellKnownClass(type, WellKnownType.System_Attribute, ref useSiteInfo);
	}

	internal override bool IsAttributeType(ITypeSymbol type)
	{
		return IsAttributeType(type.EnsureCSharpSymbolOrNull("type"));
	}

	internal bool IsExceptionType(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type, ref CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> useSiteInfo)
	{
		return IsEqualOrDerivedFromWellKnownClass(type, WellKnownType.System_Exception, ref useSiteInfo);
	}

	internal bool IsReadOnlySpanType(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type)
	{
		return Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol.Equals(type.OriginalDefinition, GetWellKnownType(WellKnownType.System_ReadOnlySpan_T), TypeCompareKind.ConsiderEverything);
	}

	internal bool IsEqualOrDerivedFromWellKnownClass(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type, WellKnownType wellKnownType, ref CompoundUseSiteInfo<Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol> useSiteInfo)
	{
		if (type.Kind != SymbolKind.NamedType || type.TypeKind != TypeKind.Class)
		{
			return false;
		}
		Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol wellKnownType2 = GetWellKnownType(wellKnownType);
		if (!type.Equals(wellKnownType2, TypeCompareKind.ConsiderEverything))
		{
			return type.IsDerivedFrom(wellKnownType2, TypeCompareKind.ConsiderEverything, ref useSiteInfo);
		}
		return true;
	}

	internal override bool IsSystemTypeReference(ITypeSymbolInternal type)
	{
		return Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol.Equals((Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol)type, GetWellKnownType(WellKnownType.System_Type), TypeCompareKind.ConsiderEverything);
	}

	internal override ISymbolInternal? CommonGetWellKnownTypeMember(WellKnownMember member)
	{
		return GetWellKnownTypeMember(member);
	}

	internal override ITypeSymbolInternal CommonGetWellKnownType(WellKnownType wellknownType)
	{
		return GetWellKnownType(wellknownType);
	}

	internal static Symbol? GetRuntimeMember(Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol declaringType, in MemberDescriptor descriptor, SignatureComparer<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol, Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol, Microsoft.CodeAnalysis.CSharp.Symbols.PropertySymbol, Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol, Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol> comparer, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol? accessWithinOpt)
	{
		return GetRuntimeMember(declaringType.GetMembers(descriptor.Name), in descriptor, comparer, accessWithinOpt);
	}

	internal static Symbol? GetRuntimeMember(ImmutableArray<Symbol> members, in MemberDescriptor descriptor, SignatureComparer<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol, Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol, Microsoft.CodeAnalysis.CSharp.Symbols.PropertySymbol, Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol, Microsoft.CodeAnalysis.CSharp.Symbols.ParameterSymbol> comparer, Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol? accessWithinOpt)
	{
		MethodKind methodKind = MethodKind.Ordinary;
		bool flag = (descriptor.Flags & MemberFlags.Static) != 0;
		Symbol symbol = null;
		SymbolKind symbolKind;
		switch (descriptor.Flags & MemberFlags.KindMask)
		{
		case MemberFlags.Constructor:
			symbolKind = SymbolKind.Method;
			methodKind = MethodKind.Constructor;
			break;
		case MemberFlags.Method:
			symbolKind = SymbolKind.Method;
			break;
		case MemberFlags.PropertyGet:
			symbolKind = SymbolKind.Method;
			methodKind = MethodKind.PropertyGet;
			break;
		case MemberFlags.Field:
			symbolKind = SymbolKind.Field;
			break;
		case MemberFlags.Property:
			symbolKind = SymbolKind.Property;
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(descriptor.Flags);
		}
		foreach (Symbol item in members)
		{
			if (!item.Name.Equals(descriptor.Name) || item.Kind != symbolKind || item.IsStatic != flag || (item.DeclaredAccessibility != Accessibility.Public && ((object)accessWithinOpt == null || !Symbol.IsSymbolAccessible(item, accessWithinOpt))))
			{
				continue;
			}
			switch (symbolKind)
			{
			case SymbolKind.Method:
			{
				Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol methodSymbol = (Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol)item;
				MethodKind methodKind2 = methodSymbol.MethodKind;
				if (methodKind2 == MethodKind.Conversion || methodKind2 == MethodKind.UserDefinedOperator)
				{
					methodKind2 = MethodKind.Ordinary;
				}
				if (methodSymbol.Arity != descriptor.Arity || methodKind2 != methodKind || (descriptor.Flags & MemberFlags.Virtual) != 0 != (methodSymbol.IsVirtual || methodSymbol.IsOverride || methodSymbol.IsAbstract) || !comparer.MatchMethodSignature(methodSymbol, descriptor.Signature))
				{
					continue;
				}
				break;
			}
			case SymbolKind.Property:
			{
				Microsoft.CodeAnalysis.CSharp.Symbols.PropertySymbol propertySymbol = (Microsoft.CodeAnalysis.CSharp.Symbols.PropertySymbol)item;
				if ((descriptor.Flags & MemberFlags.Virtual) != 0 != (propertySymbol.IsVirtual || propertySymbol.IsOverride || propertySymbol.IsAbstract) || !comparer.MatchPropertySignature(propertySymbol, descriptor.Signature))
				{
					continue;
				}
				break;
			}
			case SymbolKind.Field:
				if (!comparer.MatchFieldSignature((Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol)item, descriptor.Signature))
				{
					continue;
				}
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(symbolKind);
			}
			if ((object)symbol != null)
			{
				symbol = null;
				break;
			}
			symbol = item;
		}
		return symbol;
	}

	internal SynthesizedAttributeData? TrySynthesizeAttribute(WellKnownMember constructor, ImmutableArray<TypedConstant> arguments = default(ImmutableArray<TypedConstant>), ImmutableArray<KeyValuePair<WellKnownMember, TypedConstant>> namedArguments = default(ImmutableArray<KeyValuePair<WellKnownMember, TypedConstant>>), bool isOptionalUse = false)
	{
		Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol methodSymbol = (Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol)Binder.GetWellKnownTypeMember(this, constructor, out var useSiteInfo, isOptional: true);
		if ((object)methodSymbol == null)
		{
			return null;
		}
		if (arguments.IsDefault)
		{
			arguments = ImmutableArray<TypedConstant>.Empty;
		}
		ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments2;
		if (namedArguments.IsDefault)
		{
			namedArguments2 = ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty;
		}
		else
		{
			ArrayBuilder<KeyValuePair<string, TypedConstant>> arrayBuilder = new ArrayBuilder<KeyValuePair<string, TypedConstant>>(namedArguments.Length);
			foreach (KeyValuePair<WellKnownMember, TypedConstant> item in namedArguments)
			{
				Symbol wellKnownTypeMember = Binder.GetWellKnownTypeMember(this, item.Key, out useSiteInfo, isOptional: true);
				if (wellKnownTypeMember == null || wellKnownTypeMember is Microsoft.CodeAnalysis.CSharp.Symbols.ErrorTypeSymbol)
				{
					return null;
				}
				arrayBuilder.Add(new KeyValuePair<string, TypedConstant>(wellKnownTypeMember.Name, item.Value));
			}
			namedArguments2 = arrayBuilder.ToImmutableAndFree();
		}
		return SynthesizedAttributeData.Create(this, methodSymbol, arguments, namedArguments2);
	}

	internal SynthesizedAttributeData? TrySynthesizeAttribute(SpecialMember constructor, bool isOptionalUse = false)
	{
		Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol methodSymbol = (Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol)GetSpecialTypeMember(constructor);
		if ((object)methodSymbol == null)
		{
			return null;
		}
		return SynthesizedAttributeData.Create(this, methodSymbol, ImmutableArray<TypedConstant>.Empty, ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty);
	}

	internal SynthesizedAttributeData? SynthesizeDecimalConstantAttribute(decimal value)
	{
		value.GetBits(out var isNegative, out var scale, out var low, out var mid, out var high);
		Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol specialType = GetSpecialType(SpecialType.System_Byte);
		Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol specialType2 = GetSpecialType(SpecialType.System_UInt32);
		return TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_DecimalConstantAttribute__ctor, ImmutableArray.Create(new ReadOnlySpan<TypedConstant>(new TypedConstant[5]
		{
			new TypedConstant(specialType, TypedConstantKind.Primitive, scale),
			new TypedConstant(specialType, TypedConstantKind.Primitive, (byte)(isNegative ? 128u : 0u)),
			new TypedConstant(specialType2, TypedConstantKind.Primitive, high),
			new TypedConstant(specialType2, TypedConstantKind.Primitive, mid),
			new TypedConstant(specialType2, TypedConstantKind.Primitive, low)
		})));
	}

	internal SynthesizedAttributeData? SynthesizeDateTimeConstantAttribute(DateTime value)
	{
		TypedConstant item = new TypedConstant(GetSpecialType(SpecialType.System_Int64), TypedConstantKind.Primitive, value.Ticks);
		return TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_DateTimeConstantAttribute__ctor, ImmutableArray.Create(item));
	}

	internal SynthesizedAttributeData? SynthesizeDebuggerBrowsableNeverAttribute()
	{
		if (Options.OptimizationLevel != OptimizationLevel.Debug)
		{
			return null;
		}
		return TrySynthesizeAttribute(WellKnownMember.System_Diagnostics_DebuggerBrowsableAttribute__ctor, ImmutableArray.Create(new TypedConstant(GetWellKnownType(WellKnownType.System_Diagnostics_DebuggerBrowsableState), TypedConstantKind.Enum, DebuggerBrowsableState.Never)));
	}

	internal SynthesizedAttributeData? SynthesizeDebuggerStepThroughAttribute()
	{
		if (Options.OptimizationLevel != OptimizationLevel.Debug)
		{
			return null;
		}
		return TrySynthesizeAttribute(WellKnownMember.System_Diagnostics_DebuggerStepThroughAttribute__ctor);
	}

	private void EnsureEmbeddableAttributeExists(EmbeddableAttributes attribute, BindingDiagnosticBag? diagnostics, Location location, bool modifyCompilation)
	{
		if (CheckIfAttributeShouldBeEmbedded(attribute, diagnostics, location) & modifyCompilation)
		{
			SetNeedsGeneratedAttributes(attribute);
		}
		if (((attribute & (EmbeddableAttributes.NullableAttribute | EmbeddableAttributes.NullableContextAttribute)) != 0) & modifyCompilation)
		{
			SetUsesNullableAttributes();
		}
	}

	internal void EnsureIsReadOnlyAttributeExists(BindingDiagnosticBag? diagnostics, Location location, bool modifyCompilation)
	{
		EnsureEmbeddableAttributeExists(EmbeddableAttributes.IsReadOnlyAttribute, diagnostics, location, modifyCompilation);
	}

	internal void EnsureRequiresLocationAttributeExists(BindingDiagnosticBag? diagnostics, Location location, bool modifyCompilation)
	{
		EnsureEmbeddableAttributeExists(EmbeddableAttributes.RequiresLocationAttribute, diagnostics, location, modifyCompilation);
	}

	internal void EnsureParamCollectionAttributeExists(BindingDiagnosticBag? diagnostics, Location location, bool modifyCompilation)
	{
		EnsureEmbeddableAttributeExists(EmbeddableAttributes.ParamCollectionAttribute, diagnostics, location, modifyCompilation);
	}

	internal void EnsureIsByRefLikeAttributeExists(BindingDiagnosticBag? diagnostics, Location location, bool modifyCompilation)
	{
		EnsureEmbeddableAttributeExists(EmbeddableAttributes.IsByRefLikeAttribute, diagnostics, location, modifyCompilation);
	}

	internal void EnsureIsUnmanagedAttributeExists(BindingDiagnosticBag? diagnostics, Location location, bool modifyCompilation)
	{
		EnsureEmbeddableAttributeExists(EmbeddableAttributes.IsUnmanagedAttribute, diagnostics, location, modifyCompilation);
	}

	internal void EnsureNullableAttributeExists(BindingDiagnosticBag? diagnostics, Location location, bool modifyCompilation)
	{
		EnsureEmbeddableAttributeExists(EmbeddableAttributes.NullableAttribute, diagnostics, location, modifyCompilation);
	}

	internal void EnsureNullableContextAttributeExists(BindingDiagnosticBag? diagnostics, Location location, bool modifyCompilation)
	{
		EnsureEmbeddableAttributeExists(EmbeddableAttributes.NullableContextAttribute, diagnostics, location, modifyCompilation);
	}

	internal void EnsureNativeIntegerAttributeExists(BindingDiagnosticBag? diagnostics, Location location, bool modifyCompilation)
	{
		EnsureEmbeddableAttributeExists(EmbeddableAttributes.NativeIntegerAttribute, diagnostics, location, modifyCompilation);
	}

	internal void EnsureScopedRefAttributeExists(BindingDiagnosticBag? diagnostics, Location location, bool modifyCompilation)
	{
		EnsureEmbeddableAttributeExists(EmbeddableAttributes.ScopedRefAttribute, diagnostics, location, modifyCompilation);
	}

	internal void EnsureExtensionMarkerAttributeExists(BindingDiagnosticBag? diagnostics, Location location, bool modifyCompilation)
	{
		EnsureEmbeddableAttributeExists(EmbeddableAttributes.ExtensionMarkerAttribute, diagnostics, location, modifyCompilation);
	}

	internal bool CheckIfAttributeShouldBeEmbedded(EmbeddableAttributes attribute, BindingDiagnosticBag? diagnosticsOpt, Location locationOpt)
	{
		return attribute switch
		{
			EmbeddableAttributes.IsReadOnlyAttribute => CheckIfAttributeShouldBeEmbedded(diagnosticsOpt, locationOpt, WellKnownType.System_Runtime_CompilerServices_IsReadOnlyAttribute, WellKnownMember.System_Runtime_CompilerServices_IsReadOnlyAttribute__ctor), 
			EmbeddableAttributes.IsByRefLikeAttribute => CheckIfAttributeShouldBeEmbedded(diagnosticsOpt, locationOpt, WellKnownType.System_Runtime_CompilerServices_IsByRefLikeAttribute, WellKnownMember.System_Runtime_CompilerServices_IsByRefLikeAttribute__ctor), 
			EmbeddableAttributes.IsUnmanagedAttribute => CheckIfAttributeShouldBeEmbedded(diagnosticsOpt, locationOpt, WellKnownType.System_Runtime_CompilerServices_IsUnmanagedAttribute, WellKnownMember.System_Runtime_CompilerServices_IsUnmanagedAttribute__ctor), 
			EmbeddableAttributes.NullableAttribute => CheckIfAttributeShouldBeEmbedded(diagnosticsOpt, locationOpt, WellKnownType.System_Runtime_CompilerServices_NullableAttribute, WellKnownMember.System_Runtime_CompilerServices_NullableAttribute__ctorByte, WellKnownMember.System_Runtime_CompilerServices_NullableAttribute__ctorTransformFlags), 
			EmbeddableAttributes.NullableContextAttribute => CheckIfAttributeShouldBeEmbedded(diagnosticsOpt, locationOpt, WellKnownType.System_Runtime_CompilerServices_NullableContextAttribute, WellKnownMember.System_Runtime_CompilerServices_NullableContextAttribute__ctor), 
			EmbeddableAttributes.NullablePublicOnlyAttribute => CheckIfAttributeShouldBeEmbedded(diagnosticsOpt, locationOpt, WellKnownType.System_Runtime_CompilerServices_NullablePublicOnlyAttribute, WellKnownMember.System_Runtime_CompilerServices_NullablePublicOnlyAttribute__ctor), 
			EmbeddableAttributes.NativeIntegerAttribute => CheckIfAttributeShouldBeEmbedded(diagnosticsOpt, locationOpt, WellKnownType.System_Runtime_CompilerServices_NativeIntegerAttribute, WellKnownMember.System_Runtime_CompilerServices_NativeIntegerAttribute__ctor, WellKnownMember.System_Runtime_CompilerServices_NativeIntegerAttribute__ctorTransformFlags), 
			EmbeddableAttributes.ScopedRefAttribute => CheckIfAttributeShouldBeEmbedded(diagnosticsOpt, locationOpt, WellKnownType.System_Runtime_CompilerServices_ScopedRefAttribute, WellKnownMember.System_Runtime_CompilerServices_ScopedRefAttribute__ctor), 
			EmbeddableAttributes.RefSafetyRulesAttribute => CheckIfAttributeShouldBeEmbedded(diagnosticsOpt, locationOpt, WellKnownType.System_Runtime_CompilerServices_RefSafetyRulesAttribute, WellKnownMember.System_Runtime_CompilerServices_RefSafetyRulesAttribute__ctor), 
			EmbeddableAttributes.RequiresLocationAttribute => CheckIfAttributeShouldBeEmbedded(diagnosticsOpt, locationOpt, WellKnownType.System_Runtime_CompilerServices_RequiresLocationAttribute, WellKnownMember.System_Runtime_CompilerServices_RequiresLocationAttribute__ctor), 
			EmbeddableAttributes.ParamCollectionAttribute => CheckIfAttributeShouldBeEmbedded(diagnosticsOpt, locationOpt, WellKnownType.System_Runtime_CompilerServices_ParamCollectionAttribute, WellKnownMember.System_Runtime_CompilerServices_ParamCollectionAttribute__ctor), 
			EmbeddableAttributes.ExtensionMarkerAttribute => CheckIfAttributeShouldBeEmbedded(diagnosticsOpt, locationOpt, WellKnownType.System_Runtime_CompilerServices_ExtensionMarkerAttribute, WellKnownMember.System_Runtime_CompilerServices_ExtensionMarkerAttribute__ctor), 
			_ => throw ExceptionUtilities.UnexpectedValue(attribute), 
		};
	}

	private bool CheckIfAttributeShouldBeEmbedded(BindingDiagnosticBag? diagnosticsOpt, Location? locationOpt, WellKnownType attributeType, WellKnownMember attributeCtor, WellKnownMember? secondAttributeCtor = null)
	{
		Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol wellKnownType = GetWellKnownType(attributeType);
		if (wellKnownType is MissingMetadataTypeSymbol)
		{
			if (Options.OutputKind != OutputKind.NetModule)
			{
				return true;
			}
			if (diagnosticsOpt != null)
			{
				Binder.ReportUseSite(wellKnownType, diagnosticsOpt, locationOpt);
			}
		}
		else if (diagnosticsOpt != null && Binder.GetWellKnownTypeMember(this, attributeCtor, diagnosticsOpt, locationOpt) != null && secondAttributeCtor.HasValue)
		{
			Binder.GetWellKnownTypeMember(this, secondAttributeCtor.Value, diagnosticsOpt, locationOpt);
		}
		return false;
	}

	internal SynthesizedAttributeData? SynthesizeDebuggableAttribute()
	{
		if (GetWellKnownType(WellKnownType.System_Diagnostics_DebuggableAttribute) is MissingMetadataTypeSymbol)
		{
			return null;
		}
		Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol wellKnownType = GetWellKnownType(WellKnownType.System_Diagnostics_DebuggableAttribute__DebuggingModes);
		if (wellKnownType is MissingMetadataTypeSymbol)
		{
			return null;
		}
		Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol fieldSymbol = (Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol)GetWellKnownTypeMember(WellKnownMember.System_Diagnostics_DebuggableAttribute_DebuggingModes__IgnoreSymbolStoreSequencePoints);
		if ((object)fieldSymbol == null || !fieldSymbol.HasConstantValue)
		{
			return null;
		}
		int num = fieldSymbol.GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false).Int32Value;
		if (_options.OptimizationLevel == OptimizationLevel.Debug)
		{
			Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol fieldSymbol2 = (Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol)GetWellKnownTypeMember(WellKnownMember.System_Diagnostics_DebuggableAttribute_DebuggingModes__Default);
			if ((object)fieldSymbol2 == null || !fieldSymbol2.HasConstantValue)
			{
				return null;
			}
			Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol fieldSymbol3 = (Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol)GetWellKnownTypeMember(WellKnownMember.System_Diagnostics_DebuggableAttribute_DebuggingModes__DisableOptimizations);
			if ((object)fieldSymbol3 == null || !fieldSymbol3.HasConstantValue)
			{
				return null;
			}
			num |= fieldSymbol2.GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false).Int32Value;
			num |= fieldSymbol3.GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false).Int32Value;
		}
		if (_options.EnableEditAndContinue)
		{
			Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol fieldSymbol4 = (Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol)GetWellKnownTypeMember(WellKnownMember.System_Diagnostics_DebuggableAttribute_DebuggingModes__EnableEditAndContinue);
			if ((object)fieldSymbol4 == null || !fieldSymbol4.HasConstantValue)
			{
				return null;
			}
			num |= fieldSymbol4.GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false).Int32Value;
		}
		TypedConstant item = new TypedConstant(wellKnownType, TypedConstantKind.Enum, num);
		return TrySynthesizeAttribute(WellKnownMember.System_Diagnostics_DebuggableAttribute__ctorDebuggingModes, ImmutableArray.Create(item));
	}

	internal SynthesizedAttributeData? SynthesizeDynamicAttribute(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type, int customModifiersCount, RefKind refKindOpt = RefKind.None)
	{
		if (type.IsDynamic() && refKindOpt == RefKind.None && customModifiersCount == 0)
		{
			return TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_DynamicAttribute__ctor);
		}
		Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol specialType = GetSpecialType(SpecialType.System_Boolean);
		ImmutableArray<TypedConstant> array = DynamicTransformsEncoder.Encode(type, refKindOpt, customModifiersCount, specialType);
		ImmutableArray<TypedConstant> arguments = ImmutableArray.Create(new TypedConstant(Microsoft.CodeAnalysis.CSharp.Symbols.ArrayTypeSymbol.CreateSZArray(specialType.ContainingAssembly, TypeWithAnnotations.Create(specialType)), array));
		return TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_DynamicAttribute__ctorTransformFlags, arguments);
	}

	internal SynthesizedAttributeData? SynthesizeTupleNamesAttribute(Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol type)
	{
		Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol specialType = GetSpecialType(SpecialType.System_String);
		ImmutableArray<TypedConstant> array = TupleNamesEncoder.Encode(type, specialType);
		ImmutableArray<TypedConstant> arguments = ImmutableArray.Create(new TypedConstant(Microsoft.CodeAnalysis.CSharp.Symbols.ArrayTypeSymbol.CreateSZArray(specialType.ContainingAssembly, TypeWithAnnotations.Create(specialType)), array));
		return TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_TupleElementNamesAttribute__ctorTransformNames, arguments);
	}

	internal SynthesizedAttributeData? SynthesizeAttributeUsageAttribute(AttributeTargets targets, bool allowMultiple, bool inherited)
	{
		Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol wellKnownType = GetWellKnownType(WellKnownType.System_AttributeTargets);
		Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol specialType = GetSpecialType(SpecialType.System_Boolean);
		ImmutableArray<TypedConstant> arguments = ImmutableArray.Create(new TypedConstant(wellKnownType, TypedConstantKind.Enum, targets));
		ImmutableArray<KeyValuePair<WellKnownMember, TypedConstant>> namedArguments = ImmutableArray.Create(new KeyValuePair<WellKnownMember, TypedConstant>(WellKnownMember.System_AttributeUsageAttribute__AllowMultiple, new TypedConstant(specialType, TypedConstantKind.Primitive, allowMultiple)), new KeyValuePair<WellKnownMember, TypedConstant>(WellKnownMember.System_AttributeUsageAttribute__Inherited, new TypedConstant(specialType, TypedConstantKind.Primitive, inherited)));
		return TrySynthesizeAttribute(WellKnownMember.System_AttributeUsageAttribute__ctor, arguments, namedArguments);
	}
}
