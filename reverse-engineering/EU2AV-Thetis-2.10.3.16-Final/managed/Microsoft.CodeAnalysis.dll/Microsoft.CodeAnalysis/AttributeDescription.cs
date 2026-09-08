using System.Collections.Immutable;
using System.Reflection.Metadata;

namespace Microsoft.CodeAnalysis;

internal struct AttributeDescription(string @namespace, string name, byte[][] signatures, bool matchIgnoringCase = false)
{
	internal enum TypeHandleTarget : byte
	{
		AttributeTargets,
		AssemblyNameFlags,
		MethodImplOptions,
		CharSet,
		LayoutKind,
		UnmanagedType,
		TypeLibTypeFlags,
		ClassInterfaceType,
		ComInterfaceType,
		CompilationRelaxations,
		DebuggingModes,
		SecurityCriticalScope,
		CallingConvention,
		AssemblyHashAlgorithm,
		TransactionOption,
		SecurityAction,
		SystemType,
		DeprecationType,
		Platform
	}

	internal readonly struct TypeHandleTargetInfo(string @namespace, string name, SerializationTypeCode underlying)
	{
		public readonly string Namespace = @namespace;

		public readonly string Name = name;

		public readonly SerializationTypeCode Underlying = underlying;
	}

	public readonly string Namespace = @namespace;

	public readonly string Name = name;

	public readonly byte[][] Signatures = signatures;

	public readonly bool MatchIgnoringCase = matchIgnoringCase;

	private const byte Void = 1;

	private const byte Boolean = 2;

	private const byte Byte = 5;

	private const byte Int16 = 6;

	private const byte Int32 = 8;

	private const byte UInt32 = 9;

	private const byte Int64 = 10;

	private const byte String = 14;

	private const byte Object = 28;

	private const byte SzArray = 29;

	private const byte TypeHandle = 64;

	internal static ImmutableArray<TypeHandleTargetInfo> TypeHandleTargets;

	private static readonly byte[] s_signature_HasThis_Void;

	private static readonly byte[] s_signature_HasThis_Void_Byte;

	private static readonly byte[] s_signature_HasThis_Void_Int16;

	private static readonly byte[] s_signature_HasThis_Void_Int32;

	private static readonly byte[] s_signature_HasThis_Void_UInt32;

	private static readonly byte[] s_signature_HasThis_Void_Int32_Int32;

	private static readonly byte[] s_signature_HasThis_Void_Int32_Int32_Int32_Int32;

	private static readonly byte[] s_signature_HasThis_Void_String;

	private static readonly byte[] s_signature_HasThis_Void_Object;

	private static readonly byte[] s_signature_HasThis_Void_String_String;

	private static readonly byte[] s_signature_HasThis_Void_String_Boolean;

	private static readonly byte[] s_signature_HasThis_Void_String_String_String;

	private static readonly byte[] s_signature_HasThis_Void_String_String_String_String;

	private static readonly byte[] s_signature_HasThis_Void_AttributeTargets;

	private static readonly byte[] s_signature_HasThis_Void_AssemblyNameFlags;

	private static readonly byte[] s_signature_HasThis_Void_MethodImplOptions;

	private static readonly byte[] s_signature_HasThis_Void_CharSet;

	private static readonly byte[] s_signature_HasThis_Void_LayoutKind;

	private static readonly byte[] s_signature_HasThis_Void_UnmanagedType;

	private static readonly byte[] s_signature_HasThis_Void_TypeLibTypeFlags;

	private static readonly byte[] s_signature_HasThis_Void_ClassInterfaceType;

	private static readonly byte[] s_signature_HasThis_Void_ComInterfaceType;

	private static readonly byte[] s_signature_HasThis_Void_CompilationRelaxations;

	private static readonly byte[] s_signature_HasThis_Void_DebuggingModes;

	private static readonly byte[] s_signature_HasThis_Void_SecurityCriticalScope;

	private static readonly byte[] s_signature_HasThis_Void_CallingConvention;

	private static readonly byte[] s_signature_HasThis_Void_AssemblyHashAlgorithm;

	private static readonly byte[] s_signature_HasThis_Void_Int64;

	private static readonly byte[] s_signature_HasThis_Void_UInt8_UInt8_UInt32_UInt32_UInt32;

	private static readonly byte[] s_signature_HasThis_Void_UIn8_UInt8_Int32_Int32_Int32;

	private static readonly byte[] s_signature_HasThis_Void_Boolean;

	private static readonly byte[] s_signature_HasThis_Void_Boolean_Boolean;

	private static readonly byte[] s_signature_HasThis_Void_Boolean_TransactionOption;

	private static readonly byte[] s_signature_HasThis_Void_Boolean_TransactionOption_Int32;

	private static readonly byte[] s_signature_HasThis_Void_Boolean_TransactionOption_Int32_Boolean;

	private static readonly byte[] s_signature_HasThis_Void_SecurityAction;

	private static readonly byte[] s_signature_HasThis_Void_Type;

	private static readonly byte[] s_signature_HasThis_Void_Type_Type;

	private static readonly byte[] s_signature_HasThis_Void_Type_Type_Type;

	private static readonly byte[] s_signature_HasThis_Void_Type_Type_Type_Type;

	private static readonly byte[] s_signature_HasThis_Void_Type_Int32;

	private static readonly byte[] s_signature_HasThis_Void_Type_String;

	private static readonly byte[] s_signature_HasThis_Void_String_Int32_Int32;

	private static readonly byte[] s_signature_HasThis_Void_Int32_String;

	private static readonly byte[] s_signature_HasThis_Void_SzArray_Boolean;

	private static readonly byte[] s_signature_HasThis_Void_SzArray_Byte;

	private static readonly byte[] s_signature_HasThis_Void_SzArray_String;

	private static readonly byte[] s_signature_HasThis_Void_Boolean_SzArray_String;

	private static readonly byte[] s_signature_HasThis_Void_Boolean_String;

	private static readonly byte[] s_signature_HasThis_Void_String_DeprecationType_UInt32;

	private static readonly byte[] s_signature_HasThis_Void_String_DeprecationType_UInt32_Platform;

	private static readonly byte[] s_signature_HasThis_Void_String_DeprecationType_UInt32_Type;

	private static readonly byte[] s_signature_HasThis_Void_String_DeprecationType_UInt32_String;

	private static readonly byte[][] s_signatures_HasThis_Void_Only;

	private static readonly byte[][] s_signatures_HasThis_Void_String_Only;

	private static readonly byte[][] s_signatures_HasThis_Void_Type_Only;

	private static readonly byte[][] s_signatures_HasThis_Void_Boolean_Only;

	private static readonly byte[][] s_signatures_HasThis_Void_Int32_Only;

	private static readonly byte[][] s_signaturesOfTypeIdentifierAttribute;

	private static readonly byte[][] s_signaturesOfAttributeUsage;

	private static readonly byte[][] s_signaturesOfAssemblySignatureKeyAttribute;

	private static readonly byte[][] s_signaturesOfAssemblyFlagsAttribute;

	private static readonly byte[][] s_signaturesOfDefaultParameterValueAttribute;

	private static readonly byte[][] s_signaturesOfDateTimeConstantAttribute;

	private static readonly byte[][] s_signaturesOfDecimalConstantAttribute;

	private static readonly byte[][] s_signaturesOfSecurityPermissionAttribute;

	private static readonly byte[][] s_signaturesOfMethodImplAttribute;

	private static readonly byte[][] s_signaturesOfDefaultCharSetAttribute;

	private static readonly byte[][] s_signaturesOfMemberNotNullAttribute;

	private static readonly byte[][] s_signaturesOfMemberNotNullWhenAttribute;

	private static readonly byte[][] s_signaturesOfFixedBufferAttribute;

	private static readonly byte[][] s_signaturesOfInterceptsLocationAttribute;

	private static readonly byte[][] s_signaturesOfPrincipalPermissionAttribute;

	private static readonly byte[][] s_signaturesOfPermissionSetAttribute;

	private static readonly byte[][] s_signaturesOfStructLayoutAttribute;

	private static readonly byte[][] s_signaturesOfMarshalAsAttribute;

	private static readonly byte[][] s_signaturesOfTypeLibTypeAttribute;

	private static readonly byte[][] s_signaturesOfWebMethodAttribute;

	private static readonly byte[][] s_signaturesOfHostProtectionAttribute;

	private static readonly byte[][] s_signaturesOfVisualBasicComClassAttribute;

	private static readonly byte[][] s_signaturesOfClassInterfaceAttribute;

	private static readonly byte[][] s_signaturesOfInterfaceTypeAttribute;

	private static readonly byte[][] s_signaturesOfCompilationRelaxationsAttribute;

	private static readonly byte[][] s_signaturesOfDebuggableAttribute;

	private static readonly byte[][] s_signaturesOfComSourceInterfacesAttribute;

	private static readonly byte[][] s_signaturesOfTypeLibVersionAttribute;

	private static readonly byte[][] s_signaturesOfComCompatibleVersionAttribute;

	private static readonly byte[][] s_signaturesOfObsoleteAttribute;

	private static readonly byte[][] s_signaturesOfDynamicAttribute;

	private static readonly byte[][] s_signaturesOfTupleElementNamesAttribute;

	private static readonly byte[][] s_signaturesOfSecurityCriticalAttribute;

	private static readonly byte[][] s_signaturesOfMyGroupCollectionAttribute;

	private static readonly byte[][] s_signaturesOfComEventInterfaceAttribute;

	private static readonly byte[][] s_signaturesOfUnmanagedFunctionPointerAttribute;

	private static readonly byte[][] s_signaturesOfPrimaryInteropAssemblyAttribute;

	private static readonly byte[][] s_signaturesOfAssemblyAlgorithmIdAttribute;

	private static readonly byte[][] s_signaturesOfDeprecatedAttribute;

	private static readonly byte[][] s_signaturesOfNullableAttribute;

	private static readonly byte[][] s_signaturesOfNullableContextAttribute;

	private static readonly byte[][] s_signaturesOfNativeIntegerAttribute;

	private static readonly byte[][] s_signaturesOfInterpolatedStringArgumentAttribute;

	private static readonly byte[][] s_signaturesOfCollectionBuilderAttribute;

	internal static readonly AttributeDescription OptionalAttribute;

	internal static readonly AttributeDescription ComImportAttribute;

	internal static readonly AttributeDescription AttributeUsageAttribute;

	internal static readonly AttributeDescription ConditionalAttribute;

	internal static readonly AttributeDescription CaseInsensitiveExtensionAttribute;

	internal static readonly AttributeDescription CaseSensitiveExtensionAttribute;

	internal static readonly AttributeDescription InternalsVisibleToAttribute;

	internal static readonly AttributeDescription AssemblySignatureKeyAttribute;

	internal static readonly AttributeDescription AssemblyKeyFileAttribute;

	internal static readonly AttributeDescription AssemblyKeyNameAttribute;

	internal static readonly AttributeDescription ParamArrayAttribute;

	internal static readonly AttributeDescription ParamCollectionAttribute;

	internal static readonly AttributeDescription DefaultMemberAttribute;

	internal static readonly AttributeDescription IndexerNameAttribute;

	internal static readonly AttributeDescription AssemblyDelaySignAttribute;

	internal static readonly AttributeDescription AssemblyVersionAttribute;

	internal static readonly AttributeDescription AssemblyFileVersionAttribute;

	internal static readonly AttributeDescription AssemblyTitleAttribute;

	internal static readonly AttributeDescription AssemblyDescriptionAttribute;

	internal static readonly AttributeDescription AssemblyCultureAttribute;

	internal static readonly AttributeDescription AssemblyCompanyAttribute;

	internal static readonly AttributeDescription AssemblyProductAttribute;

	internal static readonly AttributeDescription AssemblyInformationalVersionAttribute;

	internal static readonly AttributeDescription AssemblyCopyrightAttribute;

	internal static readonly AttributeDescription SatelliteContractVersionAttribute;

	internal static readonly AttributeDescription AssemblyTrademarkAttribute;

	internal static readonly AttributeDescription AssemblyFlagsAttribute;

	internal static readonly AttributeDescription DecimalConstantAttribute;

	internal static readonly AttributeDescription IUnknownConstantAttribute;

	internal static readonly AttributeDescription CallerFilePathAttribute;

	internal static readonly AttributeDescription CallerLineNumberAttribute;

	internal static readonly AttributeDescription CallerMemberNameAttribute;

	internal static readonly AttributeDescription CallerArgumentExpressionAttribute;

	internal static readonly AttributeDescription IDispatchConstantAttribute;

	internal static readonly AttributeDescription DefaultParameterValueAttribute;

	internal static readonly AttributeDescription UnverifiableCodeAttribute;

	internal static readonly AttributeDescription SecurityPermissionAttribute;

	internal static readonly AttributeDescription DllImportAttribute;

	internal static readonly AttributeDescription MethodImplAttribute;

	internal static readonly AttributeDescription PreserveSigAttribute;

	internal static readonly AttributeDescription DefaultCharSetAttribute;

	internal static readonly AttributeDescription SpecialNameAttribute;

	internal static readonly AttributeDescription SerializableAttribute;

	internal static readonly AttributeDescription NonSerializedAttribute;

	internal static readonly AttributeDescription StructLayoutAttribute;

	internal static readonly AttributeDescription FieldOffsetAttribute;

	internal static readonly AttributeDescription FixedBufferAttribute;

	internal static readonly AttributeDescription InterceptsLocationAttribute;

	internal static readonly AttributeDescription AllowNullAttribute;

	internal static readonly AttributeDescription DisallowNullAttribute;

	internal static readonly AttributeDescription MaybeNullAttribute;

	internal static readonly AttributeDescription MaybeNullWhenAttribute;

	internal static readonly AttributeDescription NotNullAttribute;

	internal static readonly AttributeDescription MemberNotNullAttribute;

	internal static readonly AttributeDescription MemberNotNullWhenAttribute;

	internal static readonly AttributeDescription NotNullIfNotNullAttribute;

	internal static readonly AttributeDescription NotNullWhenAttribute;

	internal static readonly AttributeDescription DoesNotReturnIfAttribute;

	internal static readonly AttributeDescription DoesNotReturnAttribute;

	internal static readonly AttributeDescription MarshalAsAttribute;

	internal static readonly AttributeDescription InAttribute;

	internal static readonly AttributeDescription OutAttribute;

	internal static readonly AttributeDescription IsReadOnlyAttribute;

	internal static readonly AttributeDescription RequiresLocationAttribute;

	internal static readonly AttributeDescription IsUnmanagedAttribute;

	internal static readonly AttributeDescription CoClassAttribute;

	internal static readonly AttributeDescription GuidAttribute;

	internal static readonly AttributeDescription CLSCompliantAttribute;

	internal static readonly AttributeDescription HostProtectionAttribute;

	internal static readonly AttributeDescription SuppressUnmanagedCodeSecurityAttribute;

	internal static readonly AttributeDescription PrincipalPermissionAttribute;

	internal static readonly AttributeDescription PermissionSetAttribute;

	internal static readonly AttributeDescription TypeIdentifierAttribute;

	internal static readonly AttributeDescription VisualBasicEmbeddedAttribute;

	internal static readonly AttributeDescription CodeAnalysisEmbeddedAttribute;

	internal static readonly AttributeDescription VisualBasicComClassAttribute;

	internal static readonly AttributeDescription StandardModuleAttribute;

	internal static readonly AttributeDescription OptionCompareAttribute;

	internal static readonly AttributeDescription AccessedThroughPropertyAttribute;

	internal static readonly AttributeDescription WebMethodAttribute;

	internal static readonly AttributeDescription DateTimeConstantAttribute;

	internal static readonly AttributeDescription ClassInterfaceAttribute;

	internal static readonly AttributeDescription ComSourceInterfacesAttribute;

	internal static readonly AttributeDescription ComVisibleAttribute;

	internal static readonly AttributeDescription DispIdAttribute;

	internal static readonly AttributeDescription TypeLibVersionAttribute;

	internal static readonly AttributeDescription ComCompatibleVersionAttribute;

	internal static readonly AttributeDescription InterfaceTypeAttribute;

	internal static readonly AttributeDescription WindowsRuntimeImportAttribute;

	internal static readonly AttributeDescription DynamicSecurityMethodAttribute;

	internal static readonly AttributeDescription RequiredAttributeAttribute;

	internal static readonly AttributeDescription AsyncMethodBuilderAttribute;

	internal static readonly AttributeDescription AsyncStateMachineAttribute;

	internal static readonly AttributeDescription IteratorStateMachineAttribute;

	internal static readonly AttributeDescription AsyncIteratorStateMachineAttribute;

	internal static readonly AttributeDescription CompilationRelaxationsAttribute;

	internal static readonly AttributeDescription ReferenceAssemblyAttribute;

	internal static readonly AttributeDescription RuntimeCompatibilityAttribute;

	internal static readonly AttributeDescription DebuggableAttribute;

	internal static readonly AttributeDescription TypeForwardedToAttribute;

	internal static readonly AttributeDescription STAThreadAttribute;

	internal static readonly AttributeDescription MTAThreadAttribute;

	internal static readonly AttributeDescription ObsoleteAttribute;

	internal static readonly AttributeDescription TypeLibTypeAttribute;

	internal static readonly AttributeDescription DynamicAttribute;

	internal static readonly AttributeDescription TupleElementNamesAttribute;

	internal static readonly AttributeDescription IsByRefLikeAttribute;

	internal static readonly AttributeDescription DebuggerHiddenAttribute;

	internal static readonly AttributeDescription DebuggerNonUserCodeAttribute;

	internal static readonly AttributeDescription DebuggerStepperBoundaryAttribute;

	internal static readonly AttributeDescription DebuggerStepThroughAttribute;

	internal static readonly AttributeDescription SecurityCriticalAttribute;

	internal static readonly AttributeDescription SecuritySafeCriticalAttribute;

	internal static readonly AttributeDescription DesignerGeneratedAttribute;

	internal static readonly AttributeDescription MyGroupCollectionAttribute;

	internal static readonly AttributeDescription ComEventInterfaceAttribute;

	internal static readonly AttributeDescription BestFitMappingAttribute;

	internal static readonly AttributeDescription FlagsAttribute;

	internal static readonly AttributeDescription LCIDConversionAttribute;

	internal static readonly AttributeDescription UnmanagedFunctionPointerAttribute;

	internal static readonly AttributeDescription PrimaryInteropAssemblyAttribute;

	internal static readonly AttributeDescription ImportedFromTypeLibAttribute;

	internal static readonly AttributeDescription DefaultEventAttribute;

	internal static readonly AttributeDescription AssemblyConfigurationAttribute;

	internal static readonly AttributeDescription AssemblyAlgorithmIdAttribute;

	internal static readonly AttributeDescription DeprecatedAttribute;

	internal static readonly AttributeDescription NullableAttribute;

	internal static readonly AttributeDescription NullableContextAttribute;

	internal static readonly AttributeDescription NullablePublicOnlyAttribute;

	internal static readonly AttributeDescription WindowsExperimentalAttribute;

	internal static readonly AttributeDescription ExperimentalAttribute;

	internal static readonly AttributeDescription ExcludeFromCodeCoverageAttribute;

	internal static readonly AttributeDescription EnumeratorCancellationAttribute;

	internal static readonly AttributeDescription SkipLocalsInitAttribute;

	internal static readonly AttributeDescription NativeIntegerAttribute;

	internal static readonly AttributeDescription ScopedRefAttribute;

	internal static readonly AttributeDescription RefSafetyRulesAttribute;

	internal static readonly AttributeDescription ModuleInitializerAttribute;

	internal static readonly AttributeDescription UnmanagedCallersOnlyAttribute;

	internal static readonly AttributeDescription InterpolatedStringHandlerAttribute;

	internal static readonly AttributeDescription InterpolatedStringHandlerArgumentAttribute;

	internal static readonly AttributeDescription RequiredMemberAttribute;

	internal static readonly AttributeDescription SetsRequiredMembersAttribute;

	internal static readonly AttributeDescription CompilerFeatureRequiredAttribute;

	internal static readonly AttributeDescription UnscopedRefAttribute;

	internal static readonly AttributeDescription InlineArrayAttribute;

	internal static readonly AttributeDescription CollectionBuilderAttribute;

	internal static readonly AttributeDescription OverloadResolutionPriorityAttribute;

	internal static readonly AttributeDescription CompilerLoweringPreserveAttribute;

	internal static readonly AttributeDescription ExtensionMarkerAttribute;

	internal static readonly AttributeDescription RuntimeAsyncMethodGenerationAttribute;

	internal static readonly AttributeDescription MetadataUpdateDeletedAttribute;

	public string FullName => Namespace + "." + Name;

	public override string ToString()
	{
		return FullName + "(" + Signatures.Length + ")";
	}

	internal int GetParameterCount(int signatureIndex)
	{
		return Signatures[signatureIndex][1];
	}

	static AttributeDescription()
	{
		s_signature_HasThis_Void = new byte[3] { 32, 0, 1 };
		s_signature_HasThis_Void_Byte = new byte[4] { 32, 1, 1, 5 };
		s_signature_HasThis_Void_Int16 = new byte[4] { 32, 1, 1, 6 };
		s_signature_HasThis_Void_Int32 = new byte[4] { 32, 1, 1, 8 };
		s_signature_HasThis_Void_UInt32 = new byte[4] { 32, 1, 1, 9 };
		s_signature_HasThis_Void_Int32_Int32 = new byte[5] { 32, 2, 1, 8, 8 };
		s_signature_HasThis_Void_Int32_Int32_Int32_Int32 = new byte[7] { 32, 4, 1, 8, 8, 8, 8 };
		s_signature_HasThis_Void_String = new byte[4] { 32, 1, 1, 14 };
		s_signature_HasThis_Void_Object = new byte[4] { 32, 1, 1, 28 };
		s_signature_HasThis_Void_String_String = new byte[5] { 32, 2, 1, 14, 14 };
		s_signature_HasThis_Void_String_Boolean = new byte[5] { 32, 2, 1, 14, 2 };
		s_signature_HasThis_Void_String_String_String = new byte[6] { 32, 3, 1, 14, 14, 14 };
		s_signature_HasThis_Void_String_String_String_String = new byte[7] { 32, 4, 1, 14, 14, 14, 14 };
		s_signature_HasThis_Void_AttributeTargets = new byte[5] { 32, 1, 1, 64, 0 };
		s_signature_HasThis_Void_AssemblyNameFlags = new byte[5] { 32, 1, 1, 64, 1 };
		s_signature_HasThis_Void_MethodImplOptions = new byte[5] { 32, 1, 1, 64, 2 };
		s_signature_HasThis_Void_CharSet = new byte[5] { 32, 1, 1, 64, 3 };
		s_signature_HasThis_Void_LayoutKind = new byte[5] { 32, 1, 1, 64, 4 };
		s_signature_HasThis_Void_UnmanagedType = new byte[5] { 32, 1, 1, 64, 5 };
		s_signature_HasThis_Void_TypeLibTypeFlags = new byte[5] { 32, 1, 1, 64, 6 };
		s_signature_HasThis_Void_ClassInterfaceType = new byte[5] { 32, 1, 1, 64, 7 };
		s_signature_HasThis_Void_ComInterfaceType = new byte[5] { 32, 1, 1, 64, 8 };
		s_signature_HasThis_Void_CompilationRelaxations = new byte[5] { 32, 1, 1, 64, 9 };
		s_signature_HasThis_Void_DebuggingModes = new byte[5] { 32, 1, 1, 64, 10 };
		s_signature_HasThis_Void_SecurityCriticalScope = new byte[5] { 32, 1, 1, 64, 11 };
		s_signature_HasThis_Void_CallingConvention = new byte[5] { 32, 1, 1, 64, 12 };
		s_signature_HasThis_Void_AssemblyHashAlgorithm = new byte[5] { 32, 1, 1, 64, 13 };
		s_signature_HasThis_Void_Int64 = new byte[4] { 32, 1, 1, 10 };
		s_signature_HasThis_Void_UInt8_UInt8_UInt32_UInt32_UInt32 = new byte[8] { 32, 5, 1, 5, 5, 9, 9, 9 };
		s_signature_HasThis_Void_UIn8_UInt8_Int32_Int32_Int32 = new byte[8] { 32, 5, 1, 5, 5, 8, 8, 8 };
		s_signature_HasThis_Void_Boolean = new byte[4] { 32, 1, 1, 2 };
		s_signature_HasThis_Void_Boolean_Boolean = new byte[5] { 32, 2, 1, 2, 2 };
		s_signature_HasThis_Void_Boolean_TransactionOption = new byte[6] { 32, 2, 1, 2, 64, 14 };
		s_signature_HasThis_Void_Boolean_TransactionOption_Int32 = new byte[7] { 32, 3, 1, 2, 64, 14, 8 };
		s_signature_HasThis_Void_Boolean_TransactionOption_Int32_Boolean = new byte[8] { 32, 4, 1, 2, 64, 14, 8, 2 };
		s_signature_HasThis_Void_SecurityAction = new byte[5] { 32, 1, 1, 64, 15 };
		s_signature_HasThis_Void_Type = new byte[5] { 32, 1, 1, 64, 16 };
		s_signature_HasThis_Void_Type_Type = new byte[7] { 32, 2, 1, 64, 16, 64, 16 };
		s_signature_HasThis_Void_Type_Type_Type = new byte[9] { 32, 3, 1, 64, 16, 64, 16, 64, 16 };
		s_signature_HasThis_Void_Type_Type_Type_Type = new byte[11]
		{
			32, 4, 1, 64, 16, 64, 16, 64, 16, 64,
			16
		};
		s_signature_HasThis_Void_Type_Int32 = new byte[6] { 32, 2, 1, 64, 16, 8 };
		s_signature_HasThis_Void_Type_String = new byte[6] { 32, 2, 1, 64, 16, 14 };
		s_signature_HasThis_Void_String_Int32_Int32 = new byte[6] { 32, 3, 1, 14, 8, 8 };
		s_signature_HasThis_Void_Int32_String = new byte[5] { 32, 2, 1, 8, 14 };
		s_signature_HasThis_Void_SzArray_Boolean = new byte[5] { 32, 1, 1, 29, 2 };
		s_signature_HasThis_Void_SzArray_Byte = new byte[5] { 32, 1, 1, 29, 5 };
		s_signature_HasThis_Void_SzArray_String = new byte[5] { 32, 1, 1, 29, 14 };
		s_signature_HasThis_Void_Boolean_SzArray_String = new byte[6] { 32, 2, 1, 2, 29, 14 };
		s_signature_HasThis_Void_Boolean_String = new byte[5] { 32, 2, 1, 2, 14 };
		s_signature_HasThis_Void_String_DeprecationType_UInt32 = new byte[7] { 32, 3, 1, 14, 64, 17, 9 };
		s_signature_HasThis_Void_String_DeprecationType_UInt32_Platform = new byte[9] { 32, 4, 1, 14, 64, 17, 9, 64, 18 };
		s_signature_HasThis_Void_String_DeprecationType_UInt32_Type = new byte[9] { 32, 4, 1, 14, 64, 17, 9, 64, 16 };
		s_signature_HasThis_Void_String_DeprecationType_UInt32_String = new byte[8] { 32, 4, 1, 14, 64, 17, 9, 14 };
		s_signatures_HasThis_Void_Only = new byte[1][] { s_signature_HasThis_Void };
		s_signatures_HasThis_Void_String_Only = new byte[1][] { s_signature_HasThis_Void_String };
		s_signatures_HasThis_Void_Type_Only = new byte[1][] { s_signature_HasThis_Void_Type };
		s_signatures_HasThis_Void_Boolean_Only = new byte[1][] { s_signature_HasThis_Void_Boolean };
		s_signatures_HasThis_Void_Int32_Only = new byte[1][] { s_signature_HasThis_Void_Int32 };
		s_signaturesOfTypeIdentifierAttribute = new byte[2][] { s_signature_HasThis_Void, s_signature_HasThis_Void_String_String };
		s_signaturesOfAttributeUsage = new byte[1][] { s_signature_HasThis_Void_AttributeTargets };
		s_signaturesOfAssemblySignatureKeyAttribute = new byte[1][] { s_signature_HasThis_Void_String_String };
		s_signaturesOfAssemblyFlagsAttribute = new byte[3][] { s_signature_HasThis_Void_AssemblyNameFlags, s_signature_HasThis_Void_Int32, s_signature_HasThis_Void_UInt32 };
		s_signaturesOfDefaultParameterValueAttribute = new byte[1][] { s_signature_HasThis_Void_Object };
		s_signaturesOfDateTimeConstantAttribute = new byte[1][] { s_signature_HasThis_Void_Int64 };
		s_signaturesOfDecimalConstantAttribute = new byte[2][] { s_signature_HasThis_Void_UInt8_UInt8_UInt32_UInt32_UInt32, s_signature_HasThis_Void_UIn8_UInt8_Int32_Int32_Int32 };
		s_signaturesOfSecurityPermissionAttribute = new byte[1][] { s_signature_HasThis_Void_SecurityAction };
		s_signaturesOfMethodImplAttribute = new byte[3][] { s_signature_HasThis_Void, s_signature_HasThis_Void_Int16, s_signature_HasThis_Void_MethodImplOptions };
		s_signaturesOfDefaultCharSetAttribute = new byte[1][] { s_signature_HasThis_Void_CharSet };
		s_signaturesOfMemberNotNullAttribute = new byte[2][] { s_signature_HasThis_Void_String, s_signature_HasThis_Void_SzArray_String };
		s_signaturesOfMemberNotNullWhenAttribute = new byte[2][] { s_signature_HasThis_Void_Boolean_String, s_signature_HasThis_Void_Boolean_SzArray_String };
		s_signaturesOfFixedBufferAttribute = new byte[1][] { s_signature_HasThis_Void_Type_Int32 };
		s_signaturesOfInterceptsLocationAttribute = new byte[2][] { s_signature_HasThis_Void_String_Int32_Int32, s_signature_HasThis_Void_Int32_String };
		s_signaturesOfPrincipalPermissionAttribute = new byte[1][] { s_signature_HasThis_Void_SecurityAction };
		s_signaturesOfPermissionSetAttribute = new byte[1][] { s_signature_HasThis_Void_SecurityAction };
		s_signaturesOfStructLayoutAttribute = new byte[2][] { s_signature_HasThis_Void_Int16, s_signature_HasThis_Void_LayoutKind };
		s_signaturesOfMarshalAsAttribute = new byte[2][] { s_signature_HasThis_Void_Int16, s_signature_HasThis_Void_UnmanagedType };
		s_signaturesOfTypeLibTypeAttribute = new byte[2][] { s_signature_HasThis_Void_Int16, s_signature_HasThis_Void_TypeLibTypeFlags };
		s_signaturesOfWebMethodAttribute = new byte[5][] { s_signature_HasThis_Void, s_signature_HasThis_Void_Boolean, s_signature_HasThis_Void_Boolean_TransactionOption, s_signature_HasThis_Void_Boolean_TransactionOption_Int32, s_signature_HasThis_Void_Boolean_TransactionOption_Int32_Boolean };
		s_signaturesOfHostProtectionAttribute = new byte[2][] { s_signature_HasThis_Void, s_signature_HasThis_Void_SecurityAction };
		s_signaturesOfVisualBasicComClassAttribute = new byte[4][] { s_signature_HasThis_Void, s_signature_HasThis_Void_String, s_signature_HasThis_Void_String_String, s_signature_HasThis_Void_String_String_String };
		s_signaturesOfClassInterfaceAttribute = new byte[2][] { s_signature_HasThis_Void_Int16, s_signature_HasThis_Void_ClassInterfaceType };
		s_signaturesOfInterfaceTypeAttribute = new byte[2][] { s_signature_HasThis_Void_Int16, s_signature_HasThis_Void_ComInterfaceType };
		s_signaturesOfCompilationRelaxationsAttribute = new byte[2][] { s_signature_HasThis_Void_Int32, s_signature_HasThis_Void_CompilationRelaxations };
		s_signaturesOfDebuggableAttribute = new byte[2][] { s_signature_HasThis_Void_Boolean_Boolean, s_signature_HasThis_Void_DebuggingModes };
		s_signaturesOfComSourceInterfacesAttribute = new byte[5][] { s_signature_HasThis_Void_String, s_signature_HasThis_Void_Type, s_signature_HasThis_Void_Type_Type, s_signature_HasThis_Void_Type_Type_Type, s_signature_HasThis_Void_Type_Type_Type_Type };
		s_signaturesOfTypeLibVersionAttribute = new byte[1][] { s_signature_HasThis_Void_Int32_Int32 };
		s_signaturesOfComCompatibleVersionAttribute = new byte[1][] { s_signature_HasThis_Void_Int32_Int32_Int32_Int32 };
		s_signaturesOfObsoleteAttribute = new byte[3][] { s_signature_HasThis_Void, s_signature_HasThis_Void_String, s_signature_HasThis_Void_String_Boolean };
		s_signaturesOfDynamicAttribute = new byte[2][] { s_signature_HasThis_Void, s_signature_HasThis_Void_SzArray_Boolean };
		s_signaturesOfTupleElementNamesAttribute = new byte[2][] { s_signature_HasThis_Void, s_signature_HasThis_Void_SzArray_String };
		s_signaturesOfSecurityCriticalAttribute = new byte[2][] { s_signature_HasThis_Void, s_signature_HasThis_Void_SecurityCriticalScope };
		s_signaturesOfMyGroupCollectionAttribute = new byte[1][] { s_signature_HasThis_Void_String_String_String_String };
		s_signaturesOfComEventInterfaceAttribute = new byte[1][] { s_signature_HasThis_Void_Type_Type };
		s_signaturesOfUnmanagedFunctionPointerAttribute = new byte[1][] { s_signature_HasThis_Void_CallingConvention };
		s_signaturesOfPrimaryInteropAssemblyAttribute = new byte[1][] { s_signature_HasThis_Void_Int32_Int32 };
		s_signaturesOfAssemblyAlgorithmIdAttribute = new byte[2][] { s_signature_HasThis_Void_AssemblyHashAlgorithm, s_signature_HasThis_Void_UInt32 };
		s_signaturesOfDeprecatedAttribute = new byte[4][] { s_signature_HasThis_Void_String_DeprecationType_UInt32, s_signature_HasThis_Void_String_DeprecationType_UInt32_Platform, s_signature_HasThis_Void_String_DeprecationType_UInt32_Type, s_signature_HasThis_Void_String_DeprecationType_UInt32_String };
		s_signaturesOfNullableAttribute = new byte[2][] { s_signature_HasThis_Void_Byte, s_signature_HasThis_Void_SzArray_Byte };
		s_signaturesOfNullableContextAttribute = new byte[1][] { s_signature_HasThis_Void_Byte };
		s_signaturesOfNativeIntegerAttribute = new byte[2][] { s_signature_HasThis_Void, s_signature_HasThis_Void_SzArray_Boolean };
		s_signaturesOfInterpolatedStringArgumentAttribute = new byte[2][] { s_signature_HasThis_Void_String, s_signature_HasThis_Void_SzArray_String };
		s_signaturesOfCollectionBuilderAttribute = new byte[1][] { s_signature_HasThis_Void_Type_String };
		OptionalAttribute = new AttributeDescription("System.Runtime.InteropServices", "OptionalAttribute", s_signatures_HasThis_Void_Only);
		ComImportAttribute = new AttributeDescription("System.Runtime.InteropServices", "ComImportAttribute", s_signatures_HasThis_Void_Only);
		AttributeUsageAttribute = new AttributeDescription("System", "AttributeUsageAttribute", s_signaturesOfAttributeUsage);
		ConditionalAttribute = new AttributeDescription("System.Diagnostics", "ConditionalAttribute", s_signatures_HasThis_Void_String_Only);
		CaseInsensitiveExtensionAttribute = new AttributeDescription("System.Runtime.CompilerServices", "ExtensionAttribute", s_signatures_HasThis_Void_Only, matchIgnoringCase: true);
		CaseSensitiveExtensionAttribute = new AttributeDescription("System.Runtime.CompilerServices", "ExtensionAttribute", s_signatures_HasThis_Void_Only);
		InternalsVisibleToAttribute = new AttributeDescription("System.Runtime.CompilerServices", "InternalsVisibleToAttribute", s_signatures_HasThis_Void_String_Only);
		AssemblySignatureKeyAttribute = new AttributeDescription("System.Reflection", "AssemblySignatureKeyAttribute", s_signaturesOfAssemblySignatureKeyAttribute);
		AssemblyKeyFileAttribute = new AttributeDescription("System.Reflection", "AssemblyKeyFileAttribute", s_signatures_HasThis_Void_String_Only);
		AssemblyKeyNameAttribute = new AttributeDescription("System.Reflection", "AssemblyKeyNameAttribute", s_signatures_HasThis_Void_String_Only);
		ParamArrayAttribute = new AttributeDescription("System", "ParamArrayAttribute", s_signatures_HasThis_Void_Only);
		ParamCollectionAttribute = new AttributeDescription("System.Runtime.CompilerServices", "ParamCollectionAttribute", s_signatures_HasThis_Void_Only);
		DefaultMemberAttribute = new AttributeDescription("System.Reflection", "DefaultMemberAttribute", s_signatures_HasThis_Void_String_Only);
		IndexerNameAttribute = new AttributeDescription("System.Runtime.CompilerServices", "IndexerNameAttribute", s_signatures_HasThis_Void_String_Only);
		AssemblyDelaySignAttribute = new AttributeDescription("System.Reflection", "AssemblyDelaySignAttribute", s_signatures_HasThis_Void_Boolean_Only);
		AssemblyVersionAttribute = new AttributeDescription("System.Reflection", "AssemblyVersionAttribute", s_signatures_HasThis_Void_String_Only);
		AssemblyFileVersionAttribute = new AttributeDescription("System.Reflection", "AssemblyFileVersionAttribute", s_signatures_HasThis_Void_String_Only);
		AssemblyTitleAttribute = new AttributeDescription("System.Reflection", "AssemblyTitleAttribute", s_signatures_HasThis_Void_String_Only);
		AssemblyDescriptionAttribute = new AttributeDescription("System.Reflection", "AssemblyDescriptionAttribute", s_signatures_HasThis_Void_String_Only);
		AssemblyCultureAttribute = new AttributeDescription("System.Reflection", "AssemblyCultureAttribute", s_signatures_HasThis_Void_String_Only);
		AssemblyCompanyAttribute = new AttributeDescription("System.Reflection", "AssemblyCompanyAttribute", s_signatures_HasThis_Void_String_Only);
		AssemblyProductAttribute = new AttributeDescription("System.Reflection", "AssemblyProductAttribute", s_signatures_HasThis_Void_String_Only);
		AssemblyInformationalVersionAttribute = new AttributeDescription("System.Reflection", "AssemblyInformationalVersionAttribute", s_signatures_HasThis_Void_String_Only);
		AssemblyCopyrightAttribute = new AttributeDescription("System.Reflection", "AssemblyCopyrightAttribute", s_signatures_HasThis_Void_String_Only);
		SatelliteContractVersionAttribute = new AttributeDescription("System.Resources", "SatelliteContractVersionAttribute", s_signatures_HasThis_Void_String_Only);
		AssemblyTrademarkAttribute = new AttributeDescription("System.Reflection", "AssemblyTrademarkAttribute", s_signatures_HasThis_Void_String_Only);
		AssemblyFlagsAttribute = new AttributeDescription("System.Reflection", "AssemblyFlagsAttribute", s_signaturesOfAssemblyFlagsAttribute);
		DecimalConstantAttribute = new AttributeDescription("System.Runtime.CompilerServices", "DecimalConstantAttribute", s_signaturesOfDecimalConstantAttribute);
		IUnknownConstantAttribute = new AttributeDescription("System.Runtime.CompilerServices", "IUnknownConstantAttribute", s_signatures_HasThis_Void_Only);
		CallerFilePathAttribute = new AttributeDescription("System.Runtime.CompilerServices", "CallerFilePathAttribute", s_signatures_HasThis_Void_Only);
		CallerLineNumberAttribute = new AttributeDescription("System.Runtime.CompilerServices", "CallerLineNumberAttribute", s_signatures_HasThis_Void_Only);
		CallerMemberNameAttribute = new AttributeDescription("System.Runtime.CompilerServices", "CallerMemberNameAttribute", s_signatures_HasThis_Void_Only);
		CallerArgumentExpressionAttribute = new AttributeDescription("System.Runtime.CompilerServices", "CallerArgumentExpressionAttribute", s_signatures_HasThis_Void_String_Only);
		IDispatchConstantAttribute = new AttributeDescription("System.Runtime.CompilerServices", "IDispatchConstantAttribute", s_signatures_HasThis_Void_Only);
		DefaultParameterValueAttribute = new AttributeDescription("System.Runtime.InteropServices", "DefaultParameterValueAttribute", s_signaturesOfDefaultParameterValueAttribute);
		UnverifiableCodeAttribute = new AttributeDescription("System.Runtime.InteropServices", "UnverifiableCodeAttribute", s_signatures_HasThis_Void_Only);
		SecurityPermissionAttribute = new AttributeDescription("System.Runtime.InteropServices", "SecurityPermissionAttribute", s_signaturesOfSecurityPermissionAttribute);
		DllImportAttribute = new AttributeDescription("System.Runtime.InteropServices", "DllImportAttribute", s_signatures_HasThis_Void_String_Only);
		MethodImplAttribute = new AttributeDescription("System.Runtime.CompilerServices", "MethodImplAttribute", s_signaturesOfMethodImplAttribute);
		PreserveSigAttribute = new AttributeDescription("System.Runtime.InteropServices", "PreserveSigAttribute", s_signatures_HasThis_Void_Only);
		DefaultCharSetAttribute = new AttributeDescription("System.Runtime.InteropServices", "DefaultCharSetAttribute", s_signaturesOfDefaultCharSetAttribute);
		SpecialNameAttribute = new AttributeDescription("System.Runtime.CompilerServices", "SpecialNameAttribute", s_signatures_HasThis_Void_Only);
		SerializableAttribute = new AttributeDescription("System", "SerializableAttribute", s_signatures_HasThis_Void_Only);
		NonSerializedAttribute = new AttributeDescription("System", "NonSerializedAttribute", s_signatures_HasThis_Void_Only);
		StructLayoutAttribute = new AttributeDescription("System.Runtime.InteropServices", "StructLayoutAttribute", s_signaturesOfStructLayoutAttribute);
		FieldOffsetAttribute = new AttributeDescription("System.Runtime.InteropServices", "FieldOffsetAttribute", s_signatures_HasThis_Void_Int32_Only);
		FixedBufferAttribute = new AttributeDescription("System.Runtime.CompilerServices", "FixedBufferAttribute", s_signaturesOfFixedBufferAttribute);
		InterceptsLocationAttribute = new AttributeDescription("System.Runtime.CompilerServices", "InterceptsLocationAttribute", s_signaturesOfInterceptsLocationAttribute);
		AllowNullAttribute = new AttributeDescription("System.Diagnostics.CodeAnalysis", "AllowNullAttribute", s_signatures_HasThis_Void_Only);
		DisallowNullAttribute = new AttributeDescription("System.Diagnostics.CodeAnalysis", "DisallowNullAttribute", s_signatures_HasThis_Void_Only);
		MaybeNullAttribute = new AttributeDescription("System.Diagnostics.CodeAnalysis", "MaybeNullAttribute", s_signatures_HasThis_Void_Only);
		MaybeNullWhenAttribute = new AttributeDescription("System.Diagnostics.CodeAnalysis", "MaybeNullWhenAttribute", s_signatures_HasThis_Void_Boolean_Only);
		NotNullAttribute = new AttributeDescription("System.Diagnostics.CodeAnalysis", "NotNullAttribute", s_signatures_HasThis_Void_Only);
		MemberNotNullAttribute = new AttributeDescription("System.Diagnostics.CodeAnalysis", "MemberNotNullAttribute", s_signaturesOfMemberNotNullAttribute);
		MemberNotNullWhenAttribute = new AttributeDescription("System.Diagnostics.CodeAnalysis", "MemberNotNullWhenAttribute", s_signaturesOfMemberNotNullWhenAttribute);
		NotNullIfNotNullAttribute = new AttributeDescription("System.Diagnostics.CodeAnalysis", "NotNullIfNotNullAttribute", s_signatures_HasThis_Void_String_Only);
		NotNullWhenAttribute = new AttributeDescription("System.Diagnostics.CodeAnalysis", "NotNullWhenAttribute", s_signatures_HasThis_Void_Boolean_Only);
		DoesNotReturnIfAttribute = new AttributeDescription("System.Diagnostics.CodeAnalysis", "DoesNotReturnIfAttribute", s_signatures_HasThis_Void_Boolean_Only);
		DoesNotReturnAttribute = new AttributeDescription("System.Diagnostics.CodeAnalysis", "DoesNotReturnAttribute", s_signatures_HasThis_Void_Only);
		MarshalAsAttribute = new AttributeDescription("System.Runtime.InteropServices", "MarshalAsAttribute", s_signaturesOfMarshalAsAttribute);
		InAttribute = new AttributeDescription("System.Runtime.InteropServices", "InAttribute", s_signatures_HasThis_Void_Only);
		OutAttribute = new AttributeDescription("System.Runtime.InteropServices", "OutAttribute", s_signatures_HasThis_Void_Only);
		IsReadOnlyAttribute = new AttributeDescription("System.Runtime.CompilerServices", "IsReadOnlyAttribute", s_signatures_HasThis_Void_Only);
		RequiresLocationAttribute = new AttributeDescription("System.Runtime.CompilerServices", "RequiresLocationAttribute", s_signatures_HasThis_Void_Only);
		IsUnmanagedAttribute = new AttributeDescription("System.Runtime.CompilerServices", "IsUnmanagedAttribute", s_signatures_HasThis_Void_Only);
		CoClassAttribute = new AttributeDescription("System.Runtime.InteropServices", "CoClassAttribute", s_signatures_HasThis_Void_Type_Only);
		GuidAttribute = new AttributeDescription("System.Runtime.InteropServices", "GuidAttribute", s_signatures_HasThis_Void_String_Only);
		CLSCompliantAttribute = new AttributeDescription("System", "CLSCompliantAttribute", s_signatures_HasThis_Void_Boolean_Only);
		HostProtectionAttribute = new AttributeDescription("System.Security.Permissions", "HostProtectionAttribute", s_signaturesOfHostProtectionAttribute);
		SuppressUnmanagedCodeSecurityAttribute = new AttributeDescription("System.Security", "SuppressUnmanagedCodeSecurityAttribute", s_signatures_HasThis_Void_Only);
		PrincipalPermissionAttribute = new AttributeDescription("System.Security.Permissions", "PrincipalPermissionAttribute", s_signaturesOfPrincipalPermissionAttribute);
		PermissionSetAttribute = new AttributeDescription("System.Security.Permissions", "PermissionSetAttribute", s_signaturesOfPermissionSetAttribute);
		TypeIdentifierAttribute = new AttributeDescription("System.Runtime.InteropServices", "TypeIdentifierAttribute", s_signaturesOfTypeIdentifierAttribute);
		VisualBasicEmbeddedAttribute = new AttributeDescription("Microsoft.VisualBasic", "Embedded", s_signatures_HasThis_Void_Only);
		CodeAnalysisEmbeddedAttribute = new AttributeDescription("Microsoft.CodeAnalysis", "EmbeddedAttribute", s_signatures_HasThis_Void_Only);
		VisualBasicComClassAttribute = new AttributeDescription("Microsoft.VisualBasic", "ComClassAttribute", s_signaturesOfVisualBasicComClassAttribute);
		StandardModuleAttribute = new AttributeDescription("Microsoft.VisualBasic.CompilerServices", "StandardModuleAttribute", s_signatures_HasThis_Void_Only);
		OptionCompareAttribute = new AttributeDescription("Microsoft.VisualBasic.CompilerServices", "OptionCompareAttribute", s_signatures_HasThis_Void_Only);
		AccessedThroughPropertyAttribute = new AttributeDescription("System.Runtime.CompilerServices", "AccessedThroughPropertyAttribute", s_signatures_HasThis_Void_String_Only);
		WebMethodAttribute = new AttributeDescription("System.Web.Services", "WebMethodAttribute", s_signaturesOfWebMethodAttribute);
		DateTimeConstantAttribute = new AttributeDescription("System.Runtime.CompilerServices", "DateTimeConstantAttribute", s_signaturesOfDateTimeConstantAttribute);
		ClassInterfaceAttribute = new AttributeDescription("System.Runtime.InteropServices", "ClassInterfaceAttribute", s_signaturesOfClassInterfaceAttribute);
		ComSourceInterfacesAttribute = new AttributeDescription("System.Runtime.InteropServices", "ComSourceInterfacesAttribute", s_signaturesOfComSourceInterfacesAttribute);
		ComVisibleAttribute = new AttributeDescription("System.Runtime.InteropServices", "ComVisibleAttribute", s_signatures_HasThis_Void_Boolean_Only);
		DispIdAttribute = new AttributeDescription("System.Runtime.InteropServices", "DispIdAttribute", s_signatures_HasThis_Void_Int32_Only);
		TypeLibVersionAttribute = new AttributeDescription("System.Runtime.InteropServices", "TypeLibVersionAttribute", s_signaturesOfTypeLibVersionAttribute);
		ComCompatibleVersionAttribute = new AttributeDescription("System.Runtime.InteropServices", "ComCompatibleVersionAttribute", s_signaturesOfComCompatibleVersionAttribute);
		InterfaceTypeAttribute = new AttributeDescription("System.Runtime.InteropServices", "InterfaceTypeAttribute", s_signaturesOfInterfaceTypeAttribute);
		WindowsRuntimeImportAttribute = new AttributeDescription("System.Runtime.InteropServices.WindowsRuntime", "WindowsRuntimeImportAttribute", s_signatures_HasThis_Void_Only);
		DynamicSecurityMethodAttribute = new AttributeDescription("System.Security", "DynamicSecurityMethodAttribute", s_signatures_HasThis_Void_Only);
		RequiredAttributeAttribute = new AttributeDescription("System.Runtime.CompilerServices", "RequiredAttributeAttribute", s_signatures_HasThis_Void_Type_Only);
		AsyncMethodBuilderAttribute = new AttributeDescription("System.Runtime.CompilerServices", "AsyncMethodBuilderAttribute", s_signatures_HasThis_Void_Type_Only);
		AsyncStateMachineAttribute = new AttributeDescription("System.Runtime.CompilerServices", "AsyncStateMachineAttribute", s_signatures_HasThis_Void_Type_Only);
		IteratorStateMachineAttribute = new AttributeDescription("System.Runtime.CompilerServices", "IteratorStateMachineAttribute", s_signatures_HasThis_Void_Type_Only);
		AsyncIteratorStateMachineAttribute = new AttributeDescription("System.Runtime.CompilerServices", "AsyncIteratorStateMachineAttribute", s_signatures_HasThis_Void_Type_Only);
		CompilationRelaxationsAttribute = new AttributeDescription("System.Runtime.CompilerServices", "CompilationRelaxationsAttribute", s_signaturesOfCompilationRelaxationsAttribute);
		ReferenceAssemblyAttribute = new AttributeDescription("System.Runtime.CompilerServices", "ReferenceAssemblyAttribute", s_signatures_HasThis_Void_Only);
		RuntimeCompatibilityAttribute = new AttributeDescription("System.Runtime.CompilerServices", "RuntimeCompatibilityAttribute", s_signatures_HasThis_Void_Only);
		DebuggableAttribute = new AttributeDescription("System.Diagnostics", "DebuggableAttribute", s_signaturesOfDebuggableAttribute);
		TypeForwardedToAttribute = new AttributeDescription("System.Runtime.CompilerServices", "TypeForwardedToAttribute", s_signatures_HasThis_Void_Type_Only);
		STAThreadAttribute = new AttributeDescription("System", "STAThreadAttribute", s_signatures_HasThis_Void_Only);
		MTAThreadAttribute = new AttributeDescription("System", "MTAThreadAttribute", s_signatures_HasThis_Void_Only);
		ObsoleteAttribute = new AttributeDescription("System", "ObsoleteAttribute", s_signaturesOfObsoleteAttribute);
		TypeLibTypeAttribute = new AttributeDescription("System.Runtime.InteropServices", "TypeLibTypeAttribute", s_signaturesOfTypeLibTypeAttribute);
		DynamicAttribute = new AttributeDescription("System.Runtime.CompilerServices", "DynamicAttribute", s_signaturesOfDynamicAttribute);
		TupleElementNamesAttribute = new AttributeDescription("System.Runtime.CompilerServices", "TupleElementNamesAttribute", s_signaturesOfTupleElementNamesAttribute);
		IsByRefLikeAttribute = new AttributeDescription("System.Runtime.CompilerServices", "IsByRefLikeAttribute", s_signatures_HasThis_Void_Only);
		DebuggerHiddenAttribute = new AttributeDescription("System.Diagnostics", "DebuggerHiddenAttribute", s_signatures_HasThis_Void_Only);
		DebuggerNonUserCodeAttribute = new AttributeDescription("System.Diagnostics", "DebuggerNonUserCodeAttribute", s_signatures_HasThis_Void_Only);
		DebuggerStepperBoundaryAttribute = new AttributeDescription("System.Diagnostics", "DebuggerStepperBoundaryAttribute", s_signatures_HasThis_Void_Only);
		DebuggerStepThroughAttribute = new AttributeDescription("System.Diagnostics", "DebuggerStepThroughAttribute", s_signatures_HasThis_Void_Only);
		SecurityCriticalAttribute = new AttributeDescription("System.Security", "SecurityCriticalAttribute", s_signaturesOfSecurityCriticalAttribute);
		SecuritySafeCriticalAttribute = new AttributeDescription("System.Security", "SecuritySafeCriticalAttribute", s_signatures_HasThis_Void_Only);
		DesignerGeneratedAttribute = new AttributeDescription("Microsoft.VisualBasic.CompilerServices", "DesignerGeneratedAttribute", s_signatures_HasThis_Void_Only);
		MyGroupCollectionAttribute = new AttributeDescription("Microsoft.VisualBasic", "MyGroupCollectionAttribute", s_signaturesOfMyGroupCollectionAttribute);
		ComEventInterfaceAttribute = new AttributeDescription("System.Runtime.InteropServices", "ComEventInterfaceAttribute", s_signaturesOfComEventInterfaceAttribute);
		BestFitMappingAttribute = new AttributeDescription("System.Runtime.InteropServices", "BestFitMappingAttribute", s_signatures_HasThis_Void_Boolean_Only);
		FlagsAttribute = new AttributeDescription("System", "FlagsAttribute", s_signatures_HasThis_Void_Only);
		LCIDConversionAttribute = new AttributeDescription("System.Runtime.InteropServices", "LCIDConversionAttribute", s_signatures_HasThis_Void_Int32_Only);
		UnmanagedFunctionPointerAttribute = new AttributeDescription("System.Runtime.InteropServices", "UnmanagedFunctionPointerAttribute", s_signaturesOfUnmanagedFunctionPointerAttribute);
		PrimaryInteropAssemblyAttribute = new AttributeDescription("System.Runtime.InteropServices", "PrimaryInteropAssemblyAttribute", s_signaturesOfPrimaryInteropAssemblyAttribute);
		ImportedFromTypeLibAttribute = new AttributeDescription("System.Runtime.InteropServices", "ImportedFromTypeLibAttribute", s_signatures_HasThis_Void_String_Only);
		DefaultEventAttribute = new AttributeDescription("System.ComponentModel", "DefaultEventAttribute", s_signatures_HasThis_Void_String_Only);
		AssemblyConfigurationAttribute = new AttributeDescription("System.Reflection", "AssemblyConfigurationAttribute", s_signatures_HasThis_Void_String_Only);
		AssemblyAlgorithmIdAttribute = new AttributeDescription("System.Reflection", "AssemblyAlgorithmIdAttribute", s_signaturesOfAssemblyAlgorithmIdAttribute);
		DeprecatedAttribute = new AttributeDescription("Windows.Foundation.Metadata", "DeprecatedAttribute", s_signaturesOfDeprecatedAttribute);
		NullableAttribute = new AttributeDescription("System.Runtime.CompilerServices", "NullableAttribute", s_signaturesOfNullableAttribute);
		NullableContextAttribute = new AttributeDescription("System.Runtime.CompilerServices", "NullableContextAttribute", s_signaturesOfNullableContextAttribute);
		NullablePublicOnlyAttribute = new AttributeDescription("System.Runtime.CompilerServices", "NullablePublicOnlyAttribute", s_signatures_HasThis_Void_Boolean_Only);
		WindowsExperimentalAttribute = new AttributeDescription("Windows.Foundation.Metadata", "ExperimentalAttribute", s_signatures_HasThis_Void_Only);
		ExperimentalAttribute = new AttributeDescription("System.Diagnostics.CodeAnalysis", "ExperimentalAttribute", s_signatures_HasThis_Void_String_Only);
		ExcludeFromCodeCoverageAttribute = new AttributeDescription("System.Diagnostics.CodeAnalysis", "ExcludeFromCodeCoverageAttribute", s_signatures_HasThis_Void_Only);
		EnumeratorCancellationAttribute = new AttributeDescription("System.Runtime.CompilerServices", "EnumeratorCancellationAttribute", s_signatures_HasThis_Void_Only);
		SkipLocalsInitAttribute = new AttributeDescription("System.Runtime.CompilerServices", "SkipLocalsInitAttribute", s_signatures_HasThis_Void_Only);
		NativeIntegerAttribute = new AttributeDescription("System.Runtime.CompilerServices", "NativeIntegerAttribute", s_signaturesOfNativeIntegerAttribute);
		ScopedRefAttribute = new AttributeDescription("System.Runtime.CompilerServices", "ScopedRefAttribute", s_signatures_HasThis_Void_Only);
		RefSafetyRulesAttribute = new AttributeDescription("System.Runtime.CompilerServices", "RefSafetyRulesAttribute", s_signatures_HasThis_Void_Int32_Only);
		ModuleInitializerAttribute = new AttributeDescription("System.Runtime.CompilerServices", "ModuleInitializerAttribute", s_signatures_HasThis_Void_Only);
		UnmanagedCallersOnlyAttribute = new AttributeDescription("System.Runtime.InteropServices", "UnmanagedCallersOnlyAttribute", s_signatures_HasThis_Void_Only);
		InterpolatedStringHandlerAttribute = new AttributeDescription("System.Runtime.CompilerServices", "InterpolatedStringHandlerAttribute", s_signatures_HasThis_Void_Only);
		InterpolatedStringHandlerArgumentAttribute = new AttributeDescription("System.Runtime.CompilerServices", "InterpolatedStringHandlerArgumentAttribute", s_signaturesOfInterpolatedStringArgumentAttribute);
		RequiredMemberAttribute = new AttributeDescription("System.Runtime.CompilerServices", "RequiredMemberAttribute", s_signatures_HasThis_Void_Only);
		SetsRequiredMembersAttribute = new AttributeDescription("System.Diagnostics.CodeAnalysis", "SetsRequiredMembersAttribute", s_signatures_HasThis_Void_Only);
		CompilerFeatureRequiredAttribute = new AttributeDescription("System.Runtime.CompilerServices", "CompilerFeatureRequiredAttribute", s_signatures_HasThis_Void_String_Only);
		UnscopedRefAttribute = new AttributeDescription("System.Diagnostics.CodeAnalysis", "UnscopedRefAttribute", s_signatures_HasThis_Void_Only);
		InlineArrayAttribute = new AttributeDescription("System.Runtime.CompilerServices", "InlineArrayAttribute", s_signatures_HasThis_Void_Int32_Only);
		CollectionBuilderAttribute = new AttributeDescription("System.Runtime.CompilerServices", "CollectionBuilderAttribute", s_signaturesOfCollectionBuilderAttribute);
		OverloadResolutionPriorityAttribute = new AttributeDescription("System.Runtime.CompilerServices", "OverloadResolutionPriorityAttribute", s_signatures_HasThis_Void_Int32_Only);
		CompilerLoweringPreserveAttribute = new AttributeDescription("System.Runtime.CompilerServices", "CompilerLoweringPreserveAttribute", s_signatures_HasThis_Void_Only);
		ExtensionMarkerAttribute = new AttributeDescription("System.Runtime.CompilerServices", "ExtensionMarkerAttribute", s_signatures_HasThis_Void_String_Only);
		RuntimeAsyncMethodGenerationAttribute = new AttributeDescription("System.Runtime.CompilerServices", "RuntimeAsyncMethodGenerationAttribute", s_signatures_HasThis_Void_Boolean_Only);
		MetadataUpdateDeletedAttribute = new AttributeDescription("System.Runtime.CompilerServices", "MetadataUpdateDeletedAttribute", s_signatures_HasThis_Void_Only);
		TypeHandleTargets = new TypeHandleTargetInfo[19]
		{
			new TypeHandleTargetInfo("System", "AttributeTargets", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Reflection", "AssemblyNameFlags", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Runtime.CompilerServices", "MethodImplOptions", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Runtime.InteropServices", "CharSet", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Runtime.InteropServices", "LayoutKind", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Runtime.InteropServices", "UnmanagedType", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Runtime.InteropServices", "TypeLibTypeFlags", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Runtime.InteropServices", "ClassInterfaceType", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Runtime.InteropServices", "ComInterfaceType", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Runtime.CompilerServices", "CompilationRelaxations", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Diagnostics.DebuggableAttribute", "DebuggingModes", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Security", "SecurityCriticalScope", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Runtime.InteropServices", "CallingConvention", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Configuration.Assemblies", "AssemblyHashAlgorithm", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.EnterpriseServices", "TransactionOption", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Security.Permissions", "SecurityAction", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System", "Type", SerializationTypeCode.Type),
			new TypeHandleTargetInfo("Windows.Foundation.Metadata", "DeprecationType", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("Windows.Foundation.Metadata", "Platform", SerializationTypeCode.Int32)
		}.AsImmutable();
	}
}
