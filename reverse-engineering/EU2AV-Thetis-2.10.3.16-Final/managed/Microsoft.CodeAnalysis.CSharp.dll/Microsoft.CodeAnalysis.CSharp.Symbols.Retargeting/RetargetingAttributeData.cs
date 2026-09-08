using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Retargeting;

internal sealed class RetargetingAttributeData : CSharpAttributeData
{
	private readonly CSharpAttributeData _underlying;

	private readonly NamedTypeSymbol? _attributeClass;

	private readonly MethodSymbol? _attributeConstructor;

	private readonly ImmutableArray<TypedConstant> _constructorArguments;

	private readonly ImmutableArray<KeyValuePair<string, TypedConstant>> _namedArguments;

	public override NamedTypeSymbol? AttributeClass => _attributeClass;

	public override MethodSymbol? AttributeConstructor => _attributeConstructor;

	protected override ImmutableArray<TypedConstant> CommonConstructorArguments
	{
		protected internal get
		{
			return _constructorArguments;
		}
	}

	protected override ImmutableArray<KeyValuePair<string, TypedConstant>> CommonNamedArguments
	{
		protected internal get
		{
			return _namedArguments;
		}
	}

	public override SyntaxReference? ApplicationSyntaxReference => null;

	[MemberNotNullWhen(false, new string[] { "AttributeClass", "AttributeConstructor" })]
	internal override bool HasErrors
	{
		[MemberNotNullWhen(false, new string[] { "AttributeClass", "AttributeConstructor" })]
		get
		{
			if (!_underlying.HasErrors)
			{
				return (object)_attributeConstructor == null;
			}
			return true;
		}
	}

	internal override DiagnosticInfo? ErrorInfo
	{
		get
		{
			if (_underlying.HasErrors)
			{
				return _underlying.ErrorInfo;
			}
			if (HasErrors)
			{
				NamedTypeSymbol attributeClass = AttributeClass;
				if ((object)attributeClass != null && attributeClass.HasUseSiteError)
				{
					return AttributeClass.GetUseSiteInfo().DiagnosticInfo;
				}
				return new CSDiagnosticInfo(ErrorCode.ERR_MissingPredefinedMember, AttributeClass, ".ctor");
			}
			return null;
		}
	}

	internal override bool IsConditionallyOmitted => _underlying.IsConditionallyOmitted;

	internal RetargetingAttributeData(CSharpAttributeData underlying, NamedTypeSymbol? attributeClass, MethodSymbol? attributeConstructor, ImmutableArray<TypedConstant> constructorArguments, ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments)
	{
		_underlying = underlying;
		_attributeClass = attributeClass;
		_attributeConstructor = attributeConstructor;
		_constructorArguments = constructorArguments;
		_namedArguments = namedArguments;
	}

	internal override Location GetAttributeArgumentLocation(int parameterIndex)
	{
		return _underlying.GetAttributeArgumentLocation(parameterIndex);
	}

	internal override int GetTargetAttributeSignatureIndex(AttributeDescription description)
	{
		return _underlying.GetTargetAttributeSignatureIndex(description);
	}

	internal override bool IsTargetAttribute(string namespaceName, string typeName)
	{
		return _underlying.IsTargetAttribute(namespaceName, typeName);
	}
}
