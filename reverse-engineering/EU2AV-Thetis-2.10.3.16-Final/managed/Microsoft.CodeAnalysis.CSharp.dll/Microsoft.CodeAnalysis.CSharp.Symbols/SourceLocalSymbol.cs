using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class SourceLocalSymbol : LocalSymbol
{
	private readonly struct LocalTypeInferenceInProgressKey(SourceLocalSymbol local, SyntaxNode reference) : IEquatable<LocalTypeInferenceInProgressKey>
	{
		public readonly SourceLocalSymbol Local = local;

		public readonly SyntaxNode Reference = reference;

		public bool Equals(LocalTypeInferenceInProgressKey other)
		{
			if ((object)Local == other.Local)
			{
				return Reference == other.Reference;
			}
			return false;
		}

		public override bool Equals(object? obj)
		{
			if (obj is LocalTypeInferenceInProgressKey)
			{
				return Equals((LocalTypeInferenceInProgressKey)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(RuntimeHelpers.GetHashCode(Local), Reference.GetHashCode());
		}
	}

	private sealed class LocalWithInitializer : SourceLocalSymbol
	{
		private readonly EqualsValueClauseSyntax _initializer;

		private readonly Binder _initializerBinder;

		private EvaluatedConstant _constantTuple;

		public LocalWithInitializer(Symbol containingSymbol, Binder scopeBinder, TypeSyntax typeSyntax, SyntaxToken identifierToken, EqualsValueClauseSyntax initializer, Binder initializerBinder, LocalDeclarationKind declarationKind, bool allowScoped)
			: base(containingSymbol, scopeBinder, allowRefKind: true, allowScoped, typeSyntax, identifierToken, declarationKind)
		{
			_initializer = initializer;
			_initializerBinder = initializerBinder;
			if (base.IsConst)
			{
				_initializerBinder = _initializerBinder.GetBinder(initializer) ?? new LocalInProgressBinder(_initializer, initializerBinder);
				recordConstInBinderChain();
			}
			void recordConstInBinderChain()
			{
				for (Binder binder = _initializerBinder; binder != null; binder = binder.Next)
				{
					if (binder is LocalInProgressBinder localInProgressBinder && localInProgressBinder.InitializerSyntax == _initializer)
					{
						localInProgressBinder.SetLocalSymbol(this);
						return;
					}
				}
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/SourceLocalSymbol.cs", 655);
			}
		}

		protected override TypeWithAnnotations InferTypeOfVarVariable()
		{
			return TypeWithAnnotations.Create(_initializerBinder.BindInferredVariableInitializer(BindingDiagnosticBag.Discarded, RefKind, _initializer, _initializer)?.Type);
		}

		private void MakeConstantTuple(LocalSymbol inProgress, BoundExpression boundInitValue)
		{
			if (base.IsConst && _constantTuple == null)
			{
				ConstantValue bad = Microsoft.CodeAnalysis.ConstantValue.Bad;
				BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
				TypeSymbol type = base.Type;
				if (boundInitValue == null)
				{
					boundInitValue = _initializerBinder.BindVariableOrAutoPropInitializerValue(_initializer, RefKind, type, instance);
				}
				bad = ConstantValueUtils.GetAndValidateConstantValue(boundInitValue, this, type, _initializer.Value, instance);
				Interlocked.CompareExchange(ref _constantTuple, new EvaluatedConstant(bad, instance.ToReadOnlyAndFree()), null);
			}
		}

		internal override ConstantValue GetConstantValue(SyntaxNode node, LocalSymbol inProgress, BindingDiagnosticBag diagnostics = null)
		{
			if (base.IsConst && inProgress == this)
			{
				diagnostics?.Add(ErrorCode.ERR_CircConstValue, node.GetLocation(), this);
				return Microsoft.CodeAnalysis.ConstantValue.Bad;
			}
			MakeConstantTuple(inProgress, null);
			if (_constantTuple != null)
			{
				return _constantTuple.Value;
			}
			return null;
		}

		internal override ReadOnlyBindingDiagnostic<AssemblySymbol> GetConstantValueDiagnostics(BoundExpression boundInitValue)
		{
			MakeConstantTuple(null, boundInitValue);
			if (_constantTuple != null)
			{
				return _constantTuple.Diagnostics;
			}
			return ReadOnlyBindingDiagnostic<AssemblySymbol>.Empty;
		}
	}

	private sealed class ForEachLocalSymbol : SourceLocalSymbol
	{
		private readonly ExpressionSyntax _collection;

		private ForEachLoopBinder ForEachLoopBinder => (ForEachLoopBinder)base.ScopeBinder;

		public ForEachLocalSymbol(Symbol containingSymbol, ForEachLoopBinder scopeBinder, TypeSyntax typeSyntax, SyntaxToken identifierToken, ExpressionSyntax collection, LocalDeclarationKind declarationKind)
			: base(containingSymbol, scopeBinder, allowRefKind: true, allowScoped: true, typeSyntax, identifierToken, declarationKind)
		{
			_collection = collection;
		}

		protected override TypeWithAnnotations InferTypeOfVarVariable()
		{
			return ForEachLoopBinder.InferCollectionElementType(BindingDiagnosticBag.Discarded, _collection);
		}
	}

	private sealed class DeconstructionLocalSymbol : SourceLocalSymbol
	{
		private readonly SyntaxNode _deconstruction;

		private readonly Binder _nodeBinder;

		public DeconstructionLocalSymbol(Symbol containingSymbol, Binder scopeBinder, Binder nodeBinder, TypeSyntax typeSyntax, SyntaxToken identifierToken, LocalDeclarationKind declarationKind, SyntaxNode deconstruction)
			: base(containingSymbol, scopeBinder, allowRefKind: false, allowScoped: true, typeSyntax, identifierToken, declarationKind)
		{
			_deconstruction = deconstruction;
			_nodeBinder = nodeBinder;
		}

		protected override TypeWithAnnotations InferTypeOfVarVariable()
		{
			switch (_deconstruction.Kind())
			{
			case SyntaxKind.SimpleAssignmentExpression:
			{
				AssignmentExpressionSyntax assignmentExpressionSyntax = (AssignmentExpressionSyntax)_deconstruction;
				DeclarationExpressionSyntax declaration = null;
				ExpressionSyntax expression = null;
				_nodeBinder.BindDeconstruction(assignmentExpressionSyntax, assignmentExpressionSyntax.Left, assignmentExpressionSyntax.Right, BindingDiagnosticBag.Discarded, ref declaration, ref expression);
				break;
			}
			case SyntaxKind.ForEachVariableStatement:
				_nodeBinder.BindForEachDeconstruction(BindingDiagnosticBag.Discarded, _nodeBinder);
				break;
			default:
				return TypeWithAnnotations.Create(_nodeBinder.CreateErrorType());
			}
			return _type?.Value ?? default(TypeWithAnnotations);
		}
	}

	private sealed class LocalSymbolWithEnclosingContext : SourceLocalSymbol
	{
		private readonly Binder _nodeBinder;

		private readonly SyntaxNode _nodeToBind;

		protected override ErrorCode ForbiddenDiagnostic => ErrorCode.ERR_ImplicitlyTypedVariableUsedInForbiddenZone;

		public LocalSymbolWithEnclosingContext(Symbol containingSymbol, Binder scopeBinder, Binder nodeBinder, TypeSyntax? typeSyntax, SyntaxToken identifierToken, LocalDeclarationKind declarationKind, SyntaxNode nodeToBind)
			: base(containingSymbol, scopeBinder, allowRefKind: false, allowScoped: true, typeSyntax, identifierToken, declarationKind)
		{
			_nodeBinder = nodeBinder;
			_nodeToBind = nodeToBind;
		}

		protected override TypeWithAnnotations InferTypeOfVarVariable()
		{
			switch (_nodeToBind.Kind())
			{
			case SyntaxKind.BaseConstructorInitializer:
			case SyntaxKind.ThisConstructorInitializer:
			{
				ConstructorInitializerSyntax initializer3 = (ConstructorInitializerSyntax)_nodeToBind;
				_nodeBinder.BindConstructorInitializer(initializer3, BindingDiagnosticBag.Discarded);
				break;
			}
			case SyntaxKind.PrimaryConstructorBaseType:
				_nodeBinder.BindConstructorInitializer((PrimaryConstructorBaseTypeSyntax)_nodeToBind, BindingDiagnosticBag.Discarded);
				break;
			case SyntaxKind.ArgumentList:
			{
				SyntaxNode parent = _nodeToBind.Parent;
				if (!(parent is ConstructorInitializerSyntax initializer))
				{
					if (!(parent is PrimaryConstructorBaseTypeSyntax initializer2))
					{
						throw ExceptionUtilities.UnexpectedValue(_nodeToBind.Parent);
					}
					_nodeBinder.BindConstructorInitializer(initializer2, BindingDiagnosticBag.Discarded);
				}
				else
				{
					_nodeBinder.BindConstructorInitializer(initializer, BindingDiagnosticBag.Discarded);
				}
				break;
			}
			case SyntaxKind.CasePatternSwitchLabel:
				_nodeBinder.BindPatternSwitchLabelForInference((CasePatternSwitchLabelSyntax)_nodeToBind, BindingDiagnosticBag.Discarded);
				break;
			case SyntaxKind.VariableDeclarator:
				_nodeBinder.BindDeclaratorArguments((VariableDeclaratorSyntax)_nodeToBind, BindingDiagnosticBag.Discarded);
				break;
			case SyntaxKind.SwitchExpressionArm:
			{
				SwitchExpressionArmSyntax node = (SwitchExpressionArmSyntax)_nodeToBind;
				((SwitchExpressionArmBinder)_nodeBinder).BindSwitchExpressionArm(node, BindingDiagnosticBag.Discarded);
				break;
			}
			case SyntaxKind.GotoCaseStatement:
				_nodeBinder.BindStatement((GotoStatementSyntax)_nodeToBind, BindingDiagnosticBag.Discarded);
				break;
			default:
				_nodeBinder.BindExpression((ExpressionSyntax)_nodeToBind, BindingDiagnosticBag.Discarded);
				break;
			}
			if (_type == null)
			{
				SetTypeWithAnnotations(TypeWithAnnotations.Create(DeclaringCompilation.ImplicitlyTypedVariableInferenceFailedType));
			}
			return _type?.Value ?? default(TypeWithAnnotations);
		}
	}

	private readonly Binder _scopeBinder;

	private readonly Symbol _containingSymbol;

	private readonly SyntaxToken _identifierToken;

	private readonly TypeSyntax _typeSyntax;

	private readonly RefKind _refKind;

	private readonly LocalDeclarationKind _declarationKind;

	private readonly ScopedKind _scope;

	private TypeWithAnnotations.Boxed? _type;

	[ThreadStatic]
	private static PooledHashSet<LocalTypeInferenceInProgressKey>? s_LocalTypeInferenceInProgress;

	private ConcurrentSet<SyntaxNode>? _forbiddenReferences;

	internal Binder ScopeBinder => _scopeBinder;

	internal override SyntaxNode ScopeDesignatorOpt => _scopeBinder.ScopeDesignator;

	internal sealed override ScopedKind Scope => _scope;

	internal Binder TypeSyntaxBinder => _scopeBinder;

	internal override bool IsImportedFromMetadata => false;

	internal override LocalDeclarationKind DeclarationKind => _declarationKind;

	internal override SynthesizedLocalKind SynthesizedKind => SynthesizedLocalKind.UserDefined;

	internal override bool IsPinned => false;

	internal sealed override bool IsKnownToReferToTempIfReferenceType => false;

	public override Symbol ContainingSymbol => _containingSymbol;

	public override string Name => _identifierToken.ValueText;

	internal override SyntaxToken IdentifierToken => _identifierToken;

	public override TypeWithAnnotations TypeWithAnnotations => GetTypeWithAnnotations(CSharpSyntaxTree.Dummy.GetRoot(), BindingDiagnosticBag.Discarded);

	protected virtual ErrorCode ForbiddenDiagnostic => ErrorCode.ERR_VariableUsedBeforeDeclaration;

	public bool IsVar
	{
		get
		{
			if (_typeSyntax == null)
			{
				return true;
			}
			TypeSyntax typeSyntax = _typeSyntax.SkipScoped(out var _).SkipRef();
			if (typeSyntax.IsVar)
			{
				TypeSyntaxBinder.BindTypeOrVarKeyword(typeSyntax, BindingDiagnosticBag.Discarded, out var isVar);
				return isVar;
			}
			return false;
		}
	}

	public override ImmutableArray<Location> Locations => ImmutableArray.Create(GetFirstLocation());

	internal override bool HasSourceLocation => true;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(_identifierToken.Parent.GetReference());

	internal override bool IsCompilerGenerated => false;

	public override RefKind RefKind => _refKind;

	private SourceLocalSymbol(Symbol containingSymbol, Binder scopeBinder, bool allowRefKind, bool allowScoped, TypeSyntax typeSyntax, SyntaxToken identifierToken, LocalDeclarationKind declarationKind)
	{
		_scopeBinder = scopeBinder;
		_containingSymbol = containingSymbol;
		_identifierToken = identifierToken;
		_typeSyntax = typeSyntax;
		typeSyntax = typeSyntax.SkipScoped(out var isScoped);
		isScoped &= allowScoped;
		if (allowRefKind)
		{
			typeSyntax.SkipRefInLocalOrReturn(null, out _refKind);
		}
		_scope = ((_refKind == RefKind.None) ? (isScoped ? ScopedKind.ScopedValue : ScopedKind.None) : (isScoped ? ScopedKind.ScopedRef : ScopedKind.None));
		_declarationKind = declarationKind;
	}

	internal override string GetDebuggerDisplay()
	{
		if (_type == null)
		{
			return $"{Kind} <var> ${Name}";
		}
		return base.GetDebuggerDisplay();
	}

	public static SourceLocalSymbol MakeForeachLocal(MethodSymbol containingMethod, ForEachLoopBinder binder, TypeSyntax typeSyntax, SyntaxToken identifierToken, ExpressionSyntax collection)
	{
		return new ForEachLocalSymbol(containingMethod, binder, typeSyntax, identifierToken, collection, LocalDeclarationKind.ForEachIterationVariable);
	}

	public static SourceLocalSymbol MakeDeconstructionLocal(Symbol containingSymbol, Binder scopeBinder, Binder nodeBinder, TypeSyntax closestTypeSyntax, SyntaxToken identifierToken, LocalDeclarationKind kind, SyntaxNode deconstruction)
	{
		if (!closestTypeSyntax.SkipScoped(out var _).SkipRef().IsVar)
		{
			return new SourceLocalSymbol(containingSymbol, scopeBinder, allowRefKind: false, allowScoped: true, closestTypeSyntax, identifierToken, kind);
		}
		return new DeconstructionLocalSymbol(containingSymbol, scopeBinder, nodeBinder, closestTypeSyntax, identifierToken, kind, deconstruction);
	}

	internal static LocalSymbol MakeLocalSymbolWithEnclosingContext(Symbol containingSymbol, Binder scopeBinder, Binder nodeBinder, TypeSyntax typeSyntax, SyntaxToken identifierToken, LocalDeclarationKind kind, SyntaxNode nodeToBind)
	{
		if ((typeSyntax != null && !typeSyntax.SkipScoped(out var _).SkipRef().IsVar) || kind == LocalDeclarationKind.DeclarationExpressionVariable)
		{
			return new SourceLocalSymbol(containingSymbol, scopeBinder, allowRefKind: false, allowScoped: true, typeSyntax, identifierToken, kind);
		}
		return new LocalSymbolWithEnclosingContext(containingSymbol, scopeBinder, nodeBinder, typeSyntax, identifierToken, kind, nodeToBind);
	}

	public static SourceLocalSymbol MakeLocal(Symbol containingSymbol, Binder scopeBinder, bool allowRefKind, bool allowScoped, TypeSyntax typeSyntax, SyntaxToken identifierToken, LocalDeclarationKind declarationKind, EqualsValueClauseSyntax initializer, Binder initializerBinderOpt = null)
	{
		if (initializer == null)
		{
			return new SourceLocalSymbol(containingSymbol, scopeBinder, allowRefKind, allowScoped, typeSyntax, identifierToken, declarationKind);
		}
		return new LocalWithInitializer(containingSymbol, scopeBinder, typeSyntax, identifierToken, initializer, initializerBinderOpt ?? scopeBinder, declarationKind, allowScoped);
	}

	internal override LocalSymbol WithSynthesizedLocalKindAndSyntax(SynthesizedLocalKind kind, SyntaxNode syntax)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/SourceLocalSymbol.cs", 275);
	}

	public override TypeWithAnnotations GetTypeWithAnnotations(SyntaxNode reference, BindingDiagnosticBag diagnostics)
	{
		ConcurrentSet<SyntaxNode>? forbiddenReferences = _forbiddenReferences;
		if (forbiddenReferences != null && forbiddenReferences.Contains(reference))
		{
			diagnostics.Add(ForbiddenDiagnostic, reference.Location, reference);
			return TypeWithAnnotations.Create(DeclaringCompilation.ImplicitlyTypedVariableUsedInForbiddenZoneType);
		}
		if (_type == null)
		{
			Binder typeSyntaxBinder = TypeSyntaxBinder;
			bool isVar;
			TypeWithAnnotations typeWithAnnotations;
			if (_typeSyntax == null)
			{
				isVar = true;
				typeWithAnnotations = default(TypeWithAnnotations);
			}
			else
			{
				typeWithAnnotations = typeSyntaxBinder.BindTypeOrVarKeyword(_typeSyntax.SkipScoped(out var _).SkipRef(), BindingDiagnosticBag.Discarded, out isVar);
			}
			if (isVar)
			{
				bool flag = false;
				PooledHashSet<LocalTypeInferenceInProgressKey> pooledHashSet = s_LocalTypeInferenceInProgress;
				if (pooledHashSet == null)
				{
					flag = true;
					pooledHashSet = (s_LocalTypeInferenceInProgress = PooledHashSet<LocalTypeInferenceInProgressKey>.GetInstance());
				}
				LocalTypeInferenceInProgressKey item = new LocalTypeInferenceInProgressKey(this, reference);
				if (!pooledHashSet.Add(item))
				{
					if (_forbiddenReferences == null)
					{
						Interlocked.CompareExchange(ref _forbiddenReferences, new ConcurrentSet<SyntaxNode>(), null);
					}
					_forbiddenReferences.Add(reference);
					diagnostics.Add(ForbiddenDiagnostic, reference.Location, reference);
					return TypeWithAnnotations.Create(DeclaringCompilation.ImplicitlyTypedVariableUsedInForbiddenZoneType);
				}
				TypeWithAnnotations typeWithAnnotations2;
				try
				{
					typeWithAnnotations2 = InferTypeOfVarVariable();
				}
				finally
				{
					pooledHashSet.Remove(item);
					if (flag)
					{
						s_LocalTypeInferenceInProgress = null;
						pooledHashSet.Free();
					}
				}
				ConcurrentSet<SyntaxNode>? forbiddenReferences2 = _forbiddenReferences;
				if (forbiddenReferences2 != null && forbiddenReferences2.Contains(reference))
				{
					diagnostics.Add(ForbiddenDiagnostic, reference.Location, reference);
					return TypeWithAnnotations.Create(DeclaringCompilation.ImplicitlyTypedVariableUsedInForbiddenZoneType);
				}
				typeWithAnnotations = ((!typeWithAnnotations2.HasType || typeWithAnnotations2.IsVoidType()) ? TypeWithAnnotations.Create(DeclaringCompilation.ImplicitlyTypedVariableInferenceFailedType) : typeWithAnnotations2);
			}
			SetTypeWithAnnotations(typeWithAnnotations);
			return _type?.Value ?? typeWithAnnotations;
		}
		return _type.Value;
	}

	protected virtual TypeWithAnnotations InferTypeOfVarVariable()
	{
		return _type?.Value ?? default(TypeWithAnnotations);
	}

	internal void SetTypeWithAnnotations(TypeWithAnnotations newType)
	{
		_ = _type?.Value;
		if (_type != null)
		{
			return;
		}
		if ((object)newType.Type == DeclaringCompilation.ImplicitlyTypedVariableInferenceFailedType)
		{
			PooledHashSet<LocalTypeInferenceInProgressKey>? pooledHashSet = s_LocalTypeInferenceInProgress;
			if (pooledHashSet != null && pooledHashSet.Any((LocalTypeInferenceInProgressKey key, SourceLocalSymbol @this) => (object)key.Local == @this, this))
			{
				return;
			}
		}
		Interlocked.CompareExchange(ref _type, new TypeWithAnnotations.Boxed(newType), null);
	}

	public override Location TryGetFirstLocation()
	{
		return _identifierToken.GetLocation();
	}

	internal sealed override SyntaxNode GetDeclaratorSyntax()
	{
		return _identifierToken.Parent;
	}

	internal override ConstantValue GetConstantValue(SyntaxNode node, LocalSymbol inProgress, BindingDiagnosticBag diagnostics)
	{
		return null;
	}

	internal override ReadOnlyBindingDiagnostic<AssemblySymbol> GetConstantValueDiagnostics(BoundExpression boundInitValue)
	{
		return ReadOnlyBindingDiagnostic<AssemblySymbol>.Empty;
	}

	public sealed override bool Equals(Symbol obj, TypeCompareKind compareKind)
	{
		if ((object)obj == this)
		{
			return true;
		}
		if (obj is UpdatedContainingSymbolAndNullableAnnotationLocal updatedContainingSymbolAndNullableAnnotationLocal)
		{
			return updatedContainingSymbolAndNullableAnnotationLocal.Equals(this, compareKind);
		}
		if (obj is SourceLocalSymbol sourceLocalSymbol && sourceLocalSymbol._identifierToken.Equals(_identifierToken))
		{
			return sourceLocalSymbol._containingSymbol.Equals(_containingSymbol, compareKind);
		}
		return false;
	}

	public sealed override int GetHashCode()
	{
		return Hash.Combine(_identifierToken.GetHashCode(), _containingSymbol.GetHashCode());
	}
}
