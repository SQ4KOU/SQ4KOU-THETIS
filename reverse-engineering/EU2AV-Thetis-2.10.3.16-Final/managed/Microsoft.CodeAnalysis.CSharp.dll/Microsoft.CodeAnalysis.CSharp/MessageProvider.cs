using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class MessageProvider : CommonMessageProvider
{
	public static readonly MessageProvider Instance = new MessageProvider();

	public override string CodePrefix => "CS";

	public override Type ErrorCodeType => typeof(ErrorCode);

	public override int ERR_FailedToCreateTempFile => 1619;

	public override int ERR_MultipleAnalyzerConfigsInSameDir => 8700;

	public override int ERR_ExpectedSingleScript => 7018;

	public override int ERR_OpenResponseFile => 2011;

	public override int ERR_InvalidPathMap => 8101;

	public override int FTL_InvalidInputFileName => 2021;

	public override int ERR_FileNotFound => 2001;

	public override int ERR_NoSourceFile => 1504;

	public override int ERR_CantOpenFileWrite => 2012;

	public override int ERR_OutputWriteFailed => 16;

	public override int WRN_NoConfigNotOnCommandLine => 2023;

	public override int ERR_BinaryFile => 2015;

	public override int WRN_AnalyzerCannotBeCreated => 8032;

	public override int WRN_NoAnalyzerInAssembly => 8033;

	public override int WRN_UnableToLoadAnalyzer => 8034;

	public override int WRN_AnalyzerReferencesFramework => 8850;

	public override int WRN_AnalyzerReferencesNewerCompiler => 9057;

	public override int WRN_DuplicateAnalyzerReference => 9067;

	public override int INF_UnableToLoadSomeTypesInAnalyzer => 8040;

	public override int ERR_CantReadRulesetFile => 8035;

	public override int ERR_CompileCancelled => 1600;

	public override int ERR_BadSourceCodeKind => 8190;

	public override int ERR_BadDocumentationMode => 8191;

	public override int ERR_BadCompilationOptionValue => 7088;

	public override int ERR_MutuallyExclusiveOptions => 7102;

	public override int ERR_InvalidDebugInformationFormat => 2042;

	public override int ERR_InvalidOutputName => 2041;

	public override int ERR_InvalidFileAlignment => 2024;

	public override int ERR_InvalidSubsystemVersion => 1773;

	public override int ERR_InvalidInstrumentationKind => 8111;

	public override int ERR_InvalidHashAlgorithmName => 8113;

	public override int ERR_MetadataFileNotAssembly => 1509;

	public override int ERR_MetadataFileNotModule => 1542;

	public override int ERR_InvalidAssemblyMetadata => 9;

	public override int ERR_InvalidModuleMetadata => 9;

	public override int ERR_ErrorOpeningAssemblyFile => 9;

	public override int ERR_ErrorOpeningModuleFile => 9;

	public override int ERR_MetadataFileNotFound => 6;

	public override int ERR_MetadataReferencesNotSupported => 7099;

	public override int ERR_LinkedNetmoduleMetadataMustProvideFullPEImage => 7098;

	public override int ERR_PublicKeyFileFailure => 7027;

	public override int ERR_PublicKeyContainerFailure => 7028;

	public override int ERR_OptionMustBeAbsolutePath => 8106;

	public override int ERR_CantReadResource => 1566;

	public override int ERR_CantOpenWin32Resource => 1719;

	public override int ERR_CantOpenWin32Manifest => 1926;

	public override int ERR_CantOpenWin32Icon => 7064;

	public override int ERR_ErrorBuildingWin32Resource => 7065;

	public override int ERR_BadWin32Resource => 1583;

	public override int ERR_ResourceFileNameNotUnique => 7041;

	public override int ERR_ResourceNotUnique => 1508;

	public override int ERR_ResourceInModule => 1507;

	public override int ERR_PermissionSetAttributeFileReadError => 7057;

	public override int ERR_MethodImplAttributeAsyncCannotBeUsed => 9330;

	public override int ERR_EncodinglessSyntaxTree => 8055;

	public override int WRN_PdbUsingNameTooLong => 811;

	public override int WRN_PdbLocalNameTooLong => 8029;

	public override int ERR_PdbWritingFailed => 41;

	public override int ERR_MetadataNameTooLong => 7013;

	public override int ERR_EncReferenceToAddedMember => 7101;

	public override int ERR_TooManyUserStrings => 8103;

	public override int ERR_TooManyUserStrings_RestartRequired => 9315;

	public override int ERR_PeWritingFailure => 8104;

	public override int ERR_ModuleEmitFailure => 7038;

	public override int ERR_EncUpdateFailedMissingSymbol => 7043;

	public override int ERR_InvalidDebugInfo => 7103;

	public override int ERR_FunctionPointerTypesInAttributeNotSupported => 8911;

	public override int ERR_DataSectionStringLiteralHashCollision => 9274;

	public override int ERR_EncUpdateRequiresEmittingExplicitInterfaceImplementationNotSupportedByTheRuntime => 9346;

	public override int WRN_GeneratorFailedDuringInitialization => 8784;

	public override int WRN_GeneratorFailedDuringGeneration => 8785;

	public override int ERR_BadAssemblyName => 8203;

	public override int? WRN_ByValArraySizeConstRequired => 9125;

	private MessageProvider()
	{
	}

	public override DiagnosticSeverity GetSeverity(int code)
	{
		return ErrorFacts.GetSeverity((ErrorCode)code);
	}

	public override string LoadMessage(int code, CultureInfo language)
	{
		return ErrorFacts.GetMessage((ErrorCode)code, language);
	}

	public override LocalizableString GetMessageFormat(int code)
	{
		return ErrorFacts.GetMessageFormat((ErrorCode)code);
	}

	public override LocalizableString GetDescription(int code)
	{
		return ErrorFacts.GetDescription((ErrorCode)code);
	}

	public override LocalizableString GetTitle(int code)
	{
		return ErrorFacts.GetTitle((ErrorCode)code);
	}

	public override string GetHelpLink(int code)
	{
		return ErrorFacts.GetHelpLink((ErrorCode)code);
	}

	public override string GetCategory(int code)
	{
		return ErrorFacts.GetCategory((ErrorCode)code);
	}

	public override string GetMessagePrefix(string id, DiagnosticSeverity severity, bool isWarningAsError, CultureInfo culture)
	{
		return string.Format(culture, "{0} {1}", ((severity == DiagnosticSeverity.Error) | isWarningAsError) ? "error" : "warning", id);
	}

	public override int GetWarningLevel(int code)
	{
		return ErrorFacts.GetWarningLevel((ErrorCode)code);
	}

	public override Diagnostic CreateDiagnostic(int code, Location location, params object[] args)
	{
		return new CSDiagnostic(new CSDiagnosticInfo((ErrorCode)code, args, ImmutableArray<Symbol>.Empty, ImmutableArray<Location>.Empty), location);
	}

	public override Diagnostic CreateDiagnostic(DiagnosticInfo info)
	{
		return new CSDiagnostic(info, Location.None);
	}

	public override string GetErrorDisplayString(ISymbol symbol)
	{
		if (symbol.Kind == SymbolKind.Assembly || symbol.Kind == SymbolKind.Namespace)
		{
			return symbol.ToString();
		}
		return SymbolDisplay.ToDisplayString(symbol, SymbolDisplayFormat.CSharpShortErrorMessageFormat);
	}

	public override bool GetIsEnabledByDefault(int code)
	{
		bool flag = (((uint)(code - 9018) <= 4u || (uint)(code - 9208) <= 1u) ? true : false);
		return !flag;
	}

	public override ReportDiagnostic GetDiagnosticReport(DiagnosticInfo diagnosticInfo, CompilationOptions options)
	{
		bool hasPragmaSuppression;
		return CSharpDiagnosticFilter.GetDiagnosticReport(diagnosticInfo.Severity, isEnabledByDefault: true, diagnosticInfo.Code, diagnosticInfo.MessageIdentifier, diagnosticInfo.WarningLevel, Location.None, diagnosticInfo.CustomTags, options.WarningLevel, ((CSharpCompilationOptions)options).NullableContextOptions, options.GeneralDiagnosticOption, options.SpecificDiagnosticOptions, options.SyntaxTreeOptionsProvider, CancellationToken.None, out hasPragmaSuppression);
	}

	public override void ReportDuplicateMetadataReferenceStrong(DiagnosticBag diagnostics, Location location, MetadataReference reference, AssemblyIdentity identity, MetadataReference equivalentReference, AssemblyIdentity equivalentIdentity)
	{
		diagnostics.Add(ErrorCode.ERR_DuplicateImport, location, reference.Display ?? identity.GetDisplayName(), equivalentReference.Display ?? equivalentIdentity.GetDisplayName());
	}

	public override void ReportDuplicateMetadataReferenceWeak(DiagnosticBag diagnostics, Location location, MetadataReference reference, AssemblyIdentity identity, MetadataReference equivalentReference, AssemblyIdentity equivalentIdentity)
	{
		diagnostics.Add(ErrorCode.ERR_DuplicateImportSimple, location, identity.Name, reference.Display ?? identity.GetDisplayName());
	}

	protected override void ReportInvalidAttributeArgument(DiagnosticBag diagnostics, SyntaxNode attributeSyntax, int parameterIndex, AttributeData attribute)
	{
		AttributeSyntax attributeSyntax2 = (AttributeSyntax)attributeSyntax;
		diagnostics.Add(ErrorCode.ERR_InvalidAttributeArgument, ((CSharpAttributeData)attribute).GetAttributeArgumentLocation(parameterIndex), attributeSyntax2.GetErrorDisplayName());
	}

	protected override void ReportInvalidNamedArgument(DiagnosticBag diagnostics, SyntaxNode attributeSyntax, int namedArgumentIndex, ITypeSymbol attributeClass, string parameterName)
	{
		AttributeSyntax attributeSyntax2 = (AttributeSyntax)attributeSyntax;
		diagnostics.Add(ErrorCode.ERR_InvalidNamedArgument, attributeSyntax2.ArgumentList.Arguments[namedArgumentIndex].Location, parameterName);
	}

	protected override void ReportParameterNotValidForType(DiagnosticBag diagnostics, SyntaxNode attributeSyntax, int namedArgumentIndex)
	{
		AttributeSyntax attributeSyntax2 = (AttributeSyntax)attributeSyntax;
		diagnostics.Add(ErrorCode.ERR_ParameterNotValidForType, attributeSyntax2.ArgumentList.Arguments[namedArgumentIndex].Location);
	}

	protected override void ReportMarshalUnmanagedTypeNotValidForFields(DiagnosticBag diagnostics, SyntaxNode attributeSyntax, int parameterIndex, string unmanagedTypeName, AttributeData attribute)
	{
		diagnostics.Add(ErrorCode.ERR_MarshalUnmanagedTypeNotValidForFields, ((CSharpAttributeData)attribute).GetAttributeArgumentLocation(parameterIndex), unmanagedTypeName);
	}

	protected override void ReportMarshalUnmanagedTypeOnlyValidForFields(DiagnosticBag diagnostics, SyntaxNode attributeSyntax, int parameterIndex, string unmanagedTypeName, AttributeData attribute)
	{
		diagnostics.Add(ErrorCode.ERR_MarshalUnmanagedTypeOnlyValidForFields, ((CSharpAttributeData)attribute).GetAttributeArgumentLocation(parameterIndex), unmanagedTypeName);
	}

	protected override void ReportAttributeParameterRequired(DiagnosticBag diagnostics, SyntaxNode attributeSyntax, string parameterName)
	{
		AttributeSyntax attributeSyntax2 = (AttributeSyntax)attributeSyntax;
		diagnostics.Add(ErrorCode.ERR_AttributeParameterRequired1, attributeSyntax2.Name.Location, parameterName);
	}

	protected override void ReportAttributeParameterRequired(DiagnosticBag diagnostics, SyntaxNode attributeSyntax, string parameterName1, string parameterName2)
	{
		AttributeSyntax attributeSyntax2 = (AttributeSyntax)attributeSyntax;
		diagnostics.Add(ErrorCode.ERR_AttributeParameterRequired2, attributeSyntax2.Name.Location, parameterName1, parameterName2);
	}
}
