using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceAttributeData : CSharpAttributeData
{
	private readonly CSharpCompilation _compilation;

	private readonly NamedTypeSymbol _attributeClass;

	private readonly MethodSymbol? _attributeConstructor;

	private readonly ImmutableArray<TypedConstant> _constructorArguments;

	private readonly ImmutableArray<int> _constructorArgumentsSourceIndices;

	private readonly ImmutableArray<KeyValuePair<string, TypedConstant>> _namedArguments;

	private readonly bool _isConditionallyOmitted;

	private readonly bool _hasErrors;

	private readonly SyntaxReference _applicationNode;

	public override NamedTypeSymbol AttributeClass => _attributeClass;

	public override MethodSymbol? AttributeConstructor => _attributeConstructor;

	public override SyntaxReference ApplicationSyntaxReference => _applicationNode;

	internal ImmutableArray<int> ConstructorArgumentsSourceIndices => _constructorArgumentsSourceIndices;

	internal override bool IsConditionallyOmitted => _isConditionallyOmitted;

	[MemberNotNullWhen(false, "AttributeConstructor")]
	internal override bool HasErrors
	{
		[MemberNotNullWhen(false, "AttributeConstructor")]
		get
		{
			return _hasErrors;
		}
	}

	internal override DiagnosticInfo? ErrorInfo => null;

	protected sealed override ImmutableArray<TypedConstant> CommonConstructorArguments
	{
		protected internal get
		{
			return _constructorArguments;
		}
	}

	protected sealed override ImmutableArray<KeyValuePair<string, TypedConstant>> CommonNamedArguments
	{
		protected internal get
		{
			return _namedArguments;
		}
	}

	private SourceAttributeData(CSharpCompilation compilation, SyntaxReference applicationNode, NamedTypeSymbol attributeClass, MethodSymbol? attributeConstructor, ImmutableArray<TypedConstant> constructorArguments, ImmutableArray<int> constructorArgumentsSourceIndices, ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments, bool hasErrors, bool isConditionallyOmitted)
	{
		_compilation = compilation;
		_attributeClass = attributeClass;
		_attributeConstructor = attributeConstructor;
		_constructorArguments = constructorArguments;
		_constructorArgumentsSourceIndices = constructorArgumentsSourceIndices;
		_namedArguments = namedArguments;
		_isConditionallyOmitted = isConditionallyOmitted;
		_hasErrors = hasErrors;
		_applicationNode = applicationNode;
	}

	internal SourceAttributeData(CSharpCompilation compilation, AttributeSyntax attributeSyntax, NamedTypeSymbol attributeClass, MethodSymbol? attributeConstructor, bool hasErrors)
		: this(compilation, attributeSyntax, attributeClass, attributeConstructor, ImmutableArray<TypedConstant>.Empty, default(ImmutableArray<int>), ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty, hasErrors, isConditionallyOmitted: false)
	{
	}

	internal SourceAttributeData(CSharpCompilation compilation, AttributeSyntax attributeSyntax, NamedTypeSymbol attributeClass, MethodSymbol? attributeConstructor, ImmutableArray<TypedConstant> constructorArguments, ImmutableArray<int> constructorArgumentsSourceIndices, ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments, bool hasErrors, bool isConditionallyOmitted)
		: this(compilation, attributeSyntax.GetReference(), attributeClass, attributeConstructor, constructorArguments, constructorArgumentsSourceIndices, namedArguments, hasErrors, isConditionallyOmitted)
	{
	}

	internal CSharpSyntaxNode GetAttributeArgumentSyntax(int parameterIndex)
	{
		AttributeSyntax attributeSyntax = (AttributeSyntax)_applicationNode.GetSyntax();
		if (_constructorArgumentsSourceIndices.IsDefault)
		{
			return attributeSyntax.ArgumentList.Arguments[parameterIndex];
		}
		int num = _constructorArgumentsSourceIndices[parameterIndex];
		if (num == -1)
		{
			return attributeSyntax.Name;
		}
		return attributeSyntax.ArgumentList.Arguments[num];
	}

	internal override Location GetAttributeArgumentLocation(int parameterIndex)
	{
		return GetAttributeArgumentSyntax(parameterIndex).Location;
	}

	internal SourceAttributeData WithOmittedCondition(bool isConditionallyOmitted)
	{
		if (IsConditionallyOmitted == isConditionallyOmitted)
		{
			return this;
		}
		return new SourceAttributeData(_compilation, ApplicationSyntaxReference, AttributeClass, AttributeConstructor, CommonConstructorArguments, ConstructorArgumentsSourceIndices, CommonNamedArguments, HasErrors, isConditionallyOmitted);
	}

	internal override int GetTargetAttributeSignatureIndex(AttributeDescription description)
	{
		return GetTargetAttributeSignatureIndex(_compilation, AttributeClass, AttributeConstructor, description);
	}

	internal static int GetTargetAttributeSignatureIndex(CSharpCompilation compilation, NamedTypeSymbol attributeClass, MethodSymbol? attributeConstructor, AttributeDescription description)
	{
		if (!IsTargetAttribute(attributeClass, description.Namespace, description.Name))
		{
			return -1;
		}
		if ((object)attributeConstructor == null)
		{
			return -1;
		}
		TypeSymbol lazySystemType = null;
		ImmutableArray<ParameterSymbol> parameters = attributeConstructor.Parameters;
		for (int i = 0; i < description.Signatures.Length; i++)
		{
			if (matches(description.Signatures[i], parameters, ref lazySystemType))
			{
				return i;
			}
		}
		return -1;
		bool matches(byte[] targetSignature, ImmutableArray<ParameterSymbol> immutableArray, ref TypeSymbol? reference)
		{
			if (targetSignature[0] != 32)
			{
				return false;
			}
			if (targetSignature[1] != immutableArray.Length)
			{
				return false;
			}
			if (targetSignature[2] != 1)
			{
				return false;
			}
			int num = 0;
			for (int j = 3; j < targetSignature.Length; j++)
			{
				if (num >= immutableArray.Length)
				{
					return false;
				}
				TypeSymbol type = immutableArray[num].Type;
				SpecialType specialType = type.SpecialType;
				byte b = targetSignature[j];
				switch (b)
				{
				case 64:
				{
					j++;
					if (type.Kind != SymbolKind.NamedType && type.Kind != SymbolKind.ErrorType)
					{
						return false;
					}
					NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)type;
					AttributeDescription.TypeHandleTargetInfo typeHandleTargetInfo = AttributeDescription.TypeHandleTargets[targetSignature[j]];
					if (!string.Equals(namedTypeSymbol.MetadataName, typeHandleTargetInfo.Name, StringComparison.Ordinal) || !namedTypeSymbol.HasNameQualifier(typeHandleTargetInfo.Namespace))
					{
						return false;
					}
					b = (byte)typeHandleTargetInfo.Underlying;
					if (type.IsEnumType())
					{
						specialType = type.GetEnumUnderlyingType().SpecialType;
					}
					break;
				}
				default:
					if (type.IsArray())
					{
						if (targetSignature[j - 1] != 29)
						{
							return false;
						}
						specialType = ((ArrayTypeSymbol)type).ElementType.SpecialType;
					}
					break;
				case 29:
					break;
				}
				switch (b)
				{
				case 2:
					if (specialType != SpecialType.System_Boolean)
					{
						return false;
					}
					num++;
					break;
				case 3:
					if (specialType != SpecialType.System_Char)
					{
						return false;
					}
					num++;
					break;
				case 4:
					if (specialType != SpecialType.System_SByte)
					{
						return false;
					}
					num++;
					break;
				case 5:
					if (specialType != SpecialType.System_Byte)
					{
						return false;
					}
					num++;
					break;
				case 6:
					if (specialType != SpecialType.System_Int16)
					{
						return false;
					}
					num++;
					break;
				case 7:
					if (specialType != SpecialType.System_UInt16)
					{
						return false;
					}
					num++;
					break;
				case 8:
					if (specialType != SpecialType.System_Int32)
					{
						return false;
					}
					num++;
					break;
				case 9:
					if (specialType != SpecialType.System_UInt32)
					{
						return false;
					}
					num++;
					break;
				case 10:
					if (specialType != SpecialType.System_Int64)
					{
						return false;
					}
					num++;
					break;
				case 11:
					if (specialType != SpecialType.System_UInt64)
					{
						return false;
					}
					num++;
					break;
				case 12:
					if (specialType != SpecialType.System_Single)
					{
						return false;
					}
					num++;
					break;
				case 13:
					if (specialType != SpecialType.System_Double)
					{
						return false;
					}
					num++;
					break;
				case 14:
					if (specialType != SpecialType.System_String)
					{
						return false;
					}
					num++;
					break;
				case 28:
					if (specialType != SpecialType.System_Object)
					{
						return false;
					}
					num++;
					break;
				case 80:
					if ((object)reference == null)
					{
						reference = compilation.GetWellKnownType(WellKnownType.System_Type);
					}
					if (!TypeSymbol.Equals(type, reference, TypeCompareKind.ConsiderEverything))
					{
						return false;
					}
					num++;
					break;
				case 29:
					if (!type.IsArray())
					{
						return false;
					}
					break;
				default:
					return false;
				}
			}
			return true;
		}
	}

	internal override bool IsTargetAttribute(string namespaceName, string typeName)
	{
		return IsTargetAttribute(AttributeClass, namespaceName, typeName);
	}

	internal static bool IsTargetAttribute(NamedTypeSymbol attributeClass, string namespaceName, string typeName)
	{
		if (!attributeClass.Name.Equals(typeName))
		{
			return false;
		}
		if (attributeClass.IsErrorType() && !(attributeClass is MissingMetadataTypeSymbol))
		{
			return false;
		}
		return attributeClass.HasNameQualifier(namespaceName);
	}
}
