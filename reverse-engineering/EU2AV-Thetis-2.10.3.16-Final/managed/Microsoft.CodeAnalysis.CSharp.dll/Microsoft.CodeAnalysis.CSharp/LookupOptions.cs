using System;

namespace Microsoft.CodeAnalysis.CSharp;

[Flags]
internal enum LookupOptions
{
	Default = 0,
	NamespaceAliasesOnly = 2,
	NamespacesOrTypesOnly = 4,
	MustBeInvocableIfMember = 8,
	MustBeInstance = 0x10,
	MustNotBeInstance = 0x20,
	MustNotBeNamespace = 0x40,
	AllMethodsOnArityZero = 0x80,
	LabelsOnly = 0x100,
	UseBaseReferenceAccessibility = 0x200,
	AttributeTypeOnly = 0x804,
	VerbatimNameAttributeTypeOnly = 0x1804,
	AllNamedTypesOnArityZero = 0x2000,
	MustNotBeMethodTypeParameter = 0x4000,
	MustBeAbstractOrVirtual = 0x8000,
	MustNotBeParameter = 0x10000,
	MustBeOperator = 0x20000
}
