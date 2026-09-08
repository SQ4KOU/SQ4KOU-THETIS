using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedRecordGetHashCode : SynthesizedRecordObjectMethod
{
	private readonly PropertySymbol? _equalityContract;

	protected override SpecialMember OverriddenSpecialMember => SpecialMember.System_Object__GetHashCode;

	public SynthesizedRecordGetHashCode(SourceMemberContainerTypeSymbol containingType, PropertySymbol? equalityContract, int memberOffset)
		: base(containingType, "GetHashCode", memberOffset, containingType.IsRecordStruct)
	{
		_equalityContract = equalityContract;
	}

	protected override (TypeWithAnnotations ReturnType, ImmutableArray<ParameterSymbol> Parameters) MakeParametersAndBindReturnType(BindingDiagnosticBag diagnostics)
	{
		return (ReturnType: TypeWithAnnotations.Create(Binder.GetSpecialType(DeclaringCompilation, location: ReturnTypeLocation, typeId: SpecialType.System_Int32, diagnostics: diagnostics)), Parameters: ImmutableArray<ParameterSymbol>.Empty);
	}

	protected override int GetParameterCountFromSyntax()
	{
		return 0;
	}

	internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, base.SyntaxNode, compilationState, diagnostics);
		try
		{
			MethodSymbol equalityComparer_GetHashCode = null;
			MethodSymbol equalityComparer_get_Default = null;
			BoundExpression boundExpression;
			if (ContainingType.IsRecordStruct)
			{
				boundExpression = null;
			}
			else if (ContainingType.BaseTypeNoUseSiteDiagnostics.IsObjectType())
			{
				if ((object)_equalityContract.GetMethod == null)
				{
					syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
					return;
				}
				if (_equalityContract.IsStatic)
				{
					syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
					return;
				}
				ensureEqualityComparerHelpers(syntheticBoundNodeFactory, ref equalityComparer_GetHashCode, ref equalityComparer_get_Default);
				boundExpression = MethodBodySynthesizer.GenerateGetHashCode(equalityComparer_GetHashCode, equalityComparer_get_Default, syntheticBoundNodeFactory.Property(syntheticBoundNodeFactory.This(), _equalityContract), syntheticBoundNodeFactory);
			}
			else
			{
				MethodSymbol overriddenMethod = base.OverriddenMethod;
				if ((object)overriddenMethod == null || overriddenMethod.ReturnType.SpecialType != SpecialType.System_Int32)
				{
					syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
					return;
				}
				boundExpression = syntheticBoundNodeFactory.Call(syntheticBoundNodeFactory.Base(overriddenMethod.ContainingType), overriddenMethod);
			}
			BoundLiteral boundHashFactor = null;
			foreach (FieldSymbol item in ContainingType.GetFieldsToEmit())
			{
				if (!item.IsStatic)
				{
					ensureEqualityComparerHelpers(syntheticBoundNodeFactory, ref equalityComparer_GetHashCode, ref equalityComparer_get_Default);
					boundExpression = ((boundExpression != null) ? MethodBodySynthesizer.GenerateHashCombine(boundExpression, equalityComparer_GetHashCode, equalityComparer_get_Default, ref boundHashFactor, syntheticBoundNodeFactory.Field(syntheticBoundNodeFactory.This(), item), syntheticBoundNodeFactory) : MethodBodySynthesizer.GenerateGetHashCode(equalityComparer_GetHashCode, equalityComparer_get_Default, syntheticBoundNodeFactory.Field(syntheticBoundNodeFactory.This(), item), syntheticBoundNodeFactory));
				}
			}
			if (boundExpression == null)
			{
				boundExpression = syntheticBoundNodeFactory.Literal(0);
			}
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.Block(syntheticBoundNodeFactory.Return(boundExpression)));
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
		}
		static void ensureEqualityComparerHelpers(SyntheticBoundNodeFactory F, [NotNull] ref MethodSymbol? reference, [NotNull] ref MethodSymbol? reference2)
		{
			if ((object)reference == null)
			{
				reference = F.WellKnownMethod(WellKnownMember.System_Collections_Generic_EqualityComparer_T__GetHashCode);
			}
			if ((object)reference2 == null)
			{
				reference2 = F.WellKnownMethod(WellKnownMember.System_Collections_Generic_EqualityComparer_T__get_Default);
			}
		}
	}
}
