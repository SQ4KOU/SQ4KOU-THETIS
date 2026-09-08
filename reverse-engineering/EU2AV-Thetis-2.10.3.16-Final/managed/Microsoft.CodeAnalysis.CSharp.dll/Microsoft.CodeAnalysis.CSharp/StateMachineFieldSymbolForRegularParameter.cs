using System;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class StateMachineFieldSymbolForRegularParameter : StateMachineFieldSymbol
{
	private readonly ParameterSymbol _parameter;

	public StateMachineFieldSymbolForRegularParameter(NamedTypeSymbol stateMachineType, TypeWithAnnotations type, string name, ParameterSymbol parameter, bool isPublic)
		: base(stateMachineType, type, name, isPublic, isThis: false)
	{
		_parameter = parameter;
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		ParameterSymbol originalDefinition = _parameter.OriginalDefinition;
		if (ContainingModule == originalDefinition.ContainingModule)
		{
			foreach (CSharpAttributeData attribute in originalDefinition.GetAttributes())
			{
				NamedTypeSymbol attributeClass = attribute.AttributeClass;
				if ((object)attributeClass != null && attributeClass.HasCompilerLoweringPreserveAttribute && (attributeClass.GetAttributeUsageInfo().ValidTargets & AttributeTargets.Field) != 0)
				{
					Symbol.AddSynthesizedAttribute(ref attributes, attribute);
				}
			}
		}
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
	}
}
