using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class LambdaCapturedVariable : SynthesizedFieldSymbolBase, ISynthesizedMethodBodyImplementationSymbol, ISymbolInternal
{
	private readonly TypeWithAnnotations _type;

	private readonly bool _isThis;

	public SynthesizedClosureEnvironment Frame => (SynthesizedClosureEnvironment)ContainingType;

	public override RefKind RefKind => RefKind.None;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	internal override bool IsCapturedFrame => _isThis;

	internal override bool SuppressDynamicAttribute => false;

	public IMethodSymbolInternal Method => Frame.TopLevelMethod;

	public bool HasMethodBodyDependency => false;

	protected LambdaCapturedVariable(SynthesizedClosureEnvironment frame, TypeWithAnnotations type, string fieldName, bool isThisParameter)
		: base(frame, fieldName, DeclarationModifiers.Public, isReadOnly: false, isStatic: false)
	{
		_type = type;
		_isThis = isThisParameter;
	}

	public static LambdaCapturedVariable Create(SynthesizedClosureEnvironment frame, Symbol captured, ref int uniqueId)
	{
		string capturedVariableFieldName = GetCapturedVariableFieldName(captured, ref uniqueId);
		TypeSymbol capturedVariableFieldType = GetCapturedVariableFieldType(frame, captured);
		bool flag = IsThis(captured, out ParameterSymbol parameter);
		if ((object)parameter == null || flag)
		{
			return new LambdaCapturedVariable(frame, TypeWithAnnotations.Create(capturedVariableFieldType), capturedVariableFieldName, flag);
		}
		return new LambdaCapturedVariableForRegularParameter(frame, TypeWithAnnotations.Create(capturedVariableFieldType), capturedVariableFieldName, parameter);
	}

	private static bool IsThis(Symbol captured)
	{
		ParameterSymbol parameter;
		return IsThis(captured, out parameter);
	}

	private static bool IsThis(Symbol captured, out ParameterSymbol? parameter)
	{
		parameter = captured as ParameterSymbol;
		if ((object)parameter != null)
		{
			return parameter.IsThis;
		}
		return false;
	}

	private static string GetCapturedVariableFieldName(Symbol variable, ref int uniqueId)
	{
		if (IsThis(variable))
		{
			return GeneratedNames.ThisProxyFieldName();
		}
		if (variable is LocalSymbol { SynthesizedKind: var synthesizedKind } localSymbol)
		{
			switch (synthesizedKind)
			{
			case SynthesizedLocalKind.LambdaDisplayClass:
				return GeneratedNames.MakeLambdaDisplayLocalName(uniqueId++);
			case SynthesizedLocalKind.TryAwaitPendingException:
			case SynthesizedLocalKind.TryAwaitPendingCaughtException:
			case SynthesizedLocalKind.ExceptionFilterAwaitHoistedExceptionLocal:
				return GeneratedNames.MakeHoistedLocalFieldName(localSymbol.SynthesizedKind, uniqueId++);
			case SynthesizedLocalKind.InstrumentationPayload:
				return GeneratedNames.MakeSynthesizedInstrumentationPayloadLocalFieldName(uniqueId++);
			}
			if (localSymbol.SynthesizedKind == SynthesizedLocalKind.UserDefined)
			{
				SyntaxNode scopeDesignatorOpt = localSymbol.ScopeDesignatorOpt;
				if (scopeDesignatorOpt == null || scopeDesignatorOpt.Kind() != SyntaxKind.SwitchSection)
				{
					SyntaxNode scopeDesignatorOpt2 = localSymbol.ScopeDesignatorOpt;
					if (scopeDesignatorOpt2 == null || scopeDesignatorOpt2.Kind() != SyntaxKind.SwitchExpressionArm)
					{
						goto IL_00ce;
					}
				}
				return GeneratedNames.MakeHoistedLocalFieldName(localSymbol.SynthesizedKind, uniqueId++, localSymbol.Name);
			}
		}
		goto IL_00ce;
		IL_00ce:
		return variable.Name;
	}

	private static TypeSymbol GetCapturedVariableFieldType(SynthesizedContainer frame, Symbol variable)
	{
		LocalSymbol localSymbol = variable as LocalSymbol;
		if ((object)localSymbol != null && localSymbol.Type.OriginalDefinition is SynthesizedClosureEnvironment synthesizedClosureEnvironment)
		{
			ImmutableArray<TypeWithAnnotations> immutableArray = frame.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics;
			if (immutableArray.Length > synthesizedClosureEnvironment.Arity)
			{
				immutableArray = ImmutableArray.Create(immutableArray, 0, synthesizedClosureEnvironment.Arity);
			}
			return synthesizedClosureEnvironment.ConstructIfGeneric(immutableArray);
		}
		return frame.TypeMap.SubstituteType((localSymbol?.TypeWithAnnotations ?? ((ParameterSymbol)variable).TypeWithAnnotations).Type).Type;
	}

	internal override TypeWithAnnotations GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		return _type;
	}
}
