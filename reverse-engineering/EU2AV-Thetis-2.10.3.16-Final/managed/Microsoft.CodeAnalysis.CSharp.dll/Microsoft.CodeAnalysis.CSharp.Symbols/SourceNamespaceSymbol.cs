using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceNamespaceSymbol : NamespaceSymbol
{
	private sealed class AliasesAndUsings
	{
		private class ExternAliasesAndDiagnostics
		{
			public static readonly ExternAliasesAndDiagnostics Empty = new ExternAliasesAndDiagnostics
			{
				ExternAliases = ImmutableArray<AliasAndExternAliasDirective>.Empty,
				Diagnostics = ImmutableArray<Diagnostic>.Empty
			};

			public ImmutableArray<AliasAndExternAliasDirective> ExternAliases { get; init; }

			public ImmutableArray<Diagnostic> Diagnostics { get; init; }
		}

		private class UsingsAndDiagnostics
		{
			public static readonly UsingsAndDiagnostics Empty = new UsingsAndDiagnostics
			{
				UsingAliases = ImmutableArray<AliasAndUsingDirective>.Empty,
				UsingAliasesMap = null,
				UsingNamespacesOrTypes = ImmutableArray<NamespaceOrTypeAndUsingDirective>.Empty,
				Diagnostics = null
			};

			public ImmutableArray<AliasAndUsingDirective> UsingAliases { get; init; }

			public ImmutableDictionary<string, AliasAndUsingDirective>? UsingAliasesMap { get; init; }

			public ImmutableArray<NamespaceOrTypeAndUsingDirective> UsingNamespacesOrTypes { get; init; }

			public DiagnosticBag? Diagnostics { get; init; }
		}

		private ExternAliasesAndDiagnostics? _lazyExternAliases;

		private UsingsAndDiagnostics? _lazyGlobalUsings;

		private UsingsAndDiagnostics? _lazyUsings;

		private Imports? _lazyImports;

		private SymbolCompletionState _state;

		internal ImmutableArray<AliasAndExternAliasDirective> GetExternAliases(SourceNamespaceSymbol declaringSymbol, CSharpSyntaxNode declarationSyntax)
		{
			return GetExternAliasesAndDiagnostics(declaringSymbol, declarationSyntax).ExternAliases;
		}

		internal ImmutableArray<AliasAndExternAliasDirective> GetExternAliases(SourceNamespaceSymbol declaringSymbol, SyntaxReference declarationSyntax)
		{
			return (_lazyExternAliases ?? GetExternAliasesAndDiagnostics(declaringSymbol, (CSharpSyntaxNode)declarationSyntax.GetSyntax())).ExternAliases;
		}

		private ExternAliasesAndDiagnostics GetExternAliasesAndDiagnostics(SourceNamespaceSymbol declaringSymbol, CSharpSyntaxNode declarationSyntax)
		{
			if (_lazyExternAliases == null)
			{
				SyntaxList<ExternAliasDirectiveSyntax> externs;
				if (!(declarationSyntax is CompilationUnitSyntax compilationUnitSyntax))
				{
					if (!(declarationSyntax is BaseNamespaceDeclarationSyntax baseNamespaceDeclarationSyntax))
					{
						throw ExceptionUtilities.UnexpectedValue(declarationSyntax);
					}
					externs = baseNamespaceDeclarationSyntax.Externs;
				}
				else
				{
					externs = compilationUnitSyntax.Externs;
				}
				if (!externs.Any())
				{
					_lazyExternAliases = ExternAliasesAndDiagnostics.Empty;
				}
				else
				{
					DiagnosticBag instance = DiagnosticBag.GetInstance();
					Interlocked.CompareExchange(ref _lazyExternAliases, new ExternAliasesAndDiagnostics
					{
						ExternAliases = buildExternAliases(externs, declaringSymbol, instance),
						Diagnostics = instance.ToReadOnlyAndFree()
					}, null);
				}
			}
			return _lazyExternAliases;
			static ImmutableArray<AliasAndExternAliasDirective> buildExternAliases(SyntaxList<ExternAliasDirectiveSyntax> syntaxList, SourceNamespaceSymbol sourceNamespaceSymbol, DiagnosticBag diagnostics)
			{
				CSharpCompilation declaringCompilation = sourceNamespaceSymbol.DeclaringCompilation;
				ArrayBuilder<AliasAndExternAliasDirective> instance2 = ArrayBuilder<AliasAndExternAliasDirective>.GetInstance();
				foreach (ExternAliasDirectiveSyntax item in syntaxList)
				{
					declaringCompilation.RecordImport(item);
					bool skipInLookup = false;
					if (declaringCompilation.IsSubmission)
					{
						diagnostics.Add(ErrorCode.ERR_ExternAliasNotAllowed, item.Location);
						skipInLookup = true;
					}
					else
					{
						foreach (AliasAndExternAliasDirective item2 in instance2)
						{
							if (item2.Alias.Name == item.Identifier.ValueText)
							{
								diagnostics.Add(ErrorCode.ERR_DuplicateAlias, item2.Alias.GetFirstLocation(), item2.Alias.Name);
								break;
							}
						}
						if (item.Identifier.ContextualKind() == SyntaxKind.GlobalKeyword)
						{
							diagnostics.Add(ErrorCode.ERR_GlobalExternAlias, item.Identifier.GetLocation());
						}
					}
					instance2.Add(new AliasAndExternAliasDirective(new AliasSymbolFromSyntax(sourceNamespaceSymbol, item), item, skipInLookup));
				}
				return instance2.ToImmutableAndFree();
			}
		}

		internal ImmutableArray<AliasAndUsingDirective> GetUsingAliases(SourceNamespaceSymbol declaringSymbol, CSharpSyntaxNode declarationSyntax, ConsList<TypeSymbol>? basesBeingResolved)
		{
			return GetUsingsAndDiagnostics(declaringSymbol, declarationSyntax, basesBeingResolved).UsingAliases;
		}

		internal ImmutableArray<AliasAndUsingDirective> GetGlobalUsingAliases(SourceNamespaceSymbol declaringSymbol, CSharpSyntaxNode declarationSyntax, ConsList<TypeSymbol>? basesBeingResolved)
		{
			return GetGlobalUsingsAndDiagnostics(declaringSymbol, declarationSyntax, basesBeingResolved).UsingAliases;
		}

		internal ImmutableDictionary<string, AliasAndUsingDirective> GetUsingAliasesMap(SourceNamespaceSymbol declaringSymbol, CSharpSyntaxNode declarationSyntax, ConsList<TypeSymbol>? basesBeingResolved)
		{
			return GetUsingsAndDiagnostics(declaringSymbol, declarationSyntax, basesBeingResolved).UsingAliasesMap ?? ImmutableDictionary<string, AliasAndUsingDirective>.Empty;
		}

		internal ImmutableDictionary<string, AliasAndUsingDirective> GetGlobalUsingAliasesMap(SourceNamespaceSymbol declaringSymbol, SyntaxReference declarationSyntax, ConsList<TypeSymbol>? basesBeingResolved)
		{
			return (_lazyGlobalUsings ?? GetGlobalUsingsAndDiagnostics(declaringSymbol, (CSharpSyntaxNode)declarationSyntax.GetSyntax(), basesBeingResolved)).UsingAliasesMap ?? ImmutableDictionary<string, AliasAndUsingDirective>.Empty;
		}

		internal ImmutableArray<NamespaceOrTypeAndUsingDirective> GetUsingNamespacesOrTypes(SourceNamespaceSymbol declaringSymbol, CSharpSyntaxNode declarationSyntax, ConsList<TypeSymbol>? basesBeingResolved)
		{
			return GetUsingsAndDiagnostics(declaringSymbol, declarationSyntax, basesBeingResolved).UsingNamespacesOrTypes;
		}

		private UsingsAndDiagnostics GetUsingsAndDiagnostics(SourceNamespaceSymbol declaringSymbol, CSharpSyntaxNode declarationSyntax, ConsList<TypeSymbol>? basesBeingResolved)
		{
			return GetUsingsAndDiagnostics(ref _lazyUsings, declaringSymbol, declarationSyntax, basesBeingResolved, onlyGlobal: false);
		}

		internal ImmutableArray<NamespaceOrTypeAndUsingDirective> GetGlobalUsingNamespacesOrTypes(SourceNamespaceSymbol declaringSymbol, SyntaxReference declarationSyntax, ConsList<TypeSymbol>? basesBeingResolved)
		{
			return (_lazyGlobalUsings ?? GetGlobalUsingsAndDiagnostics(declaringSymbol, (CSharpSyntaxNode)declarationSyntax.GetSyntax(), basesBeingResolved)).UsingNamespacesOrTypes;
		}

		private UsingsAndDiagnostics GetGlobalUsingsAndDiagnostics(SourceNamespaceSymbol declaringSymbol, CSharpSyntaxNode declarationSyntax, ConsList<TypeSymbol>? basesBeingResolved)
		{
			return GetUsingsAndDiagnostics(ref _lazyGlobalUsings, declaringSymbol, declarationSyntax, basesBeingResolved, onlyGlobal: true);
		}

		private UsingsAndDiagnostics GetUsingsAndDiagnostics(ref UsingsAndDiagnostics? usings, SourceNamespaceSymbol declaringSymbol, CSharpSyntaxNode declarationSyntax, ConsList<TypeSymbol>? basesBeingResolved, bool onlyGlobal)
		{
			if (usings == null)
			{
				bool? flag;
				SyntaxList<UsingDirectiveSyntax> usings2;
				if (!(declarationSyntax is CompilationUnitSyntax compilationUnitSyntax))
				{
					if (!(declarationSyntax is BaseNamespaceDeclarationSyntax baseNamespaceDeclarationSyntax))
					{
						throw ExceptionUtilities.UnexpectedValue(declarationSyntax);
					}
					flag = null;
					usings2 = baseNamespaceDeclarationSyntax.Usings;
				}
				else
				{
					flag = onlyGlobal;
					usings2 = compilationUnitSyntax.Usings;
				}
				UsingsAndDiagnostics value = (usings2.Any() ? buildUsings(usings2, declaringSymbol, declarationSyntax, flag, basesBeingResolved) : ((flag == false) ? new UsingsAndDiagnostics
				{
					UsingAliases = GetGlobalUsingAliases(declaringSymbol, declarationSyntax, basesBeingResolved),
					UsingAliasesMap = declaringSymbol.GetGlobalUsingAliasesMap(basesBeingResolved),
					UsingNamespacesOrTypes = declaringSymbol.GetGlobalUsingNamespacesOrTypes(basesBeingResolved),
					Diagnostics = null
				} : UsingsAndDiagnostics.Empty));
				Interlocked.CompareExchange(ref usings, value, null);
			}
			return usings;
			UsingsAndDiagnostics buildUsings(SyntaxList<UsingDirectiveSyntax> usingDirectives, SourceNamespaceSymbol sourceNamespaceSymbol, CSharpSyntaxNode cSharpSyntaxNode, bool? applyIsGlobalFilter, ConsList<TypeSymbol>? basesBeingResolved2)
			{
				ImmutableArray<AliasAndExternAliasDirective> externAliases = GetExternAliases(sourceNamespaceSymbol, cSharpSyntaxNode);
				ImmutableDictionary<string, AliasAndUsingDirective> immutableDictionary = ImmutableDictionary<string, AliasAndUsingDirective>.Empty;
				ImmutableArray<NamespaceOrTypeAndUsingDirective> immutableArray = ImmutableArray<NamespaceOrTypeAndUsingDirective>.Empty;
				ImmutableArray<AliasAndUsingDirective> immutableArray2 = ImmutableArray<AliasAndUsingDirective>.Empty;
				if (applyIsGlobalFilter == false)
				{
					immutableDictionary = sourceNamespaceSymbol.GetGlobalUsingAliasesMap(basesBeingResolved2);
					immutableArray = sourceNamespaceSymbol.GetGlobalUsingNamespacesOrTypes(basesBeingResolved2);
					immutableArray2 = GetGlobalUsingAliases(sourceNamespaceSymbol, cSharpSyntaxNode, basesBeingResolved2);
				}
				DiagnosticBag diagnosticBag = new DiagnosticBag();
				CSharpCompilation declaringCompilation = sourceNamespaceSymbol.DeclaringCompilation;
				ArrayBuilder<NamespaceOrTypeAndUsingDirective> usings3 = null;
				ImmutableDictionary<string, AliasAndUsingDirective>.Builder builder = null;
				ArrayBuilder<AliasAndUsingDirective> arrayBuilder = null;
				Binder binder = null;
				PooledHashSet<NamespaceOrTypeSymbol> uniqueUsings = null;
				PooledHashSet<NamespaceOrTypeSymbol> uniqueUsings2 = null;
				foreach (UsingDirectiveSyntax item in usingDirectives)
				{
					if (!applyIsGlobalFilter.HasValue || item.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword) == (applyIsGlobalFilter == true))
					{
						declaringCompilation.RecordImport(item);
						if (item.Alias != null)
						{
							SyntaxToken identifier = item.Alias.Name.Identifier;
							Location location = item.Alias.Name.Location;
							if (identifier.ContextualKind() == SyntaxKind.GlobalKeyword)
							{
								diagnosticBag.Add(ErrorCode.WRN_GlobalAliasDefn, location);
							}
							if (item.StaticKeyword != default(SyntaxToken))
							{
								diagnosticBag.Add(ErrorCode.ERR_NoAliasHere, location);
							}
							SourceMemberContainerTypeSymbol.ReportReservedTypeName(identifier.Text, declaringCompilation, diagnosticBag, location);
							string valueText = identifier.ValueText;
							bool flag2 = false;
							if (builder?.ContainsKey(valueText) ?? immutableDictionary.ContainsKey(valueText))
							{
								flag2 = true;
								if (!item.NamespaceOrType.IsMissing)
								{
									diagnosticBag.Add(ErrorCode.ERR_DuplicateAlias, location, valueText);
								}
							}
							else
							{
								foreach (AliasAndExternAliasDirective item2 in externAliases)
								{
									if (item2.Alias.Name == valueText)
									{
										diagnosticBag.Add(ErrorCode.ERR_DuplicateAlias, item.Location, valueText);
										break;
									}
								}
							}
							AliasAndUsingDirective aliasAndUsingDirective = new AliasAndUsingDirective(new AliasSymbolFromSyntax(sourceNamespaceSymbol, item), item);
							if (arrayBuilder == null)
							{
								arrayBuilder = ArrayBuilder<AliasAndUsingDirective>.GetInstance();
								arrayBuilder.AddRange(immutableArray2);
							}
							arrayBuilder.Add(aliasAndUsingDirective);
							if (!flag2)
							{
								if (builder == null)
								{
									builder = immutableDictionary.ToBuilder();
								}
								builder.Add(valueText, aliasAndUsingDirective);
							}
						}
						else if (!item.NamespaceOrType.IsMissing)
						{
							BinderFlags binderFlags = BinderFlags.SuppressConstraintChecks;
							if (item.UnsafeKeyword != default(SyntaxToken))
							{
								Location location2 = item.UnsafeKeyword.GetLocation();
								if (item.StaticKeyword == default(SyntaxToken))
								{
									diagnosticBag.Add(ErrorCode.ERR_BadUnsafeInUsingDirective, location2);
								}
								else
								{
									MessageID.IDS_FeatureUsingTypeAlias.CheckFeatureAvailability(diagnosticBag, item, location2);
									sourceNamespaceSymbol.CheckUnsafeModifier(DeclarationModifiers.Unsafe, location2, diagnosticBag);
								}
								binderFlags |= BinderFlags.UnsafeRegion;
							}
							else if (!declaringCompilation.IsFeatureEnabled(MessageID.IDS_FeatureUsingTypeAlias))
							{
								binderFlags |= BinderFlags.UnsafeRegion;
							}
							BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
							if (binder == null)
							{
								binder = declaringCompilation.GetBinderFactory(cSharpSyntaxNode.SyntaxTree).GetBinder(item.NamespaceOrType).WithAdditionalFlags(binderFlags);
							}
							NamespaceOrTypeSymbol namespaceOrTypeSymbol = binder.BindNamespaceOrTypeSymbol(item.NamespaceOrType, instance, basesBeingResolved2).NamespaceOrTypeSymbol;
							bool flag3 = true;
							if (namespaceOrTypeSymbol.Kind == SymbolKind.Namespace)
							{
								if (item.StaticKeyword != default(SyntaxToken))
								{
									diagnosticBag.Add(ErrorCode.ERR_BadUsingType, item.NamespaceOrType.Location, namespaceOrTypeSymbol);
								}
								else if (!getOrCreateUniqueUsings(ref uniqueUsings, immutableArray).Add(namespaceOrTypeSymbol))
								{
									diagnosticBag.Add((!immutableArray.IsEmpty && getOrCreateUniqueGlobalUsingsNotInTree(ref uniqueUsings2, immutableArray, cSharpSyntaxNode.SyntaxTree).Contains(namespaceOrTypeSymbol)) ? ErrorCode.HDN_DuplicateWithGlobalUsing : ErrorCode.WRN_DuplicateUsing, item.NamespaceOrType.Location, namespaceOrTypeSymbol);
								}
								else
								{
									getOrCreateUsingsBuilder(ref usings3, immutableArray).Add(new NamespaceOrTypeAndUsingDirective(namespaceOrTypeSymbol, item, default(ImmutableArray<AssemblySymbol>)));
								}
							}
							else if (namespaceOrTypeSymbol.Kind == SymbolKind.NamedType)
							{
								if (item.StaticKeyword == default(SyntaxToken))
								{
									diagnosticBag.Add(ErrorCode.ERR_BadUsingNamespace, item.NamespaceOrType.Location, namespaceOrTypeSymbol);
								}
								else
								{
									NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)namespaceOrTypeSymbol;
									if (item.GlobalKeyword != default(SyntaxToken) && namedTypeSymbol.HasFileLocalTypes())
									{
										diagnosticBag.Add(ErrorCode.ERR_GlobalUsingStaticFileType, item.NamespaceOrType.Location, namespaceOrTypeSymbol);
									}
									if (!getOrCreateUniqueUsings(ref uniqueUsings, immutableArray).Add(namedTypeSymbol))
									{
										diagnosticBag.Add((!immutableArray.IsEmpty && getOrCreateUniqueGlobalUsingsNotInTree(ref uniqueUsings2, immutableArray, cSharpSyntaxNode.SyntaxTree).Contains(namespaceOrTypeSymbol)) ? ErrorCode.HDN_DuplicateWithGlobalUsing : ErrorCode.WRN_DuplicateUsing, item.NamespaceOrType.Location, namedTypeSymbol);
									}
									else
									{
										binder.ReportDiagnosticsIfObsolete(diagnosticBag, namedTypeSymbol, item.NamespaceOrType, hasBaseReceiver: false);
										getOrCreateUsingsBuilder(ref usings3, immutableArray).Add(new NamespaceOrTypeAndUsingDirective(namedTypeSymbol, item, instance.DependenciesBag.ToImmutableArray()));
									}
								}
							}
							else
							{
								bool flag4;
								switch (namespaceOrTypeSymbol.Kind)
								{
								case SymbolKind.ArrayType:
								case SymbolKind.DynamicType:
								case SymbolKind.PointerType:
								case SymbolKind.FunctionPointerType:
									flag4 = true;
									break;
								default:
									flag4 = false;
									break;
								}
								if (flag4)
								{
									diagnosticBag.Add(ErrorCode.ERR_BadUsingStaticType, item.NamespaceOrType.Location, namespaceOrTypeSymbol.GetKindText());
									flag3 = false;
								}
								else if (namespaceOrTypeSymbol.Kind != SymbolKind.ErrorType)
								{
									diagnosticBag.Add(ErrorCode.ERR_BadSKknown, item.NamespaceOrType.Location, item.NamespaceOrType, namespaceOrTypeSymbol.GetKindText(), MessageID.IDS_SK_TYPE_OR_NAMESPACE.Localize());
								}
							}
							if (flag3)
							{
								diagnosticBag.AddRange(instance.DiagnosticBag);
							}
							instance.Free();
						}
					}
				}
				uniqueUsings?.Free();
				uniqueUsings2?.Free();
				if (diagnosticBag.IsEmptyWithoutResolution)
				{
					diagnosticBag = null;
				}
				return new UsingsAndDiagnostics
				{
					UsingAliases = (arrayBuilder?.ToImmutableAndFree() ?? immutableArray2),
					UsingAliasesMap = (builder?.ToImmutable() ?? immutableDictionary),
					UsingNamespacesOrTypes = (usings3?.ToImmutableAndFree() ?? immutableArray),
					Diagnostics = diagnosticBag
				};
			}
			static PooledHashSet<NamespaceOrTypeSymbol> getOrCreateUniqueGlobalUsingsNotInTree(ref PooledHashSet<NamespaceOrTypeSymbol>? uniqueUsings, ImmutableArray<NamespaceOrTypeAndUsingDirective> globalUsingNamespacesOrTypes, SyntaxTree tree)
			{
				if (uniqueUsings == null)
				{
					uniqueUsings = SpecializedSymbolCollections.GetPooledSymbolHashSetInstance<NamespaceOrTypeSymbol>();
					uniqueUsings.AddAll(from n in globalUsingNamespacesOrTypes
						where n.UsingDirectiveReference?.SyntaxTree != tree
						select n.NamespaceOrType);
				}
				return uniqueUsings;
			}
			static PooledHashSet<NamespaceOrTypeSymbol> getOrCreateUniqueUsings(ref PooledHashSet<NamespaceOrTypeSymbol>? uniqueUsings, ImmutableArray<NamespaceOrTypeAndUsingDirective> globalUsingNamespacesOrTypes)
			{
				if (uniqueUsings == null)
				{
					uniqueUsings = SpecializedSymbolCollections.GetPooledSymbolHashSetInstance<NamespaceOrTypeSymbol>();
					uniqueUsings.AddAll(globalUsingNamespacesOrTypes.Select((NamespaceOrTypeAndUsingDirective n) => n.NamespaceOrType));
				}
				return uniqueUsings;
			}
			static ArrayBuilder<NamespaceOrTypeAndUsingDirective> getOrCreateUsingsBuilder(ref ArrayBuilder<NamespaceOrTypeAndUsingDirective>? reference, ImmutableArray<NamespaceOrTypeAndUsingDirective> globalUsingNamespacesOrTypes)
			{
				if (reference == null)
				{
					reference = ArrayBuilder<NamespaceOrTypeAndUsingDirective>.GetInstance();
					reference.AddRange(globalUsingNamespacesOrTypes);
				}
				return reference;
			}
		}

		internal Imports GetImports(SourceNamespaceSymbol declaringSymbol, CSharpSyntaxNode declarationSyntax, ConsList<TypeSymbol>? basesBeingResolved)
		{
			if (_lazyImports == null)
			{
				Interlocked.CompareExchange(ref _lazyImports, Imports.Create(GetUsingAliasesMap(declaringSymbol, declarationSyntax, basesBeingResolved), GetUsingNamespacesOrTypes(declaringSymbol, declarationSyntax, basesBeingResolved), GetExternAliases(declaringSymbol, declarationSyntax)), null);
			}
			return _lazyImports;
		}

		internal void Complete(SourceNamespaceSymbol declaringSymbol, SyntaxReference declarationSyntax, CancellationToken cancellationToken)
		{
			ExternAliasesAndDiagnostics externAliasesAndDiagnostics = _lazyExternAliases ?? GetExternAliasesAndDiagnostics(declaringSymbol, (CSharpSyntaxNode)declarationSyntax.GetSyntax(cancellationToken));
			cancellationToken.ThrowIfCancellationRequested();
			UsingsAndDiagnostics usingsAndDiagnostics = _lazyGlobalUsings ?? (declaringSymbol.IsGlobalNamespace ? GetGlobalUsingsAndDiagnostics(declaringSymbol, (CSharpSyntaxNode)declarationSyntax.GetSyntax(cancellationToken), null) : UsingsAndDiagnostics.Empty);
			cancellationToken.ThrowIfCancellationRequested();
			UsingsAndDiagnostics usingsAndDiagnostics2 = _lazyUsings ?? GetUsingsAndDiagnostics(declaringSymbol, (CSharpSyntaxNode)declarationSyntax.GetSyntax(cancellationToken), null);
			cancellationToken.ThrowIfCancellationRequested();
			while (true)
			{
				cancellationToken.ThrowIfCancellationRequested();
				CompletionPart nextIncompletePart = _state.NextIncompletePart;
				switch (nextIncompletePart)
				{
				case CompletionPart.StartBaseType:
					if (_state.NotePartComplete(CompletionPart.StartBaseType))
					{
						Validate(declaringSymbol, declarationSyntax, externAliasesAndDiagnostics, usingsAndDiagnostics2, usingsAndDiagnostics.Diagnostics);
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

		private static void Validate(SourceNamespaceSymbol declaringSymbol, SyntaxReference declarationSyntax, ExternAliasesAndDiagnostics externAliasesAndDiagnostics, UsingsAndDiagnostics usingsAndDiagnostics, DiagnosticBag? globalUsingDiagnostics)
		{
			CSharpCompilation compilation = declaringSymbol.DeclaringCompilation;
			DiagnosticBag declarationDiagnostics = compilation.DeclarationDiagnostics;
			BindingDiagnosticBag diagnostics = BindingDiagnosticBag.GetInstance();
			if (usingsAndDiagnostics.UsingAliasesMap != null)
			{
				foreach (var (_, aliasAndUsingDirective2) in usingsAndDiagnostics.UsingAliasesMap)
				{
					if (aliasAndUsingDirective2.UsingDirectiveReference.SyntaxTree == declarationSyntax.SyntaxTree)
					{
						NamespaceOrTypeSymbol aliasTarget = aliasAndUsingDirective2.Alias.GetAliasTarget(null);
						diagnostics.Clear();
						if (aliasAndUsingDirective2.Alias is AliasSymbolFromSyntax aliasSymbolFromSyntax)
						{
							diagnostics.AddRange(aliasSymbolFromSyntax.AliasTargetDiagnostics);
						}
						aliasAndUsingDirective2.Alias.CheckConstraints(diagnostics);
						declarationDiagnostics.AddRange(diagnostics.DiagnosticBag);
						recordImportDependencies(aliasAndUsingDirective2.UsingDirective, aliasTarget);
					}
				}
			}
			TypeConversions typeConversions = compilation.SourceAssembly.CorLibrary.TypeConversions;
			foreach (NamespaceOrTypeAndUsingDirective usingNamespacesOrType in usingsAndDiagnostics.UsingNamespacesOrTypes)
			{
				if (usingNamespacesOrType.UsingDirectiveReference.SyntaxTree == declarationSyntax.SyntaxTree)
				{
					diagnostics.Clear();
					diagnostics.AddDependencies(usingNamespacesOrType.Dependencies);
					NamespaceOrTypeSymbol namespaceOrType = usingNamespacesOrType.NamespaceOrType;
					UsingDirectiveSyntax usingDirective = usingNamespacesOrType.UsingDirective;
					if (namespaceOrType.IsType)
					{
						((TypeSymbol)namespaceOrType).CheckAllConstraints(location: usingDirective.NamespaceOrType.Location, compilation: compilation, conversions: typeConversions, diagnostics: diagnostics);
					}
					declarationDiagnostics.AddRange(diagnostics.DiagnosticBag);
					recordImportDependencies(usingDirective, namespaceOrType);
				}
			}
			foreach (AliasAndExternAliasDirective externAlias in externAliasesAndDiagnostics.ExternAliases)
			{
				if (!externAlias.SkipInLookup)
				{
					NamespaceSymbol ns = (NamespaceSymbol)externAlias.Alias.GetAliasTarget(null);
					if (externAlias.Alias is AliasSymbolFromSyntax aliasSymbolFromSyntax2)
					{
						declarationDiagnostics.AddRange(aliasSymbolFromSyntax2.AliasTargetDiagnostics.DiagnosticBag);
					}
					if (!Compilation.ReportUnusedImportsInTree(externAlias.ExternAliasDirective.SyntaxTree))
					{
						diagnostics.Clear();
						diagnostics.AddAssembliesUsedByNamespaceReference(ns);
						compilation.AddUsedAssemblies(diagnostics.DependenciesBag);
					}
				}
			}
			declarationDiagnostics.AddRange(externAliasesAndDiagnostics.Diagnostics);
			DiagnosticBag? diagnostics2 = usingsAndDiagnostics.Diagnostics;
			if (diagnostics2 != null && !diagnostics2.IsEmptyWithoutResolution)
			{
				declarationDiagnostics.AddRange(usingsAndDiagnostics.Diagnostics.AsEnumerable());
			}
			if (globalUsingDiagnostics != null && !globalUsingDiagnostics.IsEmptyWithoutResolution)
			{
				declarationDiagnostics.AddRange(globalUsingDiagnostics.AsEnumerable());
			}
			diagnostics.Free();
			void recordImportDependencies(UsingDirectiveSyntax usingDirectiveSyntax, NamespaceOrTypeSymbol target)
			{
				if (Compilation.ReportUnusedImportsInTree(usingDirectiveSyntax.SyntaxTree))
				{
					compilation.RecordImportDependencies(usingDirectiveSyntax, diagnostics.DependenciesBag.ToImmutableArray());
				}
				else
				{
					if (target.IsNamespace)
					{
						diagnostics.AddAssembliesUsedByNamespaceReference((NamespaceSymbol)target);
					}
					compilation.AddUsedAssemblies(diagnostics.DependenciesBag);
				}
			}
		}
	}

	private class MergedGlobalAliasesAndUsings
	{
		private Imports? _lazyImports;

		private SymbolCompletionState _state;

		public static readonly MergedGlobalAliasesAndUsings Empty = new MergedGlobalAliasesAndUsings
		{
			UsingAliasesMap = ImmutableDictionary<string, AliasAndUsingDirective>.Empty,
			UsingNamespacesOrTypes = ImmutableArray<NamespaceOrTypeAndUsingDirective>.Empty,
			Diagnostics = ImmutableArray<Diagnostic>.Empty,
			_lazyImports = Microsoft.CodeAnalysis.CSharp.Imports.Empty
		};

		public ImmutableDictionary<string, AliasAndUsingDirective>? UsingAliasesMap { get; init; }

		public ImmutableArray<NamespaceOrTypeAndUsingDirective> UsingNamespacesOrTypes { get; init; }

		public ImmutableArray<Diagnostic> Diagnostics { get; init; }

		public Imports Imports
		{
			get
			{
				if (_lazyImports == null)
				{
					Interlocked.CompareExchange(ref _lazyImports, Microsoft.CodeAnalysis.CSharp.Imports.Create(UsingAliasesMap ?? ImmutableDictionary<string, AliasAndUsingDirective>.Empty, UsingNamespacesOrTypes, ImmutableArray<AliasAndExternAliasDirective>.Empty), null);
				}
				return _lazyImports;
			}
		}

		internal void Complete(SourceNamespaceSymbol declaringSymbol, CancellationToken cancellationToken)
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
						if (!Diagnostics.IsDefaultOrEmpty)
						{
							declaringSymbol.DeclaringCompilation.DeclarationDiagnostics.AddRange(Diagnostics);
						}
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
	}

	private static readonly ImmutableDictionary<SingleNamespaceDeclaration, AliasesAndUsings> s_emptyMap = ImmutableDictionary<SingleNamespaceDeclaration, AliasesAndUsings>.Empty.WithComparers(ReferenceEqualityComparer.Instance);

	private readonly SourceModuleSymbol _module;

	private readonly Symbol _container;

	private readonly MergedNamespaceDeclaration _mergedDeclaration;

	private SymbolCompletionState _state;

	private ImmutableArray<Location> _locations;

	private Dictionary<ReadOnlyMemory<char>, ImmutableArray<NamespaceOrTypeSymbol>> _nameToMembersMap;

	private Dictionary<ReadOnlyMemory<char>, ImmutableArray<NamedTypeSymbol>> _nameToTypeMembersMap;

	private ImmutableArray<Symbol> _lazyAllMembers;

	private ImmutableArray<NamedTypeSymbol> _lazyTypeMembersUnordered;

	private ImmutableDictionary<SingleNamespaceDeclaration, AliasesAndUsings> _aliasesAndUsings_doNotAccessDirectly = s_emptyMap;

	private MergedGlobalAliasesAndUsings _lazyMergedGlobalAliasesAndUsings;

	private const int LazyAllMembersIsSorted = 1;

	private int _flags;

	private LexicalSortKey _lazyLexicalSortKey = LexicalSortKey.NotInitialized;

	private static readonly Func<SingleNamespaceDeclaration, SyntaxReference> s_declaringSyntaxReferencesSelector = (SingleNamespaceDeclaration d) => new NamespaceDeclarationSyntaxReference(d.SyntaxReference);

	internal MergedNamespaceDeclaration MergedDeclaration => _mergedDeclaration;

	public override Symbol ContainingSymbol => _container;

	public override AssemblySymbol ContainingAssembly => _module.ContainingAssembly;

	public override string Name => _mergedDeclaration.Name;

	public override ImmutableArray<Location> Locations
	{
		get
		{
			if (_locations.IsDefault)
			{
				ImmutableInterlocked.InterlockedCompareExchange(ref _locations, _mergedDeclaration.NameLocations, default(ImmutableArray<Location>));
			}
			return _locations;
		}
	}

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ComputeDeclaringReferencesCore();

	internal override ModuleSymbol ContainingModule => _module;

	internal override NamespaceExtent Extent => new NamespaceExtent(_module);

	public Imports GetImports(CSharpSyntaxNode declarationSyntax, ConsList<TypeSymbol>? basesBeingResolved)
	{
		if (!(declarationSyntax is CompilationUnitSyntax compilationUnitSyntax))
		{
			if (!(declarationSyntax is BaseNamespaceDeclarationSyntax baseNamespaceDeclarationSyntax))
			{
				throw ExceptionUtilities.UnexpectedValue(declarationSyntax);
			}
			if (!baseNamespaceDeclarationSyntax.Externs.Any() && !baseNamespaceDeclarationSyntax.Usings.Any())
			{
				return Imports.Empty;
			}
		}
		else if (!compilationUnitSyntax.Externs.Any() && !compilationUnitSyntax.Usings.Any())
		{
			return GetGlobalUsingImports(basesBeingResolved);
		}
		return GetAliasesAndUsings(declarationSyntax).GetImports(this, declarationSyntax, basesBeingResolved);
	}

	private AliasesAndUsings GetAliasesAndUsings(CSharpSyntaxNode declarationSyntax)
	{
		return GetAliasesAndUsings(GetMatchingNamespaceDeclaration(declarationSyntax));
	}

	private SingleNamespaceDeclaration GetMatchingNamespaceDeclaration(CSharpSyntaxNode declarationSyntax)
	{
		foreach (SingleNamespaceDeclaration declaration in _mergedDeclaration.Declarations)
		{
			SyntaxReference syntaxReference = declaration.SyntaxReference;
			if (syntaxReference.SyntaxTree == declarationSyntax.SyntaxTree && syntaxReference.GetSyntax() == declarationSyntax)
			{
				return declaration;
			}
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/SourceNamespaceSymbol.AliasesAndUsings.cs", 85);
	}

	private static AliasesAndUsings GetOrCreateAliasAndUsings(ref ImmutableDictionary<SingleNamespaceDeclaration, AliasesAndUsings> dictionary, SingleNamespaceDeclaration declaration)
	{
		return ImmutableInterlocked.GetOrAdd(ref dictionary, declaration, (SingleNamespaceDeclaration _) => new AliasesAndUsings());
	}

	private AliasesAndUsings GetAliasesAndUsings(SingleNamespaceDeclaration declaration)
	{
		return GetOrCreateAliasAndUsings(ref _aliasesAndUsings_doNotAccessDirectly, declaration);
	}

	public ImmutableArray<AliasAndExternAliasDirective> GetExternAliases(CSharpSyntaxNode declarationSyntax)
	{
		if (!(declarationSyntax is CompilationUnitSyntax compilationUnitSyntax))
		{
			if (!(declarationSyntax is BaseNamespaceDeclarationSyntax baseNamespaceDeclarationSyntax))
			{
				throw ExceptionUtilities.UnexpectedValue(declarationSyntax);
			}
			if (!baseNamespaceDeclarationSyntax.Externs.Any())
			{
				return ImmutableArray<AliasAndExternAliasDirective>.Empty;
			}
		}
		else if (!compilationUnitSyntax.Externs.Any())
		{
			return ImmutableArray<AliasAndExternAliasDirective>.Empty;
		}
		return GetAliasesAndUsings(declarationSyntax).GetExternAliases(this, declarationSyntax);
	}

	public ImmutableArray<AliasAndUsingDirective> GetUsingAliases(CSharpSyntaxNode declarationSyntax, ConsList<TypeSymbol>? basesBeingResolved)
	{
		if (!(declarationSyntax is CompilationUnitSyntax compilationUnitSyntax))
		{
			if (!(declarationSyntax is BaseNamespaceDeclarationSyntax baseNamespaceDeclarationSyntax))
			{
				throw ExceptionUtilities.UnexpectedValue(declarationSyntax);
			}
			if (!baseNamespaceDeclarationSyntax.Usings.Any())
			{
				return ImmutableArray<AliasAndUsingDirective>.Empty;
			}
		}
		else if (!compilationUnitSyntax.Usings.Any())
		{
			return ImmutableArray<AliasAndUsingDirective>.Empty;
		}
		return GetAliasesAndUsings(declarationSyntax).GetUsingAliases(this, declarationSyntax, basesBeingResolved);
	}

	public ImmutableDictionary<string, AliasAndUsingDirective> GetUsingAliasesMap(CSharpSyntaxNode declarationSyntax, ConsList<TypeSymbol>? basesBeingResolved)
	{
		if (!(declarationSyntax is CompilationUnitSyntax compilationUnitSyntax))
		{
			if (!(declarationSyntax is BaseNamespaceDeclarationSyntax baseNamespaceDeclarationSyntax))
			{
				throw ExceptionUtilities.UnexpectedValue(declarationSyntax);
			}
			if (!baseNamespaceDeclarationSyntax.Usings.Any())
			{
				return ImmutableDictionary<string, AliasAndUsingDirective>.Empty;
			}
		}
		else if (!compilationUnitSyntax.Usings.Any())
		{
			return GetGlobalUsingAliasesMap(basesBeingResolved);
		}
		return GetAliasesAndUsings(declarationSyntax).GetUsingAliasesMap(this, declarationSyntax, basesBeingResolved);
	}

	public ImmutableArray<NamespaceOrTypeAndUsingDirective> GetUsingNamespacesOrTypes(CSharpSyntaxNode declarationSyntax, ConsList<TypeSymbol>? basesBeingResolved)
	{
		if (!(declarationSyntax is CompilationUnitSyntax compilationUnitSyntax))
		{
			if (!(declarationSyntax is BaseNamespaceDeclarationSyntax baseNamespaceDeclarationSyntax))
			{
				throw ExceptionUtilities.UnexpectedValue(declarationSyntax);
			}
			if (!baseNamespaceDeclarationSyntax.Usings.Any())
			{
				return ImmutableArray<NamespaceOrTypeAndUsingDirective>.Empty;
			}
		}
		else if (!compilationUnitSyntax.Usings.Any())
		{
			return GetGlobalUsingNamespacesOrTypes(basesBeingResolved);
		}
		return GetAliasesAndUsings(declarationSyntax).GetUsingNamespacesOrTypes(this, declarationSyntax, basesBeingResolved);
	}

	private Imports GetGlobalUsingImports(ConsList<TypeSymbol>? basesBeingResolved)
	{
		return GetMergedGlobalAliasesAndUsings(basesBeingResolved).Imports;
	}

	private ImmutableDictionary<string, AliasAndUsingDirective> GetGlobalUsingAliasesMap(ConsList<TypeSymbol>? basesBeingResolved)
	{
		return GetMergedGlobalAliasesAndUsings(basesBeingResolved).UsingAliasesMap;
	}

	private ImmutableArray<NamespaceOrTypeAndUsingDirective> GetGlobalUsingNamespacesOrTypes(ConsList<TypeSymbol>? basesBeingResolved)
	{
		return GetMergedGlobalAliasesAndUsings(basesBeingResolved).UsingNamespacesOrTypes;
	}

	private MergedGlobalAliasesAndUsings GetMergedGlobalAliasesAndUsings(ConsList<TypeSymbol>? basesBeingResolved, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (_lazyMergedGlobalAliasesAndUsings == null)
		{
			if (!IsGlobalNamespace)
			{
				_lazyMergedGlobalAliasesAndUsings = MergedGlobalAliasesAndUsings.Empty;
			}
			else
			{
				ImmutableDictionary<string, AliasAndUsingDirective> immutableDictionary = null;
				ArrayBuilder<NamespaceOrTypeAndUsingDirective> arrayBuilder = ArrayBuilder<NamespaceOrTypeAndUsingDirective>.GetInstance();
				PooledHashSet<NamespaceOrTypeSymbol> pooledSymbolHashSetInstance = SpecializedSymbolCollections.GetPooledSymbolHashSetInstance<NamespaceOrTypeSymbol>();
				DiagnosticBag diagnosticBag = DiagnosticBag.GetInstance();
				try
				{
					bool flag = false;
					foreach (SingleNamespaceDeclaration declaration in _mergedDeclaration.Declarations)
					{
						if (declaration.HasExternAliases)
						{
							flag = true;
						}
						if (!declaration.HasGlobalUsings)
						{
							continue;
						}
						ImmutableDictionary<string, AliasAndUsingDirective> globalUsingAliasesMap = GetAliasesAndUsings(declaration).GetGlobalUsingAliasesMap(this, declaration.SyntaxReference, basesBeingResolved);
						cancellationToken.ThrowIfCancellationRequested();
						if (!globalUsingAliasesMap.IsEmpty)
						{
							if (immutableDictionary == null)
							{
								immutableDictionary = globalUsingAliasesMap;
							}
							else
							{
								ImmutableDictionary<string, AliasAndUsingDirective>.Builder builder = immutableDictionary.ToBuilder();
								bool flag2 = false;
								foreach (KeyValuePair<string, AliasAndUsingDirective> item in globalUsingAliasesMap)
								{
									if (builder.ContainsKey(item.Key))
									{
										diagnosticBag.Add(ErrorCode.ERR_DuplicateAlias, item.Value.Alias.GetFirstLocation(), item.Key);
									}
									else
									{
										builder.Add(item);
										flag2 = true;
									}
								}
								if (flag2)
								{
									immutableDictionary = builder.ToImmutable();
								}
								cancellationToken.ThrowIfCancellationRequested();
							}
						}
						ImmutableArray<NamespaceOrTypeAndUsingDirective> globalUsingNamespacesOrTypes = GetAliasesAndUsings(declaration).GetGlobalUsingNamespacesOrTypes(this, declaration.SyntaxReference, basesBeingResolved);
						if (!globalUsingNamespacesOrTypes.IsEmpty)
						{
							if (arrayBuilder.Count == 0)
							{
								arrayBuilder.AddRange(globalUsingNamespacesOrTypes);
								pooledSymbolHashSetInstance.AddAll(globalUsingNamespacesOrTypes.Select((NamespaceOrTypeAndUsingDirective n) => n.NamespaceOrType));
							}
							else
							{
								foreach (NamespaceOrTypeAndUsingDirective item2 in globalUsingNamespacesOrTypes)
								{
									if (!pooledSymbolHashSetInstance.Add(item2.NamespaceOrType))
									{
										diagnosticBag.Add(ErrorCode.HDN_DuplicateWithGlobalUsing, item2.UsingDirective.NamespaceOrType.Location, item2.NamespaceOrType);
									}
									else
									{
										arrayBuilder.Add(item2);
									}
								}
							}
						}
						cancellationToken.ThrowIfCancellationRequested();
					}
					if (flag && immutableDictionary != null)
					{
						foreach (SingleNamespaceDeclaration declaration2 in _mergedDeclaration.Declarations)
						{
							if (!declaration2.HasExternAliases)
							{
								continue;
							}
							ImmutableArray<AliasAndExternAliasDirective> externAliases = GetAliasesAndUsings(declaration2).GetExternAliases(this, declaration2.SyntaxReference);
							ImmutableDictionary<string, AliasAndUsingDirective> immutableDictionary2 = ImmutableDictionary<string, AliasAndUsingDirective>.Empty;
							if (declaration2.HasGlobalUsings)
							{
								immutableDictionary2 = GetAliasesAndUsings(declaration2).GetGlobalUsingAliasesMap(this, declaration2.SyntaxReference, basesBeingResolved);
							}
							foreach (AliasAndExternAliasDirective item3 in externAliases)
							{
								if (!item3.SkipInLookup && !immutableDictionary2.ContainsKey(item3.Alias.Name) && immutableDictionary.ContainsKey(item3.Alias.Name))
								{
									diagnosticBag.Add(ErrorCode.ERR_DuplicateAlias, item3.Alias.GetFirstLocation(), item3.Alias.Name);
								}
							}
						}
					}
					Interlocked.CompareExchange(ref _lazyMergedGlobalAliasesAndUsings, new MergedGlobalAliasesAndUsings
					{
						UsingAliasesMap = (immutableDictionary ?? ImmutableDictionary<string, AliasAndUsingDirective>.Empty),
						UsingNamespacesOrTypes = arrayBuilder.ToImmutableAndFree(),
						Diagnostics = diagnosticBag.ToReadOnlyAndFree()
					}, null);
					arrayBuilder = null;
					diagnosticBag = null;
				}
				finally
				{
					pooledSymbolHashSetInstance.Free();
					arrayBuilder?.Free();
					diagnosticBag?.Free();
				}
			}
		}
		return _lazyMergedGlobalAliasesAndUsings;
	}

	internal SourceNamespaceSymbol(SourceModuleSymbol module, Symbol container, MergedNamespaceDeclaration mergedDeclaration, BindingDiagnosticBag diagnostics)
	{
		_module = module;
		_container = container;
		_mergedDeclaration = mergedDeclaration;
		foreach (SingleNamespaceDeclaration declaration in mergedDeclaration.Declarations)
		{
			diagnostics.AddRange(declaration.Diagnostics);
		}
	}

	internal override LexicalSortKey GetLexicalSortKey()
	{
		if (!_lazyLexicalSortKey.IsInitialized)
		{
			_lazyLexicalSortKey.SetFrom(_mergedDeclaration.GetLexicalSortKey(DeclaringCompilation));
		}
		return _lazyLexicalSortKey;
	}

	public override Location? TryGetFirstLocation()
	{
		ImmutableArray<SingleNamespaceDeclaration> declarations = _mergedDeclaration.Declarations;
		if (declarations.Length >= 1)
		{
			SingleNamespaceDeclaration singleNamespaceDeclaration = declarations[0];
			return singleNamespaceDeclaration.NameLocation;
		}
		return null;
	}

	public override bool HasLocationContainedWithin(SyntaxTree tree, TextSpan declarationSpan, out bool wasZeroWidthMatch)
	{
		foreach (SingleNamespaceDeclaration declaration in _mergedDeclaration.Declarations)
		{
			if (Symbol.IsLocationContainedWithin(declaration.NameLocation, tree, declarationSpan, out wasZeroWidthMatch))
			{
				return true;
			}
		}
		wasZeroWidthMatch = false;
		return false;
	}

	private ImmutableArray<SyntaxReference> ComputeDeclaringReferencesCore()
	{
		return _mergedDeclaration.Declarations.SelectAsArray(s_declaringSyntaxReferencesSelector);
	}

	internal override ImmutableArray<Symbol> GetMembersUnordered()
	{
		ImmutableArray<Symbol> lazyAllMembers = _lazyAllMembers;
		if (lazyAllMembers.IsDefault)
		{
			ImmutableArray<Symbol> value = StaticCast<Symbol>.From(GetNameToMembersMap().Flatten());
			ImmutableInterlocked.InterlockedInitialize(ref _lazyAllMembers, value);
			lazyAllMembers = _lazyAllMembers;
		}
		return lazyAllMembers.ConditionallyDeOrder();
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if ((_flags & 1) != 0)
		{
			return _lazyAllMembers;
		}
		ImmutableArray<Symbol> immutableArray = GetMembersUnordered();
		if (immutableArray.Length >= 2)
		{
			immutableArray = immutableArray.Sort(LexicalOrderSymbolComparer.Instance);
			ImmutableInterlocked.InterlockedExchange(ref _lazyAllMembers, immutableArray);
		}
		ThreadSafeFlagOperations.Set(ref _flags, 1);
		return immutableArray;
	}

	public override ImmutableArray<Symbol> GetMembers(ReadOnlyMemory<char> name)
	{
		if (!GetNameToMembersMap().TryGetValue(name, out var value))
		{
			return ImmutableArray<Symbol>.Empty;
		}
		return value.Cast<NamespaceOrTypeSymbol, Symbol>();
	}

	internal override ImmutableArray<NamedTypeSymbol> GetTypeMembersUnordered()
	{
		if (_lazyTypeMembersUnordered.IsDefault)
		{
			ImmutableArray<NamedTypeSymbol> value = GetNameToTypeMembersMap().Flatten();
			ImmutableInterlocked.InterlockedInitialize(ref _lazyTypeMembersUnordered, value);
		}
		return _lazyTypeMembersUnordered;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		return GetNameToTypeMembersMap().Flatten(LexicalOrderSymbolComparer.Instance);
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name)
	{
		if (!GetNameToTypeMembersMap().TryGetValue(name, out var value))
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}
		return value;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name, int arity)
	{
		return GetTypeMembers(name).WhereAsArray((NamedTypeSymbol s, int num) => s.Arity == num, arity);
	}

	private Dictionary<ReadOnlyMemory<char>, ImmutableArray<NamespaceOrTypeSymbol>> GetNameToMembersMap()
	{
		if (_nameToMembersMap == null)
		{
			BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
			if (Interlocked.CompareExchange(ref _nameToMembersMap, MakeNameToMembersMap(instance), null) == null)
			{
				AddDeclarationDiagnostics(instance);
				RegisterDeclaredCorTypes();
				DeclaringCompilation.SymbolDeclaredEvent(this);
				_state.NotePartComplete(CompletionPart.Members);
			}
			instance.Free();
		}
		return _nameToMembersMap;
	}

	private Dictionary<ReadOnlyMemory<char>, ImmutableArray<NamedTypeSymbol>> GetNameToTypeMembersMap()
	{
		if (_nameToTypeMembersMap == null)
		{
			Interlocked.CompareExchange(ref _nameToTypeMembersMap, ImmutableArrayExtensions.GetTypesFromMemberMap<ReadOnlyMemory<char>, NamespaceOrTypeSymbol, NamedTypeSymbol>(GetNameToMembersMap(), ReadOnlyMemoryOfCharComparer.Instance), null);
		}
		return _nameToTypeMembersMap;
	}

	private Dictionary<ReadOnlyMemory<char>, ImmutableArray<NamespaceOrTypeSymbol>> MakeNameToMembersMap(BindingDiagnosticBag diagnostics)
	{
		PooledDictionary<ReadOnlyMemory<char>, object> pooledDictionary = NamespaceOrTypeSymbol.s_nameToObjectPool.Allocate();
		foreach (MergedNamespaceOrTypeDeclaration child in _mergedDeclaration.Children)
		{
			NamespaceOrTypeSymbol namespaceOrTypeSymbol = BuildSymbol(child, diagnostics);
			ImmutableArrayExtensions.AddToMultiValueDictionaryBuilder(pooledDictionary, System.MemoryExtensions.AsMemory(namespaceOrTypeSymbol.Name), namespaceOrTypeSymbol);
		}
		Dictionary<ReadOnlyMemory<char>, ImmutableArray<NamespaceOrTypeSymbol>> result = new Dictionary<ReadOnlyMemory<char>, ImmutableArray<NamespaceOrTypeSymbol>>(pooledDictionary.Count, ReadOnlyMemoryOfCharComparer.Instance);
		ImmutableArrayExtensions.CreateNameToMembersMap<ReadOnlyMemory<char>, NamespaceOrTypeSymbol, NamedTypeSymbol, NamespaceSymbol>(pooledDictionary, result);
		pooledDictionary.Free();
		CheckMembers(this, result, diagnostics);
		return result;
	}

	private static void CheckMembers(NamespaceSymbol @namespace, Dictionary<ReadOnlyMemory<char>, ImmutableArray<NamespaceOrTypeSymbol>> result, BindingDiagnosticBag diagnostics)
	{
		Symbol[] array = new Symbol[10];
		MergedNamespaceSymbol mergedNamespaceSymbol = null;
		if (@namespace.ContainingAssembly.Modules.Length > 1)
		{
			mergedNamespaceSymbol = @namespace.ContainingAssembly.GetAssemblyNamespace(@namespace) as MergedNamespaceSymbol;
		}
		foreach (ReadOnlyMemory<char> key in result.Keys)
		{
			Array.Clear(array, 0, array.Length);
			foreach (NamespaceOrTypeSymbol item2 in result[key])
			{
				SourceMemberContainerTypeSymbol sourceMemberContainerTypeSymbol = item2 as SourceMemberContainerTypeSymbol;
				int num = sourceMemberContainerTypeSymbol?.Arity ?? 0;
				if (num >= array.Length)
				{
					Array.Resize(ref array, num + 1);
				}
				if ((object)sourceMemberContainerTypeSymbol != null && sourceMemberContainerTypeSymbol.IsExtension)
				{
					continue;
				}
				Symbol symbol = array[num];
				if ((object)symbol == null && (object)mergedNamespaceSymbol != null)
				{
					foreach (NamespaceSymbol constituentNamespace in mergedNamespaceSymbol.ConstituentNamespaces)
					{
						if ((object)constituentNamespace != @namespace)
						{
							ImmutableArray<NamedTypeSymbol> typeMembers = constituentNamespace.GetTypeMembers(item2.Name, num);
							if (typeMembers.Length > 0)
							{
								symbol = typeMembers[0];
								break;
							}
						}
					}
				}
				SourceMemberContainerTypeSymbol sourceMemberContainerTypeSymbol2;
				if ((object)symbol != null)
				{
					(SourceMemberContainerTypeSymbol, Symbol) tuple = (sourceMemberContainerTypeSymbol, symbol);
					(sourceMemberContainerTypeSymbol2, _) = tuple;
					Symbol item;
					if ((object)sourceMemberContainerTypeSymbol2 == null)
					{
						item = tuple.Item2;
						if (item is SourceMemberContainerTypeSymbol { IsFileLocal: not false })
						{
							goto IL_020d;
						}
						goto IL_0256;
					}
					item = tuple.Item2;
					SourceMemberContainerTypeSymbol sourceMemberContainerTypeSymbol4 = item as SourceMemberContainerTypeSymbol;
					NamespaceOrTypeSymbol namespaceOrTypeSymbol;
					int num2;
					if ((object)sourceMemberContainerTypeSymbol4 != null)
					{
						SourceMemberContainerTypeSymbol otherSymbol = sourceMemberContainerTypeSymbol2;
						if (isFileLocalTypeInSeparateFileFrom(sourceMemberContainerTypeSymbol4, otherSymbol))
						{
							goto IL_027a;
						}
						namespaceOrTypeSymbol = (NamespaceOrTypeSymbol)item;
						num2 = 1;
					}
					else
					{
						namespaceOrTypeSymbol = item as NamespaceOrTypeSymbol;
						if ((object)namespaceOrTypeSymbol == null)
						{
							goto IL_019b;
						}
						num2 = 2;
					}
					NamespaceOrTypeSymbol otherSymbol2 = namespaceOrTypeSymbol;
					if (!isFileLocalTypeInSeparateFileFrom(sourceMemberContainerTypeSymbol2, otherSymbol2))
					{
						if (num2 == 1)
						{
							if (sourceMemberContainerTypeSymbol2.IsFileLocal || sourceMemberContainerTypeSymbol4.IsFileLocal)
							{
								goto IL_020d;
							}
							if (!sourceMemberContainerTypeSymbol2.IsPartial || !sourceMemberContainerTypeSymbol4.IsPartial)
							{
								goto IL_0256;
							}
							diagnostics.Add(ErrorCode.ERR_PartialTypeKindConflict, item2.GetFirstLocationOrNone(), item2);
						}
						else if (num2 == 2)
						{
							goto IL_019b;
						}
					}
				}
				goto IL_027a;
				IL_020d:
				diagnostics.Add(ErrorCode.ERR_FileLocalDuplicateNameInNS, item2.GetFirstLocationOrNone(), item2.Name, @namespace);
				goto IL_027a;
				IL_019b:
				if (sourceMemberContainerTypeSymbol2.IsFileLocal)
				{
					goto IL_020d;
				}
				goto IL_0256;
				IL_0256:
				diagnostics.Add(ErrorCode.ERR_DuplicateNameInNS, item2.GetFirstLocationOrNone(), item2.Name, @namespace);
				goto IL_027a;
				IL_027a:
				array[num] = item2;
				if ((object)sourceMemberContainerTypeSymbol != null)
				{
					Accessibility declaredAccessibility = sourceMemberContainerTypeSymbol.DeclaredAccessibility;
					if (declaredAccessibility != Accessibility.Public && declaredAccessibility != Accessibility.Internal)
					{
						diagnostics.Add(ErrorCode.ERR_NoNamespacePrivate, item2.GetFirstLocationOrNone());
					}
				}
			}
		}
		static bool isFileLocalTypeInSeparateFileFrom(SourceMemberContainerTypeSymbol possibleFileLocalType, NamespaceOrTypeSymbol namespaceOrTypeSymbol2)
		{
			if (!possibleFileLocalType.IsFileLocal)
			{
				return false;
			}
			SyntaxTree sourceTree = possibleFileLocalType.MergedDeclaration.Declarations[0].Location.SourceTree;
			if (namespaceOrTypeSymbol2 is SourceNamedTypeSymbol sourceNamedTypeSymbol)
			{
				MergedTypeDeclaration mergedDeclaration = sourceNamedTypeSymbol.MergedDeclaration;
				if (mergedDeclaration != null)
				{
					return !mergedDeclaration.NameLocations.Any((SourceLocation loc, SyntaxTree leftTree) => loc.SourceTree == leftTree, sourceTree);
				}
			}
			if (namespaceOrTypeSymbol2 is SourceNamespaceSymbol { MergedDeclaration: { NameLocations: var nameLocations } })
			{
				return !nameLocations.Any((Location loc, SyntaxTree leftTree) => loc.SourceTree == leftTree, sourceTree);
			}
			throw ExceptionUtilities.UnexpectedValue(namespaceOrTypeSymbol2);
		}
	}

	private NamespaceOrTypeSymbol BuildSymbol(MergedNamespaceOrTypeDeclaration declaration, BindingDiagnosticBag diagnostics)
	{
		switch (declaration.Kind)
		{
		case DeclarationKind.Namespace:
			return new SourceNamespaceSymbol(_module, this, (MergedNamespaceDeclaration)declaration, diagnostics);
		case DeclarationKind.Class:
		case DeclarationKind.Interface:
		case DeclarationKind.Struct:
		case DeclarationKind.Enum:
		case DeclarationKind.Delegate:
		case DeclarationKind.Record:
		case DeclarationKind.RecordStruct:
		case DeclarationKind.Extension:
			return new SourceNamedTypeSymbol(this, (MergedTypeDeclaration)declaration, diagnostics);
		case DeclarationKind.Script:
		case DeclarationKind.Submission:
		case DeclarationKind.ImplicitClass:
			return new ImplicitNamedTypeSymbol(this, (MergedTypeDeclaration)declaration, diagnostics);
		default:
			throw ExceptionUtilities.UnexpectedValue(declaration.Kind);
		}
	}

	private void RegisterDeclaredCorTypes()
	{
		AssemblySymbol containingAssembly = ContainingAssembly;
		if (!containingAssembly.KeepLookingForDeclaredSpecialTypes)
		{
			return;
		}
		foreach (ImmutableArray<NamespaceOrTypeSymbol> value in _nameToMembersMap.Values)
		{
			foreach (NamespaceOrTypeSymbol item in value)
			{
				if (item is NamedTypeSymbol { SpecialType: not SpecialType.None } namedTypeSymbol)
				{
					containingAssembly.RegisterDeclaredSpecialType(namedTypeSymbol);
					if (!containingAssembly.KeepLookingForDeclaredSpecialTypes)
					{
						return;
					}
				}
			}
		}
	}

	public override bool IsDefinedInSourceTree(SyntaxTree tree, TextSpan? definedWithinSpan, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (IsGlobalNamespace)
		{
			return true;
		}
		foreach (SingleNamespaceDeclaration declaration in _mergedDeclaration.Declarations)
		{
			cancellationToken.ThrowIfCancellationRequested();
			SyntaxReference syntaxReference = declaration.SyntaxReference;
			if (syntaxReference.SyntaxTree == tree)
			{
				if (!definedWithinSpan.HasValue)
				{
					return true;
				}
				if (NamespaceDeclarationSyntaxReference.GetSyntax(syntaxReference, cancellationToken).FullSpan.IntersectsWith(definedWithinSpan.Value))
				{
					return true;
				}
			}
		}
		return false;
	}

	internal override void ForceComplete(SourceLocation? locationOpt, Predicate<Symbol>? filter, CancellationToken cancellationToken)
	{
		Predicate<Symbol>? predicate = filter;
		if (predicate != null && !predicate(this))
		{
			return;
		}
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			CompletionPart nextIncompletePart = _state.NextIncompletePart;
			switch (nextIncompletePart)
			{
			case CompletionPart.Members:
				GetNameToMembersMap();
				break;
			case CompletionPart.MembersCompleted:
			{
				SingleNamespaceDeclaration singleNamespaceDeclaration = null;
				foreach (SingleNamespaceDeclaration declaration in _mergedDeclaration.Declarations)
				{
					if ((locationOpt == null || locationOpt.SourceTree == declaration.SyntaxReference.SyntaxTree) && (declaration.HasGlobalUsings || declaration.HasUsings || declaration.HasExternAliases))
					{
						singleNamespaceDeclaration = declaration;
						GetAliasesAndUsings(declaration).Complete(this, declaration.SyntaxReference, cancellationToken);
					}
				}
				if (IsGlobalNamespace && ((object)locationOpt == null || singleNamespaceDeclaration != null))
				{
					GetMergedGlobalAliasesAndUsings(null, cancellationToken).Complete(this, cancellationToken);
				}
				ImmutableArray<Symbol> members = GetMembers();
				bool flag = true;
				if (DeclaringCompilation.Options.ConcurrentBuild)
				{
					RoslynParallel.For(0, members.Length, UICultureUtilities.WithCurrentUICulture(delegate(int i)
					{
						Symbol.ForceCompleteMemberConditionally(locationOpt, filter, members[i], cancellationToken);
					}), cancellationToken);
					foreach (Symbol item in members)
					{
						if (!item.HasComplete(CompletionPart.All))
						{
							flag = false;
							break;
						}
					}
				}
				else
				{
					foreach (Symbol item2 in members)
					{
						Symbol.ForceCompleteMemberConditionally(locationOpt, filter, item2, cancellationToken);
						flag = flag && item2.HasComplete(CompletionPart.All);
					}
				}
				if (flag)
				{
					_state.NotePartComplete(CompletionPart.MembersCompleted);
					break;
				}
				CompletionPart part = ((locationOpt == null && filter == null) ? CompletionPart.NamespaceSymbolAll : CompletionPart.Members);
				_state.SpinWaitComplete(part, cancellationToken);
				return;
			}
			case CompletionPart.None:
				return;
			default:
				_state.NotePartComplete(CompletionPart.PropertySymbolAll | CompletionPart.ReturnTypeAttributes | CompletionPart.Parameters | CompletionPart.Type | CompletionPart.TypeParameters | CompletionPart.TypeMembers | CompletionPart.SynthesizedExplicitImplementations | CompletionPart.StartMemberChecks | CompletionPart.FinishMemberChecks | CompletionPart.MembersCompletedChecksStarted);
				break;
			}
			_state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	internal override bool HasComplete(CompletionPart part)
	{
		return _state.HasComplete(part);
	}
}
