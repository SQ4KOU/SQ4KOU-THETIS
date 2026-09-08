using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedRecordCopyCtor : SynthesizedInstanceConstructor
{
	private readonly int _memberOffset;

	public override ImmutableArray<ParameterSymbol> Parameters { get; }

	public override Accessibility DeclaredAccessibility
	{
		get
		{
			if (!ContainingType.IsSealed)
			{
				return Accessibility.Protected;
			}
			return Accessibility.Private;
		}
	}

	protected sealed override bool HasSetsRequiredMembersImpl
	{
		get
		{
			if (!ContainingType.HasAnyRequiredMembers)
			{
				return ContainingType.HasRequiredMembersError;
			}
			return true;
		}
	}

	public SynthesizedRecordCopyCtor(SourceMemberContainerTypeSymbol containingType, int memberOffset)
		: base(containingType)
	{
		_memberOffset = memberOffset;
		Parameters = ImmutableArray.Create(SynthesizedParameterSymbol.Create(this, TypeWithAnnotations.Create(isNullableEnabled: true, ContainingType), 0, RefKind.None, "original"));
	}

	internal override LexicalSortKey GetLexicalSortKey()
	{
		return LexicalSortKey.GetSynthesizedMemberKey(_memberOffset);
	}

	internal override void GenerateMethodBodyStatements(SyntheticBoundNodeFactory F, ArrayBuilder<BoundStatement> statements, BindingDiagnosticBag diagnostics)
	{
		BoundParameter receiver = F.Parameter(Parameters[0]);
		foreach (FieldSymbol item in ContainingType.GetFieldsToEmit())
		{
			if (!item.IsStatic)
			{
				statements.Add(F.Assignment(F.Field(F.This(), item), F.Field(receiver, item)));
			}
		}
		RecordDeclarationSyntax recordDeclarationSyntax = (RecordDeclarationSyntax)F.Syntax;
		statements.Add(new BoundSequencePointWithSpan(recordDeclarationSyntax, null, (recordDeclarationSyntax.TypeParameterList == null) ? recordDeclarationSyntax.Identifier.Span : TextSpan.FromBounds(recordDeclarationSyntax.Identifier.Span.Start, recordDeclarationSyntax.TypeParameterList.Span.End)));
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor));
		if (HasSetsRequiredMembersImpl)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Diagnostics_CodeAnalysis_SetsRequiredMembersAttribute__ctor));
		}
	}

	internal static MethodSymbol? FindCopyConstructor(NamedTypeSymbol containingType, NamedTypeSymbol within, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		MethodSymbol methodSymbol = null;
		int num = -1;
		foreach (MethodSymbol instanceConstructor in containingType.InstanceConstructors)
		{
			if (!HasCopyConstructorSignature(instanceConstructor) || instanceConstructor.HasUnsupportedMetadata || !AccessCheck.IsSymbolAccessible(instanceConstructor, within, ref useSiteInfo))
			{
				continue;
			}
			if ((object)methodSymbol == null && num < 0)
			{
				methodSymbol = instanceConstructor;
				continue;
			}
			if (num < 0)
			{
				num = methodSymbol.CustomModifierCount();
			}
			int num2 = instanceConstructor.CustomModifierCount();
			if (num2 <= num)
			{
				if (num2 == num)
				{
					methodSymbol = null;
					continue;
				}
				methodSymbol = instanceConstructor;
				num = num2;
			}
		}
		return methodSymbol;
	}

	internal static bool IsCopyConstructor(Symbol member)
	{
		if (member is MethodSymbol methodSymbol)
		{
			NamedTypeSymbol containingType = member.ContainingType;
			if ((object)containingType != null && containingType.IsRecord && methodSymbol.MethodKind == MethodKind.Constructor)
			{
				return HasCopyConstructorSignature(methodSymbol);
			}
		}
		return false;
	}

	internal static bool HasCopyConstructorSignature(MethodSymbol member)
	{
		NamedTypeSymbol containingType = member.ContainingType;
		if ((object)member != null && !member.IsStatic && member.ParameterCount == 1 && member.Arity == 0)
		{
			if (member.Parameters[0].Type.Equals(containingType, TypeCompareKind.AllIgnoreOptions))
			{
				return member.Parameters[0].RefKind == RefKind.None;
			}
		}
		return false;
	}
}
