using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class MethodToClassRewriter : BoundTreeToDifferentEnclosingContextRewriter
{
	internal sealed class BaseMethodWrapperSymbol : SynthesizedMethodBaseSymbol
	{
		internal sealed override bool GenerateDebugInfo => false;

		internal override bool SynthesizesLoweredBoundBody => true;

		internal override ExecutableCodeBinder? TryGetBodyBinder(BinderFactory? binderFactoryOpt = null, bool ignoreAccessibility = false)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Compiler/MethodBodySynthesizer.Lowered.cs", 311);
		}

		internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
		{
			SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, this.GetNonNullSyntaxNode(), compilationState, diagnostics);
			syntheticBoundNodeFactory.CurrentFunction = OriginalDefinition;
			try
			{
				MethodSymbol methodSymbol = BaseMethod;
				if (Arity > 0)
				{
					methodSymbol = methodSymbol.ConstructedFrom.Construct(StaticCast<TypeSymbol>.From(TypeParameters));
				}
				BoundBlock boundBlock = MethodBodySynthesizer.ConstructSingleInvocationMethodBody(syntheticBoundNodeFactory, methodSymbol, useBaseReference: true);
				if (boundBlock.Kind != BoundKind.Block)
				{
					boundBlock = syntheticBoundNodeFactory.Block(boundBlock);
				}
				syntheticBoundNodeFactory.CompilationState.AddMethodWrapper(methodSymbol, this, boundBlock);
			}
			catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
			{
				diagnostics.Add(missingPredefinedMember.Diagnostic);
			}
		}

		internal BaseMethodWrapperSymbol(NamedTypeSymbol containingType, MethodSymbol methodBeingWrapped, SyntaxNode syntax, string name)
			: base(containingType, methodBeingWrapped, syntax.SyntaxTree.GetReference(syntax), syntax.GetLocation(), name, DeclarationModifiers.Private, isIterator: false)
		{
			TypeMap typeMap = ((methodBeingWrapped.ContainingType is SubstitutedNamedTypeSymbol substitutedNamedTypeSymbol) ? substitutedNamedTypeSymbol.TypeSubstitution : TypeMap.Empty);
			ImmutableArray<TypeParameterSymbol> newTypeParameters;
			if (!methodBeingWrapped.IsGenericMethod)
			{
				newTypeParameters = ImmutableArray<TypeParameterSymbol>.Empty;
			}
			else
			{
				typeMap = typeMap.WithAlphaRename(methodBeingWrapped, this, propagateAttributes: false, out newTypeParameters);
			}
			AssignTypeMapAndTypeParameters(typeMap, newTypeParameters);
		}
	}

	protected Dictionary<Symbol, CapturedSymbolReplacement> proxies = new Dictionary<Symbol, CapturedSymbolReplacement>();

	protected readonly TypeCompilationState CompilationState;

	protected readonly BindingDiagnosticBag Diagnostics;

	protected readonly VariableSlotAllocator? slotAllocator;

	protected abstract NamedTypeSymbol ContainingType { get; }

	protected abstract BoundExpression FramePointer(SyntaxNode syntax, NamedTypeSymbol frameClass);

	protected MethodToClassRewriter(VariableSlotAllocator? slotAllocator, TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		CompilationState = compilationState;
		Diagnostics = diagnostics;
		this.slotAllocator = slotAllocator;
	}

	protected abstract bool NeedsProxy(Symbol localOrParameter);

	protected sealed override bool TryRewriteLocal(LocalSymbol local, [NotNullWhen(true)] out LocalSymbol? newLocal)
	{
		if (NeedsProxy(local))
		{
			newLocal = null;
			return false;
		}
		return base.TryRewriteLocal(local, out newLocal);
	}

	public abstract override BoundNode VisitScope(BoundScope node);

	public override BoundNode VisitForStatement(BoundForStatement node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/MethodToClassRewriter.cs", 71);
	}

	public override BoundNode VisitDoStatement(BoundDoStatement node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/MethodToClassRewriter.cs", 76);
	}

	public override BoundNode VisitWhileStatement(BoundWhileStatement node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/MethodToClassRewriter.cs", 81);
	}

	public override BoundNode VisitUsingStatement(BoundUsingStatement node)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/MethodToClassRewriter.cs", 86);
	}

	public override BoundNode VisitPropertyAccess(BoundPropertyAccess node)
	{
		PropertySymbol propertySymbol = VisitPropertySymbol(node.PropertySymbol);
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		return node.Update(receiverOpt, ThreeState.Unknown, propertySymbol, node.AutoPropertyAccessorKind, node.ResultKind, VisitType(node.Type));
	}

	public override BoundNode VisitCall(BoundCall node)
	{
		MethodSymbol methodSymbol = VisitMethodSymbol(node.Method);
		BoundExpression boundExpression = (BoundExpression)Visit(node.ReceiverOpt);
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol type = VisitType(node.Type);
		if (BaseReferenceInReceiverWasRewritten(node.ReceiverOpt, boundExpression) && node.Method.IsMetadataVirtual())
		{
			methodSymbol = GetMethodWrapperForBaseNonVirtualCall(methodSymbol, node.Syntax);
		}
		return node.Update(boundExpression, ThreeState.Unknown, methodSymbol, arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, node.IsDelegateCall, node.Expanded, node.InvokedAsExtensionMethod, node.ArgsToParamsOpt, node.DefaultArguments, node.ResultKind, type);
	}

	private MethodSymbol GetMethodWrapperForBaseNonVirtualCall(MethodSymbol methodBeingCalled, SyntaxNode syntax)
	{
		MethodSymbol orCreateBaseFunctionWrapper = GetOrCreateBaseFunctionWrapper(methodBeingCalled, syntax);
		if (!orCreateBaseFunctionWrapper.IsGenericMethod)
		{
			return orCreateBaseFunctionWrapper;
		}
		ImmutableArray<TypeWithAnnotations> typeArgumentsWithAnnotations = methodBeingCalled.TypeArgumentsWithAnnotations;
		ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance(typeArgumentsWithAnnotations.Length);
		foreach (TypeWithAnnotations item in typeArgumentsWithAnnotations)
		{
			instance.Add(item.WithTypeAndModifiers(VisitType(item.Type), item.CustomModifiers));
		}
		return orCreateBaseFunctionWrapper.Construct(instance.ToImmutableAndFree());
	}

	private MethodSymbol GetOrCreateBaseFunctionWrapper(MethodSymbol methodBeingWrapped, SyntaxNode syntax)
	{
		methodBeingWrapped = methodBeingWrapped.ConstructedFrom;
		MethodSymbol methodWrapper = CompilationState.GetMethodWrapper(methodBeingWrapped);
		if ((object)methodWrapper != null)
		{
			return methodWrapper;
		}
		NamedTypeSymbol containingType = ContainingType;
		string name = GeneratedNames.MakeBaseMethodWrapperName(CompilationState.NextWrapperMethodIndex);
		methodWrapper = new BaseMethodWrapperSymbol(containingType, methodBeingWrapped, syntax, name);
		if (CompilationState.Emitting)
		{
			CompilationState.ModuleBuilderOpt.AddSynthesizedDefinition(containingType, methodWrapper.GetCciAdapter());
		}
		methodWrapper.GenerateMethodBody(CompilationState, Diagnostics);
		return methodWrapper;
	}

	private bool TryReplaceWithProxy(Symbol parameterOrLocal, SyntaxNode syntax, [NotNullWhen(true)] out BoundNode? replacement)
	{
		if (proxies.TryGetValue(parameterOrLocal, out CapturedSymbolReplacement value))
		{
			replacement = value.Replacement(syntax, (NamedTypeSymbol frameType, (SyntaxNode syntax, MethodToClassRewriter self) arg) => arg.self.FramePointer(arg.syntax, frameType), (syntax, this));
			return true;
		}
		replacement = null;
		return false;
	}

	public sealed override BoundNode VisitParameter(BoundParameter node)
	{
		if (TryReplaceWithProxy(node.ParameterSymbol, node.Syntax, out BoundNode replacement))
		{
			return replacement;
		}
		return VisitUnhoistedParameter(node);
	}

	protected virtual BoundNode VisitUnhoistedParameter(BoundParameter node)
	{
		return base.VisitParameter(node);
	}

	public sealed override BoundNode VisitLocal(BoundLocal node)
	{
		if (TryReplaceWithProxy(node.LocalSymbol, node.Syntax, out BoundNode replacement))
		{
			return replacement;
		}
		return base.VisitLocal(node);
	}

	public override BoundNode? VisitLocalId(BoundLocalId node)
	{
		if (!TryGetHoistedField(node.Local, out FieldSymbol field))
		{
			return base.VisitLocalId(node);
		}
		return node.Update(node.Local, field, node.Type);
	}

	public override BoundNode? VisitParameterId(BoundParameterId node)
	{
		if (!TryGetHoistedField(node.Parameter, out FieldSymbol field))
		{
			return base.VisitParameterId(node);
		}
		return node.Update(node.Parameter, field, node.Type);
	}

	private bool TryGetHoistedField(Symbol variable, [NotNullWhen(true)] out FieldSymbol? field)
	{
		if (proxies.TryGetValue(variable, out CapturedSymbolReplacement value))
		{
			FieldSymbol hoistedField;
			if (!(value is CapturedToStateMachineFieldReplacement capturedToStateMachineFieldReplacement))
			{
				if (!(value is CapturedToFrameSymbolReplacement capturedToFrameSymbolReplacement))
				{
					throw ExceptionUtilities.UnexpectedValue(value);
				}
				hoistedField = capturedToFrameSymbolReplacement.HoistedField;
			}
			else
			{
				hoistedField = capturedToStateMachineFieldReplacement.HoistedField;
			}
			field = hoistedField;
			return true;
		}
		field = null;
		return false;
	}

	public override BoundNode VisitAssignmentOperator(BoundAssignmentOperator node)
	{
		BoundExpression left = node.Left;
		if (left.Kind != BoundKind.Local)
		{
			return base.VisitAssignmentOperator(node);
		}
		BoundLocal boundLocal = (BoundLocal)left;
		BoundExpression right = node.Right;
		if (boundLocal.LocalSymbol.RefKind != RefKind.None && node.IsRef && NeedsProxy(boundLocal.LocalSymbol))
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/MethodToClassRewriter.cs", 275);
		}
		if (NeedsProxy(boundLocal.LocalSymbol) && !proxies.ContainsKey(boundLocal.LocalSymbol))
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/MethodToClassRewriter.cs", 282);
		}
		BoundExpression boundExpression = (BoundExpression)Visit(boundLocal);
		BoundExpression boundExpression2 = (BoundExpression)Visit(right);
		TypeSymbol type = VisitType(node.Type);
		if (boundExpression.Kind != BoundKind.Local && right.Kind == BoundKind.ConvertedStackAllocExpression)
		{
			BoundLocal boundLocal2 = new SyntheticBoundNodeFactory(CurrentMethod, boundExpression.Syntax, CompilationState, Diagnostics).StoreToTemp(boundExpression2, out BoundAssignmentOperator store);
			BoundAssignmentOperator value = node.Update(boundExpression, boundLocal2, node.IsRef, type);
			return new BoundSequence(node.Syntax, ImmutableArray.Create(boundLocal2.LocalSymbol), ImmutableArray.Create((BoundExpression)store), value, type);
		}
		return node.Update(boundExpression, boundExpression2, node.IsRef, type);
	}

	public override BoundNode VisitFieldInfo(BoundFieldInfo node)
	{
		FieldSymbol field = node.Field.OriginalDefinition.AsMember((NamedTypeSymbol)VisitType(node.Field.ContainingType));
		return node.Update(field, node.GetFieldFromHandle, node.Type);
	}

	public override BoundNode VisitFieldAccess(BoundFieldAccess node)
	{
		BoundExpression receiver = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol typeSymbol = VisitType(node.Type);
		FieldSymbol fieldSymbol = node.FieldSymbol.OriginalDefinition.AsMember((NamedTypeSymbol)VisitType(node.FieldSymbol.ContainingType));
		return node.Update(receiver, fieldSymbol, node.ConstantValueOpt, node.ResultKind, typeSymbol);
	}

	public override BoundNode VisitDelegateCreationExpression(BoundDelegateCreationExpression node)
	{
		BoundExpression argument = node.Argument;
		BoundExpression boundExpression = (BoundExpression)Visit(argument);
		MethodSymbol methodSymbol = node.MethodOpt;
		if (BaseReferenceInReceiverWasRewritten(argument, boundExpression) && methodSymbol.IsMetadataVirtual())
		{
			methodSymbol = GetMethodWrapperForBaseNonVirtualCall(methodSymbol, argument.Syntax);
		}
		methodSymbol = VisitMethodSymbol(methodSymbol);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(boundExpression, methodSymbol, node.IsExtensionMethod, node.WasTargetTyped, type);
	}

	public override BoundNode VisitObjectInitializerMember(BoundObjectInitializerMember node)
	{
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol type = VisitType(node.Type);
		TypeSymbol receiverType = VisitType(node.ReceiverType);
		Symbol symbol = node.MemberSymbol;
		switch (symbol.Kind)
		{
		case SymbolKind.Field:
			symbol = VisitFieldSymbol((FieldSymbol)symbol);
			break;
		case SymbolKind.Property:
			symbol = VisitPropertySymbol((PropertySymbol)symbol);
			break;
		}
		return node.Update(symbol, arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, node.Expanded, node.ArgsToParamsOpt, node.DefaultArguments, node.ResultKind, node.AccessorKind, receiverType, type);
	}

	private static bool BaseReferenceInReceiverWasRewritten([NotNullWhen(true)] BoundExpression? originalReceiver, [NotNullWhen(true)] BoundExpression? rewrittenReceiver)
	{
		if (originalReceiver != null && originalReceiver.Kind == BoundKind.BaseReference)
		{
			if (rewrittenReceiver != null)
			{
				return rewrittenReceiver.Kind != BoundKind.BaseReference;
			}
			return false;
		}
		return false;
	}
}
