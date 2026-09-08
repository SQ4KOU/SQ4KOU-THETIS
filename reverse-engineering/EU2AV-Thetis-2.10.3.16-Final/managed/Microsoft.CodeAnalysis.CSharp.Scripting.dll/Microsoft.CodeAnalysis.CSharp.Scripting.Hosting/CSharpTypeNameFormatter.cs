using System;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Scripting.Hosting;

namespace Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;

internal class CSharpTypeNameFormatter : CommonTypeNameFormatter
{
	protected override CommonPrimitiveFormatter PrimitiveFormatter { get; }

	protected override string GenericParameterOpening => "<";

	protected override string GenericParameterClosing => ">";

	protected override string ArrayOpening => "[";

	protected override string ArrayClosing => "]";

	public CSharpTypeNameFormatter(CommonPrimitiveFormatter primitiveFormatter)
	{
		PrimitiveFormatter = primitiveFormatter;
	}

	protected override string GetPrimitiveTypeName(SpecialType type)
	{
		return type switch
		{
			SpecialType.System_Boolean => "bool", 
			SpecialType.System_Byte => "byte", 
			SpecialType.System_Char => "char", 
			SpecialType.System_Decimal => "decimal", 
			SpecialType.System_Double => "double", 
			SpecialType.System_Int16 => "short", 
			SpecialType.System_Int32 => "int", 
			SpecialType.System_Int64 => "long", 
			SpecialType.System_SByte => "sbyte", 
			SpecialType.System_Single => "float", 
			SpecialType.System_String => "string", 
			SpecialType.System_UInt16 => "ushort", 
			SpecialType.System_UInt32 => "uint", 
			SpecialType.System_UInt64 => "ulong", 
			SpecialType.System_Object => "object", 
			_ => null, 
		};
	}

	public override string FormatTypeName(Type type, CommonTypeNameFormatterOptions options)
	{
		if (GeneratedNameParser.TryParseSourceMethodNameFromGeneratedName(type.Name, GeneratedNameKind.StateMachineType, out string methodName))
		{
			return methodName;
		}
		return base.FormatTypeName(type, options);
	}
}
