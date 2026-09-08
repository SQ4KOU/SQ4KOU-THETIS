using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class OperatorFacts
{
	public static bool DefinitelyHasNoUserDefinedOperators(TypeSymbol type)
	{
		TypeKind typeKind = type.TypeKind;
		if (typeKind != TypeKind.Class && typeKind != TypeKind.Interface && typeKind - 10 > TypeKind.Array)
		{
			return true;
		}
		switch (type.SpecialType)
		{
		case SpecialType.System_IntPtr:
			if (!type.IsNativeIntegerType)
			{
				break;
			}
			goto case SpecialType.System_Object;
		case SpecialType.System_UIntPtr:
			if (!type.IsNativeIntegerType)
			{
				break;
			}
			goto case SpecialType.System_Object;
		case SpecialType.System_Object:
		case SpecialType.System_Enum:
		case SpecialType.System_MulticastDelegate:
		case SpecialType.System_Delegate:
		case SpecialType.System_ValueType:
		case SpecialType.System_Void:
		case SpecialType.System_Boolean:
		case SpecialType.System_Char:
		case SpecialType.System_SByte:
		case SpecialType.System_Byte:
		case SpecialType.System_Int16:
		case SpecialType.System_UInt16:
		case SpecialType.System_Int32:
		case SpecialType.System_UInt32:
		case SpecialType.System_Int64:
		case SpecialType.System_UInt64:
		case SpecialType.System_Decimal:
		case SpecialType.System_Single:
		case SpecialType.System_Double:
		case SpecialType.System_String:
		case SpecialType.System_Array:
			return true;
		}
		return false;
	}

	public static string BinaryOperatorNameFromSyntaxKind(SyntaxKind kind, bool isChecked)
	{
		object obj = BinaryOperatorNameFromSyntaxKindIfAny(kind, isChecked);
		if (obj == null)
		{
			if (!isChecked)
			{
				return "op_Addition";
			}
			obj = "op_CheckedAddition";
		}
		return (string)obj;
	}

	internal static string BinaryOperatorNameFromSyntaxKindIfAny(SyntaxKind kind, bool isChecked)
	{
		switch (kind)
		{
		case SyntaxKind.PlusToken:
			if (!isChecked)
			{
				return "op_Addition";
			}
			return "op_CheckedAddition";
		case SyntaxKind.MinusToken:
			if (!isChecked)
			{
				return "op_Subtraction";
			}
			return "op_CheckedSubtraction";
		case SyntaxKind.AsteriskToken:
			if (!isChecked)
			{
				return "op_Multiply";
			}
			return "op_CheckedMultiply";
		case SyntaxKind.SlashToken:
			if (!isChecked)
			{
				return "op_Division";
			}
			return "op_CheckedDivision";
		case SyntaxKind.PercentToken:
			return "op_Modulus";
		case SyntaxKind.CaretToken:
			return "op_ExclusiveOr";
		case SyntaxKind.AmpersandToken:
			return "op_BitwiseAnd";
		case SyntaxKind.BarToken:
			return "op_BitwiseOr";
		case SyntaxKind.EqualsEqualsToken:
			return "op_Equality";
		case SyntaxKind.LessThanToken:
			return "op_LessThan";
		case SyntaxKind.LessThanEqualsToken:
			return "op_LessThanOrEqual";
		case SyntaxKind.LessThanLessThanToken:
			return "op_LeftShift";
		case SyntaxKind.GreaterThanToken:
			return "op_GreaterThan";
		case SyntaxKind.GreaterThanEqualsToken:
			return "op_GreaterThanOrEqual";
		case SyntaxKind.GreaterThanGreaterThanToken:
			return "op_RightShift";
		case SyntaxKind.GreaterThanGreaterThanGreaterThanToken:
			return "op_UnsignedRightShift";
		case SyntaxKind.ExclamationEqualsToken:
			return "op_Inequality";
		default:
			return null;
		}
	}

	internal static string CompoundAssignmentOperatorNameFromSyntaxKind(SyntaxKind kind, bool isChecked)
	{
		switch (kind)
		{
		case SyntaxKind.PlusEqualsToken:
			if (!isChecked)
			{
				return "op_AdditionAssignment";
			}
			return "op_CheckedAdditionAssignment";
		case SyntaxKind.MinusEqualsToken:
			if (!isChecked)
			{
				return "op_SubtractionAssignment";
			}
			return "op_CheckedSubtractionAssignment";
		case SyntaxKind.AsteriskEqualsToken:
			if (!isChecked)
			{
				return "op_MultiplicationAssignment";
			}
			return "op_CheckedMultiplicationAssignment";
		case SyntaxKind.SlashEqualsToken:
			if (!isChecked)
			{
				return "op_DivisionAssignment";
			}
			return "op_CheckedDivisionAssignment";
		case SyntaxKind.PercentEqualsToken:
			return "op_ModulusAssignment";
		case SyntaxKind.CaretEqualsToken:
			return "op_ExclusiveOrAssignment";
		case SyntaxKind.AmpersandEqualsToken:
			return "op_BitwiseAndAssignment";
		case SyntaxKind.BarEqualsToken:
			return "op_BitwiseOrAssignment";
		case SyntaxKind.LessThanLessThanEqualsToken:
			return "op_LeftShiftAssignment";
		case SyntaxKind.GreaterThanGreaterThanEqualsToken:
			return "op_RightShiftAssignment";
		case SyntaxKind.GreaterThanGreaterThanGreaterThanEqualsToken:
			return "op_UnsignedRightShiftAssignment";
		case SyntaxKind.PlusPlusToken:
			if (!isChecked)
			{
				return "op_IncrementAssignment";
			}
			return "op_CheckedIncrementAssignment";
		case SyntaxKind.MinusMinusToken:
			if (!isChecked)
			{
				return "op_DecrementAssignment";
			}
			return "op_CheckedDecrementAssignment";
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
	}

	internal static bool IsCompoundAssignmentOperatorName(string operatorMetadataName)
	{
		switch (operatorMetadataName)
		{
		case "op_CheckedDecrementAssignment":
		case "op_CheckedIncrementAssignment":
		case "op_IncrementAssignment":
		case "op_LeftShiftAssignment":
		case "op_BitwiseOrAssignment":
		case "op_DecrementAssignment":
		case "op_AdditionAssignment":
		case "op_DivisionAssignment":
		case "op_ExclusiveOrAssignment":
		case "op_SubtractionAssignment":
		case "op_BitwiseAndAssignment":
		case "op_RightShiftAssignment":
		case "op_CheckedSubtractionAssignment":
		case "op_UnsignedRightShiftAssignment":
		case "op_CheckedAdditionAssignment":
		case "op_CheckedDivisionAssignment":
		case "op_MultiplicationAssignment":
		case "op_ModulusAssignment":
		case "op_CheckedMultiplicationAssignment":
			return true;
		default:
			return false;
		}
	}

	public static string UnaryOperatorNameFromSyntaxKind(SyntaxKind kind, bool isChecked)
	{
		return UnaryOperatorNameFromSyntaxKindIfAny(kind, isChecked) ?? "op_UnaryPlus";
	}

	internal static string UnaryOperatorNameFromSyntaxKindIfAny(SyntaxKind kind, bool isChecked)
	{
		switch (kind)
		{
		case SyntaxKind.PlusToken:
			return "op_UnaryPlus";
		case SyntaxKind.MinusToken:
			if (!isChecked)
			{
				return "op_UnaryNegation";
			}
			return "op_CheckedUnaryNegation";
		case SyntaxKind.TildeToken:
			return "op_OnesComplement";
		case SyntaxKind.ExclamationToken:
			return "op_LogicalNot";
		case SyntaxKind.PlusPlusToken:
			if (!isChecked)
			{
				return "op_Increment";
			}
			return "op_CheckedIncrement";
		case SyntaxKind.MinusMinusToken:
			if (!isChecked)
			{
				return "op_Decrement";
			}
			return "op_CheckedDecrement";
		case SyntaxKind.TrueKeyword:
			return "op_True";
		case SyntaxKind.FalseKeyword:
			return "op_False";
		default:
			return null;
		}
	}

	public static string OperatorNameFromDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.OperatorDeclarationSyntax declaration)
	{
		return OperatorNameFromDeclaration((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.OperatorDeclarationSyntax)declaration.Green);
	}

	public static string OperatorNameFromDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.OperatorDeclarationSyntax declaration)
	{
		SyntaxKind kind = declaration.OperatorToken.Kind;
		Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken? checkedKeyword = declaration.CheckedKeyword;
		bool isChecked = checkedKeyword != null && checkedKeyword.Kind == SyntaxKind.CheckedKeyword;
		if (SyntaxFacts.IsBinaryExpressionOperatorToken(kind))
		{
			if (kind != SyntaxKind.AsteriskToken && SyntaxFacts.IsPrefixUnaryExpressionOperatorToken(kind) && declaration.ParameterList.Parameters.Count == 1)
			{
				return UnaryOperatorNameFromSyntaxKind(kind, isChecked);
			}
			return BinaryOperatorNameFromSyntaxKind(kind, isChecked);
		}
		if (SyntaxFacts.IsUnaryOperatorDeclarationToken(kind))
		{
			bool flag = kind - 8262 <= SyntaxKind.List;
			if (flag && declaration.ParameterList.Parameters.Count == 0)
			{
				return CompoundAssignmentOperatorNameFromSyntaxKind(kind, isChecked);
			}
			return UnaryOperatorNameFromSyntaxKind(kind, isChecked);
		}
		if (SyntaxFacts.IsOverloadableCompoundAssignmentOperator(kind))
		{
			return CompoundAssignmentOperatorNameFromSyntaxKind(kind, isChecked);
		}
		return "op_UnaryPlus";
	}

	public static string OperatorNameFromDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.ConversionOperatorDeclarationSyntax declaration)
	{
		return OperatorNameFromDeclaration((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConversionOperatorDeclarationSyntax)declaration.Green);
	}

	public static string OperatorNameFromDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConversionOperatorDeclarationSyntax declaration)
	{
		if (declaration.ImplicitOrExplicitKeyword.Kind == SyntaxKind.ImplicitKeyword)
		{
			return "op_Implicit";
		}
		Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken? checkedKeyword = declaration.CheckedKeyword;
		if (checkedKeyword == null || checkedKeyword.Kind != SyntaxKind.CheckedKeyword)
		{
			return "op_Explicit";
		}
		return "op_CheckedExplicit";
	}

	public static string UnaryOperatorNameFromOperatorKind(UnaryOperatorKind kind, bool isChecked)
	{
		switch (kind & UnaryOperatorKind.OpMask)
		{
		case UnaryOperatorKind.UnaryPlus:
			return "op_UnaryPlus";
		case UnaryOperatorKind.UnaryMinus:
			if (!isChecked)
			{
				return "op_UnaryNegation";
			}
			return "op_CheckedUnaryNegation";
		case UnaryOperatorKind.BitwiseComplement:
			return "op_OnesComplement";
		case UnaryOperatorKind.LogicalNegation:
			return "op_LogicalNot";
		case UnaryOperatorKind.PostfixIncrement:
		case UnaryOperatorKind.PrefixIncrement:
			if (!isChecked)
			{
				return "op_Increment";
			}
			return "op_CheckedIncrement";
		case UnaryOperatorKind.PostfixDecrement:
		case UnaryOperatorKind.PrefixDecrement:
			if (!isChecked)
			{
				return "op_Decrement";
			}
			return "op_CheckedDecrement";
		case UnaryOperatorKind.True:
			return "op_True";
		case UnaryOperatorKind.False:
			return "op_False";
		default:
			throw ExceptionUtilities.UnexpectedValue(kind & UnaryOperatorKind.OpMask);
		}
	}

	public static string BinaryOperatorNameFromOperatorKind(BinaryOperatorKind kind, bool isChecked)
	{
		switch (kind & BinaryOperatorKind.OpMask)
		{
		case BinaryOperatorKind.Addition:
			if (!isChecked)
			{
				return "op_Addition";
			}
			return "op_CheckedAddition";
		case BinaryOperatorKind.And:
			return "op_BitwiseAnd";
		case BinaryOperatorKind.Division:
			if (!isChecked)
			{
				return "op_Division";
			}
			return "op_CheckedDivision";
		case BinaryOperatorKind.Equal:
			return "op_Equality";
		case BinaryOperatorKind.GreaterThan:
			return "op_GreaterThan";
		case BinaryOperatorKind.GreaterThanOrEqual:
			return "op_GreaterThanOrEqual";
		case BinaryOperatorKind.LeftShift:
			return "op_LeftShift";
		case BinaryOperatorKind.LessThan:
			return "op_LessThan";
		case BinaryOperatorKind.LessThanOrEqual:
			return "op_LessThanOrEqual";
		case BinaryOperatorKind.Multiplication:
			if (!isChecked)
			{
				return "op_Multiply";
			}
			return "op_CheckedMultiply";
		case BinaryOperatorKind.Or:
			return "op_BitwiseOr";
		case BinaryOperatorKind.NotEqual:
			return "op_Inequality";
		case BinaryOperatorKind.Remainder:
			return "op_Modulus";
		case BinaryOperatorKind.RightShift:
			return "op_RightShift";
		case BinaryOperatorKind.UnsignedRightShift:
			return "op_UnsignedRightShift";
		case BinaryOperatorKind.Subtraction:
			if (!isChecked)
			{
				return "op_Subtraction";
			}
			return "op_CheckedSubtraction";
		case BinaryOperatorKind.Xor:
			return "op_ExclusiveOr";
		default:
			throw ExceptionUtilities.UnexpectedValue(kind & BinaryOperatorKind.OpMask);
		}
	}
}
