using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedRecordBaseEquals : SynthesizedRecordOrdinaryMethod
{
	public SynthesizedRecordBaseEquals(SourceMemberContainerTypeSymbol containingType, int memberOffset)
		: base(containingType, "Equals", memberOffset, DeclarationModifiers.Sealed | DeclarationModifiers.Public | DeclarationModifiers.Override)
	{
	}

	protected override (TypeWithAnnotations ReturnType, ImmutableArray<ParameterSymbol> Parameters) MakeParametersAndBindReturnType(BindingDiagnosticBag diagnostics)
	{
		return (ReturnType: TypeWithAnnotations.Create(Binder.GetSpecialType(DeclaringCompilation, location: ReturnTypeLocation, typeId: SpecialType.System_Boolean, diagnostics: diagnostics)), Parameters: ImmutableArray.Create((ParameterSymbol)new SourceSimpleParameterSymbol(this, TypeWithAnnotations.Create(ContainingType.BaseTypeNoUseSiteDiagnostics, NullableAnnotation.Annotated), 0, RefKind.None, "other", Locations)));
	}

	protected override int GetParameterCountFromSyntax()
	{
		return 1;
	}

	protected override void MethodChecks(BindingDiagnosticBag diagnostics)
	{
		base.MethodChecks(diagnostics);
		MethodSymbol overriddenMethod = base.OverriddenMethod;
		if ((object)overriddenMethod != null && !overriddenMethod.ContainingType.Equals(ContainingType.BaseTypeNoUseSiteDiagnostics, TypeCompareKind.AllIgnoreOptions))
		{
			diagnostics.Add(ErrorCode.ERR_DoesNotOverrideBaseMethod, GetFirstLocation(), this, ContainingType.BaseTypeNoUseSiteDiagnostics);
		}
	}

	internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, base.SyntaxNode, compilationState, diagnostics);
		try
		{
			ParameterSymbol parameterSymbol = Parameters[0];
			if (parameterSymbol.Type.IsErrorType())
			{
				syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
				return;
			}
			BoundParameter arg = syntheticBoundNodeFactory.Parameter(parameterSymbol);
			NamedTypeSymbol namedTypeSymbol = syntheticBoundNodeFactory.SpecialType(SpecialType.System_Object);
			Conversion conversion = syntheticBoundNodeFactory.ClassifyEmitConversion(arg, namedTypeSymbol);
			BoundCall expression = syntheticBoundNodeFactory.Call(syntheticBoundNodeFactory.This(), ContainingType.GetMembersUnordered().OfType<SynthesizedRecordObjEquals>().Single(), syntheticBoundNodeFactory.Convert(namedTypeSymbol, arg, conversion));
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.Block(syntheticBoundNodeFactory.Return(expression)));
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
		}
	}
}
