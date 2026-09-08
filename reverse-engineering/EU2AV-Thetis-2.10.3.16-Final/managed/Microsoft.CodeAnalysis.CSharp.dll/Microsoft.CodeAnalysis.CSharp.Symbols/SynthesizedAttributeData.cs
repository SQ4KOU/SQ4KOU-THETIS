using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SynthesizedAttributeData : CSharpAttributeData
{
	private sealed class FromMethodAndArguments : SynthesizedAttributeData
	{
		private readonly CSharpCompilation _compilation;

		private readonly MethodSymbol _wellKnownMember;

		private readonly ImmutableArray<TypedConstant> _arguments;

		private readonly ImmutableArray<KeyValuePair<string, TypedConstant>> _namedArguments;

		public override SyntaxReference? ApplicationSyntaxReference => null;

		public override NamedTypeSymbol AttributeClass => _wellKnownMember.ContainingType;

		public override MethodSymbol AttributeConstructor => _wellKnownMember;

		protected override ImmutableArray<TypedConstant> CommonConstructorArguments
		{
			protected internal get
			{
				return _arguments;
			}
		}

		protected override ImmutableArray<KeyValuePair<string, TypedConstant>> CommonNamedArguments
		{
			protected internal get
			{
				return _namedArguments;
			}
		}

		internal override bool HasErrors => false;

		internal override DiagnosticInfo? ErrorInfo => null;

		internal override bool IsConditionallyOmitted => false;

		internal FromMethodAndArguments(CSharpCompilation compilation, MethodSymbol wellKnownMember, ImmutableArray<TypedConstant> arguments, ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments)
		{
			_compilation = compilation;
			_wellKnownMember = wellKnownMember;
			_arguments = arguments;
			_namedArguments = namedArguments;
		}

		internal override Location GetAttributeArgumentLocation(int parameterIndex)
		{
			return NoLocation.Singleton;
		}

		internal override int GetTargetAttributeSignatureIndex(AttributeDescription description)
		{
			return SourceAttributeData.GetTargetAttributeSignatureIndex(_compilation, AttributeClass, AttributeConstructor, description);
		}

		internal override bool IsTargetAttribute(string namespaceName, string typeName)
		{
			return SourceAttributeData.IsTargetAttribute(AttributeClass, namespaceName, typeName);
		}
	}

	private sealed class FromSourceAttributeData : SynthesizedAttributeData
	{
		private readonly SourceAttributeData _original;

		public override SyntaxReference? ApplicationSyntaxReference => _original.ApplicationSyntaxReference;

		public override NamedTypeSymbol AttributeClass => _original.AttributeClass;

		public override MethodSymbol? AttributeConstructor => _original.AttributeConstructor;

		protected override ImmutableArray<TypedConstant> CommonConstructorArguments
		{
			protected internal get
			{
				return _original.CommonConstructorArguments;
			}
		}

		protected override ImmutableArray<KeyValuePair<string, TypedConstant>> CommonNamedArguments
		{
			protected internal get
			{
				return _original.CommonNamedArguments;
			}
		}

		[MemberNotNullWhen(false, "AttributeConstructor")]
		internal override bool HasErrors
		{
			[MemberNotNullWhen(false, "AttributeConstructor")]
			get
			{
				return _original.HasErrors;
			}
		}

		internal override DiagnosticInfo? ErrorInfo => _original.ErrorInfo;

		internal override bool IsConditionallyOmitted => _original.IsConditionallyOmitted;

		internal FromSourceAttributeData(SourceAttributeData original)
		{
			_original = original;
		}

		internal override Location GetAttributeArgumentLocation(int parameterIndex)
		{
			return _original.GetAttributeArgumentLocation(parameterIndex);
		}

		internal override int GetTargetAttributeSignatureIndex(AttributeDescription description)
		{
			return _original.GetTargetAttributeSignatureIndex(description);
		}

		internal override bool IsTargetAttribute(string namespaceName, string typeName)
		{
			return _original.IsTargetAttribute(namespaceName, typeName);
		}
	}

	public static SynthesizedAttributeData Create(CSharpCompilation compilation, MethodSymbol wellKnownMember, ImmutableArray<TypedConstant> arguments, ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments)
	{
		return new FromMethodAndArguments(compilation, wellKnownMember, arguments, namedArguments);
	}

	public static SynthesizedAttributeData Create(SourceAttributeData original)
	{
		return new FromSourceAttributeData(original);
	}
}
