using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.CodeAnalysis;

internal static class CodeAnalysisResources
{
	private static ResourceManager s_resourceManager;

	internal static ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new ResourceManager(typeof(CodeAnalysisResources)));

	internal static CultureInfo Culture { get; set; }

	internal static string OutputKindNotSupported => GetResourceString("OutputKindNotSupported");

	internal static string AssemblyMustHaveAtLeastOneModule => GetResourceString("AssemblyMustHaveAtLeastOneModule");

	internal static string ModuleCopyCannotBeUsedToCreateAssemblyMetadata => GetResourceString("ModuleCopyCannotBeUsedToCreateAssemblyMetadata");

	internal static string Unresolved => GetResourceString("Unresolved");

	internal static string Assembly => GetResourceString("Assembly");

	internal static string Class1 => GetResourceString("Class1");

	internal static string Attribute => GetResourceString("Attribute");

	internal static string Constructor => GetResourceString("Constructor");

	internal static string Delegate1 => GetResourceString("Delegate1");

	internal static string Enum1 => GetResourceString("Enum1");

	internal static string Event1 => GetResourceString("Event1");

	internal static string Field => GetResourceString("Field");

	internal static string TypeParameter => GetResourceString("TypeParameter");

	internal static string Interface1 => GetResourceString("Interface1");

	internal static string Method => GetResourceString("Method");

	internal static string Module => GetResourceString("Module");

	internal static string Parameter => GetResourceString("Parameter");

	internal static string Property => GetResourceString("Property");

	internal static string Return1 => GetResourceString("Return1");

	internal static string Struct1 => GetResourceString("Struct1");

	internal static string CannotCreateReferenceToSubmission => GetResourceString("CannotCreateReferenceToSubmission");

	internal static string CannotCreateReferenceToModule => GetResourceString("CannotCreateReferenceToModule");

	internal static string InMemoryAssembly => GetResourceString("InMemoryAssembly");

	internal static string InMemoryModule => GetResourceString("InMemoryModule");

	internal static string SizeHasToBePositive => GetResourceString("SizeHasToBePositive");

	internal static string CannotEmbedInteropTypesFromModule => GetResourceString("CannotEmbedInteropTypesFromModule");

	internal static string CannotAliasModule => GetResourceString("CannotAliasModule");

	internal static string InvalidAlias => GetResourceString("InvalidAlias");

	internal static string InvalidAssemblyName => GetResourceString("InvalidAssemblyName");

	internal static string AbsolutePathExpected => GetResourceString("AbsolutePathExpected");

	internal static string EmptyKeyInPathMap => GetResourceString("EmptyKeyInPathMap");

	internal static string NullValueInPathMap => GetResourceString("NullValueInPathMap");

	internal static string ReturnTypeCannotBeValuePointerbyRefOrOpen => GetResourceString("ReturnTypeCannotBeValuePointerbyRefOrOpen");

	internal static string ReturnTypeCannotBeVoidByRefOrOpen => GetResourceString("ReturnTypeCannotBeVoidByRefOrOpen");

	internal static string TypeMustBeSameAsHostObjectTypeOfPreviousSubmission => GetResourceString("TypeMustBeSameAsHostObjectTypeOfPreviousSubmission");

	internal static string PreviousSubmissionHasErrors => GetResourceString("PreviousSubmissionHasErrors");

	internal static string InvalidOutputKindForSubmission => GetResourceString("InvalidOutputKindForSubmission");

	internal static string InvalidCompilationOptions => GetResourceString("InvalidCompilationOptions");

	internal static string ResourceStreamProviderShouldReturnNonNullStream => GetResourceString("ResourceStreamProviderShouldReturnNonNullStream");

	internal static string ReferenceResolverShouldReturnReadableNonNullStream => GetResourceString("ReferenceResolverShouldReturnReadableNonNullStream");

	internal static string EmptyOrInvalidResourceName => GetResourceString("EmptyOrInvalidResourceName");

	internal static string EmptyOrInvalidFileName => GetResourceString("EmptyOrInvalidFileName");

	internal static string ResourceDataProviderShouldReturnNonNullStream => GetResourceString("ResourceDataProviderShouldReturnNonNullStream");

	internal static string FileNotFound => GetResourceString("FileNotFound");

	internal static string InvalidModuleName => GetResourceString("InvalidModuleName");

	internal static string NameCannotBeNull => GetResourceString("NameCannotBeNull");

	internal static string NameCannotBeEmpty => GetResourceString("NameCannotBeEmpty");

	internal static string NameCannotStartWithWhitespace => GetResourceString("NameCannotStartWithWhitespace");

	internal static string NameContainsInvalidCharacter => GetResourceString("NameContainsInvalidCharacter");

	internal static string SpanDoesNotIncludeStartOfLine => GetResourceString("SpanDoesNotIncludeStartOfLine");

	internal static string SpanDoesNotIncludeEndOfLine => GetResourceString("SpanDoesNotIncludeEndOfLine");

	internal static string StartMustNotBeNegative => GetResourceString("StartMustNotBeNegative");

	internal static string EndMustNotBeLessThanStart => GetResourceString("EndMustNotBeLessThanStart");

	internal static string InvalidContentType => GetResourceString("InvalidContentType");

	internal static string InvalidSizeOfPublicKeyToken => GetResourceString("InvalidSizeOfPublicKeyToken");

	internal static string InvalidCharactersInAssemblyName => GetResourceString("InvalidCharactersInAssemblyName");

	internal static string StreamMustSupportReadAndSeek => GetResourceString("StreamMustSupportReadAndSeek");

	internal static string StreamMustSupportRead => GetResourceString("StreamMustSupportRead");

	internal static string StreamMustSupportWrite => GetResourceString("StreamMustSupportWrite");

	internal static string PdbStreamUnexpectedWhenEmbedding => GetResourceString("PdbStreamUnexpectedWhenEmbedding");

	internal static string PdbStreamUnexpectedWhenEmittingMetadataOnly => GetResourceString("PdbStreamUnexpectedWhenEmittingMetadataOnly");

	internal static string MetadataPeStreamUnexpectedWhenEmittingMetadataOnly => GetResourceString("MetadataPeStreamUnexpectedWhenEmittingMetadataOnly");

	internal static string IncludingPrivateMembersUnexpectedWhenEmittingToMetadataPeStream => GetResourceString("IncludingPrivateMembersUnexpectedWhenEmittingToMetadataPeStream");

	internal static string EmbeddingPdbUnexpectedWhenEmittingMetadata => GetResourceString("EmbeddingPdbUnexpectedWhenEmittingMetadata");

	internal static string CannotTargetNetModuleWhenEmittingRefAssembly => GetResourceString("CannotTargetNetModuleWhenEmittingRefAssembly");

	internal static string InvalidHash => GetResourceString("InvalidHash");

	internal static string UnsupportedHashAlgorithm => GetResourceString("UnsupportedHashAlgorithm");

	internal static string InconsistentLanguageVersions => GetResourceString("InconsistentLanguageVersions");

	internal static string CoffResourceInvalidRelocation => GetResourceString("CoffResourceInvalidRelocation");

	internal static string CoffResourceInvalidSectionSize => GetResourceString("CoffResourceInvalidSectionSize");

	internal static string CoffResourceInvalidSymbol => GetResourceString("CoffResourceInvalidSymbol");

	internal static string CoffResourceMissingSection => GetResourceString("CoffResourceMissingSection");

	internal static string IconStreamUnexpectedFormat => GetResourceString("IconStreamUnexpectedFormat");

	internal static string InvalidCultureName => GetResourceString("InvalidCultureName");

	internal static string WinRTIdentityCantBeRetargetable => GetResourceString("WinRTIdentityCantBeRetargetable");

	internal static string AssemblySigningNotSupported => GetResourceString("AssemblySigningNotSupported");

	internal static string XmlReferencesNotSupported => GetResourceString("XmlReferencesNotSupported");

	internal static string InvalidRuleSetInclude => GetResourceString("InvalidRuleSetInclude");

	internal static string CompilerAnalyzerFailure => GetResourceString("CompilerAnalyzerFailure");

	internal static string CompilerAnalyzerThrows => GetResourceString("CompilerAnalyzerThrows");

	internal static string AnalyzerDriverFailure => GetResourceString("AnalyzerDriverFailure");

	internal static string AnalyzerDriverThrows => GetResourceString("AnalyzerDriverThrows");

	internal static string PEImageDoesntContainManagedMetadata => GetResourceString("PEImageDoesntContainManagedMetadata");

	internal static string ChangesMustNotOverlap => GetResourceString("ChangesMustNotOverlap");

	internal static string DiagnosticIdCantBeNullOrWhitespace => GetResourceString("DiagnosticIdCantBeNullOrWhitespace");

	internal static string SuppressionIdCantBeNullOrWhitespace => GetResourceString("SuppressionIdCantBeNullOrWhitespace");

	internal static string RuleSetHasDuplicateRules => GetResourceString("RuleSetHasDuplicateRules");

	internal static string CantCreateModuleReferenceToAssembly => GetResourceString("CantCreateModuleReferenceToAssembly");

	internal static string CantCreateReferenceToDynamicAssembly => GetResourceString("CantCreateReferenceToDynamicAssembly");

	internal static string CantCreateReferenceToAssemblyWithoutLocation => GetResourceString("CantCreateReferenceToAssemblyWithoutLocation");

	internal static string ArgumentCannotBeEmpty => GetResourceString("ArgumentCannotBeEmpty");

	internal static string ArgumentElementCannotBeNull => GetResourceString("ArgumentElementCannotBeNull");

	internal static string UnsupportedDiagnosticReported => GetResourceString("UnsupportedDiagnosticReported");

	internal static string UnsupportedSuppressionReported => GetResourceString("UnsupportedSuppressionReported");

	internal static string InvalidDiagnosticSuppressionReported => GetResourceString("InvalidDiagnosticSuppressionReported");

	internal static string NonReportedDiagnosticCannotBeSuppressed => GetResourceString("NonReportedDiagnosticCannotBeSuppressed");

	internal static string InvalidDiagnosticIdReported => GetResourceString("InvalidDiagnosticIdReported");

	internal static string InvalidDiagnosticLocationReported => GetResourceString("InvalidDiagnosticLocationReported");

	internal static string SupportedDiagnosticsHasNullDescriptor => GetResourceString("SupportedDiagnosticsHasNullDescriptor");

	internal static string SupportedSuppressionsHasNullDescriptor => GetResourceString("SupportedSuppressionsHasNullDescriptor");

	internal static string InvalidNodeToTrack => GetResourceString("InvalidNodeToTrack");

	internal static string NodeOrTokenOutOfSequence => GetResourceString("NodeOrTokenOutOfSequence");

	internal static string UnexpectedTypeOfNodeInList => GetResourceString("UnexpectedTypeOfNodeInList");

	internal static string MissingListItem => GetResourceString("MissingListItem");

	internal static string MissingTokenListItem => GetResourceString("MissingTokenListItem");

	internal static string SeparatorTokenMustHaveSameRawKind => GetResourceString("SeparatorTokenMustHaveSameRawKind");

	internal static string SeparatorTokenMustHaveSameLanguage => GetResourceString("SeparatorTokenMustHaveSameLanguage");

	internal static string InvalidPublicKey => GetResourceString("InvalidPublicKey");

	internal static string InvalidPublicKeyToken => GetResourceString("InvalidPublicKeyToken");

	internal static string InvalidDataAtOffset => GetResourceString("InvalidDataAtOffset");

	internal static string SymWriterNotDeterministic => GetResourceString("SymWriterNotDeterministic");

	internal static string SymWriterOlderVersionThanRequired => GetResourceString("SymWriterOlderVersionThanRequired");

	internal static string SymWriterDoesNotSupportSourceLink => GetResourceString("SymWriterDoesNotSupportSourceLink");

	internal static string SymWriterMetadataOverLimit => GetResourceString("SymWriterMetadataOverLimit");

	internal static string RuleSetBadAttributeValue => GetResourceString("RuleSetBadAttributeValue");

	internal static string RuleSetMissingAttribute => GetResourceString("RuleSetMissingAttribute");

	internal static string KeepAliveIsNotAnInteger => GetResourceString("KeepAliveIsNotAnInteger");

	internal static string KeepAliveIsTooSmall => GetResourceString("KeepAliveIsTooSmall");

	internal static string KeepAliveWithoutShared => GetResourceString("KeepAliveWithoutShared");

	internal static string MismatchedVersion => GetResourceString("MismatchedVersion");

	internal static string MissingKeepAlive => GetResourceString("MissingKeepAlive");

	internal static string AnalyzerTotalExecutionTime => GetResourceString("AnalyzerTotalExecutionTime");

	internal static string MultithreadedAnalyzerExecutionNote => GetResourceString("MultithreadedAnalyzerExecutionNote");

	internal static string AnalyzerExecutionTimeColumnHeader => GetResourceString("AnalyzerExecutionTimeColumnHeader");

	internal static string AnalyzerNameColumnHeader => GetResourceString("AnalyzerNameColumnHeader");

	internal static string NoAnalyzersFound => GetResourceString("NoAnalyzersFound");

	internal static string DuplicateAnalyzerInstances => GetResourceString("DuplicateAnalyzerInstances");

	internal static string UnsupportedAnalyzerInstance => GetResourceString("UnsupportedAnalyzerInstance");

	internal static string InvalidTree => GetResourceString("InvalidTree");

	internal static string InvalidAdditionalFile => GetResourceString("InvalidAdditionalFile");

	internal static string ResourceStreamEndedUnexpectedly => GetResourceString("ResourceStreamEndedUnexpectedly");

	internal static string SharedArgumentMissing => GetResourceString("SharedArgumentMissing");

	internal static string ExceptionContext => GetResourceString("ExceptionContext");

	internal static string AnonymousTypeMemberAndNamesCountMismatch2 => GetResourceString("AnonymousTypeMemberAndNamesCountMismatch2");

	internal static string AnonymousTypeArgumentCountMismatch2 => GetResourceString("AnonymousTypeArgumentCountMismatch2");

	internal static string InconsistentSyntaxTreeFeature => GetResourceString("InconsistentSyntaxTreeFeature");

	internal static string ReferenceOfTypeIsInvalid1 => GetResourceString("ReferenceOfTypeIsInvalid1");

	internal static string MetadataRefNotFoundToRemove1 => GetResourceString("MetadataRefNotFoundToRemove1");

	internal static string TupleElementNameCountMismatch => GetResourceString("TupleElementNameCountMismatch");

	internal static string TupleElementNameEmpty => GetResourceString("TupleElementNameEmpty");

	internal static string TupleElementLocationCountMismatch => GetResourceString("TupleElementLocationCountMismatch");

	internal static string TupleElementNullableAnnotationCountMismatch => GetResourceString("TupleElementNullableAnnotationCountMismatch");

	internal static string TuplesNeedAtLeastTwoElements => GetResourceString("TuplesNeedAtLeastTwoElements");

	internal static string CompilationReferencesAssembliesWithDifferentAutoGeneratedVersion => GetResourceString("CompilationReferencesAssembliesWithDifferentAutoGeneratedVersion");

	internal static string TupleUnderlyingTypeMustBeTupleCompatible => GetResourceString("TupleUnderlyingTypeMustBeTupleCompatible");

	internal static string UnrecognizedResourceFileFormat => GetResourceString("UnrecognizedResourceFileFormat");

	internal static string SourceTextCannotBeEmbedded => GetResourceString("SourceTextCannotBeEmbedded");

	internal static string StreamIsTooLong => GetResourceString("StreamIsTooLong");

	internal static string EmbeddedTextsRequirePdb => GetResourceString("EmbeddedTextsRequirePdb");

	internal static string ElementIsExpected => GetResourceString("ElementIsExpected");

	internal static string SeparatorIsExpected => GetResourceString("SeparatorIsExpected");

	internal static string InvalidDiagnosticSpanReported => GetResourceString("InvalidDiagnosticSpanReported");

	internal static string NotARootOperation => GetResourceString("NotARootOperation");

	internal static string OperationHasNullSemanticModel => GetResourceString("OperationHasNullSemanticModel");

	internal static string InvalidOperationBlockForAnalysisContext => GetResourceString("InvalidOperationBlockForAnalysisContext");

	internal static string IsSymbolAccessibleBadWithin => GetResourceString("IsSymbolAccessibleBadWithin");

	internal static string IsSymbolAccessibleWrongAssembly => GetResourceString("IsSymbolAccessibleWrongAssembly");

	internal static string OperationMustNotBeControlFlowGraphPart => GetResourceString("OperationMustNotBeControlFlowGraphPart");

	internal static string WRN_InvalidSeverityInAnalyzerConfig => GetResourceString("WRN_InvalidSeverityInAnalyzerConfig");

	internal static string WRN_InvalidSeverityInAnalyzerConfig_Title => GetResourceString("WRN_InvalidSeverityInAnalyzerConfig_Title");

	internal static string SuppressionDiagnosticDescriptorTitle => GetResourceString("SuppressionDiagnosticDescriptorTitle");

	internal static string SuppressionDiagnosticDescriptorMessage => GetResourceString("SuppressionDiagnosticDescriptorMessage");

	internal static string ModuleHasInvalidAttributes => GetResourceString("ModuleHasInvalidAttributes");

	internal static string UnableToDetermineSpecificCauseOfFailure => GetResourceString("UnableToDetermineSpecificCauseOfFailure");

	internal static string ChangingVersionOfAssemblyReferenceIsNotAllowedDuringDebugging => GetResourceString("ChangingVersionOfAssemblyReferenceIsNotAllowedDuringDebugging");

	internal static string DisableAnalyzerDiagnosticsMessage => GetResourceString("DisableAnalyzerDiagnosticsMessage");

	internal static string Single_type_per_generator_0 => GetResourceString("Single_type_per_generator_0");

	internal static string WRN_MultipleGlobalAnalyzerKeys => GetResourceString("WRN_MultipleGlobalAnalyzerKeys");

	internal static string WRN_MultipleGlobalAnalyzerKeys_Title => GetResourceString("WRN_MultipleGlobalAnalyzerKeys_Title");

	internal static string HintNameUniquePerGenerator => GetResourceString("HintNameUniquePerGenerator");

	internal static string HintNameInvalidChar => GetResourceString("HintNameInvalidChar");

	internal static string SourceTextRequiresEncoding => GetResourceString("SourceTextRequiresEncoding");

	internal static string AssemblyReferencesNetFramework => GetResourceString("AssemblyReferencesNetFramework");

	internal static string WRN_InvalidGlobalSectionName => GetResourceString("WRN_InvalidGlobalSectionName");

	internal static string WRN_InvalidGlobalSectionName_Title => GetResourceString("WRN_InvalidGlobalSectionName_Title");

	internal static string ChangesMustBeWithinBoundsOfSourceText => GetResourceString("ChangesMustBeWithinBoundsOfSourceText");

	internal static string EncCannotResumeSuspendedAsyncMethod => GetResourceString("EncCannotResumeSuspendedAsyncMethod");

	internal static string EncCannotResumeSuspendedIteratorMethod => GetResourceString("EncCannotResumeSuspendedIteratorMethod");

	internal static string EncDeletedMethodInvoked => GetResourceString("EncDeletedMethodInvoked");

	internal static string EncDeletedLambdaInvoked => GetResourceString("EncDeletedLambdaInvoked");

	internal static string EncLambdaRudeEdit => GetResourceString("EncLambdaRudeEdit");

	internal static string EncLambdaRudeEdit_CapturedVariables => GetResourceString("EncLambdaRudeEdit_CapturedVariables");

	internal static string GeneratorNameColumnHeader => GetResourceString("GeneratorNameColumnHeader");

	internal static string GeneratorTotalExecutionTime => GetResourceString("GeneratorTotalExecutionTime");

	internal static string BadBuiltInOps1 => GetResourceString("BadBuiltInOps1");

	internal static string BadBuiltInOps2 => GetResourceString("BadBuiltInOps2");

	internal static string BadBuiltInOps3 => GetResourceString("BadBuiltInOps3");

	internal static string HintNameInvalidSegment => GetResourceString("HintNameInvalidSegment");

	internal static string MethodSymbolExpected => GetResourceString("MethodSymbolExpected");

	internal static string InvalidInstrumentationKind => GetResourceString("InvalidInstrumentationKind");

	internal static string LineCannotBeGreaterThanEnd => GetResourceString("LineCannotBeGreaterThanEnd");

	internal static string InternalsVisibleToHeaderSummary => GetResourceString("InternalsVisibleToHeaderSummary");

	internal static string InternalsVisibleToCurrentAssembly => GetResourceString("InternalsVisibleToCurrentAssembly");

	internal static string InternalsVisibleToReferencedAssembly => GetResourceString("InternalsVisibleToReferencedAssembly");

	internal static string InternalsVisibleToReferencedAssemblyDetails => GetResourceString("InternalsVisibleToReferencedAssemblyDetails");

	internal static string Nothing => GetResourceString("Nothing");

	internal static string SigningTempPathUnavailable => GetResourceString("SigningTempPathUnavailable");

	internal static string Type0DoesNotHaveExpectedConstructor => GetResourceString("Type0DoesNotHaveExpectedConstructor");

	internal static string ExceptionMessage_FileMayBeLockedBy => GetResourceString("ExceptionMessage_FileMayBeLockedBy");

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static string GetResourceString(string resourceKey, string defaultValue = null)
	{
		return ResourceManager.GetString(resourceKey, Culture);
	}
}
