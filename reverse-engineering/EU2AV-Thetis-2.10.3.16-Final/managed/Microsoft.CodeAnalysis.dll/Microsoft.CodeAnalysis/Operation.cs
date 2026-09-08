using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeAnalysis;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract class Operation : IOperation
{
	protected static readonly IOperation s_unset = new EmptyOperation(null, null, isImplicit: true);

	private readonly SemanticModel? _owningSemanticModelOpt;

	private IOperation? _parentDoNotAccessDirectly;

	public IOperation? Parent => _parentDoNotAccessDirectly;

	public bool IsImplicit { get; }

	public abstract OperationKind Kind { get; }

	public SyntaxNode Syntax { get; }

	public abstract ITypeSymbol? Type { get; }

	public string Language => Syntax.Language;

	internal abstract ConstantValue? OperationConstantValue { get; }

	public Optional<object?> ConstantValue
	{
		get
		{
			if (OperationConstantValue == null || OperationConstantValue.IsBad)
			{
				return default(Optional<object>);
			}
			return new Optional<object>(OperationConstantValue.Value);
		}
	}

	IEnumerable<IOperation> IOperation.Children => ChildOperations;

	public IOperation.OperationList ChildOperations => new IOperation.OperationList(this);

	internal abstract int ChildOperationsCount { get; }

	SemanticModel? IOperation.SemanticModel => _owningSemanticModelOpt?.ContainingPublicModelOrSelf;

	internal SemanticModel? OwningSemanticModel => _owningSemanticModelOpt;

	protected Operation(SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
	{
		_owningSemanticModelOpt = semanticModel;
		Syntax = syntax;
		IsImplicit = isImplicit;
		_parentDoNotAccessDirectly = s_unset;
	}

	internal abstract IOperation GetCurrent(int slot, int index);

	internal abstract (bool hasNext, int nextSlot, int nextIndex) MoveNext(int previousSlot, int previousIndex);

	internal abstract (bool hasNext, int nextSlot, int nextIndex) MoveNextReversed(int previousSlot, int previousIndex);

	public abstract void Accept(OperationVisitor visitor);

	public abstract TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument);

	protected void SetParentOperation(IOperation? parent)
	{
		_parentDoNotAccessDirectly = parent;
	}

	[return: NotNullIfNotNull("operation")]
	public static T? SetParentOperation<T>(T? operation, IOperation? parent) where T : IOperation
	{
		(operation as Operation)?.SetParentOperation(parent);
		return operation;
	}

	public static ImmutableArray<T> SetParentOperation<T>(ImmutableArray<T> operations, IOperation? parent) where T : IOperation
	{
		if (operations.Length == 0)
		{
			return operations;
		}
		foreach (T item in operations)
		{
			SetParentOperation(item, parent);
		}
		return operations;
	}

	[Conditional("DEBUG")]
	internal static void VerifyParentOperation(IOperation? parent, IOperation child)
	{
	}

	[Conditional("DEBUG")]
	internal static void VerifyParentOperation<T>(IOperation? parent, ImmutableArray<T> children) where T : IOperation
	{
		foreach (T item in children)
		{
			_ = item;
		}
	}

	private string GetDebuggerDisplay()
	{
		return string.Format("{0} Type: {1}", GetType().Name, (Type == null) ? ((object)"null") : ((object)Type));
	}
}
