using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Debugging;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.ErrorReporting;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class MethodCompiler : CSharpSymbolVisitor<TypeCompilationState, object>
{
	private readonly CSharpCompilation _compilation;

	private readonly bool _emittingPdb;

	private readonly CancellationToken _cancellationToken;

	private readonly BindingDiagnosticBag _diagnostics;

	private readonly bool _hasDeclarationErrors;

	private readonly bool _emitMethodBodies;

	private readonly PEModuleBuilder? _moduleBeingBuiltOpt;

	private readonly Predicate<Symbol>? _filterOpt;

	private readonly SynthesizedEntryPointSymbol.AsyncForwardEntryPoint? _entryPointOpt;

	private DebugDocumentProvider? _lazyDebugDocumentProvider;

	private ConcurrentStack<Task>? _compilerTasks;

	private bool _globalHasErrors;

	private bool ReportNullableDiagnostics
	{
		get
		{
			PEModuleBuilder? moduleBeingBuiltOpt = _moduleBeingBuiltOpt;
			if (moduleBeingBuiltOpt == null)
			{
				return true;
			}
			return !moduleBeingBuiltOpt.IsEncDelta;
		}
	}

	private void SetGlobalErrorIfTrue(bool arg)
	{
		if (arg)
		{
			_globalHasErrors = true;
		}
	}

	internal MethodCompiler(CSharpCompilation compilation, PEModuleBuilder? moduleBeingBuiltOpt, bool emittingPdb, bool hasDeclarationErrors, bool emitMethodBodies, BindingDiagnosticBag diagnostics, Predicate<Symbol>? filterOpt, SynthesizedEntryPointSymbol.AsyncForwardEntryPoint? entryPointOpt, CancellationToken cancellationToken)
	{
		_compilation = compilation;
		_moduleBeingBuiltOpt = moduleBeingBuiltOpt;
		_emittingPdb = emittingPdb;
		_cancellationToken = cancellationToken;
		_diagnostics = diagnostics;
		_filterOpt = filterOpt;
		_entryPointOpt = entryPointOpt;
		_hasDeclarationErrors = hasDeclarationErrors;
		SetGlobalErrorIfTrue(hasDeclarationErrors);
		_emitMethodBodies = emitMethodBodies;
	}

	public static void CompileMethodBodies(CSharpCompilation compilation, PEModuleBuilder? moduleBeingBuiltOpt, bool emittingPdb, bool hasDeclarationErrors, bool emitMethodBodies, BindingDiagnosticBag diagnostics, Predicate<Symbol>? filterOpt, CancellationToken cancellationToken)
	{
		hasDeclarationErrors |= compilation.CheckDuplicateInterceptions(diagnostics);
		if (compilation.PreviousSubmission != null)
		{
			compilation.PreviousSubmission.EnsureAnonymousTypeTemplates(cancellationToken);
		}
		MethodSymbol methodSymbol = null;
		if (filterOpt == null)
		{
			methodSymbol = GetEntryPoint(compilation, moduleBeingBuiltOpt, hasDeclarationErrors, emitMethodBodies, diagnostics, cancellationToken);
		}
		MethodCompiler methodCompiler = new MethodCompiler(compilation, moduleBeingBuiltOpt, emittingPdb, hasDeclarationErrors, emitMethodBodies, diagnostics, filterOpt, methodSymbol as SynthesizedEntryPointSymbol.AsyncForwardEntryPoint, cancellationToken);
		if (compilation.Options.ConcurrentBuild)
		{
			methodCompiler._compilerTasks = new ConcurrentStack<Task>();
		}
		methodCompiler.CompileNamespace(compilation.SourceModule.GlobalNamespace);
		methodCompiler.WaitForWorkers();
		if (moduleBeingBuiltOpt != null)
		{
			ImmutableArray<NamedTypeSymbol> additionalTopLevelTypes = moduleBeingBuiltOpt.GetAdditionalTopLevelTypes();
			methodCompiler.CompileSynthesizedMethods(additionalTopLevelTypes, diagnostics);
			ImmutableArray<NamedTypeSymbol> embeddedTypes = moduleBeingBuiltOpt.GetEmbeddedTypes(diagnostics);
			methodCompiler.CompileSynthesizedMethods(embeddedTypes, diagnostics);
			INamedTypeSymbolInternal namedTypeSymbolInternal = moduleBeingBuiltOpt.TryGetOrCreateSynthesizedHotReloadExceptionType();
			if (namedTypeSymbolInternal != null)
			{
				methodCompiler.CompileSynthesizedMethods(ImmutableCollectionsMarshal.AsImmutableArray(new NamedTypeSymbol[1] { (NamedTypeSymbol)namedTypeSymbolInternal }), diagnostics);
			}
			if (emitMethodBodies)
			{
				compilation.AnonymousTypeManager.AssignTemplatesNamesAndCompile(methodCompiler, moduleBeingBuiltOpt, diagnostics);
			}
			methodCompiler.WaitForWorkers();
			moduleBeingBuiltOpt.CreateDeletedMemberDefinitions(diagnostics.DiagnosticBag);
			PrivateImplementationDetails privateImplementationDetails = moduleBeingBuiltOpt.FreezePrivateImplementationDetails();
			if (privateImplementationDetails != null)
			{
				methodCompiler.CompileSynthesizedMethods(privateImplementationDetails, diagnostics);
			}
		}
		if (moduleBeingBuiltOpt != null && (methodCompiler._globalHasErrors || moduleBeingBuiltOpt.SourceModule.HasBadAttributes) && !diagnostics.HasAnyErrors() && !hasDeclarationErrors)
		{
			string nameOfLocalizableResource = (methodCompiler._globalHasErrors ? "UnableToDetermineSpecificCauseOfFailure" : "ModuleHasInvalidAttributes");
			diagnostics.Add(ErrorCode.ERR_ModuleEmitFailure, NoLocation.Singleton, ((INamedEntity)moduleBeingBuiltOpt).Name, new LocalizableResourceString(nameOfLocalizableResource, CodeAnalysisResources.ResourceManager, typeof(CodeAnalysisResources)));
		}
		diagnostics.AddRange(compilation.AdditionalCodegenWarnings);
		if (filterOpt == null)
		{
			WarnUnusedFields(compilation, diagnostics, cancellationToken);
			if (moduleBeingBuiltOpt != null && methodSymbol != null && compilation.Options.OutputKind.IsApplication())
			{
				moduleBeingBuiltOpt.SetPEEntryPoint(methodSymbol, diagnostics.DiagnosticBag);
			}
		}
	}

	internal static MethodSymbol GetEntryPoint(CSharpCompilation compilation, PEModuleBuilder moduleBeingBuilt, bool hasDeclarationErrors, bool emitMethodBodies, BindingDiagnosticBag diagnostics, CancellationToken cancellationToken)
	{
		CSharpCompilation.EntryPoint entryPointAndDiagnostics = compilation.GetEntryPointAndDiagnostics(cancellationToken);
		diagnostics.AddRange(entryPointAndDiagnostics.Diagnostics, allowMismatchInDependencyAccumulation: true);
		MethodSymbol methodSymbol = entryPointAndDiagnostics.MethodSymbol;
		if ((object)methodSymbol == null)
		{
			return null;
		}
		SynthesizedEntryPointSymbol synthesizedEntryPointSymbol = methodSymbol as SynthesizedEntryPointSymbol;
		if ((object)synthesizedEntryPointSymbol == null)
		{
			TypeSymbol returnType = methodSymbol.ReturnType;
			if (returnType.IsGenericTaskType(compilation) || returnType.IsNonGenericTaskType(compilation))
			{
				synthesizedEntryPointSymbol = new SynthesizedEntryPointSymbol.AsyncForwardEntryPoint(compilation, methodSymbol.ContainingType, methodSymbol);
				methodSymbol = synthesizedEntryPointSymbol;
				moduleBeingBuilt?.AddSynthesizedDefinition(methodSymbol.ContainingType, synthesizedEntryPointSymbol.GetCciAdapter());
			}
		}
		if ((object)synthesizedEntryPointSymbol != null && moduleBeingBuilt != null && !hasDeclarationErrors && !moduleBeingBuilt.EmitOptions.EmitMetadataOnly && !diagnostics.HasAnyErrors())
		{
			BoundStatement boundStatement = synthesizedEntryPointSymbol.CreateBody(diagnostics);
			if (boundStatement.HasErrors || diagnostics.HasAnyErrors())
			{
				return methodSymbol;
			}
			VariableSlotAllocator lazyVariableSlotAllocator = null;
			ArrayBuilder<EncLambdaInfo> instance = ArrayBuilder<EncLambdaInfo>.GetInstance();
			ArrayBuilder<LambdaRuntimeRudeEditInfo> instance2 = ArrayBuilder<LambdaRuntimeRudeEditInfo>.GetInstance();
			ArrayBuilder<EncClosureInfo> instance3 = ArrayBuilder<EncClosureInfo>.GetInstance();
			ArrayBuilder<StateMachineStateDebugInfo> instance4 = ArrayBuilder<StateMachineStateDebugInfo>.GetInstance();
			StateMachineTypeSymbol stateMachineTypeOpt = null;
			BoundStatement block = LowerBodyOrInitializer(synthesizedEntryPointSymbol, null, -1, boundStatement, null, new TypeCompilationState(synthesizedEntryPointSymbol.ContainingType, compilation, moduleBeingBuilt), MethodInstrumentation.Empty, null, out var _, diagnostics, ref lazyVariableSlotAllocator, instance, instance2, instance3, instance4, out stateMachineTypeOpt);
			instance.Free();
			instance2.Free();
			instance3.Free();
			instance4.Free();
			if (emitMethodBodies)
			{
				MethodBody body = GenerateMethodBody(moduleBeingBuilt, synthesizedEntryPointSymbol, -1, block, ImmutableArray<EncLambdaInfo>.Empty, ImmutableArray<LambdaRuntimeRudeEditInfo>.Empty, ImmutableArray<EncClosureInfo>.Empty, ImmutableArray<StateMachineStateDebugInfo>.Empty, null, null, diagnostics, null, null, emittingPdb: false, ImmutableArray<SourceSpan>.Empty, null);
				moduleBeingBuilt.SetMethodBody(synthesizedEntryPointSymbol, body);
			}
		}
		return methodSymbol;
	}

	private void WaitForWorkers()
	{
		ConcurrentStack<Task> compilerTasks = _compilerTasks;
		if (compilerTasks != null)
		{
			Task result;
			while (compilerTasks.TryPop(out result))
			{
				result.GetAwaiter().GetResult();
			}
		}
	}

	private static void WarnUnusedFields(CSharpCompilation compilation, BindingDiagnosticBag diagnostics, CancellationToken cancellationToken)
	{
		SourceAssemblySymbol sourceAssemblySymbol = (SourceAssemblySymbol)compilation.Assembly;
		diagnostics.AddRange(sourceAssemblySymbol.GetUnusedFieldWarnings(cancellationToken));
	}

	private DebugDocumentProvider GetDebugDocumentProvider(MethodInstrumentation instrumentation)
	{
		if (_emittingPdb || instrumentation.Kinds.Contains(InstrumentationKind.TestCoverage))
		{
			return (string path, string basePath) => _moduleBeingBuiltOpt.DebugDocumentsBuilder.GetOrAddDebugDocument(path, basePath, CreateDebugDocumentForFile);
		}
		return null;
	}

	public override object VisitNamespace(NamespaceSymbol symbol, TypeCompilationState arg)
	{
		if (!PassesFilter(_filterOpt, symbol))
		{
			return null;
		}
		arg = null;
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		if (_compilation.Options.ConcurrentBuild)
		{
			Task item = CompileNamespaceAsAsync(symbol);
			_compilerTasks.Push(item);
		}
		else
		{
			CompileNamespace(symbol);
		}
		return null;
	}

	private Task CompileNamespaceAsAsync(NamespaceSymbol symbol)
	{
		return Task.Run(UICultureUtilities.WithCurrentUICulture(delegate
		{
			try
			{
				CompileNamespace(symbol);
			}
			catch (Exception exception) when (FatalError.ReportAndPropagateUnlessCanceled(exception))
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Compiler/MethodCompiler.cs", 404);
			}
		}), _cancellationToken);
	}

	private void CompileNamespace(NamespaceSymbol symbol)
	{
		foreach (Symbol item in symbol.GetMembersUnordered())
		{
			item.Accept(this, null);
		}
	}

	public override object VisitNamedType(NamedTypeSymbol symbol, TypeCompilationState arg)
	{
		if (!PassesFilter(_filterOpt, symbol))
		{
			return null;
		}
		arg = null;
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		if (_compilation.Options.ConcurrentBuild)
		{
			Task item = CompileNamedTypeAsync(symbol);
			_compilerTasks.Push(item);
		}
		else
		{
			CompileNamedType(symbol);
		}
		return null;
	}

	private Task CompileNamedTypeAsync(NamedTypeSymbol symbol)
	{
		return Task.Run(UICultureUtilities.WithCurrentUICulture(delegate
		{
			try
			{
				CompileNamedType(symbol);
			}
			catch (Exception exception) when (FatalError.ReportAndPropagateUnlessCanceled(exception))
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Compiler/MethodCompiler.cs", 450);
			}
		}), _cancellationToken);
	}

	private void CompileNamedType(NamedTypeSymbol containingType)
	{
		TypeCompilationState typeCompilationState = new TypeCompilationState(containingType, _compilation, _moduleBeingBuiltOpt);
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		SynthesizedInstanceConstructor synthesizedInstanceConstructor = null;
		SynthesizedInteractiveInitializerMethod scriptInitializerOpt = null;
		SynthesizedEntryPointSymbol synthesizedEntryPointSymbol = null;
		int methodOrdinal = -1;
		if (containingType.IsScriptClass)
		{
			synthesizedInstanceConstructor = containingType.GetScriptConstructor();
			scriptInitializerOpt = containingType.GetScriptInitializer();
			synthesizedEntryPointSymbol = containingType.GetScriptEntryPoint();
		}
		SynthesizedSubmissionFields synthesizedSubmissionFields = (containingType.IsSubmissionClass ? new SynthesizedSubmissionFields(_compilation, containingType) : null);
		Binder.ProcessedFieldInitializers processedInitializers = default(Binder.ProcessedFieldInitializers);
		Binder.ProcessedFieldInitializers processedInitializers2 = default(Binder.ProcessedFieldInitializers);
		SourceMemberContainerTypeSymbol sourceMemberContainerTypeSymbol = containingType as SourceMemberContainerTypeSymbol;
		if ((object)sourceMemberContainerTypeSymbol != null)
		{
			cancellationToken = _cancellationToken;
			cancellationToken.ThrowIfCancellationRequested();
			Binder.BindFieldInitializers(_compilation, scriptInitializerOpt, sourceMemberContainerTypeSymbol.StaticInitializers, _diagnostics, ref processedInitializers);
			cancellationToken = _cancellationToken;
			cancellationToken.ThrowIfCancellationRequested();
			Binder.BindFieldInitializers(_compilation, scriptInitializerOpt, sourceMemberContainerTypeSymbol.InstanceInitializers, _diagnostics, ref processedInitializers2);
			if (typeCompilationState.Emitting)
			{
				CompileSynthesizedExplicitImplementations(sourceMemberContainerTypeSymbol, typeCompilationState);
			}
		}
		ImmutableArray<Symbol> members = containingType.GetMembers();
		if (containingType.IsExtension)
		{
			MethodSymbol methodSymbol = ((SourceNamedTypeSymbol)containingType).TryGetOrCreateExtensionMarker();
			if ((object)methodSymbol != null)
			{
				Binder.ProcessedFieldInitializers processedInitializers3 = default(Binder.ProcessedFieldInitializers);
				CompileMethod(methodSymbol, -1, ref processedInitializers3, synthesizedSubmissionFields, typeCompilationState);
			}
		}
		for (int i = 0; i < members.Length; i++)
		{
			Symbol symbol = members[i];
			if (!PassesFilter(_filterOpt, symbol))
			{
				continue;
			}
			switch (symbol.Kind)
			{
			case SymbolKind.NamedType:
				symbol.Accept(this, typeCompilationState);
				break;
			case SymbolKind.Method:
			{
				MethodSymbol methodSymbol2 = (MethodSymbol)symbol;
				if (methodSymbol2.IsScriptConstructor)
				{
					methodOrdinal = i;
				}
				else
				{
					if ((object)methodSymbol2 == synthesizedEntryPointSymbol)
					{
						break;
					}
					methodSymbol2 = GetMethodToCompile(methodSymbol2);
					if ((object)methodSymbol2 != null)
					{
						Binder.ProcessedFieldInitializers processedInitializers4 = ((methodSymbol2.MethodKind == MethodKind.Constructor || methodSymbol2.IsScriptInitializer) ? processedInitializers2 : ((methodSymbol2.MethodKind == MethodKind.StaticConstructor) ? processedInitializers : default(Binder.ProcessedFieldInitializers)));
						if (containingType.IsExtension && (object)methodSymbol2.TryGetCorrespondingExtensionImplementationMethod() != null)
						{
							EmitSkeletonMethodInExtension(methodSymbol2);
						}
						else
						{
							CompileMethod(methodSymbol2, i, ref processedInitializers4, synthesizedSubmissionFields, typeCompilationState);
						}
					}
				}
				break;
			}
			case SymbolKind.Property:
				if (symbol is SourcePropertySymbolBase { IsSealed: not false } sourcePropertySymbolBase && typeCompilationState.Emitting)
				{
					CompileSynthesizedSealedAccessors(sourcePropertySymbolBase, typeCompilationState);
				}
				break;
			case SymbolKind.Field:
			{
				FieldSymbol fieldSymbol = (FieldSymbol)symbol;
				if (!(symbol is TupleErrorFieldSymbol))
				{
					if (fieldSymbol.IsConst)
					{
						ConstantValue constantValue = fieldSymbol.GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false);
						SetGlobalErrorIfTrue(constantValue == null || constantValue.IsBad);
					}
					if (fieldSymbol.IsFixedSizeBuffer && typeCompilationState.Emitting)
					{
						fieldSymbol.FixedImplementationType(typeCompilationState.ModuleBuilderOpt);
					}
				}
				break;
			}
			}
		}
		if (AnonymousTypeManager.IsAnonymousTypeTemplate(containingType))
		{
			Binder.ProcessedFieldInitializers processedInitializers5 = default(Binder.ProcessedFieldInitializers);
			foreach (MethodSymbol anonymousTypeHiddenMethod in AnonymousTypeManager.GetAnonymousTypeHiddenMethods(containingType))
			{
				CompileMethod(anonymousTypeHiddenMethod, -1, ref processedInitializers5, synthesizedSubmissionFields, typeCompilationState);
			}
		}
		bool flag;
		bool flag2;
		if (containingType.StaticConstructors.IsEmpty)
		{
			if (_moduleBeingBuiltOpt != null && !processedInitializers.BoundInitializers.IsDefaultOrEmpty)
			{
				MethodSymbol methodSymbol3 = new SynthesizedStaticConstructor(sourceMemberContainerTypeSymbol);
				if (PassesFilter(_filterOpt, methodSymbol3))
				{
					CompileMethod(methodSymbol3, -1, ref processedInitializers, synthesizedSubmissionFields, typeCompilationState);
					if (_moduleBeingBuiltOpt.GetMethodBody(methodSymbol3) != null)
					{
						_moduleBeingBuiltOpt.AddSynthesizedDefinition(sourceMemberContainerTypeSymbol, methodSymbol3.GetCciAdapter());
					}
				}
			}
			flag = processedInitializers.BoundInitializers.IsDefaultOrEmpty && _compilation.LanguageVersion >= MessageID.IDS_FeatureNullableReferenceTypes.RequiredVersion();
			if (flag)
			{
				if ((object)containingType != null && !containingType.IsImplicitlyDeclared)
				{
					TypeKind typeKind = containingType.TypeKind;
					if (typeKind == TypeKind.Class || typeKind == TypeKind.Interface || typeKind == TypeKind.Struct)
					{
						flag2 = true;
						goto IL_03d4;
					}
				}
				flag2 = false;
				goto IL_03d4;
			}
			goto IL_03d8;
		}
		goto IL_040c;
		IL_040c:
		if (synthesizedInstanceConstructor != null && typeCompilationState.Emitting)
		{
			Binder.ProcessedFieldInitializers processedInitializers6 = new Binder.ProcessedFieldInitializers
			{
				BoundInitializers = ImmutableArray<BoundInitializer>.Empty
			};
			CompileMethod(synthesizedInstanceConstructor, methodOrdinal, ref processedInitializers6, synthesizedSubmissionFields, typeCompilationState);
			synthesizedSubmissionFields?.AddToType(containingType, typeCompilationState.ModuleBuilderOpt);
		}
		if (_moduleBeingBuiltOpt != null)
		{
			CompileSynthesizedMethods(typeCompilationState);
		}
		typeCompilationState.Free();
		return;
		IL_03d4:
		flag = flag2;
		goto IL_03d8;
		IL_03d8:
		if (flag && ReportNullableDiagnostics)
		{
			NullableWalker.AnalyzeIfNeeded(_compilation, new SynthesizedStaticConstructor(containingType), GetSynthesizedEmptyBody(containingType), _diagnostics.DiagnosticBag, useConstructorExitWarnings: true, null, getFinalNullableState: false, null, out NullableWalker.VariableState _);
		}
		goto IL_040c;
	}

	internal static MethodSymbol GetMethodToCompile(MethodSymbol method)
	{
		if (method.IsPartialDefinition())
		{
			return method.PartialImplementationPart;
		}
		return method;
	}

	private void CompileSynthesizedMethods(PrivateImplementationDetails privateImplClass, BindingDiagnosticBag diagnostics)
	{
		TypeCompilationState typeCompilationState = new TypeCompilationState(null, _compilation, _moduleBeingBuiltOpt);
		EmitContext context = new EmitContext(_moduleBeingBuiltOpt, null, diagnostics.DiagnosticBag, metadataOnly: false, includePrivateMembers: true);
		foreach (IMethodDefinition method in privateImplClass.GetMethods(context))
		{
			((MethodSymbol)method.GetInternalSymbol())?.GenerateMethodBody(typeCompilationState, diagnostics);
		}
		CompileSynthesizedMethods(typeCompilationState);
		typeCompilationState.Free();
	}

	private void CompileSynthesizedMethods(ImmutableArray<NamedTypeSymbol> additionalTypes, BindingDiagnosticBag diagnostics)
	{
		foreach (NamedTypeSymbol item in additionalTypes)
		{
			TypeCompilationState typeCompilationState = new TypeCompilationState(item, _compilation, _moduleBeingBuiltOpt);
			foreach (MethodSymbol item2 in item.GetMethodsToEmit())
			{
				item2.GenerateMethodBody(typeCompilationState, diagnostics);
			}
			if (!diagnostics.HasAnyErrors())
			{
				CompileSynthesizedMethods(typeCompilationState);
			}
			typeCompilationState.Free();
			CompileSynthesizedMethods(item.GetTypeMembers(), diagnostics);
		}
	}

	private void CompileSynthesizedMethods(TypeCompilationState compilationState)
	{
		ArrayBuilder<TypeCompilationState.MethodWithBody> synthesizedMethods = compilationState.SynthesizedMethods;
		if (synthesizedMethods == null)
		{
			return;
		}
		ArrayBuilder<StateMachineStateDebugInfo> instance = ArrayBuilder<StateMachineStateDebugInfo>.GetInstance();
		ImportChain currentImportChain = compilationState.CurrentImportChain;
		try
		{
			foreach (TypeCompilationState.MethodWithBody item in synthesizedMethods)
			{
				ImportChain importChain = (compilationState.CurrentImportChain = item.ImportChain);
				BindingDiagnosticBag instance2 = BindingDiagnosticBag.GetInstance(_diagnostics);
				MethodSymbol method = item.Method;
				VariableSlotAllocator variableSlotAllocator = ((method is SynthesizedClosureMethod synthesizedClosureMethod) ? _moduleBeingBuiltOpt.TryCreateVariableSlotAllocator(synthesizedClosureMethod, synthesizedClosureMethod.TopLevelMethod, instance2.DiagnosticBag) : _moduleBeingBuiltOpt.TryCreateVariableSlotAllocator(method, method, instance2.DiagnosticBag));
				MethodBody methodBody = null;
				try
				{
					BoundStatement boundStatement = IteratorRewriter.Rewrite(item.Body, method, -1, instance, variableSlotAllocator, compilationState, instance2, out var stateMachineType);
					StateMachineTypeSymbol stateMachineTypeSymbol = stateMachineType;
					if (!boundStatement.HasErrors)
					{
						AsyncStateMachine stateMachineType2 = null;
						boundStatement = ((!compilationState.Compilation.IsRuntimeAsyncEnabledIn(method)) ? AsyncRewriter.Rewrite(boundStatement, method, -1, instance, variableSlotAllocator, compilationState, instance2, out stateMachineType2) : RuntimeAsyncRewriter.Rewrite(boundStatement, method, compilationState, instance2));
						stateMachineTypeSymbol = stateMachineTypeSymbol ?? stateMachineType2;
					}
					SetGlobalErrorIfTrue(instance2.HasAnyErrors());
					if (_emitMethodBodies && !instance2.HasAnyErrors() && !_globalHasErrors)
					{
						methodBody = GenerateMethodBody(_moduleBeingBuiltOpt, method, -1, boundStatement, ImmutableArray<EncLambdaInfo>.Empty, ImmutableArray<LambdaRuntimeRudeEditInfo>.Empty, ImmutableArray<EncClosureInfo>.Empty, instance.ToImmutable(), stateMachineTypeSymbol, variableSlotAllocator, instance2, GetDebugDocumentProvider(MethodInstrumentation.Empty), method.GenerateDebugInfo ? importChain : null, _emittingPdb, ImmutableArray<SourceSpan>.Empty, _entryPointOpt);
					}
				}
				catch (BoundTreeVisitor.CancelledByStackGuardException ex)
				{
					ex.AddAnError(_diagnostics);
				}
				_diagnostics.AddRange(instance2);
				instance2.Free();
				if (_emitMethodBodies)
				{
					if (methodBody == null)
					{
						break;
					}
					_moduleBeingBuiltOpt.SetMethodBody(method, methodBody);
				}
				instance.Clear();
			}
		}
		finally
		{
			compilationState.CurrentImportChain = currentImportChain;
			instance.Free();
		}
	}

	private void CompileSynthesizedExplicitImplementations(SourceMemberContainerTypeSymbol sourceTypeSymbol, TypeCompilationState compilationState)
	{
		if (_globalHasErrors)
		{
			return;
		}
		ImmutableArray<NamedTypeSymbol> interfacesToEmit = sourceTypeSymbol.GetInterfacesToEmit();
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(_diagnostics);
		foreach (SynthesizedExplicitImplementationForwardingMethod forwardingMethod in sourceTypeSymbol.GetSynthesizedExplicitImplementations(_cancellationToken).ForwardingMethods)
		{
			if (interfacesToEmit.Contains(forwardingMethod.ExplicitInterfaceImplementations[0].ContainingType, Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything))
			{
				forwardingMethod.GenerateMethodBody(compilationState, instance);
				instance.DiagnosticBag.Clear();
				_moduleBeingBuiltOpt.AddSynthesizedDefinition(sourceTypeSymbol, forwardingMethod.GetCciAdapter());
			}
		}
		_diagnostics.AddRangeAndFree(instance);
	}

	private void CompileSynthesizedSealedAccessors(SourcePropertySymbolBase sourceProperty, TypeCompilationState compilationState)
	{
		SynthesizedSealedPropertyAccessor synthesizedSealedAccessorOpt = sourceProperty.SynthesizedSealedAccessorOpt;
		if ((object)synthesizedSealedAccessorOpt != null && !_globalHasErrors)
		{
			BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(_diagnostics);
			synthesizedSealedAccessorOpt.GenerateMethodBody(compilationState, instance);
			_diagnostics.AddDependencies(instance);
			instance.Free();
			_moduleBeingBuiltOpt.AddSynthesizedDefinition(sourceProperty.ContainingType, synthesizedSealedAccessorOpt.GetCciAdapter());
		}
	}

	public override object VisitMethod(MethodSymbol symbol, TypeCompilationState arg)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Compiler/MethodCompiler.cs", 905);
	}

	public override object VisitProperty(PropertySymbol symbol, TypeCompilationState argument)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Compiler/MethodCompiler.cs", 910);
	}

	public override object VisitEvent(EventSymbol symbol, TypeCompilationState argument)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Compiler/MethodCompiler.cs", 915);
	}

	public override object VisitField(FieldSymbol symbol, TypeCompilationState argument)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Compiler/MethodCompiler.cs", 920);
	}

	private void CompileMethod(MethodSymbol methodSymbol, int methodOrdinal, ref Binder.ProcessedFieldInitializers processedInitializers, SynthesizedSubmissionFields previousSubmissionFields, TypeCompilationState compilationState)
	{
		SourceExtensionImplementationMethodSymbol sourceExtensionImplementationMethodSymbol = methodSymbol as SourceExtensionImplementationMethodSymbol;
		if ((object)sourceExtensionImplementationMethodSymbol != null)
		{
			methodSymbol = sourceExtensionImplementationMethodSymbol.UnderlyingMethod;
		}
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		SourceMemberMethodSymbol sourceMemberMethodSymbol = methodSymbol as SourceMemberMethodSymbol;
		if (!methodSymbol.IsAbstract)
		{
			NamedTypeSymbol containingType = methodSymbol.ContainingType;
			if ((object)containingType == null || !containingType.IsDelegateType())
			{
				if (_moduleBeingBuiltOpt == null && (object)sourceMemberMethodSymbol != null)
				{
					ImmutableArray<Diagnostic> diagnostics = sourceMemberMethodSymbol.Diagnostics;
					if (!diagnostics.IsDefault)
					{
						_diagnostics.AddRange(diagnostics);
						return;
					}
				}
				ImportChain currentImportChain = compilationState.CurrentImportChain;
				BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(_diagnostics);
				try
				{
					if (methodSymbol.SynthesizesLoweredBoundBody)
					{
						if (_moduleBeingBuiltOpt != null)
						{
							methodSymbol.GenerateMethodBody(compilationState, instance);
							_diagnostics.AddRange(instance);
						}
					}
					else
					{
						if (methodSymbol.IsDefaultValueTypeConstructor())
						{
							return;
						}
						bool flag = false;
						bool originalBodyNested = false;
						MethodInstrumentation instrumentation = compilationState.ModuleBuilderOpt?.GetMethodBodyInstrumentations(methodSymbol) ?? MethodInstrumentation.Empty;
						BoundStatementList boundStatementList = null;
						MethodBodySemanticModel.InitialState forSemanticModel = default(MethodBodySemanticModel.InitialState);
						ImportChain importChain = null;
						bool hasTrailingExpression = false;
						BoundBlock boundBlock;
						ImmutableArray<FieldSymbol> implicitlyInitializedFieldsOpt;
						if (methodSymbol.IsScriptConstructor)
						{
							boundBlock = new BoundBlock(methodSymbol.GetNonNullSyntaxNode(), ImmutableArray<LocalSymbol>.Empty, ImmutableArray<BoundStatement>.Empty)
							{
								WasCompilerGenerated = true
							};
						}
						else if (methodSymbol.IsScriptInitializer)
						{
							BoundTypeOrInstanceInitializers boundTypeOrInstanceInitializers = InitializerRewriter.RewriteScriptInitializer(processedInitializers.BoundInitializers, (SynthesizedInteractiveInitializerMethod)methodSymbol, out hasTrailingExpression);
							boundBlock = BoundBlock.SynthesizedNoLocals(boundTypeOrInstanceInitializers.Syntax, boundTypeOrInstanceInitializers.Statements);
							if (ReportNullableDiagnostics)
							{
								NullableWalker.AnalyzeIfNeeded(_compilation, methodSymbol, boundTypeOrInstanceInitializers, instance.DiagnosticBag, useConstructorExitWarnings: false, null, getFinalNullableState: true, null, out NullableWalker.VariableState _);
							}
							DiagnosticBag instance2 = DiagnosticBag.GetInstance();
							DefiniteAssignmentPass.Analyze(_compilation, methodSymbol, boundTypeOrInstanceInitializers, instance2, out implicitlyInitializedFieldsOpt, requireOutParamsAssigned: false);
							DiagnosticsPass.IssueDiagnostics(_compilation, boundTypeOrInstanceInitializers, BindingDiagnosticBag.Discarded, methodSymbol);
							instance2.Free();
						}
						else
						{
							bool flag2 = methodSymbol.IncludeFieldInitializersInBody();
							flag = flag2 && !processedInitializers.BoundInitializers.IsDefaultOrEmpty;
							if (flag && processedInitializers.LoweredInitializers == null)
							{
								boundStatementList = InitializerRewriter.RewriteConstructor(processedInitializers.BoundInitializers, methodSymbol);
								processedInitializers.HasErrors = processedInitializers.HasErrors || boundStatementList.HasAnyErrors;
								RefSafetyAnalysis.Analyze(_compilation, methodSymbol, new BoundBlock(boundStatementList.Syntax, ImmutableArray<LocalSymbol>.Empty, boundStatementList.Statements), instance);
							}
							boundBlock = BindMethodBody(methodSymbol, compilationState, instance, flag2, boundStatementList, ReportNullableDiagnostics, out importChain, out originalBodyNested, out bool prependedDefaultValueTypeConstructorInitializer, out forSemanticModel);
							if (instance.HasAnyErrors() && boundBlock != null)
							{
								boundBlock = (BoundBlock)boundBlock.WithHasErrors();
							}
							if (flag && processedInitializers.LoweredInitializers == null)
							{
								if (boundBlock != null && ((methodSymbol.ContainingType.IsStructType() && !methodSymbol.IsImplicitConstructor) || methodSymbol is SynthesizedPrimaryConstructor || instrumentation.Kinds.Contains(InstrumentationKind.TestCoverage) || instrumentation.Kinds.Contains((InstrumentationKind)(-1)) || instrumentation.Kinds.Contains(InstrumentationKind.StackOverflowProbing) || instrumentation.Kinds.Contains(InstrumentationKind.ModuleCancellation)))
								{
									if (methodSymbol.IsImplicitConstructor && (instrumentation.Kinds.Contains(InstrumentationKind.TestCoverage) || instrumentation.Kinds.Contains((InstrumentationKind)(-1))))
									{
										DefiniteAssignmentPass.Analyze(_compilation, methodSymbol, boundStatementList, instance.DiagnosticBag, out implicitlyInitializedFieldsOpt, requireOutParamsAssigned: false);
									}
									int index = 0;
									if (originalBodyNested & prependedDefaultValueTypeConstructorInitializer)
									{
										index = 1;
									}
									boundBlock = boundBlock.Update(boundBlock.Locals, boundBlock.LocalFunctions, boundBlock.HasUnsafeModifier, boundBlock.Instrumentation, boundBlock.Statements.Insert(index, boundStatementList));
									flag = false;
									boundStatementList = null;
								}
								else
								{
									DefiniteAssignmentPass.Analyze(_compilation, methodSymbol, boundStatementList, instance.DiagnosticBag, out implicitlyInitializedFieldsOpt, requireOutParamsAssigned: false);
									DiagnosticsPass.IssueDiagnostics(_compilation, boundStatementList, instance, methodSymbol);
								}
							}
						}
						importChain = (compilationState.CurrentImportChain = importChain ?? processedInitializers.FirstImportChain);
						if (boundBlock != null)
						{
							DiagnosticsPass.IssueDiagnostics(_compilation, boundBlock, instance, methodSymbol);
						}
						BoundBlock boundBlock2 = null;
						if (boundBlock != null)
						{
							boundBlock2 = FlowAnalysisPass.Rewrite(methodSymbol, boundBlock, compilationState, instance, hasTrailingExpression, originalBodyNested);
						}
						bool flag3 = _hasDeclarationErrors || instance.HasAnyErrors() || processedInitializers.HasErrors;
						SetGlobalErrorIfTrue(flag3);
						ReadOnlyBindingDiagnostic<AssemblySymbol> other = instance.ToReadOnly();
						if (sourceMemberMethodSymbol != null)
						{
							_compilation.RegisterPossibleUpcomingEventEnqueue();
							try
							{
								other = new ReadOnlyBindingDiagnostic<AssemblySymbol>(sourceMemberMethodSymbol.SetDiagnostics(other.Diagnostics, out var diagsWritten), other.Dependencies);
								if (diagsWritten && !methodSymbol.IsImplicitlyDeclared && _compilation.EventQueue != null)
								{
									SyntaxTreeSemanticModel semanticModelWithCachedBoundNodes = null;
									if (boundBlock != null)
									{
										CSharpSyntaxNode syntax = forSemanticModel.Syntax;
										if (syntax != null && _compilation.SemanticModelProvider is CachingSemanticModelProvider cachingSemanticModelProvider)
										{
											SyntaxNode syntax2 = boundBlock.Syntax;
											semanticModelWithCachedBoundNodes = (SyntaxTreeSemanticModel)cachingSemanticModelProvider.GetSemanticModel(syntax2.SyntaxTree, _compilation);
											semanticModelWithCachedBoundNodes.GetOrAddModel(syntax, (CSharpSyntaxNode rootSyntax) => MethodBodySemanticModel.Create(semanticModelWithCachedBoundNodes, methodSymbol, forSemanticModel));
										}
									}
									_compilation.EventQueue.TryEnqueue(new SymbolDeclaredCompilationEvent(_compilation, methodSymbol, semanticModelWithCachedBoundNodes));
								}
							}
							finally
							{
								_compilation.UnregisterPossibleUpcomingEventEnqueue();
							}
						}
						if (!((_moduleBeingBuiltOpt == null) | flag3))
						{
							bool flag4 = boundBlock2 != null;
							VariableSlotAllocator lazyVariableSlotAllocator = null;
							StateMachineTypeSymbol stateMachineTypeOpt = null;
							ArrayBuilder<EncLambdaInfo> instance3 = ArrayBuilder<EncLambdaInfo>.GetInstance();
							ArrayBuilder<LambdaRuntimeRudeEditInfo> instance4 = ArrayBuilder<LambdaRuntimeRudeEditInfo>.GetInstance();
							ArrayBuilder<EncClosureInfo> instance5 = ArrayBuilder<EncClosureInfo>.GetInstance();
							ArrayBuilder<StateMachineStateDebugInfo> instance6 = ArrayBuilder<StateMachineStateDebugInfo>.GetInstance();
							BoundStatement boundStatement = null;
							try
							{
								ImmutableArray<SourceSpan> codeCoverageSpans;
								if (flag4)
								{
									boundStatement = LowerBodyOrInitializer(methodSymbol, sourceExtensionImplementationMethodSymbol, methodOrdinal, boundBlock2, previousSubmissionFields, compilationState, instrumentation, GetDebugDocumentProvider(instrumentation), out codeCoverageSpans, instance, ref lazyVariableSlotAllocator, instance3, instance4, instance5, instance6, out stateMachineTypeOpt);
								}
								else
								{
									boundStatement = null;
									codeCoverageSpans = ImmutableArray<SourceSpan>.Empty;
								}
								flag3 = flag3 || (flag4 && boundStatement.HasErrors) || instance.HasAnyErrors();
								SetGlobalErrorIfTrue(flag3);
								CSharpSyntaxNode nonNullSyntaxNode = methodSymbol.GetNonNullSyntaxNode();
								if (!flag3 && (flag4 | flag))
								{
									ImmutableArray<BoundStatement> immutableArray;
									if (methodSymbol.IsScriptConstructor)
									{
										immutableArray = MethodBodySynthesizer.ConstructScriptConstructorBody(boundStatement, methodSymbol, previousSubmissionFields, _compilation);
									}
									else
									{
										immutableArray = ImmutableArray<BoundStatement>.Empty;
										if (methodSymbol is SynthesizedPrimaryConstructor synthesizedPrimaryConstructor)
										{
											IReadOnlyDictionary<ParameterSymbol, FieldSymbol> capturedParameters = synthesizedPrimaryConstructor.GetCapturedParameters();
											if (capturedParameters.Count != 0)
											{
												SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(methodSymbol, nonNullSyntaxNode, compilationState, instance);
												ArrayBuilder<BoundStatement> instance7 = ArrayBuilder<BoundStatement>.GetInstance(capturedParameters.Count);
												foreach (var (p, f) in capturedParameters.OrderBy((KeyValuePair<ParameterSymbol, FieldSymbol> pair) => pair.Key.Ordinal))
												{
													instance7.Add(syntheticBoundNodeFactory.Assignment(syntheticBoundNodeFactory.Field(syntheticBoundNodeFactory.This(), f), syntheticBoundNodeFactory.Parameter(p)));
												}
												immutableArray = immutableArray.Insert(0, syntheticBoundNodeFactory.HiddenSequencePoint(syntheticBoundNodeFactory.StatementList(instance7.ToImmutableAndFree())));
											}
										}
										if (boundStatementList != null)
										{
											BoundStatement boundStatement2 = (processedInitializers.LoweredInitializers = LowerBodyOrInitializer(methodSymbol, sourceExtensionImplementationMethodSymbol, methodOrdinal, boundStatementList, previousSubmissionFields, compilationState, instrumentation, GetDebugDocumentProvider(instrumentation), out var _, instance, ref lazyVariableSlotAllocator, instance3, instance4, instance5, instance6, out var _));
											flag3 = boundStatement2.HasAnyErrors || instance.HasAnyErrors();
											SetGlobalErrorIfTrue(flag3);
											if (flag3)
											{
												_diagnostics.AddRange(instance);
												return;
											}
											processedInitializers.LoweredInitializers = (BoundStatementList)boundStatement2;
										}
										if (flag)
										{
											if (processedInitializers.LoweredInitializers.Kind == BoundKind.StatementList)
											{
												BoundStatementList boundStatementList2 = (BoundStatementList)processedInitializers.LoweredInitializers;
												immutableArray = immutableArray.Concat(boundStatementList2.Statements);
											}
											else
											{
												immutableArray = immutableArray.Add(processedInitializers.LoweredInitializers);
											}
										}
										if (flag4)
										{
											immutableArray = immutableArray.Concat(boundStatement);
										}
										flag3 = instance.HasAnyErrors();
										SetGlobalErrorIfTrue(flag3);
										if (flag3)
										{
											_diagnostics.AddRange(instance);
											return;
										}
									}
									if (_emitMethodBodies && (!(methodSymbol is SynthesizedStaticConstructor synthesizedStaticConstructor) || synthesizedStaticConstructor.ShouldEmit(processedInitializers.BoundInitializers)))
									{
										BoundStatementList block = BoundStatementList.Synthesized(nonNullSyntaxNode, immutableArray);
										instance4.Sort((LambdaRuntimeRudeEditInfo x, LambdaRuntimeRudeEditInfo y) => x.LambdaId.CompareTo(y.LambdaId));
										MethodBody body = GenerateMethodBody(_moduleBeingBuiltOpt, sourceExtensionImplementationMethodSymbol ?? methodSymbol, methodOrdinal, block, instance3.ToImmutable(), instance4.ToImmutable(), instance5.ToImmutable(), instance6.ToImmutable(), stateMachineTypeOpt, lazyVariableSlotAllocator, instance, GetDebugDocumentProvider(instrumentation), importChain, _emittingPdb, codeCoverageSpans, null);
										_moduleBeingBuiltOpt.SetMethodBody(GetSymbolForEmittedBody(sourceExtensionImplementationMethodSymbol ?? methodSymbol), body);
									}
								}
								_diagnostics.AddRange(instance);
								return;
							}
							finally
							{
								instance3.Free();
								instance4.Free();
								instance5.Free();
								instance6.Free();
							}
						}
						_diagnostics.AddRange(other);
					}
					return;
				}
				finally
				{
					instance.Free();
					compilationState.CurrentImportChain = currentImportChain;
				}
			}
		}
		if ((object)sourceMemberMethodSymbol != null)
		{
			sourceMemberMethodSymbol.SetDiagnostics(ImmutableArray<Diagnostic>.Empty, out var diagsWritten2);
			if (diagsWritten2 && !methodSymbol.IsImplicitlyDeclared && _compilation.EventQueue != null)
			{
				_compilation.SymbolDeclaredEvent(methodSymbol);
			}
		}
	}

	private void EmitSkeletonMethodInExtension(MethodSymbol methodSymbol)
	{
		if (_emitMethodBodies)
		{
			ILBuilder iLBuilder = new ILBuilder(_moduleBeingBuiltOpt, new LocalSlotManager(null), _diagnostics.DiagnosticBag, OptimizationLevel.Release, areLocalsZeroed: false);
			CSharpSyntaxNode nonNullSyntaxNode = methodSymbol.GetNonNullSyntaxNode();
			MethodSymbol methodSymbol2 = (MethodSymbol)Binder.GetWellKnownTypeMember(_compilation, WellKnownMember.System_NotSupportedException__ctor, _diagnostics, null, nonNullSyntaxNode);
			if ((object)methodSymbol2 != null)
			{
				iLBuilder.EmitOpCode(ILOpCode.Newobj, 1);
				iLBuilder.EmitToken(_moduleBeingBuiltOpt.Translate(methodSymbol2, nonNullSyntaxNode, _diagnostics.DiagnosticBag), nonNullSyntaxNode, MetadataWriter.RawTokenEncoding.None);
			}
			else
			{
				iLBuilder.EmitOpCode(ILOpCode.Ldnull);
			}
			iLBuilder.EmitThrow(isRethrow: false);
			iLBuilder.Realize();
			_moduleBeingBuiltOpt.TestData?.SetMethodILBuilder(methodSymbol, iLBuilder);
			_moduleBeingBuiltOpt.SetMethodBody(methodSymbol, new MethodBody(iLBuilder.RealizedIL, 1, methodSymbol.GetCciAdapter(), new DebugId(-1, _moduleBeingBuiltOpt.CurrentGenerationOrdinal), ImmutableArray<ILocalDefinition>.Empty, SequencePointList.Empty, null, ImmutableArray<ExceptionHandlerRegion>.Empty, areLocalsZeroed: false, hasStackalloc: false, ImmutableArray<Microsoft.Cci.LocalScope>.Empty, hasDynamicLocalVariables: false, null, ImmutableArray<EncLambdaInfo>.Empty, ImmutableArray<LambdaRuntimeRudeEditInfo>.Empty, ImmutableArray<EncClosureInfo>.Empty, null, default(ImmutableArray<StateMachineHoistedLocalScope>), default(ImmutableArray<EncHoistedLocalInfo>), default(ImmutableArray<ITypeReference>), StateMachineStatesDebugInfo.Create(null, ImmutableArray<StateMachineStateDebugInfo>.Empty), null, ImmutableArray<SourceSpan>.Empty, isPrimaryConstructor: false));
		}
	}

	private static MethodSymbol GetSymbolForEmittedBody(MethodSymbol methodSymbol)
	{
		return methodSymbol.PartialDefinitionPart ?? methodSymbol;
	}

	internal static BoundStatement LowerBodyOrInitializer(MethodSymbol method, SourceExtensionImplementationMethodSymbol extensionImplementationMethod, int methodOrdinal, BoundStatement body, SynthesizedSubmissionFields previousSubmissionFields, TypeCompilationState compilationState, MethodInstrumentation instrumentation, DebugDocumentProvider debugDocumentProvider, out ImmutableArray<SourceSpan> codeCoverageSpans, BindingDiagnosticBag diagnostics, ref VariableSlotAllocator lazyVariableSlotAllocator, ArrayBuilder<EncLambdaInfo> lambdaDebugInfoBuilder, ArrayBuilder<LambdaRuntimeRudeEditInfo> lambdaRuntimeRudeEditsBuilder, ArrayBuilder<EncClosureInfo> closureDebugInfoBuilder, ArrayBuilder<StateMachineStateDebugInfo> stateMachineStateDebugInfoBuilder, out StateMachineTypeSymbol stateMachineTypeOpt)
	{
		stateMachineTypeOpt = null;
		if (body.HasErrors)
		{
			codeCoverageSpans = ImmutableArray<SourceSpan>.Empty;
			return body;
		}
		try
		{
			BoundStatement boundStatement = LocalRewriter.Rewrite(method.DeclaringCompilation, method, methodOrdinal, method.ContainingType, body, compilationState, previousSubmissionFields, allowOmissionOfConditionalCalls: true, instrumentation, debugDocumentProvider, diagnostics, out codeCoverageSpans, out var sawLambdas, out var sawLocalFunctions, out var sawAwaitInExceptionHandler);
			if (boundStatement.HasErrors)
			{
				return boundStatement;
			}
			if ((object)extensionImplementationMethod != null)
			{
				boundStatement = (BoundStatement)new ExtensionMethodBodyRewriter(method, extensionImplementationMethod).Visit(boundStatement);
				method = extensionImplementationMethod;
			}
			else
			{
				boundStatement = ExtensionMethodReferenceRewriter.Rewrite(boundStatement);
			}
			if (boundStatement.HasErrors)
			{
				return boundStatement;
			}
			if (sawAwaitInExceptionHandler)
			{
				boundStatement = AsyncExceptionHandlerRewriter.Rewrite(method, method.ContainingType, boundStatement, compilationState, diagnostics);
			}
			if (boundStatement.HasErrors)
			{
				return boundStatement;
			}
			if (lazyVariableSlotAllocator == null)
			{
				lazyVariableSlotAllocator = compilationState.ModuleBuilderOpt.TryCreateVariableSlotAllocator(method, method, diagnostics.DiagnosticBag);
			}
			BoundStatement boundStatement2 = boundStatement;
			if (sawLambdas | sawLocalFunctions)
			{
				boundStatement2 = ClosureConversion.Rewrite(boundStatement, method.ContainingType, method.ThisParameter, method, methodOrdinal, null, lambdaDebugInfoBuilder, lambdaRuntimeRudeEditsBuilder, closureDebugInfoBuilder, lazyVariableSlotAllocator, compilationState, diagnostics, null);
			}
			if (boundStatement2.HasErrors)
			{
				return boundStatement2;
			}
			BoundStatement boundStatement3 = IteratorRewriter.Rewrite(boundStatement2, method, methodOrdinal, stateMachineStateDebugInfoBuilder, lazyVariableSlotAllocator, compilationState, diagnostics, out var stateMachineType);
			if (boundStatement3.HasErrors)
			{
				return boundStatement3;
			}
			AsyncStateMachine stateMachineType2 = null;
			BoundStatement result = ((!compilationState.Compilation.IsRuntimeAsyncEnabledIn(method)) ? AsyncRewriter.Rewrite(boundStatement3, method, methodOrdinal, stateMachineStateDebugInfoBuilder, lazyVariableSlotAllocator, compilationState, diagnostics, out stateMachineType2) : RuntimeAsyncRewriter.Rewrite(boundStatement3, method, compilationState, diagnostics));
			stateMachineTypeOpt = (StateMachineTypeSymbol)(((object)stateMachineType) ?? ((object)stateMachineType2));
			return result;
		}
		catch (BoundTreeVisitor.CancelledByStackGuardException ex)
		{
			codeCoverageSpans = ImmutableArray<SourceSpan>.Empty;
			ex.AddAnError(diagnostics);
			return new BoundBadStatement(body.Syntax, ImmutableArray.Create((BoundNode)body), hasErrors: true);
		}
	}

	private static MethodBody GenerateMethodBody(PEModuleBuilder moduleBuilder, MethodSymbol method, int methodOrdinal, BoundStatement block, ImmutableArray<EncLambdaInfo> lambdaDebugInfo, ImmutableArray<LambdaRuntimeRudeEditInfo> orderedLambdaRuntimeRudeEdits, ImmutableArray<EncClosureInfo> closureDebugInfo, ImmutableArray<StateMachineStateDebugInfo> stateMachineStateDebugInfos, StateMachineTypeSymbol stateMachineTypeOpt, VariableSlotAllocator variableSlotAllocatorOpt, BindingDiagnosticBag diagnostics, DebugDocumentProvider debugDocumentProvider, ImportChain importChainOpt, bool emittingPdb, ImmutableArray<SourceSpan> codeCoverageSpans, SynthesizedEntryPointSymbol.AsyncForwardEntryPoint entryPointOpt)
	{
		CSharpCompilation compilation = moduleBuilder.Compilation;
		LocalSlotManager localSlotManager = new LocalSlotManager(variableSlotAllocatorOpt);
		OptimizationLevel optimizationLevel = compilation.Options.OptimizationLevel;
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: true, diagnostics.AccumulatesDependencies);
		ILBuilder iLBuilder = new ILBuilder(moduleBuilder, localSlotManager, instance.DiagnosticBag, optimizationLevel, method.AreLocalsZeroed);
		try
		{
			StateMachineMoveNextBodyDebugInfo stateMachineMoveNextDebugInfoOpt = null;
			CodeGenerator codeGenerator = new CodeGenerator(method, block, iLBuilder, moduleBuilder, instance, optimizationLevel, emittingPdb);
			if (instance.HasAnyErrors())
			{
				return null;
			}
			bool flag;
			MethodSymbol kickoffMethod;
			if (method is SynthesizedStateMachineMethod synthesizedStateMachineMethod && method.Name == "MoveNext")
			{
				kickoffMethod = synthesizedStateMachineMethod.StateMachineType.KickoffMethod;
				flag = kickoffMethod.IsAsync;
				kickoffMethod = kickoffMethod.PartialDefinitionPart ?? kickoffMethod;
			}
			else
			{
				kickoffMethod = null;
				flag = false;
			}
			bool hasStackalloc;
			if (flag)
			{
				codeGenerator.Generate(out var asyncCatchHandlerOffset, out var asyncYieldPoints, out var asyncResumePoints, out hasStackalloc);
				bool flag2 = entryPointOpt?.UserMain.Equals(kickoffMethod) ?? false;
				stateMachineMoveNextDebugInfoOpt = new AsyncMoveNextBodyDebugInfo(kickoffMethod.GetCciAdapter(), (kickoffMethod.ReturnsVoid | flag2) ? asyncCatchHandlerOffset : (-1), asyncYieldPoints, asyncResumePoints);
			}
			else
			{
				codeGenerator.Generate(out hasStackalloc);
				if ((object)kickoffMethod != null)
				{
					stateMachineMoveNextDebugInfoOpt = new IteratorMoveNextBodyDebugInfo(kickoffMethod.GetCciAdapter());
				}
			}
			ImmutableArray<StateMachineHoistedLocalScope> stateMachineHoistedLocalScopes = (((object)kickoffMethod != null) ? iLBuilder.GetHoistedLocalScopes() : default(ImmutableArray<StateMachineHoistedLocalScope>));
			Microsoft.Cci.IImportScope importScopeOpt = importChainOpt?.Translate(moduleBuilder, instance.DiagnosticBag);
			ImmutableArray<ILocalDefinition> locals = iLBuilder.LocalSlotManager.LocalsInOrder();
			if (locals.Length > 65534)
			{
				instance.Add(ErrorCode.ERR_TooManyLocals, method.GetFirstLocation());
			}
			if (instance.HasAnyErrors())
			{
				return null;
			}
			ImmutableArray<EncHoistedLocalInfo> hoistedVariableSlots = default(ImmutableArray<EncHoistedLocalInfo>);
			ImmutableArray<ITypeReference> awaiterSlots = default(ImmutableArray<ITypeReference>);
			if (optimizationLevel == OptimizationLevel.Debug && (object)stateMachineTypeOpt != null)
			{
				GetStateMachineSlotDebugInfo(moduleBuilder, moduleBuilder.GetSynthesizedFields(stateMachineTypeOpt), variableSlotAllocatorOpt, instance, out hoistedVariableSlots, out awaiterSlots);
			}
			MethodSymbol symbolForEmittedBody = GetSymbolForEmittedBody(method);
			moduleBuilder.TestData?.SetMethodILBuilder(symbolForEmittedBody, iLBuilder.GetSnapshot());
			return new MethodBody(iLBuilder.RealizedIL, iLBuilder.MaxStack, symbolForEmittedBody.GetCciAdapter(), variableSlotAllocatorOpt?.MethodId ?? new DebugId(methodOrdinal, moduleBuilder.CurrentGenerationOrdinal), locals, iLBuilder.RealizedSequencePoints, debugDocumentProvider, iLBuilder.RealizedExceptionHandlers, iLBuilder.AreLocalsZeroed, hasStackalloc, iLBuilder.GetAllScopes(), iLBuilder.HasDynamicLocal, importScopeOpt, lambdaDebugInfo, orderedLambdaRuntimeRudeEdits, closureDebugInfo, stateMachineTypeOpt?.Name, stateMachineHoistedLocalScopes, hoistedVariableSlots, awaiterSlots, StateMachineStatesDebugInfo.Create(variableSlotAllocatorOpt, stateMachineStateDebugInfos), stateMachineMoveNextDebugInfoOpt, codeCoverageSpans, method is SynthesizedPrimaryConstructor);
		}
		finally
		{
			iLBuilder.FreeBasicBlocks();
			diagnostics.AddRange(instance);
			instance.Free();
		}
	}

	private static void GetStateMachineSlotDebugInfo(PEModuleBuilder moduleBuilder, IEnumerable<IFieldDefinition> fieldDefs, VariableSlotAllocator variableSlotAllocatorOpt, BindingDiagnosticBag diagnostics, out ImmutableArray<EncHoistedLocalInfo> hoistedVariableSlots, out ImmutableArray<ITypeReference> awaiterSlots)
	{
		ArrayBuilder<EncHoistedLocalInfo> instance = ArrayBuilder<EncHoistedLocalInfo>.GetInstance();
		ArrayBuilder<ITypeReference> instance2 = ArrayBuilder<ITypeReference>.GetInstance();
		foreach (StateMachineFieldSymbol fieldDef in fieldDefs)
		{
			int slotIndex = fieldDef.SlotIndex;
			if (fieldDef.SlotDebugInfo.SynthesizedKind == SynthesizedLocalKind.AwaiterField)
			{
				while (slotIndex >= instance2.Count)
				{
					instance2.Add(null);
				}
				instance2[slotIndex] = moduleBuilder.EncTranslateLocalVariableType(fieldDef.Type, diagnostics.DiagnosticBag);
			}
			else if (!fieldDef.SlotDebugInfo.Id.IsNone)
			{
				while (slotIndex >= instance.Count)
				{
					instance.Add(new EncHoistedLocalInfo(_: true));
				}
				instance[slotIndex] = new EncHoistedLocalInfo(fieldDef.SlotDebugInfo, moduleBuilder.EncTranslateLocalVariableType(fieldDef.Type, diagnostics.DiagnosticBag));
			}
		}
		if (variableSlotAllocatorOpt != null)
		{
			int previousAwaiterSlotCount = variableSlotAllocatorOpt.PreviousAwaiterSlotCount;
			while (instance2.Count < previousAwaiterSlotCount)
			{
				instance2.Add(null);
			}
			int previousHoistedLocalSlotCount = variableSlotAllocatorOpt.PreviousHoistedLocalSlotCount;
			while (instance.Count < previousHoistedLocalSlotCount)
			{
				instance.Add(new EncHoistedLocalInfo(_: true));
			}
		}
		hoistedVariableSlots = instance.ToImmutableAndFree();
		awaiterSlots = instance2.ToImmutableAndFree();
	}

	internal static BoundBlock? BindSynthesizedMethodBody(MethodSymbol method, TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		ImportChain importChain;
		bool originalBodyNested;
		bool prependedDefaultValueTypeConstructorInitializer;
		MethodBodySemanticModel.InitialState forSemanticModel;
		return BindMethodBody(method, compilationState, diagnostics, includeInitializersInBody: false, null, reportNullableDiagnostics: true, out importChain, out originalBodyNested, out prependedDefaultValueTypeConstructorInitializer, out forSemanticModel);
	}

	private static BoundBlock? BindMethodBody(MethodSymbol method, TypeCompilationState compilationState, BindingDiagnosticBag diagnostics, bool includeInitializersInBody, BoundNode? initializersBody, bool reportNullableDiagnostics, out ImportChain? importChain, out bool originalBodyNested, out bool prependedDefaultValueTypeConstructorInitializer, out MethodBodySemanticModel.InitialState forSemanticModel)
	{
		originalBodyNested = false;
		prependedDefaultValueTypeConstructorInitializer = false;
		importChain = null;
		forSemanticModel = default(MethodBodySemanticModel.InitialState);
		NullableWalker.VariableState variableState = null;
		if (initializersBody == null)
		{
			initializersBody = GetSynthesizedEmptyBody(method);
		}
		NullableWalker.VariableState finalNullableState;
		BoundBlock boundBlock;
		if (method is SourceMemberMethodSymbol { SyntaxNode: var syntaxNode } sourceMemberMethodSymbol)
		{
			if (method.MethodKind == MethodKind.StaticConstructor && syntaxNode is ConstructorDeclarationSyntax { Initializer: not null } constructorDeclarationSyntax)
			{
				diagnostics.Add(ErrorCode.ERR_StaticConstructorWithExplicitConstructorCall, constructorDeclarationSyntax.Initializer.ThisOrBaseKeyword.GetLocation(), constructorDeclarationSyntax.Identifier.ValueText);
			}
			if (sourceMemberMethodSymbol.IsExtern)
			{
				return null;
			}
			Binder binder = sourceMemberMethodSymbol.TryGetBodyBinder();
			if (binder == null)
			{
				if (sourceMemberMethodSymbol is SourcePropertyAccessorSymbol { IsAutoPropertyAccessor: not false })
				{
					return MethodBodySynthesizer.ConstructAutoPropertyAccessorBody(sourceMemberMethodSymbol);
				}
				return null;
			}
			importChain = binder.ImportChain;
			BoundNode boundNode = binder.BindWithLambdaBindingCountDiagnostics(syntaxNode, null, diagnostics, (Binder bodyBinder, CSharpSyntaxNode syntax, object _, BindingDiagnosticBag diagnostics2) => bodyBinder.BindMethodBody(syntax, diagnostics2));
			BoundNode bodyOpt = boundNode;
			NullableWalker.SnapshotManager snapshotManager = null;
			ImmutableDictionary<Symbol, Symbol> remappedSymbols = null;
			CSharpCompilation compilation = binder.Compilation;
			variableState = getInitializerState(boundNode);
			if (reportNullableDiagnostics)
			{
				if (compilation.IsNullableAnalysisEnabledIn(method))
				{
					bool flag = compilation.LanguageVersion >= MessageID.IDS_FeatureNullableReferenceTypes.RequiredVersion();
					bodyOpt = NullableWalker.AnalyzeAndRewrite(compilation, method, boundNode, binder, variableState, flag ? diagnostics.DiagnosticBag : new DiagnosticBag(), createSnapshots: true, out snapshotManager, ref remappedSymbols);
				}
				else
				{
					NullableWalker.AnalyzeIfNeeded(compilation, method, boundNode, diagnostics.DiagnosticBag, useConstructorExitWarnings: true, variableState, getFinalNullableState: false, null, out finalNullableState);
				}
			}
			forSemanticModel = new MethodBodySemanticModel.InitialState(syntaxNode, bodyOpt, binder, snapshotManager, remappedSymbols);
			RefSafetyAnalysis.Analyze(compilation, method, boundNode, diagnostics);
			switch (boundNode.Kind)
			{
			case BoundKind.ConstructorMethodBody:
			{
				BoundConstructorMethodBody boundConstructorMethodBody = (BoundConstructorMethodBody)boundNode;
				boundBlock = boundConstructorMethodBody.BlockBody ?? boundConstructorMethodBody.ExpressionBody;
				if (boundConstructorMethodBody.Initializer is BoundExpressionStatement boundExpressionStatement)
				{
					ReportCtorInitializerCycles(method, boundExpressionStatement.Expression, compilationState, diagnostics);
					if (boundBlock == null)
					{
						boundBlock = new BoundBlock(boundConstructorMethodBody.Syntax, boundConstructorMethodBody.Locals, ImmutableArray.Create(boundConstructorMethodBody.Initializer));
					}
					else
					{
						boundBlock = new BoundBlock(boundConstructorMethodBody.Syntax, boundConstructorMethodBody.Locals, ImmutableArray.Create(boundConstructorMethodBody.Initializer, boundBlock));
						originalBodyNested = true;
						int num;
						if (boundExpressionStatement.Expression is BoundCall boundCall)
						{
							MethodSymbol method2 = boundCall.Method;
							num = (method2.IsDefaultValueTypeConstructor() ? 1 : 0);
						}
						else
						{
							num = 0;
						}
						prependedDefaultValueTypeConstructorInitializer = (byte)num != 0;
					}
				}
				return boundBlock;
			}
			case BoundKind.NonConstructorMethodBody:
			{
				BoundNonConstructorMethodBody boundNonConstructorMethodBody = (BoundNonConstructorMethodBody)boundNode;
				boundBlock = boundNonConstructorMethodBody.BlockBody ?? boundNonConstructorMethodBody.ExpressionBody;
				break;
			}
			case BoundKind.Block:
				boundBlock = (BoundBlock)boundNode;
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(boundNode.Kind);
			}
		}
		else if (method is SynthesizedInstanceConstructor synthesizedInstanceConstructor)
		{
			CSharpSyntaxNode nonNullSyntaxNode = synthesizedInstanceConstructor.GetNonNullSyntaxNode();
			SyntheticBoundNodeFactory factory = new SyntheticBoundNodeFactory(synthesizedInstanceConstructor, nonNullSyntaxNode, compilationState, diagnostics);
			ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
			synthesizedInstanceConstructor.GenerateMethodBodyStatements(factory, instance, diagnostics);
			boundBlock = BoundBlock.SynthesizedNoLocals(nonNullSyntaxNode, instance.ToImmutableAndFree());
			variableState = getInitializerState(boundBlock);
		}
		else
		{
			boundBlock = null;
			variableState = getInitializerState(null);
		}
		if (reportNullableDiagnostics && method.IsConstructor() && method.IsImplicitlyDeclared && variableState != null)
		{
			NullableWalker.AnalyzeIfNeeded(compilationState.Compilation, method, boundBlock ?? GetSynthesizedEmptyBody(method), diagnostics.DiagnosticBag, useConstructorExitWarnings: true, variableState, getFinalNullableState: false, null, out finalNullableState);
		}
		if (method.MethodKind == MethodKind.Destructor && boundBlock != null)
		{
			return MethodBodySynthesizer.ConstructDestructorBody(method, boundBlock);
		}
		BoundStatement boundStatement = BindImplicitConstructorInitializerIfAny(method, compilationState, diagnostics);
		ImmutableArray<BoundStatement> statements;
		if (boundStatement == null)
		{
			if (boundBlock != null)
			{
				return boundBlock;
			}
			statements = ImmutableArray<BoundStatement>.Empty;
		}
		else if (boundBlock == null)
		{
			statements = ImmutableArray.Create(boundStatement);
		}
		else
		{
			statements = ImmutableArray.Create(boundStatement, boundBlock);
			originalBodyNested = true;
		}
		return BoundBlock.SynthesizedNoLocals(method.GetNonNullSyntaxNode(), statements);
		NullableWalker.VariableState? getInitializerState(BoundNode? body)
		{
			if (reportNullableDiagnostics & includeInitializersInBody)
			{
				return NullableWalker.GetAfterInitializersState(compilationState.Compilation, method, initializersBody, body, diagnostics);
			}
			return null;
		}
	}

	private static BoundBlock GetSynthesizedEmptyBody(Symbol symbol)
	{
		return BoundBlock.SynthesizedNoLocals(symbol.GetNonNullSyntaxNode());
	}

	private static BoundStatement BindImplicitConstructorInitializerIfAny(MethodSymbol method, TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		if (method.MethodKind == MethodKind.Constructor && !method.IsExtern)
		{
			CSharpCompilation declaringCompilation = method.DeclaringCompilation;
			BoundExpression boundExpression = Binder.BindImplicitConstructorInitializer(method, diagnostics, declaringCompilation);
			if (boundExpression != null)
			{
				ReportCtorInitializerCycles(method, boundExpression, compilationState, diagnostics);
				return new BoundExpressionStatement(boundExpression.Syntax, boundExpression)
				{
					WasCompilerGenerated = method.IsImplicitlyDeclared
				};
			}
		}
		return null;
	}

	private static void ReportCtorInitializerCycles(MethodSymbol method, BoundExpression initializerInvocation, TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		if (initializerInvocation is BoundCall { HasAnyErrors: false } boundCall && boundCall.Method != method && TypeSymbol.Equals(boundCall.Method.ContainingType, method.ContainingType, TypeCompareKind.ConsiderEverything))
		{
			compilationState.ReportCtorInitializerCycles(method, boundCall.Method, boundCall.Syntax, diagnostics);
		}
	}

	private static DebugSourceDocument CreateDebugDocumentForFile(string normalizedPath)
	{
		return new DebugSourceDocument(normalizedPath, DebugSourceDocument.CorSymLanguageTypeCSharp);
	}

	private static bool PassesFilter(Predicate<Symbol> filterOpt, Symbol symbol)
	{
		return filterOpt?.Invoke(symbol) ?? true;
	}
}
