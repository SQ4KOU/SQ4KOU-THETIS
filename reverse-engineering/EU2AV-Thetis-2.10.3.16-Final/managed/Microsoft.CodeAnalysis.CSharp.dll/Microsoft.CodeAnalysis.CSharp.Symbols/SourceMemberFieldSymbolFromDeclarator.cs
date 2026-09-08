using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class SourceMemberFieldSymbolFromDeclarator : SourceMemberFieldSymbol
{
	private sealed class TypeAndRefKind
	{
		internal readonly RefKind RefKind;

		internal readonly TypeWithAnnotations Type;

		internal TypeAndRefKind(RefKind refKind, TypeWithAnnotations type)
		{
			RefKind = refKind;
			Type = type;
		}
	}

	private readonly bool _hasInitializer;

	private TypeAndRefKind _lazyTypeAndRefKind;

	private int _lazyFieldTypeInferred;

	protected sealed override TypeSyntax TypeSyntax => GetFieldDeclaration(VariableDeclaratorNode).Declaration.Type;

	protected sealed override SyntaxTokenList ModifiersTokenList => GetFieldDeclaration(VariableDeclaratorNode).Modifiers;

	public sealed override bool HasInitializer => _hasInitializer;

	protected VariableDeclaratorSyntax VariableDeclaratorNode => (VariableDeclaratorSyntax)base.SyntaxNode;

	public sealed override RefKind RefKind => GetTypeAndRefKind(ConsList<FieldSymbol>.Empty).RefKind;

	internal override bool HasPointerType => base.TypeWithAnnotations.DefaultType.IsPointerOrFunctionPointer();

	internal SourceMemberFieldSymbolFromDeclarator(SourceMemberContainerTypeSymbol containingType, VariableDeclaratorSyntax declarator, DeclarationModifiers modifiers, bool modifierErrors, BindingDiagnosticBag diagnostics)
		: base(containingType, modifiers, declarator.Identifier.ValueText, declarator.GetReference(), declarator.Identifier.Span)
	{
		_hasInitializer = declarator.Initializer != null;
		CheckAccessibility(diagnostics);
		if (!modifierErrors)
		{
			ReportModifiersDiagnostics(diagnostics);
		}
		if (!containingType.IsInterface)
		{
			return;
		}
		if (IsStatic)
		{
			Binder.CheckFeatureAvailability(declarator, MessageID.IDS_DefaultInterfaceImplementation, diagnostics, ErrorLocation);
			if (!ContainingAssembly.RuntimeSupportsDefaultInterfaceImplementation)
			{
				diagnostics.Add(ErrorCode.ERR_RuntimeDoesNotSupportDefaultInterfaceImplementation, ErrorLocation);
			}
		}
		else
		{
			diagnostics.Add(ErrorCode.ERR_InterfacesCantContainFields, ErrorLocation);
		}
	}

	private static BaseFieldDeclarationSyntax GetFieldDeclaration(CSharpSyntaxNode declarator)
	{
		return (BaseFieldDeclarationSyntax)declarator.Parent.Parent;
	}

	protected override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		if (containingType.AnyMemberHasAttributes)
		{
			return OneOrMany.Create(GetFieldDeclaration(base.SyntaxNode).AttributeLists);
		}
		return OneOrMany<SyntaxList<AttributeListSyntax>>.Empty;
	}

	internal sealed override TypeWithAnnotations GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		return GetTypeAndRefKind(fieldsBeingBound).Type;
	}

	private TypeAndRefKind GetTypeAndRefKind(ConsList<FieldSymbol> fieldsBeingBound)
	{
		if (_lazyTypeAndRefKind != null)
		{
			return _lazyTypeAndRefKind;
		}
		VariableDeclaratorSyntax variableDeclaratorNode = VariableDeclaratorNode;
		BaseFieldDeclarationSyntax fieldDeclaration = GetFieldDeclaration(variableDeclaratorNode);
		TypeSyntax type = fieldDeclaration.Declaration.Type;
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
		RefKind refKind = RefKind.None;
		if (type is ScopedTypeSyntax)
		{
			instance.Add(ErrorCode.ERR_BadMemberFlag, ErrorLocation, SyntaxFacts.GetText(SyntaxKind.ScopedKeyword));
		}
		BindingDiagnosticBag instance2 = BindingDiagnosticBag.GetInstance();
		Symbol associatedSymbol = AssociatedSymbol;
		TypeWithAnnotations pointedAtType;
		if ((object)associatedSymbol != null && associatedSymbol.Kind == SymbolKind.Event)
		{
			EventSymbol eventSymbol = (EventSymbol)associatedSymbol;
			if (eventSymbol.IsWindowsRuntimeEvent)
			{
				NamedTypeSymbol wellKnownType = DeclaringCompilation.GetWellKnownType(WellKnownType.System_Runtime_InteropServices_WindowsRuntime_EventRegistrationTokenTable_T);
				Binder.ReportUseSite(wellKnownType, instance2, ErrorLocation);
				pointedAtType = TypeWithAnnotations.Create(wellKnownType.Construct(ImmutableArray.Create(eventSymbol.TypeWithAnnotations)));
			}
			else
			{
				pointedAtType = eventSymbol.TypeWithAnnotations;
			}
		}
		else
		{
			Binder binder = declaringCompilation.GetBinderFactory(base.SyntaxTree).GetBinder(type);
			binder = binder.WithAdditionalFlagsAndContainingMemberOrLambda(BinderFlags.SuppressConstraintChecks, this);
			bool isScoped;
			if (!ContainingType.IsScriptClass)
			{
				TypeSyntax syntax = type.SkipScoped(out isScoped).SkipRefInField(out refKind);
				pointedAtType = binder.BindType(syntax, instance2);
				if (refKind != RefKind.None)
				{
					MessageID.IDS_FeatureRefFields.CheckFeatureAvailability(instance, declaringCompilation, type.SkipScoped(out isScoped).Location);
					if (!declaringCompilation.Assembly.RuntimeSupportsByRefFields)
					{
						instance.Add(ErrorCode.ERR_RuntimeDoesNotSupportRefFields, ErrorLocation);
					}
					if (!containingType.IsRefLikeType)
					{
						instance.Add(ErrorCode.ERR_RefFieldInNonRefStruct, ErrorLocation);
					}
					if (pointedAtType.Type.IsRefLikeOrAllowsRefLikeType())
					{
						instance.Add(ErrorCode.ERR_RefFieldCannotReferToRefStruct, type.SkipScoped(out isScoped).Location);
					}
				}
			}
			else
			{
				pointedAtType = binder.BindTypeOrVarKeyword(type.SkipScoped(out isScoped).SkipRefInField(out var _), instance, out var isVar);
				if (isVar)
				{
					if (IsConst)
					{
						instance2.Add(ErrorCode.ERR_ImplicitlyTypedVariableCannotBeConst, type.Location);
					}
					if (fieldsBeingBound.ContainsReference(this))
					{
						instance.Add(ErrorCode.ERR_RecursivelyTypedVariable, ErrorLocation, this);
						pointedAtType = default(TypeWithAnnotations);
					}
					else if (fieldDeclaration.Declaration.Variables.Count > 1)
					{
						instance2.Add(ErrorCode.ERR_ImplicitlyTypedVariableMultipleDeclarator, type.Location);
					}
					else if (IsConst && ContainingType.IsScriptClass)
					{
						pointedAtType = default(TypeWithAnnotations);
					}
					else
					{
						fieldsBeingBound = new ConsList<FieldSymbol>(this, fieldsBeingBound);
						EqualsValueClauseSyntax initializer = variableDeclaratorNode.Initializer;
						ImplicitlyTypedFieldBinder next = new ImplicitlyTypedFieldBinder(binder, fieldsBeingBound);
						BoundExpression boundExpression = new ExecutableCodeBinder(initializer, this, next).BindInferredVariableInitializer(instance, RefKind.None, initializer, variableDeclaratorNode);
						if (boundExpression != null)
						{
							if ((object)boundExpression.Type != null && !boundExpression.Type.IsErrorType())
							{
								pointedAtType = TypeWithAnnotations.Create(boundExpression.Type);
							}
							_lazyFieldTypeInferred = 1;
						}
					}
					if (!pointedAtType.HasType)
					{
						pointedAtType = TypeWithAnnotations.Create(binder.CreateErrorType("var"));
					}
				}
			}
			if (IsFixedSizeBuffer)
			{
				pointedAtType = TypeWithAnnotations.Create(new PointerTypeSymbol(pointedAtType));
				if (ContainingType.TypeKind != TypeKind.Struct)
				{
					instance.Add(ErrorCode.ERR_FixedNotInStruct, ErrorLocation);
				}
				if (refKind != RefKind.None)
				{
					instance.Add(ErrorCode.ERR_FixedFieldMustNotBeRef, ErrorLocation);
				}
				if (((PointerTypeSymbol)pointedAtType.Type).PointedAtType.FixedBufferElementSizeInBytes() == 0)
				{
					Location location = type.Location;
					instance.Add(ErrorCode.ERR_IllegalFixedType, location);
				}
				if (!binder.InUnsafeRegion)
				{
					instance2.Add(ErrorCode.ERR_UnsafeNeeded, variableDeclaratorNode.Location);
				}
			}
		}
		if (Interlocked.CompareExchange(ref _lazyTypeAndRefKind, new TypeAndRefKind(refKind, pointedAtType.WithModifiers(base.RequiredCustomModifiers)), null) == null)
		{
			TypeChecks(pointedAtType.Type, instance);
			AddDeclarationDiagnostics(instance);
			if (fieldDeclaration.Declaration.Variables[0] == variableDeclaratorNode)
			{
				AddDeclarationDiagnostics(instance2);
			}
			state.NotePartComplete(CompletionPart.Type);
		}
		instance.Free();
		instance2.Free();
		return _lazyTypeAndRefKind;
	}

	internal bool FieldTypeInferred(ConsList<FieldSymbol> fieldsBeingBound)
	{
		if (!ContainingType.IsScriptClass)
		{
			return false;
		}
		GetFieldType(fieldsBeingBound);
		if (_lazyFieldTypeInferred == 0)
		{
			return Volatile.Read(in _lazyFieldTypeInferred) != 0;
		}
		return true;
	}

	protected sealed override ConstantValue MakeConstantValue(HashSet<SourceFieldSymbolWithSyntaxReference> dependencies, bool earlyDecodingWellKnownAttributes, BindingDiagnosticBag diagnostics)
	{
		if (!IsConst || VariableDeclaratorNode.Initializer == null)
		{
			return null;
		}
		return ConstantValueUtils.EvaluateFieldConstant(this, VariableDeclaratorNode.Initializer, dependencies, earlyDecodingWellKnownAttributes, diagnostics);
	}

	public override bool IsDefinedInSourceTree(SyntaxTree tree, TextSpan? definedWithinSpan, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (base.SyntaxTree == tree)
		{
			if (!definedWithinSpan.HasValue)
			{
				return true;
			}
			BaseFieldDeclarationSyntax fieldDeclaration = GetFieldDeclaration(base.SyntaxNode);
			if (fieldDeclaration.SyntaxTree.HasCompilationUnitRoot)
			{
				return fieldDeclaration.Span.IntersectsWith(definedWithinSpan.Value);
			}
			return false;
		}
		return false;
	}

	internal override void AfterAddingTypeMembersChecks(ConversionsBase conversions, BindingDiagnosticBag diagnostics)
	{
		if (!IsFixedSizeBuffer)
		{
			base.Type.CheckAllConstraints(DeclaringCompilation, conversions, ErrorLocation, diagnostics);
		}
		base.AfterAddingTypeMembersChecks(conversions, diagnostics);
	}
}
