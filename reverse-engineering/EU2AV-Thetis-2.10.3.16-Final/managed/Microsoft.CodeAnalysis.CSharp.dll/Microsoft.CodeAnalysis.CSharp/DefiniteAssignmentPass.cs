using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal class DefiniteAssignmentPass : LocalDataFlowPass<DefiniteAssignmentPass.LocalState, DefiniteAssignmentPass.LocalFunctionState>
{
	internal struct LocalState : ILocalDataFlowState, ILocalState
	{
		internal BitVector Assigned;

		public bool NormalizeToBottom { get; }

		public bool Reachable
		{
			get
			{
				if (Assigned.Capacity > 0)
				{
					return !IsAssigned(0);
				}
				return true;
			}
		}

		internal LocalState(BitVector assigned, bool normalizeToBottom = false)
		{
			Assigned = assigned;
			NormalizeToBottom = normalizeToBottom;
		}

		public LocalState Clone()
		{
			return new LocalState(Assigned.Clone());
		}

		public bool IsAssigned(int slot)
		{
			return Assigned[slot];
		}

		public void Assign(int slot)
		{
			if (slot != -1)
			{
				Assigned[slot] = true;
			}
		}

		public void Unassign(int slot)
		{
			if (slot != -1)
			{
				Assigned[slot] = false;
			}
		}
	}

	internal sealed class LocalFunctionState(LocalState stateFromBottom, LocalState stateFromTop) : AbstractLocalFunctionState(stateFromBottom, stateFromTop)
	{
		public BitVector ReadVars = BitVector.Empty;

		public BitVector CapturedMask = BitVector.Null;

		public BitVector InvertedCapturedMask = BitVector.Null;
	}

	private readonly PooledDictionary<VariableIdentifier, int> _variableSlot = PooledDictionary<LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier, int>.GetInstance();

	protected readonly ArrayBuilder<VariableIdentifier> variableBySlot = ArrayBuilder<LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier>.GetInstance(1, default(LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier));

	private readonly HashSet<Symbol>? initiallyAssignedVariables;

	private readonly PooledHashSet<LocalSymbol> _usedVariables = PooledHashSet<LocalSymbol>.GetInstance();

	private PooledHashSet<ParameterSymbol>? _readParameters;

	private readonly PooledHashSet<LocalFunctionSymbol> _usedLocalFunctions = PooledHashSet<LocalFunctionSymbol>.GetInstance();

	private readonly PooledHashSet<Symbol> _writtenVariables = PooledHashSet<Symbol>.GetInstance();

	private PooledHashSet<FieldSymbol>? _implicitlyInitializedFieldsOpt;

	private readonly PooledDictionary<Symbol, Location> _unsafeAddressTakenVariables = PooledDictionary<Symbol, Location>.GetInstance();

	private readonly PooledHashSet<Symbol> _capturedVariables = PooledHashSet<Symbol>.GetInstance();

	private readonly PooledHashSet<Symbol> _capturedInside = PooledHashSet<Symbol>.GetInstance();

	private readonly PooledHashSet<Symbol> _capturedOutside = PooledHashSet<Symbol>.GetInstance();

	private readonly SourceAssemblySymbol? _sourceAssembly;

	private readonly HashSet<PrefixUnaryExpressionSyntax>? _unassignedVariableAddressOfSyntaxes;

	private BitVector _alreadyReported;

	private readonly bool _requireOutParamsAssigned;

	private readonly bool _trackClassFields;

	private readonly bool _trackStaticMembers;

	protected MethodSymbol? topLevelMethod;

	protected bool _convertInsufficientExecutionStackExceptionToCancelledByStackGuardException;

	private readonly bool _shouldCheckConverted;

	private bool TrackImplicitlyInitializedFields
	{
		get
		{
			if (_requireOutParamsAssigned && !_emptyStructTypeCache._dev12CompilerCompatibility)
			{
				Symbol currentSymbol = CurrentSymbol;
				if (currentSymbol is MethodSymbol { MethodKind: MethodKind.Constructor })
				{
					NamedTypeSymbol containingType = currentSymbol.ContainingType;
					if ((object)containingType != null)
					{
						return containingType.TypeKind == TypeKind.Struct;
					}
				}
				return false;
			}
			return false;
		}
	}

	public sealed override bool AwaitUsingAndForeachAddsPendingBranch => true;

	private void AddImplicitlyInitializedField(FieldSymbol field)
	{
		if (TrackImplicitlyInitializedFields)
		{
			(_implicitlyInitializedFieldsOpt ?? (_implicitlyInitializedFieldsOpt = PooledHashSet<FieldSymbol>.GetInstance())).Add(field);
		}
	}

	internal DefiniteAssignmentPass(CSharpCompilation compilation, Symbol member, BoundNode node, bool strictAnalysis, bool trackUnassignments = false, HashSet<PrefixUnaryExpressionSyntax>? unassignedVariableAddressOfSyntaxes = null, bool requireOutParamsAssigned = true, bool trackClassFields = false, bool trackStaticMembers = false)
		: base(compilation, member, node, strictAnalysis ? EmptyStructTypeCache.CreatePrecise() : EmptyStructTypeCache.CreateForDev12Compatibility(compilation), trackUnassignments)
	{
		initiallyAssignedVariables = null;
		_sourceAssembly = GetSourceAssembly(compilation, member, node);
		_unassignedVariableAddressOfSyntaxes = unassignedVariableAddressOfSyntaxes;
		_requireOutParamsAssigned = requireOutParamsAssigned;
		_trackClassFields = trackClassFields;
		_trackStaticMembers = trackStaticMembers;
		topLevelMethod = member as MethodSymbol;
		_shouldCheckConverted = GetType() == typeof(DefiniteAssignmentPass);
		State = new LocalState(BitVector.Empty);
	}

	internal DefiniteAssignmentPass(CSharpCompilation compilation, Symbol member, BoundNode node, EmptyStructTypeCache emptyStructs, bool trackUnassignments = false, HashSet<Symbol>? initiallyAssignedVariables = null)
		: base(compilation, member, node, emptyStructs, trackUnassignments)
	{
		this.initiallyAssignedVariables = initiallyAssignedVariables;
		_sourceAssembly = GetSourceAssembly(compilation, member, node);
		CurrentSymbol = member;
		_unassignedVariableAddressOfSyntaxes = null;
		_requireOutParamsAssigned = true;
		topLevelMethod = member as MethodSymbol;
		_shouldCheckConverted = GetType() == typeof(DefiniteAssignmentPass);
		State = new LocalState(BitVector.Empty);
	}

	internal DefiniteAssignmentPass(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion, HashSet<Symbol> initiallyAssignedVariables, HashSet<PrefixUnaryExpressionSyntax> unassignedVariableAddressOfSyntaxes, bool trackUnassignments)
		: base(compilation, member, node, EmptyStructTypeCache.CreateNeverEmpty(), firstInRegion, lastInRegion, true, trackUnassignments)
	{
		this.initiallyAssignedVariables = initiallyAssignedVariables;
		_sourceAssembly = null;
		CurrentSymbol = member;
		_unassignedVariableAddressOfSyntaxes = unassignedVariableAddressOfSyntaxes;
		_shouldCheckConverted = GetType() == typeof(DefiniteAssignmentPass);
		State = new LocalState(BitVector.Empty);
	}

	private static SourceAssemblySymbol? GetSourceAssembly(CSharpCompilation compilation, Symbol member, BoundNode node)
	{
		if ((object)member == null)
		{
			return null;
		}
		if (node.Kind == BoundKind.Attribute)
		{
			return null;
		}
		return member.ContainingAssembly as SourceAssemblySymbol;
	}

	protected override void Free()
	{
		variableBySlot.Free();
		_variableSlot.Free();
		_usedVariables.Free();
		_readParameters?.Free();
		_implicitlyInitializedFieldsOpt?.Free();
		_usedLocalFunctions.Free();
		_writtenVariables.Free();
		_capturedVariables.Free();
		_capturedInside.Free();
		_capturedOutside.Free();
		_unsafeAddressTakenVariables.Free();
		base.Free();
	}

	protected override bool TryGetVariable(VariableIdentifier identifier, out int slot)
	{
		return _variableSlot.TryGetValue(identifier, out slot);
	}

	protected override int AddVariable(VariableIdentifier identifier)
	{
		int count = variableBySlot.Count;
		_variableSlot.Add(identifier, count);
		variableBySlot.Add(identifier);
		return count;
	}

	protected Symbol GetNonMemberSymbol(int slot)
	{
		LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier variableIdentifier = variableBySlot[slot];
		while (variableIdentifier.ContainingSlot > 0)
		{
			variableIdentifier = variableBySlot[variableIdentifier.ContainingSlot];
		}
		return variableIdentifier.Symbol;
	}

	private int RootSlot(int slot)
	{
		while (true)
		{
			int containingSlot = variableBySlot[slot].ContainingSlot;
			if (containingSlot == 0)
			{
				break;
			}
			slot = containingSlot;
		}
		return slot;
	}

	protected override bool ConvertInsufficientExecutionStackExceptionToCancelledByStackGuardException()
	{
		return _convertInsufficientExecutionStackExceptionToCancelledByStackGuardException;
	}

	protected override ImmutableArray<PendingBranch> Scan(ref bool badRegion)
	{
		base.Diagnostics.Clear();
		ImmutableArray<ParameterSymbol> methodParameters = base.MethodParameters;
		ParameterSymbol methodThisParameter = base.MethodThisParameter;
		_alreadyReported = BitVector.Empty;
		regionPlace = RegionPlace.Before;
		EnterParameters(methodParameters);
		Symbol symbol = _symbol;
		if (symbol is MethodSymbol methodSymbol)
		{
			if (!symbol.IsStatic && symbol.ContainingSymbol is SourceMemberContainerTypeSymbol sourceMemberContainerTypeSymbol)
			{
				SynthesizedPrimaryConstructor primaryConstructor = sourceMemberContainerTypeSymbol.PrimaryConstructor;
				if ((object)primaryConstructor != null && !(methodSymbol is SynthesizedPrimaryConstructor))
				{
					Symbol currentSymbol = CurrentSymbol;
					CurrentSymbol = primaryConstructor;
					foreach (ParameterSymbol parameter in primaryConstructor.Parameters)
					{
						NoteWrite(parameter, null, read: true, parameter.RefKind != RefKind.None);
					}
					CurrentSymbol = currentSymbol;
				}
			}
		}
		else if ((symbol is FieldSymbol || symbol is PropertySymbol) && !symbol.IsStatic && symbol.ContainingSymbol is SourceMemberContainerTypeSymbol sourceMemberContainerTypeSymbol2)
		{
			SynthesizedPrimaryConstructor primaryConstructor = sourceMemberContainerTypeSymbol2.PrimaryConstructor;
			if ((object)primaryConstructor != null)
			{
				SynthesizedPrimaryConstructor synthesizedPrimaryConstructor = primaryConstructor;
				EnterParameters(synthesizedPrimaryConstructor.Parameters);
			}
		}
		if ((object)methodThisParameter != null)
		{
			EnterParameter(methodThisParameter);
			if (methodThisParameter.Type.SpecialType.CanOptimizeBehavior())
			{
				int orCreateSlot = GetOrCreateSlot(methodThisParameter);
				SetSlotState(orCreateSlot, assigned: true);
			}
		}
		ParameterSymbol extensionParameter = null;
		if (_symbol.TryGetInstanceExtensionParameter(out extensionParameter))
		{
			EnterParameter(extensionParameter);
		}
		ImmutableArray<AbstractFlowPass<LocalState, LocalFunctionState>.PendingBranch> result = base.Scan(ref badRegion);
		if (ShouldAnalyzeOutParameters(out var location))
		{
			LeaveParameters(methodParameters, null, location);
			if ((object)methodThisParameter != null)
			{
				LeaveParameter(methodThisParameter, null, location);
			}
			if ((object)extensionParameter != null)
			{
				LeaveParameter(extensionParameter, null, location);
			}
			LocalState self = State;
			foreach (AbstractFlowPass<LocalState, LocalFunctionState>.PendingBranch item in result)
			{
				State = item.State;
				LeaveParameters(methodParameters, item.Branch.Syntax, null);
				if ((object)methodThisParameter != null)
				{
					LeaveParameter(methodThisParameter, item.Branch.Syntax, null);
				}
				if ((object)extensionParameter != null)
				{
					LeaveParameter(extensionParameter, item.Branch.Syntax, null);
				}
				Join(ref self, ref State);
			}
			State = self;
		}
		return result;
	}

	protected virtual void ReportUnassignedOutParameter(ParameterSymbol parameter, SyntaxNode node, Location location)
	{
		if ((!_requireOutParamsAssigned && (object)topLevelMethod == CurrentSymbol) || base.Diagnostics == null || !State.Reachable)
		{
			return;
		}
		if (location == null)
		{
			location = new SourceLocation(node);
		}
		bool flag = false;
		if (parameter.IsThis)
		{
			int num = VariableSlot(parameter);
			if (!State.IsAssigned(num))
			{
				TypeSymbol type = parameter.Type;
				foreach (FieldSymbol structInstanceField in _emptyStructTypeCache.GetStructInstanceFields(type))
				{
					if (_emptyStructTypeCache.IsEmptyStructType(structInstanceField.Type) || LocalDataFlowPass<LocalState, LocalFunctionState>.HasInitializer(structInstanceField))
					{
						continue;
					}
					int num2 = VariableSlot(structInstanceField, num);
					if (num2 == -1 || !State.IsAssigned(num2))
					{
						Symbol associatedSymbol = structInstanceField.AssociatedSymbol;
						bool flag2 = (object)associatedSymbol != null && associatedSymbol.Kind == SymbolKind.Property;
						if (compilation.IsFeatureEnabled(MessageID.IDS_FeatureAutoDefaultStructs))
						{
							base.Diagnostics.Add(flag2 ? ErrorCode.WRN_UnassignedThisAutoPropertySupportedVersion : ErrorCode.WRN_UnassignedThisSupportedVersion, location, flag2 ? associatedSymbol : structInstanceField);
						}
						else
						{
							base.Diagnostics.Add(flag2 ? ErrorCode.ERR_UnassignedThisAutoPropertyUnsupportedVersion : ErrorCode.ERR_UnassignedThisUnsupportedVersion, location, flag2 ? associatedSymbol : structInstanceField, new CSharpRequiredLanguageVersion(MessageID.IDS_FeatureAutoDefaultStructs.RequiredVersion()));
						}
						AddImplicitlyInitializedField(structInstanceField);
						flag = true;
					}
				}
				if (!flag)
				{
					if (type.HasInlineArrayAttribute(out var length) && length > 1)
					{
						FieldSymbol fieldSymbol = type.TryGetPossiblyUnsupportedByLanguageInlineArrayElementField();
						if ((object)fieldSymbol != null)
						{
							if (!compilation.IsFeatureEnabled(MessageID.IDS_FeatureAutoDefaultStructs))
							{
								base.Diagnostics.Add(ErrorCode.ERR_ParamUnassigned, location, parameter.Name);
							}
							AddImplicitlyInitializedField(fieldSymbol);
						}
					}
					flag = true;
				}
			}
		}
		if (!flag)
		{
			base.Diagnostics.Add(ErrorCode.ERR_ParamUnassigned, location, parameter.Name);
		}
	}

	public static void Analyze(CSharpCompilation compilation, MethodSymbol member, BoundNode node, DiagnosticBag diagnostics, out ImmutableArray<FieldSymbol> implicitlyInitializedFieldsOpt, bool requireOutParamsAssigned)
	{
		DiagnosticBag diagnosticBag;
		(diagnosticBag, implicitlyInitializedFieldsOpt) = analyze(strictAnalysis: true);
		if (!diagnosticBag.HasAnyErrors())
		{
			diagnostics.AddRangeAndFree(diagnosticBag);
			return;
		}
		DiagnosticBag item = analyze(strictAnalysis: false).Item1;
		if (item.AsEnumerable().Any((Diagnostic d) => d.Code == 8078))
		{
			diagnostics.AddRangeAndFree(item);
			diagnosticBag.Free();
			return;
		}
		if (diagnosticBag.Count == item.Count)
		{
			diagnostics.AddRangeAndFree(diagnosticBag);
			item.Free();
			return;
		}
		HashSet<Diagnostic> hashSet = new HashSet<Diagnostic>(item.AsEnumerable(), SameDiagnosticComparer.Instance);
		item.Free();
		foreach (Diagnostic item3 in diagnosticBag.AsEnumerable())
		{
			if (item3.Severity != DiagnosticSeverity.Error || hashSet.Contains(item3))
			{
				diagnostics.Add(item3);
				continue;
			}
			ErrorCode code = (ErrorCode)item3.Code;
			ErrorCode code2 = code switch
			{
				ErrorCode.ERR_UnassignedThisAutoPropertyUnsupportedVersion => ErrorCode.WRN_UnassignedThisAutoPropertyUnsupportedVersion, 
				ErrorCode.ERR_UnassignedThisUnsupportedVersion => ErrorCode.WRN_UnassignedThisUnsupportedVersion, 
				ErrorCode.ERR_ParamUnassigned => ErrorCode.WRN_ParamUnassigned, 
				ErrorCode.ERR_UseDefViolationProperty => ErrorCode.WRN_UseDefViolationProperty, 
				ErrorCode.ERR_UseDefViolationField => ErrorCode.WRN_UseDefViolationField, 
				ErrorCode.ERR_UseDefViolationThisUnsupportedVersion => ErrorCode.WRN_UseDefViolationThisUnsupportedVersion, 
				ErrorCode.ERR_UseDefViolationPropertyUnsupportedVersion => ErrorCode.WRN_UseDefViolationPropertyUnsupportedVersion, 
				ErrorCode.ERR_UseDefViolationFieldUnsupportedVersion => ErrorCode.WRN_UseDefViolationFieldUnsupportedVersion, 
				ErrorCode.ERR_UseDefViolationOut => ErrorCode.WRN_UseDefViolationOut, 
				ErrorCode.ERR_UseDefViolation => ErrorCode.WRN_UseDefViolation, 
				_ => code, 
			};
			object?[] array;
			if (item3 is DiagnosticWithInfo diagnosticWithInfo)
			{
				DiagnosticInfo info = diagnosticWithInfo.Info;
				if (info != null)
				{
					object[] arguments = info.Arguments;
					array = arguments;
					goto IL_024f;
				}
			}
			array = item3.Arguments.ToArray();
			goto IL_024f;
			IL_024f:
			object[] args = array;
			diagnostics.Add(code2, item3.Location, args);
		}
		diagnosticBag.Free();
		(DiagnosticBag, ImmutableArray<FieldSymbol> implicitlyInitializedFieldsOpt) analyze(bool strictAnalysis)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			ImmutableArray<FieldSymbol> item2 = default(ImmutableArray<FieldSymbol>);
			DefiniteAssignmentPass definiteAssignmentPass = new DefiniteAssignmentPass(compilation, member, node, strictAnalysis, trackUnassignments: false, null, requireOutParamsAssigned)
			{
				_convertInsufficientExecutionStackExceptionToCancelledByStackGuardException = true
			};
			try
			{
				bool badRegion = false;
				definiteAssignmentPass.Analyze(ref badRegion, instance);
				PooledHashSet<FieldSymbol> implicitlyInitializedFieldsOpt2 = definiteAssignmentPass._implicitlyInitializedFieldsOpt;
				if (implicitlyInitializedFieldsOpt2 != null)
				{
					ArrayBuilder<FieldSymbol> instance2 = ArrayBuilder<FieldSymbol>.GetInstance(implicitlyInitializedFieldsOpt2.Count);
					foreach (FieldSymbol item4 in implicitlyInitializedFieldsOpt2)
					{
						instance2.Add(item4);
					}
					instance2.Sort(LexicalOrderSymbolComparer.Instance);
					item2 = instance2.ToImmutableAndFree();
				}
			}
			catch (CancelledByStackGuardException ex) when (diagnostics != null)
			{
				ex.AddAnError(instance);
			}
			finally
			{
				definiteAssignmentPass.Free();
			}
			return (instance, implicitlyInitializedFieldsOpt: item2);
		}
	}

	protected void Analyze(ref bool badRegion, DiagnosticBag diagnostics)
	{
		Analyze(ref badRegion);
		if (diagnostics == null)
		{
			return;
		}
		foreach (Symbol capturedVariable in _capturedVariables)
		{
			if (_unsafeAddressTakenVariables.TryGetValue(capturedVariable, out Location value) && (!(capturedVariable is ParameterSymbol key) || !(capturedVariable.ContainingSymbol is SynthesizedPrimaryConstructor synthesizedPrimaryConstructor) || !synthesizedPrimaryConstructor.GetCapturedParameters().ContainsKey(key)))
			{
				diagnostics.Add(ErrorCode.ERR_LocalCantBeFixedAndHoisted, value, capturedVariable.Name);
			}
		}
		diagnostics.AddRange(base.Diagnostics);
	}

	private void CheckCaptured(Symbol variable, ParameterSymbol? rangeVariableUnderlyingParameter = null)
	{
		if (CurrentSymbol is SourceMethodSymbol containingSymbol && Symbol.IsCaptured(rangeVariableUnderlyingParameter ?? variable, containingSymbol))
		{
			NoteCaptured(variable);
		}
	}

	private void NoteCaptured(Symbol variable)
	{
		if (regionPlace == RegionPlace.Inside)
		{
			_capturedInside.Add(variable);
			_capturedVariables.Add(variable);
		}
		else if (variable.Kind != SymbolKind.RangeVariable)
		{
			_capturedOutside.Add(variable);
			_capturedVariables.Add(variable);
		}
	}

	protected IEnumerable<Symbol> GetCapturedInside()
	{
		return _capturedInside.ToArray();
	}

	protected IEnumerable<Symbol> GetCapturedOutside()
	{
		return _capturedOutside.ToArray();
	}

	protected IEnumerable<Symbol> GetCaptured()
	{
		return _capturedVariables.ToArray();
	}

	protected IEnumerable<Symbol> GetUnsafeAddressTaken()
	{
		return _unsafeAddressTakenVariables.Keys.ToArray();
	}

	protected IEnumerable<MethodSymbol> GetUsedLocalFunctions()
	{
		return _usedLocalFunctions.ToArray();
	}

	private void NotePrimaryConstructorParameterReadIfNeeded(Symbol symbol)
	{
		if (symbol is ParameterSymbol item && symbol.ContainingSymbol is SynthesizedPrimaryConstructor)
		{
			if (_readParameters == null)
			{
				_readParameters = PooledHashSet<ParameterSymbol>.GetInstance();
			}
			_readParameters.Add(item);
		}
	}

	protected virtual void NoteRead(Symbol variable, ParameterSymbol rangeVariableUnderlyingParameter = null)
	{
		if (variable is LocalSymbol item)
		{
			_usedVariables.Add(item);
		}
		NotePrimaryConstructorParameterReadIfNeeded(variable);
		if (variable is LocalFunctionSymbol item2)
		{
			_usedLocalFunctions.Add(item2);
		}
		if ((object)variable != null)
		{
			if ((object)_sourceAssembly != null && variable.Kind == SymbolKind.Field)
			{
				_sourceAssembly.NoteFieldAccess((FieldSymbol)variable.OriginalDefinition, read: true, write: false);
			}
			CheckCaptured(variable, rangeVariableUnderlyingParameter);
		}
	}

	private void NoteRead(BoundNode fieldOrEventAccess)
	{
		BoundNode boundNode = fieldOrEventAccess;
		while (boundNode != null)
		{
			switch (boundNode.Kind)
			{
			default:
				return;
			case BoundKind.FieldAccess:
			{
				BoundFieldAccess boundFieldAccess = (BoundFieldAccess)boundNode;
				NoteRead(boundFieldAccess.FieldSymbol);
				if (MayRequireTracking(boundFieldAccess.ReceiverOpt, boundFieldAccess.FieldSymbol))
				{
					boundNode = boundFieldAccess.ReceiverOpt;
					break;
				}
				return;
			}
			case BoundKind.EventAccess:
			{
				BoundEventAccess boundEventAccess = (BoundEventAccess)boundNode;
				FieldSymbol associatedField = boundEventAccess.EventSymbol.AssociatedField;
				if ((object)associatedField != null)
				{
					NoteRead(associatedField);
					if (MayRequireTracking(boundEventAccess.ReceiverOpt, associatedField))
					{
						boundNode = boundEventAccess.ReceiverOpt;
						break;
					}
					return;
				}
				return;
			}
			case BoundKind.ThisReference:
				NoteRead(base.MethodThisParameter);
				return;
			case BoundKind.Local:
				NoteRead(((BoundLocal)boundNode).LocalSymbol);
				return;
			case BoundKind.Parameter:
				NoteRead(((BoundParameter)boundNode).ParameterSymbol);
				return;
			case BoundKind.InlineArrayAccess:
				boundNode = ((BoundInlineArrayAccess)boundNode).Expression;
				break;
			}
		}
	}

	protected virtual void NoteWrite(Symbol variable, BoundExpression value, bool read, bool isRef)
	{
		if ((object)variable != null)
		{
			_writtenVariables.Add(variable);
			if ((object)_sourceAssembly != null && variable.Kind == SymbolKind.Field)
			{
				FieldSymbol fieldSymbol = (FieldSymbol)variable.OriginalDefinition;
				_sourceAssembly.NoteFieldAccess(fieldSymbol, read && WriteConsideredUse(fieldSymbol.Type, value), (fieldSymbol.RefKind == RefKind.None) | isRef);
			}
			LocalSymbol localSymbol = variable as LocalSymbol;
			if ((((object)localSymbol != null) & read) && WriteConsideredUse(localSymbol.Type, value))
			{
				_usedVariables.Add(localSymbol);
			}
			CheckCaptured(variable);
		}
	}

	internal static bool WriteConsideredUse(TypeSymbol type, BoundExpression value)
	{
		if (value == null || value.HasAnyErrors)
		{
			return true;
		}
		if ((object)type != null && type.IsReferenceType && type.SpecialType != SpecialType.System_String)
		{
			if (type is ArrayTypeSymbol { IsSZArray: not false } arrayTypeSymbol)
			{
				TypeSymbol elementType = arrayTypeSymbol.ElementType;
				if ((object)elementType != null && elementType.SpecialType == SpecialType.System_Byte)
				{
					goto IL_0059;
				}
			}
			return value.ConstantValueOpt != ConstantValue.Null;
		}
		goto IL_0059;
		IL_0059:
		if ((object)type != null && type.IsPointerOrFunctionPointer())
		{
			return true;
		}
		if (value != null && (object)value.ConstantValueOpt != null && value.Kind != BoundKind.InterpolatedString)
		{
			return false;
		}
		switch (value.Kind)
		{
		case BoundKind.Conversion:
		{
			BoundConversion boundConversion = (BoundConversion)value;
			if (boundConversion.ConversionKind.IsUserDefinedConversion() || boundConversion.ConversionKind == ConversionKind.IntPtr)
			{
				return true;
			}
			return WriteConsideredUse(null, boundConversion.Operand);
		}
		case BoundKind.DefaultLiteral:
		case BoundKind.DefaultExpression:
			return false;
		case BoundKind.ObjectCreationExpression:
		{
			BoundObjectCreationExpression boundObjectCreationExpression = (BoundObjectCreationExpression)value;
			if (boundObjectCreationExpression.Constructor.IsImplicitlyDeclared)
			{
				return boundObjectCreationExpression.InitializerExpressionOpt != null;
			}
			return true;
		}
		case BoundKind.Utf8String:
		case BoundKind.TupleLiteral:
		case BoundKind.ConvertedTupleLiteral:
			return false;
		default:
			return true;
		}
	}

	private void NoteWrite(BoundExpression n, BoundExpression value, bool read, bool isRef)
	{
		while (n != null)
		{
			switch (n.Kind)
			{
			case BoundKind.FieldAccess:
			{
				BoundFieldAccess boundFieldAccess = (BoundFieldAccess)n;
				if ((object)_sourceAssembly != null)
				{
					FieldSymbol originalDefinition2 = boundFieldAccess.FieldSymbol.OriginalDefinition;
					_sourceAssembly.NoteFieldAccess(originalDefinition2, value == null || WriteConsideredUse(boundFieldAccess.FieldSymbol.Type, value), (originalDefinition2.RefKind == RefKind.None) | isRef);
				}
				if (MayRequireTracking(boundFieldAccess.ReceiverOpt, boundFieldAccess.FieldSymbol))
				{
					n = boundFieldAccess.ReceiverOpt;
					isRef = false;
					if (n.Kind == BoundKind.Local)
					{
						_usedVariables.Add(((BoundLocal)n).LocalSymbol);
					}
					break;
				}
				return;
			}
			case BoundKind.EventAccess:
			{
				BoundEventAccess boundEventAccess = (BoundEventAccess)n;
				FieldSymbol associatedField = boundEventAccess.EventSymbol.AssociatedField;
				if ((object)associatedField != null)
				{
					if ((object)_sourceAssembly != null)
					{
						FieldSymbol originalDefinition = associatedField.OriginalDefinition;
						_sourceAssembly.NoteFieldAccess(originalDefinition, value == null || WriteConsideredUse(associatedField.Type, value), write: true);
					}
					if (MayRequireTracking(boundEventAccess.ReceiverOpt, associatedField))
					{
						n = boundEventAccess.ReceiverOpt;
						break;
					}
					return;
				}
				return;
			}
			case BoundKind.ThisReference:
				NoteWrite(base.MethodThisParameter, value, read, isRef);
				return;
			case BoundKind.Local:
				NoteWrite(((BoundLocal)n).LocalSymbol, value, read, isRef);
				return;
			case BoundKind.Parameter:
				NoteWrite(((BoundParameter)n).ParameterSymbol, value, read, isRef);
				return;
			case BoundKind.RangeVariable:
				NoteWrite(((BoundRangeVariable)n).Value, value, read, isRef);
				return;
			case BoundKind.InlineArrayAccess:
				n = ((BoundInlineArrayAccess)n).Expression;
				value = null;
				break;
			default:
				return;
			}
		}
	}

	protected override void Normalize(ref LocalState state)
	{
		int capacity = state.Assigned.Capacity;
		int count = variableBySlot.Count;
		state.Assigned.EnsureCapacity(count);
		for (int i = capacity; i < count; i++)
		{
			int containingSlot = variableBySlot[i].ContainingSlot;
			bool value = containingSlot > 0 && state.Assigned[containingSlot] && variableBySlot[containingSlot].Symbol.GetTypeOrReturnType().TypeKind == TypeKind.Struct;
			if (state.NormalizeToBottom && containingSlot == 0)
			{
				value = true;
			}
			state.Assigned[i] = value;
		}
	}

	protected override bool TryGetReceiverAndMember(BoundExpression expr, out BoundExpression receiver, out Symbol member)
	{
		receiver = null;
		member = null;
		switch (expr.Kind)
		{
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)expr;
			FieldSymbol fieldSymbol = (FieldSymbol)(member = boundFieldAccess.FieldSymbol);
			if (fieldSymbol.IsFixedSizeBuffer)
			{
				return false;
			}
			if (fieldSymbol.IsStatic)
			{
				return _trackStaticMembers;
			}
			receiver = boundFieldAccess.ReceiverOpt;
			break;
		}
		case BoundKind.EventAccess:
		{
			BoundEventAccess boundEventAccess = (BoundEventAccess)expr;
			EventSymbol eventSymbol = boundEventAccess.EventSymbol;
			member = eventSymbol.AssociatedField;
			if (eventSymbol.IsStatic)
			{
				return _trackStaticMembers;
			}
			receiver = boundEventAccess.ReceiverOpt;
			break;
		}
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)expr;
			if (Binder.AccessingAutoPropertyFromConstructor(boundPropertyAccess, CurrentSymbol))
			{
				PropertySymbol propertySymbol = boundPropertyAccess.PropertySymbol;
				member = (propertySymbol as SourcePropertySymbolBase)?.BackingField;
				if ((object)member == null)
				{
					return false;
				}
				if (propertySymbol.IsStatic)
				{
					return _trackStaticMembers;
				}
				receiver = boundPropertyAccess.ReceiverOpt;
			}
			break;
		}
		}
		if ((object)member != null && receiver != null && receiver.Kind != BoundKind.TypeExpression)
		{
			return MayRequireTrackingReceiverType(receiver.Type);
		}
		return false;
	}

	private bool MayRequireTrackingReceiverType(TypeSymbol type)
	{
		if ((object)type != null)
		{
			if (!_trackClassFields)
			{
				return type.TypeKind == TypeKind.Struct;
			}
			return true;
		}
		return false;
	}

	protected bool MayRequireTracking(BoundExpression receiverOpt, FieldSymbol fieldSymbol)
	{
		if ((object)fieldSymbol != null && receiverOpt != null && !fieldSymbol.IsStatic && !fieldSymbol.IsFixedSizeBuffer && receiverOpt.Kind != BoundKind.TypeExpression && MayRequireTrackingReceiverType(receiverOpt.Type))
		{
			return !receiverOpt.Type.IsPrimitiveRecursiveStruct();
		}
		return false;
	}

	protected void CheckAssigned(Symbol symbol, SyntaxNode node)
	{
		if ((object)symbol == null)
		{
			return;
		}
		NoteRead(symbol);
		if (State.Reachable)
		{
			int num = VariableSlot(symbol);
			if (num >= State.Assigned.Capacity)
			{
				Normalize(ref State);
			}
			if (num > 0 && !State.IsAssigned(num))
			{
				ReportUnassignedIfNotCapturedInLocalFunction(symbol, node, num);
			}
		}
	}

	private void ReportUnassignedIfNotCapturedInLocalFunction(Symbol symbol, SyntaxNode node, int slot, bool skipIfUseBeforeDeclaration = true)
	{
		if (IsCapturedInLocalFunction(slot))
		{
			RecordReadInLocalFunction(slot);
		}
		else
		{
			ReportUnassigned(symbol, node, slot, skipIfUseBeforeDeclaration);
		}
	}

	protected virtual void ReportUnassigned(Symbol symbol, SyntaxNode node, int slot, bool skipIfUseBeforeDeclaration)
	{
		if (slot <= 0 || symbol is LocalSymbol { IsConst: not false })
		{
			return;
		}
		if (slot >= _alreadyReported.Capacity)
		{
			_alreadyReported.EnsureCapacity(variableBySlot.Count);
		}
		if (skipIfUseBeforeDeclaration && symbol.Kind == SymbolKind.Local)
		{
			Location location = symbol.TryGetFirstLocation();
			if ((object)location == null || node.Span.End < location.SourceSpan.Start)
			{
				goto IL_015c;
			}
		}
		if (!_alreadyReported[slot] && !symbol.GetTypeOrReturnType().Type.IsErrorType())
		{
			string name = symbol.Name;
			if (symbol.Kind == SymbolKind.Field)
			{
				addDiagnosticForStructField(slot, (FieldSymbol)symbol);
			}
			else if (symbol.Kind == SymbolKind.Parameter && ((ParameterSymbol)symbol).RefKind == RefKind.Out)
			{
				if (((ParameterSymbol)symbol).IsThis)
				{
					addDiagnosticForStructThis(symbol, slot);
				}
				else
				{
					base.Diagnostics.Add(ErrorCode.ERR_UseDefViolationOut, node.Location, name);
				}
			}
			else
			{
				base.Diagnostics.Add(ErrorCode.ERR_UseDefViolation, node.Location, name);
			}
		}
		goto IL_015c;
		IL_015c:
		_alreadyReported[slot] = true;
		void addDiagnosticForStructField(int fieldSlot, FieldSymbol fieldSymbol)
		{
			Symbol associatedSymbol = fieldSymbol.AssociatedSymbol;
			bool flag = (object)associatedSymbol != null && associatedSymbol.Kind == SymbolKind.Property;
			string text = (flag ? associatedSymbol.Name : fieldSymbol.Name);
			Symbol currentSymbol = CurrentSymbol;
			if (currentSymbol is MethodSymbol { MethodKind: MethodKind.Constructor })
			{
				NamedTypeSymbol containingType = currentSymbol.ContainingType;
				if ((object)containingType != null && containingType.TypeKind == TypeKind.Struct)
				{
					int orCreateSlot = GetOrCreateSlot(CurrentSymbol.EnclosingThisSymbol());
					LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier variableIdentifier;
					while (true)
					{
						if (fieldSlot == 0)
						{
							base.Diagnostics.Add(flag ? ErrorCode.ERR_UseDefViolationProperty : ErrorCode.ERR_UseDefViolationField, node.Location, text);
							return;
						}
						variableIdentifier = variableBySlot[fieldSlot];
						int containingSlot = variableIdentifier.ContainingSlot;
						if (containingSlot == orCreateSlot)
						{
							break;
						}
						fieldSlot = containingSlot;
					}
					AddImplicitlyInitializedField((FieldSymbol)variableIdentifier.Symbol);
					if (fieldSymbol.RefKind != RefKind.None)
					{
						if (!flag)
						{
							base.Diagnostics.Add(ErrorCode.WRN_UseDefViolationRefField, node.Location, text);
						}
					}
					else if (compilation.IsFeatureEnabled(MessageID.IDS_FeatureAutoDefaultStructs))
					{
						base.Diagnostics.Add(flag ? ErrorCode.WRN_UseDefViolationPropertySupportedVersion : ErrorCode.WRN_UseDefViolationFieldSupportedVersion, node.Location, text);
					}
					else
					{
						base.Diagnostics.Add(flag ? ErrorCode.ERR_UseDefViolationPropertyUnsupportedVersion : ErrorCode.ERR_UseDefViolationFieldUnsupportedVersion, node.Location, text, new CSharpRequiredLanguageVersion(MessageID.IDS_FeatureAutoDefaultStructs.RequiredVersion()));
					}
					return;
				}
			}
			base.Diagnostics.Add(flag ? ErrorCode.ERR_UseDefViolationProperty : ErrorCode.ERR_UseDefViolationField, node.Location, text);
		}
		void addDiagnosticForStructThis(Symbol thisParameter, int thisSlot)
		{
			if (TrackImplicitlyInitializedFields)
			{
				bool flag = false;
				NamedTypeSymbol containingType = thisParameter.ContainingType;
				foreach (FieldSymbol structInstanceField in _emptyStructTypeCache.GetStructInstanceFields(containingType))
				{
					if (!_emptyStructTypeCache.IsEmptyStructType(structInstanceField.Type) && !(structInstanceField is TupleErrorFieldSymbol))
					{
						int num = VariableSlot(structInstanceField, thisSlot);
						if (num == -1 || !State.IsAssigned(num))
						{
							AddImplicitlyInitializedField(structInstanceField);
							flag = true;
						}
					}
				}
				if (!flag && containingType.HasInlineArrayAttribute(out var length) && length > 1)
				{
					FieldSymbol fieldSymbol = containingType.TryGetPossiblyUnsupportedByLanguageInlineArrayElementField();
					if ((object)fieldSymbol != null)
					{
						AddImplicitlyInitializedField(fieldSymbol);
						flag = true;
					}
				}
			}
			if (compilation.IsFeatureEnabled(MessageID.IDS_FeatureAutoDefaultStructs))
			{
				base.Diagnostics.Add(ErrorCode.WRN_UseDefViolationThisSupportedVersion, node.Location);
			}
			else
			{
				base.Diagnostics.Add(ErrorCode.ERR_UseDefViolationThisUnsupportedVersion, node.Location, new CSharpRequiredLanguageVersion(MessageID.IDS_FeatureAutoDefaultStructs.RequiredVersion()));
			}
		}
	}

	protected virtual void CheckAssigned(BoundExpression expr, FieldSymbol fieldSymbol, SyntaxNode node)
	{
		if (State.Reachable && !IsAssigned(expr, out var unassignedSlot))
		{
			ReportUnassignedIfNotCapturedInLocalFunction(fieldSymbol, node, unassignedSlot);
		}
		NoteRead(expr);
	}

	private bool IsAssigned(BoundExpression node, out int unassignedSlot)
	{
		unassignedSlot = -1;
		if (_emptyStructTypeCache.IsEmptyStructType(node.Type))
		{
			return true;
		}
		switch (node.Kind)
		{
		case BoundKind.ThisReference:
			if ((object)base.MethodThisParameter == null)
			{
				unassignedSlot = -1;
				return true;
			}
			unassignedSlot = GetOrCreateSlot(base.MethodThisParameter);
			break;
		case BoundKind.Local:
			unassignedSlot = GetOrCreateSlot(((BoundLocal)node).LocalSymbol);
			break;
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)node;
			if (!MayRequireTracking(boundFieldAccess.ReceiverOpt, boundFieldAccess.FieldSymbol) || IsAssigned(boundFieldAccess.ReceiverOpt, out unassignedSlot))
			{
				return true;
			}
			unassignedSlot = GetOrCreateSlot(boundFieldAccess.FieldSymbol, unassignedSlot);
			break;
		}
		case BoundKind.EventAccess:
		{
			BoundEventAccess boundEventAccess = (BoundEventAccess)node;
			if (!MayRequireTracking(boundEventAccess.ReceiverOpt, boundEventAccess.EventSymbol.AssociatedField) || IsAssigned(boundEventAccess.ReceiverOpt, out unassignedSlot))
			{
				return true;
			}
			unassignedSlot = GetOrCreateSlot(boundEventAccess.EventSymbol.AssociatedField, unassignedSlot);
			break;
		}
		case BoundKind.InlineArrayAccess:
		{
			BoundInlineArrayAccess boundInlineArrayAccess = (BoundInlineArrayAccess)node;
			return IsAssigned(boundInlineArrayAccess.Expression, out unassignedSlot);
		}
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)node;
			if (Binder.AccessingAutoPropertyFromConstructor(boundPropertyAccess, CurrentSymbol))
			{
				SynthesizedBackingFieldSymbol synthesizedBackingFieldSymbol = (boundPropertyAccess.PropertySymbol as SourcePropertySymbolBase)?.BackingField;
				if (synthesizedBackingFieldSymbol != null)
				{
					if (!MayRequireTracking(boundPropertyAccess.ReceiverOpt, synthesizedBackingFieldSymbol) || IsAssigned(boundPropertyAccess.ReceiverOpt, out unassignedSlot))
					{
						return true;
					}
					unassignedSlot = GetOrCreateSlot(synthesizedBackingFieldSymbol, unassignedSlot);
					break;
				}
			}
			goto default;
		}
		case BoundKind.Parameter:
		{
			BoundParameter boundParameter = (BoundParameter)node;
			unassignedSlot = GetOrCreateSlot(boundParameter.ParameterSymbol);
			break;
		}
		default:
			unassignedSlot = -1;
			return true;
		}
		if (unassignedSlot > 0)
		{
			return State.IsAssigned(unassignedSlot);
		}
		return true;
	}

	private Symbol UseNonFieldSymbolUnsafely(BoundExpression expression)
	{
		while (expression != null)
		{
			BoundFieldAccess boundFieldAccess;
			switch (expression.Kind)
			{
			case BoundKind.FieldAccess:
			{
				boundFieldAccess = (BoundFieldAccess)expression;
				FieldSymbol fieldSymbol = boundFieldAccess.FieldSymbol;
				if ((object)_sourceAssembly != null)
				{
					_sourceAssembly.NoteFieldAccess(fieldSymbol, read: true, write: true);
				}
				if (fieldSymbol.ContainingType.IsReferenceType || fieldSymbol.IsStatic)
				{
					return null;
				}
				break;
			}
			case BoundKind.Local:
			{
				LocalSymbol localSymbol = ((BoundLocal)expression).LocalSymbol;
				_usedVariables.Add(localSymbol);
				return localSymbol;
			}
			case BoundKind.RangeVariable:
				return ((BoundRangeVariable)expression).RangeVariableSymbol;
			case BoundKind.Parameter:
				return ((BoundParameter)expression).ParameterSymbol;
			case BoundKind.ThisReference:
				return base.MethodThisParameter;
			case BoundKind.BaseReference:
				return base.MethodThisParameter;
			default:
				return null;
			}
			expression = boundFieldAccess.ReceiverOpt;
		}
		return null;
	}

	protected void Assign(BoundNode node, BoundExpression value, bool isRef = false, bool read = true)
	{
		if (!isRef && node is BoundFieldAccess boundFieldAccess)
		{
			FieldSymbol fieldSymbol = boundFieldAccess.FieldSymbol;
			if ((object)fieldSymbol != null && fieldSymbol.RefKind != RefKind.None)
			{
				CheckAssigned(boundFieldAccess, node.Syntax);
			}
		}
		AssignImpl(node, value, isRef, written: true, read);
	}

	protected virtual void AssignImpl(BoundNode node, BoundExpression value, bool isRef, bool written, bool read)
	{
		BoundInlineArrayAccess boundInlineArrayAccess;
		switch (node.Kind)
		{
		case BoundKind.DeclarationPattern:
		case BoundKind.RecursivePattern:
		case BoundKind.ListPattern:
		{
			BoundObjectPattern boundObjectPattern = (BoundObjectPattern)node;
			if (boundObjectPattern.Variable is LocalSymbol symbol)
			{
				int orCreateSlot = GetOrCreateSlot(symbol);
				SetSlotState(orCreateSlot, written || !State.Reachable);
			}
			if (written)
			{
				NoteWrite(boundObjectPattern.VariableAccess, value, read, isRef);
			}
			break;
		}
		case BoundKind.LocalDeclaration:
		{
			LocalSymbol localSymbol = ((BoundLocalDeclaration)node).LocalSymbol;
			int orCreateSlot2 = GetOrCreateSlot(localSymbol);
			SetSlotState(orCreateSlot2, written || !State.Reachable);
			if (written)
			{
				NoteWrite(localSymbol, value, read, isRef);
			}
			break;
		}
		case BoundKind.Local:
		{
			BoundLocal boundLocal = (BoundLocal)node;
			if (boundLocal.LocalSymbol.RefKind != RefKind.None && !isRef)
			{
				if (written)
				{
					VisitRvalue(boundLocal, isKnownToBeAnLvalue: true);
				}
				break;
			}
			int slot2 = MakeSlot(boundLocal);
			SetSlotState(slot2, written);
			if (written)
			{
				NoteWrite(boundLocal, value, read, isRef);
			}
			break;
		}
		case BoundKind.InlineArrayAccess:
		{
			boundInlineArrayAccess = (BoundInlineArrayAccess)node;
			if (written)
			{
				NoteWrite(boundInlineArrayAccess.Expression, null, read, isRef);
			}
			if (boundInlineArrayAccess.Expression.Type.HasInlineArrayAttribute(out var length))
			{
				ConstantValue constantValueOpt = boundInlineArrayAccess.Argument.ConstantValueOpt;
				if ((object)constantValueOpt == null || constantValueOpt.SpecialType != SpecialType.System_Int32 || constantValueOpt.Int32Value != 0)
				{
					int? num = Binder.InferConstantIndexFromSystemIndex(compilation, boundInlineArrayAccess.Argument, length, out var _);
					if ((num ?? 1) != 0)
					{
						goto IL_0241;
					}
				}
				int num2 = MakeMemberSlot(boundInlineArrayAccess.Expression, boundInlineArrayAccess.Expression.Type.TryGetInlineArrayElementField());
				if (num2 > 0)
				{
					SetSlotState(num2, written);
					break;
				}
			}
			goto IL_0241;
		}
		case BoundKind.Parameter:
		{
			BoundParameter boundParameter = (BoundParameter)node;
			ParameterSymbol parameterSymbol = boundParameter.ParameterSymbol;
			if (isRef && parameterSymbol.RefKind == RefKind.Out)
			{
				LeaveParameter(parameterSymbol, node.Syntax, boundParameter.Syntax.Location);
			}
			int slot = MakeSlot(boundParameter);
			SetSlotState(slot, written);
			if (written)
			{
				NoteWrite(boundParameter, value, read, isRef);
			}
			break;
		}
		case BoundKind.ObjectInitializerMember:
		{
			BoundObjectInitializerMember boundObjectInitializerMember = (BoundObjectInitializerMember)node;
			if ((object)_sourceAssembly != null && boundObjectInitializerMember.MemberSymbol is FieldSymbol fieldSymbol)
			{
				_sourceAssembly.NoteFieldAccess(fieldSymbol.OriginalDefinition, read: false, (fieldSymbol.RefKind == RefKind.None) | isRef);
			}
			break;
		}
		case BoundKind.ThisReference:
		case BoundKind.FieldAccess:
		case BoundKind.PropertyAccess:
		case BoundKind.EventAccess:
		{
			BoundExpression boundExpression = (BoundExpression)node;
			int slot3 = MakeSlot(boundExpression);
			SetSlotState(slot3, written);
			if (written)
			{
				NoteWrite(boundExpression, value, read, isRef);
			}
			break;
		}
		case BoundKind.RangeVariable:
			AssignImpl(((BoundRangeVariable)node).Value, value, isRef, written, read);
			break;
		case BoundKind.BadExpression:
		{
			BoundBadExpression boundBadExpression = (BoundBadExpression)node;
			if (!boundBadExpression.ChildBoundNodes.IsDefault && boundBadExpression.ChildBoundNodes.Length == 1)
			{
				AssignImpl(boundBadExpression.ChildBoundNodes[0], value, isRef, written, read);
			}
			break;
		}
		case BoundKind.TupleLiteral:
		case BoundKind.ConvertedTupleLiteral:
			{
				((BoundTupleExpression)node).VisitAllElements(delegate(BoundExpression x, (DefiniteAssignmentPass self, bool isRef) arg)
				{
					arg.self.Assign(x, null, arg.isRef);
				}, (this, isRef));
				break;
			}
			IL_0241:
			if (!written)
			{
				AssignImpl(boundInlineArrayAccess.Expression, null, isRef, written, read);
				int slot4 = MakeSlot(boundInlineArrayAccess.Expression);
				SetSlotState(slot4, written);
			}
			break;
		}
	}

	private bool FieldsAllSet(int containingSlot, LocalState state)
	{
		TypeSymbol type = variableBySlot[containingSlot].Symbol.GetTypeOrReturnType().Type;
		if (type.HasInlineArrayAttribute(out var length) && length > 1 && (object)type.TryGetPossiblyUnsupportedByLanguageInlineArrayElementField() != null)
		{
			return false;
		}
		foreach (FieldSymbol structInstanceField in _emptyStructTypeCache.GetStructInstanceFields(type))
		{
			if (!_emptyStructTypeCache.IsEmptyStructType(structInstanceField.Type) && !(structInstanceField is TupleErrorFieldSymbol))
			{
				int num = VariableSlot(structInstanceField, containingSlot);
				if (num == -1 || !state.IsAssigned(num))
				{
					return false;
				}
			}
		}
		return true;
	}

	protected void SetSlotState(int slot, bool assigned)
	{
		if (slot > 0)
		{
			if (assigned)
			{
				SetSlotAssigned(slot);
			}
			else
			{
				SetSlotUnassigned(slot);
			}
		}
	}

	protected void SetSlotAssigned(int slot, ref LocalState state)
	{
		if (slot < 0)
		{
			return;
		}
		LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier variableIdentifier = variableBySlot[slot];
		TypeSymbol type = variableIdentifier.Symbol.GetTypeOrReturnType().Type;
		if (slot >= state.Assigned.Capacity)
		{
			Normalize(ref state);
		}
		if (state.IsAssigned(slot))
		{
			return;
		}
		state.Assign(slot);
		if (EmptyStructTypeCache.IsTrackableStructType(type))
		{
			foreach (FieldSymbol structInstanceField in _emptyStructTypeCache.GetStructInstanceFields(type))
			{
				int num = VariableSlot(structInstanceField, slot);
				if (num > 0)
				{
					SetSlotAssigned(num, ref state);
				}
			}
		}
		while (variableIdentifier.ContainingSlot > 0)
		{
			slot = variableIdentifier.ContainingSlot;
			if (!state.IsAssigned(slot) && FieldsAllSet(slot, state))
			{
				state.Assign(slot);
				variableIdentifier = variableBySlot[slot];
				continue;
			}
			break;
		}
	}

	private void SetSlotAssigned(int slot)
	{
		SetSlotAssigned(slot, ref State);
	}

	private void SetSlotUnassigned(int slot, ref LocalState state)
	{
		if (slot < 0)
		{
			return;
		}
		LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier variableIdentifier = variableBySlot[slot];
		TypeSymbol type = variableIdentifier.Symbol.GetTypeOrReturnType().Type;
		if (!state.IsAssigned(slot))
		{
			return;
		}
		state.Unassign(slot);
		if (EmptyStructTypeCache.IsTrackableStructType(type))
		{
			foreach (FieldSymbol structInstanceField in _emptyStructTypeCache.GetStructInstanceFields(type))
			{
				int num = VariableSlot(structInstanceField, slot);
				if (num > 0)
				{
					SetSlotUnassigned(num, ref state);
				}
			}
		}
		while (variableIdentifier.ContainingSlot > 0)
		{
			slot = variableIdentifier.ContainingSlot;
			state.Unassign(slot);
			variableIdentifier = variableBySlot[slot];
		}
	}

	private void SetSlotUnassigned(int slot)
	{
		if (NonMonotonicState.HasValue)
		{
			LocalState state = NonMonotonicState.Value;
			SetSlotUnassigned(slot, ref state);
			NonMonotonicState = state;
		}
		SetSlotUnassigned(slot, ref State);
	}

	protected override LocalState TopState()
	{
		LocalState state = new LocalState(BitVector.Empty);
		Symbol symbol = CurrentSymbol;
		while (true)
		{
			bool flag;
			switch (symbol?.Kind)
			{
			case SymbolKind.Field:
			case SymbolKind.Method:
			case SymbolKind.Property:
				flag = true;
				break;
			default:
				flag = false;
				break;
			}
			if (!flag)
			{
				break;
			}
			if ((object)symbol != CurrentSymbol && symbol is MethodSymbol { Parameters: var parameters } methodSymbol)
			{
				foreach (ParameterSymbol item in parameters)
				{
					if (item.RefKind != RefKind.Out)
					{
						int orCreateSlot = GetOrCreateSlot(item);
						if (orCreateSlot > 0)
						{
							SetSlotAssigned(orCreateSlot, ref state);
						}
					}
				}
				if (methodSymbol.TryGetThisParameter(out ParameterSymbol thisParameter) && (object)thisParameter != null && thisParameter.RefKind != RefKind.Out)
				{
					int orCreateSlot2 = GetOrCreateSlot(thisParameter);
					if (orCreateSlot2 > 0)
					{
						SetSlotAssigned(orCreateSlot2, ref state);
					}
				}
				if (_symbol.TryGetInstanceExtensionParameter(out ParameterSymbol extensionParameter) && extensionParameter.RefKind != RefKind.Out)
				{
					int orCreateSlot3 = GetOrCreateSlot(extensionParameter);
					if (orCreateSlot3 > 0)
					{
						SetSlotAssigned(orCreateSlot3, ref state);
					}
				}
			}
			Symbol containingSymbol = symbol.ContainingSymbol;
			if (!symbol.IsStatic && containingSymbol is SourceMemberContainerTypeSymbol sourceMemberContainerTypeSymbol)
			{
				SynthesizedPrimaryConstructor primaryConstructor = sourceMemberContainerTypeSymbol.PrimaryConstructor;
				if ((object)primaryConstructor != null && (object)symbol != primaryConstructor)
				{
					foreach (ParameterSymbol parameter in primaryConstructor.Parameters)
					{
						int orCreateSlot4 = GetOrCreateSlot(parameter);
						if (orCreateSlot4 > 0)
						{
							if (!(symbol is MethodSymbol) && parameter.RefKind == RefKind.Out)
							{
								SetSlotUnassigned(orCreateSlot4, ref state);
							}
							else
							{
								SetSlotAssigned(orCreateSlot4, ref state);
							}
						}
					}
					break;
				}
			}
			symbol = containingSymbol;
		}
		return state;
	}

	protected override LocalState ReachableBottomState()
	{
		LocalState result = new LocalState(BitVector.AllSet(variableBySlot.Count));
		result.Assigned[0] = false;
		return result;
	}

	protected override void EnterParameter(ParameterSymbol parameter)
	{
		int orCreateSlot = GetOrCreateSlot(parameter);
		if (parameter.RefKind == RefKind.Out && !(CurrentSymbol is MethodSymbol { IsAsync: not false }))
		{
			if (orCreateSlot > 0)
			{
				SetSlotState(orCreateSlot, initiallyAssignedVariables?.Contains(parameter) ?? false);
			}
		}
		else
		{
			if (orCreateSlot > 0)
			{
				SetSlotState(orCreateSlot, assigned: true);
			}
			NoteWrite(parameter, null, read: true, parameter.RefKind != RefKind.None);
		}
		SourceComplexParameterSymbolBase sourceComplexParameterSymbolBase = parameter as SourceComplexParameterSymbolBase;
		bool flag;
		if ((object)sourceComplexParameterSymbolBase != null)
		{
			Symbol containingSymbol = sourceComplexParameterSymbolBase.ContainingSymbol;
			if (containingSymbol is LocalFunctionSymbol || containingSymbol is LambdaSymbol)
			{
				flag = true;
				goto IL_0092;
			}
		}
		flag = false;
		goto IL_0092;
		IL_0092:
		if (flag)
		{
			VisitAttributes(sourceComplexParameterSymbolBase.BindParameterAttributes());
			BoundParameterEqualsValue boundParameterEqualsValue = sourceComplexParameterSymbolBase.BindParameterEqualsValue();
			if (boundParameterEqualsValue != null)
			{
				VisitRvalue(boundParameterEqualsValue.Value);
			}
		}
	}

	private void VisitAttributes(ImmutableArray<(CSharpAttributeData, BoundAttribute)> boundAttributes)
	{
		if (boundAttributes.IsDefaultOrEmpty)
		{
			return;
		}
		foreach (var (cSharpAttributeData, boundAttribute) in boundAttributes)
		{
			if (!cSharpAttributeData.HasErrors)
			{
				foreach (BoundExpression constructorArgument in boundAttribute.ConstructorArguments)
				{
					VisitRvalue(constructorArgument);
				}
				foreach (BoundAssignmentOperator namedArgument in boundAttribute.NamedArguments)
				{
					VisitRvalue(namedArgument.Right);
				}
			}
		}
	}

	protected override void LeaveParameters(ImmutableArray<ParameterSymbol> parameters, SyntaxNode syntax, Location location)
	{
		if (State.Reachable)
		{
			base.LeaveParameters(parameters, syntax, location);
		}
	}

	protected override void LeaveParameter(ParameterSymbol parameter, SyntaxNode syntax, Location location)
	{
		if (!parameter.IsThis && parameter.RefKind != RefKind.Out && parameter.ContainingSymbol is SynthesizedPrimaryConstructor synthesizedPrimaryConstructor)
		{
			PooledHashSet<ParameterSymbol>? readParameters = _readParameters;
			if ((readParameters == null || !readParameters.Contains(parameter)) && !synthesizedPrimaryConstructor.GetCapturedParameters().ContainsKey(parameter))
			{
				DiagnosticBag diagnostics = base.Diagnostics;
				SourceMemberContainerTypeSymbol containingType = synthesizedPrimaryConstructor.ContainingType;
				bool flag = (((object)containingType != null && (containingType.IsRecord || containingType.IsRecordStruct)) ? true : false);
				diagnostics.Add(flag ? ErrorCode.WRN_UnreadRecordParameter : ErrorCode.WRN_UnreadPrimaryConstructorParameter, parameter.GetFirstLocationOrNone(), parameter.Name);
			}
		}
		if (parameter.RefKind != RefKind.None)
		{
			int num = VariableSlot(parameter);
			if (num > 0 && !State.IsAssigned(num))
			{
				ReportUnassignedOutParameter(parameter, syntax, location);
			}
			NoteRead(parameter);
		}
	}

	protected override LocalState UnreachableState()
	{
		LocalState result = State.Clone();
		result.Assigned.EnsureCapacity(1);
		result.Assign(0);
		return result;
	}

	public override void VisitPattern(BoundPattern pattern)
	{
		base.VisitPattern(pattern);
		LocalState stateWhenFalse = StateWhenFalse;
		SetState(StateWhenTrue);
		assignPatternVariablesAndMarkReadFields(pattern);
		SetConditionalState(State, stateWhenFalse);
		void assignPatternVariablesAndMarkReadFields(BoundPattern boundPattern, bool definitely = true)
		{
			switch (boundPattern.Kind)
			{
			case BoundKind.DeclarationPattern:
			{
				BoundDeclarationPattern node = (BoundDeclarationPattern)boundPattern;
				if (definitely)
				{
					Assign(node, null, isRef: false, read: false);
				}
				break;
			}
			case BoundKind.SlicePattern:
			{
				BoundSlicePattern boundSlicePattern = (BoundSlicePattern)boundPattern;
				if (boundSlicePattern.Pattern != null)
				{
					assignPatternVariablesAndMarkReadFields(boundSlicePattern.Pattern, definitely);
				}
				break;
			}
			case BoundKind.ConstantPattern:
			{
				BoundConstantPattern boundConstantPattern = (BoundConstantPattern)boundPattern;
				VisitRvalue(boundConstantPattern.Value);
				break;
			}
			case BoundKind.RecursivePattern:
			{
				BoundRecursivePattern boundRecursivePattern = (BoundRecursivePattern)boundPattern;
				if (!boundRecursivePattern.Deconstruction.IsDefaultOrEmpty)
				{
					foreach (BoundPositionalSubpattern item in boundRecursivePattern.Deconstruction)
					{
						assignPatternVariablesAndMarkReadFields(item.Pattern, definitely);
					}
				}
				if (!boundRecursivePattern.Properties.IsDefaultOrEmpty)
				{
					foreach (BoundPropertySubpattern property in boundRecursivePattern.Properties)
					{
						if ((object)_sourceAssembly != null)
						{
							for (BoundPropertySubpatternMember boundPropertySubpatternMember = property.Member; boundPropertySubpatternMember != null; boundPropertySubpatternMember = boundPropertySubpatternMember.Receiver)
							{
								if (boundPropertySubpatternMember.Symbol is FieldSymbol field)
								{
									_sourceAssembly.NoteFieldAccess(field, read: true, write: false);
								}
							}
						}
						assignPatternVariablesAndMarkReadFields(property.Pattern, definitely);
					}
				}
				if (definitely)
				{
					Assign(boundRecursivePattern, null, isRef: false, read: false);
				}
				break;
			}
			case BoundKind.ITuplePattern:
				foreach (BoundPositionalSubpattern subpattern in ((BoundITuplePattern)boundPattern).Subpatterns)
				{
					assignPatternVariablesAndMarkReadFields(subpattern.Pattern, definitely);
				}
				break;
			case BoundKind.ListPattern:
			{
				BoundListPattern boundListPattern = (BoundListPattern)boundPattern;
				foreach (BoundPattern subpattern2 in boundListPattern.Subpatterns)
				{
					assignPatternVariablesAndMarkReadFields(subpattern2, definitely);
				}
				if (definitely)
				{
					Assign(boundListPattern, null, isRef: false, read: false);
				}
				break;
			}
			case BoundKind.RelationalPattern:
			{
				BoundRelationalPattern boundRelationalPattern = (BoundRelationalPattern)boundPattern;
				VisitRvalue(boundRelationalPattern.Value);
				break;
			}
			case BoundKind.NegatedPattern:
			{
				BoundNegatedPattern boundNegatedPattern = (BoundNegatedPattern)boundPattern;
				assignPatternVariablesAndMarkReadFields(boundNegatedPattern.Negated, definitely: false);
				break;
			}
			case BoundKind.BinaryPattern:
			{
				BoundBinaryPattern boundBinaryPattern = (BoundBinaryPattern)boundPattern;
				if (!(boundBinaryPattern.Left is BoundBinaryPattern))
				{
					bool definitely2 = definitely && !boundBinaryPattern.Disjunction;
					assignPatternVariablesAndMarkReadFields(boundBinaryPattern.Left, definitely2);
					assignPatternVariablesAndMarkReadFields(boundBinaryPattern.Right, definitely2);
				}
				else
				{
					ArrayBuilder<(BoundBinaryPattern, bool)> instance = ArrayBuilder<(BoundBinaryPattern, bool)>.GetInstance();
					do
					{
						definitely = definitely && !boundBinaryPattern.Disjunction;
						instance.Push((boundBinaryPattern, definitely));
						boundBinaryPattern = boundBinaryPattern.Left as BoundBinaryPattern;
					}
					while (boundBinaryPattern != null);
					(BoundBinaryPattern, bool) result = instance.Pop();
					assignPatternVariablesAndMarkReadFields(result.Item1.Left, result.Item2);
					do
					{
						assignPatternVariablesAndMarkReadFields(result.Item1.Right, result.Item2);
					}
					while (instance.TryPop(out result));
					instance.Free();
				}
				break;
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(boundPattern.Kind);
			case BoundKind.DiscardPattern:
			case BoundKind.TypePattern:
				break;
			}
		}
	}

	public override BoundNode? VisitBlock(BoundBlock node)
	{
		BoundBlockInstrumentation instrumentation = node.Instrumentation;
		if (instrumentation != null)
		{
			DeclareVariables(instrumentation.Locals);
			if (instrumentation.Prologue != null)
			{
				Visit(instrumentation.Prologue);
			}
		}
		DeclareVariables(node.Locals);
		VisitStatementsWithLocalFunctions(node);
		foreach (LocalSymbol local in node.Locals)
		{
			if (local.IsUsing)
			{
				NoteRead(local);
			}
		}
		ReportUnusedVariables(node.Locals);
		ReportUnusedVariables(node.LocalFunctions);
		if (instrumentation?.Epilogue != null)
		{
			Visit(instrumentation.Epilogue);
		}
		return null;
	}

	private void VisitStatementsWithLocalFunctions(BoundBlock block)
	{
		if (!TrackingRegions && !block.LocalFunctions.IsDefaultOrEmpty)
		{
			foreach (BoundStatement statement in block.Statements)
			{
				if (statement is BoundLocalFunctionStatement boundLocalFunctionStatement)
				{
					VisitAttributes(((LocalFunctionSymbol)boundLocalFunctionStatement.Symbol).BindMethodAttributes());
					VisitAlways(statement);
				}
			}
			foreach (BoundStatement statement2 in block.Statements)
			{
				if (statement2.Kind != BoundKind.LocalFunctionStatement)
				{
					VisitStatement(statement2);
				}
			}
		}
		else
		{
			foreach (BoundStatement statement3 in block.Statements)
			{
				VisitStatement(statement3);
			}
		}
	}

	public override BoundNode VisitSwitchStatement(BoundSwitchStatement node)
	{
		DeclareVariables(node.InnerLocals);
		BoundNode result = base.VisitSwitchStatement(node);
		ReportUnusedVariables(node.InnerLocals);
		ReportUnusedVariables(node.InnerLocalFunctions);
		return result;
	}

	protected override void VisitSwitchSection(BoundSwitchSection node, bool isLastSection)
	{
		DeclareVariables(node.Locals);
		base.VisitSwitchSection(node, isLastSection);
	}

	public override BoundNode VisitForStatement(BoundForStatement node)
	{
		DeclareVariables(node.OuterLocals);
		DeclareVariables(node.InnerLocals);
		BoundNode result = base.VisitForStatement(node);
		ReportUnusedVariables(node.InnerLocals);
		ReportUnusedVariables(node.OuterLocals);
		return result;
	}

	public override BoundNode VisitDoStatement(BoundDoStatement node)
	{
		DeclareVariables(node.Locals);
		BoundNode result = base.VisitDoStatement(node);
		ReportUnusedVariables(node.Locals);
		return result;
	}

	public override BoundNode VisitWhileStatement(BoundWhileStatement node)
	{
		DeclareVariables(node.Locals);
		BoundNode result = base.VisitWhileStatement(node);
		ReportUnusedVariables(node.Locals);
		return result;
	}

	public override BoundNode VisitUsingStatement(BoundUsingStatement node)
	{
		ImmutableArray<LocalSymbol> locals = node.Locals;
		DeclareVariables(locals);
		BoundNode result = base.VisitUsingStatement(node);
		if (!locals.IsDefaultOrEmpty)
		{
			foreach (LocalSymbol item in locals)
			{
				if (item.DeclarationKind == LocalDeclarationKind.UsingVariable)
				{
					NoteRead(item);
				}
			}
		}
		return result;
	}

	public override BoundNode VisitFixedStatement(BoundFixedStatement node)
	{
		DeclareVariables(node.Locals);
		return base.VisitFixedStatement(node);
	}

	public override BoundNode VisitSequence(BoundSequence node)
	{
		DeclareVariables(node.Locals);
		BoundNode result = base.VisitSequence(node);
		ReportUnusedVariables(node.Locals);
		return result;
	}

	private void DeclareVariables(ImmutableArray<LocalSymbol> locals)
	{
		foreach (LocalSymbol item in locals)
		{
			DeclareVariable(item);
		}
	}

	private void DeclareVariable(LocalSymbol symbol)
	{
		bool assigned = symbol.IsConst || (initiallyAssignedVariables?.Contains(symbol) ?? false);
		SetSlotState(GetOrCreateSlot(symbol), assigned);
	}

	private void ReportUnusedVariables(ImmutableArray<LocalSymbol> locals)
	{
		foreach (LocalSymbol item in locals)
		{
			ReportIfUnused(item, assigned: true);
		}
	}

	private void ReportIfUnused(LocalSymbol symbol, bool assigned)
	{
		if (!_usedVariables.Contains(symbol) && symbol.DeclarationKind != LocalDeclarationKind.PatternVariable && !string.IsNullOrEmpty(symbol.Name))
		{
			base.Diagnostics.Add((assigned && _writtenVariables.Contains(symbol)) ? ErrorCode.WRN_UnreferencedVarAssg : ErrorCode.WRN_UnreferencedVar, symbol.GetFirstLocationOrNone(), symbol.Name);
		}
	}

	private void ReportUnusedVariables(ImmutableArray<MethodSymbol> locals)
	{
		foreach (MethodSymbol item in locals)
		{
			ReportIfUnused(item);
		}
	}

	private void ReportIfUnused(MethodSymbol symbol)
	{
		if (!_usedLocalFunctions.Contains(symbol) && !string.IsNullOrEmpty(symbol.Name))
		{
			base.Diagnostics.Add(ErrorCode.WRN_UnreferencedLocalFunction, symbol.GetFirstLocationOrNone(), symbol.Name);
		}
	}

	public override BoundNode VisitLocal(BoundLocal node)
	{
		LocalSymbol localSymbol = node.LocalSymbol;
		if ((object)node.Type == compilation.ImplicitlyTypedVariableUsedInForbiddenZoneType)
		{
			int orCreateSlot = GetOrCreateSlot(localSymbol);
			if (orCreateSlot > 0)
			{
				_alreadyReported[orCreateSlot] = true;
			}
		}
		CheckAssigned(localSymbol, node.Syntax);
		if (localSymbol.IsFixed && CurrentSymbol is MethodSymbol methodSymbol && (methodSymbol.MethodKind == MethodKind.AnonymousFunction || methodSymbol.MethodKind == MethodKind.LocalFunction) && _capturedVariables.Contains(localSymbol))
		{
			base.Diagnostics.Add(ErrorCode.ERR_FixedLocalInLambda, new SourceLocation(node.Syntax), localSymbol);
		}
		SplitIfBooleanConstant(node);
		return null;
	}

	public override BoundNode VisitLocalDeclaration(BoundLocalDeclaration node)
	{
		GetOrCreateSlot(node.LocalSymbol);
		HashSet<Symbol>? hashSet = initiallyAssignedVariables;
		if (hashSet != null && hashSet.Contains(node.LocalSymbol))
		{
			Assign(node, null);
		}
		BoundNode result = base.VisitLocalDeclaration(node);
		if (node.InitializerOpt != null)
		{
			Assign(node, node.InitializerOpt);
		}
		return result;
	}

	public override BoundNode VisitLocalId(BoundLocalId node)
	{
		return null;
	}

	public override BoundNode VisitParameterId(BoundParameterId node)
	{
		return null;
	}

	public override BoundNode VisitStateMachineInstanceId(BoundStateMachineInstanceId node)
	{
		return null;
	}

	public override BoundNode VisitMethodGroup(BoundMethodGroup node)
	{
		foreach (MethodSymbol method in node.Methods)
		{
			if (method.MethodKind == MethodKind.LocalFunction)
			{
				_usedLocalFunctions.Add((LocalFunctionSymbol)method);
			}
		}
		return base.VisitMethodGroup(node);
	}

	public override BoundNode VisitLambda(BoundLambda node)
	{
		Symbol currentSymbol = CurrentSymbol;
		CurrentSymbol = node.Symbol;
		VisitAttributes(((LambdaSymbol)node.Symbol).BindMethodAttributes());
		AbstractFlowPass<LocalState, LocalFunctionState>.SavedPending oldPending = SavePending();
		LocalState self = State;
		State = (State.Reachable ? State.Clone() : ReachableBottomState());
		if (!node.WasCompilerGenerated)
		{
			EnterParameters(node.Symbol.Parameters);
		}
		AbstractFlowPass<LocalState, LocalFunctionState>.SavedPending oldPending2 = SavePending();
		VisitAlways(node.Body);
		RestorePending(oldPending2);
		ImmutableArray<AbstractFlowPass<LocalState, LocalFunctionState>.PendingBranch> immutableArray = RemoveReturns();
		RestorePending(oldPending);
		LeaveParameters(node.Symbol.Parameters, node.Syntax, null);
		Join(ref self, ref State);
		foreach (AbstractFlowPass<LocalState, LocalFunctionState>.PendingBranch item in immutableArray)
		{
			State = item.State;
			if (item.Branch.Kind == BoundKind.ReturnStatement)
			{
				LeaveParameters(node.Symbol.Parameters, item.Branch.Syntax, null);
			}
			Join(ref self, ref State);
		}
		State = self;
		CurrentSymbol = currentSymbol;
		return null;
	}

	public override BoundNode VisitThisReference(BoundThisReference node)
	{
		CheckAssigned(base.MethodThisParameter, node.Syntax);
		return null;
	}

	public override BoundNode VisitParameter(BoundParameter node)
	{
		if (!node.WasCompilerGenerated)
		{
			CheckAssigned(node.ParameterSymbol, node.Syntax);
		}
		else
		{
			NotePrimaryConstructorParameterReadIfNeeded(node.ParameterSymbol);
		}
		return null;
	}

	public override BoundNode VisitAssignmentOperator(BoundAssignmentOperator node)
	{
		base.VisitAssignmentOperator(node);
		Assign(node.Left, node.Right, node.IsRef);
		return null;
	}

	public override BoundNode VisitDeconstructionAssignmentOperator(BoundDeconstructionAssignmentOperator node)
	{
		base.VisitDeconstructionAssignmentOperator(node);
		Assign(node.Left, node.Right);
		return null;
	}

	public override BoundNode VisitIncrementOperator(BoundIncrementOperator node)
	{
		base.VisitIncrementOperator(node);
		Assign(node.Operand, node);
		return null;
	}

	public override BoundNode VisitCompoundAssignmentOperator(BoundCompoundAssignmentOperator node)
	{
		VisitCompoundAssignmentTarget(node);
		VisitRvalue(node.Right);
		AfterRightHasBeenVisited(node);
		Assign(node.Left, node);
		return null;
	}

	public override BoundNode VisitFixedLocalCollectionInitializer(BoundFixedLocalCollectionInitializer node)
	{
		BoundExpression boundExpression = node.Expression;
		if (boundExpression.Kind == BoundKind.AddressOfOperator)
		{
			boundExpression = ((BoundAddressOfOperator)boundExpression).Operand;
		}
		VisitAddressOfOperand(boundExpression, shouldReadOperand: false);
		return null;
	}

	public override BoundNode VisitAddressOfOperator(BoundAddressOfOperator node)
	{
		BoundExpression operand = node.Operand;
		bool shouldReadOperand = false;
		Symbol symbol = UseNonFieldSymbolUnsafely(operand);
		if ((object)symbol != null)
		{
			HashSet<PrefixUnaryExpressionSyntax>? unassignedVariableAddressOfSyntaxes = _unassignedVariableAddressOfSyntaxes;
			if (unassignedVariableAddressOfSyntaxes != null && !unassignedVariableAddressOfSyntaxes.Contains(node.Syntax as PrefixUnaryExpressionSyntax))
			{
				shouldReadOperand = true;
			}
			if (!_unsafeAddressTakenVariables.ContainsKey(symbol))
			{
				_unsafeAddressTakenVariables.Add(symbol, node.Syntax.Location);
			}
		}
		VisitAddressOfOperand(node.Operand, shouldReadOperand);
		return null;
	}

	protected override void WriteArgument(BoundExpression arg, RefKind refKind, MethodSymbol method)
	{
		if (refKind == RefKind.Ref)
		{
			CheckAssigned(arg, arg.Syntax);
		}
		Assign(arg, null);
		if (refKind != RefKind.None && ((object)method == null || method.IsExtern))
		{
			TypeSymbol type = arg.Type;
			if ((object)type != null)
			{
				MarkFieldsUsed(type);
			}
		}
	}

	protected void CheckAssigned(BoundExpression expr, SyntaxNode node)
	{
		if (!State.Reachable)
		{
			return;
		}
		MakeSlot(expr);
		switch (expr.Kind)
		{
		case BoundKind.Local:
			CheckAssigned(((BoundLocal)expr).LocalSymbol, node);
			break;
		case BoundKind.Parameter:
			CheckAssigned(((BoundParameter)expr).ParameterSymbol, node);
			break;
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)expr;
			FieldSymbol fieldSymbol = boundFieldAccess.FieldSymbol;
			if (!fieldSymbol.IsFixedSizeBuffer && MayRequireTracking(boundFieldAccess.ReceiverOpt, fieldSymbol))
			{
				CheckAssigned(expr, fieldSymbol, node);
			}
			break;
		}
		case BoundKind.EventAccess:
		{
			BoundEventAccess boundEventAccess = (BoundEventAccess)expr;
			FieldSymbol associatedField = boundEventAccess.EventSymbol.AssociatedField;
			if ((object)associatedField != null && MayRequireTracking(boundEventAccess.ReceiverOpt, associatedField))
			{
				CheckAssigned(boundEventAccess, associatedField, node);
			}
			break;
		}
		case BoundKind.ThisReference:
		case BoundKind.BaseReference:
			CheckAssigned(base.MethodThisParameter, node);
			break;
		case BoundKind.InlineArrayAccess:
			CheckAssigned(((BoundInlineArrayAccess)expr).Expression, node);
			break;
		}
	}

	private void MarkFieldsUsed(TypeSymbol type)
	{
		type = type.OriginalDefinition;
		switch (type.TypeKind)
		{
		case TypeKind.Array:
			MarkFieldsUsed(((ArrayTypeSymbol)type).ElementType);
			break;
		case TypeKind.Class:
		case TypeKind.Struct:
			if (!type.IsFromCompilation(compilation) || !(type.ContainingAssembly is SourceAssemblySymbol sourceAssemblySymbol) || !sourceAssemblySymbol.TypesReferencedInExternalMethods.Add(type))
			{
				break;
			}
			foreach (Symbol item in ((NamedTypeSymbol)type).GetMembersUnordered())
			{
				if (item.Kind == SymbolKind.Field)
				{
					FieldSymbol fieldSymbol = (FieldSymbol)item;
					sourceAssemblySymbol.NoteFieldAccess(fieldSymbol, read: true, write: true);
					MarkFieldsUsed(fieldSymbol.Type);
				}
			}
			break;
		}
	}

	public override BoundNode VisitBaseReference(BoundBaseReference node)
	{
		CheckAssigned(base.MethodThisParameter, node.Syntax);
		return null;
	}

	public override BoundNode VisitCatchBlock(BoundCatchBlock catchBlock)
	{
		DeclareVariables(catchBlock.Locals);
		BoundExpression exceptionSourceOpt = catchBlock.ExceptionSourceOpt;
		if (exceptionSourceOpt != null)
		{
			Assign(exceptionSourceOpt, null, isRef: false, read: false);
		}
		base.VisitCatchBlock(catchBlock);
		foreach (LocalSymbol local in catchBlock.Locals)
		{
			ReportIfUnused(local, local.DeclarationKind != LocalDeclarationKind.CatchVariable);
		}
		return null;
	}

	public override BoundNode VisitFieldAccess(BoundFieldAccess node)
	{
		BoundNode result = base.VisitFieldAccess(node);
		NoteRead(node.FieldSymbol);
		if (node.FieldSymbol.IsFixedSizeBuffer && node.Syntax != null && !SyntaxFacts.IsFixedStatementExpression(node.Syntax))
		{
			Symbol symbol = UseNonFieldSymbolUnsafely(node.ReceiverOpt);
			if ((object)symbol != null)
			{
				CheckCaptured(symbol);
				if (!_unsafeAddressTakenVariables.ContainsKey(symbol))
				{
					_unsafeAddressTakenVariables.Add(symbol, node.Syntax.Location);
					return result;
				}
			}
		}
		else if (MayRequireTracking(node.ReceiverOpt, node.FieldSymbol))
		{
			CheckAssigned(node, node.FieldSymbol, node.Syntax);
		}
		return result;
	}

	public override BoundNode VisitPropertyAccess(BoundPropertyAccess node)
	{
		BoundNode result = base.VisitPropertyAccess(node);
		if (Binder.AccessingAutoPropertyFromConstructor(node, CurrentSymbol))
		{
			SynthesizedBackingFieldSymbol synthesizedBackingFieldSymbol = (node.PropertySymbol as SourcePropertySymbolBase)?.BackingField;
			if (synthesizedBackingFieldSymbol != null && MayRequireTracking(node.ReceiverOpt, synthesizedBackingFieldSymbol) && State.Reachable && !IsAssigned(node, out var unassignedSlot))
			{
				ReportUnassignedIfNotCapturedInLocalFunction(synthesizedBackingFieldSymbol, node.Syntax, unassignedSlot);
			}
		}
		return result;
	}

	public override BoundNode VisitEventAccess(BoundEventAccess node)
	{
		BoundNode result = base.VisitEventAccess(node);
		FieldSymbol associatedField = node.EventSymbol.AssociatedField;
		if ((object)associatedField != null)
		{
			NoteRead(associatedField);
			if (MayRequireTracking(node.ReceiverOpt, associatedField))
			{
				CheckAssigned(node, associatedField, node.Syntax);
			}
		}
		return result;
	}

	public override void VisitForEachIterationVariables(BoundForEachStatement node)
	{
		foreach (LocalSymbol iterationVariable in node.IterationVariables)
		{
			int orCreateSlot = GetOrCreateSlot(iterationVariable);
			if (orCreateSlot > 0)
			{
				SetSlotAssigned(orCreateSlot);
			}
			NoteWrite(iterationVariable, null, read: true, iterationVariable.RefKind != RefKind.None);
		}
	}

	public override BoundNode VisitDynamicObjectInitializerMember(BoundDynamicObjectInitializerMember node)
	{
		return null;
	}

	protected override void VisitAssignmentOfNullCoalescingAssignment(BoundNullCoalescingAssignmentOperator node, BoundPropertyAccess propertyAccessOpt)
	{
		base.VisitAssignmentOfNullCoalescingAssignment(node, propertyAccessOpt);
		Assign(node.LeftOperand, node.RightOperand);
	}

	protected override void AdjustStateForNullCoalescingAssignmentNonNullCase(BoundNullCoalescingAssignmentOperator node)
	{
		Assign(node.LeftOperand, node.LeftOperand);
	}

	protected override void AfterVisitInlineArrayAccess(BoundInlineArrayAccess node)
	{
		if (node.GetItemOrSliceHelper == WellKnownMember.System_Span_T__Slice_Int_Int)
		{
			NoteWrite(node.Expression, null, read: false, isRef: false);
		}
	}

	protected override void AfterVisitConversion(BoundConversion node)
	{
		if (node.Conversion.IsInlineArray && node.Type.OriginalDefinition.Equals(compilation.GetWellKnownType(WellKnownType.System_Span_T), TypeCompareKind.AllIgnoreOptions))
		{
			NoteWrite(node.Operand, null, read: false, isRef: false);
		}
	}

	protected override string Dump(LocalState state)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[assigned ");
		AppendBitNames(state.Assigned, stringBuilder);
		stringBuilder.Append(']');
		return stringBuilder.ToString();
	}

	protected void AppendBitNames(BitVector a, StringBuilder builder)
	{
		bool flag = false;
		foreach (int item in a.TrueBits())
		{
			if (flag)
			{
				builder.Append(", ");
			}
			flag = true;
			AppendBitName(item, builder);
		}
	}

	protected void AppendBitName(int bit, StringBuilder builder)
	{
		LocalDataFlowPass<LocalState, LocalFunctionState>.VariableIdentifier variableIdentifier = variableBySlot[bit];
		if (variableIdentifier.ContainingSlot > 0)
		{
			AppendBitName(variableIdentifier.ContainingSlot, builder);
			builder.Append('.');
		}
		builder.Append((bit == 0) ? "<unreachable>" : (string.IsNullOrEmpty(variableIdentifier.Symbol.Name) ? ("<anon>" + variableIdentifier.Symbol.GetHashCode()) : variableIdentifier.Symbol.Name));
	}

	protected override bool Meet(ref LocalState self, ref LocalState other)
	{
		if (self.Assigned.Capacity != other.Assigned.Capacity)
		{
			Normalize(ref self);
			Normalize(ref other);
		}
		if (!other.Reachable)
		{
			self.Assigned[0] = true;
			return true;
		}
		bool result = false;
		for (int i = 1; i < self.Assigned.Capacity; i++)
		{
			if (other.Assigned[i] && !self.Assigned[i])
			{
				SetSlotAssigned(i, ref self);
				result = true;
			}
		}
		return result;
	}

	protected override bool Join(ref LocalState self, ref LocalState other)
	{
		if (self.Reachable == other.Reachable)
		{
			if (self.Assigned.Capacity != other.Assigned.Capacity)
			{
				Normalize(ref self);
				Normalize(ref other);
			}
			return self.Assigned.IntersectWith(in other.Assigned);
		}
		if (!self.Reachable)
		{
			self.Assigned = other.Assigned.Clone();
			return true;
		}
		return false;
	}

	protected override LocalFunctionState CreateLocalFunctionState(LocalFunctionSymbol symbol)
	{
		return CreateLocalFunctionState();
	}

	private LocalFunctionState CreateLocalFunctionState()
	{
		return new LocalFunctionState(new LocalState(BitVector.AllSet(variableBySlot.Count), normalizeToBottom: true), UnreachableState());
	}

	protected override void VisitLocalFunctionUse(LocalFunctionSymbol localFunc, LocalFunctionState localFunctionState, SyntaxNode syntax, bool isCall)
	{
		_usedLocalFunctions.Add(localFunc);
		BitVector readVars = localFunctionState.ReadVars;
		for (int i = 1; i < readVars.Capacity; i++)
		{
			if (readVars[i])
			{
				Symbol symbol = variableBySlot[i].Symbol;
				CheckIfAssignedDuringLocalFunctionReplay(symbol, syntax, i);
			}
		}
		base.VisitLocalFunctionUse(localFunc, localFunctionState, syntax, isCall);
	}

	private void CheckIfAssignedDuringLocalFunctionReplay(Symbol symbol, SyntaxNode node, int slot)
	{
		if ((object)symbol == null)
		{
			return;
		}
		NoteRead(symbol);
		if (State.Reachable)
		{
			if (slot >= State.Assigned.Capacity)
			{
				Normalize(ref State);
			}
			if (slot > 0 && !State.IsAssigned(slot))
			{
				ReportUnassignedIfNotCapturedInLocalFunction(symbol, node, slot, skipIfUseBeforeDeclaration: false);
			}
		}
	}

	private void RecordReadInLocalFunction(int slot)
	{
		LocalFunctionSymbol nearestLocalFunctionOpt = GetNearestLocalFunctionOpt(CurrentSymbol);
		LocalFunctionState orCreateLocalFuncUsages = GetOrCreateLocalFuncUsages(nearestLocalFunctionOpt);
		TypeSymbol type = variableBySlot[slot].Symbol.GetTypeOrReturnType().Type;
		if (EmptyStructTypeCache.IsTrackableStructType(type))
		{
			foreach (FieldSymbol structInstanceField in _emptyStructTypeCache.GetStructInstanceFields(type))
			{
				int orCreateSlot = GetOrCreateSlot(structInstanceField, slot);
				if (orCreateSlot > 0 && !State.IsAssigned(orCreateSlot))
				{
					RecordReadInLocalFunction(orCreateSlot);
				}
			}
			return;
		}
		orCreateLocalFuncUsages.ReadVars[slot] = true;
	}

	private BitVector GetCapturedBitmask()
	{
		int count = variableBySlot.Count;
		BitVector result = BitVector.AllSet(count);
		for (int i = 1; i < count; i++)
		{
			result[i] = IsCapturedInLocalFunction(i);
		}
		return result;
	}

	private bool IsCapturedInLocalFunction(int slot)
	{
		if (slot <= 0)
		{
			return false;
		}
		Symbol symbol = variableBySlot[RootSlot(slot)].Symbol;
		LocalFunctionSymbol nearestLocalFunctionOpt = GetNearestLocalFunctionOpt(CurrentSymbol);
		if ((object)nearestLocalFunctionOpt != null)
		{
			return Symbol.IsCaptured(symbol, nearestLocalFunctionOpt);
		}
		return false;
	}

	private static LocalFunctionSymbol GetNearestLocalFunctionOpt(Symbol symbol)
	{
		while (symbol != null)
		{
			if (symbol.Kind == SymbolKind.Method && ((MethodSymbol)symbol).MethodKind == MethodKind.LocalFunction)
			{
				return (LocalFunctionSymbol)symbol;
			}
			symbol = symbol.ContainingSymbol;
		}
		return null;
	}

	protected override LocalFunctionState LocalFunctionStart(LocalFunctionState startState)
	{
		LocalFunctionState localFunctionState = CreateLocalFunctionState();
		localFunctionState.ReadVars = startState.ReadVars.Clone();
		startState.ReadVars.Clear();
		return localFunctionState;
	}

	protected override bool LocalFunctionEnd(LocalFunctionState savedState, LocalFunctionState currentState, ref LocalState stateAtReturn)
	{
		if (currentState.CapturedMask.IsNull)
		{
			currentState.CapturedMask = GetCapturedBitmask();
			currentState.InvertedCapturedMask = currentState.CapturedMask.Clone();
			currentState.InvertedCapturedMask.Invert();
		}
		stateAtReturn.Assigned.IntersectWith(in currentState.CapturedMask);
		if (NonMonotonicState.HasValue)
		{
			LocalState value = NonMonotonicState.Value;
			value.Assigned.UnionWith(in currentState.InvertedCapturedMask);
			NonMonotonicState = value;
		}
		BitVector other = currentState.ReadVars;
		other.IntersectWith(in currentState.CapturedMask);
		return savedState.ReadVars.UnionWith(in other);
	}
}
