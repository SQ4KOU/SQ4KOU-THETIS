using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class RangeVariableSymbol : Symbol
{
	private readonly string _name;

	private readonly Location? _location;

	private readonly Symbol _containingSymbol;

	internal bool IsTransparent { get; }

	public override string Name => _name;

	public override SymbolKind Kind => SymbolKind.RangeVariable;

	public override ImmutableArray<Location> Locations
	{
		get
		{
			if ((object)_location != null)
			{
				return ImmutableArray.Create(_location);
			}
			return ImmutableArray<Location>.Empty;
		}
	}

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
	{
		get
		{
			if ((object)_location == null)
			{
				return ImmutableArray<SyntaxReference>.Empty;
			}
			return ImmutableArray.Create(_location.SourceTree.GetRoot().FindToken(_location.SourceSpan.Start).Parent.GetReference());
		}
	}

	public override bool IsExtern => false;

	public override bool IsSealed => false;

	public override bool IsAbstract => false;

	public override bool IsOverride => false;

	public override bool IsVirtual => false;

	public override bool IsStatic => false;

	internal sealed override ObsoleteAttributeData? ObsoleteAttributeData => null;

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public override Symbol ContainingSymbol => _containingSymbol;

	internal RangeVariableSymbol(string Name, Symbol containingSymbol, Location? location, bool isTransparent = false)
	{
		_name = Name;
		_containingSymbol = containingSymbol;
		_location = location;
		IsTransparent = isTransparent;
	}

	public override Location? TryGetFirstLocation()
	{
		return _location;
	}

	internal override TResult Accept<TArg, TResult>(CSharpSymbolVisitor<TArg, TResult> visitor, TArg a)
	{
		return visitor.VisitRangeVariable(this, a);
	}

	public override void Accept(CSharpSymbolVisitor visitor)
	{
		visitor.VisitRangeVariable(this);
	}

	public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
	{
		return visitor.VisitRangeVariable(this);
	}

	public override bool Equals(Symbol obj, TypeCompareKind compareKind)
	{
		if ((object)obj == this)
		{
			return true;
		}
		if ((object)_location == null)
		{
			return false;
		}
		if (obj is RangeVariableSymbol rangeVariableSymbol && _location.Equals(rangeVariableSymbol._location))
		{
			return _containingSymbol.Equals(rangeVariableSymbol.ContainingSymbol, compareKind);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(_location?.GetHashCode() ?? 0, _containingSymbol.GetHashCode());
	}

	protected override ISymbol CreateISymbol()
	{
		return new Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.RangeVariableSymbol(this);
	}
}
