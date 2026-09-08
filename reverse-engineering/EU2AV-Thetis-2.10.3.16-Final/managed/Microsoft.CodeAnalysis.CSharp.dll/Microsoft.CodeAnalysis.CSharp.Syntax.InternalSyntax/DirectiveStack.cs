using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal readonly struct DirectiveStack
{
	public static readonly DirectiveStack Empty = new DirectiveStack(ConsList<Directive>.Empty, seenAnyIfDirectives: false);

	private readonly ConsList<Directive>? _directives;

	private readonly bool _seenAnyIfDirectives;

	public bool IsNull => _directives == null;

	public bool IsEmpty => _directives == ConsList<Directive>.Empty;

	public bool SeenAnyIfDirectives => _seenAnyIfDirectives;

	private DirectiveStack(ConsList<Directive>? directives, bool seenAnyIfDirectives)
	{
		_directives = directives;
		_seenAnyIfDirectives = seenAnyIfDirectives;
	}

	public static void InterlockedInitialize(ref DirectiveStack location, DirectiveStack value)
	{
		Interlocked.CompareExchange(ref Unsafe.AsRef(in location._directives), value._directives, null);
	}

	public DefineState IsDefined(string id)
	{
		for (ConsList<Directive> consList = _directives; consList != null && consList.Any(); consList = consList.Tail)
		{
			switch (consList.Head.Kind)
			{
			case SyntaxKind.DefineDirectiveTrivia:
				if (!(consList.Head.GetIdentifier() == id))
				{
					continue;
				}
				return DefineState.Defined;
			case SyntaxKind.UndefDirectiveTrivia:
				if (!(consList.Head.GetIdentifier() == id))
				{
					continue;
				}
				return DefineState.Undefined;
			case SyntaxKind.ElifDirectiveTrivia:
			case SyntaxKind.ElseDirectiveTrivia:
				break;
			default:
				continue;
			}
			do
			{
				consList = consList.Tail;
				if (consList == null || !consList.Any())
				{
					return DefineState.Unspecified;
				}
			}
			while (consList.Head.Kind != SyntaxKind.IfDirectiveTrivia);
		}
		return DefineState.Unspecified;
	}

	public bool PreviousBranchTaken()
	{
		ConsList<Directive> consList = _directives;
		while (consList != null && consList.Any())
		{
			if (consList.Head.BranchTaken)
			{
				return true;
			}
			if (consList.Head.Kind == SyntaxKind.IfDirectiveTrivia)
			{
				return false;
			}
			consList = consList.Tail;
		}
		return false;
	}

	public bool HasUnfinishedIf()
	{
		ConsList<Directive> previousIfElifElseOrRegion = GetPreviousIfElifElseOrRegion(_directives);
		if (previousIfElifElseOrRegion != null && previousIfElifElseOrRegion.Any())
		{
			return previousIfElifElseOrRegion.Head.Kind != SyntaxKind.RegionDirectiveTrivia;
		}
		return false;
	}

	public bool HasPreviousIfOrElif()
	{
		ConsList<Directive> previousIfElifElseOrRegion = GetPreviousIfElifElseOrRegion(_directives);
		if (previousIfElifElseOrRegion != null && previousIfElifElseOrRegion.Any())
		{
			if (previousIfElifElseOrRegion.Head.Kind != SyntaxKind.IfDirectiveTrivia)
			{
				return previousIfElifElseOrRegion.Head.Kind == SyntaxKind.ElifDirectiveTrivia;
			}
			return true;
		}
		return false;
	}

	public bool HasUnfinishedRegion()
	{
		ConsList<Directive> previousIfElifElseOrRegion = GetPreviousIfElifElseOrRegion(_directives);
		if (previousIfElifElseOrRegion != null && previousIfElifElseOrRegion.Any())
		{
			return previousIfElifElseOrRegion.Head.Kind == SyntaxKind.RegionDirectiveTrivia;
		}
		return false;
	}

	public DirectiveStack Add(Directive directive)
	{
		switch (directive.Kind)
		{
		case SyntaxKind.EndIfDirectiveTrivia:
		{
			ConsList<Directive> previousIf = GetPreviousIf(_directives);
			bool include;
			if (previousIf != null && previousIf.Any())
			{
				return new DirectiveStack(CompleteIf(_directives, out include), _seenAnyIfDirectives);
			}
			break;
		}
		case SyntaxKind.EndRegionDirectiveTrivia:
		{
			ConsList<Directive> previousRegion = GetPreviousRegion(_directives);
			if (previousRegion != null && previousRegion.Any())
			{
				return new DirectiveStack(CompleteRegion(_directives), _seenAnyIfDirectives);
			}
			break;
		}
		}
		return new DirectiveStack(new ConsList<Directive>(directive, _directives ?? ConsList<Directive>.Empty), _seenAnyIfDirectives || directive.Kind == SyntaxKind.IfDirectiveTrivia);
	}

	private static ConsList<Directive> CompleteIf(ConsList<Directive> stack, out bool include)
	{
		if (!stack.Any())
		{
			include = true;
			return stack;
		}
		if (stack.Head.Kind == SyntaxKind.IfDirectiveTrivia)
		{
			include = stack.Head.BranchTaken;
			return stack.Tail;
		}
		ConsList<Directive> consList = CompleteIf(stack.Tail, out include);
		SyntaxKind kind = stack.Head.Kind;
		if (kind - 8549 <= SyntaxKind.List)
		{
			include = stack.Head.BranchTaken;
		}
		else if (include)
		{
			consList = new ConsList<Directive>(stack.Head, consList);
		}
		return consList;
	}

	private static ConsList<Directive> CompleteRegion(ConsList<Directive> stack)
	{
		if (!stack.Any())
		{
			return stack;
		}
		if (stack.Head.Kind == SyntaxKind.RegionDirectiveTrivia)
		{
			return stack.Tail;
		}
		ConsList<Directive> tail = CompleteRegion(stack.Tail);
		return new ConsList<Directive>(stack.Head, tail);
	}

	private static ConsList<Directive>? GetPreviousIf(ConsList<Directive>? directives)
	{
		ConsList<Directive> consList = directives;
		while (consList != null && consList.Any())
		{
			if (consList.Head.Kind == SyntaxKind.IfDirectiveTrivia)
			{
				return consList;
			}
			consList = consList.Tail;
		}
		return consList;
	}

	private static ConsList<Directive>? GetPreviousIfElifElseOrRegion(ConsList<Directive>? directives)
	{
		ConsList<Directive> consList = directives;
		while (consList != null && consList.Any())
		{
			SyntaxKind kind = consList.Head.Kind;
			if (kind - 8548 <= (SyntaxKind)2 || kind == SyntaxKind.RegionDirectiveTrivia)
			{
				return consList;
			}
			consList = consList.Tail;
		}
		return consList;
	}

	private static ConsList<Directive>? GetPreviousRegion(ConsList<Directive>? directives)
	{
		ConsList<Directive> consList = directives;
		while (consList != null && consList.Any() && consList.Head.Kind != SyntaxKind.RegionDirectiveTrivia)
		{
			consList = consList.Tail;
		}
		return consList;
	}

	internal string GetDebuggerDisplay()
	{
		if (IsNull)
		{
			return "<null>";
		}
		if (IsEmpty)
		{
			return "[]";
		}
		StringBuilder stringBuilder = new StringBuilder();
		ConsList<Directive> consList = _directives;
		while (consList != null && consList.Any())
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Insert(0, " | ");
			}
			stringBuilder.Insert(0, consList.Head.GetDebuggerDisplay());
			consList = consList.Tail;
		}
		return stringBuilder.ToString();
	}

	public bool IncrementallyEquivalent(DirectiveStack other)
	{
		ConsList<Directive> consList = SkipInsignificantDirectives(_directives);
		ConsList<Directive> consList2 = SkipInsignificantDirectives(other._directives);
		bool flag = consList?.Any() ?? false;
		bool flag2 = consList2?.Any() ?? false;
		while (flag & flag2)
		{
			if (!consList.Head.IncrementallyEquivalent(consList2.Head))
			{
				return false;
			}
			consList = SkipInsignificantDirectives(consList.Tail);
			consList2 = SkipInsignificantDirectives(consList2.Tail);
			flag = consList?.Any() ?? false;
			flag2 = consList2?.Any() ?? false;
		}
		return flag == flag2;
	}

	private static ConsList<Directive>? SkipInsignificantDirectives(ConsList<Directive>? directives)
	{
		while (directives != null && directives.Any())
		{
			SyntaxKind kind = directives.Head.Kind;
			if (kind - 8548 <= (SyntaxKind)7)
			{
				return directives;
			}
			directives = directives.Tail;
		}
		return directives;
	}
}
