using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundTreeRewriter : BoundTreeVisitor
{
	[return: NotNullIfNotNull("type")]
	public virtual TypeSymbol? VisitType(TypeSymbol? type)
	{
		return type;
	}

	public ImmutableArray<T> VisitList<T>(ImmutableArray<T> list) where T : BoundNode
	{
		if (list.IsDefault)
		{
			return list;
		}
		return DoVisitList(list);
	}

	private ImmutableArray<T> DoVisitList<T>(ImmutableArray<T> list) where T : BoundNode
	{
		ArrayBuilder<T> arrayBuilder = null;
		for (int i = 0; i < list.Length; i++)
		{
			T val = list[i];
			BoundNode boundNode = Visit(val);
			if (arrayBuilder == null && val != boundNode)
			{
				arrayBuilder = ArrayBuilder<T>.GetInstance();
				if (i > 0)
				{
					arrayBuilder.AddRange(list, i);
				}
			}
			if (arrayBuilder != null && boundNode != null)
			{
				arrayBuilder.Add((T)boundNode);
			}
		}
		return arrayBuilder?.ToImmutableAndFree() ?? list;
	}

	[return: NotNullIfNotNull("symbol")]
	public virtual AliasSymbol? VisitAliasSymbol(AliasSymbol? symbol)
	{
		return symbol;
	}

	public virtual DiscardSymbol VisitDiscardSymbol(DiscardSymbol symbol)
	{
		return symbol;
	}

	public virtual EventSymbol VisitEventSymbol(EventSymbol symbol)
	{
		return symbol;
	}

	[return: NotNullIfNotNull("symbol")]
	public virtual LabelSymbol? VisitLabelSymbol(LabelSymbol? symbol)
	{
		return symbol;
	}

	public virtual LocalSymbol VisitLocalSymbol(LocalSymbol symbol)
	{
		return symbol;
	}

	public virtual NamespaceSymbol VisitNamespaceSymbol(NamespaceSymbol symbol)
	{
		return symbol;
	}

	[return: NotNullIfNotNull("symbol")]
	public virtual RangeVariableSymbol? VisitRangeVariableSymbol(RangeVariableSymbol? symbol)
	{
		return symbol;
	}

	[return: NotNullIfNotNull("symbol")]
	public virtual FieldSymbol? VisitFieldSymbol(FieldSymbol? symbol)
	{
		return symbol;
	}

	public virtual ParameterSymbol VisitParameterSymbol(ParameterSymbol symbol)
	{
		return symbol;
	}

	[return: NotNullIfNotNull("symbol")]
	public virtual PropertySymbol? VisitPropertySymbol(PropertySymbol? symbol)
	{
		return symbol;
	}

	[return: NotNullIfNotNull("symbol")]
	public virtual MethodSymbol? VisitMethodSymbol(MethodSymbol? symbol)
	{
		return symbol;
	}

	[return: NotNullIfNotNull("symbol")]
	public Symbol? VisitSymbol(Symbol? symbol)
	{
		if ((object)symbol == null)
		{
			return null;
		}
		switch (symbol.Kind)
		{
		case SymbolKind.Alias:
			return VisitAliasSymbol((AliasSymbol)symbol);
		case SymbolKind.Discard:
			return VisitDiscardSymbol((DiscardSymbol)symbol);
		case SymbolKind.Event:
			return VisitEventSymbol((EventSymbol)symbol);
		case SymbolKind.Label:
			return VisitLabelSymbol((LabelSymbol)symbol);
		case SymbolKind.Local:
			return VisitLocalSymbol((LocalSymbol)symbol);
		case SymbolKind.Namespace:
			return VisitNamespaceSymbol((NamespaceSymbol)symbol);
		case SymbolKind.RangeVariable:
			return VisitRangeVariableSymbol((RangeVariableSymbol)symbol);
		case SymbolKind.Field:
			return VisitFieldSymbol((FieldSymbol)symbol);
		case SymbolKind.Parameter:
			return VisitParameterSymbol((ParameterSymbol)symbol);
		case SymbolKind.Property:
			return VisitPropertySymbol((PropertySymbol)symbol);
		case SymbolKind.Method:
			return VisitMethodSymbol((MethodSymbol)symbol);
		default:
			if (symbol is TypeSymbol type)
			{
				return VisitType(type);
			}
			throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
		}
	}

	[return: NotNullIfNotNull("symbol")]
	protected FunctionTypeSymbol? VisitFunctionTypeSymbol(FunctionTypeSymbol? symbol)
	{
		return (FunctionTypeSymbol)VisitType(symbol);
	}

	public ImmutableArray<T> VisitSymbols<T>(ImmutableArray<T> symbols) where T : Symbol?
	{
		if (symbols.IsDefault)
		{
			return symbols;
		}
		ArrayBuilder<T> arrayBuilder = null;
		for (int i = 0; i < symbols.Length; i++)
		{
			T val = symbols[i];
			T val2 = (T)VisitSymbol(val);
			if ((object)val2 != val)
			{
				if (arrayBuilder == null)
				{
					arrayBuilder = ArrayBuilder<T>.GetInstance(symbols.Length);
					arrayBuilder.AddRange(symbols, i);
				}
				arrayBuilder.Add(val2);
			}
			else
			{
				arrayBuilder?.Add(val);
			}
		}
		return arrayBuilder?.ToImmutableAndFree() ?? symbols;
	}

	protected virtual ImmutableArray<LocalSymbol> VisitLocals(ImmutableArray<LocalSymbol> locals)
	{
		return locals;
	}

	protected virtual ImmutableArray<MethodSymbol> VisitDeclaredLocalFunctions(ImmutableArray<MethodSymbol> localFunctions)
	{
		return localFunctions;
	}

	public override BoundNode? VisitFieldEqualsValue(BoundFieldEqualsValue node)
	{
		FieldSymbol field = VisitFieldSymbol(node.Field);
		ImmutableArray<LocalSymbol> locals = VisitLocals(node.Locals);
		BoundExpression value = (BoundExpression)Visit(node.Value);
		return node.Update(field, locals, value);
	}

	public override BoundNode? VisitPropertyEqualsValue(BoundPropertyEqualsValue node)
	{
		PropertySymbol property = VisitPropertySymbol(node.Property);
		ImmutableArray<LocalSymbol> locals = VisitLocals(node.Locals);
		BoundExpression value = (BoundExpression)Visit(node.Value);
		return node.Update(property, locals, value);
	}

	public override BoundNode? VisitParameterEqualsValue(BoundParameterEqualsValue node)
	{
		ParameterSymbol parameter = VisitParameterSymbol(node.Parameter);
		ImmutableArray<LocalSymbol> locals = VisitLocals(node.Locals);
		BoundExpression value = (BoundExpression)Visit(node.Value);
		return node.Update(parameter, locals, value);
	}

	public override BoundNode? VisitGlobalStatementInitializer(BoundGlobalStatementInitializer node)
	{
		BoundStatement statement = (BoundStatement)Visit(node.Statement);
		return node.Update(statement);
	}

	public override BoundNode? VisitValuePlaceholder(BoundValuePlaceholder node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitCapturedReceiverPlaceholder(BoundCapturedReceiverPlaceholder node)
	{
		BoundExpression receiver = (BoundExpression)Visit(node.Receiver);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiver, type);
	}

	public override BoundNode? VisitDeconstructValuePlaceholder(BoundDeconstructValuePlaceholder node)
	{
		Symbol variableSymbol = VisitSymbol(node.VariableSymbol);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(variableSymbol, node.IsDiscardExpression, type);
	}

	public override BoundNode? VisitTupleOperandPlaceholder(BoundTupleOperandPlaceholder node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitAwaitableValuePlaceholder(BoundAwaitableValuePlaceholder node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitDisposableValuePlaceholder(BoundDisposableValuePlaceholder node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitObjectOrCollectionValuePlaceholder(BoundObjectOrCollectionValuePlaceholder node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.IsNewInstance, type);
	}

	public override BoundNode? VisitImplicitIndexerValuePlaceholder(BoundImplicitIndexerValuePlaceholder node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitImplicitIndexerReceiverPlaceholder(BoundImplicitIndexerReceiverPlaceholder node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.IsEquivalentToThisReference, type);
	}

	public override BoundNode? VisitListPatternReceiverPlaceholder(BoundListPatternReceiverPlaceholder node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitListPatternIndexPlaceholder(BoundListPatternIndexPlaceholder node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitSlicePatternReceiverPlaceholder(BoundSlicePatternReceiverPlaceholder node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitSlicePatternRangePlaceholder(BoundSlicePatternRangePlaceholder node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitDup(BoundDup node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.RefKind, type);
	}

	public override BoundNode? VisitPassByCopy(BoundPassByCopy node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, type);
	}

	public override BoundNode? VisitBadExpression(BoundBadExpression node)
	{
		ImmutableArray<Symbol> symbols = VisitSymbols(node.Symbols);
		ImmutableArray<BoundExpression> childBoundNodes = VisitList(node.ChildBoundNodes);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.ResultKind, symbols, childBoundNodes, type);
	}

	public override BoundNode? VisitBadStatement(BoundBadStatement node)
	{
		ImmutableArray<BoundNode> childBoundNodes = VisitList(node.ChildBoundNodes);
		return node.Update(childBoundNodes);
	}

	public override BoundNode? VisitExtractedFinallyBlock(BoundExtractedFinallyBlock node)
	{
		BoundBlock finallyBlock = (BoundBlock)Visit(node.FinallyBlock);
		return node.Update(finallyBlock);
	}

	public override BoundNode? VisitTypeExpression(BoundTypeExpression node)
	{
		AliasSymbol aliasOpt = VisitAliasSymbol(node.AliasOpt);
		BoundTypeExpression boundContainingTypeOpt = (BoundTypeExpression)Visit(node.BoundContainingTypeOpt);
		ImmutableArray<BoundExpression> boundDimensionsOpt = VisitList(node.BoundDimensionsOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(aliasOpt, boundContainingTypeOpt, boundDimensionsOpt, node.TypeWithAnnotations, type);
	}

	public override BoundNode? VisitTypeOrValueExpression(BoundTypeOrValueExpression node)
	{
		Symbol valueSymbol = VisitSymbol(node.ValueSymbol);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Binder, valueSymbol, type);
	}

	public override BoundNode? VisitNamespaceExpression(BoundNamespaceExpression node)
	{
		NamespaceSymbol namespaceSymbol = VisitNamespaceSymbol(node.NamespaceSymbol);
		AliasSymbol aliasOpt = VisitAliasSymbol(node.AliasOpt);
		VisitType(node.Type);
		return node.Update(namespaceSymbol, aliasOpt);
	}

	public override BoundNode? VisitUnaryOperator(BoundUnaryOperator node)
	{
		MethodSymbol methodOpt = VisitMethodSymbol(node.MethodOpt);
		ImmutableArray<MethodSymbol> originalUserDefinedOperatorsOpt = VisitSymbols(node.OriginalUserDefinedOperatorsOpt);
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		TypeSymbol constrainedToTypeOpt = VisitType(node.ConstrainedToTypeOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.OperatorKind, operand, node.ConstantValueOpt, methodOpt, constrainedToTypeOpt, node.ResultKind, originalUserDefinedOperatorsOpt, type);
	}

	public override BoundNode? VisitIncrementOperator(BoundIncrementOperator node)
	{
		MethodSymbol methodOpt = VisitMethodSymbol(node.MethodOpt);
		ImmutableArray<MethodSymbol> originalUserDefinedOperatorsOpt = VisitSymbols(node.OriginalUserDefinedOperatorsOpt);
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		BoundValuePlaceholder operandPlaceholder = node.OperandPlaceholder;
		BoundExpression operandConversion = node.OperandConversion;
		BoundValuePlaceholder resultPlaceholder = node.ResultPlaceholder;
		BoundExpression resultConversion = node.ResultConversion;
		TypeSymbol constrainedToTypeOpt = VisitType(node.ConstrainedToTypeOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.OperatorKind, operand, methodOpt, constrainedToTypeOpt, operandPlaceholder, operandConversion, resultPlaceholder, resultConversion, node.ResultKind, originalUserDefinedOperatorsOpt, type);
	}

	public override BoundNode? VisitAddressOfOperator(BoundAddressOfOperator node)
	{
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(operand, node.IsManaged, type);
	}

	public override BoundNode? VisitUnconvertedAddressOfOperator(BoundUnconvertedAddressOfOperator node)
	{
		BoundMethodGroup operand = (BoundMethodGroup)Visit(node.Operand);
		VisitType(node.Type);
		return node.Update(operand);
	}

	public override BoundNode? VisitFunctionPointerLoad(BoundFunctionPointerLoad node)
	{
		MethodSymbol targetMethod = VisitMethodSymbol(node.TargetMethod);
		TypeSymbol constrainedToTypeOpt = VisitType(node.ConstrainedToTypeOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(targetMethod, constrainedToTypeOpt, type);
	}

	public override BoundNode? VisitPointerIndirectionOperator(BoundPointerIndirectionOperator node)
	{
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(operand, node.RefersToLocation, type);
	}

	public override BoundNode? VisitPointerElementAccess(BoundPointerElementAccess node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		BoundExpression index = (BoundExpression)Visit(node.Index);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, index, node.Checked, node.RefersToLocation, type);
	}

	public override BoundNode? VisitFunctionPointerInvocation(BoundFunctionPointerInvocation node)
	{
		BoundExpression invokedExpression = (BoundExpression)Visit(node.InvokedExpression);
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(invokedExpression, arguments, node.ArgumentRefKindsOpt, node.ResultKind, type);
	}

	public override BoundNode? VisitRefTypeOperator(BoundRefTypeOperator node)
	{
		MethodSymbol getTypeFromHandle = VisitMethodSymbol(node.GetTypeFromHandle);
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(operand, getTypeFromHandle, type);
	}

	public override BoundNode? VisitMakeRefOperator(BoundMakeRefOperator node)
	{
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(operand, type);
	}

	public override BoundNode? VisitRefValueOperator(BoundRefValueOperator node)
	{
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.NullableAnnotation, operand, type);
	}

	public override BoundNode? VisitFromEndIndexExpression(BoundFromEndIndexExpression node)
	{
		MethodSymbol methodOpt = VisitMethodSymbol(node.MethodOpt);
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(operand, methodOpt, type);
	}

	public override BoundNode? VisitRangeExpression(BoundRangeExpression node)
	{
		MethodSymbol methodOpt = VisitMethodSymbol(node.MethodOpt);
		BoundExpression leftOperandOpt = (BoundExpression)Visit(node.LeftOperandOpt);
		BoundExpression rightOperandOpt = (BoundExpression)Visit(node.RightOperandOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(leftOperandOpt, rightOperandOpt, methodOpt, type);
	}

	public override BoundNode? VisitBinaryOperator(BoundBinaryOperator node)
	{
		BoundExpression left = (BoundExpression)Visit(node.Left);
		BoundExpression right = (BoundExpression)Visit(node.Right);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.OperatorKind, node.Data, node.ResultKind, left, right, type);
	}

	public override BoundNode? VisitTupleBinaryOperator(BoundTupleBinaryOperator node)
	{
		BoundExpression left = (BoundExpression)Visit(node.Left);
		BoundExpression right = (BoundExpression)Visit(node.Right);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(left, right, node.OperatorKind, node.Operators, type);
	}

	public override BoundNode? VisitUserDefinedConditionalLogicalOperator(BoundUserDefinedConditionalLogicalOperator node)
	{
		MethodSymbol logicalOperator = VisitMethodSymbol(node.LogicalOperator);
		MethodSymbol trueOperator = VisitMethodSymbol(node.TrueOperator);
		MethodSymbol falseOperator = VisitMethodSymbol(node.FalseOperator);
		ImmutableArray<MethodSymbol> originalUserDefinedOperatorsOpt = VisitSymbols(node.OriginalUserDefinedOperatorsOpt);
		BoundValuePlaceholder trueFalseOperandPlaceholder = node.TrueFalseOperandPlaceholder;
		BoundExpression trueFalseOperandConversion = node.TrueFalseOperandConversion;
		BoundExpression left = (BoundExpression)Visit(node.Left);
		BoundExpression right = (BoundExpression)Visit(node.Right);
		TypeSymbol constrainedToTypeOpt = VisitType(node.ConstrainedToTypeOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.OperatorKind, logicalOperator, trueOperator, falseOperator, trueFalseOperandPlaceholder, trueFalseOperandConversion, constrainedToTypeOpt, node.ResultKind, originalUserDefinedOperatorsOpt, left, right, type);
	}

	public override BoundNode? VisitCompoundAssignmentOperator(BoundCompoundAssignmentOperator node)
	{
		ImmutableArray<MethodSymbol> originalUserDefinedOperatorsOpt = VisitSymbols(node.OriginalUserDefinedOperatorsOpt);
		BoundExpression left = (BoundExpression)Visit(node.Left);
		BoundExpression right = (BoundExpression)Visit(node.Right);
		BoundValuePlaceholder leftPlaceholder = node.LeftPlaceholder;
		BoundExpression leftConversion = node.LeftConversion;
		BoundValuePlaceholder finalPlaceholder = node.FinalPlaceholder;
		BoundExpression finalConversion = node.FinalConversion;
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Operator, left, right, leftPlaceholder, leftConversion, finalPlaceholder, finalConversion, node.ResultKind, originalUserDefinedOperatorsOpt, type);
	}

	public override BoundNode? VisitAssignmentOperator(BoundAssignmentOperator node)
	{
		BoundExpression left = (BoundExpression)Visit(node.Left);
		BoundExpression right = (BoundExpression)Visit(node.Right);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(left, right, node.IsRef, type);
	}

	public override BoundNode? VisitDeconstructionAssignmentOperator(BoundDeconstructionAssignmentOperator node)
	{
		BoundTupleExpression left = (BoundTupleExpression)Visit(node.Left);
		BoundConversion right = (BoundConversion)Visit(node.Right);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(left, right, node.IsUsed, type);
	}

	public override BoundNode? VisitNullCoalescingOperator(BoundNullCoalescingOperator node)
	{
		BoundExpression leftOperand = (BoundExpression)Visit(node.LeftOperand);
		BoundExpression rightOperand = (BoundExpression)Visit(node.RightOperand);
		BoundValuePlaceholder leftPlaceholder = node.LeftPlaceholder;
		BoundExpression leftConversion = node.LeftConversion;
		TypeSymbol type = VisitType(node.Type);
		return node.Update(leftOperand, rightOperand, leftPlaceholder, leftConversion, node.OperatorResultKind, node.Checked, type);
	}

	public override BoundNode? VisitNullCoalescingAssignmentOperator(BoundNullCoalescingAssignmentOperator node)
	{
		BoundExpression leftOperand = (BoundExpression)Visit(node.LeftOperand);
		BoundExpression rightOperand = (BoundExpression)Visit(node.RightOperand);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(leftOperand, rightOperand, type);
	}

	public override BoundNode? VisitUnconvertedConditionalOperator(BoundUnconvertedConditionalOperator node)
	{
		BoundExpression condition = (BoundExpression)Visit(node.Condition);
		BoundExpression consequence = (BoundExpression)Visit(node.Consequence);
		BoundExpression alternative = (BoundExpression)Visit(node.Alternative);
		VisitType(node.Type);
		return node.Update(condition, consequence, alternative, node.ConstantValueOpt, node.NoCommonTypeError);
	}

	public override BoundNode? VisitConditionalOperator(BoundConditionalOperator node)
	{
		BoundExpression condition = (BoundExpression)Visit(node.Condition);
		BoundExpression consequence = (BoundExpression)Visit(node.Consequence);
		BoundExpression alternative = (BoundExpression)Visit(node.Alternative);
		TypeSymbol naturalTypeOpt = VisitType(node.NaturalTypeOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.IsRef, condition, consequence, alternative, node.ConstantValueOpt, naturalTypeOpt, node.WasTargetTyped, type);
	}

	public override BoundNode? VisitArrayAccess(BoundArrayAccess node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		ImmutableArray<BoundExpression> indices = VisitList(node.Indices);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, indices, type);
	}

	public override BoundNode? VisitRefArrayAccess(BoundRefArrayAccess node)
	{
		BoundArrayAccess arrayAccess = (BoundArrayAccess)Visit(node.ArrayAccess);
		VisitType(node.Type);
		return node.Update(arrayAccess);
	}

	public override BoundNode? VisitArrayLength(BoundArrayLength node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, type);
	}

	public override BoundNode? VisitAwaitableInfo(BoundAwaitableInfo node)
	{
		PropertySymbol isCompleted = VisitPropertySymbol(node.IsCompleted);
		MethodSymbol getResult = VisitMethodSymbol(node.GetResult);
		BoundAwaitableValuePlaceholder awaitableInstancePlaceholder = (BoundAwaitableValuePlaceholder)Visit(node.AwaitableInstancePlaceholder);
		BoundExpression getAwaiter = (BoundExpression)Visit(node.GetAwaiter);
		BoundCall runtimeAsyncAwaitCall = (BoundCall)Visit(node.RuntimeAsyncAwaitCall);
		BoundAwaitableValuePlaceholder runtimeAsyncAwaitCallPlaceholder = (BoundAwaitableValuePlaceholder)Visit(node.RuntimeAsyncAwaitCallPlaceholder);
		return node.Update(awaitableInstancePlaceholder, node.IsDynamic, getAwaiter, isCompleted, getResult, runtimeAsyncAwaitCall, runtimeAsyncAwaitCallPlaceholder);
	}

	public override BoundNode? VisitAwaitExpression(BoundAwaitExpression node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		BoundAwaitableInfo awaitableInfo = (BoundAwaitableInfo)Visit(node.AwaitableInfo);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, awaitableInfo, node.DebugInfo, type);
	}

	public override BoundNode? VisitTypeOfOperator(BoundTypeOfOperator node)
	{
		MethodSymbol getTypeFromHandle = VisitMethodSymbol(node.GetTypeFromHandle);
		BoundTypeExpression sourceType = (BoundTypeExpression)Visit(node.SourceType);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(sourceType, getTypeFromHandle, type);
	}

	public override BoundNode? VisitBlockInstrumentation(BoundBlockInstrumentation node)
	{
		ImmutableArray<LocalSymbol> locals = VisitLocals(node.Locals);
		BoundStatement prologue = (BoundStatement)Visit(node.Prologue);
		BoundStatement epilogue = (BoundStatement)Visit(node.Epilogue);
		return node.Update(locals, prologue, epilogue);
	}

	public override BoundNode? VisitMethodDefIndex(BoundMethodDefIndex node)
	{
		MethodSymbol method = VisitMethodSymbol(node.Method);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(method, type);
	}

	public override BoundNode? VisitLocalId(BoundLocalId node)
	{
		LocalSymbol local = VisitLocalSymbol(node.Local);
		FieldSymbol hoistedField = VisitFieldSymbol(node.HoistedField);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(local, hoistedField, type);
	}

	public override BoundNode? VisitParameterId(BoundParameterId node)
	{
		ParameterSymbol parameter = VisitParameterSymbol(node.Parameter);
		FieldSymbol hoistedField = VisitFieldSymbol(node.HoistedField);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(parameter, hoistedField, type);
	}

	public override BoundNode? VisitStateMachineInstanceId(BoundStateMachineInstanceId node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitMaximumMethodDefIndex(BoundMaximumMethodDefIndex node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitInstrumentationPayloadRoot(BoundInstrumentationPayloadRoot node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.AnalysisKind, type);
	}

	public override BoundNode? VisitThrowIfModuleCancellationRequested(BoundThrowIfModuleCancellationRequested node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitModuleCancellationTokenExpression(ModuleCancellationTokenExpression node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitModuleVersionId(BoundModuleVersionId node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitModuleVersionIdString(BoundModuleVersionIdString node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitSourceDocumentIndex(BoundSourceDocumentIndex node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Document, type);
	}

	public override BoundNode? VisitMethodInfo(BoundMethodInfo node)
	{
		MethodSymbol method = VisitMethodSymbol(node.Method);
		MethodSymbol getMethodFromHandle = VisitMethodSymbol(node.GetMethodFromHandle);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(method, getMethodFromHandle, type);
	}

	public override BoundNode? VisitFieldInfo(BoundFieldInfo node)
	{
		FieldSymbol field = VisitFieldSymbol(node.Field);
		MethodSymbol getFieldFromHandle = VisitMethodSymbol(node.GetFieldFromHandle);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(field, getFieldFromHandle, type);
	}

	public override BoundNode? VisitDefaultLiteral(BoundDefaultLiteral node)
	{
		VisitType(node.Type);
		return node.Update();
	}

	public override BoundNode? VisitDefaultExpression(BoundDefaultExpression node)
	{
		BoundTypeExpression targetType = node.TargetType;
		TypeSymbol type = VisitType(node.Type);
		return node.Update(targetType, node.ConstantValueOpt, type);
	}

	public override BoundNode? VisitIsOperator(BoundIsOperator node)
	{
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		BoundTypeExpression targetType = (BoundTypeExpression)Visit(node.TargetType);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(operand, targetType, node.ConversionKind, type);
	}

	public override BoundNode? VisitAsOperator(BoundAsOperator node)
	{
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		BoundTypeExpression targetType = (BoundTypeExpression)Visit(node.TargetType);
		BoundValuePlaceholder operandPlaceholder = node.OperandPlaceholder;
		BoundExpression operandConversion = node.OperandConversion;
		TypeSymbol type = VisitType(node.Type);
		return node.Update(operand, targetType, operandPlaceholder, operandConversion, type);
	}

	public override BoundNode? VisitSizeOfOperator(BoundSizeOfOperator node)
	{
		BoundTypeExpression sourceType = (BoundTypeExpression)Visit(node.SourceType);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(sourceType, node.ConstantValueOpt, type);
	}

	public override BoundNode? VisitConversion(BoundConversion node)
	{
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(operand, node.Conversion, node.IsBaseConversion, node.Checked, node.ExplicitCastInCode, node.ConstantValueOpt, node.ConversionGroupOpt, type);
	}

	public override BoundNode? VisitReadOnlySpanFromArray(BoundReadOnlySpanFromArray node)
	{
		MethodSymbol conversionMethod = VisitMethodSymbol(node.ConversionMethod);
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(operand, conversionMethod, type);
	}

	public override BoundNode? VisitArgList(BoundArgList node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitArgListOperator(BoundArgListOperator node)
	{
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(arguments, node.ArgumentRefKindsOpt, type);
	}

	public override BoundNode? VisitFixedLocalCollectionInitializer(BoundFixedLocalCollectionInitializer node)
	{
		MethodSymbol getPinnableOpt = VisitMethodSymbol(node.GetPinnableOpt);
		BoundValuePlaceholder elementPointerPlaceholder = node.ElementPointerPlaceholder;
		BoundExpression elementPointerConversion = node.ElementPointerConversion;
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		TypeSymbol elementPointerType = VisitType(node.ElementPointerType);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(elementPointerType, elementPointerPlaceholder, elementPointerConversion, expression, getPinnableOpt, type);
	}

	public override BoundNode? VisitSequencePoint(BoundSequencePoint node)
	{
		BoundStatement statementOpt = (BoundStatement)Visit(node.StatementOpt);
		return node.Update(statementOpt);
	}

	public override BoundNode? VisitSequencePointWithSpan(BoundSequencePointWithSpan node)
	{
		BoundStatement statementOpt = (BoundStatement)Visit(node.StatementOpt);
		return node.Update(statementOpt, node.Span);
	}

	public override BoundNode? VisitSavePreviousSequencePoint(BoundSavePreviousSequencePoint node)
	{
		return node;
	}

	public override BoundNode? VisitRestorePreviousSequencePoint(BoundRestorePreviousSequencePoint node)
	{
		return node;
	}

	public override BoundNode? VisitStepThroughSequencePoint(BoundStepThroughSequencePoint node)
	{
		return node;
	}

	public override BoundNode? VisitBlock(BoundBlock node)
	{
		ImmutableArray<LocalSymbol> locals = VisitLocals(node.Locals);
		ImmutableArray<MethodSymbol> localFunctions = VisitDeclaredLocalFunctions(node.LocalFunctions);
		BoundBlockInstrumentation instrumentation = (BoundBlockInstrumentation)Visit(node.Instrumentation);
		ImmutableArray<BoundStatement> statements = VisitList(node.Statements);
		return node.Update(locals, localFunctions, node.HasUnsafeModifier, instrumentation, statements);
	}

	public override BoundNode? VisitScope(BoundScope node)
	{
		ImmutableArray<LocalSymbol> locals = VisitLocals(node.Locals);
		ImmutableArray<BoundStatement> statements = VisitList(node.Statements);
		return node.Update(locals, statements);
	}

	public override BoundNode? VisitStateMachineScope(BoundStateMachineScope node)
	{
		ImmutableArray<StateMachineFieldSymbol> fields = VisitSymbols(node.Fields);
		BoundStatement statement = (BoundStatement)Visit(node.Statement);
		return node.Update(fields, statement);
	}

	public override BoundNode? VisitLocalDeclaration(BoundLocalDeclaration node)
	{
		LocalSymbol localSymbol = VisitLocalSymbol(node.LocalSymbol);
		BoundTypeExpression declaredTypeOpt = (BoundTypeExpression)Visit(node.DeclaredTypeOpt);
		BoundExpression initializerOpt = (BoundExpression)Visit(node.InitializerOpt);
		ImmutableArray<BoundExpression> argumentsOpt = VisitList(node.ArgumentsOpt);
		return node.Update(localSymbol, declaredTypeOpt, initializerOpt, argumentsOpt, node.InferredType);
	}

	public override BoundNode? VisitMultipleLocalDeclarations(BoundMultipleLocalDeclarations node)
	{
		ImmutableArray<BoundLocalDeclaration> localDeclarations = VisitList(node.LocalDeclarations);
		return node.Update(localDeclarations);
	}

	public override BoundNode? VisitUsingLocalDeclarations(BoundUsingLocalDeclarations node)
	{
		BoundAwaitableInfo awaitOpt = (BoundAwaitableInfo)Visit(node.AwaitOpt);
		ImmutableArray<BoundLocalDeclaration> localDeclarations = VisitList(node.LocalDeclarations);
		return node.Update(node.PatternDisposeInfoOpt, awaitOpt, localDeclarations);
	}

	public override BoundNode? VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
	{
		MethodSymbol symbol = VisitMethodSymbol(node.Symbol);
		BoundBlock blockBody = (BoundBlock)Visit(node.BlockBody);
		BoundBlock expressionBody = (BoundBlock)Visit(node.ExpressionBody);
		return node.Update(symbol, blockBody, expressionBody);
	}

	public override BoundNode? VisitNoOpStatement(BoundNoOpStatement node)
	{
		return node;
	}

	public override BoundNode? VisitReturnStatement(BoundReturnStatement node)
	{
		BoundExpression expressionOpt = (BoundExpression)Visit(node.ExpressionOpt);
		return node.Update(node.RefKind, expressionOpt, node.Checked);
	}

	public override BoundNode? VisitYieldReturnStatement(BoundYieldReturnStatement node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		return node.Update(expression);
	}

	public override BoundNode? VisitYieldBreakStatement(BoundYieldBreakStatement node)
	{
		return node;
	}

	public override BoundNode? VisitThrowStatement(BoundThrowStatement node)
	{
		BoundExpression expressionOpt = (BoundExpression)Visit(node.ExpressionOpt);
		return node.Update(expressionOpt);
	}

	public override BoundNode? VisitExpressionStatement(BoundExpressionStatement node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		return node.Update(expression);
	}

	public override BoundNode? VisitBreakStatement(BoundBreakStatement node)
	{
		LabelSymbol label = VisitLabelSymbol(node.Label);
		return node.Update(label);
	}

	public override BoundNode? VisitContinueStatement(BoundContinueStatement node)
	{
		LabelSymbol label = VisitLabelSymbol(node.Label);
		return node.Update(label);
	}

	public override BoundNode? VisitSwitchStatement(BoundSwitchStatement node)
	{
		ImmutableArray<LocalSymbol> innerLocals = VisitLocals(node.InnerLocals);
		ImmutableArray<MethodSymbol> innerLocalFunctions = VisitDeclaredLocalFunctions(node.InnerLocalFunctions);
		LabelSymbol breakLabel = VisitLabelSymbol(node.BreakLabel);
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		ImmutableArray<BoundSwitchSection> switchSections = VisitList(node.SwitchSections);
		BoundDecisionDag reachabilityDecisionDag = node.ReachabilityDecisionDag;
		BoundSwitchLabel defaultLabel = (BoundSwitchLabel)Visit(node.DefaultLabel);
		return node.Update(expression, innerLocals, innerLocalFunctions, switchSections, reachabilityDecisionDag, defaultLabel, breakLabel);
	}

	public override BoundNode? VisitSwitchDispatch(BoundSwitchDispatch node)
	{
		LabelSymbol defaultLabel = VisitLabelSymbol(node.DefaultLabel);
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		return node.Update(expression, node.Cases, defaultLabel, node.LengthBasedStringSwitchDataOpt);
	}

	public override BoundNode? VisitIfStatement(BoundIfStatement node)
	{
		BoundExpression condition = (BoundExpression)Visit(node.Condition);
		BoundStatement consequence = (BoundStatement)Visit(node.Consequence);
		BoundStatement alternativeOpt = (BoundStatement)Visit(node.AlternativeOpt);
		return node.Update(condition, consequence, alternativeOpt);
	}

	public override BoundNode? VisitDoStatement(BoundDoStatement node)
	{
		ImmutableArray<LocalSymbol> locals = VisitLocals(node.Locals);
		LabelSymbol breakLabel = VisitLabelSymbol(node.BreakLabel);
		LabelSymbol continueLabel = VisitLabelSymbol(node.ContinueLabel);
		BoundExpression condition = (BoundExpression)Visit(node.Condition);
		BoundStatement body = (BoundStatement)Visit(node.Body);
		return node.Update(locals, condition, body, breakLabel, continueLabel);
	}

	public override BoundNode? VisitWhileStatement(BoundWhileStatement node)
	{
		ImmutableArray<LocalSymbol> locals = VisitLocals(node.Locals);
		LabelSymbol breakLabel = VisitLabelSymbol(node.BreakLabel);
		LabelSymbol continueLabel = VisitLabelSymbol(node.ContinueLabel);
		BoundExpression condition = (BoundExpression)Visit(node.Condition);
		BoundStatement body = (BoundStatement)Visit(node.Body);
		return node.Update(locals, condition, body, breakLabel, continueLabel);
	}

	public override BoundNode? VisitForStatement(BoundForStatement node)
	{
		ImmutableArray<LocalSymbol> outerLocals = VisitLocals(node.OuterLocals);
		ImmutableArray<LocalSymbol> innerLocals = VisitLocals(node.InnerLocals);
		LabelSymbol breakLabel = VisitLabelSymbol(node.BreakLabel);
		LabelSymbol continueLabel = VisitLabelSymbol(node.ContinueLabel);
		BoundStatement initializer = (BoundStatement)Visit(node.Initializer);
		BoundExpression condition = (BoundExpression)Visit(node.Condition);
		BoundStatement increment = (BoundStatement)Visit(node.Increment);
		BoundStatement body = (BoundStatement)Visit(node.Body);
		return node.Update(outerLocals, initializer, innerLocals, condition, increment, body, breakLabel, continueLabel);
	}

	public override BoundNode? VisitForEachStatement(BoundForEachStatement node)
	{
		ImmutableArray<LocalSymbol> iterationVariables = VisitLocals(node.IterationVariables);
		LabelSymbol breakLabel = VisitLabelSymbol(node.BreakLabel);
		LabelSymbol continueLabel = VisitLabelSymbol(node.ContinueLabel);
		BoundValuePlaceholder elementPlaceholder = node.ElementPlaceholder;
		BoundExpression elementConversion = node.ElementConversion;
		BoundTypeExpression iterationVariableType = (BoundTypeExpression)Visit(node.IterationVariableType);
		BoundExpression iterationErrorExpressionOpt = (BoundExpression)Visit(node.IterationErrorExpressionOpt);
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		BoundForEachDeconstructStep deconstructionOpt = (BoundForEachDeconstructStep)Visit(node.DeconstructionOpt);
		BoundStatement body = (BoundStatement)Visit(node.Body);
		return node.Update(node.EnumeratorInfoOpt, elementPlaceholder, elementConversion, iterationVariableType, iterationVariables, iterationErrorExpressionOpt, expression, deconstructionOpt, body, breakLabel, continueLabel);
	}

	public override BoundNode? VisitForEachDeconstructStep(BoundForEachDeconstructStep node)
	{
		BoundDeconstructionAssignmentOperator deconstructionAssignment = (BoundDeconstructionAssignmentOperator)Visit(node.DeconstructionAssignment);
		BoundDeconstructValuePlaceholder targetPlaceholder = (BoundDeconstructValuePlaceholder)Visit(node.TargetPlaceholder);
		return node.Update(deconstructionAssignment, targetPlaceholder);
	}

	public override BoundNode? VisitUsingStatement(BoundUsingStatement node)
	{
		ImmutableArray<LocalSymbol> locals = VisitLocals(node.Locals);
		BoundMultipleLocalDeclarations declarationsOpt = (BoundMultipleLocalDeclarations)Visit(node.DeclarationsOpt);
		BoundExpression expressionOpt = (BoundExpression)Visit(node.ExpressionOpt);
		BoundStatement body = (BoundStatement)Visit(node.Body);
		BoundAwaitableInfo awaitOpt = (BoundAwaitableInfo)Visit(node.AwaitOpt);
		return node.Update(locals, declarationsOpt, expressionOpt, body, awaitOpt, node.PatternDisposeInfoOpt);
	}

	public override BoundNode? VisitFixedStatement(BoundFixedStatement node)
	{
		ImmutableArray<LocalSymbol> locals = VisitLocals(node.Locals);
		BoundMultipleLocalDeclarations declarations = (BoundMultipleLocalDeclarations)Visit(node.Declarations);
		BoundStatement body = (BoundStatement)Visit(node.Body);
		return node.Update(locals, declarations, body);
	}

	public override BoundNode? VisitLockStatement(BoundLockStatement node)
	{
		BoundExpression argument = (BoundExpression)Visit(node.Argument);
		BoundStatement body = (BoundStatement)Visit(node.Body);
		return node.Update(argument, body);
	}

	public override BoundNode? VisitTryStatement(BoundTryStatement node)
	{
		LabelSymbol finallyLabelOpt = VisitLabelSymbol(node.FinallyLabelOpt);
		BoundBlock tryBlock = (BoundBlock)Visit(node.TryBlock);
		ImmutableArray<BoundCatchBlock> catchBlocks = VisitList(node.CatchBlocks);
		BoundBlock finallyBlockOpt = (BoundBlock)Visit(node.FinallyBlockOpt);
		return node.Update(tryBlock, catchBlocks, finallyBlockOpt, finallyLabelOpt, node.PreferFaultHandler);
	}

	public override BoundNode? VisitCatchBlock(BoundCatchBlock node)
	{
		ImmutableArray<LocalSymbol> locals = VisitLocals(node.Locals);
		BoundExpression exceptionSourceOpt = (BoundExpression)Visit(node.ExceptionSourceOpt);
		BoundStatementList exceptionFilterPrologueOpt = (BoundStatementList)Visit(node.ExceptionFilterPrologueOpt);
		BoundExpression exceptionFilterOpt = (BoundExpression)Visit(node.ExceptionFilterOpt);
		BoundBlock body = (BoundBlock)Visit(node.Body);
		TypeSymbol exceptionTypeOpt = VisitType(node.ExceptionTypeOpt);
		return node.Update(locals, exceptionSourceOpt, exceptionTypeOpt, exceptionFilterPrologueOpt, exceptionFilterOpt, body, node.IsSynthesizedAsyncCatchAll);
	}

	public override BoundNode? VisitLiteral(BoundLiteral node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.ConstantValueOpt, type);
	}

	public override BoundNode? VisitUtf8String(BoundUtf8String node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Value, type);
	}

	public override BoundNode? VisitThisReference(BoundThisReference node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitPreviousSubmissionReference(BoundPreviousSubmissionReference node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitHostObjectMemberReference(BoundHostObjectMemberReference node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitBaseReference(BoundBaseReference node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitLocal(BoundLocal node)
	{
		LocalSymbol localSymbol = VisitLocalSymbol(node.LocalSymbol);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(localSymbol, node.DeclarationKind, node.ConstantValueOpt, node.IsNullableUnknown, type);
	}

	public override BoundNode? VisitPseudoVariable(BoundPseudoVariable node)
	{
		LocalSymbol localSymbol = VisitLocalSymbol(node.LocalSymbol);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(localSymbol, node.EmitExpressions, type);
	}

	public override BoundNode? VisitRangeVariable(BoundRangeVariable node)
	{
		RangeVariableSymbol rangeVariableSymbol = VisitRangeVariableSymbol(node.RangeVariableSymbol);
		BoundExpression value = (BoundExpression)Visit(node.Value);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(rangeVariableSymbol, value, type);
	}

	public override BoundNode? VisitParameter(BoundParameter node)
	{
		ParameterSymbol parameterSymbol = VisitParameterSymbol(node.ParameterSymbol);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(parameterSymbol, type);
	}

	public override BoundNode? VisitLabelStatement(BoundLabelStatement node)
	{
		LabelSymbol label = VisitLabelSymbol(node.Label);
		return node.Update(label);
	}

	public override BoundNode? VisitGotoStatement(BoundGotoStatement node)
	{
		LabelSymbol label = VisitLabelSymbol(node.Label);
		BoundExpression caseExpressionOpt = (BoundExpression)Visit(node.CaseExpressionOpt);
		BoundLabel labelExpressionOpt = (BoundLabel)Visit(node.LabelExpressionOpt);
		return node.Update(label, caseExpressionOpt, labelExpressionOpt);
	}

	public override BoundNode? VisitLabeledStatement(BoundLabeledStatement node)
	{
		LabelSymbol label = VisitLabelSymbol(node.Label);
		BoundStatement body = (BoundStatement)Visit(node.Body);
		return node.Update(label, body);
	}

	public override BoundNode? VisitLabel(BoundLabel node)
	{
		LabelSymbol label = VisitLabelSymbol(node.Label);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(label, type);
	}

	public override BoundNode? VisitStatementList(BoundStatementList node)
	{
		ImmutableArray<BoundStatement> statements = VisitList(node.Statements);
		return node.Update(statements);
	}

	public override BoundNode? VisitConditionalGoto(BoundConditionalGoto node)
	{
		LabelSymbol label = VisitLabelSymbol(node.Label);
		BoundExpression condition = (BoundExpression)Visit(node.Condition);
		return node.Update(condition, node.JumpIfTrue, label);
	}

	public override BoundNode? VisitSwitchExpressionArm(BoundSwitchExpressionArm node)
	{
		ImmutableArray<LocalSymbol> locals = VisitLocals(node.Locals);
		LabelSymbol label = VisitLabelSymbol(node.Label);
		BoundPattern pattern = (BoundPattern)Visit(node.Pattern);
		BoundExpression whenClause = (BoundExpression)Visit(node.WhenClause);
		BoundExpression value = (BoundExpression)Visit(node.Value);
		return node.Update(locals, pattern, whenClause, value, label);
	}

	public override BoundNode? VisitUnconvertedSwitchExpression(BoundUnconvertedSwitchExpression node)
	{
		LabelSymbol defaultLabel = VisitLabelSymbol(node.DefaultLabel);
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		ImmutableArray<BoundSwitchExpressionArm> switchArms = VisitList(node.SwitchArms);
		BoundDecisionDag reachabilityDecisionDag = node.ReachabilityDecisionDag;
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, switchArms, reachabilityDecisionDag, defaultLabel, node.ReportedNotExhaustive, type);
	}

	public override BoundNode? VisitConvertedSwitchExpression(BoundConvertedSwitchExpression node)
	{
		LabelSymbol defaultLabel = VisitLabelSymbol(node.DefaultLabel);
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		ImmutableArray<BoundSwitchExpressionArm> switchArms = VisitList(node.SwitchArms);
		BoundDecisionDag reachabilityDecisionDag = node.ReachabilityDecisionDag;
		TypeSymbol naturalTypeOpt = VisitType(node.NaturalTypeOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(naturalTypeOpt, node.WasTargetTyped, expression, switchArms, reachabilityDecisionDag, defaultLabel, node.ReportedNotExhaustive, type);
	}

	public override BoundNode? VisitDecisionDag(BoundDecisionDag node)
	{
		BoundDecisionDagNode rootNode = (BoundDecisionDagNode)Visit(node.RootNode);
		return node.Update(rootNode);
	}

	public override BoundNode? VisitEvaluationDecisionDagNode(BoundEvaluationDecisionDagNode node)
	{
		BoundDagEvaluation evaluation = (BoundDagEvaluation)Visit(node.Evaluation);
		BoundDecisionDagNode next = (BoundDecisionDagNode)Visit(node.Next);
		return node.Update(evaluation, next);
	}

	public override BoundNode? VisitTestDecisionDagNode(BoundTestDecisionDagNode node)
	{
		BoundDagTest test = (BoundDagTest)Visit(node.Test);
		BoundDecisionDagNode whenTrue = (BoundDecisionDagNode)Visit(node.WhenTrue);
		BoundDecisionDagNode whenFalse = (BoundDecisionDagNode)Visit(node.WhenFalse);
		return node.Update(test, whenTrue, whenFalse);
	}

	public override BoundNode? VisitWhenDecisionDagNode(BoundWhenDecisionDagNode node)
	{
		BoundExpression whenExpression = (BoundExpression)Visit(node.WhenExpression);
		BoundDecisionDagNode whenTrue = (BoundDecisionDagNode)Visit(node.WhenTrue);
		BoundDecisionDagNode whenFalse = (BoundDecisionDagNode)Visit(node.WhenFalse);
		return node.Update(node.Bindings, whenExpression, whenTrue, whenFalse);
	}

	public override BoundNode? VisitLeafDecisionDagNode(BoundLeafDecisionDagNode node)
	{
		LabelSymbol label = VisitLabelSymbol(node.Label);
		return node.Update(label);
	}

	public override BoundNode? VisitDagTemp(BoundDagTemp node)
	{
		BoundDagEvaluation source = (BoundDagEvaluation)Visit(node.Source);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type, source, node.Index);
	}

	public override BoundNode? VisitDagTypeTest(BoundDagTypeTest node)
	{
		BoundDagTemp input = (BoundDagTemp)Visit(node.Input);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type, input);
	}

	public override BoundNode? VisitDagNonNullTest(BoundDagNonNullTest node)
	{
		BoundDagTemp input = (BoundDagTemp)Visit(node.Input);
		return node.Update(node.IsExplicitTest, input);
	}

	public override BoundNode? VisitDagExplicitNullTest(BoundDagExplicitNullTest node)
	{
		BoundDagTemp input = (BoundDagTemp)Visit(node.Input);
		return node.Update(input);
	}

	public override BoundNode? VisitDagValueTest(BoundDagValueTest node)
	{
		BoundDagTemp input = (BoundDagTemp)Visit(node.Input);
		return node.Update(node.Value, input);
	}

	public override BoundNode? VisitDagRelationalTest(BoundDagRelationalTest node)
	{
		BoundDagTemp input = (BoundDagTemp)Visit(node.Input);
		return node.Update(node.OperatorKind, node.Value, input);
	}

	public override BoundNode? VisitDagDeconstructEvaluation(BoundDagDeconstructEvaluation node)
	{
		MethodSymbol deconstructMethod = VisitMethodSymbol(node.DeconstructMethod);
		BoundDagTemp input = (BoundDagTemp)Visit(node.Input);
		return node.Update(deconstructMethod, input);
	}

	public override BoundNode? VisitDagTypeEvaluation(BoundDagTypeEvaluation node)
	{
		BoundDagTemp input = (BoundDagTemp)Visit(node.Input);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type, input);
	}

	public override BoundNode? VisitDagFieldEvaluation(BoundDagFieldEvaluation node)
	{
		FieldSymbol field = VisitFieldSymbol(node.Field);
		BoundDagTemp input = (BoundDagTemp)Visit(node.Input);
		return node.Update(field, input);
	}

	public override BoundNode? VisitDagPropertyEvaluation(BoundDagPropertyEvaluation node)
	{
		PropertySymbol property = VisitPropertySymbol(node.Property);
		BoundDagTemp input = (BoundDagTemp)Visit(node.Input);
		return node.Update(property, node.IsLengthOrCount, input);
	}

	public override BoundNode? VisitDagIndexEvaluation(BoundDagIndexEvaluation node)
	{
		PropertySymbol property = VisitPropertySymbol(node.Property);
		BoundDagTemp input = (BoundDagTemp)Visit(node.Input);
		return node.Update(property, node.Index, input);
	}

	public override BoundNode? VisitDagIndexerEvaluation(BoundDagIndexerEvaluation node)
	{
		BoundDagTemp lengthTemp = (BoundDagTemp)Visit(node.LengthTemp);
		BoundExpression indexerAccess = (BoundExpression)Visit(node.IndexerAccess);
		BoundListPatternReceiverPlaceholder receiverPlaceholder = (BoundListPatternReceiverPlaceholder)Visit(node.ReceiverPlaceholder);
		BoundListPatternIndexPlaceholder argumentPlaceholder = (BoundListPatternIndexPlaceholder)Visit(node.ArgumentPlaceholder);
		BoundDagTemp input = (BoundDagTemp)Visit(node.Input);
		TypeSymbol indexerType = VisitType(node.IndexerType);
		return node.Update(indexerType, lengthTemp, node.Index, indexerAccess, receiverPlaceholder, argumentPlaceholder, input);
	}

	public override BoundNode? VisitDagSliceEvaluation(BoundDagSliceEvaluation node)
	{
		BoundDagTemp lengthTemp = (BoundDagTemp)Visit(node.LengthTemp);
		BoundExpression indexerAccess = (BoundExpression)Visit(node.IndexerAccess);
		BoundSlicePatternReceiverPlaceholder receiverPlaceholder = (BoundSlicePatternReceiverPlaceholder)Visit(node.ReceiverPlaceholder);
		BoundSlicePatternRangePlaceholder argumentPlaceholder = (BoundSlicePatternRangePlaceholder)Visit(node.ArgumentPlaceholder);
		BoundDagTemp input = (BoundDagTemp)Visit(node.Input);
		TypeSymbol sliceType = VisitType(node.SliceType);
		return node.Update(sliceType, lengthTemp, node.StartIndex, node.EndIndex, indexerAccess, receiverPlaceholder, argumentPlaceholder, input);
	}

	public override BoundNode? VisitDagAssignmentEvaluation(BoundDagAssignmentEvaluation node)
	{
		BoundDagTemp target = (BoundDagTemp)Visit(node.Target);
		BoundDagTemp input = (BoundDagTemp)Visit(node.Input);
		return node.Update(target, input);
	}

	public override BoundNode? VisitSwitchSection(BoundSwitchSection node)
	{
		ImmutableArray<LocalSymbol> locals = VisitLocals(node.Locals);
		ImmutableArray<BoundSwitchLabel> switchLabels = VisitList(node.SwitchLabels);
		ImmutableArray<BoundStatement> statements = VisitList(node.Statements);
		return node.Update(locals, switchLabels, statements);
	}

	public override BoundNode? VisitSwitchLabel(BoundSwitchLabel node)
	{
		LabelSymbol label = VisitLabelSymbol(node.Label);
		BoundPattern pattern = (BoundPattern)Visit(node.Pattern);
		BoundExpression whenClause = (BoundExpression)Visit(node.WhenClause);
		return node.Update(label, pattern, whenClause);
	}

	public override BoundNode? VisitSequencePointExpression(BoundSequencePointExpression node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, type);
	}

	public override BoundNode? VisitSequence(BoundSequence node)
	{
		ImmutableArray<LocalSymbol> locals = VisitLocals(node.Locals);
		ImmutableArray<BoundExpression> sideEffects = VisitList(node.SideEffects);
		BoundExpression value = (BoundExpression)Visit(node.Value);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(locals, sideEffects, value, type);
	}

	public override BoundNode? VisitSpillSequence(BoundSpillSequence node)
	{
		ImmutableArray<LocalSymbol> locals = VisitLocals(node.Locals);
		ImmutableArray<BoundStatement> sideEffects = VisitList(node.SideEffects);
		BoundExpression value = (BoundExpression)Visit(node.Value);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(locals, sideEffects, value, type);
	}

	public override BoundNode? VisitDynamicMemberAccess(BoundDynamicMemberAccess node)
	{
		BoundExpression receiver = (BoundExpression)Visit(node.Receiver);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiver, node.TypeArgumentsOpt, node.Name, node.Invoked, node.Indexed, type);
	}

	public override BoundNode? VisitDynamicInvocation(BoundDynamicInvocation node)
	{
		ImmutableArray<MethodSymbol> applicableMethods = VisitSymbols(node.ApplicableMethods);
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, applicableMethods, expression, arguments, type);
	}

	public override BoundNode? VisitConditionalAccess(BoundConditionalAccess node)
	{
		BoundExpression receiver = (BoundExpression)Visit(node.Receiver);
		BoundExpression accessExpression = (BoundExpression)Visit(node.AccessExpression);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiver, accessExpression, type);
	}

	public override BoundNode? VisitLoweredConditionalAccess(BoundLoweredConditionalAccess node)
	{
		MethodSymbol hasValueMethodOpt = VisitMethodSymbol(node.HasValueMethodOpt);
		BoundExpression receiver = (BoundExpression)Visit(node.Receiver);
		BoundExpression whenNotNull = (BoundExpression)Visit(node.WhenNotNull);
		BoundExpression whenNullOpt = (BoundExpression)Visit(node.WhenNullOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiver, hasValueMethodOpt, whenNotNull, whenNullOpt, node.Id, node.ForceCopyOfNullableValueType, type);
	}

	public override BoundNode? VisitConditionalReceiver(BoundConditionalReceiver node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Id, type);
	}

	public override BoundNode? VisitComplexConditionalReceiver(BoundComplexConditionalReceiver node)
	{
		BoundExpression valueTypeReceiver = (BoundExpression)Visit(node.ValueTypeReceiver);
		BoundExpression referenceTypeReceiver = (BoundExpression)Visit(node.ReferenceTypeReceiver);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(valueTypeReceiver, referenceTypeReceiver, type);
	}

	public override BoundNode? VisitMethodGroup(BoundMethodGroup node)
	{
		ImmutableArray<MethodSymbol> methods = VisitSymbols(node.Methods);
		Symbol lookupSymbolOpt = VisitSymbol(node.LookupSymbolOpt);
		FunctionTypeSymbol functionType = VisitFunctionTypeSymbol(node.FunctionType);
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		VisitType(node.Type);
		return node.Update(node.TypeArgumentsOpt, node.Name, methods, lookupSymbolOpt, node.LookupError, node.Flags, functionType, receiverOpt, node.ResultKind);
	}

	public override BoundNode? VisitPropertyGroup(BoundPropertyGroup node)
	{
		ImmutableArray<PropertySymbol> properties = VisitSymbols(node.Properties);
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		VisitType(node.Type);
		return node.Update(properties, receiverOpt, node.ResultKind);
	}

	public override BoundNode? VisitCall(BoundCall node)
	{
		MethodSymbol method = VisitMethodSymbol(node.Method);
		ImmutableArray<MethodSymbol> originalMethodsOpt = VisitSymbols(node.OriginalMethodsOpt);
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.InitialBindingReceiverIsSubjectToCloning, method, arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, node.IsDelegateCall, node.Expanded, node.InvokedAsExtensionMethod, node.ArgsToParamsOpt, node.DefaultArguments, node.ResultKind, originalMethodsOpt, type);
	}

	public override BoundNode? VisitEventAssignmentOperator(BoundEventAssignmentOperator node)
	{
		EventSymbol eventSymbol = VisitEventSymbol(node.Event);
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		BoundExpression argument = (BoundExpression)Visit(node.Argument);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(eventSymbol, node.IsAddition, node.IsDynamic, receiverOpt, argument, type);
	}

	public override BoundNode? VisitAttribute(BoundAttribute node)
	{
		MethodSymbol constructor = VisitMethodSymbol(node.Constructor);
		ImmutableArray<BoundExpression> constructorArguments = VisitList(node.ConstructorArguments);
		ImmutableArray<BoundAssignmentOperator> namedArguments = VisitList(node.NamedArguments);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(constructor, constructorArguments, node.ConstructorArgumentNamesOpt, node.ConstructorArgumentsToParamsOpt, node.ConstructorExpanded, node.ConstructorDefaultArguments, namedArguments, node.ResultKind, type);
	}

	public override BoundNode? VisitUnconvertedObjectCreationExpression(BoundUnconvertedObjectCreationExpression node)
	{
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		VisitType(node.Type);
		return node.Update(arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, node.InitializerOpt, node.Binder);
	}

	public override BoundNode? VisitObjectCreationExpression(BoundObjectCreationExpression node)
	{
		MethodSymbol constructor = VisitMethodSymbol(node.Constructor);
		ImmutableArray<MethodSymbol> constructorsGroup = VisitSymbols(node.ConstructorsGroup);
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		BoundObjectInitializerExpressionBase initializerExpressionOpt = (BoundObjectInitializerExpressionBase)Visit(node.InitializerExpressionOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(constructor, constructorsGroup, arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, node.Expanded, node.ArgsToParamsOpt, node.DefaultArguments, node.ConstantValueOpt, initializerExpressionOpt, node.WasTargetTyped, type);
	}

	public override BoundNode? VisitUnconvertedCollectionExpression(BoundUnconvertedCollectionExpression node)
	{
		ImmutableArray<BoundNode> elements = VisitList(node.Elements);
		VisitType(node.Type);
		return node.Update(elements);
	}

	public override BoundNode? VisitCollectionExpression(BoundCollectionExpression node)
	{
		MethodSymbol collectionBuilderMethod = VisitMethodSymbol(node.CollectionBuilderMethod);
		BoundObjectOrCollectionValuePlaceholder placeholder = node.Placeholder;
		BoundExpression collectionCreation = node.CollectionCreation;
		BoundValuePlaceholder collectionBuilderInvocationPlaceholder = node.CollectionBuilderInvocationPlaceholder;
		BoundExpression collectionBuilderInvocationConversion = node.CollectionBuilderInvocationConversion;
		BoundUnconvertedCollectionExpression unconvertedCollectionExpression = node.UnconvertedCollectionExpression;
		ImmutableArray<BoundNode> elements = VisitList(node.Elements);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.CollectionTypeKind, placeholder, collectionCreation, collectionBuilderMethod, collectionBuilderInvocationPlaceholder, collectionBuilderInvocationConversion, node.WasTargetTyped, unconvertedCollectionExpression, elements, type);
	}

	public override BoundNode? VisitCollectionExpressionSpreadExpressionPlaceholder(BoundCollectionExpressionSpreadExpressionPlaceholder node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitCollectionExpressionSpreadElement(BoundCollectionExpressionSpreadElement node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		BoundCollectionExpressionSpreadExpressionPlaceholder expressionPlaceholder = node.ExpressionPlaceholder;
		BoundExpression conversion = node.Conversion;
		BoundExpression lengthOrCount = node.LengthOrCount;
		BoundValuePlaceholder elementPlaceholder = node.ElementPlaceholder;
		BoundStatement iteratorBody = node.IteratorBody;
		return node.Update(expression, expressionPlaceholder, conversion, node.EnumeratorInfoOpt, lengthOrCount, elementPlaceholder, iteratorBody);
	}

	public override BoundNode? VisitTupleLiteral(BoundTupleLiteral node)
	{
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(arguments, node.ArgumentNamesOpt, node.InferredNamesOpt, type);
	}

	public override BoundNode? VisitConvertedTupleLiteral(BoundConvertedTupleLiteral node)
	{
		BoundTupleLiteral sourceTuple = node.SourceTuple;
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(sourceTuple, node.WasTargetTyped, arguments, node.ArgumentNamesOpt, node.InferredNamesOpt, type);
	}

	public override BoundNode? VisitDynamicObjectCreationExpression(BoundDynamicObjectCreationExpression node)
	{
		ImmutableArray<MethodSymbol> applicableMethods = VisitSymbols(node.ApplicableMethods);
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		BoundObjectInitializerExpressionBase initializerExpressionOpt = (BoundObjectInitializerExpressionBase)Visit(node.InitializerExpressionOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Name, arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, initializerExpressionOpt, applicableMethods, node.WasTargetTyped, type);
	}

	public override BoundNode? VisitNoPiaObjectCreationExpression(BoundNoPiaObjectCreationExpression node)
	{
		BoundObjectInitializerExpressionBase initializerExpressionOpt = (BoundObjectInitializerExpressionBase)Visit(node.InitializerExpressionOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.GuidString, initializerExpressionOpt, node.WasTargetTyped, type);
	}

	public override BoundNode? VisitObjectInitializerExpression(BoundObjectInitializerExpression node)
	{
		BoundObjectOrCollectionValuePlaceholder placeholder = (BoundObjectOrCollectionValuePlaceholder)Visit(node.Placeholder);
		ImmutableArray<BoundExpression> initializers = VisitList(node.Initializers);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(placeholder, initializers, type);
	}

	public override BoundNode? VisitObjectInitializerMember(BoundObjectInitializerMember node)
	{
		Symbol memberSymbol = VisitSymbol(node.MemberSymbol);
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol receiverType = VisitType(node.ReceiverType);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(memberSymbol, arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, node.Expanded, node.ArgsToParamsOpt, node.DefaultArguments, node.ResultKind, node.AccessorKind, receiverType, type);
	}

	public override BoundNode? VisitDynamicObjectInitializerMember(BoundDynamicObjectInitializerMember node)
	{
		TypeSymbol receiverType = VisitType(node.ReceiverType);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.MemberName, receiverType, type);
	}

	public override BoundNode? VisitCollectionInitializerExpression(BoundCollectionInitializerExpression node)
	{
		BoundObjectOrCollectionValuePlaceholder placeholder = (BoundObjectOrCollectionValuePlaceholder)Visit(node.Placeholder);
		ImmutableArray<BoundExpression> initializers = VisitList(node.Initializers);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(placeholder, initializers, type);
	}

	public override BoundNode? VisitCollectionElementInitializer(BoundCollectionElementInitializer node)
	{
		MethodSymbol addMethod = VisitMethodSymbol(node.AddMethod);
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		BoundExpression implicitReceiverOpt = (BoundExpression)Visit(node.ImplicitReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(addMethod, arguments, implicitReceiverOpt, node.Expanded, node.ArgsToParamsOpt, node.DefaultArguments, node.InvokedAsExtensionMethod, node.ResultKind, type);
	}

	public override BoundNode? VisitDynamicCollectionElementInitializer(BoundDynamicCollectionElementInitializer node)
	{
		ImmutableArray<MethodSymbol> applicableMethods = VisitSymbols(node.ApplicableMethods);
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(applicableMethods, expression, arguments, type);
	}

	public override BoundNode? VisitImplicitReceiver(BoundImplicitReceiver node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitAnonymousObjectCreationExpression(BoundAnonymousObjectCreationExpression node)
	{
		MethodSymbol constructor = VisitMethodSymbol(node.Constructor);
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		ImmutableArray<BoundAnonymousPropertyDeclaration> declarations = VisitList(node.Declarations);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(constructor, arguments, declarations, type);
	}

	public override BoundNode? VisitAnonymousPropertyDeclaration(BoundAnonymousPropertyDeclaration node)
	{
		PropertySymbol property = VisitPropertySymbol(node.Property);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(property, type);
	}

	public override BoundNode? VisitNewT(BoundNewT node)
	{
		BoundObjectInitializerExpressionBase initializerExpressionOpt = (BoundObjectInitializerExpressionBase)Visit(node.InitializerExpressionOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(initializerExpressionOpt, node.WasTargetTyped, type);
	}

	public override BoundNode? VisitDelegateCreationExpression(BoundDelegateCreationExpression node)
	{
		MethodSymbol methodOpt = VisitMethodSymbol(node.MethodOpt);
		BoundExpression argument = (BoundExpression)Visit(node.Argument);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(argument, methodOpt, node.IsExtensionMethod, node.WasTargetTyped, type);
	}

	public override BoundNode? VisitArrayCreation(BoundArrayCreation node)
	{
		ImmutableArray<BoundExpression> bounds = VisitList(node.Bounds);
		BoundArrayInitialization initializerOpt = (BoundArrayInitialization)Visit(node.InitializerOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(bounds, initializerOpt, type);
	}

	public override BoundNode? VisitArrayInitialization(BoundArrayInitialization node)
	{
		ImmutableArray<BoundExpression> initializers = VisitList(node.Initializers);
		VisitType(node.Type);
		return node.Update(node.IsInferred, initializers);
	}

	public override BoundNode? VisitStackAllocArrayCreation(BoundStackAllocArrayCreation node)
	{
		BoundExpression count = (BoundExpression)Visit(node.Count);
		BoundArrayInitialization initializerOpt = (BoundArrayInitialization)Visit(node.InitializerOpt);
		TypeSymbol elementType = VisitType(node.ElementType);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(elementType, count, initializerOpt, type);
	}

	public override BoundNode? VisitConvertedStackAllocExpression(BoundConvertedStackAllocExpression node)
	{
		BoundExpression count = (BoundExpression)Visit(node.Count);
		BoundArrayInitialization initializerOpt = (BoundArrayInitialization)Visit(node.InitializerOpt);
		TypeSymbol elementType = VisitType(node.ElementType);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(elementType, count, initializerOpt, type);
	}

	public override BoundNode? VisitFieldAccess(BoundFieldAccess node)
	{
		FieldSymbol fieldSymbol = VisitFieldSymbol(node.FieldSymbol);
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, fieldSymbol, node.ConstantValueOpt, node.ResultKind, node.IsByValue, node.IsDeclaration, type);
	}

	public override BoundNode? VisitHoistedFieldAccess(BoundHoistedFieldAccess node)
	{
		FieldSymbol fieldSymbol = VisitFieldSymbol(node.FieldSymbol);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(fieldSymbol, type);
	}

	public override BoundNode? VisitPropertyAccess(BoundPropertyAccess node)
	{
		PropertySymbol propertySymbol = VisitPropertySymbol(node.PropertySymbol);
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.InitialBindingReceiverIsSubjectToCloning, propertySymbol, node.AutoPropertyAccessorKind, node.ResultKind, type);
	}

	public override BoundNode? VisitEventAccess(BoundEventAccess node)
	{
		EventSymbol eventSymbol = VisitEventSymbol(node.EventSymbol);
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, eventSymbol, node.IsUsableAsField, node.ResultKind, type);
	}

	public override BoundNode? VisitIndexerAccess(BoundIndexerAccess node)
	{
		PropertySymbol indexer = VisitPropertySymbol(node.Indexer);
		ImmutableArray<PropertySymbol> originalIndexersOpt = VisitSymbols(node.OriginalIndexersOpt);
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.InitialBindingReceiverIsSubjectToCloning, indexer, arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, node.Expanded, node.AccessorKind, node.ArgsToParamsOpt, node.DefaultArguments, originalIndexersOpt, type);
	}

	public override BoundNode? VisitImplicitIndexerAccess(BoundImplicitIndexerAccess node)
	{
		BoundExpression receiver = (BoundExpression)Visit(node.Receiver);
		BoundExpression argument = (BoundExpression)Visit(node.Argument);
		BoundExpression lengthOrCountAccess = node.LengthOrCountAccess;
		BoundImplicitIndexerReceiverPlaceholder receiverPlaceholder = node.ReceiverPlaceholder;
		BoundExpression indexerOrSliceAccess = node.IndexerOrSliceAccess;
		ImmutableArray<BoundImplicitIndexerValuePlaceholder> argumentPlaceholders = node.ArgumentPlaceholders;
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiver, argument, lengthOrCountAccess, receiverPlaceholder, indexerOrSliceAccess, argumentPlaceholders, type);
	}

	public override BoundNode? VisitInlineArrayAccess(BoundInlineArrayAccess node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		BoundExpression argument = (BoundExpression)Visit(node.Argument);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, argument, node.IsValue, node.GetItemOrSliceHelper, type);
	}

	public override BoundNode? VisitDynamicIndexerAccess(BoundDynamicIndexerAccess node)
	{
		ImmutableArray<PropertySymbol> applicableIndexers = VisitSymbols(node.ApplicableIndexers);
		BoundExpression receiver = (BoundExpression)Visit(node.Receiver);
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiver, arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, applicableIndexers, type);
	}

	public override BoundNode? VisitLambda(BoundLambda node)
	{
		MethodSymbol symbol = VisitMethodSymbol(node.Symbol);
		UnboundLambda unboundLambda = node.UnboundLambda;
		BoundBlock body = (BoundBlock)Visit(node.Body);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(unboundLambda, symbol, body, node.Diagnostics, node.Binder, type);
	}

	public override BoundNode? VisitUnboundLambda(UnboundLambda node)
	{
		FunctionTypeSymbol functionType = VisitFunctionTypeSymbol(node.FunctionType);
		VisitType(node.Type);
		return node.Update(node.Data, functionType, node.WithDependencies);
	}

	public override BoundNode? VisitQueryClause(BoundQueryClause node)
	{
		RangeVariableSymbol definedSymbol = VisitRangeVariableSymbol(node.DefinedSymbol);
		BoundExpression value = (BoundExpression)Visit(node.Value);
		BoundExpression operation = node.Operation;
		BoundExpression cast = node.Cast;
		BoundExpression unoptimizedForm = node.UnoptimizedForm;
		TypeSymbol type = VisitType(node.Type);
		return node.Update(value, definedSymbol, operation, cast, node.Binder, unoptimizedForm, type);
	}

	public override BoundNode? VisitTypeOrInstanceInitializers(BoundTypeOrInstanceInitializers node)
	{
		ImmutableArray<BoundStatement> statements = VisitList(node.Statements);
		return node.Update(statements);
	}

	public override BoundNode? VisitNameOfOperator(BoundNameOfOperator node)
	{
		BoundExpression argument = (BoundExpression)Visit(node.Argument);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(argument, node.ConstantValueOpt, type);
	}

	public override BoundNode? VisitUnconvertedInterpolatedString(BoundUnconvertedInterpolatedString node)
	{
		ImmutableArray<BoundExpression> parts = VisitList(node.Parts);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(parts, node.ConstantValueOpt, type);
	}

	public override BoundNode? VisitInterpolatedString(BoundInterpolatedString node)
	{
		ImmutableArray<BoundExpression> parts = VisitList(node.Parts);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.InterpolationData, parts, node.ConstantValueOpt, type);
	}

	public override BoundNode? VisitInterpolatedStringHandlerPlaceholder(BoundInterpolatedStringHandlerPlaceholder node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode? VisitInterpolatedStringArgumentPlaceholder(BoundInterpolatedStringArgumentPlaceholder node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.ArgumentIndex, type);
	}

	public override BoundNode? VisitStringInsert(BoundStringInsert node)
	{
		BoundExpression value = (BoundExpression)Visit(node.Value);
		BoundExpression alignment = (BoundExpression)Visit(node.Alignment);
		BoundLiteral format = (BoundLiteral)Visit(node.Format);
		VisitType(node.Type);
		return node.Update(value, alignment, format, node.IsInterpolatedStringHandlerAppendCall);
	}

	public override BoundNode? VisitIsPatternExpression(BoundIsPatternExpression node)
	{
		LabelSymbol whenTrueLabel = VisitLabelSymbol(node.WhenTrueLabel);
		LabelSymbol whenFalseLabel = VisitLabelSymbol(node.WhenFalseLabel);
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		BoundPattern pattern = (BoundPattern)Visit(node.Pattern);
		BoundDecisionDag reachabilityDecisionDag = node.ReachabilityDecisionDag;
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, pattern, node.IsNegated, reachabilityDecisionDag, whenTrueLabel, whenFalseLabel, type);
	}

	public override BoundNode? VisitConstantPattern(BoundConstantPattern node)
	{
		BoundExpression value = (BoundExpression)Visit(node.Value);
		TypeSymbol inputType = VisitType(node.InputType);
		TypeSymbol narrowedType = VisitType(node.NarrowedType);
		return node.Update(value, node.ConstantValue, inputType, narrowedType);
	}

	public override BoundNode? VisitDiscardPattern(BoundDiscardPattern node)
	{
		TypeSymbol inputType = VisitType(node.InputType);
		TypeSymbol narrowedType = VisitType(node.NarrowedType);
		return node.Update(inputType, narrowedType);
	}

	public override BoundNode? VisitDeclarationPattern(BoundDeclarationPattern node)
	{
		Symbol variable = VisitSymbol(node.Variable);
		BoundTypeExpression declaredType = (BoundTypeExpression)Visit(node.DeclaredType);
		BoundExpression variableAccess = (BoundExpression)Visit(node.VariableAccess);
		TypeSymbol inputType = VisitType(node.InputType);
		TypeSymbol narrowedType = VisitType(node.NarrowedType);
		return node.Update(declaredType, node.IsVar, variable, variableAccess, inputType, narrowedType);
	}

	public override BoundNode? VisitRecursivePattern(BoundRecursivePattern node)
	{
		MethodSymbol deconstructMethod = VisitMethodSymbol(node.DeconstructMethod);
		Symbol variable = VisitSymbol(node.Variable);
		BoundTypeExpression declaredType = (BoundTypeExpression)Visit(node.DeclaredType);
		ImmutableArray<BoundPositionalSubpattern> deconstruction = VisitList(node.Deconstruction);
		ImmutableArray<BoundPropertySubpattern> properties = VisitList(node.Properties);
		BoundExpression variableAccess = (BoundExpression)Visit(node.VariableAccess);
		TypeSymbol inputType = VisitType(node.InputType);
		TypeSymbol narrowedType = VisitType(node.NarrowedType);
		return node.Update(declaredType, deconstructMethod, deconstruction, properties, node.IsExplicitNotNullTest, variable, variableAccess, inputType, narrowedType);
	}

	public override BoundNode? VisitListPattern(BoundListPattern node)
	{
		Symbol variable = VisitSymbol(node.Variable);
		ImmutableArray<BoundPattern> subpatterns = VisitList(node.Subpatterns);
		BoundExpression lengthAccess = node.LengthAccess;
		BoundExpression indexerAccess = node.IndexerAccess;
		BoundListPatternReceiverPlaceholder receiverPlaceholder = node.ReceiverPlaceholder;
		BoundListPatternIndexPlaceholder argumentPlaceholder = node.ArgumentPlaceholder;
		BoundExpression variableAccess = (BoundExpression)Visit(node.VariableAccess);
		TypeSymbol inputType = VisitType(node.InputType);
		TypeSymbol narrowedType = VisitType(node.NarrowedType);
		return node.Update(subpatterns, node.HasSlice, lengthAccess, indexerAccess, receiverPlaceholder, argumentPlaceholder, variable, variableAccess, inputType, narrowedType);
	}

	public override BoundNode? VisitSlicePattern(BoundSlicePattern node)
	{
		BoundPattern pattern = (BoundPattern)Visit(node.Pattern);
		BoundExpression indexerAccess = node.IndexerAccess;
		BoundSlicePatternReceiverPlaceholder receiverPlaceholder = node.ReceiverPlaceholder;
		BoundSlicePatternRangePlaceholder argumentPlaceholder = node.ArgumentPlaceholder;
		TypeSymbol inputType = VisitType(node.InputType);
		TypeSymbol narrowedType = VisitType(node.NarrowedType);
		return node.Update(pattern, indexerAccess, receiverPlaceholder, argumentPlaceholder, inputType, narrowedType);
	}

	public override BoundNode? VisitITuplePattern(BoundITuplePattern node)
	{
		MethodSymbol getLengthMethod = VisitMethodSymbol(node.GetLengthMethod);
		MethodSymbol getItemMethod = VisitMethodSymbol(node.GetItemMethod);
		ImmutableArray<BoundPositionalSubpattern> subpatterns = VisitList(node.Subpatterns);
		TypeSymbol inputType = VisitType(node.InputType);
		TypeSymbol narrowedType = VisitType(node.NarrowedType);
		return node.Update(getLengthMethod, getItemMethod, subpatterns, inputType, narrowedType);
	}

	public override BoundNode? VisitPositionalSubpattern(BoundPositionalSubpattern node)
	{
		Symbol symbol = VisitSymbol(node.Symbol);
		BoundPattern pattern = (BoundPattern)Visit(node.Pattern);
		return node.Update(symbol, pattern);
	}

	public override BoundNode? VisitPropertySubpattern(BoundPropertySubpattern node)
	{
		BoundPropertySubpatternMember member = (BoundPropertySubpatternMember)Visit(node.Member);
		BoundPattern pattern = (BoundPattern)Visit(node.Pattern);
		return node.Update(member, node.IsLengthOrCount, pattern);
	}

	public override BoundNode? VisitPropertySubpatternMember(BoundPropertySubpatternMember node)
	{
		Symbol symbol = VisitSymbol(node.Symbol);
		BoundPropertySubpatternMember receiver = (BoundPropertySubpatternMember)Visit(node.Receiver);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiver, symbol, type);
	}

	public override BoundNode? VisitTypePattern(BoundTypePattern node)
	{
		BoundTypeExpression declaredType = (BoundTypeExpression)Visit(node.DeclaredType);
		TypeSymbol inputType = VisitType(node.InputType);
		TypeSymbol narrowedType = VisitType(node.NarrowedType);
		return node.Update(declaredType, node.IsExplicitNotNullTest, inputType, narrowedType);
	}

	public override BoundNode? VisitBinaryPattern(BoundBinaryPattern node)
	{
		BoundPattern left = (BoundPattern)Visit(node.Left);
		BoundPattern right = (BoundPattern)Visit(node.Right);
		TypeSymbol inputType = VisitType(node.InputType);
		TypeSymbol narrowedType = VisitType(node.NarrowedType);
		return node.Update(node.Disjunction, left, right, inputType, narrowedType);
	}

	public override BoundNode? VisitNegatedPattern(BoundNegatedPattern node)
	{
		BoundPattern negated = (BoundPattern)Visit(node.Negated);
		TypeSymbol inputType = VisitType(node.InputType);
		TypeSymbol narrowedType = VisitType(node.NarrowedType);
		return node.Update(negated, inputType, narrowedType);
	}

	public override BoundNode? VisitRelationalPattern(BoundRelationalPattern node)
	{
		BoundExpression value = (BoundExpression)Visit(node.Value);
		TypeSymbol inputType = VisitType(node.InputType);
		TypeSymbol narrowedType = VisitType(node.NarrowedType);
		return node.Update(node.Relation, value, node.ConstantValue, inputType, narrowedType);
	}

	public override BoundNode? VisitDiscardExpression(BoundDiscardExpression node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.NullableAnnotation, node.IsInferred, type);
	}

	public override BoundNode? VisitThrowExpression(BoundThrowExpression node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, type);
	}

	public override BoundNode? VisitOutVariablePendingInference(OutVariablePendingInference node)
	{
		Symbol variableSymbol = VisitSymbol(node.VariableSymbol);
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		VisitType(node.Type);
		return node.Update(variableSymbol, receiverOpt);
	}

	public override BoundNode? VisitDeconstructionVariablePendingInference(DeconstructionVariablePendingInference node)
	{
		Symbol variableSymbol = VisitSymbol(node.VariableSymbol);
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		VisitType(node.Type);
		return node.Update(variableSymbol, receiverOpt);
	}

	public override BoundNode? VisitOutDeconstructVarPendingInference(OutDeconstructVarPendingInference node)
	{
		Symbol variableSymbol = VisitSymbol(node.VariableSymbol);
		VisitType(node.Type);
		return node.Update(variableSymbol, node.IsDiscardExpression);
	}

	public override BoundNode? VisitNonConstructorMethodBody(BoundNonConstructorMethodBody node)
	{
		BoundBlock blockBody = (BoundBlock)Visit(node.BlockBody);
		BoundBlock expressionBody = (BoundBlock)Visit(node.ExpressionBody);
		return node.Update(blockBody, expressionBody);
	}

	public override BoundNode? VisitConstructorMethodBody(BoundConstructorMethodBody node)
	{
		ImmutableArray<LocalSymbol> locals = VisitLocals(node.Locals);
		BoundStatement initializer = (BoundStatement)Visit(node.Initializer);
		BoundBlock blockBody = (BoundBlock)Visit(node.BlockBody);
		BoundBlock expressionBody = (BoundBlock)Visit(node.ExpressionBody);
		return node.Update(locals, initializer, blockBody, expressionBody);
	}

	public override BoundNode? VisitExpressionWithNullability(BoundExpressionWithNullability node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, node.NullableAnnotation, type);
	}

	public override BoundNode? VisitWithExpression(BoundWithExpression node)
	{
		MethodSymbol cloneMethod = VisitMethodSymbol(node.CloneMethod);
		BoundExpression receiver = (BoundExpression)Visit(node.Receiver);
		BoundObjectInitializerExpressionBase initializerExpression = (BoundObjectInitializerExpressionBase)Visit(node.InitializerExpression);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiver, cloneMethod, initializerExpression, type);
	}
}
