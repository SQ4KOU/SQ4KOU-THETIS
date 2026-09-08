using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundBlockInstrumentation : BoundNode
{
	public ImmutableArray<LocalSymbol> Locals { get; }

	public BoundStatement? Prologue { get; }

	public BoundStatement? Epilogue { get; }

	public BoundBlockInstrumentation(SyntaxNode syntax, ImmutableArray<LocalSymbol> locals, BoundStatement? prologue, BoundStatement? epilogue, bool hasErrors = false)
		: base(BoundKind.BlockInstrumentation, syntax, hasErrors || prologue.HasErrors() || epilogue.HasErrors())
	{
		Locals = locals;
		Prologue = prologue;
		Epilogue = epilogue;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitBlockInstrumentation(this);
	}

	public BoundBlockInstrumentation Update(ImmutableArray<LocalSymbol> locals, BoundStatement? prologue, BoundStatement? epilogue)
	{
		if (locals != Locals || prologue != Prologue || epilogue != Epilogue)
		{
			BoundBlockInstrumentation boundBlockInstrumentation = new BoundBlockInstrumentation(Syntax, locals, prologue, epilogue, base.HasErrors);
			boundBlockInstrumentation.CopyAttributes(this);
			return boundBlockInstrumentation;
		}
		return this;
	}
}
