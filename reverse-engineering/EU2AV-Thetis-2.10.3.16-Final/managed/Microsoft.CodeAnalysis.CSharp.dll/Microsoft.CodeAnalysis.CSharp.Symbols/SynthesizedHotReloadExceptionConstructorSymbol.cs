using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedHotReloadExceptionConstructorSymbol : SynthesizedInstanceConstructor
{
	private readonly ImmutableArray<ParameterSymbol> _parameters;

	public override ImmutableArray<ParameterSymbol> Parameters => _parameters;

	public ParameterSymbol MessageParameter => _parameters[0];

	public ParameterSymbol CodeParameter => _parameters[1];

	internal SynthesizedHotReloadExceptionConstructorSymbol(NamedTypeSymbol containingType, TypeSymbol stringType, TypeSymbol intType)
		: base(containingType)
	{
		_parameters = ImmutableCollectionsMarshal.AsImmutableArray(new ParameterSymbol[2]
		{
			SynthesizedParameterSymbol.Create(this, TypeWithAnnotations.Create(stringType), 0, RefKind.None),
			SynthesizedParameterSymbol.Create(this, TypeWithAnnotations.Create(intType), 1, RefKind.None)
		});
	}

	internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		SynthesizedHotReloadExceptionSymbol synthesizedHotReloadExceptionSymbol = (SynthesizedHotReloadExceptionSymbol)ContainingType;
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, this.GetNonNullSyntaxNode(), compilationState, diagnostics);
		syntheticBoundNodeFactory.CurrentFunction = this;
		MethodSymbol methodSymbol = (MethodSymbol)syntheticBoundNodeFactory.WellKnownMember(WellKnownMember.System_Exception__ctorString, isOptional: true);
		if ((object)methodSymbol == null)
		{
			diagnostics.Add(ErrorCode.ERR_EncUpdateFailedMissingSymbol, Location.None, CodeAnalysisResources.Constructor, "System.Exception..ctor(string)");
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.Block());
			return;
		}
		MethodSymbol methodSymbol2 = (synthesizedHotReloadExceptionSymbol.CreatedActionField.Type as NamedTypeSymbol)?.DelegateInvokeMethod;
		if ((object)methodSymbol2 != null && methodSymbol2.ReturnType.SpecialType == SpecialType.System_Void)
		{
			ImmutableArray<ParameterSymbol> parameters = methodSymbol2.GetParameters();
			if (parameters.Length == 1)
			{
				ParameterSymbol parameterSymbol = parameters[0];
				if ((object)parameterSymbol != null && parameterSymbol.RefKind == RefKind.None && parameterSymbol.Type.Equals(methodSymbol.ContainingType))
				{
					BoundLocal boundLocal = syntheticBoundNodeFactory.StoreToTemp(syntheticBoundNodeFactory.Field(null, synthesizedHotReloadExceptionSymbol.CreatedActionField), out BoundAssignmentOperator store);
					BoundBlock body = syntheticBoundNodeFactory.Block(ImmutableCollectionsMarshal.AsImmutableArray(new LocalSymbol[1] { boundLocal.LocalSymbol }), syntheticBoundNodeFactory.ExpressionStatement(syntheticBoundNodeFactory.Call(syntheticBoundNodeFactory.This(), methodSymbol, syntheticBoundNodeFactory.Parameter(MessageParameter))), syntheticBoundNodeFactory.Assignment(syntheticBoundNodeFactory.Field(syntheticBoundNodeFactory.This(), synthesizedHotReloadExceptionSymbol.CodeField), syntheticBoundNodeFactory.Parameter(CodeParameter)), syntheticBoundNodeFactory.If(syntheticBoundNodeFactory.IsNotNullReference(store), syntheticBoundNodeFactory.ExpressionStatement(syntheticBoundNodeFactory.Call(boundLocal, methodSymbol2, syntheticBoundNodeFactory.This()))), syntheticBoundNodeFactory.Return());
					syntheticBoundNodeFactory.CloseMethod(body);
					return;
				}
			}
		}
		diagnostics.Add(ErrorCode.ERR_EncUpdateFailedMissingSymbol, Location.None, CodeAnalysisResources.Method, "void System.Action<T>.Invoke(T arg)");
		syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.Block());
	}
}
