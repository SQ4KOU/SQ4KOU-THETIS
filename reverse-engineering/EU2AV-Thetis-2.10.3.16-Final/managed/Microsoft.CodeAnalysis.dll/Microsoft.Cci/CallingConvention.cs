using System;

namespace Microsoft.Cci;

[Flags]
internal enum CallingConvention
{
	CDecl = 1,
	Default = 0,
	ExtraArguments = 5,
	FastCall = 4,
	Standard = 2,
	ThisCall = CDecl | Standard,
	Unmanaged = 9,
	Generic = 0x10,
	HasThis = 0x20,
	ExplicitThis = 0x40
}
