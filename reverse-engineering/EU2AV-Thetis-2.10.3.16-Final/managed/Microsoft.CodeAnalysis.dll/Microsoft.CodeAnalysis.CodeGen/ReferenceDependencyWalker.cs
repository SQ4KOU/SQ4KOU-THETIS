using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CodeGen;

internal static class ReferenceDependencyWalker
{
	public static void VisitReference(IReference reference, EmitContext context)
	{
		if (reference is ITypeReference typeReference)
		{
			VisitTypeReference(typeReference, context);
		}
		else if (reference is IMethodReference methodReference)
		{
			VisitMethodReference(methodReference, context);
		}
		else if (reference is IFieldReference fieldReference)
		{
			VisitFieldReference(fieldReference, context);
		}
	}

	private static void VisitTypeReference(ITypeReference typeReference, EmitContext context)
	{
		if (typeReference is IArrayTypeReference arrayTypeReference)
		{
			VisitTypeReference(arrayTypeReference.GetElementType(context), context);
			return;
		}
		if (typeReference is IPointerTypeReference pointerTypeReference)
		{
			VisitTypeReference(pointerTypeReference.GetTargetType(context), context);
			return;
		}
		if (typeReference is IModifiedTypeReference modifiedTypeReference)
		{
			VisitCustomModifiers(modifiedTypeReference.CustomModifiers, in context);
			VisitTypeReference(modifiedTypeReference.UnmodifiedType, context);
			return;
		}
		INestedTypeReference asNestedTypeReference = typeReference.AsNestedTypeReference;
		if (asNestedTypeReference != null)
		{
			VisitTypeReference(asNestedTypeReference.GetContainingType(context), context);
		}
		IGenericTypeInstanceReference asGenericTypeInstanceReference = typeReference.AsGenericTypeInstanceReference;
		if (asGenericTypeInstanceReference != null)
		{
			foreach (ITypeReference genericArgument in asGenericTypeInstanceReference.GetGenericArguments(context))
			{
				VisitTypeReference(genericArgument, context);
			}
		}
		if (typeReference is IFunctionPointerTypeReference functionPointerTypeReference)
		{
			VisitSignature(functionPointerTypeReference.Signature, context);
		}
	}

	private static void VisitMethodReference(IMethodReference methodReference, EmitContext context)
	{
		VisitTypeReference(methodReference.GetContainingType(context), context);
		IGenericMethodInstanceReference asGenericMethodInstanceReference = methodReference.AsGenericMethodInstanceReference;
		if (asGenericMethodInstanceReference != null)
		{
			foreach (ITypeReference genericArgument in asGenericMethodInstanceReference.GetGenericArguments(context))
			{
				VisitTypeReference(genericArgument, context);
			}
			methodReference = asGenericMethodInstanceReference.GetGenericMethod(context);
		}
		ISpecializedMethodReference asSpecializedMethodReference = methodReference.AsSpecializedMethodReference;
		if (asSpecializedMethodReference != null)
		{
			methodReference = asSpecializedMethodReference.UnspecializedVersion;
		}
		VisitSignature(methodReference, context);
		if (methodReference.AcceptsExtraArguments)
		{
			VisitParameters(methodReference.ExtraParameters, context);
		}
	}

	internal static void VisitSignature(ISignature signature, EmitContext context)
	{
		VisitParameters(signature.GetParameters(context), context);
		VisitTypeReference(signature.GetType(context), context);
		VisitCustomModifiers(signature.RefCustomModifiers, in context);
		foreach (ICustomModifier returnValueCustomModifier in signature.ReturnValueCustomModifiers)
		{
			VisitTypeReference(returnValueCustomModifier.GetModifier(context), context);
		}
	}

	private static void VisitParameters(ImmutableArray<IParameterTypeInformation> parameters, EmitContext context)
	{
		foreach (IParameterTypeInformation item in parameters)
		{
			VisitTypeReference(item.GetType(context), context);
			VisitCustomModifiers(item.RefCustomModifiers, in context);
			VisitCustomModifiers(item.CustomModifiers, in context);
		}
	}

	private static void VisitFieldReference(IFieldReference fieldReference, EmitContext context)
	{
		VisitTypeReference(fieldReference.GetContainingType(context), context);
		ISpecializedFieldReference asSpecializedFieldReference = fieldReference.AsSpecializedFieldReference;
		if (asSpecializedFieldReference != null)
		{
			fieldReference = asSpecializedFieldReference.UnspecializedVersion;
		}
		VisitTypeReference(fieldReference.GetType(context), context);
		VisitCustomModifiers(fieldReference.RefCustomModifiers, in context);
	}

	private static void VisitCustomModifiers(ImmutableArray<ICustomModifier> customModifiers, in EmitContext context)
	{
		foreach (ICustomModifier item in customModifiers)
		{
			VisitTypeReference(item.GetModifier(context), context);
		}
	}
}
