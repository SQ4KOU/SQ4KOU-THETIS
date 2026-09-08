using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
internal sealed class ImportChain : Microsoft.Cci.IImportScope
{
	public readonly Imports Imports;

	public readonly ImportChain ParentOpt;

	Microsoft.Cci.IImportScope Microsoft.Cci.IImportScope.Parent => ParentOpt;

	public ImportChain(Imports imports, ImportChain parentOpt)
	{
		Imports = imports;
		ParentOpt = parentOpt;
	}

	private string GetDebuggerDisplay()
	{
		return $"{Imports.GetDebuggerDisplay()} ^ {ParentOpt?.GetHashCode() ?? 0}";
	}

	ImmutableArray<UsedNamespaceOrType> Microsoft.Cci.IImportScope.GetUsedNamespaces(EmitContext context)
	{
		((PEModuleBuilder)context.Module).TryGetTranslatedImports(this, out var imports);
		return imports;
	}

	public Microsoft.Cci.IImportScope Translate(PEModuleBuilder moduleBuilder, DiagnosticBag diagnostics)
	{
		ImportChain importChain = this;
		ImmutableArray<UsedNamespaceOrType> imports;
		while (importChain != null && !moduleBuilder.TryGetTranslatedImports(importChain, out imports))
		{
			moduleBuilder.GetOrAddTranslatedImports(importChain, importChain.TranslateImports(moduleBuilder, diagnostics));
			importChain = importChain.ParentOpt;
		}
		return this;
	}

	private ImmutableArray<UsedNamespaceOrType> TranslateImports(PEModuleBuilder moduleBuilder, DiagnosticBag diagnostics)
	{
		ArrayBuilder<UsedNamespaceOrType> instance = ArrayBuilder<UsedNamespaceOrType>.GetInstance();
		ImmutableArray<AliasAndExternAliasDirective> externAliases = Imports.ExternAliases;
		if (!externAliases.IsDefault)
		{
			foreach (AliasAndExternAliasDirective item in externAliases)
			{
				instance.Add(UsedNamespaceOrType.CreateExternAlias(item.Alias.Name));
			}
		}
		ImmutableArray<NamespaceOrTypeAndUsingDirective> usings = Imports.Usings;
		if (!usings.IsDefault)
		{
			foreach (NamespaceOrTypeAndUsingDirective item2 in usings)
			{
				NamespaceOrTypeSymbol namespaceOrType = item2.NamespaceOrType;
				if (namespaceOrType.IsNamespace)
				{
					NamespaceSymbol namespaceSymbol = (NamespaceSymbol)namespaceOrType;
					IAssemblyReference assemblyOpt = TryGetAssemblyScope(namespaceSymbol, moduleBuilder, diagnostics);
					instance.Add(UsedNamespaceOrType.CreateNamespace(namespaceSymbol.GetCciAdapter(), assemblyOpt));
				}
				else if (!namespaceOrType.ContainingAssembly.IsLinked)
				{
					ITypeReference typeReference = GetTypeReference((TypeSymbol)namespaceOrType, item2.UsingDirective, moduleBuilder, diagnostics);
					instance.Add(UsedNamespaceOrType.CreateType(typeReference));
				}
			}
		}
		ImmutableDictionary<string, AliasAndUsingDirective> usingAliases = Imports.UsingAliases;
		if (!usingAliases.IsEmpty)
		{
			ArrayBuilder<string> instance2 = ArrayBuilder<string>.GetInstance(usingAliases.Count);
			instance2.AddRange(usingAliases.Keys);
			instance2.Sort(StringComparer.Ordinal);
			foreach (string item3 in instance2)
			{
				AliasAndUsingDirective aliasAndUsingDirective = usingAliases[item3];
				AliasSymbol alias = aliasAndUsingDirective.Alias;
				UsingDirectiveSyntax usingDirective = aliasAndUsingDirective.UsingDirective;
				NamespaceOrTypeSymbol target = alias.Target;
				if (target.Kind == SymbolKind.Namespace)
				{
					NamespaceSymbol namespaceSymbol2 = (NamespaceSymbol)target;
					IAssemblyReference assemblyOpt2 = TryGetAssemblyScope(namespaceSymbol2, moduleBuilder, diagnostics);
					instance.Add(UsedNamespaceOrType.CreateNamespace(namespaceSymbol2.GetCciAdapter(), assemblyOpt2, item3));
					continue;
				}
				bool flag;
				if (target is NamedTypeSymbol)
				{
					AssemblySymbol containingAssembly = target.ContainingAssembly;
					if ((object)containingAssembly == null || containingAssembly.IsLinked)
					{
						flag = false;
						goto IL_01ca;
					}
				}
				flag = true;
				goto IL_01ca;
				IL_01ca:
				if (flag)
				{
					ITypeReference typeReference2 = GetTypeReference((TypeSymbol)target, usingDirective, moduleBuilder, diagnostics);
					instance.Add(UsedNamespaceOrType.CreateType(typeReference2, item3));
				}
			}
			instance2.Free();
		}
		return instance.ToImmutableAndFree();
	}

	private static ITypeReference GetTypeReference(TypeSymbol type, SyntaxNode syntaxNode, PEModuleBuilder moduleBuilder, DiagnosticBag diagnostics)
	{
		return moduleBuilder.Translate(type, syntaxNode, diagnostics);
	}

	private static IAssemblyReference TryGetAssemblyScope(NamespaceSymbol @namespace, PEModuleBuilder moduleBuilder, DiagnosticBag diagnostics)
	{
		AssemblySymbol containingAssembly = @namespace.ContainingAssembly;
		if ((object)containingAssembly != null && containingAssembly != moduleBuilder.CommonCompilation.Assembly)
		{
			CSharpCompilation.ReferenceManager boundReferenceManager = ((CSharpCompilation)moduleBuilder.CommonCompilation).GetBoundReferenceManager();
			for (int i = 0; i < boundReferenceManager.ReferencedAssemblies.Length; i++)
			{
				if ((object)boundReferenceManager.ReferencedAssemblies[i] == containingAssembly && !boundReferenceManager.DeclarationsAccessibleWithoutAlias(i))
				{
					return moduleBuilder.Translate(containingAssembly, diagnostics);
				}
			}
		}
		return null;
	}
}
