using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedExtensionMarker : SynthesizedSourceOrdinaryMethodSymbol
{
	public override TypeMemberVisibility MetadataVisibility => ((SourceMemberContainerTypeSymbol)ContainingType.ContainingType).GetExtensionGroupingInfo().GetCorrespondingMarkerMethodVisibility(this);

	internal override bool HasSpecialName => true;

	internal SynthesizedExtensionMarker(SourceMemberContainerTypeSymbol extensionType, ParameterListSyntax parameterList)
		: base(extensionType, "<Extension>$", parameterList.OpenParenToken.GetLocation(), parameterList, (declarationModifiers: GetDeclarationModifiers(), flags: SourceMemberMethodSymbol.MakeFlags(MethodKind.Ordinary, RefKind.None, GetDeclarationModifiers(), returnsVoid: false, returnsVoidIsSet: false, isExpressionBodied: false, isExtensionMethod: false, isNullableAnalysisEnabled: false, isVarArg: false, isExplicitInterfaceImplementation: false, hasThisInitializer: false)))
	{
	}

	private static DeclarationModifiers GetDeclarationModifiers()
	{
		return DeclarationModifiers.Static | DeclarationModifiers.Private;
	}

	internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, ContainingType.GetNonNullSyntaxNode(), compilationState, diagnostics);
		syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.Return());
	}

	protected override int GetParameterCountFromSyntax()
	{
		SeparatedSyntaxList<ParameterSyntax> parameters = ((ParameterListSyntax)syntaxReferenceOpt.GetSyntax()).Parameters;
		int num;
		if (parameters.Count >= 1)
		{
			ParameterSyntax parameterSyntax = parameters[0];
			if (parameterSyntax != null)
			{
				num = ((!parameterSyntax.IsArgList) ? 1 : 0);
				goto IL_0041;
			}
		}
		num = 0;
		goto IL_0041;
		IL_0041:
		return (num != 0) ? 1 : 0;
	}

	protected override (TypeWithAnnotations ReturnType, ImmutableArray<ParameterSymbol> Parameters) MakeParametersAndBindReturnType(BindingDiagnosticBag diagnostics)
	{
		TypeWithAnnotations item = TypeWithAnnotations.Create(Binder.GetSpecialType(DeclaringCompilation, SpecialType.System_Void, GetFirstLocation(), diagnostics));
		ParameterSymbol parameterSymbol = makeExtensionParameter(diagnostics);
		return (ReturnType: item, Parameters: ((object)parameterSymbol != null) ? ImmutableCollectionsMarshal.AsImmutableArray(new ParameterSymbol[1] { parameterSymbol }) : ImmutableArray<ParameterSymbol>.Empty);
		ParameterSymbol? makeExtensionParameter(BindingDiagnosticBag bindingDiagnosticBag)
		{
			ParameterListSyntax parameterListSyntax = (ParameterListSyntax)syntaxReferenceOpt.GetSyntax();
			int count = parameterListSyntax.Parameters.Count;
			if (count == 0)
			{
				return null;
			}
			Binder withTypeParametersBinder = DeclaringCompilation.GetBinderFactory(parameterListSyntax.SyntaxTree).GetBinder(parameterListSyntax).WithAdditionalFlagsAndContainingMemberOrLambda(BinderFlags.SuppressConstraintChecks, this);
			for (int i = 1; i < count; i++)
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_ReceiverParameterOnlyOne, parameterListSyntax.Parameters[i].GetLocation());
			}
			ParameterSymbol parameterSymbol2 = ParameterHelpers.MakeExtensionReceiverParameter(withTypeParametersBinder, this, parameterListSyntax, bindingDiagnosticBag);
			if ((object)parameterSymbol2 != null)
			{
				TypeSymbol type = parameterSymbol2.TypeWithAnnotations.Type;
				RefKind refKind = parameterSymbol2.RefKind;
				SyntaxNode type2 = parameterListSyntax.Parameters[0].Type;
				if (!type.IsValidExtensionParameterType())
				{
					bindingDiagnosticBag.Add(ErrorCode.ERR_BadTypeforThis, type2, type);
				}
				else if (refKind == RefKind.Ref && !type.IsValueType)
				{
					bindingDiagnosticBag.Add(ErrorCode.ERR_RefExtensionParameterMustBeValueTypeOrConstrainedToOne, type2);
				}
				else
				{
					bool flag = refKind - 3 <= RefKind.Ref;
					if (flag && !type.IsValidInOrRefReadonlyExtensionParameterType())
					{
						bindingDiagnosticBag.Add(ErrorCode.ERR_InExtensionParameterMustBeValueType, type2);
					}
				}
				if (parameterSymbol2.Name == "" && refKind != RefKind.None)
				{
					bindingDiagnosticBag.Add(ErrorCode.ERR_ModifierOnUnnamedReceiverParameter, type2);
				}
			}
			if ((object)parameterSymbol2 != null)
			{
				string name = parameterSymbol2.Name;
				if (name != "" && ContainingType.TypeParameters.Any((TypeParameterSymbol p, string text) => p.Name == text, name))
				{
					bindingDiagnosticBag.Add(ErrorCode.ERR_ReceiverParameterSameNameAsTypeParameter, parameterSymbol2.GetFirstLocation(), name);
				}
			}
			return parameterSymbol2;
		}
	}
}
