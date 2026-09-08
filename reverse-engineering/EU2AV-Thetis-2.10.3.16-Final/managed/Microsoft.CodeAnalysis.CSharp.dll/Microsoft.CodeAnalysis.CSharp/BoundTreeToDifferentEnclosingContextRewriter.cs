using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundTreeToDifferentEnclosingContextRewriter : BoundTreeRewriterWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator
{
	private readonly Dictionary<LocalSymbol, LocalSymbol> localMap = new Dictionary<LocalSymbol, LocalSymbol>();

	private readonly Dictionary<BoundValuePlaceholderBase, BoundValuePlaceholderBase> _placeholderMap = new Dictionary<BoundValuePlaceholderBase, BoundValuePlaceholderBase>();

	protected abstract TypeMap TypeMap { get; }

	protected abstract MethodSymbol CurrentMethod { get; }

	protected abstract bool EnforceAccurateContainerForLocals { get; }

	public override BoundNode DefaultVisit(BoundNode node)
	{
		return base.DefaultVisit(node);
	}

	protected void RewriteLocals(ImmutableArray<LocalSymbol> locals, ArrayBuilder<LocalSymbol> newLocals)
	{
		foreach (LocalSymbol item in locals)
		{
			if (TryRewriteLocal(item, out LocalSymbol newLocal))
			{
				newLocals.Add(newLocal);
			}
		}
	}

	protected virtual bool TryRewriteLocal(LocalSymbol local, [NotNullWhen(true)] out LocalSymbol? newLocal)
	{
		if (localMap.TryGetValue(local, out newLocal))
		{
			return true;
		}
		TypeSymbol typeSymbol = VisitType(local.Type);
		if (TypeSymbol.Equals(typeSymbol, local.Type, TypeCompareKind.ConsiderEverything) && (!EnforceAccurateContainerForLocals || local.ContainingSymbol == CurrentMethod))
		{
			newLocal = local;
		}
		else
		{
			newLocal = new TypeSubstitutedLocalSymbol(local, TypeWithAnnotations.Create(typeSymbol), CurrentMethod);
			localMap.Add(local, newLocal);
		}
		return true;
	}

	protected sealed override ImmutableArray<LocalSymbol> VisitLocals(ImmutableArray<LocalSymbol> locals)
	{
		if (locals.IsEmpty)
		{
			return locals;
		}
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		RewriteLocals(locals, instance);
		return instance.ToImmutableAndFree();
	}

	public sealed override LocalSymbol VisitLocalSymbol(LocalSymbol local)
	{
		if (!TryRewriteLocal(local, out LocalSymbol newLocal))
		{
			throw ExceptionUtilities.UnexpectedValue(local);
		}
		return newLocal;
	}

	protected bool TryGetRewrittenLocal(LocalSymbol local, [NotNullWhen(true)] out LocalSymbol? localToUse)
	{
		return localMap.TryGetValue(local, out localToUse);
	}

	public override BoundNode VisitBlock(BoundBlock node)
	{
		return VisitBlock(node, removeInstrumentation: false);
	}

	protected BoundBlock VisitBlock(BoundBlock node, bool removeInstrumentation)
	{
		ImmutableArray<LocalSymbol> locals = VisitLocals(node.Locals);
		ImmutableArray<MethodSymbol> localFunctions = VisitDeclaredLocalFunctions(node.LocalFunctions);
		ImmutableArray<BoundStatement> statements = VisitList(node.Statements);
		BoundBlockInstrumentation instrumentation = (removeInstrumentation ? null : ((BoundBlockInstrumentation)Visit(node.Instrumentation)));
		return node.Update(locals, localFunctions, node.HasUnsafeModifier, instrumentation, statements);
	}

	[return: NotNullIfNotNull("type")]
	public sealed override TypeSymbol? VisitType(TypeSymbol? type)
	{
		return TypeMap.SubstituteType(type).Type;
	}

	public override BoundNode VisitAwaitableInfo(BoundAwaitableInfo node)
	{
		BoundAwaitableValuePlaceholder awaitableInstancePlaceholder = node.AwaitableInstancePlaceholder;
		if (awaitableInstancePlaceholder == null)
		{
			return node;
		}
		BoundAwaitableValuePlaceholder boundAwaitableValuePlaceholder = awaitableInstancePlaceholder.Update(VisitType(awaitableInstancePlaceholder.Type));
		_placeholderMap.Add(awaitableInstancePlaceholder, boundAwaitableValuePlaceholder);
		BoundExpression getAwaiter = (BoundExpression)Visit(node.GetAwaiter);
		PropertySymbol isCompleted = VisitPropertySymbol(node.IsCompleted);
		MethodSymbol getResult = VisitMethodSymbol(node.GetResult);
		_placeholderMap.Remove(awaitableInstancePlaceholder);
		BoundCall runtimeAsyncAwaitCall = null;
		BoundAwaitableValuePlaceholder runtimeAsyncAwaitCallPlaceholder = node.RuntimeAsyncAwaitCallPlaceholder;
		BoundAwaitableValuePlaceholder boundAwaitableValuePlaceholder2 = runtimeAsyncAwaitCallPlaceholder;
		if (boundAwaitableValuePlaceholder2 != null)
		{
			boundAwaitableValuePlaceholder2 = runtimeAsyncAwaitCallPlaceholder.Update(VisitType(runtimeAsyncAwaitCallPlaceholder.Type));
			_placeholderMap.Add(runtimeAsyncAwaitCallPlaceholder, boundAwaitableValuePlaceholder2);
			runtimeAsyncAwaitCall = (BoundCall)Visit(node.RuntimeAsyncAwaitCall);
			_placeholderMap.Remove(runtimeAsyncAwaitCallPlaceholder);
		}
		return node.Update(boundAwaitableValuePlaceholder, node.IsDynamic, getAwaiter, isCompleted, getResult, runtimeAsyncAwaitCall, boundAwaitableValuePlaceholder2);
	}

	public override BoundNode VisitAwaitableValuePlaceholder(BoundAwaitableValuePlaceholder node)
	{
		return _placeholderMap[node];
	}

	protected override BoundBinaryOperator.UncommonData? VisitBinaryOperatorData(BoundBinaryOperator node)
	{
		return BoundBinaryOperator.UncommonData.CreateIfNeeded(node.ConstantValueOpt, VisitMethodSymbol(node.BinaryOperatorMethod), VisitType(node.ConstrainedToType), node.OriginalUserDefinedOperatorsOpt);
	}

	public override BoundNode? VisitConversion(BoundConversion node)
	{
		Conversion conversion = node.Conversion;
		if ((object)conversion.Method != null)
		{
			conversion = conversion.SetConversionMethod(VisitMethodSymbol(conversion.Method));
		}
		return node.Update((BoundExpression)Visit(node.Operand), conversion, node.IsBaseConversion, node.Checked, node.ExplicitCastInCode, node.ConstantValueOpt, node.ConversionGroupOpt, VisitType(node.Type));
	}

	[return: NotNullIfNotNull("property")]
	public override PropertySymbol? VisitPropertySymbol(PropertySymbol? property)
	{
		if ((object)property == null)
		{
			return null;
		}
		if (property.ContainingType.IsAnonymousType)
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)TypeMap.SubstituteType(property.ContainingType).AsTypeSymbolOnly();
			if ((object)namedTypeSymbol == property.ContainingType)
			{
				return property;
			}
			foreach (Symbol member in namedTypeSymbol.GetMembers(property.Name))
			{
				if (member.Kind == SymbolKind.Property)
				{
					return (PropertySymbol)member;
				}
			}
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/BoundTreeToDifferentEnclosingContextRewriter.cs", 219);
		}
		return property.OriginalDefinition.AsMember((NamedTypeSymbol)TypeMap.SubstituteType(property.ContainingType).AsTypeSymbolOnly());
	}

	[return: NotNullIfNotNull("field")]
	public override FieldSymbol? VisitFieldSymbol(FieldSymbol? field)
	{
		return field?.OriginalDefinition.AsMember((NamedTypeSymbol)TypeMap.SubstituteType(field.ContainingType).AsTypeSymbolOnly());
	}

	public override BoundNode? VisitMethodDefIndex(BoundMethodDefIndex node)
	{
		return node;
	}

	[return: NotNullIfNotNull("method")]
	public override MethodSymbol? VisitMethodSymbol(MethodSymbol? method)
	{
		if ((object)method == null)
		{
			return null;
		}
		if (method.ContainingType.IsAnonymousType)
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)TypeMap.SubstituteType(method.ContainingType).AsTypeSymbolOnly();
			if ((object)namedTypeSymbol == method.ContainingType)
			{
				return method;
			}
			foreach (Symbol member in namedTypeSymbol.GetMembers(method.Name))
			{
				if (member.Kind == SymbolKind.Method)
				{
					return (MethodSymbol)member;
				}
			}
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/BoundTreeToDifferentEnclosingContextRewriter.cs", 274);
		}
		return method.OriginalDefinition.AsMember((NamedTypeSymbol)TypeMap.SubstituteType(method.ContainingType).AsTypeSymbolOnly()).ConstructIfGeneric(TypeMap.SubstituteTypes(method.TypeArgumentsWithAnnotations));
	}
}
