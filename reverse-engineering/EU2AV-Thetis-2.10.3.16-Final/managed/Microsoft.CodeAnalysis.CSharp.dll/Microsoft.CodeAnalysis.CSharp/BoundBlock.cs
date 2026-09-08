using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundBlock : BoundStatementList
{
	public ImmutableArray<LocalSymbol> Locals { get; }

	public ImmutableArray<MethodSymbol> LocalFunctions { get; }

	public bool HasUnsafeModifier { get; }

	public BoundBlockInstrumentation? Instrumentation { get; }

	public BoundBlock(SyntaxNode syntax, ImmutableArray<LocalSymbol> locals, ImmutableArray<BoundStatement> statements, bool hasErrors = false)
		: this(syntax, locals, ImmutableArray<MethodSymbol>.Empty, hasUnsafeModifier: false, null, statements, hasErrors)
	{
	}

	public static BoundBlock SynthesizedNoLocals(SyntaxNode syntax, BoundStatement statement)
	{
		return new BoundBlock(syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(statement))
		{
			WasCompilerGenerated = true
		};
	}

	public static BoundBlock SynthesizedNoLocals(SyntaxNode syntax, ImmutableArray<BoundStatement> statements)
	{
		return new BoundBlock(syntax, ImmutableArray<LocalSymbol>.Empty, statements)
		{
			WasCompilerGenerated = true
		};
	}

	public static BoundBlock SynthesizedNoLocals(SyntaxNode syntax, params BoundStatement[] statements)
	{
		return new BoundBlock(syntax, ImmutableArray<LocalSymbol>.Empty, statements.AsImmutableOrNull())
		{
			WasCompilerGenerated = true
		};
	}

	public BoundBlock(SyntaxNode syntax, ImmutableArray<LocalSymbol> locals, ImmutableArray<MethodSymbol> localFunctions, bool hasUnsafeModifier, BoundBlockInstrumentation? instrumentation, ImmutableArray<BoundStatement> statements, bool hasErrors = false)
		: base(BoundKind.Block, syntax, statements, hasErrors || instrumentation.HasErrors() || statements.HasErrors())
	{
		Locals = locals;
		LocalFunctions = localFunctions;
		HasUnsafeModifier = hasUnsafeModifier;
		Instrumentation = instrumentation;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitBlock(this);
	}

	public BoundBlock Update(ImmutableArray<LocalSymbol> locals, ImmutableArray<MethodSymbol> localFunctions, bool hasUnsafeModifier, BoundBlockInstrumentation? instrumentation, ImmutableArray<BoundStatement> statements)
	{
		if (locals != Locals || localFunctions != LocalFunctions || hasUnsafeModifier != HasUnsafeModifier || instrumentation != Instrumentation || statements != base.Statements)
		{
			BoundBlock boundBlock = new BoundBlock(Syntax, locals, localFunctions, hasUnsafeModifier, instrumentation, statements, base.HasErrors);
			boundBlock.CopyAttributes(this);
			return boundBlock;
		}
		return this;
	}
}
