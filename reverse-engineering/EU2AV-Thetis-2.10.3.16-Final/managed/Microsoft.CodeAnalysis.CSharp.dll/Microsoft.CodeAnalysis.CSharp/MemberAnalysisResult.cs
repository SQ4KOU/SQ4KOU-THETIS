using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal readonly struct MemberAnalysisResult
{
	public readonly ImmutableArray<Conversion> ConversionsOpt;

	public readonly BitVector BadArgumentsOpt;

	public readonly ImmutableArray<int> ArgsToParamsOpt;

	public readonly ImmutableArray<TypeParameterDiagnosticInfo> ConstraintFailureDiagnostics;

	public readonly int BadParameter;

	public readonly MemberResolutionKind Kind;

	public readonly TypeWithAnnotations DefinitionParamsElementTypeOpt;

	public readonly TypeWithAnnotations ParamsElementTypeOpt;

	public readonly bool HasAnyRefOmittedArgument;

	public int FirstBadArgument => BadArgumentsOpt.TrueBits().First();

	public bool IsApplicable
	{
		get
		{
			MemberResolutionKind kind = Kind;
			if (kind - 1 <= MemberResolutionKind.ApplicableInNormalForm || kind - 22 <= MemberResolutionKind.ApplicableInNormalForm)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsValid
	{
		get
		{
			MemberResolutionKind kind = Kind;
			if (kind - 1 <= MemberResolutionKind.ApplicableInNormalForm)
			{
				return true;
			}
			return false;
		}
	}

	private MemberAnalysisResult(MemberResolutionKind kind, BitVector badArgumentsOpt = default(BitVector), ImmutableArray<int> argsToParamsOpt = default(ImmutableArray<int>), ImmutableArray<Conversion> conversionsOpt = default(ImmutableArray<Conversion>), int missingParameter = -1, bool hasAnyRefOmittedArgument = false, ImmutableArray<TypeParameterDiagnosticInfo> constraintFailureDiagnosticsOpt = default(ImmutableArray<TypeParameterDiagnosticInfo>), TypeWithAnnotations definitionParamsElementTypeOpt = default(TypeWithAnnotations), TypeWithAnnotations paramsElementTypeOpt = default(TypeWithAnnotations))
	{
		Kind = kind;
		DefinitionParamsElementTypeOpt = definitionParamsElementTypeOpt;
		ParamsElementTypeOpt = paramsElementTypeOpt;
		BadArgumentsOpt = badArgumentsOpt;
		ArgsToParamsOpt = argsToParamsOpt;
		ConversionsOpt = conversionsOpt;
		BadParameter = missingParameter;
		HasAnyRefOmittedArgument = hasAnyRefOmittedArgument;
		ConstraintFailureDiagnostics = constraintFailureDiagnosticsOpt.NullToEmpty();
	}

	public override bool Equals(object obj)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/Semantics/OverloadResolution/MemberAnalysisResult.cs", 146);
	}

	public override int GetHashCode()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/Semantics/OverloadResolution/MemberAnalysisResult.cs", 151);
	}

	public Conversion ConversionForArg(int arg)
	{
		if (ConversionsOpt.IsDefault)
		{
			return Conversion.Identity;
		}
		return ConversionsOpt[arg];
	}

	public int ParameterFromArgument(int arg)
	{
		if (ArgsToParamsOpt.IsDefault)
		{
			return arg;
		}
		return ArgsToParamsOpt[arg];
	}

	internal bool HasUseSiteDiagnosticToReportFor(Symbol symbol)
	{
		if (!SuppressUseSiteDiagnosticsForKind(Kind) && (object)symbol != null)
		{
			return symbol.GetUseSiteInfo().DiagnosticInfo != null;
		}
		return false;
	}

	private static bool SuppressUseSiteDiagnosticsForKind(MemberResolutionKind kind)
	{
		switch (kind)
		{
		case MemberResolutionKind.UnsupportedMetadata:
			return true;
		case MemberResolutionKind.NoCorrespondingParameter:
		case MemberResolutionKind.NoCorrespondingNamedParameter:
		case MemberResolutionKind.DuplicateNamedArgument:
		case MemberResolutionKind.RequiredParameterMissing:
		case MemberResolutionKind.NameUsedForPositional:
		case MemberResolutionKind.LessDerived:
			return true;
		default:
			return false;
		}
	}

	public static MemberAnalysisResult ArgumentParameterMismatch(ArgumentAnalysisResult argAnalysis)
	{
		return argAnalysis.Kind switch
		{
			ArgumentAnalysisResultKind.NoCorrespondingParameter => NoCorrespondingParameter(argAnalysis.ArgumentPosition), 
			ArgumentAnalysisResultKind.NoCorrespondingNamedParameter => NoCorrespondingNamedParameter(argAnalysis.ArgumentPosition), 
			ArgumentAnalysisResultKind.DuplicateNamedArgument => DuplicateNamedArgument(argAnalysis.ArgumentPosition), 
			ArgumentAnalysisResultKind.RequiredParameterMissing => RequiredParameterMissing(argAnalysis.ParameterPosition), 
			ArgumentAnalysisResultKind.NameUsedForPositional => NameUsedForPositional(argAnalysis.ArgumentPosition), 
			ArgumentAnalysisResultKind.BadNonTrailingNamedArgument => BadNonTrailingNamedArgument(argAnalysis.ArgumentPosition), 
			_ => throw ExceptionUtilities.UnexpectedValue(argAnalysis.Kind), 
		};
	}

	public static MemberAnalysisResult NameUsedForPositional(int argumentPosition)
	{
		return new MemberAnalysisResult(MemberResolutionKind.NameUsedForPositional, CreateBadArgumentsWithPosition(argumentPosition));
	}

	public static MemberAnalysisResult BadNonTrailingNamedArgument(int argumentPosition)
	{
		return new MemberAnalysisResult(MemberResolutionKind.BadNonTrailingNamedArgument, CreateBadArgumentsWithPosition(argumentPosition));
	}

	public static MemberAnalysisResult NoCorrespondingParameter(int argumentPosition)
	{
		return new MemberAnalysisResult(MemberResolutionKind.NoCorrespondingParameter, CreateBadArgumentsWithPosition(argumentPosition));
	}

	public static MemberAnalysisResult NoCorrespondingNamedParameter(int argumentPosition)
	{
		return new MemberAnalysisResult(MemberResolutionKind.NoCorrespondingNamedParameter, CreateBadArgumentsWithPosition(argumentPosition));
	}

	public static MemberAnalysisResult DuplicateNamedArgument(int argumentPosition)
	{
		return new MemberAnalysisResult(MemberResolutionKind.DuplicateNamedArgument, CreateBadArgumentsWithPosition(argumentPosition));
	}

	internal static BitVector CreateBadArgumentsWithPosition(int argumentPosition)
	{
		BitVector result = BitVector.Create(argumentPosition + 1);
		result[argumentPosition] = true;
		return result;
	}

	public static MemberAnalysisResult RequiredParameterMissing(int parameterPosition)
	{
		return new MemberAnalysisResult(MemberResolutionKind.RequiredParameterMissing, default(BitVector), default(ImmutableArray<int>), default(ImmutableArray<Conversion>), parameterPosition);
	}

	public static MemberAnalysisResult UseSiteError()
	{
		return new MemberAnalysisResult(MemberResolutionKind.UseSiteError);
	}

	public static MemberAnalysisResult UnsupportedMetadata()
	{
		return new MemberAnalysisResult(MemberResolutionKind.UnsupportedMetadata);
	}

	public static MemberAnalysisResult BadArgumentConversions(ImmutableArray<int> argsToParamsOpt, BitVector badArguments, ImmutableArray<Conversion> conversions, TypeWithAnnotations definitionParamsElementTypeOpt, TypeWithAnnotations paramsElementTypeOpt)
	{
		return new MemberAnalysisResult(MemberResolutionKind.BadArgumentConversion, badArguments, argsToParamsOpt, conversions, -1, hasAnyRefOmittedArgument: false, default(ImmutableArray<TypeParameterDiagnosticInfo>), definitionParamsElementTypeOpt, paramsElementTypeOpt);
	}

	public static MemberAnalysisResult InaccessibleTypeArgument()
	{
		return new MemberAnalysisResult(MemberResolutionKind.InaccessibleTypeArgument);
	}

	public static MemberAnalysisResult TypeInferenceFailed()
	{
		return new MemberAnalysisResult(MemberResolutionKind.TypeInferenceFailed);
	}

	public static MemberAnalysisResult TypeInferenceExtensionInstanceArgumentFailed()
	{
		return new MemberAnalysisResult(MemberResolutionKind.TypeInferenceExtensionInstanceArgument);
	}

	public static MemberAnalysisResult StaticInstanceMismatch()
	{
		return new MemberAnalysisResult(MemberResolutionKind.StaticInstanceMismatch);
	}

	public static MemberAnalysisResult ConstructedParameterFailedConstraintsCheck(int parameterPosition)
	{
		return new MemberAnalysisResult(MemberResolutionKind.ConstructedParameterFailedConstraintCheck, default(BitVector), default(ImmutableArray<int>), default(ImmutableArray<Conversion>), parameterPosition);
	}

	public static MemberAnalysisResult WrongRefKind()
	{
		return new MemberAnalysisResult(MemberResolutionKind.WrongRefKind);
	}

	public static MemberAnalysisResult WrongReturnType()
	{
		return new MemberAnalysisResult(MemberResolutionKind.WrongReturnType);
	}

	public static MemberAnalysisResult LessDerived()
	{
		return new MemberAnalysisResult(MemberResolutionKind.LessDerived);
	}

	public static MemberAnalysisResult NormalForm(ImmutableArray<int> argsToParamsOpt, ImmutableArray<Conversion> conversions, bool hasAnyRefOmittedArgument)
	{
		return new MemberAnalysisResult(MemberResolutionKind.ApplicableInNormalForm, BitVector.Null, argsToParamsOpt, conversions, -1, hasAnyRefOmittedArgument);
	}

	public static MemberAnalysisResult ExpandedForm(ImmutableArray<int> argsToParamsOpt, ImmutableArray<Conversion> conversions, bool hasAnyRefOmittedArgument, TypeWithAnnotations definitionParamsElementType, TypeWithAnnotations paramsElementType)
	{
		return new MemberAnalysisResult(MemberResolutionKind.ApplicableInExpandedForm, BitVector.Null, argsToParamsOpt, conversions, -1, hasAnyRefOmittedArgument, default(ImmutableArray<TypeParameterDiagnosticInfo>), definitionParamsElementType, paramsElementType);
	}

	public static MemberAnalysisResult Worse()
	{
		return new MemberAnalysisResult(MemberResolutionKind.Worse);
	}

	public static MemberAnalysisResult Worst()
	{
		return new MemberAnalysisResult(MemberResolutionKind.Worst);
	}

	internal static MemberAnalysisResult ConstraintFailure(ImmutableArray<TypeParameterDiagnosticInfo> constraintFailureDiagnostics)
	{
		return new MemberAnalysisResult(MemberResolutionKind.ConstraintFailure, default(BitVector), default(ImmutableArray<int>), default(ImmutableArray<Conversion>), -1, hasAnyRefOmittedArgument: false, constraintFailureDiagnostics);
	}

	internal static MemberAnalysisResult WrongCallingConvention()
	{
		return new MemberAnalysisResult(MemberResolutionKind.WrongCallingConvention);
	}

	[Conditional("DEBUG")]
	public void ArgumentsWereCoerced()
	{
	}

	internal MemberAnalysisResult WithoutReceiverArgument()
	{
		BitVector badArgumentsOpt = shift(BadArgumentsOpt);
		ImmutableArray<int> argsToParamsOpt = adjustArgsToParams(ArgsToParamsOpt);
		ImmutableArray<Conversion> conversionsOpt = adjustConversions(ConversionsOpt);
		return new MemberAnalysisResult(Kind, badArgumentsOpt, argsToParamsOpt, conversionsOpt, BadParameter - 1, HasAnyRefOmittedArgument, ConstraintFailureDiagnostics, DefinitionParamsElementTypeOpt, ParamsElementTypeOpt);
		static ImmutableArray<int> adjustArgsToParams(ImmutableArray<int> argsToParams)
		{
			if (argsToParams.IsDefault)
			{
				return argsToParams;
			}
			ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance();
			instance.AddRange(argsToParams, 1, argsToParams.Length - 1);
			for (int i = 0; i < instance.Count; i++)
			{
				instance[i]--;
			}
			return instance.ToImmutableAndFree();
		}
		static ImmutableArray<Conversion> adjustConversions(ImmutableArray<Conversion> conversions)
		{
			if (!conversions.IsDefaultOrEmpty)
			{
				return conversions.RemoveAt(0);
			}
			return conversions;
		}
		static BitVector shift(BitVector badArguments)
		{
			if (badArguments.IsNull)
			{
				return badArguments;
			}
			BitVector result = BitVector.Create(badArguments.Capacity);
			foreach (int item in badArguments.TrueBits())
			{
				result[item] = true;
			}
			return result;
		}
	}
}
