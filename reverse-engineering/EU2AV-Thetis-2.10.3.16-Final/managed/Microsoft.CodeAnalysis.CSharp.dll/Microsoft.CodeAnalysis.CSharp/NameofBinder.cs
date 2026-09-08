using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class NameofBinder : Binder
{
	private readonly ExpressionSyntax _nameofArgument;

	private readonly WithTypeParametersBinder? _withTypeParametersBinder;

	private readonly Binder? _withParametersBinder;

	private ThreeState _lazyIsNameofOperator;

	private readonly Dictionary<GenericNameSyntax, bool>? _allowedMap;

	private bool IsNameofOperator
	{
		get
		{
			if (!_lazyIsNameofOperator.HasValue())
			{
				_lazyIsNameofOperator = (!base.NextRequired.InvocableNameofInScope()).ToThreeState();
			}
			return _lazyIsNameofOperator.Value();
		}
	}

	internal override bool IsInsideNameof
	{
		get
		{
			if (!IsNameofOperator)
			{
				return base.IsInsideNameof;
			}
			return true;
		}
	}

	protected override SyntaxNode? EnclosingNameofArgument
	{
		get
		{
			if (!IsNameofOperator)
			{
				return base.EnclosingNameofArgument;
			}
			return _nameofArgument;
		}
	}

	internal NameofBinder(ExpressionSyntax nameofArgument, Binder next, WithTypeParametersBinder? withTypeParametersBinder, Binder? withParametersBinder)
		: base(next)
	{
		_nameofArgument = nameofArgument;
		_withTypeParametersBinder = withTypeParametersBinder;
		_withParametersBinder = withParametersBinder;
		OpenTypeVisitor.Visit(nameofArgument, out _allowedMap);
	}

	protected override bool IsUnboundTypeAllowed(GenericNameSyntax syntax)
	{
		bool value = default(bool);
		return (_allowedMap != null && _allowedMap.TryGetValue(syntax, out value)) & value;
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, int arity, ConsList<TypeSymbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		bool flag = false;
		if (_withParametersBinder != null && IsNameofOperator)
		{
			_withParametersBinder.LookupSymbolsInSingleBinder(result, name, arity, basesBeingResolved, options, originalBinder, diagnose, ref useSiteInfo);
			if (!result.IsClear)
			{
				if (result.IsMultiViable)
				{
					return;
				}
				flag = true;
			}
		}
		if (_withTypeParametersBinder != null && IsNameofOperator)
		{
			if (flag)
			{
				LookupResult instance = LookupResult.GetInstance();
				_withTypeParametersBinder.LookupSymbolsInSingleBinder(instance, name, arity, basesBeingResolved, options, originalBinder, diagnose, ref useSiteInfo);
				result.MergeEqual(instance);
			}
			else
			{
				_withTypeParametersBinder.LookupSymbolsInSingleBinder(result, name, arity, basesBeingResolved, options, originalBinder, diagnose, ref useSiteInfo);
			}
		}
	}

	internal override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo info, LookupOptions options, Binder originalBinder)
	{
		if (_withParametersBinder != null && IsNameofOperator)
		{
			_withParametersBinder.AddLookupSymbolsInfoInSingleBinder(info, options, originalBinder);
		}
		if (_withTypeParametersBinder != null && IsNameofOperator)
		{
			_withTypeParametersBinder.AddLookupSymbolsInfoInSingleBinder(info, options, originalBinder);
		}
	}
}
