using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Emit;

public readonly struct SemanticEdit : IEquatable<SemanticEdit>
{
	public SemanticEditKind Kind { get; }

	public ISymbol? OldSymbol { get; }

	public ISymbol? NewSymbol { get; }

	public Func<SyntaxNode, SyntaxNode?>? SyntaxMap { get; }

	public Func<SyntaxNode, RuntimeRudeEdit?>? RuntimeRudeEdit { get; }

	public MethodInstrumentation Instrumentation { get; }

	[MemberNotNullWhen(true, "SyntaxMap")]
	public bool PreserveLocalVariables
	{
		[MemberNotNullWhen(true, "SyntaxMap")]
		get
		{
			return SyntaxMap != null;
		}
	}

	[Obsolete("Use other overload")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public SemanticEdit(SemanticEditKind kind, ISymbol? oldSymbol, ISymbol? newSymbol, Func<SyntaxNode, SyntaxNode?>? syntaxMap, bool preserveLocalVariables)
		: this(kind, oldSymbol, newSymbol, syntaxMap, preserveLocalVariables, MethodInstrumentation.Empty)
	{
	}

	[Obsolete("Use other overload")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public SemanticEdit(SemanticEditKind kind, ISymbol? oldSymbol, ISymbol? newSymbol, Func<SyntaxNode, SyntaxNode?>? syntaxMap, bool preserveLocalVariables, MethodInstrumentation instrumentation)
		: this(kind, oldSymbol, newSymbol, syntaxMap, null, MethodInstrumentation.Empty)
	{
	}

	public SemanticEdit(SemanticEditKind kind, ISymbol? oldSymbol, ISymbol? newSymbol, Func<SyntaxNode, SyntaxNode?>? syntaxMap = null, Func<SyntaxNode, RuntimeRudeEdit?>? runtimeRudeEdit = null, MethodInstrumentation instrumentation = default(MethodInstrumentation))
	{
		if (kind <= SemanticEditKind.None || kind > SemanticEditKind.Replace)
		{
			throw new ArgumentOutOfRangeException("kind");
		}
		bool flag = oldSymbol == null;
		if (flag)
		{
			bool flag2 = ((kind == SemanticEditKind.Insert || kind == SemanticEditKind.Replace) ? true : false);
			flag = !flag2;
		}
		if (flag)
		{
			throw new ArgumentNullException("oldSymbol");
		}
		if (newSymbol == null)
		{
			throw new ArgumentNullException("newSymbol");
		}
		if (runtimeRudeEdit != null && syntaxMap == null)
		{
			throw new ArgumentNullException("syntaxMap");
		}
		if (syntaxMap != null)
		{
			if (kind != SemanticEditKind.Update)
			{
				throw new ArgumentException("Syntax map can only be specified for updates", "syntaxMap");
			}
			if (!(oldSymbol is IMethodSymbol))
			{
				throw new ArgumentException(CodeAnalysisResources.MethodSymbolExpected, "oldSymbol");
			}
			if (!(newSymbol is IMethodSymbol))
			{
				throw new ArgumentException(CodeAnalysisResources.MethodSymbolExpected, "newSymbol");
			}
		}
		if ((oldSymbol is IMethodSymbol { PartialImplementationPart: not null } || oldSymbol is IPropertySymbol { PartialImplementationPart: not null }) ? true : false)
		{
			throw new ArgumentException("Partial member implementation required", "oldSymbol");
		}
		if ((newSymbol is IMethodSymbol { PartialImplementationPart: not null } || newSymbol is IPropertySymbol { PartialImplementationPart: not null }) ? true : false)
		{
			throw new ArgumentException("Partial member implementation required", "newSymbol");
		}
		flag = kind == SemanticEditKind.Delete;
		if (flag)
		{
			bool flag2 = ((oldSymbol is IMethodSymbol || oldSymbol is IPropertySymbol || oldSymbol is IEventSymbol) ? true : false);
			flag = !flag2;
		}
		if (flag)
		{
			throw new ArgumentException("Deleted symbol must be a method, property or an event", "oldSymbol");
		}
		if (instrumentation.IsDefault)
		{
			instrumentation = MethodInstrumentation.Empty;
		}
		if (!instrumentation.IsEmpty)
		{
			if (kind != SemanticEditKind.Update)
			{
				throw new ArgumentOutOfRangeException("kind");
			}
			if (!(oldSymbol is IMethodSymbol))
			{
				throw new ArgumentException(CodeAnalysisResources.MethodSymbolExpected, "oldSymbol");
			}
			if (!(newSymbol is IMethodSymbol))
			{
				throw new ArgumentException(CodeAnalysisResources.MethodSymbolExpected, "newSymbol");
			}
			foreach (InstrumentationKind kind2 in instrumentation.Kinds)
			{
				if (!kind2.IsValid())
				{
					throw new ArgumentOutOfRangeException("Kinds", string.Format(CodeAnalysisResources.InvalidInstrumentationKind, kind2));
				}
			}
		}
		Kind = kind;
		OldSymbol = oldSymbol;
		NewSymbol = newSymbol;
		SyntaxMap = syntaxMap;
		Instrumentation = instrumentation;
		RuntimeRudeEdit = runtimeRudeEdit;
	}

	internal SemanticEdit(IMethodSymbol oldSymbol, IMethodSymbol newSymbol, ImmutableArray<InstrumentationKind> instrumentationKinds)
	{
		SyntaxMap = null;
		RuntimeRudeEdit = null;
		Kind = SemanticEditKind.Update;
		OldSymbol = oldSymbol;
		NewSymbol = newSymbol;
		Instrumentation = new MethodInstrumentation
		{
			Kinds = instrumentationKinds
		};
	}

	internal static SemanticEdit Create(SemanticEditKind kind, ISymbolInternal oldSymbol, ISymbolInternal newSymbol, Func<SyntaxNode, SyntaxNode>? syntaxMap = null)
	{
		return new SemanticEdit(kind, oldSymbol?.GetISymbol(), newSymbol?.GetISymbol(), syntaxMap);
	}

	public override int GetHashCode()
	{
		return Hash.Combine(OldSymbol, Hash.Combine(NewSymbol, (int)Kind));
	}

	public override bool Equals(object? obj)
	{
		if (obj is SemanticEdit other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(SemanticEdit other)
	{
		if (Kind == other.Kind && ((OldSymbol == null) ? (other.OldSymbol == null) : OldSymbol.Equals(other.OldSymbol)))
		{
			if (NewSymbol != null)
			{
				return NewSymbol.Equals(other.NewSymbol);
			}
			return other.NewSymbol == null;
		}
		return false;
	}

	public static bool operator ==(SemanticEdit left, SemanticEdit right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SemanticEdit left, SemanticEdit right)
	{
		return !(left == right);
	}
}
