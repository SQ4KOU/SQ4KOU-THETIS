using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedRecordEquals : SynthesizedRecordOrdinaryMethod
{
	private readonly PropertySymbol? _equalityContract;

	public SynthesizedRecordEquals(SourceMemberContainerTypeSymbol containingType, PropertySymbol? equalityContract, int memberOffset)
		: base(containingType, "Equals", memberOffset, (DeclarationModifiers)(0x10 | ((!containingType.IsSealed) ? 131072 : 0) | (containingType.IsRecordStruct ? 1024 : 0)))
	{
		_equalityContract = equalityContract;
	}

	protected override (TypeWithAnnotations ReturnType, ImmutableArray<ParameterSymbol> Parameters) MakeParametersAndBindReturnType(BindingDiagnosticBag diagnostics)
	{
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		Location returnTypeLocation = ReturnTypeLocation;
		NullableAnnotation nullableAnnotation = (ContainingType.IsRecordStruct ? NullableAnnotation.Oblivious : NullableAnnotation.Annotated);
		return (ReturnType: TypeWithAnnotations.Create(Binder.GetSpecialType(declaringCompilation, SpecialType.System_Boolean, returnTypeLocation, diagnostics)), Parameters: ImmutableArray.Create((ParameterSymbol)new SourceSimpleParameterSymbol(this, TypeWithAnnotations.Create(ContainingType, nullableAnnotation), 0, RefKind.None, "other", Locations)));
	}

	protected override int GetParameterCountFromSyntax()
	{
		return 1;
	}

	internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, ContainingType.GetNonNullSyntaxNode(), compilationState, diagnostics);
		try
		{
			BoundParameter boundParameter = syntheticBoundNodeFactory.Parameter(Parameters[0]);
			bool isRecordStruct = ContainingType.IsRecordStruct;
			BoundExpression boundExpression;
			if (isRecordStruct)
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
				if (_equalityContract.IsStatic || !_equalityContract.Type.Equals(DeclaringCompilation.GetWellKnownType(WellKnownType.System_Type), TypeCompareKind.AllIgnoreOptions))
				{
					syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
					return;
				}
				boundExpression = syntheticBoundNodeFactory.ObjectNotEqual(boundParameter, syntheticBoundNodeFactory.Null(syntheticBoundNodeFactory.SpecialType(SpecialType.System_Object)));
				BoundCall right = syntheticBoundNodeFactory.Call(null, syntheticBoundNodeFactory.WellKnownMethod(WellKnownMember.System_Type__op_Equality), syntheticBoundNodeFactory.Property(syntheticBoundNodeFactory.This(), _equalityContract), syntheticBoundNodeFactory.Property(boundParameter, _equalityContract));
				boundExpression = syntheticBoundNodeFactory.LogicalAnd(boundExpression, right);
			}
			else
			{
				MethodSymbol overriddenMethod = ContainingType.GetMembersUnordered().OfType<SynthesizedRecordBaseEquals>().Single()
					.OverriddenMethod;
				if ((object)overriddenMethod == null || !overriddenMethod.ContainingType.Equals(ContainingType.BaseTypeNoUseSiteDiagnostics, TypeCompareKind.AllIgnoreOptions) || overriddenMethod.ReturnType.SpecialType != SpecialType.System_Boolean)
				{
					syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
					return;
				}
				TypeSymbol type = overriddenMethod.Parameters[0].Type;
				Conversion conversion = syntheticBoundNodeFactory.ClassifyEmitConversion(boundParameter, type);
				boundExpression = syntheticBoundNodeFactory.Call(syntheticBoundNodeFactory.Base(overriddenMethod.ContainingType), overriddenMethod, syntheticBoundNodeFactory.Convert(type, boundParameter, conversion));
			}
			ArrayBuilder<FieldSymbol> instance = ArrayBuilder<FieldSymbol>.GetInstance();
			bool flag = false;
			foreach (FieldSymbol item in ContainingType.GetFieldsToEmit())
			{
				if (!item.IsStatic)
				{
					instance.Add(item);
					TypeSymbol type2 = item.Type;
					if (type2.IsPointerOrFunctionPointer() || type2.IsRestrictedType())
					{
						flag = true;
					}
				}
			}
			if (instance.Count > 0 && !flag)
			{
				boundExpression = MethodBodySynthesizer.GenerateFieldEquals(boundExpression, boundParameter, instance, syntheticBoundNodeFactory);
			}
			else if (boundExpression == null)
			{
				boundExpression = syntheticBoundNodeFactory.Literal(value: true);
			}
			instance.Free();
			if (!isRecordStruct)
			{
				boundExpression = syntheticBoundNodeFactory.LogicalOr(syntheticBoundNodeFactory.ObjectEqual(syntheticBoundNodeFactory.This(), boundParameter), boundExpression);
			}
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.Block(syntheticBoundNodeFactory.Return(boundExpression)));
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
		}
	}
}
