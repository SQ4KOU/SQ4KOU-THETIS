using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class Conversions : ConversionsBase
{
	private readonly Binder _binder;

	protected override CSharpCompilation Compilation => _binder.Compilation;

	protected override bool IsAttributeArgumentBinding => _binder.InAttributeArgument;

	protected override bool IsParameterDefaultValueBinding => _binder.InParameterDefaultValue;

	public Conversions(Binder binder)
		: this(binder, 0, includeNullability: false, null)
	{
	}

	private Conversions(Binder binder, int currentRecursionDepth, bool includeNullability, Conversions otherNullabilityOpt)
		: base(binder.Compilation.Assembly.CorLibrary, currentRecursionDepth, includeNullability, otherNullabilityOpt)
	{
		_binder = binder;
	}

	protected override ConversionsBase CreateInstance(int currentRecursionDepth)
	{
		return new Conversions(_binder, currentRecursionDepth, IncludeNullability, null);
	}

	protected override ConversionsBase WithNullabilityCore(bool includeNullability)
	{
		return new Conversions(_binder, currentRecursionDepth, includeNullability, this);
	}

	public override Conversion GetMethodGroupDelegateConversion(BoundMethodGroup source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (!destination.IsDelegateType())
		{
			return Conversion.NoConversion;
		}
		var (methodSymbol, isFunctionPointer, callingConventionInfo) = GetDelegateInvokeOrFunctionPointerMethodIfAvailable(destination);
		if ((object)methodSymbol == null)
		{
			return Conversion.NoConversion;
		}
		if (methodSymbol.OriginalDefinition is SynthesizedDelegateInvokeMethod { Parameters: { Length: var length } parameters } synthesizedDelegateInvokeMethod)
		{
			if (length >= 1)
			{
				ParameterSymbol parameterSymbol = parameters[length - 1];
				if ((object)parameterSymbol != null && parameterSymbol.IsParamsArray)
				{
					Binder.AddUseSiteDiagnosticForSynthesizedAttribute(Compilation, WellKnownMember.System_ParamArrayAttribute__ctor, ref useSiteInfo);
				}
			}
			foreach (ParameterSymbol parameter in synthesizedDelegateInvokeMethod.Parameters)
			{
				ConstantValue explicitDefaultConstantValue = parameter.ExplicitDefaultConstantValue;
				if (explicitDefaultConstantValue != null)
				{
					WellKnownMember? wellKnownMember = explicitDefaultConstantValue.SpecialType switch
					{
						SpecialType.System_Decimal => WellKnownMember.System_Runtime_CompilerServices_DecimalConstantAttribute__ctor, 
						SpecialType.System_DateTime => WellKnownMember.System_Runtime_CompilerServices_DateTimeConstantAttribute__ctor, 
						_ => null, 
					};
					if (wellKnownMember.HasValue)
					{
						Binder.AddUseSiteDiagnosticForSynthesizedAttribute(Compilation, wellKnownMember.GetValueOrDefault(), ref useSiteInfo);
					}
				}
			}
			if (synthesizedDelegateInvokeMethod.Parameters.Any((ParameterSymbol p) => p.HasUnscopedRefAttribute))
			{
				Binder.AddUseSiteDiagnosticForSynthesizedAttribute(Compilation, WellKnownMember.System_Diagnostics_CodeAnalysis_UnscopedRefAttribute__ctor, ref useSiteInfo);
			}
		}
		MethodGroupResolution methodGroupResolution = ResolveDelegateOrFunctionPointerMethodGroup(_binder, source, methodSymbol, isFunctionPointer, in callingConventionInfo, ref useSiteInfo);
		Conversion result = ((methodGroupResolution.IsEmpty || methodGroupResolution.HasAnyErrors) ? Conversion.NoConversion : ToConversion(methodGroupResolution.OverloadResolutionResult, methodGroupResolution.MethodGroup, methodSymbol.ParameterCount));
		methodGroupResolution.Free();
		return result;
	}

	public override Conversion GetMethodGroupFunctionPointerConversion(BoundMethodGroup source, FunctionPointerTypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		MethodGroupResolution methodGroupResolution = ResolveDelegateOrFunctionPointerMethodGroup(_binder, source, destination.Signature, isFunctionPointer: true, new CallingConventionInfo(destination.Signature.CallingConvention, destination.Signature.GetCallingConventionModifiers()), ref useSiteInfo);
		Conversion result = ((methodGroupResolution.IsEmpty || methodGroupResolution.HasAnyErrors) ? Conversion.NoConversion : ToConversion(methodGroupResolution.OverloadResolutionResult, methodGroupResolution.MethodGroup, destination.Signature.ParameterCount));
		methodGroupResolution.Free();
		return result;
	}

	protected override Conversion GetInterpolatedStringConversion(BoundExpression source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (_binder.InParameterDefaultValue || _binder.InAttributeArgument)
		{
			return Conversion.NoConversion;
		}
		if (destination is NamedTypeSymbol { IsInterpolatedStringHandlerType: not false })
		{
			return Conversion.InterpolatedStringHandler;
		}
		if (source is BoundBinaryOperator)
		{
			return Conversion.NoConversion;
		}
		if (!TypeSymbol.Equals(destination, Compilation.GetWellKnownType(WellKnownType.System_IFormattable), TypeCompareKind.ConsiderEverything) && !TypeSymbol.Equals(destination, Compilation.GetWellKnownType(WellKnownType.System_FormattableString), TypeCompareKind.ConsiderEverything))
		{
			return Conversion.NoConversion;
		}
		return Conversion.InterpolatedString;
	}

	protected override Conversion GetCollectionExpressionConversion(BoundUnconvertedCollectionExpression node, TypeSymbol targetType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		SyntaxNode syntax = node.Syntax;
		CollectionExpressionTypeKind collectionExpressionTypeKind = ConversionsBase.GetCollectionExpressionTypeKind(Compilation, targetType, out var elementType);
		TypeSymbol type = elementType.Type;
		switch (collectionExpressionTypeKind)
		{
		case CollectionExpressionTypeKind.None:
			return Conversion.NoConversion;
		case CollectionExpressionTypeKind.CollectionBuilder:
		case CollectionExpressionTypeKind.ImplementsIEnumerable:
			_binder.TryGetCollectionIterationType(syntax, targetType, out elementType);
			type = elementType.Type;
			if ((object)type == null)
			{
				return Conversion.NoConversion;
			}
			break;
		}
		ImmutableArray<BoundNode> elements = node.Elements;
		MethodSymbol constructor = null;
		bool isExpanded = false;
		if (collectionExpressionTypeKind == CollectionExpressionTypeKind.ImplementsIEnumerable)
		{
			if (!_binder.HasCollectionExpressionApplicableConstructor(syntax, targetType, out constructor, out isExpanded, BindingDiagnosticBag.Discarded))
			{
				return Conversion.NoConversion;
			}
			if (elements.Length > 0 && !_binder.HasCollectionExpressionApplicableAddMethod(syntax, targetType, out ImmutableArray<MethodSymbol> _, BindingDiagnosticBag.Discarded))
			{
				return Conversion.NoConversion;
			}
		}
		ArrayBuilder<Conversion> instance = ArrayBuilder<Conversion>.GetInstance(elements.Length);
		foreach (BoundNode item2 in elements)
		{
			Conversion item = convertElement(item2, type, ref useSiteInfo);
			if (!item.Exists)
			{
				instance.Free();
				return Conversion.NoConversion;
			}
			instance.Add(item);
		}
		return Conversion.CreateCollectionExpressionConversion(collectionExpressionTypeKind, type, constructor, isExpanded, instance.ToImmutableAndFree());
		Conversion convertElement(BoundNode element, TypeSymbol typeSymbol, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2)
		{
			if (element is BoundCollectionExpressionSpreadElement element2)
			{
				return GetCollectionExpressionSpreadElementConversion(element2, typeSymbol, ref useSiteInfo2);
			}
			return ClassifyImplicitConversionFromExpression((BoundExpression)element, typeSymbol, ref useSiteInfo2);
		}
	}

	internal Conversion GetCollectionExpressionSpreadElementConversion(BoundCollectionExpressionSpreadElement element, TypeSymbol targetType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ForEachEnumeratorInfo enumeratorInfoOpt = element.EnumeratorInfoOpt;
		if (enumeratorInfoOpt == null)
		{
			return Conversion.NoConversion;
		}
		return ClassifyImplicitConversionFromExpression(new BoundValuePlaceholder(element.Syntax, enumeratorInfoOpt.ElementType), targetType, ref useSiteInfo);
	}

	private static MethodGroupResolution ResolveDelegateOrFunctionPointerMethodGroup(Binder binder, BoundMethodGroup source, MethodSymbol delegateInvokeMethodOpt, bool isFunctionPointer, in CallingConventionInfo callingConventionInfo, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		MethodGroupResolution result;
		if ((object)delegateInvokeMethodOpt != null)
		{
			AnalyzedArguments instance = AnalyzedArguments.GetInstance();
			GetDelegateOrFunctionPointerArguments(source.Syntax, instance, delegateInvokeMethodOpt.Parameters, binder.Compilation);
			result = binder.ResolveMethodGroup(source, instance, ref useSiteInfo, (OverloadResolution.Options)(5 | (isFunctionPointer ? 16 : 0)), acceptOnlyMethods: true, delegateInvokeMethodOpt.RefKind, delegateInvokeMethodOpt.ReturnType, in callingConventionInfo);
			instance.Free();
		}
		else
		{
			result = binder.ResolveMethodGroup(source, null, ref useSiteInfo, OverloadResolution.Options.IsMethodGroupConversion, acceptOnlyMethods: true, RefKind.None, null, default(CallingConventionInfo));
		}
		return result;
	}

	private static (MethodSymbol, bool isFunctionPointer, CallingConventionInfo callingConventionInfo) GetDelegateInvokeOrFunctionPointerMethodIfAvailable(TypeSymbol type)
	{
		if (type is FunctionPointerTypeSymbol functionPointerTypeSymbol)
		{
			FunctionPointerMethodSymbol signature = functionPointerTypeSymbol.Signature;
			if ((object)signature != null)
			{
				return (signature, isFunctionPointer: true, callingConventionInfo: new CallingConventionInfo(signature.CallingConvention, signature.GetCallingConventionModifiers()));
			}
		}
		NamedTypeSymbol delegateType = type.GetDelegateType();
		if ((object)delegateType == null)
		{
			return (null, isFunctionPointer: false, callingConventionInfo: default(CallingConventionInfo));
		}
		MethodSymbol delegateInvokeMethod = delegateType.DelegateInvokeMethod;
		if ((object)delegateInvokeMethod == null || delegateInvokeMethod.HasUseSiteError)
		{
			return (null, isFunctionPointer: false, callingConventionInfo: default(CallingConventionInfo));
		}
		return (delegateInvokeMethod, isFunctionPointer: false, callingConventionInfo: default(CallingConventionInfo));
	}

	public static bool ReportDelegateOrFunctionPointerMethodGroupDiagnostics(Binder binder, BoundMethodGroup expr, TypeSymbol targetType, BindingDiagnosticBag diagnostics)
	{
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = binder.GetNewCompoundUseSiteInfo(diagnostics);
		(MethodSymbol, bool isFunctionPointer, CallingConventionInfo callingConventionInfo) delegateInvokeOrFunctionPointerMethodIfAvailable = GetDelegateInvokeOrFunctionPointerMethodIfAvailable(targetType);
		MethodSymbol item = delegateInvokeOrFunctionPointerMethodIfAvailable.Item1;
		bool item2 = delegateInvokeOrFunctionPointerMethodIfAvailable.isFunctionPointer;
		CallingConventionInfo callingConventionInfo = delegateInvokeOrFunctionPointerMethodIfAvailable.callingConventionInfo;
		MethodGroupResolution methodGroupResolution = ResolveDelegateOrFunctionPointerMethodGroup(binder, expr, item, item2, in callingConventionInfo, ref useSiteInfo);
		diagnostics.Add(expr.Syntax, useSiteInfo);
		bool flag = methodGroupResolution.HasAnyErrors;
		diagnostics.AddRange(methodGroupResolution.Diagnostics);
		if (methodGroupResolution.MethodGroup != null)
		{
			OverloadResolutionResult<MethodSymbol> overloadResolutionResult = methodGroupResolution.OverloadResolutionResult;
			if (overloadResolutionResult != null)
			{
				if (overloadResolutionResult.Succeeded)
				{
					MethodSymbol member = overloadResolutionResult.BestResult.Member;
					if (methodGroupResolution.MethodGroup.IsExtensionMethodGroup)
					{
						ParameterSymbol parameterSymbol = (member.IsExtensionMethod ? member.Parameters[0] : ((!member.IsStatic) ? member.ContainingType.ExtensionParameter : null));
						if ((object)parameterSymbol != null && !parameterSymbol.Type.IsReferenceType)
						{
							diagnostics.Add(ErrorCode.ERR_ValueTypeExtDelegate, expr.Syntax.Location, member, parameterSymbol.Type);
							flag = true;
						}
					}
					else if (member.ContainingType.IsNullableType() && !member.IsOverride)
					{
						diagnostics.Add(ErrorCode.ERR_DelegateOnNullable, expr.Syntax.Location, member);
						flag = true;
					}
				}
				else if (!flag && !methodGroupResolution.IsEmpty && methodGroupResolution.ResultKind == LookupResultKind.Viable)
				{
					BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: true, diagnostics.AccumulatesDependencies);
					overloadResolutionResult.ReportDiagnostics(binder, expr.Syntax.Location, expr.Syntax, instance, expr.Name, methodGroupResolution.MethodGroup.Receiver, expr.Syntax, methodGroupResolution.AnalyzedArguments, methodGroupResolution.MethodGroup.Methods.ToImmutable(), null, null, null, isMethodGroupConversion: true, item?.RefKind, targetType);
					flag = instance.HasAnyErrors();
					diagnostics.AddRangeAndFree(instance);
				}
			}
		}
		methodGroupResolution.Free();
		return flag;
	}

	public Conversion MethodGroupConversion(SyntaxNode syntax, MethodGroup methodGroup, NamedTypeSymbol delegateType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		AnalyzedArguments instance = AnalyzedArguments.GetInstance();
		OverloadResolutionResult<MethodSymbol> instance2 = OverloadResolutionResult<MethodSymbol>.GetInstance();
		MethodSymbol delegateInvokeMethod = delegateType.DelegateInvokeMethod;
		GetDelegateOrFunctionPointerArguments(syntax, instance, delegateInvokeMethod.Parameters, Compilation);
		_binder.OverloadResolution.MethodInvocationOverloadResolution(methodGroup.Methods, methodGroup.TypeArguments, methodGroup.Receiver, instance, instance2, ref useSiteInfo, OverloadResolution.Options.IsMethodGroupConversion, delegateInvokeMethod.RefKind, delegateInvokeMethod.ReturnType, default(CallingConventionInfo));
		Conversion result = ToConversion(instance2, methodGroup, delegateType.DelegateInvokeMethod.ParameterCount);
		instance.Free();
		instance2.Free();
		return result;
	}

	public static void GetDelegateOrFunctionPointerArguments(SyntaxNode syntax, AnalyzedArguments analyzedArguments, ImmutableArray<ParameterSymbol> delegateParameters, CSharpCompilation compilation)
	{
		foreach (ParameterSymbol item in delegateParameters)
		{
			ParameterSymbol parameterSymbol = item;
			if (parameterSymbol.Type.IsDynamic())
			{
				parameterSymbol = new SignatureOnlyParameterSymbol(TypeWithAnnotations.Create(compilation.GetSpecialType(SpecialType.System_Object), NullableAnnotation.Oblivious, parameterSymbol.TypeWithAnnotations.CustomModifiers), parameterSymbol.RefCustomModifiers, parameterSymbol.IsParamsArray, parameterSymbol.IsParamsCollection, parameterSymbol.RefKind);
			}
			analyzedArguments.Arguments.Add(new BoundParameter(syntax, parameterSymbol)
			{
				WasCompilerGenerated = true
			});
			analyzedArguments.RefKinds.Add(parameterSymbol.RefKind);
		}
	}

	private static Conversion ToConversion(OverloadResolutionResult<MethodSymbol> result, MethodGroup methodGroup, int parameterCount)
	{
		if (!result.Succeeded)
		{
			return Conversion.NoConversion;
		}
		MethodSymbol member = result.BestResult.Member;
		if (methodGroup.IsExtensionMethodGroup && (!member.IsExtensionBlockMember() || !member.IsStatic) && !Binder.GetReceiverParameter(member).Type.IsReferenceType)
		{
			return Conversion.NoConversion;
		}
		if (member.RequiresInstanceReceiver)
		{
			BoundExpression receiver = methodGroup.Receiver;
			if (receiver != null && receiver.Type?.IsRestrictedType() == true)
			{
				return Conversion.NoConversion;
			}
		}
		if (member.ContainingType.IsNullableType() && !member.IsOverride)
		{
			return Conversion.NoConversion;
		}
		bool isExtensionMethod = methodGroup.IsExtensionMethodGroup && !member.IsExtensionBlockMember();
		return new Conversion(ConversionKind.MethodGroup, member, isExtensionMethod);
	}

	public override Conversion GetStackAllocConversion(BoundStackAllocArrayCreation sourceExpression, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (sourceExpression.NeedsToBeConverted())
		{
			PointerTypeSymbol source = new PointerTypeSymbol(TypeWithAnnotations.Create(sourceExpression.ElementType));
			Conversion underlyingConversion = ClassifyImplicitConversionFromType(source, destination, ref useSiteInfo);
			if (underlyingConversion.IsValid)
			{
				return Conversion.MakeStackAllocToPointerType(underlyingConversion);
			}
			NamedTypeSymbol wellKnownType = _binder.GetWellKnownType(WellKnownType.System_Span_T, ref useSiteInfo);
			if (wellKnownType.TypeKind == TypeKind.Struct && wellKnownType.IsRefLikeType)
			{
				NamedTypeSymbol source2 = wellKnownType.Construct(sourceExpression.ElementType);
				Conversion underlyingConversion2 = ClassifyImplicitConversionFromType(source2, destination, ref useSiteInfo);
				if (underlyingConversion2.Exists)
				{
					return Conversion.MakeStackAllocToSpanType(underlyingConversion2);
				}
			}
		}
		return Conversion.NoConversion;
	}

	internal new Conversions WithNullability(bool includeNullability)
	{
		return (Conversions)base.WithNullability(includeNullability);
	}
}
