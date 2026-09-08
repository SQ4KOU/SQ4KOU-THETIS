using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel;

internal abstract class AssemblySymbol : Symbol, IAssemblySymbol, ISymbol, IEquatable<ISymbol?>
{
	private IEnumerable<IModuleSymbol> _lazyModules;

	internal abstract Microsoft.CodeAnalysis.CSharp.Symbols.AssemblySymbol UnderlyingAssemblySymbol { get; }

	INamespaceSymbol IAssemblySymbol.GlobalNamespace => UnderlyingAssemblySymbol.GlobalNamespace.GetPublicSymbol();

	IEnumerable<IModuleSymbol> IAssemblySymbol.Modules => InterlockedOperations.Initialize(ref _lazyModules, (AssemblySymbol self) => self.UnderlyingAssemblySymbol.Modules.SelectAsArray((Microsoft.CodeAnalysis.CSharp.Symbols.ModuleSymbol module) => module.GetPublicSymbol()), this);

	bool IAssemblySymbol.IsInteractive => UnderlyingAssemblySymbol.IsInteractive;

	AssemblyIdentity IAssemblySymbol.Identity => UnderlyingAssemblySymbol.Identity;

	ICollection<string> IAssemblySymbol.TypeNames => UnderlyingAssemblySymbol.TypeNames;

	ICollection<string> IAssemblySymbol.NamespaceNames => UnderlyingAssemblySymbol.NamespaceNames;

	bool IAssemblySymbol.MightContainExtensionMethods => UnderlyingAssemblySymbol.MightContainExtensionMethods;

	AssemblyMetadata IAssemblySymbol.GetMetadata()
	{
		return UnderlyingAssemblySymbol.GetMetadata();
	}

	INamedTypeSymbol IAssemblySymbol.ResolveForwardedType(string fullyQualifiedMetadataName)
	{
		return UnderlyingAssemblySymbol.ResolveForwardedType(fullyQualifiedMetadataName).GetPublicSymbol();
	}

	ImmutableArray<INamedTypeSymbol> IAssemblySymbol.GetForwardedTypes()
	{
		return (from t in UnderlyingAssemblySymbol.GetAllTopLevelForwardedTypes()
			select t.GetPublicSymbol() into t
			orderby t.ToDisplayString(SymbolDisplayFormat.QualifiedNameArityFormat)
			select t).AsImmutable();
	}

	bool IAssemblySymbol.GivesAccessTo(IAssemblySymbol assemblyWantingAccess)
	{
		if (object.Equals(this, assemblyWantingAccess))
		{
			return true;
		}
		IEnumerable<ImmutableArray<byte>> internalsVisibleToPublicKeys = UnderlyingAssemblySymbol.GetInternalsVisibleToPublicKeys(assemblyWantingAccess.Name);
		if (internalsVisibleToPublicKeys.Any())
		{
			if (assemblyWantingAccess.IsNetModule())
			{
				return true;
			}
			AssemblyIdentity identity = UnderlyingAssemblySymbol.Identity;
			ImmutableArray<byte> assemblyWantingAccessKey = ((assemblyWantingAccess is AssemblySymbol assemblySymbol) ? assemblySymbol.UnderlyingAssemblySymbol.PublicKey.NullToEmpty() : assemblyWantingAccess.Identity.PublicKey);
			foreach (ImmutableArray<byte> item in internalsVisibleToPublicKeys)
			{
				IVTConclusion iVTConclusion = identity.PerformIVTCheck(assemblyWantingAccessKey, item);
				if (iVTConclusion == IVTConclusion.Match || iVTConclusion == IVTConclusion.OneSignedOneNot)
				{
					return true;
				}
			}
		}
		return false;
	}

	INamedTypeSymbol? IAssemblySymbol.GetTypeByMetadataName(string metadataName)
	{
		return UnderlyingAssemblySymbol.GetTypeByMetadataName(metadataName).GetPublicSymbol();
	}

	protected override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitAssembly(this);
	}

	protected override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitAssembly(this);
	}

	protected override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitAssembly(this, argument);
	}
}
