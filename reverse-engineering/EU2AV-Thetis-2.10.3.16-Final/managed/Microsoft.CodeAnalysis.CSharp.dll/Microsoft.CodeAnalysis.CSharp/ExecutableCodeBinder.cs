using System;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ExecutableCodeBinder : Binder
{
	private readonly Symbol _memberSymbol;

	private readonly SyntaxNode _root;

	private readonly Action<Binder, SyntaxNode> _binderUpdatedHandler;

	private SmallDictionary<SyntaxNode, Binder> _lazyBinderMap;

	internal override Symbol ContainingMemberOrLambda => _memberSymbol ?? base.Next.ContainingMemberOrLambda;

	protected override bool InExecutableBinder => true;

	internal Symbol MemberSymbol => _memberSymbol;

	private SmallDictionary<SyntaxNode, Binder> BinderMap
	{
		get
		{
			if (_lazyBinderMap == null)
			{
				ComputeBinderMap();
			}
			return _lazyBinderMap;
		}
	}

	internal ExecutableCodeBinder(SyntaxNode root, Symbol memberSymbol, Binder next, Action<Binder, SyntaxNode> binderUpdatedHandler = null)
		: this(root, memberSymbol, next, next.Flags)
	{
		_binderUpdatedHandler = binderUpdatedHandler;
	}

	internal ExecutableCodeBinder(SyntaxNode root, Symbol memberSymbol, Binder next, BinderFlags additionalFlags)
		: base(next, (BinderFlags)((uint)(next.Flags | additionalFlags) & 0xFFC0FFFFu))
	{
		_memberSymbol = memberSymbol;
		_root = root;
	}

	internal override Binder GetBinder(SyntaxNode node)
	{
		if (!BinderMap.TryGetValue(node, out var value))
		{
			return base.Next.GetBinder(node);
		}
		return value;
	}

	private void ComputeBinderMap()
	{
		SmallDictionary<SyntaxNode, Binder> smallDictionary;
		if (!(_memberSymbol is SynthesizedSimpleProgramEntryPointSymbol synthesizedSimpleProgramEntryPointSymbol) || _root != synthesizedSimpleProgramEntryPointSymbol.SyntaxNode)
		{
			smallDictionary = (((object)_memberSymbol == null || _root == null) ? SmallDictionary<SyntaxNode, Binder>.Empty : LocalBinderFactory.BuildMap(_memberSymbol, _root, this, _binderUpdatedHandler));
		}
		else
		{
			SimpleProgramBinder simpleProgramBinder = new SimpleProgramBinder(this, synthesizedSimpleProgramEntryPointSymbol);
			smallDictionary = LocalBinderFactory.BuildMap(_memberSymbol, _root, simpleProgramBinder, _binderUpdatedHandler);
			smallDictionary.Add(_root, simpleProgramBinder);
		}
		Interlocked.CompareExchange(ref _lazyBinderMap, smallDictionary, null);
	}

	public static void ValidateIteratorMethod(CSharpCompilation compilation, MethodSymbol iterator, BindingDiagnosticBag diagnostics)
	{
		if (!iterator.IsIterator)
		{
			return;
		}
		foreach (ParameterSymbol item in iterator.GetParametersIncludingExtensionParameter(skipExtensionIfStatic: true))
		{
			bool flag = item.IsExtensionParameter();
			if (item.RefKind != RefKind.None)
			{
				Location location = (flag ? iterator.GetFirstLocation() : item.GetFirstLocation());
				diagnostics.Add(ErrorCode.ERR_BadIteratorArgType, location);
			}
			else if (item.Type.IsPointerOrFunctionPointer() && !flag)
			{
				diagnostics.Add(ErrorCode.ERR_UnsafeIteratorArgType, item.GetFirstLocation());
			}
		}
		Location location2 = (iterator as SynthesizedSimpleProgramEntryPointSymbol)?.ReturnTypeSyntax.GetLocation() ?? iterator.GetFirstLocation();
		if (iterator.IsVararg)
		{
			diagnostics.Add(ErrorCode.ERR_VarargsIterator, location2);
		}
		SourceMemberMethodSymbol obj = iterator as SourceMemberMethodSymbol;
		if ((object)obj == null || !obj.IsUnsafe)
		{
			LocalFunctionSymbol obj2 = iterator as LocalFunctionSymbol;
			if ((object)obj2 == null || !obj2.IsUnsafe)
			{
				goto IL_0104;
			}
		}
		if (compilation.Options.AllowUnsafe)
		{
			MessageID.IDS_FeatureRefUnsafeInIteratorAsync.CheckFeatureAvailability(diagnostics, compilation, location2);
		}
		goto IL_0104;
		IL_0104:
		TypeSymbol returnType = iterator.ReturnType;
		RefKind refKind = iterator.RefKind;
		TypeWithAnnotations iteratorElementTypeFromReturnType = InMethodBinder.GetIteratorElementTypeFromReturnType(compilation, refKind, returnType, location2, diagnostics);
		if (iteratorElementTypeFromReturnType.IsDefault)
		{
			if (refKind != RefKind.None)
			{
				Binder.Error(diagnostics, ErrorCode.ERR_BadIteratorReturnRef, location2, iterator);
			}
			else if (!returnType.IsErrorType())
			{
				Binder.Error(diagnostics, ErrorCode.ERR_BadIteratorReturn, location2, iterator, returnType);
			}
		}
		else if (iteratorElementTypeFromReturnType.IsRefLikeOrAllowsRefLikeType())
		{
			Binder.Error(diagnostics, ErrorCode.ERR_IteratorRefLikeElementType, location2);
		}
		if (InMethodBinder.IsAsyncStreamInterface(compilation, refKind, returnType) && !iterator.IsAsync)
		{
			diagnostics.Add(ErrorCode.ERR_IteratorMustBeAsync, location2, iterator, returnType);
		}
	}
}
