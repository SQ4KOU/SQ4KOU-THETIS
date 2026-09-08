using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedRecordClone : SynthesizedRecordOrdinaryMethod
{
	public SynthesizedRecordClone(SourceMemberContainerTypeSymbol containingType, int memberOffset)
		: base(containingType, "<Clone>$", memberOffset, MakeDeclarationModifiers(containingType))
	{
	}

	private static DeclarationModifiers MakeDeclarationModifiers(SourceMemberContainerTypeSymbol containingType)
	{
		DeclarationModifiers declarationModifiers = DeclarationModifiers.Public;
		declarationModifiers = (((object)VirtualCloneInBase(containingType) == null) ? ((DeclarationModifiers)((uint)declarationModifiers | (uint)((!containingType.IsSealed) ? 131072 : 0))) : (declarationModifiers | DeclarationModifiers.Override));
		if (containingType.IsAbstract)
		{
			declarationModifiers = (DeclarationModifiers)((uint)declarationModifiers & 0xFFFDFFFFu);
			declarationModifiers |= DeclarationModifiers.Abstract;
		}
		return declarationModifiers;
	}

	private static MethodSymbol? VirtualCloneInBase(NamedTypeSymbol containingType)
	{
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = containingType.BaseTypeNoUseSiteDiagnostics;
		if (!baseTypeNoUseSiteDiagnostics.IsObjectType())
		{
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
			return FindValidCloneMethod(baseTypeNoUseSiteDiagnostics, ref useSiteInfo);
		}
		return null;
	}

	protected override (TypeWithAnnotations ReturnType, ImmutableArray<ParameterSymbol> Parameters) MakeParametersAndBindReturnType(BindingDiagnosticBag diagnostics)
	{
		TypeWithAnnotations item;
		if (!ContainingAssembly.RuntimeSupportsCovariantReturnsOfClasses)
		{
			MethodSymbol methodSymbol = VirtualCloneInBase(ContainingType);
			if ((object)methodSymbol != null)
			{
				item = methodSymbol.ReturnTypeWithAnnotations;
				goto IL_0031;
			}
		}
		item = TypeWithAnnotations.Create(isNullableEnabled: true, ContainingType);
		goto IL_0031;
		IL_0031:
		return (ReturnType: item, Parameters: ImmutableArray<ParameterSymbol>.Empty);
	}

	protected override int GetParameterCountFromSyntax()
	{
		return 0;
	}

	internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, ContainingType.GetNonNullSyntaxNode(), compilationState, diagnostics);
		try
		{
			if (base.ReturnType.IsErrorType())
			{
				syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
				return;
			}
			foreach (MethodSymbol instanceConstructor in ContainingType.InstanceConstructors)
			{
				if (instanceConstructor.ParameterCount == 1 && instanceConstructor.Parameters[0].RefKind == RefKind.None && instanceConstructor.Parameters[0].Type.Equals(ContainingType, TypeCompareKind.AllIgnoreOptions))
				{
					syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.Return(syntheticBoundNodeFactory.New(instanceConstructor, syntheticBoundNodeFactory.This())));
					return;
				}
			}
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/Records/SynthesizedRecordClone.cs", 132);
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
		}
	}

	internal static MethodSymbol? FindValidCloneMethod(TypeSymbol containingType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (containingType.IsObjectType() || !(containingType is NamedTypeSymbol namedTypeSymbol))
		{
			return null;
		}
		if (!namedTypeSymbol.HasPossibleWellKnownCloneMethod())
		{
			return null;
		}
		MethodSymbol methodSymbol = null;
		foreach (Symbol member in containingType.GetMembers("<Clone>$"))
		{
			if (member is MethodSymbol methodSymbol2 && member.DeclaredAccessibility == Accessibility.Public && !member.IsStatic && methodSymbol2.ParameterCount == 0 && methodSymbol2.Arity == 0)
			{
				if ((object)methodSymbol != null)
				{
					return null;
				}
				methodSymbol = methodSymbol2;
			}
		}
		if ((object)methodSymbol == null || (!containingType.IsSealed && !methodSymbol.IsOverride && !methodSymbol.IsVirtual && !methodSymbol.IsAbstract) || !containingType.IsEqualToOrDerivedFrom(methodSymbol.ReturnType, TypeCompareKind.AllIgnoreOptions, ref useSiteInfo))
		{
			return null;
		}
		return methodSymbol;
	}
}
