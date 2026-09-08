using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class RefSafetyAnalysis : BoundTreeWalkerWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator
{
	private enum EscapeLevel
	{
		CallingMethod,
		ReturnOnly
	}

	private readonly struct MethodInfo
	{
		internal Symbol Symbol { get; }

		internal MethodSymbol? Method { get; }

		internal MethodSymbol? SetMethod { get; }

		internal bool UseUpdatedEscapeRules => Method?.UseUpdatedEscapeRules ?? false;

		internal bool ReturnsRefToRefStruct
		{
			get
			{
				MethodSymbol method = Method;
				if ((object)method != null && method.RefKind != RefKind.None)
				{
					TypeSymbol returnType = method.ReturnType;
					if ((object)returnType != null)
					{
						return returnType.IsRefLikeOrAllowsRefLikeType();
					}
				}
				return false;
			}
		}

		private MethodInfo(Symbol symbol, MethodSymbol? method, MethodSymbol? setMethod)
		{
			Symbol = symbol;
			Method = method;
			SetMethod = setMethod;
		}

		internal static MethodInfo Create(MethodSymbol method)
		{
			return new MethodInfo(method, method, null);
		}

		internal static MethodInfo Create(PropertySymbol property)
		{
			return new MethodInfo(property, property.GetOwnOrInheritedGetMethod() ?? property.GetOwnOrInheritedSetMethod(), null);
		}

		internal static MethodInfo Create(PropertySymbol property, AccessorKind accessorKind)
		{
			return accessorKind switch
			{
				AccessorKind.Get => new MethodInfo(property, property.GetOwnOrInheritedGetMethod(), null), 
				AccessorKind.Set => new MethodInfo(property, property.GetOwnOrInheritedSetMethod(), null), 
				AccessorKind.Both => new MethodInfo(property, property.GetOwnOrInheritedGetMethod(), property.GetOwnOrInheritedSetMethod()), 
				_ => throw ExceptionUtilities.UnexpectedValue(accessorKind), 
			};
		}

		internal static MethodInfo Create(BoundIndexerAccess expr)
		{
			return Create(expr.Indexer, expr.AccessorKind);
		}

		internal MethodInfo ReplaceWithExtensionImplementation(out bool wasError)
		{
			MethodSymbol methodSymbol = replace(Method);
			MethodSymbol methodSymbol2 = replace(SetMethod);
			Symbol symbol = (((object)Symbol == Method && (object)methodSymbol != null) ? methodSymbol : Symbol);
			wasError = ((object)Method != null && (object)methodSymbol == null) || ((object)SetMethod != null && (object)methodSymbol2 == null);
			return new MethodInfo(symbol, methodSymbol, methodSymbol2);
			static MethodSymbol? replace(MethodSymbol? method)
			{
				return method?.OriginalDefinition.TryGetCorrespondingExtensionImplementationMethod()?.AsMember(method.ContainingSymbol.ContainingType).ConstructIfGeneric(method.ContainingType.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics.Concat(method.TypeArgumentsWithAnnotations));
			}
		}

		public override string? ToString()
		{
			return Method?.ToString();
		}
	}

	private struct MethodInvocationInfo
	{
		public MethodInfo MethodInfo;

		public ImmutableArray<ParameterSymbol> Parameters;

		public BoundExpression? Receiver;

		public ThreeState ReceiverIsSubjectToCloning;

		public ImmutableArray<BoundExpression> ArgsOpt;

		public ImmutableArray<RefKind> ArgumentRefKindsOpt;

		public ImmutableArray<int> ArgsToParamsOpt;

		public bool HasAnyErrors;

		public static MethodInvocationInfo FromCall(BoundCall call, BoundExpression? substitutedReceiver = null)
		{
			return new MethodInvocationInfo
			{
				MethodInfo = MethodInfo.Create(call.Method),
				Parameters = call.Method.Parameters,
				Receiver = (substitutedReceiver ?? call.ReceiverOpt),
				ReceiverIsSubjectToCloning = call.InitialBindingReceiverIsSubjectToCloning,
				ArgsOpt = call.Arguments,
				ArgumentRefKindsOpt = call.ArgumentRefKindsOpt,
				ArgsToParamsOpt = call.ArgsToParamsOpt,
				HasAnyErrors = call.HasAnyErrors
			};
		}

		public static MethodInvocationInfo FromCallParts(MethodSymbol method, BoundExpression receiver, ImmutableArray<BoundExpression> args, ThreeState receiverIsSubjectToCloning)
		{
			return new MethodInvocationInfo
			{
				MethodInfo = MethodInfo.Create(method),
				Parameters = method.Parameters,
				Receiver = receiver,
				ReceiverIsSubjectToCloning = receiverIsSubjectToCloning,
				ArgsOpt = args,
				ArgumentRefKindsOpt = default(ImmutableArray<RefKind>),
				ArgsToParamsOpt = default(ImmutableArray<int>),
				HasAnyErrors = false
			};
		}

		public static MethodInvocationInfo FromFunctionPointerInvocation(BoundFunctionPointerInvocation ptrInvocation)
		{
			FunctionPointerMethodSymbol signature = ptrInvocation.FunctionPointer.Signature;
			return new MethodInvocationInfo
			{
				MethodInfo = MethodInfo.Create(signature),
				Receiver = null,
				ReceiverIsSubjectToCloning = ThreeState.Unknown,
				Parameters = signature.Parameters,
				ArgsOpt = ptrInvocation.Arguments,
				ArgumentRefKindsOpt = ptrInvocation.ArgumentRefKindsOpt,
				ArgsToParamsOpt = default(ImmutableArray<int>),
				HasAnyErrors = ptrInvocation.HasAnyErrors
			};
		}

		public static MethodInvocationInfo FromIndexerAccess(BoundIndexerAccess indexerAccess, BoundExpression? substitutedReceiver = null)
		{
			return new MethodInvocationInfo
			{
				MethodInfo = MethodInfo.Create(indexerAccess),
				Receiver = (substitutedReceiver ?? indexerAccess.ReceiverOpt),
				ReceiverIsSubjectToCloning = indexerAccess.InitialBindingReceiverIsSubjectToCloning,
				Parameters = indexerAccess.Indexer.Parameters,
				ArgsOpt = indexerAccess.Arguments,
				ArgumentRefKindsOpt = indexerAccess.ArgumentRefKindsOpt,
				ArgsToParamsOpt = indexerAccess.ArgsToParamsOpt,
				HasAnyErrors = indexerAccess.HasAnyErrors
			};
		}

		public static MethodInvocationInfo FromObjectCreation(BoundObjectCreationExpressionBase objectCreation)
		{
			return new MethodInvocationInfo
			{
				MethodInfo = MethodInfo.Create(objectCreation.Constructor),
				Receiver = null,
				ReceiverIsSubjectToCloning = ThreeState.Unknown,
				Parameters = objectCreation.Constructor.Parameters,
				ArgsOpt = objectCreation.Arguments,
				ArgumentRefKindsOpt = objectCreation.ArgumentRefKindsOpt,
				ArgsToParamsOpt = objectCreation.ArgsToParamsOpt,
				HasAnyErrors = objectCreation.HasAnyErrors
			};
		}

		public static MethodInvocationInfo FromUnaryOperator(BoundUnaryOperator unaryOperator)
		{
			return new MethodInvocationInfo
			{
				MethodInfo = MethodInfo.Create(unaryOperator.MethodOpt),
				Receiver = null,
				ReceiverIsSubjectToCloning = ThreeState.Unknown,
				Parameters = unaryOperator.MethodOpt.Parameters,
				ArgsOpt = ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1] { unaryOperator.Operand }),
				ArgumentRefKindsOpt = default(ImmutableArray<RefKind>),
				ArgsToParamsOpt = default(ImmutableArray<int>),
				HasAnyErrors = unaryOperator.HasAnyErrors
			};
		}

		public static MethodInvocationInfo FromBinaryOperator(BoundBinaryOperator binaryOperator)
		{
			MethodSymbol binaryOperatorMethod = binaryOperator.BinaryOperatorMethod;
			return new MethodInvocationInfo
			{
				MethodInfo = MethodInfo.Create(binaryOperatorMethod),
				Receiver = null,
				ReceiverIsSubjectToCloning = ThreeState.Unknown,
				Parameters = binaryOperatorMethod.Parameters,
				ArgsOpt = ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[2] { binaryOperator.Left, binaryOperator.Right }),
				ArgumentRefKindsOpt = default(ImmutableArray<RefKind>),
				ArgsToParamsOpt = default(ImmutableArray<int>),
				HasAnyErrors = binaryOperator.HasAnyErrors
			};
		}

		public static MethodInvocationInfo FromUserDefinedConditionalLogicalOperator(BoundUserDefinedConditionalLogicalOperator logicalOperator)
		{
			return new MethodInvocationInfo
			{
				MethodInfo = MethodInfo.Create(logicalOperator.LogicalOperator),
				Receiver = null,
				ReceiverIsSubjectToCloning = ThreeState.Unknown,
				Parameters = logicalOperator.LogicalOperator.Parameters,
				ArgsOpt = ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[2] { logicalOperator.Left, logicalOperator.Right }),
				ArgumentRefKindsOpt = default(ImmutableArray<RefKind>),
				ArgsToParamsOpt = default(ImmutableArray<int>),
				HasAnyErrors = logicalOperator.HasAnyErrors
			};
		}

		public static MethodInvocationInfo FromUserDefinedConversion(MethodSymbol operatorMethod, BoundExpression operand, bool hasAnyErrors)
		{
			return new MethodInvocationInfo
			{
				MethodInfo = MethodInfo.Create(operatorMethod),
				Receiver = null,
				ReceiverIsSubjectToCloning = ThreeState.Unknown,
				Parameters = operatorMethod.Parameters,
				ArgsOpt = ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1] { operand }),
				ArgumentRefKindsOpt = default(ImmutableArray<RefKind>),
				ArgsToParamsOpt = default(ImmutableArray<int>),
				HasAnyErrors = hasAnyErrors
			};
		}

		public static MethodInvocationInfo FromInlineArrayConversion(SignatureOnlyMethodSymbol equivalentSignatureMethod, ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> refKinds, bool hasAnyErrors)
		{
			return new MethodInvocationInfo
			{
				MethodInfo = MethodInfo.Create(equivalentSignatureMethod),
				Receiver = null,
				ReceiverIsSubjectToCloning = ThreeState.Unknown,
				Parameters = equivalentSignatureMethod.Parameters,
				ArgsOpt = arguments,
				ArgumentRefKindsOpt = refKinds,
				ArgsToParamsOpt = default(ImmutableArray<int>),
				HasAnyErrors = hasAnyErrors
			};
		}

		public static MethodInvocationInfo FromIncrementOperator(BoundIncrementOperator incrementOperator)
		{
			return new MethodInvocationInfo
			{
				MethodInfo = MethodInfo.Create(incrementOperator.MethodOpt),
				Receiver = null,
				ReceiverIsSubjectToCloning = ThreeState.Unknown,
				Parameters = incrementOperator.MethodOpt.Parameters,
				ArgsOpt = ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1] { incrementOperator.Operand }),
				ArgumentRefKindsOpt = default(ImmutableArray<RefKind>),
				ArgsToParamsOpt = default(ImmutableArray<int>),
				HasAnyErrors = incrementOperator.HasAnyErrors
			};
		}

		public static MethodInvocationInfo FromCompoundAssignmentOperator(BoundCompoundAssignmentOperator compoundOperator)
		{
			MethodSymbol method = compoundOperator.Operator.Method;
			return new MethodInvocationInfo
			{
				MethodInfo = MethodInfo.Create(method),
				Receiver = (method.IsStatic ? null : compoundOperator.Left),
				ReceiverIsSubjectToCloning = ((!method.IsStatic) ? ThreeState.False : ThreeState.Unknown),
				Parameters = method.Parameters,
				ArgsOpt = (method.IsStatic ? ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[2] { compoundOperator.Left, compoundOperator.Right }) : ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1] { compoundOperator.Right })),
				ArgumentRefKindsOpt = default(ImmutableArray<RefKind>),
				ArgsToParamsOpt = default(ImmutableArray<int>),
				HasAnyErrors = compoundOperator.HasAnyErrors
			};
		}

		public static MethodInvocationInfo FromInlineArrayAccess(SignatureOnlyMethodSymbol equivalentSignatureMethod, ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> refKinds, bool hasAnyErrors)
		{
			return new MethodInvocationInfo
			{
				MethodInfo = MethodInfo.Create(equivalentSignatureMethod),
				Receiver = null,
				ReceiverIsSubjectToCloning = ThreeState.Unknown,
				Parameters = equivalentSignatureMethod.Parameters,
				ArgsOpt = arguments,
				ArgumentRefKindsOpt = refKinds,
				ArgsToParamsOpt = default(ImmutableArray<int>),
				HasAnyErrors = hasAnyErrors
			};
		}

		public static MethodInvocationInfo FromProperty(BoundPropertyAccess propertyAccess)
		{
			return new MethodInvocationInfo
			{
				MethodInfo = MethodInfo.Create(propertyAccess.PropertySymbol),
				Receiver = propertyAccess.ReceiverOpt,
				ReceiverIsSubjectToCloning = propertyAccess.InitialBindingReceiverIsSubjectToCloning,
				HasAnyErrors = propertyAccess.HasAnyErrors
			};
		}

		public static MethodInvocationInfo FromCollectionElementInitializer(BoundCollectionElementInitializer colElement)
		{
			return new MethodInvocationInfo
			{
				MethodInfo = MethodInfo.Create(colElement.AddMethod),
				Parameters = colElement.AddMethod.Parameters,
				Receiver = colElement.ImplicitReceiverOpt,
				ArgsOpt = colElement.Arguments,
				ArgsToParamsOpt = colElement.ArgsToParamsOpt
			};
		}
	}

	private readonly struct MixableDestination
	{
		internal BoundExpression Argument { get; }

		internal ParameterSymbol? Parameter { get; }

		internal EscapeLevel EscapeLevel { get; }

		internal MixableDestination(ParameterSymbol parameter, BoundExpression argument)
		{
			Argument = argument;
			Parameter = parameter;
			EscapeLevel = GetParameterValEscapeLevel(parameter).Value;
		}

		internal MixableDestination(BoundExpression argument, EscapeLevel escapeLevel)
		{
			Argument = argument;
			Parameter = null;
			EscapeLevel = escapeLevel;
		}

		internal bool IsAssignableFrom(EscapeLevel level)
		{
			return EscapeLevel switch
			{
				EscapeLevel.CallingMethod => level == EscapeLevel.CallingMethod, 
				EscapeLevel.ReturnOnly => true, 
				_ => throw ExceptionUtilities.UnexpectedValue(EscapeLevel), 
			};
		}

		public override string? ToString()
		{
			return (Parameter, Argument, EscapeLevel).ToString();
		}
	}

	private readonly struct EscapeArgument
	{
		internal ParameterSymbol? Parameter { get; }

		internal BoundExpression Argument { get; }

		internal RefKind RefKind { get; }

		internal EscapeArgument(ParameterSymbol? parameter, BoundExpression argument, RefKind refKind, bool isArgList = false)
		{
			Argument = argument;
			Parameter = parameter;
			RefKind = refKind;
		}

		public void Deconstruct(out ParameterSymbol? parameter, out BoundExpression argument, out RefKind refKind)
		{
			parameter = Parameter;
			argument = Argument;
			refKind = RefKind;
		}

		public override string? ToString()
		{
			ParameterSymbol parameter = Parameter;
			if ((object)parameter == null)
			{
				return Argument.ToString();
			}
			return parameter.ToString();
		}
	}

	private readonly struct EscapeValue
	{
		internal ParameterSymbol? Parameter { get; }

		internal BoundExpression Argument { get; }

		internal EscapeLevel EscapeLevel { get; }

		internal bool IsRefEscape { get; }

		internal EscapeValue(ParameterSymbol? parameter, BoundExpression argument, EscapeLevel escapeLevel, bool isRefEscape)
		{
			Argument = argument;
			Parameter = parameter;
			EscapeLevel = escapeLevel;
			IsRefEscape = isRefEscape;
		}

		public void Deconstruct(out ParameterSymbol? parameter, out BoundExpression argument, out EscapeLevel escapeLevel, out bool isRefEscape)
		{
			parameter = Parameter;
			argument = Argument;
			escapeLevel = EscapeLevel;
			isRefEscape = IsRefEscape;
		}

		public override string? ToString()
		{
			ParameterSymbol parameter = Parameter;
			if ((object)parameter == null)
			{
				return Argument.ToString();
			}
			return parameter.ToString();
		}
	}

	private sealed class TypeParameterThisParameterSymbol : ThisParameterSymbolBase
	{
		private readonly TypeParameterSymbol _type;

		private readonly ParameterSymbol _underlyingParameter;

		public override TypeWithAnnotations TypeWithAnnotations => TypeWithAnnotations.Create(_type, NullableAnnotation.NotAnnotated);

		public override RefKind RefKind
		{
			get
			{
				RefKind refKind = _underlyingParameter.RefKind;
				if (refKind != RefKind.None)
				{
					return refKind;
				}
				if (!_underlyingParameter.ContainingType.IsInterface || _type.IsReferenceType)
				{
					return RefKind.None;
				}
				return RefKind.Ref;
			}
		}

		public override ImmutableArray<Location> Locations => _underlyingParameter.Locations;

		public override Symbol ContainingSymbol => _underlyingParameter.ContainingSymbol;

		internal override ScopedKind DeclaredScope
		{
			get
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/Binder.ValueChecks.cs", 3400);
			}
		}

		internal override ScopedKind EffectiveScope
		{
			get
			{
				if (HasUnscopedRefAttribute && UseUpdatedEscapeRules)
				{
					return ScopedKind.None;
				}
				if (!_underlyingParameter.ContainingType.IsInterface || _type.IsReferenceType)
				{
					return ScopedKind.None;
				}
				return ScopedKind.ScopedRef;
			}
		}

		internal override bool HasUnscopedRefAttribute => _underlyingParameter.HasUnscopedRefAttribute;

		internal sealed override bool UseUpdatedEscapeRules => _underlyingParameter.UseUpdatedEscapeRules;

		internal TypeParameterThisParameterSymbol(ParameterSymbol underlyingParameter, TypeParameterSymbol type)
		{
			_underlyingParameter = underlyingParameter;
			_type = type;
		}
	}

	private ref struct LocalScope
	{
		private readonly RefSafetyAnalysis _analysis;

		private readonly ImmutableArray<LocalSymbol> _locals;

		private readonly bool _adjustDepth;

		public LocalScope(RefSafetyAnalysis analysis, ImmutableArray<LocalSymbol> locals, bool adjustDepth = true)
		{
			_analysis = analysis;
			_locals = locals;
			_adjustDepth = adjustDepth;
			if (adjustDepth)
			{
				_analysis._localScopeDepth = _analysis._localScopeDepth.Narrower();
			}
			foreach (LocalSymbol item in locals)
			{
				_analysis.AddLocalScopes(item, _analysis._localScopeDepth, SafeContext.CallingMethod);
			}
		}

		public void Dispose()
		{
			foreach (LocalSymbol local in _locals)
			{
				_analysis.RemoveLocalScopes(local);
			}
			if (_adjustDepth)
			{
				_analysis._localScopeDepth = _analysis._localScopeDepth.Wider();
			}
		}
	}

	private ref struct UnsafeRegion
	{
		private readonly RefSafetyAnalysis _analysis;

		private readonly bool _previousRegion;

		public UnsafeRegion(RefSafetyAnalysis analysis, bool inUnsafeRegion)
		{
			_analysis = analysis;
			_previousRegion = analysis._inUnsafeRegion;
			_analysis._inUnsafeRegion = inUnsafeRegion;
		}

		public void Dispose()
		{
			_analysis._inUnsafeRegion = _previousRegion;
		}
	}

	private ref struct PatternInput
	{
		private readonly RefSafetyAnalysis _analysis;

		private readonly SafeContext _previousInputValEscape;

		public PatternInput(RefSafetyAnalysis analysis, SafeContext patternInputValEscape)
		{
			_analysis = analysis;
			_previousInputValEscape = analysis._patternInputValEscape;
			_analysis._patternInputValEscape = patternInputValEscape;
		}

		public void Dispose()
		{
			_analysis._patternInputValEscape = _previousInputValEscape;
		}
	}

	private ref struct PlaceholderRegion
	{
		private readonly RefSafetyAnalysis _analysis;

		private readonly ArrayBuilder<(BoundValuePlaceholderBase, SafeContextAndLocation)> _placeholders;

		public PlaceholderRegion(RefSafetyAnalysis analysis, ArrayBuilder<(BoundValuePlaceholderBase, SafeContextAndLocation)> placeholders)
		{
			_analysis = analysis;
			_placeholders = placeholders;
			foreach (var (placeholder, valEscapeScope) in placeholders)
			{
				_analysis.AddPlaceholderScope(placeholder, valEscapeScope);
			}
		}

		public void Dispose()
		{
			foreach (var placeholder in _placeholders)
			{
				BoundValuePlaceholderBase item = placeholder.Item1;
				_analysis.RemovePlaceholderScope(item);
			}
			_placeholders.Free();
		}
	}

	private readonly struct SafeContextAndLocation
	{
		public readonly SafeContext Context;

		public static SafeContextAndLocation Create(SafeContext context)
		{
			return new SafeContextAndLocation(context);
		}

		private SafeContextAndLocation(SafeContext context)
		{
			Context = context;
		}
	}

	private readonly struct DeconstructionVariable
	{
		internal readonly BoundExpression Expression;

		internal readonly SafeContext ValEscape;

		internal readonly ArrayBuilder<DeconstructionVariable>? NestedVariables;

		internal DeconstructionVariable(BoundExpression expression, SafeContext valEscape, ArrayBuilder<DeconstructionVariable>? nestedVariables)
		{
			Expression = expression;
			ValEscape = valEscape;
			NestedVariables = nestedVariables;
		}
	}

	private readonly CSharpCompilation _compilation;

	private readonly MethodSymbol _symbol;

	private readonly BoundNode _rootNode;

	private readonly bool _useUpdatedEscapeRules;

	private readonly BindingDiagnosticBag _diagnostics;

	private bool _inUnsafeRegion;

	private SafeContext _localScopeDepth;

	private Dictionary<LocalSymbol, (SafeContext RefEscapeScope, SafeContext ValEscapeScope)>? _localEscapeScopes;

	private Dictionary<BoundValuePlaceholderBase, SafeContextAndLocation>? _placeholderScopes;

	private SafeContext _patternInputValEscape;

	private bool CheckLocalRefEscape(SyntaxNode node, BoundLocal local, SafeContext escapeTo, bool checkingReceiver, BindingDiagnosticBag diagnostics)
	{
		LocalSymbol localSymbol = local.LocalSymbol;
		if (GetLocalScopes(localSymbol).RefEscapeScope.IsConvertibleTo(escapeTo))
		{
			return true;
		}
		bool inUnsafeRegion = _inUnsafeRegion;
		if (escapeTo.IsReturnable)
		{
			if (localSymbol.RefKind == RefKind.None)
			{
				if (checkingReceiver)
				{
					Error(diagnostics, inUnsafeRegion ? ErrorCode.WRN_RefReturnLocal2 : ErrorCode.ERR_RefReturnLocal2, local.Syntax, localSymbol);
				}
				else
				{
					Error(diagnostics, inUnsafeRegion ? ErrorCode.WRN_RefReturnLocal : ErrorCode.ERR_RefReturnLocal, node, localSymbol);
				}
				return inUnsafeRegion;
			}
			if (checkingReceiver)
			{
				Error(diagnostics, inUnsafeRegion ? ErrorCode.WRN_RefReturnNonreturnableLocal2 : ErrorCode.ERR_RefReturnNonreturnableLocal2, local.Syntax, localSymbol);
			}
			else
			{
				Error(diagnostics, inUnsafeRegion ? ErrorCode.WRN_RefReturnNonreturnableLocal : ErrorCode.ERR_RefReturnNonreturnableLocal, node, localSymbol);
			}
			return inUnsafeRegion;
		}
		Error(diagnostics, inUnsafeRegion ? ErrorCode.WRN_EscapeVariable : ErrorCode.ERR_EscapeVariable, node, localSymbol);
		return inUnsafeRegion;
	}

	private static EscapeLevel? EscapeLevelFromScope(SafeContext lifetime)
	{
		if (!lifetime.IsReturnOnly)
		{
			if (lifetime.IsCallingMethod)
			{
				return EscapeLevel.CallingMethod;
			}
			return null;
		}
		return EscapeLevel.ReturnOnly;
	}

	private static SafeContext GetParameterValEscape(ParameterSymbol parameter)
	{
		if ((object)parameter != null)
		{
			if (parameter.EffectiveScope == ScopedKind.ScopedValue)
			{
				return SafeContext.CurrentMethod;
			}
			if (parameter.RefKind == RefKind.Out && parameter.UseUpdatedEscapeRules)
			{
				return SafeContext.ReturnOnly;
			}
		}
		return SafeContext.CallingMethod;
	}

	private static EscapeLevel? GetParameterValEscapeLevel(ParameterSymbol parameter)
	{
		return EscapeLevelFromScope(GetParameterValEscape(parameter));
	}

	private static SafeContext GetParameterRefEscape(ParameterSymbol parameter)
	{
		if ((object)parameter != null)
		{
			RefKind refKind = parameter.RefKind;
			if (refKind == RefKind.None)
			{
				return SafeContext.CurrentMethod;
			}
			if (parameter.EffectiveScope == ScopedKind.ScopedRef)
			{
				return SafeContext.CurrentMethod;
			}
			if (parameter.HasUnscopedRefAttribute && parameter.UseUpdatedEscapeRules)
			{
				if (refKind == RefKind.Out)
				{
					return SafeContext.ReturnOnly;
				}
				if (!parameter.IsThis)
				{
					return SafeContext.CallingMethod;
				}
			}
		}
		return SafeContext.ReturnOnly;
	}

	private static EscapeLevel? GetParameterRefEscapeLevel(ParameterSymbol parameter)
	{
		return EscapeLevelFromScope(GetParameterRefEscape(parameter));
	}

	private bool CheckParameterValEscape(SyntaxNode node, ParameterSymbol parameter, SafeContext escapeTo, BindingDiagnosticBag diagnostics)
	{
		if (_useUpdatedEscapeRules)
		{
			if (!GetParameterValEscape(parameter).IsConvertibleTo(escapeTo))
			{
				Error(diagnostics, _inUnsafeRegion ? ErrorCode.WRN_EscapeVariable : ErrorCode.ERR_EscapeVariable, node, parameter);
				return _inUnsafeRegion;
			}
			return true;
		}
		return true;
	}

	private bool CheckParameterRefEscape(SyntaxNode node, BoundExpression parameter, ParameterSymbol parameterSymbol, SafeContext escapeTo, bool checkingReceiver, BindingDiagnosticBag diagnostics)
	{
		SafeContext parameterRefEscape = GetParameterRefEscape(parameterSymbol);
		if (!parameterRefEscape.IsConvertibleTo(escapeTo))
		{
			bool flag = parameterSymbol.EffectiveScope == ScopedKind.ScopedRef;
			bool inUnsafeRegion = _inUnsafeRegion;
			if (parameter is BoundThisReference)
			{
				Error(diagnostics, inUnsafeRegion ? ErrorCode.WRN_RefReturnStructThis : ErrorCode.ERR_RefReturnStructThis, node);
				return inUnsafeRegion;
			}
			var (code, syntaxNode) = (checkingReceiver ? (flag ? (inUnsafeRegion ? (ErrorCode.WRN_RefReturnScopedParameter2, parameter.Syntax) : (ErrorCode.ERR_RefReturnScopedParameter2, parameter.Syntax)) : ((!inUnsafeRegion) ? ((!parameterRefEscape.IsReturnOnly) ? (ErrorCode.ERR_RefReturnParameter2, parameter.Syntax) : (ErrorCode.ERR_RefReturnOnlyParameter2, parameter.Syntax)) : ((!parameterRefEscape.IsReturnOnly) ? (ErrorCode.WRN_RefReturnParameter2, parameter.Syntax) : (ErrorCode.WRN_RefReturnOnlyParameter2, parameter.Syntax)))) : (flag ? (inUnsafeRegion ? (ErrorCode.WRN_RefReturnScopedParameter, node) : (ErrorCode.ERR_RefReturnScopedParameter, node)) : ((!inUnsafeRegion) ? ((!parameterRefEscape.IsReturnOnly) ? (ErrorCode.ERR_RefReturnParameter, node) : (ErrorCode.ERR_RefReturnOnlyParameter, node)) : ((!parameterRefEscape.IsReturnOnly) ? (ErrorCode.WRN_RefReturnParameter, node) : (ErrorCode.WRN_RefReturnOnlyParameter, node)))));
			Error(diagnostics, code, syntaxNode, parameterSymbol.Name);
			return inUnsafeRegion;
		}
		return true;
	}

	private SafeContext GetFieldRefEscape(BoundFieldAccess fieldAccess)
	{
		FieldSymbol fieldSymbol = fieldAccess.FieldSymbol;
		if (fieldSymbol.IsStatic || fieldSymbol.ContainingType.IsReferenceType)
		{
			return SafeContext.CallingMethod;
		}
		if (_useUpdatedEscapeRules && fieldSymbol.RefKind != RefKind.None)
		{
			return GetValEscape(fieldAccess.ReceiverOpt);
		}
		return GetRefEscape(fieldAccess.ReceiverOpt);
	}

	private bool CheckFieldRefEscape(SyntaxNode node, BoundFieldAccess fieldAccess, SafeContext escapeTo, BindingDiagnosticBag diagnostics)
	{
		FieldSymbol fieldSymbol = fieldAccess.FieldSymbol;
		if (fieldSymbol.IsStatic || fieldSymbol.ContainingType.IsReferenceType)
		{
			return true;
		}
		if (_useUpdatedEscapeRules && fieldSymbol.RefKind != RefKind.None)
		{
			return CheckValEscape(node, fieldAccess.ReceiverOpt, escapeTo, checkingReceiver: true, diagnostics);
		}
		return CheckRefEscape(node, fieldAccess.ReceiverOpt, escapeTo, checkingReceiver: true, diagnostics);
	}

	private bool CheckFieldLikeEventRefEscape(SyntaxNode node, BoundEventAccess eventAccess, SafeContext escapeTo, BindingDiagnosticBag diagnostics)
	{
		EventSymbol eventSymbol = eventAccess.EventSymbol;
		if (eventSymbol.IsStatic || eventSymbol.ContainingType.IsReferenceType)
		{
			return true;
		}
		return CheckRefEscape(node, eventAccess.ReceiverOpt, escapeTo, checkingReceiver: true, diagnostics);
	}

	internal SafeContext GetInterpolatedStringHandlerConversionEscapeScope(BoundExpression expression)
	{
		return GetValEscape(expression.GetInterpolatedStringHandlerData().Construction).Intersect(GetValEscapeOfInterpolatedStringHandlerCalls(expression));
	}

	private SafeContext GetInvocationEscapeScope(in MethodInvocationInfo methodInvocationInfo, bool isRefEscape)
	{
		MethodInvocationInfo methodInvocationInfo2 = ReplaceWithExtensionImplementationIfNeeded(in methodInvocationInfo);
		if (methodInvocationInfo.MethodInfo.UseUpdatedEscapeRules)
		{
			return GetInvocationEscapeWithUpdatedRules(in methodInvocationInfo2, isRefEscape);
		}
		return getInvocationEscapeWithOldRules(in methodInvocationInfo2, isRefEscape);
		SafeContext getInvocationEscapeWithOldRules(ref readonly MethodInvocationInfo reference, bool flag3)
		{
			SafeContext safeContext = SafeContext.CallingMethod;
			ArrayBuilder<EscapeValue> instance = ArrayBuilder<EscapeValue>.GetInstance();
			GetEscapeValuesForOldRules(in reference, ignoreArglistRefKinds: true, null, instance);
			try
			{
				foreach (var (_, expr, _, flag2) in instance)
				{
					SafeContext safeContext2;
					if (flag3)
					{
						if (!flag2)
						{
							goto IL_0063;
						}
						safeContext2 = GetRefEscape(expr);
					}
					else
					{
						if (flag2)
						{
							goto IL_0063;
						}
						safeContext2 = GetValEscape(expr);
					}
					goto IL_0066;
					IL_0066:
					SafeContext other = safeContext2;
					safeContext = safeContext.Intersect(other);
					if (_localScopeDepth.IsConvertibleTo(safeContext))
					{
						return safeContext;
					}
					continue;
					IL_0063:
					safeContext2 = safeContext;
					goto IL_0066;
				}
			}
			finally
			{
				instance.Free();
			}
			MethodSymbol? method = reference.MethodInfo.Method;
			if ((object)method != null && method.RequiresInstanceReceiver)
			{
				BoundExpression? receiver = reference.Receiver;
				if (receiver != null && receiver.Type?.IsRefLikeOrAllowsRefLikeType() == true)
				{
					safeContext = safeContext.Intersect(GetValEscape(reference.Receiver));
				}
			}
			return safeContext;
		}
	}

	private SafeContext GetInvocationEscapeWithUpdatedRules(ref readonly MethodInvocationInfo methodInvocationInfo, bool isRefEscape)
	{
		SafeContext safeContext = SafeContext.CallingMethod;
		ArrayBuilder<EscapeValue> instance = ArrayBuilder<EscapeValue>.GetInstance();
		GetFilteredInvocationArgumentsForEscapeWithUpdatedRules(in methodInvocationInfo, isRefEscape, ignoreArglistRefKinds: true, instance);
		bool returnsRefToRefStruct = methodInvocationInfo.MethodInfo.ReturnsRefToRefStruct;
		foreach (var (parameterSymbol2, expr, _, flag2) in instance)
		{
			if (returnsRefToRefStruct)
			{
				if ((object)parameterSymbol2 != null)
				{
					if ((object)parameterSymbol2 == null || parameterSymbol2.RefKind == RefKind.None)
					{
						continue;
					}
					TypeSymbol type = parameterSymbol2.Type;
					if ((object)type == null || !type.IsRefLikeOrAllowsRefLikeType())
					{
						continue;
					}
				}
				if (flag2 != isRefEscape)
				{
					continue;
				}
			}
			SafeContext other = (flag2 ? GetRefEscape(expr) : GetValEscape(expr));
			safeContext = safeContext.Intersect(other);
			if (_localScopeDepth.IsConvertibleTo(safeContext))
			{
				break;
			}
		}
		instance.Free();
		return safeContext;
	}

	private SafeContext GetInvocationEscapeToReceiver(in MethodInvocationInfo methodInvocationInfo)
	{
		SafeContext safeContext = SafeContext.CallingMethod;
		ArrayBuilder<EscapeValue> instance = ArrayBuilder<EscapeValue>.GetInstance();
		GetFilteredInvocationArgumentsForEscapeToReceiver(in methodInvocationInfo, instance);
		foreach (var (_, expr, _, flag2) in instance)
		{
			SafeContext other = (flag2 ? GetRefEscape(expr) : GetValEscape(expr));
			safeContext = safeContext.Intersect(other);
			if (_localScopeDepth.IsConvertibleTo(safeContext))
			{
				break;
			}
		}
		instance.Free();
		return safeContext;
	}

	private static MethodInvocationInfo ReplaceWithExtensionImplementationIfNeeded(ref readonly MethodInvocationInfo methodInvocationInfo)
	{
		Symbol symbol = methodInvocationInfo.MethodInfo.Symbol;
		if ((object)symbol == null || !symbol.IsExtensionBlockMember() || symbol.IsStatic)
		{
			return methodInvocationInfo;
		}
		MethodInfo methodInfo = methodInvocationInfo.MethodInfo.ReplaceWithExtensionImplementation(out var wasError);
		if (wasError)
		{
			return methodInvocationInfo;
		}
		MethodInvocationInfo methodInvocationInfo2 = methodInvocationInfo;
		methodInvocationInfo2.MethodInfo = methodInfo;
		MethodInvocationInfo result = methodInvocationInfo2;
		ParameterSymbol extensionParameter = symbol.ContainingType.ExtensionParameter;
		ImmutableArray<ParameterSymbol> parameters2;
		if (!methodInvocationInfo.Parameters.IsDefault)
		{
			ParameterSymbol parameterSymbol = extensionParameter;
			ImmutableArray<ParameterSymbol> parameters = methodInvocationInfo.Parameters;
			int num = 0;
			ParameterSymbol[] array = new ParameterSymbol[1 + parameters.Length];
			array[num] = parameterSymbol;
			num++;
			ReadOnlySpan<ParameterSymbol> readOnlySpan = parameters.AsSpan();
			readOnlySpan.CopyTo(new Span<ParameterSymbol>(array).Slice(num, readOnlySpan.Length));
			num += readOnlySpan.Length;
			parameters2 = ImmutableCollectionsMarshal.AsImmutableArray(array);
		}
		else
		{
			parameters2 = ImmutableCollectionsMarshal.AsImmutableArray(new ParameterSymbol[1] { extensionParameter });
		}
		result.Parameters = parameters2;
		if (methodInvocationInfo.Receiver != null)
		{
			ImmutableArray<BoundExpression> argsOpt2;
			if (!methodInvocationInfo.ArgsOpt.IsDefault)
			{
				BoundExpression receiver = methodInvocationInfo.Receiver;
				ImmutableArray<BoundExpression> argsOpt = methodInvocationInfo.ArgsOpt;
				int num = 0;
				BoundExpression[] array2 = new BoundExpression[1 + argsOpt.Length];
				array2[num] = receiver;
				num++;
				ReadOnlySpan<BoundExpression> readOnlySpan2 = argsOpt.AsSpan();
				readOnlySpan2.CopyTo(new Span<BoundExpression>(array2).Slice(num, readOnlySpan2.Length));
				num += readOnlySpan2.Length;
				argsOpt2 = ImmutableCollectionsMarshal.AsImmutableArray(array2);
			}
			else
			{
				argsOpt2 = ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1] { methodInvocationInfo.Receiver });
			}
			result.ArgsOpt = argsOpt2;
			result.Receiver = null;
		}
		if (!methodInvocationInfo.ArgumentRefKindsOpt.IsDefault)
		{
			RefKind refKind = RefKind.None;
			ImmutableArray<RefKind> argumentRefKindsOpt = methodInvocationInfo.ArgumentRefKindsOpt;
			int num = 0;
			RefKind[] array3 = new RefKind[1 + argumentRefKindsOpt.Length];
			array3[num] = refKind;
			num++;
			ReadOnlySpan<RefKind> readOnlySpan3 = argumentRefKindsOpt.AsSpan();
			readOnlySpan3.CopyTo(new Span<RefKind>(array3).Slice(num, readOnlySpan3.Length));
			num += readOnlySpan3.Length;
			result.ArgumentRefKindsOpt = ImmutableCollectionsMarshal.AsImmutableArray(array3);
		}
		if (!methodInvocationInfo.ArgsToParamsOpt.IsDefault)
		{
			ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance(methodInvocationInfo.ArgsToParamsOpt.Length + 1);
			instance.Add(0);
			for (int i = 0; i < methodInvocationInfo.ArgsToParamsOpt.Length; i++)
			{
				instance.Add(methodInvocationInfo.ArgsToParamsOpt[i] + 1);
			}
			result.ArgsToParamsOpt = instance.ToImmutableAndFree();
		}
		return result;
	}

	private bool CheckInvocationEscape(SyntaxNode syntax, in MethodInvocationInfo methodInvocationInfo, bool checkingReceiver, SafeContext escapeTo, BindingDiagnosticBag diagnostics, bool isRefEscape)
	{
		MethodInvocationInfo methodInvocationInfo2 = ReplaceWithExtensionImplementationIfNeeded(in methodInvocationInfo);
		if (methodInvocationInfo.MethodInfo.UseUpdatedEscapeRules)
		{
			return CheckInvocationEscapeWithUpdatedRules(syntax, in methodInvocationInfo2, checkingReceiver, escapeTo, diagnostics, isRefEscape, methodInvocationInfo.MethodInfo.Symbol);
		}
		return checkInvocationEscapeWithOldRules(syntax, in methodInvocationInfo2, checkingReceiver, escapeTo, diagnostics, isRefEscape, methodInvocationInfo.MethodInfo.Symbol);
		bool checkInvocationEscapeWithOldRules(SyntaxNode syntax2, ref readonly MethodInvocationInfo reference, bool checkingReceiver2, SafeContext escapeTo2, BindingDiagnosticBag diagnostics2, bool flag, Symbol symbolForReporting)
		{
			MethodInvocationInfo methodInvocationInfo3 = reference;
			methodInvocationInfo3.Receiver = null;
			methodInvocationInfo3.ReceiverIsSubjectToCloning = ThreeState.Unknown;
			MethodInvocationInfo methodInvocationInfo4 = methodInvocationInfo3;
			Symbol symbol = reference.MethodInfo.Symbol;
			ArrayBuilder<EscapeArgument> instance = ArrayBuilder<EscapeArgument>.GetInstance();
			GetInvocationArgumentsForEscape(in methodInvocationInfo4, ignoreArglistRefKinds: true, null, instance);
			try
			{
				foreach (var (parameter, boundExpression2, refKind2) in instance)
				{
					if (!(((refKind2 != RefKind.None) & flag) ? CheckRefEscape(boundExpression2.Syntax, boundExpression2, escapeTo2, checkingReceiver: false, diagnostics2) : CheckValEscape(boundExpression2.Syntax, boundExpression2, escapeTo2, checkingReceiver: false, diagnostics2)))
					{
						if (!(symbol is SignatureOnlyMethodSymbol))
						{
							ReportInvocationEscapeError(syntax2, reference.MethodInfo.Symbol, parameter, checkingReceiver2, diagnostics2);
						}
						return false;
					}
				}
			}
			finally
			{
				instance.Free();
			}
			if (symbol.RequiresInstanceReceiver())
			{
				BoundExpression receiver = reference.Receiver;
				if (receiver != null && receiver.Type?.IsRefLikeOrAllowsRefLikeType() == true)
				{
					return CheckValEscape(receiver.Syntax, receiver, escapeTo2, checkingReceiver: false, diagnostics2);
				}
			}
			return true;
		}
	}

	private bool CheckInvocationEscapeWithUpdatedRules(SyntaxNode syntax, ref readonly MethodInvocationInfo methodInvocationInfo, bool checkingReceiver, SafeContext escapeTo, BindingDiagnosticBag diagnostics, bool isRefEscape, Symbol symbolForReporting)
	{
		bool result = true;
		ArrayBuilder<EscapeValue> instance = ArrayBuilder<EscapeValue>.GetInstance();
		GetFilteredInvocationArgumentsForEscapeWithUpdatedRules(in methodInvocationInfo, isRefEscape, ignoreArglistRefKinds: true, instance);
		bool returnsRefToRefStruct = methodInvocationInfo.MethodInfo.ReturnsRefToRefStruct;
		foreach (var (parameterSymbol2, boundExpression2, _, flag2) in instance)
		{
			if (returnsRefToRefStruct)
			{
				if ((object)parameterSymbol2 != null)
				{
					if ((object)parameterSymbol2 == null || parameterSymbol2.RefKind == RefKind.None)
					{
						continue;
					}
					TypeSymbol type = parameterSymbol2.Type;
					if ((object)type == null || !type.IsRefLikeOrAllowsRefLikeType())
					{
						continue;
					}
				}
				if (flag2 != isRefEscape)
				{
					continue;
				}
			}
			if (!(flag2 ? CheckRefEscape(boundExpression2.Syntax, boundExpression2, escapeTo, checkingReceiver: false, diagnostics) : CheckValEscape(boundExpression2.Syntax, boundExpression2, escapeTo, checkingReceiver: false, diagnostics)))
			{
				if (((boundExpression2 as BoundCapturedReceiverPlaceholder)?.Receiver ?? boundExpression2) != methodInvocationInfo.Receiver && !(methodInvocationInfo.MethodInfo.Symbol is SignatureOnlyMethodSymbol))
				{
					ReportInvocationEscapeError(syntax, symbolForReporting, parameterSymbol2, checkingReceiver, diagnostics);
				}
				result = false;
				break;
			}
		}
		instance.Free();
		return result;
	}

	private bool CheckInvocationEscapeToReceiver(SyntaxNode syntax, in MethodInvocationInfo methodInvocationInfo, bool checkingReceiver, SafeContext escapeTo, BindingDiagnosticBag diagnostics)
	{
		bool result = true;
		ArrayBuilder<EscapeValue> instance = ArrayBuilder<EscapeValue>.GetInstance();
		GetFilteredInvocationArgumentsForEscapeToReceiver(in methodInvocationInfo, instance);
		foreach (var (parameter, boundExpression2, _, flag2) in instance)
		{
			if (!(flag2 ? CheckRefEscape(boundExpression2.Syntax, boundExpression2, escapeTo, checkingReceiver: false, diagnostics) : CheckValEscape(boundExpression2.Syntax, boundExpression2, escapeTo, checkingReceiver: false, diagnostics)))
			{
				ReportInvocationEscapeError(syntax, methodInvocationInfo.MethodInfo.Symbol, parameter, checkingReceiver, diagnostics);
				result = false;
				break;
			}
		}
		instance.Free();
		return result;
	}

	private void GetInvocationArgumentsForEscape(ref readonly MethodInvocationInfo methodInvocationInfo, bool ignoreArglistRefKinds, ArrayBuilder<MixableDestination>? mixableArguments, ArrayBuilder<EscapeArgument> escapeArguments)
	{
		BoundExpression boundExpression = methodInvocationInfo.Receiver;
		if (boundExpression != null)
		{
			_ = methodInvocationInfo.MethodInfo.Method;
			if (methodInvocationInfo.ReceiverIsSubjectToCloning == ThreeState.True)
			{
				boundExpression = new BoundCapturedReceiverPlaceholder(boundExpression.Syntax, boundExpression, boundExpression.Type).MakeCompilerGenerated();
			}
			EscapeArgument item = getReceiver(in methodInvocationInfo.MethodInfo, boundExpression);
			escapeArguments.Add(item);
			if (mixableArguments != null && isMixableParameter(item.Parameter))
			{
				mixableArguments.Add(new MixableDestination(item.Parameter, boundExpression));
			}
		}
		ImmutableArray<BoundExpression> argsOpt = methodInvocationInfo.ArgsOpt;
		if (argsOpt.IsDefault)
		{
			return;
		}
		ImmutableArray<ParameterSymbol> parameters = methodInvocationInfo.Parameters;
		ImmutableArray<int> argsToParamsOpt = methodInvocationInfo.ArgsToParamsOpt;
		ImmutableArray<RefKind> argumentRefKindsOpt = methodInvocationInfo.ArgumentRefKindsOpt;
		for (int num = 0; num < argsOpt.Length; num++)
		{
			BoundExpression boundExpression2 = argsOpt[num];
			if (boundExpression2.Kind == BoundKind.ArgListOperator)
			{
				BoundArgListOperator boundArgListOperator = (BoundArgListOperator)boundExpression2;
				getArgList(boundArgListOperator.Arguments, ignoreArglistRefKinds ? default(ImmutableArray<RefKind>) : boundArgListOperator.ArgumentRefKindsOpt, mixableArguments, escapeArguments);
				break;
			}
			ParameterSymbol parameterSymbol = ((num < parameters.Length) ? parameters[argsToParamsOpt.IsDefault ? num : argsToParamsOpt[num]] : null);
			if (mixableArguments != null && isMixableParameter(parameterSymbol) && isMixableArgument(boundExpression2))
			{
				mixableArguments.Add(new MixableDestination(parameterSymbol, boundExpression2));
			}
			RefKind refKind = parameterSymbol?.RefKind ?? RefKind.None;
			if (!argumentRefKindsOpt.IsDefault)
			{
				refKind = argumentRefKindsOpt[num];
			}
			bool flag = refKind == RefKind.None;
			bool flag2;
			if (flag)
			{
				RefKind? refKind2 = parameterSymbol?.RefKind;
				if (refKind2.HasValue)
				{
					RefKind valueOrDefault = refKind2.GetValueOrDefault();
					if (valueOrDefault - 3 <= RefKind.Ref)
					{
						flag2 = true;
						goto IL_01a7;
					}
				}
				flag2 = false;
				goto IL_01a7;
			}
			goto IL_01ab;
			IL_01a7:
			flag = flag2;
			goto IL_01ab;
			IL_01ab:
			if (flag)
			{
				refKind = parameterSymbol.RefKind;
			}
			escapeArguments.Add(new EscapeArgument(parameterSymbol, boundExpression2, refKind));
		}
		static void getArgList(ImmutableArray<BoundExpression> immutableArray, ImmutableArray<RefKind> argRefKindsOpt, ArrayBuilder<MixableDestination>? arrayBuilder2, ArrayBuilder<EscapeArgument> arrayBuilder)
		{
			for (int i = 0; i < immutableArray.Length; i++)
			{
				BoundExpression argument = immutableArray[i];
				RefKind refKind3 = ((!argRefKindsOpt.IsDefault) ? argRefKindsOpt[i] : RefKind.None);
				arrayBuilder.Add(new EscapeArgument(null, argument, refKind3, isArgList: true));
				if (refKind3 == RefKind.Ref)
				{
					arrayBuilder2?.Add(new MixableDestination(argument, EscapeLevel.CallingMethod));
				}
			}
		}
		static EscapeArgument getReceiver(in MethodInfo methodInfo, BoundExpression receiver)
		{
			if ((object)methodInfo.Method != null && (object)methodInfo.SetMethod != null)
			{
				EscapeArgument result = getReceiverCore(methodInfo.Method, receiver);
				if (result.RefKind == RefKind.Ref)
				{
					return result;
				}
				EscapeArgument result2 = getReceiverCore(methodInfo.SetMethod, receiver);
				if (result2.RefKind == RefKind.Ref)
				{
					return result2;
				}
				return result;
			}
			return getReceiverCore(methodInfo.Method, receiver);
		}
		static EscapeArgument getReceiverCore(MethodSymbol? method, BoundExpression receiver)
		{
			if (method is FunctionPointerMethodSymbol)
			{
				return new EscapeArgument(null, receiver, RefKind.None);
			}
			RefKind refKind3 = RefKind.None;
			ParameterSymbol thisParameter = null;
			if ((object)method != null && method.TryGetThisParameter(out thisParameter) && (object)thisParameter != null)
			{
				if (receiver.Type is TypeParameterSymbol type)
				{
					thisParameter = new TypeParameterThisParameterSymbol(thisParameter, type);
				}
				refKind3 = thisParameter.RefKind;
			}
			return new EscapeArgument(thisParameter, receiver, refKind3);
		}
		static bool isMixableArgument(BoundExpression argument)
		{
			if (argument is BoundDeconstructValuePlaceholder boundDeconstructValuePlaceholder)
			{
				if ((object)boundDeconstructValuePlaceholder.VariableSymbol != null)
				{
					goto IL_0026;
				}
			}
			else if (argument is BoundLocal { DeclarationKind: not BoundLocalDeclarationKind.None })
			{
				goto IL_0026;
			}
			bool flag3 = false;
			goto IL_002c;
			IL_0026:
			flag3 = true;
			goto IL_002c;
			IL_002c:
			if (flag3)
			{
				return false;
			}
			if (argument.IsDiscardExpression())
			{
				return false;
			}
			return true;
		}
		static bool isMixableParameter([NotNullWhen(true)] ParameterSymbol? parameter)
		{
			if ((object)parameter != null && parameter.Type.IsRefLikeOrAllowsRefLikeType())
			{
				return parameter.RefKind.IsWritableReference();
			}
			return false;
		}
	}

	private void GetFilteredInvocationArgumentsForEscapeWithUpdatedRules(ref readonly MethodInvocationInfo methodInvocationInfo, bool isInvokedWithRef, bool ignoreArglistRefKinds, ArrayBuilder<EscapeValue> escapeValues)
	{
		if (isInvokedWithRef || hasRefLikeReturn(methodInvocationInfo.MethodInfo.Symbol))
		{
			GetEscapeValuesForUpdatedRules(in methodInvocationInfo, ignoreArglistRefKinds, null, escapeValues);
		}
		static bool hasRefLikeReturn(Symbol symbol)
		{
			if (symbol is MethodSymbol methodSymbol)
			{
				if (methodSymbol.MethodKind == MethodKind.Constructor)
				{
					return methodSymbol.ContainingType.IsRefLikeType;
				}
				return methodSymbol.ReturnType.IsRefLikeOrAllowsRefLikeType();
			}
			if (symbol is PropertySymbol propertySymbol)
			{
				return propertySymbol.Type.IsRefLikeOrAllowsRefLikeType();
			}
			return false;
		}
	}

	private void GetFilteredInvocationArgumentsForEscapeToReceiver(ref readonly MethodInvocationInfo methodInvocationInfo, ArrayBuilder<EscapeValue> escapeValues)
	{
		MethodInvocationInfo methodInvocationInfo2 = ReplaceWithExtensionImplementationIfNeeded(in methodInvocationInfo);
		MethodInfo methodInfo = methodInvocationInfo2.MethodInfo;
		ParameterSymbol parameterSymbol = null;
		if (methodInfo.Symbol.RequiresInstanceReceiver())
		{
			if (!hasRefToRefStructThis(methodInfo.Method) && !hasRefToRefStructThis(methodInfo.SetMethod))
			{
				return;
			}
		}
		else
		{
			ImmutableArray<ParameterSymbol> parameters = methodInvocationInfo2.Parameters;
			if (parameters.Length >= 1)
			{
				ParameterSymbol parameterSymbol2 = parameters[0];
				parameterSymbol = parameterSymbol2;
				if (!isRefToRefStruct(parameterSymbol))
				{
					return;
				}
			}
		}
		ArrayBuilder<EscapeValue> instance = ArrayBuilder<EscapeValue>.GetInstance();
		MethodInvocationInfo methodInvocationInfo3 = methodInvocationInfo2;
		methodInvocationInfo3.Receiver = null;
		GetEscapeValues(in methodInvocationInfo3, ignoreArglistRefKinds: true, null, instance);
		foreach (var (parameterSymbol4, argument, escapeLevel2, isRefEscape) in instance)
		{
			if (((object)parameterSymbol == null || !(parameterSymbol4 == parameterSymbol)) && escapeLevel2 == EscapeLevel.CallingMethod)
			{
				escapeValues.Add(new EscapeValue(parameterSymbol4, argument, escapeLevel2, isRefEscape));
			}
		}
		instance.Free();
		static bool hasRefToRefStructThis(MethodSymbol? method)
		{
			if ((object)method != null && method.TryGetThisParameter(out ParameterSymbol thisParameter) && (object)thisParameter != null)
			{
				return isRefToRefStruct(thisParameter);
			}
			return false;
		}
		static bool isRefToRefStruct(ParameterSymbol parameter)
		{
			if (parameter.RefKind == RefKind.Ref)
			{
				return parameter.Type.IsRefLikeOrAllowsRefLikeType();
			}
			return false;
		}
	}

	private void GetEscapeValues(in MethodInvocationInfo methodInvocationInfo, bool ignoreArglistRefKinds, ArrayBuilder<MixableDestination>? mixableArguments, ArrayBuilder<EscapeValue> escapeValues)
	{
		if (methodInvocationInfo.MethodInfo.UseUpdatedEscapeRules)
		{
			GetEscapeValuesForUpdatedRules(in methodInvocationInfo, ignoreArglistRefKinds, mixableArguments, escapeValues);
		}
		else
		{
			GetEscapeValuesForOldRules(in methodInvocationInfo, ignoreArglistRefKinds, mixableArguments, escapeValues);
		}
	}

	private void GetEscapeValuesForUpdatedRules(ref readonly MethodInvocationInfo methodInvocationInfo, bool ignoreArglistRefKinds, ArrayBuilder<MixableDestination>? mixableArguments, ArrayBuilder<EscapeValue> escapeValues)
	{
		MethodInvocationInfo methodInvocationInfo2 = methodInvocationInfo;
		if (!methodInvocationInfo.MethodInfo.Symbol.RequiresInstanceReceiver())
		{
			MethodInvocationInfo methodInvocationInfo3 = methodInvocationInfo;
			methodInvocationInfo3.Receiver = null;
			methodInvocationInfo3.ReceiverIsSubjectToCloning = ThreeState.Unknown;
			methodInvocationInfo2 = methodInvocationInfo3;
		}
		ArrayBuilder<EscapeArgument> instance = ArrayBuilder<EscapeArgument>.GetInstance();
		GetInvocationArgumentsForEscape(in methodInvocationInfo2, ignoreArglistRefKinds, mixableArguments, instance);
		foreach (var (parameterSymbol2, boundExpression2, refKind2) in instance)
		{
			if ((object)parameterSymbol2 == null)
			{
				if (refKind2 != RefKind.None)
				{
					escapeValues.Add(new EscapeValue(null, boundExpression2, EscapeLevel.ReturnOnly, isRefEscape: true));
				}
				TypeSymbol? type = boundExpression2.Type;
				if ((object)type != null && type.IsRefLikeOrAllowsRefLikeType())
				{
					escapeValues.Add(new EscapeValue(null, boundExpression2, EscapeLevel.CallingMethod, isRefEscape: false));
				}
				continue;
			}
			if (parameterSymbol2.Type.IsRefLikeOrAllowsRefLikeType() && parameterSymbol2.RefKind != RefKind.Out)
			{
				EscapeLevel? parameterValEscapeLevel = GetParameterValEscapeLevel(parameterSymbol2);
				if (parameterValEscapeLevel.HasValue)
				{
					EscapeLevel valueOrDefault = parameterValEscapeLevel.GetValueOrDefault();
					escapeValues.Add(new EscapeValue(parameterSymbol2, boundExpression2, valueOrDefault, isRefEscape: false));
				}
			}
			if (parameterSymbol2.RefKind != RefKind.None)
			{
				EscapeLevel? parameterValEscapeLevel = GetParameterRefEscapeLevel(parameterSymbol2);
				if (parameterValEscapeLevel.HasValue)
				{
					EscapeLevel valueOrDefault2 = parameterValEscapeLevel.GetValueOrDefault();
					escapeValues.Add(new EscapeValue(parameterSymbol2, boundExpression2, valueOrDefault2, isRefEscape: true));
				}
			}
		}
		instance.Free();
	}

	private void GetEscapeValuesForOldRules(ref readonly MethodInvocationInfo methodInvocationInfo, bool ignoreArglistRefKinds, ArrayBuilder<MixableDestination>? mixableArguments, ArrayBuilder<EscapeValue> escapeValues)
	{
		MethodInvocationInfo methodInvocationInfo2 = methodInvocationInfo;
		if (!methodInvocationInfo.MethodInfo.Symbol.RequiresInstanceReceiver())
		{
			MethodInvocationInfo methodInvocationInfo3 = methodInvocationInfo2;
			methodInvocationInfo3.Receiver = null;
			methodInvocationInfo3.ReceiverIsSubjectToCloning = ThreeState.Unknown;
			methodInvocationInfo2 = methodInvocationInfo3;
		}
		ArrayBuilder<EscapeArgument> instance = ArrayBuilder<EscapeArgument>.GetInstance();
		GetInvocationArgumentsForEscape(in methodInvocationInfo2, ignoreArglistRefKinds, mixableArguments, instance);
		foreach (var (parameterSymbol2, boundExpression2, _) in instance)
		{
			if ((object)parameterSymbol2 == null)
			{
				TypeSymbol? type = boundExpression2.Type;
				if ((object)type != null && type.IsRefLikeOrAllowsRefLikeType())
				{
					escapeValues.Add(new EscapeValue(null, boundExpression2, EscapeLevel.CallingMethod, isRefEscape: false));
				}
				continue;
			}
			if (parameterSymbol2.Type.IsRefLikeOrAllowsRefLikeType())
			{
				escapeValues.Add(new EscapeValue(parameterSymbol2, boundExpression2, EscapeLevel.CallingMethod, isRefEscape: false));
			}
			if (parameterSymbol2.RefKind != RefKind.None && !parameterSymbol2.IsThis)
			{
				escapeValues.Add(new EscapeValue(parameterSymbol2, boundExpression2, EscapeLevel.CallingMethod, isRefEscape: true));
			}
		}
		instance.Free();
	}

	private static string GetInvocationParameterName(ParameterSymbol? parameter)
	{
		if ((object)parameter == null)
		{
			return "__arglist";
		}
		string text = parameter.Name;
		if (string.IsNullOrEmpty(text))
		{
			text = parameter.Ordinal.ToString();
		}
		return text;
	}

	private static void ReportInvocationEscapeError(SyntaxNode syntax, Symbol symbol, ParameterSymbol? parameter, bool checkingReceiver, BindingDiagnosticBag diagnostics)
	{
		ErrorCode standardCallEscapeError = GetStandardCallEscapeError(checkingReceiver);
		string invocationParameterName = GetInvocationParameterName(parameter);
		Error(diagnostics, standardCallEscapeError, syntax, symbol, invocationParameterName);
	}

	private bool ShouldInferDeclarationExpressionValEscape(BoundExpression argument, [NotNullWhen(true)] out SourceLocalSymbol? localSymbol)
	{
		Symbol symbol = ((argument is BoundDeconstructValuePlaceholder boundDeconstructValuePlaceholder) ? boundDeconstructValuePlaceholder.VariableSymbol : ((!(argument is BoundLocal { DeclarationKind: not BoundLocalDeclarationKind.None } boundLocal)) ? null : boundLocal.LocalSymbol));
		if (symbol is SourceLocalSymbol sourceLocalSymbol && GetLocalScopes(sourceLocalSymbol).ValEscapeScope.IsCallingMethod)
		{
			localSymbol = sourceLocalSymbol;
			return true;
		}
		localSymbol = null;
		return false;
	}

	private bool CheckInvocationArgMixing(SyntaxNode syntax, ref readonly MethodInvocationInfo methodInvocationInfo, Symbol symbolForReporting, BindingDiagnosticBag diagnostics)
	{
		if (methodInvocationInfo.MethodInfo.UseUpdatedEscapeRules)
		{
			return CheckInvocationArgMixingWithUpdatedRules(syntax, in methodInvocationInfo, diagnostics, symbolForReporting);
		}
		return checkInvocationArgMixingWithOldRules(syntax, in methodInvocationInfo, diagnostics, symbolForReporting);
		bool checkInvocationArgMixingWithOldRules(SyntaxNode syntaxNode, ref readonly MethodInvocationInfo reference, BindingDiagnosticBag diagnostics2, Symbol symbol2)
		{
			Symbol symbol = reference.MethodInfo.Symbol;
			MethodInvocationInfo methodInvocationInfo2 = reference;
			if (!symbol.RequiresInstanceReceiver())
			{
				MethodInvocationInfo methodInvocationInfo3 = reference;
				methodInvocationInfo3.Receiver = null;
				methodInvocationInfo3.ReceiverIsSubjectToCloning = ThreeState.Unknown;
				methodInvocationInfo2 = methodInvocationInfo3;
			}
			SafeContext escapeTo = _localScopeDepth;
			ArrayBuilder<EscapeArgument> instance = ArrayBuilder<EscapeArgument>.GetInstance();
			GetInvocationArgumentsForEscape(in methodInvocationInfo2, ignoreArglistRefKinds: false, null, instance);
			try
			{
				ParameterSymbol parameter;
				BoundExpression argument;
				RefKind refKind;
				foreach (EscapeArgument item in instance)
				{
					item.Deconstruct(out parameter, out argument, out refKind);
					BoundExpression boundExpression = argument;
					RefKind refKind2 = refKind;
					if (!ShouldInferDeclarationExpressionValEscape(boundExpression, out SourceLocalSymbol _) && refKind2.IsWritableReference() && !boundExpression.IsDiscardExpression())
					{
						TypeSymbol? type = boundExpression.Type;
						if ((object)type != null && type.IsRefLikeOrAllowsRefLikeType())
						{
							escapeTo = escapeTo.Union(GetValEscape(boundExpression));
						}
					}
				}
				bool flag = false;
				SafeContext valEscapeScope = SafeContext.CallingMethod;
				foreach (EscapeArgument item2 in instance)
				{
					item2.Deconstruct(out parameter, out argument, out refKind);
					ParameterSymbol parameter2 = parameter;
					BoundExpression boundExpression2 = argument;
					valEscapeScope = valEscapeScope.Intersect(GetValEscape(boundExpression2));
					if (!flag && !CheckValEscape(boundExpression2.Syntax, boundExpression2, escapeTo, checkingReceiver: false, diagnostics2))
					{
						string invocationParameterName = GetInvocationParameterName(parameter2);
						Error(diagnostics2, ErrorCode.ERR_CallArgMixing, syntaxNode, symbol2, invocationParameterName);
						flag = true;
					}
				}
				foreach (EscapeArgument item3 in instance)
				{
					item3.Deconstruct(out parameter, out argument, out refKind);
					BoundExpression argument2 = argument;
					if (ShouldInferDeclarationExpressionValEscape(argument2, out SourceLocalSymbol localSymbol2))
					{
						SetLocalScopes(localSymbol2, _localScopeDepth, valEscapeScope);
					}
				}
				return !flag;
			}
			finally
			{
				instance.Free();
			}
		}
	}

	private bool CheckInvocationArgMixingWithUpdatedRules(SyntaxNode syntax, ref readonly MethodInvocationInfo methodInvocationInfo, BindingDiagnosticBag diagnostics, Symbol symbolForReporting)
	{
		ArrayBuilder<MixableDestination> instance = ArrayBuilder<MixableDestination>.GetInstance();
		ArrayBuilder<EscapeValue> instance2 = ArrayBuilder<EscapeValue>.GetInstance();
		GetEscapeValuesForUpdatedRules(in methodInvocationInfo, ignoreArglistRefKinds: false, instance, instance2);
		bool flag = true;
		foreach (MixableDestination item in instance)
		{
			SafeContext valEscape = GetValEscape(item.Argument);
			foreach (var (parameter, boundExpression2, level, flag3) in instance2)
			{
				if (item.IsAssignableFrom(level))
				{
					flag = (flag3 ? CheckRefEscape(boundExpression2.Syntax, boundExpression2, valEscape, checkingReceiver: false, diagnostics) : CheckValEscape(boundExpression2.Syntax, boundExpression2, valEscape, checkingReceiver: false, diagnostics));
					if (!flag)
					{
						string invocationParameterName = GetInvocationParameterName(parameter);
						Error(diagnostics, ErrorCode.ERR_CallArgMixing, syntax, symbolForReporting, invocationParameterName);
						break;
					}
				}
			}
			if (!flag)
			{
				break;
			}
		}
		inferDeclarationExpressionValEscape(methodInvocationInfo.ArgsOpt, instance2);
		instance.Free();
		instance2.Free();
		return flag;
		void inferDeclarationExpressionValEscape(ImmutableArray<BoundExpression> argsOpt, ArrayBuilder<EscapeValue> escapeValues)
		{
			SafeContext valEscapeScope = SafeContext.CallingMethod;
			foreach (var (_, expr, _, flag5) in escapeValues)
			{
				valEscapeScope = valEscapeScope.Intersect(flag5 ? GetRefEscape(expr) : GetValEscape(expr));
			}
			foreach (BoundExpression item2 in argsOpt)
			{
				if (ShouldInferDeclarationExpressionValEscape(item2, out SourceLocalSymbol localSymbol))
				{
					SetLocalScopes(localSymbol, _localScopeDepth, valEscapeScope);
				}
			}
		}
	}

	private static ErrorCode GetStandardCallEscapeError(bool checkingReceiver)
	{
		if (!checkingReceiver)
		{
			return ErrorCode.ERR_EscapeCall;
		}
		return ErrorCode.ERR_EscapeCall2;
	}

	private static ErrorCode GetStandardRValueRefEscapeError(SafeContext escapeTo)
	{
		if (escapeTo.IsReturnable)
		{
			return ErrorCode.ERR_RefReturnLvalueExpected;
		}
		return ErrorCode.ERR_EscapeOther;
	}

	internal void ValidateEscape(BoundExpression expr, SafeContext escapeTo, bool isByRef, BindingDiagnosticBag diagnostics)
	{
		if (isByRef)
		{
			CheckRefEscape(expr.Syntax, expr, escapeTo, checkingReceiver: false, diagnostics);
		}
		else
		{
			CheckValEscape(expr.Syntax, expr, escapeTo, checkingReceiver: false, diagnostics);
		}
	}

	internal SafeContext GetRefEscape(BoundExpression expr)
	{
		if (expr.HasAnyErrors)
		{
			return SafeContext.CallingMethod;
		}
		TypeSymbol? type = expr.Type;
		if ((object)type != null && type.GetSpecialTypeSafe() == SpecialType.System_Void)
		{
			return SafeContext.CallingMethod;
		}
		if (expr.ConstantValueOpt != null)
		{
			return _localScopeDepth;
		}
		switch (expr.Kind)
		{
		case BoundKind.PointerIndirectionOperator:
		case BoundKind.PointerElementAccess:
		case BoundKind.ArrayAccess:
			return SafeContext.CallingMethod;
		case BoundKind.RefValueOperator:
			return SafeContext.CurrentMethod;
		case BoundKind.Parameter:
			return GetParameterRefEscape(((BoundParameter)expr).ParameterSymbol);
		case BoundKind.Local:
			return GetLocalScopes(((BoundLocal)expr).LocalSymbol).RefEscapeScope;
		case BoundKind.CapturedReceiverPlaceholder:
			return _localScopeDepth;
		case BoundKind.ThisReference:
			return GetParameterRefEscape(_symbol.ThisParameter);
		case BoundKind.ConditionalOperator:
		{
			BoundConditionalOperator boundConditionalOperator = (BoundConditionalOperator)expr;
			if (boundConditionalOperator.IsRef)
			{
				return GetRefEscape(boundConditionalOperator.Consequence).Intersect(GetRefEscape(boundConditionalOperator.Alternative));
			}
			break;
		}
		case BoundKind.FieldAccess:
			return GetFieldRefEscape((BoundFieldAccess)expr);
		case BoundKind.EventAccess:
		{
			BoundEventAccess boundEventAccess = (BoundEventAccess)expr;
			if (boundEventAccess.IsUsableAsField)
			{
				EventSymbol eventSymbol = boundEventAccess.EventSymbol;
				if (eventSymbol.IsStatic || eventSymbol.ContainingType.IsReferenceType)
				{
					return SafeContext.CallingMethod;
				}
				return GetRefEscape(boundEventAccess.ReceiverOpt);
			}
			break;
		}
		case BoundKind.Call:
		{
			BoundCall boundCall2 = (BoundCall)expr;
			if (boundCall2.IsErroneousNode)
			{
				return SafeContext.CallingMethod;
			}
			if (boundCall2.Method.RefKind != RefKind.None)
			{
				return GetInvocationEscapeScope(MethodInvocationInfo.FromCall(boundCall2), isRefEscape: true);
			}
			break;
		}
		case BoundKind.FunctionPointerInvocation:
		{
			BoundFunctionPointerInvocation boundFunctionPointerInvocation = (BoundFunctionPointerInvocation)expr;
			if (boundFunctionPointerInvocation.FunctionPointer.Signature.RefKind != RefKind.None)
			{
				return GetInvocationEscapeScope(MethodInvocationInfo.FromFunctionPointerInvocation(boundFunctionPointerInvocation), isRefEscape: true);
			}
			break;
		}
		case BoundKind.IndexerAccess:
		{
			BoundIndexerAccess boundIndexerAccess = (BoundIndexerAccess)expr;
			_ = boundIndexerAccess.Indexer;
			return GetInvocationEscapeScope(MethodInvocationInfo.FromIndexerAccess(boundIndexerAccess), isRefEscape: true);
		}
		case BoundKind.ImplicitIndexerAccess:
		{
			BoundImplicitIndexerAccess boundImplicitIndexerAccess = (BoundImplicitIndexerAccess)expr;
			BoundExpression indexerOrSliceAccess = boundImplicitIndexerAccess.IndexerOrSliceAccess;
			if (!(indexerOrSliceAccess is BoundIndexerAccess boundIndexerAccess2))
			{
				if (!(indexerOrSliceAccess is BoundArrayAccess))
				{
					if (indexerOrSliceAccess is BoundCall boundCall)
					{
						if (boundCall.IsErroneousNode)
						{
							return SafeContext.CallingMethod;
						}
						if (boundCall.Method.RefKind != RefKind.None)
						{
							return GetInvocationEscapeScope(MethodInvocationInfo.FromCall(boundCall, boundImplicitIndexerAccess.Receiver), isRefEscape: true);
						}
						break;
					}
					throw ExceptionUtilities.UnexpectedValue(boundImplicitIndexerAccess.IndexerOrSliceAccess.Kind);
				}
				return SafeContext.CallingMethod;
			}
			_ = boundIndexerAccess2.Indexer;
			return GetInvocationEscapeScope(MethodInvocationInfo.FromIndexerAccess(boundIndexerAccess2, boundImplicitIndexerAccess.Receiver), isRefEscape: true);
		}
		case BoundKind.InlineArrayAccess:
		{
			BoundInlineArrayAccess boundInlineArrayAccess = (BoundInlineArrayAccess)expr;
			WellKnownMember getItemOrSliceHelper = boundInlineArrayAccess.GetItemOrSliceHelper;
			bool flag = ((getItemOrSliceHelper == WellKnownMember.System_Span_T__get_Item || getItemOrSliceHelper == WellKnownMember.System_ReadOnlySpan_T__get_Item) ? true : false);
			if (flag && !boundInlineArrayAccess.IsValue)
			{
				SignatureOnlyMethodSymbol inlineArrayAccessEquivalentSignatureMethod = GetInlineArrayAccessEquivalentSignatureMethod(boundInlineArrayAccess, out var arguments, out var refKinds);
				return GetInvocationEscapeScope(MethodInvocationInfo.FromInlineArrayAccess(inlineArrayAccessEquivalentSignatureMethod, arguments, refKinds, boundInlineArrayAccess.HasAnyErrors), isRefEscape: true);
			}
			break;
		}
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess propertyAccess = (BoundPropertyAccess)expr;
			return GetInvocationEscapeScope(MethodInvocationInfo.FromProperty(propertyAccess), isRefEscape: true);
		}
		case BoundKind.AssignmentOperator:
		{
			BoundAssignmentOperator boundAssignmentOperator = (BoundAssignmentOperator)expr;
			if (boundAssignmentOperator.IsRef)
			{
				return GetRefEscape(boundAssignmentOperator.Right);
			}
			break;
		}
		}
		return _localScopeDepth;
	}

	internal bool CheckRefEscape(SyntaxNode node, BoundExpression expr, SafeContext escapeTo, bool checkingReceiver, BindingDiagnosticBag diagnostics)
	{
		if (_localScopeDepth.IsConvertibleTo(escapeTo))
		{
			return true;
		}
		if (expr.HasAnyErrors)
		{
			return true;
		}
		TypeSymbol? type = expr.Type;
		if ((object)type != null && type.GetSpecialTypeSafe() == SpecialType.System_Void)
		{
			return true;
		}
		if (expr.ConstantValueOpt != null)
		{
			Error(diagnostics, GetStandardRValueRefEscapeError(escapeTo), node);
			return false;
		}
		switch (expr.Kind)
		{
		case BoundKind.PointerIndirectionOperator:
		case BoundKind.PointerElementAccess:
		case BoundKind.ArrayAccess:
			return true;
		case BoundKind.RefValueOperator:
			if (!escapeTo.IsReturnable)
			{
				return true;
			}
			break;
		case BoundKind.Parameter:
		{
			BoundParameter boundParameter = (BoundParameter)expr;
			return CheckParameterRefEscape(node, boundParameter, boundParameter.ParameterSymbol, escapeTo, checkingReceiver, diagnostics);
		}
		case BoundKind.Local:
		{
			BoundLocal local = (BoundLocal)expr;
			return CheckLocalRefEscape(node, local, escapeTo, checkingReceiver, diagnostics);
		}
		case BoundKind.CapturedReceiverPlaceholder:
			if (_localScopeDepth.IsConvertibleTo(escapeTo))
			{
				return true;
			}
			break;
		case BoundKind.ThisReference:
		{
			ParameterSymbol thisParameter = _symbol.ThisParameter;
			return CheckParameterRefEscape(node, expr, thisParameter, escapeTo, checkingReceiver, diagnostics);
		}
		case BoundKind.ConditionalOperator:
		{
			BoundConditionalOperator boundConditionalOperator = (BoundConditionalOperator)expr;
			if (boundConditionalOperator.IsRef)
			{
				if (CheckRefEscape(boundConditionalOperator.Consequence.Syntax, boundConditionalOperator.Consequence, escapeTo, checkingReceiver: false, diagnostics))
				{
					return CheckRefEscape(boundConditionalOperator.Alternative.Syntax, boundConditionalOperator.Alternative, escapeTo, checkingReceiver: false, diagnostics);
				}
				return false;
			}
			break;
		}
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess fieldAccess = (BoundFieldAccess)expr;
			return CheckFieldRefEscape(node, fieldAccess, escapeTo, diagnostics);
		}
		case BoundKind.EventAccess:
		{
			BoundEventAccess boundEventAccess = (BoundEventAccess)expr;
			if (boundEventAccess.IsUsableAsField)
			{
				return CheckFieldLikeEventRefEscape(node, boundEventAccess, escapeTo, diagnostics);
			}
			break;
		}
		case BoundKind.Call:
		{
			BoundCall boundCall = (BoundCall)expr;
			if (boundCall.IsErroneousNode)
			{
				return true;
			}
			if (boundCall.Method.RefKind != RefKind.None)
			{
				return CheckInvocationEscape(boundCall.Syntax, MethodInvocationInfo.FromCall(boundCall), checkingReceiver, escapeTo, diagnostics, isRefEscape: true);
			}
			break;
		}
		case BoundKind.IndexerAccess:
		{
			BoundIndexerAccess boundIndexerAccess = (BoundIndexerAccess)expr;
			if (boundIndexerAccess.Indexer.RefKind != RefKind.None)
			{
				return CheckInvocationEscape(boundIndexerAccess.Syntax, MethodInvocationInfo.FromIndexerAccess(boundIndexerAccess), checkingReceiver, escapeTo, diagnostics, isRefEscape: true);
			}
			break;
		}
		case BoundKind.ImplicitIndexerAccess:
		{
			BoundImplicitIndexerAccess boundImplicitIndexerAccess = (BoundImplicitIndexerAccess)expr;
			BoundExpression indexerOrSliceAccess = boundImplicitIndexerAccess.IndexerOrSliceAccess;
			if (!(indexerOrSliceAccess is BoundIndexerAccess boundIndexerAccess2))
			{
				if (indexerOrSliceAccess is BoundArrayAccess)
				{
					return true;
				}
				if (!(indexerOrSliceAccess is BoundCall boundCall2))
				{
					throw ExceptionUtilities.UnexpectedValue(boundImplicitIndexerAccess.IndexerOrSliceAccess.Kind);
				}
				if (boundCall2.IsErroneousNode)
				{
					return true;
				}
				if (boundCall2.Method.RefKind != RefKind.None)
				{
					return CheckInvocationEscape(boundCall2.Syntax, MethodInvocationInfo.FromCall(boundCall2, boundImplicitIndexerAccess.Receiver), checkingReceiver, escapeTo, diagnostics, isRefEscape: true);
				}
			}
			else if (boundIndexerAccess2.Indexer.RefKind != RefKind.None)
			{
				return CheckInvocationEscape(boundIndexerAccess2.Syntax, MethodInvocationInfo.FromIndexerAccess(boundIndexerAccess2, boundImplicitIndexerAccess.Receiver), checkingReceiver, escapeTo, diagnostics, isRefEscape: true);
			}
			break;
		}
		case BoundKind.InlineArrayAccess:
		{
			BoundInlineArrayAccess boundInlineArrayAccess = (BoundInlineArrayAccess)expr;
			WellKnownMember getItemOrSliceHelper = boundInlineArrayAccess.GetItemOrSliceHelper;
			bool flag = ((getItemOrSliceHelper == WellKnownMember.System_Span_T__get_Item || getItemOrSliceHelper == WellKnownMember.System_ReadOnlySpan_T__get_Item) ? true : false);
			if (flag && !boundInlineArrayAccess.IsValue)
			{
				SignatureOnlyMethodSymbol inlineArrayAccessEquivalentSignatureMethod = GetInlineArrayAccessEquivalentSignatureMethod(boundInlineArrayAccess, out var arguments, out var refKinds);
				return CheckInvocationEscape(boundInlineArrayAccess.Syntax, MethodInvocationInfo.FromInlineArrayAccess(inlineArrayAccessEquivalentSignatureMethod, arguments, refKinds, boundInlineArrayAccess.HasAnyErrors), checkingReceiver, escapeTo, diagnostics, isRefEscape: true);
			}
			break;
		}
		case BoundKind.FunctionPointerInvocation:
		{
			BoundFunctionPointerInvocation boundFunctionPointerInvocation = (BoundFunctionPointerInvocation)expr;
			if (boundFunctionPointerInvocation.FunctionPointer.Signature.RefKind != RefKind.None)
			{
				return CheckInvocationEscape(boundFunctionPointerInvocation.Syntax, MethodInvocationInfo.FromFunctionPointerInvocation(boundFunctionPointerInvocation), checkingReceiver, escapeTo, diagnostics, isRefEscape: true);
			}
			break;
		}
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)expr;
			if (boundPropertyAccess.PropertySymbol.RefKind != RefKind.None)
			{
				return CheckInvocationEscape(boundPropertyAccess.Syntax, MethodInvocationInfo.FromProperty(boundPropertyAccess), checkingReceiver, escapeTo, diagnostics, isRefEscape: true);
			}
			break;
		}
		case BoundKind.AssignmentOperator:
		{
			BoundAssignmentOperator boundAssignmentOperator = (BoundAssignmentOperator)expr;
			if (boundAssignmentOperator.IsRef)
			{
				return CheckRefEscape(node, boundAssignmentOperator.Right, escapeTo, checkingReceiver: false, diagnostics);
			}
			break;
		}
		case BoundKind.Conversion:
		{
			BoundConversion boundConversion = (BoundConversion)expr;
			if (boundConversion.Conversion == Conversion.ImplicitThrow)
			{
				return CheckRefEscape(node, boundConversion.Operand, escapeTo, checkingReceiver, diagnostics);
			}
			break;
		}
		case BoundKind.ThrowExpression:
			return true;
		}
		Error(diagnostics, GetStandardRValueRefEscapeError(escapeTo), node);
		return false;
	}

	internal SafeContext GetBroadestValEscape(BoundTupleExpression expr)
	{
		SafeContext result = _localScopeDepth;
		foreach (BoundExpression argument in expr.Arguments)
		{
			SafeContext other = ((!(argument is BoundTupleExpression expr2)) ? GetValEscape(argument) : GetBroadestValEscape(expr2));
			result = result.Union(other);
		}
		return result;
	}

	internal SafeContext GetValEscape(BoundExpression expr)
	{
		if (expr.HasAnyErrors)
		{
			return SafeContext.CallingMethod;
		}
		if (expr.ConstantValueOpt != null)
		{
			return SafeContext.CallingMethod;
		}
		TypeSymbol? type = expr.Type;
		if ((object)type == null || !type.IsRefLikeOrAllowsRefLikeType())
		{
			return SafeContext.CallingMethod;
		}
		switch (expr.Kind)
		{
		case BoundKind.ThisReference:
			return GetParameterValEscape(_symbol.ThisParameter);
		case BoundKind.DefaultLiteral:
		case BoundKind.DefaultExpression:
		case BoundKind.Utf8String:
			return SafeContext.CallingMethod;
		case BoundKind.Parameter:
			return GetParameterValEscape(((BoundParameter)expr).ParameterSymbol);
		case BoundKind.FromEndIndexExpression:
			return SafeContext.CallingMethod;
		case BoundKind.TupleLiteral:
		case BoundKind.ConvertedTupleLiteral:
		{
			BoundTupleExpression boundTupleExpression = (BoundTupleExpression)expr;
			return GetTupleValEscape(boundTupleExpression.Arguments);
		}
		case BoundKind.MakeRefOperator:
		case BoundKind.RefValueOperator:
			return SafeContext.CallingMethod;
		case BoundKind.DiscardExpression:
			return SafeContext.CallingMethod;
		case BoundKind.DeconstructValuePlaceholder:
		case BoundKind.AwaitableValuePlaceholder:
		case BoundKind.InterpolatedStringArgumentPlaceholder:
			return GetPlaceholderScope((BoundValuePlaceholderBase)expr);
		case BoundKind.Local:
			return GetLocalScopes(((BoundLocal)expr).LocalSymbol).ValEscapeScope;
		case BoundKind.CapturedReceiverPlaceholder:
		{
			BoundCapturedReceiverPlaceholder boundCapturedReceiverPlaceholder = (BoundCapturedReceiverPlaceholder)expr;
			return GetValEscape(boundCapturedReceiverPlaceholder.Receiver);
		}
		case BoundKind.StackAllocArrayCreation:
		case BoundKind.ConvertedStackAllocExpression:
			return SafeContext.CurrentMethod;
		case BoundKind.ConditionalOperator:
		{
			BoundConditionalOperator boundConditionalOperator = (BoundConditionalOperator)expr;
			SafeContext valEscape = GetValEscape(boundConditionalOperator.Consequence);
			if (boundConditionalOperator.IsRef)
			{
				return valEscape;
			}
			return valEscape.Intersect(GetValEscape(boundConditionalOperator.Alternative));
		}
		case BoundKind.NullCoalescingOperator:
		{
			BoundNullCoalescingOperator boundNullCoalescingOperator = (BoundNullCoalescingOperator)expr;
			return GetValEscape(boundNullCoalescingOperator.LeftOperand).Intersect(GetValEscape(boundNullCoalescingOperator.RightOperand));
		}
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)expr;
			FieldSymbol fieldSymbol = boundFieldAccess.FieldSymbol;
			if (fieldSymbol.IsStatic || !fieldSymbol.ContainingType.IsRefLikeType)
			{
				return SafeContext.CallingMethod;
			}
			return GetValEscape(boundFieldAccess.ReceiverOpt);
		}
		case BoundKind.Call:
		{
			BoundCall boundCall = (BoundCall)expr;
			if (boundCall.IsErroneousNode)
			{
				return SafeContext.CallingMethod;
			}
			return GetInvocationEscapeScope(MethodInvocationInfo.FromCall(boundCall), isRefEscape: false);
		}
		case BoundKind.FunctionPointerInvocation:
		{
			BoundFunctionPointerInvocation boundFunctionPointerInvocation = (BoundFunctionPointerInvocation)expr;
			_ = boundFunctionPointerInvocation.FunctionPointer.Signature;
			return GetInvocationEscapeScope(MethodInvocationInfo.FromFunctionPointerInvocation(boundFunctionPointerInvocation), isRefEscape: false);
		}
		case BoundKind.IndexerAccess:
		{
			BoundIndexerAccess boundIndexerAccess = (BoundIndexerAccess)expr;
			_ = boundIndexerAccess.Indexer;
			return GetInvocationEscapeScope(MethodInvocationInfo.FromIndexerAccess(boundIndexerAccess), isRefEscape: false);
		}
		case BoundKind.ImplicitIndexerAccess:
		{
			BoundImplicitIndexerAccess boundImplicitIndexerAccess = (BoundImplicitIndexerAccess)expr;
			BoundExpression indexerOrSliceAccess = boundImplicitIndexerAccess.IndexerOrSliceAccess;
			if (!(indexerOrSliceAccess is BoundIndexerAccess boundIndexerAccess2))
			{
				if (!(indexerOrSliceAccess is BoundArrayAccess))
				{
					if (indexerOrSliceAccess is BoundCall boundCall2)
					{
						if (boundCall2.IsErroneousNode)
						{
							return SafeContext.CallingMethod;
						}
						return GetInvocationEscapeScope(MethodInvocationInfo.FromCall(boundCall2, boundImplicitIndexerAccess.Receiver), isRefEscape: false);
					}
					throw ExceptionUtilities.UnexpectedValue(boundImplicitIndexerAccess.IndexerOrSliceAccess.Kind);
				}
				return _localScopeDepth;
			}
			_ = boundIndexerAccess2.Indexer;
			return GetInvocationEscapeScope(MethodInvocationInfo.FromIndexerAccess(boundIndexerAccess2, boundImplicitIndexerAccess.Receiver), isRefEscape: false);
		}
		case BoundKind.InlineArrayAccess:
		{
			BoundInlineArrayAccess boundInlineArrayAccess = (BoundInlineArrayAccess)expr;
			SignatureOnlyMethodSymbol inlineArrayAccessEquivalentSignatureMethod = GetInlineArrayAccessEquivalentSignatureMethod(boundInlineArrayAccess, out var arguments, out var refKinds);
			return GetInvocationEscapeScope(MethodInvocationInfo.FromInlineArrayAccess(inlineArrayAccessEquivalentSignatureMethod, arguments, refKinds, boundInlineArrayAccess.HasAnyErrors), isRefEscape: false);
		}
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess propertyAccess = (BoundPropertyAccess)expr;
			return GetInvocationEscapeScope(MethodInvocationInfo.FromProperty(propertyAccess), isRefEscape: false);
		}
		case BoundKind.ObjectCreationExpression:
		{
			BoundObjectCreationExpression boundObjectCreationExpression = (BoundObjectCreationExpression)expr;
			_ = boundObjectCreationExpression.Constructor;
			SafeContext result2 = GetInvocationEscapeScope(MethodInvocationInfo.FromObjectCreation(boundObjectCreationExpression), isRefEscape: false);
			BoundObjectInitializerExpressionBase initializerExpressionOpt2 = boundObjectCreationExpression.InitializerExpressionOpt;
			if (initializerExpressionOpt2 != null)
			{
				result2 = result2.Intersect(GetValEscape(initializerExpressionOpt2));
			}
			return result2;
		}
		case BoundKind.NewT:
		{
			BoundNewT obj = (BoundNewT)expr;
			SafeContext result = SafeContext.CallingMethod;
			BoundObjectInitializerExpressionBase initializerExpressionOpt = obj.InitializerExpressionOpt;
			if (initializerExpressionOpt != null)
			{
				result = result.Intersect(GetValEscape(initializerExpressionOpt));
			}
			return result;
		}
		case BoundKind.WithExpression:
		{
			BoundWithExpression boundWithExpression = (BoundWithExpression)expr;
			return GetValEscape(boundWithExpression.Receiver).Intersect(GetValEscape(boundWithExpression.InitializerExpression));
		}
		case BoundKind.UnaryOperator:
		{
			BoundUnaryOperator boundUnaryOperator = (BoundUnaryOperator)expr;
			if ((object)boundUnaryOperator.MethodOpt != null)
			{
				return GetInvocationEscapeScope(MethodInvocationInfo.FromUnaryOperator(boundUnaryOperator), isRefEscape: false);
			}
			return GetValEscape(boundUnaryOperator.Operand);
		}
		case BoundKind.Conversion:
		{
			BoundConversion boundConversion = (BoundConversion)expr;
			if (boundConversion.ConversionKind == ConversionKind.InterpolatedStringHandler)
			{
				return GetInterpolatedStringHandlerConversionEscapeScope(boundConversion.Operand);
			}
			if (boundConversion.ConversionKind == ConversionKind.CollectionExpression)
			{
				if (!HasLocalScope((BoundCollectionExpression)boundConversion.Operand))
				{
					return SafeContext.CallingMethod;
				}
				return _localScopeDepth;
			}
			if (boundConversion.Conversion.IsInlineArray)
			{
				SignatureOnlyMethodSymbol inlineArrayConversionEquivalentSignatureMethod = GetInlineArrayConversionEquivalentSignatureMethod(boundConversion, out var arguments2, out var refKinds2);
				return GetInvocationEscapeScope(MethodInvocationInfo.FromInlineArrayConversion(inlineArrayConversionEquivalentSignatureMethod, arguments2, refKinds2, boundConversion.HasAnyErrors), isRefEscape: false);
			}
			if (boundConversion.Conversion.IsUserDefined)
			{
				MethodSymbol method2 = boundConversion.Conversion.Method;
				return GetInvocationEscapeScope(MethodInvocationInfo.FromUserDefinedConversion(method2, boundConversion.Operand, boundConversion.HasAnyErrors), isRefEscape: false);
			}
			return GetValEscape(boundConversion.Operand);
		}
		case BoundKind.AssignmentOperator:
			return GetValEscape(((BoundAssignmentOperator)expr).Right);
		case BoundKind.NullCoalescingAssignmentOperator:
		{
			BoundNullCoalescingAssignmentOperator boundNullCoalescingAssignmentOperator = (BoundNullCoalescingAssignmentOperator)expr;
			return GetValEscape(boundNullCoalescingAssignmentOperator.LeftOperand).Intersect(GetValEscape(boundNullCoalescingAssignmentOperator.RightOperand));
		}
		case BoundKind.IncrementOperator:
		{
			BoundIncrementOperator boundIncrementOperator = (BoundIncrementOperator)expr;
			MethodSymbol methodOpt = boundIncrementOperator.MethodOpt;
			if ((object)methodOpt != null && methodOpt.IsStatic)
			{
				UnaryOperatorKind unaryOperatorKind = boundIncrementOperator.OperatorKind.Operator();
				if ((unaryOperatorKind == UnaryOperatorKind.PrefixIncrement || unaryOperatorKind == UnaryOperatorKind.PrefixDecrement) ? true : false)
				{
					return GetInvocationEscapeScope(MethodInvocationInfo.FromIncrementOperator(boundIncrementOperator), isRefEscape: false);
				}
			}
			return GetValEscape(boundIncrementOperator.Operand);
		}
		case BoundKind.CompoundAssignmentOperator:
		{
			BoundCompoundAssignmentOperator boundCompoundAssignmentOperator = (BoundCompoundAssignmentOperator)expr;
			MethodSymbol method = boundCompoundAssignmentOperator.Operator.Method;
			if ((object)method != null)
			{
				if (method.IsStatic)
				{
					return GetInvocationEscapeScope(MethodInvocationInfo.FromCompoundAssignmentOperator(boundCompoundAssignmentOperator), isRefEscape: false);
				}
				return GetValEscape(boundCompoundAssignmentOperator.Left);
			}
			return GetValEscape(boundCompoundAssignmentOperator.Left).Intersect(GetValEscape(boundCompoundAssignmentOperator.Right));
		}
		case BoundKind.BinaryOperator:
		{
			BoundBinaryOperator boundBinaryOperator = (BoundBinaryOperator)expr;
			if ((object)boundBinaryOperator.BinaryOperatorMethod != null)
			{
				return GetInvocationEscapeScope(MethodInvocationInfo.FromBinaryOperator(boundBinaryOperator), isRefEscape: false);
			}
			return GetValEscape(boundBinaryOperator.Left).Intersect(GetValEscape(boundBinaryOperator.Right));
		}
		case BoundKind.RangeExpression:
		{
			BoundRangeExpression boundRangeExpression = (BoundRangeExpression)expr;
			BoundExpression leftOperandOpt = boundRangeExpression.LeftOperandOpt;
			SafeContext safeContext = ((leftOperandOpt != null) ? GetValEscape(leftOperandOpt) : SafeContext.CallingMethod);
			BoundExpression rightOperandOpt = boundRangeExpression.RightOperandOpt;
			return safeContext.Intersect((rightOperandOpt != null) ? GetValEscape(rightOperandOpt) : SafeContext.CallingMethod);
		}
		case BoundKind.UserDefinedConditionalLogicalOperator:
		{
			BoundUserDefinedConditionalLogicalOperator logicalOperator = (BoundUserDefinedConditionalLogicalOperator)expr;
			return GetInvocationEscapeScope(MethodInvocationInfo.FromUserDefinedConditionalLogicalOperator(logicalOperator), isRefEscape: false);
		}
		case BoundKind.QueryClause:
			return GetValEscape(((BoundQueryClause)expr).Value);
		case BoundKind.RangeVariable:
			return GetValEscape(((BoundRangeVariable)expr).Value);
		case BoundKind.ObjectInitializerExpression:
		{
			BoundObjectInitializerExpression initExpr = (BoundObjectInitializerExpression)expr;
			return GetValEscapeOfObjectInitializer(initExpr);
		}
		case BoundKind.CollectionInitializerExpression:
		{
			BoundCollectionInitializerExpression colExpr = (BoundCollectionInitializerExpression)expr;
			return GetValEscapeOfCollectionInitializer(colExpr);
		}
		case BoundKind.ObjectInitializerMember:
			return _localScopeDepth;
		case BoundKind.ObjectOrCollectionValuePlaceholder:
		case BoundKind.ImplicitReceiver:
			return _localScopeDepth;
		case BoundKind.InterpolatedStringHandlerPlaceholder:
			return _localScopeDepth;
		case BoundKind.DisposableValuePlaceholder:
			return _localScopeDepth;
		case BoundKind.PointerIndirectionOperator:
		case BoundKind.PointerElementAccess:
			return SafeContext.CallingMethod;
		case BoundKind.ArrayAccess:
		case BoundKind.AwaitExpression:
		case BoundKind.AsOperator:
		case BoundKind.ConditionalAccess:
		case BoundKind.ConditionalReceiver:
			return _localScopeDepth;
		case BoundKind.ArgList:
			return _localScopeDepth;
		case BoundKind.UnconvertedSwitchExpression:
		case BoundKind.ConvertedSwitchExpression:
		{
			BoundSwitchExpression boundSwitchExpression = (BoundSwitchExpression)expr;
			return GetValEscape(boundSwitchExpression.SwitchArms.SelectAsArray((BoundSwitchExpressionArm a) => a.Value));
		}
		default:
			return _localScopeDepth;
		}
	}

	private bool HasLocalScope(BoundCollectionExpression expr)
	{
		TypeSymbol type = expr.Type;
		if ((object)type == null || !type.IsRefLikeType || expr.Elements.Length == 0)
		{
			return false;
		}
		CollectionExpressionTypeKind collectionExpressionTypeKind = ConversionsBase.GetCollectionExpressionTypeKind(_compilation, expr.Type, out var elementType);
		switch (collectionExpressionTypeKind)
		{
		case CollectionExpressionTypeKind.ReadOnlySpan:
			return !LocalRewriter.ShouldUseRuntimeHelpersCreateSpan(expr, elementType.Type);
		case CollectionExpressionTypeKind.Span:
			return true;
		case CollectionExpressionTypeKind.CollectionBuilder:
		{
			MethodSymbol collectionBuilderMethod = expr.CollectionBuilderMethod;
			if ((object)collectionBuilderMethod != null)
			{
				ImmutableArray<ParameterSymbol> parameters = collectionBuilderMethod.Parameters;
				if (parameters.Length == 1)
				{
					ParameterSymbol parameterSymbol = parameters[0];
					if ((object)parameterSymbol != null && parameterSymbol.RefKind == RefKind.None)
					{
						if (parameterSymbol.EffectiveScope == ScopedKind.ScopedValue)
						{
							return false;
						}
						if (LocalRewriter.ShouldUseRuntimeHelpersCreateSpan(expr, ((NamedTypeSymbol)parameterSymbol.Type).TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[0].Type))
						{
							return false;
						}
						return true;
					}
				}
			}
			return true;
		}
		case CollectionExpressionTypeKind.ImplementsIEnumerable:
			return true;
		default:
			throw ExceptionUtilities.UnexpectedValue(collectionExpressionTypeKind);
		}
	}

	private SafeContext GetTupleValEscape(ImmutableArray<BoundExpression> elements)
	{
		SafeContext result = _localScopeDepth;
		foreach (BoundExpression item in elements)
		{
			result = result.Intersect(GetValEscape(item));
		}
		return result;
	}

	private SafeContext GetValEscapeOfCollectionInitializer(BoundCollectionInitializerExpression colExpr)
	{
		SafeContext result = SafeContext.CallingMethod;
		foreach (BoundExpression initializer in colExpr.Initializers)
		{
			result = result.Intersect((initializer is BoundCollectionElementInitializer colElement) ? GetInvocationEscapeToReceiver(MethodInvocationInfo.FromCollectionElementInitializer(colElement)) : GetValEscape(initializer));
		}
		return result;
	}

	private SafeContext GetValEscapeOfObjectInitializer(BoundObjectInitializerExpression initExpr)
	{
		SafeContext result = SafeContext.CallingMethod;
		foreach (BoundExpression initializer in initExpr.Initializers)
		{
			SafeContext valEscapeOfObjectMemberInitializer = GetValEscapeOfObjectMemberInitializer(initializer);
			result = result.Intersect(valEscapeOfObjectMemberInitializer);
		}
		return result;
	}

	private SafeContext GetValEscapeOfObjectMemberInitializer(BoundExpression expr)
	{
		if (expr.Kind == BoundKind.AssignmentOperator)
		{
			BoundAssignmentOperator boundAssignmentOperator = (BoundAssignmentOperator)expr;
			SafeContext safeContext = (boundAssignmentOperator.IsRef ? GetRefEscape(boundAssignmentOperator.Right) : GetValEscape(boundAssignmentOperator.Right));
			if (boundAssignmentOperator.Left is BoundObjectInitializerMember boundObjectInitializerMember)
			{
				return (!(boundObjectInitializerMember.MemberSymbol is PropertySymbol propertySymbol)) ? safeContext : ((!propertySymbol.IsIndexer) ? getPropertyEscape(propertySymbol, safeContext) : getIndexerEscape(propertySymbol, boundObjectInitializerMember, safeContext));
			}
			return safeContext;
		}
		return GetValEscape(expr);
		SafeContext getIndexerEscape(PropertySymbol indexer, BoundObjectInitializerMember boundObjectInitializerMember2, SafeContext rightEscapeScope)
		{
			MethodInfo methodInfo = MethodInfo.Create(indexer, boundObjectInitializerMember2.AccessorKind);
			if ((object)methodInfo.Method == null)
			{
				return SafeContext.CallingMethod;
			}
			if (methodInfo.Method.IsEffectivelyReadOnly)
			{
				return SafeContext.CallingMethod;
			}
			ArrayBuilder<EscapeValue> instance = ArrayBuilder<EscapeValue>.GetInstance();
			GetEscapeValues(new MethodInvocationInfo
			{
				MethodInfo = methodInfo,
				Receiver = null,
				ReceiverIsSubjectToCloning = ThreeState.Unknown,
				Parameters = methodInfo.Method.Parameters,
				ArgsOpt = boundObjectInitializerMember2.Arguments,
				ArgumentRefKindsOpt = boundObjectInitializerMember2.ArgumentRefKindsOpt,
				ArgsToParamsOpt = boundObjectInitializerMember2.ArgsToParamsOpt,
				HasAnyErrors = boundObjectInitializerMember2.HasAnyErrors
			}, ignoreArglistRefKinds: true, null, instance);
			SafeContext other = SafeContext.CallingMethod;
			foreach (EscapeValue item in instance)
			{
				if (!item.IsRefEscape || item.EscapeLevel == EscapeLevel.CallingMethod)
				{
					other = (item.IsRefEscape ? GetRefEscape(item.Argument) : GetValEscape(item.Argument)).Intersect(other);
				}
			}
			instance.Free();
			return other.Intersect(rightEscapeScope);
		}
		static SafeContext getPropertyEscape(PropertySymbol property, SafeContext rightEscapeScope)
		{
			AccessorKind accessorKind = ((property.RefKind != RefKind.None) ? AccessorKind.Get : AccessorKind.Set);
			MethodInfo methodInfo = MethodInfo.Create(property, accessorKind);
			if ((object)methodInfo.Method == null || methodInfo.Method.IsEffectivelyReadOnly)
			{
				return SafeContext.CallingMethod;
			}
			return rightEscapeScope;
		}
	}

	private SafeContext GetValEscape(ImmutableArray<BoundExpression> expressions)
	{
		SafeContext result = SafeContext.CallingMethod;
		foreach (BoundExpression item in expressions)
		{
			result = result.Intersect(GetValEscape(item));
		}
		return result;
	}

	internal bool CheckValEscape(SyntaxNode node, BoundExpression expr, SafeContext escapeTo, bool checkingReceiver, BindingDiagnosticBag diagnostics)
	{
		if (_localScopeDepth.IsConvertibleTo(escapeTo))
		{
			return true;
		}
		if (expr.HasAnyErrors)
		{
			return true;
		}
		if (expr.ConstantValueOpt != null)
		{
			return true;
		}
		TypeSymbol? type = expr.Type;
		if ((object)type == null || !type.IsRefLikeOrAllowsRefLikeType())
		{
			return true;
		}
		bool inUnsafeRegion = _inUnsafeRegion;
		switch (expr.Kind)
		{
		case BoundKind.ThisReference:
		{
			ParameterSymbol thisParameter = _symbol.ThisParameter;
			return CheckParameterValEscape(node, thisParameter, escapeTo, diagnostics);
		}
		case BoundKind.DefaultLiteral:
		case BoundKind.DefaultExpression:
		case BoundKind.Utf8String:
			return true;
		case BoundKind.Parameter:
			return CheckParameterValEscape(node, ((BoundParameter)expr).ParameterSymbol, escapeTo, diagnostics);
		case BoundKind.TupleLiteral:
		case BoundKind.ConvertedTupleLiteral:
		{
			BoundTupleExpression boundTupleExpression = (BoundTupleExpression)expr;
			return CheckTupleValEscape(boundTupleExpression.Arguments, escapeTo, diagnostics);
		}
		case BoundKind.MakeRefOperator:
		case BoundKind.RefValueOperator:
			return true;
		case BoundKind.DiscardExpression:
			return true;
		case BoundKind.DeconstructValuePlaceholder:
		case BoundKind.AwaitableValuePlaceholder:
		case BoundKind.InterpolatedStringArgumentPlaceholder:
			if (!GetPlaceholderScope((BoundValuePlaceholderBase)expr).IsConvertibleTo(escapeTo))
			{
				Error(diagnostics, inUnsafeRegion ? ErrorCode.WRN_EscapeVariable : ErrorCode.ERR_EscapeVariable, node, expr.Syntax);
				return inUnsafeRegion;
			}
			return true;
		case BoundKind.Local:
		{
			LocalSymbol localSymbol = ((BoundLocal)expr).LocalSymbol;
			if (!GetLocalScopes(localSymbol).ValEscapeScope.IsConvertibleTo(escapeTo))
			{
				Error(diagnostics, inUnsafeRegion ? ErrorCode.WRN_EscapeVariable : ErrorCode.ERR_EscapeVariable, node, localSymbol);
				return inUnsafeRegion;
			}
			return true;
		}
		case BoundKind.CapturedReceiverPlaceholder:
		{
			BoundExpression receiver = ((BoundCapturedReceiverPlaceholder)expr).Receiver;
			return CheckValEscape(receiver.Syntax, receiver, escapeTo, checkingReceiver, diagnostics);
		}
		case BoundKind.StackAllocArrayCreation:
		case BoundKind.ConvertedStackAllocExpression:
			if (!SafeContext.CurrentMethod.IsConvertibleTo(escapeTo))
			{
				Error(diagnostics, inUnsafeRegion ? ErrorCode.WRN_EscapeStackAlloc : ErrorCode.ERR_EscapeStackAlloc, node, expr.Type);
				return inUnsafeRegion;
			}
			return true;
		case BoundKind.UnconvertedConditionalOperator:
		{
			BoundUnconvertedConditionalOperator boundUnconvertedConditionalOperator = (BoundUnconvertedConditionalOperator)expr;
			if (CheckValEscape(boundUnconvertedConditionalOperator.Consequence.Syntax, boundUnconvertedConditionalOperator.Consequence, escapeTo, checkingReceiver: false, diagnostics))
			{
				return CheckValEscape(boundUnconvertedConditionalOperator.Alternative.Syntax, boundUnconvertedConditionalOperator.Alternative, escapeTo, checkingReceiver: false, diagnostics);
			}
			return false;
		}
		case BoundKind.ConditionalOperator:
		{
			BoundConditionalOperator boundConditionalOperator = (BoundConditionalOperator)expr;
			bool flag2 = CheckValEscape(boundConditionalOperator.Consequence.Syntax, boundConditionalOperator.Consequence, escapeTo, checkingReceiver: false, diagnostics);
			if (!flag2 || boundConditionalOperator.IsRef)
			{
				return flag2;
			}
			return CheckValEscape(boundConditionalOperator.Alternative.Syntax, boundConditionalOperator.Alternative, escapeTo, checkingReceiver: false, diagnostics);
		}
		case BoundKind.NullCoalescingOperator:
		{
			BoundNullCoalescingOperator boundNullCoalescingOperator = (BoundNullCoalescingOperator)expr;
			if (CheckValEscape(boundNullCoalescingOperator.LeftOperand.Syntax, boundNullCoalescingOperator.LeftOperand, escapeTo, checkingReceiver, diagnostics))
			{
				return CheckValEscape(boundNullCoalescingOperator.RightOperand.Syntax, boundNullCoalescingOperator.RightOperand, escapeTo, checkingReceiver, diagnostics);
			}
			return false;
		}
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)expr;
			FieldSymbol fieldSymbol = boundFieldAccess.FieldSymbol;
			if (fieldSymbol.IsStatic || !fieldSymbol.ContainingType.IsRefLikeType)
			{
				return true;
			}
			return CheckValEscape(node, boundFieldAccess.ReceiverOpt, escapeTo, checkingReceiver: true, diagnostics);
		}
		case BoundKind.Call:
		{
			BoundCall boundCall2 = (BoundCall)expr;
			if (boundCall2.IsErroneousNode)
			{
				return true;
			}
			_ = boundCall2.Method;
			return CheckInvocationEscape(boundCall2.Syntax, MethodInvocationInfo.FromCall(boundCall2), checkingReceiver, escapeTo, diagnostics, isRefEscape: false);
		}
		case BoundKind.FunctionPointerInvocation:
		{
			BoundFunctionPointerInvocation boundFunctionPointerInvocation = (BoundFunctionPointerInvocation)expr;
			_ = boundFunctionPointerInvocation.FunctionPointer.Signature;
			return CheckInvocationEscape(boundFunctionPointerInvocation.Syntax, MethodInvocationInfo.FromFunctionPointerInvocation(boundFunctionPointerInvocation), checkingReceiver, escapeTo, diagnostics, isRefEscape: false);
		}
		case BoundKind.IndexerAccess:
		{
			BoundIndexerAccess boundIndexerAccess2 = (BoundIndexerAccess)expr;
			_ = boundIndexerAccess2.Indexer;
			return CheckInvocationEscape(boundIndexerAccess2.Syntax, MethodInvocationInfo.FromIndexerAccess(boundIndexerAccess2), checkingReceiver, escapeTo, diagnostics, isRefEscape: false);
		}
		case BoundKind.ImplicitIndexerAccess:
		{
			BoundImplicitIndexerAccess boundImplicitIndexerAccess = (BoundImplicitIndexerAccess)expr;
			BoundExpression indexerOrSliceAccess = boundImplicitIndexerAccess.IndexerOrSliceAccess;
			if (!(indexerOrSliceAccess is BoundIndexerAccess boundIndexerAccess))
			{
				if (!(indexerOrSliceAccess is BoundArrayAccess))
				{
					if (indexerOrSliceAccess is BoundCall boundCall)
					{
						if (boundCall.IsErroneousNode)
						{
							return true;
						}
						_ = boundCall.Method;
						return CheckInvocationEscape(boundCall.Syntax, MethodInvocationInfo.FromCall(boundCall, boundImplicitIndexerAccess.Receiver), checkingReceiver, escapeTo, diagnostics, isRefEscape: false);
					}
					throw ExceptionUtilities.UnexpectedValue(boundImplicitIndexerAccess.IndexerOrSliceAccess.Kind);
				}
				return false;
			}
			_ = boundIndexerAccess.Indexer;
			return CheckInvocationEscape(boundIndexerAccess.Syntax, MethodInvocationInfo.FromIndexerAccess(boundIndexerAccess, boundImplicitIndexerAccess.Receiver), checkingReceiver, escapeTo, diagnostics, isRefEscape: false);
		}
		case BoundKind.InlineArrayAccess:
		{
			BoundInlineArrayAccess boundInlineArrayAccess = (BoundInlineArrayAccess)expr;
			SignatureOnlyMethodSymbol inlineArrayAccessEquivalentSignatureMethod = GetInlineArrayAccessEquivalentSignatureMethod(boundInlineArrayAccess, out var arguments2, out var refKinds2);
			return CheckInvocationEscape(boundInlineArrayAccess.Syntax, MethodInvocationInfo.FromInlineArrayAccess(inlineArrayAccessEquivalentSignatureMethod, arguments2, refKinds2, boundInlineArrayAccess.HasAnyErrors), checkingReceiver, escapeTo, diagnostics, isRefEscape: false);
		}
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)expr;
			return CheckInvocationEscape(boundPropertyAccess.Syntax, MethodInvocationInfo.FromProperty(boundPropertyAccess), checkingReceiver, escapeTo, diagnostics, isRefEscape: false);
		}
		case BoundKind.ObjectCreationExpression:
		{
			BoundObjectCreationExpression boundObjectCreationExpression = (BoundObjectCreationExpression)expr;
			_ = boundObjectCreationExpression.Constructor;
			bool flag = CheckInvocationEscape(boundObjectCreationExpression.Syntax, MethodInvocationInfo.FromObjectCreation(boundObjectCreationExpression), checkingReceiver, escapeTo, diagnostics, isRefEscape: false);
			BoundObjectInitializerExpressionBase initializerExpressionOpt = boundObjectCreationExpression.InitializerExpressionOpt;
			if (initializerExpressionOpt != null)
			{
				flag = flag && CheckValEscape(initializerExpressionOpt.Syntax, initializerExpressionOpt, escapeTo, checkingReceiver: false, diagnostics);
			}
			return flag;
		}
		case BoundKind.NewT:
		{
			BoundNewT obj = (BoundNewT)expr;
			bool flag3 = true;
			BoundObjectInitializerExpressionBase initializerExpressionOpt2 = obj.InitializerExpressionOpt;
			if (initializerExpressionOpt2 != null)
			{
				flag3 = flag3 && CheckValEscape(initializerExpressionOpt2.Syntax, initializerExpressionOpt2, escapeTo, checkingReceiver: false, diagnostics);
			}
			return flag3;
		}
		case BoundKind.WithExpression:
		{
			BoundWithExpression boundWithExpression = (BoundWithExpression)expr;
			bool num = CheckValEscape(node, boundWithExpression.Receiver, escapeTo, checkingReceiver: false, diagnostics);
			BoundObjectInitializerExpressionBase initializerExpression = boundWithExpression.InitializerExpression;
			if (num)
			{
				return CheckValEscape(initializerExpression.Syntax, initializerExpression, escapeTo, checkingReceiver: false, diagnostics);
			}
			return false;
		}
		case BoundKind.UnaryOperator:
		{
			BoundUnaryOperator boundUnaryOperator = (BoundUnaryOperator)expr;
			if ((object)boundUnaryOperator.MethodOpt != null)
			{
				return CheckInvocationEscape(boundUnaryOperator.Syntax, MethodInvocationInfo.FromUnaryOperator(boundUnaryOperator), checkingReceiver, escapeTo, diagnostics, isRefEscape: false);
			}
			return CheckValEscape(node, boundUnaryOperator.Operand, escapeTo, checkingReceiver: false, diagnostics);
		}
		case BoundKind.FromEndIndexExpression:
			return true;
		case BoundKind.Conversion:
		{
			BoundConversion boundConversion = (BoundConversion)expr;
			if (boundConversion.ConversionKind == ConversionKind.InterpolatedStringHandler)
			{
				return CheckInterpolatedStringHandlerConversionEscape(boundConversion.Operand, escapeTo, diagnostics);
			}
			if (boundConversion.ConversionKind == ConversionKind.CollectionExpression)
			{
				if (HasLocalScope((BoundCollectionExpression)boundConversion.Operand) && !_localScopeDepth.IsConvertibleTo(escapeTo))
				{
					Error(diagnostics, ErrorCode.ERR_CollectionExpressionEscape, node, expr.Type);
					return false;
				}
				return true;
			}
			if (boundConversion.Conversion.IsInlineArray)
			{
				SignatureOnlyMethodSymbol inlineArrayConversionEquivalentSignatureMethod = GetInlineArrayConversionEquivalentSignatureMethod(boundConversion, out var arguments, out var refKinds);
				return CheckInvocationEscape(boundConversion.Syntax, MethodInvocationInfo.FromInlineArrayConversion(inlineArrayConversionEquivalentSignatureMethod, arguments, refKinds, boundConversion.HasAnyErrors), checkingReceiver, escapeTo, diagnostics, isRefEscape: false);
			}
			if (boundConversion.Conversion.IsUserDefined)
			{
				MethodSymbol method = boundConversion.Conversion.Method;
				return CheckInvocationEscape(boundConversion.Syntax, MethodInvocationInfo.FromUserDefinedConversion(method, boundConversion.Operand, boundConversion.HasAnyErrors), checkingReceiver, escapeTo, diagnostics, isRefEscape: false);
			}
			return CheckValEscape(node, boundConversion.Operand, escapeTo, checkingReceiver: false, diagnostics);
		}
		case BoundKind.AssignmentOperator:
		{
			BoundAssignmentOperator boundAssignmentOperator = (BoundAssignmentOperator)expr;
			return CheckValEscape(node, boundAssignmentOperator.Right, escapeTo, checkingReceiver: false, diagnostics);
		}
		case BoundKind.NullCoalescingAssignmentOperator:
		{
			BoundNullCoalescingAssignmentOperator boundNullCoalescingAssignmentOperator = (BoundNullCoalescingAssignmentOperator)expr;
			if (CheckValEscape(node, boundNullCoalescingAssignmentOperator.LeftOperand, escapeTo, checkingReceiver: false, diagnostics))
			{
				return CheckValEscape(node, boundNullCoalescingAssignmentOperator.RightOperand, escapeTo, checkingReceiver: false, diagnostics);
			}
			return false;
		}
		case BoundKind.IncrementOperator:
		{
			BoundIncrementOperator boundIncrementOperator = (BoundIncrementOperator)expr;
			MethodSymbol methodOpt = boundIncrementOperator.MethodOpt;
			if ((object)methodOpt != null && methodOpt.IsStatic)
			{
				UnaryOperatorKind unaryOperatorKind = boundIncrementOperator.OperatorKind.Operator();
				if ((unaryOperatorKind == UnaryOperatorKind.PrefixIncrement || unaryOperatorKind == UnaryOperatorKind.PrefixDecrement) ? true : false)
				{
					return CheckInvocationEscape(boundIncrementOperator.Syntax, MethodInvocationInfo.FromIncrementOperator(boundIncrementOperator), checkingReceiver, escapeTo, diagnostics, isRefEscape: false);
				}
			}
			return CheckValEscape(node, boundIncrementOperator.Operand, escapeTo, checkingReceiver: false, diagnostics);
		}
		case BoundKind.CompoundAssignmentOperator:
		{
			BoundCompoundAssignmentOperator boundCompoundAssignmentOperator = (BoundCompoundAssignmentOperator)expr;
			MethodSymbol method2 = boundCompoundAssignmentOperator.Operator.Method;
			if ((object)method2 != null)
			{
				if (method2.IsStatic)
				{
					return CheckInvocationEscape(boundCompoundAssignmentOperator.Syntax, MethodInvocationInfo.FromCompoundAssignmentOperator(boundCompoundAssignmentOperator), checkingReceiver, escapeTo, diagnostics, isRefEscape: false);
				}
				return CheckValEscape(boundCompoundAssignmentOperator.Left.Syntax, boundCompoundAssignmentOperator.Left, escapeTo, checkingReceiver: false, diagnostics);
			}
			if (CheckValEscape(boundCompoundAssignmentOperator.Left.Syntax, boundCompoundAssignmentOperator.Left, escapeTo, checkingReceiver: false, diagnostics))
			{
				return CheckValEscape(boundCompoundAssignmentOperator.Right.Syntax, boundCompoundAssignmentOperator.Right, escapeTo, checkingReceiver: false, diagnostics);
			}
			return false;
		}
		case BoundKind.BinaryOperator:
		{
			BoundBinaryOperator boundBinaryOperator = (BoundBinaryOperator)expr;
			if (boundBinaryOperator.OperatorKind == BinaryOperatorKind.Utf8Addition)
			{
				return true;
			}
			if ((object)boundBinaryOperator.BinaryOperatorMethod != null)
			{
				return CheckInvocationEscape(boundBinaryOperator.Syntax, MethodInvocationInfo.FromBinaryOperator(boundBinaryOperator), checkingReceiver, escapeTo, diagnostics, isRefEscape: false);
			}
			if (CheckValEscape(boundBinaryOperator.Left.Syntax, boundBinaryOperator.Left, escapeTo, checkingReceiver: false, diagnostics))
			{
				return CheckValEscape(boundBinaryOperator.Right.Syntax, boundBinaryOperator.Right, escapeTo, checkingReceiver: false, diagnostics);
			}
			return false;
		}
		case BoundKind.RangeExpression:
		{
			BoundRangeExpression boundRangeExpression = (BoundRangeExpression)expr;
			BoundExpression leftOperandOpt = boundRangeExpression.LeftOperandOpt;
			if (leftOperandOpt != null && !CheckValEscape(leftOperandOpt.Syntax, leftOperandOpt, escapeTo, checkingReceiver: false, diagnostics))
			{
				return false;
			}
			BoundExpression rightOperandOpt = boundRangeExpression.RightOperandOpt;
			if (rightOperandOpt != null)
			{
				return CheckValEscape(rightOperandOpt.Syntax, rightOperandOpt, escapeTo, checkingReceiver: false, diagnostics);
			}
			return true;
		}
		case BoundKind.UserDefinedConditionalLogicalOperator:
		{
			BoundUserDefinedConditionalLogicalOperator boundUserDefinedConditionalLogicalOperator = (BoundUserDefinedConditionalLogicalOperator)expr;
			return CheckInvocationEscape(boundUserDefinedConditionalLogicalOperator.Syntax, MethodInvocationInfo.FromUserDefinedConditionalLogicalOperator(boundUserDefinedConditionalLogicalOperator), checkingReceiver, escapeTo, diagnostics, isRefEscape: false);
		}
		case BoundKind.QueryClause:
		{
			BoundExpression value3 = ((BoundQueryClause)expr).Value;
			return CheckValEscape(value3.Syntax, value3, escapeTo, checkingReceiver: false, diagnostics);
		}
		case BoundKind.RangeVariable:
		{
			BoundExpression value2 = ((BoundRangeVariable)expr).Value;
			return CheckValEscape(value2.Syntax, value2, escapeTo, checkingReceiver: false, diagnostics);
		}
		case BoundKind.ObjectInitializerExpression:
		{
			BoundObjectInitializerExpression initExpr = (BoundObjectInitializerExpression)expr;
			return CheckValEscapeOfObjectInitializer(initExpr, escapeTo, diagnostics);
		}
		case BoundKind.CollectionInitializerExpression:
		{
			BoundCollectionInitializerExpression colExpr = (BoundCollectionInitializerExpression)expr;
			return CheckValEscapeOfCollectionInitializer(colExpr, escapeTo, diagnostics);
		}
		case BoundKind.PointerElementAccess:
		{
			BoundExpression expression = ((BoundPointerElementAccess)expr).Expression;
			return CheckValEscape(expression.Syntax, expression, escapeTo, checkingReceiver, diagnostics);
		}
		case BoundKind.PointerIndirectionOperator:
		{
			BoundExpression operand = ((BoundPointerIndirectionOperator)expr).Operand;
			return CheckValEscape(operand.Syntax, operand, escapeTo, checkingReceiver, diagnostics);
		}
		case BoundKind.ArrayAccess:
		case BoundKind.AwaitExpression:
		case BoundKind.AsOperator:
		case BoundKind.ConditionalAccess:
		case BoundKind.ConditionalReceiver:
			return false;
		case BoundKind.UnconvertedSwitchExpression:
		case BoundKind.ConvertedSwitchExpression:
			foreach (BoundSwitchExpressionArm switchArm in ((BoundSwitchExpression)expr).SwitchArms)
			{
				BoundExpression value = switchArm.Value;
				if (!CheckValEscape(value.Syntax, value, escapeTo, checkingReceiver: false, diagnostics))
				{
					return false;
				}
			}
			return true;
		default:
			diagnostics.Add(ErrorCode.ERR_InternalError, node.Location);
			return false;
		}
	}

	private SignatureOnlyMethodSymbol GetInlineArrayAccessEquivalentSignatureMethod(BoundInlineArrayAccess elementAccess, out ImmutableArray<BoundExpression> arguments, out ImmutableArray<RefKind> refKinds)
	{
		WellKnownMember getItemOrSliceHelper = elementAccess.GetItemOrSliceHelper;
		RefKind refKind;
		RefKind refKind2;
		if ((getItemOrSliceHelper == WellKnownMember.System_Span_T__get_Item || getItemOrSliceHelper == WellKnownMember.System_ReadOnlySpan_T__get_Item) ? true : false)
		{
			if (elementAccess.IsValue)
			{
				refKind = RefKind.None;
				refKind2 = RefKind.None;
			}
			else
			{
				refKind = ((elementAccess.GetItemOrSliceHelper != WellKnownMember.System_ReadOnlySpan_T__get_Item) ? RefKind.Ref : RefKind.In);
				refKind2 = refKind;
			}
		}
		else
		{
			getItemOrSliceHelper = elementAccess.GetItemOrSliceHelper;
			if ((getItemOrSliceHelper != WellKnownMember.System_Span_T__Slice_Int_Int && getItemOrSliceHelper != WellKnownMember.System_ReadOnlySpan_T__Slice_Int_Int) || 1 == 0)
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/Binder.ValueChecks.cs", 5666);
			}
			refKind = RefKind.None;
			refKind2 = ((elementAccess.GetItemOrSliceHelper != WellKnownMember.System_ReadOnlySpan_T__Slice_Int_Int) ? RefKind.Ref : RefKind.In);
		}
		SignatureOnlyMethodSymbol result = new SignatureOnlyMethodSymbol("", _symbol.ContainingType, MethodKind.Ordinary, Microsoft.Cci.CallingConvention.Default, ImmutableArray<TypeParameterSymbol>.Empty, ImmutableArray.Create((ParameterSymbol)new SignatureOnlyParameterSymbol(TypeWithAnnotations.Create(elementAccess.Expression.Type), ImmutableArray<CustomModifier>.Empty, isParamsArray: false, isParamsCollection: false, refKind2)), refKind, isInitOnly: false, isStatic: true, TypeWithAnnotations.Create(elementAccess.Type), ImmutableArray<CustomModifier>.Empty, ImmutableArray<MethodSymbol>.Empty);
		arguments = ImmutableArray.Create(elementAccess.Expression);
		refKinds = ImmutableArray.Create(refKind2);
		return result;
	}

	private SignatureOnlyMethodSymbol GetInlineArrayConversionEquivalentSignatureMethod(BoundConversion conversion, out ImmutableArray<BoundExpression> arguments, out ImmutableArray<RefKind> refKinds)
	{
		return GetInlineArrayConversionEquivalentSignatureMethod(conversion.Operand, conversion.Type, out arguments, out refKinds);
	}

	private SignatureOnlyMethodSymbol GetInlineArrayConversionEquivalentSignatureMethod(BoundExpression inlineArray, TypeSymbol resultType, out ImmutableArray<BoundExpression> arguments, out ImmutableArray<RefKind> refKinds)
	{
		RefKind refKind = ((!resultType.OriginalDefinition.Equals(_compilation.GetWellKnownType(WellKnownType.System_ReadOnlySpan_T), TypeCompareKind.AllIgnoreOptions)) ? RefKind.Ref : RefKind.In);
		SignatureOnlyMethodSymbol result = new SignatureOnlyMethodSymbol("", _symbol.ContainingType, MethodKind.Ordinary, Microsoft.Cci.CallingConvention.Default, ImmutableArray<TypeParameterSymbol>.Empty, ImmutableArray.Create((ParameterSymbol)new SignatureOnlyParameterSymbol(TypeWithAnnotations.Create(inlineArray.Type), ImmutableArray<CustomModifier>.Empty, isParamsArray: false, isParamsCollection: false, refKind)), RefKind.None, isInitOnly: false, isStatic: true, TypeWithAnnotations.Create(resultType), ImmutableArray<CustomModifier>.Empty, ImmutableArray<MethodSymbol>.Empty);
		arguments = ImmutableArray.Create(inlineArray);
		refKinds = ImmutableArray.Create(refKind);
		return result;
	}

	private bool CheckTupleValEscape(ImmutableArray<BoundExpression> elements, SafeContext escapeTo, BindingDiagnosticBag diagnostics)
	{
		foreach (BoundExpression item in elements)
		{
			if (!CheckValEscape(item.Syntax, item, escapeTo, checkingReceiver: false, diagnostics))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckValEscapeOfCollectionInitializer(BoundCollectionInitializerExpression colExpr, SafeContext escapeTo, BindingDiagnosticBag diagnostics)
	{
		foreach (BoundExpression initializer in colExpr.Initializers)
		{
			if (initializer is BoundCollectionElementInitializer boundCollectionElementInitializer)
			{
				if (!CheckInvocationEscapeToReceiver(boundCollectionElementInitializer.Syntax, MethodInvocationInfo.FromCollectionElementInitializer(boundCollectionElementInitializer), checkingReceiver: false, escapeTo, diagnostics))
				{
					return false;
				}
			}
			else if (!CheckValEscape(initializer.Syntax, initializer, escapeTo, checkingReceiver: false, diagnostics))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckValEscapeOfObjectInitializer(BoundObjectInitializerExpression initExpr, SafeContext escapeTo, BindingDiagnosticBag diagnostics)
	{
		foreach (BoundExpression initializer in initExpr.Initializers)
		{
			if (!GetValEscapeOfObjectMemberInitializer(initializer).IsConvertibleTo(escapeTo))
			{
				Error(diagnostics, _inUnsafeRegion ? ErrorCode.WRN_EscapeVariable : ErrorCode.ERR_EscapeVariable, initExpr.Syntax, initializer.Syntax);
				return false;
			}
		}
		return true;
	}

	private bool CheckInterpolatedStringHandlerConversionEscape(BoundExpression expression, SafeContext escapeTo, BindingDiagnosticBag diagnostics)
	{
		InterpolatedStringHandlerData interpolatedStringHandlerData = expression.GetInterpolatedStringHandlerData();
		if (CheckValEscape(expression.Syntax, interpolatedStringHandlerData.Construction, escapeTo, checkingReceiver: false, diagnostics))
		{
			return CheckValEscapeOfInterpolatedStringHandlerCalls(expression, escapeTo, diagnostics);
		}
		return false;
	}

	private SafeContext GetValEscapeOfInterpolatedStringHandlerCalls(BoundExpression expression)
	{
		SafeContext safeContext = SafeContext.CallingMethod;
		while (expression is BoundBinaryOperator boundBinaryOperator)
		{
			safeContext = safeContext.Intersect(GetValEscapeOfInterpolatedStringHandlerCalls(boundBinaryOperator.Right));
			expression = boundBinaryOperator.Left;
		}
		if (expression is BoundInterpolatedString interpolatedString)
		{
			return safeContext.Intersect(getPartsScope(interpolatedString));
		}
		throw ExceptionUtilities.UnexpectedValue(expression.Kind);
		SafeContext getPartsScope(BoundInterpolatedString boundInterpolatedString)
		{
			SafeContext result = SafeContext.CallingMethod;
			foreach (BoundExpression part in boundInterpolatedString.Parts)
			{
				if (part is BoundCall { IsErroneousNode: false } boundCall)
				{
					result = result.Intersect(GetInvocationEscapeToReceiver(MethodInvocationInfo.FromCall(boundCall)));
				}
			}
			return result;
		}
	}

	private bool CheckValEscapeOfInterpolatedStringHandlerCalls(BoundExpression expression, SafeContext escapeTo, BindingDiagnosticBag diagnostics)
	{
		while (true)
		{
			if (!(expression is BoundBinaryOperator boundBinaryOperator))
			{
				if (!(expression is BoundInterpolatedString interpolatedString))
				{
					break;
				}
				return checkParts(interpolatedString, escapeTo, diagnostics);
			}
			if (!CheckValEscapeOfInterpolatedStringHandlerCalls(boundBinaryOperator.Right, escapeTo, diagnostics))
			{
				return false;
			}
			expression = boundBinaryOperator.Left;
		}
		throw ExceptionUtilities.UnexpectedValue(expression.Kind);
		bool checkParts(BoundInterpolatedString boundInterpolatedString, SafeContext escapeTo2, BindingDiagnosticBag diagnostics2)
		{
			foreach (BoundExpression part in boundInterpolatedString.Parts)
			{
				if (part is BoundCall { IsErroneousNode: false } boundCall && !CheckInvocationEscapeToReceiver(boundCall.Syntax, MethodInvocationInfo.FromCall(boundCall), checkingReceiver: false, escapeTo2, diagnostics2))
				{
					return false;
				}
			}
			return true;
		}
	}

	private void ValidateRefConditionalOperator(SyntaxNode node, BoundExpression trueExpr, BoundExpression falseExpr, BindingDiagnosticBag diagnostics)
	{
		SafeContext valEscape = GetValEscape(trueExpr);
		SafeContext valEscape2 = GetValEscape(falseExpr);
		if (valEscape != valEscape2)
		{
			if (!valEscape2.IsConvertibleTo(valEscape))
			{
				CheckValEscape(falseExpr.Syntax, falseExpr, valEscape, checkingReceiver: false, diagnostics);
			}
			else
			{
				CheckValEscape(trueExpr.Syntax, trueExpr, valEscape2, checkingReceiver: false, diagnostics);
			}
			diagnostics.Add(_inUnsafeRegion ? ErrorCode.WRN_MismatchedRefEscapeInTernary : ErrorCode.ERR_MismatchedRefEscapeInTernary, node.Location);
		}
	}

	private void ValidateAssignment(SyntaxNode node, BoundExpression op1, BoundExpression op2, bool isRef, BindingDiagnosticBag diagnostics)
	{
		if (op1.HasAnyErrors)
		{
			return;
		}
		bool flag = false;
		if (isRef)
		{
			SafeContext refEscape = GetRefEscape(op1);
			SafeContext refEscape2 = GetRefEscape(op2);
			if (!refEscape2.IsConvertibleTo(refEscape))
			{
				bool inUnsafeRegion = _inUnsafeRegion;
				ErrorCode errorCode = (refEscape2.IsReturnOnly ? (inUnsafeRegion ? ErrorCode.WRN_RefAssignReturnOnly : ErrorCode.ERR_RefAssignReturnOnly) : (inUnsafeRegion ? ErrorCode.WRN_RefAssignNarrower : ErrorCode.ERR_RefAssignNarrower));
				ErrorCode code = errorCode;
				Error(diagnostics, code, node, getName(op1), op2.Syntax);
				if (!_inUnsafeRegion)
				{
					flag = true;
				}
			}
			else
			{
				BoundKind kind = op1.Kind;
				if ((kind == BoundKind.Local || kind == BoundKind.Parameter) ? true : false)
				{
					refEscape = GetValEscape(op1);
					refEscape2 = GetValEscape(op2);
					if (!refEscape.IsConvertibleTo(refEscape2))
					{
						ErrorCode code2 = (_inUnsafeRegion ? ErrorCode.WRN_RefAssignValEscapeWider : ErrorCode.ERR_RefAssignValEscapeWider);
						Error(diagnostics, code2, node, getName(op1), op2.Syntax);
						if (!_inUnsafeRegion)
						{
							flag = true;
						}
					}
				}
			}
		}
		else if (op1 is BoundPropertyAccess boundPropertyAccess)
		{
			PropertySymbol propertySymbol = boundPropertyAccess.PropertySymbol;
			if ((object)propertySymbol != null)
			{
				MethodSymbol setMethod = propertySymbol.SetMethod;
				if ((object)setMethod != null)
				{
					BoundExpression receiverOpt = boundPropertyAccess.ReceiverOpt;
					if (setMethod.IsExtensionBlockMember())
					{
						MethodInvocationInfo methodInvocationInfo = MethodInvocationInfo.FromCallParts(setMethod, receiverOpt, ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1] { op2 }), ThreeState.Unknown);
						handleExtensionSetter(in methodInvocationInfo);
						return;
					}
				}
			}
		}
		else if (op1 is BoundIndexerAccess boundIndexerAccess)
		{
			PropertySymbol indexer = boundIndexerAccess.Indexer;
			if ((object)indexer != null)
			{
				MethodSymbol setMethod2 = indexer.SetMethod;
				if ((object)setMethod2 != null && setMethod2.IsExtensionBlockMember())
				{
					MethodInvocationInfo methodInvocationInfo = MethodInvocationInfo.FromIndexerAccess(boundIndexerAccess);
					handleExtensionSetter(in methodInvocationInfo);
					return;
				}
			}
		}
		if (!flag && op1.Type.IsRefLikeOrAllowsRefLikeType())
		{
			SafeContext valEscape = GetValEscape(op1);
			ValidateEscape(op2, valEscape, isByRef: false, diagnostics);
		}
		static object getName(BoundExpression expr)
		{
			Symbol expressionSymbol = expr.ExpressionSymbol;
			if ((object)expressionSymbol != null)
			{
				return expressionSymbol.Name;
			}
			if (expr is BoundArrayAccess)
			{
				return MessageID.IDS_ArrayAccess.Localize();
			}
			if (expr is BoundPointerElementAccess)
			{
				return MessageID.IDS_PointerElementAccess.Localize();
			}
			return "";
		}
		void handleExtensionSetter(ref readonly MethodInvocationInfo reference)
		{
			MethodInvocationInfo methodInvocationInfo2 = ReplaceWithExtensionImplementationIfNeeded(in reference);
			CheckInvocationArgMixing(node, in methodInvocationInfo2, reference.MethodInfo.Method, diagnostics);
		}
	}

	internal static void Analyze(CSharpCompilation compilation, MethodSymbol symbol, BoundNode node, BindingDiagnosticBag diagnostics)
	{
		RefSafetyAnalysis refSafetyAnalysis = new RefSafetyAnalysis(compilation, symbol, node, InUnsafeMethod(symbol), symbol.ContainingModule.UseUpdatedEscapeRules, diagnostics);
		try
		{
			refSafetyAnalysis.Visit(node);
		}
		catch (CancelledByStackGuardException ex)
		{
			ex.AddAnError(diagnostics);
		}
	}

	private static bool InUnsafeMethod(Symbol symbol)
	{
		if (symbol is SourceMemberMethodSymbol { IsUnsafe: not false })
		{
			return true;
		}
		NamedTypeSymbol containingType = symbol.ContainingType;
		while ((object)containingType != null)
		{
			NamedTypeSymbol originalDefinition = containingType.OriginalDefinition;
			if (originalDefinition is SourceMemberContainerTypeSymbol { IsUnsafe: not false })
			{
				return true;
			}
			containingType = originalDefinition.ContainingType;
		}
		return false;
	}

	private RefSafetyAnalysis(CSharpCompilation compilation, MethodSymbol symbol, BoundNode rootNode, bool inUnsafeRegion, bool useUpdatedEscapeRules, BindingDiagnosticBag diagnostics)
	{
		_compilation = compilation;
		_symbol = symbol;
		_rootNode = rootNode;
		_useUpdatedEscapeRules = useUpdatedEscapeRules;
		_diagnostics = diagnostics;
		_inUnsafeRegion = inUnsafeRegion;
		_localScopeDepth = SafeContext.CurrentMethod;
	}

	private (SafeContext RefEscapeScope, SafeContext ValEscapeScope) GetLocalScopes(LocalSymbol local)
	{
		Dictionary<LocalSymbol, (SafeContext RefEscapeScope, SafeContext ValEscapeScope)>? localEscapeScopes = _localEscapeScopes;
		if (localEscapeScopes == null || !localEscapeScopes.TryGetValue(local, out (SafeContext, SafeContext) value))
		{
			return (RefEscapeScope: SafeContext.CurrentMethod, ValEscapeScope: SafeContext.CallingMethod);
		}
		return value;
	}

	private void SetLocalScopes(LocalSymbol local, SafeContext refEscapeScope, SafeContext valEscapeScope)
	{
		AddOrSetLocalScopes(local, refEscapeScope, valEscapeScope);
	}

	private void AddPlaceholderScope(BoundValuePlaceholderBase placeholder, SafeContextAndLocation valEscapeScope)
	{
		if (_placeholderScopes == null)
		{
			_placeholderScopes = new Dictionary<BoundValuePlaceholderBase, SafeContextAndLocation>();
		}
		_placeholderScopes[placeholder] = valEscapeScope;
	}

	private void RemovePlaceholderScope(BoundValuePlaceholderBase placeholder)
	{
	}

	private SafeContext GetPlaceholderScope(BoundValuePlaceholderBase placeholder)
	{
		Dictionary<BoundValuePlaceholderBase, SafeContextAndLocation>? placeholderScopes = _placeholderScopes;
		if (placeholderScopes == null || !placeholderScopes.TryGetValue(placeholder, out var value))
		{
			return SafeContext.CallingMethod;
		}
		return value.Context;
	}

	public override BoundNode? VisitBlock(BoundBlock node)
	{
		UnsafeRegion unsafeRegion = new UnsafeRegion(this, _inUnsafeRegion || node.HasUnsafeModifier);
		try
		{
			BoundNode rootNode = _rootNode;
			bool flag = ((rootNode is BoundConstructorMethodBody boundConstructorMethodBody) ? (boundConstructorMethodBody.BlockBody != node && boundConstructorMethodBody.ExpressionBody != node) : ((rootNode is BoundNonConstructorMethodBody boundNonConstructorMethodBody) ? (boundNonConstructorMethodBody.BlockBody != node && boundNonConstructorMethodBody.ExpressionBody != node) : ((rootNode is BoundLambda boundLambda) ? (boundLambda.Body != node) : (!(rootNode is BoundLocalFunctionStatement boundLocalFunctionStatement) || boundLocalFunctionStatement.Body != node))));
			bool adjustDepth = flag;
			using (new LocalScope(this, node.Locals, adjustDepth))
			{
				return base.VisitBlock(node);
			}
		}
		finally
		{
			unsafeRegion.Dispose();
		}
	}

	public override BoundNode? Visit(BoundNode? node)
	{
		return base.Visit(node);
	}

	public override BoundNode? VisitFieldEqualsValue(BoundFieldEqualsValue node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/RefSafetyAnalysis.cs", 366);
	}

	public override BoundNode? VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
	{
		LocalFunctionSymbol localFunctionSymbol = (LocalFunctionSymbol)node.Symbol;
		RefSafetyAnalysis refSafetyAnalysis = new RefSafetyAnalysis(_compilation, localFunctionSymbol, node, _inUnsafeRegion || localFunctionSymbol.IsUnsafe, _useUpdatedEscapeRules, _diagnostics);
		refSafetyAnalysis.Visit(node.BlockBody);
		refSafetyAnalysis.Visit(node.ExpressionBody);
		return null;
	}

	public override BoundNode? VisitLambda(BoundLambda node)
	{
		MethodSymbol symbol = node.Symbol;
		new RefSafetyAnalysis(_compilation, symbol, node, _inUnsafeRegion, _useUpdatedEscapeRules, _diagnostics).Visit(node.Body);
		return null;
	}

	public override BoundNode? VisitConstructorMethodBody(BoundConstructorMethodBody node)
	{
		using (new LocalScope(this, node.Locals, adjustDepth: false))
		{
			return base.VisitConstructorMethodBody(node);
		}
	}

	public override BoundNode? VisitForStatement(BoundForStatement node)
	{
		using (new LocalScope(this, node.OuterLocals))
		{
			using (new LocalScope(this, node.InnerLocals))
			{
				return base.VisitForStatement(node);
			}
		}
	}

	public override BoundNode? VisitUsingStatement(BoundUsingStatement node)
	{
		using (new LocalScope(this, node.Locals))
		{
			Visit(node.DeclarationsOpt);
			Visit(node.ExpressionOpt);
			ArrayBuilder<(BoundValuePlaceholderBase, SafeContextAndLocation)> instance = ArrayBuilder<(BoundValuePlaceholderBase, SafeContextAndLocation)>.GetInstance();
			BoundAwaitableInfo awaitOpt = node.AwaitOpt;
			if (awaitOpt != null)
			{
				BoundExpression expressionOpt = node.ExpressionOpt;
				SafeContext valEscapeScope = ((expressionOpt != null) ? GetValEscape(expressionOpt) : _localScopeDepth);
				GetAwaitableInstancePlaceholders(instance, awaitOpt, valEscapeScope);
			}
			using (new PlaceholderRegion(this, instance))
			{
				Visit(node.AwaitOpt);
				Visit(node.Body);
				return null;
			}
		}
	}

	public override BoundNode? VisitUsingLocalDeclarations(BoundUsingLocalDeclarations node)
	{
		ArrayBuilder<(BoundValuePlaceholderBase, SafeContextAndLocation)> instance = ArrayBuilder<(BoundValuePlaceholderBase, SafeContextAndLocation)>.GetInstance();
		BoundAwaitableInfo awaitOpt = node.AwaitOpt;
		if (awaitOpt != null)
		{
			GetAwaitableInstancePlaceholders(instance, awaitOpt, _localScopeDepth);
		}
		using (new PlaceholderRegion(this, instance))
		{
			return base.VisitUsingLocalDeclarations(node);
		}
	}

	public override BoundNode? VisitFixedStatement(BoundFixedStatement node)
	{
		using (new LocalScope(this, node.Locals))
		{
			return base.VisitFixedStatement(node);
		}
	}

	public override BoundNode? VisitDoStatement(BoundDoStatement node)
	{
		using (new LocalScope(this, node.Locals))
		{
			return base.VisitDoStatement(node);
		}
	}

	public override BoundNode? VisitWhileStatement(BoundWhileStatement node)
	{
		using (new LocalScope(this, node.Locals))
		{
			return base.VisitWhileStatement(node);
		}
	}

	public override BoundNode? VisitSwitchStatement(BoundSwitchStatement node)
	{
		Visit(node.Expression);
		using (new LocalScope(this, node.InnerLocals))
		{
			using (new PatternInput(this, GetValEscape(node.Expression)))
			{
				VisitList(node.SwitchSections);
				Visit(node.DefaultLabel);
				return null;
			}
		}
	}

	public override BoundNode? VisitConvertedSwitchExpression(BoundConvertedSwitchExpression node)
	{
		Visit(node.Expression);
		using (new PatternInput(this, GetValEscape(node.Expression)))
		{
			VisitList(node.SwitchArms);
			return null;
		}
	}

	public override BoundNode? VisitSwitchSection(BoundSwitchSection node)
	{
		using (new LocalScope(this, node.Locals))
		{
			return base.VisitSwitchSection(node);
		}
	}

	public override BoundNode? VisitSwitchExpressionArm(BoundSwitchExpressionArm node)
	{
		using (new LocalScope(this, node.Locals))
		{
			return base.VisitSwitchExpressionArm(node);
		}
	}

	public override BoundNode? VisitCatchBlock(BoundCatchBlock node)
	{
		using (new LocalScope(this, node.Locals))
		{
			return base.VisitCatchBlock(node);
		}
	}

	public override BoundNode? VisitLocal(BoundLocal node)
	{
		return base.VisitLocal(node);
	}

	private void AddLocalScopes(LocalSymbol local, SafeContext refEscapeScope, SafeContext valEscapeScope)
	{
		ScopedKind scopedKind = (_useUpdatedEscapeRules ? local.Scope : ScopedKind.None);
		if (scopedKind != ScopedKind.None)
		{
			refEscapeScope = ((scopedKind == ScopedKind.ScopedRef) ? _localScopeDepth : SafeContext.CurrentMethod);
			valEscapeScope = ((scopedKind == ScopedKind.ScopedValue) ? _localScopeDepth : SafeContext.CallingMethod);
		}
		AddOrSetLocalScopes(local, refEscapeScope, valEscapeScope);
	}

	private void AddOrSetLocalScopes(LocalSymbol local, SafeContext refEscapeScope, SafeContext valEscapeScope)
	{
		if (_localEscapeScopes == null)
		{
			_localEscapeScopes = new Dictionary<LocalSymbol, (SafeContext, SafeContext)>();
		}
		_localEscapeScopes[local] = (refEscapeScope, valEscapeScope);
	}

	private void RemoveLocalScopes(LocalSymbol local)
	{
	}

	public override BoundNode? VisitLocalDeclaration(BoundLocalDeclaration node)
	{
		base.VisitLocalDeclaration(node);
		BoundExpression initializerOpt = node.InitializerOpt;
		if (initializerOpt != null)
		{
			SourceLocalSymbol sourceLocalSymbol = (SourceLocalSymbol)node.LocalSymbol;
			SafeContext refEscapeScope;
			SafeContext escapeTo;
			(refEscapeScope, escapeTo) = GetLocalScopes(sourceLocalSymbol);
			if (_useUpdatedEscapeRules && sourceLocalSymbol.Scope != ScopedKind.None)
			{
				BoundTypeExpression? declaredTypeOpt = node.DeclaredTypeOpt;
				if (declaredTypeOpt != null && declaredTypeOpt.Type.IsRefLikeOrAllowsRefLikeType())
				{
					ValidateEscape(initializerOpt, escapeTo, isByRef: false, _diagnostics);
				}
			}
			else
			{
				SetLocalScopes(sourceLocalSymbol, _localScopeDepth, _localScopeDepth);
				escapeTo = GetValEscape(initializerOpt);
				if (sourceLocalSymbol.RefKind != RefKind.None)
				{
					refEscapeScope = GetRefEscape(initializerOpt);
				}
				SetLocalScopes(sourceLocalSymbol, refEscapeScope, escapeTo);
			}
		}
		return null;
	}

	public override BoundNode? VisitReturnStatement(BoundReturnStatement node)
	{
		base.VisitReturnStatement(node);
		BoundExpression expressionOpt = node.ExpressionOpt;
		if (expressionOpt != null && (object)expressionOpt.Type != null)
		{
			ValidateEscape(expressionOpt, SafeContext.ReturnOnly, node.RefKind != RefKind.None, _diagnostics);
		}
		return null;
	}

	public override BoundNode? VisitYieldReturnStatement(BoundYieldReturnStatement node)
	{
		base.VisitYieldReturnStatement(node);
		BoundExpression expression = node.Expression;
		if (expression != null && (object)expression.Type != null)
		{
			ValidateEscape(expression, SafeContext.ReturnOnly, isByRef: false, _diagnostics);
		}
		return null;
	}

	public override BoundNode? VisitAssignmentOperator(BoundAssignmentOperator node)
	{
		base.VisitAssignmentOperator(node);
		if (node.Left.Kind != BoundKind.DiscardExpression)
		{
			ValidateAssignment(node.Syntax, node.Left, node.Right, node.IsRef, _diagnostics);
		}
		return null;
	}

	public override BoundNode? VisitCompoundAssignmentOperator(BoundCompoundAssignmentOperator node)
	{
		base.VisitCompoundAssignmentOperator(node);
		if (!node.HasErrors)
		{
			MethodSymbol method = node.Operator.Method;
			if ((object)method != null)
			{
				MethodInvocationInfo methodInvocationInfo = MethodInvocationInfo.FromCompoundAssignmentOperator(node);
				methodInvocationInfo = ReplaceWithExtensionImplementationIfNeeded(in methodInvocationInfo);
				CheckInvocationArgMixing(node.Syntax, in methodInvocationInfo, method, _diagnostics);
				if (!method.IsStatic)
				{
					return null;
				}
			}
		}
		ValidateAssignment(node.Syntax, node.Left, node, isRef: false, _diagnostics);
		return null;
	}

	public override BoundNode? VisitIsPatternExpression(BoundIsPatternExpression node)
	{
		Visit(node.Expression);
		using (new PatternInput(this, GetValEscape(node.Expression)))
		{
			Visit(node.Pattern);
			return null;
		}
	}

	public override BoundNode? VisitDeclarationPattern(BoundDeclarationPattern node)
	{
		SetPatternLocalScopes(node);
		using (new PatternInput(this, getDeclarationValEscape(node.DeclaredType, _patternInputValEscape)))
		{
			return base.VisitDeclarationPattern(node);
		}
		static SafeContext getDeclarationValEscape(BoundTypeExpression typeExpression, SafeContext valEscape)
		{
			if (!typeExpression.Type.IsRefLikeOrAllowsRefLikeType())
			{
				return SafeContext.CallingMethod;
			}
			return valEscape;
		}
	}

	public override BoundNode? VisitListPattern(BoundListPattern node)
	{
		SetPatternLocalScopes(node);
		return base.VisitListPattern(node);
	}

	public override BoundNode? VisitRecursivePattern(BoundRecursivePattern node)
	{
		SetPatternLocalScopes(node);
		MethodSymbol deconstructMethod = node.DeconstructMethod;
		if ((object)deconstructMethod != null)
		{
			ParameterSymbol? parameterSymbol = tryGetReceiverParameter(deconstructMethod);
			if ((object)parameterSymbol != null && parameterSymbol.EffectiveScope == ScopedKind.None)
			{
				using (new PatternInput(this, _localScopeDepth))
				{
					return base.VisitRecursivePattern(node);
				}
			}
		}
		return base.VisitRecursivePattern(node);
		static ParameterSymbol? tryGetReceiverParameter(MethodSymbol method)
		{
			if (method.IsExtensionMethod)
			{
				ImmutableArray<ParameterSymbol> parameters = method.Parameters;
				if (parameters.Length >= 1)
				{
					ParameterSymbol parameterSymbol2 = parameters[0];
					if ((object)parameterSymbol2 != null)
					{
						return parameterSymbol2;
					}
				}
				return null;
			}
			if (method.IsExtensionBlockMember())
			{
				return method.ContainingType.ExtensionParameter;
			}
			if (!method.TryGetThisParameter(out ParameterSymbol thisParameter))
			{
				return null;
			}
			return thisParameter;
		}
	}

	public override BoundNode? VisitPositionalSubpattern(BoundPositionalSubpattern node)
	{
		using (new PatternInput(this, getPositionalValEscape(node.Symbol, _patternInputValEscape)))
		{
			return base.VisitPositionalSubpattern(node);
		}
		static SafeContext getPositionalValEscape(Symbol? symbol, SafeContext valEscape)
		{
			if ((object)symbol != null)
			{
				if (!symbol.GetTypeOrReturnType().IsRefLikeOrAllowsRefLikeType())
				{
					return SafeContext.CallingMethod;
				}
				return valEscape;
			}
			return valEscape;
		}
	}

	public override BoundNode? VisitPropertySubpattern(BoundPropertySubpattern node)
	{
		using (new PatternInput(this, getMemberValEscape(node.Member, _patternInputValEscape)))
		{
			return base.VisitPropertySubpattern(node);
		}
		static SafeContext getMemberValEscape(BoundPropertySubpatternMember? member, SafeContext valEscape)
		{
			if (member == null)
			{
				return valEscape;
			}
			valEscape = getMemberValEscape(member.Receiver, valEscape);
			if (!member.Type.IsRefLikeOrAllowsRefLikeType())
			{
				return SafeContext.CallingMethod;
			}
			return valEscape;
		}
	}

	private void SetPatternLocalScopes(BoundObjectPattern pattern)
	{
		if (pattern.Variable is LocalSymbol local)
		{
			SetLocalScopes(local, _localScopeDepth, _patternInputValEscape);
		}
	}

	public override BoundNode? VisitConditionalOperator(BoundConditionalOperator node)
	{
		base.VisitConditionalOperator(node);
		if (node.IsRef)
		{
			ValidateRefConditionalOperator(node.Syntax, node.Consequence, node.Alternative, _diagnostics);
		}
		return null;
	}

	private void VisitArgumentsAndGetArgumentPlaceholders(BoundExpression? receiverOpt, ImmutableArray<BoundExpression> arguments, bool isExtensionBlockMethod)
	{
		for (int num = 0; num < arguments.Length; num++)
		{
			BoundExpression boundExpression = arguments[num];
			BoundConversion boundConversion = boundExpression as BoundConversion;
			bool flag;
			if (boundConversion != null && boundConversion.ConversionKind == ConversionKind.InterpolatedStringHandler)
			{
				BoundExpression operand = boundConversion.Operand;
				if (operand is BoundInterpolatedString || operand is BoundBinaryOperator)
				{
					flag = true;
					goto IL_0040;
				}
			}
			flag = false;
			goto IL_0040;
			IL_0040:
			if (flag)
			{
				InterpolatedStringHandlerData interpolationData = boundConversion.Operand.GetInterpolatedStringHandlerData();
				ArrayBuilder<(BoundValuePlaceholderBase, SafeContextAndLocation)> instance = ArrayBuilder<(BoundValuePlaceholderBase, SafeContextAndLocation)>.GetInstance();
				GetInterpolatedStringPlaceholders(instance, in interpolationData, receiverOpt, num, arguments, isExtensionBlockMethod);
				new PlaceholderRegion(this, instance);
			}
			Visit(boundExpression);
		}
	}

	public sealed override BoundNode? VisitCall(BoundCall node)
	{
		MethodInvocationInfo methodInvocationInfo = getInvocationInfo(node);
		if (methodInvocationInfo.Receiver is BoundCall boundCall)
		{
			ArrayBuilder<(BoundCall, MethodInvocationInfo)> instance = ArrayBuilder<(BoundCall, MethodInvocationInfo)>.GetInstance();
			instance.Push((node, methodInvocationInfo));
			node = boundCall;
			methodInvocationInfo = getInvocationInfo(node);
			while (methodInvocationInfo.Receiver is BoundCall boundCall2)
			{
				BeforeVisitingSkippedBoundCallChildren(node);
				instance.Push((node, methodInvocationInfo));
				node = boundCall2;
				methodInvocationInfo = getInvocationInfo(node);
			}
			BeforeVisitingSkippedBoundCallChildren(node);
			visitReceiver(node, in methodInvocationInfo);
			(BoundCall, MethodInvocationInfo) result = (node, methodInvocationInfo);
			do
			{
				visitArguments(result.Item1, in result.Item2);
			}
			while (instance.TryPop(out result));
			instance.Free();
		}
		else
		{
			visitReceiver(node, in methodInvocationInfo);
			visitArguments(node, in methodInvocationInfo);
		}
		return null;
		static MethodInvocationInfo getInvocationInfo(BoundCall boundCall3)
		{
			MethodInvocationInfo methodInvocationInfo2 = MethodInvocationInfo.FromCall(boundCall3);
			if (!boundCall3.IsErroneousNode)
			{
				methodInvocationInfo2 = ReplaceWithExtensionImplementationIfNeeded(in methodInvocationInfo2);
			}
			return methodInvocationInfo2;
		}
		void visitArguments(BoundCall boundCall3, ref readonly MethodInvocationInfo methodInvocationInfo2)
		{
			if (boundCall3.IsErroneousNode)
			{
				VisitList(boundCall3.Arguments);
			}
			else
			{
				VisitArguments(boundCall3, in methodInvocationInfo2);
			}
		}
		void visitReceiver(BoundCall boundCall3, ref readonly MethodInvocationInfo methodInvocationInfo2)
		{
			if (boundCall3.IsErroneousNode)
			{
				Visit(boundCall3.ReceiverOpt);
			}
			else
			{
				VisitReceiver(in methodInvocationInfo2);
			}
		}
	}

	protected override void VisitReceiver(BoundCall node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/RefSafetyAnalysis.cs", 837);
	}

	private void VisitReceiver(ref readonly MethodInvocationInfo methodInvocationInfo)
	{
		if (methodInvocationInfo.Receiver != null)
		{
			Visit(methodInvocationInfo.Receiver);
		}
	}

	protected override void VisitArguments(BoundCall node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/RefSafetyAnalysis.cs", 850);
	}

	private void VisitArguments(BoundCall node, ref readonly MethodInvocationInfo methodInvocationInfo)
	{
		VisitArgumentsAndGetArgumentPlaceholders(methodInvocationInfo.Receiver, methodInvocationInfo.ArgsOpt, node.Method.IsExtensionBlockMember());
		if (!node.HasErrors)
		{
			CheckInvocationArgMixing(node.Syntax, in methodInvocationInfo, node.Method, _diagnostics);
		}
	}

	private void GetInterpolatedStringPlaceholders(ArrayBuilder<(BoundValuePlaceholderBase, SafeContextAndLocation)> placeholders, in InterpolatedStringHandlerData interpolationData, BoundExpression? receiver, int nArgumentsVisited, ImmutableArray<BoundExpression> arguments, bool isExtensionBlockMethod)
	{
		placeholders.Add((interpolationData.ReceiverPlaceholder, SafeContextAndLocation.Create(_localScopeDepth)));
		foreach (BoundInterpolatedStringArgumentPlaceholder argumentPlaceholder in interpolationData.ArgumentPlaceholders)
		{
			int argumentIndex = argumentPlaceholder.ArgumentIndex;
			int num = (isExtensionBlockMethod ? 1 : 0);
			SafeContext context;
			if (argumentIndex < 0)
			{
				switch (argumentIndex)
				{
				case -1:
					context = ((receiver != null) ? (receiver.GetRefKind().IsWritableReference() ? GetRefEscape(receiver) : GetValEscape(receiver)) : SafeContext.CallingMethod);
					break;
				case -2:
					context = getArgumentEscapeScope(nArgumentsVisited, arguments, 0);
					break;
				default:
					throw ExceptionUtilities.UnexpectedValue(argumentPlaceholder.ArgumentIndex + num);
				case -4:
				case -3:
					continue;
				}
			}
			else
			{
				context = getArgumentEscapeScope(nArgumentsVisited, arguments, argumentIndex + num);
			}
			placeholders.Add((argumentPlaceholder, SafeContextAndLocation.Create(context)));
		}
		SafeContext getArgumentEscapeScope(int num2, ImmutableArray<BoundExpression> immutableArray, int argIndex)
		{
			if (argIndex < num2)
			{
				return GetValEscape(immutableArray[argIndex]);
			}
			return SafeContext.CallingMethod;
		}
	}

	public override BoundNode? VisitObjectCreationExpression(BoundObjectCreationExpression node)
	{
		VisitObjectCreationExpressionBase(node);
		return null;
	}

	public override BoundNode? VisitDynamicObjectCreationExpression(BoundDynamicObjectCreationExpression node)
	{
		VisitObjectCreationExpressionBase(node);
		return null;
	}

	public override BoundNode? VisitNewT(BoundNewT node)
	{
		VisitObjectCreationExpressionBase(node);
		return null;
	}

	public override BoundNode? VisitNoPiaObjectCreationExpression(BoundNoPiaObjectCreationExpression node)
	{
		VisitObjectCreationExpressionBase(node);
		return null;
	}

	private void VisitObjectCreationExpressionBase(BoundObjectCreationExpressionBase node)
	{
		if ((object)node.Constructor == null)
		{
			VisitArgumentsAndGetArgumentPlaceholders(null, node.Arguments, isExtensionBlockMethod: false);
			Visit(node.InitializerExpressionOpt);
			return;
		}
		MethodInvocationInfo methodInvocationInfo = MethodInvocationInfo.FromObjectCreation(node);
		methodInvocationInfo = ReplaceWithExtensionImplementationIfNeeded(in methodInvocationInfo);
		VisitArgumentsAndGetArgumentPlaceholders(null, methodInvocationInfo.ArgsOpt, node.Constructor.IsExtensionBlockMember());
		Visit(node.InitializerExpressionOpt);
		if (node.HasErrors)
		{
			return;
		}
		CheckInvocationArgMixing(node.Syntax, in methodInvocationInfo, node.Constructor, _diagnostics);
		if (node.InitializerExpressionOpt == null)
		{
			return;
		}
		ArrayBuilder<EscapeValue> instance = ArrayBuilder<EscapeValue>.GetInstance();
		SafeContext valEscape = GetValEscape(node.InitializerExpressionOpt);
		GetEscapeValues(in methodInvocationInfo, ignoreArglistRefKinds: false, null, instance);
		foreach (var (parameterSymbol2, boundExpression2, _, flag2) in instance)
		{
			if (flag2 && (object)parameterSymbol2 != null && parameterSymbol2.Type?.IsRefLikeOrAllowsRefLikeType() == true && parameterSymbol2.RefKind.IsWritableReference() && !valEscape.IsConvertibleTo(GetValEscape(boundExpression2)))
			{
				Error(_diagnostics, ErrorCode.ERR_CallArgMixing, boundExpression2.Syntax, node.Constructor, parameterSymbol2.Name);
			}
		}
		instance.Free();
	}

	public override BoundNode? VisitPropertyAccess(BoundPropertyAccess node)
	{
		return base.VisitPropertyAccess(node);
	}

	public override BoundNode? VisitIndexerAccess(BoundIndexerAccess node)
	{
		MethodInvocationInfo methodInvocationInfo = MethodInvocationInfo.FromIndexerAccess(node);
		methodInvocationInfo = ReplaceWithExtensionImplementationIfNeeded(in methodInvocationInfo);
		Visit(methodInvocationInfo.Receiver);
		VisitArgumentsAndGetArgumentPlaceholders(methodInvocationInfo.Receiver, methodInvocationInfo.ArgsOpt, node.Indexer.IsExtensionBlockMember());
		if (!node.HasErrors)
		{
			PropertySymbol indexer = node.Indexer;
			CheckInvocationArgMixing(node.Syntax, in methodInvocationInfo, indexer, _diagnostics);
		}
		return null;
	}

	public override BoundNode? VisitFunctionPointerInvocation(BoundFunctionPointerInvocation node)
	{
		VisitArgumentsAndGetArgumentPlaceholders(null, node.Arguments, isExtensionBlockMethod: false);
		if (!node.HasErrors)
		{
			MethodInvocationInfo methodInvocationInfo = MethodInvocationInfo.FromFunctionPointerInvocation(node);
			CheckInvocationArgMixing(node.Syntax, in methodInvocationInfo, node.FunctionPointer.Signature, _diagnostics);
		}
		return null;
	}

	public override BoundNode? VisitAwaitExpression(BoundAwaitExpression node)
	{
		Visit(node.Expression);
		ArrayBuilder<(BoundValuePlaceholderBase, SafeContextAndLocation)> instance = ArrayBuilder<(BoundValuePlaceholderBase, SafeContextAndLocation)>.GetInstance();
		GetAwaitableInstancePlaceholders(instance, node.AwaitableInfo, GetValEscape(node.Expression));
		using (new PlaceholderRegion(this, instance))
		{
			Visit(node.AwaitableInfo);
			return null;
		}
	}

	private void GetAwaitableInstancePlaceholders(ArrayBuilder<(BoundValuePlaceholderBase, SafeContextAndLocation)> placeholders, BoundAwaitableInfo awaitableInfo, SafeContext valEscapeScope)
	{
		BoundAwaitableValuePlaceholder awaitableInstancePlaceholder = awaitableInfo.AwaitableInstancePlaceholder;
		if (awaitableInstancePlaceholder != null)
		{
			placeholders.Add((awaitableInstancePlaceholder, SafeContextAndLocation.Create(valEscapeScope)));
		}
		BoundAwaitableValuePlaceholder runtimeAsyncAwaitCallPlaceholder = awaitableInfo.RuntimeAsyncAwaitCallPlaceholder;
		if (runtimeAsyncAwaitCallPlaceholder != null)
		{
			placeholders.Add((runtimeAsyncAwaitCallPlaceholder, SafeContextAndLocation.Create(valEscapeScope)));
		}
	}

	public override BoundNode? VisitImplicitIndexerAccess(BoundImplicitIndexerAccess node)
	{
		base.VisitImplicitIndexerAccess(node);
		return null;
	}

	public override BoundNode? VisitDeconstructionAssignmentOperator(BoundDeconstructionAssignmentOperator node)
	{
		base.VisitDeconstructionAssignmentOperator(node);
		BoundTupleExpression left = node.Left;
		BoundConversion right = node.Right;
		ArrayBuilder<DeconstructionVariable> deconstructionAssignmentVariables = GetDeconstructionAssignmentVariables(left);
		VisitDeconstructionArguments(deconstructionAssignmentVariables, right.Syntax, right.Conversion, right.Operand);
		deconstructionAssignmentVariables.FreeAll((DeconstructionVariable v) => v.NestedVariables);
		return null;
	}

	private void VisitDeconstructionArguments(ArrayBuilder<DeconstructionVariable> variables, SyntaxNode syntax, Conversion conversion, BoundExpression right)
	{
		if (conversion.DeconstructionInfo.IsDefault || !(conversion.DeconstructionInfo.Invocation is BoundCall { IsErroneousNode: false, Method: not null } boundCall))
		{
			return;
		}
		MethodInvocationInfo methodInvocationInfo = MethodInvocationInfo.FromCall(boundCall);
		methodInvocationInfo = ReplaceWithExtensionImplementationIfNeeded(in methodInvocationInfo);
		ArrayBuilder<(BoundValuePlaceholderBase, SafeContextAndLocation)> instance = ArrayBuilder<(BoundValuePlaceholderBase, SafeContextAndLocation)>.GetInstance();
		instance.Add((conversion.DeconstructionInfo.InputPlaceholder, SafeContextAndLocation.Create(GetValEscape(right))));
		int count = variables.Count;
		int num = ((boundCall.InvokedAsExtensionMethod || boundCall.Method.IsExtensionBlockMember()) ? 1 : 0);
		for (int i = 0; i < count; i++)
		{
			DeconstructionVariable deconstructionVariable = variables[i];
			ArrayBuilder<DeconstructionVariable>? nestedVariables = deconstructionVariable.NestedVariables;
			BoundDeconstructValuePlaceholder item = (BoundDeconstructValuePlaceholder)methodInvocationInfo.ArgsOpt[i + num];
			SafeContext context = ((nestedVariables == null) ? GetValEscape(deconstructionVariable.Expression) : _localScopeDepth);
			instance.Add((item, SafeContextAndLocation.Create(context)));
		}
		using (new PlaceholderRegion(this, instance))
		{
			if (num == 0)
			{
				Visit(methodInvocationInfo.Receiver);
			}
			else
			{
				Visit(methodInvocationInfo.ArgsOpt[0]);
			}
			CheckInvocationArgMixing(syntax, in methodInvocationInfo, boundCall.Method, _diagnostics);
			for (int j = 0; j < count; j++)
			{
				ArrayBuilder<DeconstructionVariable> nestedVariables2 = variables[j].NestedVariables;
				if (nestedVariables2 != null)
				{
					(BoundValuePlaceholder? placeholder, BoundExpression? conversion) tuple = conversion.DeconstructConversionInfo[j];
					Conversion conversion2 = BoundNode.GetConversion(placeholder: tuple.placeholder, conversion: tuple.conversion);
					VisitDeconstructionArguments(nestedVariables2, syntax, conversion2, methodInvocationInfo.ArgsOpt[j + num]);
				}
			}
		}
	}

	private ArrayBuilder<DeconstructionVariable> GetDeconstructionAssignmentVariables(BoundTupleExpression tuple)
	{
		ImmutableArray<BoundExpression> arguments = tuple.Arguments;
		ArrayBuilder<DeconstructionVariable> instance = ArrayBuilder<DeconstructionVariable>.GetInstance(arguments.Length);
		foreach (BoundExpression item in arguments)
		{
			instance.Add(getDeconstructionAssignmentVariable(item));
		}
		return instance;
		DeconstructionVariable getDeconstructionAssignmentVariable(BoundExpression expr)
		{
			if (!(expr is BoundTupleExpression tuple2))
			{
				return new DeconstructionVariable(expr, GetValEscape(expr), null);
			}
			return new DeconstructionVariable(expr, SafeContext.Empty, GetDeconstructionAssignmentVariables(tuple2));
		}
	}

	private static ImmutableArray<BoundExpression> GetDeconstructionRightParts(BoundExpression expr)
	{
		if (!(expr is BoundTupleExpression boundTupleExpression))
		{
			if (expr is BoundConversion { ConversionKind: var conversionKind } boundConversion && (conversionKind == ConversionKind.Identity || conversionKind == ConversionKind.ImplicitTupleLiteral))
			{
				return GetDeconstructionRightParts(boundConversion.Operand);
			}
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/RefSafetyAnalysis.cs", 1246);
		}
		return boundTupleExpression.Arguments;
	}

	public override BoundNode? VisitForEachStatement(BoundForEachStatement node)
	{
		Visit(node.Expression);
		ForEachEnumeratorInfo enumeratorInfoOpt = node.EnumeratorInfoOpt;
		BoundExpression inlineArray;
		if (enumeratorInfoOpt != null && enumeratorInfoOpt.InlineArraySpanType != WellKnownType.Unknown && !enumeratorInfoOpt.InlineArrayUsedAsValue)
		{
			if (node.Expression is BoundConversion { Conversion: { IsIdentity: not false }, ExplicitCastInCode: false } boundConversion)
			{
				BoundExpression operand = boundConversion.Operand;
				if (operand != null)
				{
					inlineArray = operand;
					goto IL_0078;
				}
			}
			inlineArray = node.Expression;
			goto IL_0078;
		}
		SafeContext safeContext = GetValEscape(node.Expression);
		goto IL_00c4;
		IL_00c4:
		using (new LocalScope(this, ImmutableArray<LocalSymbol>.Empty))
		{
			foreach (LocalSymbol iterationVariable in node.IterationVariables)
			{
				AddLocalScopes(iterationVariable, (iterationVariable.RefKind == RefKind.None) ? _localScopeDepth : safeContext, safeContext);
			}
			ArrayBuilder<(BoundValuePlaceholderBase, SafeContextAndLocation)> instance = ArrayBuilder<(BoundValuePlaceholderBase, SafeContextAndLocation)>.GetInstance();
			BoundDeconstructValuePlaceholder boundDeconstructValuePlaceholder = node.DeconstructionOpt?.TargetPlaceholder;
			if (boundDeconstructValuePlaceholder != null)
			{
				instance.Add((boundDeconstructValuePlaceholder, SafeContextAndLocation.Create(safeContext)));
			}
			enumeratorInfoOpt = node.EnumeratorInfoOpt;
			BoundAwaitableInfo boundAwaitableInfo;
			if (enumeratorInfoOpt != null)
			{
				boundAwaitableInfo = enumeratorInfoOpt.MoveNextAwaitableInfo;
				if (boundAwaitableInfo != null)
				{
					GetAwaitableInstancePlaceholders(instance, boundAwaitableInfo, safeContext);
					goto IL_0168;
				}
			}
			boundAwaitableInfo = null;
			goto IL_0168;
			IL_0168:
			using (new PlaceholderRegion(this, instance))
			{
				Visit(node.IterationVariableType);
				Visit(node.IterationErrorExpressionOpt);
				Visit(node.DeconstructionOpt);
				Visit(boundAwaitableInfo);
				Visit(node.Body);
				foreach (LocalSymbol iterationVariable2 in node.IterationVariables)
				{
					RemoveLocalScopes(iterationVariable2);
				}
				return null;
			}
		}
		IL_0078:
		SignatureOnlyMethodSymbol inlineArrayConversionEquivalentSignatureMethod = GetInlineArrayConversionEquivalentSignatureMethod(inlineArray, node.EnumeratorInfoOpt.GetEnumeratorInfo.Method.ContainingType, out var arguments, out var refKinds);
		safeContext = GetInvocationEscapeScope(MethodInvocationInfo.FromInlineArrayConversion(inlineArrayConversionEquivalentSignatureMethod, arguments, refKinds, node.HasAnyErrors), isRefEscape: false);
		goto IL_00c4;
	}

	private static void Error(BindingDiagnosticBag diagnostics, ErrorCode code, SyntaxNodeOrToken syntax, params object[] args)
	{
		Location location = syntax.GetLocation();
		Error(diagnostics, code, location, args);
	}

	private static void Error(BindingDiagnosticBag diagnostics, ErrorCode code, Location location, params object[] args)
	{
		diagnostics.Add(new CSDiagnostic(new CSDiagnosticInfo(code, args), location));
	}
}
