using System.Collections.Immutable;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SignatureOnlyParameterSymbol : ParameterSymbol
{
	private readonly TypeWithAnnotations _type;

	private readonly ImmutableArray<CustomModifier> _refCustomModifiers;

	private readonly bool _isParamsArray;

	private readonly bool _isParamsCollection;

	private readonly RefKind _refKind;

	public override TypeWithAnnotations TypeWithAnnotations => _type;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => _refCustomModifiers;

	internal override bool HasEnumeratorCancellationAttribute
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 44);
		}
	}

	public override bool IsParamsArray => _isParamsArray;

	public override bool IsParamsCollection => _isParamsCollection;

	public override RefKind RefKind => _refKind;

	public override string Name => "";

	public override bool IsImplicitlyDeclared => true;

	public override bool IsDiscard => false;

	internal override ScopedKind DeclaredScope
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 58);
		}
	}

	internal override ScopedKind EffectiveScope
	{
		get
		{
			if (!ParameterHelpers.IsRefScopedByDefault(this))
			{
				return ScopedKind.None;
			}
			return ScopedKind.ScopedRef;
		}
	}

	internal override bool HasUnscopedRefAttribute => false;

	internal override bool UseUpdatedEscapeRules => false;

	internal override bool IsMetadataIn
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 69);
		}
	}

	internal override bool IsMetadataOut
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 71);
		}
	}

	internal override MarshalPseudoCustomAttributeData MarshallingInformation
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 73);
		}
	}

	public override int Ordinal
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 75);
		}
	}

	internal override bool IsMetadataOptional
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 77);
		}
	}

	internal override ConstantValue ExplicitDefaultConstantValue
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 79);
		}
	}

	internal override ConstantValue DefaultValueFromAttributes
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 81);
		}
	}

	internal override bool IsIDispatchConstant
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 83);
		}
	}

	internal override bool IsIUnknownConstant
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 85);
		}
	}

	internal override bool IsCallerFilePath
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 87);
		}
	}

	internal override bool IsCallerLineNumber
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 89);
		}
	}

	internal override bool IsCallerMemberName
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 91);
		}
	}

	internal override int CallerArgumentExpressionParameterIndex
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 93);
		}
	}

	internal override FlowAnalysisAnnotations FlowAnalysisAnnotations
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 95);
		}
	}

	internal override ImmutableHashSet<string> NotNullIfParameterNotNull
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 97);
		}
	}

	public override Symbol ContainingSymbol
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 99);
		}
	}

	public override ImmutableArray<Location> Locations
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 101);
		}
	}

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 103);
		}
	}

	public override AssemblySymbol ContainingAssembly
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 105);
		}
	}

	internal override ModuleSymbol ContainingModule
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 107);
		}
	}

	internal override ImmutableArray<int> InterpolatedStringHandlerArgumentIndexes
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 109);
		}
	}

	internal override bool HasInterpolatedStringHandlerArgumentError
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SignatureOnlyParameterSymbol.cs", 111);
		}
	}

	public SignatureOnlyParameterSymbol(TypeWithAnnotations type, ImmutableArray<CustomModifier> refCustomModifiers, bool isParamsArray, bool isParamsCollection, RefKind refKind)
	{
		_type = type;
		_refCustomModifiers = refCustomModifiers;
		_isParamsArray = isParamsArray;
		_isParamsCollection = isParamsCollection;
		_refKind = refKind;
	}

	public override bool Equals(Symbol obj, TypeCompareKind compareKind)
	{
		if ((object)this == obj)
		{
			return true;
		}
		if (obj is SignatureOnlyParameterSymbol signatureOnlyParameterSymbol && TypeSymbol.Equals(_type.Type, signatureOnlyParameterSymbol._type.Type, compareKind) && _type.CustomModifiers.Equals(signatureOnlyParameterSymbol._type.CustomModifiers) && _refCustomModifiers.SequenceEqual(signatureOnlyParameterSymbol._refCustomModifiers) && _isParamsArray == signatureOnlyParameterSymbol._isParamsArray && _isParamsCollection == signatureOnlyParameterSymbol._isParamsCollection)
		{
			return _refKind == signatureOnlyParameterSymbol._refKind;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int hashCode = _type.Type.GetHashCode();
		int newKey = Hash.CombineValues(_type.CustomModifiers);
		int hashCode2 = (_isParamsArray || _isParamsCollection).GetHashCode();
		int refKind = (int)_refKind;
		return Hash.Combine(hashCode, Hash.Combine(newKey, Hash.Combine(hashCode2, refKind.GetHashCode())));
	}
}
