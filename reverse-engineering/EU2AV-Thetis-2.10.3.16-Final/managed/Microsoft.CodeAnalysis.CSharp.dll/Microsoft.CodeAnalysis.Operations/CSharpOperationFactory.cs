using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class CSharpOperationFactory
{
	internal class Helper
	{
		internal static bool IsPostfixIncrementOrDecrement(Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind operatorKind)
		{
			Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind unaryOperatorKind = operatorKind.Operator();
			if (unaryOperatorKind == Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.PostfixIncrement || unaryOperatorKind == Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.PostfixDecrement)
			{
				return true;
			}
			return false;
		}

		internal static bool IsDecrement(Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind operatorKind)
		{
			Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind unaryOperatorKind = operatorKind.Operator();
			if (unaryOperatorKind == Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.PostfixDecrement || unaryOperatorKind == Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.PrefixDecrement)
			{
				return true;
			}
			return false;
		}

		internal static UnaryOperatorKind DeriveUnaryOperatorKind(Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind operatorKind)
		{
			return operatorKind.Operator() switch
			{
				Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.UnaryPlus => UnaryOperatorKind.Plus, 
				Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.UnaryMinus => UnaryOperatorKind.Minus, 
				Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.LogicalNegation => UnaryOperatorKind.Not, 
				Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.BitwiseComplement => UnaryOperatorKind.BitwiseNegation, 
				Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.True => UnaryOperatorKind.True, 
				Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.False => UnaryOperatorKind.False, 
				_ => UnaryOperatorKind.None, 
			};
		}

		internal static BinaryOperatorKind DeriveBinaryOperatorKind(Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind operatorKind)
		{
			return operatorKind.OperatorWithLogical() switch
			{
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.Addition => BinaryOperatorKind.Add, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.Subtraction => BinaryOperatorKind.Subtract, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.Multiplication => BinaryOperatorKind.Multiply, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.Division => BinaryOperatorKind.Divide, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.Remainder => BinaryOperatorKind.Remainder, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.LeftShift => BinaryOperatorKind.LeftShift, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.RightShift => BinaryOperatorKind.RightShift, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.UnsignedRightShift => BinaryOperatorKind.UnsignedRightShift, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.And => BinaryOperatorKind.And, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.Or => BinaryOperatorKind.Or, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.Xor => BinaryOperatorKind.ExclusiveOr, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.LessThan => BinaryOperatorKind.LessThan, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.LessThanOrEqual => BinaryOperatorKind.LessThanOrEqual, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.Equal => BinaryOperatorKind.Equals, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.NotEqual => BinaryOperatorKind.NotEquals, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.GreaterThanOrEqual => BinaryOperatorKind.GreaterThanOrEqual, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.GreaterThan => BinaryOperatorKind.GreaterThan, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.LogicalAnd => BinaryOperatorKind.ConditionalAnd, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.LogicalOr => BinaryOperatorKind.ConditionalOr, 
				_ => BinaryOperatorKind.None, 
			};
		}
	}

	private readonly SemanticModel _semanticModel;

	public CSharpOperationFactory(SemanticModel semanticModel)
	{
		_semanticModel = semanticModel;
	}

	[return: NotNullIfNotNull("boundNode")]
	public IOperation? Create(BoundNode? boundNode)
	{
		if (boundNode == null)
		{
			return null;
		}
		switch (boundNode.Kind)
		{
		case BoundKind.DeconstructValuePlaceholder:
			return CreateBoundDeconstructValuePlaceholderOperation((BoundDeconstructValuePlaceholder)boundNode);
		case BoundKind.DeconstructionAssignmentOperator:
			return CreateBoundDeconstructionAssignmentOperator((BoundDeconstructionAssignmentOperator)boundNode);
		case BoundKind.Call:
			return CreateBoundCallOperation((BoundCall)boundNode);
		case BoundKind.Local:
			return CreateBoundLocalOperation((BoundLocal)boundNode);
		case BoundKind.FieldAccess:
			return CreateBoundFieldAccessOperation((BoundFieldAccess)boundNode);
		case BoundKind.PropertyAccess:
			return CreateBoundPropertyAccessOperation((BoundPropertyAccess)boundNode);
		case BoundKind.IndexerAccess:
			return CreateBoundIndexerAccessOperation((BoundIndexerAccess)boundNode);
		case BoundKind.EventAccess:
			return CreateBoundEventAccessOperation((BoundEventAccess)boundNode);
		case BoundKind.EventAssignmentOperator:
			return CreateBoundEventAssignmentOperatorOperation((BoundEventAssignmentOperator)boundNode);
		case BoundKind.Parameter:
			return CreateBoundParameterOperation((BoundParameter)boundNode);
		case BoundKind.Literal:
			return CreateBoundLiteralOperation((BoundLiteral)boundNode);
		case BoundKind.Utf8String:
			return CreateBoundUtf8StringOperation((BoundUtf8String)boundNode);
		case BoundKind.DynamicInvocation:
			return CreateBoundDynamicInvocationExpressionOperation((BoundDynamicInvocation)boundNode);
		case BoundKind.DynamicIndexerAccess:
			return CreateBoundDynamicIndexerAccessExpressionOperation((BoundDynamicIndexerAccess)boundNode);
		case BoundKind.ObjectCreationExpression:
			return CreateBoundObjectCreationExpressionOperation((BoundObjectCreationExpression)boundNode);
		case BoundKind.WithExpression:
			return CreateBoundWithExpressionOperation((BoundWithExpression)boundNode);
		case BoundKind.DynamicObjectCreationExpression:
			return CreateBoundDynamicObjectCreationExpressionOperation((BoundDynamicObjectCreationExpression)boundNode);
		case BoundKind.ObjectInitializerExpression:
			return CreateBoundObjectInitializerExpressionOperation((BoundObjectInitializerExpression)boundNode);
		case BoundKind.CollectionInitializerExpression:
			return CreateBoundCollectionInitializerExpressionOperation((BoundCollectionInitializerExpression)boundNode);
		case BoundKind.ObjectInitializerMember:
			return CreateBoundObjectInitializerMemberOperation((BoundObjectInitializerMember)boundNode);
		case BoundKind.CollectionElementInitializer:
			return CreateBoundCollectionElementInitializerOperation((BoundCollectionElementInitializer)boundNode);
		case BoundKind.DynamicObjectInitializerMember:
			return CreateBoundDynamicObjectInitializerMemberOperation((BoundDynamicObjectInitializerMember)boundNode);
		case BoundKind.DynamicMemberAccess:
			return CreateBoundDynamicMemberAccessOperation((BoundDynamicMemberAccess)boundNode);
		case BoundKind.DynamicCollectionElementInitializer:
			return CreateBoundDynamicCollectionElementInitializerOperation((BoundDynamicCollectionElementInitializer)boundNode);
		case BoundKind.UnboundLambda:
			return CreateUnboundLambdaOperation((UnboundLambda)boundNode);
		case BoundKind.Lambda:
			return CreateBoundLambdaOperation((BoundLambda)boundNode);
		case BoundKind.Conversion:
			return CreateBoundConversionOperation((BoundConversion)boundNode);
		case BoundKind.AsOperator:
			return CreateBoundAsOperatorOperation((BoundAsOperator)boundNode);
		case BoundKind.IsOperator:
			return CreateBoundIsOperatorOperation((BoundIsOperator)boundNode);
		case BoundKind.SizeOfOperator:
			return CreateBoundSizeOfOperatorOperation((BoundSizeOfOperator)boundNode);
		case BoundKind.TypeOfOperator:
			return CreateBoundTypeOfOperatorOperation((BoundTypeOfOperator)boundNode);
		case BoundKind.ArrayCreation:
			return CreateBoundArrayCreationOperation((BoundArrayCreation)boundNode);
		case BoundKind.ArrayInitialization:
			return CreateBoundArrayInitializationOperation((BoundArrayInitialization)boundNode);
		case BoundKind.CollectionExpression:
			return CreateBoundCollectionExpression((BoundCollectionExpression)boundNode);
		case BoundKind.DefaultLiteral:
			return CreateBoundDefaultLiteralOperation((BoundDefaultLiteral)boundNode);
		case BoundKind.DefaultExpression:
			return CreateBoundDefaultExpressionOperation((BoundDefaultExpression)boundNode);
		case BoundKind.BaseReference:
			return CreateBoundBaseReferenceOperation((BoundBaseReference)boundNode);
		case BoundKind.ThisReference:
			return CreateBoundThisReferenceOperation((BoundThisReference)boundNode);
		case BoundKind.AssignmentOperator:
			return CreateBoundAssignmentOperatorOrMemberInitializerOperation((BoundAssignmentOperator)boundNode);
		case BoundKind.CompoundAssignmentOperator:
			return CreateBoundCompoundAssignmentOperatorOperation((BoundCompoundAssignmentOperator)boundNode);
		case BoundKind.IncrementOperator:
			return CreateBoundIncrementOperatorOperation((BoundIncrementOperator)boundNode);
		case BoundKind.BadExpression:
			return CreateBoundBadExpressionOperation((BoundBadExpression)boundNode);
		case BoundKind.NewT:
			return CreateBoundNewTOperation((BoundNewT)boundNode);
		case BoundKind.NoPiaObjectCreationExpression:
			return CreateNoPiaObjectCreationExpressionOperation((BoundNoPiaObjectCreationExpression)boundNode);
		case BoundKind.UnaryOperator:
			return CreateBoundUnaryOperatorOperation((BoundUnaryOperator)boundNode);
		case BoundKind.BinaryOperator:
		case BoundKind.UserDefinedConditionalLogicalOperator:
			return CreateBoundBinaryOperatorBase((BoundBinaryOperatorBase)boundNode);
		case BoundKind.TupleBinaryOperator:
			return CreateBoundTupleBinaryOperatorOperation((BoundTupleBinaryOperator)boundNode);
		case BoundKind.ConditionalOperator:
			return CreateBoundConditionalOperatorOperation((BoundConditionalOperator)boundNode);
		case BoundKind.NullCoalescingOperator:
			return CreateBoundNullCoalescingOperatorOperation((BoundNullCoalescingOperator)boundNode);
		case BoundKind.AwaitExpression:
			return CreateBoundAwaitExpressionOperation((BoundAwaitExpression)boundNode);
		case BoundKind.ArrayAccess:
			return CreateBoundArrayAccessOperation((BoundArrayAccess)boundNode);
		case BoundKind.ImplicitIndexerAccess:
			return CreateBoundImplicitIndexerAccessOperation((BoundImplicitIndexerAccess)boundNode);
		case BoundKind.InlineArrayAccess:
			return CreateBoundInlineArrayAccessOperation((BoundInlineArrayAccess)boundNode);
		case BoundKind.NameOfOperator:
			return CreateBoundNameOfOperatorOperation((BoundNameOfOperator)boundNode);
		case BoundKind.ThrowExpression:
			return CreateBoundThrowExpressionOperation((BoundThrowExpression)boundNode);
		case BoundKind.AddressOfOperator:
			return CreateBoundAddressOfOperatorOperation((BoundAddressOfOperator)boundNode);
		case BoundKind.ImplicitReceiver:
			return CreateBoundImplicitReceiverOperation((BoundImplicitReceiver)boundNode);
		case BoundKind.ConditionalAccess:
			return CreateBoundConditionalAccessOperation((BoundConditionalAccess)boundNode);
		case BoundKind.ConditionalReceiver:
			return CreateBoundConditionalReceiverOperation((BoundConditionalReceiver)boundNode);
		case BoundKind.FieldEqualsValue:
			return CreateBoundFieldEqualsValueOperation((BoundFieldEqualsValue)boundNode);
		case BoundKind.PropertyEqualsValue:
			return CreateBoundPropertyEqualsValueOperation((BoundPropertyEqualsValue)boundNode);
		case BoundKind.ParameterEqualsValue:
			return CreateBoundParameterEqualsValueOperation((BoundParameterEqualsValue)boundNode);
		case BoundKind.Block:
			return CreateBoundBlockOperation((BoundBlock)boundNode);
		case BoundKind.ContinueStatement:
			return CreateBoundContinueStatementOperation((BoundContinueStatement)boundNode);
		case BoundKind.BreakStatement:
			return CreateBoundBreakStatementOperation((BoundBreakStatement)boundNode);
		case BoundKind.YieldBreakStatement:
			return CreateBoundYieldBreakStatementOperation((BoundYieldBreakStatement)boundNode);
		case BoundKind.GotoStatement:
			return CreateBoundGotoStatementOperation((BoundGotoStatement)boundNode);
		case BoundKind.NoOpStatement:
			return CreateBoundNoOpStatementOperation((BoundNoOpStatement)boundNode);
		case BoundKind.IfStatement:
			return CreateBoundIfStatementOperation((BoundIfStatement)boundNode);
		case BoundKind.WhileStatement:
			return CreateBoundWhileStatementOperation((BoundWhileStatement)boundNode);
		case BoundKind.DoStatement:
			return CreateBoundDoStatementOperation((BoundDoStatement)boundNode);
		case BoundKind.ForStatement:
			return CreateBoundForStatementOperation((BoundForStatement)boundNode);
		case BoundKind.ForEachStatement:
			return CreateBoundForEachStatementOperation((BoundForEachStatement)boundNode);
		case BoundKind.TryStatement:
			return CreateBoundTryStatementOperation((BoundTryStatement)boundNode);
		case BoundKind.CatchBlock:
			return CreateBoundCatchBlockOperation((BoundCatchBlock)boundNode);
		case BoundKind.FixedStatement:
			return CreateBoundFixedStatementOperation((BoundFixedStatement)boundNode);
		case BoundKind.UsingStatement:
			return CreateBoundUsingStatementOperation((BoundUsingStatement)boundNode);
		case BoundKind.ThrowStatement:
			return CreateBoundThrowStatementOperation((BoundThrowStatement)boundNode);
		case BoundKind.ReturnStatement:
			return CreateBoundReturnStatementOperation((BoundReturnStatement)boundNode);
		case BoundKind.YieldReturnStatement:
			return CreateBoundYieldReturnStatementOperation((BoundYieldReturnStatement)boundNode);
		case BoundKind.LockStatement:
			return CreateBoundLockStatementOperation((BoundLockStatement)boundNode);
		case BoundKind.BadStatement:
			return CreateBoundBadStatementOperation((BoundBadStatement)boundNode);
		case BoundKind.LocalDeclaration:
			return CreateBoundLocalDeclarationOperation((BoundLocalDeclaration)boundNode);
		case BoundKind.MultipleLocalDeclarations:
		case BoundKind.UsingLocalDeclarations:
			return CreateBoundMultipleLocalDeclarationsBaseOperation((BoundMultipleLocalDeclarationsBase)boundNode);
		case BoundKind.LabelStatement:
			return CreateBoundLabelStatementOperation((BoundLabelStatement)boundNode);
		case BoundKind.LabeledStatement:
			return CreateBoundLabeledStatementOperation((BoundLabeledStatement)boundNode);
		case BoundKind.ExpressionStatement:
			return CreateBoundExpressionStatementOperation((BoundExpressionStatement)boundNode);
		case BoundKind.TupleLiteral:
		case BoundKind.ConvertedTupleLiteral:
			return CreateBoundTupleOperation((BoundTupleExpression)boundNode);
		case BoundKind.InterpolatedString:
			return CreateBoundInterpolatedStringExpressionOperation((BoundInterpolatedString)boundNode);
		case BoundKind.StringInsert:
			return CreateBoundInterpolationOperation((BoundStringInsert)boundNode);
		case BoundKind.LocalFunctionStatement:
			return CreateBoundLocalFunctionStatementOperation((BoundLocalFunctionStatement)boundNode);
		case BoundKind.AnonymousObjectCreationExpression:
			return CreateBoundAnonymousObjectCreationExpressionOperation((BoundAnonymousObjectCreationExpression)boundNode);
		case BoundKind.ConstantPattern:
			return CreateBoundConstantPatternOperation((BoundConstantPattern)boundNode);
		case BoundKind.DeclarationPattern:
			return CreateBoundDeclarationPatternOperation((BoundDeclarationPattern)boundNode);
		case BoundKind.RecursivePattern:
			return CreateBoundRecursivePatternOperation((BoundRecursivePattern)boundNode);
		case BoundKind.ITuplePattern:
			return CreateBoundRecursivePatternOperation((BoundITuplePattern)boundNode);
		case BoundKind.DiscardPattern:
			return CreateBoundDiscardPatternOperation((BoundDiscardPattern)boundNode);
		case BoundKind.BinaryPattern:
			return CreateBoundBinaryPatternOperation((BoundBinaryPattern)boundNode);
		case BoundKind.NegatedPattern:
			return CreateBoundNegatedPatternOperation((BoundNegatedPattern)boundNode);
		case BoundKind.RelationalPattern:
			return CreateBoundRelationalPatternOperation((BoundRelationalPattern)boundNode);
		case BoundKind.TypePattern:
			return CreateBoundTypePatternOperation((BoundTypePattern)boundNode);
		case BoundKind.SlicePattern:
			return CreateBoundSlicePatternOperation((BoundSlicePattern)boundNode);
		case BoundKind.ListPattern:
			return CreateBoundListPatternOperation((BoundListPattern)boundNode);
		case BoundKind.SwitchStatement:
			return CreateBoundSwitchStatementOperation((BoundSwitchStatement)boundNode);
		case BoundKind.SwitchLabel:
			return CreateBoundSwitchLabelOperation((BoundSwitchLabel)boundNode);
		case BoundKind.IsPatternExpression:
			return CreateBoundIsPatternExpressionOperation((BoundIsPatternExpression)boundNode);
		case BoundKind.QueryClause:
			return CreateBoundQueryClauseOperation((BoundQueryClause)boundNode);
		case BoundKind.DelegateCreationExpression:
			return CreateBoundDelegateCreationExpressionOperation((BoundDelegateCreationExpression)boundNode);
		case BoundKind.RangeVariable:
			return CreateBoundRangeVariableOperation((BoundRangeVariable)boundNode);
		case BoundKind.ConstructorMethodBody:
			return CreateConstructorBodyOperation((BoundConstructorMethodBody)boundNode);
		case BoundKind.NonConstructorMethodBody:
			return CreateMethodBodyOperation((BoundNonConstructorMethodBody)boundNode);
		case BoundKind.DiscardExpression:
			return CreateBoundDiscardExpressionOperation((BoundDiscardExpression)boundNode);
		case BoundKind.NullCoalescingAssignmentOperator:
			return CreateBoundNullCoalescingAssignmentOperatorOperation((BoundNullCoalescingAssignmentOperator)boundNode);
		case BoundKind.FromEndIndexExpression:
			return CreateFromEndIndexExpressionOperation((BoundFromEndIndexExpression)boundNode);
		case BoundKind.RangeExpression:
			return CreateRangeExpressionOperation((BoundRangeExpression)boundNode);
		case BoundKind.SwitchSection:
			return CreateBoundSwitchSectionOperation((BoundSwitchSection)boundNode);
		case BoundKind.ConvertedSwitchExpression:
			return CreateBoundSwitchExpressionOperation((BoundConvertedSwitchExpression)boundNode);
		case BoundKind.SwitchExpressionArm:
			return CreateBoundSwitchExpressionArmOperation((BoundSwitchExpressionArm)boundNode);
		case BoundKind.ObjectOrCollectionValuePlaceholder:
			return CreateCollectionValuePlaceholderOperation((BoundObjectOrCollectionValuePlaceholder)boundNode);
		case BoundKind.FunctionPointerInvocation:
			return CreateBoundFunctionPointerInvocationOperation((BoundFunctionPointerInvocation)boundNode);
		case BoundKind.UnconvertedAddressOfOperator:
			return CreateBoundUnconvertedAddressOfOperatorOperation((BoundUnconvertedAddressOfOperator)boundNode);
		case BoundKind.InterpolatedStringArgumentPlaceholder:
			return CreateBoundInterpolatedStringArgumentPlaceholder((BoundInterpolatedStringArgumentPlaceholder)boundNode);
		case BoundKind.InterpolatedStringHandlerPlaceholder:
			return CreateBoundInterpolatedStringHandlerPlaceholder((BoundInterpolatedStringHandlerPlaceholder)boundNode);
		case BoundKind.Attribute:
			return CreateBoundAttributeOperation((BoundAttribute)boundNode);
		case BoundKind.GlobalStatementInitializer:
		case BoundKind.TypeExpression:
		case BoundKind.TypeOrValueExpression:
		case BoundKind.NamespaceExpression:
		case BoundKind.PointerIndirectionOperator:
		case BoundKind.PointerElementAccess:
		case BoundKind.RefTypeOperator:
		case BoundKind.MakeRefOperator:
		case BoundKind.RefValueOperator:
		case BoundKind.ArgList:
		case BoundKind.ArgListOperator:
		case BoundKind.FixedLocalCollectionInitializer:
		case BoundKind.PreviousSubmissionReference:
		case BoundKind.HostObjectMemberReference:
		case BoundKind.Sequence:
		case BoundKind.MethodGroup:
		case BoundKind.StackAllocArrayCreation:
		case BoundKind.ConvertedStackAllocExpression:
		{
			ConstantValue constantValue = (boundNode as BoundExpression)?.ConstantValueOpt;
			bool flag = boundNode.WasCompilerGenerated;
			if (!flag && boundNode.Kind == BoundKind.FixedLocalCollectionInitializer)
			{
				flag = true;
			}
			ImmutableArray<IOperation> iOperationChildren = GetIOperationChildren(boundNode);
			ITypeSymbol typeSymbol = ((!(boundNode is BoundExpression boundExpression)) ? null : boundExpression.GetPublicTypeSymbol());
			ITypeSymbol type = typeSymbol;
			return new NoneOperation(iOperationChildren, _semanticModel, boundNode.Syntax, type, constantValue, flag);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(boundNode.Kind);
		}
	}

	public ImmutableArray<TOperation> CreateFromArray<TBoundNode, TOperation>(ImmutableArray<TBoundNode> boundNodes) where TBoundNode : BoundNode where TOperation : class, IOperation
	{
		if (boundNodes.IsDefault)
		{
			return ImmutableArray<TOperation>.Empty;
		}
		ArrayBuilder<TOperation> instance = ArrayBuilder<TOperation>.GetInstance(boundNodes.Length);
		foreach (TBoundNode item in boundNodes)
		{
			instance.AddIfNotNull((TOperation)Create(item));
		}
		return instance.ToImmutableAndFree();
	}

	private IMethodBodyOperation CreateMethodBodyOperation(BoundNonConstructorMethodBody boundNode)
	{
		return new MethodBodyOperation((IBlockOperation)Create(boundNode.BlockBody), (IBlockOperation)Create(boundNode.ExpressionBody), _semanticModel, boundNode.Syntax, boundNode.WasCompilerGenerated);
	}

	private IConstructorBodyOperation CreateConstructorBodyOperation(BoundConstructorMethodBody boundNode)
	{
		return new ConstructorBodyOperation(boundNode.Locals.GetPublicSymbols(), Create(boundNode.Initializer), (IBlockOperation)Create(boundNode.BlockBody), (IBlockOperation)Create(boundNode.ExpressionBody), _semanticModel, boundNode.Syntax, boundNode.WasCompilerGenerated);
	}

	internal ImmutableArray<IOperation> GetIOperationChildren(IBoundNodeWithIOperationChildren boundNodeWithChildren)
	{
		ImmutableArray<BoundNode> children = boundNodeWithChildren.Children;
		if (children.IsDefaultOrEmpty)
		{
			return ImmutableArray<IOperation>.Empty;
		}
		ArrayBuilder<IOperation> instance = ArrayBuilder<IOperation>.GetInstance(children.Length);
		foreach (BoundNode item2 in children)
		{
			if (item2 != null)
			{
				IOperation item = Create(item2);
				instance.Add(item);
			}
		}
		return instance.ToImmutableAndFree();
	}

	internal ImmutableArray<IVariableDeclaratorOperation> CreateVariableDeclarator(BoundNode declaration, SyntaxNode declarationSyntax)
	{
		switch (declaration.Kind)
		{
		case BoundKind.LocalDeclaration:
			return ImmutableArray.Create(CreateVariableDeclaratorInternal((BoundLocalDeclaration)declaration, (declarationSyntax as VariableDeclarationSyntax)?.Variables[0] ?? declarationSyntax));
		case BoundKind.MultipleLocalDeclarations:
		case BoundKind.UsingLocalDeclarations:
		{
			BoundMultipleLocalDeclarationsBase obj = (BoundMultipleLocalDeclarationsBase)declaration;
			ArrayBuilder<IVariableDeclaratorOperation> instance = ArrayBuilder<IVariableDeclaratorOperation>.GetInstance(obj.LocalDeclarations.Length);
			foreach (BoundLocalDeclaration localDeclaration in obj.LocalDeclarations)
			{
				instance.Add(CreateVariableDeclaratorInternal(localDeclaration, localDeclaration.Syntax));
			}
			return instance.ToImmutableAndFree();
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(declaration.Kind);
		}
	}

	private IPlaceholderOperation CreateBoundDeconstructValuePlaceholderOperation(BoundDeconstructValuePlaceholder boundDeconstructValuePlaceholder)
	{
		SyntaxNode syntax = boundDeconstructValuePlaceholder.Syntax;
		ITypeSymbol publicTypeSymbol = boundDeconstructValuePlaceholder.GetPublicTypeSymbol();
		bool wasCompilerGenerated = boundDeconstructValuePlaceholder.WasCompilerGenerated;
		return new PlaceholderOperation(PlaceholderKind.Unspecified, _semanticModel, syntax, publicTypeSymbol, wasCompilerGenerated);
	}

	private IDeconstructionAssignmentOperation CreateBoundDeconstructionAssignmentOperator(BoundDeconstructionAssignmentOperator boundDeconstructionAssignmentOperator)
	{
		return new DeconstructionAssignmentOperation(Create(boundDeconstructionAssignmentOperator.Left), Create(boundDeconstructionAssignmentOperator.Right.Operand), syntax: boundDeconstructionAssignmentOperator.Syntax, type: boundDeconstructionAssignmentOperator.GetPublicTypeSymbol(), isImplicit: boundDeconstructionAssignmentOperator.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IOperation CreateBoundCallOperation(BoundCall boundCall)
	{
		MethodSymbol method = boundCall.Method;
		SyntaxNode syntax = boundCall.Syntax;
		ITypeSymbol publicTypeSymbol = boundCall.GetPublicTypeSymbol();
		ConstantValue constantValueOpt = boundCall.ConstantValueOpt;
		bool wasCompilerGenerated = boundCall.WasCompilerGenerated;
		if (boundCall.IsErroneousNode)
		{
			return new InvalidOperation(CreateFromArray<BoundNode, IOperation>(((IBoundInvalidNode)boundCall).InvalidNodeChildren), _semanticModel, syntax, publicTypeSymbol, constantValueOpt, wasCompilerGenerated);
		}
		TypeParameterSymbol constrainedToType = GetConstrainedToType(method, boundCall.ReceiverOpt);
		bool isVirtual = (object)constrainedToType != null || IsCallVirtual(method, boundCall.ReceiverOpt);
		IOperation instance = CreateReceiverOperation(boundCall.ReceiverOpt, method);
		ImmutableArray<IArgumentOperation> arguments = DeriveArguments(boundCall);
		return new InvocationOperation(method.GetPublicSymbol(), constrainedToType.GetPublicSymbol(), instance, isVirtual, arguments, _semanticModel, syntax, publicTypeSymbol, wasCompilerGenerated);
	}

	private static TypeParameterSymbol? GetConstrainedToType(Symbol targetMember, BoundExpression? receiverOpt)
	{
		if (targetMember.IsStatic && (targetMember.IsAbstract || targetMember.IsVirtual) && receiverOpt is BoundTypeExpression { Type: TypeParameterSymbol type })
		{
			return type;
		}
		return null;
	}

	private IOperation CreateBoundFunctionPointerInvocationOperation(BoundFunctionPointerInvocation boundFunctionPointerInvocation)
	{
		ITypeSymbol publicTypeSymbol = boundFunctionPointerInvocation.GetPublicTypeSymbol();
		SyntaxNode syntax = boundFunctionPointerInvocation.Syntax;
		bool wasCompilerGenerated = boundFunctionPointerInvocation.WasCompilerGenerated;
		if (boundFunctionPointerInvocation.ResultKind != LookupResultKind.Viable)
		{
			return new InvalidOperation(CreateFromArray<BoundNode, IOperation>(((IBoundInvalidNode)boundFunctionPointerInvocation).InvalidNodeChildren), _semanticModel, syntax, publicTypeSymbol, null, wasCompilerGenerated);
		}
		IOperation? target = Create(boundFunctionPointerInvocation.InvokedExpression);
		ImmutableArray<IArgumentOperation> arguments = DeriveArguments(boundFunctionPointerInvocation);
		return new FunctionPointerInvocationOperation(target, arguments, _semanticModel, syntax, publicTypeSymbol, wasCompilerGenerated);
	}

	private IOperation CreateBoundUnconvertedAddressOfOperatorOperation(BoundUnconvertedAddressOfOperator boundUnconvertedAddressOf)
	{
		return new AddressOfOperation(Create(boundUnconvertedAddressOf.Operand), _semanticModel, boundUnconvertedAddressOf.Syntax, boundUnconvertedAddressOf.GetPublicTypeSymbol(), boundUnconvertedAddressOf.WasCompilerGenerated);
	}

	private IOperation CreateBoundAttributeOperation(BoundAttribute boundAttribute)
	{
		bool wasCompilerGenerated = boundAttribute.WasCompilerGenerated;
		if ((object)boundAttribute.Constructor == null)
		{
			return new AttributeOperation(OperationFactory.CreateInvalidOperation(_semanticModel, boundAttribute.Syntax, GetIOperationChildren(boundAttribute), isImplicit: true), _semanticModel, boundAttribute.Syntax, wasCompilerGenerated);
		}
		ObjectOrCollectionInitializerOperation initializer = null;
		if (!boundAttribute.NamedArguments.IsEmpty)
		{
			initializer = new ObjectOrCollectionInitializerOperation(CreateFromArray<BoundAssignmentOperator, IOperation>(boundAttribute.NamedArguments), _semanticModel, boundAttribute.Syntax, boundAttribute.GetPublicTypeSymbol(), isImplicit: true);
		}
		return new AttributeOperation(new ObjectCreationOperation(boundAttribute.Constructor.GetPublicSymbol(), initializer, DeriveArguments(boundAttribute), _semanticModel, boundAttribute.Syntax, boundAttribute.GetPublicTypeSymbol(), boundAttribute.ConstantValueOpt, isImplicit: true), _semanticModel, boundAttribute.Syntax, wasCompilerGenerated);
	}

	internal ImmutableArray<IOperation> CreateIgnoredDimensions(BoundNode declaration)
	{
		switch (declaration.Kind)
		{
		case BoundKind.LocalDeclaration:
		{
			BoundTypeExpression declaredTypeOpt = ((BoundLocalDeclaration)declaration).DeclaredTypeOpt;
			return CreateFromArray<BoundExpression, IOperation>(declaredTypeOpt.BoundDimensionsOpt);
		}
		case BoundKind.MultipleLocalDeclarations:
		case BoundKind.UsingLocalDeclarations:
		{
			ImmutableArray<BoundLocalDeclaration> localDeclarations = ((BoundMultipleLocalDeclarationsBase)declaration).LocalDeclarations;
			ImmutableArray<BoundExpression> boundNodes = ((localDeclarations.Length <= 0) ? ImmutableArray<BoundExpression>.Empty : localDeclarations[0].DeclaredTypeOpt.BoundDimensionsOpt);
			return CreateFromArray<BoundExpression, IOperation>(boundNodes);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(declaration.Kind);
		}
	}

	internal IOperation CreateBoundLocalOperation(BoundLocal boundLocal, bool createDeclaration = true)
	{
		ILocalSymbol publicSymbol = boundLocal.LocalSymbol.GetPublicSymbol();
		bool flag = boundLocal.DeclarationKind != BoundLocalDeclarationKind.None;
		SyntaxNode syntaxNode = boundLocal.Syntax;
		ITypeSymbol publicTypeSymbol = boundLocal.GetPublicTypeSymbol();
		ConstantValue constantValueOpt = boundLocal.ConstantValueOpt;
		bool wasCompilerGenerated = boundLocal.WasCompilerGenerated;
		if (flag && syntaxNode is DeclarationExpressionSyntax declarationExpressionSyntax)
		{
			syntaxNode = declarationExpressionSyntax.Designation;
			if (createDeclaration)
			{
				return new DeclarationExpressionOperation(CreateBoundLocalOperation(boundLocal, createDeclaration: false), _semanticModel, declarationExpressionSyntax, publicTypeSymbol, isImplicit: false);
			}
		}
		return new LocalReferenceOperation(publicSymbol, flag, _semanticModel, syntaxNode, publicTypeSymbol, constantValueOpt, wasCompilerGenerated);
	}

	internal IOperation CreateBoundFieldAccessOperation(BoundFieldAccess boundFieldAccess, bool createDeclaration = true)
	{
		IFieldSymbol publicSymbol = boundFieldAccess.FieldSymbol.GetPublicSymbol();
		bool isDeclaration = boundFieldAccess.IsDeclaration;
		SyntaxNode syntaxNode = boundFieldAccess.Syntax;
		ITypeSymbol publicTypeSymbol = boundFieldAccess.GetPublicTypeSymbol();
		ConstantValue constantValueOpt = boundFieldAccess.ConstantValueOpt;
		bool wasCompilerGenerated = boundFieldAccess.WasCompilerGenerated;
		if (isDeclaration && syntaxNode is DeclarationExpressionSyntax declarationExpressionSyntax)
		{
			syntaxNode = declarationExpressionSyntax.Designation;
			if (createDeclaration)
			{
				return new DeclarationExpressionOperation(CreateBoundFieldAccessOperation(boundFieldAccess, createDeclaration: false), _semanticModel, declarationExpressionSyntax, publicTypeSymbol, isImplicit: false);
			}
		}
		IOperation instance = CreateReceiverOperation(boundFieldAccess.ReceiverOpt, boundFieldAccess.FieldSymbol);
		return new FieldReferenceOperation(publicSymbol, isDeclaration, instance, _semanticModel, syntaxNode, publicTypeSymbol, constantValueOpt, wasCompilerGenerated);
	}

	internal IOperation? CreateBoundPropertyReferenceInstance(BoundNode boundNode)
	{
		if (!(boundNode is BoundPropertyAccess boundPropertyAccess))
		{
			if (!(boundNode is BoundObjectInitializerMember boundObjectInitializerMember))
			{
				if (boundNode is BoundIndexerAccess boundIndexerAccess)
				{
					return CreateReceiverOperation(boundIndexerAccess.ReceiverOpt, boundIndexerAccess.ExpressionSymbol);
				}
				throw ExceptionUtilities.UnexpectedValue(boundNode.Kind);
			}
			Symbol? memberSymbol = boundObjectInitializerMember.MemberSymbol;
			if ((object)memberSymbol == null || !memberSymbol.IsStatic)
			{
				return CreateImplicitReceiver(boundObjectInitializerMember.Syntax, boundObjectInitializerMember.ReceiverType);
			}
			return null;
		}
		return CreateReceiverOperation(boundPropertyAccess.ReceiverOpt, boundPropertyAccess.PropertySymbol);
	}

	private IPropertyReferenceOperation CreateBoundPropertyAccessOperation(BoundPropertyAccess boundPropertyAccess)
	{
		IOperation instance = CreateReceiverOperation(boundPropertyAccess.ReceiverOpt, boundPropertyAccess.PropertySymbol);
		ImmutableArray<IArgumentOperation> empty = ImmutableArray<IArgumentOperation>.Empty;
		IPropertySymbol? publicSymbol = boundPropertyAccess.PropertySymbol.GetPublicSymbol();
		SyntaxNode syntax = boundPropertyAccess.Syntax;
		ITypeSymbol publicTypeSymbol = boundPropertyAccess.GetPublicTypeSymbol();
		bool wasCompilerGenerated = boundPropertyAccess.WasCompilerGenerated;
		TypeParameterSymbol constrainedToType = GetConstrainedToType(boundPropertyAccess.PropertySymbol, boundPropertyAccess.ReceiverOpt);
		return new PropertyReferenceOperation(publicSymbol, constrainedToType.GetPublicSymbol(), empty, instance, _semanticModel, syntax, publicTypeSymbol, wasCompilerGenerated);
	}

	private IOperation CreateBoundIndexerAccessOperation(BoundIndexerAccess boundIndexerAccess)
	{
		PropertySymbol indexer = boundIndexerAccess.Indexer;
		SyntaxNode syntax = boundIndexerAccess.Syntax;
		ITypeSymbol publicTypeSymbol = boundIndexerAccess.GetPublicTypeSymbol();
		bool wasCompilerGenerated = boundIndexerAccess.WasCompilerGenerated;
		if (!boundIndexerAccess.OriginalIndexersOpt.IsDefault || boundIndexerAccess.ResultKind == LookupResultKind.OverloadResolutionFailure)
		{
			return new InvalidOperation(CreateFromArray<BoundNode, IOperation>(((IBoundInvalidNode)boundIndexerAccess).InvalidNodeChildren), _semanticModel, syntax, publicTypeSymbol, null, wasCompilerGenerated);
		}
		ImmutableArray<IArgumentOperation> arguments = DeriveArguments(boundIndexerAccess);
		IOperation instance = CreateReceiverOperation(boundIndexerAccess.ReceiverOpt, boundIndexerAccess.ExpressionSymbol);
		TypeParameterSymbol constrainedToType = GetConstrainedToType(indexer, boundIndexerAccess.ReceiverOpt);
		return new PropertyReferenceOperation(indexer.GetPublicSymbol(), constrainedToType.GetPublicSymbol(), arguments, instance, _semanticModel, syntax, publicTypeSymbol, wasCompilerGenerated);
	}

	private IEventReferenceOperation CreateBoundEventAccessOperation(BoundEventAccess boundEventAccess)
	{
		IEventSymbol? publicSymbol = boundEventAccess.EventSymbol.GetPublicSymbol();
		IOperation instance = CreateReceiverOperation(boundEventAccess.ReceiverOpt, boundEventAccess.EventSymbol);
		SyntaxNode syntax = boundEventAccess.Syntax;
		ITypeSymbol publicTypeSymbol = boundEventAccess.GetPublicTypeSymbol();
		bool wasCompilerGenerated = boundEventAccess.WasCompilerGenerated;
		TypeParameterSymbol constrainedToType = GetConstrainedToType(boundEventAccess.EventSymbol, boundEventAccess.ReceiverOpt);
		return new EventReferenceOperation(publicSymbol, constrainedToType.GetPublicSymbol(), instance, _semanticModel, syntax, publicTypeSymbol, wasCompilerGenerated);
	}

	private IEventAssignmentOperation CreateBoundEventAssignmentOperatorOperation(BoundEventAssignmentOperator boundEventAssignmentOperator)
	{
		return new EventAssignmentOperation(CreateBoundEventAccessOperation(boundEventAssignmentOperator), Create(boundEventAssignmentOperator.Argument), syntax: boundEventAssignmentOperator.Syntax, adds: boundEventAssignmentOperator.IsAddition, type: boundEventAssignmentOperator.GetPublicTypeSymbol(), isImplicit: boundEventAssignmentOperator.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IParameterReferenceOperation CreateBoundParameterOperation(BoundParameter boundParameter)
	{
		return new ParameterReferenceOperation(boundParameter.ParameterSymbol.GetPublicSymbol(), syntax: boundParameter.Syntax, type: boundParameter.GetPublicTypeSymbol(), isImplicit: boundParameter.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	internal ILiteralOperation CreateBoundLiteralOperation(BoundLiteral boundLiteral, bool @implicit = false)
	{
		SyntaxNode syntax = boundLiteral.Syntax;
		ITypeSymbol publicTypeSymbol = boundLiteral.GetPublicTypeSymbol();
		ConstantValue constantValueOpt = boundLiteral.ConstantValueOpt;
		bool isImplicit = boundLiteral.WasCompilerGenerated | @implicit;
		return new LiteralOperation(_semanticModel, syntax, publicTypeSymbol, constantValueOpt, isImplicit);
	}

	private IUtf8StringOperation CreateBoundUtf8StringOperation(BoundUtf8String boundNode)
	{
		SyntaxNode syntax = boundNode.Syntax;
		ITypeSymbol publicTypeSymbol = boundNode.GetPublicTypeSymbol();
		bool wasCompilerGenerated = boundNode.WasCompilerGenerated;
		return new Utf8StringOperation(boundNode.Value, _semanticModel, syntax, publicTypeSymbol, wasCompilerGenerated);
	}

	private IAnonymousObjectCreationOperation CreateBoundAnonymousObjectCreationExpressionOperation(BoundAnonymousObjectCreationExpression boundAnonymousObjectCreationExpression)
	{
		SyntaxNode syntax = boundAnonymousObjectCreationExpression.Syntax;
		ITypeSymbol publicTypeSymbol = boundAnonymousObjectCreationExpression.GetPublicTypeSymbol();
		bool wasCompilerGenerated = boundAnonymousObjectCreationExpression.WasCompilerGenerated;
		return new AnonymousObjectCreationOperation(GetAnonymousObjectCreationInitializers(boundAnonymousObjectCreationExpression.Arguments, boundAnonymousObjectCreationExpression.Declarations, syntax, publicTypeSymbol, wasCompilerGenerated), _semanticModel, syntax, publicTypeSymbol, wasCompilerGenerated);
	}

	private IOperation CreateBoundObjectCreationExpressionOperation(BoundObjectCreationExpression boundObjectCreationExpression)
	{
		MethodSymbol constructor = boundObjectCreationExpression.Constructor;
		SyntaxNode syntax = boundObjectCreationExpression.Syntax;
		ITypeSymbol publicTypeSymbol = boundObjectCreationExpression.GetPublicTypeSymbol();
		ConstantValue constantValueOpt = boundObjectCreationExpression.ConstantValueOpt;
		bool wasCompilerGenerated = boundObjectCreationExpression.WasCompilerGenerated;
		if (boundObjectCreationExpression.ResultKind == LookupResultKind.OverloadResolutionFailure || constructor.OriginalDefinition is ErrorMethodSymbol)
		{
			return new InvalidOperation(CreateFromArray<BoundNode, IOperation>(((IBoundInvalidNode)boundObjectCreationExpression).InvalidNodeChildren), _semanticModel, syntax, publicTypeSymbol, constantValueOpt, wasCompilerGenerated);
		}
		if (boundObjectCreationExpression.Type.IsAnonymousType)
		{
			return new AnonymousObjectCreationOperation(GetAnonymousObjectCreationInitializers(boundObjectCreationExpression.Arguments, ImmutableArray<BoundAnonymousPropertyDeclaration>.Empty, syntax, publicTypeSymbol, wasCompilerGenerated), _semanticModel, syntax, publicTypeSymbol, wasCompilerGenerated);
		}
		ImmutableArray<IArgumentOperation> arguments = DeriveArguments(boundObjectCreationExpression);
		IObjectOrCollectionInitializerOperation initializer = (IObjectOrCollectionInitializerOperation)Create(boundObjectCreationExpression.InitializerExpressionOpt);
		return new ObjectCreationOperation(constructor.GetPublicSymbol(), initializer, arguments, _semanticModel, syntax, publicTypeSymbol, constantValueOpt, wasCompilerGenerated);
	}

	private IOperation CreateBoundWithExpressionOperation(BoundWithExpression boundWithExpression)
	{
		IOperation? operand = Create(boundWithExpression.Receiver);
		IObjectOrCollectionInitializerOperation initializer = (IObjectOrCollectionInitializerOperation)Create(boundWithExpression.InitializerExpression);
		MethodSymbol cloneMethod = boundWithExpression.CloneMethod;
		return new WithOperation(syntax: boundWithExpression.Syntax, type: boundWithExpression.GetPublicTypeSymbol(), isImplicit: boundWithExpression.WasCompilerGenerated, operand: operand, cloneMethod: cloneMethod.GetPublicSymbol(), initializer: initializer, semanticModel: _semanticModel);
	}

	private IDynamicObjectCreationOperation CreateBoundDynamicObjectCreationExpressionOperation(BoundDynamicObjectCreationExpression boundDynamicObjectCreationExpression)
	{
		return new DynamicObjectCreationOperation((IObjectOrCollectionInitializerOperation)Create(boundDynamicObjectCreationExpression.InitializerExpressionOpt), CreateFromArray<BoundExpression, IOperation>(boundDynamicObjectCreationExpression.Arguments), boundDynamicObjectCreationExpression.ArgumentNamesOpt.NullToEmpty(), boundDynamicObjectCreationExpression.ArgumentRefKindsOpt.NullToEmpty(), syntax: boundDynamicObjectCreationExpression.Syntax, type: boundDynamicObjectCreationExpression.GetPublicTypeSymbol(), isImplicit: boundDynamicObjectCreationExpression.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	internal IOperation CreateBoundDynamicInvocationExpressionReceiver(BoundNode receiver)
	{
		if (!(receiver is BoundObjectOrCollectionValuePlaceholder boundObjectOrCollectionValuePlaceholder))
		{
			if (receiver is BoundMethodGroup boundMethodGroup)
			{
				return CreateBoundDynamicMemberAccessOperation(boundMethodGroup.ReceiverOpt, TypeMap.AsTypeSymbols(boundMethodGroup.TypeArgumentsOpt), boundMethodGroup.Name, boundMethodGroup.Syntax, boundMethodGroup.GetPublicTypeSymbol(), boundMethodGroup.WasCompilerGenerated);
			}
			return Create(receiver);
		}
		return CreateBoundDynamicMemberAccessOperation(boundObjectOrCollectionValuePlaceholder, ImmutableArray<TypeSymbol>.Empty, "Add", boundObjectOrCollectionValuePlaceholder.Syntax, null, isImplicit: true);
	}

	private IDynamicInvocationOperation CreateBoundDynamicInvocationExpressionOperation(BoundDynamicInvocation boundDynamicInvocation)
	{
		return new DynamicInvocationOperation(CreateBoundDynamicInvocationExpressionReceiver(boundDynamicInvocation.Expression), CreateFromArray<BoundExpression, IOperation>(boundDynamicInvocation.Arguments), boundDynamicInvocation.ArgumentNamesOpt.NullToEmpty(), boundDynamicInvocation.ArgumentRefKindsOpt.NullToEmpty(), syntax: boundDynamicInvocation.Syntax, type: boundDynamicInvocation.GetPublicTypeSymbol(), isImplicit: boundDynamicInvocation.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	internal IOperation CreateBoundDynamicIndexerAccessExpressionReceiver(BoundExpression indexer)
	{
		if (!(indexer is BoundDynamicIndexerAccess boundDynamicIndexerAccess))
		{
			if (indexer is BoundObjectInitializerMember boundObjectInitializerMember)
			{
				return CreateImplicitReceiver(boundObjectInitializerMember.Syntax, boundObjectInitializerMember.ReceiverType);
			}
			throw ExceptionUtilities.UnexpectedValue(indexer.Kind);
		}
		return Create(boundDynamicIndexerAccess.Receiver);
	}

	internal ImmutableArray<IOperation> CreateBoundDynamicIndexerAccessArguments(BoundExpression indexer)
	{
		if (!(indexer is BoundDynamicIndexerAccess boundDynamicIndexerAccess))
		{
			if (indexer is BoundObjectInitializerMember boundObjectInitializerMember)
			{
				return CreateFromArray<BoundExpression, IOperation>(boundObjectInitializerMember.Arguments);
			}
			throw ExceptionUtilities.UnexpectedValue(indexer.Kind);
		}
		return CreateFromArray<BoundExpression, IOperation>(boundDynamicIndexerAccess.Arguments);
	}

	private IDynamicIndexerAccessOperation CreateBoundDynamicIndexerAccessExpressionOperation(BoundDynamicIndexerAccess boundDynamicIndexerAccess)
	{
		return new DynamicIndexerAccessOperation(CreateBoundDynamicIndexerAccessExpressionReceiver(boundDynamicIndexerAccess), CreateBoundDynamicIndexerAccessArguments(boundDynamicIndexerAccess), boundDynamicIndexerAccess.ArgumentNamesOpt.NullToEmpty(), boundDynamicIndexerAccess.ArgumentRefKindsOpt.NullToEmpty(), syntax: boundDynamicIndexerAccess.Syntax, type: boundDynamicIndexerAccess.GetPublicTypeSymbol(), isImplicit: boundDynamicIndexerAccess.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IObjectOrCollectionInitializerOperation CreateBoundObjectInitializerExpressionOperation(BoundObjectInitializerExpression boundObjectInitializerExpression)
	{
		return new ObjectOrCollectionInitializerOperation(CreateFromArray<BoundExpression, IOperation>(BoundObjectCreationExpression.GetChildInitializers(boundObjectInitializerExpression)), syntax: boundObjectInitializerExpression.Syntax, type: boundObjectInitializerExpression.GetPublicTypeSymbol(), isImplicit: boundObjectInitializerExpression.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IObjectOrCollectionInitializerOperation CreateBoundCollectionInitializerExpressionOperation(BoundCollectionInitializerExpression boundCollectionInitializerExpression)
	{
		return new ObjectOrCollectionInitializerOperation(CreateFromArray<BoundExpression, IOperation>(BoundObjectCreationExpression.GetChildInitializers(boundCollectionInitializerExpression)), syntax: boundCollectionInitializerExpression.Syntax, type: boundCollectionInitializerExpression.GetPublicTypeSymbol(), isImplicit: boundCollectionInitializerExpression.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IOperation CreateBoundObjectInitializerMemberOperation(BoundObjectInitializerMember boundObjectInitializerMember, bool isObjectOrCollectionInitializer = false)
	{
		Symbol memberSymbol = boundObjectInitializerMember.MemberSymbol;
		SyntaxNode syntax = boundObjectInitializerMember.Syntax;
		ITypeSymbol publicTypeSymbol = boundObjectInitializerMember.GetPublicTypeSymbol();
		bool wasCompilerGenerated = boundObjectInitializerMember.WasCompilerGenerated;
		if ((object)memberSymbol == null)
		{
			IOperation operation = CreateBoundDynamicIndexerAccessExpressionReceiver(boundObjectInitializerMember);
			ImmutableArray<IOperation> arguments = CreateBoundDynamicIndexerAccessArguments(boundObjectInitializerMember);
			ImmutableArray<string> argumentNames = boundObjectInitializerMember.ArgumentNamesOpt.NullToEmpty();
			ImmutableArray<RefKind> argumentRefKinds = boundObjectInitializerMember.ArgumentRefKindsOpt.NullToEmpty();
			return new DynamicIndexerAccessOperation(operation, arguments, argumentNames, argumentRefKinds, _semanticModel, syntax, publicTypeSymbol, wasCompilerGenerated);
		}
		switch (memberSymbol.Kind)
		{
		case SymbolKind.Field:
		{
			FieldSymbol symbol = (FieldSymbol)memberSymbol;
			bool isDeclaration = false;
			return new FieldReferenceOperation(symbol.GetPublicSymbol(), isDeclaration, createReceiver(), _semanticModel, syntax, publicTypeSymbol, null, wasCompilerGenerated);
		}
		case SymbolKind.Event:
			return new EventReferenceOperation(((EventSymbol)memberSymbol).GetPublicSymbol(), null, createReceiver(), _semanticModel, syntax, publicTypeSymbol, wasCompilerGenerated);
		case SymbolKind.Property:
		{
			PropertySymbol propertySymbol = (PropertySymbol)memberSymbol;
			ImmutableArray<IArgumentOperation> arguments2;
			if (!boundObjectInitializerMember.Arguments.IsEmpty)
			{
				MethodSymbol methodSymbol = ((isObjectOrCollectionInitializer || propertySymbol.RefKind != RefKind.None) ? propertySymbol.GetOwnOrInheritedGetMethod() : propertySymbol.GetOwnOrInheritedSetMethod());
				if (methodSymbol == null || boundObjectInitializerMember.ResultKind == LookupResultKind.OverloadResolutionFailure || methodSymbol.OriginalDefinition is ErrorMethodSymbol)
				{
					return new InvalidOperation(CreateFromArray<BoundNode, IOperation>(((IBoundInvalidNode)boundObjectInitializerMember).InvalidNodeChildren), _semanticModel, syntax, publicTypeSymbol, null, wasCompilerGenerated);
				}
				arguments2 = DeriveArguments(boundObjectInitializerMember);
			}
			else
			{
				arguments2 = ImmutableArray<IArgumentOperation>.Empty;
			}
			return new PropertyReferenceOperation(propertySymbol.GetPublicSymbol(), null, arguments2, createReceiver(), _semanticModel, syntax, publicTypeSymbol, wasCompilerGenerated);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(memberSymbol.Kind);
		}
		IOperation? createReceiver()
		{
			Symbol symbol2 = memberSymbol;
			if ((object)symbol2 == null || !symbol2.IsStatic)
			{
				return CreateImplicitReceiver(boundObjectInitializerMember.Syntax, boundObjectInitializerMember.ReceiverType);
			}
			return null;
		}
	}

	private IOperation CreateBoundDynamicObjectInitializerMemberOperation(BoundDynamicObjectInitializerMember boundDynamicObjectInitializerMember)
	{
		return new DynamicMemberReferenceOperation(CreateImplicitReceiver(boundDynamicObjectInitializerMember.Syntax, boundDynamicObjectInitializerMember.ReceiverType), boundDynamicObjectInitializerMember.MemberName, ImmutableArray<ITypeSymbol>.Empty, boundDynamicObjectInitializerMember.ReceiverType.GetPublicSymbol(), syntax: boundDynamicObjectInitializerMember.Syntax, type: boundDynamicObjectInitializerMember.GetPublicTypeSymbol(), isImplicit: boundDynamicObjectInitializerMember.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IOperation CreateBoundCollectionElementInitializerOperation(BoundCollectionElementInitializer boundCollectionElementInitializer)
	{
		MethodSymbol addMethod = boundCollectionElementInitializer.AddMethod;
		IOperation instance = CreateReceiverOperation(boundCollectionElementInitializer.ImplicitReceiverOpt, addMethod);
		ImmutableArray<IArgumentOperation> arguments = DeriveArguments(boundCollectionElementInitializer);
		SyntaxNode syntax = boundCollectionElementInitializer.Syntax;
		ITypeSymbol publicTypeSymbol = boundCollectionElementInitializer.GetPublicTypeSymbol();
		ConstantValue constantValueOpt = boundCollectionElementInitializer.ConstantValueOpt;
		bool wasCompilerGenerated = boundCollectionElementInitializer.WasCompilerGenerated;
		if (IsMethodInvalid(boundCollectionElementInitializer.ResultKind, addMethod))
		{
			return new InvalidOperation(CreateFromArray<BoundNode, IOperation>(((IBoundInvalidNode)boundCollectionElementInitializer).InvalidNodeChildren), _semanticModel, syntax, publicTypeSymbol, constantValueOpt, wasCompilerGenerated);
		}
		bool isVirtual = IsCallVirtual(addMethod, boundCollectionElementInitializer.ImplicitReceiverOpt);
		return new InvocationOperation(addMethod.GetPublicSymbol(), null, instance, isVirtual, arguments, _semanticModel, syntax, publicTypeSymbol, wasCompilerGenerated);
	}

	private IDynamicMemberReferenceOperation CreateBoundDynamicMemberAccessOperation(BoundDynamicMemberAccess boundDynamicMemberAccess)
	{
		return CreateBoundDynamicMemberAccessOperation(boundDynamicMemberAccess.Receiver, TypeMap.AsTypeSymbols(boundDynamicMemberAccess.TypeArgumentsOpt), boundDynamicMemberAccess.Name, boundDynamicMemberAccess.Syntax, boundDynamicMemberAccess.GetPublicTypeSymbol(), boundDynamicMemberAccess.WasCompilerGenerated);
	}

	private IDynamicMemberReferenceOperation CreateBoundDynamicMemberAccessOperation(BoundExpression? receiver, ImmutableArray<TypeSymbol> typeArgumentsOpt, string memberName, SyntaxNode syntaxNode, ITypeSymbol? type, bool isImplicit)
	{
		ITypeSymbol containingType = null;
		if (receiver != null && receiver.Kind == BoundKind.TypeExpression)
		{
			containingType = receiver.GetPublicTypeSymbol();
			receiver = null;
		}
		ImmutableArray<ITypeSymbol> typeArguments = ImmutableArray<ITypeSymbol>.Empty;
		if (!typeArgumentsOpt.IsDefault)
		{
			typeArguments = typeArgumentsOpt.GetPublicSymbols();
		}
		return new DynamicMemberReferenceOperation(Create(receiver), memberName, typeArguments, containingType, _semanticModel, syntaxNode, type, isImplicit);
	}

	private IDynamicInvocationOperation CreateBoundDynamicCollectionElementInitializerOperation(BoundDynamicCollectionElementInitializer boundCollectionElementInitializer)
	{
		return new DynamicInvocationOperation(CreateBoundDynamicInvocationExpressionReceiver(boundCollectionElementInitializer.Expression), CreateFromArray<BoundExpression, IOperation>(boundCollectionElementInitializer.Arguments), syntax: boundCollectionElementInitializer.Syntax, type: boundCollectionElementInitializer.GetPublicTypeSymbol(), isImplicit: boundCollectionElementInitializer.WasCompilerGenerated, argumentNames: ImmutableArray<string>.Empty, argumentRefKinds: ImmutableArray<RefKind>.Empty, semanticModel: _semanticModel);
	}

	private IOperation CreateUnboundLambdaOperation(UnboundLambda unboundLambda)
	{
		BoundLambda boundNode = unboundLambda.BindForErrorRecovery();
		return Create(boundNode);
	}

	private IAnonymousFunctionOperation CreateBoundLambdaOperation(BoundLambda boundLambda)
	{
		return new AnonymousFunctionOperation(boundLambda.Symbol.GetPublicSymbol(), (IBlockOperation)Create(boundLambda.Body), syntax: boundLambda.Syntax, isImplicit: boundLambda.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private ILocalFunctionOperation CreateBoundLocalFunctionStatementOperation(BoundLocalFunctionStatement boundLocalFunctionStatement)
	{
		IBlockOperation body = (IBlockOperation)Create(boundLocalFunctionStatement.Body);
		object obj;
		if (boundLocalFunctionStatement != null && boundLocalFunctionStatement.BlockBody != null)
		{
			BoundBlock expressionBody = boundLocalFunctionStatement.ExpressionBody;
			if (expressionBody != null)
			{
				obj = (IBlockOperation)Create(expressionBody);
				goto IL_0036;
			}
		}
		obj = null;
		goto IL_0036;
		IL_0036:
		IBlockOperation ignoredBody = (IBlockOperation)obj;
		return new LocalFunctionOperation(boundLocalFunctionStatement.Symbol.GetPublicSymbol(), syntax: boundLocalFunctionStatement.Syntax, isImplicit: boundLocalFunctionStatement.WasCompilerGenerated, body: body, ignoredBody: ignoredBody, semanticModel: _semanticModel);
	}

	private IOperation CreateBoundConversionOperation(BoundConversion boundConversion, bool forceOperandImplicitLiteral = false)
	{
		bool flag = (boundConversion.WasCompilerGenerated || !boundConversion.ExplicitCastInCode) | forceOperandImplicitLiteral;
		BoundExpression operand = boundConversion.Operand;
		if (boundConversion.ConversionKind == ConversionKind.InterpolatedStringHandler)
		{
			return CreateInterpolatedStringHandler(boundConversion);
		}
		if (boundConversion.ConversionKind == ConversionKind.MethodGroup)
		{
			SyntaxNode syntax = boundConversion.Syntax;
			ITypeSymbol publicTypeSymbol = boundConversion.GetPublicTypeSymbol();
			if (boundConversion.Type is FunctionPointerTypeSymbol)
			{
				return new AddressOfOperation(CreateBoundMethodGroupSingleMethodOperation((BoundMethodGroup)boundConversion.Operand, boundConversion.SymbolOpt, suppressVirtualCalls: false), _semanticModel, syntax, publicTypeSymbol, boundConversion.WasCompilerGenerated);
			}
			IOperation operation = CreateDelegateTargetOperation(boundConversion);
			flag = flag || (operation.Syntax == syntax && !operation.IsImplicit);
			return new DelegateCreationOperation(operation, _semanticModel, syntax, publicTypeSymbol, flag);
		}
		SyntaxNode syntax2 = boundConversion.Syntax;
		if (syntax2.IsMissing)
		{
			return Create(operand);
		}
		BoundConversion boundConversion2 = boundConversion;
		Conversion conversion = boundConversion.Conversion;
		if (operand.Syntax == boundConversion.Syntax)
		{
			if (operand.Kind == BoundKind.ConvertedTupleLiteral && TypeSymbol.Equals(operand.Type, boundConversion.Type, TypeCompareKind.ConsiderEverything))
			{
				return Create(operand);
			}
			flag = true;
		}
		if (boundConversion.ExplicitCastInCode && conversion.IsIdentity && operand.Kind == BoundKind.Conversion)
		{
			BoundConversion boundConversion3 = (BoundConversion)operand;
			BoundExpression operand2 = boundConversion3.Operand;
			if (boundConversion3.Syntax == operand2.Syntax && boundConversion3.ExplicitCastInCode && operand2.Kind == BoundKind.ConvertedTupleLiteral && !TypeSymbol.Equals(boundConversion3.Type, operand2.Type, TypeCompareKind.ConsiderEverything))
			{
				conversion = boundConversion3.Conversion;
				boundConversion2 = boundConversion3;
			}
		}
		ITypeSymbol publicTypeSymbol2 = boundConversion.GetPublicTypeSymbol();
		ConstantValue constantValueOpt = boundConversion.ConstantValueOpt;
		if ((operand.Kind == BoundKind.Lambda || operand.Kind == BoundKind.UnboundLambda || operand.Kind == BoundKind.MethodGroup) && boundConversion.Type.IsDelegateType())
		{
			return new DelegateCreationOperation(CreateDelegateTargetOperation(boundConversion2), _semanticModel, syntax2, publicTypeSymbol2, flag);
		}
		bool isTryCast = false;
		bool isChecked = boundConversion.Checked && (conversion.IsNumeric || ((object)boundConversion.SymbolOpt != null && SyntaxFacts.IsCheckedOperator(boundConversion.SymbolOpt.Name)));
		IOperation operand3;
		if (!forceOperandImplicitLiteral)
		{
			operand3 = Create(boundConversion2.Operand);
		}
		else
		{
			IOperation operation2 = CreateBoundLiteralOperation((BoundLiteral)boundConversion2.Operand, @implicit: true);
			operand3 = operation2;
		}
		return new ConversionOperation(operand3, conversion, isTryCast, isChecked, _semanticModel, syntax2, publicTypeSymbol2, constantValueOpt, flag);
	}

	private IConversionOperation CreateBoundAsOperatorOperation(BoundAsOperator boundAsOperator)
	{
		IOperation? operand = Create(boundAsOperator.Operand);
		SyntaxNode syntax = boundAsOperator.Syntax;
		Conversion conversion = BoundNode.GetConversion(boundAsOperator.OperandConversion, boundAsOperator.OperandPlaceholder);
		return new ConversionOperation(isTryCast: true, isChecked: false, type: boundAsOperator.GetPublicTypeSymbol(), isImplicit: boundAsOperator.WasCompilerGenerated, operand: operand, conversion: conversion, semanticModel: _semanticModel, syntax: syntax, constantValue: null);
	}

	private IDelegateCreationOperation CreateBoundDelegateCreationExpressionOperation(BoundDelegateCreationExpression boundDelegateCreationExpression)
	{
		return new DelegateCreationOperation(CreateDelegateTargetOperation(boundDelegateCreationExpression), syntax: boundDelegateCreationExpression.Syntax, type: boundDelegateCreationExpression.GetPublicTypeSymbol(), isImplicit: boundDelegateCreationExpression.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IMethodReferenceOperation CreateBoundMethodGroupSingleMethodOperation(BoundMethodGroup boundMethodGroup, MethodSymbol methodSymbol, bool suppressVirtualCalls)
	{
		TypeParameterSymbol constrainedToType = GetConstrainedToType(methodSymbol, boundMethodGroup.ReceiverOpt);
		bool isVirtual = (object)constrainedToType != null || ((methodSymbol.IsAbstract || methodSymbol.IsOverride || methodSymbol.IsVirtual) && !suppressVirtualCalls);
		IOperation instance = CreateReceiverOperation(boundMethodGroup.ReceiverOpt, methodSymbol);
		SyntaxNode syntax = boundMethodGroup.Syntax;
		ITypeSymbol type = null;
		bool wasCompilerGenerated = boundMethodGroup.WasCompilerGenerated;
		return new MethodReferenceOperation(methodSymbol.GetPublicSymbol(), constrainedToType.GetPublicSymbol(), isVirtual, instance, _semanticModel, syntax, type, wasCompilerGenerated);
	}

	private IIsTypeOperation CreateBoundIsOperatorOperation(BoundIsOperator boundIsOperator)
	{
		return new IsTypeOperation(Create(boundIsOperator.Operand), boundIsOperator.TargetType.GetPublicTypeSymbol(), syntax: boundIsOperator.Syntax, type: boundIsOperator.GetPublicTypeSymbol(), isNegated: false, isImplicit: boundIsOperator.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private ISizeOfOperation CreateBoundSizeOfOperatorOperation(BoundSizeOfOperator boundSizeOfOperator)
	{
		return new SizeOfOperation(boundSizeOfOperator.SourceType.GetPublicTypeSymbol(), syntax: boundSizeOfOperator.Syntax, type: boundSizeOfOperator.GetPublicTypeSymbol(), constantValue: boundSizeOfOperator.ConstantValueOpt, isImplicit: boundSizeOfOperator.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private ITypeOfOperation CreateBoundTypeOfOperatorOperation(BoundTypeOfOperator boundTypeOfOperator)
	{
		return new TypeOfOperation(boundTypeOfOperator.SourceType.GetPublicTypeSymbol(), syntax: boundTypeOfOperator.Syntax, type: boundTypeOfOperator.GetPublicTypeSymbol(), isImplicit: boundTypeOfOperator.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IArrayCreationOperation CreateBoundArrayCreationOperation(BoundArrayCreation boundArrayCreation)
	{
		ImmutableArray<IOperation> dimensionSizes = CreateFromArray<BoundExpression, IOperation>(boundArrayCreation.Bounds);
		IArrayInitializerOperation initializer = (IArrayInitializerOperation)Create(boundArrayCreation.InitializerOpt);
		SyntaxNode syntax = boundArrayCreation.Syntax;
		return new ArrayCreationOperation(type: boundArrayCreation.GetPublicTypeSymbol(), isImplicit: boundArrayCreation.WasCompilerGenerated || (boundArrayCreation.InitializerOpt?.Syntax == syntax && !boundArrayCreation.InitializerOpt.WasCompilerGenerated), dimensionSizes: dimensionSizes, initializer: initializer, semanticModel: _semanticModel, syntax: syntax);
	}

	private IArrayInitializerOperation CreateBoundArrayInitializationOperation(BoundArrayInitialization boundArrayInitialization)
	{
		return new ArrayInitializerOperation(CreateFromArray<BoundExpression, IOperation>(boundArrayInitialization.Initializers), syntax: boundArrayInitialization.Syntax, isImplicit: boundArrayInitialization.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private ICollectionExpressionOperation CreateBoundCollectionExpression(BoundCollectionExpression expr)
	{
		SyntaxNode syntax = expr.Syntax;
		ITypeSymbol publicTypeSymbol = expr.GetPublicTypeSymbol();
		bool wasCompilerGenerated = expr.WasCompilerGenerated;
		IMethodSymbol? publicSymbol = getConstructMethod((CSharpCompilation)_semanticModel.Compilation, expr).GetPublicSymbol();
		ImmutableArray<IOperation> elements = expr.Elements.SelectAsArray((BoundNode element, BoundCollectionExpression expr2) => CreateBoundCollectionExpressionElement(expr2, element), expr);
		return new CollectionExpressionOperation(publicSymbol, elements, _semanticModel, syntax, publicTypeSymbol, wasCompilerGenerated);
		static MethodSymbol? getConstructMethod(CSharpCompilation compilation, BoundCollectionExpression boundCollectionExpression)
		{
			switch (boundCollectionExpression.CollectionTypeKind)
			{
			case CollectionExpressionTypeKind.None:
			case CollectionExpressionTypeKind.Array:
			case CollectionExpressionTypeKind.Span:
			case CollectionExpressionTypeKind.ReadOnlySpan:
			case CollectionExpressionTypeKind.ArrayInterface:
				return null;
			case CollectionExpressionTypeKind.ImplementsIEnumerable:
				return (boundCollectionExpression.CollectionCreation as BoundObjectCreationExpression)?.Constructor;
			case CollectionExpressionTypeKind.CollectionBuilder:
				return boundCollectionExpression.CollectionBuilderMethod;
			default:
				throw ExceptionUtilities.UnexpectedValue(boundCollectionExpression.CollectionTypeKind);
			}
		}
	}

	private IOperation CreateBoundCollectionExpressionElement(BoundCollectionExpression expr, BoundNode element)
	{
		if (!(element is BoundCollectionExpressionSpreadElement element2))
		{
			return Create(Binder.GetUnderlyingCollectionExpressionElement(expr, (BoundExpression)element, throwOnErrors: false));
		}
		return CreateBoundCollectionExpressionSpreadElement(expr, element2);
	}

	private ISpreadOperation CreateBoundCollectionExpressionSpreadElement(BoundCollectionExpression expr, BoundCollectionExpressionSpreadElement element)
	{
		BoundStatement iteratorBody = element.IteratorBody;
		BoundExpression conversion = ((iteratorBody != null) ? Binder.GetUnderlyingCollectionExpressionElement(expr, ((BoundExpressionStatement)iteratorBody).Expression, throwOnErrors: false) : null);
		IOperation? operand = Create(element.Expression);
		SyntaxNode syntax = element.Syntax;
		bool wasCompilerGenerated = element.WasCompilerGenerated;
		ITypeSymbol elementType = element.EnumeratorInfoOpt?.ElementType.GetPublicSymbol();
		Conversion conversion2 = BoundNode.GetConversion(conversion, element.ElementPlaceholder);
		return new SpreadOperation(operand, elementType, conversion2, _semanticModel, syntax, wasCompilerGenerated);
	}

	private IDefaultValueOperation CreateBoundDefaultLiteralOperation(BoundDefaultLiteral boundDefaultLiteral)
	{
		SyntaxNode syntax = boundDefaultLiteral.Syntax;
		ConstantValue constantValueOpt = boundDefaultLiteral.ConstantValueOpt;
		bool wasCompilerGenerated = boundDefaultLiteral.WasCompilerGenerated;
		return new DefaultValueOperation(_semanticModel, syntax, null, constantValueOpt, wasCompilerGenerated);
	}

	private IDefaultValueOperation CreateBoundDefaultExpressionOperation(BoundDefaultExpression boundDefaultExpression)
	{
		SyntaxNode syntax = boundDefaultExpression.Syntax;
		ITypeSymbol publicTypeSymbol = boundDefaultExpression.GetPublicTypeSymbol();
		ConstantValue constantValueOpt = boundDefaultExpression.ConstantValueOpt;
		bool wasCompilerGenerated = boundDefaultExpression.WasCompilerGenerated;
		return new DefaultValueOperation(_semanticModel, syntax, publicTypeSymbol, constantValueOpt, wasCompilerGenerated);
	}

	private IInstanceReferenceOperation CreateBoundBaseReferenceOperation(BoundBaseReference boundBaseReference)
	{
		return new InstanceReferenceOperation(InstanceReferenceKind.ContainingTypeInstance, syntax: boundBaseReference.Syntax, type: boundBaseReference.GetPublicTypeSymbol(), isImplicit: boundBaseReference.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IInstanceReferenceOperation CreateBoundThisReferenceOperation(BoundThisReference boundThisReference)
	{
		return new InstanceReferenceOperation(InstanceReferenceKind.ContainingTypeInstance, syntax: boundThisReference.Syntax, type: boundThisReference.GetPublicTypeSymbol(), isImplicit: boundThisReference.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IOperation CreateBoundAssignmentOperatorOrMemberInitializerOperation(BoundAssignmentOperator boundAssignmentOperator)
	{
		if (!IsMemberInitializer(boundAssignmentOperator))
		{
			return CreateBoundAssignmentOperatorOperation(boundAssignmentOperator);
		}
		return CreateBoundMemberInitializerOperation(boundAssignmentOperator);
	}

	private static bool IsMemberInitializer(BoundAssignmentOperator boundAssignmentOperator)
	{
		BoundExpression right = boundAssignmentOperator.Right;
		if (right == null || right.Kind != BoundKind.ObjectInitializerExpression)
		{
			BoundExpression right2 = boundAssignmentOperator.Right;
			if (right2 == null)
			{
				return false;
			}
			return right2.Kind == BoundKind.CollectionInitializerExpression;
		}
		return true;
	}

	private ISimpleAssignmentOperation CreateBoundAssignmentOperatorOperation(BoundAssignmentOperator boundAssignmentOperator)
	{
		IOperation target = Create(boundAssignmentOperator.Left);
		IOperation value = Create(boundAssignmentOperator.Right);
		return new SimpleAssignmentOperation(boundAssignmentOperator.IsRef, syntax: boundAssignmentOperator.Syntax, type: boundAssignmentOperator.GetPublicTypeSymbol(), constantValue: boundAssignmentOperator.ConstantValueOpt, isImplicit: boundAssignmentOperator.WasCompilerGenerated, target: target, value: value, semanticModel: _semanticModel);
	}

	private IMemberInitializerOperation CreateBoundMemberInitializerOperation(BoundAssignmentOperator boundAssignmentOperator)
	{
		return new MemberInitializerOperation(CreateMemberInitializerInitializedMember(boundAssignmentOperator.Left), (IObjectOrCollectionInitializerOperation)Create(boundAssignmentOperator.Right), syntax: boundAssignmentOperator.Syntax, type: boundAssignmentOperator.GetPublicTypeSymbol(), isImplicit: boundAssignmentOperator.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private ICompoundAssignmentOperation CreateBoundCompoundAssignmentOperatorOperation(BoundCompoundAssignmentOperator boundCompoundAssignmentOperator)
	{
		IOperation target = Create(boundCompoundAssignmentOperator.Left);
		IOperation value = Create(boundCompoundAssignmentOperator.Right);
		BinaryOperatorKind operatorKind = Helper.DeriveBinaryOperatorKind(boundCompoundAssignmentOperator.Operator.Kind);
		Conversion conversion = BoundNode.GetConversion(boundCompoundAssignmentOperator.LeftConversion, boundCompoundAssignmentOperator.LeftPlaceholder);
		Conversion conversion2 = BoundNode.GetConversion(boundCompoundAssignmentOperator.FinalConversion, boundCompoundAssignmentOperator.FinalPlaceholder);
		bool isLifted = boundCompoundAssignmentOperator.Operator.Kind.IsLifted();
		MethodSymbol method = boundCompoundAssignmentOperator.Operator.Method;
		return new CompoundAssignmentOperation(isChecked: boundCompoundAssignmentOperator.Operator.Kind.IsChecked() || ((object)method != null && SyntaxFacts.IsCheckedOperator(method.Name)), operatorMethod: method.GetPublicSymbol(), syntax: boundCompoundAssignmentOperator.Syntax, type: boundCompoundAssignmentOperator.GetPublicTypeSymbol(), isImplicit: boundCompoundAssignmentOperator.WasCompilerGenerated, inConversion: conversion, outConversion: conversion2, operatorKind: operatorKind, isLifted: isLifted, constrainedToType: GetConstrainedToTypeForOperator(method, boundCompoundAssignmentOperator.Operator.ConstrainedToTypeOpt).GetPublicSymbol(), target: target, value: value, semanticModel: _semanticModel);
	}

	private static TypeParameterSymbol? GetConstrainedToTypeForOperator(MethodSymbol? operatorMethod, TypeSymbol? constrainedToTypeOpt)
	{
		if ((object)operatorMethod != null && operatorMethod.IsStatic && (operatorMethod.IsAbstract || operatorMethod.IsVirtual) && constrainedToTypeOpt is TypeParameterSymbol result)
		{
			return result;
		}
		return null;
	}

	private IIncrementOrDecrementOperation CreateBoundIncrementOperatorOperation(BoundIncrementOperator boundIncrementOperator)
	{
		OperationKind kind = (Helper.IsDecrement(boundIncrementOperator.OperatorKind) ? OperationKind.Decrement : OperationKind.Increment);
		return new IncrementOrDecrementOperation(Helper.IsPostfixIncrementOrDecrement(boundIncrementOperator.OperatorKind), boundIncrementOperator.OperatorKind.IsLifted(), boundIncrementOperator.OperatorKind.IsChecked() || ((object)boundIncrementOperator.MethodOpt != null && SyntaxFacts.IsCheckedOperator(boundIncrementOperator.MethodOpt.Name)), Create(boundIncrementOperator.Operand), boundIncrementOperator.MethodOpt.GetPublicSymbol(), syntax: boundIncrementOperator.Syntax, type: boundIncrementOperator.GetPublicTypeSymbol(), isImplicit: boundIncrementOperator.WasCompilerGenerated, constrainedToType: GetConstrainedToTypeForOperator(boundIncrementOperator.MethodOpt, boundIncrementOperator.ConstrainedToTypeOpt).GetPublicSymbol(), kind: kind, semanticModel: _semanticModel);
	}

	private IInvalidOperation CreateBoundBadExpressionOperation(BoundBadExpression boundBadExpression)
	{
		SyntaxNode syntax = boundBadExpression.Syntax;
		ITypeSymbol type = (syntax.IsMissing ? null : boundBadExpression.GetPublicTypeSymbol());
		bool isImplicit = boundBadExpression.WasCompilerGenerated || boundBadExpression.ChildBoundNodes.Any((BoundExpression e, BoundBadExpression boundBadExpression2) => e?.Syntax == boundBadExpression2.Syntax, boundBadExpression);
		return new InvalidOperation(CreateFromArray<BoundExpression, IOperation>(boundBadExpression.ChildBoundNodes), _semanticModel, syntax, type, null, isImplicit);
	}

	private ITypeParameterObjectCreationOperation CreateBoundNewTOperation(BoundNewT boundNewT)
	{
		return new TypeParameterObjectCreationOperation((IObjectOrCollectionInitializerOperation)Create(boundNewT.InitializerExpressionOpt), syntax: boundNewT.Syntax, type: boundNewT.GetPublicTypeSymbol(), isImplicit: boundNewT.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private INoPiaObjectCreationOperation CreateNoPiaObjectCreationExpressionOperation(BoundNoPiaObjectCreationExpression creation)
	{
		return new NoPiaObjectCreationOperation((IObjectOrCollectionInitializerOperation)Create(creation.InitializerExpressionOpt), syntax: creation.Syntax, type: creation.GetPublicTypeSymbol(), isImplicit: creation.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IUnaryOperation CreateBoundUnaryOperatorOperation(BoundUnaryOperator boundUnaryOperator)
	{
		return new UnaryOperation(Helper.DeriveUnaryOperatorKind(boundUnaryOperator.OperatorKind), Create(boundUnaryOperator.Operand), operatorMethod: boundUnaryOperator.MethodOpt.GetPublicSymbol(), syntax: boundUnaryOperator.Syntax, type: boundUnaryOperator.GetPublicTypeSymbol(), constantValue: boundUnaryOperator.ConstantValueOpt, isLifted: boundUnaryOperator.OperatorKind.IsLifted(), isChecked: boundUnaryOperator.OperatorKind.IsChecked() || ((object)boundUnaryOperator.MethodOpt != null && SyntaxFacts.IsCheckedOperator(boundUnaryOperator.MethodOpt.Name)), isImplicit: boundUnaryOperator.WasCompilerGenerated, constrainedToType: GetConstrainedToTypeForOperator(boundUnaryOperator.MethodOpt, boundUnaryOperator.ConstrainedToTypeOpt).GetPublicSymbol(), semanticModel: _semanticModel);
	}

	private IOperation CreateBoundBinaryOperatorBase(BoundBinaryOperatorBase boundBinaryOperatorBase)
	{
		if (boundBinaryOperatorBase is BoundBinaryOperator { InterpolatedStringHandlerData: not null } boundBinaryOperator)
		{
			return CreateBoundInterpolatedStringBinaryOperator(boundBinaryOperator);
		}
		ArrayBuilder<BoundBinaryOperatorBase> instance = ArrayBuilder<BoundBinaryOperatorBase>.GetInstance();
		BoundBinaryOperatorBase result = boundBinaryOperatorBase;
		do
		{
			instance.Push(result);
			result = result.Left as BoundBinaryOperatorBase;
		}
		while ((result != null && !(result is BoundBinaryOperator { InterpolatedStringHandlerData: not null })) ? true : false);
		IOperation operation = null;
		IBinaryOperation binaryOperation = default(IBinaryOperation);
		while (instance.TryPop(out result))
		{
			if (operation == null)
			{
				operation = Create(result.Left);
			}
			IOperation right = Create(result.Right);
			if (!(result is BoundBinaryOperator boundBinaryOperator3))
			{
				if (!(result is BoundUserDefinedConditionalLogicalOperator boundBinaryOperator4))
				{
					if (result != null)
					{
						BoundKind kind = result.Kind;
						throw ExceptionUtilities.UnexpectedValue(kind);
					}
					global::_003CPrivateImplementationDetails_003E.ThrowInvalidOperationException();
				}
				else
				{
					binaryOperation = createBoundUserDefinedConditionalLogicalOperator(boundBinaryOperator4, operation, right);
				}
			}
			else
			{
				binaryOperation = CreateBoundBinaryOperatorOperation(boundBinaryOperator3, operation, right);
			}
			operation = binaryOperation;
		}
		instance.Free();
		return operation;
		IBinaryOperation createBoundUserDefinedConditionalLogicalOperator(BoundUserDefinedConditionalLogicalOperator boundUserDefinedConditionalLogicalOperator, IOperation left, IOperation rightOperand)
		{
			BinaryOperatorKind operatorKind = Helper.DeriveBinaryOperatorKind(boundUserDefinedConditionalLogicalOperator.OperatorKind);
			IMethodSymbol publicSymbol = boundUserDefinedConditionalLogicalOperator.LogicalOperator.GetPublicSymbol();
			IMethodSymbol unaryOperatorMethod = ((boundUserDefinedConditionalLogicalOperator.OperatorKind.Operator() == Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.And) ? boundUserDefinedConditionalLogicalOperator.FalseOperator.GetPublicSymbol() : boundUserDefinedConditionalLogicalOperator.TrueOperator.GetPublicSymbol());
			SyntaxNode syntax = boundUserDefinedConditionalLogicalOperator.Syntax;
			ITypeSymbol publicTypeSymbol = boundUserDefinedConditionalLogicalOperator.GetPublicTypeSymbol();
			ConstantValue constantValueOpt = boundUserDefinedConditionalLogicalOperator.ConstantValueOpt;
			bool isLifted = boundUserDefinedConditionalLogicalOperator.OperatorKind.IsLifted();
			bool isChecked = boundUserDefinedConditionalLogicalOperator.OperatorKind.IsChecked();
			bool isCompareText = false;
			bool wasCompilerGenerated = boundUserDefinedConditionalLogicalOperator.WasCompilerGenerated;
			TypeSymbol symbol = GetConstrainedToTypeForOperator(boundUserDefinedConditionalLogicalOperator.LogicalOperator, boundUserDefinedConditionalLogicalOperator.ConstrainedToTypeOpt) ?? GetConstrainedToTypeForOperator((boundUserDefinedConditionalLogicalOperator.OperatorKind.Operator() == Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.And) ? boundUserDefinedConditionalLogicalOperator.FalseOperator : boundUserDefinedConditionalLogicalOperator.TrueOperator, boundUserDefinedConditionalLogicalOperator.ConstrainedToTypeOpt);
			return new BinaryOperation(operatorKind, left, rightOperand, isLifted, isChecked, isCompareText, publicSymbol, symbol.GetPublicSymbol(), unaryOperatorMethod, _semanticModel, syntax, publicTypeSymbol, constantValueOpt, wasCompilerGenerated);
		}
	}

	private IBinaryOperation CreateBoundBinaryOperatorOperation(BoundBinaryOperator boundBinaryOperator, IOperation left, IOperation right)
	{
		BinaryOperatorKind operatorKind = Helper.DeriveBinaryOperatorKind(boundBinaryOperator.OperatorKind);
		MethodSymbol binaryOperatorMethod = boundBinaryOperator.BinaryOperatorMethod;
		MethodSymbol leftTruthOperatorMethod = boundBinaryOperator.LeftTruthOperatorMethod;
		return new BinaryOperation(syntax: boundBinaryOperator.Syntax, type: boundBinaryOperator.GetPublicTypeSymbol(), constantValue: boundBinaryOperator.ConstantValueOpt, isLifted: boundBinaryOperator.OperatorKind.IsLifted(), isChecked: boundBinaryOperator.OperatorKind.IsChecked() || ((object)binaryOperatorMethod != null && SyntaxFacts.IsCheckedOperator(binaryOperatorMethod.Name)), isCompareText: false, isImplicit: boundBinaryOperator.WasCompilerGenerated, operatorKind: operatorKind, leftOperand: left, rightOperand: right, operatorMethod: binaryOperatorMethod.GetPublicSymbol(), constrainedToType: GetConstrainedToTypeForOperator(binaryOperatorMethod, boundBinaryOperator.ConstrainedToType).GetPublicSymbol(), unaryOperatorMethod: leftTruthOperatorMethod.GetPublicSymbol(), semanticModel: _semanticModel);
	}

	private IOperation CreateBoundInterpolatedStringBinaryOperator(BoundBinaryOperator boundBinaryOperator)
	{
		Func<BoundInterpolatedString, int, (CSharpOperationFactory, InterpolatedStringHandlerData), IOperation> interpolatedStringFactory = createInterpolatedStringOperand;
		Func<BoundBinaryOperator, IOperation, IOperation, (CSharpOperationFactory, InterpolatedStringHandlerData), IOperation> binaryOperatorFactory = createBoundBinaryOperatorOperation;
		return boundBinaryOperator.RewriteInterpolatedStringAddition((this, boundBinaryOperator.InterpolatedStringHandlerData.GetValueOrDefault()), interpolatedStringFactory, binaryOperatorFactory);
		static IBinaryOperation createBoundBinaryOperatorOperation(BoundBinaryOperator boundBinaryOperator2, IOperation left, IOperation right, (CSharpOperationFactory @this, InterpolatedStringHandlerData _) arg)
		{
			return arg.@this.CreateBoundBinaryOperatorOperation(boundBinaryOperator2, left, right);
		}
		static IInterpolatedStringOperation createInterpolatedStringOperand(BoundInterpolatedString boundInterpolatedString, int i, (CSharpOperationFactory @this, InterpolatedStringHandlerData Data) arg)
		{
			return arg.@this.CreateBoundInterpolatedStringExpressionOperation(boundInterpolatedString, arg.Data.PositionInfo[i]);
		}
	}

	private ITupleBinaryOperation CreateBoundTupleBinaryOperatorOperation(BoundTupleBinaryOperator boundTupleBinaryOperator)
	{
		IOperation leftOperand = Create(boundTupleBinaryOperator.Left);
		IOperation rightOperand = Create(boundTupleBinaryOperator.Right);
		return new TupleBinaryOperation(Helper.DeriveBinaryOperatorKind(boundTupleBinaryOperator.OperatorKind), syntax: boundTupleBinaryOperator.Syntax, type: boundTupleBinaryOperator.GetPublicTypeSymbol(), isImplicit: boundTupleBinaryOperator.WasCompilerGenerated, leftOperand: leftOperand, rightOperand: rightOperand, semanticModel: _semanticModel);
	}

	private IConditionalOperation CreateBoundConditionalOperatorOperation(BoundConditionalOperator boundConditionalOperator)
	{
		return new ConditionalOperation(Create(boundConditionalOperator.Condition), Create(boundConditionalOperator.Consequence), Create(boundConditionalOperator.Alternative), boundConditionalOperator.IsRef, syntax: boundConditionalOperator.Syntax, type: boundConditionalOperator.GetPublicTypeSymbol(), constantValue: boundConditionalOperator.ConstantValueOpt, isImplicit: boundConditionalOperator.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private ICoalesceOperation CreateBoundNullCoalescingOperatorOperation(BoundNullCoalescingOperator boundNullCoalescingOperator)
	{
		IOperation? value = Create(boundNullCoalescingOperator.LeftOperand);
		IOperation whenNull = Create(boundNullCoalescingOperator.RightOperand);
		SyntaxNode syntax = boundNullCoalescingOperator.Syntax;
		ITypeSymbol publicTypeSymbol = boundNullCoalescingOperator.GetPublicTypeSymbol();
		ConstantValue constantValueOpt = boundNullCoalescingOperator.ConstantValueOpt;
		bool wasCompilerGenerated = boundNullCoalescingOperator.WasCompilerGenerated;
		Conversion conversion = BoundNode.GetConversion(boundNullCoalescingOperator.LeftConversion, boundNullCoalescingOperator.LeftPlaceholder);
		if (conversion.Exists && !conversion.IsIdentity && boundNullCoalescingOperator.Type.Equals(boundNullCoalescingOperator.LeftOperand.Type?.StrippedType(), TypeCompareKind.IgnoreCustomModifiersAndArraySizesAndLowerBounds | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes))
		{
			conversion = Conversion.Identity;
		}
		return new CoalesceOperation(value, whenNull, conversion, _semanticModel, syntax, publicTypeSymbol, constantValueOpt, wasCompilerGenerated);
	}

	private IOperation CreateBoundNullCoalescingAssignmentOperatorOperation(BoundNullCoalescingAssignmentOperator boundNode)
	{
		return new CoalesceAssignmentOperation(Create(boundNode.LeftOperand), Create(boundNode.RightOperand), syntax: boundNode.Syntax, type: boundNode.GetPublicTypeSymbol(), isImplicit: boundNode.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IAwaitOperation CreateBoundAwaitExpressionOperation(BoundAwaitExpression boundAwaitExpression)
	{
		return new AwaitOperation(Create(boundAwaitExpression.Expression), syntax: boundAwaitExpression.Syntax, type: boundAwaitExpression.GetPublicTypeSymbol(), isImplicit: boundAwaitExpression.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IArrayElementReferenceOperation CreateBoundArrayAccessOperation(BoundArrayAccess boundArrayAccess)
	{
		return new ArrayElementReferenceOperation(Create(boundArrayAccess.Expression), CreateFromArray<BoundExpression, IOperation>(boundArrayAccess.Indices), syntax: boundArrayAccess.Syntax, type: boundArrayAccess.GetPublicTypeSymbol(), isImplicit: boundArrayAccess.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IOperation CreateBoundImplicitIndexerAccessOperation(BoundImplicitIndexerAccess boundIndexerAccess)
	{
		IOperation operation = Create(boundIndexerAccess.Receiver);
		IOperation operation2 = Create(boundIndexerAccess.Argument);
		SyntaxNode syntax = boundIndexerAccess.Syntax;
		ITypeSymbol publicTypeSymbol = boundIndexerAccess.GetPublicTypeSymbol();
		bool wasCompilerGenerated = boundIndexerAccess.WasCompilerGenerated;
		if (boundIndexerAccess.LengthOrCountAccess.Kind == BoundKind.ArrayLength)
		{
			return new ArrayElementReferenceOperation(operation, ImmutableArray.Create(operation2), _semanticModel, syntax, publicTypeSymbol, wasCompilerGenerated);
		}
		IPropertySymbol publicSymbol = Binder.GetPropertySymbol(boundIndexerAccess.LengthOrCountAccess, out var _, out var _).GetPublicSymbol();
		ISymbol publicSymbol2 = Binder.GetIndexerOrImplicitIndexerSymbol(boundIndexerAccess).GetPublicSymbol();
		return new ImplicitIndexerReferenceOperation(operation, operation2, publicSymbol, publicSymbol2, _semanticModel, syntax, publicTypeSymbol, wasCompilerGenerated);
	}

	private IInlineArrayAccessOperation CreateBoundInlineArrayAccessOperation(BoundInlineArrayAccess boundInlineArrayAccess)
	{
		return new InlineArrayAccessOperation(Create(boundInlineArrayAccess.Expression), Create(boundInlineArrayAccess.Argument), syntax: boundInlineArrayAccess.Syntax, type: boundInlineArrayAccess.GetPublicTypeSymbol(), isImplicit: boundInlineArrayAccess.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private INameOfOperation CreateBoundNameOfOperatorOperation(BoundNameOfOperator boundNameOfOperator)
	{
		return new NameOfOperation(Create(boundNameOfOperator.Argument), syntax: boundNameOfOperator.Syntax, type: boundNameOfOperator.GetPublicTypeSymbol(), constantValue: boundNameOfOperator.ConstantValueOpt, isImplicit: boundNameOfOperator.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IThrowOperation CreateBoundThrowExpressionOperation(BoundThrowExpression boundThrowExpression)
	{
		return new ThrowOperation(Create(boundThrowExpression.Expression), syntax: boundThrowExpression.Syntax, type: boundThrowExpression.GetPublicTypeSymbol(), isImplicit: boundThrowExpression.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IAddressOfOperation CreateBoundAddressOfOperatorOperation(BoundAddressOfOperator boundAddressOfOperator)
	{
		return new AddressOfOperation(Create(boundAddressOfOperator.Operand), syntax: boundAddressOfOperator.Syntax, type: boundAddressOfOperator.GetPublicTypeSymbol(), isImplicit: boundAddressOfOperator.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IInstanceReferenceOperation CreateBoundImplicitReceiverOperation(BoundImplicitReceiver boundImplicitReceiver)
	{
		return new InstanceReferenceOperation(InstanceReferenceKind.ImplicitReceiver, syntax: boundImplicitReceiver.Syntax, type: boundImplicitReceiver.GetPublicTypeSymbol(), isImplicit: boundImplicitReceiver.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IConditionalAccessOperation CreateBoundConditionalAccessOperation(BoundConditionalAccess boundConditionalAccess)
	{
		return new ConditionalAccessOperation(Create(boundConditionalAccess.Receiver), Create(boundConditionalAccess.AccessExpression), syntax: boundConditionalAccess.Syntax, type: boundConditionalAccess.GetPublicTypeSymbol(), isImplicit: boundConditionalAccess.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IConditionalAccessInstanceOperation CreateBoundConditionalReceiverOperation(BoundConditionalReceiver boundConditionalReceiver)
	{
		SyntaxNode syntax = boundConditionalReceiver.Syntax;
		ITypeSymbol publicTypeSymbol = boundConditionalReceiver.GetPublicTypeSymbol();
		bool wasCompilerGenerated = boundConditionalReceiver.WasCompilerGenerated;
		return new ConditionalAccessInstanceOperation(_semanticModel, syntax, publicTypeSymbol, wasCompilerGenerated);
	}

	private IFieldInitializerOperation CreateBoundFieldEqualsValueOperation(BoundFieldEqualsValue boundFieldEqualsValue)
	{
		return new FieldInitializerOperation(ImmutableArray.Create(boundFieldEqualsValue.Field.GetPublicSymbol()), value: Create(boundFieldEqualsValue.Value), syntax: boundFieldEqualsValue.Syntax, isImplicit: boundFieldEqualsValue.WasCompilerGenerated, locals: boundFieldEqualsValue.Locals.GetPublicSymbols(), semanticModel: _semanticModel);
	}

	private IPropertyInitializerOperation CreateBoundPropertyEqualsValueOperation(BoundPropertyEqualsValue boundPropertyEqualsValue)
	{
		return new PropertyInitializerOperation(ImmutableArray.Create(boundPropertyEqualsValue.Property.GetPublicSymbol()), value: Create(boundPropertyEqualsValue.Value), syntax: boundPropertyEqualsValue.Syntax, isImplicit: boundPropertyEqualsValue.WasCompilerGenerated, locals: boundPropertyEqualsValue.Locals.GetPublicSymbols(), semanticModel: _semanticModel);
	}

	private IParameterInitializerOperation CreateBoundParameterEqualsValueOperation(BoundParameterEqualsValue boundParameterEqualsValue)
	{
		return new ParameterInitializerOperation(boundParameterEqualsValue.Parameter.GetPublicSymbol(), value: Create(boundParameterEqualsValue.Value), syntax: boundParameterEqualsValue.Syntax, isImplicit: boundParameterEqualsValue.WasCompilerGenerated, locals: boundParameterEqualsValue.Locals.GetPublicSymbols(), semanticModel: _semanticModel);
	}

	private IBlockOperation CreateBoundBlockOperation(BoundBlock boundBlock)
	{
		return new BlockOperation(CreateFromArray<BoundStatement, IOperation>(boundBlock.Statements), boundBlock.Locals.GetPublicSymbols(), syntax: boundBlock.Syntax, isImplicit: boundBlock.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IBranchOperation CreateBoundContinueStatementOperation(BoundContinueStatement boundContinueStatement)
	{
		return new BranchOperation(boundContinueStatement.Label.GetPublicSymbol(), BranchKind.Continue, syntax: boundContinueStatement.Syntax, isImplicit: boundContinueStatement.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IBranchOperation CreateBoundBreakStatementOperation(BoundBreakStatement boundBreakStatement)
	{
		return new BranchOperation(boundBreakStatement.Label.GetPublicSymbol(), BranchKind.Break, syntax: boundBreakStatement.Syntax, isImplicit: boundBreakStatement.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IReturnOperation CreateBoundYieldBreakStatementOperation(BoundYieldBreakStatement boundYieldBreakStatement)
	{
		return new ReturnOperation(null, syntax: boundYieldBreakStatement.Syntax, isImplicit: boundYieldBreakStatement.WasCompilerGenerated, kind: OperationKind.YieldBreak, semanticModel: _semanticModel);
	}

	private IBranchOperation CreateBoundGotoStatementOperation(BoundGotoStatement boundGotoStatement)
	{
		return new BranchOperation(boundGotoStatement.Label.GetPublicSymbol(), BranchKind.GoTo, syntax: boundGotoStatement.Syntax, isImplicit: boundGotoStatement.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IEmptyOperation CreateBoundNoOpStatementOperation(BoundNoOpStatement boundNoOpStatement)
	{
		SyntaxNode syntax = boundNoOpStatement.Syntax;
		bool wasCompilerGenerated = boundNoOpStatement.WasCompilerGenerated;
		return new EmptyOperation(_semanticModel, syntax, wasCompilerGenerated);
	}

	private IConditionalOperation CreateBoundIfStatementOperation(BoundIfStatement boundIfStatement)
	{
		ArrayBuilder<BoundIfStatement> instance = ArrayBuilder<BoundIfStatement>.GetInstance();
		BoundStatement alternativeOpt;
		while (true)
		{
			instance.Push(boundIfStatement);
			alternativeOpt = boundIfStatement.AlternativeOpt;
			if (!(alternativeOpt is BoundIfStatement boundIfStatement2))
			{
				break;
			}
			boundIfStatement = boundIfStatement2;
		}
		IOperation whenFalse = Create(alternativeOpt);
		ConditionalOperation conditionalOperation;
		do
		{
			boundIfStatement = instance.Pop();
			conditionalOperation = new ConditionalOperation(Create(boundIfStatement.Condition), Create(boundIfStatement.Consequence), isRef: false, syntax: boundIfStatement.Syntax, type: null, constantValue: null, isImplicit: boundIfStatement.WasCompilerGenerated, whenFalse: whenFalse, semanticModel: _semanticModel);
			whenFalse = conditionalOperation;
		}
		while (instance.Any());
		instance.Free();
		return conditionalOperation;
	}

	private IWhileLoopOperation CreateBoundWhileStatementOperation(BoundWhileStatement boundWhileStatement)
	{
		return new WhileLoopOperation(Create(boundWhileStatement.Condition), body: Create(boundWhileStatement.Body), locals: boundWhileStatement.Locals.GetPublicSymbols(), continueLabel: boundWhileStatement.ContinueLabel.GetPublicSymbol(), exitLabel: boundWhileStatement.BreakLabel.GetPublicSymbol(), conditionIsTop: true, conditionIsUntil: false, syntax: boundWhileStatement.Syntax, isImplicit: boundWhileStatement.WasCompilerGenerated, ignoredCondition: null, semanticModel: _semanticModel);
	}

	private IWhileLoopOperation CreateBoundDoStatementOperation(BoundDoStatement boundDoStatement)
	{
		return new WhileLoopOperation(Create(boundDoStatement.Condition), body: Create(boundDoStatement.Body), continueLabel: boundDoStatement.ContinueLabel.GetPublicSymbol(), exitLabel: boundDoStatement.BreakLabel.GetPublicSymbol(), conditionIsTop: false, conditionIsUntil: false, locals: boundDoStatement.Locals.GetPublicSymbols(), syntax: boundDoStatement.Syntax, isImplicit: boundDoStatement.WasCompilerGenerated, ignoredCondition: null, semanticModel: _semanticModel);
	}

	private IForLoopOperation CreateBoundForStatementOperation(BoundForStatement boundForStatement)
	{
		return new ForLoopOperation(CreateFromArray<BoundStatement, IOperation>(ToStatements(boundForStatement.Initializer)), condition: Create(boundForStatement.Condition), atLoopBottom: CreateFromArray<BoundStatement, IOperation>(ToStatements(boundForStatement.Increment)), body: Create(boundForStatement.Body), locals: boundForStatement.OuterLocals.GetPublicSymbols(), conditionLocals: boundForStatement.InnerLocals.GetPublicSymbols(), continueLabel: boundForStatement.ContinueLabel.GetPublicSymbol(), exitLabel: boundForStatement.BreakLabel.GetPublicSymbol(), syntax: boundForStatement.Syntax, isImplicit: boundForStatement.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	internal ForEachLoopOperationInfo? GetForEachLoopOperatorInfo(BoundForEachStatement boundForEachStatement)
	{
		ForEachEnumeratorInfo enumeratorInfoOpt = boundForEachStatement.EnumeratorInfoOpt;
		if (enumeratorInfoOpt != null)
		{
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
			CSharpCompilation cSharpCompilation = (CSharpCompilation)_semanticModel.Compilation;
			NamedTypeSymbol targetInterfaceType = (enumeratorInfoOpt.IsAsync ? cSharpCompilation.GetWellKnownType(WellKnownType.System_IAsyncDisposable) : cSharpCompilation.GetSpecialType(SpecialType.System_IDisposable));
			ITypeSymbol? publicSymbol = enumeratorInfoOpt.ElementType.GetPublicSymbol();
			IMethodSymbol? publicSymbol2 = enumeratorInfoOpt.GetEnumeratorInfo.Method.GetPublicSymbol();
			IPropertySymbol? publicSymbol3 = ((PropertySymbol)enumeratorInfoOpt.CurrentPropertyGetter.AssociatedSymbol).GetPublicSymbol();
			IMethodSymbol? publicSymbol4 = enumeratorInfoOpt.MoveNextInfo.Method.GetPublicSymbol();
			bool isAsync = enumeratorInfoOpt.IsAsync;
			object inlineArrayConversion;
			if (enumeratorInfoOpt.InlineArraySpanType != WellKnownType.Unknown)
			{
				IConvertibleConversion convertibleConversion = Conversion.InlineArray;
				inlineArrayConversion = convertibleConversion;
			}
			else
			{
				inlineArrayConversion = null;
			}
			bool inlineArrayUsedAsValue = enumeratorInfoOpt.InlineArrayUsedAsValue;
			bool needsDisposal = enumeratorInfoOpt.NeedsDisposal;
			bool knownToImplementIDisposable = enumeratorInfoOpt.NeedsDisposal && cSharpCompilation.Conversions.HasImplicitConversionToOrImplementsVarianceCompatibleInterface(enumeratorInfoOpt.GetEnumeratorInfo.Method.ReturnType, targetInterfaceType, ref useSiteInfo, out var _);
			IMethodSymbol? patternDisposeMethod = enumeratorInfoOpt.PatternDisposeInfo?.Method.GetPublicSymbol();
			object currentConversion = BoundNode.GetConversion(enumeratorInfoOpt.CurrentConversion, enumeratorInfoOpt.CurrentPlaceholder);
			object elementConversion = BoundNode.GetConversion(boundForEachStatement.ElementConversion, boundForEachStatement.ElementPlaceholder);
			ImmutableArray<IArgumentOperation> getEnumeratorArguments = createArgumentOperations(enumeratorInfoOpt.GetEnumeratorInfo);
			ImmutableArray<IArgumentOperation> moveNextArguments = createArgumentOperations(enumeratorInfoOpt.MoveNextInfo);
			ImmutableArray<IArgumentOperation> disposeArguments = ((enumeratorInfoOpt.PatternDisposeInfo != null) ? CreateDisposeArguments(enumeratorInfoOpt.PatternDisposeInfo) : default(ImmutableArray<IArgumentOperation>));
			return new ForEachLoopOperationInfo(publicSymbol, publicSymbol2, publicSymbol3, publicSymbol4, isAsync, (IConvertibleConversion?)inlineArrayConversion, inlineArrayUsedAsValue, needsDisposal, knownToImplementIDisposable, patternDisposeMethod, (IConvertibleConversion)currentConversion, (IConvertibleConversion)elementConversion, getEnumeratorArguments, moveNextArguments, default(ImmutableArray<IArgumentOperation>), disposeArguments);
		}
		return null;
		ImmutableArray<IArgumentOperation> createArgumentOperations(MethodArgumentInfo? info)
		{
			if (info == null)
			{
				return default(ImmutableArray<IArgumentOperation>);
			}
			if (info.Arguments.Length == 0)
			{
				return ImmutableArray<IArgumentOperation>.Empty;
			}
			return Operation.SetParentOperation(DeriveArguments(info.Method, info.Arguments, default(ImmutableArray<int>), info.DefaultArguments, info.Method.IsExtensionMethod), null);
		}
	}

	internal IOperation CreateBoundForEachStatementLoopControlVariable(BoundForEachStatement boundForEachStatement)
	{
		if (boundForEachStatement.DeconstructionOpt != null)
		{
			return Create(boundForEachStatement.DeconstructionOpt.DeconstructionAssignment.Left);
		}
		if (boundForEachStatement.IterationErrorExpressionOpt != null)
		{
			return Create(boundForEachStatement.IterationErrorExpressionOpt);
		}
		LocalSymbol symbol = boundForEachStatement.IterationVariables[0];
		return new VariableDeclaratorOperation(syntax: boundForEachStatement.IterationVariableType.Syntax, symbol: symbol.GetPublicSymbol(), initializer: null, ignoredArguments: ImmutableArray<IOperation>.Empty, semanticModel: _semanticModel, isImplicit: false);
	}

	private IForEachLoopOperation CreateBoundForEachStatementOperation(BoundForEachStatement boundForEachStatement)
	{
		IOperation loopControlVariable = CreateBoundForEachStatementLoopControlVariable(boundForEachStatement);
		WellKnownType? wellKnownType = boundForEachStatement.EnumeratorInfoOpt?.InlineArraySpanType;
		bool flag = ((!wellKnownType.HasValue || wellKnownType.GetValueOrDefault() == WellKnownType.Unknown) ? true : false);
		BoundExpression boundNode;
		if (!flag && boundForEachStatement.Expression is BoundConversion { Conversion: { IsIdentity: not false }, ExplicitCastInCode: false } boundConversion)
		{
			BoundExpression operand = boundConversion.Operand;
			if (operand != null)
			{
				boundNode = operand;
				goto IL_0088;
			}
		}
		boundNode = boundForEachStatement.Expression;
		goto IL_0088;
		IL_0088:
		IOperation collection = Create(boundNode);
		ImmutableArray<IOperation> empty = ImmutableArray<IOperation>.Empty;
		IOperation body = Create(boundForEachStatement.Body);
		ForEachLoopOperationInfo forEachLoopOperatorInfo = GetForEachLoopOperatorInfo(boundForEachStatement);
		ImmutableArray<ILocalSymbol> publicSymbols = boundForEachStatement.IterationVariables.GetPublicSymbols();
		ILabelSymbol publicSymbol = boundForEachStatement.ContinueLabel.GetPublicSymbol();
		ILabelSymbol publicSymbol2 = boundForEachStatement.BreakLabel.GetPublicSymbol();
		SyntaxNode syntax = boundForEachStatement.Syntax;
		bool wasCompilerGenerated = boundForEachStatement.WasCompilerGenerated;
		ForEachEnumeratorInfo enumeratorInfoOpt = boundForEachStatement.EnumeratorInfoOpt;
		bool isAsynchronous = enumeratorInfoOpt != null && enumeratorInfoOpt.MoveNextAwaitableInfo != null;
		return new ForEachLoopOperation(loopControlVariable, collection, empty, forEachLoopOperatorInfo, isAsynchronous, body, publicSymbols, publicSymbol, publicSymbol2, _semanticModel, syntax, wasCompilerGenerated);
	}

	private ITryOperation CreateBoundTryStatementOperation(BoundTryStatement boundTryStatement)
	{
		return new TryOperation((IBlockOperation)Create(boundTryStatement.TryBlock), CreateFromArray<BoundCatchBlock, ICatchClauseOperation>(boundTryStatement.CatchBlocks), (IBlockOperation)Create(boundTryStatement.FinallyBlockOpt), syntax: boundTryStatement.Syntax, isImplicit: boundTryStatement.WasCompilerGenerated, exitLabel: null, semanticModel: _semanticModel);
	}

	private ICatchClauseOperation CreateBoundCatchBlockOperation(BoundCatchBlock boundCatchBlock)
	{
		return new CatchClauseOperation(CreateVariableDeclarator((BoundLocal)boundCatchBlock.ExceptionSourceOpt), filter: Create(boundCatchBlock.ExceptionFilterOpt), handler: (IBlockOperation)Create(boundCatchBlock.Body), exceptionType: boundCatchBlock.ExceptionTypeOpt.GetPublicSymbol() ?? _semanticModel.Compilation.ObjectType, locals: boundCatchBlock.Locals.GetPublicSymbols(), syntax: boundCatchBlock.Syntax, isImplicit: boundCatchBlock.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IFixedOperation CreateBoundFixedStatementOperation(BoundFixedStatement boundFixedStatement)
	{
		IVariableDeclarationGroupOperation variables = (IVariableDeclarationGroupOperation)Create(boundFixedStatement.Declarations);
		IOperation body = Create(boundFixedStatement.Body);
		return new FixedOperation(boundFixedStatement.Locals.GetPublicSymbols(), syntax: boundFixedStatement.Syntax, isImplicit: boundFixedStatement.WasCompilerGenerated, variables: variables, body: body, semanticModel: _semanticModel);
	}

	private IUsingOperation CreateBoundUsingStatementOperation(BoundUsingStatement boundUsingStatement)
	{
		return new UsingOperation(Create((BoundNode?)(((object)boundUsingStatement.DeclarationsOpt) ?? ((object)boundUsingStatement.ExpressionOpt))), Create(boundUsingStatement.Body), boundUsingStatement.Locals.GetPublicSymbols(), boundUsingStatement.AwaitOpt != null, (boundUsingStatement.PatternDisposeInfoOpt != null) ? new DisposeOperationInfo(boundUsingStatement.PatternDisposeInfoOpt.Method.GetPublicSymbol(), CreateDisposeArguments(boundUsingStatement.PatternDisposeInfoOpt)) : default(DisposeOperationInfo), syntax: boundUsingStatement.Syntax, isImplicit: boundUsingStatement.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IThrowOperation CreateBoundThrowStatementOperation(BoundThrowStatement boundThrowStatement)
	{
		return new ThrowOperation(Create(boundThrowStatement.ExpressionOpt), syntax: boundThrowStatement.Syntax, type: null, isImplicit: boundThrowStatement.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IReturnOperation CreateBoundReturnStatementOperation(BoundReturnStatement boundReturnStatement)
	{
		return new ReturnOperation(Create(boundReturnStatement.ExpressionOpt), syntax: boundReturnStatement.Syntax, isImplicit: boundReturnStatement.WasCompilerGenerated, kind: OperationKind.Return, semanticModel: _semanticModel);
	}

	private IReturnOperation CreateBoundYieldReturnStatementOperation(BoundYieldReturnStatement boundYieldReturnStatement)
	{
		return new ReturnOperation(Create(boundYieldReturnStatement.Expression), syntax: boundYieldReturnStatement.Syntax, isImplicit: boundYieldReturnStatement.WasCompilerGenerated, kind: OperationKind.YieldReturn, semanticModel: _semanticModel);
	}

	private ILockOperation CreateBoundLockStatementOperation(BoundLockStatement boundLockStatement)
	{
		ILocalSymbol lockTakenSymbol = ((_semanticModel.Compilation.CommonGetWellKnownTypeMember(WellKnownMember.System_Threading_Monitor__Enter2) == null) ? null : new SynthesizedLocal((_semanticModel.GetEnclosingSymbol(boundLockStatement.Syntax.SpanStart) as IMethodSymbol).GetSymbol(), TypeWithAnnotations.Create(((CSharpCompilation)_semanticModel.Compilation).GetSpecialType(SpecialType.System_Boolean)), SynthesizedLocalKind.LockTaken, boundLockStatement.Argument.Syntax).GetPublicSymbol());
		return new LockOperation(Create(boundLockStatement.Argument), Create(boundLockStatement.Body), syntax: boundLockStatement.Syntax, isImplicit: boundLockStatement.WasCompilerGenerated, lockTakenSymbol: lockTakenSymbol, semanticModel: _semanticModel);
	}

	private IInvalidOperation CreateBoundBadStatementOperation(BoundBadStatement boundBadStatement)
	{
		SyntaxNode syntax = boundBadStatement.Syntax;
		bool isImplicit = boundBadStatement.WasCompilerGenerated || boundBadStatement.ChildBoundNodes.Any((BoundNode e, BoundBadStatement boundBadStatement2) => e?.Syntax == boundBadStatement2.Syntax, boundBadStatement);
		return new InvalidOperation(CreateFromArray<BoundNode, IOperation>(boundBadStatement.ChildBoundNodes), _semanticModel, syntax, null, null, isImplicit);
	}

	private IOperation CreateBoundLocalDeclarationOperation(BoundLocalDeclaration boundLocalDeclaration)
	{
		SyntaxNode syntax = boundLocalDeclaration.Syntax;
		SyntaxNode syntaxNode2;
		SyntaxNode syntaxNode;
		switch (syntax.Kind())
		{
		case SyntaxKind.LocalDeclarationStatement:
			syntaxNode2 = ((LocalDeclarationStatementSyntax)(syntaxNode = (LocalDeclarationStatementSyntax)syntax)).Declaration;
			break;
		case SyntaxKind.VariableDeclarator:
			syntaxNode = syntax.Parent;
			syntaxNode2 = syntax.Parent;
			break;
		default:
			syntaxNode = (syntaxNode2 = syntax);
			break;
		}
		bool wasCompilerGenerated = boundLocalDeclaration.WasCompilerGenerated;
		ImmutableArray<IVariableDeclaratorOperation> declarators = CreateVariableDeclarator(boundLocalDeclaration, syntaxNode2);
		ImmutableArray<IOperation> ignoredDimensions = CreateIgnoredDimensions(boundLocalDeclaration);
		VariableDeclarationOperation item = new VariableDeclarationOperation(declarators, null, ignoredDimensions, _semanticModel, syntaxNode2, wasCompilerGenerated);
		return new VariableDeclarationGroupOperation(isImplicit: syntaxNode == syntaxNode2 || boundLocalDeclaration.WasCompilerGenerated, declarations: ImmutableArray.Create((IVariableDeclarationOperation)item), semanticModel: _semanticModel, syntax: syntaxNode);
	}

	private IOperation CreateBoundMultipleLocalDeclarationsBaseOperation(BoundMultipleLocalDeclarationsBase boundMultipleLocalDeclarations)
	{
		SyntaxNode syntax = boundMultipleLocalDeclarations.Syntax;
		SyntaxNode syntaxNode = (syntax.IsKind(SyntaxKind.LocalDeclarationStatement) ? ((LocalDeclarationStatementSyntax)syntax).Declaration : syntax);
		bool wasCompilerGenerated = boundMultipleLocalDeclarations.WasCompilerGenerated;
		ImmutableArray<IVariableDeclaratorOperation> declarators = CreateVariableDeclarator(boundMultipleLocalDeclarations, syntaxNode);
		ImmutableArray<IOperation> ignoredDimensions = CreateIgnoredDimensions(boundMultipleLocalDeclarations);
		VariableDeclarationOperation item = new VariableDeclarationOperation(declarators, null, ignoredDimensions, _semanticModel, syntaxNode, wasCompilerGenerated);
		VariableDeclarationGroupOperation variableDeclarationGroupOperation = new VariableDeclarationGroupOperation(isImplicit: syntax == syntaxNode || boundMultipleLocalDeclarations.WasCompilerGenerated || boundMultipleLocalDeclarations is BoundUsingLocalDeclarations, declarations: ImmutableArray.Create((IVariableDeclarationOperation)item), semanticModel: _semanticModel, syntax: syntax);
		if (boundMultipleLocalDeclarations is BoundUsingLocalDeclarations boundUsingLocalDeclarations)
		{
			return new UsingDeclarationOperation(variableDeclarationGroupOperation, boundUsingLocalDeclarations.AwaitOpt != null, (boundUsingLocalDeclarations.PatternDisposeInfoOpt != null) ? new DisposeOperationInfo(boundUsingLocalDeclarations.PatternDisposeInfoOpt.Method.GetPublicSymbol(), CreateDisposeArguments(boundUsingLocalDeclarations.PatternDisposeInfoOpt)) : default(DisposeOperationInfo), _semanticModel, syntax, boundMultipleLocalDeclarations.WasCompilerGenerated);
		}
		return variableDeclarationGroupOperation;
	}

	private ILabeledOperation CreateBoundLabelStatementOperation(BoundLabelStatement boundLabelStatement)
	{
		return new LabeledOperation(boundLabelStatement.Label.GetPublicSymbol(), syntax: boundLabelStatement.Syntax, isImplicit: boundLabelStatement.WasCompilerGenerated, operation: null, semanticModel: _semanticModel);
	}

	private ILabeledOperation CreateBoundLabeledStatementOperation(BoundLabeledStatement boundLabeledStatement)
	{
		return new LabeledOperation(boundLabeledStatement.Label.GetPublicSymbol(), Create(boundLabeledStatement.Body), syntax: boundLabeledStatement.Syntax, isImplicit: boundLabeledStatement.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IExpressionStatementOperation CreateBoundExpressionStatementOperation(BoundExpressionStatement boundExpressionStatement)
	{
		bool isImplicit = boundExpressionStatement.WasCompilerGenerated || boundExpressionStatement.Syntax == boundExpressionStatement.Expression.Syntax;
		SyntaxNode syntax = boundExpressionStatement.Syntax;
		IOperation? operation = Create(boundExpressionStatement.Expression);
		if (boundExpressionStatement.Expression is BoundSequence)
		{
			isImplicit = true;
		}
		return new ExpressionStatementOperation(operation, _semanticModel, syntax, isImplicit);
	}

	internal IOperation CreateBoundTupleOperation(BoundTupleExpression boundTupleExpression, bool createDeclaration = true)
	{
		SyntaxNode syntaxNode = boundTupleExpression.Syntax;
		bool wasCompilerGenerated = boundTupleExpression.WasCompilerGenerated;
		ITypeSymbol publicTypeSymbol = boundTupleExpression.GetPublicTypeSymbol();
		if (syntaxNode is DeclarationExpressionSyntax declarationExpressionSyntax)
		{
			syntaxNode = declarationExpressionSyntax.Designation;
			if (createDeclaration)
			{
				return new DeclarationExpressionOperation(CreateBoundTupleOperation(boundTupleExpression, createDeclaration: false), _semanticModel, declarationExpressionSyntax, publicTypeSymbol, isImplicit: false);
			}
		}
		TypeSymbol typeSymbol = default(TypeSymbol);
		if (boundTupleExpression is BoundTupleLiteral boundTupleLiteral)
		{
			TypeSymbol type = boundTupleLiteral.Type;
			typeSymbol = type;
		}
		else if (boundTupleExpression is BoundConvertedTupleLiteral boundConvertedTupleLiteral)
		{
			BoundTupleLiteral sourceTuple = boundConvertedTupleLiteral.SourceTuple;
			if (sourceTuple != null)
			{
				TypeSymbol type2 = sourceTuple.Type;
				typeSymbol = type2;
			}
			else
			{
				typeSymbol = null;
			}
		}
		else
		{
			if (boundTupleExpression != null)
			{
				BoundKind kind = boundTupleExpression.Kind;
				throw ExceptionUtilities.UnexpectedValue(kind);
			}
			global::_003CPrivateImplementationDetails_003E.ThrowInvalidOperationException();
		}
		TypeSymbol symbol = typeSymbol;
		return new TupleOperation(CreateFromArray<BoundExpression, IOperation>(boundTupleExpression.Arguments), symbol.GetPublicSymbol(), _semanticModel, syntaxNode, publicTypeSymbol, wasCompilerGenerated);
	}

	private IInterpolatedStringOperation CreateBoundInterpolatedStringExpressionOperation(BoundInterpolatedString boundInterpolatedString, ImmutableArray<(bool IsLiteral, bool HasAlignment, bool HasFormat)>? positionInfo = null)
	{
		if (!positionInfo.HasValue)
		{
			InterpolatedStringHandlerData? interpolationData = boundInterpolatedString.InterpolationData;
			if (interpolationData.HasValue)
			{
				InterpolatedStringHandlerData valueOrDefault = interpolationData.GetValueOrDefault();
				if ((object)valueOrDefault.BuilderType != null)
				{
					ImmutableArray<ImmutableArray<(bool, bool, bool)>> positionInfo2 = valueOrDefault.PositionInfo;
					positionInfo = positionInfo2[0];
				}
			}
		}
		return new InterpolatedStringOperation(CreateBoundInterpolatedStringContentOperation(boundInterpolatedString.Parts, positionInfo), syntax: boundInterpolatedString.Syntax, type: boundInterpolatedString.GetPublicTypeSymbol(), constantValue: boundInterpolatedString.ConstantValueOpt, isImplicit: boundInterpolatedString.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	internal ImmutableArray<IInterpolatedStringContentOperation> CreateBoundInterpolatedStringContentOperation(ImmutableArray<BoundExpression> parts, ImmutableArray<(bool IsLiteral, bool HasAlignment, bool HasFormat)>? positionInfo)
	{
		if (positionInfo.HasValue)
		{
			ImmutableArray<(bool, bool, bool)> valueOrDefault = positionInfo.GetValueOrDefault();
			return createHandlerInterpolatedStringContent(valueOrDefault);
		}
		return createNonHandlerInterpolatedStringContent();
		ImmutableArray<IInterpolatedStringContentOperation> createHandlerInterpolatedStringContent(ImmutableArray<(bool IsLiteral, bool HasAlignment, bool HasFormat)> immutableArray)
		{
			ArrayBuilder<IInterpolatedStringContentOperation> instance = ArrayBuilder<IInterpolatedStringContentOperation>.GetInstance(parts.Length);
			for (int i = 0; i < parts.Length; i++)
			{
				BoundExpression boundExpression = parts[i];
				(bool, bool, bool) currentPosition = immutableArray[i];
				BoundExpression boundExpression2;
				BoundExpression boundNode;
				BoundExpression boundNode2;
				if (boundExpression is BoundCall boundCall)
				{
					(boundExpression2, boundNode, boundNode2) = getCallInfo(boundCall.Arguments, boundCall.ArgumentNamesOpt, currentPosition);
				}
				else if (boundExpression is BoundDynamicInvocation boundDynamicInvocation)
				{
					(boundExpression2, boundNode, boundNode2) = getCallInfo(boundDynamicInvocation.Arguments, boundDynamicInvocation.ArgumentNamesOpt, currentPosition);
				}
				else
				{
					if (!(boundExpression is BoundBadExpression boundBadExpression))
					{
						throw ExceptionUtilities.UnexpectedValue(boundExpression.Kind);
					}
					boundExpression2 = boundBadExpression.ChildBoundNodes[0];
					if (currentPosition.Item1)
					{
						boundNode = (boundNode2 = null);
					}
					else
					{
						boundNode = (currentPosition.Item2 ? boundBadExpression.ChildBoundNodes[1] : null);
						object obj;
						if (!currentPosition.Item3)
						{
							obj = null;
						}
						else
						{
							ImmutableArray<BoundExpression> childBoundNodes = boundBadExpression.ChildBoundNodes;
							obj = childBoundNodes[childBoundNodes.Length - 2];
						}
						boundNode2 = (BoundExpression)obj;
					}
				}
				bool isImplicit = false;
				if (currentPosition.Item1)
				{
					IOperation operation;
					if (!(boundExpression2 is BoundLiteral boundLiteral))
					{
						if (!(boundExpression2 is BoundConversion boundConversion) || !(boundConversion.Operand is BoundLiteral))
						{
							throw ExceptionUtilities.UnexpectedValue(boundExpression2.Kind);
						}
						operation = CreateBoundConversionOperation(boundConversion, forceOperandImplicitLiteral: true);
					}
					else
					{
						operation = CreateBoundLiteralOperation(boundLiteral, @implicit: true);
					}
					IOperation text = operation;
					instance.Add(new InterpolatedStringTextOperation(text, _semanticModel, boundExpression.Syntax, isImplicit));
				}
				else
				{
					IOperation expression = Create(boundExpression2);
					IOperation alignment = Create(boundNode);
					IOperation formatString = Create(boundNode2);
					instance.Add(new InterpolationOperation(expression, alignment, formatString, _semanticModel, boundExpression.Syntax, isImplicit));
				}
			}
			return instance.ToImmutableAndFree();
		}
		ImmutableArray<IInterpolatedStringContentOperation> createNonHandlerInterpolatedStringContent()
		{
			ArrayBuilder<IInterpolatedStringContentOperation> instance = ArrayBuilder<IInterpolatedStringContentOperation>.GetInstance(parts.Length);
			foreach (BoundExpression item4 in parts)
			{
				if (item4.Kind == BoundKind.StringInsert)
				{
					instance.Add((IInterpolatedStringContentOperation)Create(item4));
				}
				else
				{
					instance.Add(CreateBoundInterpolatedStringTextOperation((BoundLiteral)item4));
				}
			}
			return instance.ToImmutableAndFree();
		}
		static (BoundExpression Value, BoundExpression? Alignment, BoundExpression? Format) getCallInfo(ImmutableArray<BoundExpression> arguments, ImmutableArray<string?> argumentNamesOpt, (bool IsLiteral, bool HasAlignment, bool HasFormat) currentPosition)
		{
			BoundExpression item = arguments[0];
			if (currentPosition.IsLiteral || argumentNamesOpt.IsDefault)
			{
				return (Value: item, Alignment: null, Format: null);
			}
			int num = argumentNamesOpt.IndexOf("alignment");
			BoundExpression item2 = ((num == -1) ? null : arguments[num]);
			int num2 = argumentNamesOpt.IndexOf("format");
			BoundExpression item3 = ((num2 == -1) ? null : arguments[num2]);
			return (Value: item, Alignment: item2, Format: item3);
		}
	}

	private IInterpolationOperation CreateBoundInterpolationOperation(BoundStringInsert boundStringInsert)
	{
		return new InterpolationOperation(Create(boundStringInsert.Value), Create(boundStringInsert.Alignment), Create(boundStringInsert.Format), syntax: boundStringInsert.Syntax, isImplicit: boundStringInsert.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IInterpolatedStringTextOperation CreateBoundInterpolatedStringTextOperation(BoundLiteral boundNode)
	{
		return new InterpolatedStringTextOperation(CreateBoundLiteralOperation(boundNode, @implicit: true), syntax: boundNode.Syntax, isImplicit: boundNode.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IInterpolatedStringHandlerCreationOperation CreateInterpolatedStringHandler(BoundConversion conversion)
	{
		InterpolatedStringHandlerData interpolatedStringHandlerData = conversion.Operand.GetInterpolatedStringHandlerData();
		return new InterpolatedStringHandlerCreationOperation(Create(interpolatedStringHandlerData.Construction), content: createContent(conversion.Operand), isImplicit: conversion.WasCompilerGenerated || !conversion.ExplicitCastInCode, handlerCreationHasSuccessParameter: interpolatedStringHandlerData.HasTrailingHandlerValidityParameter, handlerAppendCallsReturnBool: interpolatedStringHandlerData.UsesBoolReturns, semanticModel: _semanticModel, syntax: conversion.Syntax, type: conversion.GetPublicTypeSymbol());
		IOperation createContent(BoundExpression current)
		{
			if (current is BoundBinaryOperator boundBinaryOperator)
			{
				IOperation left = createContent(boundBinaryOperator.Left);
				IOperation right = createContent(boundBinaryOperator.Right);
				return new InterpolatedStringAdditionOperation(left, right, _semanticModel, current.Syntax, current.WasCompilerGenerated);
			}
			if (current is BoundInterpolatedString boundInterpolatedString)
			{
				return new InterpolatedStringOperation(boundInterpolatedString.Parts.SelectAsArray((Func<BoundExpression, CSharpOperationFactory, IInterpolatedStringContentOperation>)delegate(BoundExpression part, CSharpOperationFactory @this)
				{
					string text;
					if (part is BoundCall boundCall)
					{
						MethodSymbol method = boundCall.Method;
						if ((object)method != null)
						{
							string name = method.Name;
							text = name;
							goto IL_007c;
						}
					}
					else if (part is BoundDynamicInvocation boundDynamicInvocation)
					{
						if (boundDynamicInvocation.Expression is BoundMethodGroup boundMethodGroup)
						{
							string name2 = boundMethodGroup.Name;
							text = name2;
							goto IL_007c;
						}
					}
					else if (part == null)
					{
						goto IL_006b;
					}
					if (!part.HasErrors)
					{
						goto IL_006b;
					}
					text = "";
					goto IL_007c;
					IL_006b:
					throw ExceptionUtilities.UnexpectedValue(part.Kind);
					IL_007c:
					string text2 = text;
					OperationKind operationKind;
					if (text2 == null || text2.Length != 0)
					{
						if (!(text2 == "AppendLiteral"))
						{
							if (!(text2 == "AppendFormatted"))
							{
								throw ExceptionUtilities.UnexpectedValue(text2);
							}
							operationKind = OperationKind.InterpolatedStringAppendFormatted;
						}
						else
						{
							operationKind = OperationKind.InterpolatedStringAppendLiteral;
						}
					}
					else
					{
						operationKind = OperationKind.InterpolatedStringAppendInvalid;
					}
					OperationKind kind = operationKind;
					return new InterpolatedStringAppendOperation(@this.Create(part), kind, @this._semanticModel, part.Syntax, isImplicit: true);
				}, this), _semanticModel, boundInterpolatedString.Syntax, boundInterpolatedString.GetPublicTypeSymbol(), boundInterpolatedString.ConstantValueOpt, boundInterpolatedString.WasCompilerGenerated);
			}
			throw ExceptionUtilities.UnexpectedValue(current.Kind);
		}
	}

	private IOperation CreateBoundInterpolatedStringArgumentPlaceholder(BoundInterpolatedStringArgumentPlaceholder placeholder)
	{
		SyntaxNode syntax = placeholder.Syntax;
		bool isImplicit = true;
		ITypeSymbol publicTypeSymbol = placeholder.GetPublicTypeSymbol();
		if (placeholder.ArgumentIndex == -4)
		{
			return new InvalidOperation(ImmutableArray<IOperation>.Empty, _semanticModel, syntax, publicTypeSymbol, placeholder.ConstantValueOpt, isImplicit);
		}
		int argumentIndex = placeholder.ArgumentIndex;
		(InterpolatedStringArgumentPlaceholderKind, int) tuple;
		if (argumentIndex < 0)
		{
			switch (argumentIndex)
			{
			case -2:
			case -1:
				tuple = (InterpolatedStringArgumentPlaceholderKind.CallsiteReceiver, -1);
				break;
			case -3:
				tuple = (InterpolatedStringArgumentPlaceholderKind.TrailingValidityArgument, -1);
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(placeholder.ArgumentIndex);
			}
		}
		else
		{
			tuple = (InterpolatedStringArgumentPlaceholderKind.CallsiteArgument, argumentIndex);
		}
		(InterpolatedStringArgumentPlaceholderKind, int) tuple2 = tuple;
		var (placeholderKind, _) = tuple2;
		return new InterpolatedStringHandlerArgumentPlaceholderOperation(tuple2.Item2, placeholderKind, _semanticModel, syntax, isImplicit);
	}

	private IOperation CreateBoundInterpolatedStringHandlerPlaceholder(BoundInterpolatedStringHandlerPlaceholder placeholder)
	{
		return new InstanceReferenceOperation(InstanceReferenceKind.InterpolatedStringHandler, _semanticModel, placeholder.Syntax, placeholder.GetPublicTypeSymbol(), placeholder.WasCompilerGenerated);
	}

	private IConstantPatternOperation CreateBoundConstantPatternOperation(BoundConstantPattern boundConstantPattern)
	{
		IOperation? value = Create(boundConstantPattern.Value);
		SyntaxNode syntax = boundConstantPattern.Syntax;
		bool wasCompilerGenerated = boundConstantPattern.WasCompilerGenerated;
		TypeSymbol inputType = boundConstantPattern.InputType;
		return new ConstantPatternOperation(narrowedType: boundConstantPattern.NarrowedType.GetPublicSymbol(), value: value, inputType: inputType.GetPublicSymbol(), semanticModel: _semanticModel, syntax: syntax, isImplicit: wasCompilerGenerated);
	}

	private IOperation CreateBoundRelationalPatternOperation(BoundRelationalPattern boundRelationalPattern)
	{
		BinaryOperatorKind operatorKind = Helper.DeriveBinaryOperatorKind(boundRelationalPattern.Relation);
		IOperation value = Create(boundRelationalPattern.Value);
		SyntaxNode syntax = boundRelationalPattern.Syntax;
		bool wasCompilerGenerated = boundRelationalPattern.WasCompilerGenerated;
		TypeSymbol inputType = boundRelationalPattern.InputType;
		return new RelationalPatternOperation(narrowedType: boundRelationalPattern.NarrowedType.GetPublicSymbol(), operatorKind: operatorKind, value: value, inputType: inputType.GetPublicSymbol(), semanticModel: _semanticModel, syntax: syntax, isImplicit: wasCompilerGenerated);
	}

	private IDeclarationPatternOperation CreateBoundDeclarationPatternOperation(BoundDeclarationPattern boundDeclarationPattern)
	{
		ISymbol publicSymbol = boundDeclarationPattern.Variable.GetPublicSymbol();
		if (publicSymbol == null)
		{
			BoundExpression? variableAccess = boundDeclarationPattern.VariableAccess;
			if (variableAccess != null && variableAccess.Kind == BoundKind.DiscardExpression)
			{
				publicSymbol = ((BoundDiscardExpression)boundDeclarationPattern.VariableAccess).ExpressionSymbol.GetPublicSymbol();
			}
		}
		ITypeSymbol publicSymbol2 = boundDeclarationPattern.InputType.GetPublicSymbol();
		ITypeSymbol publicSymbol3 = boundDeclarationPattern.NarrowedType.GetPublicSymbol();
		bool isVar = boundDeclarationPattern.IsVar;
		return new DeclarationPatternOperation(isVar ? null : boundDeclarationPattern.DeclaredType.GetPublicTypeSymbol(), syntax: boundDeclarationPattern.Syntax, isImplicit: boundDeclarationPattern.WasCompilerGenerated, matchesNull: isVar, declaredSymbol: publicSymbol, inputType: publicSymbol2, narrowedType: publicSymbol3, semanticModel: _semanticModel);
	}

	private IRecursivePatternOperation CreateBoundRecursivePatternOperation(BoundRecursivePattern boundRecursivePattern)
	{
		ITypeSymbol publicSymbol = (boundRecursivePattern.DeclaredType?.Type ?? boundRecursivePattern.InputType.StrippedType()).GetPublicSymbol();
		ImmutableArray<BoundPositionalSubpattern> deconstruction = boundRecursivePattern.Deconstruction;
		ImmutableArray<IPatternOperation> deconstructionSubpatterns = ((!deconstruction.IsDefault) ? deconstruction.SelectAsArray((BoundPositionalSubpattern p, CSharpOperationFactory fac) => (IPatternOperation)fac.Create(p.Pattern), this) : ImmutableArray<IPatternOperation>.Empty);
		ImmutableArray<BoundPropertySubpattern> properties = boundRecursivePattern.Properties;
		ImmutableArray<IPropertySubpatternOperation> propertySubpatterns = ((!properties.IsDefault) ? properties.SelectAsArray((BoundPropertySubpattern p, (CSharpOperationFactory Fac, ITypeSymbol MatchedType) arg) => arg.Fac.CreatePropertySubpattern(p, arg.MatchedType), (this, publicSymbol)) : ImmutableArray<IPropertySubpatternOperation>.Empty);
		return new RecursivePatternOperation(publicSymbol, boundRecursivePattern.DeconstructMethod.GetPublicSymbol(), deconstructionSubpatterns, propertySubpatterns, boundRecursivePattern.Variable.GetPublicSymbol(), boundRecursivePattern.InputType.GetPublicSymbol(), boundRecursivePattern.NarrowedType.GetPublicSymbol(), _semanticModel, boundRecursivePattern.Syntax, boundRecursivePattern.WasCompilerGenerated);
	}

	private IRecursivePatternOperation CreateBoundRecursivePatternOperation(BoundITuplePattern boundITuplePattern)
	{
		ImmutableArray<BoundPositionalSubpattern> subpatterns = boundITuplePattern.Subpatterns;
		ImmutableArray<IPatternOperation> deconstructionSubpatterns = ((!subpatterns.IsDefault) ? subpatterns.SelectAsArray((BoundPositionalSubpattern p, CSharpOperationFactory fac) => (IPatternOperation)fac.Create(p.Pattern), this) : ImmutableArray<IPatternOperation>.Empty);
		return new RecursivePatternOperation(boundITuplePattern.InputType.StrippedType().GetPublicSymbol(), boundITuplePattern.GetLengthMethod.ContainingType.GetPublicSymbol(), deconstructionSubpatterns, ImmutableArray<IPropertySubpatternOperation>.Empty, null, boundITuplePattern.InputType.GetPublicSymbol(), boundITuplePattern.NarrowedType.GetPublicSymbol(), _semanticModel, boundITuplePattern.Syntax, boundITuplePattern.WasCompilerGenerated);
	}

	private IOperation CreateBoundTypePatternOperation(BoundTypePattern boundTypePattern)
	{
		return new TypePatternOperation(boundTypePattern.NarrowedType.GetPublicSymbol(), boundTypePattern.InputType.GetPublicSymbol(), boundTypePattern.NarrowedType.GetPublicSymbol(), _semanticModel, boundTypePattern.Syntax, boundTypePattern.WasCompilerGenerated);
	}

	private IOperation CreateBoundSlicePatternOperation(BoundSlicePattern boundNode)
	{
		return new SlicePatternOperation((boundNode.Pattern == null) ? null : Binder.GetIndexerOrImplicitIndexerSymbol(boundNode.IndexerAccess).GetPublicSymbol(), (IPatternOperation)Create(boundNode.Pattern), boundNode.InputType.GetPublicSymbol(), boundNode.NarrowedType.GetPublicSymbol(), _semanticModel, boundNode.Syntax, boundNode.WasCompilerGenerated);
	}

	private IOperation CreateBoundListPatternOperation(BoundListPattern boundNode)
	{
		BoundExpression receiver;
		SyntaxNode propertySyntax;
		return new ListPatternOperation(Binder.GetPropertySymbol(boundNode.LengthAccess, out receiver, out propertySyntax).GetPublicSymbol(), Binder.GetIndexerOrImplicitIndexerSymbol(boundNode.IndexerAccess).GetPublicSymbol(), boundNode.Subpatterns.SelectAsArray((BoundPattern p, CSharpOperationFactory fac) => (IPatternOperation)fac.Create(p), this), boundNode.Variable.GetPublicSymbol(), boundNode.InputType.GetPublicSymbol(), boundNode.NarrowedType.GetPublicSymbol(), _semanticModel, boundNode.Syntax, boundNode.WasCompilerGenerated);
	}

	private IOperation CreateBoundNegatedPatternOperation(BoundNegatedPattern boundNegatedPattern)
	{
		return new NegatedPatternOperation((IPatternOperation)Create(boundNegatedPattern.Negated), boundNegatedPattern.InputType.GetPublicSymbol(), boundNegatedPattern.NarrowedType.GetPublicSymbol(), _semanticModel, boundNegatedPattern.Syntax, boundNegatedPattern.WasCompilerGenerated);
	}

	private IOperation CreateBoundBinaryPatternOperation(BoundBinaryPattern boundBinaryPattern)
	{
		if (!(boundBinaryPattern.Left is BoundBinaryPattern))
		{
			return createOperation(this, boundBinaryPattern, (IPatternOperation)Create(boundBinaryPattern.Left));
		}
		ArrayBuilder<BoundBinaryPattern> instance = ArrayBuilder<BoundBinaryPattern>.GetInstance();
		BoundBinaryPattern boundBinaryPattern2 = boundBinaryPattern;
		do
		{
			instance.Push(boundBinaryPattern2);
			boundBinaryPattern2 = boundBinaryPattern2.Left as BoundBinaryPattern;
		}
		while (boundBinaryPattern2 != null);
		boundBinaryPattern2 = instance.Pop();
		IPatternOperation patternOperation = (IPatternOperation)Create(boundBinaryPattern2.Left);
		do
		{
			patternOperation = createOperation(this, boundBinaryPattern2, patternOperation);
		}
		while (instance.TryPop(out boundBinaryPattern2));
		instance.Free();
		return patternOperation;
		static BinaryPatternOperation createOperation(CSharpOperationFactory @this, BoundBinaryPattern boundBinaryPattern3, IPatternOperation left)
		{
			return new BinaryPatternOperation(boundBinaryPattern3.Disjunction ? BinaryOperatorKind.Or : BinaryOperatorKind.And, left, (IPatternOperation)@this.Create(boundBinaryPattern3.Right), boundBinaryPattern3.InputType.GetPublicSymbol(), boundBinaryPattern3.NarrowedType.GetPublicSymbol(), @this._semanticModel, boundBinaryPattern3.Syntax, boundBinaryPattern3.WasCompilerGenerated);
		}
	}

	private ISwitchOperation CreateBoundSwitchStatementOperation(BoundSwitchStatement boundSwitchStatement)
	{
		IOperation value = Create(boundSwitchStatement.Expression);
		ImmutableArray<ISwitchCaseOperation> cases = CreateFromArray<BoundSwitchSection, ISwitchCaseOperation>(boundSwitchStatement.SwitchSections);
		return new SwitchOperation(boundSwitchStatement.InnerLocals.GetPublicSymbols(), exitLabel: boundSwitchStatement.BreakLabel.GetPublicSymbol(), syntax: boundSwitchStatement.Syntax, isImplicit: boundSwitchStatement.WasCompilerGenerated, value: value, cases: cases, semanticModel: _semanticModel);
	}

	private ISwitchCaseOperation CreateBoundSwitchSectionOperation(BoundSwitchSection boundSwitchSection)
	{
		ImmutableArray<ICaseClauseOperation> clauses = CreateFromArray<BoundSwitchLabel, ICaseClauseOperation>(boundSwitchSection.SwitchLabels);
		ImmutableArray<IOperation> body = CreateFromArray<BoundStatement, IOperation>(boundSwitchSection.Statements);
		ImmutableArray<ILocalSymbol> publicSymbols = boundSwitchSection.Locals.GetPublicSymbols();
		return new SwitchCaseOperation(clauses, body, publicSymbols, null, _semanticModel, boundSwitchSection.Syntax, boundSwitchSection.WasCompilerGenerated);
	}

	private ISwitchExpressionOperation CreateBoundSwitchExpressionOperation(BoundConvertedSwitchExpression boundSwitchExpression)
	{
		IOperation? value = Create(boundSwitchExpression.Expression);
		ImmutableArray<ISwitchExpressionArmOperation> arms = CreateFromArray<BoundSwitchExpressionArm, ISwitchExpressionArmOperation>(boundSwitchExpression.SwitchArms);
		bool isExhaustive = !(boundSwitchExpression.DefaultLabel != null);
		return new SwitchExpressionOperation(value, arms, isExhaustive, _semanticModel, boundSwitchExpression.Syntax, boundSwitchExpression.GetPublicTypeSymbol(), boundSwitchExpression.WasCompilerGenerated);
	}

	private ISwitchExpressionArmOperation CreateBoundSwitchExpressionArmOperation(BoundSwitchExpressionArm boundSwitchExpressionArm)
	{
		IPatternOperation pattern = (IPatternOperation)Create(boundSwitchExpressionArm.Pattern);
		IOperation guard = Create(boundSwitchExpressionArm.WhenClause);
		IOperation value = Create(boundSwitchExpressionArm.Value);
		return new SwitchExpressionArmOperation(pattern, guard, value, boundSwitchExpressionArm.Locals.GetPublicSymbols(), _semanticModel, boundSwitchExpressionArm.Syntax, boundSwitchExpressionArm.WasCompilerGenerated);
	}

	private ICaseClauseOperation CreateBoundSwitchLabelOperation(BoundSwitchLabel boundSwitchLabel)
	{
		SyntaxNode syntax = boundSwitchLabel.Syntax;
		bool wasCompilerGenerated = boundSwitchLabel.WasCompilerGenerated;
		LabelSymbol label = boundSwitchLabel.Label;
		if (boundSwitchLabel.Syntax.Kind() == SyntaxKind.DefaultSwitchLabel)
		{
			return new DefaultCaseClauseOperation(label.GetPublicSymbol(), _semanticModel, syntax, wasCompilerGenerated);
		}
		if (boundSwitchLabel.WhenClause == null && boundSwitchLabel.Pattern.Kind == BoundKind.ConstantPattern && boundSwitchLabel.Pattern is BoundConstantPattern boundConstantPattern && boundConstantPattern.InputType.IsValidV6SwitchGoverningType())
		{
			return new SingleValueCaseClauseOperation(Create(boundConstantPattern.Value), label.GetPublicSymbol(), _semanticModel, syntax, wasCompilerGenerated);
		}
		IPatternOperation pattern = (IPatternOperation)Create(boundSwitchLabel.Pattern);
		IOperation guard = Create(boundSwitchLabel.WhenClause);
		return new PatternCaseClauseOperation(label.GetPublicSymbol(), pattern, guard, _semanticModel, syntax, wasCompilerGenerated);
	}

	private IIsPatternOperation CreateBoundIsPatternExpressionOperation(BoundIsPatternExpression boundIsPatternExpression)
	{
		return new IsPatternOperation(Create(boundIsPatternExpression.Expression), (IPatternOperation)Create(boundIsPatternExpression.Pattern), syntax: boundIsPatternExpression.Syntax, type: boundIsPatternExpression.GetPublicTypeSymbol(), isImplicit: boundIsPatternExpression.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IOperation CreateBoundQueryClauseOperation(BoundQueryClause boundQueryClause)
	{
		if (boundQueryClause.Syntax.Kind() != SyntaxKind.QueryExpression)
		{
			return Create(boundQueryClause.Value);
		}
		return new TranslatedQueryOperation(Create(boundQueryClause.Value), syntax: boundQueryClause.Syntax, type: boundQueryClause.GetPublicTypeSymbol(), isImplicit: boundQueryClause.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IOperation CreateBoundRangeVariableOperation(BoundRangeVariable boundRangeVariable)
	{
		return Create(boundRangeVariable.Value);
	}

	private IOperation CreateBoundDiscardExpressionOperation(BoundDiscardExpression boundNode)
	{
		return new DiscardOperation(((DiscardSymbol)boundNode.ExpressionSymbol).GetPublicSymbol(), _semanticModel, boundNode.Syntax, boundNode.GetPublicTypeSymbol(), boundNode.WasCompilerGenerated);
	}

	private IOperation CreateFromEndIndexExpressionOperation(BoundFromEndIndexExpression boundIndex)
	{
		return new UnaryOperation(UnaryOperatorKind.Hat, Create(boundIndex.Operand), boundIndex.Type.IsNullableType(), isChecked: false, null, null, _semanticModel, boundIndex.Syntax, boundIndex.GetPublicTypeSymbol(), null, boundIndex.WasCompilerGenerated);
	}

	private IOperation CreateRangeExpressionOperation(BoundRangeExpression boundRange)
	{
		IOperation? leftOperand = Create(boundRange.LeftOperandOpt);
		IOperation rightOperand = Create(boundRange.RightOperandOpt);
		return new RangeOperation(leftOperand, rightOperand, boundRange.Type.IsNullableType(), boundRange.MethodOpt.GetPublicSymbol(), _semanticModel, boundRange.Syntax, boundRange.GetPublicTypeSymbol(), boundRange.WasCompilerGenerated);
	}

	private IOperation CreateBoundDiscardPatternOperation(BoundDiscardPattern boundNode)
	{
		return new DiscardPatternOperation(boundNode.InputType.GetPublicSymbol(), boundNode.NarrowedType.GetPublicSymbol(), _semanticModel, boundNode.Syntax, boundNode.WasCompilerGenerated);
	}

	internal IPropertySubpatternOperation CreatePropertySubpattern(BoundPropertySubpattern subpattern, ITypeSymbol matchedType)
	{
		SyntaxNode subpatternSyntax = subpattern.Syntax;
		BoundPropertySubpatternMember boundPropertySubpatternMember = subpattern.Member;
		IPatternOperation pattern = (IPatternOperation)Create(subpattern.Pattern);
		if (boundPropertySubpatternMember == null)
		{
			return new PropertySubpatternOperation(OperationFactory.CreateInvalidOperation(_semanticModel, subpatternSyntax, ImmutableArray<IOperation>.Empty, isImplicit: true), pattern, _semanticModel, subpatternSyntax, isImplicit: false);
		}
		SyntaxNode syntax = boundPropertySubpatternMember.Syntax;
		ITypeSymbol typeSymbol = getInputType(boundPropertySubpatternMember, matchedType);
		IPropertySubpatternOperation propertySubpatternOperation = createPropertySubpattern(boundPropertySubpatternMember.Symbol, pattern, typeSymbol, syntax, boundPropertySubpatternMember.Receiver == null);
		while (boundPropertySubpatternMember.Receiver != null)
		{
			boundPropertySubpatternMember = boundPropertySubpatternMember.Receiver;
			syntax = boundPropertySubpatternMember.Syntax;
			ITypeSymbol typeSymbol2 = typeSymbol;
			typeSymbol = getInputType(boundPropertySubpatternMember, matchedType);
			IPatternOperation pattern2 = new RecursivePatternOperation(typeSymbol2, null, ImmutableArray<IPatternOperation>.Empty, ImmutableArray.Create(propertySubpatternOperation), null, typeSymbol2, typeSymbol2, _semanticModel, syntax, isImplicit: true);
			propertySubpatternOperation = createPropertySubpattern(boundPropertySubpatternMember.Symbol, pattern2, typeSymbol, syntax, isSingle: false);
		}
		return propertySubpatternOperation;
		IPropertySubpatternOperation createPropertySubpattern(Symbol? symbol, IPatternOperation pattern3, ITypeSymbol receiverType, SyntaxNode nameSyntax, bool isSingle)
		{
			IOperation member;
			if (!(symbol is FieldSymbol fieldSymbol))
			{
				member = ((!(symbol is PropertySymbol propertySymbol)) ? ((IOperation)OperationFactory.CreateInvalidOperation(_semanticModel, nameSyntax, ImmutableArray<IOperation>.Empty, isImplicit: false)) : ((IOperation)new PropertyReferenceOperation(propertySymbol.GetPublicSymbol(), null, ImmutableArray<IArgumentOperation>.Empty, createReceiver(), _semanticModel, nameSyntax, propertySymbol.Type.GetPublicSymbol(), isImplicit: false)));
			}
			else
			{
				ConstantValue constantValue = fieldSymbol.GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false);
				member = new FieldReferenceOperation(fieldSymbol.GetPublicSymbol(), isDeclaration: false, createReceiver(), _semanticModel, nameSyntax, fieldSymbol.Type.GetPublicSymbol(), constantValue, isImplicit: false);
			}
			SyntaxNode syntax2 = (isSingle ? subpatternSyntax : nameSyntax);
			return new PropertySubpatternOperation(member, pattern3, _semanticModel, syntax2, !isSingle);
			IOperation? createReceiver()
			{
				Symbol? symbol2 = symbol;
				if ((object)symbol2 == null || symbol2.IsStatic)
				{
					return null;
				}
				return new InstanceReferenceOperation(InstanceReferenceKind.PatternInput, _semanticModel, nameSyntax, receiverType, isImplicit: true);
			}
		}
		static ITypeSymbol getInputType(BoundPropertySubpatternMember member, ITypeSymbol typeSymbol3)
		{
			return member.Receiver?.Type.StrippedType().GetPublicSymbol() ?? typeSymbol3;
		}
	}

	private IInstanceReferenceOperation CreateCollectionValuePlaceholderOperation(BoundObjectOrCollectionValuePlaceholder placeholder)
	{
		return new InstanceReferenceOperation(InstanceReferenceKind.ImplicitReceiver, syntax: placeholder.Syntax, type: placeholder.GetPublicTypeSymbol(), isImplicit: placeholder.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private ImmutableArray<IArgumentOperation> CreateDisposeArguments(MethodArgumentInfo patternDisposeInfo)
	{
		if (patternDisposeInfo.Method.ParameterCount == 0)
		{
			return ImmutableArray<IArgumentOperation>.Empty;
		}
		return Operation.SetParentOperation(DeriveArguments(patternDisposeInfo.Method, patternDisposeInfo.Arguments, default(ImmutableArray<int>), patternDisposeInfo.DefaultArguments), null);
	}

	internal ImmutableArray<BoundStatement> ToStatements(BoundStatement? statement)
	{
		if (statement == null)
		{
			return ImmutableArray<BoundStatement>.Empty;
		}
		if (statement.Kind == BoundKind.StatementList)
		{
			return ((BoundStatementList)statement).Statements;
		}
		return ImmutableArray.Create(statement);
	}

	private IInstanceReferenceOperation CreateImplicitReceiver(SyntaxNode syntax, TypeSymbol type)
	{
		return new InstanceReferenceOperation(InstanceReferenceKind.ImplicitReceiver, _semanticModel, syntax, type.GetPublicSymbol(), isImplicit: true);
	}

	internal IArgumentOperation CreateArgumentOperation(ArgumentKind kind, IParameterSymbol? parameter, BoundExpression expression)
	{
		IOperation operation = Create((expression is BoundConversion { IsParamsArrayOrCollection: not false } boundConversion) ? boundConversion.Operand : expression);
		SyntaxNode syntax = expression.Syntax;
		bool flag;
		if (syntax != null)
		{
			SyntaxNode parent = syntax.Parent;
			if (parent is ArgumentSyntax || parent is AttributeArgumentSyntax)
			{
				flag = true;
				goto IL_0051;
			}
		}
		flag = false;
		goto IL_0051;
		IL_0051:
		bool isImplicit;
		SyntaxNode syntax3;
		if (!flag)
		{
			SyntaxNode syntax2 = operation.Syntax;
			isImplicit = true;
			syntax3 = syntax2;
		}
		else
		{
			SyntaxNode? parent2 = expression.Syntax.Parent;
			bool wasCompilerGenerated = expression.WasCompilerGenerated;
			isImplicit = wasCompilerGenerated;
			syntax3 = parent2;
		}
		return new ArgumentOperation(kind, parameter, operation, OperationFactory.IdentityConversion, OperationFactory.IdentityConversion, _semanticModel, syntax3, isImplicit);
	}

	internal IVariableInitializerOperation? CreateVariableDeclaratorInitializer(BoundLocalDeclaration boundLocalDeclaration, SyntaxNode syntax)
	{
		if (boundLocalDeclaration.InitializerOpt != null)
		{
			SyntaxNode syntaxNode = null;
			bool isImplicit = false;
			if (syntax is VariableDeclaratorSyntax variableDeclaratorSyntax)
			{
				syntaxNode = variableDeclaratorSyntax.Initializer;
			}
			if (syntaxNode == null)
			{
				syntaxNode = boundLocalDeclaration.InitializerOpt.Syntax;
				isImplicit = true;
			}
			IOperation value = Create(boundLocalDeclaration.InitializerOpt);
			return new VariableInitializerOperation(ImmutableArray<ILocalSymbol>.Empty, value, _semanticModel, syntaxNode, isImplicit);
		}
		return null;
	}

	private IVariableDeclaratorOperation CreateVariableDeclaratorInternal(BoundLocalDeclaration boundLocalDeclaration, SyntaxNode syntax)
	{
		ILocalSymbol? publicSymbol = boundLocalDeclaration.LocalSymbol.GetPublicSymbol();
		bool isImplicit = false;
		IVariableInitializerOperation initializer = CreateVariableDeclaratorInitializer(boundLocalDeclaration, syntax);
		ImmutableArray<IOperation> ignoredArguments = CreateFromArray<BoundExpression, IOperation>(boundLocalDeclaration.ArgumentsOpt);
		return new VariableDeclaratorOperation(publicSymbol, initializer, ignoredArguments, _semanticModel, syntax, isImplicit);
	}

	[return: NotNullIfNotNull("boundLocal")]
	internal IVariableDeclaratorOperation? CreateVariableDeclarator(BoundLocal? boundLocal)
	{
		if (boundLocal != null)
		{
			return new VariableDeclaratorOperation(boundLocal.LocalSymbol.GetPublicSymbol(), null, ImmutableArray<IOperation>.Empty, _semanticModel, boundLocal.Syntax, isImplicit: false);
		}
		return null;
	}

	internal IOperation? CreateReceiverOperation(BoundNode? instance, Symbol? symbol)
	{
		if (instance == null || instance.Kind == BoundKind.TypeExpression)
		{
			return null;
		}
		if (symbol != null && symbol.IsStatic && instance.WasCompilerGenerated && instance.Kind == BoundKind.ThisReference)
		{
			return null;
		}
		return Create(instance);
	}

	private bool IsCallVirtual(MethodSymbol? targetMethod, BoundExpression? receiver)
	{
		if ((object)targetMethod != null && receiver != null && (targetMethod.IsVirtual || targetMethod.IsAbstract || targetMethod.IsOverride))
		{
			return !receiver.SuppressVirtualCalls;
		}
		return false;
	}

	private bool IsMethodInvalid(LookupResultKind resultKind, MethodSymbol targetMethod)
	{
		if (resultKind != LookupResultKind.OverloadResolutionFailure)
		{
			return targetMethod?.OriginalDefinition is ErrorMethodSymbol;
		}
		return true;
	}

	internal IEventReferenceOperation CreateBoundEventAccessOperation(BoundEventAssignmentOperator boundEventAssignmentOperator)
	{
		SyntaxNode syntax = boundEventAssignmentOperator.Syntax;
		IEventSymbol publicSymbol = boundEventAssignmentOperator.Event.GetPublicSymbol();
		IOperation instance = CreateReceiverOperation(boundEventAssignmentOperator.ReceiverOpt, boundEventAssignmentOperator.Event);
		SyntaxNode left = ((AssignmentExpressionSyntax)syntax).Left;
		bool wasCompilerGenerated = boundEventAssignmentOperator.WasCompilerGenerated;
		TypeParameterSymbol constrainedToType = GetConstrainedToType(boundEventAssignmentOperator.Event, boundEventAssignmentOperator.ReceiverOpt);
		return new EventReferenceOperation(publicSymbol, constrainedToType.GetPublicSymbol(), instance, _semanticModel, left, publicSymbol.Type, wasCompilerGenerated);
	}

	internal IOperation CreateDelegateTargetOperation(BoundNode delegateNode)
	{
		if (delegateNode is BoundConversion boundConversion)
		{
			if (boundConversion.ConversionKind == ConversionKind.MethodGroup)
			{
				return CreateBoundMethodGroupSingleMethodOperation((BoundMethodGroup)boundConversion.Operand, boundConversion.SymbolOpt, boundConversion.SuppressVirtualCalls);
			}
			return Create(boundConversion.Operand);
		}
		BoundDelegateCreationExpression boundDelegateCreationExpression = (BoundDelegateCreationExpression)delegateNode;
		if (boundDelegateCreationExpression.Argument.Kind == BoundKind.MethodGroup && boundDelegateCreationExpression.MethodOpt != null)
		{
			BoundMethodGroup boundMethodGroup = (BoundMethodGroup)boundDelegateCreationExpression.Argument;
			return CreateBoundMethodGroupSingleMethodOperation(boundMethodGroup, boundDelegateCreationExpression.MethodOpt, boundMethodGroup.SuppressVirtualCalls);
		}
		return Create(boundDelegateCreationExpression.Argument);
	}

	internal IOperation CreateMemberInitializerInitializedMember(BoundNode initializedMember)
	{
		if (!(initializedMember is BoundObjectInitializerMember boundObjectInitializerMember))
		{
			if (initializedMember is BoundDynamicObjectInitializerMember boundDynamicObjectInitializerMember)
			{
				return CreateBoundDynamicObjectInitializerMemberOperation(boundDynamicObjectInitializerMember);
			}
			return Create(initializedMember);
		}
		return CreateBoundObjectInitializerMemberOperation(boundObjectInitializerMember, isObjectOrCollectionInitializer: true);
	}

	internal ImmutableArray<IArgumentOperation> DeriveArguments(BoundNode containingExpression)
	{
		switch (containingExpression.Kind)
		{
		case BoundKind.ObjectInitializerMember:
		{
			BoundObjectInitializerMember boundObjectInitializerMember = (BoundObjectInitializerMember)containingExpression;
			PropertySymbol methodOrIndexer = (PropertySymbol)boundObjectInitializerMember.MemberSymbol;
			return DeriveArguments(methodOrIndexer, boundObjectInitializerMember.Arguments, boundObjectInitializerMember.ArgsToParamsOpt, boundObjectInitializerMember.DefaultArguments);
		}
		case BoundKind.IndexerAccess:
		{
			BoundIndexerAccess boundIndexerAccess = (BoundIndexerAccess)containingExpression;
			return DeriveArguments(boundIndexerAccess.Indexer, boundIndexerAccess.Arguments, boundIndexerAccess.ArgsToParamsOpt, boundIndexerAccess.DefaultArguments);
		}
		case BoundKind.ObjectCreationExpression:
		{
			BoundObjectCreationExpression boundObjectCreationExpression = (BoundObjectCreationExpression)containingExpression;
			return DeriveArguments(boundObjectCreationExpression.Constructor, boundObjectCreationExpression.Arguments, boundObjectCreationExpression.ArgsToParamsOpt, boundObjectCreationExpression.DefaultArguments);
		}
		case BoundKind.Attribute:
		{
			BoundAttribute boundAttribute = (BoundAttribute)containingExpression;
			return DeriveArguments(boundAttribute.Constructor, boundAttribute.ConstructorArguments, boundAttribute.ConstructorArgumentsToParamsOpt, boundAttribute.ConstructorDefaultArguments);
		}
		case BoundKind.Call:
		{
			BoundCall boundCall = (BoundCall)containingExpression;
			return DeriveArguments(boundCall.Method, boundCall.Arguments, boundCall.ArgsToParamsOpt, boundCall.DefaultArguments, boundCall.InvokedAsExtensionMethod);
		}
		case BoundKind.CollectionElementInitializer:
		{
			BoundCollectionElementInitializer boundCollectionElementInitializer = (BoundCollectionElementInitializer)containingExpression;
			return DeriveArguments(boundCollectionElementInitializer.AddMethod, boundCollectionElementInitializer.Arguments, boundCollectionElementInitializer.ArgsToParamsOpt, boundCollectionElementInitializer.DefaultArguments, boundCollectionElementInitializer.InvokedAsExtensionMethod);
		}
		case BoundKind.FunctionPointerInvocation:
		{
			BoundFunctionPointerInvocation boundFunctionPointerInvocation = (BoundFunctionPointerInvocation)containingExpression;
			return DeriveArguments(boundFunctionPointerInvocation.FunctionPointer.Signature, boundFunctionPointerInvocation.Arguments, default(ImmutableArray<int>), BitVector.Empty);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(containingExpression.Kind);
		}
	}

	private ImmutableArray<IArgumentOperation> DeriveArguments(Symbol methodOrIndexer, ImmutableArray<BoundExpression> boundArguments, ImmutableArray<int> argumentsToParametersOpt, BitVector defaultArguments, bool invokedAsExtensionMethod = false)
	{
		if (methodOrIndexer.GetParameters().IsDefaultOrEmpty && boundArguments.IsDefaultOrEmpty)
		{
			return ImmutableArray<IArgumentOperation>.Empty;
		}
		return MakeArgumentsInEvaluationOrder(this, boundArguments, methodOrIndexer, argumentsToParametersOpt, defaultArguments, invokedAsExtensionMethod);
	}

	private static ImmutableArray<IArgumentOperation> MakeArgumentsInEvaluationOrder(CSharpOperationFactory operationFactory, ImmutableArray<BoundExpression> arguments, Symbol methodOrIndexer, ImmutableArray<int> argsToParamsOpt, BitVector defaultArguments, bool invokedAsExtensionMethod)
	{
		if (LocalRewriter.CanSkipRewriting(arguments, methodOrIndexer, argsToParamsOpt, invokedAsExtensionMethod, ignoreComReceiver: true, out var _))
		{
			ImmutableArray<ParameterSymbol> parameters = methodOrIndexer.GetParameters();
			ArrayBuilder<IArgumentOperation> instance = ArrayBuilder<IArgumentOperation>.GetInstance(arguments.Length);
			int i;
			for (i = 0; i < parameters.Length; i++)
			{
				ArgumentKind argumentKind = GetArgumentKind(arguments[i], ref defaultArguments, i);
				instance.Add(operationFactory.CreateArgumentOperation(argumentKind, parameters[i].GetPublicSymbol(), arguments[i]));
			}
			for (; i < arguments.Length; i++)
			{
				ArgumentKind kind = ((!defaultArguments[i]) ? ArgumentKind.Explicit : ArgumentKind.DefaultValue);
				instance.Add(operationFactory.CreateArgumentOperation(kind, null, arguments[i]));
			}
			return instance.ToImmutableAndFree();
		}
		return BuildArgumentsInEvaluationOrder(operationFactory, methodOrIndexer, argsToParamsOpt, defaultArguments, arguments);
	}

	private static ArgumentKind GetArgumentKind(BoundExpression argument, ref BitVector defaultArguments, int i)
	{
		if (defaultArguments[i])
		{
			return ArgumentKind.DefaultValue;
		}
		if (argument.IsParamsArrayOrCollection)
		{
			TypeSymbol? type = argument.Type;
			return ((object)type != null && type.IsSZArray()) ? ArgumentKind.ParamArray : ArgumentKind.ParamCollection;
		}
		return ArgumentKind.Explicit;
	}

	private static ImmutableArray<IArgumentOperation> BuildArgumentsInEvaluationOrder(CSharpOperationFactory operationFactory, Symbol methodOrIndexer, ImmutableArray<int> argsToParamsOpt, BitVector defaultArguments, ImmutableArray<BoundExpression> arguments)
	{
		ImmutableArray<ParameterSymbol> parameters = methodOrIndexer.GetParameters();
		ArrayBuilder<IArgumentOperation> instance = ArrayBuilder<IArgumentOperation>.GetInstance(parameters.Length);
		for (int i = 0; i < arguments.Length; i++)
		{
			BoundExpression boundExpression = arguments[i];
			int index = ((!argsToParamsOpt.IsDefault) ? argsToParamsOpt[i] : i);
			ParameterSymbol symbol = parameters[index];
			ArgumentKind argumentKind = GetArgumentKind(boundExpression, ref defaultArguments, i);
			instance.Add(operationFactory.CreateArgumentOperation(argumentKind, symbol.GetPublicSymbol(), boundExpression));
		}
		return instance.ToImmutableAndFree();
	}

	internal static ImmutableArray<BoundNode> CreateInvalidChildrenFromArgumentsExpression(BoundNode? receiverOpt, ImmutableArray<BoundExpression> arguments, BoundExpression? additionalNodeOpt = null)
	{
		ArrayBuilder<BoundNode> instance = ArrayBuilder<BoundNode>.GetInstance();
		if (receiverOpt != null && (!receiverOpt.WasCompilerGenerated || (receiverOpt.Kind != BoundKind.ThisReference && receiverOpt.Kind != BoundKind.BaseReference && receiverOpt.Kind != BoundKind.ObjectOrCollectionValuePlaceholder)))
		{
			instance.Add(receiverOpt);
		}
		instance.AddRange(StaticCast<BoundNode>.From(arguments));
		instance.AddIfNotNull(additionalNodeOpt);
		return instance.ToImmutableAndFree();
	}

	internal ImmutableArray<IOperation> GetAnonymousObjectCreationInitializers(ImmutableArray<BoundExpression> arguments, ImmutableArray<BoundAnonymousPropertyDeclaration> declarations, SyntaxNode syntax, ITypeSymbol type, bool isImplicit)
	{
		ArrayBuilder<IOperation> instance = ArrayBuilder<IOperation>.GetInstance(arguments.Length);
		int currentDeclarationIndex = 0;
		for (int i = 0; i < arguments.Length; i++)
		{
			IOperation operation = Create(arguments[i]);
			InstanceReferenceOperation instance2 = new InstanceReferenceOperation(InstanceReferenceKind.ImplicitReceiver, _semanticModel, syntax, type, isImplicit: true);
			PropertySymbol anonymousTypeProperty = AnonymousTypeManager.GetAnonymousTypeProperty(type.GetSymbol<NamedTypeSymbol>(), i);
			BoundAnonymousPropertyDeclaration boundAnonymousPropertyDeclaration = getDeclaration(declarations, anonymousTypeProperty, ref currentDeclarationIndex);
			IOperation operation2;
			bool isImplicit2;
			if (boundAnonymousPropertyDeclaration == null)
			{
				operation2 = new PropertyReferenceOperation(anonymousTypeProperty.GetPublicSymbol(), null, ImmutableArray<IArgumentOperation>.Empty, instance2, _semanticModel, operation.Syntax, anonymousTypeProperty.Type.GetPublicSymbol(), isImplicit: true);
				isImplicit2 = true;
			}
			else
			{
				operation2 = new PropertyReferenceOperation(boundAnonymousPropertyDeclaration.Property.GetPublicSymbol(), null, ImmutableArray<IArgumentOperation>.Empty, instance2, _semanticModel, boundAnonymousPropertyDeclaration.Syntax, boundAnonymousPropertyDeclaration.GetPublicTypeSymbol(), boundAnonymousPropertyDeclaration.WasCompilerGenerated);
				isImplicit2 = isImplicit;
			}
			SyntaxNode syntax2 = operation.Syntax?.Parent ?? syntax;
			ITypeSymbol type2 = operation2.Type;
			SimpleAssignmentOperation item = new SimpleAssignmentOperation(isRef: false, operation2, operation, _semanticModel, syntax2, type2, operation.GetConstantValue(), isImplicit2);
			instance.Add(item);
		}
		return instance.ToImmutableAndFree();
		static BoundAnonymousPropertyDeclaration? getDeclaration(ImmutableArray<BoundAnonymousPropertyDeclaration> immutableArray, PropertySymbol currentProperty, ref int reference)
		{
			if (reference >= immutableArray.Length)
			{
				return null;
			}
			BoundAnonymousPropertyDeclaration boundAnonymousPropertyDeclaration2 = immutableArray[reference];
			if (currentProperty.MemberIndexOpt == boundAnonymousPropertyDeclaration2.Property.MemberIndexOpt)
			{
				reference++;
				return boundAnonymousPropertyDeclaration2;
			}
			return null;
		}
	}
}
