using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis;

internal static class MarshalAsAttributeDecoder<TWellKnownAttributeData, TAttributeSyntax, TAttributeData, TAttributeLocation> where TWellKnownAttributeData : WellKnownAttributeData, IMarshalAsAttributeTarget, new() where TAttributeSyntax : SyntaxNode where TAttributeData : AttributeData
{
	internal static void Decode(ref DecodeWellKnownAttributeArguments<TAttributeSyntax, TAttributeData, TAttributeLocation> arguments, AttributeTargets target, CommonMessageProvider messageProvider)
	{
		UnmanagedType unmanagedType = DecodeMarshalAsType(arguments.Attribute);
		switch (unmanagedType)
		{
		case UnmanagedType.CustomMarshaler:
			DecodeMarshalAsCustom(ref arguments, messageProvider);
			break;
		case UnmanagedType.IUnknown:
		case UnmanagedType.IDispatch:
		case UnmanagedType.Interface:
			DecodeMarshalAsComInterface(ref arguments, unmanagedType, messageProvider);
			break;
		case UnmanagedType.LPArray:
			DecodeMarshalAsArray(ref arguments, messageProvider, isFixed: false);
			break;
		case UnmanagedType.ByValArray:
			if (target != AttributeTargets.Field)
			{
				messageProvider.ReportMarshalUnmanagedTypeOnlyValidForFields(arguments.Diagnostics, arguments.AttributeSyntaxOpt, 0, "ByValArray", arguments.Attribute);
			}
			else
			{
				DecodeMarshalAsArray(ref arguments, messageProvider, isFixed: true);
			}
			break;
		case UnmanagedType.SafeArray:
			DecodeMarshalAsSafeArray(ref arguments, messageProvider);
			break;
		case UnmanagedType.ByValTStr:
			if (target != AttributeTargets.Field)
			{
				messageProvider.ReportMarshalUnmanagedTypeOnlyValidForFields(arguments.Diagnostics, arguments.AttributeSyntaxOpt, 0, "ByValTStr", arguments.Attribute);
			}
			else
			{
				DecodeMarshalAsFixedString(ref arguments, messageProvider);
			}
			break;
		case UnmanagedType.VBByRefStr:
			if (target == AttributeTargets.Field)
			{
				messageProvider.ReportMarshalUnmanagedTypeNotValidForFields(arguments.Diagnostics, arguments.AttributeSyntaxOpt, 0, "VBByRefStr", arguments.Attribute);
			}
			else
			{
				arguments.GetOrCreateData<TWellKnownAttributeData>().GetOrCreateData().SetMarshalAsSimpleType(unmanagedType);
			}
			break;
		default:
			if (unmanagedType < (UnmanagedType)0 || unmanagedType > (UnmanagedType)536870911)
			{
				messageProvider.ReportInvalidAttributeArgument(arguments.Diagnostics, arguments.AttributeSyntaxOpt, 0, arguments.Attribute);
			}
			else
			{
				arguments.GetOrCreateData<TWellKnownAttributeData>().GetOrCreateData().SetMarshalAsSimpleType(unmanagedType);
			}
			break;
		}
	}

	private static UnmanagedType DecodeMarshalAsType(AttributeData attribute)
	{
		if (attribute.AttributeConstructor.Parameters[0].Type.SpecialType == SpecialType.System_Int16)
		{
			return (UnmanagedType)attribute.CommonConstructorArguments[0].DecodeValue<short>(SpecialType.System_Int16);
		}
		return attribute.CommonConstructorArguments[0].DecodeValue<UnmanagedType>(SpecialType.System_Enum);
	}

	private static void DecodeMarshalAsCustom(ref DecodeWellKnownAttributeArguments<TAttributeSyntax, TAttributeData, TAttributeLocation> arguments, CommonMessageProvider messageProvider)
	{
		ITypeSymbolInternal typeSymbolInternal = null;
		string text = null;
		string text2 = null;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		int num = arguments.Attribute.CommonConstructorArguments.Length;
		foreach (KeyValuePair<string, TypedConstant> namedArgument in arguments.Attribute.NamedArguments)
		{
			switch (namedArgument.Key)
			{
			case "MarshalType":
				text = namedArgument.Value.DecodeValue<string>(SpecialType.System_String);
				if (!MetadataHelpers.IsValidUnicodeString(text))
				{
					messageProvider.ReportInvalidNamedArgument(arguments.Diagnostics, arguments.AttributeSyntaxOpt, num, arguments.Attribute.AttributeClass, namedArgument.Key);
					flag3 = true;
				}
				flag = true;
				break;
			case "MarshalTypeRef":
				typeSymbolInternal = namedArgument.Value.DecodeValue<ITypeSymbolInternal>(SpecialType.None);
				flag2 = true;
				break;
			case "MarshalCookie":
				text2 = namedArgument.Value.DecodeValue<string>(SpecialType.System_String);
				if (!MetadataHelpers.IsValidUnicodeString(text2))
				{
					messageProvider.ReportInvalidNamedArgument(arguments.Diagnostics, arguments.AttributeSyntaxOpt, num, arguments.Attribute.AttributeClass, namedArgument.Key);
					flag3 = true;
				}
				break;
			}
			num++;
		}
		if (!flag && !flag2)
		{
			messageProvider.ReportAttributeParameterRequired(arguments.Diagnostics, arguments.AttributeSyntaxOpt, "MarshalType", "MarshalTypeRef");
			flag3 = true;
		}
		if (!flag3)
		{
			arguments.GetOrCreateData<TWellKnownAttributeData>().GetOrCreateData().SetMarshalAsCustom(flag ? ((object)text) : ((object)typeSymbolInternal), text2);
		}
	}

	private static void DecodeMarshalAsComInterface(ref DecodeWellKnownAttributeArguments<TAttributeSyntax, TAttributeData, TAttributeLocation> arguments, UnmanagedType unmanagedType, CommonMessageProvider messageProvider)
	{
		int? num = null;
		int num2 = arguments.Attribute.CommonConstructorArguments.Length;
		bool flag = false;
		foreach (KeyValuePair<string, TypedConstant> namedArgument in arguments.Attribute.NamedArguments)
		{
			if (namedArgument.Key == "IidParameterIndex")
			{
				num = namedArgument.Value.DecodeValue<int>(SpecialType.System_Int32);
				if (num < 0 || num > 536870911)
				{
					messageProvider.ReportInvalidNamedArgument(arguments.Diagnostics, arguments.AttributeSyntaxOpt, num2, arguments.Attribute.AttributeClass, namedArgument.Key);
					flag = true;
				}
			}
			num2++;
		}
		if (!flag)
		{
			arguments.GetOrCreateData<TWellKnownAttributeData>().GetOrCreateData().SetMarshalAsComInterface(unmanagedType, num);
		}
	}

	private static void DecodeMarshalAsArray(ref DecodeWellKnownAttributeArguments<TAttributeSyntax, TAttributeData, TAttributeLocation> arguments, CommonMessageProvider messageProvider, bool isFixed)
	{
		UnmanagedType? unmanagedType = null;
		int? num = null;
		short? num2 = null;
		bool flag = false;
		int i = arguments.Attribute.CommonConstructorArguments.Length;
		for (ImmutableArray<KeyValuePair<string, TypedConstant>>.Enumerator enumerator = arguments.Attribute.NamedArguments.GetEnumerator(); enumerator.MoveNext(); i++)
		{
			KeyValuePair<string, TypedConstant> current = enumerator.Current;
			switch (current.Key)
			{
			case "ArraySubType":
				unmanagedType = current.Value.DecodeValue<UnmanagedType>(SpecialType.System_Enum);
				if ((!isFixed && unmanagedType == UnmanagedType.CustomMarshaler) || unmanagedType.Value < (UnmanagedType)0 || unmanagedType.Value > (UnmanagedType)536870911)
				{
					messageProvider.ReportInvalidNamedArgument(arguments.Diagnostics, arguments.AttributeSyntaxOpt, i, arguments.Attribute.AttributeClass, current.Key);
					flag = true;
				}
				continue;
			case "SizeConst":
				num = current.Value.DecodeValue<int>(SpecialType.System_Int32);
				if (num < 0 || num > 536870911)
				{
					messageProvider.ReportInvalidNamedArgument(arguments.Diagnostics, arguments.AttributeSyntaxOpt, i, arguments.Attribute.AttributeClass, current.Key);
					flag = true;
				}
				continue;
			case "SizeParamIndex":
				if (!isFixed)
				{
					num2 = current.Value.DecodeValue<short>(SpecialType.System_Int16);
					if (num2 < 0)
					{
						messageProvider.ReportInvalidNamedArgument(arguments.Diagnostics, arguments.AttributeSyntaxOpt, i, arguments.Attribute.AttributeClass, current.Key);
						flag = true;
					}
					continue;
				}
				break;
			case "SafeArraySubType":
				break;
			default:
				continue;
			}
			messageProvider.ReportParameterNotValidForType(arguments.Diagnostics, arguments.AttributeSyntaxOpt, i);
			flag = true;
		}
		if (isFixed && !num.HasValue)
		{
			int? wRN_ByValArraySizeConstRequired = messageProvider.WRN_ByValArraySizeConstRequired;
			if (wRN_ByValArraySizeConstRequired.HasValue)
			{
				int valueOrDefault = wRN_ByValArraySizeConstRequired.GetValueOrDefault();
				arguments.Diagnostics.Add(messageProvider.CreateDiagnostic(valueOrDefault, arguments.AttributeSyntaxOpt.GetLocation()));
			}
			num = 1;
		}
		if (!flag)
		{
			MarshalPseudoCustomAttributeData orCreateData = arguments.GetOrCreateData<TWellKnownAttributeData>().GetOrCreateData();
			if (isFixed)
			{
				orCreateData.SetMarshalAsFixedArray(unmanagedType, num);
			}
			else
			{
				orCreateData.SetMarshalAsArray(unmanagedType, num, num2);
			}
		}
	}

	private static void DecodeMarshalAsSafeArray(ref DecodeWellKnownAttributeArguments<TAttributeSyntax, TAttributeData, TAttributeLocation> arguments, CommonMessageProvider messageProvider)
	{
		Microsoft.Cci.VarEnum? varEnum = null;
		ITypeSymbolInternal elementTypeSymbol = null;
		int num = -1;
		bool flag = false;
		int num2 = arguments.Attribute.CommonConstructorArguments.Length;
		foreach (KeyValuePair<string, TypedConstant> namedArgument in arguments.Attribute.NamedArguments)
		{
			switch (namedArgument.Key)
			{
			case "SafeArraySubType":
				varEnum = namedArgument.Value.DecodeValue<Microsoft.Cci.VarEnum>(SpecialType.System_Enum);
				if (varEnum < Microsoft.Cci.VarEnum.VT_EMPTY || varEnum.Value > (Microsoft.Cci.VarEnum)536870911)
				{
					messageProvider.ReportInvalidNamedArgument(arguments.Diagnostics, arguments.AttributeSyntaxOpt, num2, arguments.Attribute.AttributeClass, namedArgument.Key);
					flag = true;
				}
				break;
			case "SafeArrayUserDefinedSubType":
				elementTypeSymbol = namedArgument.Value.DecodeValue<ITypeSymbolInternal>(SpecialType.None);
				num = num2;
				break;
			case "ArraySubType":
			case "SizeConst":
			case "SizeParamIndex":
				messageProvider.ReportParameterNotValidForType(arguments.Diagnostics, arguments.AttributeSyntaxOpt, num2);
				flag = true;
				break;
			}
			num2++;
		}
		switch (varEnum)
		{
		default:
			if (varEnum.HasValue && num >= 0)
			{
				messageProvider.ReportParameterNotValidForType(arguments.Diagnostics, arguments.AttributeSyntaxOpt, num);
				flag = true;
			}
			else
			{
				elementTypeSymbol = null;
			}
			break;
		case Microsoft.Cci.VarEnum.VT_DISPATCH:
		case Microsoft.Cci.VarEnum.VT_UNKNOWN:
		case Microsoft.Cci.VarEnum.VT_RECORD:
			break;
		}
		if (!flag)
		{
			arguments.GetOrCreateData<TWellKnownAttributeData>().GetOrCreateData().SetMarshalAsSafeArray(varEnum, elementTypeSymbol);
		}
	}

	private static void DecodeMarshalAsFixedString(ref DecodeWellKnownAttributeArguments<TAttributeSyntax, TAttributeData, TAttributeLocation> arguments, CommonMessageProvider messageProvider)
	{
		int num = -1;
		int num2 = arguments.Attribute.CommonConstructorArguments.Length;
		bool flag = false;
		foreach (KeyValuePair<string, TypedConstant> namedArgument in arguments.Attribute.NamedArguments)
		{
			switch (namedArgument.Key)
			{
			case "SizeConst":
				num = namedArgument.Value.DecodeValue<int>(SpecialType.System_Int32);
				if (num < 0 || num > 536870911)
				{
					messageProvider.ReportInvalidNamedArgument(arguments.Diagnostics, arguments.AttributeSyntaxOpt, num2, arguments.Attribute.AttributeClass, namedArgument.Key);
					flag = true;
				}
				break;
			case "ArraySubType":
			case "SizeParamIndex":
				messageProvider.ReportParameterNotValidForType(arguments.Diagnostics, arguments.AttributeSyntaxOpt, num2);
				flag = true;
				break;
			}
			num2++;
		}
		if (num < 0)
		{
			messageProvider.ReportAttributeParameterRequired(arguments.Diagnostics, arguments.AttributeSyntaxOpt, "SizeConst");
			flag = true;
		}
		if (!flag)
		{
			arguments.GetOrCreateData<TWellKnownAttributeData>().GetOrCreateData().SetMarshalAsFixedString(num);
		}
	}
}
