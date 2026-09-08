using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class SynthesizedClosureMethod : SynthesizedMethodBaseSymbol, ISynthesizedMethodBodyImplementationSymbol, ISymbolInternal
{
	private readonly ImmutableArray<NamedTypeSymbol> _structEnvironments;

	internal readonly DebugId LambdaId;

	internal MethodSymbol TopLevelMethod { get; }

	protected override ImmutableArray<ParameterSymbol> BaseMethodParameters => BaseMethod.Parameters;

	protected override ImmutableArray<TypeSymbol> ExtraSynthesizedRefParameters => ImmutableArray<TypeSymbol>.CastUp(_structEnvironments);

	internal int ExtraSynthesizedParameterCount
	{
		get
		{
			if (!_structEnvironments.IsDefault)
			{
				return _structEnvironments.Length;
			}
			return 0;
		}
	}

	internal override bool InheritsBaseMethodAttributes => true;

	internal override bool GenerateDebugInfo => !IsAsync;

	IMethodSymbolInternal? ISynthesizedMethodBodyImplementationSymbol.Method => TopLevelMethod;

	bool ISynthesizedMethodBodyImplementationSymbol.HasMethodBodyDependency => true;

	public ClosureKind ClosureKind { get; }

	internal SynthesizedClosureMethod(NamedTypeSymbol containingType, ImmutableArray<SynthesizedClosureEnvironment> structEnvironments, ClosureKind closureKind, MethodSymbol topLevelMethod, DebugId topLevelMethodId, MethodSymbol originalMethod, SyntaxReference blockSyntax, DebugId lambdaId, TypeCompilationState compilationState)
		: base(containingType, originalMethod, blockSyntax, originalMethod.DeclaringSyntaxReferences[0].GetLocation(), ((object)originalMethod != null && originalMethod.MethodKind == MethodKind.LocalFunction) ? MakeName(topLevelMethod.Name, originalMethod.Name, topLevelMethodId, closureKind, lambdaId) : MakeName(topLevelMethod.Name, topLevelMethodId, closureKind, lambdaId), MakeDeclarationModifiers(closureKind, originalMethod), originalMethod.IsIterator)
	{
		TopLevelMethod = topLevelMethod;
		ClosureKind = closureKind;
		LambdaId = lambdaId;
		SynthesizedClosureEnvironment synthesizedClosureEnvironment = ContainingType as SynthesizedClosureEnvironment;
		TypeMap typeMap;
		ImmutableArray<TypeParameterSymbol> newTypeParameters;
		switch (closureKind)
		{
		case ClosureKind.Singleton:
		case ClosureKind.General:
			typeMap = synthesizedClosureEnvironment.TypeMap.WithAlphaRename(TypeMap.ConcatMethodTypeParameters(originalMethod, synthesizedClosureEnvironment.OriginalContainingMethodOpt), this, propagateAttributes: false, out newTypeParameters);
			break;
		case ClosureKind.Static:
		case ClosureKind.ThisOnly:
			typeMap = TypeMap.Empty.WithAlphaRename(TypeMap.ConcatMethodTypeParameters(originalMethod, null), this, propagateAttributes: false, out newTypeParameters);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(closureKind);
		}
		if (!structEnvironments.IsDefaultOrEmpty && newTypeParameters.Length != 0)
		{
			ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
			foreach (SynthesizedClosureEnvironment item2 in structEnvironments)
			{
				NamedTypeSymbol item;
				if (item2.Arity == 0)
				{
					item = item2;
				}
				else
				{
					ImmutableArray<TypeParameterSymbol> constructedFromTypeParameters = item2.ConstructedFromTypeParameters;
					ImmutableArray<TypeParameterSymbol> immutableArray = typeMap.SubstituteTypeParameters(constructedFromTypeParameters);
					item = item2.Construct(immutableArray);
				}
				instance.Add(item);
			}
			_structEnvironments = instance.ToImmutableAndFree();
		}
		else
		{
			_structEnvironments = ImmutableArray<NamedTypeSymbol>.CastUp(structEnvironments);
		}
		AssignTypeMapAndTypeParameters(typeMap, newTypeParameters);
		EnsureAttributesExist(compilationState);
	}

	private void EnsureAttributesExist(TypeCompilationState compilationState)
	{
		PEModuleBuilder moduleBuilderOpt = compilationState.ModuleBuilderOpt;
		if (moduleBuilderOpt == null)
		{
			return;
		}
		if (RefKind == RefKind.In)
		{
			moduleBuilderOpt.EnsureIsReadOnlyAttributeExists();
		}
		ParameterHelpers.EnsureRefKindAttributesExist(moduleBuilderOpt, Parameters);
		ParameterHelpers.EnsureParamCollectionAttributeExists(moduleBuilderOpt, Parameters);
		if (moduleBuilderOpt.Compilation.ShouldEmitNativeIntegerAttributes())
		{
			if (base.ReturnType.ContainsNativeIntegerWrapperType())
			{
				moduleBuilderOpt.EnsureNativeIntegerAttributeExists();
			}
			ParameterHelpers.EnsureNativeIntegerAttributeExists(moduleBuilderOpt, Parameters);
		}
		ParameterHelpers.EnsureScopedRefAttributeExists(moduleBuilderOpt, Parameters);
		if (compilationState.Compilation.ShouldEmitNullableAttributes(this))
		{
			if (ShouldEmitNullableContextValue(out var _))
			{
				moduleBuilderOpt.EnsureNullableContextAttributeExists();
			}
			if (ReturnTypeWithAnnotations.NeedsNullableAttribute())
			{
				moduleBuilderOpt.EnsureNullableAttributeExists();
			}
		}
		ParameterHelpers.EnsureNullableAttributeExists(moduleBuilderOpt, this, Parameters);
	}

	private static DeclarationModifiers MakeDeclarationModifiers(ClosureKind closureKind, MethodSymbol originalMethod)
	{
		DeclarationModifiers declarationModifiers = ((closureKind == ClosureKind.ThisOnly) ? DeclarationModifiers.Private : DeclarationModifiers.Internal);
		if (closureKind == ClosureKind.Static)
		{
			declarationModifiers |= DeclarationModifiers.Static;
		}
		if (originalMethod.IsAsync)
		{
			declarationModifiers |= DeclarationModifiers.Async;
		}
		if (originalMethod.IsExtern)
		{
			declarationModifiers |= DeclarationModifiers.Extern;
		}
		return declarationModifiers;
	}

	private static string MakeName(string topLevelMethodName, string localFunctionName, DebugId topLevelMethodId, ClosureKind closureKind, DebugId lambdaId)
	{
		return GeneratedNames.MakeLocalFunctionName(topLevelMethodName, localFunctionName, (closureKind == ClosureKind.General) ? (-1) : topLevelMethodId.Ordinal, topLevelMethodId.Generation, lambdaId.Ordinal, lambdaId.Generation);
	}

	private static string MakeName(string topLevelMethodName, DebugId topLevelMethodId, ClosureKind closureKind, DebugId lambdaId)
	{
		return GeneratedNames.MakeLambdaMethodName(topLevelMethodName, (closureKind == ClosureKind.General) ? (-1) : topLevelMethodId.Ordinal, topLevelMethodId.Generation, lambdaId.Ordinal, lambdaId.Generation);
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		return TopLevelMethod.CalculateLocalSyntaxOffset(localPosition, localTree);
	}

	internal override ExecutableCodeBinder? TryGetBodyBinder(BinderFactory? binderFactoryOpt = null, bool ignoreAccessibility = false)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/ClosureConversion/SynthesizedClosureMethod.cs", 238);
	}
}
