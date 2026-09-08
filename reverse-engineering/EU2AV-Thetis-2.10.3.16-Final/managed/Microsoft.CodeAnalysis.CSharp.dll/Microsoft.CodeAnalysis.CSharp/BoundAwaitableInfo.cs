using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundAwaitableInfo : BoundNode
{
	public BoundAwaitableValuePlaceholder? AwaitableInstancePlaceholder { get; }

	public bool IsDynamic { get; }

	public BoundExpression? GetAwaiter { get; }

	public PropertySymbol? IsCompleted { get; }

	public MethodSymbol? GetResult { get; }

	public BoundCall? RuntimeAsyncAwaitCall { get; }

	public BoundAwaitableValuePlaceholder? RuntimeAsyncAwaitCallPlaceholder { get; }

	public BoundAwaitableInfo(SyntaxNode syntax, BoundAwaitableValuePlaceholder? awaitableInstancePlaceholder, bool isDynamic, BoundExpression? getAwaiter, PropertySymbol? isCompleted, MethodSymbol? getResult, BoundCall? runtimeAsyncAwaitCall, BoundAwaitableValuePlaceholder? runtimeAsyncAwaitCallPlaceholder, bool hasErrors = false)
		: base(BoundKind.AwaitableInfo, syntax, hasErrors || awaitableInstancePlaceholder.HasErrors() || getAwaiter.HasErrors() || runtimeAsyncAwaitCall.HasErrors() || runtimeAsyncAwaitCallPlaceholder.HasErrors())
	{
		AwaitableInstancePlaceholder = awaitableInstancePlaceholder;
		IsDynamic = isDynamic;
		GetAwaiter = getAwaiter;
		IsCompleted = isCompleted;
		GetResult = getResult;
		RuntimeAsyncAwaitCall = runtimeAsyncAwaitCall;
		RuntimeAsyncAwaitCallPlaceholder = runtimeAsyncAwaitCallPlaceholder;
	}

	[Conditional("DEBUG")]
	private void Validate()
	{
		if (RuntimeAsyncAwaitCall != null)
		{
			string name = RuntimeAsyncAwaitCall.Method.Name;
			if (!(name == "Await") && !(name == "AwaitAwaiter"))
			{
				_ = name == "UnsafeAwaitAwaiter";
			}
		}
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitAwaitableInfo(this);
	}

	public BoundAwaitableInfo Update(BoundAwaitableValuePlaceholder? awaitableInstancePlaceholder, bool isDynamic, BoundExpression? getAwaiter, PropertySymbol? isCompleted, MethodSymbol? getResult, BoundCall? runtimeAsyncAwaitCall, BoundAwaitableValuePlaceholder? runtimeAsyncAwaitCallPlaceholder)
	{
		if (awaitableInstancePlaceholder != AwaitableInstancePlaceholder || isDynamic != IsDynamic || getAwaiter != GetAwaiter || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(isCompleted, IsCompleted) || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(getResult, GetResult) || runtimeAsyncAwaitCall != RuntimeAsyncAwaitCall || runtimeAsyncAwaitCallPlaceholder != RuntimeAsyncAwaitCallPlaceholder)
		{
			BoundAwaitableInfo boundAwaitableInfo = new BoundAwaitableInfo(Syntax, awaitableInstancePlaceholder, isDynamic, getAwaiter, isCompleted, getResult, runtimeAsyncAwaitCall, runtimeAsyncAwaitCallPlaceholder, base.HasErrors);
			boundAwaitableInfo.CopyAttributes(this);
			return boundAwaitableInfo;
		}
		return this;
	}
}
