using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class ReducedExtensionMethodSymbol : MethodSymbol
{
	private sealed class ReducedExtensionMethodParameterSymbol : WrappedParameterSymbol
	{
		private readonly ReducedExtensionMethodSymbol _containingMethod;

		public override Symbol ContainingSymbol => _containingMethod;

		public override int Ordinal => _underlyingParameter.Ordinal - 1;

		public override TypeWithAnnotations TypeWithAnnotations => _containingMethod._typeMap.SubstituteType(_underlyingParameter.TypeWithAnnotations);

		public override ImmutableArray<CustomModifier> RefCustomModifiers => _containingMethod._typeMap.SubstituteCustomModifiers(_underlyingParameter.RefCustomModifiers);

		internal override bool IsCallerLineNumber
		{
			get
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/ReducedExtensionMethodSymbol.cs", 657);
			}
		}

		internal override bool IsCallerFilePath
		{
			get
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/ReducedExtensionMethodSymbol.cs", 663);
			}
		}

		internal override bool IsCallerMemberName
		{
			get
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/ReducedExtensionMethodSymbol.cs", 669);
			}
		}

		internal override int CallerArgumentExpressionParameterIndex
		{
			get
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/ReducedExtensionMethodSymbol.cs", 675);
			}
		}

		internal override ImmutableArray<int> InterpolatedStringHandlerArgumentIndexes
		{
			get
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/ReducedExtensionMethodSymbol.cs", 678);
			}
		}

		internal override bool HasInterpolatedStringHandlerArgumentError
		{
			get
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/ReducedExtensionMethodSymbol.cs", 680);
			}
		}

		internal override bool HasEnumeratorCancellationAttribute
		{
			get
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/ReducedExtensionMethodSymbol.cs", 682);
			}
		}

		public ReducedExtensionMethodParameterSymbol(ReducedExtensionMethodSymbol containingMethod, ParameterSymbol underlyingParameter)
			: base(underlyingParameter)
		{
			_containingMethod = containingMethod;
		}

		public sealed override bool Equals(Symbol obj, TypeCompareKind compareKind)
		{
			if ((object)this == obj)
			{
				return true;
			}
			if (obj is ReducedExtensionMethodParameterSymbol reducedExtensionMethodParameterSymbol && Ordinal == reducedExtensionMethodParameterSymbol.Ordinal)
			{
				return ContainingSymbol.Equals(reducedExtensionMethodParameterSymbol.ContainingSymbol, compareKind);
			}
			return false;
		}

		public sealed override int GetHashCode()
		{
			return Hash.Combine(ContainingSymbol, _underlyingParameter.Ordinal);
		}

		internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/ReducedExtensionMethodSymbol.cs", 708);
		}
	}

	private readonly MethodSymbol _reducedFrom;

	private readonly TypeMap _typeMap;

	private readonly ImmutableArray<TypeParameterSymbol> _typeParameters;

	private readonly ImmutableArray<TypeWithAnnotations> _typeArguments;

	private ImmutableArray<ParameterSymbol> _lazyParameters;

	internal override MethodSymbol CallsiteReducedFromMethod => _reducedFrom.ConstructIfGeneric(_typeArguments);

	public override TypeSymbol ReceiverType => _reducedFrom.Parameters[0].Type;

	internal override Microsoft.CodeAnalysis.NullableAnnotation ReceiverNullableAnnotation => _reducedFrom.Parameters[0].TypeWithAnnotations.ToPublicAnnotation();

	public override MethodSymbol ReducedFrom => _reducedFrom;

	public override MethodSymbol ConstructedFrom => this;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => _typeParameters;

	public override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotations => _typeArguments;

	internal override CallingConvention CallingConvention => _reducedFrom.CallingConvention;

	public override int Arity => _reducedFrom.Arity;

	public override string Name => _reducedFrom.Name;

	internal override bool HasSpecialName => _reducedFrom.HasSpecialName;

	internal override MethodImplAttributes ImplementationAttributes => _reducedFrom.ImplementationAttributes;

	internal override bool RequiresSecurityObject => _reducedFrom.RequiresSecurityObject;

	public override bool AreLocalsZeroed
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/ReducedExtensionMethodSymbol.cs", 334);
		}
	}

	internal override MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation => _reducedFrom.ReturnValueMarshallingInformation;

	internal override bool HasDeclarativeSecurity => _reducedFrom.HasDeclarativeSecurity;

	public override AssemblySymbol ContainingAssembly => _reducedFrom.ContainingAssembly;

	public override ImmutableArray<Location> Locations => _reducedFrom.Locations;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => _reducedFrom.DeclaringSyntaxReferences;

	public override MethodSymbol OriginalDefinition => this;

	public override bool IsExtern => _reducedFrom.IsExtern;

	public override bool IsSealed => _reducedFrom.IsSealed;

	public override bool IsVirtual => _reducedFrom.IsVirtual;

	public override bool IsAbstract => _reducedFrom.IsAbstract;

	public override bool IsOverride => _reducedFrom.IsOverride;

	public override bool IsStatic => false;

	public override bool IsAsync => _reducedFrom.IsAsync;

	public override bool IsExtensionMethod => true;

	internal override bool IsMetadataFinal => false;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => _reducedFrom.ObsoleteAttributeData;

	internal sealed override bool HasSpecialNameAttribute
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/ReducedExtensionMethodSymbol.cs", 448);
		}
	}

	public override Accessibility DeclaredAccessibility => _reducedFrom.DeclaredAccessibility;

	public override Symbol ContainingSymbol => _reducedFrom.ContainingSymbol;

	public override Symbol AssociatedSymbol => null;

	public override MethodKind MethodKind => MethodKind.ReducedExtension;

	public override bool ReturnsVoid => _reducedFrom.ReturnsVoid;

	public override bool IsGenericMethod => _reducedFrom.IsGenericMethod;

	public override bool IsVararg => _reducedFrom.IsVararg;

	public override RefKind RefKind => _reducedFrom.RefKind;

	public override TypeWithAnnotations ReturnTypeWithAnnotations => _typeMap.SubstituteType(_reducedFrom.ReturnTypeWithAnnotations);

	public override FlowAnalysisAnnotations ReturnTypeFlowAnalysisAnnotations => _reducedFrom.ReturnTypeFlowAnalysisAnnotations;

	public override ImmutableHashSet<string> ReturnNotNullIfParameterNotNull => _reducedFrom.ReturnNotNullIfParameterNotNull;

	public override FlowAnalysisAnnotations FlowAnalysisAnnotations => _reducedFrom.FlowAnalysisAnnotations;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => _typeMap.SubstituteCustomModifiers(_reducedFrom.RefCustomModifiers);

	internal override int ParameterCount => _reducedFrom.ParameterCount - 1;

	internal override bool GenerateDebugInfo => _reducedFrom.GenerateDebugInfo;

	public override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			if (_lazyParameters.IsDefault)
			{
				ImmutableInterlocked.InterlockedInitialize(ref _lazyParameters, MakeParameters());
			}
			return _lazyParameters;
		}
	}

	internal override bool IsExplicitInterfaceImplementation => false;

	internal override bool IsDeclaredReadOnly => false;

	internal override bool IsInitOnly => false;

	internal override bool IsEffectivelyReadOnly
	{
		get
		{
			RefKind refKind = _reducedFrom.Parameters[0].RefKind;
			if (refKind - 3 <= RefKind.Ref)
			{
				return true;
			}
			return false;
		}
	}

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

	public override bool HidesBaseMethodsByName => false;

	protected sealed override bool HasSetsRequiredMembersImpl
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/ReducedExtensionMethodSymbol.cs", 601);
		}
	}

	internal sealed override bool HasUnscopedRefAttribute => false;

	internal sealed override bool UseUpdatedEscapeRules => _reducedFrom.UseUpdatedEscapeRules;

	public static MethodSymbol Create(MethodSymbol method, TypeSymbol receiverType, CSharpCompilation compilation, out bool wasFullyInferred)
	{
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.DiscardedDependencies;
		method = InferExtensionMethodTypeArguments(method, receiverType, compilation, ref useSiteInfo, out wasFullyInferred);
		if ((object)method == null)
		{
			return null;
		}
		if (!((ConversionsBase)(((object)compilation?.Conversions) ?? ((object)method.ContainingAssembly.CorLibrary.TypeConversions))).ConvertExtensionMethodThisArg(method.Parameters[0].Type, receiverType, ref useSiteInfo, false).Exists)
		{
			return null;
		}
		if (useSiteInfo.Diagnostics != null)
		{
			foreach (DiagnosticInfo diagnostic in useSiteInfo.Diagnostics)
			{
				if (diagnostic.Severity == DiagnosticSeverity.Error)
				{
					return null;
				}
			}
		}
		return Create(method);
	}

	public static MethodSymbol Create(MethodSymbol method)
	{
		MethodSymbol constructedFrom = method.ConstructedFrom;
		ReducedExtensionMethodSymbol reducedExtensionMethodSymbol = new ReducedExtensionMethodSymbol(constructedFrom);
		if (constructedFrom == method)
		{
			return reducedExtensionMethodSymbol;
		}
		return reducedExtensionMethodSymbol.Construct(method.TypeArgumentsWithAnnotations);
	}

	private ReducedExtensionMethodSymbol(MethodSymbol reducedFrom)
	{
		_reducedFrom = reducedFrom;
		_typeMap = TypeMap.Empty.WithAlphaRename(reducedFrom, this, propagateAttributes: false, out _typeParameters);
		_typeArguments = _typeMap.SubstituteTypes(reducedFrom.TypeArgumentsWithAnnotations);
	}

	internal static MethodSymbol InferExtensionMethodTypeArguments(MethodSymbol method, TypeSymbol thisType, CSharpCompilation compilation, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, out bool wasFullyInferred)
	{
		if (!method.IsGenericMethod || method != method.ConstructedFrom)
		{
			wasFullyInferred = true;
			return method;
		}
		if (thisType.IsDynamic())
		{
			wasFullyInferred = false;
			return null;
		}
		AssemblySymbol containingAssembly = method.ContainingAssembly;
		NamespaceSymbol globalNamespace = containingAssembly.GlobalNamespace;
		TypeConversions typeConversions = containingAssembly.CorLibrary.TypeConversions;
		CSharpSyntaxNode syntax = (CSharpSyntaxNode)CSharpSyntaxTree.Dummy.GetRoot();
		BoundLiteral boundLiteral = new BoundLiteral(syntax, ConstantValue.Bad, thisType)
		{
			WasCompilerGenerated = true
		};
		BoundLiteral boundLiteral2 = new BoundLiteral(type: new ExtendedErrorTypeSymbol(globalNamespace, string.Empty, 0, null), syntax: syntax, constantValueOpt: ConstantValue.Bad)
		{
			WasCompilerGenerated = true
		};
		int parameterCount = method.ParameterCount;
		BoundExpression[] array = new BoundExpression[parameterCount];
		for (int i = 0; i < parameterCount; i++)
		{
			BoundLiteral boundLiteral3 = ((i == 0) ? boundLiteral : boundLiteral2);
			array[i] = boundLiteral3;
		}
		ImmutableArray<TypeWithAnnotations> immutableArray = MethodTypeInferrer.InferTypeArgumentsFromFirstArgument(compilation, typeConversions, method, array.AsImmutable(), ref useSiteInfo);
		if (immutableArray.IsDefault)
		{
			wasFullyInferred = false;
			return null;
		}
		int num = -1;
		PooledHashSet<TypeParameterSymbol> instance = PooledHashSet<TypeParameterSymbol>.GetInstance();
		ImmutableArray<TypeParameterSymbol> typeParameters = method.TypeParameters;
		ImmutableArray<TypeWithAnnotations> immutableArray2 = immutableArray;
		for (int j = 0; j < immutableArray2.Length; j++)
		{
			if (immutableArray2[j].HasType)
			{
				continue;
			}
			num = j;
			ArrayBuilder<TypeWithAnnotations> instance2 = ArrayBuilder<TypeWithAnnotations>.GetInstance();
			instance2.AddRange(immutableArray2, num);
			for (; j < immutableArray2.Length; j++)
			{
				TypeWithAnnotations item = immutableArray2[j];
				if (!item.HasType)
				{
					instance.Add(typeParameters[j]);
					instance2.Add(TypeWithAnnotations.Create(ErrorTypeSymbol.UnknownResultType));
				}
				else
				{
					instance2.Add(item);
				}
			}
			immutableArray2 = instance2.ToImmutableAndFree();
			break;
		}
		ArrayBuilder<TypeParameterDiagnosticInfo> instance3 = ArrayBuilder<TypeParameterDiagnosticInfo>.GetInstance();
		TypeMap substitution = new TypeMap(typeParameters, immutableArray2);
		ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder = null;
		ConstraintsHelper.CheckConstraintsArgs args = new ConstraintsHelper.CheckConstraintsArgs(compilation, typeConversions, includeNullability: false, NoLocation.Singleton, null, new CompoundUseSiteInfo<AssemblySymbol>(useSiteInfo));
		ImmutableArray<TypeParameterSymbol> typeParameters2 = typeParameters;
		ImmutableArray<TypeWithAnnotations> typeArguments = immutableArray2;
		HashSet<TypeParameterSymbol> ignoreTypeConstraintsDependentOnTypeParametersOpt = ((instance.Count > 0) ? instance : null);
		bool flag = method.CheckConstraints(in args, substitution, typeParameters2, typeArguments, instance3, null, ref useSiteDiagnosticsBuilder, default(BitVector), ignoreTypeConstraintsDependentOnTypeParametersOpt);
		instance3.Free();
		instance.Free();
		if (useSiteDiagnosticsBuilder != null && useSiteDiagnosticsBuilder.Count > 0)
		{
			foreach (TypeParameterDiagnosticInfo item2 in useSiteDiagnosticsBuilder)
			{
				useSiteInfo.Add(item2.UseSiteInfo);
			}
		}
		if (!flag)
		{
			wasFullyInferred = false;
			return null;
		}
		ImmutableArray<TypeWithAnnotations> typeArguments2 = immutableArray;
		wasFullyInferred = immutableArray.All((TypeWithAnnotations t) => t.HasType);
		if (!wasFullyInferred)
		{
			typeArguments2 = immutableArray.ZipAsArray(method.TypeParameters, (TypeWithAnnotations t, TypeParameterSymbol tp) => (!t.HasType) ? TypeWithAnnotations.Create(tp) : t);
		}
		return method.Construct(typeArguments2);
	}

	public override TypeWithAnnotations GetTypeInferredDuringReduction(TypeParameterSymbol reducedFromTypeParameter)
	{
		if ((object)reducedFromTypeParameter == null)
		{
			throw new ArgumentNullException();
		}
		if (reducedFromTypeParameter.ContainingSymbol != _reducedFrom)
		{
			throw new ArgumentException();
		}
		return default(TypeWithAnnotations);
	}

	public override DllImportData GetDllImportData()
	{
		return _reducedFrom.GetDllImportData();
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		return _reducedFrom.GetSecurityInformation();
	}

	internal override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return _reducedFrom.GetAppliedConditionalSymbols();
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return _reducedFrom.GetDocumentationCommentXml(preferredCulture, expandIncludes, cancellationToken);
	}

	internal sealed override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	internal sealed override bool IsMetadataVirtual(IsMetadataVirtualOption option = IsMetadataVirtualOption.None)
	{
		return false;
	}

	internal sealed override UnmanagedCallersOnlyAttributeData GetUnmanagedCallersOnlyAttributeData(bool forceComplete)
	{
		return _reducedFrom.GetUnmanagedCallersOnlyAttributeData(forceComplete);
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return _reducedFrom.GetAttributes();
	}

	internal override bool CallsAreOmitted(SyntaxTree syntaxTree)
	{
		return _reducedFrom.CallsAreOmitted(syntaxTree);
	}

	private ImmutableArray<ParameterSymbol> MakeParameters()
	{
		ImmutableArray<ParameterSymbol> parameters = _reducedFrom.Parameters;
		int length = parameters.Length;
		if (length <= 1)
		{
			return ImmutableArray<ParameterSymbol>.Empty;
		}
		ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance(length - 1);
		for (int i = 0; i < length - 1; i++)
		{
			instance.Add(new ReducedExtensionMethodParameterSymbol(this, parameters[i + 1]));
		}
		return instance.ToImmutableAndFree();
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/ReducedExtensionMethodSymbol.cs", 583);
	}

	internal override bool IsNullableAnalysisEnabled()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/ReducedExtensionMethodSymbol.cs", 586);
	}

	public override bool Equals(Symbol obj, TypeCompareKind compareKind)
	{
		if ((object)this == obj)
		{
			return true;
		}
		if (obj is ReducedExtensionMethodSymbol reducedExtensionMethodSymbol)
		{
			return _reducedFrom.Equals(reducedExtensionMethodSymbol._reducedFrom, compareKind);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _reducedFrom.GetHashCode();
	}

	internal sealed override bool HasAsyncMethodBuilderAttribute(out TypeSymbol builderArgument)
	{
		return _reducedFrom.HasAsyncMethodBuilderAttribute(out builderArgument);
	}

	internal override int TryGetOverloadResolutionPriority()
	{
		return _reducedFrom.TryGetOverloadResolutionPriority();
	}
}
