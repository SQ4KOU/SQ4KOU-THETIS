using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal class ExpressionLambdaRewriter
{
	private enum InitializerKind
	{
		Expression,
		MemberInitializer,
		CollectionInitializer
	}

	private readonly SyntheticBoundNodeFactory _bound;

	private readonly TypeMap _typeMap;

	private readonly Dictionary<ParameterSymbol, BoundExpression> _parameterMap = new Dictionary<ParameterSymbol, BoundExpression>();

	private Dictionary<BoundValuePlaceholder, BoundExpression> _placeholderReplacementMap;

	private int _recursionDepth;

	private NamedTypeSymbol _ExpressionType;

	private NamedTypeSymbol _ParameterExpressionType;

	private NamedTypeSymbol _ElementInitType;

	private NamedTypeSymbol _MemberBindingType;

	private readonly NamedTypeSymbol _int32Type;

	private readonly NamedTypeSymbol _objectType;

	private readonly NamedTypeSymbol _nullableType;

	private NamedTypeSymbol _MemberInfoType;

	private readonly NamedTypeSymbol _IEnumerableType;

	private NamedTypeSymbol ExpressionType
	{
		get
		{
			if ((object)_ExpressionType == null)
			{
				_ExpressionType = _bound.WellKnownType(WellKnownType.System_Linq_Expressions_Expression);
			}
			return _ExpressionType;
		}
	}

	private NamedTypeSymbol ParameterExpressionType
	{
		get
		{
			if ((object)_ParameterExpressionType == null)
			{
				_ParameterExpressionType = _bound.WellKnownType(WellKnownType.System_Linq_Expressions_ParameterExpression);
			}
			return _ParameterExpressionType;
		}
	}

	private NamedTypeSymbol ElementInitType
	{
		get
		{
			if ((object)_ElementInitType == null)
			{
				_ElementInitType = _bound.WellKnownType(WellKnownType.System_Linq_Expressions_ElementInit);
			}
			return _ElementInitType;
		}
	}

	public NamedTypeSymbol MemberBindingType
	{
		get
		{
			if ((object)_MemberBindingType == null)
			{
				_MemberBindingType = _bound.WellKnownType(WellKnownType.System_Linq_Expressions_MemberBinding);
			}
			return _MemberBindingType;
		}
	}

	private NamedTypeSymbol MemberInfoType
	{
		get
		{
			if ((object)_MemberInfoType == null)
			{
				_MemberInfoType = _bound.WellKnownType(WellKnownType.System_Reflection_MemberInfo);
			}
			return _MemberInfoType;
		}
	}

	private BindingDiagnosticBag Diagnostics => _bound.Diagnostics;

	private ExpressionLambdaRewriter(TypeCompilationState compilationState, TypeMap typeMap, SyntaxNode node, int recursionDepth, BindingDiagnosticBag diagnostics)
	{
		_bound = new SyntheticBoundNodeFactory(null, compilationState.Type, node, compilationState, diagnostics);
		_int32Type = _bound.SpecialType(SpecialType.System_Int32);
		_objectType = _bound.SpecialType(SpecialType.System_Object);
		_nullableType = _bound.SpecialType(SpecialType.System_Nullable_T);
		_IEnumerableType = _bound.SpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);
		_typeMap = typeMap;
		_recursionDepth = recursionDepth;
	}

	internal static BoundNode RewriteLambda(BoundLambda node, TypeCompilationState compilationState, TypeMap typeMap, int recursionDepth, BindingDiagnosticBag diagnostics)
	{
		try
		{
			ExpressionLambdaRewriter expressionLambdaRewriter = new ExpressionLambdaRewriter(compilationState, typeMap, node.Syntax, recursionDepth, diagnostics);
			BoundExpression boundExpression = expressionLambdaRewriter.VisitLambdaInternal(node);
			if (!node.Type.Equals(boundExpression.Type, TypeCompareKind.IgnoreNullableModifiersForReferenceTypes))
			{
				diagnostics.Add(ErrorCode.ERR_MissingPredefinedMember, node.Syntax.Location, expressionLambdaRewriter.ExpressionType, "Lambda");
			}
			return boundExpression;
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
			return node;
		}
	}

	private BoundExpression TranslateLambdaBody(BoundBlock block)
	{
		foreach (BoundStatement statement in block.Statements)
		{
			BoundStatement boundStatement = statement;
			while (boundStatement != null)
			{
				switch (boundStatement.Kind)
				{
				case BoundKind.ReturnStatement:
				{
					BoundExpression boundExpression = Visit(((BoundReturnStatement)boundStatement).ExpressionOpt);
					if (boundExpression != null)
					{
						return boundExpression;
					}
					boundStatement = null;
					break;
				}
				case BoundKind.ExpressionStatement:
					return Visit(((BoundExpressionStatement)boundStatement).Expression);
				case BoundKind.SequencePoint:
					boundStatement = ((BoundSequencePoint)boundStatement).StatementOpt;
					break;
				case BoundKind.SequencePointWithSpan:
					boundStatement = ((BoundSequencePointWithSpan)boundStatement).StatementOpt;
					break;
				default:
					throw ExceptionUtilities.UnexpectedValue(boundStatement.Kind);
				}
			}
		}
		return null;
	}

	private BoundExpression Visit(BoundExpression node)
	{
		if (node == null)
		{
			return null;
		}
		SyntaxNode syntax = _bound.Syntax;
		_bound.Syntax = node.Syntax;
		BoundExpression arg = VisitInternal(node);
		_bound.Syntax = syntax;
		Conversion conversion = _bound.ClassifyEmitConversion(arg, ExpressionType);
		return _bound.Convert(ExpressionType, arg, conversion);
	}

	private BoundExpression VisitExpressionWithoutStackGuard(BoundExpression node)
	{
		switch (node.Kind)
		{
		case BoundKind.ArrayAccess:
			return VisitArrayAccess((BoundArrayAccess)node);
		case BoundKind.ArrayCreation:
			return VisitArrayCreation((BoundArrayCreation)node);
		case BoundKind.ArrayLength:
			return VisitArrayLength((BoundArrayLength)node);
		case BoundKind.AsOperator:
			return VisitAsOperator((BoundAsOperator)node);
		case BoundKind.BaseReference:
			return VisitBaseReference((BoundBaseReference)node);
		case BoundKind.BinaryOperator:
		{
			BoundBinaryOperator boundBinaryOperator = (BoundBinaryOperator)node;
			return VisitBinaryOperator(boundBinaryOperator.OperatorKind, boundBinaryOperator.BinaryOperatorMethod, boundBinaryOperator.Type, boundBinaryOperator.Left, boundBinaryOperator.Right);
		}
		case BoundKind.UserDefinedConditionalLogicalOperator:
		{
			BoundUserDefinedConditionalLogicalOperator boundUserDefinedConditionalLogicalOperator = (BoundUserDefinedConditionalLogicalOperator)node;
			return VisitBinaryOperator(boundUserDefinedConditionalLogicalOperator.OperatorKind, boundUserDefinedConditionalLogicalOperator.LogicalOperator, boundUserDefinedConditionalLogicalOperator.Type, boundUserDefinedConditionalLogicalOperator.Left, boundUserDefinedConditionalLogicalOperator.Right);
		}
		case BoundKind.Call:
			return VisitCall((BoundCall)node);
		case BoundKind.ConditionalOperator:
			return VisitConditionalOperator((BoundConditionalOperator)node);
		case BoundKind.Conversion:
			return VisitConversion((BoundConversion)node);
		case BoundKind.PassByCopy:
			return Visit(((BoundPassByCopy)node).Expression);
		case BoundKind.DelegateCreationExpression:
			return VisitDelegateCreationExpression((BoundDelegateCreationExpression)node);
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)node;
			if (boundFieldAccess.FieldSymbol.IsCapturedFrame)
			{
				return Constant(boundFieldAccess);
			}
			return VisitFieldAccess(boundFieldAccess);
		}
		case BoundKind.IsOperator:
			return VisitIsOperator((BoundIsOperator)node);
		case BoundKind.Lambda:
			return VisitLambda((BoundLambda)node);
		case BoundKind.NewT:
			return VisitNewT((BoundNewT)node);
		case BoundKind.NullCoalescingOperator:
			return VisitNullCoalescingOperator((BoundNullCoalescingOperator)node);
		case BoundKind.ObjectCreationExpression:
			return VisitObjectCreationExpression((BoundObjectCreationExpression)node);
		case BoundKind.Parameter:
			return VisitParameter((BoundParameter)node);
		case BoundKind.PointerIndirectionOperator:
			return VisitPointerIndirectionOperator((BoundPointerIndirectionOperator)node);
		case BoundKind.PointerElementAccess:
			return VisitPointerElementAccess((BoundPointerElementAccess)node);
		case BoundKind.PropertyAccess:
			return VisitPropertyAccess((BoundPropertyAccess)node);
		case BoundKind.SizeOfOperator:
			return VisitSizeOfOperator((BoundSizeOfOperator)node);
		case BoundKind.UnaryOperator:
			return VisitUnaryOperator((BoundUnaryOperator)node);
		case BoundKind.TypeOfOperator:
		case BoundKind.MethodInfo:
		case BoundKind.DefaultExpression:
		case BoundKind.Literal:
		case BoundKind.ThisReference:
		case BoundKind.PreviousSubmissionReference:
		case BoundKind.HostObjectMemberReference:
		case BoundKind.Local:
			return Constant(node);
		case BoundKind.ValuePlaceholder:
			return _placeholderReplacementMap[(BoundValuePlaceholder)node];
		default:
			throw ExceptionUtilities.UnexpectedValue(node.Kind);
		}
	}

	private BoundExpression VisitInternal(BoundExpression node)
	{
		_recursionDepth++;
		BoundExpression result;
		if (_recursionDepth > 1)
		{
			StackGuard.EnsureSufficientExecutionStack(_recursionDepth);
			result = VisitExpressionWithoutStackGuard(node);
		}
		else
		{
			result = VisitExpressionWithStackGuard(node);
		}
		_recursionDepth--;
		return result;
	}

	private BoundExpression VisitExpressionWithStackGuard(BoundExpression node)
	{
		try
		{
			return VisitExpressionWithoutStackGuard(node);
		}
		catch (InsufficientExecutionStackException inner)
		{
			throw new BoundTreeVisitor.CancelledByStackGuardException(inner, node);
		}
	}

	private BoundExpression VisitArrayAccess(BoundArrayAccess node)
	{
		BoundExpression boundExpression = Visit(node.Expression);
		if (node.Indices.Length == 1)
		{
			BoundExpression boundExpression2 = node.Indices[0];
			BoundExpression boundExpression3 = Visit(boundExpression2);
			if (!TypeSymbol.Equals(boundExpression3.Type, _int32Type, TypeCompareKind.ConsiderEverything))
			{
				boundExpression3 = ConvertIndex(boundExpression3, boundExpression2.Type, _int32Type);
			}
			return _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__ArrayIndex_Expression_Expression, boundExpression, boundExpression3);
		}
		return _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__ArrayIndex_Expression_Expressions, boundExpression, Indices(node.Indices));
	}

	private BoundExpression Indices(ImmutableArray<BoundExpression> expressions)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		foreach (BoundExpression item in expressions)
		{
			BoundExpression boundExpression = Visit(item);
			if (!TypeSymbol.Equals(boundExpression.Type, _int32Type, TypeCompareKind.ConsiderEverything))
			{
				boundExpression = ConvertIndex(boundExpression, item.Type, _int32Type);
			}
			instance.Add(boundExpression);
		}
		return _bound.ArrayOrEmpty(ExpressionType, instance.ToImmutableAndFree());
	}

	private BoundExpression Expressions(ImmutableArray<BoundExpression> expressions)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		foreach (BoundExpression item in expressions)
		{
			instance.Add(Visit(item));
		}
		return _bound.ArrayOrEmpty(ExpressionType, instance.ToImmutableAndFree());
	}

	private BoundExpression VisitArrayCreation(BoundArrayCreation node)
	{
		ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)node.Type;
		BoundExpression boundExpression = _bound.Typeof(arrayTypeSymbol.ElementType, _bound.WellKnownType(WellKnownType.System_Type));
		if (node.InitializerOpt != null)
		{
			if (arrayTypeSymbol.IsSZArray)
			{
				return _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__NewArrayInit, boundExpression, Expressions(node.InitializerOpt.Initializers));
			}
			return new BoundBadExpression(node.Syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundExpression)node), ExpressionType);
		}
		return _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__NewArrayBounds, boundExpression, Expressions(node.Bounds));
	}

	private BoundExpression VisitArrayLength(BoundArrayLength node)
	{
		return _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__ArrayLength, Visit(node.Expression));
	}

	private BoundExpression VisitAsOperator(BoundAsOperator node)
	{
		if (node.Operand.IsLiteralNull() && (object)node.Operand.Type == null)
		{
			BoundExpression operand = _bound.Null(_bound.SpecialType(SpecialType.System_Object));
			node = node.Update(operand, node.TargetType, node.OperandPlaceholder, node.OperandConversion, node.Type);
		}
		return _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__TypeAs, Visit(node.Operand), _bound.Typeof(node.Type, _bound.WellKnownType(WellKnownType.System_Type)));
	}

	private BoundExpression VisitBaseReference(BoundBaseReference node)
	{
		return new BoundBadExpression(node.Syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundExpression)node), ExpressionType);
	}

	private static WellKnownMember GetBinaryOperatorFactory(BinaryOperatorKind opKind, MethodSymbol methodOpt, out bool isChecked, out bool isLifted, out bool requiresLifted)
	{
		isChecked = opKind.IsChecked();
		isLifted = opKind.IsLifted();
		requiresLifted = opKind.IsComparison();
		switch (opKind.Operator())
		{
		case BinaryOperatorKind.Addition:
			if (!useCheckedFactory(isChecked, methodOpt))
			{
				if ((object)methodOpt != null)
				{
					return WellKnownMember.System_Linq_Expressions_Expression__Add_MethodInfo;
				}
				return WellKnownMember.System_Linq_Expressions_Expression__Add;
			}
			if ((object)methodOpt != null)
			{
				return WellKnownMember.System_Linq_Expressions_Expression__AddChecked_MethodInfo;
			}
			return WellKnownMember.System_Linq_Expressions_Expression__AddChecked;
		case BinaryOperatorKind.Multiplication:
			if (!useCheckedFactory(isChecked, methodOpt))
			{
				if ((object)methodOpt != null)
				{
					return WellKnownMember.System_Linq_Expressions_Expression__Multiply_MethodInfo;
				}
				return WellKnownMember.System_Linq_Expressions_Expression__Multiply;
			}
			if ((object)methodOpt != null)
			{
				return WellKnownMember.System_Linq_Expressions_Expression__MultiplyChecked_MethodInfo;
			}
			return WellKnownMember.System_Linq_Expressions_Expression__MultiplyChecked;
		case BinaryOperatorKind.Subtraction:
			if (!useCheckedFactory(isChecked, methodOpt))
			{
				if ((object)methodOpt != null)
				{
					return WellKnownMember.System_Linq_Expressions_Expression__Subtract_MethodInfo;
				}
				return WellKnownMember.System_Linq_Expressions_Expression__Subtract;
			}
			if ((object)methodOpt != null)
			{
				return WellKnownMember.System_Linq_Expressions_Expression__SubtractChecked_MethodInfo;
			}
			return WellKnownMember.System_Linq_Expressions_Expression__SubtractChecked;
		case BinaryOperatorKind.Division:
			if ((object)methodOpt != null)
			{
				return WellKnownMember.System_Linq_Expressions_Expression__Divide_MethodInfo;
			}
			return WellKnownMember.System_Linq_Expressions_Expression__Divide;
		case BinaryOperatorKind.Remainder:
			if ((object)methodOpt != null)
			{
				return WellKnownMember.System_Linq_Expressions_Expression__Modulo_MethodInfo;
			}
			return WellKnownMember.System_Linq_Expressions_Expression__Modulo;
		case BinaryOperatorKind.And:
			if (!opKind.IsLogical())
			{
				if ((object)methodOpt != null)
				{
					return WellKnownMember.System_Linq_Expressions_Expression__And_MethodInfo;
				}
				return WellKnownMember.System_Linq_Expressions_Expression__And;
			}
			if ((object)methodOpt != null)
			{
				return WellKnownMember.System_Linq_Expressions_Expression__AndAlso_MethodInfo;
			}
			return WellKnownMember.System_Linq_Expressions_Expression__AndAlso;
		case BinaryOperatorKind.Xor:
			if ((object)methodOpt != null)
			{
				return WellKnownMember.System_Linq_Expressions_Expression__ExclusiveOr_MethodInfo;
			}
			return WellKnownMember.System_Linq_Expressions_Expression__ExclusiveOr;
		case BinaryOperatorKind.Or:
			if (!opKind.IsLogical())
			{
				if ((object)methodOpt != null)
				{
					return WellKnownMember.System_Linq_Expressions_Expression__Or_MethodInfo;
				}
				return WellKnownMember.System_Linq_Expressions_Expression__Or;
			}
			if ((object)methodOpt != null)
			{
				return WellKnownMember.System_Linq_Expressions_Expression__OrElse_MethodInfo;
			}
			return WellKnownMember.System_Linq_Expressions_Expression__OrElse;
		case BinaryOperatorKind.LeftShift:
			if ((object)methodOpt != null)
			{
				return WellKnownMember.System_Linq_Expressions_Expression__LeftShift_MethodInfo;
			}
			return WellKnownMember.System_Linq_Expressions_Expression__LeftShift;
		case BinaryOperatorKind.RightShift:
			if ((object)methodOpt != null)
			{
				return WellKnownMember.System_Linq_Expressions_Expression__RightShift_MethodInfo;
			}
			return WellKnownMember.System_Linq_Expressions_Expression__RightShift;
		case BinaryOperatorKind.Equal:
			if ((object)methodOpt != null)
			{
				return WellKnownMember.System_Linq_Expressions_Expression__Equal_MethodInfo;
			}
			return WellKnownMember.System_Linq_Expressions_Expression__Equal;
		case BinaryOperatorKind.NotEqual:
			if ((object)methodOpt != null)
			{
				return WellKnownMember.System_Linq_Expressions_Expression__NotEqual_MethodInfo;
			}
			return WellKnownMember.System_Linq_Expressions_Expression__NotEqual;
		case BinaryOperatorKind.LessThan:
			if ((object)methodOpt != null)
			{
				return WellKnownMember.System_Linq_Expressions_Expression__LessThan_MethodInfo;
			}
			return WellKnownMember.System_Linq_Expressions_Expression__LessThan;
		case BinaryOperatorKind.LessThanOrEqual:
			if ((object)methodOpt != null)
			{
				return WellKnownMember.System_Linq_Expressions_Expression__LessThanOrEqual_MethodInfo;
			}
			return WellKnownMember.System_Linq_Expressions_Expression__LessThanOrEqual;
		case BinaryOperatorKind.GreaterThan:
			if ((object)methodOpt != null)
			{
				return WellKnownMember.System_Linq_Expressions_Expression__GreaterThan_MethodInfo;
			}
			return WellKnownMember.System_Linq_Expressions_Expression__GreaterThan;
		case BinaryOperatorKind.GreaterThanOrEqual:
			if ((object)methodOpt != null)
			{
				return WellKnownMember.System_Linq_Expressions_Expression__GreaterThanOrEqual_MethodInfo;
			}
			return WellKnownMember.System_Linq_Expressions_Expression__GreaterThanOrEqual;
		default:
			throw ExceptionUtilities.UnexpectedValue(opKind.Operator());
		}
		static bool useCheckedFactory(bool flag, MethodSymbol methodSymbol)
		{
			if (!flag)
			{
				if ((object)methodSymbol != null)
				{
					string name = methodSymbol.Name;
					if (name != null)
					{
						return SyntaxFacts.IsCheckedOperator(name);
					}
				}
				return false;
			}
			return true;
		}
	}

	private BoundExpression VisitBinaryOperator(BinaryOperatorKind opKind, MethodSymbol methodOpt, TypeSymbol type, BoundExpression left, BoundExpression right)
	{
		WellKnownMember binaryOperatorFactory = GetBinaryOperatorFactory(opKind, methodOpt, out var isChecked, out var isLifted, out var requiresLifted);
		if ((object)left.Type == null && left.IsLiteralNull())
		{
			left = _bound.Default(right.Type);
		}
		if ((object)right.Type == null && right.IsLiteralNull())
		{
			right = _bound.Default(left.Type);
		}
		BinaryOperatorKind binaryOperatorKind = opKind.OperandTypes();
		if ((uint)(binaryOperatorKind - 20) <= 2u)
		{
			BoundExpression boundExpression = ((opKind.OperandTypes() == BinaryOperatorKind.UnderlyingAndEnum) ? right : left);
			TypeSymbol typeSymbol = PromotedType(boundExpression.Type.StrippedType().GetEnumUnderlyingType());
			if (opKind.IsLifted())
			{
				typeSymbol = _nullableType.Construct(typeSymbol);
			}
			BoundExpression loweredLeft = VisitAndPromoteEnumOperand(left, typeSymbol, isChecked);
			BoundExpression loweredRight = VisitAndPromoteEnumOperand(right, typeSymbol, isChecked);
			BoundExpression node = MakeBinary(methodOpt, type, isLifted, requiresLifted, binaryOperatorFactory, loweredLeft, loweredRight);
			return Demote(node, type, isChecked);
		}
		BoundExpression loweredLeft2 = Visit(left);
		BoundExpression loweredRight2 = Visit(right);
		return MakeBinary(methodOpt, type, isLifted, requiresLifted, binaryOperatorFactory, loweredLeft2, loweredRight2);
	}

	private static BoundExpression DemoteEnumOperand(BoundExpression operand)
	{
		if (operand.Kind == BoundKind.Conversion)
		{
			BoundConversion boundConversion = (BoundConversion)operand;
			if (!boundConversion.ConversionKind.IsUserDefinedConversion() && boundConversion.ConversionKind.IsImplicitConversion() && boundConversion.ConversionKind != ConversionKind.NullLiteral && boundConversion.Type.StrippedType().IsEnumType())
			{
				operand = boundConversion.Operand;
			}
		}
		return operand;
	}

	private BoundExpression VisitAndPromoteEnumOperand(BoundExpression operand, TypeSymbol promotedType, bool isChecked)
	{
		if (operand is BoundLiteral boundLiteral)
		{
			return Constant(boundLiteral.Update(boundLiteral.ConstantValueOpt, promotedType));
		}
		BoundExpression node = DemoteEnumOperand(operand);
		BoundExpression operand2 = Visit(node);
		return Convert(operand2, operand.Type, promotedType, isChecked, isExplicit: false);
	}

	private BoundExpression MakeBinary(MethodSymbol methodOpt, TypeSymbol type, bool isLifted, bool requiresLifted, WellKnownMember opFactory, BoundExpression loweredLeft, BoundExpression loweredRight)
	{
		if ((object)methodOpt != null)
		{
			if (!requiresLifted)
			{
				return _bound.StaticCall(opFactory, loweredLeft, loweredRight, _bound.MethodInfo(methodOpt, _bound.WellKnownType(WellKnownType.System_Reflection_MethodInfo)));
			}
			return _bound.StaticCall(opFactory, loweredLeft, loweredRight, _bound.Literal(isLifted && !TypeSymbol.Equals(methodOpt.ReturnType, type, TypeCompareKind.ConsiderEverything)), _bound.MethodInfo(methodOpt, _bound.WellKnownType(WellKnownType.System_Reflection_MethodInfo)));
		}
		return _bound.StaticCall(opFactory, loweredLeft, loweredRight);
	}

	private TypeSymbol PromotedType(TypeSymbol underlying)
	{
		if (underlying.SpecialType == SpecialType.System_Boolean)
		{
			return underlying;
		}
		SpecialType enumPromotedType = Binder.GetEnumPromotedType(underlying.SpecialType);
		if (enumPromotedType == underlying.SpecialType)
		{
			return underlying;
		}
		return _bound.SpecialType(enumPromotedType);
	}

	private BoundExpression Demote(BoundExpression node, TypeSymbol type, bool isChecked)
	{
		if (type is NamedTypeSymbol namedTypeSymbol)
		{
			if (namedTypeSymbol.StrippedType().TypeKind == TypeKind.Enum)
			{
				return Convert(node, type, isChecked);
			}
			if (!TypeSymbol.Equals(namedTypeSymbol.IsNullableType() ? _nullableType.Construct(PromotedType(namedTypeSymbol.GetNullableUnderlyingType())) : PromotedType(namedTypeSymbol), type, TypeCompareKind.ConsiderEverything))
			{
				return Convert(node, type, isChecked);
			}
		}
		return node;
	}

	private BoundExpression ConvertIndex(BoundExpression expr, TypeSymbol oldType, TypeSymbol newType)
	{
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(Diagnostics, _bound.Compilation.Assembly);
		ConversionKind kind = _bound.Compilation.Conversions.ClassifyConversionFromType(oldType, newType, isChecked: false, ref useSiteInfo).Kind;
		Diagnostics.AddDependencies(useSiteInfo);
		return kind switch
		{
			ConversionKind.Identity => expr, 
			ConversionKind.ExplicitNumeric => Convert(expr, newType, isChecked: true), 
			_ => Convert(expr, _int32Type, isChecked: false), 
		};
	}

	private BoundExpression VisitCall(BoundCall node)
	{
		if (node.IsDelegateCall)
		{
			return _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__Invoke, Visit(node.ReceiverOpt), Expressions(node.Arguments));
		}
		MethodSymbol method = node.Method;
		return _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__Call, method.RequiresInstanceReceiver ? Visit(node.ReceiverOpt) : _bound.Null(ExpressionType), _bound.MethodInfo(method, _bound.WellKnownType(WellKnownType.System_Reflection_MethodInfo)), Expressions(node.Arguments));
	}

	private BoundExpression VisitConditionalOperator(BoundConditionalOperator node)
	{
		BoundExpression boundExpression = Visit(node.Condition);
		BoundExpression boundExpression2 = VisitExactType(node.Consequence);
		BoundExpression boundExpression3 = VisitExactType(node.Alternative);
		return _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__Condition, boundExpression, boundExpression2, boundExpression3);
	}

	private BoundExpression VisitExactType(BoundExpression e)
	{
		if (e is BoundConversion { ExplicitCastInCode: false } boundConversion)
		{
			e = boundConversion.Update(boundConversion.Operand, boundConversion.Conversion, boundConversion.IsBaseConversion, boundConversion.Checked, explicitCastInCode: true, conversionGroupOpt: boundConversion.ConversionGroupOpt, constantValueOpt: boundConversion.ConstantValueOpt, type: boundConversion.Type);
		}
		return Visit(e);
	}

	private BoundExpression VisitConversion(BoundConversion node)
	{
		switch (node.ConversionKind)
		{
		case ConversionKind.MethodGroup:
		{
			BoundMethodGroup boundMethodGroup = (BoundMethodGroup)node.Operand;
			return DelegateCreation(boundMethodGroup.ReceiverOpt, node.SymbolOpt, node.Type, !node.SymbolOpt.RequiresInstanceReceiver && !node.IsExtensionMethod);
		}
		case ConversionKind.ImplicitUserDefined:
		case ConversionKind.ExplicitUserDefined:
		case ConversionKind.IntPtr:
		{
			MethodSymbol symbolOpt = node.SymbolOpt;
			TypeSymbol? type2 = node.Operand.Type;
			TypeSymbol left = type2.StrippedType();
			TypeSymbol type3 = symbolOpt.Parameters[0].Type;
			bool num = !TypeSymbol.Equals(type2, type3, TypeCompareKind.ConsiderEverything) && TypeSymbol.Equals(left, type3, TypeCompareKind.ConsiderEverything);
			bool flag = !TypeSymbol.Equals(left, (node.ConversionKind == ConversionKind.ExplicitUserDefined) ? type3 : type3.StrippedType(), TypeCompareKind.ConsiderEverything);
			TypeSymbol typeSymbol = ((num && symbolOpt.ReturnType.IsNonNullableValueType() && node.Type.IsNullableType()) ? _nullableType.Construct(symbolOpt.ReturnType) : symbolOpt.ReturnType);
			BoundExpression boundExpression = (flag ? Convert(Visit(node.Operand), node.Operand.Type, symbolOpt.Parameters[0].Type, node.Checked, isExplicit: false) : Visit(node.Operand));
			BoundExpression operand2 = _bound.StaticCall((node.Checked && SyntaxFacts.IsCheckedOperator(symbolOpt.Name)) ? WellKnownMember.System_Linq_Expressions_Expression__ConvertChecked_MethodInfo : WellKnownMember.System_Linq_Expressions_Expression__Convert_MethodInfo, boundExpression, _bound.Typeof(typeSymbol, _bound.WellKnownType(WellKnownType.System_Type)), _bound.MethodInfo(symbolOpt, _bound.WellKnownType(WellKnownType.System_Reflection_MethodInfo)));
			return Convert(operand2, typeSymbol, node.Type, node.Checked, isExplicit: false);
		}
		case ConversionKind.Identity:
		case ConversionKind.ImplicitReference:
		{
			BoundExpression boundExpression2 = Visit(node.Operand);
			if (!node.ExplicitCastInCode)
			{
				return boundExpression2;
			}
			return Convert(boundExpression2, node.Type, isChecked: false);
		}
		case ConversionKind.ImplicitNullable:
		{
			if (node.Operand.Type.IsNullableType())
			{
				return Convert(Visit(node.Operand), node.Operand.Type, node.Type, node.Checked, node.ExplicitCastInCode);
			}
			TypeSymbol type = ((NamedTypeSymbol)node.Type).TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[0].Type;
			BoundExpression operand = Convert(Visit(node.Operand), node.Operand.Type, type, node.Checked, isExplicit: false);
			return Convert(operand, type, node.Type, node.Checked, isExplicit: false);
		}
		case ConversionKind.NullLiteral:
			return Convert(Constant(_bound.Null(_objectType)), _objectType, node.Type, isChecked: false, node.ExplicitCastInCode);
		default:
			return Convert(Visit(node.Operand), node.Operand.Type, node.Type, node.Checked, node.ExplicitCastInCode);
		}
	}

	private BoundExpression Convert(BoundExpression operand, TypeSymbol oldType, TypeSymbol newType, bool isChecked, bool isExplicit)
	{
		if (!TypeSymbol.Equals(oldType, newType, TypeCompareKind.ConsiderEverything) || isExplicit)
		{
			return Convert(operand, newType, isChecked);
		}
		return operand;
	}

	private BoundExpression Convert(BoundExpression expr, TypeSymbol type, bool isChecked)
	{
		return _bound.StaticCall(isChecked ? WellKnownMember.System_Linq_Expressions_Expression__ConvertChecked : WellKnownMember.System_Linq_Expressions_Expression__Convert, expr, _bound.Typeof(type, _bound.WellKnownType(WellKnownType.System_Type)));
	}

	private BoundExpression DelegateCreation(BoundExpression receiver, MethodSymbol method, TypeSymbol delegateType, bool requiresInstanceReceiver)
	{
		BoundExpression boundExpression = _bound.Null(_objectType);
		if (requiresInstanceReceiver)
		{
			receiver = boundExpression;
		}
		else if (!receiver.Type.IsReferenceType)
		{
			Conversion conversion = _bound.ClassifyEmitConversion(receiver, _objectType);
			receiver = _bound.Convert(_objectType, receiver, conversion);
		}
		MethodSymbol methodSymbol = _bound.WellKnownMethod(WellKnownMember.System_Reflection_MethodInfo__CreateDelegate, isOptional: true);
		BoundExpression node;
		if ((object)methodSymbol != null)
		{
			node = _bound.Call(_bound.MethodInfo(method, methodSymbol.ContainingType), methodSymbol, _bound.Typeof(delegateType, methodSymbol.Parameters[0].Type), receiver);
		}
		else
		{
			methodSymbol = _bound.SpecialMethod(SpecialMember.System_Delegate__CreateDelegate);
			node = _bound.Call(null, methodSymbol, _bound.Typeof(delegateType, methodSymbol.Parameters[0].Type), receiver, _bound.MethodInfo(method, methodSymbol.Parameters[2].Type));
		}
		return Convert(Visit(node), delegateType, isChecked: false);
	}

	private BoundExpression VisitDelegateCreationExpression(BoundDelegateCreationExpression node)
	{
		if (node.Argument.Kind == BoundKind.MethodGroup)
		{
			throw ExceptionUtilities.UnexpectedValue(BoundKind.MethodGroup);
		}
		if ((object)node.MethodOpt != null)
		{
			bool requiresInstanceReceiver = !node.MethodOpt.RequiresInstanceReceiver && !node.IsExtensionMethod;
			return DelegateCreation(node.Argument, node.MethodOpt, node.Type, requiresInstanceReceiver);
		}
		if (node.Argument.Type is NamedTypeSymbol { TypeKind: TypeKind.Delegate } namedTypeSymbol)
		{
			return DelegateCreation(node.Argument, namedTypeSymbol.DelegateInvokeMethod, node.Type, requiresInstanceReceiver: false);
		}
		throw ExceptionUtilities.UnexpectedValue(node.Argument);
	}

	private BoundExpression VisitFieldAccess(BoundFieldAccess node)
	{
		BoundExpression boundExpression = (node.FieldSymbol.IsStatic ? _bound.Null(ExpressionType) : Visit(node.ReceiverOpt));
		return _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__Field, boundExpression, _bound.FieldInfo(node.FieldSymbol));
	}

	private BoundExpression VisitIsOperator(BoundIsOperator node)
	{
		BoundExpression boundExpression = node.Operand;
		if ((object)boundExpression.Type == null && boundExpression.ConstantValueOpt != null && boundExpression.ConstantValueOpt.IsNull)
		{
			boundExpression = _bound.Null(_objectType);
		}
		return _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__TypeIs, Visit(boundExpression), _bound.Typeof(node.TargetType.Type, _bound.WellKnownType(WellKnownType.System_Type)));
	}

	private BoundExpression VisitLambda(BoundLambda node)
	{
		BoundExpression boundExpression = VisitLambdaInternal(node);
		if (!node.Type.IsExpressionTree())
		{
			return boundExpression;
		}
		return _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__Quote, boundExpression);
	}

	private BoundExpression VisitLambdaInternal(BoundLambda node)
	{
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance();
		ArrayBuilder<BoundExpression> instance3 = ArrayBuilder<BoundExpression>.GetInstance();
		foreach (ParameterSymbol parameter in node.Symbol.Parameters)
		{
			LocalSymbol localSymbol = _bound.SynthesizedLocal(ParameterExpressionType);
			instance.Add(localSymbol);
			BoundLocal boundLocal = _bound.Local(localSymbol);
			instance3.Add(boundLocal);
			BoundExpression right = _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__Parameter, _bound.Typeof(_typeMap.SubstituteType(parameter.Type).Type, _bound.WellKnownType(WellKnownType.System_Type)), _bound.Literal(parameter.Name));
			instance2.Add(_bound.AssignmentExpression(boundLocal, right));
			_parameterMap[parameter] = boundLocal;
		}
		NamedTypeSymbol delegateType = node.Type.GetDelegateType();
		BoundExpression result = _bound.Sequence(instance.ToImmutableAndFree(), instance2.ToImmutableAndFree(), _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__Lambda_OfTDelegate, ImmutableArray.Create((TypeSymbol)delegateType), TranslateLambdaBody(node.Body), _bound.ArrayOrEmpty(ParameterExpressionType, instance3.ToImmutableAndFree())));
		foreach (ParameterSymbol parameter2 in node.Symbol.Parameters)
		{
			_parameterMap.Remove(parameter2);
		}
		return result;
	}

	private BoundExpression VisitNewT(BoundNewT node)
	{
		return VisitObjectCreationContinued(_bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__New_Type, _bound.Typeof(node.Type, _bound.WellKnownType(WellKnownType.System_Type))), node.InitializerExpressionOpt);
	}

	private BoundExpression VisitNullCoalescingOperator(BoundNullCoalescingOperator node)
	{
		BoundExpression boundExpression = Visit(node.LeftOperand);
		BoundExpression boundExpression2 = Visit(node.RightOperand);
		if (BoundNode.GetConversion(node.LeftConversion, node.LeftPlaceholder).IsUserDefined)
		{
			return _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__Coalesce_Lambda, boundExpression, boundExpression2, makeConversionLambda(node.LeftConversion, node.LeftPlaceholder));
		}
		return _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__Coalesce, boundExpression, boundExpression2);
		BoundExpression makeConversionLambda(BoundExpression leftConversion, BoundValuePlaceholder leftPlaceholder)
		{
			string text = "p";
			TypeSymbol type = leftPlaceholder.Type;
			_bound.SynthesizedParameter(type, text);
			LocalSymbol localSymbol = _bound.SynthesizedLocal(ParameterExpressionType);
			BoundLocal boundLocal = _bound.Local(localSymbol);
			BoundExpression right = _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__Parameter, _bound.Typeof(type, _bound.WellKnownType(WellKnownType.System_Type)), _bound.Literal(text));
			if (_placeholderReplacementMap == null)
			{
				_placeholderReplacementMap = new Dictionary<BoundValuePlaceholder, BoundExpression>();
			}
			_placeholderReplacementMap.Add(leftPlaceholder, boundLocal);
			BoundExpression boundExpression3 = Visit(leftConversion);
			_placeholderReplacementMap.Remove(leftPlaceholder);
			return _bound.Sequence(ImmutableArray.Create(localSymbol), ImmutableArray.Create(_bound.AssignmentExpression(boundLocal, right)), _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__Lambda, boundExpression3, _bound.ArrayOrEmpty(ParameterExpressionType, ImmutableArray.Create((BoundExpression)boundLocal))));
		}
	}

	private BoundExpression InitializerMemberSetter(Symbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.Field:
		{
			BoundExpression arg2 = _bound.FieldInfo((FieldSymbol)symbol);
			Conversion conversion2 = _bound.ClassifyEmitConversion(arg2, MemberInfoType);
			return _bound.Convert(MemberInfoType, arg2, conversion2);
		}
		case SymbolKind.Property:
			return _bound.MethodInfo(((PropertySymbol)symbol).GetOwnOrInheritedSetMethod(), _bound.WellKnownType(WellKnownType.System_Reflection_MethodInfo));
		case SymbolKind.Event:
		{
			BoundExpression arg = _bound.FieldInfo(((EventSymbol)symbol).AssociatedField);
			Conversion conversion = _bound.ClassifyEmitConversion(arg, MemberInfoType);
			return _bound.Convert(MemberInfoType, arg, conversion);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
		}
	}

	private BoundExpression InitializerMemberGetter(Symbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.Field:
		{
			BoundExpression arg2 = _bound.FieldInfo((FieldSymbol)symbol);
			Conversion conversion2 = _bound.ClassifyEmitConversion(arg2, MemberInfoType);
			return _bound.Convert(MemberInfoType, arg2, conversion2);
		}
		case SymbolKind.Property:
			return _bound.MethodInfo(((PropertySymbol)symbol).GetOwnOrInheritedGetMethod(), _bound.WellKnownType(WellKnownType.System_Reflection_MethodInfo));
		case SymbolKind.Event:
		{
			BoundExpression arg = _bound.FieldInfo(((EventSymbol)symbol).AssociatedField);
			Conversion conversion = _bound.ClassifyEmitConversion(arg, MemberInfoType);
			return _bound.Convert(MemberInfoType, arg, conversion);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
		}
	}

	private BoundExpression VisitInitializer(BoundExpression node, out InitializerKind kind)
	{
		switch (node.Kind)
		{
		case BoundKind.ObjectInitializerExpression:
		{
			BoundObjectInitializerExpression obj2 = (BoundObjectInitializerExpression)node;
			ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance();
			foreach (BoundAssignmentOperator initializer in obj2.Initializers)
			{
				Symbol memberSymbol = ((BoundObjectInitializerMember)initializer.Left).MemberSymbol;
				BoundExpression boundExpression = VisitInitializer(initializer.Right, out var kind2);
				switch (kind2)
				{
				case InitializerKind.CollectionInitializer:
				{
					BoundExpression boundExpression4 = InitializerMemberGetter(memberSymbol);
					instance2.Add(_bound.StaticCall((memberSymbol.Kind == SymbolKind.Property) ? WellKnownMember.System_Linq_Expressions_Expression__ListBind_MethodInfo : WellKnownMember.System_Linq_Expressions_Expression__ListBind_MemberInfo, boundExpression4, boundExpression));
					break;
				}
				case InitializerKind.Expression:
				{
					BoundExpression boundExpression3 = InitializerMemberSetter(memberSymbol);
					instance2.Add(_bound.StaticCall((memberSymbol.Kind == SymbolKind.Property) ? WellKnownMember.System_Linq_Expressions_Expression__Bind_MethodInfo : WellKnownMember.System_Linq_Expressions_Expression__Bind_MemberInfo, boundExpression3, boundExpression));
					break;
				}
				case InitializerKind.MemberInitializer:
				{
					BoundExpression boundExpression2 = InitializerMemberGetter(memberSymbol);
					instance2.Add(_bound.StaticCall((memberSymbol.Kind == SymbolKind.Property) ? WellKnownMember.System_Linq_Expressions_Expression__MemberBind_MethodInfo : WellKnownMember.System_Linq_Expressions_Expression__MemberBind_MemberInfo, boundExpression2, boundExpression));
					break;
				}
				default:
					throw ExceptionUtilities.UnexpectedValue(kind2);
				}
			}
			kind = InitializerKind.MemberInitializer;
			return _bound.ArrayOrEmpty(MemberBindingType, instance2.ToImmutableAndFree());
		}
		case BoundKind.CollectionInitializerExpression:
		{
			BoundCollectionInitializerExpression obj = (BoundCollectionInitializerExpression)node;
			kind = InitializerKind.CollectionInitializer;
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
			foreach (BoundCollectionElementInitializer initializer2 in obj.Initializers)
			{
				BoundExpression item = _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__ElementInit, _bound.MethodInfo(initializer2.AddMethod, _bound.WellKnownType(WellKnownType.System_Reflection_MethodInfo)), Expressions(initializer2.Arguments));
				instance.Add(item);
			}
			return _bound.ArrayOrEmpty(ElementInitType, instance.ToImmutableAndFree());
		}
		default:
			kind = InitializerKind.Expression;
			return Visit(node);
		}
	}

	private BoundExpression VisitObjectCreationExpression(BoundObjectCreationExpression node)
	{
		return VisitObjectCreationContinued(VisitObjectCreationExpressionInternal(node), node.InitializerExpressionOpt);
	}

	private BoundExpression VisitObjectCreationContinued(BoundExpression creation, BoundExpression initializerExpressionOpt)
	{
		if (initializerExpressionOpt == null)
		{
			return creation;
		}
		BoundExpression boundExpression = VisitInitializer(initializerExpressionOpt, out var kind);
		return kind switch
		{
			InitializerKind.CollectionInitializer => _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__ListInit, creation, boundExpression), 
			InitializerKind.MemberInitializer => _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__MemberInit, creation, boundExpression), 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	private BoundExpression VisitObjectCreationExpressionInternal(BoundObjectCreationExpression node)
	{
		if (node.ConstantValueOpt != null)
		{
			return Constant(node);
		}
		if ((object)node.Constructor == null || (node.Arguments.Length == 0 && !node.Type.IsStructType()) || node.Constructor.IsDefaultValueTypeConstructor())
		{
			return _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__New_Type, _bound.Typeof(node.Type, _bound.WellKnownType(WellKnownType.System_Type)));
		}
		BoundExpression boundExpression = _bound.ConstructorInfo(node.Constructor);
		NamedTypeSymbol namedTypeSymbol = _IEnumerableType.Construct(ExpressionType);
		BoundExpression arg = Expressions(node.Arguments);
		Conversion conversion = _bound.ClassifyEmitConversion(arg, namedTypeSymbol);
		arg = _bound.Convert(namedTypeSymbol, arg, conversion);
		if (node.Type.IsAnonymousType && node.Arguments.Length != 0)
		{
			NamedTypeSymbol type = (NamedTypeSymbol)node.Type;
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
			for (int i = 0; i < node.Arguments.Length; i++)
			{
				instance.Add(_bound.MethodInfo(AnonymousTypeManager.GetAnonymousTypeProperty(type, i).GetMethod, _bound.WellKnownType(WellKnownType.System_Reflection_MethodInfo)));
			}
			return _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__New_ConstructorInfo_Expressions_MemberInfos, boundExpression, arg, _bound.ArrayOrEmpty(MemberInfoType, instance.ToImmutableAndFree()));
		}
		return _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__New_ConstructorInfo_IEnumerableExpressions, boundExpression, arg);
	}

	private BoundExpression VisitParameter(BoundParameter node)
	{
		return _parameterMap[node.ParameterSymbol];
	}

	private static BoundExpression VisitPointerIndirectionOperator(BoundPointerIndirectionOperator node)
	{
		return new BoundBadExpression(node.Syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundExpression)node), node.Type);
	}

	private static BoundExpression VisitPointerElementAccess(BoundPointerElementAccess node)
	{
		return new BoundBadExpression(node.Syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundExpression)node), node.Type);
	}

	private BoundExpression VisitPropertyAccess(BoundPropertyAccess node)
	{
		BoundExpression boundExpression = (node.PropertySymbol.IsStatic ? _bound.Null(ExpressionType) : Visit(node.ReceiverOpt));
		MethodSymbol ownOrInheritedGetMethod = node.PropertySymbol.GetOwnOrInheritedGetMethod();
		BoundExpression? receiverOpt = node.ReceiverOpt;
		if (receiverOpt != null && receiverOpt.Type.IsTypeParameter() && !node.ReceiverOpt.Type.IsReferenceType)
		{
			boundExpression = Convert(boundExpression, ownOrInheritedGetMethod.ReceiverType, isChecked: false);
		}
		return _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__Property, boundExpression, _bound.MethodInfo(ownOrInheritedGetMethod, _bound.WellKnownType(WellKnownType.System_Reflection_MethodInfo)));
	}

	private static BoundExpression VisitSizeOfOperator(BoundSizeOfOperator node)
	{
		return new BoundBadExpression(node.Syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundExpression)node), node.Type);
	}

	private BoundExpression VisitUnaryOperator(BoundUnaryOperator node)
	{
		BoundExpression operand = node.Operand;
		BoundExpression boundExpression = Visit(operand);
		UnaryOperatorKind operatorKind = node.OperatorKind;
		UnaryOperatorKind unaryOperatorKind = operatorKind & UnaryOperatorKind.OpMask;
		bool flag = (operatorKind & UnaryOperatorKind.Checked) != 0;
		WellKnownMember wellKnownMember;
		int num;
		switch (unaryOperatorKind)
		{
		case UnaryOperatorKind.UnaryPlus:
			if ((object)node.MethodOpt == null)
			{
				return boundExpression;
			}
			wellKnownMember = WellKnownMember.System_Linq_Expressions_Expression__UnaryPlus;
			break;
		case UnaryOperatorKind.UnaryMinus:
		{
			if (flag)
			{
				goto IL_0096;
			}
			MethodSymbol methodOpt = node.MethodOpt;
			if ((object)methodOpt != null)
			{
				string name = methodOpt.Name;
				if (name != null && SyntaxFacts.IsCheckedOperator(name))
				{
					goto IL_0096;
				}
			}
			num = 522;
			goto IL_009b;
		}
		case UnaryOperatorKind.LogicalNegation:
		case UnaryOperatorKind.BitwiseComplement:
			wellKnownMember = WellKnownMember.System_Linq_Expressions_Expression__Not_Expression_MethodInfo;
			break;
		default:
			{
				throw ExceptionUtilities.UnexpectedValue(unaryOperatorKind);
			}
			IL_0096:
			num = 524;
			goto IL_009b;
			IL_009b:
			wellKnownMember = (WellKnownMember)num;
			break;
		}
		if ((object)node.MethodOpt == null)
		{
			wellKnownMember = wellKnownMember switch
			{
				WellKnownMember.System_Linq_Expressions_Expression__NegateChecked_Expression_MethodInfo => WellKnownMember.System_Linq_Expressions_Expression__NegateChecked_Expression, 
				WellKnownMember.System_Linq_Expressions_Expression__Negate_Expression_MethodInfo => WellKnownMember.System_Linq_Expressions_Expression__Negate_Expression, 
				WellKnownMember.System_Linq_Expressions_Expression__Not_Expression_MethodInfo => WellKnownMember.System_Linq_Expressions_Expression__Not_Expression, 
				_ => throw ExceptionUtilities.UnexpectedValue(wellKnownMember), 
			};
		}
		if (node.OperatorKind.OperandTypes() == UnaryOperatorKind.Enum && (operatorKind & UnaryOperatorKind.Lifted) != UnaryOperatorKind.Error)
		{
			TypeSymbol typeSymbol = PromotedType(operand.Type.StrippedType().GetEnumUnderlyingType());
			typeSymbol = _nullableType.Construct(typeSymbol);
			boundExpression = Convert(boundExpression, operand.Type, typeSymbol, flag, isExplicit: false);
			BoundExpression node2 = _bound.StaticCall(wellKnownMember, boundExpression);
			return Demote(node2, node.Type, flag);
		}
		if ((object)node.MethodOpt != null)
		{
			return _bound.StaticCall(wellKnownMember, boundExpression, _bound.MethodInfo(node.MethodOpt, _bound.WellKnownType(WellKnownType.System_Reflection_MethodInfo)));
		}
		return _bound.StaticCall(wellKnownMember, boundExpression);
	}

	private BoundExpression Constant(BoundExpression node)
	{
		Conversion conversion = _bound.ClassifyEmitConversion(node, _objectType);
		return _bound.StaticCall(WellKnownMember.System_Linq_Expressions_Expression__Constant, _bound.Convert(_objectType, node, conversion), _bound.Typeof(node.Type, _bound.WellKnownType(WellKnownType.System_Type)));
	}
}
