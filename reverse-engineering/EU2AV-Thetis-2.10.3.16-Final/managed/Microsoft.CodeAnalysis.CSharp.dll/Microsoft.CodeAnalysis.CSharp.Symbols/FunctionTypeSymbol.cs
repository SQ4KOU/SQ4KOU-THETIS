using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
internal sealed class FunctionTypeSymbol : TypeSymbol
{
	private static readonly NamedTypeSymbol Uninitialized = new UnsupportedMetadataTypeSymbol();

	private readonly Binder? _binder;

	private readonly Func<Binder, BoundExpression, NamedTypeSymbol?>? _calculateDelegate;

	private BoundExpression? _expression;

	private NamedTypeSymbol? _lazyDelegateType;

	public override bool IsReferenceType => true;

	public override bool IsValueType => false;

	public override TypeKind TypeKind => (TypeKind)byte.MaxValue;

	public override bool IsRefLikeType => false;

	public override bool IsReadOnly => true;

	public override SymbolKind Kind => (SymbolKind)255;

	public override Symbol? ContainingSymbol => null;

	public override ImmutableArray<Location> Locations
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 112);
		}
	}

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 114);
		}
	}

	public override Accessibility DeclaredAccessibility
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 116);
		}
	}

	public override bool IsStatic => false;

	public override bool IsAbstract
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 120);
		}
	}

	public override bool IsSealed
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 122);
		}
	}

	internal override NamedTypeSymbol? BaseTypeNoUseSiteDiagnostics => null;

	internal override bool IsRecord
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 126);
		}
	}

	internal override bool IsRecordStruct
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 128);
		}
	}

	internal override ObsoleteAttributeData ObsoleteAttributeData
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 130);
		}
	}

	internal static FunctionTypeSymbol? CreateIfFeatureEnabled(SyntaxNode syntax, Binder binder, Func<Binder, BoundExpression, NamedTypeSymbol?> calculateDelegate)
	{
		if (!syntax.IsFeatureEnabled(MessageID.IDS_FeatureInferredDelegateType))
		{
			return null;
		}
		return new FunctionTypeSymbol(binder, calculateDelegate);
	}

	private FunctionTypeSymbol(Binder binder, Func<Binder, BoundExpression, NamedTypeSymbol?> calculateDelegate)
	{
		_binder = binder;
		_calculateDelegate = calculateDelegate;
		_lazyDelegateType = Uninitialized;
	}

	internal FunctionTypeSymbol(NamedTypeSymbol delegateType)
	{
		_lazyDelegateType = delegateType;
	}

	internal void SetExpression(BoundExpression expression)
	{
		_expression = expression;
	}

	internal NamedTypeSymbol? GetInternalDelegateType()
	{
		if ((object)_lazyDelegateType == Uninitialized)
		{
			NamedTypeSymbol value = _calculateDelegate(_binder, _expression);
			NamedTypeSymbol namedTypeSymbol = Interlocked.CompareExchange(ref _lazyDelegateType, value, Uninitialized);
			if (_binder.Compilation.TestOnlyCompilationData is InferredDelegateTypeData inferredDelegateTypeData && (object)namedTypeSymbol == Uninitialized)
			{
				Interlocked.Increment(ref inferredDelegateTypeData.InferredDelegateCount);
			}
		}
		return _lazyDelegateType;
	}

	public override void Accept(CSharpSymbolVisitor visitor)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 132);
	}

	public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 134);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 136);
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 138);
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 140);
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(ReadOnlyMemory<char> name)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 142);
	}

	protected override ISymbol CreateISymbol()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 144);
	}

	protected override ITypeSymbol CreateITypeSymbol(Microsoft.CodeAnalysis.NullableAnnotation nullableAnnotation)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 146);
	}

	internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 148);
	}

	internal override void AddNullableTransforms(ArrayBuilder<byte> transforms)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 150);
	}

	internal override bool ApplyNullableTransforms(byte defaultTransformFlag, ImmutableArray<byte> transforms, ref int position, out TypeSymbol result)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 152);
	}

	internal override ManagedKind GetManagedKind(ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 154);
	}

	internal override bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, Symbol owner, ref HashSet<TypeSymbol> checkedTypes)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 156);
	}

	internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<TypeSymbol>? basesBeingResolved = null)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override TypeSymbol MergeEquivalentTypes(TypeSymbol other, VarianceKind variance)
	{
		NamedTypeSymbol internalDelegateType = GetInternalDelegateType();
		FunctionTypeSymbol functionTypeSymbol = (FunctionTypeSymbol)other;
		NamedTypeSymbol internalDelegateType2 = functionTypeSymbol.GetInternalDelegateType();
		if ((object)internalDelegateType == null || (object)internalDelegateType2 == null)
		{
			return this;
		}
		NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)internalDelegateType.MergeEquivalentTypes(internalDelegateType2, variance);
		if ((object)internalDelegateType != namedTypeSymbol)
		{
			return functionTypeSymbol.WithDelegateType(namedTypeSymbol);
		}
		return this;
	}

	internal override TypeSymbol SetNullabilityForReferenceTypes(Func<TypeWithAnnotations, TypeWithAnnotations> transform)
	{
		NamedTypeSymbol internalDelegateType = GetInternalDelegateType();
		if ((object)internalDelegateType == null)
		{
			return this;
		}
		return WithDelegateType((NamedTypeSymbol)internalDelegateType.SetNullabilityForReferenceTypes(transform));
	}

	private FunctionTypeSymbol WithDelegateType(NamedTypeSymbol delegateType)
	{
		if ((object)GetInternalDelegateType() != delegateType)
		{
			return new FunctionTypeSymbol(delegateType);
		}
		return this;
	}

	internal override IEnumerable<(MethodSymbol Body, MethodSymbol Implemented)> SynthesizedInterfaceMethodImpls()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionTypeSymbol.cs", 197);
	}

	internal override bool HasInlineArrayAttribute(out int length)
	{
		length = 0;
		return false;
	}

	internal override bool Equals(TypeSymbol t2, TypeCompareKind compareKind)
	{
		if ((object)this == t2)
		{
			return true;
		}
		if (t2 is FunctionTypeSymbol functionTypeSymbol)
		{
			NamedTypeSymbol internalDelegateType = GetInternalDelegateType();
			NamedTypeSymbol internalDelegateType2 = functionTypeSymbol.GetInternalDelegateType();
			if ((object)internalDelegateType == null || (object)internalDelegateType2 == null)
			{
				return false;
			}
			return TypeSymbol.Equals(internalDelegateType, internalDelegateType2, compareKind);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return GetInternalDelegateType()?.GetHashCode() ?? 0;
	}

	internal override string GetDebuggerDisplay()
	{
		return "FunctionTypeSymbol: " + GetInternalDelegateType()?.GetDebuggerDisplay();
	}
}
