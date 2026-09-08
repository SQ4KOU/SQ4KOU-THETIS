using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class UserDefinedConversionAnalysis
{
	public readonly TypeSymbol FromType;

	public readonly TypeSymbol ToType;

	public readonly TypeParameterSymbol ConstrainedToTypeOpt;

	public readonly MethodSymbol Operator;

	public readonly Conversion SourceConversion;

	public readonly Conversion TargetConversion;

	public readonly UserDefinedConversionAnalysisKind Kind;

	public static UserDefinedConversionAnalysis Normal(TypeParameterSymbol constrainedToTypeOpt, MethodSymbol op, Conversion sourceConversion, Conversion targetConversion, TypeSymbol fromType, TypeSymbol toType)
	{
		return new UserDefinedConversionAnalysis(UserDefinedConversionAnalysisKind.ApplicableInNormalForm, constrainedToTypeOpt, op, sourceConversion, targetConversion, fromType, toType);
	}

	public static UserDefinedConversionAnalysis Lifted(TypeParameterSymbol constrainedToTypeOpt, MethodSymbol op, Conversion sourceConversion, Conversion targetConversion, TypeSymbol fromType, TypeSymbol toType)
	{
		return new UserDefinedConversionAnalysis(UserDefinedConversionAnalysisKind.ApplicableInLiftedForm, constrainedToTypeOpt, op, sourceConversion, targetConversion, fromType, toType);
	}

	private UserDefinedConversionAnalysis(UserDefinedConversionAnalysisKind kind, TypeParameterSymbol constrainedToTypeOpt, MethodSymbol op, Conversion sourceConversion, Conversion targetConversion, TypeSymbol fromType, TypeSymbol toType)
	{
		Kind = kind;
		ConstrainedToTypeOpt = constrainedToTypeOpt;
		Operator = op;
		SourceConversion = sourceConversion;
		TargetConversion = targetConversion;
		FromType = fromType;
		ToType = toType;
	}
}
