using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SynthesizedMethodBaseSymbol : SourceMemberMethodSymbol
{
	protected readonly MethodSymbol BaseMethod;

	private readonly string _name;

	private ImmutableArray<TypeParameterSymbol> _typeParameters;

	private ImmutableArray<ParameterSymbol> _parameters;

	internal TypeMap TypeMap { get; private set; }

	public sealed override ImmutableArray<TypeParameterSymbol> TypeParameters => _typeParameters;

	internal override int ParameterCount => Parameters.Length;

	public sealed override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			if (_parameters.IsDefault)
			{
				ImmutableInterlocked.InterlockedInitialize(ref _parameters, MakeParameters());
			}
			return _parameters;
		}
	}

	protected virtual ImmutableArray<TypeSymbol> ExtraSynthesizedRefParameters => default(ImmutableArray<TypeSymbol>);

	protected virtual ImmutableArray<ParameterSymbol> BaseMethodParameters => BaseMethod.Parameters;

	internal virtual bool InheritsBaseMethodAttributes => false;

	internal sealed override MethodImplAttributes ImplementationAttributes
	{
		get
		{
			if (!InheritsBaseMethodAttributes)
			{
				return MethodImplAttributes.IL;
			}
			return BaseMethod.ImplementationAttributes;
		}
	}

	internal sealed override MarshalPseudoCustomAttributeData? ReturnValueMarshallingInformation
	{
		get
		{
			if (!InheritsBaseMethodAttributes)
			{
				return null;
			}
			return BaseMethod.ReturnValueMarshallingInformation;
		}
	}

	internal sealed override bool HasSpecialName
	{
		get
		{
			if (InheritsBaseMethodAttributes)
			{
				return BaseMethod.HasSpecialName;
			}
			return false;
		}
	}

	public sealed override bool AreLocalsZeroed
	{
		get
		{
			if (BaseMethod is SourceMethodSymbol sourceMethodSymbol)
			{
				return sourceMethodSymbol.AreLocalsZeroed;
			}
			return true;
		}
	}

	internal sealed override bool RequiresSecurityObject
	{
		get
		{
			if (InheritsBaseMethodAttributes)
			{
				return BaseMethod.RequiresSecurityObject;
			}
			return false;
		}
	}

	internal sealed override bool HasDeclarativeSecurity
	{
		get
		{
			if (InheritsBaseMethodAttributes)
			{
				return BaseMethod.HasDeclarativeSecurity;
			}
			return false;
		}
	}

	public sealed override TypeWithAnnotations ReturnTypeWithAnnotations => TypeMap.SubstituteType(BaseMethod.OriginalDefinition.ReturnTypeWithAnnotations);

	public sealed override ImmutableArray<CustomModifier> RefCustomModifiers => TypeMap.SubstituteCustomModifiers(BaseMethod.OriginalDefinition.RefCustomModifiers);

	public sealed override FlowAnalysisAnnotations ReturnTypeFlowAnalysisAnnotations => BaseMethod.ReturnTypeFlowAnalysisAnnotations;

	public sealed override ImmutableHashSet<string> ReturnNotNullIfParameterNotNull => BaseMethod.ReturnNotNullIfParameterNotNull;

	public sealed override FlowAnalysisAnnotations FlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

	public sealed override string Name => _name;

	public sealed override bool IsImplicitlyDeclared => true;

	protected SynthesizedMethodBaseSymbol(NamedTypeSymbol containingType, MethodSymbol baseMethod, SyntaxReference syntaxReference, Location location, string name, DeclarationModifiers declarationModifiers, bool isIterator)
		: base(containingType, syntaxReference, location, isIterator, (declarationModifiers: declarationModifiers, flags: SourceMemberMethodSymbol.MakeFlags(MethodKind.Ordinary, baseMethod.RefKind, declarationModifiers, baseMethod.ReturnsVoid, returnsVoidIsSet: true, isExpressionBodied: false, isExtensionMethod: false, isNullableAnalysisEnabled: false, baseMethod.IsVararg, isExplicitInterfaceImplementation: false, hasThisInitializer: false)))
	{
		BaseMethod = baseMethod;
		_name = name;
	}

	protected void AssignTypeMapAndTypeParameters(TypeMap typeMap, ImmutableArray<TypeParameterSymbol> typeParameters)
	{
		TypeMap = typeMap;
		_typeParameters = typeParameters;
	}

	protected override void MethodChecks(BindingDiagnosticBag diagnostics)
	{
	}

	public sealed override ImmutableArray<ImmutableArray<TypeWithAnnotations>> GetTypeParameterConstraintTypes()
	{
		return ImmutableArray<ImmutableArray<TypeWithAnnotations>>.Empty;
	}

	public sealed override ImmutableArray<TypeParameterConstraintKind> GetTypeParameterConstraintKinds()
	{
		return ImmutableArray<TypeParameterConstraintKind>.Empty;
	}

	private ImmutableArray<ParameterSymbol> MakeParameters()
	{
		int num = 0;
		ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance();
		ImmutableArray<ParameterSymbol> baseMethodParameters = BaseMethodParameters;
		bool inheritsBaseMethodAttributes = InheritsBaseMethodAttributes;
		foreach (ParameterSymbol item in baseMethodParameters)
		{
			instance.Add(SynthesizedParameterSymbol.Create(this, TypeMap.SubstituteType(item.OriginalDefinition.TypeWithAnnotations), num++, item.RefKind, item.Name, item.EffectiveScope, item.ExplicitDefaultConstantValue, default(ImmutableArray<CustomModifier>), inheritsBaseMethodAttributes ? item : null, this is SynthesizedClosureMethod && item.IsParams));
		}
		ImmutableArray<TypeSymbol> extraSynthesizedRefParameters = ExtraSynthesizedRefParameters;
		if (!extraSynthesizedRefParameters.IsDefaultOrEmpty)
		{
			foreach (TypeSymbol item2 in extraSynthesizedRefParameters)
			{
				instance.Add(SynthesizedParameterSymbol.Create(this, TypeMap.SubstituteType(item2), num++, RefKind.Ref));
			}
		}
		return instance.ToImmutableAndFree();
	}

	public sealed override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		if (!InheritsBaseMethodAttributes)
		{
			return ImmutableArray<CSharpAttributeData>.Empty;
		}
		return BaseMethod.GetAttributes();
	}

	public sealed override ImmutableArray<CSharpAttributeData> GetReturnTypeAttributes()
	{
		if (!InheritsBaseMethodAttributes)
		{
			return ImmutableArray<CSharpAttributeData>.Empty;
		}
		return BaseMethod.GetReturnTypeAttributes();
	}

	public sealed override DllImportData? GetDllImportData()
	{
		if (!InheritsBaseMethodAttributes)
		{
			return null;
		}
		return BaseMethod.GetDllImportData();
	}

	internal sealed override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		if (!InheritsBaseMethodAttributes)
		{
			return SpecializedCollections.EmptyEnumerable<SecurityAttribute>();
		}
		return BaseMethod.GetSecurityInformation();
	}
}
