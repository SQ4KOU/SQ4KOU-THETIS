namespace Microsoft.CodeAnalysis;

public enum RuntimeCapability
{
	ByRefFields = 1,
	CovariantReturnsOfClasses,
	DefaultImplementationsOfInterfaces,
	NumericIntPtr,
	UnmanagedSignatureCallingConvention,
	VirtualStaticsInInterfaces,
	InlineArrayTypes,
	ByRefLikeGenerics,
	RuntimeAsyncMethods
}
