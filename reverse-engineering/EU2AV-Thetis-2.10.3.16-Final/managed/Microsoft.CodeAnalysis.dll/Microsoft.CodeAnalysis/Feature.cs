using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Microsoft.CodeAnalysis;

internal static class Feature
{
	internal const string Strict = "strict";

	internal const string UseLegacyStrongNameProvider = "UseLegacyStrongNameProvider";

	internal const string EnableGeneratorCache = "enable-generator-cache";

	internal const string PdbPathDeterminism = "pdb-path-determinism";

	internal const string DebugDeterminism = "debug-determinism";

	internal const string DebugAnalyzers = "debug-analyzers";

	internal const string RuntimeAsync = "runtime-async";

	internal const string PEVerifyCompat = "peverify-compat";

	internal const string FileBasedProgram = "FileBasedProgram";

	internal const string NullablePublicOnly = "nullablePublicOnly";

	internal const string RunNullableAnalysis = "run-nullable-analysis";

	internal const string InterceptorsNamespaces = "InterceptorsNamespaces";

	internal const string NoRefSafetyRulesAttribute = "noRefSafetyRulesAttribute";

	internal const string DisableLengthBasedSwitch = "disable-length-based-switch";

	internal const string ExperimentalDataSectionStringLiterals = "experimental-data-section-string-literals";

	internal const string Experiment = "Experiment";

	internal const string Test = "Test";

	[Conditional("DEBUG")]
	internal static void AssertValidFeature(string s)
	{
		_ = from f in typeof(Feature).GetFields(BindingFlags.Static | BindingFlags.NonPublic)
			where f.FieldType == typeof(string) && f.IsLiteral
			select (string)f.GetRawConstantValue();
	}
}
