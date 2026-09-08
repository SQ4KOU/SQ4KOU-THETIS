using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedRecordPrintMembers : SynthesizedRecordOrdinaryMethod
{
	public SynthesizedRecordPrintMembers(SourceMemberContainerTypeSymbol containingType, IEnumerable<Symbol> userDefinedMembers, int memberOffset)
		: base(containingType, "PrintMembers", memberOffset, MakeDeclarationModifiers(containingType, userDefinedMembers))
	{
	}

	private static DeclarationModifiers MakeDeclarationModifiers(SourceMemberContainerTypeSymbol containingType, IEnumerable<Symbol> userDefinedMembers)
	{
		DeclarationModifiers declarationModifiers = ((containingType.IsRecordStruct || (containingType.BaseTypeNoUseSiteDiagnostics.IsObjectType() && containingType.IsSealed)) ? DeclarationModifiers.Private : DeclarationModifiers.Protected);
		declarationModifiers = ((!containingType.IsRecord || containingType.BaseTypeNoUseSiteDiagnostics.IsObjectType()) ? ((DeclarationModifiers)((uint)declarationModifiers | (uint)((!containingType.IsSealed) ? 131072 : 0))) : (declarationModifiers | DeclarationModifiers.Override));
		if (IsReadOnly(containingType, userDefinedMembers))
		{
			declarationModifiers |= DeclarationModifiers.ReadOnly;
		}
		return declarationModifiers;
	}

	protected override (TypeWithAnnotations ReturnType, ImmutableArray<ParameterSymbol> Parameters) MakeParametersAndBindReturnType(BindingDiagnosticBag diagnostics)
	{
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		Location returnTypeLocation = ReturnTypeLocation;
		NullableAnnotation nullableAnnotation = (ContainingType.IsRecordStruct ? NullableAnnotation.Oblivious : NullableAnnotation.NotAnnotated);
		return (ReturnType: TypeWithAnnotations.Create(Binder.GetSpecialType(declaringCompilation, SpecialType.System_Boolean, returnTypeLocation, diagnostics)), Parameters: ImmutableArray.Create((ParameterSymbol)new SourceSimpleParameterSymbol(this, TypeWithAnnotations.Create(Binder.GetWellKnownType(declaringCompilation, WellKnownType.System_Text_StringBuilder, diagnostics, returnTypeLocation), nullableAnnotation), 0, RefKind.None, "builder", Locations)));
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
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, ContainingType.GetNonNullSyntaxNode(), compilationState, diagnostics);
		try
		{
			ImmutableArray<Symbol> immutableArray = ContainingType.GetMembers().WhereAsArray((Symbol m) => isPrintable(m));
			if (base.ReturnType.IsErrorType() || immutableArray.Any((Symbol m) => m.GetTypeOrReturnType().Type.IsErrorType()))
			{
				syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
				return;
			}
			BoundParameter boundParameter = syntheticBoundNodeFactory.Parameter(Parameters[0]);
			ArrayBuilder<BoundStatement> instance;
			if (ContainingType.BaseTypeNoUseSiteDiagnostics.IsObjectType() || ContainingType.IsRecordStruct)
			{
				if (immutableArray.IsEmpty)
				{
					syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.Return(syntheticBoundNodeFactory.Literal(value: false)));
					return;
				}
				instance = ArrayBuilder<BoundStatement>.GetInstance();
				if (!ContainingType.IsRecordStruct)
				{
					MethodSymbol methodSymbol = syntheticBoundNodeFactory.WellKnownMethod(WellKnownMember.System_Runtime_CompilerServices_RuntimeHelpers__EnsureSufficientExecutionStack, isOptional: true);
					if ((object)methodSymbol != null)
					{
						instance.Add(syntheticBoundNodeFactory.ExpressionStatement(syntheticBoundNodeFactory.Call(null, methodSymbol)));
					}
				}
			}
			else
			{
				MethodSymbol overriddenMethod = base.OverriddenMethod;
				if ((object)overriddenMethod == null || overriddenMethod.ReturnType.SpecialType != SpecialType.System_Boolean)
				{
					syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
					return;
				}
				BoundCall boundCall = syntheticBoundNodeFactory.Call(syntheticBoundNodeFactory.Base(ContainingType.BaseTypeNoUseSiteDiagnostics), overriddenMethod, boundParameter);
				if (immutableArray.IsEmpty)
				{
					syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.Return(boundCall));
					return;
				}
				instance = ArrayBuilder<BoundStatement>.GetInstance();
				instance.Add(syntheticBoundNodeFactory.If(boundCall, makeAppendString(syntheticBoundNodeFactory, boundParameter, ", ")));
			}
			for (int num = 0; num < immutableArray.Length; num++)
			{
				Symbol symbol = immutableArray[num];
				string text = symbol.Name + " = ";
				if (num > 0)
				{
					text = ", " + text;
				}
				instance.Add(makeAppendString(syntheticBoundNodeFactory, boundParameter, text));
				BoundExpression boundExpression = symbol.Kind switch
				{
					SymbolKind.Field => syntheticBoundNodeFactory.Field(syntheticBoundNodeFactory.This(), (FieldSymbol)symbol), 
					SymbolKind.Property => syntheticBoundNodeFactory.Property(syntheticBoundNodeFactory.This(), (PropertySymbol)symbol), 
					_ => throw ExceptionUtilities.UnexpectedValue(symbol.Kind), 
				};
				if (boundExpression.Type.IsValueType)
				{
					instance.Add(syntheticBoundNodeFactory.ExpressionStatement(syntheticBoundNodeFactory.Call(boundParameter, syntheticBoundNodeFactory.WellKnownMethod(WellKnownMember.System_Text_StringBuilder__AppendString), syntheticBoundNodeFactory.Call(boundExpression, syntheticBoundNodeFactory.SpecialMethod(SpecialMember.System_Object__ToString)))));
				}
				else if (!boundExpression.Type.IsRestrictedType())
				{
					NamedTypeSymbol namedTypeSymbol = syntheticBoundNodeFactory.SpecialType(SpecialType.System_Object);
					Conversion conversion = syntheticBoundNodeFactory.ClassifyEmitConversion(boundExpression, namedTypeSymbol);
					instance.Add(syntheticBoundNodeFactory.ExpressionStatement(syntheticBoundNodeFactory.Call(boundParameter, syntheticBoundNodeFactory.WellKnownMethod(WellKnownMember.System_Text_StringBuilder__AppendObject), syntheticBoundNodeFactory.Convert(namedTypeSymbol, boundExpression, conversion))));
				}
			}
			instance.Add(syntheticBoundNodeFactory.Return(syntheticBoundNodeFactory.Literal(value: true)));
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.Block(instance.ToImmutableAndFree()));
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
		}
		static bool isPrintable(Symbol m)
		{
			if (!IsPublicInstanceMember(m))
			{
				return false;
			}
			if (m.Kind == SymbolKind.Field && !(m is TupleErrorFieldSymbol))
			{
				return true;
			}
			if (m.Kind == SymbolKind.Property)
			{
				return IsPrintableProperty((PropertySymbol)m);
			}
			return false;
		}
		static BoundStatement makeAppendString(SyntheticBoundNodeFactory F, BoundParameter builder, string value)
		{
			return F.ExpressionStatement(F.Call(builder, F.WellKnownMethod(WellKnownMember.System_Text_StringBuilder__AppendString), F.StringLiteral(value)));
		}
	}

	internal static void VerifyOverridesPrintMembersFromBase(MethodSymbol overriding, BindingDiagnosticBag diagnostics)
	{
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = overriding.ContainingType.BaseTypeNoUseSiteDiagnostics;
		if (baseTypeNoUseSiteDiagnostics.IsObjectType() || !baseTypeNoUseSiteDiagnostics.IsRecord)
		{
			return;
		}
		bool flag = false;
		if (!overriding.IsOverride)
		{
			flag = true;
		}
		else
		{
			MethodSymbol overriddenMethod = overriding.OverriddenMethod;
			if ((object)overriddenMethod != null && !overriddenMethod.ContainingType.Equals(baseTypeNoUseSiteDiagnostics, TypeCompareKind.AllIgnoreOptions))
			{
				flag = true;
			}
		}
		if (flag)
		{
			diagnostics.Add(ErrorCode.ERR_DoesNotOverrideBaseMethod, overriding.GetFirstLocation(), overriding, baseTypeNoUseSiteDiagnostics);
		}
	}

	private static bool IsReadOnly(NamedTypeSymbol containingType, IEnumerable<Symbol> userDefinedMembers)
	{
		if (!containingType.IsReadOnly)
		{
			if (containingType.IsRecordStruct)
			{
				return AreAllPrintablePropertyGettersReadOnly(userDefinedMembers);
			}
			return false;
		}
		return true;
	}

	private static bool AreAllPrintablePropertyGettersReadOnly(IEnumerable<Symbol> members)
	{
		foreach (Symbol member in members)
		{
			if (member.Kind != SymbolKind.Property)
			{
				continue;
			}
			PropertySymbol propertySymbol = (PropertySymbol)member;
			if (IsPublicInstanceMember(propertySymbol) && IsPrintableProperty(propertySymbol))
			{
				MethodSymbol getMethod = propertySymbol.GetMethod;
				if ((object)propertySymbol.GetMethod != null && !getMethod.IsEffectivelyReadOnly)
				{
					return false;
				}
			}
		}
		return true;
	}

	private static bool IsPublicInstanceMember(Symbol m)
	{
		if (m.DeclaredAccessibility == Accessibility.Public)
		{
			return !m.IsStatic;
		}
		return false;
	}

	private static bool IsPrintableProperty(PropertySymbol property)
	{
		if (!property.IsIndexer && !property.IsOverride)
		{
			return (object)property.GetMethod != null;
		}
		return false;
	}
}
