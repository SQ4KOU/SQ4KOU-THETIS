using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Hashing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceNamedTypeSymbol : SourceMemberContainerTypeSymbol, IAttributeTargetSymbol
{
	private class ExtensionInfo
	{
		public MethodSymbol? LazyExtensionMarker = ErrorMethodSymbol.UnknownMethod;

		public ParameterSymbol? LazyExtensionParameter;

		public ImmutableDictionary<MethodSymbol, MethodSymbol>? LazyImplementationMap;

		public string? LazyExtensionGroupingName;

		public string? LazyExtensionMarkerName;
	}

	private readonly TypeParameterInfo _typeParameterInfo;

	private CustomAttributesBag<CSharpAttributeData> _lazyCustomAttributesBag;

	private string _lazyDocComment;

	private string _lazyExpandedDocComment;

	private ThreeState _lazyIsExplicitDefinitionOfNoPiaLocalType;

	private Tuple<NamedTypeSymbol, ImmutableArray<NamedTypeSymbol>> _lazyDeclaredBases;

	private NamedTypeSymbol _lazyBaseType = ErrorTypeSymbol.UnknownResultType;

	private ImmutableArray<NamedTypeSymbol> _lazyInterfaces;

	private SynthesizedEnumValueFieldSymbol _lazyEnumValueField;

	private NamedTypeSymbol _lazyEnumUnderlyingType = ErrorTypeSymbol.UnknownResultType;

	private ExtensionInfo _lazyExtensionInfo;

	internal sealed override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotationsNoUseSiteDiagnostics => GetTypeParametersAsTypeArguments();

	public override ImmutableArray<TypeParameterSymbol> TypeParameters
	{
		get
		{
			if (_typeParameterInfo.LazyTypeParameters.IsDefault)
			{
				BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
				if (ImmutableInterlocked.InterlockedInitialize(ref _typeParameterInfo.LazyTypeParameters, MakeTypeParameters(instance)))
				{
					AddDeclarationDiagnostics(instance);
				}
				instance.Free();
			}
			return _typeParameterInfo.LazyTypeParameters;
		}
	}

	IAttributeTargetSymbol IAttributeTargetSymbol.AttributesOwner => this;

	AttributeLocation IAttributeTargetSymbol.DefaultAttributeLocation => AttributeLocation.Type;

	AttributeLocation IAttributeTargetSymbol.AllowedAttributeLocations
	{
		get
		{
			switch (TypeKind)
			{
			case TypeKind.Delegate:
				return AttributeLocation.Type | AttributeLocation.Return;
			case TypeKind.Enum:
			case TypeKind.Interface:
				return AttributeLocation.Type;
			case TypeKind.Class:
			case TypeKind.Struct:
				return (AttributeLocation)(4 | (base.HasPrimaryConstructor ? 8 : 0));
			default:
				return AttributeLocation.None;
			}
		}
	}

	internal override ObsoleteAttributeData ObsoleteAttributeData
	{
		get
		{
			CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = _lazyCustomAttributesBag;
			if (lazyCustomAttributesBag != null && lazyCustomAttributesBag.IsEarlyDecodedWellKnownAttributeDataComputed)
			{
				return ((TypeEarlyWellKnownAttributeData)lazyCustomAttributesBag.EarlyDecodedWellKnownAttributeData)?.ObsoleteAttributeData;
			}
			foreach (SingleTypeDeclaration declaration in declaration.Declarations)
			{
				if (declaration.HasAnyAttributes)
				{
					return ObsoleteAttributeData.Uninitialized;
				}
			}
			return null;
		}
	}

	internal override bool IsExplicitDefinitionOfNoPiaLocalType
	{
		get
		{
			if (_lazyIsExplicitDefinitionOfNoPiaLocalType == ThreeState.Unknown)
			{
				CheckPresenceOfTypeIdentifierAttribute();
				if (_lazyIsExplicitDefinitionOfNoPiaLocalType == ThreeState.Unknown)
				{
					_lazyIsExplicitDefinitionOfNoPiaLocalType = ThreeState.False;
				}
			}
			return _lazyIsExplicitDefinitionOfNoPiaLocalType == ThreeState.True;
		}
	}

	internal override bool IsComImport => GetEarlyDecodedWellKnownAttributeData()?.HasComImportAttribute ?? false;

	internal override NamedTypeSymbol ComImportCoClass => GetDecodedWellKnownAttributeData()?.ComImportCoClass;

	internal override bool HasSpecialName
	{
		get
		{
			if (IsExtension)
			{
				return true;
			}
			return GetDecodedWellKnownAttributeData()?.HasSpecialNameAttribute ?? false;
		}
	}

	internal override bool HasCodeAnalysisEmbeddedAttribute
	{
		get
		{
			TypeEarlyWellKnownAttributeData earlyDecodedWellKnownAttributeData = GetEarlyDecodedWellKnownAttributeData();
			if (earlyDecodedWellKnownAttributeData == null || !earlyDecodedWellKnownAttributeData.HasCodeAnalysisEmbeddedAttribute)
			{
				return this.IsMicrosoftCodeAnalysisEmbeddedAttribute();
			}
			return true;
		}
	}

	internal override bool HasCompilerLoweringPreserveAttribute => GetDecodedWellKnownAttributeData()?.HasCompilerLoweringPreserveAttribute ?? false;

	internal sealed override bool IsInterpolatedStringHandlerType => GetEarlyDecodedWellKnownAttributeData()?.HasInterpolatedStringHandlerAttribute ?? false;

	internal sealed override bool ShouldAddWinRTMembers => false;

	internal sealed override bool IsWindowsRuntimeImport => GetDecodedWellKnownAttributeData()?.HasWindowsRuntimeImportAttribute ?? false;

	public sealed override bool IsSerializable => GetDecodedWellKnownAttributeData()?.HasSerializableAttribute ?? false;

	public sealed override bool AreLocalsZeroed
	{
		get
		{
			TypeWellKnownAttributeData decodedWellKnownAttributeData = GetDecodedWellKnownAttributeData();
			if (decodedWellKnownAttributeData == null || !decodedWellKnownAttributeData.HasSkipLocalsInitAttribute)
			{
				return ContainingType?.AreLocalsZeroed ?? ContainingModule.AreLocalsZeroed;
			}
			return false;
		}
	}

	internal override bool IsDirectlyExcludedFromCodeCoverage => GetDecodedWellKnownAttributeData()?.HasExcludeFromCodeCoverageAttribute ?? false;

	internal sealed override TypeLayout Layout
	{
		get
		{
			TypeWellKnownAttributeData decodedWellKnownAttributeData = GetDecodedWellKnownAttributeData();
			if (decodedWellKnownAttributeData != null && decodedWellKnownAttributeData.HasStructLayoutAttribute)
			{
				return decodedWellKnownAttributeData.Layout;
			}
			if (TypeKind == TypeKind.Struct)
			{
				return new TypeLayout(LayoutKind.Sequential, (!HasInstanceFields()) ? 1 : 0, 0);
			}
			return default(TypeLayout);
		}
	}

	internal bool HasStructLayoutAttribute => GetDecodedWellKnownAttributeData()?.HasStructLayoutAttribute ?? false;

	internal override CharSet MarshallingCharSet
	{
		get
		{
			TypeWellKnownAttributeData decodedWellKnownAttributeData = GetDecodedWellKnownAttributeData();
			if (decodedWellKnownAttributeData == null || !decodedWellKnownAttributeData.HasStructLayoutAttribute)
			{
				return base.DefaultMarshallingCharSet;
			}
			return decodedWellKnownAttributeData.MarshallingCharSet;
		}
	}

	internal sealed override bool HasDeclarativeSecurity => GetDecodedWellKnownAttributeData()?.HasDeclarativeSecurity ?? false;

	internal bool HasSecurityCriticalAttributes => GetDecodedWellKnownAttributeData()?.HasSecurityCriticalAttributes ?? false;

	internal override NamedTypeSymbol NativeIntegerUnderlyingType => null;

	internal bool IsSimpleProgram => declaration.Declarations.Any((SingleTypeDeclaration d) => d.IsSimpleProgram);

	public override string MetadataName
	{
		get
		{
			if (IsExtension)
			{
				return ExtensionMarkerName;
			}
			return base.MetadataName;
		}
	}

	internal override bool MangleName
	{
		get
		{
			if (!IsExtension)
			{
				return base.MangleName;
			}
			return false;
		}
	}

	internal sealed override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics
	{
		get
		{
			if ((object)_lazyBaseType == ErrorTypeSymbol.UnknownResultType)
			{
				bool flag = (object)ContainingType != null;
				if (flag)
				{
					TypeKind typeKind = TypeKind;
					bool flag2 = ((typeKind == TypeKind.Delegate || typeKind == TypeKind.Enum || typeKind == TypeKind.Submission) ? true : false);
					flag = !flag2;
				}
				if (flag)
				{
					_ = ContainingType.BaseTypeNoUseSiteDiagnostics;
				}
				BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
				NamedTypeSymbol value = MakeAcyclicBaseType(instance);
				if ((object)Interlocked.CompareExchange(ref _lazyBaseType, value, ErrorTypeSymbol.UnknownResultType) == ErrorTypeSymbol.UnknownResultType)
				{
					AddDeclarationDiagnostics(instance);
				}
				instance.Free();
			}
			return _lazyBaseType;
		}
	}

	public override NamedTypeSymbol EnumUnderlyingType
	{
		get
		{
			if ((object)_lazyEnumUnderlyingType == ErrorTypeSymbol.UnknownResultType)
			{
				BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
				if ((object)Interlocked.CompareExchange(ref _lazyEnumUnderlyingType, GetEnumUnderlyingType(instance), ErrorTypeSymbol.UnknownResultType) == ErrorTypeSymbol.UnknownResultType)
				{
					AddDeclarationDiagnostics(instance);
					state.NotePartComplete(CompletionPart.EnumUnderlyingType);
				}
				instance.Free();
			}
			return _lazyEnumUnderlyingType;
		}
	}

	internal FieldSymbol EnumValueField
	{
		get
		{
			if (TypeKind != TypeKind.Enum)
			{
				return null;
			}
			if ((object)_lazyEnumValueField == null)
			{
				Interlocked.CompareExchange(ref _lazyEnumValueField, new SynthesizedEnumValueFieldSymbol(this), null);
			}
			return _lazyEnumValueField;
		}
	}

	internal sealed override ParameterSymbol? ExtensionParameter
	{
		get
		{
			if (!IsExtension)
			{
				return null;
			}
			MethodSymbol methodSymbol = TryGetOrCreateExtensionMarker();
			if (_lazyExtensionInfo.LazyExtensionParameter == null && (object)methodSymbol != null)
			{
				ImmutableArray<ParameterSymbol> parameters = methodSymbol.Parameters;
				if (parameters.Length >= 1)
				{
					ParameterSymbol originalParameter = parameters[0];
					Interlocked.CompareExchange(ref _lazyExtensionInfo.LazyExtensionParameter, new ReceiverParameterSymbol(this, originalParameter), null);
				}
			}
			return _lazyExtensionInfo.LazyExtensionParameter;
		}
	}

	internal override string? ExtensionGroupingName
	{
		get
		{
			if (!IsExtension)
			{
				return null;
			}
			if (_lazyExtensionInfo == null)
			{
				Interlocked.CompareExchange(ref _lazyExtensionInfo, new ExtensionInfo(), null);
			}
			if (_lazyExtensionInfo.LazyExtensionGroupingName == null)
			{
				_lazyExtensionInfo.LazyExtensionGroupingName = "<G>$" + RawNameToHashString(ComputeExtensionGroupingRawName());
			}
			return _lazyExtensionInfo.LazyExtensionGroupingName;
		}
	}

	internal override string? ExtensionMarkerName
	{
		get
		{
			if (!IsExtension)
			{
				return null;
			}
			if (_lazyExtensionInfo == null)
			{
				Interlocked.CompareExchange(ref _lazyExtensionInfo, new ExtensionInfo(), null);
			}
			if (_lazyExtensionInfo.LazyExtensionMarkerName == null)
			{
				_lazyExtensionInfo.LazyExtensionMarkerName = "<M>$" + RawNameToHashString(ComputeExtensionMarkerRawName());
			}
			return _lazyExtensionInfo.LazyExtensionMarkerName;
		}
	}

	protected override Location GetCorrespondingBaseListLocation(NamedTypeSymbol @base)
	{
		Location location = null;
		foreach (SyntaxReference syntaxReference in base.SyntaxReferences)
		{
			BaseListSyntax baseList = ((TypeDeclarationSyntax)syntaxReference.GetSyntax()).BaseList;
			if (baseList == null)
			{
				continue;
			}
			SeparatedSyntaxList<BaseTypeSyntax> types = baseList.Types;
			Binder binder = DeclaringCompilation.GetBinder(baseList);
			binder = binder.WithAdditionalFlagsAndContainingMemberOrLambda(BinderFlags.SuppressConstraintChecks, this);
			if ((object)location == null)
			{
				location = types[0].Type.GetLocation();
			}
			foreach (BaseTypeSyntax item in types)
			{
				TypeSyntax type = item.Type;
				if (TypeSymbol.Equals(binder.BindType(type, BindingDiagnosticBag.Discarded).Type, @base, TypeCompareKind.ConsiderEverything))
				{
					return type.GetLocation();
				}
			}
		}
		return location;
	}

	internal SourceNamedTypeSymbol(NamespaceOrTypeSymbol containingSymbol, MergedTypeDeclaration declaration, BindingDiagnosticBag diagnostics, TupleExtraData tupleData = null)
		: base(containingSymbol, declaration, diagnostics, tupleData)
	{
		DeclarationKind kind = declaration.Kind;
		if (kind - 1 > DeclarationKind.Enum)
		{
			_ = kind - 9;
			_ = 2;
		}
		if (containingSymbol.Kind == SymbolKind.NamedType)
		{
			_lazyIsExplicitDefinitionOfNoPiaLocalType = ThreeState.False;
		}
		_typeParameterInfo = ((declaration.Arity == 0) ? TypeParameterInfo.Empty : new TypeParameterInfo());
	}

	protected override NamedTypeSymbol WithTupleDataCore(TupleExtraData newData)
	{
		return new SourceNamedTypeSymbol(ContainingType, declaration, BindingDiagnosticBag.Discarded, newData);
	}

	private static SyntaxToken GetName(CSharpSyntaxNode node)
	{
		switch (node.Kind())
		{
		case SyntaxKind.EnumDeclaration:
			return ((EnumDeclarationSyntax)node).Identifier;
		case SyntaxKind.DelegateDeclaration:
			return ((DelegateDeclarationSyntax)node).Identifier;
		case SyntaxKind.ClassDeclaration:
		case SyntaxKind.StructDeclaration:
		case SyntaxKind.InterfaceDeclaration:
		case SyntaxKind.RecordDeclaration:
		case SyntaxKind.RecordStructDeclaration:
			return ((BaseTypeDeclarationSyntax)node).Identifier;
		default:
			return default(SyntaxToken);
		}
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return SourceDocumentationCommentUtils.GetAndCacheDocumentationComment(this, expandIncludes, ref expandIncludes ? ref _lazyExpandedDocComment : ref _lazyDocComment);
	}

	private ImmutableArray<TypeParameterSymbol> MakeTypeParameters(BindingDiagnosticBag diagnostics)
	{
		if (declaration.Arity == 0)
		{
			return ImmutableArray<TypeParameterSymbol>.Empty;
		}
		bool flag = false;
		string[] array = new string[declaration.Arity];
		string[] array2 = new string[declaration.Arity];
		List<List<TypeParameterBuilder>> list = new List<List<TypeParameterBuilder>>();
		foreach (SyntaxReference syntaxReference in base.SyntaxReferences)
		{
			CSharpSyntaxNode cSharpSyntaxNode = (CSharpSyntaxNode)syntaxReference.GetSyntax();
			SyntaxTree syntaxTree = syntaxReference.SyntaxTree;
			SyntaxKind syntaxKind = cSharpSyntaxNode.Kind();
			TypeParameterListSyntax typeParameterList;
			switch (syntaxKind)
			{
			case SyntaxKind.ClassDeclaration:
			case SyntaxKind.StructDeclaration:
			case SyntaxKind.InterfaceDeclaration:
			case SyntaxKind.RecordDeclaration:
			case SyntaxKind.RecordStructDeclaration:
			case SyntaxKind.ExtensionBlockDeclaration:
				typeParameterList = ((TypeDeclarationSyntax)cSharpSyntaxNode).TypeParameterList;
				break;
			case SyntaxKind.DelegateDeclaration:
				typeParameterList = ((DelegateDeclarationSyntax)cSharpSyntaxNode).TypeParameterList;
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(cSharpSyntaxNode.Kind());
			}
			MessageID.IDS_FeatureGenerics.CheckFeatureAvailability(diagnostics, typeParameterList.LessThanToken);
			bool flag2 = syntaxKind == SyntaxKind.InterfaceDeclaration || syntaxKind == SyntaxKind.DelegateDeclaration;
			List<TypeParameterBuilder> list2 = new List<TypeParameterBuilder>();
			list.Add(list2);
			int num = 0;
			foreach (TypeParameterSyntax parameter in typeParameterList.Parameters)
			{
				if (parameter.VarianceKeyword.Kind() != SyntaxKind.None)
				{
					if (!flag2)
					{
						diagnostics.Add(ErrorCode.ERR_IllegalVarianceSyntax, parameter.VarianceKeyword.GetLocation());
					}
					else
					{
						MessageID.IDS_FeatureTypeVariance.CheckFeatureAvailability(diagnostics, parameter.VarianceKeyword);
					}
				}
				string text = array[num];
				SourceLocation location = new SourceLocation(parameter.Identifier);
				string text2 = array2[num];
				SourceMemberContainerTypeSymbol.ReportReservedTypeName(parameter.Identifier.Text, DeclaringCompilation, diagnostics.DiagnosticBag, location);
				if (text == null)
				{
					text = (array[num] = parameter.Identifier.ValueText);
					text2 = (array2[num] = parameter.VarianceKeyword.ValueText);
					int num2 = 0;
					while (true)
					{
						if (num2 < num)
						{
							if (text == array[num2])
							{
								flag = true;
								diagnostics.Add(ErrorCode.ERR_DuplicateTypeParameter, location, text);
								break;
							}
							num2++;
							continue;
						}
						if ((object)ContainingType != null)
						{
							TypeParameterSymbol typeParameterSymbol = ContainingType.FindEnclosingTypeParameter(text);
							if ((object)typeParameterSymbol != null)
							{
								diagnostics.Add(ErrorCode.WRN_TypeParameterSameAsOuterTypeParameter, location, text, typeParameterSymbol.ContainingType);
							}
						}
						break;
					}
				}
				else if (!flag)
				{
					if (text2 != parameter.VarianceKeyword.ValueText)
					{
						flag = true;
						diagnostics.Add(ErrorCode.ERR_PartialWrongTypeParamsVariance, declaration.NameLocations.First(), this);
					}
					else if (text != parameter.Identifier.ValueText)
					{
						flag = true;
						diagnostics.Add(ErrorCode.ERR_PartialWrongTypeParams, declaration.NameLocations.First(), this);
					}
				}
				list2.Add(new TypeParameterBuilder(syntaxTree.GetReference(parameter), this, location));
				num++;
			}
		}
		return list.Transpose().Select((IList<TypeParameterBuilder> builders, int i) => builders[0].MakeSymbol(i, builders, diagnostics)).AsImmutable();
	}

	internal ImmutableArray<TypeWithAnnotations> GetTypeParameterConstraintTypes(int ordinal)
	{
		ImmutableArray<ImmutableArray<TypeWithAnnotations>> typeParameterConstraintTypes = GetTypeParameterConstraintTypes();
		if (typeParameterConstraintTypes.Length <= 0)
		{
			return ImmutableArray<TypeWithAnnotations>.Empty;
		}
		return typeParameterConstraintTypes[ordinal];
	}

	private ImmutableArray<ImmutableArray<TypeWithAnnotations>> GetTypeParameterConstraintTypes()
	{
		if (_typeParameterInfo.LazyTypeParameterConstraintTypes.IsDefault)
		{
			GetTypeParameterConstraintKinds();
			BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
			if (ImmutableInterlocked.InterlockedInitialize(ref _typeParameterInfo.LazyTypeParameterConstraintTypes, MakeTypeParameterConstraintTypes(instance)))
			{
				AddDeclarationDiagnostics(instance);
			}
			instance.Free();
		}
		return _typeParameterInfo.LazyTypeParameterConstraintTypes;
	}

	internal TypeParameterConstraintKind GetTypeParameterConstraintKind(int ordinal)
	{
		ImmutableArray<TypeParameterConstraintKind> typeParameterConstraintKinds = GetTypeParameterConstraintKinds();
		if (typeParameterConstraintKinds.Length <= 0)
		{
			return TypeParameterConstraintKind.None;
		}
		return typeParameterConstraintKinds[ordinal];
	}

	private ImmutableArray<TypeParameterConstraintKind> GetTypeParameterConstraintKinds()
	{
		if (_typeParameterInfo.LazyTypeParameterConstraintKinds.IsDefault)
		{
			ImmutableInterlocked.InterlockedInitialize(ref _typeParameterInfo.LazyTypeParameterConstraintKinds, MakeTypeParameterConstraintKinds());
		}
		return _typeParameterInfo.LazyTypeParameterConstraintKinds;
	}

	private ImmutableArray<ImmutableArray<TypeWithAnnotations>> MakeTypeParameterConstraintTypes(BindingDiagnosticBag diagnostics)
	{
		ImmutableArray<TypeParameterSymbol> typeParameters = TypeParameters;
		ImmutableArray<TypeParameterConstraintClause> immutableArray = ImmutableArray<TypeParameterConstraintClause>.Empty;
		if (typeParameters.Length > 0)
		{
			bool flag = SkipPartialDeclarationsWithoutConstraintClauses();
			ArrayBuilder<ImmutableArray<TypeParameterConstraintClause>> arrayBuilder = null;
			foreach (SingleTypeDeclaration declaration in declaration.Declarations)
			{
				SyntaxReference syntaxReference = declaration.SyntaxReference;
				SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses = GetConstraintClauses((CSharpSyntaxNode)syntaxReference.GetSyntax(), out var typeParameterList);
				if (!flag || constraintClauses.Count != 0)
				{
					BinderFactory binderFactory = DeclaringCompilation.GetBinderFactory(syntaxReference.SyntaxTree);
					ImmutableArray<TypeParameterConstraintClause> immutableArray2 = ((constraintClauses.Count != 0) ? binderFactory.GetBinder(constraintClauses[0]).WithContainingMemberOrLambda(this).WithAdditionalFlags(BinderFlags.SuppressConstraintChecks | BinderFlags.GenericConstraintsClause)
						.BindTypeParameterConstraintClauses(this, typeParameters, typeParameterList, constraintClauses, diagnostics, performOnlyCycleSafeValidation: false) : binderFactory.GetBinder(typeParameterList.Parameters[0]).GetDefaultTypeParameterConstraintClauses(typeParameterList));
					if (immutableArray.Length == 0)
					{
						immutableArray = immutableArray2;
					}
					else
					{
						(arrayBuilder ?? (arrayBuilder = ArrayBuilder<ImmutableArray<TypeParameterConstraintClause>>.GetInstance())).Add(immutableArray2);
					}
				}
			}
			immutableArray = MergeConstraintTypesForPartialDeclarations(immutableArray, arrayBuilder, diagnostics);
			if (immutableArray.All((TypeParameterConstraintClause clause) => clause.ConstraintTypes.IsEmpty))
			{
				immutableArray = ImmutableArray<TypeParameterConstraintClause>.Empty;
			}
			arrayBuilder?.Free();
		}
		return immutableArray.SelectAsArray((TypeParameterConstraintClause clause) => clause.ConstraintTypes);
	}

	private bool SkipPartialDeclarationsWithoutConstraintClauses()
	{
		foreach (SingleTypeDeclaration declaration in declaration.Declarations)
		{
			if (GetConstraintClauses((CSharpSyntaxNode)declaration.SyntaxReference.GetSyntax(), out var _).Count != 0)
			{
				return true;
			}
		}
		return false;
	}

	private ImmutableArray<TypeParameterConstraintKind> MakeTypeParameterConstraintKinds()
	{
		ImmutableArray<TypeParameterSymbol> typeParameters = TypeParameters;
		ImmutableArray<TypeParameterConstraintClause> immutableArray = ImmutableArray<TypeParameterConstraintClause>.Empty;
		if (typeParameters.Length > 0)
		{
			bool flag = SkipPartialDeclarationsWithoutConstraintClauses();
			ArrayBuilder<ImmutableArray<TypeParameterConstraintClause>> arrayBuilder = null;
			foreach (SingleTypeDeclaration declaration in declaration.Declarations)
			{
				SyntaxReference syntaxReference = declaration.SyntaxReference;
				SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses = GetConstraintClauses((CSharpSyntaxNode)syntaxReference.GetSyntax(), out var typeParameterList);
				if (!flag || constraintClauses.Count != 0)
				{
					BinderFactory binderFactory = DeclaringCompilation.GetBinderFactory(syntaxReference.SyntaxTree);
					ImmutableArray<TypeParameterConstraintClause> immutableArray2 = ((constraintClauses.Count != 0) ? binderFactory.GetBinder(constraintClauses[0]).WithContainingMemberOrLambda(this).WithAdditionalFlags(BinderFlags.SuppressConstraintChecks | BinderFlags.GenericConstraintsClause | BinderFlags.SuppressTypeArgumentBinding)
						.BindTypeParameterConstraintClauses(this, typeParameters, typeParameterList, constraintClauses, BindingDiagnosticBag.Discarded, performOnlyCycleSafeValidation: true) : binderFactory.GetBinder(typeParameterList.Parameters[0]).GetDefaultTypeParameterConstraintClauses(typeParameterList));
					if (immutableArray.Length == 0)
					{
						immutableArray = immutableArray2;
					}
					else
					{
						(arrayBuilder ?? (arrayBuilder = ArrayBuilder<ImmutableArray<TypeParameterConstraintClause>>.GetInstance())).Add(immutableArray2);
					}
				}
			}
			immutableArray = MergeConstraintKindsForPartialDeclarations(immutableArray, arrayBuilder);
			immutableArray = ConstraintsHelper.AdjustConstraintKindsBasedOnConstraintTypes(typeParameters, immutableArray);
			if (immutableArray.All((TypeParameterConstraintClause clause) => clause.Constraints == TypeParameterConstraintKind.None))
			{
				immutableArray = ImmutableArray<TypeParameterConstraintClause>.Empty;
			}
			arrayBuilder?.Free();
		}
		return immutableArray.SelectAsArray((TypeParameterConstraintClause clause) => clause.Constraints);
	}

	private static SyntaxList<TypeParameterConstraintClauseSyntax> GetConstraintClauses(CSharpSyntaxNode node, out TypeParameterListSyntax typeParameterList)
	{
		switch (node.Kind())
		{
		case SyntaxKind.ClassDeclaration:
		case SyntaxKind.StructDeclaration:
		case SyntaxKind.InterfaceDeclaration:
		case SyntaxKind.RecordDeclaration:
		case SyntaxKind.RecordStructDeclaration:
		case SyntaxKind.ExtensionBlockDeclaration:
		{
			TypeDeclarationSyntax typeDeclarationSyntax = (TypeDeclarationSyntax)node;
			typeParameterList = typeDeclarationSyntax.TypeParameterList;
			return typeDeclarationSyntax.ConstraintClauses;
		}
		case SyntaxKind.DelegateDeclaration:
		{
			DelegateDeclarationSyntax delegateDeclarationSyntax = (DelegateDeclarationSyntax)node;
			typeParameterList = delegateDeclarationSyntax.TypeParameterList;
			return delegateDeclarationSyntax.ConstraintClauses;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(node.Kind());
		}
	}

	private ImmutableArray<TypeParameterConstraintClause> MergeConstraintTypesForPartialDeclarations(ImmutableArray<TypeParameterConstraintClause> constraintClauses, ArrayBuilder<ImmutableArray<TypeParameterConstraintClause>> otherPartialClauses, BindingDiagnosticBag diagnostics)
	{
		if (otherPartialClauses == null)
		{
			return constraintClauses;
		}
		ArrayBuilder<TypeParameterConstraintClause> arrayBuilder = null;
		ImmutableArray<TypeParameterSymbol> typeParameters = TypeParameters;
		int length = typeParameters.Length;
		for (int i = 0; i < length; i++)
		{
			TypeParameterConstraintClause typeParameterConstraintClause = constraintClauses[i];
			ImmutableArray<TypeWithAnnotations> constraintTypes = typeParameterConstraintClause.ConstraintTypes;
			ArrayBuilder<TypeWithAnnotations> mergedConstraintTypes = null;
			SmallDictionary<TypeWithAnnotations, int> originalConstraintTypesMap = null;
			bool flag = (GetTypeParameterConstraintKind(i) & TypeParameterConstraintKind.PartialMismatch) != 0;
			foreach (ImmutableArray<TypeParameterConstraintClause> otherPartialClause in otherPartialClauses)
			{
				if (!mergeConstraints(constraintTypes, ref originalConstraintTypesMap, ref mergedConstraintTypes, otherPartialClause[i]))
				{
					flag = true;
				}
			}
			if (flag)
			{
				diagnostics.Add(ErrorCode.ERR_PartialWrongConstraints, GetFirstLocation(), this, typeParameters[i]);
			}
			if (mergedConstraintTypes != null)
			{
				if (arrayBuilder == null)
				{
					arrayBuilder = ArrayBuilder<TypeParameterConstraintClause>.GetInstance(constraintClauses.Length);
					arrayBuilder.AddRange(constraintClauses);
				}
				arrayBuilder[i] = TypeParameterConstraintClause.Create(typeParameterConstraintClause.Constraints, mergedConstraintTypes?.ToImmutableAndFree() ?? constraintTypes);
			}
		}
		if (arrayBuilder != null)
		{
			constraintClauses = arrayBuilder.ToImmutableAndFree();
		}
		return constraintClauses;
		static bool mergeConstraints(ImmutableArray<TypeWithAnnotations> originalConstraintTypes, ref SmallDictionary<TypeWithAnnotations, int> reference, ref ArrayBuilder<TypeWithAnnotations> reference2, TypeParameterConstraintClause clause)
		{
			bool result = true;
			if (originalConstraintTypes.Length == 0)
			{
				if (clause.ConstraintTypes.Length == 0)
				{
					return result;
				}
				return false;
			}
			if (clause.ConstraintTypes.Length == 0)
			{
				return false;
			}
			if (reference == null)
			{
				reference = toDictionary(originalConstraintTypes, TypeWithAnnotations.EqualsComparer.IgnoreNullableModifiersForReferenceTypesComparer);
			}
			SmallDictionary<TypeWithAnnotations, int> smallDictionary = toDictionary(clause.ConstraintTypes, reference.Comparer);
			foreach (int value2 in reference.Values)
			{
				TypeWithAnnotations key = reference2?[value2] ?? originalConstraintTypes[value2];
				if (!smallDictionary.TryGetValue(key, out var value))
				{
					result = false;
				}
				else
				{
					TypeWithAnnotations other = clause.ConstraintTypes[value];
					if (!key.Equals(other, TypeCompareKind.ObliviousNullableModifierMatchesAny))
					{
						result = false;
					}
					else if (!key.Equals(other, TypeCompareKind.ConsiderEverything))
					{
						if (reference2 == null)
						{
							reference2 = ArrayBuilder<TypeWithAnnotations>.GetInstance(originalConstraintTypes.Length);
							reference2.AddRange(originalConstraintTypes);
						}
						reference2[value2] = key.MergeEquivalentTypes(other, VarianceKind.None);
					}
				}
			}
			foreach (TypeWithAnnotations key2 in smallDictionary.Keys)
			{
				if (!reference.ContainsKey(key2))
				{
					result = false;
					break;
				}
			}
			return result;
		}
		static SmallDictionary<TypeWithAnnotations, int> toDictionary(ImmutableArray<TypeWithAnnotations> immutableArray, IEqualityComparer<TypeWithAnnotations> comparer)
		{
			SmallDictionary<TypeWithAnnotations, int> smallDictionary = new SmallDictionary<TypeWithAnnotations, int>(comparer);
			for (int num = immutableArray.Length - 1; num >= 0; num--)
			{
				smallDictionary[immutableArray[num]] = num;
			}
			return smallDictionary;
		}
	}

	private ImmutableArray<TypeParameterConstraintClause> MergeConstraintKindsForPartialDeclarations(ImmutableArray<TypeParameterConstraintClause> constraintClauses, ArrayBuilder<ImmutableArray<TypeParameterConstraintClause>> otherPartialClauses)
	{
		if (otherPartialClauses == null)
		{
			return constraintClauses;
		}
		ArrayBuilder<TypeParameterConstraintClause> arrayBuilder = null;
		int length = TypeParameters.Length;
		for (int i = 0; i < length; i++)
		{
			TypeParameterConstraintClause typeParameterConstraintClause = constraintClauses[i];
			TypeParameterConstraintKind mergedKind = typeParameterConstraintClause.Constraints;
			ImmutableArray<TypeWithAnnotations> constraintTypes = typeParameterConstraintClause.ConstraintTypes;
			foreach (ImmutableArray<TypeParameterConstraintClause> otherPartialClause in otherPartialClauses)
			{
				mergeConstraints(ref mergedKind, constraintTypes, otherPartialClause[i]);
			}
			if (typeParameterConstraintClause.Constraints != mergedKind)
			{
				if (arrayBuilder == null)
				{
					arrayBuilder = ArrayBuilder<TypeParameterConstraintClause>.GetInstance(constraintClauses.Length);
					arrayBuilder.AddRange(constraintClauses);
				}
				arrayBuilder[i] = TypeParameterConstraintClause.Create(mergedKind, constraintTypes);
			}
		}
		if (arrayBuilder != null)
		{
			constraintClauses = arrayBuilder.ToImmutableAndFree();
		}
		return constraintClauses;
		static void mergeConstraints(ref TypeParameterConstraintKind reference, ImmutableArray<TypeWithAnnotations> originalConstraintTypes, TypeParameterConstraintClause clause)
		{
			if ((reference & (TypeParameterConstraintKind.AllNonNullableKinds | TypeParameterConstraintKind.NotNull)) != (clause.Constraints & (TypeParameterConstraintKind.AllNonNullableKinds | TypeParameterConstraintKind.NotNull)))
			{
				reference |= TypeParameterConstraintKind.PartialMismatch;
			}
			if ((reference & TypeParameterConstraintKind.ReferenceType) != TypeParameterConstraintKind.None && (clause.Constraints & TypeParameterConstraintKind.ReferenceType) != TypeParameterConstraintKind.None)
			{
				TypeParameterConstraintKind typeParameterConstraintKind = reference & TypeParameterConstraintKind.AllReferenceTypeKinds;
				TypeParameterConstraintKind typeParameterConstraintKind2 = clause.Constraints & TypeParameterConstraintKind.AllReferenceTypeKinds;
				if (typeParameterConstraintKind != typeParameterConstraintKind2)
				{
					if (typeParameterConstraintKind == TypeParameterConstraintKind.ReferenceType)
					{
						reference = (reference & ~TypeParameterConstraintKind.AllReferenceTypeKinds) | typeParameterConstraintKind2;
					}
					else if (typeParameterConstraintKind2 != TypeParameterConstraintKind.ReferenceType)
					{
						reference |= TypeParameterConstraintKind.PartialMismatch;
					}
				}
			}
			if (originalConstraintTypes.Length == 0 && clause.ConstraintTypes.Length == 0 && ((reference | clause.Constraints) & ~(TypeParameterConstraintKind.Constructor | TypeParameterConstraintKind.ObliviousNullabilityIfReferenceType)) == 0 && (reference & TypeParameterConstraintKind.ObliviousNullabilityIfReferenceType) != TypeParameterConstraintKind.None && (clause.Constraints & TypeParameterConstraintKind.ObliviousNullabilityIfReferenceType) == 0)
			{
				reference &= ~TypeParameterConstraintKind.ObliviousNullabilityIfReferenceType;
			}
		}
	}

	internal ImmutableArray<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations(QuickAttributes? quickAttributes = null)
	{
		if (quickAttributes.HasValue)
		{
			foreach (SingleNamespaceDeclaration declaration in DeclaringCompilation.MergedRootDeclaration.Declarations)
			{
				if (declaration is RootSingleNamespaceDeclaration { GlobalAliasedQuickAttributes: var globalAliasedQuickAttributes } && (globalAliasedQuickAttributes & quickAttributes) != 0)
				{
					return base.declaration.GetAttributeDeclarations(null);
				}
			}
		}
		return base.declaration.GetAttributeDeclarations(quickAttributes);
	}

	private CustomAttributesBag<CSharpAttributeData> GetAttributesBag()
	{
		CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = _lazyCustomAttributesBag;
		if (lazyCustomAttributesBag != null && lazyCustomAttributesBag.IsSealed)
		{
			return lazyCustomAttributesBag;
		}
		if (LoadAndValidateAttributes(OneOrMany.Create(GetAttributeDeclarations()), ref _lazyCustomAttributesBag))
		{
			state.NotePartComplete(CompletionPart.Attributes);
		}
		return _lazyCustomAttributesBag;
	}

	public sealed override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return GetAttributesBag().Attributes;
	}

	private TypeWellKnownAttributeData GetDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (TypeWellKnownAttributeData)customAttributesBag.DecodedWellKnownAttributeData;
	}

	internal TypeEarlyWellKnownAttributeData? GetEarlyDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsEarlyDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (TypeEarlyWellKnownAttributeData)customAttributesBag.EarlyDecodedWellKnownAttributeData;
	}

	internal override (CSharpAttributeData?, BoundAttribute?) EarlyDecodeWellKnownAttribute(ref EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation> arguments)
	{
		bool generatedDiagnostics;
		CSharpAttributeData cSharpAttributeData;
		BoundAttribute item;
		if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.ComImportAttribute))
		{
			(cSharpAttributeData, item) = arguments.Binder.GetAttribute(arguments.AttributeSyntax, arguments.AttributeType, null, null, out generatedDiagnostics);
			if (!cSharpAttributeData.HasErrors)
			{
				arguments.GetOrCreateData<TypeEarlyWellKnownAttributeData>().HasComImportAttribute = true;
				if (!generatedDiagnostics)
				{
					return (cSharpAttributeData, item);
				}
			}
			return (null, null);
		}
		if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.CodeAnalysisEmbeddedAttribute))
		{
			(cSharpAttributeData, item) = arguments.Binder.GetAttribute(arguments.AttributeSyntax, arguments.AttributeType, null, null, out generatedDiagnostics);
			if (!cSharpAttributeData.HasErrors)
			{
				arguments.GetOrCreateData<TypeEarlyWellKnownAttributeData>().HasCodeAnalysisEmbeddedAttribute = true;
				if (!generatedDiagnostics)
				{
					return (cSharpAttributeData, item);
				}
			}
			return (null, null);
		}
		if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.ConditionalAttribute))
		{
			(cSharpAttributeData, item) = arguments.Binder.GetAttribute(arguments.AttributeSyntax, arguments.AttributeType, null, null, out generatedDiagnostics);
			if (!cSharpAttributeData.HasErrors)
			{
				string constructorArgument = cSharpAttributeData.GetConstructorArgument<string>(0, SpecialType.System_String);
				arguments.GetOrCreateData<TypeEarlyWellKnownAttributeData>().AddConditionalSymbol(constructorArgument);
				if (!generatedDiagnostics)
				{
					return (cSharpAttributeData, item);
				}
			}
			return (null, null);
		}
		if (Symbol.EarlyDecodeDeprecatedOrExperimentalOrObsoleteAttribute(ref arguments, out cSharpAttributeData, out item, out ObsoleteAttributeData obsoleteData))
		{
			if (obsoleteData != null)
			{
				arguments.GetOrCreateData<TypeEarlyWellKnownAttributeData>().ObsoleteAttributeData = obsoleteData;
			}
			return (cSharpAttributeData, item);
		}
		if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.AttributeUsageAttribute))
		{
			(cSharpAttributeData, item) = arguments.Binder.GetAttribute(arguments.AttributeSyntax, arguments.AttributeType, null, null, out generatedDiagnostics);
			if (!cSharpAttributeData.HasErrors)
			{
				AttributeUsageInfo attributeUsageInfo = DecodeAttributeUsageAttribute(cSharpAttributeData, arguments.AttributeSyntax, diagnose: false);
				if (!attributeUsageInfo.IsNull)
				{
					TypeEarlyWellKnownAttributeData orCreateData = arguments.GetOrCreateData<TypeEarlyWellKnownAttributeData>();
					if (orCreateData.AttributeUsageInfo.IsNull)
					{
						orCreateData.AttributeUsageInfo = attributeUsageInfo;
					}
					if (!generatedDiagnostics)
					{
						return (cSharpAttributeData, item);
					}
				}
			}
			return (null, null);
		}
		if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.InterpolatedStringHandlerAttribute))
		{
			(cSharpAttributeData, item) = arguments.Binder.GetAttribute(arguments.AttributeSyntax, arguments.AttributeType, null, null, out generatedDiagnostics);
			if (!cSharpAttributeData.HasErrors)
			{
				arguments.GetOrCreateData<TypeEarlyWellKnownAttributeData>().HasInterpolatedStringHandlerAttribute = true;
				if (!generatedDiagnostics)
				{
					return (cSharpAttributeData, item);
				}
			}
			return (null, null);
		}
		if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.InlineArrayAttribute))
		{
			(cSharpAttributeData, item) = arguments.Binder.GetAttribute(arguments.AttributeSyntax, arguments.AttributeType, null, null, out generatedDiagnostics);
			if (!cSharpAttributeData.HasErrors)
			{
				int constructorArgument2 = cSharpAttributeData.GetConstructorArgument<int>(0, SpecialType.System_Int32);
				arguments.GetOrCreateData<TypeEarlyWellKnownAttributeData>().InlineArrayLength = ((constructorArgument2 > 0) ? constructorArgument2 : (-1));
				if (!generatedDiagnostics)
				{
					return (cSharpAttributeData, item);
				}
			}
			return (null, null);
		}
		if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.CollectionBuilderAttribute))
		{
			(cSharpAttributeData, item) = arguments.Binder.GetAttribute(arguments.AttributeSyntax, arguments.AttributeType, null, null, out generatedDiagnostics);
			if (!cSharpAttributeData.HasErrors)
			{
				TypeSymbol builderType = cSharpAttributeData.CommonConstructorArguments[0].ValueInternal as TypeSymbol;
				string constructorArgument3 = cSharpAttributeData.GetConstructorArgument<string>(1, SpecialType.System_String);
				CollectionBuilderAttributeData collectionBuilder = new CollectionBuilderAttributeData(builderType, constructorArgument3);
				arguments.GetOrCreateData<TypeEarlyWellKnownAttributeData>().CollectionBuilder = collectionBuilder;
				if (!generatedDiagnostics)
				{
					return (cSharpAttributeData, item);
				}
			}
			return (null, null);
		}
		return base.EarlyDecodeWellKnownAttribute(ref arguments);
	}

	internal override AttributeUsageInfo GetAttributeUsageInfo()
	{
		TypeEarlyWellKnownAttributeData earlyDecodedWellKnownAttributeData = GetEarlyDecodedWellKnownAttributeData();
		if (earlyDecodedWellKnownAttributeData != null && !earlyDecodedWellKnownAttributeData.AttributeUsageInfo.IsNull)
		{
			return earlyDecodedWellKnownAttributeData.AttributeUsageInfo;
		}
		if ((object)BaseTypeNoUseSiteDiagnostics == null)
		{
			return AttributeUsageInfo.Default;
		}
		return BaseTypeNoUseSiteDiagnostics.GetAttributeUsageInfo();
	}

	protected sealed override void DecodeWellKnownAttributeImpl(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		BindingDiagnosticBag bindingDiagnosticBag = (BindingDiagnosticBag)arguments.Diagnostics;
		CSharpAttributeData attribute = arguments.Attribute;
		if (attribute.IsTargetAttribute(AttributeDescription.AttributeUsageAttribute))
		{
			DecodeAttributeUsageAttribute(attribute, arguments.AttributeSyntaxOpt, diagnose: true, bindingDiagnosticBag);
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.DefaultMemberAttribute))
		{
			arguments.GetOrCreateData<TypeWellKnownAttributeData>().HasDefaultMemberAttribute = true;
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.CoClassAttribute))
		{
			DecodeCoClassAttribute(ref arguments);
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.ConditionalAttribute))
		{
			ValidateConditionalAttribute(attribute, arguments.AttributeSyntaxOpt, bindingDiagnosticBag);
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.GuidAttribute))
		{
			arguments.GetOrCreateData<TypeWellKnownAttributeData>().GuidString = attribute.DecodeGuidAttribute(arguments.AttributeSyntaxOpt, bindingDiagnosticBag);
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.SpecialNameAttribute))
		{
			arguments.GetOrCreateData<TypeWellKnownAttributeData>().HasSpecialNameAttribute = true;
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.SerializableAttribute))
		{
			arguments.GetOrCreateData<TypeWellKnownAttributeData>().HasSerializableAttribute = true;
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.ExcludeFromCodeCoverageAttribute))
		{
			arguments.GetOrCreateData<TypeWellKnownAttributeData>().HasExcludeFromCodeCoverageAttribute = true;
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.StructLayoutAttribute))
		{
			AttributeData.DecodeStructLayoutAttribute<TypeWellKnownAttributeData, AttributeSyntax, CSharpAttributeData, AttributeLocation>(ref arguments, base.DefaultMarshallingCharSet, 0, MessageProvider.Instance);
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.SuppressUnmanagedCodeSecurityAttribute))
		{
			arguments.GetOrCreateData<TypeWellKnownAttributeData>().HasSuppressUnmanagedCodeSecurityAttribute = true;
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.ClassInterfaceAttribute))
		{
			attribute.DecodeClassInterfaceAttribute(arguments.AttributeSyntaxOpt, bindingDiagnosticBag);
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.InterfaceTypeAttribute))
		{
			attribute.DecodeInterfaceTypeAttribute(arguments.AttributeSyntaxOpt, bindingDiagnosticBag);
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.WindowsRuntimeImportAttribute))
		{
			arguments.GetOrCreateData<TypeWellKnownAttributeData>().HasWindowsRuntimeImportAttribute = true;
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.RequiredAttributeAttribute))
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_CantUseRequiredAttribute, arguments.AttributeSyntaxOpt.Name.Location);
		}
		else
		{
			if (ReportExplicitUseOfReservedAttributes(in arguments, ReservedAttributes.DynamicAttribute | ReservedAttributes.IsReadOnlyAttribute | ReservedAttributes.IsUnmanagedAttribute | ReservedAttributes.IsByRefLikeAttribute | ReservedAttributes.TupleElementNamesAttribute | ReservedAttributes.NullableAttribute | ReservedAttributes.NullableContextAttribute | ReservedAttributes.NativeIntegerAttribute | ReservedAttributes.CaseSensitiveExtensionAttribute | ReservedAttributes.RequiredMemberAttribute | ReservedAttributes.RequiresLocationAttribute | ReservedAttributes.ExtensionMarkerAttribute))
			{
				return;
			}
			if (attribute.IsTargetAttribute(AttributeDescription.SecurityCriticalAttribute) || attribute.IsTargetAttribute(AttributeDescription.SecuritySafeCriticalAttribute))
			{
				arguments.GetOrCreateData<TypeWellKnownAttributeData>().HasSecurityCriticalAttributes = true;
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.SkipLocalsInitAttribute))
			{
				CSharpAttributeData.DecodeSkipLocalsInitAttribute<TypeWellKnownAttributeData>(DeclaringCompilation, ref arguments);
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.CollectionBuilderAttribute))
			{
				TypeSymbol typeSymbol = attribute.CommonConstructorArguments[0].ValueInternal as TypeSymbol;
				if (!IsValidCollectionBuilderType(typeSymbol))
				{
					bindingDiagnosticBag.Add(ErrorCode.ERR_CollectionBuilderAttributeInvalidType, arguments.AttributeSyntaxOpt.Name.Location);
				}
				bindingDiagnosticBag.AddDependencies(typeSymbol);
				if (string.IsNullOrEmpty(attribute.CommonConstructorArguments[1].DecodeValue<string>(SpecialType.System_String)))
				{
					bindingDiagnosticBag.Add(ErrorCode.ERR_CollectionBuilderAttributeInvalidMethodName, arguments.AttributeSyntaxOpt.Name.Location);
				}
			}
			else if (_lazyIsExplicitDefinitionOfNoPiaLocalType == ThreeState.Unknown && attribute.IsTargetAttribute(AttributeDescription.TypeIdentifierAttribute))
			{
				_lazyIsExplicitDefinitionOfNoPiaLocalType = ThreeState.True;
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.InlineArrayAttribute))
			{
				if (attribute.CommonConstructorArguments[0].DecodeValue<int>(SpecialType.System_Int32) <= 0)
				{
					bindingDiagnosticBag.Add(ErrorCode.ERR_InvalidInlineArrayLength, attribute.GetAttributeArgumentLocation(0));
				}
				if (TypeKind != TypeKind.Struct)
				{
					bindingDiagnosticBag.Add(ErrorCode.ERR_AttributeOnBadSymbolType, arguments.AttributeSyntaxOpt.Name.Location, arguments.AttributeSyntaxOpt.GetErrorDisplayName(), "struct");
				}
				else if (IsRecordStruct)
				{
					bindingDiagnosticBag.Add(ErrorCode.ERR_InlineArrayAttributeOnRecord, arguments.AttributeSyntaxOpt.Name.Location);
				}
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.CompilerLoweringPreserveAttribute))
			{
				arguments.GetOrCreateData<TypeWellKnownAttributeData>().HasCompilerLoweringPreserveAttribute = true;
			}
			else
			{
				CSharpCompilation declaringCompilation = DeclaringCompilation;
				if (attribute.IsSecurityAttribute(declaringCompilation))
				{
					attribute.DecodeSecurityAttribute<TypeWellKnownAttributeData>(this, declaringCompilation, ref arguments);
				}
			}
		}
	}

	internal static bool IsValidCollectionBuilderType([NotNullWhen(true)] TypeSymbol? builderType)
	{
		if (builderType is NamedTypeSymbol namedTypeSymbol)
		{
			TypeKind typeKind = builderType.TypeKind;
			if ((typeKind == TypeKind.Class || typeKind == TypeKind.Struct) && !namedTypeSymbol.IsGenericType)
			{
				return true;
			}
		}
		return false;
	}

	private void CheckPresenceOfTypeIdentifierAttribute()
	{
		CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = _lazyCustomAttributesBag;
		if (lazyCustomAttributesBag != null && lazyCustomAttributesBag.IsDecodedWellKnownAttributeDataComputed)
		{
			return;
		}
		foreach (SyntaxList<AttributeListSyntax> attributeDeclaration in GetAttributeDeclarations(QuickAttributes.TypeIdentifier))
		{
			_ = attributeDeclaration.Node.SyntaxTree;
			QuickAttributeChecker quickAttributeChecker = DeclaringCompilation.GetBinderFactory(attributeDeclaration.Node.SyntaxTree).GetBinder(attributeDeclaration.Node).QuickAttributeChecker;
			foreach (AttributeListSyntax item in attributeDeclaration)
			{
				foreach (AttributeSyntax attribute in item.Attributes)
				{
					if (quickAttributeChecker.IsPossibleMatch(attribute, QuickAttributes.TypeIdentifier))
					{
						GetAttributes();
						return;
					}
				}
			}
		}
	}

	private AttributeUsageInfo DecodeAttributeUsageAttribute(CSharpAttributeData attribute, AttributeSyntax node, bool diagnose, BindingDiagnosticBag diagnosticsOpt = null)
	{
		if (!DeclaringCompilation.IsAttributeType(this))
		{
			if (diagnose)
			{
				diagnosticsOpt.Add(ErrorCode.ERR_AttributeUsageOnNonAttributeClass, node.Name.Location, node.GetErrorDisplayName());
			}
			return AttributeUsageInfo.Null;
		}
		AttributeUsageInfo result = attribute.DecodeAttributeUsageAttribute();
		if (!result.HasValidAttributeTargets)
		{
			if (diagnose)
			{
				diagnosticsOpt.Add(ErrorCode.ERR_InvalidAttributeArgument, attribute.GetAttributeArgumentLocation(0), node.GetErrorDisplayName());
			}
			return AttributeUsageInfo.Null;
		}
		return result;
	}

	private void DecodeCoClassAttribute(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		CSharpAttributeData attribute = arguments.Attribute;
		if (this.IsInterfaceType() && (!arguments.HasDecodedData || (object)((TypeWellKnownAttributeData)arguments.DecodedData).ComImportCoClass == null) && attribute.CommonConstructorArguments[0].ValueInternal is NamedTypeSymbol { TypeKind: TypeKind.Class } namedTypeSymbol)
		{
			arguments.GetOrCreateData<TypeWellKnownAttributeData>().ComImportCoClass = namedTypeSymbol;
		}
	}

	internal sealed override bool HasCollectionBuilderAttribute(out TypeSymbol? builderType, out string? methodName)
	{
		CollectionBuilderAttributeData collectionBuilderAttributeData = GetEarlyDecodedWellKnownAttributeData()?.CollectionBuilder;
		if (collectionBuilderAttributeData == null)
		{
			builderType = null;
			methodName = null;
			return false;
		}
		builderType = collectionBuilderAttributeData.BuilderType;
		methodName = collectionBuilderAttributeData.MethodName;
		return true;
	}

	private void ValidateConditionalAttribute(CSharpAttributeData attribute, AttributeSyntax node, BindingDiagnosticBag diagnostics)
	{
		if (!DeclaringCompilation.IsAttributeType(this))
		{
			diagnostics.Add(ErrorCode.ERR_ConditionalOnNonAttributeClass, node.Location, node.GetErrorDisplayName());
			return;
		}
		string constructorArgument = attribute.GetConstructorArgument<string>(0, SpecialType.System_String);
		if (constructorArgument == null || !SyntaxFacts.IsValidIdentifier(constructorArgument))
		{
			diagnostics.Add(ErrorCode.ERR_BadArgumentToAttribute, attribute.GetAttributeArgumentLocation(0), node.GetErrorDisplayName());
		}
	}

	internal override bool GetGuidString(out string guidString)
	{
		guidString = GetDecodedWellKnownAttributeData()?.GuidString;
		return guidString != null;
	}

	private bool HasInstanceFields()
	{
		foreach (FieldSymbol item in GetFieldsToEmit())
		{
			if (!item.IsStatic)
			{
				return true;
			}
		}
		return false;
	}

	internal sealed override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		CustomAttributesBag<CSharpAttributeData> attributesBag = GetAttributesBag();
		TypeWellKnownAttributeData typeWellKnownAttributeData = (TypeWellKnownAttributeData)attributesBag.DecodedWellKnownAttributeData;
		if (typeWellKnownAttributeData != null)
		{
			SecurityWellKnownAttributeData securityInformation = typeWellKnownAttributeData.SecurityInformation;
			if (securityInformation != null)
			{
				return securityInformation.GetSecurityAttributes(attributesBag.Attributes);
			}
		}
		return null;
	}

	internal override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return GetEarlyDecodedWellKnownAttributeData()?.ConditionalSymbols ?? ImmutableArray<string>.Empty;
	}

	internal override void PostDecodeWellKnownAttributes(ImmutableArray<CSharpAttributeData> boundAttributes, ImmutableArray<AttributeSyntax> allAttributeSyntaxNodes, BindingDiagnosticBag diagnostics, AttributeLocation symbolPart, WellKnownAttributeData decodedData)
	{
		TypeWellKnownAttributeData typeWellKnownAttributeData = (TypeWellKnownAttributeData)decodedData;
		if (IsComImport)
		{
			if (typeWellKnownAttributeData == null || typeWellKnownAttributeData.GuidString == null)
			{
				int index = boundAttributes.IndexOfAttribute(AttributeDescription.ComImportAttribute);
				diagnostics.Add(ErrorCode.ERR_ComImportWithoutUuidAttribute, allAttributeSyntaxNodes[index].Name.Location);
			}
			if (TypeKind == TypeKind.Class)
			{
				NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
				if ((object)baseTypeNoUseSiteDiagnostics != null && baseTypeNoUseSiteDiagnostics.SpecialType != SpecialType.System_Object)
				{
					diagnostics.Add(ErrorCode.ERR_ComImportWithBase, GetFirstLocation(), Name);
				}
				ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> staticInitializers = base.StaticInitializers;
				if (!staticInitializers.IsDefaultOrEmpty)
				{
					foreach (ImmutableArray<FieldOrPropertyInitializer> item in staticInitializers)
					{
						foreach (FieldOrPropertyInitializer item2 in item)
						{
							if (!item2.FieldOpt.IsMetadataConstant)
							{
								diagnostics.Add(ErrorCode.ERR_ComImportWithInitializers, item2.Syntax.GetLocation(), Name);
							}
						}
					}
				}
				staticInitializers = base.InstanceInitializers;
				if (!staticInitializers.IsDefaultOrEmpty)
				{
					foreach (ImmutableArray<FieldOrPropertyInitializer> item3 in staticInitializers)
					{
						foreach (FieldOrPropertyInitializer item4 in item3)
						{
							diagnostics.Add(ErrorCode.ERR_ComImportWithInitializers, item4.Syntax.GetLocation(), Name);
						}
					}
				}
			}
		}
		else if ((object)ComImportCoClass != null)
		{
			int index2 = boundAttributes.IndexOfAttribute(AttributeDescription.CoClassAttribute);
			diagnostics.Add(ErrorCode.WRN_CoClassWithoutComImport, allAttributeSyntaxNodes[index2].Location, Name);
		}
		if (typeWellKnownAttributeData != null && typeWellKnownAttributeData.HasDefaultMemberAttribute && base.Indexers.Any())
		{
			int index3 = boundAttributes.IndexOfAttribute(AttributeDescription.DefaultMemberAttribute);
			diagnostics.Add(ErrorCode.ERR_DefaultMemberOnIndexedType, allAttributeSyntaxNodes[index3].Name.Location);
		}
		base.PostDecodeWellKnownAttributes(boundAttributes, allAttributeSyntaxNodes, diagnostics, symbolPart, decodedData);
	}

	internal override bool HasInlineArrayAttribute(out int length)
	{
		int? num = GetEarlyDecodedWellKnownAttributeData()?.InlineArrayLength;
		if (num.HasValue)
		{
			int valueOrDefault = num.GetValueOrDefault();
			if (valueOrDefault > 0)
			{
				length = valueOrDefault;
				return true;
			}
		}
		length = 0;
		return false;
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		if (base.ContainsExtensionMethods)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_ExtensionAttribute__ctor));
		}
		if (IsRefLikeType)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeIsByRefLikeAttribute(this));
			ObsoleteAttributeData obsoleteAttributeData = ObsoleteAttributeData;
			if (!this.IsRestrictedType(ignoreSpanLikeTypes: true))
			{
				if (obsoleteAttributeData == null)
				{
					Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_ObsoleteAttribute__ctor, ImmutableArray.Create(new TypedConstant(declaringCompilation.GetSpecialType(SpecialType.System_String), TypedConstantKind.Primitive, "Types with embedded references are not supported in this version of your compiler."), new TypedConstant(declaringCompilation.GetSpecialType(SpecialType.System_Boolean), TypedConstantKind.Primitive, true)), default(ImmutableArray<KeyValuePair<WellKnownMember, TypedConstant>>), isOptionalUse: true));
				}
				Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerFeatureRequiredAttribute__ctor, ImmutableArray.Create(new TypedConstant(declaringCompilation.GetSpecialType(SpecialType.System_String), TypedConstantKind.Primitive, "RefStructs")), default(ImmutableArray<KeyValuePair<WellKnownMember, TypedConstant>>), isOptionalUse: true));
			}
		}
		if (IsReadOnly)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeIsReadOnlyAttribute(this));
		}
		if (base.Indexers.Any())
		{
			string metadataName = base.Indexers.First().MetadataName;
			TypedConstant item = new TypedConstant(declaringCompilation.GetSpecialType(SpecialType.System_String), TypedConstantKind.Primitive, metadataName);
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Reflection_DefaultMemberAttribute__ctor, ImmutableArray.Create(item)));
		}
		if (declaration.Declarations.All((SingleTypeDeclaration d) => d.IsSimpleProgram))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, DeclaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor));
		}
		if (HasDeclaredRequiredMembers)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_RequiredMemberAttribute__ctor));
		}
		SymbolChanges? encSymbolChanges = moduleBuilder.EncSymbolChanges;
		if (encSymbolChanges != null && encSymbolChanges.IsReplaced(this))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_MetadataUpdateOriginalTypeAttribute__ctor, ImmutableArray.Create(new TypedConstant(declaringCompilation.GetWellKnownType(WellKnownType.System_Type), TypedConstantKind.Type, this)), default(ImmutableArray<KeyValuePair<WellKnownMember, TypedConstant>>), isOptionalUse: true));
		}
		bool flag = this.IsMicrosoftCodeAnalysisEmbeddedAttribute();
		if (flag)
		{
			TypeEarlyWellKnownAttributeData earlyDecodedWellKnownAttributeData = GetEarlyDecodedWellKnownAttributeData();
			bool flag2 = ((earlyDecodedWellKnownAttributeData == null || !earlyDecodedWellKnownAttributeData.HasCodeAnalysisEmbeddedAttribute) ? true : false);
			flag = flag2;
		}
		if (flag)
		{
			MethodSymbol methodSymbol = base.InstanceConstructors.FirstOrDefault((MethodSymbol c) => c.ParameterCount == 0);
			if ((object)methodSymbol != null)
			{
				Symbol.AddSynthesizedAttribute(ref attributes, SynthesizedAttributeData.Create(DeclaringCompilation, methodSymbol, ImmutableArray<TypedConstant>.Empty, ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty));
			}
		}
	}

	internal override NamedTypeSymbol AsNativeInteger()
	{
		if (ContainingAssembly.RuntimeSupportsNumericIntPtr)
		{
			return this;
		}
		return ContainingAssembly.GetNativeIntegerType(this);
	}

	internal override bool Equals(TypeSymbol t2, TypeCompareKind comparison)
	{
		if (!(t2 is NativeIntegerTypeSymbol nativeIntegerTypeSymbol))
		{
			return base.Equals(t2, comparison);
		}
		return nativeIntegerTypeSymbol.Equals(this, comparison);
	}

	protected override void AfterMembersCompletedChecks(BindingDiagnosticBag diagnostics)
	{
		base.AfterMembersCompletedChecks(diagnostics);
		if (base.ObsoleteKind == ObsoleteAttributeKind.None && !GetMembers().All((Symbol m) => !(m is MethodSymbol { MethodKind: MethodKind.Constructor } methodSymbol) || m.ObsoleteKind != ObsoleteAttributeKind.None || !methodSymbol.ShouldCheckRequiredMembers()))
		{
			foreach (Symbol member in GetMembers())
			{
				if (member.IsRequired() && member.ObsoleteKind != ObsoleteAttributeKind.None)
				{
					diagnostics.Add(ErrorCode.WRN_ObsoleteMembersShouldNotBeRequired, member.GetFirstLocation(), member);
				}
			}
		}
		PropertySymbol propertySymbol = base.Indexers.FirstOrDefault();
		if ((object)propertySymbol != null)
		{
			Binder.GetWellKnownTypeMember(DeclaringCompilation, WellKnownMember.System_Reflection_DefaultMemberAttribute__ctor, diagnostics, propertySymbol.TryGetFirstLocation() ?? GetFirstLocation());
		}
		if (TypeKind == TypeKind.Struct && !IsRecordStruct && HasInlineArrayAttribute(out var _))
		{
			if (Layout.Kind == LayoutKind.Explicit)
			{
				diagnostics.Add(ErrorCode.ERR_InvalidInlineArrayLayout, GetFirstLocation());
			}
			FieldSymbol fieldSymbol = TryGetPossiblyUnsupportedByLanguageInlineArrayElementField();
			if ((object)fieldSymbol != null)
			{
				bool flag = false;
				if (fieldSymbol.IsRequired || fieldSymbol.IsReadOnly || fieldSymbol.IsVolatile || fieldSymbol.IsFixedSizeBuffer)
				{
					diagnostics.Add(ErrorCode.ERR_InlineArrayUnsupportedElementFieldModifier, fieldSymbol.TryGetFirstLocation() ?? GetFirstLocation());
					flag = true;
				}
				NamedTypeSymbol namedTypeSymbol = null;
				NamedTypeSymbol namedTypeSymbol2 = null;
				foreach (PropertySymbol indexer in base.Indexers)
				{
					ImmutableArray<ParameterSymbol> parameters = indexer.Parameters;
					if (parameters.Length != 1)
					{
						continue;
					}
					ParameterSymbol parameterSymbol = parameters[0];
					if ((object)parameterSymbol != null)
					{
						TypeSymbol type = parameterSymbol.Type;
						if ((object)type != null && (type.SpecialType == SpecialType.System_Int32 || type.Equals(namedTypeSymbol ?? (namedTypeSymbol = DeclaringCompilation.GetWellKnownType(WellKnownType.System_Index)), TypeCompareKind.AllIgnoreOptions) || type.Equals(namedTypeSymbol2 ?? (namedTypeSymbol2 = DeclaringCompilation.GetWellKnownType(WellKnownType.System_Range)), TypeCompareKind.AllIgnoreOptions)))
						{
							diagnostics.Add(ErrorCode.WRN_InlineArrayIndexerNotUsed, indexer.TryGetFirstLocation() ?? GetFirstLocation());
						}
					}
				}
				foreach (MethodSymbol item in GetMembers("Slice").OfType<MethodSymbol>())
				{
					if (Binder.MethodHasValidSliceSignature(item))
					{
						diagnostics.Add(ErrorCode.WRN_InlineArraySliceNotUsed, item.TryGetFirstLocation() ?? GetFirstLocation());
						break;
					}
				}
				NamedTypeSymbol namedTypeSymbol3 = null;
				NamedTypeSymbol namedTypeSymbol4 = null;
				TypeWithAnnotations typeWithAnnotations = fieldSymbol.TypeWithAnnotations;
				bool flag2 = TypeSymbol.IsInlineArrayElementFieldSupported(fieldSymbol);
				if (flag2)
				{
					foreach (SourceUserDefinedConversionSymbol item2 in GetMembers().OfType<SourceUserDefinedConversionSymbol>())
					{
						TypeSymbol returnType = item2.ReturnType;
						TypeSymbol originalDefinition = returnType.OriginalDefinition;
						if (item2.ParameterCount == 1 && item2.Parameters[0].Type.Equals(this, TypeCompareKind.AllIgnoreOptions) && (originalDefinition.Equals(namedTypeSymbol3 ?? (namedTypeSymbol3 = DeclaringCompilation.GetWellKnownType(WellKnownType.System_Span_T)), TypeCompareKind.AllIgnoreOptions) || originalDefinition.Equals(namedTypeSymbol4 ?? (namedTypeSymbol4 = DeclaringCompilation.GetWellKnownType(WellKnownType.System_ReadOnlySpan_T)), TypeCompareKind.AllIgnoreOptions)) && ConversionsBase.HasIdentityConversion(((NamedTypeSymbol)originalDefinition).Construct(ImmutableArray.Create(typeWithAnnotations)), returnType))
						{
							diagnostics.Add(ErrorCode.WRN_InlineArrayConversionOperatorNotUsed, item2.TryGetFirstLocation() ?? GetFirstLocation());
						}
					}
				}
				if (!flag)
				{
					if (!flag2 || typeWithAnnotations.Type.IsPointerOrFunctionPointer() || typeWithAnnotations.IsRestrictedType(ignoreSpanLikeTypes: true))
					{
						diagnostics.Add(ErrorCode.WRN_InlineArrayNotSupportedByLanguage, fieldSymbol.TryGetFirstLocation() ?? GetFirstLocation());
					}
					else if (this.IsRestrictedType())
					{
						diagnostics.Add(ErrorCode.WRN_InlineArrayNotSupportedByLanguage, GetFirstLocation());
					}
				}
			}
			else
			{
				diagnostics.Add(ErrorCode.ERR_InvalidInlineArrayFields, GetFirstLocation());
			}
			if (!ContainingAssembly.RuntimeSupportsInlineArrayTypes)
			{
				diagnostics.Add(ErrorCode.ERR_RuntimeDoesNotSupportInlineArrayTypes, GetFirstLocation());
			}
		}
		if (this.IsMicrosoftCodeAnalysisEmbeddedAttribute() && (DeclaredAccessibility != Accessibility.Internal || TypeKind != TypeKind.Class || !IsSealed || IsStatic || IsFileLocal || !base.InstanceConstructors.Any(delegate(MethodSymbol c)
		{
			if ((object)c != null && c.ParameterCount == 0)
			{
				Accessibility declaredAccessibility = c.DeclaredAccessibility;
				if (declaredAccessibility == Accessibility.Internal || declaredAccessibility == Accessibility.Public)
				{
					return true;
				}
			}
			return false;
		}) || !DeclaringCompilation.IsAttributeType(this) || (GetAttributeUsageInfo().ValidTargets & (AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Delegate)) != (AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Delegate)))
		{
			diagnostics.Add(ErrorCode.ERR_EmbeddedAttributeMustFollowPattern, GetFirstLocation());
		}
		if (IsExtension)
		{
			NamedTypeSymbol? containingType = ContainingType;
			if (((object)containingType == null || !containingType.IsExtension) && ((object)ContainingType == null || !ContainingType.IsStatic || ContainingType.Arity != 0 || (object)ContainingType.ContainingType != null))
			{
				ExtensionBlockDeclarationSyntax extensionBlockDeclarationSyntax = (ExtensionBlockDeclarationSyntax)this.GetNonNullSyntaxNode();
				diagnostics.Add(ErrorCode.ERR_BadExtensionContainingType, extensionBlockDeclarationSyntax.Keyword);
			}
		}
	}

	internal sealed override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<TypeSymbol> basesBeingResolved)
	{
		if (_lazyInterfaces.IsDefault)
		{
			if (basesBeingResolved != null && basesBeingResolved.ContainsReference(OriginalDefinition))
			{
				return ImmutableArray<NamedTypeSymbol>.Empty;
			}
			BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
			ImmutableArray<NamedTypeSymbol> value = MakeAcyclicInterfaces(basesBeingResolved, instance);
			if (ImmutableInterlocked.InterlockedCompareExchange(ref _lazyInterfaces, value, default(ImmutableArray<NamedTypeSymbol>)).IsDefault)
			{
				AddDeclarationDiagnostics(instance);
			}
			instance.Free();
		}
		return _lazyInterfaces;
	}

	protected override void CheckBase(BindingDiagnosticBag diagnostics)
	{
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
		if ((object)baseTypeNoUseSiteDiagnostics == null)
		{
			return;
		}
		Location location = null;
		bool flag = baseTypeNoUseSiteDiagnostics.ContainsErrorType();
		if (!flag)
		{
			location = FindBaseRefSyntax(baseTypeNoUseSiteDiagnostics);
		}
		if (base.IsGenericType && !flag && DeclaringCompilation.IsAttributeType(baseTypeNoUseSiteDiagnostics))
		{
			MessageID.IDS_FeatureGenericAttributes.CheckFeatureAvailability(diagnostics, DeclaringCompilation, location);
		}
		SingleTypeDeclaration singleTypeDeclaration = FirstDeclarationWithExplicitBases();
		if (singleTypeDeclaration != null)
		{
			TypeConversions typeConversions = ContainingAssembly.CorLibrary.TypeConversions;
			SourceLocation nameLocation = singleTypeDeclaration.NameLocation;
			baseTypeNoUseSiteDiagnostics.CheckAllConstraints(DeclaringCompilation, typeConversions, nameLocation, diagnostics);
		}
		if (!this.IsClassType() || baseTypeNoUseSiteDiagnostics.IsObjectType() || flag)
		{
			return;
		}
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, ContainingAssembly);
		if (declaration.Kind == DeclarationKind.Record)
		{
			if ((object)SynthesizedRecordClone.FindValidCloneMethod(baseTypeNoUseSiteDiagnostics, ref useSiteInfo) == null)
			{
				diagnostics.Add(ErrorCode.ERR_BadRecordBase, location);
			}
		}
		else if ((object)SynthesizedRecordClone.FindValidCloneMethod(baseTypeNoUseSiteDiagnostics, ref useSiteInfo) != null)
		{
			diagnostics.Add(ErrorCode.ERR_BadInheritanceFromRecord, location);
		}
		diagnostics.Add(location, useSiteInfo);
	}

	protected override void CheckInterfaces(BindingDiagnosticBag diagnostics)
	{
		MultiDictionary<NamedTypeSymbol, NamedTypeSymbol> interfacesAndTheirBaseInterfacesNoUseSiteDiagnostics = base.InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics;
		if (interfacesAndTheirBaseInterfacesNoUseSiteDiagnostics.IsEmpty)
		{
			return;
		}
		SingleTypeDeclaration singleTypeDeclaration = FirstDeclarationWithExplicitBases();
		if (singleTypeDeclaration == null)
		{
			return;
		}
		TypeConversions typeConversions = ContainingAssembly.CorLibrary.TypeConversions;
		SourceLocation nameLocation = singleTypeDeclaration.NameLocation;
		foreach (KeyValuePair<NamedTypeSymbol, MultiDictionary<NamedTypeSymbol, NamedTypeSymbol>.ValueSet> item in interfacesAndTheirBaseInterfacesNoUseSiteDiagnostics)
		{
			MultiDictionary<NamedTypeSymbol, NamedTypeSymbol>.ValueSet value = item.Value;
			foreach (NamedTypeSymbol item2 in value)
			{
				item2.CheckAllConstraints(DeclaringCompilation, typeConversions, nameLocation, diagnostics);
			}
			if (value.Count <= 1)
			{
				continue;
			}
			NamedTypeSymbol key = item.Key;
			foreach (NamedTypeSymbol item3 in value)
			{
				if ((object)key == item3)
				{
					continue;
				}
				if (key.Equals(item3, TypeCompareKind.IgnoreNullableModifiersForReferenceTypes))
				{
					if (!key.Equals(item3, TypeCompareKind.ObliviousNullableModifierMatchesAny))
					{
						diagnostics.Add(ErrorCode.WRN_DuplicateInterfaceWithNullabilityMismatchInBaseList, nameLocation, item3, this);
					}
				}
				else if (key.Equals(item3, TypeCompareKind.IgnoreTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes))
				{
					diagnostics.Add(ErrorCode.ERR_DuplicateInterfaceWithTupleNamesInBaseList, nameLocation, item3, key, this);
				}
				else
				{
					diagnostics.Add(ErrorCode.ERR_DuplicateInterfaceWithDifferencesInBaseList, nameLocation, item3, key, this);
				}
			}
		}
	}

	private SourceLocation FindBaseRefSyntax(NamedTypeSymbol baseSym)
	{
		foreach (SingleTypeDeclaration declaration in declaration.Declarations)
		{
			BaseListSyntax baseListOpt = GetBaseListOpt(declaration);
			if (baseListOpt == null)
			{
				continue;
			}
			Binder binder = DeclaringCompilation.GetBinder(baseListOpt);
			binder = binder.WithAdditionalFlagsAndContainingMemberOrLambda(BinderFlags.SuppressConstraintChecks, this);
			foreach (BaseTypeSyntax type3 in baseListOpt.Types)
			{
				TypeSyntax type = type3.Type;
				TypeSymbol type2 = binder.BindType(type, BindingDiagnosticBag.Discarded).Type;
				if (baseSym.Equals(type2))
				{
					return new SourceLocation(type);
				}
			}
		}
		return null;
	}

	private SingleTypeDeclaration FirstDeclarationWithExplicitBases()
	{
		foreach (SingleTypeDeclaration declaration in declaration.Declarations)
		{
			if (GetBaseListOpt(declaration) != null)
			{
				return declaration;
			}
		}
		return null;
	}

	internal Tuple<NamedTypeSymbol, ImmutableArray<NamedTypeSymbol>> GetDeclaredBases(ConsList<TypeSymbol> basesBeingResolved)
	{
		if (_lazyDeclaredBases == null)
		{
			BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
			if (Interlocked.CompareExchange(ref _lazyDeclaredBases, MakeDeclaredBases(basesBeingResolved, instance), null) == null)
			{
				AddDeclarationDiagnostics(instance);
			}
			instance.Free();
		}
		return _lazyDeclaredBases;
	}

	internal override NamedTypeSymbol GetDeclaredBaseType(ConsList<TypeSymbol> basesBeingResolved)
	{
		return GetDeclaredBases(basesBeingResolved).Item1;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetDeclaredInterfaces(ConsList<TypeSymbol> basesBeingResolved)
	{
		return GetDeclaredBases(basesBeingResolved).Item2;
	}

	private Tuple<NamedTypeSymbol, ImmutableArray<NamedTypeSymbol>> MakeDeclaredBases(ConsList<TypeSymbol> basesBeingResolved, BindingDiagnosticBag diagnostics)
	{
		if (TypeKind == TypeKind.Enum)
		{
			return new Tuple<NamedTypeSymbol, ImmutableArray<NamedTypeSymbol>>(null, ImmutableArray<NamedTypeSymbol>.Empty);
		}
		bool reportedPartialConflict = false;
		ConsList<TypeSymbol> newBasesBeingResolved = basesBeingResolved.Prepend(OriginalDefinition);
		ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		NamedTypeSymbol baseType = null;
		SourceLocation baseTypeLocation = null;
		PooledDictionary<NamedTypeSymbol, SourceLocation> pooledSymbolDictionaryInstance = SpecializedSymbolCollections.GetPooledSymbolDictionaryInstance<NamedTypeSymbol, SourceLocation>();
		foreach (SingleTypeDeclaration declaration in declaration.Declarations)
		{
			SingleTypeDeclaration decl = declaration;
			Tuple<NamedTypeSymbol, ImmutableArray<NamedTypeSymbol>> tuple = MakeOneDeclaredBases(newBasesBeingResolved, decl, diagnostics);
			if (tuple == null)
			{
				continue;
			}
			NamedTypeSymbol item = tuple.Item1;
			ImmutableArray<NamedTypeSymbol> immutableArray = tuple.Item2;
			if (!reportedPartialConflict)
			{
				if ((object)baseType == null)
				{
					baseType = item;
					baseTypeLocation = decl.NameLocation;
				}
				else if (baseType.TypeKind == TypeKind.Error && (object)item != null)
				{
					immutableArray = immutableArray.Add(baseType);
					baseType = item;
					baseTypeLocation = decl.NameLocation;
				}
				else if ((object)item != null && !TypeSymbol.Equals(item, baseType, TypeCompareKind.ConsiderEverything) && item.TypeKind != TypeKind.Error)
				{
					if (item.Equals(baseType, TypeCompareKind.ObliviousNullableModifierMatchesAny))
					{
						if (containsOnlyOblivious(baseType))
						{
							baseType = item;
							baseTypeLocation = decl.NameLocation;
						}
						else if (!containsOnlyOblivious(item))
						{
							reportBaseType();
						}
					}
					else
					{
						reportBaseType();
					}
				}
			}
			foreach (NamedTypeSymbol item3 in immutableArray)
			{
				if (!pooledSymbolDictionaryInstance.ContainsKey(item3))
				{
					instance.Add(item3);
					pooledSymbolDictionaryInstance.Add(item3, decl.NameLocation);
				}
			}
			void reportBaseType()
			{
				CSDiagnosticInfo errorInfo = diagnostics.Add(ErrorCode.ERR_PartialMultipleBases, GetFirstLocation(), this);
				baseType = new ExtendedErrorTypeSymbol(baseType, LookupResultKind.Ambiguous, errorInfo);
				baseTypeLocation = decl.NameLocation;
				reportedPartialConflict = true;
			}
		}
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, ContainingAssembly);
		DeclarationKind kind = base.declaration.Kind;
		if (kind - 9 <= DeclarationKind.Class)
		{
			NamedTypeSymbol namedTypeSymbol = DeclaringCompilation.GetWellKnownType(WellKnownType.System_IEquatable_T).Construct(this);
			if (instance.IndexOf(namedTypeSymbol, SymbolEqualityComparer.AllIgnoreOptions) < 0)
			{
				instance.Add(namedTypeSymbol);
				namedTypeSymbol.AddUseSiteInfo(ref useSiteInfo);
			}
		}
		if ((object)baseType != null)
		{
			if (baseType.IsStatic)
			{
				diagnostics.Add(ErrorCode.ERR_StaticBaseClass, baseTypeLocation, baseType, this);
			}
			TypeSymbol typeSymbol = baseType.FindTypeLessVisibleThan(this, ref useSiteInfo);
			if ((object)typeSymbol != null)
			{
				if ((object)baseType == typeSymbol)
				{
					diagnostics.Add(ErrorCode.ERR_BadVisBaseClass, baseTypeLocation, this, typeSymbol);
				}
				else
				{
					diagnostics.Add(ErrorCode.ERR_BadVisBaseType, baseTypeLocation, this, typeSymbol);
				}
			}
			if (baseType.HasFileLocalTypes() && !this.HasFileLocalTypes())
			{
				diagnostics.Add(ErrorCode.ERR_FileTypeBase, baseTypeLocation, baseType, this);
			}
		}
		ImmutableArray<NamedTypeSymbol> item2 = instance.ToImmutableAndFree();
		if (DeclaredAccessibility != Accessibility.Private && IsInterface)
		{
			foreach (NamedTypeSymbol item4 in item2)
			{
				if (!item4.IsAtLeastAsVisibleAs(this, ref useSiteInfo))
				{
					diagnostics.Add(ErrorCode.ERR_BadVisBaseInterface, pooledSymbolDictionaryInstance[item4], this, item4);
				}
				if (item4.HasFileLocalTypes() && !this.HasFileLocalTypes())
				{
					diagnostics.Add(ErrorCode.ERR_FileTypeBase, pooledSymbolDictionaryInstance[item4], item4, this);
				}
			}
		}
		pooledSymbolDictionaryInstance.Free();
		diagnostics.Add(GetFirstLocation(), useSiteInfo);
		return new Tuple<NamedTypeSymbol, ImmutableArray<NamedTypeSymbol>>(baseType, item2);
		static bool containsOnlyOblivious(TypeSymbol type)
		{
			return (object)TypeWithAnnotations.Create(type).VisitType(null, (TypeWithAnnotations typeWithAnnotations, object arg, bool flag) => !typeWithAnnotations.Type.IsValueType && !typeWithAnnotations.NullableAnnotation.IsOblivious(), null, null) == null;
		}
	}

	private static BaseListSyntax GetBaseListOpt(SingleTypeDeclaration decl)
	{
		if (decl.HasBaseDeclarations)
		{
			return ((BaseTypeDeclarationSyntax)decl.SyntaxReference.GetSyntax()).BaseList;
		}
		return null;
	}

	private Tuple<NamedTypeSymbol, ImmutableArray<NamedTypeSymbol>> MakeOneDeclaredBases(ConsList<TypeSymbol> newBasesBeingResolved, SingleTypeDeclaration decl, BindingDiagnosticBag diagnostics)
	{
		BaseListSyntax baseListOpt = GetBaseListOpt(decl);
		if (baseListOpt == null)
		{
			return null;
		}
		NamedTypeSymbol namedTypeSymbol = null;
		ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		Binder binder = DeclaringCompilation.GetBinder(baseListOpt);
		binder = binder.WithAdditionalFlagsAndContainingMemberOrLambda(BinderFlags.SuppressConstraintChecks, this);
		int num = -1;
		foreach (BaseTypeSyntax type3 in baseListOpt.Types)
		{
			num++;
			TypeSyntax type = type3.Type;
			if (type.Kind() != SyntaxKind.PredefinedType && !SyntaxFacts.IsName(type.Kind()))
			{
				diagnostics.Add(ErrorCode.ERR_BadBaseType, type.GetLocation());
			}
			SourceLocation location = new SourceLocation(type);
			TypeSymbol type2;
			if (num == 0 && TypeKind == TypeKind.Class)
			{
				type2 = binder.BindType(type, diagnostics, newBasesBeingResolved).Type;
				SpecialType specialType = type2.SpecialType;
				if (IsRestrictedBaseType(specialType) && (base.SpecialType != SpecialType.System_Enum || specialType != SpecialType.System_ValueType) && (base.SpecialType != SpecialType.System_MulticastDelegate || specialType != SpecialType.System_Delegate) && (specialType != SpecialType.System_Array || !(ContainingAssembly.CorLibrary == ContainingAssembly)))
				{
					diagnostics.Add(ErrorCode.ERR_DeriveFromEnumOrValueType, location, this, type2);
					continue;
				}
				if (type2.IsSealed && !IsStatic)
				{
					diagnostics.Add(ErrorCode.ERR_CantDeriveFromSealedType, location, this, type2);
					continue;
				}
				bool flag = false;
				if (type2.TypeKind == TypeKind.Error)
				{
					flag = true;
					if (type2.GetNonErrorTypeKindGuess() == TypeKind.Interface)
					{
						flag = false;
					}
				}
				if (((type2.TypeKind == TypeKind.Class || type2.TypeKind == TypeKind.Delegate || type2.TypeKind == TypeKind.Struct) | flag) && (object)namedTypeSymbol == null)
				{
					namedTypeSymbol = (NamedTypeSymbol)type2;
					if (IsStatic && namedTypeSymbol.SpecialType != SpecialType.System_Object)
					{
						CSDiagnosticInfo errorInfo = diagnostics.Add(ErrorCode.ERR_StaticDerivedFromNonObject, location, this, namedTypeSymbol);
						namedTypeSymbol = new ExtendedErrorTypeSymbol(namedTypeSymbol, LookupResultKind.NotReferencable, errorInfo);
					}
					checkPrimaryConstructorBaseType(type3, namedTypeSymbol);
					continue;
				}
			}
			else
			{
				type2 = binder.BindType(type, diagnostics, newBasesBeingResolved).Type;
			}
			if (num == 0)
			{
				checkPrimaryConstructorBaseType(type3, type2);
			}
			switch (type2.TypeKind)
			{
			case TypeKind.Interface:
				foreach (NamedTypeSymbol item in instance)
				{
					if (item.Equals(type2, TypeCompareKind.ConsiderEverything))
					{
						diagnostics.Add(ErrorCode.ERR_DuplicateInterfaceInBaseList, location, type2);
					}
					else if (item.Equals(type2, TypeCompareKind.ObliviousNullableModifierMatchesAny))
					{
						diagnostics.Add(ErrorCode.WRN_DuplicateInterfaceWithNullabilityMismatchInBaseList, location, type2, this);
					}
				}
				if (IsStatic)
				{
					diagnostics.Add(ErrorCode.ERR_StaticClassInterfaceImpl, location, this);
				}
				if (IsRefLikeType)
				{
					Binder.CheckFeatureAvailability(type, MessageID.IDS_FeatureRefStructInterfaces, diagnostics);
				}
				if (type2.ContainsDynamic())
				{
					diagnostics.Add(ErrorCode.ERR_DeriveFromConstructedDynamic, location, this, type2);
				}
				instance.Add((NamedTypeSymbol)type2);
				continue;
			case TypeKind.Class:
				if (TypeKind == TypeKind.Class)
				{
					if ((object)namedTypeSymbol == null)
					{
						namedTypeSymbol = (NamedTypeSymbol)type2;
						diagnostics.Add(ErrorCode.ERR_BaseClassMustBeFirst, location, type2);
					}
					else
					{
						diagnostics.Add(ErrorCode.ERR_NoMultipleInheritance, location, this, namedTypeSymbol, type2);
					}
					continue;
				}
				break;
			case TypeKind.TypeParameter:
				diagnostics.Add(ErrorCode.ERR_DerivingFromATyVar, location, type2);
				continue;
			case TypeKind.Error:
				instance.Add((NamedTypeSymbol)type2);
				continue;
			case TypeKind.Dynamic:
				diagnostics.Add(ErrorCode.ERR_DeriveFromDynamic, location, this);
				continue;
			case TypeKind.Submission:
				throw ExceptionUtilities.UnexpectedValue(type2.TypeKind);
			}
			diagnostics.Add(ErrorCode.ERR_NonInterfaceInInterfaceList, location, type2);
		}
		if (base.SpecialType == SpecialType.System_Object && ((object)namedTypeSymbol != null || instance.Count != 0))
		{
			SyntaxToken token = GetName(baseListOpt.Parent);
			diagnostics.Add(ErrorCode.ERR_ObjectCantHaveBases, new SourceLocation(in token));
		}
		return new Tuple<NamedTypeSymbol, ImmutableArray<NamedTypeSymbol>>(namedTypeSymbol, instance.ToImmutableAndFree());
		void checkPrimaryConstructorBaseType(BaseTypeSyntax baseTypeSyntax, TypeSymbol baseType)
		{
			if (baseTypeSyntax is PrimaryConstructorBaseTypeSyntax primaryConstructorBaseTypeSyntax)
			{
				if (TypeKind != TypeKind.Class || baseType.TypeKind == TypeKind.Interface)
				{
					diagnostics.Add(ErrorCode.ERR_UnexpectedArgumentList, primaryConstructorBaseTypeSyntax.ArgumentList.Location);
				}
				else if (((TypeDeclarationSyntax)decl.SyntaxReference.GetSyntax()).ParameterList == null)
				{
					diagnostics.Add(ErrorCode.ERR_UnexpectedArgumentListInBaseTypeWithoutParameterList, primaryConstructorBaseTypeSyntax.ArgumentList.Location);
				}
			}
		}
	}

	private static bool IsRestrictedBaseType(SpecialType specialType)
	{
		if ((uint)(specialType - 2) <= 3u || specialType == SpecialType.System_Array)
		{
			return true;
		}
		return false;
	}

	private ImmutableArray<NamedTypeSymbol> MakeAcyclicInterfaces(ConsList<TypeSymbol> basesBeingResolved, BindingDiagnosticBag diagnostics)
	{
		TypeKind typeKind = TypeKind;
		if (typeKind == TypeKind.Enum)
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}
		ImmutableArray<NamedTypeSymbol> declaredInterfaces = GetDeclaredInterfaces(basesBeingResolved);
		bool flag = typeKind == TypeKind.Interface;
		ArrayBuilder<NamedTypeSymbol> arrayBuilder = (flag ? ArrayBuilder<NamedTypeSymbol>.GetInstance() : null);
		foreach (NamedTypeSymbol item in declaredInterfaces)
		{
			if (flag)
			{
				if (BaseTypeAnalysis.TypeDependsOn(item, this))
				{
					arrayBuilder.Add(new ExtendedErrorTypeSymbol(item, LookupResultKind.NotReferencable, diagnostics.Add(ErrorCode.ERR_CycleInInterfaceInheritance, GetFirstLocation(), this, item)));
					continue;
				}
				arrayBuilder.Add(item);
			}
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, ContainingAssembly);
			if (item.DeclaringCompilation != DeclaringCompilation)
			{
				item.AddUseSiteInfo(ref useSiteInfo);
				foreach (NamedTypeSymbol allInterfacesNoUseSiteDiagnostic in item.AllInterfacesNoUseSiteDiagnostics)
				{
					if (allInterfacesNoUseSiteDiagnostic.DeclaringCompilation != DeclaringCompilation)
					{
						allInterfacesNoUseSiteDiagnostic.AddUseSiteInfo(ref useSiteInfo);
					}
				}
			}
			diagnostics.Add(GetFirstLocation(), useSiteInfo);
		}
		if (!flag)
		{
			return declaredInterfaces;
		}
		return arrayBuilder.ToImmutableAndFree();
	}

	private NamedTypeSymbol MakeAcyclicBaseType(BindingDiagnosticBag diagnostics)
	{
		TypeKind typeKind = TypeKind;
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		bool flag = false;
		NamedTypeSymbol namedTypeSymbol;
		switch (typeKind)
		{
		case TypeKind.Enum:
			namedTypeSymbol = declaringCompilation.GetSpecialType(SpecialType.System_Enum);
			flag = true;
			break;
		case TypeKind.Extension:
			Binder.GetSpecialType(declaringCompilation, SpecialType.System_Object, GetFirstLocationOrNone(), diagnostics);
			return null;
		default:
			namedTypeSymbol = GetDeclaredBaseType(null);
			break;
		}
		if ((object)namedTypeSymbol == null)
		{
			switch (typeKind)
			{
			case TypeKind.Class:
				if (base.SpecialType == SpecialType.System_Object)
				{
					return null;
				}
				namedTypeSymbol = declaringCompilation.GetSpecialType(SpecialType.System_Object);
				flag = true;
				break;
			case TypeKind.Struct:
				namedTypeSymbol = declaringCompilation.GetSpecialType(SpecialType.System_ValueType);
				flag = true;
				break;
			case TypeKind.Interface:
				return null;
			case TypeKind.Delegate:
				namedTypeSymbol = declaringCompilation.GetSpecialType(SpecialType.System_MulticastDelegate);
				flag = true;
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(typeKind);
			}
		}
		if (BaseTypeAnalysis.TypeDependsOn(namedTypeSymbol, this))
		{
			return new ExtendedErrorTypeSymbol(namedTypeSymbol, LookupResultKind.NotReferencable, diagnostics.Add(ErrorCode.ERR_CircularBase, GetFirstLocation(), namedTypeSymbol, this));
		}
		SetKnownToHaveNoDeclaredBaseCycles();
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, ContainingAssembly);
		NamedTypeSymbol namedTypeSymbol2 = namedTypeSymbol;
		while (namedTypeSymbol2.DeclaringCompilation != DeclaringCompilation)
		{
			namedTypeSymbol2.AddUseSiteInfo(ref useSiteInfo);
			namedTypeSymbol2 = namedTypeSymbol2.BaseTypeNoUseSiteDiagnostics;
			if ((object)namedTypeSymbol2 == null)
			{
				break;
			}
		}
		diagnostics.Add(useSiteInfo.Diagnostics.IsNullOrEmpty() ? Location.None : ((flag ? null : FindBaseRefSyntax(namedTypeSymbol)) ?? GetFirstLocation()), useSiteInfo);
		return namedTypeSymbol;
	}

	private NamedTypeSymbol GetEnumUnderlyingType(BindingDiagnosticBag diagnostics)
	{
		if (TypeKind != TypeKind.Enum)
		{
			return null;
		}
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		BaseListSyntax baseListOpt = GetBaseListOpt(declaration.Declarations[0]);
		if (baseListOpt != null)
		{
			SeparatedSyntaxList<BaseTypeSyntax> types = baseListOpt.Types;
			if (types.Count > 0)
			{
				TypeSyntax type = types[0].Type;
				TypeSymbol typeSymbol = declaringCompilation.GetBinder(baseListOpt).BindType(type, diagnostics).Type;
				if (!typeSymbol.SpecialType.IsValidEnumUnderlyingType())
				{
					diagnostics.Add(ErrorCode.ERR_IntegralTypeExpected, type.Location);
					typeSymbol = declaringCompilation.GetSpecialType(SpecialType.System_Int32);
				}
				return (NamedTypeSymbol)typeSymbol;
			}
		}
		NamedTypeSymbol specialType = declaringCompilation.GetSpecialType(SpecialType.System_Int32);
		Binder.ReportUseSite(specialType, diagnostics, GetFirstLocation());
		return specialType;
	}

	internal string ComputeExtensionGroupingRawName()
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		builder.Append("extension");
		if (Arity > 0)
		{
			builder.Append("<");
			foreach (TypeParameterSymbol typeParameter in TypeParameters)
			{
				if (typeParameter.Ordinal > 0)
				{
					builder.Append(", ");
				}
				appendTypeParameterDeclaration(typeParameter, builder);
			}
			builder.Append(">");
		}
		builder.Append("(");
		ParameterSymbol extensionParameter = ExtensionParameter;
		if ((object)extensionParameter != null)
		{
			TypeWithAnnotations typeWithAnnotations = extensionParameter.TypeWithAnnotations;
			AppendClrType(typeWithAnnotations.Type, typeWithAnnotations.CustomModifiers, builder);
		}
		builder.Append(")");
		return instance.ToStringAndFree();
		static void appendTypeParameterDeclaration(TypeParameterSymbol typeParameter, StringBuilder stringBuilder)
		{
			if (typeParameter.HasReferenceTypeConstraint)
			{
				stringBuilder.Append("class ");
			}
			else if (typeParameter.HasValueTypeConstraint || typeParameter.HasUnmanagedTypeConstraint)
			{
				stringBuilder.Append("valuetype ");
			}
			if (typeParameter.AllowsRefLikeType)
			{
				stringBuilder.Append("byreflike ");
			}
			if (typeParameter.HasConstructorConstraint || typeParameter.HasValueTypeConstraint || typeParameter.HasUnmanagedTypeConstraint)
			{
				stringBuilder.Append(".ctor ");
			}
			appendTypeParameterTypeConstraints(typeParameter, stringBuilder);
			if (stringBuilder[stringBuilder.Length - 1] == ' ')
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
		}
		static void appendTypeParameterTypeConstraints(TypeParameterSymbol typeParameter, StringBuilder stringBuilder)
		{
			ImmutableArray<TypeWithAnnotations> constraintTypes = typeParameter.GetConstraintTypes(ConsList<TypeParameterSymbol>.Empty);
			if (!constraintTypes.IsEmpty || typeParameter.HasUnmanagedTypeConstraint || typeParameter.HasValueTypeConstraint)
			{
				ArrayBuilder<string> instance2 = ArrayBuilder<string>.GetInstance(constraintTypes.Length);
				foreach (TypeWithAnnotations item in constraintTypes)
				{
					PooledStringBuilder instance3 = PooledStringBuilder.GetInstance();
					AppendClrType(item.Type, item.CustomModifiers, instance3.Builder);
					instance2.Add(instance3.ToStringAndFree());
				}
				if (typeParameter.HasUnmanagedTypeConstraint)
				{
					instance2.Add("System.ValueType modreq(System.Runtime.InteropServices.UnmanagedType)");
				}
				else if (typeParameter.HasValueTypeConstraint)
				{
					instance2.Add("System.ValueType");
				}
				instance2.Sort(StringComparer.Ordinal);
				stringBuilder.Append('(');
				for (int i = 0; i < instance2.Count; i++)
				{
					if (i > 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(instance2[i]);
				}
				instance2.Free();
				stringBuilder.Append(")");
			}
		}
	}

	private static void AppendClrType(TypeSymbol type, ImmutableArray<CustomModifier> modifiers, StringBuilder builder)
	{
		if (type is NamedTypeSymbol namedType)
		{
			appendNamedType(type, builder, namedType);
		}
		else if (type is TypeParameterSymbol typeParameter)
		{
			appendTypeParameterReference(typeParameter, builder);
		}
		else if (type is ArrayTypeSymbol array)
		{
			appendArrayType(array, builder);
		}
		else if (type is PointerTypeSymbol { PointedAtTypeWithAnnotations: var pointedAtTypeWithAnnotations })
		{
			AppendClrType(pointedAtTypeWithAnnotations.Type, pointedAtTypeWithAnnotations.CustomModifiers, builder);
			builder.Append('*');
		}
		else if (type is FunctionPointerTypeSymbol functionPointer)
		{
			appendFunctionPointerType(functionPointer, builder);
		}
		else
		{
			if (!(type is DynamicTypeSymbol))
			{
				throw ExceptionUtilities.UnexpectedValue(type);
			}
			builder.Append("System.Object");
		}
		appendModifiers(modifiers, builder);
		static void appendArrayType(ArrayTypeSymbol arrayTypeSymbol, StringBuilder stringBuilder)
		{
			TypeWithAnnotations elementTypeWithAnnotations = arrayTypeSymbol.ElementTypeWithAnnotations;
			AppendClrType(elementTypeWithAnnotations.Type, elementTypeWithAnnotations.CustomModifiers, stringBuilder);
			stringBuilder.Append('[');
			for (int i = 1; i < arrayTypeSymbol.Rank; i++)
			{
				stringBuilder.Append(',');
			}
			stringBuilder.Append(']');
		}
		static void appendContainingType(NamedTypeSymbol namedTypeSymbol, StringBuilder stringBuilder)
		{
			NamedTypeSymbol containingType = namedTypeSymbol.ContainingType;
			if ((object)containingType != null)
			{
				appendContainingType(containingType, stringBuilder);
				stringBuilder.Append(containingType.MetadataName);
				stringBuilder.Append('/');
			}
		}
		static void appendFunctionPointerType(FunctionPointerTypeSymbol functionPointerTypeSymbol, StringBuilder stringBuilder)
		{
			stringBuilder.Append("method ");
			FunctionPointerMethodSymbol signature = functionPointerTypeSymbol.Signature;
			stringBuilder.Append(signature.CallingConvention switch
			{
				Microsoft.Cci.CallingConvention.Default => null, 
				Microsoft.Cci.CallingConvention.Unmanaged => "unmanaged ", 
				Microsoft.Cci.CallingConvention.CDecl => "unmanaged cdecl ", 
				Microsoft.Cci.CallingConvention.Standard => "unmanaged stdcall ", 
				Microsoft.Cci.CallingConvention.ThisCall => "unmanaged thiscall ", 
				Microsoft.Cci.CallingConvention.FastCall => "unmanaged fastcall ", 
				_ => throw ExceptionUtilities.UnexpectedValue(signature.CallingConvention), 
			});
			TypeWithAnnotations returnTypeWithAnnotations = signature.ReturnTypeWithAnnotations;
			AppendClrType(returnTypeWithAnnotations.Type, returnTypeWithAnnotations.CustomModifiers, stringBuilder);
			if (signature.RefKind != RefKind.None)
			{
				stringBuilder.Append('&');
				appendModifiers(signature.RefCustomModifiers, stringBuilder);
			}
			stringBuilder.Append(" *(");
			ImmutableArray<ParameterSymbol> parameters = signature.Parameters;
			for (int i = 0; i < parameters.Length; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(", ");
				}
				ParameterSymbol parameterSymbol = parameters[i];
				TypeWithAnnotations typeWithAnnotations = parameterSymbol.TypeWithAnnotations;
				AppendClrType(typeWithAnnotations.Type, typeWithAnnotations.CustomModifiers, stringBuilder);
				if (parameterSymbol.RefKind != RefKind.None)
				{
					stringBuilder.Append('&');
					appendModifiers(parameterSymbol.RefCustomModifiers, stringBuilder);
				}
			}
			stringBuilder.Append(')');
		}
		static void appendModifiers(ImmutableArray<CustomModifier> customModifiers, StringBuilder stringBuilder)
		{
			for (int num = customModifiers.Length - 1; num >= 0; num--)
			{
				CustomModifier customModifier = customModifiers[num];
				PooledStringBuilder instance = PooledStringBuilder.GetInstance();
				instance.Builder.Append(customModifier.IsOptional ? " modopt(" : " modreq(");
				AppendClrType(((CSharpCustomModifier)customModifier).ModifierSymbol, ImmutableArray<CustomModifier>.Empty, instance.Builder);
				instance.Builder.Append(')');
				stringBuilder.Append(instance.ToStringAndFree());
			}
		}
		static void appendNamedType(TypeSymbol typeSymbol, StringBuilder stringBuilder, NamedTypeSymbol namedTypeSymbol)
		{
			if (namedTypeSymbol.SpecialType == SpecialType.System_Void)
			{
				stringBuilder.Append("void");
			}
			else if (namedTypeSymbol.Name == "void" && namedTypeSymbol.IsTopLevelType() && namedTypeSymbol.ContainingNamespace.IsGlobalNamespace)
			{
				stringBuilder.Append("'void'");
			}
			else
			{
				appendNamespace(namedTypeSymbol.ContainingNamespace, stringBuilder);
				appendContainingType(namedTypeSymbol, stringBuilder);
				stringBuilder.Append(namedTypeSymbol.MetadataName);
				appendTypeArguments(namedTypeSymbol, stringBuilder);
			}
		}
		static void appendNamespace(NamespaceSymbol ns, StringBuilder stringBuilder)
		{
			if ((object)ns != null && !ns.IsGlobalNamespace)
			{
				appendNamespace(ns.ContainingNamespace, stringBuilder);
				stringBuilder.Append(ns.Name);
				stringBuilder.Append('.');
			}
		}
		static void appendTypeArguments(NamedTypeSymbol namedTypeSymbol, StringBuilder stringBuilder)
		{
			if (!namedTypeSymbol.IsUnboundGenericType)
			{
				ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance();
				namedTypeSymbol.GetAllTypeArgumentsNoUseSiteDiagnostics(instance);
				if (instance.Count > 0)
				{
					stringBuilder.Append('<');
					for (int i = 0; i < instance.Count; i++)
					{
						if (i > 0)
						{
							stringBuilder.Append(", ");
						}
						TypeWithAnnotations typeWithAnnotations = instance[i];
						AppendClrType(typeWithAnnotations.Type, typeWithAnnotations.CustomModifiers, stringBuilder);
					}
					stringBuilder.Append('>');
				}
				instance.Free();
			}
		}
		static void appendTypeParameterReference(TypeParameterSymbol typeParameterSymbol, StringBuilder stringBuilder)
		{
			if (typeParameterSymbol.ContainingType.IsExtension)
			{
				stringBuilder.Append("!");
				stringBuilder.Append(StringExtensions.GetNumeral(typeParameterSymbol.Ordinal));
			}
			else
			{
				stringBuilder.Append("!");
				stringBuilder.Append(typeParameterSymbol.Name);
			}
		}
	}

	internal string ComputeExtensionMarkerRawName()
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		builder.Append("extension");
		if (Arity > 0)
		{
			builder.Append("<");
			foreach (TypeParameterSymbol typeParameter in TypeParameters)
			{
				if (typeParameter.Ordinal > 0)
				{
					builder.Append(", ");
				}
				appendAttributes(typeParameter.GetAttributes(), builder);
				appendIdentifier(typeParameter.Name, builder);
			}
			builder.Append(">");
		}
		builder.Append("(");
		ParameterSymbol extensionParameter = ExtensionParameter;
		if ((object)extensionParameter != null)
		{
			if (extensionParameter.DeclaredScope != ScopedKind.None)
			{
				builder.Append("scoped ");
			}
			appendRefKind(extensionParameter.RefKind, builder, forParameter: true);
			appendAttributes(extensionParameter.GetAttributes(), builder);
			appendTypeWithAnnotation(extensionParameter.TypeWithAnnotations, builder);
			string name = extensionParameter.Name;
			if (name != "")
			{
				builder.Append(' ');
				appendIdentifier(name, builder);
			}
		}
		builder.Append(")");
		foreach (TypeParameterSymbol typeParameter2 in TypeParameters)
		{
			if (typeParameterHasConstraints(typeParameter2))
			{
				appendTypeParameterConstraints(typeParameter2, builder);
			}
		}
		return instance.ToStringAndFree();
		static void appendAnnotation(StringBuilder stringBuilder, NullableAnnotation annotation)
		{
			switch (annotation)
			{
			case NullableAnnotation.Annotated:
				stringBuilder.Append('?');
				break;
			case NullableAnnotation.NotAnnotated:
				stringBuilder.Append('!');
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(annotation);
			case NullableAnnotation.Oblivious:
			case NullableAnnotation.Ignored:
				break;
			}
		}
		static void appendArrayType(ArrayTypeSymbol array, StringBuilder stringBuilder)
		{
			appendTypeWithAnnotation(array.ElementTypeWithAnnotations, stringBuilder);
			stringBuilder.Append('[');
			for (int i = 1; i < array.Rank; i++)
			{
				stringBuilder.Append(',');
			}
			stringBuilder.Append(']');
		}
		static void appendAttribute(CSharpAttributeData attribute, StringBuilder stringBuilder)
		{
			appendType(attribute.AttributeClass, stringBuilder);
			MethodSymbol attributeConstructor = attribute.AttributeConstructor;
			if ((object)attributeConstructor != null)
			{
				appendAttributeSignature(attributeConstructor, stringBuilder);
			}
			if (!attribute.CommonConstructorArguments.IsEmpty || !attribute.CommonNamedArguments.IsEmpty)
			{
				stringBuilder.Append('(');
				bool flag = false;
				foreach (TypedConstant commonConstructorArgument in attribute.CommonConstructorArguments)
				{
					if (flag)
					{
						stringBuilder.Append(", ");
					}
					appendAttributeArgument(commonConstructorArgument, stringBuilder);
					flag = true;
				}
				if (!attribute.CommonNamedArguments.IsEmpty)
				{
					ArrayBuilder<string> instance2 = ArrayBuilder<string>.GetInstance(attribute.CommonNamedArguments.Length);
					foreach (KeyValuePair<string, TypedConstant> commonNamedArgument in attribute.CommonNamedArguments)
					{
						PooledStringBuilder instance3 = PooledStringBuilder.GetInstance();
						appendAttributeNamedArgument(commonNamedArgument, instance3.Builder);
						instance2.Add(instance3.ToStringAndFree());
					}
					instance2.Sort(StringComparer.Ordinal);
					foreach (string item in instance2)
					{
						if (flag)
						{
							stringBuilder.Append(", ");
						}
						stringBuilder.Append(item);
						flag = true;
					}
					instance2.Free();
				}
				stringBuilder.Append(')');
			}
		}
		static void appendAttributeArgument(TypedConstant argument, StringBuilder stringBuilder)
		{
			if (argument.Kind == TypedConstantKind.Error)
			{
				stringBuilder.Append("error");
			}
			else if (argument.IsNull)
			{
				stringBuilder.Append("null");
			}
			else
			{
				switch (argument.Kind)
				{
				case TypedConstantKind.Primitive:
					appendPrimitive(argument.Value, stringBuilder);
					break;
				case TypedConstantKind.Enum:
					appendPrimitive(argument.Value, stringBuilder);
					break;
				case TypedConstantKind.Type:
					stringBuilder.Append("typeof(");
					AppendClrType((TypeSymbol)argument.ValueInternal, ImmutableArray<CustomModifier>.Empty, stringBuilder);
					stringBuilder.Append(')');
					break;
				case TypedConstantKind.Array:
				{
					stringBuilder.Append("[");
					bool flag = false;
					foreach (TypedConstant value5 in argument.Values)
					{
						if (flag)
						{
							stringBuilder.Append(", ");
						}
						appendAttributeArgument(value5, stringBuilder);
						flag = true;
					}
					stringBuilder.Append("]");
					break;
				}
				default:
					throw ExceptionUtilities.UnexpectedValue(argument.Kind);
				}
			}
		}
		static void appendAttributeNamedArgument(KeyValuePair<string, TypedConstant> namedArgument, StringBuilder stringBuilder)
		{
			appendIdentifier(namedArgument.Key, stringBuilder);
			stringBuilder.Append(" = ");
			appendAttributeArgument(namedArgument.Value, stringBuilder);
		}
		static void appendAttributeSignature(MethodSymbol constructor, StringBuilder stringBuilder)
		{
			stringBuilder.Append("/*(");
			bool flag = false;
			foreach (ParameterSymbol parameter in constructor.Parameters)
			{
				if (flag)
				{
					stringBuilder.Append(", ");
				}
				TypeWithAnnotations typeWithAnnotations = parameter.TypeWithAnnotations;
				AppendClrType(typeWithAnnotations.Type, typeWithAnnotations.CustomModifiers, stringBuilder);
				flag = true;
			}
			stringBuilder.Append(")*/");
		}
		static void appendAttributes(ImmutableArray<CSharpAttributeData> attributes, StringBuilder stringBuilder)
		{
			if (!attributes.IsEmpty)
			{
				ArrayBuilder<string> instance2 = ArrayBuilder<string>.GetInstance(attributes.Length);
				foreach (CSharpAttributeData item2 in attributes)
				{
					if (!item2.IsConditionallyOmitted)
					{
						PooledStringBuilder instance3 = PooledStringBuilder.GetInstance();
						appendAttribute(item2, instance3.Builder);
						instance2.Add(instance3.ToStringAndFree());
					}
				}
				instance2.Sort(StringComparer.Ordinal);
				for (int i = 0; i < instance2.Count; i++)
				{
					stringBuilder.Append('[');
					stringBuilder.Append(instance2[i]);
					stringBuilder.Append("] ");
				}
				instance2.Free();
			}
		}
		static void appendCallingConventionTypes(ImmutableArray<NamedTypeSymbol> callingConventionTypes, StringBuilder stringBuilder)
		{
			if (!callingConventionTypes.IsEmpty)
			{
				stringBuilder.Append('[');
				for (int i = 0; i < callingConventionTypes.Length; i++)
				{
					if (i > 0)
					{
						stringBuilder.Append(", ");
					}
					string name2 = callingConventionTypes[i].Name;
					int length = "CallConv".Length;
					stringBuilder.Append(name2.Substring(length, name2.Length - length));
				}
				stringBuilder.Append(']');
			}
		}
		static void appendContainingType(NamedTypeSymbol namedType, StringBuilder stringBuilder)
		{
			NamedTypeSymbol containingType = namedType.ContainingType;
			if ((object)containingType != null)
			{
				appendContainingType(containingType, stringBuilder);
				stringBuilder.Append(containingType.Name);
				appendTypeArguments(containingType, stringBuilder);
				stringBuilder.Append('.');
			}
		}
		static void appendFunctionPointerType(FunctionPointerTypeSymbol functionPointer, StringBuilder stringBuilder)
		{
			stringBuilder.Append("delegate*");
			FunctionPointerMethodSymbol signature = functionPointer.Signature;
			stringBuilder.Append(signature.CallingConvention switch
			{
				Microsoft.Cci.CallingConvention.Default => null, 
				Microsoft.Cci.CallingConvention.Unmanaged => " unmanaged", 
				Microsoft.Cci.CallingConvention.CDecl => " unmanaged[CDecl]", 
				Microsoft.Cci.CallingConvention.Standard => " unmanaged[Stdcall]", 
				Microsoft.Cci.CallingConvention.ThisCall => " unmanaged[Thiscall]", 
				Microsoft.Cci.CallingConvention.FastCall => " unmanaged[Fastcall]", 
				_ => throw ExceptionUtilities.UnexpectedValue(signature.CallingConvention), 
			});
			appendCallingConventionTypes(signature.UnmanagedCallingConventionTypes, stringBuilder);
			bool flag = false;
			stringBuilder.Append('<');
			ImmutableArray<ParameterSymbol> parameters = signature.Parameters;
			for (int i = 0; i < parameters.Length; i++)
			{
				if (flag)
				{
					stringBuilder.Append(", ");
				}
				ParameterSymbol parameterSymbol = parameters[i];
				appendRefKind(parameterSymbol.RefKind, stringBuilder, forParameter: true);
				appendTypeWithAnnotation(parameterSymbol.TypeWithAnnotations, stringBuilder);
				flag = true;
			}
			if (flag)
			{
				stringBuilder.Append(", ");
			}
			appendRefKind(signature.RefKind, stringBuilder);
			appendTypeWithAnnotation(signature.ReturnTypeWithAnnotations.WithModifiers(ImmutableArray<CustomModifier>.Empty), stringBuilder);
			stringBuilder.Append('>');
		}
		static void appendIdentifier(string text, StringBuilder stringBuilder, bool forTypeConstraint = false)
		{
			bool flag = SyntaxFacts.GetKeywordKind(text) != SyntaxKind.None || SyntaxFacts.GetContextualKeywordKind(text) != SyntaxKind.None;
			if (!flag)
			{
				bool flag2 = ((text == "dynamic" || text == "notnull") ? true : false);
				flag = flag2;
			}
			if (flag)
			{
				stringBuilder.Append('@');
			}
			stringBuilder.Append(text);
		}
		static void appendNamedType(NamedTypeSymbol namedType, StringBuilder stringBuilder)
		{
			if (namedType.SpecialType == SpecialType.System_Void)
			{
				stringBuilder.Append("void");
			}
			else if (namedType.IsTupleType)
			{
				stringBuilder.Append('(');
				ImmutableArray<string> tupleElementNames = namedType.TupleElementNames;
				ImmutableArray<TypeWithAnnotations> tupleElementTypesWithAnnotations = namedType.TupleElementTypesWithAnnotations;
				for (int i = 0; i < tupleElementTypesWithAnnotations.Length; i++)
				{
					TypeWithAnnotations type = tupleElementTypesWithAnnotations[i];
					if (i > 0)
					{
						stringBuilder.Append(", ");
					}
					appendTypeWithAnnotation(type, stringBuilder);
					if (!tupleElementNames.IsDefault)
					{
						string text = tupleElementNames[i];
						if (text != null)
						{
							stringBuilder.Append(" ");
							appendIdentifier(text, stringBuilder);
						}
					}
				}
				stringBuilder.Append(')');
			}
			else
			{
				appendNamespace(namedType.ContainingNamespace, stringBuilder);
				appendContainingType(namedType, stringBuilder);
				appendIdentifier(namedType.Name, stringBuilder);
				appendTypeArguments(namedType, stringBuilder);
			}
		}
		static void appendNamespace(NamespaceSymbol ns, StringBuilder stringBuilder)
		{
			if ((object)ns != null && !ns.IsGlobalNamespace)
			{
				appendNamespace(ns.ContainingNamespace, stringBuilder);
				stringBuilder.Append(ns.Name);
				stringBuilder.Append('.');
			}
		}
		static void appendPrimitive(object? value, StringBuilder stringBuilder)
		{
			if (!(value is bool flag))
			{
				if (!(value is sbyte b))
				{
					if (!(value is short num))
					{
						if (!(value is int num2))
						{
							if (!(value is long num3))
							{
								if (!(value is byte b2))
								{
									if (!(value is ushort num4))
									{
										if (!(value is uint num5))
										{
											if (!(value is ulong num6))
											{
												if (!(value is float value2))
												{
													if (!(value is double value3))
													{
														if (!(value is char c))
														{
															if (value is string value4)
															{
																stringBuilder.Append(ObjectDisplay.FormatLiteral(value4, ObjectDisplayOptions.UseQuotes | ObjectDisplayOptions.EscapeNonPrintableCharacters));
															}
														}
														else
														{
															stringBuilder.Append(ObjectDisplay.FormatLiteral(c, ObjectDisplayOptions.UseQuotes | ObjectDisplayOptions.EscapeNonPrintableCharacters));
														}
													}
													else
													{
														stringBuilder.Append(BitConverter.DoubleToInt64Bits(value3).ToString(CultureInfo.InvariantCulture));
													}
												}
												else
												{
													stringBuilder.Append(BitConverter.ToInt32(BitConverter.GetBytes(value2), 0).ToString(CultureInfo.InvariantCulture));
												}
											}
											else
											{
												stringBuilder.Append(num6.ToString(CultureInfo.InvariantCulture));
											}
										}
										else
										{
											stringBuilder.Append(num5.ToString(CultureInfo.InvariantCulture));
										}
									}
									else
									{
										stringBuilder.Append(num4.ToString(CultureInfo.InvariantCulture));
									}
								}
								else
								{
									stringBuilder.Append(b2.ToString(CultureInfo.InvariantCulture));
								}
							}
							else
							{
								stringBuilder.Append(num3.ToString(CultureInfo.InvariantCulture));
							}
						}
						else
						{
							stringBuilder.Append(num2.ToString(CultureInfo.InvariantCulture));
						}
					}
					else
					{
						stringBuilder.Append(num.ToString(CultureInfo.InvariantCulture));
					}
				}
				else
				{
					stringBuilder.Append(b.ToString(CultureInfo.InvariantCulture));
				}
			}
			else
			{
				stringBuilder.Append(flag ? "true" : "false");
			}
		}
		static void appendRefKind(RefKind refKind, StringBuilder stringBuilder, bool forParameter = false)
		{
			stringBuilder.Append(refKind switch
			{
				RefKind.None => "", 
				RefKind.Ref => "ref ", 
				RefKind.Out => "out ", 
				RefKind.In => (!forParameter) ? "ref readonly " : "in ", 
				RefKind.RefReadOnlyParameter => "ref readonly ", 
				_ => throw ExceptionUtilities.UnexpectedValue(refKind), 
			});
		}
		static void appendType(TypeSymbol type, StringBuilder stringBuilder)
		{
			if (type is NamedTypeSymbol namedType)
			{
				appendNamedType(namedType, stringBuilder);
			}
			else if (type is TypeParameterSymbol)
			{
				string name2 = type.Name;
				appendIdentifier(name2, stringBuilder, forTypeConstraint: true);
			}
			else if (type is ArrayTypeSymbol array)
			{
				appendArrayType(array, stringBuilder);
			}
			else if (type is PointerTypeSymbol pointerTypeSymbol)
			{
				appendTypeWithAnnotation(pointerTypeSymbol.PointedAtTypeWithAnnotations, stringBuilder);
				stringBuilder.Append('*');
			}
			else if (type is FunctionPointerTypeSymbol functionPointer)
			{
				appendFunctionPointerType(functionPointer, stringBuilder);
			}
			else
			{
				if (!(type is DynamicTypeSymbol))
				{
					throw ExceptionUtilities.UnexpectedValue(type);
				}
				stringBuilder.Append("dynamic");
			}
		}
		static void appendTypeArguments(NamedTypeSymbol namedType, StringBuilder stringBuilder)
		{
			ImmutableArray<TypeWithAnnotations> typeArgumentsWithAnnotationsNoUseSiteDiagnostics = namedType.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics;
			if (typeArgumentsWithAnnotationsNoUseSiteDiagnostics.Length > 0)
			{
				stringBuilder.Append('<');
				for (int i = 0; i < typeArgumentsWithAnnotationsNoUseSiteDiagnostics.Length; i++)
				{
					if (i > 0)
					{
						stringBuilder.Append(", ");
					}
					appendTypeWithAnnotation(typeArgumentsWithAnnotationsNoUseSiteDiagnostics[i], stringBuilder);
				}
				stringBuilder.Append('>');
			}
		}
		static void appendTypeConstraints(TypeParameterSymbol typeParam, StringBuilder stringBuilder, ref bool needComma)
		{
			ImmutableArray<TypeWithAnnotations> constraintTypesNoUseSiteDiagnostics = typeParam.ConstraintTypesNoUseSiteDiagnostics;
			ArrayBuilder<string> instance2 = ArrayBuilder<string>.GetInstance(constraintTypesNoUseSiteDiagnostics.Length);
			for (int i = 0; i < constraintTypesNoUseSiteDiagnostics.Length; i++)
			{
				PooledStringBuilder instance3 = PooledStringBuilder.GetInstance();
				appendTypeWithAnnotation(constraintTypesNoUseSiteDiagnostics[i], instance3.Builder);
				instance2.Add(instance3.ToStringAndFree());
			}
			instance2.Sort(StringComparer.Ordinal);
			foreach (string item3 in instance2)
			{
				if (needComma)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(item3);
				needComma = true;
			}
			instance2.Free();
		}
		static void appendTypeParameterConstraints(TypeParameterSymbol typeParam, StringBuilder stringBuilder)
		{
			stringBuilder.Append(" where ");
			appendIdentifier(typeParam.Name, stringBuilder);
			stringBuilder.Append(" : ");
			bool needComma = false;
			if (typeParam.HasReferenceTypeConstraint)
			{
				stringBuilder.Append("class");
				bool? referenceTypeConstraintIsNullable = typeParam.ReferenceTypeConstraintIsNullable;
				if (referenceTypeConstraintIsNullable.HasValue)
				{
					if (referenceTypeConstraintIsNullable == true)
					{
						stringBuilder.Append('?');
					}
					else
					{
						stringBuilder.Append('!');
					}
				}
				needComma = true;
			}
			else if (typeParam.HasUnmanagedTypeConstraint)
			{
				stringBuilder.Append("unmanaged");
				needComma = true;
			}
			else if (typeParam.HasValueTypeConstraint)
			{
				stringBuilder.Append("struct");
				needComma = true;
			}
			else if (typeParam.HasNotNullConstraint)
			{
				stringBuilder.Append("notnull");
				needComma = true;
			}
			if (typeParam.ConstraintTypesNoUseSiteDiagnostics.Length > 0)
			{
				appendTypeConstraints(typeParam, stringBuilder, ref needComma);
			}
			if (typeParam.HasConstructorConstraint)
			{
				if (needComma)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append("new()");
				needComma = true;
			}
			if (typeParam.AllowsRefLikeType)
			{
				if (needComma)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append("allows ref struct");
			}
		}
		static void appendTypeWithAnnotation(TypeWithAnnotations type, StringBuilder builder2)
		{
			appendType(type.Type, builder2);
			if (!type.Type.IsValueType)
			{
				appendAnnotation(builder2, type.NullableAnnotation);
			}
		}
		static bool typeParameterHasConstraints(TypeParameterSymbol typeParameter)
		{
			if (typeParameter.ConstraintTypesNoUseSiteDiagnostics.IsEmpty && !typeParameter.HasConstructorConstraint && !typeParameter.HasReferenceTypeConstraint && !typeParameter.HasValueTypeConstraint && !typeParameter.HasNotNullConstraint)
			{
				return typeParameter.AllowsRefLikeType;
			}
			return true;
		}
	}

	public sealed override MethodSymbol? TryGetCorrespondingExtensionImplementationMethod(MethodSymbol method)
	{
		NamedTypeSymbol containingType = ContainingType;
		if ((object)containingType == null)
		{
			return null;
		}
		if (_lazyExtensionInfo == null)
		{
			Interlocked.CompareExchange(ref _lazyExtensionInfo, new ExtensionInfo(), null);
		}
		if (_lazyExtensionInfo.LazyImplementationMap == null)
		{
			ImmutableDictionary<MethodSymbol, MethodSymbol>.Builder builder = ImmutableDictionary.CreateBuilder<MethodSymbol, MethodSymbol>(ReferenceEqualityComparer.Instance);
			builder.AddRange(from m in containingType.GetMembersUnordered().OfType<SourceExtensionImplementationMethodSymbol>()
				select new KeyValuePair<MethodSymbol, MethodSymbol>(m.UnderlyingMethod, m));
			Interlocked.CompareExchange(ref _lazyExtensionInfo.LazyImplementationMap, builder.ToImmutable(), null);
		}
		return _lazyExtensionInfo.LazyImplementationMap.GetValueOrDefault(method);
	}

	[MemberNotNull("_lazyExtensionInfo")]
	internal MethodSymbol? TryGetOrCreateExtensionMarker()
	{
		if (_lazyExtensionInfo == null)
		{
			Interlocked.CompareExchange(ref _lazyExtensionInfo, new ExtensionInfo(), null);
		}
		if ((object)_lazyExtensionInfo.LazyExtensionMarker == ErrorMethodSymbol.UnknownMethod)
		{
			Interlocked.CompareExchange(ref _lazyExtensionInfo.LazyExtensionMarker, tryCreateExtensionMarker(), ErrorMethodSymbol.UnknownMethod);
		}
		return _lazyExtensionInfo.LazyExtensionMarker;
		MethodSymbol? tryCreateExtensionMarker()
		{
			ParameterListSyntax parameterList = ((ExtensionBlockDeclarationSyntax)this.GetNonNullSyntaxNode()).ParameterList;
			if (parameterList == null)
			{
				return null;
			}
			_ = parameterList.Parameters.Count;
			return new SynthesizedExtensionMarker(this, parameterList);
		}
	}

	private static string RawNameToHashString(string rawName)
	{
		Span<byte> span = stackalloc byte[16];
		ReadOnlySpan<char> span2 = System.MemoryExtensions.AsSpan(rawName);
		if (!BitConverter.IsLittleEndian)
		{
			Span<short> span3 = stackalloc short[span2.Length];
			MemoryMarshal.Cast<char, short>(span2).CopyTo(span3);
			SourceText.ReverseEndianness(span3);
			XxHash128.Hash(MemoryMarshal.AsBytes(span3), span, 0L);
		}
		else
		{
			XxHash128.Hash(MemoryMarshal.AsBytes(span2), span, 0L);
		}
		return PrivateImplementationDetails.HashToHex(span);
	}

	internal static Symbol? ReduceExtensionMember(CSharpCompilation? compilation, Symbol extensionMember, TypeSymbol receiverType, out bool wasExtensionFullyInferred)
	{
		NamedTypeSymbol containingType = extensionMember.ContainingType;
		if ((object)containingType.ExtensionParameter == null)
		{
			wasExtensionFullyInferred = false;
			return null;
		}
		Symbol symbol;
		if (extensionMember.IsDefinition)
		{
			NamedTypeSymbol namedTypeSymbol = inferExtensionTypeArguments(containingType, receiverType, compilation, out wasExtensionFullyInferred);
			if ((object)namedTypeSymbol == null)
			{
				return null;
			}
			symbol = extensionMember.SymbolAsMember(namedTypeSymbol);
		}
		else
		{
			wasExtensionFullyInferred = true;
			symbol = extensionMember;
		}
		object obj = ((object)compilation?.Conversions) ?? ((object)extensionMember.ContainingAssembly.CorLibrary.TypeConversions);
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
		if (!((ConversionsBase)obj).ConvertExtensionMethodThisArg(symbol.ContainingType.ExtensionParameter.Type, receiverType, ref useSiteInfo, false).Exists)
		{
			return null;
		}
		return symbol;
		static ImmutableArray<TypeWithAnnotations> fillNotInferredTypeArguments(NamedTypeSymbol extension, ImmutableArray<TypeWithAnnotations> typeArgs, out bool wasFullyInferred)
		{
			wasFullyInferred = typeArgs.All((TypeWithAnnotations t) => t.HasType);
			if (!wasFullyInferred)
			{
				return typeArgs.ZipAsArray(extension.TypeParameters, (TypeWithAnnotations t, TypeParameterSymbol tp) => (!t.HasType) ? TypeWithAnnotations.Create(tp) : t);
			}
			return typeArgs;
		}
		static NamedTypeSymbol? inferExtensionTypeArguments(NamedTypeSymbol extension, TypeSymbol type, CSharpCompilation? cSharpCompilation, out bool reference)
		{
			if (extension.Arity == 0)
			{
				reference = true;
				return extension;
			}
			TypeConversions typeConversions = extension.ContainingAssembly.CorLibrary.TypeConversions;
			BoundLiteral receiver = new BoundLiteral((CSharpSyntaxNode)CSharpSyntaxTree.Dummy.GetRoot(), ConstantValue.Bad, type)
			{
				WasCompilerGenerated = true
			};
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2 = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
			ImmutableArray<TypeWithAnnotations> typeArgs = MethodTypeInferrer.InferTypeArgumentsFromReceiverType(extension, receiver, cSharpCompilation, typeConversions, ref useSiteInfo2);
			if (typeArgs.IsDefault)
			{
				reference = false;
				return null;
			}
			ImmutableArray<TypeWithAnnotations> typeArguments = fillNotInferredTypeArguments(extension, typeArgs, out reference);
			NamedTypeSymbol namedTypeSymbol2 = extension.Construct(typeArguments);
			if (!namedTypeSymbol2.CheckConstraints(new ConstraintsHelper.CheckConstraintsArgs(cSharpCompilation, typeConversions, includeNullability: false, NoLocation.Singleton, BindingDiagnosticBag.Discarded, CompoundUseSiteInfo<AssemblySymbol>.Discarded)))
			{
				return null;
			}
			return namedTypeSymbol2;
		}
	}
}
